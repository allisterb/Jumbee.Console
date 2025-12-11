using Spectre.Console;
using ConsoleGUIColor = ConsoleGUI.Data.Color;

namespace Jumbee.Console;

internal static class StyleAdapter
{
    public static ConsoleGUIColor? ToConsoleColor(Color color)
    {
        if (color == Color.Default)
        {
            return null;
        }

        return new ConsoleGUIColor(color.R, color.G, color.B);
    }
}
