namespace Jumbee.Console;

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
}
