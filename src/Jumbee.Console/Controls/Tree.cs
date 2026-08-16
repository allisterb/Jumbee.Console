namespace Jumbee.Console;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Spectre.Console;
using Spectre.Console.Rendering;
using Spectre.Console.Interop;
using ConsoleGUI.Input;
using ConsoleGUI.Space;

using CircularTreeException = Spectre.Console.Interop.CircularTreeException;

/// <summary>The line style used to draw the connecting guide lines of a <see cref="Tree"/>.</summary>
public enum TreeGuide
{
    /// <summary>ASCII guide lines (<c>|</c>, <c>+</c>, <c>-</c>) for terminals without Unicode support.</summary>
    Ascii,
    /// <summary>Single box-drawing guide lines.</summary>
    Line,
    /// <summary>Heavy (bold) box-drawing guide lines.</summary>
    BoldLine,
    /// <summary>Double box-drawing guide lines.</summary>
    DoubleLine,
    /// <summary>No connector lines — hierarchy is shown by indentation (and any node glyphs) alone. Useful for a
    /// clean, icon-driven tree.</summary>
    None
}

/// <summary>
/// Displays a hierarchical list of items in a tree layout.
/// </summary>
/// <remarks>
/// Based on <see cref="Spectre.Console.Tree"/> but modified to support mutable tree nodes, concurrent updates, and node selection via user input.
/// </remarks>
public partial class Tree : RenderableControl, IScrollable
{
    #region Constructors
    /// <summary>
    /// Create a tree with a root label.
    /// </summary>
    /// <param name="rootLabel">The tree root label.</param>
    /// <param name="guide">The connector-line glyph set (defaults to <see cref="TreeGuide.Line"/>).</param>
    /// <param name="guideStyle">The style of the connector lines (defaults to <see cref="Style.Plain"/>).</param>
    /// <param name="expanded">Whether the root starts expanded.</param>
    public Tree(IRenderable rootLabel, TreeGuide? guide = null, Style? guideStyle = null, bool expanded = true) : base()
    {
        this._rootLabel = rootLabel;
        this._root = new TreeNode(this, 0, _rootLabel);
        this._style = guideStyle ?? Style.Plain;
        this._guide = guide ?? TreeGuide.Line;
        this.scguide = GetSpectreConsoleTreeGuide(this._guide);
        this._expanded = expanded;
        ApplyTheme();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tree"/> class.
    /// </summary>
    /// <param name="rootText">The tree root label as a string.</param>
    /// <param name="guide">The connector-line glyph set (defaults to <see cref="TreeGuide.Line"/>).</param>
    /// <param name="guideStyle">The style of the connector lines (defaults to <see cref="Style.Plain"/>).</param>
    /// <param name="expanded">Whether the root starts expanded.</param>
    public Tree(string rootText, TreeGuide? guide = null, Style ? guideStyle = null, bool expanded = true) :
        this(new Markup(rootText), guide, guideStyle, expanded) 
    {
        _root.Text = rootText;
    }
    #endregion
    
    #region Properties
    /// <summary>The root node of the tree.</summary>
    public TreeNode Root => _root;

    /// <summary>
    /// Gets or sets the tree style.
    /// </summary>
    public Style Style
    {
        get => _style;
        set => SetAtomicProperty(ref _style, value);
    }
    /// <summary>
    /// Gets or sets the tree guide lines.
    /// </summary>
    public TreeGuide Guide
    {
        get => _guide;
        set => SetAtomicProperty(ref _guide, value, watch: (_, _) => scguide = GetSpectreConsoleTreeGuide(_guide));
    }

    /// <summary>
    /// Gets or sets a value indicating whether or not the tree is expanded or not.
    /// </summary>
    public bool Expanded
    {
        get => _expanded;
        set => SetAtomicProperty(ref _expanded, value);
    }

    internal ICollection<TreeNode> Nodes => _root.Children;


    /// <summary>Foreground of the selected node. Defaults to the theme's <see cref="IStyleTheme.Selection"/>.</summary>
    public Color? SelectedForegroundColor
    {
        get => _selectedForegroundColor;
        set => SetAtomicProperty(ref _selectedForegroundColor, value, themeOverride: true);
    }

    /// <summary>Background of the selected node. Defaults to the theme's <see cref="IStyleTheme.Selection"/>.</summary>
    public Color? SelectedBackgroundColor
    {
        get => _selectedBackgroundColor;
        set => SetAtomicProperty(ref _selectedBackgroundColor, value, themeOverride: true);
    }

    /// <summary>How the selected node is indicated — highlight / underline / caret. Defaults to the theme's
    /// <see cref="IStyleTheme.SelectionStyle"/>.</summary>
    public SelectionStyle SelectionStyle
    {
        get => _selectionStyle;
        set => SetAtomicProperty(ref _selectionStyle, value, themeOverride: true);
    }

    /// <summary>The glyph shown before a node that has no children (a leaf), including trailing spacing. Defaults to
    /// the theme's <see cref="IGlyphTheme.TreeLeaf"/>.</summary>
    /// <remarks>Keep its width equal to the disclosure glyphs so labels align.</remarks>
    public string LeafGlyph
    {
        get => _leafGlyph;
        set => SetAtomicProperty(ref _leafGlyph, value, themeOverride: true);
    }

    /// <summary>Foreground colour of the <see cref="LeafGlyph"/>, or <see langword="null"/> for the default text
    /// colour. Defaults to the theme's <see cref="IStyleTheme.TreeLeaf"/> colour.</summary>
    /// <remarks>When a (text) leaf is selected the glyph is highlighted together with the text instead of using this
    /// colour.</remarks>
    public Color? LeafGlyphColor
    {
        get => _leafGlyphColor;
        set => SetAtomicProperty(ref _leafGlyphColor, value, themeOverride: true);
    }

    /// <summary>When <see langword="true"/>, the node under the mouse pointer is tinted with <see cref="HoverStyle"/>.
    /// Defaults to <see langword="false"/> (off — pointer movement is ignored so it doesn't distract from selection).</summary>
    public bool HoverHighlighting
    {
        get => _hoverHighlighting;
        set
        {
            if (_hoverHighlighting == value) return;
            _hoverHighlighting = value;
            // Drop any existing tint when turning it off so a stale hovered row doesn't linger.
            if (!value && _hoveredNode is not null) _hoveredNode = null;
            Invalidate();
        }
    }

    /// <summary>The style applied to the node under the mouse pointer when <see cref="HoverHighlighting"/> is on
    /// (typically a background tint). Defaults to the theme's <see cref="IStyleTheme.Hover"/>.</summary>
    /// <remarks>Like selection, it tints the glyph + label of text nodes only.</remarks>
    public Style HoverStyle
    {
        get => _hoverStyle;
        set => SetAtomicProperty(ref _hoverStyle, value, themeOverride: true);
    }

    /// <summary>An optional menu shown when a node is right-clicked. Left <see langword="null"/> = no menu.</summary>
    /// <remarks>The right-click first selects that node and raises <see cref="ContextMenuOpening"/> (with the node),
    /// then the menu floats at the pointer. Item handlers can read <see cref="SelectedNode"/> to act on the
    /// right-clicked node.</remarks>
    public ContextMenu? ContextMenu { get; set; }

    /// <summary>Always <see langword="true"/>: the tree handles its own navigation and expand/collapse keys. No
    /// opt-in needed — unlike the base default, this is on.</summary>
    public override bool HandlesInput => true;

    /// <summary>Always <see langword="true"/>: nodes receive hover and click (click to select or toggle, hover to
    /// highlight). No opt-in needed — unlike the base default, this is on.</summary>
    protected override bool WantsMouse => true;

    #endregion

    #region Events
    /// <inheritdoc/>
    public event EventHandler<RowSpan>? FocusRowChanged;

    /// <summary>Raised when a leaf node (one with no children) is activated — double-clicked, or Enter/Space pressed
    /// while it is selected. Parent nodes toggle expansion instead of raising this.</summary>
    public event EventHandler<TreeNode>? NodeActivated;

    /// <summary>Raised whenever the selected (highlighted) node changes — via the arrow/vim keys,
    /// Home/End/PageUp/PageDown, or a mouse click. Mirrors <see cref="ListBox.SelectionChanged"/>; use it to follow
    /// highlight movement (e.g. update a detail pane as the user arrows through the tree), rather than only reacting
    /// to <see cref="NodeActivated"/> (which fires only on leaf Enter/double-click).</summary>
    public event EventHandler<TreeNode>? SelectionChanged;

    /// <summary>
    /// Raised just before a node with children is expanded — by the keyboard, by a click on its disclosure glyph,
    /// or by a double-click on its label.
    /// </summary>
    /// <remarks>
    /// This is the hook for a tree too large to build up front (a file system, a remote hierarchy): populate the
    /// node's real children here. Give the unexpanded node one placeholder child so it renders as a parent and can
    /// be opened at all — a node with no children is a leaf, and nothing will ever ask it to expand — then replace
    /// that placeholder when this fires.
    /// </remarks>
    public event EventHandler<TreeNode>? NodeExpanding;

    /// <summary>Raised just before <see cref="ContextMenu"/> is shown for a right-clicked node, with that node (now
    /// the selected one). Only fires when <see cref="ContextMenu"/> is set.</summary>
    /// <remarks>Use it to tailor the menu to the node, or read <see cref="SelectedNode"/> from the menu's own item
    /// handlers.</remarks>
    public event EventHandler<TreeNode>? ContextMenuOpening;
    #endregion

    #region Indexers
    /// <summary>Gets the direct child of the root node at <paramref name="index"/>, or <see langword="null"/> if none.</summary>
    public TreeNode? this[uint index] => _root[index];
    #endregion

    #region Methods
    /// <inheritdoc/>
    // Default the selected-node colours from the theme so a bare Tree shows a visible selection (re-applied on a
    // runtime theme switch; explicit SelectedForeground/BackgroundColor overrides are left alone).
    protected override void ApplyTheme()
    {
        if (!IsThemeOverridden(nameof(SelectedForegroundColor))) _selectedForegroundColor = UI.StyleTheme.Selection.ForegroundColor;
        if (!IsThemeOverridden(nameof(SelectedBackgroundColor))) _selectedBackgroundColor = UI.StyleTheme.Selection.BackgroundColor;
        if (!IsThemeOverridden(nameof(SelectionStyle))) _selectionStyle = UI.StyleTheme.SelectionStyle;
        _selectionCaret = UI.GlyphTheme.SelectionCaret;
        _treeExpanded = UI.GlyphTheme.TreeExpanded;
        _treeCollapsed = UI.GlyphTheme.TreeCollapsed;
        if (!IsThemeOverridden(nameof(LeafGlyph))) _leafGlyph = UI.GlyphTheme.TreeLeaf;
        if (!IsThemeOverridden(nameof(LeafGlyphColor))) _leafGlyphColor = UI.StyleTheme.TreeLeaf.ForegroundColor;
        if (!IsThemeOverridden(nameof(HoverStyle))) _hoverStyle = UI.StyleTheme.Hover;
    }

    /// <summary>Adds a top-level node with the given renderable label and returns it.</summary>
    public TreeNode AddNode(IRenderable label) => _root.AddChild(label);

    /// <summary>Adds a top-level node with the given text label and returns it.</summary>
    public TreeNode AddNode(string label) => _root.AddChild(label);

    /// <summary>Adds several top-level nodes from the given renderable labels; returns this tree for chaining.</summary>
    public Tree AddNodes(params IRenderable[] labels)
    {
        _root.AddChildren(labels);
        return this;
    }

    /// <summary>Adds several top-level nodes from the given text labels; returns this tree for chaining.</summary>
    public Tree AddNodes(params string[] labels)
    {
        _root.AddChildren(labels);
        return this;
    }

    /// <summary>Removes <paramref name="node"/> from the root's children; returns <see langword="true"/> if it was removed.</summary>
    public bool RemoveNode(TreeNode node) => _root.RemoveChild(node.Index);

    /// <inheritdoc/>
    protected override void OnInput(InputEvent inputEvent)
    {
        switch (inputEvent.Key.Key)
        {
            case ConsoleKey.DownArrow:
                NavigateTree(1);
                inputEvent.Handled = true;
                break;
            case ConsoleKey.UpArrow:
                NavigateTree(-1);
                inputEvent.Handled = true;
                break;
            case ConsoleKey.Home:
                SelectVisibleIndex(0);
                inputEvent.Handled = true;
                break;
            case ConsoleKey.End:
                SelectVisibleIndex(int.MaxValue);
                inputEvent.Handled = true;
                break;
            case ConsoleKey.PageUp:
                SelectVisibleIndex(CurrentVisibleIndex() - TreePage());
                inputEvent.Handled = true;
                break;
            case ConsoleKey.PageDown:
                SelectVisibleIndex(CurrentVisibleIndex() + TreePage());
                inputEvent.Handled = true;
                break;
            // Right: expand a collapsed parent, else step into its first child. Left: collapse an expanded parent,
            // else step out to the parent. Enter/Space toggle the selected parent. (No-op on a leaf with no parent.)
            case ConsoleKey.RightArrow:
                if (SelectedNode is { Nodes.Count: > 0 } r)
                {
                    if (!r.Expanded) SetExpanded(r, true);
                    else SelectNode(r.Nodes.OrderBy(n => n.Index).First());
                }
                inputEvent.Handled = true;
                break;
            case ConsoleKey.LeftArrow:
                if (SelectedNode is { } l)
                {
                    if (l.Nodes.Count > 0 && l.Expanded) SetExpanded(l, false);
                    else if (l.Parent is { } parent) SelectNode(parent);
                }
                inputEvent.Handled = true;
                break;
            case ConsoleKey.Enter:
            case ConsoleKey.Spacebar:
                // A parent toggles; a leaf activates.
                if (SelectedNode is { } t)
                {
                    if (t.Nodes.Count > 0) SetExpanded(t, !t.Expanded);
                    else NodeActivated?.Invoke(this, t);
                }
                inputEvent.Handled = true;
                break;
        }
    }

    /// <summary>The currently selected visible node, or <see langword="null"/> if none is selected.</summary>
    public TreeNode? SelectedNode => Flatten(_root).FirstOrDefault(n => n.Selected);

    // Mouse: each visible node is one row, so the listener's content-space Y is the flattened-node index (the frame
    // adjusts for scroll). A single click selects; clicking a parent's disclosure glyph toggles it; double-clicking
    // a parent's label toggles it — so a double-click anywhere on a parent row ends up toggled exactly once. (Wheel
    // scrolling is the inherited default: OnMouseWheel -> Frame.Scroll.)
    /// <inheritdoc/>
    protected override void OnClick(Position position)
    {
        if (UI.MouseButton == TerminalMouseButton.Right) { OpenContextMenu(position); return; }
        if (NodeAt(position.Y) is not { } node) return;
        if (node.Nodes.Count > 0 && OnGlyph(node, position)) SetExpanded(node, !node.Expanded);
        SelectNode(node);
    }

    /// <summary>
    /// Expands or collapses <paramref name="node"/> the way the keyboard and mouse do — raising
    /// <see cref="NodeExpanding"/> first when it is about to open.
    /// </summary>
    /// <remarks>Prefer this over setting <see cref="TreeNode.Expanded"/> directly whenever a
    /// <see cref="NodeExpanding"/> handler is attached; the property is the raw state and does not announce
    /// itself.</remarks>
    public void SetExpanded(TreeNode node, bool expanded)
    {
        if (node.Expanded == expanded) return;
        if (expanded) NodeExpanding?.Invoke(this, node);
        node.Expanded = expanded;
    }

    /// <inheritdoc/>
    protected override void OnDoubleClick(Position position)
    {
        // A fast double right-click still just opens the menu (don't toggle/activate underneath it).
        if (UI.MouseButton == TerminalMouseButton.Right) { OpenContextMenu(position); return; }
        if (NodeAt(position.Y) is not { } node) return;
        if (node.Nodes.Count > 0)
        {
            // A glyph click was already toggled by the preceding single-click; only toggle here for a label double-click.
            // OnGlyph is now row-aware, so a wrapped continuation row (which has no glyph) counts as a label click and
            // toggles here as expected.
            if (!OnGlyph(node, position)) SetExpanded(node, !node.Expanded);
        }
        else
        {
            NodeActivated?.Invoke(this, node);   // double-clicking a leaf activates it
        }
        SelectNode(node);
    }

    // Right-click: select the node under the pointer, announce it, then float ContextMenu at the pointer. The menu
    // shows in the ambient UI.Overlay (ContextMenu.Show), and item handlers can read SelectedNode. No-op if no menu
    // is set or the click missed every node.
    private void OpenContextMenu(Position contentPos)
    {
        if (ContextMenu is null || NodeAt(contentPos.Y) is not { } node) return;
        SelectNode(node);
        ContextMenuOpening?.Invoke(this, node);
        if (ConsoleGUI.ConsoleManager.MousePosition is { } m) ContextMenu.Show(m.X, m.Y);
        else ContextMenu.Show(contentPos.X, contentPos.Y);
    }

    // Hover: track the node under the pointer and repaint so its row is tinted; clear it when the pointer leaves.
    // Opt-in via HoverHighlighting (off by default) — when disabled we ignore pointer movement entirely.
    /// <inheritdoc/>
    protected override void OnMouseMove(Position position)
    {
        if (!_hoverHighlighting) return;
        var node = NodeAt(position.Y);
        if (!ReferenceEquals(node, _hoveredNode))
        {
            _hoveredNode = node;
            Invalidate();
        }
    }

    /// <inheritdoc/>
    protected override void OnMouseLeave()
    {
        if (!_hoverHighlighting) return;
        if (_hoveredNode is not null)
        {
            _hoveredNode = null;
            Invalidate();
        }
    }

    // The visible node occupying content row `row`, or null if the row is past the tree. Uses the row map built by the
    // last Render so a wrapped (multi-row) label maps every one of its rows back to the same node — a plain
    // row==flattened-index assumption mis-identifies every node below the first wrap.
    private TreeNode? NodeAt(int row) => row >= 0 && row < _rowMap.Count ? _rowMap[row].node : null;

    // True when `row` is a node's first (glyph-bearing) row; a wrapped continuation row draws a spacer, not the
    // disclosure glyph, so a click there must not toggle expansion.
    private bool IsGlyphRow(int row) => row >= 0 && row < _rowMap.Count && _rowMap[row].first;

    // The content row where `node` starts in the last render and how many rows it spans, or (-1, 0) if it wasn't
    // rendered (not yet painted, or hidden under a collapsed ancestor). A node's rows are contiguous in the map.
    private (int start, int height) RowSpanOf(TreeNode node)
    {
        var start = -1;
        for (var i = 0; i < _rowMap.Count; i++)
        {
            if (ReferenceEquals(_rowMap[i].node, node))
            {
                if (start < 0) start = i;
            }
            else if (start >= 0)
            {
                return (start, i - start);
            }
        }
        return start < 0 ? (-1, 0) : (start, _rowMap.Count - start);
    }

    // True when `position` lands on the node's gutter glyph: on the node's first (glyph-bearing) row AND within the
    // glyph's column (drawn at depth * guide-part width). A wrapped continuation row draws a spacer, not the glyph,
    // so a click there is a label click, never a glyph click — keeping toggle behaviour consistent across all rows.
    private bool OnGlyph(TreeNode node, Position position)
    {
        if (!IsGlyphRow(position.Y)) return false;
        var glyphStart = Depth(node) * GuidePartWidth();
        var glyph = node.Nodes.Count > 0
            ? (node.Expanded ? node.ExpandedGlyph ?? _treeExpanded : node.CollapsedGlyph ?? _treeCollapsed)
            : (node.LeafGlyph ?? _leafGlyph);
        return position.X >= glyphStart && position.X < glyphStart + glyph.GetCellWidth();
    }

    // The node's depth (root = 0); its guide prefix is this many fixed-width guide parts.
    private static int Depth(TreeNode node)
    {
        var d = 0;
        for (var p = node.Parent; p is not null; p = p.Parent) d++;
        return d;
    }

    // The cell width of one guide part (e.g. "├── "); constant across parts for a given guide.
    private int GuidePartWidth() => scguide.GetSafeTreeGuide(safe: false).GetPart(TreeGuidePart.Continue).GetCellWidth();

    internal void Update() => this.Invalidate();

    // Report the rendered row count so a surrounding ControlFrame gets a real scroll range. Measured at a fixed wide
    // width, like ListBox and for the same reason: a width-dependent height feeds the layout's content-height↔width
    // convergence in a scrolling frame and can fail to settle. Long node labels clip to the viewport rather than
    // wrapping, which is what a tree wants anyway.
    /// <inheritdoc/>
    public virtual int MeasureHeight(int width)
    {
        var options = new RenderOptions(ansiConsole.Profile.Capabilities, new Spectre.Console.Size(LayoutWidth, 1));
        return Math.Max(1, Segment.SplitLines(Render(options, LayoutWidth)).Count);
    }

    /// <inheritdoc/>
    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var result = new List<Segment>();
        _rowMap.Clear();   // rebuilt below: content row -> owning node, for wrap-aware mouse hit-testing (see NodeAt)
        var visitedNodes = new HashSet<TreeNode>();

        var stack = new Stack<Queue<TreeNode>>();
        stack.Push(new Queue<TreeNode>(new[] { _root }));

        var levels = new List<Segment>();
        levels.Add(GetGuide(options, TreeGuidePart.Continue));

        while (stack.Count > 0)
        {
            var stackNode = stack.Pop();
            if (stackNode.Count == 0)
            {
                levels.RemoveLast();
                if (levels.Count > 0)
                {
                    levels.AddOrReplaceLast(GetGuide(options, TreeGuidePart.Fork));
                }

                continue;
            }

            var isLastChild = stackNode.Count == 1;
            var current = stackNode.Dequeue();
            if (!visitedNodes.Add(current))
            {
                throw new CircularTreeException("Cycle detected in tree - unable to render.");
            }

            stack.Push(stackNode);

            if (isLastChild)
            {
                levels.AddOrReplaceLast(GetGuide(options, TreeGuidePart.End));
            }

            var prefix = levels.GetRange(1, levels.Count - 1);   // a per-node mutable copy of the guide prefix (no LINQ iterator)
            
            // The gutter icon drawn before the label: a disclosure glyph for a parent (expanded/collapsed), or the
            // leaf glyph for a childless node. Each node may override the tree-wide glyph and colour for whichever it
            // shows — LeafGlyph/LeafGlyphColor for a leaf, ExpandedGlyph/CollapsedGlyph/DisclosureGlyphColor for a
            // parent; a null override falls back to the tree-wide value. Keep all glyphs the same cell width so labels
            // stay aligned. (A leaf's colour defaults to the theme leaf colour; a disclosure glyph's to the guide style.)
            var hasChildren = current.Nodes.Count > 0;
            var icon = hasChildren
                ? (current.Expanded ? current.ExpandedGlyph ?? _treeExpanded : current.CollapsedGlyph ?? _treeCollapsed)
                : current.LeafGlyph ?? _leafGlyph;
            var iconWidth = icon.GetCellWidth();
            var iconColor = hasChildren ? current.DisclosureGlyphColor : current.LeafGlyphColor ?? _leafGlyphColor;
            var iconStyle = iconColor is { } ic
                ? new Spectre.Console.Style(foreground: ic)
                : Style.SpectreConsoleStyle;

            var caret = _selectionStyle == SelectionStyle.Caret;
            var caretPad = caret ? _selectionCaret.GetCellWidth() : 0;
            var selStyle = _selectionStyle.TextStyle(_selectedForegroundColor, _selectedBackgroundColor);
            // A node with Text renders through Markup, so when it's selected (or hovered) we fold the icon INTO a
            // styled markup — the glyph is highlighted/tinted together with the text "in the usual manner". Selection
            // wins over hover. An IRenderable-only node can't be folded into markup, so a selected one is highlighted
            // by RESTYLING its already-rendered segments (below): the selection style is overlaid on each segment,
            // keeping the segment's own foreground so a colourful label stays colourful under the highlight.
            var isTextNode = !string.IsNullOrEmpty(current.Text);
            var hovered = !current.Selected && ReferenceEquals(current, _hoveredNode);
            var highlightRenderable = !isTextNode && current.Selected;

            IRenderable renderable = current.Renderable;
            var folded = false;
            if (isTextNode && current.Selected)
            {
                renderable = new Markup(_selectionStyle.Prefix(_selectionCaret) + Markup.Escape(icon) + current.Text, selStyle);
                folded = true;
            }
            else if (isTextNode && hovered)
            {
                // Tint the same region selection would (icon + label), keeping the caret gutter reserved in caret mode.
                renderable = new Markup(new string(' ', caretPad) + Markup.Escape(icon) + current.Text, _hoverStyle.SpectreConsoleStyle);
                folded = true;
            }

            var labelWidth = maxWidth - Segment.CellCount(prefix) - (folded ? 0 : iconWidth + caretPad);
            // Reuse the node's cached label render when it isn't folded and the width is unchanged. Selection/hover/
            // navigation repaints re-run Render for the whole tree, but a node's own label segments depend only on its
            // renderable and labelWidth — so this skips Spectre's Markup/Paragraph SplitLines for every unchanged node
            // (the bulk of Tree.Render's allocations). Folded nodes (selected/hovered text) build a fresh Markup and
            // always render; they're only ever 1-2 nodes.
            List<SegmentLine> renderableLines;
            if (!folded && current._cachedLabelWidth == labelWidth && current._cachedLabelLines is { } cachedLines)
            {
                renderableLines = cachedLines;
            }
            else
            {
                renderableLines = Segment.SplitLines(renderable.Render(options, labelWidth));
                if (!folded)
                {
                    current._cachedLabelLines = renderableLines;
                    current._cachedLabelWidth = labelWidth;
                }
            }

            // Indexed loop (not .Enumerate()): renderableLines is a List, and Enumerate() would box its enumerator and
            // allocate a yield state machine per node on every render (including cache hits). We only need isFirstLine.
            for (var lineIndex = 0; lineIndex < renderableLines.Count; lineIndex++)
            {
                var isFirstLine = lineIndex == 0;
                var line = renderableLines[lineIndex];
                _rowMap.Add((current, isFirstLine));   // this loop emits exactly one content row per iteration
                if (prefix.Count > 0)
                {
                    result.AddRange(prefix);   // AddRange copies now; prefix's later mutation doesn't affect result
                }

                // For a non-folded node, draw the caret reservation (caret mode) then the gutter icon — the glyph on
                // the first line, a width-matched spacer on wrapped continuation lines. A selected IRenderable row
                // highlights its gutter too: the caret glyph (caret mode) or the selection style over the icon/spacer.
                if (!folded)
                {
                    if (caretPad > 0)
                        result.Add(highlightRenderable
                            ? new Segment(isFirstLine ? _selectionCaret : new string(' ', caretPad), selStyle)
                            : new Segment(new string(' ', caretPad)));
                    if (isFirstLine)
                        result.Add(new Segment(icon, highlightRenderable ? selStyle : iconStyle));
                    else
                        result.Add(highlightRenderable
                            ? new Segment(new string(' ', iconWidth), selStyle)
                            : new Segment(new string(' ', iconWidth)));
                }

                // Overlay the selection style on a selected IRenderable's label segments (keeping each segment's own
                // foreground where set), so the label highlights like a text node's does.
                result.AddRange(highlightRenderable
                    ? line.Select(seg => seg.WithStyle(selStyle.Combine(seg.Style)))
                    : line);
                result.Add(Segment.LineBreak);

                if (isFirstLine && prefix.Count > 0)
                {
                    var part = isLastChild ? TreeGuidePart.Space : TreeGuidePart.Continue;
                    prefix.AddOrReplaceLast(GetGuide(options, part));
                }
            }

            if (current.Expanded && current.Nodes.Count > 0)
            {
                levels.AddOrReplaceLast(GetGuide(options, isLastChild ? TreeGuidePart.Space : TreeGuidePart.Continue));
                levels.Add(GetGuide(options, current.Nodes.Count == 1 ? TreeGuidePart.End : TreeGuidePart.Fork));

                stack.Push(new Queue<TreeNode>(current.Nodes.OrderBy(n => n.Index)));
            }
        }

        return result;
    }
    
