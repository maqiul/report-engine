using ReportEngine.Core;
using ReportEngine.Core.Parsing;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// TemplateParser 序列化/反序列化往返测试
/// </summary>
public class TemplateParserRoundTrip2Tests
{
    private readonly TemplateParser _parser = new();

    // ============== 基础往返 ==============

    [Fact]
    public void RoundTrip_EmptyTemplate_Works()
    {
        var original = new ReportTemplate();
        original.Bands.Add(new Band { Type = BandType.Detail, Height = 20 });

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        Assert.Single(parsed.Bands);
        Assert.Equal(BandType.Detail, parsed.Bands[0].Type);
    }

    [Fact]
    public void RoundTrip_Version_Preserved()
    {
        var original = new ReportTemplate { Version = "2.5" };
        original.Bands.Add(new Band { Type = BandType.Detail });

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        Assert.Equal("2.5", parsed.Version);
    }

    [Fact]
    public void RoundTrip_PageInfo_Preserved()
    {
        var original = new ReportTemplate
        {
            Page = new PageInfo
            {
                Width = 297,
                Height = 210,
                Orientation = "landscape",
                Watermark = "DRAFT"
            }
        };
        original.Bands.Add(new Band { Type = BandType.Detail });

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        Assert.Equal(297, parsed.Page.Width);
        Assert.Equal(210, parsed.Page.Height);
        Assert.Equal("landscape", parsed.Page.Orientation);
        Assert.Equal("DRAFT", parsed.Page.Watermark);
    }

    [Fact]
    public void RoundTrip_Parameters_Preserved()
    {
        var original = new ReportTemplate();
        original.Parameters.Add(new TemplateParam { Name = "title", Type = "string", DefaultValue = "Report" });
        original.Parameters.Add(new TemplateParam { Name = "count", Type = "number" });
        original.Bands.Add(new Band { Type = BandType.Detail });

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        Assert.Equal(2, parsed.Parameters.Count);
        Assert.Equal("title", parsed.Parameters[0].Name);
        Assert.Equal("Report", parsed.Parameters[0].DefaultValue);
    }

    [Fact]
    public void RoundTrip_DataSources_Preserved()
    {
        var original = new ReportTemplate();
        original.DataSources.Add(new DataSourceDef { Name = "orders", Type = "sql" });
        original.Bands.Add(new Band { Type = BandType.Detail, DataSource = "orders" });

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        Assert.Single(parsed.DataSources);
        Assert.Equal("orders", parsed.DataSources[0].Name);
        Assert.Equal("sql", parsed.DataSources[0].Type);
    }

    // ============== 元素类型往返 ==============

    [Fact]
    public void RoundTrip_TextElement_Preserved()
    {
        var original = new ReportTemplate();
        var band = new Band { Type = BandType.Detail };
        band.Elements.Add(new TextElement
        {
            Text = "Hello",
            DataField = "Name",
            Format = "currency",
            Alignment = TextAlignment.Right,
            CanGrow = true,
            MaxLines = 3
        });
        original.Bands.Add(band);

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        var el = Assert.IsType<TextElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal("Hello", el.Text);
        Assert.Equal("Name", el.DataField);
        Assert.Equal("currency", el.Format);
        Assert.Equal(TextAlignment.Right, el.Alignment);
        Assert.True(el.CanGrow);
        Assert.Equal(3, el.MaxLines);
    }

    [Fact]
    public void RoundTrip_ImageElement_Preserved()
    {
        var original = new ReportTemplate();
        var band = new Band { Type = BandType.Detail };
        band.Elements.Add(new ImageElement
        {
            Source = "logo.png",
            Sizing = ImageSizing.Stretch,
            Width = 50,
            Height = 30
        });
        original.Bands.Add(band);

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        var el = Assert.IsType<ImageElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal("logo.png", el.Source);
        Assert.Equal(ImageSizing.Stretch, el.Sizing);
    }

    [Fact]
    public void RoundTrip_LineElement_Preserved()
    {
        var original = new ReportTemplate();
        var band = new Band { Type = BandType.Detail };
        band.Elements.Add(new LineElement
        {
            Direction = LineDirection.Vertical,
            LineWidth = 2,
            LineColor = "#FF0000"
        });
        original.Bands.Add(band);

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        var el = Assert.IsType<LineElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal(LineDirection.Vertical, el.Direction);
        Assert.Equal(2, el.LineWidth);
        Assert.Equal("#FF0000", el.LineColor);
    }

