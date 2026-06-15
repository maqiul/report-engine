using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 框选结果提交 - 从 MainWindow.OnCanvasMouseUp 抽出。
/// 等价抽离自 MainWindow.OnCanvasMouseUp() Marquee 分支中段 (8 行)。
///
/// 行为: 清空 _selectedElements → 填充 hits → 设 _selectedBand → 设 _selectedElement=首个。
/// 副作用: 同时移除并清空 _marqueeRect。
/// </summary>
internal static class MarqueeCommitHelper
{
    public static void ApplyToSelection(
        IList<(ReportElement el, Band band)> hits,
        List<ReportElement> selectedElements,
        ref ReportElement? selectedElement,
        ref Band? selectedBand,
        ref Rectangle? marqueeRect,
        Canvas canvas)
    {
        selectedElements.Clear();
        Band? firstBand = null;
        foreach (var (el, band) in hits)
        {
            selectedElements.Add(el);
            if (firstBand == null) firstBand = band;
        }
        if (firstBand != null)
        {
            selectedBand = firstBand;
            selectedElement = selectedElements[0];
        }
        if (marqueeRect != null) { canvas.Children.Remove(marqueeRect); marqueeRect = null; }
    }
}
