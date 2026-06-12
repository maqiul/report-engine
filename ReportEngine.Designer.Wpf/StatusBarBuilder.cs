using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static ReportEngine.Designer.Wpf.UiFactory;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 状态栏构造 - 把 BuildStatusBar 从 MainWindow 抽离。
/// 等价抽离自 MainWindow.BuildStatusBar() (42 行)。
///
/// 左侧: 视图切换标签 (设计/预览) + 页面设置按钮。
/// 右侧: 状态文本 + 鼠标位置 + 缩放滑块/标签 (MainWindow ctor 已 init, 这里只挂到 panel)。
///
/// 3 个 callback: onSwitchDesign/onSwitchPreview 触发 SwitchView(..., tabDesign, tabPreview)
/// + onShowPageSetup 触发 ShowPageSetupDialog。
/// </summary>
internal static class StatusBarBuilder
{
    public static Border Build(
        TextBlock statusText,
        TextBlock posLabel,
        Slider zoomSlider,
        TextBlock zoomLabel,
        Action<Border, Border> onSwitchDesign,
        Action<Border, Border> onSwitchPreview,
        Action onShowPageSetup,
        out Border tabDesign,
        out Border tabPreview)
    {
        var outer = new DockPanel { Height = 26 };

        // 左侧:视图标签
        var tabPanel = new StackPanel { Orientation = Orientation.Horizontal, Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)) };
        tabDesign = MakeStatusTab("普通视图", true);
        tabPreview = MakeStatusTab("页面视图", false);
        var tabPageSetup = MakeStatusTab("页面设置", false, onShowPageSetup);
        var designLocal = tabDesign;
        var previewLocal = tabPreview;
        tabDesign.MouseLeftButtonDown += (_, __) => onSwitchDesign(designLocal, previewLocal);
        tabPreview.MouseLeftButtonDown += (_, __) => onSwitchPreview(designLocal, previewLocal);
        tabPanel.Children.Add(tabDesign);
        tabPanel.Children.Add(tabPreview);
        tabPanel.Children.Add(tabPageSetup);
        DockPanel.SetDock(tabPanel, Dock.Left);
        outer.Children.Add(tabPanel);

        // 右侧主栏
        var grid = new Grid { Background = new SolidColorBrush(Color.FromRgb(0, 122, 204)) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });

        statusText.Margin = new Thickness(8, 0, 0, 0);
        Grid.SetColumn(statusText, 0);
        grid.Children.Add(statusText);

        posLabel.Margin = new Thickness(4, 0, 0, 0);
        Grid.SetColumn(posLabel, 1);
        grid.Children.Add(posLabel);

        var zoomPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(4, 0, 8, 0), VerticalAlignment = VerticalAlignment.Center };
        zoomPanel.Children.Add(zoomSlider);
        zoomPanel.Children.Add(new TextBlock { Text = " ", VerticalAlignment = VerticalAlignment.Center });
        zoomPanel.Children.Add(zoomLabel);
        Grid.SetColumn(zoomPanel, 2);
        grid.Children.Add(zoomPanel);

        outer.Children.Add(grid);
        return new Border { Child = outer };
    }
}
