using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 视图模式切换 (设计/预览) - 把 SwitchView 从 MainWindow 抽离。
/// 等价抽离自 MainWindow.SwitchView() (28 行)。
///
/// 行为:
///   - mode == "preview": 隐藏设计视图(画布/标尺), 显示预览, 重渲染预览
///   - mode == "design" (else): 隐藏预览, 显示设计视图
///   - 两个 tab 标签背景色互换 (通过传入的 tabDesign/tabPreview 引用)
/// </summary>
internal static class ViewSwitcher
{
    public static void Switch(
        string mode,
        Border tabDesign,
        Border tabPreview,
        UIElement scrollViewer,
        UIElement hRuler,
        UIElement vRuler,
        UIElement previewScrollViewer,
        Action onPreviewRender)
    {
        if (mode == "preview")
        {
            scrollViewer.Visibility = Visibility.Collapsed;
            hRuler.Visibility = Visibility.Collapsed;
            vRuler.Visibility = Visibility.Collapsed;
            previewScrollViewer.Visibility = Visibility.Visible;
            tabDesign.Background = new SolidColorBrush(Color.FromRgb(220, 220, 220));
            tabPreview.Background = Brushes.White;
            onPreviewRender();
        }
        else
        {
            previewScrollViewer.Visibility = Visibility.Collapsed;
            scrollViewer.Visibility = Visibility.Visible;
            hRuler.Visibility = Visibility.Visible;
            vRuler.Visibility = Visibility.Visible;
            tabDesign.Background = Brushes.White;
            tabPreview.Background = new SolidColorBrush(Color.FromRgb(220, 220, 220));
        }
    }
}
