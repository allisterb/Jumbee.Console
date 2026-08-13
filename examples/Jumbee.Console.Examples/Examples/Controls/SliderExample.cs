namespace Jumbee.Console.Examples;

using System.Collections.Generic;

/// <summary>
/// Sliders wired to a live result — drag the thumb, click the track, or use the arrow keys; the swatch and the
/// readouts follow. The bottom slider snaps to whole units instead of moving continuously.
/// </summary>
public sealed class SliderExample : CompositeControl, IExample
{
    public SliderExample()
    {
        foreach (var channel in channels)
        {
            channel.ValueChanged += (_, _) => Mix();
        }

        steps.ValueChanged += (_, value) => stepStatus.Text = $"▸ {value:F0} of 12";

        SetContent(new VerticalStackPanel(
            Header("Colour mixer — three Sliders over one swatch. LabelWidth lines the tracks up"),
            channels[0], channels[1], channels[2],
            Framed(swatch, "Result", Blue),
            hexStatus,

            Header("Snap to step — SnapToStep quantises every path, including a drag"),
            Framed(steps, "Servings", Orange),
            stepStatus));
    }

    // A form of several fields, so Tab moves between the sliders rather than being handed to the focused one.
    protected override bool TabNavigatesChildren => true;

    private void Mix()
    {
        var color = new Color((byte)channels[0].Value, (byte)channels[1].Value, (byte)channels[2].Value);
        swatch.Fill = color;
        hexStatus.Text = $"▸ #{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    private static T Framed<T>(T control, string title, Color color) where T : Control =>
        control.WithFrame(borderStyle: BorderStyle.Rounded, borderFgColor: color).WithTitle(title, InlineTitle);

    private static TextLabel Header(string text) =>
        new TextLabel(TextLabelOrientation.Horizontal, text, HeaderColor) { Focusable = false };

    #region IExample
    string IExample.Category => "Controls";
    string IExample.Title => "Sliders";
    string IExample.Description =>
        "Draggable value controls: a colour mixer whose swatch follows three channel sliders, and a slider that snaps to whole units.";
    IReadOnlyList<string> IExample.SourceFiles => ["SliderExample.cs", "Slider.cs"];
    #endregion

    #region Fields
    // 0..255 per channel, formatted as whole numbers. LabelWidth is what aligns the three tracks; without it each
    // track would start at a different column because "Green" is a letter longer than "Red".
    private readonly Slider[] channels =
    [
        new Slider(0, 255, 0xB4, "Red") { LabelWidth = 7, ValueFormat = "F0" }.WithFill(new Color(0xd0, 0x50, 0x50)),
        new Slider(0, 255, 0x5A, "Green") { LabelWidth = 7, ValueFormat = "F0" }.WithFill(new Color(0x60, 0xc0, 0x60)),
        new Slider(0, 255, 0x28, "Blue") { LabelWidth = 7, ValueFormat = "F0" }.WithFill(new Color(0x60, 0x90, 0xe0)),
    ];

    private readonly Slider steps = new Slider(0, 12, 4, "Servings")
    {
        LabelWidth = 10,
        Step = 1,
        SnapToStep = true,
        ValueFormat = "F0",
    };

    private readonly Swatch swatch = new Swatch(new Color(0xB4, 0x5A, 0x28)) { Height = 4 };

    private readonly TextLabel hexStatus = new TextLabel(TextLabelOrientation.Horizontal, "▸ #B45A28", StatusColor);
    private readonly TextLabel stepStatus = new TextLabel(TextLabelOrientation.Horizontal, "▸ 4 of 12", StatusColor);

    private static readonly TitleStyle InlineTitle = new(TitlePos.TopLeft, TitleBorderStyle.Inline);
    private static readonly Color HeaderColor = new(0x9a, 0xc8, 0xff);
    private static readonly Color StatusColor = new(0x8f, 0xd0, 0x66);
    private static readonly Color Blue = new(0x5c, 0x9c, 0xff);
    private static readonly Color Orange = new(0xe0, 0xa0, 0x50);
    #endregion

    #region Child types
    /// <summary>A block of flat colour — the mixer's result, and about as small as a custom control gets.</summary>
    private sealed class Swatch : Control
    {
        public Swatch(Color fill)
        {
            this.fill = fill;
            Focusable = false;
        }

        public Color Fill { get => fill; set => SetAtomicProperty(ref fill, value); }

        protected override void Render()
        {
            for (var y = 0; y < ActualHeight; y++)
            {
                for (var x = 0; x < ActualWidth; x++)
                {
                    consoleBuffer.Write(
                        new ConsoleGUI.Space.Position(x, y),
                        new ConsoleGUI.Data.Character(' ', null, fill));
                }
            }
        }

        private Color fill;
    }
    #endregion
}
