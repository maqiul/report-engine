package com.reportengine.lib.exporter;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.itextpdf.kernel.font.PdfFont;
import com.itextpdf.kernel.font.PdfFontFactory;
import com.itextpdf.kernel.pdf.PdfDocument;
import com.itextpdf.kernel.pdf.PdfWriter;
import com.itextpdf.layout.Document;
import com.itextpdf.layout.element.Cell;
import com.itextpdf.layout.element.Paragraph;
import com.itextpdf.layout.element.Table;
import com.itextpdf.layout.properties.TextAlignment;
import com.reportengine.lib.model.RenderRequest;

import java.io.ByteArrayOutputStream;
import java.util.ArrayList;
import java.util.HashMap;
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
        
        // 注册中文字体 - 使用系统字体文件
        PdfFont chineseFont = PdfFontFactory.createFont("C:/Windows/Fonts/STSONG.TTF", PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);
        
        // 查找 reportHeader band 获取标题
        String title = "报表";
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
        
        // 添加标题
        document.add(new Paragraph(title)
            .setFont(chineseFont)
            .setFontSize(18)
            .setBold()
            .setTextAlignment(TextAlignment.CENTER));
        document.add(new Paragraph(" "));
        
        // 查找 detail band 并输出数据
        if (template.has("bands")) {
            for (JsonNode band : template.get("bands")) {
                if ("detail".equals(band.get("type").asText()) && band.has("dataSource")) {
                    String dsName = band.get("dataSource").asText();
                    List<Map<String, Object>> rows = data.getOrDefault(dsName, new ArrayList<>());
                    
                    if (!rows.isEmpty()) {
                        // 从 pageHeader band 提取列定义（中文列头）
                        List<ColumnDef> columns = extractColumnsFromHeader(template, band.get("elements"));
                        
                        Table table = new Table(columns.size());
                        table.useAllAvailableWidth();
                        
                        // 表头
                        for (ColumnDef col : columns) {
                            table.addHeaderCell(new Cell()
                                .add(new Paragraph(col.header).setFont(chineseFont).setBold())
                                .setTextAlignment(col.alignment));
                        }
                        
                        // 数据行
                        for (Map<String, Object> rowData : rows) {
                            for (ColumnDef col : columns) {
                                String value = replaceExpressions(col.template, rowData);
                                table.addCell(new Cell()
                                    .add(new Paragraph(value).setFont(chineseFont))
                                    .setTextAlignment(col.alignment));
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
     * 列定义
     */
    private static class ColumnDef {
        String header;
        String template;
        TextAlignment alignment;
    }
    
    /**
     * 从 pageHeader band 提取列定义（中文列头）
     * 如果没有 pageHeader，则从 detail elements 提取
     */
    private List<ColumnDef> extractColumnsFromHeader(JsonNode template, JsonNode detailElements) {
        // 先找 pageHeader band
        if (template.has("bands")) {
            for (JsonNode band : template.get("bands")) {
                if ("pageHeader".equals(band.get("type").asText()) && band.has("elements")) {
                    return extractColumnsFromElements(band.get("elements"), detailElements);
                }
            }
        }
        
        // 没有 pageHeader，从 detail elements 提取
        return extractColumnsFromElements(detailElements, null);
    }
    
    /**
     * 从 elements 提取列定义
     * headerElements: pageHeader 的 elements（提供中文列头）
     * detailElements: detail 的 elements（提供模板和位置）
     */
    private List<ColumnDef> extractColumnsFromElements(JsonNode headerElements, JsonNode detailElements) {
        List<ColumnDef> columns = new ArrayList<>();
        JsonNode sourceElements = headerElements != null ? headerElements : detailElements;
        
        for (int i = 0; i < sourceElements.size(); i++) {
            JsonNode el = sourceElements.get(i);
            if ("text".equals(el.get("type").asText())) {
                ColumnDef col = new ColumnDef();
                
                // 列头文本
                col.header = el.has("text") ? el.get("text").asText() : "";
                
                // 从 detail elements 获取模板（如果有 headerElements）
                if (detailElements != null && i < detailElements.size()) {
                    JsonNode detailEl = detailElements.get(i);
                    col.template = detailEl.has("text") ? detailEl.get("text").asText() : "";
                    
                    // 对齐方式优先从 detail element 取
                    String align = detailEl.has("alignment") ? detailEl.get("alignment").asText() : 
                                   (el.has("alignment") ? el.get("alignment").asText() : "left");
                    col.alignment = parseAlignment(align);
                } else {
                    // 没有 headerElements，从当前 element 取
                    col.template = col.header;
                    String align = el.has("alignment") ? el.get("alignment").asText() : "left";
                    col.alignment = parseAlignment(align);
                }
                
                columns.add(col);
            }
        }
        
        return columns;
    }
    
    /**
     * 从模板 elements 提取列定义（旧方法，保留兼容）
     */
    private List<ColumnDef> extractColumns(JsonNode elements) {
        List<ColumnDef> columns = new ArrayList<>();
        
        for (JsonNode el : elements) {
            if ("text".equals(el.get("type").asText())) {
                ColumnDef col = new ColumnDef();
                col.template = el.has("text") ? el.get("text").asText() : "";
                
                // 从模板文本提取列名（去掉 {{currentRow.xxx}} 包装）
                String header = col.template;
                if (header.startsWith("{{currentRow.") && header.endsWith("}}")) {
                    header = header.substring(13, header.length() - 2);
                }
                col.header = header;
                
                // 对齐方式
                String align = el.has("alignment") ? el.get("alignment").asText() : "left";
                col.alignment = parseAlignment(align);
                
                columns.add(col);
            }
        }
        
        return columns;
    }
    
    /**
     * 解析对齐方式
     */
    private TextAlignment parseAlignment(String align) {
        return switch (align) {
            case "center" -> TextAlignment.CENTER;
            case "right" -> TextAlignment.RIGHT;
            default -> TextAlignment.LEFT;
        };
    }
    
    /**
     * 替换文本中的表达式 {{currentRow.xxx}}
     */
    private String replaceExpressions(String text, Map<String, Object> rowData) {
        if (text == null || !text.contains("{{")) {
            return text;
        }
        
        String result = text;
        for (Map.Entry<String, Object> entry : rowData.entrySet()) {
            String placeholder = "{{currentRow." + entry.getKey() + "}}";
            String value = entry.getValue() != null ? entry.getValue().toString() : "";
            result = result.replace(placeholder, value);
        }
        
        return result;
    }
}
