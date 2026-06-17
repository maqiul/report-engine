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
 * Excel 导出器 - 按模板布局生成表格
 * 
 * 策略：
 * 1. 收集所有元素的 x 坐标 → 去重排序 → 作为列
 * 2. 每个 band（或数据行）→ 作为一行
 * 3. 元素按 x 坐标匹配到对应列
 */
public class ExcelExporter {

    private final ObjectMapper objectMapper = new ObjectMapper();
    
    // mm 转 Excel 列宽单位（1 字符宽度 ≈ 1.85mm）
    private static final double MM_TO_COL_WIDTH = 1.0 / 1.85;
    // mm 转 Excel 行高单位（1pt = 0.353mm）
    private static final double MM_TO_ROW_HEIGHT = 1.0 / 0.353;

    public byte[] export(RenderRequest request) throws Exception {
        JsonNode template = objectMapper.readTree(request.getTemplateJson());
        Map<String, List<Map<String, Object>>> data = request.getData();
        
        ByteArrayOutputStream baos = new ByteArrayOutputStream();
        XSSFWorkbook workbook = new XSSFWorkbook();
        XSSFSheet sheet = workbook.createSheet("Report");
        
        // 页面设置
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
        
        // 第一步：收集所有唯一的 x 坐标作为列边界
        TreeSet<Double> xSet = new TreeSet<>();
        xSet.add(marginLeft);
        xSet.add(pageWidth - marginRight);
        
        if (template.has("bands")) {
            for (JsonNode band : template.get("bands")) {
                if (band.has("elements")) {
                    for (JsonNode el : band.get("elements")) {
                        double x = el.has("x") ? el.get("x").asDouble() : 0;
                        double w = el.has("width") ? el.get("width").asDouble() : 0;
                        xSet.add(marginLeft + x);
                        xSet.add(marginLeft + x + w);
                    }
                }
            }
        }
        
        List<Double> xBoundaries = new ArrayList<>(xSet);
        int numCols = xBoundaries.size() - 1;
        
        // 设置列宽
        for (int i = 0; i < numCols; i++) {
            double widthMm = xBoundaries.get(i + 1) - xBoundaries.get(i);
            int excelWidth = (int)(widthMm * MM_TO_COL_WIDTH * 256);
            if (excelWidth < 256) excelWidth = 256;
            sheet.setColumnWidth(i, excelWidth);
        }
        
        // 第二步：逐行输出
        int currentRow = 0;
        double currentY = marginTop;
        
        if (template.has("bands")) {
            for (JsonNode band : template.get("bands")) {
                String bandType = band.get("type").asText();
                double bandHeight = band.has("height") ? band.get("height").asDouble() : 8;
                
                // 非 detail band：先创建行，输出元素
                if (!"detail".equals(bandType)) {
                    XSSFRow row = sheet.createRow(currentRow);
                    row.setHeightInPoints((float)(bandHeight * MM_TO_ROW_HEIGHT));
                    
                    if (band.has("elements")) {
                        for (JsonNode el : band.get("elements")) {
                            double x = el.has("x") ? el.get("x").asDouble() : 0;
                            double w = el.has("width") ? el.get("width").asDouble() : 0;
                            double h = el.has("height") ? el.get("height").asDouble() : 0;
                            
                            double absX = marginLeft + x;
                            
                            int col = findColumnIndex(xBoundaries, absX);
                            int endCol = findColumnIndex(xBoundaries, absX + w) - 1;
                            
                            if (col >= 0 && col < numCols) {
                                XSSFCell cell = row.createCell(col);
                                String text = el.has("text") ? el.get("text").asText() : "";
                                cell.setCellValue(text);
                                
                                // 设置样式
                                XSSFCellStyle style = workbook.createCellStyle();
                                String align = el.has("alignment") ? el.get("alignment").asText() : "left";
                                style.setAlignment(parseAlignment(align));
                                style.setVerticalAlignment(VerticalAlignment.CENTER);
                                
                                // 字体
                                if (el.has("font")) {
                                    JsonNode font = el.get("font");
                                    XSSFFont poiFont = workbook.createFont();
                                    if (font.has("family")) poiFont.setFontName(font.get("family").asText());
                                    if (font.has("size")) poiFont.setFontHeightInPoints((short) font.get("size").asInt());
                                    if (font.has("bold") && font.get("bold").asBoolean()) poiFont.setBold(true);
                                    if (font.has("italic") && font.get("italic").asBoolean()) poiFont.setItalic(true);
                                    style.setFont(poiFont);
                                }
                                
                                cell.setCellStyle(style);
                                
                                // 合并单元格
                                if (endCol > col) {
                                    sheet.addMergedRegion(new CellRangeAddress(currentRow, currentRow, col, endCol));
                                }
                            }
                        }
                    }
                    
                    currentRow++;
                    currentY += bandHeight;
                }
                
                // detail band 展开数据行
                if ("detail".equals(bandType) && band.has("dataSource")) {
                    String dsName = band.get("dataSource").asText();
                    List<Map<String, Object>> rows = data.getOrDefault(dsName, new ArrayList<>());
                    
                    for (Map<String, Object> rowData : rows) {
                        XSSFRow dataRow = sheet.createRow(currentRow);
                        dataRow.setHeightInPoints((float)(bandHeight * MM_TO_ROW_HEIGHT));
                        
                        if (band.has("elements")) {
                            for (JsonNode el : band.get("elements")) {
                                double x = el.has("x") ? el.get("x").asDouble() : 0;
                                double w = el.has("width") ? el.get("width").asDouble() : 0;
                                
                                double absX = marginLeft + x;
                                int col = findColumnIndex(xBoundaries, absX);
                                int endCol = findColumnIndex(xBoundaries, absX + w) - 1;
                                
                                if (col >= 0 && col < numCols) {
                                    XSSFCell cell = dataRow.createCell(col);
                                    String text = el.has("text") ? el.get("text").asText() : "";
                                    text = replaceExpressions(text, rowData);
                                    
                                    // 智能识别类型
                                    XSSFCellStyle style = workbook.createCellStyle();
                                    String align = el.has("alignment") ? el.get("alignment").asText() : "left";
                                    style.setAlignment(parseAlignment(align));
                                    style.setVerticalAlignment(VerticalAlignment.CENTER);
                                    
                                    if (setCellValueSmart(cell, text, style, workbook)) {
                                        // 数字已设置
                                    } else {
                                        cell.setCellValue(text);
                                    }
                                    cell.setCellStyle(style);
                                    
                                    if (endCol > col) {
                                        sheet.addMergedRegion(new CellRangeAddress(currentRow, currentRow, col, endCol));
                                    }
                                }
                            }
                        }
                        currentRow++;
                    }
                }
            }
        }
        
        // 打印设置
        applyPrintSettings(workbook, template);
        
        workbook.write(baos);
        workbook.close();
        return baos.toByteArray();
    }
    
