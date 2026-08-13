namespace Jumbee.Console.Examples;

using System.Collections.Generic;

/// <summary>
/// The explicit registry of examples, in display order. Explicit registration (rather than reflection) keeps
/// <c>PublishAot</c> happy and makes "add an example" a one-line change here plus one small class.
/// </summary>
public static class ExampleCatalog
{
    public static IReadOnlyList<IExample> All { get; } =
    [
        new WelcomeExample(),
        new BorderShowcaseExample(),
        new ButtonExample(),
        new ProgressBarExample(),
        new FormControlsExample(),
        new SliderExample(),
        new FileBrowserExample(),
        new ListBoxExample(),
        new PlotExample(),
        new CanvasExample(),
        new CanvasMarkersExample(),
        new WorldMapExample(),
        new ScatterPlotExample(),
        new StemPlotExample(),
        new BarPlotExample(),
        new GroupedBarExample(),
        new StackedBarExample(),
        new HorizontalBarExample(),
        new HistogramExample(),
        new CandlestickExample(),
        new HeatmapExample(),
        new ConfusionMatrixExample(),
        new BoxPlotExample(),
        new ErrorBarExample(),
        new AnnotatedPlotExample(),
        new LiveDashboardExample(),
        new MonitorDashboardExample(),
        new GlobeExample(),
        new InteractiveGlobeExample(),
        new BouncingBallsExample(),
        new DialogExample(),
        new LayoutGalleryExample(),
        new SplitPanelExample(),
        new SpectreRenderingExample(),
        new SpectreLiveExample(),
        new MultiTabEditorExample(),
        new MarkdownViewerExample(),
        new MarkdownExtendedViewerExample(),
        new InteractiveMarkdownEditorExample(),
        new AsciiDocViewerExample(),
        new MermaidViewerExample(),
        new MermaidClassExample(),
        new MermaidErExample(),
        new MermaidSequenceExample(),
        new InteractiveMermaidEditorExample(),
        new InteractiveAsciiDocEditorExample(),
        new InteractiveMarkdownExtendedEditorExample(),
    ];

    /// <summary>The example shown when the browser opens.</summary>
    public static IExample Default => All[0];
}
