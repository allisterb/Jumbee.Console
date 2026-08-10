using System.Reflection;

using BenchmarkDotNet.Running;

// Run with:  dotnet run -c Release --project tests/Jumbee.Console.Benchmarks
// Filter with e.g.:  dotnet run -c Release --project tests/Jumbee.Console.Benchmarks -- --filter *Render*
// Map render/composite split (one mode per process, see MapPanelDiagnostics):
//   dotnet run -c Release --project tests/Jumbee.Console.Benchmarks -- --diag RebuildNoTracking
if (args.Length >= 1 && args[0] == "--diag")
{
    Jumbee.Console.Benchmarks.MapPanelDiagnostics.Diagnose(args.Length > 1 ? args[1] : null);
    return;
}

// Scope render split + ANSI bytes/frame (the terminal-load number BDN can't show):
//   dotnet run -c Release --project tests/Jumbee.Console.Benchmarks -- --scope            (overshoot = shipped app)
//   dotnet run -c Release --project tests/Jumbee.Console.Benchmarks -- --scope fitted
if (args.Length >= 1 && args[0] == "--scope")
{
    Jumbee.Console.Benchmarks.ScopeRenderDiagnostics.Diagnose(args.Length > 1 ? args[1] : null);
    return;
}

// Damage tracking on vs off on the LIVE plot path, including the ANSI bytes/frame each costs the terminal:
//   dotnet run -c Release --project tests/Jumbee.Console.Benchmarks -- --damage
if (args.Length >= 1 && args[0] == "--damage")
{
    Jumbee.Console.Benchmarks.ScopeDamageDiagnostics.Diagnose();
    return;
}

// The ceiling for a full-screen animated viewport — whole-area repaint, every cell changed, at several sizes:
//   dotnet run -c Release --project tests/Jumbee.Console.Benchmarks -- --fullscreen
//   dotnet run -c Release --project tests/Jumbee.Console.Benchmarks -- --fullscreen 80x24,200x50
if (args.Length >= 1 && args[0] == "--fullscreen")
{
    Jumbee.Console.Benchmarks.FullScreenDiagnostics.Diagnose(args.Length > 1 ? args[1] : null);
    return;
}

BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args);
