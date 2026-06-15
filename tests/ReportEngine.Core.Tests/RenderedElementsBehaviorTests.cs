using ReportEngine.Core;
using ReportEngine.Core.Rendering;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// RenderedElements 行为测试：
///   - RenderedTextElement
///   - RenderedImageElement
///   - RenderedLineElement
///   - RenderedShapeElement
///   - RenderedBarcodeElement
///   - RenderedTableElement
///   - RenderedTableCell
///   - RenderedCrossTabElement
/// </summary>
public class RenderedElementsBehaviorTests
{
    // ============== RenderedTextElement ==============

    [Fact]
    public void RenderedTextElement_DefaultValues_AreCorrect()
    {
        var el = new RenderedTextElement();

        Assert.Equal("", el.Text);
        Assert.NotNull(el.Font);
        Assert.Equal(TextAlignment.Left, el.Alignment);
        Assert.Null(el.Hyperlink);
    }

    [Fact]
    public void RenderedTextElement_SetText_Works()
    {
        var el = new RenderedTextElement { Text = "Hello World" };
        Assert.Equal("Hello World", el.Text);
    }

    [Fact]
    public void RenderedTextElement_SetFont_Works()
    {
        var el = new RenderedTextElement
        {
            Font = new FontDef { Family = "Arial", Size = 14, Bold = true }
        };
        Assert.Equal("Arial", el.Font.Family);
        Assert.True(el.Font.Bold);
    }

    [Fact]
    public void RenderedTextElement_SetAlignment_Works()
    {
        var el = new RenderedTextElement { Alignment = TextAlignment.Center };
        Assert.Equal(TextAlignment.Center, el.Alignment);
    }

    [Fact]
    public void RenderedTextElement_SetHyperlink_Works()
    {
        var el = new RenderedTextElement { Hyperlink = "https://example.com" };
        Assert.Equal("https://example.com", el.Hyperlink);
    }

    // ============== RenderedImageElement ==============

    [Fact]
    public void RenderedImageElement_DefaultValues_AreCorrect()
    {
        var el = new RenderedImageElement();
        Assert.Equal("", el.Source);
    }

    [Fact]
    public void RenderedImageElement_SetSource_Works()
    {
        var el = new RenderedImageElement { Source = "logo.png" };
        Assert.Equal("logo.png", el.Source);
    }

    [Fact]
    public void RenderedImageElement_SetBase64_Works()
    {
        var el = new RenderedImageElement { Source = "data:image/png;base64,..." };
        Assert.StartsWith("data:image/", el.Source);
    }

    // ============== RenderedLineElement ==============

    [Fact]
    public void RenderedLineElement_DefaultValues_AreCorrect()
    {
        var el = new RenderedLineElement();

        Assert.Equal(LineDirection.Horizontal, el.Direction);
        Assert.Equal(0, el.LineWidth);
        Assert.Equal("#000000", el.LineColor);
    }

    [Fact]
    public void RenderedLineElement_SetDirection_Works()
    {
        var el = new RenderedLineElement { Direction = LineDirection.Vertical };
        Assert.Equal(LineDirection.Vertical, el.Direction);
    }

    [Fact]
    public void RenderedLineElement_SetLineWidth_Works()
    {
        var el = new RenderedLineElement { LineWidth = 2.5 };
        Assert.Equal(2.5, el.LineWidth);
    }

    [Fact]
    public void RenderedLineElement_SetLineColor_Works()
    {
        var el = new RenderedLineElement { LineColor = "#FF0000" };
        Assert.Equal("#FF0000", el.LineColor);
    }

    // ============== RenderedShapeElement ==============

    [Fact]
    public void RenderedShapeElement_DefaultValues_AreCorrect()
    {
        var el = new RenderedShapeElement();

        Assert.Equal(ShapeType.Rectangle, el.Shape);
        Assert.Equal(0, el.BorderRadius);
        Assert.Equal("#FFFFFF", el.FillColor);
    }

    [Fact]
    public void RenderedShapeElement_SetShape_Works()
    {
        var el = new RenderedShapeElement { Shape = ShapeType.Ellipse };
        Assert.Equal(ShapeType.Ellipse, el.Shape);
    }

    [Fact]
    public void RenderedShapeElement_SetBorderRadius_Works()
    {
        var el = new RenderedShapeElement { BorderRadius = 5 };
        Assert.Equal(5, el.BorderRadius);
    }

    [Fact]
    public void RenderedShapeElement_SetFillColor_Works()
    {
        var el = new RenderedShapeElement { FillColor = "#EEEEEE" };
        Assert.Equal("#EEEEEE", el.FillColor);
    }

    // ============== RenderedBarcodeElement ==============

