# <a id="Jumbee_Console_ControlFrame"></a> Class ControlFrame

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

Draws a border around a control together with margins and a title bar, and sets the foreground and background colors.

```csharp
public sealed class ControlFrame : Control, IFocusable
```

#### Inheritance

object ← 
Control ← 
[ControlFrame](Jumbee.Console.ControlFrame.md)

#### Implements

[IFocusable](Jumbee.Console.IFocusable.md)

## Constructors

### <a id="Jumbee_Console_ControlFrame__ctor_Jumbee_Console_Control_System_Nullable_Jumbee_Console_BorderStyle__System_Nullable_ConsoleGUI_Space_Offset__System_Nullable_Jumbee_Console_Color__System_Nullable_Jumbee_Console_Color__System_String_System_Nullable_Jumbee_Console_Color__System_Nullable_Jumbee_Console_Color__System_Nullable_Jumbee_Console_TitleStyle__"></a> ControlFrame\(Control, BorderStyle?, Offset?, Color?, Color?, string?, Color?, Color?, TitleStyle?\)

Initializes a new <xref href="Jumbee.Console.ControlFrame" data-throw-if-not-resolved="false"></xref> wrapping <code class="paramref">control</code>, taking the border shape, margin, foreground/background and border colours, title, and title style (defaulting any unset value from the active theme).

```csharp
public ControlFrame(Control control, BorderStyle? borderStyle = null, Offset? margin = null, Color? fgColor = null, Color? bgColor = null, string? title = null, Color? borderFgColor = null, Color? borderBgColor = null, TitleStyle? titleStyle = null)
```

#### Parameters

`control` [Control](Jumbee.Console.Control.md)

`borderStyle` [BorderStyle](Jumbee.Console.BorderStyle.md)?

`margin` Offset?

`fgColor` [Color](Jumbee.Console.Color.md)?

`bgColor` [Color](Jumbee.Console.Color.md)?

`title` string?

`borderFgColor` [Color](Jumbee.Console.Color.md)?

`borderBgColor` [Color](Jumbee.Console.Color.md)?

`titleStyle` [TitleStyle](Jumbee.Console.TitleStyle.md)?

## Properties

### <a id="Jumbee_Console_ControlFrame_Background"></a> Background

The background colour filling the frame, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for none.

```csharp
public Color? Background { get; set; }
```

#### Property Value

 [Color](Jumbee.Console.Color.md)?

### <a id="Jumbee_Console_ControlFrame_BorderBgColor"></a> BorderBgColor

The border background colour, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for none.

```csharp
public Color? BorderBgColor { get; set; }
```

#### Property Value

 [Color](Jumbee.Console.Color.md)?

### <a id="Jumbee_Console_ControlFrame_BorderFgColor"></a> BorderFgColor

The border foreground colour, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for the theme default.

```csharp
public Color? BorderFgColor { get; set; }
```

#### Property Value

 [Color](Jumbee.Console.Color.md)?

### <a id="Jumbee_Console_ControlFrame_BorderPlacement"></a> BorderPlacement

Which edges of the frame draw a border.

```csharp
public BorderPlacement BorderPlacement { get; set; }
```

#### Property Value

 [BorderPlacement](Jumbee.Console.BorderPlacement.md)

### <a id="Jumbee_Console_ControlFrame_BorderStyle"></a> BorderStyle

The border shape drawn around the control.

```csharp
public BorderStyle BorderStyle { get; set; }
```

#### Property Value

 [BorderStyle](Jumbee.Console.BorderStyle.md)

### <a id="Jumbee_Console_ControlFrame_Control"></a> Control

The control wrapped by this frame; setting it rebinds the frame to the new control.

```csharp
public Control Control { get; set; }
```

#### Property Value

 [Control](Jumbee.Console.Control.md)

### <a id="Jumbee_Console_ControlFrame_DefaultMargin"></a> DefaultMargin

The default frame margin (zero on all sides) used when none is supplied.

```csharp
public static Offset DefaultMargin { get; }
```

#### Property Value

 Offset

### <a id="Jumbee_Console_ControlFrame_Focusable"></a> Focusable

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> (the default), the frame can receive keyboard focus.

