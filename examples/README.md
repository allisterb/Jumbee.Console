# Jumbee.Console Examples
Jumbee.Console.Examples is the main examples browser project. Jumbee.Console.IdeDemo features a text-based IDE with a multi-tab editor, a file explorer tree control, and terminal emulator, built with Jumbee.Console.

Jumbee.Console.NewsReaderDemo is a terminal RSS reader (feed tree, article list, markdown reader, vim-style keys) with the domain logic split into a UI-free `NewsReaderDemo.Core` library. Its [README](Jumbee.Console.NewsReaderDemo/README.md) is a getting-started case study that walks through building it; run the headless snapshot suite with `dotnet run --project Jumbee.Console.NewsReaderDemo/NewsReaderDemo.App -c Release -- --test`.
Jumbee.Console.Wolf3DDemo walks through the original Wolfenstein 3D's levels in the terminal: real maps, textures
and scenery sprites, cast by a real raycaster onto a half-block pixel surface. The engine is vendored unmodified
from [Wolfenshine](https://github.com/deanthecoder/Wolfenshine); game data is not redistributable and must be
supplied by the user. Its [README](Jumbee.Console.Wolf3DDemo/README.md) carries the ANSI-bandwidth measurements —
including why a frame's cost tracks colour *runs* rather than colour count.
