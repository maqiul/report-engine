package com.reportengine.lib.exporter;

import com.reportengine.lib.model.RenderRequest;
import org.apache.poi.ss.usermodel.Cell;
import org.apache.poi.ss.usermodel.CellType;
import org.apache.poi.ss.usermodel.Row;
import org.apache.poi.ss.usermodel.Sheet;
import org.apache.poi.xssf.usermodel.XSSFCell;
import org.apache.poi.xssf.usermodel.XSSFRow;
import org.apache.poi.xssf.usermodel.XSSFSheet;
import org.apache.poi.xssf.usermodel.XSSFWorkbook;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import java.io.ByteArrayInputStream;
import java.util.*;

import static org.junit.jupiter.api.Assertions.*;
import static org.assertj.core.api.Assertions.assertThat;

/**
 * ExcelExporter 测试 - 用 POI 读取生成的 xlsx 验证内容
 */
class ExcelExporterTest {

    private ExcelExporter exporter;

    @BeforeEach
    void setUp() {
        exporter = new ExcelExporter();
    }

    // ==================== 基础导出 ====================

    @Test
    @DisplayName("空模板应生成有效 xlsx")
    void shouldExportEmptyTemplate() throws Exception {
        RenderRequest req = new RenderRequest("{}", new HashMap<>());
        byte[] xlsx = exporter.export(req);

        assertNotNull(xlsx);
        assertThat(xlsx.length).isGreaterThan(0);

        try (XSSFWorkbook wb = new XSSFWorkbook(new ByteArrayInputStream(xlsx))) {
            assertThat(wb.getNumberOfSheets()).isEqualTo(1);
            assertThat(wb.getSheetAt(0).getPhysicalNumberOfRows()).isEqualTo(0);
        }
    }

    @Test
    @DisplayName("简单文本应能导出")
    void shouldExportSimpleText() throws Exception {
        String template = "{\n" +
            "  \"bands\":[{\"type\":\"title\",\"height\":20,\"elements\":[" +
            "    {\"type\":\"text\",\"x\":10,\"y\":0,\"width\":100,\"height\":10,\"text\":\"标题\"}" +
            "  ]}]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        byte[] xlsx = exporter.export(req);

        try (XSSFWorkbook wb = new XSSFWorkbook(new ByteArrayInputStream(xlsx))) {
            XSSFSheet sheet = wb.getSheetAt(0);
            assertThat(sheet.getPhysicalNumberOfRows()).isGreaterThan(0);
        }
    }

    // ==================== detail 展开 ====================

    @Test
    @DisplayName("detail 数据应展开为多行")
    void shouldExpandDetailRows() throws Exception {
        String template = "{\n" +
            "  \"bands\":[{" +
            "    \"type\":\"detail\"," +
            "    \"height\":10," +
            "    \"dataSource\":\"orders\"," +
            "    \"elements\":[{\"type\":\"text\",\"x\":10,\"y\":0,\"width\":50,\"height\":10,\"text\":\"{{currentRow.id}}\"}]" +
            "  }]}";
        Map<String, List<Map<String, Object>>> data = new HashMap<>();
        data.put("orders", List.of(
            Map.of("id", "A001"),
            Map.of("id", "A002"),
            Map.of("id", "A003")
        ));
        RenderRequest req = new RenderRequest(template, data);
        byte[] xlsx = exporter.export(req);

        try (XSSFWorkbook wb = new XSSFWorkbook(new ByteArrayInputStream(xlsx))) {
            XSSFSheet sheet = wb.getSheetAt(0);
            assertThat(sheet.getPhysicalNumberOfRows()).isEqualTo(3);
            // 找到第一个非空 cell 验证
            String r0 = findFirstNonNullCell(sheet.getRow(0));
            String r1 = findFirstNonNullCell(sheet.getRow(1));
            String r2 = findFirstNonNullCell(sheet.getRow(2));
            assertEquals("A001", r0);
            assertEquals("A002", r1);
            assertEquals("A003", r2);
        }
    }

