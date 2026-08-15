namespace Jumbee.Console.Examples;

using System.Collections.Generic;

/// <summary>
/// The middle pane: a composite whose content is swapped to the selected example's live control. Swapping is just
/// re-calling <see cref="CompositeControl.SetContent"/> (it disposes the old content context and rebuilds). The demo
/// is placed in a <see cref="DockPanel"/>'s fill slot (the fill slot expands to the pane — a fixed-size Grid cell
/// would clip it), with a one-line description docked above it.
/// </summary>
public sealed class ExampleHost : CompositeControl
{
    #region Methods
    /// <summary>Shows <paramref name="content"/> (a control or layout) as the pane's live example under a one-line
    /// <paramref name="description"/>, replacing whatever was there.</summary>
    public void Show(IFocusable content, string description)
    {
        // Live examples (IActivatableExample) start/stop their feed as they're shown/replaced, so it only runs while visible.
        if (!ReferenceEquals(_active, content))
        {
            (_active as IActivatableExample)?.OnDeactivated();
            (content as IActivatableExample)?.OnActivated();
            _active = content;
        }

        // The description is a passive label, not a focus target — keep it out of focus/Tab traversal so it can't
        // become the composite's focus child (which would swallow the example's keyboard input).
        var header = new TextLabel(TextLabelOrientation.Horizontal, description) { Focusable = false };
        SetContent(new DockPanel(DockedControlPlacement.Top, header, Framed(content)));
    }

    /// <summary>Stops the active example's background work (its <see cref="IActivatableExample"/> feed) — called on app quit
    /// so a live example's timers/threads don't keep running through shutdown. Idempotent.</summary>
    public void DeactivateActive() => (_active as IActivatableExample)?.OnDeactivated();

    // Not IScrollable: the host never scrolls itself. A scrollable example scrolls inside its own inner frame (see
    // Framed), with a single scrollbar; making the host scrollable too would add a second, redundant scroller.

    // Delegate focus to the CURRENT example, not the composite's default "first focusable descendant" — which is set
    // once and never tracks the swapped-in content, so it would stick to a stale/detached control after the first
    // switch. Clicking or tabbing into the pane (both resolve to this host as the focus unit) therefore focuses the
    // live example, so keyboard input routes to it. A layout example (not a Control) falls back to the default.
    protected override Control? FocusChild => _active as Control;

    // A scrollable Control (ListBox, editors…) needs a ControlFrame to scroll — the example is a bare control, so we
    // give it a borderless frame here (the pane's own frame supplies the visible border/title). Layouts arrange their
    // own children and pass through. Framed once per example instance and cached, so re-selecting keeps scroll state.
    private IFocusable Framed(IFocusable content)
    {
        if (content is not Control control) return content;
        if (!_framed.TryGetValue(control, out var framed))
            _framed[control] = framed = control.WithFrame(borderStyle: BorderStyle.None);
        return framed;
    }
    #endregion

    #region Fields
    private readonly Dictionary<Control, IFocusable> _framed = new();
    private IFocusable? _active;
    #endregion
}
