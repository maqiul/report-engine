using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// BarcodeElement 行为测试：
///   - 默认值
///   - 条码内容
///   - 条码格式
///   - 背景色
///   - 显示文字
/// </summary>
public class BarcodeElementBehaviorTests
{
    // ============== 默认值 ==============

    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var el = new BarcodeElement();

        Assert.Equal("", el.Value);
        Assert.Equal(BarcodeFormat.QRCode, el.Format);
        Assert.Equal("#FFFFFF", el.BackColor);
        Assert.True(el.ShowText);
    }

    // ============== Value ==============

    [Fact]
    public void Value_EmptyByDefault()
    {
        var el = new BarcodeElement();
        Assert.Equal("", el.Value);
    }

    [Fact]
    public void Value_SetSimple_Works()
    {
        var el = new BarcodeElement { Value = "123456789" };
        Assert.Equal("123456789", el.Value);
    }

    [Fact]
    public void Value_SetAlphanumeric_Works()
    {
        var el = new BarcodeElement { Value = "ABC-123-XYZ" };
        Assert.Equal("ABC-123-XYZ", el.Value);
    }

    [Fact]
    public void Value_SetExpression_Works()
    {
        var el = new BarcodeElement { Value = "{{currentRow.orderNo}}" };
        Assert.Contains("currentRow", el.Value);
    }

    [Fact]
    public void Value_SetUrl_Works()
    {
        var el = new BarcodeElement { Value = "https://example.com/product/123" };
        Assert.StartsWith("https://", el.Value);
    }

    [Fact]
    public void Value_CanBeChanged()
    {
        var el = new BarcodeElement { Value = "old" };
        el.Value = "new";
        Assert.Equal("new", el.Value);
    }

    // ============== Format ==============

    [Fact]
    public void Format_DefaultIsQRCode()
    {
        var el = new BarcodeElement();
        Assert.Equal(BarcodeFormat.QRCode, el.Format);
    }

    [Fact]
    public void Format_SetCode128_Works()
    {
        var el = new BarcodeElement { Format = BarcodeFormat.Code128 };
        Assert.Equal(BarcodeFormat.Code128, el.Format);
    }

    [Fact]
    public void Format_SetCode39_Works()
    {
        var el = new BarcodeElement { Format = BarcodeFormat.Code39 };
        Assert.Equal(BarcodeFormat.Code39, el.Format);
    }

    [Fact]
    public void Format_SetEAN13_Works()
    {
        var el = new BarcodeElement { Format = BarcodeFormat.EAN13 };
        Assert.Equal(BarcodeFormat.EAN13, el.Format);
    }

    [Fact]
    public void Format_SetEAN8_Works()
    {
        var el = new BarcodeElement { Format = BarcodeFormat.EAN8 };
        Assert.Equal(BarcodeFormat.EAN8, el.Format);
    }

    [Fact]
    public void Format_SetUPC_A_Works()
    {
        var el = new BarcodeElement { Format = BarcodeFormat.UPC_A };
        Assert.Equal(BarcodeFormat.UPC_A, el.Format);
    }

    [Fact]
    public void Format_SetDataMatrix_Works()
    {
        var el = new BarcodeElement { Format = BarcodeFormat.DataMatrix };
        Assert.Equal(BarcodeFormat.DataMatrix, el.Format);
    }

    [Fact]
    public void Format_SetPDF417_Works()
    {
        var el = new BarcodeElement { Format = BarcodeFormat.PDF417 };
        Assert.Equal(BarcodeFormat.PDF417, el.Format);
    }

    [Fact]
    public void Format_CanBeChanged()
    {
        var el = new BarcodeElement { Format = BarcodeFormat.QRCode };
        el.Format = BarcodeFormat.Code128;
        Assert.Equal(BarcodeFormat.Code128, el.Format);
    }

    // ============== BackColor ==============

    [Fact]
    public void BackColor_DefaultIsWhite()
    {
        var el = new BarcodeElement();
        Assert.Equal("#FFFFFF", el.BackColor);
    }

    [Fact]
    public void BackColor_SetLightGray_Works()
    {
        var el = new BarcodeElement { BackColor = "#F0F0F0" };
        Assert.Equal("#F0F0F0", el.BackColor);
    }

    [Fact]
    public void BackColor_SetYellow_Works()
    {
        var el = new BarcodeElement { BackColor = "#FFFFCC" };
        Assert.Equal("#FFFFCC", el.BackColor);
    }

    [Fact]
    public void BackColor_CanBeChanged()
    {
        var el = new BarcodeElement { BackColor = "#FFFFFF" };
        el.BackColor = "#EEEEEE";
        Assert.Equal("#EEEEEE", el.BackColor);
    }

    // ============== ShowText ==============

    [Fact]
    public void ShowText_TrueByDefault()
    {
        var el = new BarcodeElement();
        Assert.True(el.ShowText);
    }

    [Fact]
    public void ShowText_SetFalse_Works()
    {
        var el = new BarcodeElement { ShowText = false };
        Assert.False(el.ShowText);
    }

    [Fact]
    public void ShowText_CanBeToggled()
    {
        var el = new BarcodeElement { ShowText = true };
        el.ShowText = false;
        Assert.False(el.ShowText);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void BarcodeElement_QRCode_Works()
    {
        var el = new BarcodeElement
        {
            Value = "https://example.com",
            Format = BarcodeFormat.QRCode,
            BackColor = "#FFFFFF",
            ShowText = false,
            X = 10,
            Y = 10,
            Width = 30,
            Height = 30
        };

        Assert.Equal(BarcodeFormat.QRCode, el.Format);
        Assert.False(el.ShowText);
    }

    [Fact]
    public void BarcodeElement_Code128_Works()
    {
        var el = new BarcodeElement
        {
            Value = "ABC-12345",
            Format = BarcodeFormat.Code128,
            ShowText = true,
            X = 10,
            Y = 10,
            Width = 60,
            Height = 20
        };

        Assert.Equal(BarcodeFormat.Code128, el.Format);
        Assert.True(el.ShowText);
    }

    [Fact]
    public void BarcodeElement_EAN13_Works()
    {
        var el = new BarcodeElement
        {
            Value = "5901234123457",
            Format = BarcodeFormat.EAN13,
            ShowText = true
        };

        Assert.Equal(BarcodeFormat.EAN13, el.Format);
        Assert.Equal(13, el.Value.Length);
    }

    [Fact]
    public void BarcodeElement_DataMatrix_Works()
    {
        var el = new BarcodeElement
        {
            Value = "Serial: SN123456",
            Format = BarcodeFormat.DataMatrix,
            ShowText = false
        };

        Assert.Equal(BarcodeFormat.DataMatrix, el.Format);
    }

    [Fact]
    public void BarcodeElement_WithExpression_Works()
    {
        var el = new BarcodeElement
        {
            Value = "{{currentRow.orderNo}}",
            Format = BarcodeFormat.Code128
        };

        Assert.Contains("{{", el.Value);
    }

    [Fact]
    public void BarcodeElement_InBand_Works()
    {
        var band = new Band { Type = BandType.Detail, Height = 40 };
        band.Elements.Add(new BarcodeElement
        {
            Value = "12345",
            Format = BarcodeFormat.Code128,
            X = 10,
            Y = 5,
            Width = 50,
            Height = 30
        });

        Assert.Single(band.Elements);
        var barcode = Assert.IsType<BarcodeElement>(band.Elements[0]);
        Assert.Equal("12345", barcode.Value);
    }

    [Fact]
    public void BarcodeElement_CanBeModified()
    {
        var el = new BarcodeElement
        {
            Value = "old",
            Format = BarcodeFormat.QRCode,
            ShowText = true
        };
        
        el.Value = "new";
        el.Format = BarcodeFormat.Code128;
        el.ShowText = false;
        
        Assert.Equal("new", el.Value);
        Assert.Equal(BarcodeFormat.Code128, el.Format);
        Assert.False(el.ShowText);
    }
}
