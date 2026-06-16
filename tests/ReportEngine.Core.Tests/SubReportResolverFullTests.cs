using ReportEngine.Core;
using ReportEngine.Core.Parsing;
using ReportEngine.Core.SubReports;
using Xunit;

namespace ReportEngine.Core.Tests;

// ─────────────────────────────────────────────────────────────────────────────
// TemplateNotFoundException 测试
// ─────────────────────────────────────────────────────────────────────────────

public class TemplateNotFoundExceptionFullTests
{
    [Fact]
    public void TemplateNotFoundException_HasTemplateRef()
    {
        var ex = new TemplateNotFoundException("sub.rptx", "/templates");
        Assert.Equal("sub.rptx", ex.TemplateRef);
    }

    [Fact]
    public void TemplateNotFoundException_HasSearchPath()
    {
        var ex = new TemplateNotFoundException("sub.rptx", "/templates");
        Assert.Equal("/templates", ex.SearchPath);
    }

    [Fact]
    public void TemplateNotFoundException_Message_ContainsTemplateRef()
    {
        var ex = new TemplateNotFoundException("sub.rptx", "/templates");
        Assert.Contains("sub.rptx", ex.Message);
    }

    [Fact]
    public void TemplateNotFoundException_Message_ContainsSearchPath()
    {
        var ex = new TemplateNotFoundException("sub.rptx", "/templates");
        Assert.Contains("/templates", ex.Message);
    }

