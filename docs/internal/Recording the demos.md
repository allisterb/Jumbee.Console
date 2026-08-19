# Recording the demos

How to capture the 3D sandbox (or any of the TUI demos) for a README, a post, or a release note — and *why* the
settings below are the ones to use. Everything here was measured on real output from
[`3D Rendering in a Terminal.md`](3D%20Rendering%20in%20a%20Terminal.md)'s renderers, not taken from general advice
about screen recording.

## The one thing that decides everything else

**The renderer emits a few hundred flat colour plateaus, not gradients.** Brightness is quantised to
`MeshRenderer.ShadeLevels` steps (7 by default) and the occlusion pass then multiplies by one of nine discrete
factors — an eight-sample ring, so `occluded` is 0–8. On-screen colour is therefore the product
hue × 8 × 9, and measured frames agree:

| renderer | distinct colours, viewport | whole frame |
|---|---|---|
| shaded | 719 | 1332 |
| solid | 234 | 908 |
| wireframe | 317 | 1000 |

Flat regions with hard edges between them. That is the *best* case for lossless codecs and the *worst* case for
lossy ones, and it is why the recommendations below run opposite to the usual "quality 100 looks best" instinct.

## Use WebP, not GIF

GIF caps at 256 colours per frame, and the whole point of the demo is a true-colour renderer. Measured on one real
1276×808 shaded frame:

| encoder settings | size | fidelity |
|---|---|---|
| `-lossless 0 -quality 100` | 55,588 B | PSNR 41.4 dB, pixels differ |
| `-lossless 0 -quality 90 -preset text` | 34,760 B | PSNR 40.9 dB, pixels differ |
| **`-lossless 1`** | **20,362 B** | **PSNR ∞ — bit-identical** |

Lossless is **2.7× smaller than lossy quality 100 and mathematically perfect**. Lossy WebP subsamples chroma to
4:2:0 whatever the quality, which puts colour fringing on glyph edges; lossless uses palette and predictive
transforms, which is exactly right for flat plateaus. In practice the finished animation came out about **a third
the size of the equivalent GIF**, and Reddit displays it fine.

### The ffmpeg line

ScreenToGif's custom-parameter field, where `{I}` / `{O}` are its input/output placeholders:

```
-vsync passthrough {I} -c:v libwebp_anim -lossless 1 -quality 100 -pix_fmt bgra -loop 0 -f webp {O}
```

- **`-lossless 1`** is the setting that matters. Everything else is defaults or insurance.
- **`-pix_fmt bgra`** made no measured difference — ffmpeg already negotiates it — but pin it anyway: a build that
  picked `yuv420p` would *losslessly* encode an already chroma-damaged image, silently.
- **`-quality`** is a compression-*effort* knob in lossless mode, not a fidelity one. Measured 0/50/75/100 across a
  20,344–20,832 byte spread, about 2%. Irrelevant; leave it.
- **`-vsync passthrough`** preserves ScreenToGif's per-frame delays. Deprecated in newer ffmpeg in favour of
  `-fps_mode passthrough`.

**Not measured:** `libwebp_anim` also exposes `-cr_threshold` / `-cr_size` (conditional replenishment — WebP's
answer to GIF delta frames). Default threshold is 0. Worth an A/B on a real clip; same caution as GIF's delta
tolerance, in that it can freeze blocks that only *nearly* match.

## If a GIF is genuinely required

For a target that will not take WebP. Settings are ScreenToGif's KGy SOFT encoder:

| setting | value | why |
|---|---|---|
| **Ditherer** | **None** | The single most important one. Dithering scatters noise across regions that are currently perfectly flat, inflating the file *and* defeating delta frames. |
| Palette size | 256 | Content is ~700 colours; do not starve the quantizer. |
| Quantizer | Median Cut, per frame | Fine. Worth an A/B against Wu or Octree if offered — Median Cut can spend entries on rare colours. |
| Allow delta frames | on | Essential. |
| Allow clipped frames | on | Essential. |
| Delta tolerance | 0 | Output is deterministic, so static regions are bit-identical. Raise only if the file is too big, and watch the shade plateaus for smearing. |
| Endless loop | on | |
| Back and forth | off | Doubles the frame count. |
| Alpha threshold, background colour | ignore | Source is fully opaque. |
| Linear colour space | off | Helps gradient quantization; there are no gradients. |

**Possible interaction, untested:** "optimize palette for each frame" may fight delta frames — if every frame gets
its own palette, an unchanged pixel can land on a different index and stop looking unchanged. Encode once per-frame
and once with a global palette and compare.

## Two levers that outweigh every encoder setting

1. **Keep the camera still.** Delta frames and conditional replenishment only pay when most of the frame is
   identical tick to tick. Static camera with bodies falling: perhaps 5–10% of pixels change per frame. Orbiting
   camera: **100% changes, every frame**, and both features buy nothing. This is decided before the encoder is
   opened.
2. **Shrink the terminal; do not downscale the capture.** The half-block sub-pixels are the whole trick and they do
   not survive resampling — downscaling turns every glyph edge to mush. Resizing the terminal window to fewer rows
   and columns before recording keeps each glyph pixel-exact at a fraction of the area. Dropping to 15–20 fps is the
   other free win; settling bodies do not need 30.

## Also worth knowing

- **Sleep dimming was removed** partly for recordings: a settled pile is the common case, so most of the scene sat
  at a third brightness most of the time. The awake count is still in the footer, and the `step` readout visibly
  drops when the solver stops working — a better thing to point at on video than a colour shift.
- **`ShadeLevels` is a runtime slider** and the constants behind it were chosen for *interactive* ANSI cost. A
  capture has nothing waiting on the terminal, so a finer ramp is free there and expensive live. Raise it for a
  recording.

## Caveats on the numbers above

- The source frames were rendered by `ConsoleSnapshot.SavePng` (the library's own font renderer), **not** a
  ScreenToGif screen capture. If the real capture uses ClearType subpixel antialiasing, its frames carry coloured
  fringes that inflate the colour count and narrow lossless's advantage. Re-running the two-line comparison on one
  genuinely captured frame settles it in about thirty seconds.
- Single-frame sizes do not extrapolate: 20 KB × N frames is a pessimistic ceiling, since inter-frame prediction
  does much better. Encode the real clip before concluding anything about size.
