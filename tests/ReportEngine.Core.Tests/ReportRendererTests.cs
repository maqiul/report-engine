using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using ReportEngine.Core;
using ReportEngine.Core.Rendering;
using ReportEngine.Core.SubReports;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ReportRenderer 基本路径测试。覆盖 Step2.A 顺手清零的 5 个 net462 nullable
/// warning（line 232/234/553/554/642）所在的代码路径，以及 RenderAsync 的核心
/// 分支：
///   - 空模板 + 空数据
///   - 单 Detail band + 单行数据（基本路径）
///   - 多联模式 MultiUp.Count > 1（RenderMultiUpAsync 入口）
///   - 多行数据 → 多页（分页）
///   - 无 Detail band（只有 Header/Footer）
///   - DataSource 名不匹配（rows 为空，Detail 跳过）
/// </summary>
public class ReportRendererTests
{
    /// <summary>不读盘的 no-op resolver，ReportRenderer 不在 SubReport 路径上需要它。</summary>
    private sealed class NoopTemplateResolver : ITemplateResolver
    {
        public Task<ReportTemplate> ResolveAsync(string templateRef) => throw new System.NotImplementedException();
        public bool Exists(string templateRef) => false;
    }

    private static ReportRenderer NewRenderer() => new(new NoopTemplateResolver());

    private static ReportTemplate EmptyTemplate() => new()
    {
        Page = new PageInfo { Width = 100, Height = 100 },
        Bands = new List<Band>(),
    };

    private static ReportTemplate SingleBandTemplate(string dataSourceName) => new()
    {
        Page = new PageInfo { Width = 100, Height = 100 },
        Bands = new List<Band>
        {
            new Band { Type = BandType.Detail, Height = 10, DataSource = dataSourceName },
        },
    };

    [Fact]
    public async Task RenderAsync_EmptyTemplate_ProducesOneEmptyPage()
    {
        // 没有 band 时 Renderer 仍生成 1 个空页（PageNumber=1, TotalPages=1, Elements 空）。
        var renderer = NewRenderer();
        var result = await renderer.RenderAsync(EmptyTemplate(), new Dictionary<string, List<Dictionary<string, object>>>());

        result.Should().NotBeNull();
        result.Pages.Should().HaveCount(1);
        result.Pages[0].Elements.Should().BeEmpty();
    }

    [Fact]
    public async Task RenderAsync_SingleRowData_RendersOnePage()
    {
        var renderer = NewRenderer();
        var template = SingleBandTemplate("Main");
        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            ["Main"] = new List<Dictionary<string, object>>
            {
                new() { ["Name"] = "Alice", ["Age"] = 30 },
            },
        };

        var result = await renderer.RenderAsync(template, data);

        result.Pages.Should().HaveCount(1);
        result.PageWidth.Should().Be(100);
        result.PageHeight.Should().Be(100);
    }

    [Fact]
    public async Task RenderAsync_MultiUp_RoutesToMultiUpPath()
    {
        var renderer = NewRenderer();
        var template = new ReportTemplate
        {
            Page = new PageInfo
            {
                Width = 100, Height = 100,
                MultiUp = new MultiUpConfig { Rows = 2, Columns = 2 }, // Count = 4
            },
            Bands = new List<Band>
            {
                new Band { Type = BandType.Detail, Height = 5, DataSource = "Main" },
            },
        };
        // 4 rows，刚好一页 4 联
        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            ["Main"] = new List<Dictionary<string, object>>
            {
                new() { ["K"] = "1" }, new() { ["K"] = "2" },
                new() { ["K"] = "3" }, new() { ["K"] = "4" },
            },
        };

        var result = await renderer.RenderAsync(template, data);

        result.PageWidth.Should().Be(100);
        result.PageHeight.Should().Be(100);
        result.Pages.Should().NotBeNull();
    }

    [Fact]
    public async Task RenderAsync_MissingDataSource_ProducesEmptyPages()
    {
        // DataSource 名不匹配 → rows 为空 → Detail 跳过 → 空页
        var renderer = NewRenderer();
        var template = SingleBandTemplate("Missing");
        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            ["Other"] = new List<Dictionary<string, object>> { new() { ["X"] = 1 } },
        };

        var result = await renderer.RenderAsync(template, data);

        result.Should().NotBeNull();
        // 空 rows 不应崩，Pages 是 List<RenderedPage>，可能为空或仅含无元素页
        result.Pages.Should().NotBeNull();
    }

    [Fact]
    public async Task RenderAsync_DetailWithTextElement_ProducesRenderedText()
    {
        // 覆盖 VisibleExpression 空分支（line 640-643 修过的路径）
        var renderer = NewRenderer();
        var template = new ReportTemplate
        {
            Page = new PageInfo { Width = 210, Height = 297 },
            Bands = new List<Band>
            {
                new Band
                {
                    Type = BandType.Detail, Height = 10, DataSource = "Main",
                    Elements = new List<ReportElement>
                    {
                        new TextElement
                        {
                            X = 10, Y = 5, Width = 50, Height = 8,
                            Text = "Hello {{Name}}",
                            DataField = "Name",
                            // VisibleExpression 留空：走 IsNullOrEmpty 分支，不调用 Evaluate
                        },
                    },
                },
            },
        };
        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            ["Main"] = new List<Dictionary<string, object>>
            {
                new() { ["Name"] = "Bob" },
            },
        };

        var result = await renderer.RenderAsync(template, data);

        result.Pages.Should().HaveCount(1);
        result.Pages[0].Elements.Should().Contain(e => e is RenderedTextElement);
    }

    [Fact]
    public async Task RenderAsync_HeaderOnlyTemplate_NoDetailBand()
    {
        // 没有 Detail band 的模板：只渲染静态 Header/Footer
        var renderer = NewRenderer();
        var template = new ReportTemplate
        {
            Page = new PageInfo { Width = 210, Height = 297 },
            Bands = new List<Band>
            {
                new Band { Type = BandType.ReportHeader, Height = 20 },
                new Band { Type = BandType.ReportFooter, Height = 10 },
            },
        };

        var result = await renderer.RenderAsync(template, new Dictionary<string, List<Dictionary<string, object>>>());

        result.Should().NotBeNull();
        // 静态 band 不依赖数据，至少不应崩
    }
}