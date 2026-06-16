using ReportEngine.Core;
using ReportEngine.Core.Parsing;
using Xunit;

namespace ReportEngine.Core.Tests;

// ─────────────────────────────────────────────────────────────────────────────
// TemplateParseException 测试
// ─────────────────────────────────────────────────────────────────────────────

public class TemplateParseExceptionExtraTests
{
    [Fact]
    public void TemplateParseException_Message()
    {
        var ex = new TemplateParseException("bad template");
        Assert.Equal("bad template", ex.Message);
    }

    [Fact]
    public void TemplateParseException_WithInnerException()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new TemplateParseException("outer", inner);
        Assert.Equal("outer", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void TemplateParseException_IsException()
    {
        var ex = new TemplateParseException("test");
        Assert.IsAssignableFrom<Exception>(ex);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// TemplateParser 序列化往返测试
// ─────────────────────────────────────────────────────────────────────────────

public class TemplateParserRoundTrip4Tests
{
    private readonly TemplateParser _parser = new();

    [Fact]
    public void RoundTrip_SimpleTemplate()
    {
        var t = new ReportTemplate();
        t.Bands.Add(new Band { Type = BandType.Header, Height = 25 });

        var json = _parser.Serialize(t);
        var t2 = _parser.Parse(json);

        Assert.Single(t2.Bands);
        Assert.Equal(BandType.Header, t2.Bands[0].Type);
        Assert.Equal(25, t2.Bands[0].Height);
    }

    [Fact]
    public void RoundTrip_WithDataSource()
    {
        var t = new ReportTemplate();
        t.DataSources.Add(new DataSourceDef { Name = "orders", Type = "sql" });
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 20, DataSource = "orders" });

        var json = _parser.Serialize(t);
        var t2 = _parser.Parse(json);

        Assert.Single(t2.DataSources);
        Assert.Equal("orders", t2.DataSources[0].Name);
        Assert.Equal("sql", t2.DataSources[0].Type);
    }

    [Fact]
    public void RoundTrip_WithTextElement()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Header, Height = 30 };
        band.Elements.Add(new TextElement { Text = "Hello", X = 10, Y = 5, Width = 100, Height = 15 });
        t.Bands.Add(band);

        var json = _parser.Serialize(t);
        var t2 = _parser.Parse(json);

