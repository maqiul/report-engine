using ReportEngine.Core.Barcodes;
using Xunit;

namespace ReportEngine.Core.Tests;

// ─────────────────────────────────────────────────────────────────────────────
// BarcodeGenerator 测试
// ─────────────────────────────────────────────────────────────────────────────

public class BarcodeGeneratorTests
{
    [Fact]
    public void Generate_Code128_ReturnsNonEmptyMatrix()
    {
        var matrix = BarcodeGenerator.Generate("ABC123", BarcodeFormat.Code128, 200, 50);
        Assert.NotNull(matrix);
        Assert.True(matrix.GetLength(0) > 0);
        Assert.True(matrix.GetLength(1) > 0);
    }

    [Fact]
    public void Generate_Code39_ReturnsNonEmptyMatrix()
    {
        var matrix = BarcodeGenerator.Generate("HELLO", BarcodeFormat.Code39, 200, 50);
        Assert.NotNull(matrix);
        Assert.True(matrix.GetLength(0) > 0);
    }

    [Fact]
    public void Generate_QRCode_ReturnsSquareMatrix()
    {
        var matrix = BarcodeGenerator.Generate("https://example.com", BarcodeFormat.QRCode, 100, 100);
        Assert.NotNull(matrix);
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        Assert.Equal(rows, cols); // QR 码是正方形
    }

    [Fact]
    public void Generate_DataMatrix_ReturnsNonEmptyMatrix()
    {
        var matrix = BarcodeGenerator.Generate("DATA123", BarcodeFormat.DataMatrix, 100, 100);
        Assert.NotNull(matrix);
        Assert.True(matrix.GetLength(0) > 0);
    }

    [Fact]
    public void Generate_PDF417_ReturnsNonEmptyMatrix()
    {
        var matrix = BarcodeGenerator.Generate("PDF417TEST", BarcodeFormat.PDF417, 200, 100);
        Assert.NotNull(matrix);
        Assert.True(matrix.GetLength(0) > 0);
    }

    [Fact]
    public void Generate_EmptyContent_ReturnsEmptyMatrix()
    {
        var matrix = BarcodeGenerator.Generate("", BarcodeFormat.QRCode, 100, 100);
        Assert.NotNull(matrix);
        // 空内容返回指定尺寸的空白矩阵
        Assert.Equal(100, matrix.GetLength(0));
        Assert.Equal(100, matrix.GetLength(1));
    }

    [Fact]
    public void Generate_NullContent_ReturnsEmptyMatrix()
    {
        var matrix = BarcodeGenerator.Generate(null!, BarcodeFormat.QRCode, 100, 100);
        Assert.NotNull(matrix);
        Assert.Equal(100, matrix.GetLength(0));
        Assert.Equal(100, matrix.GetLength(1));
    }

    [Fact]
    public void Generate_EAN13_ValidInput()
    {
        // EAN-13 需要 12 或 13 位数字
        var matrix = BarcodeGenerator.Generate("5901234123457", BarcodeFormat.EAN13, 200, 50);
        Assert.NotNull(matrix);
        Assert.True(matrix.GetLength(0) > 0);
    }

    [Fact]
    public void Generate_EAN8_ValidInput()
    {
        // EAN-8 需要 7 或 8 位数字
        var matrix = BarcodeGenerator.Generate("96385074", BarcodeFormat.EAN8, 150, 40);
        Assert.NotNull(matrix);
        Assert.True(matrix.GetLength(0) > 0);
    }

    [Fact]
    public void Generate_UPC_A_ValidInput()
    {
        // UPC-A 需要 11 或 12 位数字
        var matrix = BarcodeGenerator.Generate("012345678905", BarcodeFormat.UPC_A, 200, 50);
        Assert.NotNull(matrix);
        Assert.True(matrix.GetLength(0) > 0);
    }

    [Fact]
    public void Generate_MatrixContainsTrueValues()
    {
        var matrix = BarcodeGenerator.Generate("TEST", BarcodeFormat.Code128, 200, 50);
        bool hasTrue = false;
        for (int y = 0; y < matrix.GetLength(0) && !hasTrue; y++)
            for (int x = 0; x < matrix.GetLength(1) && !hasTrue; x++)
                if (matrix[y, x]) hasTrue = true;
        Assert.True(hasTrue); // 条码矩阵应该包含黑色模块
    }

    [Fact]
    public void Generate_DifferentSizes_ReturnsDifferentDimensions()
    {
        var small = BarcodeGenerator.Generate("TEST", BarcodeFormat.QRCode, 50, 50);
        var large = BarcodeGenerator.Generate("TEST", BarcodeFormat.QRCode, 200, 200);
        Assert.NotEqual(small.GetLength(0), large.GetLength(0));
    }
}
