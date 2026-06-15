using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 元素属性重置 - 从 MainWindow.ResetSelectedProperties 抽出。
/// 等价抽离自 MainWindow.ResetSelectedProperties() 重置逻辑块 (15 行)。
///
/// 行为: 将元素 BackgroundColor/Border/Rotation/Opacity 重置为默认,
///       TextElement 额外重置 Font/Alignment/CanGrow。
/// </summary>
internal static class PropertyResetter
{
    public static void Reset(ReportElement el)
    {
        el.BackgroundColor = null;
        el.Border = null;
        el.Rotation = 0;
        el.Opacity = 1.0;
        if (el is TextElement t)
        {
            t.Font.Family = "宋体";
            t.Font.Size = 9;
            t.Font.Bold = false;
            t.Font.Italic = false;
            t.Font.Color = null;
            t.Alignment = TextAlignment.Left;
            t.CanGrow = false;
        }
    }
}
