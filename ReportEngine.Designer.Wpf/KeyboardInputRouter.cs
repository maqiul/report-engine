using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 键盘输入路由 - 从 MainWindow.OnPreviewKeyDown 抽出 Tab + Nudge 双分支调度。
/// 等价抽离自 OnPreviewKeyDown() 32 行（含外层 try/return + 内部 2 分支）。
///
/// 行为: 1) Tab/Shift+Tab 切换选中；2) 方向键 nudge。
/// 返回: bool (e.Handled = true 时)。
/// </summary>
internal static class KeyboardInputRouter
{
    public static bool Route(
        KeyEventArgs e,
        ReportTemplate? template,
        Band? selectedBand,
        ReportElement? selectedElement,
        List<ReportElement> selectedElements,
        System.Action<Band, ReportElement> onTabSelected,
        System.Action<double, double> onNudge)
    {
        // Tab 切换
        if (e.Key == Key.Tab && template != null)
        {
            var band = selectedBand ?? template.Bands.FirstOrDefault();
            if (band != null && band.Elements.Count > 0)
            {
                bool reverse = Keyboard.Modifiers == ModifierKeys.Shift;
                var next = TabCycleSelector.SelectNext(band.Elements, selectedElement, reverse);
                if (next != null) onTabSelected(band, next);
            }
            e.Handled = true;
            return true;
        }

        // 方向键 nudge
        if (selectedElements.Count == 0 && selectedElement == null) return false;
        var delta = KeyNudgeCalculator.TryGetDelta(e.Key, Keyboard.Modifiers);
        if (delta == null) return false;
        onNudge(delta.Value.dx, delta.Value.dy);
        e.Handled = true;
        return true;
    }
}
