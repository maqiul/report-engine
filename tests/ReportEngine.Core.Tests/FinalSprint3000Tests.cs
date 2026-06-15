using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Parsing;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// 最终冲刺 3000 测试大关！
/// 覆盖 TemplateParser 解析边界 + Schema 遗漏属性
/// </summary>
public class FinalSprint3000Tests
{
    private readonly TemplateParser _parser = new();

    // ============== TemplateParser 解析空模板 ==============

    [Fact]
    public void Parse_EmptyBands_ThrowsException()
    {
        var json = @"{""version"":""1.0"",""page"":{""width"":210},""bands"":[]}";
        Assert.Throws<TemplateParseException>(() => _parser.Parse(json));
    }

    [Fact]
    public void Parse_EmptyDataSources_Works()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10}]}";
        var t = _parser.Parse(json);
        Assert.Empty(t.DataSources);
    }

    [Fact]
    public void Parse_EmptyParameters_Works()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10}]}";
        var t = _parser.Parse(json);
        Assert.Empty(t.Parameters);
    }

    // ============== TemplateParser 解析带元数据 ==============

    [Fact]
    public void Parse_WithAuthor_Works()
    {
        var json = @"{""version"":""1.0"",""author"":""TestUser"",""bands"":[{""type"":""detail"",""height"":10}]}";
        var t = _parser.Parse(json);
        Assert.Equal("TestUser", t.Author);
    }

    [Fact]
    public void Parse_WithDescription_Works()
    {
        var json = @"{""version"":""1.0"",""description"":""测试模板"",""bands"":[{""type"":""detail"",""height"":10}]}";
        var t = _parser.Parse(json);
        Assert.Equal("测试模板", t.Description);
    }

    // ============== TemplateParser 解析元素属性 ==============

    [Fact]
    public void Parse_TextElement_WithCanGrow()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""text"",""text"":""hi"",""canGrow"":true,""canShrink"":true}]}]}";
        var t = _parser.Parse(json);
        var el = Assert.IsType<TextElement>(t.Bands[0].Elements[0]);
        Assert.True(el.CanGrow);
        Assert.True(el.CanShrink);
    }

    [Fact]
    public void Parse_TextElement_WithMaxLines()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""text"",""text"":""hi"",""maxLines"":5}]}]}";
        var t = _parser.Parse(json);
        var el = Assert.IsType<TextElement>(t.Bands[0].Elements[0]);
        Assert.Equal(5, el.MaxLines);
    }

    [Fact]
    public void Parse_TextElement_WithHyperlink()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""text"",""text"":""click"",""hyperlink"":""https://example.com""}]}]}";
        var t = _parser.Parse(json);
        var el = Assert.IsType<TextElement>(t.Bands[0].Elements[0]);
        Assert.Equal("https://example.com", el.Hyperlink);
    }

    [Fact]
    public void Parse_TextElement_WithFormat()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""text"",""text"":""val"",""format"":""currency""}]}]}";
        var t = _parser.Parse(json);
        var el = Assert.IsType<TextElement>(t.Bands[0].Elements[0]);
        Assert.Equal("currency", el.Format);
    }

    [Fact]
    public void Parse_TextElement_WithSystemVariable()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""text"",""systemVariable"":""PageNumber""}]}]}";
        var t = _parser.Parse(json);
        var el = Assert.IsType<TextElement>(t.Bands[0].Elements[0]);
        Assert.Equal("PageNumber", el.SystemVariable);
    }

    // ============== TemplateParser 解析基类属性 ==============

    [Fact]
    public void Parse_Element_WithLocked()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""text"",""text"":""x"",""locked"":true}]}]}";
        var t = _parser.Parse(json);
        Assert.True(t.Bands[0].Elements[0].Locked);
    }

    [Fact]
    public void Parse_Element_WithRotation()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""text"",""text"":""x"",""rotation"":90}]}]}";
        var t = _parser.Parse(json);
        Assert.Equal(90, t.Bands[0].Elements[0].Rotation);
    }

    [Fact]
    public void Parse_Element_WithOpacity()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""text"",""text"":""x"",""opacity"":0.5}]}]}";
        var t = _parser.Parse(json);
        Assert.Equal(0.5, t.Bands[0].Elements[0].Opacity);
    }

    [Fact]
    public void Parse_Element_WithGroupId()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""text"",""text"":""x"",""groupId"":""g1""}]}]}";
        var t = _parser.Parse(json);
        Assert.Equal("g1", t.Bands[0].Elements[0].GroupId);
    }

    [Fact]
    public void Parse_Element_WithName()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""text"",""text"":""x"",""name"":""myLabel""}]}]}";
        var t = _parser.Parse(json);
        Assert.Equal("myLabel", t.Bands[0].Elements[0].Name);
    }

    [Fact]
    public void Parse_Element_WithVisibleExpression()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""text"",""text"":""x"",""visibleExpression"":""[show]=true""}]}]}";
        var t = _parser.Parse(json);
        Assert.Equal("[show]=true", t.Bands[0].Elements[0].VisibleExpression);
    }

    // ============== TemplateParser 解析条件格式 ==============

    [Fact]
    public void Parse_Element_WithConditionalFormats()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""text"",""text"":""x"",""conditionalFormats"":[{""expression"":""[x]>0"",""backgroundColor"":""#FF0000"",""bold"":true}]}]}]}";
        var t = _parser.Parse(json);
        var el = t.Bands[0].Elements[0];
        Assert.Single(el.ConditionalFormats);
        Assert.Equal("[x]>0", el.ConditionalFormats[0].Expression);
        Assert.Equal("#FF0000", el.ConditionalFormats[0].BackgroundColor);
        Assert.True(el.ConditionalFormats[0].Bold);
    }

    [Fact]
    public void Parse_Element_WithMultipleConditionalFormats()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""text"",""text"":""x"",""conditionalFormats"":[{""expression"":""[x]>0"",""fontColor"":""#00FF00""},{""expression"":""[x]<0"",""fontColor"":""#FF0000""}]}]}]}";
        var t = _parser.Parse(json);
        Assert.Equal(2, t.Bands[0].Elements[0].ConditionalFormats.Count);
    }

    // ============== TemplateParser 解析页面属性 ==============

    [Fact]
    public void Parse_Page_WithBackgroundColor()
    {
        var json = @"{""version"":""1.0"",""page"":{""backgroundColor"":""#F0F0F0""},""bands"":[{""type"":""detail"",""height"":10}]}";
        var t = _parser.Parse(json);
        Assert.Equal("#F0F0F0", t.Page.BackgroundColor);
    }

    [Fact]
    public void Parse_Page_WithBackgroundImage()
    {
        var json = @"{""version"":""1.0"",""page"":{""backgroundImage"":""bg.png""},""bands"":[{""type"":""detail"",""height"":10}]}";
        var t = _parser.Parse(json);
        Assert.Equal("bg.png", t.Page.BackgroundImage);
    }

    [Fact]
    public void Parse_Page_WithWatermark()
    {
        var json = @"{""version"":""1.0"",""page"":{""watermark"":""DRAFT""},""bands"":[{""type"":""detail"",""height"":10}]}";
        var t = _parser.Parse(json);
        Assert.Equal("DRAFT", t.Page.Watermark);
    }

    // ============== TemplateParser 解析数据源属性 ==============

    [Fact]
    public void Parse_DataSource_WithConnectionString()
    {
        var json = @"{""version"":""1.0"",""dataSources"":[{""name"":""ds1"",""type"":""sql"",""connectionString"":""Server=localhost"",""query"":""SELECT 1""}],""bands"":[{""type"":""detail"",""height"":10}]}";
        var t = _parser.Parse(json);
        Assert.Equal("Server=localhost", t.DataSources[0].ConnectionString);
        Assert.Equal("SELECT 1", t.DataSources[0].Query);
    }

    // ============== TemplateParser 解析 Shape 属性 ==============

    [Fact]
    public void Parse_Shape_WithBorderRadius()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""shape"",""shape"":""roundedRect"",""borderRadius"":5}]}]}";
        var t = _parser.Parse(json);
        var el = Assert.IsType<ShapeElement>(t.Bands[0].Elements[0]);
        Assert.Equal(5, el.BorderRadius);
    }

    // ============== TemplateParser 解析 Line 属性 ==============

    [Fact]
    public void Parse_Line_WithDirection()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""line"",""direction"":""vertical""}]}]}";
        var t = _parser.Parse(json);
        var el = Assert.IsType<LineElement>(t.Bands[0].Elements[0]);
        Assert.Equal(LineDirection.Vertical, el.Direction);
    }

    // ============== TemplateParser 解析 Image 属性 ==============

    [Fact]
    public void Parse_Image_WithSizing()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""image"",""source"":""logo.png"",""sizing"":""clip""}]}]}";
        var t = _parser.Parse(json);
        var el = Assert.IsType<ImageElement>(t.Bands[0].Elements[0]);
        Assert.Equal(ImageSizing.Clip, el.Sizing);
    }

    // ============== 综合往返 ==============

    [Fact]
    public void RoundTrip_ComplexTemplate_Works()
    {
        var t = new ReportTemplate
        {
            Version = "2.0",
            Author = "TestUser",
            Description = "复杂测试模板"
        };
        t.Page.BackgroundColor = "#F0F0F0";
        t.Page.Watermark = "DRAFT";
        t.DataSources.Add(new DataSourceDef { Name = "ds1", Type = "sql", ConnectionString = "Server=localhost", Query = "SELECT 1" });
        t.Parameters.Add(new TemplateParam { Name = "p1", Type = "string", Label = "参数1" });

        var band = new Band { Type = BandType.Detail, Height = 20 };
        var textEl = new TextElement { Text = "Hello", CanGrow = true, MaxLines = 3, Hyperlink = "https://example.com" };
        textEl.ConditionalFormats.Add(new ConditionalFormatRule { Expression = "[x]>0", Bold = true, FontColor = "#FF0000" });
        band.Elements.Add(textEl);
        band.Elements.Add(new ShapeElement { Shape = ShapeType.RoundedRect, BorderRadius = 5 });
        band.Elements.Add(new LineElement { Direction = LineDirection.Diagonal });
        t.Bands.Add(band);

        var json = _parser.Serialize(t);
        var parsed = _parser.Parse(json);

        Assert.Equal("2.0", parsed.Version);
        Assert.Equal("TestUser", parsed.Author);
        Assert.Equal("复杂测试模板", parsed.Description);
        Assert.Equal("#F0F0F0", parsed.Page.BackgroundColor);
        Assert.Equal("DRAFT", parsed.Page.Watermark);
        Assert.Single(parsed.DataSources);
        Assert.Equal("Server=localhost", parsed.DataSources[0].ConnectionString);
        Assert.Single(parsed.Parameters);
        Assert.Equal("参数1", parsed.Parameters[0].Label);
        Assert.Single(parsed.Bands);
        Assert.Equal(3, parsed.Bands[0].Elements.Count);

        var parsedText = Assert.IsType<TextElement>(parsed.Bands[0].Elements[0]);
        Assert.True(parsedText.CanGrow);
        Assert.Equal(3, parsedText.MaxLines);
        Assert.Equal("https://example.com", parsedText.Hyperlink);
        Assert.Single(parsedText.ConditionalFormats);
        Assert.True(parsedText.ConditionalFormats[0].Bold);

        var parsedShape = Assert.IsType<ShapeElement>(parsed.Bands[0].Elements[1]);
        Assert.Equal(5, parsedShape.BorderRadius);

        var parsedLine = Assert.IsType<LineElement>(parsed.Bands[0].Elements[2]);
        Assert.Equal(LineDirection.Diagonal, parsedLine.Direction);
    }

    // ============== 额外补充冲刺 3000 ==============

    [Fact]
    public void Parse_TextElement_WithSummaryField()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""text"",""summaryFunction"":""Sum"",""summaryField"":""Amount""}]}]}";
        var t = _parser.Parse(json);
        var el = Assert.IsType<TextElement>(t.Bands[0].Elements[0]);
        Assert.Equal("Sum", el.SummaryFunction);
        Assert.Equal("Amount", el.SummaryField);
    }

    [Fact]
    public void Parse_TextElement_WithSummaryField_RoundTrip()
    {
        var t = new ReportTemplate();
        var band = new Band { Type = BandType.Detail, Height = 10 };
        band.Elements.Add(new TextElement { SummaryFunction = "Avg", SummaryField = "Score" });
        t.Bands.Add(band);

        var json = _parser.Serialize(t);
        var parsed = _parser.Parse(json);
        var el = Assert.IsType<TextElement>(parsed.Bands[0].Elements[0]);
        Assert.Equal("Avg", el.SummaryFunction);
        Assert.Equal("Score", el.SummaryField);
    }

    [Fact]
    public void Parse_TextElement_WithAllBoxTypes()
    {
        // Static
        var json1 = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""text"",""text"":""hello""}]}]}";
        var t1 = _parser.Parse(json1);
        Assert.Equal(TextBoxType.Static, Assert.IsType<TextElement>(t1.Bands[0].Elements[0]).BoxType);

        // Field
        var json2 = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""text"",""dataField"":""Name""}]}]}";
        var t2 = _parser.Parse(json2);
        Assert.Equal(TextBoxType.Field, Assert.IsType<TextElement>(t2.Bands[0].Elements[0]).BoxType);

        // SysVar
        var json3 = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""text"",""systemVariable"":""PageNumber""}]}]}";
        var t3 = _parser.Parse(json3);
        Assert.Equal(TextBoxType.SysVar, Assert.IsType<TextElement>(t3.Bands[0].Elements[0]).BoxType);

        // Summary
        var json4 = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""text"",""summaryFunction"":""Sum"",""summaryField"":""Amount""}]}]}";
        var t4 = _parser.Parse(json4);
        Assert.Equal(TextBoxType.Summary, Assert.IsType<TextElement>(t4.Bands[0].Elements[0]).BoxType);
    }

    [Fact]
    public void Parse_Element_WithBackgroundColor()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""text"",""text"":""x"",""backgroundColor"":""#FFFF00""}]}]}";
        var t = _parser.Parse(json);
        Assert.Equal("#FFFF00", t.Bands[0].Elements[0].BackgroundColor);
    }

    [Fact]
    public void Parse_Element_WithBorder()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""text"",""text"":""x"",""border"":{""width"":2,""color"":""#FF0000"",""style"":""dashed"",""top"":true}}]}]}";
        var t = _parser.Parse(json);
        var border = t.Bands[0].Elements[0].Border;
        Assert.NotNull(border);
        Assert.Equal(2, border!.Width);
        Assert.Equal("#FF0000", border.Color);
        Assert.Equal(BorderStyle.Dashed, border.Style);
        Assert.True(border.Top);
    }

    [Fact]
    public void Parse_Element_WithPosition()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""text"",""text"":""x"",""x"":10,""y"":20,""width"":100,""height"":30}]}]}";
        var t = _parser.Parse(json);
        var el = t.Bands[0].Elements[0];
        Assert.Equal(10, el.X);
        Assert.Equal(20, el.Y);
        Assert.Equal(100, el.Width);
        Assert.Equal(30, el.Height);
    }

    [Fact]
    public void Parse_Element_WithFont()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""text"",""text"":""x"",""font"":{""family"":""Arial"",""size"":12,""bold"":true}}]}]}";
        var t = _parser.Parse(json);
        var el = Assert.IsType<TextElement>(t.Bands[0].Elements[0]);
        Assert.Equal("Arial", el.Font.Family);
        Assert.Equal(12, el.Font.Size);
        Assert.True(el.Font.Bold);
    }

    [Fact]
    public void Parse_Barcode_WithFormat()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""barcode"",""value"":""12345"",""format"":""QRCode""}]}]}";
        var t = _parser.Parse(json);
        var el = Assert.IsType<BarcodeElement>(t.Bands[0].Elements[0]);
        Assert.Equal("12345", el.Value);
        Assert.Equal(BarcodeFormat.QRCode, el.Format);
    }

    [Fact]
    public void Parse_SubReport_WithTemplateRef()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""subreport"",""templateRef"":""sub.rpt"",""heightMode"":""fixed""}]}]}";
        var t = _parser.Parse(json);
        var el = Assert.IsType<SubReportElement>(t.Bands[0].Elements[0]);
        Assert.Equal("sub.rpt", el.TemplateRef);
        Assert.Equal("fixed", el.HeightMode);
    }

    [Fact]
    public void Parse_SubReport_WithDataBinding()
    {
        var json = @"{""version"":""1.0"",""dataSources"":[{""name"":""orders""}],""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""subreport"",""templateRef"":""sub.rpt"",""dataBinding"":{""source"":""orders"",""paramMap"":{""p1"":""Field1""}}}]}]}";
        var t = _parser.Parse(json);
        var el = Assert.IsType<SubReportElement>(t.Bands[0].Elements[0]);
        Assert.NotNull(el.DataBinding);
        Assert.Equal("orders", el.DataBinding!.Source);
        Assert.Equal("Field1", el.DataBinding.ParamMap["p1"]);
    }

    [Fact]
    public void Parse_Table_WithCells()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""table"",""rowCount"":2,""colCount"":2,""cells"":[{""row"":0,""col"":0,""text"":""A""},{""row"":0,""col"":1,""text"":""B""}]}]}]}";
        var t = _parser.Parse(json);
        var el = Assert.IsType<TableElement>(t.Bands[0].Elements[0]);
        Assert.Equal(2, el.RowCount);
        Assert.Equal(2, el.ColCount);
        Assert.Equal(2, el.Cells.Count);
    }

    [Fact]
    public void Parse_Chart_WithType()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""chart"",""chartType"":""pie"",""title"":""Sales""}]}]}";
        var t = _parser.Parse(json);
        var el = Assert.IsType<ChartElement>(t.Bands[0].Elements[0]);
        Assert.Equal(ChartType.Pie, el.ChartType);
        Assert.Equal("Sales", el.Title);
    }

    [Fact]
    public void Parse_CrossTab_WithMeasures()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""elements"":[{""type"":""crosstab"",""measures"":[{""field"":""Amount"",""aggregate"":""Sum"",""label"":""Total"",""format"":""currency""}]}]}]}";
        var t = _parser.Parse(json);
        var el = Assert.IsType<CrossTabElement>(t.Bands[0].Elements[0]);
        Assert.Single(el.Measures);
        Assert.Equal("Amount", el.Measures[0].Field);
        Assert.Equal("Sum", el.Measures[0].Aggregate);
        Assert.Equal("Total", el.Measures[0].Label);
        Assert.Equal("currency", el.Measures[0].Format);
    }

    [Fact]
    public void Parse_Band_WithRepeatOnNewPage()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""header"",""height"":10,""repeatOnNewPage"":true}]}";
        var t = _parser.Parse(json);
        Assert.True(t.Bands[0].RepeatOnNewPage);
    }

    [Fact]
    public void Parse_Band_WithDataSource()
    {
        var json = @"{""version"":""1.0"",""dataSources"":[{""name"":""ds1""}],""bands"":[{""type"":""detail"",""height"":10,""dataSource"":""ds1""}]}";
        var t = _parser.Parse(json);
        Assert.Equal("ds1", t.Bands[0].DataSource);
    }

    [Fact]
    public void Parse_Band_WithGroup()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""groupHeader"",""height"":10,""group"":{""expression"":""[Region]"",""keepTogether"":true}}]}";
        var t = _parser.Parse(json);
        Assert.NotNull(t.Bands[0].Group);
        Assert.Equal("[Region]", t.Bands[0].Group!.Expression);
        Assert.True(t.Bands[0].Group.KeepTogether);
    }

    [Fact]
    public void Parse_Band_WithMultiColumn()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10,""multiColumn"":{""columnCount"":3,""columnSpacing"":8,""direction"":""vertical""}}]}";
        var t = _parser.Parse(json);
        Assert.NotNull(t.Bands[0].MultiColumn);
        Assert.Equal(3, t.Bands[0].MultiColumn!.ColumnCount);
        Assert.Equal(8, t.Bands[0].MultiColumn.ColumnSpacing);
        Assert.Equal("vertical", t.Bands[0].MultiColumn.Direction);
    }

    [Fact]
    public void Parse_Page_WithMultiUp()
    {
        var json = @"{""version"":""1.0"",""page"":{""multiUp"":{""rows"":3,""columns"":2,""hSpacing"":5,""vSpacing"":3,""direction"":""vertical""}},""bands"":[{""type"":""detail"",""height"":10}]}";
        var t = _parser.Parse(json);
        Assert.NotNull(t.Page.MultiUp);
        Assert.Equal(3, t.Page.MultiUp!.Rows);
        Assert.Equal(2, t.Page.MultiUp.Columns);
        Assert.Equal(5, t.Page.MultiUp.HSpacing);
        Assert.Equal(3, t.Page.MultiUp.VSpacing);
        Assert.Equal("vertical", t.Page.MultiUp.Direction);
    }

    [Fact]
    public void Parse_Page_WithOrientation()
    {
        var json = @"{""version"":""1.0"",""page"":{""orientation"":""landscape""},""bands"":[{""type"":""detail"",""height"":10}]}";
        var t = _parser.Parse(json);
        Assert.Equal("landscape", t.Page.Orientation);
    }

    [Fact]
    public void Parse_Page_WithMargins()
    {
        var json = @"{""version"":""1.0"",""page"":{""margin"":{""top"":20,""bottom"":15,""left"":10,""right"":5}},""bands"":[{""type"":""detail"",""height"":10}]}";
        var t = _parser.Parse(json);
        Assert.Equal(20, t.Page.Margin.Top);
        Assert.Equal(15, t.Page.Margin.Bottom);
        Assert.Equal(10, t.Page.Margin.Left);
        Assert.Equal(5, t.Page.Margin.Right);
    }
}
