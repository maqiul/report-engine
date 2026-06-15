using ReportEngine.Core;
using ReportEngine.Core.Parsing;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// TemplateParser 序列化/反序列化扩展测试
/// </summary>
public class TemplateParserSerializationTests
{
    private readonly TemplateParser _parser = new();

    // ============== 各元素类型序列化往返 ==============

    [Fact]
    public void RoundTrip_ImageElement_PreservesSizing()
    {
        var template = CreateTemplate();
        var img = new ImageElement { Source = "logo.png", Sizing = ImageSizing.FitProportional };
        template.Bands[0].Elements.Add(img);

        var json = _parser.Serialize(template);
        var parsed = _parser.Parse(json);

        var parsedImg = Assert.IsType<ImageElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal("logo.png", parsedImg.Source);
        Assert.Equal(ImageSizing.FitProportional, parsedImg.Sizing);
    }

    [Fact]
    public void RoundTrip_LineElement_PreservesDirection()
    {
        var template = CreateTemplate();
        var line = new LineElement { Direction = LineDirection.Vertical, LineWidth = 2.0, LineColor = "#FF0000" };
        template.Bands[0].Elements.Add(line);

        var json = _parser.Serialize(template);
        var parsed = _parser.Parse(json);

        var parsedLine = Assert.IsType<LineElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal(LineDirection.Vertical, parsedLine.Direction);
        Assert.Equal(2.0, parsedLine.LineWidth);
        Assert.Equal("#FF0000", parsedLine.LineColor);
    }

    [Fact]
    public void RoundTrip_ShapeElement_PreservesShape()
    {
        var template = CreateTemplate();
        var shape = new ShapeElement { Shape = ShapeType.Ellipse, FillColor = "#00FF00", BorderRadius = 5 };
        template.Bands[0].Elements.Add(shape);

        var json = _parser.Serialize(template);
        var parsed = _parser.Parse(json);

        var parsedShape = Assert.IsType<ShapeElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal(ShapeType.Ellipse, parsedShape.Shape);
        Assert.Equal("#00FF00", parsedShape.FillColor);
        Assert.Equal(5, parsedShape.BorderRadius);
    }

    [Fact]
    public void RoundTrip_BarcodeElement_PreservesFormat()
    {
        var template = CreateTemplate();
        var barcode = new BarcodeElement { Value = "12345", Format = BarcodeFormat.Code128, ShowText = false };
        template.Bands[0].Elements.Add(barcode);

        var json = _parser.Serialize(template);
        var parsed = _parser.Parse(json);

        var parsedBarcode = Assert.IsType<BarcodeElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal("12345", parsedBarcode.Value);
        Assert.Equal(BarcodeFormat.Code128, parsedBarcode.Format);
        Assert.False(parsedBarcode.ShowText);
    }

    [Fact]
    public void RoundTrip_ChartElement_PreservesChartType()
    {
        var template = CreateTemplate();
        var chart = new ChartElement { ChartType = ChartType.Pie, DataSource = "sales", CategoryField = "region" };
        chart.Series.Add(new ChartSeries { Name = "Revenue", ValueField = "amount", Color = "#FF0000" });
        template.Bands[0].Elements.Add(chart);

        var json = _parser.Serialize(template);
        var parsed = _parser.Parse(json);

        var parsedChart = Assert.IsType<ChartElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal(ChartType.Pie, parsedChart.ChartType);
        Assert.Equal("sales", parsedChart.DataSource);
        Assert.Single(parsedChart.Series);
        Assert.Equal("Revenue", parsedChart.Series[0].Name);
    }

    [Fact]
    public void RoundTrip_SubReportElement_PreservesTemplateRef()
    {
        var template = CreateTemplate();
        template.DataSources.Add(new DataSourceDef { Name = "orders" });
        var sub = new SubReportElement { TemplateRef = "header.rptx" };
        sub.DataBinding.Source = "orders";
        sub.DataBinding.ParamMap.Add("id", "orderId");
        template.Bands[0].Elements.Add(sub);

        var json = _parser.Serialize(template);
        var parsed = _parser.Parse(json);

        var parsedSub = Assert.IsType<SubReportElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal("header.rptx", parsedSub.TemplateRef);
        Assert.Equal("orders", parsedSub.DataBinding.Source);
        Assert.Equal("orderId", parsedSub.DataBinding.ParamMap["id"]);
    }

