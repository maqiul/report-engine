namespace ReportEngine.WebApi.Models;

/// <summary>
/// 渲染请求
/// </summary>
public class RenderRequest
{
    /// <summary>
    /// 模板 JSON 字符串
    /// </summary>
    public string TemplateJson { get; set; } = "";

    /// <summary>
    /// 数据源：key=数据源名称, value=行数据列表
    /// </summary>
    public Dictionary<string, List<Dictionary<string, object?>>> Data { get; set; } = new();
}

/// <summary>
/// 渲染响应（预览用）
/// </summary>
public class RenderResponse
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// 页面列表（每页包含元素）
    /// </summary>
    public List<PageInfo> Pages { get; set; } = new();

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages => Pages.Count;
}

/// <summary>
/// 页面信息
/// </summary>
public class PageInfo
{
    /// <summary>
    /// 页码（从1开始）
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// 页面宽度（mm）
    /// </summary>
    public double Width { get; set; }

    /// <summary>
    /// 页面高度（mm）
    /// </summary>
    public double Height { get; set; }

    /// <summary>
    /// 渲染元素列表
    /// </summary>
    public List<RenderedElementInfo> Elements { get; set; } = new();
}

/// <summary>
/// 渲染元素信息
/// </summary>
public class RenderedElementInfo
{
    /// <summary>
    /// 元素类型（text, line, rect, image, barcode 等）
    /// </summary>
    public string Type { get; set; } = "";

    /// <summary>
    /// X 坐标（mm）
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Y 坐标（mm）
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// 宽度（mm）
    /// </summary>
    public double Width { get; set; }

    /// <summary>
    /// 高度（mm）
    /// </summary>
    public double Height { get; set; }

    /// <summary>
    /// 文本内容（仅 text 类型）
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// 字体信息
    /// </summary>
    public FontInfo? Font { get; set; }

    /// <summary>
    /// 对齐方式
    /// </summary>
    public string? Alignment { get; set; }

    /// <summary>
    /// 背景色
    /// </summary>
    public string? BackgroundColor { get; set; }

    /// <summary>
    /// 边框颜色
    /// </summary>
    public string? BorderColor { get; set; }

    /// <summary>
    /// 线条宽度
    /// </summary>
    public double? BorderWidth { get; set; }
}

/// <summary>
/// 字体信息
/// </summary>
public class FontInfo
{
    public string Family { get; set; } = "Microsoft YaHei";
    public double Size { get; set; } = 10;
    public bool Bold { get; set; }
    public bool Italic { get; set; }
    public bool Underline { get; set; }
    public string Color { get; set; } = "#000000";
}
