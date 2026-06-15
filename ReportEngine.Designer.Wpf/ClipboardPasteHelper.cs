using System;
using System.Linq;
using ReportEngine.Core;
using ReportEngine.Core.Parsing;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 剪贴板粘贴解析 - 从 MainWindow.PasteElement 抽出。
/// 等价抽离自 MainWindow.PasteElement() try-catch 块 (8 行)。
///
/// 行为: Parse json → 取首个 element → 替换 Id → 加 (2,2) 偏移。
/// 异常或无 element 返回 null。
/// </summary>
internal static class ClipboardPasteHelper
{
    public static ReportElement? ParseAndOffset(string json, TemplateParser parser, double offsetX = 2, double offsetY = 2)
    {
        try
        {
            var t = parser.Parse(json);
            var el = t.Bands.FirstOrDefault()?.Elements.FirstOrDefault();
            if (el == null) return null;
            el.Id = Guid.NewGuid().ToString("N");
            el.X += offsetX;
            el.Y += offsetY;
            return el;
        }
        catch
        {
            return null;
        }
    }
}
