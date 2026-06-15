using System.Collections.Generic;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// Band 删除辅助 - 从 MainWindow.DeleteBand 抽出。
/// 等价抽离自 MainWindow.DeleteBand() 选中状态清理 (4 行)。
///
/// 行为: 重置 _selectedBand/_selectedElement/_selectedElements 为空。
/// </summary>
internal static class BandDeleter
{
    public static void ClearSelection(
        ref Band? selectedBand,
        ref ReportElement? selectedElement,
        List<ReportElement> selectedElements)
    {
        selectedBand = null;
        selectedElement = null;
        selectedElements.Clear();
    }
}
