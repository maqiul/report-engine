using System.Collections.Generic;
using ReportEngine.Core.Data;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// RenderContext 行为测试：
///   - 默认值
///   - DataSources 操作
///   - CurrentRow 操作
///   - 分页状态
///   - 格式化状态
/// </summary>
public class RenderContextBehaviorTests
{
    // ============== 默认值 ==============

    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var ctx = new RenderContext();

        Assert.Equal(210, ctx.PageWidth);
        Assert.Equal(297, ctx.PageHeight);
        Assert.Equal("", ctx.DataSourceName);
        Assert.Equal(1, ctx.CurrentPage);
        Assert.Equal(1, ctx.TotalPages);
        Assert.Equal(0, ctx.CurrentRowNumber);
        Assert.Null(ctx.CurrentRow);
        Assert.Empty(ctx.DataSources);
        Assert.Null(ctx.FieldFormat);
        Assert.Equal(0, ctx.NestingDepth);
    }

    // ============== DataSources ==============

    [Fact]
    public void DataSources_AddAndRetrieve_Works()
    {
        var ctx = new RenderContext();
        var data = new List<Dictionary<string, object>>
        {
            new() { { "id", 1 } },
            new() { { "id", 2 } }
        };

        ctx.DataSources.Add("orders", data);

        Assert.True(ctx.DataSources.ContainsKey("orders"));
        Assert.Equal(2, ctx.DataSources["orders"].Count);
    }

    [Fact]
    public void DataSources_MultipleSources_Independent()
    {
        var ctx = new RenderContext();
        var orders = new List<Dictionary<string, object>>
        {
            new() { { "id", 1 } }
        };
        var products = new List<Dictionary<string, object>>
        {
            new() { { "name", "Widget" } }
        };

        ctx.DataSources.Add("orders", orders);
        ctx.DataSources.Add("products", products);

        Assert.Equal(2, ctx.DataSources.Count);
        Assert.Single(ctx.DataSources["orders"]);
        Assert.Single(ctx.DataSources["products"]);
    }

    [Fact]
    public void DataSources_OverwriteExisting_ReplacesData()
    {
        var ctx = new RenderContext();
        var data1 = new List<Dictionary<string, object>>
        {
            new() { { "id", 1 } }
        };
        var data2 = new List<Dictionary<string, object>>
        {
            new() { { "id", 1 } },
            new() { { "id", 2 } },
            new() { { "id", 3 } }
        };

        ctx.DataSources.Add("orders", data1);
        ctx.DataSources["orders"] = data2;

        Assert.Equal(3, ctx.DataSources["orders"].Count);
    }

    // ============== CurrentRow ==============

    [Fact]
    public void CurrentRow_SetAndGet_Works()
    {
        var ctx = new RenderContext();
        var row = new Dictionary<string, object>
        {
            { "name", "Alice" },
            { "age", 30 }
        };

        ctx.CurrentRow = row;

        Assert.NotNull(ctx.CurrentRow);
        Assert.Equal("Alice", ctx.CurrentRow["name"]);
        Assert.Equal(30, ctx.CurrentRow["age"]);
    }

    [Fact]
    public void CurrentRow_NullAllowed()
    {
        var ctx = new RenderContext();
        ctx.CurrentRow = new Dictionary<string, object> { { "id", 1 } };
        ctx.CurrentRow = null;

        Assert.Null(ctx.CurrentRow);
    }

    [Fact]
    public void CurrentRowNumber_Increment_Works()
    {
        var ctx = new RenderContext();
        Assert.Equal(0, ctx.CurrentRowNumber);

        ctx.CurrentRowNumber = 1;
        Assert.Equal(1, ctx.CurrentRowNumber);

        ctx.CurrentRowNumber++;
        Assert.Equal(2, ctx.CurrentRowNumber);
    }

    // ============== 分页状态 ==============

    [Fact]
    public void CurrentPage_DefaultIsOne()
    {
        var ctx = new RenderContext();
        Assert.Equal(1, ctx.CurrentPage);
    }

    [Fact]
    public void CurrentPage_SetAndGet_Works()
    {
        var ctx = new RenderContext();
        ctx.CurrentPage = 5;
        Assert.Equal(5, ctx.CurrentPage);
    }

    [Fact]
    public void TotalPages_DefaultIsOne()
    {
        var ctx = new RenderContext();
        Assert.Equal(1, ctx.TotalPages);
    }

    [Fact]
    public void TotalPages_SetAndGet_Works()
    {
        var ctx = new RenderContext();
        ctx.TotalPages = 10;
        Assert.Equal(10, ctx.TotalPages);
    }

    [Fact]
    public void PageNavigation_SimulatePaging()
    {
        var ctx = new RenderContext { TotalPages = 5 };

        for (int i = 1; i <= 5; i++)
        {
            ctx.CurrentPage = i;
            Assert.Equal(i, ctx.CurrentPage);
        }
    }

    // ============== 页面尺寸 ==============

    [Fact]
    public void PageWidth_SetAndGet_Works()
    {
        var ctx = new RenderContext();
        ctx.PageWidth = 100;
        Assert.Equal(100, ctx.PageWidth);
    }

    [Fact]
    public void PageHeight_SetAndGet_Works()
    {
        var ctx = new RenderContext();
        ctx.PageHeight = 150;
        Assert.Equal(150, ctx.PageHeight);
    }

    [Fact]
    public void PageSize_LandscapeOrientation()
    {
        var ctx = new RenderContext();
        ctx.PageWidth = 297;
        ctx.PageHeight = 210;

        Assert.True(ctx.PageWidth > ctx.PageHeight);
    }

    [Fact]
    public void PageSize_CustomSize()
    {
        var ctx = new RenderContext();
        ctx.PageWidth = 105;
        ctx.PageHeight = 148;

        Assert.Equal(105, ctx.PageWidth);
        Assert.Equal(148, ctx.PageHeight);
    }

    // ============== FieldFormat ==============

    [Fact]
    public void FieldFormat_NullByDefault()
    {
        var ctx = new RenderContext();
        Assert.Null(ctx.FieldFormat);
    }

    [Fact]
    public void FieldFormat_SetAndGet_Works()
    {
        var ctx = new RenderContext();
        ctx.FieldFormat = "date";
        Assert.Equal("date", ctx.FieldFormat);
    }

    [Fact]
    public void FieldFormat_AllFormats_Accepted()
    {
        var ctx = new RenderContext();

        ctx.FieldFormat = "date";
        Assert.Equal("date", ctx.FieldFormat);

        ctx.FieldFormat = "datetime";
        Assert.Equal("datetime", ctx.FieldFormat);

        ctx.FieldFormat = "currency";
        Assert.Equal("currency", ctx.FieldFormat);

        ctx.FieldFormat = "percent";
        Assert.Equal("percent", ctx.FieldFormat);

        ctx.FieldFormat = "number:2";
        Assert.Equal("number:2", ctx.FieldFormat);
    }

    // ============== DataSourceName ==============

    [Fact]
    public void DataSourceName_EmptyByDefault()
    {
        var ctx = new RenderContext();
        Assert.Equal("", ctx.DataSourceName);
    }

    [Fact]
    public void DataSourceName_SetAndGet_Works()
    {
        var ctx = new RenderContext();
        ctx.DataSourceName = "orders";
        Assert.Equal("orders", ctx.DataSourceName);
    }

    [Fact]
    public void DataSourceName_ChangeDataSource_Works()
    {
        var ctx = new RenderContext();
        ctx.DataSourceName = "orders";
        Assert.Equal("orders", ctx.DataSourceName);

        ctx.DataSourceName = "products";
        Assert.Equal("products", ctx.DataSourceName);
    }

    // ============== NestingDepth ==============

    [Fact]
    public void NestingDepth_DefaultIsZero()
    {
        var ctx = new RenderContext();
        Assert.Equal(0, ctx.NestingDepth);
    }

    [Fact]
    public void NestingDepth_SetAndGet_Works()
    {
        var ctx = new RenderContext();
        ctx.NestingDepth = 3;
        Assert.Equal(3, ctx.NestingDepth);
    }

    [Fact]
    public void MaxNestingDepth_IsFive()
    {
        Assert.Equal(5, RenderContext.MaxNestingDepth);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void RenderContext_FullSetup_Works()
    {
        var ctx = new RenderContext
        {
            PageWidth = 210,
            PageHeight = 297,
            DataSourceName = "orders",
            CurrentPage = 3,
            TotalPages = 10,
            CurrentRowNumber = 15,
            FieldFormat = "currency"
        };

        ctx.DataSources.Add("orders", new List<Dictionary<string, object>>
        {
            new() { { "id", 1 }, { "amount", 100m } },
            new() { { "id", 2 }, { "amount", 200m } }
        });

        ctx.CurrentRow = new Dictionary<string, object>
        {
            { "id", 1 },
            { "amount", 100m }
        };

        Assert.Equal(210, ctx.PageWidth);
        Assert.Equal(297, ctx.PageHeight);
        Assert.Equal("orders", ctx.DataSourceName);
        Assert.Equal(3, ctx.CurrentPage);
        Assert.Equal(10, ctx.TotalPages);
        Assert.Equal(15, ctx.CurrentRowNumber);
        Assert.Equal("currency", ctx.FieldFormat);
        Assert.Single(ctx.DataSources);
        Assert.Equal(2, ctx.DataSources["orders"].Count);
        Assert.NotNull(ctx.CurrentRow);
    }
}