    [Fact]
    public void RenderedBarcodeElement_DefaultValues_AreCorrect()
    {
        var el = new RenderedBarcodeElement();

        Assert.Equal("", el.Value);
        Assert.Equal(BarcodeFormat.Code128, el.Format);
        Assert.Equal("#000000", el.ForeColor);
        Assert.Equal("#FFFFFF", el.BackColor);
        Assert.True(el.ShowText);
    }

    [Fact]
    public void RenderedBarcodeElement_SetValue_Works()
    {
        var el = new RenderedBarcodeElement { Value = "123456" };
        Assert.Equal("123456", el.Value);
    }

    [Fact]
    public void RenderedBarcodeElement_SetFormat_Works()
    {
        var el = new RenderedBarcodeElement { Format = BarcodeFormat.Code128 };
        Assert.Equal(BarcodeFormat.Code128, el.Format);
    }

    [Fact]
    public void RenderedBarcodeElement_SetForeColor_Works()
    {
        var el = new RenderedBarcodeElement { ForeColor = "#000000" };
        Assert.Equal("#000000", el.ForeColor);
    }

    [Fact]
    public void RenderedBarcodeElement_SetBackColor_Works()
    {
        var el = new RenderedBarcodeElement { BackColor = "#FFFFFF" };
        Assert.Equal("#FFFFFF", el.BackColor);
    }

    [Fact]
    public void RenderedBarcodeElement_SetShowText_Works()
    {
        var el = new RenderedBarcodeElement { ShowText = false };
        Assert.False(el.ShowText);
    }

    // ============== RenderedTableElement ==============

    [Fact]
    public void RenderedTableElement_DefaultValues_AreCorrect()
    {
        var el = new RenderedTableElement();

        Assert.Equal(0, el.RowCount);
        Assert.Equal(0, el.ColCount);
        Assert.NotNull(el.ColumnWidths);
        Assert.Empty(el.ColumnWidths);
        Assert.NotNull(el.RowHeights);
        Assert.Empty(el.RowHeights);
        Assert.NotNull(el.Cells);
        Assert.Empty(el.Cells);
        Assert.Equal(0, el.BorderWidth);
        Assert.Equal("#000000", el.BorderColor);
    }

    [Fact]
    public void RenderedTableElement_SetRowCount_Works()
    {
        var el = new RenderedTableElement { RowCount = 5 };
        Assert.Equal(5, el.RowCount);
    }

    [Fact]
    public void RenderedTableElement_SetColCount_Works()
    {
        var el = new RenderedTableElement { ColCount = 4 };
        Assert.Equal(4, el.ColCount);
    }

    [Fact]
    public void RenderedTableElement_AddColumnWidths_Works()
    {
        var el = new RenderedTableElement();
        el.ColumnWidths.Add(30);
        el.ColumnWidths.Add(50);
        el.ColumnWidths.Add(30);
        Assert.Equal(3, el.ColumnWidths.Count);
    }

    [Fact]
    public void RenderedTableElement_AddCells_Works()
    {
        var el = new RenderedTableElement();
        el.Cells.Add(new RenderedTableCell { Row = 0, Col = 0, Text = "A1" });
        el.Cells.Add(new RenderedTableCell { Row = 0, Col = 1, Text = "B1" });
        Assert.Equal(2, el.Cells.Count);
    }

    [Fact]
    public void RenderedTableElement_SetBorderWidth_Works()
    {
        var el = new RenderedTableElement { BorderWidth = 0.5 };
        Assert.Equal(0.5, el.BorderWidth);
    }

    [Fact]
    public void RenderedTableElement_SetBorderColor_Works()
    {
        var el = new RenderedTableElement { BorderColor = "#CCCCCC" };
        Assert.Equal("#CCCCCC", el.BorderColor);
    }

    // ============== RenderedTableCell ==============

    [Fact]
    public void RenderedTableCell_DefaultValues_AreCorrect()
    {
        var cell = new RenderedTableCell();

        Assert.Equal(0, cell.Row);
        Assert.Equal(0, cell.Col);
        Assert.Equal(1, cell.RowSpan);
        Assert.Equal(1, cell.ColSpan);
        Assert.Equal("", cell.Text);
        Assert.NotNull(cell.Font);
        Assert.Equal(TextAlignment.Center, cell.Alignment);
        Assert.Null(cell.BackgroundColor);
    }

    [Fact]
    public void RenderedTableCell_SetRowCol_Works()
    {
        var cell = new RenderedTableCell { Row = 2, Col = 3 };
        Assert.Equal(2, cell.Row);
        Assert.Equal(3, cell.Col);
    }

    [Fact]
    public void RenderedTableCell_SetRowSpan_Works()
    {
        var cell = new RenderedTableCell { RowSpan = 2 };
        Assert.Equal(2, cell.RowSpan);
    }

