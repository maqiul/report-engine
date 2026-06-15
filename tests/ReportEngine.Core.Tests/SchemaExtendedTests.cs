using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// DataSourceDef 扩展行为测试
/// </summary>
public class DataSourceDefExtendedTests
{
    // ============== Type ==============

    [Fact]
    public void Type_DefaultIsJson()
    {
        var ds = new DataSourceDef();
        Assert.Equal("json", ds.Type);
    }

    [Fact]
    public void Type_SetSql_Works()
    {
        var ds = new DataSourceDef { Type = "sql" };
        Assert.Equal("sql", ds.Type);
    }

    [Fact]
    public void Type_SetCsv_Works()
    {
        var ds = new DataSourceDef { Type = "csv" };
        Assert.Equal("csv", ds.Type);
    }

    [Fact]
    public void Type_SetApi_Works()
    {
        var ds = new DataSourceDef { Type = "api" };
        Assert.Equal("api", ds.Type);
    }

    // ============== Fields ==============

    [Fact]
    public void Fields_EmptyByDefault()
    {
        var ds = new DataSourceDef();
        Assert.NotNull(ds.Fields);
        Assert.Empty(ds.Fields);
    }

    [Fact]
    public void Fields_Add_Works()
    {
        var ds = new DataSourceDef();
        ds.Fields.Add(new FieldDef { Name = "id", Type = "number" });
        Assert.Single(ds.Fields);
    }

    [Fact]
    public void Fields_AddMultiple_Works()
    {
        var ds = new DataSourceDef();
        ds.Fields.Add(new FieldDef { Name = "id", Type = "number" });
        ds.Fields.Add(new FieldDef { Name = "name", Type = "string" });
        ds.Fields.Add(new FieldDef { Name = "created", Type = "date" });
        Assert.Equal(3, ds.Fields.Count);
    }

    // ============== ConnectionString ==============

    [Fact]
    public void ConnectionString_NullByDefault()
    {
        var ds = new DataSourceDef();
        Assert.Null(ds.ConnectionString);
    }

    [Fact]
    public void ConnectionString_Set_Works()
    {
        var ds = new DataSourceDef { ConnectionString = "Server=localhost;Database=test" };
        Assert.Equal("Server=localhost;Database=test", ds.ConnectionString);
    }

    // ============== Query ==============

    [Fact]
    public void Query_NullByDefault()
    {
        var ds = new DataSourceDef();
        Assert.Null(ds.Query);
    }

    [Fact]
    public void Query_Set_Works()
    {
        var ds = new DataSourceDef { Query = "SELECT * FROM orders" };
        Assert.Equal("SELECT * FROM orders", ds.Query);
    }

    // ============== ToString ==============

    [Fact]
    public void ToString_ContainsNameAndType()
    {
        var ds = new DataSourceDef { Name = "orders", Type = "sql" };
        var str = ds.ToString();
        Assert.Contains("orders", str);
        Assert.Contains("sql", str);
    }

    [Fact]
    public void ToString_ContainsFieldCount()
    {
        var ds = new DataSourceDef { Name = "ds" };
        ds.Fields.Add(new FieldDef { Name = "a" });
        ds.Fields.Add(new FieldDef { Name = "b" });
        var str = ds.ToString();
        Assert.Contains("2", str);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void DataSourceDef_SqlSource_Works()
    {
        var ds = new DataSourceDef
        {
            Name = "orders",
            Type = "sql",
            ConnectionString = "Server=localhost;Database=mydb",
            Query = "SELECT id, name, amount FROM orders"
        };
        ds.Fields.Add(new FieldDef { Name = "id", Type = "number" });
        ds.Fields.Add(new FieldDef { Name = "name", Type = "string" });
        ds.Fields.Add(new FieldDef { Name = "amount", Type = "number", Format = "N2" });

        Assert.Equal("orders", ds.Name);
        Assert.Equal("sql", ds.Type);
        Assert.Equal(3, ds.Fields.Count);
    }

    [Fact]
    public void DataSourceDef_JsonSource_Works()
    {
        var ds = new DataSourceDef
        {
            Name = "apiData",
            Type = "json"
        };
        ds.Fields.Add(new FieldDef { Name = "value", Type = "string" });

        Assert.Null(ds.ConnectionString);
        Assert.Null(ds.Query);
        Assert.Single(ds.Fields);
    }
}

/// <summary>
/// FieldDef 扩展行为测试
/// </summary>
public class FieldDefExtendedTests
{
    // ============== Type ==============

    [Fact]
    public void Type_DefaultIsString()
    {
        var f = new FieldDef();
        Assert.Equal("string", f.Type);
    }

    [Fact]
    public void Type_SetNumber_Works()
    {
        var f = new FieldDef { Type = "number" };
        Assert.Equal("number", f.Type);
    }

    [Fact]
    public void Type_SetDate_Works()
    {
        var f = new FieldDef { Type = "date" };
        Assert.Equal("date", f.Type);
    }

    [Fact]
    public void Type_SetBoolean_Works()
    {
        var f = new FieldDef { Type = "boolean" };
        Assert.Equal("boolean", f.Type);
    }

    // ============== Format ==============

    [Fact]
    public void Format_NullByDefault()
    {
        var f = new FieldDef();
        Assert.Null(f.Format);
    }

