namespace Jumbee.Console.Examples;

using System;
using System.Collections.Generic;
using System.Threading;

using ConsoleGUI.Data;
using ConsoleGUI.Space;

using Jumbee.Console;

using CColor = ConsoleGUI.Data.Color;

/// <summary>
/// Opt-in partial redraw: balls bounce over a static star field and only each ball's old+new cell is redrawn, so the
/// compositor skips the rest (<see cref="Control.TracksDamage"/> + <see cref="Control.Damage"/> — watch the Perf HUD).
/// </summary>
public sealed class BouncingBallsExample : Control, IActivatableExample
{
    public BouncingBallsExample() => Focusable = false;

    #region Rendering
    protected override bool TracksDamage => true;

    protected override void Render()
    {
        int w = Size.Width, h = Size.Height;
        if (w <= 0 || h <= 0) return;

        // First paint / resize: draw the whole scene once and report it all. (A resize already forces a global full
        // redraw, so the DamageAll is belt-and-braces — but it also seeds each ball's "last cell" for the incremental
        // path below.)
        if (w != _w || h != _h)
        {
            _w = w; _h = h;
            consoleBuffer.Initialize();
            BuildBackdrop(w, h);
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    if (_star[x, y]) consoleBuffer.Write(new Position(x, y), StarCell);

            foreach (var b in _balls)
            {
                b.LastX = Clamp((int)Math.Round(b.X), w);
                b.LastY = Clamp((int)Math.Round(b.Y), h);
                consoleBuffer.Write(new Position(b.LastX, b.LastY), new Character(Glyph, b.Color));
            }
            DamageAll();
            return;
        }

        // Incremental, in two passes so balls can pass over one another cleanly. Pass 1 restores the backdrop under
        // every vacated cell (and reports it); pass 2 redraws every ball on top. Reporting a moved ball's OLD and NEW
        // cell (the union) is what keeps it from leaving a trail — the vacated cell is re-composited back to backdrop.
        // Redrawing ALL balls in pass 2 means a stationary ball whose cell was just erased by a passing ball is
        // restored the same frame (that cell is already in the damage set from the erase).
        foreach (var b in _balls)
        {
            int nx = Clamp((int)Math.Round(b.X), w), ny = Clamp((int)Math.Round(b.Y), h);
            if (nx == b.LastX && ny == b.LastY) continue;   // sub-cell move — its cell is unchanged
            consoleBuffer.Write(new Position(b.LastX, b.LastY), _star[b.LastX, b.LastY] ? StarCell : BlankCell);
            Damage(new Rect(b.LastX, b.LastY, 1, 1));
        }

        foreach (var b in _balls)
        {
            int nx = Clamp((int)Math.Round(b.X), w), ny = Clamp((int)Math.Round(b.Y), h);
            consoleBuffer.Write(new Position(nx, ny), new Character(Glyph, b.Color));
            if (nx != b.LastX || ny != b.LastY) { Damage(new Rect(nx, ny, 1, 1)); b.LastX = nx; b.LastY = ny; }
        }
    }

    private void BuildBackdrop(int w, int h)
    {
        _star = new bool[w, h];
        var rng = new Random(1234);
        int count = Math.Max(1, w * h / 22);   // ~4.5% of cells are dim stars
        for (int i = 0; i < count; i++)
            _star[rng.Next(w), rng.Next(h)] = true;
    }

    private static int Clamp(int v, int size) => v < 0 ? 0 : v >= size ? size - 1 : v;
    #endregion

    #region Simulation
    // Integrate + bounce off the walls, then request the repaint that Render turns into per-ball damage.
    private void Step()
    {
        int w = Size.Width, h = Size.Height;
        if (w <= 1 || h <= 1) return;

        foreach (var b in _balls)
        {
            b.X += b.Vx; b.Y += b.Vy;
            if (b.X <= 0) { b.X = 0; b.Vx = Math.Abs(b.Vx); }
            else if (b.X >= w - 1) { b.X = w - 1; b.Vx = -Math.Abs(b.Vx); }
            if (b.Y <= 0) { b.Y = 0; b.Vy = Math.Abs(b.Vy); }
            else if (b.Y >= h - 1) { b.Y = h - 1; b.Vy = -Math.Abs(b.Vy); }
        }
        Invalidate();
    }
    #endregion

    #region Fields
    private const char Glyph = '●';   // ●
    private static readonly Character StarCell = new('·', new CColor(90, 90, 110));   // ·
    private static readonly Character BlankCell = new(' ');

    private readonly Ball[] _balls =
    [
        new(6,  3,  0.7,  0.4, new CColor(235, 90, 90)),
        new(20, 8, -0.5,  0.6, new CColor(90, 200, 120)),
        new(34, 5,  0.6, -0.5, new CColor(90, 160, 240)),
        new(12, 11, 0.4,  0.7, new CColor(235, 200, 90)),
        new(28, 2, -0.6, -0.4, new CColor(200, 130, 230)),
        new(40, 9,  0.5,  0.5, new CColor(110, 205, 220)),
    ];

    private bool[,] _star = new bool[0, 0];
    private int _w = -1, _h = -1;
    #endregion

    #region Types
    private sealed class Ball(double x, double y, double vx, double vy, CColor color)
    {
        public double X = x, Y = y, Vx = vx, Vy = vy;
        public readonly CColor Color = color;
        public int LastX = -1, LastY = -1;
    }
    #endregion

    #region IExample
    // Control.Feed posts each Step to the UI thread; the base IActivatableExample.OnDeactivated cancels it
    // (registered in Feeds) when the example is hidden.
    void IActivatableExample.OnActivated() => Feed(Step, 33);

    IReadOnlyList<CancellationTokenSource> IActivatableExample.FeedTasks => Feeds;

    string IExample.Category => "Graphics";
    string IExample.Title => "Bouncing Balls";
    string IExample.Description =>
        "Balls bounce over a static field; only each ball's old and new cell is redrawn (opt-in partial redraw) — watch the Perf HUD's dirty % stay near zero.";
    #endregion
}
