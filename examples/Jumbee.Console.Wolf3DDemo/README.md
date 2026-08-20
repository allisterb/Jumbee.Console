# Wolfenstein 3D walkthrough

Walk through the original 1992 game's levels in a terminal. Real maps, real wall textures, real scenery sprites,
cast by a real raycaster, with doors that open and a weapon that fires — no enemies, no pickups, no sound.

```bash
dotnet run --project examples/Jumbee.Console.Wolf3DDemo -c Release
```

You need the game's own data files first — see [`GameData/README.md`](GameData/README.md). They are not
redistributable and are not in this repository.

| key | |
|---|---|
| `w` `s` | walk forward and back |
| `a` `d` | turn |
| `q` `e` | strafe |
| `shift` | run |
| `space` / `enter` | open or close the door you are facing |
| `f` | fire |
| `[` `]` | previous / next level |
| `r` | back to the start marker |
| `1` `2` `3` | colour quantisation, quadrant sampling, field of view |
| `u` | show/hide the sidebar |
| `ctrl+tab` | next sidebar page |
| `esc` | back to the viewport, or quit if it already has focus |

## The sidebar

A `TabPanel` on the right, one page per group of knobs, all live:

- **Display** — quantisation level, surface mode, authentic field of view, scenery sprites, weapon. Dragging Quantize and
  watching the footer's run count halve while the picture barely moves is the fastest way to see this demo's
  central finding. (The cost readouts stay in the footer: they are true of the app rather than of a page, and the
  footer is the one thing that cannot be hidden.)
- **Input** — a **movement pad** (forward/back, turn, strafe, open, fire), the held-key inference (first press, coast, repeat
  gap, windows) and the movement speeds, plus **the auto-repeat interval measured from your own keystrokes**. That
  last one is a property of your machine rather than of the demo, and it is the number the other knobs should be
  set against.

The pad does not synthesise keystrokes. Its buttons call `Wolf3DView.Send`, the same entry point the keys use, so
**a click is exactly a key tap** — same sustain window, same coast, same Input-tab knobs governing both. That also
means a click moves as far as a tap does, about a tile; turn `Coast` and `First press` down if you want finer
steps, and the keyboard tightens identically because there is only one path.

Widgets and keys stay in agreement because neither talks to the other: the state objects own the truth and raise a
change event, a widget writes to the state, and the panel reads it back. Hiding the sidebar collapses it to zero
width rather than removing it from the tree, so every knob is where you left it when it comes back.

## How it is put together

