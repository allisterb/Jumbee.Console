# syntax=docker/dockerfile:1

# ─────────────────────────────────────────────────────────────────────────────
# Jumbee.Console demos — run any of the interactive TUIs with nothing installed but
# Docker. One image bundles six apps (examples browser, agent-harness demo, IDE
# demo, AudioScope demo, 3D sandbox, Wolfenstein 3D walkthrough); the examples.sh
# entry point picks which via the first `docker run` arg.
#
# Two stages: the SDK builds, and only the .NET RUNTIME ships. The image used to be
# a single SDK stage so the IDE demo's terminal pane could `dotnet build` its sample
# project in-container — a nice trick that cost several hundred megabytes and pulled
# in the whole build-tool CVE surface Docker Scout flags. Demonstrating "edit and run
# code in the terminal pane" does not need a 900 MB toolchain; a much smaller one can
# be wired up later. Everything else the demos do is unaffected.
# ─────────────────────────────────────────────────────────────────────────────

# ── build ────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1

WORKDIR /src

# Copy the repo (see .dockerignore) and build every app in Release. This restores each project reference (ext/*,
# src/*) and NuGet package (Mermaider, AdocNet, Sugiyama, …) from nuget.org. NOTE: the ext/* git submodules must be
# initialised on the host first (`git submodule update --init --recursive`).
#
# The VS Code–style IDE demo opens a bundled sample C# project you can browse and edit; the agent-harness demo is a
# Claude-desktop-style agent UI (session rail, chat transcript, live task list, document pane); the AudioScope demo
# is a real-time three-pane oscilloscope/spectroscope/vectorscope; the 3D sandbox is a rigid-body playground and OBJ
# model viewer over three terminal renderers; and the Wolf3D demo walks the original game's levels through a
# raycaster on a half-block pixel surface.
COPY . .
RUN dotnet build examples/Jumbee.Console.Examples/Jumbee.Console.Examples.csproj -c Release
RUN dotnet build examples/Jumbee.Console.IdeDemo/Jumbee.Console.IdeDemo.csproj -c Release
RUN dotnet build examples/Jumbee.Console.AgentHarnessDemo/Jumbee.Console.AgentHarnessDemo.csproj -c Release
RUN dotnet build examples/Jumbee.Console.AudioScopeDemo/Jumbee.Console.AudioScopeDemo.csproj -c Release
RUN dotnet build examples/Jumbee.Console.3DSandboxDemo/Jumbee.Console.3DSandboxDemo.csproj -c Release
RUN dotnet build examples/Jumbee.Console.Wolf3DDemo/Jumbee.Console.Wolf3DDemo.csproj -c Release

# Two asset directories are OPTIONAL and untracked (see docker.md), and the runtime stage copies both. A missing
# COPY source is a hard build failure, so create them unconditionally here — that keeps a checkout without them
# building a working image, just one with a reduced demo, which is what the warnings below say.
RUN mkdir -p media examples/Jumbee.Console.3DSandboxDemo/models \
    && if [ -f media/06_arido_III_the_oscilloscope_rmx.mp3 ]; then \
         echo "AudioScope: bundled sample track found."; \
       else \
         echo "AudioScope WARNING: media/06_arido_III_the_oscilloscope_rmx.mp3 is missing from the build context;" \
              "'audio-scope' will need an explicit --path. See docker.md for the download link."; \
       fi \
    && if [ -n "$(ls examples/Jumbee.Console.3DSandboxDemo/models/*.obj 2>/dev/null)" ]; then \
         echo "3D sandbox: $(ls examples/Jumbee.Console.3DSandboxDemo/models/*.obj | wc -l) model(s) found."; \
       else \
         echo "3D sandbox NOTE: examples/Jumbee.Console.3DSandboxDemo/models is missing from the build context;" \
              "'3dsandbox' will show only its generated torus knot."; \
       fi

# ── runtime ──────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/runtime:10.0

# UTF-8 everywhere (box-drawing glyphs) and a colour-capable TERM for the TUI.
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    LANG=C.UTF-8 \
    LC_ALL=C.UTF-8 \
    TERM=xterm-256color

