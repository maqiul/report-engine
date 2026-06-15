using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// TableElement 行为测试：
///   - 默认值
///   - 行列数
///   - 列宽/行高
///   - 单元格
///   - 边框
/// </summary>
public class TableElementBehaviorTests
{
    // ============== 默认值 ==============

    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var el = new TableElement();

        Assert.Equal(3, el.RowCount);
        Assert.Equal(3, el.ColCount);
        Assert.NotNull(el.ColumnWidths);
        Assert.Empty(el.ColumnWidths);
        Assert.NotNull(el.RowHeights);
        Assert.Empty(el.RowHeights);
        Assert.NotNull(el.Cells);
        Assert.Empty(el.Cells);
        Assert.Equal(0.3, el.BorderWidth);
        Assert.Equal("#000000", el.BorderColor);
    }

    // ============== RowCount ==============

    [Fact]
    public void RowCount_DefaultIs3()
    {
        var el = new TableElement();
        Assert.Equal(3, el.RowCount);
    }

    [Fact]
    public void RowCount_Set_Works()
    {
        var el = new TableElement { RowCount = 5 };
        Assert.Equal(5, el.RowCount);
    }

    [Fact]
    public void RowCount_Set1_Works()
    {
        var el = new TableElement { RowCount = 1 };
        Assert.Equal(1, el.RowCount);
    }

    [Fact]
    public void RowCount_SetLarge_Works()
    {
        var el = new TableElement { RowCount = 100 };
        Assert.Equal(100, el.RowCount);
    }

    [Fact]
    public void RowCount_CanBeChanged()
    {
        var el = new TableElement { RowCount = 3 };
        el.RowCount = 5;
        Assert.Equal(5, el.RowCount);
    }

    // ============== ColCount ==============

    [Fact]
    public void ColCount_DefaultIs3()
    {
        var el = new TableElement();
        Assert.Equal(3, el.ColCount);
    }

    [Fact]
    public void ColCount_Set_Works()
    {
        var el = new TableElement { ColCount = 4 };
        Assert.Equal(4, el.ColCount);
    }

    [Fact]
    public void ColCount_Set1_Works()
    {
        var el = new TableElement { ColCount = 1 };
        Assert.Equal(1, el.ColCount);
    }

    [Fact]
    public void ColCount_SetLarge_Works()
    {
        var el = new TableElement { ColCount = 20 };
        Assert.Equal(20, el.ColCount);
    }

    [Fact]
    public void ColCount_CanBeChanged()
    {
        var el = new TableElement { ColCount = 3 };
        el.ColCount = 6;
        Assert.Equal(6, el.ColCount);
    }

    // ============== ColumnWidths ==============

    [Fact]
    public void ColumnWidths_EmptyByDefault()
    {
        var el = new TableElement();
        Assert.Empty(el.ColumnWidths);
    }

    [Fact]
    public void ColumnWidths_Add_Works()
    {
        var el = new TableElement();
        el.ColumnWidths.Add(30);
        el.ColumnWidths.Add(50);
        el.ColumnWidths.Add(30);
        Assert.Equal(3, el.ColumnWidths.Count);
    }

    [Fact]
    public void ColumnWidths_EqualWidths_Works()
    {
        var el = new TableElement { ColCount = 3 };
        el.ColumnWidths.Add(40);
        el.ColumnWidths.Add(40);
        el.ColumnWidths.Add(40);
        Assert.All(el.ColumnWidths, w => Assert.Equal(40, w));
    }

    [Fact]
    public void ColumnWidths_UnequalWidths_Works()
    {
        var el = new TableElement { ColCount = 3 };
        el.ColumnWidths.Add(20);
        el.ColumnWidths.Add(60);
        el.ColumnWidths.Add(30);
        Assert.Equal(20, el.ColumnWidths[0]);
        Assert.Equal(60, el.ColumnWidths[1]);
        Assert.Equal(30, el.ColumnWidths[2]);
    }

    [Fact]
    public void ColumnWidths_Clear_Works()
    {
        var el = new TableElement();
        el.ColumnWidths.Add(30);
        el.ColumnWidths.Clear();
        Assert.Empty(el.ColumnWidths);
    }

    // ============== RowHeights ==============

    [Fact]
    public void RowHeights_EmptyByDefault()
    {
        var el = new TableElement();
        Assert.Empty(el.RowHeights);
    }

    [Fact]
    public void RowHeights_Add_Works()
    {
        var el = new TableElement();
        el.RowHeights.Add(10);
        el.RowHeights.Add(15);
        el.RowHeights.Add(10);
        Assert.Equal(3, el.RowHeights.Count);
    }

    [Fact]
    public void RowHeights_EqualHeights_Works()
    {
        var el = new TableElement { RowCount = 3 };
        el.RowHeights.Add(12);
        el.RowHeights.Add(12);
        el.RowHeights.Add(12);
        Assert.All(el.RowHeights, h => Assert.Equal(12, h));
    }

    [Fact]
    public void RowHeights_Clear_Works()
    {
        var el = new TableElement();
        el.RowHeights.Add(10);
        el.RowHeights.Clear();
        Assert.Empty(el.RowHeights);
    }

    // ============== Cells ==============

    [Fact]
    public void Cells_EmptyByDefault()
    {
        var el = new TableElement();
        Assert.Empty(el.Cells);
    }

    [Fact]
    public void Cells_Add_Works()
    {
        var el = new TableElement();
        el.Cells.Add(new TableCell { Row = 0, Col = 0, Text = "A1" });
        Assert.Single(el.Cells);
    }

    [Fact]
    public void Cells_AddMultiple_Works()
    {
        var el = new TableElement { RowCount = 2, ColCount = 2 };
        el.Cells.Add(new TableCell { Row = 0, Col = 0, Text = "A1" });
        el.Cells.Add(new TableCell { Row = 0, Col = 1, Text = "B1" });
        el.Cells.Add(new TableCell { Row = 1, Col = 0, Text = "A2" });
        el.Cells.Add(new TableCell { Row = 1, Col = 1, Text = "B2" });
        Assert.Equal(4, el.Cells.Count);
    }

    [Fact]
    public void Cells_Clear_Works()
    {
        var el = new TableElement();
        el.Cells.Add(new TableCell());
        el.Cells.Clear();
        Assert.Empty(el.Cells);
    }

    // ============== BorderWidth ==============

    [Fact]
    public void BorderWidth_DefaultIs03()
    {
        var el = new TableElement();
        Assert.Equal(0.3, el.BorderWidth);
    }

    [Fact]
    public void BorderWidth_SetThin_Works()
    {
        var el = new TableElement { BorderWidth = 0.1 };
        Assert.Equal(0.1, el.BorderWidth);
    }

    [Fact]
    public void BorderWidth_SetThick_Works()
    {
        var el = new TableElement { BorderWidth = 1.5 };
        Assert.Equal(1.5, el.BorderWidth);
    }

    [Fact]
    public void BorderWidth_CanBeChanged()
    {
        var el = new TableElement { BorderWidth = 0.3 };
        el.BorderWidth = 0.5;
        Assert.Equal(0.5, el.BorderWidth);
    }

    // ============== BorderColor ==============

    [Fact]
    public void BorderColor_DefaultIsBlack()
    {
        var el = new TableElement();
        Assert.Equal("#000000", el.BorderColor);
    }

    [Fact]
    public void BorderColor_SetGray_Works()
    {
        var el = new TableElement { BorderColor = "#CCCCCC" };
        Assert.Equal("#CCCCCC", el.BorderColor);
    }

    [Fact]
    public void BorderColor_SetBlue_Works()
    {
        var el = new TableElement { BorderColor = "#0000FF" };
        Assert.Equal("#0000FF", el.BorderColor);
    }

    [Fact]
    public void BorderColor_CanBeChanged()
    {
        var el = new TableElement { BorderColor = "#000000" };
        el.BorderColor = "#FF0000";
        Assert.Equal("#FF0000", el.BorderColor);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void TableElement_SimpleTable_Works()
    {
        var el = new TableElement
        {
            RowCount = 3,
            ColCount = 3,
            BorderWidth = 0.5,
            BorderColor = "#000000"
        };
        el.ColumnWidths.Add(40);
        el.ColumnWidths.Add(60);
        el.ColumnWidths.Add(40);
        el.RowHeights.Add(8);
        el.RowHeights.Add(8);
        el.RowHeights.Add(8);

        Assert.Equal(3, el.RowCount);
        Assert.Equal(3, el.ColCount);
        Assert.Equal(3, el.ColumnWidths.Count);
        Assert.Equal(3, el.RowHeights.Count);
    }

    [Fact]
    public void TableElement_WithCells_Works()
    {
        var el = new TableElement { RowCount = 2, ColCount = 2 };
        el.Cells.Add(new TableCell { Row = 0, Col = 0, Text = "Name" });
        el.Cells.Add(new TableCell { Row = 0, Col = 1, Text = "Value" });
        el.Cells.Add(new TableCell { Row = 1, Col = 0, Text = "Item1" });
        el.Cells.Add(new TableCell { Row = 1, Col = 1, Text = "100" });

        Assert.Equal(4, el.Cells.Count);
        Assert.Equal("Name", el.Cells[0].Text);
    }

    [Fact]
    public void TableElement_InBand_Works()
    {
        var band = new Band { Type = BandType.Detail, Height = 50 };
        band.Elements.Add(new TableElement
        {
            RowCount = 3,
            ColCount = 4,
            X = 10,
            Y = 5,
            Width = 180,
            Height = 40
        });

        Assert.Single(band.Elements);
        var table = Assert.IsType<TableElement>(band.Elements[0]);
        Assert.Equal(3, table.RowCount);
        Assert.Equal(4, table.ColCount);
    }

    [Fact]
    public void TableElement_CanBeModified()
    {
        var el = new TableElement { RowCount = 3, ColCount = 3 };
        
        el.RowCount = 5;
        el.ColCount = 4;
        el.BorderWidth = 0.5;
        el.BorderColor = "#CCCCCC";
        
        Assert.Equal(5, el.RowCount);
        Assert.Equal(4, el.ColCount);
        Assert.Equal(0.5, el.BorderWidth);
    }
}

