using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// LineElement 行为测试：
///   - 默认值
///   - 方向
///   - 线宽
///   - 颜色
/// </summary>
public class LineElementBehaviorTests
{
    // ============== 默认值 ==============

    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var el = new LineElement();

        Assert.Equal(LineDirection.Horizontal, el.Direction);
        Assert.Equal(1, el.LineWidth);
        Assert.Equal("#000000", el.LineColor);
    }

    // ============== Direction ==============

    [Fact]
    public void Direction_DefaultIsHorizontal()
    {
        var el = new LineElement();
        Assert.Equal(LineDirection.Horizontal, el.Direction);
    }

    [Fact]
    public void Direction_SetVertical_Works()
    {
        var el = new LineElement { Direction = LineDirection.Vertical };
        Assert.Equal(LineDirection.Vertical, el.Direction);
    }

    [Fact]
    public void Direction_SetDiagonal_Works()
    {
        var el = new LineElement { Direction = LineDirection.Diagonal };
        Assert.Equal(LineDirection.Diagonal, el.Direction);
    }

    [Fact]
    public void Direction_CanBeChanged()
    {
        var el = new LineElement { Direction = LineDirection.Horizontal };
        el.Direction = LineDirection.Vertical;
        Assert.Equal(LineDirection.Vertical, el.Direction);
    }

    // ============== LineWidth ==============

    [Fact]
    public void LineWidth_DefaultIs1()
    {
        var el = new LineElement();
        Assert.Equal(1, el.LineWidth);
    }

    [Fact]
    public void LineWidth_SetThin_Works()
    {
        var el = new LineElement { LineWidth = 0.5 };
        Assert.Equal(0.5, el.LineWidth);
    }

    [Fact]
    public void LineWidth_SetThick_Works()
    {
        var el = new LineElement { LineWidth = 3 };
        Assert.Equal(3, el.LineWidth);
    }

    [Fact]
    public void LineWidth_SetDecimal_Works()
    {
        var el = new LineElement { LineWidth = 1.5 };
        Assert.Equal(1.5, el.LineWidth);
    }

    [Fact]
    public void LineWidth_SetVeryThin_Works()
    {
        var el = new LineElement { LineWidth = 0.25 };
        Assert.Equal(0.25, el.LineWidth);
    }

    [Fact]
    public void LineWidth_SetVeryThick_Works()
    {
        var el = new LineElement { LineWidth = 5 };
        Assert.Equal(5, el.LineWidth);
    }

    [Fact]
    public void LineWidth_CanBeChanged()
    {
        var el = new LineElement { LineWidth = 1 };
        el.LineWidth = 2;
        Assert.Equal(2, el.LineWidth);
    }

    // ============== LineColor ==============

    [Fact]
    public void LineColor_DefaultIsBlack()
    {
        var el = new LineElement();
        Assert.Equal("#000000", el.LineColor);
    }

    [Fact]
    public void LineColor_SetRed_Works()
    {
        var el = new LineElement { LineColor = "#FF0000" };
        Assert.Equal("#FF0000", el.LineColor);
    }

    [Fact]
    public void LineColor_SetBlue_Works()
    {
        var el = new LineElement { LineColor = "#0000FF" };
        Assert.Equal("#0000FF", el.LineColor);
    }

    [Fact]
    public void LineColor_SetGray_Works()
    {
        var el = new LineElement { LineColor = "#808080" };
        Assert.Equal("#808080", el.LineColor);
    }

    [Fact]
    public void LineColor_SetLightGray_Works()
    {
        var el = new LineElement { LineColor = "#CCCCCC" };
        Assert.Equal("#CCCCCC", el.LineColor);
    }

    [Fact]
    public void LineColor_SetWithAlpha_Works()
    {
        var el = new LineElement { LineColor = "#80FF0000" };
        Assert.Equal("#80FF0000", el.LineColor);
    }

    [Fact]
    public void LineColor_CanBeChanged()
    {
        var el = new LineElement { LineColor = "#000000" };
        el.LineColor = "#FF0000";
        Assert.Equal("#FF0000", el.LineColor);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void LineElement_HorizontalLine_Works()
    {
        var el = new LineElement
        {
            Direction = LineDirection.Horizontal,
            LineWidth = 1,
            LineColor = "#000000",
            X = 10,
            Y = 50,
            Width = 180,
            Height = 0
        };

        Assert.Equal(LineDirection.Horizontal, el.Direction);
        Assert.Equal(1, el.LineWidth);
        Assert.Equal(180, el.Width);
    }

    [Fact]
    public void LineElement_VerticalLine_Works()
    {
        var el = new LineElement
        {
            Direction = LineDirection.Vertical,
            LineWidth = 1,
            LineColor = "#000000",
            X = 100,
            Y = 10,
            Width = 0,
            Height = 200
        };

        Assert.Equal(LineDirection.Vertical, el.Direction);
        Assert.Equal(200, el.Height);
    }

    [Fact]
    public void LineElement_DiagonalLine_Works()
    {
        var el = new LineElement
        {
            Direction = LineDirection.Diagonal,
            LineWidth = 2,
            LineColor = "#FF0000",
            X = 10,
            Y = 10,
            Width = 100,
            Height = 100
        };

        Assert.Equal(LineDirection.Diagonal, el.Direction);
        Assert.Equal(2, el.LineWidth);
    }

    [Fact]
    public void LineElement_SeparatorLine_Works()
    {
        var el = new LineElement
        {
            Direction = LineDirection.Horizontal,
            LineWidth = 0.5,
            LineColor = "#CCCCCC"
        };

        Assert.Equal(0.5, el.LineWidth);
        Assert.Equal("#CCCCCC", el.LineColor);
    }

    [Fact]
    public void LineElement_BoldLine_Works()
    {
        var el = new LineElement
        {
            Direction = LineDirection.Horizontal,
            LineWidth = 3,
            LineColor = "#000000"
        };

        Assert.Equal(3, el.LineWidth);
    }

    [Fact]
    public void LineElement_InBand_Works()
    {
        var band = new Band { Type = BandType.Detail, Height = 30 };
        band.Elements.Add(new LineElement
        {
            Direction = LineDirection.Horizontal,
            X = 10,
            Y = 25,
            Width = 180,
            LineWidth = 0.5,
            LineColor = "#CCCCCC"
        });

        Assert.Single(band.Elements);
        var line = Assert.IsType<LineElement>(band.Elements[0]);
        Assert.Equal(LineDirection.Horizontal, line.Direction);
    }

    [Fact]
    public void LineElement_CanBeModified()
    {
        var el = new LineElement
        {
            Direction = LineDirection.Horizontal,
            LineWidth = 1,
            LineColor = "#000000"
        };
        
        el.Direction = LineDirection.Vertical;
        el.LineWidth = 2;
        el.LineColor = "#FF0000";
        
        Assert.Equal(LineDirection.Vertical, el.Direction);
        Assert.Equal(2, el.LineWidth);
        Assert.Equal("#FF0000", el.LineColor);
    }
}
