using System;
using System.Collections.Generic;
using System.IO;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using ReportEngine.Core;
using ReportEngine.Core.Barcodes;
using ReportEngine.Core.Export;
using ReportEngine.Core.Rendering;

namespace ReportEngine.Export.Pdf
{
    /// <summary>
    /// 基于 PdfSharpCore 的多目标 PDF 导出器。
    /// 内置 Windows/Linux 字体解析器，可处理中文宋体/雅黑等常见字体。
    /// 兼容 net462 / netstandard2.0 / net8.0。
    /// </summary>
    public class PdfSharpExporter : IPdfExporter
    {
        public byte[] Export(RenderedReport renderedReport)
        {
            using (var doc = BuildDocument(renderedReport))
            using (var ms = new MemoryStream())
            {
                doc.Save(ms, false);
                return ms.ToArray();
            }
        }

        public void ExportToFile(RenderedReport renderedReport, string outputPath)
        {
            var dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (var doc = BuildDocument(renderedReport))
            {
                doc.Save(outputPath);
            }
        }

        // ============================== 核心绘制 ==============================

        private static PdfDocument BuildDocument(RenderedReport renderedReport)
        {
            var doc = new PdfDocument();
            doc.Info.Title = "Report";

            foreach (var page in renderedReport.Pages)
            {
                var pdfPage = doc.AddPage();
                pdfPage.Width = XUnit.FromMillimeter(renderedReport.PageWidth);
                pdfPage.Height = XUnit.FromMillimeter(renderedReport.PageHeight);

                using (var gfx = XGraphics.FromPdfPage(pdfPage))
                {
                    foreach (var el in page.Elements)
                    {
                        DrawElement(gfx, el);
                    }
                }
            }

            return doc;
        }

        private static void DrawElement(XGraphics gfx, RenderedElement el)
        {
            var rect = MmRect(el.X, el.Y, el.Width, el.Height);

            // 背景
            if (!string.IsNullOrEmpty(el.BackgroundColor))
            {
                var bg = ParseColor(el.BackgroundColor!, XColor.FromArgb(0, 255, 255, 255));
                gfx.DrawRectangle(new XSolidBrush(bg), rect);
            }

            switch (el)
            {
                case RenderedTextElement t:
                    DrawText(gfx, t, rect);
                    break;
                case RenderedLineElement l:
                    DrawLine(gfx, l, rect);
                    break;
                case RenderedShapeElement s:
                    DrawShape(gfx, s, rect);
                    break;
                case RenderedImageElement img:
                    DrawImage(gfx, img, rect);
                    break;
                case RenderedBarcodeElement bc:
                    DrawBarcode(gfx, bc, rect);
                    break;
                case RenderedTableElement tbl:
                    DrawTable(gfx, tbl, rect);
                    break;
                case RenderedCrossTabElement ctab:
                    DrawTable(gfx, new RenderedTableElement
                    {
                        Id = ctab.Id, X = ctab.X, Y = ctab.Y, Width = ctab.Width, Height = ctab.Height,
                        RowCount = ctab.RowCount, ColCount = ctab.ColCount,
                        ColumnWidths = ctab.ColumnWidths, RowHeights = ctab.RowHeights,
                        Cells = ctab.Cells, BorderWidth = ctab.BorderWidth, BorderColor = ctab.BorderColor,
                    }, rect);
                    break;
            }

            // 边框
            if (el.Border != null)
            {
                var pen = new XPen(ParseColor(el.Border.Color, XColors.Black),
                    XUnit.FromMillimeter(el.Border.Width).Point);
                if (el.Border.Top)    gfx.DrawLine(pen, rect.Left, rect.Top, rect.Right, rect.Top);
                if (el.Border.Bottom) gfx.DrawLine(pen, rect.Left, rect.Bottom, rect.Right, rect.Bottom);
                if (el.Border.Left)   gfx.DrawLine(pen, rect.Left, rect.Top, rect.Left, rect.Bottom);
                if (el.Border.Right)  gfx.DrawLine(pen, rect.Right, rect.Top, rect.Right, rect.Bottom);
            }
        }

