using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// PageInfo Schema 完整字段测试：
///   - PageInfo 完整字段（Width/Height/Unit/Orientation/Margin/MultiUp/BackgroundColor/BackgroundImage/Watermark）
///   - 字段组合行为
/// </summary>
public class PageInfoSchemaCompleteTests
{
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
            Width = 297,
            Height = 420,
            Unit = "mm",
            Orientation = "landscape",
            Margin = new Margin { Top = 15, Bottom = 15, Left = 20, Right = 20 },
            MultiUp = new MultiUpConfig { Rows = 2, Columns = 2 },
            BackgroundColor = "#FFFFFF",
            BackgroundImage = "bg.png",
            Watermark = "DRAFT",
        };
        Assert.Equal(297, p.Width);
        Assert.Equal(420, p.Height);
        Assert.Equal("landscape", p.Orientation);
        Assert.Equal(15, p.Margin.Top);
        Assert.NotNull(p.MultiUp);
        Assert.Equal("DRAFT", p.Watermark);
    }

    [Fact]
    public void PageInfo_Width_Default210()
    {
        var p = new PageInfo();
        Assert.Equal(210, p.Width);
    }

    [Fact]
    public void PageInfo_Width_CanBeSet()
    {
        var p = new PageInfo { Width = 297 };
        Assert.Equal(297, p.Width);
    }

    [Fact]
    public void PageInfo_Width_CanBeDecimal()
    {
        var p = new PageInfo { Width = 210.5 };
        Assert.Equal(210.5, p.Width);
    }

    [Fact]
    public void PageInfo_Height_Default297()
    {
        var p = new PageInfo();
        Assert.Equal(297, p.Height);
    }

    [Fact]
    public void PageInfo_Height_CanBeSet()
    {
        var p = new PageInfo { Height = 420 };
        Assert.Equal(420, p.Height);
    }

    [Fact]
    public void PageInfo_Height_CanBeDecimal()
    {
        var p = new PageInfo { Height = 297.5 };
        Assert.Equal(297.5, p.Height);
    }

    [Fact]
    public void PageInfo_Unit_DefaultMm()
    {
        var p = new PageInfo();
        Assert.Equal("mm", p.Unit);
    }

    [Fact]
    public void PageInfo_Unit_CanBeInch()
    {
        var p = new PageInfo { Unit = "in" };
        Assert.Equal("in", p.Unit);
    }

    [Fact]
    public void PageInfo_Unit_CanBeCm()
    {
        var p = new PageInfo { Unit = "cm" };
        Assert.Equal("cm", p.Unit);
    }

    [Fact]
    public void PageInfo_Orientation_DefaultPortrait()
    {
        var p = new PageInfo();
        Assert.Equal("portrait", p.Orientation);
    }

    [Fact]
    public void PageInfo_Orientation_CanBeLandscape()
    {
        var p = new PageInfo { Orientation = "landscape" };
        Assert.Equal("landscape", p.Orientation);
    }

    [Fact]
    public void PageInfo_Orientation_CanBeAnyString()
    {
        var p = new PageInfo { Orientation = "custom" };
        Assert.Equal("custom", p.Orientation);
    }

    [Fact]
    public void PageInfo_Margin_DefaultNotNull()
    {
        var p = new PageInfo();
        Assert.NotNull(p.Margin);
        Assert.Equal(10, p.Margin.Top);
    }

    [Fact]
    public void PageInfo_Margin_CanBeReplaced()
    {
        var p = new PageInfo { Margin = new Margin { Top = 20, Bottom = 20, Left = 15, Right = 15 } };
        Assert.Equal(20, p.Margin.Top);
        Assert.Equal(15, p.Margin.Left);
    }

    [Fact]
    public void PageInfo_MultiUp_CanBeNull()
    {
        var p = new PageInfo { MultiUp = null };
        Assert.Null(p.MultiUp);
    }

    [Fact]
    public void PageInfo_MultiUp_CanBeSet()
    {
        var p = new PageInfo { MultiUp = new MultiUpConfig { Rows = 3, Columns = 2 } };
        Assert.NotNull(p.MultiUp);
        Assert.Equal(3, p.MultiUp.Rows);
        Assert.Equal(2, p.MultiUp.Columns);
    }

    [Fact]
    public void PageInfo_BackgroundColor_CanBeNull()
    {
        var p = new PageInfo { BackgroundColor = null };
        Assert.Null(p.BackgroundColor);
    }

    [Fact]
    public void PageInfo_BackgroundColor_CanBeHex()
    {
        var p = new PageInfo { BackgroundColor = "#F5F5F5" };
        Assert.Equal("#F5F5F5", p.BackgroundColor);
    }

    [Fact]
    public void PageInfo_BackgroundColor_CanBeHex8()
    {
        var p = new PageInfo { BackgroundColor = "#80FFFFFF" };
        Assert.Equal("#80FFFFFF", p.BackgroundColor);
    }

    [Fact]
    public void PageInfo_BackgroundImage_CanBeNull()
    {
        var p = new PageInfo { BackgroundImage = null };
        Assert.Null(p.BackgroundImage);
    }

    [Fact]
    public void PageInfo_BackgroundImage_CanBePath()
    {
        var p = new PageInfo { BackgroundImage = "images/bg.png" };
        Assert.Equal("images/bg.png", p.BackgroundImage);
    }

    [Fact]
    public void PageInfo_BackgroundImage_CanBeUrl()
    {
        var p = new PageInfo { BackgroundImage = "https://example.com/bg.jpg" };
        Assert.Equal("https://example.com/bg.jpg", p.BackgroundImage);
    }

    [Fact]
    public void PageInfo_Watermark_CanBeNull()
    {
        var p = new PageInfo { Watermark = null };
        Assert.Null(p.Watermark);
    }

    [Fact]
    public void PageInfo_Watermark_CanBeEmpty()
    {
        var p = new PageInfo { Watermark = "" };
        Assert.Equal("", p.Watermark);
    }

    [Fact]
    public void PageInfo_Watermark_CanBeText()
    {
        var p = new PageInfo { Watermark = "CONFIDENTIAL" };
        Assert.Equal("CONFIDENTIAL", p.Watermark);
    }

    [Fact]
    public void PageInfo_Watermark_CanBeChinese()
    {
        var p = new PageInfo { Watermark = "草稿" };
        Assert.Equal("草稿", p.Watermark);
    }

    [Fact]
    public void PageInfo_A4Portrait()
    {
        var p = new PageInfo { Width = 210, Height = 297, Orientation = "portrait" };
        Assert.True(p.Height > p.Width);
    }

    [Fact]
    public void PageInfo_A4Landscape()
    {
        var p = new PageInfo { Width = 297, Height = 210, Orientation = "landscape" };
        Assert.True(p.Width > p.Height);
    }

    [Fact]
    public void PageInfo_LetterSize()
    {
        var p = new PageInfo { Width = 215.9, Height = 279.4 };
        Assert.Equal(215.9, p.Width);
        Assert.Equal(279.4, p.Height);
    }

    [Fact]
    public void PageInfo_FullCombination()
    {
        var p = new PageInfo
        {
            Width = 297,
            Height = 420,
            Unit = "mm",
            Orientation = "landscape",
            Margin = new Margin { Top = 25, Bottom = 25, Left = 20, Right = 20 },
            MultiUp = new MultiUpConfig { Rows = 2, Columns = 3 },
            BackgroundColor = "#FFFFFF",
            BackgroundImage = "bg.png",
            Watermark = "DRAFT",
        };
        Assert.Equal(297, p.Width);
        Assert.Equal(420, p.Height);
        Assert.Equal("landscape", p.Orientation);
        Assert.Equal(25, p.Margin.Top);
        Assert.NotNull(p.MultiUp);
        Assert.Equal(6, p.MultiUp.Count);
        Assert.Equal("DRAFT", p.Watermark);
    }
}
