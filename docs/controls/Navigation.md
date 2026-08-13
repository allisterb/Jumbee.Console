# Navigation

Buttons, menus, dialogs, tabs and help — the chrome that lets people move around an app and act on it.

## Choosing

| I want | Use |
|---|---|
| A button the user clicks or activates | `Button` |
| A menu bar across the top | `MenuBar` |
| A right-click menu | `ContextMenu` |
| To ask a question or confirm | `Dialog.Confirm` / `Dialog.Message`, or a custom `Dialog` |
| To swap between whole pages | [`TabPanel`](Layouts.md) |
| A key-hints bar along the bottom | [`Footer`](Display%20Widgets.md) |
| F1 help | `HelpInfo` on your controls, rendered by `HelpControl` |
| A clickable link | [`Link`](Links.md) |

`MenuBar`, `ContextMenu` and `Dialog` all float in the ambient `UI.Overlay`, which `UI.Start` sets up for you —
there's no overlay wiring to do.

## `Button`

A focusable, clickable button. Enter, Space or a click activates it.

```csharp
var save = new Button("Save");
save.Activated += (_, _) => Save();
```

`Text` is the label, `Activate()` fires it from code, and `Activated` is the event. Two static factories give you
the themed roles without building a style by hand:

```csharp
var ok     = Button.Primary("OK");         // the accent role — the action you expect
var cancel = Button.Secondary("Cancel");
```

`Style` takes a `ButtonStyle`, whose `Shape` decides the whole look: `ButtonShape.Flat` is a single-row text
button (the default), `ButtonShape.Modern` a solid three-row tile with a raised bevel derived from the fill
colour. `WithColors`, `WithShape` and `WithWidth` build a variant without spelling out every field.

```csharp
save.Style = save.Style.WithShape(ButtonShape.Modern);
```

Note the height difference: a `Modern` button occupies **three** rows, so a row of them needs the space allocated
for it. Buttons draw their own focus cue (`RendersOwnFocus`), so they don't take the default background tint.

For a button row inside a modal, you don't need any of this — [`Dialog`](#dialog) builds and drives its own.

## `MenuBar`

A horizontal bar of top-level titles, each opening a drop-down.

```csharp
var menu = new MenuBar();
menu.Add("File",
    new MenuItem("New",  NewFile),
    new MenuItem("Open", OpenFile),
    MenuItem.Separator,
    new MenuItem("Quit", UI.Stop));
menu.Add("View",
    new MenuItem("Theme", [new MenuItem("Dark", () => SetTheme(dark)),
                           new MenuItem("Light", () => SetTheme(light))]));

menu.ItemActivated += (_, item) => status.Text = item.Text;
```

Click a title to open it, or focus the bar and press Enter or Down; Left/Right move between titles. Choosing an item
runs its `Action` and raises `ItemActivated`.

`MenuItem` is `(text, action)` for a leaf or `(text, children)` for a submenu, with `MenuItem.Separator` for a
divider. `Enabled` greys an item out, and `Shortcut` displays a key hint beside it — display only, so register the
actual key yourself with `UI.RegisterHotKey`.

### Menus that report state

`MenuItem` is immutable, so items handed to `Add` are fixed for the life of the bar. When a menu should *show*
what the app is currently doing — which renderer is live, whether a panel is visible — pass a function instead and
it is rebuilt each time the menu opens:

```csharp
menu.Add("Render", () =>
[
    new MenuItem("Wireframe", () => scene.Use(wireframe)) { Checked = scene.Renderer == wireframe },
    new MenuItem("Solid",     () => scene.Use(solid))     { Checked = scene.Renderer == solid },
]);
```

`Checked` is a `bool?`: `true` draws a ✓, `false` leaves the marker column blank, and `null` (the default) means
the item isn't that kind of item at all. Any level containing a checkable item reserves the marker column across
**every** row, so plain commands in the same menu keep their labels aligned with the checkable ones.

Because the menu is rebuilt from live state, a keyboard shortcut and a menu click stay in agreement with no
synchronising code — the menu is a view of the state, not a second copy of it.

Dock it to the top of your shell:

```csharp
var root = new DockPanel(DockedControlPlacement.Top, menu, body);
```

## `ContextMenu`

The same items, opened at a position rather than from a bar — and it draws the whole open submenu chain itself, so
nesting works to any depth.

```csharp
var menu = new ContextMenu([
    new MenuItem("Copy",   Copy),
    new MenuItem("Paste",  Paste),
    MenuItem.Separator,
    new MenuItem("Delete", Delete),
]);

menu.ItemActivated += (_, item) => Announce(item.Text);
menu.Show(x, y);
```

Attach one to a list rather than showing it by hand, and rebuild the items for whatever the pointer is over:

```csharp
list.ContextMenuOpening += (_, _) => list.ContextMenu = MenuFor(list.SelectedItem);
list.ContextMenu = defaultMenu;
```

`ListBox` and `Tree` both support this. `Closed` fires whenever the menu closes, however it closed.

## `Dialog`

A modal over `UI.Overlay`. The two common cases are static helpers:

```csharp
Dialog.Message("Saved", $"Wrote {count} rows.");

Dialog.Confirm("Quit", "Discard unsaved changes?", yes => { if (yes) UI.Stop(); });
```

For anything richer, build one around your own content control:

