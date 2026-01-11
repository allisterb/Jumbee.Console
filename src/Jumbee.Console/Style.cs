namespace Jumbee.Console;

using SCStyle = Spectre.Console.Style; 
using SCDecoration = Spectre.Console.Decoration;    

public readonly struct Style
{
    public SCStyle SpectreConsoleStyle { get; }

    public Style(SCStyle spectreConsoleStyle)
    {
        SpectreConsoleStyle = spectreConsoleStyle;
    }       

    public Style(string style) : this(SCStyle.Parse(style)) {}

    public static string EscapeMarkup(string text) => Spectre.Console.Markup.Escape(text);  

    public static implicit operator SCStyle(Style style) => style.SpectreConsoleStyle;

    public static implicit operator Style(SCStyle spectreConsoleStyle) => new Style(spectreConsoleStyle);   
    
    public static implicit operator Style(string style) => new Style(style);

    public static implicit operator string(Style style) => style.SpectreConsoleStyle.ToMarkup();

    public static implicit operator Style(Color color) => new Style(new SCStyle(color) );

    public static implicit operator Color(Style style) => style.SpectreConsoleStyle.Foreground;

    public static Style operator + (Style a, Style b) => new Style(a.SpectreConsoleStyle.Combine(b.SpectreConsoleStyle));

    public readonly static Style Plain = SCStyle.Plain;

    public readonly static Style Bold = new SCStyle(decoration: SCDecoration.Bold);

    public readonly static Style Dim = new SCStyle(decoration: SCDecoration.Dim);

    public readonly static Style Italic = new SCStyle(decoration: SCDecoration.Italic);

    public readonly static Style Underline = new SCStyle(decoration: SCDecoration.Underline);

    public readonly static Style Invert = new SCStyle(decoration: SCDecoration.Invert);

    public readonly static Style Conceal = new SCStyle(decoration: SCDecoration.Conceal);

    public readonly static Style SlowBlink = new SCStyle(decoration: SCDecoration.SlowBlink);

    public readonly static Style RapidBlink = new SCStyle(decoration: SCDecoration.RapidBlink);

    public readonly static Style Strikethrough = new SCStyle(decoration: SCDecoration.Strikethrough);
}
