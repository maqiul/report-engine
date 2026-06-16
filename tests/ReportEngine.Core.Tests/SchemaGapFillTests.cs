using ReportEngine.Core;
using ReportEngine.Core.Data;
using Xunit;

namespace ReportEngine.Core.Tests;

// ─────────────────────────────────────────────────────────────────────────────
// PageInfo 未覆盖属性
// ─────────────────────────────────────────────────────────────────────────────

public class PageInfoBackgroundTests
{
    [Fact]
    public void PageInfo_BackgroundColor_DefaultNull()
    {
        var p = new PageInfo();
        Assert.Null(p.BackgroundColor);
    }

    [Fact]
    public void PageInfo_BackgroundColor_SetValue()
    {
        var p = new PageInfo { BackgroundColor = "#F0F0F0" };
        Assert.Equal("#F0F0F0", p.BackgroundColor);
    }

    [Fact]
    public void PageInfo_BackgroundImage_DefaultNull()
    {
        var p = new PageInfo();
        Assert.Null(p.BackgroundImage);
    }

    [Fact]
    public void PageInfo_BackgroundImage_SetValue()
    {
        var p = new PageInfo { BackgroundImage = "bg.png" };
        Assert.Equal("bg.png", p.BackgroundImage);
    }

    [Fact]
    public void PageInfo_Watermark_DefaultNull()
    {
        var p = new PageInfo();
        Assert.Null(p.Watermark);
    }

    [Fact]
    public void PageInfo_Watermark_SetValue()
    {
        var p = new PageInfo { Watermark = "机密" };
        Assert.Equal("机密", p.Watermark);
    }

    [Fact]
    public void PageInfo_MultiUp_DefaultNull()
    {
        var p = new PageInfo();
        Assert.Null(p.MultiUp);
    }

