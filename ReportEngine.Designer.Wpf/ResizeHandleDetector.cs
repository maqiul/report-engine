using System.Windows;
using System.Windows.Input;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 调整手柄检测 - 从 OnCanvasMouseDown 抽出。
/// 等价抽离自 MainWindow.OnCanvasMouseDown() resize handle 分支 (15 行)。
///
/// 检查鼠标点是否落在选中元素的 resize handle 上, 返回手柄编号 0-7 (NW/N/NE/E/SE/S/SW/W)。
/// 返回 -1 表示未命中。
/// </summary>
internal static class ResizeHandleDetector
{
    public const int NoHandle = -1;

    public static int Detect(UIElement canvas, Point pos, ReportElement? selectedElement)
    {
        if (selectedElement == null || selectedElement.Locked) return NoHandle;
        var hit = canvas.InputHitTest(pos) as FrameworkElement;
        if (hit?.Tag is string tag && tag.StartsWith("handle_"))
            return int.Parse(tag.Substring(7));
        return NoHandle;
    }
}
