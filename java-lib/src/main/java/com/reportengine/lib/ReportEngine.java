package com.reportengine.lib;

import com.reportengine.lib.exporter.ExcelExporter;
import com.reportengine.lib.exporter.PdfExporter;
import com.reportengine.lib.model.RenderRequest;
import com.reportengine.lib.model.RenderResponse;
import com.reportengine.lib.renderer.ReportRenderer;

/**
 * ReportEngine 门面（统一入口）
 *
 * 把渲染器 + 两个导出器装到一个 Bean 里，外部调用方只需要：
 * <pre>{@code
 *   @Autowired
 *   ReportEngine engine;
 *
 *   RenderResponse resp = engine.render(request);
 *   byte[] pdf = engine.exportPdf(request);
 *   byte[] excel = engine.exportExcel(request);
 * }</pre>
 *
 * 线程安全。所有内部组件都是无状态 Bean。
 *
 * @since 0.3.0
 */
public class ReportEngine {

    private final ReportRenderer renderer;
    private final PdfExporter pdfExporter;
    private final ExcelExporter excelExporter;

    public ReportEngine() {
        this(new ReportRenderer(), new PdfExporter(), new ExcelExporter());
    }

    public ReportEngine(ReportRenderer renderer, PdfExporter pdfExporter, ExcelExporter excelExporter) {
        this.renderer = renderer;
        this.pdfExporter = pdfExporter;
        this.excelExporter = excelExporter;
    }

    /**
     * 渲染模板为中间结构（RenderResponse），可序列化为 JSON 给前端预览。
     */
    public RenderResponse render(RenderRequest request) {
        return renderer.render(request);
    }

    /**
     * 直接渲染并导出为 PDF。
     */
    public byte[] exportPdf(RenderRequest request) throws Exception {
        return pdfExporter.export(request);
    }

    /**
     * 直接渲染并导出为 Excel (.xlsx)。
     */
    public byte[] exportExcel(RenderRequest request) throws Exception {
        return excelExporter.export(request);
    }
}