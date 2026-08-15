namespace Jumbee.Console.Tests;

using ConsoleGUI.Input;

using Jumbee.Console;
using Jumbee.Console.Snapshot;

using Xunit;

// Reproduces the examples-browser middle pane: a content-swapping composite whose live content is a framed control,
// with a focusable-but-non-input description label docked above it. Focusing a child resolves to the composite (the
// navigable unit), which delegates focus back to a child — so the composite must delegate to the child that was
// actually asked for, not to the first focusable descendant it claimed (here the label, which would swallow the
// keys). Overriding FocusChild pins the content regardless of what was asked for.
public class CompositeFocusChildTests
{
    private sealed class PaneHost : CompositeControl
    {
        private readonly Control _content;
        private readonly bool _override;

        public PaneHost(Control content, bool overrideFocusChild)
        {
            _content = content;
            _override = overrideFocusChild;
            // Force the docked label focusable to model a focusable-but-non-input sibling (a hazard the composite
            // must tolerate). Passive display controls are non-focusable by default now, but a composite shouldn't
            // rely on that to route keys to its real content.
            var header = new TextLabel(TextLabelOrientation.Horizontal, "description") { Focusable = true };
            content.WithFrame(borderStyle: BorderStyle.None);
            SetContent(new DockPanel(DockedControlPlacement.Top, header, content.Frame!));
        }
        protected override Control? FocusChild => _override ? _content : base.FocusChild;
    }

    private static ConsoleKeyInfo Down => new('\0', ConsoleKey.DownArrow, false, false, false);

    // Builds root(DockPanel) > SplitPanel > host(composite) > DockPanel(label, Framed(list)), like the browser shell.
    private static (ListBox list, ILayout root) BuildPane(bool overrideFocusChild)
    {
        var list = new ListBox("alpha", "beta", "gamma");
        var host = new PaneHost(list, overrideFocusChild);
        host.WithFrame(title: "Example");
        var split = new SplitPanel(SplitOrientation.Horizontal, host, new ListBox("other"), splitPosition: 20);
        var status = new TextLabel(TextLabelOrientation.Horizontal, "status");
        var root = new DockPanel(DockedControlPlacement.Bottom, status, split);
        ConsoleSnapshot.Render(root, 40, 12);   // establish layout + register controls for focus
        return (list, root);
    }

    [Fact]
    public void FocusingContent_ReachesIt_DespiteAnEarlierFocusableSibling()
    {
        var (list, root) = BuildPane(overrideFocusChild: false);

        list.Focus();   // resolves to the host, which must delegate back to the list — not to the label it claimed first
        root.OnInput(new UI.InputEventArgs(new InputEvent(Down)));

        Assert.Equal(1, list.SelectedIndex);   // DownArrow reached the list
    }

    [Fact]
    public void FocusChildOverride_RoutesArrowKeysToTheContent()
    {
        var (list, root) = BuildPane(overrideFocusChild: true);

        list.Focus();
        root.OnInput(new UI.InputEventArgs(new InputEvent(Down)));

        Assert.Equal(1, list.SelectedIndex);   // DownArrow moved the selection — keyboard navigation works
    }
}
