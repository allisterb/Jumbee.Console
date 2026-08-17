namespace Jumbee.Console.Tests;

using ConsoleGUI.Space;

using Jumbee.Console;
using Jumbee.Console.Snapshot;

using Spectre.Console.Rendering;

using Xunit;

/// <summary>
/// <see cref="Select"/> options that are <see cref="IRenderable"/> rather than text.
/// </summary>
/// <remarks>
/// The case that motivated them is a colour swatch beside a name, and it is exactly what a text option cannot do:
/// the closed control renders its value as a single <c>Segment</c>, so one style covers the whole row, and it has
/// never parsed markup. Both halves of the control — the closed row and the drop-down — have to handle renderables,
/// and they are separate render paths.
/// </remarks>
public class SelectRenderableTests
{
    private static readonly Color Mint = new(120, 200, 160);
    private static readonly Color Rose = new(225, 130, 110);

    private static IRenderable Swatch(string name, Color color) =>
        new Spectre.Console.Markup($"[#{color.R:x2}{color.G:x2}{color.B:x2}]███[/] {name}");

    // These headless tests never start the UI loop, so the ambient overlay the drop-down shows into is set directly.
    private static Overlay HostOverlay()
    {
        var overlay = new Overlay(new Grid([1], [10], [[new Button("host")]]));
        UI.Overlay = overlay;
        return overlay;
    }

    [Fact]
    public void RenderableOption_DrawsInTheClosedControl()
    {
        var select = new Select(Swatch("Mint", Mint), Swatch("Rose", Rose)) { SelectedIndex = 0, Height = 1 };

        var buffer = ConsoleSnapshot.Render(select, 24, 1);
        var text = ConsoleSnapshot.ToText(buffer);

        Assert.Contains("Mint", text);
        Assert.Contains("▼", text);

        // The block keeps its own colour while the label falls back to the control's foreground — the two-colour
        // row a single-Segment render could not produce, and the half that markup in a string never reached.
        Assert.Equal(Mint, ConsoleSnapshot.ForegroundAt(buffer, 1, 0));
        Assert.Equal(select.Foreground, ConsoleSnapshot.ForegroundAt(buffer, 5, 0));
    }

    [Fact]
    public void RenderableOption_HasNoTextValueButCarriesATag()
    {
        var select = new Select(new SelectOption(Swatch("Mint", Mint)) { Tag = Mint }) { SelectedIndex = 0 };

        Assert.Null(select.SelectedValue);              // there is no sensible text to report
        Assert.Equal(Mint, select.SelectedItem?.Tag);   // ...so the Tag is how a selection maps back
    }

    [Fact]
    public void RenderableOption_StillRaisesSelectionChanged()
    {
        // Guarding the event on the text — as it was — would mean a Select of renderables never announced anything.
        var select = new Select(Swatch("Mint", Mint), Swatch("Rose", Rose));
        var fired = 0;
        select.SelectionChanged += (_, _) => fired++;

        select.SelectedIndex = 1;

        Assert.Equal(1, fired);
        Assert.Equal(1, select.SelectedIndex);
    }

    [Fact]
    public void RenderableOptions_CommitFromDropdownMapsBackByPosition()
    {
        var overlay = HostOverlay();
        var select = new Select(
            new SelectOption(Swatch("Mint", Mint)),
            new SelectOption(Swatch("Rose", Rose)) { Tag = Rose });

        select.Open();
        var dropdown = (ListBox)overlay.Top!;
        Assert.Contains("Rose", ConsoleSnapshot.ToText(dropdown, 20, 8));

        dropdown.SelectedIndex = 1;
        UI.SendInput(dropdown, ConsoleKey.Enter);

        // By index, not by text: a renderable row has no text to look up, and two text rows can read the same.
        Assert.Equal(1, select.SelectedIndex);
        Assert.Equal(Rose, select.SelectedItem?.Tag);
        Assert.False(overlay.IsShowing);
    }

    [Fact]
    public void TextOption_IsStillNotTreatedAsMarkup()
    {
        // The closed control has never parsed markup and adding renderables must not change that, or every existing
        // option containing a bracket would silently change meaning.
        var select = new Select("[red]literal[/]") { SelectedIndex = 0, Height = 1 };
        Assert.Contains("[red]literal[/]", ConsoleSnapshot.ToText(select, 26, 1));
    }

    [Fact]
    public void MixedOptions_MeasureToTheWidestRow()
    {
        var narrow = new Select(new SelectOption("ab")).Width;
        var wide = new Select(new SelectOption("ab"), new SelectOption(Swatch("Mint", Mint))).Width;

        Assert.True(wide > narrow, $"a renderable row should widen the control: {wide} vs {narrow}");
    }

    [Fact]
    public void NewSelect_StartsUnselectedAndShowsItsPlaceholder()
    {
        // Construction is not SetOptions: routing the constructors through it auto-selected the first option and
        // lost the placeholder, which is a behaviour every existing Select relies on.
        var select = new Select("Red", "Green", "Blue") { Height = 1 };

        Assert.Equal(-1, select.SelectedIndex);
        Assert.Contains(select.Placeholder, ConsoleSnapshot.ToText(select, 24, 1));
    }

    [Fact]
    public void SetOptions_WithRenderables_KeepsThePositionWhenThereIsNoTextToMatchOn()
    {
        var select = new Select(Swatch("Mint", Mint), Swatch("Rose", Rose)) { SelectedIndex = 1 };

        select.SetOptions(Swatch("Mint", Mint), Swatch("Rose", Rose), Swatch("Sky", new Color(130, 175, 235)));

        Assert.Equal(1, select.SelectedIndex);
    }

    [Fact]
    public void SetOptions_ClampsWhenTheListShrinksUnderTheSelection()
    {
        var select = new Select(Swatch("Mint", Mint), Swatch("Rose", Rose)) { SelectedIndex = 1 };

        select.SetOptions(Swatch("Mint", Mint));

        Assert.Equal(0, select.SelectedIndex);
    }
}
