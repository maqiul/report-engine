using ReportEngine.Core;
using ReportEngine.Core.Barcodes;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// BarcodeGenerator 扩展行为测试
/// </summary>
public class BarcodeGeneratorExtendedTests
{
    // ============== 空内容 ==============

    [Fact]
    public void Generate_EmptyContent_ReturnsEmptyMatrix()
    {
        var result = BarcodeGenerator.Generate("", BarcodeFormat.QRCode, 100, 100);
        Assert.Equal(100, result.GetLength(0));
        Assert.Equal(100, result.GetLength(1));
        // 空内容应全 false
        for (int y = 0; y < result.GetLength(0); y++)
            for (int x = 0; x < result.GetLength(1); x++)
                Assert.False(result[y, x]);
    }

    [Fact]
    public void Generate_NullContent_ReturnsEmptyMatrix()
    {
        var result = BarcodeGenerator.Generate(null!, BarcodeFormat.QRCode, 50, 50);
        Assert.Equal(50, result.GetLength(0));
        Assert.Equal(50, result.GetLength(1));
    }

    // ============== QR Code ==============

    [Fact]
    public void Generate_QRCode_ProducesNonEmptyMatrix()
    {
        var result = BarcodeGenerator.Generate("Hello", BarcodeFormat.QRCode, 100, 100);
        Assert.True(result.GetLength(0) > 0);
        Assert.True(result.GetLength(1) > 0);
        // QR 码应有黑点
        bool hasBlack = false;
        for (int y = 0; y < result.GetLength(0) && !hasBlack; y++)
            for (int x = 0; x < result.GetLength(1) && !hasBlack; x++)
                if (result[y, x]) hasBlack = true;
        Assert.True(hasBlack);
    }

    // ============== Code128 ==============

    [Fact]
    public void Generate_Code128_ProducesNonEmptyMatrix()
    {
        var result = BarcodeGenerator.Generate("ABC123", BarcodeFormat.Code128, 200, 50);
        Assert.True(result.GetLength(0) > 0);
        Assert.True(result.GetLength(1) > 0);
    }

    // ============== Code39 ==============

    [Fact]
    public void Generate_Code39_ProducesNonEmptyMatrix()
    {
        var result = BarcodeGenerator.Generate("HELLO", BarcodeFormat.Code39, 200, 50);
        Assert.True(result.GetLength(0) > 0);
        Assert.True(result.GetLength(1) > 0);
    }

    // ============== EAN13 ==============

    [Fact]
    public void Generate_EAN13_ValidContent_Works()
    {
        var result = BarcodeGenerator.Generate("590123412345", BarcodeFormat.EAN13, 200, 80);
        Assert.True(result.GetLength(0) > 0);
        Assert.True(result.GetLength(1) > 0);
    }

    // ============== EAN8 ==============

    [Fact]
    public void Generate_EAN8_ValidContent_Works()
    {
        var result = BarcodeGenerator.Generate("9638507", BarcodeFormat.EAN8, 150, 60);
        Assert.True(result.GetLength(0) > 0);
        Assert.True(result.GetLength(1) > 0);
    }

    // ============== UPC_A ==============

    [Fact]
    public void Generate_UPC_A_ValidContent_Works()
    {
        var result = BarcodeGenerator.Generate("01234567890", BarcodeFormat.UPC_A, 200, 80);
        Assert.True(result.GetLength(0) > 0);
        Assert.True(result.GetLength(1) > 0);
    }

    // ============== DataMatrix ==============

    [Fact]
    public void Generate_DataMatrix_ProducesNonEmptyMatrix()
    {
        var result = BarcodeGenerator.Generate("Test123", BarcodeFormat.DataMatrix, 100, 100);
        Assert.True(result.GetLength(0) > 0);
        Assert.True(result.GetLength(1) > 0);
    }

    // ============== PDF417 ==============

    [Fact]
    public void Generate_PDF417_ProducesNonEmptyMatrix()
    {
        var result = BarcodeGenerator.Generate("PDF417 Test Content Long Enough", BarcodeFormat.PDF417, 300, 150);
        Assert.True(result.GetLength(0) > 0);
        Assert.True(result.GetLength(1) > 0);
    }

    // ============== 矩阵维度 ==============

    [Fact]
    public void Generate_MatrixDimensions_AreConsistent()
    {
        var result = BarcodeGenerator.Generate("Test", BarcodeFormat.QRCode, 80, 80);
        // 行数和列数都应 > 0
        Assert.True(result.GetLength(0) > 0);
        Assert.True(result.GetLength(1) > 0);
    }

    [Fact]
    public void Generate_SmallSize_StillWorks()
    {
        var result = BarcodeGenerator.Generate("A", BarcodeFormat.QRCode, 23, 23);
        Assert.True(result.GetLength(0) >= 23);
        Assert.True(result.GetLength(1) >= 23);
    }

    // ============== 不同内容生成不同矩阵 ==============

    [Fact]
    public void Generate_DifferentContent_ProducesDifferentMatrices()
    {
        var r1 = BarcodeGenerator.Generate("AAA", BarcodeFormat.QRCode, 100, 100);
        var r2 = BarcodeGenerator.Generate("BBB", BarcodeFormat.QRCode, 100, 100);

        // 至少有一个像素不同
        bool different = false;
        int h = Math.Min(r1.GetLength(0), r2.GetLength(0));
        int w = Math.Min(r1.GetLength(1), r2.GetLength(1));
        for (int y = 0; y < h && !different; y++)
            for (int x = 0; x < w && !different; x++)
                if (r1[y, x] != r2[y, x]) different = true;
        Assert.True(different);
    }
}
