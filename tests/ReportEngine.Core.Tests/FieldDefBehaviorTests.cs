using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// FieldDef 行为测试：
///   - 默认值
///   - 名称
///   - 类型
///   - 格式
/// </summary>
public class FieldDefBehaviorTests
{
    // ============== 默认值 ==============

    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var field = new FieldDef();

        Assert.Equal("", field.Name);
        Assert.Equal("string", field.Type);
        Assert.Null(field.Format);
    }

    // ============== Name ==============

    [Fact]
    public void Name_EmptyByDefault()
    {
        var field = new FieldDef();
        Assert.Equal("", field.Name);
    }

    [Fact]
    public void Name_SetAndGet_Works()
    {
        var field = new FieldDef { Name = "id" };
        Assert.Equal("id", field.Name);
    }

    [Fact]
    public void Name_ChineseCharacters_Works()
    {
        var field = new FieldDef { Name = "订单编号" };
        Assert.Equal("订单编号", field.Name);
    }

    [Fact]
    public void Name_WithUnderscore_Works()
    {
        var field = new FieldDef { Name = "customer_id" };
        Assert.Equal("customer_id", field.Name);
    }

    [Fact]
    public void Name_CamelCase_Works()
    {
        var field = new FieldDef { Name = "orderDate" };
        Assert.Equal("orderDate", field.Name);
    }

    // ============== Type ==============

    [Fact]
    public void Type_DefaultIsString()
    {
        var field = new FieldDef();
        Assert.Equal("string", field.Type);
    }

    [Fact]
    public void Type_SetNumber_Works()
    {
        var field = new FieldDef { Type = "number" };
        Assert.Equal("number", field.Type);
    }

    [Fact]
    public void Type_SetDecimal_Works()
    {
        var field = new FieldDef { Type = "decimal" };
        Assert.Equal("decimal", field.Type);
    }

    [Fact]
    public void Type_SetDate_Works()
    {
        var field = new FieldDef { Type = "date" };
        Assert.Equal("date", field.Type);
    }

    [Fact]
    public void Type_SetBoolean_Works()
    {
        var field = new FieldDef { Type = "boolean" };
        Assert.Equal("boolean", field.Type);
    }

    [Fact]
    public void Type_SetObject_Works()
    {
        var field = new FieldDef { Type = "object" };
        Assert.Equal("object", field.Type);
    }

    [Fact]
    public void Type_AnyString_Accepted()
    {
        var field = new FieldDef { Type = "custom" };
        Assert.Equal("custom", field.Type);
    }

    // ============== Format ==============

    [Fact]
    public void Format_NullByDefault()
    {
        var field = new FieldDef();
        Assert.Null(field.Format);
    }

    [Fact]
    public void Format_SetDate_Works()
    {
        var field = new FieldDef { Format = "yyyy-MM-dd" };
        Assert.Equal("yyyy-MM-dd", field.Format);
    }

    [Fact]
    public void Format_SetDateTime_Works()
    {
        var field = new FieldDef { Format = "yyyy-MM-dd HH:mm:ss" };
        Assert.Equal("yyyy-MM-dd HH:mm:ss", field.Format);
    }

    [Fact]
    public void Format_SetCurrency_Works()
    {
        var field = new FieldDef { Format = "C2" };
        Assert.Equal("C2", field.Format);
    }

    [Fact]
    public void Format_SetPercent_Works()
    {
        var field = new FieldDef { Format = "P1" };
        Assert.Equal("P1", field.Format);
    }

    [Fact]
    public void Format_SetNumber_Works()
    {
        var field = new FieldDef { Format = "N2" };
        Assert.Equal("N2", field.Format);
    }

    [Fact]
    public void Format_SetCustom_Works()
    {
        var field = new FieldDef { Format = "#,##0.00" };
        Assert.Equal("#,##0.00", field.Format);
    }

    [Fact]
    public void Format_CanBeCleared()
    {
        var field = new FieldDef { Format = "yyyy-MM-dd" };
        field.Format = null;
        Assert.Null(field.Format);
    }

    // ============== ToString ==============

    [Fact]
    public void ToString_WithoutFormat_Works()
    {
        var field = new FieldDef { Name = "id", Type = "number" };
        var result = field.ToString();
        Assert.Contains("id", result);
        Assert.Contains("number", result);
    }

    [Fact]
    public void ToString_WithFormat_Works()
    {
        var field = new FieldDef { Name = "amount", Type = "decimal", Format = "N2" };
        var result = field.ToString();
        Assert.Contains("amount", result);
        Assert.Contains("decimal", result);
        Assert.Contains("N2", result);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void FieldDef_StringField_Works()
    {
        var field = new FieldDef
        {
            Name = "customer_name",
            Type = "string"
        };

        Assert.Equal("customer_name", field.Name);
        Assert.Equal("string", field.Type);
        Assert.Null(field.Format);
    }

    [Fact]
    public void FieldDef_NumberField_Works()
    {
        var field = new FieldDef
        {
            Name = "amount",
            Type = "decimal",
            Format = "N2"
        };

        Assert.Equal("amount", field.Name);
        Assert.Equal("decimal", field.Type);
        Assert.Equal("N2", field.Format);
    }

    [Fact]
    public void FieldDef_DateField_Works()
    {
        var field = new FieldDef
        {
            Name = "order_date",
            Type = "date",
            Format = "yyyy-MM-dd"
        };

        Assert.Equal("order_date", field.Name);
        Assert.Equal("date", field.Type);
        Assert.Equal("yyyy-MM-dd", field.Format);
    }

    [Fact]
    public void FieldDef_BooleanField_Works()
    {
        var field = new FieldDef
        {
            Name = "is_active",
            Type = "boolean"
        };

        Assert.Equal("is_active", field.Name);
        Assert.Equal("boolean", field.Type);
    }

    [Fact]
    public void FieldDef_InDataSource_Works()
    {
        var ds = new DataSourceDef { Name = "orders", Type = "json" };
        ds.Fields.Add(new FieldDef { Name = "id", Type = "number" });
        ds.Fields.Add(new FieldDef { Name = "amount", Type = "decimal", Format = "N2" });
        ds.Fields.Add(new FieldDef { Name = "order_date", Type = "date", Format = "yyyy-MM-dd" });

        Assert.Equal(3, ds.Fields.Count);
        Assert.Equal("id", ds.Fields[0].Name);
        Assert.Equal("amount", ds.Fields[1].Name);
        Assert.Equal("N2", ds.Fields[1].Format);
    }

    [Fact]
    public void FieldDef_CanBeModified()
    {
        var field = new FieldDef { Name = "amount", Type = "number" };
        
        field.Type = "decimal";
        field.Format = "C2";
        
        Assert.Equal("decimal", field.Type);
        Assert.Equal("C2", field.Format);
    }
}
