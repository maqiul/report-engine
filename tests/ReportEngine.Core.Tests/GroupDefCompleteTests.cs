using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// GroupDef 完整字段测试：
///   - GroupDef 完整字段（Expression/KeepTogether）
///   - 字段组合行为
/// </summary>
public class GroupDefCompleteTests
{
    [Fact]
    public void GroupDef_Defaults()
    {
        var g = new GroupDef();
        Assert.Equal("", g.Expression);
        Assert.True(g.KeepTogether);
    }

    [Fact]
    public void GroupDef_AllSetters()
    {
        var g = new GroupDef
        {
            Expression = "category",
            KeepTogether = false,
        };
        Assert.Equal("category", g.Expression);
        Assert.False(g.KeepTogether);
    }

    [Fact]
    public void GroupDef_Expression_CanBeEmpty()
    {
        var g = new GroupDef { Expression = "" };
        Assert.Equal("", g.Expression);
    }

    [Fact]
    public void GroupDef_Expression_CanBeFieldName()
    {
        var g = new GroupDef { Expression = "region" };
        Assert.Equal("region", g.Expression);
    }

    [Fact]
    public void GroupDef_Expression_CanBeComplex()
    {
        var g = new GroupDef { Expression = "LEFT(region, 1)" };
        Assert.Equal("LEFT(region, 1)", g.Expression);
    }

    [Fact]
    public void GroupDef_Expression_CanBeChinese()
    {
        var g = new GroupDef { Expression = "分类" };
        Assert.Equal("分类", g.Expression);
    }

    [Fact]
    public void GroupDef_KeepTogether_DefaultTrue()
    {
        var g = new GroupDef();
        Assert.True(g.KeepTogether);
    }

    [Fact]
    public void GroupDef_KeepTogether_CanBeFalse()
    {
        var g = new GroupDef { KeepTogether = false };
        Assert.False(g.KeepTogether);
    }

    [Fact]
    public void GroupDef_KeepTogether_CanBeSet()
    {
        var g = new GroupDef { KeepTogether = true };
        Assert.True(g.KeepTogether);
    }

    [Fact]
    public void GroupDef_FullCombination()
    {
        var g = new GroupDef
        {
            Expression = "department",
            KeepTogether = false,
        };
        Assert.Equal("department", g.Expression);
        Assert.False(g.KeepTogether);
    }
}
