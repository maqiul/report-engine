using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using static ReportEngine.Designer.Wpf.UiFactory;

namespace ReportEngine.Designer.Wpf
{
    public partial class MainWindow
    {
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
    }
}
