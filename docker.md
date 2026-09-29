# Running the Jumbee.Console examples with Docker

Run the interactive examples browser with nothing installed on your machine but Docker.

## Quick start — run the published image

No clone, no build. Docker pulls the image ([`allisterb/jumbee-console`](https://hub.docker.com/r/allisterb/jumbee-console) on Docker Hub) on first run:

```sh
docker run --rm -it --pull=always allisterb/jumbee-console
```

The first argument picks the app; with none, the examples browser runs:

| Argument | App |
| --- | --- |
| *(none)* | Interactive examples browser |
| `agent-harness` | Claude-desktop-style agent UI (session rail, transcript, live task list) |
| `ide` | VS Code–style IDE demo — file explorer, multi-tab editor and a working terminal pane over a sample project (see below for building it) |
| `audio-scope` | Real-time oscilloscope, vectorscope and spectroscope over a bundled audio track |
| `3dsandbox` | Real-time 3D rigid-body sandbox over three terminal renderers; `3dsandbox view` opens its model viewer (`.obj`, `.stl`, `.ply`) |
| `wolf3d` | Wolfenstein 3D walkthrough — real maps and textures through a raycaster (needs game data, see below) |

```sh
docker run --rm -it --pull=always allisterb/jumbee-console audio-scope
```

**Every app is a native binary**, compiled with NativeAOT: there is no .NET runtime in the image, so it is small
(about 490 MB, most of it the bundled 3D models and audio track) and every app starts at once. **It is multi-arch**,
`linux/amd64` and `linux/arm64`, and Docker pulls whichever matches your machine — so it runs natively on Apple
Silicon and ARM Linux as well as on Intel/AMD. Built from [`Dockerfile.aot`](Dockerfile.aot).

> **One image, two names.** `allisterb/jumbee-console-aot` is the same image under another name — the name it was
> first published under while it and a larger JIT-based image were both offered. Either works; they are identical.

The examples are a full-screen TUI (mouse, alternate screen, raw key input), so the container **must** be given an
interactive terminal — the `-i` (keep stdin open) and `-t` (allocate a TTY) flags are required.

- Navigate with the arrow keys / mouse; **Ctrl+Q** quits.
- On exit the app restores your terminal (it renders on the alternate screen buffer).
- `--rm` removes the container when you quit.
- If colours or box-drawing look off, forward your terminal type: `docker run --rm -it -e TERM=$TERM allisterb/jumbee-console`.

### Building the IDE demo's sample project

The `ide` demo opens a small C# project with a real shell in its terminal pane. Building it needs the .NET SDK, which
the image deliberately leaves out — it would more than double the image for one menu. So the first time you choose
**Build** (Ctrl+B), **Run** or **Clean**, the IDE offers to install it, and runs the install in its own terminal pane:

```sh
apt-get update && apt-get install -y dotnet-sdk-10.0
```

That is about 150 MB from Ubuntu's own archive and takes under a minute; choose Build again when it finishes. The
install lasts only as long as the container — with `--rm` it is gone when you quit. To keep it, bake it into a
derived image:

```dockerfile
FROM allisterb/jumbee-console
RUN apt-get update && apt-get install -y --no-install-recommends dotnet-sdk-10.0 && rm -rf /var/lib/apt/lists/*
```

Every app also takes **`--verify`**: a headless smoke check that renders its layout offscreen, prints one line and
exits. That is the way to test the image where there is no terminal to look at — a build, or CI. Without a TTY the
apps otherwise paint escape codes at a pipe until something times out.

```sh
docker run --rm jumbee-console 3dsandbox --verify
# PASS  3DSandbox verify — sandbox (shaded 5110, wireframe 2682, solid 5110), viewer (shaded 5025, ...).
```

`build-docker.sh` / `build-docker.cmd` run exactly that after building the image, over every app it ships and on
both architectures, and fail the build if any does not pass. Pass `--no-verify` to skip it.

**`wolf3d --verify` is the one that cannot pass inside an image.** The id Software assets are not redistributable,
so `.dockerignore` keeps them out of the build context and no image carries game data — the app exits with a message
saying where to put the files. The build scripts check that much instead: that `wolf3d` starts and reports its data
missing, which only a binary that actually runs can do. Verify it properly against a mounted data directory:

```sh
docker run --rm -v /path/to/WL1:/data jumbee-console wolf3d /data --verify
```

### Getting the current image

`docker run` defaults to `--pull=missing`: if *any* copy of the image is already on the machine it runs that one and
never contacts the registry. Since `latest` is a moving tag, a machine that ran an earlier release keeps running it
indefinitely — which is why the commands above pass **`--pull=always`**. It is cheap when you are already current:
Docker re-resolves the tag to a manifest digest and re-downloads only layers that actually changed, so an up-to-date
machine pays one small request rather than the full image.

To pin a specific release instead, name its version tag — no staleness question, and `--pull=always` becomes
unnecessary:

```sh
docker run --rm -it allisterb/jumbee-console:0.2.1 audio-scope
```

(Not to be confused with `docker build --pull` further down, which refreshes the *base* image during a build.)

## Build it yourself

```sh
# Make sure the vendored submodules are present first:
git submodule update --init --recursive

# The image, for both architectures, with every app verified on each:
./build-docker.sh              # build-docker.cmd on Windows
./build-docker.sh --no-arm64   # quicker: amd64 only

# Or directly:
docker build --pull --platform linux/amd64,linux/arm64 -f Dockerfile.aot -t jumbee-console .
```

[`Dockerfile.aot`](Dockerfile.aot) is a two-stage build: the **.NET 10 SDK** plus the clang toolchain NativeAOT links
with compiles each app to a native binary, and the runtime stage is `runtime-deps` — the OS libraries a native binary
needs, with no .NET runtime at all. The runtime stage copies the binaries plus the two directories the demos read
from disk (`media/`, the sandbox's `models/`) into `/app`; the sources, the `ext/` submodules and the intermediates
all stay behind in the build stage. `--pull` refreshes the base images and re-applies the latest OS security patches
(see below).

**The arm64 half is cross-compiled, not emulated.** The build stage always runs on the build machine's own platform
and, for arm64, installs Ubuntu's aarch64 cross toolchain and publishes with `-r linux-arm64`; only the small runtime
stage runs as arm64, under QEMU. Emulating the whole build instead would run the SDK and the NativeAOT compiler under
QEMU, which is many times slower. `build-docker` then verifies the arm64 image too, by running it under QEMU
(`docker run --platform linux/arm64 …`), which executes the real arm64 binaries. Two requirements:

- **An amd64 build machine.** Cross-compiling is set up for amd64 → arm64 only; building on an arm64 host is not.
- **Docker's containerd image store**, so one local tag can hold both architectures. It is the default in current
  Docker Desktop; check with `docker info` (look for `driver-type: io.containerd.snapshotter.v1`).

> Until 0.2.1, `allisterb/jumbee-console` was a different, JIT-based image, running the demos on the .NET 10 runtime.
> The NativeAOT image replaced it under that name, and its Dockerfile was removed.

### The AudioScope demo's sample track

`audio-scope` plays a bundled MP3 when given no `--path`, and that file is **not in the repository** — it is
third-party music, so `media/` is gitignored. Everything else builds without it; only `audio-scope`'s zero-argument
default needs it, so a media-less build still produces a working image (you just pass `--path yours.mp3`, or
`audio-scope --input live` to capture a device).

To get the default track, download `06_arido_III_the_oscilloscope_rmx.mp3` from
[M028 — Of. *Áridos* on archive.org](https://archive.org/details/M028_Of_Aridos) into `media/` before building:

```sh
mkdir -p media && curl -L -o media/06_arido_III_the_oscilloscope_rmx.mp3 \
  https://archive.org/download/M028_Of_Aridos/06_arido_III_the_oscilloscope_rmx.mp3
```

The build prints a warning when the file is absent, rather than letting it surface as a runtime error inside the demo.

> **Attribution.** The track is *Of.* — “Árido III (The Oscilloscope remix)”, from **M028 — Of. *Áridos*** on the
> Chilean netlabel [Modismo](https://archive.org/details/M028_Of_Aridos) (2018); music by Christian González,
> mastered by Daniel Nieto. It is licensed **Creative Commons Attribution-NonCommercial** — the release notes bundled
> with the album say BY-NC 3.0 while archive.org lists BY-NC-ND 4.0. The image redistributes it **unmodified**, which
> both permit, and ships the album's credits file next to it. The NonCommercial term means this demo image must not
> be used commercially with the track in place; the ND term means don't ship a trimmed or remixed excerpt.


### The 3D sandbox's models

Same arrangement, same reason. `3dsandbox` looks for a `models` folder and loads every model in it (`.obj`, `.stl`
and `.ply`) — the sandbox makes them spawnable, and `3dsandbox view` opens the viewer on the first. That folder lives at
`examples/Jumbee.Console.3DSandboxDemo/models` and is **not in the repository**: the meshes are third-party research
assets (the Stanford bunny and dragon, the Utah teapot, and friends), each with its own terms.

Without it the image still builds, and the sandbox falls back to a torus knot it generates itself — which is a
working app, not a broken one, and is what the demo ships with by design. Drop model files into that folder to
bundle them; the build copies the whole folder, recursively, to `/app/models` in the image.

> If you bundle models in an image you publish, ship their licences too — most research meshes carry attribution or
> non-commercial terms, and `THIRD-PARTY-NOTICES.TXT` is where this repo records that kind of thing. The whole folder
> is copied, so a licence file kept alongside the models ships with them.

### The Wolf3D demo's game data

`wolf3d` reads the original 1992 game's own files. **They are not redistributable and are deliberately excluded
from the build context** ([`.dockerignore`](.dockerignore)), so no published image contains them — note that
Docker ignores `.gitignore` entirely, which is why the exclusion has to be stated in both places.

Without them the demo prints what it needs and exits. Mount a directory holding the eight free shareware `.WL1`
files (or the full game's `.WL6` files) over the demo's `GameData` folder to play:

```sh
docker run --rm -it -v /path/to/wolf3d-data:/app/wolf3d/GameData allisterb/jumbee-console wolf3d
```

Or mount it anywhere and pass the path as the first argument:
`docker run --rm -it -v /path/to/wolf3d-data:/data allisterb/jumbee-console wolf3d /data`.

See [`examples/Jumbee.Console.Wolf3DDemo/GameData/README.md`](examples/Jumbee.Console.Wolf3DDemo/GameData/README.md)
for which files are needed and where the shareware archive comes from.

### Scoping a live audio device (Linux hosts)

`audio-scope` can capture a real input instead of the file. The image ships the ALSA runtime, so all that is needed
is passing the host's sound devices in:

```sh
docker run --rm -it --device /dev/snd allisterb/jumbee-console audio-scope live
```

List what `--device` can select first (this also works without `/dev/snd`, it just finds nothing but ALSA's `null`):

```sh
docker run --rm -it --device /dev/snd allisterb/jumbee-console audio-scope --list-devices
```

A few caveats worth knowing:

- **Linux hosts only.** Docker Desktop on Windows and macOS runs containers in a VM with no audio hardware attached —
  `/dev/snd` does not exist there, so it cannot be passed through and there is no supported workaround. Run the demo
  natively instead; on Windows that also gets you WASAPI `--loopback` (scope what is *playing*).
- **Non-root images** additionally need `--group-add audio`. This image runs as root, which can open `/dev/snd`
  directly.
- **On a Linux desktop, PipeWire or PulseAudio usually already holds the capture device**, so raw ALSA may come back
  busy. Headless servers are the easy case. To route through the host's sound server instead, share its socket and
  add the ALSA `pulse` plugin — deliberately *not* preinstalled, because `libasound2-plugins` pulls in ffmpeg and
  librsvg for a path most users never take:

  ```sh
  docker run --rm -it -v /run/user/$(id -u)/pulse/native:/tmp/pulse -e PULSE_SERVER=unix:/tmp/pulse allisterb/jumbee-console audio-scope live --device pulse:DEVICE=your-source
  ```

  Add `RUN apt-get update && apt-get install -y libasound2-plugins` to a derived image to get the plugin. `--loopback`
  on Linux means pointing at a sink's `.monitor` source rather than a WASAPI loopback endpoint.

The runtime stage patches its base OS packages (`apt-get upgrade`) before installing ALSA. Shipping only the OS
libraries a native binary needs — no SDK, and no .NET runtime — is what keeps Docker Scout's findings down: those were
overwhelmingly medium-severity issues in build tools (`git`, `wget`, `tar`, …) that the running TUI never touched but
that shipped anyway. Rebuilding periodically with **`--pull`** keeps both the base image and the patch layer current.

## Publishing (maintainers)

The image is published from a maintainer's machine. `build-docker` builds and verifies it, tagged with the version in
`src/Directory.Build.props` and `latest`, under **both** names — `jumbee-console` and `jumbee-console-aot` — which
are one image, not two builds. Each name is then tagged into the `allisterb/` namespace and pushed. Pushing the same
image under a second repository re-uploads nothing: Docker Hub links the layers the first push already stored.

Pushing from the containerd store sends both architectures under each tag; afterwards
`docker buildx imagetools inspect allisterb/jumbee-console:<version>` should list `linux/amd64` and `linux/arm64`, and
show the same digest as `allisterb/jumbee-console-aot:<version>`. Don't publish a `--no-arm64` build: it would replace
the multi-arch tags with amd64-only ones.

> Publishing from a working tree is also what gets the untracked assets into the image: `media/` (the AudioScope
> track) and the sandbox's `models/` exist only there, so an image built from a fresh clone has neither. A GitHub
> Actions workflow published multi-arch images for a while (July 2026) and was removed; anything that brings CI
> publishing back has to fetch the MP3 (see the `curl` above) as a build step, or `audio-scope` ships without its
> default input.
