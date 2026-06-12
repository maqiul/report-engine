using System.Collections.Generic;
using System.Windows;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 多选元素批量移动 - 从 OnCanvasMouseMove 抽出。
/// 等价抽离自 MainWindow.OnCanvasMouseMove() 多选移动分支 (9 行)。
///
/// 行为: dx/dy 是本次相对上次的位移, 每个元素 X/Y 累加并 snap 到 0.5mm 网格, 最小 0。
/// </summary>
internal static class ElementMover
{
    public static void MoveMultiple(IEnumerable<ReportElement> elements, double dx, double dy)
    {
        foreach (var el in elements)
        {
            el.X = System.Math.Max(0, System.Math.Round((el.X + dx) * 2) / 2);
            el.Y = System.Math.Max(0, System.Math.Round((el.Y + dy) * 2) / 2);
        }
    }
}