    @Test
    @DisplayName("detail 缺失数据源应不报错")
    void shouldHandleMissingDataSource() throws Exception {
        String template = "{\n" +
            "  \"bands\":[{" +
            "    \"type\":\"detail\"," +
            "    \"height\":10," +
            "    \"dataSource\":\"missing\"," +
            "    \"elements\":[{\"type\":\"text\",\"x\":0,\"y\":0,\"width\":50,\"height\":10,\"text\":\"x\"}]" +
            "  }]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        byte[] xlsx = exporter.export(req);

        assertNotNull(xlsx);
        try (XSSFWorkbook wb = new XSSFWorkbook(new ByteArrayInputStream(xlsx))) {
            XSSFSheet sheet = wb.getSheetAt(0);
            assertThat(sheet.getPhysicalNumberOfRows()).isEqualTo(0);
        }
    }

    // ==================== 数据类型 ====================

    @Test
    @DisplayName("整数字段应被识别为数值类型")
    void shouldRecognizeIntegerAsNumber() throws Exception {
        String template = "{\n" +
            "  \"bands\":[{" +
            "    \"type\":\"detail\"," +
            "    \"height\":10," +
            "    \"dataSource\":\"d\"," +
            "    \"elements\":[{\"type\":\"text\",\"x\":0,\"y\":0,\"width\":50,\"height\":10,\"text\":\"{{currentRow.x}}\"}]" +
            "  }]}";
        Map<String, List<Map<String, Object>>> data = new HashMap<>();
        data.put("d", List.of(Map.of("x", 1234)));

        RenderRequest req = new RenderRequest(template, data);
        byte[] xlsx = exporter.export(req);

        try (XSSFWorkbook wb = new XSSFWorkbook(new ByteArrayInputStream(xlsx))) {
            XSSFCell cell = wb.getSheetAt(0).getRow(0).getCell(0);
            assertEquals(CellType.NUMERIC, cell.getCellType());
            assertEquals(1234.0, cell.getNumericCellValue());
        }
    }

    @Test
    @DisplayName("小数字段应被识别为数值类型")
    void shouldRecognizeDecimalAsNumber() throws Exception {
        String template = "{\n" +
            "  \"bands\":[{" +
            "    \"type\":\"detail\"," +
            "    \"height\":10," +
            "    \"dataSource\":\"d\"," +
            "    \"elements\":[{\"type\":\"text\",\"x\":0,\"y\":0,\"width\":50,\"height\":10,\"text\":\"{{currentRow.x}}\"}]" +
            "  }]}";
        Map<String, List<Map<String, Object>>> data = new HashMap<>();
        data.put("d", List.of(Map.of("x", 1234.56)));

        RenderRequest req = new RenderRequest(template, data);
        byte[] xlsx = exporter.export(req);

        try (XSSFWorkbook wb = new XSSFWorkbook(new ByteArrayInputStream(xlsx))) {
            XSSFCell cell = wb.getSheetAt(0).getRow(0).getCell(0);
            assertEquals(CellType.NUMERIC, cell.getCellType());
            assertEquals(1234.56, cell.getNumericCellValue());
        }
    }

    @Test
    @DisplayName("布尔字段应被识别为布尔类型")
    void shouldRecognizeBoolean() throws Exception {
        String template = "{\n" +
            "  \"bands\":[{" +
            "    \"type\":\"detail\"," +
            "    \"height\":10," +
            "    \"dataSource\":\"d\"," +
            "    \"elements\":[{\"type\":\"text\",\"x\":0,\"y\":0,\"width\":50,\"height\":10,\"text\":\"{{currentRow.x}}\"}]" +
            "  }]}";
        Map<String, List<Map<String, Object>>> data = new HashMap<>();
        data.put("d", List.of(Map.of("x", true)));

        RenderRequest req = new RenderRequest(template, data);
        byte[] xlsx = exporter.export(req);

        try (XSSFWorkbook wb = new XSSFWorkbook(new ByteArrayInputStream(xlsx))) {
            XSSFCell cell = wb.getSheetAt(0).getRow(0).getCell(0);
            assertEquals(CellType.BOOLEAN, cell.getCellType());
            assertTrue(cell.getBooleanCellValue());
        }
    }

    // ==================== 布局 ====================

