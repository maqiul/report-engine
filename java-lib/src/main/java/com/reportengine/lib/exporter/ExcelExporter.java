package com.reportengine.lib.exporter;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.reportengine.lib.model.RenderRequest;
import org.apache.poi.ss.usermodel.HorizontalAlignment;
import org.apache.poi.ss.usermodel.VerticalAlignment;
import org.apache.poi.ss.util.CellRangeAddress;
import org.apache.poi.xssf.usermodel.XSSFCell;
import org.apache.poi.xssf.usermodel.XSSFCellStyle;
import org.apache.poi.xssf.usermodel.XSSFFont;
import org.apache.poi.xssf.usermodel.XSSFRow;
import org.apache.poi.xssf.usermodel.XSSFSheet;
import org.apache.poi.xssf.usermodel.XSSFWorkbook;

import java.io.ByteArrayOutputStream;
import java.util.*;

/**
 * Excel 导出器 - 按模板精确坐标布局
 * 
 * 根据模板中每个元素的 x、y、width、height（mm）精确设置 Excel 的列宽和行高，
 * 最大程度还原模板的视觉效果。
 */
public class ExcelExporter {

    private final ObjectMapper objectMapper = new ObjectMapper();
    
    // mm 转 Excel 列宽单位（1 字符宽度 ≈ 1.85mm，基于默认字体）
    private static final double MM_TO_COL_WIDTH = 1.0 / 1.85;
    // mm 转 Excel 行高单位（1pt = 0.353mm）
    private static final double MM_TO_ROW_HEIGHT = 1.0 / 0.353;

