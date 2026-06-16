using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using ReportEngine.Core;
using ReportEngine.Core.Data;
using ReportEngine.Core.Parsing;
using ReportEngine.Core.Rendering;
using ReportEngine.Core.SubReports;
using ReportEngine.Export.Excel;
using Xunit;

namespace ReportEngine.Export.Excel.Tests;

/// <summary>
/// 端到端集成测试：模板解析 → 渲染 → Excel 导出
/// </summary>
public class EndToEndExcelTests
{
    private readonly TemplateParser _parser = new();
    private readonly InMemoryTemplateResolver _resolver = new();
    private readonly ClosedXmlExporter _excelExporter = new();

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

    private byte[] RenderAndExportExcel(ReportTemplate template, Dictionary<string, List<Dictionary<string, object>>> data)
    {
        var renderer = new ReportRenderer(_resolver);
        var rendered = renderer.RenderAsync(template, data).GetAwaiter().GetResult();
        return _excelExporter.Export(rendered);
    }

    private XLWorkbook OpenExcel(byte[] bytes)
    {
        return new XLWorkbook(new MemoryStream(bytes));
    }

    #endregion

    // ============== 基本流程 ==============

    [Fact]
    public void EndToEnd_SimpleTemplate_ExcelGenerated()
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

        var xlsxBytes = RenderAndExportExcel(template, data);
        Assert.NotNull(xlsxBytes);
        Assert.True(xlsxBytes.Length > 0);

