using ReportEngine.Core.Rendering;

namespace ReportEngine.Core.Export;

/// <summary>
/// Excel 导出接口
/// 将渲染后的页面模型 / 数据源输出为 .xlsx 字节流
/// </summary>
public interface IExcelExporter
{
    /// <summary>
    /// 导出为 Excel 字节数组
    /// </summary>
    byte[] Export(RenderedReport renderedReport);

    /// <summary>
    /// 导出为 Excel 文件
    /// </summary>
    void ExportToFile(RenderedReport renderedReport, string outputPath);
}