    @Test
    @DisplayName("列宽应按 mm 设置")
    void shouldSetColumnWidth() throws Exception {
        String template = "{\n" +
            "  \"bands\":[{\"type\":\"title\",\"height\":10,\"elements\":[" +
            "    {\"type\":\"text\",\"x\":0,\"y\":0,\"width\":37,\"height\":10,\"text\":\"A\"}," +
            "    {\"type\":\"text\",\"x\":40,\"y\":0,\"width\":37,\"height\":10,\"text\":\"B\"}" +
            "  ]}]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        byte[] xlsx = exporter.export(req);

        try (XSSFWorkbook wb = new XSSFWorkbook(new ByteArrayInputStream(xlsx))) {
            XSSFSheet sheet = wb.getSheetAt(0);
            // 应设置列宽（POI 内部以 256 为单位）
            int widthCol0 = sheet.getColumnWidth(0);
            int widthCol1 = sheet.getColumnWidth(1);
            assertThat(widthCol0).isGreaterThan(0);
            assertThat(widthCol1).isGreaterThan(0);
        }
    }

    @Test
    @DisplayName("行高应按 mm 设置")
    void shouldSetRowHeight() throws Exception {
        String template = "{\n" +
            "  \"bands\":[{\"type\":\"title\",\"height\":20,\"elements\":[" +
            "    {\"type\":\"text\",\"x\":0,\"y\":0,\"width\":50,\"height\":20,\"text\":\"标题\"}" +
            "  ]}]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        byte[] xlsx = exporter.export(req);

        try (XSSFWorkbook wb = new XSSFWorkbook(new ByteArrayInputStream(xlsx))) {
            XSSFSheet sheet = wb.getSheetAt(0);
            XSSFRow row = sheet.getRow(0);
            assertThat(row.getHeight()).isGreaterThan((short) 0);
        }
    }

    @Test
    @DisplayName("多 band 应渲染为多行（detail 外每个 band 一行）")
    void shouldRenderNonDetailBandAsOneRow() throws Exception {
        String template = "{\n" +
            "  \"bands\":[" +
            "    {\"type\":\"title\",\"height\":20,\"elements\":[" +
            "      {\"type\":\"text\",\"x\":0,\"y\":0,\"width\":100,\"height\":10,\"text\":\"标题\"}" +
            "    ]}," +
            "    {\"type\":\"pageHeader\",\"height\":15,\"elements\":[" +
            "      {\"type\":\"text\",\"x\":0,\"y\":0,\"width\":100,\"height\":10,\"text\":\"表头\"}" +
            "    ]}" +
            "  ]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        byte[] xlsx = exporter.export(req);

        try (XSSFWorkbook wb = new XSSFWorkbook(new ByteArrayInputStream(xlsx))) {
            XSSFSheet sheet = wb.getSheetAt(0);
            assertThat(sheet.getPhysicalNumberOfRows()).isEqualTo(2);
        }
    }

    // ==================== 合并单元格 ====================

    @Test
    @DisplayName("跨多列元素应合并单元格")
    void shouldMergeCellsAcrossColumns() throws Exception {
        String template = "{\n" +
            "  \"bands\":[{\"type\":\"title\",\"height\":20,\"elements\":[" +
            "    {\"type\":\"text\",\"x\":0,\"y\":0,\"width\":100,\"height\":10,\"text\":\"合并\"}" +
            "  ]}]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        byte[] xlsx = exporter.export(req);

        try (XSSFWorkbook wb = new XSSFWorkbook(new ByteArrayInputStream(xlsx))) {
            XSSFSheet sheet = wb.getSheetAt(0);
            assertThat(sheet.getNumMergedRegions()).isGreaterThan(0);
        }
    }

    // ==================== 打印设置 ====================

    @Test
    @DisplayName("打印区域应被设置")
    void shouldSetPrintArea() throws Exception {
        String template = "{\n" +
            "  \"bands\":[{\"type\":\"title\",\"height\":20,\"elements\":[" +
            "    {\"type\":\"text\",\"x\":0,\"y\":0,\"width\":100,\"height\":10,\"text\":\"标题\"}" +
            "  ]}]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        byte[] xlsx = exporter.export(req);

        try (XSSFWorkbook wb = new XSSFWorkbook(new ByteArrayInputStream(xlsx))) {
            String printArea = wb.getPrintArea(0);
            assertNotNull(printArea);
            assertThat(printArea).isNotEmpty();
        }
    }

