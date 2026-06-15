using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Rendering;
using ReportEngine.Core.Parsing;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ConditionalFormatRule 完整属性测试
/// </summary>
public class ConditionalFormatRuleFullTests
{
    [Fact]
    public void ConditionalFormatRule_Expression_DefaultEmpty()
    {
        var r = new ConditionalFormatRule();
        Assert.Equal("", r.Expression);
    }

    [Fact]
    public void ConditionalFormatRule_Expression_Settable()
    {
        var r = new ConditionalFormatRule { Expression = "[Amount] > 1000" };
        Assert.Equal("[Amount] > 1000", r.Expression);
    }

    [Fact]
    public void ConditionalFormatRule_BackgroundColor_DefaultNull()
    {
        var r = new ConditionalFormatRule();
        Assert.Null(r.BackgroundColor);
    }

    [Fact]
    public void ConditionalFormatRule_FontColor_DefaultNull()
    {
        var r = new ConditionalFormatRule();
        Assert.Null(r.FontColor);
    }

    [Fact]
    public void ConditionalFormatRule_Bold_DefaultFalse()
    {
        var r = new ConditionalFormatRule();
        Assert.False(r.Bold);
    }

    [Fact]
    public void ConditionalFormatRule_Bold_Settable()
    {
        var r = new ConditionalFormatRule { Bold = true };
        Assert.True(r.Bold);
    }

    [Fact]
    public void ConditionalFormatRule_FullSetup()
    {
        var r = new ConditionalFormatRule
        {
            Expression = "[Amount] > 1000",
            BackgroundColor = "#FF0000",
            FontColor = "#FFFFFF",
            Bold = true
        };
        Assert.Equal("[Amount] > 1000", r.Expression);
        Assert.Equal("#FF0000", r.BackgroundColor);
        Assert.Equal("#FFFFFF", r.FontColor);
        Assert.True(r.Bold);
    }
}

/// <summary>
/// MultiColumnConfig 完整属性测试
/// </summary>
public class MultiColumnConfigFullTests
{
    [Fact]
    public void MultiColumnConfig_ColumnCount_Default2()
    {
        var c = new MultiColumnConfig();
        Assert.Equal(2, c.ColumnCount);
    }

    [Fact]
    public void MultiColumnConfig_ColumnCount_Settable()
    {
        var c = new MultiColumnConfig { ColumnCount = 4 };
        Assert.Equal(4, c.ColumnCount);
    }

    [Fact]
    public void MultiColumnConfig_ColumnSpacing_Default5()
    {
        var c = new MultiColumnConfig();
        Assert.Equal(5, c.ColumnSpacing);
    }

    [Fact]
    public void MultiColumnConfig_ColumnSpacing_Settable()
    {
        var c = new MultiColumnConfig { ColumnSpacing = 10 };
        Assert.Equal(10, c.ColumnSpacing);
    }

    [Fact]
    public void MultiColumnConfig_Direction_DefaultHorizontal()
    {
        var c = new MultiColumnConfig();
        Assert.Equal("Horizontal", c.Direction);
    }

    [Fact]
    public void MultiColumnConfig_Direction_Vertical()
    {
        var c = new MultiColumnConfig { Direction = "Vertical" };
        Assert.Equal("Vertical", c.Direction);
    }

    [Fact]
    public void MultiColumnConfig_FullSetup()
    {
        var c = new MultiColumnConfig { ColumnCount = 3, ColumnSpacing = 8, Direction = "Vertical" };
        Assert.Equal(3, c.ColumnCount);
        Assert.Equal(8, c.ColumnSpacing);
        Assert.Equal("Vertical", c.Direction);
    }
}

/// <summary>
/// Band.MultiColumn 属性测试
/// </summary>
public class BandMultiColumnTests
{
    [Fact]
    public void Band_MultiColumn_DefaultNull()
    {
        var b = new Band();
        Assert.Null(b.MultiColumn);
    }

    [Fact]
    public void Band_MultiColumn_Settable()
    {
        var b = new Band { MultiColumn = new MultiColumnConfig { ColumnCount = 3 } };
        Assert.NotNull(b.MultiColumn);
        Assert.Equal(3, b.MultiColumn.ColumnCount);
    }

    [Fact]
    public void Band_MultiColumn_OnlyOnDetail()
    {
        var b = new Band { Type = BandType.Detail, MultiColumn = new MultiColumnConfig() };
        Assert.NotNull(b.MultiColumn);
    }
}

/// <summary>
/// TableElement 完整属性测试
/// </summary>
public class TableElementFullTests
{
    [Fact]
    public void TableElement_RowCount_Default3()
    {
        var t = new TableElement();
        Assert.Equal(3, t.RowCount);
    }

    [Fact]
    public void TableElement_ColCount_Default3()
    {
        var t = new TableElement();
        Assert.Equal(3, t.ColCount);
    }

