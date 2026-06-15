using ReportEngine.Core;
using ReportEngine.Core.Parsing;
using ReportEngine.Core.SubReports;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// FileSystemTemplateResolver 扩展行为测试：Save / ListTemplates / ClearCache / Exists
/// </summary>
public class FileSystemTemplateResolverExtendedTests : IDisposable
{
    private readonly string _tempDir;

    public FileSystemTemplateResolverExtendedTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "rpt_test_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    // ============== Save ==============

    [Fact]
    public void Save_CreatesFile()
    {
        var resolver = new FileSystemTemplateResolver(_tempDir);
        var template = new ReportTemplate();
        template.Bands.Add(new Band { Type = BandType.Detail, Height = 100 });
        template.DataSources.Add(new DataSourceDef { Name = "ds1" });

        resolver.Save(template, "test.rptx");

        Assert.True(File.Exists(Path.Combine(_tempDir, "test.rptx")));
    }

    [Fact]
    public void Save_CanResolve_AfterSave()
    {
        var resolver = new FileSystemTemplateResolver(_tempDir);
        var template = new ReportTemplate();
        template.Bands.Add(new Band { Type = BandType.Detail, Height = 100 });
        template.DataSources.Add(new DataSourceDef { Name = "ds1" });

        resolver.Save(template, "saved.rptx");

        Assert.True(resolver.Exists("saved.rptx"));
    }

    [Fact]
    public async Task Save_ThenResolve_ReturnsCached()
    {
        var resolver = new FileSystemTemplateResolver(_tempDir);
        var template = new ReportTemplate();
        template.Bands.Add(new Band { Type = BandType.Detail, Height = 50 });
        template.DataSources.Add(new DataSourceDef { Name = "ds1" });

        resolver.Save(template, "cached.rptx");
        var resolved = await resolver.ResolveAsync("cached.rptx");

        Assert.NotNull(resolved);
        Assert.Single(resolved.Bands);
    }

    // ============== ListTemplates ==============

    [Fact]
    public void ListTemplates_EmptyDir_ReturnsEmpty()
    {
        var resolver = new FileSystemTemplateResolver(_tempDir);
        var list = resolver.ListTemplates();
        Assert.Empty(list);
    }

    [Fact]
    public void ListTemplates_AfterSave_ReturnsFile()
    {
        var resolver = new FileSystemTemplateResolver(_tempDir);
        var template = new ReportTemplate();
        template.Bands.Add(new Band { Type = BandType.Detail, Height = 100 });
        template.DataSources.Add(new DataSourceDef { Name = "ds1" });

        resolver.Save(template, "report1.rptx");

        var list = resolver.ListTemplates();
        Assert.Single(list);
        Assert.Equal("report1.rptx", list[0]);
    }

    [Fact]
    public void ListTemplates_MultipleFiles_ReturnsAll()
    {
        var resolver = new FileSystemTemplateResolver(_tempDir);
        var template = new ReportTemplate();
        template.Bands.Add(new Band { Type = BandType.Detail, Height = 100 });
        template.DataSources.Add(new DataSourceDef { Name = "ds1" });

        resolver.Save(template, "a.rptx");
        resolver.Save(template, "b.rptx");
        resolver.Save(template, "c.rptx");

        var list = resolver.ListTemplates();
        Assert.Equal(3, list.Count);
    }

    // ============== ClearCache ==============

    [Fact]
    public async Task ClearCache_ForcesReload()
    {
        var resolver = new FileSystemTemplateResolver(_tempDir);
        var template = new ReportTemplate();
        template.Bands.Add(new Band { Type = BandType.Detail, Height = 100 });
        template.DataSources.Add(new DataSourceDef { Name = "ds1" });

        resolver.Save(template, "test.rptx");

        // 第一次解析（缓存）
        var first = await resolver.ResolveAsync("test.rptx");
        Assert.NotNull(first);

        // 清缓存
        resolver.ClearCache();

        // 第二次解析（从文件重新加载）
        var second = await resolver.ResolveAsync("test.rptx");
        Assert.NotNull(second);
    }

    // ============== Exists ==============

    [Fact]
    public void Exists_NonExistent_ReturnsFalse()
    {
        var resolver = new FileSystemTemplateResolver(_tempDir);
        Assert.False(resolver.Exists("nonexistent.rptx"));
    }

    [Fact]
    public void Exists_AfterSave_ReturnsTrue()
    {
        var resolver = new FileSystemTemplateResolver(_tempDir);
        var template = new ReportTemplate();
        template.Bands.Add(new Band { Type = BandType.Detail, Height = 100 });
        template.DataSources.Add(new DataSourceDef { Name = "ds1" });

        resolver.Save(template, "exists.rptx");
        Assert.True(resolver.Exists("exists.rptx"));
    }

    // ============== ResolveAsync ==============

    [Fact]
    public async Task ResolveAsync_NonExistent_ThrowsTemplateNotFoundException()
    {
        var resolver = new FileSystemTemplateResolver(_tempDir);
        await Assert.ThrowsAsync<TemplateNotFoundException>(
            () => resolver.ResolveAsync("missing.rptx"));
    }

