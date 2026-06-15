using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// GroupDef 高级属性测试
/// </summary>
public class GroupDefAdvancedTests
{
    // ============== Expression ==============

    [Fact]
    public void Expression_EmptyByDefault()
    {
        var group = new GroupDef();
        Assert.Equal("", group.Expression);
    }

    [Fact]
    public void Expression_SetField_Works()
    {
        var group = new GroupDef { Expression = "[Category]" };
        Assert.Equal("[Category]", group.Expression);
    }

    [Fact]
    public void Expression_SetComplex_Works()
    {
        var group = new GroupDef { Expression = "Year([OrderDate])" };
        Assert.Contains("Year", group.Expression);
    }

    [Fact]
    public void Expression_CanBeChanged()
    {
        var group = new GroupDef { Expression = "[A]" };
        group.Expression = "[B]";
        Assert.Equal("[B]", group.Expression);
    }

    // ============== KeepTogether ==============

    [Fact]
    public void KeepTogether_TrueByDefault()
    {
        var group = new GroupDef();
        Assert.True(group.KeepTogether);
    }

    [Fact]
    public void KeepTogether_SetFalse_Works()
    {
        var group = new GroupDef { KeepTogether = false };
        Assert.False(group.KeepTogether);
    }

    [Fact]
    public void KeepTogether_CanBeToggled()
    {
        var group = new GroupDef { KeepTogether = true };
        group.KeepTogether = false;
        Assert.False(group.KeepTogether);
        group.KeepTogether = true;
        Assert.True(group.KeepTogether);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void GroupDef_CategoryGroup_Works()
    {
        var group = new GroupDef
        {
            Expression = "[Category]",
            KeepTogether = true
        };

        Assert.Equal("[Category]", group.Expression);
        Assert.True(group.KeepTogether);
    }

    [Fact]
    public void GroupDef_DateGroup_Works()
    {
        var group = new GroupDef
        {
            Expression = "Year([OrderDate])",
            KeepTogether = false
        };

        Assert.Contains("Year", group.Expression);
        Assert.False(group.KeepTogether);
    }

    [Fact]
    public void GroupDef_FullSetup_Works()
    {
        var group = new GroupDef
        {
            Expression = "[Department]",
            KeepTogether = true
        };

        Assert.Equal("[Department]", group.Expression);
        Assert.True(group.KeepTogether);
    }
}

/// <summary>
/// MultiUpConfig 高级属性测试
/// </summary>
public class MultiUpConfigAdvancedTests
{
    // ============== Rows ==============

    [Fact]
    public void Rows_DefaultIs2()
    {
        var config = new MultiUpConfig();
        Assert.Equal(2, config.Rows);
    }

    [Fact]
    public void Rows_Set_Works()
    {
        var config = new MultiUpConfig { Rows = 3 };
        Assert.Equal(3, config.Rows);
    }

    [Fact]
    public void Rows_SetLarge_Works()
    {
        var config = new MultiUpConfig { Rows = 10 };
        Assert.Equal(10, config.Rows);
    }

    [Fact]
    public void Rows_CanBeChanged()
    {
        var config = new MultiUpConfig { Rows = 2 };
        config.Rows = 4;
        Assert.Equal(4, config.Rows);
    }

    // ============== Columns ==============

    [Fact]
    public void Columns_DefaultIs2()
    {
        var config = new MultiUpConfig();
        Assert.Equal(2, config.Columns);
    }

    [Fact]
    public void Columns_Set_Works()
    {
        var config = new MultiUpConfig { Columns = 3 };
        Assert.Equal(3, config.Columns);
    }

    [Fact]
    public void Columns_SetLarge_Works()
    {
        var config = new MultiUpConfig { Columns = 5 };
        Assert.Equal(5, config.Columns);
    }

    [Fact]
    public void Columns_CanBeChanged()
    {
        var config = new MultiUpConfig { Columns = 2 };
        config.Columns = 4;
        Assert.Equal(4, config.Columns);
    }

    // ============== Direction ==============

    [Fact]
    public void Direction_DefaultIsHorizontal()
    {
        var config = new MultiUpConfig();
        Assert.Equal("Horizontal", config.Direction);
    }

