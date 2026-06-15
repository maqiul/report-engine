using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Rendering;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// RenderedElement 基类属性测试
/// </summary>
public class RenderedElementBasePropsTests
{
    [Fact]
    public void RenderedElement_Id_DefaultEmpty()
    {
        var el = new RenderedTextElement();
        Assert.Equal("", el.Id);
    }

    [Fact]
    public void RenderedElement_Id_Settable()
    {
        var el = new RenderedTextElement { Id = "elem1" };
        Assert.Equal("elem1", el.Id);
    }

    [Fact]
    public void RenderedElement_X_Default0()
    {
        var el = new RenderedTextElement();
        Assert.Equal(0, el.X);
    }

    [Fact]
    public void RenderedElement_Y_Default0()
    {
        var el = new RenderedTextElement();
        Assert.Equal(0, el.Y);
    }

    [Fact]
    public void RenderedElement_Width_Default0()
    {
        var el = new RenderedTextElement();
        Assert.Equal(0, el.Width);
    }

    [Fact]
    public void RenderedElement_Height_Default0()
    {
        var el = new RenderedTextElement();
        Assert.Equal(0, el.Height);
    }

    [Fact]
    public void RenderedElement_BackgroundColor_DefaultNull()
    {
        var el = new RenderedTextElement();
        Assert.Null(el.BackgroundColor);
    }

    [Fact]
    public void RenderedElement_BackgroundColor_Settable()
    {
        var el = new RenderedTextElement { BackgroundColor = "#FF0000" };
        Assert.Equal("#FF0000", el.BackgroundColor);
    }

    [Fact]
    public void RenderedElement_Border_DefaultNull()
    {
        var el = new RenderedTextElement();
        Assert.Null(el.Border);
    }

    [Fact]
    public void RenderedElement_Border_Settable()
    {
        var el = new RenderedTextElement { Border = new BorderDef { Width = 1, Color = "#000000" } };
        Assert.NotNull(el.Border);
        Assert.Equal(1, el.Border.Width);
    }

    [Fact]
    public void RenderedElement_FullSetup()
    {
        var el = new RenderedTextElement
        {
            Id = "text1",
            X = 10, Y = 20, Width = 100, Height = 30,
            BackgroundColor = "#EEEEEE",
            Border = new BorderDef { Width = 0.5, Style = BorderStyle.Dashed }
        };
        Assert.Equal("text1", el.Id);
        Assert.Equal(10, el.X);
        Assert.Equal(20, el.Y);
        Assert.Equal(100, el.Width);
        Assert.Equal(30, el.Height);
        Assert.Equal("#EEEEEE", el.BackgroundColor);
        Assert.NotNull(el.Border);
        Assert.Equal(0.5, el.Border.Width);
        Assert.Equal(BorderStyle.Dashed, el.Border.Style);
    }
}
