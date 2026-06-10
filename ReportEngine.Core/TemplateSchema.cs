using System;
using System.Collections.Generic;

namespace ReportEngine.Core;

/// <summary>
/// 统一的报表模板模型（.rptx 反序列化结果）
/// 此模型是所有端（WinForms / WPF / Web）的契约基础
/// </summary>
public class ReportTemplate
{
    public string Version { get; set; } = "1.0";
    public PageInfo Page { get; set; } = new PageInfo();
    public List<DataSourceDef> DataSources { get; set; } = new List<DataSourceDef>();
    public List<Band> Bands { get; set; } = new List<Band>();
    /// <summary>模板作者</summary>
    public string? Author { get; set; }
    /// <summary>模板描述</summary>
    public string? Description { get; set; }
    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    /// <summary>最后修改时间</summary>
    public DateTime ModifiedAt { get; set; } = DateTime.Now;
    /// <summary>模板参数列表（用于变量替换）</summary>
    public List<TemplateParam> Parameters { get; set; } = new List<TemplateParam>();
}

public class PageInfo
{
    public double Width { get; set; } = 210;   // mm
    public double Height { get; set; } = 297;  // mm
    public string Unit { get; set; } = "mm";
    public string Orientation { get; set; } = "portrait";
    public Margin Margin { get; set; } = new Margin();
    /// <summary>多联打印配置（一张纸平均分成 N 份，每份打印一个模板实例）</summary>
    public MultiUpConfig? MultiUp { get; set; }
    /// <summary>页面背景色</summary>
    public string? BackgroundColor { get; set; }
    /// <summary>页面背景图片路径</summary>
    public string? BackgroundImage { get; set; }
    /// <summary>页面水印文字</summary>
    public string? Watermark { get; set; }
}

public class Margin
{
    public double Top { get; set; } = 10;
    public double Bottom { get; set; } = 10;
    public double Left { get; set; } = 10;
    public double Right { get; set; } = 10;
}

/// <summary>模板参数定义</summary>
public class TemplateParam
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "string"; // string, number, date
    public string DefaultValue { get; set; } = "";
    public string? Label { get; set; } // 显示名称
}

public class DataSourceDef
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "json"; // json | sql | csv | api
    public List<FieldDef> Fields { get; set; } = new List<FieldDef>();
    public string? ConnectionString { get; set; }
    public string? Query { get; set; }
    public override string ToString() => Name + " (" + Type + ", " + Fields.Count + " 字段)";
}

public class FieldDef
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "string"; // string | number | date | boolean
    public string? Format { get; set; }
    public override string ToString() => Name + " [" + Type + "]" + (Format != null ? " (" + Format + ")" : "");
}

public class Band
{
    public BandType Type { get; set; }
    public double Height { get; set; }
    public bool RepeatOnNewPage { get; set; }
    public string? DataSource { get; set; }  // detail / group band 绑定数据源
    public GroupDef? Group { get; set; }     // group band 的分组键
    public List<ReportElement> Elements { get; set; } = new List<ReportElement>();
    /// <summary>多栏打印配置（仅 Detail band 有效）</summary>
    public MultiColumnConfig? MultiColumn { get; set; }
    /// <summary>多层表头：嵌套的子 Band（仅 Header/GroupHeader 有效）</summary>
    public List<Band>? SubBands { get; set; }
}

public enum BandType
{
    Header,       // 页眉（每页顶部）
    Footer,       // 页脚（每页底部）
    ReportHeader, // 报表头（仅第一页顶部）
    ReportFooter, // 报表尾（仅最后一页底部）
    Detail,       // 明细（按数据源行数重复）
    GroupHeader,  // 分组头
    GroupFooter,  // 分组尾
}

public class GroupDef
{
    public string Expression { get; set; } = "";
    public bool KeepTogether { get; set; } = true;
}

/// <summary>
/// 报表元素基类 - 所有可视元素继承此类
/// </summary>
public abstract class ReportElement
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string? Name { get; set; }           // 元素自定义名称
    public string? GroupId { get; set; }        // 分组ID, 同组元素共享
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public string? BackgroundColor { get; set; }
    public BorderDef? Border { get; set; }
    public bool Visible { get; set; } = true;
    public string? VisibleExpression { get; set; }
    /// <summary>锁定元素，防止移动/调整尺寸</summary>
    public bool Locked { get; set; }
    /// <summary>旋转角度（度）</summary>
    public double Rotation { get; set; }
    /// <summary>透明度 0~1</summary>
    public double Opacity { get; set; } = 1.0;
    /// <summary>条件格式规则列表</summary>
    public List<ConditionalFormatRule> ConditionalFormats { get; set; } = new List<ConditionalFormatRule>();
}

public class BorderDef
{
    public double Width { get; set; } = 1;
    public string Color { get; set; } = "#000000";
    public BorderStyle Style { get; set; } = BorderStyle.Solid;
    public bool Top { get; set; }
    public bool Bottom { get; set; }
    public bool Left { get; set; }
    public bool Right { get; set; }
}

