using System;
using System.Windows;
using ReportEngine.Core;
using static ReportEngine.Designer.Wpf.BandHitTester;
using static ReportEngine.Designer.Wpf.ElementFactory;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 画布拖放处理 - 把 OnCanvasDrop 从 MainWindow 抽离。
/// 等价抽离自 MainWindow.OnCanvasDrop() (37 行)。
///
/// 通过 typeName (从 DataObject 取) 决定 NewXxx 工厂产出元素，
/// 通过 BandHitTester 找 drop 位置对应 Band，
/// 通过 onInsert 回调传出"插入元素"副作用 (调 MainWindow.InsertElementAt)。
/// </summary>
internal static class CanvasDropProcessor
{
    /// <summary>
    /// 处理画布 drop 事件。
    /// </summary>
    /// <param name="e">DragEventArgs (用于 GetData 和 GetPosition)</param>
    /// <param name="template">当前模板 (null check)</param>
    /// <param name="canvas">画布元素 (GetPosition 取坐标)</param>
    /// <param name="zoom">当前缩放 (用于坐标转换)</param>
    /// <param name="canvasPadding">画布内边距 (mm)</param>
    /// <param name="pixelsPerMm">每毫米像素数</param>
    /// <param name="onInsert">插入元素的回调 (element, targetBand, relX, relY)</param>
    /// <returns>成功 drop 的元素类型名称，null 表示未成功</returns>
    public static string? Process(
        DragEventArgs e,
        ReportTemplate? template,
        UIElement canvas,
        double zoom,
        double canvasPadding,
        double pixelsPerMm,
        Action<ReportElement, Band, double, double> onInsert)
    {
        if (template == null || !e.Data.GetDataPresent("ElementType")) return null;
        var typeName = e.Data.GetData("ElementType") as string;
        if (typeName == null) return null;

        var pos = e.GetPosition(canvas);
        double mmPx = pixelsPerMm * zoom;

        // 确定drop位置对应的Band
        var (targetBand, relY) = FindBandAtY(template, pos.X, pos.Y, zoom, canvasPadding, pixelsPerMm);
        if (targetBand == null) return null;

        double relX = (pos.X - canvasPadding) / mmPx;

        ReportElement? newEl = typeName switch
        {
            "Text" => NewText(),
            "Field" => NewFieldBox(),
            "Summary" => NewSummaryBox(),
            "SysVar" => NewSysVarBox(),
            "Line" => NewLine(),
            "Shape" => NewShape(),
            "Image" => NewImage(),
            "Barcode" => NewBarcode(),
            "Table" => NewTable(),
            "CrossTab" => NewCrossTab(),
            "Chart" => NewChart(),
            "SubReport" => NewSubReport(),
            _ => null
        };
        if (newEl == null) return null;
        onInsert(newEl, targetBand, relX, relY);
        return typeName;
    }
}