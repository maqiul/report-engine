using System.Collections.Generic;
using System.Windows.Media;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// CanvasRenderContext 工厂 - 集中构造画布渲染上下文, 避免 MainWindow 散落字段。
/// 等价抽离自 MainWindow.BuildCanvasRenderContext()。
/// </summary>
internal static class CanvasRenderContextFactory
{
    /// <summary>
    /// 构造画布渲染上下文, 收集画布、标尺、参考线、网格、吸附线等状态。
    /// ShowMargins 始终 true (margin 永远绘制以保留行为)。
    /// </summary>
    public static CanvasRenderContext Build(
        ReportTemplate? template,
        double zoom,
        double gridSpacingMm,
        bool showGrid,
        Color gridColor,
        List<double> vGuides,
        List<double> hGuides,
        List<double> snapLinesX,
        List<double> snapLinesY)
    {
        if (template == null)
            return null!; // 调用方均已判 _template 非空，这里不抛
        return new CanvasRenderContext(
            template, zoom, gridSpacingMm, showGrid,
            showMargins: true, gridColor,
            vGuides, hGuides, snapLinesX, snapLinesY);
    }
}