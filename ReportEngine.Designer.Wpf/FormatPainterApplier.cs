using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 格式刷应用算法 - 把 source 的格式 (Border + BackgroundColor + TextElement Font/Alignment) 复制到 target。
/// 等价抽离自 MainWindow.ApplyFormatToTarget() (25 行)。
/// </summary>
public static class FormatPainterApplier
{
    /// <summary>
    /// 把 source 元素的格式复制到 target。Border 深拷贝 (避免共享引用)，BackgroundColor 字符串直接赋值，
    /// TextElement 之间复制 Font 字段 + Alignment。
    /// </summary>
    public static void Apply(ReportElement source, ReportElement target)
    {
        if (source == null || target == null) return;

        // 复制边框
        target.Border = source.Border != null
            ? new BorderDef
            {
                Width = source.Border.Width,
                Color = source.Border.Color,
                Style = source.Border.Style,
                Top = source.Border.Top,
                Bottom = source.Border.Bottom,
                Left = source.Border.Left,
                Right = source.Border.Right,
            }
            : null;

        // 复制背景色
        target.BackgroundColor = source.BackgroundColor;

        // 复制 TextElement 样式
        if (source is TextElement st && target is TextElement tt)
        {
            tt.Font.Family = st.Font.Family;
            tt.Font.Size = st.Font.Size;
            tt.Font.Bold = st.Font.Bold;
            tt.Font.Italic = st.Font.Italic;
            tt.Font.Color = st.Font.Color;
            tt.Alignment = st.Alignment;
        }
    }
}