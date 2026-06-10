using FluentAssertions;
using ReportEngine.Core.Parsing;
using ReportEngine.Core.SubReports;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// FileSystemTemplateResolver 行为测试：
///   - 解析真实 .rptx 文件
///   - 缓存命中（同一 ref 第二次不再读文件）
///   - 不存在的模板抛 TemplateNotFoundException
///   - ListTemplates 列出目录里的 .rptx 文件
///   - Save 写出文件 + 列表中可见
/// </summary>
public class SubReportResolverTests : IDisposable
{
    private readonly string _tempDir;

    public SubReportResolverTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "ReportEngine.Tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
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
    public async Task ResolveAsync_Reads_File_And_Returns_Template()
    {
        File.WriteAllText(Path.Combine(_tempDir, "child.rptx"), MinimalTemplateJson("lines"));
        var resolver = new FileSystemTemplateResolver(_tempDir);

        var template = await resolver.ResolveAsync("child.rptx");

        template.Should().NotBeNull();
        template.Page.Width.Should().Be(100);
        template.DataSources.Should().ContainSingle(ds => ds.Name == "lines");
    }

    [Fact]
    public async Task ResolveAsync_Caches_Result_Second_Call_Does_Not_ReRead()
    {
        var path = Path.Combine(_tempDir, "cached.rptx");
        File.WriteAllText(path, MinimalTemplateJson("ds1"));
        var resolver = new FileSystemTemplateResolver(_tempDir);

        var first = await resolver.ResolveAsync("cached.rptx");

        // 删掉磁盘文件；如果第二次真正读了文件，会抛 TemplateNotFoundException
        File.Delete(path);

        var second = await resolver.ResolveAsync("cached.rptx");

        second.Should().BeSameAs(first);
    }

    [Fact]
    public async Task ResolveAsync_Missing_File_Throws_TemplateNotFoundException()
    {
        var resolver = new FileSystemTemplateResolver(_tempDir);

        var act = async () => await resolver.ResolveAsync("ghost.rptx");

        await act.Should().ThrowAsync<TemplateNotFoundException>();
    }

    [Fact]
    public void Exists_Returns_False_For_Missing_File()
    {
        var resolver = new FileSystemTemplateResolver(_tempDir);

        resolver.Exists("nope.rptx").Should().BeFalse();
    }

    [Fact]
    public void ListTemplates_Returns_All_Rptx_Files_In_Directory()
    {
        File.WriteAllText(Path.Combine(_tempDir, "a.rptx"), MinimalTemplateJson("a"));
        File.WriteAllText(Path.Combine(_tempDir, "b.rptx"), MinimalTemplateJson("b"));
        File.WriteAllText(Path.Combine(_tempDir, "ignored.txt"), "not a template");
        var resolver = new FileSystemTemplateResolver(_tempDir);

        var list = resolver.ListTemplates();

        list.Should().Contain(new[] { "a.rptx", "b.rptx" });
        list.Should().NotContain("ignored.txt");
    }

    [Fact]
    public void Save_Writes_File_And_Makes_It_Listable()
    {
        var resolver = new FileSystemTemplateResolver(_tempDir);
        var parser = new TemplateParser();
        var template = parser.Parse(MinimalTemplateJson("saved"));

        resolver.Save(template, "saved.rptx");

        File.Exists(Path.Combine(_tempDir, "saved.rptx")).Should().BeTrue();
        resolver.ListTemplates().Should().Contain("saved.rptx");
    }

    [Fact]
    public async Task ClearCache_Removes_Previously_Resolved_Templates()
    {
        var path = Path.Combine(_tempDir, "clear.rptx");
        File.WriteAllText(path, MinimalTemplateJson("c"));
        var resolver = new FileSystemTemplateResolver(_tempDir);

        await resolver.ResolveAsync("clear.rptx");
        resolver.ClearCache();

        File.Delete(path);
        var act = async () => await resolver.ResolveAsync("clear.rptx");
        await act.Should().ThrowAsync<TemplateNotFoundException>();
    }
}