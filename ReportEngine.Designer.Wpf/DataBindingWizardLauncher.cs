using System.Windows;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 数据绑定向导启动器 - 从 MainWindow.OnShowDataBindingWizardClicked 抽出整块。
/// 等价抽离自 OnShowDataBindingWizardClicked() 21 行（2 个校验 + 8 行 callback）。
///
/// 行为: template==null || element==null → "请先选中元素" status + return
///       template.DataSources 空 → "请先添加数据源" status + return
///       弹 DataBindingWizardDialog，confirm 后 onCommit(expression)。
/// </summary>
internal static class DataBindingWizardLauncher
{
    public static bool Launch(Window owner, ReportTemplate? template, ReportElement? element,
        System.Action<string> setStatus,
        System.Action<string, string> onCommit)
    {
        if (template == null || element == null)
        {
            setStatus("请先选中一个元素再打开数据绑定向导");
            return false;
        }
        if (template.DataSources.Count == 0)
        {
            setStatus("请先添加数据源（文件→数据源）");
            return false;
        }
        DataBindingWizardDialog.Show(owner, template, element,
            (dsName, fieldName, propChoice, expression) => onCommit(propChoice, expression));
        return true;
    }
}
