using ReportEngine.Core;
using ReportEngine.Core.Data;
using Xunit;

namespace ReportEngine.Core.Tests;

// ─────────────────────────────────────────────────────────────────────────────
// ExpressionEngine 特殊值测试
// ─────────────────────────────────────────────────────────────────────────────

public class ExpressionEngineSpecialValueTests
{
    private readonly ExpressionEngine _engine = new();

    [Fact]
    public void Evaluate_IntegerZero()
    {
        var ctx = new RenderContext();
        ctx.CurrentRow = new Dictionary<string, object> { { "value", 0 } };
        var result = _engine.Evaluate("Value: {{value}}", ctx);
        Assert.Equal("Value: 0", result);
    }

    [Fact]
    public void Evaluate_NegativeNumber()
    {
        var ctx = new RenderContext();
        ctx.CurrentRow = new Dictionary<string, object> { { "value", -100 } };
        var result = _engine.Evaluate("Value: {{value}}", ctx);
        Assert.Equal("Value: -100", result);
    }

    [Fact]
    public void Evaluate_LargeNumber()
    {
        var ctx = new RenderContext();
        ctx.CurrentRow = new Dictionary<string, object> { { "value", 999999999 } };
        var result = _engine.Evaluate("Value: {{value}}", ctx);
        Assert.Equal("Value: 999999999", result);
    }

    [Fact]
    public void Evaluate_FalseBoolean()
    {
        var ctx = new RenderContext();
        ctx.CurrentRow = new Dictionary<string, object> { { "active", false } };
        var result = _engine.Evaluate("Active: {{active}}", ctx);
        Assert.Equal("Active: False", result);
    }

    [Fact]
    public void Evaluate_WhitespaceString()
    {
        var ctx = new RenderContext();
        ctx.CurrentRow = new Dictionary<string, object> { { "value", "   " } };
        var result = _engine.Evaluate("Value: {{value}}", ctx);
        Assert.Equal("Value:    ", result);
    }
}
