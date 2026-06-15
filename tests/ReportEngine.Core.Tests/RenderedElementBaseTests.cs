using ReportEngine.Core;
using ReportEngine.Core.Rendering;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// RenderedElement 基类属性测试
/// </summary>
public class RenderedElementBaseTests
{
    // ============== 基类属性 ==============

    [Fact]
    public void RenderedTextElement_Id_DefaultIsEmpty()
    {
        var el = new RenderedTextElement();
        Assert.Equal("", el.Id);
    }

    [Fact]
    public void RenderedTextElement_Id_Set_Works()
    {
        var el = new RenderedTextElement { Id = "text1" };
        Assert.Equal("text1", el.Id);
    }

    [Fact]
    public void RenderedTextElement_Position_DefaultIsZero()
    {
        var el = new RenderedTextElement();
        Assert.Equal(0, el.X);
        Assert.Equal(0, el.Y);
    }

    [Fact]
    public void RenderedTextElement_Position_Set_Works()
    {
        var el = new RenderedTextElement { X = 10.5, Y = 20.3 };
        Assert.Equal(10.5, el.X);
        Assert.Equal(20.3, el.Y);
    }

    [Fact]
    public void RenderedTextElement_Size_DefaultIsZero()
    {
        var el = new RenderedTextElement();
        Assert.Equal(0, el.Width);
        Assert.Equal(0, el.Height);
    }

    [Fact]
    public void RenderedTextElement_Size_Set_Works()
    {
        var el = new RenderedTextElement { Width = 100, Height = 50 };
        Assert.Equal(100, el.Width);
        Assert.Equal(50, el.Height);
    }

    [Fact]
    public void RenderedTextElement_BackgroundColor_DefaultIsNull()
    {
        var el = new RenderedTextElement();
        Assert.Null(el.BackgroundColor);
    }

    [Fact]
    public void RenderedTextElement_BackgroundColor_Set_Works()
    {
        var el = new RenderedTextElement { BackgroundColor = "#FFFFCC" };
        Assert.Equal("#FFFFCC", el.BackgroundColor);
    }

    [Fact]
    public void RenderedTextElement_Border_DefaultIsNull()
    {
        var el = new RenderedTextElement();
        Assert.Null(el.Border);
    }

    [Fact]
    public void RenderedTextElement_Border_Set_Works()
    {
        var el = new RenderedTextElement
        {
            Border = new BorderDef { Width = 1, Color = "#000000" }
        };
        Assert.NotNull(el.Border);
        Assert.Equal(1, el.Border.Width);
    }

    // ============== 各子类继承基类属性 ==============

    [Fact]
    public void RenderedImageElement_InheritsBaseProperties()
    {
        var el = new RenderedImageElement
        {
            Id = "img1",
            X = 5, Y = 10,
            Width = 40, Height = 40,
            BackgroundColor = "#EEEEEE"
        };
        Assert.Equal("img1", el.Id);
        Assert.Equal(5, el.X);
        Assert.Equal(40, el.Width);
        Assert.Equal("#EEEEEE", el.BackgroundColor);
    }

    [Fact]
    public void RenderedLineElement_InheritsBaseProperties()
    {
        var el = new RenderedLineElement
        {
            Id = "line1",
            X = 0, Y = 50,
            Width = 200, Height = 0
        };
        Assert.Equal("line1", el.Id);
        Assert.Equal(200, el.Width);
    }

    [Fact]
    public void RenderedShapeElement_InheritsBaseProperties()
    {
        var el = new RenderedShapeElement
        {
            Id = "shape1",
            X = 10, Y = 10,
            Width = 50, Height = 50,
            BackgroundColor = "#CCCCCC"
        };
        Assert.Equal("shape1", el.Id);
        Assert.Equal("#CCCCCC", el.BackgroundColor);
    }

    [Fact]
    public void RenderedBarcodeElement_InheritsBaseProperties()
    {
        var el = new RenderedBarcodeElement
        {
            Id = "bc1",
            X = 10, Y = 10,
            Width = 100, Height = 50
        };
        Assert.Equal("bc1", el.Id);
        Assert.Equal(100, el.Width);
    }

    [Fact]
    public void RenderedTableElement_InheritsBaseProperties()
    {
        var el = new RenderedTableElement
        {
            Id = "table1",
            X = 10, Y = 100,
            Width = 180, Height = 80
        };
        Assert.Equal("table1", el.Id);
        Assert.Equal(180, el.Width);
    }

    [Fact]
    public void RenderedCrossTabElement_InheritsBaseProperties()
    {
        var el = new RenderedCrossTabElement
        {
            Id = "ct1",
            X = 10, Y = 100,
            Width = 180, Height = 80
        };
        Assert.Equal("ct1", el.Id);
        Assert.Equal(180, el.Width);
    }

    // ============== RenderedPage ==============

    [Fact]
    public void RenderedPage_DefaultValues_AreCorrect()
    {
        var page = new RenderedPage();
        Assert.Equal(0, page.PageNumber);
        Assert.Equal(0, page.TotalPages);
        Assert.NotNull(page.Elements);
        Assert.Empty(page.Elements);
    }

    [Fact]
    public void RenderedPage_SetPageNumber_Works()
    {
        var page = new RenderedPage { PageNumber = 3, TotalPages = 10 };
        Assert.Equal(3, page.PageNumber);
        Assert.Equal(10, page.TotalPages);
    }

    [Fact]
    public void RenderedPage_AddElements_Works()
    {
        var page = new RenderedPage();
        page.Elements.Add(new RenderedTextElement { Text = "A" });
        page.Elements.Add(new RenderedTextElement { Text = "B" });
        Assert.Equal(2, page.Elements.Count);
    }

    // ============== RenderedReport ==============

    [Fact]
    public void RenderedReport_DefaultValues_AreCorrect()
    {
        var report = new RenderedReport();
        Assert.NotNull(report.Template);
        Assert.NotNull(report.Pages);
        Assert.Empty(report.Pages);
        Assert.Equal(0, report.PageWidth);
        Assert.Equal(0, report.PageHeight);
    }

    [Fact]
    public void RenderedReport_SetPageSize_Works()
    {
        var report = new RenderedReport { PageWidth = 210, PageHeight = 297 };
        Assert.Equal(210, report.PageWidth);
        Assert.Equal(297, report.PageHeight);
    }

    [Fact]
    public void RenderedReport_AddPages_Works()
    {
        var report = new RenderedReport();
        report.Pages.Add(new RenderedPage { PageNumber = 1, TotalPages = 3 });
        report.Pages.Add(new RenderedPage { PageNumber = 2, TotalPages = 3 });
        report.Pages.Add(new RenderedPage { PageNumber = 3, TotalPages = 3 });
        Assert.Equal(3, report.Pages.Count);
    }
}
