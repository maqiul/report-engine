using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// FontDef 完整字段测试：
///   - FontDef 完整字段（Family/Size/Bold/Italic/Underline/Color）
///   - 字段组合行为
/// </summary>
public class FontDefCompleteTests
{
    [Fact]
    public void FontDef_Defaults()
    {
        var f = new FontDef();
        Assert.Equal("SimSun", f.Family);
        Assert.Equal(10, f.Size);
        Assert.False(f.Bold);
        Assert.False(f.Italic);
        Assert.False(f.Underline);
        Assert.Null(f.Color);
    }

    [Fact]
    public void FontDef_AllSetters()
    {
        var f = new FontDef
        {
            Family = "Arial",
            Size = 14,
            Bold = true,
            Italic = true,
            Underline = true,
            Color = "#FF0000",
        };
        Assert.Equal("Arial", f.Family);
        Assert.Equal(14, f.Size);
        Assert.True(f.Bold);
        Assert.True(f.Italic);
        Assert.True(f.Underline);
        Assert.Equal("#FF0000", f.Color);
    }

    [Fact]
    public void FontDef_Family_DefaultSimSun()
    {
        var f = new FontDef();
        Assert.Equal("SimSun", f.Family);
    }

    [Fact]
    public void FontDef_Family_CanBeArial()
    {
        var f = new FontDef { Family = "Arial" };
        Assert.Equal("Arial", f.Family);
    }

    [Fact]
    public void FontDef_Family_CanBeTimesNewRoman()
    {
        var f = new FontDef { Family = "Times New Roman" };
        Assert.Equal("Times New Roman", f.Family);
    }

    [Fact]
    public void FontDef_Family_CanBeChinese()
    {
        var f = new FontDef { Family = "微软雅黑" };
        Assert.Equal("微软雅黑", f.Family);
    }

    [Fact]
    public void FontDef_Family_CanBeEmpty()
    {
        var f = new FontDef { Family = "" };
        Assert.Equal("", f.Family);
    }

    [Fact]
    public void FontDef_Size_Default10()
    {
        var f = new FontDef();
        Assert.Equal(10, f.Size);
    }

    [Fact]
    public void FontDef_Size_CanBeSmall()
    {
        var f = new FontDef { Size = 6 };
        Assert.Equal(6, f.Size);
    }

    [Fact]
    public void FontDef_Size_CanBeLarge()
    {
        var f = new FontDef { Size = 72 };
        Assert.Equal(72, f.Size);
    }

    [Fact]
    public void FontDef_Size_CanBeDecimal()
    {
        var f = new FontDef { Size = 10.5 };
        Assert.Equal(10.5, f.Size);
    }

    [Fact]
    public void FontDef_Size_CanBeZero()
    {
        var f = new FontDef { Size = 0 };
        Assert.Equal(0, f.Size);
    }

    [Fact]
    public void FontDef_Bold_DefaultFalse()
    {
        var f = new FontDef();
        Assert.False(f.Bold);
    }

    [Fact]
    public void FontDef_Bold_CanBeTrue()
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
    public void FontDef_Italic_CanBeTrue()
    {
        var f = new FontDef { Italic = true };
        Assert.True(f.Italic);
    }

    [Fact]
    public void FontDef_Underline_DefaultFalse()
    {
        var f = new FontDef();
        Assert.False(f.Underline);
    }

    [Fact]
    public void FontDef_Underline_CanBeTrue()
    {
        var f = new FontDef { Underline = true };
        Assert.True(f.Underline);
    }

    [Fact]
    public void FontDef_Color_CanBeNull()
    {
        var f = new FontDef { Color = null };
        Assert.Null(f.Color);
    }

    [Fact]
    public void FontDef_Color_CanBeEmpty()
    {
        var f = new FontDef { Color = "" };
        Assert.Equal("", f.Color);
    }

    [Fact]
    public void FontDef_Color_CanBeHex()
    {
        var f = new FontDef { Color = "#0000FF" };
        Assert.Equal("#0000FF", f.Color);
    }

    [Fact]
    public void FontDef_Color_CanBeHex8()
    {
        var f = new FontDef { Color = "#80000000" };
        Assert.Equal("#80000000", f.Color);
    }

    [Fact]
    public void FontDef_AllStylesTrue()
    {
        var f = new FontDef { Bold = true, Italic = true, Underline = true };
        Assert.True(f.Bold && f.Italic && f.Underline);
    }

    [Fact]
    public void FontDef_FullCombination()
    {
        var f = new FontDef
        {
            Family = "Calibri",
            Size = 12,
            Bold = true,
            Italic = false,
            Underline = true,
            Color = "#333333",
        };
        Assert.Equal("Calibri", f.Family);
        Assert.Equal(12, f.Size);
        Assert.True(f.Bold);
        Assert.False(f.Italic);
        Assert.True(f.Underline);
        Assert.Equal("#333333", f.Color);
    }
}
