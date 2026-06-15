using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// BorderDef 行为测试：
///   - 默认值
///   - 宽度
///   - 颜色
///   - 样式
///   - 四边独立设置
/// </summary>
public class BorderDefBehaviorTests
{
    // ============== 默认值 ==============

    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var border = new BorderDef();

        Assert.Equal(1, border.Width);
        Assert.Equal("#000000", border.Color);
        Assert.Equal(BorderStyle.Solid, border.Style);
        Assert.False(border.Top);
        Assert.False(border.Bottom);
        Assert.False(border.Left);
        Assert.False(border.Right);
    }

    // ============== Width ==============

    [Fact]
    public void Width_DefaultIs1()
    {
        var border = new BorderDef();
        Assert.Equal(1, border.Width);
    }

    [Fact]
    public void Width_SetThin_Works()
    {
        var border = new BorderDef { Width = 0.5 };
        Assert.Equal(0.5, border.Width);
    }

    [Fact]
    public void Width_SetThick_Works()
    {
        var border = new BorderDef { Width = 3 };
        Assert.Equal(3, border.Width);
    }

    [Fact]
    public void Width_SetDecimal_Works()
    {
        var border = new BorderDef { Width = 1.5 };
        Assert.Equal(1.5, border.Width);
    }

    [Fact]
    public void Width_SetVeryThin_Works()
    {
        var border = new BorderDef { Width = 0.25 };
        Assert.Equal(0.25, border.Width);
    }

    [Fact]
    public void Width_SetVeryThick_Works()
    {
        var border = new BorderDef { Width = 5 };
        Assert.Equal(5, border.Width);
    }

    // ============== Color ==============

    [Fact]
    public void Color_DefaultIsBlack()
    {
        var border = new BorderDef();
        Assert.Equal("#000000", border.Color);
    }

    [Fact]
    public void Color_SetRed_Works()
    {
        var border = new BorderDef { Color = "#FF0000" };
        Assert.Equal("#FF0000", border.Color);
    }

    [Fact]
    public void Color_SetBlue_Works()
    {
        var border = new BorderDef { Color = "#0000FF" };
        Assert.Equal("#0000FF", border.Color);
    }

    [Fact]
    public void Color_SetGray_Works()
    {
        var border = new BorderDef { Color = "#808080" };
        Assert.Equal("#808080", border.Color);
    }

    [Fact]
    public void Color_SetWithAlpha_Works()
    {
        var border = new BorderDef { Color = "#80FF0000" };
        Assert.Equal("#80FF0000", border.Color);
    }

    [Fact]
    public void Color_SetLightGray_Works()
    {
        var border = new BorderDef { Color = "#CCCCCC" };
        Assert.Equal("#CCCCCC", border.Color);
    }

    // ============== Style ==============

    [Fact]
    public void Style_DefaultIsSolid()
    {
        var border = new BorderDef();
        Assert.Equal(BorderStyle.Solid, border.Style);
    }

    [Fact]
    public void Style_SetDashed_Works()
    {
        var border = new BorderDef { Style = BorderStyle.Dashed };
        Assert.Equal(BorderStyle.Dashed, border.Style);
    }

    [Fact]
    public void Style_SetDotted_Works()
    {
        var border = new BorderDef { Style = BorderStyle.Dotted };
        Assert.Equal(BorderStyle.Dotted, border.Style);
    }

    [Fact]
    public void Style_SetNone_Works()
    {
        var border = new BorderDef { Style = BorderStyle.None };
        Assert.Equal(BorderStyle.None, border.Style);
    }

    [Fact]
    public void Style_CanBeChanged()
    {
        var border = new BorderDef { Style = BorderStyle.Solid };
        border.Style = BorderStyle.Dashed;
        Assert.Equal(BorderStyle.Dashed, border.Style);
    }

    // ============== Top ==============

    [Fact]
    public void Top_DefaultIsFalse()
    {
        var border = new BorderDef();
        Assert.False(border.Top);
    }

    [Fact]
    public void Top_SetTrue_Works()
    {
        var border = new BorderDef { Top = true };
        Assert.True(border.Top);
    }

    [Fact]
    public void Top_CanBeToggled()
    {
        var border = new BorderDef { Top = true };
        border.Top = false;
        Assert.False(border.Top);
    }

    // ============== Bottom ==============

    [Fact]
    public void Bottom_DefaultIsFalse()
    {
        var border = new BorderDef();
        Assert.False(border.Bottom);
    }

    [Fact]
    public void Bottom_SetTrue_Works()
    {
        var border = new BorderDef { Bottom = true };
        Assert.True(border.Bottom);
    }

    [Fact]
    public void Bottom_CanBeToggled()
    {
        var border = new BorderDef { Bottom = true };
        border.Bottom = false;
        Assert.False(border.Bottom);
    }

    // ============== Left ==============

    [Fact]
    public void Left_DefaultIsFalse()
    {
        var border = new BorderDef();
        Assert.False(border.Left);
    }

    [Fact]
    public void Left_SetTrue_Works()
    {
        var border = new BorderDef { Left = true };
        Assert.True(border.Left);
    }

    [Fact]
    public void Left_CanBeToggled()
    {
        var border = new BorderDef { Left = true };
        border.Left = false;
        Assert.False(border.Left);
    }

    // ============== Right ==============

    [Fact]
    public void Right_DefaultIsFalse()
    {
        var border = new BorderDef();
        Assert.False(border.Right);
    }

    [Fact]
    public void Right_SetTrue_Works()
    {
        var border = new BorderDef { Right = true };
        Assert.True(border.Right);
    }

    [Fact]
    public void Right_CanBeToggled()
    {
        var border = new BorderDef { Right = true };
        border.Right = false;
        Assert.False(border.Right);
    }

    // ============== 四边组合 ==============

    [Fact]
    public void AllSides_True()
    {
        var border = new BorderDef { Top = true, Bottom = true, Left = true, Right = true };
        Assert.True(border.Top);
        Assert.True(border.Bottom);
        Assert.True(border.Left);
        Assert.True(border.Right);
    }

    [Fact]
    public void TopAndBottom_Only()
    {
        var border = new BorderDef { Top = true, Bottom = true, Left = false, Right = false };
        Assert.True(border.Top);
        Assert.True(border.Bottom);
        Assert.False(border.Left);
        Assert.False(border.Right);
    }

    [Fact]
    public void LeftAndRight_Only()
    {
        var border = new BorderDef { Top = false, Bottom = false, Left = true, Right = true };
        Assert.False(border.Top);
        Assert.False(border.Bottom);
        Assert.True(border.Left);
        Assert.True(border.Right);
    }

    [Fact]
    public void TopAndLeft_Only()
    {
        var border = new BorderDef { Top = true, Bottom = false, Left = true, Right = false };
        Assert.True(border.Top);
        Assert.False(border.Bottom);
        Assert.True(border.Left);
        Assert.False(border.Right);
    }

    [Fact]
    public void BottomAndRight_Only()
    {
        var border = new BorderDef { Top = false, Bottom = true, Left = false, Right = true };
        Assert.False(border.Top);
        Assert.True(border.Bottom);
        Assert.False(border.Left);
        Assert.True(border.Right);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void BorderDef_FullSetup_Works()
    {
        var border = new BorderDef
        {
            Width = 2,
            Color = "#333333",
            Style = BorderStyle.Dashed,
            Top = true,
            Bottom = true,
            Left = true,
            Right = true
        };

        Assert.Equal(2, border.Width);
        Assert.Equal("#333333", border.Color);
        Assert.Equal(BorderStyle.Dashed, border.Style);
        Assert.True(border.Top);
        Assert.True(border.Bottom);
        Assert.True(border.Left);
        Assert.True(border.Right);
    }

    [Fact]
    public void BorderDef_TableBorder_Works()
    {
        var border = new BorderDef
        {
            Width = 0.5,
            Color = "#CCCCCC",
            Style = BorderStyle.Solid,
            Top = true,
            Bottom = true,
            Left = true,
            Right = true
        };

        Assert.Equal(0.5, border.Width);
        Assert.Equal("#CCCCCC", border.Color);
    }

    [Fact]
    public void BorderDef_HeaderBorder_Works()
    {
        var border = new BorderDef
        {
            Width = 1.5,
            Color = "#000000",
            Style = BorderStyle.Solid,
            Top = true,
            Bottom = true,
            Left = false,
            Right = false
        };

        Assert.Equal(1.5, border.Width);
        Assert.True(border.Top);
        Assert.True(border.Bottom);
        Assert.False(border.Left);
        Assert.False(border.Right);
    }

    [Fact]
    public void BorderDef_NoBorder_Works()
    {
        var border = new BorderDef
        {
            Width = 0,
            Style = BorderStyle.None,
            Top = false,
            Bottom = false,
            Left = false,
            Right = false
        };

        Assert.Equal(0, border.Width);
        Assert.Equal(BorderStyle.None, border.Style);
        Assert.False(border.Top);
        Assert.False(border.Bottom);
    }

    [Fact]
    public void BorderDef_CanBeModified()
    {
        var border = new BorderDef();
        
        border.Width = 2;
        border.Color = "#FF0000";
        border.Style = BorderStyle.Dotted;
        border.Top = true;
        border.Bottom = true;
        
        Assert.Equal(2, border.Width);
        Assert.Equal("#FF0000", border.Color);
        Assert.Equal(BorderStyle.Dotted, border.Style);
        Assert.True(border.Top);
        Assert.True(border.Bottom);
    }
}
