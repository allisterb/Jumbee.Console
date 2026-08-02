<#
.SYNOPSIS
    Generates the Markdown API reference for the Jumbee.Console core libraries.

.DESCRIPTION
    Runs `docfx metadata` (per docfx.json) to emit one Markdown page per public type into docs/api,
    then builds docs/api/README.md — a namespace-grouped index (with each type's summary) that GitHub
    renders automatically when the folder is opened. docfx does not produce such an index itself; it
    only emits toc.yml (for its HTML site) and the per-namespace pages.

    Also writes /llms.txt (https://llmstxt.org): the same type index reshaped into the curated link
    list that spec defines, so an LLM can discover the project's docs and fetch any page as Markdown.

.PARAMETER DocsBaseUrl
    Base URL that llms.txt links resolve against. Defaults to the repo's raw GitHub content, which
    serves the docs as plain Markdown (what the spec's ".md convention" asks for). Point this at a
    docs site if one is ever published.

.NOTES
    Run from the repo root:  powershell -File build-api-docs.ps1
    (Skip the docfx step with -NoMetadata if the pages are already current.)
#>
[CmdletBinding()]
param(
    [switch] $NoMetadata,
    [string] $DocsBaseUrl = 'https://raw.githubusercontent.com/allisterb/Jumbee.Console/master/'
)

Write-Host 'Building Jumbee.Console API documentation...' -ForegroundColor Blue

$ErrorActionPreference = 'Stop'
$repoRoot = $PSScriptRoot
$apiDir = Join-Path $repoRoot 'docs/api'

if (-not $NoMetadata) {
    Write-Host 'Running docfx metadata...' -ForegroundColor Cyan
    dotnet docfx metadata (Join-Path $repoRoot 'docfx.json')
    if ($LASTEXITCODE -ne 0) { throw "docfx metadata failed (exit $LASTEXITCODE)." }
}

Write-Host 'Building docs/api/README.md index...' -ForegroundColor Cyan

# Category display order within each namespace.
$kindOrder = @{ 'Class' = 0; 'Struct' = 1; 'Interface' = 2; 'Enum' = 3; 'Delegate' = 4 }
$kindHeading = @{ 'Class' = 'Classes'; 'Struct' = 'Structs'; 'Interface' = 'Interfaces'; 'Enum' = 'Enums'; 'Delegate' = 'Delegates' }
$dash = [string][char]0x2014   # em dash (kept out of the script literal to avoid encoding issues)

$types = New-Object System.Collections.Generic.List[object]

Get-ChildItem -Path $apiDir -Filter '*.md' | Where-Object { $_.Name -ne 'README.md' } | ForEach-Object {
    $lines = Get-Content -LiteralPath $_.FullName -Encoding UTF8
    if ($lines.Count -eq 0) { return }

    # H1, e.g. '# <a id="Jumbee_Console_Globe"></a> Class Globe'
    if ($lines[0] -notmatch '^#\s+(?:<a id="[^"]*"></a>\s*)?(Namespace|Class|Struct|Interface|Enum|Delegate)\s+(.+?)\s*$') { return }
    $kind = $Matches[1]
    if ($kind -eq 'Namespace') { return }   # namespace pages are section anchors, not entries

    $ns = ''
    foreach ($l in $lines) { if ($l -match '^Namespace:\s+\[([^\]]+)\]') { $ns = $Matches[1]; break } }
    if (-not $ns) { return }

    # Display name = file basename minus the "<namespace>." prefix (keeps nested types qualified, e.g. BarChart.HorizontalBar).
    $base = [System.IO.Path]::GetFileNameWithoutExtension($_.Name)
    $display = $base
    if ($base.StartsWith("$ns.")) { $display = $base.Substring($ns.Length + 1) }

    # Summary = the text between the "Assembly:" line and the first ```csharp fence.
    $summary = ''
    $ai = -1
    for ($i = 0; $i -lt $lines.Count; $i++) { if ($lines[$i] -match '^Assembly:') { $ai = $i; break } }
    if ($ai -ge 0) {
        $buf = @()
        for ($i = $ai + 1; $i -lt $lines.Count; $i++) {
            $t = $lines[$i]
            if ($t -match '^\s*```') { break }
            if ($t.Trim() -ne '') { $buf += $t.Trim() }
        }
        $summary = ($buf -join ' ') -replace '\s+', ' '
    }

    # docfx emits inline <xref>/<code>/<em>/<b> in summaries; convert to plain Markdown so it reads on GitHub.
    $bt = [string][char]96   # backtick
    $summary = [regex]::Replace($summary, '<xref href="([^"]+)"[^>]*>\s*</xref>', {
        param($m) $uid = $m.Groups[1].Value -replace '\(.*$', ''; ('{0}{1}{0}' -f [string][char]96, ($uid -split '\.')[-1])
    })
    $summary = $summary -replace '</?code>', $bt
    $summary = $summary -replace '</?(em|i)>', '*'
    $summary = $summary -replace '</?(b|strong)>', '**'
    $summary = $summary -replace '<[^>]+>', ''          # drop any remaining tags
    $summary = $summary -replace '&lt;', '<' -replace '&gt;', '>' -replace '&quot;', '"' -replace '&#39;', "'" -replace '&amp;', '&'
    $summary = ($summary -replace '\s+', ' ').Trim()

    $types.Add([pscustomobject]@{
        Namespace = $ns
        Display   = $display
        File      = $_.Name
        Kind      = $kind
        Summary   = $summary
    })
}

$sb = New-Object System.Text.StringBuilder
[void]$sb.AppendLine('# Jumbee.Console API Reference')
[void]$sb.AppendLine('')
[void]$sb.AppendLine('Auto-generated from the core libraries'' XML-doc comments with [docfx](https://dotnet.github.io/docfx/).')
[void]$sb.AppendLine('**Do not edit by hand** - regenerate with `powershell -File build-api-docs.ps1` from the repo root.')
[void]$sb.AppendLine('')

foreach ($ns in ($types | Select-Object -ExpandProperty Namespace -Unique | Sort-Object)) {
    [void]$sb.AppendLine("## $ns")
    [void]$sb.AppendLine('')
    $inNs = $types | Where-Object { $_.Namespace -eq $ns }
    $groups = $inNs | Group-Object Kind | Sort-Object { $kindOrder[$_.Name] }
    foreach ($g in $groups) {
        [void]$sb.AppendLine("### $($kindHeading[$g.Name])")
        [void]$sb.AppendLine('')
        foreach ($t in ($g.Group | Sort-Object Display)) {
            $line = "- [$($t.Display)]($($t.File))"
            if ($t.Summary) { $line += " $dash $($t.Summary)" }
            [void]$sb.AppendLine($line)
        }
        [void]$sb.AppendLine('')
    }
}

$readmePath = Join-Path $apiDir 'README.md'
$enc = New-Object System.Text.UTF8Encoding($false)   # UTF-8, no BOM
[System.IO.File]::WriteAllText($readmePath, $sb.ToString(), $enc)

Write-Host ("Wrote {0} ({1} types across {2} namespaces)." -f $readmePath, $types.Count, (($types | Select-Object -ExpandProperty Namespace -Unique).Count)) -ForegroundColor Green

# ─────────────────────────────────────────────────────────────────────────────
# /llms.txt  (https://llmstxt.org)
#
# Same $types data as the index above, reshaped into the structure that spec defines: an H1, a
# blockquote summary, free-form content lines (no headings), then H2-delimited link lists of the form
# "- [name](url): notes". Two rules shape the layout below:
#   * only H1/H2 are structural, so each namespace gets its own H2 rather than H3 subsections;
#   * "## Optional" has defined meaning — a section a consumer may skip for a shorter context — so the
#     internals live there and the consumer-facing docs do not.
# Links are absolute and point at raw Markdown, which is what the spec's ".md convention" is after.
# ─────────────────────────────────────────────────────────────────────────────

Write-Host 'Building llms.txt...' -ForegroundColor Cyan

function Get-DocNote {
    # First real prose PARAGRAPH of a Markdown file — headings, badges and images are chrome, not description.
    # Joined across lines because these docs hard-wrap, so taking a single line cuts the note mid-sentence.
    param([string] $Path)
    if (-not (Test-Path -LiteralPath $Path)) { return '' }
    $buf = @()
    foreach ($line in (Get-Content -LiteralPath $Path -Encoding UTF8)) {
        $t = $line.Trim()
        if ($buf.Count -eq 0) {
            if ($t -eq '' -or $t.StartsWith('#') -or $t.StartsWith('!') -or $t.StartsWith('[!') -or $t.StartsWith('>')) { continue }
            $buf += $t
        }
        elseif ($t -eq '' -or $t.StartsWith('#')) { break }
        else { $buf += $t }
    }
    if ($buf.Count -eq 0) { return '' }
    $note = ($buf -join ' ') -replace '\s+', ' '
    # Trim to a sentence boundary when one falls in range, so the note doesn't end mid-clause.
    if ($note.Length -gt 220) {
        $cut = $note.Substring(0, 220)
        $dot = $cut.LastIndexOf('. ')
        if ($dot -gt 80) { $note = $cut.Substring(0, $dot + 1) } else { $note = $cut.TrimEnd() + '...' }
    }
    return $note
}

function New-LlmsLink {
    param([string] $Title, [string] $RelPath, [string] $Notes)
    $url = $DocsBaseUrl + ($RelPath -replace ' ', '%20')
    if ($Notes) { return "- [$Title]($url): $Notes" }
    return "- [$Title]($url)"
}

# Summary comes from README's About section so the two can't drift; the fallback keeps the build green
# if that heading is ever renamed.
$about = ''
$readmeSrc = Join-Path $repoRoot 'README.md'
if (Test-Path $readmeSrc) {
    $rl = @(Get-Content -LiteralPath $readmeSrc -Encoding UTF8)
    for ($i = 0; $i -lt $rl.Count; $i++) {
        if ($rl[$i] -match '^##\s+About\b') {
            for ($j = $i + 1; $j -lt $rl.Count; $j++) {
                $t = $rl[$j].Trim()
                if ($t.StartsWith('#')) { break }
                if ($t -ne '') { $about = $t; break }
            }
            break
        }
    }
}
if (-not $about) {
    $about = 'A .NET library for building advanced terminal user interfaces (TUIs), focused on performance and usability.'
}

$lb = New-Object System.Text.StringBuilder
[void]$lb.AppendLine('# Jumbee.Console')
[void]$lb.AppendLine('')
[void]$lb.AppendLine("> $about")
[void]$lb.AppendLine('')
[void]$lb.AppendLine('Requires .NET 10.0. Distributed as three NuGet packages: `Jumbee.Console` (core, self-contained), `Jumbee.Console.Documents` (Markdown/AsciiDoc/Mermaid viewers) and `Jumbee.Console.Snapshot` (headless snapshot testing).')
[void]$lb.AppendLine('')
[void]$lb.AppendLine('Every link below is raw Markdown and can be fetched directly. The API reference is generated from the libraries'' XML-doc comments, one page per public type.')
[void]$lb.AppendLine('')

# Hand-written notes for the handful of top-level docs — they are stable, and a curated sentence beats
# whatever their first prose line happens to be.
$topDocs = @(
    @{ Title = 'README';           Path = 'README.md';           Notes = 'Project overview, feature list, Docker quick start, and a first app.' },
    @{ Title = 'Getting Started';  Path = 'GETTING-STARTED.md';  Notes = 'Task-oriented guide: layouts, controls, input, theming, testing without a terminal.' },
    @{ Title = 'Documentation index'; Path = 'docs/README.md';   Notes = 'Entry point to the guides, control docs and API reference.' },
    @{ Title = 'API reference index'; Path = 'docs/api/README.md'; Notes = 'All public types grouped by namespace, each with its summary.' },
    @{ Title = 'Troubleshooting';  Path = 'TROUBLESHOOTING.md';  Notes = 'Common build, rendering and terminal problems and their fixes.' },
    @{ Title = 'Changelog';        Path = 'CHANGELOG.md';        Notes = 'Release history.' },
    @{ Title = 'Running with Docker'; Path = 'docker.md';        Notes = 'Run the demo apps with only Docker installed; also how the images are built.' },
    @{ Title = 'Contributing';     Path = 'CONTRIBUTING.md';     Notes = 'Building the repo (submodules required), running the tests.' }
)

[void]$lb.AppendLine('## Docs')
[void]$lb.AppendLine('')
foreach ($d in $topDocs) {
    if (Test-Path (Join-Path $repoRoot $d.Path)) { [void]$lb.AppendLine((New-LlmsLink $d.Title $d.Path $d.Notes)) }
}
[void]$lb.AppendLine('')

# Curated notes for the control guides, same rationale as $topDocs above: Get-DocNote takes whatever
# the first prose paragraph happens to be, which for these ends mid-thought (e.g. "...comes in two
# groups:"). Any guide without an entry here still falls back to Get-DocNote, so adding a new guide
# needs no script change — the directory is globbed.
$controlNotes = @{
    'README'               = 'START HERE to find the right control. A decision table mapping a task ("I want to...") to the control that does it, across the whole library, then one guide per category. Also links the cross-cutting concept guides.'
    'Control Model'        = 'What nests inside what: Control, ControlFrame, ILayout and CompositeControl. Framing is a property of a control rather than a wrapper node; sizing order (Width/Height, then intrinsic, then allocated) and why 0 means unset; focus exclusivity and input tunnelling; how redraw is requested.'
    'Writing Applications' = 'Retained mode versus immediate mode and why it matters if you come from Spectre.Console: your app has no render loop. What the framework owns (frame loop, redraw scheduling, terminal setup/restore, input routing, layout) versus what you own, and how to choose a base class when writing a control.'
    'What Happens When'    = 'Behavioural FAQ with the answer first: resizing the terminal, a Grid that will not grow, no resize event, Width = 0 meaning unset, reading ActualWidth before layout, mutating a control off the UI thread, UI.Invoke swallowing exceptions, expensive work in Render, a control that throws every frame failing invisibly, double-click routing, themed properties pinning.'
    'Layouts'              = 'Arranging controls, and which layouts fill the terminal and which do not (DockPanel and SplitPanel fill; Grid is fixed-size). Boundary, overlays, and recipes for an app shell, master-detail and a dashboard.'
    'Text and Input'       = 'Showing and entering text: TextLabel, TextInput (and its orientation-first TextLabel gotcha), TextPrompt, Autocomplete, and the editor stack TextEditor -> CodeEditor -> MultiTabCodeEditor, plus ChatPrompt for an agent/chat input line.'
    'Lists and Data'       = 'Presenting rows: ListBox, DataTable, Tree, Log and TextPanel, and how to pick between them, including which own their scrolling and which need a ControlFrame, why Log rather than a growing ListBox for a stream, and DataTable having no sort and no in-place row update.'
    'Selection Controls'   = 'Checkboxes, radio buttons, switches, the single- and multi-select list controls (RadioSet, SelectionList), and the collapsed drop-down Select, including when to show every option versus collapse them.'
    'Charts'               = 'Plot, Canvas, BarChart, RunChart and Globe, and choosing between them: numeric bars (Plot) versus labelled categories (BarChart), and the dense braille filled-area chart being Canvas plus Drawing.FilledLine rather than Plot. Live series via AddLiveSeries.'
    'Display Widgets'      = 'Small self-contained readouts and status indicators: Sparkline, Digits, Gauge, ProgressBar, Spinner, Badge, Footer, GlassPanel, PerfHud and Log, including which of the four progress-ish controls to use.'
    'Navigation'           = 'Buttons, menus, modals and help: Button, MenuBar, ContextMenu, Dialog (with the Control-not-ILayout content gotcha and how to snapshot-test a modal), TabPanel and the F1 help system.'
    'Links'                = 'Link, the focusable clickable control that opens a URL or runs an action, and wiring app-level keys with UI.RegisterHotKey.'
    'Documents'            = 'Rendering and editing Markdown, AsciiDoc and Mermaid: MarkdownViewer, MarkdownExtendedViewer, AsciiDocViewer, MermaidViewer and the Interactive*Editor split-preview editors. Which are core and which need the Jumbee.Console.Documents package; all parse off the UI thread and need a frame to scroll.'
    'Terminal'             = 'TerminalEmulator: running a child process in a pseudo-console (ConPty on Windows, UnixPty elsewhere) and painting its output as a control. Input forwarding, why app hotkeys need UI.RegisterHotKey to survive it, and disposing to avoid orphan processes.'
    'Spectre Interop'      = 'Bringing existing Spectre.Console code in: SpectreControl<T> wrapping any IRenderable, SpectreLiveDisplay and SpectreTaskProgress, subclassing RenderableControl for a new control, and the AnsiConsoleBuffer bridge. Mutate wrapped widgets through UpdateContent, never directly.'
    'Composite Controls'   = 'Building one reusable Control out of several children (CompositeControl), how content height and scrolling work, and a worked example.'
    'Live Data'            = 'Wiring a continuously-updating data source into the UI: sample off the UI thread, apply one immutable snapshot per tick via UI.Invoke, split fast and slow cadences, keep the frame path cheap, and measure it with PerfHud.'
}

$controlsDir = Join-Path $repoRoot 'docs/controls'
if (Test-Path $controlsDir) {
    [void]$lb.AppendLine('## Control guides')
    [void]$lb.AppendLine('')
    # The folder's README is the hub the other guides hang off, so it leads the section rather than sorting
    # into the middle of it under the unhelpful title "README".
    $controlFiles = @(Get-ChildItem -Path $controlsDir -Filter '*.md' | Sort-Object Name)
    $controlFiles = @($controlFiles | Where-Object { $_.Name -eq 'README.md' }) +
                    @($controlFiles | Where-Object { $_.Name -ne 'README.md' })
    foreach ($f in $controlFiles) {
        $key = [System.IO.Path]::GetFileNameWithoutExtension($f.Name)
        $title = if ($key -eq 'README') { 'Controls: start here' } else { $key }
        $note = if ($controlNotes.ContainsKey($key)) { $controlNotes[$key] } else { Get-DocNote $f.FullName }
        [void]$lb.AppendLine((New-LlmsLink $title "docs/controls/$($f.Name)" $note))
    }
    [void]$lb.AppendLine('')
}

foreach ($ns in ($types | Select-Object -ExpandProperty Namespace -Unique | Sort-Object)) {
    [void]$lb.AppendLine("## API reference: $ns")
    [void]$lb.AppendLine('')
    foreach ($t in ($types | Where-Object { $_.Namespace -eq $ns } | Sort-Object Display)) {
        $title = "$($t.Display) ($($t.Kind.ToLower()))"
        [void]$lb.AppendLine((New-LlmsLink $title "docs/api/$($t.File)" $t.Summary))
    }
    [void]$lb.AppendLine('')
}

# "Optional" is load-bearing per the spec: everything here can be dropped when a shorter context is
# needed. The internals docs explain how the library is built, which a consumer of the API never needs.
$internalDir = Join-Path $repoRoot 'docs/internal'
if (Test-Path $internalDir) {
    [void]$lb.AppendLine('## Optional')
    [void]$lb.AppendLine('')
    # eval-findings is a live working backlog of API/doc gaps, not documentation — noise for a consumer.
    foreach ($f in (Get-ChildItem -Path $internalDir -Filter '*.md' | Sort-Object Name)) {
        if ($f.Name -eq 'README.md' -or $f.Name -eq 'eval-findings.md') { continue }
        $title = "Internals: " + [System.IO.Path]::GetFileNameWithoutExtension($f.Name)
        [void]$lb.AppendLine((New-LlmsLink $title "docs/internal/$($f.Name)" (Get-DocNote $f.FullName)))
    }
    [void]$lb.AppendLine('')
}

$llmsPath = Join-Path $repoRoot 'llms.txt'
[System.IO.File]::WriteAllText($llmsPath, $lb.ToString(), $enc)
Write-Host ("Wrote {0}." -f $llmsPath) -ForegroundColor Green
