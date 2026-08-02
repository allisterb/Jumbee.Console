#:package Jumbee.Console@*

using Jumbee.Console;

using static Jumbee.Console.Color;   // import the static color names

var count = 0;

var label = new TextLabel(TextLabelOrientation.Horizontal, "Count: 0", Cyan1);
var button = new Button("Increment");

// Change the label text when the button is clicked or pressed
button.Activated += (_, _) => label.Text = $"Count: {++count}"; 

// One column, two rows: the label above the button.
var root = new Grid(
    columnWidths: [30],
    rowHeights: [1, 3],
    controls:
    [
        [label],
        [button],
    ]);

UI.RegisterHotKey(UI.HotKeys.Escape, UI.Stop);   // Esc quits (Ctrl+Q also quits by default)
UI.SetFocus(button);                             // focus it so Enter/Space activates

// Start the UI with a width/height. Mouse/hover need a VtInputSource.
// Returns a Task.
var t = UI.Start(root, width: 34, height: 6, input: new VtInputSource(anyMotion: true));

// Wait until the UI stops.
t.Wait();
