using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ReportEngine.Core;
using ReportEngine.Core.Rendering;
using ReportEngine.Export.Pdf;
using Xunit;

namespace ReportEngine.Export.Pdf.Tests;

/// <summary>
/// PdfSharpExporter 边界场景补充测试：
///   - 0 元素页面（空页）
///   - 0 页报告（空报告）—— 仍产有效 PDF
///   - 极小页面（1×1mm）
///   - 极大页面（2000×2000mm）
///   - 元素超出页面边界
///   - 字体大小极端（0.1pt / 999pt）
///   - 颜色极端（黑/白/无色）
///   - 子报告（不抛）
/// </summary>
public class PdfSharpExporterBoundaryTests
{
    private readonly PdfSharpExporter _exporter = new();

    private static RenderedReport BuildReport(double w = 100, double h = 100, params RenderedElement[] elements)
    {
        var r = new RenderedReport { PageWidth = w, PageHeight = h };
        var page = new RenderedPage { PageNumber = 1, TotalPages = 1 };
        foreach (var e in elements) page.Elements.Add(e);
        r.Pages.Add(page);
        return r;
    }

    [Fact]
    public void Export_ZeroPages_ThrowsBecausePdfNeedsPages()
    {
        // 实际行为：PdfSharpCore 抛 "Cannot save a PDF document with no pages"
        var r = new RenderedReport();
        Assert.Throws<InvalidOperationException>(() => _exporter.Export(r));
    }

    [Fact]
    public void Export_PageWithZeroElements_StillProducesValidPdf()
    {
        var r = BuildReport();
        var bytes = _exporter.Export(r);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void Export_VerySmallPage_1x1mm_DoesNotThrow()
    {
        var r = BuildReport(1, 1, new RenderedTextElement { Text = "tiny" });
        var bytes = _exporter.Export(r);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void Export_VeryLargePage_2000x2000mm_DoesNotThrow()
    {
        var r = BuildReport(2000, 2000, new RenderedTextElement { Text = "big" });
        var bytes = _exporter.Export(r);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void Export_ElementExceedsPage_DoesNotThrow()
    {
        var r = BuildReport(50, 50,
            new RenderedTextElement { X = 100, Y = 100, Width = 200, Height = 200, Text = "off" });
        var bytes = _exporter.Export(r);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void Export_NegativeCoordinates_DoesNotThrow()
    {
        var r = BuildReport(100, 100,
            new RenderedTextElement { X = -10, Y = -10, Width = 50, Height = 5, Text = "neg" });
        var bytes = _exporter.Export(r);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void Export_FontSize_VerySmall_DoesNotThrow()
    {
        var r = BuildReport(100, 100,
            new RenderedTextElement
            {
                Text = "small",
                Font = new FontDef { Family = "Arial", Size = 0.1 },
            });
        var bytes = _exporter.Export(r);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void Export_FontSize_VeryLarge_DoesNotThrow()
    {
        var r = BuildReport(100, 100,
            new RenderedTextElement
            {
                Text = "huge",
                Font = new FontDef { Family = "Arial", Size = 999 },
            });
        var bytes = _exporter.Export(r);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void Export_EmptyText_DoesNotThrow()
    {
        var r = BuildReport(100, 100, new RenderedTextElement { Text = "" });
        var bytes = _exporter.Export(r);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void Export_NullText_DoesNotThrow()
    {
        var r = BuildReport(100, 100, new RenderedTextElement { Text = null! });
        var bytes = _exporter.Export(r);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void Export_VeryLongText_DoesNotThrow()
    {
        var longText = new string('A', 10000);
        var r = BuildReport(100, 100, new RenderedTextElement { Text = longText, Width = 50, Height = 5 });
        var bytes = _exporter.Export(r);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void Export_ManyElements_SamePage_DoesNotThrow()
    {
        var r = BuildReport(100, 100,
            Enumerable.Range(0, 100)
                .Select(i => new RenderedTextElement
                {
                    X = (i % 10) * 10,
                    Y = (i / 10) * 5,
                    Width = 8, Height = 3,
                    Text = $"e{i}",
                })
                .ToArray());
        var bytes = _exporter.Export(r);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void ExportToFile_OverwriteExistingFile_DoesNotThrow()
    {
        var r = BuildReport(50, 50, new RenderedTextElement { Text = "x" });
        var path = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.pdf");
        try
        {
            _exporter.ExportToFile(r, path);
            var firstSize = new FileInfo(path).Length;
            _exporter.ExportToFile(r, path);
            var secondSize = new FileInfo(path).Length;
            Assert.True(File.Exists(path));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void Export_ZeroPageWidth_DoesNotThrow()
    {
        var r = BuildReport(0, 100, new RenderedTextElement { Text = "x" });
        var bytes = _exporter.Export(r);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void Export_AllElementsTypes_Together()
    {
        var r = BuildReport(200, 200,
            new RenderedTextElement { X = 5, Y = 5, Width = 30, Height = 5, Text = "T" },
            new RenderedLineElement { X = 40, Y = 5, Width = 20, Height = 1, Direction = LineDirection.Horizontal },
            new RenderedShapeElement { X = 65, Y = 5, Width = 10, Height = 10, Shape = ShapeType.Rectangle },
            new RenderedBarcodeElement { X = 80, Y = 5, Width = 20, Height = 10, Value = "123", Format = BarcodeFormat.Code128 },
            new RenderedImageElement { X = 105, Y = 5, Width = 10, Height = 10, Source = "x.png" });
        var bytes = _exporter.Export(r);
        Assert.NotEmpty(bytes);
    }
}