    [Fact]
    public void Direction_SetVertical_Works()
    {
        var config = new MultiUpConfig { Direction = "Vertical" };
        Assert.Equal("Vertical", config.Direction);
    }

    [Fact]
    public void Direction_SetHorizontal_Works()
    {
        var config = new MultiUpConfig { Direction = "Horizontal" };
        Assert.Equal("Horizontal", config.Direction);
    }

    [Fact]
    public void Direction_CanBeChanged()
    {
        var config = new MultiUpConfig { Direction = "Horizontal" };
        config.Direction = "Vertical";
        Assert.Equal("Vertical", config.Direction);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void MultiUpConfig_2x2Grid_Works()
    {
        var config = new MultiUpConfig
        {
            Rows = 2,
            Columns = 2,
            Direction = "Horizontal"
        };

        Assert.Equal(2, config.Rows);
        Assert.Equal(2, config.Columns);
        Assert.Equal("Horizontal", config.Direction);
    }

    [Fact]
    public void MultiUpConfig_3x3Grid_Works()
    {
        var config = new MultiUpConfig
        {
            Rows = 3,
            Columns = 3,
            Direction = "Vertical"
        };

        Assert.Equal(3, config.Rows);
        Assert.Equal(3, config.Columns);
        Assert.Equal("Vertical", config.Direction);
    }

    [Fact]
    public void MultiUpConfig_1x4Grid_Works()
    {
        var config = new MultiUpConfig
        {
            Rows = 1,
            Columns = 4,
            Direction = "Horizontal"
        };

        Assert.Equal(1, config.Rows);
        Assert.Equal(4, config.Columns);
    }

    [Fact]
    public void MultiUpConfig_4x1Grid_Works()
    {
        var config = new MultiUpConfig
        {
            Rows = 4,
            Columns = 1,
            Direction = "Vertical"
        };

        Assert.Equal(4, config.Rows);
        Assert.Equal(1, config.Columns);
    }

    [Fact]
    public void MultiUpConfig_FullSetup_Works()
    {
        var config = new MultiUpConfig
        {
            Rows = 5,
            Columns = 3,
            Direction = "Horizontal"
        };

        Assert.Equal(5, config.Rows);
        Assert.Equal(3, config.Columns);
        Assert.Equal("Horizontal", config.Direction);
    }

    [Fact]
    public void MultiUpConfig_CanBeModified()
    {
        var config = new MultiUpConfig();
        config.Rows = 4;
        config.Columns = 3;
        config.Direction = "Vertical";

        Assert.Equal(4, config.Rows);
        Assert.Equal(3, config.Columns);
        Assert.Equal("Vertical", config.Direction);
    }
}

/// <summary>
/// PageInfo 扩展属性测试
/// </summary>
public class PageInfoExtendedTests
{
    // ============== Width ==============

    [Fact]
    public void Width_DefaultIs210()
    {
        var page = new PageInfo();
        Assert.Equal(210, page.Width);
    }

    [Fact]
    public void Width_SetA4_Works()
    {
        var page = new PageInfo { Width = 210 };
        Assert.Equal(210, page.Width);
    }

    [Fact]
    public void Width_SetLetter_Works()
    {
        var page = new PageInfo { Width = 216 };
        Assert.Equal(216, page.Width);
    }

    [Fact]
    public void Width_SetA3_Works()
    {
        var page = new PageInfo { Width = 297 };
        Assert.Equal(297, page.Width);
    }

    [Fact]
    public void Width_SetCustom_Works()
    {
        var page = new PageInfo { Width = 150 };
        Assert.Equal(150, page.Width);
    }

    // ============== Height ==============

    [Fact]
    public void Height_DefaultIs297()
    {
        var page = new PageInfo();
        Assert.Equal(297, page.Height);
    }

    [Fact]
    public void Height_SetA4_Works()
    {
        var page = new PageInfo { Height = 297 };
        Assert.Equal(297, page.Height);
    }

    [Fact]
    public void Height_SetLetter_Works()
    {
        var page = new PageInfo { Height = 279 };
        Assert.Equal(279, page.Height);
    }

    [Fact]
    public void Height_SetA3_Works()
    {
        var page = new PageInfo { Height = 420 };
        Assert.Equal(420, page.Height);
    }

