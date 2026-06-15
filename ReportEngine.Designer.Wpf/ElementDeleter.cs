using System;
using System.Collections.Generic;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 元素批量删除 - 从 MainWindow.DeleteSelected 抽出。
/// 等价抽离自 MainWindow.DeleteSelected() (24 行, 2 对称分支)。
///
/// 行为: 遍历所有 bands, 从每个 band.Elements 中移除 targets 中存在的元素。
/// </summary>
internal static class ElementDeleter
{
    public static void DeleteFromBands(IEnumerable<Band> bands, IList<ReportElement> targets)
    {
        foreach (var el in targets)
            foreach (var b in bands)
                b.Elements.Remove(el);
    }
}
