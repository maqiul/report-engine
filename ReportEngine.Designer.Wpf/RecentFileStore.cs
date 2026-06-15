using System;
using System.Collections.Generic;
using System.IO;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 最近文件存储 - 从 MainWindow.AddRecentFile + LoadRecentFiles 抽出。
/// 等价抽离自 AddRecentFile() 列表去重 + 头插 + 截断 + 持久化 (5 行)。
///
/// 行为: 移除 path 旧条目 → 头插 → 截断到 10 → 写 RecentFilesPath。
/// </summary>
internal static class RecentFileStore
{
    public const int MaxCount = 10;

    public static void Add(List<string> recentFiles, string path, string persistPath)
    {
        recentFiles.Remove(path);
        recentFiles.Insert(0, path);
        if (recentFiles.Count > MaxCount) recentFiles.RemoveAt(MaxCount);
        try { File.WriteAllLines(persistPath, recentFiles); } catch { }
    }
}
