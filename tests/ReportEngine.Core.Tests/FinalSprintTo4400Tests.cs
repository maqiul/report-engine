using ReportEngine.Core;
using ReportEngine.Core.Data;
using Xunit;

namespace ReportEngine.Core.Tests;

// ─────────────────────────────────────────────────────────────────────────────
// ExpressionEngine 字符串处理测试
// ─────────────────────────────────────────────────────────────────────────────

public class ExpressionEngineStringTests
{
    private readonly ExpressionEngine _engine = new();

    [Fact]
    public void Evaluate_StringWithLiteral()
    {
        var ctx = new RenderContext();
        ctx.CurrentRow = new Dictionary<string, object> { { "name", "Alice" } };
        var result = _engine.Evaluate("Hello, {{name}}!", ctx);
        Assert.Equal("Hello, Alice!", result);
    }

    [Fact]
    public void Evaluate_MultipleStrings()
    {
        var ctx = new RenderContext();
        ctx.CurrentRow = new Dictionary<string, object> { { "city", "Ningbo" }, { "country", "China" } };
        var result = _engine.Evaluate("{{city}}, {{country}}", ctx);
        Assert.Equal("Ningbo, China", result);
    }

    [Fact]
    public void Evaluate_EmptyString()
    {
        var ctx = new RenderContext();
        ctx.CurrentRow = new Dictionary<string, object> { { "value", "" } };
        var result = _engine.Evaluate("Value: {{value}}", ctx);
        Assert.Equal("Value: ", result);
    }

    [Fact]
    public void Evaluate_NullValue()
    {
        var ctx = new RenderContext();
        ctx.CurrentRow = new Dictionary<string, object> { { "value", null! } };
        var result = _engine.Evaluate("Value: {{value}}", ctx);
        Assert.Equal("Value: ", result);
    }

    [Fact]
    public void Evaluate_NumericValue()
    {
        var ctx = new RenderContext();
        ctx.CurrentRow = new Dictionary<string, object> { { "count", 42 } };
        var result = _engine.Evaluate("Count: {{count}}", ctx);
        Assert.Equal("Count: 42", result);
    }

    [Fact]
    public void Evaluate_BooleanValue()
    {
        var ctx = new RenderContext();
        ctx.CurrentRow = new Dictionary<string, object> { { "active", true } };
        var result = _engine.Evaluate("Active: {{active}}", ctx);
        Assert.Equal("Active: True", result);
    }

    [Fact]
    public void Evaluate_DateTimeValue()
    {
        var ctx = new RenderContext();
        var date = new DateTime(2026, 6, 16);
        ctx.CurrentRow = new Dictionary<string, object> { { "date", date } };
        var result = _engine.Evaluate("Date: {{date}}", ctx);
        Assert.Contains("2026", result);
    }

    [Fact]
    public void Evaluate_DecimalValue()
    {
        var ctx = new RenderContext();
        ctx.CurrentRow = new Dictionary<string, object> { { "price", 99.99 } };
        var result = _engine.Evaluate("Price: {{price}}", ctx);
        Assert.Equal("Price: 99.99", result);
    }
}
