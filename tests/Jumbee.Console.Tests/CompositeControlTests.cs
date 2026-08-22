namespace Jumbee.Console.Tests;

using System;

using Jumbee.Console;
using Jumbee.Console.Snapshot;

using Xunit;

public class CompositeControlTests
{
    #region Rendering / layout
    [Fact]
    public void CodeEditor_RendersGutterAndCode()
    {
        var ed = new CodeEditor { Text = "class Foo\n{\n}" };

        var text = ConsoleSnapshot.ToText(ed, 30, 5);

        Assert.Contains("class Foo", text);   // the editor's content
        Assert.Contains("1", text);           // gutter numbers the lines
        Assert.Contains("3", text);
    }
    #endregion

    #region Runtime resize
    // Setting Height at runtime used to call Resize() directly, which changed the composite's Size but skipped
    // everything Initialize does around it -- so OnInitialization never fired and the content kept its OLD size
    // forever, drawing a stale picture into a correctly-sized control.
    //
    // The Wolf3D status bar hit this on startup: control 237x15, content still 237x50, so the bar rendered the top
    // 15 rows of a 50-row picture with its numbers cut off, and only a terminal resize (which forces a real layout
    // pass from the parent) put it right. It never reproduced through ConsoleSnapshot because ToText re-lays-out
    // the whole tree on every call, which papers the missing propagation over -- hence the explicit second render
    // here with no other layout trigger in between.
    [Fact]
    public void SettingHeightAtRuntime_ResizesTheContentToMatch()
    {
        // Docked, not standalone: a control rendered on its own is pinned by the snapshot's min=max limits, so its
        // requested height is clamped straight back and nothing can move. A Bottom dock is the shape the bug was
        // found in and the one that actually lets the height change.
        var editor = new CodeEditor { Text = "one\ntwo\nthree" };
        var root = new DockPanel(DockedControlPlacement.Bottom, editor, new TextEditor());
        _ = ConsoleSnapshot.ToText(root, 40, 20);

        editor.Height = 6;

        // The invariant, not the number: the content must describe the same area the control reports. It was
        // control 237x15 over content 237x50 in the live fault, and the absolute values depend on what the parent
        // allows -- the mismatch is the bug.
        Assert.Equal(6, editor.ActualHeight);
        Assert.Equal(editor.ActualHeight, editor.ContentLayout!.CControl.Size.Height);
        Assert.Equal(editor.ActualWidth, editor.ContentLayout!.CControl.Size.Width);
    }

    // The same setter also paired the new width with the REQUESTED height, which is 0 whenever a control fills its
    // parent -- so setting Width alone could collapse the height to nothing.
    [Fact]
    public void SettingWidthAtRuntime_DoesNotCollapseTheHeight()
    {
        var editor = new CodeEditor { Text = "one\ntwo\nthree" };
        var root = new DockPanel(DockedControlPlacement.Right, editor, new TextEditor());
        _ = ConsoleSnapshot.ToText(root, 40, 20);
        var before = editor.ActualHeight;

        editor.Width = 25;

        Assert.Equal(25, editor.ActualWidth);
        Assert.Equal(before, editor.ActualHeight);
    }
    #endregion

    #region Inter-child wiring
    [Fact]
    public void CodeEditor_GutterTracksEditorLineCount()
    {
        var ed = new CodeEditor();

        ed.Text = "one\ntwo\nthree\nfour";   // 4 lines -> editor.Changed -> gutter sync

        Assert.Equal(4, ed.Editor.LineCount);
        Assert.Equal(4, ed.Gutter.LineCount);
    }

    [Fact]
    public void CodeEditor_GutterIsNotFocusable()
    {
        var ed = new CodeEditor();
        Assert.False(ed.Gutter.Focusable);
    }

    [Fact]
    public void CodeEditor_Gutter_AlignsWithSoftWrappedLines()
    {
        var ed = new CodeEditor { Text = "this is a long first line that wraps\nsecond" };

        var rows = ConsoleSnapshot.ToText(ed, 24, 6).TrimEnd('\n').Split('\n');

        Assert.StartsWith(" 1 ", rows[0]);   // logical line 1
        Assert.StartsWith("   ", rows[1]);   // its wrapped continuation -> blank gutter
        Assert.StartsWith(" 2 ", rows[2]);   // logical line 2
    }

