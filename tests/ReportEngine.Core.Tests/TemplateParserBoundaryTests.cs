using ReportEngine.Core;
using ReportEngine.Core.Parsing;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// TemplateParser 解析边界测试
/// </summary>
public class TemplateParserBoundaryTests
{
    private readonly TemplateParser _parser = new();

    // ============== 空/无效输入 ==============

    [Fact]
    public void Parse_EmptyJson_Throws()
    {
        Assert.ThrowsAny<Exception>(() => _parser.Parse(""));
    }

    [Fact]
    public void Parse_InvalidJson_Throws()
    {
        Assert.ThrowsAny<Exception>(() => _parser.Parse("{invalid json"));
    }

    [Fact]
    public void Parse_NullJson_Throws()
    {
        Assert.ThrowsAny<Exception>(() => _parser.Parse(null!));
    }

    // ============== 最小有效模板 ==============

    [Fact]
    public void Parse_MinimalTemplate_Works()
    {
        var json = @"{
            ""bands"": [{""type"": ""Detail"", ""height"": 50}],
            ""dataSources"": [{""name"": ""ds""}]
        }";
        var template = _parser.Parse(json);
        Assert.Single(template.Bands);
        Assert.Single(template.DataSources);
    }

    // ============== 无 Bands ==============

    [Fact]
    public void Parse_NoBands_Throws()
    {
        var json = @"{""dataSources"": [{""name"": ""ds""}]}";
        Assert.Throws<TemplateParseException>(() => _parser.Parse(json));
    }

    // ============== 空 Bands 数组 ==============

    [Fact]
    public void Parse_EmptyBands_Throws()
    {
        var json = @"{""bands"": [], ""dataSources"": [{""name"": ""ds""}]}";
        Assert.Throws<TemplateParseException>(() => _parser.Parse(json));
    }

    // ============== 未知元素类型 ==============

    [Fact]
    public void Parse_UnknownElementType_Throws()
    {
        var json = @"{
            ""bands"": [{""type"": ""Detail"", ""height"": 50, ""elements"": [{""type"": ""unknowntype"", ""x"": 0, ""y"": 0, ""width"": 10, ""height"": 10}]}],
            ""dataSources"": [{""name"": ""ds""}]
        }";
        Assert.Throws<TemplateParseException>(() => _parser.Parse(json));
    }

    // ============== 未知 BandType ==============

    [Fact]
    public void Parse_UnknownBandType_WorksOrDefault()
    {
        var json = @"{
            ""bands"": [{""type"": ""UnknownBand"", ""height"": 50}],
            ""dataSources"": [{""name"": ""ds""}]
        }";
        // 未知 band type 可能抛异常或 fallback 到默认
        var ex = Record.Exception(() => _parser.Parse(json));
        // 不管是否抛异常，测试不崩溃即可
        Assert.True(ex == null || ex is TemplateParseException);
    }

    // ============== 多数据源 ==============

    [Fact]
    public void Parse_MultipleDataSources_Works()
    {
        var json = @"{
            ""bands"": [{""type"": ""Detail"", ""height"": 50}],
            ""dataSources"": [
                {""name"": ""ds1""},
                {""name"": ""ds2""},
                {""name"": ""ds3""}
            ]
        }";
        var template = _parser.Parse(json);
        Assert.Equal(3, template.DataSources.Count);
    }

    // ============== Group on Band ==============

    [Fact]
    public void Parse_BandWithGroup_Works()
    {
        var json = @"{
            ""bands"": [{""type"": ""GroupHeader"", ""height"": 50, ""group"": {""expression"": ""region""}}],
            ""dataSources"": [{""name"": ""ds""}]
        }";
        var template = _parser.Parse(json);
        Assert.NotNull(template.Bands[0].Group);
        Assert.Equal("region", template.Bands[0].Group!.Expression);
    }

    // ============== PageInfo 完整配置 ==============

    [Fact]
    public void Parse_FullPageInfo_Works()
    {
        var json = @"{
            ""bands"": [{""type"": ""Detail"", ""height"": 50}],
            ""dataSources"": [{""name"": ""ds""}],
            ""page"": {
                ""width"": 297,
                ""height"": 210
            }
        }";
        var template = _parser.Parse(json);
        Assert.Equal(297, template.Page.Width);
        Assert.Equal(210, template.Page.Height);
    }

    // ============== Serialize 输出有效 JSON ==============

    [Fact]
    public void Serialize_ProducesValidJson()
    {
        var template = new ReportTemplate();
        template.Bands.Add(new Band { Type = BandType.Detail, Height = 100 });
        template.DataSources.Add(new DataSourceDef { Name = "ds" });

        var json = _parser.Serialize(template);

        Assert.NotEmpty(json);
        Assert.Contains("\"bands\"", json);
        Assert.Contains("\"dataSources\"", json);
    }

    [Fact]
    public void Serialize_ThenParse_RoundTrips()
    {
        var original = new ReportTemplate();
        original.Bands.Add(new Band { Type = BandType.Detail, Height = 75 });
        original.DataSources.Add(new DataSourceDef { Name = "testDs" });

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        Assert.Single(parsed.Bands);
        Assert.Equal(75, parsed.Bands[0].Height);
        Assert.Equal("testDs", parsed.DataSources[0].Name);
    }

    // ============== 元素位置/尺寸 ==============

    [Fact]
    public void Parse_ElementPosition_Preserved()
    {
        var json = @"{
            ""bands"": [{
                ""type"": ""Detail"",
                ""height"": 100,
                ""elements"": [{
                    ""type"": ""text"",
                    ""x"": 15.5,
                    ""y"": 25.3,
                    ""width"": 80.7,
                    ""height"": 12.4,
                    ""text"": ""Hello""
                }]
            }],
            ""dataSources"": [{""name"": ""ds""}]
        }";
        var template = _parser.Parse(json);
        var el = Assert.IsType<TextElement>(template.Bands[0].Elements[0]);
        Assert.Equal(15.5, el.X);
        Assert.Equal(25.3, el.Y);
        Assert.Equal(80.7, el.Width);
        Assert.Equal(12.4, el.Height);
    }

    // ============== 元素背景色/边框 ==============

    [Fact]
    public void Parse_ElementBackground_Preserved()
    {
        var json = @"{
            ""bands"": [{
                ""type"": ""Detail"",
                ""height"": 100,
                ""elements"": [{
                    ""type"": ""text"",
                    ""x"": 0, ""y"": 0, ""width"": 50, ""height"": 20,
                    ""text"": ""Colored"",
                    ""backgroundColor"": ""#FFFFCC""
                }]
            }],
            ""dataSources"": [{""name"": ""ds""}]
        }";
        var template = _parser.Parse(json);
        var el = Assert.IsType<TextElement>(template.Bands[0].Elements[0]);
        Assert.Equal("#FFFFCC", el.BackgroundColor);
    }
}
