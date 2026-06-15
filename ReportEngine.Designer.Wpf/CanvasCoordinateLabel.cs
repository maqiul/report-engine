using System.Windows;
using System.Windows.Controls;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 画布鼠标坐标标签 - 从 MainWindow.OnCanvasMouseMove 抽出。
/// 等价抽离自 OnCanvasMouseMove() 状态栏坐标更新 (4 行)。
///
/// 行为: 将像素坐标 pos 转为 mm, 写入 label.Text = "X: ?mm  Y: ?mm"。
/// </summary>
internal static class CanvasCoordinateLabel
{
    public static void Update(
        TextBlock label,
        Point pos,
        double zoom,
        double pixelsPerMm,
        double canvasPadding)
    {
        double mmPx = pixelsPerMm * zoom;
        double mmX = (pos.X - canvasPadding) / mmPx;
        double mmY = (pos.Y - canvasPadding) / mmPx;
        label.Text = $"X: {mmX:F1}mm  Y: {mmY:F1}mm";
    }
}
