using System.Collections.Generic;
using System.IO;
using ReportEngine.Core;
using ReportEngine.Core.Parsing;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// TemplateParser 序列化/反序列化往返测试：
///   - Serialize → Parse 往返
///   - 文件序列化往返
///   - 各种元素类型的往返
///   - 复杂模板的往返
/// </summary>
public class TemplateParserRoundTripTests
{
    private readonly TemplateParser _parser = new TemplateParser();

    // ============== 基本往返 ==============

    [Fact]
    public void Serialize_ThenParse_MinimalTemplate_RoundTrips()
    {
        var original = new ReportTemplate();
        original.Bands.Add(new Band { Type = BandType.Header, Height = 20 });

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        Assert.Single(parsed.Bands);
        Assert.Equal(BandType.Header, parsed.Bands[0].Type);
        Assert.Equal(20, parsed.Bands[0].Height);
    }

    [Fact]
    public void Serialize_ThenParse_WithDataSource_RoundTrips()
    {
        var original = new ReportTemplate();
        original.DataSources.Add(new DataSourceDef { Name = "orders" });
        original.Bands.Add(new Band { Type = BandType.Detail, Height = 30, DataSource = "orders" });

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        Assert.Single(parsed.DataSources);
        Assert.Equal("orders", parsed.DataSources[0].Name);
        Assert.Single(parsed.Bands);
        Assert.Equal("orders", parsed.Bands[0].DataSource);
    }

    // ============== 页面设置往返 ==============

    [Fact]
    public void Serialize_ThenParse_PageSettings_RoundTrips()
    {
        var original = new ReportTemplate();
        original.Page = new PageInfo
        {
            Width = 150,
            Height = 200,
            Orientation = "landscape",
            Margin = new Margin { Top = 10, Bottom = 15, Left = 12, Right = 12 }
        };
        original.Bands.Add(new Band { Type = BandType.Header, Height = 20 });

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        Assert.Equal(150, parsed.Page.Width);
        Assert.Equal(200, parsed.Page.Height);
        Assert.Equal("landscape", parsed.Page.Orientation);
        Assert.Equal(10, parsed.Page.Margin.Top);
        Assert.Equal(15, parsed.Page.Margin.Bottom);
        Assert.Equal(12, parsed.Page.Margin.Left);
        Assert.Equal(12, parsed.Page.Margin.Right);
    }

    // ============== 文本元素往返 ==============

    [Fact]
    public void Serialize_ThenParse_TextElement_RoundTrips()
    {
        var original = new ReportTemplate();
        var band = new Band { Type = BandType.Header, Height = 20 };
        band.Elements.Add(new TextElement
        {
            X = 10,
            Y = 5,
            Width = 80,
            Height = 10,
            Text = "Hello {{name}}",
            Font = new FontDef { Family = "Arial", Size = 12, Bold = true, Italic = false, Color = "#333333" },
            Alignment = TextAlignment.Center
        });
        original.Bands.Add(band);

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        var el = Assert.IsType<TextElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal(10, el.X);
        Assert.Equal(5, el.Y);
        Assert.Equal(80, el.Width);
        Assert.Equal(10, el.Height);
        Assert.Equal("Hello {{name}}", el.Text);
        Assert.Equal("Arial", el.Font.Family);
        Assert.Equal(12, el.Font.Size);
        Assert.True(el.Font.Bold);
        Assert.Equal(TextAlignment.Center, el.Alignment);
        Assert.Equal("#333333", el.Font.Color);
    }

    // ============== 图片元素往返 ==============

