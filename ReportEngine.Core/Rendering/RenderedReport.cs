using System;
using System.Collections.Generic;

namespace ReportEngine.Core.Rendering;

// ============ 渲染输出模型 ============

public class RenderedReport
{
    public ReportTemplate Template { get; set; } = new();
    public List<RenderedPage> Pages { get; set; } = new();
    public double PageWidth { get; set; }
    public double PageHeight { get; set; }

    /// <summary>
    /// 打印自适应：将所有元素缩放到指定纸张宽度（mm）。
    /// 如果内容已在目标宽度内则不缩放。
    /// </summary>
    public void FitToWidth(double targetWidthMm)
    {
        if (PageWidth <= 0 || targetWidthMm <= 0) return;
        double scale = targetWidthMm / PageWidth;
        if (scale >= 1.0) return; // 已在目标宽度内

        PageWidth = targetWidthMm;
        PageHeight = PageHeight * scale;

        foreach (var page in Pages)
        {
            foreach (var el in page.Elements)
            {
                el.X *= scale;
                el.Y *= scale;
                el.Width *= scale;
                el.Height *= scale;
            }
        }
    }

    /// <summary>
    /// 等比缩放到指定比例
    /// </summary>
    public void Scale(double factor)
    {
        if (factor <= 0 || Math.Abs(factor - 1.0) < 0.001) return;
        PageWidth *= factor;
        PageHeight *= factor;
        foreach (var page in Pages)
        {
            foreach (var el in page.Elements)
            {
                el.X *= factor;
                el.Y *= factor;
                el.Width *= factor;
                el.Height *= factor;
            }
        }
    }
}

public class RenderedPage
{
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
    public List<RenderedElement> Elements { get; set; } = new();
}

public abstract class RenderedElement
{
    public string Id { get; set; } = "";
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public string? BackgroundColor { get; set; }
    public BorderDef? Border { get; set; }
}