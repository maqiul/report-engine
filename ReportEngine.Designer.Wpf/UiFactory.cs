using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// WPF 设计器 UI 控件工厂：按钮 / 菜单项 / 状态栏标签 / 工具箱按钮 / 元素边框。
/// 从 MainWindow.cs 抽出，所有方法均为静态无副作用。
/// </summary>
internal static class UiFactory
{
    /// <summary>工具栏按钮：透明背景 + 自定义 disabled 样式</summary>
    public static Button MakeToolBtn(string text, Action onClick)
    {
        var btn = new Button
        {
            Content = text,
            Padding = new Thickness(6, 3, 6, 3),
            Background = Brushes.Transparent,
            Foreground = Brushes.Black,
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand,
            FontSize = 12,
        };
        btn.Click += (_, __) => onClick();

        // 添加禁用状态样式：置灰
        btn.IsEnabledChanged += (s, e) =>
        {
            var b = (Button)s;
            if (!b.IsEnabled)
            {
                b.Foreground = Brushes.Gray;
                b.Opacity = 0.5;
                b.Cursor = Cursors.Arrow;
            }
            else
            {
                b.Foreground = Brushes.Black;
                b.Opacity = 1.0;
                b.Cursor = Cursors.Hand;
            }
        };

        return btn;
    }

    /// <summary>底部状态栏视图切换 Tab</summary>
    public static Border MakeStatusTab(string text, bool active, Action? onClick = null)
    {
        var border = new Border
        {
            Padding = new Thickness(12, 4, 12, 4),
            Background = active ? new SolidColorBrush(Color.FromRgb(0, 122, 204)) : Brushes.Transparent,
            Child = new TextBlock
            {
                Text = text,
                Foreground = active ? Brushes.White : Brushes.Black,
                FontSize = 11,
            },
            Cursor = onClick != null ? Cursors.Hand : Cursors.Arrow,
        };
        if (onClick != null)
            border.MouseLeftButtonDown += (_, __) => onClick();
        return border;
    }

    /// <summary>格式按钮：粗体 / 斜体 / 下划线</summary>
    public static Button MakeFmtBtn(string text, FontWeight weight, Action onClick, bool italic = false, bool underline = false)
    {
        var tb = new TextBlock { Text = text, FontWeight = weight, FontSize = 13 };
        if (italic) tb.FontStyle = FontStyles.Italic;
        if (underline) tb.TextDecorations = TextDecorations.Underline;
        var btn = new Button { Content = tb, Padding = new Thickness(6, 2, 6, 2), Background = Brushes.Transparent, BorderThickness = new Thickness(0), Cursor = Cursors.Hand, MinWidth = 28 };
        btn.Click += (_, __) => onClick();
        return btn;
    }

    /// <summary>工具箱按钮：左侧对齐 + 可选拖拽类型</summary>
    public static void AddToolboxBtn(StackPanel sp, string text, Action onClick, string? dragType = null)
    {
        var btn = new Button
        {
            Content = text,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            Padding = new Thickness(8, 4, 8, 4),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Cursor = dragType != null ? Cursors.SizeAll : Cursors.Hand,
        };
        btn.Click += (_, __) => onClick();
        if (dragType != null)
            btn.Tag = dragType;
        sp.Children.Add(btn);
    }

    /// <summary>小图标按钮（属性面板用）</summary>
    public static Button MakeSmallIconBtn(string icon, string tooltip)
    {
        return new Button
        {
            Content = new TextBlock { Text = icon, FontSize = 12 },
            Width = 22,
            Height = 20,
            Padding = new Thickness(0),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            ToolTip = tooltip,
            Cursor = Cursors.Hand,
        };
    }

    /// <summary>菜单项（带快捷键提示）</summary>
    public static MenuItem MakeMenuItem(string header, string? gesture, Action onClick)
    {
        var mi = new MenuItem { Header = header };
        if (gesture != null) mi.InputGestureText = gesture;
        mi.Click += (_, __) => onClick();
        return mi;
    }

    /// <summary>模板面板里的元素边框（视觉占位）</summary>
    public static Border MakeElementBorder(double w, double h, Brush borderBrush, Color bgColor, string text, double fontSize)
    {
        return new Border
        {
            Width = w,
            Height = h,
            BorderBrush = borderBrush,
            BorderThickness = new Thickness(1),
            Background = new SolidColorBrush(bgColor),
            Child = new TextBlock
            {
                Text = text,
                FontSize = Math.Max(6, fontSize),
                Foreground = borderBrush,
                Margin = new Thickness(2),
                TextTrimming = TextTrimming.CharacterEllipsis,
            },
        };
    }
}