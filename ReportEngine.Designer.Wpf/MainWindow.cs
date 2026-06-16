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
    public partial class MainWindow : Window
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

        // ============================== 布局 ==============================

        // 字体工具栏组件
        private ComboBox _fontFamilyCombo = null!;
        private ComboBox _fontSizeCombo = null!;

        private Border _tabDesign = null!;
        private Border _tabPreview = null!;

        /// <summary>组合选中的元素为一组</summary>
        /// <summary>取消选中元素的分组</summary>
        /// <summary>启动格式刷：复制当前选中元素的样式</summary>
        /// <summary>应用格式刷到目标元素</summary>
        /// <summary>关闭格式刷</summary>
        /// <summary>设置自动保存定时器：每60秒自动保存一次草稿</summary>

        /// <summary>启动时检查是否有自动保存的草稿</summary>

        /// <summary>手动保存后删除自动保存草稿</summary>

        /// <summary>重置选中元素的属性为默认值</summary>
        private TextBlock _selectedObjLabel = null!;

        // ============================== 画布渲染 ==============================

        // ============================== 标尺 ==============================

        // ============================== 标尺参考线拖拽 ==============================

        /// <summary>开始 drag：设 _dragMode + _dragStart + CaptureMouse。OnCanvasMouseDown 4 个 start 模式共享。</summary>

        // ============================== 文件操作 ==============================

        /// <summary>导出当前画布为PNG图片</summary>
        /// <summary>批量导出PDF+Excel到同一目录</summary>
        // ============================== 元素操作 ==============================

        /// <summary>Ctrl+D 复制并偏移</summary>
        // ============================== 元素工厂 ==============================

        // ============================== 刷新 ==============================

        /// <summary>更新撤销/重做按钮的启用状态</summary>
        /// <summary>弹出重命名对话框</summary>
        private bool _updatingProps;

        // ============================== 属性面板辅助 ==============================

        /// <summary>多选批量编辑属性面板</summary>
        // ============================== 其他 ==============================

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!ConfirmDiscard()) e.Cancel = true;
            base.OnClosing(e);
        }

        // ============================== 中英文转换 ==============================

        // ============================== 页面设置弹窗 ==============================

        // ============================== 数据源管理 ==============================

        /// <summary>"加载预览数据" 菜单回调: 调用 LoadPreviewDataDialog 弹窗 + 应用副作用。</summary>
        /// <summary>搜索元素对话框</summary>
        /// <summary>构建导出数据：如果有预览数据则使用，否则返回空</summary>

        /// <summary>数据绑定向导 — 引导用户选择数据源并绑定字段到元素</summary>
        // ============================== RelayCmd ==============================

    }
}