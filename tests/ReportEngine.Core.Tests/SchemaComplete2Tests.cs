using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Rendering;
using ReportEngine.Core.Parsing;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ReportTemplate 完整属性测试 2
/// </summary>
public class ReportTemplateComplete2Tests
{
    [Fact]
    public void ReportTemplate_Author_DefaultNull()
    {
        var t = new ReportTemplate();
        Assert.Null(t.Author);
    }

    [Fact]
    public void ReportTemplate_Description_DefaultNull()
    {
        var t = new ReportTemplate();
        Assert.Null(t.Description);
    }

    [Fact]
    public void ReportTemplate_CreatedAt_DefaultNow()
    {
        var before = DateTime.Now;
        var t = new ReportTemplate();
        var after = DateTime.Now;
        Assert.True(t.CreatedAt >= before.AddSeconds(-1) && t.CreatedAt <= after.AddSeconds(1));
    }

    [Fact]
    public void ReportTemplate_ModifiedAt_DefaultNow()
    {
        var before = DateTime.Now;
        var t = new ReportTemplate();
        var after = DateTime.Now;
        Assert.True(t.ModifiedAt >= before.AddSeconds(-1) && t.ModifiedAt <= after.AddSeconds(1));
    }

    [Fact]
    public void ReportTemplate_Parameters_DefaultEmpty()
    {
        var t = new ReportTemplate();
        Assert.NotNull(t.Parameters);
        Assert.Empty(t.Parameters);
    }

    [Fact]
    public void ReportTemplate_Parameters_Addable()
    {
        var t = new ReportTemplate();
        t.Parameters.Add(new TemplateParam { Name = "p1", Type = "string" });
        Assert.Single(t.Parameters);
    }

    [Fact]
    public void ReportTemplate_FullSetup()
    {
        var t = new ReportTemplate
        {
            Version = "2.0",
            Author = "Tester",
            Description = "A test report",
        };
        t.DataSources.Add(new DataSourceDef { Name = "ds" });
        t.Parameters.Add(new TemplateParam { Name = "p1", Type = "string" });
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 10 });

        Assert.Equal("2.0", t.Version);
        Assert.Equal("Tester", t.Author);
        Assert.Equal("A test report", t.Description);
        Assert.Single(t.DataSources);
        Assert.Single(t.Parameters);
        Assert.Single(t.Bands);
    }
}

/// <summary>
/// DataSourceDef 完整属性测试 2
/// </summary>
public class DataSourceDefComplete2Tests
{
    [Fact]
    public void DataSourceDef_Name_DefaultEmpty()
    {
        var ds = new DataSourceDef();
        Assert.Equal("", ds.Name);
    }

    [Fact]
    public void DataSourceDef_Name_Settable()
    {
        var ds = new DataSourceDef { Name = "orders" };
        Assert.Equal("orders", ds.Name);
    }

    [Fact]
    public void DataSourceDef_Type_DefaultJson()
    {
        var ds = new DataSourceDef();
        Assert.Equal("json", ds.Type);
    }

    [Fact]
    public void DataSourceDef_Type_Settable()
    {
        var ds = new DataSourceDef { Type = "sql" };
        Assert.Equal("sql", ds.Type);
    }

    [Fact]
    public void DataSourceDef_Fields_DefaultEmpty()
    {
        var ds = new DataSourceDef();
        Assert.NotNull(ds.Fields);
        Assert.Empty(ds.Fields);
    }

    [Fact]
    public void DataSourceDef_Fields_Addable()
    {
        var ds = new DataSourceDef();
        ds.Fields.Add(new FieldDef { Name = "id", Type = "number" });
        Assert.Single(ds.Fields);
    }

    [Fact]
    public void DataSourceDef_ConnectionString_DefaultNull()
    {
        var ds = new DataSourceDef();
        Assert.Null(ds.ConnectionString);
    }

    [Fact]
    public void DataSourceDef_Query_DefaultNull()
    {
        var ds = new DataSourceDef();
        Assert.Null(ds.Query);
    }

