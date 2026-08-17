# <a id="Jumbee_Console_IStyleTheme"></a> Interface IStyleTheme

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.Styles.dll  

The general appearance theme: a set of semantic <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> tokens (foreground + background +
decoration) that controls compose when resolving their default appearance, plus the rest of a control's
non-glyph styling — e.g. a frame's border shape (<xref href="Jumbee.Console.IStyleTheme.FrameBorder" data-throw-if-not-resolved="false"></xref>) and its title's
position/border/colour.

```csharp
public interface IStyleTheme
```

## Remarks

<p>
Only the literal glyphs rendered in controls live in <xref href="Jumbee.Console.IGlyphTheme" data-throw-if-not-resolved="false"></xref>. A theme defines
<em>appearance only</em>; it never changes a control's behaviour. Members are default-implemented, so a
custom theme overrides only what it wants to change. Because the members are default interface
implementations, hold and read a theme through this interface type (e.g. <code>UI.StyleTheme</code>), not
through a concrete class.
</p>
<p>
Controls must read these tokens <em>once</em> (in their constructor) into plain fields — never on the
render path — so theming costs nothing per frame. See <xref href="Jumbee.Console.DefaultStyleTheme" data-throw-if-not-resolved="false"></xref> for the built-in values.
</p>

## Properties

### <a id="Jumbee_Console_IStyleTheme_BorderFocusedText"></a> BorderFocusedText

The text/character style of a frame border when its control is focused.

