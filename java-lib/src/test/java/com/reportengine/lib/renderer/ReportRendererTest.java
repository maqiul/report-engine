package com.reportengine.lib.renderer;

import com.reportengine.lib.model.RenderRequest;
import com.reportengine.lib.model.RenderResponse;
import com.reportengine.lib.model.RenderResponse.PageInfo;
import com.reportengine.lib.model.RenderResponse.RenderedElementInfo;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import java.util.*;

import static org.junit.jupiter.api.Assertions.*;
import static org.assertj.core.api.Assertions.assertThat;

/**
 * ReportRenderer 核心渲染测试
 */
class ReportRendererTest {

    private ReportRenderer renderer;

    @BeforeEach
    void setUp() {
        renderer = new ReportRenderer();
    }

    // ==================== 基础模板解析 ====================

    @Test
    @DisplayName("空模板应返回成功 + 0 元素")
    void shouldHandleEmptyTemplate() {
        RenderRequest req = new RenderRequest("{}", new HashMap<>());
        RenderResponse resp = renderer.render(req);

        assertTrue(resp.isSuccess());
        assertNull(resp.getError());
        assertThat(resp.getPages()).hasSize(1);
        assertThat(resp.getPages().get(0).getElements()).isEmpty();
        assertEquals(1, resp.getTotalPages());
    }

    @Test
    @DisplayName("无 bands 的模板应正常处理")
    void shouldHandleTemplateWithoutBands() {
        String template = "{\"page\":{\"width\":210,\"height\":297}}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        RenderResponse resp = renderer.render(req);

        assertTrue(resp.isSuccess());
        assertEquals(210.0, resp.getPages().get(0).getWidth());
        assertEquals(297.0, resp.getPages().get(0).getHeight());
    }

    @Test
    @DisplayName("自定义页面尺寸应被解析")
    void shouldParseCustomPageSize() {
        String template = "{\"page\":{\"width\":100,\"height\":150},\"bands\":[]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        RenderResponse resp = renderer.render(req);

        assertTrue(resp.isSuccess());
        assertEquals(100.0, resp.getPages().get(0).getWidth());
        assertEquals(150.0, resp.getPages().get(0).getHeight());
    }

    // ==================== band 渲染 ====================

    @Test
    @DisplayName("简单 title band 应输出 1 个元素")
    void shouldRenderTitleBand() {
        String template = "{\n" +
            "  \"bands\":[{\"type\":\"title\",\"height\":20,\"elements\":[" +
            "    {\"type\":\"text\",\"x\":10,\"y\":0,\"width\":100,\"height\":10,\"text\":\"销售报表\"}" +
            "  ]}]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        RenderResponse resp = renderer.render(req);

        assertTrue(resp.isSuccess());
        List<RenderedElementInfo> elements = resp.getPages().get(0).getElements();
        assertThat(elements).hasSize(1);
        assertEquals("销售报表", elements.get(0).getText());
    }

    @Test
    @DisplayName("多个 band 应顺序渲染")
    void shouldRenderMultipleBandsInOrder() {
        String template = "{\n" +
            "  \"bands\":[" +
            "    {\"type\":\"title\",\"height\":20,\"elements\":[{\"type\":\"text\",\"x\":0,\"y\":0,\"width\":100,\"height\":10,\"text\":\"标题\"}]}," +
            "    {\"type\":\"pageHeader\",\"height\":15,\"elements\":[{\"type\":\"text\",\"x\":0,\"y\":0,\"width\":100,\"height\":10,\"text\":\"表头\"}]}" +
            "  ]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        RenderResponse resp = renderer.render(req);

        assertTrue(resp.isSuccess());
        List<RenderedElementInfo> elements = resp.getPages().get(0).getElements();
        assertThat(elements).hasSize(2);
        assertEquals("标题", elements.get(0).getText());
        assertEquals("表头", elements.get(1).getText());
    }

