# Live data: sampling, threading and redraw

Most non-trivial TUIs are the same shape: something produces data continuously — a metrics sampler, an audio
device, an HTTP feed, a log tail — and the UI has to show it without stuttering, tearing or drifting. This guide is
how to wire that up.

The rule is one sentence: **do the work off the UI thread, and touch controls only on the UI thread, once per
sample.** Everything below is a consequence of it.

## Why: there is exactly one UI thread, and no lock

Jumbee runs all UI state and rendering on a single dedicated thread. There is no lock around control state — that's
deliberate, and it's why frames are cheap. The cost is that you cannot mutate a control from a background thread
and hope for the best: there's nothing protecting the control's internals from your writes.

The failure isn't hypothetical or rare. A background sampler appending to a chart's sample list while the render
path enumerates it produces an intermittent "collection was modified" crash or a torn frame, and it surfaces under
load — precisely when someone is watching.

> **A data race shows up as a crash, not as contention.** Unsynchronized access takes no locks, so it will not
> appear in any lock-contention counter. Zero contention does not mean your threading is correct.

## The pattern: one snapshot, one marshal

Build an immutable snapshot off-thread, hand it over in **one** marshaled call, and let a single handler apply it to
every control. One hop per sample, one consistent frame, and no per-control discipline to remember.

```csharp
// The snapshot: everything the UI needs for one frame, immutable, built off-thread.
public sealed record Sample(double CpuPercent, double MemoryPercent, IReadOnlyList<ProcessGroup> Groups);

async Task SampleLoop(CancellationToken ct)
{
    using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(200));
    while (await timer.WaitForNextTickAsync(ct))
    {
        var sample = _sampler.Take();          // all the expensive work, off the UI thread

        UI.Invoke(() =>                        // one hop; everything below runs on the UI thread
        {
            cpuChart.Push(sample.CpuPercent);
            memChart.Push(sample.MemoryPercent);
            header.Clock = DateTime.Now;
        });
    }
}
```

`PeriodicTimer` is worth preferring over `await Task.Delay(interval)` at the end of the body: the latter makes the
real period *work + interval*, so a slow pass silently stretches your sampling cadence and the chart's time axis
stops meaning what it says.

## Choosing what to stream into

Before the plumbing, pick the right control — the wrong one is hard to notice until the shape is finished:

