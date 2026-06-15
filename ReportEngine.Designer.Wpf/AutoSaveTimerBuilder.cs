using System;
using System.Windows.Threading;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 自动保存定时器构造 - 从 MainWindow.SetupAutoSave 抽出。
/// 等价抽离自 MainWindow.SetupAutoSave() DispatcherTimer 配置 (15 行)。
///
/// 行为: 启动一个按 interval 触发的 DispatcherTimer,
///       每次 tick 调用 onTick callback。
/// </summary>
internal static class AutoSaveTimerBuilder
{
    public static DispatcherTimer Build(TimeSpan interval, EventHandler tick)
    {
        var timer = new DispatcherTimer { Interval = interval };
        timer.Tick += tick;
        timer.Start();
        return timer;
    }
}
