using ReportEngine.Core;
using ReportEngine.Core.Data;
using Xunit;

namespace ReportEngine.Core.Tests;

// ─────────────────────────────────────────────────────────────────────────────
// ImageElement 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class ImageElementFull2Tests
{
    [Fact]
    public void ImageElement_Source_DefaultEmpty()
    {
        var el = new ImageElement();
        Assert.Equal("", el.Source);
    }

    [Fact]
    public void ImageElement_Source_SetValue()
    {
        var el = new ImageElement { Source = "logo.png" };
        Assert.Equal("logo.png", el.Source);
    }

    [Fact]
    public void ImageElement_Sizing_DefaultFitProportional()
    {
        var el = new ImageElement();
        Assert.Equal(ImageSizing.FitProportional, el.Sizing);
    }

    [Fact]
    public void ImageElement_Sizing_SetStretch()
    {
        var el = new ImageElement { Sizing = ImageSizing.Stretch };
        Assert.Equal(ImageSizing.Stretch, el.Sizing);
    }

    [Fact]
    public void ImageElement_Sizing_SetClip()
    {
        var el = new ImageElement { Sizing = ImageSizing.Clip };
        Assert.Equal(ImageSizing.Clip, el.Sizing);
    }

    [Fact]
    public void ImageElement_Sizing_SetActualSize()
    {
        var el = new ImageElement { Sizing = ImageSizing.ActualSize };
        Assert.Equal(ImageSizing.ActualSize, el.Sizing);
    }

    [Fact]
    public void ImageElement_InheritsBaseProps()
    {
        var el = new ImageElement { X = 10, Y = 20, Width = 50, Height = 50, Visible = true };
        Assert.Equal(10, el.X);
        Assert.Equal(20, el.Y);
        Assert.Equal(50, el.Width);
        Assert.Equal(50, el.Height);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// LineElement 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class LineElementFull2Tests
{
    [Fact]
    public void LineElement_Direction_DefaultHorizontal()
    {
        var el = new LineElement();
        Assert.Equal(LineDirection.Horizontal, el.Direction);
    }

    [Fact]
    public void LineElement_Direction_SetVertical()
    {
        var el = new LineElement { Direction = LineDirection.Vertical };
        Assert.Equal(LineDirection.Vertical, el.Direction);
    }

    [Fact]
    public void LineElement_Direction_SetDiagonal()
    {
        var el = new LineElement { Direction = LineDirection.Diagonal };
        Assert.Equal(LineDirection.Diagonal, el.Direction);
    }

    [Fact]
    public void LineElement_LineWidth_Default1()
    {
        var el = new LineElement();
        Assert.Equal(1, el.LineWidth);
    }

    [Fact]
    public void LineElement_LineWidth_SetValue()
    {
        var el = new LineElement { LineWidth = 2.5 };
        Assert.Equal(2.5, el.LineWidth);
    }

    [Fact]
    public void LineElement_LineColor_Default000000()
    {
        var el = new LineElement();
        Assert.Equal("#000000", el.LineColor);
    }

    [Fact]
    public void LineElement_LineColor_SetValue()
    {
        var el = new LineElement { LineColor = "#00FF00" };
        Assert.Equal("#00FF00", el.LineColor);
    }
}
