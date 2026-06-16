using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Rendering;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// RenderedElement 基类属性测试 3
/// </summary>
public class RenderedElementBase3Tests
{
    [Fact]
    public void RenderedTextElement_Id_DefaultEmpty()
    {
        var el = new RenderedTextElement();
        Assert.Equal("", el.Id);
    }

    [Fact]
    public void RenderedTextElement_Id_SetValue()
    {
        var el = new RenderedTextElement { Id = "txt_001" };
        Assert.Equal("txt_001", el.Id);
    }

    [Fact]
    public void RenderedTextElement_X_DefaultZero()
    {
        var el = new RenderedTextElement();
        Assert.Equal(0, el.X);
    }

    [Fact]
    public void RenderedTextElement_Y_DefaultZero()
    {
        var el = new RenderedTextElement();
        Assert.Equal(0, el.Y);
    }

    [Fact]
    public void RenderedTextElement_Width_DefaultZero()
    {
        var el = new RenderedTextElement();
        Assert.Equal(0, el.Width);
    }

    [Fact]
    public void RenderedTextElement_Height_DefaultZero()
    {
        var el = new RenderedTextElement();
        Assert.Equal(0, el.Height);
    }

    [Fact]
    public void RenderedTextElement_BackgroundColor_DefaultNull()
    {
        var el = new RenderedTextElement();
        Assert.Null(el.BackgroundColor);
    }

    [Fact]
    public void RenderedTextElement_BackgroundColor_SetValue()
    {
        var el = new RenderedTextElement { BackgroundColor = "#FF0000" };
        Assert.Equal("#FF0000", el.BackgroundColor);
    }

    [Fact]
    public void RenderedTextElement_Border_DefaultNull()
    {
        var el = new RenderedTextElement();
        Assert.Null(el.Border);
    }

    [Fact]
    public void RenderedTextElement_Border_SetValue()
    {
        var el = new RenderedTextElement { Border = new BorderDef { Width = 2, Color = "#000000" } };
        Assert.NotNull(el.Border);
        Assert.Equal(2, el.Border.Width);
    }

    [Fact]
    public void RenderedTextElement_Position_Set()
    {
        var el = new RenderedTextElement { X = 10.5, Y = 20.3, Width = 100, Height = 30 };
        Assert.Equal(10.5, el.X);
        Assert.Equal(20.3, el.Y);
        Assert.Equal(100, el.Width);
        Assert.Equal(30, el.Height);
    }
}

/// <summary>
/// RenderedTextElement 更多属性测试 3
/// </summary>
public class RenderedTextElementExtra3Tests
{
    [Fact]
    public void RenderedTextElement_Text_DefaultEmpty()
    {
        var el = new RenderedTextElement();
        Assert.Equal("", el.Text);
    }

    [Fact]
    public void RenderedTextElement_Text_SetValue()
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
    public void RenderedTextElement_Font_DefaultSize()
    {
        var el = new RenderedTextElement();
        Assert.Equal(10, el.Font.Size);
    }

    [Fact]
    public void RenderedTextElement_Font_DefaultFamily()
    {
        var el = new RenderedTextElement();
        Assert.Equal("SimSun", el.Font.Family);
    }

    [Fact]
    public void RenderedTextElement_Alignment_DefaultLeft()
    {
        var el = new RenderedTextElement();
        Assert.Equal(TextAlignment.Left, el.Alignment);
    }

    [Fact]
    public void RenderedTextElement_Alignment_SetCenter()
    {
        var el = new RenderedTextElement { Alignment = TextAlignment.Center };
        Assert.Equal(TextAlignment.Center, el.Alignment);
    }

    [Fact]
    public void RenderedTextElement_Hyperlink_DefaultNull()
    {
        var el = new RenderedTextElement();
        Assert.Null(el.Hyperlink);
    }