    [Fact]
    public void TableElement_ColumnWidths_DefaultEmpty()
    {
        var t = new TableElement();
        Assert.NotNull(t.ColumnWidths);
        Assert.Empty(t.ColumnWidths);
    }

    [Fact]
    public void TableElement_ColumnWidths_Addable()
    {
        var t = new TableElement();
        t.ColumnWidths.Add(30);
        t.ColumnWidths.Add(50);
        Assert.Equal(2, t.ColumnWidths.Count);
    }

    [Fact]
    public void TableElement_RowHeights_DefaultEmpty()
    {
        var t = new TableElement();
        Assert.NotNull(t.RowHeights);
        Assert.Empty(t.RowHeights);
    }

    [Fact]
    public void TableElement_RowHeights_Addable()
    {
        var t = new TableElement();
        t.RowHeights.Add(10);
        t.RowHeights.Add(15);
        Assert.Equal(2, t.RowHeights.Count);
    }

    [Fact]
    public void TableElement_Cells_DefaultEmpty()
    {
        var t = new TableElement();
        Assert.NotNull(t.Cells);
        Assert.Empty(t.Cells);
    }

    [Fact]
    public void TableElement_Cells_Addable()
    {
        var t = new TableElement();
        t.Cells.Add(new TableCell { Row = 0, Col = 0, Text = "A1" });
        Assert.Single(t.Cells);
    }

    [Fact]
    public void TableElement_BorderWidth_Default03()
    {
        var t = new TableElement();
        Assert.Equal(0.3, t.BorderWidth);
    }

    [Fact]
    public void TableElement_BorderColor_Default000000()
    {
        var t = new TableElement();
        Assert.Equal("#000000", t.BorderColor);
    }

    [Fact]
    public void TableElement_FullSetup()
    {
        var t = new TableElement
        {
            RowCount = 4,
            ColCount = 3,
            BorderWidth = 0.5,
            BorderColor = "#FF0000"
        };
        t.ColumnWidths.AddRange(new[] { 30.0, 50, 40 });
        t.RowHeights.AddRange(new[] { 10.0, 10, 10, 10 });
        t.Cells.Add(new TableCell { Row = 0, Col = 0, Text = "Header" });

        Assert.Equal(4, t.RowCount);
        Assert.Equal(3, t.ColCount);
        Assert.Equal(3, t.ColumnWidths.Count);
        Assert.Equal(4, t.RowHeights.Count);
        Assert.Single(t.Cells);
        Assert.Equal(0.5, t.BorderWidth);
        Assert.Equal("#FF0000", t.BorderColor);
    }
}

/// <summary>
/// TableCell 完整属性测试
/// </summary>
public class TableCellFullTests
{
    [Fact]
    public void TableCell_Row_Default0()
    {
        var c = new TableCell();
        Assert.Equal(0, c.Row);
    }

    [Fact]
    public void TableCell_Col_Default0()
    {
        var c = new TableCell();
        Assert.Equal(0, c.Col);
    }

    [Fact]
    public void TableCell_RowSpan_Default1()
    {
        var c = new TableCell();
        Assert.Equal(1, c.RowSpan);
    }

    [Fact]
    public void TableCell_ColSpan_Default1()
    {
        var c = new TableCell();
        Assert.Equal(1, c.ColSpan);
    }

    [Fact]
    public void TableCell_Text_DefaultEmpty()
    {
        var c = new TableCell();
        Assert.Equal("", c.Text);
    }

    [Fact]
    public void TableCell_Font_DefaultNotNull()
    {
        var c = new TableCell();
        Assert.NotNull(c.Font);
    }

    [Fact]
    public void TableCell_Alignment_DefaultCenter()
    {
        var c = new TableCell();
        Assert.Equal(TextAlignment.Center, c.Alignment);
    }

    [Fact]
    public void TableCell_BackgroundColor_DefaultNull()
    {
        var c = new TableCell();
        Assert.Null(c.BackgroundColor);
    }

    [Fact]
    public void TableCell_MergedCell()
    {
        var c = new TableCell { Row = 0, Col = 0, RowSpan = 2, ColSpan = 3, Text = "Merged" };
        Assert.Equal(2, c.RowSpan);
        Assert.Equal(3, c.ColSpan);
        Assert.Equal("Merged", c.Text);
    }

    [Fact]
    public void TableCell_FullSetup()
    {
        var c = new TableCell
        {
            Row = 1,
            Col = 2,
            RowSpan = 1,
            ColSpan = 1,
            Text = "{{amount}}",
            Font = new FontDef { Size = 12, Bold = true },
            Alignment = TextAlignment.Right,
            BackgroundColor = "#F0F0F0"
        };
        Assert.Equal(1, c.Row);
        Assert.Equal(2, c.Col);
        Assert.Equal("{{amount}}", c.Text);
        Assert.Equal(12, c.Font.Size);
        Assert.True(c.Font.Bold);
        Assert.Equal(TextAlignment.Right, c.Alignment);
        Assert.Equal("#F0F0F0", c.BackgroundColor);
    }
}

