using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Rendering;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// RenderedReport 完整属性测试
/// </summary>
public class RenderedReportCompleteTests
{
    [Fact]
    public void RenderedReport_Template_DefaultNotNull()
    {
        var r = new RenderedReport();
        Assert.NotNull(r.Template);
    }

    [Fact]
    public void RenderedReport_Pages_DefaultEmpty()
    {
        var r = new RenderedReport();
        Assert.NotNull(r.Pages);
        Assert.Empty(r.Pages);
    }

    [Fact]
    public void RenderedReport_Pages_Addable()
    {
        var r = new RenderedReport();
        r.Pages.Add(new RenderedPage { PageNumber = 1, TotalPages = 1 });
        Assert.Single(r.Pages);
    }

    [Fact]
    public void RenderedReport_PageWidth_Default0()
    {
        var r = new RenderedReport();
        Assert.Equal(0, r.PageWidth);
    }

    [Fact]
    public void RenderedReport_PageWidth_Settable()
    {
        var r = new RenderedReport { PageWidth = 210 };
        Assert.Equal(210, r.PageWidth);
    }

    [Fact]
    public void RenderedReport_PageHeight_Default0()
    {
        var r = new RenderedReport();
        Assert.Equal(0, r.PageHeight);
    }

    [Fact]
    public void RenderedReport_PageHeight_Settable()
    {
        var r = new RenderedReport { PageHeight = 297 };
        Assert.Equal(297, r.PageHeight);
    }

    [Fact]
    public void RenderedReport_FitToWidth_ScalesDown()
    {
        var r = new RenderedReport { PageWidth = 210, PageHeight = 297 };
        r.Pages.Add(new RenderedPage());
        r.Pages[0].Elements.Add(new RenderedTextElement { X = 100, Y = 100, Width = 50, Height = 20 });
        
        r.FitToWidth(105); // 50% scale
        
        Assert.Equal(105, r.PageWidth);
        Assert.Equal(148.5, r.PageHeight, 1);
        Assert.Equal(50, r.Pages[0].Elements[0].X, 1);
        Assert.Equal(50, r.Pages[0].Elements[0].Y, 1);
    }

    [Fact]
    public void RenderedReport_FitToWidth_NoScaleIfAlreadySmaller()
    {
        var r = new RenderedReport { PageWidth = 100, PageHeight = 150 };
        r.Pages.Add(new RenderedPage());
        r.Pages[0].Elements.Add(new RenderedTextElement { X = 50, Y = 50, Width = 25, Height = 10 });
        
        r.FitToWidth(200); // target > current, no scale
        
        Assert.Equal(100, r.PageWidth); // unchanged
        Assert.Equal(150, r.PageHeight); // unchanged
        Assert.Equal(50, r.Pages[0].Elements[0].X);
    }

    [Fact]
    public void RenderedReport_FitToWidth_NoScaleIfZero()
    {
        var r = new RenderedReport { PageWidth = 0, PageHeight = 0 };
        r.FitToWidth(100); // should not throw
        Assert.Equal(0, r.PageWidth);
    }

    [Fact]
    public void RenderedReport_Scale_ScalesAll()
    {
        var r = new RenderedReport { PageWidth = 210, PageHeight = 297 };
        r.Pages.Add(new RenderedPage());
        r.Pages[0].Elements.Add(new RenderedTextElement { X = 100, Y = 100, Width = 50, Height = 20 });
        
        r.Scale(2.0);
        
        Assert.Equal(420, r.PageWidth);
        Assert.Equal(594, r.PageHeight);
        Assert.Equal(200, r.Pages[0].Elements[0].X);
        Assert.Equal(200, r.Pages[0].Elements[0].Y);
        Assert.Equal(100, r.Pages[0].Elements[0].Width);
        Assert.Equal(40, r.Pages[0].Elements[0].Height);
    }

