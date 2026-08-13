# Selection Controls

The selection family lets users pick and toggle values. It comes in two groups:

| Control | Picks | Renders | Multiple? |
|---------|-------|---------|-----------|
| `Checkbox` | one on/off value | `[X]` / `[ ]` Label | independent |
| `RadioButton` | one on value | `(●)` / `( )` Label | latches on |
| `Switch` | one on/off value | `(─●)` / `(●─)` Label | independent |
| `RadioSet` | one option from a list | a column of `(●)`/`( )` rows | single-select |
| `SelectionList` | any options from a list | a column of `[X]`/`[ ]` rows | multi-select |
| `Select` | one option from a list | one row: the value plus `▼`, options in a pop-up | single-select |
| `Slider` | a number in a range | one row: a track with a draggable thumb | continuous |

`RadioSet` and `Select` both pick one option from a list; the difference is space. `RadioSet` shows every option
all the time, which is right for a handful of choices the user should be able to compare. `Select` costs one row
whatever the list length, at the price of a click to see the options — right for long lists, and for forms where
vertical space matters more than visibility.

All of them are in the `Jumbee.Console` namespace and are used like any other control — placed in a layout and
shown with `UI.Start`.

> **Naming note:** `Switch` collides with `System.Diagnostics.Switch`. If your file imports `System.Diagnostics`,
> qualify it as `Jumbee.Console.Switch`.

## A first example

```csharp
using Jumbee.Console;

var accept = new Checkbox("Accept terms");
accept.Changed += (_, isChecked) =>
{
    // react to the new state
};

// One column, one row, the checkbox in it.
var grid = new Grid(rowHeights: [1], columnWidths: [30], controls: [[accept]]);

// Mouse needs a VT terminal + a VtInputSource; keyboard-only works with the default input.
var run = UI.Start(grid, width: 32, height: 3, input: new VtInputSource(anyMotion: true));
UI.SetFocus(accept);   // so Space/Enter toggles it immediately; clicking works regardless of focus
run.Wait();
```

Controls **auto-size** to their content (indicator + label), so you normally let the layout decide placement and
leave `Width`/`Height` alone.

## Single toggles: `Checkbox`, `RadioButton`, `Switch`

The three single toggles share a base (`ToggleButton`) and therefore the same surface. A user toggles one by
**clicking it**, or by **focusing it and pressing Enter or Space**.

```csharp
var notify = new Checkbox("Enable notifications", isChecked: true);
var dark   = new Jumbee.Console.Switch("Dark mode");   // qualified to avoid System.Diagnostics.Switch
var option = new RadioButton("Option A");
```

### Shared members

| Member | Type | Description |
|--------|------|-------------|
| `IsChecked` | `bool` | The current state. Set it to change the control programmatically. |
| `Changed` | `event EventHandler<bool>` | Raised with the new state whenever `IsChecked` changes. |
| `Text` | `string` | The label. Changing it re-sizes the control. |
| `Toggle()` | `void` | Flips the state (the same path a click takes). |

```csharp
notify.Changed += (_, on) => status.Text = on ? "Notifications on" : "Notifications off";

if (notify.IsChecked) { /* ... */ }   // read it any time
notify.IsChecked = false;             // set it programmatically (also raises Changed)
```

### `RadioButton` vs `Checkbox`

A `RadioButton` **latches**: activating it always turns it *on* (a click never turns it off), which is the
expected behaviour for "pick one of several." A standalone `RadioButton` doesn't coordinate with its siblings,
so for a real mutually-exclusive group reach for **`RadioSet`** (below) — it manages the exclusivity for you.

### `Switch`

`Switch` is an on/off control identical in behaviour to `Checkbox`; it just renders as a slider. Its constructor
names the initial state `isOn`:

```csharp
var wifi = new Jumbee.Console.Switch("Wi-Fi", isOn: true);
```

## Single-select list: `RadioSet`

`RadioSet` is a vertical group of mutually-exclusive options — exactly one is selected at a time. Construct it
with the option labels:

```csharp
var theme = new RadioSet("Light", "Dark", "Solarized") { SelectedIndex = 0 };
theme.SelectionChanged += (_, index) =>
{
    string picked = theme.SelectedValue!;   // or use `index`
};
```

