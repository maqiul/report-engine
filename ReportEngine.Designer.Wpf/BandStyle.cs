using System.Windows.Media;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// Band 的视觉样式表：背景色 + 中文名称。
/// 从 MainWindow 抽出，纯静态无副作用。
/// </summary>
internal static class BandStyle
{
    /// <summary>BandType → 中文显示名</summary>
    public static string Name(BandType t) => t switch
    {
        BandType.Header => "页眉",
        BandType.Footer => "页脚",
        BandType.Detail => "明细",
        BandType.ReportHeader => "报表头",
        BandType.ReportFooter => "报表尾",
        BandType.GroupHeader => "分组头",
        BandType.GroupFooter => "分组尾",
        _ => t.ToString(),
    };

    /// <summary>BandType → 设计态背景色（含透明度）</summary>
    public static Brush GetBrush(BandType type) => type switch
    {
        BandType.ReportHeader => new SolidColorBrush(Color.FromArgb(40, 70, 130, 180)),
        BandType.ReportFooter => new SolidColorBrush(Color.FromArgb(40, 100, 149, 237)),
        BandType.Header => new SolidColorBrush(Color.FromArgb(40, 60, 179, 113)),
        BandType.Footer => new SolidColorBrush(Color.FromArgb(40, 144, 238, 144)),
        BandType.Detail => new SolidColorBrush(Color.FromArgb(20, 200, 200, 200)),
        BandType.GroupHeader => new SolidColorBrush(Color.FromArgb(40, 255, 165, 0)),
        BandType.GroupFooter => new SolidColorBrush(Color.FromArgb(40, 255, 215, 0)),
        _ => Brushes.Transparent,
    };
}