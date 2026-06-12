using System;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// Canvas 右键菜单总构造 - 把 OnCanvasRightClick 中所有菜单项打包到一个 builder。
/// 等价抽离自 MainWindow.OnCanvasRightClick() 菜单构造 (33 行)。
///
/// 菜单结构 (从顶到底):
///   1. 剪切 / 复制 / 删除 (当选中元素时)
///   2. --- 粘贴 (有剪贴板时)
///   3. --- 插入元素子菜单
///   4. --- 插入区域子菜单
///   5. 删除区域 (选中 band 时)
///   6. --- 页面设置
///
/// 所有副作用 (CutSelected/CopySelected 等) 通过 callback 传出, MainWindow 用 lambda 包裹 owner 字段访问。
/// </summary>
internal static class RightClickMenuBuilder
{
    public static System.Windows.Controls.ContextMenu Build(
        ReportElement? selectedElement,
        bool hasClipboard,
        Band? selectedBand,
        string? bandName,
        Action onCut,
        Action onCopy,
        Action onDelete,
        Action onPaste,
        Action<ReportElement> onInsert,
        Func<ReportElement> staticText,
        Func<ReportElement> field,
        Func<ReportElement> summary,
        Func<ReportElement> sysVar,
        Func<ReportElement> line,
        Func<ReportElement> shape,
        Func<ReportElement> image,
        Func<ReportElement> barcode,
        Func<ReportElement> table,
        Func<ReportElement> crossTab,
        Func<ReportElement> chart,
        Func<ReportElement> subReport,
        Action<BandType, double> onAddBand,
        Action<Band> onDeleteBand,
        Action onPageSetup)
    {
        var menu = new System.Windows.Controls.ContextMenu();
        if (selectedElement != null)
        {
            var miCut = Make("剪切", onCut); menu.Items.Add(miCut);
            var miCopy = Make("复制", onCopy); menu.Items.Add(miCopy);
            var miDel = Make("删除", onDelete); menu.Items.Add(miDel);
            menu.Items.Add(new System.Windows.Controls.Separator());
        }
        if (hasClipboard)
        {
            var miPaste = Make("粘贴", onPaste); menu.Items.Add(miPaste);
        }
        menu.Items.Add(new System.Windows.Controls.Separator());
        menu.Items.Add(InsertElementMenuBuilder.Build(
            onInsert, staticText, field, summary, sysVar, line, shape, image,
            barcode, table, crossTab, chart, subReport));
        menu.Items.Add(InsertBandMenuBuilder.Build(onAddBand));
        PageMenuBuilder.AppendTo(menu, selectedBand, bandName, onDeleteBand, onPageSetup);
        return menu;
    }

    private static System.Windows.Controls.MenuItem Make(string header, Action onClick)
    {
        var mi = new System.Windows.Controls.MenuItem { Header = header };
        mi.Click += (_, __) => onClick();
        return mi;
    }
}
