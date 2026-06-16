using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Rendering;
using Xunit;

namespace ReportEngine.Core.Tests;

// ─────────────────────────────────────────────────────────────────────────────
// RenderContext 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class RenderContextFullTests
{
    [Fact]
    public void RenderContext_DataSources_DefaultEmpty()
    {
        var ctx = new RenderContext();
        Assert.NotNull(ctx.DataSources);
        Assert.Empty(ctx.DataSources);
    }

    [Fact]
    public void RenderContext_DataSourceName_DefaultEmpty()
    {
        var ctx = new RenderContext();
        Assert.Equal("", ctx.DataSourceName);
    }

    [Fact]
    public void RenderContext_DataSourceName_SetValue()
    {
        var ctx = new RenderContext { DataSourceName = "orders" };
        Assert.Equal("orders", ctx.DataSourceName);
    }

    [Fact]
    public void RenderContext_CurrentRow_DefaultNull()
    {
        var ctx = new RenderContext();
        Assert.Null(ctx.CurrentRow);
    }

    [Fact]
    public void RenderContext_CurrentRow_SetValue()
    {
        var ctx = new RenderContext
        {
            CurrentRow = new Dictionary<string, object> { { "id", 1 } }
        };
        Assert.NotNull(ctx.CurrentRow);
        Assert.Equal(1, ctx.CurrentRow["id"]);
    }

    [Fact]
    public void RenderContext_CurrentRowNumber_DefaultZero()
    {
        var ctx = new RenderContext();
        Assert.Equal(0, ctx.CurrentRowNumber);
    }

    [Fact]
    public void RenderContext_CurrentRowNumber_SetValue()
    {
        var ctx = new RenderContext { CurrentRowNumber = 5 };
        Assert.Equal(5, ctx.CurrentRowNumber);
    }

    [Fact]
    public void RenderContext_CurrentPage_Default1()
    {
        var ctx = new RenderContext();
        Assert.Equal(1, ctx.CurrentPage);
    }

    [Fact]
    public void RenderContext_CurrentPage_SetValue()
    {
        var ctx = new RenderContext { CurrentPage = 3 };
        Assert.Equal(3, ctx.CurrentPage);
    }

    [Fact]
    public void RenderContext_TotalPages_Default1()
    {
        var ctx = new RenderContext();
        Assert.Equal(1, ctx.TotalPages);
    }

    [Fact]
    public void RenderContext_TotalPages_SetValue()
    {
        var ctx = new RenderContext { TotalPages = 10 };
        Assert.Equal(10, ctx.TotalPages);
    }

    [Fact]
    public void RenderContext_FieldFormat_DefaultNull()
    {
        var ctx = new RenderContext();
        Assert.Null(ctx.FieldFormat);
    }

    [Fact]
    public void RenderContext_FieldFormat_SetValue()
    {
        var ctx = new RenderContext { FieldFormat = "#,##0.00" };
        Assert.Equal("#,##0.00", ctx.FieldFormat);
    }

    [Fact]
    public void RenderContext_PageWidth_Default210()
    {
        var ctx = new RenderContext();
        Assert.Equal(210, ctx.PageWidth);
    }

    [Fact]
    public void RenderContext_PageHeight_Default297()
    {
        var ctx = new RenderContext();
        Assert.Equal(297, ctx.PageHeight);
    }

    [Fact]
    public void RenderContext_NestingDepth_DefaultZero()
    {
        var ctx = new RenderContext();
        Assert.Equal(0, ctx.NestingDepth);
    }

    [Fact]
    public void RenderContext_NestingDepth_SetValue()
    {
        var ctx = new RenderContext { NestingDepth = 2 };
        Assert.Equal(2, ctx.NestingDepth);
    }

    [Fact]
    public void RenderContext_MaxNestingDepth_Is5()
    {
        Assert.Equal(5, RenderContext.MaxNestingDepth);
    }

    [Fact]
    public void RenderContext_DataSources_AddItem()
    {
        var ctx = new RenderContext();
        ctx.DataSources["orders"] = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "id", 1 }, { "amount", 100.0 } }
        };
        Assert.Single(ctx.DataSources);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// ExpressionEngine 系统变量测试
// ─────────────────────────────────────────────────────────────────────────────

public class ExpressionEngineSystemVarTests
{
    private readonly ExpressionEngine _engine = new();

    [Fact]
    public void Evaluate_ROW_NUMBER_ReturnsCurrentRowNumber()
    {
        var ctx = new RenderContext { CurrentRowNumber = 7 };
        var result = _engine.Evaluate("第{{ROW_NUMBER}}行", ctx);
        Assert.Equal("第7行", result);
    }

