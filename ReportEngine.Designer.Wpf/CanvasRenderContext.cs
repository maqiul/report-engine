using System.Collections.Generic;
using System.Windows.Media;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 设计态画布渲染上下文。把渲染所需的输入打包，方便 CanvasRenderer / 单测使用。
/// 字段命名沿用 MainWindow 私有字段语义，便于一对一映射。
/// 注意：用 class 而非 record，因为 net462 / netstandard2.0 不支持 record（IsExternalInit 缺失）。
/// </summary>
internal sealed class CanvasRenderContext
{
    /// <summary>毫米 → 设备像素（96 DPI）。</summary>
    public const double PixelsPerMm = 96.0 / 25.4;

    /// <summary>画布四周留白（像素）。与 MainWindow 原 CanvasPadding 常量保持一致，避免拆分时偏移。</summary>
    public const double CanvasPadding = 30.0;

    public ReportTemplate Template { get; }
    public double Zoom { get; }
    public double GridSpacingMm { get; }
    public bool ShowGrid { get; }
    public bool ShowMargins { get; }
    public Color GridColor { get; }
    public IReadOnlyList<double> VerticalGuides { get; }
    public IReadOnlyList<double> HorizontalGuides { get; }
    public IReadOnlyList<double> SnapLinesX { get; }
    public IReadOnlyList<double> SnapLinesY { get; }

    public CanvasRenderContext(
        ReportTemplate template,
        double zoom,
        double gridSpacingMm,
        bool showGrid,
        bool showMargins,
        Color gridColor,
        IReadOnlyList<double> verticalGuides,
        IReadOnlyList<double> horizontalGuides,
        IReadOnlyList<double> snapLinesX,
        IReadOnlyList<double> snapLinesY)
    {
        Template = template;
        Zoom = zoom;
        GridSpacingMm = gridSpacingMm;
        ShowGrid = showGrid;
        ShowMargins = showMargins;
        GridColor = gridColor;
        VerticalGuides = verticalGuides;
        HorizontalGuides = horizontalGuides;
        SnapLinesX = snapLinesX;
        SnapLinesY = snapLinesY;
    }
}