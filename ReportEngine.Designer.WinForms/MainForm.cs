using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ReportEngine.Core;
using ReportEngine.Core.Parsing;

namespace ReportEngine.Designer.WinForms
{
    public class MainForm : Form
    {
        private readonly TemplateParser _parser = new TemplateParser();
        private readonly DesignCanvas _canvas;
        private readonly PropertyGrid _propertyGrid;
        private readonly TreeView _bandTree;
        private readonly StatusStrip _statusStrip;
        private readonly ToolStripStatusLabel _statusLabel;

        private string? _currentFilePath;
        private bool _dirty;

        public MainForm()
        {
            Text = "Report Designer";
            Width = 1280;
            Height = 800;
            StartPosition = FormStartPosition.CenterScreen;

            _canvas = new DesignCanvas { Dock = DockStyle.Fill };
            _canvas.SelectionChanged += OnCanvasSelectionChanged;

            _propertyGrid = new PropertyGrid
            {
                Dock = DockStyle.Fill,
                ToolbarVisible = true,
                HelpVisible = true,
                PropertySort = PropertySort.Categorized,
            };
            _propertyGrid.PropertyValueChanged += OnPropertyValueChanged;

            _bandTree = new TreeView { Dock = DockStyle.Fill, HideSelection = false };
            _bandTree.AfterSelect += OnBandTreeAfterSelect;

            _statusStrip = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel("就绪");
            _statusStrip.Items.Add(_statusLabel);

            BuildLayout();
            BuildMenu();

            // 默认创建一份空模板
            NewTemplate();
        }

        // ============================== 布局 ==============================

        private void BuildLayout()
        {
            var split1 = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 220,
            };
            var split2 = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 700,
            };

            // 左：Bands TreeView
            var leftPanel = new Panel { Dock = DockStyle.Fill };
            var leftLabel = new Label { Text = "  报表结构", Dock = DockStyle.Top, Height = 24, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            leftPanel.Controls.Add(_bandTree);
            leftPanel.Controls.Add(leftLabel);
            split1.Panel1.Controls.Add(leftPanel);
            split1.Panel1.Controls.Add(BuildToolbox());

            // 右上：画布
            var canvasPanel = new Panel { Dock = DockStyle.Fill };
            canvasPanel.Controls.Add(_canvas);
            split2.Panel1.Controls.Add(canvasPanel);

            // 右下：属性
            var propPanel = new Panel { Dock = DockStyle.Fill };
            var propLabel = new Label { Text = "  属性", Dock = DockStyle.Top, Height = 24, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            propPanel.Controls.Add(_propertyGrid);
            propPanel.Controls.Add(propLabel);
            split2.Panel2.Controls.Add(propPanel);

            split1.Panel2.Controls.Add(split2);

            Controls.Add(split1);
            Controls.Add(_statusStrip);
        }

        private Control BuildToolbox()
        {
            var toolbox = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 310,
                BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(4),
            };
            toolbox.Controls.Add(new Label { Text = "插入元素", Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = true });
            AddTbButton(toolbox, "文本 (Text)",      () => InsertElement(NewText()));
            AddTbButton(toolbox, "线条 (Line)",      () => InsertElement(NewLine()));
            AddTbButton(toolbox, "图像 (Image)",     () => InsertElement(NewImage()));
            AddTbButton(toolbox, "矩形 (Shape)",     () => InsertElement(NewShape()));
            AddTbButton(toolbox, "子报表 (SubReport)", () => InsertElement(NewSubReport()));
            AddTbButton(toolbox, "条码/二维码 (Barcode)", () => InsertElement(NewBarcode()));
            AddTbButton(toolbox, "表格 (Table)",     () => InsertElement(NewTable()));
            AddTbButton(toolbox, "交叉表 (CrossTab)", () => InsertElement(NewCrossTab()));
            return toolbox;
        }

        private static void AddTbButton(FlowLayoutPanel host, string text, Action onClick)
        {
            var b = new Button
            {
                Text = text,
                Width = 190,
                Height = 28,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(2),
            };
            b.Click += (_, __) => onClick();
            host.Controls.Add(b);
        }

