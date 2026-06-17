package com.reportengine.web;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.reportengine.lib.model.RenderResponse;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.http.MediaType;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.MvcResult;
import org.springframework.test.web.servlet.setup.MockMvcBuilders;
import org.springframework.web.context.WebApplicationContext;

import java.util.HashMap;
import java.util.List;
import java.util.Map;

import static org.junit.jupiter.api.Assertions.*;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.post;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.*;

/**
 * 后端 E2E 集成测试 - Spring Boot Test + MockMvc
 */
@SpringBootTest
class BackendE2ETest {

    @Autowired
    private WebApplicationContext webApplicationContext;

    private MockMvc mockMvc;
    private final ObjectMapper objectMapper = new ObjectMapper();

    private MockMvc mockMvc() {
        if (mockMvc == null) {
            mockMvc = MockMvcBuilders.webAppContextSetup(webApplicationContext).build();
        }
        return mockMvc;
    }

    // ==================== /api/render/preview ====================

    @Test
    @DisplayName("POST /api/render/preview 空模板应返回成功")
    void shouldPreviewEmptyTemplate() throws Exception {
        Map<String, Object> req = new HashMap<>();
        req.put("templateJson", "{}");
        req.put("data", new HashMap<>());

        mockMvc().perform(post("/api/render/preview")
                .contentType(MediaType.APPLICATION_JSON)
                .content(objectMapper.writeValueAsString(req)))
            .andExpect(status().isOk())
            .andExpect(jsonPath("$.success").value(true));
    }

    @Test
    @DisplayName("POST /api/render/preview 应返回 elements 列表")
    void shouldReturnElementsList() throws Exception {
        Map<String, Object> req = new HashMap<>();
        req.put("templateJson", "{\"bands\":[{\"type\":\"title\",\"height\":20,\"elements\":[" +
            "{\"type\":\"text\",\"x\":10,\"y\":0,\"width\":100,\"height\":10,\"text\":\"标题\"}]}]}");
        req.put("data", new HashMap<>());

        MvcResult result = mockMvc().perform(post("/api/render/preview")
                .contentType(MediaType.APPLICATION_JSON)
                .content(objectMapper.writeValueAsString(req)))
            .andExpect(status().isOk())
            .andExpect(jsonPath("$.success").value(true))
            .andExpect(jsonPath("$.pages[0].elements[0].text").value("标题"))
            .andReturn();

        assertNotNull(result);
    }

    @Test
    @DisplayName("POST /api/render/preview 无效 JSON 应返回失败")
    void shouldReturnErrorOnInvalidJson() throws Exception {
        Map<String, Object> req = new HashMap<>();
        req.put("templateJson", "{invalid");
        req.put("data", new HashMap<>());

        mockMvc().perform(post("/api/render/preview")
                .contentType(MediaType.APPLICATION_JSON)
                .content(objectMapper.writeValueAsString(req)))
            .andExpect(status().isOk())
            .andExpect(jsonPath("$.success").value(false));
    }

    @Test
    @DisplayName("POST /api/render/preview detail 数据应展开")
    void shouldExpandDetailData() throws Exception {
        Map<String, Object> req = new HashMap<>();
        req.put("templateJson", "{\"bands\":[{" +
            "\"type\":\"detail\",\"height\":10,\"dataSource\":\"orders\"," +
            "\"elements\":[{\"type\":\"text\",\"x\":0,\"y\":0,\"width\":50,\"height\":10,\"text\":\"{{currentRow.id}}\"}]}]}");
        Map<String, List<Map<String, Object>>> data = new HashMap<>();
        data.put("orders", List.of(Map.of("id", "1"), Map.of("id", "2")));
        req.put("data", data);

        mockMvc().perform(post("/api/render/preview")
                .contentType(MediaType.APPLICATION_JSON)
                .content(objectMapper.writeValueAsString(req)))
            .andExpect(status().isOk())
            .andExpect(jsonPath("$.success").value(true))
            .andExpect(jsonPath("$.pages[0].elements.length()").value(2));
    }

    // ==================== /api/export/pdf ====================

    @Test
    @DisplayName("POST /api/export/pdf 应返回 application/pdf")
    void shouldExportPdf() throws Exception {
        Map<String, Object> req = new HashMap<>();
        req.put("templateJson", "{\"bands\":[{\"type\":\"title\",\"height\":20,\"elements\":[" +
            "{\"type\":\"text\",\"x\":10,\"y\":0,\"width\":100,\"height\":10,\"text\":\"标题\"}]}]}");
        req.put("data", new HashMap<>());

        MvcResult result = mockMvc().perform(post("/api/export/pdf")
                .contentType(MediaType.APPLICATION_JSON)
                .content(objectMapper.writeValueAsString(req)))
            .andExpect(status().isOk())
            .andExpect(content().contentType(MediaType.APPLICATION_PDF))
            .andReturn();

        byte[] body = result.getResponse().getContentAsByteArray();
        assertTrue(body.length > 0);
        String header = new String(new byte[]{body[0], body[1], body[2], body[3], body[4]});
        assertEquals("%PDF-", header);
    }

    @Test
    @DisplayName("POST /api/export/pdf 中文应能导出")
    void shouldExportPdfWithChinese() throws Exception {
        Map<String, Object> req = new HashMap<>();
        req.put("templateJson", "{\"bands\":[{\"type\":\"title\",\"height\":20,\"elements\":[" +
            "{\"type\":\"text\",\"x\":10,\"y\":0,\"width\":100,\"height\":10,\"text\":\"张三李四王五\"}]}]}");
        req.put("data", new HashMap<>());

        mockMvc().perform(post("/api/export/pdf")
                .contentType(MediaType.APPLICATION_JSON)
                .content(objectMapper.writeValueAsString(req)))
            .andExpect(status().isOk())
            .andExpect(content().contentType(MediaType.APPLICATION_PDF));
    }