    public byte[] export(RenderRequest request) throws Exception {
        JsonNode template = objectMapper.readTree(request.getTemplateJson());
        Map<String, List<Map<String, Object>>> data = request.getData();
        
        ByteArrayOutputStream baos = new ByteArrayOutputStream();
        XSSFWorkbook workbook = new XSSFWorkbook();
        XSSFSheet sheet = workbook.createSheet("Report");
        
        // 获取页面尺寸和边距
        double pageWidth = 210, pageHeight = 297;
        double marginTop = 0, marginBottom = 0, marginLeft = 0, marginRight = 0;
        if (template.has("page")) {
            JsonNode page = template.get("page");
            if (page.has("width")) pageWidth = page.get("width").asDouble();
            if (page.has("height")) pageHeight = page.get("height").asDouble();
            if (page.has("margin")) {
                JsonNode margin = page.get("margin");
                if (margin.has("top")) marginTop = margin.get("top").asDouble();
                if (margin.has("bottom")) marginBottom = margin.get("bottom").asDouble();
                if (margin.has("left")) marginLeft = margin.get("left").asDouble();
                if (margin.has("right")) marginRight = margin.get("right").asDouble();
            }
        }
        
        // 收集所有元素（包括数据展开后的）
        List<ElementInfo> allElements = new ArrayList<>();
        List<Double> xBoundaries = new ArrayList<>();
        List<Double> yBoundaries = new ArrayList<>();
        
        // 添加左边距和上边距作为起始边界
        xBoundaries.add(marginLeft);
        yBoundaries.add(marginTop);
        
        double currentY = marginTop;
        
        if (template.has("bands")) {
            for (JsonNode band : template.get("bands")) {
                String bandType = band.get("type").asText();
                double bandHeight = band.has("height") ? band.get("height").asDouble() : 0;
                
                // detail band 不直接输出元素，而是按数据行展开
                if (!"detail".equals(bandType) && band.has("elements")) {
                    for (JsonNode el : band.get("elements")) {
                        double x = el.has("x") ? el.get("x").asDouble() : 0;
                        double y = el.has("y") ? el.get("y").asDouble() : 0;
                        double w = el.has("width") ? el.get("width").asDouble() : 0;
                        double h = el.has("height") ? el.get("height").asDouble() : 0;
                        
                        // 绝对坐标
                        double absX = marginLeft + x;
                        double absY = currentY + y;
                        
                        ElementInfo info = new ElementInfo();
                        info.type = el.has("type") ? el.get("type").asText() : "text";
                        info.text = el.has("text") ? el.get("text").asText() : "";
                        info.absX = absX;
                        info.absY = absY;
                        info.width = w;
                        info.height = h;
                        info.alignment = el.has("alignment") ? el.get("alignment").asText() : "left";
                        info.font = el.has("font") ? el.get("font") : null;
                        
                        allElements.add(info);
                        
                        // 收集边界
                        xBoundaries.add(absX);
                        xBoundaries.add(absX + w);
                        yBoundaries.add(absY);
                        yBoundaries.add(absY + h);
                    }
                }
                
                currentY += bandHeight;
                
                // detail band 按数据行展开
                if ("detail".equals(bandType) && band.has("dataSource")) {
                    String dsName = band.get("dataSource").asText();
                    List<Map<String, Object>> rows = data.getOrDefault(dsName, new ArrayList<>());
                    
                    for (Map<String, Object> rowData : rows) {
                        if (band.has("elements")) {
                            for (JsonNode el : band.get("elements")) {
                                double x = el.has("x") ? el.get("x").asDouble() : 0;
                                double y = el.has("y") ? el.get("y").asDouble() : 0;
                                double w = el.has("width") ? el.get("width").asDouble() : 0;
                                double h = el.has("height") ? el.get("height").asDouble() : 0;
                                
                                double absX = marginLeft + x;
                                double absY = currentY + y;
                                
                                ElementInfo info = new ElementInfo();
                                info.type = el.has("type") ? el.get("type").asText() : "text";
                                info.text = replaceExpressions(el.has("text") ? el.get("text").asText() : "", rowData);
                                info.absX = absX;
                                info.absY = absY;
                                info.width = w;
                                info.height = h;
                                info.alignment = el.has("alignment") ? el.get("alignment").asText() : "left";
                                info.font = el.has("font") ? el.get("font") : null;
                                
                                allElements.add(info);
                                
                                xBoundaries.add(absX);
                                xBoundaries.add(absX + w);
                                yBoundaries.add(absY);
                                yBoundaries.add(absY + h);
                            }
                        }
                        currentY += bandHeight;
                    }
                }
            }
        }
        
        // 排序去重边界
        Collections.sort(xBoundaries);
        Collections.sort(yBoundaries);
        xBoundaries = removeDuplicates(xBoundaries);
        yBoundaries = removeDuplicates(yBoundaries);
        
        // 添加右边距和下边距
        if (xBoundaries.isEmpty() || xBoundaries.get(xBoundaries.size() - 1) < pageWidth - marginRight) {
            xBoundaries.add(pageWidth - marginRight);
        }
        if (yBoundaries.isEmpty() || yBoundaries.get(yBoundaries.size() - 1) < pageHeight - marginBottom) {
            yBoundaries.add(pageHeight - marginBottom);
        }
        
        // 设置列宽
        for (int i = 0; i < xBoundaries.size() - 1; i++) {
            double widthMm = xBoundaries.get(i + 1) - xBoundaries.get(i);
            int excelWidth = (int)(widthMm * MM_TO_COL_WIDTH * 256);
            if (excelWidth < 256) excelWidth = 256; // 最小 1 字符
            sheet.setColumnWidth(i, excelWidth);
        }
        
        // 设置行高
        for (int i = 0; i < yBoundaries.size() - 1; i++) {
            double heightMm = yBoundaries.get(i + 1) - yBoundaries.get(i);
            float excelHeight = (float)(heightMm * MM_TO_ROW_HEIGHT);
            XSSFRow row = sheet.getRow(i);
            if (row == null) row = sheet.createRow(i);
            row.setHeightInPoints(excelHeight);
        }
        
        // 创建样式缓存
        Map<String, XSSFCellStyle> styleCache = new HashMap<>();
        
        // 填充元素到单元格
        for (ElementInfo el : allElements) {
            // 找到元素对应的行列范围
            int startCol = findBoundaryIndex(xBoundaries, el.absX);
            int endCol = findBoundaryIndex(xBoundaries, el.absX + el.width) - 1;
            int startRow = findBoundaryIndex(yBoundaries, el.absY);
            int endRow = findBoundaryIndex(yBoundaries, el.absY + el.height) - 1;
            
            if (startCol < 0 || startRow < 0) continue;
            if (endCol >= xBoundaries.size() - 1) endCol = xBoundaries.size() - 2;
            if (endRow >= yBoundaries.size() - 1) endRow = yBoundaries.size() - 2;
            
            XSSFRow row = sheet.getRow(startRow);
            if (row == null) row = sheet.createRow(startRow);
            XSSFCell cell = row.getCell(startCol);
            if (cell == null) cell = row.createCell(startCol);
            
            // 设置值
            if ("text".equals(el.type)) {
                cell.setCellValue(el.text);
            }
            
            // 设置样式
            String styleKey = el.alignment + "_" + (el.font != null ? el.font.toString() : "default");
            XSSFCellStyle style = styleCache.get(styleKey);
            if (style == null) {
                style = workbook.createCellStyle();
                
                // 对齐
                style.setAlignment(parseAlignment(el.alignment));
                style.setVerticalAlignment(VerticalAlignment.CENTER);
                
                // 字体
                if (el.font != null) {
                    XSSFFont font = workbook.createFont();
                    if (el.font.has("family")) font.setFontName(el.font.get("family").asText());
                    if (el.font.has("size")) font.setFontHeightInPoints((short) el.font.get("size").asInt());
                    if (el.font.has("bold") && el.font.get("bold").asBoolean()) font.setBold(true);
                    if (el.font.has("italic") && el.font.get("italic").asBoolean()) font.setItalic(true);
                    if (el.font.has("underline") && el.font.get("underline").asBoolean()) font.setUnderline(org.apache.poi.ss.usermodel.Font.U_SINGLE);
                    if (el.font.has("color")) {
                        String color = el.font.get("color").asText();
                        if (color.startsWith("#") && color.length() == 7) {
                            short r = Short.parseShort(color.substring(1, 3), 16);
                            short g = Short.parseShort(color.substring(3, 5), 16);
                            short b = Short.parseShort(color.substring(5, 7), 16);
                            // 简化：使用黑色
                            font.setColor(org.apache.poi.ss.usermodel.IndexedColors.BLACK.getIndex());
                        }
                    }
                    style.setFont(font);
                }
                
                styleCache.put(styleKey, style);
            }
            cell.setCellStyle(style);
            
            // 合并单元格（如果跨越多列/多行）
            if (endCol > startCol || endRow > startRow) {
                CellRangeAddress range = new CellRangeAddress(startRow, endRow, startCol, endCol);
                sheet.addMergedRegion(range);
            }
        }
        
        // 设置打印参数
        applyPrintSettings(workbook, template);
        
        workbook.write(baos);
        workbook.close();
        return baos.toByteArray();
    }
    
