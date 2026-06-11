using ReportEngine.Core;
using static ReportEngine.Designer.Wpf.EnumCnMap;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 元素树 label 构造 - 优先用 Name, 否则按元素类型取中文标签 (TextElement 用 BoxType+GetTextElLabel)。
/// 等价抽离自 MainWindow.UpdateBandTree() 内部元素 label switch 块 (12 行)。
/// </summary>
public static class ElementTreeLabelBuilder
{
    /// <summary>
    /// 返回元素在树节点中的中文 label。
    /// 优先返回 Name (用户自定义名称)，否则按类型返回中文标签。
    /// </summary>
    public static string BuildLabel(ReportElement el)
    {
        if (!string.IsNullOrEmpty(el.Name)) return el.Name!;
        return el switch
        {
            TextElement te => BoxTypeToCN(te.BoxType) + ": " + GetTextElLabel(te),
            LineElement _ => "线段",
            ImageElement _ => "图象框",
            ShapeElement _ => "图形框",
            SubReportElement _ => "子报表",
            BarcodeElement _ => "条码",
            TableElement _ => "表格",
            CrossTabElement _ => "交叉表",
            _ => el.GetType().Name,
        };
    }
}