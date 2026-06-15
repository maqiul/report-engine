using ReportEngine.Core;
using ReportEngine.Core.Barcodes;
using ReportEngine.Core.Data;
using ReportEngine.Core.Rendering;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// Schema 遗漏属性 + 计算属性 + ToString 测试
/// </summary>
public class SchemaGapFinalTests
{
    // ============== ReportTemplate 时间戳 ==============

    [Fact]
    public void ReportTemplate_CreatedAt_DefaultIsNow()
    {
        var t = new ReportTemplate();
        Assert.True((DateTime.Now - t.CreatedAt).TotalSeconds < 5);
    }

    [Fact]
    public void ReportTemplate_ModifiedAt_DefaultIsNow()
    {
        var t = new ReportTemplate();
        Assert.True((DateTime.Now - t.ModifiedAt).TotalSeconds < 5);
    }

    [Fact]
    public void ReportTemplate_CreatedAt_Settable()
    {
        var t = new ReportTemplate();
        var dt = new DateTime(2025, 6, 15);
        t.CreatedAt = dt;
        Assert.Equal(dt, t.CreatedAt);
    }

    [Fact]
    public void ReportTemplate_ModifiedAt_Settable()
    {
        var t = new ReportTemplate();
        var dt = new DateTime(2025, 6, 15);
        t.ModifiedAt = dt;
        Assert.Equal(dt, t.ModifiedAt);
    }

    // ============== TextElement.BoxType 计算属性 ==============

    [Fact]
    public void TextElement_BoxType_DefaultIsStatic()
    {
        var el = new TextElement();
        Assert.Equal(TextBoxType.Static, el.BoxType);
    }

    [Fact]
    public void TextElement_BoxType_WithDataField_IsField()
    {
        var el = new TextElement { DataField = "name" };
        Assert.Equal(TextBoxType.Field, el.BoxType);
    }

    [Fact]
    public void TextElement_BoxType_WithSummaryFunction_IsSummary()
    {
        var el = new TextElement { SummaryFunction = "Sum" };
        Assert.Equal(TextBoxType.Summary, el.BoxType);
    }

    [Fact]
    public void TextElement_BoxType_WithSystemVariable_IsSysVar()
    {
        var el = new TextElement { SystemVariable = "PageNumber" };
        Assert.Equal(TextBoxType.SysVar, el.BoxType);
    }

    [Fact]
    public void TextElement_BoxType_SysVarTakesPrecedenceOverSummary()
    {
        var el = new TextElement { SystemVariable = "PageNumber", SummaryFunction = "Sum", DataField = "name" };
        Assert.Equal(TextBoxType.SysVar, el.BoxType);
    }

    [Fact]
    public void TextElement_BoxType_SummaryTakesPrecedenceOverField()
    {
        var el = new TextElement { SummaryFunction = "Sum", DataField = "name" };
        Assert.Equal(TextBoxType.Summary, el.BoxType);
    }

    // ============== TextElement.SummaryFunction ==============

    [Fact]
    public void TextElement_SummaryFunction_DefaultNull()
    {
        var el = new TextElement();
        Assert.Null(el.SummaryFunction);
    }

    [Fact]
    public void TextElement_SummaryFunction_SetSum()
    {
        var el = new TextElement { SummaryFunction = "Sum" };
        Assert.Equal("Sum", el.SummaryFunction);
    }

    [Fact]
    public void TextElement_SummaryFunction_SetCount()
    {
        var el = new TextElement { SummaryFunction = "Count" };
        Assert.Equal("Count", el.SummaryFunction);
    }

    [Fact]
    public void TextElement_SummaryFunction_SetAvg()
    {
        var el = new TextElement { SummaryFunction = "Avg" };
        Assert.Equal("Avg", el.SummaryFunction);
    }

    [Fact]
    public void TextElement_SummaryFunction_SetMax()
    {
        var el = new TextElement { SummaryFunction = "Max" };
        Assert.Equal("Max", el.SummaryFunction);
    }

    [Fact]
    public void TextElement_SummaryFunction_SetMin()
    {
        var el = new TextElement { SummaryFunction = "Min" };
        Assert.Equal("Min", el.SummaryFunction);
    }

    // ============== ConditionalFormatRule.FontColor ==============

