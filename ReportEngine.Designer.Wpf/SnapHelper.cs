using System;
using System.Collections.Generic;
using System.Linq;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 元素吸附算法 - 把元素 X/Y 吸到最近的参考点 (其他元素边/中线 / 参考线)。
/// 等价抽离自 MainWindow.SnapPosition()。
///
/// 输出 snapLinesX / snapLinesY 是本次吸附命中的坐标集合, 用于 UI 画吸附辅助线。
/// </summary>
public static class SnapHelper
{
    /// <summary>
    /// 在指定容差内把 (x, y) 吸附到最近的参考点。
    /// </summary>
    /// <param name="x">入参 x, 出参为吸附后的 x</param>
    /// <param name="y">入参 y, 出参为吸附后的 y</param>
    /// <param name="w">元素宽 (用于算中线吸附点)</param>
    /// <param name="h">元素高 (用于算中线吸附点)</param>
    /// <param name="band">当前 band (候选吸附源)</param>
    /// <param name="excludedElements">要排除的元素 (如当前正在拖的元素本身 + 多选集)</param>
    /// <param name="vGuides">垂直参考线列表</param>
    /// <param name="hGuides">水平参考线列表</param>
    /// <param name="snapThresholdMm">吸附容差 (mm)</param>
    /// <param name="snapLinesX">输出: X 方向吸附命中的参考线</param>
    /// <param name="snapLinesY">输出: Y 方向吸附命中的参考线</param>
    public static void Snap(
        ref double x,
        ref double y,
        double w,
        double h,
        Band band,
        IEnumerable<ReportElement> excludedElements,
        IEnumerable<double> vGuides,
        IEnumerable<double> hGuides,
        double snapThresholdMm,
        List<double> snapLinesX,
        List<double> snapLinesY)
    {
        var excluded = new HashSet<ReportElement>(excludedElements ?? Enumerable.Empty<ReportElement>());

        // 收集吸附线: 其他元素的边缘和中线
        var xSnaps = new List<double>();
        var ySnaps = new List<double>();

        foreach (var other in band.Elements)
        {
            if (excluded.Contains(other)) continue;
            xSnaps.Add(other.X);                    // 左边
            xSnaps.Add(other.X + other.Width / 2);  // 中线
            xSnaps.Add(other.X + other.Width);      // 右边
            ySnaps.Add(other.Y);
            ySnaps.Add(other.Y + other.Height / 2);
            ySnaps.Add(other.Y + other.Height);
        }

        if (vGuides != null) foreach (var g in vGuides) xSnaps.Add(g);
        if (hGuides != null) foreach (var g in hGuides) ySnaps.Add(g);

        // 当前元素的3个吸附点
        double[] myXs = { x, x + w / 2, x + w };
        double[] myYs = { y, y + h / 2, y + h };

        double bestDx = double.MaxValue;
        double snapX = x;
        foreach (var sx in xSnaps)
        {
            foreach (var mx in myXs)
            {
                double d = Math.Abs(mx - sx);
                if (d < snapThresholdMm && d < Math.Abs(bestDx))
                {
                    bestDx = mx - sx;
                    snapX = sx;
                }
            }
        }
        if (bestDx != double.MaxValue)
        {
            x -= bestDx;
            snapLinesX.Add(snapX);
        }

        double bestDy = double.MaxValue;
        double snapY = y;
        foreach (var sy in ySnaps)
        {
            foreach (var my in myYs)
            {
                double d = Math.Abs(my - sy);
                if (d < snapThresholdMm && d < Math.Abs(bestDy))
                {
                    bestDy = my - sy;
                    snapY = sy;
                }
            }
        }
        if (bestDy != double.MaxValue)
        {
            y -= bestDy;
            snapLinesY.Add(snapY);
        }
    }
}