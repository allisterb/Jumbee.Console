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
:sel_audioscope
set "name=audioscope demo" & set "exe=%audioscope%" & set "args=%rest%"
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
"%exe%" %args%
exit /b %errorlevel%

:usage
echo Jumbee.Console demos - pick one to run:
echo.
echo   examples               Interactive examples browser ^(default^)
echo   agent-harness          Claude-style agent harness demo
echo   ide  [dir]             VS Code-style IDE demo ^(opens an optional project directory^)
echo   audio-scope  [args]    Real-time oscilloscope,vectorscope, and spectroscope displsy that reads audio data from a recording device or file
echo.
echo Usage:
echo   examples.cmd [target] [args...]
echo.
echo With no target the examples browser runs. Quit any app with Ctrl+Q.
exit /b 0
