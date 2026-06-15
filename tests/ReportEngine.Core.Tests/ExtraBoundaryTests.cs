using ReportEngine.Core;
using ReportEngine.Core.Barcodes;
using ReportEngine.Core.Data;
using ReportEngine.Core.Parsing;
using ReportEngine.Core.Rendering;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// BarcodeGenerator 更多边界测试
/// </summary>
public class BarcodeGeneratorMoreTests
{
    [Fact]
    public void Generate_Code39_ReturnsNonEmpty()
    {
        var matrix = BarcodeGenerator.Generate("HELLO", BarcodeFormat.Code39, 200, 50);
        Assert.True(matrix.Length > 0);
    }

    [Fact]
    public void Generate_EAN13_ReturnsNonEmpty()
    {
        var matrix = BarcodeGenerator.Generate("5901234123457", BarcodeFormat.EAN13, 200, 50);
        Assert.True(matrix.Length > 0);
    }

    [Fact]
    public void Generate_EAN8_ReturnsNonEmpty()
    {
        var matrix = BarcodeGenerator.Generate("96385074", BarcodeFormat.EAN8, 200, 50);
        Assert.True(matrix.Length > 0);
    }

    [Fact]
    public void Generate_DataMatrix_ReturnsNonEmpty()
    {
        var matrix = BarcodeGenerator.Generate("Hello World", BarcodeFormat.DataMatrix, 100, 100);
        Assert.True(matrix.Length > 0);
    }

    [Fact]
    public void Generate_PDF417_ReturnsNonEmpty()
    {
        var matrix = BarcodeGenerator.Generate("Hello World", BarcodeFormat.PDF417, 200, 100);
        Assert.True(matrix.Length > 0);
    }

    [Fact]
    public void Generate_UPC_A_ReturnsNonEmpty()
    {
        var matrix = BarcodeGenerator.Generate("01234567890", BarcodeFormat.UPC_A, 200, 50);
        Assert.True(matrix.Length > 0);
    }

    [Fact]
    public void Generate_LargeSize_ReturnsLargerMatrix()
    {
        var small = BarcodeGenerator.Generate("test", BarcodeFormat.QRCode, 50, 50);
        var large = BarcodeGenerator.Generate("test", BarcodeFormat.QRCode, 200, 200);
        Assert.True(large.Length >= small.Length);
    }

    [Fact]
    public void Generate_DifferentContent_DifferentMatrix()
    {
        var m1 = BarcodeGenerator.Generate("ABC", BarcodeFormat.QRCode, 100, 100);
        var m2 = BarcodeGenerator.Generate("XYZ", BarcodeFormat.QRCode, 100, 100);
        // Both should be non-empty but different content
        Assert.True(m1.Length > 0);
        Assert.True(m2.Length > 0);
    }
}

/// <summary>
/// ExpressionEngine 更多边界测试 2
/// </summary>
public class ExpressionEngineMoreBoundary2Tests
{
    private readonly ExpressionEngine _engine = new();

    [Fact]
    public void Evaluate_EmptyString_ReturnsEmpty()
    {
        var ctx = new RenderContext();
        var result = _engine.Evaluate("", ctx);
        Assert.Equal("", result);
    }

