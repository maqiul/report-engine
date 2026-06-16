using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Parsing;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// TemplateParser 解析更多边界测试
/// </summary>
public class TemplateParserBoundary2Tests
{
    private readonly TemplateParser _parser = new();

    // ============== 解析空/最小模板 ==============

    [Fact]
    public void Parse_MinimalTemplate_Works()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":10}]}";
        var t = _parser.Parse(json);
        Assert.NotNull(t);
        Assert.Single(t.Bands);
    }

    [Fact]
    public void Parse_VersionPreserved()
    {
        var json = @"{""version"":""2.5"",""bands"":[{""type"":""detail"",""height"":10}]}";
        var t = _parser.Parse(json);
        Assert.Equal("2.5", t.Version);
    }

    // ============== 解析多 Band ==============

    [Fact]
    public void Parse_MultipleBands_AllPreserved()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""header"",""height"":20},{""type"":""detail"",""height"":30},{""type"":""footer"",""height"":15}]}";
        var t = _parser.Parse(json);
        Assert.Equal(3, t.Bands.Count);
        Assert.Equal(BandType.Header, t.Bands[0].Type);
        Assert.Equal(BandType.Detail, t.Bands[1].Type);
        Assert.Equal(BandType.Footer, t.Bands[2].Type);
    }

    [Fact]
    public void Parse_BandHeights_Preserved()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""header"",""height"":25.5},{""type"":""detail"",""height"":40.3}]}";
        var t = _parser.Parse(json);
        Assert.Equal(25.5, t.Bands[0].Height);
        Assert.Equal(40.3, t.Bands[1].Height);
    }

    // ============== 解析多数据源 ==============

    [Fact]
    public void Parse_MultipleDataSources_AllPreserved()
    {
        var json = @"{""version"":""1.0"",""dataSources"":[{""name"":""ds1"",""type"":""json""},{""name"":""ds2"",""type"":""sql""},{""name"":""ds3"",""type"":""csv""}],""bands"":[{""type"":""detail"",""height"":10}]}";
        var t = _parser.Parse(json);
        Assert.Equal(3, t.DataSources.Count);
        Assert.Equal("ds1", t.DataSources[0].Name);
        Assert.Equal("ds2", t.DataSources[1].Name);
        Assert.Equal("ds3", t.DataSources[2].Name);
    }

    [Fact]
    public void Parse_DataSourceTypes_Preserved()
    {
        var json = @"{""version"":""1.0"",""dataSources"":[{""name"":""ds1"",""type"":""api""}],""bands"":[{""type"":""detail"",""height"":10}]}";
        var t = _parser.Parse(json);
        Assert.Equal("api", t.DataSources[0].Type);
    }

    // ============== 解析多参数 ==============

    [Fact]
    public void Parse_MultipleParameters_AllPreserved()
    {
        var json = @"{""version"":""1.0"",""parameters"":[{""name"":""p1"",""type"":""string"",""defaultValue"":""a""},{""name"":""p2"",""type"":""number"",""defaultValue"":""100""},{""name"":""p3"",""type"":""date"",""defaultValue"":""2025-01-01""}],""bands"":[{""type"":""detail"",""height"":10}]}";
        var t = _parser.Parse(json);
        Assert.Equal(3, t.Parameters.Count);
        Assert.Equal("p1", t.Parameters[0].Name);
        Assert.Equal("string", t.Parameters[0].Type);
        Assert.Equal("a", t.Parameters[0].DefaultValue);
    }

    [Fact]
    public void Parse_ParameterLabel_Preserved()
    {
        var json = @"{""version"":""1.0"",""parameters"":[{""name"":""startDate"",""label"":""开始日期""}],""bands"":[{""type"":""detail"",""height"":10}]}";
        var t = _parser.Parse(json);
        Assert.Equal("开始日期", t.Parameters[0].Label);
    }

    // ============== 解析页面配置 ==============

    [Fact]
    public void Parse_PageDimensions_Preserved()
    {
        var json = @"{""version"":""1.0"",""page"":{""width"":297,""height"":210},""bands"":[{""type"":""detail"",""height"":10}]}";
        var t = _parser.Parse(json);
        Assert.Equal(297, t.Page.Width);
        Assert.Equal(210, t.Page.Height);
    }

    [Fact]
    public void Parse_PageMargins_Preserved()
    {
        var json = @"{""version"":""1.0"",""page"":{""margin"":{""top"":15,""bottom"":20,""left"":10,""right"":5}},""bands"":[{""type"":""detail"",""height"":10}]}";
        var t = _parser.Parse(json);
        Assert.Equal(15, t.Page.Margin.Top);
        Assert.Equal(20, t.Page.Margin.Bottom);
        Assert.Equal(10, t.Page.Margin.Left);
        Assert.Equal(5, t.Page.Margin.Right);
    }

    [Fact]
    public void Parse_PageOrientation_Preserved()
    {
        var json = @"{""version"":""1.0"",""page"":{""orientation"":""landscape""},""bands"":[{""type"":""detail"",""height"":10}]}";
        var t = _parser.Parse(json);
        Assert.Equal("landscape", t.Page.Orientation);
    }

    [Fact]
    public void Parse_PageUnit_Preserved()
    {
        var json = @"{""version"":""1.0"",""page"":{""unit"":""inch""},""bands"":[{""type"":""detail"",""height"":10}]}";
        var t = _parser.Parse(json);
        Assert.Equal("inch", t.Page.Unit);
    }

    // ============== 解析元素位置 ==============

    [Fact]
    public void Parse_ElementPosition_Preserved()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":50,""elements"":[{""type"":""text"",""text"":""Hello"",""x"":10.5,""y"":20.3,""width"":100.7,""height"":15.2}]}]}";
        var t = _parser.Parse(json);
        var el = t.Bands[0].Elements[0];
        Assert.Equal(10.5, el.X);
        Assert.Equal(20.3, el.Y);
        Assert.Equal(100.7, el.Width);
        Assert.Equal(15.2, el.Height);
    }

    // ============== 解析 TextElement 完整属性 ==============

    [Fact]
    public void Parse_TextElement_AllProperties()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":50,""elements"":[{""type"":""text"",""text"":""Hello"",""dataField"":""name"",""format"":""currency"",""alignment"":""right"",""canGrow"":true,""canShrink"":true,""maxLines"":5}]}]}";
        var t = _parser.Parse(json);
        var el = Assert.IsType<TextElement>(t.Bands[0].Elements[0]);
        Assert.Equal("Hello", el.Text);
        Assert.Equal("name", el.DataField);
        Assert.Equal("currency", el.Format);
        Assert.Equal(TextAlignment.Right, el.Alignment);
        Assert.True(el.CanGrow);
        Assert.True(el.CanShrink);
        Assert.Equal(5, el.MaxLines);
    }

    [Fact]
    public void Parse_TextElement_FontPreserved()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":50,""elements"":[{""type"":""text"",""text"":""Hello"",""font"":{""family"":""Arial"",""size"":14,""bold"":true,""italic"":true,""color"":""#FF0000""}}]}]}";
        var t = _parser.Parse(json);
        var el = Assert.IsType<TextElement>(t.Bands[0].Elements[0]);
        Assert.Equal("Arial", el.Font.Family);
        Assert.Equal(14, el.Font.Size);
        Assert.True(el.Font.Bold);
        Assert.True(el.Font.Italic);
        Assert.Equal("#FF0000", el.Font.Color);
    }

    // ============== 解析 ImageElement ==============

    [Fact]
    public void Parse_ImageElement_AllProperties()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":50,""elements"":[{""type"":""image"",""source"":""logo.png"",""sizing"":""stretch""}]}]}";
        var t = _parser.Parse(json);
        var el = Assert.IsType<ImageElement>(t.Bands[0].Elements[0]);
        Assert.Equal("logo.png", el.Source);
        Assert.Equal(ImageSizing.Stretch, el.Sizing);
    }

    // ============== 解析 LineElement ==============

    [Fact]
    public void Parse_LineElement_AllProperties()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":50,""elements"":[{""type"":""line"",""direction"":""vertical"",""lineWidth"":2.5,""lineColor"":""#0000FF""}]}]}";
        var t = _parser.Parse(json);
        var el = Assert.IsType<LineElement>(t.Bands[0].Elements[0]);
        Assert.Equal(LineDirection.Vertical, el.Direction);
        Assert.Equal(2.5, el.LineWidth);
        Assert.Equal("#0000FF", el.LineColor);
    }

    // ============== 解析 ShapeElement ==============

    [Fact]
    public void Parse_ShapeElement_AllProperties()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":50,""elements"":[{""type"":""shape"",""shape"":""ellipse"",""fillColor"":""#00FF00"",""borderRadius"":10}]}]}";
        var t = _parser.Parse(json);
        var el = Assert.IsType<ShapeElement>(t.Bands[0].Elements[0]);
        Assert.Equal(ShapeType.Ellipse, el.Shape);
        Assert.Equal("#00FF00", el.FillColor);
        Assert.Equal(10, el.BorderRadius);
    }

    // ============== 解析 BarcodeElement ==============

    [Fact]
    public void Parse_BarcodeElement_AllProperties()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":50,""elements"":[{""type"":""barcode"",""value"":""12345"",""format"":""QRCode"",""showText"":false}]}]}";
        var t = _parser.Parse(json);
        var el = Assert.IsType<BarcodeElement>(t.Bands[0].Elements[0]);
        Assert.Equal("12345", el.Value);
        Assert.Equal(BarcodeFormat.QRCode, el.Format);
        Assert.False(el.ShowText);
    }

    // ============== 解析 ChartElement ==============

    [Fact]
    public void Parse_ChartElement_AllProperties()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":50,""elements"":[{""type"":""chart"",""chartType"":""pie"",""title"":""Sales Chart""}]}]}";
        var t = _parser.Parse(json);
        var el = Assert.IsType<ChartElement>(t.Bands[0].Elements[0]);
        Assert.Equal(ChartType.Pie, el.ChartType);
        Assert.Equal("Sales Chart", el.Title);
    }

    // ============== 解析 TableElement ==============

    [Fact]
    public void Parse_TableElement_AllProperties()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":50,""elements"":[{""type"":""table"",""rowCount"":3,""colCount"":4,""borderWidth"":1.5,""borderColor"":""#999999"",""columnWidths"":[50,60,70,80],""rowHeights"":[20,25,30]}]}]}";
        var t = _parser.Parse(json);
        var el = Assert.IsType<TableElement>(t.Bands[0].Elements[0]);
        Assert.Equal(3, el.RowCount);
        Assert.Equal(4, el.ColCount);
        Assert.Equal(1.5, el.BorderWidth);
        Assert.Equal("#999999", el.BorderColor);
        Assert.Equal(4, el.ColumnWidths.Count);
        Assert.Equal(3, el.RowHeights.Count);
    }

    [Fact]
    public void Parse_TableElement_WithCells()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":50,""elements"":[{""type"":""table"",""rowCount"":2,""colCount"":2,""cells"":[{""row"":0,""col"":0,""text"":""A1"",""rowSpan"":2},{""row"":0,""col"":1,""text"":""B1"",""colSpan"":1}]}]}]}";
        var t = _parser.Parse(json);
        var el = Assert.IsType<TableElement>(t.Bands[0].Elements[0]);
        Assert.Equal(2, el.Cells.Count);
        Assert.Equal("A1", el.Cells[0].Text);
        Assert.Equal(2, el.Cells[0].RowSpan);
    }

    // ============== 解析 CrossTabElement ==============

    [Fact]
    public void Parse_CrossTabElement_AllProperties()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":50,""elements"":[{""type"":""crosstab"",""dataSource"":""sales"",""rowFields"":[""Region"",""Product""],""columnFields"":[""Year""],""measures"":[{""field"":""Amount"",""aggregate"":""Sum"",""label"":""Total"",""format"":""currency""}],""showRowTotal"":true,""showColumnTotal"":false,""cellPadding"":2.0,""borderWidth"":0.5}]}]}";
        var t = _parser.Parse(json);
        var el = Assert.IsType<CrossTabElement>(t.Bands[0].Elements[0]);
        Assert.Equal("sales", el.DataSource);
        Assert.Equal(2, el.RowFields.Count);
        Assert.Single(el.ColumnFields);
        Assert.Single(el.Measures);
        Assert.Equal("Amount", el.Measures[0].Field);
        Assert.Equal("Sum", el.Measures[0].Aggregate);
        Assert.True(el.ShowRowTotal);
        Assert.False(el.ShowColumnTotal);
        Assert.Equal(2.0, el.CellPadding);
        Assert.Equal(0.5, el.BorderWidth);
    }

    // ============== 解析 SubReportElement ==============

    [Fact]
    public void Parse_SubReportElement_AllProperties()
    {
        var json = @"{""version"":""1.0"",""dataSources"":[{""name"":""orders""}],""bands"":[{""type"":""detail"",""height"":50,""elements"":[{""type"":""subreport"",""templateRef"":""sub.rpt"",""heightMode"":""fixed"",""repeatPerRow"":true,""dataBinding"":{""source"":""orders"",""paramMap"":{""customerId"":""Id""}}}]}]}";
        var t = _parser.Parse(json);
        var el = Assert.IsType<SubReportElement>(t.Bands[0].Elements[0]);
        Assert.Equal("sub.rpt", el.TemplateRef);
        Assert.Equal("fixed", el.HeightMode);
        Assert.True(el.RepeatPerRow);
        Assert.NotNull(el.DataBinding);
        Assert.Equal("orders", el.DataBinding!.Source);
        Assert.Equal("Id", el.DataBinding.ParamMap["customerId"]);
    }

    // ============== 解析 Border ==============

    [Fact]
    public void Parse_Element_WithBorder()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":50,""elements"":[{""type"":""text"",""text"":""Hello"",""border"":{""width"":2,""color"":""#FF0000"",""style"":""dashed"",""top"":true,""bottom"":true,""left"":false,""right"":false}}]}]}";
        var t = _parser.Parse(json);
        var border = t.Bands[0].Elements[0].Border;
        Assert.NotNull(border);
        Assert.Equal(2, border!.Width);
        Assert.Equal("#FF0000", border.Color);
        Assert.Equal(BorderStyle.Dashed, border.Style);
        Assert.True(border.Top);
        Assert.True(border.Bottom);
        Assert.False(border.Left);
        Assert.False(border.Right);
    }

    // ============== 解析 ConditionalFormats ==============

    [Fact]
    public void Parse_Element_WithMultipleConditionalFormats()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":50,""elements"":[{""type"":""text"",""text"":""Hello"",""conditionalFormats"":[{""expression"":""[x]>100"",""backgroundColor"":""#FF0000"",""bold"":true},{""expression"":""[x]<0"",""fontColor"":""#0000FF"",""italic"":true}]}]}]}";
        var t = _parser.Parse(json);
        var el = t.Bands[0].Elements[0];
        Assert.Equal(2, el.ConditionalFormats.Count);
        Assert.Equal("[x]>100", el.ConditionalFormats[0].Expression);
        Assert.Equal("#FF0000", el.ConditionalFormats[0].BackgroundColor);
        Assert.True(el.ConditionalFormats[0].Bold);
    }

    // ============== 解析 Band 配置 ==============

    [Fact]
    public void Parse_Band_WithMultiColumn()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""detail"",""height"":50,""multiColumn"":{""columnCount"":3,""columnSpacing"":10,""direction"":""vertical""}}]}";
        var t = _parser.Parse(json);
        Assert.NotNull(t.Bands[0].MultiColumn);
        Assert.Equal(3, t.Bands[0].MultiColumn!.ColumnCount);
        Assert.Equal(10, t.Bands[0].MultiColumn!.ColumnSpacing);
        Assert.Equal("vertical", t.Bands[0].MultiColumn!.Direction);
    }

    [Fact]
    public void Parse_Band_WithGroup()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""groupHeader"",""height"":20,""group"":{""expression"":""[Region]"",""keepTogether"":false}}]}";
        var t = _parser.Parse(json);
        Assert.NotNull(t.Bands[0].Group);
        Assert.Equal("[Region]", t.Bands[0].Group!.Expression);
        Assert.False(t.Bands[0].Group!.KeepTogether);
    }

    [Fact]
    public void Parse_Band_RepeatOnNewPage()
    {
        var json = @"{""version"":""1.0"",""bands"":[{""type"":""header"",""height"":20,""repeatOnNewPage"":true}]}";
        var t = _parser.Parse(json);
        Assert.True(t.Bands[0].RepeatOnNewPage);
    }

    // ============== 综合往返 ==============

    [Fact]
    public void RoundTrip_ComplexTemplate_AllPreserved()
    {
        var t = new ReportTemplate
        {
            Version = "3.0",
            Author = "TestAuthor",
            Description = "Test Description"
        };

        t.Page.Width = 297;
        t.Page.Height = 210;
        t.Page.Orientation = "landscape";
        t.Page.Margin = new Margin { Top = 15, Bottom = 20, Left = 10, Right = 5 };
        t.Page.BackgroundColor = "#F0F0F0";
        t.Page.Watermark = "DRAFT";

        t.DataSources.Add(new DataSourceDef { Name = "ds1", Type = "sql", ConnectionString = "Server=localhost", Query = "SELECT * FROM orders" });
        t.Parameters.Add(new TemplateParam { Name = "startDate", Type = "date", DefaultValue = "2025-01-01", Label = "开始日期" });

        var headerBand = new Band { Type = BandType.Header, Height = 30, RepeatOnNewPage = true };
        headerBand.Elements.Add(new TextElement { Text = "Report Title", Font = new FontDef { Family = "Arial", Size = 16, Bold = true } });
        t.Bands.Add(headerBand);

        var detailBand = new Band { Type = BandType.Detail, Height = 20, DataSource = "ds1" };
        detailBand.Elements.Add(new TextElement { DataField = "CustomerName", Format = "currency" });
        detailBand.Elements.Add(new ImageElement { Source = "logo.png", Sizing = ImageSizing.Stretch });
        detailBand.Elements.Add(new BarcodeElement { Value = "12345", Format = BarcodeFormat.QRCode });
        t.Bands.Add(detailBand);

        var json = _parser.Serialize(t);
        var parsed = _parser.Parse(json);

        Assert.Equal("3.0", parsed.Version);
        Assert.Equal("TestAuthor", parsed.Author);
        Assert.Equal("Test Description", parsed.Description);
        Assert.Equal(297, parsed.Page.Width);
        Assert.Equal(210, parsed.Page.Height);
        Assert.Equal("landscape", parsed.Page.Orientation);
        Assert.Equal(15, parsed.Page.Margin.Top);
        Assert.Equal("#F0F0F0", parsed.Page.BackgroundColor);
        Assert.Equal("DRAFT", parsed.Page.Watermark);
        Assert.Single(parsed.DataSources);
        Assert.Equal("Server=localhost", parsed.DataSources[0].ConnectionString);
        Assert.Single(parsed.Parameters);
        Assert.Equal("开始日期", parsed.Parameters[0].Label);
        Assert.Equal(2, parsed.Bands.Count);
        Assert.True(parsed.Bands[0].RepeatOnNewPage);
        Assert.Equal(3, parsed.Bands[1].Elements.Count);
    }
}

