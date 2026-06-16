package com.reportengine.web.controller;

import tools.jackson.databind.JsonNode;
import tools.jackson.databind.ObjectMapper;
import com.reportengine.web.model.RenderRequest;
import com.reportengine.web.model.RenderResponse;
import com.reportengine.web.model.RenderResponse.FontInfo;
import com.reportengine.web.model.RenderResponse.PageInfo;
import com.reportengine.web.model.RenderResponse.RenderedElementInfo;
import org.springframework.web.bind.annotation.*;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import java.util.Map;

/**
 * 渲染控制器 - 提供报表预览 API
 */
@RestController
@RequestMapping("/api/render")
@CrossOrigin(origins = {"http://localhost:5173", "http://localhost:3000", "http://127.0.0.1:3000"})
public class RenderController {

    private final ObjectMapper objectMapper = new ObjectMapper();

    /**
     * 预览报表 - 解析模板并返回渲染结果
     */
    @PostMapping("/preview")
    public RenderResponse preview(@RequestBody RenderRequest request) {
        RenderResponse response = new RenderResponse();
        
        try {
            // 解析模板 JSON
            JsonNode template = objectMapper.readTree(request.getTemplateJson());
            
            // 获取页面设置
            double pageWidth = 210;
            double pageHeight = 297;
            if (template.has("page")) {
                JsonNode page = template.get("page");
                if (page.has("width")) pageWidth = page.get("width").asDouble();
                if (page.has("height")) pageHeight = page.get("height").asDouble();
            }
            
            // 获取数据源
            Map<String, List<Map<String, Object>>> data = request.getData();
            
            // 渲染 bands
            List<RenderedElementInfo> allElements = new ArrayList<>();
            double currentY = 0;
            
            if (template.has("bands")) {
                JsonNode bands = template.get("bands");
                for (JsonNode band : bands) {
                    String bandType = band.has("type") ? band.get("type").asText() : "";
                    double bandHeight = band.has("height") ? band.get("height").asDouble() : 10;
                    
                    if ("detail".equals(bandType) && band.has("dataSource")) {
                        String dsName = band.get("dataSource").asText();
                        List<Map<String, Object>> rows = data.getOrDefault(dsName, new ArrayList<>());
                        
                        // 为每行数据渲染
                        for (int i = 0; i < rows.size(); i++) {
                            Map<String, Object> row = rows.get(i);
                            if (band.has("elements")) {
                                for (JsonNode element : band.get("elements")) {
                                    RenderedElementInfo el = renderElement(element, row, currentY);
                                    if (el != null) {
                                        allElements.add(el);
                                    }
                                }
                            }
                            currentY += bandHeight;
                        }
                    } else {
                        // 非 detail band（header, footer 等）
                        if (band.has("elements")) {
                            for (JsonNode element : band.get("elements")) {
                                RenderedElementInfo el = renderElement(element, null, currentY);
                                if (el != null) {
                                    allElements.add(el);
                                }
                            }
                        }
                        currentY += bandHeight;
                    }
                }
            }
            
            // 构建页面
            PageInfo pageInfo = new PageInfo();
            pageInfo.setPageNumber(1);
            pageInfo.setWidth(pageWidth);
            pageInfo.setHeight(pageHeight);
            pageInfo.setElements(allElements);
            
            List<PageInfo> pages = new ArrayList<>();
            pages.add(pageInfo);
            
            response.setSuccess(true);
            response.setPages(pages);
            
        } catch (Exception e) {
            response.setSuccess(false);
            response.setError("渲染失败: " + e.getMessage());
        }
        
        return response;
    }
    
    /**
     * 渲染单个元素
     */
    private RenderedElementInfo renderElement(JsonNode element, Map<String, Object> rowData, double offsetY) {
        String type = element.has("type") ? element.get("type").asText() : "text";
        
        RenderedElementInfo el = new RenderedElementInfo();
        el.setType(type);
        el.setX(element.has("x") ? element.get("x").asDouble() : 0);
        el.setY((element.has("y") ? element.get("y").asDouble() : 0) + offsetY);
        el.setWidth(element.has("width") ? element.get("width").asDouble() : 100);
        el.setHeight(element.has("height") ? element.get("height").asDouble() : 10);
        
        if ("text".equals(type)) {
            String text = element.has("text") ? element.get("text").asText() : "";
            // 替换表达式 {{currentRow.xxx}}
            if (rowData != null) {
                text = replaceExpressions(text, rowData);
            }
            el.setText(text);
            el.setAlignment(element.has("alignment") ? element.get("alignment").asText() : "left");
            
            // 解析字体
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
}
