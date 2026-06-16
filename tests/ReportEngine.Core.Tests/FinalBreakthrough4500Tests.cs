using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Rendering;
using Xunit;

namespace ReportEngine.Core.Tests;

// ─────────────────────────────────────────────────────────────────────────────
// MultiUpConfig 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class MultiUpConfigComplete4Tests
{
    [Fact]
    public void MultiUpConfig_Rows_Default2()
    {
        var m = new MultiUpConfig();
        Assert.Equal(2, m.Rows);
    }

    [Fact]
    public void MultiUpConfig_Rows_SetValue()
    {
        var m = new MultiUpConfig { Rows = 3 };
        Assert.Equal(3, m.Rows);
    }

    [Fact]
    public void MultiUpConfig_Columns_Default2()
    {
        var m = new MultiUpConfig();
        Assert.Equal(2, m.Columns);
    }

    [Fact]
    public void MultiUpConfig_Columns_SetValue()
    {
        var m = new MultiUpConfig { Columns = 4 };
        Assert.Equal(4, m.Columns);
    }

    [Fact]
    public void MultiUpConfig_Direction_DefaultHorizontal()
    {
        var m = new MultiUpConfig();
        Assert.Equal("Horizontal", m.Direction);
    }

    [Fact]
    public void MultiUpConfig_Direction_SetVertical()
    {
        var m = new MultiUpConfig { Direction = "Vertical" };
        Assert.Equal("Vertical", m.Direction);
    }

    [Fact]
    public void MultiUpConfig_Count_Calculated()
    {
        var m = new MultiUpConfig { Rows = 3, Columns = 4 };
        Assert.Equal(12, m.Count);
    }

    [Fact]
    public void MultiUpConfig_Count_Default4()
    {
        var m = new MultiUpConfig();
        Assert.Equal(4, m.Count);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// FieldDef 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class FieldDefComplete4Tests
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
        var f = new FieldDef { Name = "price" };
        Assert.Equal("price", f.Name);
    }

    [Fact]
    public void FieldDef_Type_DefaultString()
    {
        var f = new FieldDef();
        Assert.Equal("string", f.Type);
    }

    [Fact]
    public void FieldDef_Type_SetNumber()
    {
        var f = new FieldDef { Type = "number" };
        Assert.Equal("number", f.Type);
    }

    [Fact]
    public void FieldDef_Type_SetDate()
    {
        var f = new FieldDef { Type = "date" };
        Assert.Equal("date", f.Type);
    }

    [Fact]
    public void FieldDef_Type_SetBoolean()
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
        var f = new FieldDef { Format = "0.00" };
        Assert.Equal("0.00", f.Format);
    }

    [Fact]
    public void FieldDef_ToString_WithName()
    {
        var f = new FieldDef { Name = "total", Type = "number" };
        Assert.Equal("total [number]", f.ToString());
    }

    [Fact]
    public void FieldDef_ToString_WithFormat()
    {
        var f = new FieldDef { Name = "amount", Type = "number", Format = "0.00" };
        Assert.Equal("amount [number] (0.00)", f.ToString());
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// DataSourceDef 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class DataSourceDefComplete4Tests
{
    [Fact]
    public void DataSourceDef_Name_DefaultEmpty()
    {
        var d = new DataSourceDef();
        Assert.Equal("", d.Name);
    }

    [Fact]
    public void DataSourceDef_Name_SetValue()
    {
        var d = new DataSourceDef { Name = "orders" };
        Assert.Equal("orders", d.Name);
    }

    [Fact]
    public void DataSourceDef_Type_DefaultJson()
    {
        var d = new DataSourceDef();
        Assert.Equal("json", d.Type);
    }

    [Fact]
    public void DataSourceDef_Type_SetSql()
    {
        var d = new DataSourceDef { Type = "sql" };
        Assert.Equal("sql", d.Type);
    }

    [Fact]
    public void DataSourceDef_Type_SetCsv()
    {
        var d = new DataSourceDef { Type = "csv" };
        Assert.Equal("csv", d.Type);
    }

    [Fact]
    public void DataSourceDef_Type_SetApi()
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
        d.Fields.Add(new FieldDef { Name = "id" });
        Assert.Single(d.Fields);
    }

    [Fact]
    public void DataSourceDef_ConnectionString_DefaultNull()
    {
        var d = new DataSourceDef();
        Assert.Null(d.ConnectionString);
    }

    [Fact]
    public void DataSourceDef_ConnectionString_SetValue()
    {
        var d = new DataSourceDef { ConnectionString = "Server=localhost" };
        Assert.Equal("Server=localhost", d.ConnectionString);
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
    public void DataSourceDef_ToString()
    {
        var d = new DataSourceDef { Name = "products", Type = "sql" };
        d.Fields.Add(new FieldDef { Name = "id" });
        Assert.Equal("products (sql, 1 字段)", d.ToString());
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// BorderDef 额外属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class BorderDefExtra4Tests
{
    [Fact]
    public void BorderDef_Width_Default1()
    {
        var b = new BorderDef();
        Assert.Equal(1, b.Width);
    }

    [Fact]
    public void BorderDef_Width_SetValue()
    {
        var b = new BorderDef { Width = 2.5 };
        Assert.Equal(2.5, b.Width);
    }

    [Fact]
    public void BorderDef_Color_DefaultBlack()
    {
        var b = new BorderDef();
        Assert.Equal("#000000", b.Color);
    }

    [Fact]
    public void BorderDef_Color_SetValue()
    {
        var b = new BorderDef { Color = "#FF0000" };
        Assert.Equal("#FF0000", b.Color);
    }

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
}
