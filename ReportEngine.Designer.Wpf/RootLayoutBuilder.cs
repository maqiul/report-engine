using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 主窗口根布局构造 - 把 BuildLayout 从 MainWindow 抽离。
/// 等价抽离自 MainWindow.BuildLayout() (66 行)。
///
/// 顶部: 菜单 / 工具栏 / 字体栏 / 对齐栏 (DockPanel.Dock.Top)。
/// 底部: 状态栏 (DockPanel.Dock.Bottom)。
/// 中央: 5 列 Grid (左 220 + 2 个 5px 分割条 + 中 * + 右 280)。
///
/// 各 BuildXxx 由调用方提供, 这里只负责拼接顺序和列宽。
/// </summary>
internal static class RootLayoutBuilder
{
    public static DockPanel Build(
        Menu menu,
        FrameworkElement toolbar,
        FrameworkElement fontBar,
        FrameworkElement alignBar,
        FrameworkElement statusBar,
        FrameworkElement leftPanel,
        FrameworkElement centerPanel,
        FrameworkElement rightPanel)
    {
        var root = new DockPanel();

        // 顶部四栏
        DockPanel.SetDock(menu, Dock.Top);
        menu.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));
        menu.Foreground = Brushes.Black;
        root.Children.Add(menu);

        DockPanel.SetDock(toolbar, Dock.Top);
        root.Children.Add(toolbar);

        DockPanel.SetDock(fontBar, Dock.Top);
        root.Children.Add(fontBar);

        DockPanel.SetDock(alignBar, Dock.Top);
        root.Children.Add(alignBar);

        // 底部状态栏
        DockPanel.SetDock(statusBar, Dock.Bottom);
        root.Children.Add(statusBar);

        // 三栏主体
        var mainGrid = new Grid();
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5) });
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5) });
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(280) });

        // 左栏
        Grid.SetColumn(leftPanel, 0);
        mainGrid.Children.Add(leftPanel);

        var sp1 = new GridSplitter { Width = 5, HorizontalAlignment = HorizontalAlignment.Stretch, Background = new SolidColorBrush(Color.FromRgb(210, 210, 210)) };
        Grid.SetColumn(sp1, 1);
        mainGrid.Children.Add(sp1);

        // 中央
        Grid.SetColumn(centerPanel, 2);
        mainGrid.Children.Add(centerPanel);

        var sp2 = new GridSplitter { Width = 5, HorizontalAlignment = HorizontalAlignment.Stretch, Background = new SolidColorBrush(Color.FromRgb(210, 210, 210)) };
        Grid.SetColumn(sp2, 3);
        mainGrid.Children.Add(sp2);

        // 右栏
        Grid.SetColumn(rightPanel, 4);
        mainGrid.Children.Add(rightPanel);

        root.Children.Add(mainGrid);
        return root;
    }
}