```csharp
public bool Focusable { get; set; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_ControlFrame_FocusableControl"></a> FocusableControl

The focus target the UI registers for this frame — the frame itself, which routes input to the wrapped control.

```csharp
public IFocusable FocusableControl { get; }
```

#### Property Value

 [IFocusable](Jumbee.Console.IFocusable.md)

### <a id="Jumbee_Console_ControlFrame_FocusedBorderStyle"></a> FocusedBorderStyle

The border shape drawn while the frame is focused (or contains focus), overriding the theme's
    <xref href="Jumbee.Console.IStyleTheme.FocusedFrameBorder" data-throw-if-not-resolved="false"></xref>. Set to <xref href="Jumbee.Console.BorderStyle.None" data-throw-if-not-resolved="false"></xref> to suppress the focus
    border entirely — e.g. when the wrapped control already shows focus another way (a text cursor).

```csharp
public BorderStyle? FocusedBorderStyle { get; set; }
```

#### Property Value

 [BorderStyle](Jumbee.Console.BorderStyle.md)?

### <a id="Jumbee_Console_ControlFrame_FocusedControl"></a> FocusedControl

When something inside the frame is focused, the frame stays the routing node (so it can still intercept
scroll keys) but reports that focus is present.

```csharp
public IFocusable? FocusedControl { get; }
```

#### Property Value

 [IFocusable](Jumbee.Console.IFocusable.md)?

#### Remarks

This delegates to the wrapped control's <xref href="Jumbee.Console.Control.FocusedControl" data-throw-if-not-resolved="false"></xref> rather than the frame's own
<xref href="Jumbee.Console.ControlFrame.IsFocused" data-throw-if-not-resolved="false"></xref>, so focus nested deeper than one level — e.g. a child inside a
<xref href="Jumbee.Console.CompositeControl" data-throw-if-not-resolved="false"></xref> — still routes correctly.

### <a id="Jumbee_Console_ControlFrame_Foreground"></a> Foreground

The foreground colour used for the title text, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for the theme default.

```csharp
public Color? Foreground { get; set; }
```

#### Property Value

 [Color](Jumbee.Console.Color.md)?

### <a id="Jumbee_Console_ControlFrame_HandlesInput"></a> HandlesInput

Always <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> — the frame processes input (scroll keys) and tunnels the rest to the wrapped control.

```csharp
public bool HandlesInput { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_ControlFrame_IsFocused"></a> IsFocused

Whether the frame (and its wrapped control) holds focus; setting it raises the focus events and repaints the border cue.

```csharp
public bool IsFocused { get; set; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_ControlFrame_Margin"></a> Margin

The margin (empty space) between the border and the wrapped control.

```csharp
public Offset Margin { get; set; }
```

#### Property Value

 Offset

### <a id="Jumbee_Console_ControlFrame_ScrollBarBackground"></a> ScrollBarBackground

The glyph/cell drawn for the scrollbar track (background).

```csharp
public Character ScrollBarBackground { get; set; }
```

#### Property Value

 Character

### <a id="Jumbee_Console_ControlFrame_ScrollBarDownArrow"></a> ScrollBarDownArrow

The glyph/cell drawn for the scrollbar's down arrow.

```csharp
public Character ScrollBarDownArrow { get; set; }
```

#### Property Value

 Character

### <a id="Jumbee_Console_ControlFrame_ScrollBarForeground"></a> ScrollBarForeground

The glyph/cell drawn for the scrollbar thumb (foreground).

```csharp
public Character ScrollBarForeground { get; set; }
```

#### Property Value

 Character

### <a id="Jumbee_Console_ControlFrame_ScrollBarGlyphs"></a> ScrollBarGlyphs

Gets or sets the scrollbar glyphs (thumb, track, up/down arrows).

```csharp
public ScrollBarGlyphs ScrollBarGlyphs { get; set; }
```

#### Property Value

 [ScrollBarGlyphs](Jumbee.Console.ScrollBarGlyphs.md)

#### Remarks

Setting it recomposes the part cells with the current <xref href="Jumbee.Console.ControlFrame.ScrollBarStyle" data-throw-if-not-resolved="false"></xref> colours.

### <a id="Jumbee_Console_ControlFrame_ScrollBarStyle"></a> ScrollBarStyle

Gets or sets the scrollbar part colours/decoration.

```csharp
public ScrollBarStyle ScrollBarStyle { get; set; }
```

#### Property Value

 [ScrollBarStyle](Jumbee.Console.ScrollBarStyle.md)

#### Remarks

Setting it recomposes the part cells with the current <xref href="Jumbee.Console.ControlFrame.ScrollBarGlyphs" data-throw-if-not-resolved="false"></xref> glyphs.

### <a id="Jumbee_Console_ControlFrame_ScrollBarUpArrow"></a> ScrollBarUpArrow

The glyph/cell drawn for the scrollbar's up arrow.

```csharp
public Character ScrollBarUpArrow { get; set; }
```

#### Property Value

 Character

### <a id="Jumbee_Console_ControlFrame_ScrollDownKey"></a> ScrollDownKey

The key that scrolls the frame's content down one row.

```csharp
public ConsoleKeyInfo ScrollDownKey { get; set; }
```

#### Property Value

 ConsoleKeyInfo

### <a id="Jumbee_Console_ControlFrame_ScrollUpKey"></a> ScrollUpKey

The key that scrolls the frame's content up one row.

```csharp
public ConsoleKeyInfo ScrollUpKey { get; set; }
```

#### Property Value

 ConsoleKeyInfo

### <a id="Jumbee_Console_ControlFrame_Title"></a> Title

The text shown in the frame's title bar, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for no title.

```csharp
public string? Title { get; set; }
```

#### Property Value

 string?

### <a id="Jumbee_Console_ControlFrame_TitleStyle"></a> TitleStyle

The title's position, border style, and colour.

```csharp
public TitleStyle TitleStyle { get; set; }
```

#### Property Value

 [TitleStyle](Jumbee.Console.TitleStyle.md)

### <a id="Jumbee_Console_ControlFrame_Top"></a> Top

The vertical scroll offset (topmost visible row of the wrapped control); clamped to the scrollable range and raises <xref href="Jumbee.Console.ControlFrame.Scrolled" data-throw-if-not-resolved="false"></xref> on change.

```csharp
public int Top { get; set; }
```

#### Property Value

 int

### <a id="Jumbee_Console_ControlFrame_ViewportSize"></a> ViewportSize

The size of the visible content area inside the border and margins.

```csharp
public Size ViewportSize { get; }
```

#### Property Value

 Size

### <a id="Jumbee_Console_ControlFrame_Item_ConsoleGUI_Space_Position_"></a> this\[Position\]

Gets the composited <xref href="ConsoleGUI.Data.Cell" data-throw-if-not-resolved="false"></xref> at <code class="paramref">position</code>, drawing the border, title, scrollbar, and margins around the wrapped control.

```csharp
public override Cell this[Position position] { get; }
```

#### Property Value

 Cell

## Methods

### <a id="Jumbee_Console_ControlFrame_ApplyTheme"></a> ApplyTheme\(\)

Re-applies theme defaults to every themeable property the caller has <em>not</em> explicitly overridden
(tracked via <xref href="Jumbee.Console.ThemeOverrides" data-throw-if-not-resolved="false"></xref>), then re-lays-out.

```csharp
public void ApplyTheme()
```

#### Remarks

Invoked on a runtime theme switch through <xref href="Jumbee.Console.UI.ThemeChanged" data-throw-if-not-resolved="false"></xref> (wired by the owning control's
<xref href="Jumbee.Console.Control.Frame" data-throw-if-not-resolved="false"></xref> setter).

### <a id="Jumbee_Console_ControlFrame_Initialize"></a> Initialize\(\)

Re-lays-out the frame on the UI thread: recomputes the border offset and re-establishes the wrapped control's size limits and scroll position.

```csharp
protected override void Initialize()
```

### <a id="Jumbee_Console_ControlFrame_OnInput_Jumbee_Console_UI_InputEventArgs_"></a> OnInput\(InputEventArgs\)

Handles a UI input event: consumes the frame's own scroll keys, then tunnels the rest to the focused descendant (or a composite's navigation).

```csharp
public void OnInput(UI.InputEventArgs inputEventArgs)
```

#### Parameters

`inputEventArgs` [UI](Jumbee.Console.UI.md).[InputEventArgs](Jumbee.Console.UI.InputEventArgs.md)

### <a id="Jumbee_Console_ControlFrame_OnInput_ConsoleGUI_Input_InputEvent_"></a> OnInput\(InputEvent\)

Handles a keyboard input event, scrolling the frame when the scroll key is pressed and the frame can scroll in that direction.

```csharp
public void OnInput(InputEvent inputEvent)
```

#### Parameters

`inputEvent` InputEvent

### <a id="Jumbee_Console_ControlFrame_OnPaste_System_String_"></a> OnPaste\(string\)

Tunnels a bracketed-paste payload through the frame to the focused descendant.

```csharp
public void OnPaste(string text)
```

#### Parameters

`text` string

### <a id="Jumbee_Console_ControlFrame_Relayout"></a> Relayout\(\)

Re-runs the frame's child layout — re-reads the wrapped control's <xref href="Jumbee.Console.Control.FillsFrameViewport" data-throw-if-not-resolved="false"></xref> and
re-establishes its size limits.

```csharp
public void Relayout()
```

#### Remarks

Needed after a change that alters how the child should be sized but does not itself change the child's size
(so no redraw bubbles up to trigger a relayout): e.g. swapping a composite's content between a scrollable
control and a fill-to-viewport one.

### <a id="Jumbee_Console_ControlFrame_Scroll_System_Int32_"></a> Scroll\(int\)

Scrolls the frame's content by <code class="paramref">n</code> rows (negative up, positive down).

```csharp
public void Scroll(int n)
```

#### Parameters

`n` int

### <a id="Jumbee_Console_ControlFrame_OnFocus"></a> OnFocus

Raised when the frame gains focus.

```csharp
public event FocusableEventHandler? OnFocus
```

#### Event Type

 [FocusableEventHandler](Jumbee.Console.FocusableEventHandler.md)?

### <a id="Jumbee_Console_ControlFrame_OnLostFocus"></a> OnLostFocus

Raised when the frame loses focus.

```csharp
public event FocusableEventHandler? OnLostFocus
```

#### Event Type

 [FocusableEventHandler](Jumbee.Console.FocusableEventHandler.md)?

### <a id="Jumbee_Console_ControlFrame_Scrolled"></a> Scrolled

Raised after the vertical scroll position (<xref href="Jumbee.Console.ControlFrame.Top" data-throw-if-not-resolved="false"></xref>) changes.

```csharp
public event EventHandler? Scrolled
```

#### Event Type

 EventHandler?