    /// <summary>Maps a Jumbee <see cref="TreeGuide"/> to the corresponding Spectre.Console tree guide.</summary>
    protected static Spectre.Console.TreeGuide GetSpectreConsoleTreeGuide(TreeGuide guide) => guide switch
    {
        TreeGuide.Ascii => Spectre.Console.TreeGuide.Ascii,
        TreeGuide.Line => Spectre.Console.TreeGuide.Line,
        TreeGuide.BoldLine => Spectre.Console.TreeGuide.BoldLine,
        TreeGuide.DoubleLine => Spectre.Console.TreeGuide.DoubleLine,
        _ => Spectre.Console.TreeGuide.Line,
    };  

    private Segment GetGuide(RenderOptions options, TreeGuidePart part)
    {
        var guide = scguide.GetSafeTreeGuide(safe: !options.Unicode);
        // TreeGuide.None draws no connector lines: emit the blank Space part for every position so labels stay
        // aligned by indentation alone (scguide falls back to Line purely as the source of the blank's width).
        if (_guide == TreeGuide.None) part = TreeGuidePart.Space;
        return new Segment(guide.GetPart(part), Style);
    }

    // Select the visible node at a clamped flattened index (used by Home/End/PageUp/PageDown); does not wrap.
    private void SelectVisibleIndex(int index)
    {
        var nodes = Flatten(_root).ToList();
        if (nodes.Count == 0) return;
        index = Math.Clamp(index, 0, nodes.Count - 1);
        var current = nodes.FirstOrDefault(n => n.Selected);
        if (current is not null && !ReferenceEquals(current, nodes[index])) current.Selected = false;
        nodes[index].Selected = true;
        AutoScroll(nodes[index], index);
        RaiseSelectionChanged();
    }