public enum BorderStyle { Solid, Dashed, Dotted, None }

/// <summary>条件格式规则</summary>
public class ConditionalFormatRule
{
    public string Expression { get; set; } = ""; // 条件表达式，如: [Amount] > 1000
    public string? BackgroundColor { get; set; } // 满足条件时的背景色
    public string? FontColor { get; set; }       // 满足条件时的字体颜色
    public bool Bold { get; set; }               // 是否加粗
}

// --- 具体元素类型 ---

public class TextElement : ReportElement
{
    public string Text { get; set; } = "";
    public string? DataField { get; set; }         // 字段绑定，如 "CustomerName"
    public string? SummaryFunction { get; set; }   // Sum, Count, Avg, Max, Min
    public string? SummaryField { get; set; }      // 统计字段
    public string? SystemVariable { get; set; }    // PageNumber, TotalPages, PrintDate, PrintTime, ReportTitle
    public string? Format { get; set; }            // "currency", "date", "percent", "number:2"
    public TextAlignment Alignment { get; set; } = TextAlignment.Left;
    public FontDef Font { get; set; } = new FontDef();
    public bool CanGrow { get; set; }
    public bool CanShrink { get; set; }
    public int MaxLines { get; set; }
    public string? Hyperlink { get; set; }

    /// <summary>
    /// 框类型：Static=静态框, Field=字段框, Summary=统计框, SysVar=系统变量框
    /// </summary>
    public TextBoxType BoxType
    {
        get
        {
            if (!string.IsNullOrEmpty(SystemVariable)) return TextBoxType.SysVar;
            if (!string.IsNullOrEmpty(SummaryFunction)) return TextBoxType.Summary;
            if (!string.IsNullOrEmpty(DataField)) return TextBoxType.Field;
            return TextBoxType.Static;
        }
    }
}

public enum TextBoxType { Static, Field, Summary, SysVar }

public enum TextAlignment { Left, Center, Right, Justify }

public class FontDef
{
    public string Family { get; set; } = "SimSun";
    public double Size { get; set; } = 10;
    public bool Bold { get; set; }
    public bool Italic { get; set; }
    public bool Underline { get; set; }
    public string? Color { get; set; }
}

public class ImageElement : ReportElement
{
    public string Source { get; set; } = ""; // 路径、URL、或 base64 data URI
    public ImageSizing Sizing { get; set; } = ImageSizing.FitProportional;
}

public enum ImageSizing { Stretch, FitProportional, Clip, ActualSize }

public class LineElement : ReportElement
{
    public LineDirection Direction { get; set; } = LineDirection.Horizontal;
    public double LineWidth { get; set; } = 1;
    public string LineColor { get; set; } = "#000000";
}

public enum LineDirection { Horizontal, Vertical, Diagonal }

public class ShapeElement : ReportElement
{
    public ShapeType Shape { get; set; } = ShapeType.Rectangle;
    public double BorderRadius { get; set; }
    public string FillColor { get; set; } = "#FFFFFF";
}

public enum ShapeType { Rectangle, Ellipse, RoundedRect, Triangle }

/// <summary>
/// 子报表元素 - 实现模板嵌套的核心
/// </summary>
public class SubReportElement : ReportElement
{
    public string TemplateRef { get; set; } = "";
    public SubReportDataBinding DataBinding { get; set; } = new SubReportDataBinding();
    public string HeightMode { get; set; } = "auto";
    public bool RepeatPerRow { get; set; } = true;
}

public class SubReportDataBinding
{
    public string Source { get; set; } = "";
    public Dictionary<string, string> ParamMap { get; set; } = new Dictionary<string, string>();
}

/// <summary>
/// 图表元素
/// </summary>
public class ChartElement : ReportElement
{
    public ChartType ChartType { get; set; } = ChartType.Bar;
    public string DataSource { get; set; } = "";
    public string CategoryField { get; set; } = "";
    public List<ChartSeries> Series { get; set; } = new List<ChartSeries>();
    public string? Title { get; set; }
}

public enum ChartType { Bar, Line, Pie, Area, Scatter }

public class ChartSeries
{
    public string Name { get; set; } = "";
    public string ValueField { get; set; } = "";
    public string? Color { get; set; }
}

/// <summary>
/// 条形码 / 二维码元素
/// </summary>
public class BarcodeElement : ReportElement
{
    /// <summary>条码内容（支持表达式，如 {{currentRow.orderNo}}）</summary>
    public string Value { get; set; } = "";
    /// <summary>条码类型</summary>
    public BarcodeFormat Format { get; set; } = BarcodeFormat.QRCode;
    /// <summary>前景色</summary>
    public string ForeColor { get; set; } = "#000000";
    /// <summary>背景色</summary>
    public string BackColor { get; set; } = "#FFFFFF";
    /// <summary>是否显示条码下方文字（仅一维码有效）</summary>
    public bool ShowText { get; set; } = true;
}