    [Fact]
    public void RoundTrip_ShapeElement_Preserved()
    {
        var original = new ReportTemplate();
        var band = new Band { Type = BandType.Detail };
        band.Elements.Add(new ShapeElement
        {
            Shape = ShapeType.Ellipse,
            FillColor = "#00FF00",
            BorderRadius = 10
        });
        original.Bands.Add(band);

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        var el = Assert.IsType<ShapeElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal(ShapeType.Ellipse, el.Shape);
        Assert.Equal("#00FF00", el.FillColor);
    }

    [Fact]
    public void RoundTrip_BarcodeElement_Preserved()
    {
        var original = new ReportTemplate();
        var band = new Band { Type = BandType.Detail };
        band.Elements.Add(new BarcodeElement
        {
            Value = "ABC123",
            Format = BarcodeFormat.Code128,
            ShowText = false
        });
        original.Bands.Add(band);

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        var el = Assert.IsType<BarcodeElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal("ABC123", el.Value);
        Assert.Equal(BarcodeFormat.Code128, el.Format);
        Assert.False(el.ShowText);
    }

    [Fact]
    public void RoundTrip_ChartElement_Preserved()
    {
        var original = new ReportTemplate();
        var band = new Band { Type = BandType.Detail };
        var chart = new ChartElement
        {
            ChartType = ChartType.Pie,
            DataSource = "sales",
            CategoryField = "Month",
            Title = "Sales Chart"
        };
        chart.Series.Add(new ChartSeries { Name = "Revenue", ValueField = "Amount" });
        band.Elements.Add(chart);
        original.Bands.Add(band);

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        var el = Assert.IsType<ChartElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal(ChartType.Pie, el.ChartType);
        Assert.Equal("Sales Chart", el.Title);
        Assert.Single(el.Series);
    }

    [Fact]
    public void RoundTrip_TableElement_Preserved()
    {
        var original = new ReportTemplate();
        var band = new Band { Type = BandType.Detail };
        var table = new TableElement
        {
            RowCount = 4,
            ColCount = 3,
            BorderWidth = 0.5,
            BorderColor = "#CCCCCC"
        };
        table.Cells.Add(new TableCell { Row = 0, Col = 0, Text = "Header", RowSpan = 1, ColSpan = 2 });
        band.Elements.Add(table);
        original.Bands.Add(band);

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        var el = Assert.IsType<TableElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal(4, el.RowCount);
        Assert.Equal(3, el.ColCount);
        Assert.Single(el.Cells);
        Assert.Equal(2, el.Cells[0].ColSpan);
    }

    [Fact]
    public void RoundTrip_SubReportElement_Preserved()
    {
        var original = new ReportTemplate();
        original.DataSources.Add(new DataSourceDef { Name = "orders" });
        var band = new Band { Type = BandType.Detail };
        var sub = new SubReportElement
        {
            TemplateRef = "detail.rptx",
            HeightMode = "fixed",
            RepeatPerRow = false
        };
        sub.DataBinding.Source = "orders";
        sub.DataBinding.ParamMap["parentId"] = "Id";
        band.Elements.Add(sub);
        original.Bands.Add(band);

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        var el = Assert.IsType<SubReportElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal("detail.rptx", el.TemplateRef);
        Assert.Equal("fixed", el.HeightMode);
        Assert.False(el.RepeatPerRow);
        Assert.Equal("orders", el.DataBinding.Source);
    }

    [Fact]
    public void RoundTrip_CrossTabElement_Preserved()
    {
        var original = new ReportTemplate();
        var band = new Band { Type = BandType.Detail };
        var ct = new CrossTabElement
        {
            DataSource = "sales",
            ShowRowTotal = false,
            ShowColumnTotal = false,
            CellPadding = 2
        };
        ct.RowFields.Add("Region");
        ct.ColumnFields.Add("Quarter");
        ct.Measures.Add(new CrossTabMeasure { Field = "Amount", Aggregate = "Sum" });
        band.Elements.Add(ct);
        original.Bands.Add(band);

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        var el = Assert.IsType<CrossTabElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal("sales", el.DataSource);
        Assert.False(el.ShowRowTotal);
        Assert.Single(el.RowFields);
        Assert.Single(el.Measures);
    }

    // ============== 多元素混合 ==============

