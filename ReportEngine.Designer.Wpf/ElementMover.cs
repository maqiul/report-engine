using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 元素移动 (多选 + 单选 + Snap) - 从 OnCanvasMouseMove 抽出。
/// 等价抽离自 MainWindow.OnCanvasMouseMove() MoveElement 分支 (22 行)。
///
/// 行为:
///   - MoveMultiple: 多选批量移动 + 0.5mm 网格对齐
///   - MoveSingle: 单选移动 + 0.5mm 网格对齐 + 可选 SnapHelper
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

    /// <summary>
    /// 单选移动 + Snap + 0.5mm 网格对齐。等价抽离自 MoveElement 单选分支 (13 行)。
    /// </summary>
    public static void MoveSingle(
        ReportElement element,
        Band? band,
        double startX, double startY,
        double dx, double dy,
        bool snapEnabled,
        IEnumerable<ReportElement> excludedElements,
        List<double> vGuides, List<double> hGuides,
        double snapThresholdMm,
        List<double> snapLinesX, List<double> snapLinesY)
    {
        double newX = System.Math.Max(0, startX + dx);
        double newY = System.Math.Max(0, startY + dy);
        if (snapEnabled && band != null)
        {
            SnapHelper.Snap(ref newX, ref newY, element.Width, element.Height, band,
                excludedElements: new[] { element }.Concat(excludedElements).Distinct(),
                vGuides: vGuides, hGuides: hGuides, snapThresholdMm: snapThresholdMm,
                snapLinesX: snapLinesX, snapLinesY: snapLinesY);
        }
        element.X = System.Math.Round(newX * 2) / 2;
        element.Y = System.Math.Round(newY * 2) / 2;
    }
}
