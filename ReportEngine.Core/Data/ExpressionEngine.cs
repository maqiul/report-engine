using System.Text.RegularExpressions;

namespace ReportEngine.Core.Data;

/// <summary>
/// 表达式引擎：处理模板中的 {{...}} 占位符替换
/// 支持：字段引用、聚合函数、简单算术、条件判断
/// </summary>
public class ExpressionEngine
{
    // 匹配 {{expression}} 格式
    private static readonly Regex PlaceholderRegex = new(
        @"\{\{(.+?)\}\}",
        RegexOptions.Compiled);

    // 聚合函数：{{SUM(field)}}, {{AVG(field)}}, {{COUNT(field)}}, {{MIN(field)}}, {{MAX(field)}}
    private static readonly Regex AggregateRegex = new(
        @"^(SUM|AVG|COUNT|MIN|MAX)\((.+?)\)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // 分页变量：{{PAGE}}, {{TOTAL_PAGES}}, {{NOW}}, {{REPORT_DATE}}
    private static readonly Dictionary<string, Func<RenderContext, object>> SystemVariables = new()
    {
        ["PAGE"] = ctx => ctx.CurrentPage,
        ["TOTAL_PAGES"] = ctx => ctx.TotalPages,
        ["NOW"] = ctx => DateTime.Now,
        ["REPORT_DATE"] = ctx => DateTime.Now.ToString("yyyy-MM-dd"),
        ["ROW_NUMBER"] = ctx => ctx.CurrentRowNumber,
    };

    /// <summary>
    /// 替换模板文本中的所有表达式占位符
    /// </summary>
    public string Evaluate(string text, RenderContext context)
    {
        return PlaceholderRegex.Replace(text, match =>
        {
            var expression = match.Groups[1].Value.Trim();
            var result = EvaluateExpression(expression, context);
            return FormatValue(result, context);
        });
    }

    /// <summary>
    /// 计算单个表达式的值
    /// </summary>
    private object EvaluateExpression(string expression, RenderContext context)
    {
        // 1. 系统变量
        if (SystemVariables.TryGetValue(expression.ToUpperInvariant(), out var sysFunc))
            return sysFunc(context);

        // 2. 聚合函数
        var aggMatch = AggregateRegex.Match(expression);
        if (aggMatch.Success)
            return EvaluateAggregate(aggMatch.Groups[1].Value.ToUpperInvariant(), aggMatch.Groups[2].Value, context);

        // 3. 条件表达式 IF(cond, trueVal, falseVal)
        if (expression.StartsWith("IF(", StringComparison.OrdinalIgnoreCase))
            return EvaluateIf(expression, context);

        // 4. 字段引用：dataSource.fieldName 或 currentRow.fieldName
        if (expression.Contains('.'))
            return ResolveFieldReference(expression, context);

        // 5. 纯文本或简单变量
        if (context.CurrentRow != null && context.CurrentRow.ContainsKey(expression))
            return context.CurrentRow[expression];

        return expression; // 未匹配到，返回原文本
    }

    /// <summary>
    /// 解析字段引用，支持嵌套路径
    /// </summary>
    private object ResolveFieldReference(string expression, RenderContext context)
    {
        var parts = expression.Split('.');

        if (parts[0] == "currentRow" && context.CurrentRow != null)
        {
            var rowFieldName = parts[1];
            return context.CurrentRow.TryGetValue(rowFieldName, out var val) ? val : $"{{{{{expression}}}}}";
        }

        // 数据源引用：orders.customer
        var dsName = parts[0];
        var fieldName = parts.Length > 1 ? string.Join(".", parts.Skip(1)) : "";

        if (context.DataSources.TryGetValue(dsName, out var data))
        {
            // 如果只有一层，返回数据源元信息
            if (string.IsNullOrEmpty(fieldName))
                return data;

            // 如果当前行属于该数据源，取字段值
            if (context.CurrentRow?.TryGetValue(fieldName, out var val) == true)
                return val;
        }

        return $"{{{{{expression}}}}}";
    }

    /// <summary>
    /// 计算聚合函数
    /// </summary>
    private object EvaluateAggregate(string funcName, string fieldExpression, RenderContext context)
    {
        var dataSource = context.DataSourceName;
        if (!context.DataSources.TryGetValue(dataSource, out var rows))
            return 0;

        var values = rows.Select(row =>
        {
            if (row.TryGetValue(fieldExpression, out var v))
                return v;
            return null;
        }).Where(v => v != null).ToList();

        return funcName switch
        {
            "SUM"   => values.Sum(v => ConvertToDecimal(v)),
            "AVG"   => values.Average(v => ConvertToDouble(v)),
            "COUNT" => values.Count,
            "MIN"   => values.Min(v => ConvertToDecimal(v)),
            "MAX"   => values.Max(v => ConvertToDecimal(v)),
            _ => 0
        };
    }

    /// <summary>
    /// 计算 IF 条件表达式
    /// IF({{row.status}} == 1, "已确认", "待确认")
    /// </summary>
    private object EvaluateIf(string expression, RenderContext context)
    {
        // 简单实现：提取 IF(cond, trueVal, falseVal) 三个参数
        var inner = expression.Substring(3, expression.Length - 4); // 去掉 IF( 和 )
        var parts = SplitArguments(inner);

        if (parts.Count < 3)
            return "";

        var condition = EvaluateExpression(parts[0], context);
        var isTrue = Convert.ToBoolean(condition);
        var result = isTrue ? parts[1] : parts[2];

        return EvaluateExpression(result, context);
    }

    private List<string> SplitArguments(string input)
    {
        var result = new List<string>();
        int depth = 0;
        int start = 0;

        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == '(') depth++;
            else if (input[i] == ')') depth--;
            else if (input[i] == ',' && depth == 0)
            {
                result.Add(input.Substring(start, i - start).Trim());
                start = i + 1;
            }
        }
        result.Add(input.Substring(start).Trim());
        return result;
    }

    /// <summary>
    /// 格式化输出值（根据字段格式定义）
    /// </summary>
    private string FormatValue(object value, RenderContext context)
    {
        if (value == null) return "";

        return value switch
        {
            DateTime dt => context.FieldFormat switch
            {
                "date" => dt.ToString("yyyy-MM-dd"),
                "datetime" => dt.ToString("yyyy-MM-dd HH:mm:ss"),
                _ => dt.ToString()
            },
            decimal d => context.FieldFormat switch
            {
                "currency" => d.ToString("C2"),
                "percent" => (d * 100).ToString("F1") + "%",
                "number:0" => d.ToString("F0"),
                "number:1" => d.ToString("F1"),
                "number:2" => d.ToString("F2"),
                _ => d.ToString()
            },
            double dd => context.FieldFormat switch
            {
                "currency" => dd.ToString("C2"),
                "percent" => (dd * 100).ToString("F1") + "%",
                _ => dd.ToString()
            },
            _ => value.ToString()!
        };
    }

    private static decimal ConvertToDecimal(object? value) => value switch
    {
        null => 0m,
        decimal d => d,
        double db => (decimal)db,
        int i => i,
        long l => l,
        float f => (decimal)f,
        _ => decimal.TryParse(value.ToString(), out var r) ? r : 0m
    };

    private static double ConvertToDouble(object? value) => value switch
    {
        null => 0,
        double d => d,
        decimal dc => (double)dc,
        int i => i,
        long l => l,
        float f => f,
        _ => double.TryParse(value.ToString(), out var r) ? r : 0
    };
}

