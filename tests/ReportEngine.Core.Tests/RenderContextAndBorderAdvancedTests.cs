using ReportEngine.Core;
using ReportEngine.Core.Data;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// RenderContext 高级属性测试
/// </summary>
public class RenderContextAdvancedTests
{
    // ============== PageWidth ==============

    [Fact]
    public void PageWidth_DefaultIs210()
    {
        var ctx = new RenderContext();
        Assert.Equal(210, ctx.PageWidth);
    }

    [Fact]
    public void PageWidth_SetA4_Works()
    {
        var ctx = new RenderContext { PageWidth = 210 };
        Assert.Equal(210, ctx.PageWidth);
    }

    [Fact]
    public void PageWidth_SetLetter_Works()
    {
        var ctx = new RenderContext { PageWidth = 216 };
        Assert.Equal(216, ctx.PageWidth);
    }

    [Fact]
    public void PageWidth_SetA3_Works()
    {
        var ctx = new RenderContext { PageWidth = 297 };
        Assert.Equal(297, ctx.PageWidth);
    }

    [Fact]
    public void PageWidth_SetCustom_Works()
    {
        var ctx = new RenderContext { PageWidth = 150 };
        Assert.Equal(150, ctx.PageWidth);
    }

    // ============== PageHeight ==============

    [Fact]
    public void PageHeight_DefaultIs297()
    {
        var ctx = new RenderContext();
        Assert.Equal(297, ctx.PageHeight);
    }

    [Fact]
    public void PageHeight_SetA4_Works()
    {
        var ctx = new RenderContext { PageHeight = 297 };
        Assert.Equal(297, ctx.PageHeight);
    }

    [Fact]
    public void PageHeight_SetLetter_Works()
    {
        var ctx = new RenderContext { PageHeight = 279 };
        Assert.Equal(279, ctx.PageHeight);
    }

    [Fact]
    public void PageHeight_SetA3_Works()
    {
        var ctx = new RenderContext { PageHeight = 420 };
        Assert.Equal(420, ctx.PageHeight);
    }

    [Fact]
    public void PageHeight_SetCustom_Works()
    {
        var ctx = new RenderContext { PageHeight = 200 };
        Assert.Equal(200, ctx.PageHeight);
    }

    // ============== DataSourceName ==============

    [Fact]
    public void DataSourceName_EmptyByDefault()
    {
        var ctx = new RenderContext();
        Assert.Equal("", ctx.DataSourceName);
    }

    [Fact]
    public void DataSourceName_Set_Works()
    {
        var ctx = new RenderContext { DataSourceName = "orders" };
        Assert.Equal("orders", ctx.DataSourceName);
    }

    [Fact]
    public void DataSourceName_SetDifferent_Works()
    {
        var ctx = new RenderContext { DataSourceName = "customers" };
        Assert.Equal("customers", ctx.DataSourceName);
    }

    [Fact]
    public void DataSourceName_CanBeChanged()
    {
        var ctx = new RenderContext { DataSourceName = "ds1" };
        ctx.DataSourceName = "ds2";
        Assert.Equal("ds2", ctx.DataSourceName);
    }

    // ============== CurrentRow ==============

    [Fact]
    public void CurrentRow_NullByDefault()
    {
        var ctx = new RenderContext();
        Assert.Null(ctx.CurrentRow);
    }

    [Fact]
    public void CurrentRow_Set_Works()
    {
        var ctx = new RenderContext();
        ctx.CurrentRow = new Dictionary<string, object> { ["id"] = 1, ["name"] = "test" };
        Assert.NotNull(ctx.CurrentRow);
        Assert.Equal(1, ctx.CurrentRow["id"]);
    }