        // 验证是有效的 xlsx 文件（PK 头）
        Assert.Equal((byte)'P', xlsxBytes[0]);
        Assert.Equal((byte)'K', xlsxBytes[1]);
    }

    [Fact]
    public void EndToEnd_ParseJsonThenExport_ExcelGenerated()
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
                new() { ["id"] = 2, ["name"] = "Item B" }
            }
        };

        var xlsxBytes = RenderAndExportExcel(template, data);
        Assert.True(xlsxBytes.Length > 100);

        using var wb = OpenExcel(xlsxBytes);
        Assert.True(wb.Worksheets.Count >= 1);
    }

    [Fact]
    public void EndToEnd_SerializeDeserializeThenExport_ExcelGenerated()
    {
        var template = new ReportTemplate();
        template.DataSources.Add(new DataSourceDef { Name = "ds" });
        template.Bands.Add(new Band
        {
            Type = BandType.Detail,
            Height = 10,
            DataSource = "ds",
            Elements = { new TextElement { Text = "{{currentRow.value}}", X = 10, Y = 0, Width = 80, Height = 8 } }
        });

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

        var xlsxBytes = RenderAndExportExcel(restored, data);
        Assert.True(xlsxBytes.Length > 100);
    }

    // ============== 内容验证 ==============

    [Fact]
    public void EndToEnd_DataContent_Verified()
    {
        var template = new ReportTemplate();
        template.Bands.Add(new Band
        {
            Type = BandType.Detail,
            Height = 10,
            DataSource = "ds",
            Elements =
            {
                new TextElement { Text = "{{currentRow.name}}", X = 10, Y = 0, Width = 50, Height = 8 },
                new TextElement { Text = "{{currentRow.value}}", X = 70, Y = 0, Width = 30, Height = 8 }
            }
        });

        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            ["ds"] = new()
            {
                new() { ["name"] = "Alpha", ["value"] = 100 },
                new() { ["name"] = "Beta", ["value"] = 200 }
            }
        };

        var xlsxBytes = RenderAndExportExcel(template, data);
        using var wb = OpenExcel(xlsxBytes);

        var ws = wb.Worksheets.First();
        // 验证有内容被写入
        Assert.True(ws.CellsUsed().Any());
    }

    // ============== 多 Band 类型 ==============

    [Fact]
    public void EndToEnd_HeaderAndFooter_ExcelGenerated()
    {
        var template = new ReportTemplate();

        template.Bands.Add(new Band
        {
            Type = BandType.Header,
            Height = 15,
            Elements = { new TextElement { Text = "Report Title", X = 10, Y = 0, Width = 190, Height = 10, Font = new FontDef { Size = 14, Bold = true } } }
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
            Elements = { new TextElement { Text = "Page Footer", X = 10, Y = 0, Width = 190, Height = 8 } }
        });

        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            ["ds"] = new()
            {
                new() { ["name"] = "Row 1" },
                new() { ["name"] = "Row 2" }
            }
        };

        var xlsxBytes = RenderAndExportExcel(template, data);
        Assert.True(xlsxBytes.Length > 100);
    }

    // ============== 大数据量 ==============

    [Fact]
    public void EndToEnd_LargeDataset_ExcelGenerated()
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
        for (int i = 0; i < 200; i++)
            rows.Add(new Dictionary<string, object> { ["id"] = i + 1 });

        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            ["ds"] = rows
        };

        var xlsxBytes = RenderAndExportExcel(template, data);
        Assert.True(xlsxBytes.Length > 1000);

        using var wb = OpenExcel(xlsxBytes);
        var ws = wb.Worksheets.First();
        // 验证有单元格被写入（聚类算法可能合并相近元素）
        Assert.True(ws.CellsUsed().Any());
    }

    // ============== 空数据 ==============

    [Fact]
    public void EndToEnd_EmptyData_ExcelGenerated()
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

        var xlsxBytes = RenderAndExportExcel(template, data);
        Assert.True(xlsxBytes.Length > 0);
    }

    // ============== 文件导出 ==============

    [Fact]
    public async Task EndToEnd_ExportToFile_FileCreated()
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
        var rendered = await renderer.RenderAsync(template, data);

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_e2e_{Guid.NewGuid()}.xlsx");
        try
        {
            _excelExporter.ExportToFile(rendered, tempFile);
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
    public void EndToEnd_MultipleDataSources_ExcelGenerated()
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

        var xlsxBytes = RenderAndExportExcel(template, data);
        Assert.True(xlsxBytes.Length > 100);
    }

    // ============== 格式化 ==============

    [Fact]
    public void EndToEnd_WithFormatting_ExcelGenerated()
    {
        var template = new ReportTemplate();
        template.Bands.Add(new Band
        {
            Type = BandType.Detail,
            Height = 10,
            DataSource = "ds",
            Elements =
            {
                new TextElement
                {
                    Text = "{{currentRow.name}}",
                    X = 10, Y = 0, Width = 60, Height = 8,
                    Font = new FontDef { Size = 12, Bold = true, Color = "#FF0000" }
                },
                new TextElement
                {
                    Text = "{{currentRow.value}}",
                    X = 80, Y = 0, Width = 40, Height = 8,
                    Alignment = TextAlignment.Right,
                    Format = "#,##0.00"
                }
            }
        });

        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            ["ds"] = new()
            {
                new() { ["name"] = "Bold Red", ["value"] = 1234.56 }
            }
        };

        var xlsxBytes = RenderAndExportExcel(template, data);
        Assert.True(xlsxBytes.Length > 100);
    }

    // ============== 子报表 ==============

    [Fact]
    public void EndToEnd_SubReport_ExcelGenerated()
    {
        // 子模板
        var childJson = @"{
            ""version"": ""1.0"",
            ""dataSources"": [{""name"": ""items""}],
            ""bands"": [{
                ""type"": ""detail"",
                ""height"": 8,
                ""dataSource"": ""items"",
                ""elements"": [{
                    ""type"": ""text"",
                    ""text"": ""  - {{currentRow.name}}"",
                    ""x"": 10, ""y"": 0, ""width"": 100, ""height"": 6
                }]
            }]
        }";
        _resolver.Add("child.rptx", childJson);

        // 主模板
        var template = new ReportTemplate();
        template.DataSources.Add(new DataSourceDef { Name = "orders" });
        template.Bands.Add(new Band
        {
            Type = BandType.Detail,
            Height = 12,
            DataSource = "orders",
            Elements = {
                new TextElement { Text = "Order: {{currentRow.id}}", X = 10, Y = 0, Width = 100, Height = 8 },
                new SubReportElement {
                    TemplateRef = "child.rptx",
                    X = 10, Y = 8, Width = 180, Height = 4,
                    DataBinding = new SubReportDataBinding { Source = "orders" }
                }
            }
        });

        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            ["orders"] = new() { new() { ["id"] = "ORD-001" } }
        };

        var xlsxBytes = RenderAndExportExcel(template, data);
        Assert.True(xlsxBytes.Length > 100);

        using var wb = OpenExcel(xlsxBytes);
        Assert.True(wb.Worksheets.Count >= 1);
    }

    [Fact]
    public void EndToEnd_SubReportJsonRoundTrip_ExcelGenerated()
    {
        // 子模板
        var childJson = @"{
            ""version"": ""1.0"",
            ""dataSources"": [{""name"": ""items""}],
            ""bands"": [{
                ""type"": ""detail"",
                ""height"": 8,
                ""dataSource"": ""items"",
                ""elements"": [{
                    ""type"": ""text"",
                    ""text"": ""Item: {{currentRow.name}}"",
                    ""x"": 10, ""y"": 0, ""width"": 100, ""height"": 6
                }]
            }]
        }";
        _resolver.Add("items.rptx", childJson);

        // 主模板 JSON → 解析 → 序列化 → 反序列化 → 渲染 → 导出
        var mainJson = @"{
            ""version"": ""1.0"",
            ""dataSources"": [{""name"": ""ds""}],
            ""bands"": [{
                ""type"": ""detail"",
                ""height"": 15,
                ""dataSource"": ""ds"",
                ""elements"": [{
                    ""type"": ""text"",
                    ""text"": ""{{currentRow.title}}"",
                    ""x"": 10, ""y"": 0, ""width"": 100, ""height"": 8
                }, {
                    ""type"": ""subreport"",
                    ""templateRef"": ""items.rptx"",
                    ""x"": 10, ""y"": 8, ""width"": 180, ""height"": 5,
                    ""dataBinding"": {""source"": ""ds""}
                }]
            }]
        }";

        var template = _parser.Parse(mainJson);
        var json2 = _parser.Serialize(template);
        var restored = _parser.Parse(json2);

        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            ["ds"] = new() { new() { ["title"] = "Test" } }
        };

        var xlsxBytes = RenderAndExportExcel(restored, data);
        Assert.True(xlsxBytes.Length > 100);

        using var wb = OpenExcel(xlsxBytes);
        Assert.True(wb.Worksheets.Count >= 1);
    }

    [Fact]
    public void EndToEnd_SubReportMissing_ThrowsException()
    {
        var template = new ReportTemplate();
        template.DataSources.Add(new DataSourceDef { Name = "ds" });
        template.Bands.Add(new Band
        {
            Type = BandType.Detail,
            Height = 15,
            DataSource = "ds",
            Elements = {
                new TextElement { Text = "Main", X = 10, Y = 0, Width = 80, Height = 8 },
                new SubReportElement {
                    TemplateRef = "nonexistent.rptx",
                    X = 10, Y = 8, Width = 180, Height = 5,
                    DataBinding = new SubReportDataBinding { Source = "ds" }
                }
            }
        });

        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            ["ds"] = new() { new() { ["name"] = "test" } }
        };

        Assert.Throws<Exception>(() => RenderAndExportExcel(template, data));
    }

    [Fact]
    public void EndToEnd_NestedSubReport_ExcelGenerated()
    {
        // 孙模板
        var grandchildJson = @"{
            ""version"": ""1.0"",
            ""dataSources"": [{""name"": ""details""}],
            ""bands"": [{
                ""type"": ""detail"",
                ""height"": 6,
                ""dataSource"": ""details"",
                ""elements"": [{
                    ""type"": ""text"",
                    ""text"": ""    * {{currentRow.info}}"",
                    ""x"": 10, ""y"": 0, ""width"": 100, ""height"": 5
                }]
            }]
        }";
        _resolver.Add("grandchild.rptx", grandchildJson);

        // 子模板：含嵌套子报表
        var childJson = @"{
            ""version"": ""1.0"",
            ""dataSources"": [{""name"": ""items""}],
            ""bands"": [{
                ""type"": ""detail"",
                ""height"": 12,
                ""dataSource"": ""items"",
                ""elements"": [{
                    ""type"": ""text"",
                    ""text"": ""  - {{currentRow.name}}"",
                    ""x"": 10, ""y"": 0, ""width"": 100, ""height"": 6
                }, {
                    ""type"": ""subreport"",
                    ""templateRef"": ""grandchild.rptx"",
                    ""x"": 10, ""y"": 6, ""width"": 160, ""height"": 5,
                    ""dataBinding"": {""source"": ""items""}
                }]
            }]
        }";
        _resolver.Add("child.rptx", childJson);

        // 主模板
        var template = new ReportTemplate();
        template.DataSources.Add(new DataSourceDef { Name = "ds" });
        template.Bands.Add(new Band
        {
            Type = BandType.Detail,
            Height = 15,
            DataSource = "ds",
            Elements = {
                new TextElement { Text = "Root: {{currentRow.title}}", X = 10, Y = 0, Width = 100, Height = 8 },
                new SubReportElement {
                    TemplateRef = "child.rptx",
                    X = 10, Y = 8, Width = 180, Height = 5,
                    DataBinding = new SubReportDataBinding { Source = "ds" }
                }
            }
        });

        var data = new Dictionary<string, List<Dictionary<string, object>>>
        {
            ["ds"] = new() { new() { ["title"] = "Root Item" } }
        };

        var xlsxBytes = RenderAndExportExcel(template, data);
        Assert.True(xlsxBytes.Length > 100);

        using var wb = OpenExcel(xlsxBytes);
        Assert.True(wb.Worksheets.Count >= 1);
    }
}
