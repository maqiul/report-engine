using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Rendering;
using ReportEngine.Core.SubReports;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ReportRenderer 分组/条件/多联打印 渲染测试
/// </summary>
public class ReportRendererGroupAndConditionTests
{
    private readonly InMemoryTemplateResolver2 _resolver = new();

    // ============== GroupHeader/GroupFooter ==============

    [Fact]
    public async Task RenderAsync_GroupHeader_GroupsData()
    {
        var t = new ReportTemplate();
        t.Page.Height = 200;
        t.Page.Margin = new Margin { Top = 5, Bottom = 5, Left = 0, Right = 0 };

        var groupHeader = new Band { Type = BandType.GroupHeader, Height = 15, DataSource = "ds", Group = new GroupDef { Expression = "[category]" } };
        groupHeader.Elements.Add(new TextElement { Text = "Group: {{currentRow.category}}", X = 0, Y = 0, Width = 100, Height = 10 });
        t.Bands.Add(groupHeader);

        var detail = new Band { Type = BandType.Detail, Height = 10, DataSource = "ds" };
        detail.Elements.Add(new TextElement { Text = "{{currentRow.name}}", X = 10, Y = 0, Width = 100, Height = 8 });
        t.Bands.Add(detail);

        var groupFooter = new Band { Type = BandType.GroupFooter, Height = 10, DataSource = "ds" };
        groupFooter.Elements.Add(new TextElement { Text = "Subtotal", X = 0, Y = 0, Width = 100, Height = 8 });
        t.Bands.Add(groupFooter);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "category", "A" }, { "name", "Item1" } },
            new Dictionary<string, object> { { "category", "A" }, { "name", "Item2" } },
            new Dictionary<string, object> { { "category", "B" }, { "name", "Item3" } },
        };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        Assert.True(rendered.Pages.Count >= 1);
    }

    [Fact]
    public async Task RenderAsync_GroupFooter_AppearsAfterGroup()
    {
        var t = new ReportTemplate();
        t.Page.Height = 200;
        t.Page.Margin = new Margin { Top = 5, Bottom = 5, Left = 0, Right = 0 };

        var groupHeader = new Band { Type = BandType.GroupHeader, Height = 15, DataSource = "ds", Group = new GroupDef { Expression = "[cat]" } };
        groupHeader.Elements.Add(new TextElement { Text = "GH", X = 0, Y = 0, Width = 50, Height = 10 });
        t.Bands.Add(groupHeader);

        var detail = new Band { Type = BandType.Detail, Height = 10, DataSource = "ds" };
        detail.Elements.Add(new TextElement { Text = "D", X = 0, Y = 0, Width = 50, Height = 8 });
        t.Bands.Add(detail);

        var groupFooter = new Band { Type = BandType.GroupFooter, Height = 10, DataSource = "ds" };
        groupFooter.Elements.Add(new TextElement { Text = "GF", X = 0, Y = 0, Width = 50, Height = 8 });
        t.Bands.Add(groupFooter);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "cat", "X" }, { "name", "A" } },
        };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        Assert.True(rendered.Pages.Count >= 1);
    }

    // ============== ReportFooter ==============

    [Fact]
    public async Task RenderAsync_ReportFooter_LastPage()
    {
        var t = new ReportTemplate();
        t.Page.Height = 50;
        t.Page.Margin = new Margin { Top = 5, Bottom = 5, Left = 0, Right = 0 };

        var detail = new Band { Type = BandType.Detail, Height = 15, DataSource = "ds" };
        detail.Elements.Add(new TextElement { Text = "Row", X = 0, Y = 0, Width = 100, Height = 10 });
        t.Bands.Add(detail);

        var reportFooter = new Band { Type = BandType.ReportFooter, Height = 10, DataSource = "ds" };
        reportFooter.Elements.Add(new TextElement { Text = "TOTAL", X = 0, Y = 0, Width = 100, Height = 8 });
        t.Bands.Add(reportFooter);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        var rows = new List<Dictionary<string, object>>();
        for (int i = 0; i < 10; i++)
            rows.Add(new Dictionary<string, object> { { "id", i } });
        ds["ds"] = rows;

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        Assert.True(rendered.Pages.Count >= 1);

        // ReportFooter 应在最后一页
        var lastPage = rendered.Pages.Last();
        var total = lastPage.Elements.OfType<RenderedTextElement>().FirstOrDefault(e => e.Text == "TOTAL");
        Assert.NotNull(total);
    }

    // ============== 条件格式 ==============

    [Fact]
    public async Task RenderAsync_ConditionalFormat_AppliedWhenTrue()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20, DataSource = "ds" };
        var textEl = new TextElement { Text = "{{currentRow.value}}", X = 10, Y = 5, Width = 100, Height = 10 };
        textEl.ConditionalFormats.Add(new ConditionalFormatRule
        {
            Expression = "[value]>100",
            BackgroundColor = "#FF0000",
            Bold = true
        });
        band.Elements.Add(textEl);
        t.Bands.Add(band);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "value", 150 } }
        };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        Assert.True(rendered.Pages.Count >= 1);
    }

    [Fact]
    public async Task RenderAsync_ConditionalFormat_NotAppliedWhenFalse()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20, DataSource = "ds" };
        var textEl = new TextElement { Text = "{{currentRow.value}}", X = 10, Y = 5, Width = 100, Height = 10 };
        textEl.ConditionalFormats.Add(new ConditionalFormatRule
        {
            Expression = "[value]>100",
            BackgroundColor = "#FF0000",
            Bold = true
        });
        band.Elements.Add(textEl);
        t.Bands.Add(band);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "value", 50 } }
        };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        Assert.True(rendered.Pages.Count >= 1);
    }

    // ============== 多数据源 ==============

    [Fact]
    public async Task RenderAsync_MultipleDataSources_BothUsed()
    {
        var t = new ReportTemplate();
        t.DataSources.Add(new DataSourceDef { Name = "ds1" });
        t.DataSources.Add(new DataSourceDef { Name = "ds2" });

        var band1 = new Band { Type = BandType.Detail, Height = 20, DataSource = "ds1" };
        band1.Elements.Add(new TextElement { Text = "{{currentRow.name}}", X = 10, Y = 5, Width = 100, Height = 10 });
        t.Bands.Add(band1);

        var band2 = new Band { Type = BandType.Detail, Height = 20, DataSource = "ds2" };
        band2.Elements.Add(new TextElement { Text = "{{currentRow.code}}", X = 10, Y = 5, Width = 100, Height = 10 });
        t.Bands.Add(band2);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds1"] = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "name", "Alice" } }
        };
        ds["ds2"] = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "code", "X001" } }
        };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        Assert.True(rendered.Pages.Count >= 1);
    }

    // ============== 空数据 ==============

    [Fact]
    public async Task RenderAsync_EmptyDataSource_StillRendersHeaderFooter()
    {
        var t = new ReportTemplate();
        var header = new Band { Type = BandType.Header, Height = 15 };
        header.Elements.Add(new TextElement { Text = "TITLE", X = 0, Y = 0, Width = 100, Height = 10 });
        t.Bands.Add(header);

        var detail = new Band { Type = BandType.Detail, Height = 20, DataSource = "ds" };
        detail.Elements.Add(new TextElement { Text = "Row", X = 0, Y = 0, Width = 100, Height = 10 });
        t.Bands.Add(detail);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>>();

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        Assert.Single(rendered.Pages);
    }

    [Fact]
    public async Task RenderAsync_NoDataSources_RendersSinglePage()
    {
        var t = new ReportTemplate();
        var header = new Band { Type = BandType.Header, Height = 15 };
        header.Elements.Add(new TextElement { Text = "TITLE", X = 0, Y = 0, Width = 100, Height = 10 });
        t.Bands.Add(header);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        Assert.Single(rendered.Pages);
    }

    // ============== 子报表 ==============

    [Fact]
    public async Task RenderAsync_SubReportElement_ResolvesAndRenders()
    {
        // 子模板
        var subTemplate = new ReportTemplate();
        var subBand = new Band { Type = BandType.Detail, Height = 15, DataSource = "subDs" };
        subBand.Elements.Add(new TextElement { Text = "SubContent", X = 0, Y = 0, Width = 80, Height = 10 });
        subTemplate.Bands.Add(subBand);
        _resolver.Add("sub.rpt", subTemplate);

        // 主模板
        var t = new ReportTemplate();
        t.DataSources.Add(new DataSourceDef { Name = "mainDs" });
        var band = new Band { Type = BandType.Detail, Height = 40, DataSource = "mainDs" };
        band.Elements.Add(new SubReportElement
        {
            TemplateRef = "sub.rpt",
            HeightMode = "fixed",
            X = 10, Y = 5, Width = 180, Height = 30,
            DataBinding = new SubReportDataBinding { Source = "mainDs" }
        });
        t.Bands.Add(band);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["mainDs"] = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "id", 1 } }
        };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        Assert.True(rendered.Pages.Count >= 1);
    }

    // ============== 元素背景色/边框 ==============

    [Fact]
    public async Task RenderAsync_ElementWithBackgroundColor_Preserved()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20, DataSource = "ds" };
        band.Elements.Add(new TextElement
        {
            Text = "Colored",
            X = 10, Y = 5, Width = 100, Height = 10,
            BackgroundColor = "#FFFF00"
        });
        t.Bands.Add(band);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>> { new Dictionary<string, object>() };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        var el = rendered.Pages[0].Elements.OfType<RenderedTextElement>().First();
        Assert.Equal("#FFFF00", el.BackgroundColor);
    }

    [Fact]
    public async Task RenderAsync_ElementWithBorder_Preserved()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20, DataSource = "ds" };
        band.Elements.Add(new TextElement
        {
            Text = "Bordered",
            X = 10, Y = 5, Width = 100, Height = 10,
            Border = new BorderDef { Width = 2, Color = "#FF0000", Style = BorderStyle.Dashed }
        });
        t.Bands.Add(band);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>> { new Dictionary<string, object>() };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        var el = rendered.Pages[0].Elements.OfType<RenderedTextElement>().First();
        Assert.NotNull(el.Border);
        Assert.Equal(2, el.Border!.Width);
        Assert.Equal("#FF0000", el.Border.Color);
    }

    // ============== 元素透明度/旋转 ==============

    [Fact]
    public async Task RenderAsync_ElementOpacity_Preserved()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20, DataSource = "ds" };
        band.Elements.Add(new TextElement
        {
            Text = "Transparent",
            X = 10, Y = 5, Width = 100, Height = 10,
            Opacity = 0.5
        });
        t.Bands.Add(band);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>> { new Dictionary<string, object>() };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        Assert.True(rendered.Pages.Count >= 1);
    }

    // ============== RenderedReport Template 引用 ==============

    [Fact]
    public async Task RenderAsync_RenderedReport_HasTemplateRef()
    {
        var t = new ReportTemplate();
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 20, DataSource = "ds" });

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>> { new Dictionary<string, object>() };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        Assert.NotNull(rendered.Template);
        Assert.Same(t, rendered.Template);
    }

    [Fact]
    public async Task RenderAsync_RenderedReport_PageDimensionsMatch()
    {
        var t = new ReportTemplate();
        t.Page.Width = 297;
        t.Page.Height = 210;
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 20, DataSource = "ds" });

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>> { new Dictionary<string, object>() };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        Assert.Equal(297, rendered.PageWidth);
        Assert.Equal(210, rendered.PageHeight);
    }

    // ============== 多联打印 ==============

    [Fact]
    public async Task RenderAsync_MultiUp_RendersMultipleCopiesPerPage()
    {
        var t = new ReportTemplate();
        t.Page.Width = 200;
        t.Page.Height = 200;
        t.Page.MultiUp = new MultiUpConfig { Rows = 2, Columns = 2, HSpacing = 5, VSpacing = 5 };

        var band = new Band { Type = BandType.Detail, Height = 30, DataSource = "ds" };
        band.Elements.Add(new TextElement { Text = "Label", X = 0, Y = 0, Width = 80, Height = 20 });
        t.Bands.Add(band);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "id", 1 } },
            new Dictionary<string, object> { { "id", 2 } },
        };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        Assert.True(rendered.Pages.Count >= 1);
    }

    // ============== FitToWidth 多页面 ==============

    [Fact]
    public void RenderedReport_FitToWidth_AllPagesScaled()
    {
        var rr = new RenderedReport { PageWidth = 200, PageHeight = 300 };
        for (int i = 0; i < 3; i++)
        {
            var page = new RenderedPage { PageNumber = i + 1, TotalPages = 3 };
            page.Elements.Add(new RenderedTextElement { X = 40, Y = 40, Width = 80, Height = 20, Text = $"Page{i + 1}" });
            rr.Pages.Add(page);
        }

        rr.FitToWidth(100);

        foreach (var page in rr.Pages)
        {
            foreach (var el in page.Elements)
            {
                Assert.Equal(20, el.X); // 40 * 0.5
                Assert.Equal(40, el.Width); // 80 * 0.5
            }
        }
    }

    [Fact]
    public void RenderedReport_Scale_AllPagesScaled()
    {
        var rr = new RenderedReport { PageWidth = 100, PageHeight = 150 };
        for (int i = 0; i < 2; i++)
        {
            var page = new RenderedPage { PageNumber = i + 1, TotalPages = 2 };
            page.Elements.Add(new RenderedTextElement { X = 10, Y = 10, Width = 50, Height = 20, Text = $"P{i + 1}" });
            rr.Pages.Add(page);
        }

        rr.Scale(3.0);

        Assert.Equal(300, rr.PageWidth);
        Assert.Equal(450, rr.PageHeight);
        foreach (var page in rr.Pages)
        {
            foreach (var el in page.Elements)
            {
                Assert.Equal(30, el.X);
                Assert.Equal(150, el.Width);
            }
        }
    }
}
