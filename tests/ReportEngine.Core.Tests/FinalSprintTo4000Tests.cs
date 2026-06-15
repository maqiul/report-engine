using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Rendering;
using ReportEngine.Core.Parsing;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ReportElement 更多属性组合测试
/// </summary>
public class ReportElementCombinationTests
{
    [Fact]
    public void ReportElement_Name_DefaultNull()
    {
        var el = new TextElement();
        Assert.Null(el.Name);
    }

    [Fact]
    public void ReportElement_Name_SetValue()
    {
        var el = new TextElement { Name = "txtTitle" };
        Assert.Equal("txtTitle", el.Name);
    }

    [Fact]
    public void ReportElement_GroupId_DefaultNull()
    {
        var el = new TextElement();
        Assert.Null(el.GroupId);
    }

    [Fact]
    public void ReportElement_GroupId_SetValue()
    {
        var el = new TextElement { GroupId = "group1" };
        Assert.Equal("group1", el.GroupId);
    }

    [Fact]
    public void ReportElement_Visible_DefaultTrue()
    {
        var el = new TextElement();
        Assert.True(el.Visible);
    }

    [Fact]
    public void ReportElement_Visible_SetFalse()
    {
        var el = new TextElement { Visible = false };
        Assert.False(el.Visible);
    }

    [Fact]
    public void ReportElement_VisibleExpression_DefaultNull()
    {
        var el = new TextElement();
        Assert.Null(el.VisibleExpression);
    }

    [Fact]
    public void ReportElement_VisibleExpression_SetValue()
    {
        var el = new TextElement { VisibleExpression = "{{showTitle}}" };
        Assert.Equal("{{showTitle}}", el.VisibleExpression);
    }

    [Fact]
    public void ReportElement_Locked_DefaultFalse()
    {
        var el = new TextElement();
        Assert.False(el.Locked);
    }

    [Fact]
    public void ReportElement_Locked_SetTrue()
    {
        var el = new TextElement { Locked = true };
        Assert.True(el.Locked);
    }

    [Fact]
    public void ReportElement_Rotation_DefaultZero()
    {
        var el = new TextElement();
        Assert.Equal(0, el.Rotation);
    }

    [Fact]
    public void ReportElement_Rotation_Set90()
    {
        var el = new TextElement { Rotation = 90 };
        Assert.Equal(90, el.Rotation);
    }

    [Fact]
    public void ReportElement_Opacity_DefaultOne()
    {
        var el = new TextElement();
        Assert.Equal(1.0, el.Opacity);
    }

    [Fact]
    public void ReportElement_Opacity_SetHalf()
    {
        var el = new TextElement { Opacity = 0.5 };
        Assert.Equal(0.5, el.Opacity);
    }

    [Fact]
    public void ReportElement_ConditionalFormats_DefaultEmpty()
    {
        var el = new TextElement();
        Assert.NotNull(el.ConditionalFormats);
        Assert.Empty(el.ConditionalFormats);
    }

    [Fact]
    public void ReportElement_ConditionalFormats_AddItem()
    {
        var el = new TextElement();
        el.ConditionalFormats.Add(new ConditionalFormatRule { Expression = "[amount] > 1000", FontColor = "#FF0000" });
        Assert.Single(el.ConditionalFormats);
    }

    [Fact]
    public void ReportElement_BackgroundColor_DefaultNull()
    {
        var el = new TextElement();
        Assert.Null(el.BackgroundColor);
    }

    [Fact]
    public void ReportElement_BackgroundColor_SetValue()
    {
        var el = new TextElement { BackgroundColor = "#FFFF00" };
        Assert.Equal("#FFFF00", el.BackgroundColor);
    }

    [Fact]
    public void ReportElement_Border_DefaultNull()
    {
        var el = new TextElement();
        Assert.Null(el.Border);
    }

    [Fact]
    public void ReportElement_Border_SetValue()
    {
        var el = new TextElement { Border = new BorderDef { Width = 1, Color = "#000000" } };
        Assert.NotNull(el.Border);
        Assert.Equal(1, el.Border.Width);
    }
}

/// <summary>
/// Margin 更多属性测试
/// </summary>
public class MarginExtraTests
{
    [Fact]
    public void Margin_Top_Default10()
    {
        var m = new Margin();
        Assert.Equal(10, m.Top);
    }

    [Fact]
    public void Margin_Bottom_Default10()
    {
        var m = new Margin();
        Assert.Equal(10, m.Bottom);
    }

