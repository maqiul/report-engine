using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// BarcodeElement 完整字段测试：
///   - BarcodeElement 完整字段（Value/Format/ForeColor/BackColor/ShowText）
///   - 字段组合行为
/// </summary>
public class BarcodeElementCompleteTests
{
    [Fact]
    public void BarcodeElement_Defaults()
    {
        var b = new BarcodeElement();
        Assert.Equal("", b.Value);
        Assert.Equal(BarcodeFormat.QRCode, b.Format);
        Assert.Equal("#000000", b.ForeColor);
        Assert.Equal("#FFFFFF", b.BackColor);
        Assert.True(b.ShowText);
    }

    [Fact]
    public void BarcodeElement_AllSetters()
    {
        var b = new BarcodeElement
        {
            Value = "{{orderNo}}",
            Format = BarcodeFormat.Code128,
            ForeColor = "#FF0000",
            BackColor = "#FFFF00",
            ShowText = false,
        };
        Assert.Equal("{{orderNo}}", b.Value);
        Assert.Equal(BarcodeFormat.Code128, b.Format);
        Assert.Equal("#FF0000", b.ForeColor);
        Assert.Equal("#FFFF00", b.BackColor);
        Assert.False(b.ShowText);
    }

    [Fact]
    public void BarcodeElement_Value_CanBeEmpty()
    {
        var b = new BarcodeElement { Value = "" };
        Assert.Equal("", b.Value);
    }

    [Fact]
    public void BarcodeElement_Value_CanBeExpression()
    {
        var b = new BarcodeElement { Value = "{{currentRow.id}}" };
        Assert.Equal("{{currentRow.id}}", b.Value);
    }

    [Fact]
    public void BarcodeElement_Value_CanBeStatic()
    {
        var b = new BarcodeElement { Value = "ABC123" };
        Assert.Equal("ABC123", b.Value);
    }

    [Fact]
    public void BarcodeElement_Format_DefaultQRCode()
    {
        var b = new BarcodeElement();
        Assert.Equal(BarcodeFormat.QRCode, b.Format);
    }

    [Fact]
    public void BarcodeElement_Format_CanBeCode128()
    {
        var b = new BarcodeElement { Format = BarcodeFormat.Code128 };
        Assert.Equal(BarcodeFormat.Code128, b.Format);
    }

    [Fact]
    public void BarcodeElement_Format_CanBeCode39()
    {
        var b = new BarcodeElement { Format = BarcodeFormat.Code39 };
        Assert.Equal(BarcodeFormat.Code39, b.Format);
    }

    [Fact]
    public void BarcodeElement_Format_CanBeEAN13()
    {
        var b = new BarcodeElement { Format = BarcodeFormat.EAN13 };
        Assert.Equal(BarcodeFormat.EAN13, b.Format);
    }

    [Fact]
    public void BarcodeElement_Format_CanBeEAN8()
    {
        var b = new BarcodeElement { Format = BarcodeFormat.EAN8 };
        Assert.Equal(BarcodeFormat.EAN8, b.Format);
    }

    [Fact]
    public void BarcodeElement_Format_CanBeUPC_A()
    {
        var b = new BarcodeElement { Format = BarcodeFormat.UPC_A };
        Assert.Equal(BarcodeFormat.UPC_A, b.Format);
    }

    [Fact]
    public void BarcodeElement_Format_CanBeDataMatrix()
    {
        var b = new BarcodeElement { Format = BarcodeFormat.DataMatrix };
        Assert.Equal(BarcodeFormat.DataMatrix, b.Format);
    }

    [Fact]
    public void BarcodeElement_Format_CanBePDF417()
    {
        var b = new BarcodeElement { Format = BarcodeFormat.PDF417 };
        Assert.Equal(BarcodeFormat.PDF417, b.Format);
    }

    [Fact]
    public void BarcodeElement_ForeColor_DefaultBlack()
    {
        var b = new BarcodeElement();
        Assert.Equal("#000000", b.ForeColor);
    }

    [Fact]
    public void BarcodeElement_ForeColor_CanBeRed()
    {
        var b = new BarcodeElement { ForeColor = "#FF0000" };
        Assert.Equal("#FF0000", b.ForeColor);
    }

    [Fact]
    public void BarcodeElement_ForeColor_CanBeHex8()
    {
        var b = new BarcodeElement { ForeColor = "#FF000000" };
        Assert.Equal("#FF000000", b.ForeColor);
    }

    [Fact]
    public void BarcodeElement_BackColor_DefaultWhite()
    {
        var b = new BarcodeElement();
        Assert.Equal("#FFFFFF", b.BackColor);
    }

    [Fact]
    public void BarcodeElement_BackColor_CanBeBlack()
    {
        var b = new BarcodeElement { BackColor = "#000000" };
        Assert.Equal("#000000", b.BackColor);
    }

    [Fact]
    public void BarcodeElement_BackColor_CanBeHex8()
    {
        var b = new BarcodeElement { BackColor = "#80FFFFFF" };
        Assert.Equal("#80FFFFFF", b.BackColor);
    }

    [Fact]
    public void BarcodeElement_ShowText_DefaultTrue()
    {
        var b = new BarcodeElement();
        Assert.True(b.ShowText);
    }

    [Fact]
    public void BarcodeElement_ShowText_CanBeFalse()
    {
        var b = new BarcodeElement { ShowText = false };
        Assert.False(b.ShowText);
    }

    [Fact]
    public void BarcodeElement_FullCombination()
    {
        var b = new BarcodeElement
        {
            Value = "123456789012",
            Format = BarcodeFormat.EAN13,
            ForeColor = "#333333",
            BackColor = "#EEEEEE",
            ShowText = true,
        };
        Assert.Equal("123456789012", b.Value);
        Assert.Equal(BarcodeFormat.EAN13, b.Format);
        Assert.Equal("#333333", b.ForeColor);
        Assert.Equal("#EEEEEE", b.BackColor);
        Assert.True(b.ShowText);
    }
}
