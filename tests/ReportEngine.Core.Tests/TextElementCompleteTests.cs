using System.Linq;
using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// TextElement 完整字段 + FontDef 完整字段 + ImageElement.Sizing 测试：
///   - TextElement 12 属性默认值
///   - TextElement.BoxType 优先级（SysVar > Summary > Field > Static）
///   - FontDef 6 属性默认值
///   - ImageElement.Sizing 枚举
///   - TextBoxType 4 枚举值
///   - TextAlignment 4 枚举值
/// </summary>
public class TextElementCompleteTests
{
    // ============== TextElement ==============

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
    public void TextElement_InheritsReportElementDefaults()
    {
        var t = new TextElement();
        Assert.NotNull(t.Id);
        Assert.NotEmpty(t.Id);
        Assert.True(t.Visible);
        Assert.Equal(1.0, t.Opacity);
    }

    [Fact]
    public void TextElement_BoxType_NoBinding_Static()
    {
        var t = new TextElement { Text = "hi" };
        Assert.Equal(TextBoxType.Static, t.BoxType);
    }

    [Fact]
    public void TextElement_BoxType_DataField_Field()
    {
        var t = new TextElement { DataField = "name" };
        Assert.Equal(TextBoxType.Field, t.BoxType);
    }

    [Fact]
    public void TextElement_BoxType_Summary_Summary()
    {
        var t = new TextElement
        {
            SummaryFunction = "Sum",
            SummaryField = "amount",
        };
        Assert.Equal(TextBoxType.Summary, t.BoxType);
    }

    [Fact]
    public void TextElement_BoxType_SystemVariable_SysVar()
    {
        var t = new TextElement { SystemVariable = "PageNumber" };
        Assert.Equal(TextBoxType.SysVar, t.BoxType);
    }

    [Fact]
    public void TextElement_BoxType_SysVarHasPriority()
    {
        // 同时设置 DataField 和 SystemVariable → SysVar 赢
        var t = new TextElement
        {
            DataField = "name",
            SystemVariable = "PageNumber",
        };
        Assert.Equal(TextBoxType.SysVar, t.BoxType);
    }

    [Fact]
    public void TextElement_BoxType_SummaryOverField()
    {
        var t = new TextElement
        {
            DataField = "name",
            SummaryFunction = "Sum",
        };
        Assert.Equal(TextBoxType.Summary, t.BoxType);
    }

    [Fact]
    public void TextElement_BoxType_EmptyStrings_TreatedAsNotSet()
    {
        // 空字符串被视为"未设置"
        var t = new TextElement { DataField = "" };
        Assert.Equal(TextBoxType.Static, t.BoxType);
    }

    [Fact]
    public void TextElement_CanGrow_CanBeSet()
    {
        var t = new TextElement { CanGrow = true, CanShrink = true };
        Assert.True(t.CanGrow);
        Assert.True(t.CanShrink);
    }

    [Fact]
    public void TextElement_MaxLines_CanBeSet()
    {
        var t = new TextElement { MaxLines = 5 };
        Assert.Equal(5, t.MaxLines);
    }

    [Fact]
    public void TextElement_Hyperlink_CanBeSet()
    {
        var t = new TextElement { Hyperlink = "https://example.com" };
        Assert.Equal("https://example.com", t.Hyperlink);
    }

    [Fact]
    public void TextElement_Format_CanBeSet()
    {
        var t = new TextElement { Format = "currency" };
        Assert.Equal("currency", t.Format);
    }

    [Fact]
    public void TextElement_Alignment_AllValues()
    {
        foreach (TextAlignment a in System.Enum.GetValues(typeof(TextAlignment)))
        {
            var t = new TextElement { Alignment = a };
            Assert.Equal(a, t.Alignment);
        }
    }

    [Fact]
    public void TextBoxType_HasFourValues()
    {
        Assert.True(System.Enum.IsDefined(typeof(TextBoxType), TextBoxType.Static));
        Assert.True(System.Enum.IsDefined(typeof(TextBoxType), TextBoxType.Field));
        Assert.True(System.Enum.IsDefined(typeof(TextBoxType), TextBoxType.Summary));
        Assert.True(System.Enum.IsDefined(typeof(TextBoxType), TextBoxType.SysVar));
    }

    [Fact]
    public void TextAlignment_HasFourValues()
    {
        Assert.True(System.Enum.IsDefined(typeof(TextAlignment), TextAlignment.Left));
        Assert.True(System.Enum.IsDefined(typeof(TextAlignment), TextAlignment.Center));
        Assert.True(System.Enum.IsDefined(typeof(TextAlignment), TextAlignment.Right));
        Assert.True(System.Enum.IsDefined(typeof(TextAlignment), TextAlignment.Justify));
    }

    // ============== FontDef ==============

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
    public void FontDef_CanSetAllProperties()
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
    public void FontDef_Color_DefaultsToNull()
    {
        var f = new FontDef();
        Assert.Null(f.Color);
    }

    [Fact]
    public void FontDef_Color_CanBeAnyString()
    {
        // Color 是 string，任意值接受
        var f = new FontDef { Color = "rgba(255,0,0,0.5)" };
        Assert.Equal("rgba(255,0,0,0.5)", f.Color);
    }

    [Fact]
    public void FontDef_Family_DefaultsToSimSun()
    {
        // 中文字体默认（已记入经验）
        var f = new FontDef();
        Assert.Equal("SimSun", f.Family);
    }

    // ============== ImageElement.Sizing ==============

    [Fact]
    public void ImageElement_Defaults()
    {
        var e = new ImageElement();
        Assert.Equal("", e.Source);
        Assert.Equal(ImageSizing.FitProportional, e.Sizing);
    }

    [Fact]
    public void ImageElement_Sizing_CanBeChanged()
    {
        var e = new ImageElement { Sizing = ImageSizing.Stretch };
        Assert.Equal(ImageSizing.Stretch, e.Sizing);
    }

    [Fact]
    public void ImageSizing_HasValues()
    {
        Assert.True(System.Enum.IsDefined(typeof(ImageSizing), ImageSizing.FitProportional));
        Assert.True(System.Enum.IsDefined(typeof(ImageSizing), ImageSizing.Stretch));
    }
}