        private void BuildMenu()
        {
            var menu = new MenuStrip();

            var file = new ToolStripMenuItem("文件(&F)");
            file.DropDownItems.Add(new ToolStripMenuItem("新建(&N)", null, (_, __) => NewTemplate()) { ShortcutKeys = Keys.Control | Keys.N });
            file.DropDownItems.Add(new ToolStripMenuItem("打开(&O)...", null, (_, __) => OpenTemplate()) { ShortcutKeys = Keys.Control | Keys.O });
            file.DropDownItems.Add(new ToolStripSeparator());
            file.DropDownItems.Add(new ToolStripMenuItem("保存(&S)", null, (_, __) => SaveTemplate(false)) { ShortcutKeys = Keys.Control | Keys.S });
            file.DropDownItems.Add(new ToolStripMenuItem("另存为(&A)...", null, (_, __) => SaveTemplate(true)));
            file.DropDownItems.Add(new ToolStripSeparator());
            file.DropDownItems.Add(new ToolStripMenuItem("退出(&X)", null, (_, __) => Close()));

            var insert = new ToolStripMenuItem("插入(&I)");
            insert.DropDownItems.Add(new ToolStripMenuItem("文本", null, (_, __) => InsertElement(NewText())));
            insert.DropDownItems.Add(new ToolStripMenuItem("线条", null, (_, __) => InsertElement(NewLine())));
            insert.DropDownItems.Add(new ToolStripMenuItem("图像", null, (_, __) => InsertElement(NewImage())));
            insert.DropDownItems.Add(new ToolStripMenuItem("矩形", null, (_, __) => InsertElement(NewShape())));
            insert.DropDownItems.Add(new ToolStripMenuItem("子报表", null, (_, __) => InsertElement(NewSubReport())));
            insert.DropDownItems.Add(new ToolStripMenuItem("条码/二维码", null, (_, __) => InsertElement(NewBarcode())));
            insert.DropDownItems.Add(new ToolStripMenuItem("表格", null, (_, __) => InsertElement(NewTable())));
            insert.DropDownItems.Add(new ToolStripMenuItem("交叉表", null, (_, __) => InsertElement(NewCrossTab())));
            insert.DropDownItems.Add(new ToolStripSeparator());
            insert.DropDownItems.Add(new ToolStripMenuItem("Band - Header", null, (_, __) => AddBand(BandType.Header, 15)));
            insert.DropDownItems.Add(new ToolStripMenuItem("Band - Detail", null, (_, __) => AddBand(BandType.Detail, 10)));
            insert.DropDownItems.Add(new ToolStripMenuItem("Band - Footer", null, (_, __) => AddBand(BandType.Footer, 10)));

            var edit = new ToolStripMenuItem("编辑(&E)");
            edit.DropDownItems.Add(new ToolStripMenuItem("撤销(&Z)", null, (_, __) => { _canvas.Undo(); MarkDirty(); }) { ShortcutKeys = Keys.Control | Keys.Z });
            edit.DropDownItems.Add(new ToolStripMenuItem("重做(&Y)", null, (_, __) => { _canvas.Redo(); MarkDirty(); }) { ShortcutKeys = Keys.Control | Keys.Y });
            edit.DropDownItems.Add(new ToolStripSeparator());
            edit.DropDownItems.Add(new ToolStripMenuItem("删除选中元素", null, (_, __) => { _canvas.DeleteSelected(); MarkDirty(); }) { ShortcutKeys = Keys.Delete });

            var help = new ToolStripMenuItem("帮助(&H)");
            help.DropDownItems.Add(new ToolStripMenuItem("关于...", null, (_, __) => MessageBox.Show(this, "Report Designer (WinForms)\n基于 ReportEngine.Core 多目标库\nMVP", "关于")));

            menu.Items.AddRange(new ToolStripItem[] { file, insert, edit, help });
            MainMenuStrip = menu;
            Controls.Add(menu);
        }

        // ============================== 事件 ==============================

        private void OnCanvasSelectionChanged(object? sender, EventArgs e)
        {
            object? target = (object?)_canvas.SelectedElement ?? _canvas.SelectedBand;
            _propertyGrid.SelectedObject = target;
            UpdateBandTree();
            UpdateStatus();
        }

        private void OnPropertyValueChanged(object? s, PropertyValueChangedEventArgs e)
        {
            _canvas.RefreshAll();
            MarkDirty();
        }