    [Fact]
    public void CodeEditor_SameLineTyping_LeavesGutterValid_ButLineChangeInvalidatesIt()
    {
        // Partial-invalidation: the editor re-renders on every keystroke, but the gutter should only be repainted
        // when what it draws (line numbers / active-row highlight) actually changes. Typing within a line must
        // leave the gutter valid (no pending paint request); adding a line must invalidate it.
        var ed = new CodeEditor(Language.None) { Text = "abc" };   // a single logical line
        ed.WithRoundedBorder();
        ed.Editor.Focus();

        ConsoleSnapshot.Render(ed, 28, 8);                 // establish layout; everything painted + validated
        Assert.Equal(0u, PaintRequests(ed.Gutter));        // sanity: gutter clean after a render

        // A within-line edit: same numbers, same active row -> the gutter is not touched.
        UI.SendInput(ed.FocusedControl!, new ConsoleKeyInfo('x', ConsoleKey.X, false, false, false));
        Assert.Equal(0u, PaintRequests(ed.Gutter));        // gutter stayed valid (not re-rendered)

        ConsoleSnapshot.Render(ed, 28, 8);                 // re-validate
        // A new line changes the line count (and an extra label) -> the gutter must be invalidated.
        UI.SendInput(ed.FocusedControl!, new ConsoleKeyInfo('\0', ConsoleKey.Enter, false, false, false));
        Assert.True(PaintRequests(ed.Gutter) > 0u);        // gutter invalidated by the line-count change
    }

    /// <summary>Reads a control's pending paint-request count (the private Control.paintRequests) for asserting
    /// that a clean control was not needlessly invalidated.</summary>
    private static uint PaintRequests(Control c) =>
        (uint)typeof(Control)
            .GetField("paintRequests", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(c)!;

    [Fact]
    public void CodeEditor_ScrollsToKeepCaretVisible_WithGutterAndScrollbar()
    {
        var lines = new string[15];
        for (var i = 0; i < 15; i++) lines[i] = $"line {i + 1:00}";
        var ed = new CodeEditor { Text = string.Join("\n", lines) };
        ed.WithRoundedBorder();   // a frame scrolls the content-sized composite (gutter + text together)
        ed.Editor.Focus();

        ConsoleSnapshot.Render(ed, 18, 9);
        for (var i = 0; i < 14; i++)   // from the top, drive the caret + auto-scroll to the bottom line
            UI.SendInput(ed.FocusedControl!, new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false));

        var text = ConsoleSnapshot.ToText(ed, 18, 9);

        Assert.Equal(15, ed.Editor.ActualHeight);   // composite/editor sized to content, not the 1000-row fill
        Assert.Contains("15 line 15", text);        // scrolled to the bottom, gutter number aligned with its line
        Assert.DoesNotContain("line 01", text);     // the top scrolled out of view
    }

    [Fact]
    public void CodeEditor_MouseWheelOverEditor_ScrollsTheFrame()
    {
        var lines = new string[15];
        for (var i = 0; i < 15; i++) lines[i] = $"line {i + 1:00}";
        var ed = new CodeEditor { Text = string.Join("\n", lines) };
        ed.WithRoundedBorder();   // the frame is what scrolls; the editor itself isn't framed

        ConsoleSnapshot.Render(ed, 18, 9);
        Assert.Equal(0, ed.Frame!.Top);   // opens at the top

        // A wheel notch over the editor (its cells carry the editor's listener) bubbles to the composite's frame.
        ((ConsoleGUI.Input.IMouseWheelListener)ed.Editor).OnMouseWheel(new ConsoleGUI.Space.Position(0, 0), 3);

        Assert.True(ed.Frame!.Top > 0);   // scrolled down without moving the caret
    }

    [Fact]
    public void CodeEditor_OpensAtTheTop_WithCaretOnFirstChar()
    {
        var lines = new string[20];
        for (var i = 0; i < 20; i++) lines[i] = $"line {i + 1:00}";
        var ed = new CodeEditor { Text = string.Join("\n", lines) };
        ed.WithRoundedBorder();
        ed.Editor.Focus();

        var text = ConsoleSnapshot.ToText(ed, 18, 9);

        Assert.Equal(0, ed.Editor.CaretIndex);    // caret on the first character, not at the end of the file
        Assert.Contains("line 01", text);         // the top is visible on load
        Assert.DoesNotContain("line 20", text);   // not scrolled to the bottom
    }
    #endregion

