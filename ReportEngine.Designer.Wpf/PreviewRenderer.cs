using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 预览渲染器：把模板画成"接近真实输出"的预览图，写入预览 Canvas。
/// 拆自 MainWindow.RenderPreview / DrawPreviewBand / DrawPreviewElement / ResolvePreviewValue。
/// 预览数据为空时显示静态占位（字段显示 "[" + name + "]"，SysVar 显示示例值）。
/// </summary>
internal sealed class PreviewRenderer
{
    private readonly Canvas _previewCanvas;

    public PreviewRenderer(Canvas previewCanvas)
    {
        _previewCanvas = previewCanvas;
    }

    /// <summary>渲染预览画布</summary>
    public void Render(ReportTemplate template, double zoom, IReadOnlyDictionary<string, object>? previewData)
    {
        _previewCanvas.Children.Clear();
        if (template == null) return;

        double z = zoom;
        double physW = template.Page.Width;
        double physH = template.Page.Height;
        double pageW = physW * CanvasRenderContext.PixelsPerMm * z;
        double pageH = physH * CanvasRenderContext.PixelsPerMm * z;
        double pad = 30;

        _previewCanvas.Width = pageW + pad * 2;
        _previewCanvas.Height = pageH + pad * 2;

        // 页面阴影
        var shadow = new Rectangle { Width = pageW, Height = pageH, Fill = new SolidColorBrush(Color.FromArgb(60, 0, 0, 0)) };
        Canvas.SetLeft(shadow, pad + 4); Canvas.SetTop(shadow, pad + 4);
        _previewCanvas.Children.Add(shadow);

        // 页面白底
        var pageBg = new Rectangle { Width = pageW, Height = pageH, Fill = Brushes.White, Stroke = Brushes.Gray, StrokeThickness = 1 };
        Canvas.SetLeft(pageBg, pad); Canvas.SetTop(pageBg, pad);
        _previewCanvas.Children.Add(pageBg);

        var muCfg = template.Page.MultiUp;
        bool isMultiUp = muCfg != null && muCfg.Count > 1;

        if (isMultiUp)
        {
            // 多联模式：平铺渲染
            int rows = Math.Max(1, muCfg!.Rows);
            int cols = Math.Max(1, muCfg.Columns);
            double cellW = (physW - muCfg.HSpacing * (cols - 1)) / cols;
            double cellH = (physH - muCfg.VSpacing * (rows - 1)) / rows;
            if (cellW < 1) cellW = 1;
            if (cellH < 1) cellH = 1;

            bool horizontal = muCfg.Direction != "Vertical";
            int idx = 0;
            for (int r = 0; r < rows && idx < muCfg.Count; r++)
            {
                for (int c = 0; c < cols && idx < muCfg.Count; c++, idx++)
                {
                    int row = horizontal ? r : c;
                    int col = horizontal ? c : r;
                    double ox = pad + (cellW + muCfg.HSpacing) * col * CanvasRenderContext.PixelsPerMm * z;
                    double oy = pad + (cellH + muCfg.VSpacing) * row * CanvasRenderContext.PixelsPerMm * z;

                    // 单联边框
                    var cellBorder = new Rectangle
                    {
                        Width = cellW * CanvasRenderContext.PixelsPerMm * z,
                        Height = cellH * CanvasRenderContext.PixelsPerMm * z,
                        Stroke = new SolidColorBrush(Color.FromArgb(80, 0, 120, 200)),
                        StrokeThickness = 0.5,
                        StrokeDashArray = new DoubleCollection(new[] { 3.0, 2.0 }),
                        Fill = Brushes.Transparent,
                    };
                    Canvas.SetLeft(cellBorder, ox); Canvas.SetTop(cellBorder, oy);
                    _previewCanvas.Children.Add(cellBorder);

                    // 序号标签
                    var numLabel = new TextBlock
                    {
                        Text = "#" + (idx + 1),
                        FontSize = 8 * z,
                        Foreground = new SolidColorBrush(Color.FromArgb(120, 0, 120, 200)),
                    };
                    Canvas.SetLeft(numLabel, ox + 2);
                    Canvas.SetTop(numLabel, oy + 1);
                    _previewCanvas.Children.Add(numLabel);

                    // 渲染每联内的 Bands
                    double bandY = oy;
                    foreach (var band in template.Bands)
                    {
                        double bh = band.Height * CanvasRenderContext.PixelsPerMm * z;
                        var bandRect = new Rect(ox, bandY, cellW * CanvasRenderContext.PixelsPerMm * z, bh);
                        DrawPreviewBand(band, bandRect, z, previewData);
                        bandY += bh;
                    }
                }
            }
        }
        else
        {
            // 单联：直接堆叠 Bands
            double bandY = pad;
            foreach (var band in template.Bands)
            {
                double bh = band.Height * CanvasRenderContext.PixelsPerMm * z;
                var bandRect = new Rect(pad, bandY, pageW, bh);
                DrawPreviewBand(band, bandRect, z, previewData);
                bandY += bh;
            }
        }

        // 预览底部状态：页码标识
        var pageNum = new TextBlock
        {
            Text = "预览（数据源为空时显示静态占位）",
            FontSize = 9,
            Foreground = Brushes.Gray,
        };
        Canvas.SetLeft(pageNum, pad);
        Canvas.SetTop(pageNum, pageH + pad + 8);
        _previewCanvas.Children.Add(pageNum);
    }

    private void DrawPreviewBand(Band band, Rect rect, double z, IReadOnlyDictionary<string, object>? previewData)
    {
        foreach (var el in band.Elements)
            DrawPreviewElement(el, rect, z, previewData);
    }