    [Fact]
    public void Serialize_ThenParse_ImageElement_RoundTrips()
    {
        var original = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 30, DataSource = "ds" };
        band.Elements.Add(new ImageElement
        {
            X = 5,
            Y = 5,
            Width = 40,
            Height = 20,
            Source = "{{photo}}",
            Sizing = ImageSizing.Stretch
        });
        original.DataSources.Add(new DataSourceDef { Name = "ds" });
        original.Bands.Add(band);

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        var el = Assert.IsType<ImageElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal(5, el.X);
        Assert.Equal(5, el.Y);
        Assert.Equal(40, el.Width);
        Assert.Equal(20, el.Height);
        Assert.Equal("{{photo}}", el.Source);
        Assert.Equal(ImageSizing.Stretch, el.Sizing);
    }

    // ============== 线条元素往返 ==============

    [Fact]
    public void Serialize_ThenParse_LineElement_RoundTrips()
    {
        var original = new ReportTemplate();
        var band = new Band { Type = BandType.Header, Height = 20 };
        band.Elements.Add(new LineElement
        {
            X = 0,
            Y = 19,
            Width = 200,
            Height = 0,
            LineWidth = 2,
            LineColor = "#FF0000",
            Direction = LineDirection.Horizontal
        });
        original.Bands.Add(band);

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        var el = Assert.IsType<LineElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal(0, el.X);
        Assert.Equal(19, el.Y);
        Assert.Equal(200, el.Width);
        Assert.Equal(2, el.LineWidth);
        Assert.Equal("#FF0000", el.LineColor);
        Assert.Equal(LineDirection.Horizontal, el.Direction);
    }

    // ============== 形状元素往返 ==============

    [Fact]
    public void Serialize_ThenParse_ShapeElement_RoundTrips()
    {
        var original = new ReportTemplate();
        var band = new Band { Type = BandType.Header, Height = 30 };
        band.Elements.Add(new ShapeElement
        {
            X = 10,
            Y = 5,
            Width = 30,
            Height = 20,
            Shape = ShapeType.Rectangle,
            FillColor = "#EEEEEE",
            Border = new BorderDef { Top = true, Bottom = true, Left = true, Right = true }
        });
        original.Bands.Add(band);

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        var el = Assert.IsType<ShapeElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal(ShapeType.Rectangle, el.Shape);
        Assert.Equal("#EEEEEE", el.FillColor);
    }

    // ============== 条码元素往返 ==============

    [Fact]
    public void Serialize_ThenParse_BarcodeElement_RoundTrips()
    {
        var original = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 30, DataSource = "ds" };
        band.Elements.Add(new BarcodeElement
        {
            X = 10,
            Y = 5,
            Width = 60,
            Height = 20,
            Format = BarcodeFormat.Code128,
            Value = "{{orderNo}}",
            ShowText = true
        });
        original.DataSources.Add(new DataSourceDef { Name = "ds" });
        original.Bands.Add(band);

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        var el = Assert.IsType<BarcodeElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal(BarcodeFormat.Code128, el.Format);
        Assert.Equal("{{orderNo}}", el.Value);
        Assert.True(el.ShowText);
    }

    // ============== 表格元素往返 ==============

    [Fact]
    public void Serialize_ThenParse_TableElement_RoundTrips()
    {
        var original = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 50, DataSource = "ds" };
        var table = new TableElement
        {
            X = 10,
            Y = 5,
            Width = 180,
            Height = 40,
            RowCount = 2,
            ColCount = 3,
            ColumnWidths = new List<double> { 60, 60, 60 },
            RowHeights = new List<double> { 15, 25 }
        };
        table.Cells.Add(new TableCell { Row = 0, Col = 0, Text = "Header 1" });
        table.Cells.Add(new TableCell { Row = 0, Col = 1, Text = "Header 2" });
        table.Cells.Add(new TableCell { Row = 0, Col = 2, Text = "Header 3" });
        band.Elements.Add(table);
        original.DataSources.Add(new DataSourceDef { Name = "ds" });
        original.Bands.Add(band);

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        var el = Assert.IsType<TableElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal(2, el.RowCount);
        Assert.Equal(3, el.ColCount);
        Assert.Equal(3, el.ColumnWidths.Count);
        Assert.Equal(2, el.RowHeights.Count);
        Assert.True(el.Cells.Count >= 3);
    }

    // ============== 多 Band 往返 ==============

    [Fact]
    public void Serialize_ThenParse_MultipleBands_RoundTrips()
    {
        var original = new ReportTemplate();
        original.DataSources.Add(new DataSourceDef { Name = "ds" });
        original.Bands.Add(new Band { Type = BandType.ReportHeader, Height = 25 });
        original.Bands.Add(new Band { Type = BandType.Header, Height = 15 });
        original.Bands.Add(new Band { Type = BandType.Detail, Height = 30, DataSource = "ds" });
        original.Bands.Add(new Band { Type = BandType.Footer, Height = 15 });

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        Assert.Equal(4, parsed.Bands.Count);
        Assert.Equal(BandType.ReportHeader, parsed.Bands[0].Type);
        Assert.Equal(BandType.Header, parsed.Bands[1].Type);
        Assert.Equal(BandType.Detail, parsed.Bands[2].Type);
        Assert.Equal(BandType.Footer, parsed.Bands[3].Type);
    }

    // ============== 多 DataSource 往返 ==============

    [Fact]
    public void Serialize_ThenParse_MultipleDataSources_RoundTrips()
    {
        var original = new ReportTemplate();
        original.DataSources.Add(new DataSourceDef { Name = "orders" });
        original.DataSources.Add(new DataSourceDef { Name = "products" });
        original.DataSources.Add(new DataSourceDef { Name = "customers" });
        original.Bands.Add(new Band { Type = BandType.Detail, Height = 30, DataSource = "orders" });

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        Assert.Equal(3, parsed.DataSources.Count);
        Assert.Equal("orders", parsed.DataSources[0].Name);
        Assert.Equal("products", parsed.DataSources[1].Name);
        Assert.Equal("customers", parsed.DataSources[2].Name);
    }

    // ============== 子报表元素往返 ==============

    [Fact]
    public void Serialize_ThenParse_SubReportElement_RoundTrips()
    {
        var original = new ReportTemplate();
        original.DataSources.Add(new DataSourceDef { Name = "ds" });
        var band = new Band { Type = BandType.Detail, Height = 50, DataSource = "ds" };
        band.Elements.Add(new SubReportElement
        {
            X = 10,
            Y = 5,
            Width = 180,
            Height = 40,
            TemplateRef = "sub_report.rptx",
            DataBinding = new SubReportDataBinding { Source = "ds" },
            HeightMode = "auto",
            RepeatPerRow = true
        });
        original.Bands.Add(band);

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        var el = Assert.IsType<SubReportElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal("sub_report.rptx", el.TemplateRef);
        Assert.Equal("ds", el.DataBinding.Source);
        Assert.Equal("auto", el.HeightMode);
        Assert.True(el.RepeatPerRow);
    }

    // ============== CrossTab 元素往返 ==============

    [Fact]
    public void Serialize_ThenParse_CrossTabElement_RoundTrips()
    {
        var original = new ReportTemplate();
        original.DataSources.Add(new DataSourceDef { Name = "ds" });
        var band = new Band { Type = BandType.Detail, Height = 60, DataSource = "ds" };
        var crossTab = new CrossTabElement
        {
            X = 10,
            Y = 5,
            Width = 180,
            Height = 50,
            DataSource = "ds",
            RowFields = new List<string> { "region" },
            ColumnFields = new List<string> { "quarter" }
        };
        crossTab.Measures.Add(new CrossTabMeasure { Field = "amount", Aggregate = "Sum" });
        band.Elements.Add(crossTab);
        original.Bands.Add(band);

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        var el = Assert.IsType<CrossTabElement>(parsed.Bands[0].Elements[0]);
        Assert.Single(el.RowFields);
        Assert.Equal("region", el.RowFields[0]);
        Assert.Single(el.ColumnFields);
        Assert.Equal("quarter", el.ColumnFields[0]);
        Assert.Single(el.Measures);
        Assert.Equal("amount", el.Measures[0].Field);
        Assert.Equal("Sum", el.Measures[0].Aggregate);
    }

    // ============== Chart 元素往返 ==============

    [Fact]
    public void Serialize_ThenParse_ChartElement_RoundTrips()
    {
        var original = new ReportTemplate();
        original.DataSources.Add(new DataSourceDef { Name = "ds" });
        var band = new Band { Type = BandType.Detail, Height = 80, DataSource = "ds" };
        var chart = new ChartElement
        {
            X = 10,
            Y = 5,
            Width = 180,
            Height = 70,
            ChartType = ChartType.Bar,
            CategoryField = "month",
            DataSource = "ds",
            Title = "Monthly Sales"
        };
        chart.Series.Add(new ChartSeries { Name = "Sales", ValueField = "sales" });
        band.Elements.Add(chart);
        original.Bands.Add(band);

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        var el = Assert.IsType<ChartElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal(ChartType.Bar, el.ChartType);
        Assert.Equal("month", el.CategoryField);
        Assert.Equal("Monthly Sales", el.Title);
        Assert.Single(el.Series);
        Assert.Equal("sales", el.Series[0].ValueField);
    }

    // ============== 文件往返 ==============

    [Fact]
    public void SaveFile_ThenParseFile_RoundTrips()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{System.Guid.NewGuid():N}.rptx");
        try
        {
            var original = new ReportTemplate();
            original.DataSources.Add(new DataSourceDef { Name = "ds" });
            original.Bands.Add(new Band { Type = BandType.Header, Height = 20 });
            original.Bands.Add(new Band { Type = BandType.Detail, Height = 30, DataSource = "ds" });

            var json = _parser.Serialize(original);
            File.WriteAllText(tempFile, json);

            var parsed = _parser.ParseFile(tempFile);
            Assert.Equal(2, parsed.Bands.Count);
            Assert.Single(parsed.DataSources);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    // ============== 条件格式往返 ==============

    [Fact]
    public void Serialize_ThenParse_ConditionalFormat_RoundTrips()
    {
        var original = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 30, DataSource = "ds" };
        var textEl = new TextElement
        {
            X = 10,
            Y = 5,
            Width = 80,
            Height = 10,
            Text = "{{amount}}"
        };
        textEl.ConditionalFormats.Add(new ConditionalFormatRule
        {
            Expression = "[Amount] > 1000",
            FontColor = "#FF0000",
            BackgroundColor = "#FFFF00",
            Bold = true
        });
        band.Elements.Add(textEl);
        original.DataSources.Add(new DataSourceDef { Name = "ds" });
        original.Bands.Add(band);

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        var el = Assert.IsType<TextElement>(parsed.Bands[0].Elements[0]);
        Assert.Single(el.ConditionalFormats);
        Assert.Equal("[Amount] > 1000", el.ConditionalFormats[0].Expression);
        Assert.Equal("#FF0000", el.ConditionalFormats[0].FontColor);
        Assert.Equal("#FFFF00", el.ConditionalFormats[0].BackgroundColor);
        Assert.True(el.ConditionalFormats[0].Bold);
    }

    // ============== Group 往返 ==============

    [Fact]
    public void Serialize_ThenParse_GroupDef_RoundTrips()
    {
        var original = new ReportTemplate();
        original.DataSources.Add(new DataSourceDef { Name = "ds" });
        original.Bands.Add(new Band
        {
            Type = BandType.GroupHeader,
            Height = 20,
            DataSource = "ds",
            Group = new GroupDef { Expression = "region", KeepTogether = true }
        });
        original.Bands.Add(new Band { Type = BandType.Detail, Height = 30, DataSource = "ds" });
        original.Bands.Add(new Band { Type = BandType.GroupFooter, Height = 15, DataSource = "ds" });

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        Assert.Equal(3, parsed.Bands.Count);
        Assert.NotNull(parsed.Bands[0].Group);
        Assert.Equal("region", parsed.Bands[0].Group!.Expression);
        Assert.True(parsed.Bands[0].Group.KeepTogether);
    }

    // ============== TemplateParam 往返 ==============

    [Fact]
    public void Serialize_ThenParse_TemplateParams_RoundTrips()
    {
        var original = new ReportTemplate();
        original.Parameters.Add(new TemplateParam
        {
            Name = "startDate",
            Type = "date",
            DefaultValue = "2024-01-01",
            Label = "Start Date"
        });
        original.Parameters.Add(new TemplateParam
        {
            Name = "showDetails",
            Type = "boolean",
            DefaultValue = "true",
            Label = "Show Details"
        });
        original.Bands.Add(new Band { Type = BandType.Header, Height = 20 });

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        Assert.Equal(2, parsed.Parameters.Count);
        Assert.Equal("startDate", parsed.Parameters[0].Name);
        Assert.Equal("date", parsed.Parameters[0].Type);
        Assert.Equal("2024-01-01", parsed.Parameters[0].DefaultValue);
        Assert.Equal("showDetails", parsed.Parameters[1].Name);
        Assert.Equal("boolean", parsed.Parameters[1].Type);
    }

    // ============== MultiUp 往返 ==============

    [Fact]
    public void Serialize_ThenParse_MultiUpConfig_RoundTrips()
    {
        var original = new ReportTemplate();
        original.Page.MultiUp = new MultiUpConfig { Rows = 2, Columns = 3 };
        original.Bands.Add(new Band { Type = BandType.Detail, Height = 30, DataSource = "ds" });
        original.DataSources.Add(new DataSourceDef { Name = "ds" });

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        Assert.NotNull(parsed.Page.MultiUp);
        Assert.Equal(2, parsed.Page.MultiUp!.Rows);
        Assert.Equal(3, parsed.Page.MultiUp.Columns);
    }

    // ============== ReportElement 基类属性往返 ==============

    [Fact]
    public void Serialize_ThenParse_BaseElementProperties_RoundTrips()
    {
        var original = new ReportTemplate();
        var band = new Band { Type = BandType.Header, Height = 30 };
        band.Elements.Add(new TextElement
        {
            X = 10,
            Y = 5,
            Width = 80,
            Height = 10,
            Text = "Test",
            Visible = false,
            Locked = true,
            Rotation = 45,
            Opacity = 0.5,
            BackgroundColor = "#CCCCCC",
            Border = new BorderDef { Top = true, Bottom = true, Left = true, Right = true, Width = 2, Color = "#000000", Style = BorderStyle.Dashed }
        });
        original.Bands.Add(band);

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        var el = Assert.IsType<TextElement>(parsed.Bands[0].Elements[0]);
        Assert.False(el.Visible);
        Assert.True(el.Locked);
        Assert.Equal(45, el.Rotation);
        Assert.Equal(0.5, el.Opacity);
        Assert.Equal("#CCCCCC", el.BackgroundColor);
        Assert.NotNull(el.Border);
        Assert.True(el.Border!.Top);
        Assert.Equal(2, el.Border.Width);
        Assert.Equal(BorderStyle.Dashed, el.Border.Style);
    }
}
