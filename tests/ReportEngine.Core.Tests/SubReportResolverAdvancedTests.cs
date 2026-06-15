using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ReportEngine.Core;
using ReportEngine.Core.Parsing;
using ReportEngine.Core.SubReports;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// SubReport 解析器与 TemplateParser 高级特性测试：
///   - FileSystemTemplateResolver（Save/List/ClearCache）
///   - CompositeTemplateResolver（多 resolver 组合）
///   - TemplateParser（坏 JSON 处理 + 空模板往返）
/// </summary>
public class SubReportResolverAdvancedTests : IDisposable
{
    private readonly string _tempDir;

    public SubReportResolverAdvancedTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "report_engine_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true);
    }

    [Fact]
    public async Task FileSystemResolver_SaveAndResolve_RoundTrip()
    {
        var resolver = new FileSystemTemplateResolver(_tempDir);
        var t = new ReportTemplate
        {
            Page = new PageInfo { Width = 100, Height = 50, Margin = new Margin() },
            Bands = new List<Band> { new Band { Type = BandType.Detail, Height = 10 } },
        };
        resolver.Save(t, "test.rptx");
        Assert.True(File.Exists(Path.Combine(_tempDir, "test.rptx")));
        Assert.True(resolver.Exists("test.rptx"));
        var loaded = await resolver.ResolveAsync("test.rptx");
        Assert.Equal(100, loaded.Page.Width);
        Assert.Single(loaded.Bands);
    }

    [Fact]
    public async Task FileSystemResolver_ResolveUnknown_Throws()
    {
        var resolver = new FileSystemTemplateResolver(_tempDir);
        await Assert.ThrowsAsync<TemplateNotFoundException>(() => resolver.ResolveAsync("nonexistent.rptx"));
    }

    [Fact]
    public void FileSystemResolver_ListTemplates_AfterSave()
    {
        var resolver = new FileSystemTemplateResolver(_tempDir);
        resolver.Save(new ReportTemplate { Page = new PageInfo { Width = 100, Height = 50, Margin = new Margin() } }, "a.rptx");
        resolver.Save(new ReportTemplate { Page = new PageInfo { Width = 100, Height = 50, Margin = new Margin() } }, "b.rptx");
        var list = resolver.ListTemplates();
        Assert.Equal(2, list.Count);
        Assert.Contains("a.rptx", list);
        Assert.Contains("b.rptx", list);
    }

    [Fact]
    public async Task FileSystemResolver_ClearCache_ForcesReload()
    {
        var resolver = new FileSystemTemplateResolver(_tempDir);
        var t = new ReportTemplate
        {
            Page = new PageInfo { Width = 100, Height = 50, Margin = new Margin() },
            Bands = new List<Band> { new Band { Type = BandType.Detail, Height = 10 } },
        };
        resolver.Save(t, "x.rptx");
        var first = await resolver.ResolveAsync("x.rptx");
        resolver.ClearCache();
        var second = await resolver.ResolveAsync("x.rptx");
        Assert.NotSame(first, second);
    }

    [Fact]
    public async Task FileSystemResolver_AbsolutePath_BypassesDirectory()
    {
        var resolver = new FileSystemTemplateResolver(_tempDir);
        var absPath = Path.Combine(_tempDir, "abs.rptx");
        var t = new ReportTemplate
        {
            Page = new PageInfo { Width = 100, Height = 50, Margin = new Margin() },
            Bands = new List<Band> { new Band { Type = BandType.Detail, Height = 10 } },
        };
        File.WriteAllText(absPath, new TemplateParser().Serialize(t));
        var loaded = await resolver.ResolveAsync(absPath);
        Assert.Equal(100, loaded.Page.Width);
    }

    [Fact]
    public async Task CompositeResolver_TriesResolversInOrder()
    {
        var first = new InMemoryTemplateResolver(
            new ReportTemplate { Page = new PageInfo { Width = 50, Height = 50, Margin = new Margin() } });
        var second = new InMemoryTemplateResolver(
            new ReportTemplate { Page = new PageInfo { Width = 200, Height = 200, Margin = new Margin() } });

        var composite = new CompositeTemplateResolver()
            .AddResolver(first)
            .AddResolver(second);

        var t = await composite.ResolveAsync("anything");
        // 两个都 Exists=true，但 Composite 取第一个命中
        Assert.Equal(50, t.Page.Width);
    }

    [Fact]
    public void CompositeResolver_Empty_ReturnsEmptyTemplate()
    {
        var composite = new CompositeTemplateResolver();
        Assert.False(composite.Exists("anything"));
    }

    [Fact]
    public async Task TemplateParser_EmptyTemplate_RoundTrip()
    {
        var parser = new TemplateParser();
        var t = new ReportTemplate
        {
            Page = new PageInfo { Width = 100, Height = 50, Margin = new Margin() },
            Bands = new List<Band> { new Band { Type = BandType.Detail, Height = 10 } },
            DataSources = new List<DataSourceDef>(),
        };
        var json = parser.Serialize(t);
        var parsed = parser.Parse(json);
        Assert.Equal(100, parsed.Page.Width);
        Assert.Single(parsed.Bands);
    }

    [Fact]
    public void TemplateParser_MalformedJson_ThrowsTemplateParseException()
    {
        var parser = new TemplateParser();
        Assert.Throws<TemplateParseException>(() => parser.Parse("{ not valid json"));
    }

    [Fact]
    public void TemplateParser_EmptyString_ThrowsTemplateParseException()
    {
        var parser = new TemplateParser();
        Assert.Throws<TemplateParseException>(() => parser.Parse(""));
    }

    [Fact]
    public void TemplateParser_SerializeThenParse_PreservesAllElementTypes()
    {
        var parser = new TemplateParser();
        var t = new ReportTemplate
        {
            Page = new PageInfo { Width = 200, Height = 200, Margin = new Margin() },
            Bands = new List<Band>
            {
                new Band
                {
                    Type = BandType.Detail, Height = 100,
                    Elements = new List<ReportElement>
                    {
                        new TextElement { X = 1, Y = 2, Width = 30, Height = 5, Text = "Hello" },
                        new BarcodeElement { X = 0, Y = 10, Width = 40, Height = 15, Value = "12345", Format = BarcodeFormat.Code128 },
                        new LineElement { X = 0, Y = 30, Width = 50, Height = 0, LineWidth = 1, Direction = LineDirection.Horizontal },
                        new ShapeElement { X = 0, Y = 40, Width = 20, Height = 20, Shape = ShapeType.Rectangle },
                        new ImageElement { X = 50, Y = 0, Width = 30, Height = 30, Source = "logo.png" },
                    },
                },
            },
        };
        var json = parser.Serialize(t);
        var loaded = parser.Parse(json);
        var elems = loaded.Bands[0].Elements;
        Assert.Equal(5, elems.Count);
        Assert.IsType<TextElement>(elems[0]);
        Assert.IsType<BarcodeElement>(elems[1]);
        Assert.IsType<LineElement>(elems[2]);
        Assert.IsType<ShapeElement>(elems[3]);
        Assert.IsType<ImageElement>(elems[4]);
    }
}
