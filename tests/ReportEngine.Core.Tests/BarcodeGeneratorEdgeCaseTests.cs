using ReportEngine.Core.Barcodes;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// BarcodeGenerator 高级边界测试：
///   - 不同尺寸
///   - 不同格式
///   - 空内容
///   - 特殊字符
///   - 矩阵维度验证
/// </summary>
public class BarcodeGeneratorEdgeCaseTests
{
    // ============== 空内容 ==============

    [Fact]
    public void Generate_EmptyContent_ReturnsEmptyMatrix()
    {
        var result = BarcodeGenerator.Generate("", BarcodeFormat.QRCode, 100, 100);
        Assert.Equal(100, result.GetLength(0));
        Assert.Equal(100, result.GetLength(1));
        // Empty content should produce all-false matrix
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

    // ============== 不同尺寸 ==============

    [Fact]
    public void Generate_SmallSize_ReturnsValidMatrix()
    {
        var result = BarcodeGenerator.Generate("A", BarcodeFormat.Code128, 30, 20);
        Assert.True(result.GetLength(0) > 0);
        Assert.True(result.GetLength(1) > 0);
    }

    [Fact]
    public void Generate_LargeSize_ReturnsValidMatrix()
    {
        var result = BarcodeGenerator.Generate("TEST123", BarcodeFormat.Code128, 300, 100);
        Assert.True(result.GetLength(0) > 0);
        Assert.True(result.GetLength(1) > 0);
    }

    [Fact]
    public void Generate_NonSquareSize_Works()
    {
        var result = BarcodeGenerator.Generate("HELLO", BarcodeFormat.Code128, 200, 50);
        Assert.True(result.GetLength(0) > 0);
        Assert.True(result.GetLength(1) > 0);
    }

    // ============== 矩阵非全空 ==============

    [Fact]
    public void Generate_ValidContent_HasBlackModules()
    {
        var result = BarcodeGenerator.Generate("TEST", BarcodeFormat.Code128, 100, 50);
        bool hasBlack = false;
        for (int y = 0; y < result.GetLength(0) && !hasBlack; y++)
            for (int x = 0; x < result.GetLength(1) && !hasBlack; x++)
                if (result[y, x]) hasBlack = true;
        Assert.True(hasBlack);
    }

    [Fact]
    public void Generate_QRCode_HasBlackModules()
    {
        var result = BarcodeGenerator.Generate("Hello World", BarcodeFormat.QRCode, 100, 100);
        bool hasBlack = false;
        for (int y = 0; y < result.GetLength(0) && !hasBlack; y++)
            for (int x = 0; x < result.GetLength(1) && !hasBlack; x++)
                if (result[y, x]) hasBlack = true;
        Assert.True(hasBlack);
    }

    // ============== 不同格式 ==============

    [Fact]
    public void Generate_Code39_ReturnsValidMatrix()
    {
        var result = BarcodeGenerator.Generate("ABC123", BarcodeFormat.Code39, 150, 50);
        Assert.True(result.GetLength(0) > 0);
        Assert.True(result.GetLength(1) > 0);
    }

    [Fact]
    public void Generate_EAN13_ReturnsValidMatrix()
    {
        // EAN13 requires 12 or 13 digits
        var result = BarcodeGenerator.Generate("590123456789", BarcodeFormat.EAN13, 100, 50);
        Assert.True(result.GetLength(0) > 0);
        Assert.True(result.GetLength(1) > 0);
    }

    [Fact]
    public void Generate_EAN8_ReturnsValidMatrix()
    {
        // EAN8 requires 7 or 8 digits
        var result = BarcodeGenerator.Generate("9638507", BarcodeFormat.EAN8, 80, 40);
        Assert.True(result.GetLength(0) > 0);
        Assert.True(result.GetLength(1) > 0);
    }

    [Fact]
    public void Generate_UPC_A_ReturnsValidMatrix()
    {
        // UPC_A requires 11 or 12 digits
        var result = BarcodeGenerator.Generate("01234567890", BarcodeFormat.UPC_A, 100, 50);
        Assert.True(result.GetLength(0) > 0);
        Assert.True(result.GetLength(1) > 0);
    }

    [Fact]
    public void Generate_DataMatrix_ReturnsValidMatrix()
    {
        var result = BarcodeGenerator.Generate("DataMatrix Test", BarcodeFormat.DataMatrix, 100, 100);
        Assert.True(result.GetLength(0) > 0);
        Assert.True(result.GetLength(1) > 0);
    }

    [Fact]
    public void Generate_PDF417_ReturnsValidMatrix()
    {
        var result = BarcodeGenerator.Generate("PDF417 Content", BarcodeFormat.PDF417, 200, 100);
        Assert.True(result.GetLength(0) > 0);
        Assert.True(result.GetLength(1) > 0);
    }

    // ============== 特殊字符 ==============

    [Fact]
    public void Generate_SpecialCharacters_Works()
    {
        var result = BarcodeGenerator.Generate("Hello! @#$%", BarcodeFormat.QRCode, 100, 100);
        Assert.True(result.GetLength(0) > 0);
        Assert.True(result.GetLength(1) > 0);
    }

    [Fact]
    public void Generate_ChineseCharacters_Works()
    {
        var result = BarcodeGenerator.Generate("你好世界", BarcodeFormat.QRCode, 100, 100);
        Assert.True(result.GetLength(0) > 0);
        Assert.True(result.GetLength(1) > 0);
    }

    [Fact]
    public void Generate_LongContent_Works()
    {
        var longContent = new string('A', 500);
        var result = BarcodeGenerator.Generate(longContent, BarcodeFormat.QRCode, 200, 200);
        Assert.True(result.GetLength(0) > 0);
        Assert.True(result.GetLength(1) > 0);
    }

    [Fact]
    public void Generate_NumericContent_Code128_Works()
    {
        var result = BarcodeGenerator.Generate("1234567890", BarcodeFormat.Code128, 150, 50);
        Assert.True(result.GetLength(0) > 0);
        Assert.True(result.GetLength(1) > 0);
    }

    // ============== 单字符 ==============

    [Fact]
    public void Generate_SingleCharacter_Works()
    {
        var result = BarcodeGenerator.Generate("A", BarcodeFormat.Code128, 50, 30);
        Assert.True(result.GetLength(0) > 0);
        Assert.True(result.GetLength(1) > 0);
    }

    [Fact]
    public void Generate_SingleDigit_Works()
    {
        var result = BarcodeGenerator.Generate("5", BarcodeFormat.Code128, 50, 30);
        Assert.True(result.GetLength(0) > 0);
        Assert.True(result.GetLength(1) > 0);
    }

    // ============== 不同内容产生不同矩阵 ==============

    [Fact]
    public void Generate_DifferentContent_ProducesDifferentMatrix()
    {
        var r1 = BarcodeGenerator.Generate("AAA", BarcodeFormat.Code128, 100, 50);
        var r2 = BarcodeGenerator.Generate("BBB", BarcodeFormat.Code128, 100, 50);

        // At least some pixels should differ
        bool differs = false;
        int minY = System.Math.Min(r1.GetLength(0), r2.GetLength(0));
        int minX = System.Math.Min(r1.GetLength(1), r2.GetLength(1));
        for (int y = 0; y < minY && !differs; y++)
            for (int x = 0; x < minX && !differs; x++)
                if (r1[y, x] != r2[y, x]) differs = true;

        Assert.True(differs);
    }
}
