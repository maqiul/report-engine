using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Rendering;
using ReportEngine.Core.SubReports;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// 内存模板解析器（测试用）
/// </summary>
internal class InMemoryTemplateResolver2 : ITemplateResolver
{
    private readonly Dictionary<string, ReportTemplate> _templates = new();

    public void Add(string key, ReportTemplate template) => _templates[key] = template;

    public Task<ReportTemplate> ResolveAsync(string templateRef)
    {
        if (_templates.TryGetValue(templateRef, out var t))
            return Task.FromResult(t);
        throw new Exception($"Template not found: {templateRef}");
    }

    public bool Exists(string templateRef) => _templates.ContainsKey(templateRef);
}

/// <summary>
/// ReportRenderer 更多渲染行为测试
/// </summary>
public class ReportRendererMoreTests
{
    private readonly InMemoryTemplateResolver2 _resolver = new();

    // ============== 基础渲染 ==============

    [Fact]
    public async Task RenderAsync_EmptyDetailBand_ReturnsOnePage()
    {
        var t = new ReportTemplate();
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 20, DataSource = "ds" });

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>>();

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        Assert.Single(rendered.Pages);
    }

    [Fact]
    public async Task RenderAsync_SingleRow_ReturnsOnePage()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20, DataSource = "ds" };
        band.Elements.Add(new TextElement { Text = "Hello", X = 10, Y = 5, Width = 100, Height = 10 });
        t.Bands.Add(band);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "name", "Alice" } }
        };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        Assert.Single(rendered.Pages);
        Assert.True(rendered.Pages[0].Elements.Count > 0);
    }

    [Fact]
    public async Task RenderAsync_MultipleRows_AllRendered()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20, DataSource = "ds" };
        band.Elements.Add(new TextElement { Text = "{{currentRow.name}}", X = 10, Y = 5, Width = 100, Height = 10 });
        t.Bands.Add(band);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "name", "Alice" } },
            new Dictionary<string, object> { { "name", "Bob" } },
            new Dictionary<string, object> { { "name", "Charlie" } }
        };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        Assert.True(rendered.Pages.Count >= 1);
        // 至少 3 个文本元素
        var textElements = rendered.Pages.SelectMany(p => p.Elements).OfType<RenderedTextElement>().ToList();
        Assert.True(textElements.Count >= 3);
    }

    // ============== Header/Footer ==============

    [Fact]
    public async Task RenderAsync_HeaderBand_RenderedOnEveryPage()
    {
        var t = new ReportTemplate();
        t.Page.Height = 50;
        t.Page.Margin = new Margin { Top = 5, Bottom = 5, Left = 0, Right = 0 };

        var header = new Band { Type = BandType.Header, Height = 10 };
        header.Elements.Add(new TextElement { Text = "HEADER", X = 0, Y = 0, Width = 100, Height = 8 });
        t.Bands.Add(header);

        var detail = new Band { Type = BandType.Detail, Height = 20, DataSource = "ds" };
        detail.Elements.Add(new TextElement { Text = "Row", X = 0, Y = 0, Width = 100, Height = 10 });
        t.Bands.Add(detail);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "id", 1 } },
            new Dictionary<string, object> { { "id", 2 } }
        };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        Assert.True(rendered.Pages.Count >= 1);
    }

    [Fact]
    public async Task RenderAsync_ReportHeader_OnlyFirstPage()
    {
        var t = new ReportTemplate();
        t.Page.Height = 200;
        t.Page.Margin = new Margin { Top = 5, Bottom = 5, Left = 0, Right = 0 };

        var reportHeader = new Band { Type = BandType.ReportHeader, Height = 30 };
        reportHeader.Elements.Add(new TextElement { Text = "REPORT TITLE", X = 0, Y = 0, Width = 200, Height = 20 });
        t.Bands.Add(reportHeader);

        var detail = new Band { Type = BandType.Detail, Height = 10, DataSource = "ds" };
        detail.Elements.Add(new TextElement { Text = "Row", X = 0, Y = 0, Width = 100, Height = 8 });
        t.Bands.Add(detail);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "id", 1 } }
        };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);

        // ReportHeader 应该只在第一页
        var titleElements = rendered.Pages[0].Elements.OfType<RenderedTextElement>()
            .Where(e => e.Text == "REPORT TITLE").ToList();
        Assert.Single(titleElements);
    }

    // ============== 分页 ==============

    [Fact]
    public async Task RenderAsync_ManyRows_Paginates()
    {
        var t = new ReportTemplate();
        t.Page.Height = 50;
        t.Page.Margin = new Margin { Top = 5, Bottom = 5, Left = 0, Right = 0 };

        var detail = new Band { Type = BandType.Detail, Height = 15, DataSource = "ds" };
        detail.Elements.Add(new TextElement { Text = "Row", X = 0, Y = 0, Width = 100, Height = 10 });
        t.Bands.Add(detail);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        var rows = new List<Dictionary<string, object>>();
        for (int i = 0; i < 20; i++)
            rows.Add(new Dictionary<string, object> { { "id", i } });
        ds["ds"] = rows;

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        Assert.True(rendered.Pages.Count > 1);
    }

    [Fact]
    public async Task RenderAsync_PageNumbers_AreCorrect()
    {
        var t = new ReportTemplate();
        t.Page.Height = 50;
        t.Page.Margin = new Margin { Top = 5, Bottom = 5, Left = 0, Right = 0 };

        var detail = new Band { Type = BandType.Detail, Height = 15, DataSource = "ds" };
        detail.Elements.Add(new TextElement { Text = "Row", X = 0, Y = 0, Width = 100, Height = 10 });
        t.Bands.Add(detail);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        var rows = new List<Dictionary<string, object>>();
        for (int i = 0; i < 20; i++)
            rows.Add(new Dictionary<string, object> { { "id", i } });
        ds["ds"] = rows;

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);

        for (int i = 0; i < rendered.Pages.Count; i++)
        {
            Assert.Equal(i + 1, rendered.Pages[i].PageNumber);
            Assert.Equal(rendered.Pages.Count, rendered.Pages[i].TotalPages);
        }
    }

    // ============== 元素类型 ==============

    [Fact]
    public async Task RenderAsync_ImageElement_Rendered()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 30, DataSource = "ds" };
        band.Elements.Add(new ImageElement { Source = "logo.png", X = 10, Y = 5, Width = 50, Height = 20 });
        t.Bands.Add(band);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>> { new Dictionary<string, object>() };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        var img = rendered.Pages[0].Elements.OfType<RenderedImageElement>().FirstOrDefault();
        Assert.NotNull(img);
        Assert.Equal("logo.png", img!.Source);
    }

    [Fact]
    public async Task RenderAsync_LineElement_Rendered()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 30, DataSource = "ds" };
        band.Elements.Add(new LineElement { Direction = LineDirection.Horizontal, LineWidth = 2, LineColor = "#FF0000", X = 0, Y = 10, Width = 200, Height = 0 });
        t.Bands.Add(band);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>> { new Dictionary<string, object>() };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        var line = rendered.Pages[0].Elements.OfType<RenderedLineElement>().FirstOrDefault();
        Assert.NotNull(line);
        Assert.Equal(LineDirection.Horizontal, line!.Direction);
        Assert.Equal(2, line.LineWidth);
    }

    [Fact]
    public async Task RenderAsync_ShapeElement_Rendered()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 30, DataSource = "ds" };
        band.Elements.Add(new ShapeElement { Shape = ShapeType.Rectangle, FillColor = "#00FF00", BorderRadius = 5, X = 10, Y = 5, Width = 40, Height = 20 });
        t.Bands.Add(band);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>> { new Dictionary<string, object>() };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        var shape = rendered.Pages[0].Elements.OfType<RenderedShapeElement>().FirstOrDefault();
        Assert.NotNull(shape);
        Assert.Equal(ShapeType.Rectangle, shape!.Shape);
        Assert.Equal("#00FF00", shape.FillColor);
    }

    [Fact]
    public async Task RenderAsync_BarcodeElement_Rendered()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 30, DataSource = "ds" };
        band.Elements.Add(new BarcodeElement { Value = "12345", Format = BarcodeFormat.Code128, X = 10, Y = 5, Width = 80, Height = 20 });
        t.Bands.Add(band);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>> { new Dictionary<string, object>() };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        var bc = rendered.Pages[0].Elements.OfType<RenderedBarcodeElement>().FirstOrDefault();
        Assert.NotNull(bc);
        Assert.Equal("12345", bc!.Value);
        Assert.Equal(BarcodeFormat.Code128, bc.Format);
    }

    // ============== VisibleExpression ==============

    [Fact]
    public async Task RenderAsync_VisibleExpression_True_ElementShown()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 30, DataSource = "ds" };
        band.Elements.Add(new TextElement
        {
            Text = "Visible",
            VisibleExpression = "true",
            X = 10, Y = 5, Width = 100, Height = 10
        });
        t.Bands.Add(band);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>> { new Dictionary<string, object>() };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        var text = rendered.Pages[0].Elements.OfType<RenderedTextElement>().FirstOrDefault(e => e.Text == "Visible");
        Assert.NotNull(text);
    }

    [Fact]
    public async Task RenderAsync_VisibleExpression_False_ElementHidden()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 30, DataSource = "ds" };
        band.Elements.Add(new TextElement
        {
            Text = "Hidden",
            VisibleExpression = "false",
            X = 10, Y = 5, Width = 100, Height = 10
        });
        t.Bands.Add(band);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>> { new Dictionary<string, object>() };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        var text = rendered.Pages[0].Elements.OfType<RenderedTextElement>().FirstOrDefault(e => e.Text == "Hidden");
        Assert.Null(text);
    }

    // ============== 表达式替换 ==============

    [Fact]
    public async Task RenderAsync_TextWithFieldExpression_Replaced()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20, DataSource = "ds" };
        band.Elements.Add(new TextElement { Text = "{{currentRow.name}}", X = 10, Y = 5, Width = 100, Height = 10 });
        t.Bands.Add(band);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "name", "Alice" } }
        };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        var text = rendered.Pages[0].Elements.OfType<RenderedTextElement>().First();
        Assert.Equal("Alice", text.Text);
    }

    [Fact]
    public async Task RenderAsync_TextWithSysVar_Replaced()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20, DataSource = "ds" };
        // {{currentPage}} 不是已知系统变量，返回去括号后的变量名
        band.Elements.Add(new TextElement { Text = "{{currentPage}}", X = 10, Y = 5, Width = 100, Height = 10 });
        t.Bands.Add(band);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "id", 1 } }
        };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        var text = rendered.Pages[0].Elements.OfType<RenderedTextElement>().First();
        Assert.Equal("currentPage", text.Text);
    }

    // ============== Table 渲染 ==============

    [Fact]
    public async Task RenderAsync_TableElement_Rendered()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 50, DataSource = "ds" };
        var table = new TableElement
        {
            RowCount = 2,
            ColCount = 2,
            BorderWidth = 1,
            BorderColor = "#000000",
            X = 10, Y = 5, Width = 180, Height = 40
        };
        table.Cells.Add(new TableCell { Row = 0, Col = 0, Text = "A" });
        table.Cells.Add(new TableCell { Row = 0, Col = 1, Text = "B" });
        table.Cells.Add(new TableCell { Row = 1, Col = 0, Text = "C" });
        table.Cells.Add(new TableCell { Row = 1, Col = 1, Text = "D" });
        band.Elements.Add(table);
        t.Bands.Add(band);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>> { new Dictionary<string, object>() };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        var tbl = rendered.Pages[0].Elements.OfType<RenderedTableElement>().FirstOrDefault();
        Assert.NotNull(tbl);
        Assert.Equal(2, tbl!.RowCount);
        Assert.Equal(2, tbl.ColCount);
        Assert.Equal(4, tbl.Cells.Count);
    }

    // ============== CrossTab 渲染 ==============

    [Fact]
    public async Task RenderAsync_CrossTabElement_Rendered()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 80, DataSource = "ds" };
        var ct = new CrossTabElement
        {
            DataSource = "sales",
            X = 10, Y = 5, Width = 180, Height = 70,
            ShowRowTotal = true,
            ShowColumnTotal = true
        };
        ct.RowFields.Add("Region");
        ct.ColumnFields.Add("Year");
        ct.Measures.Add(new CrossTabMeasure { Field = "Amount", Aggregate = "Sum" });
        band.Elements.Add(ct);
        t.Bands.Add(band);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>> { new Dictionary<string, object>() };
        ds["sales"] = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "Region", "North" }, { "Year", "2024" }, { "Amount", 100.0 } },
            new Dictionary<string, object> { { "Region", "North" }, { "Year", "2025" }, { "Amount", 150.0 } },
            new Dictionary<string, object> { { "Region", "South" }, { "Year", "2024" }, { "Amount", 200.0 } },
        };

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        var ctab = rendered.Pages[0].Elements.OfType<RenderedCrossTabElement>().FirstOrDefault();
        Assert.NotNull(ctab);
        Assert.True(ctab!.RowCount > 0);
        Assert.True(ctab.ColCount > 0);
        Assert.True(ctab.Cells.Count > 0);
    }

    // ============== 多栏渲染 ==============

    [Fact]
    public async Task RenderAsync_MultiColumn_RendersMultipleColumns()
    {
        var t = new ReportTemplate();
        t.Page.Width = 200;
        t.Page.Height = 200;
        t.Page.Margin = new Margin { Top = 10, Bottom = 10, Left = 10, Right = 10 };

        var detail = new Band { Type = BandType.Detail, Height = 15, DataSource = "ds" };
        detail.MultiColumn = new MultiColumnConfig { ColumnCount = 2, ColumnSpacing = 5, Direction = "Horizontal" };
        detail.Elements.Add(new TextElement { Text = "Item", X = 0, Y = 0, Width = 80, Height = 10 });
        t.Bands.Add(detail);

        var ds = new Dictionary<string, List<Dictionary<string, object>>>();
        ds["ds"] = new List<Dictionary<string, object>>();
        for (int i = 0; i < 6; i++)
            ds["ds"].Add(new Dictionary<string, object> { { "id", i } });

        var renderer = new ReportRenderer(_resolver);
        var rendered = await renderer.RenderAsync(t, ds);
        Assert.True(rendered.Pages.Count >= 1);
    }

    // ============== RenderedReport 缩放 ==============

    [Fact]
    public void RenderedReport_FitToWidth_ScalesDown()
    {
        var rr = new RenderedReport { PageWidth = 200, PageHeight = 300 };
        rr.Pages.Add(new RenderedPage());
        rr.Pages[0].Elements.Add(new RenderedTextElement { X = 50, Y = 50, Width = 100, Height = 20, Text = "Test" });

        rr.FitToWidth(100);

        Assert.Equal(100, rr.PageWidth);
        Assert.Equal(150, rr.PageHeight); // 300 * 0.5
        Assert.Equal(25, rr.Pages[0].Elements[0].X); // 50 * 0.5
        Assert.Equal(50, rr.Pages[0].Elements[0].Width); // 100 * 0.5
    }

    [Fact]
    public void RenderedReport_FitToWidth_NoScaleIfAlreadyFits()
    {
        var rr = new RenderedReport { PageWidth = 100, PageHeight = 150 };
        rr.Pages.Add(new RenderedPage());
        rr.Pages[0].Elements.Add(new RenderedTextElement { X = 10, Y = 10, Width = 50, Height = 20, Text = "Test" });

        rr.FitToWidth(200); // 目标比当前宽

        Assert.Equal(100, rr.PageWidth); // 不变
        Assert.Equal(150, rr.PageHeight); // 不变
    }

    [Fact]
    public void RenderedReport_FitToWidth_ZeroWidth_NoOp()
    {
        var rr = new RenderedReport { PageWidth = 200, PageHeight = 300 };
        rr.FitToWidth(0);
        Assert.Equal(200, rr.PageWidth);
    }

    [Fact]
    public void RenderedReport_FitToWidth_NegativeWidth_NoOp()
    {
        var rr = new RenderedReport { PageWidth = 200, PageHeight = 300 };
        rr.FitToWidth(-50);
        Assert.Equal(200, rr.PageWidth);
    }

    [Fact]
    public void RenderedReport_Scale_DoubleSize()
    {
        var rr = new RenderedReport { PageWidth = 100, PageHeight = 150 };
        rr.Pages.Add(new RenderedPage());
        rr.Pages[0].Elements.Add(new RenderedTextElement { X = 10, Y = 20, Width = 30, Height = 40, Text = "Test" });

        rr.Scale(2.0);

        Assert.Equal(200, rr.PageWidth);
        Assert.Equal(300, rr.PageHeight);
        Assert.Equal(20, rr.Pages[0].Elements[0].X);
        Assert.Equal(40, rr.Pages[0].Elements[0].Y);
        Assert.Equal(60, rr.Pages[0].Elements[0].Width);
        Assert.Equal(80, rr.Pages[0].Elements[0].Height);
    }

    [Fact]
    public void RenderedReport_Scale_HalfSize()
    {
        var rr = new RenderedReport { PageWidth = 200, PageHeight = 300 };
        rr.Pages.Add(new RenderedPage());
        rr.Pages[0].Elements.Add(new RenderedTextElement { X = 40, Y = 60, Width = 80, Height = 100, Text = "Test" });

        rr.Scale(0.5);

        Assert.Equal(100, rr.PageWidth);
        Assert.Equal(150, rr.PageHeight);
        Assert.Equal(20, rr.Pages[0].Elements[0].X);
        Assert.Equal(30, rr.Pages[0].Elements[0].Y);
    }

    [Fact]
    public void RenderedReport_Scale_ZeroFactor_NoOp()
    {
        var rr = new RenderedReport { PageWidth = 200, PageHeight = 300 };
        rr.Scale(0);
        Assert.Equal(200, rr.PageWidth);
    }

    [Fact]
    public void RenderedReport_Scale_NegativeFactor_NoOp()
    {
        var rr = new RenderedReport { PageWidth = 200, PageHeight = 300 };
        rr.Scale(-1);
        Assert.Equal(200, rr.PageWidth);
    }

    [Fact]
    public void RenderedReport_Scale_FactorOne_NoOp()
    {
        var rr = new RenderedReport { PageWidth = 200, PageHeight = 300 };
        rr.Scale(1.0);
        Assert.Equal(200, rr.PageWidth);
        Assert.Equal(300, rr.PageHeight);
    }

    [Fact]
    public void RenderedReport_Scale_FactorNearOne_NoOp()
    {
        var rr = new RenderedReport { PageWidth = 200, PageHeight = 300 };
        rr.Scale(1.0001); // 在 0.001 容差内
        Assert.Equal(200, rr.PageWidth);
    }

    // ============== RenderedReport 属性 ==============

    [Fact]
    public void RenderedReport_DefaultProperties()
    {
        var rr = new RenderedReport();
        Assert.NotNull(rr.Template);
        Assert.NotNull(rr.Pages);
        Assert.Empty(rr.Pages);
        Assert.Equal(0, rr.PageWidth);
        Assert.Equal(0, rr.PageHeight);
    }

    [Fact]
    public void RenderedPage_DefaultProperties()
    {
        var page = new RenderedPage();
        Assert.Equal(0, page.PageNumber);
        Assert.Equal(0, page.TotalPages);
        Assert.NotNull(page.Elements);
        Assert.Empty(page.Elements);
    }

    // ============== RenderedElement 属性 ==============

    [Fact]
    public void RenderedTextElement_Properties()
    {
        var el = new RenderedTextElement
        {
            Id = "test1",
            X = 10, Y = 20,
            Width = 100, Height = 30,
            Text = "Hello World",
            Font = new FontDef { Family = "Arial", Size = 12 },
            Alignment = TextAlignment.Center,
            BackgroundColor = "#FFFFFF",
            Hyperlink = "https://example.com"
        };

        Assert.Equal("test1", el.Id);
        Assert.Equal(10, el.X);
        Assert.Equal(20, el.Y);
        Assert.Equal(100, el.Width);
        Assert.Equal(30, el.Height);
        Assert.Equal("Hello World", el.Text);
        Assert.Equal("Arial", el.Font.Family);
        Assert.Equal(12, el.Font.Size);
        Assert.Equal(TextAlignment.Center, el.Alignment);
        Assert.Equal("#FFFFFF", el.BackgroundColor);
        Assert.Equal("https://example.com", el.Hyperlink);
    }

    [Fact]
    public void RenderedImageElement_Properties()
    {
        var el = new RenderedImageElement
        {
            Id = "img1",
            X = 5, Y = 10,
            Width = 50, Height = 40,
            Source = "photo.jpg"
        };

        Assert.Equal("img1", el.Id);
        Assert.Equal("photo.jpg", el.Source);
        Assert.Equal(50, el.Width);
    }

    [Fact]
    public void RenderedLineElement_Properties()
    {
        var el = new RenderedLineElement
        {
            Id = "line1",
            X = 0, Y = 50,
            Width = 200, Height = 0,
            Direction = LineDirection.Horizontal,
            LineWidth = 2,
            LineColor = "#FF0000"
        };

        Assert.Equal("line1", el.Id);
        Assert.Equal(LineDirection.Horizontal, el.Direction);
        Assert.Equal(2, el.LineWidth);
        Assert.Equal("#FF0000", el.LineColor);
    }

    [Fact]
    public void RenderedShapeElement_Properties()
    {
        var el = new RenderedShapeElement
        {
            Id = "shape1",
            X = 10, Y = 10,
            Width = 60, Height = 40,
            Shape = ShapeType.Ellipse,
            FillColor = "#0000FF",
            BorderRadius = 8
        };

        Assert.Equal("shape1", el.Id);
        Assert.Equal(ShapeType.Ellipse, el.Shape);
        Assert.Equal("#0000FF", el.FillColor);
        Assert.Equal(8, el.BorderRadius);
    }

    [Fact]
    public void RenderedBarcodeElement_Properties()
    {
        var el = new RenderedBarcodeElement
        {
            Id = "bc1",
            X = 10, Y = 10,
            Width = 80, Height = 30,
            Value = "ABC123",
            Format = BarcodeFormat.QRCode,
            ShowText = false,
            ForeColor = "#000000",
            BackColor = "#FFFFFF"
        };

        Assert.Equal("bc1", el.Id);
        Assert.Equal("ABC123", el.Value);
        Assert.Equal(BarcodeFormat.QRCode, el.Format);
        Assert.False(el.ShowText);
    }

    [Fact]
    public void RenderedTableElement_Properties()
    {
        var el = new RenderedTableElement
        {
            Id = "tbl1",
            X = 10, Y = 10,
            Width = 180, Height = 60,
            RowCount = 3,
            ColCount = 4,
            BorderWidth = 1,
            BorderColor = "#333333"
        };
        el.ColumnWidths.AddRange(new[] { 40.0, 50, 40, 50 });
        el.RowHeights.AddRange(new[] { 20.0, 20, 20 });
        el.Cells.Add(new RenderedTableCell { Row = 0, Col = 0, Text = "A1" });

        Assert.Equal("tbl1", el.Id);
        Assert.Equal(3, el.RowCount);
        Assert.Equal(4, el.ColCount);
        Assert.Equal(4, el.ColumnWidths.Count);
        Assert.Equal(3, el.RowHeights.Count);
        Assert.Single(el.Cells);
    }

    [Fact]
    public void RenderedTableCell_Properties()
    {
        var cell = new RenderedTableCell
        {
            Row = 1,
            Col = 2,
            RowSpan = 2,
            ColSpan = 3,
            Text = "Cell Value",
            Font = new FontDef { Bold = true },
            Alignment = TextAlignment.Right,
            BackgroundColor = "#EEEEEE"
        };

        Assert.Equal(1, cell.Row);
        Assert.Equal(2, cell.Col);
        Assert.Equal(2, cell.RowSpan);
        Assert.Equal(3, cell.ColSpan);
        Assert.Equal("Cell Value", cell.Text);
        Assert.True(cell.Font.Bold);
        Assert.Equal(TextAlignment.Right, cell.Alignment);
        Assert.Equal("#EEEEEE", cell.BackgroundColor);
    }

    [Fact]
    public void RenderedCrossTabElement_Properties()
    {
        var el = new RenderedCrossTabElement
        {
            Id = "ct1",
            X = 10, Y = 10,
            Width = 180, Height = 80,
            RowCount = 4,
            ColCount = 5,
            BorderWidth = 0.5,
            BorderColor = "#CCCCCC"
        };

        Assert.Equal("ct1", el.Id);
        Assert.Equal(4, el.RowCount);
        Assert.Equal(5, el.ColCount);
        Assert.Equal(0.5, el.BorderWidth);
    }
}
