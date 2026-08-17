namespace Jumbee.Console.Tests;

using ConsoleGUI.Input;
using ConsoleGUI.Space;

using Jumbee.Console;
using Jumbee.Console.Snapshot;

using Xunit;

/// <summary>
/// <c>Enabled</c> on the interactive controls: inert to the user, out of the Tab order, visibly muted, and still
/// truthful about the value it carries.
/// </summary>
/// <remarks>
/// That last part is the point of the feature and the easiest to regress. A panel disables a control to say "this
/// is real but you cannot change it here"; a disabled control that blanks its value, or that a caller cannot set
/// in code, is no better than hiding it.
/// </remarks>
public class EnabledTests
{
    public EnabledTests()
    {
        UiTestHarness.EnsureStopped();
        ConsoleSnapshot.ResetMouse();
    }

    private static readonly Position Origin = new(0, 0);

    private static void Click(Control c, Position p)
    {
        var m = (IMouseListener)c;
        m.OnMouseDown(p);
        m.OnMouseUp(p);
    }

    // The same entry point UI dispatch uses, so the control's own HandlesInput/OnInput path is what runs.
    private static void Key(Control c, ConsoleKey key) =>
        c.OnInput(new UI.InputEventArgs(new InputEvent(new ConsoleKeyInfo('\0', key, false, false, false))));

    #region Switch
    [Fact]
    public void Switch_Disabled_IgnoresClickAndKeys()
    {
        var toggle = new Switch("Even over screen") { Enabled = false };
        var changes = 0;
        toggle.Changed += (_, _) => changes++;

        Click(toggle, Origin);
        Key(toggle, ConsoleKey.Spacebar);
        Key(toggle, ConsoleKey.Enter);

        Assert.False(toggle.IsChecked);
        Assert.Equal(0, changes);
    }

    [Fact]
    public void Switch_Disabled_StillReportsItsState()
    {
        // The whole reason to disable rather than hide: it must keep telling the truth.
        var toggle = new Switch("Even over screen", isOn: true) { Enabled = false };
        Assert.True(toggle.IsChecked);

        toggle.IsChecked = false;
        Assert.False(toggle.IsChecked);
        toggle.Toggle();
        Assert.True(toggle.IsChecked);
    }

    [Fact]
    public void Switch_Disabled_RendersDifferently()
    {
        var on = ConsoleSnapshot.Render(new Switch("Detail", isOn: true), 20, 1);
        var off = ConsoleSnapshot.Render(new Switch("Detail", isOn: true) { Enabled = false }, 20, 1);

        // Same text, different colour — a disabled control must not be mistakeable for an enabled one.
        Assert.Equal(ConsoleSnapshot.ToText(on), ConsoleSnapshot.ToText(off));
        Assert.NotEqual(ConsoleSnapshot.ForegroundAt(on, 0, 0), ConsoleSnapshot.ForegroundAt(off, 0, 0));
    }
    #endregion

    #region Select
    [Fact]
    public void Select_Disabled_DoesNotOpenAndKeepsItsValue()
    {
        var select = new Select("alpha", "beta") { SelectedIndex = 1, Enabled = false };
        var changes = 0;
        select.SelectionChanged += (_, _) => changes++;

        Click(select, Origin);
        Key(select, ConsoleKey.Enter);
        Key(select, ConsoleKey.DownArrow);

        Assert.Equal("beta", select.SelectedValue);
        Assert.Equal(0, changes);
    }

    [Fact]
    public void Select_Disabled_StillShowsTheSelectedOption()
    {
        var text = ConsoleSnapshot.ToText(new Select("alpha", "beta") { SelectedIndex = 1, Enabled = false }, 20, 1);
        Assert.Contains("beta", text);
    }

    [Fact]
    public void Select_Disabled_CanStillBeSetInCode()
    {
        var select = new Select("alpha", "beta") { Enabled = false };
        select.SelectedIndex = 1;
        Assert.Equal("beta", select.SelectedValue);
    }
    #endregion

    #region Slider
    [Fact]
    public void Slider_Disabled_IgnoresKeysDragAndWheel()
    {
        var slider = new Slider(0, 10, 5) { Enabled = false, Width = 20, Height = 1 };
        _ = ConsoleSnapshot.Render(slider, 20, 1);   // lay out, so the track geometry is real

        Key(slider, ConsoleKey.RightArrow);
        Key(slider, ConsoleKey.End);
        Click(slider, new Position(19, 0));
        ((IMouseWheelListener)slider).OnMouseWheel(new Position(10, 0), -1);

        Assert.Equal(5, slider.Value);
    }

    [Fact]
    public void Slider_Disabled_StillShowsWhereItIsSet()
    {
        // Flattened, not blanked: the handle has to stay findable or the control stops working as a readout, which
        // is most of what a disabled control is for.
        var low = ConsoleSnapshot.Render(new Slider(0, 10, 0) { Enabled = false, Width = 20, Height = 1 }, 20, 1);
        var high = ConsoleSnapshot.Render(new Slider(0, 10, 10) { Enabled = false, Width = 20, Height = 1 }, 20, 1);
        Assert.NotEqual(ConsoleSnapshot.ToText(low), ConsoleSnapshot.ToText(high));
    }

    [Fact]
    public void Slider_Disabled_CanStillBeSetInCode()
    {
        var slider = new Slider(0, 10, 5) { Enabled = false };
        slider.Value = 8;
        Assert.Equal(8, slider.Value);
    }
    #endregion

    #region Focus
    [Theory]
    [MemberData(nameof(Interactive))]
    public void Disabled_LeavesTheTabOrder(Control control)
    {
        Assert.True(control.Focusable);

        SetEnabled(control, false);
        Assert.False(control.Focusable);

        SetEnabled(control, true);
        Assert.True(control.Focusable);
    }

    [Theory]
    [MemberData(nameof(Interactive))]
    public void Disabled_GivesUpFocusItAlreadyHeld(Control control)
    {
        control.IsFocused = true;
        Assert.True(control.IsFocused);

        SetEnabled(control, false);
        Assert.False(control.IsFocused);
    }

    [Theory]
    [MemberData(nameof(Interactive))]
    public void ReEnabling_DoesNotOverrideAnExplicitlyUnfocusableControl(Control control)
    {
        // Disabling remembers what Focusable was, so a control the caller deliberately kept out of the Tab order
        // does not quietly join it by being disabled and enabled again.
        control.Focusable = false;

        SetEnabled(control, false);
        SetEnabled(control, true);

        Assert.False(control.Focusable);
    }

    public static TheoryData<Control> Interactive() =>
    [
        new Switch("s"),
        new Checkbox("c"),
        new RadioButton("r"),
        new Select("alpha", "beta"),
        new Slider(0, 10, 5),
    ];

    private static void SetEnabled(Control control, bool enabled)
    {
        switch (control)
        {
            case ToggleButton t: t.Enabled = enabled; break;
            case Select s: s.Enabled = enabled; break;
            case Slider s: s.Enabled = enabled; break;
            default: Assert.Fail($"no Enabled on {control.GetType().Name}"); break;
        }
    }
    #endregion
}
