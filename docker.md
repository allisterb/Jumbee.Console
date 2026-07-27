# Running the Jumbee.Console examples with Docker

Run the interactive examples browser with nothing installed on your machine but Docker.

## Quick start — run the published image

No clone, no build. Docker pulls the image ([`allisterb/jumbee-console`](https://hub.docker.com/r/allisterb/jumbee-console) on Docker Hub) on first run:

```sh
docker run --rm -it --pull=always allisterb/jumbee-console
```

One image bundles four apps. The first argument picks which; with none, the examples browser runs:

| Argument | App |
| --- | --- |
| *(none)* | Interactive examples browser |
| `agent-harness` | Claude-desktop-style agent UI (session rail, transcript, live task list) |
| `ide` | VS Code–style IDE demo — edit and `dotnet build`/`run` a sample project in its terminal pane |
| `audio-scope` | Real-time oscilloscope, vectorscope and spectroscope over a bundled audio track |

```sh
docker run --rm -it --pull=always allisterb/jumbee-console audio-scope
```

The examples are a full-screen TUI (mouse, alternate screen, raw key input), so the container **must** be given an
interactive terminal — the `-i` (keep stdin open) and `-t` (allocate a TTY) flags are required.

- Navigate with the arrow keys / mouse; **Ctrl+Q** quits.
- On exit the app restores your terminal (it renders on the alternate screen buffer).
- `--rm` removes the container when you quit.
- If colours or box-drawing look off, forward your terminal type: `docker run --rm -it -e TERM=$TERM allisterb/jumbee-console`.

### Getting the current image

`docker run` defaults to `--pull=missing`: if *any* copy of the image is already on the machine it runs that one and
never contacts the registry. Since `latest` is a moving tag, a machine that ran an earlier release keeps running it
indefinitely — which is why the commands above pass **`--pull=always`**. It is cheap when you are already current:
Docker re-resolves the tag to a manifest digest and re-downloads only layers that actually changed, so an up-to-date
machine pays one small request rather than the full image.

To pin a specific release instead, name its version tag — no staleness question, and `--pull=always` becomes
unnecessary:

```sh
docker run --rm -it allisterb/jumbee-console:0.1.6 audio-scope
```

(Not to be confused with `docker build --pull` further down, which refreshes the *base* image during a build.)

> The published image is `linux/amd64` (native on Windows/WSL2 and Intel Linux/macOS; Apple Silicon runs it under emulation).

## Build it yourself

The [`Dockerfile`](Dockerfile) uses the full **.NET 10 SDK** image (Ubuntu 24.04) — not just the runtime — so the
`dotnet` build tools stay available inside the container. It copies the repo and builds the examples project in
Release, baking the build into the image.

```sh
# Make sure the vendored submodules are present first:
git submodule update --init --recursive

# --pull refreshes the SDK base image and re-applies the latest OS security patches (see below).
docker build --pull -t jumbee-console .
docker run --rm -it jumbee-console
```

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

Both Dockerfiles print a warning at build time when the file is absent, rather than letting it surface as a runtime
error inside the demo.

> **Attribution.** The track is *Of.* — “Árido III (The Oscilloscope remix)”, from **M028 — Of. *Áridos*** on the
> Chilean netlabel [Modismo](https://archive.org/details/M028_Of_Aridos) (2018); music by Christian González,
> mastered by Daniel Nieto. It is licensed **Creative Commons Attribution-NonCommercial** — the release notes bundled
> with the album say BY-NC 3.0 while archive.org lists BY-NC-ND 4.0. The images redistribute it **unmodified**, which
> both permit, and ship the album's credits file next to it. The NonCommercial term means these demo images must not
> be used commercially with the track in place; the ND term means don't ship a trimmed or remixed excerpt.

### Scoping a live audio device (Linux hosts)

`audio-scope` can capture a real input instead of the file. Both images ship the ALSA runtime, so all that is needed
is passing the host's sound devices in:

```sh
docker run --rm -it --device /dev/snd allisterb/jumbee-console-aot audio-scope live
```

List what `--device` can select first (this also works without `/dev/snd`, it just finds nothing but ALSA's `null`):

```sh
docker run --rm -it --device /dev/snd allisterb/jumbee-console-aot audio-scope --list-devices
```

A few caveats worth knowing:

- **Linux hosts only.** Docker Desktop on Windows and macOS runs containers in a VM with no audio hardware attached —
  `/dev/snd` does not exist there, so it cannot be passed through and there is no supported workaround. Run the demo
  natively instead; on Windows that also gets you WASAPI `--loopback` (scope what is *playing*).
- **Non-root images** additionally need `--group-add audio`. These images run as root, which can open `/dev/snd`
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

The build patches the base OS packages (`apt-get upgrade`) to trim the CVEs Docker Scout reports in the SDK image's
build tools. Those are mostly medium-severity issues in tools (`git`, `wget`, `tar`, …) the running TUI never uses, so
the practical risk is low — but rebuilding periodically with **`--pull`** keeps both the base image and the patch layer
current.

> For a smaller image, change the base tag in the `Dockerfile` to `mcr.microsoft.com/dotnet/sdk:10.0-alpine`.
> The examples set `InvariantGlobalization=true`, so no ICU package is required on either base.

## Publishing (maintainers)

The published `linux/amd64` + `linux/arm64` image is built and pushed by GitHub Actions
([`.github/workflows/docker-publish.yml`](.github/workflows/docker-publish.yml)) — on a version tag `vX.Y.Z`
(tags the image `X.Y.Z`, `X.Y`, `latest`) or a manual **Run workflow** (tags `latest`). Each arch builds on its own
**native** runner (`ubuntu-24.04` + `ubuntu-24.04-arm`, no QEMU emulation) and the digests are merged into one
multi-arch manifest. The native arm64 runner is free only for **public** repositories.

One-time setup — add two repository secrets (Settings → Secrets and variables → Actions):

| Secret | Value |
| --- | --- |
| `DOCKERHUB_USERNAME` | your Docker Hub username (`allisterb`) |
| `DOCKERHUB_TOKEN` | a Docker Hub **access token** (Account Settings → Security → New Access Token, Read/Write) |

Then `git tag v0.1.0 && git push --tags`, or trigger the workflow by hand from the Actions tab.

> Because `media/` is untracked, a **CI** build has no AudioScope sample track — its checkout cannot contain one. Any
> workflow that publishes these images has to fetch the MP3 (see the `curl` above) as a build step, or `audio-scope`
> will ship without its default input. Publishing from a local `build-docker` + `publish-docker` run picks the file up
> from the working tree and is unaffected.