    [Fact]
    public void RoundTrip_TableElement_PreservesStructure()
    {
        var template = CreateTemplate();
        var table = new TableElement { RowCount = 2, ColCount = 3, BorderWidth = 0.5 };
        table.ColumnWidths.AddRange(new[] { 30.0, 50.0, 30.0 });
        table.Cells.Add(new TableCell { Row = 0, Col = 0, Text = "A1" });
        table.Cells.Add(new TableCell { Row = 0, Col = 1, Text = "B1" });
        template.Bands[0].Elements.Add(table);

        var json = _parser.Serialize(template);
        var parsed = _parser.Parse(json);

        var parsedTable = Assert.IsType<TableElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal(2, parsedTable.RowCount);
        Assert.Equal(3, parsedTable.ColCount);
        Assert.Equal(0.5, parsedTable.BorderWidth);
        Assert.Equal(3, parsedTable.ColumnWidths.Count);
        Assert.Equal(2, parsedTable.Cells.Count);
    }

    [Fact]
    public void RoundTrip_CrossTabElement_PreservesFields()
    {
        var template = CreateTemplate();
        var ct = new CrossTabElement
        {
            DataSource = "data",
            ShowRowTotal = false,
            ShowColumnTotal = false,
            CellPadding = 2.0
        };
        ct.RowFields.Add("region");
        ct.ColumnFields.Add("year");
        ct.Measures.Add(new CrossTabMeasure { Field = "amount", Aggregate = "Sum" });
        template.Bands[0].Elements.Add(ct);

        var json = _parser.Serialize(template);
        var parsed = _parser.Parse(json);

        var parsedCt = Assert.IsType<CrossTabElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal("data", parsedCt.DataSource);
        Assert.False(parsedCt.ShowRowTotal);
        Assert.Single(parsedCt.RowFields);
        Assert.Single(parsedCt.ColumnFields);
        Assert.Single(parsedCt.Measures);
    }

    // ============== 多元素混合 ==============

    [Fact]
    public void RoundTrip_MixedElements_PreservesOrder()
    {
        var template = CreateTemplate();
        template.Bands[0].Elements.Add(new TextElement { Text = "Title" });
        template.Bands[0].Elements.Add(new ImageElement { Source = "logo.png" });
        template.Bands[0].Elements.Add(new LineElement { Direction = LineDirection.Horizontal });
        template.Bands[0].Elements.Add(new ShapeElement { Shape = ShapeType.Rectangle });

        var json = _parser.Serialize(template);
        var parsed = _parser.Parse(json);

        Assert.Equal(4, parsed.Bands[0].Elements.Count);
        Assert.IsType<TextElement>(parsed.Bands[0].Elements[0]);
        Assert.IsType<ImageElement>(parsed.Bands[0].Elements[1]);
        Assert.IsType<LineElement>(parsed.Bands[0].Elements[2]);
        Assert.IsType<ShapeElement>(parsed.Bands[0].Elements[3]);
    }

    // ============== 多 Band 混合 ==============

    [Fact]
    public void RoundTrip_MultipleBands_PreservesAll()
    {
        var template = new ReportTemplate();
        template.DataSources.Add(new DataSourceDef { Name = "ds" });
        
        var header = new Band { Type = BandType.Header, Height = 30 };
        header.Elements.Add(new TextElement { Text = "Header" });
        
        var detail = new Band { Type = BandType.Detail, Height = 20 };
        detail.Elements.Add(new TextElement { Text = "{{currentRow.name}}" });
        
        var footer = new Band { Type = BandType.Footer, Height = 25 };
        footer.Elements.Add(new TextElement { Text = "Page {{PAGE}}" });
        
        template.Bands.Add(header);
        template.Bands.Add(detail);
        template.Bands.Add(footer);

        var json = _parser.Serialize(template);
        var parsed = _parser.Parse(json);

        Assert.Equal(3, parsed.Bands.Count);
        Assert.Equal(BandType.Header, parsed.Bands[0].Type);
        Assert.Equal(BandType.Detail, parsed.Bands[1].Type);
        Assert.Equal(BandType.Footer, parsed.Bands[2].Type);
    }

    // ============== 条件格式序列化 ==============

    [Fact]
    public void RoundTrip_ConditionalFormat_PreservesRule()
    {
        var template = CreateTemplate();
        var text = new TextElement { Text = "{{amount}}" };
        text.ConditionalFormats.Add(new ConditionalFormatRule
        {
            Expression = "[Amount] > 1000",
            FontColor = "#FF0000"
        });
        template.Bands[0].Elements.Add(text);

        var json = _parser.Serialize(template);
        var parsed = _parser.Parse(json);

        var parsedText = Assert.IsType<TextElement>(parsed.Bands[0].Elements[0]);
        Assert.Single(parsedText.ConditionalFormats);
        Assert.Equal("[Amount] > 1000", parsedText.ConditionalFormats[0].Expression);
        Assert.Equal("#FF0000", parsedText.ConditionalFormats[0].FontColor);
    }

    // ============== 辅助 ==============

    private static ReportTemplate CreateTemplate()
    {
        var t = new ReportTemplate();
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 100 });
        t.DataSources.Add(new DataSourceDef { Name = "ds" });
        return t;
    }
}
