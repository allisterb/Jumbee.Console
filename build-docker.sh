#!/usr/bin/env bash
# Build the six examples projects, then build BOTH Docker images tagged with the shared ProjectAssemblyVersion:
# the full playground image (Dockerfile) and the slim NativeAOT image (Dockerfile.aot). Each image is then VERIFIED
# by running every app it ships with --verify, so a broken image fails here rather than for whoever pulls it.
# `--no-verify` skips that; any other argument is passed through to `docker build` (e.g. --pull, --no-cache).
# Mirrors build-docker.cmd.
set -euo pipefail
cd "$(dirname "$0")"

# --no-verify is consumed here so it can never reach `docker build`, which would reject it.
verify=1
docker_args=()
for a in "$@"; do
  if [[ "$a" == "--no-verify" ]]; then verify=0; else docker_args+=("$a"); fi
done

# Through ./build rather than an inline list, so the set of example projects is defined in exactly one place and a
# new demo cannot end up in the images but not the build script (or the reverse).
echo "Building the examples projects (Release)..."
./build examples

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

# Runs every app an image ships with --verify. Each app prints one PASS/FAIL line and exits, so this is the whole
# smoke test: no TTY needed, and a container that cannot compose its layout fails the build.
#
# wolf3d is deliberately absent. .dockerignore excludes the id Software assets from the build context (they are not
# redistributable), so the images never carry game data and `wolf3d --verify` inside one cannot do anything but
# fail. It is reported as skipped rather than quietly dropped.
verify_image() {
  local image="$1"; shift
  if [[ $verify -eq 0 ]]; then
    echo "Skipping verification of $image (--no-verify)."
    return 0
  fi

  echo "Verifying $image..."
  local target
  for target in "$@"; do
    if ! docker run --rm "$image" "$target" --verify; then
      echo "FAIL  $image: '$target --verify' did not pass." >&2
      exit 1
    fi
  done
}

echo "Building Docker image jumbee-console:$version (also tagged latest)..."
docker build ${docker_args[@]+"${docker_args[@]}"} -t "jumbee-console:$version" -t jumbee-console:latest .
verify_image "jumbee-console:$version" browser agent-harness ide audio-scope 3dsandbox
# An `if`, not `[[ ... ]] && echo`: under `set -e` a false test at the end of an && list exits the script, which
# would abort the build for the entirely normal case of --no-verify.
if [[ $verify -eq 1 ]]; then
  echo "  (wolf3d not verified: .dockerignore keeps the game data out of the image.)"
fi

# Also build the slim NativeAOT image (examples browser, agent harness, AudioScope and the 3D sandbox as native
# binaries; see Dockerfile.aot). The IDE demo is not in the AOT image.
echo "Building NativeAOT Docker image jumbee-console-aot:$version (also tagged latest)..."
docker build ${docker_args[@]+"${docker_args[@]}"} -f Dockerfile.aot -t "jumbee-console-aot:$version" -t jumbee-console-aot:latest .
# Four apps, not five: the IDE demo is not AOT-eligible and is not in the slim image (see examples-aot.sh).
verify_image "jumbee-console-aot:$version" browser agent-harness audio-scope 3dsandbox

echo "Done: jumbee-console:$version and jumbee-console-aot:$version (both also tagged latest)."
