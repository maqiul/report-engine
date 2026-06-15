using System.Collections.Generic;
using ReportEngine.Core.Export;
using ReportEngine.Core.Rendering;
using ReportEngine.Export.Excel;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// IExcelExporter 接口契约 + Fake 实现测试（Core.Tests 范围）：
///   - ClosedXmlExporter 实现 IExcelExporter
///   - 多态可工作
///   - FakeIExcelExporter 记录调用次数
/// </summary>
public class ExcelExporterContractTests
{
    [Fact]
    public void ClosedXmlExporter_ImplementsIExcelExporter()
    {
        IExcelExporter exporter = new ClosedXmlExporter();
        Assert.NotNull(exporter);
    }

    [Fact]
    public void ClosedXmlExporter_Export_ReturnsNonEmpty()
    {
        IExcelExporter exporter = new ClosedXmlExporter();
        var r = new RenderedReport { PageWidth = 100, PageHeight = 100 };
        r.Pages.Add(new RenderedPage
        {
            Elements = { new RenderedTextElement { Text = "x" } },
        });
        var bytes = exporter.Export(r);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void ClosedXmlExporter_Export_ReturnsValidXlsx()
    {
        IExcelExporter exporter = new ClosedXmlExporter();
        var r = new RenderedReport { PageWidth = 100, PageHeight = 100 };
        r.Pages.Add(new RenderedPage
        {
            Elements = { new RenderedTextElement { Text = "x" } },
        });
        var bytes = exporter.Export(r);
        // XLSX magic
        Assert.Equal(0x50, bytes[0]);
        Assert.Equal(0x4B, bytes[1]);
    }

    [Fact]
    public void FakeIExcelExporter_RecordsCalls()
    {
        var fake = new FakeIExcelExporter();
        var r = new RenderedReport();
        var bytes = fake.Export(r);
        Assert.Equal(1, fake.ExportCallCount);
        Assert.NotNull(bytes);
    }

    [Fact]
    public void FakeIExcelExporter_ExportToFile_RecordsPath()
    {
        var fake = new FakeIExcelExporter();
        var r = new RenderedReport();
        fake.ExportToFile(r, "test.xlsx");
        Assert.Equal(1, fake.ExportToFileCallCount);
        Assert.Equal("test.xlsx", fake.LastPath);
    }

    [Fact]
    public void IExcelExporter_MultipleExport_IndependentReports()
    {
        var exporter = new ClosedXmlExporter();
        var r1 = new RenderedReport { PageWidth = 50, PageHeight = 50 };
        r1.Pages.Add(new RenderedPage { Elements = { new RenderedTextElement { Text = "a" } } });
        var r2 = new RenderedReport { PageWidth = 80, PageHeight = 80 };
        r2.Pages.Add(new RenderedPage { Elements = { new RenderedTextElement { Text = "b" } } });
        var b1 = exporter.Export(r1);
        var b2 = exporter.Export(r2);
        Assert.NotEqual(b1.Length, b2.Length);
    }

    [Fact]
    public void IExcelExporter_Polymorphism_FakeReturnsPredefinedBytes()
    {
        IExcelExporter fake = new FakeIExcelExporter();
        var bytes = fake.Export(new RenderedReport());
        // 模拟 4 字节
        Assert.Equal(4, bytes.Length);
    }

    [Fact]
    public void FakeIExcelExporter_MultipleExports_AllCounted()
    {
        var fake = new FakeIExcelExporter();
        for (int i = 0; i < 5; i++) fake.Export(new RenderedReport());
        Assert.Equal(5, fake.ExportCallCount);
    }

    [Fact]
    public void FakeIExcelExporter_ExportToFile_MultiplePaths_OnlyLastKept()
    {
        var fake = new FakeIExcelExporter();
        fake.ExportToFile(new RenderedReport(), "first.xlsx");
        fake.ExportToFile(new RenderedReport(), "second.xlsx");
        Assert.Equal(2, fake.ExportToFileCallCount);
        Assert.Equal("second.xlsx", fake.LastPath);
    }

    [Fact]
    public void ClosedXmlExporter_ClusterTolerance_DefaultAndCustom()
    {
        var def = new ClosedXmlExporter();
        Assert.Equal(0.8, def.ClusterTolerance);
        var custom = new ClosedXmlExporter { ClusterTolerance = 2.0 };
        Assert.Equal(2.0, custom.ClusterTolerance);
    }
}

/// <summary>
/// 测试用模拟 IExcelExporter（不依赖 ClosedXML）
/// </summary>
public class FakeIExcelExporter : IExcelExporter
{
    public int ExportCallCount { get; private set; }
    public int ExportToFileCallCount { get; private set; }
    public string? LastPath { get; private set; }

    public byte[] Export(RenderedReport renderedReport)
    {
        ExportCallCount++;
        return new byte[] { 0x50, 0x4B, 0x03, 0x04 };
    }

    public void ExportToFile(RenderedReport renderedReport, string outputPath)
    {
        ExportToFileCallCount++;
        LastPath = outputPath;
    }
}