        private void OnBandTreeAfterSelect(object? sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag is Band b)
            {
                _propertyGrid.SelectedObject = b;
            }
            else if (e.Node?.Tag is ReportElement el)
            {
                _propertyGrid.SelectedObject = el;
            }
        }

        // ============================== 文件 ==============================

        private void NewTemplate()
        {
            if (!ConfirmDiscard()) return;
            var t = new ReportTemplate
            {
                Page = new PageInfo { Width = 210, Height = 297 },
                Bands = new List<Band>
                {
                    new Band { Type = BandType.Header,  Height = 15 },
                    new Band { Type = BandType.Detail,  Height = 10 },
                    new Band { Type = BandType.Footer,  Height = 10 },
                },
            };
            _canvas.Template = t;
            _currentFilePath = null;
            _dirty = false;
            UpdateBandTree();
            UpdateTitle();
            UpdateStatus();
        }

        private void OpenTemplate()
        {
            if (!ConfirmDiscard()) return;
            using var dlg = new OpenFileDialog
            {
                Filter = "报表模板 (*.rptx)|*.rptx|JSON 文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                Title = "打开报表模板",
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            try
            {
                var t = _parser.ParseFile(dlg.FileName);
                _canvas.Template = t;
                _currentFilePath = dlg.FileName;
                _dirty = false;
                UpdateBandTree();
                UpdateTitle();
                _statusLabel.Text = "已加载: " + dlg.FileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "打开模板失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveTemplate(bool saveAs)
        {
            if (_canvas.Template == null) return;
            var path = _currentFilePath;
            if (saveAs || string.IsNullOrEmpty(path))
            {
                using var dlg = new SaveFileDialog
                {
                    Filter = "报表模板 (*.rptx)|*.rptx|JSON 文件 (*.json)|*.json",
                    DefaultExt = "rptx",
                    Title = "保存报表模板",
                    FileName = path != null ? Path.GetFileName(path) : "untitled.rptx",
                };
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                path = dlg.FileName;
            }
            try
            {
                var json = _parser.Serialize(_canvas.Template);
                File.WriteAllText(path!, json);
                _currentFilePath = path;
                _dirty = false;
                UpdateTitle();
                _statusLabel.Text = "已保存: " + path;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "保存失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ConfirmDiscard()
        {
            if (!_dirty) return true;
            var r = MessageBox.Show(this, "当前模板有未保存的修改，是否放弃？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return r == DialogResult.Yes;
        }

        // ============================== 元素工厂 ==============================

        private static TextElement NewText()      => new TextElement { X = 10, Y = 2, Width = 60, Height = 6, Text = "Sample Text" };
        private static LineElement NewLine()      => new LineElement { X = 10, Y = 4, Width = 80, Height = 1, Direction = LineDirection.Horizontal };
        private static ImageElement NewImage()    => new ImageElement { X = 10, Y = 2, Width = 30, Height = 20, Source = "" };
        private static ShapeElement NewShape()    => new ShapeElement { X = 10, Y = 2, Width = 40, Height = 8, Shape = ShapeType.Rectangle, FillColor = "#E8F0FE" };
        private static SubReportElement NewSubReport() => new SubReportElement { X = 10, Y = 2, Width = 100, Height = 10, TemplateRef = "subreport.rptx" };
        private static BarcodeElement NewBarcode() => new BarcodeElement { X = 10, Y = 2, Width = 30, Height = 30, Value = "123456", Format = BarcodeFormat.QRCode };
        private static TableElement NewTable() => new TableElement { X = 5, Y = 2, Width = 120, Height = 30, RowCount = 3, ColCount = 4 };
        private static CrossTabElement NewCrossTab() => new CrossTabElement { X = 5, Y = 2, Width = 140, Height = 40, DataSource = "" };

        private void InsertElement(ReportElement el)
        {
            if (_canvas.Template == null) return;
            _canvas.AddElement(el);
            UpdateBandTree();
            MarkDirty();
        }

        private void AddBand(BandType type, double heightMm)
        {
            if (_canvas.Template == null) return;
            _canvas.Template.Bands.Add(new Band { Type = type, Height = heightMm });
            _canvas.RefreshAll();
            UpdateBandTree();
            MarkDirty();
        }

        // ============================== 树 / 状态 / 标题 ==============================

        private void UpdateBandTree()
        {
            _bandTree.BeginUpdate();
            _bandTree.Nodes.Clear();
            if (_canvas.Template != null)
            {
                var root = _bandTree.Nodes.Add("Template");
                foreach (var band in _canvas.Template.Bands)
                {
                    var bn = root.Nodes.Add(band.Type + " (" + band.Height + "mm)");
                    bn.Tag = band;
                    foreach (var el in band.Elements)
                    {
                        var en = bn.Nodes.Add(el.GetType().Name + " #" + (el.Id.Length > 4 ? el.Id.Substring(0, 4) : el.Id));
                        en.Tag = el;
                    }
                }
                root.ExpandAll();
            }
            _bandTree.EndUpdate();
        }

        private void UpdateTitle()
        {
            var name = _currentFilePath == null ? "untitled" : Path.GetFileName(_currentFilePath);
            Text = "Report Designer - " + name + (_dirty ? " *" : "");
        }

        private void UpdateStatus()
        {
            if (_canvas.SelectedElement != null)
            {
                var el = _canvas.SelectedElement;
                _statusLabel.Text = string.Format("已选中: {0}  X={1:F1}mm Y={2:F1}mm  W={3:F1}mm H={4:F1}mm",
                    el.GetType().Name, el.X, el.Y, el.Width, el.Height);
            }
            else if (_canvas.SelectedBand != null)
            {
                _statusLabel.Text = "选中 Band: " + _canvas.SelectedBand.Type;
            }
            else
            {
                _statusLabel.Text = "就绪";
            }
        }

        private void MarkDirty()
        {
            _dirty = true;
            UpdateTitle();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!ConfirmDiscard()) e.Cancel = true;
            base.OnFormClosing(e);
        }
    }
}
