# Running the Jumbee.Console examples with Docker

Run the interactive examples browser with nothing installed on your machine but Docker.

## Quick start — run the published image

No clone, no build. Docker pulls the image ([`allisterb/jumbee-console`](https://hub.docker.com/r/allisterb/jumbee-console) on Docker Hub) on first run:

```sh
docker run --rm -it allisterb/jumbee-console
```

One image bundles four apps. The first argument picks which; with none, the examples browser runs:

| Argument | App |
| --- | --- |
| *(none)* | Interactive examples browser |
| `agent-harness` | Claude-desktop-style agent UI (session rail, transcript, live task list) |
| `ide` | VS Code–style IDE demo — edit and `dotnet build`/`run` a sample project in its terminal pane |
| `audio-scope` | Real-time oscilloscope, vectorscope and spectroscope over a bundled audio track |

```sh
docker run --rm -it allisterb/jumbee-console audio-scope
```

The examples are a full-screen TUI (mouse, alternate screen, raw key input), so the container **must** be given an
interactive terminal — the `-i` (keep stdin open) and `-t` (allocate a TTY) flags are required.

- Navigate with the arrow keys / mouse; **Ctrl+Q** quits.
- On exit the app restores your terminal (it renders on the alternate screen buffer).
- `--rm` removes the container when you quit.
- If colours or box-drawing look off, forward your terminal type: `docker run --rm -it -e TERM=$TERM allisterb/jumbee-console`.

> The published image is `linux/amd64` (native on Windows/WSL2 and Intel Linux/macOS; Apple Silicon runs it under emulation).

## Build it yourself

The [`Dockerfile`](Dockerfile) uses the full **.NET 10 SDK** image (Debian-slim) — not just the runtime — so the
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
