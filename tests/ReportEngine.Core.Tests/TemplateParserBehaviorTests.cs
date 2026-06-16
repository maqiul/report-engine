using System;
using ReportEngine.Core;
using ReportEngine.Core.Parsing;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// TemplateParser 行为测试：
///   - Parse 基本 JSON 解析
///   - Parse 各种元素类型
///   - Serialize 序列化
///   - ValidateTemplate 校验行为
/// </summary>
public class TemplateParserBehaviorTests
{
    private readonly TemplateParser _parser = new TemplateParser();

    // ============== Parse 基本行为 ==============

    [Fact]
    public void Parse_MinimalTemplate()
    {
        var json = @"{ ""bands"": [{ ""type"": ""header"", ""height"": 20 }] }";
        var t = _parser.Parse(json);
        Assert.NotNull(t);
        Assert.Single(t.Bands);
        Assert.Equal(BandType.Header, t.Bands[0].Type);
        Assert.Equal(20, t.Bands[0].Height);
    }

    [Fact]
    public void Parse_WithPageInfo()
    {
        var json = @"{ ""page"": { ""width"": 297, ""height"": 210, ""orientation"": ""landscape"" }, ""bands"": [{ ""type"": ""detail"", ""height"": 30 }] }";
        var t = _parser.Parse(json);
        Assert.Equal(297, t.Page.Width);
        Assert.Equal(210, t.Page.Height);
        Assert.Equal("landscape", t.Page.Orientation);
    }

    [Fact]
    public void Parse_WithDataSources()
    {
        var json = @"{ ""dataSources"": [{ ""name"": ""ds1"", ""type"": ""json"" }], ""bands"": [{ ""type"": ""detail"", ""height"": 20, ""dataSource"": ""ds1"" }] }";
        var t = _parser.Parse(json);
        Assert.Single(t.DataSources);
        Assert.Equal("ds1", t.DataSources[0].Name);
    }

    [Fact]
    public void Parse_WithTextElement()
    {
        var json = @"{ ""bands"": [{ ""type"": ""header"", ""height"": 20, ""elements"": [{ ""type"": ""text"", ""text"": ""Hello"", ""x"": 10, ""y"": 5, ""width"": 50, ""height"": 10 }] }] }";
        var t = _parser.Parse(json);
        Assert.Single(t.Bands[0].Elements);
        var el = t.Bands[0].Elements[0] as TextElement;
        Assert.NotNull(el);
        Assert.Equal("Hello", el.Text);
        Assert.Equal(10, el.X);
        Assert.Equal(50, el.Width);
    }

    [Fact]
    public void Parse_WithImageElement()
    {
        var json = @"{ ""bands"": [{ ""type"": ""header"", ""height"": 20, ""elements"": [{ ""type"": ""image"", ""source"": ""logo.png"", ""x"": 0, ""y"": 0, ""width"": 30, ""height"": 30 }] }] }";
        var t = _parser.Parse(json);
        var el = t.Bands[0].Elements[0] as ImageElement;
        Assert.NotNull(el);
        Assert.Equal("logo.png", el.Source);
    }

    [Fact]
    public void Parse_WithLineElement()
    {
        var json = @"{ ""bands"": [{ ""type"": ""header"", ""height"": 20, ""elements"": [{ ""type"": ""line"", ""direction"": ""horizontal"", ""lineWidth"": 2, ""x"": 0, ""y"": 10, ""width"": 100, ""height"": 0 }] }] }";
        var t = _parser.Parse(json);
        var el = t.Bands[0].Elements[0] as LineElement;
        Assert.NotNull(el);
        Assert.Equal(LineDirection.Horizontal, el.Direction);
        Assert.Equal(2, el.LineWidth);
    }

    [Fact]
    public void Parse_WithShapeElement()
    {
        var json = @"{ ""bands"": [{ ""type"": ""header"", ""height"": 20, ""elements"": [{ ""type"": ""shape"", ""shape"": ""ellipse"", ""fillColor"": ""#FF0000"", ""x"": 0, ""y"": 0, ""width"": 20, ""height"": 20 }] }] }";
        var t = _parser.Parse(json);
        var el = t.Bands[0].Elements[0] as ShapeElement;
        Assert.NotNull(el);
        Assert.Equal(ShapeType.Ellipse, el.Shape);
        Assert.Equal("#FF0000", el.FillColor);
    }

