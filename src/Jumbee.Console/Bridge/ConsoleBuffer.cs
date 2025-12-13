namespace Jumbee.Console;

using System;
using ConsoleGUI.Api;

using ConsoleGUI.Data;
using ConsoleGUI.Space;
using ConsoleGuiSize = ConsoleGUI.Space.Size;

/// <summary>
/// A ConsoleGUI.IConsole implementation that writes to a buffer.
/// </summary>
public class ConsoleBuffer : IConsole
{
    #region Properties
    public Cell[,]? Buffer { get; private set; }
    public ConsoleGuiSize Size { get; set; }
    public bool KeyAvailable => false;
    #endregion

    #region Methods
    public void Initialize()
    {
        if (Buffer != null)
        {
            // Fill with empty/transparent cells
            for (int x = 0; x < Size.Width; x++)
            {
                for (int y = 0; y < Size.Height; y++)
                {
                    Buffer[x, y] = new Cell(Character.Empty);
                }
            }
        }
    }

    public void OnRefresh() { }

    public void Write(Position position, in Character character)
    {
        if (Buffer == null) return;
        if (position.X >= 0 && position.X < Size.Width && position.Y >= 0 && position.Y < Size.Height)
        {
            Buffer[position.X, position.Y] = new Cell(character);
        }
    }

    public ConsoleKeyInfo ReadKey() => default;
    
    public void Resize(ConsoleGuiSize size)
    {
        Size = size;
        Buffer = new Cell[size.Width, size.Height];
        Initialize();
    }
    #endregion
}
