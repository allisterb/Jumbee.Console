# <a id="Jumbee_Console_CodeEditor"></a> Class CodeEditor

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

A composite control pairing a <xref href="Jumbee.Console.TextEditor" data-throw-if-not-resolved="false"></xref> with a <xref href="Jumbee.Console.LineNumberGutter" data-throw-if-not-resolved="false"></xref> docked to its left.

```csharp
public class CodeEditor : CompositeControl, IFocusable, IScrollable
```

#### Inheritance

object ← 
Control ← 
[Control](Jumbee.Console.Control.md) ← 
[CompositeControl](Jumbee.Console.CompositeControl.md) ← 
[CodeEditor](Jumbee.Console.CodeEditor.md)

#### Implements

[IFocusable](Jumbee.Console.IFocusable.md), 
[IScrollable](Jumbee.Console.IScrollable.md)

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

[ControlExtensions.WithAsciiBorder<CodeEditor\>\(CodeEditor, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithAsciiBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithBorder<CodeEditor\>\(CodeEditor, BorderStyle?, Color?, Color?, BorderPlacement?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_), 
[ControlExtensions.WithDoubleBorder<CodeEditor\>\(CodeEditor, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithDoubleBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithFrame<CodeEditor\>\(CodeEditor, ControlFrame\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_Jumbee\_Console\_ControlFrame\_), 
[ControlExtensions.WithFrame<CodeEditor\>\(CodeEditor, BorderStyle?, Offset?, Color?, Color?, string?, Color?, Color?, BorderPlacement?, BorderStyle?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_ConsoleGUI\_Space\_Offset\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_String\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_), 
[ControlExtensions.WithHeavyBorder<CodeEditor\>\(CodeEditor, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeavyBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithHeight<CodeEditor\>\(CodeEditor, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeight\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithMargin<CodeEditor\>\(CodeEditor, int, int, int, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_System\_Int32\_System\_Int32\_System\_Int32\_), 
[ControlExtensions.WithMargin<CodeEditor\>\(CodeEditor, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithNoBorder<CodeEditor\>\(CodeEditor\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithNoBorder\_\_1\_\_\_0\_), 
[ControlExtensions.WithRoundedBorder<CodeEditor\>\(CodeEditor, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithRoundedBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithScrollBarGlyphs<CodeEditor\>\(CodeEditor, ScrollBarGlyphs\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarGlyphs\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarGlyphs\_), 
[ControlExtensions.WithScrollBarStyle<CodeEditor\>\(CodeEditor, ScrollBarStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarStyle\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarStyle\_), 
[ControlExtensions.WithSize<CodeEditor\>\(CodeEditor, int?, int?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSize\_\_1\_\_\_0\_System\_Nullable\_System\_Int32\_\_System\_Nullable\_System\_Int32\_\_), 
[ControlExtensions.WithSquareBorder<CodeEditor\>\(CodeEditor, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSquareBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithTitle<CodeEditor\>\(CodeEditor, string\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_), 
[ControlExtensions.WithTitle<CodeEditor\>\(CodeEditor, string, TitleStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitleStyle\_), 
[ControlExtensions.WithTitle<CodeEditor\>\(CodeEditor, string, TitlePos, TitleBorderStyle, TitleColorStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitlePos\_Jumbee\_Console\_TitleBorderStyle\_Jumbee\_Console\_TitleColorStyle\_), 
[ControlExtensions.WithWidth<CodeEditor\>\(CodeEditor, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithWidth\_\_1\_\_\_0\_System\_Int32\_)

## Remarks

The gutter is kept in sync with the editor (line count + active line) by listening to the editor's
<xref href="Jumbee.Console.TextEditor.Changed" data-throw-if-not-resolved="false"></xref> event — the canonical "child controls react to each other" pattern that
<xref href="Jumbee.Console.CompositeControl" data-throw-if-not-resolved="false"></xref> exists to support.

<p>
The composite sizes to its content (the editor's wrapped row count), so wrapping it in a <xref href="Jumbee.Console.ControlFrame" data-throw-if-not-resolved="false"></xref>
(e.g. <code>codeEditor.WithFrame()</code>) scrolls the gutter and text together with an accurate scrollbar; the
editor's caret is kept in view by <xref href="Jumbee.Console.CodeEditor.AutoScroll" data-throw-if-not-resolved="false"></xref> driving that frame.
</p>

## Constructors

### <a id="Jumbee_Console_CodeEditor__ctor_Jumbee_Console_Language_"></a> CodeEditor\(Language\)

Creates a code editor highlighted for the given built-in <xref href="Jumbee.Console.Language" data-throw-if-not-resolved="false"></xref>.

```csharp
public CodeEditor(Language language = Language.None)
```

#### Parameters

`language` [Language](Jumbee.Console.Language.md)

### <a id="Jumbee_Console_CodeEditor__ctor_ColorCode_ILanguage_"></a> CodeEditor\(ILanguage\)

Creates an editor highlighted by a custom ColorCode grammar — for languages outside the built-in
    <xref href="Jumbee.Console.Language" data-throw-if-not-resolved="false"></xref> enum (e.g. a Mermaid grammar defined by another project).

```csharp
public CodeEditor(ILanguage customLanguage)
```

#### Parameters

`customLanguage` ILanguage

## Properties

### <a id="Jumbee_Console_CodeEditor_Editor"></a> Editor

The wrapped text editor (focus this to type; e.g. <code>UI.SetFocus(codeEditor.Editor)</code>).

```csharp
public TextEditor Editor { get; }
```

#### Property Value

 [TextEditor](Jumbee.Console.TextEditor.md)

### <a id="Jumbee_Console_CodeEditor_Gutter"></a> Gutter

The line-number gutter.

```csharp
public LineNumberGutter Gutter { get; }
```

#### Property Value

 [LineNumberGutter](Jumbee.Console.LineNumberGutter.md)

### <a id="Jumbee_Console_CodeEditor_ReadOnly"></a> ReadOnly

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the editor ignores edits (typing/Backspace/Delete/Enter/Tab/paste) but
    still navigates and scrolls — a read-only code viewer. Passthrough to <xref href="Jumbee.Console.TextEditor.ReadOnly" data-throw-if-not-resolved="false"></xref>.

```csharp
public bool ReadOnly { get; set; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_CodeEditor_Text"></a> Text

The editor's text. Setting it loads the document with the caret at the start (top of the file).

```csharp
public string Text { get; set; }
```

#### Property Value

 string

## Methods

### <a id="Jumbee_Console_CodeEditor_Control_OnInitialization"></a> Control\_OnInitialization\(\)

Sizes the internal content layout to fill the composite's current area after initialization.

```csharp
protected override void Control_OnInitialization()
```

### <a id="Jumbee_Console_CodeEditor_GetHelpInfo"></a> GetHelpInfo\(\)

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

### <a id="Jumbee_Console_CodeEditor_MeasureHeight_System_Int32_"></a> MeasureHeight\(int\)

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

### <a id="Jumbee_Console_CodeEditor_FocusRowChanged"></a> FocusRowChanged

Raised when the control's point of interest — a selected item, a caret — moves, carrying the content rows it
now occupies. The wrapping <xref href="Jumbee.Console.ControlFrame" data-throw-if-not-resolved="false"></xref> subscribes and scrolls them into view.

```csharp
public event EventHandler<RowSpan>? FocusRowChanged
```

#### Event Type

 EventHandler<[RowSpan](Jumbee.Console.RowSpan.md)\>?

#### Remarks

<p>
Declare it as a plain field-like event and raise it wherever the selection moves:

<pre><code class="lang-csharp">public event EventHandler&lt;RowSpan&gt;? FocusRowChanged;
private void Select(int i) { _index = i; FocusRowChanged?.Invoke(this, new RowSpan(RowOf(i))); }</code></pre>

Written that way the compiler reports <code>CS0067</code> if it is never raised, which is the mistake worth
catching: a control that says it has a moving selection and then leaves it to scroll off screen.
</p>
<p>
The default implementation does nothing, so a control with no moving point of interest — a document viewer,
a panel of static text — simply omits it. Scrolls that are not selection moves (following new output to the
bottom, restoring a saved position) are not what this is for; call
<xref href="Jumbee.Console.ControlFrame.ScrollIntoView(System.Int32%2cSystem.Int32)" data-throw-if-not-resolved="false"></xref> directly instead.
</p>

