using System.Collections.Generic;
using System.Windows;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 框选元素计算 - 把 OnCanvasMouseUp 框选结束逻辑抽离。
/// 等价抽离自 MainWindow.OnCanvasMouseUp() 框选分支 (24 行)。
///
/// 输入: 拖动起点 + 终点 (canvas 像素) + zoom + PixelsPerMm + CanvasPadding
/// 输出: 框选命中的 (ReportElement, Band) 列表
/// </summary>
internal static class MarqueeSelector
{
    public static List<(ReportElement element, Band band)> Select(
        Point dragStart,
        Point endPos,
        double zoom,
        double pixelsPerMm,
        double canvasPadding,
        ReportTemplate? template)
    {
        var result = new List<(ReportElement, Band)>();
        if (template == null) return result;
        double mmPx = pixelsPerMm * zoom;
        double mx = System.Math.Min(endPos.X, dragStart.X) - canvasPadding;
        double my = System.Math.Min(endPos.Y, dragStart.Y) - canvasPadding;
        double mw = System.Math.Abs(endPos.X - dragStart.X);
        double mh = System.Math.Abs(endPos.Y - dragStart.Y);
        var rect = new Rect(mx / mmPx, my / mmPx, mw / mmPx, mh / mmPx);

        double bandY = 0;
        foreach (var band in template.Bands)
        {
            foreach (var el in band.Elements)
            {
                var elRect = new Rect(el.X, bandY + el.Y, el.Width, el.Height);
                if (rect.IntersectsWith(elRect))
                    result.Add((el, band));
            }
            bandY += band.Height;
        }
        return result;
    }
}
