using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using ReportEngine.Core;
using ReportEngine.Core.Parsing;
using ReportEngine.Core.Rendering;
using ReportEngine.Core.SubReports;
using ReportEngine.Export.Excel;
using ReportEngine.Export.Pdf;
using ReportEngine.Viewer.Wpf;

namespace ReportEngine.WpfSample;

/// <summary>
/// ReportEngine WPF 集成示例
///
/// 演示三件事：
///   1. 读取 .rptx 模板（JSON） → ReportTemplate
///   2. 传入数据 → RenderAsync → RenderedReport
///   3. 替换标题/字段 → 序列化回 JSON / 重新渲染
///
/// 模板路径：bin\Templates\order-summary.rptx（csproj 自动 CopyToOutput）
/// </summary>
public partial class MainWindow : Window
{
    private readonly TemplateParser _parser = new();
    private readonly ITemplateResolver _resolver;
    private readonly ReportRenderer _renderer;
    private ReportTemplate? _currentTemplate;

    public MainWindow()
    {
        InitializeComponent();

        // 模板目录（运行时 = bin\Templates\）
        var templatesDir = Path.Combine(AppContext.BaseDirectory, "Templates");
        _resolver = new FileSystemTemplateResolver(templatesDir);
        _renderer = new ReportRenderer(_resolver);

        Loaded += async (_, _) => await LoadAndRenderAsync();
    }