    [Fact]
    public void DataSourceDef_ToString_ContainsName()
    {
        var ds = new DataSourceDef { Name = "orders", Type = "sql" };
        ds.Fields.Add(new FieldDef { Name = "id" });
        var str = ds.ToString();
        Assert.Contains("orders", str);
        Assert.Contains("sql", str);
        Assert.Contains("1", str);
    }

    [Fact]
    public void DataSourceDef_FullSetup()
    {
        var ds = new DataSourceDef
        {
            Name = "db",
            Type = "sql",
            ConnectionString = "Server=localhost;Database=test",
            Query = "SELECT * FROM orders"
        };
        ds.Fields.Add(new FieldDef { Name = "id", Type = "number" });
        Assert.Equal("db", ds.Name);
        Assert.Equal("sql", ds.Type);
        Assert.Equal("Server=localhost;Database=test", ds.ConnectionString);
        Assert.Equal("SELECT * FROM orders", ds.Query);
        Assert.Single(ds.Fields);
    }
}

/// <summary>
/// FieldDef 完整属性测试
/// </summary>
public class FieldDefCompleteTests
{
    [Fact]
    public void FieldDef_Name_DefaultEmpty()
    {
        var f = new FieldDef();
        Assert.Equal("", f.Name);
    }

    [Fact]
    public void FieldDef_Type_DefaultString()
    {
        var f = new FieldDef();
        Assert.Equal("string", f.Type);
    }

    [Fact]
    public void FieldDef_Format_DefaultNull()
    {
        var f = new FieldDef();
        Assert.Null(f.Format);
    }

    [Fact]
    public void FieldDef_Format_Settable()
    {
        var f = new FieldDef { Format = "yyyy-MM-dd" };
        Assert.Equal("yyyy-MM-dd", f.Format);
    }

    [Fact]
    public void FieldDef_ToString_ContainsNameAndType()
    {
        var f = new FieldDef { Name = "amount", Type = "number" };
        var str = f.ToString();
        Assert.Contains("amount", str);
        Assert.Contains("number", str);
    }

    [Fact]
    public void FieldDef_ToString_WithFormat_ContainsFormat()
    {
        var f = new FieldDef { Name = "amount", Type = "number", Format = "F2" };
        var str = f.ToString();
        Assert.Contains("F2", str);
    }

    [Fact]
    public void FieldDef_FullSetup()
    {
        var f = new FieldDef { Name = "price", Type = "number", Format = "C2" };
        Assert.Equal("price", f.Name);
        Assert.Equal("number", f.Type);
        Assert.Equal("C2", f.Format);
    }
}

/// <summary>
/// TemplateParam 完整属性测试 2
/// </summary>
public class TemplateParamComplete2Tests
{
    [Fact]
    public void TemplateParam_Name_DefaultEmpty()
    {
        var p = new TemplateParam();
        Assert.Equal("", p.Name);
    }

    [Fact]
    public void TemplateParam_Type_DefaultString()
    {
        var p = new TemplateParam();
        Assert.Equal("string", p.Type);
    }

    [Fact]
    public void TemplateParam_DefaultValue_DefaultEmpty()
    {
        var p = new TemplateParam();
        Assert.Equal("", p.DefaultValue);
    }

    [Fact]
    public void TemplateParam_Label_DefaultNull()
    {
        var p = new TemplateParam();
        Assert.Null(p.Label);
    }

    [Fact]
    public void TemplateParam_Label_Settable()
    {
        var p = new TemplateParam { Label = "Start Date" };
        Assert.Equal("Start Date", p.Label);
    }

    [Fact]
    public void TemplateParam_FullSetup()
    {
        var p = new TemplateParam
        {
            Name = "startDate",
            Type = "date",
            DefaultValue = "2026-01-01",
            Label = "Select start date"
        };
        Assert.Equal("startDate", p.Name);
        Assert.Equal("date", p.Type);
        Assert.Equal("2026-01-01", p.DefaultValue);
        Assert.Equal("Select start date", p.Label);
    }
}
