using ReportEngine.Core.Data;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// RenderContext 行为测试：
///   - 默认值
///   - DataSources 管理
///   - 当前行状态
///   - 页码状态
///   - 页面尺寸
///   - 嵌套深度
/// </summary>
public class RenderContextBehaviorTests
{
    // ============== 默认值 ==============

    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
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

    // ============== DataSources ==============

    [Fact]
    public void DataSources_NotNull_ByDefault()
    {
        var ctx = new RenderContext();
        Assert.NotNull(ctx.DataSources);
    }

    [Fact]
    public void DataSources_EmptyByDefault()
    {
        var ctx = new RenderContext();
        Assert.Empty(ctx.DataSources);
    }

    [Fact]
    public void DataSources_Add_Works()
    {
        var ctx = new RenderContext();
        var rows = new List<Dictionary<string, object>>
        {
            new() { { "id", 1 }, { "name", "Alice" } }
        };
        ctx.DataSources.Add("users", rows);
        Assert.Single(ctx.DataSources);
        Assert.True(ctx.DataSources.ContainsKey("users"));
    }

    [Fact]
    public void DataSources_AddMultiple_Works()
    {
        var ctx = new RenderContext();
        ctx.DataSources.Add("users", new List<Dictionary<string, object>>());
        ctx.DataSources.Add("orders", new List<Dictionary<string, object>>());
        ctx.DataSources.Add("products", new List<Dictionary<string, object>>());
        Assert.Equal(3, ctx.DataSources.Count);
    }

    [Fact]
    public void DataSources_ContainsKey_Works()
    {
        var ctx = new RenderContext();
        ctx.DataSources.Add("users", new List<Dictionary<string, object>>());
        Assert.True(ctx.DataSources.ContainsKey("users"));
        Assert.False(ctx.DataSources.ContainsKey("orders"));
    }

    [Fact]
    public void DataSources_GetRows_Works()
    {
        var ctx = new RenderContext();
        var rows = new List<Dictionary<string, object>>
        {
            new() { { "id", 1 }, { "name", "Alice" } },
            new() { { "id", 2 }, { "name", "Bob" } }
        };
        ctx.DataSources.Add("users", rows);
        
        var retrieved = ctx.DataSources["users"];
        Assert.Equal(2, retrieved.Count);
        Assert.Equal("Alice", retrieved[0]["name"]);
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
        var ctx = new RenderContext { DataSourceName = "orders" };
        Assert.Equal("orders", ctx.DataSourceName);
    }

    [Fact]
    public void DataSourceName_CanBeChanged()
    {
        var ctx = new RenderContext { DataSourceName = "users" };
        ctx.DataSourceName = "orders";
        Assert.Equal("orders", ctx.DataSourceName);
    }

    // ============== CurrentRow ==============

    [Fact]
    public void CurrentRow_NullByDefault()
    {
        var ctx = new RenderContext();
        Assert.Null(ctx.CurrentRow);
    }

    [Fact]
    public void CurrentRow_SetAndGet_Works()
    {
        var ctx = new RenderContext();
        var row = new Dictionary<string, object> { { "id", 1 }, { "name", "Alice" } };
        ctx.CurrentRow = row;
        Assert.Same(row, ctx.CurrentRow);
        Assert.Equal("Alice", ctx.CurrentRow["name"]);
    }

    [Fact]
    public void CurrentRow_CanBeCleared()
    {
        var ctx = new RenderContext();
        ctx.CurrentRow = new Dictionary<string, object>();
        ctx.CurrentRow = null;
        Assert.Null(ctx.CurrentRow);
    }

    // ============== CurrentRowNumber ==============

    [Fact]
    public void CurrentRowNumber_ZeroByDefault()
    {
        var ctx = new RenderContext();
        Assert.Equal(0, ctx.CurrentRowNumber);
    }

    [Fact]
    public void CurrentRowNumber_SetAndGet_Works()
    {
        var ctx = new RenderContext { CurrentRowNumber = 5 };
        Assert.Equal(5, ctx.CurrentRowNumber);
    }

    [Fact]
    public void CurrentRowNumber_CanBeIncremented()
    {
        var ctx = new RenderContext { CurrentRowNumber = 1 };
        ctx.CurrentRowNumber++;
        Assert.Equal(2, ctx.CurrentRowNumber);
    }

    // ============== CurrentPage ==============

    [Fact]
    public void CurrentPage_DefaultIs1()
    {
        var ctx = new RenderContext();
        Assert.Equal(1, ctx.CurrentPage);
    }

    [Fact]
    public void CurrentPage_SetAndGet_Works()
    {
        var ctx = new RenderContext { CurrentPage = 3 };
        Assert.Equal(3, ctx.CurrentPage);
    }

