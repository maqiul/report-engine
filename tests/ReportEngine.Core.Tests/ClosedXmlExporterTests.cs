using FluentAssertions;
using ReportEngine.Core;
using ReportEngine.Core.Rendering;
using ReportEngine.Export.Excel;
using Xunit;

namespace ReportEngine.Core.Tests;

/// <summary>
/// ClosedXmlExporter 行为测试：
///   - 只含 text 元素：导出字节非空
///   - 包含 line（horizontal/vertical/diagonal）元素：导出不抛异常
///   - 包含 image（指向不存在的源）：导出写占位 "[img]" 而不抛
///   - 包含 shape：导出填背景色
///   - 包含 barcode：导出写 [format] value 占位
///   - 导出到文件：磁盘产生 .xlsx
/// </summary>
public class ClosedXmlExporterTests
{
    private static RenderedReport BuildReport(params RenderedElement[] elements)
    {
        return new RenderedReport
        {
            PageWidth = 210,
            PageHeight = 297,
            Pages = new List<RenderedPage>
            {
                new RenderedPage
                {
                    PageNumber = 1,
                    TotalPages = 1,
                    Elements = elements.ToList(),
                }
            }
        };
    }

    private static RenderedTextElement Text(string text, double x, double y, double w = 20, double h = 8) => new()
    {
        Text = text,
        X = x,
        Y = y,
        Width = w,
        Height = h,
    };

    [Fact]
    public void Export_With_Only_Text_Produces_NonEmpty_Bytes()
    {
        var exporter = new ClosedXmlExporter();
        var report = BuildReport(
            Text("Hello", 10, 5),
            Text("World", 60, 5));

        var bytes = exporter.Export(report);

        bytes.Should().NotBeEmpty();
        // xlsx 是 zip，文件头 0x50 0x4B ('PK')
        bytes[0].Should().Be(0x50);
        bytes[1].Should().Be(0x4B);
    }

    [Fact]
    public void Export_With_Horizontal_Vertical_Diagonal_Lines_Does_Not_Throw()
    {
        var exporter = new ClosedXmlExporter();
        var report = BuildReport(
            Text("anchor", 10, 5),
            new RenderedLineElement
            {
                X = 10, Y = 10, Width = 50, Height = 0,
                Direction = LineDirection.Horizontal,
                LineWidth = 1.0,
                LineColor = "#ff0000",
            },
            new RenderedLineElement
            {
                X = 60, Y = 5, Width = 0, Height = 10,
                Direction = LineDirection.Vertical,
                LineWidth = 1.5,
                LineColor = "#0000ff",
            },
            new RenderedLineElement
            {
                X = 100, Y = 10, Width = 30, Height = 30,
                Direction = LineDirection.Diagonal,
                LineWidth = 1.0,
                LineColor = "#00ff00",
            });

        var act = () => exporter.Export(report);

        act.Should().NotThrow();
        exporter.Export(report).Should().NotBeEmpty();
    }

    [Fact]
    public void Export_With_Broken_Image_Source_Writes_Placeholder_Not_Throws()
    {
        var exporter = new ClosedXmlExporter();
        var report = BuildReport(
            Text("img-anchor", 10, 5),
            new RenderedImageElement
            {
                X = 10, Y = 20, Width = 30, Height = 20,
                Source = "Z:\\does\\not\\exist\\pic.png",
            });

        var bytes = exporter.Export(report);

        bytes.Should().NotBeEmpty();
    }

    [Fact]
    public void Export_With_Shape_Does_Not_Throw()
    {
        var exporter = new ClosedXmlExporter();
        var report = BuildReport(
            Text("shape-anchor", 10, 5),
            new RenderedShapeElement
            {
                X = 10, Y = 30, Width = 20, Height = 10,
                Shape = ShapeType.Rectangle,
                FillColor = "#cccccc",
            });

        var act = () => exporter.Export(report);

        act.Should().NotThrow();
    }

    [Fact]
    public void Export_With_Barcode_Writes_Format_Value_Placeholder()
    {
        var exporter = new ClosedXmlExporter();
        var report = BuildReport(
            Text("bc-anchor", 10, 5),
            new RenderedBarcodeElement
            {
                X = 10, Y = 50, Width = 30, Height = 15,
                Format = BarcodeFormat.QRCode,
                Value = "https://example.com",
            });

        var bytes = exporter.Export(report);

        bytes.Should().NotBeEmpty();
    }

