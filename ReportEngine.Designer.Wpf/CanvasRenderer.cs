using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 设计态画布渲染器：把模板画成 WPF 视觉树并写入目标 Canvas。
/// 拆自 MainWindow.RenderCanvas / DrawBand / DrawElement / DrawHandle / DrawDashedLine / DrawRulers。
/// 调用方持有本类实例，每次 Render 前把当前模板状态作为 ctx 传入。
/// </summary>
internal sealed class CanvasRenderer
{
    private const double RulerSize = 22;

    private readonly Canvas _canvas;
    private readonly Canvas _hRuler;
    private readonly Canvas _vRuler;
    private readonly ScrollViewer _scroll;

    public CanvasRenderer(Canvas canvas, Canvas hRuler, Canvas vRuler, ScrollViewer scroll)
    {
        _canvas = canvas;
        _hRuler = hRuler;
        _vRuler = vRuler;
        _scroll = scroll;
    }

    /// <summary>渲染设计画布（含页面背景/网格/Bands/选中态）</summary>
    public void Render(CanvasRenderContext ctx, IEnumerable<ReportElement> selected, Band? selectedBand)
    {
        // IEnumerable 接口不便于 Contains，构建一次 HashSet 给 DrawBand/DrawElement 用
        var selectedSet = new HashSet<ReportElement>(selected);
        _canvas.Children.Clear();
        var template = ctx.Template;
        double z = ctx.Zoom;

        // 计算设计区域尺寸
        double designW, designH;
        var muCfg = template.Page.MultiUp;
        bool isMultiUp = muCfg != null && muCfg.Count > 1;
        if (isMultiUp)
        {
            int muRows = Math.Max(1, muCfg!.Rows);
            int muCols = Math.Max(1, muCfg.Columns);
            designW = (template.Page.Width - muCfg.HSpacing * (muCols - 1)) / muCols;
            designH = (template.Page.Height - muCfg.VSpacing * (muRows - 1)) / muRows;
            if (designW < 1) designW = 1;
            if (designH < 1) designH = 1;
        }
        else
        {
            designW = template.Page.Width;
            designH = template.Page.Height;
        }

        double pageW = designW * CanvasRenderContext.PixelsPerMm * z;
        double pageH = designH * CanvasRenderContext.PixelsPerMm * z;
        _canvas.Width = pageW + CanvasRenderContext.CanvasPadding * 2;
        _canvas.Height = pageH + CanvasRenderContext.CanvasPadding * 2;

        // 页面阴影
        var shadow = new Rectangle
        {
            Width = pageW, Height = pageH,
            Fill = new SolidColorBrush(Color.FromArgb(60, 0, 0, 0)),
        };
        Canvas.SetLeft(shadow, CanvasRenderContext.CanvasPadding + 4);
        Canvas.SetTop(shadow, CanvasRenderContext.CanvasPadding + 4);
        _canvas.Children.Add(shadow);

        // 页面背景
        Brush pageFill = !string.IsNullOrEmpty(template.Page.BackgroundColor)
            ? BrushParser.Parse(template.Page.BackgroundColor, Brushes.White)
            : Brushes.White;
        var pageBg = new Rectangle
        {
            Width = pageW, Height = pageH,
            Fill = pageFill, Stroke = Brushes.Gray, StrokeThickness = 1,
        };
        Canvas.SetLeft(pageBg, CanvasRenderContext.CanvasPadding);
        Canvas.SetTop(pageBg, CanvasRenderContext.CanvasPadding);
        _canvas.Children.Add(pageBg);

        // 网格线
        if (ctx.ShowGrid)
        {
            double gridStep = ctx.GridSpacingMm * CanvasRenderContext.PixelsPerMm * z;
            var gridPen = new SolidColorBrush(ctx.GridColor);
            for (double gx = gridStep; gx < pageW; gx += gridStep)
            {
                var gl = new Line
                {
                    X1 = CanvasRenderContext.CanvasPadding + gx,
                    Y1 = CanvasRenderContext.CanvasPadding,
                    X2 = CanvasRenderContext.CanvasPadding + gx,
                    Y2 = CanvasRenderContext.CanvasPadding + pageH,
                    Stroke = gridPen, StrokeThickness = 0.5,
                };
                _canvas.Children.Add(gl);
            }
            for (double gy = gridStep; gy < pageH; gy += gridStep)
            {
                var gl = new Line
                {
                    X1 = CanvasRenderContext.CanvasPadding,
                    Y1 = CanvasRenderContext.CanvasPadding + gy,
                    X2 = CanvasRenderContext.CanvasPadding + pageW,
                    Y2 = CanvasRenderContext.CanvasPadding + gy,
                    Stroke = gridPen, StrokeThickness = 0.5,
                };
                _canvas.Children.Add(gl);
            }
        }

        // 对齐参考线
        if (ctx.SnapLinesX.Count > 0 || ctx.SnapLinesY.Count > 0)
        {
            var guidePen = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            foreach (var x in ctx.SnapLinesX)
            {
                var line = new Line
                {
                    X1 = CanvasRenderContext.CanvasPadding + x * CanvasRenderContext.PixelsPerMm * z,
                    Y1 = CanvasRenderContext.CanvasPadding,
                    X2 = CanvasRenderContext.CanvasPadding + x * CanvasRenderContext.PixelsPerMm * z,
                    Y2 = CanvasRenderContext.CanvasPadding + pageH,
                    Stroke = guidePen, StrokeThickness = 1,
                };
                _canvas.Children.Add(line);
            }
            foreach (var y in ctx.SnapLinesY)
            {
                var line = new Line
                {
                    X1 = CanvasRenderContext.CanvasPadding,
                    Y1 = CanvasRenderContext.CanvasPadding + y * CanvasRenderContext.PixelsPerMm * z,
                    X2 = CanvasRenderContext.CanvasPadding + pageW,
                    Y2 = CanvasRenderContext.CanvasPadding + y * CanvasRenderContext.PixelsPerMm * z,
                    Stroke = guidePen, StrokeThickness = 1,
                };
                _canvas.Children.Add(line);
            }
        }

        // 垂直参考线（持久）
        if (ctx.VerticalGuides.Count > 0)
        {
            var guidePen = new SolidColorBrush(Color.FromRgb(0, 120, 200));
            foreach (var x in ctx.VerticalGuides)
            {
                var line = new Line
                {
                    X1 = CanvasRenderContext.CanvasPadding + x * CanvasRenderContext.PixelsPerMm * z,
                    Y1 = CanvasRenderContext.CanvasPadding,
                    X2 = CanvasRenderContext.CanvasPadding + x * CanvasRenderContext.PixelsPerMm * z,
                    Y2 = CanvasRenderContext.CanvasPadding + pageH,
                    Stroke = guidePen, StrokeThickness = 0.5,
                    StrokeDashArray = new DoubleCollection(new[] { 4.0, 4.0 }),
                };
                _canvas.Children.Add(line);
            }
        }
        // 水平参考线
        if (ctx.HorizontalGuides.Count > 0)
        {
            var guidePen = new SolidColorBrush(Color.FromRgb(0, 120, 200));
            foreach (var y in ctx.HorizontalGuides)
            {
                var line = new Line
                {
                    X1 = CanvasRenderContext.CanvasPadding,
                    Y1 = CanvasRenderContext.CanvasPadding + y * CanvasRenderContext.PixelsPerMm * z,
                    X2 = CanvasRenderContext.CanvasPadding + pageW,
                    Y2 = CanvasRenderContext.CanvasPadding + y * CanvasRenderContext.PixelsPerMm * z,
                    Stroke = guidePen, StrokeThickness = 0.5,
                    StrokeDashArray = new DoubleCollection(new[] { 4.0, 4.0 }),
                };
                _canvas.Children.Add(line);
            }
        }

        if (isMultiUp)
        {
            var muHint = new TextBlock
            {
                Text = "多联打印: " + muCfg!.Rows + "×" + muCfg.Columns + "=" + muCfg.Count + "份/页  单联: "
                    + Math.Round(designW, 1) + "×" + Math.Round(designH, 1) + "mm",
                FontSize = 10 * z,
                Foreground = Brushes.OrangeRed,
                FontWeight = FontWeights.Bold,
            };
            Canvas.SetLeft(muHint, CanvasRenderContext.CanvasPadding + 4);
            Canvas.SetTop(muHint, CanvasRenderContext.CanvasPadding - 14 * z);
            _canvas.Children.Add(muHint);
        }
        else if (ctx.ShowMargins)
        {
            var ml = template.Page.Margin.Left * CanvasRenderContext.PixelsPerMm * z;
            var mr = template.Page.Margin.Right * CanvasRenderContext.PixelsPerMm * z;
            var mt = template.Page.Margin.Top * CanvasRenderContext.PixelsPerMm * z;
            var mb = template.Page.Margin.Bottom * CanvasRenderContext.PixelsPerMm * z;
            DrawDashedLine(CanvasRenderContext.CanvasPadding + ml, CanvasRenderContext.CanvasPadding,
                CanvasRenderContext.CanvasPadding + ml, CanvasRenderContext.CanvasPadding + pageH, Colors.LightBlue);
            DrawDashedLine(CanvasRenderContext.CanvasPadding + pageW - mr, CanvasRenderContext.CanvasPadding,
                CanvasRenderContext.CanvasPadding + pageW - mr, CanvasRenderContext.CanvasPadding + pageH, Colors.LightBlue);
            DrawDashedLine(CanvasRenderContext.CanvasPadding, CanvasRenderContext.CanvasPadding + mt,
                CanvasRenderContext.CanvasPadding + pageW, CanvasRenderContext.CanvasPadding + mt, Colors.LightBlue);
            DrawDashedLine(CanvasRenderContext.CanvasPadding, CanvasRenderContext.CanvasPadding + pageH - mb,
                CanvasRenderContext.CanvasPadding + pageW, CanvasRenderContext.CanvasPadding + pageH - mb, Colors.LightBlue);
        }

        // 渲染 Bands
        double currentY = CanvasRenderContext.CanvasPadding;
        foreach (var band in template.Bands)
        {
            double bandH = band.Height * CanvasRenderContext.PixelsPerMm * z;
            var bandRect = new Rect(CanvasRenderContext.CanvasPadding, currentY, pageW, bandH);
            DrawBand(band, bandRect, z, selectedBand, selectedSet);

            // Band 底部调整手柄
            var handle = new Rectangle
            {
                Width = pageW, Height = 5,
                Fill = Brushes.Transparent, Cursor = Cursors.SizeNS,
                Tag = band,
            };
            Canvas.SetLeft(handle, CanvasRenderContext.CanvasPadding);
            Canvas.SetTop(handle, currentY + bandH - 2);
            _canvas.Children.Add(handle);

            currentY += bandH;
        }

        _canvas.Height = Math.Max(_canvas.Height, currentY + CanvasRenderContext.CanvasPadding);
    }