    @Test
    @DisplayName("band 元素 Y 坐标应累加 band 高度")
    void shouldAccumulateBandYOffset() {
        String template = "{\n" +
            "  \"bands\":[" +
            "    {\"type\":\"title\",\"height\":30,\"elements\":[{\"type\":\"text\",\"x\":0,\"y\":5,\"width\":100,\"height\":10,\"text\":\"A\"}]}," +
            "    {\"type\":\"pageHeader\",\"height\":20,\"elements\":[{\"type\":\"text\",\"x\":0,\"y\":5,\"width\":100,\"height\":10,\"text\":\"B\"}]}" +
            "  ]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        RenderResponse resp = renderer.render(req);

        List<RenderedElementInfo> elements = resp.getPages().get(0).getElements();
        assertEquals(5.0, elements.get(0).getY());
        assertEquals(35.0, elements.get(1).getY());
    }

    // ==================== detail band 数据展开 ====================

    @Test
    @DisplayName("detail band 应展开为多行")
    void shouldExpandDetailBandRows() {
        String template = "{\n" +
            "  \"bands\":[{" +
            "    \"type\":\"detail\"," +
            "    \"height\":10," +
            "    \"dataSource\":\"orders\"," +
            "    \"elements\":[{\"type\":\"text\",\"x\":0,\"y\":0,\"width\":50,\"height\":10,\"text\":\"{{currentRow.id}}\"}]" +
            "  }]}";
        Map<String, List<Map<String, Object>>> data = new HashMap<>();
        data.put("orders", List.of(
            Map.of("id", "1"),
            Map.of("id", "2"),
            Map.of("id", "3")
        ));
        RenderRequest req = new RenderRequest(template, data);
        RenderResponse resp = renderer.render(req);

        assertTrue(resp.isSuccess());
        List<RenderedElementInfo> elements = resp.getPages().get(0).getElements();
        assertThat(elements).hasSize(3);
        assertEquals("1", elements.get(0).getText());
        assertEquals("2", elements.get(1).getText());
        assertEquals("3", elements.get(2).getText());
    }

    @Test
    @DisplayName("detail 数据源缺失时不应报错")
    void shouldHandleMissingDataSource() {
        String template = "{\n" +
            "  \"bands\":[{" +
            "    \"type\":\"detail\"," +
            "    \"height\":10," +
            "    \"dataSource\":\"missing\"," +
            "    \"elements\":[{\"type\":\"text\",\"x\":0,\"y\":0,\"width\":50,\"height\":10,\"text\":\"x\"}]" +
            "  }]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        RenderResponse resp = renderer.render(req);

        assertTrue(resp.isSuccess());
        assertThat(resp.getPages().get(0).getElements()).isEmpty();
    }

    @Test
    @DisplayName("detail 元素 Y 坐标应按行累加")
    void shouldAccumulateDetailRowY() {
        String template = "{\n" +
            "  \"bands\":[{" +
            "    \"type\":\"detail\"," +
            "    \"height\":8," +
            "    \"dataSource\":\"items\"," +
            "    \"elements\":[{\"type\":\"text\",\"x\":0,\"y\":0,\"width\":50,\"height\":8,\"text\":\"{{currentRow.id}}\"}]" +
            "  }]}";
        Map<String, List<Map<String, Object>>> data = new HashMap<>();
        data.put("items", List.of(Map.of("id", "a"), Map.of("id", "b")));
        RenderRequest req = new RenderRequest(template, data);
        RenderResponse resp = renderer.render(req);

        List<RenderedElementInfo> elements = resp.getPages().get(0).getElements();
        assertEquals(0.0, elements.get(0).getY());
        assertEquals(8.0, elements.get(1).getY());
    }

    // ==================== 模板变量替换 ====================

