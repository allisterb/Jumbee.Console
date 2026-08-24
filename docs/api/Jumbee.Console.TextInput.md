# <a id="Jumbee_Console_TextInput"></a> Class TextInput

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

A single-line text entry control: caret, selection (Shift+navigation), horizontal scrolling when the text
exceeds the width, an optional muted placeholder shown while empty, and optional password masking.

```csharp
public class TextInput : Control, IFocusable
```

#### Inheritance

object ← 
Control ← 
[Control](Jumbee.Console.Control.md) ← 
[TextInput](Jumbee.Console.TextInput.md)

#### Implements

[IFocusable](Jumbee.Console.IFocusable.md)

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

[ControlExtensions.WithAsciiBorder<TextInput\>\(TextInput, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithAsciiBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithBorder<TextInput\>\(TextInput, BorderStyle?, Color?, Color?, BorderPlacement?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_), 
[ControlExtensions.WithDoubleBorder<TextInput\>\(TextInput, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithDoubleBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithFrame<TextInput\>\(TextInput, ControlFrame\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_Jumbee\_Console\_ControlFrame\_), 
[ControlExtensions.WithFrame<TextInput\>\(TextInput, BorderStyle?, Offset?, Color?, Color?, string?, Color?, Color?, BorderPlacement?, BorderStyle?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_ConsoleGUI\_Space\_Offset\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_String\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_), 
[ControlExtensions.WithHeavyBorder<TextInput\>\(TextInput, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeavyBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithHeight<TextInput\>\(TextInput, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeight\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithMargin<TextInput\>\(TextInput, int, int, int, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_System\_Int32\_System\_Int32\_System\_Int32\_), 
[ControlExtensions.WithMargin<TextInput\>\(TextInput, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithNoBorder<TextInput\>\(TextInput\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithNoBorder\_\_1\_\_\_0\_), 
[ControlExtensions.WithRoundedBorder<TextInput\>\(TextInput, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithRoundedBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithScrollBarGlyphs<TextInput\>\(TextInput, ScrollBarGlyphs\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarGlyphs\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarGlyphs\_), 
[ControlExtensions.WithScrollBarStyle<TextInput\>\(TextInput, ScrollBarStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarStyle\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarStyle\_), 
[ControlExtensions.WithSize<TextInput\>\(TextInput, int?, int?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSize\_\_1\_\_\_0\_System\_Nullable\_System\_Int32\_\_System\_Nullable\_System\_Int32\_\_), 
[ControlExtensions.WithSquareBorder<TextInput\>\(TextInput, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSquareBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithTitle<TextInput\>\(TextInput, string\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_), 
[ControlExtensions.WithTitle<TextInput\>\(TextInput, string, TitleStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitleStyle\_), 
[ControlExtensions.WithTitle<TextInput\>\(TextInput, string, TitlePos, TitleBorderStyle, TitleColorStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitlePos\_Jumbee\_Console\_TitleBorderStyle\_Jumbee\_Console\_TitleColorStyle\_), 
[ControlExtensions.WithWidth<TextInput\>\(TextInput, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithWidth\_\_1\_\_\_0\_System\_Int32\_)

## Remarks

Submitting with Enter raises <xref href="Jumbee.Console.TextInput.Submitted" data-throw-if-not-resolved="false"></xref>; any edit raises <xref href="Jumbee.Console.TextInput.Changed" data-throw-if-not-resolved="false"></xref>. The native terminal
cursor is owned while focused (only the focused control draws it), mirroring <xref href="Jumbee.Console.TextEditor" data-throw-if-not-resolved="false"></xref>.

## Constructors

### <a id="Jumbee_Console_TextInput__ctor_System_String_System_String_"></a> TextInput\(string, string\)

Initializes a new <xref href="Jumbee.Console.TextInput" data-throw-if-not-resolved="false"></xref> with the given initial <code class="paramref">text</code> and <code class="paramref">placeholder</code> hint.

```csharp
public TextInput(string text = "", string placeholder = "")
```

#### Parameters

`text` string

`placeholder` string

## Properties

### <a id="Jumbee_Console_TextInput_CaretIndex"></a> CaretIndex

The caret's index into <xref href="Jumbee.Console.TextInput.Text" data-throw-if-not-resolved="false"></xref> (0..length). Setting it clears any selection.

```csharp
public int CaretIndex { get; set; }
```

#### Property Value

 int

### <a id="Jumbee_Console_TextInput_HandlesInput"></a> HandlesInput

Reports <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> so input routing delivers keys to the control.

```csharp
public override bool HandlesInput { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_TextInput_KeyInterceptor"></a> KeyInterceptor

Optional first look at each key, before the field's own handling. Return <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> to consume
the key (the field ignores it).

```csharp
public Func<InputEvent, bool>? KeyInterceptor { get; set; }
```

#### Property Value

 Func<InputEvent, bool\>?

#### Remarks

An attached <xref href="Jumbee.Console.Autocomplete" data-throw-if-not-resolved="false"></xref> uses this to grab Up/Down/Enter/Esc for its suggestion popup
    while the field keeps focus and continues to edit on other keys.

### <a id="Jumbee_Console_TextInput_PasswordChar"></a> PasswordChar

When set, each character renders as this glyph (password entry); the real text is still in <xref href="Jumbee.Console.TextInput.Text" data-throw-if-not-resolved="false"></xref>.

```csharp
public char? PasswordChar { get; set; }
```

#### Property Value

 char?

### <a id="Jumbee_Console_TextInput_Placeholder"></a> Placeholder

Muted hint shown while the text is empty.

```csharp
public string Placeholder { get; set; }
```

#### Property Value

 string

### <a id="Jumbee_Console_TextInput_PlaceholderStyle"></a> PlaceholderStyle

Style of the placeholder hint. Defaults to <xref href="Jumbee.Console.IStyleTheme.TextMuted" data-throw-if-not-resolved="false"></xref>.

```csharp
public Style PlaceholderStyle { get; set; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_TextInput_ReadOnly"></a> ReadOnly

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, edits are ignored (caret/selection navigation still works).

```csharp
public bool ReadOnly { get; set; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_TextInput_RendersOwnFocus"></a> RendersOwnFocus

Reports <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>: the text cursor indicates focus, so no default focus tint is drawn.

```csharp
protected override bool RendersOwnFocus { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_TextInput_SelectedText"></a> SelectedText

The selected substring, or empty when there is no selection.

```csharp
public string SelectedText { get; }
```

#### Property Value

 string

### <a id="Jumbee_Console_TextInput_SelectionStyle"></a> SelectionStyle

Style of selected text. Defaults to <xref href="Jumbee.Console.IStyleTheme.Selection" data-throw-if-not-resolved="false"></xref>.

```csharp
public Style SelectionStyle { get; set; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_TextInput_Text"></a> Text

The entered text (newlines stripped). Setting it moves the caret to the end and raises <xref href="Jumbee.Console.TextInput.Changed" data-throw-if-not-resolved="false"></xref>.

```csharp
public string Text { get; set; }
```

#### Property Value

 string

### <a id="Jumbee_Console_TextInput_TextStyle"></a> TextStyle

Style of the entered text. Defaults to <xref href="Jumbee.Console.IStyleTheme.Text" data-throw-if-not-resolved="false"></xref>.

```csharp
public Style TextStyle { get; set; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

## Methods

### <a id="Jumbee_Console_TextInput_ApplyTheme"></a> ApplyTheme\(\)

Re-captures this control's themed colours/glyphs from the current <xref href="Jumbee.Console.UI.StyleTheme" data-throw-if-not-resolved="false"></xref>/
<xref href="Jumbee.Console.UI.GlyphTheme" data-throw-if-not-resolved="false"></xref>. The default is a no-op for controls that don't use the theme.

```csharp
protected override void ApplyTheme()
```

#### Remarks

Called by themed controls from their constructor and again on a runtime theme switch (<xref href="Jumbee.Console.UI.SetTheme(Jumbee.Console.IStyleTheme%2cJumbee.Console.IGlyphTheme)" data-throw-if-not-resolved="false"></xref>).
Must read the themes <em>only here</em> (and in the constructor), never on the render path.

### <a id="Jumbee_Console_TextInput_GetHelpInfo"></a> GetHelpInfo\(\)

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

### <a id="Jumbee_Console_TextInput_IntrinsicHeight"></a> IntrinsicHeight\(\)

The input's fixed height of one row.

```csharp
protected override int IntrinsicHeight()
```

#### Returns

 int

### <a id="Jumbee_Console_TextInput_IntrinsicWidth"></a> IntrinsicWidth\(\)

Returns 0 so the input fills the available width.

```csharp
protected override int IntrinsicWidth()
```

#### Returns

 int

### <a id="Jumbee_Console_TextInput_OnInput_ConsoleGUI_Input_InputEvent_"></a> OnInput\(InputEvent\)

Handles a keyboard input event; override on input-handling controls. The default is a no-op.

```csharp
protected override void OnInput(InputEvent inputEvent)
```

#### Parameters

`inputEvent` InputEvent

### <a id="Jumbee_Console_TextInput_OnPaste_System_String_"></a> OnPaste\(string\)

Inserts pasted <code class="paramref">text</code> (flattened to a single line) at the caret, replacing any selection.

```csharp
public override void OnPaste(string text)
```

#### Parameters

`text` string

### <a id="Jumbee_Console_TextInput_Render"></a> Render\(\)

Renders the (scrolled) text or placeholder, highlighting any selection.

```csharp
protected override void Render()
```

### <a id="Jumbee_Console_TextInput_Changed"></a> Changed

Raised whenever the text changes (typing, deletion, paste, or the <xref href="Jumbee.Console.TextInput.Text" data-throw-if-not-resolved="false"></xref> setter).

```csharp
public event EventHandler? Changed
```

#### Event Type

 EventHandler?

### <a id="Jumbee_Console_TextInput_Submitted"></a> Submitted

Raised when Enter is pressed. Read <xref href="Jumbee.Console.TextInput.Text" data-throw-if-not-resolved="false"></xref> for the value.

```csharp
public event EventHandler? Submitted
```

#### Event Type

 EventHandler?