/// <summary>
/// BorderDef 完整属性测试
/// </summary>
public class BorderDefComplete2Tests
{
    [Fact]
    public void Width_DefaultIs1()
    {
        var b = new BorderDef();
        Assert.Equal(1, b.Width);
    }

    [Fact]
    public void Width_Set_Works()
    {
        var b = new BorderDef { Width = 2.5 };
        Assert.Equal(2.5, b.Width);
    }

    [Fact]
    public void Color_DefaultIsBlack()
    {
        var b = new BorderDef();
        Assert.Equal("#000000", b.Color);
    }

    [Fact]
    public void Color_Set_Works()
    {
        var b = new BorderDef { Color = "#FF0000" };
        Assert.Equal("#FF0000", b.Color);
    }

    [Fact]
    public void Style_DefaultIsSolid()
    {
        var b = new BorderDef();
        Assert.Equal(BorderStyle.Solid, b.Style);
    }

    [Fact]
    public void Style_SetDashed_Works()
    {
        var b = new BorderDef { Style = BorderStyle.Dashed };
        Assert.Equal(BorderStyle.Dashed, b.Style);
    }

    [Fact]
    public void Style_SetDotted_Works()
    {
        var b = new BorderDef { Style = BorderStyle.Dotted };
        Assert.Equal(BorderStyle.Dotted, b.Style);
    }

