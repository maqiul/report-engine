using System.Collections.Generic;
using System.IO;
using ReportEngine.Core;
using ReportEngine.Core.Export;
using ReportEngine.Core.Rendering;
using ReportEngine.Export.Excel;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ClosedXmlExporter 行为测试：
///   - 空报表导出
///   - 单页导出
///   - 多页导出
///   - 文本元素导出
///   - 文件导出
/// </summary>
public class ClosedXmlExporterBehaviorTests
{
    private readonly ClosedXmlExporter _exporter = new ClosedXmlExporter();

    // ============== 空报表 ==============

    [Fact]
    public void Export_EmptyReport_ThrowsInvalidOperationException()
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            Pages = new List<RenderedPage>()
        };

        // ClosedXML requires at least one worksheet
        Assert.Throws<System.InvalidOperationException>(() => _exporter.Export(report));
    }

    [Fact]
    public void Export_SingleEmptyPage_GeneratesExcel()
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            Pages = new List<RenderedPage>
            {
                new RenderedPage { PageNumber = 1, TotalPages = 1, Elements = new List<RenderedElement>() }
            }
        };

        var bytes = _exporter.Export(report);
        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);
    }

    // ============== 文本元素 ==============

    [Fact]
    public void Export_SingleTextElement_GeneratesExcel()
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            Pages = new List<RenderedPage>
            {
                new RenderedPage
                {
                    PageNumber = 1,
                    TotalPages = 1,
                    Elements = new List<RenderedElement>
                    {
                        new RenderedTextElement
                        {
                            Id = "text1",
                            X = 10,
                            Y = 10,
                            Width = 50,
                            Height = 10,
                            Text = "Hello World",
                            Font = new FontDef { Family = "Arial", Size = 12 }
                        }
                    }
                }
            }
        };

        var bytes = _exporter.Export(report);
        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);
    }

    [Fact]
    public void Export_MultipleTextElements_GeneratesExcel()
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            Pages = new List<RenderedPage>
            {
                new RenderedPage
                {
                    PageNumber = 1,
                    TotalPages = 1,
                    Elements = new List<RenderedElement>
                    {
                        new RenderedTextElement
                        {
                            Id = "text1",
                            X = 10,
                            Y = 10,
                            Width = 50,
                            Height = 10,
                            Text = "Title",
                            Font = new FontDef { Family = "Arial", Size = 14, Bold = true }
                        },
                        new RenderedTextElement
                        {
                            Id = "text2",
                            X = 10,
                            Y = 25,
                            Width = 80,
                            Height = 8,
                            Text = "Content line 1",
                            Font = new FontDef { Family = "Arial", Size = 10 }
                        },
                        new RenderedTextElement
                        {
                            Id = "text3",
                            X = 10,
                            Y = 35,
                            Width = 80,
                            Height = 8,
                            Text = "Content line 2",
                            Font = new FontDef { Family = "Arial", Size = 10 }
                        }
                    }
                }
            }
        };

        var bytes = _exporter.Export(report);
        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);
    }

    // ============== 多页 ==============

    [Fact]
    public void Export_MultiplePages_GeneratesExcel()
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            Pages = new List<RenderedPage>
            {
                new RenderedPage
                {
                    PageNumber = 1,
                    TotalPages = 3,
                    Elements = new List<RenderedElement>
                    {
                        new RenderedTextElement
                        {
                            Id = "text1",
                            X = 10,
                            Y = 10,
                            Width = 50,
                            Height = 10,
                            Text = "Page 1",
                            Font = new FontDef()
                        }
                    }
                },
                new RenderedPage
                {
                    PageNumber = 2,
                    TotalPages = 3,
                    Elements = new List<RenderedElement>
                    {
                        new RenderedTextElement
                        {
                            Id = "text2",
                            X = 10,
                            Y = 10,
                            Width = 50,
                            Height = 10,
                            Text = "Page 2",
                            Font = new FontDef()
                        }
                    }
                },
                new RenderedPage
                {
                    PageNumber = 3,
                    TotalPages = 3,
                    Elements = new List<RenderedElement>
                    {
                        new RenderedTextElement
                        {
                            Id = "text3",
                            X = 10,
                            Y = 10,
                            Width = 50,
                            Height = 10,
                            Text = "Page 3",
                            Font = new FontDef()
                        }
                    }
                }
            }
        };

        var bytes = _exporter.Export(report);
        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);
    }

    // ============== 文件导出 ==============

    [Fact]
    public void ExportToFile_CreatesFile()
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            Pages = new List<RenderedPage>
            {
                new RenderedPage
                {
                    PageNumber = 1,
                    TotalPages = 1,
                    Elements = new List<RenderedElement>
                    {
                        new RenderedTextElement
                        {
                            Id = "text1",
                            X = 10,
                            Y = 10,
                            Width = 50,
                            Height = 10,
                            Text = "Test",
                            Font = new FontDef()
                        }
                    }
                }
            }
        };

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{System.Guid.NewGuid()}.xlsx");
        try
        {
            _exporter.ExportToFile(report, tempFile);
            Assert.True(File.Exists(tempFile));
            Assert.True(new FileInfo(tempFile).Length > 0);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExportToFile_CreatesDirectoryIfNotExists()
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            Pages = new List<RenderedPage>
            {
                new RenderedPage
                {
                    PageNumber = 1,
                    TotalPages = 1,
                    Elements = new List<RenderedElement>()
                }
            }
        };

        var tempDir = Path.Combine(Path.GetTempPath(), $"test_dir_{System.Guid.NewGuid()}");
        var tempFile = Path.Combine(tempDir, "test.xlsx");
        try
        {
            _exporter.ExportToFile(report, tempFile);
            Assert.True(Directory.Exists(tempDir));
            Assert.True(File.Exists(tempFile));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    // ============== 样式 ==============

    [Fact]
    public void Export_TextWithBoldFont_GeneratesExcel()
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            Pages = new List<RenderedPage>
            {
                new RenderedPage
                {
                    PageNumber = 1,
                    TotalPages = 1,
                    Elements = new List<RenderedElement>
                    {
                        new RenderedTextElement
                        {
                            Id = "text1",
                            X = 10,
                            Y = 10,
                            Width = 50,
                            Height = 10,
                            Text = "Bold Text",
                            Font = new FontDef { Bold = true, Size = 14 }
                        }
                    }
                }
            }
        };

        var bytes = _exporter.Export(report);
        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);
    }

    [Fact]
    public void Export_TextWithBackgroundColor_GeneratesExcel()
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            Pages = new List<RenderedPage>
            {
                new RenderedPage
                {
                    PageNumber = 1,
                    TotalPages = 1,
                    Elements = new List<RenderedElement>
                    {
                        new RenderedTextElement
                        {
                            Id = "text1",
                            X = 10,
                            Y = 10,
                            Width = 50,
                            Height = 10,
                            Text = "Colored Background",
                            Font = new FontDef(),
                            BackgroundColor = "#FFFF00"
                        }
                    }
                }
            }
        };

        var bytes = _exporter.Export(report);
        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);
    }

    // ============== 对齐 ==============

    [Fact]
    public void Export_TextWithDifferentAlignments_GeneratesExcel()
    {
        var report = new RenderedReport
        {
            Template = new ReportTemplate(),
            Pages = new List<RenderedPage>
            {
                new RenderedPage
                {
                    PageNumber = 1,
                    TotalPages = 1,
                    Elements = new List<RenderedElement>
                    {
                        new RenderedTextElement
                        {
                            Id = "text1",
                            X = 10,
                            Y = 10,
                            Width = 50,
                            Height = 10,
                            Text = "Left",
                            Font = new FontDef(),
                            Alignment = TextAlignment.Left
                        },
                        new RenderedTextElement
                        {
                            Id = "text2",
                            X = 10,
                            Y = 25,
                            Width = 50,
                            Height = 10,
                            Text = "Center",
                            Font = new FontDef(),
                            Alignment = TextAlignment.Center
                        },
                        new RenderedTextElement
                        {
                            Id = "text3",
                            X = 10,
                            Y = 40,
                            Width = 50,
                            Height = 10,
                            Text = "Right",
                            Font = new FontDef(),
                            Alignment = TextAlignment.Right
                        }
                    }
                }
            }
        };

        var bytes = _exporter.Export(report);
        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);
    }
}
