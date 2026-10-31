using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ReportEngine.Core;
using ReportEngine.Core.Rendering;
using ReportEngine.Export.Pdf;
using Xunit;

namespace ReportEngine.Export.Pdf.Tests;

/// <summary>
/// 旋转元素 + 长文本自动换行 的 PDF 导出测试
/// </summary>
public class PdfRotationAndLongTextTests
{
    private static ReportTemplate BuildTemplate(Action<Band> setup)
    {
        var t = new ReportTemplate();
        t.Page.Width = 210;
        t.Page.Height = 297;
        t.Page.Margin = new Margin { Top = 15, Bottom = 15, Left = 15, Right = 15 };

        var band = new Band { Type = BandType.Detail, Height = 30, DataSource = "ds" };
        setup(band);
        t.Bands.Add(band);
        return t;
    }

    private static Dictionary<string, List<Dictionary<string, object>>> Data() =>
        new()
        {
            ["ds"] = new List<Dictionary<string, object>>
            {
                new() { ["name"] = "张三", ["amount"] = 100 }
            }
        };

    [Fact]
    public async Task Export_WithRotatedText_ProducesValidPdf()
    {
        var template = BuildTemplate(band =>
            band.Elements.Add(new TextElement
            {
                Text = "旋转文本", X = 20, Y = 5, Width = 60, Height = 12,
                Font = new FontDef { Size = 14 },
                Rotation = 30
            }));

        var exporter = new PdfSharpExporter();
        var rendered = await new ReportRenderer(new NoopResolver()).RenderAsync(template, Data());

        var bytes = exporter.Export(rendered);
        Assert.NotEmpty(bytes);
        Assert.Equal((byte)'%', bytes[0]);
        Assert.Equal((byte)'P', bytes[1]);
    }

    [Fact]
    public async Task Export_WithRotatedShape_ProducesValidPdf()
    {
        var template = BuildTemplate(band =>
            band.Elements.Add(new ShapeElement
            {
                Shape = ShapeType.Rectangle, X = 10, Y = 5, Width = 40, Height = 20,
                FillColor = "#DDDDDD", Rotation = -20
            }));

        var exporter = new PdfSharpExporter();
        var rendered = await new ReportRenderer(new NoopResolver()).RenderAsync(template, Data());

        var bytes = exporter.Export(rendered);
        Assert.NotEmpty(bytes);
        Assert.Equal((byte)'%', bytes[0]);
    }

    [Fact]
    public async Task Export_WithLongText_Multiline_ProducesValidPdf()
    {
        var template = BuildTemplate(band =>
            band.Elements.Add(new TextElement
            {
                Text = "这是一段很长的中文文本需要自动换行处理才能正确显示在报表上",
                X = 10, Y = 2, Width = 60, Height = 8,
                Font = new FontDef { Size = 10 },
                CanGrow = true
            }));

        var exporter = new PdfSharpExporter();
        var rendered = await new ReportRenderer(new NoopResolver()).RenderAsync(template, Data());

        var text = rendered.Pages.SelectMany(p => p.Elements).OfType<RenderedTextElement>().First();
        Assert.True(text.Lines.Count > 1, "应换行为多行");

        var bytes = exporter.Export(rendered);
        Assert.NotEmpty(bytes);
        Assert.Equal((byte)'%', bytes[0]);
    }

    [Fact]
    public async Task Export_WithRotatedAndLongText_ProducesValidPdf()
    {
        var template = BuildTemplate(band =>
        {
            band.Elements.Add(new TextElement
            {
                Text = "旋转的长文本内容需要换行处理", X = 10, Y = 2, Width = 50, Height = 10,
                Font = new FontDef { Size = 10 }, CanGrow = true, Rotation = 45
            });
        });

        var exporter = new PdfSharpExporter();
        var rendered = await new ReportRenderer(new NoopResolver()).RenderAsync(template, Data());

        var bytes = exporter.Export(rendered);
        Assert.NotEmpty(bytes);
        Assert.Equal((byte)'%', bytes[0]);
    }

    private sealed class NoopResolver : Core.SubReports.ITemplateResolver
    {
        public bool Exists(string templateRef) => false;
        public System.Threading.Tasks.Task<ReportTemplate> ResolveAsync(string templateRef)
            => throw new NotSupportedException();
    }
}
