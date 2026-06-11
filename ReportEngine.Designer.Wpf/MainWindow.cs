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
            var root = new DockPanel();

            // 菜单
            var menu = BuildMenu();
            menu.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));
            menu.Foreground = Brushes.Black;
            DockPanel.SetDock(menu, Dock.Top);
            root.Children.Add(menu);

            // 工具栏
            var toolbar = BuildToolBar();
            DockPanel.SetDock(toolbar, Dock.Top);
            root.Children.Add(toolbar);

            // 字体格式工具栏
            var fontBar = BuildFontToolBar();
            DockPanel.SetDock(fontBar, Dock.Top);
            root.Children.Add(fontBar);

            // 对齐工具栏
            var alignBar = BuildAlignToolBar();
            DockPanel.SetDock(alignBar, Dock.Top);
            root.Children.Add(alignBar);

            // 状态栏
            var statusBar = BuildStatusBar();
            DockPanel.SetDock(statusBar, Dock.Bottom);
            root.Children.Add(statusBar);

            // 三栏主体
            var mainGrid = new Grid();
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(280) });

            // 左栏
            var leftPanel = BuildLeftPanel();
            Grid.SetColumn(leftPanel, 0);
            mainGrid.Children.Add(leftPanel);

            var sp1 = new GridSplitter { Width = 5, HorizontalAlignment = HorizontalAlignment.Stretch, Background = new SolidColorBrush(Color.FromRgb(210, 210, 210)) };
            Grid.SetColumn(sp1, 1);
            mainGrid.Children.Add(sp1);

            // 中央：标尺 + 画布
            var centerPanel = BuildCenterPanel();
            Grid.SetColumn(centerPanel, 2);
            mainGrid.Children.Add(centerPanel);

            var sp2 = new GridSplitter { Width = 5, HorizontalAlignment = HorizontalAlignment.Stretch, Background = new SolidColorBrush(Color.FromRgb(210, 210, 210)) };
            Grid.SetColumn(sp2, 3);
            mainGrid.Children.Add(sp2);

            // 右栏
            var rightPanel = BuildRightPanel();
            Grid.SetColumn(rightPanel, 4);
            mainGrid.Children.Add(rightPanel);

            root.Children.Add(mainGrid);
            Content = root;
        }

        private ToolBar BuildToolBar()
        {
            var tb = new ToolBar
            {
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                Foreground = Brushes.Black,
                Band = 0,
            };
            tb.Items.Add(MakeToolBtn(" 新建", NewTemplate));
            tb.Items.Add(MakeToolBtn("📂 打开", OpenTemplate));
            tb.Items.Add(MakeToolBtn("💾 保存", () => SaveTemplate(false)));
            tb.Items.Add(new Separator());
            _undoBtn = MakeToolBtn("↩ 撤销", Undo);
            _redoBtn = MakeToolBtn("↪ 重做", Redo);
            tb.Items.Add(_undoBtn);
            tb.Items.Add(_redoBtn);
            tb.Items.Add(new Separator());
            _cutBtn = MakeToolBtn("✂ 剪切", CutSelected);
            _copyBtn = MakeToolBtn("📋 复制", CopySelected);
            _pasteBtn = MakeToolBtn("📌 粘贴", PasteElement);
            _deleteBtn = MakeToolBtn("🗑 删除", DeleteSelected);
            tb.Items.Add(_cutBtn);
            tb.Items.Add(_copyBtn);
            tb.Items.Add(_pasteBtn);
            tb.Items.Add(_deleteBtn);
            tb.Items.Add(new Separator());
            tb.Items.Add(MakeToolBtn("🔍 页面设置", ShowPageSetupDialog));
            tb.Items.Add(new Separator());
            tb.Items.Add(MakeToolBtn("📑 PDF", ExportPdf));
            tb.Items.Add(MakeToolBtn("📊 Excel", ExportExcel));
            tb.Items.Add(new Separator());
            tb.Items.Add(MakeToolBtn("🔍+ 放大", () => { _zoomSlider.Value = Math.Min(400, _zoomSlider.Value + 25); }));
            tb.Items.Add(MakeToolBtn("🔍- 缩小", () => { _zoomSlider.Value = Math.Max(25, _zoomSlider.Value - 25); }));
            tb.Items.Add(MakeToolBtn("🔍 适合", () => { _zoomSlider.Value = 100; }));
            return tb;
        }

        private Border BuildStatusBar()
        {
            var outer = new DockPanel { Height = 26 };

            // 左侧：视图标签
            var tabPanel = new StackPanel { Orientation = Orientation.Horizontal, Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)) };
            var tabDesign = MakeStatusTab("普通视图", true);
            var tabPreview = MakeStatusTab("页面视图", false);
            var tabPageSetup = MakeStatusTab("页面设置", false, ShowPageSetupDialog);
            tabDesign.MouseLeftButtonDown += (_, __) => SwitchView("design", tabDesign, tabPreview);
            tabPreview.MouseLeftButtonDown += (_, __) => SwitchView("preview", tabDesign, tabPreview);
            tabPanel.Children.Add(tabDesign);
            tabPanel.Children.Add(tabPreview);
            tabPanel.Children.Add(tabPageSetup);
            DockPanel.SetDock(tabPanel, Dock.Left);
            outer.Children.Add(tabPanel);

            // 右侧主栏
            var grid = new Grid { Background = new SolidColorBrush(Color.FromRgb(0, 122, 204)) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });

            _statusText.Margin = new Thickness(8, 0, 0, 0);
            Grid.SetColumn(_statusText, 0);
            grid.Children.Add(_statusText);

            _posLabel.Margin = new Thickness(4, 0, 0, 0);
            Grid.SetColumn(_posLabel, 1);
            grid.Children.Add(_posLabel);

            var zoomPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(4, 0, 8, 0), VerticalAlignment = VerticalAlignment.Center };
            zoomPanel.Children.Add(_zoomSlider);
            zoomPanel.Children.Add(new TextBlock { Text = " ", VerticalAlignment = VerticalAlignment.Center });
            zoomPanel.Children.Add(_zoomLabel);
            Grid.SetColumn(zoomPanel, 2);
            grid.Children.Add(zoomPanel);

            outer.Children.Add(grid);
            return new Border { Child = outer };
        }

        private ToolBarTray BuildFontToolBar()
        {
            var tray = new ToolBarTray { Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)) };
            var tb = new ToolBar { Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)), Band = 0 };

            _fontFamilyCombo = new ComboBox { Width = 120, FontSize = 11, IsEditable = true };
            foreach (var f in new[] { "宋体", "黑体", "微软雅黑", "楷体", "仿宋", "Arial", "Times New Roman", "Courier New" })
                _fontFamilyCombo.Items.Add(f);
            _fontFamilyCombo.Text = "宋体";
            _fontFamilyCombo.SelectionChanged += (_, __) => ApplyFontFamily();
            _fontFamilyCombo.LostFocus += (_, __) => ApplyFontFamily();
            tb.Items.Add(_fontFamilyCombo);

            _fontSizeCombo = new ComboBox { Width = 60, FontSize = 11, IsEditable = true };
            foreach (var s in new[] { "8", "9", "10", "10.5", "11", "12", "14", "16", "18", "20", "22", "24", "28", "36", "48", "72" })
                _fontSizeCombo.Items.Add(s);
            _fontSizeCombo.Text = "10";
            _fontSizeCombo.SelectionChanged += (_, __) => ApplyFontSize();
            _fontSizeCombo.LostFocus += (_, __) => ApplyFontSize();
            tb.Items.Add(_fontSizeCombo);

            tb.Items.Add(new Separator());
            tb.Items.Add(MakeFmtBtn("B", FontWeights.Bold, () => ToggleFontBold()));
            tb.Items.Add(MakeFmtBtn("I", FontWeights.Normal, () => ToggleFontItalic(), true));
            tb.Items.Add(MakeFmtBtn("U", FontWeights.Normal, () => ToggleFontUnderline(), false, true));
            tb.Items.Add(new Separator());
            tb.Items.Add(MakeToolBtn("☰ 左", () => SetAlignment(ReportEngine.Core.TextAlignment.Left)));
            tb.Items.Add(MakeToolBtn("☰ 中", () => SetAlignment(ReportEngine.Core.TextAlignment.Center)));
            tb.Items.Add(MakeToolBtn("☰ 右", () => SetAlignment(ReportEngine.Core.TextAlignment.Right)));

            tray.ToolBars.Add(tb);
            return tray;
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
            var tray = new ToolBarTray { Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)) };
            var tb = new ToolBar { Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)), Band = 0 };

            tb.Items.Add(MakeToolBtn("▐▌ 左对齐", () => AlignElements("left")));
            tb.Items.Add(MakeToolBtn("▐▌ 右对齐", () => AlignElements("right")));
            tb.Items.Add(MakeToolBtn("▔ 顶端对齐", () => AlignElements("top")));
            tb.Items.Add(MakeToolBtn("▁ 底端对齐", () => AlignElements("bottom")));
            tb.Items.Add(new Separator());
            tb.Items.Add(MakeToolBtn("┃ 水平居中", () => AlignElements("hcenter")));
            tb.Items.Add(MakeToolBtn("━ 垂直居中", () => AlignElements("vcenter")));
            tb.Items.Add(new Separator());
            tb.Items.Add(MakeToolBtn("↔ 等宽", () => AlignElements("samewidth")));
            tb.Items.Add(MakeToolBtn("↕ 等高", () => AlignElements("sameheight")));
            tb.Items.Add(new Separator());
            tb.Items.Add(MakeToolBtn("▢↑ 置顶", () => MoveElementOrder("front")));
            tb.Items.Add(MakeToolBtn("▢↓ 置底", () => MoveElementOrder("back")));
            tb.Items.Add(new Separator());
            tb.Items.Add(MakeToolBtn("🖌 格式刷", StartFormatPainter));
            tb.Items.Add(MakeToolBtn("🔒 锁定", ToggleLockSelected));
            tb.Items.Add(new Separator());
            tb.Items.Add(MakeToolBtn("🔗 组合", GroupSelected));
            tb.Items.Add(MakeToolBtn("✂ 取消组合", UngroupSelected));

            tray.ToolBars.Add(tb);
            return tray;
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
            string groupId = "grp_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            foreach (var el in targets) el.GroupId = groupId;
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
            foreach (var el in targets) el.GroupId = null;
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
            var src = _formatPainterSource;
            // 复制边框
            target.Border = src.Border != null ? new BorderDef { Width = src.Border.Width, Color = src.Border.Color, Style = src.Border.Style, Top = src.Border.Top, Bottom = src.Border.Bottom, Left = src.Border.Left, Right = src.Border.Right } : null;
            // 复制背景色
            target.BackgroundColor = src.BackgroundColor;
            // 复制TextElement样式
            if (src is TextElement st && target is TextElement tt)
            {
                tt.Font.Family = st.Font.Family;
                tt.Font.Size = st.Font.Size;
                tt.Font.Bold = st.Font.Bold;
                tt.Font.Italic = st.Font.Italic;
                tt.Font.Color = st.Font.Color;
                tt.Alignment = st.Alignment;
            }
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
            if (!System.IO.File.Exists(AutoSavePath)) return;
            var result = MessageBox.Show("发现未保存的草稿文件，是否恢复？\n(" + AutoSavePath + ")", "恢复草稿", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var json = System.IO.File.ReadAllText(AutoSavePath);
                    _template = _parser.Parse(json);
                    _currentFilePath = null;
                    _dirty = false;
                    RefreshUI();
                    _statusText.Text = "已从草稿恢复";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("恢复草稿失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // 用户选择不恢复，删除草稿文件
                try
                {
                    System.IO.File.Delete(AutoSavePath);
                    _statusText.Text = "已忽略草稿文件";
                }
                catch { }
            }
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
            el.BackgroundColor = null;
            el.Border = null;
            el.Rotation = 0;
            el.Opacity = 1.0;
            if (el is TextElement t)
            {
                t.Font.Family = "宋体";
                t.Font.Size = 9;
                t.Font.Bold = false;
                t.Font.Italic = false;
                t.Font.Color = null;
                t.Alignment = ReportEngine.Core.TextAlignment.Left;
                t.CanGrow = false;
            }
            MarkDirty();
            RefreshUI();
            _statusText.Text = "已重置属性为默认值";
        }

        private DockPanel BuildLeftPanel()
        {
            var panel = new DockPanel { Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)) };

            // 标题
            var toolHeader = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
                Padding = new Thickness(6, 3, 6, 3),
                Child = new TextBlock { Text = "插入元素", FontWeight = FontWeights.Bold, Foreground = Brushes.Black, FontSize = 12 },
            };
            DockPanel.SetDock(toolHeader, Dock.Top);
            panel.Children.Add(toolHeader);

            // 插入工具按钮
            var toolbox = new StackPanel { Margin = new Thickness(4) };
            AddToolboxBtn(toolbox, "📝 静态框", () => InsertElement(NewText()), "Text");
            AddToolboxBtn(toolbox, "📊 字段框", () => InsertElement(NewFieldBox()), "Field");
            AddToolboxBtn(toolbox, "Σ 统计框", () => InsertElement(NewSummaryBox()), "Summary");
            AddToolboxBtn(toolbox, "@ 系统变量框", () => InsertElement(NewSysVarBox()), "SysVar");
            AddToolboxBtn(toolbox, "📏 线段", () => InsertElement(NewLine()), "Line");
            AddToolboxBtn(toolbox, "▬ 图形框", () => InsertElement(NewShape()), "Shape");
            AddToolboxBtn(toolbox, "🖼 图象框", () => InsertElement(NewImage()), "Image");
            AddToolboxBtn(toolbox, "▦ 条形码&二维码", () => InsertElement(NewBarcode()), "Barcode");
            AddToolboxBtn(toolbox, "▤ 表格", () => InsertElement(NewTable()), "Table");
            AddToolboxBtn(toolbox, "⊞ 交叉表", () => InsertElement(NewCrossTab()), "CrossTab");
            AddToolboxBtn(toolbox, "📈 图表", () => InsertElement(NewChart()), "Chart");
            AddToolboxBtn(toolbox, "📄 子报表", () => InsertElement(NewSubReport()), "SubReport");

            // 区域插入
            toolbox.Children.Add(new Border { Height = 8 });
            toolbox.Children.Add(new TextBlock { Text = "插入区域", FontWeight = FontWeights.Bold, Foreground = Brushes.Black, Margin = new Thickness(0, 4, 0, 4), FontSize = 11 });
            AddToolboxBtn(toolbox, "▤ 页眉", () => AddBand(BandType.Header, 15));
            AddToolboxBtn(toolbox, "▤ 明细", () => AddBand(BandType.Detail, 10));
            AddToolboxBtn(toolbox, "▤ 页脚", () => AddBand(BandType.Footer, 10));
            AddToolboxBtn(toolbox, "▤ 分组头", () => AddBand(BandType.GroupHeader, 12));
            AddToolboxBtn(toolbox, "▤ 分组尾", () => AddBand(BandType.GroupFooter, 10));
            AddToolboxBtn(toolbox, "▤ 报表头", () => AddBand(BandType.ReportHeader, 20));
            AddToolboxBtn(toolbox, "▤ 报表尾", () => AddBand(BandType.ReportFooter, 10));

            var toolScroll = new ScrollViewer { Content = toolbox, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            panel.Children.Add(toolScroll);

            return panel;
        }

        private Grid BuildCenterPanel()
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(RulerSize) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(RulerSize) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // 左上角
            var corner = new Border { Background = new SolidColorBrush(Color.FromRgb(230, 230, 230)) };
            Grid.SetRow(corner, 0); Grid.SetColumn(corner, 0);
            grid.Children.Add(corner);

            Grid.SetRow(_hRuler, 0); Grid.SetColumn(_hRuler, 1);
            grid.Children.Add(_hRuler);

            Grid.SetRow(_vRuler, 1); Grid.SetColumn(_vRuler, 0);
            grid.Children.Add(_vRuler);

            Grid.SetRow(_scrollViewer, 1); Grid.SetColumn(_scrollViewer, 1);
            _scrollViewer.ScrollChanged += (_, __) => _canvasRenderer.RenderRulers(_template!, _zoom);
            _scrollViewer.PreviewMouseWheel += OnCanvasWheel;
            grid.Children.Add(_scrollViewer);

            // 预览视图（叠加在同一位置）
            Grid.SetRow(_previewScrollViewer, 0); Grid.SetColumn(_previewScrollViewer, 0);
            Grid.SetRowSpan(_previewScrollViewer, 2); Grid.SetColumnSpan(_previewScrollViewer, 2);
            grid.Children.Add(_previewScrollViewer);

            return grid;
        }

        private Grid BuildRightPanel()
        {
            var grid = new Grid { Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)) };
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(180) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(5) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // 上半部分：对象树
            var treePanel = new DockPanel();
            var treeHeader = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
                Padding = new Thickness(6, 3, 6, 3),
                Child = new TextBlock { Text = "报表主对象", FontWeight = FontWeights.Bold, FontSize = 12, Foreground = Brushes.Black },
            };
            DockPanel.SetDock(treeHeader, Dock.Top);
            treePanel.Children.Add(treeHeader);
            treePanel.Children.Add(_bandTree);
            Grid.SetRow(treePanel, 0);
            grid.Children.Add(treePanel);

            // 分隔条
            var splitter = new GridSplitter { Height = 5, HorizontalAlignment = HorizontalAlignment.Stretch, Background = new SolidColorBrush(Color.FromRgb(210, 210, 210)) };
            Grid.SetRow(splitter, 1);
            grid.Children.Add(splitter);

            // 下半部分：属性网格
            var propPanel = new DockPanel();

            // 选中对象名 + 操作图标
            _selectedObjLabel = new TextBlock { Text = "", FontSize = 11, FontWeight = FontWeights.Bold, Foreground = Brushes.Black, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 0, 0) };
            var objNameBar = new DockPanel { Background = new SolidColorBrush(Color.FromRgb(230, 230, 230)), Height = 24 };
            var objIconPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(2, 0, 4, 0) };
            objIconPanel.Children.Add(MakeSmallIconBtn("📎", "属性"));
            objIconPanel.Children.Add(MakeSmallIconBtn("⚡", "事件"));
            DockPanel.SetDock(objIconPanel, Dock.Left);
            objNameBar.Children.Add(objIconPanel);
            objNameBar.Children.Add(_selectedObjLabel);

            // 重置按钮
            var btnReset = new Button { Content = "↺ 重置", Width = 50, Height = 18, FontSize = 9, Margin = new Thickness(0, 0, 4, 0), HorizontalAlignment = HorizontalAlignment.Right };
            btnReset.Click += (_, __) => ResetSelectedProperties();
            DockPanel.SetDock(btnReset, Dock.Right);
            objNameBar.Children.Add(btnReset);

            DockPanel.SetDock(objNameBar, Dock.Top);
            propPanel.Children.Add(objNameBar);

            // 属性分类标题栏
            var propTitleBar = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
                Padding = new Thickness(6, 2, 6, 2),
                BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                BorderThickness = new Thickness(0, 0, 0, 1),
            };
            var propTitleRow = new DockPanel();
            // 排序/分类图标
            var sortIcons = new StackPanel { Orientation = Orientation.Horizontal };
            sortIcons.Children.Add(MakeSmallIconBtn("≡", "分类视图"));
            sortIcons.Children.Add(MakeSmallIconBtn("↕", "字母排序"));
            DockPanel.SetDock(sortIcons, Dock.Left);
            propTitleRow.Children.Add(sortIcons);
            propTitleRow.Children.Add(new TextBlock { Text = "", FontSize = 10 });
            propTitleBar.Child = propTitleRow;
            DockPanel.SetDock(propTitleBar, Dock.Top);
            propPanel.Children.Add(propTitleBar);

            propPanel.Children.Add(_propertyPanel);
            Grid.SetRow(propPanel, 2);
            grid.Children.Add(propPanel);

            return grid;
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
                    int idx = _selectedElement != null ? band.Elements.IndexOf(_selectedElement) : -1;
                    if (reverse)
                        idx = idx <= 0 ? band.Elements.Count - 1 : idx - 1;
                    else
                        idx = idx < 0 || idx >= band.Elements.Count - 1 ? 0 : idx + 1;
                    _selectedElement = band.Elements[idx];
                    _selectedElements.Clear();
                    _selectedElements.Add(_selectedElement);
                    _selectedBand = band;
                    RefreshUI();
                }
                e.Handled = true;
                return;
            }

            if (_selectedElements.Count == 0 && _selectedElement == null) return;
            double step = Keyboard.Modifiers == ModifierKeys.Shift ? 5 : 0.5; // Shift=5mm, 否则 0.5mm
            bool handled = true;
            switch (e.Key)
            {
                case Key.Left:
                    NudgeSelected(-step, 0); break;
                case Key.Right:
                    NudgeSelected(step, 0); break;
                case Key.Up:
                    NudgeSelected(0, -step); break;
                case Key.Down:
                    NudgeSelected(0, step); break;
                default:
                    handled = false; break;
            }
            if (handled) { e.Handled = true; }
        }

        private void NudgeSelected(double dx, double dy)
        {
            PushUndo();
            var targets = _selectedElements.Count > 0 ? _selectedElements : (_selectedElement != null ? new List<ReportElement> { _selectedElement } : new List<ReportElement>());
            foreach (var el in targets)
            {
                if (el.Locked) continue;
                el.X = Math.Max(0, el.X + dx);
                el.Y = Math.Max(0, el.Y + dy);
            }
            MarkDirty();
            RefreshUI();
        }

        // ============================== 画布渲染 ==============================

        private void SwitchView(string mode, Border tabDesign, Border tabPreview)
        {
            _viewMode = mode;
            if (mode == "preview")
            {
                _scrollViewer.Visibility = Visibility.Collapsed;
                _hRuler.Visibility = Visibility.Collapsed;
                _vRuler.Visibility = Visibility.Collapsed;
                _previewScrollViewer.Visibility = Visibility.Visible;
                tabDesign.Background = new SolidColorBrush(Color.FromRgb(220, 220, 220));
                tabPreview.Background = Brushes.White;
                _previewRenderer.Render(_template!, _zoom, _previewData);
            }
            else
            {
                _previewScrollViewer.Visibility = Visibility.Collapsed;
                _scrollViewer.Visibility = Visibility.Visible;
                _hRuler.Visibility = Visibility.Visible;
                _vRuler.Visibility = Visibility.Visible;
                tabDesign.Background = Brushes.White;
                tabPreview.Background = new SolidColorBrush(Color.FromRgb(220, 220, 220));
            }
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
            double mmPx = PixelsPerMm * _zoom;
            if (_draggingHGuide)
            {
                double py = e.GetPosition(_vRuler).Y;
                double offsetY = -_scrollViewer.VerticalOffset + CanvasPadding;
                double mm = Math.Round((py - offsetY) / mmPx, 1);
                if (_draggingGuideIndex >= 0 && _draggingGuideIndex < _hGuides.Count)
                    _hGuides[_draggingGuideIndex] = mm;
            }
            else
            {
                double px = e.GetPosition(_hRuler).X;
                double offsetX = -_scrollViewer.HorizontalOffset + CanvasPadding;
                double mm = Math.Round((px - offsetX) / mmPx, 1);
                if (_draggingGuideIndex >= 0 && _draggingGuideIndex < _vGuides.Count)
                    _vGuides[_draggingGuideIndex] = mm;
            }
            _canvasRenderer.Render(CanvasRenderContextFactory.Build(_template, _zoom, _gridSpacingMm, _showGrid, _gridColor, _vGuides, _hGuides, _snapLinesX, _snapLinesY), _selectedElements, _selectedBand);
        }

        private void OnRulerGuideMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_draggingGuide) return;
            // 如果拖到负数区域（标尺外）则删除
            if (_draggingHGuide)
            {
                if (_draggingGuideIndex >= 0 && _draggingGuideIndex < _hGuides.Count && _hGuides[_draggingGuideIndex] < 0)
                    _hGuides.RemoveAt(_draggingGuideIndex);
                _vRuler.ReleaseMouseCapture();
                _vRuler.MouseMove -= OnRulerGuideMouseMove;
                _vRuler.MouseLeftButtonUp -= OnRulerGuideMouseUp;
            }
            else
            {
                if (_draggingGuideIndex >= 0 && _draggingGuideIndex < _vGuides.Count && _vGuides[_draggingGuideIndex] < 0)
                    _vGuides.RemoveAt(_draggingGuideIndex);
                _hRuler.ReleaseMouseCapture();
                _hRuler.MouseMove -= OnRulerGuideMouseMove;
                _hRuler.MouseLeftButtonUp -= OnRulerGuideMouseUp;
            }
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
            if (_selectedElement != null && !_selectedElement.Locked)
            {
                var hit = _canvas.InputHitTest(pos) as FrameworkElement;
                if (hit?.Tag is string tag && tag.StartsWith("handle_"))
                {
                    _resizeHandle = int.Parse(tag.Substring(7));
                    _dragMode = DragMode.ResizeElement;
                    _dragStart = pos;
                    _dragStartX = _selectedElement.X; _dragStartY = _selectedElement.Y;
                    _dragStartW = _selectedElement.Width; _dragStartH = _selectedElement.Height;
                    _canvas.CaptureMouse();
                    PushUndo();
                    return;
                }
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

            var (band, element) = HitTester.Hit(pos, _template, _zoom, CanvasPadding, PixelsPerMm);

            // 格式刷模式：点击元素应用格式
            if (element != null && _formatPainterActive)
            {
                ApplyFormatToTarget(element);
                StopFormatPainter();
                return;
            }

            if (element != null)
            {
                // Ctrl+Click 多选
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    if (_selectedElements.Contains(element))
                        _selectedElements.Remove(element);
                    else
                        _selectedElements.Add(element);
                    _selectedElement = element;
                    _selectedBand = band;
                }
                else
                {
                    _selectedElements.Clear();
                    _selectedElements.Add(element);
                    _selectedElement = element;
                    _selectedBand = band;
                    if (!element.Locked)
                    {
                        _dragMode = DragMode.MoveElement;
                        _dragStart = pos;
                        _dragStartX = element.X;
                        _dragStartY = element.Y;
                        _canvas.CaptureMouse();
                        PushUndo();
                    }
                }
            }
            else if (band != null)
            {
                _selectedElement = null;
                _selectedElements.Clear();
                _selectedBand = band;
            }
            else
            {
                // 空白区域开始框选
                _selectedElement = null;
                _selectedElements.Clear();
                _selectedBand = null;
                _dragMode = DragMode.MarqueeSelect;
                _dragStart = pos;
                _canvas.CaptureMouse();
            }

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
                    foreach (var el in _selectedElements)
                    {
                        el.X = Math.Max(0, Math.Round((el.X + dx) * 2) / 2);
                        el.Y = Math.Max(0, Math.Round((el.Y + dy) * 2) / 2);
                    }
                    _dragStart = pos;
                }
                else
                {
                    double newX = Math.Max(0, _dragStartX + dx);
                    double newY = Math.Max(0, _dragStartY + dy);
                    if (_snapEnabled && _selectedBand != null)
                    {
                        SnapHelper.Snap(ref newX, ref newY, _selectedElement.Width, _selectedElement.Height, _selectedBand,
                            excludedElements: new[] { _selectedElement }.Concat(_selectedElements).Distinct(),
                            vGuides: _vGuides, hGuides: _hGuides, snapThresholdMm: SnapThresholdMm,
                            snapLinesX: _snapLinesX, snapLinesY: _snapLinesY);
                    }
                    _selectedElement.X = Math.Round(newX * 2) / 2;
                    _selectedElement.Y = Math.Round(newY * 2) / 2;
                }
                _canvasRenderer.Render(CanvasRenderContextFactory.Build(_template, _zoom, _gridSpacingMm, _showGrid, _gridColor, _vGuides, _hGuides, _snapLinesX, _snapLinesY), _selectedElements, _selectedBand);
            }
            else if (_dragMode == DragMode.ResizeElement && _selectedElement != null)
            {
                double dx = (pos.X - _dragStart.X) / mmPx;
                double dy = (pos.Y - _dragStart.Y) / mmPx;
                double newX = _dragStartX, newY = _dragStartY, newW = _dragStartW, newH = _dragStartH;
                switch (_resizeHandle)
                {
                    case 0: newX += dx; newY += dy; newW -= dx; newH -= dy; break;
                    case 1: newY += dy; newH -= dy; break;
                    case 2: newY += dy; newW += dx; newH -= dy; break;
                    case 3: newW += dx; break;
                    case 4: newW += dx; newH += dy; break;
                    case 5: newH += dy; break;
                    case 6: newX += dx; newW -= dx; newH += dy; break;
                    case 7: newX += dx; newW -= dx; break;
                }
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
                double mx = Math.Min(pos.X, _dragStart.X);
                double my = Math.Min(pos.Y, _dragStart.Y);
                double mw = Math.Abs(pos.X - _dragStart.X);
                double mh = Math.Abs(pos.Y - _dragStart.Y);
                _marqueeRect = new Rectangle { Width = mw, Height = mh, Stroke = Brushes.DodgerBlue, StrokeThickness = 1, StrokeDashArray = new DoubleCollection { 4, 2 }, Fill = new SolidColorBrush(Color.FromArgb(30, 30, 144, 255)) };
                Canvas.SetLeft(_marqueeRect, mx);
                Canvas.SetTop(_marqueeRect, my);
                _canvas.Children.Add(_marqueeRect);
            }
        }

        private void OnCanvasMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_dragMode == DragMode.MarqueeSelect)
            {
                // 框选结束：找出框内元素
                var pos = e.GetPosition(_canvas);
                double z = _zoom;
                double mmPx = PixelsPerMm * z;
                double mx = Math.Min(pos.X, _dragStart.X) - CanvasPadding;
                double my = Math.Min(pos.Y, _dragStart.Y) - CanvasPadding;
                double mw = Math.Abs(pos.X - _dragStart.X);
                double mh = Math.Abs(pos.Y - _dragStart.Y);
                var rect = new Rect(mx / mmPx, my / mmPx, mw / mmPx, mh / mmPx);

                _selectedElements.Clear();
                if (_template != null)
                {
                    double bandY = 0;
                    foreach (var band in _template.Bands)
                    {
                        foreach (var el in band.Elements)
                        {
                            var elRect = new Rect(el.X, bandY + el.Y, el.Width, el.Height);
                            if (rect.IntersectsWith(elRect))
                            {
                                _selectedElements.Add(el);
                                _selectedBand = band;
                            }
                        }
                        bandY += band.Height;
                    }
                }
                if (_selectedElements.Count > 0)
                    _selectedElement = _selectedElements[0];

                if (_marqueeRect != null) { _canvas.Children.Remove(_marqueeRect); _marqueeRect = null; }
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
            if (!ConfirmDiscard()) return;
            var dlg = new OpenFileDialog { Filter = "报表模板 (*.rptx)|*.rptx|所有文件|*.*", Title = "打开报表模板" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var json = File.ReadAllText(dlg.FileName);
                    _template = _parser.Parse(json);
                    _currentFilePath = dlg.FileName;
                    _dirty = false;
                    _undoStack.Clear();
                    _redoStack.Clear();
                    _selectedElement = null;
                    _selectedBand = null;
                    RefreshUI();
                    _statusText.Text = "已打开: " + System.IO.Path.GetFileName(dlg.FileName);
                    AddRecentFile(dlg.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("打开失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveTemplate(bool saveAs)
        {
            if (_template == null) return;
            var path = _currentFilePath;
            if (saveAs || string.IsNullOrEmpty(path))
            {
                var dlg = new SaveFileDialog { Filter = "报表模板 (*.rptx)|*.rptx", Title = "保存报表模板" };
                if (!string.IsNullOrEmpty(path)) dlg.FileName = path;
                if (dlg.ShowDialog() != true) return;
                path = dlg.FileName;
            }
            try
            {
                _template.ModifiedAt = DateTime.Now;
                var json = _parser.Serialize(_template);
                File.WriteAllText(path!, json);
                _currentFilePath = path;
                _dirty = false;
                UpdateTitle();
                ClearAutoSave();
                _statusText.Text = "已保存: " + System.IO.Path.GetFileName(path!);
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExportPdf()
        {
            if (_template == null) return;
            
            // 导出前预览确认
            var previewResult = MessageBox.Show(
                "导出前预览:\n" +
                (_previewData != null && _previewData.Count > 0 ? $"数据源: {_previewData.Count} 条记录" : "无预览数据，将导出空模板") + "\n\n是否继续导出？",
                "导出确认",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (previewResult != MessageBoxResult.Yes) return;

            var dlg = new SaveFileDialog { Filter = "PDF 文件 (*.pdf)|*.pdf", Title = "导出PDF", FileName = (_currentFilePath != null ? System.IO.Path.GetFileNameWithoutExtension(_currentFilePath) : "报表") + ".pdf" };
            if (dlg.ShowDialog() != true) return;
            try
            {
                _statusText.Text = "正在导出PDF...";
                var resolver = new FileSystemTemplateResolver(System.IO.Path.GetDirectoryName(_currentFilePath) ?? ".");
                var renderer = new ReportRenderer(resolver);
                // 如果有预览数据，导出时带入
                var data = Build(_previewData);
                var rendered = await renderer.RenderAsync(_template, data);
                var exporter = new PdfSharpExporter();
                exporter.ExportToFile(rendered, dlg.FileName);
                _statusText.Text = "PDF已导出: " + System.IO.Path.GetFileName(dlg.FileName);
                MessageBox.Show("PDF 导出完成！\n" + dlg.FileName, "导出成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("PDF导出失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExportExcel()
        {
            if (_template == null) return;

            // 导出前预览确认
            var previewResult = MessageBox.Show(
                "导出前预览:\n" +
                (_previewData != null && _previewData.Count > 0 ? $"数据源: {_previewData.Count} 条记录" : "无预览数据，将导出空模板") + "\n\n是否继续导出？",
                "导出确认",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (previewResult != MessageBoxResult.Yes) return;

            var dlg = new SaveFileDialog { Filter = "Excel 文件 (*.xlsx)|*.xlsx", Title = "导出Excel", FileName = (_currentFilePath != null ? System.IO.Path.GetFileNameWithoutExtension(_currentFilePath) : "报表") + ".xlsx" };
            if (dlg.ShowDialog() != true) return;
            try
            {
                _statusText.Text = "正在导出Excel...";
                var resolver = new FileSystemTemplateResolver(System.IO.Path.GetDirectoryName(_currentFilePath) ?? ".");
                var renderer = new ReportRenderer(resolver);
                var data = Build(_previewData);
                var rendered = await renderer.RenderAsync(_template, data);
                var exporter = new ClosedXmlExporter();
                exporter.ExportToFile(rendered, dlg.FileName);
                _statusText.Text = "Excel已导出: " + System.IO.Path.GetFileName(dlg.FileName);
                MessageBox.Show("Excel 导出完成！\n" + dlg.FileName, "导出成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Excel导出失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>导出当前画布为PNG图片</summary>
        private void ExportPng()
        {
            if (_template == null) return;
            var dlg = new SaveFileDialog { Filter = "PNG 图片 (*.png)|*.png", Title = "导出图片", FileName = (_currentFilePath != null ? System.IO.Path.GetFileNameWithoutExtension(_currentFilePath) : "报表") + ".png" };
            if (dlg.ShowDialog() != true) return;
            try
            {
                _statusText.Text = "正在导出PNG...";
                // 临时渲染到100%缩放以保证清晰度
                double oldZoom = _zoom;
                _zoom = 1.0;
                _canvasRenderer.Render(CanvasRenderContextFactory.Build(_template, _zoom, _gridSpacingMm, _showGrid, _gridColor, _vGuides, _hGuides, _snapLinesX, _snapLinesY), _selectedElements, _selectedBand);

                // 计算画布实际大小
                double pageW = _template.Page.Width * PixelsPerMm;
                double pageH = _template.Page.Height * PixelsPerMm;
                var bmp = new System.Windows.Media.Imaging.RenderTargetBitmap(
                    (int)(pageW + CanvasPadding * 2),
                    (int)(pageH + CanvasPadding * 2),
                    96, 96,
                    System.Windows.Media.PixelFormats.Pbgra32);
                bmp.Render(_canvas);

                // 保存为PNG
                var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bmp));
                using (var stream = System.IO.File.Create(dlg.FileName))
                    encoder.Save(stream);

                _zoom = oldZoom;
                _canvasRenderer.Render(CanvasRenderContextFactory.Build(_template, _zoom, _gridSpacingMm, _showGrid, _gridColor, _vGuides, _hGuides, _snapLinesX, _snapLinesY), _selectedElements, _selectedBand);
                _statusText.Text = "PNG已导出: " + System.IO.Path.GetFileName(dlg.FileName);
                MessageBox.Show("PNG 导出完成！\n" + dlg.FileName, "导出成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("PNG导出失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>批量导出PDF+Excel到同一目录</summary>
        private async void ExportBatch()
        {
            if (_template == null) return;
            
            // 导出前预览确认
            var previewResult = MessageBox.Show(
                "批量导出预览:\n" +
                (_previewData != null && _previewData.Count > 0 ? $"数据源: {_previewData.Count} 条记录" : "无预览数据，将导出空模板") + "\n\n是否继续导出？",
                "批量导出确认",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (previewResult != MessageBoxResult.Yes) return;

            var dlg = new SaveFileDialog { Filter = "PDF 文件 (*.pdf)|*.pdf", Title = "选择保存位置和文件名", FileName = (_currentFilePath != null ? System.IO.Path.GetFileNameWithoutExtension(_currentFilePath) : "报表") };
            if (dlg.ShowDialog() != true) return;
            
            string baseName = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);
            string dir = System.IO.Path.GetDirectoryName(dlg.FileName)!;
            string pdfPath = System.IO.Path.Combine(dir, baseName + ".pdf");
            string excelPath = System.IO.Path.Combine(dir, baseName + ".xlsx");

            try
            {
                _statusText.Text = "正在批量导出...";
                var resolver = new FileSystemTemplateResolver(System.IO.Path.GetDirectoryName(_currentFilePath) ?? ".");
                var renderer = new ReportRenderer(resolver);
                var data = Build(_previewData);
                var rendered = await renderer.RenderAsync(_template, data);

                // 导出PDF
                var pdfExporter = new PdfSharpExporter();
                pdfExporter.ExportToFile(rendered, pdfPath);

                // 导出Excel
                var excelExporter = new ClosedXmlExporter();
                excelExporter.ExportToFile(rendered, excelPath);

                _statusText.Text = "批量导出完成";
                MessageBox.Show($"批量导出完成！\nPDF: {pdfPath}\nExcel: {excelPath}", "导出成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("批量导出失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            if (_recentFiles.Count == 0)
            {
                var empty = new MenuItem { Header = "(无最近文件)", IsEnabled = false };
                parent.Items.Add(empty);
                return;
            }
            for (int i = 0; i < _recentFiles.Count; i++)
            {
                var fp = _recentFiles[i];
                var mi = new MenuItem { Header = (i + 1) + ". " + System.IO.Path.GetFileName(fp) };
                mi.Click += (_, __) => OpenFileDirect(fp);
                parent.Items.Add(mi);
            }
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
            if (_template == null || !e.Data.GetDataPresent("ElementType")) return;
            var typeName = e.Data.GetData("ElementType") as string;
            if (typeName == null) return;

            var pos = e.GetPosition(_canvas);
            double z = _zoom;
            double mmPx = PixelsPerMm * z;

            // 确定drop位置对应的Band
            var (targetBand, relY) = BandHitTester.FindBandAtY(_template, pos.X, pos.Y, _zoom, CanvasPadding, PixelsPerMm);
            if (targetBand == null) return;

            double relX = (pos.X - CanvasPadding) / mmPx;

            ReportElement? newEl = typeName switch
            {
                "Text" => NewText(),
                "Field" => NewFieldBox(),
                "Summary" => NewSummaryBox(),
                "SysVar" => NewSysVarBox(),
                "Line" => NewLine(),
                "Shape" => NewShape(),
                "Image" => NewImage(),
                "Barcode" => NewBarcode(),
                "Table" => NewTable(),
                "CrossTab" => NewCrossTab(),
                "Chart" => NewChart(),
                "SubReport" => NewSubReport(),
                _ => null
            };
            if (newEl == null) return;
            InsertElementAt(newEl, targetBand, relX, relY);
            _statusText.Text = "已拖入元素: " + typeName;
        }

        private void DeleteSelected()
        {
            if (_template == null) return;
            if (_selectedElements.Count > 0)
            {
                PushUndo();
                foreach (var el in _selectedElements)
                    foreach (var b in _template.Bands)
                        b.Elements.Remove(el);
                _selectedElements.Clear();
                _selectedElement = null;
                MarkDirty();
                RefreshUI();
            }
            else if (_selectedElement != null)
            {
                PushUndo();
                foreach (var b in _template.Bands)
                    b.Elements.Remove(_selectedElement);
                _selectedElement = null;
                MarkDirty();
                RefreshUI();
            }
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
            try
            {
                var t = new ReportTemplate();
                t.Bands.Add(new Band { Elements = { _selectedElement } });
                _clipboardJson = _parser.Serialize(t);
                _statusText.Text = "已复制元素";
            }
            catch { }
        }

        private void PasteElement()
        {
            if (_clipboardJson == null || _template == null) return;
            try
            {
                var t = _parser.Parse(_clipboardJson);
                var el = t.Bands.FirstOrDefault()?.Elements.FirstOrDefault();
                if (el == null) return;
                el.Id = Guid.NewGuid().ToString("N");
                el.X += 2; el.Y += 2;
                InsertElement(el);
                _statusText.Text = "已粘贴元素";
            }
            catch { }
        }

        /// <summary>Ctrl+D 复制并偏移</summary>
        private void DuplicateSelected()
        {
            if (_selectedElement == null || _template == null) return;
            try
            {
                var t = new ReportTemplate();
                t.Bands.Add(new Band { Elements = { _selectedElement } });
                var json = _parser.Serialize(t);
                var t2 = _parser.Parse(json);
                var newEl = t2.Bands.FirstOrDefault()?.Elements.FirstOrDefault();
                if (newEl == null) return;
                newEl.Id = Guid.NewGuid().ToString("N");
                newEl.X += 3; newEl.Y += 3;
                var band = _selectedBand ?? _template.Bands.FirstOrDefault();
                if (band == null) return;
                PushUndo();
                band.Elements.Add(newEl);
                _selectedElement = newEl;
                _selectedElements.Clear();
                _selectedElements.Add(newEl);
                MarkDirty();
                RefreshUI();
                _statusText.Text = "已复制元素 (Ctrl+D)";
            }
            catch { }
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
            if (targets.Count < 2) { _statusText.Text = "请选中多个元素后再对齐"; return; }
            PushUndo();
            var first = targets[0];
            switch (mode)
            {
                case "left":
                    double minX = targets.Min(e => e.X);
                    foreach (var el in targets) el.X = minX;
                    break;
                case "right":
                    double maxR = targets.Max(e => e.X + e.Width);
                    foreach (var el in targets) el.X = maxR - el.Width;
                    break;
                case "top":
                    double minY = targets.Min(e => e.Y);
                    foreach (var el in targets) el.Y = minY;
                    break;
                case "bottom":
                    double maxB = targets.Max(e => e.Y + e.Height);
                    foreach (var el in targets) el.Y = maxB - el.Height;
                    break;
                case "hcenter":
                    double avgCX = targets.Average(e => e.X + e.Width / 2);
                    foreach (var el in targets) el.X = avgCX - el.Width / 2;
                    break;
                case "vcenter":
                    double avgCY = targets.Average(e => e.Y + e.Height / 2);
                    foreach (var el in targets) el.Y = avgCY - el.Height / 2;
                    break;
                case "samewidth":
                    double w = first.Width;
                    foreach (var el in targets) el.Width = w;
                    break;
                case "sameheight":
                    double h = first.Height;
                    foreach (var el in targets) el.Height = h;
                    break;
            }
            MarkDirty();
            RefreshUI();
        }

        private void MoveElementOrder(string direction)
        {
            if (_selectedElement == null || _selectedBand == null) return;
            var list = _selectedBand.Elements;
            int idx = list.IndexOf(_selectedElement);
            if (idx < 0) return;
            PushUndo();
            switch (direction)
            {
                case "front":
                    list.Remove(_selectedElement);
                    list.Add(_selectedElement);
                    break;
                case "back":
                    list.Remove(_selectedElement);
                    list.Insert(0, _selectedElement);
                    break;
                case "up":
                    if (idx < list.Count - 1) { list.Remove(_selectedElement); list.Insert(idx + 1, _selectedElement); }
                    break;
                case "down":
                    if (idx > 0) { list.Remove(_selectedElement); list.Insert(idx - 1, _selectedElement); }
                    break;
            }
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
                    string elLabel;
                    // 优先显示自定义名称
                    if (!string.IsNullOrEmpty(el.Name))
                        elLabel = el.Name!;
                    else
                        switch (el) { case TextElement te: elLabel = BoxTypeToCN(te.BoxType) + ": " + GetTextElLabel(te); break; case LineElement _: elLabel = "线段"; break; case ImageElement _: elLabel = "图象框"; break; case ShapeElement _: elLabel = "图形框"; break; case SubReportElement _: elLabel = "子报表"; break; case BarcodeElement _: elLabel = "条码"; break; case TableElement _: elLabel = "表格"; break; case CrossTabElement _: elLabel = "交叉表"; break; default: elLabel = el.GetType().Name; break; }
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
            var menu = new ContextMenu();
            var miInsert = new MenuItem { Header = "插入元素" };
            miInsert.Items.Add(MakeMenuItem("静态框", null, () => { _selectedBand = band; InsertElement(NewText()); }));
            miInsert.Items.Add(MakeMenuItem("字段框", null, () => { _selectedBand = band; InsertElement(NewFieldBox()); }));
            miInsert.Items.Add(MakeMenuItem("统计框", null, () => { _selectedBand = band; InsertElement(NewSummaryBox()); }));
            miInsert.Items.Add(MakeMenuItem("系统变量框", null, () => { _selectedBand = band; InsertElement(NewSysVarBox()); }));
            miInsert.Items.Add(new Separator());
            miInsert.Items.Add(MakeMenuItem("线段", null, () => { _selectedBand = band; InsertElement(NewLine()); }));
            miInsert.Items.Add(MakeMenuItem("图形框", null, () => { _selectedBand = band; InsertElement(NewShape()); }));
            miInsert.Items.Add(MakeMenuItem("图象框", null, () => { _selectedBand = band; InsertElement(NewImage()); }));
            menu.Items.Add(miInsert);
            menu.Items.Add(new Separator());
            var miDel = new MenuItem { Header = "删除区域 [" + Name(band.Type) + "]" };
            miDel.Click += (_, __) => DeleteBand(band);
            menu.Items.Add(miDel);
            return menu;
        }

        private ContextMenu BuildElementTreeContextMenu(ReportElement el, Band band)
        {
            var menu = new ContextMenu();
            var miSelect = new MenuItem { Header = "选中" };
            miSelect.Click += (_, __) => { _selectedElement = el; _selectedBand = band; _selectedElements.Clear(); _selectedElements.Add(el); RefreshUI(); };
            menu.Items.Add(miSelect);
            menu.Items.Add(new Separator());
            var miCopy = new MenuItem { Header = "复制" };
            miCopy.Click += (_, __) => { _selectedElement = el; _selectedBand = band; CopySelected(); };
            menu.Items.Add(miCopy);
            var miCut = new MenuItem { Header = "剪切" };
            miCut.Click += (_, __) => { _selectedElement = el; _selectedBand = band; CutSelected(); };
            menu.Items.Add(miCut);
            var miDel = new MenuItem { Header = "删除" };
            miDel.Click += (_, __) => { _selectedElement = el; _selectedBand = band; DeleteSelected(); };
            menu.Items.Add(miDel);
            menu.Items.Add(new Separator());
            var miRename = new MenuItem { Header = "重命名" };
            miRename.Click += (_, __) => ShowRenameDialog(el);
            menu.Items.Add(miRename);
            var miLock = new MenuItem { Header = el.Locked ? "解锁" : "锁定" };
            miLock.Click += (_, __) => { PushUndo(); el.Locked = !el.Locked; MarkDirty(); RefreshUI(); };
            menu.Items.Add(miLock);
            menu.Items.Add(new Separator());
            var miTop = new MenuItem { Header = "置于顶层" };
            miTop.Click += (_, __) => { _selectedElement = el; _selectedBand = band; MoveElementOrder("front"); };
            menu.Items.Add(miTop);
            var miBottom = new MenuItem { Header = "置于底层" };
            miBottom.Click += (_, __) => { _selectedElement = el; _selectedBand = band; MoveElementOrder("back"); };
            menu.Items.Add(miBottom);
            return menu;
        }

        /// <summary>弹出重命名对话框</summary>
        private void ShowRenameDialog(ReportElement el)
        {
            var dlg = new Window
            {
                Title = "重命名元素",
                Width = 300, Height = 120,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this, ResizeMode = ResizeMode.NoResize,
            };
            var sp = new StackPanel { Margin = new Thickness(12) };
            sp.Children.Add(new TextBlock { Text = "名称:", Margin = new Thickness(0, 0, 0, 4), Foreground = Brushes.Black });
            var tb = new TextBox { Text = el.Name ?? "", Margin = new Thickness(0, 0, 0, 8), Foreground = Brushes.Black };
            sp.Children.Add(tb);
            var btnOk = new Button { Content = "确定", Width = 70, Height = 26, HorizontalAlignment = HorizontalAlignment.Right, IsDefault = true };
            btnOk.Click += (_, __) => { PushUndo(); el.Name = string.IsNullOrEmpty(tb.Text) ? null : tb.Text; MarkDirty(); RefreshUI(); dlg.Close(); };
            sp.Children.Add(btnOk);
            dlg.Content = sp;
            dlg.ShowDialog();
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
                var band = _selectedBand;
                ctx.AddSection("设计");
                ctx.AddLabel("类型", Name(band.Type));
                ctx.AddLabel("标识", band.Type.ToString());

                ctx.AddSection("外观");
                ctx.AddEditor(this, "高度(mm)", band.Height.ToString(), v => { if (double.TryParse(v, out var d) && d > 0) { PushUndo(); band.Height = d; MarkDirty(); RefreshUI(); } });

                ctx.AddSection("行为");
                ctx.AddBool("重复每页", band.RepeatOnNewPage, v => { PushUndo(); band.RepeatOnNewPage = v; MarkDirty(); });

                ctx.AddSection("其它");
                ctx.AddEditor(this, "数据源", band.DataSource ?? "", v => { PushUndo(); band.DataSource = string.IsNullOrEmpty(v) ? null : v; MarkDirty(); });
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

            var menu = new ContextMenu();
            if (_selectedElement != null)
            {
                var miCut = new MenuItem { Header = "剪切" }; miCut.Click += (_, __) => CutSelected(); menu.Items.Add(miCut);
                var miCopy = new MenuItem { Header = "复制" }; miCopy.Click += (_, __) => CopySelected(); menu.Items.Add(miCopy);
                var miDel = new MenuItem { Header = "删除" }; miDel.Click += (_, __) => DeleteSelected(); menu.Items.Add(miDel);
                menu.Items.Add(new Separator());
            }
            if (_clipboardJson != null)
            {
                var miPaste = new MenuItem { Header = "粘贴" }; miPaste.Click += (_, __) => PasteElement(); menu.Items.Add(miPaste);
            }
            menu.Items.Add(new Separator());

            var miInsert = new MenuItem { Header = "插入" };
            miInsert.Items.Add(MakeMenuItem("静态框", null, () => InsertElement(NewText())));
            miInsert.Items.Add(MakeMenuItem("字段框", null, () => InsertElement(NewFieldBox())));
            miInsert.Items.Add(MakeMenuItem("统计框", null, () => InsertElement(NewSummaryBox())));
            miInsert.Items.Add(MakeMenuItem("系统变量框", null, () => InsertElement(NewSysVarBox())));
            miInsert.Items.Add(new Separator());
            miInsert.Items.Add(MakeMenuItem("线段", null, () => InsertElement(NewLine())));
            miInsert.Items.Add(MakeMenuItem("图形框", null, () => InsertElement(NewShape())));
            miInsert.Items.Add(MakeMenuItem("图象框", null, () => InsertElement(NewImage())));
            miInsert.Items.Add(MakeMenuItem("条形码&二维码", null, () => InsertElement(NewBarcode())));
            miInsert.Items.Add(MakeMenuItem("表格", null, () => InsertElement(NewTable())));
            miInsert.Items.Add(MakeMenuItem("交叉表", null, () => InsertElement(NewCrossTab())));
            miInsert.Items.Add(MakeMenuItem("图表", null, () => InsertElement(NewChart())));
            miInsert.Items.Add(MakeMenuItem("子报表", null, () => InsertElement(NewSubReport())));
            menu.Items.Add(miInsert);

            var miBand = new MenuItem { Header = "插入区域" };
            miBand.Items.Add(MakeMenuItem("页眉", null, () => AddBand(BandType.Header, 15)));
            miBand.Items.Add(MakeMenuItem("明细", null, () => AddBand(BandType.Detail, 10)));
            miBand.Items.Add(MakeMenuItem("页脚", null, () => AddBand(BandType.Footer, 10)));
            miBand.Items.Add(MakeMenuItem("分组头", null, () => AddBand(BandType.GroupHeader, 12)));
            miBand.Items.Add(MakeMenuItem("分组尾", null, () => AddBand(BandType.GroupFooter, 10)));
            miBand.Items.Add(MakeMenuItem("报表头", null, () => AddBand(BandType.ReportHeader, 20)));
            miBand.Items.Add(MakeMenuItem("报表尾", null, () => AddBand(BandType.ReportFooter, 10)));
            menu.Items.Add(miBand);

            // 删除当前区域
            if (_selectedBand != null)
            {
                var miDelBand = new MenuItem { Header = "删除区域 [" + Name(_selectedBand.Type) + "]" };
                var delB = _selectedBand;
                miDelBand.Click += (_, __) => DeleteBand(delB);
                menu.Items.Add(miDelBand);
            }

            menu.Items.Add(new Separator());
            var miPage = new MenuItem { Header = "页面设置..." }; miPage.Click += (_, __) => ShowPageSetupDialog(); menu.Items.Add(miPage);

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