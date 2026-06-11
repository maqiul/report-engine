using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 网格设置对话框 - 修改网格间距 (mm)。
/// 等价抽离自 MainWindow.ShowGridSettingsDialog()。
///
/// 通过回调传出结果:
///   - onConfirm: (newSpacingMm) → 调用方更新字段并触发画布重渲染
/// </summary>
public static class GridSettingsDialog
{
    /// <summary>显示网格设置窗口。如果输入合法, 触发 onConfirm。</summary>
    public static void Show(Window owner, double currentSpacingMm, Action<double> onConfirm)
    {
        var dlg = new Window
        {
            Title = "网格设置",
            Width = 280,
            Height = 180,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = owner,
            ResizeMode = ResizeMode.NoResize,
        };

        var sp = new StackPanel { Margin = new Thickness(12) };
        sp.Children.Add(new TextBlock
        {
            Text = "网格间距(mm):",
            Margin = new Thickness(0, 0, 0, 4),
            Foreground = Brushes.Black,
        });

        var tbSpacing = new TextBox
        {
            Text = currentSpacingMm.ToString(),
            Margin = new Thickness(0, 0, 0, 8),
            Foreground = Brushes.Black,
        };
        sp.Children.Add(tbSpacing);

        sp.Children.Add(new TextBlock
        {
            Text = "吸附距离阈值(mm): 1.5",
            Margin = new Thickness(0, 0, 0, 12),
            Foreground = Brushes.Gray,
            FontSize = 10,
        });

        var btnOk = new Button
        {
            Content = "确定",
            Width = 70,
            Height = 26,
            HorizontalAlignment = HorizontalAlignment.Right,
            IsDefault = true,
        };
        btnOk.Click += (_, __) =>
        {
            if (double.TryParse(tbSpacing.Text, out var d) && d > 0 && d <= 50)
            {
                onConfirm(d);
            }
            dlg.Close();
        };
        sp.Children.Add(btnOk);

        dlg.Content = sp;
        dlg.ShowDialog();
    }
}