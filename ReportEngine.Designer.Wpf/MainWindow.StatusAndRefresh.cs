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
private void ResetSelectedProperties()
{
    var el = _selectedElement;
    if (el == null) { _statusText.Text = "请先选中一个元素"; return; }
    PushUndo();
    PropertyResetter.Reset(el);
    MarkDirty();
    RefreshUI();
    _statusText.Text = "已重置属性为默认值";
}

internal void RefreshUI()
{
    _canvasRenderer.Render(CanvasRenderContextFactory.Build(_template, _zoom, _gridSpacingMm, _showGrid, _gridColor, _vGuides, _hGuides, _snapLinesX, _snapLinesY), _selectedElements, _selectedBand);
    _canvasRenderer.RenderRulers(_template!, _zoom);
    UpdateBandTree();
    UpdatePropertyList();
    UpdateTitle();
    UpdateStatusInfo();
    UpdateUndoRedoButtons();
}

private void UpdateStatusInfo()
{
    if (_template == null) { _statusText.Text = "就绪"; return; }
    var parts = new List<string>();
    if (_selectedBand != null) parts.Add(Name(_selectedBand.Type));
    if (_selectedElements.Count > 1)
        parts.Add("选中 " + _selectedElements.Count + " 个元素");
    else if (_selectedElement != null)
        parts.Add(_selectedElement.GetType().Name.Replace("Element", "") + " [" + Math.Round(_selectedElement.X, 1) + "," + Math.Round(_selectedElement.Y, 1) + " " + Math.Round(_selectedElement.Width, 1) + "×" + Math.Round(_selectedElement.Height, 1) + "mm]");
    if (parts.Count > 0) _statusText.Text = string.Join(" | ", parts);
}

private void UpdateTitle()
{
    var name = _currentFilePath != null ? System.IO.Path.GetFileName(_currentFilePath) : "新模板";
    Title = (_dirty ? "* " : "") + name + " - 报表设计器";
}

internal void MarkDirty()
{
    _dirty = true;
    UpdateTitle();
}

private void OnBandTreeSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
{
    if (_bandTree.SelectedItem is TreeViewItem item)
    {
        BandTreeSelectionCommitter.Commit(item.Tag, _template,
            ref _selectedBand, ref _selectedElement,
            onRender: () => _canvasRenderer.Render(CanvasRenderContextFactory.Build(_template, _zoom, _gridSpacingMm, _showGrid, _gridColor, _vGuides, _hGuides, _snapLinesX, _snapLinesY), _selectedElements, _selectedBand),
            onUpdateProps: UpdatePropertyList);
    }
}

private void OnCanvasRightClick(object sender, MouseButtonEventArgs e)
{
    if (_template == null) return;
    var pos = e.GetPosition(_canvas);
    var (band, element) = HitTester.Hit(pos, _template, _zoom, CanvasPadding, PixelsPerMm);

    if (element != null) { _selectedElement = element; _selectedBand = band; RefreshUI(); }
    else if (band != null) { _selectedElement = null; _selectedBand = band; RefreshUI(); }

    var menu = RightClickMenuBuilder.Build(
        selectedElement: _selectedElement,
        hasClipboard: _clipboardJson != null,
        selectedBand: _selectedBand,
        bandName: _selectedBand != null ? Name(_selectedBand.Type) : null,
        onCut: CutSelected,
        onCopy: CopySelected,
        onDelete: DeleteSelected,
        onPaste: PasteElement,
        onInsert: InsertElement,
        staticText: NewText,
        field: NewFieldBox,
        summary: NewSummaryBox,
        sysVar: NewSysVarBox,
        line: NewLine,
        shape: NewShape,
        image: NewImage,
        barcode: NewBarcode,
        table: NewTable,
        crossTab: NewCrossTab,
        chart: NewChart,
        subReport: NewSubReport,
        onAddBand: AddBand,
        onDeleteBand: DeleteBand,
        onPageSetup: ShowPageSetupDialog);

    _canvas.ContextMenu = menu;
    menu.IsOpen = true;
}

    }
}