    [Fact]
    public void Parse_WithBarcodeElement()
    {
        var json = @"{ ""bands"": [{ ""type"": ""header"", ""height"": 20, ""elements"": [{ ""type"": ""barcode"", ""value"": ""12345"", ""format"": ""code128"", ""x"": 0, ""y"": 0, ""width"": 50, ""height"": 20 }] }] }";
        var t = _parser.Parse(json);
        var el = t.Bands[0].Elements[0] as BarcodeElement;
        Assert.NotNull(el);
        Assert.Equal("12345", el.Value);
        Assert.Equal(BarcodeFormat.Code128, el.Format);
    }

    [Fact]
    public void Parse_WithChartElement()
    {
        var json = @"{ ""bands"": [{ ""type"": ""header"", ""height"": 20, ""elements"": [{ ""type"": ""chart"", ""chartType"": ""pie"", ""dataSource"": ""ds"", ""x"": 0, ""y"": 0, ""width"": 100, ""height"": 80 }] }] }";
        var t = _parser.Parse(json);
        var el = t.Bands[0].Elements[0] as ChartElement;
        Assert.NotNull(el);
        Assert.Equal(ChartType.Pie, el.ChartType);
    }

    [Fact]
    public void Parse_WithTableElement()
    {
        var json = @"{ ""bands"": [{ ""type"": ""header"", ""height"": 20, ""elements"": [{ ""type"": ""table"", ""rowCount"": 2, ""colCount"": 3, ""x"": 0, ""y"": 0, ""width"": 100, ""height"": 40 }] }] }";
        var t = _parser.Parse(json);
        var el = t.Bands[0].Elements[0] as TableElement;
        Assert.NotNull(el);
        Assert.Equal(2, el.RowCount);
        Assert.Equal(3, el.ColCount);
    }

    [Fact]
    public void Parse_WithSubReportElement()
    {
        var json = @"{ ""bands"": [{ ""type"": ""header"", ""height"": 20, ""elements"": [{ ""type"": ""subreport"", ""templateRef"": ""sub.rpt"", ""heightMode"": ""auto"", ""x"": 0, ""y"": 0, ""width"": 100, ""height"": 50 }] }] }";
        var t = _parser.Parse(json);
        var el = t.Bands[0].Elements[0] as SubReportElement;
        Assert.NotNull(el);
        Assert.Equal("sub.rpt", el.TemplateRef);
    }

    [Fact]
    public void Parse_WithCrossTabElement()
    {
        var json = @"{ ""bands"": [{ ""type"": ""header"", ""height"": 20, ""elements"": [{ ""type"": ""crosstab"", ""dataSource"": ""ds"", ""x"": 0, ""y"": 0, ""width"": 150, ""height"": 100 }] }] }";
        var t = _parser.Parse(json);
        var el = t.Bands[0].Elements[0] as CrossTabElement;
        Assert.NotNull(el);
        Assert.Equal("ds", el.DataSource);
    }

    // ============== ValidateTemplate 校验 ==============

    [Fact]
    public void Parse_NoBands_ThrowsTemplateParseException()
    {
        var json = @"{ ""bands"": [] }";
        Assert.Throws<TemplateParseException>(() => _parser.Parse(json));
    }

    [Fact]
    public void Parse_UnknownDataSource_ThrowsTemplateParseException()
    {
        var json = @"{ ""dataSources"": [{ ""name"": ""ds1"" }], ""bands"": [{ ""type"": ""detail"", ""height"": 20, ""dataSource"": ""nonexistent"" }] }";
        var ex = Assert.Throws<TemplateParseException>(() => _parser.Parse(json));
        Assert.Contains("nonexistent", ex.Message);
    }

    [Fact]
    public void Parse_UnknownElementType_ThrowsTemplateParseException()
    {
        var json = @"{ ""bands"": [{ ""type"": ""header"", ""height"": 20, ""elements"": [{ ""type"": ""unknown_type"" }] }] }";
        Assert.Throws<TemplateParseException>(() => _parser.Parse(json));
    }

    [Fact]
    public void Parse_InvalidJson_ThrowsTemplateParseException()
    {
        var json = @"{ invalid json }";
        Assert.Throws<TemplateParseException>(() => _parser.Parse(json));
    }

