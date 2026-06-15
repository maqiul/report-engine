using ReportEngine.Core.Data;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ExpressionEngine 高级行为测试
/// </summary>
public class ExpressionEngineAdvancedTests
{
    // ============== 系统变量 ==============

    [Fact]
    public void Evaluate_PAGE_ReturnsCurrentPage()
    {
        var engine = new ExpressionEngine();
        var ctx = new RenderContext { CurrentPage = 3 };
        var result = engine.Evaluate("Page {{PAGE}}", ctx);
        Assert.Equal("Page 3", result);
    }

    [Fact]
    public void Evaluate_TOTAL_PAGES_ReturnsTotalPages()
    {
        var engine = new ExpressionEngine();
        var ctx = new RenderContext { TotalPages = 10 };
        var result = engine.Evaluate("Total {{TOTAL_PAGES}}", ctx);
        Assert.Equal("Total 10", result);
    }

    [Fact]
    public void Evaluate_NOW_ReturnsDateTime()
    {
        var engine = new ExpressionEngine();
        var ctx = new RenderContext();
        var result = engine.Evaluate("{{NOW}}", ctx);
        Assert.NotEmpty(result);
        Assert.NotEqual("{{NOW}}", result);
    }

    [Fact]
    public void Evaluate_REPORT_DATE_ReturnsFormattedDate()
    {
        var engine = new ExpressionEngine();
        var ctx = new RenderContext();
        var result = engine.Evaluate("{{REPORT_DATE}}", ctx);
        Assert.Matches(@"\d{4}-\d{2}-\d{2}", result);
    }

    [Fact]
    public void Evaluate_ROW_NUMBER_ReturnsCurrentRowNumber()
    {
        var engine = new ExpressionEngine();
        var ctx = new RenderContext { CurrentRowNumber = 5 };
        var result = engine.Evaluate("Row {{ROW_NUMBER}}", ctx);
        Assert.Equal("Row 5", result);
    }

    // ============== 字段引用 ==============

    [Fact]
    public void Evaluate_CurrentRowField_Works()
    {
        var engine = new ExpressionEngine();
        var ctx = new RenderContext
        {
            CurrentRow = new Dictionary<string, object> { ["name"] = "Alice" }
        };
        var result = engine.Evaluate("Hello {{currentRow.name}}", ctx);
        Assert.Equal("Hello Alice", result);
    }

    [Fact]
    public void Evaluate_MissingCurrentRowField_ReturnsOriginal()
    {
        var engine = new ExpressionEngine();
        var ctx = new RenderContext
        {
            CurrentRow = new Dictionary<string, object> { ["name"] = "Alice" }
        };
        var result = engine.Evaluate("{{currentRow.missing}}", ctx);
        Assert.Equal("{{currentRow.missing}}", result);
    }

    [Fact]
    public void Evaluate_NullCurrentRow_ReturnsOriginal()
    {
        var engine = new ExpressionEngine();
        var ctx = new RenderContext { CurrentRow = null };
        var result = engine.Evaluate("{{currentRow.name}}", ctx);
        Assert.Equal("{{currentRow.name}}", result);
    }

    // ============== 聚合函数 ==============

    [Fact]
    public void Evaluate_SUM_Works()
    {
        var engine = new ExpressionEngine();
        var ctx = new RenderContext
        {
            DataSourceName = "orders",
            DataSources = { ["orders"] = new List<Dictionary<string, object>>
            {
                new() { ["amount"] = 100 },
                new() { ["amount"] = 200 },
                new() { ["amount"] = 300 }
            }}
        };
        var result = engine.Evaluate("Total: {{SUM(amount)}}", ctx);
        Assert.Equal("Total: 600", result);
    }

    [Fact]
    public void Evaluate_AVG_Works()
    {
        var engine = new ExpressionEngine();
        var ctx = new RenderContext
        {
            DataSourceName = "orders",
            DataSources = { ["orders"] = new List<Dictionary<string, object>>
            {
                new() { ["amount"] = 100 },
                new() { ["amount"] = 200 },
                new() { ["amount"] = 300 }
            }}
        };
        var result = engine.Evaluate("Avg: {{AVG(amount)}}", ctx);
        Assert.Equal("Avg: 200", result);
    }

    [Fact]
    public void Evaluate_COUNT_Works()
    {
        var engine = new ExpressionEngine();
        var ctx = new RenderContext
        {
            DataSourceName = "orders",
            DataSources = { ["orders"] = new List<Dictionary<string, object>>
            {
                new() { ["id"] = 1 },
                new() { ["id"] = 2 },
                new() { ["id"] = 3 }
            }}
        };
        var result = engine.Evaluate("Count: {{COUNT(id)}}", ctx);
        Assert.Equal("Count: 3", result);
    }

