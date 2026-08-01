# Documents

Rendering Markdown, AsciiDoc and Mermaid in the terminal — and editing them with a live preview.

## Which package

Only some of these are in the core package:

| Control | Package |
|---|---|
| `MarkdownViewer`, `InteractiveMarkdownEditor` | `Jumbee.Console` |
| `MarkdownExtendedViewer`, `AsciiDocViewer`, `MermaidViewer`, and their editors | `Jumbee.Console.Documents` |

The `Jumbee.Console.Documents` types are in the `Jumbee.Console.Documents` namespace and need that package
referenced — so the snippets below for those controls assume `using Jumbee.Console.Documents;`.

## Choosing

| I want | Use |
|---|---|
| Show Markdown | `MarkdownViewer` |
| Show Markdown with embedded Mermaid diagrams | `MarkdownExtendedViewer` |
| Show AsciiDoc | `AsciiDocViewer` |
| Show a Mermaid diagram on its own | `MermaidViewer` |
| Edit any of the above with a live preview | the matching `Interactive*Editor` |

## What the viewers have in common

All four behave the same way, so this applies to each:

**They need a frame to scroll.** They render at content height and expect a `ControlFrame` to give them a border,
a title and a scrollbar:

```csharp
viewer.WithFrame(title: "README.md");
```

Once framed, ↑/↓, PgUp/PgDn, Home/End and the mouse wheel scroll them.

**Parsing runs on a background thread.** Setting the content or resizing never blocks the UI thread — the view
fills in when the render completes. This is deliberate: these parses are slow enough to drop frames if they ran
inline. It also means the content won't be on screen the instant after you assign it, which matters when
snapshot-testing.

**They re-render only when they need to** — when the text, the styles or the width change. Assigning the same
content again is cheap; resizing reflows.

**Styling is a `Styles` object** per viewer (`MarkdownStyles`, `AsciiDocStyles`, `MermaidStyles`), so headings,
code spans, links and the rest follow your theme or your explicit choices.

## `MarkdownViewer`

```csharp
var viewer = new MarkdownViewer(File.ReadAllText("README.md"));
viewer.WithFrame(title: "README.md");

viewer.Markdown = File.ReadAllText(otherPath);   // swap the document
```

`Markdown` is the content, `Styles` the appearance, `MaxRows` caps the rendered height when you need a bounded
block rather than a whole document. Text reflows to the control width.

## `MarkdownExtendedViewer`

A `MarkdownViewer` that also renders ```` ```mermaid ```` fences as diagrams instead of code blocks.

```csharp
using Jumbee.Console.Documents;

var viewer = new MarkdownExtendedViewer(markdown);
viewer.DiagramStyles = myMermaidStyles;
```

Everything from `MarkdownViewer` applies — it's a subclass, hooking the `RenderMarkdown` seam. Use it whenever your
Markdown might carry diagrams; there's no cost when it doesn't.

## `AsciiDocViewer`

```csharp
using Jumbee.Console.Documents;

var viewer = new AsciiDocViewer(File.ReadAllText("guide.adoc"));
viewer.WithFrame(title: "guide.adoc");
```

`AsciiDoc` is the content. Renders headings, lists, box-drawn tables and bordered source and example blocks.

## `MermaidViewer`

Diagrams drawn as box-drawing cells — node boxes, rectilinear edges with arrowheads and labels, and subgraph
groups.

```csharp
using Jumbee.Console.Documents;

var viewer = new MermaidViewer("""
    flowchart TD
      A[Client] --> B{Cache?}
      B -- hit --> C[Return]
      B -- miss --> D[(Database)]
      D --> C
    """);
viewer.WithFrame(title: "Request flow");
```

Supported: `flowchart` / `graph`, `stateDiagram`, `classDiagram`, `erDiagram` and `sequenceDiagram`. Anything else
renders a short message rather than failing.

The diagram draws at its intrinsic size and clips horizontally if it's wider than the control — ←/→ pan it.

## The interactive editors

Each viewer has an editor pairing a `CodeEditor` with a live preview in a split:

| Editor | Edits | Package |
|---|---|---|
| `InteractiveMarkdownEditor` | Markdown | `Jumbee.Console` |
| `InteractiveMarkdownExtendedEditor` | Markdown with Mermaid fences | `Jumbee.Console.Documents` |
| `InteractiveAsciiDocEditor` | AsciiDoc | `Jumbee.Console.Documents` |
| `InteractiveMermaidEditor` | Mermaid | `Jumbee.Console.Documents` |

```csharp
var editor = new InteractiveMarkdownEditor(markdown, SplitOrientation.Vertical, splitPosition: 50);
editor.TextChanged += text => MarkDirty(text);      // Action<string>, not EventHandler
```

They share a base, `InteractiveSourceEditor`: `Editor` reaches the source `CodeEditor`, `PreviewControl` the
rendered side, `Split` the divider, and `Text` the source. The preview updates as you type — `ApplyPreviewText`
is the seam a subclass overrides to decide what that means.

Source highlighting comes from a ColorCode grammar per format (`MermaidLanguage`, `AsciiDocLanguage`,
`MarkdownWithMermaidLanguage`), which is also why `TextEditor` accepts a custom `ILanguage`.

## See also

- [Text and Input](Text%20and%20Input.md) — `TextEditor` and `CodeEditor`, which these are built on.
- [Layouts](Layouts.md) — `SplitPanel`, and framing for scroll.
- [Lists and Data](Lists%20and%20Data.md) — `TextPanel`, for a static block rather than a document.
