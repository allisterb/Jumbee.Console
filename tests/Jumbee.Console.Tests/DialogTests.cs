namespace Jumbee.Console.Tests;

using System;
using System.Collections.Generic;

using ConsoleGUI.Input;

using Jumbee.Console;
using Jumbee.Console.Snapshot;

using Xunit;

/// <summary>Tests for the modal <see cref="Dialog"/>: predefined-button results, keyboard navigation/activation,
/// Escape/cancel, and hosting a custom control with exclusive focus.</summary>
public class DialogTests
{
    private static ConsoleKeyInfo K(ConsoleKey k, bool shift = false) => new('\0', k, shift, false, false);

    // A host overlay with a simple focusable bottom layer, laid out and set as the ambient UI.Overlay.
    private static Overlay Host()
    {
        var bottom = new Grid([1], [50], [[new TextInput(placeholder: "bg")]]);
        var overlay = new Overlay(bottom);
        UI.Overlay = overlay;
        ConsoleSnapshot.Render(overlay, 60, 20);
        return overlay;
    }

    // Route a key through the root overlay exactly like the live input path.
    private static void Send(Overlay overlay, ConsoleKeyInfo key)
    {
        overlay.OnInput(new UI.InputEventArgs(new InputEvent(key)));
        ConsoleSnapshot.Render(overlay, 60, 20);
    }

    #region Opacity + sizing
    [Fact]
    public void CustomContent_SizesToItsContent_NotATallEmptyBox()
    {
        var overlay = Host();
        var field = new TextInput("report.pdf");     // one row
        var d = new Dialog("Rename file", field, DialogButtons.OkCancel);
        d.Show();
        ConsoleSnapshot.Render(overlay, 62, 18);

        Assert.Equal(1 + 2, d.ActualHeight);   // input row + button bar (spacer + buttons); shrunk from the default
    }

    [Fact]
    public void Interior_IsOpaque_NoScrimBleedsThrough()
    {
        // Every interior cell must carry a background (the surface), so the dimmed layer behind can't show through a
        // transparent gap (short content, spacing, a glyph with no background).
        var overlay = Host();
        var field = new TextInput();                  // empty -> its row has cells with no background of their own
        var d = new Dialog("Rename", field, DialogButtons.OkCancel);
        d.Show();
        ConsoleSnapshot.Render(overlay, 62, 18);

        for (var y = 0; y < d.ActualHeight; y++)
            for (var x = 0; x < d.ActualWidth; x++)
                Assert.NotNull(d[new ConsoleGUI.Space.Position(x, y)].Character.Background);
    }

    [Fact]
    public void Frame_ReservesNoScrollbarGutter()
    {
        var overlay = Host();
        var d = new Dialog("Title", "A short message.", DialogButtons.Ok);
        d.Show();
        ConsoleSnapshot.Render(overlay, 62, 18);

        // The frame adds only the border (no reserved vertical-scrollbar column), so there's no blank right gutter.
        Assert.Equal(d.ActualWidth + 2, d.Frame!.Size.Width);
    }
    #endregion

    #region Results
    [Fact]
    public void Confirm_EnterOnYes_ReturnsTrue_AndCloses()
    {
        var overlay = Host();
        bool? result = null;
        Dialog.Confirm("Delete?", "Delete the file?", r => result = r);
        ConsoleSnapshot.Render(overlay, 60, 20);

        Assert.True(overlay.IsShowing);
        Assert.True(overlay.IsModal);

        Send(overlay, K(ConsoleKey.Enter));   // first button (Yes) is focused by default

        Assert.True(result);
        Assert.False(overlay.IsShowing);       // dismissed
    }

    [Fact]
    public void Confirm_RightThenEnter_ChoosesNo()
    {
        var overlay = Host();
        bool? result = null;
        Dialog.Confirm("Delete?", "Delete the file?", r => result = r);
        ConsoleSnapshot.Render(overlay, 60, 20);

        Send(overlay, K(ConsoleKey.RightArrow));   // move to "No"
        Send(overlay, K(ConsoleKey.Enter));

        Assert.False(result);
    }

    [Fact]
    public void Escape_CancelsWithTheButtonSetsCancelResult()
    {
        var overlay = Host();
        DialogResult result = DialogResult.None;
        var d = new Dialog("Title", "Body text", DialogButtons.OkCancel);
        d.Completed += (_, r) => result = r;
        d.Show();
        ConsoleSnapshot.Render(overlay, 60, 20);

        Send(overlay, K(ConsoleKey.Escape));

        Assert.Equal(DialogResult.Cancel, result);
        Assert.False(overlay.IsShowing);
    }

