using ReportEngine.Core.Rendering;

namespace ReportEngine.Core.Export;

/// <summary>
/// PDF 导出接口
/// 将渲染后的页面模型输出为 PDF 字节流
/// 桌面端和后端各自实现此接口
/// </summary>
public interface IPdfExporter
{
    /// <summary>
    /// 导出为 PDF 字节数组
    /// </summary>
    byte[] Export(RenderedReport renderedReport);

    /// <summary>
    /// 导出为 PDF 文件
    /// </summary>
    void ExportToFile(RenderedReport renderedReport, string outputPath);
}
