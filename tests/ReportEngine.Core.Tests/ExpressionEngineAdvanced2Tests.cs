using ReportEngine.Core.Data;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ExpressionEngine 高级测试
/// </summary>
public class ExpressionEngineExtendedTests
{
    private readonly ExpressionEngine _engine = new();

    private RenderContext CreateContext()
    {
        var ctx = new RenderContext
        {
            DataSourceName = "orders",
            CurrentPage = 3,
            TotalPages = 10,
            CurrentRowNumber = 5
        };
        ctx.DataSources["orders"] = new List<Dictionary<string, object>>
        {
            new() { ["Name"] = "Alice", ["Amount"] = 100m, ["Qty"] = 2 },
            new() { ["Name"] = "Bob", ["Amount"] = 200m, ["Qty"] = 3 },
            new() { ["Name"] = "Charlie", ["Amount"] = 150m, ["Qty"] = 1 }
        };
        ctx.CurrentRow = new Dictionary<string, object>
        {
            ["Name"] = "Bob",
            ["Amount"] = 200m,
            ["Qty"] = 3
        };
        return ctx;
    }

    // ============== 简单字段引用 ==============

    [Fact]
    public void Evaluate_CurrentRowField_ReturnsValue()
    {
        var ctx = CreateContext();
        var result = _engine.Evaluate("{{currentRow.Name}}", ctx);
        Assert.Equal("Bob", result);
    }

    [Fact]
    public void Evaluate_CurrentRowAmount_ReturnsDecimal()
    {
        var ctx = CreateContext();
        var result = _engine.Evaluate("{{currentRow.Amount}}", ctx);
        Assert.Equal("200", result);
    }

    [Fact]
    public void Evaluate_SimpleFieldName_ReturnsValue()
    {
        var ctx = CreateContext();
        var result = _engine.Evaluate("{{Name}}", ctx);
        Assert.Equal("Bob", result);
    }

    // ============== 系统变量 ==============

    [Fact]
    public void Evaluate_PAGE_ReturnsCurrentPage()
    {
        var ctx = CreateContext();
        var result = _engine.Evaluate("{{PAGE}}", ctx);
        Assert.Equal("3", result);
    }

    [Fact]
    public void Evaluate_TOTAL_PAGES_ReturnsTotalPages()
    {
        var ctx = CreateContext();
        var result = _engine.Evaluate("{{TOTAL_PAGES}}", ctx);
        Assert.Equal("10", result);
    }

    [Fact]
    public void Evaluate_ROW_NUMBER_ReturnsRowNumber()
    {
        var ctx = CreateContext();
        var result = _engine.Evaluate("{{ROW_NUMBER}}", ctx);
        Assert.Equal("5", result);
    }

    [Fact]
    public void Evaluate_REPORT_DATE_ReturnsDate()
    {
        var ctx = CreateContext();
        var result = _engine.Evaluate("{{REPORT_DATE}}", ctx);
        Assert.Contains("-", result);
        Assert.Equal(10, result.Length); // yyyy-MM-dd
    }

    [Fact]
    public void Evaluate_NOW_ReturnsDateTime()
    {
        var ctx = CreateContext();
        var result = _engine.Evaluate("{{NOW}}", ctx);
        Assert.NotEmpty(result);
    }

    // ============== 聚合函数 ==============

    [Fact]
    public void Evaluate_SUM_ReturnsSum()
    {
        var ctx = CreateContext();
        var result = _engine.Evaluate("{{SUM(Amount)}}", ctx);
        Assert.Equal("450", result);
    }

    [Fact]
    public void Evaluate_AVG_ReturnsAverage()
    {
        var ctx = CreateContext();
        var result = _engine.Evaluate("{{AVG(Amount)}}", ctx);
        Assert.Equal("150", result);
    }

    [Fact]
    public void Evaluate_COUNT_ReturnsCount()
    {
        var ctx = CreateContext();
        var result = _engine.Evaluate("{{COUNT(Amount)}}", ctx);
        Assert.Equal("3", result);
    }

    [Fact]
    public void Evaluate_MIN_ReturnsMin()
    {
        var ctx = CreateContext();
        var result = _engine.Evaluate("{{MIN(Amount)}}", ctx);
        Assert.Equal("100", result);
    }

    [Fact]
    public void Evaluate_MAX_ReturnsMax()
    {
        var ctx = CreateContext();
        var result = _engine.Evaluate("{{MAX(Amount)}}", ctx);
        Assert.Equal("200", result);
    }

    // ============== 多占位符 ==============

