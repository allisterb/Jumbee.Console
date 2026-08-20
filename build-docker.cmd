@echo off
setlocal
cd /d "%~dp0"

echo Restoring Jumbee.Console...
dotnet restore src\Jumbee.Console.sln
if errorlevel 1 exit /b 1

echo Building Jumbee.Console examples projects...
dotnet build examples\Jumbee.Console.Examples\Jumbee.Console.Examples.csproj /p:Configuration=Release
if errorlevel 1 exit /b 1
dotnet build examples\Jumbee.Console.AgentHarnessDemo\Jumbee.Console.AgentHarnessDemo.csproj /p:Configuration=Release
if errorlevel 1 exit /b 1
dotnet build examples\Jumbee.Console.IdeDemo\Jumbee.Console.IdeDemo.csproj /p:Configuration=Release
if errorlevel 1 exit /b 1
dotnet build examples\Jumbee.Console.AudioScopeDemo\Jumbee.Console.AudioScopeDemo.csproj /p:Configuration=Release
if errorlevel 1 exit /b 1
dotnet build examples\Jumbee.Console.3DSandboxDemo\Jumbee.Console.3DSandboxDemo.csproj /p:Configuration=Release
if errorlevel 1 exit /b 1
dotnet build examples\Jumbee.Console.Wolf3DDemo\Jumbee.Console.Wolf3DDemo.csproj /p:Configuration=Release
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
docker build %* -t jumbee-console:%VERSION% -t jumbee-console:latest .
if errorlevel 1 exit /b 1

rem Also build the slim NativeAOT image (examples browser, agent harness and AudioScope as native binaries; see
rem Dockerfile.aot). The IDE demo is not in the AOT image.
echo Building NativeAOT Docker image jumbee-console-aot:%VERSION% (also tagged latest)...
docker build %* -f Dockerfile.aot -t jumbee-console-aot:%VERSION% -t jumbee-console-aot:latest .
if errorlevel 1 exit /b 1

echo Done: jumbee-console:%VERSION% and jumbee-console-aot:%VERSION% (both also tagged latest).
