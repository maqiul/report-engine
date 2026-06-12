using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static ReportEngine.Designer.Wpf.UiFactory;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 主工具栏构造 - 把 BuildToolBar 从 MainWindow 抽离。
/// 等价抽离自 MainWindow.BuildToolBar() (37 行)。
///
/// 17 个工具按钮: 新建/打开/保存/撤销/重做/剪切/复制/粘贴/删除/页面设置/PDF/Excel/+/-/适合/。
///
/// 通过 out 参数传出 6 个按钮 (MainWindow 需在 undo/redo 状态更新时 Enable/Disable 它们)。
/// 12 个 callback 触发对应操作 + 3 个缩放 slider 操作。
/// </summary>
internal static class ToolBarBuilder
{
    public static ToolBar Build(
        Action onNew,
        Action onOpen,
        Action onSave,
        Action onUndo,
        Action onRedo,
        Action onCut,
        Action onCopy,
        Action onPaste,
        Action onDelete,
        Action onPageSetup,
        Action onExportPdf,
        Action onExportExcel,
        Action onZoomIn,
        Action onZoomOut,
        Action onZoomFit,
        out Button undoBtn,
        out Button redoBtn,
        out Button cutBtn,
        out Button copyBtn,
        out Button pasteBtn,
        out Button deleteBtn)
    {
        var tb = new ToolBar
        {
            Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
            Foreground = Brushes.Black,
            Band = 0,
        };
        tb.Items.Add(MakeToolBtn(" 新建", onNew));
        tb.Items.Add(MakeToolBtn("📂 打开", onOpen));
        tb.Items.Add(MakeToolBtn("💾 保存", onSave));
        tb.Items.Add(new Separator());
        undoBtn = MakeToolBtn("↩ 撤销", onUndo);
        redoBtn = MakeToolBtn("↪ 重做", onRedo);
        tb.Items.Add(undoBtn);
        tb.Items.Add(redoBtn);
        tb.Items.Add(new Separator());
        cutBtn = MakeToolBtn("✂ 剪切", onCut);
        copyBtn = MakeToolBtn("📋 复制", onCopy);
        pasteBtn = MakeToolBtn("📌 粘贴", onPaste);
        deleteBtn = MakeToolBtn("🗑 删除", onDelete);
        tb.Items.Add(cutBtn);
        tb.Items.Add(copyBtn);
        tb.Items.Add(pasteBtn);
        tb.Items.Add(deleteBtn);
        tb.Items.Add(new Separator());
        tb.Items.Add(MakeToolBtn("🔍 页面设置", onPageSetup));
        tb.Items.Add(new Separator());
        tb.Items.Add(MakeToolBtn("📑 PDF", onExportPdf));
        tb.Items.Add(MakeToolBtn("📊 Excel", onExportExcel));
        tb.Items.Add(new Separator());
        tb.Items.Add(MakeToolBtn("🔍+ 放大", onZoomIn));
        tb.Items.Add(MakeToolBtn("🔍- 缩小", onZoomOut));
        tb.Items.Add(MakeToolBtn("🔍 适合", onZoomFit));
        return tb;
    }
}