        Assert.Single(t2.Bands[0].Elements);
        var el = Assert.IsType<TextElement>(t2.Bands[0].Elements[0]);
        Assert.Equal("Hello", el.Text);
        Assert.Equal(10, el.X);
    }

    [Fact]
    public void RoundTrip_WithImageElement()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Header, Height = 50 };
        band.Elements.Add(new ImageElement { Source = "logo.png", Width = 40, Height = 40 });
        t.Bands.Add(band);

        var json = _parser.Serialize(t);
        var t2 = _parser.Parse(json);

        var el = Assert.IsType<ImageElement>(t2.Bands[0].Elements[0]);
        Assert.Equal("logo.png", el.Source);
    }

    [Fact]
    public void RoundTrip_WithLineElement()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Header, Height = 10 };
        band.Elements.Add(new LineElement { LineWidth = 2, LineColor = "#FF0000" });
        t.Bands.Add(band);

        var json = _parser.Serialize(t);
        var t2 = _parser.Parse(json);

        var el = Assert.IsType<LineElement>(t2.Bands[0].Elements[0]);
        Assert.Equal(2, el.LineWidth);
        Assert.Equal("#FF0000", el.LineColor);
    }

    [Fact]
    public void RoundTrip_WithShapeElement()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Header, Height = 50 };
        band.Elements.Add(new ShapeElement { Shape = ShapeType.Ellipse, FillColor = "#00FF00" });
        t.Bands.Add(band);

        var json = _parser.Serialize(t);
        var t2 = _parser.Parse(json);

        var el = Assert.IsType<ShapeElement>(t2.Bands[0].Elements[0]);
        Assert.Equal(ShapeType.Ellipse, el.Shape);
        Assert.Equal("#00FF00", el.FillColor);
    }

    [Fact]
    public void RoundTrip_WithBarcodeElement()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Footer, Height = 30 };
        band.Elements.Add(new BarcodeElement { Value = "ABC123", Format = BarcodeFormat.Code128 });
        t.Bands.Add(band);

        var json = _parser.Serialize(t);
        var t2 = _parser.Parse(json);

        var el = Assert.IsType<BarcodeElement>(t2.Bands[0].Elements[0]);
        Assert.Equal("ABC123", el.Value);
        Assert.Equal(BarcodeFormat.Code128, el.Format);
    }

    [Fact]
    public void RoundTrip_WithTableElement()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 50, DataSource = "ds" };
        t.DataSources.Add(new DataSourceDef { Name = "ds" });
        band.Elements.Add(new TableElement { RowCount = 2, ColCount = 3 });
        t.Bands.Add(band);

        var json = _parser.Serialize(t);
        var t2 = _parser.Parse(json);

        var el = Assert.IsType<TableElement>(t2.Bands[0].Elements[0]);
        Assert.Equal(2, el.RowCount);
        Assert.Equal(3, el.ColCount);
    }

    [Fact]
    public void RoundTrip_WithChartElement()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 100, DataSource = "ds" };
        t.DataSources.Add(new DataSourceDef { Name = "ds" });
        band.Elements.Add(new ChartElement { ChartType = ChartType.Pie, Title = "Sales" });
        t.Bands.Add(band);

        var json = _parser.Serialize(t);
        var t2 = _parser.Parse(json);

        var el = Assert.IsType<ChartElement>(t2.Bands[0].Elements[0]);
        Assert.Equal(ChartType.Pie, el.ChartType);
        Assert.Equal("Sales", el.Title);
    }

    [Fact]
    public void RoundTrip_WithSubReportElement()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 50, DataSource = "ds" };
        t.DataSources.Add(new DataSourceDef { Name = "ds" });
        band.Elements.Add(new SubReportElement { TemplateRef = "sub.rptx" });
        t.Bands.Add(band);

        var json = _parser.Serialize(t);
        var t2 = _parser.Parse(json);

        var el = Assert.IsType<SubReportElement>(t2.Bands[0].Elements[0]);
        Assert.Equal("sub.rptx", el.TemplateRef);
    }

    [Fact]
    public void RoundTrip_WithCrossTabElement()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 100, DataSource = "ds" };
        t.DataSources.Add(new DataSourceDef { Name = "ds" });
        band.Elements.Add(new CrossTabElement { DataSource = "ds" });
        t.Bands.Add(band);

        var json = _parser.Serialize(t);
        var t2 = _parser.Parse(json);

        var el = Assert.IsType<CrossTabElement>(t2.Bands[0].Elements[0]);
        Assert.Equal("ds", el.DataSource);
    }

    [Fact]
    public void RoundTrip_MultipleElements()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Header, Height = 50 };
        band.Elements.Add(new TextElement { Text = "Title" });
        band.Elements.Add(new LineElement { LineWidth = 1 });
        band.Elements.Add(new ShapeElement { Shape = ShapeType.Rectangle });
        t.Bands.Add(band);

        var json = _parser.Serialize(t);
        var t2 = _parser.Parse(json);

        Assert.Equal(3, t2.Bands[0].Elements.Count);
        Assert.IsType<TextElement>(t2.Bands[0].Elements[0]);
        Assert.IsType<LineElement>(t2.Bands[0].Elements[1]);
        Assert.IsType<ShapeElement>(t2.Bands[0].Elements[2]);
    }

    [Fact]
    public void RoundTrip_MultipleBands()
    {
        var t = new ReportTemplate();
        t.Bands.Add(new Band { Type = BandType.ReportHeader, Height = 40 });
        t.Bands.Add(new Band { Type = BandType.Header, Height = 20 });
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 15 });
        t.Bands.Add(new Band { Type = BandType.Footer, Height = 20 });
        t.Bands.Add(new Band { Type = BandType.ReportFooter, Height = 30 });

        var json = _parser.Serialize(t);
        var t2 = _parser.Parse(json);

        Assert.Equal(5, t2.Bands.Count);
        Assert.Equal(BandType.ReportHeader, t2.Bands[0].Type);
        Assert.Equal(BandType.Header, t2.Bands[1].Type);
        Assert.Equal(BandType.Detail, t2.Bands[2].Type);
        Assert.Equal(BandType.Footer, t2.Bands[3].Type);
        Assert.Equal(BandType.ReportFooter, t2.Bands[4].Type);
    }

    [Fact]
    public void RoundTrip_PageSettings()
    {
        var t = new ReportTemplate();
        t.Page.Width = 297;
        t.Page.Height = 420;
        t.Page.Orientation = "landscape";
        t.Page.Margin.Top = 20;
        t.Page.Margin.Bottom = 20;
        t.Page.BackgroundColor = "#FFFFFF";
        t.Bands.Add(new Band { Type = BandType.Header, Height = 20 });

        var json = _parser.Serialize(t);
        var t2 = _parser.Parse(json);

        Assert.Equal(297, t2.Page.Width);
        Assert.Equal(420, t2.Page.Height);
        Assert.Equal("landscape", t2.Page.Orientation);
        Assert.Equal(20, t2.Page.Margin.Top);
        Assert.Equal("#FFFFFF", t2.Page.BackgroundColor);
    }

    [Fact]
    public void RoundTrip_TemplateMetadata()
    {
        var t = new ReportTemplate();
        t.Version = "2.0";
        t.Author = "老马";
        t.Description = "测试模板";
        t.Bands.Add(new Band { Type = BandType.Header, Height = 20 });

        var json = _parser.Serialize(t);
        var t2 = _parser.Parse(json);

        Assert.Equal("2.0", t2.Version);
        Assert.Equal("老马", t2.Author);
        Assert.Equal("测试模板", t2.Description);
    }

    [Fact]
    public void RoundTrip_Parameters()
    {
        var t = new ReportTemplate();
        t.Parameters.Add(new TemplateParam { Name = "year", Type = "number", DefaultValue = "2026", Label = "年份" });
        t.Parameters.Add(new TemplateParam { Name = "startDate", Type = "date" });
        t.Bands.Add(new Band { Type = BandType.Header, Height = 20 });

        var json = _parser.Serialize(t);
        var t2 = _parser.Parse(json);

        Assert.Equal(2, t2.Parameters.Count);
        Assert.Equal("year", t2.Parameters[0].Name);
        Assert.Equal("number", t2.Parameters[0].Type);
        Assert.Equal("2026", t2.Parameters[0].DefaultValue);
        Assert.Equal("年份", t2.Parameters[0].Label);
    }
}