    [Fact]
    public void RenderedReport_Scale_NoScaleIfOne()
    {
        var r = new RenderedReport { PageWidth = 210, PageHeight = 297 };
        r.Pages.Add(new RenderedPage());
        r.Pages[0].Elements.Add(new RenderedTextElement { X = 100, Y = 100, Width = 50, Height = 20 });
        
        r.Scale(1.0);
        
        Assert.Equal(210, r.PageWidth);
        Assert.Equal(297, r.PageHeight);
        Assert.Equal(100, r.Pages[0].Elements[0].X);
    }

    [Fact]
    public void RenderedReport_Scale_NoScaleIfZero()
    {
        var r = new RenderedReport { PageWidth = 210, PageHeight = 297 };
        r.Scale(0);
        Assert.Equal(210, r.PageWidth); // unchanged
    }
}

/// <summary>
/// RenderedPage 完整属性测试
/// </summary>
public class RenderedPageCompleteTests
{
    [Fact]
    public void RenderedPage_PageNumber_Default0()
    {
        var p = new RenderedPage();
        Assert.Equal(0, p.PageNumber);
    }

    [Fact]
    public void RenderedPage_PageNumber_Settable()
    {
        var p = new RenderedPage { PageNumber = 3 };
        Assert.Equal(3, p.PageNumber);
    }

    [Fact]
    public void RenderedPage_TotalPages_Default0()
    {
        var p = new RenderedPage();
        Assert.Equal(0, p.TotalPages);
    }

    [Fact]
    public void RenderedPage_TotalPages_Settable()
    {
        var p = new RenderedPage { TotalPages = 10 };
        Assert.Equal(10, p.TotalPages);
    }

    [Fact]
    public void RenderedPage_Elements_DefaultEmpty()
    {
        var p = new RenderedPage();
        Assert.NotNull(p.Elements);
        Assert.Empty(p.Elements);
    }

    [Fact]
    public void RenderedPage_Elements_Addable()
    {
        var p = new RenderedPage();
        p.Elements.Add(new RenderedTextElement { Text = "Hello" });
        Assert.Single(p.Elements);
    }

    [Fact]
    public void RenderedPage_MultipleElements()
    {
        var p = new RenderedPage();
        p.Elements.Add(new RenderedTextElement { Text = "Title" });
        p.Elements.Add(new RenderedImageElement { Source = "logo.png" });
        p.Elements.Add(new RenderedLineElement { Direction = LineDirection.Horizontal });
        Assert.Equal(3, p.Elements.Count);
    }
}

/// <summary>
/// RenderedTextElement 完整属性测试
/// </summary>
public class RenderedTextElementCompleteTests
{
    [Fact]
    public void RenderedTextElement_Text_DefaultEmpty()
    {
        var el = new RenderedTextElement();
        Assert.Equal("", el.Text);
    }

    [Fact]
    public void RenderedTextElement_Text_Settable()
    {
        var el = new RenderedTextElement { Text = "Hello World" };
        Assert.Equal("Hello World", el.Text);
    }

    [Fact]
    public void RenderedTextElement_Font_DefaultNotNull()
    {
        var el = new RenderedTextElement();
        Assert.NotNull(el.Font);
    }

    [Fact]
    public void RenderedTextElement_Font_Settable()
    {
        var el = new RenderedTextElement { Font = new FontDef { Size = 14, Bold = true } };
        Assert.Equal(14, el.Font.Size);
        Assert.True(el.Font.Bold);
    }

    [Fact]
    public void RenderedTextElement_Alignment_DefaultLeft()
    {
        var el = new RenderedTextElement();
        Assert.Equal(TextAlignment.Left, el.Alignment);
    }

    [Fact]
    public void RenderedTextElement_Alignment_Center()
    {
        var el = new RenderedTextElement { Alignment = TextAlignment.Center };
        Assert.Equal(TextAlignment.Center, el.Alignment);
    }

    [Fact]
    public void RenderedTextElement_Alignment_Right()
    {
        var el = new RenderedTextElement { Alignment = TextAlignment.Right };
        Assert.Equal(TextAlignment.Right, el.Alignment);
    }

