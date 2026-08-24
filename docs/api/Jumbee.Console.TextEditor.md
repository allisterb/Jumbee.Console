# <a id="Jumbee_Console_TextEditor"></a> Class TextEditor

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

A text editor control with syntax highlighting for supported languages.

```csharp
public class TextEditor : Control, IFocusable, IScrollable
```

#### Inheritance

object ← 
Control ← 
[Control](Jumbee.Console.Control.md) ← 
[TextEditor](Jumbee.Console.TextEditor.md)

#### Implements

[IFocusable](Jumbee.Console.IFocusable.md), 
[IScrollable](Jumbee.Console.IScrollable.md)

#### Inherited Members

[Control.this\[Position\]](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Item\_ConsoleGUI\_Space\_Position\_), 
[Control.Width](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Width), 
[Control.ActualWidth](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_ActualWidth), 
[Control.Height](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Height), 
[Control.ActualHeight](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_ActualHeight), 
[Control.HasLayout](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_HasLayout), 
[Control.Frame](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Frame), 
[Control.HasFrame](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_HasFrame), 
[Control.Focusable](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Focusable), 
[Control.IsFocused](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_IsFocused), 
[Control.FocusableControl](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_FocusableControl), 
[Control.FocusedControl](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_FocusedControl), 
[Control.HandlesInput](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_HandlesInput), 
[Control.OnInput\(UI.InputEventArgs\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_OnInput\_Jumbee\_Console\_UI\_InputEventArgs\_), 
[Control.OnInput\(InputEvent\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_OnInput\_ConsoleGUI\_Input\_InputEvent\_), 
[Control.IsMouseOver](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_IsMouseOver), 
[Control.IsMousePressed](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_IsMousePressed), 
[Control.WantsMouse](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_WantsMouse), 
[Control.RendersOwnFocus](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_RendersOwnFocus), 
[Control.OnMouseEnter\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_OnMouseEnter), 
[Control.OnMouseLeave\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_OnMouseLeave), 
[Control.OnMouseMove\(Position\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_OnMouseMove\_ConsoleGUI\_Space\_Position\_), 
[Control.OnMousePress\(Position\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_OnMousePress\_ConsoleGUI\_Space\_Position\_), 
[Control.OnMouseRelease\(Position\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_OnMouseRelease\_ConsoleGUI\_Space\_Position\_), 
[Control.OnClick\(Position\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_OnClick\_ConsoleGUI\_Space\_Position\_), 
[Control.OnDoubleClick\(Position\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_OnDoubleClick\_ConsoleGUI\_Space\_Position\_), 
[Control.OnMouseWheel\(Position, int\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_OnMouseWheel\_ConsoleGUI\_Space\_Position\_System\_Int32\_), 
[Control.ScrollIntoView\(int, int\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_ScrollIntoView\_System\_Int32\_System\_Int32\_), 
[Control.CaptureMouse\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_CaptureMouse), 
[Control.ReleaseMouse\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_ReleaseMouse), 
[Control.OnPaste\(string\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_OnPaste\_System\_String\_), 
[Control.Dispose\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Dispose), 
[Control.ApplyTheme\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_ApplyTheme), 
[Control.IsThemeOverridden\(string\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_IsThemeOverridden\_System\_String\_), 
[Control.Focus\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Focus), 
[Control.UnFocus\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_UnFocus), 
[Control.GetHelpInfo\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_GetHelpInfo), 
[Control.CompileHelp\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_CompileHelp), 
[Control.Control\_OnInitialization\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Control\_OnInitialization), 
[Control.Control\_OnLostFocus\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Control\_OnLostFocus), 
[Control.Control\_OnFocus\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Control\_OnFocus), 
[Control.Render\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Render), 
[Control.Initialize\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Initialize), 
[Control.Paint\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Paint), 
[Control.Invalidate\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Invalidate), 
[Control.InvalidateInteractive\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_InvalidateInteractive), 
[Control.TracksDamage](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_TracksDamage), 
[Control.Damage\(in Rect\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Damage\_ConsoleGUI\_Space\_Rect\_\_), 
[Control.DamageAll\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_DamageAll), 
[Control.Feed\(Action, TimeSpan, Action<Exception\>?\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Feed\_System\_Action\_System\_TimeSpan\_System\_Action\_System\_Exception\_\_), 
[Control.Feed\(Action, int, Action<Exception\>?\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Feed\_System\_Action\_System\_Int32\_System\_Action\_System\_Exception\_\_), 
[Control.Feed<T\>\(Func<T\>, Action<T\>, TimeSpan, Action<Exception\>?\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Feed\_\_1\_System\_Func\_\_\_0\_\_System\_Action\_\_\_0\_\_System\_TimeSpan\_System\_Action\_System\_Exception\_\_), 
[Control.Feed<T\>\(Func<T\>, Action<T\>, int, Action<Exception\>?\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Feed\_\_1\_System\_Func\_\_\_0\_\_System\_Action\_\_\_0\_\_System\_Int32\_System\_Action\_System\_Exception\_\_), 
[Control.Job<T\>\(Func<T\>, Action<T\>, Action<Exception\>?\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Job\_\_1\_System\_Func\_\_\_0\_\_System\_Action\_\_\_0\_\_System\_Action\_System\_Exception\_\_), 
[Control.Feeds](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Feeds), 
[Control.SetAtomicProperty<T\>\(ref T, T, bool, Func<T, T\>?, Action<T, T\>?, bool, string?\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_SetAtomicProperty\_\_1\_\_\_0\_\_\_\_0\_System\_Boolean\_System\_Func\_\_\_0\_\_\_0\_\_System\_Action\_\_\_0\_\_\_0\_\_System\_Boolean\_System\_String\_), 
[Control.Validate\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Validate), 
[Control.CalculateSize\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_CalculateSize), 
[Control.IntrinsicWidth\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_IntrinsicWidth), 
[Control.IntrinsicHeight\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_IntrinsicHeight), 
[Control.ClampWidth\(int\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_ClampWidth\_System\_Int32\_), 
[Control.ClampHeight\(int\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_ClampHeight\_System\_Int32\_), 
[Control.OnInitialization](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_OnInitialization), 
[Control.OnFocus](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_OnFocus), 
[Control.OnLostFocus](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_OnLostFocus), 
[Control.OnHelp](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_OnHelp), 
[Control.MouseEntered](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_MouseEntered), 
[Control.MouseLeft](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_MouseLeft), 
[Control.MouseMoved](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_MouseMoved), 
[Control.MousePressed](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_MousePressed), 
[Control.MouseReleased](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_MouseReleased), 
[Control.Clicked](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Clicked), 
[Control.DoubleClicked](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_DoubleClicked), 
[Control.MouseWheeled](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_MouseWheeled), 
[Control.emptyChar](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_emptyChar), 
[Control.emptyCell](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_emptyCell), 
[Control.paintRequests](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_paintRequests), 
[Control.consoleBuffer](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_consoleBuffer), 
[Control.ansiConsole](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_ansiConsole), 
[Control.DoubleClickMs](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_DoubleClickMs)

#### Extension Methods

[ControlExtensions.WithAsciiBorder<TextEditor\>\(TextEditor, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithAsciiBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithBorder<TextEditor\>\(TextEditor, BorderStyle?, Color?, Color?, BorderPlacement?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_), 
[ControlExtensions.WithDoubleBorder<TextEditor\>\(TextEditor, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithDoubleBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithFrame<TextEditor\>\(TextEditor, ControlFrame\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_Jumbee\_Console\_ControlFrame\_), 
[ControlExtensions.WithFrame<TextEditor\>\(TextEditor, BorderStyle?, Offset?, Color?, Color?, string?, Color?, Color?, BorderPlacement?, BorderStyle?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_ConsoleGUI\_Space\_Offset\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_String\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_), 
[ControlExtensions.WithHeavyBorder<TextEditor\>\(TextEditor, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeavyBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithHeight<TextEditor\>\(TextEditor, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeight\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithMargin<TextEditor\>\(TextEditor, int, int, int, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_System\_Int32\_System\_Int32\_System\_Int32\_), 
[ControlExtensions.WithMargin<TextEditor\>\(TextEditor, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithNoBorder<TextEditor\>\(TextEditor\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithNoBorder\_\_1\_\_\_0\_), 
[ControlExtensions.WithRoundedBorder<TextEditor\>\(TextEditor, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithRoundedBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithScrollBarGlyphs<TextEditor\>\(TextEditor, ScrollBarGlyphs\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarGlyphs\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarGlyphs\_), 
[ControlExtensions.WithScrollBarStyle<TextEditor\>\(TextEditor, ScrollBarStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarStyle\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarStyle\_), 
[ControlExtensions.WithSize<TextEditor\>\(TextEditor, int?, int?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSize\_\_1\_\_\_0\_System\_Nullable\_System\_Int32\_\_System\_Nullable\_System\_Int32\_\_), 
[ControlExtensions.WithSquareBorder<TextEditor\>\(TextEditor, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSquareBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithTitle<TextEditor\>\(TextEditor, string\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_), 
[ControlExtensions.WithTitle<TextEditor\>\(TextEditor, string, TitleStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitleStyle\_), 
[ControlExtensions.WithTitle<TextEditor\>\(TextEditor, string, TitlePos, TitleBorderStyle, TitleColorStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitlePos\_Jumbee\_Console\_TitleBorderStyle\_Jumbee\_Console\_TitleColorStyle\_), 
[ControlExtensions.WithWidth<TextEditor\>\(TextEditor, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithWidth\_\_1\_\_\_0\_System\_Int32\_)

## Constructors

### <a id="Jumbee_Console_TextEditor__ctor_Jumbee_Console_Language_System_Boolean_System_Boolean_"></a> TextEditor\(Language, bool, bool\)

Initializes a new <xref href="Jumbee.Console.TextEditor" data-throw-if-not-resolved="false"></xref> highlighted for the given <code class="paramref">language</code>, with optional caret display and blink.

```csharp
public TextEditor(Language language = Language.None, bool showCursor = true, bool blinkCursor = false)
```

#### Parameters

`language` [Language](Jumbee.Console.Language.md)

`showCursor` bool

`blinkCursor` bool

### <a id="Jumbee_Console_TextEditor__ctor_ColorCode_ILanguage_System_Boolean_System_Boolean_"></a> TextEditor\(ILanguage, bool, bool\)

Creates an editor highlighted by a custom ColorCode grammar — for languages outside the built-in
    <xref href="Jumbee.Console.Language" data-throw-if-not-resolved="false"></xref> enum (e.g. a Mermaid grammar defined by another project).

```csharp
public TextEditor(ILanguage customLanguage, bool showCursor = true, bool blinkCursor = false)
```

#### Parameters

`customLanguage` ILanguage

`showCursor` bool

`blinkCursor` bool

#### Remarks

The grammar is applied by the same segment formatter/cache as the built-in languages.

## Properties

### <a id="Jumbee_Console_TextEditor_BlinkCursor"></a> BlinkCursor

Whether the caret blinks.

```csharp
public bool BlinkCursor { get; set; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_TextEditor_CaretIndex"></a> CaretIndex

The caret's index into the text, clamped to <code>0..Text.Length</code>. Setting it moves the caret and
    raises <xref href="Jumbee.Console.TextEditor.Changed" data-throw-if-not-resolved="false"></xref> (so adornments/auto-scroll follow); e.g. set to 0 to move to the start.

```csharp
public int CaretIndex { get; set; }
```

#### Property Value

 int

### <a id="Jumbee_Console_TextEditor_CaretLine"></a> CaretLine

The zero-based logical line the caret is on (newline count before it).

```csharp
public int CaretLine { get; }
```

#### Property Value

 int

### <a id="Jumbee_Console_TextEditor_CaretVisualRow"></a> CaretVisualRow

The zero-based visual (wrapped) row the caret is on.

```csharp
public int CaretVisualRow { get; }
```

#### Property Value

 int

### <a id="Jumbee_Console_TextEditor_CursorX"></a> CursorX

The caret's column within the viewport.

```csharp
public int CursorX { get; set; }
```

#### Property Value

 int

### <a id="Jumbee_Console_TextEditor_CursorY"></a> CursorY

The caret's row within the viewport.

```csharp
public int CursorY { get; set; }
```

#### Property Value

 int

### <a id="Jumbee_Console_TextEditor_HandlesInput"></a> HandlesInput

Reports <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> so input routing delivers keys to the control.

```csharp
public override bool HandlesInput { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_TextEditor_LineCount"></a> LineCount

The number of lines (newline count + 1).

```csharp
public int LineCount { get; }
```

#### Property Value

 int

### <a id="Jumbee_Console_TextEditor_ReadOnly"></a> ReadOnly

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, edit keys (typing, Backspace/Delete/Enter/Tab) and paste are ignored;
    navigation (arrows/Home/End/PgUp/PgDn) and the caret still work.

```csharp
public bool ReadOnly { get; set; }
```

#### Property Value

 bool

#### Remarks

Use for read-only viewers (e.g. a response body). Does not change appearance.

### <a id="Jumbee_Console_TextEditor_RendersOwnFocus"></a> RendersOwnFocus

Reports <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>: the caret indicates focus, so no default focus tint is drawn.

```csharp
protected override bool RendersOwnFocus { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_TextEditor_SelectedText"></a> SelectedText

The selected text, or empty when there is no selection.

```csharp
public string SelectedText { get; }
```

#### Property Value

 string

### <a id="Jumbee_Console_TextEditor_ShowCursor"></a> ShowCursor

Whether the caret is drawn.

```csharp
public bool ShowCursor { get; set; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_TextEditor_TabWidth"></a> TabWidth

The number of spaces inserted when the Tab key is pressed. Defaults to 4.

```csharp
public int TabWidth { get; set; }
```

#### Property Value

 int

### <a id="Jumbee_Console_TextEditor_Text"></a> Text

The full text content. Setting it replaces the buffer, moves the caret to the end, and raises
    <xref href="Jumbee.Console.TextEditor.Changed" data-throw-if-not-resolved="false"></xref>.

```csharp
public string Text { get; set; }
```

#### Property Value

 string

## Methods

### <a id="Jumbee_Console_TextEditor_ApplyTheme"></a> ApplyTheme\(\)

Re-captures this control's themed colours/glyphs from the current <xref href="Jumbee.Console.UI.StyleTheme" data-throw-if-not-resolved="false"></xref>/
<xref href="Jumbee.Console.UI.GlyphTheme" data-throw-if-not-resolved="false"></xref>. The default is a no-op for controls that don't use the theme.

```csharp
protected override void ApplyTheme()
```

#### Remarks

Called by themed controls from their constructor and again on a runtime theme switch (<xref href="Jumbee.Console.UI.SetTheme(Jumbee.Console.IStyleTheme%2cJumbee.Console.IGlyphTheme)" data-throw-if-not-resolved="false"></xref>).
Must read the themes <em>only here</em> (and in the constructor), never on the render path.

### <a id="Jumbee_Console_TextEditor_Control_OnInitialization"></a> Control\_OnInitialization\(\)

Fired when a control's Initialize method is called.

```csharp
protected override void Control_OnInitialization()
```

#### Remarks

This method is always called inside UI.Invoke.

### <a id="Jumbee_Console_TextEditor_GetHelpInfo"></a> GetHelpInfo\(\)

The help shown for this control in the global help dialog (F1), or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for no help.

```csharp
protected override HelpInfo? GetHelpInfo()
```

#### Returns

 [HelpInfo](Jumbee.Console.HelpInfo.md)?

#### Remarks

Override to describe the control and its keys. The result is deduplicated across the UI by
<xref href="Jumbee.Console.HelpInfo.Name" data-throw-if-not-resolved="false"></xref>, so give controls of the same kind the same name. <xref href="Jumbee.Console.Control.OnHelp" data-throw-if-not-resolved="false"></xref> handlers
can further modify (or create) it.

### <a id="Jumbee_Console_TextEditor_MeasureHeight_System_Int32_"></a> MeasureHeight\(int\)

The control's content height in rows at the given <code class="paramref">width</code> — the frame's scroll range.

```csharp
public virtual int MeasureHeight(int width)
```

#### Parameters

`width` int

#### Returns

 int

#### Remarks

Measure the content, not the viewport: a list returns its item count, a text control its wrapped row count.
Returning the visible height instead defeats the purpose, leaving a scrollbar that never moves.

### <a id="Jumbee_Console_TextEditor_OnInput_ConsoleGUI_Input_InputEvent_"></a> OnInput\(InputEvent\)

Handles a keyboard input event; override on input-handling controls. The default is a no-op.

```csharp
protected override void OnInput(InputEvent inputEvent)
```

#### Parameters

`inputEvent` InputEvent

### <a id="Jumbee_Console_TextEditor_OnPaste_System_String_"></a> OnPaste\(string\)

Inserts pasted <code class="paramref">text</code> at the caret in one shot, replacing any selection.

```csharp
public override void OnPaste(string text)
```

#### Parameters

`text` string

### <a id="Jumbee_Console_TextEditor_Render"></a> Render\(\)

Re-highlights the text when it or the selection changed, then draws the caret.

```csharp
protected override void Render()
```

### <a id="Jumbee_Console_TextEditor_RenderCursor"></a> RenderCursor\(\)

Positions and shows or hides the terminal caret according to the current focus and cursor settings.

```csharp
protected void RenderCursor()
```

### <a id="Jumbee_Console_TextEditor_SelectAll"></a> SelectAll\(\)

Selects the whole document (the same as Ctrl+A).

```csharp
public void SelectAll()
```

### <a id="Jumbee_Console_TextEditor_VisualLineNumbers"></a> VisualLineNumbers\(\)

For each visual (wrapped) row, the 1-based logical line number when the row starts a logical line, or 0
for a wrapped continuation row.

```csharp
public IReadOnlyList<int> VisualLineNumbers()
```

#### Returns

 IReadOnlyList<int\>

#### Remarks

A line-number gutter uses this to stay aligned with soft-wrapped text.

### <a id="Jumbee_Console_TextEditor_VisualRowCount_System_Int32_"></a> VisualRowCount\(int\)

The number of visual (wrapped) rows the text occupies at the given width — the editor's content
    height.

```csharp
public int VisualRowCount(int width)
```

#### Parameters

`width` int

#### Returns

 int

#### Remarks

A composite (e.g. <xref href="Jumbee.Console.CodeEditor" data-throw-if-not-resolved="false"></xref>) uses it to size/scroll itself around the editor.

### <a id="Jumbee_Console_TextEditor_Changed"></a> Changed

Raised after the text or caret position changes (typing, paste, delete, navigation, or setting
    <xref href="Jumbee.Console.TextEditor.Text" data-throw-if-not-resolved="false"></xref>).

```csharp
public event EventHandler? Changed
```

#### Event Type

 EventHandler?

#### Remarks

Composites use it to keep adornments — e.g. a line-number gutter — in sync.

