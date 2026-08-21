@echo off
rem Build Jumbee.Console. The first argument picks WHAT to build; with no argument the three core libraries are built
rem and nothing else. Targets mirror examples.cmd, so `build.cmd wolf3d` and `examples.cmd wolf3d` name the same
rem thing. Remaining arguments pass through to `dotnet build`. Mirrors ./build.
rem
rem   build.cmd                    the three packable libraries
rem   build.cmd examples           every example app
rem   build.cmd wolf3d             one example app
rem   build.cmd all --verify       everything, then run each demo's headless smoke check
setlocal enabledelayedexpansion
cd /d "%~dp0"

set "CORE=src\Jumbee.Console\Jumbee.Console.csproj src\Jumbee.Console.Documents\Jumbee.Console.Documents.csproj src\Jumbee.Console.Snapshot\Jumbee.Console.Snapshot.csproj"

set "P_browser=examples\Jumbee.Console.Examples\Jumbee.Console.Examples.csproj"
set "P_agent-harness=examples\Jumbee.Console.AgentHarnessDemo\Jumbee.Console.AgentHarnessDemo.csproj"
set "P_ide=examples\Jumbee.Console.IdeDemo\Jumbee.Console.IdeDemo.csproj"
set "P_audio-scope=examples\Jumbee.Console.AudioScopeDemo\Jumbee.Console.AudioScopeDemo.csproj"
set "P_3dsandbox=examples\Jumbee.Console.3DSandboxDemo\Jumbee.Console.3DSandboxDemo.csproj"
set "P_wolf3d=examples\Jumbee.Console.Wolf3DDemo\Jumbee.Console.Wolf3DDemo.csproj"

rem The demo targets in the order the images build them. Both lists below are derived from this one, so adding a
rem demo means adding its P_ variable above and its name here.
set "ALLDEMOS=browser agent-harness ide audio-scope 3dsandbox wolf3d"

set "TARGET=%~1"
set "SKIP="

rem Conditional gotos only (a `goto` inside a parenthesized `if (...)` block can fail to resolve its label in cmd).
if not defined TARGET goto sel_core
if /i "%TARGET%"=="-h"      goto usage
if /i "%TARGET%"=="--help"  goto usage
if /i "%TARGET%"=="help"    goto usage
if /i "%TARGET%"=="core"     goto sel_core_named
if /i "%TARGET%"=="examples" goto sel_examples
if /i "%TARGET%"=="all"      goto sel_all
if defined P_%TARGET% goto sel_one

rem An OPTION rather than a verb goes to the default target with the flag kept, matching examples.cmd. Only a bare
rem word is a target, so a mistyped one is an error instead of silently building something else.
if "%TARGET:~0,1%"=="-" goto sel_core
echo Unknown target: %TARGET% 1>&2
echo. 1>&2
goto usage_err

:sel_core
set "PROJECTS=%CORE%" & set "DEMOS=" & goto collect
:sel_core_named
set "PROJECTS=%CORE%" & set "DEMOS=" & set "SKIP=1" & goto collect
:sel_examples
set "DEMOS=%ALLDEMOS%" & set "SKIP=1" & call :expand & goto collect
:sel_all
set "DEMOS=%ALLDEMOS%" & set "SKIP=1" & call :expand & set "PROJECTS=%CORE% %PROJECTS%" & goto collect
:sel_one
set "DEMOS=%TARGET%" & set "SKIP=1" & call :expand & goto collect

rem Pull --verify out of the remaining arguments: everything still standing after that is for `dotnet build`, so a
rem flag this script understands can never reach the compiler by accident.
:collect
if defined SKIP shift
set "VERIFY="
set "ARGS="
:collect_loop
if "%~1"=="" goto collected
if /i "%~1"=="--verify" (set "VERIFY=1") else (set "ARGS=!ARGS! %~1")
shift
goto collect_loop
:collected

echo Restoring Jumbee.Console...
dotnet restore src\Jumbee.Console.sln
if errorlevel 1 exit /b 1

for %%p in (%PROJECTS%) do (
  echo Building %%p...
  dotnet build %%p /p:Configuration=Release !ARGS!
  if errorlevel 1 exit /b 1
)

if not defined VERIFY goto done
if not defined DEMOS (
  echo Nothing to verify: the libraries have no headless check ^(build 'examples' or a demo^).
  goto done
)

echo.
echo Verifying...
for %%d in (%DEMOS%) do (
  rem Through examples.cmd rather than by invoking the exe directly, so the check exercises the SAME launcher a user
  rem runs -- a demo whose path or working directory is wrong there fails here.
  call "%~dp0examples.cmd" %%d --verify
  if errorlevel 1 (
    echo FAIL  %%d did not verify. 1>&2
    exit /b 1
  )
)

:done
echo Jumbee.Console build complete.
exit /b 0

rem Turns the DEMOS name list into the PROJECTS path list, via the P_<name> variables.
:expand
set "PROJECTS="
for %%d in (%DEMOS%) do set "PROJECTS=!PROJECTS! !P_%%d!"
exit /b 0

:usage
set "rc=0"
goto usage_print

:usage_err
set "rc=2"

:usage_print
echo Build Jumbee.Console - pick what to build:
echo.
echo   ^(no target^)       The three core libraries: Jumbee.Console, .Documents, .Snapshot ^(default^)
echo   examples          Every example app ^(the six the Docker images ship^)
echo   all               Core libraries, then every example app
echo.
echo   browser           Interactive examples browser
echo   agent-harness     Claude-style agent harness demo
echo   ide               VS Code-style IDE demo
echo   audio-scope       Oscilloscope / vectorscope / spectroscope demo
echo   3dsandbox         3D rigid-body sandbox and model viewer
echo   wolf3d            Wolf3D game engine port
echo.
echo Usage:
echo   build.cmd [target] [--verify] [dotnet build args...]
echo.
echo   --verify   After building, run each built demo's headless smoke check ^(--verify^) and
echo              fail if any does not print PASS. Libraries have no check and are skipped.
echo.
echo Anything else is passed to `dotnet build`, e.g. `build.cmd examples -v n`.
exit /b %rc%
