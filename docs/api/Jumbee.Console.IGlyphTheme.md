# <a id="Jumbee_Console_IGlyphTheme"></a> Interface IGlyphTheme

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.Styles.dll  

The glyphs controls use for state indicators.

```csharp
public interface IGlyphTheme
```

## Remarks

Like <xref href="Jumbee.Console.IStyleTheme" data-throw-if-not-resolved="false"></xref>, a glyph theme affects <em>appearance only</em>: swapping glyphs never changes
behaviour, though glyphs of a different cell width do change a control's measured size (controls derive their
layout from the themed glyph's width rather than assuming a constant). Members are default-implemented;
override only the glyphs you want to change, and read the theme through this interface type (e.g.
<code>UI.GlyphTheme</code>).

## Properties

### <a id="Jumbee_Console_IGlyphTheme_CheckboxChecked"></a> CheckboxChecked

Glyph for a checked checkbox.

```csharp
string CheckboxChecked { get; }
```

#### Property Value

 string

### <a id="Jumbee_Console_IGlyphTheme_CheckboxUnchecked"></a> CheckboxUnchecked

Glyph for an unchecked checkbox.

```csharp
string CheckboxUnchecked { get; }
```

#### Property Value

 string

### <a id="Jumbee_Console_IGlyphTheme_File"></a> File

The marker a <code>FileBrowser</code> draws before a file. Defaults to a blank of the same width as
    <xref href="Jumbee.Console.IGlyphTheme.FolderClosed" data-throw-if-not-resolved="false"></xref>, so only folders carry a marker.

```csharp
string File { get; }
```

#### Property Value

 string

### <a id="Jumbee_Console_IGlyphTheme_FolderClosed"></a> FolderClosed

The marker a <code>FileBrowser</code> draws before a folder. Defaults to <code>"▸"</code>.

```csharp
string FolderClosed { get; }
```

#### Property Value

 string

#### Remarks

Should share a cell width with <xref href="Jumbee.Console.IGlyphTheme.File" data-throw-if-not-resolved="false"></xref> so names stay aligned down the listing.

### <a id="Jumbee_Console_IGlyphTheme_MenuChecked"></a> MenuChecked

The marker drawn against a checked <code>MenuItem</code>. Defaults to <code>"✓"</code> (override with
    <code>"*"</code> for an ASCII terminal).

```csharp
string MenuChecked { get; }
```

#### Property Value

 string

#### Remarks

Its width sets the marker column a menu level reserves once any of its items is checkable, so all
    the labels in that level stay aligned.

### <a id="Jumbee_Console_IGlyphTheme_ProgressBar"></a> ProgressBar

The glyphs a <code>ProgressBar</code> draws its bar with (colours come from <xref href="Jumbee.Console.IStyleTheme.ProgressBar" data-throw-if-not-resolved="false"></xref>).
    Defaults to <xref href="Jumbee.Console.ProgressBarGlyphs.Default" data-throw-if-not-resolved="false"></xref> (the solid sub-cell band).

```csharp
ProgressBarGlyphs ProgressBar { get; }
```

#### Property Value

 [ProgressBarGlyphs](Jumbee.Console.ProgressBarGlyphs.md)

### <a id="Jumbee_Console_IGlyphTheme_RadioSelected"></a> RadioSelected

Glyph for a selected radio button.

```csharp
string RadioSelected { get; }
```

#### Property Value

 string

### <a id="Jumbee_Console_IGlyphTheme_RadioUnselected"></a> RadioUnselected

Glyph for an unselected radio button.

```csharp
string RadioUnselected { get; }
```

#### Property Value

 string

### <a id="Jumbee_Console_IGlyphTheme_ScrollBar"></a> ScrollBar

The glyphs a control frame's vertical scrollbar uses (colours come from <xref href="Jumbee.Console.IStyleTheme.ScrollBar" data-throw-if-not-resolved="false"></xref>).
    Defaults to <xref href="Jumbee.Console.ScrollBarGlyphs.Default" data-throw-if-not-resolved="false"></xref>.