/// <summary>
/// TableCell 行为测试
/// </summary>
public class TableCellBehaviorTests
{
    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var cell = new TableCell();

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
    public void Row_SetAndGet_Works()
    {
        var cell = new TableCell { Row = 2 };
        Assert.Equal(2, cell.Row);
    }

    [Fact]
    public void Col_SetAndGet_Works()
    {
        var cell = new TableCell { Col = 3 };
        Assert.Equal(3, cell.Col);
    }

    [Fact]
    public void RowSpan_DefaultIs1()
    {
        var cell = new TableCell();
        Assert.Equal(1, cell.RowSpan);
    }

    [Fact]
    public void RowSpan_Set_Works()
    {
        var cell = new TableCell { RowSpan = 2 };
        Assert.Equal(2, cell.RowSpan);
    }

    [Fact]
    public void ColSpan_DefaultIs1()
    {
        var cell = new TableCell();
        Assert.Equal(1, cell.ColSpan);
    }

    [Fact]
    public void ColSpan_Set_Works()
    {
        var cell = new TableCell { ColSpan = 3 };
        Assert.Equal(3, cell.ColSpan);
    }

    [Fact]
    public void Text_EmptyByDefault()
    {
        var cell = new TableCell();
        Assert.Equal("", cell.Text);
    }

