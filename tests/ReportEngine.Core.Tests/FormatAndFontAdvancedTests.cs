using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ConditionalFormatRule 高级属性测试
/// </summary>
public class ConditionalFormatRuleAdvancedTests
{
    // ============== Expression ==============

    [Fact]
    public void Expression_EmptyByDefault()
    {
        var rule = new ConditionalFormatRule();
        Assert.Equal("", rule.Expression);
    }

    [Fact]
    public void Expression_SetComparison_Works()
    {
        var rule = new ConditionalFormatRule { Expression = "[Amount] > 1000" };
        Assert.Equal("[Amount] > 1000", rule.Expression);
    }

    [Fact]
    public void Expression_SetEquality_Works()
    {
        var rule = new ConditionalFormatRule { Expression = "[Status] == 'Active'" };
        Assert.Contains("==", rule.Expression);
    }

    [Fact]
    public void Expression_SetComplex_Works()
    {
        var rule = new ConditionalFormatRule { Expression = "[Qty] > 10 AND [Price] < 100" };
        Assert.Contains("AND", rule.Expression);
    }

    [Fact]
    public void Expression_CanBeChanged()
    {
        var rule = new ConditionalFormatRule { Expression = "[A] > 1" };
        rule.Expression = "[B] < 2";
        Assert.Equal("[B] < 2", rule.Expression);
    }

    // ============== BackgroundColor ==============

    [Fact]
    public void BackgroundColor_NullByDefault()
    {
        var rule = new ConditionalFormatRule();
        Assert.Null(rule.BackgroundColor);
    }

    [Fact]
    public void BackgroundColor_SetRed_Works()
    {
        var rule = new ConditionalFormatRule { BackgroundColor = "#FF0000" };
        Assert.Equal("#FF0000", rule.BackgroundColor);
    }

    [Fact]
    public void BackgroundColor_SetGreen_Works()
    {
        var rule = new ConditionalFormatRule { BackgroundColor = "#00FF00" };
        Assert.Equal("#00FF00", rule.BackgroundColor);
    }

    [Fact]
    public void BackgroundColor_SetWithAlpha_Works()
    {
        var rule = new ConditionalFormatRule { BackgroundColor = "#80FF0000" };
        Assert.Equal("#80FF0000", rule.BackgroundColor);
    }

    [Fact]
    public void BackgroundColor_CanBeCleared()
    {
        var rule = new ConditionalFormatRule { BackgroundColor = "#FFFF00" };
        rule.BackgroundColor = null;
        Assert.Null(rule.BackgroundColor);
    }

    // ============== FontColor ==============

    [Fact]
    public void FontColor_NullByDefault()
    {
        var rule = new ConditionalFormatRule();
        Assert.Null(rule.FontColor);
    }

    [Fact]
    public void FontColor_SetRed_Works()
    {
        var rule = new ConditionalFormatRule { FontColor = "#FF0000" };
        Assert.Equal("#FF0000", rule.FontColor);
    }

    [Fact]
    public void FontColor_SetBlue_Works()
    {
        var rule = new ConditionalFormatRule { FontColor = "#0000FF" };
        Assert.Equal("#0000FF", rule.FontColor);
    }

    [Fact]
    public void FontColor_CanBeCleared()
    {
        var rule = new ConditionalFormatRule { FontColor = "#00FF00" };
        rule.FontColor = null;
        Assert.Null(rule.FontColor);
    }

    // ============== Bold ==============

    [Fact]
    public void Bold_FalseByDefault()
    {
        var rule = new ConditionalFormatRule();
        Assert.False(rule.Bold);
    }

    [Fact]
    public void Bold_SetTrue_Works()
    {
        var rule = new ConditionalFormatRule { Bold = true };
        Assert.True(rule.Bold);
    }

