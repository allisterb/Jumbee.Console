namespace Jumbee.Console;

using System;

using ConsoleGUI.Api;
using ConsoleGUI.Data;
using ConsoleGUI.Space;

/// <summary>
/// An <see cref="IConsoleBuffer"/> wrapper that records which cells a plot's draw actually <em>changed</em>, so
/// <see cref="Plot"/> can report partial damage instead of its whole rect. Only used when
/// <see cref="Plot.DamageTracking"/> is on.
/// </summary>
/// <remarks>
/// <para>
/// It compares each cell's value at the END of a draw against its value at the START, not at each write. A per-write
/// comparison would over-report everything drawn: the renderer clears before it draws, so a cell holding the same
/// glyph two frames running is still written twice — once blank, once back to its old value — and both writes differ
/// from what was there a moment earlier. Deferring the comparison is what lets the (unchanged) grid, axes and tick
/// labels cost nothing in damage even though they are rewritten every frame.
/// </para>
/// <para>
/// The deferral is only affordable because a draw no longer touches every cell (see <c>ConsoleImage.ClearDrawn</c>
/// in the ConsolePlot fork): the touched set is the previous frame's content plus this frame's, so the flush is
/// O(content), not O(area).
/// </para>
/// <para>
/// Damage is emitted as one rect per changed row, spanning its first-to-last change — a trace is a run of cells per
/// row, so a row span bounds it tightly and there are at most <c>height</c> rects, which keeps a normal plot under
/// the compositor's dirty-rect collapse threshold.
/// </para>
/// </remarks>
internal sealed class DamageBuffer : IConsoleBuffer
{
    #region Constructors
    public DamageBuffer(IConsoleBuffer inner)
    {
        _inner = inner;
        var size = inner.Size;
        _width = size.Width;
        _height = size.Height;
        var cells = Math.Max(0, _width * _height);
        _saved = new Character[cells];
        _isSaved = new bool[cells];
        _touched = new int[cells];
        _limit = Math.Max(1, cells / 2);
        _rowFirst = new int[Math.Max(0, _height)];
        _rowLast = new int[Math.Max(0, _height)];
    }
    #endregion

    #region Properties
    public Size Size => _inner.Size;
    #endregion

    #region Methods
    public Character CharacterAt(int x, int y) => _inner.CharacterAt(x, y);

    public void Write(int x, int y, in Character character)
    {
        // Past the limit the figure is dense enough that damage would cover most of the rect anyway, so the diff is
        // paying for a saving that isn't there. Stop recording and let the caller fall back to reporting everything;
        // this bounds the worst case (a full-scale trace) instead of letting it become a straight loss.
        if (_tracking && !_hitLimit)
        {
            var index = (y * _width) + x;
            // First touch this frame: remember what was there BEFORE the draw started, which is the previous frame's
            // final value. Later writes to the same cell leave the saved value alone.
            if ((uint)index < (uint)_isSaved.Length && !_isSaved[index])
            {
                _isSaved[index] = true;
                _saved[index] = _inner.CharacterAt(x, y);
                _touched[_touchedCount++] = index;
                if (_touchedCount >= _limit) _hitLimit = true;
            }
        }

        _inner.Write(x, y, character);
    }

    /// <summary>Starts a new draw: forgets the previous frame's touched set.</summary>
    /// <remarks>
    /// A frame that gave up gets no bookkeeping at all for the next <see cref="ReprobeFrames"/> frames. Without
    /// that, a figure which is persistently too dense (a signal sitting at full scale) would pay to rediscover the
    /// fact on every frame — half the surface saved, then discarded. Re-probing on a slow cycle costs one frame of
    /// overhead now and then, and lets a figure that thins out start tracking again on its own.
    /// </remarks>
    public void BeginFrame()
    {
        for (var i = 0; i < _touchedCount; i++) _isSaved[_touched[i]] = false;
        _touchedCount = 0;
        _hitLimit = false;
        _tracking = _skipFrames == 0;
        if (!_tracking) _skipFrames--;
    }

    /// <summary>
    /// Compares every touched cell's current value against what it held before the draw and collects the changes
    /// into one row-span per changed row. Returns how many rows changed; read them with <see cref="ChangedRow"/>.
    /// Returns <c>-1</c> when the draw was too dense to track and the caller should report everything.
    /// </summary>
    public int Flush()
    {
        if (!_tracking || _hitLimit)
        {
            if (_hitLimit) _skipFrames = ReprobeFrames;
            return -1;
        }

        var rows = 0;
        for (var i = 0; i < _touchedCount; i++)
        {
            var index = _touched[i];
            var x = index % _width;
            var y = index / _width;
            if (_inner.CharacterAt(x, y) == _saved[index]) continue;   // rewritten with the same value: not damage

            if (_rowFirst[y] < 0) { _rowFirst[y] = x; _rowLast[y] = x; rows++; }
            else
            {
                if (x < _rowFirst[y]) _rowFirst[y] = x;
                if (x > _rowLast[y]) _rowLast[y] = x;
            }
        }
        return rows;
    }

    /// <summary>The changed span of row <paramref name="y"/>, or <see langword="false"/> if it did not change.
    /// Valid until the next <see cref="ResetRows"/>.</summary>
    public bool ChangedRow(int y, out int first, out int last)
    {
        first = _rowFirst[y];
        last = _rowLast[y];
        return first >= 0;
    }

    /// <summary>Clears the per-row spans, ready for the next <see cref="Flush"/>.</summary>
    public void ResetRows()
    {
        for (var y = 0; y < _height; y++) { _rowFirst[y] = -1; _rowLast[y] = -1; }
    }
    #endregion

    #region Fields
    private readonly IConsoleBuffer _inner;
    private readonly int _width;
    private readonly int _height;
    // Per cell: the value before this frame's first write, and whether it has been saved yet. `_touched` keeps the
    // saved cells enumerable in O(touched) — scanning `_isSaved` would reinstate the O(area) cost this avoids.
    private readonly Character[] _saved;
    private readonly bool[] _isSaved;
    private readonly int[] _touched;
    private int _touchedCount;
    // Half the surface: past that, damage would cover most of the rect anyway, so the diff is paying for a saving
    // that isn't there. Measured on the 220x53 live scope (`-- --damage`): a full-scale trace dirties every cell
    // with or without tracking, so the composite saves nothing while the bookkeeping still costs.
    //
    // Note what the limit and the re-probe below can NOT recover: this buffer sits in the plot's write chain, so
    // even with tracking fully disabled every cell write pays one extra indirection through here. On the dense
    // workload that residual tracked the whole observed paint overhead (~370us over ~20k writes/frame), which is
    // why the honest advice on Plot.DamageTracking is to leave it OFF for a figure that fills its area, rather than
    // to rely on these guards making it free.
    private readonly int _limit;
    private const int ReprobeFrames = 60;
    private bool _tracking = true;
    private bool _hitLimit;
    private int _skipFrames;
    private readonly int[] _rowFirst;
    private readonly int[] _rowLast;
    #endregion
}
