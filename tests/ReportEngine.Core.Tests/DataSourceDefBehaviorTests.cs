using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// DataSourceDef 行为测试：
///   - 默认值
///   - 字段设置
///   - 类型设置
///   - 连接字符串
///   - 查询语句
/// </summary>
public class DataSourceDefBehaviorTests
{
    // ============== 默认值 ==============

    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var ds = new DataSourceDef();

        Assert.Equal("", ds.Name);
        Assert.Equal("json", ds.Type);
        Assert.Null(ds.ConnectionString);
        Assert.Null(ds.Query);
        Assert.NotNull(ds.Fields);
        Assert.Empty(ds.Fields);
    }

    // ============== Name ==============

    [Fact]
    public void Name_EmptyByDefault()
    {
        var ds = new DataSourceDef();
        Assert.Equal("", ds.Name);
    }

    [Fact]
    public void Name_SetAndGet_Works()
    {
        var ds = new DataSourceDef { Name = "orders" };
        Assert.Equal("orders", ds.Name);
    }

    [Fact]
    public void Name_ChineseCharacters_Works()
    {
        var ds = new DataSourceDef { Name = "订单数据" };
        Assert.Equal("订单数据", ds.Name);
    }

    [Fact]
    public void Name_WithUnderscore_Works()
    {
        var ds = new DataSourceDef { Name = "order_details" };
        Assert.Equal("order_details", ds.Name);
    }

    // ============== Type ==============

    [Fact]
    public void Type_DefaultIsJson()
    {
        var ds = new DataSourceDef();
        Assert.Equal("json", ds.Type);
    }

    [Fact]
    public void Type_SetCsv_Works()
    {
        var ds = new DataSourceDef { Type = "csv" };
        Assert.Equal("csv", ds.Type);
    }

    [Fact]
    public void Type_SetDatabase_Works()
    {
        var ds = new DataSourceDef { Type = "database" };
        Assert.Equal("database", ds.Type);
    }

    [Fact]
    public void Type_SetApi_Works()
    {
        var ds = new DataSourceDef { Type = "api" };
        Assert.Equal("api", ds.Type);
    }

    [Fact]
    public void Type_AnyString_Accepted()
    {
        var ds = new DataSourceDef { Type = "custom" };
        Assert.Equal("custom", ds.Type);
    }

    // ============== ConnectionString ==============

    [Fact]
    public void ConnectionString_NullByDefault()
    {
        var ds = new DataSourceDef();
        Assert.Null(ds.ConnectionString);
    }

    [Fact]
    public void ConnectionString_SetAndGet_Works()
    {
        var ds = new DataSourceDef
        {
            ConnectionString = "Server=localhost;Database=test;User=sa;Password=123"
        };
        Assert.Equal("Server=localhost;Database=test;User=sa;Password=123", ds.ConnectionString);
    }

    [Fact]
    public void ConnectionString_CanBeCleared()
    {
        var ds = new DataSourceDef { ConnectionString = "conn" };
        ds.ConnectionString = null;
        Assert.Null(ds.ConnectionString);
    }

    // ============== Query ==============

    [Fact]
    public void Query_NullByDefault()
    {
        var ds = new DataSourceDef();
        Assert.Null(ds.Query);
    }

    [Fact]
    public void Query_SetAndGet_Works()
    {
        var ds = new DataSourceDef { Query = "SELECT * FROM orders WHERE status = 'active'" };
        Assert.Equal("SELECT * FROM orders WHERE status = 'active'", ds.Query);
    }

    [Fact]
    public void Query_SqlQuery_Works()
    {
        var ds = new DataSourceDef
        {
            Type = "database",
            Query = "SELECT o.id, o.amount, c.name FROM orders o JOIN customers c ON o.customer_id = c.id"
        };
        Assert.Contains("JOIN", ds.Query);
    }

    [Fact]
    public void Query_CanBeCleared()
    {
        var ds = new DataSourceDef { Query = "SELECT 1" };
        ds.Query = null;
        Assert.Null(ds.Query);
    }

    // ============== Fields ==============

    [Fact]
    public void Fields_EmptyByDefault()
    {
        var ds = new DataSourceDef();
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
        ds.Fields.Add(new FieldDef { Name = "amount", Type = "decimal" });
        ds.Fields.Add(new FieldDef { Name = "created", Type = "date" });
        Assert.Equal(4, ds.Fields.Count);
    }

    [Fact]
    public void Fields_Remove_Works()
    {
        var ds = new DataSourceDef();
        var field = new FieldDef { Name = "id" };
        ds.Fields.Add(field);
        ds.Fields.Remove(field);
        Assert.Empty(ds.Fields);
    }

    [Fact]
    public void Fields_Clear_Works()
    {
        var ds = new DataSourceDef();
        ds.Fields.Add(new FieldDef { Name = "a" });
        ds.Fields.Add(new FieldDef { Name = "b" });
        ds.Fields.Clear();
        Assert.Empty(ds.Fields);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void DataSourceDef_JsonType_Works()
    {
        var ds = new DataSourceDef
        {
            Name = "orders",
            Type = "json"
        };
        ds.Fields.Add(new FieldDef { Name = "id", Type = "number" });
        ds.Fields.Add(new FieldDef { Name = "amount", Type = "decimal" });

        Assert.Equal("orders", ds.Name);
        Assert.Equal("json", ds.Type);
        Assert.Equal(2, ds.Fields.Count);
    }

    [Fact]
    public void DataSourceDef_DatabaseType_Works()
    {
        var ds = new DataSourceDef
        {
            Name = "customers",
            Type = "database",
            ConnectionString = "Server=db.example.com;Database=sales",
            Query = "SELECT * FROM customers WHERE active = 1"
        };
        ds.Fields.Add(new FieldDef { Name = "id", Type = "number" });
        ds.Fields.Add(new FieldDef { Name = "name", Type = "string" });
        ds.Fields.Add(new FieldDef { Name = "email", Type = "string" });

        Assert.Equal("customers", ds.Name);
        Assert.Equal("database", ds.Type);
        Assert.NotNull(ds.ConnectionString);
        Assert.NotNull(ds.Query);
        Assert.Equal(3, ds.Fields.Count);
    }

    [Fact]
    public void DataSourceDef_CsvType_Works()
    {
        var ds = new DataSourceDef
        {
            Name = "products",
            Type = "csv"
        };

        Assert.Equal("products", ds.Name);
        Assert.Equal("csv", ds.Type);
    }

    [Fact]
    public void DataSourceDef_ComplexQuery_Works()
    {
        var ds = new DataSourceDef
        {
            Name = "report_data",
            Type = "database",
            Query = @"
                SELECT 
                    o.order_id,
                    o.order_date,
                    o.total_amount,
                    c.customer_name,
                    c.customer_email,
                    SUM(oi.quantity * oi.unit_price) AS calculated_total
                FROM orders o
                INNER JOIN customers c ON o.customer_id = c.id
                INNER JOIN order_items oi ON o.order_id = oi.order_id
                WHERE o.order_date >= @startDate AND o.order_date <= @endDate
                GROUP BY o.order_id, o.order_date, o.total_amount, c.customer_name, c.customer_email
                HAVING SUM(oi.quantity * oi.unit_price) > 1000
                ORDER BY o.order_date DESC
            "
        };

        Assert.Contains("INNER JOIN", ds.Query);
        Assert.Contains("GROUP BY", ds.Query);
        Assert.Contains("HAVING", ds.Query);
    }
}
