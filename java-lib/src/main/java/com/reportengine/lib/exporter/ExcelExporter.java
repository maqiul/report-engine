package com.reportengine.lib.exporter;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.reportengine.lib.model.RenderRequest;
import org.apache.poi.xssf.usermodel.XSSFCell;
import org.apache.poi.xssf.usermodel.XSSFCellStyle;
import org.apache.poi.xssf.usermodel.XSSFFont;
import org.apache.poi.xssf.usermodel.XSSFRow;
import org.apache.poi.xssf.usermodel.XSSFSheet;
import org.apache.poi.xssf.usermodel.XSSFWorkbook;

import java.io.ByteArrayOutputStream;
import java.util.List;
import java.util.Map;

/**
 * Excel 导出器
 * 
 * 使用示例：
 * <pre>
 * ExcelExporter exporter = new ExcelExporter();
 * byte[] excelBytes = exporter.export(request);
 * </pre>
 */
public class ExcelExporter {

    private final ObjectMapper objectMapper = new ObjectMapper();

    /**
     * 导出 Excel
     */
    public byte[] export(RenderRequest request) throws Exception {
        JsonNode template = objectMapper.readTree(request.getTemplateJson());
        Map<String, List<Map<String, Object>>> data = request.getData();
        
        ByteArrayOutputStream baos = new ByteArrayOutputStream();
        XSSFWorkbook workbook = new XSSFWorkbook();
        
        // 查找 detail band 并输出数据
        if (template.has("bands")) {
            for (JsonNode band : template.get("bands")) {
                if ("detail".equals(band.get("type").asText()) && band.has("dataSource")) {
                    String dsName = band.get("dataSource").asText();
                    List<Map<String, Object>> rows = data.getOrDefault(dsName, new java.util.ArrayList<>());
                    
                    if (!rows.isEmpty()) {
                        XSSFSheet sheet = workbook.createSheet("Report");
                        
                        // 表头样式
                        XSSFCellStyle headerStyle = workbook.createCellStyle();
                        XSSFFont headerFont = workbook.createFont();
                        headerFont.setBold(true);
                        headerStyle.setFont(headerFont);
                        
                        // 表头
                        XSSFRow headerRow = sheet.createRow(0);
                        int colIdx = 0;
                        for (String key : rows.get(0).keySet()) {
                            XSSFCell cell = headerRow.createCell(colIdx++);
                            cell.setCellValue(key);
                            cell.setCellStyle(headerStyle);
                        }
                        
                        // 数据行
                        int rowIdx = 1;
                        for (Map<String, Object> rowData : rows) {
                            XSSFRow row = sheet.createRow(rowIdx++);
                            colIdx = 0;
                            for (Object value : rowData.values()) {
                                XSSFCell cell = row.createCell(colIdx++);
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