    [Fact]
    public void CurrentRow_CanBeCleared()
    {
        var ctx = new RenderContext();
        ctx.CurrentRow = new Dictionary<string, object> { ["id"] = 1 };
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
    public void CurrentRowNumber_Set_Works()
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
    public void CurrentPage_Set_Works()
    {
        var ctx = new RenderContext { CurrentPage = 3 };
        Assert.Equal(3, ctx.CurrentPage);
    }

    [Fact]
    public void CurrentPage_CanBeChanged()
    {
        var ctx = new RenderContext { CurrentPage = 1 };
        ctx.CurrentPage = 2;
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
    public void TotalPages_Set_Works()
    {
        var ctx = new RenderContext { TotalPages = 10 };
        Assert.Equal(10, ctx.TotalPages);
    }

    [Fact]
    public void TotalPages_CanBeChanged()
    {
        var ctx = new RenderContext { TotalPages = 5 };
        ctx.TotalPages = 8;
        Assert.Equal(8, ctx.TotalPages);
    }

    // ============== FieldFormat ==============

    [Fact]
    public void FieldFormat_NullByDefault()
    {
        var ctx = new RenderContext();
        Assert.Null(ctx.FieldFormat);
    }

    [Fact]
    public void FieldFormat_Set_Works()
    {
        var ctx = new RenderContext { FieldFormat = "N2" };
        Assert.Equal("N2", ctx.FieldFormat);
    }

    [Fact]
    public void FieldFormat_CanBeCleared()
    {
        var ctx = new RenderContext { FieldFormat = "currency" };
        ctx.FieldFormat = null;
        Assert.Null(ctx.FieldFormat);
    }

    // ============== NestingDepth ==============

    [Fact]
    public void NestingDepth_ZeroByDefault()
    {
        var ctx = new RenderContext();
        Assert.Equal(0, ctx.NestingDepth);
    }

    [Fact]
    public void NestingDepth_Set_Works()
    {
        var ctx = new RenderContext { NestingDepth = 2 };
        Assert.Equal(2, ctx.NestingDepth);
    }

    [Fact]
    public void MaxNestingDepth_Is5()
    {
        Assert.Equal(5, RenderContext.MaxNestingDepth);
    }

    // ============== DataSources ==============

    [Fact]
    public void DataSources_EmptyByDefault()
    {
        var ctx = new RenderContext();
        Assert.NotNull(ctx.DataSources);
        Assert.Empty(ctx.DataSources);
    }

    [Fact]
    public void DataSources_Add_Works()
    {
        var ctx = new RenderContext();
        ctx.DataSources["orders"] = new List<Dictionary<string, object>>();
        Assert.Single(ctx.DataSources);
    }

    [Fact]
    public void DataSources_AddMultiple_Works()
    {
        var ctx = new RenderContext();
        ctx.DataSources["orders"] = new List<Dictionary<string, object>>();
        ctx.DataSources["customers"] = new List<Dictionary<string, object>>();
        Assert.Equal(2, ctx.DataSources.Count);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void RenderContext_A4Portrait_Works()
    {
        var ctx = new RenderContext
        {
            PageWidth = 210,
            PageHeight = 297,
            DataSourceName = "mainData"
        };

        Assert.Equal(210, ctx.PageWidth);
        Assert.Equal(297, ctx.PageHeight);
        Assert.Equal("mainData", ctx.DataSourceName);
    }

    [Fact]
    public void RenderContext_A4Landscape_Works()
    {
        var ctx = new RenderContext
        {
            PageWidth = 297,
            PageHeight = 210,
            DataSourceName = "reportData"
        };

        Assert.Equal(297, ctx.PageWidth);
        Assert.Equal(210, ctx.PageHeight);
    }

    [Fact]
    public void RenderContext_WithCurrentRow_Works()
    {
        var ctx = new RenderContext
        {
            DataSourceName = "orders",
            CurrentRowNumber = 1,
            CurrentPage = 1,
            TotalPages = 3
        };
        ctx.CurrentRow = new Dictionary<string, object>
        {
            ["id"] = 101,
            ["amount"] = 999.99
        };

        Assert.Equal("orders", ctx.DataSourceName);
        Assert.Equal(1, ctx.CurrentRowNumber);
        Assert.Equal(101, ctx.CurrentRow["id"]);
    }

    [Fact]
    public void RenderContext_CanBeModified()
    {
        var ctx = new RenderContext();
        ctx.PageWidth = 300;
        ctx.PageHeight = 400;
        ctx.DataSourceName = "newData";
        ctx.CurrentPage = 5;
        ctx.TotalPages = 10;

        Assert.Equal(300, ctx.PageWidth);
        Assert.Equal(400, ctx.PageHeight);
        Assert.Equal("newData", ctx.DataSourceName);
        Assert.Equal(5, ctx.CurrentPage);
        Assert.Equal(10, ctx.TotalPages);
    }
}

/// <summary>
/// BorderDef 高级属性测试
/// </summary>
public class BorderDefAdvancedTests
{
    // ============== Width ==============

    [Fact]
    public void Width_DefaultIs1()
    {
        var border = new BorderDef();
        Assert.Equal(1, border.Width);
    }

    [Fact]
    public void Width_Set_Works()
    {
        var border = new BorderDef { Width = 2 };
        Assert.Equal(2, border.Width);
    }

    [Fact]
    public void Width_SetThin_Works()
    {
        var border = new BorderDef { Width = 0.5 };
        Assert.Equal(0.5, border.Width);
    }

    [Fact]
    public void Width_SetThick_Works()
    {
        var border = new BorderDef { Width = 5 };
        Assert.Equal(5, border.Width);
    }

    [Fact]
    public void Width_CanBeChanged()
    {
        var border = new BorderDef { Width = 1 };
        border.Width = 3;
        Assert.Equal(3, border.Width);
    }

    // ============== Color ==============

    [Fact]
    public void Color_DefaultIsBlack()
    {
        var border = new BorderDef();
        Assert.Equal("#000000", border.Color);
    }

    [Fact]
    public void Color_SetRed_Works()
    {
        var border = new BorderDef { Color = "#FF0000" };
        Assert.Equal("#FF0000", border.Color);
    }

    [Fact]
    public void Color_SetGray_Works()
    {
        var border = new BorderDef { Color = "#CCCCCC" };
        Assert.Equal("#CCCCCC", border.Color);
    }

    [Fact]
    public void Color_CanBeChanged()
    {
        var border = new BorderDef { Color = "#000000" };
        border.Color = "#FF0000";
        Assert.Equal("#FF0000", border.Color);
    }

    // ============== Style ==============

    [Fact]
    public void Style_DefaultIsSolid()
    {
        var border = new BorderDef();
        Assert.Equal(BorderStyle.Solid, border.Style);
    }

    [Fact]
    public void Style_SetDashed_Works()
    {
        var border = new BorderDef { Style = BorderStyle.Dashed };
        Assert.Equal(BorderStyle.Dashed, border.Style);
    }

    [Fact]
    public void Style_SetDotted_Works()
    {
        var border = new BorderDef { Style = BorderStyle.Dotted };
        Assert.Equal(BorderStyle.Dotted, border.Style);
    }

    [Fact]
    public void Style_SetNone_Works()
    {
        var border = new BorderDef { Style = BorderStyle.None };
        Assert.Equal(BorderStyle.None, border.Style);
    }

    [Fact]
    public void Style_CanBeChanged()
    {
        var border = new BorderDef { Style = BorderStyle.Solid };
        border.Style = BorderStyle.Dashed;
        Assert.Equal(BorderStyle.Dashed, border.Style);
    }

    // ============== Top/Bottom/Left/Right ==============

    [Fact]
    public void Top_FalseByDefault()
    {
        var border = new BorderDef();
        Assert.False(border.Top);
    }

    [Fact]
    public void Top_SetTrue_Works()
    {
        var border = new BorderDef { Top = true };
        Assert.True(border.Top);
    }

    [Fact]
    public void Bottom_FalseByDefault()
    {
        var border = new BorderDef();
        Assert.False(border.Bottom);
    }

    [Fact]
    public void Bottom_SetTrue_Works()
    {
        var border = new BorderDef { Bottom = true };
        Assert.True(border.Bottom);
    }

    [Fact]
    public void Left_FalseByDefault()
    {
        var border = new BorderDef();
        Assert.False(border.Left);
    }

    [Fact]
    public void Left_SetTrue_Works()
    {
        var border = new BorderDef { Left = true };
        Assert.True(border.Left);
    }

    [Fact]
    public void Right_FalseByDefault()
    {
        var border = new BorderDef();
        Assert.False(border.Right);
    }

    [Fact]
    public void Right_SetTrue_Works()
    {
        var border = new BorderDef { Right = true };
        Assert.True(border.Right);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void BorderDef_AllSides_Works()
    {
        var border = new BorderDef
        {
            Width = 2,
            Color = "#333333",
            Style = BorderStyle.Solid,
            Top = true,
            Bottom = true,
            Left = true,
            Right = true
        };

        Assert.True(border.Top);
        Assert.True(border.Bottom);
        Assert.True(border.Left);
        Assert.True(border.Right);
    }

    [Fact]
    public void BorderDef_TopBottomOnly_Works()
    {
        var border = new BorderDef
        {
            Width = 1,
            Color = "#000000",
            Top = true,
            Bottom = true,
            Left = false,
            Right = false
        };

        Assert.True(border.Top);
        Assert.True(border.Bottom);
        Assert.False(border.Left);
        Assert.False(border.Right);
    }

    [Fact]
    public void BorderDef_LeftRightOnly_Works()
    {
        var border = new BorderDef
        {
            Top = false,
            Bottom = false,
            Left = true,
            Right = true
        };

        Assert.False(border.Top);
        Assert.False(border.Bottom);
        Assert.True(border.Left);
        Assert.True(border.Right);
    }

    [Fact]
    public void BorderDef_DashedStyle_Works()
    {
        var border = new BorderDef
        {
            Width = 1,
            Style = BorderStyle.Dashed,
            Color = "#666666",
            Top = true,
            Bottom = true
        };

        Assert.Equal(BorderStyle.Dashed, border.Style);
        Assert.Equal("#666666", border.Color);
    }

    [Fact]
    public void BorderDef_FullSetup_Works()
    {
        var border = new BorderDef
        {
            Width = 3,
            Color = "#FF0000",
            Style = BorderStyle.Dotted,
            Top = true,
            Bottom = true,
            Left = true,
            Right = true
        };

        Assert.Equal(3, border.Width);
        Assert.Equal("#FF0000", border.Color);
        Assert.Equal(BorderStyle.Dotted, border.Style);
        Assert.True(border.Top);
        Assert.True(border.Bottom);
        Assert.True(border.Left);
        Assert.True(border.Right);
    }
}
