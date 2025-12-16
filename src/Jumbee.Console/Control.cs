namespace Jumbee.Console;

using System;
using System.Threading;
using System.Threading.Tasks;

using ConsoleGUI.Data;

public abstract class Control : ConsoleGUI.Common.Control, IDisposable    
{
    #region Constructor
    public Control() : base()
    {
        UI.Paint += OnPaint;
    }
    #endregion

    #region Methods
    public virtual void Dispose()
    {
        UI.Paint -= OnPaint;
    }

    protected abstract void Render();

    /// <summary>
    /// Handles the paint event triggered by the UI timer.
    /// </summary>
    /// <remarks>
    /// This method tries to implement thread-safe painting by locking on the provided synchronization object.
    /// If one or more paint requests are pending, it runs the painting process and resets the paint request count.
    /// </remarks>
    /// <param name="sender">The source of the event. This parameter can be <see langword="null"/>.</param>
    /// <param name="e">An instance of <see cref="PaintEventArgs"/> containing event data, including a synchronization lock.</param>
    protected virtual void OnPaint(object? sender, UI.PaintEventArgs e)
    {
        lock (e.Lock)
        {
            if (paintRequests > 0)
            {
                Paint();
                Interlocked.Exchange(ref paintRequests, 0u);
            }
        }
    }

    protected void Paint()
    {
        Render();
        Redraw();
    }

    /// <summary>
    /// Indicates the control should be repainted on the next UI update tick.
    /// </summary>    
    protected void Invalidate() => Interlocked.Increment(ref paintRequests);
    #endregion

    #region Fields
    protected static readonly Cell _emptyCell = new Cell(Character.Empty);
    protected internal uint paintRequests;    
    #endregion
}
