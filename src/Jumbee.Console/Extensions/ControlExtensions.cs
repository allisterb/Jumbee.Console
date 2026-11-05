namespace Jumbee.Console;

using ConsoleGUI.Common;
using ConsoleGUI.Space;
using System;

/// <summary>Fluent extension helpers for configuring controls, frames, and geometry values.</summary>
public static class ControlExtensions
{
    /// <summary>
    /// The render node to bind into a parent's <see cref="DrawingContext"/> for a child pane.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For a nested <see cref="ILayout"/> this MUST be its wrapped <see cref="ILayout.CControl"/>, not the layout
    /// wrapper itself: a <see cref="DrawingContext"/> identity-checks the bubbling control against its <c>Child</c>
    /// (<c>control != Child → drop</c>), and a Layout's wrapped ConsoleGUI control bubbles as itself — so binding the
    /// Layout proxy silently drops ALL damage from the nested layout, leaving the frame loop to fall back to a
    /// full-screen redraw every frame (the "dashboards force a full composite every tick" bug). Controls (and framed
    /// controls, via <see cref="IFocusable.FocusableControl"/>) already bubble as the same object they're bound as, so
    /// they're unaffected.
    /// </para>
    /// <para>
    /// A layout that binds render nodes must keep its focus/routing children in its own fields (every built-in layout
    /// does): routing that cast the bound ConsoleGUI child back to <see cref="IFocusable"/> would get the CControl,
    /// not the layout. Grid and the stack panels once routed that way and bound the wrapper instead, so a control
    /// inside a layout nested in them never reported damage, and its change reached the screen only when something
    /// forced a full redraw.
    /// </para>
    /// </remarks>
    internal static ConsoleGUI.IControl RenderNode(this IFocusable focusable) =>
        focusable is ILayout layout ? layout.CControl : focusable.FocusableControl;

    /// <summary>Deconstructs a <see cref="Position"/> into its <paramref name="X"/> and <paramref name="Y"/> components.</summary>
    public static void Deconstruct(this Position position, out int X, out int Y)
    {
        X = position.X;
        Y = position.Y;
    }

    /// <summary>Subtracts <paramref name="position2"/> from <paramref name="position1"/>, clamping each axis at zero.</summary>
    public static Position SubtractClamp(this Position position1, Position position2)
    {
        var x = position1.X - position2.X;
        var y = position1.Y - position2.Y;
        return new Position(Math.Max(0, x), Math.Max(0, y));
    }

    /// <summary>Returns <paramref name="position"/> offset by <paramref name="x"/> columns and <paramref name="y"/> rows.</summary>
    public static Position Add(this Position position, int x, int y) => new Position(position.X + x, position.Y + y);

    /// <summary>Returns <paramref name="size"/> with its width reduced by <paramref name="width"/>.</summary>
    public static CSize SubtractWidth(this CSize size, int width) => new CSize(size.Width - width, size.Height);

    /// <summary>Sets the control's <see cref="Control.Width"/> and returns it.</summary>
    public static T WithWidth<T>(this T control, int width) where T : Control
    {
        control.Width = width;
        return control;
    }

    /// <summary>Sets the control's <see cref="Control.Height"/> and returns it.</summary>
    public static T WithHeight<T>(this T control, int height) where T : Control
    {
        control.Height = height;
        return control;
    }

    /// <summary>Sets the control's width and/or height (at least one must be supplied) and returns it.</summary>
    public static T WithSize<T>(this T control, int? width = null, int? height = null) where T : Control
    {
        if (width is null && height is null) throw new ArgumentNullException("You must specify either a width or height.");

        if (width.HasValue)
        {
            control.Width = width.Value;
        }
        if (height.HasValue)
        {
            control.Height = height.Value;
        }
        return control;
    }

    /// <summary>Sets the control's <see cref="Control.Frame"/> to <paramref name="frame"/> and returns it.</summary>
    public static T WithFrame<T>(this T control, ControlFrame frame) where T : Control
    {
        control.Frame = frame;
        return control;
    }
    /// <summary>Creates or updates the control's <see cref="ControlFrame"/> from the supplied border, margin, colour,
    /// and title options (only supplied arguments are applied) and returns the control.</summary>
    public static T WithFrame<T>(this T control, BorderStyle? borderStyle = null, Offset? margin = null, Color? fgColor = null, Color? bgColor = null, string? title = null, Color? borderFgColor = null, Color? borderBgColor = null, BorderPlacement? borderPlacement = null, BorderStyle? focusedBorderStyle = null) where T : Control
    {
        // Assign only the arguments actually supplied: self-assigning (x ?? frame.x) would fire the themeable
        // setters and wrongly mark those properties as explicit overrides, freezing them against theme switches.
        var frame = control.Frame ??= new ControlFrame(control);
        if (borderStyle.HasValue) frame.BorderStyle = borderStyle.Value;
        if (margin.HasValue) frame.Margin = margin.Value;
        if (fgColor.HasValue) frame.Foreground = fgColor.Value;
        if (bgColor.HasValue) frame.Background = bgColor.Value;
        if (title is not null) frame.Title = title;
        if (borderFgColor.HasValue) frame.BorderFgColor = borderFgColor.Value;
        if (borderBgColor.HasValue) frame.BorderBgColor = borderBgColor.Value;
        if (borderPlacement.HasValue) frame.BorderPlacement = borderPlacement.Value;
        if (focusedBorderStyle.HasValue) frame.FocusedBorderStyle = focusedBorderStyle.Value;
        return control;
    }