    [Fact]
    public void TemplateNotFoundException_IsException()
    {
        var ex = new TemplateNotFoundException("sub.rptx", "/templates");
        Assert.IsAssignableFrom<Exception>(ex);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// FileSystemTemplateResolver 测试
// ─────────────────────────────────────────────────────────────────────────────

public class FileSystemTemplateResolverTests
{
    private static string CreateTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "rpt_test_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static void CleanupTempDir(string dir)
    {
        if (Directory.Exists(dir))
            Directory.Delete(dir, true);
    }

    [Fact]
    public void FileSystemTemplateResolver_CreateDir_IfNotExists()
    {
        var dir = Path.Combine(Path.GetTempPath(), "rpt_new_" + Guid.NewGuid().ToString("N")[..8]);
        Assert.False(Directory.Exists(dir));
        var resolver = new FileSystemTemplateResolver(dir);
        Assert.True(Directory.Exists(dir));
        CleanupTempDir(dir);
    }

    [Fact]
    public void FileSystemTemplateResolver_Exists_ReturnsFalse_WhenFileMissing()
    {
        var dir = CreateTempDir();
        try
        {
            var resolver = new FileSystemTemplateResolver(dir);
            Assert.False(resolver.Exists("missing.rptx"));
        }
        finally { CleanupTempDir(dir); }
    }

    [Fact]
    public void FileSystemTemplateResolver_Exists_ReturnsTrue_WhenFilePresent()
    {
        var dir = CreateTempDir();
        try
        {
            File.WriteAllText(Path.Combine(dir, "test.rptx"), "{}");
            var resolver = new FileSystemTemplateResolver(dir);
            Assert.True(resolver.Exists("test.rptx"));
        }
        finally { CleanupTempDir(dir); }
    }

    [Fact]
    public async Task FileSystemTemplateResolver_ResolveAsync_Throws_WhenMissing()
    {
        var dir = CreateTempDir();
        try
        {
            var resolver = new FileSystemTemplateResolver(dir);
            await Assert.ThrowsAsync<TemplateNotFoundException>(() => resolver.ResolveAsync("missing.rptx"));
        }
        finally { CleanupTempDir(dir); }
    }

    [Fact]
    public async Task FileSystemTemplateResolver_ResolveAsync_ReturnsTemplate_WhenPresent()
    {
        var dir = CreateTempDir();
        try
        {
            var parser = new TemplateParser();
            var template = new ReportTemplate();
            template.Bands.Add(new Band { Type = BandType.Header, Height = 20 });
            var json = parser.Serialize(template);
            File.WriteAllText(Path.Combine(dir, "test.rptx"), json);

            var resolver = new FileSystemTemplateResolver(dir);
            var result = await resolver.ResolveAsync("test.rptx");
            Assert.NotNull(result);
            Assert.Single(result.Bands);
        }
        finally { CleanupTempDir(dir); }
    }

    [Fact]
    public async Task FileSystemTemplateResolver_ResolveAsync_CachesResult()
    {
        var dir = CreateTempDir();
        try
        {
            var parser = new TemplateParser();
            var template = new ReportTemplate();
            template.Bands.Add(new Band { Type = BandType.Header, Height = 20 });
            var json = parser.Serialize(template);
            File.WriteAllText(Path.Combine(dir, "cached.rptx"), json);

            var resolver = new FileSystemTemplateResolver(dir);
            var r1 = await resolver.ResolveAsync("cached.rptx");
            var r2 = await resolver.ResolveAsync("cached.rptx");
            Assert.Same(r1, r2);
        }
        finally { CleanupTempDir(dir); }
    }

    [Fact]
    public void FileSystemTemplateResolver_Save_WritesFile()
    {
        var dir = CreateTempDir();
        try
        {
            var resolver = new FileSystemTemplateResolver(dir);
            var template = new ReportTemplate();
            template.Bands.Add(new Band { Type = BandType.Header, Height = 30 });
            resolver.Save(template, "saved.rptx");

            Assert.True(File.Exists(Path.Combine(dir, "saved.rptx")));
        }
        finally { CleanupTempDir(dir); }
    }

    [Fact]
    public void FileSystemTemplateResolver_ListTemplates_ReturnsFiles()
    {
        var dir = CreateTempDir();
        try
        {
            File.WriteAllText(Path.Combine(dir, "a.rptx"), "{}");
            File.WriteAllText(Path.Combine(dir, "b.rptx"), "{}");
            File.WriteAllText(Path.Combine(dir, "c.txt"), "not a template");

            var resolver = new FileSystemTemplateResolver(dir);
            var list = resolver.ListTemplates();
            Assert.Equal(2, list.Count);
            Assert.Contains("a.rptx", list);
            Assert.Contains("b.rptx", list);
        }
        finally { CleanupTempDir(dir); }
    }

    [Fact]
    public void FileSystemTemplateResolver_ListTemplates_Empty_WhenNoFiles()
    {
        var dir = CreateTempDir();
        try
        {
            var resolver = new FileSystemTemplateResolver(dir);
            var list = resolver.ListTemplates();
            Assert.Empty(list);
        }
        finally { CleanupTempDir(dir); }
    }

    [Fact]
    public void FileSystemTemplateResolver_ClearCache_DoesNotThrow()
    {
        var dir = CreateTempDir();
        try
        {
            var resolver = new FileSystemTemplateResolver(dir);
            resolver.ClearCache();
        }
        finally { CleanupTempDir(dir); }
    }

    [Fact]
    public async Task FileSystemTemplateResolver_ResolveAsync_AbsolutePath()
    {
        var dir = CreateTempDir();
        try
        {
            var parser = new TemplateParser();
            var template = new ReportTemplate();
            template.Bands.Add(new Band { Type = BandType.Header, Height = 15 });
            var json = parser.Serialize(template);
            var filePath = Path.Combine(dir, "abs.rptx");
            File.WriteAllText(filePath, json);

            var resolver = new FileSystemTemplateResolver(dir);
            var result = await resolver.ResolveAsync(filePath);
            Assert.NotNull(result);
        }
        finally { CleanupTempDir(dir); }
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// CompositeTemplateResolver 测试
// ─────────────────────────────────────────────────────────────────────────────

public class CompositeTemplateResolverTests
{
    private class InMemoryResolver : ITemplateResolver
    {
        private readonly Dictionary<string, ReportTemplate> _templates = new();

        public void Add(string name, ReportTemplate template) => _templates[name] = template;
        public bool Exists(string templateRef) => _templates.ContainsKey(templateRef);
        public Task<ReportTemplate> ResolveAsync(string templateRef) =>
            Task.FromResult(_templates[templateRef]);
    }

    [Fact]
    public void CompositeTemplateResolver_AddResolver_ReturnsSelf()
    {
        var composite = new CompositeTemplateResolver();
        var result = composite.AddResolver(new InMemoryResolver());
        Assert.Same(composite, result);
    }

    [Fact]
    public void CompositeTemplateResolver_Exists_ReturnsFalse_WhenEmpty()
    {
        var composite = new CompositeTemplateResolver();
        Assert.False(composite.Exists("any.rptx"));
    }

    [Fact]
    public void CompositeTemplateResolver_Exists_ReturnsTrue_WhenResolverHasIt()
    {
        var mem = new InMemoryResolver();
        mem.Add("test.rptx", new ReportTemplate());
        var composite = new CompositeTemplateResolver().AddResolver(mem);
        Assert.True(composite.Exists("test.rptx"));
    }

    [Fact]
    public void CompositeTemplateResolver_Exists_ReturnsFalse_WhenNoResolverHasIt()
    {
        var mem = new InMemoryResolver();
        mem.Add("test.rptx", new ReportTemplate());
        var composite = new CompositeTemplateResolver().AddResolver(mem);
        Assert.False(composite.Exists("missing.rptx"));
    }

    [Fact]
    public async Task CompositeTemplateResolver_ResolveAsync_ReturnsFromFirstResolver()
    {
        var mem = new InMemoryResolver();
        var t = new ReportTemplate();
        t.Bands.Add(new Band { Type = BandType.Header, Height = 10 });
        mem.Add("test.rptx", t);

        var composite = new CompositeTemplateResolver().AddResolver(mem);
        var result = await composite.ResolveAsync("test.rptx");
        Assert.Single(result.Bands);
    }

    [Fact]
    public async Task CompositeTemplateResolver_ResolveAsync_Throws_WhenNotFound()
    {
        var mem = new InMemoryResolver();
        var composite = new CompositeTemplateResolver().AddResolver(mem);
        await Assert.ThrowsAsync<TemplateNotFoundException>(() => composite.ResolveAsync("missing.rptx"));
    }

    [Fact]
    public async Task CompositeTemplateResolver_ResolveAsync_FallsThrough()
    {
        var mem1 = new InMemoryResolver();
        var mem2 = new InMemoryResolver();
        var t = new ReportTemplate();
        t.Bands.Add(new Band { Type = BandType.Detail, Height = 20 });
        mem2.Add("deep.rptx", t);

        var composite = new CompositeTemplateResolver()
            .AddResolver(mem1)
            .AddResolver(mem2);
        var result = await composite.ResolveAsync("deep.rptx");
        Assert.Single(result.Bands);
    }

    [Fact]
    public async Task CompositeTemplateResolver_ResolveAsync_FirstMatch_Wins()
    {
        var mem1 = new InMemoryResolver();
        var mem2 = new InMemoryResolver();
        var t1 = new ReportTemplate { Version = "1.0" };
        var t2 = new ReportTemplate { Version = "2.0" };
        mem1.Add("same.rptx", t1);
        mem2.Add("same.rptx", t2);

        var composite = new CompositeTemplateResolver()
            .AddResolver(mem1)
            .AddResolver(mem2);
        var result = await composite.ResolveAsync("same.rptx");
        Assert.Equal("1.0", result.Version);
    }
}
