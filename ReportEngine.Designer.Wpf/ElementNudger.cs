using System;
using System.Collections.Generic;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 元素键盘方向键微移算法 - Shift=5mm, 否则 0.5mm。
/// 等价抽离自 MainWindow.NudgeSelected() (13 行)。
///
/// 跳过 Locked 元素，X/Y 最小为 0 (不越出画布)。
/// </summary>
public static class ElementNudger
{
    /// <summary>
    /// 对 targets 元素按 (dx, dy) 微移。
    /// 跳过 Locked 元素，并对 X/Y 应用 Math.Max(0, ...) 边界。
    /// </summary>
    public static void Nudge(IEnumerable<ReportElement> targets, double dx, double dy)
    {
        if (targets == null) return;
        foreach (var el in targets)
        {
            if (el.Locked) continue;
            el.X = Math.Max(0, el.X + dx);
            el.Y = Math.Max(0, el.Y + dy);
        }
    }
}