using System;
using System.IO;
using System.Windows;
using ReportEngine.Core;
using ReportEngine.Core.Parsing;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 自动保存草稿恢复检查 - 把 CheckAutoSaveRecovery 从 MainWindow 抽离。
/// 等价抽离自 MainWindow.CheckAutoSaveRecovery() (33 行)。
///
/// 行为:
///   1. AutoSavePath 不存在则 return
///   2. 弹 MessageBox (是/否) 询问是否恢复
///   3. 是: parser.Parse(json) -> 触发 onRecovered 写回 MainWindow 状态
///   4. 否: 删草稿文件 + 触发 onSkipped
///   5. 失败: MessageBox 错误提示
/// </summary>
internal static class AutoSaveRecoveryChecker
{
    public static void Check(
        string autoSavePath,
        TemplateParser parser,
        Action<ReportTemplate> onRecovered,
        Action onSkipped)
    {
        if (!File.Exists(autoSavePath)) return;
        var result = MessageBox.Show("发现未保存的草稿文件，是否恢复？\n(" + autoSavePath + ")", "恢复草稿", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var json = File.ReadAllText(autoSavePath);
                var template = parser.Parse(json);
                onRecovered(template);
            }
            catch (Exception ex)
            {
                MessageBox.Show("恢复草稿失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        else
        {
            // 用户选择不恢复，删除草稿文件
            try
            {
                File.Delete(autoSavePath);
                onSkipped();
            }
            catch { }
        }
    }
}
