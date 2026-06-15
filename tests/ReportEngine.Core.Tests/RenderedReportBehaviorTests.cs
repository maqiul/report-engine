using ReportEngine.Core;
using ReportEngine.Core.Rendering;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// RenderedReport 行为测试：
///   - 默认值
///   - 页面管理
///   - FitToWidth 缩放
///   - Scale 等比缩放
/// </summary>
public class RenderedReportBehaviorTests
{
    // ============== 默认值 ==============

    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var report = new RenderedReport();

        Assert.NotNull(report.Template);
        Assert.NotNull(report.Pages);
        Assert.Empty(report.Pages);
        Assert.Equal(0, report.PageWidth);
        Assert.Equal(0, report.PageHeight);
    }

    // ============== Template ==============

    [Fact]
    public void Template_NotNull_ByDefault()
    {
        var report = new RenderedReport();
        Assert.NotNull(report.Template);
    }

    [Fact]
    public void Template_CanBeSet()
    {
        var template = new ReportTemplate();
        var report = new RenderedReport { Template = template };
        Assert.Same(template, report.Template);
    }

    // ============== Pages ==============

    [Fact]
    public void Pages_EmptyByDefault()
    {
        var report = new RenderedReport();
        Assert.Empty(report.Pages);
    }

    [Fact]
    public void Pages_Add_Works()
    {
        var report = new RenderedReport();
        report.Pages.Add(new RenderedPage { PageNumber = 1, TotalPages = 1 });
        Assert.Single(report.Pages);
    }

    [Fact]
    public void Pages_AddMultiple_Works()
    {
        var report = new RenderedReport();
        report.Pages.Add(new RenderedPage { PageNumber = 1, TotalPages = 3 });
        report.Pages.Add(new RenderedPage { PageNumber = 2, TotalPages = 3 });
        report.Pages.Add(new RenderedPage { PageNumber = 3, TotalPages = 3 });
        Assert.Equal(3, report.Pages.Count);
    }

    // ============== PageWidth/PageHeight ==============

    [Fact]
    public void PageWidth_SetAndGet_Works()
    {
        var report = new RenderedReport { PageWidth = 210 };
        Assert.Equal(210, report.PageWidth);
    }

    [Fact]
    public void PageHeight_SetAndGet_Works()
    {
        var report = new RenderedReport { PageHeight = 297 };
        Assert.Equal(297, report.PageHeight);
    }

    [Fact]
    public void PageSize_A4_Works()
    {
        var report = new RenderedReport { PageWidth = 210, PageHeight = 297 };
        Assert.Equal(210, report.PageWidth);
        Assert.Equal(297, report.PageHeight);
    }

    // ============== FitToWidth ==============

    [Fact]
    public void FitToWidth_ScaleDown_Works()
    {
        var report = new RenderedReport
        {
            PageWidth = 210,
            PageHeight = 297
        };
        report.Pages.Add(new RenderedPage());
        report.Pages[0].Elements.Add(new TestRenderedElement { X = 100, Y = 100, Width = 50, Height = 50 });

        report.FitToWidth(105); // 50% 缩放

        Assert.Equal(105, report.PageWidth);
        Assert.Equal(148.5, report.PageHeight, 1);
        Assert.Equal(50, report.Pages[0].Elements[0].X, 1);
        Assert.Equal(50, report.Pages[0].Elements[0].Y, 1);
        Assert.Equal(25, report.Pages[0].Elements[0].Width, 1);
        Assert.Equal(25, report.Pages[0].Elements[0].Height, 1);
    }

    [Fact]
    public void FitToWidth_NoScale_WhenAlreadyFits()
    {
        var report = new RenderedReport
        {
            PageWidth = 100,
            PageHeight = 150
        };
        report.Pages.Add(new RenderedPage());
        report.Pages[0].Elements.Add(new TestRenderedElement { X = 50, Y = 50, Width = 30, Height = 30 });

        report.FitToWidth(200); // 目标更大，不缩放

        Assert.Equal(100, report.PageWidth); // 不变
        Assert.Equal(150, report.PageHeight); // 不变
        Assert.Equal(50, report.Pages[0].Elements[0].X);
    }

    [Fact]
    public void FitToWidth_NoScale_WhenTargetZero()
    {
        var report = new RenderedReport
        {
            PageWidth = 210,
            PageHeight = 297
        };

        report.FitToWidth(0); // 无效目标

        Assert.Equal(210, report.PageWidth); // 不变
    }

    [Fact]
    public void FitToWidth_NoScale_WhenPageWidthZero()
    {
        var report = new RenderedReport
        {
            PageWidth = 0,
            PageHeight = 297
        };

        report.FitToWidth(100); // 源宽度为 0

        Assert.Equal(0, report.PageWidth); // 不变
    }

    [Fact]
    public void FitToWidth_MultiplePages_Works()
    {
        var report = new RenderedReport
        {
            PageWidth = 200,
            PageHeight = 300
        };
        report.Pages.Add(new RenderedPage());
        report.Pages.Add(new RenderedPage());
        report.Pages[0].Elements.Add(new TestRenderedElement { X = 100, Y = 100, Width = 50, Height = 50 });
        report.Pages[1].Elements.Add(new TestRenderedElement { X = 80, Y = 80, Width = 40, Height = 40 });

        report.FitToWidth(100); // 50% 缩放

        Assert.Equal(50, report.Pages[0].Elements[0].X, 1);
        Assert.Equal(40, report.Pages[1].Elements[0].X, 1);
    }

    // ============== Scale ==============

    [Fact]
    public void Scale_Up_Works()
    {
        var report = new RenderedReport
        {
            PageWidth = 100,
            PageHeight = 150
        };
        report.Pages.Add(new RenderedPage());
        report.Pages[0].Elements.Add(new TestRenderedElement { X = 50, Y = 50, Width = 30, Height = 30 });

        report.Scale(2.0);

        Assert.Equal(200, report.PageWidth);
        Assert.Equal(300, report.PageHeight);
        Assert.Equal(100, report.Pages[0].Elements[0].X);
        Assert.Equal(100, report.Pages[0].Elements[0].Y);
        Assert.Equal(60, report.Pages[0].Elements[0].Width);
        Assert.Equal(60, report.Pages[0].Elements[0].Height);
    }

    [Fact]
    public void Scale_Down_Works()
    {
        var report = new RenderedReport
        {
            PageWidth = 200,
            PageHeight = 300
        };
        report.Pages.Add(new RenderedPage());
        report.Pages[0].Elements.Add(new TestRenderedElement { X = 100, Y = 100, Width = 60, Height = 60 });

        report.Scale(0.5);

        Assert.Equal(100, report.PageWidth);
        Assert.Equal(150, report.PageHeight);
        Assert.Equal(50, report.Pages[0].Elements[0].X);
        Assert.Equal(50, report.Pages[0].Elements[0].Y);
        Assert.Equal(30, report.Pages[0].Elements[0].Width);
        Assert.Equal(30, report.Pages[0].Elements[0].Height);
    }

    [Fact]
    public void Scale_NoScale_WhenFactorOne()
    {
        var report = new RenderedReport
        {
            PageWidth = 100,
            PageHeight = 150
        };
        report.Pages.Add(new RenderedPage());
        report.Pages[0].Elements.Add(new TestRenderedElement { X = 50, Y = 50, Width = 30, Height = 30 });

        report.Scale(1.0);

        Assert.Equal(100, report.PageWidth); // 不变
        Assert.Equal(50, report.Pages[0].Elements[0].X); // 不变
    }

    [Fact]
    public void Scale_NoScale_WhenFactorZero()
    {
        var report = new RenderedReport
        {
            PageWidth = 100,
            PageHeight = 150
        };

        report.Scale(0); // 无效因子

        Assert.Equal(100, report.PageWidth); // 不变
    }

    [Fact]
    public void Scale_NoScale_WhenFactorNegative()
    {
        var report = new RenderedReport
        {
            PageWidth = 100,
            PageHeight = 150
        };

        report.Scale(-1); // 无效因子

        Assert.Equal(100, report.PageWidth); // 不变
    }

    [Fact]
    public void Scale_MultiplePages_Works()
    {
        var report = new RenderedReport
        {
            PageWidth = 100,
            PageHeight = 150
        };
        report.Pages.Add(new RenderedPage());
        report.Pages.Add(new RenderedPage());
        report.Pages[0].Elements.Add(new TestRenderedElement { X = 50, Y = 50, Width = 30, Height = 30 });
        report.Pages[1].Elements.Add(new TestRenderedElement { X = 40, Y = 40, Width = 20, Height = 20 });

        report.Scale(2.0);

        Assert.Equal(100, report.Pages[0].Elements[0].X);
        Assert.Equal(80, report.Pages[1].Elements[0].X);
    }

    // ============== RenderedPage ==============

    [Fact]
    public void RenderedPage_DefaultValues()
    {
        var page = new RenderedPage();

        Assert.Equal(0, page.PageNumber);
        Assert.Equal(0, page.TotalPages);
        Assert.NotNull(page.Elements);
        Assert.Empty(page.Elements);
    }

    [Fact]
    public void RenderedPage_SetPageNumber_Works()
    {
        var page = new RenderedPage { PageNumber = 1, TotalPages = 5 };
        Assert.Equal(1, page.PageNumber);
        Assert.Equal(5, page.TotalPages);
    }

    [Fact]
    public void RenderedPage_AddElements_Works()
    {
        var page = new RenderedPage();
        page.Elements.Add(new TestRenderedElement { X = 10, Y = 20, Width = 100, Height = 50 });
        page.Elements.Add(new TestRenderedElement { X = 10, Y = 80, Width = 100, Height = 50 });
        Assert.Equal(2, page.Elements.Count);
    }

    // ============== RenderedElement ==============

    [Fact]
    public void RenderedElement_DefaultValues()
    {
        var el = new TestRenderedElement();

        Assert.Equal("", el.Id);
        Assert.Equal(0, el.X);
        Assert.Equal(0, el.Y);
        Assert.Equal(0, el.Width);
        Assert.Equal(0, el.Height);
        Assert.Null(el.BackgroundColor);
        Assert.Null(el.Border);
    }

    [Fact]
    public void RenderedElement_SetPosition_Works()
    {
        var el = new TestRenderedElement { X = 10, Y = 20, Width = 100, Height = 50 };
        Assert.Equal(10, el.X);
        Assert.Equal(20, el.Y);
        Assert.Equal(100, el.Width);
        Assert.Equal(50, el.Height);
    }

    [Fact]
    public void RenderedElement_SetBackgroundColor_Works()
    {
        var el = new TestRenderedElement { BackgroundColor = "#FF0000" };
        Assert.Equal("#FF0000", el.BackgroundColor);
    }

    [Fact]
    public void RenderedElement_SetBorder_Works()
    {
        var border = new BorderDef { Width = 1, Color = "#000000" };
        var el = new TestRenderedElement { Border = border };
        Assert.Same(border, el.Border);
    }

    // ============== 辅助测试类 ==============

    private class TestRenderedElement : RenderedElement
    {
    }
}