    [Fact]
    public void RenderedTextElement_Hyperlink_DefaultNull()
    {
        var el = new RenderedTextElement();
        Assert.Null(el.Hyperlink);
    }

    [Fact]
    public void RenderedTextElement_Hyperlink_Settable()
    {
        var el = new RenderedTextElement { Hyperlink = "https://example.com" };
        Assert.Equal("https://example.com", el.Hyperlink);
    }

    [Fact]
    public void RenderedTextElement_FullSetup()
    {
        var el = new RenderedTextElement
        {
            X = 10, Y = 20, Width = 100, Height = 30,
            Text = "Click here",
            Font = new FontDef { Size = 12, Color = "#0000FF" },
            Alignment = TextAlignment.Center,
            Hyperlink = "https://example.com",
            BackgroundColor = "#FFFF00"
        };
        Assert.Equal(10, el.X);
        Assert.Equal(20, el.Y);
        Assert.Equal(100, el.Width);
        Assert.Equal(30, el.Height);
        Assert.Equal("Click here", el.Text);
        Assert.Equal(12, el.Font.Size);
        Assert.Equal(TextAlignment.Center, el.Alignment);
        Assert.Equal("https://example.com", el.Hyperlink);
        Assert.Equal("#FFFF00", el.BackgroundColor);
    }
}

/// <summary>
/// RenderedImageElement 完整属性测试
/// </summary>
public class RenderedImageElementCompleteTests
{
    [Fact]
    public void RenderedImageElement_Source_DefaultEmpty()
    {
        var el = new RenderedImageElement();
        Assert.Equal("", el.Source);
    }

    [Fact]
    public void RenderedImageElement_Source_Settable()
    {
        var el = new RenderedImageElement { Source = "logo.png" };
        Assert.Equal("logo.png", el.Source);
    }

    [Fact]
    public void RenderedImageElement_FullSetup()
    {
        var el = new RenderedImageElement
        {
            X = 50, Y = 50, Width = 100, Height = 100,
            Source = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg=="
        };
        Assert.Equal(50, el.X);
        Assert.Equal(50, el.Y);
        Assert.Equal(100, el.Width);
        Assert.Equal(100, el.Height);
        Assert.StartsWith("data:image/png;base64,", el.Source);
    }
}

/// <summary>
/// RenderedLineElement 完整属性测试
/// </summary>
public class RenderedLineElementCompleteTests
{
    [Fact]
    public void RenderedLineElement_Direction_DefaultHorizontal()
    {
        var el = new RenderedLineElement();
        Assert.Equal(LineDirection.Horizontal, el.Direction);
    }

    [Fact]
    public void RenderedLineElement_Direction_Vertical()
    {
        var el = new RenderedLineElement { Direction = LineDirection.Vertical };
        Assert.Equal(LineDirection.Vertical, el.Direction);
    }

    [Fact]
    public void RenderedLineElement_Direction_Diagonal()
    {
        var el = new RenderedLineElement { Direction = LineDirection.Diagonal };
        Assert.Equal(LineDirection.Diagonal, el.Direction);
    }

    [Fact]
    public void RenderedLineElement_LineWidth_Default0()
    {
        var el = new RenderedLineElement();
        Assert.Equal(0, el.LineWidth);
    }

    [Fact]
    public void RenderedLineElement_LineWidth_Settable()
    {
        var el = new RenderedLineElement { LineWidth = 2.5 };
        Assert.Equal(2.5, el.LineWidth);
    }

    [Fact]
    public void RenderedLineElement_LineColor_Default000000()
    {
        var el = new RenderedLineElement();
        Assert.Equal("#000000", el.LineColor);
    }

    [Fact]
    public void RenderedLineElement_LineColor_Settable()
    {
        var el = new RenderedLineElement { LineColor = "#FF0000" };
        Assert.Equal("#FF0000", el.LineColor);
    }

