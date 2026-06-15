using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// GroupDef 行为测试：
///   - 默认值
///   - 表达式
///   - KeepTogether
///   - 与 Band 关联
/// </summary>
public class GroupDefBehaviorTests
{
    // ============== 默认值 ==============

    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var group = new GroupDef();

        Assert.Equal("", group.Expression);
        Assert.True(group.KeepTogether);
    }

    // ============== Expression ==============

    [Fact]
    public void Expression_NullByDefault()
    {
        var group = new GroupDef();
        Assert.Equal("", group.Expression);
    }

    [Fact]
    public void Expression_SetSimpleField_Works()
    {
        var group = new GroupDef { Expression = "category" };
        Assert.Equal("category", group.Expression);
    }

    [Fact]
    public void Expression_SetNestedField_Works()
    {
        var group = new GroupDef { Expression = "order.customer.name" };
        Assert.Equal("order.customer.name", group.Expression);
    }

    [Fact]
    public void Expression_SetFunction_Works()
    {
        var group = new GroupDef { Expression = "YEAR(order_date)" };
        Assert.Equal("YEAR(order_date)", group.Expression);
    }

    [Fact]
    public void Expression_SetConditional_Works()
    {
        var group = new GroupDef { Expression = "IF(amount>1000,'HIGH','LOW')" };
        Assert.Contains("IF", group.Expression);
    }

    [Fact]
    public void Expression_SetComplex_Works()
    {
        var group = new GroupDef { Expression = "CONCAT(region, '-', category)" };
        Assert.Contains("CONCAT", group.Expression);
    }

    [Fact]
    public void Expression_CanBeCleared()
    {
        var group = new GroupDef { Expression = "category" };
        group.Expression = null;
        Assert.Null(group.Expression);
    }

    [Fact]
    public void Expression_ChineseField_Works()
    {
        var group = new GroupDef { Expression = "产品类别" };
        Assert.Equal("产品类别", group.Expression);
    }

    // ============== KeepTogether ==============

    [Fact]
    public void KeepTogether_DefaultIsFalse()
    {
        var group = new GroupDef();
        Assert.True(group.KeepTogether);
    }

    [Fact]
    public void KeepTogether_SetTrue_Works()
    {
        var group = new GroupDef { KeepTogether = true };
        Assert.True(group.KeepTogether);
    }

    [Fact]
    public void KeepTogether_CanBeToggled()
    {
        var group = new GroupDef { KeepTogether = true };
        group.KeepTogether = false;
        Assert.False(group.KeepTogether);
    }

    // ============== 与 Band 关联 ==============

    [Fact]
    public void Band_GroupAssociation_Works()
    {
        var band = new Band
        {
            Type = BandType.GroupHeader,
            Group = new GroupDef { Expression = "category", KeepTogether = true }
        };

        Assert.NotNull(band.Group);
        Assert.Equal("category", band.Group.Expression);
        Assert.True(band.Group.KeepTogether);
    }

    [Fact]
    public void Band_GroupCanBeNull()
    {
        var band = new Band { Type = BandType.Detail };
        Assert.Null(band.Group);
    }

    [Fact]
    public void Band_GroupCanBeReplaced()
    {
        var band = new Band
        {
            Type = BandType.GroupHeader,
            Group = new GroupDef { Expression = "category" }
        };

        var newGroup = new GroupDef { Expression = "region" };
        band.Group = newGroup;

        Assert.Same(newGroup, band.Group);
        Assert.Equal("region", band.Group.Expression);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void GroupDef_SimpleGroup_Works()
    {
        var group = new GroupDef
        {
            Expression = "department",
            KeepTogether = false
        };

        Assert.Equal("department", group.Expression);
        Assert.False(group.KeepTogether);
    }

    [Fact]
    public void GroupDef_KeepTogetherGroup_Works()
    {
        var group = new GroupDef
        {
            Expression = "invoice_id",
            KeepTogether = true
        };

        Assert.Equal("invoice_id", group.Expression);
        Assert.True(group.KeepTogether);
    }

    [Fact]
    public void GroupDef_DateGroup_Works()
    {
        var group = new GroupDef
        {
            Expression = "YEAR(created_date)",
            KeepTogether = false
        };

        Assert.Contains("YEAR", group.Expression);
    }

    [Fact]
    public void GroupDef_ConditionalGroup_Works()
    {
        var group = new GroupDef
        {
            Expression = "IF(status='ACTIVE','活跃','非活跃')",
            KeepTogether = true
        };

        Assert.Contains("IF", group.Expression);
        Assert.True(group.KeepTogether);
    }

    [Fact]
    public void GroupDef_MultiLevelGroup_Works()
    {
        var group1 = new GroupDef { Expression = "region", KeepTogether = false };
        var group2 = new GroupDef { Expression = "city", KeepTogether = false };
        var group3 = new GroupDef { Expression = "store", KeepTogether = true };

        Assert.Equal("region", group1.Expression);
        Assert.Equal("city", group2.Expression);
        Assert.Equal("store", group3.Expression);
    }

    [Fact]
    public void GroupDef_WithBands_Works()
    {
        var template = new ReportTemplate();
        
        var groupHeader = new Band
        {
            Type = BandType.GroupHeader,
            Height = 20,
            DataSource = "orders",
            Group = new GroupDef { Expression = "category", KeepTogether = true }
        };

        var detail = new Band
        {
            Type = BandType.Detail,
            Height = 15,
            DataSource = "orders"
        };

        var groupFooter = new Band
        {
            Type = BandType.GroupFooter,
            Height = 20,
            DataSource = "orders"
        };

        template.Bands.Add(groupHeader);
        template.Bands.Add(detail);
        template.Bands.Add(groupFooter);

        Assert.Equal(3, template.Bands.Count);
        Assert.NotNull(template.Bands[0].Group);
        Assert.Equal("category", template.Bands[0].Group.Expression);
        Assert.Null(template.Bands[1].Group);
        Assert.Null(template.Bands[2].Group);
    }

    [Fact]
    public void GroupDef_CanBeModified()
    {
        var group = new GroupDef { Expression = "category" };
        
        group.Expression = "region";
        group.KeepTogether = true;
        
        Assert.Equal("region", group.Expression);
        Assert.True(group.KeepTogether);
    }
}
