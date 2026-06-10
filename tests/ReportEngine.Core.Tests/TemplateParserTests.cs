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
}