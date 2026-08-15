namespace Jumbee.Console.Tests;

using ConsoleGUI.Data;
using ConsoleGUI.Input;
using ConsoleGUI.Space;

using Jumbee.Console;
using Jumbee.Console.Snapshot;

using Xunit;

/// <summary>
/// The "Tabbed Code Editor" example stacks an editor over a plain-control pane (a <c>TerminalEmulator</c>) in a
/// vertical <see cref="SplitPanel"/>, shown in the examples-browser middle pane (a <see cref="CompositeControl"/>).
/// A plain control nested in a <em>layout</em> example gets owned by that outer host, whose <c>FocusChild</c> is null
/// for a layout — so click-to-focus on it collapses to the host and dead-ends. Wrapping the split in its own
/// composite (with <c>FocusChild</c> ⇒ the plain pane) gives that pane a focus root that delegates back to it, so the
/// click lands. These tests guard that fix (and that it doesn't hijack clicks meant for the nested-composite pane).
/// </summary>
public class TerminalPaneFocusTests
{
    // Stand-in for the examples-browser middle pane: shows one example under a docked label. Like the real host it
    // delegates focus only to a Control example (null for a bare layout) — the condition that made the bug possible.
    private sealed class Host : CompositeControl
    {
        private IFocusable? _active;
        public void Show(IFocusable content)
        {
            _active = content;
            SetContent(new DockPanel(DockedControlPlacement.Top,
                new TextLabel(TextLabelOrientation.Horizontal, "desc") { Focusable = false },
                content is Control c ? c.WithFrame(borderStyle: Jumbee.Console.BorderStyle.None) : content));
        }        protected override Control? FocusChild => _active as Control;
    }

    // The fix: the example is a composite hosting the editor-over-terminal split, delegating focus to the terminal
    // pane (the one plain control it owns). Mirrors MultiTabEditorExample.
    private sealed class Workspace : CompositeControl
    {
        public readonly ListBox Terminal = new("$ prompt");     // plain-control stand-in for the terminal pane
        public readonly Editor EditorPane = new();
        public Workspace() =>
            SetContent(new SplitPanel(SplitOrientation.Vertical,
                EditorPane.WithFrame(borderStyle: Jumbee.Console.BorderStyle.None),
                Terminal.WithFrame(borderStyle: Jumbee.Console.BorderStyle.None), splitPosition: 6));        protected override Control? FocusChild => Terminal;
    }

    // Stand-in for the editor pane: a nested composite is its own focus unit, so a click resolves to it directly and
    // never routes up through the workspace's FocusChild.
    private sealed class Editor : CompositeControl
    {
        public readonly ListBox Inner = new("code");
        public Editor()
        {
            Inner.WithFrame(borderStyle: Jumbee.Console.BorderStyle.None);
            SetContent(new DockPanel(DockedControlPlacement.Top,
                new TextLabel(TextLabelOrientation.Horizontal, "ed") { Focusable = false }, Inner.Frame!));
        }    }

    [Fact]
    public void ClickTerminalPane_FocusesIt()
    {
        var host = new Host();
        var work = new Workspace();
        host.Show(work);
        var buf = ConsoleSnapshot.Render(host, 44, 18);

        ClickOn(buf, 44, 18, work.Terminal);

        Assert.True(work.Terminal.IsFocused);          // the click reached the terminal pane...
        Assert.False(work.EditorPane.Inner.IsFocused); // ...and not the editor
    }

    [Fact]
    public void ClickEditorPane_FocusesEditor_NotTerminal()
    {
        var host = new Host();
        var work = new Workspace();
        host.Show(work);
        var buf = ConsoleSnapshot.Render(host, 44, 18);

        ClickOn(buf, 44, 18, work.EditorPane.Inner);

        Assert.True(work.EditorPane.Inner.IsFocused);  // the FocusChild⇒Terminal override doesn't hijack this click
        Assert.False(work.Terminal.IsFocused);
    }

    // Dispatch a click on the composited cell carrying the target's mouse listener, exactly as ConsoleManager does.
    private static void ClickOn(ConsoleBuffer buf, int width, int height, IMouseListener target)
    {
        for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var mc = buf[x, y].MouseListener;
                if (mc?.MouseListener is { } listener && ReferenceEquals(listener, target))
                {
                    listener.OnMouseDown(mc.Value.RelativePosition);
                    listener.OnMouseUp(mc.Value.RelativePosition);
                    return;
                }
            }
        Assert.Fail("No composited cell carried the target control's mouse listener.");
    }
}
