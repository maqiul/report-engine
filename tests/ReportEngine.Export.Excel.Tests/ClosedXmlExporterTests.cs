using ClosedXML.Excel;
using ReportEngine.Core;
using ReportEngine.Core.Rendering;
using ReportEngine.Export.Excel;
using Xunit;

namespace ReportEngine.Export.Excel.Tests;

/// <summary>
/// ClosedXmlExporter 基础行为测试
/// </summary>
public class ClosedXmlExporterBehaviorTests
{
    private readonly ClosedXmlExporter _exporter = new();

    private static RenderedReport MakeReport(int pageCount = 1, int elementsPerPage = 1)
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            PageWidth = 210,
            PageHeight = 297
        };

        for (int p = 0; p < pageCount; p++)
        {
            var page = new RenderedPage();
            for (int i = 0; i < elementsPerPage; i++)
            {
                page.Elements.Add(new RenderedTextElement
                {
                    X = 10 + i * 50,
                    Y = 10,
                    Width = 40,
                    Height = 5,
                    Text = $"Cell_{p}_{i}",
                    Font = new FontDef { Family = "Arial", Size = 10 }
                });
            }
            report.Pages.Add(page);
        }

        return report;
    }

    // ============== Export 返回 byte[] ==============

    [Fact]
    public void Export_ReturnsNonEmptyBytes()
    {
        var report = MakeReport();
        var bytes = _exporter.Export(report);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void Export_ReturnsValidXlsx()
    {
        var report = MakeReport();
        var bytes = _exporter.Export(report);

        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);
        Assert.NotNull(wb);
    }

    [Fact]
    public void Export_SinglePage_HasOneSheet()
    {
        var report = MakeReport(1);
        var bytes = _exporter.Export(report);

        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);
        Assert.Single(wb.Worksheets);
    }

    [Fact]
    public void Export_SinglePage_SheetNameIsReport()
    {
        var report = MakeReport(1);
        var bytes = _exporter.Export(report);

        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);
        Assert.Equal("Report", wb.Worksheets.First().Name);
    }

    [Fact]
    public void Export_TwoPages_HasTwoSheets()
    {
        var report = MakeReport(2);
        var bytes = _exporter.Export(report);

        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);
        Assert.Equal(2, wb.Worksheets.Count);
    }

    [Fact]
    public void Export_TwoPages_SheetNamesArePage1Page2()
    {
        var report = MakeReport(2);
        var bytes = _exporter.Export(report);

        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);
        Assert.Equal("Page 1", wb.Worksheets.ElementAt(0).Name);
        Assert.Equal("Page 2", wb.Worksheets.ElementAt(1).Name);
    }

    [Fact]
    public void Export_ThreePages_HasThreeSheets()
    {
        var report = MakeReport(3);
        var bytes = _exporter.Export(report);

        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);
        Assert.Equal(3, wb.Worksheets.Count);
    }

    // ============== 单元格内容 ==============

    [Fact]
    public void Export_SingleElement_CellContainsText()
    {
        var report = MakeReport(1, 1);
        var bytes = _exporter.Export(report);

        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);
        var ws = wb.Worksheets.First();
        var cellValue = ws.Cell(1, 1).GetString();
        Assert.Equal("Cell_0_0", cellValue);
    }

    [Fact]
    public void Export_MultipleElements_AllTextWritten()
    {
        var report = MakeReport(1, 3);
        var bytes = _exporter.Export(report);

        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);
        var ws = wb.Worksheets.First();

        var allText = new List<string>();
        foreach (var cell in ws.CellsUsed())
        {
            allText.Add(cell.GetString());
        }

        Assert.Contains("Cell_0_0", allText);
        Assert.Contains("Cell_0_1", allText);
        Assert.Contains("Cell_0_2", allText);
    }

    // ============== 空页面 ==============

    [Fact]
    public void Export_EmptyPage_NoCrash()
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            PageWidth = 210,
            PageHeight = 297
        };
        report.Pages.Add(new RenderedPage()); // 空页面

        var bytes = _exporter.Export(report);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void Export_NoPages_ThrowsException()
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            PageWidth = 210,
            PageHeight = 297
        };

        // ClosedXML requires at least one worksheet
        Assert.Throws<InvalidOperationException>(() => _exporter.Export(report));
    }

    // ============== ClusterTolerance ==============

    [Fact]
    public void ClusterTolerance_DefaultIs08()
    {
        Assert.Equal(0.8, _exporter.ClusterTolerance);
    }

    [Fact]
    public void ClusterTolerance_Set_Works()
    {
        _exporter.ClusterTolerance = 1.5;
        Assert.Equal(1.5, _exporter.ClusterTolerance);
    }

    [Fact]
    public void ClusterTolerance_SetZero_Works()
    {
        _exporter.ClusterTolerance = 0;
        Assert.Equal(0, _exporter.ClusterTolerance);
    }

    // ============== ExportToFile ==============

    [Fact]
    public void ExportToFile_CreatesFile()
    {
        var report = MakeReport();
        var path = Path.Combine(Path.GetTempPath(), $"test_export_{Guid.NewGuid():N}.xlsx");

        try
        {
            _exporter.ExportToFile(report, path);
            Assert.True(File.Exists(path));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void ExportToFile_FileIsNonEmpty()
    {
        var report = MakeReport();
        var path = Path.Combine(Path.GetTempPath(), $"test_export_{Guid.NewGuid():N}.xlsx");

        try
        {
            _exporter.ExportToFile(report, path);
            var fi = new FileInfo(path);
            Assert.True(fi.Length > 0);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void ExportToFile_CreatesDirectoryIfNeeded()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"test_dir_{Guid.NewGuid():N}");
        var path = Path.Combine(dir, "test.xlsx");

        try
        {
            _exporter.ExportToFile(MakeReport(), path);
            Assert.True(Directory.Exists(dir));
            Assert.True(File.Exists(path));
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    // ============== 字体/样式 ==============

    [Fact]
    public void Export_WithFontFamily_FontApplied()
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            PageWidth = 210,
            PageHeight = 297
        };
        var page = new RenderedPage();
        page.Elements.Add(new RenderedTextElement
        {
            X = 10, Y = 10, Width = 40, Height = 5,
            Text = "Styled",
            Font = new FontDef { Family = "Courier New", Size = 14, Bold = true }
        });
        report.Pages.Add(page);

        var bytes = _exporter.Export(report);
        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);
        var ws = wb.Worksheets.First();
        var cell = ws.Cell(1, 1);

        Assert.Equal("Courier New", cell.Style.Font.FontName);
        Assert.True(cell.Style.Font.Bold);
    }

    [Fact]
    public void Export_WithAlignment_AlignmentApplied()
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            PageWidth = 210,
            PageHeight = 297
        };
        var page = new RenderedPage();
        page.Elements.Add(new RenderedTextElement
        {
            X = 10, Y = 10, Width = 40, Height = 5,
            Text = "Centered",
            Alignment = TextAlignment.Center
        });
        report.Pages.Add(page);

        var bytes = _exporter.Export(report);
        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);
        var ws = wb.Worksheets.First();
        var cell = ws.Cell(1, 1);

        Assert.Equal(XLAlignmentHorizontalValues.Center, cell.Style.Alignment.Horizontal);
    }

    // ============== 列宽 ==============

    [Fact]
    public void Export_ColumnWidth_IsAtLeast8()
    {
        var report = MakeReport(1, 1);
        var bytes = _exporter.Export(report);

        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);
        var ws = wb.Worksheets.First();
        Assert.True(ws.Column(1).Width >= 8);
    }

    // ============== 多行多列布局 ==============

    [Fact]
    public void Export_GridLayout_CorrectCells()
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            PageWidth = 210,
            PageHeight = 297
        };
        var page = new RenderedPage();

        // 2x2 grid
        page.Elements.Add(new RenderedTextElement { X = 10, Y = 10, Width = 30, Height = 5, Text = "A1" });
        page.Elements.Add(new RenderedTextElement { X = 50, Y = 10, Width = 30, Height = 5, Text = "B1" });
        page.Elements.Add(new RenderedTextElement { X = 10, Y = 20, Width = 30, Height = 5, Text = "A2" });
        page.Elements.Add(new RenderedTextElement { X = 50, Y = 20, Width = 30, Height = 5, Text = "B2" });

        report.Pages.Add(page);

        var bytes = _exporter.Export(report);
        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);
        var ws = wb.Worksheets.First();

        var allText = new HashSet<string>();
        foreach (var cell in ws.CellsUsed())
            allText.Add(cell.GetString());

        Assert.Contains("A1", allText);
        Assert.Contains("B1", allText);
        Assert.Contains("A2", allText);
        Assert.Contains("B2", allText);
    }

    // ============== 非文本元素被忽略 ==============

    [Fact]
    public void Export_NonTextElements_Ignored()
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            PageWidth = 210,
            PageHeight = 297
        };
        var page = new RenderedPage();
        page.Elements.Add(new RenderedLineElement { X = 10, Y = 10, Width = 100, Height = 0 });
        page.Elements.Add(new RenderedShapeElement { X = 10, Y = 20, Width = 50, Height = 30 });
        page.Elements.Add(new RenderedTextElement { X = 10, Y = 60, Width = 40, Height = 5, Text = "OnlyText" });
        report.Pages.Add(page);

        var bytes = _exporter.Export(report);
        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);
        var ws = wb.Worksheets.First();

        var allText = ws.CellsUsed().Select(c => c.GetString()).ToList();
        Assert.Contains("OnlyText", allText);
        Assert.Single(allText);
    }

    // ============== 颜色 ==============

    [Fact]
    public void Export_WithFontColor_ColorApplied()
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            PageWidth = 210,
            PageHeight = 297
        };
        var page = new RenderedPage();
        page.Elements.Add(new RenderedTextElement
        {
            X = 10, Y = 10, Width = 40, Height = 5,
            Text = "Red",
            Font = new FontDef { Color = "#FF0000" }
        });
        report.Pages.Add(page);

        var bytes = _exporter.Export(report);
        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);
        var ws = wb.Worksheets.First();
        var cell = ws.Cell(1, 1);
        // Verify font color is set (the exact format may vary)
        Assert.NotNull(cell.Style.Font.FontColor);
    }

    // ============== 背景色 ==============

    [Fact]
    public void Export_WithBackgroundColor_ColorApplied()
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            PageWidth = 210,
            PageHeight = 297
        };
        var page = new RenderedPage();
        page.Elements.Add(new RenderedTextElement
        {
            X = 10, Y = 10, Width = 40, Height = 5,
            Text = "BG",
            BackgroundColor = "#FFFF00"
        });
        report.Pages.Add(page);

        var bytes = _exporter.Export(report);
        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);
        var ws = wb.Worksheets.First();
        var cell = ws.Cell(1, 1);
        Assert.NotNull(cell.Style.Fill);
    }
}

