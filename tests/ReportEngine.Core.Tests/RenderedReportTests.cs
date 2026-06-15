using System.Collections.Generic;
using ReportEngine.Core.Rendering;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// RenderedReport 自适应与缩放测试：
///   - FitToWidth：缩放到目标宽度（不放大）
///   - Scale：等比缩放
///   - 边界：factor≤0、PageWidth=0、factor=1.0（不变）
/// </summary>
public class RenderedReportTests
{
    private static RenderedReport MakeReport(double pageW, double pageH, params (double x, double y, double w, double h)[] elems)
    {
        var r = new RenderedReport { PageWidth = pageW, PageHeight = pageH };
        var page = new RenderedPage { PageNumber = 1 };
        foreach (var (x, y, w, h) in elems)
        {
            page.Elements.Add(new RenderedTextElement { X = x, Y = y, Width = w, Height = h });
        }
        r.Pages.Add(page);
        return r;
    }

    [Fact]
    public void FitToWidth_ScalesAllElementsAndPageHeight()
    {
        var r = MakeReport(200, 100, (10, 20, 50, 20), (50, 0, 50, 20));
        r.FitToWidth(100);  // 缩放 0.5
        Assert.Equal(100, r.PageWidth);
        Assert.Equal(50, r.PageHeight, 1);
        // 第一个元素: X=10*0.5=5, W=50*0.5=25
        Assert.Equal(5, r.Pages[0].Elements[0].X, 1);
        Assert.Equal(25, r.Pages[0].Elements[0].Width, 1);
        // 第二个元素: X=50*0.5=25
        Assert.Equal(25, r.Pages[0].Elements[1].X, 1);
    }

    [Fact]
    public void FitToWidth_AlreadySmaller_NoChange()
    {
        var r = MakeReport(50, 50, (0, 0, 10, 10));
        r.FitToWidth(100);
        Assert.Equal(50, r.PageWidth);
        Assert.Equal(10, r.Pages[0].Elements[0].Width);
    }

    [Fact]
    public void FitToWidth_ZeroPageWidth_NoChange()
    {
        var r = MakeReport(0, 0, (1, 2, 3, 4));
        r.FitToWidth(100);
        Assert.Equal(0, r.PageWidth);
        Assert.Equal(3, r.Pages[0].Elements[0].Width);
    }

    [Fact]
    public void FitToWidth_ZeroTargetWidth_NoChange()
    {
        var r = MakeReport(200, 100, (0, 0, 50, 20));
        r.FitToWidth(0);
        Assert.Equal(200, r.PageWidth);
        Assert.Equal(50, r.Pages[0].Elements[0].Width);
    }

    [Fact]
    public void Scale_DoublesWidthAndHeight()
    {
        var r = MakeReport(100, 50, (10, 20, 30, 5));
        r.Scale(2.0);
        Assert.Equal(200, r.PageWidth);
        Assert.Equal(100, r.PageHeight);
        Assert.Equal(20, r.Pages[0].Elements[0].X, 1);
        Assert.Equal(60, r.Pages[0].Elements[0].Width, 1);
    }

    [Fact]
    public void Scale_Half_ShrinksProportionally()
    {
        var r = MakeReport(100, 50, (10, 20, 30, 5));
        r.Scale(0.5);
        Assert.Equal(50, r.PageWidth);
        Assert.Equal(25, r.PageHeight);
        Assert.Equal(5, r.Pages[0].Elements[0].X, 1);
    }

    [Fact]
    public void Scale_FactorOne_NoChange()
    {
        var r = MakeReport(100, 50, (10, 20, 30, 5));
        r.Scale(1.0);
        Assert.Equal(100, r.PageWidth);
        Assert.Equal(30, r.Pages[0].Elements[0].Width);
    }

    [Fact]
    public void Scale_FactorNegative_NoChange()
    {
        var r = MakeReport(100, 50, (0, 0, 30, 5));
        r.Scale(-1.0);
        Assert.Equal(100, r.PageWidth);
    }

    [Fact]
    public void Scale_FactorZero_NoChange()
    {
        var r = MakeReport(100, 50, (0, 0, 30, 5));
        r.Scale(0);
        Assert.Equal(100, r.PageWidth);
    }

    [Fact]
    public void Scale_AcrossMultiplePages_AllScaled()
    {
        var r = new RenderedReport { PageWidth = 100, PageHeight = 50 };
        r.Pages.Add(new RenderedPage { PageNumber = 1, Elements = new List<RenderedElement> { new RenderedTextElement { X = 10, Y = 10, Width = 20, Height = 5 } } });
        r.Pages.Add(new RenderedPage { PageNumber = 2, Elements = new List<RenderedElement> { new RenderedTextElement { X = 30, Y = 40, Width = 40, Height = 10 } } });
        r.Scale(2.0);
        Assert.Equal(20, r.Pages[0].Elements[0].X, 1);
        Assert.Equal(60, r.Pages[1].Elements[0].X, 1);
        Assert.Equal(80, r.Pages[1].Elements[0].Width, 1);
    }
}