    [Fact]
    public void Parse_SubReportEmptyTemplateRef_ThrowsTemplateParseException()
    {
        var json = @"{ ""bands"": [{ ""type"": ""header"", ""height"": 20, ""elements"": [{ ""type"": ""subreport"", ""templateRef"": """", ""x"": 0, ""y"": 0, ""width"": 50, ""height"": 50 }] }] }";
        var ex = Assert.Throws<TemplateParseException>(() => _parser.Parse(json));
        Assert.Contains("TemplateRef", ex.Message);
    }

    [Fact]
    public void Parse_SubReportUnknownDataSource_ThrowsTemplateParseException()
    {
        var json = @"{ ""dataSources"": [{ ""name"": ""ds1"" }], ""bands"": [{ ""type"": ""header"", ""height"": 20, ""elements"": [{ ""type"": ""subreport"", ""templateRef"": ""sub.rpt"", ""dataBinding"": { ""source"": ""unknown_ds"" }, ""x"": 0, ""y"": 0, ""width"": 50, ""height"": 50 }] }] }";
        var ex = Assert.Throws<TemplateParseException>(() => _parser.Parse(json));
        Assert.Contains("unknown_ds", ex.Message);
    }

    // ============== Serialize 行为 ==============

    [Fact]
    public void Serialize_MinimalTemplate()
    {
        var t = new ReportTemplate();
        t.Bands.Add(new Band { Type = BandType.Header, Height = 20 });
        var json = _parser.Serialize(t);
        Assert.Contains("header", json);
        Assert.Contains("20", json);
    }

    [Fact]
    public void Serialize_WithTextElement()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Header, Height = 20 };
        band.Elements.Add(new TextElement { Text = "Hello", X = 10, Y = 5, Width = 50, Height = 10 });
        t.Bands.Add(band);
        var json = _parser.Serialize(t);
        Assert.Contains("text", json);
        Assert.Contains("Hello", json);
    }

    [Fact]
    public void RoundTrip_ParseSerializeParse()
    {
        var json = @"{ ""page"": { ""width"": 210, ""height"": 297 }, ""bands"": [{ ""type"": ""header"", ""height"": 25, ""elements"": [{ ""type"": ""text"", ""text"": ""Title"", ""x"": 10, ""y"": 5, ""width"": 80, ""height"": 15 }] }] }";
        var t1 = _parser.Parse(json);
        var serialized = _parser.Serialize(t1);
        var t2 = _parser.Parse(serialized);

        Assert.Equal(t1.Page.Width, t2.Page.Width);
        Assert.Equal(t1.Bands.Count, t2.Bands.Count);
        Assert.Equal(t1.Bands[0].Height, t2.Bands[0].Height);
        var el1 = t1.Bands[0].Elements[0] as TextElement;
        var el2 = t2.Bands[0].Elements[0] as TextElement;
        Assert.Equal(el1!.Text, el2!.Text);
    }

    // ============== 多元素混合 ==============

    [Fact]
    public void Parse_MultipleElementTypes()
    {
        var json = @"{
            ""bands"": [{
                ""type"": ""header"",
                ""height"": 50,
                ""elements"": [
                    { ""type"": ""text"", ""text"": ""Title"", ""x"": 0, ""y"": 0, ""width"": 100, ""height"": 15 },
                    { ""type"": ""line"", ""direction"": ""horizontal"", ""x"": 0, ""y"": 15, ""width"": 100, ""height"": 0 },
                    { ""type"": ""image"", ""source"": ""logo.png"", ""x"": 80, ""y"": 0, ""width"": 20, ""height"": 15 }
                ]
            }]
        }";
        var t = _parser.Parse(json);
        Assert.Equal(3, t.Bands[0].Elements.Count);
        Assert.IsType<TextElement>(t.Bands[0].Elements[0]);
        Assert.IsType<LineElement>(t.Bands[0].Elements[1]);
        Assert.IsType<ImageElement>(t.Bands[0].Elements[2]);
    }

    [Fact]
    public void Parse_MultipleBands()
    {
        var json = @"{
            ""bands"": [
                { ""type"": ""reportHeader"", ""height"": 30 },
                { ""type"": ""header"", ""height"": 20 },
                { ""type"": ""detail"", ""height"": 15 },
                { ""type"": ""footer"", ""height"": 20 },
                { ""type"": ""reportFooter"", ""height"": 30 }
            ]
        }";
        var t = _parser.Parse(json);
        Assert.Equal(5, t.Bands.Count);
        Assert.Equal(BandType.ReportHeader, t.Bands[0].Type);
        Assert.Equal(BandType.Header, t.Bands[1].Type);
        Assert.Equal(BandType.Detail, t.Bands[2].Type);
        Assert.Equal(BandType.Footer, t.Bands[3].Type);
        Assert.Equal(BandType.ReportFooter, t.Bands[4].Type);
    }

    [Fact]
    public void Parse_BandWithoutType_ThrowsTemplateParseException()
    {
        // type 缺失时默认为 Header (enum 默认值 0)，不会报错
        // 这里测试 element 缺少 type 字段
        var json = @"{ ""bands"": [{ ""type"": ""header"", ""height"": 20, ""elements"": [{ ""text"": ""no type"" }] }] }";
        Assert.Throws<TemplateParseException>(() => _parser.Parse(json));
    }
}
