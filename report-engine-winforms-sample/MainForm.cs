using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using ReportEngine.Core;
using ReportEngine.Core.Parsing;
using ReportEngine.Core.Rendering;
using ReportEngine.Core.SubReports;
using ReportEngine.Export.Excel;
using ReportEngine.Export.Pdf;
using ReportEngine.Viewer.WinForms;

namespace ReportEngine.WinFormsSample;

/// <summary>
/// ReportEngine WinForms 集成示例
///
/// 演示（对称 WPF sample）：
///   1. 读取 .rptx 模板（JSON） → ReportTemplate
///   2. 传入数据 → RenderAsync → RenderedReport → viewer.SetReport
///   3. 替换标题 / 保存 JSON / 导出 PDF / 导出 Excel
///
/// 注意：WinForms 版 ReportViewerControl 自带工具栏（首页/翻页/缩放/适合宽度/打印），
///       所以本示例无需再做翻页控件，专注 5 个业务操作按钮。
/// </summary>
public class MainForm : Form
{
    private readonly TemplateParser _parser = new();
    private readonly ITemplateResolver _resolver;
    private readonly ReportRenderer _renderer;
    private ReportTemplate? _currentTemplate;

    private readonly ReportViewerControl _viewer = new();
    private readonly TextBox _txtStatus = new();
    private readonly Button _btnLoad = MakeButton("1. 加载模板 + 渲染预览", 44);
    private readonly Button _btnEditTitle = MakeButton("2. 替换标题后重新渲染", 84);
    private readonly Button _btnSaveJson = MakeButton("3. 保存当前模板为 JSON", 124);
    private readonly Button _btnExportPdf = MakeButton("4. 导出 PDF", 164);
    private readonly Button _btnExportExcel = MakeButton("5. 导出 Excel", 204);

    public MainForm()
    {
        Text = "ReportEngine WinForms Sample - 模板读取/替换/预览/导出";
        ClientSize = new Size(1100, 720);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Microsoft YaHei UI", 9f);

        var templatesDir = Path.Combine(AppContext.BaseDirectory, "Templates");
        _resolver = new FileSystemTemplateResolver(templatesDir);
        _renderer = new ReportRenderer(_resolver);

        // ---- 左侧控制面板 ----
        var left = new Panel
        {
            Dock = DockStyle.Left,
            Width = 340,
            Padding = new Padding(14),
            BackColor = Color.FromArgb(245, 245, 245)
        };

        var lblTitle = new Label
        {
            Text = "ReportEngine WinForms Sample",
            Font = new Font("Microsoft YaHei UI", 12f, FontStyle.Bold),
            Location = new Point(14, 12),
            AutoSize = true
        };
        var lblHint = new Label
        {
            Text = "演示：模板读取 + 替换 + 预览 + 导出",
            ForeColor = Color.DimGray,
            Location = new Point(14, 26),
            AutoSize = true
        };

        _btnEditTitle.Enabled = false;
        _btnSaveJson.Enabled = false;
        _btnExportPdf.Enabled = false;
        _btnExportExcel.Enabled = false;

        _btnLoad.Click += async (_, __) => await LoadAndRenderAsync();
        _btnEditTitle.Click += async (_, __) => await EditTitleAndRenderAsync();
        _btnSaveJson.Click += OnSaveJson;
        _btnExportPdf.Click += OnExportPdf;
        _btnExportExcel.Click += OnExportExcel;

        var lblStatus = new Label
        {
            Text = "状态",
            Font = new Font("Microsoft YaHei UI", 9f, FontStyle.Bold),
            Location = new Point(14, 250),
            AutoSize = true
        };
        _txtStatus.Location = new Point(14, 270);
        _txtStatus.Size = new Size(312, 410);
        _txtStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        _txtStatus.Multiline = true;
        _txtStatus.ReadOnly = true;
        _txtStatus.ScrollBars = ScrollBars.Vertical;
        _txtStatus.BackColor = Color.White;
        _txtStatus.Font = new Font("Consolas", 9.5f);

        left.Controls.Add(lblTitle);
        left.Controls.Add(lblHint);
        left.Controls.Add(_btnLoad);
        left.Controls.Add(_btnEditTitle);
        left.Controls.Add(_btnSaveJson);
        left.Controls.Add(_btnExportPdf);
        left.Controls.Add(_btnExportExcel);
        left.Controls.Add(lblStatus);
        left.Controls.Add(_txtStatus);

        // ---- 右侧预览控件（自带工具栏）----
        _viewer.Dock = DockStyle.Fill;
        _viewer.BackColor = Color.FromArgb(80, 80, 80);

        Controls.Add(_viewer);
        Controls.Add(left);

        Load += async (_, __) => await LoadAndRenderAsync();
    }

    private static Button MakeButton(string text, int y)
    {
        return new Button
        {
            Text = text,
            Location = new Point(14, y),
            Size = new Size(312, 32),
            FlatStyle = FlatStyle.System
        };
    }

