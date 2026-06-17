package com.reportengine.web.controller;

import com.reportengine.lib.exporter.ExcelExporter;
import com.reportengine.lib.exporter.PdfExporter;
import com.reportengine.lib.model.RenderRequest;
import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.tags.Tag;
import org.springframework.http.HttpHeaders;
import org.springframework.http.MediaType;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

/**
 * 导出控制器 - 提供 PDF/Excel 导出 API
 */
@RestController
@RequestMapping("/api/export")
@CrossOrigin(origins = {"http://localhost:5173", "http://localhost:3000", "http://127.0.0.1:3000"})
@Tag(name = "报表导出", description = "将模板渲染为 PDF 或 Excel 二进制流")
public class ExportController {

    private final PdfExporter pdfExporter = new PdfExporter();
    private final ExcelExporter excelExporter = new ExcelExporter();

    /**
     * 导出 PDF
     */
    @PostMapping(value = "/pdf", produces = MediaType.APPLICATION_PDF_VALUE)
    @Operation(
        summary = "导出 PDF",
        description = "将报表模板渲染为 PDF 字节流。模板内 {{page}} 表示当前页号，{{totalPages}} 表示总页数。"
    )
    public ResponseEntity<byte[]> exportPdf(@RequestBody RenderRequest request) {
        try {
            byte[] pdfBytes = pdfExporter.export(request);

            HttpHeaders headers = new HttpHeaders();
            headers.setContentType(MediaType.APPLICATION_PDF);
            headers.setContentDispositionFormData("attachment", "report.pdf");

            return ResponseEntity.ok().headers(headers).body(pdfBytes);
        } catch (Exception e) {
            e.printStackTrace();
            return ResponseEntity.badRequest().body(("Error: " + e.getMessage()).getBytes());
        }
    }

    /**
     * 导出 Excel
     */
    @PostMapping(value = "/excel",
        produces = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
    @Operation(
        summary = "导出 Excel",
        description = "将报表模板渲染为 xlsx 字节流。支持精确布局、打印设置、页脚页码、字体配置。"
    )
    public ResponseEntity<byte[]> exportExcel(@RequestBody RenderRequest request) {
        try {
            byte[] excelBytes = excelExporter.export(request);

            HttpHeaders headers = new HttpHeaders();
            headers.setContentType(MediaType.parseMediaType("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
            headers.setContentDispositionFormData("attachment", "report.xlsx");

            return ResponseEntity.ok().headers(headers).body(excelBytes);
        } catch (Exception e) {
            e.printStackTrace();
            return ResponseEntity.badRequest().body(("Error: " + e.getMessage()).getBytes());
        }
    }
}
