using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// Margin 行为测试：
///   - 默认值
///   - 各边设置
///   - 对称/非对称边距
///   - 零边距
///   - 大边距
/// </summary>
public class MarginBehaviorTests
{
    // ============== 默认值 ==============

    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var margin = new Margin();

        Assert.Equal(10, margin.Top);
        Assert.Equal(10, margin.Bottom);
        Assert.Equal(10, margin.Left);
        Assert.Equal(10, margin.Right);
    }

    // ============== Top ==============

    [Fact]
    public void Top_SetAndGet_Works()
    {
        var margin = new Margin();
        margin.Top = 10;
        Assert.Equal(10, margin.Top);
    }

    [Fact]
    public void Top_Zero_IsValid()
    {
        var margin = new Margin { Top = 0 };
        Assert.Equal(0, margin.Top);
    }

    [Fact]
    public void Top_LargeValue_IsValid()
    {
        var margin = new Margin { Top = 100 };
        Assert.Equal(100, margin.Top);
    }

    [Fact]
    public void Top_DecimalValue_IsValid()
    {
        var margin = new Margin { Top = 12.5 };
        Assert.Equal(12.5, margin.Top);
    }

    // ============== Bottom ==============

    [Fact]
    public void Bottom_SetAndGet_Works()
    {
        var margin = new Margin();
        margin.Bottom = 15;
        Assert.Equal(15, margin.Bottom);
    }

    [Fact]
    public void Bottom_Zero_IsValid()
    {
        var margin = new Margin { Bottom = 0 };
        Assert.Equal(0, margin.Bottom);
    }

    [Fact]
    public void Bottom_LargeValue_IsValid()
    {
        var margin = new Margin { Bottom = 200 };
        Assert.Equal(200, margin.Bottom);
    }

    [Fact]
    public void Bottom_DecimalValue_IsValid()
    {
        var margin = new Margin { Bottom = 25.4 };
        Assert.Equal(25.4, margin.Bottom);
    }

    // ============== Left ==============

    [Fact]
    public void Left_SetAndGet_Works()
    {
        var margin = new Margin();
        margin.Left = 12;
        Assert.Equal(12, margin.Left);
    }

    [Fact]
    public void Left_Zero_IsValid()
    {
        var margin = new Margin { Left = 0 };
        Assert.Equal(0, margin.Left);
    }

    [Fact]
    public void Left_LargeValue_IsValid()
    {
        var margin = new Margin { Left = 150 };
        Assert.Equal(150, margin.Left);
    }

    [Fact]
    public void Left_DecimalValue_IsValid()
    {
        var margin = new Margin { Left = 18.75 };
        Assert.Equal(18.75, margin.Left);
    }

    // ============== Right ==============

    [Fact]
    public void Right_SetAndGet_Works()
    {
        var margin = new Margin();
        margin.Right = 12;
        Assert.Equal(12, margin.Right);
    }

    [Fact]
    public void Right_Zero_IsValid()
    {
        var margin = new Margin { Right = 0 };
        Assert.Equal(0, margin.Right);
    }

    [Fact]
    public void Right_LargeValue_IsValid()
    {
        var margin = new Margin { Right = 150 };
        Assert.Equal(150, margin.Right);
    }

    [Fact]
    public void Right_DecimalValue_IsValid()
    {
        var margin = new Margin { Right = 20.25 };
        Assert.Equal(20.25, margin.Right);
    }

    // ============== 对称边距 ==============

    [Fact]
    public void SymmetricMargin_AllSidesEqual()
    {
        var margin = new Margin { Top = 25, Bottom = 25, Left = 25, Right = 25 };
        Assert.Equal(margin.Top, margin.Bottom);
        Assert.Equal(margin.Left, margin.Right);
        Assert.Equal(margin.Top, margin.Left);
    }

    [Fact]
    public void SymmetricMargin_Standard1Inch()
    {
        // 1 inch = 25.4 mm
        var margin = new Margin { Top = 25.4, Bottom = 25.4, Left = 25.4, Right = 25.4 };
        Assert.Equal(25.4, margin.Top);
        Assert.Equal(25.4, margin.Bottom);
        Assert.Equal(25.4, margin.Left);
        Assert.Equal(25.4, margin.Right);
    }

    // ============== 非对称边距 ==============

    [Fact]
    public void AsymmetricMargin_DifferentValues()
    {
        var margin = new Margin { Top = 10, Bottom = 20, Left = 15, Right = 25 };
        Assert.Equal(10, margin.Top);
        Assert.Equal(20, margin.Bottom);
        Assert.Equal(15, margin.Left);
        Assert.Equal(25, margin.Right);
    }

    [Fact]
    public void AsymmetricMargin_TopBottomDifferent()
    {
        var margin = new Margin { Top = 30, Bottom = 10, Left = 20, Right = 20 };
        Assert.NotEqual(margin.Top, margin.Bottom);
        Assert.Equal(margin.Left, margin.Right);
    }

    [Fact]
    public void AsymmetricMargin_LeftRightDifferent()
    {
        var margin = new Margin { Top = 20, Bottom = 20, Left = 10, Right = 30 };
        Assert.Equal(margin.Top, margin.Bottom);
        Assert.NotEqual(margin.Left, margin.Right);
    }

    // ============== 零边距 ==============

    [Fact]
    public void ZeroMargin_AllSidesZero()
    {
        var margin = new Margin { Top = 0, Bottom = 0, Left = 0, Right = 0 };
        Assert.Equal(0, margin.Top);
        Assert.Equal(0, margin.Bottom);
        Assert.Equal(0, margin.Left);
        Assert.Equal(0, margin.Right);
    }

    [Fact]
    public void ZeroMargin_SomeSidesZero()
    {
        var margin = new Margin { Top = 0, Bottom = 10, Left = 0, Right = 15 };
        Assert.Equal(0, margin.Top);
        Assert.Equal(10, margin.Bottom);
        Assert.Equal(0, margin.Left);
        Assert.Equal(15, margin.Right);
    }

    // ============== 大边距 ==============

    [Fact]
    public void LargeMargin_AllSidesLarge()
    {
        var margin = new Margin { Top = 50, Bottom = 50, Left = 50, Right = 50 };
        Assert.Equal(50, margin.Top);
        Assert.Equal(50, margin.Bottom);
        Assert.Equal(50, margin.Left);
        Assert.Equal(50, margin.Right);
    }

    [Fact]
    public void LargeMargin_ExtremeValues()
    {
        var margin = new Margin { Top = 100, Bottom = 100, Left = 100, Right = 100 };
        Assert.Equal(100, margin.Top);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void Margin_FullSetup_Works()
    {
        var margin = new Margin
        {
            Top = 25.4,
            Bottom = 25.4,
            Left = 31.8,
            Right = 31.8
        };

        Assert.Equal(25.4, margin.Top);
        Assert.Equal(25.4, margin.Bottom);
        Assert.Equal(31.8, margin.Left);
        Assert.Equal(31.8, margin.Right);
    }

    [Fact]
    public void Margin_BindingMargin_Standard()
    {
        // 装订边距：左边距更大
        var margin = new Margin
        {
            Top = 20,
            Bottom = 20,
            Left = 35,  // 装订边
            Right = 20
        };

        Assert.True(margin.Left > margin.Right);
    }

    [Fact]
    public void Margin_MinimalMargin_ForLabels()
    {
        // 标签打印：最小边距
        var margin = new Margin
        {
            Top = 5,
            Bottom = 5,
            Left = 5,
            Right = 5
        };

        Assert.Equal(5, margin.Top);
        Assert.Equal(5, margin.Bottom);
        Assert.Equal(5, margin.Left);
        Assert.Equal(5, margin.Right);
    }

    [Fact]
    public void Margin_CanBeModified()
    {
        var margin = new Margin { Top = 10, Bottom = 10, Left = 10, Right = 10 };
        
        margin.Top = 20;
        margin.Bottom = 20;
        
        Assert.Equal(20, margin.Top);
        Assert.Equal(20, margin.Bottom);
        Assert.Equal(10, margin.Left);
        Assert.Equal(10, margin.Right);
    }
}
