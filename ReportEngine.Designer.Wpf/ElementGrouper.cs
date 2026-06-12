using System;
using System.Collections.Generic;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 元素分组/取消分组算法。
/// 等价抽离自 MainWindow.GroupSelected() (12 行) + UngroupSelected() (11 行)。
/// </summary>
public static class ElementGrouper
{
    /// <summary>
    /// 把 targets 元素标记为同一组 (GroupId = "grp_" + 8 字符 GUID)。
    /// 返回新生成的 GroupId；targets 不足 2 个时返回 null。
    /// </summary>
    public static string? Group(IEnumerable<ReportElement> targets)
    {
        if (targets == null) return null;
        var list = new List<ReportElement>(targets);
        if (list.Count < 2) return null;
        string groupId = "grp_" + Guid.NewGuid().ToString("N").Substring(0, 8);
        foreach (var el in list) el.GroupId = groupId;
        return groupId;
    }

    /// <summary>
    /// 取消 targets 元素的分组 (GroupId = null)。
    /// 返回是否实际取消了至少一个元素的分组 (即至少一个元素原本有 GroupId)。
    /// </summary>
    public static bool Ungroup(IEnumerable<ReportElement> targets)
    {
        if (targets == null) return false;
        bool anyUngrouped = false;
        foreach (var el in targets)
        {
            if (el.GroupId != null)
            {
                el.GroupId = null;
                anyUngrouped = true;
            }
        }
        return anyUngrouped;
    }
}