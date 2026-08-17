# <a id="Jumbee_Console_RowSpan"></a> Struct RowSpan

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

A run of content rows: <code class="paramref">Start</code> and the <code class="paramref">Height</code> rows following it.

```csharp
public readonly record struct RowSpan
```

## Constructors

### <a id="Jumbee_Console_RowSpan__ctor_System_Int32_System_Int32_"></a> RowSpan\(int, int\)

A run of content rows: <code class="paramref">Start</code> and the <code class="paramref">Height</code> rows following it.

```csharp
public RowSpan(int Start, int Height = 1)
```

#### Parameters

`Start` int

The first row, in the control's own content coordinates.

`Height` int

How many rows the run covers. Values below 1 are treated as 1.

## Properties

### <a id="Jumbee_Console_RowSpan_Height"></a> Height

How many rows the run covers. Values below 1 are treated as 1.

```csharp
public int Height { get; init; }
```

#### Property Value

 int

### <a id="Jumbee_Console_RowSpan_Start"></a> Start

The first row, in the control's own content coordinates.

```csharp
public int Start { get; init; }
```

#### Property Value

 int