    @Test
    @DisplayName("{{currentRow.xxx}} 应被替换为字段值")
    void shouldReplaceCurrentRowExpression() {
        String template = "{\n" +
            "  \"bands\":[{" +
            "    \"type\":\"detail\"," +
            "    \"height\":10," +
            "    \"dataSource\":\"users\"," +
            "    \"elements\":[{\"type\":\"text\",\"x\":0,\"y\":0,\"width\":50,\"height\":10,\"text\":\"姓名:{{currentRow.name}}\"}]" +
            "  }]}";
        Map<String, List<Map<String, Object>>> data = new HashMap<>();
        data.put("users", List.of(Map.of("name", "张三")));

        RenderRequest req = new RenderRequest(template, data);
        RenderResponse resp = renderer.render(req);

        assertEquals("姓名:张三", resp.getPages().get(0).getElements().get(0).getText());
    }

    @Test
    @DisplayName("多字段模板变量应全部替换")
    void shouldReplaceMultipleFields() {
        String template = "{\n" +
            "  \"bands\":[{" +
            "    \"type\":\"detail\"," +
            "    \"height\":10," +
            "    \"dataSource\":\"o\"," +
            "    \"elements\":[{\"type\":\"text\",\"x\":0,\"y\":0,\"width\":100,\"height\":10," +
            "      \"text\":\"{{currentRow.no}}-{{currentRow.cust}}-{{currentRow.amt}}\"}]" +
            "  }]}";
        Map<String, List<Map<String, Object>>> data = new HashMap<>();
        data.put("o", List.of(Map.of("no", "S001", "cust", "客户A", "amt", 1000)));

        RenderRequest req = new RenderRequest(template, data);
        RenderResponse resp = renderer.render(req);

        assertEquals("S001-客户A-1000", resp.getPages().get(0).getElements().get(0).getText());
    }

    @Test
    @DisplayName("无 {{}} 的纯文本应原样返回")
    void shouldReturnPlainTextAsIs() {
        String template = "{\n" +
            "  \"bands\":[{\"type\":\"title\",\"height\":10,\"elements\":[" +
            "    {\"type\":\"text\",\"x\":0,\"y\":0,\"width\":100,\"height\":10,\"text\":\"hello world\"}" +
            "  ]}]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        RenderResponse resp = renderer.render(req);

        assertEquals("hello world", resp.getPages().get(0).getElements().get(0).getText());
    }

    @Test
    @DisplayName("{{page}} 应被替换为当前页号")
    void shouldReplacePageVariable() {
        String template = "{\n" +
            "  \"bands\":[{\"type\":\"title\",\"height\":10,\"elements\":[" +
            "    {\"type\":\"text\",\"x\":0,\"y\":0,\"width\":100,\"height\":10,\"text\":\"第 {{page}} 页\"}" +
            "  ]}]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        RenderResponse resp = renderer.render(req);

        assertEquals("第 1 页", resp.getPages().get(0).getElements().get(0).getText());
    }

    @Test
    @DisplayName("{{totalPages}} 应被替换为总页数")
    void shouldReplaceTotalPagesVariable() {
        String template = "{\n" +
            "  \"bands\":[{\"type\":\"title\",\"height\":10,\"elements\":[" +
            "    {\"type\":\"text\",\"x\":0,\"y\":0,\"width\":100,\"height\":10,\"text\":\"共 {{totalPages}} 页\"}" +
            "  ]}]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        RenderResponse resp = renderer.render(req);

        assertEquals("共 1 页", resp.getPages().get(0).getElements().get(0).getText());
    }

    @Test
    @DisplayName("pageFooter band 不应在 elements 列表中")
    void shouldSkipPageFooterBand() {
        String template = "{\n" +
            "  \"bands\":[" +
            "    {\"type\":\"pageFooter\",\"height\":15,\"elements\":[" +
            "      {\"type\":\"text\",\"x\":0,\"y\":0,\"width\":100,\"height\":10,\"text\":\"第 {{page}} / {{totalPages}} 页\"}" +
            "    ]}" +
            "  ]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        RenderResponse resp = renderer.render(req);

        assertTrue(resp.isSuccess());
        assertThat(resp.getPages().get(0).getElements()).isEmpty();
    }

