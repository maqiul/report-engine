using System;
using ReportEngine.Core;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 设计器 UI 层用的枚举 ↔ 中文显示名 ↔ 字符串 双向映射。
/// 从 MainWindow 抽出的纯静态无副作用工具。
/// 这些都是 switch-on-enum 表达式，编译为静态表，零开销。
/// </summary>
internal static class EnumCnMap
{
    // ── 元素类型 → 中文 ─────────────────────────────
    public static string ElementTypeName(ReportElement el) => el switch
    {
        TextElement => "文本",
        LineElement => "线条",
        ImageElement => "图像",
        ShapeElement => "形状",
        SubReportElement => "子报表",
        BarcodeElement => "条码/二维码",
        TableElement => "表格",
        CrossTabElement => "交叉表",
        ChartElement => "图表",
        _ => el.GetType().Name,
    };

    // ── 文本对齐 ─────────────────────────────
    public static string AlignToCN(string v) => v switch
    {
        "Left" => "左对齐",
        "Center" => "居中",
        "Right" => "右对齐",
        "Justify" => "两端对齐",
        _ => v,
    };
    public static TextAlignment CNToAlign(string v) => v switch
    {
        "居中" => TextAlignment.Center,
        "右对齐" => TextAlignment.Right,
        "两端对齐" => TextAlignment.Justify,
        _ => TextAlignment.Left,
    };

    // ── 线段方向 ─────────────────────────────
    public static string DirToCN(string v) => v switch
    {
        "Horizontal" => "水平",
        "Vertical" => "垂直",
        "Diagonal" => "对角线",
        _ => v,
    };
    public static LineDirection CNToDir(string v) => v switch
    {
        "垂直" => LineDirection.Vertical,
        "对角线" => LineDirection.Diagonal,
        _ => LineDirection.Horizontal,
    };

    // ── 形状类型 ─────────────────────────────
    public static string ShapeToCN(string v) => v switch
    {
        "Rectangle" => "矩形",
        "Ellipse" => "椭圆",
        "RoundedRect" => "圆角矩形",
        "Triangle" => "三角形",
        _ => v,
    };
    public static ShapeType CNToShape(string v) => v switch
    {
        "椭圆" => ShapeType.Ellipse,
        "圆角矩形" => ShapeType.RoundedRect,
        "三角形" => ShapeType.Triangle,
        _ => ShapeType.Rectangle,
    };

    // ── 图像缩放 ─────────────────────────────
    public static string SizingToCN(string v) => v switch
    {
        "Stretch" => "拉伸",
        "FitProportional" => "等比缩放",
        "Clip" => "裁剪",
        "ActualSize" => "原始尺寸",
        _ => v,
    };
    public static ImageSizing CNToSizing(string v) => v switch
    {
        "拉伸" => ImageSizing.Stretch,
        "裁剪" => ImageSizing.Clip,
        "原始尺寸" => ImageSizing.ActualSize,
        _ => ImageSizing.FitProportional,
    };

    // ── 文本框类型 ─────────────────────────────
    public static string BoxTypeToCN(TextBoxType t) => t switch
    {
        TextBoxType.Field => "字段框",
        TextBoxType.Summary => "统计框",
        TextBoxType.SysVar => "系统变量框",
        _ => "静态框",
    };

    /// <summary>画布上文本元素的简短标签（字段名 / Sum(field) / 系统变量 / 静态文本）</summary>
    public static string GetTextElLabel(TextElement t) => t.BoxType switch
    {
        TextBoxType.Field => t.DataField ?? "",
        TextBoxType.Summary => t.SummaryFunction + "(" + (t.SummaryField ?? "") + ")",
        TextBoxType.SysVar => t.SystemVariable ?? "",
        _ => t.Text is { Length: > 12 } s ? s.Substring(0, 12) + "…" : t.Text ?? "",
    };

    // ── 条码格式（保留原文案 + 二维码特殊处理） ────────
    public static string BcFmtToCN(string v) => v == "QRCode" ? "二维码(QR)" : v;
    public static BarcodeFormat CNToBcFmt(string v) => v switch
    {
        "二维码(QR)" => BarcodeFormat.QRCode,
        _ => Enum.TryParse<BarcodeFormat>(v, out var f) ? f : BarcodeFormat.QRCode,
    };

    // ── 图表类型 ─────────────────────────────
    public static string ChartTypeCN(ChartType t) => t switch
    {
        ChartType.Bar => "柱状图",
        ChartType.Line => "折线图",
        ChartType.Pie => "饼图",
        ChartType.Area => "面积图",
        ChartType.Scatter => "散点图",
        _ => t.ToString(),
    };
    public static ChartType CNToChartType(string v) => v switch
    {
        "折线图" => ChartType.Line,
        "饼图" => ChartType.Pie,
        "面积图" => ChartType.Area,
        "散点图" => ChartType.Scatter,
        _ => ChartType.Bar,
    };
}
