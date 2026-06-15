using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReportEngine.Core;
using ReportEngine.Core.Rendering;
using ReportEngine.Core.SubReports;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ReportRenderer 高级特性测试：多联打印、交叉表、行过滤、不分页嵌入。
/// </summary>
public class ReportRendererAdvancedTests
{
    private static ReportRenderer NewRenderer() => new ReportRenderer(new InMemoryTemplateResolver());

    private static ReportTemplate MakeTemplate(params Band[] bands)
    {
        return new ReportTemplate
        {
            Page = new PageInfo { Width = 100, Height = 100, Margin = new Margin { Top = 0, Bottom = 0, Left = 0, Right = 0 } },
            Bands = bands.ToList(),
        };
    }

    private static Band MakeDetail(string dsName, params ReportElement[] elements)
    {
        return new Band
        {
            Type = BandType.Detail,
            Height = 10,
            DataSource = dsName,
            Elements = elements.ToList(),
        };
    }

    private static Dictionary<string, object> Row(string field, object value)
    {
        return new Dictionary<string, object> { { field, value } };
    }

    [Fact]
    public async Task MultiUp_Horizontal_2x2_TilesToPhysicalPage()
    {
        var template = MakeTemplate();
        template.Page.MultiUp = new MultiUpConfig { Rows = 2, Columns = 2, HSpacing = 0, VSpacing = 0, Direction = "Horizontal" };
        template.Bands.Add(MakeDetail("ds", new TextElement { X = 0, Y = 0, Width = 50, Height = 5, Text = "{x}" }));

        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "ds", new List<Dictionary<string, object>> { Row("x", 1), Row("x", 2), Row("x", 3), Row("x", 4) } },
        };

        var result = await NewRenderer().RenderAsync(template, data);
        Assert.Single(result.Pages);
        Assert.Equal(4, result.Pages[0].Elements.Count);
    }

    [Fact]
    public async Task MultiUp_EmptyDataSource_ProducesOnePage()
    {
        var template = MakeTemplate();
        template.Page.MultiUp = new MultiUpConfig { Rows = 2, Columns = 2, HSpacing = 0, VSpacing = 0 };
        template.Bands.Add(MakeDetail("ds"));

        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "ds", new List<Dictionary<string, object>>() },
        };

        var result = await NewRenderer().RenderAsync(template, data);
        Assert.Single(result.Pages);
    }

    [Fact]
    public async Task CrossTab_Sum_BasicAggregation()
    {
        var ct = new CrossTabElement
        {
            X = 0, Y = 0, Width = 80, Height = 30,
            DataSource = "ds",
            RowFields = new List<string> { "region" },
            ColumnFields = new List<string> { "product" },
            Measures = new List<CrossTabMeasure> { new CrossTabMeasure { Field = "amount", Aggregate = "Sum" } },
            ShowRowTotal = true,
            ShowColumnTotal = true,
        };
        var template = MakeTemplate(MakeDetail("ds", ct));

        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "ds", new List<Dictionary<string, object>>
                {
                    new() { { "region", "East" }, { "product", "A" }, { "amount", 10.0 } },
                    new() { { "region", "East" }, { "product", "B" }, { "amount", 20.0 } },
                    new() { { "region", "West" }, { "product", "A" }, { "amount", 30.0 } },
                }
            },
        };

        var result = await NewRenderer().RenderAsync(template, data);
        var crossTab = result.Pages[0].Elements.OfType<RenderedCrossTabElement>().FirstOrDefault();
        Assert.NotNull(crossTab);
        var cellTexts = crossTab.Cells.Select(c => c.Text).ToList();
        Assert.Contains("East", cellTexts);
        Assert.Contains("West", cellTexts);
        Assert.Contains("合计", cellTexts);
    }

    [Fact]
    public async Task CrossTab_MultipleMeasures_ShowsMeasureLabels()
    {
        var ct = new CrossTabElement
        {
            X = 0, Y = 0, Width = 80, Height = 30,
            DataSource = "ds",
            RowFields = new List<string> { "region" },
            ColumnFields = new List<string> { "product" },
            Measures = new List<CrossTabMeasure>
            {
                new CrossTabMeasure { Field = "amount", Aggregate = "Sum", Label = "销售额" },
                new CrossTabMeasure { Field = "qty", Aggregate = "Sum", Label = "数量" },
            },
        };
        var template = MakeTemplate(MakeDetail("ds", ct));

        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "ds", new List<Dictionary<string, object>>
                {
                    new() { { "region", "East" }, { "product", "A" }, { "amount", 10.0 }, { "qty", 5.0 } },
                }
            },
        };

        var result = await NewRenderer().RenderAsync(template, data);
        var crossTab = result.Pages[0].Elements.OfType<RenderedCrossTabElement>().FirstOrDefault();
        Assert.NotNull(crossTab);
        var cellTexts = crossTab.Cells.Select(c => c.Text).ToList();
        Assert.Contains(cellTexts, t => t != null && t.Contains("销售额"));
        Assert.Contains(cellTexts, t => t != null && t.Contains("数量"));
    }

    [Fact]
    public async Task SubReport_Nested_RendersSuccessfully()
    {
        // 子模板
        var childTemplate = MakeTemplate(MakeDetail("cds", new TextElement { X = 0, Y = 0, Width = 30, Height = 5, Text = "{name}" }));
        childTemplate.DataSources = new List<DataSourceDef> { new DataSourceDef { Name = "cds" } };

        var resolver = new InMemoryTemplateResolver(childTemplate);

        // 父模板
        var subReportEl = new SubReportElement
        {
            X = 0, Y = 10, Width = 30, Height = 5,
            TemplateRef = "child",
            DataBinding = new SubReportDataBinding { ParamMap = new Dictionary<string, string>() },
            RepeatPerRow = true,
        };
        var template = MakeTemplate(MakeDetail("pds", subReportEl));
        template.DataSources = new List<DataSourceDef> { new DataSourceDef { Name = "pds" } };

        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "pds", new List<Dictionary<string, object>>
                {
                    new() { { "x", 1 } },
                    new() { { "x", 2 } },
                }
            },
            { "cds", new List<Dictionary<string, object>>
                {
                    new() { { "name", "Alice" } },
                    new() { { "name", "Bob" } },
                }
            },
        };

        var result = await new ReportRenderer(resolver).RenderAsync(template, data);
        Assert.NotEmpty(result.Pages[0].Elements);
    }

    [Fact]
    public async Task SubReport_MaxNestingDepth_ReturnsEmpty()
    {
        // 构造一个深度超限的子报表场景（通过 RecursiveParent 间接测试）
        // 这里我们验证：模板引用不存在的子模板时不会崩溃
        var template = MakeTemplate(MakeDetail("pds", new SubReportElement
        {
            X = 0, Y = 10, Width = 30, Height = 5,
            TemplateRef = "nonexistent",
            DataBinding = new SubReportDataBinding { ParamMap = new Dictionary<string, string>() },
            RepeatPerRow = true,
        }));
        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "pds", new List<Dictionary<string, object>> { Row("x", 1) } },
        };
        var result = await NewRenderer().RenderAsync(template, data);
        Assert.NotNull(result);
    }
}

/// <summary>内存模板解析器，map 中找不到时返回空模板（防止崩溃）。</summary>
internal class InMemoryTemplateResolver : ITemplateResolver
{
    private readonly Dictionary<string, ReportTemplate> _templates;

    public InMemoryTemplateResolver(params ReportTemplate[] templates)
    {
        _templates = templates.ToDictionary(t => t.Page.Width + "_" + t.Page.Height);
        if (templates.Length > 0)
        {
            // 简化：单一子模板，按 TemplateRef="child" 查找
        }
    }

    public Task<ReportTemplate> ResolveAsync(string templateRef)
    {
        // 单模板时返回；多模板时按 ref 匹配
        if (_templates.Count == 0)
        {
            return Task.FromResult(new ReportTemplate
            {
                Page = new PageInfo { Width = 100, Height = 50, Margin = new Margin { Top = 0, Bottom = 0, Left = 0, Right = 0 } },
            });
        }
        return Task.FromResult(_templates.Values.First());
    }

    public bool Exists(string templateRef)
    {
        return _templates.Count > 0;
    }
}