    [Fact]
    public void CurrentPage_CanBeIncremented()
    {
        var ctx = new RenderContext { CurrentPage = 1 };
        ctx.CurrentPage++;
        Assert.Equal(2, ctx.CurrentPage);
    }

    // ============== TotalPages ==============

    [Fact]
    public void TotalPages_DefaultIs1()
    {
        var ctx = new RenderContext();
        Assert.Equal(1, ctx.TotalPages);
    }

    [Fact]
    public void TotalPages_SetAndGet_Works()
    {
        var ctx = new RenderContext { TotalPages = 10 };
        Assert.Equal(10, ctx.TotalPages);
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
        var ctx = new RenderContext { FieldFormat = "N2" };
        Assert.Equal("N2", ctx.FieldFormat);
    }

    [Fact]
    public void FieldFormat_CanBeCleared()
    {
        var ctx = new RenderContext { FieldFormat = "yyyy-MM-dd" };
        ctx.FieldFormat = null;
        Assert.Null(ctx.FieldFormat);
    }

    // ============== PageWidth/PageHeight ==============

    [Fact]
    public void PageWidth_DefaultIs210()
    {
        var ctx = new RenderContext();
        Assert.Equal(210, ctx.PageWidth);
    }

    [Fact]
    public void PageHeight_DefaultIs297()
    {
        var ctx = new RenderContext();
        Assert.Equal(297, ctx.PageHeight);
    }

    [Fact]
    public void PageWidth_SetAndGet_Works()
    {
        var ctx = new RenderContext { PageWidth = 297 };
        Assert.Equal(297, ctx.PageWidth);
    }

    [Fact]
    public void PageHeight_SetAndGet_Works()
    {
        var ctx = new RenderContext { PageHeight = 210 };
        Assert.Equal(210, ctx.PageHeight);
    }

    [Fact]
    public void PageSize_A4Landscape_Works()
    {
        var ctx = new RenderContext { PageWidth = 297, PageHeight = 210 };
        Assert.Equal(297, ctx.PageWidth);
        Assert.Equal(210, ctx.PageHeight);
    }

    // ============== NestingDepth ==============

    [Fact]
    public void NestingDepth_ZeroByDefault()
    {
        var ctx = new RenderContext();
        Assert.Equal(0, ctx.NestingDepth);
    }

    [Fact]
    public void NestingDepth_SetAndGet_Works()
    {
        var ctx = new RenderContext { NestingDepth = 3 };
        Assert.Equal(3, ctx.NestingDepth);
    }

    [Fact]
    public void NestingDepth_CanBeIncremented()
    {
        var ctx = new RenderContext { NestingDepth = 0 };
        ctx.NestingDepth++;
        Assert.Equal(1, ctx.NestingDepth);
    }

    [Fact]
    public void NestingDepth_CanBeDecremented()
    {
        var ctx = new RenderContext { NestingDepth = 2 };
        ctx.NestingDepth--;
        Assert.Equal(1, ctx.NestingDepth);
    }

    [Fact]
    public void MaxNestingDepth_Is5()
    {
        Assert.Equal(5, RenderContext.MaxNestingDepth);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void RenderContext_FullSetup_Works()
    {
        var ctx = new RenderContext
        {
            DataSourceName = "orders",
            CurrentRow = new Dictionary<string, object> { { "id", 1 } },
            CurrentRowNumber = 1,
            CurrentPage = 1,
            TotalPages = 5,
            FieldFormat = "N2",
            PageWidth = 210,
            PageHeight = 297,
            NestingDepth = 0
        };
        ctx.DataSources.Add("orders", new List<Dictionary<string, object>>());

        Assert.Equal("orders", ctx.DataSourceName);
        Assert.NotNull(ctx.CurrentRow);
        Assert.Equal(1, ctx.CurrentRowNumber);
        Assert.Equal(1, ctx.CurrentPage);
        Assert.Equal(5, ctx.TotalPages);
        Assert.Equal("N2", ctx.FieldFormat);
        Assert.Single(ctx.DataSources);
    }

    [Fact]
    public void RenderContext_IterateRows_Works()
    {
        var ctx = new RenderContext { DataSourceName = "users" };
        var rows = new List<Dictionary<string, object>>
        {
            new() { { "id", 1 }, { "name", "Alice" } },
            new() { { "id", 2 }, { "name", "Bob" } },
            new() { { "id", 3 }, { "name", "Charlie" } }
        };
        ctx.DataSources.Add("users", rows);

        int count = 0;
        foreach (var row in ctx.DataSources["users"])
        {
            ctx.CurrentRow = row;
            ctx.CurrentRowNumber = ++count;
        }

        Assert.Equal(3, count);
        Assert.Equal(3, ctx.CurrentRowNumber);
        Assert.Equal("Charlie", ctx.CurrentRow!["name"]);
    }
}
