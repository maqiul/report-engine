using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 关于对话框 - 显示产品信息和版本号。
/// 等价抽离自 MainWindow.ShowAboutDialog()。
/// </summary>
public static class AboutDialog
{
    /// <summary>显示关于窗口 (模态)。</summary>
    public static void Show(Window owner)
    {
        var dlg = new Window
        {
            Title = "关于",
            Width = 320,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = owner,
            ResizeMode = ResizeMode.NoResize,
        };

        var sp = new StackPanel
        {
            Margin = new Thickness(20),
            HorizontalAlignment = HorizontalAlignment.Center,
        };

        sp.Children.Add(new TextBlock
        {
            Text = "报表设计器",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.DarkBlue,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 8),
        });

        sp.Children.Add(new TextBlock
        {
            Text = "基于 ReportEngine.Core",
            FontSize = 12,
            Foreground = Brushes.Gray,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 4),
        });

        sp.Children.Add(new TextBlock
        {
            Text = "版本 1.0.0",
            FontSize = 11,
            Foreground = Brushes.DimGray,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 16),
        });

        sp.Children.Add(new TextBlock
        {
            Text = "支持元素拖拽、吸附对齐、格式刷、分组、旋转、透明度等",
            FontSize = 10,
            Foreground = Brushes.Gray,
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 12),
        });

        var btnClose = new Button
        {
            Content = "关闭",
            Width = 70,
            Height = 26,
            HorizontalAlignment = HorizontalAlignment.Center,
            IsDefault = true,
        };
        btnClose.Click += (_, __) => dlg.Close();
        sp.Children.Add(btnClose);

        dlg.Content = sp;
        dlg.ShowDialog();
    }
}