namespace Jumbee.Console;

using ConsoleGUIColor = ConsoleGUI.Data.Color;
using SpectreColor = Spectre.Console.Color;

public struct Color 
{
    #region Properties
    public byte R { get; }
    public byte G { get; }
    public byte B { get; }
    #endregion

    #region Constructors
    public Color(byte r, byte g, byte b)
    {
        R = r;
        G = g;
        B = b;
    }
    #endregion

    #region Methods
    public SpectreColor ToSpectreColor()
    {
        return new SpectreColor(R, G, B);
    }

    public static Color FromSpectreColor(SpectreColor color)
    {
        return new Color(color.R, color.G, color.B);
    }

    public static ConsoleGUIColor? ToConsoleGUIColor(SpectreColor color)
    {
        if (color == SpectreColor.Default)
        {
            return null;
        }

        return new ConsoleGUIColor(color.R, color.G, color.B);
    }

    public static Color FromConsoleGUIColor(ConsoleGUIColor color)
    {
        return new Color(color.Red, color.Green, color.Blue);
    }
    #endregion

    #region Operators
    public static implicit operator SpectreColor(Color color) => color.ToSpectreColor();

    public static implicit operator Color(SpectreColor color) => FromSpectreColor(color);
    #endregion
}
