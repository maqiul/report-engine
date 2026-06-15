using System.Linq;
using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// GroupDef / TemplateParam / DataSourceDef / Band / BandType 行为测试：
///   - GroupDef 默认值（Expression=""，KeepTogether=true）
///   - TemplateParam 默认值
///   - DataSourceDef 默认值（Type="json"）
///   - DataSourceDef.ToString() 包含 "0 字段" 空字段
///   - Band.SubBands 嵌套 Band（null 默认，可加）
///   - BandType 7 个枚举值
///   - 6 个 Element 子类默认值（Image/Line/Shape/Barcode/Table/CrossTab）
/// </summary>
public class GroupParamDataSourceBandTests
{
    // ============== GroupDef ==============

    [Fact]
    public void GroupDef_Defaults()
    {
        var g = new GroupDef();
        Assert.Equal("", g.Expression);
        Assert.True(g.KeepTogether);
    }

    [Fact]
    public void GroupDef_Expression_CanBeSet()
    {
        var g = new GroupDef { Expression = "currentRow.category" };
        Assert.Equal("currentRow.category", g.Expression);
    }

    [Fact]
    public void GroupDef_KeepTogether_CanBeSet()
    {
        var g = new GroupDef { KeepTogether = false };
        Assert.False(g.KeepTogether);
    }

    // ============== TemplateParam ==============

    [Fact]
    public void TemplateParam_Defaults()
    {
        var p = new TemplateParam();
        Assert.Equal("", p.Name);
        Assert.Equal("string", p.Type);
        Assert.Equal("", p.DefaultValue);
        Assert.Null(p.Label);
    }

    [Fact]
    public void TemplateParam_AllSetters()
    {
        var p = new TemplateParam
        {
            Name = "year",
            Type = "number",
            DefaultValue = "2025",
            Label = "年份",
        };
        Assert.Equal("year", p.Name);
        Assert.Equal("number", p.Type);
        Assert.Equal("2025", p.DefaultValue);
        Assert.Equal("年份", p.Label);
    }

    [Fact]
    public void ReportTemplate_Parameters_AddAndRead()
    {
        var t = new ReportTemplate();
        t.Parameters.Add(new TemplateParam { Name = "p1" });
        t.Parameters.Add(new TemplateParam { Name = "p2" });
        Assert.Equal(2, t.Parameters.Count);
        Assert.Equal("p1", t.Parameters[0].Name);
    }

    // ============== DataSourceDef ==============

    [Fact]
    public void DataSourceDef_Defaults()
    {
        var d = new DataSourceDef();
        Assert.Equal("", d.Name);
        Assert.Equal("json", d.Type);
        Assert.NotNull(d.Fields);
        Assert.Empty(d.Fields);
        Assert.Null(d.ConnectionString);
        Assert.Null(d.Query);
    }

    [Fact]
    public void DataSourceDef_ConnectionStringAndQuery_CanBeSet()
    {
        var d = new DataSourceDef
        {
            ConnectionString = "Server=.;Database=test",
            Query = "SELECT * FROM orders",
        };
        Assert.Equal("Server=.;Database=test", d.ConnectionString);
        Assert.Contains("SELECT", d.Query);
    }

    [Fact]
    public void DataSourceDef_ToString_EmptyFields_ShowsZero()
    {
        var d = new DataSourceDef { Name = "d", Type = "csv" };
        var s = d.ToString();
        Assert.Contains("0 字段", s);
    }

    [Fact]
    public void DataSourceDef_ToString_OneField_ShowsOne()
    {
        var d = new DataSourceDef
        {
            Name = "d",
            Fields = { new FieldDef { Name = "x" } },
        };
        Assert.Contains("1 字段", d.ToString());
    }

    // ============== Band ==============

    [Fact]
    public void Band_SubBands_DefaultsToNull()
    {
        var b = new Band();
        Assert.Null(b.SubBands);
    }

    [Fact]
    public void Band_SubBands_CanBeSet()
    {
        var b = new Band { Type = BandType.Header };
        b.SubBands = new System.Collections.Generic.List<Band>
        {
            new() { Type = BandType.Detail, Height = 5 },
            new() { Type = BandType.Detail, Height = 3 },
        };
        Assert.Equal(2, b.SubBands.Count);
        Assert.Equal(5, b.SubBands[0].Height);
    }

    [Fact]
    public void Band_MultiColumn_CanBeSet()
    {
        var b = new Band
        {
            Type = BandType.Detail,
            MultiColumn = new MultiColumnConfig { ColumnCount = 3 },
        };
        Assert.NotNull(b.MultiColumn);
        Assert.Equal(3, b.MultiColumn.ColumnCount);
    }