    [Fact]
    public void RenderedTextElement_Hyperlink_SetValue()
    {
        var el = new RenderedTextElement { Hyperlink = "https://example.com" };
        Assert.Equal("https://example.com", el.Hyperlink);
    }
}

/// <summary>
/// RenderedImageElement 测试 2
/// </summary>
public class RenderedImageElement2Tests
{
    [Fact]
    public void RenderedImageElement_Source_DefaultEmpty()
    {
        var el = new RenderedImageElement();
        Assert.Equal("", el.Source);
    }

    [Fact]
    public void RenderedImageElement_Source_SetValue()
    {
        var el = new RenderedImageElement { Source = "data:image/png;base64,..." };
        Assert.Equal("data:image/png;base64,...", el.Source);
    }

    [Fact]
    public void RenderedImageElement_InheritsBaseProperties()
    {
        var el = new RenderedImageElement { X = 5, Y = 10, Width = 50, Height = 50, Id = "img_001" };
        Assert.Equal("img_001", el.Id);
        Assert.Equal(5, el.X);
        Assert.Equal(10, el.Y);
        Assert.Equal(50, el.Width);
        Assert.Equal(50, el.Height);
    }
}

/// <summary>
/// RenderedLineElement 测试 2
/// </summary>
public class RenderedLineElement2Tests
{
    [Fact]
    public void RenderedLineElement_Direction_Default()
    {
        var el = new RenderedLineElement();
        Assert.Equal(LineDirection.Horizontal, el.Direction);
    }

    [Fact]
    public void RenderedLineElement_Direction_SetVertical()
    {
        var el = new RenderedLineElement { Direction = LineDirection.Vertical };
        Assert.Equal(LineDirection.Vertical, el.Direction);
    }

    [Fact]
    public void RenderedLineElement_LineWidth_DefaultZero()
    {
        var el = new RenderedLineElement();
        Assert.Equal(0, el.LineWidth);
    }

    [Fact]
    public void RenderedLineElement_LineWidth_SetValue()
    {
        var el = new RenderedLineElement { LineWidth = 1.5 };
        Assert.Equal(1.5, el.LineWidth);
    }

    [Fact]
    public void RenderedLineElement_LineColor_Default000000()
    {
        var el = new RenderedLineElement();
        Assert.Equal("#000000", el.LineColor);
    }

    [Fact]
    public void RenderedLineElement_LineColor_SetValue()
    {
        var el = new RenderedLineElement { LineColor = "#FF0000" };
        Assert.Equal("#FF0000", el.LineColor);
    }
}

/// <summary>
/// RenderedShapeElement 测试 2
/// </summary>
public class RenderedShapeElement2Tests
{
    [Fact]
    public void RenderedShapeElement_Shape_Default()
    {
        var el = new RenderedShapeElement();
        Assert.Equal(ShapeType.Rectangle, el.Shape);
    }

    [Fact]
    public void RenderedShapeElement_Shape_SetEllipse()
    {
        var el = new RenderedShapeElement { Shape = ShapeType.Ellipse };
        Assert.Equal(ShapeType.Ellipse, el.Shape);
    }

    [Fact]
    public void RenderedShapeElement_BorderRadius_DefaultZero()
    {
        var el = new RenderedShapeElement();
        Assert.Equal(0, el.BorderRadius);
    }

    [Fact]
    public void RenderedShapeElement_BorderRadius_SetValue()
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
    public void RenderedShapeElement_FillColor_SetValue()
    {
        var el = new RenderedShapeElement { FillColor = "#00FF00" };
        Assert.Equal("#00FF00", el.FillColor);
    }
}

/// <summary>
/// RenderedBarcodeElement 测试 2
/// </summary>
public class RenderedBarcodeElement2Tests
{
    [Fact]
    public void RenderedBarcodeElement_Value_DefaultEmpty()
    {
        var el = new RenderedBarcodeElement();
        Assert.Equal("", el.Value);
    }

