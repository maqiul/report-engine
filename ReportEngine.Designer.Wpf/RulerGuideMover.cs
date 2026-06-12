using System.Collections.Generic;
using System.Windows;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 标尺参考线拖动 - 从 OnRulerGuideMouseMove 抽出。
/// 等价抽离自 MainWindow.OnRulerGuideMouseMove() (22 行)。
///
/// 行为:
///   - draggingHGuide=true: 从 VRuler 读 Y → 写 hGuides
///   - draggingHGuide=false: 从 HRuler 读 X → 写 vGuides
/// 返回是否应该触发 redraw (有有效 guide 命中)。
/// </summary>
internal static class RulerGuideMover
{
    public static bool Update(
        bool draggingHGuide,
        int draggingGuideIndex,
        Point posOnHRuler,
        Point posOnVRuler,
        double zoom,
        double pixelsPerMm,
        double canvasPadding,
        double scrollViewerHorizontalOffset,
        double scrollViewerVerticalOffset,
        List<double> hGuides, List<double> vGuides)
    {
        double mmPx = pixelsPerMm * zoom;
        if (draggingHGuide)
        {
            double offsetY = -scrollViewerVerticalOffset + canvasPadding;
            double mm = System.Math.Round((posOnVRuler.Y - offsetY) / mmPx, 1);
            if (draggingGuideIndex >= 0 && draggingGuideIndex < hGuides.Count)
            {
                hGuides[draggingGuideIndex] = mm;
                return true;
            }
        }
        else
        {
            double offsetX = -scrollViewerHorizontalOffset + canvasPadding;
            double mm = System.Math.Round((posOnHRuler.X - offsetX) / mmPx, 1);
            if (draggingGuideIndex >= 0 && draggingGuideIndex < vGuides.Count)
            {
                vGuides[draggingGuideIndex] = mm;
                return true;
            }
        }
        return false;
    }
}
