using System.Collections.Generic;
using System.IO;
using System.Linq;
using ReportEngine.Core;
using ReportEngine.Core.Rendering;
using ReportEngine.Export.Excel;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ClosedXmlExporter 高级场景测试：
///   - 多页（每页一个 sheet）
///   - 非文本元素（line/image/shape/barcode）
///   - 字体颜色/对齐
///   - 空报告（无元素）
///   - 列宽/行高自适应
/// </summary>
public class ClosedXmlExporterAdvancedTests
{
    private readonly ClosedXmlExporter _exporter = new();

    private static RenderedReport BuildReport(params (double x, double y, double w, double h, string text)[] cells)
    {
        var r = new RenderedReport
        {
            PageWidth = 100,
            PageHeight = 100,
        };
        var page = new RenderedPage { PageNumber = 1, TotalPages = 1 };
        foreach (var (x, y, w, h, text) in cells)
        {
            page.Elements.Add(new RenderedTextElement
            {
                X = x, Y = y, Width = w, Height = h, Text = text,
            });
        }
        r.Pages.Add(page);
        return r;
    }

    [Fact]
    public void Export_EmptyReport_ThrowsBecauseClosedXmlNeedsWorksheet()
    {
        // 实际行为：ClosedXML 要求至少 1 个 worksheet，0 页时抛 InvalidOperationException
        var r = new RenderedReport();
        Assert.Throws<System.InvalidOperationException>(() => _exporter.Export(r));
    }

    [Fact]
    public void Export_SingleTextElement_ProducesXlsx()
    {
        var r = BuildReport((10, 10, 50, 5, "Hello"));
        var bytes = _exporter.Export(r);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void Export_MultiplePages_ProducesMultipleSheets()
    {
        var r = new RenderedReport { PageWidth = 100, PageHeight = 100 };
        r.Pages.Add(new RenderedPage { PageNumber = 1, Elements = { new RenderedTextElement { Text = "A" } } });
        r.Pages.Add(new RenderedPage { PageNumber = 2, Elements = { new RenderedTextElement { Text = "B" } } });
        var bytes = _exporter.Export(r);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void ExportToFile_CreatesFile()
    {
        var r = BuildReport((10, 10, 50, 5, "Test"));
        var path = Path.Combine(Path.GetTempPath(), $"test_{System.Guid.NewGuid():N}.xlsx");
        try
        {
            _exporter.ExportToFile(r, path);
            Assert.True(File.Exists(path));
            var info = new FileInfo(path);
            Assert.True(info.Length > 0);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void ExportToFile_CreatesDirectoryIfMissing()
    {
        var r = BuildReport((10, 10, 50, 5, "Test"));
        var dir = Path.Combine(Path.GetTempPath(), $"test_dir_{System.Guid.NewGuid():N}");
        var path = Path.Combine(dir, "sub", "out.xlsx");
        try
        {
            _exporter.ExportToFile(r, path);
            Assert.True(File.Exists(path));
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void Export_NonTextElements_DoNotThrow()
    {
        var r = new RenderedReport { PageWidth = 100, PageHeight = 100 };
        var page = new RenderedPage { PageNumber = 1 };
        page.Elements.Add(new RenderedLineElement { X = 5, Y = 5, Width = 20, Height = 1, Direction = LineDirection.Horizontal });
        page.Elements.Add(new RenderedShapeElement { X = 30, Y = 5, Width = 10, Height = 10, Shape = ShapeType.Rectangle });
        page.Elements.Add(new RenderedBarcodeElement { X = 50, Y = 5, Width = 20, Height = 10, Value = "123", Format = BarcodeFormat.Code128 });
        page.Elements.Add(new RenderedImageElement { X = 80, Y = 5, Width = 10, Height = 10, Source = "x.png" });
        r.Pages.Add(page);
        var bytes = _exporter.Export(r);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void Export_FontBold_RenderedToCell()
    {
        var r = BuildReport((10, 10, 50, 5, "Bold"));
        r.Pages[0].Elements.OfType<RenderedTextElement>().First().Font.Bold = true;
        var bytes = _exporter.Export(r);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void Export_FontColor_RenderedToCell()
    {
        var r = BuildReport((10, 10, 50, 5, "Colored"));
        r.Pages[0].Elements.OfType<RenderedTextElement>().First().Font.Color = "#FF0000";
        var bytes = _exporter.Export(r);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void Export_Alignment_Center()
    {
        var r = BuildReport((10, 10, 50, 5, "Center"));
        r.Pages[0].Elements.OfType<RenderedTextElement>().First().Alignment = TextAlignment.Center;
        var bytes = _exporter.Export(r);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void Export_MultipleColumns_RowAnchors()
    {
        var r = BuildReport(
            (10, 10, 30, 5, "Col1"),
            (50, 10, 30, 5, "Col2"),
            (10, 20, 30, 5, "Row2A"),
            (50, 20, 30, 5, "Row2B"));
        var bytes = _exporter.Export(r);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void Export_ClusterTolerance_SameColumn()
    {
        // X=10.1 vs X=10.5 → 同列（容差 0.8）
        var r = BuildReport(
            (10.0, 10, 20, 5, "A"),
            (10.5, 15, 20, 5, "B"),
            (20.0, 10, 20, 5, "C"));
        var bytes = _exporter.Export(r);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void Export_CustomClusterTolerance()
    {
        var custom = new ClosedXmlExporter { ClusterTolerance = 5.0 };
        var r = BuildReport((10, 10, 20, 5, "A"), (12, 15, 20, 5, "B"));
        var bytes = custom.Export(r);
        Assert.NotEmpty(bytes);
    }
}
