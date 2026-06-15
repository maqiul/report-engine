using System.IO;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 文件删除 - 从 MainWindow.ClearAutoSave 抽出。
/// 等价抽离自 ClearAutoSave() 存在性检查 + try-catch 删除 (5 行)。
///
/// 行为: 若文件存在则安全删除 (异常被吞)。不存在则无操作。
/// </summary>
internal static class FileDeleter
{
    public static void SafeDelete(string path)
    {
        if (File.Exists(path))
        {
            try { File.Delete(path); } catch { }
        }
    }
}
