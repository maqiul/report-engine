using System.Collections.Generic;
using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// TableElement 完整字段测试：
///   - TableElement 完整字段（RowCount/ColCount/ColumnWidths/RowHeights/Cells/BorderWidth/BorderColor）
///   - 字段组合行为
/// </summary>
public class TableElementCompleteTests
{
    [Fact]
    public void TableElement_Defaults()
    {
        var t = new TableElement();
        Assert.Equal(3, t.RowCount);
        Assert.Equal(3, t.ColCount);
        Assert.NotNull(t.ColumnWidths);
        Assert.Empty(t.ColumnWidths);
        Assert.NotNull(t.RowHeights);
        Assert.Empty(t.RowHeights);
        Assert.NotNull(t.Cells);
        Assert.Empty(t.Cells);
        Assert.Equal(0.3, t.BorderWidth);
        Assert.Equal("#000000", t.BorderColor);
    }

    [Fact]
    public void TableElement_AllSetters()
    {
        var t = new TableElement
        {
            RowCount = 5,
            ColCount = 4,
            BorderWidth = 0.5,
            BorderColor = "#FF0000",
        };
        t.ColumnWidths.AddRange(new[] { 20.0, 30.0, 25.0, 25.0 });
        t.RowHeights.AddRange(new[] { 10.0, 8.0, 8.0, 8.0, 12.0 });
        t.Cells.Add(new TableCell { Row = 0, Col = 0, Text = "A1" });

        Assert.Equal(5, t.RowCount);
        Assert.Equal(4, t.ColCount);
        Assert.Equal(4, t.ColumnWidths.Count);
        Assert.Equal(5, t.RowHeights.Count);
        Assert.Single(t.Cells);
        Assert.Equal(0.5, t.BorderWidth);
        Assert.Equal("#FF0000", t.BorderColor);
    }

    [Fact]
    public void TableElement_RowCount_Default3()
    {
        var t = new TableElement();
        Assert.Equal(3, t.RowCount);
    }

    [Fact]
    public void TableElement_RowCount_CanBeOne()
    {
        var t = new TableElement { RowCount = 1 };
        Assert.Equal(1, t.RowCount);
    }

    [Fact]
    public void TableElement_RowCount_CanBeLarge()
    {
        var t = new TableElement { RowCount = 100 };
        Assert.Equal(100, t.RowCount);
    }

    [Fact]
    public void TableElement_ColCount_Default3()
    {
        var t = new TableElement();
        Assert.Equal(3, t.ColCount);
    }

    [Fact]
    public void TableElement_ColCount_CanBeOne()
    {
        var t = new TableElement { ColCount = 1 };
        Assert.Equal(1, t.ColCount);
    }

    [Fact]
    public void TableElement_ColCount_CanBeLarge()
    {
        var t = new TableElement { ColCount = 20 };
        Assert.Equal(20, t.ColCount);
    }

    [Fact]
    public void TableElement_ColumnWidths_CanBeEmpty()
    {
        var t = new TableElement();
        Assert.Empty(t.ColumnWidths);
    }

    [Fact]
    public void TableElement_ColumnWidths_CanBeSet()
    {
        var t = new TableElement();
        t.ColumnWidths.AddRange(new[] { 10.0, 20.0, 30.0 });
        Assert.Equal(3, t.ColumnWidths.Count);
        Assert.Equal(10.0, t.ColumnWidths[0]);
    }

    [Fact]
    public void TableElement_RowHeights_CanBeEmpty()
    {
        var t = new TableElement();
        Assert.Empty(t.RowHeights);
    }

    [Fact]
    public void TableElement_RowHeights_CanBeSet()
    {
        var t = new TableElement();
        t.RowHeights.AddRange(new[] { 8.0, 10.0, 12.0 });
        Assert.Equal(3, t.RowHeights.Count);
        Assert.Equal(8.0, t.RowHeights[0]);
    }

    [Fact]
    public void TableElement_Cells_CanBeEmpty()
    {
        var t = new TableElement();
        Assert.Empty(t.Cells);
    }

    [Fact]
    public void TableElement_Cells_CanAddMultiple()
    {
        var t = new TableElement();
        t.Cells.Add(new TableCell { Row = 0, Col = 0 });
        t.Cells.Add(new TableCell { Row = 0, Col = 1 });
        t.Cells.Add(new TableCell { Row = 1, Col = 0 });
        Assert.Equal(3, t.Cells.Count);
    }

    [Fact]
    public void TableElement_BorderWidth_Default03()
    {
        var t = new TableElement();
        Assert.Equal(0.3, t.BorderWidth);
    }

    [Fact]
    public void TableElement_BorderWidth_CanBeZero()
    {
        var t = new TableElement { BorderWidth = 0 };
        Assert.Equal(0, t.BorderWidth);
    }

    [Fact]
    public void TableElement_BorderWidth_CanBeDecimal()
    {
        var t = new TableElement { BorderWidth = 1.5 };
        Assert.Equal(1.5, t.BorderWidth);
    }

    [Fact]
    public void TableElement_BorderColor_DefaultBlack()
    {
        var t = new TableElement();
        Assert.Equal("#000000", t.BorderColor);
    }

    [Fact]
    public void TableElement_BorderColor_CanBeRed()
    {
        var t = new TableElement { BorderColor = "#FF0000" };
        Assert.Equal("#FF0000", t.BorderColor);
    }

    [Fact]
    public void TableElement_FullCombination()
    {
        var t = new TableElement
        {
            RowCount = 4,
            ColCount = 3,
            BorderWidth = 0.5,
            BorderColor = "#333333",
        };
        t.ColumnWidths.AddRange(new[] { 30.0, 40.0, 30.0 });
        t.RowHeights.AddRange(new[] { 10.0, 8.0, 8.0, 12.0 });
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 3; c++)
                t.Cells.Add(new TableCell { Row = r, Col = c, Text = $"R{r}C{c}" });

        Assert.Equal(4, t.RowCount);
        Assert.Equal(3, t.ColCount);
        Assert.Equal(12, t.Cells.Count);
        Assert.Equal(0.5, t.BorderWidth);
        Assert.Equal("#333333", t.BorderColor);
    }
}
