using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static ReportEngine.Designer.Wpf.UiFactory;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 右侧属性面板构造 - 把 BuildRightPanel 从 MainWindow 抽离。
/// 等价抽离自 MainWindow.BuildRightPanel() (76 行)。
///
/// 上半: 报表主对象树 (_bandTree)
/// 下半: 选中对象标签 + 属性标题栏 + 属性面板 (_propertyPanel)
///
/// 通过 out 参数传出 _selectedObjLabel (MainWindow 需在 update 时写它)。
/// onResetSelected callback 触发 ResetSelectedProperties。
/// </summary>
internal static class RightPanelBuilder
{
    public static Grid Build(
        FrameworkElement bandTree,
        FrameworkElement propertyPanel,
        Action onResetSelected,
        out TextBlock selectedObjLabel)
    {
        var grid = new Grid { Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)) };
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(180) });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(5) });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        // 上半部分:对象树
        var treePanel = new DockPanel();
        var treeHeader = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
            Padding = new Thickness(6, 3, 6, 3),
            Child = new TextBlock { Text = "报表主对象", FontWeight = FontWeights.Bold, FontSize = 12, Foreground = Brushes.Black },
        };
        DockPanel.SetDock(treeHeader, Dock.Top);
        treePanel.Children.Add(treeHeader);
        treePanel.Children.Add(bandTree);
        Grid.SetRow(treePanel, 0);
        grid.Children.Add(treePanel);

        // 分隔条
        var splitter = new GridSplitter { Height = 5, HorizontalAlignment = HorizontalAlignment.Stretch, Background = new SolidColorBrush(Color.FromRgb(210, 210, 210)) };
        Grid.SetRow(splitter, 1);
        grid.Children.Add(splitter);

        // 下半部分:属性网格
        var propPanel = new DockPanel();

        // 选中对象名 + 操作图标
        selectedObjLabel = new TextBlock { Text = "", FontSize = 11, FontWeight = FontWeights.Bold, Foreground = Brushes.Black, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 0, 0) };
        var objNameBar = new DockPanel { Background = new SolidColorBrush(Color.FromRgb(230, 230, 230)), Height = 24 };
        var objIconPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(2, 0, 4, 0) };
        objIconPanel.Children.Add(MakeSmallIconBtn("📎", "属性"));
        objIconPanel.Children.Add(MakeSmallIconBtn("⚡", "事件"));
        DockPanel.SetDock(objIconPanel, Dock.Left);
        objNameBar.Children.Add(objIconPanel);
        objNameBar.Children.Add(selectedObjLabel);

        // 重置按钮
        var btnReset = new Button { Content = "↺ 重置", Width = 50, Height = 18, FontSize = 9, Margin = new Thickness(0, 0, 4, 0), HorizontalAlignment = HorizontalAlignment.Right };
        btnReset.Click += (_, __) => onResetSelected();
        DockPanel.SetDock(btnReset, Dock.Right);
        objNameBar.Children.Add(btnReset);

        DockPanel.SetDock(objNameBar, Dock.Top);
        propPanel.Children.Add(objNameBar);

        // 属性分类标题栏
        var propTitleBar = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
            Padding = new Thickness(6, 2, 6, 2),
            BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
            BorderThickness = new Thickness(0, 0, 0, 1),
        };
        var propTitleRow = new DockPanel();
        // 排序/分类图标
        var sortIcons = new StackPanel { Orientation = Orientation.Horizontal };
        sortIcons.Children.Add(MakeSmallIconBtn("≡", "分类视图"));
        sortIcons.Children.Add(MakeSmallIconBtn("↕", "字母排序"));
        DockPanel.SetDock(sortIcons, Dock.Left);
        propTitleRow.Children.Add(sortIcons);
        propTitleRow.Children.Add(new TextBlock { Text = "", FontSize = 10 });
        propTitleBar.Child = propTitleRow;
        DockPanel.SetDock(propTitleBar, Dock.Top);
        propPanel.Children.Add(propTitleBar);

        propPanel.Children.Add(propertyPanel);
        Grid.SetRow(propPanel, 2);
        grid.Children.Add(propPanel);

        return grid;
    }
}