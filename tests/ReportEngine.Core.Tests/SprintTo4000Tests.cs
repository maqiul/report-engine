using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Rendering;
using ReportEngine.Core.Parsing;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// TextElement 更多属性测试（CanGrow/CanShrink/MaxLines/Hyperlink/Format/SummaryField/SystemVariable）
/// </summary>
public class TextElementExtraPropsTests
{
    [Fact]
    public void TextElement_CanGrow_DefaultFalse()
    {
        var el = new TextElement();
        Assert.False(el.CanGrow);
    }

    [Fact]
    public void TextElement_CanGrow_SetTrue()
    {
        var el = new TextElement { CanGrow = true };
        Assert.True(el.CanGrow);
    }

    [Fact]
    public void TextElement_CanShrink_DefaultFalse()
    {
        var el = new TextElement();
        Assert.False(el.CanShrink);
    }

    [Fact]
    public void TextElement_CanShrink_SetTrue()
    {
        var el = new TextElement { CanShrink = true };
        Assert.True(el.CanShrink);
    }

    [Fact]
    public void TextElement_MaxLines_DefaultZero()
    {
        var el = new TextElement();
        Assert.Equal(0, el.MaxLines);
    }

    [Fact]
    public void TextElement_MaxLines_SetValue()
    {
        var el = new TextElement { MaxLines = 5 };
        Assert.Equal(5, el.MaxLines);
    }

    [Fact]
    public void TextElement_Hyperlink_DefaultNull()
    {
        var el = new TextElement();
        Assert.Null(el.Hyperlink);
    }

    [Fact]
    public void TextElement_Hyperlink_SetValue()
    {
        var el = new TextElement { Hyperlink = "https://example.com" };
        Assert.Equal("https://example.com", el.Hyperlink);
    }

    [Fact]
    public void TextElement_Format_DefaultNull()
    {
        var el = new TextElement();
        Assert.Null(el.Format);
    }

    [Fact]
    public void TextElement_Format_Currency()
    {
        var el = new TextElement { Format = "currency" };
        Assert.Equal("currency", el.Format);
    }

    [Fact]
    public void TextElement_Format_Date()
    {
        var el = new TextElement { Format = "date" };
        Assert.Equal("date", el.Format);
    }

    [Fact]
    public void TextElement_Format_Percent()
    {
        var el = new TextElement { Format = "percent" };
        Assert.Equal("percent", el.Format);
    }

    [Fact]
    public void TextElement_SummaryField_DefaultNull()
    {
        var el = new TextElement();
        Assert.Null(el.SummaryField);
    }

    [Fact]
    public void TextElement_SummaryField_SetValue()
    {
        var el = new TextElement { SummaryField = "amount" };
        Assert.Equal("amount", el.SummaryField);
    }

    [Fact]
    public void TextElement_SystemVariable_DefaultNull()
    {
        var el = new TextElement();
        Assert.Null(el.SystemVariable);
    }

    [Fact]
    public void TextElement_SystemVariable_PageNumber()
    {
        var el = new TextElement { SystemVariable = "PageNumber" };
        Assert.Equal("PageNumber", el.SystemVariable);
    }

    [Fact]
    public void TextElement_SystemVariable_TotalPages()
    {
        var el = new TextElement { SystemVariable = "TotalPages" };
        Assert.Equal("TotalPages", el.SystemVariable);
    }

    [Fact]
    public void TextElement_SystemVariable_PrintDate()
    {
        var el = new TextElement { SystemVariable = "PrintDate" };
        Assert.Equal("PrintDate", el.SystemVariable);
    }

    [Fact]
    public void TextElement_SystemVariable_PrintTime()
    {
        var el = new TextElement { SystemVariable = "PrintTime" };
        Assert.Equal("PrintTime", el.SystemVariable);
    }

    [Fact]
    public void TextElement_SystemVariable_ReportTitle()
    {
        var el = new TextElement { SystemVariable = "ReportTitle" };
        Assert.Equal("ReportTitle", el.SystemVariable);
    }
}

/// <summary>
/// CrossTabElement 更多属性测试
/// </summary>
public class CrossTabElementExtraPropsTests
{
    [Fact]
    public void CrossTabElement_RowFields_DefaultEmpty()
    {
        var el = new CrossTabElement();
        Assert.NotNull(el.RowFields);
        Assert.Empty(el.RowFields);
    }

    [Fact]
    public void CrossTabElement_RowFields_AddItems()
    {
        var el = new CrossTabElement();
        el.RowFields.Add("region");
        el.RowFields.Add("product");
        Assert.Equal(2, el.RowFields.Count);
    }