    /// <summary>渲染标尺（独立调用，ScrollChanged 时调用）</summary>
    public void RenderRulers(ReportTemplate template, double zoom)
    {
        _hRuler.Children.Clear();
        _vRuler.Children.Clear();
        if (template == null) return;

        double z = zoom;
        double offsetX = -_scroll.HorizontalOffset + CanvasRenderContext.CanvasPadding;
        double offsetY = -_scroll.VerticalOffset + CanvasRenderContext.CanvasPadding;
        double mmPx = CanvasRenderContext.PixelsPerMm * z;
        double pageW = template.Page.Width;
        double pageH = template.Page.Height;

        var fg = Brushes.DimGray;
        var pen = new SolidColorBrush(Color.FromRgb(160, 160, 160));

        // 水平标尺
        for (int mm = 0; mm <= (int)pageW; mm += 5)
        {
            double px = offsetX + mm * mmPx;
            if (px < 0 || px > _hRuler.ActualWidth) continue;
            double tickH = mm % 10 == 0 ? RulerSize * 0.6 : RulerSize * 0.3;
            _hRuler.Children.Add(new Line
            {
                X1 = px, Y1 = RulerSize, X2 = px, Y2 = RulerSize - tickH,
                Stroke = pen, StrokeThickness = 0.5,
            });
            if (mm % 10 == 0)
            {
                var txt = new TextBlock { Text = mm.ToString(), FontSize = 8, Foreground = fg };
                Canvas.SetLeft(txt, px + 2);
                Canvas.SetTop(txt, 1);
                _hRuler.Children.Add(txt);
            }
        }

        // 垂直标尺
        for (int mm = 0; mm <= (int)pageH; mm += 5)
        {
            double py = offsetY + mm * mmPx;
            if (py < 0 || py > _vRuler.ActualHeight) continue;
            double tickW = mm % 10 == 0 ? RulerSize * 0.6 : RulerSize * 0.3;
            _vRuler.Children.Add(new Line
            {
                X1 = RulerSize, Y1 = py, X2 = RulerSize - tickW, Y2 = py,
                Stroke = pen, StrokeThickness = 0.5,
            });
            if (mm % 10 == 0)
            {
                var txt = new TextBlock
                {
                    Text = mm.ToString(),
                    FontSize = 8, Foreground = fg,
                    RenderTransform = new RotateTransform(-90),
                };
                Canvas.SetLeft(txt, 1);
                Canvas.SetTop(txt, py + 12);
                _vRuler.Children.Add(txt);
            }
        }
    }

