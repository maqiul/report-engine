using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf
{
    public partial class MainWindow
    {
private void NewTemplate()
{
    if (!ConfirmDiscard()) return;
    _template = new ReportTemplate();
    _currentFilePath = null;
    _dirty = false;
    _undoStack.Clear();
    _redoStack.Clear();
    _selectedElement = null;
    _selectedBand = null;
    RefreshUI();
}

private void OpenTemplate()
{
    TemplateFileOpener.Open(_parser, ConfirmDiscard, (path, template) =>
    {
        _template = template;
        _currentFilePath = path;
        _dirty = false;
        _undoStack.Clear();
        _redoStack.Clear();
        _selectedElement = null;
        _selectedBand = null;
        RefreshUI();
        _statusText.Text = "已打开: " + System.IO.Path.GetFileName(path);
        AddRecentFile(path);
    });
}

private void SaveTemplate(bool saveAs)
{
    TemplateFileSaver.Save(_template, _parser, _currentFilePath, saveAs, path =>
    {
        _currentFilePath = path;
        _dirty = false;
        UpdateTitle();
        ClearAutoSave();
        _statusText.Text = "已保存: " + System.IO.Path.GetFileName(path);
    });
}

private async void ExportPdf()
{
    _statusText.Text = "正在导出PDF...";
    await ReportFileExporter.ExportPdfAsync(_template, _previewData, _currentFilePath, path =>
    {
        _statusText.Text = "PDF已导出: " + System.IO.Path.GetFileName(path);
    });
}

private async void ExportExcel()
{
    _statusText.Text = "正在导出Excel...";
    await ReportFileExporter.ExportExcelAsync(_template, _previewData, _currentFilePath, path =>
    {
        _statusText.Text = "Excel已导出: " + System.IO.Path.GetFileName(path);
    });
}

private void ExportPng()
{
    _statusText.Text = "正在导出PNG...";
    double oldZoom = _zoom;
    CanvasImageExporter.Export(
        template: _template,
        pixelsPerMm: PixelsPerMm,
        canvasPadding: CanvasPadding,
        canvas: _canvas,
        currentFilePath: _currentFilePath,
        renderAt100: () =>
        {
            _zoom = 1.0;
            _canvasRenderer.Render(CanvasRenderContextFactory.Build(_template, _zoom, _gridSpacingMm, _showGrid, _gridColor, _vGuides, _hGuides, _snapLinesX, _snapLinesY), _selectedElements, _selectedBand);
        },
        renderAtCurrent: () =>
        {
            _zoom = oldZoom;
            _canvasRenderer.Render(CanvasRenderContextFactory.Build(_template, _zoom, _gridSpacingMm, _showGrid, _gridColor, _vGuides, _hGuides, _snapLinesX, _snapLinesY), _selectedElements, _selectedBand);
        },
        onSuccess: path => _statusText.Text = "PNG已导出: " + System.IO.Path.GetFileName(path));
}

private async void ExportBatch()
{
    _statusText.Text = "正在批量导出...";
    await ReportFileExporter.ExportBatchAsync(_template, _previewData, _currentFilePath,
        (pdfPath, excelPath) => { _statusText.Text = "批量导出完成"; });
}

    }
}