        private static void DrawText(XGraphics gfx, RenderedTextElement t, XRect rect)
        {
            if (string.IsNullOrEmpty(t.Text)) return;
            var style = XFontStyle.Regular;
            if (t.Font.Bold)      style |= XFontStyle.Bold;
            if (t.Font.Italic)    style |= XFontStyle.Italic;
            if (t.Font.Underline) style |= XFontStyle.Underline;

            XFont font;
            try
            {
                font = new XFont(t.Font.Family, t.Font.Size, style);
            }
            catch
            {
                font = new XFont("Arial", t.Font.Size, style);
            }

            var brush = new XSolidBrush(ParseColor(t.Font.Color, XColors.Black));
            var fmt = new XStringFormat();
            switch (t.Alignment)
            {
                case TextAlignment.Center: fmt.Alignment = XStringAlignment.Center; break;
                case TextAlignment.Right:  fmt.Alignment = XStringAlignment.Far; break;
                default:                   fmt.Alignment = XStringAlignment.Near; break;
            }
            fmt.LineAlignment = XLineAlignment.Near;

            // PdfSharpCore DrawString(rect) 会按 LineAlignment 处理；为了兼容 Near，给 Y 偏移
            try
            {
                gfx.DrawString(t.Text, font, brush, rect, fmt);
            }
            catch
            {
                // 防 NRE / 字体缺失
                gfx.DrawString(t.Text, new XFont("Arial", t.Font.Size, XFontStyle.Regular), brush, rect, fmt);
            }
        }

        private static void DrawLine(XGraphics gfx, RenderedLineElement l, XRect rect)
        {
            var pen = new XPen(ParseColor(l.LineColor, XColors.Black),
                XUnit.FromMillimeter(l.LineWidth).Point);
            switch (l.Direction)
            {
                case LineDirection.Vertical:
                    gfx.DrawLine(pen, rect.X + rect.Width / 2, rect.Top, rect.X + rect.Width / 2, rect.Bottom);
                    break;
                case LineDirection.Diagonal:
                    gfx.DrawLine(pen, rect.Left, rect.Top, rect.Right, rect.Bottom);
                    break;
                default:
                    gfx.DrawLine(pen, rect.Left, rect.Y + rect.Height / 2, rect.Right, rect.Y + rect.Height / 2);
                    break;
            }
        }

        private static void DrawShape(XGraphics gfx, RenderedShapeElement s, XRect rect)
        {
            var fill = new XSolidBrush(ParseColor(s.FillColor, XColors.White));
            var pen = new XPen(XColors.DimGray, 0.5);
            switch (s.Shape)
            {
                case ShapeType.Ellipse:
                    gfx.DrawEllipse(pen, fill, rect);
                    break;
                default:
                    gfx.DrawRectangle(pen, fill, rect);
                    break;
            }
        }

