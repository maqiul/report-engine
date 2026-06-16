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
private void ApplyScrollBarStyle()
{
    // 让滚动条滑块更明显：通过覆盖 SystemColors 资源
    Resources[SystemColors.ScrollBarColorKey] = Color.FromRgb(220, 220, 220);
    // 滑块颜色（Thumb）
    var thumbBrush = new SolidColorBrush(Color.FromRgb(140, 140, 140));
    thumbBrush.Freeze();
    Resources[SystemColors.ControlDarkColorKey] = Color.FromRgb(140, 140, 140);

    // 给 ScrollBar 设置固定宽度/高度让其更粗一些
    var vScrollStyle = new Style(typeof(System.Windows.Controls.Primitives.ScrollBar));
    vScrollStyle.Setters.Add(new Setter(WidthProperty, 15.0));
    var trigger = new Trigger { Property = System.Windows.Controls.Primitives.ScrollBar.OrientationProperty, Value = System.Windows.Controls.Orientation.Horizontal };
    trigger.Setters.Add(new Setter(WidthProperty, double.NaN));
    trigger.Setters.Add(new Setter(HeightProperty, 15.0));
    vScrollStyle.Triggers.Add(trigger);
    Resources[typeof(System.Windows.Controls.Primitives.ScrollBar)] = vScrollStyle;
}

private void BuildLayout()
{
    Content = RootLayoutBuilder.Build(
        menu: BuildMenu(),
        toolbar: BuildToolBar(),
        fontBar: BuildFontToolBar(),
        alignBar: BuildAlignToolBar(),
        statusBar: BuildStatusBar(),
        leftPanel: BuildLeftPanel(),
        centerPanel: BuildCenterPanel(),
        rightPanel: BuildRightPanel());
}

private ToolBar BuildToolBar()
{
    return ToolBarBuilder.Build(
        onNew: NewTemplate,
        onOpen: OpenTemplate,
        onSave: () => SaveTemplate(false),
        onUndo: Undo,
        onRedo: Redo,
        onCut: CutSelected,
        onCopy: CopySelected,
        onPaste: PasteElement,
        onDelete: DeleteSelected,
        onPageSetup: ShowPageSetupDialog,
        onExportPdf: ExportPdf,
        onExportExcel: ExportExcel,
        onZoomIn: () => { _zoomSlider.Value = Math.Min(400, _zoomSlider.Value + 25); },
        onZoomOut: () => { _zoomSlider.Value = Math.Max(25, _zoomSlider.Value - 25); },
        onZoomFit: () => { _zoomSlider.Value = 100; },
        out _undoBtn, out _redoBtn, out _cutBtn, out _copyBtn, out _pasteBtn, out _deleteBtn);
}

private Border BuildStatusBar()
{
    return StatusBarBuilder.Build(
        statusText: _statusText,
        posLabel: _posLabel,
        zoomSlider: _zoomSlider,
        zoomLabel: _zoomLabel,
        onSwitchDesign: (d, p) => SwitchView("design", d, p),
        onSwitchPreview: (d, p) => SwitchView("preview", d, p),
        onShowPageSetup: ShowPageSetupDialog,
        out _tabDesign,
        out _tabPreview);
}

private DockPanel BuildLeftPanel()
{
    return LeftToolBoxBuilder.Build(
        onInsertText: () => InsertElement(NewText()),
        onInsertFieldBox: () => InsertElement(NewFieldBox()),
        onInsertSummaryBox: () => InsertElement(NewSummaryBox()),
        onInsertSysVarBox: () => InsertElement(NewSysVarBox()),
        onInsertLine: () => InsertElement(NewLine()),
        onInsertShape: () => InsertElement(NewShape()),
        onInsertImage: () => InsertElement(NewImage()),
        onInsertBarcode: () => InsertElement(NewBarcode()),
        onInsertTable: () => InsertElement(NewTable()),
        onInsertCrossTab: () => InsertElement(NewCrossTab()),
        onInsertChart: () => InsertElement(NewChart()),
        onInsertSubReport: () => InsertElement(NewSubReport()),
        onAddHeader: () => AddBand(BandType.Header, 15),
        onAddDetail: () => AddBand(BandType.Detail, 10),
        onAddFooter: () => AddBand(BandType.Footer, 10),
        onAddGroupHeader: () => AddBand(BandType.GroupHeader, 12),
        onAddGroupFooter: () => AddBand(BandType.GroupFooter, 10),
        onAddReportHeader: () => AddBand(BandType.ReportHeader, 20),
        onAddReportFooter: () => AddBand(BandType.ReportFooter, 10));
}

private Grid BuildCenterPanel()
{
    return CenterPanelBuilder.Build(
        rulerSize: RulerSize,
        hRuler: _hRuler,
        vRuler: _vRuler,
        scrollViewer: _scrollViewer,
        previewScrollViewer: _previewScrollViewer,
        onScrollChanged: () => _canvasRenderer.RenderRulers(_template!, _zoom),
        onPreviewMouseWheel: OnCanvasWheel);
}

private Grid BuildRightPanel()
{
    return RightPanelBuilder.Build(
        bandTree: _bandTree,
        propertyPanel: _propertyPanel,
        onResetSelected: ResetSelectedProperties,
        out _selectedObjLabel);
}

    }
}