```csharp
ScrollBarGlyphs ScrollBar { get; }
```

#### Property Value

 [ScrollBarGlyphs](Jumbee.Console.ScrollBarGlyphs.md)

### <a id="Jumbee_Console_IGlyphTheme_SelectionCaret"></a> SelectionCaret

The glyph prefixed to the selected item when a control's <xref href="Jumbee.Console.SelectionStyle" data-throw-if-not-resolved="false"></xref> is
    <xref href="Jumbee.Console.SelectionStyle.Caret" data-throw-if-not-resolved="false"></xref> (includes its trailing spacing). Defaults to <code>"▶ "</code>.

```csharp
string SelectionCaret { get; }
```

#### Property Value

 string

### <a id="Jumbee_Console_IGlyphTheme_SliderThumb"></a> SliderThumb

The glyph a <code>Slider</code> draws its thumb with (colours come from <xref href="Jumbee.Console.IStyleTheme.Slider" data-throw-if-not-resolved="false"></xref>).
    Defaults to <code>"█"</code>.

```csharp
string SliderThumb { get; }
```

#### Property Value

 string

#### Remarks

Should be one cell wide: the thumb marks a single position on the track, and a wider glyph would
    push the rest of the track out of alignment with the pointer.

### <a id="Jumbee_Console_IGlyphTheme_SwitchOff"></a> SwitchOff

Glyph for a switch in the off position.

```csharp
string SwitchOff { get; }
```

#### Property Value

 string

### <a id="Jumbee_Console_IGlyphTheme_SwitchOn"></a> SwitchOn

Glyph for a switch in the on position.

```csharp
string SwitchOn { get; }
```

#### Property Value

 string

### <a id="Jumbee_Console_IGlyphTheme_TabAdd"></a> TabAdd

The glyph on a tab panel's "+" new-tab button. Defaults to <code>"+"</code>.

```csharp
string TabAdd { get; }
```

#### Property Value

 string

### <a id="Jumbee_Console_IGlyphTheme_TabClose"></a> TabClose

The close glyph shown on a closable tab. Defaults to <code>"✕"</code> (override with <code>"x"</code> for an
    ASCII terminal).

```csharp
string TabClose { get; }
```

#### Property Value

 string

#### Remarks

Drawn only on the active/hovered tab; other tabs reserve a same-width blank.

### <a id="Jumbee_Console_IGlyphTheme_TreeCollapsed"></a> TreeCollapsed

Disclosure glyph shown before a <em>collapsed</em> node that has children (includes trailing
    spacing). Defaults to <code>"► "</code>.

```csharp
string TreeCollapsed { get; }
```

#### Property Value

 string

#### Remarks

U+25BA — the text-presentation counterpart of <code>▼</code>; the emoji-variant <code>▶</code> U+25B6 tofus in
    some fonts.

### <a id="Jumbee_Console_IGlyphTheme_TreeExpanded"></a> TreeExpanded

Disclosure glyph shown before an <em>expanded</em> node that has children (includes trailing
    spacing). Defaults to <code>"▼ "</code>.

```csharp
string TreeExpanded { get; }
```

#### Property Value

 string

#### Remarks

Both tree glyphs should share a cell width so labels stay aligned.

### <a id="Jumbee_Console_IGlyphTheme_TreeLeaf"></a> TreeLeaf

Glyph shown before a node that has <em>no</em> children (a leaf), including trailing spacing.
    Defaults to <code>"• "</code>.

```csharp
string TreeLeaf { get; }
```

#### Property Value

 string

#### Remarks

Should share a cell width with the disclosure glyphs so labels stay aligned.

## Methods

### <a id="Jumbee_Console_IGlyphTheme_CellWidth_System_String_System_String_"></a> CellWidth\(string, string\)

The cell width of the widest of two state glyphs (both states of a toggle should be equal width;
    this guards against a theme that isn't).

```csharp
public static int CellWidth(string a, string b)
```

#### Parameters

`a` string

`b` string

#### Returns

 int

