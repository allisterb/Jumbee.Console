namespace Jumbee.Console;

using System;

/// <summary>A run of content rows: <paramref name="Start"/> and the <paramref name="Height"/> rows following it.</summary>
/// <param name="Start">The first row, in the control's own content coordinates.</param>
/// <param name="Height">How many rows the run covers. Values below 1 are treated as 1.</param>
public readonly record struct RowSpan(int Start, int Height = 1);

/// <summary>
/// A control whose content can be taller than the space it is given, and which therefore wants its
/// <see cref="ControlFrame"/> to scroll it.
/// </summary>
/// <remarks>
/// <para>
/// Scrolling is the frame's job: it owns the viewport, the scrollbar and the offset. A frame gives an
/// <see cref="IScrollable"/> child an <b>unbounded</b> height so the content can grow past the visible area, reserves
/// a column for the scrollbar, and moves a window over the result. A control that does <b>not</b> implement this
/// interface is given the bounded viewport height and is never scrolled — that is the default, and it is the right
/// one for controls that fit their space or manage their own viewport (<see cref="Log"/>, <see cref="DataTable"/>,
/// <see cref="TerminalEmulator"/>).
/// </para>
/// <para>
/// Implementing it costs one method, and that method is the whole contract: report the content height and the
/// frame's scroll range and scrollbar follow from it. When that height changes, re-lay-out with
/// <c>Initialize()</c> — not merely <c>Invalidate()</c>, which only repaints — so the frame re-measures.
/// </para>
/// </remarks>
public interface IScrollable
{
    /// <summary>The control's content height in rows at the given <paramref name="width"/> — the frame's scroll range.</summary>
    /// <remarks>
    /// Measure the content, not the viewport: a list returns its item count, a text control its wrapped row count.
    /// Returning the visible height instead defeats the purpose, leaving a scrollbar that never moves.
    /// </remarks>
    int MeasureHeight(int width);

    /// <summary>
    /// Raised when the control's point of interest — a selected item, a caret — moves, carrying the content rows it
    /// now occupies. The wrapping <see cref="ControlFrame"/> subscribes and scrolls them into view.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Declare it as a plain field-like event and raise it wherever the selection moves:
    /// <code>
    /// public event EventHandler&lt;RowSpan&gt;? FocusRowChanged;
    /// private void Select(int i) { _index = i; FocusRowChanged?.Invoke(this, new RowSpan(RowOf(i))); }
    /// </code>
    /// Written that way the compiler reports <c>CS0067</c> if it is never raised, which is the mistake worth
    /// catching: a control that says it has a moving selection and then leaves it to scroll off screen.
    /// </para>
    /// <para>
    /// The default implementation does nothing, so a control with no moving point of interest — a document viewer,
    /// a panel of static text — simply omits it. Scrolls that are not selection moves (following new output to the
    /// bottom, restoring a saved position) are not what this is for; call
    /// <see cref="ControlFrame.ScrollIntoView"/> directly instead.
    /// </para>
    /// </remarks>
    event EventHandler<RowSpan>? FocusRowChanged { add { } remove { } }
}