    [Fact]
    public void RenderedLineElement_FullSetup()
    {
        var el = new RenderedLineElement
        {
            X = 10, Y = 100, Width = 200, Height = 0,
            Direction = LineDirection.Horizontal,
            LineWidth = 1.5,
            LineColor = "#0000FF"
        };
        Assert.Equal(10, el.X);
        Assert.Equal(100, el.Y);
        Assert.Equal(200, el.Width);
        Assert.Equal(0, el.Height);
        Assert.Equal(LineDirection.Horizontal, el.Direction);
        Assert.Equal(1.5, el.LineWidth);
        Assert.Equal("#0000FF", el.LineColor);
    }
}

/// <summary>
/// RenderedShapeElement 完整属性测试
/// </summary>
public class RenderedShapeElementCompleteTests
{
    [Fact]
    public void RenderedShapeElement_Shape_DefaultRectangle()
    {
        var el = new RenderedShapeElement();
        Assert.Equal(ShapeType.Rectangle, el.Shape);
    }

    [Fact]
    public void RenderedShapeElement_Shape_Ellipse()
    {
        var el = new RenderedShapeElement { Shape = ShapeType.Ellipse };
        Assert.Equal(ShapeType.Ellipse, el.Shape);
    }

    [Fact]
    public void RenderedShapeElement_Shape_RoundedRect()
    {
        var el = new RenderedShapeElement { Shape = ShapeType.RoundedRect };
        Assert.Equal(ShapeType.RoundedRect, el.Shape);
    }

    [Fact]
    public void RenderedShapeElement_Shape_Triangle()
    {
        var el = new RenderedShapeElement { Shape = ShapeType.Triangle };
        Assert.Equal(ShapeType.Triangle, el.Shape);
    }

    [Fact]
    public void RenderedShapeElement_BorderRadius_Default0()
    {
        var el = new RenderedShapeElement();
        Assert.Equal(0, el.BorderRadius);
    }

    [Fact]
    public void RenderedShapeElement_BorderRadius_Settable()
    {
        var el = new RenderedShapeElement { BorderRadius = 5 };
        Assert.Equal(5, el.BorderRadius);
    }

    [Fact]
    public void RenderedShapeElement_FillColor_DefaultFFFFFF()
    {
        var el = new RenderedShapeElement();
        Assert.Equal("#FFFFFF", el.FillColor);
    }

    [Fact]
    public void RenderedShapeElement_FillColor_Settable()
    {
        var el = new RenderedShapeElement { FillColor = "#00FF00" };
        Assert.Equal("#00FF00", el.FillColor);
    }

    [Fact]
    public void RenderedShapeElement_FullSetup()
    {
        var el = new RenderedShapeElement
        {
            X = 10, Y = 10, Width = 50, Height = 50,
            Shape = ShapeType.RoundedRect,
            BorderRadius = 8,
            FillColor = "#CCCCCC"
        };
        Assert.Equal(10, el.X);
        Assert.Equal(10, el.Y);
        Assert.Equal(50, el.Width);
        Assert.Equal(50, el.Height);
        Assert.Equal(ShapeType.RoundedRect, el.Shape);
        Assert.Equal(8, el.BorderRadius);
        Assert.Equal("#CCCCCC", el.FillColor);
    }
}

/// <summary>
/// RenderedBarcodeElement 完整属性测试
/// </summary>
public class RenderedBarcodeElementCompleteTests
{
    [Fact]
    public void RenderedBarcodeElement_Value_DefaultEmpty()
    {
        var el = new RenderedBarcodeElement();
        Assert.Equal("", el.Value);
    }

    [Fact]
    public void RenderedBarcodeElement_Value_Settable()
    {
        var el = new RenderedBarcodeElement { Value = "ABC123" };
        Assert.Equal("ABC123", el.Value);
    }

    [Fact]
    public void RenderedBarcodeElement_Format_DefaultCode128()
    {
        var el = new RenderedBarcodeElement();
        Assert.Equal(BarcodeFormat.Code128, el.Format);
    }

