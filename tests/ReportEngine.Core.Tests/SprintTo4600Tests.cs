using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Rendering;
using Xunit;

namespace ReportEngine.Core.Tests;

// ─────────────────────────────────────────────────────────────────────────────
// TextElement 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class TextElementComplete5Tests
{
    [Fact]
    public void TextElement_Text_DefaultEmpty()
    {
        var t = new TextElement();
        Assert.Equal("", t.Text);
    }

    [Fact]
    public void TextElement_Text_SetValue()
    {
        var t = new TextElement { Text = "Hello World" };
        Assert.Equal("Hello World", t.Text);
    }

    [Fact]
    public void TextElement_DataField_DefaultNull()
    {
        var t = new TextElement();
        Assert.Null(t.DataField);
    }

    [Fact]
    public void TextElement_DataField_SetValue()
    {
        var t = new TextElement { DataField = "name" };
        Assert.Equal("name", t.DataField);
    }

    [Fact]
    public void TextElement_SummaryFunction_DefaultNull()
    {
        var t = new TextElement();
        Assert.Null(t.SummaryFunction);
    }

    [Fact]
    public void TextElement_SummaryFunction_SetSum()
    {
        var t = new TextElement { SummaryFunction = "Sum" };
        Assert.Equal("Sum", t.SummaryFunction);
    }

    [Fact]
    public void TextElement_SummaryFunction_SetAvg()
    {
        var t = new TextElement { SummaryFunction = "Avg" };
        Assert.Equal("Avg", t.SummaryFunction);
    }

    [Fact]
    public void TextElement_SummaryFunction_SetCount()
    {
        var t = new TextElement { SummaryFunction = "Count" };
        Assert.Equal("Count", t.SummaryFunction);
    }

    [Fact]
    public void TextElement_SummaryFunction_SetMax()
    {
        var t = new TextElement { SummaryFunction = "Max" };
        Assert.Equal("Max", t.SummaryFunction);
    }

    [Fact]
    public void TextElement_SummaryFunction_SetMin()
    {
        var t = new TextElement { SummaryFunction = "Min" };
        Assert.Equal("Min", t.SummaryFunction);
    }

    [Fact]
    public void TextElement_SummaryField_DefaultNull()
    {
        var t = new TextElement();
        Assert.Null(t.SummaryField);
    }

    [Fact]
    public void TextElement_SummaryField_SetValue()
    {
        var t = new TextElement { SummaryField = "amount" };
        Assert.Equal("amount", t.SummaryField);
    }

    [Fact]
    public void TextElement_SystemVariable_DefaultNull()
    {
        var t = new TextElement();
        Assert.Null(t.SystemVariable);
    }

    [Fact]
    public void TextElement_SystemVariable_SetPageNumber()
    {
        var t = new TextElement { SystemVariable = "PageNumber" };
        Assert.Equal("PageNumber", t.SystemVariable);
    }

    [Fact]
    public void TextElement_SystemVariable_SetTotalPages()
    {
        var t = new TextElement { SystemVariable = "TotalPages" };
        Assert.Equal("TotalPages", t.SystemVariable);
    }

    [Fact]
    public void TextElement_SystemVariable_SetPrintDate()
    {
        var t = new TextElement { SystemVariable = "PrintDate" };
        Assert.Equal("PrintDate", t.SystemVariable);
    }

    [Fact]
    public void TextElement_Format_DefaultNull()
    {
        var t = new TextElement();
        Assert.Null(t.Format);
    }

    [Fact]
    public void TextElement_Format_SetValue()
    {
        var t = new TextElement { Format = "0.00" };
        Assert.Equal("0.00", t.Format);
    }

    [Fact]
    public void TextElement_Alignment_DefaultLeft()
    {
        var t = new TextElement();
        Assert.Equal(TextAlignment.Left, t.Alignment);
    }

    [Fact]
    public void TextElement_Alignment_SetCenter()
    {
        var t = new TextElement { Alignment = TextAlignment.Center };
        Assert.Equal(TextAlignment.Center, t.Alignment);
    }

    [Fact]
    public void TextElement_Alignment_SetRight()
    {
        var t = new TextElement { Alignment = TextAlignment.Right };
        Assert.Equal(TextAlignment.Right, t.Alignment);
    }

    [Fact]
    public void TextElement_Font_DefaultNotNull()
    {
        var t = new TextElement();
        Assert.NotNull(t.Font);
    }

    [Fact]
    public void TextElement_Font_DefaultSimSun()
    {
        var t = new TextElement();
        Assert.Equal("SimSun", t.Font.Family);
    }

    [Fact]
    public void TextElement_Font_DefaultSize10()
    {
        var t = new TextElement();
        Assert.Equal(10, t.Font.Size);
    }

    [Fact]
    public void TextElement_Font_SetValue()
    {
        var t = new TextElement { Font = new FontDef { Family = "Arial", Size = 12 } };
        Assert.NotNull(t.Font);
        Assert.Equal("Arial", t.Font.Family);
        Assert.Equal(12, t.Font.Size);
    }

    [Fact]
    public void TextElement_CanGrow_DefaultFalse()
    {
        var t = new TextElement();
        Assert.False(t.CanGrow);
    }

    [Fact]
    public void TextElement_CanGrow_SetTrue()
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
    public void TextElement_CanShrink_SetTrue()
    {
        var t = new TextElement { CanShrink = true };
        Assert.True(t.CanShrink);
    }

    [Fact]
    public void TextElement_MaxLines_DefaultZero()
    {
        var t = new TextElement();
        Assert.Equal(0, t.MaxLines);
    }

    [Fact]
    public void TextElement_MaxLines_SetValue()
    {
        var t = new TextElement { MaxLines = 3 };
        Assert.Equal(3, t.MaxLines);
    }

    [Fact]
    public void TextElement_Hyperlink_DefaultNull()
    {
        var t = new TextElement();
        Assert.Null(t.Hyperlink);
    }

    [Fact]
    public void TextElement_Hyperlink_SetValue()
    {
        var t = new TextElement { Hyperlink = "https://example.com" };
        Assert.Equal("https://example.com", t.Hyperlink);
    }

    [Fact]
    public void TextElement_BoxType_DefaultStatic()
    {
        var t = new TextElement();
        Assert.Equal(TextBoxType.Static, t.BoxType);
    }

    [Fact]
    public void TextElement_BoxType_Field_WhenDataFieldSet()
    {
        var t = new TextElement { DataField = "name" };
        Assert.Equal(TextBoxType.Field, t.BoxType);
    }

    [Fact]
    public void TextElement_BoxType_Summary_WhenSummaryFunctionSet()
    {
        var t = new TextElement { SummaryFunction = "Sum" };
        Assert.Equal(TextBoxType.Summary, t.BoxType);
    }

    [Fact]
    public void TextElement_BoxType_SysVar_WhenSystemVariableSet()
    {
        var t = new TextElement { SystemVariable = "PageNumber" };
        Assert.Equal(TextBoxType.SysVar, t.BoxType);
    }

    [Fact]
    public void TextElement_BoxType_SysVar_TakesPrecedenceOverSummary()
    {
        var t = new TextElement { SystemVariable = "PageNumber", SummaryFunction = "Sum" };
        Assert.Equal(TextBoxType.SysVar, t.BoxType);
    }

    [Fact]
    public void TextElement_BoxType_Summary_TakesPrecedenceOverField()
    {
        var t = new TextElement { SummaryFunction = "Sum", DataField = "amount" };
        Assert.Equal(TextBoxType.Summary, t.BoxType);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// FontDef 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class FontDefComplete5Tests
{
    [Fact]
    public void FontDef_Family_DefaultSimSun()
    {
        var f = new FontDef();
        Assert.Equal("SimSun", f.Family);
    }

    [Fact]
    public void FontDef_Family_SetValue()
    {
        var f = new FontDef { Family = "Arial" };
        Assert.Equal("Arial", f.Family);
    }

    [Fact]
    public void FontDef_Size_Default10()
    {
        var f = new FontDef();
        Assert.Equal(10, f.Size);
    }

    [Fact]
    public void FontDef_Size_SetValue()
    {
        var f = new FontDef { Size = 14 };
        Assert.Equal(14, f.Size);
    }

    [Fact]
    public void FontDef_Bold_DefaultFalse()
    {
        var f = new FontDef();
        Assert.False(f.Bold);
    }

    [Fact]
    public void FontDef_Bold_SetTrue()
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
    public void FontDef_Italic_SetTrue()
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
    public void FontDef_Underline_SetTrue()
    {
        var f = new FontDef { Underline = true };
        Assert.True(f.Underline);
    }

    [Fact]
    public void FontDef_Color_DefaultNull()
    {
        var f = new FontDef();
        Assert.Null(f.Color);
    }

    [Fact]
    public void FontDef_Color_SetBlack()
    {
        var f = new FontDef { Color = "#000000" };
        Assert.Equal("#000000", f.Color);
    }

    [Fact]
    public void FontDef_Color_SetRed()
    {
        var f = new FontDef { Color = "#FF0000" };
        Assert.Equal("#FF0000", f.Color);
    }
}