    [Fact]
    public void RenderedTableCell_SetColSpan_Works()
    {
        var cell = new RenderedTableCell { ColSpan = 3 };
        Assert.Equal(3, cell.ColSpan);
    }

    [Fact]
    public void RenderedTableCell_SetText_Works()
    {
        var cell = new RenderedTableCell { Text = "Cell Content" };
        Assert.Equal("Cell Content", cell.Text);
    }

    [Fact]
    public void RenderedTableCell_SetFont_Works()
    {
        var cell = new RenderedTableCell
        {
            Font = new FontDef { Family = "Arial", Size = 12 }
        };
        Assert.Equal("Arial", cell.Font.Family);
    }

    [Fact]
    public void RenderedTableCell_SetAlignment_Works()
    {
        var cell = new RenderedTableCell { Alignment = TextAlignment.Right };
        Assert.Equal(TextAlignment.Right, cell.Alignment);
    }

    [Fact]
    public void RenderedTableCell_SetBackgroundColor_Works()
    {
        var cell = new RenderedTableCell { BackgroundColor = "#FFFFCC" };
        Assert.Equal("#FFFFCC", cell.BackgroundColor);
    }

    [Fact]
    public void RenderedTableCell_FontDefault_IsSimSun()
    {
        var cell = new RenderedTableCell();
        Assert.Equal("SimSun", cell.Font.Family);
    }

    // ============== RenderedCrossTabElement ==============

    [Fact]
    public void RenderedCrossTabElement_DefaultValues_AreCorrect()
    {
        var el = new RenderedCrossTabElement();

        Assert.Equal(0, el.RowCount);
        Assert.Equal(0, el.ColCount);
        Assert.NotNull(el.ColumnWidths);
        Assert.Empty(el.ColumnWidths);
        Assert.NotNull(el.RowHeights);
        Assert.Empty(el.RowHeights);
        Assert.NotNull(el.Cells);
        Assert.Empty(el.Cells);
        Assert.Equal(0, el.BorderWidth);
        Assert.Equal("#000000", el.BorderColor);
    }

    [Fact]
    public void RenderedCrossTabElement_SetRowCount_Works()
    {
        var el = new RenderedCrossTabElement { RowCount = 10 };
        Assert.Equal(10, el.RowCount);
    }

    [Fact]
    public void RenderedCrossTabElement_SetColCount_Works()
    {
        var el = new RenderedCrossTabElement { ColCount = 8 };
        Assert.Equal(8, el.ColCount);
    }

    [Fact]
    public void RenderedCrossTabElement_AddCells_Works()
    {
        var el = new RenderedCrossTabElement();
        el.Cells.Add(new RenderedTableCell { Row = 0, Col = 0, Text = "Header" });
        el.Cells.Add(new RenderedTableCell { Row = 1, Col = 0, Text = "Data" });
        Assert.Equal(2, el.Cells.Count);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void RenderedPage_WithMultipleElements_Works()
    {
        var page = new RenderedPage { PageNumber = 1, TotalPages = 1 };
        
        page.Elements.Add(new RenderedTextElement
        {
            Text = "Title",
            X = 10,
            Y = 10,
            Width = 100,
            Height = 20,
            Font = new FontDef { Size = 18, Bold = true }
        });
        
        page.Elements.Add(new RenderedImageElement
        {
            Source = "logo.png",
            X = 150,
            Y = 10,
            Width = 40,
            Height = 40
        });
        
        page.Elements.Add(new RenderedLineElement
        {
            Direction = LineDirection.Horizontal,
            X = 10,
            Y = 60,
            Width = 180,
            LineWidth = 0.5
        });

        Assert.Equal(3, page.Elements.Count);
        Assert.IsType<RenderedTextElement>(page.Elements[0]);
        Assert.IsType<RenderedImageElement>(page.Elements[1]);
        Assert.IsType<RenderedLineElement>(page.Elements[2]);
    }

    [Fact]
    public void RenderedReport_WithPages_Works()
    {
        var report = new RenderedReport
        {
            PageWidth = 210,
            PageHeight = 297
        };
        
        report.Pages.Add(new RenderedPage { PageNumber = 1, TotalPages = 2 });
        report.Pages.Add(new RenderedPage { PageNumber = 2, TotalPages = 2 });
        
        report.Pages[0].Elements.Add(new RenderedTextElement { Text = "Page 1" });
        report.Pages[1].Elements.Add(new RenderedTextElement { Text = "Page 2" });

        Assert.Equal(2, report.Pages.Count);
        Assert.Equal("Page 1", ((RenderedTextElement)report.Pages[0].Elements[0]).Text);
        Assert.Equal("Page 2", ((RenderedTextElement)report.Pages[1].Elements[0]).Text);
    }
}
