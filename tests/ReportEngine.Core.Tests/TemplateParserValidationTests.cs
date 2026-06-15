using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Parsing;
using ReportEngine.Core.Rendering;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// TemplateParser ValidateTemplate 错误测试
/// </summary>
public class TemplateParserValidationTests
{
    private readonly TemplateParser _parser = new();

    [Fact]
    public void Parse_NoBands_ThrowsTemplateParseException()
    {
        var json = @"{ ""version"": ""1.0"" }";
        var ex = Assert.Throws<TemplateParseException>(() => _parser.Parse(json));
        Assert.Contains("at least one band", ex.Message);
    }

    [Fact]
    public void Parse_UnknownDataSource_ThrowsTemplateParseException()
    {
        var json = @"{
            ""version"": ""1.0"",
            ""bands"": [{ ""type"": ""detail"", ""height"": 10, ""dataSource"": ""unknown"" }]
        }";
        var ex = Assert.Throws<TemplateParseException>(() => _parser.Parse(json));
        Assert.Contains("unknown datasource", ex.Message);
    }

    [Fact]
    public void Parse_SubReportEmptyTemplateRef_ThrowsTemplateParseException()
    {
        var json = @"{
            ""version"": ""1.0"",
            ""bands"": [{
                ""type"": ""detail"",
                ""height"": 10,
                ""elements"": [{ ""type"": ""subreport"", ""templateRef"": """" }]
            }]
        }";
        var ex = Assert.Throws<TemplateParseException>(() => _parser.Parse(json));
        Assert.Contains("empty TemplateRef", ex.Message);
    }

    [Fact]
    public void Parse_SubReportUnknownDataSource_ThrowsTemplateParseException()
    {
        var json = @"{
            ""version"": ""1.0"",
            ""bands"": [{
                ""type"": ""detail"",
                ""height"": 10,
                ""elements"": [{
                    ""type"": ""subreport"",
                    ""templateRef"": ""sub.rpt"",
                    ""dataBinding"": { ""source"": ""unknownDs"" }
                }]
            }]
        }";
        var ex = Assert.Throws<TemplateParseException>(() => _parser.Parse(json));
        Assert.Contains("unknown datasource", ex.Message);
    }

    [Fact]
    public void Parse_ValidSubReport_NoException()
    {
        var json = @"{
            ""version"": ""1.0"",
            ""dataSources"": [{ ""name"": ""mainDs"" }],
            ""bands"": [{
                ""type"": ""detail"",
                ""height"": 10,
                ""dataSource"": ""mainDs"",
                ""elements"": [{
                    ""type"": ""subreport"",
                    ""templateRef"": ""sub.rpt"",
                    ""dataBinding"": { ""source"": ""mainDs"" }
                }]
            }]
        }";
        var template = _parser.Parse(json);
        Assert.NotNull(template);
    }
}

/// <summary>
/// ReportElementConverter 元素类型测试
/// </summary>
public class ReportElementConverterTests
{
    private readonly TemplateParser _parser = new();

    [Fact]
    public void Parse_TextElement_HasCorrectType()
    {
        var json = @"{
            ""version"": ""1.0"",
            ""bands"": [{
                ""type"": ""detail"",
                ""height"": 10,
                ""elements"": [{ ""type"": ""text"", ""text"": ""Hello"" }]
            }]
        }";
        var template = _parser.Parse(json);
        var el = Assert.IsType<TextElement>(template.Bands[0].Elements[0]);
        Assert.Equal("Hello", el.Text);
    }

    [Fact]
    public void Parse_ImageElement_HasCorrectType()
    {
        var json = @"{
            ""version"": ""1.0"",
            ""bands"": [{
                ""type"": ""detail"",
                ""height"": 10,
                ""elements"": [{ ""type"": ""image"", ""source"": ""logo.png"" }]
            }]
        }";
        var template = _parser.Parse(json);
        var el = Assert.IsType<ImageElement>(template.Bands[0].Elements[0]);
        Assert.Equal("logo.png", el.Source);
    }

    [Fact]
    public void Parse_LineElement_HasCorrectType()
    {
        var json = @"{
            ""version"": ""1.0"",
            ""bands"": [{
                ""type"": ""detail"",
                ""height"": 10,
                ""elements"": [{ ""type"": ""line"", ""lineWidth"": 2 }]
            }]
        }";
        var template = _parser.Parse(json);
        var el = Assert.IsType<LineElement>(template.Bands[0].Elements[0]);
        Assert.Equal(2, el.LineWidth);
    }

    [Fact]
    public void Parse_ShapeElement_HasCorrectType()
    {
        var json = @"{
            ""version"": ""1.0"",
            ""bands"": [{
                ""type"": ""detail"",
                ""height"": 10,
                ""elements"": [{ ""type"": ""shape"", ""shape"": ""ellipse"" }]
            }]
        }";
        var template = _parser.Parse(json);
        var el = Assert.IsType<ShapeElement>(template.Bands[0].Elements[0]);
        Assert.Equal(ShapeType.Ellipse, el.Shape);
    }

    [Fact]
    public void Parse_ChartElement_HasCorrectType()
    {
        var json = @"{
            ""version"": ""1.0"",
            ""bands"": [{
                ""type"": ""detail"",
                ""height"": 10,
                ""elements"": [{ ""type"": ""chart"", ""chartType"": ""pie"" }]
            }]
        }";
        var template = _parser.Parse(json);
        var el = Assert.IsType<ChartElement>(template.Bands[0].Elements[0]);
        Assert.Equal(ChartType.Pie, el.ChartType);
    }

    [Fact]
    public void Parse_BarcodeElement_HasCorrectType()
    {
        var json = @"{
            ""version"": ""1.0"",
            ""bands"": [{
                ""type"": ""detail"",
                ""height"": 10,
                ""elements"": [{ ""type"": ""barcode"", ""value"": ""ABC123"", ""format"": ""code128"" }]
            }]
        }";
        var template = _parser.Parse(json);
        var el = Assert.IsType<BarcodeElement>(template.Bands[0].Elements[0]);
        Assert.Equal("ABC123", el.Value);
        Assert.Equal(BarcodeFormat.Code128, el.Format);
    }

    [Fact]
    public void Parse_TableElement_HasCorrectType()
    {
        var json = @"{
            ""version"": ""1.0"",
            ""bands"": [{
                ""type"": ""detail"",
                ""height"": 10,
                ""elements"": [{ ""type"": ""table"", ""rowCount"": 5, ""colCount"": 4 }]
            }]
        }";
        var template = _parser.Parse(json);
        var el = Assert.IsType<TableElement>(template.Bands[0].Elements[0]);
        Assert.Equal(5, el.RowCount);
        Assert.Equal(4, el.ColCount);
    }

    [Fact]
    public void Parse_CrossTabElement_HasCorrectType()
    {
        var json = @"{
            ""version"": ""1.0"",
            ""bands"": [{
                ""type"": ""detail"",
                ""height"": 10,
                ""elements"": [{ ""type"": ""crosstab"", ""dataSource"": ""ds"" }]
            }]
        }";
        var template = _parser.Parse(json);
        var el = Assert.IsType<CrossTabElement>(template.Bands[0].Elements[0]);
        Assert.Equal("ds", el.DataSource);
    }

    [Fact]
    public void Parse_UnknownElementType_ThrowsTemplateParseException()
    {
        var json = @"{
            ""version"": ""1.0"",
            ""bands"": [{
                ""type"": ""detail"",
                ""height"": 10,
                ""elements"": [{ ""type"": ""unknown"" }]
            }]
        }";
        var ex = Assert.Throws<TemplateParseException>(() => _parser.Parse(json));
        Assert.Contains("Unknown element type", ex.Message);
    }

    [Fact]
    public void Parse_MissingTypeProperty_ThrowsTemplateParseException()
    {
        var json = @"{
            ""version"": ""1.0"",
            ""bands"": [{
                ""type"": ""detail"",
                ""height"": 10,
                ""elements"": [{ ""text"": ""Hello"" }]
            }]
        }";
        var ex = Assert.Throws<TemplateParseException>(() => _parser.Parse(json));
        Assert.Contains("type", ex.Message);
    }
}

/// <summary>
/// TemplateParser 序列化往返测试
/// </summary>
public class TemplateParserRoundTripMoreTests
{
    private readonly TemplateParser _parser = new();

    [Fact]
    public void RoundTrip_TextElement_PreservesText()
    {
        var t = new ReportTemplate();
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
        t.Bands[0].Elements.Add(new TextElement { Text = "Hello World" });
        
        var json = _parser.Serialize(t);
        var t2 = _parser.Parse(json);
        
        var el = Assert.IsType<TextElement>(t2.Bands[0].Elements[0]);
        Assert.Equal("Hello World", el.Text);
    }

    [Fact]
    public void RoundTrip_ImageElement_PreservesSource()
    {
        var t = new ReportTemplate();
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
        t.Bands[0].Elements.Add(new ImageElement { Source = "logo.png" });
        
        var json = _parser.Serialize(t);
        var t2 = _parser.Parse(json);
        
        var el = Assert.IsType<ImageElement>(t2.Bands[0].Elements[0]);
        Assert.Equal("logo.png", el.Source);
    }

    [Fact]
    public void RoundTrip_LineElement_PreservesLineWidth()
    {
        var t = new ReportTemplate();
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
        t.Bands[0].Elements.Add(new LineElement { LineWidth = 2.5, LineColor = "#FF0000" });
        
        var json = _parser.Serialize(t);
        var t2 = _parser.Parse(json);
        
        var el = Assert.IsType<LineElement>(t2.Bands[0].Elements[0]);
        Assert.Equal(2.5, el.LineWidth);
        Assert.Equal("#FF0000", el.LineColor);
    }

    [Fact]
    public void RoundTrip_ShapeElement_PreservesShape()
    {
        var t = new ReportTemplate();
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
        t.Bands[0].Elements.Add(new ShapeElement { Shape = ShapeType.Ellipse, FillColor = "#00FF00" });
        
        var json = _parser.Serialize(t);
        var t2 = _parser.Parse(json);
        
        var el = Assert.IsType<ShapeElement>(t2.Bands[0].Elements[0]);
        Assert.Equal(ShapeType.Ellipse, el.Shape);
        Assert.Equal("#00FF00", el.FillColor);
    }

    [Fact]
    public void RoundTrip_ChartElement_PreservesChartType()
    {
        var t = new ReportTemplate();
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
        t.Bands[0].Elements.Add(new ChartElement { ChartType = ChartType.Pie, Title = "Sales" });
        
        var json = _parser.Serialize(t);
        var t2 = _parser.Parse(json);
        
        var el = Assert.IsType<ChartElement>(t2.Bands[0].Elements[0]);
        Assert.Equal(ChartType.Pie, el.ChartType);
        Assert.Equal("Sales", el.Title);
    }

    [Fact]
    public void RoundTrip_BarcodeElement_PreservesFormat()
    {
        var t = new ReportTemplate();
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
        t.Bands[0].Elements.Add(new BarcodeElement { Value = "ABC123", Format = BarcodeFormat.Code128 });
        
        var json = _parser.Serialize(t);
        var t2 = _parser.Parse(json);
        
        var el = Assert.IsType<BarcodeElement>(t2.Bands[0].Elements[0]);
        Assert.Equal("ABC123", el.Value);
        Assert.Equal(BarcodeFormat.Code128, el.Format);
    }

    [Fact]
    public void RoundTrip_TableElement_PreservesDimensions()
    {
        var t = new ReportTemplate();
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
        t.Bands[0].Elements.Add(new TableElement { RowCount = 5, ColCount = 4, BorderWidth = 0.5 });
        
        var json = _parser.Serialize(t);
        var t2 = _parser.Parse(json);
        
        var el = Assert.IsType<TableElement>(t2.Bands[0].Elements[0]);
        Assert.Equal(5, el.RowCount);
        Assert.Equal(4, el.ColCount);
        Assert.Equal(0.5, el.BorderWidth);
    }

    [Fact]
    public void RoundTrip_CrossTabElement_PreservesDataSource()
    {
        var t = new ReportTemplate();
        t.DataSources.Add(new DataSourceDef { Name = "ds" });
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
        t.Bands[0].Elements.Add(new CrossTabElement { DataSource = "ds", ShowRowTotal = false });
        
        var json = _parser.Serialize(t);
        var t2 = _parser.Parse(json);
        
        var el = Assert.IsType<CrossTabElement>(t2.Bands[0].Elements[0]);
        Assert.Equal("ds", el.DataSource);
        Assert.False(el.ShowRowTotal);
    }

    [Fact]
    public void RoundTrip_MultipleElements_PreservesAll()
    {
        var t = new ReportTemplate();
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
        t.Bands[0].Elements.Add(new TextElement { Text = "Title" });
        t.Bands[0].Elements.Add(new ImageElement { Source = "logo.png" });
        t.Bands[0].Elements.Add(new LineElement { LineWidth = 1 });
        
        var json = _parser.Serialize(t);
        var t2 = _parser.Parse(json);
        
        Assert.Equal(3, t2.Bands[0].Elements.Count);
        Assert.IsType<TextElement>(t2.Bands[0].Elements[0]);
        Assert.IsType<ImageElement>(t2.Bands[0].Elements[1]);
        Assert.IsType<LineElement>(t2.Bands[0].Elements[2]);
    }

    [Fact]
    public void RoundTrip_MultipleBands_PreservesAll()
    {
        var t = new ReportTemplate();
        t.Bands.Add(new Band { Type = BandType.ReportHeader, Height = 30 });
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
        t.Bands.Add(new Band { Type = BandType.ReportFooter, Height = 20 });
        
        var json = _parser.Serialize(t);
        var t2 = _parser.Parse(json);
        
        Assert.Equal(3, t2.Bands.Count);
        Assert.Equal(BandType.ReportHeader, t2.Bands[0].Type);
        Assert.Equal(BandType.Detail, t2.Bands[1].Type);
        Assert.Equal(BandType.ReportFooter, t2.Bands[2].Type);
    }
}

/// <summary>
/// TemplateParseException 测试
/// </summary>
public class TemplateParseExceptionTests
{
    [Fact]
    public void TemplateParseException_WithMessage_HasMessage()
    {
        var ex = new TemplateParseException("Test error");
        Assert.Equal("Test error", ex.Message);
    }

    [Fact]
    public void TemplateParseException_WithInnerException_HasInnerException()
    {
        var inner = new Exception("Inner error");
        var ex = new TemplateParseException("Outer error", inner);
        Assert.Equal("Outer error", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void TemplateParseException_IsException()
    {
        var ex = new TemplateParseException("Test");
        Assert.IsAssignableFrom<Exception>(ex);
    }
}
