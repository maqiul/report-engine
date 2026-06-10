using FluentAssertions;
using ReportEngine.Core;
using ReportEngine.Core.Barcodes;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// BarcodeGenerator 行为测试：
///   - 空内容返回尺寸正确的空矩阵
///   - QRCode 生成非空矩阵且维度符合请求
///   - Code128 同样可生成
///   - DataMatrix / EAN13 等其他格式不抛异常
///   - 矩阵中存在 true / false 两种值（说明真有编码发生）
/// </summary>
public class BarcodeGeneratorTests
{
    [Fact]
    public void Generate_EmptyContent_Returns_EmptyMatrix_With_Requested_Size()
    {
        var matrix = BarcodeGenerator.Generate("", BarcodeFormat.QRCode, 100, 50);

        matrix.GetLength(0).Should().Be(50);
        matrix.GetLength(1).Should().Be(100);
        // 空矩阵所有点都为 false
        matrix.Cast<bool>().Should().AllBeEquivalentTo(false);
    }

    [Fact]
    public void Generate_QRCode_Returns_NonEmpty_Matrix_With_True_And_False_Pixels()
    {
        var matrix = BarcodeGenerator.Generate("https://example.com", BarcodeFormat.QRCode, 200, 200);

        matrix.GetLength(0).Should().Be(200);
        matrix.GetLength(1).Should().Be(200);

        // QR 编码一定既有黑点也有白点
        matrix.Cast<bool>().Should().Contain(true);
        matrix.Cast<bool>().Should().Contain(false);
    }

    [Fact]
    public void Generate_Code128_Returns_Matrix_With_Expected_Width()
    {
        var matrix = BarcodeGenerator.Generate("ABC-12345", BarcodeFormat.Code128, 200, 80);

        matrix.GetLength(0).Should().Be(80);
        matrix.GetLength(1).Should().Be(200);
        matrix.Cast<bool>().Should().Contain(true);
    }

    [Theory]
    [InlineData(BarcodeFormat.EAN13, "590123412345")]   // 13 位 EAN-13
    [InlineData(BarcodeFormat.EAN8, "9638507")]          // 7 位 EAN-8
    [InlineData(BarcodeFormat.UPC_A, "123456789012")]    // 12 位 UPC-A
    [InlineData(BarcodeFormat.Code39, "ABC-123")]
    [InlineData(BarcodeFormat.DataMatrix, "test")]
    [InlineData(BarcodeFormat.PDF417, "hi", 300, 100)]   // PDF417 需要更大画布才能放下
    public void Generate_Various_Formats_Do_Not_Throw(BarcodeFormat format, string content, int width = 100, int height = 100)
    {
        var act = () => BarcodeGenerator.Generate(content, format, width, height);

        act.Should().NotThrow();
    }
}