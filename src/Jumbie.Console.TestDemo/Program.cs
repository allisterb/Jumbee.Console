using System;
using System.Threading;
using ConsoleGUI;
using ConsoleGUI.Controls;
using ConsoleGUI.Space;
using ConsoleGUI.Input;
using Jumbie.Console;
using Spectre.Console;
using Spectre.Console.Rendering;
using ConsoleGuiSize = ConsoleGUI.Space.Size;
using ConsoleGuiColor = ConsoleGUI.Data.Color;
using LayoutGrid = ConsoleGUI.Controls.Grid;
using Jumbie.Console.Prompts;
using Jumbie.Console.Controls;

class Program
{
    static void Main(string[] args)
    {
        // Setup ConsoleGUI
        ConsoleManager.Setup();
        ConsoleManager.Resize(new ConsoleGuiSize(120, 40));

        // --- Spectre.Console Controls ---
        // 1. Table
        var table = new Table();
        table.Title("[bold yellow]Jumbie Console[/]");
        table.AddColumn("Library");
        table.AddColumn("Role");
        table.AddColumn("Status");
        table.AddRow("Spectre.Console", "Widgets & Styling", "[green]Integrated[/]");
        table.AddRow("ConsoleGUI", "Layout & Windowing", "[blue]Integrated[/]");
        table.AddRow("Jumbie", "The Bridge", "[bold red]Working![/]");
        table.Border(TableBorder.Rounded);

        // 2. Bar Chart
        var barChart = new BarChart()
            .Width(50)
            .Label("[green bold]Activity[/]")
            .CenterLabel()
            .AddItem("Planning", 12, Color.Yellow)
            .AddItem("Coding", 54, Color.Green)
            .AddItem("Testing", 33, Color.Red);

        // 3. Tree
        var root = new Tree("Root");
        var foo = root.AddNode("[yellow]Foo[/]");
        var bar = foo.AddNode("[blue]Bar[/]");
        bar.AddNode("Baz");
        bar.AddNode("Qux");
        var quux = root.AddNode("Quux");
        quux.AddNode("Corgi");
        
        // --- Wrap Spectre.Console Controls for ConsoleGUI ---
        var tableControl = new SpectreWidgetControl(table);
        var chartControl = new SpectreWidgetControl(barChart);
        var treeControl = new SpectreWidgetControl(root);

        // --- ConsoleGUI Controls ---
        // Spinner
        var spinner = new ConsoleGuiSpinner
        {
            Spinner = Spinner.Known.Dots,
            Text = "Waiting for input...",
            Style = Spectre.Console.Style.Parse("green bold")
        };
        spinner.Start();

        // The TextPrompt control
        var prompt = new ConsoleGuiTextPrompt<string>("[yellow]What is your name?[/]", enableCursorBlink: true);
        prompt.Committed += (sender, name) => 
        {
            spinner.Text = $"Hello, [blue]{name}[/]!";
            //spinner.Spinner = Spinner.Known.Ascii; // Change spinner style on success
        };
        
        // --- ConsoleGUI Layout ---
        // Use a Grid for layout
        var grid = new LayoutGrid
        {
            Columns = new[]
            {
                new LayoutGrid.ColumnDefinition(60),
                new LayoutGrid.ColumnDefinition(50)
            },
            Rows = new[]
            {
                new LayoutGrid.RowDefinition(15),
                new LayoutGrid.RowDefinition(20)
            }
        };

        // Add controls to grid
        grid.AddChild(0, 1, new Margin { Offset = new Offset(1, 1, 1, 1), Content = prompt }); // Top Left
        grid.AddChild(0, 0, new Margin { Offset = new Offset(1, 1, 1, 1), Content = spinner });  // Top Right
        //grid.AddChild(0, 1, new Margin { Offset = new Offset(1, 1, 1, 1), Content = chartControl }); // Bottom Left
        /*
        grid.AddChild(1, 1, new Box // Bottom Right
        {
            HorizontalContentPlacement = Box.HorizontalPlacement.Center,
            VerticalContentPlacement = Box.VerticalPlacement.Center,
            Content = spinner
        });
        */
        // Main Layout with DockPanel
        /*
        var mainLayout = new DockPanel
        {
            Placement = DockPanel.DockedControlPlacement.Top,
            DockedControl = new Background
            {
                Color = new ConsoleGuiColor(0, 0, 100),
                Content = new Box
                {
                    HorizontalContentPlacement = Box.HorizontalPlacement.Center,
                    VerticalContentPlacement = Box.VerticalPlacement.Center,
                    Content = new TextBlock { Text = "Jumbie Console Advanced Demo - Grid Layout" }
                }
            },
            FillingControl = new DockPanel
            {
                Placement = DockPanel.DockedControlPlacement.Bottom,
                DockedControl = new Margin // Prompt at the bottom
                {
                    Offset = new Offset(2, 0, 2, 1),
                    Content = prompt
                },
                FillingControl = new Background // Grid fills the rest
                {
                    Color = new ConsoleGuiColor(10, 10, 10),
                    Content = grid 
                }
            }
        };
        */
        ConsoleManager.Content = grid;

        // Start the global animation timer
        ConsoleGuiTimer.Start();

        // Main loop
        while (true)
        {
            ConsoleManager.ReadInput([prompt, new InputListener()]);
            Thread.Sleep(50);
        }
    }
}

public class InputListener : IInputListener
{
    public void OnInput(InputEvent inputEvent)
    {
        if (!inputEvent.Handled)
        {
            Environment.Exit(0);
        }
    }
}