    [Fact]
    public void ConditionalFormatRule_FontColor_DefaultNull()
    {
        var r = new ConditionalFormatRule();
        Assert.Null(r.FontColor);
    }

    [Fact]
    public void ConditionalFormatRule_FontColor_Settable()
    {
        var r = new ConditionalFormatRule { FontColor = "#FF0000" };
        Assert.Equal("#FF0000", r.FontColor);
    }

    // ============== BarcodeElement 颜色默认值 ==============

    [Fact]
    public void BarcodeElement_ForeColor_DefaultIsBlack()
    {
        var el = new BarcodeElement();
        Assert.Equal("#000000", el.ForeColor);
    }

    [Fact]
    public void BarcodeElement_BackColor_DefaultIsWhite()
    {
        var el = new BarcodeElement();
        Assert.Equal("#FFFFFF", el.BackColor);
    }

    [Fact]
    public void BarcodeElement_ShowText_DefaultTrue()
    {
        var el = new BarcodeElement();
        Assert.True(el.ShowText);
    }

    // ============== MultiUpConfig.Count 计算属性 ==============

    [Fact]
    public void MultiUpConfig_Count_DefaultIs4()
    {
        var mu = new MultiUpConfig();
        Assert.Equal(4, mu.Count); // 2 * 2
    }

    [Fact]
    public void MultiUpConfig_Count_Rows3Cols2_Is6()
    {
        var mu = new MultiUpConfig { Rows = 3, Columns = 2 };
        Assert.Equal(6, mu.Count);
    }

    [Fact]
    public void MultiUpConfig_Count_Rows1Cols1_Is1()
    {
        var mu = new MultiUpConfig { Rows = 1, Columns = 1 };
        Assert.Equal(1, mu.Count);
    }

    // ============== DataSourceDef.ToString() ==============

    [Fact]
    public void DataSourceDef_ToString_ContainsNameAndType()
    {
        var ds = new DataSourceDef { Name = "orders", Type = "sql" };
        var s = ds.ToString();
        Assert.Contains("orders", s);
        Assert.Contains("sql", s);
    }

    [Fact]
    public void DataSourceDef_ToString_ContainsFieldCount()
    {
        var ds = new DataSourceDef { Name = "ds", Type = "json" };
        ds.Fields.Add(new FieldDef { Name = "id" });
        ds.Fields.Add(new FieldDef { Name = "name" });
        var s = ds.ToString();
        Assert.Contains("2", s);
    }

    // ============== FieldDef.ToString() ==============

    [Fact]
    public void FieldDef_ToString_ContainsNameAndType()
    {
        var f = new FieldDef { Name = "amount", Type = "number" };
        var s = f.ToString();
        Assert.Contains("amount", s);
        Assert.Contains("number", s);
    }

    [Fact]
    public void FieldDef_ToString_WithFormat_ContainsFormat()
    {
        var f = new FieldDef { Name = "price", Type = "number", Format = "currency" };
        var s = f.ToString();
        Assert.Contains("currency", s);
    }

    [Fact]
    public void FieldDef_ToString_WithoutFormat_NoFormatPart()
    {
        var f = new FieldDef { Name = "id", Type = "string" };
        var s = f.ToString();
        Assert.DoesNotContain("(", s);
    }

    // ============== RenderedElements 默认值 ==============

    [Fact]
    public void RenderedTextElement_DefaultValues()
    {
        var el = new RenderedTextElement();
        Assert.Equal("", el.Text);
        Assert.NotNull(el.Font);
        Assert.Equal(TextAlignment.Left, el.Alignment);
        Assert.Null(el.Hyperlink);
    }

    [Fact]
    public void RenderedImageElement_DefaultSource()
    {
        var el = new RenderedImageElement();
        Assert.Equal("", el.Source);
    }

    [Fact]
    public void RenderedLineElement_DefaultValues()
    {
        var el = new RenderedLineElement();
        Assert.Equal("#000000", el.LineColor);
        Assert.Equal(0, el.LineWidth);
    }

    [Fact]
    public void RenderedShapeElement_DefaultValues()
    {
        var el = new RenderedShapeElement();
        Assert.Equal("#FFFFFF", el.FillColor);
        Assert.Equal(0, el.BorderRadius);
    }