    // ========== 私有辅助：Band / Element / Handle / DashedLine ==========

    private void DrawBand(Band band, Rect rect, double z, Band? selectedBand, HashSet<ReportElement> selected)
    {
        bool isBandSelected = band == selectedBand;
        var bg = new Rectangle
        {
            Width = rect.Width, Height = rect.Height,
            Fill = BandStyle.GetBrush(band.Type),
            Stroke = isBandSelected ? Brushes.DodgerBlue : new SolidColorBrush(Color.FromRgb(200, 200, 200)),
            StrokeThickness = isBandSelected ? 2 : 0.5,
        };
        Canvas.SetLeft(bg, rect.X);
        Canvas.SetTop(bg, rect.Y);
        _canvas.Children.Add(bg);

        var label = new TextBlock
        {
            Text = BandStyle.Name(band.Type) + " (" + band.Height + "mm)",
            FontSize = 9 * z,
            Foreground = new SolidColorBrush(Color.FromRgb(120, 120, 120)),
        };
        Canvas.SetLeft(label, rect.X + 3);
        Canvas.SetTop(label, rect.Y + 1);
        _canvas.Children.Add(label);

        foreach (var el in band.Elements)
            DrawElement(el, rect, z, selected);
    }

    private void DrawElement(ReportElement el, Rect bandRect, double z, HashSet<ReportElement> selected)
    {
        double x = bandRect.X + el.X * CanvasRenderContext.PixelsPerMm * z;
        double y = bandRect.Y + el.Y * CanvasRenderContext.PixelsPerMm * z;
        double w = Math.Max(4, el.Width * CanvasRenderContext.PixelsPerMm * z);
        double h = Math.Max(4, el.Height * CanvasRenderContext.PixelsPerMm * z);

        FrameworkElement? visual = null;
        switch (el)
        {
            case TextElement t:
                string displayText;
                Brush textFg = Brushes.Black;
                switch (t.BoxType)
                {
                    case TextBoxType.Field:
                        displayText = "{" + (t.DataField ?? "") + "}";
                        textFg = Brushes.DarkBlue;
                        break;
                    case TextBoxType.Summary:
                        displayText = "Σ" + (t.SummaryFunction ?? "") + "(" + (t.SummaryField ?? "") + ")";
                        textFg = Brushes.DarkRed;
                        break;
                    case TextBoxType.SysVar:
                        displayText = "@" + (t.SystemVariable ?? "");
                        textFg = Brushes.DarkGreen;
                        break;
                    default:
                        displayText = t.Text ?? "";
                        break;
                }
                var tb = new TextBlock
                {
                    Text = displayText,
                    Width = w, Height = h,
                    FontSize = Math.Max(6, t.Font.Size * z),
                    FontWeight = t.Font.Bold ? FontWeights.Bold : FontWeights.Normal,
                    FontStyle = t.Font.Italic ? FontStyles.Italic : FontStyles.Normal,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    Padding = new Thickness(1),
                    Foreground = textFg,
                };
                if (t.BoxType == TextBoxType.Static && !string.IsNullOrEmpty(t.Font.Color))
                    tb.Foreground = BrushParser.Parse(t.Font.Color, Brushes.Black);
                visual = tb;
                break;
            case LineElement l:
                var lineCanvas = new Canvas { Width = w, Height = h };
                var line = new Line
                {
                    StrokeThickness = l.LineWidth * z,
                    Stroke = BrushParser.Parse(l.LineColor, Brushes.Black),
                };
                if (l.Direction == LineDirection.Vertical)
                {
                    line.X1 = w / 2; line.Y1 = 0;
                    line.X2 = w / 2; line.Y2 = h;
                }
                else if (l.Direction == LineDirection.Diagonal)
                {
                    line.X1 = 0; line.Y1 = 0;
                    line.X2 = w; line.Y2 = h;
                }
                else
                {
                    line.X1 = 0; line.Y1 = h / 2;
                    line.X2 = w; line.Y2 = h / 2;
                }
                lineCanvas.Children.Add(line);
                visual = lineCanvas;
                break;
            case ShapeElement s:
                var shape = new Rectangle
                {
                    Width = w, Height = h,
                    Fill = BrushParser.Parse(s.FillColor, Brushes.White),
                    Stroke = Brushes.DimGray, StrokeThickness = 1,
                };
                if (s.Shape == ShapeType.Ellipse)
                {
                    shape.RadiusX = w / 2; shape.RadiusY = h / 2;
                }
                else if (s.BorderRadius > 0)
                {
                    shape.RadiusX = s.BorderRadius * z;
                    shape.RadiusY = s.BorderRadius * z;
                }
                visual = shape;
                break;
            case SubReportElement sr:
                visual = UiFactory.MakeElementBorder(w, h, Brushes.MediumPurple, Color.FromArgb(20, 147, 112, 219),
                    "[SubReport] " + sr.TemplateRef, 9 * z);
                break;
            case BarcodeElement bc:
                visual = UiFactory.MakeElementBorder(w, h, Brushes.DarkOrange, Color.FromRgb(255, 250, 240),
                    "[" + bc.Format + "] " + bc.Value, 8 * z);
                break;
            case TableElement tbl:
                var tblCanvas = new Canvas { Width = w, Height = h, Background = Brushes.White };
                double tcw = w / Math.Max(tbl.ColCount, 1);
                double trh = h / Math.Max(tbl.RowCount, 1);
                for (int ci = 0; ci <= tbl.ColCount; ci++)
                    tblCanvas.Children.Add(new Line { X1 = ci * tcw, Y1 = 0, X2 = ci * tcw, Y2 = h, Stroke = Brushes.LightGray, StrokeThickness = 0.5 });
                for (int ri = 0; ri <= tbl.RowCount; ri++)
                    tblCanvas.Children.Add(new Line { X1 = 0, Y1 = ri * trh, X2 = w, Y2 = ri * trh, Stroke = Brushes.LightGray, StrokeThickness = 0.5 });
                var tl = new TextBlock { Text = "[Table " + tbl.RowCount + "x" + tbl.ColCount + "]", FontSize = 8 * z, Foreground = Brushes.DarkSlateGray };
                Canvas.SetLeft(tl, 2); Canvas.SetTop(tl, 2);
                tblCanvas.Children.Add(tl);
                visual = new Border
                {
                    Width = w, Height = h,
                    BorderBrush = Brushes.DarkSlateGray, BorderThickness = new Thickness(1),
                    Child = tblCanvas,
                };
                break;
            case CrossTabElement ct:
                visual = UiFactory.MakeElementBorder(w, h, Brushes.DarkCyan, Color.FromRgb(240, 248, 255),
                    "[CrossTab] " + ct.DataSource, 8 * z);
                break;
            case ChartElement ch:
                visual = UiFactory.MakeElementBorder(w, h, Brushes.MediumVioletRed, Color.FromRgb(255, 245, 250),
                    "[Chart] " + ch.Title + " (" + ChartTypeCN(ch.ChartType) + ")", 9 * z);
                break;
            case ImageElement img:
                visual = UiFactory.MakeElementBorder(w, h, Brushes.SteelBlue, Color.FromArgb(0, 0, 0, 0), "[Image]", 8 * z);
                break;
        }

        if (visual == null) return;

        if (el.Opacity < 1.0) visual.Opacity = el.Opacity;

        if (el.Rotation != 0)
        {
            var container = new Border { Width = w, Height = h, Child = visual };
            var transform = new RotateTransform(el.Rotation, w / 2, h / 2);
            container.RenderTransform = transform;
            visual = container;
        }

        // 元素边框
        if (el.Border != null && el.Border.Width > 0)
        {
            var borderBrush = BrushParser.Parse(el.Border.Color, Brushes.Black);
            double bw = el.Border.Width * z;
            DoubleCollection? dash = el.Border.Style == BorderStyle.Dashed ? new DoubleCollection { 4, 2 }
                : el.Border.Style == BorderStyle.Dotted ? new DoubleCollection { 1, 2 } : null;
            if (el.Border.Top)
            {
                var ln = new Line { X1 = x, Y1 = y, X2 = x + w, Y2 = y, Stroke = borderBrush, StrokeThickness = bw };
                if (dash != null) ln.StrokeDashArray = dash;
                _canvas.Children.Add(ln);
            }
            if (el.Border.Bottom)
            {
                var ln = new Line { X1 = x, Y1 = y + h, X2 = x + w, Y2 = y + h, Stroke = borderBrush, StrokeThickness = bw };
                if (dash != null) ln.StrokeDashArray = dash;
                _canvas.Children.Add(ln);
            }
            if (el.Border.Left)
            {
                var ln = new Line { X1 = x, Y1 = y, X2 = x, Y2 = y + h, Stroke = borderBrush, StrokeThickness = bw };
                if (dash != null) ln.StrokeDashArray = dash;
                _canvas.Children.Add(ln);
            }
            if (el.Border.Right)
            {
                var ln = new Line { X1 = x + w, Y1 = y, X2 = x + w, Y2 = y + h, Stroke = borderBrush, StrokeThickness = bw };
                if (dash != null) ln.StrokeDashArray = dash;
                _canvas.Children.Add(ln);
            }
        }

        // 选中态边框 + 8 向 resize 手柄（由 DrawElement 内部决定；选中集合传入父 Render）
        bool isElSelected = selected.Contains(el);
        if (isElSelected)
        {
            var selBorder = new Border
            {
                Width = w + 4, Height = h + 4,
                BorderBrush = el.Locked ? Brushes.Gray : Brushes.OrangeRed,
                BorderThickness = new Thickness(1.5),
                Child = visual,
            };
            Canvas.SetLeft(selBorder, x - 2);
            Canvas.SetTop(selBorder, y - 2);
            selBorder.Tag = el;
            _canvas.Children.Add(selBorder);

            if (el.Locked)
            {
                var lockLabel = new TextBlock { Text = "🔒", FontSize = 10 * z, Foreground = Brushes.DimGray };
                Canvas.SetLeft(lockLabel, x + w - 12 * z);
                Canvas.SetTop(lockLabel, y - 14 * z);
                _canvas.Children.Add(lockLabel);
            }
            else
            {
                double hs = 6;
                DrawHandle(x - hs / 2, y - hs / 2, hs, 0);
                DrawHandle(x + w / 2 - hs / 2, y - hs / 2, hs, 1);
                DrawHandle(x + w - hs / 2, y - hs / 2, hs, 2);
                DrawHandle(x + w - hs / 2, y + h / 2 - hs / 2, hs, 3);
                DrawHandle(x + w - hs / 2, y + h - hs / 2, hs, 4);
                DrawHandle(x + w / 2 - hs / 2, y + h - hs / 2, hs, 5);
                DrawHandle(x - hs / 2, y + h - hs / 2, hs, 6);
                DrawHandle(x - hs / 2, y + h / 2 - hs / 2, hs, 7);
            }
        }
        else
        {
            var container = new Border { Width = w, Height = h, Child = visual };
            Canvas.SetLeft(container, x);
            Canvas.SetTop(container, y);
            container.Tag = el;
            _canvas.Children.Add(container);
        }
    }