    private int findBoundaryIndex(List<Double> boundaries, double value) {
        for (int i = 0; i < boundaries.size(); i++) {
            if (Math.abs(boundaries.get(i) - value) < 0.1) {
                return i;
            }
        }
        // 找最近的
        int closest = 0;
        double minDiff = Math.abs(boundaries.get(0) - value);
        for (int i = 1; i < boundaries.size(); i++) {
            double diff = Math.abs(boundaries.get(i) - value);
            if (diff < minDiff) {
                minDiff = diff;
                closest = i;
            }
        }
        return closest;
    }
    
    private List<Double> removeDuplicates(List<Double> list) {
        List<Double> result = new ArrayList<>();
        for (int i = 0; i < list.size(); i++) {
            if (i == 0 || Math.abs(list.get(i) - list.get(i - 1)) > 0.1) {
                result.add(list.get(i));
            }
        }
        return result;
    }
    
    private void applyPrintSettings(XSSFWorkbook workbook, JsonNode template) {
        if (!template.has("page")) return;
        
        JsonNode page = template.get("page");
        double widthMm = page.has("width") ? page.get("width").asDouble() : 210;
        double heightMm = page.has("height") ? page.get("height").asDouble() : 297;
        
        double marginTop = 0, marginBottom = 0, marginLeft = 0, marginRight = 0;
        if (page.has("margin")) {
            JsonNode margin = page.get("margin");
            if (margin.has("top")) marginTop = margin.get("top").asDouble();
            if (margin.has("bottom")) marginBottom = margin.get("bottom").asDouble();
            if (margin.has("left")) marginLeft = margin.get("left").asDouble();
            if (margin.has("right")) marginRight = margin.get("right").asDouble();
        }
        
        for (int i = 0; i < workbook.getNumberOfSheets(); i++) {
            XSSFSheet sheet = workbook.getSheetAt(i);
            
            short paperSize = getPaperSize(widthMm, heightMm);
            sheet.getPrintSetup().setPaperSize(paperSize);
            sheet.getPrintSetup().setLandscape(widthMm > heightMm);
            
            double mmToInch = 0.03937;
            sheet.setMargin(org.apache.poi.ss.usermodel.Sheet.TopMargin, marginTop * mmToInch);
            sheet.setMargin(org.apache.poi.ss.usermodel.Sheet.BottomMargin, marginBottom * mmToInch);
            sheet.setMargin(org.apache.poi.ss.usermodel.Sheet.LeftMargin, marginLeft * mmToInch);
            sheet.setMargin(org.apache.poi.ss.usermodel.Sheet.RightMargin, marginRight * mmToInch);
            
            int lastRow = sheet.getLastRowNum();
            int lastCol = 0;
            for (int r = 0; r <= lastRow; r++) {
                XSSFRow row = sheet.getRow(r);
                if (row != null) {
                    for (int c = 0; c < row.getLastCellNum(); c++) {
                        if (row.getCell(c) != null) {
                            lastCol = Math.max(lastCol, c);
                        }
                    }
                }
            }
            if (lastRow >= 0 && lastCol >= 0) {
                // setPrintArea(sheetIndex, startColumn, startRow, endColumn, endRow)
                workbook.setPrintArea(i, 0, 0, lastCol, lastRow);
            }
        }
    }
    
