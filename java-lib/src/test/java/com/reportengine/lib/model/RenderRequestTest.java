package com.reportengine.lib.model;

import com.fasterxml.jackson.databind.ObjectMapper;
import org.junit.jupiter.api.Test;

import java.util.*;

import static org.junit.jupiter.api.Assertions.*;
import static org.assertj.core.api.Assertions.assertThat;

/**
 * RenderRequest 模型测试
 */
class RenderRequestTest {

    @Test
    void shouldCreateEmptyRequest() {
        RenderRequest req = new RenderRequest();
        assertNull(req.getTemplateJson());
        assertNull(req.getData());
    }

    @Test
    void shouldCreateRequestWithAllFields() {
        Map<String, List<Map<String, Object>>> data = new HashMap<>();
        List<Map<String, Object>> rows = new ArrayList<>();
        rows.add(Map.of("name", "张三", "amount", 1000));
        data.put("orders", rows);

        RenderRequest req = new RenderRequest("{\"title\":\"销售报表\"}", data);
        assertEquals("{\"title\":\"销售报表\"}", req.getTemplateJson());
        assertThat(req.getData()).containsKey("orders");
        assertThat(req.getData().get("orders")).hasSize(1);
    }

    @Test
    void shouldSetAndGetTemplateJson() {
        RenderRequest req = new RenderRequest();
        req.setTemplateJson("{\"a\":1}");
        assertEquals("{\"a\":1}", req.getTemplateJson());
    }

    @Test
    void shouldSetAndGetData() {
        RenderRequest req = new RenderRequest();
        Map<String, List<Map<String, Object>>> data = Map.of(
            "users", List.of(Map.of("id", 1))
        );
        req.setData(data);
        assertSame(data, req.getData());
    }

    @Test
    void shouldHandleNullData() {
        RenderRequest req = new RenderRequest("{}", null);
        assertNull(req.getData());
    }

    @Test
    void shouldSerializeToJson() throws Exception {
        ObjectMapper mapper = new ObjectMapper();
        RenderRequest req = new RenderRequest("{\"x\":1}", Map.of("k", List.of()));

        String json = mapper.writeValueAsString(req);
        assertThat(json).contains("templateJson");
        assertThat(json).contains("data");
    }

    @Test
    void shouldDeserializeFromJson() throws Exception {
        ObjectMapper mapper = new ObjectMapper();
        String json = "{\"templateJson\":\"{}\",\"data\":{\"a\":[{\"x\":1}]}}";

        RenderRequest req = mapper.readValue(json, RenderRequest.class);
        assertEquals("{}", req.getTemplateJson());
        assertThat(req.getData()).containsKey("a");
    }

    @Test
    void shouldHandleEmptyDataMap() {
        RenderRequest req = new RenderRequest("{}", new HashMap<>());
        assertNotNull(req.getData());
        assertTrue(req.getData().isEmpty());
    }

    @Test
    void shouldHandleMultipleDataSources() {
        Map<String, List<Map<String, Object>>> data = new HashMap<>();
        data.put("orders", List.of(Map.of("id", 1), Map.of("id", 2)));
        data.put("users", List.of(Map.of("name", "alice")));

        RenderRequest req = new RenderRequest("{}", data);
        assertThat(req.getData().keySet()).containsExactlyInAnyOrder("orders", "users");
        assertThat(req.getData().get("orders")).hasSize(2);
        assertThat(req.getData().get("users")).hasSize(1);
    }

    @Test
    void shouldHandleEmptyTemplateJson() {
        RenderRequest req = new RenderRequest("", null);
        assertEquals("", req.getTemplateJson());
    }
}
