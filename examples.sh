#!/usr/bin/env bash
# Single entry point for the Jumbee.Console demos. The first argument picks which app to run; with no argument (or an
# unrecognized one) the interactive examples browser runs. Any remaining arguments are passed through to the chosen
# app. This is also the Docker image entry point, so the same selection works there:
#
#   ./examples.sh                       docker run --rm -it jumbee-console
#   ./examples.sh agent-harness         docker run --rm -it jumbee-console agent-harness
#   ./examples.sh ide [project-dir]     docker run --rm -it jumbee-console ide [project-dir]
#   ./examples.sh audio-scope           docker run --rm -it jumbee-console audio-scope
#   ./examples.sh 3dsandbox             docker run --rm -it jumbee-console 3dsandbox
set -euo pipefail
cd "$(dirname "$0")"

usage() {
  cat <<'EOF'
Jumbee.Console demos — pick one to run:

  examples          Interactive examples browser (default)
  agent-harness     Claude-style agent harness demo
  audio-scope       Real-time oscilloscope, vectorscope and spectroscope reading audio from a file or recording device
                    (no arguments plays the bundled sample track; --help lists --path, --live, --scheme and the rest)
  ide  [dir]        VS Code-style IDE demo (opens an optional project directory)
  3dsandbox [args]  Real-time 3D rigid-body sandbox and OBJ model viewer, three renderers over one scene
                    (`3dsandbox obj [path]` opens the model viewer instead; --help lists the rest)

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
  # Resolve the dll before any cd, so `workdir` can move us without breaking the path.
  local resolved="$PWD/$dll"
  if [[ -n "${workdir:-}" && -d "$workdir" ]]; then cd "$workdir"; fi
  exec dotnet "$resolved" "$@"
}

examples="examples/Jumbee.Console.Examples/bin/Release/net10.0/Jumbee.Console.Examples.dll"
agent="examples/Jumbee.Console.AgentHarnessDemo/bin/Release/net10.0/Jumbee.Console.AgentHarnessDemo.dll"
ide="examples/Jumbee.Console.IdeDemo/bin/Release/net10.0/Jumbee.Console.IdeDemo.dll"
audioscope="examples/Jumbee.Console.AudioScopeDemo/bin/Release/net10.0/Jumbee.Console.AudioScopeDemo.dll"
sandbox="examples/Jumbee.Console.3DSandboxDemo/bin/Release/net10.0/Jumbee.Console.3DSandboxDemo.dll"

# The 3D sandbox looks for a `models` folder in its WORKING DIRECTORY (falling back to a generated torus knot), so it
# runs from its own project directory rather than from the repo root. One line, and it makes the bundled models work
# the same way on a host checkout and in the full image, where COPY puts the whole repo under /src. The slim AOT image
# has no repo to point at and stages models/ beside the binaries instead -- see examples-aot.sh.
sandboxdir="examples/Jumbee.Console.3DSandboxDemo"

case "${1:-}" in
  agent-harness|agent|ah)  shift; run "agent harness demo" "$agent"       "$@" ;;
  ide)                     shift; run "IDE demo"           "$ide"         "$@" ;;
  audio-scope|audioscope|scope)
                           shift; run "audio scope demo"   "$audioscope"  "$@" ;;
  3dsandbox|3d|sandbox)    shift; workdir="$sandboxdir" run "3D sandbox demo" "$sandbox" "$@" ;;
  examples|browser)        shift; run "examples browser"   "$examples"    "$@" ;;
  -h|--help|help)          usage; exit 0 ;;
  '')                      run "examples browser" "$examples" ;;
  *)                       run "examples browser" "$examples" "$@" ;;   # unrecognized → default browser
esac
