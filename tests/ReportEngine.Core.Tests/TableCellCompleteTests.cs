using ReportEngine.Core;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// TableCell 完整字段测试：
///   - TableCell 完整字段（Row/Col/RowSpan/ColSpan/Text/Font/Alignment/BackgroundColor）
///   - 字段组合行为
/// </summary>
public class TableCellCompleteTests
{
    [Fact]
    public void TableCell_Defaults()
    {
        var c = new TableCell();
        Assert.Equal(0, c.Row);
        Assert.Equal(0, c.Col);
        Assert.Equal(1, c.RowSpan);
        Assert.Equal(1, c.ColSpan);
        Assert.Equal("", c.Text);
        Assert.NotNull(c.Font);
        Assert.Equal(TextAlignment.Center, c.Alignment);
        Assert.Null(c.BackgroundColor);
    }

    [Fact]
    public void TableCell_AllSetters()
    {
        var c = new TableCell
        {
            Row = 2,
            Col = 3,
            RowSpan = 2,
            ColSpan = 3,
            Text = "合计",
            Font = new FontDef { Family = "Arial", Size = 12 },
            Alignment = TextAlignment.Right,
            BackgroundColor = "#FFFF00",
        };
        Assert.Equal(2, c.Row);
        Assert.Equal(3, c.Col);
        Assert.Equal(2, c.RowSpan);
        Assert.Equal(3, c.ColSpan);
        Assert.Equal("合计", c.Text);
        Assert.Equal("Arial", c.Font.Family);
        Assert.Equal(12, c.Font.Size);
        Assert.Equal(TextAlignment.Right, c.Alignment);
        Assert.Equal("#FFFF00", c.BackgroundColor);
    }

    [Fact]
    public void TableCell_Row_CanBeZero()
    {
        var c = new TableCell { Row = 0 };
        Assert.Equal(0, c.Row);
    }

    [Fact]
    public void TableCell_Row_CanBePositive()
    {
        var c = new TableCell { Row = 5 };
        Assert.Equal(5, c.Row);
    }

    [Fact]
    public void TableCell_Col_CanBeZero()
    {
        var c = new TableCell { Col = 0 };
        Assert.Equal(0, c.Col);
    }

    [Fact]
    public void TableCell_Col_CanBePositive()
    {
        var c = new TableCell { Col = 10 };
        Assert.Equal(10, c.Col);
    }

    [Fact]
    public void TableCell_RowSpan_Default1()
    {
        var c = new TableCell();
        Assert.Equal(1, c.RowSpan);
    }

    [Fact]
    public void TableCell_RowSpan_CanBeLarger()
    {
        var c = new TableCell { RowSpan = 3 };
        Assert.Equal(3, c.RowSpan);
    }

    [Fact]
    public void TableCell_ColSpan_Default1()
    {
        var c = new TableCell();
        Assert.Equal(1, c.ColSpan);
    }

    [Fact]
    public void TableCell_ColSpan_CanBeLarger()
    {
        var c = new TableCell { ColSpan = 4 };
        Assert.Equal(4, c.ColSpan);
    }

    [Fact]
    public void TableCell_Text_CanBeEmpty()
    {
        var c = new TableCell { Text = "" };
        Assert.Equal("", c.Text);
    }

    [Fact]
    public void TableCell_Text_CanBeExpression()
    {
        var c = new TableCell { Text = "{{SUM(amount)}}" };
        Assert.Equal("{{SUM(amount)}}", c.Text);
    }

    [Fact]
    public void TableCell_Text_CanBeChinese()
    {
        var c = new TableCell { Text = "总计" };
        Assert.Equal("总计", c.Text);
    }

    [Fact]
    public void TableCell_Font_DefaultNotNull()
    {
        var c = new TableCell();
        Assert.NotNull(c.Font);
        Assert.Equal("SimSun", c.Font.Family);
        Assert.Equal(10, c.Font.Size);
    }

    [Fact]
    public void TableCell_Font_CanBeReplaced()
    {
        var c = new TableCell { Font = new FontDef { Family = "Calibri", Bold = true } };
        Assert.Equal("Calibri", c.Font.Family);
        Assert.True(c.Font.Bold);
    }

    [Fact]
    public void TableCell_Alignment_DefaultCenter()
    {
        var c = new TableCell();
        Assert.Equal(TextAlignment.Center, c.Alignment);
    }

    [Fact]
    public void TableCell_Alignment_CanBeLeft()
    {
        var c = new TableCell { Alignment = TextAlignment.Left };
        Assert.Equal(TextAlignment.Left, c.Alignment);
    }

    [Fact]
    public void TableCell_Alignment_CanBeRight()
    {
        var c = new TableCell { Alignment = TextAlignment.Right };
        Assert.Equal(TextAlignment.Right, c.Alignment);
    }

    [Fact]
    public void TableCell_Alignment_CanBeJustify()
    {
        var c = new TableCell { Alignment = TextAlignment.Justify };
        Assert.Equal(TextAlignment.Justify, c.Alignment);
    }

    [Fact]
    public void TableCell_BackgroundColor_CanBeNull()
    {
        var c = new TableCell { BackgroundColor = null };
        Assert.Null(c.BackgroundColor);
    }

    [Fact]
    public void TableCell_BackgroundColor_CanBeEmpty()
    {
        var c = new TableCell { BackgroundColor = "" };
        Assert.Equal("", c.BackgroundColor);
    }

    [Fact]
    public void TableCell_BackgroundColor_CanBeHex()
    {
        var c = new TableCell { BackgroundColor = "#00FF00" };
        Assert.Equal("#00FF00", c.BackgroundColor);
    }

    [Fact]
    public void TableCell_FullCombination()
    {
        var c = new TableCell
        {
            Row = 1,
            Col = 2,
            RowSpan = 2,
            ColSpan = 1,
            Text = "小计",
            Font = new FontDef { Bold = true },
            Alignment = TextAlignment.Right,
            BackgroundColor = "#CCCCCC",
        };
        Assert.Equal(1, c.Row);
        Assert.Equal(2, c.Col);
        Assert.Equal(2, c.RowSpan);
        Assert.Equal(1, c.ColSpan);
        Assert.Equal("小计", c.Text);
        Assert.True(c.Font.Bold);
        Assert.Equal(TextAlignment.Right, c.Alignment);
        Assert.Equal("#CCCCCC", c.BackgroundColor);
    }
}
