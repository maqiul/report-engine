using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ReportEngine.Core;
using ReportEngine.Core.Parsing;
using ReportEngine.Core.Rendering;
using ReportEngine.Core.SubReports;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// 跨端一致性测试 — .NET 侧 dump。
/// 用与 java-lib CrossConsistencyDumpTest 完全相同的模板 JSON + 字符串数据，
/// 渲染后导出规范化摘要 JSON，供 tests/cross-consistency/check.js 比对。
///
/// 输出路径由环境变量 CC_DUMP 指定；未设置时只做结构断言（不写文件）。
/// </summary>
public class CrossConsistencyDumpTest
{
    // ⚠️ 这份 JSON 必须与 java-lib CrossConsistencyDumpTest.java 里的 TEMPLATE 逐字一致。
    //    表达式用两端通用的 {{currentRow.xxx}}；字段值全用字符串，规避 .NET FormatValue
    //    vs Java toString() 的数字格式化差异（该差异作为「已知跨端差异」单独记录）。
    internal const string TemplateJson = """
    {
      "version": "1.0",
      "page": { "width": 210, "height": 297, "margin": { "top": 15, "bottom": 15, "left": 15, "right": 15 } },
      "dataSources": [ { "name": "orders", "type": "json" } ],
      "bands": [
        { "type": "reportHeader", "height": 12, "elements": [
          { "type": "text", "text": "ORDER SUMMARY", "x": 0, "y": 0, "width": 180, "height": 10, "font": { "size": 18, "bold": true }, "alignment": "center" }
        ] },
        { "type": "detail", "height": 8, "dataSource": "orders", "elements": [
          { "type": "text", "text": "{{currentRow.id}}",       "x": 0,   "y": 0, "width": 30, "height": 6, "font": { "size": 12 }, "alignment": "left" },
          { "type": "text", "text": "{{currentRow.customer}}", "x": 35,  "y": 0, "width": 70, "height": 6, "font": { "size": 12 }, "alignment": "left" },
          { "type": "text", "text": "{{currentRow.status}}",   "x": 110, "y": 0, "width": 70, "height": 6, "font": { "size": 12 }, "alignment": "left" }
        ] }
      ]
    }
    """;

    internal static List<Dictionary<string, object>> Orders() => new()
    {
        new() { ["id"] = "SO-001", ["customer"] = "Acme Corp", ["status"] = "PAID" },
        new() { ["id"] = "SO-002", ["customer"] = "Globex",    ["status"] = "OPEN" },
        new() { ["id"] = "SO-003", ["customer"] = "Initech",   ["status"] = "PAID" },
    };

    [Fact]
    public async Task Dump_Dotnet_Summary()
    {
        var parser = new TemplateParser();
        var template = parser.Parse(TemplateJson);

        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            ["orders"] = Orders()
        };

        var renderer = new ReportRenderer(new NullResolver());
        var rendered = await renderer.RenderAsync(template, data);

        // ---- 结构断言：确保 .NET 渲染本身正确 ----
        Assert.NotEmpty(rendered.Pages);
        var texts = rendered.Pages
            .SelectMany(p => p.Elements)
            .OfType<RenderedTextElement>()
            .Select(e => e.Text)
            .ToList();

        Assert.Contains("ORDER SUMMARY", texts);
        Assert.Contains("SO-001", texts);
        Assert.Contains("SO-002", texts);
        Assert.Contains("SO-003", texts);
        Assert.Contains("Acme Corp", texts);
        Assert.Contains("Initech", texts);
        Assert.Contains("PAID", texts);
        Assert.Contains("OPEN", texts);
        // 占位符必须被替换（不能有残留 {{currentRow.）
        Assert.DoesNotContain(texts, t => t.Contains("currentRow."));

        // ---- 规范化摘要（坐标不含 y：.NET 分页 vs Java 流式纵向不可比）----
        var summary = new
        {
            engine = "dotnet",
            pageCount = rendered.Pages.Count,
            elements = rendered.Pages
                .SelectMany(p => p.Elements)
                .Select(ToSummaryElement)
                .OrderBy(e => e.Text, StringComparer.Ordinal)
                .ThenBy(e => e.X)
                .ToList()
        };

        var json = JsonSerializer.Serialize(summary, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var outPath = Environment.GetEnvironmentVariable("CC_DUMP");
        if (!string.IsNullOrWhiteSpace(outPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath))!);
            File.WriteAllText(outPath, json);
        }
    }

    private static SummaryElement ToSummaryElement(RenderedElement el)
    {
        string type = el switch
        {
            RenderedTextElement => "text",
            RenderedImageElement => "image",
            RenderedLineElement => "line",
            RenderedShapeElement => "shape",
            _ => "other"
        };

        string text = el is RenderedTextElement t ? t.Text : "";
        string align = el is RenderedTextElement te ? te.Alignment.ToString().ToLowerInvariant() : "";
        double size = el is RenderedTextElement ts ? ts.Font.Size : 0;

        return new SummaryElement
        {
            Type = type,
            Text = text,
            X = Round(el.X),
            W = Round(el.Width),
            Size = Round(size),
            Align = align
        };
    }

    private static double Round(double v) => Math.Round(v, 2);

    private sealed class SummaryElement
    {
        public string Type { get; set; } = "";
        public string Text { get; set; } = "";
        public double X { get; set; }
        public double W { get; set; }
        public double Size { get; set; }
        public string Align { get; set; } = "";
    }

    private sealed class NullResolver : ITemplateResolver
    {
        public bool Exists(string templateRef) => false;
        public Task<ReportTemplate> ResolveAsync(string templateRef)
            => throw new NotSupportedException("cross-consistency fixture has no sub-reports");
    }
}