    [Fact]
    public void Bold_CanBeToggled()
    {
        var rule = new ConditionalFormatRule { Bold = true };
        rule.Bold = false;
        Assert.False(rule.Bold);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void ConditionalFormatRule_HighlightWarning_Works()
    {
        var rule = new ConditionalFormatRule
        {
            Expression = "[Stock] < 10",
            BackgroundColor = "#FFFF00",
            FontColor = "#FF0000",
            Bold = true
        };

        Assert.Equal("[Stock] < 10", rule.Expression);
        Assert.Equal("#FFFF00", rule.BackgroundColor);
        Assert.Equal("#FF0000", rule.FontColor);
        Assert.True(rule.Bold);
    }

    [Fact]
    public void ConditionalFormatRule_HighlightSuccess_Works()
    {
        var rule = new ConditionalFormatRule
        {
            Expression = "[Status] == 'Completed'",
            BackgroundColor = "#00FF00",
            Bold = false
        };

        Assert.Equal("#00FF00", rule.BackgroundColor);
        Assert.False(rule.Bold);
    }

    [Fact]
    public void ConditionalFormatRule_OnlyFontChange_Works()
    {
        var rule = new ConditionalFormatRule
        {
            Expression = "[Amount] > 10000",
            FontColor = "#FF0000",
            Bold = true
        };

        Assert.Null(rule.BackgroundColor);
        Assert.Equal("#FF0000", rule.FontColor);
        Assert.True(rule.Bold);
    }

    [Fact]
    public void ConditionalFormatRule_OnlyBackground_Works()
    {
        var rule = new ConditionalFormatRule
        {
            Expression = "[Overdue] == true",
            BackgroundColor = "#FFCCCC"
        };

        Assert.Equal("#FFCCCC", rule.BackgroundColor);
        Assert.Null(rule.FontColor);
        Assert.False(rule.Bold);
    }

    [Fact]
    public void ConditionalFormatRule_FullSetup_Works()
    {
        var rule = new ConditionalFormatRule
        {
            Expression = "[Score] >= 90",
            BackgroundColor = "#E8F5E9",
            FontColor = "#2E7D32",
            Bold = true
        };

        Assert.Equal("[Score] >= 90", rule.Expression);
        Assert.Equal("#E8F5E9", rule.BackgroundColor);
        Assert.Equal("#2E7D32", rule.FontColor);
        Assert.True(rule.Bold);
    }
}

/// <summary>
/// FontDef 高级属性测试
/// </summary>
public class FontDefAdvancedTests
{
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
    public void Family_CanBeChanged()
    {
        var font = new FontDef { Family = "Arial" };
        font.Family = "Helvetica";
        Assert.Equal("Helvetica", font.Family);
    }

    // ============== Size ==============

    [Fact]
    public void Size_DefaultIs10()
    {
        var font = new FontDef();
        Assert.Equal(10, font.Size);
    }

    [Fact]
    public void Size_Set_Works()
    {
        var font = new FontDef { Size = 14 };
        Assert.Equal(14, font.Size);
    }

    [Fact]
    public void Size_SetDecimal_Works()
    {
        var font = new FontDef { Size = 10.5 };
        Assert.Equal(10.5, font.Size);
    }

    [Fact]
    public void Size_SetSmall_Works()
    {
        var font = new FontDef { Size = 6 };
        Assert.Equal(6, font.Size);
    }

    [Fact]
    public void Size_SetLarge_Works()
    {
        var font = new FontDef { Size = 72 };
        Assert.Equal(72, font.Size);
    }

    // ============== Bold ==============

    [Fact]
    public void Bold_FalseByDefault()
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
    public void Italic_FalseByDefault()
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
    public void Underline_FalseByDefault()
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
    public void Color_SetRed_Works()
    {
        var font = new FontDef { Color = "#FF0000" };
        Assert.Equal("#FF0000", font.Color);
    }

    [Fact]
    public void Color_SetBlue_Works()
    {
        var font = new FontDef { Color = "#0000FF" };
        Assert.Equal("#0000FF", font.Color);
    }

    [Fact]
    public void Color_SetWithAlpha_Works()
    {
        var font = new FontDef { Color = "#80FF0000" };
        Assert.Equal("#80FF0000", font.Color);
    }

    [Fact]
    public void Color_CanBeChanged()
    {
        var font = new FontDef { Color = "#FF0000" };
        font.Color = "#00FF00";
        Assert.Equal("#00FF00", font.Color);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void FontDef_TitleStyle_Works()
    {
        var font = new FontDef
        {
            Family = "Arial",
            Size = 18,
            Bold = true,
            Color = "#333333"
        };

        Assert.Equal("Arial", font.Family);
        Assert.Equal(18, font.Size);
        Assert.True(font.Bold);
        Assert.Equal("#333333", font.Color);
    }

    [Fact]
    public void FontDef_BodyStyle_Works()
    {
        var font = new FontDef
        {
            Family = "Times New Roman",
            Size = 12,
            Color = "#000000"
        };

        Assert.False(font.Bold);
        Assert.False(font.Italic);
        Assert.False(font.Underline);
    }

    [Fact]
    public void FontDef_HyperlinkStyle_Works()
    {
        var font = new FontDef
        {
            Family = "Arial",
            Size = 10,
            Color = "#0000FF",
            Underline = true
        };

        Assert.Equal("#0000FF", font.Color);
        Assert.True(font.Underline);
    }

    [Fact]
    public void FontDef_EmphasisStyle_Works()
    {
        var font = new FontDef
        {
            Bold = true,
            Italic = true,
            Color = "#FF0000"
        };

        Assert.True(font.Bold);
        Assert.True(font.Italic);
        Assert.Equal("#FF0000", font.Color);
    }

    [Fact]
    public void FontDef_FullSetup_Works()
    {
        var font = new FontDef
        {
            Family = "Helvetica",
            Size = 14,
            Bold = true,
            Italic = true,
            Underline = true,
            Color = "#336699"
        };

        Assert.Equal("Helvetica", font.Family);
        Assert.Equal(14, font.Size);
        Assert.True(font.Bold);
        Assert.True(font.Italic);
        Assert.True(font.Underline);
        Assert.Equal("#336699", font.Color);
    }
}