The user navigates with **Up/Down** and chooses with **Space/Enter**, or simply **clicks a row**.

| Member | Type | Description |
|--------|------|-------------|
| `SelectedIndex` | `int` | Index of the selected option, or `-1` when nothing is selected. Settable. |
| `SelectedValue` | `string?` | The selected option's text, or `null`. |
| `SelectionChanged` | `event EventHandler<int>` | Raised with the new index when the selection changes. |
| `Options` | `IReadOnlyList<string>` | The option labels. |

## Multi-select list: `SelectionList`

`SelectionList` is a vertical checklist — each option is independently checkable.

```csharp
var toppings = new SelectionList("Cheese", "Mushroom", "Pepperoni", "Olives");
toppings.SetChecked(0, true);     // pre-check "Cheese"
toppings.SelectionChanged += (_, _) =>
{
    IReadOnlyList<string> chosen = toppings.SelectedValues;   // e.g. ["Cheese", "Pepperoni"]
};
```

The user navigates with **Up/Down** and toggles the highlighted row with **Space/Enter**, or **clicks a row**.

| Member | Type | Description |
|--------|------|-------------|
| `SelectedIndices` | `IReadOnlyList<int>` | Indices of the checked options, ascending. |
| `SelectedValues` | `IReadOnlyList<string>` | Text of the checked options, in option order. |
| `SetChecked(int index, bool isChecked)` | `void` | Check/uncheck an option (raises `SelectionChanged` when it changes). |
| `IsCheckedAt(int index)` | `bool` | Whether a given option is checked. |
| `SelectionChanged` | `event EventHandler<int>` | Raised with the affected index when an option's checked state changes. |
| `Options` | `IReadOnlyList<string>` | The option labels. |

Both list controls also expose `CursorIndex` (the highlighted row), which you can set to move the keyboard cursor
programmatically.

## Collapsed single-select: `Select`

A drop-down. Closed, it shows the current value and a `▼`; opening it floats the options in the ambient
`UI.Overlay`.

```csharp
var method = new Select("GET", "POST", "PUT", "DELETE") { Placeholder = "method" };
method.SelectionChanged += (_, value) => request.Method = value;
```

`SelectedIndex` and `SelectedValue` read and set the choice, `Options` is the list, and `Placeholder` is shown
while nothing is selected. `Open()` drops the list from code.

Clicking it — or Enter/Space while focused — opens the list; choosing an option commits it and raises
`SelectionChanged`. **Escape, or a click outside, cancels** and leaves the previous value alone. By default the
list opens below the control and flips above when there isn't room; `PopupPosition` pins it if you'd rather it
didn't move.

Because the pop-up uses `UI.Overlay`, which `UI.Start` sets up, there's no overlay wiring to do — but it does mean
the list is drawn above everything else, so don't rely on the control's own bounds when reasoning about what's on
screen.

**Width.** By default the closed control fills whatever width its layout offers, with the `▼` at the right edge —
the form convention, and what lines a column of fields up. The pop-up is always sized to the widest option, so in a
narrow panel of mixed controls the two don't match and a three-word choice becomes a full-width block of colour.
`FitContent` closes that gap:

```csharp
var shape = new Select("box", "sphere", "mesh") { FitContent = true };   // as wide as its list, no wider
```

Options can change at runtime — `SetOptions` replaces them, keeps the current value selected if it survives, and
re-sizes the control:

```csharp
models.SetOptions(LoadedModelNames());
```

## A number in a range: `Slider`

Everything above picks from a fixed set. `Slider` picks a number: an optional label, a track filled to the current
value, a thumb at the fill's edge, and an optional readout.

```csharp
var gravity = new Slider(minimum: 0, maximum: 30, value: 9.8, label: "Gravity");
gravity.ValueChanged += (_, value) => world.Gravity = value;
```

Drag the thumb, click anywhere on the track, or use the keyboard: Left/Right (and Up/Down) move by `Step`, Page
Up/Down by ten of them, Home/End jump to the ends, and the wheel steps while the pointer is over the control. Hold
Shift with an arrow for a fifth of a step. `Step` defaults to a hundredth of the range.

