using ReportEngine.Core.Export;
using ReportEngine.Core.Rendering;
using Xunit;

namespace ReportEngine.Export.Pdf.Tests;

/// <summary>
/// IPdfExporter 接口契约 + Fake 实现测试（Pdf.Tests 范围）：
///   - PdfSharpExporter 实现 IPdfExporter
///   - 多态可工作
///   - FakeIPdfExporter 记录调用次数
///   - 多次导出独立
/// </summary>
public class PdfExporterContractTests
{
    [Fact]
    public void PdfSharpExporter_ImplementsIPdfExporter()
    {
        IPdfExporter exporter = new PdfSharpExporter();
        Assert.NotNull(exporter);
    }

    [Fact]
    public void PdfSharpExporter_Export_ReturnsNonEmpty()
    {
        IPdfExporter exporter = new PdfSharpExporter();
        var r = new RenderedReport { PageWidth = 100, PageHeight = 100 };
        r.Pages.Add(new RenderedPage
        {
            Elements = { new RenderedTextElement { Text = "x" } },
        });
        var bytes = exporter.Export(r);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void PdfSharpExporter_Export_ReturnsValidPdf()
    {
        IPdfExporter exporter = new PdfSharpExporter();
        var r = new RenderedReport { PageWidth = 100, PageHeight = 100 };
        r.Pages.Add(new RenderedPage
        {
            Elements = { new RenderedTextElement { Text = "x" } },
        });
        var bytes = exporter.Export(r);
        // PDF magic
        Assert.Equal(0x25, bytes[0]);
    }

    [Fact]
    public void FakeIPdfExporter_RecordsCalls()
    {
        var fake = new FakeIPdfExporter();
        var r = new RenderedReport();
        var bytes = fake.Export(r);
        Assert.Equal(1, fake.ExportCallCount);
        Assert.NotNull(bytes);
    }

    [Fact]
    public void FakeIPdfExporter_ExportToFile_RecordsPath()
    {
        var fake = new FakeIPdfExporter();
        var r = new RenderedReport();
        fake.ExportToFile(r, "out.pdf");
        Assert.Equal(1, fake.ExportToFileCallCount);
        Assert.Equal("out.pdf", fake.LastPath);
    }

    [Fact]
    public void IPdfExporter_Polymorphism_FakeReturnsPredefinedBytes()
    {
        IPdfExporter fake = new FakeIPdfExporter();
        var bytes = fake.Export(new RenderedReport());
        Assert.Equal(4, bytes.Length);
    }

    [Fact]
    public void FakeIPdfExporter_MultipleExports_AllCounted()
    {
        var fake = new FakeIPdfExporter();
        for (int i = 0; i < 5; i++) fake.Export(new RenderedReport());
        Assert.Equal(5, fake.ExportCallCount);
    }

    [Fact]
    public void FakeIPdfExporter_ExportToFile_MultiplePaths_OnlyLastKept()
    {
        var fake = new FakeIPdfExporter();
        fake.ExportToFile(new RenderedReport(), "first.pdf");
        fake.ExportToFile(new RenderedReport(), "second.pdf");
        Assert.Equal(2, fake.ExportToFileCallCount);
        Assert.Equal("second.pdf", fake.LastPath);
    }

    [Fact]
    public void PdfSharpExporter_MultipleExport_IndependentReports()
    {
        var exporter = new PdfSharpExporter();
        var r1 = new RenderedReport { PageWidth = 50, PageHeight = 50 };
        r1.Pages.Add(new RenderedPage { Elements = { new RenderedTextElement { Text = "a" } } });
        var r2 = new RenderedReport { PageWidth = 80, PageHeight = 80 };
        r2.Pages.Add(new RenderedPage { Elements = { new RenderedTextElement { Text = "b" } } });
        var b1 = exporter.Export(r1);
        var b2 = exporter.Export(r2);
        Assert.NotEmpty(b1);
        Assert.NotEmpty(b2);
    }

    [Fact]
    public void PdfSharpExporter_ViaInterface_NoThrow()
    {
        IPdfExporter exporter = new PdfSharpExporter();
        var r = new RenderedReport { PageWidth = 100, PageHeight = 100 };
        r.Pages.Add(new RenderedPage { Elements = { new RenderedTextElement { Text = "x" } } });
        var bytes = exporter.Export(r);
        Assert.NotEmpty(bytes);
    }
}

/// <summary>
/// 测试用模拟 IPdfExporter（不依赖 PdfSharpCore）
/// </summary>
public class FakeIPdfExporter : IPdfExporter
{
    public int ExportCallCount { get; private set; }
    public int ExportToFileCallCount { get; private set; }
    public string? LastPath { get; private set; }

    public byte[] Export(RenderedReport renderedReport)
    {
        ExportCallCount++;
        return new byte[] { 0x25, 0x50, 0x44, 0x46 };
    }

    public void ExportToFile(RenderedReport renderedReport, string outputPath)
    {
        ExportToFileCallCount++;
        LastPath = outputPath;
    }
}
