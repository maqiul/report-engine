using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// PageInfo 行为测试：
///   - 默认值
///   - 尺寸设置
///   - 方向设置
///   - Margin 设置
///   - MultiUp 设置
/// </summary>
public class PageInfoBehaviorTests
{
    // ============== 默认值 ==============

    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var page = new PageInfo();

        Assert.Equal(210, page.Width);
        Assert.Equal(297, page.Height);
        Assert.Equal("portrait", page.Orientation);
        Assert.NotNull(page.Margin);
        Assert.Null(page.MultiUp);
    }

    // ============== Width/Height ==============

    [Fact]
    public void Width_SetAndGet_Works()
    {
        var page = new PageInfo();
        page.Width = 150;
        Assert.Equal(150, page.Width);
    }

    [Fact]
    public void Height_SetAndGet_Works()
    {
        var page = new PageInfo();
        page.Height = 200;
        Assert.Equal(200, page.Height);
    }

    [Fact]
    public void Size_CustomSize_Works()
    {
        var page = new PageInfo { Width = 105, Height = 148 };
        Assert.Equal(105, page.Width);
        Assert.Equal(148, page.Height);
    }

    [Fact]
    public void Size_LargeFormat_Works()
    {
        var page = new PageInfo { Width = 420, Height = 594 };
        Assert.Equal(420, page.Width);
        Assert.Equal(594, page.Height);
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

    [Fact]
    public void Orientation_SetPortrait_Works()
    {
        var page = new PageInfo { Orientation = "landscape" };
        page.Orientation = "portrait";
        Assert.Equal("portrait", page.Orientation);
    }

    [Fact]
    public void Orientation_AnyString_Accepted()
    {
        var page = new PageInfo { Orientation = "custom" };
        Assert.Equal("custom", page.Orientation);
    }

    // ============== Margin ==============

    [Fact]
    public void Margin_NotNull_ByDefault()
    {
        var page = new PageInfo();
        Assert.NotNull(page.Margin);
    }

    [Fact]
    public void Margin_SetAllSides_Works()
    {
        var page = new PageInfo
        {
            Margin = new Margin { Top = 10, Bottom = 15, Left = 12, Right = 12 }
        };
        Assert.Equal(10, page.Margin.Top);
        Assert.Equal(15, page.Margin.Bottom);
        Assert.Equal(12, page.Margin.Left);
        Assert.Equal(12, page.Margin.Right);
    }

    [Fact]
    public void Margin_ZeroMargin_Works()
    {
        var page = new PageInfo
        {
            Margin = new Margin { Top = 0, Bottom = 0, Left = 0, Right = 0 }
        };
        Assert.Equal(0, page.Margin.Top);
        Assert.Equal(0, page.Margin.Bottom);
    }

    [Fact]
    public void Margin_Asymmetric_Works()
    {
        var page = new PageInfo
        {
            Margin = new Margin { Top = 5, Bottom = 20, Left = 10, Right = 30 }
        };
        Assert.Equal(5, page.Margin.Top);
        Assert.Equal(20, page.Margin.Bottom);
        Assert.Equal(10, page.Margin.Left);
        Assert.Equal(30, page.Margin.Right);
    }

    [Fact]
    public void Margin_CanBeReplaced()
    {
        var page = new PageInfo();
        var newMargin = new Margin { Top = 25, Bottom = 25, Left = 25, Right = 25 };
        page.Margin = newMargin;
        Assert.Same(newMargin, page.Margin);
    }

    // ============== MultiUp ==============

    [Fact]
    public void MultiUp_NullByDefault()
    {
        var page = new PageInfo();
        Assert.Null(page.MultiUp);
    }

    [Fact]
    public void MultiUp_SetAndGet_Works()
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
                VSpacing = 10
            }
        };
        Assert.Equal(5, page.MultiUp.HSpacing);
        Assert.Equal(10, page.MultiUp.VSpacing);
    }

    [Fact]
    public void MultiUp_WithDirection_Works()
    {
        var page = new PageInfo
        {
            MultiUp = new MultiUpConfig
            {
                Rows = 3,
                Columns = 2,
                Direction = "Vertical"
            }
        };
        Assert.Equal("Vertical", page.MultiUp.Direction);
    }

    [Fact]
    public void MultiUp_Count_CalculatedCorrectly()
    {
        var config = new MultiUpConfig { Rows = 2, Columns = 3 };
        Assert.Equal(6, config.Count);
    }

    [Fact]
    public void MultiUp_CanBeCleared()
    {
        var page = new PageInfo { MultiUp = new MultiUpConfig { Rows = 2, Columns = 2 } };
        page.MultiUp = null;
        Assert.Null(page.MultiUp);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void PageInfo_FullSetup_Works()
    {
        var page = new PageInfo
        {
            Width = 297,
            Height = 210,
            Orientation = "landscape",
            Margin = new Margin { Top = 15, Bottom = 15, Left = 20, Right = 20 },
            MultiUp = new MultiUpConfig { Rows = 2, Columns = 2, HSpacing = 5, VSpacing = 5 }
        };

        Assert.Equal(297, page.Width);
        Assert.Equal(210, page.Height);
        Assert.Equal("landscape", page.Orientation);
        Assert.Equal(15, page.Margin.Top);
        Assert.Equal(20, page.Margin.Left);
        Assert.NotNull(page.MultiUp);
        Assert.Equal(4, page.MultiUp.Count);
    }

    [Fact]
    public void PageInfo_A4Portrait_Standard()
    {
        var page = new PageInfo
        {
            Width = 210,
            Height = 297,
            Orientation = "portrait",
            Margin = new Margin { Top = 25.4f, Bottom = 25.4f, Left = 25.4f, Right = 25.4f }
        };

        Assert.Equal(210, page.Width);
        Assert.Equal(297, page.Height);
        Assert.Equal("portrait", page.Orientation);
    }

    [Fact]
    public void PageInfo_A4Landscape_Standard()
    {
        var page = new PageInfo
        {
            Width = 297,
            Height = 210,
            Orientation = "landscape",
            Margin = new Margin { Top = 25.4f, Bottom = 25.4f, Left = 25.4f, Right = 25.4f }
        };

        Assert.Equal(297, page.Width);
        Assert.Equal(210, page.Height);
        Assert.Equal("landscape", page.Orientation);
    }

    [Fact]
    public void PageInfo_LabelPrinting_MultiUp()
    {
        var page = new PageInfo
        {
            Width = 210,
            Height = 297,
            MultiUp = new MultiUpConfig
            {
                Rows = 7,
                Columns = 3,
                HSpacing = 2,
                VSpacing = 2,
                Direction = "Horizontal"
            }
        };

        Assert.Equal(21, page.MultiUp.Count);
        Assert.Equal("Horizontal", page.MultiUp.Direction);
    }
}
