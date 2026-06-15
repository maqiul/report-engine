using System;
using ReportEngine.Core;
using ReportEngine.Core.Parsing;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 元素剪贴板序列化 - 从 CopySelected 抽出。
/// 等价抽离自 MainWindow.CopySelected() try-catch 块 (8 行)。
///
/// 行为: 将单个 element 包装为 ReportTemplate, 序列化为 JSON 字符串。
/// 异常返回 null。
/// </summary>
internal static class ClipboardHelper
{
    public static string? SerializeElement(ReportElement element, TemplateParser parser)
    {
        try
        {
            var t = new ReportTemplate();
            t.Bands.Add(new Band { Elements = { element } });
            return parser.Serialize(t);
        }
        catch
        {
            return null;
        }
    }
}
