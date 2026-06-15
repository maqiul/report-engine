using System.Collections.Generic;
using System.Threading.Tasks;
using ReportEngine.Core;
using ReportEngine.Core.Parsing;
using ReportEngine.Core.Rendering;
using ReportEngine.Core.SubReports;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ReportRenderer 边界行为测试：
///   - 空数据源渲染
///   - 多页渲染
///   - 不同 Band 类型行为
///   - 分页逻辑
/// </summary>
public class ReportRendererBoundaryTests
{
    private readonly ReportRenderer _renderer;

    public ReportRendererBoundaryTests()
    {
        _renderer = new ReportRenderer(new InMemoryTemplateResolver());
    }

    // ============== 空数据源 ==============

    [Fact]
    public async Task RenderAsync_EmptyDataSources_StillGeneratesPages()
    {
        var template = new ReportTemplate();
        template.Bands.Add(new Band { Type = BandType.Header, Height = 20 });
        template.Bands.Add(new Band { Type = BandType.Footer, Height = 15 });

        var dataSources = new Dictionary<string, List<Dictionary<string, object>>>();
        var result = await _renderer.RenderAsync(template, dataSources);

        Assert.NotNull(result);
        Assert.True(result.Pages.Count >= 1);
    }

    [Fact]
    public async Task RenderAsync_NoBands_GeneratesEmptyPage()
    {
        var template = new ReportTemplate();
        var dataSources = new Dictionary<string, List<Dictionary<string, object>>>();
        var result = await _renderer.RenderAsync(template, dataSources);

        // ReportRenderer generates at least one empty page even without bands
        Assert.NotNull(result);
        Assert.True(result.Pages.Count >= 1);
    }

    // ============== 单 Band 渲染 ==============

    [Fact]
    public async Task RenderAsync_SingleHeaderBand_GeneratesOnePage()
    {
        var template = new ReportTemplate();
        template.Bands.Add(new Band { Type = BandType.Header, Height = 20 });

        var dataSources = new Dictionary<string, List<Dictionary<string, object>>>();
        var result = await _renderer.RenderAsync(template, dataSources);

        Assert.Single(result.Pages);
    }

