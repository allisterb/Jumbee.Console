# <a id="Jumbee_Console_InteractiveMarkdownEditor"></a> Class InteractiveMarkdownEditor

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

A live, split-pane Markdown editor for the terminal — the TUI equivalent of a web Markdown editor. The left pane
is a <xref href="Jumbee.Console.CodeEditor" data-throw-if-not-resolved="false"></xref> with Markdown syntax highlighting; the right pane is a <xref href="Jumbee.Console.MarkdownViewer" data-throw-if-not-resolved="false"></xref>
that re-renders the document — headings, tables and syntax-highlighted code — as you type (see
<xref href="Jumbee.Console.InteractiveSourceEditor" data-throw-if-not-resolved="false"></xref> for the sync model).

```csharp
public class InteractiveMarkdownEditor : InteractiveSourceEditor, IFocusable
```

#### Inheritance

object ← 
Control ← 
[Control](Jumbee.Console.Control.md) ← 
[CompositeControl](Jumbee.Console.CompositeControl.md) ← 
[InteractiveSourceEditor](Jumbee.Console.InteractiveSourceEditor.md) ← 
[InteractiveMarkdownEditor](Jumbee.Console.InteractiveMarkdownEditor.md)

#### Implements

[IFocusable](Jumbee.Console.IFocusable.md)

#### Inherited Members

