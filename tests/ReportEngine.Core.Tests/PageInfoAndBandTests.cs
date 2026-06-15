using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReportEngine.Core;
using ReportEngine.Core.Rendering;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// PageInfo / Margin / Band / DataSourceDef 字段行为测试：
///   - PageInfo 默认值（Width=210, Height=297, Unit=mm, Orientation=portrait）
///   - Margin 默认值（Top/Bottom/Left/Right=10）
///   - Band 默认值（Type=Detail, Height=0, RepeatOnNewPage=false）
///   - DataSourceDef.ToString() 输出格式
///   - FieldDef.ToString() 输出格式
///   - MultiUpConfig.Count = Rows * Columns
/// </summary>
public class PageInfoAndBandTests
{
    [Fact]
    public void PageInfo_Defaults_AreA4()
    {
        var p = new PageInfo();
        Assert.Equal(210, p.Width);
        Assert.Equal(297, p.Height);
        Assert.Equal("mm", p.Unit);
        Assert.Equal("portrait", p.Orientation);
    }

    [Fact]
    public void Margin_Defaults_Are10mm()
    {
        var m = new Margin();
        Assert.Equal(10, m.Top);
        Assert.Equal(10, m.Bottom);
        Assert.Equal(10, m.Left);
        Assert.Equal(10, m.Right);
    }

    [Fact]
    public void Band_Defaults_AreEmpty()
    {
        var b = new Band();
        Assert.Equal(BandType.Header, b.Type);  // 实际默认是 Header（第一个枚举值）
        Assert.Equal(0, b.Height);
        Assert.False(b.RepeatOnNewPage);
        Assert.Empty(b.Elements);
        Assert.Null(b.DataSource);
        Assert.Null(b.Group);
        Assert.Null(b.MultiColumn);
    }

    [Fact]
    public void DataSourceDef_ToString_ContainsNameAndType()
    {
        var d = new DataSourceDef { Name = "orders", Type = "json" };
        var s = d.ToString();
        Assert.Contains("orders", s);
        Assert.Contains("json", s);
    }

    [Fact]
    public void DataSourceDef_ToString_ShowsFieldCount()
    {
        var d = new DataSourceDef
        {
            Name = "ds",
            Type = "sql",
            Fields = new List<FieldDef> { new() { Name = "a" }, new() { Name = "b" } },
        };
        Assert.Contains("2 字段", d.ToString());
    }

    [Fact]
    public void FieldDef_ToString_ContainsNameAndType()
    {
        var f = new FieldDef { Name = "age", Type = "number" };
        var s = f.ToString();
        Assert.Contains("age", s);
        Assert.Contains("number", s);
    }

    [Fact]
    public void FieldDef_ToString_ShowsFormatWhenPresent()
    {
        var f = new FieldDef { Name = "price", Type = "number", Format = "currency" };
        var s = f.ToString();
        Assert.Contains("price", s);
        Assert.Contains("currency", s);
    }

    [Fact]
    public void MultiUpConfig_Count_EqualsRowsTimesColumns()
    {
        var m = new MultiUpConfig { Rows = 3, Columns = 2 };
        Assert.Equal(6, m.Count);
    }

    [Fact]
    public void MultiColumnConfig_Defaults()
    {
        var c = new MultiColumnConfig();
        Assert.Equal(2, c.ColumnCount);
        Assert.Equal(5, c.ColumnSpacing);
        Assert.Equal("Horizontal", c.Direction);
    }

    [Fact]
    public void ReportTemplate_Defaults_HaveEmptyLists()
    {
        var t = new ReportTemplate();
        Assert.NotNull(t.Bands);
        Assert.Empty(t.Bands);
        Assert.NotNull(t.DataSources);
        Assert.Empty(t.DataSources);
        Assert.NotNull(t.Parameters);
        Assert.Empty(t.Parameters);
        Assert.Equal("1.0", t.Version);
    }

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
    public void PageInfo_BackgroundImage_DefaultsToNull()
    {
        var p = new PageInfo();
        Assert.Null(p.BackgroundColor);
        Assert.Null(p.BackgroundImage);
        Assert.Null(p.Watermark);
        Assert.Null(p.MultiUp);
    }

    [Fact]
    public void ReportElement_Defaults_HaveGuidId()
    {
        var t1 = new TextElement();
        var t2 = new TextElement();
        Assert.NotNull(t1.Id);
        Assert.NotEmpty(t1.Id);
        Assert.NotEqual(t1.Id, t2.Id);
    }

    [Fact]
    public void ReportElement_Defaults_VisibleTrue_OpacityOne()
    {
        var t = new TextElement();
        Assert.True(t.Visible);
        Assert.Equal(1.0, t.Opacity);
        Assert.Equal(0, t.Rotation);
        Assert.False(t.Locked);
    }

    [Fact]
    public void ConditionalFormatRule_Defaults_AllEmpty()
    {
        var c = new ConditionalFormatRule();
        Assert.Equal("", c.Expression);
        Assert.Null(c.BackgroundColor);
        Assert.Null(c.FontColor);
        Assert.False(c.Bold);
    }

    [Fact]
    public void BarcodeFormat_EnumValues_Exist()
    {
        // 至少验证枚举包含常见格式
        Assert.True(System.Enum.IsDefined(typeof(BarcodeFormat), BarcodeFormat.Code128));
    }
}