    // ==================== 按钮 1：加载 + 渲染 ====================
    private async Task LoadAndRenderAsync()
    {
        try
        {
            var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "order-summary.rptx");
            Log($"加载模板: {templatePath}");

            // 1. 读取 .rptx（JSON 反序列化为强类型 ReportTemplate）
            _currentTemplate = _parser.ParseFile(templatePath);
            Log($"  解析成功: {_currentTemplate.Bands.Count} bands");

            // 2. 准备数据（替换 {{orders.xxx}}）
            var data = new Dictionary<string, List<Dictionary<string, object>>>
            {
                ["orders"] = new List<Dictionary<string, object>>
                {
                    new() { ["id"] = "SO-001", ["customer"] = "Acme Corp",   ["total"] = 1990.00 },
                    new() { ["id"] = "SO-002", ["customer"] = "Globex Inc",  ["total"] = 1497.50 },
                    new() { ["id"] = "SO-003", ["customer"] = "Initech Ltd", ["total"] =  749.85 }
                }
            };
            Log($"  数据: 3 个订单");

            // 3. 渲染
            var rendered = await _renderer.RenderAsync(_currentTemplate, data);
            Log($"  渲染完成: {rendered.Pages.Count} 页");

            // 4. 喂给 WPF 预览控件
            Viewer.SetReport(rendered);
            Log($"  ✓ 已显示预览");

            // 5. 启用导出按钮（首次渲染成功后）
            BtnExportPdf.IsEnabled = true;
            BtnExportExcel.IsEnabled = true;
        }
        catch (Exception ex)
        {
            Log($"✗ 错误: {ex.Message}");
        }
    }

    private async void BtnLoad_Click(object sender, RoutedEventArgs e)
    {
        await LoadAndRenderAsync();
    }

    // ==================== 按钮 2：动态替换标题 + 重新渲染 ====================
    private async void BtnEditTitle_Click(object sender, RoutedEventArgs e)
    {
        if (_currentTemplate == null)
        {
            Log("请先加载模板");
            return;
        }

        try
        {
            // 找到 title band 的第一个 text 元素
            var titleBand = _currentTemplate.Bands[0];
            var titleElement = titleBand.Elements[0];

            // 替换标题文本（演示动态修改）- 强转 TextElement
            if (titleElement is TextElement textEl)
            {
                textEl.Text = $"订单汇总（{DateTime.Now:HH:mm:ss} 重新生成）";
                Log($"替换标题: {textEl.Text}");
            }
            else
            {
                Log($"✗ 第一个元素不是 TextElement，实际类型: {titleElement.GetType().Name}");
                return;
            }

            // 重新渲染（数据也可同时换）
            var data = new Dictionary<string, List<Dictionary<string, object>>>
            {
                ["orders"] = new List<Dictionary<string, object>>
                {
                    new() { ["id"] = "SO-NEW", ["customer"] = "替换后的客户", ["total"] = 9999.99 }
                }
            };
            var rendered = await _renderer.RenderAsync(_currentTemplate, data);
            Viewer.SetReport(rendered);
            Log($"  ✓ 重新渲染: {rendered.Pages.Count} 页");
        }
        catch (Exception ex)
        {
            Log($"✗ 错误: {ex.Message}");
        }
    }

    // ==================== 按钮 3：保存模板为 JSON ====================
    private void BtnSaveJson_Click(object sender, RoutedEventArgs e)
    {
        if (_currentTemplate == null)
        {
            Log("请先加载模板");
            return;
        }

        try
        {
            // 序列化 ReportTemplate → JSON 字符串
            string json = _parser.Serialize(_currentTemplate);

            var outPath = Path.Combine(AppContext.BaseDirectory, "exported-template.rptx");
            File.WriteAllText(outPath, json);
            Log($"✓ 已保存: {outPath}");
            Log($"  JSON 长度: {json.Length} 字符");

            // 演示重新读取验证
            var reparsed = _parser.Parse(json);
            Log($"  重新解析成功: {reparsed.Bands.Count} bands");
        }
        catch (Exception ex)
        {
            Log($"✗ 错误: {ex.Message}");
        }
    }

    // ==================== 按钮 4：导出 PDF ====================
    private void BtnExportPdf_Click(object sender, RoutedEventArgs e)
    {
        if (Viewer.TotalPages == 0)
        {
            Log("请先加载并渲染模板");
            return;
        }

        try
        {
            // 让用户选择保存路径
            var dlg = new SaveFileDialog
            {
                Title = "导出 PDF",
                Filter = "PDF 文件 (*.pdf)|*.pdf",
                FileName = "order-summary.pdf",
                InitialDirectory = AppContext.BaseDirectory
            };
            if (dlg.ShowDialog(this) != true) return;

            // 重新渲染一次以拿最新数据
            var data = SampleData();
            var rendered = _renderer.RenderAsync(_currentTemplate!, data).Result;

            // 导出
            var exporter = new PdfSharpExporter();
            exporter.ExportToFile(rendered, dlg.FileName);
            var fi = new FileInfo(dlg.FileName);
            Log($"✓ PDF 已导出: {fi.Name} ({fi.Length:N0} 字节)");
        }
        catch (Exception ex)
        {
            Log($"✗ PDF 导出失败: {ex.Message}");
        }
    }

    // ==================== 按钮 5：导出 Excel ====================
    private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
    {
        if (Viewer.TotalPages == 0)
        {
            Log("请先加载并渲染模板");
            return;
        }

        try
        {
            // 让用户选择保存路径
            var dlg = new SaveFileDialog
            {
                Title = "导出 Excel",
                Filter = "Excel 文件 (*.xlsx)|*.xlsx",
                FileName = "order-summary.xlsx",
                InitialDirectory = AppContext.BaseDirectory
            };
            if (dlg.ShowDialog(this) != true) return;

            // 重新渲染一次以拿最新数据
            var data = SampleData();
            var rendered = _renderer.RenderAsync(_currentTemplate!, data).Result;

            // 导出
            var exporter = new ClosedXmlExporter();
            exporter.ExportToFile(rendered, dlg.FileName);
            var fi = new FileInfo(dlg.FileName);
            Log($"✓ Excel 已导出: {fi.Name} ({fi.Length:N0} 字节)");
        }
        catch (Exception ex)
        {
            Log($"✗ Excel 导出失败: {ex.Message}");
        }
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

    // ==================== 翻页 + 缩放 ====================
    private void BtnPrev_Click(object sender, RoutedEventArgs e)
    {
        if (Viewer.TotalPages > 0) Viewer.CurrentPage = Math.Max(0, Viewer.CurrentPage - 1);
        Log($"第 {Viewer.CurrentPage + 1} / {Viewer.TotalPages} 页");
    }

    private void BtnNext_Click(object sender, RoutedEventArgs e)
    {
        if (Viewer.TotalPages > 0) Viewer.CurrentPage = Math.Min(Viewer.TotalPages - 1, Viewer.CurrentPage + 1);
        Log($"第 {Viewer.CurrentPage + 1} / {Viewer.TotalPages} 页");
    }

    private void SliderZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        Viewer.Zoom = e.NewValue;
        TxtZoom.Text = $"{e.NewValue * 100:0}%";
    }

    // ==================== 日志 ====================
    private void Log(string msg)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] {msg}\n";
        Dispatcher.Invoke(() =>
        {
            TxtStatus.Text += line;
        }, DispatcherPriority.Background);
    }
}