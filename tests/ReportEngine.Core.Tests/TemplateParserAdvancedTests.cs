using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ReportEngine.Core;
using ReportEngine.Core.Parsing;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// TemplateParser 全流程测试：
///   - Parse / ParseFile / Serialize
///   - Band 必须 ≥ 1
///   - DataSource 必须在 DataSources 列表注册
///   - SubReport.TemplateRef 必须非空
///   - SubReport.DataBinding.Source 校验
///   - ReportElementConverter 路由（text/image/line/shape/subreport/chart/barcode/table/crosstab）
///   - 未知 type 抛 TemplateParseException
///   - 损坏 JSON 抛 TemplateParseException
/// </summary>
public class TemplateParserAdvancedTests
{
    private readonly TemplateParser _parser = new();

    private static string MinimalValidJson => @"{
        ""page"": { ""width"": 210, ""height"": 297 },
        ""dataSources"": [ { ""name"": ""ds"", ""type"": ""json"" } ],
        ""bands"": [
            { ""type"": ""detail"", ""dataSource"": ""ds"", ""elements"": [] }
        ]
    }";

    [Fact]
    public void Parse_ValidJson_ReturnsTemplate()
    {
        var t = _parser.Parse(MinimalValidJson);
        Assert.NotNull(t);
        Assert.Single(t.Bands);
        Assert.Single(t.DataSources);
    }

    [Fact]
    public void Parse_NoBands_Throws()
    {
        var json = @"{ ""page"": {}, ""dataSources"": [], ""bands"": [] }";
        var ex = Assert.Throws<TemplateParseException>(() => _parser.Parse(json));
        Assert.Contains("at least one band", ex.Message);
    }

    [Fact]
    public void Parse_BandReferencesUnknownDS_Throws()
    {
        var json = @"{
            ""dataSources"": [ { ""name"": ""ds"" } ],
            ""bands"": [
                { ""type"": ""detail"", ""dataSource"": ""unknown"", ""elements"": [] }
            ]
        }";
        var ex = Assert.Throws<TemplateParseException>(() => _parser.Parse(json));
        Assert.Contains("unknown datasource", ex.Message);
    }

    [Fact]
    public void Parse_NullJson_Throws()
    {
        Assert.Throws<TemplateParseException>(() => _parser.Parse("null"));
    }

    [Fact]
    public void Parse_InvalidJson_Throws()
    {
        Assert.Throws<TemplateParseException>(() => _parser.Parse("{ invalid"));
    }

    [Fact]
    public void Parse_UnknownElementType_Throws()
    {
        var json = @"{
            ""dataSources"": [ { ""name"": ""ds"" } ],
            ""bands"": [
                { ""type"": ""detail"", ""dataSource"": ""ds"", ""elements"": [
                    { ""type"": ""unknown_type"", ""x"": 0, ""y"": 0, ""width"": 10, ""height"": 10 }
                ] }
            ]
        }";
        Assert.Throws<TemplateParseException>(() => _parser.Parse(json));
    }

    [Fact]
    public void Parse_TextElement_RoutesCorrectly()
    {
        var json = @"{
            ""dataSources"": [ { ""name"": ""ds"" } ],
            ""bands"": [
                { ""type"": ""detail"", ""dataSource"": ""ds"", ""elements"": [
                    { ""type"": ""text"", ""text"": ""hello"", ""x"": 0, ""y"": 0, ""width"": 50, ""height"": 10 }
                ] }
            ]
        }";
        var t = _parser.Parse(json);
        var el = t.Bands[0].Elements[0];
        Assert.IsType<TextElement>(el);
        Assert.Equal("hello", ((TextElement)el).Text);
    }

    [Fact]
    public void Parse_AllElementTypes_RouteCorrectly()
    {
        var json = @"{
            ""dataSources"": [ { ""name"": ""ds"" } ],
            ""bands"": [
                { ""type"": ""detail"", ""dataSource"": ""ds"", ""elements"": [
                    { ""type"": ""text"", ""x"": 0, ""y"": 0, ""width"": 10, ""height"": 10 },
                    { ""type"": ""image"", ""source"": ""a.png"", ""x"": 0, ""y"": 0, ""width"": 10, ""height"": 10 },
                    { ""type"": ""line"", ""x"": 0, ""y"": 0, ""width"": 10, ""height"": 10 },
                    { ""type"": ""shape"", ""shape"": ""rectangle"", ""x"": 0, ""y"": 0, ""width"": 10, ""height"": 10 },
                    { ""type"": ""chart"", ""chartType"": ""bar"", ""x"": 0, ""y"": 0, ""width"": 100, ""height"": 100 },
                    { ""type"": ""barcode"", ""format"": ""code128"", ""value"": ""123"", ""x"": 0, ""y"": 0, ""width"": 100, ""height"": 50 },
                    { ""type"": ""table"", ""x"": 0, ""y"": 0, ""width"": 100, ""height"": 50 },
                    { ""type"": ""crosstab"", ""dataSource"": ""ds"", ""x"": 0, ""y"": 0, ""width"": 100, ""height"": 50 }
                ] }
            ]
        }";
        var t = _parser.Parse(json);
        var els = t.Bands[0].Elements;
        Assert.IsType<TextElement>(els[0]);
        Assert.IsType<ImageElement>(els[1]);
        Assert.IsType<LineElement>(els[2]);
        Assert.IsType<ShapeElement>(els[3]);
        Assert.IsType<ChartElement>(els[4]);
        Assert.IsType<BarcodeElement>(els[5]);
        Assert.IsType<TableElement>(els[6]);
        Assert.IsType<CrossTabElement>(els[7]);
    }

    [Fact]
    public void Parse_Serialize_PreservesBandCount()
    {
        var t = _parser.Parse(MinimalValidJson);
        var json = _parser.Serialize(t);
        var t2 = _parser.Parse(json);
        Assert.Equal(t.Bands.Count, t2.Bands.Count);
        Assert.Equal(t.DataSources.Count, t2.DataSources.Count);
    }

    [Fact]
    public void ParseFile_NonExistent_Throws()
    {
        Assert.Throws<FileNotFoundException>(() => _parser.ParseFile("Z:\\nonexistent\\path\\template.rptx"));
    }

    [Fact]
    public void ParseFile_ValidFile_Parses()
    {
        var path = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.rptx");
        File.WriteAllText(path, MinimalValidJson);
        try
        {
            var t = _parser.ParseFile(path);
            Assert.NotNull(t);
            Assert.Single(t.Bands);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Parse_SubReport_EmptyTemplateRef_Throws()
    {
        var json = @"{
            ""dataSources"": [ { ""name"": ""ds"" } ],
            ""bands"": [
                { ""type"": ""detail"", ""dataSource"": ""ds"", ""elements"": [
                    { ""type"": ""subreport"", ""templateRef"": """", ""x"": 0, ""y"": 0, ""width"": 10, ""height"": 10 }
                ] }
            ]
        }";
        var ex = Assert.Throws<TemplateParseException>(() => _parser.Parse(json));
        Assert.Contains("TemplateRef", ex.Message);
    }

    [Fact]
    public void Parse_SubReport_UnknownDataBinding_Throws()
    {
        var json = @"{
            ""dataSources"": [ { ""name"": ""ds"" } ],
            ""bands"": [
                { ""type"": ""detail"", ""dataSource"": ""ds"", ""elements"": [
                    { ""type"": ""subreport"", ""templateRef"": ""x.rptx"", ""dataBinding"": { ""source"": ""unknown"" }, ""x"": 0, ""y"": 0, ""width"": 10, ""height"": 10 }
                ] }
            ]
        }";
        var ex = Assert.Throws<TemplateParseException>(() => _parser.Parse(json));
        Assert.Contains("unknown datasource", ex.Message);
    }

    [Fact]
    public void Parse_SubReport_ValidTemplateRef_Passes()
    {
        var json = @"{
            ""dataSources"": [ { ""name"": ""ds"" } ],
            ""bands"": [
                { ""type"": ""detail"", ""dataSource"": ""ds"", ""elements"": [
                    { ""type"": ""subreport"", ""templateRef"": ""child.rptx"", ""x"": 0, ""y"": 0, ""width"": 10, ""height"": 10 }
                ] }
            ]
        }";
        var t = _parser.Parse(json);
        var sub = (SubReportElement)t.Bands[0].Elements[0];
        Assert.Equal("child.rptx", sub.TemplateRef);
    }

    [Fact]
    public void Parse_NullFields_AreHandled()
    {
        var json = @"{
            ""dataSources"": [ { ""name"": ""ds"" } ],
            ""bands"": [
                { ""type"": ""detail"", ""dataSource"": ""ds"", ""elements"": [] }
            ]
        }";
        var t = _parser.Parse(json);
        // 各默认字段可访问
        Assert.NotNull(t.Page);
        Assert.NotNull(t.Bands);
    }
}
