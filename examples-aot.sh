#!/usr/bin/env bash
# Entry point for the SLIM NativeAOT image (Dockerfile.aot). Unlike the full image's examples.sh — which runs each
# demo via `dotnet <dll>` — this execs the pre-compiled NATIVE binaries. Only the AOT-eligible apps are bundled:
# the examples browser, the agent-harness demo, the AudioScope demo and the 3D sandbox. The IDE demo is NOT here: it
# is not AOT-eligible — use the full `jumbee-console` image for it.
#
#   docker run --rm -it jumbee-console-aot                 # examples browser (default)
#   docker run --rm -it jumbee-console-aot agent-harness   # agent harness demo
#   docker run --rm -it jumbee-console-aot audio-scope     # AudioScope demo (bundled sample track)
#   docker run --rm -it jumbee-console-aot 3dsandbox       # 3D physics sandbox (3dsandbox obj = model viewer)
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
audioscope=/app/audioscope/Jumbee.Console.AudioScopeDemo
sandbox=/app/sandbox/Jumbee.Console.3DSandboxDemo

case "${1:-}" in
  agent-harness)           shift; exec "$agent" "$@" ;;
  audio-scope)
                           shift; exec "$audioscope" "$@" ;;
  3dsandbox)               shift; exec "$sandbox" "$@" ;;
  browser)                 shift; exec "$examples" "$@" ;;
  ide)
    echo "The IDE demo is not AOT-eligible and is not in the slim image." >&2
    echo "Use the full image:  docker run --rm -it jumbee-console ide" >&2
    exit 2 ;;
  -h|--help|help)
    echo "Slim AOT image apps:  browser (default) | agent-harness | audio-scope | 3dsandbox"
    echo "(The IDE demo is not AOT-eligible — use the full 'jumbee-console' image.)"
    exit 0 ;;
  '')                      exec "$examples" ;;
  # An OPTION rather than a verb goes to the default browser; a mistyped target is an error (mirrors examples.sh).
  -*)                      exec "$examples" "$@" ;;
  *)
    echo "Unknown target: $1" >&2
    echo "Slim AOT image apps:  browser (default) | agent-harness | audio-scope | 3dsandbox" >&2
    exit 2 ;;
esac
