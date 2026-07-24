@echo off
echo Restoring Jumbee.Console...
dotnet restore src\Jumbee.Console.sln
if errorlevel 1 exit /b 1
echo Building Jumbee.Console and examples projects...
dotnet build examples\Jumbee.Console.Examples\Jumbee.Console.Examples.csproj /p:Configuration=Release
if errorlevel 1 exit /b 1
dotnet build examples\Jumbee.Console.AgentHarnessDemo\Jumbee.Console.AgentHarnessDemo.csproj /p:Configuration=Release
if errorlevel 1 exit /b 1
dotnet build examples\Jumbee.Console.IdeDemo\Jumbee.Console.IdeDemo.csproj /p:Configuration=Release
if errorlevel 1 exit /b 1
dotnet build examples\Jumbee.Console.AudioScopeDemo\Jumbee.Console.AudioScopeDemo.csproj /p:Configuration=Release
if errorlevel 1 exit /b 1
echo Jumbee.Console build complete.