    [Fact]
    public void CrossTabElement_ColumnFields_DefaultEmpty()
    {
        var el = new CrossTabElement();
        Assert.NotNull(el.ColumnFields);
        Assert.Empty(el.ColumnFields);
    }

    [Fact]
    public void CrossTabElement_ColumnFields_AddItems()
    {
        var el = new CrossTabElement();
        el.ColumnFields.Add("year");
        el.ColumnFields.Add("quarter");
        Assert.Equal(2, el.ColumnFields.Count);
    }

    [Fact]
    public void CrossTabElement_Measures_DefaultEmpty()
    {
        var el = new CrossTabElement();
        Assert.NotNull(el.Measures);
        Assert.Empty(el.Measures);
    }

    [Fact]
    public void CrossTabElement_Measures_AddItems()
    {
        var el = new CrossTabElement();
        el.Measures.Add(new CrossTabMeasure { Field = "amount", Aggregate = "Sum" });
        el.Measures.Add(new CrossTabMeasure { Field = "count", Aggregate = "Count" });
        Assert.Equal(2, el.Measures.Count);
    }

    [Fact]
    public void CrossTabElement_ShowRowTotal_DefaultTrue()
    {
        var el = new CrossTabElement();
        Assert.True(el.ShowRowTotal);
    }

    [Fact]
    public void CrossTabElement_ShowRowTotal_SetFalse()
    {
        var el = new CrossTabElement { ShowRowTotal = false };
        Assert.False(el.ShowRowTotal);
    }

    [Fact]
    public void CrossTabElement_ShowColumnTotal_DefaultTrue()
    {
        var el = new CrossTabElement();
        Assert.True(el.ShowColumnTotal);
    }

    [Fact]
    public void CrossTabElement_ShowColumnTotal_SetFalse()
    {
        var el = new CrossTabElement { ShowColumnTotal = false };
        Assert.False(el.ShowColumnTotal);
    }

    [Fact]
    public void CrossTabElement_CellFont_DefaultNotNull()
    {
        var el = new CrossTabElement();
        Assert.NotNull(el.CellFont);
    }

    [Fact]
    public void CrossTabElement_CellFont_SetSize()
    {
        var el = new CrossTabElement();
        el.CellFont.Size = 12;
        Assert.Equal(12, el.CellFont.Size);
    }

    [Fact]
    public void CrossTabElement_HeaderFont_DefaultNotNull()
    {
        var el = new CrossTabElement();
        Assert.NotNull(el.HeaderFont);
    }

    [Fact]
    public void CrossTabElement_HeaderFont_DefaultBold()
    {
        var el = new CrossTabElement();
        Assert.True(el.HeaderFont.Bold);
    }

    [Fact]
    public void CrossTabElement_CellPadding_Default1()
    {
        var el = new CrossTabElement();
        Assert.Equal(1, el.CellPadding);
    }

    [Fact]
    public void CrossTabElement_CellPadding_SetValue()
    {
        var el = new CrossTabElement { CellPadding = 2.5 };
        Assert.Equal(2.5, el.CellPadding);
    }

    [Fact]
    public void CrossTabElement_BorderWidth_Default03()
    {
        var el = new CrossTabElement();
        Assert.Equal(0.3, el.BorderWidth);
    }

    [Fact]
    public void CrossTabElement_BorderColor_Default000000()
    {
        var el = new CrossTabElement();
        Assert.Equal("#000000", el.BorderColor);
    }
}

/// <summary>
/// 枚举完整性测试 2
/// </summary>
public class EnumCompleteness2Tests
{
    [Fact]
    public void BandType_Has7Values()
    {
        var values = Enum.GetValues(typeof(BandType));
        Assert.Equal(7, values.Length);
    }

    [Fact]
    public void BandType_ContainsAllExpected()
    {
        Assert.True(Enum.IsDefined(typeof(BandType), BandType.Header));
        Assert.True(Enum.IsDefined(typeof(BandType), BandType.Footer));
        Assert.True(Enum.IsDefined(typeof(BandType), BandType.ReportHeader));
        Assert.True(Enum.IsDefined(typeof(BandType), BandType.ReportFooter));
        Assert.True(Enum.IsDefined(typeof(BandType), BandType.Detail));
        Assert.True(Enum.IsDefined(typeof(BandType), BandType.GroupHeader));
        Assert.True(Enum.IsDefined(typeof(BandType), BandType.GroupFooter));
    }

    [Fact]
    public void TextAlignment_Has4Values()
    {
        var values = Enum.GetValues(typeof(TextAlignment));
        Assert.Equal(4, values.Length);
    }

