namespace DTC.Core;

/// <summary>
/// Stand-in for the DTC.Core logger the vendored Wolfenshine files call into, so they stay byte-identical to
/// upstream. Messages go nowhere: the demo has a UI on stdout, and a stray Console.WriteLine corrupts it.
/// </summary>
internal sealed class Logger
{
    #region Properties
    public static Logger Instance { get; } = new();
    #endregion

    #region Methods
    public void Info(string message) { }

    public void Warn(string message) { }

    public void Error(string message) { }
    #endregion
}
