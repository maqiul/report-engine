using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Rendering;
using Xunit;

namespace ReportEngine.Core.Tests;

// ─────────────────────────────────────────────────────────────────────────────
// ImageElement 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class ImageElementComplete6Tests
{
    [Fact]
    public void ImageElement_Source_DefaultEmpty()
    {
        var e = new ImageElement();
        Assert.Equal("", e.Source);
    }

    [Fact]
    public void ImageElement_Source_SetValue()
    {
        var e = new ImageElement { Source = "logo.png" };
        Assert.Equal("logo.png", e.Source);
    }

    [Fact]
    public void ImageElement_Sizing_DefaultFitProportional()
    {
        var e = new ImageElement();
        Assert.Equal(ImageSizing.FitProportional, e.Sizing);
    }

    [Fact]
    public void ImageElement_Sizing_SetFitProportional()
    {
        var e = new ImageElement { Sizing = ImageSizing.FitProportional };
        Assert.Equal(ImageSizing.FitProportional, e.Sizing);
    }

    [Fact]
    public void ImageElement_Sizing_SetClip()
    {
        var e = new ImageElement { Sizing = ImageSizing.Clip };
        Assert.Equal(ImageSizing.Clip, e.Sizing);
    }

    [Fact]
    public void ImageElement_Sizing_SetActualSize()
    {
        var e = new ImageElement { Sizing = ImageSizing.ActualSize };
        Assert.Equal(ImageSizing.ActualSize, e.Sizing);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// LineElement 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class LineElementComplete6Tests
{
    [Fact]
    public void LineElement_Direction_DefaultHorizontal()
    {
        var e = new LineElement();
        Assert.Equal(LineDirection.Horizontal, e.Direction);
    }

    [Fact]
    public void LineElement_Direction_SetVertical()
    {
        var e = new LineElement { Direction = LineDirection.Vertical };
        Assert.Equal(LineDirection.Vertical, e.Direction);
    }

    [Fact]
    public void LineElement_LineWidth_Default1()
    {
        var e = new LineElement();
        Assert.Equal(1, e.LineWidth);
    }

    [Fact]
    public void LineElement_LineWidth_SetValue()
    {
        var e = new LineElement { LineWidth = 2.5 };
        Assert.Equal(2.5, e.LineWidth);
    }

    [Fact]
    public void LineElement_LineColor_DefaultBlack()
    {
        var e = new LineElement();
        Assert.Equal("#000000", e.LineColor);
    }

    [Fact]
    public void LineElement_LineColor_SetValue()
    {
        var e = new LineElement { LineColor = "#FF0000" };
        Assert.Equal("#FF0000", e.LineColor);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// ShapeElement 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class ShapeElementComplete6Tests
{
    [Fact]
    public void ShapeElement_Shape_DefaultRectangle()
    {
        var e = new ShapeElement();
        Assert.Equal(ShapeType.Rectangle, e.Shape);
    }

    [Fact]
    public void ShapeElement_Shape_SetEllipse()
    {
        var e = new ShapeElement { Shape = ShapeType.Ellipse };
        Assert.Equal(ShapeType.Ellipse, e.Shape);
    }

    [Fact]
    public void ShapeElement_Shape_SetTriangle()
    {
        var e = new ShapeElement { Shape = ShapeType.Triangle };
        Assert.Equal(ShapeType.Triangle, e.Shape);
    }

    [Fact]
    public void ShapeElement_FillColor_DefaultWhite()
    {
        var e = new ShapeElement();
        Assert.Equal("#FFFFFF", e.FillColor);
    }

    [Fact]
    public void ShapeElement_FillColor_SetValue()
    {
        var e = new ShapeElement { FillColor = "#CCCCCC" };
        Assert.Equal("#CCCCCC", e.FillColor);
    }

    [Fact]
    public void ShapeElement_BorderRadius_DefaultZero()
    {
        var e = new ShapeElement();
        Assert.Equal(0, e.BorderRadius);
    }

    [Fact]
    public void ShapeElement_BorderRadius_SetValue()
    {
        var e = new ShapeElement { BorderRadius = 5 };
        Assert.Equal(5, e.BorderRadius);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// TableElement 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class TableElementComplete6Tests
{
    [Fact]
    public void TableElement_RowCount_Default3()
    {
        var e = new TableElement();
        Assert.Equal(3, e.RowCount);
    }

    [Fact]
    public void TableElement_RowCount_SetValue()
    {
        var e = new TableElement { RowCount = 5 };
        Assert.Equal(5, e.RowCount);
    }

    [Fact]
    public void TableElement_ColCount_Default3()
    {
        var e = new TableElement();
        Assert.Equal(3, e.ColCount);
    }

    [Fact]
    public void TableElement_ColCount_SetValue()
    {
        var e = new TableElement { ColCount = 4 };
        Assert.Equal(4, e.ColCount);
    }

    [Fact]
    public void TableElement_ColumnWidths_DefaultNotNull()
    {
        var e = new TableElement();
        Assert.NotNull(e.ColumnWidths);
    }

    [Fact]
    public void TableElement_ColumnWidths_SetValue()
    {
        var e = new TableElement { ColumnWidths = new List<double> { 50, 100, 50 } };
        Assert.NotNull(e.ColumnWidths);
        Assert.Equal(3, e.ColumnWidths.Count);
    }

    [Fact]
    public void TableElement_RowHeights_DefaultNotNull()
    {
        var e = new TableElement();
        Assert.NotNull(e.RowHeights);
    }

    [Fact]
    public void TableElement_RowHeights_SetValue()
    {
        var e = new TableElement { RowHeights = new List<double> { 20, 25, 30 } };
        Assert.NotNull(e.RowHeights);
        Assert.Equal(3, e.RowHeights.Count);
    }

    [Fact]
    public void TableElement_Cells_DefaultEmpty()
    {
        var e = new TableElement();
        Assert.NotNull(e.Cells);
        Assert.Empty(e.Cells);
    }

    [Fact]
    public void TableElement_Cells_AddItem()
    {
        var e = new TableElement();
        e.Cells.Add(new TableCell { Row = 0, Col = 0 });
        Assert.Single(e.Cells);
    }

    [Fact]
    public void TableElement_BorderWidth_Default03()
    {
        var e = new TableElement();
        Assert.Equal(0.3, e.BorderWidth);
    }

    [Fact]
    public void TableElement_BorderWidth_SetValue()
    {
        var e = new TableElement { BorderWidth = 1.0 };
        Assert.Equal(1.0, e.BorderWidth);
    }

    [Fact]
    public void TableElement_BorderColor_DefaultBlack()
    {
        var e = new TableElement();
        Assert.Equal("#000000", e.BorderColor);
    }

    [Fact]
    public void TableElement_BorderColor_SetValue()
    {
        var e = new TableElement { BorderColor = "#999999" };
        Assert.Equal("#999999", e.BorderColor);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// TableCell 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class TableCellComplete6Tests
{
    [Fact]
    public void TableCell_Row_Default0()
    {
        var c = new TableCell();
        Assert.Equal(0, c.Row);
    }

    [Fact]
    public void TableCell_Row_SetValue()
    {
        var c = new TableCell { Row = 2 };
        Assert.Equal(2, c.Row);
    }

    [Fact]
    public void TableCell_Col_Default0()
    {
        var c = new TableCell();
        Assert.Equal(0, c.Col);
    }

    [Fact]
    public void TableCell_Col_SetValue()
    {
        var c = new TableCell { Col = 3 };
        Assert.Equal(3, c.Col);
    }

    [Fact]
    public void TableCell_RowSpan_Default1()
    {
        var c = new TableCell();
        Assert.Equal(1, c.RowSpan);
    }

    [Fact]
    public void TableCell_RowSpan_SetValue()
    {
        var c = new TableCell { RowSpan = 2 };
        Assert.Equal(2, c.RowSpan);
    }

    [Fact]
    public void TableCell_ColSpan_Default1()
    {
        var c = new TableCell();
        Assert.Equal(1, c.ColSpan);
    }

    [Fact]
    public void TableCell_ColSpan_SetValue()
    {
        var c = new TableCell { ColSpan = 3 };
        Assert.Equal(3, c.ColSpan);
    }

    [Fact]
    public void TableCell_Alignment_DefaultCenter()
    {
        var c = new TableCell();
        Assert.Equal(TextAlignment.Center, c.Alignment);
    }

    [Fact]
    public void TableCell_Alignment_SetLeft()
    {
        var c = new TableCell { Alignment = TextAlignment.Left };
        Assert.Equal(TextAlignment.Left, c.Alignment);
    }

    [Fact]
    public void TableCell_Alignment_SetRight()
    {
        var c = new TableCell { Alignment = TextAlignment.Right };
        Assert.Equal(TextAlignment.Right, c.Alignment);
    }

    [Fact]
    public void TableCell_Text_DefaultEmpty()
    {
        var c = new TableCell();
        Assert.Equal("", c.Text);
    }

    [Fact]
    public void TableCell_Text_SetValue()
    {
        var c = new TableCell { Text = "Header" };
        Assert.Equal("Header", c.Text);
    }

    [Fact]
    public void TableCell_Font_DefaultNotNull()
    {
        var c = new TableCell();
        Assert.NotNull(c.Font);
    }

    [Fact]
    public void TableCell_Font_SetValue()
    {
        var c = new TableCell { Font = new FontDef { Family = "Arial" } };
        Assert.NotNull(c.Font);
        Assert.Equal("Arial", c.Font.Family);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// BarcodeElement 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class BarcodeElementComplete6Tests
{
    [Fact]
    public void BarcodeElement_Format_DefaultQRCode()
    {
        var e = new BarcodeElement();
        Assert.Equal(BarcodeFormat.QRCode, e.Format);
    }

    [Fact]
    public void BarcodeElement_Format_SetCode128()
    {
        var e = new BarcodeElement { Format = BarcodeFormat.Code128 };
        Assert.Equal(BarcodeFormat.Code128, e.Format);
    }

    [Fact]
    public void BarcodeElement_Format_SetEAN13()
    {
        var e = new BarcodeElement { Format = BarcodeFormat.EAN13 };
        Assert.Equal(BarcodeFormat.EAN13, e.Format);
    }

    [Fact]
    public void BarcodeElement_Value_DefaultEmpty()
    {
        var e = new BarcodeElement();
        Assert.Equal("", e.Value);
    }

    [Fact]
    public void BarcodeElement_Value_SetValue()
    {
        var e = new BarcodeElement { Value = "123456789012" };
        Assert.Equal("123456789012", e.Value);
    }

    [Fact]
    public void BarcodeElement_ShowText_DefaultTrue()
    {
        var e = new BarcodeElement();
        Assert.True(e.ShowText);
    }

    [Fact]
    public void BarcodeElement_ShowText_SetTrue()
    {
        var e = new BarcodeElement { ShowText = true };
        Assert.True(e.ShowText);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// ChartElement 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class ChartElementComplete6Tests
{
    [Fact]
    public void ChartElement_ChartType_DefaultBar()
    {
        var e = new ChartElement();
        Assert.Equal(ChartType.Bar, e.ChartType);
    }

    [Fact]
    public void ChartElement_ChartType_SetLine()
    {
        var e = new ChartElement { ChartType = ChartType.Line };
        Assert.Equal(ChartType.Line, e.ChartType);
    }

    [Fact]
    public void ChartElement_ChartType_SetPie()
    {
        var e = new ChartElement { ChartType = ChartType.Pie };
        Assert.Equal(ChartType.Pie, e.ChartType);
    }

    [Fact]
    public void ChartElement_ChartType_SetArea()
    {
        var e = new ChartElement { ChartType = ChartType.Area };
        Assert.Equal(ChartType.Area, e.ChartType);
    }

    [Fact]
    public void ChartElement_ChartType_SetScatter()
    {
        var e = new ChartElement { ChartType = ChartType.Scatter };
        Assert.Equal(ChartType.Scatter, e.ChartType);
    }

    [Fact]
    public void ChartElement_DataSource_DefaultEmpty()
    {
        var e = new ChartElement();
        Assert.Equal("", e.DataSource);
    }

    [Fact]
    public void ChartElement_DataSource_SetValue()
    {
        var e = new ChartElement { DataSource = "sales" };
        Assert.Equal("sales", e.DataSource);
    }

    [Fact]
    public void ChartElement_CategoryField_DefaultEmpty()
    {
        var e = new ChartElement();
        Assert.Equal("", e.CategoryField);
    }

    [Fact]
    public void ChartElement_CategoryField_SetValue()
    {
        var e = new ChartElement { CategoryField = "month" };
        Assert.Equal("month", e.CategoryField);
    }

    [Fact]
    public void ChartElement_Series_DefaultEmpty()
    {
        var e = new ChartElement();
        Assert.NotNull(e.Series);
        Assert.Empty(e.Series);
    }

    [Fact]
    public void ChartElement_Series_AddItem()
    {
        var e = new ChartElement();
        e.Series.Add(new ChartSeries { Name = "Revenue", ValueField = "amount" });
        Assert.Single(e.Series);
        Assert.Equal("Revenue", e.Series[0].Name);
        Assert.Equal("amount", e.Series[0].ValueField);
    }

    [Fact]
    public void ChartElement_Title_DefaultNull()
    {
        var e = new ChartElement();
        Assert.Null(e.Title);
    }

    [Fact]
    public void ChartElement_Title_SetValue()
    {
        var e = new ChartElement { Title = "Sales Chart" };
        Assert.Equal("Sales Chart", e.Title);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// ChartSeries 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class ChartSeriesComplete6Tests
{
    [Fact]
    public void ChartSeries_Name_DefaultEmpty()
    {
        var s = new ChartSeries();
        Assert.Equal("", s.Name);
    }

    [Fact]
    public void ChartSeries_Name_SetValue()
    {
        var s = new ChartSeries { Name = "Revenue" };
        Assert.Equal("Revenue", s.Name);
    }

    [Fact]
    public void ChartSeries_ValueField_DefaultEmpty()
    {
        var s = new ChartSeries();
        Assert.Equal("", s.ValueField);
    }

    [Fact]
    public void ChartSeries_ValueField_SetValue()
    {
        var s = new ChartSeries { ValueField = "amount" };
        Assert.Equal("amount", s.ValueField);
    }

    [Fact]
    public void ChartSeries_Color_DefaultNull()
    {
        var s = new ChartSeries();
        Assert.Null(s.Color);
    }

    [Fact]
    public void ChartSeries_Color_SetValue()
    {
        var s = new ChartSeries { Color = "#FF6600" };
        Assert.Equal("#FF6600", s.Color);
    }
}
