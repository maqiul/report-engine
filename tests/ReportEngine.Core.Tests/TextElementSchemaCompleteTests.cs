using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// TextElement Schema 完整字段测试：
///   - TextElement 完整字段（Text/DataField/SummaryFunction/SummaryField/SystemVariable/Format/Alignment/Font/CanGrow/CanShrink/MaxLines/Hyperlink）
///   - BoxType 只读 getter 行为
/// </summary>
public class TextElementSchemaCompleteTests
{
    [Fact]
    public void TextElement_Defaults()
    {
        var t = new TextElement();
        Assert.Equal("", t.Text);
        Assert.Null(t.DataField);
        Assert.Null(t.SummaryFunction);
        Assert.Null(t.SummaryField);
        Assert.Null(t.SystemVariable);
        Assert.Null(t.Format);
        Assert.Equal(TextAlignment.Left, t.Alignment);
        Assert.NotNull(t.Font);
        Assert.False(t.CanGrow);
        Assert.False(t.CanShrink);
        Assert.Equal(0, t.MaxLines);
        Assert.Null(t.Hyperlink);
    }

    [Fact]
    public void TextElement_AllSetters()
    {
        var t = new TextElement
        {
            Text = "Hello",
            DataField = "name",
            Format = "N2",
            Alignment = TextAlignment.Center,
            Font = new FontDef { Bold = true },
            CanGrow = true,
            CanShrink = true,
            MaxLines = 3,
            Hyperlink = "https://example.com",
        };
        Assert.Equal("Hello", t.Text);
        Assert.Equal("name", t.DataField);
        Assert.Equal("N2", t.Format);
        Assert.Equal(TextAlignment.Center, t.Alignment);
        Assert.True(t.Font.Bold);
        Assert.True(t.CanGrow);
        Assert.True(t.CanShrink);
        Assert.Equal(3, t.MaxLines);
        Assert.Equal("https://example.com", t.Hyperlink);
    }

    [Fact]
    public void TextElement_Text_CanBeEmpty()
    {
        var t = new TextElement { Text = "" };
        Assert.Equal("", t.Text);
    }

    [Fact]
    public void TextElement_Text_CanBeChinese()
    {
        var t = new TextElement { Text = "标题" };
        Assert.Equal("标题", t.Text);
    }

    [Fact]
    public void TextElement_DataField_CanBeNull()
    {
        var t = new TextElement { DataField = null };
        Assert.Null(t.DataField);
    }

    [Fact]
    public void TextElement_DataField_CanBeSet()
    {
        var t = new TextElement { DataField = "customerName" };
        Assert.Equal("customerName", t.DataField);
    }

    [Fact]
    public void TextElement_SummaryFunction_CanBeNull()
    {
        var t = new TextElement { SummaryFunction = null };
        Assert.Null(t.SummaryFunction);
    }

    [Fact]
    public void TextElement_SummaryFunction_CanBeSum()
    {
        var t = new TextElement { SummaryFunction = "Sum" };
        Assert.Equal("Sum", t.SummaryFunction);
    }

    [Fact]
    public void TextElement_SummaryFunction_CanBeCount()
    {
        var t = new TextElement { SummaryFunction = "Count" };
        Assert.Equal("Count", t.SummaryFunction);
    }

    [Fact]
    public void TextElement_SummaryFunction_CanBeAvg()
    {
        var t = new TextElement { SummaryFunction = "Avg" };
        Assert.Equal("Avg", t.SummaryFunction);
    }

    [Fact]
    public void TextElement_SummaryFunction_CanBeMax()
    {
        var t = new TextElement { SummaryFunction = "Max" };
        Assert.Equal("Max", t.SummaryFunction);
    }

    [Fact]
    public void TextElement_SummaryFunction_CanBeMin()
    {
        var t = new TextElement { SummaryFunction = "Min" };
        Assert.Equal("Min", t.SummaryFunction);
    }

    [Fact]
    public void TextElement_SummaryField_CanBeNull()
    {
        var t = new TextElement { SummaryField = null };
        Assert.Null(t.SummaryField);
    }

    [Fact]
    public void TextElement_SummaryField_CanBeSet()
    {
        var t = new TextElement { SummaryField = "amount" };
        Assert.Equal("amount", t.SummaryField);
    }

    [Fact]
    public void TextElement_SystemVariable_CanBeNull()
    {
        var t = new TextElement { SystemVariable = null };
        Assert.Null(t.SystemVariable);
    }

    [Fact]
    public void TextElement_SystemVariable_CanBePageNumber()
    {
        var t = new TextElement { SystemVariable = "PageNumber" };
        Assert.Equal("PageNumber", t.SystemVariable);
    }

    [Fact]
    public void TextElement_SystemVariable_CanBeTotalPages()
    {
        var t = new TextElement { SystemVariable = "TotalPages" };
        Assert.Equal("TotalPages", t.SystemVariable);
    }

    [Fact]
    public void TextElement_SystemVariable_CanBePrintDate()
    {
        var t = new TextElement { SystemVariable = "PrintDate" };
        Assert.Equal("PrintDate", t.SystemVariable);
    }

    [Fact]
    public void TextElement_SystemVariable_CanBePrintTime()
    {
        var t = new TextElement { SystemVariable = "PrintTime" };
        Assert.Equal("PrintTime", t.SystemVariable);
    }

    [Fact]
    public void TextElement_SystemVariable_CanBeReportTitle()
    {
        var t = new TextElement { SystemVariable = "ReportTitle" };
        Assert.Equal("ReportTitle", t.SystemVariable);
    }

