using ReportEngine.Core;
using ReportEngine.Core.Data;
using Xunit;

namespace ReportEngine.Core.Tests;

// ─────────────────────────────────────────────────────────────────────────────
// ExpressionEngine 格式化测试
// ─────────────────────────────────────────────────────────────────────────────

public class ExpressionEngineFormat2Tests
{
    private readonly ExpressionEngine _engine = new();

    [Fact]
    public void Evaluate_DateFieldFormat_Date()
    {
        var ctx = new RenderContext { FieldFormat = "date" };
        ctx.CurrentRow = new Dictionary<string, object> { { "d", new DateTime(2026, 6, 15) } };
        var result = _engine.Evaluate("{{d}}", ctx);
        Assert.Equal("2026-06-15", result);
    }

    [Fact]
    public void Evaluate_DateFieldFormat_Datetime()
    {
        var ctx = new RenderContext { FieldFormat = "datetime" };
        ctx.CurrentRow = new Dictionary<string, object> { { "d", new DateTime(2026, 6, 15, 14, 30, 0) } };
        var result = _engine.Evaluate("{{d}}", ctx);
        Assert.Equal("2026-06-15 14:30:00", result);
    }

    [Fact]
    public void Evaluate_DecimalFieldFormat_Currency()
    {
        var ctx = new RenderContext { FieldFormat = "currency" };
        ctx.CurrentRow = new Dictionary<string, object> { { "price", 123.456m } };
        var result = _engine.Evaluate("{{price}}", ctx);
        Assert.Contains("123.46", result);
    }

    [Fact]
    public void Evaluate_DecimalFieldFormat_Percent()
    {
        var ctx = new RenderContext { FieldFormat = "percent" };
        ctx.CurrentRow = new Dictionary<string, object> { { "rate", 0.856m } };
        var result = _engine.Evaluate("{{rate}}", ctx);
        Assert.Contains("85.6", result);
        Assert.Contains("%", result);
    }

    [Fact]
    public void Evaluate_DecimalFieldFormat_Number0()
    {
        var ctx = new RenderContext { FieldFormat = "number:0" };
        ctx.CurrentRow = new Dictionary<string, object> { { "n", 123.789m } };
        var result = _engine.Evaluate("{{n}}", ctx);
        Assert.Equal("124", result);
    }

    [Fact]
    public void Evaluate_DecimalFieldFormat_Number1()
    {
        var ctx = new RenderContext { FieldFormat = "number:1" };
        ctx.CurrentRow = new Dictionary<string, object> { { "n", 123.456m } };
        var result = _engine.Evaluate("{{n}}", ctx);
        Assert.Equal("123.5", result);
    }

    [Fact]
    public void Evaluate_DecimalFieldFormat_Number2()
    {
        var ctx = new RenderContext { FieldFormat = "number:2" };
        ctx.CurrentRow = new Dictionary<string, object> { { "n", 123.456m } };
        var result = _engine.Evaluate("{{n}}", ctx);
        Assert.Equal("123.46", result);
    }

    [Fact]
    public void Evaluate_DoubleFieldFormat_Currency()
    {
        var ctx = new RenderContext { FieldFormat = "currency" };
        ctx.CurrentRow = new Dictionary<string, object> { { "price", 99.99 } };
        var result = _engine.Evaluate("{{price}}", ctx);
        Assert.Contains("99.99", result);
    }

    [Fact]
    public void Evaluate_DoubleFieldFormat_Percent()
    {
        var ctx = new RenderContext { FieldFormat = "percent" };
        ctx.CurrentRow = new Dictionary<string, object> { { "rate", 0.5 } };
        var result = _engine.Evaluate("{{rate}}", ctx);
        Assert.Contains("50.0", result);
        Assert.Contains("%", result);
    }

    [Fact]
    public void Evaluate_NoFieldFormat_DefaultToString()
    {
        var ctx = new RenderContext();
        ctx.CurrentRow = new Dictionary<string, object> { { "n", 42 } };
        var result = _engine.Evaluate("{{n}}", ctx);
        Assert.Equal("42", result);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// ExpressionEngine 聚合函数测试
// ─────────────────────────────────────────────────────────────────────────────

public class ExpressionEngineAggregate2Tests
{
    private readonly ExpressionEngine _engine = new();

    private RenderContext MakeContext(string dsName, List<Dictionary<string, object>> rows)
    {
        var ctx = new RenderContext { DataSourceName = dsName };
        ctx.DataSources[dsName] = rows;
        return ctx;
    }

    [Fact]
    public void Evaluate_SUM()
    {
        var rows = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "amount", 100m } },
            new Dictionary<string, object> { { "amount", 200m } },
            new Dictionary<string, object> { { "amount", 300m } },
        };
        var ctx = MakeContext("ds", rows);
        var result = _engine.Evaluate("{{SUM(amount)}}", ctx);
        Assert.Equal("600", result);
    }

    [Fact]
    public void Evaluate_AVG()
    {
        var rows = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "score", 80.0 } },
            new Dictionary<string, object> { { "score", 90.0 } },
            new Dictionary<string, object> { { "score", 100.0 } },
        };
        var ctx = MakeContext("ds", rows);
        var result = _engine.Evaluate("{{AVG(score)}}", ctx);
        Assert.Equal("90", result);
    }

    [Fact]
    public void Evaluate_COUNT()
    {
        var rows = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "id", 1 } },
            new Dictionary<string, object> { { "id", 2 } },
            new Dictionary<string, object> { { "id", 3 } },
        };
        var ctx = MakeContext("ds", rows);
        var result = _engine.Evaluate("{{COUNT(id)}}", ctx);
        Assert.Equal("3", result);
    }

    [Fact]
    public void Evaluate_MIN()
    {
        var rows = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "price", 50m } },
            new Dictionary<string, object> { { "price", 30m } },
            new Dictionary<string, object> { { "price", 80m } },
        };
        var ctx = MakeContext("ds", rows);
        var result = _engine.Evaluate("{{MIN(price)}}", ctx);
        Assert.Equal("30", result);
    }

    [Fact]
    public void Evaluate_MAX()
    {
        var rows = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "price", 50m } },
            new Dictionary<string, object> { { "price", 30m } },
            new Dictionary<string, object> { { "price", 80m } },
        };
        var ctx = MakeContext("ds", rows);
        var result = _engine.Evaluate("{{MAX(price)}}", ctx);
        Assert.Equal("80", result);
    }

    [Fact]
    public void Evaluate_SUM_EmptyDataSource_ReturnsZero()
    {
        var ctx = new RenderContext { DataSourceName = "empty" };
        var result = _engine.Evaluate("{{SUM(amount)}}", ctx);
        Assert.Equal("0", result);
    }

    [Fact]
    public void Evaluate_COUNT_EmptyDataSource_ReturnsZero()
    {
        var ctx = new RenderContext { DataSourceName = "empty" };
        var result = _engine.Evaluate("{{COUNT(id)}}", ctx);
        Assert.Equal("0", result);
    }

    [Fact]
    public void Evaluate_Aggregate_CaseInsensitive()
    {
        var rows = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "v", 10m } },
            new Dictionary<string, object> { { "v", 20m } },
        };
        var ctx = MakeContext("ds", rows);
        var result = _engine.Evaluate("{{sum(v)}}", ctx);
        Assert.Equal("30", result);
    }
}