    [Fact]
    public void RenderedBarcodeElement_Format_Code128()
    {
        var el = new RenderedBarcodeElement { Format = BarcodeFormat.Code128 };
        Assert.Equal(BarcodeFormat.Code128, el.Format);
    }

    [Fact]
    public void RenderedBarcodeElement_ForeColor_Default000000()
    {
        var el = new RenderedBarcodeElement();
        Assert.Equal("#000000", el.ForeColor);
    }

    [Fact]
    public void RenderedBarcodeElement_ForeColor_Settable()
    {
        var el = new RenderedBarcodeElement { ForeColor = "#FF0000" };
        Assert.Equal("#FF0000", el.ForeColor);
    }

    [Fact]
    public void RenderedBarcodeElement_BackColor_DefaultFFFFFF()
    {
        var el = new RenderedBarcodeElement();
        Assert.Equal("#FFFFFF", el.BackColor);
    }

    [Fact]
    public void RenderedBarcodeElement_BackColor_Settable()
    {
        var el = new RenderedBarcodeElement { BackColor = "#0000FF" };
        Assert.Equal("#0000FF", el.BackColor);
    }

    [Fact]
    public void RenderedBarcodeElement_ShowText_DefaultTrue()
    {
        var el = new RenderedBarcodeElement();
        Assert.True(el.ShowText);
    }

    [Fact]
    public void RenderedBarcodeElement_ShowText_Settable()
    {
        var el = new RenderedBarcodeElement { ShowText = false };
        Assert.False(el.ShowText);
    }

    [Fact]
    public void RenderedBarcodeElement_FullSetup()
    {
        var el = new RenderedBarcodeElement
        {
            X = 10, Y = 10, Width = 100, Height = 100,
            Value = "https://example.com",
            Format = BarcodeFormat.QRCode,
            ForeColor = "#000000",
            BackColor = "#FFFFFF",
            ShowText = false
        };
        Assert.Equal(10, el.X);
        Assert.Equal(10, el.Y);
        Assert.Equal(100, el.Width);
        Assert.Equal(100, el.Height);
        Assert.Equal("https://example.com", el.Value);
        Assert.Equal(BarcodeFormat.QRCode, el.Format);
        Assert.Equal("#000000", el.ForeColor);
        Assert.Equal("#FFFFFF", el.BackColor);
        Assert.False(el.ShowText);
    }
}

/// <summary>
/// RenderedTableElement 完整属性测试
/// </summary>
public class RenderedTableElementCompleteTests
{
    [Fact]
    public void RenderedTableElement_RowCount_Default0()
    {
        var el = new RenderedTableElement();
        Assert.Equal(0, el.RowCount);
    }

    [Fact]
    public void RenderedTableElement_RowCount_Settable()
    {
        var el = new RenderedTableElement { RowCount = 5 };
        Assert.Equal(5, el.RowCount);
    }

    [Fact]
    public void RenderedTableElement_ColCount_Default0()
    {
        var el = new RenderedTableElement();
        Assert.Equal(0, el.ColCount);
    }

    [Fact]
    public void RenderedTableElement_ColCount_Settable()
    {
        var el = new RenderedTableElement { ColCount = 4 };
        Assert.Equal(4, el.ColCount);
    }

    [Fact]
    public void RenderedTableElement_ColumnWidths_DefaultEmpty()
    {
        var el = new RenderedTableElement();
        Assert.NotNull(el.ColumnWidths);
        Assert.Empty(el.ColumnWidths);
    }

    [Fact]
    public void RenderedTableElement_ColumnWidths_Addable()
    {
        var el = new RenderedTableElement();
        el.ColumnWidths.Add(30);
        el.ColumnWidths.Add(50);
        el.ColumnWidths.Add(40);
        Assert.Equal(3, el.ColumnWidths.Count);
    }