    @Test
    @DisplayName("非 detail band 中的元素不应有 rowData 替换")
    void shouldNotReplaceRowDataInNonDetailBand() {
        String template = "{\n" +
            "  \"bands\":[{\"type\":\"title\",\"height\":10,\"elements\":[" +
            "    {\"type\":\"text\",\"x\":0,\"y\":0,\"width\":100,\"height\":10,\"text\":\"{{currentRow.name}}\"}" +
            "  ]}]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        RenderResponse resp = renderer.render(req);

        assertEquals("{{currentRow.name}}", resp.getPages().get(0).getElements().get(0).getText());
    }

    // ==================== 元素类型 ====================

    @Test
    @DisplayName("line 元素应设置 borderColor")
    void shouldRenderLineElement() {
        String template = "{\n" +
            "  \"bands\":[{\"type\":\"title\",\"height\":10,\"elements\":[" +
            "    {\"type\":\"line\",\"x\":10,\"y\":0,\"width\":100,\"height\":0,\"color\":\"#FF0000\",\"lineWidth\":1}" +
            "  ]}]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        RenderResponse resp = renderer.render(req);

        RenderedElementInfo el = resp.getPages().get(0).getElements().get(0);
        assertEquals("line", el.getType());
        assertEquals("#FF0000", el.getBorderColor());
    }

    @Test
    @DisplayName("rect 元素应设置 backgroundColor")
    void shouldRenderRectElement() {
        String template = "{\n" +
            "  \"bands\":[{\"type\":\"title\",\"height\":10,\"elements\":[" +
            "    {\"type\":\"rect\",\"x\":10,\"y\":0,\"width\":100,\"height\":10,\"fillColor\":\"#00FF00\"}" +
            "  ]}]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        RenderResponse resp = renderer.render(req);

        RenderedElementInfo el = resp.getPages().get(0).getElements().get(0);
        assertEquals("rect", el.getType());
        assertEquals("#00FF00", el.getBackgroundColor());
    }

    @Test
    @DisplayName("barcode 元素应保留 value")
    void shouldRenderBarcodeElement() {
        String template = "{\n" +
            "  \"bands\":[{" +
            "    \"type\":\"detail\"," +
            "    \"height\":10," +
            "    \"dataSource\":\"codes\"," +
            "    \"elements\":[{\"type\":\"barcode\",\"x\":0,\"y\":0,\"width\":50,\"height\":10,\"value\":\"{{currentRow.code}}\"}]" +
            "  }]}";
        Map<String, List<Map<String, Object>>> data = new HashMap<>();
        data.put("codes", List.of(Map.of("code", "1234567890")));

        RenderRequest req = new RenderRequest(template, data);
        RenderResponse resp = renderer.render(req);

        assertEquals("1234567890", resp.getPages().get(0).getElements().get(0).getText());
    }

    // ==================== 字体 ====================

    @Test
    @DisplayName("text 元素应解析 font 配置")
    void shouldParseFontConfig() {
        String template = "{\n" +
            "  \"bands\":[{\"type\":\"title\",\"height\":10,\"elements\":[" +
            "    {\"type\":\"text\",\"x\":0,\"y\":0,\"width\":100,\"height\":10,\"text\":\"标题\"," +
            "      \"font\":{\"family\":\"SimSun\",\"size\":16,\"bold\":true,\"italic\":false,\"color\":\"#FF0000\"}}" +
            "  ]}]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        RenderResponse resp = renderer.render(req);

        RenderedElementInfo el = resp.getPages().get(0).getElements().get(0);
        assertNotNull(el.getFont());
        assertEquals("SimSun", el.getFont().getFamily());
        assertEquals(16, el.getFont().getSize());
        assertTrue(el.getFont().isBold());
        assertEquals("#FF0000", el.getFont().getColor());
    }

