package com.reportengine.web.controller;

import tools.jackson.databind.JsonNode;
import tools.jackson.databind.ObjectMapper;
import com.reportengine.web.model.RenderRequest;
import org.springframework.http.HttpHeaders;
import org.springframework.http.MediaType;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.io.ByteArrayOutputStream;
import java.util.List;
import java.util.Map;

/**
 * 导出控制器 - 提供 PDF/Excel 导出 API
 * 
 * 注意：这是一个简化实现，完整功能需要：
 * 1. 完整的模板解析引擎
 * 2. 布局计算和分页
 * 3. 字体嵌入
 * 4. 条码生成
 * 等复杂功能
 */
@RestController
@RequestMapping("/api/export")
@CrossOrigin(origins = {"http://localhost:5173", "http://localhost:3000", "http://127.0.0.1:3000"})
public class ExportController {

    private final ObjectMapper objectMapper = new ObjectMapper();

    /**
     * 导出 PDF
     */
    @PostMapping("/pdf")
    public ResponseEntity<byte[]> exportPdf(@RequestBody RenderRequest request) {
        try {
            // 解析模板
            JsonNode template = objectMapper.readTree(request.getTemplateJson());
            Map<String, List<Map<String, Object>>> data = request.getData();
            
            // 生成 PDF（简化实现）
            byte[] pdfBytes = generateSimplePdf(template, data);
            
            HttpHeaders headers = new HttpHeaders();
            headers.setContentType(MediaType.APPLICATION_PDF);
            headers.setContentDispositionFormData("attachment", "report.pdf");
            
            return ResponseEntity.ok()
                    .headers(headers)
                    .body(pdfBytes);
                    
        } catch (Exception e) {
            return ResponseEntity.badRequest().build();
        }
    }

