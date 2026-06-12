using System;
using System.Windows;
using System.Windows.Controls;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 元素重命名弹窗 - 把 ShowRenameDialog 从 MainWindow 抽离。
/// 等价抽离自 MainWindow.ShowRenameDialog() (16 行)。
///
/// 通过 onCommit 回调传出用户输入的新名称 (空串视为 null)。
/// </summary>
internal static class RenameDialog
{
    public static void Show(Window owner, ReportElement element, Action<string> onCommit)
    {
        var dlg = new Window
        {
            Title = "重命名元素",
            Width = 300, Height = 120,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = owner, ResizeMode = ResizeMode.NoResize,
        };
        var sp = new StackPanel { Margin = new Thickness(12) };
        sp.Children.Add(new TextBlock { Text = "名称:", Margin = new Thickness(0, 0, 0, 4), Foreground = System.Windows.Media.Brushes.Black });
        var tb = new TextBox { Text = element.Name ?? "", Margin = new Thickness(0, 0, 0, 8), Foreground = System.Windows.Media.Brushes.Black };
        sp.Children.Add(tb);
        var btnOk = new Button { Content = "确定", Width = 70, Height = 26, HorizontalAlignment = HorizontalAlignment.Right, IsDefault = true };
        btnOk.Click += (_, __) => { onCommit(tb.Text); dlg.Close(); };
        sp.Children.Add(btnOk);
        dlg.Content = sp;
        dlg.ShowDialog();
    }
}