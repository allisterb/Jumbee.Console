# <a id="Jumbee_Console_FeedHandle"></a> Class FeedHandle

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

A handle to a running background feed started by <xref href="Jumbee.Console.Control.Feed(System.Action%2cSystem.Int32%2cSystem.Action%7bSystem.Exception%7d)" data-throw-if-not-resolved="false"></xref> and its overloads.
Cancel it to stop the feed; await <xref href="Jumbee.Console.FeedHandle.Completion" data-throw-if-not-resolved="false"></xref> (or <xref href="Jumbee.Console.FeedHandle.StopAsync" data-throw-if-not-resolved="false"></xref>) to know the in-flight tick
has finished — for safely disposing a resource the feed's producer reads.

```csharp
public sealed class FeedHandle
```

#### Inheritance

object ← 
[FeedHandle](Jumbee.Console.FeedHandle.md)

## Properties

### <a id="Jumbee_Console_FeedHandle_Completion"></a> Completion

Completes when the feed's loop has fully stopped — the in-flight tick has finished running. Await this
    after <xref href="Jumbee.Console.FeedHandle.Cancel" data-throw-if-not-resolved="false"></xref> (or use <xref href="Jumbee.Console.FeedHandle.StopAsync" data-throw-if-not-resolved="false"></xref>) before disposing anything the producer touches, so
    the resource is never torn down under a live tick.

```csharp
public Task Completion { get; }
```

#### Property Value

 Task

## Methods

### <a id="Jumbee_Console_FeedHandle_Cancel"></a> Cancel\(\)

Requests the feed to stop. Idempotent; does not wait — await <xref href="Jumbee.Console.FeedHandle.Completion" data-throw-if-not-resolved="false"></xref> to join.

```csharp
public void Cancel()
```

### <a id="Jumbee_Console_FeedHandle_Dispose"></a> Dispose\(\)

Stops the feed (same as <xref href="Jumbee.Console.FeedHandle.Cancel" data-throw-if-not-resolved="false"></xref>). Disposing the control that started it cancels it too.

```csharp
public void Dispose()
```

### <a id="Jumbee_Console_FeedHandle_StopAsync"></a> StopAsync\(\)

Stops the feed and returns a task that completes once the in-flight tick has finished.

```csharp
public Task StopAsync()
```

#### Returns

 Task

