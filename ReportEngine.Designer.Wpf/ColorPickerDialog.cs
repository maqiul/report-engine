using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 颜色选择弹窗：从 MainWindow 抽离的纯 WPF 弹窗工具。
/// 提供 24 个预设色 + 自定义 Hex 输入 + 实时预览，返回 null = 取消，
/// 返回 "" = 清除（不选），返回 "#RRGGBB" = 选中。
/// </summary>
internal static class ColorPickerDialog
{
    /// <summary>
    /// 弹出颜色选择对话框。返回 null 表示取消；返回 "" 表示清除；返回
    /// "#RRGGBB"/"#AARRGGBB" 表示用户选中的颜色。
    /// </summary>
    /// <param name="owner">父窗口（居中和模态用）</param>
    /// <param name="currentColor">当前颜色（Hex 字符串，可空可空）</param>
    public static string? Show(Window owner, string currentColor)
    {
        var dlg = new Window
        {
            Title = "选择颜色", Width = 300, Height = 360,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = owner, ResizeMode = ResizeMode.NoResize,
        };
        var stack = new StackPanel { Margin = new Thickness(12) };

        // 预设颜色网格
        var presetColors = new[] { "#000000", "#333333", "#666666", "#999999", "#CCCCCC", "#FFFFFF",
            "#FF0000", "#FF6600", "#FFCC00", "#33CC00", "#0099FF", "#6633FF",
            "#CC0066", "#FF3399", "#FF9966", "#66CC66", "#3399CC", "#9966CC",
            "#800000", "#804000", "#808000", "#008000", "#004080", "#400080" };
        var grid2 = new WrapPanel { Margin = new Thickness(0, 0, 0, 12) };
        string? result = null;
        var previewBorder = new Border
        {
            Width = 40, Height = 40, CornerRadius = new CornerRadius(4),
            BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 8, 0, 8)
        };
        previewBorder.Background = BrushParser.Parse(currentColor, Brushes.White);
        var hexBox = new TextBox
        {
            Text = currentColor, FontSize = 12, Width = 120,
            Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center
        };
        hexBox.TextChanged += (_, __) =>
        {
            previewBorder.Background = BrushParser.Parse(hexBox.Text, Brushes.White);
        };

        foreach (var c in presetColors)
        {
            var swatch = new Border
            {
                Width = 28, Height = 28, Margin = new Thickness(2), CornerRadius = new CornerRadius(3),
                Background = BrushParser.Parse(c, Brushes.White), BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1), Cursor = Cursors.Hand
            };
            var cc = c;
            swatch.MouseLeftButtonDown += (_, __) =>
            {
                hexBox.Text = cc;
                previewBorder.Background = BrushParser.Parse(cc, Brushes.White);
            };
            grid2.Children.Add(swatch);
        }
        stack.Children.Add(new TextBlock { Text = "预设颜色", FontSize = 11, FontWeight = FontWeights.Bold });
        stack.Children.Add(grid2);

        // 预览 + hex 输入
        var previewRow = new StackPanel { Orientation = Orientation.Horizontal };
        previewRow.Children.Add(previewBorder);
        previewRow.Children.Add(hexBox);
        stack.Children.Add(new TextBlock { Text = "自定义 (Hex)", FontSize = 11, FontWeight = FontWeights.Bold });
        stack.Children.Add(previewRow);

        // 按钮
        var btnPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 16, 0, 0)
        };
        var btnOk = new Button
        {
            Content = "确定", Width = 70, Height = 26,
            Margin = new Thickness(0, 0, 8, 0), IsDefault = true
        };
        var btnClear = new Button
        {
            Content = "清除", Width = 70, Height = 26, Margin = new Thickness(0, 0, 8, 0)
        };
        var btnCancel = new Button
        {
            Content = "取消", Width = 70, Height = 26, IsCancel = true
        };
        btnOk.Click += (_, __) => { result = hexBox.Text; dlg.DialogResult = true; };
        btnClear.Click += (_, __) => { result = ""; dlg.DialogResult = true; };
        btnPanel.Children.Add(btnOk);
        btnPanel.Children.Add(btnClear);
        btnPanel.Children.Add(btnCancel);
        stack.Children.Add(btnPanel);

        dlg.Content = stack;
        return dlg.ShowDialog() == true ? result : null;
    }
}
