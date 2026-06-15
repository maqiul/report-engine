using ReportEngine.Core;
using ReportEngine.Core.Data;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// 最后冲刺 4000 测试
/// </summary>
public class FinalPushTo4000Tests
{
    [Fact]
    public void GroupDef_Expression_DefaultEmpty()
    {
        var g = new GroupDef();
        Assert.Equal("", g.Expression);
    }

    [Fact]
    public void GroupDef_Expression_SetValue()
    {
        var g = new GroupDef { Expression = "{{category}}" };
        Assert.Equal("{{category}}", g.Expression);
    }

    [Fact]
    public void GroupDef_KeepTogether_DefaultTrue()
    {
        var g = new GroupDef();
        Assert.True(g.KeepTogether);
    }

    [Fact]
    public void GroupDef_KeepTogether_SetFalse()
    {
        var g = new GroupDef { KeepTogether = false };
        Assert.False(g.KeepTogether);
    }

    [Fact]
    public void Band_Group_DefaultNull()
    {
        var b = new Band();
        Assert.Null(b.Group);
    }

    [Fact]
    public void Band_Group_SetValue()
    {
        var b = new Band { Group = new GroupDef { Expression = "{{region}}" } };
        Assert.NotNull(b.Group);
        Assert.Equal("{{region}}", b.Group.Expression);
    }
}