    [Fact]
    public void Message_Ok_ReportsOk()
    {
        var overlay = Host();
        DialogResult result = DialogResult.None;
        var d = new Dialog("Info", "All done.", DialogButtons.Ok);
        d.Completed += (_, r) => result = r;
        d.Show();
        ConsoleSnapshot.Render(overlay, 60, 20);

        Send(overlay, K(ConsoleKey.Enter));

        Assert.Equal(DialogResult.Ok, result);
    }
    #endregion

    #region Rendering
    [Fact]
    public void RendersTitleMessageAndButtons()
    {
        var overlay = Host();
        new Dialog("Confirm", "Are you sure you want to continue?", DialogButtons.YesNo).Show();

        var text = ConsoleSnapshot.ToText(overlay, 60, 20);

        Assert.Contains("Confirm", text);                       // title in the border
        Assert.Contains("Are you sure", text);                  // wrapped message
        Assert.Contains("Yes", text);
        Assert.Contains("No", text);
    }
    #endregion

    #region Custom content modal
    [Fact]
    public void CustomContent_TakesExclusiveFocus_AndRoutesInput()
    {
        var overlay = Host();
        var input = new TextInput();
        var d = new Dialog("Rename", input, DialogButtons.OkCancel);
        d.Show();
        ConsoleSnapshot.Render(overlay, 60, 20);

        // The custom content is the default focus stop; typing reaches it (exclusive modal focus).
        Send(overlay, new ConsoleKeyInfo('h', ConsoleKey.H, false, false, false));
        Send(overlay, new ConsoleKeyInfo('i', ConsoleKey.I, false, false, false));

        Assert.Equal("hi", input.Text);
        Assert.True(overlay.IsShowing);   // still open (typing didn't dismiss)
    }

    [Fact]
    public void CustomContent_TabMovesFocusToButtons()
    {
        var overlay = Host();
        var input = new TextInput();
        DialogResult result = DialogResult.None;
        var d = new Dialog("Rename", input, DialogButtons.OkCancel);
        d.Completed += (_, r) => result = r;
        d.Show();
        ConsoleSnapshot.Render(overlay, 60, 20);

        Send(overlay, K(ConsoleKey.Tab));     // content -> button bar
        Send(overlay, K(ConsoleKey.Enter));   // activate the focused (OK) button

        Assert.Equal(DialogResult.Ok, result);
    }

    // Regression: the dialog used to treat ANY loss of focus as a dismissal. Clicking a field inside it moves focus
    // to that field — a nested composite is its own focus unit — so the dialog silently completed with its cancel
    // result on the first click, and every button afterwards was a no-op with the dialog still on screen.
    [Fact]
    public void ClickingInsideTheContent_DoesNotDismissTheDialog()
    {
        ConsoleSnapshot.ResetMouse();
        var overlay = Host();

        var list = new ListBox("alpha", "beta");
        var content = new FormContent(list);
        var results = new List<DialogResult>();
        var d = new Dialog("Pick", content, DialogButtons.OkCancel);
        d.Completed += (_, r) => results.Add(r);
        d.Show();

        var buffer = ConsoleSnapshot.Render(overlay, 60, 20);
        var lines = ConsoleSnapshot.ToLines(buffer);
        var row = Array.FindIndex(lines, l => l.Contains("beta"));
        Assert.True(row >= 0, "the list content did not render");
        Assert.True(ConsoleSnapshot.Click(buffer, lines[row].IndexOf("beta", StringComparison.Ordinal), row));

        Assert.Empty(results);              // still open, nothing reported
        Assert.True(overlay.IsShowing);

        // And the buttons still work afterwards, which is the part that was actually dead.
        buffer = ConsoleSnapshot.Render(overlay, 60, 20);
        lines = ConsoleSnapshot.ToLines(buffer);
        var okRow = Array.FindIndex(lines, l => l.Contains("OK"));
        Assert.True(ConsoleSnapshot.Click(buffer, lines[okRow].IndexOf("OK", StringComparison.Ordinal), okRow));

        Assert.Equal([DialogResult.Ok], results);
        Assert.False(overlay.IsShowing);
    }

    // A composite wrapping one focusable child — the shape the docs recommend for dialog content, and the shape
    // that exposed the bug.
    private sealed class FormContent : CompositeControl
    {
        public FormContent(Control child)
        {
            Height = 4;
            SetContent(new VerticalStackPanel(child));
        }
    }
    #endregion
}