    // The flattened (visible) index of the selected node, or 0 when nothing is selected.
    private int CurrentVisibleIndex()
    {
        var nodes = Flatten(_root).ToList();
        var current = nodes.FirstOrDefault(n => n.Selected);
        return current is null ? 0 : nodes.IndexOf(current);
    }

    // A page for PageUp/PageDown: the visible viewport when framed, else a sensible default.
    private int TreePage() => Math.Max(1, Frame?.ViewportSize.Height ?? 10);

    private void NavigateTree(int direction)
    {
        var nodes = Flatten(_root).ToList();
        if (nodes.Count == 0) return;

        var current = nodes.FirstOrDefault(n => n.Selected);
        int nextIndex;

        if (current == null)
        {
            nextIndex = 0;
        }
        else
        {
            var currentIndex = nodes.IndexOf(current);
            nextIndex = (currentIndex + direction + nodes.Count) % nodes.Count;
            current.Selected = false;
        }

        nodes[nextIndex].Selected = true;
        AutoScroll(nodes[nextIndex], nextIndex);
        RaiseSelectionChanged();
    }

    /// <summary>
    /// Moves the selection to <paramref name="node"/>, clearing the previous selection, scrolling it into view and
    /// raising <see cref="SelectionChanged"/>.
    /// </summary>
    /// <remarks>Does nothing if the node is not currently visible — it is under a collapsed ancestor, or it has been
    /// removed. Expand its ancestors first (see <see cref="SetExpanded"/>) when driving the tree from elsewhere,
    /// such as a search field or a path.</remarks>
    public void SelectNode(TreeNode node)
    {
        var nodes = Flatten(_root).ToList();
        var index = nodes.IndexOf(node);
        if (index < 0) return;   // not currently visible (e.g. under a collapsed ancestor)

        foreach (var n in nodes) if (n.Selected && !ReferenceEquals(n, node)) n.Selected = false;
        node.Selected = true;
        AutoScroll(node, index);
        RaiseSelectionChanged();
    }

