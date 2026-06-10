using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 元素 / Band 图标字符串（emoji）。
/// 从 MainWindow.cs 抽出，纯静态无副作用。
/// </summary>
internal static class ElementIcons
{
    public static string BandIcon(BandType t)
    {
        switch (t)
        {
            case BandType.Header: return "📃";
            case BandType.Footer: return "📃";
            case BandType.Detail: return "📊";
            case BandType.ReportHeader: return "📘";
            case BandType.ReportFooter: return "📘";
            case BandType.GroupHeader: return "📁";
            case BandType.GroupFooter: return "📁";
            default: return "□";
        }
    }

    public static string ElementIcon(ReportElement el)
    {
        switch (el)
        {
            case TextElement _: return "🅰";
            case LineElement _: return "—";
            case ImageElement _: return "🖼";
            case ShapeElement _: return "□";
            case SubReportElement _: return "📎";
            case BarcodeElement _: return "▓";
            case TableElement _: return "≡";
            case CrossTabElement _: return "⊞";
            default: return "●";
        }
    }
}