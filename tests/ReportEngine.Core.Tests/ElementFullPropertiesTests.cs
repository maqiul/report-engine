using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Rendering;
using ReportEngine.Core.Parsing;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// TextElement 完整属性测试
/// </summary>
public class TextElementFullTests
{
    [Fact]
    public void TextElement_Text_DefaultEmpty()
    {
        var el = new TextElement();
        Assert.Equal("", el.Text);
    }

    [Fact]
    public void TextElement_Text_Settable()
    {
        var el = new TextElement { Text = "Hello World" };
        Assert.Equal("Hello World", el.Text);
    }

    [Fact]
    public void TextElement_DataField_DefaultNull()
    {
        var el = new TextElement();
        Assert.Null(el.DataField);
    }

    [Fact]
    public void TextElement_DataField_Settable()
    {
        var el = new TextElement { DataField = "CustomerName" };
        Assert.Equal("CustomerName", el.DataField);
    }

    [Fact]
    public void TextElement_SummaryFunction_DefaultNull()
    {
        var el = new TextElement();
        Assert.Null(el.SummaryFunction);
    }

    [Fact]
    public void TextElement_SummaryFunction_Settable()
    {
        var el = new TextElement { SummaryFunction = "Sum" };
        Assert.Equal("Sum", el.SummaryFunction);
    }

    [Fact]
    public void TextElement_Font_DefaultNotNull()
    {
        var el = new TextElement();
        Assert.NotNull(el.Font);
    }

    [Fact]
    public void TextElement_Font_Size10()
    {
        var el = new TextElement();
        Assert.Equal(10, el.Font.Size);
    }

    [Fact]
    public void TextElement_Alignment_DefaultLeft()
    {
        var el = new TextElement();
        Assert.Equal(TextAlignment.Left, el.Alignment);
    }

    [Fact]
    public void TextElement_Alignment_Center()
    {
        var el = new TextElement { Alignment = TextAlignment.Center };
        Assert.Equal(TextAlignment.Center, el.Alignment);
    }

    [Fact]
    public void TextElement_Alignment_Right()
    {
        var el = new TextElement { Alignment = TextAlignment.Right };
        Assert.Equal(TextAlignment.Right, el.Alignment);
    }

    [Fact]
    public void TextElement_Alignment_Justify()
    {
        var el = new TextElement { Alignment = TextAlignment.Justify };
        Assert.Equal(TextAlignment.Justify, el.Alignment);
    }

    [Fact]
    public void TextElement_CanGrow_DefaultFalse()
    {
        var el = new TextElement();
        Assert.False(el.CanGrow);
    }

    [Fact]
    public void TextElement_CanGrow_Settable()
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
    public void TextElement_CanShrink_Settable()
    {
        var el = new TextElement { CanShrink = true };
        Assert.True(el.CanShrink);
    }

    [Fact]
    public void TextElement_MaxLines_Default0()
    {
        var el = new TextElement();
        Assert.Equal(0, el.MaxLines);
    }

    [Fact]
    public void TextElement_MaxLines_Settable()
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
    public void TextElement_Hyperlink_Settable()
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
    public void TextElement_Format_Settable()
    {
        var el = new TextElement { Format = "currency" };
        Assert.Equal("currency", el.Format);
    }

    [Fact]
    public void TextElement_SummaryField_DefaultNull()
    {
        var el = new TextElement();
        Assert.Null(el.SummaryField);
    }

    [Fact]
    public void TextElement_SummaryField_Settable()
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
    public void TextElement_SystemVariable_Settable()
    {
        var el = new TextElement { SystemVariable = "PageNumber" };
        Assert.Equal("PageNumber", el.SystemVariable);
    }

    [Fact]
    public void TextElement_FullSetup()
    {
        var el = new TextElement
        {
            Text = "{{name}}",
            DataField = "name",
            Font = new FontDef { Size = 14, Bold = true, Color = "#333333" },
            Alignment = TextAlignment.Center,
            CanGrow = true,
            CanShrink = true,
            MaxLines = 3,
            Hyperlink = "https://example.com",
            Format = "currency"
        };
        Assert.Equal("{{name}}", el.Text);
        Assert.Equal("name", el.DataField);
        Assert.Equal(14, el.Font.Size);
        Assert.True(el.Font.Bold);
        Assert.Equal(TextAlignment.Center, el.Alignment);
        Assert.True(el.CanGrow);
        Assert.True(el.CanShrink);
        Assert.Equal(3, el.MaxLines);
        Assert.Equal("https://example.com", el.Hyperlink);
        Assert.Equal("currency", el.Format);
    }
}

