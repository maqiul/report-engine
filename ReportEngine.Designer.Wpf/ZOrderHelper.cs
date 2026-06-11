using System.Collections.Generic;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 元素 Z-order 调整算法 - 4 种模式 (front/back/up/down)。
/// 等价抽离自 MainWindow.MoveElementOrder() (25 行)。
///
/// 算法语义: list 索引越大 = 越上层绘制。
/// front = 移到末尾 (最上层), back = 移到首位 (最底层),
/// up = 与上一个交换 (更上层), down = 与下一个交换 (更下层)。
/// </summary>
public static class ZOrderHelper
{
    public const string ModeFront = "front";
    public const string ModeBack = "back";
    public const string ModeUp = "up";
    public const string ModeDown = "down";

    /// <summary>
    /// 调整 element 在 list 中的位置。
    /// 返回 true 表示移动了, false 表示未移动 (元素不在列表中或模式无效或越界)。
    /// </summary>
    public static bool Move(IList<ReportElement> list, ReportElement element, string direction)
    {
        if (list == null || element == null) return false;
        int idx = list.IndexOf(element);
        if (idx < 0) return false;
        switch (direction)
        {
            case ModeFront:
                list.Remove(element);
                list.Add(element);
                return true;
            case ModeBack:
                list.Remove(element);
                list.Insert(0, element);
                return true;
            case ModeUp:
                if (idx < list.Count - 1)
                {
                    list.Remove(element);
                    list.Insert(idx + 1, element);
                    return true;
                }
                return false;
            case ModeDown:
                if (idx > 0)
                {
                    list.Remove(element);
                    list.Insert(idx - 1, element);
                    return true;
                }
                return false;
            default:
                return false;
        }
    }
}