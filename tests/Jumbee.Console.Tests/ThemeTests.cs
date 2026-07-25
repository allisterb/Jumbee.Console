namespace Jumbee.Console.Tests;

using System;

using ConsoleGUI.Input;
using ConsoleGUI.Space;

using Jumbee.Console;
using Jumbee.Console.Snapshot;

using Xunit;

public class ThemeTests
{
    #region Custom themes
    /// <summary>An ASCII glyph theme; the switch glyphs are a different cell width (5) than the default (4).</summary>
    private sealed class AsciiGlyphTheme : IGlyphTheme
    {
        public string CheckboxChecked => "[*]";
        public string RadioSelected => "(o)";
        public string SwitchOn => "[ON ]";
        public string SwitchOff => "[OFF]";
    }

    /// <summary>A style theme that overrides a single token; everything else falls back to the defaults.</summary>
    private sealed class AccentStyleTheme : IStyleTheme
    {
        public Style TextAccent => Style.Magenta1;
    }

    /// <summary>A style theme driving every plot token plus the label token off four colours, so a switch between
    /// two of these moves every themed surface at once (what a scope colour scheme does).</summary>
    private sealed class PlotStyleTheme(Color surface, Color axis, Color tick, Color tickLabel) : IStyleTheme
    {
        public Style PlotSurface => Style.Bg(surface);
        public Style PlotAxis => axis;
        public Style PlotGrid => axis;
        public Style PlotTick => tick;
        public Style PlotTickLabel => tickLabel;
        public PlotPalette PlotSeries => new([axis, tick]);
        public Style LabelText => tickLabel | Style.Bg(surface);
    }

    /// <summary>A glyph theme that overrides only the scrollbar glyphs.</summary>
    private sealed class BlockScrollGlyphTheme : IGlyphTheme
    {
        public ScrollBarGlyphs ScrollBar => ScrollBarGlyphs.Block;
    }

    /// <summary>A style theme that overrides only the scrollbar colours (all parts red).</summary>
    private sealed class RedScrollStyleTheme : IStyleTheme
    {
        public ScrollBarStyle ScrollBar => ScrollBarStyle.Uniform(Style.Red1);
    }

    /// <summary>A style theme that overrides the frame border/title defaults (all non-glyph appearance).</summary>
    private sealed class FramedStyleTheme : IStyleTheme
    {
        public BorderStyle FrameBorder => BorderStyle.Rounded;
        public TitleStyle TitleStyle => new(TitlePos.BottomCenter, TitleBorderStyle.Inline, TitleColorStyle.Reverse);
    }

    private static void WithGlyphTheme(IGlyphTheme theme, Action body)
    {
        var prev = UI.GlyphTheme;
        UI.GlyphTheme = theme;
        try { body(); } finally { UI.GlyphTheme = prev; }
    }

    private static void WithStyleTheme(IStyleTheme theme, Action body)
    {
        var prev = UI.StyleTheme;
        UI.StyleTheme = theme;
        try { body(); } finally { UI.StyleTheme = prev; }
    }
    #endregion

    #region Defaults
    [Fact]
    public void UI_ThemesDefaultToBuiltIns()
    {
        Assert.IsType<DefaultStyleTheme>(UI.StyleTheme);
        Assert.IsType<DefaultGlyphTheme>(UI.GlyphTheme);
    }

    [Fact]
    public void DefaultGlyphTheme_HasExpectedDefaults()
    {
        IGlyphTheme g = new DefaultGlyphTheme();
        Assert.Equal("[X]", g.CheckboxChecked);
        Assert.Equal("( )", g.RadioUnselected);
    }

    [Fact]
    public void CustomGlyphTheme_FallsBackToDefaultsForUnsetMembers()
    {
        IGlyphTheme g = new AsciiGlyphTheme();
        Assert.Equal("[*]", g.CheckboxChecked);     // overridden
        Assert.Equal("[ ]", g.CheckboxUnchecked);   // default (not overridden)
    }
    #endregion

    #region Glyph theme drives appearance + layout
    [Fact]
    public void Checkbox_UsesThemedGlyph()
    {
        WithGlyphTheme(new AsciiGlyphTheme(), () =>
        {
            var cb = new Checkbox("OK", isChecked: true);
            Assert.Contains("[*] OK", ConsoleSnapshot.ToText(cb, 12, 1));
        });
    }