    private int findColumnIndex(List<Double> boundaries, double value) {
        for (int i = 0; i < boundaries.size(); i++) {
            if (Math.abs(boundaries.get(i) - value) < 0.5) {
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
                // 用字符串方式设置打印区域，避免参数顺序问题
                String colLetter = org.apache.poi.ss.util.CellReference.convertNumToColString(lastCol);
                String printArea = "$A$1:$" + colLetter + "$" + (lastRow + 1);
                workbook.setPrintArea(i, printArea);
            }
        }
    }
    
    private short getPaperSize(double widthMm, double heightMm) {
        if (Math.abs(widthMm - 210) < 5 && Math.abs(heightMm - 297) < 5) return 9; // A4
        if (Math.abs(widthMm - 297) < 5 && Math.abs(heightMm - 210) < 5) return 9; // A4 landscape
        if (Math.abs(widthMm - 216) < 5 && Math.abs(heightMm - 279) < 5) return 1; // Letter
        if (Math.abs(widthMm - 297) < 10 && Math.abs(heightMm - 420) < 10) return 8; // A3
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
    
    /**
     * 智能识别值类型并设置单元格
     * @return true 表示是数字/日期
     */
    private boolean setCellValueSmart(XSSFCell cell, String text, XSSFCellStyle style, XSSFWorkbook workbook) {
        if (text == null || text.isEmpty()) return false;
        
        // 数字识别（含小数、负数、千分位）
        if (text.matches("^-?\\d{1,3}(,\\d{3})*(\\.\\d+)?$") || text.matches("^-?\\d+(\\.\\d+)?$")) {
            try {
                double num = Double.parseDouble(text.replace(",", ""));
                cell.setCellValue(num);
                // 检测是否含小数
                if (text.contains(".")) {
                    // 保留原始小数位数
                    int decimals = text.substring(text.indexOf('.') + 1).length();
                    if (decimals > 0 && decimals <= 10) {
                        String format = "#,##0.";
                        for (int i = 0; i < decimals; i++) format += "0";
                        style.setDataFormat(workbook.createDataFormat().getFormat(format));
                    }
                } else {
                    style.setDataFormat(workbook.createDataFormat().getFormat("#,##0"));
                }
                return true;
            } catch (NumberFormatException e) {
                return false;
            }
        }
        
        // 日期识别 yyyy-MM-dd 或 yyyy/MM/dd
        if (text.matches("^\\d{4}[-/]\\d{1,2}[-/]\\d{1,2}.*$")) {
            try {
                String datePart = text.substring(0, 10).replace("/", "-");
                java.time.LocalDate date = java.time.LocalDate.parse(datePart);
                cell.setCellValue(java.sql.Date.valueOf(date));
                style.setDataFormat(workbook.createDataFormat().getFormat("yyyy-MM-dd"));
                return true;
            } catch (Exception e) {
                return false;
            }
        }
        
        // 布尔值
        if ("true".equalsIgnoreCase(text) || "false".equalsIgnoreCase(text)) {
            cell.setCellValue(Boolean.parseBoolean(text));
            return true;
        }
        
        return false;
    }
}
