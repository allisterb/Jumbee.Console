namespace Jumbee.Console;

using System;

using ConsoleGUI.Common;
using ConsoleGUI.Data;
using ConsoleGUI.Space;
using Spectre.Console;

using ConsoleGuiSize = ConsoleGUI.Space.Size;

public abstract class AnimatedControl : Control
{
    #region Constructors
    public AnimatedControl() : base()   
    {
        bufferConsole = new ConsoleBuffer();
        ansiConsole = new AnsiConsoleBuffer(bufferConsole);
    }
    #endregion

    #region Indexers
    public sealed override Cell this[Position position]
    {
        get
        {
            lock (UI.Lock)
            {
                if (bufferConsole.Buffer == null) return _emptyCell;
                if (position.X < 0 || position.X >= Size.Width || position.Y < 0 || position.Y >= Size.Height) return _emptyCell;
                return bufferConsole.Buffer[position.X, position.Y];
            }
        }
    }
    #endregion

    #region Methods
    public void Start()
    {
        if (isRunning) return;
        isRunning = true;
        lastUpdate = DateTime.UtcNow;
        accumulated = TimeSpan.Zero;
    }

    public void Stop()
    {
        if (!isRunning) return;
        isRunning = false;
    }

    public override void Dispose()
    {
        base.Dispose();
        Stop();
    }
    
    protected sealed override void Initialize()
    {
        lock (UI.Lock)
        {
            var targetSize = MaxSize;
            targetSize = new ConsoleGuiSize(Math.Max(0, targetSize.Width), Math.Max(0, targetSize.Height));

            if (targetSize.Width > 1000) targetSize = new ConsoleGuiSize(1000, targetSize.Height);
            if (targetSize.Height > 1000) targetSize = new ConsoleGuiSize(targetSize.Width, 1000);
            Resize(targetSize);
            bufferConsole.Resize(new ConsoleGuiSize(Math.Max(0, Size.Width), Math.Max(0, Size.Height)));
            Paint();
        }
    }

    protected sealed override void OnPaint(object? sender, UI.PaintEventArgs e)
    {
        lock (e.Lock)
        {
            if (!isRunning) return;

            var now = DateTime.UtcNow;
            var delta = now - lastUpdate;
            lastUpdate = now;
            accumulated += delta;
            if (accumulated >= interval)
            {
                accumulated = TimeSpan.Zero;
                frameIndex = (frameIndex + 1) % frameCount;
                Paint();
            }
        }
    }
    #endregion

    #region Fields
    protected readonly ConsoleBuffer bufferConsole;
    protected readonly AnsiConsoleBuffer ansiConsole;
    protected int frameCount = 0;
    protected int frameIndex = 0;    
    protected DateTime lastUpdate;
    protected TimeSpan accumulated;
    protected TimeSpan interval;
    protected bool isRunning = false;
    #endregion
}
