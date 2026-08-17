# <a id="Jumbee_Console_IScrollable"></a> Interface IScrollable

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

A control whose content can be taller than the space it is given, and which therefore wants its
<xref href="Jumbee.Console.ControlFrame" data-throw-if-not-resolved="false"></xref> to scroll it.

```csharp
public interface IScrollable
```

## Remarks

<p>
Scrolling is the frame's job: it owns the viewport, the scrollbar and the offset. A frame gives an
<xref href="Jumbee.Console.IScrollable" data-throw-if-not-resolved="false"></xref> child an <b>unbounded</b> height so the content can grow past the visible area, reserves
a column for the scrollbar, and moves a window over the result. A control that does <b>not</b> implement this
interface is given the bounded viewport height and is never scrolled — that is the default, and it is the right
one for controls that fit their space or manage their own viewport (<xref href="Jumbee.Console.Log" data-throw-if-not-resolved="false"></xref>, <xref href="Jumbee.Console.DataTable" data-throw-if-not-resolved="false"></xref>,
<xref href="Jumbee.Console.TerminalEmulator" data-throw-if-not-resolved="false"></xref>).
</p>
<p>
Implementing it costs one method, and that method is the whole contract: report the content height and the
frame's scroll range and scrollbar follow from it. When that height changes, re-lay-out with
<code>Initialize()</code> — not merely <code>Invalidate()</code>, which only repaints — so the frame re-measures.
</p>

## Methods

### <a id="Jumbee_Console_IScrollable_MeasureHeight_System_Int32_"></a> MeasureHeight\(int\)

The control's content height in rows at the given <code class="paramref">width</code> — the frame's scroll range.

```csharp
int MeasureHeight(int width)
```

#### Parameters

`width` int

#### Returns

 int

#### Remarks

Measure the content, not the viewport: a list returns its item count, a text control its wrapped row count.
Returning the visible height instead defeats the purpose, leaving a scrollbar that never moves.

### <a id="Jumbee_Console_IScrollable_FocusRowChanged"></a> FocusRowChanged

Raised when the control's point of interest — a selected item, a caret — moves, carrying the content rows it
now occupies. The wrapping <xref href="Jumbee.Console.ControlFrame" data-throw-if-not-resolved="false"></xref> subscribes and scrolls them into view.

```csharp
event EventHandler<RowSpan>? FocusRowChanged
```

#### Event Type

 EventHandler<[RowSpan](Jumbee.Console.RowSpan.md)\>?

#### Remarks

<p>
Declare it as a plain field-like event and raise it wherever the selection moves:

<pre><code class="lang-csharp">public event EventHandler&lt;RowSpan&gt;? FocusRowChanged;
private void Select(int i) { _index = i; FocusRowChanged?.Invoke(this, new RowSpan(RowOf(i))); }</code></pre>

Written that way the compiler reports <code>CS0067</code> if it is never raised, which is the mistake worth
catching: a control that says it has a moving selection and then leaves it to scroll off screen.
</p>
<p>
The default implementation does nothing, so a control with no moving point of interest — a document viewer,
a panel of static text — simply omits it. Scrolls that are not selection moves (following new output to the
bottom, restoring a saved position) are not what this is for; call
<xref href="Jumbee.Console.ControlFrame.ScrollIntoView(System.Int32%2cSystem.Int32)" data-throw-if-not-resolved="false"></xref> directly instead.
</p>