    [Fact]
    public void RenderedBarcodeElement_DefaultValues()
    {
        var el = new RenderedBarcodeElement();
        Assert.Equal("", el.Value);
        Assert.Equal("#000000", el.ForeColor);
        Assert.Equal("#FFFFFF", el.BackColor);
        Assert.True(el.ShowText);
    }

    [Fact]
    public void RenderedTableElement_DefaultValues()
    {
        var el = new RenderedTableElement();
        Assert.Equal("#000000", el.BorderColor);
        Assert.NotNull(el.ColumnWidths);
        Assert.NotNull(el.RowHeights);
        Assert.NotNull(el.Cells);
    }

    [Fact]
    public void RenderedTableCell_DefaultValues()
    {
        var cell = new RenderedTableCell();
        Assert.Equal("", cell.Text);
        Assert.Equal(1, cell.RowSpan);
        Assert.Equal(1, cell.ColSpan);
        Assert.Equal(TextAlignment.Center, cell.Alignment);
        Assert.NotNull(cell.Font);
        Assert.Null(cell.BackgroundColor);
    }

    [Fact]
    public void RenderedCrossTabElement_DefaultValues()
    {
        var el = new RenderedCrossTabElement();
        Assert.Equal("#000000", el.BorderColor);
        Assert.NotNull(el.ColumnWidths);
        Assert.NotNull(el.RowHeights);
        Assert.NotNull(el.Cells);
    }

    // ============== RenderedElement 基类默认值 ==============

    [Fact]
    public void RenderedElement_BaseDefaults()
    {
        var el = new RenderedTextElement();
        Assert.Equal("", el.Id);
        Assert.Equal(0, el.X);
        Assert.Equal(0, el.Y);
        Assert.Equal(0, el.Width);
        Assert.Equal(0, el.Height);
        Assert.Null(el.BackgroundColor);
        Assert.Null(el.Border);
    }

    // ============== 枚举完整性 ==============

    [Fact]
    public void TextAlignment_Has4Values()
    {
        var values = Enum.GetValues(typeof(TextAlignment));
        Assert.Equal(4, values.Length);
    }

    [Fact]
    public void TextBoxType_Has4Values()
    {
        var values = Enum.GetValues(typeof(TextBoxType));
        Assert.Equal(4, values.Length);
    }

    [Fact]
    public void LineDirection_Has3Values()
    {
        var values = Enum.GetValues(typeof(LineDirection));
        Assert.Equal(3, values.Length);
    }

    [Fact]
    public void ShapeType_Has4Values()
    {
        var values = Enum.GetValues(typeof(ShapeType));
        Assert.Equal(4, values.Length);
    }

    [Fact]
    public void ChartType_Has5Values()
    {
        var values = Enum.GetValues(typeof(ChartType));
        Assert.Equal(5, values.Length);
    }

    [Fact]
    public void BorderStyle_Has4Values()
    {
        var values = Enum.GetValues(typeof(BorderStyle));
        Assert.Equal(4, values.Length);
    }

    [Fact]
    public void ImageSizing_Has4Values()
    {
        var values = Enum.GetValues(typeof(ImageSizing));
        Assert.Equal(4, values.Length);
    }

    [Fact]
    public void BarcodeFormat_Has8Values()
    {
        var values = Enum.GetValues(typeof(BarcodeFormat));
        Assert.Equal(8, values.Length);
    }
}

/// <summary>
/// ExpressionEngine 聚合函数 + IF 测试
/// </summary>
public class ExpressionEngineAggregateTests
{
    private readonly ExpressionEngine _engine = new();

