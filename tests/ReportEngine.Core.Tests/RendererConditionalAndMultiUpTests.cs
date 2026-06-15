using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Rendering;
using ReportEngine.Core.SubReports;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ReportRenderer 高级场景测试：
///   - MultiUp 渲染（行×列等分 + 多页）
///   - RenderContext 默认值
///   - RenderContext 数据源添加
///   - 大数据量多页
/// </summary>
public class RendererConditionalAndMultiUpTests
{
    private static ReportRenderer NewRenderer() => new(new InMemoryTemplateResolver());

    private static ReportTemplate BuildTemplate(int rows, int? multiUpRows = null, int? multiUpCols = null)
    {
        var data = Enumerable.Range(1, rows).Select(i => (Dictionary<string, object>)new Dictionary<string, object>
        {
            { "id", i },
        }).ToList();

        var t = new ReportTemplate
        {
            Page = new PageInfo { Width = 100, Height = 100 },
        };
        if (multiUpRows.HasValue && multiUpCols.HasValue)
        {
            t.Page.MultiUp = new MultiUpConfig
            {
                Rows = multiUpRows.Value,
                Columns = multiUpCols.Value,
                HSpacing = 1,
                VSpacing = 1,
            };
        }
        t.DataSources.Add(new DataSourceDef { Name = "ds" });
        t.Bands.Add(new Band
        {
            Type = BandType.Detail,
            DataSource = "ds",
            Elements = { new TextElement { X = 10, Y = 10, Width = 20, Height = 5 } },
        });

        // 把数据塞到额外字典中作为输出参数传入
        LastBuiltData = data;
        return t;
    }

    private static List<Dictionary<string, object>>? LastBuiltData;

    [Fact]
    public async Task RenderAsync_SimpleTemplate_ReturnsAtLeastOnePage()
    {
        var t = BuildTemplate(rows: 3);
        var ds = new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "ds", LastBuiltData! },
        };
        var result = await NewRenderer().RenderAsync(t, ds);
        Assert.NotNull(result);
        Assert.NotEmpty(result.Pages);
    }

    [Fact]
    public async Task RenderAsync_DetailBand_ContainsElements()
    {
        var t = BuildTemplate(rows: 1);
        var ds = new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "ds", LastBuiltData! },
        };
        var result = await NewRenderer().RenderAsync(t, ds);
        Assert.NotEmpty(result.Pages[0].Elements);
    }

    [Fact]
    public async Task RenderAsync_MultiUp_2x2_ProducesAtLeastOnePage()
    {
        var t = BuildTemplate(rows: 3, multiUpRows: 2, multiUpCols: 2);
        var ds = new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "ds", LastBuiltData! },
        };
        var result = await NewRenderer().RenderAsync(t, ds);
        Assert.NotEmpty(result.Pages);
    }

    [Fact]
    public async Task RenderAsync_MultiUp_5Rows_2x2_ProducesTwoPages()
    {
        var t = BuildTemplate(rows: 5, multiUpRows: 2, multiUpCols: 2);
        var ds = new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "ds", LastBuiltData! },
        };
        var result = await NewRenderer().RenderAsync(t, ds);
        // 5 条 2×2 → 第一页 4 第二页 1
        Assert.True(result.Pages.Count >= 2, $"实际页数: {result.Pages.Count}");
    }

    [Fact]
    public async Task RenderAsync_NoBands_ProducesAtLeastOnePage()
    {
        // 无 Band 时仍会创建 1 个空页（实际行为：renderer 至少留一页）
        var t = new ReportTemplate { Page = new PageInfo { Width = 100, Height = 100 } };
        var ds = new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "ds", new List<Dictionary<string, object>>() },
        };
        var result = await NewRenderer().RenderAsync(t, ds);
        Assert.NotEmpty(result.Pages);
    }

    [Fact]
    public void RenderContext_Defaults_AreA4Like()
    {
        var ctx = new RenderContext();
        // RenderContext 默认 PageWidth=210（与 PageInfo 默认一致）
        Assert.Equal(210, ctx.PageWidth);
        Assert.Equal(297, ctx.PageHeight);
        Assert.NotNull(ctx.DataSources);
        Assert.Empty(ctx.DataSources);
        Assert.Null(ctx.CurrentRow);
        // DataSourceName 默认是空字符串（不是 null）
        Assert.Equal("", ctx.DataSourceName);
    }

    [Fact]
    public void RenderContext_AddDataSource()
    {
        var ctx = new RenderContext();
        var data = new List<Dictionary<string, object>> { new() { { "x", 1 } } };
        ctx.DataSources.Add("k", data);
        Assert.True(ctx.DataSources.ContainsKey("k"));
    }

    [Fact]
    public async Task RenderAsync_LargeDataSet_GeneratesMultiplePages()
    {
        var rows = Enumerable.Range(1, 50).Select(i => new Dictionary<string, object> { { "id", i } }).ToList();
        var t = new ReportTemplate
        {
            Page = new PageInfo { Width = 100, Height = 30 },
        };
        t.DataSources.Add(new DataSourceDef { Name = "ds" });
        t.Bands.Add(new Band
        {
            Type = BandType.Detail,
            DataSource = "ds",
            Height = 10,
            Elements = { new TextElement { X = 1, Y = 1, Width = 10, Height = 3 } },
        });
        var ds = new Dictionary<string, List<Dictionary<string, object>>> { { "ds", rows } };
        var result = await NewRenderer().RenderAsync(t, ds);
        Assert.True(result.Pages.Count >= 2);
    }

    [Fact]
    public async Task RenderAsync_MultiUp_ConfigPreservedInResult()
    {
        var t = BuildTemplate(rows: 1, multiUpRows: 1, multiUpCols: 2);
        var ds = new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "ds", LastBuiltData! },
        };
        var result = await NewRenderer().RenderAsync(t, ds);
        Assert.NotNull(result.Template);
        Assert.NotNull(result.Template.Page.MultiUp);
        Assert.Equal(1, result.Template.Page.MultiUp.Rows);
        Assert.Equal(2, result.Template.Page.MultiUp.Columns);
    }

    [Fact]
    public async Task RenderAsync_EmptyDataSet_NoBandsAtDetail_NoException()
    {
        var t = new ReportTemplate
        {
            Page = new PageInfo { Width = 100, Height = 100 },
        };
        t.DataSources.Add(new DataSourceDef { Name = "ds" });
        t.Bands.Add(new Band
        {
            Type = BandType.Detail,
            DataSource = "ds",
        });
        var ds = new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "ds", new List<Dictionary<string, object>>() },
        };
        var result = await NewRenderer().RenderAsync(t, ds);
        Assert.NotNull(result);
    }
}
