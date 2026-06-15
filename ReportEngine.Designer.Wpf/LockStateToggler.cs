using System.Collections.Generic;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 锁定状态切换 - 从 MainWindow.ToggleLockSelected 抽出。
/// 等价抽离自 MainWindow.ToggleLockSelected() 锁定循环 (2 行)。
///
/// 行为: 取第一个的 Locked 取反 → 全部应用 → 返回 newState。
/// </summary>
internal static class LockStateToggler
{
    public static bool Toggle(IList<ReportElement> targets)
    {
        bool newState = !targets[0].Locked;
        foreach (var el in targets) el.Locked = newState;
        return newState;
    }
}