| You want | Use |
|---|---|
| A one-row inline trend | [`Sparkline`](Display%20Widgets.md#sparkline) — block bars, one cell per value |
| Axes, ticks, a legend, several series | [`Plot`](../api/Jumbee.Console.Plot.md), and `AddLiveSeries`/`PlotSeries` to push into it |
| A **filled/area chart at sub-cell resolution** — the dense braille look of a system monitor | [`Canvas`](../api/Jumbee.Console.Canvas.md) with `CanvasMarker.Braille` (the default) and one [`Drawing.FilledLine`](../api/Jumbee.Console.Drawing.FilledLine.md) per column |
| An append-only tail | [`Log`](Display%20Widgets.md) — it owns its own scrolling and virtualisation |
| Rows that change every tick | `DataTable` — but it has no in-place row update, so expect to rebuild and restore the selection (see below) |

The braille one is the least discoverable: `Canvas` reads as a general drawing surface, and `Plot`'s bar methods
take no braille brush, so a filled sub-cell chart is a few lines on `Canvas` rather than a chart control you can
switch on.

## Choosing the marshal

| Call | Behaviour | Use it for |
|---|---|---|
| `UI.Invoke(action)` | Runs inline if you're already on the UI thread, otherwise posts. **Requests a redraw.** | Almost everything — applying a sample, reacting to an event. |
| `UI.Post(action)` | Always defers to a later frame. Does **not** request a redraw. | Deliberately deferring work; coalescing (see below). |
| `UI.InvokeAsync(action)` | Returns a task that completes when the action has run. | When you need the result, or need to observe the action's exception. |

> **`UI.Invoke` does not block, and it does not surface the action's exception to the caller.** If you are coming
> from WPF, this is not `Dispatcher.Invoke` — it is closer to `BeginInvoke`. An exception thrown inside the action
> will not appear at the call site. Use `UI.InvokeAsync` and await it when you need either guarantee.

## What you may write directly, and what you may not

- **Simple scalar properties on controls marshal themselves.** Control property setters route through the
  library's atomic-property helper, so assigning a number, string, colour or enum from a background thread is safe
  and invalidates correctly.
- **Everything else must be marshaled by you.** Collections (adding rows, pushing samples into a list), mutating a
  wrapped Spectre renderable's content, or any change that has to be consistent across several fields — those are
  not atomic, and they are exactly what a live app does most.

When in doubt, marshal. `UI.Invoke` runs inline when you're already on the UI thread, so wrapping something that
didn't need it costs a delegate, not a frame.

## Cadence: don't tie expensive work to the frame rate

Split sampling by cost, not by convenience. A system monitor typically wants three different rates:

| Work | Typical rate | Why |
|---|---|---|
| A counter feeding a chart (a level meter, a queue depth, one performance counter) | 200–300 ms | The chart's time resolution; spikes are lost if this is slow. |
| Expensive enumeration (process list, directory scan, HTTP poll) | 1–5 s | Costs milliseconds and rarely changes that fast. |
| Redraw | driven by the UI loop | Not something you schedule per sample. |

One global tick makes your fastest signal hostage to your slowest work: a chart on the same timer as a full process
enumeration advances at the enumeration's pace, and short spikes are averaged away before they are ever drawn.

> **Don't assume the "fast" signal is cheap — measure it.** The rate a value *should* update at says nothing about
> what it costs to obtain. System-wide CPU on .NET is the classic trap: there is no cheap portable BCL call for it,
> and reconstructing it by enumerating every process and summing `TotalProcessorTime` costs milliseconds *and*
> throws on every process you aren't allowed to read. Do that at 300 ms and a monitor idles at a fifth of a core.
> If a "cheap" counter turns out to be expensive, either move it to the slow tick and accept coarser resolution, or
> find a cheaper source (a platform performance counter, a single `/proc` read) — but decide it with a measurement,
> not by which column of this table it looks like it belongs in.

## Lifecycle: cancel it, and observe its faults

```csharp
var cts = new CancellationTokenSource();
var loop = Task.Run(() => SampleLoop(cts.Token));   // keep the Task; don't discard it

// on shutdown
cts.Cancel();
try { await loop; } catch (OperationCanceledException) { }
```

Two failure modes worth naming, because both are silent:

- **Fire-and-forget** (`_ = Task.Run(...)`) with no cancellation outlives `UI.Stop`, and one unhandled exception
  ends the loop for good. The app keeps running with a frozen chart and stale numbers, and nothing anywhere says
  why. Keep the task, cancel it, and await it on shutdown.
- **A blanket `catch { }` around the whole tick** hides the bug that killed your data. Catch per-operation, count
  failures, and surface them somewhere you can see during development.

## Keeping the frame path cheap

A monitor redraws forever, so per-frame cost is a product characteristic, not a micro-optimisation. The things that
actually bite:

- **Exceptions as control flow.** If a per-item read throws for a predictable, permanent reason — a process you're
  not allowed to query, a file you can't stat — that condition won't change between ticks. Probe once, cache the
  verdict, and stop paying for it. Hundreds of throws per second is both a real cost and a mess in any profile.
- **Rebuilding what didn't change.** Regenerating an entire shape set, row set or string on every paint when the
  data changed once a second. Rebuild on data or size change, gated by a dirty flag.
- **Growing buffers.** A history `List<T>` with `RemoveAt(0)` at its cap memmoves the whole buffer on every sample.
  Use a fixed-size ring.
- **Tearing down views.** Rebuilding a whole view each tick when a fraction of it changed. Where a control lets you
  update in place, diff against a stable key and do that instead of clearing and refilling.

  **`DataTable` is the exception, and it's the one you'll hit.** Its row API is `AddRow` / `RemoveRow(int)` /
  `Clear()` — there is no `UpdateRow` and no row indexer, so a changing table *must* be rebuilt, and the rebuild
  drops the selection. Restore it by key rather than by index, since rows reorder:

  ```csharp
  // Before the rebuild: remember what the user had selected, by identity — not by row number.
  var selectedKey = table.SelectedRow?[0];

  table.Clear();
  foreach (var p in snapshot)
      table.AddRow(p.Name, p.Cpu.ToString("F1"), p.Memory.ToString("F1"));

  // After: find that key again. Rows move between ticks, so the old index means nothing.
  if (selectedKey is not null)
  {
      for (var i = 0; i < table.RowCount; i++)
      {
          table.SelectedIndex = i;
          if (table.SelectedRow?[0] == selectedKey) break;
      }
  }
  ```

  Keep the whole sequence inside one `UI.Invoke` so the table is never half-rebuilt on a painted frame.

Also see `Canvas.DamageTracking` and `Control.TracksDamage` for opting into partial redraw — worthwhile when a small
region of a large surface changes, and measurably *not* worthwhile when the whole picture changes every frame.

## Measuring it

Drop a `PerfHud` over the app and read it while it runs:

```csharp
var hud = new PerfHud();
hud.ShowTopRight();              // floats over the app in the ambient overlay
hud.RegisterToggle();            // a hotkey to show/hide it
```

What the counters mean, and what good looks like:

| Counter | Reading it |
|---|---|
| `frame` | Time for the last frame, in µs, and the retained-frame count. |
| `busy` | UI-thread utilisation. Near 0 when idle; sustained high means the UI thread is doing work that belongs on a background one. |
| `redraw` / `dirty` | How much of the surface is being repainted. Persistently ~100% dirty on a mostly-static screen means something invalidates too broadly. |
| `alloc` | Bytes allocated per frame. This is the one that catches per-frame rebuilds. |
| `exc/s` | First-chance exceptions per second, thrown *or* caught. **Should be 0.** A steady non-zero rate is exceptions used as control flow. |
| `locks` | `Monitor.LockContentionCount`, **cumulative since process start** — not a rate. A single-threaded UI design should hold this at or near 0. |

Two caveats worth internalising:

- **`locks` is a running total**, so what matters is whether it *climbs* while the app sits there, not its absolute
  value at a glance.
- **`locks` measures contention, not correctness.** The dangerous bug — a background thread writing control state
  with no synchronization at all — produces *zero* contention and still corrupts. Use the counter to confirm you
  haven't introduced locking, not to prove your threading is right.

## Checklist

- [ ] All sampling, parsing, I/O and enumeration happen off the UI thread.
- [ ] The UI is touched in exactly one place per sample, through `UI.Invoke`.
- [ ] The thing handed across is an immutable snapshot, not shared mutable state.
- [ ] Cheap and expensive sampling run on separate cadences.
- [ ] The loop is cancellable, its task is retained, and its faults are observed.
- [ ] Per-item failures are counted, not blanket-swallowed.
- [ ] `exc/s` reads 0 and `locks` isn't climbing.

## See also

- [Layouts](Layouts.md) — building the shell the live data renders into.
- [Composite Controls](Composite%20Controls.md) — packaging a live widget as a reusable control.
- [Getting started §1 — The single UI thread](../../GETTING-STARTED.md#1-the-single-ui-thread).
- API: [`UI`](../api/Jumbee.Console.UI.md) · [`PerfHud`](../api/Jumbee.Console.PerfHud.md) ·
  [`Canvas`](../api/Jumbee.Console.Canvas.md)