    [Fact]
    public void RoundTrip_MixedElements_Preserved()
    {
        var original = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 50 };
        band.Elements.Add(new TextElement { Text = "Title" });
        band.Elements.Add(new ImageElement { Source = "logo.png" });
        band.Elements.Add(new LineElement { Direction = LineDirection.Horizontal });
        original.Bands.Add(band);

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        Assert.Equal(3, parsed.Bands[0].Elements.Count);
        Assert.IsType<TextElement>(parsed.Bands[0].Elements[0]);
        Assert.IsType<ImageElement>(parsed.Bands[0].Elements[1]);
        Assert.IsType<LineElement>(parsed.Bands[0].Elements[2]);
    }

    [Fact]
    public void RoundTrip_MultipleBands_Preserved()
    {
        var original = new ReportTemplate();
        original.Bands.Add(new Band { Type = BandType.ReportHeader, Height = 30 });
        original.Bands.Add(new Band { Type = BandType.Detail, Height = 20 });
        original.Bands.Add(new Band { Type = BandType.ReportFooter, Height = 25 });

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        Assert.Equal(3, parsed.Bands.Count);
        Assert.Equal(BandType.ReportHeader, parsed.Bands[0].Type);
        Assert.Equal(BandType.Detail, parsed.Bands[1].Type);
        Assert.Equal(BandType.ReportFooter, parsed.Bands[2].Type);
    }

    // ============== 元素基类属性 ==============

    [Fact]
    public void RoundTrip_ElementBaseProperties_Preserved()
    {
        var original = new ReportTemplate();
        var band = new Band { Type = BandType.Detail };
        band.Elements.Add(new TextElement
        {
            X = 10,
            Y = 20,
            Width = 100,
            Height = 25,
            BackgroundColor = "#EEEEEE",
            Locked = true,
            Rotation = 45,
            Opacity = 0.8
        });
        original.Bands.Add(band);

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        var el = Assert.IsType<TextElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal(10, el.X);
        Assert.Equal(20, el.Y);
        Assert.Equal(100, el.Width);
        Assert.Equal(25, el.Height);
        Assert.Equal("#EEEEEE", el.BackgroundColor);
        Assert.True(el.Locked);
        Assert.Equal(45, el.Rotation);
        Assert.Equal(0.8, el.Opacity);
    }

    [Fact]
    public void RoundTrip_ElementBorder_Preserved()
    {
        var original = new ReportTemplate();
        var band = new Band { Type = BandType.Detail };
        band.Elements.Add(new TextElement
        {
            Border = new BorderDef
            {
                Width = 2,
                Color = "#FF0000",
                Style = BorderStyle.Dashed,
                Top = true,
                Bottom = true
            }
        });
        original.Bands.Add(band);

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        var el = Assert.IsType<TextElement>(parsed.Bands[0].Elements[0]);
        Assert.NotNull(el.Border);
        Assert.Equal(2, el.Border.Width);
        Assert.Equal("#FF0000", el.Border.Color);
        Assert.Equal(BorderStyle.Dashed, el.Border.Style);
        Assert.True(el.Border.Top);
    }

    [Fact]
    public void RoundTrip_ConditionalFormats_Preserved()
    {
        var original = new ReportTemplate();
        var band = new Band { Type = BandType.Detail };
        var textEl = new TextElement { Text = "Amount" };
        textEl.ConditionalFormats.Add(new ConditionalFormatRule
        {
            Expression = "[Amount] > 1000",
            BackgroundColor = "#FF0000",
            FontColor = "#FFFFFF",
            Bold = true
        });
        band.Elements.Add(textEl);
        original.Bands.Add(band);

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        var el = Assert.IsType<TextElement>(parsed.Bands[0].Elements[0]);
        Assert.Single(el.ConditionalFormats);
        Assert.Equal("[Amount] > 1000", el.ConditionalFormats[0].Expression);
        Assert.True(el.ConditionalFormats[0].Bold);
    }

    [Fact]
    public void RoundTrip_FontDef_Preserved()
    {
        var original = new ReportTemplate();
        var band = new Band { Type = BandType.Detail };
        band.Elements.Add(new TextElement
        {
            Font = new FontDef
            {
                Family = "Arial",
                Size = 14,
                Bold = true,
                Italic = true,
                Underline = true,
                Color = "#333333"
            }
        });
        original.Bands.Add(band);

        var json = _parser.Serialize(original);
        var parsed = _parser.Parse(json);

        var el = Assert.IsType<TextElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal("Arial", el.Font.Family);
        Assert.Equal(14, el.Font.Size);
        Assert.True(el.Font.Bold);
        Assert.True(el.Font.Italic);
        Assert.True(el.Font.Underline);
        Assert.Equal("#333333", el.Font.Color);
    }
}

/// <summary>
/// TemplateParser 校验边界测试
/// </summary>
public class TemplateParserValidation2Tests
{
    private readonly TemplateParser _parser = new();