# Patch the base OS packages to the latest available at build time. Rebuild with `docker build --pull` to also
# refresh the base image itself and re-fetch fresh patches (a plain rebuild reuses this cached layer). Cleaned up in
# the same layer to stay small.
#
# libasound2 is the ALSA runtime the AudioScope demo needs for `audio-scope live` (device capture) on a Linux host —
# see the live-capture section in docker.md. Two non-obvious details:
#   * the package is `libasound2t64` on Ubuntu noble / Debian trixie (the 64-bit time_t transition) and `libasound2`
#     on older Debian, so try both rather than pinning the wrong one;
#   * NAudio.Alsa's P/Invoke is `DllImport("libasound")`, and .NET's probe wants an UNVERSIONED `libasound.so`. The
#     runtime package ships only `libasound.so.2`; the bare symlink normally lives in `libasound2-dev`. Rather than
#     pull the dev package (headers we never compile against), resolve the real path from ldconfig — which keeps this
#     arch-agnostic for the arm64 leg of the multi-arch build — and link it ourselves.
# NOT installed: `libasound2-plugins`. It provides the ALSA `pulse` plugin (needed only to route through a host
# PulseAudio/PipeWire socket) but drags in ffmpeg and librsvg — a large, CVE-heavy addition for an optional path.
# docker.md documents the one-line opt-in.
RUN apt-get update \
    && apt-get -y upgrade \
    && (apt-get install -y --no-install-recommends libasound2t64 \
        || apt-get install -y --no-install-recommends libasound2) \
    && ln -sf "$(ldconfig -p | awk '/libasound\.so\.2 /{print $NF; exit}')" /usr/lib/libasound.so \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /src

# Only what actually runs. The build tree's sources, ext/ submodules, docs and obj/ intermediates stay in the build
# stage: the examples browser shows its side-by-side source from EMBEDDED RESOURCES, and the IDE demo copies its
# sample project out of its own output directory, so nothing here reads the repo at runtime. The two paths that ARE
# read from disk are media/ (AudioScope's default track) and the sandbox's models/, both below.
COPY --from=build /src/examples.sh ./examples.sh
COPY --from=build /src/media ./media
COPY --from=build /src/examples/Jumbee.Console.3DSandboxDemo/models ./examples/Jumbee.Console.3DSandboxDemo/models
COPY --from=build /src/examples/Jumbee.Console.Examples/bin/Release/net10.0 ./examples/Jumbee.Console.Examples/bin/Release/net10.0
COPY --from=build /src/examples/Jumbee.Console.IdeDemo/bin/Release/net10.0 ./examples/Jumbee.Console.IdeDemo/bin/Release/net10.0
COPY --from=build /src/examples/Jumbee.Console.AgentHarnessDemo/bin/Release/net10.0 ./examples/Jumbee.Console.AgentHarnessDemo/bin/Release/net10.0
COPY --from=build /src/examples/Jumbee.Console.AudioScopeDemo/bin/Release/net10.0 ./examples/Jumbee.Console.AudioScopeDemo/bin/Release/net10.0
COPY --from=build /src/examples/Jumbee.Console.3DSandboxDemo/bin/Release/net10.0 ./examples/Jumbee.Console.3DSandboxDemo/bin/Release/net10.0
COPY --from=build /src/examples/Jumbee.Console.Wolf3DDemo/bin/Release/net10.0 ./examples/Jumbee.Console.Wolf3DDemo/bin/Release/net10.0

# The apps are INTERACTIVE full-screen TUIs (mouse, alternate screen, raw key input), so the container MUST be given a
# TTY. examples.sh is the single entry point; its first argument selects the app (no argument = the examples browser):
#     docker build -t jumbee-console .
#     docker run --rm -it jumbee-console                 # examples browser
#     docker run --rm -it jumbee-console agent-harness   # agent harness demo
#     docker run --rm -it jumbee-console ide             # IDE demo
#     docker run --rm -it jumbee-console audio-scope     # AudioScope demo (bundled sample track)
#     docker run --rm -it jumbee-console 3dsandbox       # 3D physics sandbox (3dsandbox obj = the model viewer)
#     docker run --rm -it jumbee-console wolf3d          # Wolfenstein 3D walkthrough (needs mounted game data)
# Quit any app with Ctrl+Q; it restores your terminal on exit.
#
# `wolf3d` reads the original game's own files, which are NOT redistributable and are deliberately kept out of the
# build context (.dockerignore). Without them it prints how to supply them; mount a folder holding the eight
# shareware .WL1 files over its GameData directory to play:
#     docker run --rm -it -v /path/to/wl1:/src/examples/Jumbee.Console.Wolf3DDemo/bin/Release/net10.0/GameData \
#       jumbee-console wolf3d
#
# The bundled track is "Of. — Árido III (The Oscilloscope remix)" from Modismo M028, under Creative Commons
# Attribution-NonCommercial (see docker.md). It is redistributed UNMODIFIED, and its credits and licence terms ship
# alongside it at /src/media/M028_Of_Arido.txt.
ENTRYPOINT ["bash", "/src/examples.sh"]