    [Fact]
    public void Style_SetNone_Works()
    {
        var b = new BorderDef { Style = BorderStyle.None };
        Assert.Equal(BorderStyle.None, b.Style);
    }

    [Fact]
    public void Top_FalseByDefault()
    {
        var b = new BorderDef();
        Assert.False(b.Top);
    }

    [Fact]
    public void Top_SetTrue_Works()
    {
        var b = new BorderDef { Top = true };
        Assert.True(b.Top);
    }

    [Fact]
    public void Bottom_FalseByDefault()
    {
        var b = new BorderDef();
        Assert.False(b.Bottom);
    }

    [Fact]
    public void Bottom_SetTrue_Works()
    {
        var b = new BorderDef { Bottom = true };
        Assert.True(b.Bottom);
    }

    [Fact]
    public void Left_FalseByDefault()
    {
        var b = new BorderDef();
        Assert.False(b.Left);
    }

    [Fact]
    public void Left_SetTrue_Works()
    {
        var b = new BorderDef { Left = true };
        Assert.True(b.Left);
    }

    [Fact]
    public void Right_FalseByDefault()
    {
        var b = new BorderDef();
        Assert.False(b.Right);
    }

    [Fact]
    public void Right_SetTrue_Works()
    {
        var b = new BorderDef { Right = true };
        Assert.True(b.Right);
    }