/// <summary>
/// ImageElement 完整属性测试
/// </summary>
public class ImageElementFullTests
{
    [Fact]
    public void ImageElement_Source_DefaultEmpty()
    {
        var el = new ImageElement();
        Assert.Equal("", el.Source);
    }

    [Fact]
    public void ImageElement_Source_Settable()
    {
        var el = new ImageElement { Source = "logo.png" };
        Assert.Equal("logo.png", el.Source);
    }

    [Fact]
    public void ImageElement_Sizing_DefaultFitProportional()
    {
        var el = new ImageElement();
        Assert.Equal(ImageSizing.FitProportional, el.Sizing);
    }

    [Fact]
    public void ImageElement_Sizing_Stretch()
    {
        var el = new ImageElement { Sizing = ImageSizing.Stretch };
        Assert.Equal(ImageSizing.Stretch, el.Sizing);
    }

    [Fact]
    public void ImageElement_Sizing_Clip()
    {
        var el = new ImageElement { Sizing = ImageSizing.Clip };
        Assert.Equal(ImageSizing.Clip, el.Sizing);
    }

    [Fact]
    public void ImageElement_Sizing_ActualSize()
    {
        var el = new ImageElement { Sizing = ImageSizing.ActualSize };
        Assert.Equal(ImageSizing.ActualSize, el.Sizing);
    }

    [Fact]
    public void ImageElement_FullSetup()
    {
        var el = new ImageElement
        {
            X = 10, Y = 20, Width = 100, Height = 80,
            Source = "data:image/png;base64,abc",
            Sizing = ImageSizing.Stretch
        };
        Assert.Equal(10, el.X);
        Assert.Equal(20, el.Y);
        Assert.Equal(100, el.Width);
        Assert.Equal(80, el.Height);
        Assert.StartsWith("data:image/png;base64,", el.Source);
        Assert.Equal(ImageSizing.Stretch, el.Sizing);
    }
}

/// <summary>
/// LineElement 完整属性测试
/// </summary>
public class LineElementFullTests
{
    [Fact]
    public void LineElement_Direction_DefaultHorizontal()
    {
        var el = new LineElement();
        Assert.Equal(LineDirection.Horizontal, el.Direction);
    }

    [Fact]
    public void LineElement_Direction_Vertical()
    {
        var el = new LineElement { Direction = LineDirection.Vertical };
        Assert.Equal(LineDirection.Vertical, el.Direction);
    }

    [Fact]
    public void LineElement_Direction_Diagonal()
    {
        var el = new LineElement { Direction = LineDirection.Diagonal };
        Assert.Equal(LineDirection.Diagonal, el.Direction);
    }

    [Fact]
    public void LineElement_LineWidth_Default1()
    {
        var el = new LineElement();
        Assert.Equal(1, el.LineWidth);
    }

    [Fact]
    public void LineElement_LineWidth_Settable()
    {
        var el = new LineElement { LineWidth = 2.5 };
        Assert.Equal(2.5, el.LineWidth);
    }

    [Fact]
    public void LineElement_LineColor_Default000000()
    {
        var el = new LineElement();
        Assert.Equal("#000000", el.LineColor);
    }

    [Fact]
    public void LineElement_LineColor_Settable()
    {
        var el = new LineElement { LineColor = "#FF0000" };
        Assert.Equal("#FF0000", el.LineColor);
    }

    [Fact]
    public void LineElement_FullSetup()
    {
        var el = new LineElement
        {
            X = 0, Y = 100, Width = 200, Height = 0,
            Direction = LineDirection.Horizontal,
            LineWidth = 1.5,
            LineColor = "#0000FF"
        };
        Assert.Equal(0, el.X);
        Assert.Equal(100, el.Y);
        Assert.Equal(200, el.Width);
        Assert.Equal(LineDirection.Horizontal, el.Direction);
        Assert.Equal(1.5, el.LineWidth);
        Assert.Equal("#0000FF", el.LineColor);
    }
}