`ValueChanged` fires only when the value actually moves, and `Value` is always clamped into the range — so setting
it out of bounds is safe rather than something to guard.

```csharp
var servings = new Slider(0, 12, 4, "Servings")
{
    Step = 1,
    SnapToStep = true,     // every path quantises, including a drag
    ValueFormat = "F0",    // "4" rather than "4.00"
};
```

**Aligning a stack of them.** Set `LabelWidth` to a common value and every track starts in the same column,
whatever the labels are:

```csharp
foreach (var s in new[] { gravity, friction, bounce })
    s.LabelWidth = 10;
```

The readout is right-aligned in a field wide enough for either end of the range, so the track keeps its width as
the digits change — `9.99` and `10.00` do not shift it.

The thumb occupies one whole cell, so a column of sliders reads as a row of controls rather than as a bar chart.
Only the thumb's *position* rounds to a cell; the value itself stays continuous, so a drag still reports the
fraction it landed on.

## Layout, focus, and frames

**Placement.** Add controls to a layout such as `Grid` and pass it to `UI.Start`:

```csharp
var grid = new Grid(
    rowHeights:    [1, 1, 5],          // a row per control; the list needs a few rows
    columnWidths:  [40],
    controls:
    [
        [notify],
        [dark],
        [theme],                       // a 3-option RadioSet
    ]);
UI.Start(grid, width: 44, height: 9, input: new VtInputSource(anyMotion: true)).Wait();
```

**Focus.** Keyboard input goes to the focused control. Use `UI.SetFocus(control)` to set it, and `control.IsFocused`
to read it. Clicking a control also focuses it. (Mouse events require a VT terminal and a
`VtInputSource`; pass `anyMotion: true` to get hover highlighting.)

**Frames.** Any control can be wrapped in a border/title — handy for the list controls:

```csharp
theme.WithRoundedBorder().WithTitle("Theme");
toppings.WithRoundedBorder(Color.Green).WithTitle("Toppings");
```

`WithFrame`, `WithBorder`, `WithRoundedBorder`, and `WithTitle` all return the control, so they chain. If a list
is taller than its frame, the frame shows a scrollbar and the keyboard cursor auto-scrolls to stay in view.

## Styling

Each control's colours come from the active theme by default, so an app-wide look is set once via the theme
(`UI.StyleTheme` / `UI.GlyphTheme`) rather than per control. When you need a one-off, set the style properties
directly — they take a `Style` (and a plain `Color` converts to one):

```csharp
var cb = new Checkbox("Custom")
{
    AccentStyle = Color.Magenta1,     // the checked mark
    MutedStyle  = Color.Grey50,       // the unchecked mark
    LabelStyle  = Color.White,
};
```

| Control(s) | Style properties |
|------------|------------------|
| `Checkbox` / `RadioButton` / `Switch` | `LabelStyle`, `AccentStyle` (checked), `MutedStyle` (unchecked), `HoverStyle` |
| `RadioSet` / `SelectionList` | `TextStyle`, `AccentStyle` (selected/checked mark), `MutedStyle`, `SelectionStyle` (highlighted row) |
| `Slider` | `Style` (a `SliderStyle` of label/fill/track/thumb/value), `ThumbGlyph`, `HoverStyle`, `FocusedStyle` |

Explicit values like these survive a runtime theme switch; everything you leave unset keeps following the theme.

## Behaviour summary

| | Activate by | Keyboard |
|---|-------------|----------|
| `Checkbox` / `RadioButton` / `Switch` | click | Enter / Space toggles (when focused) |
| `RadioSet` | click a row | Up / Down move, Enter / Space select |
| `SelectionList` | click a row | Up / Down move, Enter / Space toggle |
| `Select` | click to open, click an option | Enter / Space opens, Up / Down move, Enter selects, Esc cancels |
| `Slider` | drag the thumb, click the track, wheel | Arrows step (Shift = fine), PgUp / PgDn by ten, Home / End to the ends |

Double-clicking a toggle counts as two activations (e.g. a checkbox ends where it started), so rapid clicks
behave predictably.