```csharp
Style BorderFocusedText { get; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_IStyleTheme_BorderText"></a> BorderText

The text/character style of a frame border at rest (its colour).

```csharp
Style BorderText { get; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

#### Remarks

Distinct from <xref href="Jumbee.Console.IStyleTheme.FrameBorder" data-throw-if-not-resolved="false"></xref>, which selects the border <em>shape</em>.

### <a id="Jumbee_Console_IStyleTheme_Error"></a> Error

The style for error messages and indicators.

```csharp
Style Error { get; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_IStyleTheme_Focus"></a> Focus

The default focus cue applied to a focused control that isn't framed with a visible border and
    doesn't indicate focus in its own way — so keyboard focus is always visible.

```csharp
Style Focus { get; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

#### Remarks

Controls that show focus themselves (buttons, tabs, editors with a cursor) opt out via
    <code>Control.RendersOwnFocus</code>. The colour is used per <xref href="Jumbee.Console.IStyleTheme.FocusStyle" data-throw-if-not-resolved="false"></xref>.

### <a id="Jumbee_Console_IStyleTheme_FocusStyle"></a> FocusStyle

How the default focus cue (see <xref href="Jumbee.Console.IStyleTheme.Focus" data-throw-if-not-resolved="false"></xref>) is drawn — a full tint, an edge ring, or an
    underline. Defaults to <xref href="Jumbee.Console.FocusStyle.Tint" data-throw-if-not-resolved="false"></xref>.

```csharp
FocusStyle FocusStyle { get; }
```

#### Property Value

 [FocusStyle](Jumbee.Console.FocusStyle.md)

### <a id="Jumbee_Console_IStyleTheme_FocusedFrameBorder"></a> FocusedFrameBorder

The border shape a control frame uses while its control is focused, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to keep
    <xref href="Jumbee.Console.IStyleTheme.FrameBorder" data-throw-if-not-resolved="false"></xref> unchanged (showing focus through the <xref href="Jumbee.Console.IStyleTheme.BorderFocusedText" data-throw-if-not-resolved="false"></xref> colour only).
    Defaults to <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a>.

```csharp
BorderStyle? FocusedFrameBorder { get; }
```

#### Property Value

 [BorderStyle](Jumbee.Console.BorderStyle.md)?

#### Remarks

Switching shape on focus never changes the frame's geometry — the border offset comes from
    <code>BorderPlacement</code>, not the shape — so a focused frame restyles in place without reflowing its
    siblings.

### <a id="Jumbee_Console_IStyleTheme_FrameBorder"></a> FrameBorder

The default border shape for a control frame when none is specified. Defaults to <xref href="Jumbee.Console.BorderStyle.None" data-throw-if-not-resolved="false"></xref>.

```csharp
BorderStyle FrameBorder { get; }
```

#### Property Value

 [BorderStyle](Jumbee.Console.BorderStyle.md)

### <a id="Jumbee_Console_IStyleTheme_Gauge"></a> Gauge

The fill/track/text colours a <code>Gauge</code> uses. Defaults to
    <xref href="Jumbee.Console.GaugeStyle.Default" data-throw-if-not-resolved="false"></xref>.

```csharp
GaugeStyle Gauge { get; }
```

#### Property Value

 [GaugeStyle](Jumbee.Console.GaugeStyle.md)

### <a id="Jumbee_Console_IStyleTheme_Hover"></a> Hover

A row/control under the pointer (background tint).

```csharp
Style Hover { get; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_IStyleTheme_Info"></a> Info

The style for informational messages and indicators.

```csharp
Style Info { get; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_IStyleTheme_LabelText"></a> LabelText

The colours a plain <code>TextLabel</code> draws with — <em>both</em> halves are read, so a theme can give
    labels a background as well as a foreground.

```csharp
Style LabelText { get; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

#### Remarks

Defaults to <xref href="Jumbee.Console.Style.Plain" data-throw-if-not-resolved="false"></xref>, which carries neither colour and so leaves a label exactly as it is
unthemed: foreground at the terminal default, background transparent. That matters — a label painting an
opaque background by default would block compositing and dim oddly under an overlay scrim. Distinct from
<xref href="Jumbee.Console.IStyleTheme.Text" data-throw-if-not-resolved="false"></xref>, which is the token for body text a control renders itself.

### <a id="Jumbee_Console_IStyleTheme_PlotAxis"></a> PlotAxis

The colour of a <code>Plot</code>'s axis lines.

```csharp
Style PlotAxis { get; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

#### Remarks

Only its foreground colour is used. The axis <em>captions</em> are set per-plot with
    <code>SetAxisTitles</code> and are not themed.

### <a id="Jumbee_Console_IStyleTheme_PlotGrid"></a> PlotGrid

The colour of a <code>Plot</code>'s background grid lines.

```csharp
Style PlotGrid { get; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

#### Remarks

Only its foreground colour is used. The grid's dashed/solid <em>brush</em> is a per-plot setting.

### <a id="Jumbee_Console_IStyleTheme_PlotSeries"></a> PlotSeries

The ordered colours a <code>Plot</code> cycles through for series that don't name one. Defaults to
    <xref href="Jumbee.Console.PlotPalette.Default" data-throw-if-not-resolved="false"></xref>.

```csharp
PlotPalette PlotSeries { get; }
```

#### Property Value

 [PlotPalette](Jumbee.Console.PlotPalette.md)

### <a id="Jumbee_Console_IStyleTheme_PlotSurface"></a> PlotSurface

The fill painted behind a <code>Plot</code>'s drawing area.

```csharp
Style PlotSurface { get; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

#### Remarks

Only its background colour is used; the default has none, leaving a plot transparent so whatever is
    behind it shows through.

### <a id="Jumbee_Console_IStyleTheme_PlotTick"></a> PlotTick

The colour of a <code>Plot</code>'s tick marks.

```csharp
Style PlotTick { get; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

#### Remarks

Only its foreground colour is used. Separate from <xref href="Jumbee.Console.IStyleTheme.PlotTickLabel" data-throw-if-not-resolved="false"></xref>: scope-style themes
    group the marks with the axis and grid as structure, and colour the numbers with the other text.

### <a id="Jumbee_Console_IStyleTheme_PlotTickLabel"></a> PlotTickLabel

The colour of a <code>Plot</code>'s numeric tick labels.

```csharp
Style PlotTickLabel { get; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

#### Remarks

Only its foreground colour is used. See <xref href="Jumbee.Console.IStyleTheme.PlotTick" data-throw-if-not-resolved="false"></xref> for why this is its own token.

### <a id="Jumbee_Console_IStyleTheme_Primary"></a> Primary

A primary action surface at rest (e.g. a default button).

```csharp
Style Primary { get; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_IStyleTheme_PrimaryActive"></a> PrimaryActive

A primary action surface while pressed/active.

```csharp
Style PrimaryActive { get; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_IStyleTheme_PrimaryButton"></a> PrimaryButton

The default style for a primary <code>Button</code> (its per-state fills, border mode, and width).

```csharp
ButtonStyle PrimaryButton { get; }
```

#### Property Value

 [ButtonStyle](Jumbee.Console.ButtonStyle.md)

#### Remarks

Composed from the <xref href="Jumbee.Console.IStyleTheme.Primary" data-throw-if-not-resolved="false"></xref> family so a theme that recolours those gets a matching
    button for free. Flat by default; a button can opt into <xref href="Jumbee.Console.ButtonShape.Modern" data-throw-if-not-resolved="false"></xref> for the raised
    look.

### <a id="Jumbee_Console_IStyleTheme_PrimaryHover"></a> PrimaryHover

A primary action surface under the pointer.

```csharp
Style PrimaryHover { get; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_IStyleTheme_ProgressBar"></a> ProgressBar

The part styles a <code>ProgressBar</code> composes (description, bar fill/track, percentage, time,
    spinner). Defaults to <xref href="Jumbee.Console.ProgressBarStyle.Default" data-throw-if-not-resolved="false"></xref>.

```csharp
ProgressBarStyle ProgressBar { get; }
```

#### Property Value

 [ProgressBarStyle](Jumbee.Console.ProgressBarStyle.md)

### <a id="Jumbee_Console_IStyleTheme_Scrim"></a> Scrim

The tint a modal overlay scrim blends the layer beneath it toward. Defaults to a near-black.

```csharp
Style Scrim { get; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

#### Remarks

Only its background colour is used. Paired with <xref href="Jumbee.Console.IStyleTheme.ScrimDim" data-throw-if-not-resolved="false"></xref>.

### <a id="Jumbee_Console_IStyleTheme_ScrimDim"></a> ScrimDim

How strongly a modal scrim dims the layer beneath it: 0 = fully see-through, 1 = a solid
    <xref href="Jumbee.Console.IStyleTheme.Scrim" data-throw-if-not-resolved="false"></xref> fill. Defaults to 0.6.

```csharp
float ScrimDim { get; }
```

#### Property Value

 float

### <a id="Jumbee_Console_IStyleTheme_ScrollBar"></a> ScrollBar

The per-part colours/decoration a control frame's vertical scrollbar uses (glyphs come from
    <xref href="Jumbee.Console.IGlyphTheme.ScrollBar" data-throw-if-not-resolved="false"></xref>). Defaults to <xref href="Jumbee.Console.ScrollBarStyle.Default" data-throw-if-not-resolved="false"></xref>.

```csharp
ScrollBarStyle ScrollBar { get; }
```

#### Property Value

 [ScrollBarStyle](Jumbee.Console.ScrollBarStyle.md)

### <a id="Jumbee_Console_IStyleTheme_Secondary"></a> Secondary

A secondary/neutral action surface at rest.

```csharp
Style Secondary { get; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_IStyleTheme_SecondaryActive"></a> SecondaryActive

A secondary action surface while pressed/active.

```csharp
Style SecondaryActive { get; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_IStyleTheme_SecondaryButton"></a> SecondaryButton

The default style for a secondary <code>Button</code>.

```csharp
ButtonStyle SecondaryButton { get; }
```

#### Property Value

 [ButtonStyle](Jumbee.Console.ButtonStyle.md)

#### Remarks

Composed from the <xref href="Jumbee.Console.IStyleTheme.Secondary" data-throw-if-not-resolved="false"></xref> family.

### <a id="Jumbee_Console_IStyleTheme_SecondaryHover"></a> SecondaryHover

A secondary action surface under the pointer.

```csharp
Style SecondaryHover { get; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_IStyleTheme_Selection"></a> Selection

A selected/highlighted row (foreground + background).

```csharp
Style Selection { get; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_IStyleTheme_SelectionStyle"></a> SelectionStyle

How navigable controls (ListBox, Tree, TabPanel) indicate their selected item — a background
    highlight, an underline, or a caret prefix. Defaults to <xref href="Jumbee.Console.SelectionStyle.Highlight" data-throw-if-not-resolved="false"></xref>.

```csharp
SelectionStyle SelectionStyle { get; }
```

#### Property Value

 [SelectionStyle](Jumbee.Console.SelectionStyle.md)

### <a id="Jumbee_Console_IStyleTheme_Slider"></a> Slider

The part styles a <code>Slider</code> composes (label, track fill/empty, thumb, value readout). The thumb
    glyph comes from <xref href="Jumbee.Console.IGlyphTheme.SliderThumb" data-throw-if-not-resolved="false"></xref>. Defaults to <xref href="Jumbee.Console.SliderStyle.Default" data-throw-if-not-resolved="false"></xref>.

```csharp
SliderStyle Slider { get; }
```

#### Property Value

 [SliderStyle](Jumbee.Console.SliderStyle.md)

### <a id="Jumbee_Console_IStyleTheme_Success"></a> Success

The style for success messages and indicators.

```csharp
Style Success { get; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_IStyleTheme_Surface"></a> Surface

A panel/container fill.

```csharp
Style Surface { get; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_IStyleTheme_Text"></a> Text

Primary body/label text.

```csharp
Style Text { get; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_IStyleTheme_TextAccent"></a> TextAccent

Accent-coloured text/marks (a checked box's tick, a selected radio's dot, a switch's "on" thumb).

```csharp
Style TextAccent { get; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_IStyleTheme_TextDisabled"></a> TextDisabled

Disabled text/controls.

```csharp
Style TextDisabled { get; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_IStyleTheme_TextMuted"></a> TextMuted

Secondary, de-emphasised text.

```csharp
Style TextMuted { get; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_IStyleTheme_TitleStyle"></a> TitleStyle

The default title style for a control frame — its position, border placement, and Normal/Reverse
    colouring, in one value. Defaults to <xref href="Jumbee.Console.TitleStyle.Default" data-throw-if-not-resolved="false"></xref>.

```csharp
TitleStyle TitleStyle { get; }
```

#### Property Value

 [TitleStyle](Jumbee.Console.TitleStyle.md)

### <a id="Jumbee_Console_IStyleTheme_TitleText"></a> TitleText

The text/character style of a frame title (its colour).

```csharp
Style TitleText { get; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

#### Remarks

Distinct from <xref href="Jumbee.Console.IStyleTheme.TitleStyle" data-throw-if-not-resolved="false"></xref>, which controls the title's placement, border, and
    Normal/Reverse colouring.

### <a id="Jumbee_Console_IStyleTheme_TreeLeaf"></a> TreeLeaf

The colour of a <code>Tree</code>'s leaf glyph (the marker before a childless node).
    Defaults to <xref href="Jumbee.Console.IStyleTheme.TextAccent" data-throw-if-not-resolved="false"></xref>.

```csharp
Style TreeLeaf { get; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

#### Remarks

Only its foreground is used.

### <a id="Jumbee_Console_IStyleTheme_Warning"></a> Warning

The style for warning messages and indicators.

```csharp
Style Warning { get; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

