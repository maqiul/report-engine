using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// TextElement 行为测试：
///   - 默认值
///   - 文本内容
///   - 数据绑定
///   - 统计函数
///   - 系统变量
///   - 格式/对齐
///   - 字体
///   - CanGrow/CanShrink
///   - BoxType 计算逻辑
/// </summary>
public class TextElementBehaviorTests
{
    // ============== 默认值 ==============

    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var el = new TextElement();

        Assert.Equal("", el.Text);
        Assert.Null(el.DataField);
        Assert.Null(el.SummaryFunction);
        Assert.Null(el.SummaryField);
        Assert.Null(el.SystemVariable);
        Assert.Null(el.Format);
        Assert.Equal(TextAlignment.Left, el.Alignment);
        Assert.NotNull(el.Font);
        Assert.False(el.CanGrow);
        Assert.False(el.CanShrink);
        Assert.Equal(0, el.MaxLines);
        Assert.Null(el.Hyperlink);
        Assert.Equal(TextBoxType.Static, el.BoxType);
    }

    // ============== Text ==============

    [Fact]
    public void Text_EmptyByDefault()
    {
        var el = new TextElement();
        Assert.Equal("", el.Text);
    }

    [Fact]
    public void Text_SetAndGet_Works()
    {
        var el = new TextElement { Text = "Hello World" };
        Assert.Equal("Hello World", el.Text);
    }

    [Fact]
    public void Text_ChineseCharacters_Works()
    {
        var el = new TextElement { Text = "报表标题" };
        Assert.Equal("报表标题", el.Text);
    }

    [Fact]
    public void Text_LongText_Works()
    {
        var longText = new string('A', 1000);
        var el = new TextElement { Text = longText };
        Assert.Equal(1000, el.Text.Length);
    }

    // ============== DataField ==============

    [Fact]
    public void DataField_NullByDefault()
    {
        var el = new TextElement();
        Assert.Null(el.DataField);
    }

    [Fact]
    public void DataField_SetAndGet_Works()
    {
        var el = new TextElement { DataField = "CustomerName" };
        Assert.Equal("CustomerName", el.DataField);
    }

    [Fact]
    public void DataField_NestedField_Works()
    {
        var el = new TextElement { DataField = "order.customer.name" };
        Assert.Equal("order.customer.name", el.DataField);
    }

    // ============== SummaryFunction ==============

    [Fact]
    public void SummaryFunction_NullByDefault()
    {
        var el = new TextElement();
        Assert.Null(el.SummaryFunction);
    }

    [Fact]
    public void SummaryFunction_SetSum_Works()
    {
        var el = new TextElement { SummaryFunction = "Sum" };
        Assert.Equal("Sum", el.SummaryFunction);
    }

    [Fact]
    public void SummaryFunction_SetCount_Works()
    {
        var el = new TextElement { SummaryFunction = "Count" };
        Assert.Equal("Count", el.SummaryFunction);
    }

    [Fact]
    public void SummaryFunction_SetAvg_Works()
    {
        var el = new TextElement { SummaryFunction = "Avg" };
        Assert.Equal("Avg", el.SummaryFunction);
    }

    [Fact]
    public void SummaryFunction_SetMax_Works()
    {
        var el = new TextElement { SummaryFunction = "Max" };
        Assert.Equal("Max", el.SummaryFunction);
    }

    [Fact]
    public void SummaryFunction_SetMin_Works()
    {
        var el = new TextElement { SummaryFunction = "Min" };
        Assert.Equal("Min", el.SummaryFunction);
    }

    // ============== SummaryField ==============

    [Fact]
    public void SummaryField_NullByDefault()
    {
        var el = new TextElement();
        Assert.Null(el.SummaryField);
    }

    [Fact]
    public void SummaryField_SetAndGet_Works()
    {
        var el = new TextElement { SummaryFunction = "Sum", SummaryField = "Amount" };
        Assert.Equal("Amount", el.SummaryField);
    }

    // ============== SystemVariable ==============

    [Fact]
    public void SystemVariable_NullByDefault()
    {
        var el = new TextElement();
        Assert.Null(el.SystemVariable);
    }

    [Fact]
    public void SystemVariable_SetPageNumber_Works()
    {
        var el = new TextElement { SystemVariable = "PageNumber" };
        Assert.Equal("PageNumber", el.SystemVariable);
    }

    [Fact]
    public void SystemVariable_SetTotalPages_Works()
    {
        var el = new TextElement { SystemVariable = "TotalPages" };
        Assert.Equal("TotalPages", el.SystemVariable);
    }

    [Fact]
    public void SystemVariable_SetPrintDate_Works()
    {
        var el = new TextElement { SystemVariable = "PrintDate" };
        Assert.Equal("PrintDate", el.SystemVariable);
    }

    [Fact]
    public void SystemVariable_SetPrintTime_Works()
    {
        var el = new TextElement { SystemVariable = "PrintTime" };
        Assert.Equal("PrintTime", el.SystemVariable);
    }

    [Fact]
    public void SystemVariable_SetReportTitle_Works()
    {
        var el = new TextElement { SystemVariable = "ReportTitle" };
        Assert.Equal("ReportTitle", el.SystemVariable);
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
    public void Format_SetNumber_Works()
    {
        var el = new TextElement { Format = "number:2" };
        Assert.Equal("number:2", el.Format);
    }

    // ============== Alignment ==============

    [Fact]
    public void Alignment_DefaultIsLeft()
    {
        var el = new TextElement();
        Assert.Equal(TextAlignment.Left, el.Alignment);
    }

    [Fact]
    public void Alignment_SetCenter_Works()
    {
        var el = new TextElement { Alignment = TextAlignment.Center };
        Assert.Equal(TextAlignment.Center, el.Alignment);
    }

    [Fact]
    public void Alignment_SetRight_Works()
    {
        var el = new TextElement { Alignment = TextAlignment.Right };
        Assert.Equal(TextAlignment.Right, el.Alignment);
    }

    [Fact]
    public void Alignment_SetJustify_Works()
    {
        var el = new TextElement { Alignment = TextAlignment.Justify };
        Assert.Equal(TextAlignment.Justify, el.Alignment);
    }

    // ============== Font ==============

    [Fact]
    public void Font_NotNull_ByDefault()
    {
        var el = new TextElement();
        Assert.NotNull(el.Font);
    }

    [Fact]
    public void Font_DefaultValues_AreCorrect()
    {
        var el = new TextElement();
        Assert.Equal("SimSun", el.Font.Family);
        Assert.Equal(10, el.Font.Size);
        Assert.False(el.Font.Bold);
    }

    [Fact]
    public void Font_CanBeCustomized()
    {
        var el = new TextElement
        {
            Font = new FontDef { Family = "Arial", Size = 14, Bold = true }
        };
        Assert.Equal("Arial", el.Font.Family);
        Assert.Equal(14, el.Font.Size);
        Assert.True(el.Font.Bold);
    }

    // ============== CanGrow/CanShrink ==============

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
    public void Hyperlink_SetExpression_Works()
    {
        var el = new TextElement { Hyperlink = "='https://example.com/' + [Id]" };
        Assert.Contains("=", el.Hyperlink);
    }

    // ============== BoxType 计算逻辑 ==============

    [Fact]
    public void BoxType_DefaultIsStatic()
    {
        var el = new TextElement();
        Assert.Equal(TextBoxType.Static, el.BoxType);
    }

    [Fact]
    public void BoxType_WithDataField_IsField()
    {
        var el = new TextElement { DataField = "Name" };
        Assert.Equal(TextBoxType.Field, el.BoxType);
    }

    [Fact]
    public void BoxType_WithSummaryFunction_IsSummary()
    {
        var el = new TextElement { SummaryFunction = "Sum", SummaryField = "Amount" };
        Assert.Equal(TextBoxType.Summary, el.BoxType);
    }

    [Fact]
    public void BoxType_WithSystemVariable_IsSysVar()
    {
        var el = new TextElement { SystemVariable = "PageNumber" };
        Assert.Equal(TextBoxType.SysVar, el.BoxType);
    }

    [Fact]
    public void BoxType_SysVar_HasPriority_OverSummary()
    {
        var el = new TextElement
        {
            SystemVariable = "PageNumber",
            SummaryFunction = "Sum",
            DataField = "Name"
        };
        Assert.Equal(TextBoxType.SysVar, el.BoxType);
    }

    [Fact]
    public void BoxType_Summary_HasPriority_OverField()
    {
        var el = new TextElement
        {
            SummaryFunction = "Sum",
            DataField = "Name"
        };
        Assert.Equal(TextBoxType.Summary, el.BoxType);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void TextElement_StaticText_Works()
    {
        var el = new TextElement
        {
            Text = "报表标题",
            Alignment = TextAlignment.Center,
            Font = new FontDef { Family = "微软雅黑", Size = 18, Bold = true }
        };

        Assert.Equal("报表标题", el.Text);
        Assert.Equal(TextAlignment.Center, el.Alignment);
        Assert.Equal(TextBoxType.Static, el.BoxType);
    }

    [Fact]
    public void TextElement_DataField_Works()
    {
        var el = new TextElement
        {
            DataField = "CustomerName",
            Format = "string",
            Alignment = TextAlignment.Left
        };

        Assert.Equal("CustomerName", el.DataField);
        Assert.Equal(TextBoxType.Field, el.BoxType);
    }

    [Fact]
    public void TextElement_Summary_Works()
    {
        var el = new TextElement
        {
            SummaryFunction = "Sum",
            SummaryField = "Amount",
            Format = "currency",
            Alignment = TextAlignment.Right
        };

        Assert.Equal("Sum", el.SummaryFunction);
        Assert.Equal("Amount", el.SummaryField);
        Assert.Equal(TextBoxType.Summary, el.BoxType);
    }

    [Fact]
    public void TextElement_SysVar_Works()
    {
        var el = new TextElement
        {
            SystemVariable = "PageNumber",
            Format = "number"
        };

        Assert.Equal("PageNumber", el.SystemVariable);
        Assert.Equal(TextBoxType.SysVar, el.BoxType);
    }

    [Fact]
    public void TextElement_CanBeModified()
    {
        var el = new TextElement { Text = "Hello" };
        
        el.Text = "World";
        el.DataField = "Name";
        el.Alignment = TextAlignment.Center;
        el.Font.Bold = true;
        
        Assert.Equal("World", el.Text);
        Assert.Equal("Name", el.DataField);
        Assert.Equal(TextAlignment.Center, el.Alignment);
        Assert.True(el.Font.Bold);
    }
}
