#!/usr/bin/env bash
# Build the six examples projects, then the Docker image (Dockerfile.aot: every app as a NativeAOT binary), tagged
# with the shared ProjectAssemblyVersion under BOTH names it is published as -- jumbee-console-aot and jumbee-console --
# plus `latest`. One image, four tags: all of them point at the same multi-arch image.
# It is MULTI-ARCH (linux/amd64 + linux/arm64, for Apple Silicon); the arm64 half is cross-compiled (see
# Dockerfile.aot). Every app it ships is then VERIFIED with --verify, on both architectures -- arm64 under QEMU
# emulation -- so a broken image fails here rather than for whoever pulls it.
# `--no-verify` skips verification; `--no-arm64` builds for amd64 only (quicker, for local iteration -- not for
# publishing); any other argument is passed through to `docker build` (e.g. --pull, --no-cache).
# Mirrors build-docker.cmd.
set -euo pipefail
cd "$(dirname "$0")"

# Our flags are consumed here so they can never reach `docker build`, which would reject them.
verify=1
arm64=1
docker_args=()
for a in "$@"; do
  case "$a" in
    --no-verify) verify=0 ;;
    --no-arm64)  arm64=0 ;;
    *)           docker_args+=("$a") ;;
  esac
done
# Extra `docker run` arguments for the verify passes: empty for the machine's own platform.
run_platform=()

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
# fail. See wolf3d_starts for the check it gets instead.
verify_image() {
  local image="$1"; shift
  if [[ $verify -eq 0 ]]; then
    echo "Skipping verification of $image (--no-verify)."
    return 0
  fi

  echo "Verifying $image..."
  local target
  for target in "$@"; do
    if ! docker run --rm ${run_platform[@]+"${run_platform[@]}"} "$image" "$target" --verify; then
      echo "FAIL  $image: '$target --verify' did not pass." >&2
      exit 1
    fi
  done
}

# The most wolf3d can be checked without game data: that it STARTS -- runs, looks for its data and reports it missing
# (exit 1, a message naming the folder) -- rather than, say, being an apphost stub that exits silently in an image with
# no runtime. Only a binary that actually ran can print that message.
wolf3d_starts() {
  local image="$1"
  [[ $verify -eq 0 ]] && return 0
  local out
  out="$(docker run --rm ${run_platform[@]+"${run_platform[@]}"} "$image" wolf3d 2>&1 || true)"
  if [[ "$out" != *"No Wolfenstein 3D game data"* ]]; then
    echo "FAIL  $image: 'wolf3d' did not start and report its missing game data. It printed:" >&2
    echo "$out" >&2
    exit 1
  fi
  echo "PASS  wolf3d starts and reports its missing game data (not verified further: no data in the image)."
}

platforms=linux/amd64
if [[ $arm64 -eq 1 ]]; then platforms=linux/amd64,linux/arm64; fi
echo "Building Docker image jumbee-console-aot:$version = jumbee-console:$version for $platforms (both also tagged latest)..."
docker build ${docker_args[@]+"${docker_args[@]}"} --platform "$platforms" -f Dockerfile.aot \
  -t "jumbee-console-aot:$version" -t jumbee-console-aot:latest \
  -t "jumbee-console:$version" -t jumbee-console:latest .
verify_image "jumbee-console-aot:$version" browser agent-harness ide audio-scope 3dsandbox
wolf3d_starts "jumbee-console-aot:$version"
# The arm64 half, run under QEMU: slower, but it executes the real arm64 binaries, which is the point. An `if`, not
# `&&`, for the same set -e reason as elsewhere.
if [[ $arm64 -eq 1 && $verify -eq 1 ]]; then
  echo "Verifying the arm64 build (under QEMU emulation)..."
  run_platform=(--platform linux/arm64)
  verify_image "jumbee-console-aot:$version" browser agent-harness ide audio-scope 3dsandbox
  wolf3d_starts "jumbee-console-aot:$version"
  run_platform=()
fi

echo "Done: jumbee-console-aot:$version and jumbee-console:$version, one image ($platforms), both also tagged latest."
