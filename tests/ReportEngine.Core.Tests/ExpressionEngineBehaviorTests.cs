using System;
using System.Collections.Generic;
using ReportEngine.Core.Data;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ExpressionEngine 行为测试：
///   - 系统变量（PAGE, TOTAL_PAGES, NOW, REPORT_DATE, ROW_NUMBER）
///   - 字段引用（currentRow.field, dataSource.field）
///   - 聚合函数（SUM, AVG, COUNT, MIN, MAX）
///   - 条件表达式（IF）
///   - 格式化（date, datetime, currency, percent, number）
///   - 边界情况
/// </summary>
public class ExpressionEngineBehaviorTests
{
    private readonly ExpressionEngine _engine = new ExpressionEngine();

    // ============== 系统变量 ==============

    [Fact]
    public void Evaluate_PAGE_ReturnsCurrentPage()
    {
        var ctx = new RenderContext { CurrentPage = 5 };
        var result = _engine.Evaluate("Page {{PAGE}}", ctx);
        Assert.Equal("Page 5", result);
    }

    [Fact]
    public void Evaluate_TOTAL_PAGES_ReturnsTotalPages()
    {
        var ctx = new RenderContext { TotalPages = 10 };
        var result = _engine.Evaluate("Total {{TOTAL_PAGES}}", ctx);
        Assert.Equal("Total 10", result);
    }

    [Fact]
    public void Evaluate_ROW_NUMBER_ReturnsCurrentRowNumber()
    {
        var ctx = new RenderContext { CurrentRowNumber = 42 };
        var result = _engine.Evaluate("Row {{ROW_NUMBER}}", ctx);
        Assert.Equal("Row 42", result);
    }

    [Fact]
    public void Evaluate_NOW_ReturnsDateTime()
    {
        var ctx = new RenderContext();
        var result = _engine.Evaluate("{{NOW}}", ctx);
        Assert.NotEmpty(result);
        // NOW returns DateTime.Now, which should be parseable
        Assert.True(DateTime.TryParse(result, out _));
    }

    [Fact]
    public void Evaluate_REPORT_DATE_ReturnsFormattedDate()
    {
        var ctx = new RenderContext();
        var result = _engine.Evaluate("{{REPORT_DATE}}", ctx);
        Assert.NotEmpty(result);
        // REPORT_DATE returns yyyy-MM-dd format
        Assert.Matches(@"^\d{4}-\d{2}-\d{2}$", result);
    }

    [Fact]
    public void Evaluate_SystemVariables_CaseInsensitive()
    {
        var ctx = new RenderContext { CurrentPage = 3 };
        Assert.Equal("3", _engine.Evaluate("{{page}}", ctx));
        Assert.Equal("3", _engine.Evaluate("{{Page}}", ctx));
        Assert.Equal("3", _engine.Evaluate("{{PAGE}}", ctx));
    }

    // ============== 字段引用 ==============

    [Fact]
    public void Evaluate_CurrentRowField_ReturnsValue()
    {
        var ctx = new RenderContext
        {
            CurrentRow = new Dictionary<string, object>
            {
                { "name", "Alice" },
                { "age", 30 }
            }
        };
        Assert.Equal("Alice", _engine.Evaluate("{{currentRow.name}}", ctx));
        Assert.Equal("30", _engine.Evaluate("{{currentRow.age}}", ctx));
    }

    [Fact]
    public void Evaluate_CurrentRowField_NotFound_ReturnsOriginal()
    {
        var ctx = new RenderContext
        {
            CurrentRow = new Dictionary<string, object>
            {
                { "name", "Alice" }
            }
        };
        var result = _engine.Evaluate("{{currentRow.missing}}", ctx);
        Assert.Equal("{{currentRow.missing}}", result);
    }