/// <summary>
/// 渲染上下文：携带当前渲染状态
/// </summary>
public class RenderContext
{
    /// <summary>
    /// 所有已加载的数据源：name → rows
    /// </summary>
    public Dictionary<string, List<Dictionary<string, object>>> DataSources { get; } = new();

    /// <summary>
    /// 当前正在渲染的数据源名称
    /// </summary>
    public string DataSourceName { get; set; } = "";

    /// <summary>
    /// 当前行数据
    /// </summary>
    public Dictionary<string, object>? CurrentRow { get; set; }

    /// <summary>
    /// 当前行号（从1开始）
    /// </summary>
    public int CurrentRowNumber { get; set; } = 0;

    /// <summary>
    /// 当前页码
    /// </summary>
    public int CurrentPage { get; set; } = 1;

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages { get; set; } = 1;

    /// <summary>
    /// 当前字段格式化定义
    /// </summary>
    public string? FieldFormat { get; set; }

    /// <summary>
    /// 页面尺寸（mm）
    /// </summary>
    public double PageWidth { get; set; } = 210;
    public double PageHeight { get; set; } = 297;

    /// <summary>
    /// 嵌套深度（防无限递归）
    /// </summary>
    public int NestingDepth { get; set; } = 0;
    public const int MaxNestingDepth = 5;
}
