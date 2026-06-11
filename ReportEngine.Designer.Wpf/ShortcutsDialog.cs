using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 快捷键一览对话框 - 列出所有支持的快捷键。
/// 等价抽离自 MainWindow.ShowShortcutsDialog()。
/// </summary>
public static class ShortcutsDialog
{
    /// <summary>显示快捷键列表窗口 (模态)。</summary>
    public static void Show(Window owner)
    {
        var dlg = new Window
        {
            Title = "快捷键列表",
            Width = 380,
            Height = 420,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = owner,
            ResizeMode = ResizeMode.NoResize,
        };

        var sp = new StackPanel { Margin = new Thickness(12) };

        var shortcuts = new[]
        {
            ("Ctrl+N", "新建模板"), ("Ctrl+O", "打开模板"), ("Ctrl+S", "保存"),
            ("Ctrl+Z", "撤销"), ("Ctrl+Y", "重做"),
            ("Ctrl+C", "复制"), ("Ctrl+X", "剪切"), ("Ctrl+V", "粘贴"),
            ("Ctrl+D", "复制并偏移"), ("Delete", "删除"), ("Ctrl+A", "全选"),
            ("F1", "快捷键列表"), ("Tab", "切换选中"), ("Shift+Tab", "反向切换"),
            ("方向键", "微调0.5mm"), ("Shift+方向键", "微调5mm"),
            ("Ctrl+滚轮", "缩放"),
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        int row = 0;
        foreach (var (key, desc) in shortcuts)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(22) });

            var keyBlock = new TextBlock
            {
                Text = key,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DarkBlue,
                VerticalAlignment = VerticalAlignment.Center,
            };

            var descBlock = new TextBlock
            {
                Text = desc,
                Foreground = Brushes.Black,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetColumn(descBlock, 1);
            Grid.SetRow(descBlock, row);

            Grid.SetColumn(keyBlock, 0);
            Grid.SetRow(keyBlock, row);

            grid.Children.Add(keyBlock);
            grid.Children.Add(descBlock);

            row++;
        }

        sp.Children.Add(grid);

        var btnClose = new Button
        {
            Content = "关闭",
            Width = 70,
            Height = 26,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 12, 0, 0),
            IsDefault = true,
        };
        btnClose.Click += (_, __) => dlg.Close();
        sp.Children.Add(btnClose);

        dlg.Content = sp;
        dlg.ShowDialog();
    }
}