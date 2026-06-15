using System;
using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ReportElement 基类完整字段测试：
///   - ReportElement 完整字段（Id/Name/GroupId/X/Y/Width/Height/BackgroundColor/Border/Visible/VisibleExpression/Locked/Rotation/Opacity/ConditionalFormats）
///   - 字段组合行为
/// </summary>
public class ReportElementBaseCompleteTests
{
    [Fact]
    public void ReportElement_Defaults()
    {
        var e = new TextElement();
        Assert.NotNull(e.Id);
        Assert.NotEmpty(e.Id);
        Assert.Null(e.Name);
        Assert.Null(e.GroupId);
        Assert.Equal(0, e.X);
        Assert.Equal(0, e.Y);
        Assert.Equal(0, e.Width);
        Assert.Equal(0, e.Height);
        Assert.Null(e.BackgroundColor);
        Assert.Null(e.Border);
        Assert.True(e.Visible);
        Assert.Null(e.VisibleExpression);
        Assert.False(e.Locked);
        Assert.Equal(0, e.Rotation);
        Assert.Equal(1.0, e.Opacity);
        Assert.NotNull(e.ConditionalFormats);
        Assert.Empty(e.ConditionalFormats);
    }

    [Fact]
    public void ReportElement_Id_IsGuidFormat()
    {
        var e = new TextElement();
        Assert.Equal(32, e.Id.Length); // Guid "N" format = 32 hex chars
    }

    [Fact]
    public void ReportElement_Id_IsUnique()
    {
        var e1 = new TextElement();
        var e2 = new TextElement();
        Assert.NotEqual(e1.Id, e2.Id);
    }

    [Fact]
    public void ReportElement_Id_CanBeOverridden()
    {
        var e = new TextElement { Id = "custom-id" };
        Assert.Equal("custom-id", e.Id);
    }

    [Fact]
    public void ReportElement_Name_CanBeNull()
    {
        var e = new TextElement { Name = null };
        Assert.Null(e.Name);
    }

    [Fact]
    public void ReportElement_Name_CanBeSet()
    {
        var e = new TextElement { Name = "标题文本" };
        Assert.Equal("标题文本", e.Name);
    }

    [Fact]
    public void ReportElement_GroupId_CanBeNull()
    {
        var e = new TextElement { GroupId = null };
        Assert.Null(e.GroupId);
    }

    [Fact]
    public void ReportElement_GroupId_CanBeSet()
    {
        var e = new TextElement { GroupId = "group1" };
        Assert.Equal("group1", e.GroupId);
    }

    [Fact]
    public void ReportElement_X_CanBeZero()
    {
        var e = new TextElement { X = 0 };
        Assert.Equal(0, e.X);
    }

    [Fact]
    public void ReportElement_X_CanBePositive()
    {
        var e = new TextElement { X = 50.5 };
        Assert.Equal(50.5, e.X);
    }

    [Fact]
    public void ReportElement_X_CanBeNegative()
    {
        var e = new TextElement { X = -10 };
        Assert.Equal(-10, e.X);
    }

    [Fact]
    public void ReportElement_Y_CanBeZero()
    {
        var e = new TextElement { Y = 0 };
        Assert.Equal(0, e.Y);
    }

    [Fact]
    public void ReportElement_Y_CanBePositive()
    {
        var e = new TextElement { Y = 100.25 };
        Assert.Equal(100.25, e.Y);
    }

    [Fact]
    public void ReportElement_Width_CanBeZero()
    {
        var e = new TextElement { Width = 0 };
        Assert.Equal(0, e.Width);
    }

    [Fact]
    public void ReportElement_Width_CanBePositive()
    {
        var e = new TextElement { Width = 80 };
        Assert.Equal(80, e.Width);
    }

    [Fact]
    public void ReportElement_Height_CanBeZero()
    {
        var e = new TextElement { Height = 0 };
        Assert.Equal(0, e.Height);
    }

    [Fact]
    public void ReportElement_Height_CanBePositive()
    {
        var e = new TextElement { Height = 20 };
        Assert.Equal(20, e.Height);
    }

    [Fact]
    public void ReportElement_BackgroundColor_CanBeNull()
    {
        var e = new TextElement { BackgroundColor = null };
        Assert.Null(e.BackgroundColor);
    }

    [Fact]
    public void ReportElement_BackgroundColor_CanBeHex()
    {
        var e = new TextElement { BackgroundColor = "#FFFF00" };
        Assert.Equal("#FFFF00", e.BackgroundColor);
    }

    [Fact]
    public void ReportElement_BackgroundColor_CanBeHex8()
    {
        var e = new TextElement { BackgroundColor = "#80FF0000" };
        Assert.Equal("#80FF0000", e.BackgroundColor);
    }

    [Fact]
    public void ReportElement_Border_CanBeNull()
    {
        var e = new TextElement { Border = null };
        Assert.Null(e.Border);
    }

