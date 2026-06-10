using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ReportEngine.Core;

namespace ReportEngine.Designer.WinForms
{
    /// <summary>
    /// 报表设计画布（增强版）：
    /// - 自绘 Bands 与元素
    /// - 选中、拖动移动
    /// - 8 向 Resize 手柄
    /// - 网格吸附
    /// - 鼠标滚轮缩放
    /// - 撤销/重做
    /// </summary>
    public class DesignCanvas : UserControl
    {
        private ReportTemplate? _template;
        private ReportElement? _selectedElement;
        private Band? _selectedBand;

        // 拖动/缩放状态
        private enum DragMode { None, Move, Resize }
        private DragMode _dragMode;
        private int _resizeHandle = -1; // 0-7: TL,T,TR,R,BR,B,BL,L
        private Point _dragStart;
        private double _startX, _startY, _startW, _startH;

        // 撤销/重做
        private readonly List<UndoRecord> _undoStack = new List<UndoRecord>();
        private readonly List<UndoRecord> _redoStack = new List<UndoRecord>();
        private const int MaxUndo = 50;

        // === 公共属性 ===

        /// <summary>每毫米对应的像素数（含缩放）</summary>
        public double PixelsPerMm { get; private set; } = 96.0 / 25.4;

        private double _zoomFactor = 1.0;
        /// <summary>缩放因子 (0.25~4.0)</summary>
        public double ZoomFactor
        {
            get => _zoomFactor;
            set
            {
                _zoomFactor = Math.Max(0.25, Math.Min(4.0, value));
                PixelsPerMm = (96.0 / 25.4) * _zoomFactor;
                UpdateCanvasSize();
                Invalidate();
                ZoomChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>网格吸附步进（mm），0 表示不吸附</summary>
        public double GridSnapMm { get; set; } = 1.0;

        /// <summary>是否显示网格</summary>
        public bool ShowGrid { get; set; } = true;

        /// <summary>Resize 手柄大小（像素）</summary>
        private const int HandleSize = 7;

        public ReportTemplate? Template
        {
            get => _template;
            set
            {
                _template = value;
                _selectedElement = null;
                _selectedBand = null;
                _undoStack.Clear();
                _redoStack.Clear();
                UpdateCanvasSize();
                Invalidate();
            }
        }

        public ReportElement? SelectedElement => _selectedElement;
        public Band? SelectedBand => _selectedBand;

        public event EventHandler? SelectionChanged;
        public event EventHandler? ZoomChanged;

        public DesignCanvas()
        {
            DoubleBuffered = true;
            BackColor = Color.FromArgb(230, 230, 230);
            AutoScroll = true;
        }

        public void RefreshAll() { UpdateCanvasSize(); Invalidate(); }
        public void RaiseSelectionChanged() { SelectionChanged?.Invoke(this, EventArgs.Empty); Invalidate(); }

        private void UpdateCanvasSize()
        {
            if (_template == null) { AutoScrollMinSize = new Size(800, 600); return; }
            int w = (int)(_template.Page.Width * PixelsPerMm) + 60;
            double totalMm = 0;
            foreach (var b in _template.Bands) totalMm += b.Height;
            int h = (int)(totalMm * PixelsPerMm) + 80;
            AutoScrollMinSize = new Size(Math.Max(w, 600), Math.Max(h, 400));
        }

        // ============================== 吸附 ==============================

        private double Snap(double valueMm)
        {
            if (GridSnapMm <= 0) return valueMm;
            return Math.Round(valueMm / GridSnapMm) * GridSnapMm;
        }

        // ============================== 撤销/重做 ==============================

        private void PushUndo(ReportElement el)
        {
            _undoStack.Add(new UndoRecord(el, el.X, el.Y, el.Width, el.Height));
            if (_undoStack.Count > MaxUndo) _undoStack.RemoveAt(0);
            _redoStack.Clear();
        }

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public void Undo()
        {
            if (_undoStack.Count == 0) return;
            var rec = _undoStack[_undoStack.Count - 1];
            _undoStack.RemoveAt(_undoStack.Count - 1);
            _redoStack.Add(new UndoRecord(rec.Element, rec.Element.X, rec.Element.Y, rec.Element.Width, rec.Element.Height));
            rec.Element.X = rec.X; rec.Element.Y = rec.Y; rec.Element.Width = rec.W; rec.Element.Height = rec.H;
            Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Redo()
        {
            if (_redoStack.Count == 0) return;
            var rec = _redoStack[_redoStack.Count - 1];
            _redoStack.RemoveAt(_redoStack.Count - 1);
            _undoStack.Add(new UndoRecord(rec.Element, rec.Element.X, rec.Element.Y, rec.Element.Width, rec.Element.Height));
            rec.Element.X = rec.X; rec.Element.Y = rec.Y; rec.Element.Width = rec.W; rec.Element.Height = rec.H;
            Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        // ============================== 绘制 ==============================

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TranslateTransform(AutoScrollPosition.X, AutoScrollPosition.Y);

            if (_template == null)
            {
                using (var f = new Font("Segoe UI", 12))
                    g.DrawString("请通过菜单 [文件] -> [新建/打开] 创建或加载模板", f, Brushes.Gray, 30, 30);
                return;
            }

            float pageW = (float)(_template.Page.Width * PixelsPerMm);
            float originX = 20;
            float currentY = 20;

            // 网格
            if (ShowGrid && GridSnapMm > 0)
            {
                DrawGrid(g, originX, currentY, pageW);
            }

            foreach (var band in _template.Bands)
            {
                float bandH = (float)(band.Height * PixelsPerMm);
                var bandRect = new RectangleF(originX, currentY, pageW, bandH);
                DrawBand(g, band, bandRect);
                currentY += bandH;
            }
        }

        private void DrawGrid(Graphics g, float ox, float oy, float pageW)
        {
            if (_template == null) return;
            double totalH = 0;
            foreach (var b in _template.Bands) totalH += b.Height;
            float pageH = (float)(totalH * PixelsPerMm);

            float step = (float)(GridSnapMm * PixelsPerMm);
            if (step < 4) return; // 太密不画

            using (var pen = new Pen(Color.FromArgb(40, 100, 100, 100), 0.5f))
            {
                for (float x = ox; x <= ox + pageW; x += step)
                    g.DrawLine(pen, x, oy, x, oy + pageH);
                for (float y = oy; y <= oy + pageH; y += step)
                    g.DrawLine(pen, ox, y, ox + pageW, y);
            }
        }

        private void DrawBand(Graphics g, Band band, RectangleF bandRect)
        {
            using (var bg = new SolidBrush(GetBandColor(band.Type)))
                g.FillRectangle(bg, bandRect);
            using (var pen = new Pen(_selectedBand == band ? Color.DodgerBlue : Color.Gray,
                _selectedBand == band ? 2f : 1f))
                g.DrawRectangle(pen, bandRect.X, bandRect.Y, bandRect.Width, bandRect.Height);
            using (var f = new Font("Segoe UI", 8, FontStyle.Bold))
                g.DrawString(band.Type.ToString() + " (" + band.Height + "mm)", f, Brushes.DimGray, bandRect.X + 4, bandRect.Y + 2);

            foreach (var element in band.Elements)
            {
                var elRect = new RectangleF(
                    bandRect.X + (float)(element.X * PixelsPerMm),
                    bandRect.Y + (float)(element.Y * PixelsPerMm),
                    (float)(element.Width * PixelsPerMm),
                    (float)(element.Height * PixelsPerMm));
                DrawElement(g, element, elRect);

                if (_selectedElement == element)
                {
                    // 选中虚线框
                    using (var pen = new Pen(Color.OrangeRed, 1.5f) { DashStyle = DashStyle.Dash })
                        g.DrawRectangle(pen, elRect.X - 1, elRect.Y - 1, elRect.Width + 2, elRect.Height + 2);
                    // 8 个 Resize 手柄
                    DrawHandles(g, elRect);
                }
            }
        }

        private void DrawHandles(Graphics g, RectangleF r)
        {
            var handles = GetHandleRects(r);
            for (int i = 0; i < handles.Length; i++)
            {
                g.FillRectangle(Brushes.White, handles[i]);
                g.DrawRectangle(Pens.OrangeRed, handles[i].X, handles[i].Y, handles[i].Width, handles[i].Height);
            }
        }

        private RectangleF[] GetHandleRects(RectangleF r)
        {
            int hs = HandleSize;
            float cx = r.X + r.Width / 2;
            float cy = r.Y + r.Height / 2;
            return new RectangleF[]
            {
                new RectangleF(r.Left - hs/2, r.Top - hs/2, hs, hs),       // 0 TL
                new RectangleF(cx - hs/2,     r.Top - hs/2, hs, hs),       // 1 T
                new RectangleF(r.Right - hs/2, r.Top - hs/2, hs, hs),      // 2 TR
                new RectangleF(r.Right - hs/2, cy - hs/2, hs, hs),         // 3 R
                new RectangleF(r.Right - hs/2, r.Bottom - hs/2, hs, hs),   // 4 BR
                new RectangleF(cx - hs/2,      r.Bottom - hs/2, hs, hs),   // 5 B
                new RectangleF(r.Left - hs/2,  r.Bottom - hs/2, hs, hs),   // 6 BL
                new RectangleF(r.Left - hs/2,  cy - hs/2, hs, hs),         // 7 L
            };
        }

        private static readonly Cursor[] HandleCursors = new Cursor[]
        {
            Cursors.SizeNWSE, Cursors.SizeNS, Cursors.SizeNESW, Cursors.SizeWE,
            Cursors.SizeNWSE, Cursors.SizeNS, Cursors.SizeNESW, Cursors.SizeWE,
        };

        private static Color GetBandColor(BandType t)
        {
            switch (t)
            {
                case BandType.ReportHeader: return Color.FromArgb(255, 244, 220);
                case BandType.ReportFooter: return Color.FromArgb(255, 244, 220);
                case BandType.Header:       return Color.FromArgb(220, 235, 255);
                case BandType.Footer:       return Color.FromArgb(220, 235, 255);
                case BandType.GroupHeader:  return Color.FromArgb(232, 245, 232);
                case BandType.GroupFooter:  return Color.FromArgb(232, 245, 232);
                case BandType.Detail:       return Color.White;
                default: return Color.White;
            }
        }

        private static void DrawElement(Graphics g, ReportElement element, RectangleF rect)
        {
            if (!string.IsNullOrEmpty(element.BackgroundColor))
            {
                try { using (var br = new SolidBrush(ColorTranslator.FromHtml(element.BackgroundColor))) g.FillRectangle(br, rect); } catch { }
            }

            switch (element)
            {
                case TextElement t:
                    var fs = FontStyle.Regular;
                    if (t.Font.Bold) fs |= FontStyle.Bold;
                    if (t.Font.Italic) fs |= FontStyle.Italic;
                    if (t.Font.Underline) fs |= FontStyle.Underline;
                    using (var font = new Font(t.Font.Family, (float)t.Font.Size, fs))
                    using (var brush = new SolidBrush(string.IsNullOrEmpty(t.Font.Color) ? Color.Black : SafeColor(t.Font.Color, Color.Black)))
                    {
                        var sf = new StringFormat { Trimming = StringTrimming.EllipsisCharacter };
                        switch (t.Alignment)
                        {
                            case TextAlignment.Center: sf.Alignment = StringAlignment.Center; break;
                            case TextAlignment.Right: sf.Alignment = StringAlignment.Far; break;
                        }
                        g.DrawString(t.Text ?? "", font, brush, rect, sf);
                    }
                    break;
                case LineElement l:
                    using (var pen = new Pen(SafeColor(l.LineColor, Color.Black), (float)l.LineWidth))
                    {
                        if (l.Direction == LineDirection.Vertical)
                            g.DrawLine(pen, rect.X + rect.Width / 2, rect.Top, rect.X + rect.Width / 2, rect.Bottom);
                        else if (l.Direction == LineDirection.Diagonal)
                            g.DrawLine(pen, rect.Left, rect.Top, rect.Right, rect.Bottom);
                        else
                            g.DrawLine(pen, rect.Left, rect.Y + rect.Height / 2, rect.Right, rect.Y + rect.Height / 2);
                    }
                    break;
                case ImageElement img:
                    using (var pen = new Pen(Color.SteelBlue) { DashStyle = DashStyle.Dash })
                    {
                        g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                        g.DrawLine(pen, rect.Left, rect.Top, rect.Right, rect.Bottom);
                        g.DrawLine(pen, rect.Right, rect.Top, rect.Left, rect.Bottom);
                    }
                    using (var f = new Font("Segoe UI", 7))
                        g.DrawString("[Image]", f, Brushes.SteelBlue, rect.X + 2, rect.Y + 2);
                    break;
                case ShapeElement s:
                    using (var fill = new SolidBrush(SafeColor(s.FillColor, Color.White)))
                    using (var pen = new Pen(Color.DimGray))
                    {
                        if (s.Shape == ShapeType.Ellipse) { g.FillEllipse(fill, rect); g.DrawEllipse(pen, rect); }
                        else { g.FillRectangle(fill, rect); g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height); }
                    }
                    break;
                case SubReportElement sr:
                    using (var br = new SolidBrush(Color.FromArgb(240, 248, 255)))
                        g.FillRectangle(br, rect);
                    using (var pen = new Pen(Color.MediumPurple, 1.5f) { DashStyle = DashStyle.Dash })
                        g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                    using (var f = new Font("Segoe UI", 8, FontStyle.Bold))
                        g.DrawString("[SubReport] " + sr.TemplateRef, f, Brushes.MediumPurple, rect.X + 4, rect.Y + 4);
                    break;
                case BarcodeElement bc:
                    using (var br = new SolidBrush(Color.FromArgb(255, 250, 240)))
                        g.FillRectangle(br, rect);
                    g.DrawRectangle(Pens.DarkOrange, rect.X, rect.Y, rect.Width, rect.Height);
                    using (var f = new Font("Segoe UI", 7))
                        g.DrawString("[" + bc.Format + "] " + bc.Value, f, Brushes.DarkOrange, rect.X + 2, rect.Y + 2);
                    break;
                case TableElement tbl:
                    g.FillRectangle(Brushes.White, rect);
                    g.DrawRectangle(Pens.DarkSlateGray, rect.X, rect.Y, rect.Width, rect.Height);
                    // 画简化网格
                    float cw = rect.Width / Math.Max(tbl.ColCount, 1);
                    float rh = rect.Height / Math.Max(tbl.RowCount, 1);
                    using (var gpen = new Pen(Color.LightGray))
                    {
                        for (int ci = 1; ci < tbl.ColCount; ci++) g.DrawLine(gpen, rect.X + ci * cw, rect.Y, rect.X + ci * cw, rect.Bottom);
                        for (int ri = 1; ri < tbl.RowCount; ri++) g.DrawLine(gpen, rect.X, rect.Y + ri * rh, rect.Right, rect.Y + ri * rh);
                    }
                    using (var f = new Font("Segoe UI", 7))
                        g.DrawString("[Table " + tbl.RowCount + "x" + tbl.ColCount + "]", f, Brushes.DarkSlateGray, rect.X + 2, rect.Y + 2);
                    break;
                case CrossTabElement ct:
                    g.FillRectangle(Brushes.AliceBlue, rect);
                    g.DrawRectangle(Pens.DarkCyan, rect.X, rect.Y, rect.Width, rect.Height);
                    using (var f = new Font("Segoe UI", 7))
                        g.DrawString("[CrossTab] " + ct.DataSource, f, Brushes.DarkCyan, rect.X + 2, rect.Y + 2);
                    break;
                default:
                    using (var pen = new Pen(Color.Silver) { DashStyle = DashStyle.Dot })
                        g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                    break;
            }

            if (element.Border != null)
            {
                try
                {
                    var c = ColorTranslator.FromHtml(element.Border.Color);
                    using (var pen = new Pen(c, (float)element.Border.Width))
                    {
                        if (element.Border.Top) g.DrawLine(pen, rect.Left, rect.Top, rect.Right, rect.Top);
                        if (element.Border.Bottom) g.DrawLine(pen, rect.Left, rect.Bottom, rect.Right, rect.Bottom);
                        if (element.Border.Left) g.DrawLine(pen, rect.Left, rect.Top, rect.Left, rect.Bottom);
                        if (element.Border.Right) g.DrawLine(pen, rect.Right, rect.Top, rect.Right, rect.Bottom);
                    }
                }
                catch { }
            }
        }

        private static Color SafeColor(string? html, Color fallback)
        {
            if (string.IsNullOrEmpty(html)) return fallback;
            try { return ColorTranslator.FromHtml(html); } catch { return fallback; }
        }

        // ============================== 鼠标事件 ==============================

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (_template == null) return;
            Focus();

            // 检查是否点中了 Resize 手柄
            if (_selectedElement != null)
            {
                var elRect = GetElementScreenRect(_selectedElement);
                if (elRect.HasValue)
                {
                    var handles = GetHandleRects(elRect.Value);
                    var pt = new PointF(e.X - AutoScrollPosition.X, e.Y - AutoScrollPosition.Y);
                    for (int i = 0; i < handles.Length; i++)
                    {
                        if (handles[i].Contains(pt))
                        {
                            PushUndo(_selectedElement);
                            _dragMode = DragMode.Resize;
                            _resizeHandle = i;
                            _dragStart = e.Location;
                            _startX = _selectedElement.X;
                            _startY = _selectedElement.Y;
                            _startW = _selectedElement.Width;
                            _startH = _selectedElement.Height;
                            return;
                        }
                    }
                }
            }

            var (band, element, _, _) = HitTest(e.Location);
            _selectedBand = band;
            if (element != _selectedElement)
            {
                _selectedElement = element;
            }
            if (element != null)
            {
                PushUndo(element);
                _dragMode = DragMode.Move;
                _dragStart = e.Location;
                _startX = element.X;
                _startY = element.Y;
                _startW = element.Width;
                _startH = element.Height;
            }
            else
            {
                _dragMode = DragMode.None;
            }
            RaiseSelectionChanged();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // Cursor hint
            if (_dragMode == DragMode.None && _selectedElement != null)
            {
                var elRect = GetElementScreenRect(_selectedElement);
                if (elRect.HasValue)
                {
                    var pt = new PointF(e.X - AutoScrollPosition.X, e.Y - AutoScrollPosition.Y);
                    var handles = GetHandleRects(elRect.Value);
                    for (int i = 0; i < handles.Length; i++)
                    {
                        if (handles[i].Contains(pt)) { Cursor = HandleCursors[i]; return; }
                    }
                }
                Cursor = Cursors.Default;
            }

            if (_dragMode == DragMode.None || _selectedElement == null) return;

            int dx = e.X - _dragStart.X;
            int dy = e.Y - _dragStart.Y;
            double dxMm = dx / PixelsPerMm;
            double dyMm = dy / PixelsPerMm;

            if (_dragMode == DragMode.Move)
            {
                _selectedElement.X = Snap(Math.Max(0, _startX + dxMm));
                _selectedElement.Y = Snap(Math.Max(0, _startY + dyMm));
            }
            else if (_dragMode == DragMode.Resize)
            {
                ApplyResize(dxMm, dyMm);
            }

            Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _dragMode = DragMode.None;
            _resizeHandle = -1;
            Cursor = Cursors.Default;
        }

        /// <summary>鼠标滚轮缩放</summary>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (ModifierKeys == Keys.Control)
            {
                ZoomFactor += e.Delta > 0 ? 0.1 : -0.1;
                ((HandledMouseEventArgs)e).Handled = true;
            }
            else
            {
                base.OnMouseWheel(e);
            }
        }

