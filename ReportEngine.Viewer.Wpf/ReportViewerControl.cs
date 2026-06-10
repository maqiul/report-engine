using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ReportEngine.Core;
using ReportEngine.Core.Rendering;

namespace ReportEngine.Viewer.Wpf
{
    /// <summary>
    /// WPF 报表运行时预览控件。
    /// 功能：分页显示 RenderedReport、缩放、翻页。
    /// 用户在自己的 WPF 应用中嵌入此控件。
    /// </summary>
    public class ReportViewerControl : UserControl
    {
        private RenderedReport? _report;
        private int _currentPage;
        private double _zoom = 1.0;

        private readonly Canvas _canvas;
        private readonly TextBlock _pageLabel;

        private const double PixelsPerMm = 96.0 / 25.4;

        public double Zoom
        {
            get => _zoom;
            set { _zoom = Math.Max(0.25, Math.Min(4, value)); RenderCurrentPage(); UpdateLabel(); }
        }

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_report == null) return;
                _currentPage = Math.Max(0, Math.Min(value, _report.Pages.Count - 1));
                RenderCurrentPage();
                UpdateLabel();
            }
        }

        public int TotalPages => _report?.Pages.Count ?? 0;

        public ReportViewerControl()
        {
            _canvas = new Canvas { Background = Brushes.White, ClipToBounds = true };
            _pageLabel = new TextBlock { Text = "0/0", Margin = new Thickness(8, 2, 8, 2) };

            var toolbar = new ToolBar();
            var btnFirst = new Button { Content = "⏮", ToolTip = "首页" }; btnFirst.Click += (_, __) => CurrentPage = 0;
            var btnPrev = new Button { Content = "◀", ToolTip = "上一页" }; btnPrev.Click += (_, __) => CurrentPage--;
            var btnNext = new Button { Content = "▶", ToolTip = "下一页" }; btnNext.Click += (_, __) => CurrentPage++;
            var btnLast = new Button { Content = "⏭", ToolTip = "末页" }; btnLast.Click += (_, __) => { if (_report != null) CurrentPage = _report.Pages.Count - 1; };
            var btnZoomIn = new Button { Content = "🔍+", ToolTip = "放大" }; btnZoomIn.Click += (_, __) => Zoom += 0.25;
            var btnZoomOut = new Button { Content = "🔍-", ToolTip = "缩小" }; btnZoomOut.Click += (_, __) => Zoom -= 0.25;
            var btnFit = new Button { Content = "适合宽度", ToolTip = "适合宽度" }; btnFit.Click += (_, __) => FitWidth();

            toolbar.Items.Add(btnFirst);
            toolbar.Items.Add(btnPrev);
            toolbar.Items.Add(_pageLabel);
            toolbar.Items.Add(btnNext);
            toolbar.Items.Add(btnLast);
            toolbar.Items.Add(new Separator());
            toolbar.Items.Add(btnZoomOut);
            toolbar.Items.Add(btnZoomIn);
            toolbar.Items.Add(btnFit);

            var scroll = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = _canvas,
                Background = new SolidColorBrush(Color.FromRgb(80, 80, 80)),
            };

            var dock = new DockPanel();
            DockPanel.SetDock(toolbar, Dock.Top);
            dock.Children.Add(toolbar);
            dock.Children.Add(scroll);
            Content = dock;
        }

        public void SetReport(RenderedReport report)
        {
            _report = report;
            _currentPage = 0;
            FitWidth();
            RenderCurrentPage();
            UpdateLabel();
        }

        public void FitWidth()
        {
            if (_report == null) return;
            double availW = ActualWidth > 0 ? ActualWidth - 60 : 700;
            double pageW = _report.PageWidth * PixelsPerMm;
            if (pageW > 0) Zoom = availW / pageW;
        }

        private void UpdateLabel()
        {
            _pageLabel.Text = _report == null ? "0/0" : string.Format("{0}/{1}  ({2}%)", _currentPage + 1, _report.Pages.Count, (int)(_zoom * 100));
        }

        // ============================== 渲染 ==============================

        private void RenderCurrentPage()
        {
            _canvas.Children.Clear();
            if (_report == null || _report.Pages.Count == 0) return;
            var page = _report.Pages[_currentPage];

            double ppm = PixelsPerMm * _zoom;
            double pageW = _report.PageWidth * ppm;
            double pageH = _report.PageHeight * ppm;

            // 页面白底
            var bg = new Rectangle { Width = pageW, Height = pageH, Fill = Brushes.White, Stroke = Brushes.Gray, StrokeThickness = 1 };
            Canvas.SetLeft(bg, 20);
            Canvas.SetTop(bg, 10);
            _canvas.Children.Add(bg);

            foreach (var el in page.Elements)
            {
                DrawElement(el, 20, 10, ppm);
            }

            _canvas.Width = pageW + 40;
            _canvas.Height = pageH + 20;
        }

        private void DrawElement(RenderedElement el, double ox, double oy, double ppm)
        {
            double x = ox + el.X * ppm;
            double y = oy + el.Y * ppm;
            double w = el.Width * ppm;
            double h = el.Height * ppm;

            FrameworkElement? visual = null;

            switch (el)
            {
                case RenderedTextElement t:
                    var tb = new TextBlock
                    {
                        Text = t.Text ?? "",
                        Width = w,
                        Height = h,
                        FontSize = Math.Max(6, t.Font.Size * ppm / PixelsPerMm * 0.75),
                        FontWeight = t.Font.Bold ? FontWeights.Bold : FontWeights.Normal,
                        FontStyle = t.Font.Italic ? FontStyles.Italic : FontStyles.Normal,
                        TextTrimming = TextTrimming.CharacterEllipsis,
                    };
                    if (!string.IsNullOrEmpty(t.Font.Color)) tb.Foreground = ParseBrush(t.Font.Color);
                    if (!string.IsNullOrEmpty(t.Font.Family))
                    {
                        try { tb.FontFamily = new FontFamily(t.Font.Family); } catch { }
                    }
                    visual = tb;
                    break;
                case RenderedLineElement l:
                    var lineCanvas = new Canvas { Width = w, Height = h };
                    var line = new Line { Stroke = ParseBrush(l.LineColor), StrokeThickness = l.LineWidth * ppm };
                    if (l.Direction == LineDirection.Vertical)
                    { line.X1 = w / 2; line.Y1 = 0; line.X2 = w / 2; line.Y2 = h; }
                    else if (l.Direction == LineDirection.Diagonal)
                    { line.X1 = 0; line.Y1 = 0; line.X2 = w; line.Y2 = h; }
                    else
                    { line.X1 = 0; line.Y1 = h / 2; line.X2 = w; line.Y2 = h / 2; }
                    lineCanvas.Children.Add(line);
                    visual = lineCanvas;
                    break;
                case RenderedShapeElement s:
                    if (s.Shape == ShapeType.Ellipse)
                    {
                        var ellipse = new Ellipse { Width = w, Height = h, Fill = ParseBrush(s.FillColor), Stroke = Brushes.DimGray, StrokeThickness = 0.5 };
                        visual = ellipse;
                    }
                    else
                    {
                        var rect = new Rectangle { Width = w, Height = h, Fill = ParseBrush(s.FillColor), Stroke = Brushes.DimGray, StrokeThickness = 0.5 };
                        visual = rect;
                    }
                    break;
            }

            if (visual != null)
            {
                Canvas.SetLeft(visual, x);
                Canvas.SetTop(visual, y);
                _canvas.Children.Add(visual);
            }
        }

        private static Brush ParseBrush(string? html)
        {
            if (string.IsNullOrEmpty(html)) return Brushes.Black;
            try
            {
                var s = html!.Trim();
                if (s.StartsWith("#")) s = s.Substring(1);
                if (s.Length == 6)
                {
                    var r = Convert.ToByte(s.Substring(0, 2), 16);
                    var g = Convert.ToByte(s.Substring(2, 2), 16);
                    var b = Convert.ToByte(s.Substring(4, 2), 16);
                    return new SolidColorBrush(Color.FromRgb(r, g, b));
                }
            }
            catch { }
            return Brushes.Black;
        }
    }
}
