using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// Margin Schema 完整字段测试：
///   - Margin 完整字段（Top/Bottom/Left/Right）
///   - 字段组合行为
/// </summary>
public class MarginSchemaCompleteTests
{
    [Fact]
    public void Margin_Defaults()
    {
        var m = new Margin();
        Assert.Equal(10, m.Top);
        Assert.Equal(10, m.Bottom);
        Assert.Equal(10, m.Left);
        Assert.Equal(10, m.Right);
    }

    [Fact]
    public void Margin_AllSetters()
    {
        var m = new Margin { Top = 15, Bottom = 20, Left = 25, Right = 30 };
        Assert.Equal(15, m.Top);
        Assert.Equal(20, m.Bottom);
        Assert.Equal(25, m.Left);
        Assert.Equal(30, m.Right);
    }

    [Fact]
    public void Margin_Top_Default10()
    {
        Assert.Equal(10, new Margin().Top);
    }

    [Fact]
    public void Margin_Top_CanBeZero()
    {
        var m = new Margin { Top = 0 };
        Assert.Equal(0, m.Top);
    }

    [Fact]
    public void Margin_Top_CanBeDecimal()
    {
        var m = new Margin { Top = 12.5 };
        Assert.Equal(12.5, m.Top);
    }

    [Fact]
    public void Margin_Bottom_Default10()
    {
        Assert.Equal(10, new Margin().Bottom);
    }

    [Fact]
    public void Margin_Bottom_CanBeZero()
    {
        var m = new Margin { Bottom = 0 };
        Assert.Equal(0, m.Bottom);
    }

    [Fact]
    public void Margin_Bottom_CanBeDecimal()
    {
        var m = new Margin { Bottom = 15.3 };
        Assert.Equal(15.3, m.Bottom);
    }

    [Fact]
    public void Margin_Left_Default10()
    {
        Assert.Equal(10, new Margin().Left);
    }

    [Fact]
    public void Margin_Left_CanBeZero()
    {
        var m = new Margin { Left = 0 };
        Assert.Equal(0, m.Left);
    }

    [Fact]
    public void Margin_Left_CanBeDecimal()
    {
        var m = new Margin { Left = 18.7 };
        Assert.Equal(18.7, m.Left);
    }

    [Fact]
    public void Margin_Right_Default10()
    {
        Assert.Equal(10, new Margin().Right);
    }

    [Fact]
    public void Margin_Right_CanBeZero()
    {
        var m = new Margin { Right = 0 };
        Assert.Equal(0, m.Right);
    }

    [Fact]
    public void Margin_Right_CanBeDecimal()
    {
        var m = new Margin { Right = 22.1 };
        Assert.Equal(22.1, m.Right);
    }

    [Fact]
    public void Margin_AllZero()
    {
        var m = new Margin { Top = 0, Bottom = 0, Left = 0, Right = 0 };
        Assert.Equal(0, m.Top + m.Bottom + m.Left + m.Right);
    }

    [Fact]
    public void Margin_Asymmetric()
    {
        var m = new Margin { Top = 5, Bottom = 15, Left = 10, Right = 20 };
        Assert.True(m.Bottom > m.Top);
        Assert.True(m.Right > m.Left);
    }

    [Fact]
    public void Margin_Symmetric()
    {
        var m = new Margin { Top = 10, Bottom = 10, Left = 10, Right = 10 };
        Assert.Equal(m.Top, m.Bottom);
        Assert.Equal(m.Left, m.Right);
    }

    [Fact]
    public void Margin_LargeValues()
    {
        var m = new Margin { Top = 50, Bottom = 50, Left = 50, Right = 50 };
        Assert.Equal(50, m.Top);
    }
}
