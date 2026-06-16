using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Parsing;
using ReportEngine.Core.Rendering;
using ReportEngine.Core.SubReports;
using ReportEngine.Export.Pdf;
using Xunit;

namespace ReportEngine.Export.Pdf.Tests;

/// <summary>
/// 端到端集成测试：模板解析 → 渲染 → PDF 导出
/// </summary>
public class EndToEndPdfTests
{
    private readonly TemplateParser _parser = new();
    private readonly InMemoryTemplateResolver _resolver = new();
    private readonly PdfSharpExporter _pdfExporter = new();

    #region Helper

    private class InMemoryTemplateResolver : ITemplateResolver
    {
        private readonly Dictionary<string, string> _templates = new();
        private readonly TemplateParser _parser = new();
        public void Add(string name, string json) => _templates[name] = json;
        public Task<ReportTemplate> ResolveAsync(string templateRef)
        {
            if (_templates.TryGetValue(templateRef, out var json))
                return Task.FromResult(_parser.Parse(json));
            throw new Exception($"Template '{templateRef}' not found");
        }
        public bool Exists(string templateRef) => _templates.ContainsKey(templateRef);
    }

    private byte[] RenderAndExportPdf(ReportTemplate template, Dictionary<string, List<Dictionary<string, object>>> data)
    {
        var renderer = new ReportRenderer(_resolver);
        var rendered = renderer.RenderAsync(template, data).GetAwaiter().GetResult();
        return _pdfExporter.Export(rendered);
    }

    #endregion

    // ============== 基本流程 ==============

    [Fact]
    public void EndToEnd_SimpleTemplate_PdfGenerated()
    {
        var template = new ReportTemplate();
        template.Bands.Add(new Band
        {
            Type = BandType.Detail,
            Height = 10,
            DataSource = "ds",
            Elements = { new TextElement { Text = "{{currentRow.name}}", X = 10, Y = 0, Width = 80, Height = 8 } }
        });

        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            ["ds"] = new()
            {
                new() { ["name"] = "Alice" },
                new() { ["name"] = "Bob" }
            }
        };