    [Fact]
    public void Switch_GlyphWidth_DrivesControlWidth()
    {
        var defaultWidth = new Switch("P").Width;            // "(─●)" = 4 + space + 1

        WithGlyphTheme(new AsciiGlyphTheme(), () =>
        {
            var themed = new Switch("P").Width;              // "[ON ]"/"[OFF]" = 5 + space + 1
            Assert.Equal(defaultWidth + 1, themed);
        });
    }

    [Fact]
    public void RadioSet_UsesThemedGlyph()
    {
        WithGlyphTheme(new AsciiGlyphTheme(), () =>
        {
            var rs = new RadioSet("Red", "Green") { SelectedIndex = 0 };
            var text = ConsoleSnapshot.ToText(rs, 14, 2);
            Assert.Contains("(o) Red", text);   // themed selected marker
            Assert.Contains("( ) Green", text);
        });
    }

    [Fact]
    public void ThemedGlyph_DoesNotChangeBehaviour()
    {
        WithGlyphTheme(new AsciiGlyphTheme(), () =>
        {
            var cb = new Checkbox("x");
            var m = (IMouseListener)cb;
            m.OnMouseDown(new Position(0, 0));
            m.OnMouseUp(new Position(0, 0));
            Assert.True(cb.IsChecked);   // clicking still toggles, regardless of glyph
        });
    }
    #endregion

    #region Style theme drives captured tokens
    [Fact]
    public void Checkbox_CapturesAccentFromStyleTheme()
    {
        WithStyleTheme(new AccentStyleTheme(), () =>
        {
            var cb = new Checkbox("x");
            Assert.Equal(Style.Magenta1, cb.AccentStyle);          // captured override
            Assert.Equal(((IStyleTheme)new AccentStyleTheme()).Text, cb.LabelStyle);   // default token
        });
    }

    [Fact]
    public void AssigningStyleTheme_IsALiveSwitch()
    {
        // Assigning the property (not just SetTheme) raises ThemeChanged, so a live control re-captures.
        var cb = new Checkbox("x");
        var prev = UI.StyleTheme;
        try
        {
            UI.StyleTheme = new AccentStyleTheme();      // TextAccent = Magenta1
            Assert.Equal(Style.Magenta1, cb.AccentStyle);
        }
        finally { UI.StyleTheme = prev; }
    }

    [Fact]
    public void Override_StillWinsOverTheme()
    {
        var cb = new Checkbox("x") { AccentStyle = Style.Red1 };
        Assert.Equal(Style.Red1, cb.AccentStyle);
    }
    #endregion

    #region Scrollbar theming (glyphs from glyph theme, colours from style theme)
    [Fact]
    public void DefaultGlyphTheme_ScrollBar_IsSmoothByDefault()
    {
        IGlyphTheme g = new DefaultGlyphTheme();
        Assert.Equal(ScrollBarMode.Smooth, g.ScrollBar.Mode);   // the modern block bar is the default
    }

    [Fact]
    public void ControlFrame_ComposesScrollBar_FromGlyphAndStyleThemes()
    {
        var prevG = UI.GlyphTheme; var prevS = UI.StyleTheme;
        UI.GlyphTheme = new BlockScrollGlyphTheme();    // glyph: block thumb
        UI.StyleTheme = new RedScrollStyleTheme();      // colour: red
        try
        {
            var lb = new ListBox("a");
            lb.WithRoundedBorder();
            var thumb = lb.Frame!.ScrollBarForeground;
            Assert.Equal('█', thumb.Content);                       // glyph from the glyph theme
            Assert.Equal((byte)255, thumb.Foreground!.Value.Red);   // colour from the style theme
            Assert.Equal((byte)0, thumb.Foreground!.Value.Green);
        }
        finally { UI.GlyphTheme = prevG; UI.StyleTheme = prevS; }
    }

    [Fact]
    public void ControlFrame_ScrollBarGlyphsOverride_Wins()
    {
        WithGlyphTheme(new BlockScrollGlyphTheme(), () =>
        {
            var lb = new ListBox("a");
            lb.WithRoundedBorder().WithScrollBarGlyphs(ScrollBarGlyphs.Shaded);
            Assert.Equal('▒', lb.Frame!.ScrollBarForeground.Content);   // explicit glyph override wins
        });
    }

