@echo off
rem Build the examples projects, then the Docker image (Dockerfile.aot: every app as a NativeAOT binary), tagged with the
rem shared ProjectAssemblyVersion under BOTH names it is published as -- jumbee-console-aot and jumbee-console -- plus
rem `latest`. One image, four tags: all of them point at the same multi-arch image.
rem It is MULTI-ARCH (linux/amd64 + linux/arm64, for Apple Silicon); the arm64 half is cross-compiled (see
rem Dockerfile.aot). Every app it ships is then VERIFIED with --verify, on both architectures -- arm64 under QEMU
rem emulation -- so a broken image fails here rather than for whoever pulls it.
rem `--no-verify` skips verification; `--no-arm64` builds for amd64 only (quicker, for local iteration -- not for
rem publishing); any other argument is passed through to `docker build`.
rem Mirrors build-docker.sh.
setlocal enabledelayedexpansion
cd /d "%~dp0"

rem Our flags are consumed here so they can never reach `docker build`, which would reject them.
set "VERIFY=1"
set "ARM64=1"
set "DOCKERARGS="
rem Extra `docker run` arguments for the verify passes: empty for the machine's own platform.
set "RUNPLATFORM="
:parse
if "%~1"=="" goto parsed
if /i "%~1"=="--no-verify" (
  set "VERIFY="
) else if /i "%~1"=="--no-arm64" (
  set "ARM64="
) else (
  set "DOCKERARGS=!DOCKERARGS! %~1"
)
shift
goto parse
:parsed

rem Through build.cmd rather than an inline list, so the set of example projects is defined in exactly one place and
rem a new demo cannot end up in the images but not the build script (or the reverse).
echo Building the examples projects (Release)...
call "%~dp0build.cmd" examples
if errorlevel 1 exit /b 1

rem The AudioScope demo defaults to the bundled sample track, which is not tracked in git — warn before an image is
rem built without it (see docker.md for the download link).
if not exist "media\06_arido_III_the_oscilloscope_rmx.mp3" (
  echo WARNING: media\06_arido_III_the_oscilloscope_rmx.mp3 is missing, so the images will have no default 1>&2
  echo          AudioScope track. See docker.md for where to download it. 1>&2
)

rem Same for the 3D sandbox's models, also untracked. Without them the sandbox still runs, on its generated torus knot.
if not exist "examples\Jumbee.Console.3DSandboxDemo\models" (
  echo WARNING: examples\Jumbee.Console.3DSandboxDemo\models is missing, so '3dsandbox' in the images will 1>&2
  echo          show only its generated torus knot. See docker.md. 1>&2
)

rem Read the shared version (ProjectAssemblyVersion, defined in src\Directory.Build.props) from a src project — the
rem examples projects live under examples\ and don't import that props file, so query Jumbee.Console for it.
set "VERSION="
for /f "usebackq delims=" %%v in (`dotnet msbuild src\Jumbee.Console\Jumbee.Console.csproj -getProperty:ProjectAssemblyVersion -nologo`) do set "VERSION=%%v"
if not defined VERSION (
  echo Could not read ProjectAssemblyVersion from src\Jumbee.Console. 1>&2
  exit /b 1
)

set "PLATFORMS=linux/amd64"
if defined ARM64 set "PLATFORMS=linux/amd64,linux/arm64"
echo Building Docker image jumbee-console-aot:%VERSION% = jumbee-console:%VERSION% for %PLATFORMS% (both also tagged latest)...
docker build !DOCKERARGS! --platform %PLATFORMS% -f Dockerfile.aot -t jumbee-console-aot:%VERSION% -t jumbee-console-aot:latest -t jumbee-console:%VERSION% -t jumbee-console:latest .
if errorlevel 1 exit /b 1
rem wolf3d is deliberately absent from the --verify list: .dockerignore excludes the id Software assets from the build
rem context (they are not redistributable), so the image never carries game data and `wolf3d --verify` inside it
rem cannot pass. :wolf3d_starts checks what can be checked without it.
call :verify jumbee-console-aot:%VERSION% browser agent-harness ide audio-scope 3dsandbox
if errorlevel 1 exit /b 1
call :wolf3d_starts jumbee-console-aot:%VERSION%
if errorlevel 1 exit /b 1

rem The arm64 half, run under QEMU: slower, but it executes the real arm64 binaries, which is the point.
if not defined ARM64 goto done
if not defined VERIFY goto done
echo Verifying the arm64 build (under QEMU emulation)...
set "RUNPLATFORM=--platform linux/arm64"
call :verify jumbee-console-aot:%VERSION% browser agent-harness ide audio-scope 3dsandbox
if errorlevel 1 exit /b 1
call :wolf3d_starts jumbee-console-aot:%VERSION%
if errorlevel 1 exit /b 1
set "RUNPLATFORM="

:done
echo Done: jumbee-console-aot:%VERSION% and jumbee-console:%VERSION%, one image (%PLATFORMS%), both also tagged latest.
exit /b 0

rem Runs every app an image ships with --verify. Each prints one PASS/FAIL line and exits, so this is the whole
rem smoke test: no TTY needed, and a container that cannot compose its layout fails the build.
:verify
set "IMAGE=%~1"
if not defined VERIFY (
  echo Skipping verification of %IMAGE% ^(--no-verify^).
  exit /b 0
)
echo Verifying %IMAGE%...
shift
:verify_loop
if "%~1"=="" exit /b 0
docker run --rm %RUNPLATFORM% %IMAGE% %~1 --verify
if errorlevel 1 (
  echo FAIL  %IMAGE%: '%~1 --verify' did not pass. 1>&2
  exit /b 1
)
shift
goto verify_loop

rem The most wolf3d can be checked without game data: that it STARTS -- runs, looks for its data and reports it missing
rem -- rather than, say, being an apphost stub that exits silently in an image with no runtime. Only a binary that
rem actually ran can print that message.
:wolf3d_starts
if not defined VERIFY exit /b 0
docker run --rm %RUNPLATFORM% %~1 wolf3d 2>&1 | findstr /C:"No Wolfenstein 3D game data" >nul
if errorlevel 1 (
  echo FAIL  %~1: 'wolf3d' did not start and report its missing game data. 1>&2
  exit /b 1
)
echo PASS  wolf3d starts and reports its missing game data ^(not verified further: no data in the image^).
exit /b 0
