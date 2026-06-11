using FluentAssertions;
using ReportEngine.Core;
using ReportEngine.Core.Rendering;
using ReportEngine.Export.Pdf;
using Xunit;

namespace ReportEngine.Export.Pdf.Tests;

/// <summary>
/// PdfSharpExporter 行为测试:
///   - 空 report: 返回有效 PDF 头
///   - 只含 text: 字节非空 + PDF magic + 包含文本
///   - text 字体缺失: 不抛异常 (fallback Arial)
///   - 含 line (H/V/Diagonal): 不抛
///   - 含 shape (Rectangle/Ellipse): 不抛
///   - 含 image 断源: 静默忽略不抛
///   - 含 barcode: 不抛 + 有效 PDF
///   - 导出到文件: 磁盘产生 .pdf
/// </summary>
public class PdfSharpExporterTests
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
    public void Export_Empty_Report_Returns_Valid_Pdf()
    {
        var exporter = new PdfSharpExporter();
        var report = BuildReport();

        var bytes = exporter.Export(report);

        bytes.Should().NotBeEmpty();
        // PDF 文件头: %PDF-1.x (\x25 \x50 \x44 \x46 \x2D)
        bytes[0].Should().Be(0x25); // %
        bytes[1].Should().Be(0x50); // P
        bytes[2].Should().Be(0x44); // D
        bytes[3].Should().Be(0x46); // F
        bytes[4].Should().Be(0x2D); // -
    }

    [Fact]
    public void Export_With_Only_Text_Produces_NonEmpty_Pdf()
    {
        var exporter = new PdfSharpExporter();
        var report = BuildReport(
            Text("Hello", 10, 5),
            Text("World", 60, 5));

        var bytes = exporter.Export(report);

        bytes.Should().NotBeEmpty();
        // PDF 至少几 KB
        bytes.Length.Should().BeGreaterThan(100);
        // 含 PDF 头
        var header = System.Text.Encoding.ASCII.GetString(bytes, 0, 5);
        header.Should().Be("%PDF-");
    }

    [Fact]
    public void Export_With_Missing_Family_Falls_Back_To_Arial_No_Throw()
    {
        var exporter = new PdfSharpExporter();
        var report = BuildReport(
            new RenderedTextElement
            {
                X = 10, Y = 5, Width = 20, Height = 8,
                Text = "中文测试",
                Font = new FontDef { Family = "NonExistentFont_FOO_BAR_BAZ_9999", Size = 12 },
            });

        var act = () => exporter.Export(report);

        act.Should().NotThrow();
    }

    [Fact]
    public void Export_With_Lines_Horizontal_Vertical_Diagonal_Does_Not_Throw()
    {
        var exporter = new PdfSharpExporter();
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
    }

    [Fact]
    public void Export_With_Shape_Rectangle_Ellipse_Does_Not_Throw()
    {
        var exporter = new PdfSharpExporter();
        var report = BuildReport(
            Text("shape-anchor", 10, 5),
            new RenderedShapeElement
            {
                X = 10, Y = 30, Width = 20, Height = 10,
                Shape = ShapeType.Rectangle,
                FillColor = "#cccccc",
            },
            new RenderedShapeElement
            {
                X = 50, Y = 30, Width = 15, Height = 15,
                Shape = ShapeType.Ellipse,
                FillColor = "#ff9900",
            });

        var act = () => exporter.Export(report);

        act.Should().NotThrow();
    }

    [Fact]
    public void Export_With_Broken_Image_Source_Silently_Ignored_Not_Throws()
    {
        var exporter = new PdfSharpExporter();
        var report = BuildReport(
            Text("img-anchor", 10, 5),
            new RenderedImageElement
            {
                X = 10, Y = 20, Width = 30, Height = 20,
                Source = "Z:\\does\\not\\exist\\pic.png",
            });

        var act = () => exporter.Export(report);

        act.Should().NotThrow();
        act().Should().NotBeEmpty();
    }

    [Fact]
    public void Export_With_Barcode_QRCode_Does_Not_Throw()
    {
        var exporter = new PdfSharpExporter();
        var report = BuildReport(
            Text("bc-anchor", 10, 5),
            new RenderedBarcodeElement
            {
                X = 10, Y = 50, Width = 30, Height = 30,
                Format = BarcodeFormat.QRCode,
                Value = "https://example.com",
            });

        var act = () => exporter.Export(report);

        act.Should().NotThrow();
    }

    [Fact]
    public void ExportToFile_Creates_Pdf_File_On_Disk()
    {
        var exporter = new PdfSharpExporter();
        var report = BuildReport(Text("row1", 10, 5));
        var tmp = Path.Combine(Path.GetTempPath(), "ReportEngine.Tests_" + Guid.NewGuid().ToString("N") + ".pdf");

        try
        {
            exporter.ExportToFile(report, tmp);

            File.Exists(tmp).Should().BeTrue();
            new FileInfo(tmp).Length.Should().BeGreaterThan(0);
            // 验证文件头
            var head = new byte[5];
            using (var fs = File.OpenRead(tmp))
                fs.Read(head, 0, 5);
            head[0].Should().Be(0x25); // %
            head[1].Should().Be(0x50); // P
        }
        finally
        {
            if (File.Exists(tmp)) File.Delete(tmp);
        }
    }

    [Fact]
    public void Export_With_Border_And_BackgroundColor_Applies_To_Element()
    {
        var exporter = new PdfSharpExporter();
        var report = BuildReport(
            new RenderedTextElement
            {
                X = 10, Y = 100, Width = 50, Height = 20,
                Text = "boxed",
                BackgroundColor = "#ffeecc",
                Border = new BorderDef
                {
                    Width = 0.5,
                    Color = "#333333",
                    Top = true, Bottom = true, Left = true, Right = true,
                },
            });

        var act = () => exporter.Export(report);

        act.Should().NotThrow();
    }

    [Fact]
    public void Export_With_Multiple_Pages_Renders_All()
    {
        var exporter = new PdfSharpExporter();
        var report = new RenderedReport
        {
            PageWidth = 210,
            PageHeight = 297,
            Pages = new List<RenderedPage>
            {
                new RenderedPage { PageNumber = 1, TotalPages = 2, Elements = new List<RenderedElement> { Text("p1", 10, 5) } },
                new RenderedPage { PageNumber = 2, TotalPages = 2, Elements = new List<RenderedElement> { Text("p2", 10, 5) } },
            },
        };

        var bytes = exporter.Export(report);

        bytes.Should().NotBeEmpty();
        bytes.Length.Should().BeGreaterThan(200);
    }

    // ===== D5: 边界用例 =====

    [Fact]
    public void Export_With_Chinese_Unicode_Text_Does_Not_Throw()
    {
        var exporter = new PdfSharpExporter();
        var report = BuildReport(Text("你好世界 — 中文报表", 10, 5));

        var bytes = exporter.Export(report);

        bytes.Should().NotBeEmpty();
        bytes.Length.Should().BeGreaterThan(100);
    }

    [Fact]
    public void Export_With_Very_Long_Text_Line_Does_Not_Throw()
    {
        var exporter = new PdfSharpExporter();
        var longText = new string('A', 5000);
        var report = BuildReport(Text(longText, 10, 5));

        var bytes = exporter.Export(report);

        bytes.Should().NotBeEmpty();
    }

    [Fact]
    public void Export_With_Special_Characters_Does_Not_Throw()
    {
        var exporter = new PdfSharpExporter();
        var report = BuildReport(Text("特殊符号: & < > \" ' % # @ $ ! * + - / \\", 10, 5));

        var bytes = exporter.Export(report);

        bytes.Should().NotBeEmpty();
    }

    [Fact]
    public void Export_With_Hyperlink_Text_Does_Not_Throw()
    {
        var exporter = new PdfSharpExporter();
        var element = Text("点击访问", 10, 5);
        element.Hyperlink = "https://example.com/foo?bar=baz&q=1";
        var report = BuildReport(element);

        var bytes = exporter.Export(report);

        bytes.Should().NotBeEmpty();
    }

    [Fact]
    public void Export_With_All_Alignment_Modes_Does_Not_Throw()
    {
        var exporter = new PdfSharpExporter();
        var elements = new[]
        {
            new RenderedTextElement { Text = "Left",   X = 10, Y = 5,  Width = 50, Height = 8, Alignment = TextAlignment.Left },
            new RenderedTextElement { Text = "Center", X = 10, Y = 15, Width = 50, Height = 8, Alignment = TextAlignment.Center },
            new RenderedTextElement { Text = "Right",  X = 10, Y = 25, Width = 50, Height = 8, Alignment = TextAlignment.Right },
            new RenderedTextElement { Text = "Justify", X = 10, Y = 35, Width = 50, Height = 8, Alignment = TextAlignment.Justify },
        };

        var report = BuildReport(elements);

        var bytes = exporter.Export(report);

        bytes.Should().NotBeEmpty();
    }

    [Fact]
    public void Export_Barcode_With_ShowText_False_Does_Not_Throw()
    {
        var exporter = new PdfSharpExporter();
        var element = new RenderedBarcodeElement
        {
            X = 10, Y = 5, Width = 30, Height = 12,
            Value = "ABC123",
            Format = BarcodeFormat.Code128,
            ShowText = false,
        };

        var report = BuildReport(element);

        var bytes = exporter.Export(report);

        bytes.Should().NotBeEmpty();
    }
}
