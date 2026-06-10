using System;
using System.Windows.Media;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 字符串颜色 → WPF Brush 解析器。设计器全局共用。
/// 仅支持 #RRGGBB / #AARRGGBB 格式（与 MainWindow 原 ParseBrush 行为一致）。
/// 解析失败返回 fallback。
/// </summary>
internal static class BrushParser
{
    public static Brush Parse(string? color, Brush fallback)
    {
        if (string.IsNullOrEmpty(color)) return fallback;
        var c = color!;
        try
        {
            if (c[0] == '#' && c.Length == 7)
            {
                byte r = Convert.ToByte(c.Substring(1, 2), 16);
                byte g = Convert.ToByte(c.Substring(3, 2), 16);
                byte b = Convert.ToByte(c.Substring(5, 2), 16);
                return new SolidColorBrush(Color.FromRgb(r, g, b));
            }
            if (c[0] == '#' && c.Length == 9)
            {
                byte a = Convert.ToByte(c.Substring(1, 2), 16);
                byte r = Convert.ToByte(c.Substring(3, 2), 16);
                byte g = Convert.ToByte(c.Substring(5, 2), 16);
                byte b = Convert.ToByte(c.Substring(7, 2), 16);
                return new SolidColorBrush(Color.FromArgb(a, r, g, b));
            }
        }
        catch { }
        return fallback;
    }
}