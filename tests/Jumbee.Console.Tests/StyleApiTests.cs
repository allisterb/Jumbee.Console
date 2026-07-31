namespace Jumbee.Console.Tests;

using Jumbee.Console;

using Xunit;

/// <summary>Tests for <see cref="Style"/>'s public construction surface.</summary>
public class StyleApiTests
{
    // Before this ctor existed the only ways to get a background were the markup-string ctor ("black on white") or
    // composing Style.Bg(..) with |, so consumers reached for Spectre's Style directly — or mistook the get-only
    // ForegroundColor/BackgroundColor properties for settable ones and silently got no background at all.
    [Fact]
    public void TwoColourConstructor_SetsBothHalves()
    {
        var style = new Style(Color.Black, Color.White);

        Assert.Equal(Color.Black, style.ForegroundColor);
        Assert.Equal(Color.White, style.BackgroundColor);
    }

    [Fact]
    public void TwoColourConstructor_MatchesTheMarkupAndCompositionForms()
    {
        var ctor = new Style(Color.Black, Color.White);

        Assert.Equal(ctor, new Style("black on white"));
        Assert.Equal(ctor, (Style)Color.Black | Style.Bg(Color.White));
    }

    [Fact]
    public void TwoColourConstructor_ComposesWithADecoration()
    {
        var bold = new Style(Color.Black, Color.White) | Style.Bold;

        Assert.Equal(Color.Black, bold.ForegroundColor);
        Assert.Equal(Color.White, bold.BackgroundColor);
        Assert.Contains("bold", bold.ToMarkup());
    }

    [Fact]
    public void SingleColour_ConvertsImplicitlyToAForegroundOnlyStyle()
    {
        Style style = Color.Red;

        Assert.Equal(Color.Red, style.ForegroundColor);
        Assert.Null(style.BackgroundColor);   // left at the terminal default
    }
}