    // Fires SelectionChanged when the highlighted node actually changed since the last raise, so every selection
    // path (arrows/vim, Home/End/Page, mouse) reports a move exactly once. Called at the end of each selection method.
    private void RaiseSelectionChanged()
    {
        var selected = SelectedNode;
        if (ReferenceEquals(selected, _lastSelection)) return;
        _lastSelection = selected;
        if (selected is not null) SelectionChanged?.Invoke(this, selected);
    }

    /// <summary>
    /// Scrolls the containing <see cref="ControlFrame"/> (if any) so <paramref name="node"/> stays within the
    /// viewport. Uses the node's actual rendered row span, so a wrapped (multi-row) node is kept fully in view;
    /// <paramref name="fallbackRow"/> (its flattened index) is used only before the first render populates the map.
    /// </summary>
    private void AutoScroll(TreeNode node, int fallbackRow)
    {
        var (start, height) = RowSpanOf(node);
        if (start < 0) { start = fallbackRow; height = 1; }
        FocusRowChanged?.Invoke(this, new RowSpan(start, height));
    }

    private IEnumerable<TreeNode> Flatten(TreeNode node)
    {
        yield return node;
        if (node.Expanded)
        {
            foreach (var child in node.Nodes.OrderBy(n => n.Index))
            {
                foreach (var descendant in Flatten(child))
                {
                    yield return descendant;
                }
            }
        }
    }
    #endregion