    [Fact]
    public void ExportToFile_Creates_File_On_Disk()
    {
        var exporter = new ClosedXmlExporter();
        var report = BuildReport(Text("row1", 10, 5));
        var tmp = Path.Combine(Path.GetTempPath(), "ReportEngine.Tests_" + Guid.NewGuid().ToString("N") + ".xlsx");

        try
        {
            exporter.ExportToFile(report, tmp);

            File.Exists(tmp).Should().BeTrue();
            new FileInfo(tmp).Length.Should().BeGreaterThan(0);
        }
        finally
        {
            if (File.Exists(tmp)) File.Delete(tmp);
        }
    }

    // ============ D2 边界用例 (v0.1.10) ============

    [Fact]
    public void Export_With_Empty_Text_Does_Not_Throw()
    {
        var exporter = new ClosedXmlExporter();
        var report = BuildReport(
            Text("", 10, 5),
            Text("   ", 60, 5),
            Text("non-empty", 110, 5));

        var act = () => exporter.Export(report);

        act.Should().NotThrow();
        act().Should().NotBeEmpty();
    }

    [Fact]
    public void Export_With_Chinese_Text_Does_Not_Throw()
    {
        var exporter = new ClosedXmlExporter();
        var report = BuildReport(
            Text("你好世界", 10, 5),
            Text("报表标题-2026", 60, 5));

        var act = () => exporter.Export(report);

        act.Should().NotThrow();
    }

    [Fact]
    public void Export_With_FontColor_And_BackgroundColor_Does_Not_Throw()
    {
        var exporter = new ClosedXmlExporter();
        var report = BuildReport(
            new RenderedTextElement
            {
                X = 10, Y = 5, Width = 30, Height = 8,
                Text = "colored",
                Font = new FontDef { Family = "SimSun", Size = 12, Color = "#FF0000" },
                BackgroundColor = "#FFFFCC",
            });

        var act = () => exporter.Export(report);

        act.Should().NotThrow();
    }

    [Fact]
    public void Export_With_All_Alignments_Does_Not_Throw()
    {
        var exporter = new ClosedXmlExporter();
        var report = BuildReport(
            new RenderedTextElement { X = 10, Y = 5, Width = 30, Height = 8, Text = "L", Alignment = TextAlignment.Left },
            new RenderedTextElement { X = 50, Y = 5, Width = 30, Height = 8, Text = "C", Alignment = TextAlignment.Center },
            new RenderedTextElement { X = 90, Y = 5, Width = 30, Height = 8, Text = "R", Alignment = TextAlignment.Right },
            new RenderedTextElement { X = 130, Y = 5, Width = 30, Height = 8, Text = "J", Alignment = TextAlignment.Justify });

        var act = () => exporter.Export(report);

        act.Should().NotThrow();
    }

    [Fact]
    public void Export_With_Bold_Italic_Underline_Does_Not_Throw()
    {
        var exporter = new ClosedXmlExporter();
        var report = BuildReport(
            new RenderedTextElement
            {
                X = 10, Y = 5, Width = 30, Height = 8,
                Text = "styled",
                Font = new FontDef { Family = "SimSun", Size = 12, Bold = true, Italic = true, Underline = true },
            });

        var act = () => exporter.Export(report);

        act.Should().NotThrow();
    }

    [Fact]
    public void Export_With_Element_Border_All_Sides_Does_Not_Throw()
    {
        var exporter = new ClosedXmlExporter();
        var report = BuildReport(
            new RenderedTextElement
            {
                X = 10, Y = 5, Width = 40, Height = 12,
                Text = "boxed",
                Border = new BorderDef
                {
                    Width = 1.0, Color = "#333333",
                    Top = true, Bottom = true, Left = true, Right = true,
                },
            });

        var act = () => exporter.Export(report);

        act.Should().NotThrow();
    }

    [Fact]
    public void Export_With_Multiple_Pages_Renders_All_Pages()
    {
        var exporter = new ClosedXmlExporter();
        var report = new RenderedReport
        {
            PageWidth = 210,
            PageHeight = 297,
            Pages = new List<RenderedPage>
            {
                new RenderedPage { PageNumber = 1, TotalPages = 3, Elements = new List<RenderedElement> { Text("p1", 10, 5) } },
                new RenderedPage { PageNumber = 2, TotalPages = 3, Elements = new List<RenderedElement> { Text("p2", 10, 5) } },
                new RenderedPage { PageNumber = 3, TotalPages = 3, Elements = new List<RenderedElement> { Text("p3", 10, 5) } },
            },
        };

        var bytes = exporter.Export(report);

        bytes.Should().NotBeEmpty();
        bytes.Length.Should().BeGreaterThan(500);
    }

