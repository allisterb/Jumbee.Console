@echo off
rem Build the examples projects, then BOTH Docker images tagged with the shared ProjectAssemblyVersion: the full
rem playground image (Dockerfile) and the slim NativeAOT image (Dockerfile.aot). Each image is then VERIFIED by
rem running every app it ships with --verify, so a broken image fails here rather than for whoever pulls it.
rem `--no-verify` skips that; any other argument is passed through to `docker build`. Mirrors build-docker.sh.
setlocal enabledelayedexpansion
cd /d "%~dp0"

rem --no-verify is consumed here so it can never reach `docker build`, which would reject it.
set "VERIFY=1"
set "DOCKERARGS="
:parse
if "%~1"=="" goto parsed
if /i "%~1"=="--no-verify" (set "VERIFY=") else (set "DOCKERARGS=!DOCKERARGS! %~1")
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

echo Building Docker image jumbee-console:%VERSION% (also tagged latest)...
docker build !DOCKERARGS! -t jumbee-console:%VERSION% -t jumbee-console:latest .
if errorlevel 1 exit /b 1
call :verify jumbee-console:%VERSION% browser agent-harness ide audio-scope 3dsandbox
if errorlevel 1 exit /b 1
rem wolf3d is deliberately absent above: .dockerignore excludes the id Software assets from the build context (they
rem are not redistributable), so the images never carry game data and `wolf3d --verify` inside one cannot pass.
if defined VERIFY echo   (wolf3d not verified: .dockerignore keeps the game data out of the image.)

rem Also build the slim NativeAOT image (examples browser, agent harness and AudioScope as native binaries; see
rem Dockerfile.aot). The IDE demo is not in the AOT image.
echo Building NativeAOT Docker image jumbee-console-aot:%VERSION% (also tagged latest)...
docker build !DOCKERARGS! -f Dockerfile.aot -t jumbee-console-aot:%VERSION% -t jumbee-console-aot:latest .
if errorlevel 1 exit /b 1
rem Four apps, not five: the IDE demo is not AOT-eligible and is not in the slim image (see examples-aot.sh).
call :verify jumbee-console-aot:%VERSION% browser agent-harness audio-scope 3dsandbox
if errorlevel 1 exit /b 1

echo Done: jumbee-console:%VERSION% and jumbee-console-aot:%VERSION% (both also tagged latest).
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
docker run --rm %IMAGE% %~1 --verify
if errorlevel 1 (
  echo FAIL  %IMAGE%: '%~1 --verify' did not pass. 1>&2
  exit /b 1
)
shift
goto verify_loop
