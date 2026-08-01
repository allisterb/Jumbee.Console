namespace Jumbee.Console.TestDemo;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using ConsoleGUI;
using ConsoleGUI.Input;

using Vezel.Cathode.Text.Control;


using Jumbee.Console;
using static Jumbee.Console.Style;


public class Program
{
    static async Task Main(string[] args)
    {
        //MultiTabCodeEditorDemo(args);
        //TerminalDemo(args);
        //NavigationDemo(args);
        //TabsDemo(args);
        //DynamicTabsDemo(args);
        //CodeEditorDemo(args);
        //LinkDemo(args);
        //WidgetGalleryDemo(args);
        //ToggleDemo(args);
        //SelectDemo(args);
        //OverlayDemo(args);
        //WheelDemo(args);
        //ButtonDemo(args);
        //GridTest(args);
        //GridTest(args);
        //SpectreControlTests.LiveDisplayTests();
        //InputDemo(args);
        //InputsDemo(args);
        //DialogDemo(args);
        //ChatPromptDemo(args);
        PostingDemo(args);
        //DynamicTabsDemo(args);
        //MultiTabCodeEditorDemo(args);
        //ConsoleStudioDemo(args);
        //DockPanelTest(args);
        //TitleStyleTest(args);
        //ScrollBarStyleTest(args);
        //TreeAutoScrollTest(args);
        //SpectreControlTests.ProgressTests();
        Console.Clear();
        Console.WriteLine("Average UI draw time: {0}ms. Average UI paint time: {1}ms.", UI.AverageDrawTime, UI.AveragePaintTime);
        Console.WriteLine("Average control paint times:");
        foreach(var c in UI.AverageControlPaintTimes)
        {
            Console.WriteLine("{0}: {1}ms", c.Key.GetType().Name, c.Value);
        }
        Console.WriteLine("Max control paint times:");
        foreach (var c in UI.MaxControlPaintTimes)
        {
            Console.WriteLine("{0}: {1}ms", c.Key.GetType().Name, c.Value);
        }

        var m = UI.ProcessMetrics;
        Console.WriteLine($"Peak render: {m.RenderTimeMsPeak * 1000:F0} µs/frame, busy {m.BusyPercentPeak:F0}%");
        Console.WriteLine($"CPU Usage: {m.CpuUsagePercent:F1}% (whole process, matches Task Manager)");
        Console.WriteLine($"Working Set: {m.WorkingSetBytes / 1024.0 / 1024:F1} MB");
        Console.WriteLine($"Managed Heap: {m.ManagedHeapBytes / 1024.0 / 1024:F1} MB");
        Console.WriteLine($"Alloc/frame: {m.AllocatedBytesPerFrame / 1024:F1} KB (peak {m.PeakAllocatedBytesPerFrame / 1024.0:F1} KB)");
        Console.WriteLine($"GC pause: {m.GcPausePercent:F2}% (gen0 {m.Gen0Collections}, gen1 {m.Gen1Collections}, gen2 {m.Gen2Collections})");
        Console.WriteLine($"ThreadPool: {m.ThreadPoolThreadCount} threads, {m.ThreadPoolQueueLength} queued");
        Console.WriteLine($"Exceptions/s: {m.ExceptionsPerSecond:F1}");
        Console.WriteLine($"Lock Contentions: {m.LockContentions}");
    }

    // Step C demo: the VT input pipeline end-to-end. Click an editor to focus it (mouse), type, and paste
    // (bracketed paste arrives as one chunk via TextEditor.OnPaste). Ctrl+Q quits.
    static void InputDemo(string[] args)
    {
        var editor1 = new TextEditor();
        var editor2 = new TextEditor();
        var grid = new Grid(
            [10, 10],
            [70],
            [
                [editor1.WithFrame(title: "Editor 1 — click to focus, type, paste (Ctrl+Q quits)")],
                [editor2.WithFrame(title: "Editor 2 — click to focus")],
            ]);

        var run = UI.Start(grid, width: 72, height: 22, isAnsiTerminal: false);
        UI.SetFocus(editor1);
        run.Wait();
    }

    // Interactive Select/dropdown demo. Click the selector (or focus it and press Enter/Space) to open the
    // options anchored below; arrow keys + Enter or a click choose; Escape / click-outside cancels. The chosen
    // value shows in the status line. Needs a VT terminal (e.g. Windows Terminal).
    static void SelectDemo(string[] args)
    {
        var status = new TextLabel(TextLabelOrientation.Horizontal, "Pick a colour from the dropdown.".PadRight(36), Color.White);
        var select = new Select("Red", "Green", "Blue", "Magenta", "Cyan", "Yellow") { Placeholder = "Choose a colour ▾" };

        var bottom = new Jumbee.Console.Grid([2, 1], [38], [[status], [select]]);
        // UI.Start wraps the root in UI.Overlay; the dropdown floats into it automatically (no wiring needed).
        select.SelectionChanged += (_, value) => status.Text = $"You picked: {value}".PadRight(36);

        var run = UI.Start(bottom, width: 42, height: 14, isAnsiTerminal: true, input: new Jumbee.Console.VtInputSource(anyMotion: true));
        UI.SetFocus(select);
        run.Wait();
    }