        // ============================== Resize 逻辑 ==============================

        private void ApplyResize(double dxMm, double dyMm)
        {
            if (_selectedElement == null) return;
            double x = _startX, y = _startY, w = _startW, h = _startH;

            switch (_resizeHandle)
            {
                case 0: // TL
                    x += dxMm; y += dyMm; w -= dxMm; h -= dyMm; break;
                case 1: // T
                    y += dyMm; h -= dyMm; break;
                case 2: // TR
                    y += dyMm; w += dxMm; h -= dyMm; break;
                case 3: // R
                    w += dxMm; break;
                case 4: // BR
                    w += dxMm; h += dyMm; break;
                case 5: // B
                    h += dyMm; break;
                case 6: // BL
                    x += dxMm; w -= dxMm; h += dyMm; break;
                case 7: // L
                    x += dxMm; w -= dxMm; break;
            }

            // 最小尺寸
            if (w < 2) w = 2;
            if (h < 1) h = 1;

            _selectedElement.X = Snap(Math.Max(0, x));
            _selectedElement.Y = Snap(Math.Max(0, y));
            _selectedElement.Width = Snap(w);
            _selectedElement.Height = Snap(h);
        }

        // ============================== 命中测试 ==============================

        private RectangleF? GetElementScreenRect(ReportElement el)
        {
            if (_template == null) return null;
            float pageW = (float)(_template.Page.Width * PixelsPerMm);
            float originX = 20;
            float currentY = 20;
            foreach (var band in _template.Bands)
            {
                float bandH = (float)(band.Height * PixelsPerMm);
                if (band.Elements.Contains(el))
                {
                    return new RectangleF(
                        originX + (float)(el.X * PixelsPerMm),
                        currentY + (float)(el.Y * PixelsPerMm),
                        (float)(el.Width * PixelsPerMm),
                        (float)(el.Height * PixelsPerMm));
                }
                currentY += bandH;
            }
            return null;
        }

