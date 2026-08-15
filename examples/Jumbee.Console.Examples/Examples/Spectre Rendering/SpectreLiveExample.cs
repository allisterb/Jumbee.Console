namespace Jumbee.Console.Examples;

using System;
using System.Collections.Generic;
using System.Threading;

using Spectre.Console;   // for the AddColumn/AddRow extension methods

using S = Spectre.Console;

/// <summary>
/// Spectre's live widgets — a LiveDisplay and a Progress — running at the same time as a fully usable code editor.
/// Plain Spectre.Console can't do this: its live widgets own the console and the input loop until they finish.
/// </summary>
public sealed class SpectreLiveExample : CompositeControl, IActivatableExample
{
    public SpectreLiveExample()
    {
        // SpectreLiveDisplay wraps Spectre's LiveDisplay: it renders into this control's buffer instead of the
        // terminal, so it is just another control in the layout.
        live = new SpectreLiveDisplay(BuildFeedTable(0));

        // SpectreTaskProgress wraps Spectre's Progress the same way. The columns are Spectre's own.
        progress = new SpectreTaskProgress().AddColumns(
            new S.TaskDescriptionColumn(),
            new S.ProgressBarColumn(),
            new S.PercentageColumn(),
            new S.SpinnerColumn());

        editor = new MultiTabCodeEditor(Language.CSharp);
        editor.OpenDocument("Notes.cs",
            "// The widgets above are Spectre.Console's LiveDisplay and Progress, refreshing\n" +
            "// from their own threads — while this editor stays fully interactive.\n" +
            "//\n" +
            "// Click here and type. Arrow keys, selection and undo all work; the tabs\n" +
            "// close with the x and add with the + (Alt+Left/Right switches them).\n" +
            "//\n" +
            "// Under plain Spectre.Console a live widget owns stdout and the input loop\n" +
            "// until its callback returns, so an editor beside it is not an option.\n" +
            "\n" +
            "static class Notes\n" +
            "{\n" +
            "    static void Main() => System.Console.WriteLine(\"try typing here\");\n" +
            "}\n");

        var widgets = new SplitPanel(SplitOrientation.Horizontal,
            live.WithFrame(BorderStyle.Rounded, borderFgColor: Blue).WithTitle("SpectreLiveDisplay — live table", InlineTitle).FocusableControl,
            progress.WithFrame(BorderStyle.Rounded, borderFgColor: Green).WithTitle("SpectreTaskProgress", InlineTitle).FocusableControl,
            splitPosition: 54);

        SetContent(new SplitPanel(SplitOrientation.Vertical,
            widgets,
            editor.WithFrame(BorderStyle.Rounded, borderFgColor: Purple).WithTitle("MultiTabCodeEditor — type while they run", InlineTitle).FocusableControl,
            splitPosition: 13));
    }

    #region IExample
    // Both widgets drive themselves from a background thread, so they run only while the example is on screen.
    // Each Start call returns once its callback does, which is what the token below makes happen.
    void IActivatableExample.OnActivated()
    {
        cancel = new CancellationTokenSource();
        var token = cancel.Token;

        // LiveDisplay: hand it a fresh renderable each tick and refresh. Rebuilding the table off the UI thread is
        // safe — the control renders it to segments on this thread and only the buffer write is marshalled.
        live.Start(ctx =>
        {
            var tick = 0;
            while (!token.IsCancellationRequested)
            {
                ctx.UpdateTarget(BuildFeedTable(++tick));
                ctx.Refresh();
                if (token.WaitHandle.WaitOne(400)) break;
            }
        });

        // Progress: a set of tasks that fill at different rates, then reset so the demo loops.
        progress.Start(ctx =>
        {
            var tasks = new List<S.ProgressTask>
            {
                ctx.AddTask("[green]Indexing symbols[/]"),
                ctx.AddTask("[yellow]Compiling[/]"),
                ctx.AddTask("[skyblue1]Running tests[/]"),
            };
            var rates = new[] { 1.7, 1.1, 0.6 };
            while (!token.IsCancellationRequested)
            {
                for (var i = 0; i < tasks.Count; i++) tasks[i].Increment(rates[i] * rng.NextDouble() * 2);
                if (tasks.TrueForAll(t => t.IsFinished)) tasks.ForEach(t => t.Value = 0);
                if (token.WaitHandle.WaitOne(90)) break;
            }
        });
    }

    // The default OnDeactivated cancels these, which ends both callbacks and releases the widgets.
    IReadOnlyList<CancellationTokenSource> IActivatableExample.FeedTasks => cancel is null ? [] : [cancel];

    string IExample.Category => "Spectre Rendering";
    string IExample.Title => "Spectre Live";
    string IExample.Description =>
        "Spectre's LiveDisplay and Progress refreshing from their own threads — beside a code editor that stays fully interactive, which plain Spectre.Console cannot do.";
    IReadOnlyList<string> IExample.SourceFiles =>
        ["SpectreLiveExample.cs", "SpectreLiveDisplay.cs", "SpectreTaskProgress.cs"];
    #endregion

    #region Methods
    // A "live feed" table, rebuilt each tick with fresh numbers — the sort of thing Spectre's LiveDisplay is for.
    private S.Table BuildFeedTable(int tick)
    {
        var table = new S.Table { Border = S.TableBorder.Minimal, Expand = true };
        table.AddColumn("[grey62]node[/]");
        table.AddColumn(new S.TableColumn("[grey62]req/s[/]").RightAligned());
        table.AddColumn(new S.TableColumn("[grey62]p99[/]").RightAligned());
        table.AddColumn("[grey62]state[/]");
        foreach (var node in Nodes)
        {
            var ms = rng.Next(20, 140);
            var state = ms > 120 ? "[red]degraded[/]" : ms > 80 ? "[yellow]busy[/]" : "[green]healthy[/]";
            table.AddRow(node, $"[white]{rng.Next(200, 1800)}[/]", $"[white]{ms}[/] ms", state);
        }
        table.Caption = new S.TableTitle($"[grey54]refresh #{tick}[/]");
        return table;
    }
    #endregion

    #region Fields
    private readonly SpectreLiveDisplay live;
    private readonly SpectreTaskProgress progress;
    private readonly MultiTabCodeEditor editor;
    private readonly Random rng = new Random(7);
    private CancellationTokenSource? cancel;

    private static readonly string[] Nodes = ["us-east-1", "us-west-2", "eu-west-1", "ap-south-1"];
    private static readonly TitleStyle InlineTitle = new(TitlePos.TopLeft, TitleBorderStyle.Inline);
    private static readonly Color Blue = new(0x5c, 0x9c, 0xff);
    private static readonly Color Green = new(0x8f, 0xd0, 0x66);
    private static readonly Color Purple = new(0xc8, 0x92, 0xf0);
    #endregion
}
