using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using ReportEngine.Core;
using ReportEngine.Core.Export;
using ReportEngine.Core.Rendering;

namespace ReportEngine.Export.Excel
{

/// <summary>
/// 基于 ClosedXML 的 Excel 导出
/// 策略：
///   1. 每页一个 worksheet
///   2. 把页面所有文本元素的 X/Y 坐标聚类为列/行锚点（带容差）
///   3. 元素按所属锚点写入单元格，并应用字体/对齐/颜色
///   4. 根据元素宽度近似计算列宽
/// </summary>
public class ClosedXmlExporter : IExcelExporter
{
    /// <summary>X/Y 聚类容差（mm），同一锚点视为同行/同列</summary>
    public double ClusterTolerance { get; set; } = 0.8;

    /// <summary>1mm 约等于的 Excel 列宽字符数</summary>
    private const double MmPerExcelColWidth = 1.86;

    /// <summary>1mm 等于的磅数（pt）</summary>
    private const double MmToPoint = 2.83465;

    public byte[] Export(RenderedReport renderedReport)
    {
        using var wb = BuildWorkbook(renderedReport);
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public void ExportToFile(RenderedReport renderedReport, string outputPath)
    {
        var dir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        using var wb = BuildWorkbook(renderedReport);
        wb.SaveAs(outputPath);
    }

    // ---------------------------------------------------------------

    private XLWorkbook BuildWorkbook(RenderedReport report)
    {
        var wb = new XLWorkbook();

        for (int pageIdx = 0; pageIdx < report.Pages.Count; pageIdx++)
        {
            var page = report.Pages[pageIdx];
            var sheetName = report.Pages.Count == 1 ? "Report" : $"Page {pageIdx + 1}";
            var ws = wb.Worksheets.Add(sheetName);

            FillSheet(ws, page);
        }

        return wb;
    }

    private void FillSheet(IXLWorksheet ws, RenderedPage page)
    {
        var textElements = page.Elements.OfType<RenderedTextElement>().ToList();
        if (textElements.Count == 0) return;

        // 列锚点：取所有元素 X 起点，聚类
        var columnAnchors = ClusterPositions(textElements.Select(e => e.X));
        // 行锚点：取所有元素 Y 起点，聚类
        var rowAnchors = ClusterPositions(textElements.Select(e => e.Y));

        // 设置列宽（基于该列内最大元素宽度）
        for (int c = 0; c < columnAnchors.Count; c++)
        {
            var x = columnAnchors[c];
            var widthsAtCol = textElements
                .Where(e => Math.Abs(e.X - x) <= ClusterTolerance)
                .Select(e => e.Width);
            if (!widthsAtCol.Any()) continue;
            var w = widthsAtCol.Max();
            ws.Column(c + 1).Width = Math.Max(8, w / MmPerExcelColWidth);
        }

        // 设置行高（基于该行内最大元素高度）
        for (int r = 0; r < rowAnchors.Count; r++)
        {
            var y = rowAnchors[r];
            var heightsAtRow = textElements
                .Where(e => Math.Abs(e.Y - y) <= ClusterTolerance)
                .Select(e => e.Height);
            if (!heightsAtRow.Any()) continue;
            var h = heightsAtRow.Max();
            ws.Row(r + 1).Height = Math.Max(14, h * MmToPoint);
        }

        // 写入文本元素
        foreach (var t in textElements)
        {
            int col = FindAnchorIndex(columnAnchors, t.X) + 1;
            int row = FindAnchorIndex(rowAnchors, t.Y) + 1;
            if (col <= 0 || row <= 0) continue;

            var cell = ws.Cell(row, col);
            cell.Value = t.Text ?? string.Empty;

            ApplyTextStyle(cell, t);
        }

        // 冻结首行（如果第一行像表头）
        if (rowAnchors.Count > 1)
        {
            // 简单启发式：若第一行有 bold 元素则视为表头
            var firstRowY = rowAnchors[0];
            var firstRowBold = textElements.Any(e =>
                Math.Abs(e.Y - firstRowY) <= ClusterTolerance && e.Font.Bold);
            // 不强制冻结，避免子报表混入造成误判
            _ = firstRowBold;
        }
    }

    private static void ApplyTextStyle(IXLCell cell, RenderedTextElement t)
    {
        var font = cell.Style.Font;
        if (!string.IsNullOrEmpty(t.Font.Family))
            font.FontName = t.Font.Family;
        font.FontSize = Math.Max(t.Font.Size, 6);
        font.Bold = t.Font.Bold;
        font.Italic = t.Font.Italic;
        font.Underline = t.Font.Underline ? XLFontUnderlineValues.Single : XLFontUnderlineValues.None;
        if (TryParseHexColor(t.Font.Color, out var fontColor))
            font.FontColor = fontColor;

        // 背景色
        if (TryParseHexColor(t.BackgroundColor, out var bgColor))
        {
            cell.Style.Fill.BackgroundColor = bgColor;
            cell.Style.Fill.PatternType = XLFillPatternValues.Solid;
        }

        // 对齐
        cell.Style.Alignment.Horizontal = t.Alignment switch
        {
            TextAlignment.Center => XLAlignmentHorizontalValues.Center,
            TextAlignment.Right  => XLAlignmentHorizontalValues.Right,
            _                    => XLAlignmentHorizontalValues.Left,
        };
        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        cell.Style.Alignment.WrapText = true;

        // 边框
        if (t.Border != null)
        {
            var border = t.Border;
            var color = TryParseHexColor(border.Color, out var bColor) ? bColor : XLColor.Black;
            var style = border.Style switch
            {
                BorderStyle.Dashed => XLBorderStyleValues.Dashed,
                BorderStyle.Dotted => XLBorderStyleValues.Dotted,
                BorderStyle.None   => XLBorderStyleValues.None,
                _                  => XLBorderStyleValues.Thin,
            };
            if (border.Top)    { cell.Style.Border.TopBorder    = style; cell.Style.Border.TopBorderColor    = color; }
            if (border.Bottom) { cell.Style.Border.BottomBorder = style; cell.Style.Border.BottomBorderColor = color; }
            if (border.Left)   { cell.Style.Border.LeftBorder   = style; cell.Style.Border.LeftBorderColor   = color; }
            if (border.Right)  { cell.Style.Border.RightBorder  = style; cell.Style.Border.RightBorderColor  = color; }
        }

        // 超链接
        if (!string.IsNullOrEmpty(t.Hyperlink))
        {
            try { cell.Hyperlink = new XLHyperlink(t.Hyperlink); }
            catch { /* 忽略非法链接 */ }
        }
    }

    /// <summary>
    /// 将 X 或 Y 坐标按容差聚类，返回升序锚点列表
    /// </summary>
    private List<double> ClusterPositions(IEnumerable<double> positions)
    {
        var sorted = positions.Distinct().OrderBy(v => v).ToList();
        var anchors = new List<double>();
        foreach (var p in sorted)
        {
            if (anchors.Count == 0 || Math.Abs(p - anchors[anchors.Count - 1]) > ClusterTolerance)
                anchors.Add(p);
        }
        return anchors;
    }

    private int FindAnchorIndex(List<double> anchors, double value)
    {
        for (int i = 0; i < anchors.Count; i++)
        {
            if (Math.Abs(anchors[i] - value) <= ClusterTolerance)
                return i;
            if (anchors[i] > value) return Math.Max(0, i - 1);
        }
        return anchors.Count - 1;
    }

    private static bool TryParseHexColor(string? hex, out XLColor color)
    {
        color = XLColor.Black;
        if (string.IsNullOrWhiteSpace(hex)) return false;
        try
        {
            color = XLColor.FromHtml(hex);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
}
