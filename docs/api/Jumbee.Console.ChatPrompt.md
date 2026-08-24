# <a id="Jumbee_Console_ChatPrompt"></a> Class ChatPrompt

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

The input area of an agent/chat CLI (Claude Code, Gemini CLI): a prompt glyph on the left that turns into an
animated <em>busy</em> spinner while an operation runs, and a single-line <xref href="Jumbee.Console.TextInput" data-throw-if-not-resolved="false"></xref> filling the
rest. Submitting (Enter) raises <xref href="Jumbee.Console.ChatPrompt.Submitted" data-throw-if-not-resolved="false"></xref>; any edit raises <xref href="Jumbee.Console.ChatPrompt.Changed" data-throw-if-not-resolved="false"></xref>.

```csharp
public class ChatPrompt : CompositeControl, IFocusable
```

#### Inheritance

object ← 
Control ← 
[Control](Jumbee.Console.Control.md) ← 
[CompositeControl](Jumbee.Console.CompositeControl.md) ← 
[ChatPrompt](Jumbee.Console.ChatPrompt.md)

#### Implements

[IFocusable](Jumbee.Console.IFocusable.md)

#### Inherited Members

[CompositeControl.Content](Jumbee.Console.CompositeControl.md\#Jumbee\_Console\_CompositeControl\_Content), 
[CompositeControl.FocusedControl](Jumbee.Console.CompositeControl.md\#Jumbee\_Console\_CompositeControl\_FocusedControl), 
[CompositeControl.HandlesInput](Jumbee.Console.CompositeControl.md\#Jumbee\_Console\_CompositeControl\_HandlesInput), 
[CompositeControl.FocusChild](Jumbee.Console.CompositeControl.md\#Jumbee\_Console\_CompositeControl\_FocusChild), 
[CompositeControl.Focusables](Jumbee.Console.CompositeControl.md\#Jumbee\_Console\_CompositeControl\_Focusables), 
[CompositeControl.TabNavigatesChildren](Jumbee.Console.CompositeControl.md\#Jumbee\_Console\_CompositeControl\_TabNavigatesChildren), 
[CompositeControl.this\[Position\]](Jumbee.Console.CompositeControl.md\#Jumbee\_Console\_CompositeControl\_Item\_ConsoleGUI\_Space\_Position\_), 
[CompositeControl.SetContent\(ILayout\)](Jumbee.Console.CompositeControl.md\#Jumbee\_Console\_CompositeControl\_SetContent\_Jumbee\_Console\_ILayout\_), 
[CompositeControl.Control\_OnFocus\(\)](Jumbee.Console.CompositeControl.md\#Jumbee\_Console\_CompositeControl\_Control\_OnFocus), 
[CompositeControl.Control\_OnLostFocus\(\)](Jumbee.Console.CompositeControl.md\#Jumbee\_Console\_CompositeControl\_Control\_OnLostFocus), 
[CompositeControl.InterceptInput\(UI.InputEventArgs\)](Jumbee.Console.CompositeControl.md\#Jumbee\_Console\_CompositeControl\_InterceptInput\_Jumbee\_Console\_UI\_InputEventArgs\_), 
[CompositeControl.MoveFocusToChild\(int\)](Jumbee.Console.CompositeControl.md\#Jumbee\_Console\_CompositeControl\_MoveFocusToChild\_System\_Int32\_), 
[CompositeControl.Render\(\)](Jumbee.Console.CompositeControl.md\#Jumbee\_Console\_CompositeControl\_Render), 
[CompositeControl.Control\_OnInitialization\(\)](Jumbee.Console.CompositeControl.md\#Jumbee\_Console\_CompositeControl\_Control\_OnInitialization), 
[CompositeControl.Dispose\(\)](Jumbee.Console.CompositeControl.md\#Jumbee\_Console\_CompositeControl\_Dispose), 
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

[ControlExtensions.WithAsciiBorder<ChatPrompt\>\(ChatPrompt, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithAsciiBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithBorder<ChatPrompt\>\(ChatPrompt, BorderStyle?, Color?, Color?, BorderPlacement?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_), 
[ControlExtensions.WithDoubleBorder<ChatPrompt\>\(ChatPrompt, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithDoubleBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithFrame<ChatPrompt\>\(ChatPrompt, ControlFrame\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_Jumbee\_Console\_ControlFrame\_), 
[ControlExtensions.WithFrame<ChatPrompt\>\(ChatPrompt, BorderStyle?, Offset?, Color?, Color?, string?, Color?, Color?, BorderPlacement?, BorderStyle?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_ConsoleGUI\_Space\_Offset\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_String\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_), 
[ControlExtensions.WithHeavyBorder<ChatPrompt\>\(ChatPrompt, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeavyBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithHeight<ChatPrompt\>\(ChatPrompt, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeight\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithMargin<ChatPrompt\>\(ChatPrompt, int, int, int, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_System\_Int32\_System\_Int32\_System\_Int32\_), 
[ControlExtensions.WithMargin<ChatPrompt\>\(ChatPrompt, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithNoBorder<ChatPrompt\>\(ChatPrompt\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithNoBorder\_\_1\_\_\_0\_), 
[ControlExtensions.WithRoundedBorder<ChatPrompt\>\(ChatPrompt, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithRoundedBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithScrollBarGlyphs<ChatPrompt\>\(ChatPrompt, ScrollBarGlyphs\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarGlyphs\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarGlyphs\_), 
[ControlExtensions.WithScrollBarStyle<ChatPrompt\>\(ChatPrompt, ScrollBarStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarStyle\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarStyle\_), 
[ControlExtensions.WithSize<ChatPrompt\>\(ChatPrompt, int?, int?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSize\_\_1\_\_\_0\_System\_Nullable\_System\_Int32\_\_System\_Nullable\_System\_Int32\_\_), 
[ControlExtensions.WithSquareBorder<ChatPrompt\>\(ChatPrompt, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSquareBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithTitle<ChatPrompt\>\(ChatPrompt, string\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_), 
[ControlExtensions.WithTitle<ChatPrompt\>\(ChatPrompt, string, TitleStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitleStyle\_), 
[ControlExtensions.WithTitle<ChatPrompt\>\(ChatPrompt, string, TitlePos, TitleBorderStyle, TitleColorStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitlePos\_Jumbee\_Console\_TitleBorderStyle\_Jumbee\_Console\_TitleColorStyle\_), 
[ControlExtensions.WithWidth<ChatPrompt\>\(ChatPrompt, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithWidth\_\_1\_\_\_0\_System\_Int32\_)

## Remarks

Optional type-ahead is attached with <xref href="Jumbee.Console.ChatPrompt.WithSuggestions(System.String%5b%5d)" data-throw-if-not-resolved="false"></xref>. Built as a
<xref href="Jumbee.Console.CompositeControl" data-throw-if-not-resolved="false"></xref> — a <xref href="Jumbee.Console.PromptGutter" data-throw-if-not-resolved="false"></xref> docked left of the input — so it drops into any
layout cell and is framed like any control (<code>chat.WithRoundedBorder()</code>). Focus delegates to the input,
which keeps the caret; the gutter is a non-focusable adornment.

## Constructors

### <a id="Jumbee_Console_ChatPrompt__ctor_System_String_"></a> ChatPrompt\(string\)

Initializes a new <xref href="Jumbee.Console.ChatPrompt" data-throw-if-not-resolved="false"></xref> with an optional <code class="paramref">placeholder</code> hint.

```csharp
public ChatPrompt(string placeholder = "")
```

#### Parameters

`placeholder` string

## Properties

### <a id="Jumbee_Console_ChatPrompt_Busy"></a> Busy

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the gutter animates a spinner to signal a running operation; when
    <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a> it shows the static prompt glyph. The field stays editable either way (queue input
    while busy, like Claude Code) — set <xref href="Jumbee.Console.ChatPrompt.ReadOnly" data-throw-if-not-resolved="false"></xref> if that's not wanted.

```csharp
public bool Busy { get; set; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_ChatPrompt_Input"></a> Input

The wrapped text field (focus it to type; the composite delegates focus here).

```csharp
public TextInput Input { get; }
```

#### Property Value

 [TextInput](Jumbee.Console.TextInput.md)

### <a id="Jumbee_Console_ChatPrompt_Placeholder"></a> Placeholder

Muted hint shown while the field is empty.

```csharp
public string Placeholder { get; set; }
```

#### Property Value

 string

### <a id="Jumbee_Console_ChatPrompt_Prompt"></a> Prompt

The prompt glyph shown left of the input when idle (default <code>❯</code>). A trailing space separates it
    from the text, so a longer prompt widens the gutter.

```csharp
public string Prompt { get; set; }
```

#### Property Value

 string

### <a id="Jumbee_Console_ChatPrompt_PromptStyle"></a> PromptStyle

Style of the prompt glyph / spinner. Defaults to <xref href="Jumbee.Console.IStyleTheme.TextAccent" data-throw-if-not-resolved="false"></xref>.

```csharp
public Style PromptStyle { get; set; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_ChatPrompt_ReadOnly"></a> ReadOnly

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, edits are ignored (caret navigation still works). Set this while an
    operation runs if the field should not accept input.

```csharp
public bool ReadOnly { get; set; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_ChatPrompt_Spinner"></a> Spinner

The spinner shown in place of the prompt glyph while <xref href="Jumbee.Console.ChatPrompt.Busy" data-throw-if-not-resolved="false"></xref> (default
    <xref href="Spectre.Console.Spinner.Known.Dots" data-throw-if-not-resolved="false"></xref>).

```csharp
public Spinner Spinner { get; set; }
```

#### Property Value

 Spinner

### <a id="Jumbee_Console_ChatPrompt_Text"></a> Text

The entered text. Setting it moves the caret to the end and raises <xref href="Jumbee.Console.ChatPrompt.Changed" data-throw-if-not-resolved="false"></xref>.

```csharp
public string Text { get; set; }
```

#### Property Value

 string

## Methods

### <a id="Jumbee_Console_ChatPrompt_GetHelpInfo"></a> GetHelpInfo\(\)

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

### <a id="Jumbee_Console_ChatPrompt_IntrinsicHeight"></a> IntrinsicHeight\(\)

The intrinsic, fixed height counterpart of <xref href="Jumbee.Console.Control.IntrinsicWidth" data-throw-if-not-resolved="false"></xref> (e.g. a horizontal
    <xref href="Jumbee.Console.TextLabel" data-throw-if-not-resolved="false"></xref> is one row tall). Returns 0 to fill the parent's height (the default).

```csharp
protected override int IntrinsicHeight()
```

#### Returns

 int

### <a id="Jumbee_Console_ChatPrompt_WithSuggestions_System_String___"></a> WithSuggestions\(params string\[\]\)

Attaches type-ahead suggestions from a fixed candidate list (returns the <xref href="Jumbee.Console.Autocomplete" data-throw-if-not-resolved="false"></xref>
    for further tuning, e.g. <xref href="Jumbee.Console.Autocomplete.MaxRows" data-throw-if-not-resolved="false"></xref>).

```csharp
public Autocomplete WithSuggestions(params string[] candidates)
```

#### Parameters

`candidates` string\[\]

#### Returns

 [Autocomplete](Jumbee.Console.Autocomplete.md)

### <a id="Jumbee_Console_ChatPrompt_WithSuggestions_System_Func_System_String_System_Collections_Generic_IEnumerable_System_String___"></a> WithSuggestions\(Func<string, IEnumerable<string\>\>\)

Attaches type-ahead suggestions from a provider called with the current text.

```csharp
public Autocomplete WithSuggestions(Func<string, IEnumerable<string>> suggest)
```

#### Parameters

`suggest` Func<string, IEnumerable<string\>\>

#### Returns

 [Autocomplete](Jumbee.Console.Autocomplete.md)

### <a id="Jumbee_Console_ChatPrompt_Changed"></a> Changed

Raised whenever the text changes.

```csharp
public event EventHandler? Changed
```

#### Event Type

 EventHandler?

### <a id="Jumbee_Console_ChatPrompt_Submitted"></a> Submitted

Raised when Enter is pressed. The argument is the submitted <xref href="Jumbee.Console.ChatPrompt.Text" data-throw-if-not-resolved="false"></xref>.

```csharp
public event EventHandler<string>? Submitted
```

#### Event Type

 EventHandler<string\>?

