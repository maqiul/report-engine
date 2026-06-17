package com.reportengine.lib.exporter;

import com.itextpdf.kernel.pdf.PdfDocument;
import com.itextpdf.kernel.pdf.PdfReader;
import com.itextpdf.kernel.pdf.PdfWriter;
import com.reportengine.lib.model.RenderRequest;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.util.*;

import static org.junit.jupiter.api.Assertions.*;
import static org.assertj.core.api.Assertions.assertThat;

/**
 * PdfExporter 测试 - 用 iText 读取生成的 PDF 验证内容
 */
class PdfExporterTest {

    private PdfExporter exporter;

    @BeforeEach
    void setUp() {
        exporter = new PdfExporter();
    }

    // ==================== 基础导出 ====================

    @Test
    @DisplayName("空模板应生成有效 PDF")
    void shouldExportEmptyTemplate() throws Exception {
        RenderRequest req = new RenderRequest("{}", new HashMap<>());
        byte[] pdf = exporter.export(req);

        assertNotNull(pdf);
        assertThat(pdf.length).isGreaterThan(0);
        // 验证 PDF 头部
        String header = new String(Arrays.copyOfRange(pdf, 0, 5));
        assertEquals("%PDF-", header);
    }

    @Test
    @DisplayName("简单文本应正确渲染")
    void shouldRenderSimpleText() throws Exception {
        String template = "{\n" +
            "  \"bands\":[{\"type\":\"title\",\"height\":20,\"elements\":[" +
            "    {\"type\":\"text\",\"x\":10,\"y\":5,\"width\":100,\"height\":10,\"text\":\"销售报表\"}" +
            "  ]}]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        byte[] pdf = exporter.export(req);

        assertNotNull(pdf);
        assertThat(pdf.length).isGreaterThan(1000);
        verifyPdfIsValid(pdf);
    }

    @Test
    @DisplayName("中文文本应正确渲染（不报错）")
    void shouldRenderChineseText() throws Exception {
        String template = "{\n" +
            "  \"bands\":[{\"type\":\"title\",\"height\":20,\"elements\":[" +
            "    {\"type\":\"text\",\"x\":10,\"y\":5,\"width\":100,\"height\":10,\"text\":\"张三李四王五\"}" +
            "  ]}]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        byte[] pdf = exporter.export(req);

        assertNotNull(pdf);
        verifyPdfIsValid(pdf);
    }

    // ==================== detail 展开 ====================

    @Test
    @DisplayName("detail 数据应正确展开")
    void shouldExportDetailRows() throws Exception {
        String template = "{\n" +
            "  \"bands\":[{" +
            "    \"type\":\"detail\"," +
            "    \"height\":10," +
            "    \"dataSource\":\"orders\"," +
            "    \"elements\":[{\"type\":\"text\",\"x\":10,\"y\":0,\"width\":50,\"height\":10,\"text\":\"{{currentRow.id}}\"}]" +
            "  }]}";
        Map<String, List<Map<String, Object>>> data = new HashMap<>();
        data.put("orders", List.of(
            Map.of("id", "1"),
            Map.of("id", "2"),
            Map.of("id", "3")
        ));
        RenderRequest req = new RenderRequest(template, data);
        byte[] pdf = exporter.export(req);

        assertNotNull(pdf);
        verifyPdfIsValid(pdf);
        // 3 行数据
        try (PdfDocument doc = new PdfDocument(new PdfReader(new ByteArrayInputStream(pdf)))) {
            assertThat(doc.getNumberOfPages()).isGreaterThanOrEqualTo(1);
        }
    }

    // ==================== 多种元素类型 ====================

    @Test
    @DisplayName("line 元素应能导出")
    void shouldExportLineElement() throws Exception {
        String template = "{\n" +
            "  \"bands\":[{\"type\":\"title\",\"height\":20,\"elements\":[" +
            "    {\"type\":\"line\",\"x\":10,\"y\":10,\"width\":100,\"height\":0,\"lineWidth\":1}" +
            "  ]}]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        byte[] pdf = exporter.export(req);

        assertNotNull(pdf);
        verifyPdfIsValid(pdf);
    }