    private short getPaperSize(double widthMm, double heightMm) {
        if (Math.abs(widthMm - 210) < 5 && Math.abs(heightMm - 297) < 5) return 9;
        if (Math.abs(widthMm - 297) < 5 && Math.abs(heightMm - 210) < 5) return 9;
        if (Math.abs(widthMm - 216) < 5 && Math.abs(heightMm - 279) < 5) return 1;
        if (Math.abs(widthMm - 297) < 10 && Math.abs(heightMm - 420) < 10) return 8;
        return 9;
    }
    
    private HorizontalAlignment parseAlignment(String align) {
        return switch (align) {
            case "center" -> HorizontalAlignment.CENTER;
            case "right" -> HorizontalAlignment.RIGHT;
            default -> HorizontalAlignment.LEFT;
        };
    }
    
    private String replaceExpressions(String text, Map<String, Object> rowData) {
        if (text == null || !text.contains("{{")) return text;
        String result = text;
        for (Map.Entry<String, Object> entry : rowData.entrySet()) {
            String placeholder = "{{currentRow." + entry.getKey() + "}}";
            String value = entry.getValue() != null ? entry.getValue().toString() : "";
            result = result.replace(placeholder, value);
        }
        return result;
    }
    
    private static class ElementInfo {
        String type;
        String text;
        double absX;
        double absY;
        double width;
        double height;
        String alignment;
        JsonNode font;
    }
}
