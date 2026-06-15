using System;
using ReportEngine.Core.Barcodes;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// BarcodeGenerator 边界场景测试：
///   - 空字符串/null 输入
///   - 所有支持的格式（Code128/Code39/EAN13/EAN8/UPC_A/QRCode/DataMatrix/PDF417）
///   - 不同宽高
///   - 内容含特殊字符
/// </summary>
public class BarcodeGeneratorAdvancedTests
{
    [Fact]
    public void Generate_EmptyString_ReturnsEmptyMatrix()
    {
        var m = BarcodeGenerator.Generate("", BarcodeFormat.Code128, 100, 50);
        Assert.Equal(50, m.GetLength(0));
        Assert.Equal(100, m.GetLength(1));
        for (int y = 0; y < m.GetLength(0); y++)
            for (int x = 0; x < m.GetLength(1); x++)
                Assert.False(m[y, x]);
    }

    [Fact]
    public void Generate_NullString_ReturnsEmptyMatrix()
    {
        var m = BarcodeGenerator.Generate(null!, BarcodeFormat.Code128, 100, 50);
        Assert.Equal(50, m.GetLength(0));
        Assert.Equal(100, m.GetLength(1));
    }

    [Fact]
    public void Generate_AllSupportedFormats_DoNotThrow()
    {
        // 1D 格式都用 "12345"（Code128/Code39 接受任意字符）
        // EAN13 需要 12/13 位，EAN8 需要 7/8 位
        var oneDFormats = new[] { BarcodeFormat.Code128, BarcodeFormat.Code39 };
        foreach (var f in oneDFormats)
        {
            var m = BarcodeGenerator.Generate("12345", f, 100, 100);
            Assert.NotNull(m);
        }

        // EAN13/UPC_A 用 12 位数字
        var eanFormats = new[] { BarcodeFormat.EAN13, BarcodeFormat.UPC_A };
        foreach (var f in eanFormats)
        {
            var m = BarcodeGenerator.Generate("123456789012", f, 100, 100);
            Assert.NotNull(m);
        }

        // EAN8 用 7 位数字
        var m8 = BarcodeGenerator.Generate("1234567", BarcodeFormat.EAN8, 100, 100);
        Assert.NotNull(m8);

        // 2D 格式（PDF417 需要足够大尺寸）
        var twoDFormats = new[] { BarcodeFormat.QRCode, BarcodeFormat.DataMatrix };
        foreach (var f in twoDFormats)
        {
            var m = BarcodeGenerator.Generate("hello", f, 100, 100);
            Assert.NotNull(m);
        }
        // PDF417 单独用更大尺寸
        var pdf417 = BarcodeGenerator.Generate("hello", BarcodeFormat.PDF417, 300, 100);
        Assert.NotNull(pdf417);
    }

    [Fact]
    public void Generate_QRCode_ProducesSquareMatrix()
    {
        var m = BarcodeGenerator.Generate("hello world", BarcodeFormat.QRCode, 200, 200);
        Assert.Equal(200, m.GetLength(0));
        Assert.Equal(200, m.GetLength(1));
        // QR 矩阵至少有部分黑点
        bool anyBlack = false;
        for (int y = 0; y < m.GetLength(0); y++)
            for (int x = 0; x < m.GetLength(1); x++)
                if (m[y, x]) { anyBlack = true; break; }
        Assert.True(anyBlack, "QRCode matrix should have at least one black module");
    }

    [Fact]
    public void Generate_Code128_ProducesRectangle()
    {
        var m = BarcodeGenerator.Generate("ABC123", BarcodeFormat.Code128, 200, 50);
        Assert.Equal(50, m.GetLength(0));
        Assert.Equal(200, m.GetLength(1));
    }

    [Fact]
    public void Generate_UnsupportedFormat_DefaultsToQR()
    {
        // 默认值是 QRCode，不抛
        var m = BarcodeGenerator.Generate("test", (BarcodeFormat)999, 100, 100);
        Assert.NotNull(m);
    }

    [Fact]
    public void Generate_SpecialCharacters_DoNotThrow()
    {
        var m = BarcodeGenerator.Generate("a-b_c.d/e\\f", BarcodeFormat.Code128, 100, 30);
        Assert.NotNull(m);
    }

    [Fact]
    public void Generate_LongContent_StillProduces()
    {
        var content = new string('A', 100);
        var m = BarcodeGenerator.Generate(content, BarcodeFormat.Code128, 200, 30);
        Assert.Equal(30, m.GetLength(0));
    }

    [Fact]
    public void Generate_VerySmallSize_StillProduces()
    {
        // QR 码有最小尺寸（23×23 是 version 1 的大小）
        // Code128 在小尺寸下也按内容缩放
        var m = BarcodeGenerator.Generate("X", BarcodeFormat.QRCode, 10, 10);
        Assert.NotNull(m);
        Assert.True(m.GetLength(0) > 0);
        Assert.True(m.GetLength(1) > 0);
    }
}
