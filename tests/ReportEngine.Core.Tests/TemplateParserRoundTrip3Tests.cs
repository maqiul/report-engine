using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Parsing;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// TemplateParser 序列化/解析往返测试 — 确保所有属性不丢失
/// </summary>
public class TemplateParserRoundTrip3Tests
{
    private readonly TemplateParser _parser = new();

    [Fact]
    public void RoundTrip_AuthorDescription_Preserved()
    {
        var t = new ReportTemplate { Author = "Alice", Description = "Test report" };
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
        var json = _parser.Serialize(t);
        var parsed = _parser.Parse(json);
        Assert.Equal("Alice", parsed.Author);
        Assert.Equal("Test report", parsed.Description);
    }

    [Fact]
    public void RoundTrip_PageBackgroundColor_Preserved()
    {
        var t = new ReportTemplate();
        t.Page.BackgroundColor = "#E0E0E0";
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
        var json = _parser.Serialize(t);
        var parsed = _parser.Parse(json);
        Assert.Equal("#E0E0E0", parsed.Page.BackgroundColor);
    }

    [Fact]
    public void RoundTrip_PageWatermark_Preserved()
    {
        var t = new ReportTemplate();
        t.Page.Watermark = "CONFIDENTIAL";
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
        var json = _parser.Serialize(t);
        var parsed = _parser.Parse(json);
        Assert.Equal("CONFIDENTIAL", parsed.Page.Watermark);
    }

    [Fact]
    public void RoundTrip_DataSourceConnectionString_Preserved()
    {
        var t = new ReportTemplate();
        t.DataSources.Add(new DataSourceDef { Name = "ds", Type = "sql", ConnectionString = "Server=localhost;Database=test", Query = "SELECT * FROM t" });
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
        var json = _parser.Serialize(t);
        var parsed = _parser.Parse(json);
        Assert.Equal("Server=localhost;Database=test", parsed.DataSources[0].ConnectionString);
        Assert.Equal("SELECT * FROM t", parsed.DataSources[0].Query);
    }

