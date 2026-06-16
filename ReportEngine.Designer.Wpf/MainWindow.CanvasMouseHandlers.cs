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
private void OnCanvasMouseDown(object sender, MouseButtonEventArgs e)
{
    if (_template == null) return;
    var pos = e.GetPosition(_canvas);

    // 检查 resize 手柄
    _resizeHandle = ResizeHandleDetector.Detect(_canvas, pos, _selectedElement);
    if (_resizeHandle != ResizeHandleDetector.NoHandle)
    {
        BeginDrag(DragMode.ResizeElement, pos);
        _dragStartX = _selectedElement!.X; _dragStartY = _selectedElement.Y;
        _dragStartW = _selectedElement.Width; _dragStartH = _selectedElement.Height;
        PushUndo();
        return;
    }

    // 检查 Band 调整
    var hitEl = _canvas.InputHitTest(pos) as FrameworkElement;
    if (hitEl?.Tag is Band dragBand && hitEl.Cursor == Cursors.SizeNS)
    {
        BeginDrag(DragMode.ResizeBandHeight, pos);
        _selectedBand = dragBand;
        _dragStartH = dragBand.Height;
        PushUndo();
        RefreshUI();
        return;
    }

    Band? hitBand;
    ReportElement? hitElement;
    CanvasMouseDownHandler.Handle(
        canvas: _canvas,
        pos: pos,
        template: _template,
        zoom: _zoom,
        canvasPadding: CanvasPadding,
        pixelsPerMm: PixelsPerMm,
        selectedElements: _selectedElements,
        formatPainterActive: _formatPainterActive,
        onApplyFormat: () => ApplyFormatToTarget(_selectedElement!),
        onStopPainter: StopFormatPainter,
        onStartMove: el =>
        {
            BeginDrag(DragMode.MoveElement, pos);
            _dragStartX = el.X;
            _dragStartY = el.Y;
            PushUndo();
        },
        onStartMarquee: () => BeginDrag(DragMode.MarqueeSelect, pos),
        out hitBand,
        out hitElement);

    _selectedElement = hitElement;
    _selectedBand = hitBand;
    RefreshUI();
}

private void BeginDrag(DragMode mode, Point pos)
{
    _dragMode = mode;
    _dragStart = pos;
    _canvas.CaptureMouse();
}

private void OnCanvasDoubleClick(object sender, MouseButtonEventArgs e)
{
    if (_template == null) return;
    var pos = e.GetPosition(_canvas);
    var (_, element) = HitTester.Hit(pos, _template, _zoom, CanvasPadding, PixelsPerMm);
    if (element is TextElement txt)
    {
        TextEditDialog.Show(this, txt, newText =>
        {
            PushUndo();
            txt.Text = newText;
            MarkDirty();
            RefreshUI();
        });
    }
}

