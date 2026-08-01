# Navigation

Menus, dialogs, tabs and help — the chrome that lets people move around an app and act on it.

## Choosing

| I want | Use |
|---|---|
| A menu bar across the top | `MenuBar` |
| A right-click menu | `ContextMenu` |
| To ask a question or confirm | `Dialog.Confirm` / `Dialog.Message`, or a custom `Dialog` |
| To swap between whole pages | [`TabPanel`](Layouts.md) |
| A key-hints bar along the bottom | [`Footer`](Display%20Widgets.md) |
| F1 help | `HelpInfo` on your controls, rendered by `HelpControl` |
| A clickable link | [`Link`](Links.md) |

`MenuBar`, `ContextMenu` and `Dialog` all float in the ambient `UI.Overlay`, which `UI.Start` sets up for you —
there's no overlay wiring to do.

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

## `TabPanel`

`TabPanel` is a layout — it belongs to [Layouts](Layouts.md) — but it's how most apps do top-level navigation, so
the essentials are here.

```csharp
var tabs = new TabPanel(TabBarDock.Top,
    ("Overview", overviewPage),
    ("Logs",     logsPage),
    ("Settings", settingsPage));

tabs.SelectionChanged += i => Refresh(i);      // Action<int>, not EventHandler
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