    // 选中态由 Render 通过 selected 集合推算

    private void DrawHandle(double x, double y, double size, int index)
    {
        var rect = new Rectangle
        {
            Width = size, Height = size,
            Fill = Brushes.White, Stroke = Brushes.OrangeRed, StrokeThickness = 1,
            Tag = "handle_" + index,
            Cursor = index == 0 || index == 4 ? Cursors.SizeNWSE
                : index == 2 || index == 6 ? Cursors.SizeNESW
                : index == 1 || index == 5 ? Cursors.SizeNS
                : Cursors.SizeWE,
        };
        Canvas.SetLeft(rect, x);
        Canvas.SetTop(rect, y);
        _canvas.Children.Add(rect);
    }

    private void DrawDashedLine(double x1, double y1, double x2, double y2, Color color)
    {
        var line = new Line
        {
            X1 = x1, Y1 = y1, X2 = x2, Y2 = y2,
            Stroke = new SolidColorBrush(color),
            StrokeThickness = 0.5,
            StrokeDashArray = new DoubleCollection(new[] { 4.0, 4.0 }),
        };
        _canvas.Children.Add(line);
    }

    /// <summary>图表类型中文名（MainWindow 私有，搬过来共享）</summary>
    private static string ChartTypeCN(ChartType t) => t switch
    {
        ChartType.Bar => "柱状图",
        ChartType.Line => "折线图",
        ChartType.Pie => "饼图",
        ChartType.Area => "面积图",
        ChartType.Scatter => "散点图",
        _ => t.ToString(),
    };
}