    [Fact]
    public void FullSetup_Works()
    {
        var b = new BorderDef
        {
            Width = 3,
            Color = "#0000FF",
            Style = BorderStyle.Dotted,
            Top = true,
            Bottom = true,
            Left = true,
            Right = true
        };

        Assert.Equal(3, b.Width);
        Assert.Equal("#0000FF", b.Color);
        Assert.Equal(BorderStyle.Dotted, b.Style);
        Assert.True(b.Top);
        Assert.True(b.Bottom);
        Assert.True(b.Left);
        Assert.True(b.Right);
    }
}

/// <summary>
/// GroupDef 完整属性测试
/// </summary>
public class GroupDefComplete2Tests
{
    [Fact]
    public void Expression_EmptyByDefault()
    {
        var g = new GroupDef();
        Assert.Equal("", g.Expression);
    }

    [Fact]
    public void Expression_Set_Works()
    {
        var g = new GroupDef { Expression = "[Region]" };
        Assert.Equal("[Region]", g.Expression);
    }

    [Fact]
    public void KeepTogether_TrueByDefault()
    {
        var g = new GroupDef();
        Assert.True(g.KeepTogether);
    }

    [Fact]
    public void KeepTogether_SetFalse_Works()
    {
        var g = new GroupDef { KeepTogether = false };
        Assert.False(g.KeepTogether);
    }

