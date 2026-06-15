using System.Linq;
using ReportEngine.Core;
using ReportEngine.Core.Rendering;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// RenderedTableElement / RenderedTableCell / RenderedCrossTabElement 行为测试：
///   - RenderedTableCell 合并行/列（RowSpan/ColSpan）
///   - RenderedTableElement 默认值（RowCount=0, ColCount=0, BorderWidth=0）
///   - RenderedCrossTabElement 默认值
///   - Cells 增删改
///   - ColumnWidths/RowHeights 设置
/// </summary>
public class RenderedTableAndCrossTabTests
{
    [Fact]
    public void RenderedTableCell_Defaults()
    {
        var c = new RenderedTableCell();
        Assert.Equal(0, c.Row);
        Assert.Equal(0, c.Col);
        Assert.Equal(1, c.RowSpan);
        Assert.Equal(1, c.ColSpan);
        Assert.Equal("", c.Text);
        Assert.Equal(TextAlignment.Center, c.Alignment);
        Assert.Null(c.BackgroundColor);
    }

    [Fact]
    public void RenderedTableCell_RowSpanAndColSpan()
    {
        var c = new RenderedTableCell { Row = 0, Col = 1, RowSpan = 2, ColSpan = 3, Text = "merged" };
        Assert.Equal(2, c.RowSpan);
        Assert.Equal(3, c.ColSpan);
        Assert.Equal("merged", c.Text);
    }

    [Fact]
    public void RenderedTableCell_Alignment_AllValues()
    {
        foreach (TextAlignment a in System.Enum.GetValues(typeof(TextAlignment)))
        {
            var c = new RenderedTableCell { Alignment = a };
            Assert.Equal(a, c.Alignment);
        }
    }

    [Fact]
    public void RenderedTableCell_Font_DefaultsToFontDef()
    {
        var c = new RenderedTableCell();
        Assert.NotNull(c.Font);
        // 默认是 SimSun（中文字体，不是 Arial）
        Assert.Equal("SimSun", c.Font.Family);
    }

    [Fact]
    public void RenderedTableCell_BackgroundColor_CanBeSet()
    {
        var c = new RenderedTableCell { BackgroundColor = "#FFFF00" };
        Assert.Equal("#FFFF00", c.BackgroundColor);
    }

    [Fact]
    public void RenderedTableElement_Defaults()
    {
        var e = new RenderedTableElement();
        Assert.Equal(0, e.RowCount);
        Assert.Equal(0, e.ColCount);
        Assert.NotNull(e.ColumnWidths);
        Assert.Empty(e.ColumnWidths);
        Assert.NotNull(e.RowHeights);
        Assert.Empty(e.RowHeights);
        Assert.NotNull(e.Cells);
        Assert.Empty(e.Cells);
        Assert.Equal(0, e.BorderWidth);
        Assert.Equal("#000000", e.BorderColor);
    }

    [Fact]
    public void RenderedTableElement_ColumnWidths_CanBeSet()
    {
        var e = new RenderedTableElement();
        e.ColumnWidths.Add(20);
        e.ColumnWidths.Add(30);
        e.ColumnWidths.Add(50);
        Assert.Equal(3, e.ColumnWidths.Count);
        Assert.Equal(20, e.ColumnWidths[0]);
    }

    [Fact]
    public void RenderedTableElement_RowHeights_CanBeSet()
    {
        var e = new RenderedTableElement();
        e.RowHeights.Add(10);
        e.RowHeights.Add(15);
        Assert.Equal(2, e.RowHeights.Count);
    }

    [Fact]
    public void RenderedTableElement_Cells_AddAndRemove()
    {
        var e = new RenderedTableElement();
        e.Cells.Add(new RenderedTableCell { Row = 0, Col = 0, Text = "A1" });
        e.Cells.Add(new RenderedTableCell { Row = 0, Col = 1, Text = "A2" });
        e.Cells.Add(new RenderedTableCell { Row = 1, Col = 0, Text = "B1" });
        Assert.Equal(3, e.Cells.Count);
        e.Cells.RemoveAt(1);
        Assert.Equal(2, e.Cells.Count);
        Assert.Equal("B1", e.Cells[1].Text);
    }

    [Fact]
    public void RenderedTableElement_Cells_QueryByRow()
    {
        var e = new RenderedTableElement();
        e.Cells.Add(new RenderedTableCell { Row = 0, Col = 0, Text = "A1" });
        e.Cells.Add(new RenderedTableCell { Row = 0, Col = 1, Text = "A2" });
        e.Cells.Add(new RenderedTableCell { Row = 1, Col = 0, Text = "B1" });
        var row0 = e.Cells.Where(c => c.Row == 0).ToList();
        Assert.Equal(2, row0.Count);
    }

    [Fact]
    public void RenderedCrossTabElement_Defaults()
    {
        var e = new RenderedCrossTabElement();
        Assert.Equal(0, e.RowCount);
        Assert.Equal(0, e.ColCount);
        Assert.NotNull(e.ColumnWidths);
        Assert.Empty(e.ColumnWidths);
        Assert.NotNull(e.RowHeights);
        Assert.Empty(e.RowHeights);
        Assert.NotNull(e.Cells);
        Assert.Empty(e.Cells);
        Assert.Equal(0, e.BorderWidth);
        Assert.Equal("#000000", e.BorderColor);
    }

    [Fact]
    public void RenderedCrossTabElement_Cells_AddAndRead()
    {
        var e = new RenderedCrossTabElement();
        e.Cells.Add(new RenderedTableCell { Row = 0, Col = 0, Text = "header" });
        e.Cells.Add(new RenderedTableCell { Row = 1, Col = 1, Text = "value" });
        Assert.Equal(2, e.Cells.Count);
    }

    [Fact]
    public void RenderedTableElement_InheritsRenderedElement()
    {
        var e = new RenderedTableElement();
        Assert.Equal("", e.Id);
        Assert.Equal(0, e.X);
        Assert.Equal(0, e.Y);
        Assert.Null(e.BackgroundColor);
        Assert.Null(e.Border);
    }

    [Fact]
    public void RenderedCrossTabElement_InheritsRenderedElement()
    {
        var e = new RenderedCrossTabElement();
        Assert.Equal("", e.Id);
        Assert.Null(e.Border);
    }

    [Fact]
    public void RenderedTableElement_BorderWidth_CanBeSet()
    {
        var e = new RenderedTableElement { BorderWidth = 0.5 };
        Assert.Equal(0.5, e.BorderWidth);
    }

    [Fact]
    public void RenderedCrossTabElement_BorderColor_CanBeSet()
    {
        var e = new RenderedCrossTabElement { BorderColor = "#FF0000" };
        Assert.Equal("#FF0000", e.BorderColor);
    }
}
