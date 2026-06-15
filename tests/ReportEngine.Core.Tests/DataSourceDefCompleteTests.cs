using System.Collections.Generic;
using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// DataSourceDef / FieldDef 完整字段测试：
///   - DataSourceDef 完整字段（Name/Type/Fields/ConnectionString/Query）
///   - DataSourceDef.ToString() 行为
///   - FieldDef 完整字段（Name/Type/Format）
///   - FieldDef.ToString() 行为
/// </summary>
public class DataSourceDefCompleteTests
{
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
    public void DataSourceDef_AllSetters()
    {
        var d = new DataSourceDef
        {
            Name = "orders",
            Type = "sql",
            ConnectionString = "Server=localhost;Database=test",
            Query = "SELECT * FROM orders",
        };
        d.Fields.Add(new FieldDef { Name = "id", Type = "number" });
        d.Fields.Add(new FieldDef { Name = "name", Type = "string" });

        Assert.Equal("orders", d.Name);
        Assert.Equal("sql", d.Type);
        Assert.Equal("Server=localhost;Database=test", d.ConnectionString);
        Assert.Equal("SELECT * FROM orders", d.Query);
        Assert.Equal(2, d.Fields.Count);
    }

    [Fact]
    public void DataSourceDef_Name_CanBeEmpty()
    {
        var d = new DataSourceDef { Name = "" };
        Assert.Equal("", d.Name);
    }

    [Fact]
    public void DataSourceDef_Type_CanBeSql()
    {
        var d = new DataSourceDef { Type = "sql" };
        Assert.Equal("sql", d.Type);
    }

    [Fact]
    public void DataSourceDef_Type_CanBeCsv()
    {
        var d = new DataSourceDef { Type = "csv" };
        Assert.Equal("csv", d.Type);
    }

    [Fact]
    public void DataSourceDef_Type_CanBeApi()
    {
        var d = new DataSourceDef { Type = "api" };
        Assert.Equal("api", d.Type);
    }

    [Fact]
    public void DataSourceDef_Type_CanBeAnyString()
    {
        var d = new DataSourceDef { Type = "custom" };
        Assert.Equal("custom", d.Type);
    }

    [Fact]
    public void DataSourceDef_Fields_CanAddMultiple()
    {
        var d = new DataSourceDef();
        d.Fields.Add(new FieldDef { Name = "f1" });
        d.Fields.Add(new FieldDef { Name = "f2" });
        d.Fields.Add(new FieldDef { Name = "f3" });
        Assert.Equal(3, d.Fields.Count);
    }

    [Fact]
    public void DataSourceDef_ConnectionString_CanBeEmpty()
    {
        var d = new DataSourceDef { ConnectionString = "" };
        Assert.Equal("", d.ConnectionString);
    }

    [Fact]
    public void DataSourceDef_Query_CanBeEmpty()
    {
        var d = new DataSourceDef { Query = "" };
        Assert.Equal("", d.Query);
    }

    [Fact]
    public void DataSourceDef_ToString_NoFields()
    {
        var d = new DataSourceDef { Name = "ds", Type = "json" };
        var s = d.ToString();
        Assert.Contains("ds", s);
        Assert.Contains("json", s);
        Assert.Contains("0 字段", s);
    }

    [Fact]
    public void DataSourceDef_ToString_OneField()
    {
        var d = new DataSourceDef { Name = "ds", Type = "sql" };
        d.Fields.Add(new FieldDef { Name = "id" });
        var s = d.ToString();
        Assert.Contains("1 字段", s);
    }

    [Fact]
    public void DataSourceDef_ToString_MultipleFields()
    {
        var d = new DataSourceDef { Name = "ds", Type = "csv" };
        d.Fields.Add(new FieldDef());
        d.Fields.Add(new FieldDef());
        d.Fields.Add(new FieldDef());
        var s = d.ToString();
        Assert.Contains("3 字段", s);
    }

    // ============== FieldDef ==============

    [Fact]
    public void FieldDef_Defaults()
    {
        var f = new FieldDef();
        Assert.Equal("", f.Name);
        Assert.Equal("string", f.Type);
        Assert.Null(f.Format);
    }

    [Fact]
    public void FieldDef_AllSetters()
    {
        var f = new FieldDef
        {
            Name = "price",
            Type = "number",
            Format = "currency",
        };
        Assert.Equal("price", f.Name);
        Assert.Equal("number", f.Type);
        Assert.Equal("currency", f.Format);
    }

    [Fact]
    public void FieldDef_Name_CanBeEmpty()
    {
        var f = new FieldDef { Name = "" };
        Assert.Equal("", f.Name);
    }

    [Fact]
    public void FieldDef_Type_CanBeNumber()
    {
        var f = new FieldDef { Type = "number" };
        Assert.Equal("number", f.Type);
    }

    [Fact]
    public void FieldDef_Type_CanBeDate()
    {
        var f = new FieldDef { Type = "date" };
        Assert.Equal("date", f.Type);
    }

    [Fact]
    public void FieldDef_Type_CanBeBoolean()
    {
        var f = new FieldDef { Type = "boolean" };
        Assert.Equal("boolean", f.Type);
    }

    [Fact]
    public void FieldDef_Type_CanBeAnyString()
    {
        var f = new FieldDef { Type = "decimal" };
        Assert.Equal("decimal", f.Type);
    }

    [Fact]
    public void FieldDef_Format_CanBeEmpty()
    {
        var f = new FieldDef { Format = "" };
        Assert.Equal("", f.Format);
    }

    [Fact]
    public void FieldDef_Format_CanBeNull()
    {
        var f = new FieldDef { Format = null };
        Assert.Null(f.Format);
    }

    [Fact]
    public void FieldDef_ToString_NoFormat()
    {
        var f = new FieldDef { Name = "id", Type = "number" };
        var s = f.ToString();
        Assert.Contains("id", s);
        Assert.Contains("number", s);
        Assert.DoesNotContain("(", s);
    }

    [Fact]
    public void FieldDef_ToString_WithFormat()
    {
        var f = new FieldDef { Name = "price", Type = "number", Format = "currency" };
        var s = f.ToString();
        Assert.Contains("price", s);
        Assert.Contains("number", s);
        Assert.Contains("currency", s);
    }
}