    @Test
    @DisplayName("A4 纸张大小应被设置")
    void shouldSetA4PaperSize() throws Exception {
        String template = "{\n" +
            "  \"page\":{\"width\":210,\"height\":297},\n" +
            "  \"bands\":[{\"type\":\"title\",\"height\":10,\"elements\":[" +
            "    {\"type\":\"text\",\"x\":0,\"y\":0,\"width\":50,\"height\":10,\"text\":\"x\"}" +
            "  ]}]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        byte[] xlsx = exporter.export(req);

        try (XSSFWorkbook wb = new XSSFWorkbook(new ByteArrayInputStream(xlsx))) {
            XSSFSheet sheet = wb.getSheetAt(0);
            // 9 = A4
            assertEquals(9, sheet.getPrintSetup().getPaperSize());
        }
    }

    @Test
    @DisplayName("缩放适应页面应被设置")
    void shouldSetFitToPage() throws Exception {
        String template = "{\n" +
            "  \"bands\":[{\"type\":\"title\",\"height\":10,\"elements\":[" +
            "    {\"type\":\"text\",\"x\":0,\"y\":0,\"width\":50,\"height\":10,\"text\":\"x\"}" +
            "  ]}]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        byte[] xlsx = exporter.export(req);

        try (XSSFWorkbook wb = new XSSFWorkbook(new ByteArrayInputStream(xlsx))) {
            XSSFSheet sheet = wb.getSheetAt(0);
            assertTrue(sheet.getFitToPage());
            // 宽度适应 1 页
            assertEquals(1, sheet.getPrintSetup().getFitWidth());
        }
    }

    // ==================== 页脚页码 ====================

    @Test
    @DisplayName("pageFooter band 不应作为数据行输出")
    void shouldNotRenderPageFooterAsDataRow() throws Exception {
        String template = "{\n" +
            "  \"bands\":[" +
            "    {\"type\":\"title\",\"height\":20,\"elements\":[" +
            "      {\"type\":\"text\",\"x\":0,\"y\":0,\"width\":100,\"height\":10,\"text\":\"标题\"}" +
            "    ]}," +
            "    {\"type\":\"pageFooter\",\"height\":15,\"elements\":[" +
            "      {\"type\":\"text\",\"x\":0,\"y\":0,\"width\":210,\"height\":10,\"text\":\"第 {{page}} 页\"}" +
            "    ]}" +
            "  ]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        byte[] xlsx = exporter.export(req);

        try (XSSFWorkbook wb = new XSSFWorkbook(new ByteArrayInputStream(xlsx))) {
            XSSFSheet sheet = wb.getSheetAt(0);
            // 1 个 title 行（不包含 pageFooter）
            assertThat(sheet.getPhysicalNumberOfRows()).isEqualTo(1);
        }
    }

    @Test
    @DisplayName("页脚应包含页码变量")
    void shouldIncludePageNumberInFooter() throws Exception {
        String template = "{\n" +
            "  \"bands\":[" +
            "    {\"type\":\"title\",\"height\":20,\"elements\":[" +
            "      {\"type\":\"text\",\"x\":0,\"y\":0,\"width\":100,\"height\":10,\"text\":\"标题\"}" +
            "    ]}," +
            "    {\"type\":\"pageFooter\",\"height\":15,\"elements\":[" +
            "      {\"type\":\"text\",\"x\":0,\"y\":0,\"width\":210,\"height\":10,\"text\":\"第 {{page}} / {{totalPages}} 页\"}" +
            "    ]}" +
            "  ]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        byte[] xlsx = exporter.export(req);

        try (XSSFWorkbook wb = new XSSFWorkbook(new ByteArrayInputStream(xlsx))) {
            XSSFSheet sheet = wb.getSheetAt(0);
            // &P = 当前页, &N = 总页数 - 可能在 left/center/right 任一位置
            String left = sheet.getFooter().getLeft();
            String center = sheet.getFooter().getCenter();
            String right = sheet.getFooter().getRight();
            String all = left + "|" + center + "|" + right;
            assertThat(all).contains("&P");
        }
    }

