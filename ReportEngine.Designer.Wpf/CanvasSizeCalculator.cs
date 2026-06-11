using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 画布尺寸计算 - 把 template.Page 尺寸按 PixelsPerMm 转换为 RenderTargetBitmap 像素大小，含 CanvasPadding。
/// 等价抽离自 MainWindow.ExportPng() 内部 "计算画布实际大小" 块 (3 行)。
/// </summary>
public static class CanvasSizeCalculator
{
    /// <summary>
    /// 返回 RenderTargetBitmap 应使用的 (Width, Height) 像素值。
    /// 算法: (template.Page.{Width,Height} * PixelsPerMm) + CanvasPadding * 2
    /// </summary>
    public static (int Width, int Height) ComputePixelSize(ReportTemplate template, double pixelsPerMm, double canvasPadding)
    {
        double pageW = template.Page.Width * pixelsPerMm;
        double pageH = template.Page.Height * pixelsPerMm;
        return (
            Width: (int)(pageW + canvasPadding * 2),
            Height: (int)(pageH + canvasPadding * 2)
        );
    }
}