    // ============== 空 Bands ==============

    [Fact]
    public void Parse_EmptyBands_Throws()
    {
        var json = _parser.Serialize(new ReportTemplate());
        // 手动构造空 bands 的 JSON
        json = "{\"version\":\"1.0\",\"bands\":[]}";
        Assert.Throws<TemplateParseException>(() => _parser.Parse(json));
    }

    // ============== 未知数据源 ==============

    [Fact]
    public void Parse_UnknownBandDataSource_Throws()
    {
        var template = new ReportTemplate();
        template.Bands.Add(new Band { Type = BandType.Detail, DataSource = "nonexistent" });

        var json = _parser.Serialize(template);
        var ex = Assert.Throws<TemplateParseException>(() => _parser.Parse(json));
        Assert.Contains("nonexistent", ex.Message);
    }

    [Fact]
    public void Parse_UnknownSubReportDataSource_Throws()
    {
        var template = new ReportTemplate();
        var band = new Band { Type = BandType.Detail };
        var sub = new SubReportElement { TemplateRef = "sub.rptx" };
        sub.DataBinding.Source = "badSource";
        band.Elements.Add(sub);
        template.Bands.Add(band);

        var json = _parser.Serialize(template);
        var ex = Assert.Throws<TemplateParseException>(() => _parser.Parse(json));
        Assert.Contains("badSource", ex.Message);
    }

    // ============== 空 TemplateRef ==============

    [Fact]
    public void Parse_EmptyTemplateRef_Throws()
    {
        var template = new ReportTemplate();
        var band = new Band { Type = BandType.Detail };
        band.Elements.Add(new SubReportElement { TemplateRef = "" });
        template.Bands.Add(band);

        var json = _parser.Serialize(template);
        var ex = Assert.Throws<TemplateParseException>(() => _parser.Parse(json));
        Assert.Contains("TemplateRef", ex.Message);
    }

    // ============== 无效 JSON ==============

    [Fact]
    public void Parse_InvalidJson_Throws()
    {
        Assert.Throws<TemplateParseException>(() => _parser.Parse("not json"));
    }

    [Fact]
    public void Parse_EmptyString_Throws()
    {
        Assert.Throws<TemplateParseException>(() => _parser.Parse(""));
    }

    // ============== 合法模板不抛异常 ==============

    [Fact]
    public void Parse_ValidTemplate_DoesNotThrow()
    {
        var template = new ReportTemplate();
        template.DataSources.Add(new DataSourceDef { Name = "ds" });
        template.Bands.Add(new Band { Type = BandType.Detail, DataSource = "ds" });

        var json = _parser.Serialize(template);
        var result = _parser.Parse(json);
        Assert.NotNull(result);
    }

    [Fact]
    public void Parse_BandWithoutDataSource_DoesNotThrow()
    {
        var template = new ReportTemplate();
        template.Bands.Add(new Band { Type = BandType.Header });

        var json = _parser.Serialize(template);
        var result = _parser.Parse(json);
        Assert.NotNull(result);
    }

    // ============== ParseFile ==============

    [Fact]
    public void ParseFile_NonexistentFile_Throws()
    {
        Assert.Throws<FileNotFoundException>(() => _parser.ParseFile("nonexistent.rptx"));
    }
}

/// <summary>
/// TemplateParseException 测试
/// </summary>
public class TemplateExceptionTests
{
    // ============== TemplateParseException ==============

    [Fact]
    public void TemplateParseException_Message_Works()
    {
        var ex = new TemplateParseException("test error");
        Assert.Equal("test error", ex.Message);
    }

    [Fact]
    public void TemplateParseException_MessageAndInner_Works()
    {
        var inner = new Exception("inner");
        var ex = new TemplateParseException("outer", inner);
        Assert.Equal("outer", ex.Message);
        Assert.NotNull(ex.InnerException);
        Assert.Equal("inner", ex.InnerException.Message);
    }

    [Fact]
    public void TemplateParseException_IsException()
    {
        var ex = new TemplateParseException("test");
        Assert.IsAssignableFrom<Exception>(ex);
    }

    [Fact]
    public void TemplateParseException_ChineseMessage_Works()
    {
        var ex = new TemplateParseException("模板解析失败：无效的元素类型");
        Assert.Contains("模板解析失败", ex.Message);
    }

    [Fact]
    public void TemplateParseException_EmptyMessage_Works()
    {
        var ex = new TemplateParseException("");
        Assert.Equal("", ex.Message);
    }
}
