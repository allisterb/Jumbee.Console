@echo off
rem Single entry point for the Jumbee.Console demos on Windows. The first argument picks which app to run; with no
rem argument (or an unrecognized one) the interactive examples browser runs. Remaining arguments pass through to the
rem chosen app. Mirrors examples.sh.
rem
rem   examples.cmd                    examples.cmd agent-harness    examples.cmd ide [project-dir]
setlocal
cd /d "%~dp0"

set "examples=examples\Jumbee.Console.Examples\bin\Release\net10.0\Jumbee.Console.Examples.exe"
set "agent=examples\Jumbee.Console.AgentHarnessDemo\bin\Release\net10.0\Jumbee.Console.AgentHarnessDemo.exe"
set "ide=examples\Jumbee.Console.IdeDemo\bin\Release\net10.0\Jumbee.Console.IdeDemo.exe"
set "audioscope=examples\Jumbee.Console.AudioScopeDemo\bin\Release\net10.0\Jumbee.Console.AudioScopeDemo.exe"
set "sandbox=examples\Jumbee.Console.3DSandboxDemo\bin\Release\net10.0\Jumbee.Console.3DSandboxDemo.exe"

rem Split the first token (the target) from the rest of the command line.
set "target=%~1"
set "rest="
for /f "tokens=1,*" %%a in ("%*") do set "rest=%%b"

rem Conditional gotos only (a `goto` inside a parenthesized `if (...)` block can fail to resolve its label in cmd).
if not defined target goto sel_examples_default
if /i "%target%"=="agent-harness" goto sel_agent
if /i "%target%"=="agent"         goto sel_agent
if /i "%target%"=="ah"            goto sel_agent
if /i "%target%"=="ide"           goto sel_ide
if /i "%target%"=="audio-scope"   goto sel_audioscope
if /i "%target%"=="3dsandbox"     goto sel_sandbox
if /i "%target%"=="3d"            goto sel_sandbox
if /i "%target%"=="sandbox"       goto sel_sandbox
if /i "%target%"=="examples"      goto sel_examples
if /i "%target%"=="browser"       goto sel_examples
if /i "%target%"=="-h"     goto usage
if /i "%target%"=="--help" goto usage
if /i "%target%"=="help"   goto usage

rem Unrecognized target -> default browser, pass every argument through.
set "name=examples browser" & set "exe=%examples%" & set "args=%*"
goto launch

:sel_agent
set "name=agent harness demo" & set "exe=%agent%" & set "args=%rest%"
goto launch
:sel_ide
set "name=IDE demo" & set "exe=%ide%" & set "args=%rest%"
goto launch
:sel_audioscope
set "name=audioscope demo" & set "exe=%audioscope%" & set "args=%rest%"
goto launch
:sel_sandbox
rem The 3D sandbox looks for a `models` folder in its WORKING DIRECTORY (falling back to a generated torus knot), so
rem it runs from its own project directory rather than from the repo root. :launch resolves %exe% before moving.
set "name=3D sandbox demo" & set "exe=%sandbox%" & set "args=%rest%" & set "workdir=%~dp0examples\Jumbee.Console.3DSandboxDemo"
goto launch
:sel_examples
set "name=examples browser" & set "exe=%examples%" & set "args=%rest%"
goto launch
:sel_examples_default
set "name=examples browser" & set "exe=%examples%" & set "args="

:launch
if not exist "%exe%" (
  echo The %name% isn't built at: %exe% 1>&2
  echo Build it first with build.cmd ^(or dotnet build ^<its .csproj^> -c Release^). 1>&2
  exit /b 1
)
rem Resolve the exe before any cd, so a target that sets `workdir` can move us without breaking the path.
set "exeabs=%CD%\%exe%"
if defined workdir cd /d "%workdir%"
"%exeabs%" %args%
exit /b %errorlevel%

:usage
echo Jumbee.Console demos - pick one to run:
echo.
echo   examples               Interactive examples browser ^(default^)
echo   agent-harness          Claude-style agent harness demo
echo   ide  [dir]             VS Code-style IDE demo ^(opens an optional project directory^)
echo   audio-scope  [args]    Real-time oscilloscope, vectorscope and spectroscope reading audio from a file or recording device
echo   3dsandbox  [args]      Real-time 3D rigid-body sandbox and OBJ model viewer ^(`3dsandbox obj [path]` opens the viewer^)
echo.
echo Usage:
echo   examples.cmd [target] [args...]
echo.
echo With no target the examples browser runs. Quit any app with Ctrl+Q.
exit /b 0
