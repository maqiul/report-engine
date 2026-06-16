using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Parsing;
using Xunit;

namespace ReportEngine.Core.Tests;

// ─────────────────────────────────────────────────────────────────────────────
// ShapeElement 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class ShapeElementFull2Tests
{
    [Fact]
    public void ShapeElement_Shape_DefaultRectangle()
    {
        var el = new ShapeElement();
        Assert.Equal(ShapeType.Rectangle, el.Shape);
    }

    [Fact]
    public void ShapeElement_Shape_SetEllipse()
    {
        var el = new ShapeElement { Shape = ShapeType.Ellipse };
        Assert.Equal(ShapeType.Ellipse, el.Shape);
    }

    [Fact]
    public void ShapeElement_Shape_SetRoundedRect()
    {
        var el = new ShapeElement { Shape = ShapeType.RoundedRect };
        Assert.Equal(ShapeType.RoundedRect, el.Shape);
    }

    [Fact]
    public void ShapeElement_Shape_SetTriangle()
    {
        var el = new ShapeElement { Shape = ShapeType.Triangle };
        Assert.Equal(ShapeType.Triangle, el.Shape);
    }

    [Fact]
    public void ShapeElement_BorderRadius_DefaultZero()
    {
        var el = new ShapeElement();
        Assert.Equal(0, el.BorderRadius);
    }

    [Fact]
    public void ShapeElement_BorderRadius_SetValue()
    {
        var el = new ShapeElement { BorderRadius = 5 };
        Assert.Equal(5, el.BorderRadius);
    }

    [Fact]
    public void ShapeElement_FillColor_DefaultFFFFFF()
    {
        var el = new ShapeElement();
        Assert.Equal("#FFFFFF", el.FillColor);
    }

    [Fact]
    public void ShapeElement_FillColor_SetValue()
    {
        var el = new ShapeElement { FillColor = "#FF0000" };
        Assert.Equal("#FF0000", el.FillColor);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// SubReportElement 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class SubReportElementFull2Tests
{
    [Fact]
    public void SubReportElement_TemplateRef_DefaultEmpty()
    {
        var el = new SubReportElement();
        Assert.Equal("", el.TemplateRef);
    }

    [Fact]
    public void SubReportElement_TemplateRef_SetValue()
    {
        var el = new SubReportElement { TemplateRef = "sub.rptx" };
        Assert.Equal("sub.rptx", el.TemplateRef);
    }

    [Fact]
    public void SubReportElement_DataBinding_DefaultNotNull()
    {
        var el = new SubReportElement();
        Assert.NotNull(el.DataBinding);
    }

    [Fact]
    public void SubReportElement_DataBinding_Source_DefaultEmpty()
    {
        var el = new SubReportElement();
        Assert.Equal("", el.DataBinding.Source);
    }

    [Fact]
    public void SubReportElement_DataBinding_ParamMap_DefaultEmpty()
    {
        var el = new SubReportElement();
        Assert.NotNull(el.DataBinding.ParamMap);
        Assert.Empty(el.DataBinding.ParamMap);
    }

    [Fact]
    public void SubReportElement_HeightMode_DefaultAuto()
    {
        var el = new SubReportElement();
        Assert.Equal("auto", el.HeightMode);
    }

    [Fact]
    public void SubReportElement_HeightMode_SetFixed()
    {
        var el = new SubReportElement { HeightMode = "fixed" };
        Assert.Equal("fixed", el.HeightMode);
    }

    [Fact]
    public void SubReportElement_RepeatPerRow_DefaultTrue()
    {
        var el = new SubReportElement();
        Assert.True(el.RepeatPerRow);
    }

    [Fact]
    public void SubReportElement_RepeatPerRow_SetFalse()
    {
        var el = new SubReportElement { RepeatPerRow = false };
        Assert.False(el.RepeatPerRow);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// ConditionalFormatRule 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class ConditionalFormatRuleFull2Tests
{
    [Fact]
    public void ConditionalFormatRule_Expression_DefaultEmpty()
    {
        var r = new ConditionalFormatRule();
        Assert.Equal("", r.Expression);
    }

    [Fact]
    public void ConditionalFormatRule_Expression_SetValue()
    {
        var r = new ConditionalFormatRule { Expression = "[amount] > 1000" };
        Assert.Equal("[amount] > 1000", r.Expression);
    }

    [Fact]
    public void ConditionalFormatRule_BackgroundColor_DefaultNull()
    {
        var r = new ConditionalFormatRule();
        Assert.Null(r.BackgroundColor);
    }

    [Fact]
    public void ConditionalFormatRule_BackgroundColor_SetValue()
    {
        var r = new ConditionalFormatRule { BackgroundColor = "#FFFF00" };
        Assert.Equal("#FFFF00", r.BackgroundColor);
    }

    [Fact]
    public void ConditionalFormatRule_FontColor_DefaultNull()
    {
        var r = new ConditionalFormatRule();
        Assert.Null(r.FontColor);
    }

    [Fact]
    public void ConditionalFormatRule_FontColor_SetValue()
    {
        var r = new ConditionalFormatRule { FontColor = "#FF0000" };
        Assert.Equal("#FF0000", r.FontColor);
    }

    [Fact]
    public void ConditionalFormatRule_Bold_DefaultFalse()
    {
        var r = new ConditionalFormatRule();
        Assert.False(r.Bold);
    }

    [Fact]
    public void ConditionalFormatRule_Bold_SetTrue()
    {
        var r = new ConditionalFormatRule { Bold = true };
        Assert.True(r.Bold);
    }

    [Fact]
    public void ConditionalFormatRule_FullSetup()
    {
        var r = new ConditionalFormatRule
        {
            Expression = "[amount] > 1000",
            BackgroundColor = "#FFFF00",
            FontColor = "#FF0000",
            Bold = true
        };
        Assert.Equal("[amount] > 1000", r.Expression);
        Assert.Equal("#FFFF00", r.BackgroundColor);
        Assert.Equal("#FF0000", r.FontColor);
        Assert.True(r.Bold);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// TableElement 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class TableElementFull2Tests
{
    [Fact]
    public void TableElement_RowCount_Default3()
    {
        var el = new TableElement();
        Assert.Equal(3, el.RowCount);
    }

    [Fact]
    public void TableElement_ColCount_Default3()
    {
        var el = new TableElement();
        Assert.Equal(3, el.ColCount);
    }

    [Fact]
    public void TableElement_ColumnWidths_DefaultEmpty()
    {
        var el = new TableElement();
        Assert.NotNull(el.ColumnWidths);
        Assert.Empty(el.ColumnWidths);
    }

    [Fact]
    public void TableElement_RowHeights_DefaultEmpty()
    {
        var el = new TableElement();
        Assert.NotNull(el.RowHeights);
        Assert.Empty(el.RowHeights);
    }

    [Fact]
    public void TableElement_Cells_DefaultEmpty()
    {
        var el = new TableElement();
        Assert.NotNull(el.Cells);
        Assert.Empty(el.Cells);
    }

    [Fact]
    public void TableElement_BorderWidth_Default03()
    {
        var el = new TableElement();
        Assert.Equal(0.3, el.BorderWidth);
    }

    [Fact]
    public void TableElement_BorderColor_Default000000()
    {
        var el = new TableElement();
        Assert.Equal("#000000", el.BorderColor);
    }

    [Fact]
    public void TableElement_SetRowCount()
    {
        var el = new TableElement { RowCount = 5 };
        Assert.Equal(5, el.RowCount);
    }

    [Fact]
    public void TableElement_SetColCount()
    {
        var el = new TableElement { ColCount = 4 };
        Assert.Equal(4, el.ColCount);
    }

    [Fact]
    public void TableElement_AddColumnWidths()
    {
        var el = new TableElement();
        el.ColumnWidths.Add(30);
        el.ColumnWidths.Add(50);
        el.ColumnWidths.Add(70);
        Assert.Equal(3, el.ColumnWidths.Count);
    }

    [Fact]
    public void TableElement_AddCells()
    {
        var el = new TableElement { RowCount = 2, ColCount = 2 };
        el.Cells.Add(new TableCell { Row = 0, Col = 0, Text = "A" });
        el.Cells.Add(new TableCell { Row = 0, Col = 1, Text = "B" });
        el.Cells.Add(new TableCell { Row = 1, Col = 0, Text = "C" });
        el.Cells.Add(new TableCell { Row = 1, Col = 1, Text = "D" });
        Assert.Equal(4, el.Cells.Count);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// TableCell 完整属性测试
// ─────────────────────────────────────────────────────────────────────────────

public class TableCellFull2Tests
{
    [Fact]
    public void TableCell_Row_DefaultZero()
    {
        var c = new TableCell();
        Assert.Equal(0, c.Row);
    }

    [Fact]
    public void TableCell_Col_DefaultZero()
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
        Assert.Equal(0, c.Row);
        Assert.Equal(0, c.Col);
        Assert.Equal(2, c.RowSpan);
        Assert.Equal(3, c.ColSpan);
        Assert.Equal("Merged", c.Text);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// TemplateParser.ParseFile 测试
// ─────────────────────────────────────────────────────────────────────────────

public class TemplateParserFileTests
{
    [Fact]
    public void ParseFile_ThrowsFileNotFoundException_WhenMissing()
    {
        var parser = new TemplateParser();
        Assert.Throws<FileNotFoundException>(() => parser.ParseFile("nonexistent.rptx"));
    }

    [Fact]
    public void ParseFile_ParsesValidFile()
    {
        var parser = new TemplateParser();
        var template = new ReportTemplate();
        template.Bands.Add(new Band { Type = BandType.Header, Height = 25 });
        var json = parser.Serialize(template);

        var tmpFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tmpFile, json);
            var result = parser.ParseFile(tmpFile);
            Assert.Single(result.Bands);
            Assert.Equal(BandType.Header, result.Bands[0].Type);
        }
        finally
        {
            File.Delete(tmpFile);
        }
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// ExpressionEngine IF 条件测试
// ─────────────────────────────────────────────────────────────────────────────

public class ExpressionEngineIf2Tests
{
    private readonly ExpressionEngine _engine = new();

    [Fact]
    public void Evaluate_IF_TrueCondition_NonZero()
    {
        var ctx = new RenderContext();
        ctx.CurrentRow = new Dictionary<string, object> { { "status", 1 } };
        var result = _engine.Evaluate("{{IF(status, active, inactive)}}", ctx);
        Assert.Equal("active", result);
    }

    [Fact]
    public void Evaluate_IF_FalseCondition_Zero()
    {
        var ctx = new RenderContext();
        ctx.CurrentRow = new Dictionary<string, object> { { "status", 0 } };
        var result = _engine.Evaluate("{{IF(status, active, inactive)}}", ctx);
        Assert.Equal("inactive", result);
    }

    [Fact]
    public void Evaluate_IF_TrueCondition_TrueString()
    {
        var ctx = new RenderContext();
        ctx.CurrentRow = new Dictionary<string, object> { { "flag", "true" } };
        var result = _engine.Evaluate("{{IF(flag, yes, no)}}", ctx);
        Assert.Equal("yes", result);
    }

    [Fact]
    public void Evaluate_IF_FalseCondition_FalseString()
    {
        var ctx = new RenderContext();
        ctx.CurrentRow = new Dictionary<string, object> { { "flag", "false" } };
        var result = _engine.Evaluate("{{IF(flag, yes, no)}}", ctx);
        Assert.Equal("no", result);
    }

    [Fact]
    public void Evaluate_IF_WithFieldReference()
    {
        var ctx = new RenderContext();
        ctx.CurrentRow = new Dictionary<string, object> { { "qty", 100 } };
        var result = _engine.Evaluate("数量: {{qty}}, 状态: {{IF(qty, 充足, 不足)}}", ctx);
        Assert.Contains("100", result);
        Assert.Contains("充足", result);
    }
}
