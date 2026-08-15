namespace Jumbee.Console.Examples;

using System.Collections.Generic;

/// <summary>
/// A VS-Code-style workspace: a tabbed <see cref="MultiTabCodeEditor"/> stacked over a live
/// <see cref="TerminalEmulator"/> in a vertical <see cref="SplitPanel"/> — closable tabs, syntax highlighting, and a
/// draggable divider to the shell below. Click either pane to direct input to it.
/// </summary>
public sealed class MultiTabEditorExample : CompositeControl, IActivatableExample
{
    public MultiTabEditorExample()
    {
        editor = new MultiTabCodeEditor(Language.CSharp);
        terminal = new TerminalEmulator(Pty.DefaultShell);   
        editor.OpenDocument("Program.cs",
            "static class Program\n{\n    static void Main()\n    {\n        System.Console.WriteLine(\"Hello, Jumbee!\");\n    }\n}\n");
        editor.OpenDocument("notes.md",
            "# Notes\n\n- Edit any tab; click the ✕ to close one, the + to add one (Alt+←/→ switches tabs)\n" +
            "- Click the terminal below and type — try `dir`, `echo hi`, or `dotnet --version`\n" +
            "- Drag the divider between the editor and terminal, or focus it and press ↑/↓\n",
            Language.Markdown);

        SetContent(new SplitPanel(SplitOrientation.Vertical, editor, terminal, splitPosition: 22));
        
        // The example host wraps a Control example in a borderless frame that lights up (the theme's
        // FocusedFrameBorder) when the example contains focus — an outer box around the whole workspace we don't want
        // (the panes show focus via their cursors). Claim that frame up front with BorderPlacement.None so it draws
        // nothing, focused or not; the host's WithFrame reuses this frame rather than adding its own.
        this.WithFrame(borderStyle: BorderStyle.None, borderPlacement: BorderPlacement.None);        
    }

    // When focus resolves up to this workspace (Ctrl-nav into the pane, or a click on the terminal — the one plain
    // control the composite owns), delegate to the terminal. The editor is a self-contained composite that
    // click-to-focus resolves to directly, so it doesn't route through here.
    protected override Control? FocusChild => terminal;

    #region IActivatableExample
    string IExample.Category => "Editors";
    string IExample.Title => "Tabbed Code Editor";
    string IExample.Description =>
        "A VS-Code-style editor group over a live terminal: closable tabs, syntax highlighting, and a draggable divider to the shell below.";

    // Show the two controls this example composes rather than the CompositeControl framework base.
    IReadOnlyList<string> IExample.SourceFiles => ["MultiTabEditorExample.cs", "MultiTabCodeEditor.cs", "TerminalEmulator.cs"];
    // Start the shell only while shown and stop it when navigated away, so browsing other examples doesn't leave a background shell running.
    void IActivatableExample.OnActivated() => terminal.StartProcess();
    void IActivatableExample.OnDeactivated() => terminal.StopProcess();
    #endregion

    #region Fields
    private readonly MultiTabCodeEditor editor;
    private readonly TerminalEmulator terminal;
    #endregion
}
