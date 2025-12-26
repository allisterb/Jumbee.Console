namespace Jumbee.Console.TestDemo;

using System;
using System.Linq;
using ConsoleGUI;
using ConsoleGUI.Common;
using ConsoleGUI.Controls;
using ConsoleGUI.Space;
using ConsoleGUI.Data;
using Jumbee.Console;

using ConsoleSize = ConsoleGUI.Space.Size;

public class Program2
{
    static void Main2(string[] args)
    {
        // 1. Create a fixed size control (Height 20)
        var control = new FixedSizeControl(20);

        // 2. Create Frame with height 10, width 20
        var frame = new ControlFrame(control)
        {
            ScrollBarForeground = new Character('#'), // Thumb
            ScrollBarBackground = new Character('|'), // Track
            ScrollBarUpArrow = new Character('^'),
            ScrollBarDownArrow = new Character('v'),
            BorderPlacement = BorderPlacement.None,
            Margin = new Offset(0, 0, 0, 0)
        };

        // 3. Set Context to force size (20x10)
        var context = new MockContext(new ConsoleSize(20, 10), new ConsoleSize(20, 10));
        ((IControl)frame).Context = context;

        // Scroll to middle (Top = 5, MaxScroll = 10)
        frame.Top = 5;

        // 4. Query the right-most column (x=19)
        Console.WriteLine("Scrollbar Column (x=19):");
        for (int y = 0; y < 10; y++)
        {
            var cell = frame[new Position(19, y)];
            var c = cell.Character.Content;
            var display = (c == '\0' || c == ' ') ? (c == '\0' ? "\\0" : "SPACE") : $"'{c}'";
            Console.WriteLine($"y={y}: {display}");
        }
    }
}

public class FixedSizeControl : Jumbee.Console.Control
{
    private int _desiredHeight;
    public FixedSizeControl(int height)
    {
        _desiredHeight = height;
    }
    protected override void Render() { }
    
}

public class MockContext : IDrawingContext
{
    public ConsoleSize MinSize { get; }
    public ConsoleSize MaxSize { get; }

    public MockContext(ConsoleSize min, ConsoleSize max)
    {
        MinSize = min;
        MaxSize = max;
    }

    public event SizeLimitsChangedHandler? SizeLimitsChanged;

    public void Redraw(IControl control) { }
    public void Update(IControl control, in Rect rect) { }
}
