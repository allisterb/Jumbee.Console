namespace Jumbee.Console;

using System;

/// <summary>
/// Base for live, split-pane source editors: a <see cref="CodeEditor"/> in one pane and a read-only preview control in
/// the other, wired so the preview re-renders as the source is edited. A draggable <see cref="SplitPanel"/> divider
/// sits between them (drag it, or focus it and press the arrows). Subclasses supply the editor's language, the preview
/// control, and how to push text into it (<see cref="ApplyPreviewText"/>).
/// </summary>
/// <remarks>
/// The preview render is comparatively slow, so it never runs on the UI thread: edits are coalesced to at most one
/// update per frame and the preview is expected to render on a background thread (as <see cref="MarkdownViewer"/> and
/// the Mermaid viewer do), discarding any render superseded by a newer edit. A half-typed document therefore reflows
/// harmlessly on the next completed render rather than blocking input.
/// </remarks>
public abstract class InteractiveSourceEditor : CompositeControl
{
    #region Constructors
    /// <param name="editor">The source editor (already constructed with its language and initial text).</param>
    /// <param name="preview">The preview control; already holding <paramref name="initialText"/> so both panes start in sync.</param>
    /// <param name="editorTitle">Frame title for the editor pane.</param>
    /// <param name="previewTitle">Frame title for the preview pane.</param>
    /// <param name="initialText">The initial document text (the sync baseline).</param>
    /// <param name="orientation">Side-by-side (editor first) or stacked (editor on top).</param>
    /// <param name="splitPosition">The editor pane's initial extent in cells.</param>
    protected InteractiveSourceEditor(CodeEditor editor, Control preview, string editorTitle, string previewTitle,
        string initialText, SplitOrientation orientation, int splitPosition)
    {
        _lastSynced = initialText ?? "";

        _editor = editor;
        // A title bar with no box: BorderStyle.None draws no border glyphs, but the title only renders when its edge
        // is a placed border — so keep BorderPlacement.Top (BorderPlacement.None would drop the title entirely).
        // focusedBorderStyle: None suppresses the theme's focus border too — each pane already shows focus its own way
        // (the editor's text cursor), so a focus box would just be redundant chrome.
        _editor.WithFrame(title: editorTitle, borderStyle: BorderStyle.None, borderPlacement: BorderPlacement.Top,
            focusedBorderStyle: BorderStyle.None);

        _previewControl = preview;
        preview.WithFrame(title: previewTitle, borderStyle: BorderStyle.None, borderPlacement: BorderPlacement.Top,
            focusedBorderStyle: BorderStyle.None);

        _split = new SplitPanel(orientation, _editor, preview, splitPosition);

        // Live preview: an edit schedules a single coalesced sync (see ScheduleSync). The editor raises Changed on
        // caret moves too, so Sync compares against the last-synced text and skips navigation-only events.
        _editor.Editor.Changed += (_, _) => ScheduleSync();

        // Dragging the divider resizes the framed panes but leaves this composite's own size unchanged, so the layout
        // only ever reports a partial OnUpdate rect — and that rect starts at the divider's *new* position, missing the
        // growing pane's old border cell just behind it. The result is a smear of stale border glyphs trailing the
        // divider until the drag ends. Force a full recomposite on each split change so the whole pane area repaints
        // from the panes' live buffers. SplitChanged fires only on a drag/nudge, so typing keeps the fast partial path.
        _split.SplitChanged += (_, _) => Invalidate();

        SetContent(_split);
    }
    #endregion

    #region Events
    /// <summary>Raised (on the UI thread, coalesced per frame) after the document text actually changes — not for
    /// caret-only movement. Carries the new text.</summary>
    public event EventHandler<string>? TextChanged;
    #endregion

    #region Properties
    /// <summary>The editor pane (focus <c>Editor.Editor</c> to type; wrapped in its own titled frame).</summary>
    public CodeEditor Editor => _editor;

    /// <summary>The split container hosting the two panes (for resizing, theming the divider, or changing minimums).</summary>
    public SplitPanel Split => _split;

    /// <summary>The preview control, for a subclass to expose as its concrete type.</summary>
    protected Control PreviewControl => _previewControl;

    /// <summary>The document text. Setting it loads the editor (caret at the top) and refreshes the preview.</summary>
    public string Text
    {
        get => _editor.Text;
        set => UI.Invoke(() => { _editor.Text = value; });   // raises Changed -> ScheduleSync -> preview
    }
    #endregion

    #region Methods
    /// <summary>Push the editor's current <paramref name="text"/> into the preview control (e.g.
    /// <c>preview.Markdown = text</c>). Called on the UI thread, coalesced per frame, only when the text changed.</summary>
    protected abstract void ApplyPreviewText(string text);

    // Coalesce a burst of edits within one frame into a single preview update: mark a sync pending and post one apply
    // to the next dispatcher drain. Headless (no running loop) syncs inline so a following render/read reflects the
    // edit immediately — which is what tests rely on.
    private void ScheduleSync()
    {
        if (!UI.IsRunning) { Sync(); return; }
        if (_syncQueued) return;
        _syncQueued = true;
        UI.Post(() => { _syncQueued = false; Sync(); });
    }

    private void Sync()
    {
        var text = _editor.Text;
        if (text == _lastSynced) return;   // caret-only Changed (navigation) — no text change, no re-render
        _lastSynced = text;
        ApplyPreviewText(text);            // subclass pushes text to the preview (de-dupes / renders off-thread)
        TextChanged?.Invoke(this, text);
    }

    // Keyboard focus lands in the editor pane by default (SetContent skips nested composites, so it can't be picked
    // up as the automatic first-focusable).
    /// <inheritdoc/>
    protected override Control? FocusChild => _editor;

    // Deliberately NOT IScrollable: both panes scroll inside their own frames, so a surrounding frame that also
    // scrolled this editor would be a second, conflicting scroller.

    /// <inheritdoc/>
    protected internal override HelpInfo? GetHelpInfo() => new HelpInfo("Editor", "Interactive Editor",
        "Edit the source on the left; a live preview renders on the right.")
        .WithKey("Type", "Edit the source")
        .WithKey("Ctrl+← / Ctrl+→", "Move focus between the panes")
        .WithKey("Drag divider", "Resize the panes")
        .WithKey("↑ / ↓, PgUp / PgDn", "Scroll the focused pane");
    #endregion

    #region Fields
    private readonly CodeEditor _editor;
    private readonly Control _previewControl;
    private readonly SplitPanel _split;
    private string _lastSynced;
    private bool _syncQueued;   // a coalesced preview sync is posted for the next frame (see ScheduleSync)
    #endregion
}
