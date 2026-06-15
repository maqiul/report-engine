using ReportEngine.Core;
using ReportEngine.Core.Rendering;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// RenderedReport 缩放行为测试：FitToWidth / Scale
/// </summary>
public class RenderedReportScalingTests
{
    // ============== FitToWidth ==============

    [Fact]
    public void FitToWidth_ScalesDown_WhenContentWider()
    {
        var report = new RenderedReport { PageWidth = 210, PageHeight = 297 };
        report.Pages.Add(new RenderedPage { PageNumber = 1 });
        report.Pages[0].Elements.Add(new RenderedTextElement
        {
            X = 10, Y = 10, Width = 100, Height = 20
        });

        report.FitToWidth(105); // 缩放到 50%

        Assert.Equal(105, report.PageWidth);
        Assert.Equal(148.5, report.PageHeight, 1);
        Assert.Equal(5, report.Pages[0].Elements[0].X, 1);
        Assert.Equal(50, report.Pages[0].Elements[0].Width, 1);
    }

    [Fact]
    public void FitToWidth_NoScale_WhenContentNarrower()
    {
        var report = new RenderedReport { PageWidth = 210, PageHeight = 297 };
        report.Pages.Add(new RenderedPage { PageNumber = 1 });

        report.FitToWidth(300); // 目标比当前宽，不缩放

        Assert.Equal(210, report.PageWidth);
        Assert.Equal(297, report.PageHeight);
    }

    [Fact]
    public void FitToWidth_NoScale_WhenTargetZero()
    {
        var report = new RenderedReport { PageWidth = 210, PageHeight = 297 };
        report.FitToWidth(0);
        Assert.Equal(210, report.PageWidth);
    }

    [Fact]
    public void FitToWidth_NoScale_WhenPageWidthZero()
    {
        var report = new RenderedReport { PageWidth = 0, PageHeight = 297 };
        report.FitToWidth(100);
        Assert.Equal(0, report.PageWidth);
    }

    [Fact]
    public void FitToWidth_NoScale_WhenNegativeTarget()
    {
        var report = new RenderedReport { PageWidth = 210, PageHeight = 297 };
        report.FitToWidth(-50);
        Assert.Equal(210, report.PageWidth);
    }

    [Fact]
    public void FitToWidth_ScalesAllPages()
    {
        var report = new RenderedReport { PageWidth = 200, PageHeight = 300 };
        report.Pages.Add(new RenderedPage { PageNumber = 1 });
        report.Pages.Add(new RenderedPage { PageNumber = 2 });
        report.Pages[0].Elements.Add(new RenderedTextElement { X = 20, Y = 30, Width = 40, Height = 50 });
        report.Pages[1].Elements.Add(new RenderedTextElement { X = 60, Y = 70, Width = 80, Height = 90 });

        report.FitToWidth(100); // 50%

        Assert.Equal(10, report.Pages[0].Elements[0].X);
        Assert.Equal(15, report.Pages[0].Elements[0].Y);
        Assert.Equal(30, report.Pages[1].Elements[0].X);
        Assert.Equal(35, report.Pages[1].Elements[0].Y);
    }

    // ============== Scale ==============

    [Fact]
    public void Scale_ByFactor_Works()
    {
        var report = new RenderedReport { PageWidth = 210, PageHeight = 297 };
        report.Pages.Add(new RenderedPage { PageNumber = 1 });
        report.Pages[0].Elements.Add(new RenderedTextElement
        {
            X = 10, Y = 20, Width = 30, Height = 40
        });

        report.Scale(2.0);

        Assert.Equal(420, report.PageWidth);
        Assert.Equal(594, report.PageHeight);
        Assert.Equal(20, report.Pages[0].Elements[0].X);
        Assert.Equal(40, report.Pages[0].Elements[0].Y);
        Assert.Equal(60, report.Pages[0].Elements[0].Width);
        Assert.Equal(80, report.Pages[0].Elements[0].Height);
    }

    [Fact]
    public void Scale_ByHalf_Works()
    {
        var report = new RenderedReport { PageWidth = 200, PageHeight = 300 };
        report.Pages.Add(new RenderedPage());
        report.Pages[0].Elements.Add(new RenderedTextElement { X = 40, Y = 60, Width = 80, Height = 100 });

        report.Scale(0.5);

        Assert.Equal(100, report.PageWidth);
        Assert.Equal(150, report.PageHeight);
        Assert.Equal(20, report.Pages[0].Elements[0].X);
        Assert.Equal(30, report.Pages[0].Elements[0].Y);
    }

    [Fact]
    public void Scale_NoOp_WhenFactorIsOne()
    {
        var report = new RenderedReport { PageWidth = 210, PageHeight = 297 };
        report.Pages.Add(new RenderedPage());
        report.Pages[0].Elements.Add(new RenderedTextElement { X = 10, Y = 20, Width = 30, Height = 40 });

        report.Scale(1.0);

        Assert.Equal(210, report.PageWidth);
        Assert.Equal(10, report.Pages[0].Elements[0].X);
    }

    [Fact]
    public void Scale_NoOp_WhenFactorZero()
    {
        var report = new RenderedReport { PageWidth = 210, PageHeight = 297 };
        report.Scale(0);
        Assert.Equal(210, report.PageWidth);
    }

    [Fact]
    public void Scale_NoOp_WhenFactorNegative()
    {
        var report = new RenderedReport { PageWidth = 210, PageHeight = 297 };
        report.Scale(-1);
        Assert.Equal(210, report.PageWidth);
    }

    [Fact]
    public void Scale_ScalesAllPages()
    {
        var report = new RenderedReport { PageWidth = 100, PageHeight = 100 };
        report.Pages.Add(new RenderedPage());
        report.Pages.Add(new RenderedPage());
        report.Pages[0].Elements.Add(new RenderedTextElement { X = 10, Y = 10, Width = 20, Height = 20 });
        report.Pages[1].Elements.Add(new RenderedTextElement { X = 30, Y = 30, Width = 40, Height = 40 });

        report.Scale(3.0);

        Assert.Equal(30, report.Pages[0].Elements[0].X);
        Assert.Equal(90, report.Pages[1].Elements[0].X);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void FitToWidth_ThenScale_CombinedWorks()
    {
        var report = new RenderedReport { PageWidth = 200, PageHeight = 200 };
        report.Pages.Add(new RenderedPage());
        report.Pages[0].Elements.Add(new RenderedTextElement { X = 20, Y = 20, Width = 40, Height = 40 });

        report.FitToWidth(100); // 50%
        Assert.Equal(100, report.PageWidth);
        Assert.Equal(10, report.Pages[0].Elements[0].X);

        report.Scale(2.0); // 2x
        Assert.Equal(200, report.PageWidth);
        Assert.Equal(20, report.Pages[0].Elements[0].X);
    }

    [Fact]
    public void FitToWidth_EmptyPages_NoError()
    {
        var report = new RenderedReport { PageWidth = 200, PageHeight = 200 };
        report.Pages.Add(new RenderedPage()); // 无元素

        report.FitToWidth(100);
        Assert.Equal(100, report.PageWidth);
    }

    [Fact]
    public void Scale_EmptyPages_NoError()
    {
        var report = new RenderedReport { PageWidth = 200, PageHeight = 200 };
        report.Pages.Add(new RenderedPage());

        report.Scale(2.0);
        Assert.Equal(400, report.PageWidth);
    }
}