    #region Fluid layout (resizes with the console window)
    // A framed CodeEditor as the fill control of a DockPanel — the canonical "fluid" arrangement: a header/status
    // bar docked to one edge and the editor flowing into whatever space the console leaves. Resizing is simulated
    // by rendering the same tree at a different size (ConsoleSnapshot.Render re-runs layout via the new limits).
    private static DockPanel DockedEditor(out CodeEditor ed, DockedControlPlacement placement, string text,
        Language lang = Language.CSharp)
    {
        ed = new CodeEditor(lang) { Text = text };
        ed.WithRoundedBorder();   // framed so it scrolls its content within the docked fill region
        var bar = new TextLabel(TextLabelOrientation.Horizontal, "STATUS") { Height = 1 };   // a 1-row status bar
        return new DockPanel(placement, bar, ed);
    }

    [Fact]
    public void CodeEditor_InDockPanel_FillsAreaBelowHeader()
    {
        var dock = DockedEditor(out var ed, DockedControlPlacement.Top, "class Foo\n{\n}");

        var rows = ConsoleSnapshot.ToText(dock, 30, 10).TrimEnd('\n').Split('\n');

        Assert.StartsWith("STATUS", rows[0]);             // header docked at the top
        Assert.DoesNotContain("class Foo", rows[0]);      // editor does NOT bleed into the header row
        Assert.Contains("class Foo", string.Join("\n", rows));   // editor fills the rest
        // The editor's frame takes the full width and everything below the 1-row header.
        Assert.Equal(30, ed.Frame!.Size.Width);
        Assert.Equal(9, ed.Frame!.Size.Height);
    }

    [Fact]
    public void DockPanel_AutoSizesIntrinsicHeaderWithoutExplicitHeight()
    {
        // A docked TextLabel with NO explicit Height must size to its intrinsic 1 row (it reports an intrinsic
        // height) rather than ballooning to fill the panel and collapsing the editor's fill region to zero.
        var ed = new CodeEditor(Language.CSharp) { Text = "class Foo\n{\n}" };
        ed.WithRoundedBorder();
        var bar = new TextLabel(TextLabelOrientation.Horizontal, "STATUS");   // no explicit Height
        var dock = new DockPanel(DockedControlPlacement.Top, bar, ed);

        var rows = ConsoleSnapshot.ToText(dock, 30, 10).TrimEnd('\n').Split('\n');

        Assert.Equal(30, bar.ActualWidth);    // status bar fills the width...
        Assert.Equal(1, bar.ActualHeight);    // ...but stays one row tall
        Assert.Equal(30, ed.Frame!.Size.Width);
        Assert.Equal(9, ed.Frame!.Size.Height);   // editor gets everything below the 1-row header
        Assert.StartsWith("STATUS", rows[0]);
        Assert.Contains("class Foo", string.Join("\n", rows));
    }

    [Fact]
    public void CodeEditor_InDockPanel_ReflowsWhenConsoleWidens()
    {
        var line = "the quick brown fox jumps over the lazy dog again and again and again and again";
        var dock = DockedEditor(out var ed, DockedControlPlacement.Top, line, Language.None);

        ConsoleSnapshot.Render(dock, 24, 12);   // narrow console window
        var narrowWidth = ed.Editor.ActualWidth;
        var narrowRows = ed.Editor.VisualRowCount(narrowWidth);

        ConsoleSnapshot.Render(dock, 60, 12);   // user widens the console window
        var wideWidth = ed.Editor.ActualWidth;
        var wideRows = ed.Editor.VisualRowCount(wideWidth);

        Assert.True(wideWidth > narrowWidth,
            $"editor should grow with the console (narrow {narrowWidth} -> wide {wideWidth})");
        Assert.True(wideRows < narrowRows,
            $"the long line should reflow into fewer wrapped rows when wider (narrow {narrowRows} -> wide {wideRows})");
    }

