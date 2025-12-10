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

class Program : IInputListener
{
    static void Main(string[] args)
    {
        // Setup ConsoleGUI
        ConsoleManager.Setup();
        ConsoleManager.Resize(new ConsoleGuiSize(80, 25));

        // Create a Spectre Table
        var table = new Table();
        table.Title("[bold yellow]Jumbie Console[/]");
        table.AddColumn("Library");
        table.AddColumn("Role");
        table.AddColumn("Status");
        table.AddRow("Spectre.Console", "Widgets & Styling", "[green]Integrated[/]");
        table.AddRow("ConsoleGUI", "Layout & Windowing", "[blue]Integrated[/]");
        table.AddRow("Jumbie", "The Bridge", "[bold red]Working![/]");
        table.Border(TableBorder.Double);
        table.Expand();

        // Wrap it in our control
        var spectreControl = new SpectreWidgetControl(table);

        // Create layout
        var layout = new DockPanel
        {
            Placement = DockPanel.DockedControlPlacement.Top,
            DockedControl = new Background
            {
                Color = new ConsoleGUI.Data.Color(0, 0, 100),
                Content = new Box
                {
                    HorizontalContentPlacement = Box.HorizontalPlacement.Center,
                    VerticalContentPlacement = Box.VerticalPlacement.Center,
                    Content = new TextBlock { Text = "Jumbie Console Demo" }
                }
            },
            FillingControl = new DockPanel
            {
                Placement = DockPanel.DockedControlPlacement.Bottom,
                DockedControl = new TextBlock { Text = "Press any key to exit" },
                FillingControl = new Margin
                {
                    Offset = new Offset(2, 1, 2, 1),
                    Content = spectreControl
                }
            }
        };

        ConsoleManager.Content = layout;

                        // Main loop

                        while (true)

                        {

                            // Just dummy input reading to keep window responsive if controls used it

                            ConsoleManager.ReadInput([new InputListener()]);

                            Thread.Sleep(50);

                        }

                    }
    public void OnInput(InputEvent inputEvent) => Environment.Exit(0);

}

public class InputListener : IInputListener
{
    public void OnInput(InputEvent inputEvent)
    {
        Environment.Exit(0);    
    }
}



    