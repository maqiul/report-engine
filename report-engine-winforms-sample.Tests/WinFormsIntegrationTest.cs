using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ReportEngine.Core;
using ReportEngine.Core.Parsing;
using ReportEngine.Core.Rendering;
using ReportEngine.Core.SubReports;
using ReportEngine.Export.Excel;
using ReportEngine.Export.Pdf;
using ReportEngine.Viewer.WinForms;
using Xunit;
// Xunit.StaFact 1.1.11: namespace = Xunit, 提供 [WinFormsFact] / [StaFact] / [WpfFact]

namespace ReportEngine.WinFormsSample.Tests;

/// <summary>
/// WinForms 集成测试 — 验证 ParseFile → RenderAsync → viewer.SetReport 完整链路 + 导出
/// </summary>
public class WinFormsIntegrationTest
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
    public async Task Renderer_Detail_Band_Expands_To_Three_Rows()
    {
        var template = LoadTemplate();
        var resolver = new FileSystemTemplateResolver(Path.GetDirectoryName(SampleRptx)!);
        var renderer = new ReportRenderer(resolver);

        var rendered = await renderer.RenderAsync(template, SampleData());

        Assert.True(rendered.Pages.Count >= 1);
        var totalElements = rendered.Pages.Sum(p => p.Elements?.Count ?? 0);
        Assert.True(totalElements >= 4, $"应有 ≥4 元素块，实际 {totalElements}");
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

        string json = Parser.Serialize(template);
        var reparsed = Parser.Parse(json);
        var reTextEl = (TextElement)reparsed.Bands.First(b => b.Type == BandType.ReportHeader).Elements[0];
        Assert.Equal("动态新标题", reTextEl.Text);

        textEl.Text = original;
    }

    [Fact]
    public async Task PdfExport_Produces_NonEmpty_Bytes()
    {
        var template = LoadTemplate();
        var resolver = new FileSystemTemplateResolver(Path.GetDirectoryName(SampleRptx)!);
        var renderer = new ReportRenderer(resolver);

        var rendered = await renderer.RenderAsync(template, SampleData());

        var exporter = new PdfSharpExporter();
        byte[] pdfBytes = exporter.Export(rendered);

        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);
        Assert.True(pdfBytes.Length >= 5);
        // PDF 头部魔数 "%PDF"
        Assert.Equal((byte)'%', pdfBytes[0]);
        Assert.Equal((byte)'P',  pdfBytes[1]);
        Assert.Equal((byte)'D',  pdfBytes[2]);
        Assert.Equal((byte)'F',  pdfBytes[3]);
    }

    [Fact]
    public async Task ExcelExport_Produces_NonEmpty_Bytes()
    {
        var template = LoadTemplate();
        var resolver = new FileSystemTemplateResolver(Path.GetDirectoryName(SampleRptx)!);
        var renderer = new ReportRenderer(resolver);

        var rendered = await renderer.RenderAsync(template, SampleData());

        var exporter = new ClosedXmlExporter();
        byte[] xlsxBytes = exporter.Export(rendered);

        Assert.NotNull(xlsxBytes);
        Assert.NotEmpty(xlsxBytes);
        // xlsx = zip 格式，PK 头
        Assert.True(xlsxBytes.Length >= 4);
        Assert.Equal((byte)'P', xlsxBytes[0]);
        Assert.Equal((byte)'K', xlsxBytes[1]);
    }

    [WinFormsFact]
    public async Task WinFormsViewerControl_Accepts_RenderedReport()
    {
        var template = LoadTemplate();
        var resolver = new FileSystemTemplateResolver(Path.GetDirectoryName(SampleRptx)!);
        var renderer = new ReportRenderer(resolver);

        var rendered = await renderer.RenderAsync(template, SampleData());

        var viewer = new ReportViewerControl();
        viewer.SetReport(rendered);

        Assert.Equal(rendered.Pages.Count, viewer.TotalPages);
        Assert.Equal(0, viewer.CurrentPage);
        Assert.True(viewer.Zoom > 0f);
    }

    [WinFormsFact]
    public void WinFormsViewerControl_Zoom_Property_Range()
    {
        // WinForms viewer 的 Zoom 是 float，钳制 0.25 ~ 4.0
        var viewer = new ReportViewerControl();
        Assert.Equal(1.0f, viewer.Zoom); // 默认 100%

        viewer.Zoom = 2.0f;
        Assert.Equal(2.0f, viewer.Zoom);

        viewer.Zoom = 0.1f; // 低于下限
        Assert.Equal(0.25f, viewer.Zoom);

        viewer.Zoom = 10.0f; // 高于上限
        Assert.Equal(4.0f, viewer.Zoom);
    }

    [WinFormsFact]
    public void MainForm_Instantiates_Without_Throwing()
    {
        // 纯代码构建的 WinForms 窗体，ctor 不触发 Load 事件（未 Show），可安全实例化
        var form = new ReportEngine.WinFormsSample.MainForm();
        Assert.NotNull(form);
        Assert.True(form.Width > 0);
        Assert.True(form.Height > 0);
        Assert.False(string.IsNullOrEmpty(form.Text));
        form.Dispose();
    }
}
