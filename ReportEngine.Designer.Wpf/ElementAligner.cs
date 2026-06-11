using System.Collections.Generic;
using System.Linq;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 元素对齐/分布/等宽等高算法 - 8 种模式 (left/right/top/bottom/hcenter/vcenter/samewidth/sameheight)。
/// 等价抽离自 MainWindow.AlignElements() (45 行)。
/// </summary>
public static class ElementAligner
{
    /// <summary>对齐模式常量</summary>
    public const string ModeLeft = "left";
    public const string ModeRight = "right";
    public const string ModeTop = "top";
    public const string ModeBottom = "bottom";
    public const string ModeHCenter = "hcenter";
    public const string ModeVCenter = "vcenter";
    public const string ModeSameWidth = "samewidth";
    public const string ModeSameHeight = "sameheight";

    /// <summary>
    /// 对 targets 元素按 mode 对齐/分布/等宽等高。
    /// 返回 true 表示应用了变更，false 表示模式无效。
    /// </summary>
    public static bool Align(IList<ReportElement> targets, string mode)
    {
        if (targets.Count < 2) return false;
        var first = targets[0];
        switch (mode)
        {
            case ModeLeft:
                {
                    double minX = targets.Min(e => e.X);
                    foreach (var el in targets) el.X = minX;
                    return true;
                }
            case ModeRight:
                {
                    double maxR = targets.Max(e => e.X + e.Width);
                    foreach (var el in targets) el.X = maxR - el.Width;
                    return true;
                }
            case ModeTop:
                {
                    double minY = targets.Min(e => e.Y);
                    foreach (var el in targets) el.Y = minY;
                    return true;
                }
            case ModeBottom:
                {
                    double maxB = targets.Max(e => e.Y + e.Height);
                    foreach (var el in targets) el.Y = maxB - el.Height;
                    return true;
                }
            case ModeHCenter:
                {
                    double avgCX = targets.Average(e => e.X + e.Width / 2);
                    foreach (var el in targets) el.X = avgCX - el.Width / 2;
                    return true;
                }
            case ModeVCenter:
                {
                    double avgCY = targets.Average(e => e.Y + e.Height / 2);
                    foreach (var el in targets) el.Y = avgCY - el.Height / 2;
                    return true;
                }
            case ModeSameWidth:
                {
                    double w = first.Width;
                    foreach (var el in targets) el.Width = w;
                    return true;
                }
            case ModeSameHeight:
                {
                    double h = first.Height;
                    foreach (var el in targets) el.Height = h;
                    return true;
                }
            default:
                return false;
        }
    }
}