using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using ReportEngine.Core;
using ReportEngine.Core.Parsing;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 模板文件打开 - 把 OpenTemplate 从 MainWindow 抽离。
/// 等价抽离自 MainWindow.OpenTemplate() (27 行)。
///
/// 行为:
///   1. onConfirmDiscard() 检查未保存变更, 用户取消则 return
///   2. 弹 OpenFileDialog (filter: 报表模板/所有文件)
///   3. 读文件 + parser.Parse(json) + 触发 onOpened (写回 MainWindow 状态)
///   4. 失败: MessageBox 错误提示
/// </summary>
internal static class TemplateFileOpener
{
    public static void Open(
        TemplateParser parser,
        Func<bool> onConfirmDiscard,
        Action<string, ReportTemplate> onOpened)
    {
        if (!onConfirmDiscard()) return;
        var dlg = new OpenFileDialog { Filter = "报表模板 (*.rptx)|*.rptx|所有文件|*.*", Title = "打开报表模板" };
        if (dlg.ShowDialog() != true) return;
        try
        {
            var json = File.ReadAllText(dlg.FileName);
            var template = parser.Parse(json);
            onOpened(dlg.FileName, template);
        }
        catch (Exception ex)
        {
            MessageBox.Show("打开失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