/// <summary>
/// ShapeElement 完整属性测试
/// </summary>
public class ShapeElementFullTests
{
    [Fact]
    public void ShapeElement_Shape_DefaultRectangle()
    {
        var el = new ShapeElement();
        Assert.Equal(ShapeType.Rectangle, el.Shape);
    }

    [Fact]
    public void ShapeElement_Shape_Ellipse()
    {
        var el = new ShapeElement { Shape = ShapeType.Ellipse };
        Assert.Equal(ShapeType.Ellipse, el.Shape);
    }

    [Fact]
    public void ShapeElement_Shape_RoundedRect()
    {
        var el = new ShapeElement { Shape = ShapeType.RoundedRect };
        Assert.Equal(ShapeType.RoundedRect, el.Shape);
    }

    [Fact]
    public void ShapeElement_Shape_Triangle()
    {
        var el = new ShapeElement { Shape = ShapeType.Triangle };
        Assert.Equal(ShapeType.Triangle, el.Shape);
    }

    [Fact]
    public void ShapeElement_BorderRadius_Default0()
    {
        var el = new ShapeElement();
        Assert.Equal(0, el.BorderRadius);
    }

    [Fact]
    public void ShapeElement_BorderRadius_Settable()
    {
        var el = new ShapeElement { BorderRadius = 5 };
        Assert.Equal(5, el.BorderRadius);
    }

    [Fact]
    public void ShapeElement_FillColor_DefaultFFFFFF()
    {
        var el = new ShapeElement();
        Assert.Equal("#FFFFFF", el.FillColor);
    }

    [Fact]
    public void ShapeElement_FillColor_Settable()
    {
        var el = new ShapeElement { FillColor = "#00FF00" };
        Assert.Equal("#00FF00", el.FillColor);
    }

    [Fact]
    public void ShapeElement_FullSetup()
    {
        var el = new ShapeElement
        {
            X = 10, Y = 10, Width = 50, Height = 50,
            Shape = ShapeType.RoundedRect,
            BorderRadius = 8,
            FillColor = "#CCCCCC"
        };
        Assert.Equal(10, el.X);
        Assert.Equal(10, el.Y);
        Assert.Equal(50, el.Width);
        Assert.Equal(50, el.Height);
        Assert.Equal(ShapeType.RoundedRect, el.Shape);
        Assert.Equal(8, el.BorderRadius);
        Assert.Equal("#CCCCCC", el.FillColor);
    }
}

/// <summary>
/// SubReportElement 完整属性测试
/// </summary>
public class SubReportElementFullTests
{
    [Fact]
    public void SubReportElement_TemplateRef_DefaultEmpty()
    {
        var el = new SubReportElement();
        Assert.Equal("", el.TemplateRef);
    }

    [Fact]
    public void SubReportElement_TemplateRef_Settable()
    {
        var el = new SubReportElement { TemplateRef = "sub.rptx" };
        Assert.Equal("sub.rptx", el.TemplateRef);
    }

    [Fact]
    public void SubReportElement_DataBinding_DefaultNotNull()
    {
        var el = new SubReportElement();
        Assert.NotNull(el.DataBinding);
    }

    [Fact]
    public void SubReportElement_HeightMode_DefaultAuto()
    {
        var el = new SubReportElement();
        Assert.Equal("auto", el.HeightMode);
    }

    [Fact]
    public void SubReportElement_HeightMode_Fixed()
    {
        var el = new SubReportElement { HeightMode = "fixed" };
        Assert.Equal("fixed", el.HeightMode);
    }

    [Fact]
    public void SubReportElement_RepeatPerRow_DefaultTrue()
    {
        var el = new SubReportElement();
        Assert.True(el.RepeatPerRow);
    }

    [Fact]
    public void SubReportElement_RepeatPerRow_Settable()
    {
        var el = new SubReportElement { RepeatPerRow = false };
        Assert.False(el.RepeatPerRow);
    }