    /**
     * 导出 Excel
     */
    @PostMapping("/excel")
    public ResponseEntity<byte[]> exportExcel(@RequestBody RenderRequest request) {
        try {
            // 解析模板
            JsonNode template = objectMapper.readTree(request.getTemplateJson());
            Map<String, List<Map<String, Object>>> data = request.getData();
            
            // 生成 Excel（简化实现）
            byte[] excelBytes = generateSimpleExcel(template, data);
            
            HttpHeaders headers = new HttpHeaders();
            headers.setContentType(MediaType.parseMediaType("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
            headers.setContentDispositionFormData("attachment", "report.xlsx");
            
            return ResponseEntity.ok()
                    .headers(headers)
                    .body(excelBytes);
                    
        } catch (Exception e) {
            return ResponseEntity.badRequest().build();
        }
    }

    /**
     * 生成简单 PDF（示例实现）
     * 实际项目中应使用完整的渲染引擎
     */
    private byte[] generateSimplePdf(JsonNode template, Map<String, List<Map<String, Object>>> data) throws Exception {
        ByteArrayOutputStream baos = new ByteArrayOutputStream();
        
        // 使用 iText 7 生成 PDF
        // 注意：这里是一个简化示例，实际应实现完整的模板渲染
        com.itextpdf.kernel.pdf.PdfWriter writer = new com.itextpdf.kernel.pdf.PdfWriter(baos);
        com.itextpdf.kernel.pdf.PdfDocument pdf = new com.itextpdf.kernel.pdf.PdfDocument(writer);
        com.itextpdf.layout.Document document = new com.itextpdf.layout.Document(pdf);
        
        // 添加标题
        String title = "ReportEngine 报表";
        if (template.has("bands")) {
            for (JsonNode band : template.get("bands")) {
                if ("reportHeader".equals(band.get("type").asText()) && band.has("elements")) {
                    for (JsonNode el : band.get("elements")) {
                        if ("text".equals(el.get("type").asText()) && el.has("text")) {
                            title = el.get("text").asText();
                            break;
                        }
                    }
                    break;
                }
            }
        }
        
        document.add(new com.itextpdf.layout.element.Paragraph(title)
                .setFontSize(18)
                .setBold());
        document.add(new com.itextpdf.layout.element.Paragraph(" "));
        
        // 查找 detail band 并输出数据
        if (template.has("bands")) {
            for (JsonNode band : template.get("bands")) {
                if ("detail".equals(band.get("type").asText()) && band.has("dataSource")) {
                    String dsName = band.get("dataSource").asText();
                    List<Map<String, Object>> rows = data.getOrDefault(dsName, new java.util.ArrayList<>());
                    
                    // 创建表格
                    if (!rows.isEmpty()) {
                        int colCount = rows.get(0).size();
                        com.itextpdf.layout.element.Table table = new com.itextpdf.layout.element.Table(colCount);
                        table.useAllAvailableWidth();
                        
                        // 表头
                        for (String key : rows.get(0).keySet()) {
                            table.addHeaderCell(new com.itextpdf.layout.element.Cell()
                                    .add(new com.itextpdf.layout.element.Paragraph(key).setBold()));
                        }
                        
                        // 数据行
                        for (Map<String, Object> row : rows) {
                            for (Object value : row.values()) {
                                table.addCell(new com.itextpdf.layout.element.Cell()
                                        .add(new com.itextpdf.layout.element.Paragraph(
                                                value != null ? value.toString() : "")));
                            }
                        }
                        
                        document.add(table);
                    }
                    break;
                }
            }
        }
        
        document.close();
        return baos.toByteArray();
    }

    /**
     * 生成简单 Excel（示例实现）
     * 实际项目中应使用完整的渲染引擎
     */
    private byte[] generateSimpleExcel(JsonNode template, Map<String, List<Map<String, Object>>> data) throws Exception {
        ByteArrayOutputStream baos = new ByteArrayOutputStream();
        
        // 使用 Apache POI 生成 Excel
        org.apache.poi.xssf.usermodel.XSSFWorkbook workbook = new org.apache.poi.xssf.usermodel.XSSFWorkbook();
        
        // 查找 detail band 并输出数据
        if (template.has("bands")) {
            for (JsonNode band : template.get("bands")) {
                if ("detail".equals(band.get("type").asText()) && band.has("dataSource")) {
                    String dsName = band.get("dataSource").asText();
                    List<Map<String, Object>> rows = data.getOrDefault(dsName, new java.util.ArrayList<>());
                    
                    if (!rows.isEmpty()) {
                        org.apache.poi.xssf.usermodel.XSSFSheet sheet = workbook.createSheet("Report");
                        
                        // 表头样式
                        org.apache.poi.xssf.usermodel.XSSFCellStyle headerStyle = workbook.createCellStyle();
                        org.apache.poi.xssf.usermodel.XSSFFont headerFont = workbook.createFont();
                        headerFont.setBold(true);
                        headerStyle.setFont(headerFont);
                        
                        // 表头
                        org.apache.poi.xssf.usermodel.XSSFRow headerRow = sheet.createRow(0);
                        int colIdx = 0;
                        for (String key : rows.get(0).keySet()) {
                            org.apache.poi.xssf.usermodel.XSSFCell cell = headerRow.createCell(colIdx++);
                            cell.setCellValue(key);
                            cell.setCellStyle(headerStyle);
                        }
                        
                        // 数据行
                        int rowIdx = 1;
                        for (Map<String, Object> rowData : rows) {
                            org.apache.poi.xssf.usermodel.XSSFRow row = sheet.createRow(rowIdx++);
                            colIdx = 0;
                            for (Object value : rowData.values()) {
                                org.apache.poi.xssf.usermodel.XSSFCell cell = row.createCell(colIdx++);
                                if (value instanceof Number) {
                                    cell.setCellValue(((Number) value).doubleValue());
                                } else {
                                    cell.setCellValue(value != null ? value.toString() : "");
                                }
                            }
                        }
                        
                        // 自动列宽
                        for (int i = 0; i < rows.get(0).size(); i++) {
                            sheet.autoSizeColumn(i);
                        }
                    }
                    break;
                }
            }
        }
        
        // 如果没有 detail band，创建一个空 sheet
        if (workbook.getNumberOfSheets() == 0) {
            workbook.createSheet("Report");
        }
        
        workbook.write(baos);
        workbook.close();
        return baos.toByteArray();
    }
}