    [Fact]
    public void Evaluate_SimpleField_ReturnsValue()
    {
        var ctx = new RenderContext
        {
            CurrentRow = new Dictionary<string, object>
            {
                { "name", "Bob" }
            }
        };
        Assert.Equal("Bob", _engine.Evaluate("{{name}}", ctx));
    }

    [Fact]
    public void Evaluate_SimpleField_NotFound_ReturnsOriginal()
    {
        var ctx = new RenderContext
        {
            CurrentRow = new Dictionary<string, object>()
        };
        var result = _engine.Evaluate("{{missing}}", ctx);
        Assert.Equal("missing", result);
    }

    // ============== 聚合函数 ==============

    [Fact]
    public void Evaluate_SUM_ReturnsSumOfField()
    {
        var ctx = new RenderContext
        {
            DataSourceName = "orders",
            DataSources =
            {
                ["orders"] = new List<Dictionary<string, object>>
                {
                    new() { { "amount", 100m } },
                    new() { { "amount", 200m } },
                    new() { { "amount", 150m } }
                }
            }
        };
        var result = _engine.Evaluate("{{SUM(amount)}}", ctx);
        Assert.Equal("450", result);
    }

    [Fact]
    public void Evaluate_AVG_ReturnsAverageOfField()
    {
        var ctx = new RenderContext
        {
            DataSourceName = "orders",
            DataSources =
            {
                ["orders"] = new List<Dictionary<string, object>>
                {
                    new() { { "amount", 100m } },
                    new() { { "amount", 200m } },
                    new() { { "amount", 300m } }
                }
            }
        };
        var result = _engine.Evaluate("{{AVG(amount)}}", ctx);
        Assert.Equal("200", result);
    }

    [Fact]
    public void Evaluate_COUNT_ReturnsCountOfRows()
    {
        var ctx = new RenderContext
        {
            DataSourceName = "orders",
            DataSources =
            {
                ["orders"] = new List<Dictionary<string, object>>
                {
                    new() { { "id", 1 } },
                    new() { { "id", 2 } },
                    new() { { "id", 3 } },
                    new() { { "id", 4 } }
                }
            }
        };
        var result = _engine.Evaluate("{{COUNT(id)}}", ctx);
        Assert.Equal("4", result);
    }

    [Fact]
    public void Evaluate_MIN_ReturnsMinValue()
    {
        var ctx = new RenderContext
        {
            DataSourceName = "orders",
            DataSources =
            {
                ["orders"] = new List<Dictionary<string, object>>
                {
                    new() { { "price", 50m } },
                    new() { { "price", 30m } },
                    new() { { "price", 80m } }
                }
            }
        };
        var result = _engine.Evaluate("{{MIN(price)}}", ctx);
        Assert.Equal("30", result);
    }

    [Fact]
    public void Evaluate_MAX_ReturnsMaxValue()
    {
        var ctx = new RenderContext
        {
            DataSourceName = "orders",
            DataSources =
            {
                ["orders"] = new List<Dictionary<string, object>>
                {
                    new() { { "price", 50m } },
                    new() { { "price", 30m } },
                    new() { { "price", 80m } }
                }
            }
        };
        var result = _engine.Evaluate("{{MAX(price)}}", ctx);
        Assert.Equal("80", result);
    }

    [Fact]
    public void Evaluate_AggregateFunction_CaseInsensitive()
    {
        var ctx = new RenderContext
        {
            DataSourceName = "orders",
            DataSources =
            {
                ["orders"] = new List<Dictionary<string, object>>
                {
                    new() { { "amount", 100m } },
                    new() { { "amount", 200m } }
                }
            }
        };
        Assert.Equal("300", _engine.Evaluate("{{sum(amount)}}", ctx));
        Assert.Equal("300", _engine.Evaluate("{{Sum(amount)}}", ctx));
        Assert.Equal("300", _engine.Evaluate("{{SUM(amount)}}", ctx));
    }