    [Fact]
    public async Task ResolveAsync_CachesResult()
    {
        var resolver = new FileSystemTemplateResolver(_tempDir);
        var template = new ReportTemplate();
        template.Bands.Add(new Band { Type = BandType.Detail, Height = 100 });
        template.DataSources.Add(new DataSourceDef { Name = "ds1" });

        resolver.Save(template, "cache_test.rptx");

        var first = await resolver.ResolveAsync("cache_test.rptx");
        var second = await resolver.ResolveAsync("cache_test.rptx");

        // 缓存命中应返回同一对象
        Assert.Same(first, second);
    }

    // ============== 构造器 ==============

    [Fact]
    public void Constructor_CreatesDir_IfNotExists()
    {
        var newDir = Path.Combine(_tempDir, "subdir_" + Guid.NewGuid().ToString("N")[..8]);
        Assert.False(Directory.Exists(newDir));

        var resolver = new FileSystemTemplateResolver(newDir);

        Assert.True(Directory.Exists(newDir));
    }
}

/// <summary>
/// CompositeTemplateResolver 行为测试
/// </summary>
public class CompositeTemplateResolverExtendedTests
{
    // ============== AddResolver ==============

    [Fact]
    public void AddResolver_ReturnsSelf_ForChaining()
    {
        var composite = new CompositeTemplateResolver();
        var result = composite.AddResolver(new InMemoryResolver());
        Assert.Same(composite, result);
    }

    [Fact]
    public void AddResolver_Multiple_Works()
    {
        var composite = new CompositeTemplateResolver();
        composite.AddResolver(new InMemoryResolver());
        composite.AddResolver(new InMemoryResolver());
        // 不抛异常即成功
    }

    // ============== Exists ==============

    [Fact]
    public void Exists_NoResolvers_ReturnsFalse()
    {
        var composite = new CompositeTemplateResolver();
        Assert.False(composite.Exists("any.rptx"));
    }

    [Fact]
    public void Exists_FirstResolverHas_ReturnsTrue()
    {
        var r1 = new InMemoryResolver();
        r1.AddTemplate("header.rptx", CreateTemplate());

        var composite = new CompositeTemplateResolver();
        composite.AddResolver(r1);

        Assert.True(composite.Exists("header.rptx"));
    }

    [Fact]
    public void Exists_SecondResolverHas_ReturnsTrue()
    {
        var r1 = new InMemoryResolver();
        var r2 = new InMemoryResolver();
        r2.AddTemplate("footer.rptx", CreateTemplate());

        var composite = new CompositeTemplateResolver();
        composite.AddResolver(r1);
        composite.AddResolver(r2);

        Assert.True(composite.Exists("footer.rptx"));
    }

    [Fact]
    public void Exists_NoneHas_ReturnsFalse()
    {
        var r1 = new InMemoryResolver();
        var r2 = new InMemoryResolver();

        var composite = new CompositeTemplateResolver();
        composite.AddResolver(r1);
        composite.AddResolver(r2);

        Assert.False(composite.Exists("missing.rptx"));
    }

    // ============== ResolveAsync ==============

    [Fact]
    public async Task ResolveAsync_FirstResolverHas_ReturnsIt()
    {
        var r1 = new InMemoryResolver();
        var template = CreateTemplate();
        r1.AddTemplate("test.rptx", template);

        var composite = new CompositeTemplateResolver();
        composite.AddResolver(r1);

        var resolved = await composite.ResolveAsync("test.rptx");
        Assert.Same(template, resolved);
    }

    [Fact]
    public async Task ResolveAsync_FallsToSecondResolver()
    {
        var r1 = new InMemoryResolver();
        var r2 = new InMemoryResolver();
        var template = CreateTemplate();
        r2.AddTemplate("only_in_r2.rptx", template);

        var composite = new CompositeTemplateResolver();
        composite.AddResolver(r1);
        composite.AddResolver(r2);

        var resolved = await composite.ResolveAsync("only_in_r2.rptx");
        Assert.Same(template, resolved);
    }

    [Fact]
    public async Task ResolveAsync_NoneHas_ThrowsTemplateNotFoundException()
    {
        var composite = new CompositeTemplateResolver();
        composite.AddResolver(new InMemoryResolver());

        await Assert.ThrowsAsync<TemplateNotFoundException>(
            () => composite.ResolveAsync("missing.rptx"));
    }

    [Fact]
    public async Task ResolveAsync_NoResolvers_ThrowsTemplateNotFoundException()
    {
        var composite = new CompositeTemplateResolver();
        await Assert.ThrowsAsync<TemplateNotFoundException>(
            () => composite.ResolveAsync("any.rptx"));
    }

    // ============== 辅助 ==============

    private static ReportTemplate CreateTemplate()
    {
        var t = new ReportTemplate();
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 100 });
        t.DataSources.Add(new DataSourceDef { Name = "ds" });
        return t;
    }

    /// <summary>
    /// 内存模板解析器（测试用）
    /// </summary>
    private class InMemoryResolver : ITemplateResolver
    {
        private readonly Dictionary<string, ReportTemplate> _templates = new();

        public void AddTemplate(string name, ReportTemplate template)
            => _templates[name] = template;

        public bool Exists(string templateRef)
            => _templates.ContainsKey(templateRef);

        public Task<ReportTemplate> ResolveAsync(string templateRef)
        {
            if (_templates.TryGetValue(templateRef, out var t))
                return Task.FromResult(t);
            throw new TemplateNotFoundException(templateRef, "(memory)");
        }
    }
}