    [Fact]
    public void Height_SetCustom_Works()
    {
        var page = new PageInfo { Height = 200 };
        Assert.Equal(200, page.Height);
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
        var page = new PageInfo { Orientation = "portrait" };
        Assert.Equal("portrait", page.Orientation);
    }

    [Fact]
    public void Orientation_CanBeChanged()
    {
        var page = new PageInfo { Orientation = "portrait" };
        page.Orientation = "landscape";
        Assert.Equal("landscape", page.Orientation);
    }

    // ============== Margin ==============

    [Fact]
    public void Margin_NotNull()
    {
        var page = new PageInfo();
        Assert.NotNull(page.Margin);
    }

    [Fact]
    public void Margin_TopDefaultIs10()
    {
        var page = new PageInfo();
        Assert.Equal(10, page.Margin.Top);
    }

    [Fact]
    public void Margin_BottomDefaultIs10()
    {
        var page = new PageInfo();
        Assert.Equal(10, page.Margin.Bottom);
    }

    [Fact]
    public void Margin_LeftDefaultIs10()
    {
        var page = new PageInfo();
        Assert.Equal(10, page.Margin.Left);
    }

    [Fact]
    public void Margin_RightDefaultIs10()
    {
        var page = new PageInfo();
        Assert.Equal(10, page.Margin.Right);
    }

    [Fact]
    public void Margin_SetCustom_Works()
    {
        var page = new PageInfo();
        page.Margin = new Margin { Top = 15, Bottom = 15, Left = 20, Right = 20 };
        Assert.Equal(15, page.Margin.Top);
        Assert.Equal(20, page.Margin.Left);
    }

    [Fact]
    public void Margin_CanBeChanged()
    {
        var page = new PageInfo();
        page.Margin.Top = 25;
        Assert.Equal(25, page.Margin.Top);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void PageInfo_A4Portrait_Works()
    {
        var page = new PageInfo
        {
            Width = 210,
            Height = 297,
            Orientation = "portrait"
        };

        Assert.Equal(210, page.Width);
        Assert.Equal(297, page.Height);
        Assert.Equal("portrait", page.Orientation);
        Assert.Equal(10, page.Margin.Top);
    }

    [Fact]
    public void PageInfo_A4Landscape_Works()
    {
        var page = new PageInfo
        {
            Width = 297,
            Height = 210,
            Orientation = "landscape",
            Margin = new Margin { Top = 15, Bottom = 15, Left = 15, Right = 15 }
        };

        Assert.Equal(297, page.Width);
        Assert.Equal(210, page.Height);
        Assert.Equal("landscape", page.Orientation);
    }

    [Fact]
    public void PageInfo_LetterPortrait_Works()
    {
        var page = new PageInfo
        {
            Width = 216,
            Height = 279,
            Orientation = "portrait"
        };

        Assert.Equal(216, page.Width);
        Assert.Equal(279, page.Height);
    }

    [Fact]
    public void PageInfo_LetterLandscape_Works()
    {
        var page = new PageInfo
        {
            Width = 279,
            Height = 216,
            Orientation = "landscape"
        };

        Assert.Equal(279, page.Width);
        Assert.Equal(216, page.Height);
    }

    [Fact]
    public void PageInfo_A3Portrait_Works()
    {
        var page = new PageInfo
        {
            Width = 297,
            Height = 420,
            Orientation = "portrait"
        };

        Assert.Equal(297, page.Width);
        Assert.Equal(420, page.Height);
    }

    [Fact]
    public void PageInfo_CustomSize_Works()
    {
        var page = new PageInfo
        {
            Width = 100,
            Height = 150,
            Orientation = "portrait",
            Margin = new Margin { Top = 5, Bottom = 5, Left = 5, Right = 5 }
        };

        Assert.Equal(100, page.Width);
        Assert.Equal(150, page.Height);
        Assert.Equal(5, page.Margin.Top);
    }

    [Fact]
    public void PageInfo_CanBeModified()
    {
        var page = new PageInfo();
        page.Width = 300;
        page.Height = 400;
        page.Orientation = "landscape";
        page.Margin.Top = 20;

        Assert.Equal(300, page.Width);
        Assert.Equal(400, page.Height);
        Assert.Equal("landscape", page.Orientation);
        Assert.Equal(20, page.Margin.Top);
    }
}
