using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using ReportEngine.Core;
using ReportEngine.Core.Rendering;
using ReportEngine.Core.SubReports;
using ReportEngine.Export.Excel;
using ReportEngine.Export.Pdf;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 报表导出器抽象接口 - 统一 PdfSharpExporter / ClosedXmlExporter 调用接口。
/// </summary>
internal interface IReportFileExporter
{
    void ExportToFile(RenderedReport report, string path);
}

internal sealed class PdfSharpExporterAdapter : IReportFileExporter
{
    private readonly PdfSharpExporter _inner = new();
    public void ExportToFile(RenderedReport report, string path) => _inner.ExportToFile(report, path);
}

internal sealed class ClosedXmlExporterAdapter : IReportFileExporter
{
    private readonly ClosedXmlExporter _inner = new();
    public void ExportToFile(RenderedReport report, string path) => _inner.ExportToFile(report, path);
}

/// <summary>
/// 报表文件导出 (PDF/Excel 共用) - 把 ExportPdf/ExportExcel 从 MainWindow 抽离。
/// 等价抽离自 MainWindow.ExportPdf/ExportExcel (各 38 行)。
///
/// 行为:
///   1. 弹 MessageBox 预览确认 (Yes=继续, No=取消)
///   2. 弹 SaveFileDialog 选择保存路径
///   3. RenderAsync(template, data) -> exporter.ExportToFile(rendered, path)
///   4. 失败: MessageBox 错误提示
///   5. 成功: 触发 onSuccess 写回 MainWindow 状态 (_statusText)
/// </summary>
internal static class ReportFileExporter
{
    public static Task ExportPdfAsync(
        ReportTemplate? template,
        IReadOnlyDictionary<string, object>? previewData,
        string? currentFilePath,
        Action<string> onSuccess)
    {
        return ExportCoreAsync(
            template, previewData, currentFilePath,
            formatName: "PDF",
            fileFilter: "PDF 文件 (*.pdf)|*.pdf",
            fileExtension: ".pdf",
            dialogTitle: "导出PDF",
            exporter: new PdfSharpExporterAdapter(),
            onSuccess: onSuccess);
    }

    public static Task ExportExcelAsync(
        ReportTemplate? template,
        IReadOnlyDictionary<string, object>? previewData,
        string? currentFilePath,
        Action<string> onSuccess)
    {
        return ExportCoreAsync(
            template, previewData, currentFilePath,
            formatName: "Excel",
            fileFilter: "Excel 文件 (*.xlsx)|*.xlsx",
            fileExtension: ".xlsx",
            dialogTitle: "导出Excel",
            exporter: new ClosedXmlExporterAdapter(),
            onSuccess: onSuccess);
    }

    private static async Task ExportCoreAsync(
        ReportTemplate? template,
        IReadOnlyDictionary<string, object>? previewData,
        string? currentFilePath,
        string formatName,
        string fileFilter,
        string fileExtension,
        string dialogTitle,
        IReportFileExporter exporter,
        Action<string> onSuccess)
    {
        if (template == null) return;

        // 1. 预览确认
        var previewResult = MessageBox.Show(
            "导出前预览:\n" +
            (previewData != null && previewData.Count > 0 ? $"数据源: {previewData.Count} 条记录" : "无预览数据，将导出空模板") + "\n\n是否继续导出？",
            "导出确认",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        if (previewResult != MessageBoxResult.Yes) return;

        // 2. 选文件
        var dlg = new SaveFileDialog
        {
            Filter = fileFilter,
            Title = dialogTitle,
            FileName = (currentFilePath != null ? Path.GetFileNameWithoutExtension(currentFilePath) : "报表") + fileExtension,
        };
        if (dlg.ShowDialog() != true) return;

        // 3. 渲染 + 导出
        try
        {
            var resolver = new FileSystemTemplateResolver(Path.GetDirectoryName(currentFilePath) ?? ".");
            var renderer = new ReportRenderer(resolver);
            var data = ExportDataBuilder.Build(previewData);
            var rendered = await renderer.RenderAsync(template, data);
            exporter.ExportToFile(rendered, dlg.FileName);
            onSuccess(dlg.FileName);
            MessageBox.Show($"{formatName} 导出完成！\n{dlg.FileName}", "导出成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"{formatName}导出失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