    [Fact]
    public void RenderedTableElement_RowHeights_DefaultEmpty()
    {
        var el = new RenderedTableElement();
        Assert.NotNull(el.RowHeights);
        Assert.Empty(el.RowHeights);
    }

    [Fact]
    public void RenderedTableElement_RowHeights_Addable()
    {
        var el = new RenderedTableElement();
        el.RowHeights.Add(10);
        el.RowHeights.Add(15);
        Assert.Equal(2, el.RowHeights.Count);
    }

    [Fact]
    public void RenderedTableElement_Cells_DefaultEmpty()
    {
        var el = new RenderedTableElement();
        Assert.NotNull(el.Cells);
        Assert.Empty(el.Cells);
    }

    [Fact]
    public void RenderedTableElement_Cells_Addable()
    {
        var el = new RenderedTableElement();
        el.Cells.Add(new RenderedTableCell { Row = 0, Col = 0, Text = "A1" });
        Assert.Single(el.Cells);
    }

    [Fact]
    public void RenderedTableElement_BorderWidth_Default0()
    {
        var el = new RenderedTableElement();
        Assert.Equal(0, el.BorderWidth);
    }

    [Fact]
    public void RenderedTableElement_BorderWidth_Settable()
    {
        var el = new RenderedTableElement { BorderWidth = 0.5 };
        Assert.Equal(0.5, el.BorderWidth);
    }

    [Fact]
    public void RenderedTableElement_BorderColor_Default000000()
    {
        var el = new RenderedTableElement();
        Assert.Equal("#000000", el.BorderColor);
    }

    [Fact]
    public void RenderedTableElement_BorderColor_Settable()
    {
        var el = new RenderedTableElement { BorderColor = "#FF0000" };
        Assert.Equal("#FF0000", el.BorderColor);
    }

    [Fact]
    public void RenderedTableElement_FullSetup()
    {
        var el = new RenderedTableElement
        {
            X = 10, Y = 50, Width = 180, Height = 100,
            RowCount = 3,
            ColCount = 4,
            BorderWidth = 0.3,
            BorderColor = "#CCCCCC"
        };
        el.ColumnWidths.AddRange(new[] { 40.0, 50, 40, 50 });
        el.RowHeights.AddRange(new[] { 10.0, 15, 15 });
        el.Cells.Add(new RenderedTableCell { Row = 0, Col = 0, Text = "Header" });

        Assert.Equal(10, el.X);
        Assert.Equal(50, el.Y);
        Assert.Equal(180, el.Width);
        Assert.Equal(100, el.Height);
        Assert.Equal(3, el.RowCount);
        Assert.Equal(4, el.ColCount);
        Assert.Equal(4, el.ColumnWidths.Count);
        Assert.Equal(3, el.RowHeights.Count);
        Assert.Single(el.Cells);
        Assert.Equal(0.3, el.BorderWidth);
        Assert.Equal("#CCCCCC", el.BorderColor);
    }
}

/// <summary>
/// RenderedTableCell 完整属性测试
/// </summary>
public class RenderedTableCellCompleteTests
{
    [Fact]
    public void RenderedTableCell_Row_Default0()
    {
        var c = new RenderedTableCell();
        Assert.Equal(0, c.Row);
    }

    [Fact]
    public void RenderedTableCell_Col_Default0()
    {
        var c = new RenderedTableCell();
        Assert.Equal(0, c.Col);
    }

    [Fact]
    public void RenderedTableCell_RowSpan_Default1()
    {
        var c = new RenderedTableCell();
        Assert.Equal(1, c.RowSpan);
    }

    [Fact]
    public void RenderedTableCell_ColSpan_Default1()
    {
        var c = new RenderedTableCell();
        Assert.Equal(1, c.ColSpan);
    }

    [Fact]
    public void RenderedTableCell_Text_DefaultEmpty()
    {
        var c = new RenderedTableCell();
        Assert.Equal("", c.Text);
    }

