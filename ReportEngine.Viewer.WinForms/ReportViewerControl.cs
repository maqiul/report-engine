using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Windows.Forms;
using ReportEngine.Core;
using ReportEngine.Core.Rendering;

namespace ReportEngine.Viewer.WinForms
{
    /// <summary>
    /// 报表运行时预览控件。
    /// 功能：分页显示渲染结果、缩放、翻页、打印。
    /// 嵌入到用户的 WinForms 应用中使用。
    /// </summary>
    public class ReportViewerControl : UserControl
    {
        private RenderedReport? _report;
        private int _currentPage;
        private float _zoom = 1.0f;
        private readonly ToolStrip _toolbar;
        private readonly Panel _pagePanel;
        private readonly Label _pageLabel;

        /// <summary>缩放比例 (0.25~4.0)</summary>
        public float Zoom
        {
            get => _zoom;
            set { _zoom = Math.Max(0.25f, Math.Min(4f, value)); _pagePanel.Invalidate(); UpdatePageLabel(); }
        }

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_report == null) return;
                _currentPage = Math.Max(0, Math.Min(value, _report.Pages.Count - 1));
                _pagePanel.Invalidate();
                UpdatePageLabel();
            }
        }

        public int TotalPages => _report?.Pages.Count ?? 0;

        public ReportViewerControl()
        {
            _pagePanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(80, 80, 80), AutoScroll = true };
            _pagePanel.Paint += OnPagePaint;

            _toolbar = new ToolStrip();
            var btnFirst = new ToolStripButton("⏮") { ToolTipText = "首页" };
            btnFirst.Click += (_, __) => CurrentPage = 0;
            var btnPrev = new ToolStripButton("◀") { ToolTipText = "上一页" };
            btnPrev.Click += (_, __) => CurrentPage--;
            _pageLabel = new Label { Text = "0/0", AutoSize = true };
            var lblHost = new ToolStripControlHost(_pageLabel);
            var btnNext = new ToolStripButton("▶") { ToolTipText = "下一页" };
            btnNext.Click += (_, __) => CurrentPage++;
            var btnLast = new ToolStripButton("⏭") { ToolTipText = "末页" };
            btnLast.Click += (_, __) => { if (_report != null) CurrentPage = _report.Pages.Count - 1; };
            _toolbar.Items.Add(btnFirst);
            _toolbar.Items.Add(btnPrev);
            _toolbar.Items.Add(lblHost);
            _toolbar.Items.Add(btnNext);
            _toolbar.Items.Add(btnLast);
            _toolbar.Items.Add(new ToolStripSeparator());

            var btnZoomIn = new ToolStripButton("🔍+") { ToolTipText = "放大" };
            btnZoomIn.Click += (_, __) => Zoom += 0.25f;
            var btnZoomOut = new ToolStripButton("🔍-") { ToolTipText = "缩小" };
            btnZoomOut.Click += (_, __) => Zoom -= 0.25f;
            var btnFit = new ToolStripButton("适合") { ToolTipText = "适合宽度" };
            btnFit.Click += (_, __) => FitWidth();
            _toolbar.Items.Add(btnZoomOut);
            _toolbar.Items.Add(btnZoomIn);
            _toolbar.Items.Add(btnFit);
            _toolbar.Items.Add(new ToolStripSeparator());

            var btnPrint = new ToolStripButton("🖨 打印") { ToolTipText = "打印" };
            btnPrint.Click += (_, __) => PrintReport();
            _toolbar.Items.Add(btnPrint);

            Controls.Add(_pagePanel);
            Controls.Add(_toolbar);
        }

        /// <summary>
        /// 设置要预览的已渲染报表。
        /// </summary>
        public void SetReport(RenderedReport report)
        {
            _report = report;
            _currentPage = 0;
            FitWidth();
            _pagePanel.Invalidate();
            UpdatePageLabel();
        }

        public void FitWidth()
        {
            if (_report == null) return;
            float availW = _pagePanel.ClientSize.Width - 40;
            float pageW = (float)(_report.PageWidth * 96.0 / 25.4);
            if (pageW > 0) Zoom = availW / pageW;
        }

        private void UpdatePageLabel()
        {
            _pageLabel.Text = _report == null ? "0/0" : $"{_currentPage + 1}/{_report.Pages.Count}  ({(int)(_zoom * 100)}%)";
        }

        // ============================== 绘制 ==============================

        private void OnPagePaint(object? sender, PaintEventArgs e)
        {
            if (_report == null || _report.Pages.Count == 0) return;
            var page = _report.Pages[_currentPage];
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float ppm = (float)(96.0 / 25.4) * _zoom;
            float pageW = (float)(_report.PageWidth * ppm / (96.0 / 25.4)) * (96f / 96f);
            // 简化：
            pageW = (float)(_report.PageWidth * ppm);
            float pageH = (float)(_report.PageHeight * ppm);

            float ox = Math.Max(10, (_pagePanel.ClientSize.Width - pageW) / 2);
            float oy = 10;

            // 页面白底 + 阴影
            g.FillRectangle(Brushes.DarkGray, ox + 3, oy + 3, pageW, pageH);
            g.FillRectangle(Brushes.White, ox, oy, pageW, pageH);
            g.DrawRectangle(Pens.Gray, ox, oy, pageW, pageH);

            // 元素
            foreach (var el in page.Elements)
            {
                DrawElement(g, el, ox, oy, ppm);
            }

            _pagePanel.AutoScrollMinSize = new Size((int)(pageW + 40), (int)(pageH + 40));
        }

        private static void DrawElement(Graphics g, RenderedElement el, float ox, float oy, float ppm)
        {
            float x = ox + (float)(el.X * ppm);
            float y = oy + (float)(el.Y * ppm);
            float w = (float)(el.Width * ppm);
            float h = (float)(el.Height * ppm);
            var rect = new RectangleF(x, y, w, h);

            // 背景
            if (!string.IsNullOrEmpty(el.BackgroundColor))
            {
                try
                {
                    using (var br = new SolidBrush(ColorTranslator.FromHtml(el.BackgroundColor)))
                        g.FillRectangle(br, rect);
                }
                catch { }
            }

            switch (el)
            {
                case RenderedTextElement t:
                    var fs = FontStyle.Regular;
                    if (t.Font.Bold) fs |= FontStyle.Bold;
                    if (t.Font.Italic) fs |= FontStyle.Italic;
                    if (t.Font.Underline) fs |= FontStyle.Underline;
                    using (var font = new Font(t.Font.Family, (float)(t.Font.Size * ppm / (96.0 / 25.4) * 0.75), fs))
                    {
                        var color = Color.Black;
                        if (!string.IsNullOrEmpty(t.Font.Color))
                            try { color = ColorTranslator.FromHtml(t.Font.Color); } catch { }
                        using (var brush = new SolidBrush(color))
                        {
                            var sf = new StringFormat { Trimming = StringTrimming.EllipsisCharacter };
                            switch (t.Alignment)
                            {
                                case TextAlignment.Center: sf.Alignment = StringAlignment.Center; break;
                                case TextAlignment.Right: sf.Alignment = StringAlignment.Far; break;
                            }
                            g.DrawString(t.Text ?? "", font, brush, rect, sf);
                        }
                    }
                    break;
                case RenderedLineElement l:
                    using (var pen = new Pen(SafeColor(l.LineColor), (float)(l.LineWidth * ppm)))
                    {
                        if (l.Direction == LineDirection.Vertical)
                            g.DrawLine(pen, x + w / 2, y, x + w / 2, y + h);
                        else if (l.Direction == LineDirection.Diagonal)
                            g.DrawLine(pen, x, y, x + w, y + h);
                        else
                            g.DrawLine(pen, x, y + h / 2, x + w, y + h / 2);
                    }
                    break;
                case RenderedShapeElement s:
                    using (var fill = new SolidBrush(SafeColor(s.FillColor)))
                    {
                        if (s.Shape == ShapeType.Ellipse) g.FillEllipse(fill, rect);
                        else g.FillRectangle(fill, rect);
                    }
                    g.DrawRectangle(Pens.DimGray, x, y, w, h);
                    break;
            }

            // 边框
            if (el.Border != null)
            {
                var bColor = SafeColor(el.Border.Color);
                using (var pen = new Pen(bColor, (float)(el.Border.Width * ppm)))
                {
                    if (el.Border.Top) g.DrawLine(pen, x, y, x + w, y);
                    if (el.Border.Bottom) g.DrawLine(pen, x, y + h, x + w, y + h);
                    if (el.Border.Left) g.DrawLine(pen, x, y, x, y + h);
                    if (el.Border.Right) g.DrawLine(pen, x + w, y, x + w, y + h);
                }
            }
        }

        private static Color SafeColor(string? html)
        {
            if (string.IsNullOrEmpty(html)) return Color.Black;
            try { return ColorTranslator.FromHtml(html); } catch { return Color.Black; }
        }

        // ============================== 打印 ==============================

        public void PrintReport()
        {
            if (_report == null) return;
            var doc = new PrintDocument();
            int printPage = 0;
            doc.PrintPage += (_, e2) =>
            {
                if (e2.Graphics == null || _report == null) return;
                var page = _report.Pages[printPage];
                float ppm = e2.Graphics.DpiX / 25.4f;
                foreach (var el in page.Elements)
                    DrawElement(e2.Graphics, el, (float)(e2.MarginBounds.Left), (float)(e2.MarginBounds.Top), ppm);
                printPage++;
                e2.HasMorePages = printPage < _report.Pages.Count;
            };
            using (var dlg = new PrintPreviewDialog { Document = doc, Width = 800, Height = 600 })
            {
                dlg.ShowDialog();
            }
        }
    }
}
