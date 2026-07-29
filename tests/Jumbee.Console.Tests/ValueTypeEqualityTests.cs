namespace Jumbee.Console.Tests;

using System;
using System.Collections.Generic;

using ConsoleGUI.Data;
using ConsoleGUI.Space;

using Jumbee.Console;

using Xunit;

using CColor = ConsoleGUI.Data.Color;
using JColor = Jumbee.Console.Color;

/// <summary>
/// Every value type that flows through a property setter or a collection owes callers value equality, and owes it
/// via <see cref="IEquatable{T}"/> specifically.
/// <para>
/// Without <see cref="IEquatable{T}"/> the runtime picks <c>ObjectEqualityComparer</c> for
/// <see cref="EqualityComparer{T}.Default"/> — which <c>Control.SetAtomicProperty</c> uses on every assignment. That
/// comparer boxes both operands, and for a struct holding a reference (any of the style tokens below hold a
/// <see cref="Style"/>) it compares field-by-field <em>reflectively</em>: ButtonStyle cost 368ns and 672 bytes per
/// comparison before this, against 8.8ns and nothing now. Nothing fails when it regresses — it just quietly
/// allocates again — so the allocation assertion is the real test here.
/// </para>
/// </summary>
public class ValueTypeEqualityTests
{
    // (name, two equal values, one different value)
    public static TheoryData<string> Types => new()
    {
        "ButtonStyle", "GaugeStyle", "ProgressBarStyle", "ProgressBarGlyphs", "ScrollBarStyle", "ScrollBarGlyphs",
        "TitleStyle", "JColor", "Size", "Position", "Rect", "Offset", "Vector", "CColor", "Character", "PlotPalette",
    };

    [Theory]
    [MemberData(nameof(Types))]
    public void BehavesAsAValueType(string type)
    {
        switch (type)
        {
            case "ButtonStyle": Check(ButtonStyle.Primary, ButtonStyle.Primary, ButtonStyle.Secondary); break;
            case "GaugeStyle": Check(GaugeStyle.Default, GaugeStyle.Default, GaugeStyle.Default with { Text = Style.White }); break;
            case "ProgressBarStyle": Check(ProgressBarStyle.Default, ProgressBarStyle.Default, ProgressBarStyle.Default with { Time = Style.White }); break;
            case "ProgressBarGlyphs": Check(ProgressBarGlyphs.Hatched, ProgressBarGlyphs.Hatched, ProgressBarGlyphs.Segmented); break;
            case "ScrollBarStyle": Check(ScrollBarStyle.Default, ScrollBarStyle.Default, ScrollBarStyle.Default with { Thumb = Style.White }); break;
            case "ScrollBarGlyphs": Check(ScrollBarGlyphs.Classic, ScrollBarGlyphs.Classic, ScrollBarGlyphs.Smooth); break;
            case "TitleStyle": Check(new TitleStyle(TitlePos.TopLeft), new TitleStyle(TitlePos.TopLeft), new TitleStyle(TitlePos.TopRight)); break;
            case "JColor": Check(new JColor(1, 2, 3), new JColor(1, 2, 3), new JColor(1, 2, 4)); break;
            case "Size": Check(new Size(80, 24), new Size(80, 24), new Size(80, 25)); break;
            case "Position": Check(new Position(1, 2), new Position(1, 2), new Position(1, 3)); break;
            case "Rect": Check(new Rect(0, 0, 10, 10), new Rect(0, 0, 10, 10), new Rect(0, 0, 10, 11)); break;
            case "Offset": Check(new Offset(1, 1, 1, 1), new Offset(1, 1, 1, 1), new Offset(1, 1, 1, 2)); break;
            case "Vector": Check(new Vector(1, 2), new Vector(1, 2), new Vector(1, 3)); break;
            case "CColor": Check(new CColor(1, 2, 3), new CColor(1, 2, 3), new CColor(1, 2, 4)); break;
            case "Character": Check(new Character('a'), new Character('a'), new Character('b')); break;
            // Two SEPARATELY-constructed palettes with the same colours: the point of the type is that these are
            // equal, where the bare arrays behind them would not be.
            case "PlotPalette":
                Check(new PlotPalette([new JColor(1, 2, 3), new JColor(4, 5, 6)]),
                      new PlotPalette([new JColor(1, 2, 3), new JColor(4, 5, 6)]),
                      new PlotPalette([new JColor(1, 2, 3), new JColor(4, 5, 7)]));
                break;
            default: throw new ArgumentOutOfRangeException(nameof(type), type, "unmapped");
        }
    }

