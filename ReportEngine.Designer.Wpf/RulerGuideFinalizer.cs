using System;
using System.Windows;
using System.Windows.Input;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 标尺参考线拖动收尾 - 从 OnRulerGuideMouseUp 抽出。
/// 等价抽离自 MainWindow.OnRulerGuideMouseUp() 对称 release/unsubscribe 分支 (12 行)。
///
/// 行为:
///   - draggingHGuide=true:  对 vRuler Release + 取消订阅 onMove/onUp
///   - draggingHGuide=false: 对 hRuler Release + 取消订阅 onMove/onUp
/// </summary>
internal static class RulerGuideFinalizer
{
    public static void ReleaseAndUnsubscribe(
        bool draggingHGuide,
        UIElement hRuler, UIElement vRuler,
        MouseEventHandler onMove,
        MouseButtonEventHandler onUp)
    {
        if (draggingHGuide)
        {
            vRuler.ReleaseMouseCapture();
            vRuler.MouseMove -= onMove;
            vRuler.MouseLeftButtonUp -= onUp;
        }
        else
        {
            hRuler.ReleaseMouseCapture();
            hRuler.MouseMove -= onMove;
            hRuler.MouseLeftButtonUp -= onUp;
        }
    }
}
