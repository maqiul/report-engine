using System.Collections.Generic;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 导出数据组装器：从预览数据构造 ReportRenderer 所需的多数据源格式。
/// 从 MainWindow.BuildExportData 抽出。把实例方法改为纯静态，接收 data 参数，便于单测。
/// 约定：单数据源名为 "Default"；如果输入为 null 或空，返回空字典（渲染器会处理空数据）。
/// </summary>
internal static class ExportDataBuilder
{
    public static Dictionary<string, List<Dictionary<string, object>>> Build(
        IReadOnlyDictionary<string, object>? previewData)
    {
        var result = new Dictionary<string, List<Dictionary<string, object>>>();
        if (previewData != null && previewData.Count > 0)
        {
            var row = new Dictionary<string, object>();
            foreach (var kv in previewData)
                row[kv.Key] = kv.Value;
            result["Default"] = new List<Dictionary<string, object>> { row };
        }
        return result;
    }
}