    [Fact]
    public void Evaluate_MIN_Works()
    {
        var engine = new ExpressionEngine();
        var ctx = new RenderContext
        {
            DataSourceName = "orders",
            DataSources = { ["orders"] = new List<Dictionary<string, object>>
            {
                new() { ["amount"] = 100 },
                new() { ["amount"] = 50 },
                new() { ["amount"] = 200 }
            }}
        };
        var result = engine.Evaluate("Min: {{MIN(amount)}}", ctx);
        Assert.Equal("Min: 50", result);
    }

    [Fact]
    public void Evaluate_MAX_Works()
    {
        var engine = new ExpressionEngine();
        var ctx = new RenderContext
        {
            DataSourceName = "orders",
            DataSources = { ["orders"] = new List<Dictionary<string, object>>
            {
                new() { ["amount"] = 100 },
                new() { ["amount"] = 300 },
                new() { ["amount"] = 200 }
            }}
        };
        var result = engine.Evaluate("Max: {{MAX(amount)}}", ctx);
        Assert.Equal("Max: 300", result);
    }

    // ============== 条件表达式 ==============

    [Fact]
    public void Evaluate_IF_TrueCondition_Works()
    {
        var engine = new ExpressionEngine();
        var ctx = new RenderContext
        {
            CurrentRow = new Dictionary<string, object> { ["active"] = "true" }
        };
        var result = engine.Evaluate("{{IF(active,YES,NO)}}", ctx);
        Assert.Equal("YES", result);
    }

    [Fact]
    public void Evaluate_IF_FalseCondition_Works()
    {
        var engine = new ExpressionEngine();
        var ctx = new RenderContext
        {
            CurrentRow = new Dictionary<string, object> { ["active"] = "false" }
        };
        var result = engine.Evaluate("{{IF(active,YES,NO)}}", ctx);
        Assert.Equal("NO", result);
    }

    // ============== 多表达式 ==============

    [Fact]
    public void Evaluate_MultipleExpressions_Works()
    {
        var engine = new ExpressionEngine();
        var ctx = new RenderContext
        {
            CurrentPage = 2,
            TotalPages = 5,
            CurrentRow = new Dictionary<string, object> { ["name"] = "Test" }
        };
        var result = engine.Evaluate("{{currentRow.name}} - Page {{PAGE}} of {{TOTAL_PAGES}}", ctx);
        Assert.Equal("Test - Page 2 of 5", result);
    }

    [Fact]
    public void Evaluate_NoExpressions_ReturnsOriginal()
    {
        var engine = new ExpressionEngine();
        var ctx = new RenderContext();
        var result = engine.Evaluate("Plain text without expressions", ctx);
        Assert.Equal("Plain text without expressions", result);
    }

    [Fact]
    public void Evaluate_EmptyText_ReturnsEmpty()
    {
        var engine = new ExpressionEngine();
        var ctx = new RenderContext();
        var result = engine.Evaluate("", ctx);
        Assert.Equal("", result);
    }

    // ============== 边界情况 ==============

    [Fact]
    public void Evaluate_UnmatchedExpression_ReturnsOriginal()
    {
        var engine = new ExpressionEngine();
        var ctx = new RenderContext();
        var result = engine.Evaluate("{{unknown}}", ctx);
        Assert.Equal("unknown", result);
    }

    [Fact]
    public void Evaluate_WhitespaceInExpression_Works()
    {
        var engine = new ExpressionEngine();
        var ctx = new RenderContext { CurrentPage = 1 };
        var result = engine.Evaluate("{{ PAGE }}", ctx);
        Assert.Equal("1", result);
    }

    [Fact]
    public void Evaluate_CaseInsensitiveSystemVars_Works()
    {
        var engine = new ExpressionEngine();
        var ctx = new RenderContext { CurrentPage = 7 };
        Assert.Equal("7", engine.Evaluate("{{page}}", ctx));
        Assert.Equal("7", engine.Evaluate("{{Page}}", ctx));
        Assert.Equal("7", engine.Evaluate("{{PAGE}}", ctx));
    }

    [Fact]
    public void Evaluate_CaseInsensitiveAggregates_Works()
    {
        var engine = new ExpressionEngine();
        var ctx = new RenderContext
        {
            DataSourceName = "data",
            DataSources = { ["data"] = new List<Dictionary<string, object>>
            {
                new() { ["val"] = 10 },
                new() { ["val"] = 20 }
            }}
        };
        Assert.Equal("30", engine.Evaluate("{{sum(val)}}", ctx));
        Assert.Equal("30", engine.Evaluate("{{Sum(val)}}", ctx));
        Assert.Equal("30", engine.Evaluate("{{SUM(val)}}", ctx));
    }
}
