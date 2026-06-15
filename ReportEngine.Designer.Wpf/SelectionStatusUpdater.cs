using System.Windows.Controls;
using ReportEngine.Core;
using static ReportEngine.Designer.Wpf.EnumCnMap;
using static ReportEngine.Designer.Wpf.BandStyle;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 选中状态标签同步 - 从 MainWindow.UpdatePropertyListCore 抽出。
/// 等价抽离自 UpdatePropertyListCore() 选中标签 + 字体同步 (12 行)。
///
/// 行为:
///   - 更新 _selectedObjLabel.Text (ElementTypeName/Name/页面)
///   - 若 TextElement: 同步 _fontFamilyCombo.Text + _fontSizeCombo.Text
/// </summary>
internal static class SelectionStatusUpdater
{
    public static void Sync(
        TextBlock selectedObjLabel,
        ComboBox fontFamilyCombo,
        ComboBox fontSizeCombo,
        ReportElement? selectedElement,
        Band? selectedBand)
    {
        if (selectedElement != null)
            selectedObjLabel.Text = ElementTypeName(selectedElement);
        else if (selectedBand != null)
            selectedObjLabel.Text = Name(selectedBand.Type);
        else
            selectedObjLabel.Text = "页面";

        if (selectedElement is TextElement syncT)
        {
            fontFamilyCombo.Text = syncT.Font.Family;
            fontSizeCombo.Text = syncT.Font.Size.ToString();
        }
    }
}