The engine under [`engine/`](engine/README.md) is [Wolfenshine](https://github.com/deanthecoder/Wolfenshine) by
Dean Edis, vendored unmodified. Its raycaster and software renderer write into an RGBA framebuffer and have no idea
what a terminal is — no Avalonia, no SkiaSharp, no threads. Everything in this directory is the ~350 lines that put
that framebuffer on screen.

`HalfBlockSurface` carries **two independently coloured sub-pixels per character cell**, drawn as `▀` with the top
half in the foreground colour and the bottom in the background. At 200×50 cells that is 200×100 square pixels —
the exact 2:1 aspect of the original's 320×160 3D view, so the picture is a uniform 0.625× downscale of the real
thing rather than a reinterpretation of it. (The file is a copy of the 3D sandbox demo's; a second consumer is the
argument for promoting it into the library.)

`Wolf3DRenderer` casts one ray per sub-pixel *column*, hands the columns to Wolfenshine's own
`SoftwareRaycastRenderer`, and blits the result. The RGBA intermediate is deliberate: it keeps the vendored
renderer byte-for-byte the original, so what you see is the reference engine's output.

## Movement, without a key-up event

A terminal reports key *presses*, not key *state*. There is no key-up, so "is W held?" cannot be answered: holding a
key gives one press, a pause of a few hundred milliseconds, then the OS auto-repeat stream. Wolfenshine's own
`PlayerInput` is a struct of held booleans that a desktop toolkit fills in trivially and a terminal simply cannot.

Each press therefore opens a sustain window, and the frame clock rather than the key integrates movement while it is
open. Three details separate that from feeling like input lag:

**The window belongs to an axis, not a key.** Forward/back, strafe and turn are one axis each, carrying a single
direction, so the opposite key *reverses* it on the next frame. The first version gave every key its own window and
summed them — which meant pressing `a` while `d`'s window was still open bought exactly **zero** turn until it
lapsed. It read as heavy lag, and it was really a dead zone.

**The window is measured, not guessed.** Presses closer together than 250 ms are auto-repeat, so their spacing *is*
the repeat interval; the window becomes a small multiple of it. That is far tighter than any fixed value — release
stops the player in about a repeat and a half instead of a fixed quarter-second.

**Speed decays instead of stopping until the first repeat arrives.** The OS initial repeat delay is longer than any
window short enough to make a tap feel like a tap, so a fixed window *always* stalls once at the start of a hold.
Coasting turns that unavoidable gap into a slight slow-down that the first repeat cancels. Once repeats are flowing
the key is known to be held, so release goes back to being crisp.

What is left is a tap travelling about 1.3 tiles — further than the key was actually down for. That is the
irreducible cost of never being told when it came up, and it is the trade the Input tab exposes: **Coast** and
**First press** buy a tap's precision against how much a hold sags before its first repeat. None of these can be
shipped as a constant, because the repeat rate and initial delay behind them are per-OS, per-keyboard and
user-adjustable — which is why they are knobs rather than numbers in the source.

## What it cost, and what that taught us

Median of 90 frames of the assembled app — border, footer and all — through the real `ConsoleManager` at 200×52:

| motion | quantise | ANSI B/frame | runs | frame total |
|---|---|---|---|---|
| standing still | — | **8 B** | — | 1.9 ms |
| walking | none | 86,797 B | 3,372 | 4.7 ms |
| walking | 6/channel | **46,000 B** | 1,828 | 2.8 ms |
| turning | none | 82,735 B | 3,331 | 2.7 ms |
| turning | 6/channel | **47,300 B** | 1,943 | 2.3 ms |

At 30 fps a moving frame is about **1.4 MB/s** quantised, 2.6 MB/s not. Compute is never the constraint: the
raycast is ~0.5 ms and the emit is most of the rest.

**The cost is run count, not palette size.** The obvious guess is that a 256-colour game overwhelms the emitter
with colours. It does not — authentic Wolfenstein does no distance shading at all, so a whole frame is only
13–26 distinct colours. What costs is how finely those few colours are *interleaved*: every texel boundary breaks
a colour run and forces another SGR. Across every configuration measured, bytes land at a near-constant 25–34 per
run, which is why `LastRuns` rather than `LastColors` is what the footer reports.

That is also why snapping colours to a coarse ramp cuts the bill — it merges neighbouring texels back into runs.
Press `1` to compare, or use Display ▸ Quantize.

**The level is measured, not guessed.** Against a 2×12 sub-pixel ground truth over six levels and four viewpoints:

| levels | colour error vs full palette | bytes vs full |
|---|---|---|
| 4 | 296% | 45% |
| 6 | 155% | 55% |
| 8 | 152% | 63% |
| **10** *(default)* | **113%** | **68%** |
| 12 | 122% | 72% |
| 16 | 113% | 77% |

Ten levels lands within 13% of the full palette while still saving a third of the bandwidth, and is where the
banding on a gradient stops being obvious. Note the sweep is **non-monotonic** — 10 beats 12 — because the source
art is itself on a lattice (the original VGA palette expands six-bit channels), so an even RGB grid lands well or
badly depending on alignment. Pick this empirically; do not reason it upward.

**Quadrant sampling is nearly free in bandwidth**, unlike in the 3D sandbox where it costs 42% more bytes. It adds
about 9% (44.9 → 48.7 KB) because a picture already fragmented by texture detail gains few new run breaks from
extra horizontal resolution. Press `2`, or pick it from Display ▸ Surface.

It is not free in *fill*, though, and the reason is a trap worth knowing. A quadrant sub-pixel is half a cell wide
but a whole half-cell tall — **twice as tall as it is wide** — so anything assuming a square pixel breaks. The
vendored sprite projector does assume one: it derives a single `RenderedSize` and applies it to both axes, which
rendered every sprite and the weapon at *half* their proper width, while the walls (drawn per column from ray
angles) stayed correct and hid the cause. The fix is to render the framebuffer at double height in that mode and
sample rows back on the way out, restoring square pixels for the vendored code rather than patching its maths.
That roughly doubles the scene time, to about 1.1 ms of a 33 ms budget.

But note what it does *not* buy. **Every character cell gets exactly two colours — one foreground, one background
— whatever glyph is used.** Half blocks are therefore *exact*: two samples, two colours, no quantisation error at
all. Quadrants take four samples and must squeeze them back into the same two, so they buy placement accuracy with
colour accuracy. A finer sub-cell grid is a trade, not a quality dial, and which way it goes depends on the
content: flat-shaded geometry has little colour per cell to lose, dense texture has little placement detail to
gain. Sextants (2×3) and octants (2×4) extend the grid further but never lift the two-colour ceiling; that ceiling
is the whole difference between this and a pixel protocol such as Sixel, which addresses a hundred-odd real device
pixels per cell, each independently coloured.

**The authentic 66° field of view is also the cheaper one** — the derived ~90° view that fills a wide terminal
costs about 15–20% more bytes for geometry the level was never composed for. Press `3`.

## What is deliberately not here

The enhanced renderer (`F2` in Wolfenshine) is an SKSL GPU shader and cannot follow us into a terminal. Actors,
pickups and level progression all exist in the vendored `GameSession` and are simply not driven — a static
walkthrough was the goal.

Doors **do** open: the vendored engine already has the whole state machine (`Operate`, the slide animation, the
locks), so `space` drives it and the raycaster was already drawing a partly-retracted panel correctly. One
deliberate fiction — the player is assumed to hold every key, because a walkthrough has no pickups and an
authentic empty key ring would wall off parts of most levels, which reads as a broken demo rather than as
fidelity. Two things that bite if you try this yourself: a door tile reads as **solid** in map plane zero, so
collision has to ask about doors *before* testing walls or an open doorway stays an invisible wall; and the
animation needs ticking every frame or the panel never moves.

`f` fires. There is nothing to shoot in a static scene, so it animates the pistol and no more — the weapon sprite
is real, out of VSWAP's weapon pages, and the Display tab can turn it off.
