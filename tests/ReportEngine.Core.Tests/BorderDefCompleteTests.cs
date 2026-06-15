using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// BorderDef / BorderStyle 完整字段测试：
///   - BorderDef 完整字段（Width/Color/Style/Top/Bottom/Left/Right）
///   - BorderStyle 枚举 4 值
///   - 字段组合行为
/// </summary>
public class BorderDefCompleteTests
{
    [Fact]
    public void BorderDef_Defaults()
    {
        var b = new BorderDef();
        Assert.Equal(1, b.Width);
        Assert.Equal("#000000", b.Color);
        Assert.Equal(BorderStyle.Solid, b.Style);
        Assert.False(b.Top);
        Assert.False(b.Bottom);
        Assert.False(b.Left);
        Assert.False(b.Right);
    }

    [Fact]
    public void BorderDef_AllSetters()
    {
        var b = new BorderDef
        {
            Width = 2.5,
            Color = "#FF0000",
            Style = BorderStyle.Dashed,
            Top = true,
            Bottom = true,
            Left = true,
            Right = true,
        };
        Assert.Equal(2.5, b.Width);
        Assert.Equal("#FF0000", b.Color);
        Assert.Equal(BorderStyle.Dashed, b.Style);
        Assert.True(b.Top);
        Assert.True(b.Bottom);
        Assert.True(b.Left);
        Assert.True(b.Right);
    }

    [Fact]
    public void BorderDef_Width_CanBeZero()
    {
        var b = new BorderDef { Width = 0 };
        Assert.Equal(0, b.Width);
    }

    [Fact]
    public void BorderDef_Width_CanBeDecimal()
    {
        var b = new BorderDef { Width = 0.5 };
        Assert.Equal(0.5, b.Width);
    }

    [Fact]
    public void BorderDef_Width_CanBeLarge()
    {
        var b = new BorderDef { Width = 10 };
        Assert.Equal(10, b.Width);
    }

    [Fact]
    public void BorderDef_Color_DefaultBlack()
    {
        var b = new BorderDef();
        Assert.Equal("#000000", b.Color);
    }

    [Fact]
    public void BorderDef_Color_CanBeRed()
    {
        var b = new BorderDef { Color = "#FF0000" };
        Assert.Equal("#FF0000", b.Color);
    }

    [Fact]
    public void BorderDef_Color_CanBeHex8()
    {
        var b = new BorderDef { Color = "#80FF0000" };
        Assert.Equal("#80FF0000", b.Color);
    }

    [Fact]
    public void BorderDef_Style_DefaultSolid()
    {
        var b = new BorderDef();
        Assert.Equal(BorderStyle.Solid, b.Style);
    }

    [Fact]
    public void BorderDef_Style_CanBeDashed()
    {
        var b = new BorderDef { Style = BorderStyle.Dashed };
        Assert.Equal(BorderStyle.Dashed, b.Style);
    }

    [Fact]
    public void BorderDef_Style_CanBeDotted()
    {
        var b = new BorderDef { Style = BorderStyle.Dotted };
        Assert.Equal(BorderStyle.Dotted, b.Style);
    }

    [Fact]
    public void BorderDef_Style_CanBeNone()
    {
        var b = new BorderDef { Style = BorderStyle.None };
        Assert.Equal(BorderStyle.None, b.Style);
    }

    [Fact]
    public void BorderStyle_Has4Values()
    {
        Assert.Equal(4, System.Enum.GetValues(typeof(BorderStyle)).Length);
    }

    [Fact]
    public void BorderDef_Top_DefaultFalse()
    {
        var b = new BorderDef();
        Assert.False(b.Top);
    }

    [Fact]
    public void BorderDef_Top_CanBeTrue()
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
    public void BorderDef_Bottom_CanBeTrue()
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
    public void BorderDef_Left_CanBeTrue()
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
    public void BorderDef_Right_CanBeTrue()
    {
        var b = new BorderDef { Right = true };
        Assert.True(b.Right);
    }

    [Fact]
    public void BorderDef_AllSidesTrue()
    {
        var b = new BorderDef { Top = true, Bottom = true, Left = true, Right = true };
        Assert.True(b.Top && b.Bottom && b.Left && b.Right);
    }

    [Fact]
    public void BorderDef_OnlyTopBorder()
    {
        var b = new BorderDef { Top = true };
        Assert.True(b.Top);
        Assert.False(b.Bottom && b.Left && b.Right);
    }

    [Fact]
    public void BorderDef_FullCombination()
    {
        var b = new BorderDef
        {
            Width = 1.5,
            Color = "#0000FF",
            Style = BorderStyle.Dotted,
            Top = true,
            Bottom = true,
            Left = false,
            Right = false,
        };
        Assert.Equal(1.5, b.Width);
        Assert.Equal("#0000FF", b.Color);
        Assert.Equal(BorderStyle.Dotted, b.Style);
        Assert.True(b.Top);
        Assert.True(b.Bottom);
        Assert.False(b.Left);
        Assert.False(b.Right);
    }
}
