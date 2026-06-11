using System;
using System.Windows.Controls;
using static ReportEngine.Designer.Wpf.BandStyle;
using static ReportEngine.Designer.Wpf.UiFactory;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// Band 树右键菜单构造 - 把 BuildBandTreeContextMenu 从 MainWindow 抽离。
/// 等价抽离自 MainWindow.BuildBandTreeContextMenu() (19 行)。
///
/// 7 个"插入元素"子菜单项共用 onInsert callback 模式，
/// 1 个"删除区域"用 onDelete callback。
/// </summary>
internal static class BandTreeContextMenuBuilder
{
    public static ContextMenu Build(
        ReportEngine.Core.Band band,
        Action onInsertText,
        Action onInsertFieldBox,
        Action onInsertSummaryBox,
        Action onInsertSysVarBox,
        Action onInsertLine,
        Action onInsertShape,
        Action onInsertImage,
        Action onDelete)
    {
        var menu = new ContextMenu();

        var miInsert = new MenuItem { Header = "插入元素" };
        miInsert.Items.Add(MakeMenuItem("静态框", null, onInsertText));
        miInsert.Items.Add(MakeMenuItem("字段框", null, onInsertFieldBox));
        miInsert.Items.Add(MakeMenuItem("统计框", null, onInsertSummaryBox));
        miInsert.Items.Add(MakeMenuItem("系统变量框", null, onInsertSysVarBox));
        miInsert.Items.Add(new Separator());
        miInsert.Items.Add(MakeMenuItem("线段", null, onInsertLine));
        miInsert.Items.Add(MakeMenuItem("图形框", null, onInsertShape));
        miInsert.Items.Add(MakeMenuItem("图象框", null, onInsertImage));
        menu.Items.Add(miInsert);
        menu.Items.Add(new Separator());

        var miDel = new MenuItem { Header = "删除区域 [" + Name(band.Type) + "]" };
        miDel.Click += (_, __) => onDelete();
        menu.Items.Add(miDel);

        return menu;
    }
}