    [Fact]
    public void Evaluate_Aggregate_EmptyDataSource_ReturnsZero()
    {
        var ctx = new RenderContext
        {
            DataSourceName = "orders",
            DataSources =
            {
                ["orders"] = new List<Dictionary<string, object>>()
            }
        };
        Assert.Equal("0", _engine.Evaluate("{{SUM(amount)}}", ctx));
        Assert.Equal("0", _engine.Evaluate("{{COUNT(id)}}", ctx));
    }

    // ============== 条件表达式 ==============

    [Fact]
    public void Evaluate_IF_TrueCondition_ReturnsTrueValue()
    {
        var ctx = new RenderContext
        {
            CurrentRow = new Dictionary<string, object>
            {
                { "status", true }
            }
        };
        var result = _engine.Evaluate("{{IF(status, YES, NO)}}", ctx);
        Assert.Equal("YES", result);
    }

    [Fact]
    public void Evaluate_IF_FalseCondition_ReturnsFalseValue()
    {
        var ctx = new RenderContext
        {
            CurrentRow = new Dictionary<string, object>
            {
                { "status", false }
            }
        };
        var result = _engine.Evaluate("{{IF(status, YES, NO)}}", ctx);
        Assert.Equal("NO", result);
    }

    [Fact]
    public void Evaluate_IF_NumericCondition_TrueWhenNonZero()
    {
        var ctx = new RenderContext
        {
            CurrentRow = new Dictionary<string, object>
            {
                { "count", 5 }
            }
        };
        var result = _engine.Evaluate("{{IF(count, many, none)}}", ctx);
        Assert.Equal("many", result);
    }

    [Fact]
    public void Evaluate_IF_NumericCondition_FalseWhenZero()
    {
        var ctx = new RenderContext
        {
            CurrentRow = new Dictionary<string, object>
            {
                { "count", 0 }
            }
        };
        var result = _engine.Evaluate("{{IF(count, many, none)}}", ctx);
        Assert.Equal("none", result);
    }

    // ============== 格式化 ==============

    [Fact]
    public void Evaluate_DateFormat_ReturnsFormattedDate()
    {
        var ctx = new RenderContext
        {
            CurrentRow = new Dictionary<string, object>
            {
                { "orderDate", new DateTime(2024, 3, 15) }
            },
            FieldFormat = "date"
        };
        var result = _engine.Evaluate("{{orderDate}}", ctx);
        Assert.Equal("2024-03-15", result);
    }

    [Fact]
    public void Evaluate_DateTimeFormat_ReturnsFormattedDateTime()
    {
        var ctx = new RenderContext
        {
            CurrentRow = new Dictionary<string, object>
            {
                { "createdAt", new DateTime(2024, 3, 15, 14, 30, 45) }
            },
            FieldFormat = "datetime"
        };
        var result = _engine.Evaluate("{{createdAt}}", ctx);
        Assert.Equal("2024-03-15 14:30:45", result);
    }

    [Fact]
    public void Evaluate_CurrencyFormat_ReturnsFormattedCurrency()
    {
        var ctx = new RenderContext
        {
            CurrentRow = new Dictionary<string, object>
            {
                { "amount", 1234.56m }
            },
            FieldFormat = "currency"
        };
        var result = _engine.Evaluate("{{amount}}", ctx);
        Assert.Contains("1,234.56", result); // Currency format includes symbol and separators
    }

    [Fact]
    public void Evaluate_PercentFormat_ReturnsFormattedPercent()
    {
        var ctx = new RenderContext
        {
            CurrentRow = new Dictionary<string, object>
            {
                { "rate", 0.856m }
            },
            FieldFormat = "percent"
        };
        var result = _engine.Evaluate("{{rate}}", ctx);
        Assert.Equal("85.6%", result);
    }

    [Fact]
    public void Evaluate_NumberFormat0_ReturnsInteger()
    {
        var ctx = new RenderContext
        {
            CurrentRow = new Dictionary<string, object>
            {
                { "quantity", 123.789m }
            },
            FieldFormat = "number:0"
        };
        var result = _engine.Evaluate("{{quantity}}", ctx);
        Assert.Equal("124", result);
    }