    [Fact]
    public void ControlFrame_ScrollBarStyleOverride_Wins()
    {
        var lb = new ListBox("a");
        lb.WithRoundedBorder().WithScrollBarStyle(ScrollBarStyle.Uniform(Style.Lime));
        var thumb = lb.Frame!.ScrollBarForeground;
        Assert.Equal((byte)0, thumb.Foreground!.Value.Red);     // Lime = (0,255,0)
        Assert.Equal((byte)255, thumb.Foreground!.Value.Green);
    }
    #endregion

    #region Frame border/title theming (all on the style theme now)
    [Fact]
    public void DefaultStyleTheme_FrameTokens_HaveExpectedDefaults()
    {
        IStyleTheme s = new DefaultStyleTheme();
        Assert.Equal(BorderStyle.None, s.FrameBorder);
        Assert.Equal(TitleStyle.Default, s.TitleStyle);
    }

    [Fact]
    public void ControlFrame_DefaultBorder_ComesFromStyleTheme()
    {
        WithStyleTheme(new FramedStyleTheme(), () =>
        {
            var frame = new ControlFrame(new ListBox("a"));   // no explicit border
            Assert.Equal(BorderStyle.Rounded, frame.BorderStyle);
        });
    }

    [Fact]
    public void ControlFrame_ExplicitBorder_OverridesTheme()
    {
        WithStyleTheme(new FramedStyleTheme(), () =>
        {
            var frame = new ControlFrame(new ListBox("a"), borderStyle: BorderStyle.Double);
            Assert.Equal(BorderStyle.Double, frame.BorderStyle);
        });
    }

    [Fact]
    public void ControlFrame_DefaultTitleStyle_ComesFromStyleTheme()
    {
        WithStyleTheme(new FramedStyleTheme(), () =>
        {
            var frame = new ControlFrame(new ListBox("a"), title: "T");
            Assert.Equal(TitlePos.BottomCenter, frame.TitleStyle.Pos);
            Assert.Equal(TitleBorderStyle.Inline, frame.TitleStyle.BorderStyle);
            Assert.Equal(TitleColorStyle.Reverse, frame.TitleStyle.Color);
        });
    }

    [Fact]
    public void ControlFrame_ExplicitTitleStyle_OverridesTheme()
    {
        WithStyleTheme(new FramedStyleTheme(), () =>
        {
            var frame = new ControlFrame(new ListBox("a"), title: "T", titleStyle: TitleStyle.Default);
            Assert.Equal(TitlePos.TopLeft, frame.TitleStyle.Pos);   // explicit Default wins over themed BottomCenter
        });
    }
    #endregion

    #region Bundled ITheme
    private sealed class BundledTheme : ITheme
    {
        public IStyleTheme Styles => new AccentStyleTheme();   // Magenta accent
        public IGlyphTheme Glyphs => new AsciiGlyphTheme();    // "[*]" checkbox
    }

    private sealed class EmptyTheme : ITheme { }

    [Fact]
    public void ITheme_DefaultsToBuiltIns()
    {
        ITheme t = new EmptyTheme();
        Assert.IsType<DefaultStyleTheme>(t.Styles);
        Assert.IsType<DefaultGlyphTheme>(t.Glyphs);
    }

    [Fact]
    public void SetTheme_ITheme_AppliesBothHalves()
    {
        var cb = new Checkbox("x", isChecked: true);
        try
        {
            UI.SetTheme(new BundledTheme());
            Assert.Equal(Style.Magenta1, cb.AccentStyle);                  // style half
            Assert.Contains("[*] x", ConsoleSnapshot.ToText(cb, 10, 1));   // glyph half
        }
        finally { UI.SetTheme(new DefaultStyleTheme(), new DefaultGlyphTheme()); }
    }
    #endregion

    #region Live theme switching (UI.SetTheme)
    [Fact]
    public void SetTheme_LiveUpdatesGlyphs_OnExistingControl()
    {
        var cb = new Checkbox("OK", isChecked: true);
        Assert.Contains("[X] OK", ConsoleSnapshot.ToText(cb, 12, 1));
        try
        {
            UI.SetTheme(new DefaultStyleTheme(), new AsciiGlyphTheme());   // CheckboxChecked = "[*]"
            Assert.Contains("[*] OK", ConsoleSnapshot.ToText(cb, 12, 1));  // re-glyphed live
        }
        finally { UI.SetTheme(new DefaultStyleTheme(), new DefaultGlyphTheme()); }
    }

