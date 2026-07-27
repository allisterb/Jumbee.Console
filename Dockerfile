# syntax=docker/dockerfile:1

# ─────────────────────────────────────────────────────────────────────────────
# Jumbee.Console demos — run any of the interactive TUIs with nothing installed but
# Docker. One image bundles four apps (examples browser, agent-harness demo, IDE
# demo, AudioScope demo); the examples.sh entry point picks which via the first
# `docker run` arg.
# Uses the FULL .NET 10 SDK (not just the runtime) so the build tools stay available
# inside the image (some demos shell out to `dotnet`). Base is the Debian-slim SDK
# image; swap the tag for `10.0-alpine` for a smaller image (the examples set
# InvariantGlobalization=true, so no ICU is required either way).
# ─────────────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0

# UTF-8 everywhere (box-drawing glyphs) and a colour-capable TERM for the TUI.
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    LANG=C.UTF-8 \
    LC_ALL=C.UTF-8 \
    TERM=xterm-256color

# Patch the base OS packages to the latest available at build time — trims the (mostly medium, build-tool) CVEs
# Docker Scout flags in the SDK image. Rebuild with `docker build --pull` to also refresh the base image itself and
# re-fetch fresh patches (a plain rebuild reuses this cached layer). Cleaned up in the same layer to stay small.
RUN apt-get update \
    && apt-get -y upgrade \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /src

# Copy the repo (see .dockerignore) and build the examples browser in Release. This
# restores every project reference (ext/*, src/*) and NuGet package (Mermaider,
# AdocNet, Sugiyama, …) from nuget.org and bakes the build into the image, so the
# container starts straight into the app. NOTE: the ext/* git submodules must be
# initialised on the host first (`git submodule update --init --recursive`).
COPY . .
RUN dotnet build examples/Jumbee.Console.Examples/Jumbee.Console.Examples.csproj -c Release

# Also build the three standalone demos so one image can run any of the four apps (the entry-point script below picks
# which). The VS Code–style IDE demo opens a bundled sample C# project you can edit and `dotnet build`/`dotnet run`
# right in its terminal pane (the SDK above makes that work in-container); the agent-harness demo is a Claude-desktop-
# style agent UI (session rail, chat transcript, live task list, document pane). The AudioScope demo is a real-time three-pane audio oscilloscope/spectroscope/vectorscope.
RUN dotnet build examples/Jumbee.Console.IdeDemo/Jumbee.Console.IdeDemo.csproj -c Release
RUN dotnet build examples/Jumbee.Console.AgentHarnessDemo/Jumbee.Console.AgentHarnessDemo.csproj -c Release
RUN dotnet build examples/Jumbee.Console.AudioScopeDemo/Jumbee.Console.AudioScopeDemo.csproj -c Release

# The AudioScope demo defaults to the bundled sample track at media/ when given no --path, and examples.sh cds to
# /src, so the COPY above is what puts it where the demo looks. media/ is NOT tracked in git (see docker.md) — a
# checkout without it still builds a working image, just one where `audio-scope` needs an explicit --path, so warn
# at build time rather than letting that surface as a runtime error.
RUN if [ -f media/06_arido_III_the_oscilloscope_rmx.mp3 ]; then \
      echo "AudioScope: bundled sample track found at /src/media."; \
    else \
      echo "AudioScope WARNING: media/06_arido_III_the_oscilloscope_rmx.mp3 is missing from the build context;" \
           "'audio-scope' will need an explicit --path. See docker.md for the download link."; \
    fi

# The apps are INTERACTIVE full-screen TUIs (mouse, alternate screen, raw key input), so the container MUST be given a
# TTY. examples.sh is the single entry point; its first argument selects the app (no argument = the examples browser):
#     docker build -t jumbee-console .
#     docker run --rm -it jumbee-console                 # examples browser
#     docker run --rm -it jumbee-console agent-harness   # agent harness demo
#     docker run --rm -it jumbee-console ide             # IDE demo
#     docker run --rm -it jumbee-console audio-scope     # AudioScope demo (bundled sample track)
# Quit any app with Ctrl+Q; it restores your terminal on exit.
#
# The bundled track is "Of. — Árido III (The Oscilloscope remix)" from Modismo M028, under Creative Commons
# Attribution-NonCommercial (see docker.md). It is redistributed UNMODIFIED, and its credits and licence terms ship
# alongside it at /src/media/M028_Of_Arido.txt.
ENTRYPOINT ["bash", "/src/examples.sh"]