```csharp
var dialog = new Dialog("Connect", hostControl, DialogButtons.OkCancel);
dialog.Completed += (_, result) => { if (result == DialogResult.Ok) Connect(); };
dialog.Show();
```

> **The content parameter is a `Control`, not an `ILayout`.** You can't pass a `VerticalStackPanel` or a `Grid`
> directly. To lay several controls out inside a dialog, put them in a `CompositeControl` — that's a single
> `Control` whose interior is a layout — and pass that. See [Composite Controls](Composite%20Controls.md).

Buttons are keyboard-navigable with ←/→ or Tab, Enter or Space activates, and **Escape always cancels** — the
dialog owns that key. `Completed` carries the `DialogResult`; `Close(result)` dismisses it from code.

The dialog shrinks to fit its content, and dims the layer behind it.

> **Testing a modal:** snapshot the *overlay*, not the root — `Dialog.Show(overlay)` with
> `new Overlay(root)` is the pattern, and the full recipe is in
> [GETTING-STARTED](../../GETTING-STARTED.md). Snapshotting the root captures the app without the dialog, which
> looks like the dialog never rendered.

## `FileBrowser`

A two-pane file chooser — folders on the left, the current directory's contents on the right, a path field above
and a filter drop-down below. Two static helpers cover the common cases:

```csharp
FileBrowser.OpenFile("Load a model", start: null, filters: ["*.obj"], path =>
{
    if (path is not null) Load(path);        // null means the user cancelled
});

FileBrowser.OpenDirectory("Choose a folder", start: null, directory =>
{
    if (directory is not null) Scan(directory);
});
```

Both wrap the browser in a `Dialog` with OK/Cancel and report the choice exactly once. `start` may be a directory,
or a **file** — in which case its directory opens with the file already highlighted. A `start` that no longer
exists falls back to the working directory rather than refusing to open.

Filters are glob patterns; several in one string (`"*.jpg;*.png"`) list as one group, which is how a user thinks of
"images". `FileBrowser.AllFiles` (`*.*`) is always offered as the last option, so a filter can never leave someone
unable to see that they are in the wrong folder.

A single click only **selects**, so you can look through a listing without acting on it. Enter or a double-click
**opens a folder and chooses a file** — the same gesture doing the obvious thing on either kind of row. The `..`
row goes up, and Tab moves between the three panes.

Place it directly for a browser embedded in a pane rather than floating over one:

```csharp
var browser = new FileBrowser(startPath, FileBrowserMode.OpenFile, ["*.csv"]);
browser.SelectionChanged += (_, path) => preview.Load(path);
browser.PathActivated += (_, path) => Open(path);
```

`SelectedPath` is the current answer — the highlighted file, or in `OpenDirectory` mode the listed directory while
nothing else is highlighted, so OK always has something to return. `CurrentDirectory` reads and sets where it is
looking, and `ShowHidden` lists hidden and system entries.

The tree shows the directory being listed and what is under it, re-rooting whenever the listing moves; going up or
elsewhere is what the `..` row and the path field are for. Directories are read on demand, so a folder with a large
tree under it costs nothing until you open it, and an unreadable directory shows a message in the pane rather than
throwing.

## `TabPanel`

`TabPanel` is a layout — it belongs to [Layouts](Layouts.md) — but it's how most apps do top-level navigation, so
the essentials are here.

```csharp
var tabs = new TabPanel(TabBarDock.Top,
    ("Overview", overviewPage),
    ("Logs",     logsPage),
    ("Settings", settingsPage));

tabs.SelectionChanged += (_, i) => Refresh(i);
```

Click a label or use the arrow keys while the bar is focused. `SelectedIndex`, `ActiveTab` and `ActiveTabName` read
the current tab; `SelectTab` sets it.

Tabs are mutable at runtime — `AddTab` and `RemoveTab`, plus `Closable`, `IsHidden` and `IsDisabled` on the returned
`TabItem`. **Hold the `TabItem`, not an index**: its identity is stable across structural changes and an index isn't.

`ClosableTabs` puts a ✕ on each tab and `ShowAddButton` a `+` at the end of the bar, with `TabCloseRequested` and
`NewTabRequested` to handle them. That's what `MultiTabCodeEditor` is built from.

## Help

Help is opt-in per control and assembled globally. A control describes itself by overriding `GetHelpInfo()`:

```csharp
protected override HelpInfo GetHelpInfo() =>
    new HelpInfo("Process list", "Shows running processes, refreshed every second.")
        .WithKey("↑/↓", "move selection")
        .WithKey("Enter", "show details")
        .WithKey("k", "kill the selected process");
```

`UI` collects these and shows them in a `HelpControl` — a modal with one tab per `HelpInfo`, each rendering the
markup text with its key bindings listed below. Escape closes it. `KeysInline` puts the keys in the body instead of
a list.

Most built-in controls already return their own help, so it fills in as you compose an app.

For the always-visible version, use a [`Footer`](Display%20Widgets.md) — a one-row hints bar, typically mirroring
your global hotkeys.

## See also

- [Layouts](Layouts.md) — `TabPanel` and `Overlay` in full, and building the shell.
- [Links](Links.md) — the `Link` control and wiring app-level keys.
- [Display Widgets](Display%20Widgets.md) — `Footer` and the other status chrome.
