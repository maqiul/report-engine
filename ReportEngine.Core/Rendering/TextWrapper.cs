using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReportEngine.Core.Rendering;

/// <summary>
/// 长文本自动换行（三端一致规范 v1）
///
/// 规范（.NET / Java / 前端必须完全一致，否则跨端一致性校验会失败）：
///   1. fontMm = fontSize(pt) * 25.4 / 72
///   2. 字符宽度（em）：CJK / 全角标点 = 1.0，其余 = 0.5
///   3. 行容量（em）= widthMm / fontMm（最小 1）
///   4. 逐字符累加，超出容量则换行（行首不额外留空格）
///   5. 行高 lineHeightMm = fontMm * 1.2
///   6. 显式 \r\n / \r / \n 先按行拆分，再各自软换行
///   7. maxLines &gt; 0 时截断到 maxLines 行，末行末尾用 "…" 结尾
/// </summary>
public static class TextWrapper
{
    /// <summary>行距系数（相对字号）</summary>
    public const double LineHeightFactor = 1.2;

    /// <summary>截断标记</summary>
    public const string Ellipsis = "…";

    /// <summary>字号(pt) → 毫米</summary>
    public static double FontMm(double fontSizePt)
    {
        double mm = fontSizePt * 25.4 / 72.0;
        return mm > 0 ? mm : 1.0;
    }

    /// <summary>行高（mm）</summary>
    public static double LineHeightMm(double fontSizePt)
    {
        return FontMm(fontSizePt) * LineHeightFactor;
    }

    /// <summary>自动换行，返回文本行集合</summary>
    public static List<string> Wrap(string? text, double widthMm, double fontSizePt, int maxLines = 0)
    {
        var lines = new List<string>();
        if (string.IsNullOrEmpty(text))
        {
            lines.Add(string.Empty);
            return lines;
        }

        double fontMm = FontMm(fontSizePt);
        double capacity = widthMm / fontMm;
        if (capacity <= 0) capacity = 1;

        string normalized = text!.Replace("\r\n", "\n").Replace('\r', '\n');
        foreach (var rawLine in normalized.Split('\n'))
        {
            var cur = new StringBuilder();
            double curWidth = 0;
            foreach (char ch in rawLine)
            {
                double w = IsFullWidth(ch) ? 1.0 : 0.5;
                if (curWidth + w > capacity && cur.Length > 0)
                {
                    lines.Add(cur.ToString());
                    cur.Clear();
                    curWidth = 0;
                }
                cur.Append(ch);
                curWidth += w;
            }
            lines.Add(cur.ToString());
        }

        if (maxLines > 0 && lines.Count > maxLines)
        {
            lines = lines.Take(maxLines).ToList();
            if (lines.Count > 0)
            {
                string last = lines[lines.Count - 1];
                lines[lines.Count - 1] = last.Length > 0
                    ? last.Substring(0, last.Length - 1) + Ellipsis
                    : Ellipsis;
            }
        }

        return lines;
    }

    /// <summary>自动换行并按 canGrow 撑高：返回 (行集合, 行高mm, 撑高后高度mm)</summary>
    public static (List<string> Lines, double LineHeight, double GrownHeight) WrapAndGrow(
        string? text, double widthMm, double heightMm, double fontSizePt, bool canGrow, int maxLines = 0)
    {
        var lines = Wrap(text, widthMm, fontSizePt, maxLines);
        double lh = LineHeightMm(fontSizePt);
        double grown = canGrow ? lines.Count * lh : 0;
        return (lines, lh, grown);
    }

    /// <summary>是否全角字符（CJK / 全角标点）</summary>
    public static bool IsFullWidth(char ch)
    {
        return ch >= 0x1100 && (
            ch <= 0x115F ||
            ch == 0x2329 || ch == 0x232A ||
            (ch >= 0x2E80 && ch <= 0xA4CF) ||
            (ch >= 0xAC00 && ch <= 0xD7A3) ||
            (ch >= 0xF900 && ch <= 0xFAFF) ||
            (ch >= 0xFE10 && ch <= 0xFE19) ||
            (ch >= 0xFE30 && ch <= 0xFE6F) ||
            (ch >= 0xFF00 && ch <= 0xFF60) ||
            (ch >= 0xFFE0 && ch <= 0xFFE6) ||
            (ch >= 0x3000 && ch <= 0x303E));
    }
}
