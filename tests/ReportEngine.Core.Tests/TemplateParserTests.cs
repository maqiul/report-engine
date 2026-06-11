using FluentAssertions;
using ReportEngine.Core;
using ReportEngine.Core.Parsing;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// TemplateParser 行为测试：
///   - 成功解析真实模板
///   - 非法 JSON 抛出 TemplateParseException
///   - 空 bands 校验失败
///   - 子报表引用未知 dataSource 校验失败
/// </summary>
public class TemplateParserTests
{
    private static string SampleTemplatePath =>
        Path.Combine(AppContext.BaseDirectory, "Samples", "sales_order.rptx");

    private static TemplateParser NewParser() => new();

    [Fact]
    public void Parse_File_Parses_Real_Sales_Order_Template()
    {
        // Arrange
        var parser = NewParser();

        // Act
        var template = parser.ParseFile(SampleTemplatePath);

        // Assert
        template.Should().NotBeNull();
        template.Version.Should().Be("1.0");
        template.Page.Width.Should().Be(210);
        template.Page.Height.Should().Be(297);

        template.DataSources.Should().ContainSingle(ds => ds.Name == "orders");
        template.Bands.Should().HaveCount(4);
        template.Bands.Select(b => b.Type).Should().ContainInOrder(
            BandType.ReportHeader, BandType.Header, BandType.Detail, BandType.Footer);

        // detail band 必须绑定到 orders 数据源
        var detail = template.Bands.Single(b => b.Type == BandType.Detail);
        detail.DataSource.Should().Be("orders");

        // detail band 应包含一个 subreport，引用 order_detail.rptx
        var subs = detail.Elements.OfType<SubReportElement>().ToList();
        subs.Should().ContainSingle();
        subs[0].TemplateRef.Should().Be("order_detail.rptx");
        subs[0].DataBinding.Source.Should().Be("orders");
        subs[0].RepeatPerRow.Should().BeTrue();
    }

    [Fact]
    public void Parse_Invalid_Json_Throws_TemplateParseException()
    {
        var parser = NewParser();
        const string badJson = "{ this is not valid json }";

        var act = () => parser.Parse(badJson);

        act.Should().Throw<TemplateParseException>();
    }

    [Fact]
    public void Parse_Template_With_No_Bands_Fails_Validation()
    {
        var parser = NewParser();
        // 一个结构合法但 Bands 为空的模板
        var json = """{"version":"1.0","page":{"width":210,"height":297},"dataSources":[],"bands":[]}""";

        var act = () => parser.Parse(json);

        act.Should().Throw<TemplateParseException>()
            .WithMessage("*at least one band*");
    }

    [Fact]
    public void Parse_SubReport_Referencing_Unknown_DataSource_Fails_Validation()
    {
        var parser = NewParser();
        // band.DataSource 指向一个未在 dataSources 里声明的名字
        var json = """
        {
          "version": "1.0",
          "page": {"width": 210, "height": 297},
          "dataSources": [
            { "name": "orders", "type": "json", "fields": [] }
          ],
          "bands": [
            {
              "type": "detail",
              "height": 10,
              "dataSource": "ghostSource",
              "elements": []
            }
          ]
        }
        """;

        var act = () => parser.Parse(json);

        act.Should().Throw<TemplateParseException>()
            .WithMessage("*ghostSource*");
    }

    // ===== D6: 边界用例 =====

    [Fact]
    public void Parse_SubReport_With_Empty_TemplateRef_Fails_Validation()
    {
        var parser = NewParser();
        var json = """
        {
          "version": "1.0",
          "page": {"width": 210, "height": 297},
          "dataSources": [{ "name": "orders", "type": "json", "fields": [] }],
          "bands": [{
            "type": "detail", "dataSource": "orders",
            "elements": [{ "type": "subreport", "dataBinding": { "source": "orders" } }]
          }]
        }
        """;

        var act = () => parser.Parse(json);

        act.Should().Throw<TemplateParseException>()
            .WithMessage("*empty TemplateRef*");
    }

    [Fact]
    public void Parse_Unknown_Element_Type_Fails_Validation()
    {
        var parser = NewParser();
        var json = """
        {
          "version": "1.0",
          "page": {"width": 210, "height": 297},
          "dataSources": [{ "name": "orders", "type": "json", "fields": [] }],
          "bands": [{
            "type": "detail", "dataSource": "orders",
            "elements": [{ "type": "totallyMadeUpElement", "x": 0, "y": 0, "width": 10, "height": 10 }]
          }]
        }
        """;

        var act = () => parser.Parse(json);

        act.Should().Throw<TemplateParseException>()
            .WithMessage("*Unknown element type*");
    }

    [Fact]
    public void Parse_Empty_Object_Fails_Validation()
    {
        var parser = NewParser();

        var act = () => parser.Parse("{}");

        act.Should().Throw<TemplateParseException>();
    }

    [Fact]
    public void ParseFile_Nonexistent_Path_Throws_FileNotFoundException()
    {
        var parser = NewParser();
        var path = Path.Combine(Path.GetTempPath(), "definitely_not_exists_" + Guid.NewGuid() + ".rptx");

        var act = () => parser.ParseFile(path);

        act.Should().Throw<FileNotFoundException>()
            .WithMessage("*" + path + "*");
    }

    [Fact]
    public void Serialize_Then_Parse_RoundTrips_Template()
    {
        var parser = NewParser();
        var original = parser.ParseFile(SampleTemplatePath);

        var json = parser.Serialize(original);
        var roundTripped = parser.Parse(json);

        roundTripped.Version.Should().Be(original.Version);
        roundTripped.Page.Width.Should().Be(original.Page.Width);
        roundTripped.Page.Height.Should().Be(original.Page.Height);
        roundTripped.DataSources.Should().HaveCount(original.DataSources.Count);
        roundTripped.Bands.Should().HaveCount(original.Bands.Count);
    }

    [Fact]
    public void Parse_Template_With_TemplateParameters_Succeeds()
    {
        var parser = NewParser();
        var json = """
        {
          "version": "1.0",
          "page": {"width": 210, "height": 297},
          "dataSources": [{ "name": "orders", "type": "json", "fields": [] }],
          "parameters": [
            { "name": "title",   "label": "报表标题", "defaultValue": "销售报表", "type": "string" },
            { "name": "maxRows", "label": "最大行数", "defaultValue": "100",      "type": "number" }
          ],
          "bands": [{
            "type": "detail", "dataSource": "orders",
            "elements": [{ "type": "text", "text": "{{title}}", "x": 10, "y": 5, "width": 80, "height": 8 }]
          }]
        }
        """;

        var template = parser.Parse(json);

        template.Parameters.Should().HaveCount(2);
        template.Parameters[0].Name.Should().Be("title");
        template.Parameters[0].DefaultValue.Should().Be("销售报表");
        template.Parameters[1].Name.Should().Be("maxRows");
        template.Parameters[1].Type.Should().Be("number");
    }
}