    [Fact]
    public void Evaluate_NumberFormat1_ReturnsOneDecimal()
    {
        var ctx = new RenderContext
        {
            CurrentRow = new Dictionary<string, object>
            {
                { "quantity", 123.789m }
            },
            FieldFormat = "number:1"
        };
        var result = _engine.Evaluate("{{quantity}}", ctx);
        Assert.Equal("123.8", result);
    }

    [Fact]
    public void Evaluate_NumberFormat2_ReturnsTwoDecimals()
    {
        var ctx = new RenderContext
        {
            CurrentRow = new Dictionary<string, object>
            {
                { "quantity", 123.789m }
            },
            FieldFormat = "number:2"
        };
        var result = _engine.Evaluate("{{quantity}}", ctx);
        Assert.Equal("123.79", result);
    }

    // ============== 多表达式 ==============

    [Fact]
    public void Evaluate_MultiplePlaceholders_AllReplaced()
    {
        var ctx = new RenderContext
        {
            CurrentPage = 2,
            TotalPages = 10,
            CurrentRow = new Dictionary<string, object>
            {
                { "name", "Alice" }
            }
        };
        var result = _engine.Evaluate("{{name}} - Page {{PAGE}} of {{TOTAL_PAGES}}", ctx);
        Assert.Equal("Alice - Page 2 of 10", result);
    }

    [Fact]
    public void Evaluate_NoPlaceholders_ReturnsOriginal()
    {
        var ctx = new RenderContext();
        var result = _engine.Evaluate("Hello World", ctx);
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void Evaluate_EmptyText_ReturnsEmpty()
    {
        var ctx = new RenderContext();
        var result = _engine.Evaluate("", ctx);
        Assert.Equal("", result);
    }

    // ============== 边界情况 ==============

    [Fact]
    public void Evaluate_NullCurrentRow_HandlesGracefully()
    {
        var ctx = new RenderContext { CurrentRow = null };
        var result = _engine.Evaluate("{{name}}", ctx);
        Assert.Equal("name", result);
    }

    [Fact]
    public void Evaluate_WhitespaceInExpression_Trimmed()
    {
        var ctx = new RenderContext { CurrentPage = 5 };
        var result = _engine.Evaluate("{{ PAGE }}", ctx);
        Assert.Equal("5", result);
    }

    [Fact]
    public void Evaluate_IntegerValue_ConvertedToString()
    {
        var ctx = new RenderContext
        {
            CurrentRow = new Dictionary<string, object>
            {
                { "count", 42 }
            }
        };
        var result = _engine.Evaluate("{{count}}", ctx);
        Assert.Equal("42", result);
    }

    [Fact]
    public void Evaluate_DoubleValue_ConvertedToString()
    {
        var ctx = new RenderContext
        {
            CurrentRow = new Dictionary<string, object>
            {
                { "price", 19.99 }
            }
        };
        var result = _engine.Evaluate("{{price}}", ctx);
        Assert.Equal("19.99", result);
    }

    [Fact]
    public void Evaluate_NullValue_ReturnsEmpty()
    {
        var ctx = new RenderContext
        {
            CurrentRow = new Dictionary<string, object>
            {
                { "value", null! }
            }
        };
        var result = _engine.Evaluate("{{value}}", ctx);
        Assert.Equal("", result);
    }

    [Fact]
    public void Evaluate_BooleanValue_ConvertedToString()
    {
        var ctx = new RenderContext
        {
            CurrentRow = new Dictionary<string, object>
            {
                { "active", true },
                { "deleted", false }
            }
        };
        Assert.Equal("True", _engine.Evaluate("{{active}}", ctx));
        Assert.Equal("False", _engine.Evaluate("{{deleted}}", ctx));
    }
}