    @Test
    @DisplayName("rect 元素应能导出")
    void shouldExportRectElement() throws Exception {
        String template = "{\n" +
            "  \"bands\":[{\"type\":\"title\",\"height\":20,\"elements\":[" +
            "    {\"type\":\"rect\",\"x\":10,\"y\":5,\"width\":100,\"height\":10,\"fillColor\":\"#FF0000\"}" +
            "  ]}]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        byte[] pdf = exporter.export(req);

        assertNotNull(pdf);
        verifyPdfIsValid(pdf);
    }

    @Test
    @DisplayName("shape 元素应能导出")
    void shouldExportShapeElement() throws Exception {
        String template = "{\n" +
            "  \"bands\":[{\"type\":\"title\",\"height\":20,\"elements\":[" +
            "    {\"type\":\"shape\",\"x\":10,\"y\":5,\"width\":100,\"height\":10,\"fillColor\":\"#0000FF\"}" +
            "  ]}]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        byte[] pdf = exporter.export(req);

        assertNotNull(pdf);
        verifyPdfIsValid(pdf);
    }

    // ==================== 对齐方式 ====================

    @Test
    @DisplayName("左对齐文本应能导出")
    void shouldExportLeftAlignedText() throws Exception {
        exportWithAlignment("left");
    }

    @Test
    @DisplayName("居中对齐文本应能导出")
    void shouldExportCenterAlignedText() throws Exception {
        exportWithAlignment("center");
    }

    @Test
    @DisplayName("右对齐文本应能导出")
    void shouldExportRightAlignedText() throws Exception {
        exportWithAlignment("right");
    }

    private void exportWithAlignment(String alignment) throws Exception {
        String template = "{\n" +
            "  \"bands\":[{\"type\":\"title\",\"height\":20,\"elements\":[" +
            "    {\"type\":\"text\",\"x\":10,\"y\":5,\"width\":100,\"height\":10,\"text\":\"X\",\"alignment\":\"" + alignment + "\"}" +
            "  ]}]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        byte[] pdf = exporter.export(req);

        assertNotNull(pdf);
        verifyPdfIsValid(pdf);
    }

    // ==================== 字体 ====================

    @Test
    @DisplayName("自定义字体大小应能导出")
    void shouldExportCustomFontSize() throws Exception {
        String template = "{\n" +
            "  \"bands\":[{\"type\":\"title\",\"height\":20,\"elements\":[" +
            "    {\"type\":\"text\",\"x\":10,\"y\":5,\"width\":100,\"height\":10,\"text\":\"X\"," +
            "      \"font\":{\"family\":\"SimSun\",\"size\":18,\"bold\":true}}" +
            "  ]}]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        byte[] pdf = exporter.export(req);

        assertNotNull(pdf);
        verifyPdfIsValid(pdf);
    }

    // ==================== pageFooter 页脚 ====================

    @Test
    @DisplayName("带 pageFooter 的模板应能导出")
    void shouldExportWithPageFooter() throws Exception {
        String template = "{\n" +
            "  \"bands\":[" +
            "    {\"type\":\"title\",\"height\":20,\"elements\":[" +
            "      {\"type\":\"text\",\"x\":10,\"y\":5,\"width\":100,\"height\":10,\"text\":\"标题\"}" +
            "    ]}," +
            "    {\"type\":\"pageFooter\",\"height\":15,\"elements\":[" +
            "      {\"type\":\"text\",\"x\":0,\"y\":0,\"width\":210,\"height\":10,\"text\":\"第 {{page}} / {{totalPages}} 页\",\"alignment\":\"right\"}" +
            "    ]}" +
            "  ]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        byte[] pdf = exporter.export(req);

        assertNotNull(pdf);
        verifyPdfIsValid(pdf);
    }