    [Fact]
    public void TextElement_Format_CanBeNull()
    {
        var t = new TextElement { Format = null };
        Assert.Null(t.Format);
    }

    [Fact]
    public void TextElement_Format_CanBeCurrency()
    {
        var t = new TextElement { Format = "currency" };
        Assert.Equal("currency", t.Format);
    }

    [Fact]
    public void TextElement_Format_CanBeDate()
    {
        var t = new TextElement { Format = "date" };
        Assert.Equal("date", t.Format);
    }

    [Fact]
    public void TextElement_Format_CanBePercent()
    {
        var t = new TextElement { Format = "percent" };
        Assert.Equal("percent", t.Format);
    }

    [Fact]
    public void TextElement_Format_CanBeNumberWithDecimal()
    {
        var t = new TextElement { Format = "number:2" };
        Assert.Equal("number:2", t.Format);
    }

    [Fact]
    public void TextElement_Alignment_DefaultLeft()
    {
        var t = new TextElement();
        Assert.Equal(TextAlignment.Left, t.Alignment);
    }

    [Fact]
    public void TextElement_Alignment_CanBeCenter()
    {
        var t = new TextElement { Alignment = TextAlignment.Center };
        Assert.Equal(TextAlignment.Center, t.Alignment);
    }

    [Fact]
    public void TextElement_Alignment_CanBeRight()
    {
        var t = new TextElement { Alignment = TextAlignment.Right };
        Assert.Equal(TextAlignment.Right, t.Alignment);
    }

    [Fact]
    public void TextElement_Alignment_CanBeJustify()
    {
        var t = new TextElement { Alignment = TextAlignment.Justify };
        Assert.Equal(TextAlignment.Justify, t.Alignment);
    }

    [Fact]
    public void TextElement_Font_DefaultNotNull()
    {
        var t = new TextElement();
        Assert.NotNull(t.Font);
        Assert.Equal("SimSun", t.Font.Family);
    }

    [Fact]
    public void TextElement_Font_CanBeReplaced()
    {
        var t = new TextElement { Font = new FontDef { Family = "Arial", Size = 14 } };
        Assert.Equal("Arial", t.Font.Family);
        Assert.Equal(14, t.Font.Size);
    }

    [Fact]
    public void TextElement_CanGrow_DefaultFalse()
    {
        var t = new TextElement();
        Assert.False(t.CanGrow);
    }

    [Fact]
    public void TextElement_CanGrow_CanBeTrue()
    {
        var t = new TextElement { CanGrow = true };
        Assert.True(t.CanGrow);
    }

    [Fact]
    public void TextElement_CanShrink_DefaultFalse()
    {
        var t = new TextElement();
        Assert.False(t.CanShrink);
    }

    [Fact]
    public void TextElement_CanShrink_CanBeTrue()
    {
        var t = new TextElement { CanShrink = true };
        Assert.True(t.CanShrink);
    }

    [Fact]
    public void TextElement_MaxLines_Default0()
    {
        var t = new TextElement();
        Assert.Equal(0, t.MaxLines);
    }

    [Fact]
    public void TextElement_MaxLines_CanBePositive()
    {
        var t = new TextElement { MaxLines = 5 };
        Assert.Equal(5, t.MaxLines);
    }

    [Fact]
    public void TextElement_Hyperlink_CanBeNull()
    {
        var t = new TextElement { Hyperlink = null };
        Assert.Null(t.Hyperlink);
    }

    [Fact]
    public void TextElement_Hyperlink_CanBeUrl()
    {
        var t = new TextElement { Hyperlink = "https://example.com" };
        Assert.Equal("https://example.com", t.Hyperlink);
    }

    [Fact]
    public void TextElement_Hyperlink_CanBeExpression()
    {
        var t = new TextElement { Hyperlink = "{{currentRow.link}}" };
        Assert.Equal("{{currentRow.link}}", t.Hyperlink);
    }

    // ============== BoxType 只读 getter ==============

    [Fact]
    public void TextElement_BoxType_DefaultStatic()
    {
        var t = new TextElement();
        Assert.Equal(TextBoxType.Static, t.BoxType);
    }

    [Fact]
    public void TextElement_BoxType_FieldWhenDataFieldSet()
    {
        var t = new TextElement { DataField = "name" };
        Assert.Equal(TextBoxType.Field, t.BoxType);
    }

    [Fact]
    public void TextElement_BoxType_SummaryWhenSummaryFunctionSet()
    {
        var t = new TextElement { SummaryFunction = "Sum" };
        Assert.Equal(TextBoxType.Summary, t.BoxType);
    }

    [Fact]
    public void TextElement_BoxType_SysVarWhenSystemVariableSet()
    {
        var t = new TextElement { SystemVariable = "PageNumber" };
        Assert.Equal(TextBoxType.SysVar, t.BoxType);
    }

    [Fact]
    public void TextElement_BoxType_SysVarPriorityOverSummary()
    {
        var t = new TextElement { SystemVariable = "PageNumber", SummaryFunction = "Sum" };
        Assert.Equal(TextBoxType.SysVar, t.BoxType);
    }

    [Fact]
    public void TextElement_BoxType_SummaryPriorityOverField()
    {
        var t = new TextElement { SummaryFunction = "Sum", DataField = "amount" };
        Assert.Equal(TextBoxType.Summary, t.BoxType);
    }

    [Fact]
    public void TextElement_BoxType_StaticWhenAllNull()
    {
        var t = new TextElement { Text = "static" };
        Assert.Equal(TextBoxType.Static, t.BoxType);
    }
}
