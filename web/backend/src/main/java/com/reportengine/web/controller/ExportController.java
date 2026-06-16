package com.reportengine.web.controller;

import com.reportengine.lib.exporter.ExcelExporter;
import com.reportengine.lib.exporter.PdfExporter;
import com.reportengine.lib.model.RenderRequest;
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
public class ExportController {

    private final PdfExporter pdfExporter = new PdfExporter();
    private final ExcelExporter excelExporter = new ExcelExporter();

    /**
     * 导出 PDF
     */
    @PostMapping("/pdf")
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
    @PostMapping("/excel")
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