    private RenderContext CreateContextWithDataSource()
    {
        var ctx = new RenderContext { DataSourceName = "ds" };
        ctx.DataSources["ds"] = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "amount", 100 }, { "name", "A" } },
            new Dictionary<string, object> { { "amount", 200 }, { "name", "B" } },
            new Dictionary<string, object> { { "amount", 300 }, { "name", "C" } },
        };
        return ctx;
    }

    [Fact]
    public void Evaluate_Sum_ReturnsSum()
    {
        var ctx = CreateContextWithDataSource();
        var result = _engine.Evaluate("{{SUM(amount)}}", ctx);
        Assert.Equal("600", result);
    }

    [Fact]
    public void Evaluate_Avg_ReturnsAverage()
    {
        var ctx = CreateContextWithDataSource();
        var result = _engine.Evaluate("{{AVG(amount)}}", ctx);
        Assert.Equal("200", result);
    }

    [Fact]
    public void Evaluate_Count_ReturnsCount()
    {
        var ctx = CreateContextWithDataSource();
        var result = _engine.Evaluate("{{COUNT(amount)}}", ctx);
        Assert.Equal("3", result);
    }

    [Fact]
    public void Evaluate_Min_ReturnsMin()
    {
        var ctx = CreateContextWithDataSource();
        var result = _engine.Evaluate("{{MIN(amount)}}", ctx);
        Assert.Equal("100", result);
    }

    [Fact]
    public void Evaluate_Max_ReturnsMax()
    {
        var ctx = CreateContextWithDataSource();
        var result = _engine.Evaluate("{{MAX(amount)}}", ctx);
        Assert.Equal("300", result);
    }

    [Fact]
    public void Evaluate_Sum_NoDataSource_ReturnsZero()
    {
        var ctx = new RenderContext { DataSourceName = "nonexistent" };
        var result = _engine.Evaluate("{{SUM(amount)}}", ctx);
        Assert.Equal("0", result);
    }

    [Fact]
    public void Evaluate_IF_TrueCondition_ReturnsTrueValue()
    {
        var ctx = new RenderContext();
        var result = _engine.Evaluate("{{IF(true, yes, no)}}", ctx);
        Assert.Equal("yes", result);
    }

    [Fact]
    public void Evaluate_IF_FalseCondition_ReturnsFalseValue()
    {
        var ctx = new RenderContext();
        var result = _engine.Evaluate("{{IF(false, yes, no)}}", ctx);
        Assert.Equal("no", result);
    }

    [Fact]
    public void Evaluate_IF_WithFieldReference()
    {
        var row = new Dictionary<string, object> { { "status", true } };
        var ctx = new RenderContext { CurrentRow = row };
        var result = _engine.Evaluate("{{IF(currentRow.status, active, inactive)}}", ctx);
        Assert.Equal("active", result);
    }

    [Fact]
    public void Evaluate_IF_WithFieldReference_False()
    {
        var row = new Dictionary<string, object> { { "status", false } };
        var ctx = new RenderContext { CurrentRow = row };
        var result = _engine.Evaluate("{{IF(currentRow.status, active, inactive)}}", ctx);
        Assert.Equal("inactive", result);
    }

    // ============== FormatValue ==============

    [Fact]
    public void Evaluate_DecimalResult_FormatsCorrectly()
    {
        var ctx = CreateContextWithDataSource();
        var result = _engine.Evaluate("{{AVG(amount)}}", ctx);
        // 200.0 可能格式化为 "200" 或 "200.0"
        Assert.Contains("200", result);
    }

    [Fact]
    public void Evaluate_DateTimeResult_FormatsAsString()
    {
        var ctx = new RenderContext();
        var result = _engine.Evaluate("{{NOW}}", ctx);
        Assert.NotEmpty(result);
        // 应该是一个日期时间字符串
        Assert.NotEqual("NOW", result);
    }
}

/// <summary>
/// BarcodeGenerator 更多边界测试
/// </summary>
public class BarcodeGeneratorFinalTests
{
    [Fact]
    public void Generate_EmptyContent_ReturnsEmptyMatrix()
    {
        var result = BarcodeGenerator.Generate("", BarcodeFormat.QRCode, 100, 100);
        Assert.Equal(100, result.GetLength(0));
        Assert.Equal(100, result.GetLength(1));
        // 空内容应该全 false
        for (int y = 0; y < result.GetLength(0); y++)
            for (int x = 0; x < result.GetLength(1); x++)
                Assert.False(result[y, x]);
    }

    [Fact]
    public void Generate_NullContent_ReturnsEmptyMatrix()
    {
        var result = BarcodeGenerator.Generate(null!, BarcodeFormat.QRCode, 50, 50);
        Assert.Equal(50, result.GetLength(0));
        Assert.Equal(50, result.GetLength(1));
    }