    [Fact]
    public void Evaluate_MultiplePlaceholders_AllReplaced()
    {
        var ctx = CreateContext();
        var result = _engine.Evaluate("Name: {{currentRow.Name}}, Amount: {{currentRow.Amount}}", ctx);
        Assert.Contains("Bob", result);
        Assert.Contains("200", result);
    }

    [Fact]
    public void Evaluate_NoPlaceholders_ReturnsOriginal()
    {
        var ctx = CreateContext();
        var result = _engine.Evaluate("Hello World", ctx);
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void Evaluate_EmptyString_ReturnsEmpty()
    {
        var ctx = CreateContext();
        var result = _engine.Evaluate("", ctx);
        Assert.Equal("", result);
    }

    // ============== 格式化 ==============

    [Fact]
    public void Evaluate_CurrencyFormat_Works()
    {
        var ctx = CreateContext();
        ctx.FieldFormat = "currency";
        var result = _engine.Evaluate("{{currentRow.Amount}}", ctx);
        Assert.Contains("¥", result);
    }

    [Fact]
    public void Evaluate_DateFormat_Works()
    {
        var ctx = CreateContext();
        ctx.FieldFormat = "date";
        ctx.CurrentRow["Date"] = new DateTime(2024, 6, 15);
        var result = _engine.Evaluate("{{currentRow.Date}}", ctx);
        Assert.Equal("2024-06-15", result);
    }

    [Fact]
    public void Evaluate_DateTimeFormat_Works()
    {
        var ctx = CreateContext();
        ctx.FieldFormat = "datetime";
        ctx.CurrentRow["Date"] = new DateTime(2024, 6, 15, 14, 30, 0);
        var result = _engine.Evaluate("{{currentRow.Date}}", ctx);
        Assert.Equal("2024-06-15 14:30:00", result);
    }

    // ============== 未知字段 ==============

    [Fact]
    public void Evaluate_UnknownField_ReturnsOriginalText()
    {
        var ctx = CreateContext();
        var result = _engine.Evaluate("{{unknownField}}", ctx);
        Assert.Equal("unknownField", result);
    }

    [Fact]
    public void Evaluate_UnknownCurrentRowField_ReturnsWrappedText()
    {
        var ctx = CreateContext();
        var result = _engine.Evaluate("{{currentRow.Missing}}", ctx);
        Assert.Contains("currentRow.Missing", result);
    }

    // ============== 数据源引用 ==============

    [Fact]
    public void Evaluate_DataSourceField_Works()
    {
        var ctx = CreateContext();
        var result = _engine.Evaluate("{{orders.Name}}", ctx);
        Assert.Equal("Bob", result);
    }

    // ============== 聚合函数无数据 ==============

    [Fact]
    public void Evaluate_SUM_NoDataSource_ReturnsZero()
    {
        var ctx = new RenderContext { DataSourceName = "nonexistent" };
        var result = _engine.Evaluate("{{SUM(Amount)}}", ctx);
        Assert.Equal("0", result);
    }

    // ============== 空 CurrentRow ==============

    [Fact]
    public void Evaluate_NullCurrentRow_FieldReturnsOriginal()
    {
        var ctx = new RenderContext();
        var result = _engine.Evaluate("{{Name}}", ctx);
        Assert.Equal("Name", result);
    }

    // ============== 综合场景 ==============

    [Fact]
    public void Evaluate_PageInfo_Works()
    {
        var ctx = CreateContext();
        var result = _engine.Evaluate("Page {{PAGE}} of {{TOTAL_PAGES}}", ctx);
        Assert.Equal("Page 3 of 10", result);
    }

    [Fact]
    public void Evaluate_MixedContent_Works()
    {
        var ctx = CreateContext();
        var result = _engine.Evaluate("Report: {{currentRow.Name}} - Total: {{SUM(Amount)}}", ctx);
        Assert.Contains("Bob", result);
        Assert.Contains("450", result);
    }

    [Fact]
    public void Evaluate_PercentFormat_Works()
    {
        var ctx = CreateContext();
        ctx.FieldFormat = "percent";
        ctx.CurrentRow["Rate"] = 0.856m;
        var result = _engine.Evaluate("{{currentRow.Rate}}", ctx);
        Assert.Contains("%", result);
    }

    [Fact]
    public void Evaluate_NumberFormat0_Works()
    {
        var ctx = CreateContext();
        ctx.FieldFormat = "number:0";
        var result = _engine.Evaluate("{{currentRow.Amount}}", ctx);
        Assert.Equal("200", result);
    }

    [Fact]
    public void Evaluate_NumberFormat2_Works()
    {
        var ctx = CreateContext();
        ctx.FieldFormat = "number:2";
        var result = _engine.Evaluate("{{currentRow.Amount}}", ctx);
        Assert.Equal("200.00", result);
    }
}
