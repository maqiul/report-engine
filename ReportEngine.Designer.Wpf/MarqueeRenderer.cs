using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 框选虚线矩形创建 - 从 OnCanvasMouseMove 抽出。
/// 等价抽离自 MainWindow.OnCanvasMouseMove() MarqueeSelect 分支 (9 行)。
///
/// 蓝色虚线 + 半透明蓝色填充, 尺寸 = |pos - dragStart|。
/// </summary>
internal static class MarqueeRenderer
{
    public static Rectangle Create(Point pos, Point dragStart)
    {
        double mx = System.Math.Min(pos.X, dragStart.X);
        double my = System.Math.Min(pos.Y, dragStart.Y);
        double mw = System.Math.Abs(pos.X - dragStart.X);
        double mh = System.Math.Abs(pos.Y - dragStart.Y);
        return new Rectangle
        {
            Width = mw,
            Height = mh,
            Stroke = Brushes.DodgerBlue,
            StrokeThickness = 1,
            StrokeDashArray = new DoubleCollection { 4, 2 },
            Fill = new SolidColorBrush(Color.FromArgb(30, 30, 144, 255))
        };
    }
}
