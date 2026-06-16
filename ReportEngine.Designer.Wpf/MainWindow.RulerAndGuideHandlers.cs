using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using Microsoft.Win32;
using ReportEngine.Core;
using ReportEngine.Core.Parsing;
using ReportEngine.Core.Rendering;
using ReportEngine.Core.SubReports;
using ReportEngine.Export.Pdf;
using ReportEngine.Export.Excel;
using static ReportEngine.Designer.Wpf.UiFactory;
using static ReportEngine.Designer.Wpf.ElementFactory;
using static ReportEngine.Designer.Wpf.ElementIcons;
using static ReportEngine.Designer.Wpf.BandStyle;
using static ReportEngine.Designer.Wpf.PreviewJsonParser;
using static ReportEngine.Designer.Wpf.ExportDataBuilder;
using static ReportEngine.Designer.Wpf.EnumCnMap;

namespace ReportEngine.Designer.Wpf
{
    public partial class MainWindow
    {
private void OnHRulerMouseDown(object sender, MouseButtonEventArgs e)
{
    // 从水平标尺拖出垂直参考线
    if (_template == null) return;
    double px = e.GetPosition(_hRuler).X;
    double mmPx = PixelsPerMm * _zoom;
    double offsetX = -_scrollViewer.HorizontalOffset + CanvasPadding;
    double mm = Math.Round((px - offsetX) / mmPx, 1);
    if (mm < 0 || mm > _template.Page.Width) return;
    _vGuides.Add(mm);
    _draggingGuide = true;
    _draggingHGuide = false;
    _draggingGuideIndex = _vGuides.Count - 1;
    _hRuler.CaptureMouse();
    _hRuler.MouseMove += OnRulerGuideMouseMove;
    _hRuler.MouseLeftButtonUp += OnRulerGuideMouseUp;
    _canvasRenderer.Render(CanvasRenderContextFactory.Build(_template, _zoom, _gridSpacingMm, _showGrid, _gridColor, _vGuides, _hGuides, _snapLinesX, _snapLinesY), _selectedElements, _selectedBand);
    _statusText.Text = "垂直参考线: " + mm + "mm (拖出标尺外删除)";
}

private void OnVRulerMouseDown(object sender, MouseButtonEventArgs e)
{
    // 从垂直标尺拖出水平参考线
    if (_template == null) return;
    double py = e.GetPosition(_vRuler).Y;
    double mmPx = PixelsPerMm * _zoom;
    double offsetY = -_scrollViewer.VerticalOffset + CanvasPadding;
    double mm = Math.Round((py - offsetY) / mmPx, 1);
    if (mm < 0 || mm > _template.Page.Height) return;
    _hGuides.Add(mm);
    _draggingGuide = true;
    _draggingHGuide = true;
    _draggingGuideIndex = _hGuides.Count - 1;
    _vRuler.CaptureMouse();
    _vRuler.MouseMove += OnRulerGuideMouseMove;
    _vRuler.MouseLeftButtonUp += OnRulerGuideMouseUp;
    _canvasRenderer.Render(CanvasRenderContextFactory.Build(_template, _zoom, _gridSpacingMm, _showGrid, _gridColor, _vGuides, _hGuides, _snapLinesX, _snapLinesY), _selectedElements, _selectedBand);
    _statusText.Text = "水平参考线: " + mm + "mm (拖出标尺外删除)";
}

private void OnRulerGuideMouseMove(object sender, MouseEventArgs e)
{
    if (!_draggingGuide || _template == null) return;
    if (RulerGuideMover.Update(
        _draggingHGuide, _draggingGuideIndex,
        e.GetPosition(_hRuler), e.GetPosition(_vRuler),
        _zoom, PixelsPerMm, CanvasPadding,
        _scrollViewer.HorizontalOffset, _scrollViewer.VerticalOffset,
        _hGuides, _vGuides))
    {
        _canvasRenderer.Render(CanvasRenderContextFactory.Build(_template, _zoom, _gridSpacingMm, _showGrid, _gridColor, _vGuides, _hGuides, _snapLinesX, _snapLinesY), _selectedElements, _selectedBand);
    }
}

private void OnRulerGuideMouseUp(object sender, MouseButtonEventArgs e)
{
    if (!_draggingGuide) return;
    // 如果拖到负数区域（标尺外）则删除
    if (_draggingHGuide)
        GuideListEditor.RemoveIfNegative(_hGuides, _draggingGuideIndex);
    else
        GuideListEditor.RemoveIfNegative(_vGuides, _draggingGuideIndex);
    RulerGuideFinalizer.ReleaseAndUnsubscribe(_draggingHGuide, _hRuler, _vRuler, OnRulerGuideMouseMove, OnRulerGuideMouseUp);
    _draggingGuide = false;
    _draggingGuideIndex = -1;
    _canvasRenderer.Render(CanvasRenderContextFactory.Build(_template, _zoom, _gridSpacingMm, _showGrid, _gridColor, _vGuides, _hGuides, _snapLinesX, _snapLinesY), _selectedElements, _selectedBand);
    _statusText.Text = "参考线: 水平" + _hGuides.Count + "条 垂直" + _vGuides.Count + "条";
}

    }
}