[InteractiveSourceEditor.TextChanged](Jumbee.Console.InteractiveSourceEditor.md\#Jumbee\_Console\_InteractiveSourceEditor\_TextChanged), 
[InteractiveSourceEditor.Editor](Jumbee.Console.InteractiveSourceEditor.md\#Jumbee\_Console\_InteractiveSourceEditor\_Editor), 
[InteractiveSourceEditor.Split](Jumbee.Console.InteractiveSourceEditor.md\#Jumbee\_Console\_InteractiveSourceEditor\_Split), 
[InteractiveSourceEditor.PreviewControl](Jumbee.Console.InteractiveSourceEditor.md\#Jumbee\_Console\_InteractiveSourceEditor\_PreviewControl), 
[InteractiveSourceEditor.Text](Jumbee.Console.InteractiveSourceEditor.md\#Jumbee\_Console\_InteractiveSourceEditor\_Text), 
[InteractiveSourceEditor.ApplyPreviewText\(string\)](Jumbee.Console.InteractiveSourceEditor.md\#Jumbee\_Console\_InteractiveSourceEditor\_ApplyPreviewText\_System\_String\_), 
[InteractiveSourceEditor.FocusChild](Jumbee.Console.InteractiveSourceEditor.md\#Jumbee\_Console\_InteractiveSourceEditor\_FocusChild), 
[InteractiveSourceEditor.GetHelpInfo\(\)](Jumbee.Console.InteractiveSourceEditor.md\#Jumbee\_Console\_InteractiveSourceEditor\_GetHelpInfo), 
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

[ControlExtensions.WithAsciiBorder<InteractiveMarkdownEditor\>\(InteractiveMarkdownEditor, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithAsciiBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithBorder<InteractiveMarkdownEditor\>\(InteractiveMarkdownEditor, BorderStyle?, Color?, Color?, BorderPlacement?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_), 
[ControlExtensions.WithDoubleBorder<InteractiveMarkdownEditor\>\(InteractiveMarkdownEditor, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithDoubleBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithFrame<InteractiveMarkdownEditor\>\(InteractiveMarkdownEditor, ControlFrame\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_Jumbee\_Console\_ControlFrame\_), 
[ControlExtensions.WithFrame<InteractiveMarkdownEditor\>\(InteractiveMarkdownEditor, BorderStyle?, Offset?, Color?, Color?, string?, Color?, Color?, BorderPlacement?, BorderStyle?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_ConsoleGUI\_Space\_Offset\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_String\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_), 
[ControlExtensions.WithHeavyBorder<InteractiveMarkdownEditor\>\(InteractiveMarkdownEditor, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeavyBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithHeight<InteractiveMarkdownEditor\>\(InteractiveMarkdownEditor, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeight\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithMargin<InteractiveMarkdownEditor\>\(InteractiveMarkdownEditor, int, int, int, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_System\_Int32\_System\_Int32\_System\_Int32\_), 
[ControlExtensions.WithMargin<InteractiveMarkdownEditor\>\(InteractiveMarkdownEditor, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithNoBorder<InteractiveMarkdownEditor\>\(InteractiveMarkdownEditor\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithNoBorder\_\_1\_\_\_0\_), 
[ControlExtensions.WithRoundedBorder<InteractiveMarkdownEditor\>\(InteractiveMarkdownEditor, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithRoundedBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithScrollBarGlyphs<InteractiveMarkdownEditor\>\(InteractiveMarkdownEditor, ScrollBarGlyphs\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarGlyphs\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarGlyphs\_), 
[ControlExtensions.WithScrollBarStyle<InteractiveMarkdownEditor\>\(InteractiveMarkdownEditor, ScrollBarStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarStyle\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarStyle\_), 
[ControlExtensions.WithSize<InteractiveMarkdownEditor\>\(InteractiveMarkdownEditor, int?, int?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSize\_\_1\_\_\_0\_System\_Nullable\_System\_Int32\_\_System\_Nullable\_System\_Int32\_\_), 
[ControlExtensions.WithSquareBorder<InteractiveMarkdownEditor\>\(InteractiveMarkdownEditor, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSquareBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithTitle<InteractiveMarkdownEditor\>\(InteractiveMarkdownEditor, string\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_), 
[ControlExtensions.WithTitle<InteractiveMarkdownEditor\>\(InteractiveMarkdownEditor, string, TitleStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitleStyle\_), 
[ControlExtensions.WithTitle<InteractiveMarkdownEditor\>\(InteractiveMarkdownEditor, string, TitlePos, TitleBorderStyle, TitleColorStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitlePos\_Jumbee\_Console\_TitleBorderStyle\_Jumbee\_Console\_TitleColorStyle\_), 
[ControlExtensions.WithWidth<InteractiveMarkdownEditor\>\(InteractiveMarkdownEditor, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithWidth\_\_1\_\_\_0\_System\_Int32\_)

## Remarks

To extend the preview (e.g. rendering embedded diagrams), subclass and pass a richer <xref href="Jumbee.Console.MarkdownViewer" data-throw-if-not-resolved="false"></xref>
to the <xref href="Jumbee.Console.InteractiveMarkdownEditor.%23ctor(Jumbee.Console.MarkdownViewer%2cSystem.String%2cJumbee.Console.SplitOrientation%2cSystem.Int32)?text=protected+constructor" data-throw-if-not-resolved="false"></xref>.

## Constructors

### <a id="Jumbee_Console_InteractiveMarkdownEditor__ctor_System_String_Jumbee_Console_SplitOrientation_System_Int32_"></a> InteractiveMarkdownEditor\(string, SplitOrientation, int\)

```csharp
public InteractiveMarkdownEditor(string markdown = "", SplitOrientation orientation = SplitOrientation.Horizontal, int splitPosition = 48)
```

#### Parameters

`markdown` string

Initial document text (both panes start in sync).

`orientation` [SplitOrientation](Jumbee.Console.SplitOrientation.md)

Side-by-side (<xref href="Jumbee.Console.SplitOrientation.Horizontal" data-throw-if-not-resolved="false"></xref>, editor left) or stacked
    (<xref href="Jumbee.Console.SplitOrientation.Vertical" data-throw-if-not-resolved="false"></xref>, editor on top).

`splitPosition` int

The editor pane's initial extent in cells (width when horizontal, height when
    vertical). Adjustable at runtime via the divider.

### <a id="Jumbee_Console_InteractiveMarkdownEditor__ctor_Jumbee_Console_MarkdownViewer_System_String_Jumbee_Console_SplitOrientation_System_Int32_"></a> InteractiveMarkdownEditor\(MarkdownViewer, string, SplitOrientation, int\)

For subclasses that supply a richer preview (e.g. a mermaid-aware viewer). The
    <code class="paramref">preview</code> must already hold <code class="paramref">markdown</code> so both panes start in sync.

```csharp
protected InteractiveMarkdownEditor(MarkdownViewer preview, string markdown, SplitOrientation orientation, int splitPosition)
```

#### Parameters

`preview` [MarkdownViewer](Jumbee.Console.MarkdownViewer.md)

`markdown` string

`orientation` [SplitOrientation](Jumbee.Console.SplitOrientation.md)

`splitPosition` int

## Properties

### <a id="Jumbee_Console_InteractiveMarkdownEditor_Preview"></a> Preview

The preview pane rendering the live Markdown.

```csharp
public MarkdownViewer Preview { get; }
```

#### Property Value

 [MarkdownViewer](Jumbee.Console.MarkdownViewer.md)

### <a id="Jumbee_Console_InteractiveMarkdownEditor_PreviewStyles"></a> PreviewStyles

Render styles (heading / code / table colours) for the preview pane.

```csharp
public MarkdownStyles? PreviewStyles { get; set; }
```

#### Property Value

 MarkdownStyles?

## Methods

### <a id="Jumbee_Console_InteractiveMarkdownEditor_ApplyPreviewText_System_String_"></a> ApplyPreviewText\(string\)

Push the editor's current <code class="paramref">text</code> into the preview control (e.g.
    <code>preview.Markdown = text</code>). Called on the UI thread, coalesced per frame, only when the text changed.

```csharp
protected override void ApplyPreviewText(string text)
```

#### Parameters

`text` string

### <a id="Jumbee_Console_InteractiveMarkdownEditor_GetHelpInfo"></a> GetHelpInfo\(\)

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