    [Fact]
    public void Evaluate_PAGE_ReturnsCurrentPage()
    {
        var ctx = new RenderContext { CurrentPage = 3 };
        var result = _engine.Evaluate("第{{PAGE}}页", ctx);
        Assert.Equal("第3页", result);
    }

    [Fact]
    public void Evaluate_TOTAL_PAGES_ReturnsTotalPages()
    {
        var ctx = new RenderContext { TotalPages = 15 };
        var result = _engine.Evaluate("共{{TOTAL_PAGES}}页", ctx);
        Assert.Equal("共15页", result);
    }

    [Fact]
    public void Evaluate_REPORT_DATE_ReturnsFormattedDate()
    {
        var ctx = new RenderContext();
        var result = _engine.Evaluate("日期：{{REPORT_DATE}}", ctx);
        Assert.Contains(DateTime.Now.ToString("yyyy-MM-dd"), result);
    }

    [Fact]
    public void Evaluate_NOW_ReturnsDateTime()
    {
        var ctx = new RenderContext();
        var result = _engine.Evaluate("时间：{{NOW}}", ctx);
        Assert.StartsWith("时间：", result);
        Assert.True(result.Length > 5);
    }

    [Fact]
    public void Evaluate_SystemVars_CaseInsensitive()
    {
        var ctx = new RenderContext { CurrentPage = 2 };
        var result = _engine.Evaluate("{{page}}", ctx);
        Assert.Equal("2", result);
    }

    [Fact]
    public void Evaluate_MultipleSystemVars()
    {
        var ctx = new RenderContext { CurrentPage = 1, TotalPages = 5 };
        var result = _engine.Evaluate("第{{PAGE}}页/共{{TOTAL_PAGES}}页", ctx);
        Assert.Equal("第1页/共5页", result);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// RenderedReport 属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class RenderedReportPropsTests
{
    [Fact]
    public void RenderedReport_PageWidth_DefaultZero()
    {
        var r = new RenderedReport();
        Assert.Equal(0, r.PageWidth);
    }

    [Fact]
    public void RenderedReport_PageWidth_SetValue()
    {
        var r = new RenderedReport { PageWidth = 210 };
        Assert.Equal(210, r.PageWidth);
    }

    [Fact]
    public void RenderedReport_PageHeight_DefaultZero()
    {
        var r = new RenderedReport();
        Assert.Equal(0, r.PageHeight);
    }

    [Fact]
    public void RenderedReport_PageHeight_SetValue()
    {
        var r = new RenderedReport { PageHeight = 297 };
        Assert.Equal(297, r.PageHeight);
    }

    [Fact]
    public void RenderedReport_Pages_DefaultEmpty()
    {
        var r = new RenderedReport();
        Assert.NotNull(r.Pages);
        Assert.Empty(r.Pages);
    }

    [Fact]
    public void RenderedReport_Pages_AddItem()
    {
        var r = new RenderedReport();
        r.Pages.Add(new RenderedPage());
        Assert.Single(r.Pages);
    }

    [Fact]
    public void RenderedReport_Template_DefaultNotNull()
    {
        var r = new RenderedReport();
        Assert.NotNull(r.Template);
    }

    [Fact]
    public void RenderedReport_Template_SetValue()
    {
        var t = new ReportTemplate { Version = "2.0" };
        var r = new RenderedReport { Template = t };
        Assert.Equal("2.0", r.Template.Version);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// RenderedPage 更多属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class RenderedPageMoreTests
{
    [Fact]
    public void RenderedPage_PageNumber_DefaultZero()
    {
        var p = new RenderedPage();
        Assert.Equal(0, p.PageNumber);
    }

    [Fact]
    public void RenderedPage_PageNumber_SetValue()
    {
        var p = new RenderedPage { PageNumber = 5 };
        Assert.Equal(5, p.PageNumber);
    }

    [Fact]
    public void RenderedPage_TotalPages_DefaultZero()
    {
        var p = new RenderedPage();
        Assert.Equal(0, p.TotalPages);
    }

    [Fact]
    public void RenderedPage_TotalPages_SetValue()
    {
        var p = new RenderedPage { TotalPages = 20 };
        Assert.Equal(20, p.TotalPages);
    }

    [Fact]
    public void RenderedPage_Elements_DefaultEmpty()
    {
        var p = new RenderedPage();
        Assert.NotNull(p.Elements);
        Assert.Empty(p.Elements);
    }

    [Fact]
    public void RenderedPage_Elements_AddMultiple()
    {
        var p = new RenderedPage();
        p.Elements.Add(new RenderedTextElement { Text = "A" });
        p.Elements.Add(new RenderedImageElement { Source = "img.png" });
        p.Elements.Add(new RenderedLineElement());
        p.Elements.Add(new RenderedShapeElement());
        Assert.Equal(4, p.Elements.Count);
    }
}
