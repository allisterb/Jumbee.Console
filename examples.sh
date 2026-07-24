#!/usr/bin/env bash
# Single entry point for the Jumbee.Console demos. The first argument picks which app to run; with no argument (or an
# unrecognized one) the interactive examples browser runs. Any remaining arguments are passed through to the chosen
# app. This is also the Docker image entry point, so the same selection works there:
#
#   ./examples.sh                       docker run --rm -it jumbee-console
#   ./examples.sh agent-harness         docker run --rm -it jumbee-console agent-harness
#   ./examples.sh ide [project-dir]     docker run --rm -it jumbee-console ide [project-dir]
set -euo pipefail
cd "$(dirname "$0")"

usage() {
  cat <<'EOF'
Jumbee.Console demos — pick one to run:

  examples          Interactive examples browser (default)
  agent-harness     Claude-style agent harness demo
  audio-scope       Real-time oscilloscope,vectorscope, and spectroscope displsy that reads audio data from a recording device or file
  ide  [dir]        VS Code-style IDE demo (opens an optional project directory)

Usage:
  ./examples.sh [target] [args...]
  docker run --rm -it jumbee-console [target] [args...]

With no target the examples browser runs. The apps are full-screen TUIs, so a
container needs a TTY (the -it flags). Quit any of them with Ctrl+Q.
EOF
}

run() {
  local name="$1" dll="$2"; shift 2
  if [[ ! -f "$dll" ]]; then
    echo "The $name isn't built at: $dll" >&2
    echo "Build it (dotnet build <its .csproj> -c Release) or rebuild the Docker image." >&2
    exit 1
  fi
  exec dotnet "$dll" "$@"
}

examples="examples/Jumbee.Console.Examples/bin/Release/net10.0/Jumbee.Console.Examples.dll"
agent="examples/Jumbee.Console.AgentHarnessDemo/bin/Release/net10.0/Jumbee.Console.AgentHarnessDemo.dll"
ide="examples/Jumbee.Console.IdeDemo/bin/Release/net10.0/Jumbee.Console.IdeDemo.dll"
audioscope="examples/Jumbee.Console.AudioScopeDemo/bin/Release/net10.0/Jumbee.Console.AudioScopeDemo.dll"

case "${1:-}" in
  agent-harness|agent|ah)  shift; run "agent harness demo" "$agent"       "$@" ;;
  ide)                     shift; run "IDE demo"           "$ide"         "$@" ;;
  audio-scope)             shift; run "audio scope demo"   "$audioscope"  "$@" ;;
  examples|browser)        shift; run "examples browser"   "$examples"    "$@" ;;
  -h|--help|help)          usage; exit 0 ;;
  '')                      run "examples browser" "$examples" ;;
  *)                       run "examples browser" "$examples" "$@" ;;   # unrecognized → default browser
esac
