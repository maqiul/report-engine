using System.Windows.Input;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 方向键位移步进计算 - 把 OnPreviewKeyDown 中的方向键部分抽离。
/// 等价抽离自 MainWindow.OnPreviewKeyDown() 方向键 switch (8 行)。
///
/// Shift=5mm, 否则 0.5mm。
/// 返回值: null 表示非方向键, 应交给原逻辑处理; (dx, dy) 表示位移向量。
/// </summary>
internal static class KeyNudgeCalculator
{
    public static (double dx, double dy)? TryGetDelta(Key key, ModifierKeys modifiers)
    {
        double step = modifiers == ModifierKeys.Shift ? 5 : 0.5;
        return key switch
        {
            Key.Left => (-step, 0),
            Key.Right => (step, 0),
            Key.Up => (0, -step),
            Key.Down => (0, step),
            _ => null,
        };
    }
}
