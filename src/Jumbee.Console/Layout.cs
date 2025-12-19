namespace Jumbee.Console;

using ConsoleGUI;
using ConsoleGUI.Common;

public abstract class Layout<T> where T:ConsoleGUI.Common.Control, IDrawingContextListener
{
    protected Layout(T control)
    {
        this.control = control;
    }

    public abstract int Rows { get; }

    public abstract int Columns { get; }    

    public abstract IControl this[int row, int column] { get; }  

    public readonly T control;
}
