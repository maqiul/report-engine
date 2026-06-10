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
            _zoomSlider.ValueChanged += (_, __) => { _zoom = _zoomSlider.Value / 100.0; _zoomLabel.Text = (int)_zoomSlider.Value + "%"; _canvasRenderer.Render(BuildCanvasRenderContext(), _selectedElements, _selectedBand); _canvasRenderer.RenderRulers(_template!, _zoom); };

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
            file.Items.Add(MakeMenuItem("数据源(_S)...", null, ShowDataSourceDialog));
            file.Items.Add(MakeMenuItem("数据绑定向导(_B)...", null, ShowDataBindingWizard));
            file.Items.Add(MakeMenuItem("模板参数(_P)...", null, ShowTemplateParamsDialog));
            file.Items.Add(MakeMenuItem("加载预览数据(_L)...", null, LoadPreviewData));
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
            gridItem.Click += (_, __) => { _showGrid = gridItem.IsChecked; _canvasRenderer.Render(BuildCanvasRenderContext(), _selectedElements, _selectedBand); };
            view.Items.Add(gridItem);
            view.Items.Add(MakeMenuItem("网格设置...", null, ShowGridSettingsDialog));
            view.Items.Add(new Separator());
            var miClearGuides = new MenuItem { Header = "清除所有参考线" };
            miClearGuides.Click += (_, __) => { _hGuides.Clear(); _vGuides.Clear(); _canvasRenderer.Render(BuildCanvasRenderContext(), _selectedElements, _selectedBand); _statusText.Text = "已清除所有参考线"; };
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
            help.Items.Add(MakeMenuItem("快捷键...", "F1", ShowShortcutsDialog));
            help.Items.Add(MakeMenuItem("关于...", null, ShowAboutDialog));
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
            InputBindings.Add(new KeyBinding(new RelayCmd(ShowShortcutsDialog), Key.F1, ModifierKeys.None));
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
            _canvasRenderer.Render(BuildCanvasRenderContext(), _selectedElements, _selectedBand);
            _statusText.Text = "垂直参考线: " + mm + "mm (拖出标尺外删除)";
        }

        private CanvasRenderContext BuildCanvasRenderContext()
        {
            if (_template == null)
                return null!; // 调用方均已判 _template 非空，这里不抛
            // ShowMargins 原 MainWindow 无 toggle，始终为 true（margin 永远绘制以保留行为）
            return new CanvasRenderContext(
                _template, _zoom, _gridSpacingMm, _showGrid,
                showMargins: true, _gridColor,
                _vGuides, _hGuides, _snapLinesX, _snapLinesY);
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
            _canvasRenderer.Render(BuildCanvasRenderContext(), _selectedElements, _selectedBand);
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
            _canvasRenderer.Render(BuildCanvasRenderContext(), _selectedElements, _selectedBand);
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
            _canvasRenderer.Render(BuildCanvasRenderContext(), _selectedElements, _selectedBand);
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

            HitTest(pos, out var band, out var element);

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
            HitTest(pos, out _, out var element);
            if (element is TextElement txt)
            {
                // 双击文本元素：弹出编辑框
                var input = new Window
                {
                    Title = "编辑文本",
                    Width = 320, Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this, ResizeMode = ResizeMode.NoResize,
                };
                var tb = new TextBox { Text = txt.Text ?? "", FontSize = 13, Margin = new Thickness(12), AcceptsReturn = true, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
                var btnOk = new Button { Content = "确定", Width = 60, Height = 26, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 0, 12, 8), IsDefault = true };
                btnOk.Click += (_, __) => { PushUndo(); txt.Text = tb.Text; MarkDirty(); RefreshUI(); input.DialogResult = true; };
                var sp = new StackPanel();
                sp.Children.Add(tb);
                sp.Children.Add(btnOk);
                input.Content = sp;
                input.ShowDialog();
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
                        SnapPosition(ref newX, ref newY, _selectedElement.Width, _selectedElement.Height, _selectedBand);
                    }
                    _selectedElement.X = Math.Round(newX * 2) / 2;
                    _selectedElement.Y = Math.Round(newY * 2) / 2;
                }
                _canvasRenderer.Render(BuildCanvasRenderContext(), _selectedElements, _selectedBand);
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
                _canvasRenderer.Render(BuildCanvasRenderContext(), _selectedElements, _selectedBand);
            }
            else if (_dragMode == DragMode.ResizeBandHeight && _selectedBand != null)
            {
                double dy = (pos.Y - _dragStart.Y) / mmPx;
                _selectedBand.Height = Math.Max(3, Math.Round((_dragStartH + dy) * 2) / 2);
                _canvasRenderer.Render(BuildCanvasRenderContext(), _selectedElements, _selectedBand);
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

        /// <summary>吸附对齐：检查当前元素边缘/中线是否接近其他元素边缘/中线或参考线</summary>
        private void SnapPosition(ref double x, ref double y, double w, double h, Band band)
        {
            // 收集吸附线: 其他元素的边缘和中线
            var xSnaps = new List<double>();
            var ySnaps = new List<double>();

            foreach (var other in band.Elements)
            {
                if (other == _selectedElement) continue;
                if (_selectedElements.Contains(other)) continue;
                xSnaps.Add(other.X);                    // 左边
                xSnaps.Add(other.X + other.Width / 2);  // 中线
                xSnaps.Add(other.X + other.Width);      // 右边
                ySnaps.Add(other.Y);
                ySnaps.Add(other.Y + other.Height / 2);
                ySnaps.Add(other.Y + other.Height);
            }

            // 加入参考线
            foreach (var g in _vGuides) xSnaps.Add(g);
            foreach (var g in _hGuides) ySnaps.Add(g);

            // 当前元素的3个吸附点
            double[] myXs = { x, x + w / 2, x + w };
            double[] myYs = { y, y + h / 2, y + h };

            double bestDx = double.MaxValue;
            double snapX = x;
            foreach (var sx in xSnaps)
            {
                foreach (var mx in myXs)
                {
                    double d = Math.Abs(mx - sx);
                    if (d < SnapThresholdMm && d < Math.Abs(bestDx))
                    {
                        bestDx = mx - sx;
                        snapX = sx;
                    }
                }
            }
            if (bestDx != double.MaxValue)
            {
                x -= bestDx;
                _snapLinesX.Add(snapX);
            }

            double bestDy = double.MaxValue;
            double snapY = y;
            foreach (var sy in ySnaps)
            {
                foreach (var my in myYs)
                {
                    double d = Math.Abs(my - sy);
                    if (d < SnapThresholdMm && d < Math.Abs(bestDy))
                    {
                        bestDy = my - sy;
                        snapY = sy;
                    }
                }
            }
            if (bestDy != double.MaxValue)
            {
                y -= bestDy;
                _snapLinesY.Add(snapY);
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

        private void HitTest(Point pos, out Band? hitBand, out ReportElement? hitElement)
        {
            hitBand = null;
            hitElement = null;
            if (_template == null) return;
            double z = _zoom;
            double mmPx = PixelsPerMm * z;
            double currentY = CanvasPadding;
            foreach (var band in _template.Bands)
            {
                double bandH = band.Height * mmPx;
                double bandTop = currentY;
                double bandBot = currentY + bandH;
                if (pos.Y >= bandTop && pos.Y <= bandBot)
                {
                    hitBand = band;
                    // 反序遍历，上层元素优先
                    for (int i = band.Elements.Count - 1; i >= 0; i--)
                    {
                        var el = band.Elements[i];
                        double ex = CanvasPadding + el.X * mmPx;
                        double ey = bandTop + el.Y * mmPx;
                        double ew = el.Width * mmPx;
                        double eh = el.Height * mmPx;
                        if (pos.X >= ex && pos.X <= ex + ew && pos.Y >= ey && pos.Y <= ey + eh)
                        {
                            hitElement = el;
                            return;
                        }
                    }
                    return;
                }
                currentY += bandH;
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
                var data = BuildExportData();
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
                var data = BuildExportData();
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
                _canvasRenderer.Render(BuildCanvasRenderContext(), _selectedElements, _selectedBand);

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
                _canvasRenderer.Render(BuildCanvasRenderContext(), _selectedElements, _selectedBand);
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
                var data = BuildExportData();
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
            double bandY = CanvasPadding;
            Band? targetBand = null;
            double relY = 0;
            foreach (var band in _template.Bands)
            {
                double bh = band.Height * mmPx;
                if (pos.Y >= bandY && pos.Y < bandY + bh)
                {
                    targetBand = band;
                    relY = (pos.Y - bandY) / mmPx;
                    break;
                }
                bandY += bh;
            }
            if (targetBand == null) targetBand = _template.Bands.LastOrDefault();
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

        private void PushUndo()
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

        private void RefreshUI()
        {
            _canvasRenderer.Render(BuildCanvasRenderContext(), _selectedElements, _selectedBand);
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

        private int _propRowIndex;

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
            _currentExpander = null;
            _propRowIndex = 0;
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
                AddPropSection("页面");
                AddPropLabel("纸张", _template.Page.Width + " × " + _template.Page.Height + " mm");
                AddPropLabel("方向", _template.Page.Orientation == "landscape" ? "横向" : "纵向");
                AddPropLabel("边距", "上" + _template.Page.Margin.Top + " 下" + _template.Page.Margin.Bottom + " 左" + _template.Page.Margin.Left + " 右" + _template.Page.Margin.Right);
                AddPropColor("背景色", _template.Page.BackgroundColor ?? "", v => { PushUndo(); _template.Page.BackgroundColor = string.IsNullOrEmpty(v) ? null : v; MarkDirty(); RefreshUI(); });
                AddPropEditor("背景图片", _template.Page.BackgroundImage ?? "", v => { PushUndo(); _template.Page.BackgroundImage = string.IsNullOrEmpty(v) ? null : v; MarkDirty(); RefreshUI(); });
                AddPropEditor("水印文字", _template.Page.Watermark ?? "", v => { PushUndo(); _template.Page.Watermark = string.IsNullOrEmpty(v) ? null : v; MarkDirty(); RefreshUI(); });
                
                // 多联打印 - 分组卡片式
                AddPropSection("多联打印");
                var muInfo = _template.Page.MultiUp;
                if (muInfo != null)
                {
                    // 多联信息卡片
                    var muCard = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(245, 250, 245)),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(100, 180, 100)),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(4),
                        Padding = new Thickness(8),
                        Margin = new Thickness(0, 2, 0, 8)
                    };
                    var muStack = new StackPanel();
                    
                    // 标题行
                    var muTitle = new DockPanel { Margin = new Thickness(0, 0, 0, 6) };
                    muTitle.Children.Add(new TextBlock { Text = "✅ 多联打印已启用", FontWeight = FontWeights.Bold, Foreground = Brushes.DarkGreen, FontSize = 11 });
                    muStack.Children.Add(muTitle);
                    
                    // 配置信息网格
                    var muGrid = new Grid();
                    muGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
                    muGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    
                    int muRow = 0;
                    void AddMuRow(string label, string value)
                    {
                        muGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(22) });
                        var lblTb = new TextBlock { Text = label + ":", Foreground = Brushes.Gray, FontSize = 10, VerticalAlignment = VerticalAlignment.Center };
                        muGrid.Children.Add(lblTb);
                        Grid.SetRow(lblTb, muRow);
                        var valTb = new TextBlock { Text = value, Foreground = Brushes.Black, FontSize = 10, FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center };
                        Grid.SetColumn(valTb, 1);
                        Grid.SetRow(valTb, muRow);
                        muGrid.Children.Add(valTb);
                        muRow++;
                    }
                    
                    AddMuRow("布局", $"{muInfo.Rows} 行 × {muInfo.Columns} 列 = {muInfo.Count} 份/页");
                    
                    // 打印顺序 - 可切换
                    var muDirRow = muRow;
                    muGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(22) });
                    var dirLbl = new TextBlock { Text = "打印顺序:", Foreground = Brushes.Gray, FontSize = 10, VerticalAlignment = VerticalAlignment.Center };
                    muGrid.Children.Add(dirLbl);
                    Grid.SetRow(dirLbl, muDirRow);
                    
                    // 值 + 按钮放在 DockPanel 中
                    var dirPanel = new DockPanel();
                    var dirValue = new TextBlock { Text = muInfo.Direction == "Vertical" ? "先列后行 (垂直)" : "先行后列 (水平/Z字形)", Foreground = Brushes.Black, FontSize = 10, FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center };
                    DockPanel.SetDock(dirValue, Dock.Left);
                    dirPanel.Children.Add(dirValue);
                    
                    // 切换按钮
                    var dirToggleBtn = new Button { Content = "⇄ 切换", Width = 50, Height = 18, FontSize = 9, Margin = new Thickness(8, 0, 0, 0) };
                    dirToggleBtn.Click += (_, __) =>
                    {
                        PushUndo();
                        muInfo.Direction = muInfo.Direction == "Vertical" ? "Horizontal" : "Vertical";
                        dirValue.Text = muInfo.Direction == "Vertical" ? "先列后行 (垂直)" : "先行后列 (水平/Z字形)";
                        MarkDirty();
                        RefreshUI();
                    };
                    DockPanel.SetDock(dirToggleBtn, Dock.Left);
                    dirPanel.Children.Add(dirToggleBtn);
                    
                    Grid.SetColumn(dirPanel, 1);
                    Grid.SetRow(dirPanel, muDirRow);
                    muGrid.Children.Add(dirPanel);
                    muRow++;
                    
                    AddMuRow("间距", $"水平 {muInfo.HSpacing}mm  垂直 {muInfo.VSpacing}mm");
                    
                    // 计算单联尺寸
                    double cw = (_template.Page.Width - muInfo.HSpacing * (muInfo.Columns - 1)) / muInfo.Columns;
                    double ch = (_template.Page.Height - muInfo.VSpacing * (muInfo.Rows - 1)) / muInfo.Rows;
                    AddMuRow("单联尺寸", $"{Math.Round(cw, 1)} × {Math.Round(ch, 1)} mm");
                    
                    muStack.Children.Add(muGrid);
                    muCard.Child = muStack;
                    _propertyStack.Children.Add(muCard);
                    
                    // 修改按钮
                    var muEditBtn = new Button
                    {
                        Content = "⚙ 修改配置",
                        Width = 100,
                        Height = 24,
                        Margin = new Thickness(0, 0, 0, 4),
                        FontSize = 10,
                        HorizontalAlignment = HorizontalAlignment.Right
                    };
                    muEditBtn.Click += (_, __) => ShowPageSetupDialog();
                    _propertyStack.Children.Add(muEditBtn);
                }
                else
                {
                    // 未启用提示卡片
                    var muCard = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(250, 250, 250)),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(4),
                        Padding = new Thickness(8),
                        Margin = new Thickness(0, 2, 0, 8)
                    };
                    var muStack = new StackPanel();
                    muStack.Children.Add(new TextBlock { Text = "多联打印未启用", Foreground = Brushes.Gray, FontSize = 10, Margin = new Thickness(0, 0, 0, 4) });
                    
                    var muEnableBtn = new Button
                    {
                        Content = "+ 启用多联打印",
                        Width = 100,
                        Height = 24,
                        FontSize = 10,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Foreground = Brushes.DarkBlue
                    };
                    muEnableBtn.Click += (_, __) => ShowPageSetupDialog();
                    muStack.Children.Add(muEnableBtn);
                    
                    muCard.Child = muStack;
                    _propertyStack.Children.Add(muCard);
                }

                AddPropSection("元数据");
                AddPropEditor("作者", _template.Author ?? "", v => { PushUndo(); _template.Author = string.IsNullOrEmpty(v) ? null : v; MarkDirty(); });
                AddPropEditor("描述", _template.Description ?? "", v => { PushUndo(); _template.Description = string.IsNullOrEmpty(v) ? null : v; MarkDirty(); });
                AddPropLabel("创建时间", _template.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
                AddPropLabel("修改时间", _template.ModifiedAt.ToString("yyyy-MM-dd HH:mm"));
                return;
            }

            // Band 属性（仅当没有选中元素时才显示）
            if (_selectedBand != null && _selectedElement == null)
            {
                var band = _selectedBand;
                AddPropSection("设计");
                AddPropLabel("类型", Name(band.Type));
                AddPropLabel("标识", band.Type.ToString());

                AddPropSection("外观");
                AddPropEditor("高度(mm)", band.Height.ToString(), v => { if (double.TryParse(v, out var d) && d > 0) { PushUndo(); band.Height = d; MarkDirty(); RefreshUI(); } });

                AddPropSection("行为");
                AddPropBool("重复每页", band.RepeatOnNewPage, v => { PushUndo(); band.RepeatOnNewPage = v; MarkDirty(); });

                AddPropSection("其它");
                AddPropEditor("数据源", band.DataSource ?? "", v => { PushUndo(); band.DataSource = string.IsNullOrEmpty(v) ? null : v; MarkDirty(); });
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

                var el = _selectedElement;

                // === 设计 ===
                AddPropSection("设计");
                AddPropLabel("类型", ElementTypeName(el));
                AddPropLabel("标识", el.Id ?? "");
                AddPropEditor("名称", el.Name ?? "", v => { PushUndo(); el.Name = string.IsNullOrEmpty(v) ? null : v; MarkDirty(); RefreshUI(); });
                AddPropBool("可见", el.Visible, v => { PushUndo(); el.Visible = v; MarkDirty(); });
                AddPropBool("锁定", el.Locked, v => { PushUndo(); el.Locked = v; MarkDirty(); RefreshUI(); });
                AddPropEditor("旋转(°)", el.Rotation.ToString(), v => { if (double.TryParse(v, out var d)) { PushUndo(); el.Rotation = d % 360; MarkDirty(); RefreshUI(); } });
                AddPropEditor("透明度", el.Opacity.ToString("F2"), v => { if (double.TryParse(v, out var d)) { PushUndo(); el.Opacity = Math.Max(0, Math.Min(1, d)); MarkDirty(); RefreshUI(); } });

                // === 外观 ===
                AddPropSection("外观");
                AddPropColor("背景色", el.BackgroundColor ?? "", v => { PushUndo(); el.BackgroundColor = string.IsNullOrEmpty(v) ? null : v; MarkDirty(); RefreshUI(); });

                // 边框属性
                AddPropSection("边框");
                var border = el.Border ?? new BorderDef();
                AddPropEditor("边框宽", border.Width.ToString(), v =>
                {
                    if (double.TryParse(v, out var d) && d >= 0) { PushUndo(); EnsureBorder(el).Width = d; MarkDirty(); RefreshUI(); }
                });
                AddPropColor("边框色", border.Color ?? "#000000", v =>
                {
                    PushUndo(); EnsureBorder(el).Color = string.IsNullOrEmpty(v) ? "#000000" : v; MarkDirty(); RefreshUI();
                });
                AddPropCombo("边框样式", new[] { "Solid", "Dashed", "Dotted", "None" }, border.Style.ToString(), v =>
                {
                    PushUndo();
                    if (Enum.TryParse<BorderStyle>(v, out var bs)) EnsureBorder(el).Style = bs;
                    MarkDirty(); RefreshUI();
                });
                AddPropBool("上边框", border.Top, v => { PushUndo(); EnsureBorder(el).Top = v; MarkDirty(); RefreshUI(); });
                AddPropBool("下边框", border.Bottom, v => { PushUndo(); EnsureBorder(el).Bottom = v; MarkDirty(); RefreshUI(); });
                AddPropBool("左边框", border.Left, v => { PushUndo(); EnsureBorder(el).Left = v; MarkDirty(); RefreshUI(); });
                AddPropBool("右边框", border.Right, v => { PushUndo(); EnsureBorder(el).Right = v; MarkDirty(); RefreshUI(); });

                switch (el)
                {
                    case TextElement t:
                        AddPropFontRow(t);
                        AddPropColor("字体色", t.Font.Color ?? "", v => { PushUndo(); t.Font.Color = string.IsNullOrEmpty(v) ? null : v; MarkDirty(); RefreshUI(); });
                        AddPropCombo("对齐", new[] { "左对齐", "居中", "右对齐", "两端对齐" }, AlignToCN(t.Alignment.ToString()), v => { var a = CNToAlign(v); PushUndo(); t.Alignment = a; MarkDirty(); RefreshUI(); });
                        break;
                    case LineElement l:
                        AddPropEditor("线宽", l.LineWidth.ToString(), v => { if (double.TryParse(v, out var d)) { PushUndo(); l.LineWidth = d; MarkDirty(); RefreshUI(); } });
                        AddPropColor("线色", l.LineColor, v => { PushUndo(); l.LineColor = v; MarkDirty(); RefreshUI(); });
                        break;
                    case ShapeElement s:
                        AddPropColor("填充色", s.FillColor, v => { PushUndo(); s.FillColor = v; MarkDirty(); RefreshUI(); });
                        AddPropEditor("圆角", s.BorderRadius.ToString(), v => { if (double.TryParse(v, out var d)) { PushUndo(); s.BorderRadius = d; MarkDirty(); RefreshUI(); } });
                        break;
                    case BarcodeElement bc:
                        AddPropColor("前景色", bc.ForeColor, v => { PushUndo(); bc.ForeColor = v; MarkDirty(); });
                        break;
                    case TableElement tbl:
                        AddPropEditor("边框宽", tbl.BorderWidth.ToString(), v => { if (double.TryParse(v, out var d)) { PushUndo(); tbl.BorderWidth = d; MarkDirty(); } });
                        AddPropColor("边框色", tbl.BorderColor, v => { PushUndo(); tbl.BorderColor = v; MarkDirty(); });
                        break;
                }

                // === 位置尺寸 ===
                AddPropEditor("X(mm)", el.X.ToString(), v => { if (double.TryParse(v, out var d)) { PushUndo(); el.X = d; MarkDirty(); RefreshUI(); } });
                AddPropEditor("Y(mm)", el.Y.ToString(), v => { if (double.TryParse(v, out var d)) { PushUndo(); el.Y = d; MarkDirty(); RefreshUI(); } });
                AddPropEditor("宽(mm)", el.Width.ToString(), v => { if (double.TryParse(v, out var d) && d > 0) { PushUndo(); el.Width = d; MarkDirty(); RefreshUI(); } });
                AddPropEditor("高(mm)", el.Height.ToString(), v => { if (double.TryParse(v, out var d) && d > 0) { PushUndo(); el.Height = d; MarkDirty(); RefreshUI(); } });

                // === 行为 ===
                AddPropSection("行为");
                switch (el)
                {
                    case TextElement t:
                        AddPropBool("自动增高", t.CanGrow, v => { PushUndo(); t.CanGrow = v; MarkDirty(); });
                        break;
                    case LineElement l:
                        AddPropCombo("方向", new[] { "水平", "垂直", "对角线" }, DirToCN(l.Direction.ToString()), v => { PushUndo(); l.Direction = CNToDir(v); MarkDirty(); RefreshUI(); });
                        break;
                    case ShapeElement s:
                        AddPropCombo("形状", new[] { "矩形", "椭圆", "圆角矩形", "三角形" }, ShapeToCN(s.Shape.ToString()), v => { PushUndo(); s.Shape = CNToShape(v); MarkDirty(); RefreshUI(); });
                        break;
                    case ImageElement img:
                        AddPropCombo("缩放", new[] { "拉伸", "等比缩放", "裁剪", "原始尺寸" }, SizingToCN(img.Sizing.ToString()), v => { PushUndo(); img.Sizing = CNToSizing(v); MarkDirty(); });
                        break;
                    case SubReportElement sr:
                        AddPropBool("每行重复", sr.RepeatPerRow, v => { PushUndo(); sr.RepeatPerRow = v; MarkDirty(); });
                        break;
                    case BarcodeElement bc:
                        AddPropCombo("格式", new[] { "Code128", "Code39", "EAN13", "EAN8", "UPC_A", "二维码(QR)", "DataMatrix", "PDF417" }, BcFmtToCN(bc.Format.ToString()), v => { PushUndo(); bc.Format = CNToBcFmt(v); MarkDirty(); RefreshUI(); });
                        AddPropBool("显示文字", bc.ShowText, v => { PushUndo(); bc.ShowText = v; MarkDirty(); });
                        break;
                    case TableElement tbl:
                        AddPropEditor("行数", tbl.RowCount.ToString(), v => { if (int.TryParse(v, out var i) && i > 0) { PushUndo(); tbl.RowCount = i; MarkDirty(); RefreshUI(); } });
                        AddPropEditor("列数", tbl.ColCount.ToString(), v => { if (int.TryParse(v, out var i) && i > 0) { PushUndo(); tbl.ColCount = i; MarkDirty(); RefreshUI(); } });
                        break;
                    case CrossTabElement ct:
                        AddPropBool("行合计", ct.ShowRowTotal, v => { PushUndo(); ct.ShowRowTotal = v; MarkDirty(); });
                        AddPropBool("列合计", ct.ShowColumnTotal, v => { PushUndo(); ct.ShowColumnTotal = v; MarkDirty(); });
                        break;
                    case ChartElement ch:
                        AddPropCombo("图表类型", new[] { "柱状图", "折线图", "饼图", "面积图", "散点图" },
                            ChartTypeCN(ch.ChartType), v => { PushUndo(); ch.ChartType = CNToChartType(v); MarkDirty(); RefreshUI(); });
                        break;
                }

                // === 其它 ===
                AddPropSection("其它");
                switch (el)
                {
                    case TextElement t:
                        AddPropCombo("框类型", new[] { "静态框", "字段框", "统计框", "系统变量框" },
                            BoxTypeToCN(t.BoxType), v => {
                                PushUndo();
                                switch (v) {
                                    case "字段框": t.DataField = t.DataField ?? "FieldName"; t.SummaryFunction = null; t.SystemVariable = null; break;
                                    case "统计框": t.SummaryFunction = t.SummaryFunction ?? "Sum"; t.SummaryField = t.SummaryField ?? "Amount"; t.DataField = null; t.SystemVariable = null; break;
                                    case "系统变量框": t.SystemVariable = t.SystemVariable ?? "PageNumber"; t.DataField = null; t.SummaryFunction = null; break;
                                    default: t.DataField = null; t.SummaryFunction = null; t.SystemVariable = null; break;
                                }
                                MarkDirty(); RefreshUI();
                            });
                        if (t.BoxType == TextBoxType.Static)
                            AddPropEditor("文本", t.Text, v => { PushUndo(); t.Text = v; MarkDirty(); RefreshUI(); });
                        if (t.BoxType == TextBoxType.Field)
                            AddPropExpr("绑定字段", t.DataField ?? "", v => { PushUndo(); t.DataField = v; MarkDirty(); RefreshUI(); });
                        if (t.BoxType == TextBoxType.Summary)
                        {
                            AddPropCombo("统计函数", new[] { "Sum", "Count", "Avg", "Max", "Min" },
                                t.SummaryFunction ?? "Sum", v => { PushUndo(); t.SummaryFunction = v; MarkDirty(); RefreshUI(); });
                            AddPropEditor("统计字段", t.SummaryField ?? "", v => { PushUndo(); t.SummaryField = v; MarkDirty(); RefreshUI(); });
                        }
                        if (t.BoxType == TextBoxType.SysVar)
                            AddPropCombo("系统变量", new[] { "PageNumber", "TotalPages", "PrintDate", "PrintTime", "ReportTitle" },
                                t.SystemVariable ?? "PageNumber", v => { PushUndo(); t.SystemVariable = v; MarkDirty(); RefreshUI(); });
                        AddPropExpr("格式", t.Format ?? "", v => { PushUndo(); t.Format = string.IsNullOrEmpty(v) ? null : v; MarkDirty(); });
                        AddPropExpr("超链接", t.Hyperlink ?? "", v => { PushUndo(); t.Hyperlink = string.IsNullOrEmpty(v) ? null : v; MarkDirty(); });
                        break;
                    case ImageElement img:
                        AddPropEditor("图像源", img.Source, v => { PushUndo(); img.Source = v; MarkDirty(); });
                        break;
                    case SubReportElement sr:
                        AddPropExpr("模板引用", sr.TemplateRef, v => { PushUndo(); sr.TemplateRef = v; MarkDirty(); RefreshUI(); });
                        AddPropExpr("数据源", sr.DataBinding.Source, v => { PushUndo(); sr.DataBinding.Source = v; MarkDirty(); });
                        break;
                    case BarcodeElement bc:
                        AddPropExpr("内容", bc.Value, v => { PushUndo(); bc.Value = v; MarkDirty(); RefreshUI(); });
                        break;
                    case CrossTabElement ct:
                        AddPropExpr("数据源", ct.DataSource, v => { PushUndo(); ct.DataSource = v; MarkDirty(); RefreshUI(); });
                        AddPropExpr("行字段", string.Join(",", ct.RowFields), v => { PushUndo(); ct.RowFields = v.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList(); MarkDirty(); });
                        AddPropExpr("列字段", string.Join(",", ct.ColumnFields), v => { PushUndo(); ct.ColumnFields = v.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList(); MarkDirty(); });
                        AddPropEditor("边框宽", ct.BorderWidth.ToString(), v => { if (double.TryParse(v, out var d)) { PushUndo(); ct.BorderWidth = d; MarkDirty(); } });
                        break;
                    case ChartElement ch:
                        AddPropExpr("标题", ch.Title ?? "", v => { PushUndo(); ch.Title = v; MarkDirty(); });
                        AddPropExpr("数据源", ch.DataSource, v => { PushUndo(); ch.DataSource = v; MarkDirty(); });
                        AddPropExpr("分类字段", ch.CategoryField, v => { PushUndo(); ch.CategoryField = v; MarkDirty(); });
                        break;
                }
            }
        }

        // ============================== 属性面板辅助 ==============================

        /// <summary>多选批量编辑属性面板</summary>
        private void UpdateMultiSelectProperties()
        {
            var targets = _selectedElements;
            AddPropSection("批量编辑 (" + targets.Count + " 个元素)");
            AddPropLabel("数量", targets.Count.ToString());

            // 通用属性
            AddPropSection("位置尺寸");
            AddPropEditor("宽(mm)", "", v => { if (double.TryParse(v, out var d) && d > 0) { PushUndo(); foreach (var el in targets) el.Width = d; MarkDirty(); RefreshUI(); } });
            AddPropEditor("高(mm)", "", v => { if (double.TryParse(v, out var d) && d > 0) { PushUndo(); foreach (var el in targets) el.Height = d; MarkDirty(); RefreshUI(); } });
            AddPropEditor("X(mm)", "", v => { if (double.TryParse(v, out var d)) { PushUndo(); foreach (var el in targets) el.X = d; MarkDirty(); RefreshUI(); } });
            AddPropEditor("Y(mm)", "", v => { if (double.TryParse(v, out var d)) { PushUndo(); foreach (var el in targets) el.Y = d; MarkDirty(); RefreshUI(); } });

            AddPropSection("设计");
            bool allVisible = targets.All(e => e.Visible);
            AddPropBool("可见", allVisible, v => { PushUndo(); foreach (var el in targets) el.Visible = v; MarkDirty(); });
            bool allLocked = targets.All(e => e.Locked);
            AddPropBool("锁定", allLocked, v => { PushUndo(); foreach (var el in targets) el.Locked = v; MarkDirty(); RefreshUI(); });

            AddPropSection("外观");
            AddPropColor("背景色", "", v => { PushUndo(); foreach (var el in targets) el.BackgroundColor = string.IsNullOrEmpty(v) ? null : v; MarkDirty(); RefreshUI(); });

            // 批量边框编辑
            AddPropSection("边框");
            AddPropEditor("边框宽", "", v => { if (double.TryParse(v, out var d) && d >= 0) { PushUndo(); foreach (var el in targets) EnsureBorder(el).Width = d; MarkDirty(); RefreshUI(); } });
            AddPropColor("边框色", "", v => { PushUndo(); foreach (var el in targets) EnsureBorder(el).Color = string.IsNullOrEmpty(v) ? "#000000" : v; MarkDirty(); RefreshUI(); });
            AddPropBool("上边框", true, v => { PushUndo(); foreach (var el in targets) EnsureBorder(el).Top = v; MarkDirty(); RefreshUI(); });
            AddPropBool("下边框", true, v => { PushUndo(); foreach (var el in targets) EnsureBorder(el).Bottom = v; MarkDirty(); RefreshUI(); });
            AddPropBool("左边框", true, v => { PushUndo(); foreach (var el in targets) EnsureBorder(el).Left = v; MarkDirty(); RefreshUI(); });
            AddPropBool("右边框", true, v => { PushUndo(); foreach (var el in targets) EnsureBorder(el).Right = v; MarkDirty(); RefreshUI(); });

            // 如果所有选中元素都是 TextElement，显示文本共有属性
            if (targets.All(e => e is TextElement))
            {
                var texts = targets.Cast<TextElement>().ToList();
                AddPropSection("字体");
                string commonFamily = texts.Select(t => t.Font.Family).Distinct().Count() == 1 ? texts[0].Font.Family : "";
                AddPropEditor("字体", commonFamily, v => { if (!string.IsNullOrEmpty(v)) { PushUndo(); foreach (var t in texts) t.Font.Family = v; MarkDirty(); RefreshUI(); } });
                string commonSize = texts.Select(t => t.Font.Size).Distinct().Count() == 1 ? texts[0].Font.Size.ToString() : "";
                AddPropEditor("字号", commonSize, v => { if (double.TryParse(v, out var sz) && sz > 0) { PushUndo(); foreach (var t in texts) t.Font.Size = sz; MarkDirty(); RefreshUI(); } });
                bool allBold = texts.All(t => t.Font.Bold);
                AddPropBool("加粗", allBold, v => { PushUndo(); foreach (var t in texts) t.Font.Bold = v; MarkDirty(); RefreshUI(); });
                bool allItalic = texts.All(t => t.Font.Italic);
                AddPropBool("斜体", allItalic, v => { PushUndo(); foreach (var t in texts) t.Font.Italic = v; MarkDirty(); RefreshUI(); });
                AddPropColor("字体色", "", v => { PushUndo(); foreach (var t in texts) t.Font.Color = string.IsNullOrEmpty(v) ? null : v; MarkDirty(); RefreshUI(); });
                AddPropCombo("对齐", new[] { "左对齐", "居中", "右对齐", "两端对齐" }, "",
                    v => { var a = CNToAlign(v); PushUndo(); foreach (var t in texts) t.Alignment = a; MarkDirty(); RefreshUI(); });
            }
        }

        private Expander? _currentExpander;

        private void AddPropSection(string title)
        {
            var content = new StackPanel();
            var expander = new Expander
            {
                Header = title,
                IsExpanded = true,
                Content = content,
                Margin = new Thickness(0, 0, 0, 0),
                Padding = new Thickness(0),
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0, 100, 140)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(210, 210, 210)),
                BorderThickness = new Thickness(0, 0, 0, 1),
            };
            _propertyStack.Children.Add(expander);
            _currentExpander = expander;
        }

        private void AddPropToCurrentSection(UIElement element)
        {
            // 交替行背景色
            if (element is Grid g)
            {
                g.Background = _propRowIndex % 2 == 0 ? Brushes.White : new SolidColorBrush(Color.FromRgb(245, 245, 245));
                _propRowIndex++;
            }
            if (_currentExpander?.Content is StackPanel sp)
                sp.Children.Add(element);
            else
                _propertyStack.Children.Add(element);
        }

        private void AddPropLabel(string label, string value)
        {
            var grid = new Grid { Margin = new Thickness(0), MinHeight = 22 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var lb = new TextBlock { Text = label, Foreground = Brushes.DimGray, FontSize = 11, FontWeight = FontWeights.Normal, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 0, 0) };
            var vb = new TextBlock { Text = value, Foreground = Brushes.Black, FontSize = 11, FontWeight = FontWeights.Normal, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 4, 0) };
            // 分隔线
            var sep = new Border { Width = 1, Background = new SolidColorBrush(Color.FromRgb(220, 220, 220)), HorizontalAlignment = HorizontalAlignment.Right };
            Grid.SetColumn(lb, 0); Grid.SetColumn(sep, 0); Grid.SetColumn(vb, 1);
            grid.Children.Add(lb); grid.Children.Add(sep); grid.Children.Add(vb);
            AddPropToCurrentSection(grid);
        }

        private void AddPropEditor(string label, string value, Action<string> onCommit)
        {
            var grid = new Grid { Margin = new Thickness(0), MinHeight = 22 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var lb = new TextBlock { Text = label, Foreground = Brushes.DimGray, FontSize = 11, FontWeight = FontWeights.Normal, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 0, 0) };
            var tb = new TextBox
            {
                Text = value, FontSize = 11,
                Background = Brushes.Transparent,
                Foreground = Brushes.Black,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(4, 1, 4, 1),
                VerticalAlignment = VerticalAlignment.Center,
            };
            tb.GotFocus += (_, __) => { tb.Background = Brushes.White; tb.BorderThickness = new Thickness(1); tb.BorderBrush = new SolidColorBrush(Color.FromRgb(100, 150, 200)); };
            tb.LostFocus += (_, __) => { tb.Background = Brushes.Transparent; tb.BorderThickness = new Thickness(0); onCommit(tb.Text); };
            tb.KeyDown += (_, args) => { if (args.Key == Key.Enter) onCommit(tb.Text); };
            var sep = new Border { Width = 1, Background = new SolidColorBrush(Color.FromRgb(220, 220, 220)), HorizontalAlignment = HorizontalAlignment.Right };
            Grid.SetColumn(lb, 0); Grid.SetColumn(sep, 0); Grid.SetColumn(tb, 1);
            grid.Children.Add(lb); grid.Children.Add(sep); grid.Children.Add(tb);
            AddPropToCurrentSection(grid);
        }

        private void AddPropExpr(string label, string value, Action<string> onCommit)
        {
            var grid = new Grid { Margin = new Thickness(0), MinHeight = 22 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(26) });
            var lb = new TextBlock { Text = label, Foreground = Brushes.DimGray, FontSize = 11, FontWeight = FontWeights.Normal, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 0, 0) };
            var tb = new TextBox
            {
                Text = value, FontSize = 11,
                Background = Brushes.Transparent,
                Foreground = Brushes.Black,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(4, 1, 4, 1),
                VerticalAlignment = VerticalAlignment.Center,
            };
            tb.GotFocus += (_, __) => { tb.Background = Brushes.White; tb.BorderThickness = new Thickness(1); tb.BorderBrush = new SolidColorBrush(Color.FromRgb(100, 150, 200)); };
            tb.LostFocus += (_, __) => { tb.Background = Brushes.Transparent; tb.BorderThickness = new Thickness(0); onCommit(tb.Text); };
            tb.KeyDown += (_, args) => { if (args.Key == Key.Enter) onCommit(tb.Text); };
            // 表达式按钮
            var exprBtn = new Button
            {
                Content = "🧮", FontSize = 11, Width = 22, Height = 18,
                Padding = new Thickness(0), Background = Brushes.Transparent,
                BorderThickness = new Thickness(0), Cursor = Cursors.Hand,
                ToolTip = "打开表达式编辑器",
            };
            exprBtn.Click += (_, __) => ShowExpressionEditor(tb.Text, v => { tb.Text = v; onCommit(v); });
            var sep = new Border { Width = 1, Background = new SolidColorBrush(Color.FromRgb(220, 220, 220)), HorizontalAlignment = HorizontalAlignment.Right };
            Grid.SetColumn(lb, 0); Grid.SetColumn(sep, 0); Grid.SetColumn(tb, 1); Grid.SetColumn(exprBtn, 2);
            grid.Children.Add(lb); grid.Children.Add(sep); grid.Children.Add(tb); grid.Children.Add(exprBtn);
            AddPropToCurrentSection(grid);
        }

        private void AddPropColor(string label, string value, Action<string> onCommit)
        {
            var grid = new Grid { Margin = new Thickness(0), MinHeight = 22 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) });
            var lb = new TextBlock { Text = label, Foreground = Brushes.DimGray, FontSize = 11, FontWeight = FontWeights.Normal, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 0, 0) };
            var tb = new TextBox
            {
                Text = value ?? "", FontSize = 11,
                Background = Brushes.Transparent, Foreground = Brushes.Black,
                BorderThickness = new Thickness(0), Padding = new Thickness(4, 1, 4, 1),
                VerticalAlignment = VerticalAlignment.Center,
            };
            tb.GotFocus += (_, __) => { tb.Background = Brushes.White; tb.BorderThickness = new Thickness(1); tb.BorderBrush = new SolidColorBrush(Color.FromRgb(100, 150, 200)); };
            tb.LostFocus += (_, __) => { tb.Background = Brushes.Transparent; tb.BorderThickness = new Thickness(0); onCommit(tb.Text); };
            tb.KeyDown += (_, args) => { if (args.Key == Key.Enter) onCommit(tb.Text); };

            // 颜色预览块 + 点击弹出选色器
            var colorPreview = new Border { Width = 16, Height = 16, CornerRadius = new CornerRadius(2), BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1), Margin = new Thickness(2), Cursor = Cursors.Hand, VerticalAlignment = VerticalAlignment.Center };
            colorPreview.Background = BrushParser.Parse(value ?? "", Brushes.Transparent);
            colorPreview.MouseLeftButtonDown += (_, __) =>
            {
                var picked = ShowColorPicker(value ?? "");
                if (picked != null)
                {
                    tb.Text = picked;
                    colorPreview.Background = BrushParser.Parse(picked, Brushes.Transparent);
                    onCommit(picked);
                }
            };

            var sep = new Border { Width = 1, Background = new SolidColorBrush(Color.FromRgb(220, 220, 220)), HorizontalAlignment = HorizontalAlignment.Right };
            Grid.SetColumn(lb, 0); Grid.SetColumn(sep, 0); Grid.SetColumn(tb, 1); Grid.SetColumn(colorPreview, 2);
            grid.Children.Add(lb); grid.Children.Add(sep); grid.Children.Add(tb); grid.Children.Add(colorPreview);
            AddPropToCurrentSection(grid);
        }

        private string? ShowColorPicker(string currentColor)
        {
            var dlg = new Window
            {
                Title = "选择颜色", Width = 300, Height = 360,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this, ResizeMode = ResizeMode.NoResize,
            };
            var stack = new StackPanel { Margin = new Thickness(12) };

            // 预设颜色网格
            var presetColors = new[] { "#000000", "#333333", "#666666", "#999999", "#CCCCCC", "#FFFFFF",
                "#FF0000", "#FF6600", "#FFCC00", "#33CC00", "#0099FF", "#6633FF",
                "#CC0066", "#FF3399", "#FF9966", "#66CC66", "#3399CC", "#9966CC",
                "#800000", "#804000", "#808000", "#008000", "#004080", "#400080" };
            var grid2 = new WrapPanel { Margin = new Thickness(0, 0, 0, 12) };
            string? result = null;
            var previewBorder = new Border { Width = 40, Height = 40, CornerRadius = new CornerRadius(4), BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1), Margin = new Thickness(0, 8, 0, 8) };
            previewBorder.Background = BrushParser.Parse(currentColor, Brushes.White);
            var hexBox = new TextBox { Text = currentColor, FontSize = 12, Width = 120, Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
            hexBox.TextChanged += (_, __) => { previewBorder.Background = BrushParser.Parse(hexBox.Text, Brushes.White); };

            foreach (var c in presetColors)
            {
                var swatch = new Border { Width = 28, Height = 28, Margin = new Thickness(2), CornerRadius = new CornerRadius(3), Background = BrushParser.Parse(c, Brushes.White), BorderBrush = Brushes.LightGray, BorderThickness = new Thickness(1), Cursor = Cursors.Hand };
                var cc = c;
                swatch.MouseLeftButtonDown += (_, __) => { hexBox.Text = cc; previewBorder.Background = BrushParser.Parse(cc, Brushes.White); };
                grid2.Children.Add(swatch);
            }
            stack.Children.Add(new TextBlock { Text = "预设颜色", FontSize = 11, FontWeight = FontWeights.Bold });
            stack.Children.Add(grid2);

            // 预览 + hex输入
            var previewRow = new StackPanel { Orientation = Orientation.Horizontal };
            previewRow.Children.Add(previewBorder);
            previewRow.Children.Add(hexBox);
            stack.Children.Add(new TextBlock { Text = "自定义 (Hex)", FontSize = 11, FontWeight = FontWeights.Bold });
            stack.Children.Add(previewRow);

            // 按钮
            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 16, 0, 0) };
            var btnOk = new Button { Content = "确定", Width = 70, Height = 26, Margin = new Thickness(0, 0, 8, 0), IsDefault = true };
            var btnClear = new Button { Content = "清除", Width = 70, Height = 26, Margin = new Thickness(0, 0, 8, 0) };
            var btnCancel = new Button { Content = "取消", Width = 70, Height = 26, IsCancel = true };
            btnOk.Click += (_, __) => { result = hexBox.Text; dlg.DialogResult = true; };
            btnClear.Click += (_, __) => { result = ""; dlg.DialogResult = true; };
            btnPanel.Children.Add(btnOk); btnPanel.Children.Add(btnClear); btnPanel.Children.Add(btnCancel);
            stack.Children.Add(btnPanel);

            dlg.Content = stack;
            return dlg.ShowDialog() == true ? result : null;
        }

        private void AddPropBool(string label, bool value, Action<bool> onCommit)
        {
            AddPropCombo(label, new[] { "是", "否" }, value ? "是" : "否", v => onCommit(v == "是"));
        }

        private void AddPropCombo(string label, string[] options, string current, Action<string> onCommit)
        {
            var grid = new Grid { Margin = new Thickness(0), MinHeight = 22 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var lb = new TextBlock { Text = label, Foreground = Brushes.DimGray, FontSize = 11, FontWeight = FontWeights.Normal, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 0, 0) };
            var cb = new ComboBox
            {
                FontSize = 11,
                Background = Brushes.Transparent,
                Foreground = Brushes.Black,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(2, 0, 2, 0),
                VerticalAlignment = VerticalAlignment.Center,
            };
            foreach (var opt in options) cb.Items.Add(opt);
            cb.SelectedItem = current;
            cb.SelectionChanged += (_, __) => { if (cb.SelectedItem is string s) onCommit(s); };
            var sep = new Border { Width = 1, Background = new SolidColorBrush(Color.FromRgb(220, 220, 220)), HorizontalAlignment = HorizontalAlignment.Right };
            Grid.SetColumn(lb, 0); Grid.SetColumn(sep, 0); Grid.SetColumn(cb, 1);
            grid.Children.Add(lb); grid.Children.Add(sep); grid.Children.Add(cb);
            AddPropToCurrentSection(grid);
        }

        // ============================== 其他 ==============================

        private void UpdateTitle()
        {
            var name = _currentFilePath != null ? System.IO.Path.GetFileName(_currentFilePath) : "新模板";
            Title = (_dirty ? "* " : "") + name + " - 报表设计器";
        }

        private void MarkDirty()
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
                _canvasRenderer.Render(BuildCanvasRenderContext(), _selectedElements, _selectedBand);
                UpdatePropertyList();
            }
        }

        private void OnCanvasRightClick(object sender, MouseButtonEventArgs e)
        {
            if (_template == null) return;
            var pos = e.GetPosition(_canvas);
            HitTest(pos, out var band, out var element);

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

        private static Brush GetBandBrush(BandType type)
        {
            switch (type)
            {
                case BandType.ReportHeader: return new SolidColorBrush(Color.FromArgb(40, 70, 130, 180));
                case BandType.ReportFooter: return new SolidColorBrush(Color.FromArgb(40, 100, 149, 237));
                case BandType.Header: return new SolidColorBrush(Color.FromArgb(40, 60, 179, 113));
                case BandType.Footer: return new SolidColorBrush(Color.FromArgb(40, 144, 238, 144));
                case BandType.Detail: return new SolidColorBrush(Color.FromArgb(20, 200, 200, 200));
                case BandType.GroupHeader: return new SolidColorBrush(Color.FromArgb(40, 255, 165, 0));
                case BandType.GroupFooter: return new SolidColorBrush(Color.FromArgb(40, 255, 215, 0));
                default: return Brushes.Transparent;
            }
        }


        // ============================== 中英文转换 ==============================


        private static string ElementTypeName(ReportElement el)
        {
            switch (el)
            {
                case TextElement _: return "文本";
                case LineElement _: return "线条";
                case ImageElement _: return "图像";
                case ShapeElement _: return "形状";
                case SubReportElement _: return "子报表";
                case BarcodeElement _: return "条码/二维码";
                case TableElement _: return "表格";
                case CrossTabElement _: return "交叉表";
                case ChartElement _: return "图表";
                default: return el.GetType().Name;
            }
        }

        private static string AlignToCN(string v) { switch (v) { case "Left": return "左对齐"; case "Center": return "居中"; case "Right": return "右对齐"; case "Justify": return "两端对齐"; default: return v; } }
        private static ReportEngine.Core.TextAlignment CNToAlign(string v) { switch (v) { case "居中": return ReportEngine.Core.TextAlignment.Center; case "右对齐": return ReportEngine.Core.TextAlignment.Right; case "两端对齐": return ReportEngine.Core.TextAlignment.Justify; default: return ReportEngine.Core.TextAlignment.Left; } }

        private static string DirToCN(string v) { switch (v) { case "Horizontal": return "水平"; case "Vertical": return "垂直"; case "Diagonal": return "对角线"; default: return v; } }
        private static LineDirection CNToDir(string v) { switch (v) { case "垂直": return LineDirection.Vertical; case "对角线": return LineDirection.Diagonal; default: return LineDirection.Horizontal; } }

        private static string ShapeToCN(string v) { switch (v) { case "Rectangle": return "矩形"; case "Ellipse": return "椭圆"; case "RoundedRect": return "圆角矩形"; case "Triangle": return "三角形"; default: return v; } }
        private static ShapeType CNToShape(string v) { switch (v) { case "椭圆": return ShapeType.Ellipse; case "圆角矩形": return ShapeType.RoundedRect; case "三角形": return ShapeType.Triangle; default: return ShapeType.Rectangle; } }

        private static string SizingToCN(string v) { switch (v) { case "Stretch": return "拉伸"; case "FitProportional": return "等比缩放"; case "Clip": return "裁剪"; case "ActualSize": return "原始尺寸"; default: return v; } }
        private static ImageSizing CNToSizing(string v) { switch (v) { case "拉伸": return ImageSizing.Stretch; case "裁剪": return ImageSizing.Clip; case "原始尺寸": return ImageSizing.ActualSize; default: return ImageSizing.FitProportional; } }

        private static string BoxTypeToCN(TextBoxType t) { switch (t) { case TextBoxType.Field: return "字段框"; case TextBoxType.Summary: return "统计框"; case TextBoxType.SysVar: return "系统变量框"; default: return "静态框"; } }

        private static string GetTextElLabel(TextElement t)
        {
            switch (t.BoxType)
            {
                case TextBoxType.Field: return t.DataField ?? "";
                case TextBoxType.Summary: return t.SummaryFunction + "(" + (t.SummaryField ?? "") + ")";
                case TextBoxType.SysVar: return t.SystemVariable ?? "";
                default: var s = t.Text ?? ""; return s.Length > 12 ? s.Substring(0, 12) + "…" : s;
            }
        }

        private static string BcFmtToCN(string v) { return v == "QRCode" ? "二维码(QR)" : v; }
        private static BarcodeFormat CNToBcFmt(string v) { switch (v) { case "二维码(QR)": return BarcodeFormat.QRCode; default: if (Enum.TryParse<BarcodeFormat>(v, out var f)) return f; return BarcodeFormat.QRCode; } }

        private static string ChartTypeCN(ChartType t) { switch (t) { case ChartType.Bar: return "柱状图"; case ChartType.Line: return "折线图"; case ChartType.Pie: return "饼图"; case ChartType.Area: return "面积图"; case ChartType.Scatter: return "散点图"; default: return t.ToString(); } }
        private static ChartType CNToChartType(string v) { switch (v) { case "折线图": return ChartType.Line; case "饼图": return ChartType.Pie; case "面积图": return ChartType.Area; case "散点图": return ChartType.Scatter; default: return ChartType.Bar; } }

        // ============================== 页面设置弹窗 ==============================

        private void ShowPageSetupDialog()
        {
            if (_template == null) return;

            var dlg = new Window
            {
                Title = "页面设置",
                Width = 420,
                Height = 520,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
            };

            var stack = new StackPanel { Margin = new Thickness(16) };

            // ── 打印机选择 ──
            stack.Children.Add(new TextBlock { Text = "打印机", FontSize = 12, FontWeight = FontWeights.Bold });
            var printerCombo = new ComboBox { FontSize = 12, Margin = new Thickness(0, 4, 0, 8) };
            try
            {
                foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
                    printerCombo.Items.Add(printer);
                // 选中默认打印机
                var defPrinter = new System.Drawing.Printing.PrinterSettings().PrinterName;
                for (int i = 0; i < printerCombo.Items.Count; i++)
                    if ((string)printerCombo.Items[i] == defPrinter) { printerCombo.SelectedIndex = i; break; }
            }
            catch { /* 无打印机环境 */ }
            stack.Children.Add(printerCombo);

            // 纸张类型预设
            var paperTypes = new[] { "A4 (210 × 297mm)", "A3 (297 × 420mm)", "A5 (148 × 210mm)", "B4 (250 × 353mm)", "B5 (176 × 250mm)", "Letter (216 × 279mm)", "Legal (216 × 356mm)", "(打印机默认纸张)", "自定义" };
            var paperSizes = new (double w, double h)[] { (210, 297), (297, 420), (148, 210), (250, 353), (176, 250), (216, 279), (216, 356), (0, 0), (0, 0) };
            // paperSizes[7] = 打印机默认纸张(动态填充), paperSizes[8] = 自定义

            var paperCombo = new ComboBox { FontSize = 12, Margin = new Thickness(0, 4, 0, 8) };
            foreach (var pt in paperTypes) paperCombo.Items.Add(pt);

            var widthBox = new TextBox { Text = _template.Page.Width.ToString(), FontSize = 12, Margin = new Thickness(0, 4, 0, 4), Padding = new Thickness(4, 2, 4, 2) };
            var heightBox = new TextBox { Text = _template.Page.Height.ToString(), FontSize = 12, Margin = new Thickness(0, 4, 0, 8), Padding = new Thickness(4, 2, 4, 2) };

            var orientCombo = new ComboBox { FontSize = 12, Margin = new Thickness(0, 4, 0, 8) };
            orientCombo.Items.Add("纵向");
            orientCombo.Items.Add("横向");
            orientCombo.SelectedIndex = _template.Page.Orientation == "landscape" ? 1 : 0;

            var topBox = new TextBox { Text = _template.Page.Margin.Top.ToString(), FontSize = 12, Padding = new Thickness(4, 2, 4, 2) };
            var bottomBox = new TextBox { Text = _template.Page.Margin.Bottom.ToString(), FontSize = 12, Padding = new Thickness(4, 2, 4, 2) };
            var leftBox = new TextBox { Text = _template.Page.Margin.Left.ToString(), FontSize = 12, Padding = new Thickness(4, 2, 4, 2) };
            var rightBox = new TextBox { Text = _template.Page.Margin.Right.ToString(), FontSize = 12, Padding = new Thickness(4, 2, 4, 2) };

            // 匹配当前纸张
            int matchIdx = paperSizes.Length - 1;
            for (int i = 0; i < paperSizes.Length - 1; i++)
            {
                var sz = paperSizes[i];
                if ((Math.Abs(_template.Page.Width - sz.w) < 1 && Math.Abs(_template.Page.Height - sz.h) < 1) ||
                    (Math.Abs(_template.Page.Width - sz.h) < 1 && Math.Abs(_template.Page.Height - sz.w) < 1))
                { matchIdx = i; break; }
            }
            paperCombo.SelectedIndex = matchIdx;

            // 从打印机获取纸张尺寸的方法
            Action updatePrinterPaper = () =>
            {
                try
                {
                    if (printerCombo.SelectedItem == null) return;
                    var ps = new System.Drawing.Printing.PrinterSettings { PrinterName = (string)printerCombo.SelectedItem };
                    if (ps.IsValid)
                    {
                        // 获取打印机默认纸张尺寸（单位 1/100 inch → mm）
                        var defPaper = ps.DefaultPageSettings.PaperSize;
                        double pw = Math.Round(defPaper.Width / 100.0 * 25.4, 2);
                        double ph = Math.Round(defPaper.Height / 100.0 * 25.4, 2);
                        paperSizes[7] = (pw, ph);
                    }
                }
                catch { paperSizes[7] = (0, 0); }
            };
            updatePrinterPaper();

            printerCombo.SelectionChanged += (_, __) => {
                updatePrinterPaper();
                // 如果当前选择的是"打印机默认纸张"，刷新尺寸
                if (paperCombo.SelectedIndex == 7 && paperSizes[7].w > 0)
                {
                    widthBox.Text = paperSizes[7].w.ToString();
                    heightBox.Text = paperSizes[7].h.ToString();
                }
            };

            Action updateEditable = () =>
            {
                bool custom = paperCombo.SelectedIndex == paperSizes.Length - 1; // 自定义
                widthBox.IsReadOnly = !custom;
                heightBox.IsReadOnly = !custom;
                widthBox.Background = custom ? Brushes.White : new SolidColorBrush(Color.FromRgb(235, 235, 235));
                heightBox.Background = custom ? Brushes.White : new SolidColorBrush(Color.FromRgb(235, 235, 235));
            };

            paperCombo.SelectionChanged += (_, __) =>
            {
                int idx = paperCombo.SelectedIndex;
                if (idx == 7) // 打印机默认纸张
                {
                    updatePrinterPaper();
                    if (paperSizes[7].w > 0)
                    {
                        bool landscape = orientCombo.SelectedIndex == 1;
                        widthBox.Text = (landscape ? paperSizes[7].h : paperSizes[7].w).ToString();
                        heightBox.Text = (landscape ? paperSizes[7].w : paperSizes[7].h).ToString();
                    }
                }
                else if (idx >= 0 && idx < paperSizes.Length - 1)
                {
                    bool landscape = orientCombo.SelectedIndex == 1;
                    widthBox.Text = (landscape ? paperSizes[idx].h : paperSizes[idx].w).ToString();
                    heightBox.Text = (landscape ? paperSizes[idx].w : paperSizes[idx].h).ToString();
                }
                updateEditable();
            };

            orientCombo.SelectionChanged += (_, __) =>
            {
                int idx = paperCombo.SelectedIndex;
                if (idx >= 0 && idx < paperSizes.Length - 1)
                {
                    bool landscape = orientCombo.SelectedIndex == 1;
                    widthBox.Text = (landscape ? paperSizes[idx].h : paperSizes[idx].w).ToString();
                    heightBox.Text = (landscape ? paperSizes[idx].w : paperSizes[idx].h).ToString();
                }
            };

            updateEditable();

            stack.Children.Add(new TextBlock { Text = "纸张类型", FontSize = 12, FontWeight = FontWeights.Bold });
            stack.Children.Add(paperCombo);

            var sizeGrid = new Grid();
            sizeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            sizeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });
            sizeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var wPanel = new StackPanel();
            wPanel.Children.Add(new TextBlock { Text = "宽度(mm)", FontSize = 11 });
            wPanel.Children.Add(widthBox);
            Grid.SetColumn(wPanel, 0);
            sizeGrid.Children.Add(wPanel);
            var hPanel = new StackPanel();
            hPanel.Children.Add(new TextBlock { Text = "高度(mm)", FontSize = 11 });
            hPanel.Children.Add(heightBox);
            Grid.SetColumn(hPanel, 2);
            sizeGrid.Children.Add(hPanel);
            stack.Children.Add(sizeGrid);

            stack.Children.Add(new TextBlock { Text = "方向", FontSize = 12, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 4, 0, 0) });
            stack.Children.Add(orientCombo);

            stack.Children.Add(new TextBlock { Text = "边距(mm)", FontSize = 12, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 4, 0, 4) });
            var marginGrid = new Grid();
            marginGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            marginGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            marginGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            marginGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            marginGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            marginGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            marginGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var lbTop = new StackPanel(); lbTop.Children.Add(new TextBlock { Text = "上", FontSize = 10 }); lbTop.Children.Add(topBox); Grid.SetColumn(lbTop, 0); marginGrid.Children.Add(lbTop);
            var lbBot = new StackPanel(); lbBot.Children.Add(new TextBlock { Text = "下", FontSize = 10 }); lbBot.Children.Add(bottomBox); Grid.SetColumn(lbBot, 2); marginGrid.Children.Add(lbBot);
            var lbLft = new StackPanel(); lbLft.Children.Add(new TextBlock { Text = "左", FontSize = 10 }); lbLft.Children.Add(leftBox); Grid.SetColumn(lbLft, 4); marginGrid.Children.Add(lbLft);
            var lbRgt = new StackPanel(); lbRgt.Children.Add(new TextBlock { Text = "右", FontSize = 10 }); lbRgt.Children.Add(rightBox); Grid.SetColumn(lbRgt, 6); marginGrid.Children.Add(lbRgt);
            stack.Children.Add(marginGrid);

            // ── 多联打印 ──
            stack.Children.Add(new TextBlock { Text = "多联打印（将纸张等分为多份）", FontSize = 12, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 8, 0, 4) });
            var mu = _template.Page.MultiUp;
            var chkMultiUp = new CheckBox { Content = "启用多联打印", FontSize = 11, IsChecked = mu != null, Margin = new Thickness(0, 0, 0, 4) };
            stack.Children.Add(chkMultiUp);

            var muPanel = new StackPanel { Margin = new Thickness(16, 0, 0, 0) };

            // 行列数 + 间距
            muPanel.Children.Add(new TextBlock { Text = "拼版布局", FontSize = 10, Margin = new Thickness(0, 2, 0, 2) });
            var muGrid = new Grid();
            muGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            muGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            muGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            muGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            muGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            muGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            muGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var muRowsBox = new TextBox { Text = (mu?.Rows ?? 2).ToString(), FontSize = 12, Padding = new Thickness(4, 2, 4, 2) };
            var muColsBox = new TextBox { Text = (mu?.Columns ?? 2).ToString(), FontSize = 12, Padding = new Thickness(4, 2, 4, 2) };
            var muHSpBox = new TextBox { Text = (mu?.HSpacing ?? 0).ToString(), FontSize = 12, Padding = new Thickness(4, 2, 4, 2) };
            var muVSpBox = new TextBox { Text = (mu?.VSpacing ?? 0).ToString(), FontSize = 12, Padding = new Thickness(4, 2, 4, 2) };

            var pRows = new StackPanel(); pRows.Children.Add(new TextBlock { Text = "行数", FontSize = 10 }); pRows.Children.Add(muRowsBox); Grid.SetColumn(pRows, 0); muGrid.Children.Add(pRows);
            var pCols = new StackPanel(); pCols.Children.Add(new TextBlock { Text = "列数", FontSize = 10 }); pCols.Children.Add(muColsBox); Grid.SetColumn(pCols, 2); muGrid.Children.Add(pCols);
            var pHSp = new StackPanel(); pHSp.Children.Add(new TextBlock { Text = "水平间距", FontSize = 10 }); pHSp.Children.Add(muHSpBox); Grid.SetColumn(pHSp, 4); muGrid.Children.Add(pHSp);
            var pVSp = new StackPanel(); pVSp.Children.Add(new TextBlock { Text = "垂直间距", FontSize = 10 }); pVSp.Children.Add(muVSpBox); Grid.SetColumn(pVSp, 6); muGrid.Children.Add(pVSp);
            muPanel.Children.Add(muGrid);

            muPanel.Visibility = mu != null ? Visibility.Visible : Visibility.Collapsed;
            chkMultiUp.Checked += (_, __) => muPanel.Visibility = Visibility.Visible;
            chkMultiUp.Unchecked += (_, __) => muPanel.Visibility = Visibility.Collapsed;
            stack.Children.Add(muPanel);

            // 按钮
            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 16, 0, 0) };
            var btnOk = new Button { Content = "确定", Width = 80, Height = 28, Margin = new Thickness(0, 0, 8, 0), IsDefault = true };
            var btnCancel = new Button { Content = "取消", Width = 80, Height = 28, IsCancel = true };
            btnPanel.Children.Add(btnOk);
            btnPanel.Children.Add(btnCancel);
            stack.Children.Add(btnPanel);

            btnOk.Click += (_, __) =>
            {
                if (!double.TryParse(widthBox.Text, out var pw) || pw <= 0 ||
                    !double.TryParse(heightBox.Text, out var ph) || ph <= 0)
                {
                    MessageBox.Show("宽度和高度必须为正数", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                double.TryParse(topBox.Text, out var mt2);
                double.TryParse(bottomBox.Text, out var mb2);
                double.TryParse(leftBox.Text, out var ml2);
                double.TryParse(rightBox.Text, out var mr2);

                PushUndo();
                _template.Page.Width = pw;
                _template.Page.Height = ph;
                _template.Page.Orientation = orientCombo.SelectedIndex == 1 ? "landscape" : "portrait";
                _template.Page.Margin.Top = mt2;
                _template.Page.Margin.Bottom = mb2;
                _template.Page.Margin.Left = ml2;
                _template.Page.Margin.Right = mr2;

                // 多联打印
                if (chkMultiUp.IsChecked == true)
                {
                    int.TryParse(muRowsBox.Text, out var muR); if (muR < 1) muR = 1;
                    int.TryParse(muColsBox.Text, out var muC); if (muC < 1) muC = 1;
                    double.TryParse(muHSpBox.Text, out var muHS);
                    double.TryParse(muVSpBox.Text, out var muVS);
                    _template.Page.MultiUp = new MultiUpConfig { Rows = muR, Columns = muC, HSpacing = muHS, VSpacing = muVS };
                }
                else
                {
                    _template.Page.MultiUp = null;
                }

                MarkDirty();
                RefreshUI();
                dlg.DialogResult = true;
            };

            dlg.Content = stack;
            dlg.ShowDialog();
        }

        // ============================== 字体选择弹窗 ==============================

        private void AddPropFontRow(TextElement t)
        {
            string fontDesc = t.Font.Family + "(" + t.Font.Size + ")";
            if (t.Font.Bold) fontDesc += ",粗体";
            if (t.Font.Italic) fontDesc += ",斜体";

            var grid = new Grid { Margin = new Thickness(0), MinHeight = 22 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) });
            var lb = new TextBlock { Text = "字体", Foreground = Brushes.DimGray, FontSize = 11, FontWeight = FontWeights.Normal, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 0, 0) };
            var vb = new TextBlock { Text = fontDesc, Foreground = Brushes.Black, FontSize = 11, FontWeight = FontWeights.Normal, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 0, 0), TextTrimming = TextTrimming.CharacterEllipsis };
            var btn = new Button { Content = "...", Width = 22, Height = 18, FontSize = 10, Padding = new Thickness(0), Background = Brushes.Transparent, BorderThickness = new Thickness(1), BorderBrush = new SolidColorBrush(Color.FromRgb(180, 180, 180)), Cursor = Cursors.Hand, VerticalAlignment = VerticalAlignment.Center };
            btn.Click += (_, __) => ShowFontDialog(t);
            var sep = new Border { Width = 1, Background = new SolidColorBrush(Color.FromRgb(220, 220, 220)), HorizontalAlignment = HorizontalAlignment.Right };
            Grid.SetColumn(lb, 0); Grid.SetColumn(sep, 0); Grid.SetColumn(vb, 1); Grid.SetColumn(btn, 2);
            grid.Children.Add(lb); grid.Children.Add(sep); grid.Children.Add(vb); grid.Children.Add(btn);
            AddPropToCurrentSection(grid);
        }

        private void ShowFontDialog(TextElement t)
        {
            var dlg = new Window
            {
                Title = "字体",
                Width = 560,
                Height = 460,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
            };

            var mainGrid = new Grid { Margin = new Thickness(12) };
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(8) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(8) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(100) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(10) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(32) });

            // 上部三栏: 字体 | 字形 | 大小
            var topGrid = new Grid();
            topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) });
            topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.8, GridUnitType.Star) });

            // 字体列
            var fontPanel = new DockPanel();
            fontPanel.Children.Add(new TextBlock { Text = "字体(F):", FontSize = 11, Margin = new Thickness(0, 0, 0, 2) });
            DockPanel.SetDock(fontPanel.Children[0], Dock.Top);
            var fontInput = new TextBox { Text = t.Font.Family, FontSize = 12, Padding = new Thickness(4, 2, 4, 2), Margin = new Thickness(0, 0, 0, 2) };
            DockPanel.SetDock(fontInput, Dock.Top);
            fontPanel.Children.Add(fontInput);
            var fontList = new ListBox { FontSize = 12 };
            foreach (var f in new[] { "宋体", "黑体", "楷体", "仿宋", "微软雅黑", "华文中宋", "新宋体", "幼圆", "Arial", "Times New Roman", "Courier New", "Verdana", "Tahoma", "Segoe UI", "Consolas" })
                fontList.Items.Add(new ListBoxItem { Content = f, FontFamily = new FontFamily(f) });
            // 选中当前字体
            foreach (ListBoxItem item in fontList.Items)
                if ((string)item.Content == t.Font.Family) { item.IsSelected = true; fontList.ScrollIntoView(item); break; }
            fontList.SelectionChanged += (_, __) => { if (fontList.SelectedItem is ListBoxItem si) fontInput.Text = (string)si.Content; };
            fontPanel.Children.Add(fontList);
            Grid.SetColumn(fontPanel, 0);
            topGrid.Children.Add(fontPanel);

            // 字形列
            var stylePanel = new DockPanel();
            stylePanel.Children.Add(new TextBlock { Text = "字形(Y):", FontSize = 11, Margin = new Thickness(0, 0, 0, 2) });
            DockPanel.SetDock(stylePanel.Children[0], Dock.Top);
            var styleInput = new TextBox { FontSize = 12, Padding = new Thickness(4, 2, 4, 2), Margin = new Thickness(0, 0, 0, 2), IsReadOnly = true };
            // 确定当前字形
            string curStyle = "常规";
            if (t.Font.Bold && t.Font.Italic) curStyle = "粗斜体";
            else if (t.Font.Bold) curStyle = "粗体";
            else if (t.Font.Italic) curStyle = "斜体";
            styleInput.Text = curStyle;
            DockPanel.SetDock(styleInput, Dock.Top);
            stylePanel.Children.Add(styleInput);
            var styleList = new ListBox { FontSize = 12 };
            styleList.Items.Add(new ListBoxItem { Content = "常规" });
            styleList.Items.Add(new ListBoxItem { Content = "斜体", FontStyle = FontStyles.Italic });
            styleList.Items.Add(new ListBoxItem { Content = "粗体", FontWeight = FontWeights.Bold });
            styleList.Items.Add(new ListBoxItem { Content = "粗斜体", FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic });
            foreach (ListBoxItem item in styleList.Items)
                if ((string)item.Content == curStyle) { item.IsSelected = true; break; }
            styleList.SelectionChanged += (_, __) => { if (styleList.SelectedItem is ListBoxItem si) styleInput.Text = (string)si.Content; };
            stylePanel.Children.Add(styleList);
            Grid.SetColumn(stylePanel, 2);
            topGrid.Children.Add(stylePanel);

            // 大小列
            var sizePanel = new DockPanel();
            sizePanel.Children.Add(new TextBlock { Text = "大小(S):", FontSize = 11, Margin = new Thickness(0, 0, 0, 2) });
            DockPanel.SetDock(sizePanel.Children[0], Dock.Top);
            var sizeInput = new TextBox { Text = t.Font.Size.ToString(), FontSize = 12, Padding = new Thickness(4, 2, 4, 2), Margin = new Thickness(0, 0, 0, 2) };
            DockPanel.SetDock(sizeInput, Dock.Top);
            sizePanel.Children.Add(sizeInput);
            var sizeList = new ListBox { FontSize = 12 };
            foreach (var sz in new[] { "8", "9", "10", "10.5", "11", "12", "13", "14", "16", "18", "20", "22", "24", "26", "28", "36", "48", "72" })
                sizeList.Items.Add(sz);
            sizeList.SelectedItem = t.Font.Size.ToString();
            sizeList.SelectionChanged += (_, __) => { if (sizeList.SelectedItem is string s) sizeInput.Text = s; };
            sizePanel.Children.Add(sizeList);
            Grid.SetColumn(sizePanel, 4);
            topGrid.Children.Add(sizePanel);

            Grid.SetRow(topGrid, 0);
            mainGrid.Children.Add(topGrid);

            // 效果区
            var effectGroup = new GroupBox { Header = "效果", FontSize = 11, Padding = new Thickness(8, 4, 8, 4) };
            var effectStack = new StackPanel();
            var chkStrike = new CheckBox { Content = "删除线(K)", FontSize = 11, Margin = new Thickness(0, 2, 0, 2) };
            var chkUnderline = new CheckBox { Content = "下划线(U)", FontSize = 11, IsChecked = t.Font.Underline, Margin = new Thickness(0, 2, 0, 2) };
            effectStack.Children.Add(chkStrike);
            effectStack.Children.Add(chkUnderline);
            effectGroup.Content = effectStack;
            Grid.SetRow(effectGroup, 2);
            mainGrid.Children.Add(effectGroup);

            // 预览区
            var previewGroup = new GroupBox { Header = "预览", FontSize = 11, Padding = new Thickness(8) };
            var previewText = new TextBlock { Text = "微软中文软件", FontSize = 16, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            previewGroup.Content = previewText;
            Grid.SetRow(previewGroup, 4);
            mainGrid.Children.Add(previewGroup);

            // 实时预览更新
            Action updatePreview = () =>
            {
                previewText.FontFamily = new FontFamily(fontInput.Text);
                if (double.TryParse(sizeInput.Text, out var psz) && psz > 0 && psz <= 100)
                    previewText.FontSize = psz;
                var st = styleInput.Text;
                previewText.FontWeight = (st == "粗体" || st == "粗斜体") ? FontWeights.Bold : FontWeights.Normal;
                previewText.FontStyle = (st == "斜体" || st == "粗斜体") ? FontStyles.Italic : FontStyles.Normal;
                previewText.TextDecorations = chkUnderline.IsChecked == true ? TextDecorations.Underline : null;
            };
            fontInput.TextChanged += (_, __) => updatePreview();
            sizeInput.TextChanged += (_, __) => updatePreview();
            styleInput.TextChanged += (_, __) => updatePreview();
            chkUnderline.Checked += (_, __) => updatePreview();
            chkUnderline.Unchecked += (_, __) => updatePreview();
            updatePreview();

            // 按钮
            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var btnOk = new Button { Content = "确定", Width = 75, Height = 26, Margin = new Thickness(0, 0, 8, 0), IsDefault = true };
            var btnCancel = new Button { Content = "取消", Width = 75, Height = 26, IsCancel = true };
            btnPanel.Children.Add(btnOk);
            btnPanel.Children.Add(btnCancel);
            Grid.SetRow(btnPanel, 6);
            mainGrid.Children.Add(btnPanel);

            btnOk.Click += (_, __) =>
            {
                PushUndo();
                t.Font.Family = fontInput.Text;
                if (double.TryParse(sizeInput.Text, out var fsz) && fsz > 0)
                    t.Font.Size = fsz;
                var style = styleInput.Text;
                t.Font.Bold = (style == "粗体" || style == "粗斜体");
                t.Font.Italic = (style == "斜体" || style == "粗斜体");
                t.Font.Underline = chkUnderline.IsChecked == true;
                MarkDirty();
                RefreshUI();
                dlg.DialogResult = true;
            };

            dlg.Content = mainGrid;
            dlg.ShowDialog();
        }

        // ============================== 表达式编辑器 ==============================

        private void ShowExpressionEditor(string currentValue, Action<string> onCommit)
        {
            var dlg = new Window
            {
                Title = "表达式编辑器", Width = 520, Height = 480,
                WindowStartupLocation = WindowStartupLocation.CenterOwner, Owner = this,
                Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                ResizeMode = ResizeMode.CanResize,
            };

            var mainPanel = new DockPanel { Margin = new Thickness(12) };

            // 顶部：表达式输入
            var topPanel = new StackPanel();
            topPanel.Children.Add(new TextBlock { Text = "表达式:", FontSize = 12, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 4) });
            var exprBox = new TextBox { Text = currentValue, FontSize = 12, FontFamily = new FontFamily("Consolas, Courier New"), MinHeight = 28, Padding = new Thickness(4, 4, 4, 4), AcceptsReturn = true, MaxHeight = 60, TextWrapping = TextWrapping.Wrap };
            exprBox.SelectAll();
            topPanel.Children.Add(exprBox);
            DockPanel.SetDock(topPanel, Dock.Top);
            mainPanel.Children.Add(topPanel);

            // 下部：快捷插入区
            var contentGrid = new Grid { Margin = new Thickness(0, 8, 0, 0) };
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // 左侧：系统变量
            var sysPanel = new StackPanel();
            sysPanel.Children.Add(new TextBlock { Text = "▸ 系统变量:", FontSize = 11, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 4) });
            foreach (var kv in new[] { ("{{PAGE}}", "当前页码"), ("{{TOTAL_PAGES}}", "总页数"), ("{{REPORT_DATE}}", "报表日期"), ("{{NOW}}", "当前时间"), ("{{ROW_NUMBER}}", "行号") })
                AddExprBtn(sysPanel, kv.Item1, kv.Item2, exprBox);
            Grid.SetColumn(sysPanel, 0);
            contentGrid.Children.Add(sysPanel);

            // 右侧：聚合函数
            var aggPanel = new StackPanel();
            aggPanel.Children.Add(new TextBlock { Text = "▸ 聚合函数:", FontSize = 11, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 4) });
            foreach (var kv in new[] { ("{{SUM(field)}}", "求和"), ("{{COUNT(field)}}", "计数"), ("{{AVG(field)}}", "平均值"), ("{{MIN(field)}}", "最小值"), ("{{MAX(field)}}", "最大值") })
                AddExprBtn(aggPanel, kv.Item1, kv.Item2, exprBox);
            aggPanel.Children.Add(new TextBlock { Text = "\n▸ 字段引用:", FontSize = 11, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 8, 0, 4) });
            foreach (var kv in new[] { ("{{currentRow.fieldName}}", "当前行字段"), ("{{dsName.fieldName}}", "数据源字段") })
                AddExprBtn(aggPanel, kv.Item1, kv.Item2, exprBox);
            Grid.SetColumn(aggPanel, 2);
            contentGrid.Children.Add(aggPanel);

            mainPanel.Children.Add(contentGrid);

            // 底部提示
            var hint = new TextBlock { Text = "💡 提示: 用 {{ }} 包裹表达式，点击下方按钮快速插入模板", FontSize = 10, Foreground = Brushes.DimGray, Margin = new Thickness(0, 6, 0, 0) };
            DockPanel.SetDock(hint, Dock.Bottom);
            mainPanel.Children.Add(hint);

            // 按钮
            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 0, 0) };
            var btnOk = new Button { Content = "确定", Width = 75, Height = 26, Margin = new Thickness(0, 0, 8, 0), IsDefault = true };
            var btnCancel = new Button { Content = "取消", Width = 75, Height = 26, IsCancel = true };
            btnPanel.Children.Add(btnOk); btnPanel.Children.Add(btnCancel);
            mainPanel.Children.Add(btnPanel);

            btnOk.Click += (_, __) => { onCommit(exprBox.Text); dlg.DialogResult = true; };

            dlg.Content = mainPanel;
            dlg.ShowDialog();
        }

        private static void AddExprBtn(StackPanel sp, string template, string desc, TextBox target)
        {
            var btn = new Button
            {
                Content = template + "  " + desc,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                FontSize = 10,
                FontFamily = new FontFamily("Consolas, Courier New"),
                Padding = new Thickness(6, 3, 6, 3),
                Margin = new Thickness(0, 1, 0, 1),
                Background = new SolidColorBrush(Color.FromRgb(225, 225, 225)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                BorderThickness = new Thickness(1),
                Foreground = Brushes.Black,
                Cursor = Cursors.Hand,
            };
            btn.Click += (_, __) =>
            {
                var p = template.Replace("field", "").Replace("fieldName", "").Replace("dsName", "");
                var idx = p.IndexOf("}}");
                var cursorPos = idx >= 0 ? idx : p.Length - 1;
                // 插入模板并选中变量名部分
                var selStart = target.SelectionStart;
                target.Text = target.Text.Insert(selStart, p);
                target.SelectionStart = selStart + cursorPos;
                target.SelectionLength = 0;
                target.Focus();
            };
            sp.Children.Add(btn);
        }

        // ============================== 数据源管理 ==============================

        private void LoadPreviewData()
        {
            var dlg = new OpenFileDialog { Filter = "JSON文件|*.json|所有文件|*.*", Title = "加载预览数据" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var text = File.ReadAllText(dlg.FileName);
                    // 简单JSON解析: 支持 {"key":"value",...} 格式
                    _previewData = ParseSimpleJson(text);
                    _statusText.Text = "已加载预览数据: " + System.IO.Path.GetFileName(dlg.FileName) + " (" + (_previewData?.Count ?? 0) + " 个字段)";
                    if (_viewMode == "preview") _previewRenderer.Render(_template!, _zoom, _previewData);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("加载失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>简单JSON解析 (浅层平铺 key-value), 兼容net462用Newtonsoft风格手动解析</summary>
        private static Dictionary<string, object>? ParseSimpleJson(string json)
        {
            json = json.Trim();
            // 如果是数组，取第一个对象
            if (json.StartsWith("["))
            {
                int depth = 0;
                int start = -1;
                for (int i = 0; i < json.Length; i++)
                {
                    if (json[i] == '{') { if (depth == 0) start = i; depth++; }
                    else if (json[i] == '}') { depth--; if (depth == 0 && start >= 0) { json = json.Substring(start, i - start + 1); break; } }
                }
            }
            if (!json.StartsWith("{")) return null;
            var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            // 简单解析: 用TemplateParser的JSON库(Newtonsoft)
            try
            {
                var parser = new TemplateParser();
                // 借助 Newtonsoft.Json 解析
                var dict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (dict != null)
                    foreach (var kv in dict)
                        result[kv.Key] = kv.Value ?? "";
            }
            catch { }
            return result;
        }


        /// <summary>快捷键一览对话框</summary>
        private void ShowShortcutsDialog()
        {
            var dlg = new Window
            {
                Title = "快捷键列表",
                Width = 380, Height = 420,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this, ResizeMode = ResizeMode.NoResize,
            };
            var sp = new StackPanel { Margin = new Thickness(12) };
            var shortcuts = new[]
            {
                ("Ctrl+N", "新建模板"), ("Ctrl+O", "打开模板"), ("Ctrl+S", "保存"),
                ("Ctrl+Z", "撤销"), ("Ctrl+Y", "重做"),
                ("Ctrl+C", "复制"), ("Ctrl+X", "剪切"), ("Ctrl+V", "粘贴"),
                ("Ctrl+D", "复制并偏移"), ("Delete", "删除"), ("Ctrl+A", "全选"),
                ("F1", "快捷键列表"), ("Tab", "切换选中"), ("Shift+Tab", "反向切换"),
                ("方向键", "微调0.5mm"), ("Shift+方向键", "微调5mm"),
                ("Ctrl+滚轮", "缩放"),
            };
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            int row = 0;
            foreach (var (key, desc) in shortcuts)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(22) });
                grid.Children.Add(new TextBlock { Text = key, FontWeight = FontWeights.Bold, Foreground = Brushes.DarkBlue, VerticalAlignment = VerticalAlignment.Center });
                var tb = new TextBlock { Text = desc, Foreground = Brushes.Black, VerticalAlignment = VerticalAlignment.Center };
                Grid.SetColumn(tb, 1);
                Grid.SetRow(tb, row);
                Grid.SetColumn(grid.Children[grid.Children.Count - 1], 0);
                Grid.SetRow(grid.Children[grid.Children.Count - 1], row);
                grid.Children.Add(tb);
                row++;
            }
            sp.Children.Add(grid);
            var btnClose = new Button { Content = "关闭", Width = 70, Height = 26, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 12, 0, 0), IsDefault = true };
            btnClose.Click += (_, __) => dlg.Close();
            sp.Children.Add(btnClose);
            dlg.Content = sp;
            dlg.ShowDialog();
        }

        /// <summary>关于对话框</summary>
        private void ShowAboutDialog()
        {
            var dlg = new Window
            {
                Title = "关于",
                Width = 320, Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this, ResizeMode = ResizeMode.NoResize,
            };
            var sp = new StackPanel { Margin = new Thickness(20), HorizontalAlignment = HorizontalAlignment.Center };
            sp.Children.Add(new TextBlock { Text = "报表设计器", FontSize = 18, FontWeight = FontWeights.Bold, Foreground = Brushes.DarkBlue, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 8) });
            sp.Children.Add(new TextBlock { Text = "基于 ReportEngine.Core", FontSize = 12, Foreground = Brushes.Gray, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 4) });
            sp.Children.Add(new TextBlock { Text = "版本 1.0.0", FontSize = 11, Foreground = Brushes.DimGray, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 16) });
            sp.Children.Add(new TextBlock { Text = "支持元素拖拽、吸附对齐、格式刷、分组、旋转、透明度等", FontSize = 10, Foreground = Brushes.Gray, TextWrapping = TextWrapping.Wrap, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 12) });
            var btnClose = new Button { Content = "关闭", Width = 70, Height = 26, HorizontalAlignment = HorizontalAlignment.Center, IsDefault = true };
            btnClose.Click += (_, __) => dlg.Close();
            sp.Children.Add(btnClose);
            dlg.Content = sp;
            dlg.ShowDialog();
        }

        /// <summary>搜索元素对话框</summary>
        private void SearchElement()
        {
            if (_template == null) return;
            var dlg = new Window
            {
                Title = "搜索元素",
                Width = 320, Height = 120,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this, ResizeMode = ResizeMode.NoResize,
            };
            var sp = new StackPanel { Margin = new Thickness(12) };
            sp.Children.Add(new TextBlock { Text = "搜索名称或ID:", Margin = new Thickness(0, 0, 0, 4), Foreground = Brushes.Black });
            var tb = new TextBox { Text = "", Margin = new Thickness(0, 0, 0, 8), Foreground = Brushes.Black };
            sp.Children.Add(tb);
            var btnOk = new Button { Content = "搜索", Width = 70, Height = 26, HorizontalAlignment = HorizontalAlignment.Right, IsDefault = true };
            btnOk.Click += (_, __) =>
            {
                var keyword = tb.Text.Trim();
                if (string.IsNullOrEmpty(keyword)) { dlg.Close(); return; }
                ReportElement? found = null;
                Band? foundBand = null;
                foreach (var band in _template.Bands)
                {
                    foreach (var el in band.Elements)
                    {
                        if ((!string.IsNullOrEmpty(el.Name) && el.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                            el.Id.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            found = el;
                            foundBand = band;
                            break;
                        }
                    }
                    if (found != null) break;
                }
                if (found != null)
                {
                    _selectedElement = found;
                    _selectedBand = foundBand;
                    _selectedElements.Clear();
                    _selectedElements.Add(found);
                    RefreshUI();
                    _statusText.Text = "已找到: " + (found.Name ?? found.GetType().Name);
                }
                else
                {
                    _statusText.Text = "未找到匹配的元素";
                }
                dlg.Close();
            };
            sp.Children.Add(btnOk);
            dlg.Content = sp;
            dlg.ShowDialog();
        }

        /// <summary>构建导出数据：如果有预览数据则使用，否则返回空</summary>
        private Dictionary<string, List<Dictionary<string, object>>> BuildExportData()
        {
            var result = new Dictionary<string, List<Dictionary<string, object>>>();
            if (_previewData != null && _previewData.Count > 0)
            {
                // 将预览数据转换为渲染器需要的格式
                var row = new Dictionary<string, object>();
                foreach (var kv in _previewData)
                    row[kv.Key] = kv.Value;
                // 默认数据源名为"Default"
                result["Default"] = new List<Dictionary<string, object>> { row };
            }
            return result;
        }

        private void ShowGridSettingsDialog()
        {
            var dlg = new Window
            {
                Title = "网格设置",
                Width = 280, Height = 180,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this, ResizeMode = ResizeMode.NoResize,
            };
            var sp = new StackPanel { Margin = new Thickness(12) };
            sp.Children.Add(new TextBlock { Text = "网格间距(mm):", Margin = new Thickness(0, 0, 0, 4), Foreground = Brushes.Black });
            var tbSpacing = new TextBox { Text = _gridSpacingMm.ToString(), Margin = new Thickness(0, 0, 0, 8), Foreground = Brushes.Black };
            sp.Children.Add(tbSpacing);
            sp.Children.Add(new TextBlock { Text = "吸附距离阈值(mm): 1.5", Margin = new Thickness(0, 0, 0, 12), Foreground = Brushes.Gray, FontSize = 10 });
            var btnOk = new Button { Content = "确定", Width = 70, Height = 26, HorizontalAlignment = HorizontalAlignment.Right, IsDefault = true };
            btnOk.Click += (_, __) =>
            {
                if (double.TryParse(tbSpacing.Text, out var d) && d > 0 && d <= 50)
                {
                    _gridSpacingMm = d;
                    _canvasRenderer.Render(BuildCanvasRenderContext(), _selectedElements, _selectedBand);
                }
                dlg.Close();
            };
            sp.Children.Add(btnOk);
            dlg.Content = sp;
            dlg.ShowDialog();
        }

        private void ShowDataSourceDialog()
        {
            if (_template == null) return;

            var dlg = new Window
            {
                Title = "数据源管理", Width = 600, Height = 480,
                WindowStartupLocation = WindowStartupLocation.CenterOwner, Owner = this,
                Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                ResizeMode = ResizeMode.CanResize,
            };

            var mainGrid = new Grid { Margin = new Thickness(12) };
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });

            // 左侧：数据源列表
            var leftPanel = new DockPanel();
            leftPanel.Children.Add(new TextBlock { Text = "数据源列表:", FontSize = 12, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 4) });
            DockPanel.SetDock(leftPanel.Children[0], Dock.Top);
            var dsList = new ListBox { FontSize = 12, MinHeight = 300 };
            RefreshDsList(dsList, null);
            leftPanel.Children.Add(dsList);
            // 添加/删除按钮
            var dsBtnPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 6, 0, 0) };
            var btnAddDs = new Button { Content = "+ 新增", Width = 60, Height = 26, Margin = new Thickness(0, 0, 4, 0) };
            var btnDelDs = new Button { Content = "- 删除", Width = 60, Height = 26 };
            dsBtnPanel.Children.Add(btnAddDs); dsBtnPanel.Children.Add(btnDelDs);
            DockPanel.SetDock(dsBtnPanel, Dock.Bottom);
            leftPanel.Children.Add(dsBtnPanel);
            Grid.SetColumn(leftPanel, 0);
            mainGrid.Children.Add(leftPanel);

            // 右侧：字段列表
            var rightPanel = new DockPanel();
            var fieldHeader = new DockPanel { Margin = new Thickness(0, 0, 0, 4) };
            fieldHeader.Children.Add(new TextBlock { Text = "字段列表:", FontSize = 12, FontWeight = FontWeights.Bold });
            var selectedDsLabel = new TextBlock { Text = "", FontSize = 11, Foreground = Brushes.DimGray, Margin = new Thickness(8, 0, 0, 0) };
            DockPanel.SetDock(selectedDsLabel, Dock.Right);
            fieldHeader.Children.Add(selectedDsLabel);
            DockPanel.SetDock(fieldHeader, Dock.Top);
            rightPanel.Children.Add(fieldHeader);

            var fieldList = new ListBox { FontSize = 12, MinHeight = 280 };
            rightPanel.Children.Add(fieldList);

            // 添加/编辑字段按钮
            var fieldBtnPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 6, 0, 0) };
            var btnAddField = new Button { Content = "+ 添加", Width = 60, Height = 26, Margin = new Thickness(0, 0, 4, 0) };
            var btnEditField = new Button { Content = "✎ 编辑", Width = 60, Height = 26, Margin = new Thickness(0, 0, 4, 0) };
            var btnDelField = new Button { Content = "- 删除", Width = 60, Height = 26 };
            fieldBtnPanel.Children.Add(btnAddField); fieldBtnPanel.Children.Add(btnEditField); fieldBtnPanel.Children.Add(btnDelField);
            DockPanel.SetDock(fieldBtnPanel, Dock.Bottom);
            rightPanel.Children.Add(fieldBtnPanel);
            Grid.SetColumn(rightPanel, 2);
            mainGrid.Children.Add(rightPanel);

            // 事件：选中数据源
            dsList.SelectionChanged += (_, __) =>
            {
                if (dsList.SelectedItem is DataSourceDef ds)
                {
                    selectedDsLabel.Text = ds.Name;
                    RefreshFieldList(fieldList, ds);
                }
            };

            // 新增数据源
            btnAddDs.Click += (_, __) =>
            {
                var input = new InputDialog("新增数据源", "请输入数据源名称:", "DataSource" + (_template.DataSources.Count + 1));
                input.Owner = dlg;
                if (input.ShowDialog() == true)
                {
                    PushUndo();
                    var newDs = new DataSourceDef { Name = input.Result };
                    _template.DataSources.Add(newDs);
                    RefreshDsList(dsList, newDs);
                    MarkDirty();
                }
            };

            // 删除数据源
            btnDelDs.Click += (_, __) =>
            {
                if (dsList.SelectedItem is DataSourceDef ds)
                {
                    PushUndo();
                    _template.DataSources.Remove(ds);
                    RefreshDsList(dsList, null);
                    fieldList.Items.Clear();
                    selectedDsLabel.Text = "";
                    MarkDirty();
                }
            };

            // 添加字段
            btnAddField.Click += (_, __) =>
            {
                if (dsList.SelectedItem is DataSourceDef ds)
                {
                    var input = new FieldInputDialog("添加字段");
                    input.Owner = dlg;
                    if (input.ShowDialog() == true)
                    {
                        PushUndo();
                        ds.Fields.Add(new FieldDef { Name = input.FieldName, Type = input.FieldType, Format = input.FieldFormat });
                        RefreshFieldList(fieldList, ds);
                        MarkDirty();
                    }
                }
            };

            // 编辑字段
            btnEditField.Click += (_, __) =>
            {
                if (dsList.SelectedItem is DataSourceDef ds && fieldList.SelectedItem is FieldDef fd)
                {
                    var input = new FieldInputDialog("编辑字段", fd.Name, fd.Type, fd.Format);
                    input.Owner = dlg;
                    if (input.ShowDialog() == true)
                    {
                        PushUndo();
                        fd.Name = input.FieldName;
                        fd.Type = input.FieldType;
                        fd.Format = input.FieldFormat;
                        RefreshFieldList(fieldList, ds);
                        MarkDirty();
                    }
                }
            };

            // 删除字段
            btnDelField.Click += (_, __) =>
            {
                if (dsList.SelectedItem is DataSourceDef ds && fieldList.SelectedItem is FieldDef fd)
                {
                    PushUndo();
                    ds.Fields.Remove(fd);
                    RefreshFieldList(fieldList, ds);
                    MarkDirty();
                }
            };

            // 按钮
            var bottomPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 12, 0, 0) };
            var btnClose = new Button { Content = "关闭", Width = 75, Height = 26, IsCancel = true };
            btnClose.Click += (_, __) => dlg.DialogResult = true;
            bottomPanel.Children.Add(btnClose);
            mainGrid.Children.Add(bottomPanel);

            dlg.Content = mainGrid;
            dlg.ShowDialog();
        }

        /// <summary>数据绑定向导 — 引导用户选择数据源并绑定字段到元素</summary>
        private void ShowDataBindingWizard()
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

            var dlg = new Window
            {
                Title = "数据绑定向导",
                Width = 420, Height = 380,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this, ResizeMode = ResizeMode.NoResize,
            };
            var sp = new StackPanel { Margin = new Thickness(12) };

            // 步骤1：选择数据源
            sp.Children.Add(new TextBlock { Text = "步骤1: 选择数据源", FontSize = 13, FontWeight = FontWeights.Bold, Foreground = Brushes.DarkBlue, Margin = new Thickness(0, 0, 0, 6) });
            var dsCombo = new ComboBox { Margin = new Thickness(0, 0, 0, 12) };
            foreach (var ds in _template.DataSources)
                dsCombo.Items.Add(ds.Name);
            dsCombo.SelectedIndex = 0;
            sp.Children.Add(dsCombo);

            // 步骤2：选择字段
            sp.Children.Add(new TextBlock { Text = "步骤2: 选择要绑定的字段", FontSize = 13, FontWeight = FontWeights.Bold, Foreground = Brushes.DarkBlue, Margin = new Thickness(0, 8, 0, 6) });
            var fieldList = new ListBox { Height = 150, Margin = new Thickness(0, 0, 0, 12) };
            void RefreshFields()
            {
                fieldList.Items.Clear();
                var ds = _template.DataSources.FirstOrDefault(d => d.Name == dsCombo.SelectedItem?.ToString());
                if (ds != null)
                    foreach (var f in ds.Fields)
                        fieldList.Items.Add(f.Name);
            }
            dsCombo.SelectionChanged += (_, __) => RefreshFields();
            RefreshFields();
            sp.Children.Add(fieldList);

            // 步骤3：绑定到元素的哪个属性
            sp.Children.Add(new TextBlock { Text = "步骤3: 绑定到元素的属性", FontSize = 13, FontWeight = FontWeights.Bold, Foreground = Brushes.DarkBlue, Margin = new Thickness(0, 8, 0, 6) });
            var propCombo = new ComboBox { Margin = new Thickness(0, 0, 0, 16) };
            propCombo.Items.Add("文本内容 (Text)");
            propCombo.Items.Add("可见性表达式 (VisibleExpression)");
            propCombo.SelectedIndex = 0;
            sp.Children.Add(propCombo);

            // 按钮
            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var btnCancel = new Button { Content = "取消", Width = 70, Height = 26, Margin = new Thickness(0, 0, 8, 0), IsCancel = true };
            btnCancel.Click += (_, __) => dlg.Close();
            btnPanel.Children.Add(btnCancel);
            var btnBind = new Button { Content = "绑定", Width = 70, Height = 26, IsDefault = true };
            btnBind.Click += (_, __) =>
            {
                var dsName = dsCombo.SelectedItem?.ToString();
                var fieldName = fieldList.SelectedItem?.ToString();
                var propChoice = propCombo.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(dsName) || string.IsNullOrEmpty(fieldName))
                {
                    MessageBox.Show("请选择数据源和字段", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                PushUndo();
                var expression = $"[{dsName}.{fieldName}]";
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
                _statusText.Text = $"已绑定: {expression}";
                dlg.Close();
            };
            btnPanel.Children.Add(btnBind);
            sp.Children.Add(btnPanel);

            dlg.Content = sp;
            dlg.ShowDialog();
        }

        /// <summary>模板参数管理对话框</summary>
        private void ShowTemplateParamsDialog()
        {
            if (_template == null) return;

            var dlg = new Window
            {
                Title = "模板参数管理",
                Width = 450, Height = 350,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this, ResizeMode = ResizeMode.NoResize,
            };
            var sp = new StackPanel { Margin = new Thickness(12) };

            // 参数列表
            sp.Children.Add(new TextBlock { Text = "模板参数列表（用于导出时变量替换）", FontSize = 12, FontWeight = FontWeights.Bold, Foreground = Brushes.DarkBlue, Margin = new Thickness(0, 0, 0, 6) });
            var paramList = new ListBox { Height = 180, Margin = new Thickness(0, 0, 0, 8) };
            void RefreshList()
            {
                paramList.Items.Clear();
                foreach (var p in _template.Parameters)
                    paramList.Items.Add($"{p.Label ?? p.Name} ({p.Name}) = {p.DefaultValue}");
            }
            RefreshList();
            sp.Children.Add(paramList);

            // 添加/删除按钮
            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            var btnAdd = new Button { Content = "+ 添加", Width = 70, Height = 26, Margin = new Thickness(0, 0, 4, 0) };
            var btnDel = new Button { Content = "- 删除", Width = 70, Height = 26 };
            btnPanel.Children.Add(btnAdd);
            btnPanel.Children.Add(btnDel);
            sp.Children.Add(btnPanel);

            // 添加参数
            btnAdd.Click += (_, __) =>
            {
                var inputDlg = new Window
                {
                    Title = "添加参数",
                    Width = 300, Height = 220,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = dlg, ResizeMode = ResizeMode.NoResize,
                };
                var inputSp = new StackPanel { Margin = new Thickness(12) };
                inputSp.Children.Add(new TextBlock { Text = "参数名称:", Margin = new Thickness(0, 0, 0, 4) });
                var tbName = new TextBox { Margin = new Thickness(0, 0, 0, 8) };
                inputSp.Children.Add(tbName);
                inputSp.Children.Add(new TextBlock { Text = "显示名称:", Margin = new Thickness(0, 0, 0, 4) });
                var tbLabel = new TextBox { Margin = new Thickness(0, 0, 0, 8) };
                inputSp.Children.Add(tbLabel);
                inputSp.Children.Add(new TextBlock { Text = "默认值:", Margin = new Thickness(0, 0, 0, 4) });
                var tbDefault = new TextBox { Margin = new Thickness(0, 0, 0, 12) };
                inputSp.Children.Add(tbDefault);
                var btnOk = new Button { Content = "确定", Width = 70, Height = 26, HorizontalAlignment = HorizontalAlignment.Right, IsDefault = true };
                btnOk.Click += (_, ___) =>
                {
                    if (!string.IsNullOrWhiteSpace(tbName.Text))
                    {
                        PushUndo();
                        _template.Parameters.Add(new ReportEngine.Core.TemplateParam
                        {
                            Name = tbName.Text.Trim(),
                            Label = string.IsNullOrWhiteSpace(tbLabel.Text) ? null : tbLabel.Text.Trim(),
                            DefaultValue = tbDefault.Text,
                            Type = "string"
                        });
                        MarkDirty();
                        RefreshList();
                        inputDlg.Close();
                    }
                };
                inputSp.Children.Add(btnOk);
                inputDlg.Content = inputSp;
                inputDlg.ShowDialog();
            };

            // 删除参数
            btnDel.Click += (_, __) =>
            {
                if (paramList.SelectedIndex >= 0 && paramList.SelectedIndex < _template.Parameters.Count)
                {
                    PushUndo();
                    _template.Parameters.RemoveAt(paramList.SelectedIndex);
                    MarkDirty();
                    RefreshList();
                }
            };

            // 关闭按钮
            var btnClose = new Button { Content = "关闭", Width = 70, Height = 26, HorizontalAlignment = HorizontalAlignment.Right, IsCancel = true };
            btnClose.Click += (_, __) => dlg.Close();
            sp.Children.Add(btnClose);

            dlg.Content = sp;
            dlg.ShowDialog();
        }

        private void RefreshDsList(ListBox list, DataSourceDef? selectAfter)
        {
            list.Items.Clear();
            if (_template == null) return;
            foreach (var ds in _template.DataSources)
                list.Items.Add(ds);
            if (selectAfter != null)
                for (int i = 0; i < list.Items.Count; i++)
                    if (list.Items[i] == selectAfter) { list.SelectedIndex = i; return; }
            if (list.Items.Count > 0) list.SelectedIndex = 0;
        }

        private static void RefreshFieldList(ListBox list, DataSourceDef ds)
        {
            list.Items.Clear();
            foreach (var f in ds.Fields)
                list.Items.Add(f);
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

        private class InputDialog : Window
        {
            public string Result { get; private set; } = "";
            public InputDialog(string title, string prompt, string defaultText = "")
            {
                Title = title; Width = 360; Height = 160;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
                ResizeMode = ResizeMode.NoResize;
                var sp = new StackPanel { Margin = new Thickness(12) };
                sp.Children.Add(new TextBlock { Text = prompt, FontSize = 12, Margin = new Thickness(0, 0, 0, 6) });
                var tb = new TextBox { Text = defaultText, FontSize = 12, Padding = new Thickness(4, 3, 4, 3) };
                tb.SelectAll(); tb.Focus();
                sp.Children.Add(tb);
                var btnRow = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 12, 0, 0) };
                var btnOk = new Button { Content = "确定", Width = 60, Height = 26, Margin = new Thickness(0, 0, 8, 0), IsDefault = true };
                var btnCancel = new Button { Content = "取消", Width = 60, Height = 26, IsCancel = true };
                btnOk.Click += (_, __) => { Result = tb.Text; DialogResult = true; };
                btnCancel.Click += (_, __) => DialogResult = false;
                btnRow.Children.Add(btnOk); btnRow.Children.Add(btnCancel);
                sp.Children.Add(btnRow);
                Content = sp;
            }
        }

        private class FieldInputDialog : Window
        {
            public string FieldName { get; private set; } = "";
            public string FieldType { get; private set; } = "string";
            public string? FieldFormat { get; private set; }
            public FieldInputDialog(string title, string name = "", string type = "string", string? format = null)
            {
                Title = title; Width = 380; Height = 260;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
                ResizeMode = ResizeMode.NoResize;
                var sp = new StackPanel { Margin = new Thickness(12) };
                sp.Children.Add(new TextBlock { Text = "字段名:", FontSize = 11, Margin = new Thickness(0, 0, 0, 2) });
                var nameBox = new TextBox { Text = name, FontSize = 12, Padding = new Thickness(4, 3, 4, 3) };
                sp.Children.Add(nameBox);
                sp.Children.Add(new TextBlock { Text = "类型:", FontSize = 11, Margin = new Thickness(0, 6, 0, 2) });
                var typeCombo = new ComboBox { FontSize = 12, Padding = new Thickness(4, 3, 4, 3) };
                foreach (var t in new[] { "string", "int", "decimal", "double", "DateTime", "bool" }) typeCombo.Items.Add(t);
                typeCombo.SelectedItem = type;
                sp.Children.Add(typeCombo);
                sp.Children.Add(new TextBlock { Text = "格式 (可选):", FontSize = 11, Margin = new Thickness(0, 6, 0, 2) });
                var fmtBox = new TextBox { Text = format ?? "", FontSize = 12, Padding = new Thickness(4, 3, 4, 3) };
                sp.Children.Add(fmtBox);
                var btnRow = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 12, 0, 0) };
                var btnOk = new Button { Content = "确定", Width = 60, Height = 26, Margin = new Thickness(0, 0, 8, 0), IsDefault = true };
                var btnCancel = new Button { Content = "取消", Width = 60, Height = 26, IsCancel = true };
                btnOk.Click += (_, __) => { FieldName = nameBox.Text; FieldType = (string?)typeCombo.SelectedItem ?? "string"; FieldFormat = string.IsNullOrWhiteSpace(fmtBox.Text) ? null : fmtBox.Text; DialogResult = true; };
                btnCancel.Click += (_, __) => DialogResult = false;
                btnRow.Children.Add(btnOk); btnRow.Children.Add(btnCancel);
                sp.Children.Add(btnRow);
                Content = sp;
            }
        }
    }
}