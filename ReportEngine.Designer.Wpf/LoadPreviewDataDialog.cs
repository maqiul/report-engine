using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Win32;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 加载预览数据对话框 - 选择 JSON 文件并解析为扁平 key-value 字典。
/// 等价抽离自 MainWindow.LoadPreviewData() + 内部 Parse() 助手。
///
/// 副作用通过回调传出:
///   - onLoaded: (parsedDict, fileName) → 调用方更新 _previewData / 状态栏 / 触发重渲染
///   - onError:  (message)             → 调用方显示错误对话框
/// </summary>
public static class LoadPreviewDataDialog
{
    /// <summary>显示文件选择对话框并解析 JSON。返回是否成功加载。</summary>
    public static bool Show(
        Window owner,
        Action<Dictionary<string, object>?, string> onLoaded,
        Action<string>? onError = null)
    {
        var dlg = new OpenFileDialog
        {
            Filter = "JSON文件|*.json|所有文件|*.*",
            Title = "加载预览数据",
        };

        if (dlg.ShowDialog(owner) != true) return false;

        try
        {
            var text = File.ReadAllText(dlg.FileName);
            var parsed = ParseSimpleJson(text);
            onLoaded(parsed, System.IO.Path.GetFileName(dlg.FileName));
            return true;
        }
        catch (Exception ex)
        {
            if (onError != null)
                onError(ex.Message);
            else
                MessageBox.Show(owner, "加载失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    /// <summary>
    /// 浅层平铺 key-value JSON 解析, 兼容 net462 (无 Newtonsoft.Json)。
    /// 支持 {"key":"value", "key2":123, ...} 顶层对象。
    /// </summary>
    public static Dictionary<string, object> ParseSimpleJson(string text)
    {
        var dict = new Dictionary<string, object>();

        // 去除首尾空白 + 去掉外层 {...}
        var trimmed = text.Trim();
        if (trimmed.StartsWith("{") && trimmed.EndsWith("}"))
            trimmed = trimmed.Substring(1, trimmed.Length - 2);

        // 简单分词: 按 , 分割, 但尊重字符串内的 ,
        var parts = SplitTopLevelCommas(trimmed);
        foreach (var part in parts)
        {
            var colonIdx = part.IndexOf(':');
            if (colonIdx < 0) continue;
            var key = part.Substring(0, colonIdx).Trim().Trim('"');
            var val = part.Substring(colonIdx + 1).Trim();
            dict[key] = Unquote(val);
        }

        return dict;
    }

    private static List<string> SplitTopLevelCommas(string input)
    {
        var result = new List<string>();
        bool inString = false;
        bool escape = false;
        int start = 0;

        for (int i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (escape) { escape = false; continue; }
            if (c == '\\') { escape = true; continue; }
            if (c == '"') { inString = !inString; continue; }
            if (c == ',' && !inString)
            {
                result.Add(input.Substring(start, i - start));
                start = i + 1;
            }
        }
        result.Add(input.Substring(start));
        return result;
    }

    private static object Unquote(string raw)
    {
        raw = raw.Trim();
        if (raw.StartsWith("\"") && raw.EndsWith("\""))
            return raw.Substring(1, raw.Length - 2);
        if (long.TryParse(raw, out var l)) return l;
        if (double.TryParse(raw, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d)) return d;
        if (raw == "true") return true;
        if (raw == "false") return false;
        if (raw == "null") return null!;
        return raw;
    }
}