# Contributing to Jumbee.Console

## Clone with submodules

The library contains forks of several libraries (ConsoleGUI, Spectre.Console, and a handful of others) that live under `ext/` as git submodules.

    git clone --recurse-submodules https://github.com/allisterb/Jumbee.Console

Or from an existing clone:
    git submodule update --init --recursive

## Build

    .\build.cmd     # Windows
    ./build         # Linux/macOS

With no target that builds the three packable libraries — `Jumbee.Console`, `.Documents` and `.Snapshot` — and
nothing else. A target picks something bigger or narrower, using the same names `examples.cmd` / `examples.sh` use
to *run* them:

    .\build.cmd examples        # every example app
    .\build.cmd wolf3d          # just one
    .\build.cmd all             # libraries, then every example app
    .\build.cmd --help          # the full list

Add `--verify` to run each demo's headless smoke check after building it, failing if any does not pass. Anything
else is handed to `dotnet build`, so `./build examples -v n` works. To build the whole solution directly:

    dotnet build src/Jumbee.Console.sln

You'll' need the .NET 10 SDK.

## Test

The first-party test suite is an xUnit project:

    dotnet test tests/Jumbee.Console.Tests/Jumbee.Console.Tests.csproj

Or run everything in the solution with `dotnet test src/Jumbee.Console.sln`.

## Where 

- `src/` contains the main library code, and it's where most changes will land.
- `/ext` contains library-wide dependencies, imported as vendored forks in submodules. If a fork genuinely needs a change, you should raise an issue first.
- Match the surrounding style: 4-space indentation, `#region` grouping, and the public → internal → protected → private member ordering the files already use. Target .NET 10 / C# 14.

The architecture (how the ConsoleGUI layout engine and the Spectre.Console rendering pipeline are bridged, plus other core design points) is written up under [docs/internal](docs/internal/). Read that before attempting a large change.

## Pull requests

- Keep a PR focused on one thing.
- Say what you changed, why, and how you tested it.
- New public API should carry XML-doc comments and, where it makes sense, a snapshot test (see the `Jumbee.Console.Snapshot` package and the existing tests).
