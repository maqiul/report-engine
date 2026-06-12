using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 画布 PNG 导出 - 把 ExportPng 从 MainWindow 抽离。
/// 等价抽离自 MainWindow.ExportPng() (41 行)。
///
/// 行为:
///   1. 弹 SaveFileDialog 选 PNG 路径
///   2. 调 renderAtZoom(1.0) 临时以 100% 渲染
///   3. 用 RenderTargetBitmap 抓 canvas 内容
///   4. PngBitmapEncoder 编码保存
///   5. 调 renderAtZoom(oldZoom) 恢复
///   6. 触发 onSuccess 写回 MainWindow 状态 (_statusText)
/// </summary>
internal static class CanvasImageExporter
{
    public static void Export(
        ReportTemplate? template,
        double pixelsPerMm,
        double canvasPadding,
        UIElement canvas,
        string? currentFilePath,
        Action renderAt100, // 临时以 100% 缩放渲染
        Action renderAtCurrent, // 恢复原缩放渲染
        Action<string> onSuccess)
    {
        if (template == null) return;
        var dlg = new SaveFileDialog
        {
            Filter = "PNG 图片 (*.png)|*.png",
            Title = "导出图片",
            FileName = (currentFilePath != null ? Path.GetFileNameWithoutExtension(currentFilePath) : "报表") + ".png",
        };
        if (dlg.ShowDialog() != true) return;
        try
        {
            // 临时渲染到100%缩放以保证清晰度
            renderAt100();

            // 计算画布实际大小
            var (bmpW, bmpH) = CanvasSizeCalculator.ComputePixelSize(template, pixelsPerMm, canvasPadding);
            var bmp = new RenderTargetBitmap(bmpW, bmpH, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(canvas);

            // 保存为PNG
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            using (var stream = File.Create(dlg.FileName))
                encoder.Save(stream);

            // 恢复 zoom
            renderAtCurrent();
            onSuccess(dlg.FileName);
            MessageBox.Show("PNG 导出完成！\n" + dlg.FileName, "导出成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("PNG导出失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
