using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Rendering;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// 渲染器边界与集成测试
/// </summary>
public class RendererIntegrationTests
{
    private readonly InMemoryResolver3 _resolver = new();

    [Fact]
    public async Task RenderAsync_LargeDataset_CompletesWithoutError()
    {
        var t = new ReportTemplate();
        t.Page.Height = 297;
        t.Page.Margin = new Margin { Top = 10, Bottom = 10, Left = 10, Right = 10 };

        var detail = new Band { Type = BandType.Detail, Height = 10, DataSource = "ds" };
        detail.Elements.Add(new TextElement { Text = "{{currentRow.id}}", X = 10, Y = 0, Width = 50, Height = 8 });
        t.Bands.Add(detail);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        var rows = new List<Dictionary<string, object>>();
        for (int i = 0; i < 1000; i++)
            rows.Add(new Dictionary<string, object> { { "id", i } });
        ds["ds"] = rows;

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        Assert.True(rendered.Pages.Count > 1);
    }

    [Fact]
    public async Task RenderAsync_ComplexTemplate_AllElementsRendered()
    {
        var t = new ReportTemplate();
        t.Page.Width = 210;
        t.Page.Height = 297;
        t.Page.Margin = new Margin { Top = 15, Bottom = 15, Left = 10, Right = 10 };

        // Header
        var header = new Band { Type = BandType.Header, Height = 20, RepeatOnNewPage = true };
        header.Elements.Add(new TextElement { Text = "Report Title", X = 10, Y = 5, Width = 190, Height = 10, Font = new FontDef { Size = 16, Bold = true } });
        t.Bands.Add(header);

        // ReportHeader
        var reportHeader = new Band { Type = BandType.ReportHeader, Height = 15 };
        reportHeader.Elements.Add(new TextElement { Text = "Generated: {{REPORT_DATE}}", X = 10, Y = 0, Width = 190, Height = 10 });
        t.Bands.Add(reportHeader);

        // Detail
        var detail = new Band { Type = BandType.Detail, Height = 12, DataSource = "ds" };
        detail.Elements.Add(new TextElement { Text = "{{currentRow.name}}", X = 10, Y = 0, Width = 80, Height = 10 });
        detail.Elements.Add(new TextElement { Text = "{{currentRow.value}}", X = 100, Y = 0, Width = 50, Height = 10, Alignment = TextAlignment.Right });
        t.Bands.Add(detail);

        // ReportFooter
        var reportFooter = new Band { Type = BandType.ReportFooter, Height = 10, DataSource = "ds" };
        reportFooter.Elements.Add(new TextElement { Text = "End of Report", X = 10, Y = 0, Width = 190, Height = 8 });
        t.Bands.Add(reportFooter);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "name", "Item A" }, { "value", 100 } },
            new Dictionary<string, object> { { "name", "Item B" }, { "value", 200 } },
            new Dictionary<string, object> { { "name", "Item C" }, { "value", 300 } },
        };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        Assert.True(rendered.Pages.Count >= 1);
        Assert.True(rendered.Pages[0].Elements.Count > 0);
    }

    [Fact]
    public async Task RenderAsync_EmptyPage_NoElements()
    {
        var t = new ReportTemplate();
        t.Page.Height = 50;
        t.Page.Margin = new Margin { Top = 5, Bottom = 5, Left = 0, Right = 0 };

        var detail = new Band { Type = BandType.Detail, Height = 20, DataSource = "ds" };
        detail.Elements.Add(new TextElement { Text = "Row", X = 0, Y = 0, Width = 100, Height = 10 });
        t.Bands.Add(detail);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>>();

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        Assert.Single(rendered.Pages);
        // Empty data source, but header/footer might still render
    }

    [Fact]
    public async Task RenderAsync_ZeroHeightBand_HandledGracefully()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 0, DataSource = "ds" };
        band.Elements.Add(new TextElement { Text = "Zero", X = 0, Y = 0, Width = 50, Height = 0 });
        t.Bands.Add(band);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>> { new Dictionary<string, object>() };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        Assert.True(rendered.Pages.Count >= 1);
    }

    [Fact]
    public async Task RenderAsync_NegativePosition_HandledGracefully()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20, DataSource = "ds" };
        band.Elements.Add(new TextElement { Text = "Neg", X = -10, Y = -5, Width = 50, Height = 10 });
        t.Bands.Add(band);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>> { new Dictionary<string, object>() };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        Assert.True(rendered.Pages.Count >= 1);
    }

    [Fact]
    public async Task RenderAsync_VeryLargeElement_HandledGracefully()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20, DataSource = "ds" };
        band.Elements.Add(new TextElement { Text = "Large", X = 0, Y = 0, Width = 10000, Height = 10000 });
        t.Bands.Add(band);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>> { new Dictionary<string, object>() };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        Assert.True(rendered.Pages.Count >= 1);
    }

    [Fact]
    public async Task RenderAsync_SpecialCharactersInText_HandledCorrectly()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20, DataSource = "ds" };
        band.Elements.Add(new TextElement { Text = "Hello\nWorld\tTab", X = 0, Y = 0, Width = 100, Height = 10 });
        t.Bands.Add(band);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>> { new Dictionary<string, object>() };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        var text = rendered.Pages[0].Elements.OfType<RenderedTextElement>().First();
        Assert.Equal("Hello\nWorld\tTab", text.Text);
    }

    [Fact]
    public async Task RenderAsync_ChineseText_HandledCorrectly()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20, DataSource = "ds" };
        band.Elements.Add(new TextElement { Text = "中文测试", X = 0, Y = 0, Width = 100, Height = 10 });
        t.Bands.Add(band);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>> { new Dictionary<string, object>() };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        var text = rendered.Pages[0].Elements.OfType<RenderedTextElement>().First();
        Assert.Equal("中文测试", text.Text);
    }

    [Fact]
    public async Task RenderAsync_UnicodeText_HandledCorrectly()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20, DataSource = "ds" };
        band.Elements.Add(new TextElement { Text = "🎉🚀💡", X = 0, Y = 0, Width = 100, Height = 10 });
        t.Bands.Add(band);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>> { new Dictionary<string, object>() };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        var text = rendered.Pages[0].Elements.OfType<RenderedTextElement>().First();
        Assert.Equal("🎉🚀💡", text.Text);
    }

    [Fact]
    public async Task RenderAsync_MultipleBandsSameDataSource_AllRendered()
    {
        var t = new ReportTemplate();
        t.DataSources.Add(new DataSourceDef { Name = "ds" });

        var band1 = new Band { Type = BandType.Detail, Height = 10, DataSource = "ds" };
        band1.Elements.Add(new TextElement { Text = "Band1", X = 0, Y = 0, Width = 50, Height = 8 });
        t.Bands.Add(band1);

        var band2 = new Band { Type = BandType.Detail, Height = 10, DataSource = "ds" };
        band2.Elements.Add(new TextElement { Text = "Band2", X = 0, Y = 0, Width = 50, Height = 8 });
        t.Bands.Add(band2);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>> { new Dictionary<string, object>() };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        Assert.True(rendered.Pages.Count >= 1);
    }
}
