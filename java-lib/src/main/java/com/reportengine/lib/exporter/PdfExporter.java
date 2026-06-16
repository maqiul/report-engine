package com.reportengine.lib.exporter;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.itextpdf.kernel.pdf.PdfDocument;
import com.itextpdf.kernel.pdf.PdfWriter;
import com.itextpdf.layout.Document;
import com.itextpdf.layout.element.Cell;
import com.itextpdf.layout.element.Paragraph;
import com.itextpdf.layout.element.Table;
import com.reportengine.lib.model.RenderRequest;

import java.io.ByteArrayOutputStream;
import java.util.List;
import java.util.Map;

/**
 * PDF 导出器
 * 
 * 使用示例：
 * <pre>
 * PdfExporter exporter = new PdfExporter();
 * byte[] pdfBytes = exporter.export(request);
 * </pre>
 */
public class PdfExporter {

    private final ObjectMapper objectMapper = new ObjectMapper();

    /**
     * 导出 PDF
     */
    public byte[] export(RenderRequest request) throws Exception {
        JsonNode template = objectMapper.readTree(request.getTemplateJson());
        Map<String, List<Map<String, Object>>> data = request.getData();
        
        ByteArrayOutputStream baos = new ByteArrayOutputStream();
        PdfWriter writer = new PdfWriter(baos);
        PdfDocument pdf = new PdfDocument(writer);
        Document document = new Document(pdf);
        
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
        
        document.add(new Paragraph(title).setFontSize(18).setBold());
        document.add(new Paragraph(" "));
        
        // 查找 detail band 并输出数据
        if (template.has("bands")) {
            for (JsonNode band : template.get("bands")) {
                if ("detail".equals(band.get("type").asText()) && band.has("dataSource")) {
                    String dsName = band.get("dataSource").asText();
                    List<Map<String, Object>> rows = data.getOrDefault(dsName, new java.util.ArrayList<>());
                    
                    if (!rows.isEmpty()) {
                        int colCount = rows.get(0).size();
                        Table table = new Table(colCount);
                        table.useAllAvailableWidth();
                        
                        // 表头
                        for (String key : rows.get(0).keySet()) {
                            table.addHeaderCell(new Cell().add(new Paragraph(key).setBold()));
                        }
                        
                        // 数据行
                        for (Map<String, Object> row : rows) {
                            for (Object value : row.values()) {
                                table.addCell(new Cell().add(new Paragraph(
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
}
