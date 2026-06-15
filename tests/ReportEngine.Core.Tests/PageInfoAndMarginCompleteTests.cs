using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// PageInfo / Margin 完整字段测试：
///   - PageInfo 完整字段（Width/Height/Unit/Orientation/Margin/MultiUp/BackgroundColor/BackgroundImage/Watermark）
///   - Margin 完整字段（Top/Bottom/Left/Right）
///   - 字段组合行为
/// </summary>
public class PageInfoAndMarginCompleteTests
{
    // ============== PageInfo ==============

    [Fact]
    public void PageInfo_Defaults()
    {
        var p = new PageInfo();
        Assert.Equal(210, p.Width);
        Assert.Equal(297, p.Height);
        Assert.Equal("mm", p.Unit);
        Assert.Equal("portrait", p.Orientation);
        Assert.NotNull(p.Margin);
        Assert.Null(p.MultiUp);
        Assert.Null(p.BackgroundColor);
        Assert.Null(p.BackgroundImage);
        Assert.Null(p.Watermark);
    }

    [Fact]
    public void PageInfo_AllSetters()
    {
        var p = new PageInfo
        {
            Width = 100,
            Height = 150,
            Unit = "inch",
            Orientation = "landscape",
            MultiUp = new MultiUpConfig { Rows = 2, Columns = 2 },
            BackgroundColor = "#F0F0F0",
            BackgroundImage = "bg.png",
            Watermark = "CONFIDENTIAL",
        };
        Assert.Equal(100, p.Width);
        Assert.Equal(150, p.Height);
        Assert.Equal("inch", p.Unit);
        Assert.Equal("landscape", p.Orientation);
        Assert.NotNull(p.MultiUp);
        Assert.Equal(2, p.MultiUp.Rows);
        Assert.Equal("#F0F0F0", p.BackgroundColor);
        Assert.Equal("bg.png", p.BackgroundImage);
        Assert.Equal("CONFIDENTIAL", p.Watermark);
    }

    [Fact]
    public void PageInfo_Width_CanBeZero()
    {
        var p = new PageInfo { Width = 0 };
        Assert.Equal(0, p.Width);
    }

    [Fact]
    public void PageInfo_Height_CanBeNegative()
    {
        var p = new PageInfo { Height = -100 };
        Assert.Equal(-100, p.Height);
    }

    [Fact]
    public void PageInfo_Unit_AcceptsAnyString()
    {
        var p = new PageInfo { Unit = "cm" };
        Assert.Equal("cm", p.Unit);
    }

    [Fact]
    public void PageInfo_Orientation_AcceptsAnyString()
    {
        var p = new PageInfo { Orientation = "custom" };
        Assert.Equal("custom", p.Orientation);
    }

    [Fact]
    public void PageInfo_Margin_CanBeReplaced()
    {
        var p = new PageInfo();
        var m = new Margin { Top = 20, Bottom = 20, Left = 15, Right = 15 };
        p.Margin = m;
        Assert.Equal(20, p.Margin.Top);
        Assert.Equal(15, p.Margin.Left);
    }

    [Fact]
    public void PageInfo_MultiUp_CanBeSet()
    {
        var p = new PageInfo();
        p.MultiUp = new MultiUpConfig { Rows = 3, Columns = 2 };
        Assert.NotNull(p.MultiUp);
        Assert.Equal(3, p.MultiUp.Rows);
        Assert.Equal(2, p.MultiUp.Columns);
    }

    [Fact]
    public void PageInfo_BackgroundColor_AcceptsAnyHex()
    {
        var p = new PageInfo { BackgroundColor = "#AABBCC" };
        Assert.Equal("#AABBCC", p.BackgroundColor);
    }

    [Fact]
    public void PageInfo_BackgroundImage_CanBeEmpty()
    {
        var p = new PageInfo { BackgroundImage = "" };
        Assert.Equal("", p.BackgroundImage);
    }

    [Fact]
    public void PageInfo_Watermark_CanBeLongText()
    {
        var p = new PageInfo { Watermark = "This is a very long watermark text for testing purposes" };
        Assert.Contains("watermark", p.Watermark);
    }

    // ============== Margin ==============

    [Fact]
    public void Margin_Defaults()
    {
        var m = new Margin();
        Assert.Equal(10, m.Top);
        Assert.Equal(10, m.Bottom);
        Assert.Equal(10, m.Left);
        Assert.Equal(10, m.Right);
    }

    [Fact]
    public void Margin_AllSetters()
    {
        var m = new Margin
        {
            Top = 20,
            Bottom = 15,
            Left = 25,
            Right = 30,
        };
        Assert.Equal(20, m.Top);
        Assert.Equal(15, m.Bottom);
        Assert.Equal(25, m.Left);
        Assert.Equal(30, m.Right);
    }

    [Fact]
    public void Margin_Top_CanBeZero()
    {
        var m = new Margin { Top = 0 };
        Assert.Equal(0, m.Top);
    }

    [Fact]
    public void Margin_Bottom_CanBeNegative()
    {
        var m = new Margin { Bottom = -5 };
        Assert.Equal(-5, m.Bottom);
    }

    [Fact]
    public void Margin_Left_CanBeLarge()
    {
        var m = new Margin { Left = 1000 };
        Assert.Equal(1000, m.Left);
    }

    [Fact]
    public void Margin_Right_CanBeDecimal()
    {
        var m = new Margin { Right = 12.5 };
        Assert.Equal(12.5, m.Right);
    }

    [Fact]
    public void Margin_AllSides_Independent()
    {
        var m = new Margin
        {
            Top = 1,
            Bottom = 2,
            Left = 3,
            Right = 4,
        };
        Assert.Equal(1, m.Top);
        Assert.Equal(2, m.Bottom);
        Assert.Equal(3, m.Left);
        Assert.Equal(4, m.Right);
    }
}