    [Fact]
    public void Text_Set_Works()
    {
        var cell = new TableCell { Text = "Hello" };
        Assert.Equal("Hello", cell.Text);
    }

    [Fact]
    public void Text_SetExpression_Works()
    {
        var cell = new TableCell { Text = "{{currentRow.amount}}" };
        Assert.Contains("{{", cell.Text);
    }

    [Fact]
    public void Font_NotNull_ByDefault()
    {
        var cell = new TableCell();
        Assert.NotNull(cell.Font);
    }

    [Fact]
    public void Font_DefaultValues_AreCorrect()
    {
        var cell = new TableCell();
        Assert.Equal("SimSun", cell.Font.Family);
        Assert.Equal(10, cell.Font.Size);
    }

    [Fact]
    public void Font_CanBeCustomized()
    {
        var cell = new TableCell
        {
            Font = new FontDef { Family = "Arial", Size = 12, Bold = true }
        };
        Assert.Equal("Arial", cell.Font.Family);
        Assert.True(cell.Font.Bold);
    }

    [Fact]
    public void Alignment_DefaultIsCenter()
    {
        var cell = new TableCell();
        Assert.Equal(TextAlignment.Center, cell.Alignment);
    }

    [Fact]
    public void Alignment_SetLeft_Works()
    {
        var cell = new TableCell { Alignment = TextAlignment.Left };
        Assert.Equal(TextAlignment.Left, cell.Alignment);
    }

    [Fact]
    public void Alignment_SetRight_Works()
    {
        var cell = new TableCell { Alignment = TextAlignment.Right };
        Assert.Equal(TextAlignment.Right, cell.Alignment);
    }

    [Fact]
    public void BackgroundColor_NullByDefault()
    {
        var cell = new TableCell();
        Assert.Null(cell.BackgroundColor);
    }

    [Fact]
    public void BackgroundColor_Set_Works()
    {
        var cell = new TableCell { BackgroundColor = "#FFFFCC" };
        Assert.Equal("#FFFFCC", cell.BackgroundColor);
    }

    [Fact]
    public void TableCell_HeaderCell_Works()
    {
        var cell = new TableCell
        {
            Row = 0,
            Col = 0,
            Text = "Header",
            Font = new FontDef { Bold = true },
            Alignment = TextAlignment.Center,
            BackgroundColor = "#EEEEEE"
        };

        Assert.True(cell.Font.Bold);
        Assert.Equal("#EEEEEE", cell.BackgroundColor);
    }

    [Fact]
    public void TableCell_MergedCell_Works()
    {
        var cell = new TableCell
        {
            Row = 0,
            Col = 0,
            RowSpan = 2,
            ColSpan = 3,
            Text = "Merged"
        };

        Assert.Equal(2, cell.RowSpan);
        Assert.Equal(3, cell.ColSpan);
    }

    [Fact]
    public void TableCell_CanBeModified()
    {
        var cell = new TableCell { Text = "old" };
        
        cell.Text = "new";
        cell.Row = 1;
        cell.Col = 2;
        cell.Alignment = TextAlignment.Right;
        
        Assert.Equal("new", cell.Text);
        Assert.Equal(1, cell.Row);
        Assert.Equal(2, cell.Col);
        Assert.Equal(TextAlignment.Right, cell.Alignment);
    }
}