/// <summary>
/// CrossTabMeasure 完整属性测试
/// </summary>
public class CrossTabMeasureFullTests2
{
    [Fact]
    public void CrossTabMeasure_Field_DefaultEmpty()
    {
        var m = new CrossTabMeasure();
        Assert.Equal("", m.Field);
    }

    [Fact]
    public void CrossTabMeasure_Aggregate_DefaultSum()
    {
        var m = new CrossTabMeasure();
        Assert.Equal("Sum", m.Aggregate);
    }

    [Fact]
    public void CrossTabMeasure_Aggregate_Count()
    {
        var m = new CrossTabMeasure { Aggregate = "Count" };
        Assert.Equal("Count", m.Aggregate);
    }

    [Fact]
    public void CrossTabMeasure_Aggregate_Avg()
    {
        var m = new CrossTabMeasure { Aggregate = "Avg" };
        Assert.Equal("Avg", m.Aggregate);
    }

    [Fact]
    public void CrossTabMeasure_Aggregate_Min()
    {
        var m = new CrossTabMeasure { Aggregate = "Min" };
        Assert.Equal("Min", m.Aggregate);
    }

    [Fact]
    public void CrossTabMeasure_Aggregate_Max()
    {
        var m = new CrossTabMeasure { Aggregate = "Max" };
        Assert.Equal("Max", m.Aggregate);
    }

    [Fact]
    public void CrossTabMeasure_Format_DefaultNull()
    {
        var m = new CrossTabMeasure();
        Assert.Null(m.Format);
    }

    [Fact]
    public void CrossTabMeasure_Format_Settable()
    {
        var m = new CrossTabMeasure { Format = "C2" };
        Assert.Equal("C2", m.Format);
    }

    [Fact]
    public void CrossTabMeasure_Label_DefaultNull()
    {
        var m = new CrossTabMeasure();
        Assert.Null(m.Label);
    }

    [Fact]
    public void CrossTabMeasure_Label_Settable()
    {
        var m = new CrossTabMeasure { Label = "Total Revenue" };
        Assert.Equal("Total Revenue", m.Label);
    }

    [Fact]
    public void CrossTabMeasure_FullSetup()
    {
        var m = new CrossTabMeasure
        {
            Field = "amount",
            Aggregate = "Sum",
            Format = "C2",
            Label = "Total"
        };
        Assert.Equal("amount", m.Field);
        Assert.Equal("Sum", m.Aggregate);
        Assert.Equal("C2", m.Format);
        Assert.Equal("Total", m.Label);
    }
}

/// <summary>
/// ReportElement 更多属性测试
/// </summary>
public class ReportElementMorePropsTests
{
    [Fact]
    public void ReportElement_Name_DefaultNull()
    {
        var el = new TextElement();
        Assert.Null(el.Name);
    }

    [Fact]
    public void ReportElement_Name_Settable()
    {
        var el = new TextElement { Name = "title" };
        Assert.Equal("title", el.Name);
    }

    [Fact]
    public void ReportElement_GroupId_DefaultNull()
    {
        var el = new TextElement();
        Assert.Null(el.GroupId);
    }

    [Fact]
    public void ReportElement_GroupId_Settable()
    {
        var el = new TextElement { GroupId = "group1" };
        Assert.Equal("group1", el.GroupId);
    }

    [Fact]
    public void ReportElement_VisibleExpression_DefaultNull()
    {
        var el = new TextElement();
        Assert.Null(el.VisibleExpression);
    }

    [Fact]
    public void ReportElement_VisibleExpression_Settable()
    {
        var el = new TextElement { VisibleExpression = "{{showTitle}}" };
        Assert.Equal("{{showTitle}}", el.VisibleExpression);
    }

    [Fact]
    public void ReportElement_ConditionalFormats_DefaultEmpty()
    {
        var el = new TextElement();
        Assert.NotNull(el.ConditionalFormats);
        Assert.Empty(el.ConditionalFormats);
    }

    [Fact]
    public void ReportElement_ConditionalFormats_Addable()
    {
        var el = new TextElement();
        el.ConditionalFormats.Add(new ConditionalFormatRule { Expression = "[val] > 100", FontColor = "#FF0000" });
        Assert.Single(el.ConditionalFormats);
    }

    [Fact]
    public void ReportElement_MultipleConditionalFormats()
    {
        var el = new TextElement();
        el.ConditionalFormats.Add(new ConditionalFormatRule { Expression = "[val] > 100", FontColor = "#FF0000" });
        el.ConditionalFormats.Add(new ConditionalFormatRule { Expression = "[val] > 50", FontColor = "#FFA500" });
        el.ConditionalFormats.Add(new ConditionalFormatRule { Expression = "[val] <= 50", FontColor = "#00FF00" });
        Assert.Equal(3, el.ConditionalFormats.Count);
    }
}
