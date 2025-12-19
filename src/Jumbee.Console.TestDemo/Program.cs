namespace Jumbee.Console.TestDemo;

using System;
using System.Threading;

using ConsoleGUI;
using ConsoleGUI.Controls;
using ConsoleGUI.Space;
using ConsoleGUI.Input;
using Jumbee.Console;

using Spectre.Console;

using static Jumbee.Console.Color;

public class Program
{
    static void Main(string[] args)
    {
        // --- Spectre.Console Controls ---
        // 1. Table
        var table = new Table();
        table.Title("[bold yellow]Jumbee Console[/]");
        table.AddColumn("Library");
        table.AddColumn("Role");
        table.AddColumn("Status");
        table.AddRow("Spectre.Console", "Widgets & Styling", "[green]Integrated[/]");
        table.AddRow("ConsoleGUI", "Layout & Windowing", "[blue]Integrated[/]");
        table.AddRow("Jumbee", "The Bridge", "[bold red]Working![/]");
        table.Border(TableBorder.DoubleEdge);

        // 2. Bar Chart
        var barChart = new Jumbee.Console.BarChart(
            ("Planning", 12, Yellow),
            ("Coding", 54, Green),
            ("Testing", 33, Red)
        );

        barChart.Width = 50;
        barChart.Label = "[green bold]Activity[/]";
        barChart.CenterLabel = true;

        // 3. Tree
        var root = new Tree("Root");
        var foo = root.AddNode("[yellow]Foo[/]");
        var bar = foo.AddNode("[blue]Bar[/]");
        bar.AddNode("Baz");
        bar.AddNode("Qux");
        var quux = root.AddNode("Quux");
        quux.AddNode("Corgi");

        // --- Wrap Spectre.Console Controls for ConsoleGUI ---
        var tableControl = new SpectreControl<Spectre.Console.Table>(table);

        tableControl.Content.Border = TableBorder.Rounded;
        // var chartControl = new SpectreControl<Spectre.Console.BarChart>(barChart); // No longer needed
        var treeControl = new SpectreControl<Spectre.Console.Tree>(root);

        // --- ConsoleGUI Controls ---
        // Spinner
        var spinner = new Jumbee.Console.Spinner
        {
            SpinnerType = Spectre.Console.Spinner.Known.Dots,
            Text = "Waiting for input...",
            Style = Spectre.Console.Style.Parse("green bold")
        };
        spinner.Start();

        // The TextPrompt control
        var prompt = new TextPrompt("[yellow]What is your name?[/]", blinkCursor: true);
        prompt.Committed += (sender, name) =>
        {
            spinner.Text = $"Hello, [blue]{name}[/]!";
            spinner.SpinnerType = Spectre.Console.Spinner.Known.Ascii; // Change spinner style on success
        };

        var p = prompt.WithAsciiBorder();
        var grid = new Jumbee.Console.Grid([15, 15], [40, 40, 50], [
            [spinner.WithFrame(borderStyle: BorderStyle.Rounded, fgColor: Red, title: "Spinna benz"), p, barChart],
            [tableControl, barChart, treeControl]
        ]);

        // Start the user interface
        UI.Start(grid, 130, 40);
        //UI.Start(internalGrid, width:250, height: 60, isTrueColorTerminal: true);
        // Create a separate timer to update the chartControl content periodically
        var random = new Random();
        var chartTimer = new Timer(_ =>
        {
            // The SpectreControl.Content setter will acquire the lock internally
            var newPlanning = (double)random.Next(10, 30);
            var newCoding = (double)random.Next(40, 70);
            var newTesting = (double)random.Next(20, 40);

            // Update existing items using the bulk-update indexer
            barChart["Planning", "Coding", "Testing"] = [newPlanning, newCoding, newTesting];

        }, null, 0, 100);

        // Main loop
        while (true)
        {
            ConsoleManager.ReadInput([p, prompt, new InputListener()]);
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
            if (inputEvent.Key.Key == ConsoleKey.Escape)
            {
                UI.Stop();  
                Environment.Exit(0);
            }
        }
    }
}
