using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 数据绑定应用 - 从 MainWindow.OnShowDataBindingWizardClicked callback 抽出。
/// 等价抽离自 callback 类型判断分支 (8 行)。
///
/// 行为: 根据 propChoice 把 expression 写入 TextElement.Text 或 VisibleExpression。
/// 命中返回 true。
/// </summary>
internal static class DataBindingApplier
{
    public static bool Apply(ReportElement element, string propChoice, string expression)
    {
        if (propChoice == "文本内容 (Text)" && element is TextElement txt)
        {
            txt.Text = expression;
            return true;
        }
        if (propChoice == "可见性表达式 (VisibleExpression)")
        {
            element.VisibleExpression = expression;
            return true;
        }
        return false;
    }
}