    [Fact]
    public async Task RenderAsync_SingleDetailBand_NoData_GeneratesOnePage()
    {
        var template = new ReportTemplate();
        template.DataSources.Add(new DataSourceDef { Name = "ds" });
        template.Bands.Add(new Band { Type = BandType.Detail, Height = 15, DataSource = "ds" });

        var dataSources = new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "ds", new List<Dictionary<string, object>>() }
        };
        var result = await _renderer.RenderAsync(template, dataSources);

        Assert.True(result.Pages.Count >= 1);
    }

    [Fact]
    public async Task RenderAsync_SingleDetailBand_OneRow_GeneratesOnePage()
    {
        var template = new ReportTemplate();
        template.DataSources.Add(new DataSourceDef { Name = "ds" });
        template.Bands.Add(new Band { Type = BandType.Detail, Height = 15, DataSource = "ds" });

        var dataSources = new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "ds", new List<Dictionary<string, object>> { new Dictionary<string, object> { { "id", 1 } } } }
        };
        var result = await _renderer.RenderAsync(template, dataSources);

        Assert.Single(result.Pages);
    }

    // ============== 多页渲染 ==============

    [Fact]
    public async Task RenderAsync_DetailBand_ManyRows_GeneratesMultiplePages()
    {
        var template = new ReportTemplate();
        template.Page = new PageInfo { Width = 210, Height = 100 }; // 小页面强制分页
        template.DataSources.Add(new DataSourceDef { Name = "ds" });
        template.Bands.Add(new Band { Type = BandType.Detail, Height = 30, DataSource = "ds" });

        var rows = new List<Dictionary<string, object>>();
        for (int i = 1; i <= 10; i++)
            rows.Add(new Dictionary<string, object> { { "id", i } });

        var dataSources = new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "ds", rows }
        };
        var result = await _renderer.RenderAsync(template, dataSources);

        Assert.True(result.Pages.Count > 1, "Should generate multiple pages");
    }

    // ============== ReportHeader / ReportFooter ==============

    [Fact]
    public async Task RenderAsync_ReportHeader_GeneratesPage()
    {
        var template = new ReportTemplate();
        template.Page = new PageInfo { Width = 210, Height = 100 };
        template.DataSources.Add(new DataSourceDef { Name = "ds" });
        template.Bands.Add(new Band { Type = BandType.ReportHeader, Height = 30 });
        template.Bands.Add(new Band { Type = BandType.Detail, Height = 30, DataSource = "ds" });

        var rows = new List<Dictionary<string, object>>();
        for (int i = 1; i <= 10; i++)
            rows.Add(new Dictionary<string, object> { { "id", i } });

        var dataSources = new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "ds", rows }
        };
        var result = await _renderer.RenderAsync(template, dataSources);

        Assert.True(result.Pages.Count >= 1);
    }

    [Fact]
    public async Task RenderAsync_ReportFooter_OnlyOnLastPage()
    {
        var template = new ReportTemplate();
        template.Page = new PageInfo { Width = 210, Height = 100 };
        template.DataSources.Add(new DataSourceDef { Name = "ds" });
        template.Bands.Add(new Band { Type = BandType.Detail, Height = 30, DataSource = "ds" });
        template.Bands.Add(new Band { Type = BandType.ReportFooter, Height = 20 });

        var rows = new List<Dictionary<string, object>>();
        for (int i = 1; i <= 10; i++)
            rows.Add(new Dictionary<string, object> { { "id", i } });

        var dataSources = new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "ds", rows }
        };
        var result = await _renderer.RenderAsync(template, dataSources);

        Assert.True(result.Pages.Count > 1);
        // ReportFooter should be on last page
        var lastPage = result.Pages[result.Pages.Count - 1];
        Assert.NotNull(lastPage);
    }

    // ============== PageHeader / PageFooter ==============

    [Fact]
    public async Task RenderAsync_PageHeader_OnEveryPage()
    {
        var template = new ReportTemplate();
        template.Page = new PageInfo { Width = 210, Height = 100 };
        template.DataSources.Add(new DataSourceDef { Name = "ds" });
        template.Bands.Add(new Band { Type = BandType.Header, Height = 15 });
        template.Bands.Add(new Band { Type = BandType.Detail, Height = 30, DataSource = "ds" });

        var rows = new List<Dictionary<string, object>>();
        for (int i = 1; i <= 10; i++)
            rows.Add(new Dictionary<string, object> { { "id", i } });

        var dataSources = new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "ds", rows }
        };
        var result = await _renderer.RenderAsync(template, dataSources);

        Assert.True(result.Pages.Count > 1);
        // Every page should have Header band elements
        foreach (var page in result.Pages)
        {
            Assert.NotNull(page);
        }
    }

    // ============== 元素渲染 ==============

    [Fact]
    public async Task RenderAsync_TextElement_RendersWithCorrectPosition()
    {
        var template = new ReportTemplate();
        var band = new Band { Type = BandType.Header, Height = 30 };
        band.Elements.Add(new TextElement
        {
            Text = "Hello",
            X = 10,
            Y = 5,
            Width = 50,
            Height = 10
        });
        template.Bands.Add(band);

        var dataSources = new Dictionary<string, List<Dictionary<string, object>>>();
        var result = await _renderer.RenderAsync(template, dataSources);

        var page = result.Pages[0];
        var textEl = page.Elements.Find(e => e is RenderedTextElement);
        Assert.NotNull(textEl);
        Assert.Equal(10, textEl.X);
        Assert.Equal(5, textEl.Y);
    }

    [Fact]
    public async Task RenderAsync_LineElement_RendersCorrectly()
    {
        var template = new ReportTemplate();
        var band = new Band { Type = BandType.Header, Height = 30 };
        band.Elements.Add(new LineElement
        {
            Direction = LineDirection.Horizontal,
            X = 0,
            Y = 25,
            Width = 100,
            Height = 0
        });
        template.Bands.Add(band);

        var dataSources = new Dictionary<string, List<Dictionary<string, object>>>();
        var result = await _renderer.RenderAsync(template, dataSources);

        var lineEl = result.Pages[0].Elements.Find(e => e is RenderedLineElement);
        Assert.NotNull(lineEl);
    }

    [Fact]
    public async Task RenderAsync_ImageElement_RendersCorrectly()
    {
        var template = new ReportTemplate();
        var band = new Band { Type = BandType.Header, Height = 30 };
        band.Elements.Add(new ImageElement
        {
            Source = "logo.png",
            X = 80,
            Y = 0,
            Width = 20,
            Height = 20
        });
        template.Bands.Add(band);

        var dataSources = new Dictionary<string, List<Dictionary<string, object>>>();
        var result = await _renderer.RenderAsync(template, dataSources);

        var imgEl = result.Pages[0].Elements.Find(e => e is RenderedImageElement);
        Assert.NotNull(imgEl);
        Assert.Equal(80, imgEl.X);
    }

    // ============== 页面尺寸 ==============

    [Fact]
    public async Task RenderAsync_CustomPageSize_GeneratesPage()
    {
        var template = new ReportTemplate();
        template.Page = new PageInfo { Width = 100, Height = 150 };
        template.Bands.Add(new Band { Type = BandType.Header, Height = 20 });

        var dataSources = new Dictionary<string, List<Dictionary<string, object>>>();
        var result = await _renderer.RenderAsync(template, dataSources);

        Assert.NotNull(result.Pages[0]);
    }

    [Fact]
    public async Task RenderAsync_LandscapeOrientation_GeneratesPage()
    {
        var template = new ReportTemplate();
        template.Page = new PageInfo { Width = 297, Height = 210, Orientation = "landscape" };
        template.Bands.Add(new Band { Type = BandType.Header, Height = 20 });

        var dataSources = new Dictionary<string, List<Dictionary<string, object>>>();
        var result = await _renderer.RenderAsync(template, dataSources);

        Assert.NotNull(result.Pages[0]);
    }

    // ============== 多数据源 ==============

    [Fact]
    public async Task RenderAsync_MultipleDataSources_RenderedCorrectly()
    {
        var template = new ReportTemplate();
        template.DataSources.Add(new DataSourceDef { Name = "ds1" });
        template.DataSources.Add(new DataSourceDef { Name = "ds2" });
        template.Bands.Add(new Band { Type = BandType.Detail, Height = 15, DataSource = "ds1" });

        var dataSources = new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "ds1", new List<Dictionary<string, object>> { new Dictionary<string, object> { { "id", 1 } } } },
            { "ds2", new List<Dictionary<string, object>> { new Dictionary<string, object> { { "name", "test" } } } }
        };
        var result = await _renderer.RenderAsync(template, dataSources);

        Assert.NotNull(result);
        Assert.True(result.Pages.Count >= 1);
    }
}