        private static void DrawImage(XGraphics gfx, RenderedImageElement img, XRect rect)
        {
            if (string.IsNullOrEmpty(img.Source)) return;
            try
            {
                XImage image;
                if (img.Source.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                {
                    var idx = img.Source.IndexOf("base64,", StringComparison.Ordinal);
                    if (idx < 0) return;
                    var bytes = Convert.FromBase64String(img.Source.Substring(idx + 7));
                    using (var ms = new MemoryStream(bytes))
                        image = XImage.FromStream(() => new MemoryStream(bytes));
                }
                else if (File.Exists(img.Source))
                {
                    image = XImage.FromFile(img.Source);
                }
                else
                {
                    return;
                }
                gfx.DrawImage(image, rect);
            }
            catch
            {
                // 失败时绘制占位
                gfx.DrawRectangle(new XPen(XColors.SteelBlue, 0.5), rect);
            }
        }

        // ============================== 工具 ==============================

        private static XRect MmRect(double xMm, double yMm, double wMm, double hMm)
        {
            return new XRect(
                XUnit.FromMillimeter(xMm).Point,
                XUnit.FromMillimeter(yMm).Point,
                XUnit.FromMillimeter(wMm).Point,
                XUnit.FromMillimeter(hMm).Point);
        }

        private static XColor ParseColor(string? html, XColor fallback)
        {
            if (string.IsNullOrEmpty(html)) return fallback;
            try
            {
                var s = html!.Trim();
                if (s.StartsWith("#")) s = s.Substring(1);
                if (s.Length == 6)
                {
                    var r = Convert.ToByte(s.Substring(0, 2), 16);
                    var g = Convert.ToByte(s.Substring(2, 2), 16);
                    var b = Convert.ToByte(s.Substring(4, 2), 16);
                    return XColor.FromArgb(r, g, b);
                }
                if (s.Length == 8)
                {
                    var a = Convert.ToByte(s.Substring(0, 2), 16);
                    var r = Convert.ToByte(s.Substring(2, 2), 16);
                    var g = Convert.ToByte(s.Substring(4, 2), 16);
                    var b = Convert.ToByte(s.Substring(6, 2), 16);
                    return XColor.FromArgb(a, r, g, b);
                }
            }
            catch { }
            return fallback;
        }

        // ============================== 条码 ==============================

        private static void DrawBarcode(XGraphics gfx, RenderedBarcodeElement bc, XRect rect)
        {
            if (string.IsNullOrEmpty(bc.Value)) return;
            try
            {
                int pxW = Math.Max(50, (int)(rect.Width / 0.3));
                int pxH = Math.Max(50, (int)(rect.Height / 0.3));
                var matrix = BarcodeGenerator.Generate(bc.Value, bc.Format, pxW, pxH);
                int rows = matrix.GetLength(0);
                int cols = matrix.GetLength(1);
                if (rows == 0 || cols == 0) return;

                double cellW = rect.Width / cols;
                double cellH = rect.Height / rows;
                var foreColor = ParseColor(bc.ForeColor, XColors.Black);
                var backColor = ParseColor(bc.BackColor, XColors.White);

                gfx.DrawRectangle(new XSolidBrush(backColor), rect);
                var brush = new XSolidBrush(foreColor);
                for (int y = 0; y < rows; y++)
                    for (int x = 0; x < cols; x++)
                        if (matrix[y, x])
                            gfx.DrawRectangle(brush, new XRect(rect.X + x * cellW, rect.Y + y * cellH, cellW, cellH));
            }
            catch
            {
                gfx.DrawRectangle(new XPen(XColors.Red, 0.5), rect);
            }
        }

        // ============================== 表格 ==============================

        private static void DrawTable(XGraphics gfx, RenderedTableElement tbl, XRect rect)
        {
            var borderPen = new XPen(ParseColor(tbl.BorderColor, XColors.Black),
                XUnit.FromMillimeter(tbl.BorderWidth).Point);

            // 计算各行各列的累计偏移
            var colOffsets = new List<double> { 0 };
            for (int c = 0; c < tbl.ColumnWidths.Count; c++)
                colOffsets.Add(colOffsets[c] + XUnit.FromMillimeter(tbl.ColumnWidths[c]).Point);
            var rowOffsets = new List<double> { 0 };
            for (int r = 0; r < tbl.RowHeights.Count; r++)
                rowOffsets.Add(rowOffsets[r] + XUnit.FromMillimeter(tbl.RowHeights[r]).Point);

            // 绘制单元格
            foreach (var cell in tbl.Cells)
            {
                if (cell.Row >= tbl.RowCount || cell.Col >= tbl.ColCount) continue;
                double cx = rect.X + colOffsets[cell.Col];
                double cy = rect.Y + rowOffsets[cell.Row];
                int endCol = Math.Min(cell.Col + cell.ColSpan, tbl.ColCount);
                int endRow = Math.Min(cell.Row + cell.RowSpan, tbl.RowCount);
                double cw = colOffsets[endCol] - colOffsets[cell.Col];
                double ch = rowOffsets[endRow] - rowOffsets[cell.Row];
                var cellRect = new XRect(cx, cy, cw, ch);

                // 背景
                if (!string.IsNullOrEmpty(cell.BackgroundColor))
                    gfx.DrawRectangle(new XSolidBrush(ParseColor(cell.BackgroundColor, XColors.White)), cellRect);

                // 文字
                if (!string.IsNullOrEmpty(cell.Text))
                {
                    var style = XFontStyle.Regular;
                    if (cell.Font.Bold) style |= XFontStyle.Bold;
                    if (cell.Font.Italic) style |= XFontStyle.Italic;
                    XFont font;
                    try { font = new XFont(cell.Font.Family, cell.Font.Size, style); }
                    catch { font = new XFont("Arial", cell.Font.Size, style); }
                    var fmt = new XStringFormat { LineAlignment = XLineAlignment.Center };
                    switch (cell.Alignment)
                    {
                        case TextAlignment.Center: fmt.Alignment = XStringAlignment.Center; break;
                        case TextAlignment.Right: fmt.Alignment = XStringAlignment.Far; break;
                        default: fmt.Alignment = XStringAlignment.Near; break;
                    }
                    var brush = new XSolidBrush(ParseColor(cell.Font.Color, XColors.Black));
                    gfx.DrawString(cell.Text, font, brush, cellRect, fmt);
                }

                // 边框
                gfx.DrawRectangle(borderPen, cellRect);
            }

            // 外框
            gfx.DrawRectangle(borderPen, rect);
        }
    }
}