    private static void Check<T>(T a, T sameAsA, T different) where T : IEquatable<T>
    {
        Assert.True(typeof(IEquatable<T>).IsAssignableFrom(typeof(T)), $"{typeof(T).Name} must implement IEquatable<T>");

        Assert.True(a.Equals(sameAsA));
        Assert.False(a.Equals(different));
        Assert.True(a.Equals((object?)sameAsA));
        Assert.False(a.Equals((object?)different));
        Assert.False(a.Equals(null));
        Assert.Equal(a.GetHashCode(), sameAsA!.GetHashCode());

        // Equal values must be interchangeable as dictionary keys.
        var map = new Dictionary<T, int> { [a] = 1 };
        map[sameAsA] = 2;
        Assert.Single(map);
        Assert.Equal(2, map[a]);

        // The guard that matters: generic comparison must not box PER COMPARISON. This is what SetAtomicProperty runs.
        // Threshold rather than zero: an unrelated one-off (JIT tiering promoting the loop) can charge a few dozen
        // bytes to this thread. Boxing would allocate at least 24 bytes every iteration — 240KB+ here, and up to
        // 3.4MB for the reflective path — so anything under 1KB is unambiguously "not boxing".
        const int comparisons = 10_000;
        var comparer = EqualityComparer<T>.Default;
        for (var i = 0; i < 2_000; i++) comparer.Equals(a, different);   // warm up

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < comparisons; i++) comparer.Equals(a, different);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.True(allocated < 1024,
            $"{typeof(T).Name} allocated {allocated} bytes over {comparisons} comparisons — it is boxing, so it has " +
            $"lost IEquatable<{typeof(T).Name}> (EqualityComparer<T>.Default fell back to {comparer.GetType().Name})");
    }

    // The operators exist and agree with Equals (several of these types had no == at all, so this would not compile).
    [Fact]
    public void OperatorsAgreeWithEquals()
    {
        Assert.True(new JColor(1, 2, 3) == new JColor(1, 2, 3));
        Assert.True(new JColor(1, 2, 3) != new JColor(3, 2, 1));
        Assert.True(ButtonStyle.Primary == ButtonStyle.Primary);
        Assert.True(ButtonStyle.Primary != ButtonStyle.Secondary);
        Assert.True(new TitleStyle(TitlePos.TopLeft) == new TitleStyle(TitlePos.TopLeft));
        Assert.True(ScrollBarGlyphs.Classic != ScrollBarGlyphs.Smooth);
        Assert.True(GaugeStyle.Default == GaugeStyle.Default);
        Assert.True(new Rect(0, 0, 4, 4) == new Rect(0, 0, 4, 4));
        Assert.True(new Rect(0, 0, 4, 4) != new Rect(0, 0, 4, 5));
        Assert.True(new Offset(1, 2, 3, 4) == new Offset(1, 2, 3, 4));
        Assert.True(new Offset(1, 2, 3, 4) != new Offset(1, 2, 3, 5));
    }

    // ScrollBarGlyphs holds strings; equality must be by value (and ordinal — these are glyphs, not prose).
    [Fact]
    public void ScrollBarGlyphs_CompareByValue_NotReference()
    {
        var a = new ScrollBarGlyphs("#", "|", "^", "v");
        var b = new ScrollBarGlyphs(new string('#', 1), new string('|', 1), new string('^', 1), new string('v', 1));

        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }
}
