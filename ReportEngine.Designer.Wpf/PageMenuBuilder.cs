using System;
using System.Windows.Controls;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 右键菜单"删除区域 + 页面设置"子菜单 - 从 OnCanvasRightClick 抽出。
/// 等价抽离自 MainWindow.OnCanvasRightClick() 末尾菜单 (10 行)。
///
/// 行为:
///   - 选中 Band 时显示 "删除区域 [Name]" 菜单项, 点击删除
///   - 始终显示 "页面设置..." 菜单项
///   - 两项之间用 Separator 分隔
///
/// 注意: Name(BandType) 来自 using static BandStyle; 调用方需自行处理.
/// </summary>
internal static class PageMenuBuilder
{
    public static void AppendTo(ContextMenu menu, Band? selectedBand, string? bandName, Action<Band> onDeleteBand, Action onPageSetup)
    {
        if (selectedBand != null && bandName != null)
        {
            var miDelBand = new MenuItem { Header = "删除区域 [" + bandName + "]" };
            var delB = selectedBand;
            miDelBand.Click += (_, __) => onDeleteBand(delB);
            menu.Items.Add(miDelBand);
        }
        menu.Items.Add(new Separator());
        var miPage = new MenuItem { Header = "页面设置..." };
        miPage.Click += (_, __) => onPageSetup();
        menu.Items.Add(miPage);
    }
}