        var pdfBytes = RenderAndExportPdf(template, data);
        Assert.NotNull(pdfBytes);
        Assert.True(pdfBytes.Length > 0);
        // PDF 文件头应该是 %PDF
        Assert.Equal('%', (char)pdfBytes[0]);
        Assert.Equal('P', (char)pdfBytes[1]);
        Assert.Equal('D', (char)pdfBytes[2]);
        Assert.Equal('F', (char)pdfBytes[3]);
    }

    [Fact]
    public void EndToEnd_ParseJsonThenExport_PdfGenerated()
    {
        var json = @"{
            ""version"": ""1.0"",
            ""dataSources"": [{""name"": ""ds""}],
            ""bands"": [{
                ""type"": ""detail"",
                ""height"": 10,
                ""dataSource"": ""ds"",
                ""elements"": [{
                    ""type"": ""text"",
                    ""text"": ""{{currentRow.id}} - {{currentRow.name}}"",
                    ""x"": 10, ""y"": 0, ""width"": 100, ""height"": 8
                }]
            }]
        }";

        var template = _parser.Parse(json);
        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            ["ds"] = new()
            {
                new() { ["id"] = 1, ["name"] = "Item A" },
                new() { ["id"] = 2, ["name"] = "Item B" },
                new() { ["id"] = 3, ["name"] = "Item C" }
            }
        };

        var pdfBytes = RenderAndExportPdf(template, data);
        Assert.True(pdfBytes.Length > 100);
        Assert.Equal('%', (char)pdfBytes[0]);
    }

    [Fact]
    public void EndToEnd_SerializeDeserializeThenExport_PdfGenerated()
    {
        var template = new ReportTemplate();
        template.Author = "Test Author";
        template.Description = "Integration test template";
        template.DataSources.Add(new DataSourceDef { Name = "ds" });
        template.Bands.Add(new Band
        {
            Type = BandType.Header,
            Height = 20,
            Elements = { new TextElement { Text = "Report Header", X = 10, Y = 5, Width = 190, Height = 10, Font = new FontDef { Size = 14, Bold = true } } }
        });
        template.Bands.Add(new Band
        {
            Type = BandType.Detail,
            Height = 10,
            DataSource = "ds",
            Elements = { new TextElement { Text = "{{currentRow.value}}", X = 10, Y = 0, Width = 80, Height = 8 } }
        });

        // 序列化 → 反序列化 → 导出
        var json = _parser.Serialize(template);
        var restored = _parser.Parse(json);

        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            ["ds"] = new()
            {
                new() { ["value"] = "Hello" },
                new() { ["value"] = "World" }
            }
        };

        var pdfBytes = RenderAndExportPdf(restored, data);
        Assert.True(pdfBytes.Length > 100);
    }

    // ============== 多 Band 类型 ==============

    [Fact]
    public void EndToEnd_AllBandTypes_PdfGenerated()
    {
        var template = new ReportTemplate();
        template.Page.Margin = new Margin { Top = 15, Bottom = 15, Left = 10, Right = 10 };

        template.Bands.Add(new Band
        {
            Type = BandType.ReportHeader,
            Height = 15,
            Elements = { new TextElement { Text = "Report Header", X = 10, Y = 0, Width = 190, Height = 10 } }
        });

        template.Bands.Add(new Band
        {
            Type = BandType.Header,
            Height = 10,
            RepeatOnNewPage = true,
            Elements = { new TextElement { Text = "Page Header", X = 10, Y = 0, Width = 190, Height = 8 } }
        });

        template.Bands.Add(new Band
        {
            Type = BandType.Detail,
            Height = 10,
            DataSource = "ds",
            Elements = { new TextElement { Text = "{{currentRow.name}}", X = 10, Y = 0, Width = 80, Height = 8 } }
        });

        template.Bands.Add(new Band
        {
            Type = BandType.Footer,
            Height = 10,
            RepeatOnNewPage = true,
            Elements = { new TextElement { Text = "Page Footer", X = 10, Y = 0, Width = 190, Height = 8 } }
        });

        template.Bands.Add(new Band
        {
            Type = BandType.ReportFooter,
            Height = 10,
            DataSource = "ds",
            Elements = { new TextElement { Text = "Report Footer", X = 10, Y = 0, Width = 190, Height = 8 } }
        });

        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            ["ds"] = new()
            {
                new() { ["name"] = "Row 1" },
                new() { ["name"] = "Row 2" },
                new() { ["name"] = "Row 3" }
            }
        };

        var pdfBytes = RenderAndExportPdf(template, data);
        Assert.True(pdfBytes.Length > 100);
    }

    // ============== 多元素类型 ==============

    [Fact]
    public void EndToEnd_MultipleElementTypes_PdfGenerated()
    {
        var template = new ReportTemplate();
        var detail = new Band { Type = BandType.Detail, Height = 30, DataSource = "ds" };

        detail.Elements.Add(new TextElement { Text = "{{currentRow.name}}", X = 10, Y = 0, Width = 60, Height = 8 });
        detail.Elements.Add(new LineElement { X = 10, Y = 10, Width = 180, Height = 0, LineWidth = 0.5 });
        detail.Elements.Add(new ShapeElement { X = 10, Y = 15, Width = 20, Height = 10, Shape = ShapeType.Rectangle });
        detail.Elements.Add(new ImageElement { X = 40, Y = 15, Width = 20, Height = 10, Source = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==" });

        template.Bands.Add(detail);

        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            ["ds"] = new()
            {
                new() { ["name"] = "Test Item" }
            }
        };

        var pdfBytes = RenderAndExportPdf(template, data);
        Assert.True(pdfBytes.Length > 100);
    }

    // ============== 大数据量 ==============

    [Fact]
    public void EndToEnd_LargeDataset_PdfGenerated()
    {
        var template = new ReportTemplate();
        template.Bands.Add(new Band
        {
            Type = BandType.Detail,
            Height = 8,
            DataSource = "ds",
            Elements = { new TextElement { Text = "{{currentRow.id}}", X = 10, Y = 0, Width = 50, Height = 6 } }
        });

        var rows = new List<Dictionary<string, object>>();
        for (int i = 0; i < 500; i++)
            rows.Add(new Dictionary<string, object> { ["id"] = i + 1 });

        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            ["ds"] = rows
        };

        var pdfBytes = RenderAndExportPdf(template, data);
        Assert.True(pdfBytes.Length > 1000);

        // 验证生成了多页
        var renderer = new ReportRenderer(_resolver);
        var rendered = renderer.RenderAsync(template, data).GetAwaiter().GetResult();
        Assert.True(rendered.Pages.Count > 1);
    }

    // ============== 分组报表 ==============

    [Fact]
    public void EndToEnd_GroupedReport_PdfGenerated()
    {
        var template = new ReportTemplate();

        template.Bands.Add(new Band
        {
            Type = BandType.GroupHeader,
            Height = 12,
            DataSource = "ds",
            Group = new GroupDef { Expression = "{{currentRow.category}}" },
            Elements = { new TextElement { Text = "Group: {{currentRow.category}}", X = 10, Y = 0, Width = 100, Height = 10, Font = new FontDef { Bold = true } } }
        });

        template.Bands.Add(new Band
        {
            Type = BandType.Detail,
            Height = 8,
            DataSource = "ds",
            Elements = { new TextElement { Text = "{{currentRow.name}}", X = 20, Y = 0, Width = 80, Height = 6 } }
        });

        template.Bands.Add(new Band
        {
            Type = BandType.GroupFooter,
            Height = 8,
            DataSource = "ds",
            Elements = { new TextElement { Text = "---", X = 10, Y = 0, Width = 100, Height = 6 } }
        });

        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            ["ds"] = new()
            {
                new() { ["category"] = "A", ["name"] = "Item A1" },
                new() { ["category"] = "A", ["name"] = "Item A2" },
                new() { ["category"] = "B", ["name"] = "Item B1" },
                new() { ["category"] = "B", ["name"] = "Item B2" },
                new() { ["category"] = "B", ["name"] = "Item B3" },
            }
        };

        var pdfBytes = RenderAndExportPdf(template, data);
        Assert.True(pdfBytes.Length > 100);
    }

    // ============== 表达式与格式化 ==============

    [Fact]
    public void EndToEnd_ExpressionsAndFormatting_PdfGenerated()
    {
        var template = new ReportTemplate();
        var detail = new Band { Type = BandType.Detail, Height = 10, DataSource = "ds" };

        detail.Elements.Add(new TextElement { Text = "{{currentRow.price}}", X = 10, Y = 0, Width = 40, Height = 8, Format = "#,##0.00" });
        detail.Elements.Add(new TextElement { Text = "{{currentRow.date}}", X = 60, Y = 0, Width = 50, Height = 8, Format = "yyyy-MM-dd" });
        detail.Elements.Add(new TextElement { Text = "{{PAGE_NUMBER}} / {{TOTAL_PAGES}}", X = 150, Y = 0, Width = 40, Height = 8 });

        template.Bands.Add(detail);

        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            ["ds"] = new()
            {
                new() { ["price"] = 1234.56, ["date"] = new DateTime(2024, 6, 15) },
                new() { ["price"] = 789.00, ["date"] = new DateTime(2024, 7, 20) }
            }
        };

        var pdfBytes = RenderAndExportPdf(template, data);
        Assert.True(pdfBytes.Length > 100);
    }

    // ============== 空数据 ==============

    [Fact]
    public void EndToEnd_EmptyData_PdfGenerated()
    {
        var template = new ReportTemplate();
        template.Bands.Add(new Band
        {
            Type = BandType.Header,
            Height = 20,
            Elements = { new TextElement { Text = "Empty Report", X = 10, Y = 5, Width = 190, Height = 10 } }
        });
        template.Bands.Add(new Band
        {
            Type = BandType.Detail,
            Height = 10,
            DataSource = "ds",
            Elements = { new TextElement { Text = "{{currentRow.name}}", X = 10, Y = 0, Width = 80, Height = 8 } }
        });

        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            ["ds"] = new List<Dictionary<string, object>>()
        };

        var pdfBytes = RenderAndExportPdf(template, data);
        Assert.True(pdfBytes.Length > 0);
    }

    // ============== 文件导出 ==============

    [Fact]
    public void EndToEnd_ExportToFile_FileCreated()
    {
        var template = new ReportTemplate();
        template.Bands.Add(new Band
        {
            Type = BandType.Detail,
            Height = 10,
            DataSource = "ds",
            Elements = { new TextElement { Text = "Test", X = 10, Y = 0, Width = 80, Height = 8 } }
        });

        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            ["ds"] = new() { new() { ["x"] = 1 } }
        };

        var renderer = new ReportRenderer(_resolver);
        var rendered = renderer.RenderAsync(template, data).GetAwaiter().GetResult();

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_e2e_{Guid.NewGuid()}.pdf");
        try
        {
            _pdfExporter.ExportToFile(rendered, tempFile);
            Assert.True(File.Exists(tempFile));
            Assert.True(new FileInfo(tempFile).Length > 0);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    // ============== 多数据源 ==============

    [Fact]
    public void EndToEnd_MultipleDataSources_PdfGenerated()
    {
        var template = new ReportTemplate();

        template.Bands.Add(new Band
        {
            Type = BandType.Detail,
            Height = 10,
            DataSource = "ds1",
            Elements = { new TextElement { Text = "DS1: {{currentRow.name}}", X = 10, Y = 0, Width = 80, Height = 8 } }
        });

        template.Bands.Add(new Band
        {
            Type = BandType.Detail,
            Height = 10,
            DataSource = "ds2",
            Elements = { new TextElement { Text = "DS2: {{currentRow.value}}", X = 10, Y = 0, Width = 80, Height = 8 } }
        });

        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            ["ds1"] = new() { new() { ["name"] = "From DS1" } },
            ["ds2"] = new() { new() { ["value"] = "From DS2" } }
        };

        var pdfBytes = RenderAndExportPdf(template, data);
        Assert.True(pdfBytes.Length > 100);
    }
}