    [Fact]
    public void CodeEditor_InDockPanel_ScrollsToCaretAfterResize()
    {
        var lines = new string[20];
        for (var i = 0; i < 20; i++) lines[i] = $"row {i + 1:00}";
        var dock = DockedEditor(out var ed, DockedControlPlacement.Top, string.Join("\n", lines));
        ed.Editor.Focus();

        ConsoleSnapshot.Render(dock, 20, 8);    // lay out at one size...
        ConsoleSnapshot.Render(dock, 26, 12);   // ...then resize before scrolling

        for (var i = 0; i < 19; i++)            // drive the caret to the last line at the new size
            UI.SendInput(ed.FocusedControl!, new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false));

        var text = ConsoleSnapshot.ToText(dock, 26, 12);

        Assert.Contains("row 20", text);        // auto-scrolled to the bottom against the resized viewport
        Assert.DoesNotContain("row 01", text);  // the top scrolled out of view
    }

    [Fact]
    public void CodeEditor_InDockPanel_BottomStatusBar_StaysPinnedAcrossResizes()
    {
        var dock = DockedEditor(out _, DockedControlPlacement.Bottom, "class Foo\n{\n}");

        foreach (var (w, h) in new[] { (28, 8), (44, 14), (22, 20) })
        {
            var rows = ConsoleSnapshot.ToText(dock, w, h).TrimEnd('\n').Split('\n');

            Assert.StartsWith("STATUS", rows[^1]);                       // status bar pinned to the bottom row
            Assert.Contains("class Foo", string.Join("\n", rows));       // editor fills the area above it
            Assert.DoesNotContain("class Foo", rows[^1]);                // and never overlaps the status row
        }
    }
    #endregion

    #region Focus + input routing
    [Fact]
    public void FocusedControl_SurfacesFocusedChild()
    {
        var ed = new CodeEditor();
        Assert.Null(ed.FocusedControl);   // nothing focused yet

        ed.Editor.Focus();

        Assert.Same(ed.Editor, ed.FocusedControl);   // composite surfaces the focused descendant
    }

    [Fact]
    public void FocusingComposite_DelegatesFocusToItsChild()
    {
        var ed = new CodeEditor();

        UI.SetFocus(ed);                          // focus the composite (the navigable unit, e.g. via Ctrl+arrows)

        Assert.True(ed.IsFocused);                // the composite itself is focused (so a frame would light up)
        Assert.True(ed.Editor.IsFocused);         // and it delegated focus inward to its editor child
        Assert.Same(ed.Editor, ed.FocusedControl);
    }

    [Fact]
    public void FocusingInnerChild_ResolvesToOwningComposite()
    {
        // Click-to-focus / Control.Focus() target the inner child; focus must resolve UP to the owning composite
        // so mouse and keyboard agree on the focused unit (and the composite's frame reflects it).
        var ed = new CodeEditor();

        ed.Editor.Focus();

        Assert.True(ed.IsFocused);                // resolved up to the composite...
        Assert.True(ed.Editor.IsFocused);         // ...which delegated focus back to the child
    }

    [Fact]
    public void FocusingFramedComposite_FocusesItsFrame()
    {
        var ed = new CodeEditor();
        ed.WithSquareBorder();

        ed.Editor.Focus();

        Assert.True(ed.Frame!.IsFocused);         // the frame reflects focus, so its border restyles when focused
    }

    [Fact]
    public void Input_RoutedThroughFocusedControl_ReachesEditor()
    {
        var ed = new CodeEditor();
        ed.Editor.Focus();

        // This is exactly what Layout.OnInput does: deliver to the composite's FocusedControl.
        var target = ed.FocusedControl!;
        UI.SendInput(target, new ConsoleKeyInfo('X', ConsoleKey.X, shift: false, alt: false, control: false));

        Assert.Contains("X", ed.Text);
    }

    [Fact]
    public void Input_RoutesIntoFramedComposite_ReachesEditor()
    {
        // The demo frames the composite; the parent layout then sees the ControlFrame, which must delegate focus
        // down to the composite's focused descendant. Regression guard for the "framed composite ignores keys" bug.
        var ed = new CodeEditor();
        ed.WithRoundedBorder();
        ed.Editor.Focus();

        Assert.Same(ed.FocusableControl, ed.FocusableControl.FocusedControl);   // the frame is the routing node

        // UI.SendInput delivers to ed.FocusableControl (the frame), mirroring the live input path.
        UI.SendInput(ed, new ConsoleKeyInfo('Z', ConsoleKey.Z, shift: false, alt: false, control: false));

        Assert.Contains("Z", ed.Text);
    }
    #endregion
}
