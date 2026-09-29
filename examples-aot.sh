#!/usr/bin/env bash
# Entry point for the Docker image (Dockerfile.aot), published as both jumbee-console and jumbee-console-aot. It execs
# the pre-compiled NATIVE binaries, where examples.sh -- the launcher for a checkout -- runs each demo via
# `dotnet <dll>`. It accepts exactly the targets examples.sh does, and the old JIT image's entry point was examples.sh,
# so every command written for that image still works against this one.
#
#   docker run --rm -it jumbee-console-aot                 # examples browser (default)
#   docker run --rm -it jumbee-console-aot agent-harness   # agent harness demo
#   docker run --rm -it jumbee-console-aot ide             # IDE demo (its Build menu offers to install the SDK)
#   docker run --rm -it jumbee-console-aot audio-scope     # AudioScope demo (bundled sample track)
#   docker run --rm -it jumbee-console-aot 3dsandbox       # 3D physics sandbox (3dsandbox view = model viewer)
#   docker run --rm -it -v /path/to/WL1:/app/wolf3d/GameData jumbee-console-aot wolf3d
#                                                          # Wolf3D walkthrough (the game data is yours to supply)
#
# The first argument picks the app; any remaining arguments pass through. Quit any app with Ctrl+Q.
set -euo pipefail

# AudioScope and the 3D sandbox both resolve a default asset path relative to the working directory (media/… and
# models/), so pin it rather than inheriting whatever `docker run -w` was given. WORKDIR already sets this in the
# image, so this only matters when it was overridden — and it is skipped outside the image, where /app does not exist
# and the paths below are wrong anyway.
if [[ -d /app ]]; then cd /app; fi

examples=/app/examples/Jumbee.Console.Examples
agent=/app/agent/Jumbee.Console.AgentHarnessDemo
ide=/app/ide/Jumbee.Console.IdeDemo
audioscope=/app/audioscope/Jumbee.Console.AudioScopeDemo
sandbox=/app/sandbox/Jumbee.Console.3DSandboxDemo
wolf3d=/app/wolf3d/Jumbee.Console.Wolf3DDemo

case "${1:-}" in
  agent-harness)           shift; exec "$agent" "$@" ;;
  audio-scope)
                           shift; exec "$audioscope" "$@" ;;
  3dsandbox)               shift; exec "$sandbox" "$@" ;;
  wolf3d)                  shift; exec "$wolf3d" "$@" ;;
  browser)                 shift; exec "$examples" "$@" ;;
  ide)                     shift; exec "$ide" "$@" ;;
  -h|--help|help)
    echo "Apps:  browser (default) | agent-harness | ide | audio-scope | 3dsandbox | wolf3d"
    exit 0 ;;
  '')                      exec "$examples" ;;
  # An OPTION rather than a verb goes to the default browser; a mistyped target is an error (mirrors examples.sh).
  -*)                      exec "$examples" "$@" ;;
  *)
    echo "Unknown target: $1" >&2
    echo "Apps:  browser (default) | agent-harness | ide | audio-scope | 3dsandbox | wolf3d" >&2
    exit 2 ;;
esac
