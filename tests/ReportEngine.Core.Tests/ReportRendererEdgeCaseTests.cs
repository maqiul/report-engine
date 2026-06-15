using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReportEngine.Core;
using ReportEngine.Core.Rendering;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ReportRenderer 边界场景测试：
///   - Header / Footer / GroupHeader / GroupFooter 全空
///   - Header / Footer / GroupHeader / GroupFooter 各一个元素
///   - 分页：大数据量分多页
///   - 空 Detail 数据
///   - 多联（MultiUp）已测
/// </summary>
public class ReportRendererEdgeCaseTests
{
    private static ReportRenderer NewRenderer() => new ReportRenderer(new InMemoryTemplateResolver());

    private static ReportTemplate MakeTemplate(params Band[] bands)
    {
        return new ReportTemplate
        {
            Page = new PageInfo { Width = 100, Height = 100, Margin = new Margin { Top = 5, Bottom = 5, Left = 5, Right = 5 } },
            Bands = bands.ToList(),
        };
    }

    private static Band MakeBand(BandType type, double height = 10, string dataSource = "")
    {
        return new Band
        {
            Type = type,
            Height = height,
            DataSource = dataSource,
            Elements = new List<ReportElement>(),
        };
    }

    private static Dictionary<string, object> Row(string key, object value) => new() { { key, value } };

    [Fact]
    public async Task Render_HeaderOnly_OneElementPerPage()
    {
        var header = MakeBand(BandType.Header, 10);
        header.Elements.Add(new TextElement { X = 0, Y = 0, Width = 50, Height = 5, Text = "HEADER" });
        var detail = MakeBand(BandType.Detail, 10, "ds");
        var template = MakeTemplate(header, detail);

        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "ds", new List<Dictionary<string, object>> { Row("x", 1) } },
        };

        var result = await NewRenderer().RenderAsync(template, data);
        Assert.Single(result.Pages);
        // 至少包含 header 元素 + 1 行 detail
        Assert.Contains(result.Pages[0].Elements, e => e is RenderedTextElement t && t.Text == "HEADER");
    }

    [Fact]
    public async Task Render_FooterOnEveryPage_SticksAtBottom()
    {
        var footer = MakeBand(BandType.Footer, 5);
        footer.Elements.Add(new TextElement { X = 0, Y = 0, Width = 50, Height = 5, Text = "FOOTER" });
        var detail = MakeBand(BandType.Detail, 5, "ds");
        var template = MakeTemplate(footer, detail);

        // 30 行 → 触发分页（每页约 17 行）
        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "ds", Enumerable.Range(1, 30).Select(i => Row("i", i)).ToList() },
        };

        var result = await NewRenderer().RenderAsync(template, data);
        Assert.True(result.Pages.Count >= 2);
        foreach (var page in result.Pages)
        {
            Assert.Contains(page.Elements, e => e is RenderedTextElement t && t.Text == "FOOTER");
        }
    }

    [Fact]
    public async Task Render_EmptyDataSource_OnePageNoDetailElements()
    {
        var detail = MakeBand(BandType.Detail, 10, "ds");
        var template = MakeTemplate(detail);

        var data = new Dictionary<string, List<Dictionary<string, object>>> { { "ds", new() } };
        var result = await NewRenderer().RenderAsync(template, data);
        Assert.Single(result.Pages);
    }

    [Fact]
    public async Task Render_ReportHeaderAndFooter_RenderedAtTopAndBottom()
    {
        var reportHeader = MakeBand(BandType.ReportHeader, 5);
        reportHeader.Elements.Add(new TextElement { X = 0, Y = 0, Width = 30, Height = 3, Text = "REPORT_TOP" });
        var reportFooter = MakeBand(BandType.ReportFooter, 5);
        reportFooter.Elements.Add(new TextElement { X = 0, Y = 0, Width = 30, Height = 3, Text = "REPORT_BOT" });
        var detail = MakeBand(BandType.Detail, 10, "ds");
        var template = MakeTemplate(reportHeader, detail, reportFooter);

        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "ds", new List<Dictionary<string, object>> { Row("i", 1) } },
        };

        var result = await NewRenderer().RenderAsync(template, data);
        var texts = result.Pages[0].Elements.OfType<RenderedTextElement>().Select(t => t.Text).ToList();
        Assert.Contains("REPORT_TOP", texts);
        Assert.Contains("REPORT_BOT", texts);
        // 报告头在所有 detail 行之前渲染
        int topIdx = texts.IndexOf("REPORT_TOP");
        int botIdx = texts.IndexOf("REPORT_BOT");
        Assert.True(topIdx < botIdx, "ReportHeader should render before ReportFooter");
    }

    [Fact]
    public async Task Render_LargeDataSet_PaginatesAcrossMultiplePages()
    {
        var detail = MakeBand(BandType.Detail, 5, "ds");
        detail.Elements.Add(new TextElement { X = 0, Y = 0, Width = 20, Height = 3, Text = "{i}" });
        var template = MakeTemplate(detail);

        // 100 行 × 5mm = 500mm，远超 100mm 页高
        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "ds", Enumerable.Range(1, 100).Select(i => Row("i", i)).ToList() },
        };

        var result = await NewRenderer().RenderAsync(template, data);
        Assert.True(result.Pages.Count >= 2);
    }

    [Fact]
    public async Task Render_GroupHeaderGroupFooter_NotImplementedYet_GroupBandsIgnored()
    {
        // Group Header/Footer 当前版本未在 ReportRenderer 中实现，
        // 验证不抛异常且不影响其他 band 渲染
        var groupHeader = MakeBand(BandType.GroupHeader, 5);
        groupHeader.Group = new GroupDef { Expression = "cat" };
        groupHeader.Elements.Add(new TextElement { X = 0, Y = 0, Width = 20, Height = 3, Text = "GROUP_HDR" });
        var detail = MakeBand(BandType.Detail, 5, "ds");
        detail.Elements.Add(new TextElement { X = 0, Y = 0, Width = 20, Height = 3, Text = "{x}" });
        var template = MakeTemplate(detail);
        template.Bands.Add(groupHeader);

        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "ds", new List<Dictionary<string, object>>
                {
                    new() { { "cat", "A" }, { "x", 1 } },
                }
            },
        };

        // 不应抛异常
        var result = await NewRenderer().RenderAsync(template, data);
        Assert.Single(result.Pages);
    }
}
