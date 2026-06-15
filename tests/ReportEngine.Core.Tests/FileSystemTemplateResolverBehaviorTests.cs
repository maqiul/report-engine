using System;
using System.IO;
using System.Threading.Tasks;
using ReportEngine.Core.Parsing;
using ReportEngine.Core.SubReports;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// FileSystemTemplateResolver 行为测试：
///   - ResolveAsync 从文件解析
///   - Exists 检查文件存在
///   - 缓存行为
///   - 文件不存在抛异常
/// </summary>
public class FileSystemTemplateResolverBehaviorTests : IDisposable
{
    private readonly string _tempDir;

    public FileSystemTemplateResolverBehaviorTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "rpt_test_" + Guid.NewGuid().ToString("N").Substring(0, 8));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private void WriteTemplate(string filename, string json)
    {
        File.WriteAllText(Path.Combine(_tempDir, filename), json);
    }

    [Fact]
    public void Constructor_CreatesDirectoryIfNotExists()
    {
        var newDir = Path.Combine(_tempDir, "subdir");
        Assert.False(Directory.Exists(newDir));
        var resolver = new FileSystemTemplateResolver(newDir);
        Assert.True(Directory.Exists(newDir));
    }

    [Fact]
    public async Task ResolveAsync_SimpleTemplate()
    {
        WriteTemplate("sub.rpt", @"{ ""bands"": [{ ""type"": ""header"", ""height"": 20 }] }");
        var resolver = new FileSystemTemplateResolver(_tempDir);
        var t = await resolver.ResolveAsync("sub.rpt");
        Assert.NotNull(t);
        Assert.Single(t.Bands);
    }

    [Fact]
    public async Task ResolveAsync_FileNotFound_ThrowsTemplateNotFoundException()
    {
        var resolver = new FileSystemTemplateResolver(_tempDir);
        await Assert.ThrowsAsync<TemplateNotFoundException>(() => resolver.ResolveAsync("nonexistent.rpt"));
    }

    [Fact]
    public void Exists_TrueWhenFileExists()
    {
        WriteTemplate("exists.rpt", @"{ ""bands"": [{ ""type"": ""header"", ""height"": 10 }] }");
        var resolver = new FileSystemTemplateResolver(_tempDir);
        Assert.True(resolver.Exists("exists.rpt"));
    }

    [Fact]
    public void Exists_FalseWhenFileNotExists()
    {
        var resolver = new FileSystemTemplateResolver(_tempDir);
        Assert.False(resolver.Exists("no_such_file.rpt"));
    }

    [Fact]
    public async Task ResolveAsync_CachesResult()
    {
        WriteTemplate("cached.rpt", @"{ ""bands"": [{ ""type"": ""header"", ""height"": 20 }] }");
        var resolver = new FileSystemTemplateResolver(_tempDir);
        var t1 = await resolver.ResolveAsync("cached.rpt");
        var t2 = await resolver.ResolveAsync("cached.rpt");
        Assert.Same(t1, t2); // 同一对象引用 = 缓存生效
    }

    [Fact]
    public async Task ResolveAsync_AbsolutePath()
    {
        var fullPath = Path.Combine(_tempDir, "absolute.rpt");
        File.WriteAllText(fullPath, @"{ ""bands"": [{ ""type"": ""detail"", ""height"": 15 }] }");
        var resolver = new FileSystemTemplateResolver(_tempDir);
        var t = await resolver.ResolveAsync(fullPath);
        Assert.NotNull(t);
        Assert.Equal(BandType.Detail, t.Bands[0].Type);
    }

    [Fact]
    public void Exists_AbsolutePath_True()
    {
        var fullPath = Path.Combine(_tempDir, "abs.rpt");
        File.WriteAllText(fullPath, @"{ ""bands"": [{ ""type"": ""header"", ""height"": 10 }] }");
        var resolver = new FileSystemTemplateResolver(_tempDir);
        Assert.True(resolver.Exists(fullPath));
    }

    [Fact]
    public void Exists_AbsolutePath_False()
    {
        var resolver = new FileSystemTemplateResolver(_tempDir);
        Assert.False(resolver.Exists(Path.Combine(_tempDir, "nope.rpt")));
    }

    [Fact]
    public async Task ResolveAsync_MultipleTemplates()
    {
        WriteTemplate("a.rpt", @"{ ""bands"": [{ ""type"": ""header"", ""height"": 10 }] }");
        WriteTemplate("b.rpt", @"{ ""bands"": [{ ""type"": ""detail"", ""height"": 20 }] }");
        WriteTemplate("c.rpt", @"{ ""bands"": [{ ""type"": ""footer"", ""height"": 15 }] }");

        var resolver = new FileSystemTemplateResolver(_tempDir);
        var ta = await resolver.ResolveAsync("a.rpt");
        var tb = await resolver.ResolveAsync("b.rpt");
        var tc = await resolver.ResolveAsync("c.rpt");

        Assert.Equal(BandType.Header, ta.Bands[0].Type);
        Assert.Equal(BandType.Detail, tb.Bands[0].Type);
        Assert.Equal(BandType.Footer, tc.Bands[0].Type);
    }

    [Fact]
    public async Task ResolveAsync_InvalidJson_ThrowsTemplateParseException()
    {
        WriteTemplate("bad.rpt", @"{ invalid json }");
        var resolver = new FileSystemTemplateResolver(_tempDir);
        await Assert.ThrowsAsync<TemplateParseException>(() => resolver.ResolveAsync("bad.rpt"));
    }

    [Fact]
    public async Task ResolveAsync_ComplexTemplate()
    {
        var json = @"{
            ""page"": { ""width"": 297, ""height"": 210 },
            ""dataSources"": [{ ""name"": ""ds1"" }],
            ""bands"": [
                { ""type"": ""header"", ""height"": 25, ""elements"": [
                    { ""type"": ""text"", ""text"": ""Title"", ""x"": 10, ""y"": 5, ""width"": 80, ""height"": 15 }
                ]},
                { ""type"": ""detail"", ""height"": 15, ""dataSource"": ""ds1"" }
            ]
        }";
        WriteTemplate("complex.rpt", json);
        var resolver = new FileSystemTemplateResolver(_tempDir);
        var t = await resolver.ResolveAsync("complex.rpt");

        Assert.Equal(297, t.Page.Width);
        Assert.Equal(210, t.Page.Height);
        Assert.Single(t.DataSources);
        Assert.Equal(2, t.Bands.Count);
    }
}