    [Fact]
    public void TextAlignment_ContainsAllExpected()
    {
        Assert.True(Enum.IsDefined(typeof(TextAlignment), TextAlignment.Left));
        Assert.True(Enum.IsDefined(typeof(TextAlignment), TextAlignment.Center));
        Assert.True(Enum.IsDefined(typeof(TextAlignment), TextAlignment.Right));
        Assert.True(Enum.IsDefined(typeof(TextAlignment), TextAlignment.Justify));
    }

    [Fact]
    public void BorderStyle_Has4Values()
    {
        var values = Enum.GetValues(typeof(BorderStyle));
        Assert.Equal(4, values.Length);
    }

    [Fact]
    public void BorderStyle_ContainsAllExpected()
    {
        Assert.True(Enum.IsDefined(typeof(BorderStyle), BorderStyle.Solid));
        Assert.True(Enum.IsDefined(typeof(BorderStyle), BorderStyle.Dashed));
        Assert.True(Enum.IsDefined(typeof(BorderStyle), BorderStyle.Dotted));
        Assert.True(Enum.IsDefined(typeof(BorderStyle), BorderStyle.None));
    }

    [Fact]
    public void ImageSizing_Has4Values()
    {
        var values = Enum.GetValues(typeof(ImageSizing));
        Assert.Equal(4, values.Length);
    }

    [Fact]
    public void ImageSizing_ContainsAllExpected()
    {
        Assert.True(Enum.IsDefined(typeof(ImageSizing), ImageSizing.Stretch));
        Assert.True(Enum.IsDefined(typeof(ImageSizing), ImageSizing.FitProportional));
        Assert.True(Enum.IsDefined(typeof(ImageSizing), ImageSizing.Clip));
        Assert.True(Enum.IsDefined(typeof(ImageSizing), ImageSizing.ActualSize));
    }

    [Fact]
    public void LineDirection_Has3Values()
    {
        var values = Enum.GetValues(typeof(LineDirection));
        Assert.Equal(3, values.Length);
    }

    [Fact]
    public void LineDirection_ContainsAllExpected()
    {
        Assert.True(Enum.IsDefined(typeof(LineDirection), LineDirection.Horizontal));
        Assert.True(Enum.IsDefined(typeof(LineDirection), LineDirection.Vertical));
        Assert.True(Enum.IsDefined(typeof(LineDirection), LineDirection.Diagonal));
    }

    [Fact]
    public void ShapeType_Has4Values()
    {
        var values = Enum.GetValues(typeof(ShapeType));
        Assert.Equal(4, values.Length);
    }

    [Fact]
    public void ShapeType_ContainsAllExpected()
    {
        Assert.True(Enum.IsDefined(typeof(ShapeType), ShapeType.Rectangle));
        Assert.True(Enum.IsDefined(typeof(ShapeType), ShapeType.Ellipse));
        Assert.True(Enum.IsDefined(typeof(ShapeType), ShapeType.RoundedRect));
        Assert.True(Enum.IsDefined(typeof(ShapeType), ShapeType.Triangle));
    }

    [Fact]
    public void ChartType_Has5Values()
    {
        var values = Enum.GetValues(typeof(ChartType));
        Assert.Equal(5, values.Length);
    }

    [Fact]
    public void ChartType_ContainsAllExpected()
    {
        Assert.True(Enum.IsDefined(typeof(ChartType), ChartType.Bar));
        Assert.True(Enum.IsDefined(typeof(ChartType), ChartType.Line));
        Assert.True(Enum.IsDefined(typeof(ChartType), ChartType.Pie));
        Assert.True(Enum.IsDefined(typeof(ChartType), ChartType.Area));
        Assert.True(Enum.IsDefined(typeof(ChartType), ChartType.Scatter));
    }

    [Fact]
    public void BarcodeFormat_Has8Values()
    {
        var values = Enum.GetValues(typeof(BarcodeFormat));
        Assert.Equal(8, values.Length);
    }

    [Fact]
    public void BarcodeFormat_ContainsAllExpected()
    {
        Assert.True(Enum.IsDefined(typeof(BarcodeFormat), BarcodeFormat.Code128));
        Assert.True(Enum.IsDefined(typeof(BarcodeFormat), BarcodeFormat.Code39));
        Assert.True(Enum.IsDefined(typeof(BarcodeFormat), BarcodeFormat.EAN13));
        Assert.True(Enum.IsDefined(typeof(BarcodeFormat), BarcodeFormat.EAN8));
        Assert.True(Enum.IsDefined(typeof(BarcodeFormat), BarcodeFormat.UPC_A));
        Assert.True(Enum.IsDefined(typeof(BarcodeFormat), BarcodeFormat.QRCode));
        Assert.True(Enum.IsDefined(typeof(BarcodeFormat), BarcodeFormat.DataMatrix));
        Assert.True(Enum.IsDefined(typeof(BarcodeFormat), BarcodeFormat.PDF417));
    }
}
