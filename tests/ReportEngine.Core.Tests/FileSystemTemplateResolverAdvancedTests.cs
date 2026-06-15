using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ReportEngine.Core;
using ReportEngine.Core.Parsing;
using ReportEngine.Core.SubReports;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// FileSystemTemplateResolver 高级行为测试：
///   - Save + Resolve 往返
///   - ListTemplates
///   - ClearCache
///   - 缓存命中
///   - Exists
/// </summary>
public class FileSystemTemplateResolverAdvancedTests
{
    private readonly string _tempDir;

    public FileSystemTemplateResolverAdvancedTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"rpt_test_{System.Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    private void Cleanup()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    // ============== Save + Resolve ==============

    [Fact]
    public async Task Save_ThenResolve_ReturnsTemplate()
    {
        try
        {
            var resolver = new FileSystemTemplateResolver(_tempDir);
            var template = new ReportTemplate();
            template.Bands.Add(new Band { Type = BandType.Header, Height = 20 });

            resolver.Save(template, "test.rptx");

            var resolved = await resolver.ResolveAsync("test.rptx");
            Assert.NotNull(resolved);
            Assert.Single(resolved.Bands);
            Assert.Equal(BandType.Header, resolved.Bands[0].Type);
        }
        finally { Cleanup(); }
    }

    [Fact]
    public async Task Save_MultipleTemplates_AllResolvable()
    {
        try
        {
            var resolver = new FileSystemTemplateResolver(_tempDir);

            var t1 = new ReportTemplate();
            t1.Bands.Add(new Band { Type = BandType.Header, Height = 20 });
            resolver.Save(t1, "header.rptx");

            var t2 = new ReportTemplate();
            t2.Bands.Add(new Band { Type = BandType.Detail, Height = 30, DataSource = "ds" });
            t2.DataSources.Add(new DataSourceDef { Name = "ds" });
            resolver.Save(t2, "detail.rptx");

            var r1 = await resolver.ResolveAsync("header.rptx");
            var r2 = await resolver.ResolveAsync("detail.rptx");

            Assert.Single(r1.Bands);
            Assert.Single(r2.Bands);
            Assert.Equal(BandType.Header, r1.Bands[0].Type);
            Assert.Equal(BandType.Detail, r2.Bands[0].Type);
        }
        finally { Cleanup(); }
    }

    // ============== ListTemplates ==============

    [Fact]
    public void ListTemplates_EmptyDir_ReturnsEmpty()
    {
        try
        {
            var resolver = new FileSystemTemplateResolver(_tempDir);
            var list = resolver.ListTemplates();
            Assert.Empty(list);
        }
        finally { Cleanup(); }
    }

    [Fact]
    public void ListTemplates_AfterSave_ReturnsFiles()
    {
        try
        {
            var resolver = new FileSystemTemplateResolver(_tempDir);
            resolver.Save(new ReportTemplate(), "a.rptx");
            resolver.Save(new ReportTemplate(), "b.rptx");

            var list = resolver.ListTemplates();
            Assert.Equal(2, list.Count);
            Assert.Contains("a.rptx", list);
            Assert.Contains("b.rptx", list);
        }
        finally { Cleanup(); }
    }

    [Fact]
    public void ListTemplates_IgnoresNonRptxFiles()
    {
        try
        {
            var resolver = new FileSystemTemplateResolver(_tempDir);
            resolver.Save(new ReportTemplate(), "test.rptx");
            File.WriteAllText(Path.Combine(_tempDir, "readme.txt"), "hello");

            var list = resolver.ListTemplates();
            Assert.Single(list);
            Assert.Contains("test.rptx", list);
        }
        finally { Cleanup(); }
    }

    // ============== Exists ==============

    [Fact]
    public void Exists_NonExistent_ReturnsFalse()
    {
        try
        {
            var resolver = new FileSystemTemplateResolver(_tempDir);
            Assert.False(resolver.Exists("missing.rptx"));
        }
        finally { Cleanup(); }
    }

    [Fact]
    public void Exists_AfterSave_ReturnsTrue()
    {
        try
        {
            var resolver = new FileSystemTemplateResolver(_tempDir);
            resolver.Save(new ReportTemplate(), "test.rptx");
            Assert.True(resolver.Exists("test.rptx"));
        }
        finally { Cleanup(); }
    }

    // ============== ClearCache ==============

    [Fact]
    public async Task ClearCache_NextResolve_ReadsFromFile()
    {
        try
        {
            var resolver = new FileSystemTemplateResolver(_tempDir);
            var template = new ReportTemplate();
            template.Bands.Add(new Band { Type = BandType.Header, Height = 20 });
            resolver.Save(template, "test.rptx");

            // First resolve (populates cache)
            var r1 = await resolver.ResolveAsync("test.rptx");
            Assert.Single(r1.Bands);

            // Clear cache
            resolver.ClearCache();

            // Second resolve (reads from file)
            var r2 = await resolver.ResolveAsync("test.rptx");
            Assert.Single(r2.Bands);
        }
        finally { Cleanup(); }
    }

    // ============== Caching ==============

    [Fact]
    public async Task ResolveAsync_SecondCall_ReturnsCachedInstance()
    {
        try
        {
            var resolver = new FileSystemTemplateResolver(_tempDir);
            resolver.Save(new ReportTemplate(), "test.rptx");

            var r1 = await resolver.ResolveAsync("test.rptx");
            var r2 = await resolver.ResolveAsync("test.rptx");

            // Should return same cached instance
            Assert.Same(r1, r2);
        }
        finally { Cleanup(); }
    }

    // ============== Constructor ==============

    [Fact]
    public void Constructor_CreatesDirectoryIfNotExists()
    {
        var newDir = Path.Combine(_tempDir, "subdir");
        try
        {
            Assert.False(Directory.Exists(newDir));
            var resolver = new FileSystemTemplateResolver(newDir);
            Assert.True(Directory.Exists(newDir));
        }
        finally
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }
    }

    // ============== ResolveAsync Not Found ==============

    [Fact]
    public async Task ResolveAsync_MissingFile_ThrowsTemplateNotFoundException()
    {
        try
        {
            var resolver = new FileSystemTemplateResolver(_tempDir);
            var ex = await Assert.ThrowsAsync<TemplateNotFoundException>(
                () => resolver.ResolveAsync("missing.rptx"));
            Assert.Equal("missing.rptx", ex.TemplateRef);
        }
        finally { Cleanup(); }
    }

    // ============== Save overwrites ==============

    [Fact]
    public async Task Save_OverwritesExistingFile()
    {
        try
        {
            var resolver = new FileSystemTemplateResolver(_tempDir);

            var t1 = new ReportTemplate();
            t1.Bands.Add(new Band { Type = BandType.Header, Height = 20 });
            resolver.Save(t1, "test.rptx");

            var t2 = new ReportTemplate();
            t2.Bands.Add(new Band { Type = BandType.Footer, Height = 15 });
            resolver.Save(t2, "test.rptx");

            resolver.ClearCache();
            var resolved = await resolver.ResolveAsync("test.rptx");
            Assert.Single(resolved.Bands);
            Assert.Equal(BandType.Footer, resolved.Bands[0].Type);
        }
        finally { Cleanup(); }
    }
}
