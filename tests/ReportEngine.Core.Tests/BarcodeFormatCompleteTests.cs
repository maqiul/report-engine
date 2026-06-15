using System;
using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// BarcodeFormat 枚举完整测试：
///   - BarcodeFormat 8 值
///   - 枚举值存在性验证
/// </summary>
public class BarcodeFormatCompleteTests
{
    [Fact]
    public void BarcodeFormat_Has8Values()
    {
        Assert.Equal(8, Enum.GetValues(typeof(BarcodeFormat)).Length);
    }

    [Fact]
    public void BarcodeFormat_HasCode128()
    {
        Assert.True(Enum.IsDefined(typeof(BarcodeFormat), BarcodeFormat.Code128));
    }

    [Fact]
    public void BarcodeFormat_HasCode39()
    {
        Assert.True(Enum.IsDefined(typeof(BarcodeFormat), BarcodeFormat.Code39));
    }

    [Fact]
    public void BarcodeFormat_HasEAN13()
    {
        Assert.True(Enum.IsDefined(typeof(BarcodeFormat), BarcodeFormat.EAN13));
    }

    [Fact]
    public void BarcodeFormat_HasEAN8()
    {
        Assert.True(Enum.IsDefined(typeof(BarcodeFormat), BarcodeFormat.EAN8));
    }

    [Fact]
    public void BarcodeFormat_HasUPC_A()
    {
        Assert.True(Enum.IsDefined(typeof(BarcodeFormat), BarcodeFormat.UPC_A));
    }

    [Fact]
    public void BarcodeFormat_HasQRCode()
    {
        Assert.True(Enum.IsDefined(typeof(BarcodeFormat), BarcodeFormat.QRCode));
    }

    [Fact]
    public void BarcodeFormat_HasDataMatrix()
    {
        Assert.True(Enum.IsDefined(typeof(BarcodeFormat), BarcodeFormat.DataMatrix));
    }

    [Fact]
    public void BarcodeFormat_HasPDF417()
    {
        Assert.True(Enum.IsDefined(typeof(BarcodeFormat), BarcodeFormat.PDF417));
    }

    [Fact]
    public void BarcodeFormat_DefaultIsCode128()
    {
        Assert.Equal(BarcodeFormat.Code128, default(BarcodeFormat));
    }

    [Fact]
    public void BarcodeFormat_Code128_IsFirst()
    {
        Assert.Equal(0, (int)BarcodeFormat.Code128);
    }

    [Fact]
    public void BarcodeFormat_Code39_IsSecond()
    {
        Assert.Equal(1, (int)BarcodeFormat.Code39);
    }

    [Fact]
    public void BarcodeFormat_EAN13_IsThird()
    {
        Assert.Equal(2, (int)BarcodeFormat.EAN13);
    }

    [Fact]
    public void BarcodeFormat_EAN8_IsFourth()
    {
        Assert.Equal(3, (int)BarcodeFormat.EAN8);
    }

    [Fact]
    public void BarcodeFormat_UPC_A_IsFifth()
    {
        Assert.Equal(4, (int)BarcodeFormat.UPC_A);
    }

    [Fact]
    public void BarcodeFormat_QRCode_IsSixth()
    {
        Assert.Equal(5, (int)BarcodeFormat.QRCode);
    }

    [Fact]
    public void BarcodeFormat_DataMatrix_IsSeventh()
    {
        Assert.Equal(6, (int)BarcodeFormat.DataMatrix);
    }

    [Fact]
    public void BarcodeFormat_PDF417_IsEighth()
    {
        Assert.Equal(7, (int)BarcodeFormat.PDF417);
    }
}