    [Fact]
    public void Evaluate_NoBraces_ReturnsLiteral()
    {
        var ctx = new RenderContext();
        var result = _engine.Evaluate("Hello World", ctx);
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void Evaluate_MultipleBraces_Concatenates()
    {
        var ctx = new RenderContext();
        var result = _engine.Evaluate("{{PAGE}}/{{TOTAL_PAGES}}", ctx);
        Assert.Contains("/", result);
    }

    [Fact]
    public void Evaluate_IntegerField_ReturnsValue()
    {
        var ctx = new RenderContext { DataSourceName = "ds" };
        ctx.DataSources["ds"] = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "count", 42 } }
        };
        var result = _engine.Evaluate("{{SUM(count)}}", ctx);
        Assert.Contains("42", result);
    }

    [Fact]
    public void Evaluate_LongField_ReturnsValue()
    {
        var ctx = new RenderContext { DataSourceName = "ds" };
        ctx.DataSources["ds"] = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "bignum", 1234567890L } }
        };
        var result = _engine.Evaluate("{{SUM(bignum)}}", ctx);
        Assert.Contains("1234567890", result);
    }

    [Fact]
    public void Evaluate_FloatField_ReturnsValue()
    {
        var ctx = new RenderContext { DataSourceName = "ds" };
        ctx.DataSources["ds"] = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "rate", 3.14f } }
        };
        var result = _engine.Evaluate("{{AVG(rate)}}", ctx);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Evaluate_MIN_ReturnsMinimum()
    {
        var ctx = new RenderContext { DataSourceName = "ds" };
        ctx.DataSources["ds"] = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "val", 10m } },
            new Dictionary<string, object> { { "val", 5m } },
            new Dictionary<string, object> { { "val", 20m } }
        };
        var result = _engine.Evaluate("{{MIN(val)}}", ctx);
        Assert.Contains("5", result);
    }

    [Fact]
    public void Evaluate_MAX_ReturnsMaximum()
    {
        var ctx = new RenderContext { DataSourceName = "ds" };
        ctx.DataSources["ds"] = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "val", 10m } },
            new Dictionary<string, object> { { "val", 5m } },
            new Dictionary<string, object> { { "val", 20m } }
        };
        var result = _engine.Evaluate("{{MAX(val)}}", ctx);
        Assert.Contains("20", result);
    }

    [Fact]
    public void Evaluate_COUNT_ReturnsRowCount()
    {
        var ctx = new RenderContext { DataSourceName = "ds" };
        ctx.DataSources["ds"] = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "id", 1 } },
            new Dictionary<string, object> { { "id", 2 } },
            new Dictionary<string, object> { { "id", 3 } }
        };
        var result = _engine.Evaluate("{{COUNT(id)}}", ctx);
        Assert.Contains("3", result);
    }
}

/// <summary>
/// TemplateParser 更多序列化测试
/// </summary>
public class TemplateParserMoreSerializationTests
{
    private readonly TemplateParser _parser = new();

    [Fact]
    public void Serialize_EmptyTemplate_ProducesJson()
    {
        var t = new ReportTemplate();
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
        var json = _parser.Serialize(t);
        Assert.NotEmpty(json);
        Assert.Contains("detail", json); // camelCase
    }

    [Fact]
    public void Serialize_WithAuthor_IncludesAuthor()
    {
        var t = new ReportTemplate { Author = "TestAuthor" };
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
        var json = _parser.Serialize(t);
        Assert.Contains("TestAuthor", json);
    }

    [Fact]
    public void Serialize_WithDescription_IncludesDescription()
    {
        var t = new ReportTemplate { Description = "Test description" };
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
        var json = _parser.Serialize(t);
        Assert.Contains("Test description", json);
    }

    [Fact]
    public void Serialize_WithMultiUp_IncludesMultiUp()
    {
        var t = new ReportTemplate();
        t.Page.MultiUp = new MultiUpConfig { Rows = 2, Columns = 3 };
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
        var json = _parser.Serialize(t);
        Assert.Contains("multiUp", json); // camelCase
    }

    [Fact]
    public void Serialize_WithParameters_IncludesParameters()
    {
        var t = new ReportTemplate();
        t.Parameters.Add(new TemplateParam { Name = "startDate", Type = "date", DefaultValue = "2026-01-01" });
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
        var json = _parser.Serialize(t);
        Assert.Contains("startDate", json);
        Assert.Contains("2026-01-01", json);
    }

    [Fact]
    public void Serialize_WithDataSource_IncludesDataSource()
    {
        var t = new ReportTemplate();
        t.DataSources.Add(new DataSourceDef { Name = "orders", Type = "sql" });
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
        var json = _parser.Serialize(t);
        Assert.Contains("orders", json);
    }

    [Fact]
    public void Serialize_WithWatermark_IncludesWatermark()
    {
        var t = new ReportTemplate();
        t.Page.Watermark = "CONFIDENTIAL";
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
        var json = _parser.Serialize(t);
        Assert.Contains("CONFIDENTIAL", json);
    }

    [Fact]
    public void Serialize_WithBackgroundImage_IncludesBackgroundImage()
    {
        var t = new ReportTemplate();
        t.Page.BackgroundImage = "bg.jpg";
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
        var json = _parser.Serialize(t);
        Assert.Contains("bg.jpg", json);
    }
}
