using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// FontDef 行为测试：
///   - 默认值
///   - 字体系列
///   - 字号
///   - 粗体/斜体/下划线
///   - 颜色
/// </summary>
public class FontDefBehaviorTests
{
    // ============== 默认值 ==============

    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var font = new FontDef();

        Assert.Equal("SimSun", font.Family);
        Assert.Equal(10, font.Size);
        Assert.False(font.Bold);
        Assert.False(font.Italic);
        Assert.False(font.Underline);
        Assert.Null(font.Color);
    }

    // ============== Family ==============

    [Fact]
    public void Family_DefaultIsSimSun()
    {
        var font = new FontDef();
        Assert.Equal("SimSun", font.Family);
    }

    [Fact]
    public void Family_SetArial_Works()
    {
        var font = new FontDef { Family = "Arial" };
        Assert.Equal("Arial", font.Family);
    }

    [Fact]
    public void Family_SetTimesNewRoman_Works()
    {
        var font = new FontDef { Family = "Times New Roman" };
        Assert.Equal("Times New Roman", font.Family);
    }

    [Fact]
    public void Family_SetChineseFont_Works()
    {
        var font = new FontDef { Family = "微软雅黑" };
        Assert.Equal("微软雅黑", font.Family);
    }

    [Fact]
    public void Family_SetCourierNew_Works()
    {
        var font = new FontDef { Family = "Courier New" };
        Assert.Equal("Courier New", font.Family);
    }

    [Fact]
    public void Family_EmptyString_Works()
    {
        var font = new FontDef { Family = "" };
        Assert.Equal("", font.Family);
    }

    // ============== Size ==============

    [Fact]
    public void Size_DefaultIs10()
    {
        var font = new FontDef();
        Assert.Equal(10, font.Size);
    }

    [Fact]
    public void Size_SetSmall_Works()
    {
        var font = new FontDef { Size = 8 };
        Assert.Equal(8, font.Size);
    }

    [Fact]
    public void Size_SetLarge_Works()
    {
        var font = new FontDef { Size = 24 };
        Assert.Equal(24, font.Size);
    }

    [Fact]
    public void Size_SetDecimal_Works()
    {
        var font = new FontDef { Size = 10.5 };
        Assert.Equal(10.5, font.Size);
    }

    [Fact]
    public void Size_SetVerySmall_Works()
    {
        var font = new FontDef { Size = 6 };
        Assert.Equal(6, font.Size);
    }

    [Fact]
    public void Size_SetVeryLarge_Works()
    {
        var font = new FontDef { Size = 72 };
        Assert.Equal(72, font.Size);
    }

    // ============== Bold ==============

    [Fact]
    public void Bold_DefaultIsFalse()
    {
        var font = new FontDef();
        Assert.False(font.Bold);
    }

    [Fact]
    public void Bold_SetTrue_Works()
    {
        var font = new FontDef { Bold = true };
        Assert.True(font.Bold);
    }

    [Fact]
    public void Bold_CanBeToggled()
    {
        var font = new FontDef { Bold = true };
        font.Bold = false;
        Assert.False(font.Bold);
    }

    // ============== Italic ==============

    [Fact]
    public void Italic_DefaultIsFalse()
    {
        var font = new FontDef();
        Assert.False(font.Italic);
    }

    [Fact]
    public void Italic_SetTrue_Works()
    {
        var font = new FontDef { Italic = true };
        Assert.True(font.Italic);
    }

    [Fact]
    public void Italic_CanBeToggled()
    {
        var font = new FontDef { Italic = true };
        font.Italic = false;
        Assert.False(font.Italic);
    }

    // ============== Underline ==============

    [Fact]
    public void Underline_DefaultIsFalse()
    {
        var font = new FontDef();
        Assert.False(font.Underline);
    }

    [Fact]
    public void Underline_SetTrue_Works()
    {
        var font = new FontDef { Underline = true };
        Assert.True(font.Underline);
    }

    [Fact]
    public void Underline_CanBeToggled()
    {
        var font = new FontDef { Underline = true };
        font.Underline = false;
        Assert.False(font.Underline);
    }

    // ============== Color ==============

    [Fact]
    public void Color_NullByDefault()
    {
        var font = new FontDef();
        Assert.Null(font.Color);
    }

    [Fact]
    public void Color_SetHex_Works()
    {
        var font = new FontDef { Color = "#FF0000" };
        Assert.Equal("#FF0000", font.Color);
    }

    [Fact]
    public void Color_SetHexWithAlpha_Works()
    {
        var font = new FontDef { Color = "#80FF0000" };
        Assert.Equal("#80FF0000", font.Color);
    }

    [Fact]
    public void Color_SetBlack_Works()
    {
        var font = new FontDef { Color = "#000000" };
        Assert.Equal("#000000", font.Color);
    }

    [Fact]
    public void Color_SetBlue_Works()
    {
        var font = new FontDef { Color = "#0000FF" };
        Assert.Equal("#0000FF", font.Color);
    }

    [Fact]
    public void Color_CanBeCleared()
    {
        var font = new FontDef { Color = "#FF0000" };
        font.Color = null;
        Assert.Null(font.Color);
    }

    // ============== 组合样式 ==============

    [Fact]
    public void BoldAndItalic_BothTrue()
    {
        var font = new FontDef { Bold = true, Italic = true };
        Assert.True(font.Bold);
        Assert.True(font.Italic);
    }

    [Fact]
    public void BoldAndUnderline_BothTrue()
    {
        var font = new FontDef { Bold = true, Underline = true };
        Assert.True(font.Bold);
        Assert.True(font.Underline);
    }

    [Fact]
    public void ItalicAndUnderline_BothTrue()
    {
        var font = new FontDef { Italic = true, Underline = true };
        Assert.True(font.Italic);
        Assert.True(font.Underline);
    }

    [Fact]
    public void AllStyles_True()
    {
        var font = new FontDef { Bold = true, Italic = true, Underline = true };
        Assert.True(font.Bold);
        Assert.True(font.Italic);
        Assert.True(font.Underline);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void FontDef_FullSetup_Works()
    {
        var font = new FontDef
        {
            Family = "Arial",
            Size = 14,
            Bold = true,
            Italic = false,
            Underline = true,
            Color = "#333333"
        };

        Assert.Equal("Arial", font.Family);
        Assert.Equal(14, font.Size);
        Assert.True(font.Bold);
        Assert.False(font.Italic);
        Assert.True(font.Underline);
        Assert.Equal("#333333", font.Color);
    }

    [Fact]
    public void FontDef_TitleStyle_Works()
    {
        var font = new FontDef
        {
            Family = "微软雅黑",
            Size = 18,
            Bold = true,
            Color = "#000000"
        };

        Assert.Equal(18, font.Size);
        Assert.True(font.Bold);
    }

    [Fact]
    public void FontDef_BodyStyle_Works()
    {
        var font = new FontDef
        {
            Family = "SimSun",
            Size = 10,
            Color = "#333333"
        };

        Assert.Equal("SimSun", font.Family);
        Assert.Equal(10, font.Size);
        Assert.False(font.Bold);
    }

    [Fact]
    public void FontDef_LinkStyle_Works()
    {
        var font = new FontDef
        {
            Family = "Arial",
            Size = 10,
            Underline = true,
            Color = "#0000FF"
        };

        Assert.True(font.Underline);
        Assert.Equal("#0000FF", font.Color);
    }

    [Fact]
    public void FontDef_CanBeModified()
    {
        var font = new FontDef { Family = "Arial", Size = 10 };
        
        font.Size = 12;
        font.Bold = true;
        font.Color = "#FF0000";
        
        Assert.Equal(12, font.Size);
        Assert.True(font.Bold);
        Assert.Equal("#FF0000", font.Color);
    }
}
