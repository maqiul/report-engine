using System;
using System.Collections.Generic;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 预览数据 JSON 解析器。从 MainWindow.ParseSimpleJson 抽出。
/// 支持 JSON 对象 { "key": "value", ... } 或包含一个对象的 JSON 数组。
/// 解析后返回 Dictionary&lt;string, object&gt;（key 大小写不敏感）。
/// 内部用 Newtonsoft.Json 解析（TemplateParser 已依赖）。
/// 解析失败返回 null，不抛异常。
/// </summary>
internal static class PreviewJsonParser
{
    public static Dictionary<string, object>? Parse(string json)
    {
        json = json.Trim();
        // 如果是数组，取第一个对象
        if (json.StartsWith("["))
        {
            int depth = 0;
            int start = -1;
            for (int i = 0; i < json.Length; i++)
            {
                if (json[i] == '{') { if (depth == 0) start = i; depth++; }
                else if (json[i] == '}') { depth--; if (depth == 0 && start >= 0) { json = json.Substring(start, i - start + 1); break; } }
            }
        }
        if (!json.StartsWith("{")) return null;
        var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        try
        {
            // 借助 Newtonsoft.Json 解析
            var dict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            if (dict != null)
                foreach (var kv in dict)
                    result[kv.Key] = kv.Value ?? "";
        }
        catch { }
        return result;
    }
}