    // Phase 2 display-widget gallery: a Digits clock, a Sparkline, and a colour Log. A background loop ticks the
    // clock, shifts the sparkline, and appends log lines once a second to show the live-update path (UI.Post from
    // a worker thread).
    static void WidgetGalleryDemo(string[] args)
    {
        var clock = new Digits(DateTime.Now.ToString("HH:mm:ss"));

        var sparkData = new List<double> { 3, 5, 2, 8, 6, 7, 4, 2, 5, 9 };
        // AsciiBars renders on a legacy console (cmd.exe). In Windows Terminal you can drop this and the default
        // block ramp (▁▂▃▄▅▆▇█) looks crisper.
        var spark = new Sparkline(sparkData.ToArray()) { BarStyle = Cyan1, Bars = Sparkline.AsciiBars };



        var log = new Log();
        
        log.Write("[green]OK[/]   gallery started");

        var grid = new Jumbee.Console.Grid(
            [10, 3, 10],
            [54],
            [                
                [clock],                
                [spark],
                [log],
            ]);

        var n = 1;
        var rnd = new Random();
        _ = Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(1000);
                UI.Post(() =>
                {
                    clock.Text = DateTime.Now.ToString("HH:mm:ss");
                    sparkData.Add(rnd.Next(1, 10));
                    if (sparkData.Count > 10) sparkData.RemoveAt(0);
                    spark.Values = sparkData.ToArray();
                    log.Write($"tick {n} at {DateTime.Now:HH:mm:ss}");
                    if (n % 3 == 0) log.Write($"[grey]heartbeat[/] {n}");
                    n++;
                });
            }
        });

        var run = UI.Start(grid, width: 58, height: 24, isAnsiTerminal: true, input: new Jumbee.Console.VtInputSource());
        run.Wait();
    }

    // Phase 3 demo: two clickable Links (each opens a URL in the system browser on click or Enter/Space while
    // focused). Focus moves by clicking a link or pressing Tab (wired below via a global hotkey — the framework
    // has no built-in Tab traversal); Esc quits. A plain TextLabel serves as the key-hint line. Needs a VT terminal.
    static void LinkDemo(string[] args)
    {
        var status = new TextLabel(TextLabelOrientation.Horizontal, "Tab between links, Enter/click opens, Esc quits.".PadRight(52), Color.White);

        var docs = new Link("Open the Spectre.Console website", "https://spectreconsole.net");
        var repo = new Link("Open the ConsoleGUI repository", "https://github.com/TomaszRewak/C-sharp-console-gui-framework");
        docs.Activated += (_, _) => status.Text = "Opened spectreconsole.net".PadRight(52);
        repo.Activated += (_, _) => status.Text = "Opened the ConsoleGUI repo".PadRight(52);

        var hints = new TextLabel(TextLabelOrientation.Horizontal, "Tab Focus   Enter Open   Esc Quit", Color.Grey);

        var spacer = new TextLabel(TextLabelOrientation.Horizontal, "", Color.White);
        var grid = new Jumbee.Console.Grid(
            [2, 1, 1, 5, 1],
            [60],
            [
                [status],
                [docs],
                [repo],
                [spacer],
                [hints],
            ]);

        // Tab cycles focus across the links (click-to-focus also works); Esc quits. These are global hotkeys,
        // dispatched before the focused control sees the key.
        var links = new[] { docs, repo };
        var names = new[] { "docs", "repo" };
        var focus = 0;
        UI.RegisterHotKey(UI.HotKeys.Tab, () =>
        {
            focus = (focus + 1) % links.Length;
            UI.SetFocus(links[focus]);
            status.Text = $"Focused the {names[focus]} link (Enter to open).".PadRight(52);
        });
        UI.RegisterHotKey(UI.HotKeys.Escape, UI.Stop);

        var run = UI.Start(grid, width: 64, height: 12, isAnsiTerminal: true, input: new Jumbee.Console.VtInputSource(anyMotion: true));
        UI.SetFocus(docs);
        run.Wait();
    }

    // Composite-control demo: a CodeEditor = a TextEditor with a line-number gutter docked to its left, built as
    // a single CompositeControl. The content is taller than the viewport, so it VERTICALLY SCROLLS: type / use
    // arrows + PageUp/Down, or the mouse wheel; AutoScroll keeps the caret visible, the scrollbar tracks position,
    // and the gutter stays aligned with the scrolled text (long lines also soft-wrap). Esc quits. Needs a VT terminal.
    static void CodeEditorDemo(string[] args)
    {
        var code = string.Join("\n",
            "// CodeEditor — scroll with arrows / PageUp-Down / wheel; long lines soft-wrap.",
            "using System;",
            "",
            "class Demo",
            "{",
            "    static void Main()",
            "    {",
            "        for (var i = 0; i < 20; i++)",
            "        {",
            "            Console.WriteLine($\"This is line number {i} — long enough to soft-wrap at the edge.\");",
            "        }",
            "",
            "        var message = \"the gutter numbers stay aligned with each logical line as you scroll\";",
            "        Console.WriteLine(message);",
            "    }",
            "}",
            "",
            "// Keep scrolling — there is more below the fold than the viewport can show at once.",
            "// 1\n// 2\n// 3\n// 4\n// 5\n// 6\n// 7\n// 8\n// 9\n// 10");

        var editor = new CodeEditor(Language.CSharp) { Text = code };
        editor.WithRoundedBorder(Cyan1).WithTitle("CodeEditor — vertical scroll + soft-wrap");

        UI.RegisterHotKey(UI.HotKeys.Escape, UI.Stop);

        var grid = new Jumbee.Console.Grid([16], [72], [[editor]]);
        var run = UI.Start(grid, width: 76, height: 18, isAnsiTerminal: true, input: new Jumbee.Console.VtInputSource(anyMotion: true));
        UI.SetFocus(editor.Editor);
        run.Wait();
    }

    // Interactive tabbed-container demo: a TabPanel with three tabs whose contents differ (a file list, a status
    // line, an about line). Click a tab label to switch, or use Alt+Left/Right (handled by the panel, so it works
    // from anywhere); Ctrl+arrows move focus between root regions and Ctrl+N/P cycle within the focused region (here
    // the tab panel: its headers + active content); Ctrl+T cycles the selection style (Highlight/Underline/Caret) on
    // both the list and the tab bar. The selected tab's content fills the area below.
    // Needs a VT terminal (e.g. Windows Terminal) for mouse + hover. Esc quits.
    static void TabsDemo(string[] args)
    {
        // A bare ListBox now highlights the selected row from the theme, so arrowing up/down is visible with no
        // explicit colours (override SelectedForeground/BackgroundColor to customise).
        var files = new ListBox();
        foreach (var f in new[] { "Program.cs", "TabPanel.cs", "TabHeader.cs", "CodeEditor.cs", "UI.cs", "Control.cs" })
            files.AddItem(f);

        var status = new TextLabel(TextLabelOrientation.Horizontal, "Build: OK    Tests: 202 passing", Color.White);
        var about = new TextLabel(TextLabelOrientation.Horizontal, "Click a tab or Alt+Left/Right.  Right-click a file for a menu.  Esc quits.", Color.White);

        var tabs = new TabPanel(TabBarDock.Top,
            ("Files", files),
            ("Status", status),
            ("About", about));

        var hint = new TextLabel(TextLabelOrientation.Horizontal, "".PadRight(54), Color.White);

        // Right-click a file for a context menu. The right-click selects that row first, so the menu's items act on
        // files.SelectedItem; ContextMenuOpening reports which file was clicked.
        files.ContextMenu = new ContextMenu(
        [
            new MenuItem("Open", () => { if (files.SelectedItem is { } f) hint.Text = $"Opened {f.Text}".PadRight(54); }),
            new MenuItem("Rename"),
            MenuItem.Separator,
            new MenuItem("Delete", () => { if (files.SelectedItem is { } f) files.RemoveItem(f); }),
        ]);
        files.ContextMenuOpening += (_, item) => hint.Text = $"Right-clicked {item.Text}".PadRight(54);

        // Cycle the selection style (Highlight -> Underline -> Caret) on Ctrl+T so the three modes can be eyeballed
        // on both the list (selected row) and the tab bar (active tab) at once.
        var styles = new[] { SelectionStyle.Highlight, SelectionStyle.Underline, SelectionStyle.Caret };
        var styleIdx = 0;
        void ApplyStyle()
        {
            files.SelectionStyle = styles[styleIdx];
            tabs.SelectionStyle = styles[styleIdx];
            hint.Text = $"Selection style: {styles[styleIdx]}   (Ctrl+T to cycle)".PadRight(54);
        }
        ApplyStyle();
        UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.T), () => { styleIdx = (styleIdx + 1) % styles.Length; ApplyStyle(); });

        UI.RegisterHotKey(UI.HotKeys.Escape, UI.Stop);

        var grid = new Jumbee.Console.Grid([15, 1], [54], [[tabs], [hint]]);
        var run = UI.Start(grid, width: 58, height: 18, isAnsiTerminal: true, input: new Jumbee.Console.VtInputSource(anyMotion: true));
        UI.SetFocus(files);   // focus the first tab's content so its keys work; Alt+arrows still switch tabs
        run.Wait();
    }

    // Dynamic TabPanel demo — a mini "code editor" whose tabs are opened/closed/hidden/disabled/renamed at runtime
    // (the TabPanel foundation for a future MultiCodeEditor). Each tab's content is a TextEditor. Alt+Left/Right (or
    // clicking a tab) switches; the hotkeys below mutate the tab set and the status line reflects it:
    //   Ctrl+T new file (adds a tab and switches to it)   Ctrl+W close active   Ctrl+D disable active
    //   Ctrl+B hide active   Ctrl+E reset (show + enable all)   Ctrl+R toggle a "●" dirty marker   Esc quits.
    // Disabling/hiding the active tab hops the selection to the nearest usable tab; closing the last tab empties it.
    static void DynamicTabsDemo(string[] args)
    {
        static TextEditor Editor(string text) => new() { Text = text };

        var main = Editor("// main.cs\nstatic void Main()\n{\n    System.Console.WriteLine(\"hi\");\n}\n");
        var utils = Editor("// utils.cs\nstatic int Add(int a, int b) => a + b;\n");
        var tabs = new TabPanel(TabBarDock.Top, ("main.cs", main), ("utils.cs", utils));

        var help = new TextLabel(TextLabelOrientation.Horizontal,
            "Ctrl+T new  Ctrl+W close  Ctrl+D disable  Ctrl+B hide  Ctrl+E reset  Ctrl+R dirty  Alt+←/→ switch  Esc quit", Color.White);
        var hint = new TextLabel(TextLabelOrientation.Horizontal, "".PadRight(72), Color.White);
        var counter = 3;

        static string Tag(TabItem t) => t.IsHidden ? $"({t.Name})" : t.IsDisabled ? $"[{t.Name}]" : t.Name;
        void Status() => hint.Text = $"active: {tabs.ActiveTabName ?? "—"}   |   {string.Join("  ", tabs.Tabs.Select(Tag))}".PadRight(72);
        Status();
        tabs.SelectionChanged += (_, _) => Status();

        UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.T), () =>
        {
            var name = $"file{counter++}.cs";
            tabs.SelectTab(tabs.AddTab(name, Editor($"// {name}\n")));   // open switches to the new file
            Status();
        });
        UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.W), () => { if (tabs.ActiveTab is { } t) { tabs.RemoveTab(t); Status(); } });
        UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.D), () => { if (tabs.ActiveTab is { } t) { t.IsDisabled = true; Status(); } });
        UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.B), () => { if (tabs.ActiveTab is { } t) { t.IsHidden = true; Status(); } });
        UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.E), () =>
        {
            foreach (var t in tabs.Tabs) { t.IsHidden = false; t.IsDisabled = false; }
            Status();
        });
        UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.R), () =>
        {
            if (tabs.ActiveTab is { } t) t.Name = t.Name.StartsWith("● ") ? t.Name[2..] : "● " + t.Name;
            Status();
        });
        UI.RegisterHotKey(UI.HotKeys.Escape, UI.Stop);

        var grid = new Jumbee.Console.Grid([16, 1, 1], [72], [[tabs], [help], [hint]]);
        
       

        var run = UI.Start(grid, width: 76, height: 20, isAnsiTerminal: true, input: new Jumbee.Console.VtInputSource(anyMotion: true));
        //hud.RegisterToggle();            // Ctrl+G toggles it in the top-right corner
                                         // or: hud.RegisterToggle(UI.HotKeys.Ctrl(ConsoleKey.F));
        UI.SetFocus(main);
        run.Wait();
    }

    // MultiTabCodeEditor demo — the reusable editor-group control: closable tabs (click the ✕ on the active/hovered
    // tab), a "+" button to open a new document, Alt+←/→ (or clicking a tab) to switch. Ctrl+T new, Ctrl+W close,
    // Esc quits. Each tab is a full CodeEditor (syntax highlighting + line-number gutter + independent scroll).
    static void MultiTabCodeEditorDemo(string[] args)
    {
        var group = new MultiTabCodeEditor(Language.CSharp) { ConfirmOnClose = true };
        group.OpenDocument("README.md", "// pinned, non-closable tab\n", Language.Markdown, closable: false);
        group.OpenDocument("main.cs", "// main.cs\nstatic void Main()\n{\n    System.Console.WriteLine(\"hi\");\n}\n");
        group.OpenDocument("utils.cs", "// utils.cs\nstatic int Add(int a, int b) => a + b;\n");
        group.Tabs.SelectedIndex = 1;

        var help = new TextLabel(TextLabelOrientation.Horizontal,
            "Edit a file then close it → confirm.  Click ✕ / + · Arrow to + · Enter · Ctrl+T new · Ctrl+W close · Alt+←/→ · Esc", Color.White);

        UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.T), () => group.NewDocument());
        UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.W), () => group.CloseActiveDocument());
        UI.RegisterHotKey(UI.HotKeys.Escape, UI.Stop);

        var grid = new Jumbee.Console.Grid([18, 1], [90], [[group], [help]]);
        var run = UI.Start(grid, width: 92, height: 22, isAnsiTerminal: true, input: new Jumbee.Console.VtInputSource(anyMotion: true));
        if (group.ActiveEditor is { } e) UI.SetFocus(e.Editor);
        run.Wait();
    }

    // Console Studio skeleton — the VS-Code-style shell from nested SplitPanels: an Explorer on the left, and on the
    // right an editor tab group over an Output panel. Drag any divider (it captures, so the drag stays live), or click
    // a divider and resize with the arrows (Shift = faster). Ctrl+B collapses/restores the Explorer; Alt+←/→ switch
    // editor tabs; Esc quits. Exercises SplitPanel + TabPanel + ListBox + TextEditor composed together.
    static void ConsoleStudioDemo(string[] args)
    {
        var explorer = new ListBox();
        foreach (var f in new[] { "Program.cs", "SplitPanel.cs", "SplitDivider.cs", "TabPanel.cs", "TerminalEmulator.cs", "UI.cs", "README.md" })
            explorer.AddItem(f);

        var editors = new TabPanel(TabBarDock.Top,
            ("Program.cs", new TextEditor { Text = "static void Main()\n{\n    Console.WriteLine(\"Console Studio\");\n}\n" }),
            ("SplitPanel.cs", new TextEditor { Text = "public class SplitPanel : Layout<SplitPanelDockPanel>\n{\n}\n" }));

        var output = new ListBox();
        foreach (var line in new[] { "> dotnet build", "  Build succeeded.", "  0 Warning(s)   0 Error(s)", "> _" })
            output.AddItem(line);

        // Right column = editors over output; root = explorer | right.
        var right = new SplitPanel(SplitOrientation.Vertical, editors, output.WithFrame(title: "Output"), 14);
        var root = new SplitPanel(SplitOrientation.Horizontal, explorer.WithFrame(title: "Explorer"), right, 22);

        var footer = new TextLabel(TextLabelOrientation.Horizontal,
            "Drag a divider, or click one + arrows (Shift faster).  Ctrl+B toggle Explorer.  Alt+←/→ switch tabs.  Esc quits.", Color.White);
        var grid = new Jumbee.Console.Grid([28, 1], [104], [[root], [footer]]);

        var savedExplorerWidth = 22;
        UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.B), () =>
        {
            if (root.SplitPosition > root.MinFirst) { savedExplorerWidth = root.SplitPosition; root.SplitPosition = root.MinFirst; }
            else root.SplitPosition = savedExplorerWidth;
        });
        UI.RegisterHotKey(UI.HotKeys.Escape, UI.Stop);

        var run = UI.Start(grid, width: 104, height: 30, isAnsiTerminal: true, input: new Jumbee.Console.VtInputSource(anyMotion: true));
        UI.SetFocus(explorer);
        run.Wait();
    }

    // Navigation playground exercising the focus model:
    //   * Ctrl+arrows move spatially between the root grid's regions (wrapping; skipping non-focusable regions).
    //   * Ctrl+N / Ctrl+P cycle focus WITHIN the current region — only when it's a multi-focusable layout (the
    //     "Actions" button stack); a single control / single-focusable region is a no-op.
    //   * Plain keys go to the focused control (arrows navigate the list; type / Tab indents the editor).
    //   * Non-focusable controls (Spinners, Digits, labels) and the regions made only of them are skipped by nav.
    //   * Ctrl+O opens a MODAL dialog that takes input exclusively until closed (Enter / Esc); the rest is inert
    //     and click-blocked while it is up. Ctrl+Q quits. Needs a VT terminal.
    static void NavigationDemo(string[] args)
    {
        // Live focus readout + key hints (all non-focusable, so navigation skips this region).
        var focusLabel = new TextLabel(TextLabelOrientation.Horizontal, "", Cyan1);
        void ShowFocus(string name) => focusLabel.Text = $"Focused: {name}".PadRight(28);
        var statusRegion = new Jumbee.Console.VerticalStackPanel(
            focusLabel,
            new TextLabel(TextLabelOrientation.Horizontal, "Ctrl+arrows: between regions", Color.White),
            new TextLabel(TextLabelOrientation.Horizontal, "Ctrl+N/P: within a region", Color.White),
            new TextLabel(TextLabelOrientation.Horizontal, "F1: help   Ctrl+O: dialog", Color.White),
            new TextLabel(TextLabelOrientation.Horizontal, "Ctrl+T: theme   Ctrl+Q: quit", Color.White));

        // [0,0] Actions: a multi-focusable region (a stack of buttons) -> Ctrl+N/P cycles within it. The buttons use
        // the modern raised bevel; the focused one reverses its label.
        var bA = new Button("Action A"); var bB = new Button("Action B"); var bC = new Button("Action C");
        bA.OnFocus += () => ShowFocus("Action A");
        bB.OnFocus += () => ShowFocus("Action B");
        bC.OnFocus += () => ShowFocus("Action C");
        bA.Style = bA.Style.WithShape(ButtonShape.Modern);
        bB.Style = bB.Style.WithShape(ButtonShape.Modern);
        bC.Style = bC.Style.WithShape(ButtonShape.Modern);
        var actions = new Jumbee.Console.VerticalStackPanel(bA, bB, bC);

        // [0,1] Files: a single interactive control -> plain Up/Down navigate; Ctrl+N/P is a no-op here. A neutral
        // square border (no explicit colour) so focus shows by turning the border cyan.
        var files = new ListBox("alpha.cs", "beta.cs", "gamma.cs", "delta.cs");
        files.OnFocus += () => ShowFocus("Files list");
        files.WithSquareBorder().WithTitle("Files (Up/Down)");

        // [0,2] Editor: a single interactive control -> type, Tab indents.
        var editor = new CodeEditor(Language.CSharp) { Text = "// edit me\nclass Demo\n{\n}" };
        editor.Editor.OnFocus += () => ShowFocus("Editor");
        editor.WithRoundedBorder(Magenta1).WithTitle("Editor (type, Tab)");

        // [1,0] Dock: a DockPanel (a different layout type) — a non-focusable header docked on top + a focusable
        // button filling below, so arrows enter the button and Ctrl+N/P is a no-op (one focusable in the region).
        var openBtn = new Button("Open dialog");
        openBtn.Style = openBtn.Style.WithShape(ButtonShape.Modern);
        openBtn.OnFocus += () => ShowFocus("Open-dialog button");   // focused button reverses its label
        var dockHeader = new TextLabel(TextLabelOrientation.Horizontal, "— dock region —", Color.White) { Height = 1 };
        var dock = new Jumbee.Console.DockPanel(DockedControlPlacement.Top, dockHeader, openBtn);

        // [1,1] Display: a non-focusable animated Spinner -> Ctrl+arrows skip the whole region.
        var spin = new Spinner { SpinnerType = Spectre.Console.Spinner.Known.Dots }; spin.Start();
        spin.WithSquareBorder().WithTitle("Spinner (skipped)");   // non-focusable: border stays grey, never turns cyan
        // Supply help for this control without subclassing it: OnHelp adds an F1 tab for the spinner.
        spin.OnHelp += info =>
        {
            info.Name = "Spinner";
            info.Title = "Spinner";
            info.Text = "An animated busy indicator. It is display-only, so focus navigation skips it.";
        };

        var bottom = new Jumbee.Console.Grid([9, 6], [26, 24, 30],
        [
            [actions, files, editor],
            [dock,    spin,  statusRegion],
        ]);
        var overlay = new Overlay(bottom);

        // Modal dialog: a framed OK button. While shown it has exclusive input (Enter activates, Esc = CloseKey).
        void OpenDialog()
        {
            var ok = new Button("OK   (Enter / Esc to close)");   // borderless (default); the frame supplies border + title
            ok.Activated += (_, _) => overlay.Hide();
            ok.WithRoundedBorder(Cyan1).WithTitle("Modal — input is exclusive until closed");
            overlay.ShowModal(ok);
        }
        openBtn.Activated += (_, _) => OpenDialog();
        UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.O), OpenDialog);   // Ctrl+Q quits (default); Esc closes the dialog

        // Theme switch (Ctrl+T): toggles cool <-> retro, re-skinning the frames in place AND changing the modal scrim
        // tint/dim — open a dialog (Ctrl+O) or help (F1) after switching to see the see-through scrim differ.
        var retro = false;
        UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.T), () =>
        {
            retro = !retro;
            UI.SetTheme(
                retro ? new RetroStyleTheme() : new CoolStyleTheme(),
                retro ? new RetroGlyphTheme() : new DefaultGlyphTheme());
        });

        var run = UI.Start(overlay, width: 84, height: 17, isAnsiTerminal: true, input: new Jumbee.Console.VtInputSource(anyMotion: true));
        UI.SetFocus(bA);   // start on the first action button
        run.Wait();
    }

    // Terminal playground: a live shell (TerminalEmulator) framed alongside an interactive command list, action
    // buttons, an animated Spinner, and a modal dialog — to shake out interactive/rendering bugs when a real
    // subprocess, an animation, focus navigation, and a modal overlay all share the single UI thread.
    //   * Type into the terminal; mouse-wheel / Shift+PageUp-Down scrolls its scrollback.
    //   * Ctrl+arrows move between regions (terminal ⇄ commands ⇄ buttons); Ctrl+N/P within the button stack.
    //   * Focus the Commands list and press Enter to run the selected command in the terminal.
    //   * Buttons run a command / open a modal (also Ctrl+O). The Spinner keeps animating throughout.
    //   * Ctrl+Q quits; typing 'exit' in the shell also closes it. Needs a VT terminal (e.g. Windows Terminal).
    static void TerminalDemo(string[] args)
    {
        ConsoleManager.EmulateBlinkingCursor = true;
        var shell = args.Length > 0 ? args[0] : ResolveShell();

        var term = new TerminalEmulator(shell);
        var baseTitle = $"{shell}  —  Ctrl+arrows to leave · type 'exit'";
        // Neutral border (no explicit colour) so it turns the focus colour when focused, like the other regions —
        // hard-coding it to Cyan1 (the focus colour) would make the frame look focused at all times.
        term.WithRoundedBorder().WithTitle(baseTitle);
        // The running program's window title (OSC 0/2) flows into the frame title.
        term.TitleChanged += (_, t) => term.Frame!.Title = string.IsNullOrWhiteSpace(t) ? baseTitle : t;

        var focusLabel = new TextLabel(TextLabelOrientation.Horizontal, "", Cyan1);
        void ShowFocus(string n) => focusLabel.Text = $"Focused: {n}".PadRight(28);
        term.OnFocus += () => ShowFocus("Terminal");

        // Interactive command list — Enter runs the selected command in the terminal (then returns focus to it).
        var commands = new[] { "dir", "git status -s", "echo hello from Jumbee", "$PSVersionTable" };
        var cmds = new ListBox(commands) { SelectedForegroundColor = Color.White, SelectedBackgroundColor = Color.Blue };
        cmds.OnFocus += () => ShowFocus("Commands (Enter runs)");
        cmds.Committed += (_, _) =>
        {
            var i = cmds.SelectedIndex;
            if (i >= 0 && i < commands.Length) { term.SendText(commands[i] + "\r"); UI.SetFocus(term); }
        };
        cmds.WithSquareBorder().WithTitle("Commands (Enter)");

        // Action buttons (a multi-focusable region: Ctrl+N/P cycles within it).
        var clearBtn = new Button("Clear (cls)"); clearBtn.Style = clearBtn.Style.WithShape(ButtonShape.Modern);
        var aboutBtn = new Button("About ▸");     aboutBtn.Style = aboutBtn.Style.WithShape(ButtonShape.Modern);
        clearBtn.OnFocus += () => ShowFocus("Clear button");
        aboutBtn.OnFocus += () => ShowFocus("About button");
        clearBtn.Activated += (_, _) => { term.SendText("cls\r"); UI.SetFocus(term); };
        var actions = new Jumbee.Console.VerticalStackPanel(clearBtn, aboutBtn);

        // Animated, non-focusable Spinner (navigation skips it) — proves the animation keeps ticking alongside the
        // live terminal, the modal, and focus changes.
        var spin = new Spinner { SpinnerType = Spectre.Console.Spinner.Known.Dots, Text = "live" };
        spin.Start();
        spin.WithSquareBorder().WithTitle("Spinner");

        var hints = new TextLabel(TextLabelOrientation.Horizontal, "Ctrl+O modal · Ctrl+Q quit", Color.Grey);

        var grid = new Jumbee.Console.Grid(
            [18, 7, 1],
            [70, 28],
            [
                [term,        cmds],
                [actions,     spin],
                [focusLabel,  hints],
            ]);
        var overlay = new Overlay(grid);

        // Modal dialog over the live terminal (the terminal keeps reading/rendering behind the scrim; input is
        // exclusive until the dialog closes).
        void OpenAbout()
        {
            var ok = new Button("OK   (Enter / Esc to close)");
            ok.Activated += (_, _) => { overlay.Hide(); UI.SetFocus(term); };
            ok.WithRoundedBorder(Yellow).WithTitle("Terminal demo — live PTY + VtNetCore");
            overlay.ShowModal(ok);
        }
        aboutBtn.Activated += (_, _) => OpenAbout();
        UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.O), OpenAbout);

        // Quit the UI loop when the child process exits (e.g. the user typed 'exit').
        term.Exited += (_, _) => UI.Stop();

        var run = UI.Start(overlay, width: 100, height: 28, isAnsiTerminal: true, input: new Jumbee.Console.VtInputSource(anyMotion: true));
        UI.SetFocus(term);
        run.Wait();
        term.Dispose();
    }

    // First shell on PATH from the preference list (cmd.exe is the guaranteed fallback).
    static string ResolveShell()
    {
        foreach (var candidate in new[] { "pwsh.exe", "powershell.exe", "cmd.exe" })
            if (OnPath(candidate)) return candidate;
        return "cmd.exe";
    }

    static bool OnPath(string exe)
    {
        var path = Environment.GetEnvironmentVariable("PATH") ?? "";
        foreach (var dir in path.Split(System.IO.Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            try { if (System.IO.File.Exists(System.IO.Path.Combine(dir.Trim(), exe))) return true; } catch { }
        return false;
    }

    // Interactive toggle-widgets demo: a Checkbox, a Switch, a single-select RadioSet and a multi-select
    // SelectionList. Click a control to focus it, then Space toggles / arrows navigate (or just click rows).
    // The status line reflects the latest change. Needs a VT terminal (e.g. Windows Terminal).
    static void ToggleDemo(string[] args)
    {
        // Start under the "cool" theme so the frames pick up its border shape/colour. Set the theme statics
        // BEFORE constructing controls so they capture it (the switch below uses UI.SetTheme for the live path).
        UI.StyleTheme = new CoolStyleTheme();
        UI.GlyphTheme = new DefaultGlyphTheme();

        var status = new TextLabel(TextLabelOrientation.Horizontal, "Click a control, then Space / arrows.".PadRight(44), Color.White);

        var notify = new Checkbox("Enable notifications");
        var dark = new Jumbee.Console.Switch("Dark mode");

        // Keep the title placement inline (compact) but leave the BORDER unset so it follows the theme: the
        // switch then changes the frame's border shape AND colour (cool = rounded cyan, retro = heavy magenta).
        var inlineTitle = new TitleStyle(TitlePos.TopLeft, TitleBorderStyle.Inline, TitleColorStyle.Normal);

        var theme = new RadioSet("Light", "Dark", "Solarized") { SelectedIndex = 0 };
        theme.WithFrame().WithTitle("Theme (one of)", inlineTitle);

        var toppings = new SelectionList("Cheese", "Mushroom", "Pepperoni", "Olives");
        toppings.WithFrame().WithTitle("Toppings (any of)", inlineTitle);

        // Live theme switch: clicking re-skins every control above in place via UI.SetTheme — checkbox/radio/
        // switch glyphs, accent colours, the button, AND the frame borders (shape + colour).
        var retro = false;
        var themeBtn = new Button("Switch theme (cool ⇄ retro)");   // plain button → follows the theme's Primary
        themeBtn.Activated += (_, _) =>
        {
            retro = !retro;
            UI.SetTheme(
                retro ? new RetroStyleTheme() : new CoolStyleTheme(),
                retro ? new RetroGlyphTheme() : new DefaultGlyphTheme());
            status.Text = $"Theme: {(retro ? "retro" : "cool")}".PadRight(44);
        };

        notify.Changed += (_, v) => status.Text = $"Notifications: {(v ? "on" : "off")}".PadRight(44);
        dark.Changed += (_, v) => status.Text = $"Dark mode: {(v ? "on" : "off")}".PadRight(44);
        theme.SelectionChanged += (_, _) => status.Text = $"Theme: {theme.SelectedValue}".PadRight(44);
        toppings.SelectionChanged += (_, _) => status.Text = ("Toppings: " + string.Join(", ", toppings.SelectedValues)).PadRight(44);

        var grid = new Jumbee.Console.Grid(
            [2, 1, 1, 5, 6, 3],   // last row 3 tall for the bordered button
            [46],
            [
                [status],
                [notify],
                [dark],
                [theme],
                [toppings],
                [themeBtn],
            ]);

        var run = UI.Start(grid, width: 50, height: 20, isAnsiTerminal: true, input: new Jumbee.Console.VtInputSource(anyMotion: true));
        UI.SetFocus(notify);
        run.Wait();
    }

    // Interactive overlay/modal demo. Click "Open dialog" to float a MODAL framed popup over a dimming scrim:
    // the background is blocked (clicking outside does nothing), and the dialog closes with its button or Esc
    // (the overlay's CloseKey). Needs a VT terminal (e.g. Windows Terminal).
    // Builds a button whose default theme styles are overridden with a custom colour scheme (showing that a
    // control can still depart from the active theme via its style setters).
    static Button ColorButton(string text, Color baseColor) => new Button(text)
    {
        Style = new ButtonStyle(
            normal: Style.White | Style.Bg(baseColor),
            hover: Style.White | Style.Bg(Lighten(baseColor, 25)),
            press: Style.White | Style.Bg(Lighten(baseColor, 55)),
            shape: ButtonShape.Modern,   // opt into the modern raised look; edge highlights derive from the fill
            minWidth: 16),
    };

    static Color Lighten(Color c, int by) =>
        new((byte)Math.Min(255, c.R + by), (byte)Math.Min(255, c.G + by), (byte)Math.Min(255, c.B + by));

    // Phase 1 controls (TextInput, Badge, Footer) in a "Posting"-style request bar.
    //   * Click the URL field (or Tab to it) and type; arrows/Shift+arrows/Ctrl+A edit & select; Enter sends.
    //   * Click "Send" (or press Enter in the URL) -> the status Badge turns green "200 OK" and the request is logged.
    //   * Fill the Header / Value fields and click "Add" -> the header is appended to the Activity log.
    //   * Ctrl+M opens the method dropdown. Tab / Shift+Tab move focus; Ctrl+Q quits; F1 help.
    // Needs a VT terminal (e.g. Windows Terminal).
    static void InputsDemo(string[] args)
    {
        var method = new Select("GET", "POST", "PUT", "DELETE", "PATCH") { Placeholder = "GET" };
        var url = new TextInput(placeholder: "https://api.example.com/…   (Enter to send)");
        var send = new Button("Send");
        var statusBadge = new Badge("—");
        var reqRow = new Jumbee.Console.Grid([1], [12, 50, 8, 12], [[method, url, send, statusBadge]]);

        var keyInput = new TextInput(placeholder: "Header");
        var valueInput = new TextInput(placeholder: "Value");
        var addBtn = new Button("Add");
        var addRow = new Jumbee.Console.Grid([1], [26, 40, 8], [[keyInput, valueInput, addBtn]]);

        // The added request headers, as an interactive grid: Up/Down select, Enter deletes the selected header.
        var headersTable = new DataTable("Header", "Value");

        var status = new TextLabel(TextLabelOrientation.Horizontal, "Type a URL and press Enter (or click Send).".PadRight(80), Color.White);

        // Declared before the menu bar so its item actions can close over them.
        var currentMethod = "GET";
        var methods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH" };

        // A menu bar across the top: click a title (or Tab to the bar and press Enter/Down) to drop a non-modal
        // menu; clicking elsewhere or pressing Esc dismisses it. Items run the same actions as the buttons.
        var menuBar = new MenuBar()
            .Add("Request",
                new MenuItem("Send", () => Send()) { Shortcut = "Enter" },
                new MenuItem("Add header", () => AddHeader()),
                MenuItem.Separator,
                new MenuItem("Quit", () => UI.Stop()) { Shortcut = "^c" })
            .Add("Method",
                new MenuItem("GET", () => SetMethod("GET")),
                new MenuItem("POST", () => SetMethod("POST")),
                new MenuItem("PUT", () => SetMethod("PUT")),
                new MenuItem("DELETE", () => SetMethod("DELETE")),
                new MenuItem("PATCH", () => SetMethod("PATCH")))
            .Add("View",
                new MenuItem("Clear headers", () => headersTable.Clear()));

        var body = new Jumbee.Console.Grid(
            [1, 1, 1, 1, 1, 10, 1],
            [82],
            [
                [menuBar],
                [reqRow],
                [new TextLabel(TextLabelOrientation.Horizontal, "", Color.White)],
                [new TextLabel(TextLabelOrientation.Horizontal, "Add a header:", Color.White)],
                [addRow],
                [headersTable],
                [status],
            ]);

        var footer = new Footer(
            new FooterHint("Enter", "Send"), new FooterHint("^m", "Method"),
            new FooterHint("Tab", "Move"), new FooterHint("^c", "Quit"), new FooterHint("f1", "Help"));

        var dock = new Jumbee.Console.DockPanel(DockedControlPlacement.Bottom, footer, body);
        // UI.Start wraps the root in UI.Overlay; the method dropdown, the menu bar's drop-downs, and the Autocomplete
        // below all float into it automatically — no per-control wiring.

        // Type-ahead on the Header field: a passive popup of common header names floats under the caret while the
        // field keeps focus. Type (e.g. "acc"), Down/Up to pick, Enter/Tab to accept, Esc to dismiss.
        _ = new Autocomplete(keyInput,
            "Accept", "Accept-Encoding", "Accept-Language", "Authorization", "Cache-Control", "Connection",
            "Content-Type", "Cookie", "Host", "If-None-Match", "Origin", "Referer", "User-Agent");

        method.SelectionChanged += (_, v) => { currentMethod = v; status.Text = $"Method: {v}".PadRight(80); };

        void SetMethod(string m) { var i = Array.IndexOf(methods, m); if (i >= 0) method.SelectedIndex = i; }

        void Send()
        {
            statusBadge.Text = "200 OK";
            statusBadge.Variant = BadgeVariant.Success;
            status.Text = $"Sent {currentMethod} {url.Text} ({headersTable.RowCount} headers)".PadRight(80);
        }

        void AddHeader()
        {
            if (string.IsNullOrWhiteSpace(keyInput.Text)) { status.Text = "Enter a header name first.".PadRight(80); return; }
            headersTable.AddRow(keyInput.Text, valueInput.Text);
            status.Text = $"Added header {keyInput.Text}".PadRight(80);
            keyInput.Text = "";
            valueInput.Text = "";
        }

        send.Activated += (_, _) => Send();
        url.Submitted += (_, _) => Send();
        addBtn.Activated += (_, _) => AddHeader();
        valueInput.Submitted += (_, _) => AddHeader();
        // Enter on a selected header row deletes it.
        headersTable.RowActivated += (_, i) => { var h = headersTable.SelectedRow?[0]; headersTable.RemoveRow(i); status.Text = $"Removed header {h}".PadRight(80); };
        UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.M), () => method.Open());

        var run = UI.Start(dock, width: 90, height: 22, isAnsiTerminal: true, input: new Jumbee.Console.VtInputSource(anyMotion: true));
        UI.SetFocus(url);
        run.Wait();
    }

    // Dialog demo: modal popups over the ambient overlay. A column of buttons opens each dialog kind — a Yes/No
    // Confirm, an OK Message, and a custom-content modal (a rename field with OK/Cancel). Each dialog dims and
    // blocks the background until dismissed (←/→ or Tab between buttons, Enter/Space activates, Esc cancels); the
    // status line shows the result. Ctrl+N/P move between the launcher buttons; Ctrl+Q quits. Needs a VT terminal.
    static void DialogDemo(string[] args)
    {
        var status = new TextLabel(TextLabelOrientation.Horizontal, "Click a button (or Ctrl+N/P then Enter) to open a dialog.".PadRight(56), Cyan1);
        void SetStatus(string s) => status.Text = s.PadRight(56);

        var confirmBtn = new Button("Confirm (Yes / No)");
        var messageBtn = new Button("Message (OK)");
        var renameBtn = new Button("Custom content — Rename…");
        var quitBtn = new Button("Quit");
        foreach (var b in new[] { confirmBtn, messageBtn, renameBtn, quitBtn })
            b.Style = b.Style.WithShape(ButtonShape.Modern);

        confirmBtn.Activated += (_, _) =>
            Dialog.Confirm("Delete file", "Delete report.pdf? This action cannot be undone.",
                yes => SetStatus(yes ? "You chose: Yes (deleted)" : "You chose: No (kept)"));

        messageBtn.Activated += (_, _) =>
            Dialog.Message("About", "Jumbee.Console dialogs — modal windows over the system overlay.",
                () => SetStatus("Message dismissed."));

        renameBtn.Activated += (_, _) =>
        {
            var field = new TextInput("report.pdf");
            Dialog.Show("Rename file", field, DialogButtons.OkCancel, result =>
                SetStatus(result == DialogResult.Ok ? $"Renamed to: {field.Text}" : "Rename cancelled."));
        };

        quitBtn.Activated += (_, _) =>
            Dialog.Confirm("Quit", "Exit the demo?", yes => { if (yes) UI.Stop(); });

        var buttons = new Jumbee.Console.VerticalStackPanel(confirmBtn, messageBtn, renameBtn, quitBtn);
        var root = new Jumbee.Console.Grid([2, 13], [60], [[status], [buttons]]);

        // Note: no global Escape hotkey here — the dialog uses Escape to cancel. Ctrl+Q quits (registered by default).
        var run = UI.Start(root, width: 64, height: 18, isAnsiTerminal: true, input: new Jumbee.Console.VtInputSource(anyMotion: true));
        UI.SetFocus(confirmBtn);
        run.Wait();
    }

    // ChatPrompt demo: the input area of an agent CLI (Claude Code / Gemini CLI) — a prompt glyph that turns into a
    // busy spinner while a request "runs", a bordered TextInput below a scrolling transcript, and slash-command
    // type-ahead.
    //   * Type a message and press Enter -> it's echoed into the transcript, the gutter spins for ~1.5s (as if the
    //     agent were working), then a canned reply appears and the spinner reverts to the prompt glyph.
    //   * Type "/" for slash-command type-ahead (/help, /clear, /model, /quit); Down/Up pick, Enter/Tab accept.
    //   * "/clear" empties the transcript, "/quit" (or Ctrl+Q) exits. Needs a VT terminal (e.g. Windows Terminal).
    static void ChatPromptDemo(string[] args)
    {
        var log = new Log();
        log.Write("[grey]Welcome. Ask a question, or type [/][cyan]/[/][grey] for commands.[/]");

        // A rounded border, no title: the prompt is a single input row, and the default titled-frame style spends a
        // dedicated title row + separator on top — which would squeeze the one content row out of a short cell.
        var chat = new ChatPrompt(placeholder: "Send a message…  (Enter to submit, / for commands)");
        chat.WithRoundedBorder(new Color(90, 120, 200));
        chat.WithSuggestions("/help", "/clear", "/model", "/quit");

        void Reply(string text) => log.Write($"[green]● agent[/]  {Spectre.Console.Markup.Escape(text)}");

        chat.Submitted += async (_, msg) =>
        {
            if (string.IsNullOrWhiteSpace(msg)) return;
            chat.Text = "";

            switch (msg.Trim())
            {
                case "/quit": UI.Stop(); return;
                case "/clear": log.Clear(); return;
                case "/help":
                    Reply("Commands: /help  /clear  /model  /quit. Otherwise just type a message.");
                    return;
                case "/model":
                    Reply("Current model: claude-opus-4-8.");
                    return;
            }

            log.Write($"[cyan]❯ you[/]    {Spectre.Console.Markup.Escape(msg)}");

            // Simulate an in-flight request: spin the gutter, then answer. The await resumes on the UI thread (the
            // dispatcher installs a SynchronizationContext), so the post-await updates need no explicit marshaling.
            chat.Busy = true;
            await Task.Delay(1500);
            Reply($"You said: \"{msg}\". (This is a canned reply — wire me to a real backend.)");
            chat.Busy = false;
        };

        var transcript = log.WithRoundedBorder(new Color(60, 70, 90)).WithTitle("Conversation");
        // Transcript fills the top; the framed prompt is a fixed 3 rows (1 input row + border) at the bottom.
        var root = new Jumbee.Console.Grid([20, 3], [78], [[transcript], [chat]]);

        UI.RegisterHotKey(UI.HotKeys.Escape, UI.Stop);

        var run = UI.Start(root, width: 82, height: 25, isAnsiTerminal: true, input: new Jumbee.Console.VtInputSource(anyMotion: true));
        UI.SetFocus(chat);
        run.Wait();
    }

    // The full multi-pane "Posting"-style HTTP-client TUI, assembling the controls built this cycle: a Collection
    // Tree, a method Select + URL TextInput + Send Button + status Badge, a Request panel (TabPanel of Headers
    // DataTable + add-row + Autocomplete / Body / Query / Auth) and a Response panel (status Badge + TabPanel of a
    // read-only JSON CodeEditor / Headers / Cookies), with a Footer docked along the bottom. The method dropdown,
    // and header Autocomplete popup float in the ambient UI.Overlay (UI.Start wraps the root in it automatically).
    static void PostingDemo(string[] args)
    {
        // ----- Header bar -----
        var appTitle = new TextLabel(TextLabelOrientation.Horizontal, " Posting 2.3.0", new Color(150, 170, 255));
        var userLabel = new TextLabel(TextLabelOrientation.Horizontal, "user@host ", new Color(120, 130, 150));
        var header = new Jumbee.Console.Grid([1], [60, 50], [[appTitle, userLabel]]);

        // ----- URL bar -----
        var method = new Select("GET", "POST", "PUT", "DELETE", "PATCH") { Placeholder = "GET" };
        var url = new TextInput(placeholder: "https://api.example.com/users   (Enter to send)");
        var send = new Button("Send");
        var statusBadge = new Badge("—");
        var urlRow = new Jumbee.Console.Grid([1], [12, 78, 8, 12], [[method, url, send, statusBadge]]);

        // ----- Left: Collection tree. Parent nodes show a disclosure glyph (expand/collapse with ←/→/Enter);
        // leaf nodes use plain text so they get the themed leaf glyph and highlight when selected (Up/Down). -----
        var tree = new Tree("sample-collections");
        var users = tree.AddNode("users");
        var posts = tree.AddNode("posts");
        users.AddChild("GET list users");
        users.AddChild("POST create user");
        users.AddChild("DEL delete user");
        posts.AddChild("GET list posts");
        posts.AddChild("PUT update post");
        var treeFrame = tree.WithRoundedBorder(new Color(80, 90, 110)).WithTitle("Collection");

        // ----- Request panel: Headers tab = add-row + DataTable, plus Body / Query / Auth -----
        var keyInput = new TextInput(placeholder: "Header");
        var valueInput = new TextInput(placeholder: "Value");
        var addBtn = new Button("Add");
        var addRow = new Jumbee.Console.Grid([1], [24, 34, 8], [[keyInput, valueInput, addBtn]]);
        var headersTable = new DataTable("Header", "Value");
        var headersContent = new Jumbee.Console.Grid([1, 12], [68], [[addRow], [headersTable]]);

        var reqBody = new CodeEditor(Language.Json) { Text = "{\n  \"name\": \"Ada\"\n}" };
        var queryTable = new DataTable("Param", "Value");
        var authType = new Select("None", "Bearer", "Basic", "API Key") { Placeholder = "None" };
        var authToken = new TextInput(placeholder: "token");
        var authContent = new Jumbee.Console.Grid([1, 1], [68], [[authType], [authToken]]);

        var reqTabs = new TabPanel(TabBarDock.Top,
            ("Headers", headersContent),
            ("Body", reqBody.WithFrame()),
            ("Query", queryTable),
            ("Auth", authContent));
        // Layouts can't carry a ControlFrame (only Controls can), so the Request/Response panels are labeled with a
        // styled title row rather than a bordered box; the Collection tree (a Control) keeps its real frame.
        var reqTitle = new TextLabel(TextLabelOrientation.Horizontal, "Request", new Color(150, 170, 255));
        var reqPanel = new Jumbee.Console.Grid([1, 15], [72], [[reqTitle], [reqTabs]]);

        // ----- Response panel: status Badge + TabPanel (read-only JSON body / Headers / Cookies) -----
        var respStatus = new Badge("200 OK") { Variant = BadgeVariant.Success };
        var respBody = new CodeEditor(Language.Json)
        {
            Text = "{\n  \"id\": 1,\n  \"name\": \"Ada Lovelace\",\n  \"email\": \"ada@example.com\"\n}",
            ReadOnly = true,
        };
        var respHeaders = new DataTable("Header", "Value");
        respHeaders.AddRow("Content-Type", "application/json");
        respHeaders.AddRow("Server", "Jumbee/1.0");
        var respCookies = new DataTable("Name", "Value");
        var respTabs = new TabPanel(TabBarDock.Top,
            ("Body", respBody.WithFrame()),
            ("Headers", respHeaders),
            ("Cookies", respCookies));
        var responseContent = new Jumbee.Console.Grid([1, 11], [68], [[respStatus], [respTabs]]);
        var respTitle = new TextLabel(TextLabelOrientation.Horizontal, "Response", new Color(150, 170, 255));
        var respPanel = new Jumbee.Console.Grid([1, 13], [72], [[respTitle], [responseContent]]);

        // ----- Compose: right column (Request over Response), then the two-column body -----
        var rightGrid = new Jumbee.Console.Grid([16, 15], [72], [[reqPanel], [respPanel]]);
        var bodyGrid = new Jumbee.Console.Grid([31], [38, 72], [[treeFrame, rightGrid]]);

        var body = new Jumbee.Console.Grid([1, 1, 31], [110], [[header], [urlRow], [bodyGrid]]);

        var footer = new Footer(
            new FooterHint("Enter", "Send"), new FooterHint("^m", "Method"),
            new FooterHint("Alt+←→", "Tabs"), new FooterHint("Tab", "Move"),
            new FooterHint("^c", "Quit"), new FooterHint("f1", "Help"));

        var dock = new Jumbee.Console.DockPanel(DockedControlPlacement.Bottom, footer, body);
        // No manual Overlay: UI.Start wraps the root in one (UI.Overlay), and the dropdown Selects, the menu bar,
        // and the Autocomplete below all default to it — no per-control wiring needed.

        // Type-ahead on the header name field (floats in the ambient UI.Overlay).
        _ = new Autocomplete(keyInput,
            "Accept", "Accept-Encoding", "Accept-Language", "Authorization", "Cache-Control", "Connection",
            "Content-Type", "Cookie", "Host", "If-None-Match", "Origin", "Referer", "User-Agent");

        var currentMethod = "GET";
        method.SelectionChanged += (_, v) => currentMethod = v;

        void Send()
        {
            respStatus.Text = "200 OK";
            respStatus.Variant = BadgeVariant.Success;
            statusBadge.Text = "200 OK";
            statusBadge.Variant = BadgeVariant.Success;
        }

        void AddHeader()
        {
            if (string.IsNullOrWhiteSpace(keyInput.Text)) return;
            headersTable.AddRow(keyInput.Text, valueInput.Text);
            keyInput.Text = "";
            valueInput.Text = "";
        }

        send.Activated += (_, _) => Send();
        url.Submitted += (_, _) => Send();
        addBtn.Activated += (_, _) => AddHeader();
        valueInput.Submitted += (_, _) => AddHeader();
        headersTable.RowActivated += (_, i) => headersTable.RemoveRow(i);
        // Activate a request leaf (double-click or Enter) to load its method + URL into the request bar.
        var methodNames = new[] { "GET", "POST", "PUT", "DELETE", "PATCH" };
        void LoadRequest(Jumbee.Console.Tree.TreeNode node)
        {
            if (string.IsNullOrWhiteSpace(node.Text)) return;
            var parts = node.Text.Split(' ', 2);
            var verb = parts[0] == "DEL" ? "DELETE" : parts[0];
            var mi = Array.IndexOf(methodNames, verb);
            if (mi >= 0) method.SelectedIndex = mi;
            var resource = parts.Length > 1 ? parts[1].Split(' ')[^1] : "";
            url.Text = $"https://api.example.com/{resource}";
        }
        tree.NodeActivated += (_, node) => LoadRequest(node);

        // Right-click a request to act on it. The menu's items read tree.SelectedNode (the right-clicked node).
        // "Copy as ▸" opens a submenu (Right/Enter or hover to expand, Left to go back).
        tree.ContextMenu = new ContextMenu(
        [
            new MenuItem("Open", () => { if (tree.SelectedNode is { } n) LoadRequest(n); }),
            new MenuItem("Copy as", new MenuItem[]
            {
                new("cURL"),
                new("HTTPie"),
                new("fetch()"),
            }),
            MenuItem.Separator,
            new MenuItem("Delete", () => { if (tree.SelectedNode is { Parent: { } p } n) p.RemoveChild(n.Index); }),
        ]);

        UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.M), () => method.Open());

        var run = UI.Start(dock, width: 112, height: 38, isAnsiTerminal: true, input: new Jumbee.Console.VtInputSource(anyMotion: true));
        UI.SetFocus(url);
        run.Wait();
    }

    static void OverlayDemo(string[] args)
    {
        var status = new TextLabel(TextLabelOrientation.Horizontal, "Click 'Open dialog' to pop a window.".PadRight(40), Color.White);
        var openBtn = ColorButton("Open dialog", new Color(40, 70, 120));

        var bottom = new Jumbee.Console.Grid([2, 3], [44], [[status], [openBtn]]);
        var overlay = new Overlay(bottom);

        var closeBtn = ColorButton("Close (or press Esc)", new Color(110, 40, 40));
        closeBtn.Style = closeBtn.Style with { Shape = ButtonShape.Flat };   // the frame supplies the border + title
        closeBtn.WithRoundedBorder(Yellow).WithTitle("Modal dialog");

        openBtn.Activated += (_, _) => { status.Text = "Modal open — Close or Esc (bg blocked).".PadRight(40); overlay.ShowModal(closeBtn); };
        closeBtn.Activated += (_, _) => { overlay.Hide(); status.Text = "Dialog closed.".PadRight(40); };

        var run = UI.Start(overlay, width: 48, height: 14, isAnsiTerminal: true, input: new Jumbee.Console.VtInputSource(anyMotion: true));
        UI.SetFocus(openBtn);
        run.Wait();
    }

    // Interactive mouse-wheel demo: a list taller than its frame. Scroll the wheel over it (needs a VT terminal,
    // e.g. Windows Terminal); Alt+Up / Alt+Down still scroll too. The scrollbar thumb tracks the position.
    static void WheelDemo(string[] args)
    {
        var list = new ListBox { SelectedForegroundColor = Color.White, SelectedBackgroundColor = Color.Blue };
        for (int i = 1; i <= 40; i++) list.AddItem($"Item {i:00}  — scroll the wheel over me");

        list.WithRoundedBorder(Cyan1)
            .WithTitle("Mouse wheel to scroll  (Alt+Up/Down also works)")
            .WithScrollBarGlyphs(ScrollBarGlyphs.Block)
            .WithScrollBarStyle(ScrollBarStyle.Default.WithColors(thumb: Cyan1, arrows: Yellow));

        list.IsFocused = true;

        var grid = new Jumbee.Console.Grid([16], [50], [[list]]);

        var run = UI.Start(grid, width: 56, height: 20, isAnsiTerminal: true, input: new Jumbee.Console.VtInputSource());
        run.Wait();
    }

    // Interactive Button demo. Click the buttons with the mouse (needs a VT terminal, e.g. Windows Terminal),
    // or focus one and press Enter/Space. Pressing a button tints its background; the status line updates.
    static void ButtonDemo(string[] args)
    {
        var count = 0;

        var status = new TextLabel(TextLabelOrientation.Horizontal, "Count: 0".PadRight(38), Color.White);
        void SetStatus(string last) => status.Text = $"Count: {count}   (last: {last})".PadRight(38);

        var inc = ColorButton("Increment (+1)", new Color(30, 90, 50));
        var dec = ColorButton("Decrement (-1)", new Color(110, 70, 30));
        var reset = ColorButton("Reset", new Color(60, 60, 110));
        var quit = ColorButton("Quit", new Color(110, 40, 40));

        inc.Activated += (_, _) => { count++; SetStatus("Increment"); };
        dec.Activated += (_, _) => { count--; SetStatus("Decrement"); };
        reset.Activated += (_, _) => { count = 0; SetStatus("Reset"); };
        quit.Activated += (_, _) => Environment.Exit(0);

        var grid = new Jumbee.Console.Grid(
            [2, 3, 3, 3, 3],
            [40],
            [
                [status],
                [inc],
                [dec],
                [reset],
                [quit],
            ]);

        // Focus a button so Enter/Space works immediately; clicks work regardless of focus.
        // anyMotion: true → DEC 1003 so hover (mouse-over without a button held) updates the button background.
        var run = UI.Start(grid, width: 44, height: 16, isAnsiTerminal: true, input: new Jumbee.Console.VtInputSource(anyMotion: true));
        UI.SetFocus(inc);
        run.Wait();
    }

    static void GridTest(string[] args)
    {
        // --- Spectre.Console Controls ---
        // 1. Table
        var table = new Spectre.Console.Table();
        /*
        table.Title("[bold yellow]Jumbee Console[/]");
        table.AddColumn("Library");
        table.AddColumn("Role");
        table.AddColumn("Status");
        table.AddRow("Spectre.Console", "Widgets & Styling", "[green]Integrated[/]");
        table.AddRow("ConsoleGUI", "Layout & Windowing", "[blue]Integrated[/]");
        table.AddRow("Jumbee", "The Bridge", "[bold red]Working![/]");
        table.Border(TableBorder.DoubleEdge);
        */
        // 2. Bar Chart
        var barChart = new BarChart(ChartOrientation.Horizontal,
            ("Planning", 12, Yellow),
            ("Coding", 54, Green),
            ("Testing", 33, Red)
        )
        {
            BarWidth = 50,
            Label = "[green bold]Activity[/]",
            CenterLabel = true
        };
        var table3 = new Spectre.Console.Table()
                .AddColumn(new Spectre.Console.TableColumn("Line"));
        var disp = new SpectreLiveDisplay(table3);
        //disp.Display.Overflow = Spectre.Console.VerticalOverflow.Ellipsis;
        
        // 3. Tree
        var treeControl = new Tree("Root", guide: TreeGuide.BoldLine)
        {
            SelectedForegroundColor = Color.White,
            SelectedBackgroundColor = Color.Blue
        };
        treeControl.AddNode("Foo").AddChildren("Bar", "Baz", "Qux");

        // Example of adding a subtree (since AddNode takes IRenderable)
        //var subTree = new Tree(rootText: "Subtree");
        //subTree.AddNode("Leaf 1");
        //subTree.AddNode("Leaf 2");
        //treeControl.AddNode(subTree);

        // --- Wrap Spectre.Console Controls for ConsoleGUI ---
        //var tableControl = new SpectreControl<Spectre.Console.Table>(table);

        //tableControl.Content.Border = TableBorder.Rounded;
        // var chartControl = new SpectreControl<Spectre.Console.BarChart>(barChart); // No longer needed
        // var treeControl = new SpectreControl<Spectre.Console.Tree>(root); // Replaced by Jumbee.Console.Tree above

        // --- ConsoleGUI Controls ---
        // Spinner
        var spinner = new Spinner
        {
            SpinnerType = Spectre.Console.Spinner.Known.Dots,
            Text = "Waiting for input...",
            Style = Spectre.Console.Style.Parse("green bold")
        };
        spinner.Start();

        // The TextPrompt control
        var prompt = new TextPrompt("[yellow]What is your name?[/]", blinkCursor: true) { Width = 20};
        prompt.Committed += (sender, name) =>
        {
            spinner.Text = $"Hello, [blue]{name}[/]!";
            spinner.SpinnerType = Spectre.Console.Spinner.Known.Ascii; // Change spinner style on success
        };

        var p = prompt
            .WithAsciiBorder()
            .WithTitle("Write here");
        var grid = new Jumbee.Console.Grid([15, 15], [40, 80], [
            [spinner.WithFrame(borderStyle: BorderStyle.Rounded, fgColor: Red, title: "Spinna benz"), prompt],
            [disp, barChart]
        ]);
        //treeControl.IsFocused = true;
        // Start the user interface
        p.Focus();
        var t = UI.Start(grid, 130, 40);
        disp.Start((ctx) =>
        {
            for (int i = 1; i <= 100; i++)
            {
                //ctx.
                table3.Rows.Add([new Spectre.Console.Markup($"Line {i}")]);
                ctx.Refresh();
                Thread.Sleep(50);
            }
        });
        //UI.Start(internalGrid, width:250, height: 60, isTrueColorTerminal: true);
        // Create a separate timer to update the chartControl content periodically
        var random = new Random();
        var chartTimer = new Timer(_ =>
        {
            // The SpectreControl.Content setter will acquire the lock internally
            var newPlanning = (double)random.Next(10, 30);
            var newCoding = (double)random.Next(40, 70);
            var newTesting = (double)random.Next(20, 40);

            // Update existing items using the bulk-update indexer
            barChart["Planning", "Coding", "Testing"] = [newPlanning, newCoding, newTesting];

        }, null, 0, 50);

        t.Wait();        
    }
    
    static void ListBoxTest(string[] args)
    {
        var listBox = new ListBox
        {
            SelectedForegroundColor = Color.White,
            SelectedBackgroundColor = Color.DarkMagenta
        };
        listBox.AddItem("[red]Item 1 (Markup)[/]");
        listBox.AddItem("Item 2 (Green FG)", Color.Green);
        listBox.AddItem("Item 3 (Blue BG)", background: Color.Blue);
        listBox.AddItem("Item 4 (Yellow on Navy)", Color.Yellow, Color.Navy);

        var dynamicItem = listBox.AddItem("I will change color...", Color.White);

        var prompt = new TextPrompt("[yellow]Type a command (add/remove/clear/color/exit):[/]", blinkCursor: true) { Width = 80 };
        prompt.Committed += (sender, text) =>
        {
            if (text.StartsWith("add "))
            {
                var content = text.Substring(4);
                listBox.AddItem(content);
            }
            else if (text == "color")
            {
                dynamicItem.ForegroundColor = Color.FromSpectreColor(Spectre.Console.Color.FromInt32(new Random().Next(0, 255)));
                dynamicItem.Text = $"My color is now: {dynamicItem.ForegroundColor?.ToString() ?? "Default"}";
            }
            else if (text == "remove")
            {
                var items = listBox.Items.ToList();
                if (items.Count > 0)
                {
                    listBox.RemoveItem(items.Last());
                }
            }
            else if (text == "clear")
            {
                listBox.Clear();
            }
            else if (text == "exit")
            {
                Environment.Exit(0);
            }
        };

        var grid = new Jumbee.Console.Grid([20], [50, 50], [
            [listBox.WithRoundedBorder(Purple).WithHeight(3).WithWidth(5), prompt.WithFrame(title: "Controls")]
        ]);
        
        listBox.IsFocused = true;

        var t = UI.Start(grid);
        
        // Add some dynamic items automatically
        var random = new Random();
        var timer = new Timer(_ =>
        {
            var r = random.Next(0, 100);
            if (r < 30)
            {
                listBox.AddItem($"Auto Item {DateTime.Now.Second}");
            }
        }, null, 0, 1000);

        t.Wait();
    }
    
    static void TitleStyleTest(string[] args)
    {
        // Builds a framed label demonstrating one TitleStyle combination.
        Control Box(string content, BorderStyle border, TitlePos pos, TitleBorderStyle titleBorder, TitleColorStyle titleColor, Color color) =>
            new TextLabel(TextLabelOrientation.Horizontal, content, Color.White)
                .WithWidth(26)
                .WithHeight(1)
                .WithBorder(border, color)
                .WithTitle("Title", new TitleStyle(pos, titleBorder, titleColor));

        // Columns: Inline title (top, Reverse colors) vs. Double title (bottom, Normal colors).
        // Rows: Left, Center, Right alignment.
        var grid = new Jumbee.Console.Grid(
            [6, 6, 6],
            [30, 30],
            [
                [Box("inline top-left",      BorderStyle.Rounded, TitlePos.TopLeft,      TitleBorderStyle.Inline, TitleColorStyle.Reverse, Green),
                 Box("double bottom-left",   BorderStyle.Double,  TitlePos.BottomLeft,   TitleBorderStyle.Double, TitleColorStyle.Normal,  Red)],
                [Box("inline top-center",    BorderStyle.Heavy,   TitlePos.TopCenter,    TitleBorderStyle.Inline, TitleColorStyle.Reverse, Cyan1),
                 Box("double bottom-center", BorderStyle.Double,  TitlePos.BottomCenter, TitleBorderStyle.Double, TitleColorStyle.Reverse, Magenta1)],
                [Box("inline top-right",     BorderStyle.Square,  TitlePos.TopRight,     TitleBorderStyle.Inline, TitleColorStyle.Reverse, Yellow),
                 Box("double bottom-right",  BorderStyle.Double,  TitlePos.BottomRight,  TitleBorderStyle.Double, TitleColorStyle.Normal,  Blue)],
            ]);

        var t = UI.Start(grid, 64, 20);
        t.Wait();
    }

    static void ScrollBarStyleTest(string[] args)
    {
        // A list long enough to overflow its frame so the vertical scrollbar is shown.
        ListBox MakeList(Color selectColor)
        {
            var lb = new ListBox { SelectedForegroundColor = Color.White, SelectedBackgroundColor = selectColor };
            for (int i = 1; i <= 40; i++) lb.AddItem($"Item {i}");
            return lb;
        }

        var blockList  = MakeList(Blue);
        var shadedList = MakeList(Purple);
        var lineList   = MakeList(Green);

        blockList
            .WithRoundedBorder(Blue).WithTitle("Block")
            .WithScrollBarGlyphs(ScrollBarGlyphs.Block)
            .WithScrollBarStyle(ScrollBarStyle.Default.WithColors(thumb: Cyan1, arrows: White));
        shadedList
            .WithRoundedBorder(Purple).WithTitle("Shaded")
            .WithScrollBarGlyphs(ScrollBarGlyphs.Shaded)
            .WithScrollBarStyle(ScrollBarStyle.Default.WithColors(thumb: Magenta1, track: Silver, arrows: Red));
        lineList
            .WithRoundedBorder(Green).WithTitle("Line")
            .WithScrollBarGlyphs(ScrollBarGlyphs.Line)
            .WithScrollBarStyle(ScrollBarStyle.Default.WithColors(thumb: Yellow, arrows: Green));

        var grid = new Jumbee.Console.Grid(
            [20],
            [26, 26, 26],
            [[blockList, shadedList, lineList]]);

        // Focus one so Alt+Up / Alt+Down scrolls it and moves the thumb.
        blockList.IsFocused = true;

        var t = UI.Start(grid, 84, 24);
        t.Wait();
    }

    static void TreeAutoScrollTest(string[] args)
    {
        var tree = new Tree("Project", TreeGuide.BoldLine, Green | Dim)
        {
            SelectedForegroundColor = Color.White,
            SelectedBackgroundColor = Color.Blue
        };

        // Build a tree tall enough to overflow its frame so navigating scrolls the view.
        for (int i = 1; i <= 8; i++)
        {
            var folder = tree.AddNode($"Folder {i}");
            folder.AddChildren($"file{i}a.cs", $"file{i}b.cs", $"file{i}c.cs");
        }

        tree.WithRoundedBorder(Purple)
            .WithTitle("Tree (Up/Down to navigate)")
            .WithScrollBarGlyphs(ScrollBarGlyphs.Block)
            .WithScrollBarStyle(ScrollBarStyle.Default.WithColors(thumb: Cyan1, arrows: Yellow));

        tree.IsFocused = true;

        var grid = new Jumbee.Console.Grid(
            [16],
            [44],
            [[tree]]);

        var t = UI.Start(grid, 60, 20);
        t.Wait();
    }

    static void DockPanelTest(string[] args)
    {
        var p = new TextEditor(Language.Markdown, blinkCursor: false)
           .WithRoundedBorder(Purple)
           .WithTitle("Editor");
        var tree = new Tree("tree", TreeGuide.Line, Green | Dim) { Width = 20, Height = 10 };//.WithHeavyBorder();
        tree.AddNodes("Y".WithStyle(Red | Dim), "Z".WithStyle(Blue | Underline)).WithTitle("Functions");
        p.Focus();
        var d = new DockPanel(DockedControlPlacement.Left, tree, p);
        //var g = new Grid([10], [100, 100], [p, tree.WithRoundedBorder(Blue)]);
        // No explicit input source: UI.Start auto-selects a mouse-capable VtInputSource on an interactive terminal
        // (so clicking the tree/editor focuses it), falling back to keyboard-only when redirected/non-ANSI. Pass an
        // explicit VtInputSource(anyMotion: true) if you also want hover reporting.
        var t = UI.Start(d);
        t.Wait();
        Console.WriteLine("Average draw time: {0} Average paint time: {1}.", UI.AverageDrawTime, UI.AveragePaintTime);
        
    }

    static void AnsiControlSequenceBuilderTest(string[] args)
    {
        var cb = new AnsiControlSequenceBuilder();
        var t = new Stopwatch();
        t.Start();
        Console.CursorLeft = Console.CursorLeft + 9;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("This is red text that is really long xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
        Console.CursorLeft = Console.CursorLeft + 10;
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("This is blue text that is really long x");
        t.Stop();
        Console.WriteLine($"Elapsed: {t.ElapsedMilliseconds} ms");
        t.Reset();
        t.Restart();
        cb.MoveCursorRight(9);
        cb.SetForegroundColor(Red);
        cb.PrintLine("This is red text that is really longs xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
        cb.MoveCursorRight(10);
        cb.SetForegroundColor(Blue);
        cb.PrintLine("This is blue text that is really long x");
        //Task.Run(cb.WriteToSystemConsole2);
        cb.WriteToSystemConsole();
        t.Stop();
        Console.WriteLine($"Elapsed: {t.ElapsedMilliseconds} ms");
        Thread.Sleep(50);



    }
    /*
    static void Test2(string[] args)
    {
        // --- Helpers ---
        IControl CreateBox(string text, Jumbee.Console.Color color)
        {
            return new ConsoleGUI.Controls.Background
            {
                Color = color,
                Content = new ConsoleGUI.Controls.Border
                {
                    Content = new ConsoleGUI.Controls.TextBlock
                    {
                        Text = text,
                        Color = Jumbee.Console.Color.Black,
                    }
                }
            };
        }

        // --- 1. HorizontalStackPanel Test ---
        var hStack = new HorizontalStackPanel(
            CreateBox("H-Item 1", Red),
            CreateBox("H-Item 2", Green),
            CreateBox("H-Item 3", Blue)
        );
        //var hStackFrame = hStack.WithFrame(borderStyle: BorderStyle.Single, title: "Horizontal Stack");

        // --- 2. VerticalStackPanel Test ---
        var vStack = new Jumbee.Console.VerticalStackPanel(
            CreateBox("V-Item 1", Cyan1),
            CreateBox("V-Item 2", Magenta1),
            CreateBox("V-Item 3", Yellow)
        );
        //var vStackFrame = vStack.WithFrame(borderStyle: BorderStyle.Single, title: "Vertical Stack");

        // --- 3. DockedControl Test ---
        var dockedContent = CreateBox("Docked (Left)", DarkSlateGray1);
        var fillingContent = CreateBox("Filling Content", White);
        
        var dockedPanel = new Jumbee.Console.DockPanel(
            DockedControlPlacement.Left,
            dockedContent,
            fillingContent
        );
        //var dockedFrame = dockedPanel.WithFrame(borderStyle: BorderStyle.Single, title: "Docked Panel (Left)");

        var tabpanel = new TabPanel(TabBarDock.Top, tabs: [("Tab 1", CreateBox("T-Item 1", Magenta1)), ("Tab 2", CreateBox("T-Item 2", Cyan1))]);

        var vt = new TextLabel(TextLabelOrientation.Horizontal, "hello", Red);
        // --- Main Layout ---
        // Combine them into a grid for display
        var grid = new Jumbee.Console.Grid([20, 10, 20, 20], [60], [
            [tabpanel],
            [hStack],
            [vStack],
            [dockedPanel],
            
        ]);

        //var mainFrame = grid.WithFrame(borderStyle: BorderStyle.Double, title: "Jumbee Console Layout Tests");

        // Start the user interface
        UI.Start(grid);

        // Main loop
        while (true)
        {
            ConsoleManager.ReadInput([new InputListener()]);
            Thread.Sleep(50);
        }


    }
    */
}

// Example custom themes used by ToggleDemo's live theme switch. A theme overrides only the tokens/glyphs it
// wants; everything else falls back to the built-in defaults. Both set the frame border shape/colour
// (FrameBorder + BorderText + TitleText) so a switch visibly re-skins the frames around the radio/selection lists.
file sealed class CoolStyleTheme : IStyleTheme
{
    public BorderStyle FrameBorder => BorderStyle.Rounded;
    public Style BorderText => Style.Cyan1;
    public Style TitleText => Style.Cyan1;
    public Style TextAccent => Style.Green1;
    public Style Selection => Style.White | Style.Bg(new Color(40, 50, 80));
    public Style Primary => Style.White | Style.Bg(new Color(40, 70, 120));
    public Style PrimaryHover => Style.White | Style.Bg(new Color(60, 90, 150));
    public Style PrimaryActive => Style.White | Style.Bg(new Color(90, 130, 200));
    // A cool blue-tinted modal scrim, lightly dimmed so the controls behind stay clearly visible.
    public Style Scrim => Style.Bg(new Color(10, 15, 30));
    public float ScrimDim => 0.5f;
}

file sealed class RetroStyleTheme : IStyleTheme
{
    public BorderStyle FrameBorder => BorderStyle.Heavy;
    public Style BorderText => Style.Magenta1;
    public Style TitleText => Style.Magenta1;
    public Style TextAccent => Style.Magenta1;
    public Style Selection => Style.White | Style.Bg(new Color(80, 30, 80));
    public Style Primary => Style.White | Style.Bg(new Color(90, 30, 90));
    public Style PrimaryHover => Style.White | Style.Bg(new Color(120, 45, 120));
    public Style PrimaryActive => Style.White | Style.Bg(new Color(160, 70, 160));
    // A magenta-tinted modal scrim, more heavily dimmed for the retro look.
    public Style Scrim => Style.Bg(new Color(35, 10, 35));
    public float ScrimDim => 0.8f;
}

file sealed class RetroGlyphTheme : IGlyphTheme
{
    public string CheckboxChecked => "[*]";
    public string RadioSelected => "<o>";
    public string RadioUnselected => "< >";
    public string SwitchOn => "[ON ]";
    public string SwitchOff => "[OFF]";
}

