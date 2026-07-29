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

        SetContent(new VerticalStackPanel(
            Header("Determinate — description, smooth sub-cell fill, percentage"),
            downloading,

            Header("With an elapsed-time column"),
            installing,

            Header("Recoloured with WithFill(color)"),
            optimizing,

            Header("Indeterminate — a pulse and spinner when the total is unknown"),
            scanning,

            Header("Themed glyphs — Glyphs = Hatched / Segmented / Ascii"),
            hatched,
            segmented,
            ascii,

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
        ascii.Value = progress;
        status.Text = $"▸ {progress}%";
    }

    private static TextLabel Header(string text) =>
        new TextLabel(TextLabelOrientation.Horizontal, text, HeaderColor) { Focusable = false };

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

    private readonly ProgressBar downloading = new ProgressBar("Downloading packages");
    private readonly ProgressBar installing = new ProgressBar("Installing");
    private readonly ProgressBar optimizing = new ProgressBar("Optimizing assets").WithFill(new Color(0xc8, 0x92, 0xf0));
    private readonly ProgressBar scanning = new ProgressBar("Scanning for changes");
    private readonly ProgressBar hatched = new ProgressBar("Hatched").WithGlyphs(ProgressBarGlyphs.Hatched);
    private readonly ProgressBar segmented = new ProgressBar("Segmented").WithGlyphs(ProgressBarGlyphs.Segmented);
    private readonly ProgressBar ascii = new ProgressBar("Ascii").WithGlyphs(ProgressBarGlyphs.Ascii);
    private readonly TextLabel status = new TextLabel(TextLabelOrientation.Horizontal, "▸ 0%", StatusColor);

    private ProgressBar[] bars => [downloading, installing, optimizing, scanning, hatched, segmented, ascii];

    private static readonly Color HeaderColor = new(0x9a, 0xc8, 0xff);
    private static readonly Color StatusColor = new(0x8f, 0xd0, 0x66);
    #endregion
}
