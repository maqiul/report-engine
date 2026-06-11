using System;
using System.Linq;
using ReportEngine.Core;
using ReportEngine.Core.Parsing;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 元素深拷贝算法 - 通过 JSON 序列化/反序列化产生一个全新 Id 的副本 (X/Y 各偏移 3mm)。
/// 等价抽离自 MainWindow.DuplicateSelected() (27 行)。
/// </summary>
public static class ElementDuplicator
{
    private const double OffsetX = 3;
    private const double OffsetY = 3;

    /// <summary>
    /// 深拷贝 source 元素，重置 Id 并按 (OffsetX, OffsetY) 偏移位置。
    /// parser 用于序列化/反序列化 (深拷贝机制)。
    /// 返回新元素；source 为 null 或解析失败时返回 null。
    /// </summary>
    public static ReportElement? Duplicate(ReportElement source, TemplateParser parser)
    {
        if (source == null || parser == null) return null;
        try
        {
            // 借用 ReportTemplate 作为序列化容器
            var wrapper = new ReportTemplate();
            wrapper.Bands.Add(new Band { Elements = { source } });
            var json = parser.Serialize(wrapper);
            var restored = parser.Parse(json);
            var copy = restored.Bands.FirstOrDefault()?.Elements.FirstOrDefault();
            if (copy == null) return null;
            copy.Id = Guid.NewGuid().ToString("N");
            copy.X += OffsetX;
            copy.Y += OffsetY;
            return copy;
        }
        catch
        {
            return null;
        }
    }
}