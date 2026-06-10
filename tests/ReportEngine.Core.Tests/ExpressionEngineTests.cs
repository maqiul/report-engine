using FluentAssertions;
using ReportEngine.Core.Data;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ExpressionEngine 行为测试：
///   - 系统变量：{{PAGE}} / {{REPORT_DATE}}
///   - 聚合：{{SUM(...)}} / {{COUNT(...)}}
///   - 字段引用：{{currentRow.xxx}}
///   - IF 条件
///   - 未知表达式：原样返回（保留现有行为，便于兼容老模板）
/// </summary>
public class ExpressionEngineTests
{
    private static ExpressionEngine NewEngine() => new();

    private static RenderContext NewContext(
        Dictionary<string, object>? currentRow = null,
        string dataSourceName = "orders",
        IEnumerable<Dictionary<string, object>>? rows = null)
    {
        var ctx = new RenderContext
        {
            DataSourceName = dataSourceName,
            CurrentRow = currentRow,
            CurrentPage = 3,
            TotalPages = 10,
            CurrentRowNumber = 2,
        };

        var list = (rows ?? new List<Dictionary<string, object>>()).ToList();
        ctx.DataSources[dataSourceName] = list;
        return ctx;
    }

    // -------- (a) 系统变量 --------

    [Fact]
    public void Evaluate_Page_SystemVariable_Returns_CurrentPage()
    {
        var engine = NewEngine();
        var ctx = NewContext();

        var result = engine.Evaluate("第 {{PAGE}} 页", ctx);

        result.Should().Be("第 3 页");
    }

    [Fact]
    public void Evaluate_ReportDate_SystemVariable_Returns_Today()
    {
        var engine = NewEngine();
        var ctx = NewContext();

        var result = engine.Evaluate("{{REPORT_DATE}}", ctx);

        result.Should().Be(DateTime.Now.ToString("yyyy-MM-dd"));
    }

    // -------- (b) 聚合函数 --------

    [Fact]
    public void Evaluate_Sum_Aggregates_Across_DataSource_Rows()
    {
        var engine = NewEngine();
        var rows = new List<Dictionary<string, object>>
        {
            new() { ["totalAmount"] = 100m },
            new() { ["totalAmount"] = 250m },
            new() { ["totalAmount"] = 50m },
        };
        // 聚合函数参数是裸字段名；数据源名由 RenderContext.DataSourceName 提供
        var ctx = NewContext(rows: rows);

        var result = engine.Evaluate("合计: {{SUM(totalAmount)}}", ctx);

        result.Should().Be("合计: 400");
    }

    [Fact]
    public void Evaluate_Count_Returns_Number_Of_Rows()
    {
        var engine = NewEngine();
        var rows = new List<Dictionary<string, object>>
        {
            new() { ["orderNo"] = "A" },
            new() { ["orderNo"] = "B" },
            new() { ["orderNo"] = "C" },
        };
        var ctx = NewContext(rows: rows);

        var result = engine.Evaluate("共 {{COUNT(orderNo)}} 条", ctx);

        result.Should().Be("共 3 条");
    }

    // -------- (c) 字段引用 --------

    [Fact]
    public void Evaluate_CurrentRow_Field_Reference_Returns_Value()
    {
        var engine = NewEngine();
        var ctx = NewContext(currentRow: new Dictionary<string, object>
        {
            ["customer"] = "张三科技"
        });

        var result = engine.Evaluate("客户: {{currentRow.customer}}", ctx);

        result.Should().Be("客户: 张三科技");
    }

    // -------- (d) IF 条件 --------

    [Fact]
    public void Evaluate_If_True_Returns_TrueBranch()
    {
        var engine = NewEngine();
        var ctx = NewContext();

        // EvaluateIf 将条件参数直接走 Convert.ToBoolean：
        //   - "true" 字面量 -> true 分支
        //   - 非空字符串在 Convert.ToBoolean 下视为 true，因此也可以用 currentRow 字段做判定
        var result = engine.Evaluate("{{IF(true, 大单, 小单)}}", ctx);

        result.Should().Be("大单");
    }

    [Fact]
    public void Evaluate_If_False_Returns_FalseBranch()
    {
        var engine = NewEngine();
        var ctx = NewContext();

        var result = engine.Evaluate("{{IF(false, 大单, 小单)}}", ctx);

        result.Should().Be("小单");
    }

    // -------- (e) 未知表达式：保留原样 --------

    [Fact]
    public void Evaluate_Unknown_Expression_Returns_Original_Text()
    {
        var engine = NewEngine();
        var ctx = NewContext();

        // 没有任何数据源/系统变量能匹配时，ExpressionEngine 现状是原样返回。
        // 这里锁住这个行为，避免无意中改成抛异常或返回空导致老模板被破坏。
        var result = engine.Evaluate("{{totally.unknown.token}}", ctx);

        result.Should().Be("{{totally.unknown.token}}");
    }
}