    [Fact]
    public void FullSetup_Works()
    {
        var g = new GroupDef { Expression = "[Department]", KeepTogether = false };
        Assert.Equal("[Department]", g.Expression);
        Assert.False(g.KeepTogether);
    }
}

/// <summary>
/// MultiColumnConfig 完整属性测试
/// </summary>
public class MultiColumnConfigComplete2Tests
{
    [Fact]
    public void ColumnCount_DefaultIs2()
    {
        var mc = new MultiColumnConfig();
        Assert.Equal(2, mc.ColumnCount);
    }

    [Fact]
    public void ColumnCount_Set_Works()
    {
        var mc = new MultiColumnConfig { ColumnCount = 4 };
        Assert.Equal(4, mc.ColumnCount);
    }

    [Fact]
    public void ColumnSpacing_DefaultIs5()
    {
        var mc = new MultiColumnConfig();
        Assert.Equal(5, mc.ColumnSpacing);
    }

    [Fact]
    public void ColumnSpacing_Set_Works()
    {
        var mc = new MultiColumnConfig { ColumnSpacing = 10 };
        Assert.Equal(10, mc.ColumnSpacing);
    }

    [Fact]
    public void Direction_DefaultIsHorizontal()
    {
        var mc = new MultiColumnConfig();
        Assert.Equal("Horizontal", mc.Direction);
    }

    [Fact]
    public void Direction_SetVertical_Works()
    {
        var mc = new MultiColumnConfig { Direction = "Vertical" };
        Assert.Equal("Vertical", mc.Direction);
    }

    [Fact]
    public void FullSetup_Works()
    {
        var mc = new MultiColumnConfig
        {
            ColumnCount = 3,
            ColumnSpacing = 8,
            Direction = "Vertical"
        };

        Assert.Equal(3, mc.ColumnCount);
        Assert.Equal(8, mc.ColumnSpacing);
        Assert.Equal("Vertical", mc.Direction);
    }
}
