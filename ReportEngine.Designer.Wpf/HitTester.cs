using System.Windows;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 画布命中测试 - 根据鼠标位置反查命中的 band + element (上层优先)。
/// 等价抽离自 MainWindow.HitTest()。
/// </summary>
public static class HitTester
{
    /// <summary>
    /// 在画布坐标 (pos) 命中测试, 反查命中的 band 与 element。
    /// 同一 band 内元素按反序遍历 (后插入的画在上层, 优先命中)。
    /// </summary>
    public static (Band? Band, ReportElement? Element) Hit(
        Point pos,
        ReportTemplate? template,
        double zoom,
        double canvasPadding,
        double pixelsPerMm)
    {
        if (template == null) return (null, null);

        double mmPx = pixelsPerMm * zoom;
        double currentY = canvasPadding;
        foreach (var band in template.Bands)
        {
            double bandH = band.Height * mmPx;
            double bandTop = currentY;
            double bandBot = currentY + bandH;
            if (pos.Y >= bandTop && pos.Y <= bandBot)
            {
                // 反序遍历，上层元素优先
                for (int i = band.Elements.Count - 1; i >= 0; i--)
                {
                    var el = band.Elements[i];
                    double ex = canvasPadding + el.X * mmPx;
                    double ey = bandTop + el.Y * mmPx;
                    double ew = el.Width * mmPx;
                    double eh = el.Height * mmPx;
                    if (pos.X >= ex && pos.X <= ex + ew && pos.Y >= ey && pos.Y <= ey + eh)
                    {
                        return (band, el);
                    }
                }
                return (band, null);
            }
            currentY += bandH;
        }
        return (null, null);
    }
}