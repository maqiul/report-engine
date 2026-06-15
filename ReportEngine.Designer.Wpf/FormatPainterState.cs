using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 格式刷状态 - 从 MainWindow.StopFormatPainter 抽出。
/// 等价抽离自 StopFormatPainter() 状态重置 (2 行)。
///
/// 行为: 重置 active=false, source=null。
/// </summary>
internal static class FormatPainterState
{
    public static void Reset(ref bool active, ref ReportElement? source)
    {
        active = false;
        source = null;
    }
}
