using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ConditionalFormatRule 完整字段测试：
///   - ConditionalFormatRule 完整字段（Expression/BackgroundColor/FontColor/Bold）
///   - 字段组合行为
/// </summary>
public class ConditionalFormatRuleCompleteTests
{
    [Fact]
    public void ConditionalFormatRule_Defaults()
    {
        var r = new ConditionalFormatRule();
        Assert.Equal("", r.Expression);
        Assert.Null(r.BackgroundColor);
        Assert.Null(r.FontColor);
        Assert.False(r.Bold);
    }

    [Fact]
    public void ConditionalFormatRule_AllSetters()
    {
        var r = new ConditionalFormatRule
        {
            Expression = "[Amount] > 1000",
            BackgroundColor = "#FF0000",
            FontColor = "#FFFFFF",
            Bold = true,
        };
        Assert.Equal("[Amount] > 1000", r.Expression);
        Assert.Equal("#FF0000", r.BackgroundColor);
        Assert.Equal("#FFFFFF", r.FontColor);
        Assert.True(r.Bold);
    }

    [Fact]
    public void ConditionalFormatRule_Expression_CanBeEmpty()
    {
        var r = new ConditionalFormatRule { Expression = "" };
        Assert.Equal("", r.Expression);
    }

    [Fact]
    public void ConditionalFormatRule_Expression_CanBeComparison()
    {
        var r = new ConditionalFormatRule { Expression = "[Price] >= 100" };
        Assert.Equal("[Price] >= 100", r.Expression);
    }

    [Fact]
    public void ConditionalFormatRule_Expression_CanBeEquality()
    {
        var r = new ConditionalFormatRule { Expression = "[Status] == 'Active'" };
        Assert.Equal("[Status] == 'Active'", r.Expression);
    }

    [Fact]
    public void ConditionalFormatRule_Expression_CanBeComplex()
    {
        var r = new ConditionalFormatRule { Expression = "[Qty] > 10 AND [Price] < 50" };
        Assert.Contains("AND", r.Expression);
    }

    [Fact]
    public void ConditionalFormatRule_BackgroundColor_CanBeNull()
    {
        var r = new ConditionalFormatRule { BackgroundColor = null };
        Assert.Null(r.BackgroundColor);
    }

    [Fact]
    public void ConditionalFormatRule_BackgroundColor_CanBeEmpty()
    {
        var r = new ConditionalFormatRule { BackgroundColor = "" };
        Assert.Equal("", r.BackgroundColor);
    }

    [Fact]
    public void ConditionalFormatRule_BackgroundColor_CanBeHex()
    {
        var r = new ConditionalFormatRule { BackgroundColor = "#FFFF00" };
        Assert.Equal("#FFFF00", r.BackgroundColor);
    }

    [Fact]
    public void ConditionalFormatRule_BackgroundColor_CanBeHex8()
    {
        var r = new ConditionalFormatRule { BackgroundColor = "#80FF0000" };
        Assert.Equal("#80FF0000", r.BackgroundColor);
    }

    [Fact]
    public void ConditionalFormatRule_FontColor_CanBeNull()
    {
        var r = new ConditionalFormatRule { FontColor = null };
        Assert.Null(r.FontColor);
    }

    [Fact]
    public void ConditionalFormatRule_FontColor_CanBeEmpty()
    {
        var r = new ConditionalFormatRule { FontColor = "" };
        Assert.Equal("", r.FontColor);
    }

    [Fact]
    public void ConditionalFormatRule_FontColor_CanBeHex()
    {
        var r = new ConditionalFormatRule { FontColor = "#00FF00" };
        Assert.Equal("#00FF00", r.FontColor);
    }

    [Fact]
    public void ConditionalFormatRule_FontColor_CanBeHex8()
    {
        var r = new ConditionalFormatRule { FontColor = "#FF000000" };
        Assert.Equal("#FF000000", r.FontColor);
    }

    [Fact]
    public void ConditionalFormatRule_Bold_DefaultFalse()
    {
        var r = new ConditionalFormatRule();
        Assert.False(r.Bold);
    }

    [Fact]
    public void ConditionalFormatRule_Bold_CanBeTrue()
    {
        var r = new ConditionalFormatRule { Bold = true };
        Assert.True(r.Bold);
    }

    [Fact]
    public void ConditionalFormatRule_FullCombination()
    {
        var r = new ConditionalFormatRule
        {
            Expression = "[Score] < 60",
            BackgroundColor = "#FFCCCC",
            FontColor = "#CC0000",
            Bold = true,
        };
        Assert.Equal("[Score] < 60", r.Expression);
        Assert.Equal("#FFCCCC", r.BackgroundColor);
        Assert.Equal("#CC0000", r.FontColor);
        Assert.True(r.Bold);
    }

    [Fact]
    public void ConditionalFormatRule_OnlyExpression()
    {
        var r = new ConditionalFormatRule { Expression = "[Value] > 0" };
        Assert.Equal("[Value] > 0", r.Expression);
        Assert.Null(r.BackgroundColor);
        Assert.Null(r.FontColor);
        Assert.False(r.Bold);
    }

    [Fact]
    public void ConditionalFormatRule_OnlyBold()
    {
        var r = new ConditionalFormatRule { Bold = true };
        Assert.Equal("", r.Expression);
        Assert.Null(r.BackgroundColor);
        Assert.Null(r.FontColor);
        Assert.True(r.Bold);
    }
}
