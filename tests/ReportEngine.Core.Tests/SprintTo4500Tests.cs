using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Rendering;
using Xunit;

namespace ReportEngine.Core.Tests;

// ─────────────────────────────────────────────────────────────────────────────
// PageInfo 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class PageInfoCompleteTests
{
    [Fact]
    public void PageInfo_Width_DefaultA4()
    {
        var p = new PageInfo();
        Assert.Equal(210, p.Width);
    }

    [Fact]
    public void PageInfo_Height_DefaultA4()
    {
        var p = new PageInfo();
        Assert.Equal(297, p.Height);
    }

    [Fact]
    public void PageInfo_Orientation_DefaultPortrait()
    {
        var p = new PageInfo();
        Assert.Equal("portrait", p.Orientation);
    }

    [Fact]
    public void PageInfo_Orientation_SetLandscape()
    {
        var p = new PageInfo { Orientation = "Landscape" };
        Assert.Equal("Landscape", p.Orientation);
    }

    [Fact]
    public void PageInfo_Margin_DefaultNotNull()
    {
        var p = new PageInfo();
        Assert.NotNull(p.Margin);
    }

    [Fact]
    public void PageInfo_Margin_TopDefault10()
    {
        var p = new PageInfo();
        Assert.Equal(10, p.Margin.Top);
    }

    [Fact]
    public void PageInfo_Margin_BottomDefault10()
    {
        var p = new PageInfo();
        Assert.Equal(10, p.Margin.Bottom);
    }

    [Fact]
    public void PageInfo_Margin_LeftDefault10()
    {
        var p = new PageInfo();
        Assert.Equal(10, p.Margin.Left);
    }

    [Fact]
    public void PageInfo_Margin_RightDefault10()
    {
        var p = new PageInfo();
        Assert.Equal(10, p.Margin.Right);
    }

    [Fact]
    public void PageInfo_Margin_SetCustomValues()
    {
        var p = new PageInfo { Margin = new Margin { Top = 20, Bottom = 15, Left = 25, Right = 30 } };
        Assert.Equal(20, p.Margin.Top);
        Assert.Equal(15, p.Margin.Bottom);
        Assert.Equal(25, p.Margin.Left);
        Assert.Equal(30, p.Margin.Right);
    }

    [Fact]
    public void PageInfo_MultiUp_DefaultNull()
    {
        var p = new PageInfo();
        Assert.Null(p.MultiUp);
    }

    [Fact]
    public void PageInfo_MultiUp_SetValue()
    {
        var p = new PageInfo { MultiUp = new MultiUpConfig() };
        Assert.NotNull(p.MultiUp);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// MultiUpConfig 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class MultiUpConfigComplete2Tests
{
    [Fact]
    public void MultiUpConfig_Rows_Default2()
    {
        var m = new MultiUpConfig();
        Assert.Equal(2, m.Rows);
    }

    [Fact]
    public void MultiUpConfig_Columns_Default2()
    {
        var m = new MultiUpConfig();
        Assert.Equal(2, m.Columns);
    }

    [Fact]
    public void MultiUpConfig_Direction_DefaultHorizontal()
    {
        var m = new MultiUpConfig();
        Assert.Equal("Horizontal", m.Direction);
    }

    [Fact]
    public void MultiUpConfig_Direction_SetVertical()
    {
        var m = new MultiUpConfig { Direction = "Vertical" };
        Assert.Equal("Vertical", m.Direction);
    }

    [Fact]
    public void MultiUpConfig_Count_Calculated()
    {
        var m = new MultiUpConfig { Rows = 3, Columns = 4 };
        Assert.Equal(12, m.Count);
    }

    [Fact]
    public void MultiUpConfig_Count_Default4()
    {
        var m = new MultiUpConfig();
        Assert.Equal(4, m.Count); // 2x2
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// FontDef 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class FontDefComplete3Tests
{
    [Fact]
    public void FontDef_Family_DefaultSimSun()
    {
        var f = new FontDef();
        Assert.Equal("SimSun", f.Family);
    }

    [Fact]
    public void FontDef_Size_Default10()
    {
        var f = new FontDef();
        Assert.Equal(10, f.Size);
    }

    [Fact]
    public void FontDef_Color_DefaultNull()
    {
        var f = new FontDef();
        Assert.Null(f.Color);
    }

    [Fact]
    public void FontDef_Color_SetValue()
    {
        var f = new FontDef { Color = "#FF0000" };
        Assert.Equal("#FF0000", f.Color);
    }

    [Fact]
    public void FontDef_Bold_DefaultFalse()
    {
        var f = new FontDef();
        Assert.False(f.Bold);
    }

    [Fact]
    public void FontDef_Bold_SetTrue()
    {
        var f = new FontDef { Bold = true };
        Assert.True(f.Bold);
    }

    [Fact]
    public void FontDef_Italic_DefaultFalse()
    {
        var f = new FontDef();
        Assert.False(f.Italic);
    }

    [Fact]
    public void FontDef_Underline_DefaultFalse()
    {
        var f = new FontDef();
        Assert.False(f.Underline);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// BorderDef 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class BorderDefComplete3Tests
{
    [Fact]
    public void BorderDef_Width_Default1()
    {
        var b = new BorderDef();
        Assert.Equal(1, b.Width);
    }

    [Fact]
    public void BorderDef_Color_DefaultBlack()
    {
        var b = new BorderDef();
        Assert.Equal("#000000", b.Color);
    }

    [Fact]
    public void BorderDef_Style_DefaultSolid()
    {
        var b = new BorderDef();
        Assert.Equal(BorderStyle.Solid, b.Style);
    }

    [Fact]
    public void BorderDef_Top_DefaultFalse()
    {
        var b = new BorderDef();
        Assert.False(b.Top);
    }

    [Fact]
    public void BorderDef_Bottom_DefaultFalse()
    {
        var b = new BorderDef();
        Assert.False(b.Bottom);
    }

    [Fact]
    public void BorderDef_Left_DefaultFalse()
    {
        var b = new BorderDef();
        Assert.False(b.Left);
    }

    [Fact]
    public void BorderDef_Right_DefaultFalse()
    {
        var b = new BorderDef();
        Assert.False(b.Right);
    }

    [Fact]
    public void BorderDef_AllSides_SetTrue()
    {
        var b = new BorderDef { Top = true, Bottom = true, Left = true, Right = true };
        Assert.True(b.Top && b.Bottom && b.Left && b.Right);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// DataSourceDef 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class DataSourceDefComplete3Tests
{
    [Fact]
    public void DataSourceDef_Name_DefaultEmpty()
    {
        var d = new DataSourceDef();
        Assert.Equal("", d.Name);
    }

    [Fact]
    public void DataSourceDef_Name_SetValue()
    {
        var d = new DataSourceDef { Name = "orders" };
        Assert.Equal("orders", d.Name);
    }

    [Fact]
    public void DataSourceDef_Fields_DefaultEmpty()
    {
        var d = new DataSourceDef();
        Assert.NotNull(d.Fields);
        Assert.Empty(d.Fields);
    }

    [Fact]
    public void DataSourceDef_Fields_AddItem()
    {
        var d = new DataSourceDef();
        d.Fields.Add(new FieldDef { Name = "id", Type = "int" });
        Assert.Single(d.Fields);
    }

    [Fact]
    public void DataSourceDef_ConnectionString_DefaultNull()
    {
        var d = new DataSourceDef();
        Assert.Null(d.ConnectionString);
    }

    [Fact]
    public void DataSourceDef_Query_DefaultNull()
    {
        var d = new DataSourceDef();
        Assert.Null(d.Query);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// FieldDef 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class FieldDefComplete2Tests
{
    [Fact]
    public void FieldDef_Name_DefaultEmpty()
    {
        var f = new FieldDef();
        Assert.Equal("", f.Name);
    }

    [Fact]
    public void FieldDef_Type_DefaultString()
    {
        var f = new FieldDef();
        Assert.Equal("string", f.Type);
    }

    [Fact]
    public void FieldDef_Type_SetInt()
    {
        var f = new FieldDef { Type = "int" };
        Assert.Equal("int", f.Type);
    }

    [Fact]
    public void FieldDef_Type_SetDecimal()
    {
        var f = new FieldDef { Type = "decimal" };
        Assert.Equal("decimal", f.Type);
    }

    [Fact]
    public void FieldDef_Type_SetDateTime()
    {
        var f = new FieldDef { Type = "datetime" };
        Assert.Equal("datetime", f.Type);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// ConditionalFormatRule 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class ConditionalFormatRuleComplete2Tests
{
    [Fact]
    public void ConditionalFormatRule_Expression_DefaultEmpty()
    {
        var c = new ConditionalFormatRule();
        Assert.Equal("", c.Expression);
    }

    [Fact]
    public void ConditionalFormatRule_BackgroundColor_DefaultNull()
    {
        var c = new ConditionalFormatRule();
        Assert.Null(c.BackgroundColor);
    }

    [Fact]
    public void ConditionalFormatRule_FontColor_DefaultNull()
    {
        var c = new ConditionalFormatRule();
        Assert.Null(c.FontColor);
    }

    [Fact]
    public void ConditionalFormatRule_Bold_DefaultFalse()
    {
        var c = new ConditionalFormatRule();
        Assert.False(c.Bold);
    }

    [Fact]
    public void ConditionalFormatRule_AllProperties_Set()
    {
        var c = new ConditionalFormatRule
        {
            Expression = "value > 100",
            BackgroundColor = "#FF0000",
            FontColor = "#FFFFFF",
            Bold = true
        };
        Assert.Equal("value > 100", c.Expression);
        Assert.Equal("#FF0000", c.BackgroundColor);
        Assert.Equal("#FFFFFF", c.FontColor);
        Assert.True(c.Bold);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// RenderContext 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class RenderContextComplete2Tests
{
    [Fact]
    public void RenderContext_PageWidth_Default210()
    {
        var r = new RenderContext();
        Assert.Equal(210, r.PageWidth);
    }

    [Fact]
    public void RenderContext_PageHeight_Default297()
    {
        var r = new RenderContext();
        Assert.Equal(297, r.PageHeight);
    }

    [Fact]
    public void RenderContext_CurrentPage_Default1()
    {
        var r = new RenderContext();
        Assert.Equal(1, r.CurrentPage);
    }

    [Fact]
    public void RenderContext_TotalPages_Default1()
    {
        var r = new RenderContext();
        Assert.Equal(1, r.TotalPages);
    }

    [Fact]
    public void RenderContext_DataSourceName_DefaultEmpty()
    {
        var r = new RenderContext();
        Assert.Equal("", r.DataSourceName);
    }

    [Fact]
    public void RenderContext_CurrentRow_DefaultNull()
    {
        var r = new RenderContext();
        Assert.Null(r.CurrentRow);
    }

    [Fact]
    public void RenderContext_DataSources_DefaultEmpty()
    {
        var r = new RenderContext();
        Assert.NotNull(r.DataSources);
        Assert.Empty(r.DataSources);
    }

    [Fact]
    public void RenderContext_MaxNestingDepth_Constant5()
    {
        Assert.Equal(5, RenderContext.MaxNestingDepth);
    }
}
