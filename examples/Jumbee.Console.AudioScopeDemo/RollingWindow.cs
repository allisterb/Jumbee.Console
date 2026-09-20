namespace ScopeTui;

using System;

/// <summary>
/// The most recent N interleaved samples: a producer rolls new audio in, a consumer takes a snapshot when there is
/// something new. Deliberately latest-wins, not a queue — a scope wants the current audio, and a consumer that falls
/// behind should skip ahead rather than accumulate a backlog it will never catch up on.
/// </summary>
/// <remarks>
/// Shared by every PUSH source: a capture device (<see cref="RecordingAudioSource"/>) and a file on its way to the
/// speakers (<see cref="PlayingFileAudioSource"/>). Both are fed from an audio callback thread and read from the
/// pump thread, which is what the lock is for; it is held only for a copy.
/// </remarks>
internal sealed class RollingWindow
{
    #region Constructors
    /// <param name="samples">Window size in interleaved samples (per-channel size × channels).</param>
    public RollingWindow(int samples) => _buffer = new float[samples];
    #endregion

    #region Methods
    /// <summary>Rolls freshly produced audio in, keeping only the newest window's worth.</summary>
    public void Write(ReadOnlySpan<float> samples)
    {
        if (samples.IsEmpty) return;

        lock (_gate)
        {
            var n = samples.Length;
            if (n >= _buffer.Length)
            {
                samples[^_buffer.Length..].CopyTo(_buffer);              // bigger than a window: keep its tail
            }
            else
            {
                Array.Copy(_buffer, n, _buffer, 0, _buffer.Length - n);  // shift older samples left...
                samples.CopyTo(_buffer.AsSpan(_buffer.Length - n));      // ...and append the newest at the tail
            }

            _writes++;
        }
    }

    /// <summary>
    /// A copy of the window, or <see langword="null"/> when nothing has been written since the last call.
    /// </summary>
    /// <remarks>
    /// The null is the point: a consumer polling faster than the producer pushes would otherwise clone and
    /// de-interleave a window it has already seen, for every pane, on every tick. Answering "nothing new" costs a
    /// comparison. The first call always returns a snapshot, so a consumer has something to draw before any audio
    /// has arrived.
    /// </remarks>
    public float[]? TrySnapshot()
    {
        lock (_gate)
        {
            if (_writes == _lastSeen) return null;
            _lastSeen = _writes;
            return (float[])_buffer.Clone();
        }
    }
    #endregion

    #region Fields
    private readonly float[] _buffer;
    private readonly object _gate = new();
    private long _writes;
    // Below _writes so the first TrySnapshot always yields, even from a producer that has pushed nothing yet.
    private long _lastSeen = -1;
    #endregion
}
