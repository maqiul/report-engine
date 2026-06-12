using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 中央画布区面板构造 - 把 BuildCenterPanel 从 MainWindow 抽离。
/// 等价抽离自 MainWindow.BuildCenterPanel() (31 行)。
///
/// 左上角 + 水平/垂直标尺 + 画布 ScrollViewer + 预览 ScrollViewer 叠加。
///
/// onScrollChanged: ScrollViewer 滚动时触发 RenderRulers。
/// onPreviewMouseWheel: 滚轮缩放事件。
/// </summary>
internal static class CenterPanelBuilder
{
    public static Grid Build(
        double rulerSize,
        FrameworkElement hRuler,
        FrameworkElement vRuler,
        FrameworkElement scrollViewer,
        FrameworkElement previewScrollViewer,
        Action onScrollChanged,
        MouseWheelEventHandler onPreviewMouseWheel)
    {
        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(rulerSize) });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(rulerSize) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // 左上角
        var corner = new Border { Background = new SolidColorBrush(Color.FromRgb(230, 230, 230)) };
        Grid.SetRow(corner, 0); Grid.SetColumn(corner, 0);
        grid.Children.Add(corner);

        Grid.SetRow(hRuler, 0); Grid.SetColumn(hRuler, 1);
        grid.Children.Add(hRuler);

        Grid.SetRow(vRuler, 1); Grid.SetColumn(vRuler, 0);
        grid.Children.Add(vRuler);

        Grid.SetRow(scrollViewer, 1); Grid.SetColumn(scrollViewer, 1);
        scrollViewer.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler((_, __) => onScrollChanged()));
        scrollViewer.PreviewMouseWheel += onPreviewMouseWheel;
        grid.Children.Add(scrollViewer);

        // 预览视图(叠加在同一位置)
        Grid.SetRow(previewScrollViewer, 0); Grid.SetColumn(previewScrollViewer, 0);
        Grid.SetRowSpan(previewScrollViewer, 2); Grid.SetColumnSpan(previewScrollViewer, 2);
        grid.Children.Add(previewScrollViewer);

        return grid;
    }
}