    [Fact]
    public void SubReportElement_FullSetup()
    {
        var el = new SubReportElement
        {
            X = 10, Y = 50, Width = 180, Height = 100,
            TemplateRef = "detail.rptx",
            HeightMode = "fixed",
            RepeatPerRow = false
        };
        el.DataBinding.Source = "orders";
        el.DataBinding.ParamMap["customerId"] = "Id";

        Assert.Equal(10, el.X);
        Assert.Equal(50, el.Y);
        Assert.Equal(180, el.Width);
        Assert.Equal(100, el.Height);
        Assert.Equal("detail.rptx", el.TemplateRef);
        Assert.Equal("fixed", el.HeightMode);
        Assert.False(el.RepeatPerRow);
        Assert.Equal("orders", el.DataBinding.Source);
        Assert.Single(el.DataBinding.ParamMap);
    }
}

/// <summary>
/// BarcodeElement 完整属性测试
/// </summary>
public class BarcodeElementFullTests
{
    [Fact]
    public void BarcodeElement_Value_DefaultEmpty()
    {
        var el = new BarcodeElement();
        Assert.Equal("", el.Value);
    }

    [Fact]
    public void BarcodeElement_Value_Settable()
    {
        var el = new BarcodeElement { Value = "ABC123" };
        Assert.Equal("ABC123", el.Value);
    }

    [Fact]
    public void BarcodeElement_Format_DefaultQRCode()
    {
        var el = new BarcodeElement();
        Assert.Equal(BarcodeFormat.QRCode, el.Format);
    }

    [Fact]
    public void BarcodeElement_Format_Code128()
    {
        var el = new BarcodeElement { Format = BarcodeFormat.Code128 };
        Assert.Equal(BarcodeFormat.Code128, el.Format);
    }

    [Fact]
    public void BarcodeElement_Format_Code39()
    {
        var el = new BarcodeElement { Format = BarcodeFormat.Code39 };
        Assert.Equal(BarcodeFormat.Code39, el.Format);
    }

    [Fact]
    public void BarcodeElement_Format_DataMatrix()
    {
        var el = new BarcodeElement { Format = BarcodeFormat.DataMatrix };
        Assert.Equal(BarcodeFormat.DataMatrix, el.Format);
    }

    [Fact]
    public void BarcodeElement_Format_PDF417()
    {
        var el = new BarcodeElement { Format = BarcodeFormat.PDF417 };
        Assert.Equal(BarcodeFormat.PDF417, el.Format);
    }

    [Fact]
    public void BarcodeElement_ForeColor_Default000000()
    {
        var el = new BarcodeElement();
        Assert.Equal("#000000", el.ForeColor);
    }

    [Fact]
    public void BarcodeElement_ForeColor_Settable()
    {
        var el = new BarcodeElement { ForeColor = "#FF0000" };
        Assert.Equal("#FF0000", el.ForeColor);
    }

    [Fact]
    public void BarcodeElement_BackColor_DefaultFFFFFF()
    {
        var el = new BarcodeElement();
        Assert.Equal("#FFFFFF", el.BackColor);
    }

    [Fact]
    public void BarcodeElement_BackColor_Settable()
    {
        var el = new BarcodeElement { BackColor = "#0000FF" };
        Assert.Equal("#0000FF", el.BackColor);
    }

    [Fact]
    public void BarcodeElement_ShowText_DefaultTrue()
    {
        var el = new BarcodeElement();
        Assert.True(el.ShowText);
    }

    [Fact]
    public void BarcodeElement_ShowText_Settable()
    {
        var el = new BarcodeElement { ShowText = false };
        Assert.False(el.ShowText);
    }

    [Fact]
    public void BarcodeElement_FullSetup()
    {
        var el = new BarcodeElement
        {
            X = 10, Y = 10, Width = 100, Height = 100,
            Value = "https://example.com",
            Format = BarcodeFormat.QRCode,
            ForeColor = "#000000",
            BackColor = "#FFFFFF",
            ShowText = false
        };
        Assert.Equal(10, el.X);
        Assert.Equal(10, el.Y);
        Assert.Equal(100, el.Width);
        Assert.Equal(100, el.Height);
        Assert.Equal("https://example.com", el.Value);
        Assert.Equal(BarcodeFormat.QRCode, el.Format);
        Assert.Equal("#000000", el.ForeColor);
        Assert.Equal("#FFFFFF", el.BackColor);
        Assert.False(el.ShowText);
    }
}
