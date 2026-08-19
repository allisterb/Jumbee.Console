# Vendored Wolfenshine engine — do not edit

Unmodified copy of the presentation-free layer of [Wolfenshine](https://github.com/deanthecoder/Wolfenshine)
by Dean Edis, taken from `reference/projects/Wolfenshine-main/Wolfenshine`. Directory structure mirrors upstream
so a refresh is a copy and a diff.

| | |
|---|---|
| `Resources/` | locating the original game files, VSWAP page decoding, the embedded VGA palette |
| `Maps/` | RLEW/Carmack decompression, the two tile planes |
| `Graphics/` | indexed wall pages, sprites, HUD pictures, palette lookup |
| `Rendering/` | the raycaster, the camera, the software framebuffer renderer, sprite projection |
| `Game/` | doors, pushwalls, static scenery, the game session |
| `Audio/` | **two data types only** (`WolfensteinSoundEffect`, `WolfensteinSoundEvent`) |

## Why the whole layer, when the walkthrough uses a third of it

Cherry-picking is a re-derivation every time the demo grows a feature, and it does not fail loudly — you find out
which file you forgot when the build breaks, or worse, when a subtly different reimplementation renders. Copying the
layer whole means `GameSession` (doors opening, pushwalls, elevators, collision, level progression) is already here
when it is wanted, and refreshing from upstream stays a directory diff.

It is self-contained: no Avalonia, no SkiaSharp, no threads, and the only cross-layer reference is
`GameSession` → the two `Audio/` **data** types, which it queues for a host to drain. The audio *engine*
(`WolfensteinAudioPlayer`, the sound and music loaders, and their OpenAL and OPL package references) is
deliberately not vendored.

## Rules

- **Do not edit these files.** The one addition is `Logger.cs`, a stand-in for the `DTC.Core` logger the upstream
  files call into, which keeps every other file byte-identical to upstream.
- Everything Jumbee-specific lives in the demo project one directory up.
- Upstream's per-file copyright headers are intact, which is what its licence asks. See
  `THIRD-PARTY-NOTICES.TXT` at the repo root.

Scanned at the codepoint level before vendoring (2026-08-19): no bidi overrides, zero-width characters, Unicode
Tag block, soft hyphens, private-use characters, or injection phrasing; the only non-ASCII is `·`, `×`, `–`, `—`
in comments. No MSBuild/editorconfig files, no `ModuleInitializer`, `DllImport`, `Process.Start`, `Assembly.Load`,
`Marshal`, or `unsafe`.
