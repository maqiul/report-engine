using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static ReportEngine.Designer.Wpf.UiFactory;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 对齐工具栏构造 - 把 BuildAlignToolBar 从 MainWindow 抽离。
/// 等价抽离自 MainWindow.BuildAlignToolBar() (28 行)。
///
/// 17 个工具按钮 (8 对齐 + 2 Z-order + 1 格式刷 + 1 锁定 + 2 组合/取消组合 + 3 Separator)，
/// 每个按钮的 click handler 通过 callback 传出。
/// </summary>
internal static class AlignToolBarBuilder
{
    public static ToolBarTray Build(
        Action onAlignLeft,
        Action onAlignRight,
        Action onAlignTop,
        Action onAlignBottom,
        Action onAlignHCenter,
        Action onAlignVCenter,
        Action onSameWidth,
        Action onSameHeight,
        Action onBringToFront,
        Action onSendToBack,
        Action onFormatPainter,
        Action onToggleLock,
        Action onGroup,
        Action onUngroup)
    {
        var tray = new ToolBarTray { Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)) };
        var tb = new ToolBar { Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)), Band = 0 };

        tb.Items.Add(MakeToolBtn("▐▌ 左对齐", onAlignLeft));
        tb.Items.Add(MakeToolBtn("▐▌ 右对齐", onAlignRight));
        tb.Items.Add(MakeToolBtn("▔ 顶端对齐", onAlignTop));
        tb.Items.Add(MakeToolBtn("▁ 底端对齐", onAlignBottom));
        tb.Items.Add(new Separator());
        tb.Items.Add(MakeToolBtn("┃ 水平居中", onAlignHCenter));
        tb.Items.Add(MakeToolBtn("━ 垂直居中", onAlignVCenter));
        tb.Items.Add(new Separator());
        tb.Items.Add(MakeToolBtn("↔ 等宽", onSameWidth));
        tb.Items.Add(MakeToolBtn("↕ 等高", onSameHeight));
        tb.Items.Add(new Separator());
        tb.Items.Add(MakeToolBtn("▢↑ 置顶", onBringToFront));
        tb.Items.Add(MakeToolBtn("▢↓ 置底", onSendToBack));
        tb.Items.Add(new Separator());
        tb.Items.Add(MakeToolBtn("🖌 格式刷", onFormatPainter));
        tb.Items.Add(MakeToolBtn("🔒 锁定", onToggleLock));
        tb.Items.Add(new Separator());
        tb.Items.Add(MakeToolBtn("🔗 组合", onGroup));
        tb.Items.Add(MakeToolBtn("✂ 取消组合", onUngroup));

        tray.ToolBars.Add(tb);
        return tray;
    }
}