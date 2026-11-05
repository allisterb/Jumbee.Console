namespace Jumbee.Console.SandboxDemo;

using System.Threading;

/// <summary>What a background folder load produced.</summary>
/// <param name="StartId">The registry id to open on — the file that was named, if it loaded, else the first model
/// that did — or -1 when nothing loaded.</param>
/// <param name="Loaded">How many models were registered.</param>
/// <param name="Failures">Each file that could not be loaded, with the reason.</param>
/// <param name="Stopped">Whether the load was stopped before the end.</param>
public readonly record struct ModelLoadResult(int StartId, int Loaded, (string File, string Reason)[] Failures, bool Stopped);

/// <summary>
/// Loads a folder of models on a background thread behind a modal progress dialog, so the app is up and drawing while
/// the files parse.
/// </summary>
/// <remarks>
/// <para>
/// Parsing is what the viewer spends its startup on — a folder of large models is seconds of work — and it used to
/// happen before <see cref="UI.Start"/>, so all of it was a blank terminal. Now the shell comes up first with the
/// generated knot turning, and the folder loads over it: the modal shows which file is parsing and how far through
/// the folder it is, and the viewer keeps rendering, dimmed, underneath.
/// </para>
/// <para>
/// <b>Registered in one batch at the end, on the UI thread.</b> The loader thread only parses: it never touches the
/// registry, so ids stay in folder order and the only writer is the UI thread (see <see cref="Meshes.Register"/>).
/// </para>
/// <para>
/// <b>Escape stops, and keeps what loaded.</b> It takes effect between files — a parse in progress cannot be
/// interrupted — so the models already parsed are registered a moment after the modal closes.
/// </para>
/// <para>
/// <b>A file that fails is skipped and reported, not fatal.</b> When loading ran before the UI, the first bad file
/// ended the app, because a message and an exit code were all there was. With the viewer up, the rest of the folder
/// is still worth showing, so every failure is collected and listed afterwards — none is silently dropped.
/// </para>
/// </remarks>
public sealed class ModelLoadDialog : CompositeControl
{
    #region Constructors
    private ModelLoadDialog(ModelSet set)
    {
        files = set.Files;
        named = set.Files.Length > 0 ? set.Files[Math.Clamp(set.StartIndex, 0, set.Files.Length - 1)] : null;

        var directory = files.Length > 0 ? Path.GetDirectoryName(Path.GetFullPath(files[0])) ?? "" : "";
        progress = new ProgressBar(null, 0, files.Length)
        {
            Width = Columns,
            ShowSpinner = true,
            TimeDisplay = ProgressTimeDisplay.Elapsed,
        }.WithPadding(1, 1);

        SetContent(new VerticalStackPanel(
            Gap(),
            Label(Fit($"From {directory}", Columns - 2)),
            progress,
            Gap(),
            Label("Esc stops here. Models already loaded are kept.")));

        Focusable = false;
        Width = Columns;
        Height = Rows;
    }
    #endregion

    #region Properties
    /// <summary>Whether a load is in progress. The folder dialog checks it, because its global hotkey still fires
    /// while a modal is up and a second load would race the first.</summary>
    public static bool IsLoading => Volatile.Read(ref loading);
    #endregion

    #region Methods
    /// <summary>
    /// Loads every file in <paramref name="set"/> behind the modal, registers what loaded, and hands the outcome to
    /// <paramref name="done"/> on the UI thread.
    /// </summary>
    /// <remarks>Call on the UI thread, once <see cref="UI.Start"/> is running: the modal needs its overlay.</remarks>
    public static void Load(ModelSet set, Action<ModelLoadResult> done)
    {
        if (set.Files.Length == 0 || IsLoading) return;
        Volatile.Write(ref loading, true);

        var panel = new ModelLoadDialog(set);
        var dialog = new Dialog("Loading models", panel, DialogButtons.None);
        // Escape completes the dialog; so does the Close in Finish, by which time stopping is a no-op.
        dialog.Completed += (_, _) => panel.stop = true;
        dialog.Show();
        panel.progress.Start();

        // A dedicated thread rather than the pool: this runs for seconds, and a pool thread held that long is one the
        // rasteriser's job may be waiting for.
        new Thread(() => panel.Run(dialog, done)) { IsBackground = true, Name = "Model loader" }.Start();
    }
    #endregion

    #region Private methods
    // The loader thread: parse each file, report progress, and hand everything back in one go.
    private void Run(Dialog dialog, Action<ModelLoadResult> done)
    {
        var loaded = new List<(string Path, Mesh Mesh)>(files.Length);
        var failures = new List<(string File, string Reason)>();

        for (var i = 0; i < files.Length && !stop; i++)
        {
            // Quitting mid-load leaves nothing to report to — and UI.Invoke with no UI running would run the update
            // inline, on this thread.
            if (!UI.IsRunning) { Volatile.Write(ref loading, false); return; }

            var (index, path) = (i, files[i]);
            UI.Invoke(() => Report(index, path));
            try
            {
                loaded.Add((path, ModelLoader.Load(path)));
            }
            catch (Exception ex)
            {
                // The expected failures are the file's fault and read as themselves; anything else is a bug, and
                // naming the exception type keeps that visible instead of dressing it up as a bad file.
                var reason = ex is IOException or InvalidDataException or UnauthorizedAccessException
                    ? ex.Message
                    : $"{ex.GetType().Name}: {ex.Message}";
                failures.Add((Path.GetFileName(path), reason));
            }
        }

        if (!UI.IsRunning) { Volatile.Write(ref loading, false); return; }
        var stopped = stop;
        UI.Invoke(() => Finish(dialog, loaded, failures, stopped, done));
    }

    private void Report(int index, string path)
    {
        progress.Value = index;
        progress.Description = $"{index + 1}/{files.Length}  {Path.GetFileName(path)}";
    }

    private void Finish(Dialog dialog, List<(string Path, Mesh Mesh)> loaded, List<(string File, string Reason)> failures,
                        bool stopped, Action<ModelLoadResult> done)
    {
        progress.Value = files.Length;
        progress.Stop();
        dialog.Close(DialogResult.Ok);

        var startId = -1;
        foreach (var (path, mesh) in loaded)
        {
            var id = Meshes.Register(mesh, Path.GetFileNameWithoutExtension(path));
            if (startId < 0 || string.Equals(path, named, StringComparison.OrdinalIgnoreCase)) startId = id;
        }

        Volatile.Write(ref loading, false);
        done(new ModelLoadResult(startId, loaded.Count, [.. failures], stopped));
    }

    private static TextLabel Label(string text) =>
        new(TextLabelOrientation.Horizontal, " " + text, MutedColor) { Height = 1 };

    // A fresh blank each call: a control belongs to one place in a layout.
    private static TextLabel Gap() => new(TextLabelOrientation.Horizontal, "") { Height = 1 };

    // Keeps the END of a long path — the folder's own name is the part worth reading.
    private static string Fit(string text, int width) =>
        text.Length <= width ? text : "…" + text[^(width - 1)..];
    #endregion

    #region Fields
    private const int Columns = 60;
    private const int Rows = 5;

    private static readonly Color MutedColor = new(150, 156, 170);
    private static bool loading;

    private readonly string[] files;
    private readonly string? named;
    private readonly ProgressBar progress;
    private volatile bool stop;
    #endregion
}
