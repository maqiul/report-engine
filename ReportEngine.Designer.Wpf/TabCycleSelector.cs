using System.Collections.Generic;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// Tab/Shift+Tab 键在 Band 内元素间循环选中算法。
/// 等价抽离自 MainWindow.OnPreviewKeyDown() 中 Tab 键处理块 (15 行)。
///
/// reverse = true 时反向 (Shift+Tab)，false 时正向 (Tab)。
/// 当没有当前选中元素时从 0/末位开始。
/// </summary>
public static class TabCycleSelector
{
    /// <summary>
    /// 在 band.Elements 中按 (reverse) 方向循环选中下一个元素。
    /// 返回选中的元素，未选中任何元素 (空列表) 时返回 null。
    /// </summary>
    public static ReportElement? SelectNext(IList<ReportElement> elements, ReportElement? current, bool reverse)
    {
        if (elements == null || elements.Count == 0) return null;
        int idx = current != null ? elements.IndexOf(current) : -1;
        if (reverse)
            idx = idx <= 0 ? elements.Count - 1 : idx - 1;
        else
            idx = idx < 0 || idx >= elements.Count - 1 ? 0 : idx + 1;
        return elements[idx];
    }
}