    [Fact]
    public void RenderedTableCell_Text_Settable()
    {
        var c = new RenderedTableCell { Text = "Cell content" };
        Assert.Equal("Cell content", c.Text);
    }

    [Fact]
    public void RenderedTableCell_Font_DefaultNotNull()
    {
        var c = new RenderedTableCell();
        Assert.NotNull(c.Font);
    }

    [Fact]
    public void RenderedTableCell_Font_Settable()
    {
        var c = new RenderedTableCell { Font = new FontDef { Size = 11, Bold = true } };
        Assert.Equal(11, c.Font.Size);
        Assert.True(c.Font.Bold);
    }

    [Fact]
    public void RenderedTableCell_Alignment_DefaultCenter()
    {
        var c = new RenderedTableCell();
        Assert.Equal(TextAlignment.Center, c.Alignment);
    }

    [Fact]
    public void RenderedTableCell_Alignment_Left()
    {
        var c = new RenderedTableCell { Alignment = TextAlignment.Left };
        Assert.Equal(TextAlignment.Left, c.Alignment);
    }

    [Fact]
    public void RenderedTableCell_Alignment_Right()
    {
        var c = new RenderedTableCell { Alignment = TextAlignment.Right };
        Assert.Equal(TextAlignment.Right, c.Alignment);
    }

    [Fact]
    public void RenderedTableCell_BackgroundColor_DefaultNull()
    {
        var c = new RenderedTableCell();
        Assert.Null(c.BackgroundColor);
    }

    [Fact]
    public void RenderedTableCell_BackgroundColor_Settable()
    {
        var c = new RenderedTableCell { BackgroundColor = "#F0F0F0" };
        Assert.Equal("#F0F0F0", c.BackgroundColor);
    }

    [Fact]
    public void RenderedTableCell_MergedCell()
    {
        var c = new RenderedTableCell
        {
            Row = 0, Col = 0,
            RowSpan = 2, ColSpan = 3,
            Text = "Merged Header"
        };
        Assert.Equal(0, c.Row);
        Assert.Equal(0, c.Col);
        Assert.Equal(2, c.RowSpan);
        Assert.Equal(3, c.ColSpan);
        Assert.Equal("Merged Header", c.Text);
    }

    [Fact]
    public void RenderedTableCell_FullSetup()
    {
        var c = new RenderedTableCell
        {
            Row = 1, Col = 2,
            RowSpan = 1, ColSpan = 1,
            Text = "{{amount}}",
            Font = new FontDef { Size = 12, Color = "#333333" },
            Alignment = TextAlignment.Right,
            BackgroundColor = "#FFFFCC"
        };
        Assert.Equal(1, c.Row);
        Assert.Equal(2, c.Col);
        Assert.Equal(1, c.RowSpan);
        Assert.Equal(1, c.ColSpan);
        Assert.Equal("{{amount}}", c.Text);
        Assert.Equal(12, c.Font.Size);
        Assert.Equal(TextAlignment.Right, c.Alignment);
        Assert.Equal("#FFFFCC", c.BackgroundColor);
    }
}

/// <summary>
/// RenderedCrossTabElement 完整属性测试
/// </summary>
public class RenderedCrossTabElementCompleteTests
{
    [Fact]
    public void RenderedCrossTabElement_RowCount_Default0()
    {
        var el = new RenderedCrossTabElement();
        Assert.Equal(0, el.RowCount);
    }

    [Fact]
    public void RenderedCrossTabElement_RowCount_Settable()
    {
        var el = new RenderedCrossTabElement { RowCount = 6 };
        Assert.Equal(6, el.RowCount);
    }

    [Fact]
    public void RenderedCrossTabElement_ColCount_Default0()
    {
        var el = new RenderedCrossTabElement();
        Assert.Equal(0, el.ColCount);
    }

    [Fact]
    public void RenderedCrossTabElement_ColCount_Settable()
    {
        var el = new RenderedCrossTabElement { ColCount = 5 };
        Assert.Equal(5, el.ColCount);
    }

