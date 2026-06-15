using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// LineElement / ShapeElement 完整字段测试：
///   - LineElement 完整字段（Direction/LineWidth/LineColor）
///   - ShapeElement 完整字段（Shape/BorderRadius/FillColor）
///   - 字段组合行为
/// </summary>
public class LineAndShapeElementCompleteTests
{
    // ============== LineElement ==============

    [Fact]
    public void LineElement_Defaults()
    {
        var l = new LineElement();
        Assert.Equal(LineDirection.Horizontal, l.Direction);
        Assert.Equal(1, l.LineWidth);
        Assert.Equal("#000000", l.LineColor);
    }

    [Fact]
    public void LineElement_AllSetters()
    {
        var l = new LineElement
        {
            Direction = LineDirection.Vertical,
            LineWidth = 2.5,
            LineColor = "#FF0000",
        };
        Assert.Equal(LineDirection.Vertical, l.Direction);
        Assert.Equal(2.5, l.LineWidth);
        Assert.Equal("#FF0000", l.LineColor);
    }

    [Fact]
    public void LineElement_Direction_DefaultHorizontal()
    {
        var l = new LineElement();
        Assert.Equal(LineDirection.Horizontal, l.Direction);
    }

    [Fact]
    public void LineElement_Direction_CanBeVertical()
    {
        var l = new LineElement { Direction = LineDirection.Vertical };
        Assert.Equal(LineDirection.Vertical, l.Direction);
    }

    [Fact]
    public void LineElement_Direction_CanBeDiagonal()
    {
        var l = new LineElement { Direction = LineDirection.Diagonal };
        Assert.Equal(LineDirection.Diagonal, l.Direction);
    }

    [Fact]
    public void LineElement_LineWidth_Default1()
    {
        var l = new LineElement();
        Assert.Equal(1, l.LineWidth);
    }

    [Fact]
    public void LineElement_LineWidth_CanBeZero()
    {
        var l = new LineElement { LineWidth = 0 };
        Assert.Equal(0, l.LineWidth);
    }

    [Fact]
    public void LineElement_LineWidth_CanBeDecimal()
    {
        var l = new LineElement { LineWidth = 0.5 };
        Assert.Equal(0.5, l.LineWidth);
    }

    [Fact]
    public void LineElement_LineWidth_CanBeLarge()
    {
        var l = new LineElement { LineWidth = 10 };
        Assert.Equal(10, l.LineWidth);
    }

    [Fact]
    public void LineElement_LineColor_DefaultBlack()
    {
        var l = new LineElement();
        Assert.Equal("#000000", l.LineColor);
    }

    [Fact]
    public void LineElement_LineColor_CanBeRed()
    {
        var l = new LineElement { LineColor = "#FF0000" };
        Assert.Equal("#FF0000", l.LineColor);
    }

    [Fact]
    public void LineElement_LineColor_CanBeHex8()
    {
        var l = new LineElement { LineColor = "#80000000" };
        Assert.Equal("#80000000", l.LineColor);
    }

    [Fact]
    public void LineElement_FullCombination()
    {
        var l = new LineElement
        {
            Direction = LineDirection.Diagonal,
            LineWidth = 1.5,
            LineColor = "#0000FF",
        };
        Assert.Equal(LineDirection.Diagonal, l.Direction);
        Assert.Equal(1.5, l.LineWidth);
        Assert.Equal("#0000FF", l.LineColor);
    }

    // ============== ShapeElement ==============

    [Fact]
    public void ShapeElement_Defaults()
    {
        var s = new ShapeElement();
        Assert.Equal(ShapeType.Rectangle, s.Shape);
        Assert.Equal(0, s.BorderRadius);
        Assert.Equal("#FFFFFF", s.FillColor);
    }

    [Fact]
    public void ShapeElement_AllSetters()
    {
        var s = new ShapeElement
        {
            Shape = ShapeType.RoundedRect,
            BorderRadius = 5,
            FillColor = "#FFCC00",
        };
        Assert.Equal(ShapeType.RoundedRect, s.Shape);
        Assert.Equal(5, s.BorderRadius);
        Assert.Equal("#FFCC00", s.FillColor);
    }

    [Fact]
    public void ShapeElement_Shape_DefaultRectangle()
    {
        var s = new ShapeElement();
        Assert.Equal(ShapeType.Rectangle, s.Shape);
    }

    [Fact]
    public void ShapeElement_Shape_CanBeEllipse()
    {
        var s = new ShapeElement { Shape = ShapeType.Ellipse };
        Assert.Equal(ShapeType.Ellipse, s.Shape);
    }

    [Fact]
    public void ShapeElement_Shape_CanBeRoundedRect()
    {
        var s = new ShapeElement { Shape = ShapeType.RoundedRect };
        Assert.Equal(ShapeType.RoundedRect, s.Shape);
    }

    [Fact]
    public void ShapeElement_Shape_CanBeTriangle()
    {
        var s = new ShapeElement { Shape = ShapeType.Triangle };
        Assert.Equal(ShapeType.Triangle, s.Shape);
    }

    [Fact]
    public void ShapeElement_BorderRadius_Default0()
    {
        var s = new ShapeElement();
        Assert.Equal(0, s.BorderRadius);
    }

    [Fact]
    public void ShapeElement_BorderRadius_CanBePositive()
    {
        var s = new ShapeElement { BorderRadius = 10 };
        Assert.Equal(10, s.BorderRadius);
    }

    [Fact]
    public void ShapeElement_BorderRadius_CanBeDecimal()
    {
        var s = new ShapeElement { BorderRadius = 2.5 };
        Assert.Equal(2.5, s.BorderRadius);
    }

    [Fact]
    public void ShapeElement_FillColor_DefaultWhite()
    {
        var s = new ShapeElement();
        Assert.Equal("#FFFFFF", s.FillColor);
    }

    [Fact]
    public void ShapeElement_FillColor_CanBeBlack()
    {
        var s = new ShapeElement { FillColor = "#000000" };
        Assert.Equal("#000000", s.FillColor);
    }

    [Fact]
    public void ShapeElement_FillColor_CanBeHex8()
    {
        var s = new ShapeElement { FillColor = "#80FF0000" };
        Assert.Equal("#80FF0000", s.FillColor);
    }

    [Fact]
    public void ShapeElement_FullCombination()
    {
        var s = new ShapeElement
        {
            Shape = ShapeType.Ellipse,
            BorderRadius = 0,
            FillColor = "#00FF00",
        };
        Assert.Equal(ShapeType.Ellipse, s.Shape);
        Assert.Equal(0, s.BorderRadius);
        Assert.Equal("#00FF00", s.FillColor);
    }
}
