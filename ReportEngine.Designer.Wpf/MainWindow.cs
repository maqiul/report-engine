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
    public class MainWindow : Window
    {
        private readonly TemplateParser _parser = new TemplateParser();
        private ReportTemplate? _template;
        private string? _currentFilePath;
        private bool _dirty;
        private bool _showGrid = true;
        private readonly List<string> _recentFiles = new List<string>();
        private const string RecentFilesPath = "recent_files.txt";
        private const string AutoSavePath = "autosave_backup.rptx";
        private System.Windows.Threading.DispatcherTimer? _autoSaveTimer;
        private string _viewMode = "design"; // design | preview

        // UI 组件
        private readonly Canvas _canvas;
        private readonly ScrollViewer _scrollViewer;
        private readonly TreeView _bandTree;
        private readonly ScrollViewer _propertyPanel;
        private readonly StackPanel _propertyStack;
        internal StackPanel PropertyStack => _propertyStack;
        private readonly TextBlock _statusText;
        private readonly TextBlock _zoomLabel;
        private readonly Slider _zoomSlider;
        private readonly TextBlock _posLabel;
        private readonly Canvas _hRuler;
        private readonly ScrollViewer _previewScrollViewer;
        private readonly Canvas _previewCanvas;
        private readonly Canvas _vRuler;

        // 渲染器（Step2.A 拆出）
        private readonly CanvasRenderer _canvasRenderer;
        private readonly PreviewRenderer _previewRenderer;

        private ReportElement? _selectedElement;
        private Band? _selectedBand;
        private readonly List<ReportElement> _selectedElements = new List<ReportElement>();
        internal IReadOnlyList<ReportElement> SelectedElements => _selectedElements;

        // 拖动
        private enum DragMode { None, MoveElement, ResizeBandHeight, ResizeElement, MarqueeSelect }
        private DragMode _dragMode;
        private Point _dragStart;
        private double _dragStartX, _dragStartY, _dragStartW, _dragStartH;
        private int _resizeHandle = -1;
        private Rectangle? _marqueeRect;

        // 缩放
        private double _zoom = 1.0;
        private const double PixelsPerMm = 96.0 / 25.4;
        private const double CanvasPadding = 30;
        private const double RulerSize = 22;

        // 撤销/重做
        private readonly List<string> _undoStack = new List<string>();
        private readonly List<string> _redoStack = new List<string>();
        private const int MaxUndo = 50;
        private Button? _undoBtn;
        private Button? _redoBtn;
        private Button? _cutBtn;
        private Button? _copyBtn;
        private Button? _deleteBtn;
        private Button? _pasteBtn;

        // 剪贴板
        private string? _clipboardJson;

        // 预览数据
        private Dictionary<string, object>? _previewData;

        // 标尺参考线
        private readonly List<double> _hGuides = new List<double>(); // 水平参考线位置(mm)
        private readonly List<double> _vGuides = new List<double>(); // 垂直参考线位置(mm)
        private bool _draggingGuide;
        private bool _draggingHGuide; // true=水平, false=垂直
        private int _draggingGuideIndex = -1;

        // 吸附
        private bool _snapEnabled = true;
        private const double SnapThresholdMm = 1.5; // 吸附距离阈值(mm)
        private readonly List<double> _snapLinesX = new List<double>(); // 当前吸附辅助线
        private readonly List<double> _snapLinesY = new List<double>();

        // 网格设置
        private double _gridSpacingMm = 5.0;
        private Color _gridColor = Color.FromArgb(40, 180, 180, 180);

        // 格式刷
        private bool _formatPainterActive;
        private ReportElement? _formatPainterSource;

        public MainWindow()
        {
            Title = "报表设计器";
            Width = 1400;
            Height = 900;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));

            _canvas = new Canvas { Background = Brushes.Transparent, ClipToBounds = true, AllowDrop = true };
            _canvas.MouseLeftButtonDown += OnCanvasMouseDown;
            _canvas.MouseMove += OnCanvasMouseMove;
            _canvas.MouseLeftButtonUp += OnCanvasMouseUp;
            _canvas.MouseRightButtonUp += OnCanvasRightClick;
            _canvas.PreviewMouseLeftButtonDown += (s, e) => { if (e.ClickCount == 2) { OnCanvasDoubleClick(s, e); e.Handled = true; } };
            _canvas.Drop += OnCanvasDrop;
            _canvas.DragOver += (s, e) => { e.Effects = e.Data.GetDataPresent("ElementType") ? DragDropEffects.Copy : DragDropEffects.None; e.Handled = true; };

            _scrollViewer = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Background = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                Content = _canvas,
            };

            _hRuler = new Canvas { Height = RulerSize, Background = new SolidColorBrush(Color.FromRgb(230, 230, 230)), ClipToBounds = true };
            _vRuler = new Canvas { Width = RulerSize, Background = new SolidColorBrush(Color.FromRgb(230, 230, 230)), ClipToBounds = true };
            _hRuler.MouseLeftButtonDown += OnHRulerMouseDown;
            _vRuler.MouseLeftButtonDown += OnVRulerMouseDown;

            _previewCanvas = new Canvas { Background = new SolidColorBrush(Color.FromRgb(180, 180, 180)) };
            _previewScrollViewer = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Background = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                Content = _previewCanvas,
                Visibility = Visibility.Collapsed,
            };

            // 渲染器（Step2.A 拆出）：构造函数持 canvas/ruler/scroll 引用，渲染状态走 ctx 参数
            _canvasRenderer = new CanvasRenderer(_canvas, _hRuler, _vRuler, _scrollViewer);
            _previewRenderer = new PreviewRenderer(_previewCanvas);

            _bandTree = new TreeView { Background = Brushes.White, Foreground = Brushes.Black, BorderThickness = new Thickness(0) };
            _bandTree.SelectedItemChanged += OnBandTreeSelectionChanged;

            _propertyStack = new StackPanel();
            _propertyPanel = new ScrollViewer
            {
                Content = _propertyStack,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Background = Brushes.White,
            };

            _statusText = new TextBlock { Text = "就绪", Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center };
            _zoomLabel = new TextBlock { Text = "100%", Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center, MinWidth = 40 };
            _posLabel = new TextBlock { Text = "", Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center, MinWidth = 120 };
            _zoomSlider = new Slider { Minimum = 25, Maximum = 400, Value = 100, Width = 120, VerticalAlignment = VerticalAlignment.Center };
            _zoomSlider.ValueChanged += (_, __) => { _zoom = _zoomSlider.Value / 100.0; _zoomLabel.Text = (int)_zoomSlider.Value + "%"; _canvasRenderer.Render(CanvasRenderContextFactory.Build(_template, _zoom, _gridSpacingMm, _showGrid, _gridColor, _vGuides, _hGuides, _snapLinesX, _snapLinesY), _selectedElements, _selectedBand); _canvasRenderer.RenderRulers(_template!, _zoom); };

            BuildLayout();
            ApplyScrollBarStyle();
            SetupKeyBindings();
            LoadRecentFiles();
            SetupAutoSave();
            NewTemplate();
            CheckAutoSaveRecovery();
        }

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

        // ============================== 布局 ==============================

        // 字体工具栏组件
        private ComboBox _fontFamilyCombo = null!;
        private ComboBox _fontSizeCombo = null!;

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

        private Border _tabDesign = null!;
        private Border _tabPreview = null!;

        private ToolBarTray BuildFontToolBar()
        {
            return FontToolBarBuilder.Build(
                onFontFamilyChanged: ApplyFontFamily,
                onFontSizeChanged: ApplyFontSize,
                onToggleBold: ToggleFontBold,
                onToggleItalic: ToggleFontItalic,
                onToggleUnderline: ToggleFontUnderline,
                onAlignLeft: () => SetAlignment(ReportEngine.Core.TextAlignment.Left),
                onAlignCenter: () => SetAlignment(ReportEngine.Core.TextAlignment.Center),
                onAlignRight: () => SetAlignment(ReportEngine.Core.TextAlignment.Right),
                out _fontFamilyCombo,
                out _fontSizeCombo);
        }

        private void ApplyFontFamily()
        {
            if (_selectedElement is TextElement t && !string.IsNullOrEmpty(_fontFamilyCombo.Text))
            { PushUndo(); t.Font.Family = _fontFamilyCombo.Text; MarkDirty(); RefreshUI(); }
        }

        private void ApplyFontSize()
        {
            if (_selectedElement is TextElement t && double.TryParse(_fontSizeCombo.Text, out var sz) && sz > 0)
            { PushUndo(); t.Font.Size = sz; MarkDirty(); RefreshUI(); }
        }

        private void ToggleFontBold()
        {
            if (_selectedElement is TextElement t) { PushUndo(); t.Font.Bold = !t.Font.Bold; MarkDirty(); RefreshUI(); }
        }

        private void ToggleFontItalic()
        {
            if (_selectedElement is TextElement t) { PushUndo(); t.Font.Italic = !t.Font.Italic; MarkDirty(); RefreshUI(); }
        }

        private void ToggleFontUnderline()
        {
            if (_selectedElement is TextElement t) { PushUndo(); t.Font.Underline = !t.Font.Underline; MarkDirty(); RefreshUI(); }
        }

        private void SetAlignment(ReportEngine.Core.TextAlignment a)
        {
            if (_selectedElement is TextElement t) { PushUndo(); t.Alignment = a; MarkDirty(); RefreshUI(); }
        }

        private ToolBarTray BuildAlignToolBar()
        {
            return AlignToolBarBuilder.Build(
                onAlignLeft: () => AlignElements("left"),
                onAlignRight: () => AlignElements("right"),
                onAlignTop: () => AlignElements("top"),
                onAlignBottom: () => AlignElements("bottom"),
                onAlignHCenter: () => AlignElements("hcenter"),
                onAlignVCenter: () => AlignElements("vcenter"),
                onSameWidth: () => AlignElements("samewidth"),
                onSameHeight: () => AlignElements("sameheight"),
                onBringToFront: () => MoveElementOrder("front"),
                onSendToBack: () => MoveElementOrder("back"),
                onFormatPainter: StartFormatPainter,
                onToggleLock: ToggleLockSelected,
                onGroup: GroupSelected,
                onUngroup: UngroupSelected);
        }

        private void ToggleLockSelected()
        {
            var targets = _selectedElements.Count > 0 ? _selectedElements : (_selectedElement != null ? new List<ReportElement> { _selectedElement } : new List<ReportElement>());
            if (targets.Count == 0) { _statusText.Text = "请先选中元素"; return; }
            PushUndo();
            bool newState = !targets[0].Locked;
            foreach (var el in targets) el.Locked = newState;
            MarkDirty();
            RefreshUI();
            _statusText.Text = newState ? "已锁定 " + targets.Count + " 个元素" : "已解锁 " + targets.Count + " 个元素";
        }

        /// <summary>组合选中的元素为一组</summary>
        private void GroupSelected()
        {
            var targets = _selectedElements.Count > 1 ? _selectedElements : (_selectedElements.Count == 0 && _selectedElement != null ? new List<ReportElement> { _selectedElement } : _selectedElements);
            if (targets.Count < 2) { _statusText.Text = "请选中2个以上元素再组合"; return; }
            PushUndo();
            ElementGrouper.Group(targets);
            MarkDirty();
            RefreshUI();
            _statusText.Text = "已组合 " + targets.Count + " 个元素";
        }

        /// <summary>取消选中元素的分组</summary>
        private void UngroupSelected()
        {
            var targets = _selectedElements.Count > 0 ? _selectedElements : (_selectedElement != null ? new List<ReportElement> { _selectedElement } : new List<ReportElement>());
            if (targets.Count == 0 || targets.All(e => e.GroupId == null)) { _statusText.Text = "选中的元素未分组"; return; }
            PushUndo();
            ElementGrouper.Ungroup(targets);
            MarkDirty();
            RefreshUI();
            _statusText.Text = "已取消组合";
        }

        /// <summary>启动格式刷：复制当前选中元素的样式</summary>
        private void StartFormatPainter()
        {
            var src = _selectedElement;
            if (src == null) { _statusText.Text = "请先选中一个元素作为样式源"; return; }
            _formatPainterSource = src;
            _formatPainterActive = true;
            _statusText.Text = "格式刷已激活，请点击目标元素应用样式";
        }

        /// <summary>应用格式刷到目标元素</summary>
        private void ApplyFormatToTarget(ReportElement target)
        {
            if (_formatPainterSource == null || !_formatPainterActive) return;
            PushUndo();
            FormatPainterApplier.Apply(_formatPainterSource, target);
            MarkDirty();
            RefreshUI();
            _statusText.Text = "已应用格式刷";
        }

        /// <summary>关闭格式刷</summary>
        private void StopFormatPainter()
        {
            _formatPainterActive = false;
            _formatPainterSource = null;
        }

        /// <summary>设置自动保存定时器：每60秒自动保存一次草稿</summary>
        private void SetupAutoSave()
        {
            _autoSaveTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };
            _autoSaveTimer.Tick += (_, __) =>
            {
                if (_dirty && _template != null)
                {
                    try
                    {
                        var json = _parser.Serialize(_template);
                        System.IO.File.WriteAllText(AutoSavePath, json);
                        _statusText.Text = "自动保存草稿: " + DateTime.Now.ToString("HH:mm:ss");
                    }
                    catch { }
                }
            };
            _autoSaveTimer.Start();
        }

        /// <summary>启动时检查是否有自动保存的草稿</summary>
        private void CheckAutoSaveRecovery()
        {
            AutoSaveRecoveryChecker.Check(AutoSavePath, _parser,
                onRecovered: template =>
                {
                    _template = template;
                    _currentFilePath = null;
                    _dirty = false;
                    RefreshUI();
                    _statusText.Text = "已从草稿恢复";
                },
                onSkipped: () => { _statusText.Text = "已忽略草稿文件"; });
        }

        /// <summary>手动保存后删除自动保存草稿</summary>
        private void ClearAutoSave()
        {
            if (System.IO.File.Exists(AutoSavePath))
            {
                try { System.IO.File.Delete(AutoSavePath); } catch { }
            }
        }

        /// <summary>重置选中元素的属性为默认值</summary>
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

        private TextBlock _selectedObjLabel = null!;

        private Menu BuildMenu()
        {
            var menu = new Menu();
            var file = new MenuItem { Header = "文件(_F)" };
            file.Items.Add(MakeMenuItem("新建(_N)", "Ctrl+N", NewTemplate));
            file.Items.Add(MakeMenuItem("打开(_O)...", "Ctrl+O", OpenTemplate));
            file.Items.Add(new Separator());
            file.Items.Add(MakeMenuItem("保存(_S)", "Ctrl+S", () => SaveTemplate(false)));
            file.Items.Add(MakeMenuItem("另存为(_A)...", null, () => SaveTemplate(true)));
            file.Items.Add(new Separator());
            file.Items.Add(MakeMenuItem("页面设置(_P)...", null, ShowPageSetupDialog));
            file.Items.Add(new Separator());
            file.Items.Add(MakeMenuItem("导出PDF(_D)...", null, ExportPdf));
            file.Items.Add(MakeMenuItem("导出Excel(_E)...", null, ExportExcel));
            file.Items.Add(MakeMenuItem("导出图片(_I)...", null, ExportPng));
            file.Items.Add(MakeMenuItem("批量导出PDF+Excel(_B)...", null, ExportBatch));
            file.Items.Add(new Separator());
            file.Items.Add(MakeMenuItem("数据源(_S)...", null, OnShowDataSourceClicked));
            file.Items.Add(MakeMenuItem("数据绑定向导(_B)...", null, OnShowDataBindingWizardClicked));
            file.Items.Add(MakeMenuItem("模板参数(_P)...", null, OnShowTemplateParamsClicked));
            file.Items.Add(MakeMenuItem("加载预览数据(_L)...", null, OnLoadPreviewDataClicked));
            file.Items.Add(new Separator());
            BuildRecentFilesMenu(file);
            file.Items.Add(new Separator());
            file.Items.Add(MakeMenuItem("退出(_X)", "Alt+F4", Close));
            menu.Items.Add(file);

            var edit = new MenuItem { Header = "编辑(_E)" };
            edit.Items.Add(MakeMenuItem("撤销(_Z)", "Ctrl+Z", Undo));
            edit.Items.Add(MakeMenuItem("重做(_Y)", "Ctrl+Y", Redo));
            edit.Items.Add(new Separator());
            edit.Items.Add(MakeMenuItem("剪切(_X)", "Ctrl+X", CutSelected));
            edit.Items.Add(MakeMenuItem("复制(_C)", "Ctrl+C", CopySelected));
            edit.Items.Add(MakeMenuItem("粘贴(_V)", "Ctrl+V", PasteElement));
            edit.Items.Add(new Separator());
            edit.Items.Add(MakeMenuItem("删除", "Delete", DeleteSelected));
            edit.Items.Add(MakeMenuItem("全选", "Ctrl+A", SelectAll));
            menu.Items.Add(edit);

            var insert = new MenuItem { Header = "插入(_I)" };
            // 部件框子菜单
            var ctrlMenu = new MenuItem { Header = "部件框(_C)" };
            ctrlMenu.Items.Add(MakeMenuItem("静态框(_T)", null, () => InsertElement(NewText())));
            ctrlMenu.Items.Add(MakeMenuItem("字段框(_F)", null, () => InsertElement(NewFieldBox())));
            ctrlMenu.Items.Add(MakeMenuItem("统计框(_U)", null, () => InsertElement(NewSummaryBox())));
            ctrlMenu.Items.Add(MakeMenuItem("系统变量框(_V)", null, () => InsertElement(NewSysVarBox())));
            ctrlMenu.Items.Add(new Separator());
            ctrlMenu.Items.Add(MakeMenuItem("线段(_L)", null, () => InsertElement(NewLine())));
            ctrlMenu.Items.Add(MakeMenuItem("图形框(_S)", null, () => InsertElement(NewShape())));
            ctrlMenu.Items.Add(MakeMenuItem("图象框(_P)", null, () => InsertElement(NewImage())));
            ctrlMenu.Items.Add(MakeMenuItem("条形码&二维码(_B)", null, () => InsertElement(NewBarcode())));
            ctrlMenu.Items.Add(MakeMenuItem("表格(_G)", null, () => InsertElement(NewTable())));
            ctrlMenu.Items.Add(MakeMenuItem("交叉表", null, () => InsertElement(NewCrossTab())));
            ctrlMenu.Items.Add(MakeMenuItem("图表(_H)", null, () => InsertElement(NewChart())));
            ctrlMenu.Items.Add(MakeMenuItem("子报表(_E)", null, () => InsertElement(NewSubReport())));
            insert.Items.Add(ctrlMenu);
            // 报表节子菜单
            var sectionMenu = new MenuItem { Header = "报表节(_S)" };
            sectionMenu.Items.Add(MakeMenuItem("报表头", null, () => AddBand(BandType.ReportHeader, 20)));
            sectionMenu.Items.Add(MakeMenuItem("页眉", null, () => AddBand(BandType.Header, 15)));
            sectionMenu.Items.Add(MakeMenuItem("分组头", null, () => AddBand(BandType.GroupHeader, 12)));
            sectionMenu.Items.Add(MakeMenuItem("明细", null, () => AddBand(BandType.Detail, 10)));
            sectionMenu.Items.Add(MakeMenuItem("分组尾", null, () => AddBand(BandType.GroupFooter, 10)));
            sectionMenu.Items.Add(MakeMenuItem("页脚", null, () => AddBand(BandType.Footer, 10)));
            sectionMenu.Items.Add(MakeMenuItem("报表尾", null, () => AddBand(BandType.ReportFooter, 10)));
            insert.Items.Add(sectionMenu);
            insert.Items.Add(new Separator());
            insert.Items.Add(MakeMenuItem("页面设置(_P)...", null, ShowPageSetupDialog));
            menu.Items.Add(insert);

            var view = new MenuItem { Header = "视图(_V)" };
            view.Items.Add(MakeMenuItem("放大", "Ctrl++", () => { _zoomSlider.Value = Math.Min(400, _zoomSlider.Value + 25); }));
            view.Items.Add(MakeMenuItem("缩小", "Ctrl+-", () => { _zoomSlider.Value = Math.Max(25, _zoomSlider.Value - 25); }));
            view.Items.Add(MakeMenuItem("100%", null, () => { _zoomSlider.Value = 100; }));
            view.Items.Add(new Separator());
            var gridItem = new MenuItem { Header = "显示网格线(_G)", IsCheckable = true, IsChecked = _showGrid };
            gridItem.Click += (_, __) => { _showGrid = gridItem.IsChecked; _canvasRenderer.Render(CanvasRenderContextFactory.Build(_template, _zoom, _gridSpacingMm, _showGrid, _gridColor, _vGuides, _hGuides, _snapLinesX, _snapLinesY), _selectedElements, _selectedBand); };
            view.Items.Add(gridItem);
            view.Items.Add(MakeMenuItem("网格设置...", null, ShowGridSettingsDialog));
            view.Items.Add(new Separator());
            var miClearGuides = new MenuItem { Header = "清除所有参考线" };
            miClearGuides.Click += (_, __) => { _hGuides.Clear(); _vGuides.Clear(); _canvasRenderer.Render(CanvasRenderContextFactory.Build(_template, _zoom, _gridSpacingMm, _showGrid, _gridColor, _vGuides, _hGuides, _snapLinesX, _snapLinesY), _selectedElements, _selectedBand); _statusText.Text = "已清除所有参考线"; };
            view.Items.Add(miClearGuides);
            view.Items.Add(new Separator());
            var snapItem = new MenuItem { Header = "吸附对齐(_S)", IsCheckable = true, IsChecked = _snapEnabled };
            snapItem.Click += (_, __) => { _snapEnabled = snapItem.IsChecked; };
            view.Items.Add(snapItem);
            menu.Items.Add(view);

            var format = new MenuItem { Header = "格式(_O)" };
            format.Items.Add(MakeMenuItem("左对齐", null, () => AlignElements("left")));
            format.Items.Add(MakeMenuItem("右对齐", null, () => AlignElements("right")));
            format.Items.Add(MakeMenuItem("顶端对齐", null, () => AlignElements("top")));
            format.Items.Add(MakeMenuItem("底端对齐", null, () => AlignElements("bottom")));
            format.Items.Add(new Separator());
            format.Items.Add(MakeMenuItem("水平居中", null, () => AlignElements("hcenter")));
            format.Items.Add(MakeMenuItem("垂直居中", null, () => AlignElements("vcenter")));
            format.Items.Add(new Separator());
            format.Items.Add(MakeMenuItem("等宽", null, () => AlignElements("samewidth")));
            format.Items.Add(MakeMenuItem("等高", null, () => AlignElements("sameheight")));
            format.Items.Add(new Separator());
            format.Items.Add(MakeMenuItem("置于顶层", null, () => MoveElementOrder("front")));
            format.Items.Add(MakeMenuItem("置于底层", null, () => MoveElementOrder("back")));
            format.Items.Add(MakeMenuItem("上移一层", null, () => MoveElementOrder("up")));
            format.Items.Add(MakeMenuItem("下移一层", null, () => MoveElementOrder("down")));
            menu.Items.Add(format);

            var help = new MenuItem { Header = "帮助(_H)" };
            help.Items.Add(MakeMenuItem("快捷键...", "F1", () => ShortcutsDialog.Show(this)));
            help.Items.Add(MakeMenuItem("关于...", null, () => AboutDialog.Show(this)));
            menu.Items.Add(help);

            return menu;
        }

        private void SetupKeyBindings()
        {
            InputBindings.Add(new KeyBinding(new RelayCmd(NewTemplate), Key.N, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(new RelayCmd(OpenTemplate), Key.O, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(new RelayCmd(() => SaveTemplate(false)), Key.S, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(new RelayCmd(Undo), Key.Z, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(new RelayCmd(Redo), Key.Y, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(new RelayCmd(CutSelected), Key.X, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(new RelayCmd(CopySelected), Key.C, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(new RelayCmd(PasteElement), Key.V, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(new RelayCmd(DeleteSelected), Key.Delete, ModifierKeys.None));
            InputBindings.Add(new KeyBinding(new RelayCmd(SelectAll), Key.A, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(new RelayCmd(DuplicateSelected), Key.D, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(new RelayCmd(() => ShortcutsDialog.Show(this)), Key.F1, ModifierKeys.None));
            InputBindings.Add(new KeyBinding(new RelayCmd(SearchElement), Key.F, ModifierKeys.Control));

            // 方向键微调
            PreviewKeyDown += OnPreviewKeyDown;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Tab键切换选中元素
            if (e.Key == Key.Tab && _template != null)
            {
                var band = _selectedBand ?? _template.Bands.FirstOrDefault();
                if (band != null && band.Elements.Count > 0)
                {
                    bool reverse = Keyboard.Modifiers == ModifierKeys.Shift;
                    var next = TabCycleSelector.SelectNext(band.Elements, _selectedElement, reverse);
                    if (next != null)
                    {
                        _selectedElement = next;
                        _selectedElements.Clear();
                        _selectedElements.Add(_selectedElement);
                        _selectedBand = band;
                        RefreshUI();
                    }
                }
                e.Handled = true;
                return;
            }

            if (_selectedElements.Count == 0 && _selectedElement == null) return;
            var delta = KeyNudgeCalculator.TryGetDelta(e.Key, Keyboard.Modifiers);
            if (delta == null) return;
            NudgeSelected(delta.Value.dx, delta.Value.dy);
            e.Handled = true;
        }

        private void NudgeSelected(double dx, double dy)
        {
            PushUndo();
            var targets = _selectedElements.Count > 0 ? _selectedElements : (_selectedElement != null ? new List<ReportElement> { _selectedElement } : new List<ReportElement>());
            ElementNudger.Nudge(targets, dx, dy);
            MarkDirty();
            RefreshUI();
        }

        // ============================== 画布渲染 ==============================

        private void SwitchView(string mode, Border tabDesign, Border tabPreview)
        {
            _viewMode = mode;
            ViewSwitcher.Switch(
                mode: mode,
                tabDesign: tabDesign,
                tabPreview: tabPreview,
                scrollViewer: _scrollViewer,
                hRuler: _hRuler,
                vRuler: _vRuler,
                previewScrollViewer: _previewScrollViewer,
                onPreviewRender: () => _previewRenderer.Render(_template!, _zoom, _previewData));
        }

        // ============================== 标尺 ==============================

        // ============================== 标尺参考线拖拽 ==============================

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

        private void OnCanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_template == null) return;
            var pos = e.GetPosition(_canvas);

            // 检查 resize 手柄
            _resizeHandle = ResizeHandleDetector.Detect(_canvas, pos, _selectedElement);
            if (_resizeHandle != ResizeHandleDetector.NoHandle)
            {
                _dragMode = DragMode.ResizeElement;
                _dragStart = pos;
                _dragStartX = _selectedElement!.X; _dragStartY = _selectedElement.Y;
                _dragStartW = _selectedElement.Width; _dragStartH = _selectedElement.Height;
                _canvas.CaptureMouse();
                PushUndo();
                return;
            }

            // 检查 Band 调整
            var hitEl = _canvas.InputHitTest(pos) as FrameworkElement;
            if (hitEl?.Tag is Band dragBand && hitEl.Cursor == Cursors.SizeNS)
            {
                _dragMode = DragMode.ResizeBandHeight;
                _selectedBand = dragBand;
                _dragStart = pos;
                _dragStartH = dragBand.Height;
                _canvas.CaptureMouse();
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
                    _dragMode = DragMode.MoveElement;
                    _dragStart = pos;
                    _dragStartX = el.X;
                    _dragStartY = el.Y;
                    _canvas.CaptureMouse();
                    PushUndo();
                },
                onStartMarquee: () =>
                {
                    _dragMode = DragMode.MarqueeSelect;
                    _dragStart = pos;
                    _canvas.CaptureMouse();
                },
                out hitBand,
                out hitElement);

            _selectedElement = hitElement;
            _selectedBand = hitBand;
            RefreshUI();
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
            double z = _zoom;
            double mmPx = PixelsPerMm * z;

            // 状态栏坐标
            double mmX = (pos.X - CanvasPadding) / mmPx;
            double mmY = (pos.Y - CanvasPadding) / mmPx;
            _posLabel.Text = $"X: {mmX:F1}mm  Y: {mmY:F1}mm";

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


        // ============================== 文件操作 ==============================

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

        /// <summary>导出当前画布为PNG图片</summary>
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

        /// <summary>批量导出PDF+Excel到同一目录</summary>
        private async void ExportBatch()
        {
            _statusText.Text = "正在批量导出...";
            await ReportFileExporter.ExportBatchAsync(_template, _previewData, _currentFilePath,
                (pdfPath, excelPath) => { _statusText.Text = "批量导出完成"; });
        }

        private bool ConfirmDiscard()
        {
            if (!_dirty) return true;
            var r = MessageBox.Show("模板已修改，是否保存？", "确认", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            if (r == MessageBoxResult.Cancel) return false;
            if (r == MessageBoxResult.Yes) SaveTemplate(false);
            return true;
        }

        private void AddRecentFile(string path)
        {
            _recentFiles.Remove(path);
            _recentFiles.Insert(0, path);
            if (_recentFiles.Count > 10) _recentFiles.RemoveAt(10);
            try { File.WriteAllLines(RecentFilesPath, _recentFiles); } catch { }
        }

        private void LoadRecentFiles()
        {
            try
            {
                if (File.Exists(RecentFilesPath))
                    _recentFiles.AddRange(File.ReadAllLines(RecentFilesPath).Where(l => !string.IsNullOrWhiteSpace(l)).Take(10));
            }
            catch { }
        }

        private void BuildRecentFilesMenu(MenuItem parent)
        {
            RecentFilesMenuBuilder.Build(parent, _recentFiles, OpenFileDirect);
        }

        private void OpenFileDirect(string path)
        {
            if (!ConfirmDiscard()) return;
            try
            {
                var json = File.ReadAllText(path);
                _template = _parser.Parse(json);
                _currentFilePath = path;
                _dirty = false;
                _undoStack.Clear(); _redoStack.Clear();
                _selectedElement = null; _selectedBand = null;
                RefreshUI();
                _statusText.Text = "已打开: " + System.IO.Path.GetFileName(path);
                AddRecentFile(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show("打开失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                _recentFiles.Remove(path);
            }
        }

        // ============================== 元素操作 ==============================

        private void InsertElement(ReportElement el)
        {
            if (_template == null) return;
            var band = _selectedBand ?? _template.Bands.FirstOrDefault(b => b.Type == BandType.Detail) ?? _template.Bands.FirstOrDefault();
            if (band == null) return;
            PushUndo();
            band.Elements.Add(el);
            _selectedElement = el;
            _selectedBand = band;
            MarkDirty();
            RefreshUI();
        }

        private void InsertElementAt(ReportElement el, Band band, double mmX, double mmY)
        {
            el.X = Math.Max(0, Math.Round(mmX * 2) / 2);
            el.Y = Math.Max(0, Math.Round(mmY * 2) / 2);
            PushUndo();
            band.Elements.Add(el);
            _selectedElement = el;
            _selectedBand = band;
            _selectedElements.Clear();
            _selectedElements.Add(el);
            MarkDirty();
            RefreshUI();
        }

        private void OnCanvasDrop(object sender, DragEventArgs e)
        {
            var insertedType = CanvasDropProcessor.Process(
                e, _template, _canvas, _zoom, CanvasPadding, PixelsPerMm,
                (el, band, x, y) => InsertElementAt(el, band, x, y));
            if (insertedType != null)
                _statusText.Text = "已拖入元素: " + insertedType;
        }

        private void DeleteSelected()
        {
            if (_template == null) return;
            var targets = _selectedElements.Count > 0
                ? (IList<ReportElement>)_selectedElements
                : (_selectedElement != null ? new List<ReportElement> { _selectedElement } : null);
            if (targets == null) return;
            PushUndo();
            ElementDeleter.DeleteFromBands(_template.Bands, targets);
            _selectedElements.Clear();
            _selectedElement = null;
            MarkDirty();
            RefreshUI();
        }

        private void CutSelected()
        {
            if (_selectedElement == null) return;
            CopySelected();
            DeleteSelected();
        }

        private void CopySelected()
        {
            if (_selectedElement == null) return;
            _clipboardJson = ClipboardHelper.SerializeElement(_selectedElement, _parser);
            _statusText.Text = "已复制元素";
        }

        private void PasteElement()
        {
            if (_clipboardJson == null || _template == null) return;
            var el = ClipboardPasteHelper.ParseAndOffset(_clipboardJson, _parser);
            if (el == null) return;
            InsertElement(el);
            _statusText.Text = "已粘贴元素";
        }

        /// <summary>Ctrl+D 复制并偏移</summary>
        private void DuplicateSelected()
        {
            if (_selectedElement == null || _template == null) return;
            var band = _selectedBand ?? _template.Bands.FirstOrDefault();
            if (band == null) return;
            var newEl = ElementDuplicator.Duplicate(_selectedElement, _parser);
            if (newEl == null) return;
            PushUndo();
            band.Elements.Add(newEl);
            _selectedElement = newEl;
            _selectedElements.Clear();
            _selectedElements.Add(newEl);
            MarkDirty();
            RefreshUI();
            _statusText.Text = "已复制元素 (Ctrl+D)";
        }

        private void SelectAll()
        {
            if (_template == null) return;
            _selectedElements.Clear();
            var band = _selectedBand ?? _template.Bands.FirstOrDefault();
            if (band != null)
            {
                foreach (var el in band.Elements)
                    _selectedElements.Add(el);
                if (_selectedElements.Count > 0)
                    _selectedElement = _selectedElements[0];
                _selectedBand = band;
            }
            RefreshUI();
        }

        private void DeleteBand(Band band)
        {
            if (_template == null) return;
            PushUndo();
            _template.Bands.Remove(band);
            _selectedBand = null;
            _selectedElement = null;
            _selectedElements.Clear();
            MarkDirty();
            RefreshUI();
        }

        private void AddBand(BandType type, double height)
        {
            if (_template == null) return;
            PushUndo();
            _template.Bands.Add(new Band { Type = type, Height = height });
            MarkDirty();
            RefreshUI();
        }

        private void AlignElements(string mode)
        {
            var targets = _selectedElements.Count > 1 ? _selectedElements : new List<ReportElement>();
            if (!ElementAligner.Align(targets, mode))
            {
                _statusText.Text = "请选中多个元素后再对齐";
                return;
            }
            PushUndo();
            MarkDirty();
            RefreshUI();
        }

        private void MoveElementOrder(string direction)
        {
            if (_selectedElement == null || _selectedBand == null) return;
            if (!ZOrderHelper.Move(_selectedBand.Elements, _selectedElement, direction)) return;
            PushUndo();
            MarkDirty();
            RefreshUI();
        }

        internal void PushUndo()
        {
            if (_template == null) return;
            try
            {
                var json = _parser.Serialize(_template);
                _undoStack.Add(json);
                if (_undoStack.Count > MaxUndo) _undoStack.RemoveAt(0);
                _redoStack.Clear();
            }
            catch { }
        }

        private void Undo()
        {
            if (_undoStack.Count == 0 || _template == null) return;
            try
            {
                _redoStack.Add(_parser.Serialize(_template));
                var json = _undoStack[_undoStack.Count - 1];
                _undoStack.RemoveAt(_undoStack.Count - 1);
                _template = _parser.Parse(json);
                _selectedElement = null;
                _selectedBand = null;
                _dirty = true;
                RefreshUI();
                _statusText.Text = "已撤销";
            }
            catch { }
        }

        private void Redo()
        {
            if (_redoStack.Count == 0 || _template == null) return;
            try
            {
                _undoStack.Add(_parser.Serialize(_template));
                var json = _redoStack[_redoStack.Count - 1];
                _redoStack.RemoveAt(_redoStack.Count - 1);
                _template = _parser.Parse(json);
                _selectedElement = null;
                _selectedBand = null;
                _dirty = true;
                RefreshUI();
                _statusText.Text = "已重做";
            }
            catch { }
        }

        // ============================== 元素工厂 ==============================

        // ============================== 刷新 ==============================

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

        /// <summary>更新撤销/重做按钮的启用状态</summary>
        private void UpdateUndoRedoButtons()
        {
            if (_undoBtn != null)
                _undoBtn.IsEnabled = _undoStack.Count > 0;
            if (_redoBtn != null)
                _redoBtn.IsEnabled = _redoStack.Count > 0;
            
            // 剪切/复制/删除：需要选中元素
            bool hasSelection = _selectedElement != null;
            if (_cutBtn != null)
                _cutBtn.IsEnabled = hasSelection;
            if (_copyBtn != null)
                _copyBtn.IsEnabled = hasSelection;
            if (_deleteBtn != null)
                _deleteBtn.IsEnabled = hasSelection;
            
            // 粘贴：需要剪贴板有内容
            if (_pasteBtn != null)
                _pasteBtn.IsEnabled = _clipboardJson != null;
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

        private void UpdateBandTree()
        {
            _bandTree.Items.Clear();
            if (_template == null) return;
            foreach (var band in _template.Bands)
            {
                var node = new TreeViewItem
                {
                    Header = BandIcon(band.Type) + " " + Name(band.Type) + " (" + band.Height + "mm)",
                    Tag = band,
                    Foreground = Brushes.Black,
                    IsExpanded = true,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    VerticalContentAlignment = VerticalAlignment.Center,
                };
                // Band 右键菜单
                node.ContextMenu = BuildBandTreeContextMenu(band);

                foreach (var el in band.Elements)
                {
                    string icon = ElementIcon(el);
                    string elLabel = ElementTreeLabelBuilder.BuildLabel(el);
                    string lockIcon = el.Locked ? " 🔒" : "";
                    var child = new TreeViewItem { Header = icon + " " + elLabel + lockIcon, Tag = el, Foreground = el.Locked ? Brushes.Gray : Brushes.DimGray, HorizontalContentAlignment = HorizontalAlignment.Left, VerticalContentAlignment = VerticalAlignment.Center };
                    // 元素右键菜单
                    child.ContextMenu = BuildElementTreeContextMenu(el, band);
                    node.Items.Add(child);
                }
                _bandTree.Items.Add(node);
            }
        }

        private ContextMenu BuildBandTreeContextMenu(Band band)
        {
            return BandTreeContextMenuBuilder.Build(
                band,
                onInsertText: () => { _selectedBand = band; InsertElement(NewText()); },
                onInsertFieldBox: () => { _selectedBand = band; InsertElement(NewFieldBox()); },
                onInsertSummaryBox: () => { _selectedBand = band; InsertElement(NewSummaryBox()); },
                onInsertSysVarBox: () => { _selectedBand = band; InsertElement(NewSysVarBox()); },
                onInsertLine: () => { _selectedBand = band; InsertElement(NewLine()); },
                onInsertShape: () => { _selectedBand = band; InsertElement(NewShape()); },
                onInsertImage: () => { _selectedBand = band; InsertElement(NewImage()); },
                onDelete: () => DeleteBand(band));
        }

        private ContextMenu BuildElementTreeContextMenu(ReportElement el, Band band)
        {
            return ElementTreeContextMenuBuilder.Build(
                isLocked: el.Locked,
                onSelect: () => { _selectedElement = el; _selectedBand = band; _selectedElements.Clear(); _selectedElements.Add(el); RefreshUI(); },
                onCopy: () => { _selectedElement = el; _selectedBand = band; CopySelected(); },
                onCut: () => { _selectedElement = el; _selectedBand = band; CutSelected(); },
                onDelete: () => { _selectedElement = el; _selectedBand = band; DeleteSelected(); },
                onRename: () => ShowRenameDialog(el),
                onToggleLock: () => { PushUndo(); el.Locked = !el.Locked; MarkDirty(); RefreshUI(); },
                onBringToFront: () => { _selectedElement = el; _selectedBand = band; MoveElementOrder("front"); },
                onSendToBack: () => { _selectedElement = el; _selectedBand = band; MoveElementOrder("back"); });
        }

        /// <summary>弹出重命名对话框</summary>
        private void ShowRenameDialog(ReportElement el)
        {
            RenameDialog.Show(this, el, name => { PushUndo(); el.Name = string.IsNullOrEmpty(name) ? null : name; MarkDirty(); RefreshUI(); });
        }

        private bool _updatingProps;

        private void UpdatePropertyList()
        {
            if (_updatingProps) return;
            _updatingProps = true;
            try
            {
                UpdatePropertyListCore();
            }
            finally { _updatingProps = false; }
        }

        private void UpdatePropertyListCore()
        {
            _propertyStack.Children.Clear();
            using var ctx = new PropertyRowContext(_propertyStack);
            if (_template == null) return;

            // 更新选中对象标签
            if (_selectedElement != null)
                _selectedObjLabel.Text = ElementTypeName(_selectedElement);
            else if (_selectedBand != null)
                _selectedObjLabel.Text = Name(_selectedBand.Type);
            else
                _selectedObjLabel.Text = "页面";

            // 同步字体工具栏
            if (_selectedElement is TextElement syncT)
            {
                _fontFamilyCombo.Text = syncT.Font.Family;
                _fontSizeCombo.Text = syncT.Font.Size.ToString();
            }

            // 未选中任何 Band / 元素时显示页面级属性
            if (_selectedBand == null && _selectedElement == null)
            {
                PagePropertySectionBuilder.Build(ctx, _template, this);
                return;
            }

            // Band 属性（仅当没有选中元素时才显示）
            if (_selectedBand != null && _selectedElement == null)
            {
                BandPropertySectionBuilder.Build(ctx, _selectedBand, this);
            }

            // 元素属性
            if (_selectedElement != null)
            {
                // 多选批量编辑模式
                if (_selectedElements.Count > 1)
                {
                    UpdateMultiSelectProperties();
                    return;
                }

                PropertySectionBuilder.BuildElementProperties(ctx, _selectedElement, this);
            }
        }

        // ============================== 属性面板辅助 ==============================

        /// <summary>多选批量编辑属性面板</summary>
        private void UpdateMultiSelectProperties()
        {
            using var ctx = new PropertyRowContext(_propertyStack);
            MultiSelectPropertySectionBuilder.Build(ctx, this);
        }

        // ============================== 其他 ==============================

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
                if (item.Tag is Band band)
                {
                    _selectedBand = band;
                    _selectedElement = null;
                }
                else if (item.Tag is ReportElement el)
                {
                    _selectedElement = el;
                    // 找到元素所在 Band
                    if (_template != null)
                        _selectedBand = _template.Bands.FirstOrDefault(b => b.Elements.Contains(el));
                }
                _canvasRenderer.Render(CanvasRenderContextFactory.Build(_template, _zoom, _gridSpacingMm, _showGrid, _gridColor, _vGuides, _hGuides, _snapLinesX, _snapLinesY), _selectedElements, _selectedBand);
                UpdatePropertyList();
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

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!ConfirmDiscard()) e.Cancel = true;
            base.OnClosing(e);
        }

        // ============================== 中英文转换 ==============================

        // ============================== 页面设置弹窗 ==============================

        internal void ShowPageSetupDialog()
        {
            if (_template == null) return;
            PageSetupDialog.Show(this, _template, () => {
                PushUndo(); MarkDirty(); RefreshUI();
            });
        }

        // ============================== 数据源管理 ==============================

        /// <summary>"加载预览数据" 菜单回调: 调用 LoadPreviewDataDialog 弹窗 + 应用副作用。</summary>
        private void OnLoadPreviewDataClicked()
        {
            LoadPreviewDataDialog.Show(this,
                onLoaded: (parsed, fileName) =>
                {
                    _previewData = parsed;
                    _statusText.Text = "已加载预览数据: " + fileName + " (" + (_previewData?.Count ?? 0) + " 个字段)";
                    if (_viewMode == "preview") _previewRenderer.Render(_template!, _zoom, _previewData);
                });
        }

        /// <summary>搜索元素对话框</summary>
        private void SearchElement()
        {
            SearchElementDialog.Show(this, _template,
                onFound: (found, foundBand) =>
                {
                    _selectedElement = found;
                    _selectedBand = foundBand;
                    _selectedElements.Clear();
                    _selectedElements.Add(found);
                    RefreshUI();
                    _statusText.Text = "已找到: " + (found.Name ?? found.GetType().Name);
                },
                onNotFound: () => _statusText.Text = "未找到匹配的元素");
        }

        /// <summary>构建导出数据：如果有预览数据则使用，否则返回空</summary>

        private void ShowGridSettingsDialog()
        {
            GridSettingsDialog.Show(this, _gridSpacingMm,
                newSpacing =>
                {
                    _gridSpacingMm = newSpacing;
                    _canvasRenderer.Render(CanvasRenderContextFactory.Build(_template, _zoom, _gridSpacingMm, _showGrid, _gridColor, _vGuides, _hGuides, _snapLinesX, _snapLinesY), _selectedElements, _selectedBand);
                });
        }

        private void OnShowDataSourceClicked()
        {
            DataSourceDialog.Show(this, _template, () => { PushUndo(); MarkDirty(); });
        }

        /// <summary>数据绑定向导 — 引导用户选择数据源并绑定字段到元素</summary>
        private void OnShowDataBindingWizardClicked()
        {
            if (_template == null || _selectedElement == null)
            {
                _statusText.Text = "请先选中一个元素再打开数据绑定向导";
                return;
            }
            if (_template.DataSources.Count == 0)
            {
                _statusText.Text = "请先添加数据源（文件→数据源）";
                return;
            }
            DataBindingWizardDialog.Show(this, _template, _selectedElement,
                (dsName, fieldName, propChoice, expression) =>
                {
                    PushUndo();
                    if (propChoice == "文本内容 (Text)" && _selectedElement is TextElement txt)
                    {
                        txt.Text = expression;
                    }
                    else if (propChoice == "可见性表达式 (VisibleExpression)")
                    {
                        _selectedElement.VisibleExpression = expression;
                    }
                    MarkDirty();
                    RefreshUI();
                    _statusText.Text = "已绑定: " + expression;
                });
        }

        private void OnShowTemplateParamsClicked()
        {
            if (_template == null) return;
            TemplateParamsDialog.Show(this, _template,
                () => { PushUndo(); MarkDirty(); });
        }

        // ============================== RelayCmd ==============================

        private class RelayCmd : ICommand
        {
            private readonly Action _execute;
            public RelayCmd(Action execute) { _execute = execute; }
            public event EventHandler? CanExecuteChanged { add { } remove { } }
            public bool CanExecute(object? parameter) => true;
            public void Execute(object? parameter) => _execute();
        }
    }
}