using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 属性面板卡片 UI 工厂 - 把"多联打印"等复杂卡片从 UpdatePropertyListCore 抽离。
/// 等价抽离自 MainWindow.UpdatePropertyListCore() 中的 2 个多联打印 inline Border。
/// </summary>
internal static class PropertyCardFactory
{
    /// <summary>
    /// 多联打印"已启用"状态卡片：标题 + 网格（布局/方向可切换/间距/单联尺寸）+ "修改配置" 按钮。
    /// </summary>
    /// <param name="muInfo">多联打印配置</param>
    /// <param name="template">当前模板（用于算单联尺寸）</param>
    /// <param name="onEdit">点 "修改配置" 按钮时回调（通常打开 PageSetupDialog）</param>
    /// <param name="onDirectionToggled">点 "切换" 按钮时回调, 接收新方向 ("Vertical"/"Horizontal")，外部负责写回 + 重新渲染</param>
    public static FrameworkElement CreateMultiUpEnabledCard(
        MultiUpConfig muInfo,
        ReportTemplate template,
        Action onEdit,
        Action<string> onDirectionToggled)
    {
        var card = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(245, 250, 245)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(100, 180, 100)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(8),
            Margin = new Thickness(0, 2, 0, 8)
        };
        var stack = new StackPanel();

        // 标题行
        var title = new DockPanel { Margin = new Thickness(0, 0, 0, 6) };
        title.Children.Add(new TextBlock
        {
            Text = "✅ 多联打印已启用",
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.DarkGreen,
            FontSize = 11
        });
        stack.Children.Add(title);

        // 配置信息网格
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        int muRow = 0;
        void AddMuRow(string label, string value)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(22) });
            var lblTb = new TextBlock
            {
                Text = label + ":",
                Foreground = Brushes.Gray,
                FontSize = 10,
                VerticalAlignment = VerticalAlignment.Center
            };
            grid.Children.Add(lblTb);
            Grid.SetRow(lblTb, muRow);
            var valTb = new TextBlock
            {
                Text = value,
                Foreground = Brushes.Black,
                FontSize = 10,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(valTb, 1);
            Grid.SetRow(valTb, muRow);
            grid.Children.Add(valTb);
            muRow++;
        }

        AddMuRow("布局", $"{muInfo.Rows} 行 × {muInfo.Columns} 列 = {muInfo.Count} 份/页");

        // 打印顺序 - 可切换
        var muDirRow = muRow;
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(22) });
        var dirLbl = new TextBlock
        {
            Text = "打印顺序:",
            Foreground = Brushes.Gray,
            FontSize = 10,
            VerticalAlignment = VerticalAlignment.Center
        };
        grid.Children.Add(dirLbl);
        Grid.SetRow(dirLbl, muDirRow);

        // 值 + 按钮放在 DockPanel 中
        var dirPanel = new DockPanel();
        var dirValue = new TextBlock
        {
            Text = muInfo.Direction == "Vertical" ? "先列后行 (垂直)" : "先行后列 (水平/Z字形)",
            Foreground = Brushes.Black,
            FontSize = 10,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center
        };
        DockPanel.SetDock(dirValue, Dock.Left);
        dirPanel.Children.Add(dirValue);

        // 切换按钮
        var dirToggleBtn = new Button
        {
            Content = "⇄ 切换",
            Width = 50,
            Height = 18,
            FontSize = 9,
            Margin = new Thickness(8, 0, 0, 0)
        };
        dirToggleBtn.Click += (_, __) =>
        {
            var newDir = muInfo.Direction == "Vertical" ? "Horizontal" : "Vertical";
            dirValue.Text = newDir == "Vertical" ? "先列后行 (垂直)" : "先行后列 (水平/Z字形)";
            onDirectionToggled(newDir);
        };
        DockPanel.SetDock(dirToggleBtn, Dock.Left);
        dirPanel.Children.Add(dirToggleBtn);

        Grid.SetColumn(dirPanel, 1);
        Grid.SetRow(dirPanel, muDirRow);
        grid.Children.Add(dirPanel);
        muRow++;

        AddMuRow("间距", $"水平 {muInfo.HSpacing}mm  垂直 {muInfo.VSpacing}mm");

        // 计算单联尺寸
        double cw = (template.Page.Width - muInfo.HSpacing * (muInfo.Columns - 1)) / muInfo.Columns;
        double ch = (template.Page.Height - muInfo.VSpacing * (muInfo.Rows - 1)) / muInfo.Rows;
        AddMuRow("单联尺寸", $"{Math.Round(cw, 1)} × {Math.Round(ch, 1)} mm");

        stack.Children.Add(grid);
        card.Child = stack;

        // 包装 Border + 按钮到外层容器返回
        var container = new StackPanel();
        container.Children.Add(card);

        // 修改按钮
        var muEditBtn = new Button
        {
            Content = "⚙ 修改配置",
            Width = 100,
            Height = 24,
            Margin = new Thickness(0, 0, 0, 4),
            FontSize = 10,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        muEditBtn.Click += (_, __) => onEdit();
        container.Children.Add(muEditBtn);

        return container;
    }

    /// <summary>
    /// 多联打印"未启用"状态卡片：未启用提示 + "启用" 按钮。
    /// </summary>
    /// <param name="onEnable">点 "启用多联打印" 按钮时回调（通常打开 PageSetupDialog）</param>
    public static FrameworkElement CreateMultiUpDisabledCard(Action onEnable)
    {
        var card = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(250, 250, 250)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(8),
            Margin = new Thickness(0, 2, 0, 8)
        };
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock
        {
            Text = "多联打印未启用",
            Foreground = Brushes.Gray,
            FontSize = 10,
            Margin = new Thickness(0, 0, 0, 4)
        });

        var muEnableBtn = new Button
        {
            Content = "+ 启用多联打印",
            Width = 100,
            Height = 24,
            FontSize = 10,
            HorizontalAlignment = HorizontalAlignment.Right,
            Foreground = Brushes.DarkBlue
        };
        muEnableBtn.Click += (_, __) => onEnable();
        stack.Children.Add(muEnableBtn);

        card.Child = stack;
        return card;
    }
}