    @Test
    @DisplayName("POST /api/export/pdf 完整报表应能导出")
    void shouldExportCompletePdf() throws Exception {
        Map<String, Object> req = new HashMap<>();
        req.put("templateJson", "{\"page\":{\"width\":210,\"height\":297},\"bands\":[" +
            "{\"type\":\"title\",\"height\":20,\"elements\":[{\"type\":\"text\",\"x\":0,\"y\":0,\"width\":210,\"height\":10,\"text\":\"销售报表\"}]}," +
            "{\"type\":\"detail\",\"height\":10,\"dataSource\":\"orders\",\"elements\":[" +
            "{\"type\":\"text\",\"x\":10,\"y\":0,\"width\":50,\"height\":10,\"text\":\"{{currentRow.id}}\"}," +
            "{\"type\":\"text\",\"x\":70,\"y\":0,\"width\":70,\"height\":10,\"text\":\"{{currentRow.cust}}\"}," +
            "{\"type\":\"text\",\"x\":150,\"y\":0,\"width\":50,\"height\":10,\"text\":\"{{currentRow.amt}}\"}]}," +
            "{\"type\":\"pageFooter\",\"height\":15,\"elements\":[" +
            "{\"type\":\"text\",\"x\":0,\"y\":0,\"width\":210,\"height\":10,\"text\":\"第 {{page}} / {{totalPages}} 页\"}]}]}");
        Map<String, List<Map<String, Object>>> data = new HashMap<>();
        data.put("orders", List.of(
            Map.of("id", "1", "cust", "张三", "amt", "100"),
            Map.of("id", "2", "cust", "李四", "amt", "200")
        ));
        req.put("data", data);

        mockMvc().perform(post("/api/export/pdf")
                .contentType(MediaType.APPLICATION_JSON)
                .content(objectMapper.writeValueAsString(req)))
            .andExpect(status().isOk())
            .andExpect(content().contentType(MediaType.APPLICATION_PDF));
    }

    // ==================== /api/export/excel ====================

    @Test
    @DisplayName("POST /api/export/excel 应返回 application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
    void shouldExportExcel() throws Exception {
        Map<String, Object> req = new HashMap<>();
        req.put("templateJson", "{\"bands\":[{\"type\":\"title\",\"height\":20,\"elements\":[" +
            "{\"type\":\"text\",\"x\":10,\"y\":0,\"width\":100,\"height\":10,\"text\":\"标题\"}]}]}");
        req.put("data", new HashMap<>());

        MvcResult result = mockMvc().perform(post("/api/export/excel")
                .contentType(MediaType.APPLICATION_JSON)
                .content(objectMapper.writeValueAsString(req)))
            .andExpect(status().isOk())
            .andReturn();

        byte[] body = result.getResponse().getContentAsByteArray();
        assertTrue(body.length > 0);
        // xlsx 是 zip，PK 开头
        assertEquals('P', (char) body[0]);
        assertEquals('K', (char) body[1]);
    }

    @Test
    @DisplayName("POST /api/export/excel 完整报表应能导出")
    void shouldExportCompleteExcel() throws Exception {
        Map<String, Object> req = new HashMap<>();
        req.put("templateJson", "{\"page\":{\"width\":210,\"height\":297},\"bands\":[" +
            "{\"type\":\"title\",\"height\":20,\"elements\":[{\"type\":\"text\",\"x\":0,\"y\":0,\"width\":210,\"height\":10,\"text\":\"销售报表\"}]}," +
            "{\"type\":\"pageHeader\",\"height\":15,\"elements\":[{\"type\":\"text\",\"x\":10,\"y\":0,\"width\":50,\"height\":10,\"text\":\"订单号\"}]}," +
            "{\"type\":\"detail\",\"height\":10,\"dataSource\":\"orders\",\"elements\":[" +
            "{\"type\":\"text\",\"x\":10,\"y\":0,\"width\":50,\"height\":10,\"text\":\"{{currentRow.id}}\"}]}," +
            "{\"type\":\"pageFooter\",\"height\":15,\"elements\":[" +
            "{\"type\":\"text\",\"x\":0,\"y\":0,\"width\":210,\"height\":10,\"text\":\"第 {{page}} 页\"}]}]}");
        Map<String, List<Map<String, Object>>> data = new HashMap<>();
        data.put("orders", List.of(Map.of("id", "1"), Map.of("id", "2"), Map.of("id", "3")));
        req.put("data", data);

        mockMvc().perform(post("/api/export/excel")
                .contentType(MediaType.APPLICATION_JSON)
                .content(objectMapper.writeValueAsString(req)))
            .andExpect(status().isOk());
    }

    // ==================== 边界测试 ====================

    @Test
    @DisplayName("POST /api/render/preview 缺省 data 字段应不报错")
    void shouldHandleMissingDataField() throws Exception {
        String body = "{\"templateJson\":\"{}\"}";

        mockMvc().perform(post("/api/render/preview")
                .contentType(MediaType.APPLICATION_JSON)
                .content(body))
            .andExpect(status().isOk());
    }

    @Test
    @DisplayName("POST /api/export/pdf 无效模板应返回 400")
    void shouldReturn400OnInvalidPdfTemplate() throws Exception {
        Map<String, Object> req = new HashMap<>();
        req.put("templateJson", "{not valid");
        req.put("data", new HashMap<>());

        mockMvc().perform(post("/api/export/pdf")
                .contentType(MediaType.APPLICATION_JSON)
                .content(objectMapper.writeValueAsString(req)))
            .andExpect(status().isBadRequest());
    }
}
