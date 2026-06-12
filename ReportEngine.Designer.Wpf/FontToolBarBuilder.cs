using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static ReportEngine.Designer.Wpf.UiFactory;
using TextAlignment = System.Windows.TextAlignment;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 字体工具栏构造 - 把 BuildFontToolBar 从 MainWindow 抽离。
/// 等价抽离自 MainWindow.BuildFontToolBar() (35 行)。
///
/// 字号 + 字体下拉 + B/I/U + 左/中/右对齐。
///
/// 通过 out 参数传出 _fontFamilyCombo / _fontSizeCombo (MainWindow 需在 Apply 时读它们)。
/// 5 个 callback 触发 ApplyFontFamily/Size/ToggleB/I/U + SetAlignment。
/// </summary>
internal static class FontToolBarBuilder
{
    public static ToolBarTray Build(
        Action onFontFamilyChanged,
        Action onFontSizeChanged,
        Action onToggleBold,
        Action onToggleItalic,
        Action onToggleUnderline,
        Action onAlignLeft,
        Action onAlignCenter,
        Action onAlignRight,
        out ComboBox fontFamilyCombo,
        out ComboBox fontSizeCombo)
    {
        var tray = new ToolBarTray { Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)) };
        var tb = new ToolBar { Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)), Band = 0 };

        fontFamilyCombo = new ComboBox { Width = 120, FontSize = 11, IsEditable = true };
        foreach (var f in new[] { "宋体", "黑体", "微软雅黑", "楷体", "仿宋", "Arial", "Times New Roman", "Courier New" })
            fontFamilyCombo.Items.Add(f);
        fontFamilyCombo.Text = "宋体";
        fontFamilyCombo.SelectionChanged += (_, __) => onFontFamilyChanged();
        fontFamilyCombo.LostFocus += (_, __) => onFontFamilyChanged();
        tb.Items.Add(fontFamilyCombo);

        fontSizeCombo = new ComboBox { Width = 60, FontSize = 11, IsEditable = true };
        foreach (var s in new[] { "8", "9", "10", "10.5", "11", "12", "14", "16", "18", "20", "22", "24", "28", "36", "48", "72" })
            fontSizeCombo.Items.Add(s);
        fontSizeCombo.Text = "10";
        fontSizeCombo.SelectionChanged += (_, __) => onFontSizeChanged();
        fontSizeCombo.LostFocus += (_, __) => onFontSizeChanged();
        tb.Items.Add(fontSizeCombo);

        tb.Items.Add(new Separator());
        tb.Items.Add(MakeFmtBtn("B", FontWeights.Bold, onToggleBold));
        tb.Items.Add(MakeFmtBtn("I", FontWeights.Normal, onToggleItalic, true));
        tb.Items.Add(MakeFmtBtn("U", FontWeights.Normal, onToggleUnderline, false, true));
        tb.Items.Add(new Separator());
        tb.Items.Add(MakeToolBtn("☰ 左", onAlignLeft));
        tb.Items.Add(MakeToolBtn("☰ 中", onAlignCenter));
        tb.Items.Add(MakeToolBtn("☰ 右", onAlignRight));

        tray.ToolBars.Add(tb);
        return tray;
    }
}
