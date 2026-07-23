# <a id="Jumbee_Console_PlotSeries"></a> Class PlotSeries

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

A live, updatable series in a <xref href="Jumbee.Console.Plot" data-throw-if-not-resolved="false"></xref>. Returned by <xref href="Jumbee.Console.Plot.AddLiveSeries(System.Nullable%7bJumbee.Console.Color%7d%2cJumbee.Console.PlotBrush)" data-throw-if-not-resolved="false"></xref> (line),
<xref href="Jumbee.Console.Plot.AddLiveScatter(System.Nullable%7bJumbee.Console.Color%7d%2cJumbee.Console.PlotBrush)" data-throw-if-not-resolved="false"></xref> (markers) or <xref href="Jumbee.Console.Plot.AddLiveBars(System.Nullable%7bJumbee.Console.Color%7d%2cSystem.Double%2cSystem.Double)" data-throw-if-not-resolved="false"></xref>; hold onto it and feed data as it
arrives with <xref href="Jumbee.Console.PlotSeries.SetData(System.Double%5b%5d%2cSystem.Double%5b%5d)" data-throw-if-not-resolved="false"></xref>, <xref href="Jumbee.Console.PlotSeries.SetValues(System.Double%5b%5d)" data-throw-if-not-resolved="false"></xref>, <xref href="Jumbee.Console.PlotSeries.Push(System.Double%2cSystem.Double%2cSystem.Int32)" data-throw-if-not-resolved="false"></xref> or <xref href="Jumbee.Console.PlotSeries.Clear" data-throw-if-not-resolved="false"></xref>.

```csharp
public sealed class PlotSeries
```

#### Inheritance

object ← 
[PlotSeries](Jumbee.Console.PlotSeries.md)

## Remarks

Every update is marshalled onto the UI thread and re-draws the plot, so the methods are safe to call from any
data-source thread.

## Methods

### <a id="Jumbee_Console_PlotSeries_Clear"></a> Clear\(\)

Removes all points from the series.

```csharp
public void Clear()
```

### <a id="Jumbee_Console_PlotSeries_Push_System_Double_System_Double_System_Int32_"></a> Push\(double, double, int\)

Appends a point, keeping at most <code class="paramref">maxPoints</code> (0 = unbounded) by dropping the oldest — a
rolling window for streaming time-series.

```csharp
public void Push(double x, double y, int maxPoints = 0)
```

#### Parameters

`x` double

`y` double

`maxPoints` int

### <a id="Jumbee_Console_PlotSeries_Scroll_System_Double_System_Int32_"></a> Scroll\(double, int\)

Scrolls a new value into a fixed-width strip chart: the series keeps the last <code class="paramref">window</code> values
at fixed x positions 0..window−1, so the data flows through a <b>stationary</b> x axis (a real-time monitor).

```csharp
public void Scroll(double value, int window)
```

#### Parameters

`value` double

`window` int

#### Remarks

Unlike <xref href="Jumbee.Console.PlotSeries.Push(System.Double%2cSystem.Double%2cSystem.Int32)" data-throw-if-not-resolved="false"></xref> there is no ever-growing x — pair with <code>SetXRange(0, window − 1)</code> to pin the axis.

### <a id="Jumbee_Console_PlotSeries_SetData_System_Double___System_Double___"></a> SetData\(double\[\], double\[\]\)

Replaces the series data with the paired <code class="paramref">xs</code>/<code class="paramref">ys</code> (same length).
    Passing empty lists is valid and draws nothing — a live series can be emptied (equivalently, <xref href="Jumbee.Console.PlotSeries.Clear" data-throw-if-not-resolved="false"></xref>)
    and refilled without removing/re-adding it, so a series that appears only under some state (a trigger marker,
    peaks) can be toggled by feeding it data or emptying it.

```csharp
public void SetData(double[] xs, double[] ys)
```

#### Parameters

`xs` double\[\]

`ys` double\[\]

### <a id="Jumbee_Console_PlotSeries_SetValues_System_Double___"></a> SetValues\(double\[\]\)

Replaces the series with <code class="paramref">values</code> at implicit x positions 1, 2, 3, … — for bars.

```csharp
public void SetValues(double[] values)
```

#### Parameters

`values` double\[\]

