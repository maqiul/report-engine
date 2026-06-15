using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// PageInfo 高级属性测试
/// </summary>
public class PageInfoAdvancedTests
{
    // ============== Unit ==============

    [Fact]
    public void Unit_DefaultIsMm()
    {
        var page = new PageInfo();
        Assert.Equal("mm", page.Unit);
    }

    [Fact]
    public void Unit_SetInch_Works()
    {
        var page = new PageInfo { Unit = "inch" };
        Assert.Equal("inch", page.Unit);
    }

    // ============== Orientation ==============

    [Fact]
    public void Orientation_DefaultIsPortrait()
    {
        var page = new PageInfo();
        Assert.Equal("portrait", page.Orientation);
    }

    [Fact]
    public void Orientation_SetLandscape_Works()
    {
        var page = new PageInfo { Orientation = "landscape" };
        Assert.Equal("landscape", page.Orientation);
    }

    // ============== BackgroundColor ==============

    [Fact]
    public void BackgroundColor_NullByDefault()
    {
        var page = new PageInfo();
        Assert.Null(page.BackgroundColor);
    }

    [Fact]
    public void BackgroundColor_Set_Works()
    {
        var page = new PageInfo { BackgroundColor = "#F0F0F0" };
        Assert.Equal("#F0F0F0", page.BackgroundColor);
    }

    // ============== BackgroundImage ==============

    [Fact]
    public void BackgroundImage_NullByDefault()
    {
        var page = new PageInfo();
        Assert.Null(page.BackgroundImage);
    }

    [Fact]
    public void BackgroundImage_Set_Works()
    {
        var page = new PageInfo { BackgroundImage = "letterhead.png" };
        Assert.Equal("letterhead.png", page.BackgroundImage);
    }

    // ============== Watermark ==============

    [Fact]
    public void Watermark_NullByDefault()
    {
        var page = new PageInfo();
        Assert.Null(page.Watermark);
    }

    [Fact]
    public void Watermark_Set_Works()
    {
        var page = new PageInfo { Watermark = "CONFIDENTIAL" };
        Assert.Equal("CONFIDENTIAL", page.Watermark);
    }

    // ============== MultiUp ==============

    [Fact]
    public void MultiUp_NullByDefault()
    {
        var page = new PageInfo();
        Assert.Null(page.MultiUp);
    }

    [Fact]
    public void MultiUp_Set_Works()
    {
        var page = new PageInfo
        {
            MultiUp = new MultiUpConfig { Rows = 2, Columns = 3 }
        };
        Assert.NotNull(page.MultiUp);
        Assert.Equal(2, page.MultiUp.Rows);
        Assert.Equal(3, page.MultiUp.Columns);
    }

    [Fact]
    public void MultiUp_WithSpacing_Works()
    {
        var page = new PageInfo
        {
            MultiUp = new MultiUpConfig
            {
                Rows = 2,
                Columns = 2,
                HSpacing = 5,
                VSpacing = 3
            }
        };
        Assert.Equal(5, page.MultiUp.HSpacing);
        Assert.Equal(3, page.MultiUp.VSpacing);
    }

    [Fact]
    public void MultiUp_VerticalDirection_Works()
    {
        var page = new PageInfo
        {
            MultiUp = new MultiUpConfig { Direction = "Vertical" }
        };
        Assert.Equal("Vertical", page.MultiUp.Direction);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void PageInfo_LandscapeA4_Works()
    {
        var page = new PageInfo
        {
            Width = 297,
            Height = 210,
            Orientation = "landscape"
        };
        Assert.Equal(297, page.Width);
        Assert.Equal(210, page.Height);
        Assert.Equal("landscape", page.Orientation);
    }

    [Fact]
    public void PageInfo_Letterhead_Works()
    {
        var page = new PageInfo
        {
            BackgroundImage = "company-letterhead.png",
            Watermark = "DRAFT"
        };
        Assert.Equal("company-letterhead.png", page.BackgroundImage);
        Assert.Equal("DRAFT", page.Watermark);
    }

    [Fact]
    public void PageInfo_LabelPrint_Works()
    {
        var page = new PageInfo
        {
            Width = 210,
            Height = 297,
            MultiUp = new MultiUpConfig
            {
                Rows = 5,
                Columns = 2,
                HSpacing = 2,
                VSpacing = 1
            }
        };
        Assert.Equal(10, page.MultiUp.Count); // 5 * 2
    }

    [Fact]
    public void PageInfo_FullSetup_Works()
    {
        var page = new PageInfo
        {
            Width = 210,
            Height = 297,
            Unit = "mm",
            Orientation = "portrait",
            BackgroundColor = "#FFFFFF",
            Watermark = "SAMPLE"
        };
        page.Margin.Top = 15;
        page.Margin.Bottom = 15;
        page.Margin.Left = 20;
        page.Margin.Right = 20;

        Assert.Equal("mm", page.Unit);
        Assert.Equal("portrait", page.Orientation);
        Assert.Equal("#FFFFFF", page.BackgroundColor);
        Assert.Equal("SAMPLE", page.Watermark);
        Assert.Equal(15, page.Margin.Top);
        Assert.Equal(20, page.Margin.Left);
    }
}