private void OnCanvasMouseMove(object sender, MouseEventArgs e)
{
    if (_template == null) return;
    var pos = e.GetPosition(_canvas);

    // 状态栏坐标
    CanvasCoordinateLabel.Update(_posLabel, pos, _zoom, PixelsPerMm, CanvasPadding);
    double z = _zoom;
    double mmPx = PixelsPerMm * z;

    if (_dragMode == DragMode.MoveElement && _selectedElement != null)
    {
        double dx = (pos.X - _dragStart.X) / mmPx;
        double dy = (pos.Y - _dragStart.Y) / mmPx;
        _snapLinesX.Clear(); _snapLinesY.Clear();
        // 多选时批量移动
        if (_selectedElements.Count > 1)
        {
            ElementMover.MoveMultiple(_selectedElements, dx, dy);
            _dragStart = pos;
        }
        else
        {
            ElementMover.MoveSingle(
                element: _selectedElement,
                band: _selectedBand,
                startX: _dragStartX, startY: _dragStartY,
                dx: dx, dy: dy,
                snapEnabled: _snapEnabled,
                excludedElements: _selectedElements,
                vGuides: _vGuides, hGuides: _hGuides,
                snapThresholdMm: SnapThresholdMm,
                snapLinesX: _snapLinesX, snapLinesY: _snapLinesY);
        }
        _canvasRenderer.Render(CanvasRenderContextFactory.Build(_template, _zoom, _gridSpacingMm, _showGrid, _gridColor, _vGuides, _hGuides, _snapLinesX, _snapLinesY), _selectedElements, _selectedBand);
    }
    else if (_dragMode == DragMode.ResizeElement && _selectedElement != null)
    {
        double dx = (pos.X - _dragStart.X) / mmPx;
        double dy = (pos.Y - _dragStart.Y) / mmPx;
        var (newX, newY, newW, newH) = ResizeCalculator.Compute(
            _resizeHandle, dx, dy, _dragStartX, _dragStartY, _dragStartW, _dragStartH);
        _selectedElement.X = Math.Max(0, Math.Round(newX * 2) / 2);
        _selectedElement.Y = Math.Max(0, Math.Round(newY * 2) / 2);
        _selectedElement.Width = Math.Max(2, Math.Round(newW * 2) / 2);
        _selectedElement.Height = Math.Max(2, Math.Round(newH * 2) / 2);
        _canvasRenderer.Render(CanvasRenderContextFactory.Build(_template, _zoom, _gridSpacingMm, _showGrid, _gridColor, _vGuides, _hGuides, _snapLinesX, _snapLinesY), _selectedElements, _selectedBand);
    }
    else if (_dragMode == DragMode.ResizeBandHeight && _selectedBand != null)
    {
        double dy = (pos.Y - _dragStart.Y) / mmPx;
        _selectedBand.Height = Math.Max(3, Math.Round((_dragStartH + dy) * 2) / 2);
        _canvasRenderer.Render(CanvasRenderContextFactory.Build(_template, _zoom, _gridSpacingMm, _showGrid, _gridColor, _vGuides, _hGuides, _snapLinesX, _snapLinesY), _selectedElements, _selectedBand);
    }
    else if (_dragMode == DragMode.MarqueeSelect)
    {
        // 绘制框选矩形
        if (_marqueeRect != null) _canvas.Children.Remove(_marqueeRect);
        _marqueeRect = MarqueeRenderer.Create(pos, _dragStart);
        Canvas.SetLeft(_marqueeRect, Math.Min(pos.X, _dragStart.X));
        Canvas.SetTop(_marqueeRect, Math.Min(pos.Y, _dragStart.Y));
        _canvas.Children.Add(_marqueeRect);
    }
}

private void OnCanvasMouseUp(object sender, MouseButtonEventArgs e)
{
    if (_dragMode == DragMode.MarqueeSelect)
    {
        // 框选结束：找出框内元素
        var hits = MarqueeSelector.Select(
            _dragStart, e.GetPosition(_canvas), _zoom, PixelsPerMm, CanvasPadding, _template);
        MarqueeCommitHelper.ApplyToSelection(hits, _selectedElements,
            ref _selectedElement, ref _selectedBand,
            ref _marqueeRect, _canvas);
        _canvas.ReleaseMouseCapture();
        _dragMode = DragMode.None;
        RefreshUI();
    }
    else if (_dragMode != DragMode.None)
    {
        _snapLinesX.Clear(); _snapLinesY.Clear();
        _canvas.ReleaseMouseCapture();
        _dragMode = DragMode.None;
        MarkDirty();
        RefreshUI();
    }
}

private void OnCanvasWheel(object sender, MouseWheelEventArgs e)
{
    if (Keyboard.Modifiers == ModifierKeys.Control)
    {
        e.Handled = true;
        _zoomSlider.Value = Math.Max(25, Math.Min(400, _zoomSlider.Value + (e.Delta > 0 ? 15 : -15)));
    }
}

private void OnCanvasDrop(object sender, DragEventArgs e)
{
    var insertedType = CanvasDropProcessor.Process(
        e, _template, _canvas, _zoom, CanvasPadding, PixelsPerMm,
        (el, band, x, y) => InsertElementAt(el, band, x, y));
    if (insertedType != null)
        _statusText.Text = "已拖入元素: " + insertedType;
}

    }
}
