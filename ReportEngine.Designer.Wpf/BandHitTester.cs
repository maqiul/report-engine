using System.Linq;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 拖放命中测试 - 根据画布坐标反查鼠标所在的 band (在哪个 band 的高度范围内)。
/// 等价抽离自 MainWindow.OnCanvasDrop() 中的"确定 drop 位置对应的 Band"循环。
/// </summary>
public static class BandHitTester
{
    /// <summary>
    /// 在画布坐标 (canvasX, canvasY) 命中测试, 返回 (命中的 band, band 内相对 y mm)。
    /// 如 y 落在所有 band 之外, 落到最后一个 band (允许拖入底部 band 之外);
    /// 若模板无 band, 返回 (null, 0)。
    /// </summary>
    public static (Band? Band, double RelativeY) FindBandAtY(
        ReportTemplate template,
        double canvasX,
        double canvasY,
        double zoom,
        double canvasPadding,
        double pixelsPerMm)
    {
        if (template.Bands.Count == 0) return (null, 0);

        double mmPx = pixelsPerMm * zoom;
        double bandY = canvasPadding;
        foreach (var band in template.Bands)
        {
            double bh = band.Height * mmPx;
            if (canvasY >= bandY && canvasY < bandY + bh)
            {
                double relY = (canvasY - bandY) / mmPx;
                return (band, relY);
            }
            bandY += bh;
        }
        // y 超出所有 band, 落到最后一个 band
        var last = template.Bands[template.Bands.Count - 1];
        return (last, last.Height);
    }
}