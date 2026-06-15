using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 最近文件读取 - 从 MainWindow.LoadRecentFiles 抽出。
/// 等价抽离自 LoadRecentFiles() 读取 + 过滤 + 截断 (5 行)。
///
/// 行为: 读 RecentFilesPath → 过滤空行 → 取前 maxCount 加入 recentFiles。
/// 异常被吞。
/// </summary>
internal static class RecentFileLoader
{
    public static void Load(List<string> recentFiles, string persistPath, int maxCount)
    {
        try
        {
            if (File.Exists(persistPath))
                recentFiles.AddRange(File.ReadAllLines(persistPath).Where(l => !string.IsNullOrWhiteSpace(l)).Take(maxCount));
        }
        catch { }
    }
}