    [Fact]
    public void RenderedBarcodeElement_Value_SetValue()
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
    public void RenderedBarcodeElement_Format_SetQRCode()
    {
        var el = new RenderedBarcodeElement { Format = BarcodeFormat.QRCode };
        Assert.Equal(BarcodeFormat.QRCode, el.Format);
    }

    [Fact]
    public void RenderedBarcodeElement_ForeColor_Default000000()
    {
        var el = new RenderedBarcodeElement();
        Assert.Equal("#000000", el.ForeColor);
    }

    [Fact]
    public void RenderedBarcodeElement_BackColor_DefaultFFFFFF()
    {
        var el = new RenderedBarcodeElement();
        Assert.Equal("#FFFFFF", el.BackColor);
    }

    [Fact]
    public void RenderedBarcodeElement_ShowText_DefaultTrue()
    {
        var el = new RenderedBarcodeElement();
        Assert.True(el.ShowText);
    }

    [Fact]
    public void RenderedBarcodeElement_ShowText_SetFalse()
    {
        var el = new RenderedBarcodeElement { ShowText = false };
        Assert.False(el.ShowText);
    }
}

/// <summary>
/// RenderedTableElement 测试 2
/// </summary>
public class RenderedTableElement2Tests
{
    [Fact]
    public void RenderedTableElement_RowCount_DefaultZero()
    {
        var el = new RenderedTableElement();
        Assert.Equal(0, el.RowCount);
    }

    [Fact]
    public void RenderedTableElement_ColCount_DefaultZero()
    {
        var el = new RenderedTableElement();
        Assert.Equal(0, el.ColCount);
    }

    [Fact]
    public void RenderedTableElement_ColumnWidths_DefaultEmpty()
    {
        var el = new RenderedTableElement();
        Assert.NotNull(el.ColumnWidths);
        Assert.Empty(el.ColumnWidths);
    }

    [Fact]
    public void RenderedTableElement_RowHeights_DefaultEmpty()
    {
        var el = new RenderedTableElement();
        Assert.NotNull(el.RowHeights);
        Assert.Empty(el.RowHeights);
    }

    [Fact]
    public void RenderedTableElement_Cells_DefaultEmpty()
    {
        var el = new RenderedTableElement();
        Assert.NotNull(el.Cells);
        Assert.Empty(el.Cells);
    }

    [Fact]
    public void RenderedTableElement_BorderWidth_DefaultZero()
    {
        var el = new RenderedTableElement();
        Assert.Equal(0, el.BorderWidth);
    }

    [Fact]
    public void RenderedTableElement_BorderColor_Default000000()
    {
        var el = new RenderedTableElement();
        Assert.Equal("#000000", el.BorderColor);
    }

    [Fact]
    public void RenderedTableElement_FullSetup()
    {
        var el = new RenderedTableElement
        {
            RowCount = 3,
            ColCount = 2,
            ColumnWidths = new List<double> { 50, 100 },
            RowHeights = new List<double> { 20, 20, 20 },
            BorderWidth = 0.5,
            BorderColor = "#333333"
        };
        Assert.Equal(3, el.RowCount);
        Assert.Equal(2, el.ColCount);
        Assert.Equal(2, el.ColumnWidths.Count);
        Assert.Equal(3, el.RowHeights.Count);
        Assert.Equal(0.5, el.BorderWidth);
    }
}

/// <summary>
/// RenderedTableCell 测试 2
/// </summary>
public class RenderedTableCell2Tests
{
    [Fact]
    public void RenderedTableCell_Row_DefaultZero()
    {
        var cell = new RenderedTableCell();
        Assert.Equal(0, cell.Row);
    }

    [Fact]
    public void RenderedTableCell_Col_DefaultZero()
    {
        var cell = new RenderedTableCell();
        Assert.Equal(0, cell.Col);
    }