    [Fact]
    public void SetTheme_LiveUpdatesStyle_OnExistingControl()
    {
        var cb = new Checkbox("x");
        Assert.NotEqual(Style.Magenta1, cb.AccentStyle);
        try
        {
            UI.SetTheme(new AccentStyleTheme(), new DefaultGlyphTheme());   // TextAccent = Magenta1
            Assert.Equal(Style.Magenta1, cb.AccentStyle);
        }
        finally { UI.SetTheme(new DefaultStyleTheme(), new DefaultGlyphTheme()); }
    }

    [Fact]
    public void SetTheme_GlyphWidthChange_ResizesLiveControl()
    {
        var sw = new Switch("P");
        var before = sw.Width;
        try
        {
            UI.SetTheme(new DefaultStyleTheme(), new AsciiGlyphTheme());   // switch glyph width 5 vs 4
            Assert.Equal(before + 1, sw.Width);
        }
        finally { UI.SetTheme(new DefaultStyleTheme(), new DefaultGlyphTheme()); }
    }

    [Fact]
    public void SetTheme_PreservesExplicitOverride()
    {
        var cb = new Checkbox("x") { AccentStyle = Style.Red1 };   // explicit override
        Assert.Equal(Style.Red1, cb.AccentStyle);
        try
        {
            UI.SetTheme(new AccentStyleTheme(), new DefaultGlyphTheme());   // theme TextAccent = Magenta1
            Assert.Equal(Style.Red1, cb.AccentStyle);   // explicit override survives the switch (not clobbered)
        }
        finally { UI.SetTheme(new DefaultStyleTheme(), new DefaultGlyphTheme()); }
    }

    [Fact]
    public void SetTheme_UpdatesNonOverriddenSiblings_WhenOneIsOverridden()
    {
        // Override only AccentStyle; LabelStyle should still follow the theme on a switch.
        var cb = new Checkbox("x") { AccentStyle = Style.Red1 };
        try
        {
            UI.SetTheme(new AccentStyleTheme(), new DefaultGlyphTheme());
            Assert.Equal(Style.Red1, cb.AccentStyle);                                  // overridden → kept
            Assert.Equal(((IStyleTheme)new AccentStyleTheme()).Text, cb.LabelStyle);   // not overridden → followed
        }
        finally { UI.SetTheme(new DefaultStyleTheme(), new DefaultGlyphTheme()); }
    }
    #endregion

    #region Live theme switching — Plot and TextLabel (the scope colour-scheme path)
    private static readonly PlotStyleTheme ThemeA =
        new(new Color(10, 20, 30), new Color(1, 2, 3), new Color(4, 5, 6), new Color(7, 8, 9));
    private static readonly PlotStyleTheme ThemeB =
        new(new Color(90, 80, 70), new Color(11, 12, 13), new Color(14, 15, 16), new Color(17, 18, 19));

    [Fact]
    public void SetTheme_LiveUpdatesEveryPlotToken_OnExistingPlot()
    {
        try
        {
            UI.SetTheme(ThemeA, new DefaultGlyphTheme());
            var plot = new Plot();
            Assert.Equal(new Color(10, 20, 30), plot.Background);
            Assert.Equal(new Color(1, 2, 3), plot.AxisColor);
            Assert.Equal(new Color(1, 2, 3), plot.GridColor);
            Assert.Equal(new Color(4, 5, 6), plot.TickColor);
            Assert.Equal(new Color(7, 8, 9), plot.TickLabelColor);
            Assert.Equal(new Color(1, 2, 3), plot.SeriesPalette[0]);

            UI.SetTheme(ThemeB, new DefaultGlyphTheme());   // the live switch under test
            Assert.Equal(new Color(90, 80, 70), plot.Background);
            Assert.Equal(new Color(11, 12, 13), plot.AxisColor);
            Assert.Equal(new Color(11, 12, 13), plot.GridColor);
            Assert.Equal(new Color(14, 15, 16), plot.TickColor);
            Assert.Equal(new Color(17, 18, 19), plot.TickLabelColor);
            Assert.Equal(new Color(11, 12, 13), plot.SeriesPalette[0]);
        }
        finally { UI.SetTheme(new DefaultStyleTheme(), new DefaultGlyphTheme()); }
    }

