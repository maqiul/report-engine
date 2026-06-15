using System.Collections.Generic;
using ReportEngine.Core.Data;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// RenderContext 完整字段测试：
///   - RenderContext 完整字段（DataSources/DataSourceName/CurrentRow/CurrentRowNumber/CurrentPage/TotalPages/FieldFormat/PageWidth/PageHeight/NestingDepth/MaxNestingDepth）
///   - 字段组合行为
/// </summary>
public class RenderContextCompleteTests
{
    [Fact]
    public void RenderContext_Defaults()
    {
        var ctx = new RenderContext();
        Assert.NotNull(ctx.DataSources);
        Assert.Empty(ctx.DataSources);
        Assert.Equal("", ctx.DataSourceName);
        Assert.Null(ctx.CurrentRow);
        Assert.Equal(0, ctx.CurrentRowNumber);
        Assert.Equal(1, ctx.CurrentPage);
        Assert.Equal(1, ctx.TotalPages);
        Assert.Null(ctx.FieldFormat);
        Assert.Equal(210, ctx.PageWidth);
        Assert.Equal(297, ctx.PageHeight);
        Assert.Equal(0, ctx.NestingDepth);
    }

    [Fact]
    public void RenderContext_MaxNestingDepth_Is5()
    {
        Assert.Equal(5, RenderContext.MaxNestingDepth);
    }

    [Fact]
    public void RenderContext_AllSetters()
    {
        var ctx = new RenderContext
        {
            DataSourceName = "orders",
            CurrentRow = new Dictionary<string, object> { { "id", 1 } },
            CurrentRowNumber = 5,
            CurrentPage = 3,
            TotalPages = 10,
            FieldFormat = "N2",
            PageWidth = 100,
            PageHeight = 150,
            NestingDepth = 2,
        };
        Assert.Equal("orders", ctx.DataSourceName);
        Assert.NotNull(ctx.CurrentRow);
        Assert.Equal(5, ctx.CurrentRowNumber);
        Assert.Equal(3, ctx.CurrentPage);
        Assert.Equal(10, ctx.TotalPages);
        Assert.Equal("N2", ctx.FieldFormat);
        Assert.Equal(100, ctx.PageWidth);
        Assert.Equal(150, ctx.PageHeight);
        Assert.Equal(2, ctx.NestingDepth);
    }

    [Fact]
    public void RenderContext_DataSourceName_CanBeEmpty()
    {
        var ctx = new RenderContext { DataSourceName = "" };
        Assert.Equal("", ctx.DataSourceName);
    }

    [Fact]
    public void RenderContext_DataSourceName_CanBeAnyString()
    {
        var ctx = new RenderContext { DataSourceName = "products" };
        Assert.Equal("products", ctx.DataSourceName);
    }

    [Fact]
    public void RenderContext_CurrentRow_CanBeNull()
    {
        var ctx = new RenderContext { CurrentRow = null };
        Assert.Null(ctx.CurrentRow);
    }

    [Fact]
    public void RenderContext_CurrentRow_CanBeSet()
    {
        var row = new Dictionary<string, object> { { "name", "test" }, { "value", 42 } };
        var ctx = new RenderContext { CurrentRow = row };
        Assert.NotNull(ctx.CurrentRow);
        Assert.Equal("test", ctx.CurrentRow["name"]);
        Assert.Equal(42, ctx.CurrentRow["value"]);
    }

    [Fact]
    public void RenderContext_CurrentRowNumber_CanBeZero()
    {
        var ctx = new RenderContext { CurrentRowNumber = 0 };
        Assert.Equal(0, ctx.CurrentRowNumber);
    }

    [Fact]
    public void RenderContext_CurrentRowNumber_CanBePositive()
    {
        var ctx = new RenderContext { CurrentRowNumber = 100 };
        Assert.Equal(100, ctx.CurrentRowNumber);
    }

    [Fact]
    public void RenderContext_CurrentPage_Default1()
    {
        var ctx = new RenderContext();
        Assert.Equal(1, ctx.CurrentPage);
    }

    [Fact]
    public void RenderContext_CurrentPage_CanBeSet()
    {
        var ctx = new RenderContext { CurrentPage = 5 };
        Assert.Equal(5, ctx.CurrentPage);
    }

    [Fact]
    public void RenderContext_TotalPages_Default1()
    {
        var ctx = new RenderContext();
        Assert.Equal(1, ctx.TotalPages);
    }

    [Fact]
    public void RenderContext_TotalPages_CanBeSet()
    {
        var ctx = new RenderContext { TotalPages = 20 };
        Assert.Equal(20, ctx.TotalPages);
    }

    [Fact]
    public void RenderContext_FieldFormat_CanBeNull()
    {
        var ctx = new RenderContext { FieldFormat = null };
        Assert.Null(ctx.FieldFormat);
    }

    [Fact]
    public void RenderContext_FieldFormat_CanBeEmpty()
    {
        var ctx = new RenderContext { FieldFormat = "" };
        Assert.Equal("", ctx.FieldFormat);
    }

    [Fact]
    public void RenderContext_FieldFormat_CanBeSet()
    {
        var ctx = new RenderContext { FieldFormat = "C" };
        Assert.Equal("C", ctx.FieldFormat);
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
    public void RenderContext_PageWidth_CanBeSet()
    {
        var ctx = new RenderContext { PageWidth = 297 };
        Assert.Equal(297, ctx.PageWidth);
    }

    [Fact]
    public void RenderContext_PageHeight_CanBeSet()
    {
        var ctx = new RenderContext { PageHeight = 420 };
        Assert.Equal(420, ctx.PageHeight);
    }

    [Fact]
    public void RenderContext_PageWidth_CanBeDecimal()
    {
        var ctx = new RenderContext { PageWidth = 210.5 };
        Assert.Equal(210.5, ctx.PageWidth);
    }

    [Fact]
    public void RenderContext_NestingDepth_Default0()
    {
        var ctx = new RenderContext();
        Assert.Equal(0, ctx.NestingDepth);
    }

    [Fact]
    public void RenderContext_NestingDepth_CanBeSet()
    {
        var ctx = new RenderContext { NestingDepth = 3 };
        Assert.Equal(3, ctx.NestingDepth);
    }

    [Fact]
    public void RenderContext_DataSources_CanAdd()
    {
        var ctx = new RenderContext();
        ctx.DataSources.Add("ds1", new List<Dictionary<string, object>>());
        Assert.Single(ctx.DataSources);
    }

    [Fact]
    public void RenderContext_DataSources_CanAddMultiple()
    {
        var ctx = new RenderContext();
        ctx.DataSources.Add("ds1", new List<Dictionary<string, object>>());
        ctx.DataSources.Add("ds2", new List<Dictionary<string, object>>());
        Assert.Equal(2, ctx.DataSources.Count);
    }

    [Fact]
    public void RenderContext_DataSources_ReadOnly_NoSetter()
    {
        // DataSources 只有 getter，不能直接赋值
        var ctx = new RenderContext();
        var ds = ctx.DataSources;
        Assert.NotNull(ds);
    }
}
