using System;
using System.Linq;
using ReportEngine.Core;
using ReportEngine.Core.Parsing;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 元素深拷贝 (DuplicateSelected 用的核心) - 从 MainWindow.DuplicateSelected 抽出。
/// 等价抽离自 MainWindow.DuplicateSelected() (16 行)。
///
/// 行为: 序列化 element → 反序列化 → 返回深拷贝 (失败返回 null)。
/// </summary>
internal static class ElementDuplicator
{
    public static ReportElement? Duplicate(ReportElement element, TemplateParser parser)
    {
        var json = ClipboardHelper.SerializeElement(element, parser);
        if (json == null) return null;
        try
        {
            var parsed = parser.Parse(json);
            return parsed.Bands.FirstOrDefault()?.Elements.FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }
}