    [Fact]
    public void Generate_QRCode_ProducesNonEmptyMatrix()
    {
        var result = BarcodeGenerator.Generate("Hello", BarcodeFormat.QRCode, 100, 100);
        Assert.True(result.GetLength(0) > 0);
        Assert.True(result.GetLength(1) > 0);
        // 至少有一些黑色模块
        bool hasBlack = false;
        for (int y = 0; y < result.GetLength(0) && !hasBlack; y++)
            for (int x = 0; x < result.GetLength(1) && !hasBlack; x++)
                if (result[y, x]) hasBlack = true;
        Assert.True(hasBlack);
    }

    [Fact]
    public void Generate_Code128_ProducesMatrix()
    {
        var result = BarcodeGenerator.Generate("ABC123", BarcodeFormat.Code128, 200, 50);
        Assert.True(result.GetLength(0) > 0);
        Assert.True(result.GetLength(1) > 0);
    }

    [Fact]
    public void Generate_Code39_ProducesMatrix()
    {
        var result = BarcodeGenerator.Generate("HELLO", BarcodeFormat.Code39, 200, 50);
        Assert.True(result.GetLength(0) > 0);
    }

    [Fact]
    public void Generate_DataMatrix_ProducesMatrix()
    {
        var result = BarcodeGenerator.Generate("Test", BarcodeFormat.DataMatrix, 100, 100);
        Assert.True(result.GetLength(0) > 0);
    }

    [Fact]
    public void Generate_AllFormats_ProduceMatrix()
    {
        var formats = new[]
        {
            BarcodeFormat.Code128, BarcodeFormat.Code39, BarcodeFormat.QRCode,
            BarcodeFormat.DataMatrix
        };
        foreach (var fmt in formats)
        {
            var result = BarcodeGenerator.Generate("Test", fmt, 100, 100);
            Assert.True(result.GetLength(0) > 0, $"Format {fmt} produced empty matrix");
            Assert.True(result.GetLength(1) > 0, $"Format {fmt} produced empty matrix");
        }
    }
}

/// <summary>
/// RenderContext 完整字段测试
/// </summary>
public class RenderContextFinalTests
{
    [Fact]
    public void RenderContext_DefaultDataSourceName_IsEmpty()
    {
        var ctx = new RenderContext();
        Assert.Equal("", ctx.DataSourceName);
    }

    [Fact]
    public void RenderContext_DefaultCurrentPage_IsOne()
    {
        var ctx = new RenderContext();
        Assert.Equal(1, ctx.CurrentPage);
    }

    [Fact]
    public void RenderContext_DefaultTotalPages_IsOne()
    {
        var ctx = new RenderContext();
        Assert.Equal(1, ctx.TotalPages);
    }

    [Fact]
    public void RenderContext_DefaultCurrentRowNumber_IsZero()
    {
        var ctx = new RenderContext();
        Assert.Equal(0, ctx.CurrentRowNumber);
    }

    [Fact]
    public void RenderContext_DefaultCurrentRow_IsNull()
    {
        var ctx = new RenderContext();
        Assert.Null(ctx.CurrentRow);
    }

    [Fact]
    public void RenderContext_DataSources_InitiallyEmpty()
    {
        var ctx = new RenderContext();
        Assert.NotNull(ctx.DataSources);
        Assert.Empty(ctx.DataSources);
    }

    [Fact]
    public void RenderContext_PageWidth_Default210()
    {
        var ctx = new RenderContext();
        Assert.Equal(210, ctx.PageWidth);
    }

    [Fact]
    public void RenderContext_PageHeight_Default297()
    {
        var ctx = new RenderContext();
        Assert.Equal(297, ctx.PageHeight);
    }

    [Fact]
    public void RenderContext_AllPropertiesSettable()
    {
        var ctx = new RenderContext
        {
            PageWidth = 297,
            PageHeight = 210,
            CurrentPage = 3,
            TotalPages = 10,
            CurrentRowNumber = 42,
            DataSourceName = "orders"
        };

        Assert.Equal(297, ctx.PageWidth);
        Assert.Equal(210, ctx.PageHeight);
        Assert.Equal(3, ctx.CurrentPage);
        Assert.Equal(10, ctx.TotalPages);
        Assert.Equal(42, ctx.CurrentRowNumber);
        Assert.Equal("orders", ctx.DataSourceName);
    }
}