    [Fact]
    public void SetTheme_LiveUpdatesBothLabelColours_OnExistingTextLabel()
    {
        try
        {
            UI.SetTheme(ThemeA, new DefaultGlyphTheme());
            var label = new TextLabel(TextLabelOrientation.Horizontal, "hdr");
            Assert.Equal(new Color(7, 8, 9), label.FgColor);
            Assert.Equal(new Color(10, 20, 30), label.BgColor);   // the token carries a background too

            UI.SetTheme(ThemeB, new DefaultGlyphTheme());
            Assert.Equal(new Color(17, 18, 19), label.FgColor);
            Assert.Equal(new Color(90, 80, 70), label.BgColor);
        }
        finally { UI.SetTheme(new DefaultStyleTheme(), new DefaultGlyphTheme()); }
    }

    [Fact]
    public void DefaultTheme_LeavesTextLabelColoursUnset()
    {
        // LabelText defaults to Style.Plain, which carries neither colour — a label must stay transparent by
        // default, or it would paint an opaque background and break overlay-scrim compositing.
        var label = new TextLabel(TextLabelOrientation.Horizontal, "plain");
        Assert.Null(label.FgColor);
        Assert.Null(label.BgColor);
    }

    [Fact]
    public void SetTheme_LeavesExplicitlySetColoursAlone()
    {
        try
        {
            UI.SetTheme(ThemeA, new DefaultGlyphTheme());
            var label = new TextLabel(TextLabelOrientation.Horizontal, "x", new Color(200, 100, 50));
            var plot = new Plot();
            plot.SetAxisColor(new Color(255, 0, 255));

            UI.SetTheme(ThemeB, new DefaultGlyphTheme());
            Assert.Equal(new Color(200, 100, 50), label.FgColor);   // ctor colour is an override
            Assert.Equal(new Color(255, 0, 255), plot.AxisColor);   // SetAxisColor marks one too
            Assert.Equal(new Color(90, 80, 70), label.BgColor);     // ...but the half left unset still follows
            Assert.Equal(new Color(11, 12, 13), plot.GridColor);
        }
        finally { UI.SetTheme(new DefaultStyleTheme(), new DefaultGlyphTheme()); }
    }
    #endregion

    #region Live theme switching — ControlFrame (override-aware)
    [Fact]
    public void SetTheme_UpdatesFrame_NonOverriddenBorder()
    {
        var frame = new ControlFrame(new ListBox("a"));   // border left at the theme default (None)
        Assert.Equal(BorderStyle.None, frame.BorderStyle);
        try
        {
            UI.SetTheme(new FramedStyleTheme(), new DefaultGlyphTheme());   // FrameBorder = Rounded
            Assert.Equal(BorderStyle.Rounded, frame.BorderStyle);          // followed the theme
        }
        finally { UI.SetTheme(new DefaultStyleTheme(), new DefaultGlyphTheme()); }
    }

    [Fact]
    public void SetTheme_PreservesFrame_ExplicitBorder()
    {
        var lb = new ListBox("a");
        lb.WithRoundedBorder(Color.Cyan1);   // explicit shape + colour
        try
        {
            UI.SetTheme(new DefaultStyleTheme(), new DefaultGlyphTheme());   // default FrameBorder is None
            Assert.Equal(BorderStyle.Rounded, lb.Frame!.BorderStyle);        // explicit override survives the switch
            Assert.Equal(Color.Cyan1, lb.Frame!.BorderFgColor);
        }
        finally { UI.SetTheme(new DefaultStyleTheme(), new DefaultGlyphTheme()); }
    }
    #endregion

    #region Style value-equality
    [Fact]
    public void Style_ValueEquality()
    {
        Assert.True(Style.Green1 == (Style)Color.Green1);
        Assert.Equal(Style.Green1, (Style)Color.Green1);
        Assert.NotEqual(Style.Green1, Style.Red1);
    }

    [Fact]
    public void Style_ColorAccessors_ReturnNullForDefault()
    {
        Assert.Null(Style.Plain.ForegroundColor);              // no foreground set
        Assert.Equal(Color.Green1, Style.Green1.ForegroundColor);
    }
    #endregion
}
