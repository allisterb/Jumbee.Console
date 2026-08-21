namespace ScopeTui;

/// <summary>
/// A generated stereo tone, used by <c>--verify</c> when the bundled sample track is absent.
/// </summary>
/// <remarks>
/// The default track is not redistributable and so is not in git — an image or a checkout without it is the normal
/// case, not a broken one. A check that failed there would either break every build that skipped the download or,
/// worse, be switched off. This keeps the check meaningful in both: with the track present <c>--verify</c> decodes
/// it and exercises the real MP3 path, and without it the scopes still get audio with genuine amplitude and a
/// genuine spectrum, which is what the three panes are being asked to draw.
/// <para>
/// Two channels at different frequencies deliberately: a mono-identical pair would draw the vectorscope as a
/// diagonal line, which is a degenerate picture that would hide a bug in its plotting.
/// </para>
/// </remarks>
internal sealed class VerifyToneSource : IAudioSource
{
    #region Constructors
    public VerifyToneSource(int samplesPerChannel) => this.samplesPerChannel = samplesPerChannel;
    #endregion

    #region Properties
    public int Channels => 2;

    public int SampleRate => 48_000;
    #endregion

    #region Methods
    public double[][] NextFrame()
    {
        var frame = new double[2][];
        for (var c = 0; c < 2; c++)
        {
            var samples = new double[samplesPerChannel];
            // 440 Hz left, 660 Hz right -- a fifth apart, so the two are neither identical nor harmonically aliased
            // onto the same FFT bins.
            var step = 2.0 * Math.PI * (c == 0 ? 440.0 : 660.0) / SampleRate;
            for (var i = 0; i < samples.Length; i++) samples[i] = 0.6 * Math.Sin((position + i) * step);
            frame[c] = samples;
        }

        position += samplesPerChannel;
        return frame;
    }

    public void SetOverlap(double overlap) { }

    public void Dispose() { }
    #endregion

    #region Fields
    private readonly int samplesPerChannel;
    private long position;
    #endregion
}
