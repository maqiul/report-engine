using System;
using System.Collections.Generic;
using System.Linq;
using ReportEngine.Core;
using ReportEngine.Core.Rendering;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// RenderedReport / RenderedPage / RenderedElement 行为测试：
///   - FitToWidth: 内容宽 > 目标宽 → 缩放
///   - FitToWidth: 内容宽 ≤ 目标宽 → 不变
///   - FitToWidth: 零值保护（PageWidth=0, targetWidth=0）
///   - Scale: 正常缩放
///   - Scale: factor=1 跳过
///   - Scale: factor<=0 跳过
///   - AddPage 行为
///   - Element X/Y/W/H 缩放正确
/// </summary>
public class RenderedReportBehaviorTests
{
    private static RenderedReport BuildReport(double w, double h, params (double x, double y, double ew, double eh)[] elements)
    {
        var r = new RenderedReport
        {
            PageWidth = w,
            PageHeight = h,
        };
        var page = new RenderedPage { PageNumber = 1, TotalPages = 1 };
        foreach (var (ex, ey, ew, eh) in elements)
        {
            page.Elements.Add(new RenderedTextElement
            {
                X = ex,
                Y = ey,
                Width = ew,
                Height = eh,
            });
        }
        r.Pages.Add(page);
        return r;
    }

    [Fact]
    public void FitToWidth_ScalesContent()
    {
        var r = BuildReport(200, 100, (10, 10, 50, 20));
        r.FitToWidth(100);
        Assert.Equal(100, r.PageWidth);
        // 高度也按比例缩放
        Assert.Equal(50, r.PageHeight, 2);
        var el = r.Pages[0].Elements[0];
        Assert.Equal(5, el.X, 2);
        Assert.Equal(5, el.Y, 2);
        Assert.Equal(25, el.Width, 2);
        Assert.Equal(10, el.Height, 2);
    }

    [Fact]
    public void FitToWidth_AlreadyFits_NoScale()
    {
        var r = BuildReport(100, 100, (10, 10, 20, 20));
        r.FitToWidth(200);
        Assert.Equal(100, r.PageWidth);
        Assert.Equal(100, r.PageHeight);
        var el = r.Pages[0].Elements[0];
        Assert.Equal(10, el.X, 2);
    }

    [Fact]
    public void FitToWidth_ZeroPageWidth_NoOp()
    {
        var r = BuildReport(0, 0, (10, 10, 50, 20));
        r.FitToWidth(100);
        Assert.Equal(0, r.PageWidth);
    }

    [Fact]
    public void FitToWidth_ZeroTargetWidth_NoOp()
    {
        var r = BuildReport(200, 100, (10, 10, 50, 20));
        r.FitToWidth(0);
        Assert.Equal(200, r.PageWidth);
    }

    [Fact]
    public void FitToWidth_MultipleElements_AllScaled()
    {
        var r = BuildReport(200, 100, (10, 10, 50, 20), (20, 30, 30, 40));
        r.FitToWidth(100);
        Assert.Equal(2, r.Pages[0].Elements.Count);
        Assert.Equal(5, r.Pages[0].Elements[0].X, 2);
        Assert.Equal(10, r.Pages[0].Elements[1].X, 2);
    }

    [Fact]
    public void FitToWidth_PreservesPageCount()
    {
        var r = BuildReport(200, 100, (10, 10, 50, 20));
        r.Pages.Add(new RenderedPage { PageNumber = 2, TotalPages = 2 });
        r.FitToWidth(100);
        Assert.Equal(2, r.Pages.Count);
    }

    [Fact]
    public void Scale_NormalFactor_ScalesAll()
    {
        var r = BuildReport(100, 100, (10, 10, 20, 20));
        r.Scale(2.0);
        Assert.Equal(200, r.PageWidth);
        Assert.Equal(200, r.PageHeight);
        var el = r.Pages[0].Elements[0];
        Assert.Equal(20, el.X, 2);
        Assert.Equal(40, el.Width, 2);
    }

    [Fact]
    public void Scale_FactorOne_NoOp()
    {
        var r = BuildReport(100, 100, (10, 10, 20, 20));
        r.Scale(1.0);
        Assert.Equal(100, r.PageWidth);
        Assert.Equal(10, r.Pages[0].Elements[0].X, 2);
    }

    [Fact]
    public void Scale_FactorZero_NoOp()
    {
        var r = BuildReport(100, 100, (10, 10, 20, 20));
        r.Scale(0);
        Assert.Equal(100, r.PageWidth);
    }

    [Fact]
    public void Scale_FactorNegative_NoOp()
    {
        var r = BuildReport(100, 100, (10, 10, 20, 20));
        r.Scale(-1);
        Assert.Equal(100, r.PageWidth);
    }

    [Fact]
    public void Scale_SmallFactor_Shrinks()
    {
        var r = BuildReport(100, 100, (10, 10, 20, 20));
        r.Scale(0.5);
        Assert.Equal(50, r.PageWidth);
        Assert.Equal(5, r.Pages[0].Elements[0].X, 2);
    }

    [Fact]
    public void RenderedPage_Defaults()
    {
        var p = new RenderedPage();
        Assert.Equal(0, p.PageNumber);
        Assert.Equal(0, p.TotalPages);
        Assert.NotNull(p.Elements);
        Assert.Empty(p.Elements);
    }

    [Fact]
    public void RenderedElement_Defaults()
    {
        var e = new RenderedTextElement();
        Assert.Equal("", e.Id);
        Assert.Equal(0, e.X);
        Assert.Equal(0, e.Y);
        Assert.Null(e.BackgroundColor);
        Assert.Null(e.Border);
    }

    [Fact]
    public void RenderedReport_Defaults()
    {
        var r = new RenderedReport();
        Assert.NotNull(r.Template);
        Assert.NotNull(r.Pages);
        Assert.Empty(r.Pages);
        Assert.Equal(0, r.PageWidth);
        Assert.Equal(0, r.PageHeight);
    }

    [Fact]
    public void AddPage_GrowsList()
    {
        var r = new RenderedReport();
        r.Pages.Add(new RenderedPage { PageNumber = 1 });
        r.Pages.Add(new RenderedPage { PageNumber = 2 });
        Assert.Equal(2, r.Pages.Count);
    }

    [Fact]
    public void RemovePage_ShrinksList()
    {
        var r = new RenderedReport();
        r.Pages.Add(new RenderedPage { PageNumber = 1 });
        r.Pages.Add(new RenderedPage { PageNumber = 2 });
        r.Pages.RemoveAt(0);
        Assert.Single(r.Pages);
        Assert.Equal(2, r.Pages[0].PageNumber);
    }
}

/// <summary>
/// 测试用 RenderedTextElement 删除（用真品）
/// </summary>
