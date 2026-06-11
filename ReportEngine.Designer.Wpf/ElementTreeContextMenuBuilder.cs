using System;
using System.Windows.Controls;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 元素树右键菜单构造 - 把 BuildElementTreeContextMenu 从 MainWindow 抽离。
/// 等价抽离自 MainWindow.BuildElementTreeContextMenu() (34 行)。
///
/// 强耦合菜单项通过 callback 传出副作用 (onSelect/onCopy/onCut/onDelete/onRename/onToggleLock/onBringToFront/onSendToBack)，
/// MainWindow 用 lambda 包裹 owner 状态更新。
/// </summary>
internal static class ElementTreeContextMenuBuilder
{
    public static ContextMenu Build(
        bool isLocked,
        Action onSelect,
        Action onCopy,
        Action onCut,
        Action onDelete,
        Action onRename,
        Action onToggleLock,
        Action onBringToFront,
        Action onSendToBack)
    {
        var menu = new ContextMenu();

        var miSelect = new MenuItem { Header = "选中" };
        miSelect.Click += (_, __) => onSelect();
        menu.Items.Add(miSelect);
        menu.Items.Add(new Separator());

        var miCopy = new MenuItem { Header = "复制" };
        miCopy.Click += (_, __) => onCopy();
        menu.Items.Add(miCopy);

        var miCut = new MenuItem { Header = "剪切" };
        miCut.Click += (_, __) => onCut();
        menu.Items.Add(miCut);

        var miDel = new MenuItem { Header = "删除" };
        miDel.Click += (_, __) => onDelete();
        menu.Items.Add(miDel);
        menu.Items.Add(new Separator());

        var miRename = new MenuItem { Header = "重命名" };
        miRename.Click += (_, __) => onRename();
        menu.Items.Add(miRename);

        var miLock = new MenuItem { Header = isLocked ? "解锁" : "锁定" };
        miLock.Click += (_, __) => onToggleLock();
        menu.Items.Add(miLock);
        menu.Items.Add(new Separator());

        var miTop = new MenuItem { Header = "置于顶层" };
        miTop.Click += (_, __) => onBringToFront();
        menu.Items.Add(miTop);

        var miBottom = new MenuItem { Header = "置于底层" };
        miBottom.Click += (_, __) => onSendToBack();
        menu.Items.Add(miBottom);

        return menu;
    }
}