    #region Fields
    // The fixed wide width rows are measured at, so the content height is width-independent (see MeasureHeight).
    // Matches Control.CalculateSize's 1000-cell clamp, so nothing visible is lost to the cap.
    private const int LayoutWidth = 1000;
    /// <summary>The renderable used as the root node's label.</summary>
    public IRenderable _rootLabel;
    /// <summary>Backing field for <see cref="Root"/>.</summary>
    public TreeNode _root;
    /// <summary>Backing field for <see cref="Style"/>.</summary>
    protected Style _style;
    /// <summary>Backing field for <see cref="Guide"/>.</summary>
    protected TreeGuide _guide;
    /// <summary>The Spectre.Console guide derived from <see cref="_guide"/>, used when rendering guide lines.</summary>
    protected Spectre.Console.TreeGuide scguide;
    /// <summary>Backing field for <see cref="Expanded"/>.</summary>
    protected bool _expanded;
    private Color? _selectedForegroundColor;
    private Color? _selectedBackgroundColor;
    private SelectionStyle _selectionStyle;
    private string _selectionCaret = "";
    private string _treeExpanded = "";
    private string _treeCollapsed = "";
    private string _leafGlyph = "";
    private Color? _leafGlyphColor;
    private Style _hoverStyle;
    private bool _hoverHighlighting;
    private TreeNode? _hoveredNode;
    private TreeNode? _lastSelection;   // last node reported by SelectionChanged, so we only raise on an actual change
    // Content row -> (owning node, is that node's first/glyph row) for the last render. Lets the mouse handlers map a
    // clicked row to the correct node when labels wrap onto multiple rows. Written in Render, read in the mouse
    // handlers — both on the UI thread.
    private readonly List<(TreeNode node, bool first)> _rowMap = new();
    #endregion
}
