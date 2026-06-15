using System;
using System.Collections.Generic;
using System.Linq;
using ReportEngine.Core.Data;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ExpressionEngine 表达式替换测试：
///   - {{Field}} 字段引用（currentRow + 数据源）
///   - {{SUM/AVG/COUNT/MIN/MAX(field)}} 聚合函数
///   - {{IF(cond, trueVal, falseVal)}} 条件表达式
///   - {{PAGE/TOTAL_PAGES/NOW/REPORT_DATE/ROW_NUMBER}} 系统变量
///   - 嵌套路径（a.b.c）
///   - 缺失字段返回原占位符
/// </summary>
public class ExpressionEngineTests
{
    private readonly ExpressionEngine _engine = new();

    private static RenderContext Ctx(Dictionary<string, object>? row = null,
        Dictionary<string, List<Dictionary<string, object>>>? dataSources = null,
        int page = 1, int total = 1, int rowNum = 0)
    {
        var ctx = new RenderContext
        {
            CurrentRow = row,
            CurrentPage = page,
            TotalPages = total,
            CurrentRowNumber = rowNum,
        };
        if (dataSources != null)
        {
            foreach (var kv in dataSources) ctx.DataSources.Add(kv.Key, kv.Value);
        }
        return ctx;
    }

    [Fact]
    public void Evaluate_PlainText_NoPlaceholders_Returned()
    {
        Assert.Equal("Hello World", _engine.Evaluate("Hello World", Ctx()));
    }

    [Fact]
    public void Evaluate_FieldReference_ResolvesToValue()
    {
        var row = new Dictionary<string, object> { { "Name", "Alice" } };
        Assert.Equal("Alice", _engine.Evaluate("{{Name}}", Ctx(row)));
    }

    [Fact]
    public void Evaluate_FieldReference_WithSurroundingText()
    {
        var row = new Dictionary<string, object> { { "Qty", 5 } };
        Assert.Equal("Q: 5 pcs", _engine.Evaluate("Q: {{Qty}} pcs", Ctx(row)));
    }

    [Fact]
    public void Evaluate_MissingField_ReturnsExpressionText()
    {
        // 字段未命中时原 EvaluateExpression 第 5 步 "未匹配到返回原文本" 行为
        var row = new Dictionary<string, object>();
        Assert.Equal("Unknown", _engine.Evaluate("{{Unknown}}", Ctx(row)));
    }

    [Fact]
    public void Evaluate_SystemVariable_PAGE_ReturnsCurrentPage()
    {
        Assert.Equal("3", _engine.Evaluate("{{PAGE}}", Ctx(page: 3)));
    }

    [Fact]
    public void Evaluate_SystemVariable_TOTAL_PAGES()
    {
        Assert.Equal("10", _engine.Evaluate("{{TOTAL_PAGES}}", Ctx(total: 10)));
    }

    [Fact]
    public void Evaluate_SystemVariable_REPORT_DATE_FormattedAsYyyyMmDd()
    {
        var result = _engine.Evaluate("{{REPORT_DATE}}", Ctx());
        Assert.Matches(@"^\d{4}-\d{2}-\d{2}$", result);
    }

    [Fact]
    public void Evaluate_SystemVariable_ROW_NUMBER()
    {
        Assert.Equal("7", _engine.Evaluate("{{ROW_NUMBER}}", Ctx(rowNum: 7)));
    }

    [Fact]
    public void Evaluate_Sum_AggregatesColumn()
    {
        var data = new List<Dictionary<string, object>>
        {
            new() { { "amount", 10 } },
            new() { { "amount", 20 } },
            new() { { "amount", 30 } },
        };
        var ctx = new RenderContext { CurrentRow = data[0], DataSourceName = "orders" };
        ctx.DataSources.Add("orders", data);
        Assert.Equal("60", _engine.Evaluate("{{SUM(amount)}}", ctx));
    }

    [Fact]
    public void Evaluate_Avg_AggregatesColumn()
    {
        var data = new List<Dictionary<string, object>>
        {
            new() { { "x", 10 } }, new() { { "x", 20 } }, new() { { "x", 30 } },
        };
        var ctx = new RenderContext { CurrentRow = data[0], DataSourceName = "d" };
        ctx.DataSources.Add("d", data);
        Assert.Equal("20", _engine.Evaluate("{{AVG(x)}}", ctx));
    }

    [Fact]
    public void Evaluate_Count_AggregatesColumn()
    {
        var data = new List<Dictionary<string, object>>
        {
            new() { { "k", 1 } }, new() { { "k", 2 } }, new() { { "k", 3 } },
        };
        var ctx = new RenderContext { CurrentRow = data[0], DataSourceName = "d" };
        ctx.DataSources.Add("d", data);
        Assert.Equal("3", _engine.Evaluate("{{COUNT(k)}}", ctx));
    }

    [Fact]
    public void Evaluate_Min_MaxAggregates()
    {
        var data = new List<Dictionary<string, object>>
        {
            new() { { "v", 5 } }, new() { { "v", 1 } }, new() { { "v", 9 } },
        };
        var ctx = new RenderContext { CurrentRow = data[0], DataSourceName = "d" };
        ctx.DataSources.Add("d", data);
        Assert.Equal("1", _engine.Evaluate("{{MIN(v)}}", ctx));
        Assert.Equal("9", _engine.Evaluate("{{MAX(v)}}", ctx));
    }

    [Fact]
    public void Evaluate_If_TrueBranch()
    {
        // IF 的 cond 解析为 boolean
        var row = new Dictionary<string, object> { { "status", true } };
        Assert.Equal("YES", _engine.Evaluate("{{IF(status,YES,NO)}}", Ctx(row)));
    }

    [Fact]
    public void Evaluate_If_FalseBranch()
    {
        var row = new Dictionary<string, object> { { "status", false } };
        Assert.Equal("NO", _engine.Evaluate("{{IF(status,YES,NO)}}", Ctx(row)));
    }

    [Fact]
    public void Evaluate_MultiplePlaceholders_AllReplaced()
    {
        var row = new Dictionary<string, object> { { "first", "A" }, { "last", "B" } };
        Assert.Equal("Hello A B!", _engine.Evaluate("Hello {{first}} {{last}}!", Ctx(row)));
    }

    [Fact]
    public void Evaluate_DataSourceField_Resolves()
    {
        var data = new List<Dictionary<string, object>> { new() { { "name", "X" } } };
        var ctx = new RenderContext { CurrentRow = data[0] };
        ctx.DataSources.Add("ds", data);
        Assert.Equal("X", _engine.Evaluate("{{ds.name}}", ctx));
    }

    [Fact]
    public void Evaluate_EmptyExpression_ReturnsEmpty()
    {
        Assert.Equal("", _engine.Evaluate("", Ctx()));
    }

    [Fact]
    public void Evaluate_StaticTextOnly_NoCurrentRow_ReturnsExpressionText()
    {
        // 没有 CurrentRow 时，{{Name}} 不解析，返回纯 expression 文本
        Assert.Equal("Name", _engine.Evaluate("{{Name}}", Ctx()));
    }
}
