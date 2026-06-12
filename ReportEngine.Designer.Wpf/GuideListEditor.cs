using System.Collections.Generic;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 标尺参考线列表操作 - 从 OnRulerGuideMouseUp 抽出。
/// 等价抽离自 MainWindow.OnRulerGuideMouseUp() RemoveAt 条件分支 (4 行, 出现 2 次)。
/// </summary>
internal static class GuideListEditor
{
    /// <summary>若 index 有效且 list[index] &lt; 0, 删除该元素。</summary>
    public static void RemoveIfNegative(List<double> list, int index)
    {
        if (index >= 0 && index < list.Count && list[index] < 0)
            list.RemoveAt(index);
    }
}
