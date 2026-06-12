using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using ReportEngine.Core;
using ReportEngine.Core.Parsing;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 模板文件保存 - 把 SaveTemplate 从 MainWindow 抽离。
/// 等价抽离自 MainWindow.SaveTemplate() (28 行)。
///
/// 行为:
///   1. saveAs=true 或 _currentFilePath 为空时弹 SaveFileDialog
///   2. _template.ModifiedAt = DateTime.Now
///   3. _parser.Serialize(_template) -> File.WriteAllText(path)
///   4. 成功后: 触发 onSaved (更新 _currentFilePath / _dirty / UpdateTitle / ClearAutoSave / _statusText)
///   5. 失败: MessageBox 错误提示
/// </summary>
internal static class TemplateFileSaver
{
    public static void Save(
        ReportTemplate? template,
        TemplateParser parser,
        string? currentFilePath,
        bool saveAs,
        Action<string> onSaved)
    {
        if (template == null) return;
        var path = currentFilePath;
        if (saveAs || string.IsNullOrEmpty(path))
        {
            var dlg = new SaveFileDialog { Filter = "报表模板 (*.rptx)|*.rptx", Title = "保存报表模板" };
            if (!string.IsNullOrEmpty(path)) dlg.FileName = path;
            if (dlg.ShowDialog() != true) return;
            path = dlg.FileName;
        }
        try
        {
            template.ModifiedAt = DateTime.Now;
            var json = parser.Serialize(template);
            File.WriteAllText(path!, json);
            onSaved(path!);
        }
        catch (Exception ex)
        {
            MessageBox.Show("保存失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