public enum BarcodeFormat
{
    Code128,
    Code39,
    EAN13,
    EAN8,
    UPC_A,
    QRCode,
    DataMatrix,
    PDF417,
}

/// <summary>
/// 表格元素 — rows×cols 网格布局，支持单元格合并
/// </summary>
public class TableElement : ReportElement
{
    public int RowCount { get; set; } = 3;
    public int ColCount { get; set; } = 3;
    /// <summary>列宽度（mm），长度应等于 ColCount；若为空则平分</summary>
    public List<double> ColumnWidths { get; set; } = new List<double>();
    /// <summary>行高（mm），长度应等于 RowCount；若为空则平分</summary>
    public List<double> RowHeights { get; set; } = new List<double>();
    /// <summary>单元格列表（按行优先）</summary>
    public List<TableCell> Cells { get; set; } = new List<TableCell>();
    /// <summary>边框宽度 mm</summary>
    public double BorderWidth { get; set; } = 0.3;
    /// <summary>边框颜色</summary>
    public string BorderColor { get; set; } = "#000000";
}

public class TableCell
{
    public int Row { get; set; }
    public int Col { get; set; }
    /// <summary>合并行数（1=不合并）</summary>
    public int RowSpan { get; set; } = 1;
    /// <summary>合并列数（1=不合并）</summary>
    public int ColSpan { get; set; } = 1;
    /// <summary>单元格文本（支持表达式）</summary>
    public string Text { get; set; } = "";
    public FontDef Font { get; set; } = new FontDef();
    public TextAlignment Alignment { get; set; } = TextAlignment.Center;
    public string? BackgroundColor { get; set; }
}

/// <summary>
/// 交叉表元素 — 行/列字段动态展开，交叉区显示汇总值
/// 类似 Excel 数据透视表
/// </summary>
public class CrossTabElement : ReportElement
{
    /// <summary>数据源名称</summary>
    public string DataSource { get; set; } = "";
    /// <summary>行字段（分组键），支持多层</summary>
    public List<string> RowFields { get; set; } = new List<string>();
    /// <summary>列字段（横向展开键），支持多层</summary>
    public List<string> ColumnFields { get; set; } = new List<string>();
    /// <summary>值字段与聚合方式</summary>
    public List<CrossTabMeasure> Measures { get; set; } = new List<CrossTabMeasure>();
    /// <summary>是否显示行合计</summary>
    public bool ShowRowTotal { get; set; } = true;
    /// <summary>是否显示列合计</summary>
    public bool ShowColumnTotal { get; set; } = true;
    /// <summary>单元格字体</summary>
    public FontDef CellFont { get; set; } = new FontDef();
    /// <summary>表头字体</summary>
    public FontDef HeaderFont { get; set; } = new FontDef { Bold = true };
    /// <summary>单元格内边距 mm</summary>
    public double CellPadding { get; set; } = 1.0;
    /// <summary>边框宽度 mm</summary>
    public double BorderWidth { get; set; } = 0.3;
    /// <summary>边框颜色</summary>
    public string BorderColor { get; set; } = "#000000";
}

public class CrossTabMeasure
{
    /// <summary>值字段名</summary>
    public string Field { get; set; } = "";
    /// <summary>聚合函数：Sum / Count / Avg / Min / Max</summary>
    public string Aggregate { get; set; } = "Sum";
    /// <summary>显示格式</summary>
    public string? Format { get; set; }
    /// <summary>列标题（为空时用 Field 名）</summary>
    public string? Label { get; set; }
}

/// <summary>
/// 多栏打印配置 — 用于标签/条码批量打印
/// 附加在 Band 上的配置，不是独立元素
/// </summary>
public class MultiColumnConfig
{
    /// <summary>栏数</summary>
    public int ColumnCount { get; set; } = 2;
    /// <summary>栏间距 mm</summary>
    public double ColumnSpacing { get; set; } = 5;
    /// <summary>排列方向：Horizontal=先行后列，Vertical=先列后行</summary>
    public string Direction { get; set; } = "Horizontal";
}

/// <summary>
/// 多联打印配置 — 将一张纸等分为 Rows×Columns 份
/// 用户在 1/N 区域内设计，渲染时平铺到整页各位置
/// </summary>
public class MultiUpConfig
{
    /// <summary>行数（垂直方向拼几份）</summary>
    public int Rows { get; set; } = 2;
    /// <summary>列数（水平方向拼几份）</summary>
    public int Columns { get; set; } = 2;
    /// <summary>水平间距 mm</summary>
    public double HSpacing { get; set; } = 0;
    /// <summary>垂直间距 mm</summary>
    public double VSpacing { get; set; } = 0;
    /// <summary>排列方向：Horizontal=先行后列（Z字形），Vertical=先列后行</summary>
    public string Direction { get; set; } = "Horizontal";
    /// <summary>每页份数（N×M）</summary>
    public int Count => Rows * Columns;
}
