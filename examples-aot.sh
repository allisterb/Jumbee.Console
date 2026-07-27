#!/usr/bin/env bash
# Entry point for the SLIM NativeAOT image (Dockerfile.aot). Unlike the full image's examples.sh — which runs each
# demo via `dotnet <dll>` — this execs the pre-compiled NATIVE binaries. Only the three AOT-eligible apps are bundled:
# the examples browser, the agent-harness demo and the AudioScope demo. The IDE demo is intentionally NOT here: it
# needs the in-container .NET SDK (you edit a project and run `dotnet build`/`run` in its terminal pane), which the
# slim runtime image does not ship — use the full `jumbee-console` image for it.
#
#   docker run --rm -it jumbee-console-aot                 # examples browser (default)
#   docker run --rm -it jumbee-console-aot agent-harness   # agent harness demo
#   docker run --rm -it jumbee-console-aot audio-scope     # AudioScope demo (bundled sample track)
#
# The first argument picks the app; any remaining arguments pass through. Quit any app with Ctrl+Q.
set -euo pipefail

# AudioScope resolves its default --path relative to the working directory (media/…), so pin it rather than inheriting
# whatever `docker run -w` was given. WORKDIR already sets this in the image, so this only matters when it was
# overridden — and it is skipped outside the image, where /app does not exist and the paths below are wrong anyway.
if [[ -d /app ]]; then cd /app; fi

examples=/app/examples/Jumbee.Console.Examples
agent=/app/agent/Jumbee.Console.AgentHarnessDemo
audioscope=/app/audioscope/Jumbee.Console.AudioScopeDemo

case "${1:-}" in
  agent-harness|agent|ah)  shift; exec "$agent" "$@" ;;
  audio-scope|audioscope|scope)
                           shift; exec "$audioscope" "$@" ;;
  examples|browser)        shift; exec "$examples" "$@" ;;
  ide)
    echo "The IDE demo needs the .NET SDK and is not in the slim AOT image." >&2
    echo "Use the full image:  docker run --rm -it jumbee-console ide" >&2
    exit 2 ;;
  -h|--help|help)
    echo "Slim AOT image apps:  examples (default) | agent-harness | audio-scope"
    echo "(The IDE demo needs the SDK — use the full 'jumbee-console' image.)"
    exit 0 ;;
  '')                      exec "$examples" ;;
  *)                       exec "$examples" "$@" ;;   # unrecognized → default browser (mirrors examples.sh)
esac
