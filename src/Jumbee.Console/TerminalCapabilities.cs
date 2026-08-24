namespace Jumbee.Console;

using System;
using System.Runtime.InteropServices;

/// <summary>
/// What the terminal this process is attached to can actually do — currently just the one question that matters
/// before the first frame: may we emit ANSI escape sequences, or must we drive the console through Win32 calls?
/// </summary>
/// <remarks>
/// <para>
/// <b>Windows consoles do not interpret escape sequences unless asked.</b> A console starts with
/// <c>ENABLE_VIRTUAL_TERMINAL_PROCESSING</c> off; an application turns it on with <c>SetConsoleMode</c>, and from
/// then on conhost renders escapes rather than printing them. When the user has ticked <b>Use legacy console</b> —
/// a per-machine setting, not per-window — that call is refused with <c>ERROR_INVALID_PARAMETER</c> and there is no
/// way to make escapes work at all.
/// </para>
/// <para>
/// So enabling and detecting are the same operation, and it has to happen before anything is drawn. Assuming ANSI
/// and finding out otherwise is not a degraded picture, it is no picture: every sequence lands on screen as literal
/// text (<c>[38;2;148;148;148m</c>, <c>[59;1H</c>) and the UI is unreadable from the first frame.
/// </para>
/// <para>
/// A <b>redirected</b> stdout is deliberately NOT treated as legacy. <c>GetConsoleMode</c> fails on a pipe or a
/// file, which says nothing about what will eventually read the bytes — a recording, a Docker log, a test harness
/// capturing frames. Those want the escapes, so an inconclusive probe leaves the caller's choice alone. Only a
/// console that is present and refuses is a downgrade.
/// </para>
/// </remarks>
internal static class TerminalCapabilities
{
    #region Methods
    /// <summary>
    /// Turns on VT output processing when the console supports it, and reports whether ANSI may be used.
    /// </summary>
    /// <returns>
    /// <see langword="false"/> only when this is a real Windows console that <b>refused</b> VT processing;
    /// <see langword="true"/> on every other platform, and whenever the probe is inconclusive (see the remarks on
    /// redirected output).
    /// </returns>
    public static bool TryEnableAnsiOutput()
    {
        if (!OperatingSystem.IsWindows()) return true;

        try
        {
            var handle = GetStdHandle(StdOutputHandle);
            if (handle == IntPtr.Zero || handle == InvalidHandle) return true;

            // Not a console (redirected to a pipe or a file) — inconclusive, so leave the caller's choice alone.
            if (!GetConsoleMode(handle, out var mode)) return true;

            if ((mode & EnableVirtualTerminalProcessing) != 0) return true;   // already on; nothing to restore

            if (!SetConsoleMode(handle, mode | EnableVirtualTerminalProcessing))
            {
                // ERROR_INVALID_PARAMETER here is the "Use legacy console" signature. There is no fallback that
                // makes escapes work, so the renderer must take the Win32 path instead.
                return false;
            }

            // Remember that WE turned it on, so Restore can put the console back as we found it. The mode belongs
            // to the console, which outlives this process and is shared with the shell that launched us.
            _originalMode = mode;
            _modeChanged = true;
            _handle = handle;
            return true;
        }
        catch
        {
            // P/Invoke unavailable or the handle vanished — inconclusive rather than negative.
            return true;
        }
    }

    /// <summary>
    /// Puts the console mode back if <see cref="TryEnableAnsiOutput"/> changed it.
    /// </summary>
    /// <remarks>
    /// <b>Must be the last thing done to the terminal.</b> Restoring the mode turns escape interpretation back off,
    /// so any sequence written after this point — an SGR reset, showing the cursor, leaving the alternate screen —
    /// would be printed as literal text instead of obeyed.
    /// </remarks>
    public static void Restore()
    {
        if (!_modeChanged) return;
        _modeChanged = false;
        try { SetConsoleMode(_handle, _originalMode); } catch { /* best effort */ }
    }
    #endregion

    #region Fields
    private const int StdOutputHandle = -11;
    private const uint EnableVirtualTerminalProcessing = 0x0004;
    private static readonly IntPtr InvalidHandle = new(-1);

    private static IntPtr _handle;
    private static uint _originalMode;
    private static bool _modeChanged;
    #endregion

    #region Interop
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
    #endregion
}
