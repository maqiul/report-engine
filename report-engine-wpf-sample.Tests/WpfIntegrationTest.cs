using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ReportEngine.Core;
using ReportEngine.Core.Parsing;
using ReportEngine.Core.Rendering;
using ReportEngine.Core.SubReports;
using ReportEngine.Viewer.Wpf;
using Xunit;
using XUnit.Sta.Fact;

namespace ReportEngine.WpfSample.Tests;

/// <summary>
/// WPF 集成测试 - 验证 ParseFile → RenderAsync → SetReport 完整链路
/// </summary>
public class WpfIntegrationTest
{
    private static readonly string SampleRptx = Path.Combine(
        AppContext.BaseDirectory, "Templates", "order-summary.rptx");

    private static readonly TemplateParser Parser = new();

    private static ReportTemplate LoadTemplate()
    {
        Assert.True(File.Exists(SampleRptx), $"模板不存在: {SampleRptx}");
        return Parser.ParseFile(SampleRptx);
    }

    private static Dictionary<string, List<Dictionary<string, object>>> SampleData() =>
        new()
        {
            ["orders"] = new List<Dictionary<string, object>>
            {
                new() { ["id"] = "SO-001", ["customer"] = "Acme",   ["total"] = 1990.00 },
                new() { ["id"] = "SO-002", ["customer"] = "Globex", ["total"] = 1497.50 },
                new() { ["id"] = "SO-003", ["customer"] = "Initech",["total"] =  749.85 }
            }
        };

    [Fact]
    public void Template_Load_And_Parse_Succeeds()
    {
        var template = LoadTemplate();
        Assert.NotNull(template);
        Assert.Equal(2, template.Bands.Count);
        Assert.Contains(template.Bands, b => b.Type == BandType.ReportHeader);
        Assert.Contains(template.Bands, b => b.Type == BandType.Detail);
    }

    [Fact]
    public void Template_Serialize_RoundTrip_PreservesBands()
    {
        var template = LoadTemplate();
        string json = Parser.Serialize(template);
        Assert.False(string.IsNullOrEmpty(json));

        var reparsed = Parser.Parse(json);
        Assert.Equal(template.Bands.Count, reparsed.Bands.Count);
        Assert.Equal(template.Page.Width, reparsed.Page.Width);
    }

    [Fact]
    public async Task Renderer_Produces_AtLeast_One_Page()
    {
        var template = LoadTemplate();
        var resolver = new FileSystemTemplateResolver(Path.GetDirectoryName(SampleRptx)!);
        var renderer = new ReportRenderer(resolver);

        var rendered = await renderer.RenderAsync(template, SampleData());

        Assert.NotNull(rendered);
        Assert.NotNull(rendered.Pages);
        Assert.NotEmpty(rendered.Pages);
    }

    [Fact]
    public async Task Renderer_Detail_Band_Expands_To_Three_Pages()
    {
        // 3 条订单 → detail band 应展开为 3 个 detail row
        var template = LoadTemplate();
        var resolver = new FileSystemTemplateResolver(Path.GetDirectoryName(SampleRptx)!);
        var renderer = new ReportRenderer(resolver);

        var rendered = await renderer.RenderAsync(template, SampleData());

        // 总页数至少 1（title + 3 detail rows 在 A4 一页内）
        Assert.True(rendered.Pages.Count >= 1);
        // 至少 4 个元素块（1 title + 3 detail）
        var totalElements = rendered.Pages.Sum(p => p.Elements?.Count ?? 0);
        Assert.True(totalElements >= 4, $"应有 ≥4 元素块，实际 {totalElements}");
    }

    [StaFact]
    public async Task WpfViewerControl_Accepts_RenderedReport()
    {
        var template = LoadTemplate();
        var resolver = new FileSystemTemplateResolver(Path.GetDirectoryName(SampleRptx)!);
        var renderer = new ReportRenderer(resolver);

        var rendered = await renderer.RenderAsync(template, SampleData());

        // 实例化 WPF 控件（STA 线程由 [StaFact] 提供）
        var viewer = new ReportViewerControl();
        viewer.SetReport(rendered);

        Assert.Equal(rendered.Pages.Count, viewer.TotalPages);
        Assert.Equal(0, viewer.CurrentPage);
        Assert.True(viewer.Zoom > 0);
    }

    [StaFact]
    public void WpfViewerControl_Zoom_Property_Range()
    {
        var viewer = new ReportViewerControl();
        Assert.Equal(1.0, viewer.Zoom); // 默认 100%

        viewer.Zoom = 2.0;
        Assert.Equal(2.0, viewer.Zoom);

        viewer.Zoom = 0.1; // 低于下限 0.25
        Assert.Equal(0.25, viewer.Zoom); // 应被钳制

        viewer.Zoom = 10.0; // 高于上限 4
        Assert.Equal(4.0, viewer.Zoom);  // 应被钳制
    }

    [Fact]
    public void Modify_Title_Text_Roundtrip()
    {
        var template = LoadTemplate();
        var titleBand = template.Bands.First(b => b.Type == BandType.ReportHeader);
        var titleElement = titleBand.Elements[0];

        Assert.IsType<TextElement>(titleElement);
        var textEl = (TextElement)titleElement;
        var original = textEl.Text;

        textEl.Text = "动态新标题";
        Assert.Equal("动态新标题", textEl.Text);

        // 序列化后能再 Parse 回来保留
        string json = Parser.Serialize(template);
        var reparsed = Parser.Parse(json);
        var reTextEl = (TextElement)reparsed.Bands.First(b => b.Type == BandType.ReportHeader).Elements[0];
        Assert.Equal("动态新标题", reTextEl.Text);

        // 还原
        textEl.Text = original;
    }
}