    [Fact]
    public void ReportElement_Border_CanBeSet()
    {
        var e = new TextElement { Border = new BorderDef { Width = 2, Color = "#FF0000" } };
        Assert.NotNull(e.Border);
        Assert.Equal(2, e.Border.Width);
        Assert.Equal("#FF0000", e.Border.Color);
    }

    [Fact]
    public void ReportElement_Visible_DefaultTrue()
    {
        var e = new TextElement();
        Assert.True(e.Visible);
    }

    [Fact]
    public void ReportElement_Visible_CanBeFalse()
    {
        var e = new TextElement { Visible = false };
        Assert.False(e.Visible);
    }

    [Fact]
    public void ReportElement_VisibleExpression_CanBeNull()
    {
        var e = new TextElement { VisibleExpression = null };
        Assert.Null(e.VisibleExpression);
    }

    [Fact]
    public void ReportElement_VisibleExpression_CanBeExpression()
    {
        var e = new TextElement { VisibleExpression = "{{showHeader}}" };
        Assert.Equal("{{showHeader}}", e.VisibleExpression);
    }

    [Fact]
    public void ReportElement_Locked_DefaultFalse()
    {
        var e = new TextElement();
        Assert.False(e.Locked);
    }

    [Fact]
    public void ReportElement_Locked_CanBeTrue()
    {
        var e = new TextElement { Locked = true };
        Assert.True(e.Locked);
    }

    [Fact]
    public void ReportElement_Rotation_Default0()
    {
        var e = new TextElement();
        Assert.Equal(0, e.Rotation);
    }

    [Fact]
    public void ReportElement_Rotation_CanBe90()
    {
        var e = new TextElement { Rotation = 90 };
        Assert.Equal(90, e.Rotation);
    }

    [Fact]
    public void ReportElement_Rotation_CanBe180()
    {
        var e = new TextElement { Rotation = 180 };
        Assert.Equal(180, e.Rotation);
    }

    [Fact]
    public void ReportElement_Rotation_CanBe270()
    {
        var e = new TextElement { Rotation = 270 };
        Assert.Equal(270, e.Rotation);
    }

    [Fact]
    public void ReportElement_Rotation_CanBeDecimal()
    {
        var e = new TextElement { Rotation = 45.5 };
        Assert.Equal(45.5, e.Rotation);
    }

    [Fact]
    public void ReportElement_Opacity_Default1()
    {
        var e = new TextElement();
        Assert.Equal(1.0, e.Opacity);
    }

    [Fact]
    public void ReportElement_Opacity_CanBe0()
    {
        var e = new TextElement { Opacity = 0 };
        Assert.Equal(0, e.Opacity);
    }

    [Fact]
    public void ReportElement_Opacity_CanBe05()
    {
        var e = new TextElement { Opacity = 0.5 };
        Assert.Equal(0.5, e.Opacity);
    }

    [Fact]
    public void ReportElement_ConditionalFormats_CanBeEmpty()
    {
        var e = new TextElement();
        Assert.Empty(e.ConditionalFormats);
    }

    [Fact]
    public void ReportElement_ConditionalFormats_CanAddOne()
    {
        var e = new TextElement();
        e.ConditionalFormats.Add(new ConditionalFormatRule { Expression = "[Value] > 100" });
        Assert.Single(e.ConditionalFormats);
    }

    [Fact]
    public void ReportElement_ConditionalFormats_CanAddMultiple()
    {
        var e = new TextElement();
        e.ConditionalFormats.Add(new ConditionalFormatRule { Expression = "[Value] > 100", Bold = true });
        e.ConditionalFormats.Add(new ConditionalFormatRule { Expression = "[Value] < 0", FontColor = "#FF0000" });
        Assert.Equal(2, e.ConditionalFormats.Count);
    }

    [Fact]
    public void ReportElement_FullCombination()
    {
        var e = new TextElement
        {
            Name = "标题",
            GroupId = "header",
            X = 10,
            Y = 20,
            Width = 100,
            Height = 30,
            BackgroundColor = "#EEEEEE",
            Border = new BorderDef { Width = 1 },
            Visible = true,
            Locked = true,
            Rotation = 0,
            Opacity = 0.8,
        };
        e.ConditionalFormats.Add(new ConditionalFormatRule { Expression = "[Value] > 100" });

        Assert.Equal("标题", e.Name);
        Assert.Equal("header", e.GroupId);
        Assert.Equal(10, e.X);
        Assert.Equal(20, e.Y);
        Assert.Equal(100, e.Width);
        Assert.Equal(30, e.Height);
        Assert.Equal("#EEEEEE", e.BackgroundColor);
        Assert.NotNull(e.Border);
        Assert.True(e.Visible);
        Assert.True(e.Locked);
        Assert.Equal(0.8, e.Opacity);
        Assert.Single(e.ConditionalFormats);
    }
}
