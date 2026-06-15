using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// TextElement 高级属性测试
/// </summary>
public class TextElementAdvancedTests
{
    // ============== CanGrow ==============

    [Fact]
    public void CanGrow_FalseByDefault()
    {
        var el = new TextElement();
        Assert.False(el.CanGrow);
    }

    [Fact]
    public void CanGrow_SetTrue_Works()
    {
        var el = new TextElement { CanGrow = true };
        Assert.True(el.CanGrow);
    }

    [Fact]
    public void CanGrow_CanBeToggled()
    {
        var el = new TextElement();
        el.CanGrow = true;
        Assert.True(el.CanGrow);
        el.CanGrow = false;
        Assert.False(el.CanGrow);
    }

    // ============== CanShrink ==============

    [Fact]
    public void CanShrink_FalseByDefault()
    {
        var el = new TextElement();
        Assert.False(el.CanShrink);
    }

    [Fact]
    public void CanShrink_SetTrue_Works()
    {
        var el = new TextElement { CanShrink = true };
        Assert.True(el.CanShrink);
    }

    [Fact]
    public void CanShrink_CanBeToggled()
    {
        var el = new TextElement();
        el.CanShrink = true;
        Assert.True(el.CanShrink);
        el.CanShrink = false;
        Assert.False(el.CanShrink);
    }

    // ============== MaxLines ==============

    [Fact]
    public void MaxLines_ZeroByDefault()
    {
        var el = new TextElement();
        Assert.Equal(0, el.MaxLines);
    }

    [Fact]
    public void MaxLines_Set_Works()
    {
        var el = new TextElement { MaxLines = 3 };
        Assert.Equal(3, el.MaxLines);
    }

    [Fact]
    public void MaxLines_SetLarge_Works()
    {
        var el = new TextElement { MaxLines = 100 };
        Assert.Equal(100, el.MaxLines);
    }

    [Fact]
    public void MaxLines_CanBeChanged()
    {
        var el = new TextElement { MaxLines = 5 };
        Assert.Equal(5, el.MaxLines);
        el.MaxLines = 10;
        Assert.Equal(10, el.MaxLines);
    }

    // ============== Hyperlink ==============

    [Fact]
    public void Hyperlink_NullByDefault()
    {
        var el = new TextElement();
        Assert.Null(el.Hyperlink);
    }

    [Fact]
    public void Hyperlink_SetUrl_Works()
    {
        var el = new TextElement { Hyperlink = "https://example.com" };
        Assert.Equal("https://example.com", el.Hyperlink);
    }

    [Fact]
    public void Hyperlink_SetEmail_Works()
    {
        var el = new TextElement { Hyperlink = "mailto:test@example.com" };
        Assert.StartsWith("mailto:", el.Hyperlink);
    }

    [Fact]
    public void Hyperlink_SetExpression_Works()
    {
        var el = new TextElement { Hyperlink = "{{currentRow.url}}" };
        Assert.Contains("{{", el.Hyperlink);
    }

    [Fact]
    public void Hyperlink_CanBeCleared()
    {
        var el = new TextElement { Hyperlink = "https://example.com" };
        el.Hyperlink = null;
        Assert.Null(el.Hyperlink);
    }

    // ============== Format ==============

    [Fact]
    public void Format_NullByDefault()
    {
        var el = new TextElement();
        Assert.Null(el.Format);
    }

    [Fact]
    public void Format_SetCurrency_Works()
    {
        var el = new TextElement { Format = "currency" };
        Assert.Equal("currency", el.Format);
    }

    [Fact]
    public void Format_SetDate_Works()
    {
        var el = new TextElement { Format = "date" };
        Assert.Equal("date", el.Format);
    }

    [Fact]
    public void Format_SetPercent_Works()
    {
        var el = new TextElement { Format = "percent" };
        Assert.Equal("percent", el.Format);
    }

    [Fact]
    public void Format_SetNumberWithDecimals_Works()
    {
        var el = new TextElement { Format = "number:2" };
        Assert.Equal("number:2", el.Format);
    }

    [Fact]
    public void Format_CanBeChanged()
    {
        var el = new TextElement { Format = "currency" };
        el.Format = "date";
        Assert.Equal("date", el.Format);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void TextElement_WithCanGrowAndCanShrink_Works()
    {
        var el = new TextElement
        {
            Text = "Long text that may grow or shrink",
            CanGrow = true,
            CanShrink = true
        };

        Assert.True(el.CanGrow);
        Assert.True(el.CanShrink);
    }

    [Fact]
    public void TextElement_WithMaxLinesAndCanGrow_Works()
    {
        var el = new TextElement
        {
            Text = "Multi-line text",
            MaxLines = 5,
            CanGrow = true
        };

        Assert.Equal(5, el.MaxLines);
        Assert.True(el.CanGrow);
    }

    [Fact]
    public void TextElement_WithHyperlinkAndFormat_Works()
    {
        var el = new TextElement
        {
            Text = "{{currentRow.amount}}",
            DataField = "amount",
            Format = "currency",
            Hyperlink = "https://example.com/details/{{currentRow.id}}"
        };

        Assert.Equal("currency", el.Format);
        Assert.NotNull(el.Hyperlink);
        Assert.Equal("amount", el.DataField);
    }

    [Fact]
    public void TextElement_FullSetup_Works()
    {
        var el = new TextElement
        {
            Text = "Report Total",
            DataField = "total",
            Format = "currency",
            Alignment = TextAlignment.Right,
            CanGrow = true,
            CanShrink = false,
            MaxLines = 1,
            Hyperlink = "https://example.com/total"
        };
        el.Font = new FontDef { Family = "Arial", Size = 12, Bold = true };

        Assert.Equal("Report Total", el.Text);
        Assert.Equal("total", el.DataField);
        Assert.Equal("currency", el.Format);
        Assert.Equal(TextAlignment.Right, el.Alignment);
        Assert.True(el.CanGrow);
        Assert.False(el.CanShrink);
        Assert.Equal(1, el.MaxLines);
        Assert.Equal("Arial", el.Font.Family);
        Assert.True(el.Font.Bold);
    }

    [Fact]
    public void TextElement_SummaryWithFormat_Works()
    {
        var el = new TextElement
        {
            SummaryFunction = "Sum",
            SummaryField = "amount",
            Format = "currency",
            Alignment = TextAlignment.Right
        };

        Assert.Equal(TextBoxType.Summary, el.BoxType);
        Assert.Equal("currency", el.Format);
    }

    [Fact]
    public void TextElement_SysVarWithFormat_Works()
    {
        var el = new TextElement
        {
            SystemVariable = "PrintDate",
            Format = "date"
        };

        Assert.Equal(TextBoxType.SysVar, el.BoxType);
        Assert.Equal("date", el.Format);
    }
}
