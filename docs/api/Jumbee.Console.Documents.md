# <a id="Jumbee_Console_Documents"></a> Namespace Jumbee.Console.Documents

### Classes

 [AsciiDocLanguage](Jumbee.Console.Documents.AsciiDocLanguage.md)

A ColorCode <xref href="ColorCode.ILanguage" data-throw-if-not-resolved="false"></xref> grammar for AsciiDoc source, for syntax-highlighting an AsciiDoc document in a
<xref href="Jumbee.Console.CodeEditor" data-throw-if-not-resolved="false"></xref> (<code>new CodeEditor(AsciiDocLanguage.Instance)</code>).

 [AsciiDocStyles](Jumbee.Console.Documents.AsciiDocStyles.md)

Visual styling for <xref href="Jumbee.Console.Documents.AsciiDocViewer" data-throw-if-not-resolved="false"></xref>.

 [AsciiDocViewer](Jumbee.Console.Documents.AsciiDocViewer.md)

A read-only, scrollable AsciiDoc viewer.

 [InteractiveAsciiDocEditor](Jumbee.Console.Documents.InteractiveAsciiDocEditor.md)

A live, split-pane AsciiDoc editor for the terminal: the left pane is a <xref href="Jumbee.Console.CodeEditor" data-throw-if-not-resolved="false"></xref> with AsciiDoc
syntax highlighting (see <xref href="Jumbee.Console.Documents.AsciiDocLanguage" data-throw-if-not-resolved="false"></xref>); the right pane is an <xref href="Jumbee.Console.Documents.AsciiDocViewer" data-throw-if-not-resolved="false"></xref> that
re-renders the document — headings, formatting, lists, admonitions, tables and blocks — as you type.

 [InteractiveMarkdownExtendedEditor](Jumbee.Console.Documents.InteractiveMarkdownExtendedEditor.md)

A live, split-pane Markdown editor whose preview renders embedded <code>```mermaid</code> code blocks as diagrams — the
interactive complement to <xref href="Jumbee.Console.Documents.MarkdownExtendedViewer" data-throw-if-not-resolved="false"></xref>.

 [InteractiveMermaidEditor](Jumbee.Console.Documents.InteractiveMermaidEditor.md)

A live, split-pane Mermaid editor for the terminal: the left pane is a <xref href="Jumbee.Console.CodeEditor" data-throw-if-not-resolved="false"></xref> with Mermaid syntax
highlighting (see <xref href="Jumbee.Console.Documents.MermaidLanguage" data-throw-if-not-resolved="false"></xref>); the right pane is a <xref href="Jumbee.Console.Documents.MermaidViewer" data-throw-if-not-resolved="false"></xref> that re-renders
the diagram as you type.

 [MarkdownExtendedViewer](Jumbee.Console.Documents.MarkdownExtendedViewer.md)

A <xref href="Jumbee.Console.MarkdownViewer" data-throw-if-not-resolved="false"></xref> that renders fenced <code>```mermaid</code> code blocks as diagrams (flowchart, sequence,
class, ER, state) instead of showing their source.

 [MarkdownWithMermaidLanguage](Jumbee.Console.Documents.MarkdownWithMermaidLanguage.md)

A ColorCode <xref href="ColorCode.ILanguage" data-throw-if-not-resolved="false"></xref> that highlights Markdown <em>and</em> the contents of embedded
<code>```mermaid</code> fenced blocks (using the <xref href="Jumbee.Console.Documents.MermaidLanguage" data-throw-if-not-resolved="false"></xref> grammar) — for editing Markdown that
contains mermaid diagrams in a <xref href="Jumbee.Console.CodeEditor" data-throw-if-not-resolved="false"></xref>.

 [MermaidLanguage](Jumbee.Console.Documents.MermaidLanguage.md)

A ColorCode <xref href="ColorCode.ILanguage" data-throw-if-not-resolved="false"></xref> grammar for Mermaid diagram source, for syntax-highlighting a Mermaid document
in a <xref href="Jumbee.Console.CodeEditor" data-throw-if-not-resolved="false"></xref> (<code>new CodeEditor(MermaidLanguage.Instance)</code>).

 [MermaidStyles](Jumbee.Console.Documents.MermaidStyles.md)

Colours and scale for <xref href="Jumbee.Console.Documents.MermaidViewer" data-throw-if-not-resolved="false"></xref>.

 [MermaidViewer](Jumbee.Console.Documents.MermaidViewer.md)

A read-only, scrollable viewer for Mermaid <code>flowchart</code>/<code>graph</code>, <code>stateDiagram</code>,
<code>classDiagram</code>, <code>erDiagram</code> and <code>sequenceDiagram</code> diagrams.

