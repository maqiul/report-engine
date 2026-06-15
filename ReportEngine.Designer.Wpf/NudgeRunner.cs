using System.Collections.Generic;
using System.Linq;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 微调执行器 - 从 MainWindow.NudgeSelected 抽出 targets 解析 + Nudge 调用。
/// 等价抽离自 NudgeSelected() targets 解析 (3 行)。
///
/// 行为: 多选用 selectedElements, 单选用 [selectedElement], 否则空 → 列表化。
/// </summary>
internal static class NudgeRunner
{
    public static List<ReportElement> ResolveTargets(
        IList<ReportElement> selectedElements,
        ReportElement? selectedElement)
    {
        if (selectedElements.Count > 0) return selectedElements.ToList();
        if (selectedElement != null) return new List<ReportElement> { selectedElement };
        return new List<ReportElement>();
    }
}