    [Fact]
    public void RenderedCrossTabElement_ColumnWidths_DefaultEmpty()
    {
        var el = new RenderedCrossTabElement();
        Assert.NotNull(el.ColumnWidths);
        Assert.Empty(el.ColumnWidths);
    }

    [Fact]
    public void RenderedCrossTabElement_ColumnWidths_Addable()
    {
        var el = new RenderedCrossTabElement();
        el.ColumnWidths.Add(60);
        el.ColumnWidths.Add(40);
        el.ColumnWidths.Add(40);
        Assert.Equal(3, el.ColumnWidths.Count);
    }

    [Fact]
    public void RenderedCrossTabElement_RowHeights_DefaultEmpty()
    {
        var el = new RenderedCrossTabElement();
        Assert.NotNull(el.RowHeights);
        Assert.Empty(el.RowHeights);
    }

    [Fact]
    public void RenderedCrossTabElement_RowHeights_Addable()
    {
        var el = new RenderedCrossTabElement();
        el.RowHeights.Add(12);
        el.RowHeights.Add(15);
        Assert.Equal(2, el.RowHeights.Count);
    }

    [Fact]
    public void RenderedCrossTabElement_Cells_DefaultEmpty()
    {
        var el = new RenderedCrossTabElement();
        Assert.NotNull(el.Cells);
        Assert.Empty(el.Cells);
    }

    [Fact]
    public void RenderedCrossTabElement_Cells_Addable()
    {
        var el = new RenderedCrossTabElement();
        el.Cells.Add(new RenderedTableCell { Row = 0, Col = 0, Text = "Total" });
        Assert.Single(el.Cells);
    }

    [Fact]
    public void RenderedCrossTabElement_BorderWidth_Default0()
    {
        var el = new RenderedCrossTabElement();
        Assert.Equal(0, el.BorderWidth);
    }

    [Fact]
    public void RenderedCrossTabElement_BorderWidth_Settable()
    {
        var el = new RenderedCrossTabElement { BorderWidth = 0.3 };
        Assert.Equal(0.3, el.BorderWidth);
    }

    [Fact]
    public void RenderedCrossTabElement_BorderColor_Default000000()
    {
        var el = new RenderedCrossTabElement();
        Assert.Equal("#000000", el.BorderColor);
    }

    [Fact]
    public void RenderedCrossTabElement_BorderColor_Settable()
    {
        var el = new RenderedCrossTabElement { BorderColor = "#999999" };
        Assert.Equal("#999999", el.BorderColor);
    }

    [Fact]
    public void RenderedCrossTabElement_FullSetup()
    {
        var el = new RenderedCrossTabElement
        {
            X = 10, Y = 50, Width = 180, Height = 120,
            RowCount = 4,
            ColCount = 3,
            BorderWidth = 0.3,
            BorderColor = "#666666"
        };
        el.ColumnWidths.AddRange(new[] { 60.0, 40, 40 });
        el.RowHeights.AddRange(new[] { 12.0, 15, 15, 15 });
        el.Cells.Add(new RenderedTableCell { Row = 0, Col = 0, Text = "Region" });
        el.Cells.Add(new RenderedTableCell { Row = 0, Col = 1, Text = "Q1" });
        el.Cells.Add(new RenderedTableCell { Row = 1, Col = 0, Text = "North" });
        el.Cells.Add(new RenderedTableCell { Row = 1, Col = 1, Text = "1000" });

        Assert.Equal(10, el.X);
        Assert.Equal(50, el.Y);
        Assert.Equal(180, el.Width);
        Assert.Equal(120, el.Height);
        Assert.Equal(4, el.RowCount);
        Assert.Equal(3, el.ColCount);
        Assert.Equal(3, el.ColumnWidths.Count);
        Assert.Equal(4, el.RowHeights.Count);
        Assert.Equal(4, el.Cells.Count);
        Assert.Equal(0.3, el.BorderWidth);
        Assert.Equal("#666666", el.BorderColor);
    }
}
