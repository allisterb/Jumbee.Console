# <a id="Jumbee_Console_FileBrowser"></a> Class FileBrowser

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

A two-pane file chooser: a lazily-populated directory <xref href="Jumbee.Console.Tree" data-throw-if-not-resolved="false"></xref> on the left, the current directory's
contents in a <xref href="Jumbee.Console.ListBox" data-throw-if-not-resolved="false"></xref> on the right, a path field above and a filter drop-down below.

```csharp
public class FileBrowser : CompositeControl, IFocusable
```

#### Inheritance

object ← 
Control ← 
[Control](Jumbee.Console.Control.md) ← 
[CompositeControl](Jumbee.Console.CompositeControl.md) ← 
[FileBrowser](Jumbee.Console.FileBrowser.md)

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

[ControlExtensions.WithAsciiBorder<FileBrowser\>\(FileBrowser, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithAsciiBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithBorder<FileBrowser\>\(FileBrowser, BorderStyle?, Color?, Color?, BorderPlacement?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_), 
[ControlExtensions.WithDoubleBorder<FileBrowser\>\(FileBrowser, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithDoubleBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithFrame<FileBrowser\>\(FileBrowser, ControlFrame\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_Jumbee\_Console\_ControlFrame\_), 
[ControlExtensions.WithFrame<FileBrowser\>\(FileBrowser, BorderStyle?, Offset?, Color?, Color?, string?, Color?, Color?, BorderPlacement?, BorderStyle?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_ConsoleGUI\_Space\_Offset\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_String\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_), 
[ControlExtensions.WithHeavyBorder<FileBrowser\>\(FileBrowser, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeavyBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithHeight<FileBrowser\>\(FileBrowser, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeight\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithMargin<FileBrowser\>\(FileBrowser, int, int, int, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_System\_Int32\_System\_Int32\_System\_Int32\_), 
[ControlExtensions.WithMargin<FileBrowser\>\(FileBrowser, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithNoBorder<FileBrowser\>\(FileBrowser\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithNoBorder\_\_1\_\_\_0\_), 
[ControlExtensions.WithRoundedBorder<FileBrowser\>\(FileBrowser, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithRoundedBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithScrollBarGlyphs<FileBrowser\>\(FileBrowser, ScrollBarGlyphs\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarGlyphs\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarGlyphs\_), 
[ControlExtensions.WithScrollBarStyle<FileBrowser\>\(FileBrowser, ScrollBarStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarStyle\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarStyle\_), 
[ControlExtensions.WithSize<FileBrowser\>\(FileBrowser, int?, int?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSize\_\_1\_\_\_0\_System\_Nullable\_System\_Int32\_\_System\_Nullable\_System\_Int32\_\_), 
[ControlExtensions.WithSquareBorder<FileBrowser\>\(FileBrowser, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSquareBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithTitle<FileBrowser\>\(FileBrowser, string\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_), 
[ControlExtensions.WithTitle<FileBrowser\>\(FileBrowser, string, TitleStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitleStyle\_), 
[ControlExtensions.WithTitle<FileBrowser\>\(FileBrowser, string, TitlePos, TitleBorderStyle, TitleColorStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitlePos\_Jumbee\_Console\_TitleBorderStyle\_Jumbee\_Console\_TitleColorStyle\_), 
[ControlExtensions.WithWidth<FileBrowser\>\(FileBrowser, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithWidth\_\_1\_\_\_0\_System\_Int32\_)

## Remarks

<p>
Usually shown as a modal through <xref href="Jumbee.Console.FileBrowser.OpenFile(System.String%2cSystem.String%2cSystem.String%5b%5d%2cSystem.Action%7bSystem.String%7d)" data-throw-if-not-resolved="false"></xref> or <xref href="Jumbee.Console.FileBrowser.OpenDirectory(System.String%2cSystem.String%2cSystem.Action%7bSystem.String%7d)" data-throw-if-not-resolved="false"></xref>, which wrap it in a
<xref href="Jumbee.Console.Dialog" data-throw-if-not-resolved="false"></xref> and report the chosen path (or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> if cancelled). Place it directly
when you want a browser embedded in a pane rather than floating over one.
</p>
<p>
Directories are read on demand — a tree node's children when it is first expanded, the listing when the
directory changes — so a folder with thousands of entries under it costs nothing until you look. Every
enumeration is guarded: an unreadable directory shows a message in the list instead of throwing out of a paint,
which matters because the first thing a Windows user meets under <code>C:\</code> is
<code>System Volume Information</code>.
</p>
<p>
The tree shows the directory being listed and what is under it, and is re-rooted whenever the listing moves.
Going up or elsewhere is what the <code>..</code> row and the path field are for. See <code>RerootTree</code> for why it
works this way rather than showing the whole machine.
</p>

## Constructors

### <a id="Jumbee_Console_FileBrowser__ctor_System_String_Jumbee_Console_FileBrowserMode_System_Collections_Generic_IReadOnlyList_System_String__"></a> FileBrowser\(string?, FileBrowserMode, IReadOnlyList<string\>?\)

Creates a browser rooted at <code class="paramref">startPath</code> (a directory, or a file whose directory is
    opened with it selected), choosing whatever <code class="paramref">mode</code> asks for and narrowing the file list to
    <code class="paramref">filters</code>.

```csharp
public FileBrowser(string? startPath = null, FileBrowserMode mode = FileBrowserMode.OpenFile, IReadOnlyList<string>? filters = null)
```

#### Parameters

`startPath` string?

`mode` [FileBrowserMode](Jumbee.Console.FileBrowserMode.md)

`filters` IReadOnlyList<string\>?

#### Remarks

The filters are fixed for the browser's lifetime because they populate the filter drop-down; build
    a new browser to change them.

## Fields

### <a id="Jumbee_Console_FileBrowser_AllFiles"></a> AllFiles

The pattern that lists every file, and the default when none is given.

```csharp
public const string AllFiles = "*.*"
```

#### Field Value

 string

## Properties

### <a id="Jumbee_Console_FileBrowser_CurrentDirectory"></a> CurrentDirectory

The directory being listed. Setting it navigates.

```csharp
public string CurrentDirectory { get; set; }
```

#### Property Value

 string

### <a id="Jumbee_Console_FileBrowser_Filters"></a> Filters

The glob patterns the file list can be narrowed to (<code>*.obj</code>, <code>*.png</code>, …), as offered by
    the filter drop-down. <xref href="Jumbee.Console.FileBrowser.AllFiles" data-throw-if-not-resolved="false"></xref> shows everything and is always the last option.

```csharp
public IReadOnlyList<string> Filters { get; }
```

#### Property Value

 IReadOnlyList<string\>

### <a id="Jumbee_Console_FileBrowser_Mode"></a> Mode

What the browser is choosing.

```csharp
public FileBrowserMode Mode { get; }
```

#### Property Value

 [FileBrowserMode](Jumbee.Console.FileBrowserMode.md)

### <a id="Jumbee_Console_FileBrowser_SelectedPath"></a> SelectedPath

The full path of the highlighted entry, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> when nothing valid is highlighted.

```csharp
public string? SelectedPath { get; }
```

#### Property Value

 string?

#### Remarks

In <xref href="Jumbee.Console.FileBrowserMode.OpenDirectory" data-throw-if-not-resolved="false"></xref> this is the listed directory itself while no
    subdirectory is highlighted, so the dialog's OK button always has something to return.

### <a id="Jumbee_Console_FileBrowser_ShowHidden"></a> ShowHidden

Whether hidden and system entries are listed. Default <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>.

```csharp
public bool ShowHidden { get; set; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_FileBrowser_TabNavigatesChildren"></a> TabNavigatesChildren

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, Tab / Shift+Tab move focus between the composite's focusable children
    (cycling within it) instead of reaching the focused child. Off by default: Tab belongs to the focused control
    (a <xref href="Jumbee.Console.TextEditor" data-throw-if-not-resolved="false"></xref> indents with it). Turn it on for a form — several fields the user tabs between.

```csharp
protected override bool TabNavigatesChildren { get; }
```

#### Property Value

 bool

## Methods

### <a id="Jumbee_Console_FileBrowser_ApplyTheme"></a> ApplyTheme\(\)

Re-captures this control's themed colours/glyphs from the current <xref href="Jumbee.Console.UI.StyleTheme" data-throw-if-not-resolved="false"></xref>/
<xref href="Jumbee.Console.UI.GlyphTheme" data-throw-if-not-resolved="false"></xref>. The default is a no-op for controls that don't use the theme.

```csharp
protected override void ApplyTheme()
```

#### Remarks

Called by themed controls from their constructor and again on a runtime theme switch (<xref href="Jumbee.Console.UI.SetTheme(Jumbee.Console.IStyleTheme%2cJumbee.Console.IGlyphTheme)" data-throw-if-not-resolved="false"></xref>).
Must read the themes <em>only here</em> (and in the constructor), never on the render path.

### <a id="Jumbee_Console_FileBrowser_GetHelpInfo"></a> GetHelpInfo\(\)

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

### <a id="Jumbee_Console_FileBrowser_NavigateTo_System_String_"></a> NavigateTo\(string?\)

Lists <code class="paramref">directory</code>, if it can be read.

```csharp
public void NavigateTo(string? directory)
```

#### Parameters

`directory` string?

### <a id="Jumbee_Console_FileBrowser_OpenDirectory_System_String_System_String_System_Action_System_String__"></a> OpenDirectory\(string, string?, Action<string?\>\)

Shows a modal directory chooser and reports the chosen directory, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> if
    cancelled.

```csharp
public static Dialog OpenDirectory(string title, string? start, Action<string?> onResult)
```

#### Parameters

`title` string

`start` string?

`onResult` Action<string?\>

#### Returns

 [Dialog](Jumbee.Console.Dialog.md)

### <a id="Jumbee_Console_FileBrowser_OpenFile_System_String_System_String_System_String___System_Action_System_String__"></a> OpenFile\(string, string?, string\[\]?, Action<string?\>\)

Shows a modal file chooser and reports the chosen path, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> if cancelled.

```csharp
public static Dialog OpenFile(string title, string? start, string[]? filters, Action<string?> onResult)
```

#### Parameters

`title` string

The dialog's title bar.

`start` string?

Where to open — a directory, or a file to preselect. Null starts in the current directory.

`filters` string\[\]?

Glob patterns for the filter drop-down; the first is applied. Null lists everything.

`onResult` Action<string?\>

Invoked with the chosen path, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> on cancel.

#### Returns

 [Dialog](Jumbee.Console.Dialog.md)

### <a id="Jumbee_Console_FileBrowser_Select_System_String_"></a> Select\(string\)

Highlights <code class="paramref">fullPath</code> in the list, if it is in the directory being listed.

```csharp
public void Select(string fullPath)
```

#### Parameters

`fullPath` string

### <a id="Jumbee_Console_FileBrowser_PathActivated"></a> PathActivated

Raised when an entry is committed (Enter, or a double-click) and it is a valid choice for the
    current <xref href="Jumbee.Console.FileBrowser.Mode" data-throw-if-not-resolved="false"></xref>. Committing a directory in <xref href="Jumbee.Console.FileBrowserMode.OpenFile" data-throw-if-not-resolved="false"></xref> navigates into
    it instead and raises nothing.

```csharp
public event EventHandler<string>? PathActivated
```

#### Event Type

 EventHandler<string\>?

### <a id="Jumbee_Console_FileBrowser_SelectionChanged"></a> SelectionChanged

Raised when the highlighted entry changes, with the full path or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a>.

```csharp
public event EventHandler<string?>? SelectionChanged
```

#### Event Type

 EventHandler<string?\>?