    @Test
    @DisplayName("多个 pageFooter 元素应能导出")
    void shouldExportMultiplePageFooterElements() throws Exception {
        String template = "{\n" +
            "  \"bands\":[" +
            "    {\"type\":\"pageFooter\",\"height\":15,\"elements\":[" +
            "      {\"type\":\"text\",\"x\":0,\"y\":0,\"width\":100,\"height\":10,\"text\":\"左对齐\"}," +
            "      {\"type\":\"text\",\"x\":110,\"y\":0,\"width\":100,\"height\":10,\"text\":\"右对齐\",\"alignment\":\"right\"}" +
            "    ]}" +
            "  ]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        byte[] pdf = exporter.export(req);

        assertNotNull(pdf);
        verifyPdfIsValid(pdf);
    }

    // ==================== 异常处理 ====================

    @Test
    @DisplayName("无效 JSON 模板应抛异常")
    void shouldThrowOnInvalidJson() {
        RenderRequest req = new RenderRequest("{not valid", new HashMap<>());
        assertThrows(Exception.class, () -> exporter.export(req));
    }

    @Test
    @DisplayName("null data 应不报错")
    void shouldHandleNullData() throws Exception {
        String template = "{\n" +
            "  \"bands\":[{\"type\":\"title\",\"height\":20,\"elements\":[" +
            "    {\"type\":\"text\",\"x\":10,\"y\":5,\"width\":100,\"height\":10,\"text\":\"标题\"}" +
            "  ]}]}";
        RenderRequest req = new RenderRequest(template, null);
        byte[] pdf = exporter.export(req);

        assertNotNull(pdf);
        verifyPdfIsValid(pdf);
    }

    // ==================== 复杂模板 ====================

    @Test
    @DisplayName("完整报表模板（标题+表头+明细+页脚）应能导出")
    void shouldExportCompleteReport() throws Exception {
        String template = "{\n" +
            "  \"page\":{\"width\":210,\"height\":297},\n" +
            "  \"bands\":[" +
            "    {\"type\":\"title\",\"height\":25,\"elements\":[" +
            "      {\"type\":\"text\",\"x\":0,\"y\":5,\"width\":210,\"height\":15,\"text\":\"销售订单报表\"," +
            "        \"font\":{\"family\":\"SimSun\",\"size\":18,\"bold\":true},\"alignment\":\"center\"}" +
            "    ]}," +
            "    {\"type\":\"pageHeader\",\"height\":20,\"elements\":[" +
            "      {\"type\":\"text\",\"x\":10,\"y\":5,\"width\":50,\"height\":10,\"text\":\"订单号\"," +
            "        \"font\":{\"size\":11,\"bold\":true}}," +
            "      {\"type\":\"text\",\"x\":70,\"y\":5,\"width\":70,\"height\":10,\"text\":\"客户\"," +
            "        \"font\":{\"size\":11,\"bold\":true}}," +
            "      {\"type\":\"text\",\"x\":150,\"y\":5,\"width\":50,\"height\":10,\"text\":\"金额\"," +
            "        \"font\":{\"size\":11,\"bold\":true},\"alignment\":\"right\"}" +
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
            Map.of("no", "SO-001", "cust", "张三", "amt", "1234.56"),
            Map.of("no", "SO-002", "cust", "李四", "amt", "2345.67"),
            Map.of("no", "SO-003", "cust", "王五", "amt", "3456.78")
        ));
        RenderRequest req = new RenderRequest(template, data);
        byte[] pdf = exporter.export(req);

        assertNotNull(pdf);
        assertThat(pdf.length).isGreaterThan(2000);
        verifyPdfIsValid(pdf);

        // 验证 PDF 至少 1 页
        try (PdfDocument doc = new PdfDocument(new PdfReader(new ByteArrayInputStream(pdf)))) {
            assertThat(doc.getNumberOfPages()).isEqualTo(1);
        }
    }

    // ==================== 工具方法 ====================

    private void verifyPdfIsValid(byte[] pdf) {
        assertNotNull(pdf);
        assertThat(pdf.length).isGreaterThan(0);
        String header = new String(Arrays.copyOfRange(pdf, 0, 5));
        assertEquals("%PDF-", header, "PDF 头部应正确");
    }
}
