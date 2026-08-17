# <a id="Jumbee_Console_MultiTabCodeEditor"></a> Class MultiTabCodeEditor

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

A tabbed group of <xref href="Jumbee.Console.CodeEditor" data-throw-if-not-resolved="false"></xref>s — a VS-Code-style editor area. Each open document is a closable tab
(click the ✕ on the active/hovered tab), and a "+" button at the end of the bar opens a new document.

```csharp
public class MultiTabCodeEditor : CompositeControl, IFocusable
```

#### Inheritance

object ← 
Control ← 
[Control](Jumbee.Console.Control.md) ← 
[CompositeControl](Jumbee.Console.CompositeControl.md) ← 
[MultiTabCodeEditor](Jumbee.Console.MultiTabCodeEditor.md)

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

[ControlExtensions.WithAsciiBorder<MultiTabCodeEditor\>\(MultiTabCodeEditor, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithAsciiBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithBorder<MultiTabCodeEditor\>\(MultiTabCodeEditor, BorderStyle?, Color?, Color?, BorderPlacement?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_), 
[ControlExtensions.WithDoubleBorder<MultiTabCodeEditor\>\(MultiTabCodeEditor, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithDoubleBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithFrame<MultiTabCodeEditor\>\(MultiTabCodeEditor, ControlFrame\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_Jumbee\_Console\_ControlFrame\_), 
[ControlExtensions.WithFrame<MultiTabCodeEditor\>\(MultiTabCodeEditor, BorderStyle?, Offset?, Color?, Color?, string?, Color?, Color?, BorderPlacement?, BorderStyle?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_ConsoleGUI\_Space\_Offset\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_String\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_), 
[ControlExtensions.WithHeavyBorder<MultiTabCodeEditor\>\(MultiTabCodeEditor, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeavyBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithHeight<MultiTabCodeEditor\>\(MultiTabCodeEditor, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeight\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithMargin<MultiTabCodeEditor\>\(MultiTabCodeEditor, int, int, int, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_System\_Int32\_System\_Int32\_System\_Int32\_), 
[ControlExtensions.WithMargin<MultiTabCodeEditor\>\(MultiTabCodeEditor, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithNoBorder<MultiTabCodeEditor\>\(MultiTabCodeEditor\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithNoBorder\_\_1\_\_\_0\_), 
[ControlExtensions.WithRoundedBorder<MultiTabCodeEditor\>\(MultiTabCodeEditor, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithRoundedBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithScrollBarGlyphs<MultiTabCodeEditor\>\(MultiTabCodeEditor, ScrollBarGlyphs\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarGlyphs\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarGlyphs\_), 
[ControlExtensions.WithScrollBarStyle<MultiTabCodeEditor\>\(MultiTabCodeEditor, ScrollBarStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarStyle\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarStyle\_), 
[ControlExtensions.WithSize<MultiTabCodeEditor\>\(MultiTabCodeEditor, int?, int?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSize\_\_1\_\_\_0\_System\_Nullable\_System\_Int32\_\_System\_Nullable\_System\_Int32\_\_), 
[ControlExtensions.WithSquareBorder<MultiTabCodeEditor\>\(MultiTabCodeEditor, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSquareBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithTitle<MultiTabCodeEditor\>\(MultiTabCodeEditor, string\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_), 
[ControlExtensions.WithTitle<MultiTabCodeEditor\>\(MultiTabCodeEditor, string, TitleStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitleStyle\_), 
[ControlExtensions.WithTitle<MultiTabCodeEditor\>\(MultiTabCodeEditor, string, TitlePos, TitleBorderStyle, TitleColorStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitlePos\_Jumbee\_Console\_TitleBorderStyle\_Jumbee\_Console\_TitleColorStyle\_), 
[ControlExtensions.WithWidth<MultiTabCodeEditor\>\(MultiTabCodeEditor, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithWidth\_\_1\_\_\_0\_System\_Int32\_)

## Remarks

Built on a top-docked <xref href="Jumbee.Console.TabPanel" data-throw-if-not-resolved="false"></xref>; each editor is wrapped in its own frame so it scrolls independently.
Switch tabs with Alt+←/→ or by clicking a tab.

## Constructors

### <a id="Jumbee_Console_MultiTabCodeEditor__ctor_Jumbee_Console_Language_"></a> MultiTabCodeEditor\(Language\)

Initializes an empty editor group whose new documents default to <code class="paramref">defaultLanguage</code>.

```csharp
public MultiTabCodeEditor(Language defaultLanguage = Language.None)
```

#### Parameters

`defaultLanguage` [Language](Jumbee.Console.Language.md)

## Properties

### <a id="Jumbee_Console_MultiTabCodeEditor_ActiveDocumentName"></a> ActiveDocumentName

The selected document's name (tab label), or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> when none are open.

```csharp
public string? ActiveDocumentName { get; }
```

#### Property Value

 string?

### <a id="Jumbee_Console_MultiTabCodeEditor_ActiveEditor"></a> ActiveEditor

The selected document's editor, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> when none are open.

```csharp
public CodeEditor? ActiveEditor { get; }
```

#### Property Value

 [CodeEditor](Jumbee.Console.CodeEditor.md)?

### <a id="Jumbee_Console_MultiTabCodeEditor_ConfirmOnClose"></a> ConfirmOnClose

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, closing a document with unsaved changes (see <xref href="Jumbee.Console.MultiTabCodeEditor.IsDirty(Jumbee.Console.CodeEditor)" data-throw-if-not-resolved="false"></xref>)
    first shows a modal "Discard changes?" confirmation and only closes on confirm. Default
    <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>.

```csharp
public bool ConfirmOnClose { get; set; }
```

#### Property Value

 bool

#### Remarks

Requires an ambient <xref href="Jumbee.Console.UI.Overlay" data-throw-if-not-resolved="false"></xref> (present after <xref href="Jumbee.Console.UI.Start(Jumbee.Console.ILayout%2cSystem.Int32%2cSystem.Int32%2cSystem.Int32%2cSystem.Boolean%2cConsoleGUI.Api.IConsole%2cJumbee.Console.IInputSource%2cSystem.Boolean)" data-throw-if-not-resolved="false"></xref>); without one, the
    close proceeds.

### <a id="Jumbee_Console_MultiTabCodeEditor_DocumentCount"></a> DocumentCount

The number of open documents.

```csharp
public int DocumentCount { get; }
```

#### Property Value

 int

### <a id="Jumbee_Console_MultiTabCodeEditor_Editors"></a> Editors

All open editors, in tab order.

```csharp
public IReadOnlyList<CodeEditor> Editors { get; }
```

#### Property Value

 IReadOnlyList<[CodeEditor](Jumbee.Console.CodeEditor.md)\>

### <a id="Jumbee_Console_MultiTabCodeEditor_Tabs"></a> Tabs

The underlying tab panel (for styling or advanced tab operations).

```csharp
public TabPanel Tabs { get; }
```

#### Property Value

 [TabPanel](Jumbee.Console.TabPanel.md)

## Methods

### <a id="Jumbee_Console_MultiTabCodeEditor_Clear"></a> Clear\(\)

Closes every document immediately, without the <xref href="Jumbee.Console.MultiTabCodeEditor.DocumentClosing" data-throw-if-not-resolved="false"></xref> veto or confirm prompt
    (each still raises <xref href="Jumbee.Console.MultiTabCodeEditor.DocumentClosed" data-throw-if-not-resolved="false"></xref>).

```csharp
public void Clear()
```

#### Remarks

For resetting the group — e.g. reloading a different set of files.

### <a id="Jumbee_Console_MultiTabCodeEditor_CloseActiveDocument"></a> CloseActiveDocument\(\)

Closes the active document (if any).

```csharp
public void CloseActiveDocument()
```

### <a id="Jumbee_Console_MultiTabCodeEditor_CloseDocument_Jumbee_Console_CodeEditor_"></a> CloseDocument\(CodeEditor\)

Closes the given document, honoring <xref href="Jumbee.Console.MultiTabCodeEditor.DocumentClosing" data-throw-if-not-resolved="false"></xref> and (when
    <xref href="Jumbee.Console.MultiTabCodeEditor.ConfirmOnClose" data-throw-if-not-resolved="false"></xref> is set) the unsaved-changes prompt.

```csharp
public void CloseDocument(CodeEditor editor)
```

#### Parameters

`editor` [CodeEditor](Jumbee.Console.CodeEditor.md)

### <a id="Jumbee_Console_MultiTabCodeEditor_GetHelpInfo"></a> GetHelpInfo\(\)

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

### <a id="Jumbee_Console_MultiTabCodeEditor_IsDirty_Jumbee_Console_CodeEditor_"></a> IsDirty\(CodeEditor\)

Whether a document has unsaved changes (its text differs from when it was opened or last marked
    saved via <xref href="Jumbee.Console.MultiTabCodeEditor.SetDirty(Jumbee.Console.CodeEditor%2cSystem.Boolean)" data-throw-if-not-resolved="false"></xref>).

```csharp
public bool IsDirty(CodeEditor editor)
```

#### Parameters

`editor` [CodeEditor](Jumbee.Console.CodeEditor.md)

#### Returns

 bool

### <a id="Jumbee_Console_MultiTabCodeEditor_NewDocument"></a> NewDocument\(\)

Opens a new empty document with a generated "untitled-N" name.

```csharp
public CodeEditor NewDocument()
```

#### Returns

 [CodeEditor](Jumbee.Console.CodeEditor.md)

### <a id="Jumbee_Console_MultiTabCodeEditor_OpenDocument_System_String_System_String_System_Nullable_Jumbee_Console_Language__System_Boolean_"></a> OpenDocument\(string, string, Language?, bool\)

Opens a document in a new tab and selects it. Returns the created editor. Set
    <code class="paramref">closable</code> to <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a> to pin it (no ✕).

```csharp
public CodeEditor OpenDocument(string name, string text = "", Language? language = null, bool closable = true)
```

#### Parameters

`name` string

`text` string

`language` [Language](Jumbee.Console.Language.md)?

`closable` bool

#### Returns

 [CodeEditor](Jumbee.Console.CodeEditor.md)

### <a id="Jumbee_Console_MultiTabCodeEditor_SetDirty_Jumbee_Console_CodeEditor_System_Boolean_"></a> SetDirty\(CodeEditor, bool\)

Marks a document dirty, or clean (<code class="paramref">dirty</code> = <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a> also records the
    current text as the new saved baseline). Toggles a leading "● " marker on the tab label.

```csharp
public void SetDirty(CodeEditor editor, bool dirty)
```

#### Parameters

`editor` [CodeEditor](Jumbee.Console.CodeEditor.md)

`dirty` bool

### <a id="Jumbee_Console_MultiTabCodeEditor_ActiveDocumentChanged"></a> ActiveDocumentChanged

Raised after the active document changes (its editor, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> when none remain).

```csharp
public event EventHandler<CodeEditor?>? ActiveDocumentChanged
```

#### Event Type

 EventHandler<[CodeEditor](Jumbee.Console.CodeEditor.md)?\>?

### <a id="Jumbee_Console_MultiTabCodeEditor_DocumentClosed"></a> DocumentClosed

Raised after a document's tab has been removed.

```csharp
public event EventHandler<CodeEditor>? DocumentClosed
```

#### Event Type

 EventHandler<[CodeEditor](Jumbee.Console.CodeEditor.md)\>?

### <a id="Jumbee_Console_MultiTabCodeEditor_DocumentClosing"></a> DocumentClosing

Raised before a document closes (via ✕ or <xref href="Jumbee.Console.MultiTabCodeEditor.CloseDocument(Jumbee.Console.CodeEditor)" data-throw-if-not-resolved="false"></xref>). Set
    <xref href="Jumbee.Console.DocumentClosingEventArgs.Cancel" data-throw-if-not-resolved="false"></xref> to keep it open — e.g. after prompting about unsaved changes.

```csharp
public event EventHandler<DocumentClosingEventArgs>? DocumentClosing
```

#### Event Type

 EventHandler<[DocumentClosingEventArgs](Jumbee.Console.DocumentClosingEventArgs.md)\>?

### <a id="Jumbee_Console_MultiTabCodeEditor_DocumentOpened"></a> DocumentOpened

Raised after a document is opened (its editor + tab exist and it is selected).

```csharp
public event EventHandler<CodeEditor>? DocumentOpened
```

#### Event Type

 EventHandler<[CodeEditor](Jumbee.Console.CodeEditor.md)\>?

