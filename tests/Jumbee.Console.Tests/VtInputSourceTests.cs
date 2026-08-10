namespace Jumbee.Console.Tests;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Jumbee.Console;

using Xunit;

/// <summary>
/// The reader thread's behaviour when a read <em>fails</em> — the path a real terminal takes only rarely (a console
/// resize with a read outstanding has been seen to surface <c>ERROR_PIPE_NOT_CONNECTED</c> on Windows) but which
/// used to take the whole thread down with it.
/// </summary>
public class VtInputSourceTests
{
    [Fact]
    public void AFailedReadDoesNotKillTheReaderThread()
    {
        // Regression: the loop caught exceptions around `read.Result` but waited with `read.Wait(timeout)`, and Wait
        // rethrows a faulted task's exception too — so the failure escaped before the catch and killed the thread,
        // surfacing as an unhandled AggregateException and leaving the app with no keyboard.
        var stdin = new ScriptedStream();
        using var source = new VtInputSource(stdin, idleFlushMs: 5);

        stdin.FailNextRead(new IOException("No process is on the other end of the pipe."));
        stdin.WaitUntilConsumed();

        stdin.Provide("A"u8.ToArray());
        Assert.True(TryReadWithin(source, TimeSpan.FromSeconds(5), out var evt),
            "no input arrived after a failed read — the reader thread did not survive it");
        Assert.Equal('A', Assert.IsType<KeyInputEvent>(evt).KeyChar);
    }

    [Fact]
    public void RepeatedFailuresStillRecoverWhenInputComesBack()
    {
        // A permanently-broken stdin must not spin a core, and must not permanently give up either: the thread stays
        // alive on a backoff so input resumes if the console does.
        var stdin = new ScriptedStream();
        using var source = new VtInputSource(stdin, idleFlushMs: 5);

        for (var i = 0; i < 6; i++)
        {
            stdin.FailNextRead(new IOException("No process is on the other end of the pipe."));
            stdin.WaitUntilConsumed();
        }

        stdin.Provide("B"u8.ToArray());
        Assert.True(TryReadWithin(source, TimeSpan.FromSeconds(5), out var evt),
            "input never resumed after repeated read failures");
        Assert.Equal('B', Assert.IsType<KeyInputEvent>(evt).KeyChar);
    }

    [Fact]
    public void DisposeStopsTheReaderEvenWhileReadsAreFailing()
    {
        var stdin = new ScriptedStream();
        var source = new VtInputSource(stdin, idleFlushMs: 5);
        stdin.FailNextRead(new IOException("No process is on the other end of the pipe."));
        stdin.WaitUntilConsumed();

        source.Dispose();
        Assert.True(stdin.WaitUntilNoReadOutstanding(TimeSpan.FromSeconds(5)),
            "the reader thread was still issuing reads after Dispose");
    }

    private static bool TryReadWithin(VtInputSource source, TimeSpan timeout, out TerminalInputEvent? evt)
    {
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed < timeout)
        {
            if (source.TryRead(out evt)) return true;
            Thread.Sleep(5);
        }

        evt = null;
        return false;
    }

    /// <summary>A stdin stand-in whose reads complete only when the test says so — with data, or with a failure.</summary>
    private sealed class ScriptedStream : Stream
    {
        public void FailNextRead(Exception ex) => Post(ex);

        public void Provide(byte[] data) => Post(data);

        /// <summary>Blocks until the reader thread has taken whatever was posted.</summary>
        public void WaitUntilConsumed()
        {
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < TimeSpan.FromSeconds(5))
            {
                lock (_gate)
                {
                    if (_pending is null) return;
                }

                Thread.Sleep(2);
            }

            throw new TimeoutException("the reader never consumed the scripted item");
        }

        /// <summary>Waits for the reader to stop issuing new reads (i.e. the loop has exited).</summary>
        public bool WaitUntilNoReadOutstanding(TimeSpan timeout)
        {
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < timeout)
            {
                lock (_gate) { _reads = 0; }
                Thread.Sleep(400);   // longer than the loop's failure backoff
                lock (_gate) { if (_reads == 0) return true; }
            }

            return false;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            lock (_gate) _reads++;

            // Mirrors a real console read: the call does not complete until something arrives. The scripted item is
            // either bytes or the failure to raise, so the reader loop sees exactly what a broken pipe gives it.
            return Task.Run(() =>
            {
                while (true)
                {
                    lock (_gate)
                    {
                        if (_pending is Exception ex) { _pending = null; throw ex; }
                        if (_pending is byte[] data)
                        {
                            _pending = null;
                            var n = Math.Min(count, data.Length);
                            Array.Copy(data, 0, buffer, offset, n);
                            return n;
                        }
                    }

                    Thread.Sleep(2);
                }
            }, cancellationToken);
        }

        private void Post(object item)
        {
            lock (_gate) _pending = item;
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => ReadAsync(buffer, offset, count, default).GetAwaiter().GetResult();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        private readonly object _gate = new();
        private object? _pending;
        private int _reads;
    }
}