    [Fact]
    public void PageInfo_MultiUp_SetValue()
    {
        var p = new PageInfo { MultiUp = new MultiUpConfig { Rows = 3, Columns = 2 } };
        Assert.NotNull(p.MultiUp);
        Assert.Equal(3, p.MultiUp.Rows);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Band 未覆盖属性
// ─────────────────────────────────────────────────────────────────────────────

public class BandRepeatAndMultiColumnTests
{
    [Fact]
    public void Band_RepeatOnNewPage_DefaultFalse()
    {
        var b = new Band();
        Assert.False(b.RepeatOnNewPage);
    }

    [Fact]
    public void Band_RepeatOnNewPage_SetTrue()
    {
        var b = new Band { RepeatOnNewPage = true };
        Assert.True(b.RepeatOnNewPage);
    }

    [Fact]
    public void Band_MultiColumn_DefaultNull()
    {
        var b = new Band();
        Assert.Null(b.MultiColumn);
    }

    [Fact]
    public void Band_MultiColumn_SetValue()
    {
        var b = new Band { MultiColumn = new MultiColumnConfig { ColumnCount = 3 } };
        Assert.NotNull(b.MultiColumn);
        Assert.Equal(3, b.MultiColumn.ColumnCount);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// MultiColumnConfig 完整属性
// ─────────────────────────────────────────────────────────────────────────────

public class MultiColumnConfigFull2Tests
{
    [Fact]
    public void MultiColumnConfig_ColumnCount_Default2()
    {
        var c = new MultiColumnConfig();
        Assert.Equal(2, c.ColumnCount);
    }

    [Fact]
    public void MultiColumnConfig_ColumnCount_SetValue()
    {
        var c = new MultiColumnConfig { ColumnCount = 4 };
        Assert.Equal(4, c.ColumnCount);
    }

    [Fact]
    public void MultiColumnConfig_ColumnSpacing_Default5()
    {
        var c = new MultiColumnConfig();
        Assert.Equal(5, c.ColumnSpacing);
    }

    [Fact]
    public void MultiColumnConfig_ColumnSpacing_SetValue()
    {
        var c = new MultiColumnConfig { ColumnSpacing = 8.5 };
        Assert.Equal(8.5, c.ColumnSpacing);
    }

    [Fact]
    public void MultiColumnConfig_Direction_DefaultHorizontal()
    {
        var c = new MultiColumnConfig();
        Assert.Equal("Horizontal", c.Direction);
    }

    [Fact]
    public void MultiColumnConfig_Direction_Vertical()
    {
        var c = new MultiColumnConfig { Direction = "Vertical" };
        Assert.Equal("Vertical", c.Direction);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// MultiUpConfig HSpacing / VSpacing
// ─────────────────────────────────────────────────────────────────────────────

public class MultiUpConfigSpacingTests
{
    [Fact]
    public void MultiUpConfig_HSpacing_DefaultZero()
    {
        var c = new MultiUpConfig();
        Assert.Equal(0, c.HSpacing);
    }

    [Fact]
    public void MultiUpConfig_HSpacing_SetValue()
    {
        var c = new MultiUpConfig { HSpacing = 3.5 };
        Assert.Equal(3.5, c.HSpacing);
    }

    [Fact]
    public void MultiUpConfig_VSpacing_DefaultZero()
    {
        var c = new MultiUpConfig();
        Assert.Equal(0, c.VSpacing);
    }

    [Fact]
    public void MultiUpConfig_VSpacing_SetValue()
    {
        var c = new MultiUpConfig { VSpacing = 2.0 };
        Assert.Equal(2.0, c.VSpacing);
    }

    [Fact]
    public void MultiUpConfig_FullSetup()
    {
        var c = new MultiUpConfig
        {
            Rows = 2, Columns = 3,
            HSpacing = 1.0, VSpacing = 2.0,
            Direction = "Vertical"
        };
        Assert.Equal(2, c.Rows);
        Assert.Equal(3, c.Columns);
        Assert.Equal(1.0, c.HSpacing);
        Assert.Equal(2.0, c.VSpacing);
        Assert.Equal("Vertical", c.Direction);
        Assert.Equal(6, c.Count);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// BorderDef 未覆盖属性
// ─────────────────────────────────────────────────────────────────────────────

public class BorderDefStyleAndSidesTests
{
    [Fact]
    public void BorderDef_Style_DefaultSolid()
    {
        var b = new BorderDef();
        Assert.Equal(BorderStyle.Solid, b.Style);
    }

    [Fact]
    public void BorderDef_Style_SetDashed()
    {
        var b = new BorderDef { Style = BorderStyle.Dashed };
        Assert.Equal(BorderStyle.Dashed, b.Style);
    }

    [Fact]
    public void BorderDef_Style_SetDotted()
    {
        var b = new BorderDef { Style = BorderStyle.Dotted };
        Assert.Equal(BorderStyle.Dotted, b.Style);
    }

    [Fact]
    public void BorderDef_Style_SetNone()
    {
        var b = new BorderDef { Style = BorderStyle.None };
        Assert.Equal(BorderStyle.None, b.Style);
    }

    [Fact]
    public void BorderDef_Top_DefaultFalse()
    {
        var b = new BorderDef();
        Assert.False(b.Top);
    }

    [Fact]
    public void BorderDef_Top_SetTrue()
    {
        var b = new BorderDef { Top = true };
        Assert.True(b.Top);
    }

    [Fact]
    public void BorderDef_Bottom_DefaultFalse()
    {
        var b = new BorderDef();
        Assert.False(b.Bottom);
    }

    [Fact]
    public void BorderDef_Bottom_SetTrue()
    {
        var b = new BorderDef { Bottom = true };
        Assert.True(b.Bottom);
    }

    [Fact]
    public void BorderDef_Left_DefaultFalse()
    {
        var b = new BorderDef();
        Assert.False(b.Left);
    }

    [Fact]
    public void BorderDef_Left_SetTrue()
    {
        var b = new BorderDef { Left = true };
        Assert.True(b.Left);
    }

    [Fact]
    public void BorderDef_Right_DefaultFalse()
    {
        var b = new BorderDef();
        Assert.False(b.Right);
    }

    [Fact]
    public void BorderDef_Right_SetTrue()
    {
        var b = new BorderDef { Right = true };
        Assert.True(b.Right);
    }

    [Fact]
    public void BorderDef_AllSides()
    {
        var b = new BorderDef { Top = true, Bottom = true, Left = true, Right = true };
        Assert.True(b.Top);
        Assert.True(b.Bottom);
        Assert.True(b.Left);
        Assert.True(b.Right);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// FontDef 未覆盖属性
// ─────────────────────────────────────────────────────────────────────────────

public class FontDefStylePropsTests
{
    [Fact]
    public void FontDef_Italic_DefaultFalse()
    {
        var f = new FontDef();
        Assert.False(f.Italic);
    }

    [Fact]
    public void FontDef_Italic_SetTrue()
    {
        var f = new FontDef { Italic = true };
        Assert.True(f.Italic);
    }

    [Fact]
    public void FontDef_Underline_DefaultFalse()
    {
        var f = new FontDef();
        Assert.False(f.Underline);
    }

    [Fact]
    public void FontDef_Underline_SetTrue()
    {
        var f = new FontDef { Underline = true };
        Assert.True(f.Underline);
    }

    [Fact]
    public void FontDef_Color_DefaultNull()
    {
        var f = new FontDef();
        Assert.Null(f.Color);
    }

    [Fact]
    public void FontDef_Color_SetValue()
    {
        var f = new FontDef { Color = "#FF0000" };
        Assert.Equal("#FF0000", f.Color);
    }

    [Fact]
    public void FontDef_Bold_DefaultFalse()
    {
        var f = new FontDef();
        Assert.False(f.Bold);
    }

    [Fact]
    public void FontDef_Bold_SetTrue()
    {
        var f = new FontDef { Bold = true };
        Assert.True(f.Bold);
    }

    [Fact]
    public void FontDef_Family_DefaultSimSun()
    {
        var f = new FontDef();
        Assert.Equal("SimSun", f.Family);
    }

    [Fact]
    public void FontDef_Size_Default10()
    {
        var f = new FontDef();
        Assert.Equal(10, f.Size);
    }

    [Fact]
    public void FontDef_FullSetup()
    {
        var f = new FontDef
        {
            Family = "Arial", Size = 14,
            Bold = true, Italic = true, Underline = true,
            Color = "#0000FF"
        };
        Assert.Equal("Arial", f.Family);
        Assert.Equal(14, f.Size);
        Assert.True(f.Bold);
        Assert.True(f.Italic);
        Assert.True(f.Underline);
        Assert.Equal("#0000FF", f.Color);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// ChartElement / ChartSeries 未覆盖属性
// ─────────────────────────────────────────────────────────────────────────────

public class ChartElementExtraPropsTests
{
    [Fact]
    public void ChartElement_Title_DefaultNull()
    {
        var el = new ChartElement();
        Assert.Null(el.Title);
    }

    [Fact]
    public void ChartElement_Title_SetValue()
    {
        var el = new ChartElement { Title = "销售趋势" };
        Assert.Equal("销售趋势", el.Title);
    }

    [Fact]
    public void ChartElement_CategoryField_DefaultEmpty()
    {
        var el = new ChartElement();
        Assert.Equal("", el.CategoryField);
    }

    [Fact]
    public void ChartElement_CategoryField_SetValue()
    {
        var el = new ChartElement { CategoryField = "month" };
        Assert.Equal("month", el.CategoryField);
    }

    [Fact]
    public void ChartElement_DataSource_DefaultEmpty()
    {
        var el = new ChartElement();
        Assert.Equal("", el.DataSource);
    }

    [Fact]
    public void ChartElement_DataSource_SetValue()
    {
        var el = new ChartElement { DataSource = "sales" };
        Assert.Equal("sales", el.DataSource);
    }

    [Fact]
    public void ChartSeries_Name_DefaultEmpty()
    {
        var s = new ChartSeries();
        Assert.Equal("", s.Name);
    }

    [Fact]
    public void ChartSeries_Name_SetValue()
    {
        var s = new ChartSeries { Name = "Revenue" };
        Assert.Equal("Revenue", s.Name);
    }

    [Fact]
    public void ChartSeries_ValueField_DefaultEmpty()
    {
        var s = new ChartSeries();
        Assert.Equal("", s.ValueField);
    }

    [Fact]
    public void ChartSeries_ValueField_SetValue()
    {
        var s = new ChartSeries { ValueField = "amount" };
        Assert.Equal("amount", s.ValueField);
    }

    [Fact]
    public void ChartSeries_Color_DefaultNull()
    {
        var s = new ChartSeries();
        Assert.Null(s.Color);
    }

    [Fact]
    public void ChartSeries_Color_SetValue()
    {
        var s = new ChartSeries { Color = "#FF6600" };
        Assert.Equal("#FF6600", s.Color);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// BarcodeElement 未覆盖属性
// ─────────────────────────────────────────────────────────────────────────────

public class BarcodeElementFullPropsTests
{
    [Fact]
    public void BarcodeElement_Value_DefaultEmpty()
    {
        var el = new BarcodeElement();
        Assert.Equal("", el.Value);
    }

    [Fact]
    public void BarcodeElement_Value_SetValue()
    {
        var el = new BarcodeElement { Value = "ABC123" };
        Assert.Equal("ABC123", el.Value);
    }

    [Fact]
    public void BarcodeElement_Format_DefaultQRCode()
    {
        var el = new BarcodeElement();
        Assert.Equal(BarcodeFormat.QRCode, el.Format);
    }

    [Fact]
    public void BarcodeElement_Format_SetCode128()
    {
        var el = new BarcodeElement { Format = BarcodeFormat.Code128 };
        Assert.Equal(BarcodeFormat.Code128, el.Format);
    }

    [Fact]
    public void BarcodeElement_ForeColor_Default000000()
    {
        var el = new BarcodeElement();
        Assert.Equal("#000000", el.ForeColor);
    }

    [Fact]
    public void BarcodeElement_ForeColor_SetValue()
    {
        var el = new BarcodeElement { ForeColor = "#FF0000" };
        Assert.Equal("#FF0000", el.ForeColor);
    }

    [Fact]
    public void BarcodeElement_BackColor_DefaultFFFFFF()
    {
        var el = new BarcodeElement();
        Assert.Equal("#FFFFFF", el.BackColor);
    }

    [Fact]
    public void BarcodeElement_BackColor_SetValue()
    {
        var el = new BarcodeElement { BackColor = "#EEEEEE" };
        Assert.Equal("#EEEEEE", el.BackColor);
    }

    [Fact]
    public void BarcodeElement_ShowText_DefaultTrue()
    {
        var el = new BarcodeElement();
        Assert.True(el.ShowText);
    }

    [Fact]
    public void BarcodeElement_ShowText_SetFalse()
    {
        var el = new BarcodeElement { ShowText = false };
        Assert.False(el.ShowText);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// SubReportDataBinding.ParamMap
// ─────────────────────────────────────────────────────────────────────────────

public class SubReportDataBindingParamMapTests
{
    [Fact]
    public void SubReportDataBinding_ParamMap_DefaultEmpty()
    {
        var b = new SubReportDataBinding();
        Assert.NotNull(b.ParamMap);
        Assert.Empty(b.ParamMap);
    }

    [Fact]
    public void SubReportDataBinding_ParamMap_AddItem()
    {
        var b = new SubReportDataBinding();
        b.ParamMap.Add("region", "currentRegion");
        Assert.Single(b.ParamMap);
    }

    [Fact]
    public void SubReportDataBinding_ParamMap_AddMultiple()
    {
        var b = new SubReportDataBinding();
        b.ParamMap.Add("region", "currentRegion");
        b.ParamMap.Add("year", "currentYear");
        b.ParamMap.Add("dept", "currentDept");
        Assert.Equal(3, b.ParamMap.Count);
    }

    [Fact]
    public void SubReportDataBinding_Source_DefaultEmpty()
    {
        var b = new SubReportDataBinding();
        Assert.Equal("", b.Source);
    }

    [Fact]
    public void SubReportDataBinding_Source_SetValue()
    {
        var b = new SubReportDataBinding { Source = "dsMain" };
        Assert.Equal("dsMain", b.Source);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// CrossTabMeasure 完整属性
// ─────────────────────────────────────────────────────────────────────────────

public class CrossTabMeasureFullTests
{
    [Fact]
    public void CrossTabMeasure_Field_DefaultEmpty()
    {
        var m = new CrossTabMeasure();
        Assert.Equal("", m.Field);
    }

    [Fact]
    public void CrossTabMeasure_Field_SetValue()
    {
        var m = new CrossTabMeasure { Field = "amount" };
        Assert.Equal("amount", m.Field);
    }

    [Fact]
    public void CrossTabMeasure_Aggregate_DefaultSum()
    {
        var m = new CrossTabMeasure();
        Assert.Equal("Sum", m.Aggregate);
    }

    [Fact]
    public void CrossTabMeasure_Aggregate_SetSum()
    {
        var m = new CrossTabMeasure { Aggregate = "Sum" };
        Assert.Equal("Sum", m.Aggregate);
    }

    [Fact]
    public void CrossTabMeasure_Format_DefaultNull()
    {
        var m = new CrossTabMeasure();
        Assert.Null(m.Format);
    }

    [Fact]
    public void CrossTabMeasure_Format_SetValue()
    {
        var m = new CrossTabMeasure { Format = "#,##0.00" };
        Assert.Equal("#,##0.00", m.Format);
    }

    [Fact]
    public void CrossTabMeasure_Label_DefaultNull()
    {
        var m = new CrossTabMeasure();
        Assert.Null(m.Label);
    }

    [Fact]
    public void CrossTabMeasure_Label_SetValue()
    {
        var m = new CrossTabMeasure { Label = "总金额" };
        Assert.Equal("总金额", m.Label);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// ReportTemplate 未覆盖属性
// ─────────────────────────────────────────────────────────────────────────────

public class ReportTemplateMetaPropsTests
{
    [Fact]
    public void ReportTemplate_Author_DefaultNull()
    {
        var t = new ReportTemplate();
        Assert.Null(t.Author);
    }

    [Fact]
    public void ReportTemplate_Author_SetValue()
    {
        var t = new ReportTemplate { Author = "老马" };
        Assert.Equal("老马", t.Author);
    }

    [Fact]
    public void ReportTemplate_Description_DefaultNull()
    {
        var t = new ReportTemplate();
        Assert.Null(t.Description);
    }

    [Fact]
    public void ReportTemplate_Description_SetValue()
    {
        var t = new ReportTemplate { Description = "月度销售报表" };
        Assert.Equal("月度销售报表", t.Description);
    }

    [Fact]
    public void ReportTemplate_Version_Default10()
    {
        var t = new ReportTemplate();
        Assert.Equal("1.0", t.Version);
    }

    [Fact]
    public void ReportTemplate_Version_SetValue()
    {
        var t = new ReportTemplate { Version = "2.0" };
        Assert.Equal("2.0", t.Version);
    }

    [Fact]
    public void ReportTemplate_Parameters_DefaultEmpty()
    {
        var t = new ReportTemplate();
        Assert.NotNull(t.Parameters);
        Assert.Empty(t.Parameters);
    }

    [Fact]
    public void ReportTemplate_Parameters_AddItem()
    {
        var t = new ReportTemplate();
        t.Parameters.Add(new TemplateParam { Name = "year", Type = "number" });
        Assert.Single(t.Parameters);
    }

    [Fact]
    public void ReportTemplate_DataSources_DefaultEmpty()
    {
        var t = new ReportTemplate();
        Assert.NotNull(t.DataSources);
        Assert.Empty(t.DataSources);
    }

    [Fact]
    public void ReportTemplate_Bands_DefaultEmpty()
    {
        var t = new ReportTemplate();
        Assert.NotNull(t.Bands);
        Assert.Empty(t.Bands);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// TemplateParam 完整属性
// ─────────────────────────────────────────────────────────────────────────────

public class TemplateParamFullTests
{
    [Fact]
    public void TemplateParam_Name_DefaultEmpty()
    {
        var p = new TemplateParam();
        Assert.Equal("", p.Name);
    }

    [Fact]
    public void TemplateParam_Name_SetValue()
    {
        var p = new TemplateParam { Name = "startDate" };
        Assert.Equal("startDate", p.Name);
    }

    [Fact]
    public void TemplateParam_Type_DefaultString()
    {
        var p = new TemplateParam();
        Assert.Equal("string", p.Type);
    }

    [Fact]
    public void TemplateParam_Type_Number()
    {
        var p = new TemplateParam { Type = "number" };
        Assert.Equal("number", p.Type);
    }

    [Fact]
    public void TemplateParam_Type_Date()
    {
        var p = new TemplateParam { Type = "date" };
        Assert.Equal("date", p.Type);
    }

    [Fact]
    public void TemplateParam_DefaultValue_DefaultEmpty()
    {
        var p = new TemplateParam();
        Assert.Equal("", p.DefaultValue);
    }

    [Fact]
    public void TemplateParam_DefaultValue_SetValue()
    {
        var p = new TemplateParam { DefaultValue = "2026-01-01" };
        Assert.Equal("2026-01-01", p.DefaultValue);
    }

    [Fact]
    public void TemplateParam_Label_DefaultNull()
    {
        var p = new TemplateParam();
        Assert.Null(p.Label);
    }

    [Fact]
    public void TemplateParam_Label_SetValue()
    {
        var p = new TemplateParam { Label = "开始日期" };
        Assert.Equal("开始日期", p.Label);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// DataSourceDef 未覆盖属性
// ─────────────────────────────────────────────────────────────────────────────

public class DataSourceDefExtra2Tests
{
    [Fact]
    public void DataSourceDef_ConnectionString_DefaultNull()
    {
        var d = new DataSourceDef();
        Assert.Null(d.ConnectionString);
    }

    [Fact]
    public void DataSourceDef_ConnectionString_SetValue()
    {
        var d = new DataSourceDef { ConnectionString = "Server=localhost;Database=mydb" };
        Assert.Equal("Server=localhost;Database=mydb", d.ConnectionString);
    }

    [Fact]
    public void DataSourceDef_Query_DefaultNull()
    {
        var d = new DataSourceDef();
        Assert.Null(d.Query);
    }

    [Fact]
    public void DataSourceDef_Query_SetValue()
    {
        var d = new DataSourceDef { Query = "SELECT * FROM orders" };
        Assert.Equal("SELECT * FROM orders", d.Query);
    }

    [Fact]
    public void DataSourceDef_Type_DefaultJson()
    {
        var d = new DataSourceDef();
        Assert.Equal("json", d.Type);
    }

    [Fact]
    public void DataSourceDef_Type_Sql()
    {
        var d = new DataSourceDef { Type = "sql" };
        Assert.Equal("sql", d.Type);
    }

    [Fact]
    public void DataSourceDef_Type_Csv()
    {
        var d = new DataSourceDef { Type = "csv" };
        Assert.Equal("csv", d.Type);
    }

    [Fact]
    public void DataSourceDef_Type_Api()
    {
        var d = new DataSourceDef { Type = "api" };
        Assert.Equal("api", d.Type);
    }

    [Fact]
    public void DataSourceDef_Fields_DefaultEmpty()
    {
        var d = new DataSourceDef();
        Assert.NotNull(d.Fields);
        Assert.Empty(d.Fields);
    }

    [Fact]
    public void DataSourceDef_Fields_AddItem()
    {
        var d = new DataSourceDef();
        d.Fields.Add(new FieldDef { Name = "id", Type = "number" });
        Assert.Single(d.Fields);
    }

    [Fact]
    public void DataSourceDef_ToString_ContainsNameAndType()
    {
        var d = new DataSourceDef { Name = "orders", Type = "sql" };
        d.Fields.Add(new FieldDef { Name = "id" });
        d.Fields.Add(new FieldDef { Name = "amount" });
        var s = d.ToString();
        Assert.Contains("orders", s);
        Assert.Contains("sql", s);
        Assert.Contains("2", s);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// FieldDef 完整属性
// ─────────────────────────────────────────────────────────────────────────────

public class FieldDefFullTests
{
    [Fact]
    public void FieldDef_Name_DefaultEmpty()
    {
        var f = new FieldDef();
        Assert.Equal("", f.Name);
    }

    [Fact]
    public void FieldDef_Name_SetValue()
    {
        var f = new FieldDef { Name = "orderId" };
        Assert.Equal("orderId", f.Name);
    }

    [Fact]
    public void FieldDef_Type_DefaultString()
    {
        var f = new FieldDef();
        Assert.Equal("string", f.Type);
    }

    [Fact]
    public void FieldDef_Type_Number()
    {
        var f = new FieldDef { Type = "number" };
        Assert.Equal("number", f.Type);
    }

    [Fact]
    public void FieldDef_Type_Date()
    {
        var f = new FieldDef { Type = "date" };
        Assert.Equal("date", f.Type);
    }

    [Fact]
    public void FieldDef_Type_Boolean()
    {
        var f = new FieldDef { Type = "boolean" };
        Assert.Equal("boolean", f.Type);
    }

    [Fact]
    public void FieldDef_Format_DefaultNull()
    {
        var f = new FieldDef();
        Assert.Null(f.Format);
    }

    [Fact]
    public void FieldDef_Format_SetValue()
    {
        var f = new FieldDef { Format = "yyyy-MM-dd" };
        Assert.Equal("yyyy-MM-dd", f.Format);
    }

    [Fact]
    public void FieldDef_ToString_NameAndType()
    {
        var f = new FieldDef { Name = "amount", Type = "number" };
        var s = f.ToString();
        Assert.Contains("amount", s);
        Assert.Contains("number", s);
    }

    [Fact]
    public void FieldDef_ToString_WithFormat()
    {
        var f = new FieldDef { Name = "price", Type = "number", Format = "#,##0.00" };
        var s = f.ToString();
        Assert.Contains("#,##0.00", s);
    }

    [Fact]
    public void FieldDef_ToString_WithoutFormat_NoParentheses()
    {
        var f = new FieldDef { Name = "id", Type = "number" };
        var s = f.ToString();
        Assert.DoesNotContain("(", s);
    }
}
