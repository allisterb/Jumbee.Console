#!/usr/bin/env bash
# Build the six examples projects, then build BOTH Docker images tagged with the shared ProjectAssemblyVersion:
# the full playground image (Dockerfile) and the slim NativeAOT image (Dockerfile.aot).
# Any arguments are passed through to `docker build` (e.g. --pull, --no-cache). Mirrors build-docker.cmd.
set -euo pipefail
cd "$(dirname "$0")"

echo "Restoring Jumbee.Console..."
dotnet restore src/Jumbee.Console.sln

echo "Building the six examples projects (Release)..."
dotnet build examples/Jumbee.Console.Examples/Jumbee.Console.Examples.csproj -c Release
dotnet build examples/Jumbee.Console.AgentHarnessDemo/Jumbee.Console.AgentHarnessDemo.csproj -c Release
dotnet build examples/Jumbee.Console.IdeDemo/Jumbee.Console.IdeDemo.csproj -c Release
dotnet build examples/Jumbee.Console.AudioScopeDemo/Jumbee.Console.AudioScopeDemo.csproj -c Release
dotnet build examples/Jumbee.Console.3DSandboxDemo/Jumbee.Console.3DSandboxDemo.csproj -c Release
dotnet build examples/Jumbee.Console.Wolf3DDemo/Jumbee.Console.Wolf3DDemo.csproj -c Release

# The AudioScope demo defaults to the bundled sample track, which is not tracked in git — warn before an image is
# built without it (see docker.md for the download link).
if [[ ! -f media/06_arido_III_the_oscilloscope_rmx.mp3 ]]; then
  echo "WARNING: media/06_arido_III_the_oscilloscope_rmx.mp3 is missing, so the images will have no default" >&2
  echo "         AudioScope track. See docker.md for where to download it." >&2
fi

# Same for the 3D sandbox's models, also untracked. Without them the sandbox still runs, on its generated torus knot.
if [[ ! -d examples/Jumbee.Console.3DSandboxDemo/models ]]; then
  echo "WARNING: examples/Jumbee.Console.3DSandboxDemo/models is missing, so '3dsandbox' in the images will" >&2
  echo "         show only its generated torus knot. See docker.md." >&2
fi

# Read the shared version (ProjectAssemblyVersion, defined in src/Directory.Build.props) from a src project — the
# examples projects live under examples/ and don't import that props file, so query Jumbee.Console for it.
version="$(dotnet msbuild src/Jumbee.Console/Jumbee.Console.csproj -getProperty:ProjectAssemblyVersion -nologo)"
version="${version//[$'\r\n ']/}"
if [[ -z "$version" ]]; then
  echo "Could not read ProjectAssemblyVersion from src/Jumbee.Console." >&2
  exit 1
fi

echo "Building Docker image jumbee-console:$version (also tagged latest)..."
docker build "$@" -t "jumbee-console:$version" -t jumbee-console:latest .

# Also build the slim NativeAOT image (examples browser, agent harness, AudioScope and the 3D sandbox as native
# binaries; see Dockerfile.aot). The IDE demo is not in the AOT image.
echo "Building NativeAOT Docker image jumbee-console-aot:$version (also tagged latest)..."
docker build "$@" -f Dockerfile.aot -t "jumbee-console-aot:$version" -t jumbee-console-aot:latest .

echo "Done: jumbee-console:$version and jumbee-console-aot:$version (both also tagged latest)."
