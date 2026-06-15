using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ConditionalFormatRule 行为测试：
///   - 默认值
///   - 表达式
///   - 背景色
///   - 字体色
///   - 粗体
/// </summary>
public class ConditionalFormatRuleBehaviorTests
{
    // ============== 默认值 ==============

    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var rule = new ConditionalFormatRule();

        Assert.Equal("", rule.Expression);
        Assert.Null(rule.BackgroundColor);
        Assert.Null(rule.FontColor);
        Assert.False(rule.Bold);
    }

    // ============== Expression ==============

    [Fact]
    public void Expression_EmptyByDefault()
    {
        var rule = new ConditionalFormatRule();
        Assert.Equal("", rule.Expression);
    }

    [Fact]
    public void Expression_SetSimple_Works()
    {
        var rule = new ConditionalFormatRule { Expression = "[Amount] > 1000" };
        Assert.Equal("[Amount] > 1000", rule.Expression);
    }

    [Fact]
    public void Expression_SetComplex_Works()
    {
        var rule = new ConditionalFormatRule { Expression = "[Status] = 'Active' AND [Amount] > 500" };
        Assert.Contains("AND", rule.Expression);
    }

    [Fact]
    public void Expression_SetWithFunction_Works()
    {
        var rule = new ConditionalFormatRule { Expression = "SUM([Quantity]) > 100" };
        Assert.Contains("SUM", rule.Expression);
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
    public void BackgroundColor_SetYellow_Works()
    {
        var rule = new ConditionalFormatRule { BackgroundColor = "#FFFF00" };
        Assert.Equal("#FFFF00", rule.BackgroundColor);
    }

    [Fact]
    public void BackgroundColor_CanBeCleared()
    {
        var rule = new ConditionalFormatRule { BackgroundColor = "#FF0000" };
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
    public void FontColor_SetWhite_Works()
    {
        var rule = new ConditionalFormatRule { FontColor = "#FFFFFF" };
        Assert.Equal("#FFFFFF", rule.FontColor);
    }

    [Fact]
    public void FontColor_CanBeCleared()
    {
        var rule = new ConditionalFormatRule { FontColor = "#FF0000" };
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
    public void ConditionalFormatRule_HighValue_Works()
    {
        var rule = new ConditionalFormatRule
        {
            Expression = "[Amount] > 1000",
            BackgroundColor = "#FFCCCC",
            FontColor = "#CC0000",
            Bold = true
        };

        Assert.Equal("[Amount] > 1000", rule.Expression);
        Assert.Equal("#FFCCCC", rule.BackgroundColor);
        Assert.Equal("#CC0000", rule.FontColor);
        Assert.True(rule.Bold);
    }

    [Fact]
    public void ConditionalFormatRule_LowValue_Works()
    {
        var rule = new ConditionalFormatRule
        {
            Expression = "[Amount] < 100",
            BackgroundColor = "#FFFFCC",
            FontColor = "#CC6600"
        };

        Assert.Equal("[Amount] < 100", rule.Expression);
        Assert.Equal("#FFFFCC", rule.BackgroundColor);
        Assert.False(rule.Bold);
    }

    [Fact]
    public void ConditionalFormatRule_StatusCheck_Works()
    {
        var rule = new ConditionalFormatRule
        {
            Expression = "[Status] = 'Overdue'",
            BackgroundColor = "#FF0000",
            FontColor = "#FFFFFF",
            Bold = true
        };

        Assert.Contains("Status", rule.Expression);
        Assert.True(rule.Bold);
    }

    [Fact]
    public void ConditionalFormatRule_InElement_Works()
    {
        var el = new TestReportElement();
        el.ConditionalFormats.Add(new ConditionalFormatRule
        {
            Expression = "[Amount] > 1000",
            BackgroundColor = "#FFCCCC"
        });
        el.ConditionalFormats.Add(new ConditionalFormatRule
        {
            Expression = "[Amount] < 100",
            BackgroundColor = "#FFFFCC"
        });

        Assert.Equal(2, el.ConditionalFormats.Count);
        Assert.Equal("[Amount] > 1000", el.ConditionalFormats[0].Expression);
        Assert.Equal("[Amount] < 100", el.ConditionalFormats[1].Expression);
    }

    [Fact]
    public void ConditionalFormatRule_CanBeModified()
    {
        var rule = new ConditionalFormatRule { Expression = "[A] > 1" };
        
        rule.Expression = "[B] < 2";
        rule.BackgroundColor = "#00FF00";
        rule.FontColor = "#0000FF";
        rule.Bold = true;
        
        Assert.Equal("[B] < 2", rule.Expression);
        Assert.Equal("#00FF00", rule.BackgroundColor);
        Assert.Equal("#0000FF", rule.FontColor);
        Assert.True(rule.Bold);
    }

    // ============== 辅助测试类 ==============

    private class TestReportElement : ReportElement
    {
    }
}