    private void DrawPreviewElement(ReportElement el, Rect bandRect, double z, IReadOnlyDictionary<string, object>? previewData)
    {
        double px = bandRect.X + el.X * CanvasRenderContext.PixelsPerMm * z;
        double py = bandRect.Y + el.Y * CanvasRenderContext.PixelsPerMm * z;
        double pw = Math.Max(1, el.Width * CanvasRenderContext.PixelsPerMm * z);
        double ph = Math.Max(1, el.Height * CanvasRenderContext.PixelsPerMm * z);

        if (el is TextElement txt)
        {
            string prevText;
            switch (txt.BoxType)
            {
                case TextBoxType.Field:
                    prevText = ResolvePreviewValue(txt.DataField, previewData);
                    break;
                case TextBoxType.Summary:
                    var sumVal = ResolvePreviewValue(txt.SummaryField, previewData);
                    prevText = previewData != null && previewData.Count > 0
                        ? sumVal
                        : "Σ" + (txt.SummaryFunction ?? "") + "(" + (txt.SummaryField ?? "") + ")";
                    break;
                case TextBoxType.SysVar:
                    prevText = txt.SystemVariable switch
                    {
                        "PageNumber" => "1",
                        "TotalPages" => "1",
                        "PrintDate" => DateTime.Now.ToString("yyyy-MM-dd"),
                        "PrintTime" => DateTime.Now.ToString("HH:mm:ss"),
                        "ReportTitle" => "报表",
                        _ => "@" + (txt.SystemVariable ?? ""),
                    };
                    break;
                default:
                    prevText = txt.Text ?? "";
                    break;
            }
            var tb = new TextBlock
            {
                Text = prevText,
                FontSize = Math.Max(6, (txt.Font?.Size ?? 9) * z),
                Foreground = BrushParser.Parse(txt.Font?.Color, Brushes.Black),
                Width = pw, Height = ph,
                TextTrimming = TextTrimming.CharacterEllipsis,
            };
            if (el.BackgroundColor != null)
            {
                var bg = new Border
                {
                    Width = pw, Height = ph,
                    Background = BrushParser.Parse(el.BackgroundColor, Brushes.Transparent),
                    Child = tb,
                };
                Canvas.SetLeft(bg, px); Canvas.SetTop(bg, py);
                _previewCanvas.Children.Add(bg);
            }
            else
            {
                Canvas.SetLeft(tb, px); Canvas.SetTop(tb, py);
                _previewCanvas.Children.Add(tb);
            }
        }
        else if (el is LineElement line)
        {
            var l = new Line
            {
                X1 = px, Y1 = py, X2 = px + pw, Y2 = py + ph,
                Stroke = BrushParser.Parse(line.LineColor, Brushes.Black),
                StrokeThickness = Math.Max(0.5, line.LineWidth * z),
            };
            _previewCanvas.Children.Add(l);
        }
        else if (el is ShapeElement shape)
        {
            var rect2 = new Rectangle
            {
                Width = pw, Height = ph,
                Fill = BrushParser.Parse(shape.FillColor, Brushes.Transparent),
                Stroke = Brushes.Black, StrokeThickness = 0.5 * z,
                RadiusX = shape.BorderRadius * z, RadiusY = shape.BorderRadius * z,
            };
            Canvas.SetLeft(rect2, px); Canvas.SetTop(rect2, py);
            _previewCanvas.Children.Add(rect2);
        }
        else if (el is ImageElement)
        {
            var placeholder = new Rectangle
            {
                Width = pw, Height = ph,
                Fill = new SolidColorBrush(Color.FromArgb(30, 100, 100, 100)),
                Stroke = Brushes.Gray, StrokeThickness = 0.5,
            };
            Canvas.SetLeft(placeholder, px); Canvas.SetTop(placeholder, py);
            _previewCanvas.Children.Add(placeholder);
            var imgLabel = new TextBlock
            {
                Text = "🖼", FontSize = Math.Min(pw, ph) * 0.5,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            Canvas.SetLeft(imgLabel, px + pw / 2 - 6);
            Canvas.SetTop(imgLabel, py + ph / 2 - 8);
            _previewCanvas.Children.Add(imgLabel);
        }
        else if (el is BarcodeElement)
        {
            var bcRect = new Rectangle { Width = pw, Height = ph, Fill = Brushes.White, Stroke = Brushes.Black, StrokeThickness = 0.5 };
            Canvas.SetLeft(bcRect, px); Canvas.SetTop(bcRect, py);
            _previewCanvas.Children.Add(bcRect);
            for (double bx = 2; bx < pw - 2; bx += 3)
            {
                var bar = new Rectangle { Width = 1.5, Height = ph * 0.7, Fill = Brushes.Black };
                Canvas.SetLeft(bar, px + bx); Canvas.SetTop(bar, py + ph * 0.1);
                _previewCanvas.Children.Add(bar);
            }
        }
        else
        {
            // 通用占位（Table / CrossTab / Chart / SubReport）
            var generic = new Rectangle
            {
                Width = pw, Height = ph,
                Fill = new SolidColorBrush(Color.FromArgb(20, 0, 0, 0)),
                Stroke = Brushes.Gray, StrokeThickness = 0.5,
            };
            Canvas.SetLeft(generic, px); Canvas.SetTop(generic, py);
            _previewCanvas.Children.Add(generic);
        }
    }

    /// <summary>从预览数据查字段值；空数据时返回 "[" + 字段名 + "]"。</summary>
    public static string ResolvePreviewValue(string? fieldName, IReadOnlyDictionary<string, object>? previewData)
    {
        if (previewData == null || string.IsNullOrEmpty(fieldName)) return "[" + (fieldName ?? "") + "]";
        var key = fieldName!;
        return previewData.TryGetValue(key, out var val) ? val?.ToString() ?? "" : "[" + key + "]";
    }
}