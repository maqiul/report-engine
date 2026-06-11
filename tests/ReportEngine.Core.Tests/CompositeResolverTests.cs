using FluentAssertions;
using ReportEngine.Core.Parsing;
using ReportEngine.Core.SubReports;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// CompositeTemplateResolver 行为测试：
///   - 空 composite: 抛 TemplateNotFoundException
///   - 单 resolver 命中: 走该 resolver
///   - 多 resolver 链: 第一个 Exists 命中就返回
///   - 都不命中: 抛 TemplateNotFoundException (searchPath = "(composite)")
///   - Exists: 任意 resolver 命中即返回 true
///   - 短路: 第一个 resolver 命中时第二个不会被 ResolveAsync
/// </summary>
public class CompositeResolverTests : IDisposable
{
    private readonly string _tempDir1;
    private readonly string _tempDir2;

    public CompositeResolverTests()
    {
        _tempDir1 = Path.Combine(Path.GetTempPath(), "RE_C1_" + Guid.NewGuid().ToString("N"));
        _tempDir2 = Path.Combine(Path.GetTempPath(), "RE_C2_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir1);
        Directory.CreateDirectory(_tempDir2);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir1)) Directory.Delete(_tempDir1, recursive: true);
        if (Directory.Exists(_tempDir2)) Directory.Delete(_tempDir2, recursive: true);
    }

    private static string MinimalTemplateJson(string name) => """
    {
      "version": "1.0",
      "page": { "width": 100, "height": 100 },
      "dataSources": [
        { "name": "%NAME%", "type": "json", "fields": [] }
      ],
      "bands": [
        {
          "type": "detail",
          "height": 10,
          "dataSource": "%NAME%",
          "elements": []
        }
      ]
    }
    """.Replace("%NAME%", name);

    [Fact]
    public async Task ResolveAsync_Empty_Composite_Throws_NotFound()
    {
        var composite = new CompositeTemplateResolver();

        var act = async () => await composite.ResolveAsync("anything.rptx");

        await act.Should().ThrowAsync<TemplateNotFoundException>()
            .Where(ex => ex.SearchPath == "(composite)");
    }

    [Fact]
    public async Task ResolveAsync_First_Resolver_Hits_Returns_It()
    {
        File.WriteAllText(Path.Combine(_tempDir1, "shared.rptx"), MinimalTemplateJson("fromDir1"));
        var r1 = new FileSystemTemplateResolver(_tempDir1);
        var r2 = new FileSystemTemplateResolver(_tempDir2);
        // 注意: 第二个目录里同名文件不同内容, 不应被使用
        File.WriteAllText(Path.Combine(_tempDir2, "shared.rptx"), MinimalTemplateJson("fromDir2"));

        var composite = new CompositeTemplateResolver().AddResolver(r1).AddResolver(r2);

        var template = await composite.ResolveAsync("shared.rptx");

        template.DataSources.Should().ContainSingle(ds => ds.Name == "fromDir1");
    }

    [Fact]
    public async Task ResolveAsync_Second_Resolver_Hits_When_First_Misses()
    {
        File.WriteAllText(Path.Combine(_tempDir2, "only-in-two.rptx"), MinimalTemplateJson("fromTwo"));
        var r1 = new FileSystemTemplateResolver(_tempDir1);
        var r2 = new FileSystemTemplateResolver(_tempDir2);

        var composite = new CompositeTemplateResolver().AddResolver(r1).AddResolver(r2);

        var template = await composite.ResolveAsync("only-in-two.rptx");

        template.DataSources.Should().ContainSingle(ds => ds.Name == "fromTwo");
    }

    [Fact]
    public async Task ResolveAsync_No_Resolver_Hits_Throws_NotFound()
    {
        var r1 = new FileSystemTemplateResolver(_tempDir1);
        var r2 = new FileSystemTemplateResolver(_tempDir2);

        var composite = new CompositeTemplateResolver().AddResolver(r1).AddResolver(r2);

        var act = async () => await composite.ResolveAsync("ghost.rptx");

        await act.Should().ThrowAsync<TemplateNotFoundException>();
    }

    [Fact]
    public void Exists_Returns_True_If_Any_Resolver_Has_It()
    {
        File.WriteAllText(Path.Combine(_tempDir2, "present.rptx"), MinimalTemplateJson("p"));
        var r1 = new FileSystemTemplateResolver(_tempDir1);
        var r2 = new FileSystemTemplateResolver(_tempDir2);

        var composite = new CompositeTemplateResolver().AddResolver(r1).AddResolver(r2);

        composite.Exists("present.rptx").Should().BeTrue();
        composite.Exists("absent.rptx").Should().BeFalse();
    }

    [Fact]
    public async Task ResolveAsync_Short_Circuits_After_First_Hit()
    {
        // r1 抛异常 (如果被命中) - 但实际上 Exists 返回 false, 应跳过
        var r1 = new ThrowingResolver();
        var r2 = new FileSystemTemplateResolver(_tempDir1);
        File.WriteAllText(Path.Combine(_tempDir1, "x.rptx"), MinimalTemplateJson("x"));

        var composite = new CompositeTemplateResolver().AddResolver(r1).AddResolver(r2);

        var template = await composite.ResolveAsync("x.rptx");

        template.Should().NotBeNull();
        // r1.Exists 应被调用, 但 r1.ResolveAsync 不应被调用
        r1.ExistsCallCount.Should().Be(1);
        r1.ResolveCallCount.Should().Be(0);
    }

    /// <summary>测试用 stub: Exists 返回 false, ResolveAsync 抛异常（如果被错误调用）</summary>
    private sealed class ThrowingResolver : ITemplateResolver
    {
        public int ExistsCallCount { get; private set; }
        public int ResolveCallCount { get; private set; }

        public bool Exists(string templateRef)
        {
            ExistsCallCount++;
            return false;
        }

        public Task<ReportTemplate> ResolveAsync(string templateRef)
        {
            ResolveCallCount++;
            throw new InvalidOperationException("Should not be called - Exists returned false");
        }
    }
}
