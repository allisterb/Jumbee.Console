namespace Jumbee.Console;

using System;

using ConsoleGUI.Api;
using ConsoleGUI.Data;
using ConsoleGUI.Space;

/// <summary>
/// A ConsoleGUI.IConsole implementation that writes to a buffer.
/// </summary>
public class ConsoleBuffer : IConsole, IConsoleBuffer
{
    #region Properties
    /// <summary>The logical size of the buffer; setting it resizes the backing cell arrays.</summary>
    public Size Size
    {
        get => field;
        set
        {
            if (field == value) return;   // no size change -> the backing arrays already fit; skip the row scan/realloc
            Resize(value);
            field = value;
        }
    }
    /// <summary>Always <see langword="false"/>; input is handled by the input listeners, not this buffer.</summary>
    public bool KeyAvailable => false;
    #endregion

    #region Indexers
    /// <summary>Gets the cell at <paramref name="position"/>.</summary>
    public Cell this[Position position] => buffer[position.Y][position.X];

    /// <summary>Gets the cell at column <paramref name="x"/>, row <paramref name="y"/>.</summary>
    public Cell this[int x, int y] => buffer[y][x];
    #endregion

    #region Methods
    /// <summary>
    /// Fill buffer with empty/transparent cells.
    /// </summary>
    public void Initialize()
    {
        for (int y = 0; y < Size.Height; y++)
        {
            // Rows may be wider than the logical width (capacity is retained across shrinks — see Resize), so blank
            // only the live columns; the rest are never read.
            Array.Fill(buffer[y], emptyCell, 0, Size.Width);
        }
    }

    /// <summary>No-op; the buffer is composited by the renderer rather than refreshing itself.</summary>
    public void OnRefresh() { }

    /// <summary>
    /// Sets the console buffer cell character.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="character"></param>
    public void Write(Position position, in Character character) => buffer[position.Y][position.X] = new Cell(character);

    /// <summary>Writes a character at column <paramref name="x"/>, row <paramref name="y"/> — the
    /// <see cref="IConsoleBuffer"/> render-target entry point (used by controls that draw straight into the buffer).</summary>
    public void Write(int x, int y, in Character character) => buffer[y][x] = new Cell(character);

    /// <summary>The <see cref="Character"/> at (<paramref name="x"/>, <paramref name="y"/>) — a cell read without the
    /// surrounding <see cref="Cell"/> (no mouse-listener copy), for a hot draw loop reading its own output back.</summary>
    public Character CharacterAt(int x, int y) => buffer[y][x].Character;
        
    
    /// <summary>
    /// Sets the console buffer cell character.
    /// </summary>
    public void Write(in int X, in int Y, in Cell cell) => buffer[Y][X] = cell;

    /// <summary>
    /// Sets the console buffer cell character.
    /// </summary>
    public void Write(Position position, in Cell cell) => buffer[position.Y][position.X] = cell;

    /// <summary>
    /// Will be handled by IInputListeners.
    /// </summary>
    /// <returns></returns>
    public ConsoleKeyInfo ReadKey() => throw new NotImplementedException();
    
    /// <summary>Converts a linear cell <paramref name="distance"/> (row-major) into an (x, y) <see cref="Position"/>.</summary>
    public Position GetPosition(int distance)
    {
        if (Size.Width == 0)
        {
            return new Position(0, 0);
        }
        int x = distance % Size.Width;
        int y = distance / Size.Width;
        return new Position(x, y);
    }

    /// <summary>Advances <paramref name="pos1"/> by <paramref name="x"/> cells in row-major order, wrapping across rows.</summary>
    public Position AddX(Position pos1, int x)
    {
        if (Size.Width == 0)
        {
            return new Position(0, 0);
        }

        int linear_pos1 = pos1.Y * Size.Width + pos1.X;
        int total_linear_distance = linear_pos1 + x;

        return GetPosition(total_linear_distance);
    }

    /// <summary>
    /// Resizing the control dimensions resizes the console buffer.
    /// </summary>
    /// <param name="size"></param>
    protected void Resize(Size size)
    {
        // Capacity-retentive: only ever grow the backing arrays. A control whose size fluctuates (layout convergence,
        // variable content) previously reallocated the whole Cell[][] whenever its width/height changed by even one
        // column — the largest per-frame allocation in the profiler. Retaining capacity makes a shrink free and a
        // grow-back within the high-water mark reuse the existing arrays; the high-water is bounded by the screen, so
        // this is not a leak.
        int oldW = Size.Width, oldH = Size.Height;   // current logical size (field is updated by the caller after this)

        // Grow to a rounded-up CAPACITY, not the exact width/height. Otherwise a drag/resize that widens a pane one
        // column at a time reallocates every row on every single-cell step (100 -> 101 -> 102 ...), since each is a new
        // maximum — retention alone only saves the shrink. Bucketing (what an ArrayPool does internally) collapses a
        // whole growth sweep into one realloc per bucket boundary. Logical Size stays exact; the slack is never read.
        // Grow the row count to fit, but also SHRINK it when the retained capacity dwarfs what's needed. A layout
        // convergence pass can briefly measure content at a near-zero width — a document-length wrapped-row count — and
        // balloon the buffer to thousands of full-width rows; without this, retention would pin that transient
        // overshoot in memory. Height is content-driven (a divider drag changes it only slightly), so the 2x slack
        // never churns during normal resizing. Row WIDTH keeps pure retention below — a divider drag sweeps the width
        // across a wide range, and each row is cheap to hold.
        int capHeight = RoundUpCapacity(size.Height);
        if (buffer.Length < size.Height || buffer.Length > capHeight * 2)
            Array.Resize(ref buffer, capHeight);
        for (int i = 0; i < size.Height; i++)
        {
            if (buffer[i] is null || buffer[i].Length < size.Width)
                Array.Resize(ref buffer[i], RoundUpCapacity(size.Width));
        }

        // Blank the cells that are newly inside the logical bounds but sat outside the previous ones — new columns of
        // existing rows, and every column of newly live rows. Without this, a grow-back into retained capacity would
        // expose stale glyphs from when the buffer was last that large (a fresh Array.Resize used to zero them). The
        // overlap with the old bounds is left intact, matching the previous copy-on-resize behaviour.
        for (int y = 0; y < size.Height; y++)
        {
            int clearFrom = y < oldH ? oldW : 0;
            if (clearFrom < size.Width)
                Array.Fill(buffer[y], emptyCell, clearFrom, size.Width - clearFrom);
        }
    }

    // Rounds a required length up to the next capacity bucket, so growth reallocates in chunks rather than on every
    // one-cell change. 64 halves the bucket-boundary crossings of a drag sweep versus 32, for a little more slack
    // (<=63 cells per row) — a good trade since the churn, not the retained size, was the cost.
    private const int CapacityChunk = 64;
    private static int RoundUpCapacity(int n) => n <= 0 ? 0 : (n + CapacityChunk - 1) & ~(CapacityChunk - 1);
    #endregion

    #region Fields
    private static readonly Cell emptyCell = new Cell(' ');
    private Cell[][] buffer = [];
    #endregion
}