    @Test
    @DisplayName("默认对齐方式应为 left")
    void shouldDefaultAlignmentToLeft() {
        String template = "{\n" +
            "  \"bands\":[{\"type\":\"title\",\"height\":10,\"elements\":[" +
            "    {\"type\":\"text\",\"x\":0,\"y\":0,\"width\":100,\"height\":10,\"text\":\"x\"}" +
            "  ]}]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        RenderResponse resp = renderer.render(req);

        assertEquals("left", resp.getPages().get(0).getElements().get(0).getAlignment());
    }

    @Test
    @DisplayName("自定义对齐方式应被解析")
    void shouldParseCustomAlignment() {
        String template = "{\n" +
            "  \"bands\":[{\"type\":\"title\",\"height\":10,\"elements\":[" +
            "    {\"type\":\"text\",\"x\":0,\"y\":0,\"width\":100,\"height\":10,\"text\":\"x\",\"alignment\":\"center\"}" +
            "  ]}]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        RenderResponse resp = renderer.render(req);

        assertEquals("center", resp.getPages().get(0).getElements().get(0).getAlignment());
    }

    // ==================== 默认值 ====================

    @Test
    @DisplayName("元素缺省坐标应使用默认值")
    void shouldUseDefaultCoordinates() {
        String template = "{\n" +
            "  \"bands\":[{\"type\":\"title\",\"height\":10,\"elements\":[" +
            "    {\"type\":\"text\",\"text\":\"x\"}" +
            "  ]}]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        RenderResponse resp = renderer.render(req);

        RenderedElementInfo el = resp.getPages().get(0).getElements().get(0);
        assertEquals(0.0, el.getX());
        assertEquals(0.0, el.getY());
        assertEquals(100.0, el.getWidth());
        assertEquals(10.0, el.getHeight());
    }

    @Test
    @DisplayName("无效 JSON 应返回失败 + 错误信息")
    void shouldHandleInvalidJson() {
        RenderRequest req = new RenderRequest("{invalid json}", new HashMap<>());
        RenderResponse resp = renderer.render(req);

        assertFalse(resp.isSuccess());
        assertNotNull(resp.getError());
        assertThat(resp.getError()).contains("渲染失败");
    }

    // ==================== 边界场景 ====================

    @Test
    @DisplayName("band 缺省 type 应按非 detail 处理")
    void shouldHandleBandWithoutType() {
        String template = "{\n" +
            "  \"bands\":[{\"height\":10,\"elements\":[" +
            "    {\"type\":\"text\",\"x\":0,\"y\":0,\"width\":50,\"height\":10,\"text\":\"x\"}" +
            "  ]}]}";
        RenderRequest req = new RenderRequest(template, new HashMap<>());
        RenderResponse resp = renderer.render(req);

        assertTrue(resp.isSuccess());
        assertThat(resp.getPages().get(0).getElements()).hasSize(1);
    }

    @Test
    @DisplayName("null 字段值应替换为空字符串")
    void shouldReplaceNullValueWithEmptyString() {
        String template = "{\n" +
            "  \"bands\":[{" +
            "    \"type\":\"detail\"," +
            "    \"height\":10," +
            "    \"dataSource\":\"d\"," +
            "    \"elements\":[{\"type\":\"text\",\"x\":0,\"y\":0,\"width\":50,\"height\":10,\"text\":\"{{currentRow.x}}\"}]" +
            "  }]}";
        Map<String, List<Map<String, Object>>> data = new HashMap<>();
        Map<String, Object> row = new HashMap<>();
        row.put("x", null);
        data.put("d", List.of(row));

        RenderRequest req = new RenderRequest(template, data);
        RenderResponse resp = renderer.render(req);

        assertEquals("", resp.getPages().get(0).getElements().get(0).getText());
    }
}