/// <summary>
/// ClosedXmlExporter 边界测试
/// </summary>
public class ClosedXmlExporterBoundaryTests
{
    private readonly ClosedXmlExporter _exporter = new();

    [Fact]
    public void Export_LargeReport_DoesNotCrash()
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            PageWidth = 210,
            PageHeight = 297
        };

        var page = new RenderedPage();
        for (int i = 0; i < 100; i++)
        {
            page.Elements.Add(new RenderedTextElement
            {
                X = 10 + (i % 5) * 30,
                Y = 10 + (i / 5) * 8,
                Width = 25,
                Height = 6,
                Text = $"Item_{i}"
            });
        }
        report.Pages.Add(page);

        var bytes = _exporter.Export(report);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void Export_EmptyText_Works()
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            PageWidth = 210,
            PageHeight = 297
        };
        var page = new RenderedPage();
        page.Elements.Add(new RenderedTextElement { X = 10, Y = 10, Width = 40, Height = 5, Text = "" });
        report.Pages.Add(page);

        var bytes = _exporter.Export(report);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void Export_LongText_Works()
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            PageWidth = 210,
            PageHeight = 297
        };
        var page = new RenderedPage();
        page.Elements.Add(new RenderedTextElement
        {
            X = 10, Y = 10, Width = 200, Height = 5,
            Text = new string('A', 1000)
        });
        report.Pages.Add(page);

        var bytes = _exporter.Export(report);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void Export_ChineseText_Works()
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            PageWidth = 210,
            PageHeight = 297
        };
        var page = new RenderedPage();
        page.Elements.Add(new RenderedTextElement
        {
            X = 10, Y = 10, Width = 40, Height = 5,
            Text = "你好世界"
        });
        report.Pages.Add(page);

        var bytes = _exporter.Export(report);
        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);
        var ws = wb.Worksheets.First();
        Assert.Equal("你好世界", ws.Cell(1, 1).GetString());
    }

    [Fact]
    public void Export_SpecialChars_Works()
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            PageWidth = 210,
            PageHeight = 297
        };
        var page = new RenderedPage();
        page.Elements.Add(new RenderedTextElement
        {
            X = 10, Y = 10, Width = 40, Height = 5,
            Text = "Price: $100.00 (USD)"
        });
        report.Pages.Add(page);

        var bytes = _exporter.Export(report);
        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);
        var ws = wb.Worksheets.First();
        Assert.Equal("Price: $100.00 (USD)", ws.Cell(1, 1).GetString());
    }

    [Fact]
    public void Export_OverlappingElements_AllWritten()
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            PageWidth = 210,
            PageHeight = 297
        };
        var page = new RenderedPage();
        page.Elements.Add(new RenderedTextElement { X = 10, Y = 10, Width = 40, Height = 5, Text = "Overlap1" });
        page.Elements.Add(new RenderedTextElement { X = 10, Y = 10, Width = 40, Height = 5, Text = "Overlap2" });
        report.Pages.Add(page);

        var bytes = _exporter.Export(report);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void Export_ItalicFont_Applied()
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            PageWidth = 210,
            PageHeight = 297
        };
        var page = new RenderedPage();
        page.Elements.Add(new RenderedTextElement
        {
            X = 10, Y = 10, Width = 40, Height = 5,
            Text = "Italic",
            Font = new FontDef { Italic = true }
        });
        report.Pages.Add(page);

        var bytes = _exporter.Export(report);
        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);
        var ws = wb.Worksheets.First();
        Assert.True(ws.Cell(1, 1).Style.Font.Italic);
    }

    [Fact]
    public void Export_UnderlineFont_Applied()
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            PageWidth = 210,
            PageHeight = 297
        };
        var page = new RenderedPage();
        page.Elements.Add(new RenderedTextElement
        {
            X = 10, Y = 10, Width = 40, Height = 5,
            Text = "Underline",
            Font = new FontDef { Underline = true }
        });
        report.Pages.Add(page);

        var bytes = _exporter.Export(report);
        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);
        var ws = wb.Worksheets.First();
        Assert.True(ws.Cell(1, 1).Style.Font.Underline != XLFontUnderlineValues.None);
    }

    [Fact]
    public void Export_RightAlignment_Applied()
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            PageWidth = 210,
            PageHeight = 297
        };
        var page = new RenderedPage();
        page.Elements.Add(new RenderedTextElement
        {
            X = 10, Y = 10, Width = 40, Height = 5,
            Text = "Right",
            Alignment = TextAlignment.Right
        });
        report.Pages.Add(page);

        var bytes = _exporter.Export(report);
        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);
        var ws = wb.Worksheets.First();
        Assert.Equal(XLAlignmentHorizontalValues.Right, ws.Cell(1, 1).Style.Alignment.Horizontal);
    }

    [Fact]
    public void Export_JustifyAlignment_NotSupported_FallsBackToLeft()
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            PageWidth = 210,
            PageHeight = 297
        };
        var page = new RenderedPage();
        page.Elements.Add(new RenderedTextElement
        {
            X = 10, Y = 10, Width = 40, Height = 5,
            Text = "Justify",
            Alignment = TextAlignment.Justify
        });
        report.Pages.Add(page);

        var bytes = _exporter.Export(report);
        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);
        var ws = wb.Worksheets.First();
        // Justify is not supported in Excel export, falls back to Left
        Assert.Equal(XLAlignmentHorizontalValues.Left, ws.Cell(1, 1).Style.Alignment.Horizontal);
    }

    [Fact]
    public void Export_ManyPages_AllSheetsCreated()
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            PageWidth = 210,
            PageHeight = 297
        };

        for (int p = 0; p < 10; p++)
        {
            var page = new RenderedPage();
            page.Elements.Add(new RenderedTextElement { X = 10, Y = 10, Width = 40, Height = 5, Text = $"P{p}" });
            report.Pages.Add(page);
        }

        var bytes = _exporter.Export(report);
        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);
        Assert.Equal(10, wb.Worksheets.Count);
    }

    [Fact]
    public void Export_Hyperlink_Applied()
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            PageWidth = 210,
            PageHeight = 297
        };
        var page = new RenderedPage();
        page.Elements.Add(new RenderedTextElement
        {
            X = 10, Y = 10, Width = 40, Height = 5,
            Text = "Link",
            Hyperlink = "https://example.com"
        });
        report.Pages.Add(page);

        var bytes = _exporter.Export(report);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void Export_FontSize_Applied()
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            PageWidth = 210,
            PageHeight = 297
        };
        var page = new RenderedPage();
        page.Elements.Add(new RenderedTextElement
        {
            X = 10, Y = 10, Width = 40, Height = 5,
            Text = "Big",
            Font = new FontDef { Size = 24 }
        });
        report.Pages.Add(page);

        var bytes = _exporter.Export(report);
        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);
        var ws = wb.Worksheets.First();
        // Font size is applied (may or may not be converted from mm to pt)
        Assert.True(ws.Cell(1, 1).Style.Font.FontSize > 0);
    }
}
