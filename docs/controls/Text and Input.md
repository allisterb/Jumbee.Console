# Text and Input

Displaying text, and letting the user type it. Runs from a one-line label up to a tabbed code editor.

## Choosing

| I want | Use |
|---|---|
| Show one line of text | `TextLabel` |
| Show a block of static formatted text | [`TextPanel`](Lists%20and%20Data.md#textpanel) |
| Show a scrolling document | [`MarkdownViewer`](Documents.md) and friends |
| Take one line of input | `TextInput` |
| Take a line at a REPL-style prompt | `TextPrompt` |
| Edit multi-line text | `TextEditor` |
| Edit code, with line numbers | `CodeEditor` |
| Edit several documents at once | `MultiTabCodeEditor` |
| Build an agent/chat input line | `ChatPrompt` |
| Add type-ahead to any `TextInput` | `Autocomplete` |

The editors stack: `TextEditor` is the text surface, `CodeEditor` is that plus a line-number gutter, and
`MultiTabCodeEditor` is a tabbed group of those. Pick the lowest one that does what you need.

## `TextLabel`

One line of text with a colour and an orientation.

```csharp
var label = new TextLabel(TextLabelOrientation.Horizontal, "Count: 0", Color.Cyan1);
label.Text = "Count: 1";     // setting it redraws — there is no refresh call
```

> **The orientation comes first, not the text.** `new TextLabel("hello")` doesn't compile, and it's the single most
> common first-day mistake with this control.

`FgColor`, `BgColor` and `Decoration` cover the styling; vertical orientation renders the text down a column.

For anything longer than a line, use [`TextPanel`](Lists%20and%20Data.md#textpanel).

## `TextInput`

A single-line entry field: caret, selection with Shift+navigation, horizontal scrolling once the text outgrows the
width, an optional placeholder, and optional masking.

```csharp
var input = new TextInput(placeholder: "search…");
input.Changed   += (_, _) => Filter(input.Text);
input.Submitted += (_, _) => Run(input.Text);
```

The constructor is `TextInput(string text = "", string placeholder = "")`, so pass `placeholder:` by name unless
you're also seeding the text.

`Text` is the content, `CaretIndex` the caret position and `SelectedText` the selection. `PasswordChar` masks input
for a secret; `ReadOnly` makes it display-only while still selectable. Styling is `TextStyle`, `PlaceholderStyle`
and `SelectionStyle`.

It owns the native terminal cursor while focused — only the focused control draws one — so the caret you see is the
real terminal caret, not a drawn glyph.

`KeyInterceptor` lets you see keys before the control does, which is how you bolt on behaviour (history recall, a
custom chord) without subclassing. `OnPaste` handles bracketed paste.

## `Autocomplete`

Type-ahead attached to an existing `TextInput` — not a control you place.

```csharp
var input = new TextInput(placeholder: "method");
var complete = new Autocomplete(input, "GET", "POST", "PUT", "PATCH", "DELETE");
```

Matching candidates appear in a passive popup just below the caret. Enter or Tab accepts, Escape dismisses, and the
popup closes when the field loses focus or nothing matches. A suggestion can also be clicked.

For dynamic suggestions, pass a function instead of a fixed list:

```csharp
var complete = new Autocomplete(input, term => index.Search(term).Take(10));
```

`MaxRows` caps the popup height; `Close()` dismisses it programmatically.

## `TextPrompt`

A REPL-style prompt line — a fixed prompt string followed by an editable field, raising `Committed` when the user
presses Enter.

```csharp
var prompt = new TextPrompt("> ");
prompt.Committed += (_, line) => Execute(line);
```

`TextPrompt(string prompt, bool showCursor = true, bool blinkCursor = false)`. Use it for command lines and shells;
for a general form field use `TextInput`, and for a chat-style composer use `ChatPrompt`.

## `TextEditor`

Multi-line editing with syntax highlighting.

```csharp
var editor = new TextEditor(Language.CSharp);
editor.Text = File.ReadAllText(path);
editor.Changed += (_, _) => MarkDirty();
```

`TextEditor(Language language = Language.None, bool showCursor = true, bool blinkCursor = false)`, or pass an
`ILanguage` for a ColorCode grammar outside the built-in enum — that's how the Mermaid and AsciiDoc editors get
their highlighting.

`Text` is the whole document; `CaretLine`, `CaretIndex` and `CursorX`/`CursorY` locate the caret, `LineCount` and
`VisualRowCount(width)` give you the logical and wrapped line counts. `SelectAll()` and `SelectedText` cover
selection, `TabWidth` indentation, `ReadOnly` a viewer mode.

**It soft-wraps at the character level**, and the caret arithmetic mirrors that wrap exactly — which is why it
disables Spectre's own word wrap. `VisualLineNumbers()` maps wrapped rows back to logical lines, which is what the
gutter uses.

The editor sizes to its content, so wrap it in a frame to get scrolling:

```csharp
editor.WithFrame();
```

## `CodeEditor`

A `TextEditor` with a `LineNumberGutter` docked to its left, kept in sync with the line count and the active line.

```csharp
var code = new CodeEditor(Language.CSharp);
code.Text = source;
code.WithFrame();          // frames the pair, so gutter and text scroll together
```

`Editor` and `Gutter` reach the two halves when you need them; `Text` and `ReadOnly` are forwarded for convenience.

Frame the composite rather than the editor inside it — that's what scrolls both together with an accurate
scrollbar, and what keeps the caret in view.

## `MultiTabCodeEditor`

A tabbed group of `CodeEditor`s — a VS-Code-style editing area. Each document is a closable tab, with a `+` at the
end of the bar for a new one.

```csharp
var editors = new MultiTabCodeEditor(Language.CSharp);
editors.OpenDocument("Program.cs", source);
editors.ConfirmOnClose = true;      // prompt when closing a modified document

editors.DocumentClosing += (_, e) => { if (!ShouldClose(e)) e.Cancel = true; };
```

`OpenDocument(name, text, language, …)` adds a tab, `NewDocument()` opens an empty one, and
`CloseDocument` / `CloseActiveDocument()` close them. `ActiveEditor` and `ActiveDocumentName` track the current tab,
`Editors` and `Tabs` expose the whole set, `DocumentCount` counts them.

Dirty state is tracked automatically; `IsDirty` reads it and `SetDirty` overrides it (after your own save, for
instance). With `ConfirmOnClose` set, closing a modified document raises a confirmation dialog. The
`DocumentOpened` / `DocumentClosing` / `DocumentClosed` / `ActiveDocumentChanged` events cover the lifecycle, and
`DocumentClosing` is cancellable.

Switch tabs with Alt+←/→ or by clicking.

## `ChatPrompt`

The input line of an agent or chat CLI: a prompt glyph on the left that becomes an animated spinner while work is
running, and a `TextInput` filling the rest.

```csharp
var chat = new ChatPrompt("› ");
chat.Placeholder = "ask anything";
chat.WithSuggestions("/help", "/clear", "/model");
chat.WithRoundedBorder();

chat.Submitted += async (_, _) =>
{
    var text = chat.Text;
    chat.Busy = true;            // the glyph becomes a spinner
    await Respond(text);
    chat.Busy = false;
};
```

`Busy` drives the spinner, `Prompt` and `PromptStyle` the gutter glyph, `Input` reaches the underlying `TextInput`.
`WithSuggestions` takes either a fixed list or a function, and wires an `Autocomplete` for you.

Focus delegates to the input, which keeps the caret; the gutter is a non-focusable adornment. It's a composite
control, so it drops into a layout cell and frames like anything else.

## See also

- [Documents](Documents.md) — the Markdown/AsciiDoc/Mermaid viewers and their interactive editors.
- [Lists and Data](Lists%20and%20Data.md) — `TextPanel`, for static multi-line text.
- [Input](../internal/Input.md) — focus, key routing and what reaches a control.