    [Fact]
    public void Format_SetNumberFormat_Works()
    {
        var f = new FieldDef { Format = "N2" };
        Assert.Equal("N2", f.Format);
    }

    [Fact]
    public void Format_SetDateFormat_Works()
    {
        var f = new FieldDef { Format = "yyyy-MM-dd" };
        Assert.Equal("yyyy-MM-dd", f.Format);
    }

    [Fact]
    public void Format_SetCurrencyFormat_Works()
    {
        var f = new FieldDef { Format = "C2" };
        Assert.Equal("C2", f.Format);
    }

    [Fact]
    public void Format_SetPercentFormat_Works()
    {
        var f = new FieldDef { Format = "P1" };
        Assert.Equal("P1", f.Format);
    }

    // ============== ToString ==============

    [Fact]
    public void ToString_WithNameAndType()
    {
        var f = new FieldDef { Name = "amount", Type = "number" };
        var str = f.ToString();
        Assert.Contains("amount", str);
        Assert.Contains("number", str);
    }

    [Fact]
    public void ToString_WithFormat_IncludesFormat()
    {
        var f = new FieldDef { Name = "price", Type = "number", Format = "N2" };
        var str = f.ToString();
        Assert.Contains("price", str);
        Assert.Contains("N2", str);
    }

    [Fact]
    public void ToString_WithoutFormat_NoParentheses()
    {
        var f = new FieldDef { Name = "id", Type = "number" };
        var str = f.ToString();
        Assert.DoesNotContain("(", str);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void FieldDef_FullSetup_Works()
    {
        var f = new FieldDef
        {
            Name = "orderDate",
            Type = "date",
            Format = "yyyy-MM-dd HH:mm:ss"
        };

        Assert.Equal("orderDate", f.Name);
        Assert.Equal("date", f.Type);
        Assert.Equal("yyyy-MM-dd HH:mm:ss", f.Format);
    }

    [Fact]
    public void FieldDef_BooleanField_Works()
    {
        var f = new FieldDef { Name = "isActive", Type = "boolean" };
        Assert.Null(f.Format);
        Assert.Equal("boolean", f.Type);
    }
}

/// <summary>
/// TemplateParam 扩展行为测试
/// </summary>
public class TemplateParamExtendedTests
{
    // ============== Type ==============

    [Fact]
    public void Type_DefaultIsString()
    {
        var p = new TemplateParam();
        Assert.Equal("string", p.Type);
    }

    [Fact]
    public void Type_SetNumber_Works()
    {
        var p = new TemplateParam { Type = "number" };
        Assert.Equal("number", p.Type);
    }

    [Fact]
    public void Type_SetDate_Works()
    {
        var p = new TemplateParam { Type = "date" };
        Assert.Equal("date", p.Type);
    }

    [Fact]
    public void Type_SetBoolean_Works()
    {
        var p = new TemplateParam { Type = "boolean" };
        Assert.Equal("boolean", p.Type);
    }

    // ============== DefaultValue ==============

    [Fact]
    public void DefaultValue_EmptyByDefault()
    {
        var p = new TemplateParam();
        Assert.Equal("", p.DefaultValue);
    }

    [Fact]
    public void DefaultValue_Set_Works()
    {
        var p = new TemplateParam { DefaultValue = "Hello" };
        Assert.Equal("Hello", p.DefaultValue);
    }

    [Fact]
    public void DefaultValue_SetNumber_Works()
    {
        var p = new TemplateParam { DefaultValue = "42" };
        Assert.Equal("42", p.DefaultValue);
    }

    // ============== Label ==============

    [Fact]
    public void Label_NullByDefault()
    {
        var p = new TemplateParam();
        Assert.Null(p.Label);
    }

    [Fact]
    public void Label_Set_Works()
    {
        var p = new TemplateParam { Label = "报表标题" };
        Assert.Equal("报表标题", p.Label);
    }

    [Fact]
    public void Label_ChineseCharacters_Works()
    {
        var p = new TemplateParam { Label = "开始日期" };
        Assert.Equal("开始日期", p.Label);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void TemplateParam_StringParam_Works()
    {
        var p = new TemplateParam
        {
            Name = "title",
            Type = "string",
            DefaultValue = "Monthly Report",
            Label = "报表标题"
        };

        Assert.Equal("title", p.Name);
        Assert.Equal("string", p.Type);
        Assert.Equal("Monthly Report", p.DefaultValue);
        Assert.Equal("报表标题", p.Label);
    }

    [Fact]
    public void TemplateParam_NumberParam_Works()
    {
        var p = new TemplateParam
        {
            Name = "maxRows",
            Type = "number",
            DefaultValue = "100"
        };

        Assert.Equal("number", p.Type);
        Assert.Null(p.Label);
    }

    [Fact]
    public void TemplateParam_DateParam_Works()
    {
        var p = new TemplateParam
        {
            Name = "startDate",
            Type = "date",
            DefaultValue = "2024-01-01",
            Label = "开始日期"
        };

        Assert.Equal("date", p.Type);
        Assert.Equal("2024-01-01", p.DefaultValue);
    }

    [Fact]
    public void TemplateParam_ToString_ReturnsTypeFullName()
    {
        var p = new TemplateParam { Name = "year", Type = "number" };
        var str = p.ToString();
        // TemplateParam doesn't override ToString, so it returns the type name
        Assert.Contains("TemplateParam", str);
    }
}
