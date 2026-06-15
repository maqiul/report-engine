using System.Linq;
using System.Windows.Controls;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// Band 树选中提交 - 从 MainWindow.OnBandTreeSelectionChanged 抽出选中项 dispatch + Render + UpdatePropertyList。
/// 等价抽离自 OnBandTreeSelectionChanged() 14 行体。
///
/// 行为: TreeViewItem.Tag is Band → 写 band + 清 element
///       TreeViewItem.Tag is ReportElement → 写 element + 反查所在 band
///       之后调用 onRender + onUpdateProps。
/// </summary>
internal static class BandTreeSelectionCommitter
{
    public static void Commit(object? item, ReportTemplate? template,
        ref Band? selectedBand, ref ReportElement? selectedElement,
        System.Action onRender, System.Action onUpdateProps)
    {
        if (item is Band band)
        {
            selectedBand = band;
            selectedElement = null;
        }
        else if (item is ReportElement el)
        {
            selectedElement = el;
            if (template != null)
                selectedBand = template.Bands.FirstOrDefault(b => b.Elements.Contains(el));
        }
        else return;
        onRender();
        onUpdateProps();
    }
}
