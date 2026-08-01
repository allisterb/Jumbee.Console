# Jumbee.Console — Documentation


## Start here
- [Getting Started](../GETTING-STARTED.md) — install, your first app, and the essential concepts.

## Reference
- [API](./api)

- [Internals](./internal)

## Control Guides (incomplete)
- [Layouts](controls/Layouts.md) — arranging controls, and **which layouts fill the terminal and which don't**
  (`DockPanel`, `SplitPanel`, `Grid`, `Boundary`, `Overlay`), with shell/master-detail/dashboard recipes.
- [Live Data](controls/Live%20Data.md) — sampling off the UI thread and updating controls on it: the
  snapshot-per-tick pattern, `UI.Invoke`/`Post`/`InvokeAsync`, sampling cadence, and reading `PerfHud`.
- [Selection Controls](controls/Selection%20Controls.md) — checkboxes, radio buttons, switches, and the
  single-/multi-select list controls (`RadioSet`, `SelectionList`).
- [Composite Controls](controls/Composite%20Controls.md) — building a single `Control` out of several child
  controls (`CompositeControl`), e.g. `CodeEditor` (editor + line-number gutter).

