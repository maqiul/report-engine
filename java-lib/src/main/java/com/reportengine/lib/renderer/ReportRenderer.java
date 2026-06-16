package com.reportengine.lib.renderer;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.reportengine.lib.model.RenderRequest;
import com.reportengine.lib.model.RenderResponse;
import com.reportengine.lib.model.RenderResponse.*;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import java.util.Map;

/**
 * 报表渲染引擎 - 核心渲染逻辑
 * 
 * 使用示例：
 * <pre>
 * ReportRenderer renderer = new ReportRenderer();
 * RenderResponse response = renderer.render(request);
 * </pre>
 */
public class ReportRenderer {

    private final ObjectMapper objectMapper = new ObjectMapper();

    /**
     * 渲染报表
     */
    public RenderResponse render(RenderRequest request) {
        RenderResponse response = new RenderResponse();
        
        try {
            JsonNode template = objectMapper.readTree(request.getTemplateJson());
            
            double pageWidth = 210;
            double pageHeight = 297;
            if (template.has("page")) {
                JsonNode page = template.get("page");
                if (page.has("width")) pageWidth = page.get("width").asDouble();
                if (page.has("height")) pageHeight = page.get("height").asDouble();
            }
            
            Map<String, List<Map<String, Object>>> data = request.getData();
            
            List<RenderedElementInfo> allElements = new ArrayList<>();
            double currentY = 0;
            int totalPages = 1;
            
            if (template.has("bands")) {
                JsonNode bands = template.get("bands");
                for (JsonNode band : bands) {
                    String bandType = band.has("type") ? band.get("type").asText() : "";
                    double bandHeight = band.has("height") ? band.get("height").asDouble() : 10;
                    
                    if ("detail".equals(bandType) && band.has("dataSource")) {
                        String dsName = band.get("dataSource").asText();
                        List<Map<String, Object>> rows = data.getOrDefault(dsName, new ArrayList<>());
                        
                        for (int i = 0; i < rows.size(); i++) {
                            Map<String, Object> row = rows.get(i);
                            if (band.has("elements")) {
                                for (JsonNode element : band.get("elements")) {
                                    RenderedElementInfo el = renderElement(element, row, currentY, 1, totalPages);
                                    if (el != null) {
                                        allElements.add(el);
                                    }
                                }
                            }
                            currentY += bandHeight;
                        }
                    } else {
                        if (band.has("elements")) {
                            for (JsonNode element : band.get("elements")) {
                                RenderedElementInfo el = renderElement(element, null, currentY, 1, totalPages);
                                if (el != null) {
                                    allElements.add(el);
                                }
                            }
                        }
                        currentY += bandHeight;
                    }
                }
            }
            
            PageInfo pageInfo = new PageInfo();
            pageInfo.setPageNumber(1);
            pageInfo.setWidth(pageWidth);
            pageInfo.setHeight(pageHeight);
            pageInfo.setElements(allElements);
            
            List<PageInfo> pages = new ArrayList<>();
            pages.add(pageInfo);
            
            response.setSuccess(true);
            response.setPages(pages);
            response.setTotalPages(totalPages);
            
        } catch (Exception e) {
            response.setSuccess(false);
            response.setError("渲染失败: " + e.getMessage());
        }
        
        return response;
    }
    
    /**
     * 渲染单个元素
     */
    private RenderedElementInfo renderElement(JsonNode element, Map<String, Object> rowData, 
                                               double offsetY, int currentPage, int totalPages) {
        String type = element.has("type") ? element.get("type").asText() : "text";
        
        RenderedElementInfo el = new RenderedElementInfo();
        el.setType(type);
        el.setX(element.has("x") ? element.get("x").asDouble() : 0);
        el.setY((element.has("y") ? element.get("y").asDouble() : 0) + offsetY);
        el.setWidth(element.has("width") ? element.get("width").asDouble() : 100);
        el.setHeight(element.has("height") ? element.get("height").asDouble() : 10);
        
        if ("text".equals(type)) {
            String text = element.has("text") ? element.get("text").asText() : "";
            if (rowData != null) {
                text = replaceExpressions(text, rowData);
            }
            text = replaceGlobalVariables(text, currentPage, totalPages);
            el.setText(text);
            el.setAlignment(element.has("alignment") ? element.get("alignment").asText() : "left");
            
            if (element.has("font")) {
                JsonNode font = element.get("font");
                FontInfo fontInfo = new FontInfo();
                if (font.has("family")) fontInfo.setFamily(font.get("family").asText());
                if (font.has("size")) fontInfo.setSize(font.get("size").asDouble());
                if (font.has("bold")) fontInfo.setBold(font.get("bold").asBoolean());
                if (font.has("italic")) fontInfo.setItalic(font.get("italic").asBoolean());
                if (font.has("underline")) fontInfo.setUnderline(font.get("underline").asBoolean());
                if (font.has("color")) fontInfo.setColor(font.get("color").asText());
                el.setFont(fontInfo);
            }
        } else if ("line".equals(type)) {
            el.setBorderColor(element.has("color") ? element.get("color").asText() : "#000000");
            el.setBorderWidth(element.has("width") ? element.get("width").asDouble() : 1);
        } else if ("shape".equals(type) || "rect".equals(type)) {
            el.setBackgroundColor(element.has("fillColor") ? element.get("fillColor").asText() : "#FFFFFF");
        } else if ("barcode".equals(type)) {
            String value = element.has("value") ? element.get("value").asText() : "";
            if (rowData != null) {
                value = replaceExpressions(value, rowData);
            }
            el.setText(value);
        }
        
        return el;
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
    
    /**
     * 替换全局变量 {{page}}, {{totalPages}} 等
     */
    private String replaceGlobalVariables(String text, int currentPage, int totalPages) {
        if (text == null || !text.contains("{{")) {
            return text;
        }
        
        String result = text;
        result = result.replace("{{page}}", String.valueOf(currentPage));
        result = result.replace("{{totalPages}}", String.valueOf(totalPages));
        return result;
    }
}
