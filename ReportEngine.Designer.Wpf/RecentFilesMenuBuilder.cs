using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// "最近文件" 菜单构造 - 把 BuildRecentFilesMenu 从 MainWindow 抽离。
/// 等价抽离自 MainWindow.BuildRecentFilesMenu() (14 行)。
///
/// 接收 recentFiles 列表 (外部读) + onFileOpen callback (点击文件项触发打开)。
/// </summary>
internal static class RecentFilesMenuBuilder
{
    public static void Build(MenuItem parent, IReadOnlyList<string> recentFiles, Action<string> onFileOpen)
    {
        if (recentFiles == null || recentFiles.Count == 0)
        {
            var empty = new MenuItem { Header = "(无最近文件)", IsEnabled = false };
            parent.Items.Add(empty);
            return;
        }
        for (int i = 0; i < recentFiles.Count; i++)
        {
            var fp = recentFiles[i];
            var mi = new MenuItem { Header = (i + 1) + ". " + Path.GetFileName(fp) };
            mi.Click += (_, __) => onFileOpen(fp);
            parent.Items.Add(mi);
        }
    }
}