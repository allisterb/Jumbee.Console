# <a id="Jumbee_Console_JobHandle"></a> Class JobHandle

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

A handle to a background job started by <code>Control.Job</code>. Call <xref href="Jumbee.Console.JobHandle.Request" data-throw-if-not-resolved="false"></xref> to ask for a run; cancel
it to stop the job.

```csharp
public sealed class JobHandle
```

#### Inheritance

object ← 
[JobHandle](Jumbee.Console.JobHandle.md)

## Remarks

The counterpart to <xref href="Jumbee.Console.FeedHandle" data-throw-if-not-resolved="false"></xref>: a feed runs on a timer, a job runs when something asks it to — see
<xref href="Jumbee.Console.Control.Job%60%601(System.Func%7b%60%600%7d%2cSystem.Action%7b%60%600%7d%2cSystem.Action%7bSystem.Exception%7d)" data-throw-if-not-resolved="false"></xref>. Await <xref href="Jumbee.Console.JobHandle.Completion" data-throw-if-not-resolved="false"></xref> (or
<xref href="Jumbee.Console.JobHandle.StopAsync" data-throw-if-not-resolved="false"></xref>) after cancelling to know the in-flight run has finished, before disposing anything the
producer reads.

## Properties

### <a id="Jumbee_Console_JobHandle_Coalesced"></a> Coalesced

How many <xref href="Jumbee.Console.JobHandle.Request" data-throw-if-not-resolved="false"></xref> calls were absorbed into an already-pending run rather than causing one of
their own.

```csharp
public long Coalesced { get; }
```

#### Property Value

 long

#### Remarks

The measure of how far the producer is behind its callers: a steadily climbing count means requests arrive
faster than the job can serve them, which is the job doing its job — the alternative is an unbounded queue
of stale work.

### <a id="Jumbee_Console_JobHandle_Completed"></a> Completed

How many runs have completed. Diagnostics, and what a test waits on.

```csharp
public long Completed { get; }
```

#### Property Value

 long

### <a id="Jumbee_Console_JobHandle_Completion"></a> Completion

Completes when the job's loop has fully stopped and the in-flight run has finished. Await this after
    <xref href="Jumbee.Console.JobHandle.Cancel" data-throw-if-not-resolved="false"></xref> (or use <xref href="Jumbee.Console.JobHandle.StopAsync" data-throw-if-not-resolved="false"></xref>) before tearing down anything the producer touches.

```csharp
public Task Completion { get; }
```

#### Property Value

 Task

## Methods

### <a id="Jumbee_Console_JobHandle_Cancel"></a> Cancel\(\)

Requests the job to stop. Idempotent; does not wait — await <xref href="Jumbee.Console.JobHandle.Completion" data-throw-if-not-resolved="false"></xref> to join.

```csharp
public void Cancel()
```

### <a id="Jumbee_Console_JobHandle_Dispose"></a> Dispose\(\)

Stops the job (same as <xref href="Jumbee.Console.JobHandle.Cancel" data-throw-if-not-resolved="false"></xref>). Disposing the control that started it cancels it too.

```csharp
public void Dispose()
```

### <a id="Jumbee_Console_JobHandle_Request"></a> Request\(\)

Asks for a run. Returns immediately.

```csharp
public void Request()
```

#### Remarks

<b>At most one run is ever in flight, and at most one more is ever queued.</b> Calling this a hundred times
while a run is going produces exactly one further run, not a hundred — the requests collapse (see
<xref href="Jumbee.Console.JobHandle.Coalesced" data-throw-if-not-resolved="false"></xref>). That is the whole point for a render queue: what a caller wants is "the newest
state on screen soon", never "every intermediate state, eventually".

### <a id="Jumbee_Console_JobHandle_StopAsync"></a> StopAsync\(\)

Stops the job and returns a task that completes once the in-flight run has finished.

```csharp
public Task StopAsync()
```

#### Returns

 Task

