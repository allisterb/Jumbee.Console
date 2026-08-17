namespace Jumbee.Console;

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Spectre.Console.Rendering;

/// <summary>
/// A control that implements Spectre.Console.IRenderable
/// </summary>
public abstract class RenderableControl : Control, IRenderable
{
    /// <summary>Initializes a new <see cref="RenderableControl"/>.</summary>
    public RenderableControl() : base() {}
    
    #region Methods
    Measurement IRenderable.Measure(RenderOptions options, int maxWidth) => this.Measure(options, Math.Min(maxWidth, ActualWidth));

    IEnumerable<Segment> IRenderable.Render(RenderOptions options, int maxWidth) => this.Render(options, maxWidth);

    /// <summary>Produces the Spectre.Console <see cref="Segment"/>s for the control's content within <paramref name="maxWidth"/>.</summary>
    protected abstract IEnumerable<Segment> Render(RenderOptions options, int maxWidth);

    /// <summary>Measures the control's desired width; the default reports <paramref name="maxWidth"/> as both minimum and maximum. Override for intrinsic sizing.</summary>
    [DebuggerStepThrough]
    protected virtual Measurement Measure(RenderOptions options, int maxWidth) => new Measurement(maxWidth, maxWidth);

    /// <summary>
    /// Whether this control's rendered output depends on interactive state (focus / mouse hover / press) — i.e.
    /// whether <see cref="Render(RenderOptions, int)"/> reads <see cref="Control.IsFocused"/>, <c>IsMouseOver</c>,
    /// or <c>IsMousePressed</c>.
    /// </summary>
    /// <remarks>
    /// When <see langword="false"/>, focus/mouse changes skip the (expensive) Spectre re-render and reuse the cached
    /// buffer — the retained-mode fast path. Defaults to <see langword="true"/> (always re-render), so controls that
    /// highlight on hover/focus keep working without opting in.
    /// </remarks>
    protected virtual bool RendersInteractiveState => true;

    // Content changes go through Invalidate(), which marks the buffer stale so the next paint re-runs the Spectre
    // pipeline. Interactive-state changes route through InvalidateInteractive() below, which (for content-only
    // controls) requests a repaint WITHOUT re-rendering.
    /// <summary>Marks the content stale (so the next paint re-runs the Spectre pipeline) and requests a repaint.</summary>
    protected override void Invalidate()
    {
        _contentDirty = true;
        base.Invalidate();
    }

    /// <summary>Requests an interactive-state repaint: re-renders when <see cref="RendersInteractiveState"/> is set, otherwise reuses the cached buffer for a cheap composite.</summary>
    protected override void InvalidateInteractive()
    {
        if (RendersInteractiveState)
        {
            Invalidate();
        }
        else
        {
            // Output is independent of focus/hover: keep the cached render, just request the cheap repaint/composite.
            base.Invalidate();
        }
    }

    /// <summary>Lays the control out on the UI thread: measures the renderable, resizes the control and buffer when the size changed, and invalidates.</summary>
    protected override void Initialize()
    {
        UI.Invoke(() =>
        {
            var (width, height) = CalculateSize();

            // Create RenderOptions based on the virtual console and max width and height
            var options = new RenderOptions(ansiConsole.Profile.Capabilities, new Spectre.Console.Size(width, height));

            // Determine Spectre.Console control measurement
            var measurement = this.Measure(options, width);
            
            // Resize the ConsoleGUI control — but only when the size actually changed. Initialize is re-run for every
            // control on any size-limit change (and repeatedly during layout convergence); ConsoleGUI's Resize
            // unconditionally sets a full dirty rect and propagates an Update to the parent, so an unguarded call
            // cascades a re-layout up the tree on every pass even when nothing moved. CalculateSize already clamps to
            // Min/MaxSize, so an unchanged computed size means a genuine no-op. The Invalidate below still requests the
            // control's own repaint.
            var size = new ConsoleGUI.Space.Size(width, height);
            if (size != Size)
                Resize(size);

            // Update buffer size
            consoleBuffer.Size = Size;

            Invalidate();        
        });
    }

    /// <summary>Renders the control's content to the console buffer.</summary>
    protected sealed override void Render()
    {
        // Retained-mode fast path: re-run the Spectre pipeline only when the content (or size/theme) actually
        // changed. Interactive-state-only repaints (hover/focus on a content-only control) leave _contentDirty
        // false, so the cached cells already in consoleBuffer are reused — see InvalidateInteractive.
        if (!_contentDirty)
        {
            return;
        }

        _contentDirty = false;
        ansiConsole.Clear(true);
        // We probably want to render with the full width of the control
        // Spectre will look at the Profile.Width which comes from the IConsole.Size (BufferConsole.Size)
        ansiConsole.Write(this);
    }

    /// <summary>
    /// Applies the focus consequences of an interactive control's <c>Enabled</c> state: a disabled control leaves
    /// the Tab order, and gives up focus if it currently holds it.
    /// </summary>
    /// <remarks>
    /// Shared because it is the half of "disabled" that cannot be expressed by an override: focus navigation
    /// collects candidates by <see cref="Control.Focusable"/>, which is not virtual, so leaving the Tab order means
    /// actually clearing it. The caller's own setting is remembered and restored, so a control deliberately made
    /// unfocusable does not silently become focusable by being disabled and enabled again.
    /// </remarks>
    protected void ApplyEnabledToFocus(bool enabled)
    {
        if (enabled)
        {
            Focusable = _focusableWhenEnabled;
        }
        else
        {
            _focusableWhenEnabled = Focusable;
            Focusable = false;
            if (IsFocused) UnFocus();
        }
    }
    #endregion

    #region Fields
    // True when the wrapped renderable must be re-rendered into consoleBuffer on the next paint. Set by Invalidate
    // (content/size/theme changes); cleared after a render. Starts true so the first paint renders.
    private bool _contentDirty = true;

    // What Focusable was before this control was disabled. See ApplyEnabledToFocus.
    private bool _focusableWhenEnabled = true;
    #endregion
}