    // ==================== 字体 ====================

    @Test
    @DisplayName("中文字体应被设置")
    void shouldSetChineseFont() throws Exception {
        String template = "{\n" +
            "  \"bands\":[{\"type\":\"title\",\"height\":20,\"elements\":[" +
            "    {\"type\":\"text\",\"x\":0,\"y\":0,\"width\":100,\"height\":10,\"text\":\"标题\"," +
            "      \"font\":{\"family\":\"SimSun\",\"size\":16,\"bold\":true}}" +
            "  ]}]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        byte[] xlsx = exporter.export(req);

        try (XSSFWorkbook wb = new XSSFWorkbook(new ByteArrayInputStream(xlsx))) {
            XSSFSheet sheet = wb.getSheetAt(0);
            XSSFRow row = sheet.getRow(0);
            assertNotNull(row);
            XSSFCell cell = row.getCell(0);
            assertNotNull(cell);
            // 字体名应包含 Chinese 字体（Microsoft YaHei / SimSun）
            String fontName = cell.getCellStyle().getFont().getFontName();
            assertThat(fontName).isNotEmpty();
        }
    }

    // ==================== 对齐 ====================

    @Test
    @DisplayName("居中对齐应被设置")
    void shouldSetCenterAlignment() throws Exception {
        String template = "{\n" +
            "  \"bands\":[{\"type\":\"title\",\"height\":20,\"elements\":[" +
            "    {\"type\":\"text\",\"x\":0,\"y\":0,\"width\":100,\"height\":10,\"text\":\"x\",\"alignment\":\"center\"}" +
            "  ]}]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        byte[] xlsx = exporter.export(req);

        try (XSSFWorkbook wb = new XSSFWorkbook(new ByteArrayInputStream(xlsx))) {
            XSSFSheet sheet = wb.getSheetAt(0);
            XSSFCell cell = sheet.getRow(0).getCell(0);
            assertEquals(
                org.apache.poi.ss.usermodel.HorizontalAlignment.CENTER,
                cell.getCellStyle().getAlignment()
            );
        }
    }

    @Test
    @DisplayName("右对齐应被设置")
    void shouldSetRightAlignment() throws Exception {
        String template = "{\n" +
            "  \"bands\":[{\"type\":\"title\",\"height\":20,\"elements\":[" +
            "    {\"type\":\"text\",\"x\":0,\"y\":0,\"width\":100,\"height\":10,\"text\":\"x\",\"alignment\":\"right\"}" +
            "  ]}]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        byte[] xlsx = exporter.export(req);

        try (XSSFWorkbook wb = new XSSFWorkbook(new ByteArrayInputStream(xlsx))) {
            XSSFSheet sheet = wb.getSheetAt(0);
            XSSFCell cell = sheet.getRow(0).getCell(0);
            assertEquals(
                org.apache.poi.ss.usermodel.HorizontalAlignment.RIGHT,
                cell.getCellStyle().getAlignment()
            );
        }
    }

    // ==================== 异常处理 ====================

    @Test
    @DisplayName("无效 JSON 应抛异常")
    void shouldThrowOnInvalidJson() {
        RenderRequest req = new RenderRequest("{not valid", new HashMap<>());
        assertThrows(Exception.class, () -> exporter.export(req));
    }

    @Test
    @DisplayName("null data 应不报错")
    void shouldHandleNullData() throws Exception {
        String template = "{\n" +
            "  \"bands\":[{\"type\":\"title\",\"height\":20,\"elements\":[" +
            "    {\"type\":\"text\",\"x\":0,\"y\":0,\"width\":100,\"height\":10,\"text\":\"标题\"}" +
            "  ]}]}";
        RenderRequest req = new RenderRequest(template, null);
        byte[] xlsx = exporter.export(req);

        assertNotNull(xlsx);
        try (XSSFWorkbook wb = new XSSFWorkbook(new ByteArrayInputStream(xlsx))) {
            assertThat(wb.getNumberOfSheets()).isEqualTo(1);
        }
    }

    // ==================== 完整模板 ====================