    // ==================== 按钮 1：加载 + 渲染 ====================
    private async Task LoadAndRenderAsync()
    {
        try
        {
            var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "order-summary.rptx");
            Log($"加载模板: {templatePath}");

            _currentTemplate = _parser.ParseFile(templatePath);
            Log($"  解析成功: {_currentTemplate.Bands.Count} bands");

            var rendered = await _renderer.RenderAsync(_currentTemplate, SampleData());
            Log($"  渲染完成: {rendered.Pages.Count} 页");

            _viewer.SetReport(rendered);
            Log($"  ✓ 已显示预览（右侧工具栏可翻页/缩放/打印）");

            EnableButtons();
        }
        catch (Exception ex)
        {
            Log($"✗ 错误: {ex.Message}");
        }
    }

    // ==================== 按钮 2：替换标题 + 重新渲染 ====================
    private async Task EditTitleAndRenderAsync()
    {
        if (_currentTemplate == null) { Log("请先加载模板"); return; }

        try
        {
            var titleBand = _currentTemplate.Bands[0];
            if (titleBand.Elements[0] is TextElement textEl)
            {
                textEl.Text = $"订单汇总（{DateTime.Now:HH:mm:ss} 重新生成）";
                Log($"替换标题: {textEl.Text}");
            }
            else
            {
                Log($"✗ 第一个元素不是 TextElement: {titleBand.Elements[0].GetType().Name}");
                return;
            }

            var rendered = await _renderer.RenderAsync(_currentTemplate, SampleData());
            _viewer.SetReport(rendered);
            Log($"  ✓ 重新渲染: {rendered.Pages.Count} 页");
        }
        catch (Exception ex)
        {
            Log($"✗ 错误: {ex.Message}");
        }
    }

    // ==================== 按钮 3：保存模板为 JSON ====================
    private void OnSaveJson(object? sender, EventArgs e)
    {
        if (_currentTemplate == null) { Log("请先加载模板"); return; }

        try
        {
            string json = _parser.Serialize(_currentTemplate);
            var outPath = Path.Combine(AppContext.BaseDirectory, "exported-template.rptx");
            File.WriteAllText(outPath, json);
            Log($"✓ 已保存: {outPath}");
            Log($"  JSON 长度: {json.Length} 字符");

            var reparsed = _parser.Parse(json);
            Log($"  重新解析成功: {reparsed.Bands.Count} bands");
        }
        catch (Exception ex)
        {
            Log($"✗ 错误: {ex.Message}");
        }
    }

    // ==================== 按钮 4：导出 PDF ====================
    private void OnExportPdf(object? sender, EventArgs e)
    {
        if (_viewer.TotalPages == 0) { Log("请先加载并渲染模板"); return; }

        try
        {
            var dlg = new SaveFileDialog
            {
                Title = "导出 PDF",
                Filter = "PDF 文件 (*.pdf)|*.pdf",
                FileName = "order-summary.pdf",
                InitialDirectory = AppContext.BaseDirectory
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            var rendered = _renderer.RenderAsync(_currentTemplate!, SampleData()).Result;
            new PdfSharpExporter().ExportToFile(rendered, dlg.FileName);
            var fi = new FileInfo(dlg.FileName);
            Log($"✓ PDF 已导出: {fi.Name} ({fi.Length:N0} 字节)");
        }
        catch (Exception ex)
        {
            Log($"✗ PDF 导出失败: {ex.Message}");
        }
    }

    // ==================== 按钮 5：导出 Excel ====================
    private void OnExportExcel(object? sender, EventArgs e)
    {
        if (_viewer.TotalPages == 0) { Log("请先加载并渲染模板"); return; }

        try
        {
            var dlg = new SaveFileDialog
            {
                Title = "导出 Excel",
                Filter = "Excel 文件 (*.xlsx)|*.xlsx",
                FileName = "order-summary.xlsx",
                InitialDirectory = AppContext.BaseDirectory
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            var rendered = _renderer.RenderAsync(_currentTemplate!, SampleData()).Result;
            new ClosedXmlExporter().ExportToFile(rendered, dlg.FileName);
            var fi = new FileInfo(dlg.FileName);
            Log($"✓ Excel 已导出: {fi.Name} ({fi.Length:N0} 字节)");
        }
        catch (Exception ex)
        {
            Log($"✗ Excel 导出失败: {ex.Message}");
        }
    }

    private void EnableButtons()
    {
        _btnEditTitle.Enabled = true;
        _btnSaveJson.Enabled = true;
        _btnExportPdf.Enabled = true;
        _btnExportExcel.Enabled = true;
    }

    private static Dictionary<string, List<Dictionary<string, object>>> SampleData() =>
        new()
        {
            ["orders"] = new List<Dictionary<string, object>>
            {
                new() { ["id"] = "SO-001", ["customer"] = "Acme Corp",   ["total"] = 1990.00 },
                new() { ["id"] = "SO-002", ["customer"] = "Globex Inc",  ["total"] = 1497.50 },
                new() { ["id"] = "SO-003", ["customer"] = "Initech Ltd", ["total"] =  749.85 }
            }
        };

    private void Log(string msg)
    {
        if (_txtStatus.InvokeRequired)
        {
            _txtStatus.BeginInvoke(new Action(() => AppendLog(msg)));
        }
        else
        {
            AppendLog(msg);
        }
    }

    private void AppendLog(string msg)
    {
        _txtStatus.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}\r\n");
    }
}
