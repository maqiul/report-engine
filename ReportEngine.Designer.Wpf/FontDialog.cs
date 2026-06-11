using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 字体选择弹窗：从 MainWindow 抽离的纯 WPF 弹窗工具。
/// 提供字体 / 字形 / 大小 / 下划线 / 颜色 / 删除线 6 项设置, OK 后直接
/// 写回传入的 TextElement.Font 字段, Cancel 不做修改。
/// 行为完全等价原 MainWindow.ShowFontDialog (line 2989-3143, ~154 行)。
/// 注: 原代码没有 PushUndo 记录, 这是属性面板字体行的历史设计, 本
/// 次抽离不修复, 保持等价行为。
/// </summary>
internal static class FontDialog
{
    /// <summary>
    /// 弹出字体选择对话框。OK 后直接写回 t.Font 字段, Cancel 不做修改。
    /// </summary>
    /// <param name="owner">父窗口（居中和模态用）</param>
    /// <param name="t">要修改字体的 TextElement（OK 时直接改 Font 字段）</param>
    public static void Show(Window owner, TextElement t)
    {
        var dlg = new Window
        {
            Title = "字体",
            Width = 560,
            Height = 460,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = owner,
            ResizeMode = ResizeMode.NoResize,
            Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
        };

        var mainGrid = new Grid { Margin = new Thickness(12) };
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(8) });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80) });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(8) });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(100) });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(10) });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(32) });

        // 上部三栏: 字体 | 字形 | 大小
        var topGrid = new Grid();
        topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
        topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) });
        topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
        topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.8, GridUnitType.Star) });

        // 字体列
        var fontPanel = new DockPanel();
        fontPanel.Children.Add(new TextBlock { Text = "字体(F):", FontSize = 11, Margin = new Thickness(0, 0, 0, 2) });
        DockPanel.SetDock(fontPanel.Children[0], Dock.Top);
        var fontInput = new TextBox { Text = t.Font.Family, FontSize = 12, Padding = new Thickness(4, 2, 4, 2), Margin = new Thickness(0, 0, 0, 2) };
        DockPanel.SetDock(fontInput, Dock.Top);
        fontPanel.Children.Add(fontInput);
        var fontList = new ListBox { FontSize = 12 };
        foreach (var f in new[] { "宋体", "黑体", "楷体", "仿宋", "微软雅黑", "华文中宋", "新宋体", "幼圆", "Arial", "Times New Roman", "Courier New", "Verdana", "Tahoma", "Segoe UI", "Consolas" })
            fontList.Items.Add(new ListBoxItem { Content = f, FontFamily = new FontFamily(f) });
        foreach (ListBoxItem item in fontList.Items)
            if ((string)item.Content == t.Font.Family) { item.IsSelected = true; fontList.ScrollIntoView(item); break; }
        fontList.SelectionChanged += (_, __) => { if (fontList.SelectedItem is ListBoxItem si) fontInput.Text = (string)si.Content; };
        fontPanel.Children.Add(fontList);
        Grid.SetColumn(fontPanel, 0);
        topGrid.Children.Add(fontPanel);

        // 字形列
        var stylePanel = new DockPanel();
        stylePanel.Children.Add(new TextBlock { Text = "字形(Y):", FontSize = 11, Margin = new Thickness(0, 0, 0, 2) });
        DockPanel.SetDock(stylePanel.Children[0], Dock.Top);
        var styleList = new ListBox { FontSize = 12 };
        foreach (var s in new[] { "常规", "粗体", "斜体", "粗斜体" })
            styleList.Items.Add(s);
        styleList.SelectedItem = t.Font.Bold && t.Font.Italic ? "粗斜体" : t.Font.Bold ? "粗体" : t.Font.Italic ? "斜体" : "常规";
        stylePanel.Children.Add(styleList);
        Grid.SetColumn(stylePanel, 2);
        topGrid.Children.Add(stylePanel);

        // 大小列
        var sizePanel = new DockPanel();
        sizePanel.Children.Add(new TextBlock { Text = "大小(S):", FontSize = 11, Margin = new Thickness(0, 0, 0, 2) });
        DockPanel.SetDock(sizePanel.Children[0], Dock.Top);
        var sizeInput = new TextBox { Text = t.Font.Size.ToString(), FontSize = 12, Padding = new Thickness(4, 2, 4, 2), Margin = new Thickness(0, 0, 0, 2) };
        DockPanel.SetDock(sizeInput, Dock.Top);
        sizePanel.Children.Add(sizeInput);
        var sizeList = new ListBox { FontSize = 12 };
        foreach (var s in new[] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 })
            sizeList.Items.Add(s);
        sizeList.SelectedItem = t.Font.Size;
        sizeList.SelectionChanged += (_, __) => { if (sizeList.SelectedItem != null) sizeInput.Text = sizeList.SelectedItem.ToString(); };
        sizePanel.Children.Add(sizeList);
        Grid.SetColumn(sizePanel, 4);
        topGrid.Children.Add(sizePanel);

        Grid.SetRow(topGrid, 0);
        mainGrid.Children.Add(topGrid);

        // 中部: 效果行 (下划线 + 删除线)
        var effectGrid = new Grid();
        effectGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
        effectGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        effectGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
        var chkUnderline = new CheckBox { Content = "下划线(U)", IsChecked = t.Font.Underline, FontSize = 11, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 16, 0) };
        Grid.SetColumn(chkUnderline, 0);
        effectGrid.Children.Add(chkUnderline);
        // 注: 原 ShowFontDialog 中 chkStrike 仅为占位 UI, 未回写到 FontDef (无 Strikeout 字段)
        var chkStrike = new CheckBox { Content = "删除线(K)", FontSize = 11, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Right };
        Grid.SetColumn(chkStrike, 2);
        effectGrid.Children.Add(chkStrike);
        Grid.SetRow(effectGrid, 2);
        mainGrid.Children.Add(effectGrid);

        // 颜色行
        var colorGrid = new Grid();
        colorGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
        colorGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        var colorLabel = new TextBlock { Text = "颜色:", FontSize = 11, VerticalAlignment = VerticalAlignment.Center };
        Grid.SetColumn(colorLabel, 0);
        colorGrid.Children.Add(colorLabel);

        var colorPanel = new WrapPanel();
        var presetColors = new[] { "#000000", "#333333", "#666666", "#999999", "#CCCCCC", "#FFFFFF",
            "#FF0000", "#FF6600", "#FFCC00", "#33CC00", "#0099FF", "#6633FF",
            "#CC0066", "#FF3399", "#FF9966", "#66CC66", "#3399CC", "#9966CC" };
        // 推断 currentColor: 从 Font.Color? 默认黑
        string currentColor = "#000000";
        var colorTextBox = new TextBox { Text = currentColor, Width = 80, FontSize = 11, Margin = new Thickness(4, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
        var colorPreview = new Border { Width = 24, Height = 18, CornerRadius = new CornerRadius(2), BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1), Background = BrushParser.Parse(currentColor, Brushes.Black), Cursor = Cursors.Hand, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 0, 0) };
        foreach (var c in presetColors)
        {
            var sw = new Border { Width = 20, Height = 20, Margin = new Thickness(2), CornerRadius = new CornerRadius(2), Background = BrushParser.Parse(c, Brushes.White), BorderBrush = Brushes.LightGray, BorderThickness = new Thickness(1), Cursor = Cursors.Hand };
            var cc = c;
            sw.MouseLeftButtonDown += (_, __) => { colorTextBox.Text = cc; colorPreview.Background = BrushParser.Parse(cc, Brushes.Black); };
            colorPanel.Children.Add(sw);
        }
        colorPanel.Children.Add(colorPreview);
        colorPanel.Children.Add(colorTextBox);
        Grid.SetColumn(colorPanel, 1);
        colorGrid.Children.Add(colorPanel);
        Grid.SetRow(colorGrid, 4);
        mainGrid.Children.Add(colorGrid);

        // 底部按钮
        var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var btnOk = new Button { Content = "确定", Width = 80, Height = 28, Margin = new Thickness(0, 0, 8, 0), IsDefault = true };
        var btnCancel = new Button { Content = "取消", Width = 80, Height = 28, IsCancel = true };
        btnPanel.Children.Add(btnOk);
        btnPanel.Children.Add(btnCancel);
        Grid.SetRow(btnPanel, 6);
        mainGrid.Children.Add(btnPanel);

        btnOk.Click += (_, __) =>
        {
            if (!double.TryParse(sizeInput.Text, out var sz) || sz <= 0) sz = t.Font.Size;
            t.Font.Family = fontInput.Text;
            t.Font.Size = sz;
            string style = (string)(styleList.SelectedItem ?? "常规");
            t.Font.Bold = style == "粗体" || style == "粗斜体";
            t.Font.Italic = style == "斜体" || style == "粗斜体";
            t.Font.Underline = chkUnderline.IsChecked == true;
            // 注: FontDef 无 Strikeout 字段, chkStrike 仅展示, 不回写
            dlg.DialogResult = true;
        };

        dlg.Content = mainGrid;
        dlg.ShowDialog();
    }
}
