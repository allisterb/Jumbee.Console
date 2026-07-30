namespace Jumbee.Console.Examples;

using System.Collections.Generic;
using System.Threading;

/// <summary>
/// The ProgressBar control: a task row with a description, a smooth sub-cell bar, and optional percentage, time and
/// spinner columns — plus an indeterminate pulse for work whose total isn't known.
/// </summary>
public sealed class ProgressBarExample : CompositeControl, IActivatableExample
{
    public ProgressBarExample()
    {
        // The indeterminate row has no meaningful total, so it shows a pulse and a spinner instead of a percentage.
        scanning.IsIndeterminate = true;
        scanning.ShowSpinner = true;

        // Elapsed vs estimated-remaining time, and a spinner on the busy row.
        downloading.TimeDisplay = ProgressTimeDisplay.Remaining;
        downloading.ShowSpinner = true;
        installing.TimeDisplay = ProgressTimeDisplay.Elapsed;

        // The band-hatch fill carries a foreground (the hatch glyph) and a background (the band behind it); the
        // track is a dim glyph on a darker band. This is the colour half of the image-1 look — glyph mode now
        // honours the style's fill/track backgrounds.
        bandHatch.Style = bandHatch.Style with
        {
            Fill = (Style)new Color(0x7d, 0xf0, 0xff) | Style.Bg(new Color(0x10, 0x50, 0x66)),
            Track = (Style)new Color(0x30, 0x40, 0x48) | Style.Bg(new Color(0x14, 0x20, 0x28)),
        };

        SetContent(new VerticalStackPanel(
            Header("Determinate — description, smooth sub-cell fill, percentage"),
            downloading,

            Header("With an elapsed-time column"),
            installing,

            Header("Recoloured with WithFill(color)"),
            optimizing,

            Header("Indeterminate — a pulse and spinner when the total is unknown"),
            scanning,

            Header("Themed glyphs — Glyphs = Hatched / Segmented / Dashed / Ascii"),
            hatched,
            segmented,
            dashed,
            ascii,

            Header("Per-segment gradient (Style.WithGradient) and a hatch on a coloured band"),
            gradient,
            bandHatch,

            Header("Spinners — SpinnerType = any Spectre.Console.Spinner.Known.*"),
            spinDots,
            spinLine,
            spinStar,
            spinArrow,
            spinBounce,

            status));
    }

    // Drive the determinate bars: each advances at its own rate and wraps, so the demo runs forever. The bars
    // animate their own spinners/pulse and clocks internally (via Start); this feed only moves Value.
    private void Advance()
    {
        progress = (progress + 1) % 101;
        downloading.Value = progress;
        installing.Value = (progress * 0.7) % 101;
        optimizing.Value = (progress * 1.3) % 101;
        hatched.Value = progress;
        segmented.Value = progress;
        dashed.Value = progress;
        ascii.Value = progress;
        gradient.Value = progress;
        bandHatch.Value = progress;
        status.Text = $"▸ {progress}%";
    }

    private static TextLabel Header(string text) =>
        new TextLabel(TextLabelOrientation.Horizontal, text, HeaderColor) { Focusable = false };

    // A busy-style row where only the spinner animates: the bar is a fixed partial fill, no percentage, so the
    // chosen Spinner is the moving element. The spinner ticks at its own interval once the bar is Started.
    private static ProgressBar Spinner(string name, Spectre.Console.Spinner type, double value) =>
        new ProgressBar(name, value) { ShowSpinner = true, ShowPercentage = false, SpinnerType = type };

    #region IExample
    void IActivatableExample.OnActivated()
    {
        foreach (var bar in bars) bar.Start();   // begins each bar's clock and spinner/pulse animation
        Feed(Advance, 100);                       // moves Value on the determinate bars
    }

    void IActivatableExample.OnDeactivated()
    {
        foreach (var bar in bars) bar.Stop();
        foreach (var feed in Feeds) feed?.Cancel();
    }

    IReadOnlyList<CancellationTokenSource> IActivatableExample.FeedTasks => Feeds;

    string IExample.Category => "Controls";
    string IExample.Title => "Progress Bars";
    string IExample.Description =>
        "A composable single-row progress control: description, sub-cell bar, and optional percentage, elapsed/remaining time and spinner columns — plus an indeterminate pulse.";
    IReadOnlyList<string> IExample.SourceFiles => ["ProgressBarExample.cs", "ProgressBar.cs", "ProgressBarStyle.cs"];
    #endregion

    #region Fields
    private int progress;

    // WithPadding(left, right) reserves blank cells at the row edges — here a 4-cell gap on the right so the
    // readouts don't jam against the pane border. (A margin frame does not work for a fixed-height bar in a stack.)
    private readonly ProgressBar downloading = new ProgressBar("Downloading packages").WithPadding(0, 4);
    private readonly ProgressBar installing = new ProgressBar("Installing").WithPadding(0, 4);
    private readonly ProgressBar optimizing = new ProgressBar("Optimizing assets").WithFill(new Color(0xc8, 0x92, 0xf0)).WithPadding(0, 4);
    private readonly ProgressBar scanning = new ProgressBar("Scanning for changes").WithPadding(0, 4);
    private readonly ProgressBar hatched = new ProgressBar("Hatched").WithGlyphs(ProgressBarGlyphs.Hatched).WithPadding(0, 4);
    private readonly ProgressBar segmented = new ProgressBar("Segmented").WithGlyphs(ProgressBarGlyphs.Segmented);
    private readonly ProgressBar dashed = new ProgressBar("Dashed").WithGlyphs(ProgressBarGlyphs.Dashed);
    private readonly ProgressBar ascii = new ProgressBar("Ascii").WithGlyphs(ProgressBarGlyphs.Ascii);
    // A segmented bar whose fill fades from light to deep teal across its width (image-2 look).
    private readonly ProgressBar gradient = new ProgressBar("Gradient")
        .WithGlyphs(ProgressBarGlyphs.Segmented)
        .WithGradient(new Color(0x7a, 0xe6, 0xc8), new Color(0x0a, 0x3c, 0x50));
    // A diagonal hatch drawn over a coloured band (image-1 look): the Fill style carries both a foreground (the
    // hatch glyph colour) and a background (the band behind it).
    private readonly ProgressBar bandHatch = new ProgressBar("Band hatch") { ShowPercentage = false }
        .WithGlyphs(ProgressBarGlyphs.Hatched);
    // A sampler of Spectre.Console.Spinner.Known spinners — Dots is the default; the others just set SpinnerType.
    private readonly ProgressBar spinDots = Spinner("Dots", Spectre.Console.Spinner.Known.Dots, 45);
    private readonly ProgressBar spinLine = Spinner("Line", Spectre.Console.Spinner.Known.Line, 60);
    private readonly ProgressBar spinStar = Spinner("Star", Spectre.Console.Spinner.Known.Star, 72);
    private readonly ProgressBar spinArrow = Spinner("Arrow", Spectre.Console.Spinner.Known.Arrow3, 84);
    private readonly ProgressBar spinBounce = Spinner("Bouncing bar", Spectre.Console.Spinner.Known.BouncingBar, 95);
    private readonly TextLabel status = new TextLabel(TextLabelOrientation.Horizontal, "▸ 0%", StatusColor);

    private ProgressBar[] bars => [downloading, installing, optimizing, scanning, hatched, segmented, dashed, ascii,
        gradient, bandHatch, spinDots, spinLine, spinStar, spinArrow, spinBounce];

    private static readonly Color HeaderColor = new(0x9a, 0xc8, 0xff);
    private static readonly Color StatusColor = new(0x8f, 0xd0, 0x66);
    #endregion
}
