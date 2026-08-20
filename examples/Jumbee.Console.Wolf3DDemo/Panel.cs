#nullable enable

namespace Jumbee.Console.Wolf3DDemo;

/// <summary>
/// The pieces the sidebar's tab pages are built from — a titled section, and the rows that go in one.
/// </summary>
/// <remarks>
/// Deliberately the same shapes and colours as the 3D sandbox's sidebar, so the two demos read as one family:
/// rounded sections with an inline title, a muted caption column, and a blank row between interactive controls
/// when there is room for it.
/// </remarks>
internal static class Panel
{
    #region Properties
    /// <summary>Columns the sidebar occupies when docked, matching the sandbox's.</summary>
    public const int Columns = 32;

    /// <summary>Exact caption width shared by every slider on every page.</summary>
    /// <remarks>
    /// <c>Slider.LabelWidth</c> is exact rather than a minimum, so sliders that disagree about it get tracks that
    /// do not line up. Routing them all through <see cref="Knob"/> is what stops that happening the next time a
    /// knob is added.
    /// </remarks>
    public const int LabelWidth = 12;

    public static readonly Color Heading = new(200, 205, 215);
    public static readonly Color Text = new(170, 176, 190);
    public static readonly Color Muted = new(130, 136, 150);
    public static readonly Color Border = new(70, 78, 96);
    #endregion

    #region Methods
    /// <summary>A single non-focusable row of text — a caption, a readout, or a blank spacer.</summary>
    public static TextLabel Line(string text, Color color) =>
        new(TextLabelOrientation.Horizontal, text, color) { Focusable = false, Height = 1 };

    /// <summary>A blank row.</summary>
    public static TextLabel Spacer() => Line("", Muted);

    /// <summary>A labelled slider sized to the shared caption column.</summary>
    public static Slider Knob(string label, double minimum, double maximum, double value, double step,
                              string format = "0.##") =>
        new Slider(minimum, maximum, value)
        {
            Step = step,
            SnapToStep = true,
            ShowValue = true,
            ValueFormat = format,
            // Explicitly black behind the caption. The default label style sets only a foreground, so the caption
            // cells inherit whatever is behind them -- which is the section's fill, not the terminal's background,
            // and it reads as a lighter band running through the panel.
            Style = SliderStyle.Default with
            {
                Label = new Style(Text, Color.Black),
                Value = new Style(Text, Color.Black),
            },
        }.WithLabel(label, LabelWidth);

    /// <summary>The items stacked, with a blank row between each when <paramref name="spaced"/>.</summary>
    /// <remarks>One fewer spacer than items, so a section never carries a trailing blank row against its border.</remarks>
    public static ILayout Stack(bool spaced, params IFocusable[] items)
    {
        if (!spaced) return new VerticalStackPanel(items);

        var rows = new List<IFocusable>(items.Length * 2);
        for (var i = 0; i < items.Length; i++)
        {
            if (i > 0) rows.Add(Spacer());
            rows.Add(items[i]);
        }

        return new VerticalStackPanel([.. rows]);
    }

    /// <summary>A single button occupying the left half of a row, as the sandbox's sections do.</summary>
    public static ILayout Row(Button only) =>
        new Grid([1], [(Columns / 2) - 2, (Columns / 2) - 2], [[only, Spacer()]]);
    #endregion

    #region Child types
    /// <summary>One titled, bordered group of controls in a page.</summary>
    internal sealed class Section : CompositeControl
    {
        public Section(string title, ILayout content, int rows)
        {
            Height = rows;
            SetContent(content);
            this.WithFrame(borderStyle: BorderStyle.Rounded, borderFgColor: Border)
                .WithTitle(title, new TitleStyle(TitlePos.TopLeft, TitleBorderStyle.Inline));
        }

        /// <summary>Rows this section occupies in a page: its content plus the frame's two border rows. An inline
        /// title lives IN the top border row, so it costs nothing extra.</summary>
        public int OuterRows => Height + 2;

        protected override bool TabNavigatesChildren => true;
    }
    #endregion
}
