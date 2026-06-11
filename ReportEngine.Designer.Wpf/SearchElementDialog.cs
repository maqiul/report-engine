using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 搜索元素对话框 - 输入关键字, 在模板所有 Band 的元素中匹配 Name/Id。
/// 等价抽离自 MainWindow.SearchElement()。
///
/// 通过回调传出结果 (避免静态事件总线导致 MainWindow 内存泄漏):
///   - onFound:    (element, band) → 调用方更新选中态并 RefreshUI
///   - onNotFound: ()              → 调用方更新状态栏提示
/// </summary>
public static class SearchElementDialog
{
    /// <summary>显示搜索对话框。返回用户是否输入了非空关键字。</summary>
    public static bool Show(
        Window owner,
        ReportTemplate? template,
        Action<ReportElement, Band> onFound,
        Action onNotFound)
    {
        if (template == null) return false;

        var dlg = new Window
        {
            Title = "搜索元素",
            Width = 320,
            Height = 120,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = owner,
            ResizeMode = ResizeMode.NoResize,
        };

        var sp = new StackPanel { Margin = new Thickness(12) };
        sp.Children.Add(new TextBlock
        {
            Text = "搜索名称或ID:",
            Margin = new Thickness(0, 0, 0, 4),
            Foreground = Brushes.Black,
        });

        var tb = new TextBox
        {
            Text = "",
            Margin = new Thickness(0, 0, 0, 8),
            Foreground = Brushes.Black,
        };
        sp.Children.Add(tb);

        var btnOk = new Button
        {
            Content = "搜索",
            Width = 70,
            Height = 26,
            HorizontalAlignment = HorizontalAlignment.Right,
            IsDefault = true,
        };
        btnOk.Click += (_, __) =>
        {
            var keyword = tb.Text.Trim();
            if (string.IsNullOrEmpty(keyword)) { dlg.Close(); return; }

            foreach (var band in template.Bands)
            {
                foreach (var el in band.Elements)
                {
                    if ((!string.IsNullOrEmpty(el.Name) && el.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                        el.Id.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        onFound(el, band);
                        dlg.Close();
                        return;
                    }
                }
            }
            onNotFound();
            dlg.Close();
        };
        sp.Children.Add(btnOk);

        dlg.Content = sp;
        dlg.ShowDialog();
        return true;
    }
}