    [Fact]
    public void Margin_Left_Default10()
    {
        var m = new Margin();
        Assert.Equal(10, m.Left);
    }

    [Fact]
    public void Margin_Right_Default10()
    {
        var m = new Margin();
        Assert.Equal(10, m.Right);
    }

    [Fact]
    public void Margin_SetAll()
    {
        var m = new Margin { Top = 20, Bottom = 20, Left = 15, Right = 15 };
        Assert.Equal(20, m.Top);
        Assert.Equal(20, m.Bottom);
        Assert.Equal(15, m.Left);
        Assert.Equal(15, m.Right);
    }

    [Fact]
    public void Margin_Asymmetric()
    {
        var m = new Margin { Top = 30, Bottom = 10, Left = 20, Right = 5 };
        Assert.Equal(30, m.Top);
        Assert.Equal(10, m.Bottom);
        Assert.Equal(20, m.Left);
        Assert.Equal(5, m.Right);
    }
}

/// <summary>
/// MultiUpConfig 更多属性测试 2
/// </summary>
public class MultiUpConfigExtra2Tests
{
    [Fact]
    public void MultiUpConfig_Rows_Default2()
    {
        var c = new MultiUpConfig();
        Assert.Equal(2, c.Rows);
    }

    [Fact]
    public void MultiUpConfig_Columns_Default2()
    {
        var c = new MultiUpConfig();
        Assert.Equal(2, c.Columns);
    }

    [Fact]
    public void MultiUpConfig_Direction_DefaultHorizontal()
    {
        var c = new MultiUpConfig();
        Assert.Equal("Horizontal", c.Direction);
    }

    [Fact]
    public void MultiUpConfig_Direction_Vertical()
    {
        var c = new MultiUpConfig { Direction = "Vertical" };
        Assert.Equal("Vertical", c.Direction);
    }

    [Fact]
    public void MultiUpConfig_Count_CalculatedProperty()
    {
        var c = new MultiUpConfig { Rows = 3, Columns = 4 };
        Assert.Equal(12, c.Count);
    }

    [Fact]
    public void MultiUpConfig_Count_Default4()
    {
        var c = new MultiUpConfig();
        Assert.Equal(4, c.Count); // 2x2
    }

    [Fact]
    public void MultiUpConfig_CustomSetup()
    {
        var c = new MultiUpConfig { Rows = 2, Columns = 3, Direction = "Vertical" };
        Assert.Equal(2, c.Rows);
        Assert.Equal(3, c.Columns);
        Assert.Equal("Vertical", c.Direction);
        Assert.Equal(6, c.Count);
    }
}

/// <summary>
/// PageInfo 更多属性测试 2
/// </summary>
public class PageInfoExtra2Tests
{
    [Fact]
    public void PageInfo_Width_Default210()
    {
        var p = new PageInfo();
        Assert.Equal(210, p.Width);
    }

    [Fact]
    public void PageInfo_Height_Default297()
    {
        var p = new PageInfo();
        Assert.Equal(297, p.Height);
    }

    [Fact]
    public void PageInfo_Unit_DefaultMM()
    {
        var p = new PageInfo();
        Assert.Equal("mm", p.Unit);
    }

    [Fact]
    public void PageInfo_Orientation_DefaultPortrait()
    {
        var p = new PageInfo();
        Assert.Equal("portrait", p.Orientation);
    }

    [Fact]
    public void PageInfo_Orientation_Landscape()
    {
        var p = new PageInfo { Orientation = "landscape" };
        Assert.Equal("landscape", p.Orientation);
    }

    [Fact]
    public void PageInfo_Margin_DefaultNotNull()
    {
        var p = new PageInfo();
        Assert.NotNull(p.Margin);
    }

    [Fact]
    public void PageInfo_Margin_DefaultValues()
    {
        var p = new PageInfo();
        Assert.Equal(10, p.Margin.Top);
        Assert.Equal(10, p.Margin.Bottom);
        Assert.Equal(10, p.Margin.Left);
        Assert.Equal(10, p.Margin.Right);
    }

    [Fact]
    public void PageInfo_CustomSize()
    {
        var p = new PageInfo { Width = 297, Height = 420, Unit = "mm", Orientation = "landscape" };
        Assert.Equal(297, p.Width);
        Assert.Equal(420, p.Height);
        Assert.Equal("landscape", p.Orientation);
    }
}