    [Fact]
    public void RoundTrip_ParameterLabel_Preserved()
    {
        var t = new ReportTemplate();
        t.Parameters.Add(new TemplateParam { Name = "p1", Type = "string", DefaultValue = "hello", Label = "Enter value" });
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });
        var json = _parser.Serialize(t);
        var parsed = _parser.Parse(json);
        Assert.Equal("Enter value", parsed.Parameters[0].Label);
        Assert.Equal("hello", parsed.Parameters[0].DefaultValue);
    }

    [Fact]
    public void RoundTrip_TextElement_Hyperlink_Preserved()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20 };
        band.Elements.Add(new TextElement { Text = "Click", Hyperlink = "https://example.com" });
        t.Bands.Add(band);
        var json = _parser.Serialize(t);
        var parsed = _parser.Parse(json);
        var el = Assert.IsType<TextElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal("https://example.com", el.Hyperlink);
    }

    [Fact]
    public void RoundTrip_TextElement_SummaryField_Preserved()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20 };
        band.Elements.Add(new TextElement { SummaryField = "Amount", Format = "N2" });
        t.Bands.Add(band);
        var json = _parser.Serialize(t);
        var parsed = _parser.Parse(json);
        var el = Assert.IsType<TextElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal("Amount", el.SummaryField);
        Assert.Equal("N2", el.Format);
    }

    [Fact]
    public void RoundTrip_TextElement_SystemVariable_Preserved()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20 };
        band.Elements.Add(new TextElement { SystemVariable = "PageNumber" });
        t.Bands.Add(band);
        var json = _parser.Serialize(t);
        var parsed = _parser.Parse(json);
        var el = Assert.IsType<TextElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal("PageNumber", el.SystemVariable);
    }

    [Fact]
    public void RoundTrip_ImageElement_SourceAndSizing_Preserved()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20 };
        band.Elements.Add(new ImageElement { Source = "img/logo.png", Sizing = ImageSizing.FitProportional });
        t.Bands.Add(band);
        var json = _parser.Serialize(t);
        var parsed = _parser.Parse(json);
        var el = Assert.IsType<ImageElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal("img/logo.png", el.Source);
        Assert.Equal(ImageSizing.FitProportional, el.Sizing);
    }

    [Fact]
    public void RoundTrip_LineElement_Direction_Preserved()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20 };
        band.Elements.Add(new LineElement { Direction = LineDirection.Vertical, LineWidth = 3, LineColor = "#ABCDEF" });
        t.Bands.Add(band);
        var json = _parser.Serialize(t);
        var parsed = _parser.Parse(json);
        var el = Assert.IsType<LineElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal(LineDirection.Vertical, el.Direction);
        Assert.Equal(3, el.LineWidth);
        Assert.Equal("#ABCDEF", el.LineColor);
    }

    [Fact]
    public void RoundTrip_ShapeElement_FillColor_Preserved()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20 };
        band.Elements.Add(new ShapeElement { Shape = ShapeType.Rectangle, FillColor = "#123456", BorderRadius = 5 });
        t.Bands.Add(band);
        var json = _parser.Serialize(t);
        var parsed = _parser.Parse(json);
        var el = Assert.IsType<ShapeElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal(ShapeType.Rectangle, el.Shape);
        Assert.Equal("#123456", el.FillColor);
        Assert.Equal(5, el.BorderRadius);
    }

    [Fact]
    public void RoundTrip_BarcodeElement_Format_Preserved()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20 };
        band.Elements.Add(new BarcodeElement { Value = "{{OrderId}}", Format = BarcodeFormat.EAN13, ShowText = false });
        t.Bands.Add(band);
        var json = _parser.Serialize(t);
        var parsed = _parser.Parse(json);
        var el = Assert.IsType<BarcodeElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal("{{OrderId}}", el.Value);
        Assert.Equal(BarcodeFormat.EAN13, el.Format);
        Assert.False(el.ShowText);
    }

    [Fact]
    public void RoundTrip_ChartElement_Series_Preserved()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20 };
        var chart = new ChartElement { ChartType = ChartType.Line, Title = "Trend" };
        chart.Series.Add(new ChartSeries { Name = "Revenue", ValueField = "Amount" });
        band.Elements.Add(chart);
        t.Bands.Add(band);
        var json = _parser.Serialize(t);
        var parsed = _parser.Parse(json);
        var el = Assert.IsType<ChartElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal(ChartType.Line, el.ChartType);
        Assert.Equal("Trend", el.Title);
        Assert.Single(el.Series);
        Assert.Equal("Revenue", el.Series[0].Name);
        Assert.Equal("Amount", el.Series[0].ValueField);
    }

    [Fact]
    public void RoundTrip_SubReportElement_DataBinding_Preserved()
    {
        var t = new ReportTemplate();
        t.DataSources.Add(new DataSourceDef { Name = "sub" });
        var band = new Band { Type = BandType.Detail, Height = 20 };
        band.Elements.Add(new SubReportElement
        {
            TemplateRef = "sub.rpt",
            HeightMode = "auto",
            RepeatPerRow = false,
            DataBinding = new SubReportDataBinding { Source = "sub" }
        });
        t.Bands.Add(band);
        var json = _parser.Serialize(t);
        var parsed = _parser.Parse(json);
        var el = Assert.IsType<SubReportElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal("sub.rpt", el.TemplateRef);
        Assert.Equal("auto", el.HeightMode);
        Assert.False(el.RepeatPerRow);
        Assert.Equal("sub", el.DataBinding!.Source);
    }

    [Fact]
    public void RoundTrip_BandRepeatOnNewPage_Preserved()
    {
        var t = new ReportTemplate();
        t.Bands.Add(new Band { Type = BandType.Header, Height = 20, RepeatOnNewPage = true });
        var json = _parser.Serialize(t);
        var parsed = _parser.Parse(json);
        Assert.True(parsed.Bands[0].RepeatOnNewPage);
    }

    [Fact]
    public void RoundTrip_BandDataSource_Preserved()
    {
        var t = new ReportTemplate();
        t.DataSources.Add(new DataSourceDef { Name = "ds1" });
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 20, DataSource = "ds1" });
        var json = _parser.Serialize(t);
        var parsed = _parser.Parse(json);
        Assert.Equal("ds1", parsed.Bands[0].DataSource);
    }

    [Fact]
    public void RoundTrip_BandGroup_Preserved()
    {
        var t = new ReportTemplate();
        t.Bands.Add(new Band
        {
            Type = BandType.GroupHeader,
            Height = 15,
            Group = new GroupDef { Expression = "[Category]", KeepTogether = true }
        });
        var json = _parser.Serialize(t);
        var parsed = _parser.Parse(json);
        Assert.NotNull(parsed.Bands[0].Group);
        Assert.Equal("[Category]", parsed.Bands[0].Group!.Expression);
        Assert.True(parsed.Bands[0].Group!.KeepTogether);
    }

    [Fact]
    public void RoundTrip_MultiColumn_Preserved()
    {
        var t = new ReportTemplate();
        t.Bands.Add(new Band
        {
            Type = BandType.Detail,
            Height = 30,
            MultiColumn = new MultiColumnConfig { ColumnCount = 3, ColumnSpacing = 8, Direction = "Vertical" }
        });
        var json = _parser.Serialize(t);
        var parsed = _parser.Parse(json);
        Assert.NotNull(parsed.Bands[0].MultiColumn);
        Assert.Equal(3, parsed.Bands[0].MultiColumn!.ColumnCount);
        Assert.Equal(8, parsed.Bands[0].MultiColumn!.ColumnSpacing);
        Assert.Equal("Vertical", parsed.Bands[0].MultiColumn!.Direction);
    }

    [Fact]
    public void RoundTrip_ElementBorder_Preserved()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20 };
        band.Elements.Add(new TextElement
        {
            Text = "Bordered",
            Border = new BorderDef { Width = 2, Color = "#FF0000", Style = BorderStyle.Dashed, Top = true, Bottom = true }
        });
        t.Bands.Add(band);
        var json = _parser.Serialize(t);
        var parsed = _parser.Parse(json);
        var border = parsed.Bands[0].Elements[0].Border;
        Assert.NotNull(border);
        Assert.Equal(2, border!.Width);
        Assert.Equal("#FF0000", border.Color);
        Assert.Equal(BorderStyle.Dashed, border.Style);
        Assert.True(border.Top);
        Assert.True(border.Bottom);
    }

    [Fact]
    public void RoundTrip_ElementConditionalFormats_Preserved()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20 };
        var el = new TextElement { Text = "Cond" };
        el.ConditionalFormats.Add(new ConditionalFormatRule { Expression = "[x]>100", BackgroundColor = "#FF0000", Bold = true });
        band.Elements.Add(el);
        t.Bands.Add(band);
        var json = _parser.Serialize(t);
        var parsed = _parser.Parse(json);
        Assert.Single(parsed.Bands[0].Elements[0].ConditionalFormats);
        Assert.Equal("[x]>100", parsed.Bands[0].Elements[0].ConditionalFormats[0].Expression);
        Assert.Equal("#FF0000", parsed.Bands[0].Elements[0].ConditionalFormats[0].BackgroundColor);
        Assert.True(parsed.Bands[0].Elements[0].ConditionalFormats[0].Bold);
    }

    [Fact]
    public void RoundTrip_CrossTabElement_Measures_Preserved()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 30 };
        var ct = new CrossTabElement
        {
            DataSource = "sales",
            ShowRowTotal = false,
            ShowColumnTotal = false,
            CellPadding = 2.5,
            BorderWidth = 0.5,
            BorderColor = "#CCCCCC"
        };
        ct.RowFields.Add("Region");
        ct.ColumnFields.Add("Year");
        ct.Measures.Add(new CrossTabMeasure { Field = "Amount", Aggregate = "Avg" });
        band.Elements.Add(ct);
        t.Bands.Add(band);
        var json = _parser.Serialize(t);
        var parsed = _parser.Parse(json);
        var el = Assert.IsType<CrossTabElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal("sales", el.DataSource);
        Assert.Single(el.RowFields);
        Assert.Single(el.ColumnFields);
        Assert.Single(el.Measures);
        Assert.Equal("Avg", el.Measures[0].Aggregate);
        Assert.False(el.ShowRowTotal);
        Assert.Equal(2.5, el.CellPadding);
    }

    [Fact]
    public void RoundTrip_TableElement_Cells_Preserved()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 30 };
        var table = new TableElement { RowCount = 2, ColCount = 2, BorderWidth = 1, BorderColor = "#333333" };
        table.ColumnWidths.Add(50);
        table.ColumnWidths.Add(80);
        table.RowHeights.Add(15);
        table.RowHeights.Add(20);
        table.Cells.Add(new TableCell { Row = 0, Col = 0, Text = "A", RowSpan = 1, ColSpan = 2 });
        table.Cells.Add(new TableCell { Row = 1, Col = 0, Text = "B" });
        band.Elements.Add(table);
        t.Bands.Add(band);
        var json = _parser.Serialize(t);
        var parsed = _parser.Parse(json);
        var el = Assert.IsType<TableElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal(2, el.RowCount);
        Assert.Equal(2, el.ColCount);
        Assert.Equal(2, el.Cells.Count);
        Assert.Equal(2, el.ColumnWidths.Count);
        Assert.Equal(2, el.Cells[0].ColSpan);
    }

    [Fact]
    public void RoundTrip_FontDef_AllProperties()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 20 };
        band.Elements.Add(new TextElement
        {
            Text = "Styled",
            Font = new FontDef { Family = "Consolas", Size = 12, Bold = true, Italic = true, Underline = true, Color = "#00FF00" }
        });
        t.Bands.Add(band);
        var json = _parser.Serialize(t);
        var parsed = _parser.Parse(json);
        var el = Assert.IsType<TextElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal("Consolas", el.Font.Family);
        Assert.Equal(12, el.Font.Size);
        Assert.True(el.Font.Bold);
        Assert.True(el.Font.Italic);
        Assert.True(el.Font.Underline);
        Assert.Equal("#00FF00", el.Font.Color);
    }

    [Fact]
    public void RoundTrip_ElementPositionAndSize()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 50 };
        band.Elements.Add(new TextElement { Text = "Pos", X = 15.5, Y = 25.3, Width = 100.7, Height = 12.8 });
        t.Bands.Add(band);
        var json = _parser.Serialize(t);
        var parsed = _parser.Parse(json);
        var el = parsed.Bands[0].Elements[0];
        Assert.Equal(15.5, el.X);
        Assert.Equal(25.3, el.Y);
        Assert.Equal(100.7, el.Width);
        Assert.Equal(12.8, el.Height);
    }
}