    [Fact]
    public void Band_RepeatOnNewPage_DefaultsFalse()
    {
        var b = new Band();
        Assert.False(b.RepeatOnNewPage);
    }

    [Fact]
    public void Band_RepeatOnNewPage_CanBeSet()
    {
        var b = new Band { RepeatOnNewPage = true };
        Assert.True(b.RepeatOnNewPage);
    }

    [Fact]
    public void Band_Group_DefaultsToNull()
    {
        var b = new Band();
        Assert.Null(b.Group);
    }

    [Fact]
    public void Band_Group_CanBeSet()
    {
        var b = new Band
        {
            Type = BandType.GroupHeader,
            Group = new GroupDef { Expression = "category" },
        };
        Assert.NotNull(b.Group);
        Assert.Equal("category", b.Group.Expression);
    }

    // ============== BandType ==============

    [Fact]
    public void BandType_HasSevenValues()
    {
        Assert.True(System.Enum.IsDefined(typeof(BandType), BandType.Header));
        Assert.True(System.Enum.IsDefined(typeof(BandType), BandType.Footer));
        Assert.True(System.Enum.IsDefined(typeof(BandType), BandType.ReportHeader));
        Assert.True(System.Enum.IsDefined(typeof(BandType), BandType.ReportFooter));
        Assert.True(System.Enum.IsDefined(typeof(BandType), BandType.Detail));
        Assert.True(System.Enum.IsDefined(typeof(BandType), BandType.GroupHeader));
        Assert.True(System.Enum.IsDefined(typeof(BandType), BandType.GroupFooter));
    }

    [Fact]
    public void BandType_Default_FirstValueIsHeader()
    {
        // BandType 默认是 Header（第一个枚举值）
        var b = new Band();
        Assert.Equal(BandType.Header, b.Type);
    }

    // ============== Element 子类默认值 ==============

    [Fact]
    public void ImageElement_Defaults()
    {
        var e = new ImageElement();
        Assert.Equal("", e.Source);
    }

    [Fact]
    public void LineElement_Defaults()
    {
        var e = new LineElement();
        Assert.Equal(1.0, e.LineWidth);
        Assert.Equal("#000000", e.LineColor);
        Assert.Equal(LineDirection.Horizontal, e.Direction);
    }

    [Fact]
    public void ShapeElement_Defaults()
    {
        var e = new ShapeElement();
        Assert.Equal(ShapeType.Rectangle, e.Shape);
        Assert.Equal("#FFFFFF", e.FillColor);
        Assert.Equal(0, e.BorderRadius);
    }

    [Fact]
    public void BarcodeElement_Defaults()
    {
        var e = new BarcodeElement();
        Assert.Equal("", e.Value);
        Assert.Equal(BarcodeFormat.QRCode, e.Format);
        Assert.Equal("#000000", e.ForeColor);
        Assert.Equal("#FFFFFF", e.BackColor);
        Assert.True(e.ShowText);
    }

    [Fact]
    public void TableElement_Defaults()
    {
        var e = new TableElement();
        Assert.Equal(3, e.RowCount);
        Assert.Equal(3, e.ColCount);
        Assert.NotNull(e.ColumnWidths);
        Assert.Empty(e.ColumnWidths);
        Assert.NotNull(e.RowHeights);
        Assert.Empty(e.RowHeights);
        Assert.NotNull(e.Cells);
        Assert.Empty(e.Cells);
        Assert.Equal(0.3, e.BorderWidth);
        Assert.Equal("#000000", e.BorderColor);
    }

    [Fact]
    public void CrossTabElement_Defaults()
    {
        var e = new CrossTabElement();
        Assert.Equal("", e.DataSource);
        Assert.NotNull(e.RowFields);
        Assert.Empty(e.RowFields);
        Assert.NotNull(e.ColumnFields);
        Assert.Empty(e.ColumnFields);
        Assert.NotNull(e.Measures);
        Assert.Empty(e.Measures);
        Assert.True(e.ShowRowTotal);
        Assert.True(e.ShowColumnTotal);
    }

    [Fact]
    public void TableCell_Defaults()
    {
        var c = new TableCell();
        Assert.Equal(0, c.Row);
        Assert.Equal(0, c.Col);
        Assert.Equal(1, c.RowSpan);
        Assert.Equal(1, c.ColSpan);
        Assert.Equal("", c.Text);
        Assert.Equal(TextAlignment.Center, c.Alignment);
    }
}
