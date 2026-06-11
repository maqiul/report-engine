using System.Collections.Generic;
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

    // ============ D4 边界用例 (v0.1.13) ============

    [Fact]
    public void Evaluate_CurrentRow_DotPath_Resolves_To_Value()
    {
        // {{currentRow.amount}} 显式 currentRow 前缀
        var engine = NewEngine();
        var ctx = NewContext(currentRow: new Dictionary<string, object> { ["amount"] = 99.5m });

        var result = engine.Evaluate("{{currentRow.amount}}", ctx);

        result.Should().Be("99.5");
    }

    [Fact]
    public void Evaluate_Avg_Returns_Mean_Across_All_Rows()
    {
        // AVG 聚合: (100 + 200 + 300) / 3 = 200
        var engine = NewEngine();
        var ctx = NewContext(
            rows: new List<Dictionary<string, object>>
            {
                new() { ["amount"] = 100m },
                new() { ["amount"] = 200m },
                new() { ["amount"] = 300m },
            });

        var result = engine.Evaluate("{{AVG(amount)}}", ctx);

        result.Should().Be("200");
    }

    [Fact]
    public void Evaluate_Min_Returns_Smallest_Value()
    {
        // MIN 聚合: min(100, 200, 300) = 100
        var engine = NewEngine();
        var ctx = NewContext(
            rows: new List<Dictionary<string, object>>
            {
                new() { ["amount"] = 100m },
                new() { ["amount"] = 200m },
                new() { ["amount"] = 300m },
            });

        var result = engine.Evaluate("{{MIN(amount)}}", ctx);

        result.Should().Be("100");
    }

    [Fact]
    public void Evaluate_Max_Returns_Largest_Value()
    {
        // MAX 聚合: max(100, 200, 300) = 300
        var engine = NewEngine();
        var ctx = NewContext(
            rows: new List<Dictionary<string, object>>
            {
                new() { ["amount"] = 100m },
                new() { ["amount"] = 200m },
                new() { ["amount"] = 300m },
            });

        var result = engine.Evaluate("{{MAX(amount)}}", ctx);

        result.Should().Be("300");
    }

    [Fact]
    public void Evaluate_FieldFormat_Currency_Formats_Decimal()
    {
        // FieldFormat = "currency" => C2 格式 (zh-CN: ¥1,234.50)
        var engine = NewEngine();
        var ctx = NewContext(currentRow: new Dictionary<string, object> { ["price"] = 1234.5m });
        ctx.FieldFormat = "currency";

        var result = engine.Evaluate("{{price}}", ctx);

        // 验证金额格式: 以 ¥ 开头, 含千分位, 含 2 位小数
        result.Should().StartWith("¥");
        result.Should().Contain("1,234.50");
    }

    [Fact]
    public void Evaluate_FieldFormat_Date_Formats_DateTime()
    {
        // FieldFormat = "date" => yyyy-MM-dd
        var engine = NewEngine();
        var ctx = NewContext(currentRow: new Dictionary<string, object>
        {
            ["created"] = new DateTime(2026, 6, 11, 14, 30, 0),
        });
        ctx.FieldFormat = "date";

        var result = engine.Evaluate("{{created}}", ctx);

        result.Should().Be("2026-06-11");
    }

    [Fact]
    public void Evaluate_EmptyString_Returns_EmptyString()
    {
        // 空模板: 不崩, 返回 ""
        var engine = NewEngine();
        var ctx = NewContext();

        var result = engine.Evaluate("", ctx);

        result.Should().Be("");
    }

    [Fact]
    public void Evaluate_NoPlaceholder_Returns_Input_Unchanged()
    {
        // 无 {{...}} 占位符: 原文返回
        var engine = NewEngine();
        var ctx = NewContext();

        var result = engine.Evaluate("普通文本 - 没有占位符", ctx);

        result.Should().Be("普通文本 - 没有占位符");
    }
}