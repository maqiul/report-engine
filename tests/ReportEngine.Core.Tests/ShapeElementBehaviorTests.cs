using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ShapeElement 行为测试：
///   - 默认值
///   - 形状类型
///   - 圆角半径
///   - 填充颜色
/// </summary>
public class ShapeElementBehaviorTests
{
    // ============== 默认值 ==============

    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var el = new ShapeElement();

        Assert.Equal(ShapeType.Rectangle, el.Shape);
        Assert.Equal(0, el.BorderRadius);
        Assert.Equal("#FFFFFF", el.FillColor);
    }

    // ============== Shape ==============

    [Fact]
    public void Shape_DefaultIsRectangle()
    {
        var el = new ShapeElement();
        Assert.Equal(ShapeType.Rectangle, el.Shape);
    }

    [Fact]
    public void Shape_SetEllipse_Works()
    {
        var el = new ShapeElement { Shape = ShapeType.Ellipse };
        Assert.Equal(ShapeType.Ellipse, el.Shape);
    }

    [Fact]
    public void Shape_SetRoundedRect_Works()
    {
        var el = new ShapeElement { Shape = ShapeType.RoundedRect };
        Assert.Equal(ShapeType.RoundedRect, el.Shape);
    }

    [Fact]
    public void Shape_SetTriangle_Works()
    {
        var el = new ShapeElement { Shape = ShapeType.Triangle };
        Assert.Equal(ShapeType.Triangle, el.Shape);
    }

    [Fact]
    public void Shape_CanBeChanged()
    {
        var el = new ShapeElement { Shape = ShapeType.Rectangle };
        el.Shape = ShapeType.Ellipse;
        Assert.Equal(ShapeType.Ellipse, el.Shape);
    }

    // ============== BorderRadius ==============

    [Fact]
    public void BorderRadius_ZeroByDefault()
    {
        var el = new ShapeElement();
        Assert.Equal(0, el.BorderRadius);
    }

    [Fact]
    public void BorderRadius_SetSmall_Works()
    {
        var el = new ShapeElement { BorderRadius = 3 };
        Assert.Equal(3, el.BorderRadius);
    }

    [Fact]
    public void BorderRadius_SetLarge_Works()
    {
        var el = new ShapeElement { BorderRadius = 10 };
        Assert.Equal(10, el.BorderRadius);
    }

    [Fact]
    public void BorderRadius_SetDecimal_Works()
    {
        var el = new ShapeElement { BorderRadius = 5.5 };
        Assert.Equal(5.5, el.BorderRadius);
    }

    [Fact]
    public void BorderRadius_CanBeChanged()
    {
        var el = new ShapeElement { BorderRadius = 5 };
        el.BorderRadius = 10;
        Assert.Equal(10, el.BorderRadius);
    }

    // ============== FillColor ==============

    [Fact]
    public void FillColor_DefaultIsWhite()
    {
        var el = new ShapeElement();
        Assert.Equal("#FFFFFF", el.FillColor);
    }

    [Fact]
    public void FillColor_SetRed_Works()
    {
        var el = new ShapeElement { FillColor = "#FF0000" };
        Assert.Equal("#FF0000", el.FillColor);
    }

    [Fact]
    public void FillColor_SetBlue_Works()
    {
        var el = new ShapeElement { FillColor = "#0000FF" };
        Assert.Equal("#0000FF", el.FillColor);
    }

    [Fact]
    public void FillColor_SetGray_Works()
    {
        var el = new ShapeElement { FillColor = "#CCCCCC" };
        Assert.Equal("#CCCCCC", el.FillColor);
    }

    [Fact]
    public void FillColor_SetLightBlue_Works()
    {
        var el = new ShapeElement { FillColor = "#E0F0FF" };
        Assert.Equal("#E0F0FF", el.FillColor);
    }

    [Fact]
    public void FillColor_SetWithAlpha_Works()
    {
        var el = new ShapeElement { FillColor = "#80FF0000" };
        Assert.Equal("#80FF0000", el.FillColor);
    }

    [Fact]
    public void FillColor_CanBeChanged()
    {
        var el = new ShapeElement { FillColor = "#FFFFFF" };
        el.FillColor = "#FF0000";
        Assert.Equal("#FF0000", el.FillColor);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void ShapeElement_Rectangle_Works()
    {
        var el = new ShapeElement
        {
            Shape = ShapeType.Rectangle,
            FillColor = "#EEEEEE",
            X = 10,
            Y = 10,
            Width = 100,
            Height = 50
        };

        Assert.Equal(ShapeType.Rectangle, el.Shape);
        Assert.Equal("#EEEEEE", el.FillColor);
    }

    [Fact]
    public void ShapeElement_Ellipse_Works()
    {
        var el = new ShapeElement
        {
            Shape = ShapeType.Ellipse,
            FillColor = "#FFCCCC",
            X = 50,
            Y = 50,
            Width = 80,
            Height = 80
        };

        Assert.Equal(ShapeType.Ellipse, el.Shape);
    }

    [Fact]
    public void ShapeElement_RoundedRect_Works()
    {
        var el = new ShapeElement
        {
            Shape = ShapeType.RoundedRect,
            BorderRadius = 5,
            FillColor = "#CCE0FF",
            X = 10,
            Y = 10,
            Width = 100,
            Height = 50
        };

        Assert.Equal(ShapeType.RoundedRect, el.Shape);
        Assert.Equal(5, el.BorderRadius);
    }

    [Fact]
    public void ShapeElement_Triangle_Works()
    {
        var el = new ShapeElement
        {
            Shape = ShapeType.Triangle,
            FillColor = "#CCFFCC",
            X = 10,
            Y = 10,
            Width = 50,
            Height = 50
        };

        Assert.Equal(ShapeType.Triangle, el.Shape);
    }

    [Fact]
    public void ShapeElement_WithBorder_Works()
    {
        var el = new ShapeElement
        {
            Shape = ShapeType.Rectangle,
            FillColor = "#FFFFFF",
            Border = new BorderDef { Width = 1, Color = "#000000" }
        };

        Assert.NotNull(el.Border);
        Assert.Equal(1, el.Border.Width);
    }

    [Fact]
    public void ShapeElement_InBand_Works()
    {
        var band = new Band { Type = BandType.Detail, Height = 50 };
        band.Elements.Add(new ShapeElement
        {
            Shape = ShapeType.RoundedRect,
            BorderRadius = 3,
            FillColor = "#F0F0F0",
            X = 5,
            Y = 5,
            Width = 90,
            Height = 40
        });

        Assert.Single(band.Elements);
        var shape = Assert.IsType<ShapeElement>(band.Elements[0]);
        Assert.Equal(ShapeType.RoundedRect, shape.Shape);
    }

    [Fact]
    public void ShapeElement_CanBeModified()
    {
        var el = new ShapeElement
        {
            Shape = ShapeType.Rectangle,
            FillColor = "#FFFFFF"
        };
        
        el.Shape = ShapeType.Ellipse;
        el.FillColor = "#FF0000";
        el.BorderRadius = 5;
        
        Assert.Equal(ShapeType.Ellipse, el.Shape);
        Assert.Equal("#FF0000", el.FillColor);
        Assert.Equal(5, el.BorderRadius);
    }
}