    /// <summary>Sets the frame's margin to the given left/top/right/bottom offsets (creating a frame if needed) and returns the control.</summary>
    public static T WithMargin<T>(this T control, int left, int top, int right, int bottom) where T : Control
    {
        if (control.Frame != null)
        {
            control.Frame.Margin = new Offset(left, top, right, bottom);
            return control;
        }
        else
        {
            control.Frame = new ControlFrame(control, margin: new Offset(left, top, right, bottom));
            return control;
        }
    }

    /// <summary>Sets a uniform frame margin of <paramref name="offset"/> on all sides and returns the control.</summary>
    public static T WithMargin<T>(this T control, int offset) where T : Control => control.WithMargin(offset, offset, offset, offset);

    /// <summary>Sets the frame's border style, colours, and placement (creating a frame if needed) and returns the control.</summary>
    public static T WithBorder<T>(this T control, BorderStyle? style, Color? borderFgColor = null, Color? borderBgColor = null, BorderPlacement? borderPlacement = null) where T : Control
    {
        var frame = control.Frame ??= new ControlFrame(control);
        if (style.HasValue) frame.BorderStyle = style.Value;
        if (borderFgColor.HasValue) frame.BorderFgColor = borderFgColor.Value;
        if (borderBgColor.HasValue) frame.BorderBgColor = borderBgColor.Value;
        if (borderPlacement.HasValue) frame.BorderPlacement = borderPlacement.Value;
        return control;
    }

    /// <summary>Sets the frame's title (creating a frame if needed) and returns the control.</summary>
    public static T WithTitle<T>(this T control, string title) where T : Control
    {
        var frame = control.Frame ??= new ControlFrame(control);
        frame.Title = title;
        return control;
    }

    /// <summary>Sets the frame's title and <see cref="TitleStyle"/> (creating a frame if needed) and returns the control.</summary>
    public static T WithTitle<T>(this T control, string title, TitleStyle titleStyle) where T : Control
    {
        var frame = control.Frame ??= new ControlFrame(control);
        frame.Title = title;
        frame.TitleStyle = titleStyle;
        return control;
    }

    /// <summary>Sets the frame's title with the given position, border style, and colour style (creating a frame if needed) and returns the control.</summary>
    public static T WithTitle<T>(this T control, string title, TitlePos pos, TitleBorderStyle borderStyle, TitleColorStyle color) where T : Control
    {
        var frame = control.Frame ??= new ControlFrame(control);
        frame.Title = title;
        frame.TitleStyle = new TitleStyle(pos, borderStyle, color);
        return control;
    }

    /// <summary>Sets the frame's <see cref="ScrollBarStyle"/> (creating a frame if needed) and returns the control.</summary>
    public static T WithScrollBarStyle<T>(this T control, ScrollBarStyle style) where T : Control
    {
        var frame = control.Frame ??= new ControlFrame(control);
        frame.ScrollBarStyle = style;
        return control;
    }

    /// <summary>Sets the frame's <see cref="ScrollBarGlyphs"/> (creating a frame if needed) and returns the control.</summary>
    public static T WithScrollBarGlyphs<T>(this T control, ScrollBarGlyphs glyphs) where T : Control
    {
        var frame = control.Frame ??= new ControlFrame(control);
        frame.ScrollBarGlyphs = glyphs;
        return control;
    }

    /// <summary>Removes the frame's border (<see cref="BorderStyle.None"/>) and returns the control.</summary>
    public static T WithNoBorder<T>(this T control) where T : Control =>
        control.WithBorder(BorderStyle.None);

    /// <summary>Applies an ASCII border with optional colours and returns the control.</summary>
    public static T WithAsciiBorder<T>(this T control, Color? borderFgColor = null, Color? borderBgColor = null) where T : Control =>
        control.WithBorder(BorderStyle.Ascii, borderFgColor, borderBgColor);

    /// <summary>Applies a heavy-line border with optional colours and returns the control.</summary>
    public static T WithHeavyBorder<T>(this T control, Color? borderFgColor = null, Color? borderBgColor = null) where T : Control =>
         control.WithBorder(BorderStyle.Heavy, borderFgColor, borderBgColor);

    /// <summary>Applies a double-line border with optional colours and returns the control.</summary>
    public static T WithDoubleBorder<T>(this T control, Color? borderFgColor = null, Color? borderBgColor = null) where T : Control =>
        control.WithBorder(BorderStyle.Double, borderFgColor, borderBgColor);

    /// <summary>Applies a rounded border with optional colours and returns the control.</summary>
    public static T WithRoundedBorder<T>(this T control, Color? borderFgColor = null, Color? borderBgColor = null) where T : Control =>
        control.WithBorder(BorderStyle.Rounded, borderFgColor, borderBgColor);

    /// <summary>Applies a square border with optional colours and returns the control.</summary>
    public static T WithSquareBorder<T>(this T control, Color? borderFgColor = null, Color? borderBgColor = null) where T : Control =>
        control.WithBorder(BorderStyle.Square, borderFgColor, borderBgColor);

    /// <summary>Wraps <paramref name="s"/> in a Spectre <see cref="Spectre.Console.Markup"/> using <paramref name="style"/>.</summary>
    public static Spectre.Console.Markup WithStyle(this string s, Style style) => style[s];
}
