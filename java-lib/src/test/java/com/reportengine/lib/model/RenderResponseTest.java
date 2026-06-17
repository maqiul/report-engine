package com.reportengine.lib.model;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.reportengine.lib.model.RenderResponse.FontInfo;
import com.reportengine.lib.model.RenderResponse.PageInfo;
import com.reportengine.lib.model.RenderResponse.RenderedElementInfo;
import org.junit.jupiter.api.Test;

import java.util.List;

import static org.junit.jupiter.api.Assertions.*;
import static org.assertj.core.api.Assertions.assertThat;

/**
 * RenderResponse 模型测试
 */
class RenderResponseTest {

    @Test
    void shouldCreateEmptyResponse() {
        RenderResponse r = new RenderResponse();
        assertFalse(r.isSuccess());
        assertNull(r.getError());
        assertNull(r.getPages());
        assertEquals(0, r.getTotalPages());
    }

    @Test
    void shouldSetAndGetSuccess() {
        RenderResponse r = new RenderResponse();
        r.setSuccess(true);
        assertTrue(r.isSuccess());
        r.setSuccess(false);
        assertFalse(r.isSuccess());
    }

    @Test
    void shouldSetAndGetError() {
        RenderResponse r = new RenderResponse();
        r.setError("模板错误");
        assertEquals("模板错误", r.getError());
    }

    @Test
    void shouldSetAndGetTotalPages() {
        RenderResponse r = new RenderResponse();
        r.setTotalPages(5);
        assertEquals(5, r.getTotalPages());
    }

    @Test
    void shouldSetAndGetPages() {
        RenderResponse r = new RenderResponse();
        PageInfo page = new PageInfo();
        page.setPageNumber(1);
        r.setPages(List.of(page));
        assertThat(r.getPages()).hasSize(1);
    }

    // --- PageInfo ---

    @Test
    void pageInfoShouldSetAndGetAllFields() {
        PageInfo page = new PageInfo();
        page.setPageNumber(2);
        page.setWidth(210);
        page.setHeight(297);
        page.setElements(List.of());

        assertEquals(2, page.getPageNumber());
        assertEquals(210.0, page.getWidth());
        assertEquals(297.0, page.getHeight());
        assertNotNull(page.getElements());
    }

    // --- RenderedElementInfo ---

    @Test
    void elementInfoShouldSetAndGetAllFields() {
        RenderedElementInfo el = new RenderedElementInfo();
        el.setType("text");
        el.setX(10);
        el.setY(20);
        el.setWidth(100);
        el.setHeight(15);
        el.setText("标题");
        el.setAlignment("center");
        el.setBackgroundColor("#FFFFFF");
        el.setBorderColor("#000000");
        el.setBorderWidth(0.5);

        FontInfo font = new FontInfo();
        font.setFamily("Microsoft YaHei");
        font.setSize(12);
        font.setBold(true);
        el.setFont(font);

        assertEquals("text", el.getType());
        assertEquals(10.0, el.getX());
        assertEquals(20.0, el.getY());
        assertEquals(100.0, el.getWidth());
        assertEquals(15.0, el.getHeight());
        assertEquals("标题", el.getText());
        assertEquals("center", el.getAlignment());
        assertEquals(0.5, el.getBorderWidth());
        assertNotNull(el.getFont());
        assertTrue(el.getFont().isBold());
    }

    // --- FontInfo ---

    @Test
    void fontInfoShouldDefaultToFalse() {
        FontInfo font = new FontInfo();
        assertFalse(font.isBold());
        assertFalse(font.isItalic());
        assertFalse(font.isUnderline());
    }

    @Test
    void fontInfoShouldSetAndGetAllFields() {
        FontInfo font = new FontInfo();
        font.setFamily("SimSun");
        font.setSize(14);
        font.setBold(true);
        font.setItalic(true);
        font.setUnderline(true);
        font.setColor("#FF0000");

        assertEquals("SimSun", font.getFamily());
        assertEquals(14, font.getSize());
        assertTrue(font.isBold());
        assertTrue(font.isItalic());
        assertTrue(font.isUnderline());
        assertEquals("#FF0000", font.getColor());
    }

    // --- JSON 序列化 ---

    @Test
    void shouldSerializeResponseToJson() throws Exception {
        ObjectMapper mapper = new ObjectMapper();
        RenderResponse r = new RenderResponse();
        r.setSuccess(true);
        r.setTotalPages(1);

        String json = mapper.writeValueAsString(r);
        assertThat(json).contains("\"success\":true");
        assertThat(json).contains("\"totalPages\":1");
    }

    @Test
    void shouldDeserializeResponseFromJson() throws Exception {
        ObjectMapper mapper = new ObjectMapper();
        String json = "{\"success\":true,\"error\":null,\"pages\":[],\"totalPages\":3}";

        RenderResponse r = mapper.readValue(json, RenderResponse.class);
        assertTrue(r.isSuccess());
        assertEquals(3, r.getTotalPages());
    }

    @Test
    void shouldRoundTripWithElementInfo() throws Exception {
        ObjectMapper mapper = new ObjectMapper();
        RenderedElementInfo el = new RenderedElementInfo();
        el.setType("text");
        el.setX(10.5);
        el.setY(20.5);
        el.setText("测试");

        String json = mapper.writeValueAsString(el);
        RenderedElementInfo parsed = mapper.readValue(json, RenderedElementInfo.class);

        assertEquals("text", parsed.getType());
        assertEquals(10.5, parsed.getX());
        assertEquals("测试", parsed.getText());
    }
}