    [Fact]
    public void RenderedTableCell_RowSpan_Default1()
    {
        var cell = new RenderedTableCell();
        Assert.Equal(1, cell.RowSpan);
    }

    [Fact]
    public void RenderedTableCell_ColSpan_Default1()
    {
        var cell = new RenderedTableCell();
        Assert.Equal(1, cell.ColSpan);
    }

    [Fact]
    public void RenderedTableCell_Text_DefaultEmpty()
    {
        var cell = new RenderedTableCell();
        Assert.Equal("", cell.Text);
    }

    [Fact]
    public void RenderedTableCell_Font_DefaultNotNull()
    {
        var cell = new RenderedTableCell();
        Assert.NotNull(cell.Font);
    }

    [Fact]
    public void RenderedTableCell_Alignment_DefaultCenter()
    {
        var cell = new RenderedTableCell();
        Assert.Equal(TextAlignment.Center, cell.Alignment);
    }

    [Fact]
    public void RenderedTableCell_BackgroundColor_DefaultNull()
    {
        var cell = new RenderedTableCell();
        Assert.Null(cell.BackgroundColor);
    }

    [Fact]
    public void RenderedTableCell_MergedCell()
    {
        var cell = new RenderedTableCell { Row = 0, Col = 0, RowSpan = 2, ColSpan = 3, Text = "Merged" };
        Assert.Equal(0, cell.Row);
        Assert.Equal(0, cell.Col);
        Assert.Equal(2, cell.RowSpan);
        Assert.Equal(3, cell.ColSpan);
        Assert.Equal("Merged", cell.Text);
    }
}

/// <summary>
/// RenderedCrossTabElement 测试 2
/// </summary>
public class RenderedCrossTabElement2Tests
{
    [Fact]
    public void RenderedCrossTabElement_RowCount_DefaultZero()
    {
        var el = new RenderedCrossTabElement();
        Assert.Equal(0, el.RowCount);
    }

    [Fact]
    public void RenderedCrossTabElement_ColCount_DefaultZero()
    {
        var el = new RenderedCrossTabElement();
        Assert.Equal(0, el.ColCount);
    }

    [Fact]
    public void RenderedCrossTabElement_ColumnWidths_DefaultEmpty()
    {
        var el = new RenderedCrossTabElement();
        Assert.NotNull(el.ColumnWidths);
        Assert.Empty(el.ColumnWidths);
    }

    [Fact]
    public void RenderedCrossTabElement_RowHeights_DefaultEmpty()
    {
        var el = new RenderedCrossTabElement();
        Assert.NotNull(el.RowHeights);
        Assert.Empty(el.RowHeights);
    }

    [Fact]
    public void RenderedCrossTabElement_Cells_DefaultEmpty()
    {
        var el = new RenderedCrossTabElement();
        Assert.NotNull(el.Cells);
        Assert.Empty(el.Cells);
    }

    [Fact]
    public void RenderedCrossTabElement_BorderWidth_DefaultZero()
    {
        var el = new RenderedCrossTabElement();
        Assert.Equal(0, el.BorderWidth);
    }

    [Fact]
    public void RenderedCrossTabElement_BorderColor_Default000000()
    {
        var el = new RenderedCrossTabElement();
        Assert.Equal("#000000", el.BorderColor);
    }

    [Fact]
    public void RenderedCrossTabElement_FullSetup()
    {
        var el = new RenderedCrossTabElement
        {
            RowCount = 4,
            ColCount = 5,
            ColumnWidths = new List<double> { 30, 40, 40, 40, 40 },
            RowHeights = new List<double> { 25, 25, 25, 25 },
            BorderWidth = 1,
            BorderColor = "#CCCCCC"
        };
        Assert.Equal(4, el.RowCount);
        Assert.Equal(5, el.ColCount);
        Assert.Equal(5, el.ColumnWidths.Count);
        Assert.Equal(4, el.RowHeights.Count);
        Assert.Equal(1, el.BorderWidth);
    }
}
