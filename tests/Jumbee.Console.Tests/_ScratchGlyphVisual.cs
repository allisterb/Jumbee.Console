namespace Jumbee.Console.Tests;

using System.IO;

using Jumbee.Console.Snapshot;

using Xunit;

// TEMP visual capture — deleted after eyeballing.
public class _ScratchGlyphVisual
{
    [Fact]
    public void Dump()
    {
        var rows = new (string, ProgressBarGlyphs)[]
        {
            ("Solid (default)", ProgressBarGlyphs.Solid),
            ("Hatched", ProgressBarGlyphs.Hatched),
            ("Shaded", ProgressBarGlyphs.Shaded),
            ("Segmented", ProgressBarGlyphs.Segmented),
            ("Ascii", ProgressBarGlyphs.Ascii),
        };
        var sw = new StringWriter();
        foreach (var (name, g) in rows)
        {
            var pb = new ProgressBar(value: 62) { ShowPercentage = true }.WithGlyphs(g);
            sw.WriteLine($"[{name,-16}] |" + ConsoleSnapshot.ToText(pb, 40, 1).TrimEnd('\n') + "|");
        }
        File.WriteAllText(Path.Combine(Path.GetTempPath(), "pb-glyphs.txt"), sw.ToString());
    }
}