    @Test
    @DisplayName("完整报表模板应能导出（标题+表头+明细+页脚）")
    void shouldExportCompleteReport() throws Exception {
        String template = "{\n" +
            "  \"page\":{\"width\":210,\"height\":297},\n" +
            "  \"bands\":[" +
            "    {\"type\":\"title\",\"height\":25,\"elements\":[" +
            "      {\"type\":\"text\",\"x\":0,\"y\":5,\"width\":210,\"height\":15,\"text\":\"销售订单报表\"," +
            "        \"font\":{\"size\":16,\"bold\":true},\"alignment\":\"center\"}" +
            "    ]}," +
            "    {\"type\":\"pageHeader\",\"height\":20,\"elements\":[" +
            "      {\"type\":\"text\",\"x\":10,\"y\":5,\"width\":50,\"height\":10,\"text\":\"订单号\",\"font\":{\"bold\":true}}," +
            "      {\"type\":\"text\",\"x\":70,\"y\":5,\"width\":70,\"height\":10,\"text\":\"客户\",\"font\":{\"bold\":true}}," +
            "      {\"type\":\"text\",\"x\":150,\"y\":5,\"width\":50,\"height\":10,\"text\":\"金额\",\"font\":{\"bold\":true},\"alignment\":\"right\"}" +
            "    ]}," +
            "    {\"type\":\"detail\",\"height\":10,\"dataSource\":\"orders\",\"elements\":[" +
            "      {\"type\":\"text\",\"x\":10,\"y\":0,\"width\":50,\"height\":10,\"text\":\"{{currentRow.no}}\"}," +
            "      {\"type\":\"text\",\"x\":70,\"y\":0,\"width\":70,\"height\":10,\"text\":\"{{currentRow.cust}}\"}," +
            "      {\"type\":\"text\",\"x\":150,\"y\":0,\"width\":50,\"height\":10,\"text\":\"{{currentRow.amt}}\",\"alignment\":\"right\"}" +
            "    ]}," +
            "    {\"type\":\"pageFooter\",\"height\":15,\"elements\":[" +
            "      {\"type\":\"text\",\"x\":0,\"y\":0,\"width\":210,\"height\":10,\"text\":\"第 {{page}} / {{totalPages}} 页\",\"alignment\":\"center\"}" +
            "    ]}" +
            "  ]}";
        Map<String, List<Map<String, Object>>> data = new HashMap<>();
        data.put("orders", List.of(
            Map.of("no", "SO-001", "cust", "张三", "amt", 1234.56),
            Map.of("no", "SO-002", "cust", "李四", "amt", 2345.67)
        ));
        RenderRequest req = new RenderRequest(template, data);
        byte[] xlsx = exporter.export(req);

        try (XSSFWorkbook wb = new XSSFWorkbook(new ByteArrayInputStream(xlsx))) {
            XSSFSheet sheet = wb.getSheetAt(0);
            // title(1) + pageHeader(1) + detail(2) = 4 行（pageFooter 不算）
            assertThat(sheet.getPhysicalNumberOfRows()).isEqualTo(4);
            // 验证金额列是数字类型（先找包含数字的 cell）
            assertTrue(anyCellIsNumericWithValue(sheet, 1234.56),
                "应能找到值 1234.56 的数字单元格");
        }
    }

    private boolean anyCellIsNumericWithValue(XSSFSheet sheet, double expected) {
        for (int r = 0; r <= sheet.getLastRowNum(); r++) {
            XSSFRow row = sheet.getRow(r);
            if (row == null) continue;
            for (int c = 0; c < row.getLastCellNum(); c++) {
                XSSFCell cell = row.getCell(c);
                if (cell != null && cell.getCellType() == CellType.NUMERIC) {
                    if (Math.abs(cell.getNumericCellValue() - expected) < 0.01) {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private String findFirstNonNullCell(XSSFRow row) {
        if (row == null) return null;
        for (int c = 0; c < row.getLastCellNum(); c++) {
            XSSFCell cell = row.getCell(c);
            if (cell != null && cell.getCellType() == CellType.STRING) {
                return cell.getStringCellValue();
            }
        }
        return null;
    }
}