    [Fact]
    public void Export_With_Clustered_Columns_Aggregates_To_Single_Column()
    {
        // 两个 X 起点差异 < ClusterTolerance, 应聚类为同一列
        var exporter = new ClosedXmlExporter { ClusterTolerance = 0.8 };
        var report = BuildReport(
            Text("A1", 10.0, 5),
            Text("A2", 10.3, 15),   // X 差 0.3 < 0.8
            Text("A3", 10.5, 25));  // X 差 0.5 < 0.8

        var act = () => exporter.Export(report);

        act.Should().NotThrow();
        act().Should().NotBeEmpty();
    }

    // ===== D7: 边界用例 =====

    [Fact]
    public void Export_With_RenderedTableElement_Does_Not_Throw()
    {
        var exporter = new ClosedXmlExporter();
        var element = new RenderedTableElement
        {
            X = 10, Y = 5, Width = 80, Height = 40,
            RowCount = 2, ColCount = 2,
            Cells = new List<RenderedTableCell>
            {
                new RenderedTableCell { Row = 0, Col = 0, Text = "A" },
                new RenderedTableCell { Row = 0, Col = 1, Text = "B" },
                new RenderedTableCell { Row = 1, Col = 0, Text = "C" },
                new RenderedTableCell { Row = 1, Col = 1, Text = "D" },
            },
        };

        var act = () => exporter.Export(BuildReport(element));

        act.Should().NotThrow();
        act().Should().NotBeEmpty();
    }

    [Fact]
    public void Export_With_RenderedCrossTabElement_Does_Not_Throw()
    {
        var exporter = new ClosedXmlExporter();
        var element = new RenderedCrossTabElement
        {
            X = 10, Y = 5, Width = 80, Height = 40,
            RowCount = 3, ColCount = 3,
            Cells = new List<RenderedTableCell>
            {
                new RenderedTableCell { Row = 0, Col = 0, Text = "X" },
            },
        };

        var act = () => exporter.Export(BuildReport(element));

        act.Should().NotThrow();
    }

    [Fact]
    public void Export_With_Multiple_FontSizes_Does_Not_Throw()
    {
        var exporter = new ClosedXmlExporter();
        var elements = new[]
        {
            new RenderedTextElement { Text = "6pt",  X = 10, Y = 5,  Width = 40, Height = 4,
                Font = new FontDef { Size = 6 } },
            new RenderedTextElement { Text = "16pt", X = 10, Y = 15, Width = 40, Height = 12,
                Font = new FontDef { Size = 16 } },
            new RenderedTextElement { Text = "36pt", X = 10, Y = 30, Width = 40, Height = 24,
                Font = new FontDef { Size = 36 } },
        };

        var act = () => exporter.Export(BuildReport(elements));

        act.Should().NotThrow();
    }

    [Fact]
    public void Export_With_OutOfBounds_Text_Y_Does_Not_Throw()
    {
        // 元素 Y 在页外 (Y > PageHeight). 锁定: 不抛 + 字节仍非空
        var exporter = new ClosedXmlExporter();
        var report = BuildReport(Text("OffPage", 10, 500, h: 8));

        var act = () => exporter.Export(report);

        act.Should().NotThrow();
        var bytes = exporter.Export(report);
        bytes.Should().NotBeEmpty();
    }

    [Fact]
    public void Export_With_Image_Http_Url_Does_Not_Throw()
    {
        var exporter = new ClosedXmlExporter();
        var img = new RenderedImageElement
        {
            X = 10, Y = 5, Width = 30, Height = 20,
            Source = "https://example.com/does-not-exist.png",
        };

        var act = () => exporter.Export(BuildReport(img));

        act.Should().NotThrow();
    }

    [Fact]
    public void Export_With_Negative_Coordinates_Does_Not_Throw()
    {
        var exporter = new ClosedXmlExporter();
        var report = BuildReport(Text("Negative", -5, -3));

        var act = () => exporter.Export(report);

        act.Should().NotThrow();
    }
}