        private (Band? band, ReportElement? element, RectangleF bandRect, RectangleF elemRect) HitTest(Point p)
        {
            if (_template == null) return (null, null, RectangleF.Empty, RectangleF.Empty);
            var pt = new PointF(p.X - AutoScrollPosition.X, p.Y - AutoScrollPosition.Y);
            float pageW = (float)(_template.Page.Width * PixelsPerMm);
            float originX = 20;
            float currentY = 20;

            foreach (var band in _template.Bands)
            {
                float bandH = (float)(band.Height * PixelsPerMm);
                var bandRect = new RectangleF(originX, currentY, pageW, bandH);

                for (int i = band.Elements.Count - 1; i >= 0; i--)
                {
                    var el = band.Elements[i];
                    var er = new RectangleF(
                        bandRect.X + (float)(el.X * PixelsPerMm),
                        bandRect.Y + (float)(el.Y * PixelsPerMm),
                        (float)(el.Width * PixelsPerMm),
                        (float)(el.Height * PixelsPerMm));
                    if (er.Contains(pt)) return (band, el, bandRect, er);
                }
                if (bandRect.Contains(pt)) return (band, null, bandRect, RectangleF.Empty);
                currentY += bandH;
            }
            return (null, null, RectangleF.Empty, RectangleF.Empty);
        }

        // ============================== 操作 ==============================

        public void DeleteSelected()
        {
            if (_selectedElement == null || _selectedBand == null) return;
            _selectedBand.Elements.Remove(_selectedElement);
            _selectedElement = null;
            RaiseSelectionChanged();
        }

        public void AddElement(ReportElement element, Band? targetBand = null)
        {
            if (_template == null) return;
            var band = targetBand ?? _selectedBand ?? _template.Bands[0];
            band.Elements.Add(element);
            _selectedBand = band;
            _selectedElement = element;
            RefreshAll();
            RaiseSelectionChanged();
        }

        // ============================== 内部类 ==============================

        private class UndoRecord
        {
            public ReportElement Element { get; }
            public double X { get; }
            public double Y { get; }
            public double W { get; }
            public double H { get; }
            public UndoRecord(ReportElement el, double x, double y, double w, double h)
            { Element = el; X = x; Y = y; W = w; H = h; }
        }
    }
}
