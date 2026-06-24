package com.example.demo;

import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.boot.test.web.server.LocalServerPort;
import org.springframework.http.MediaType;
import org.springframework.web.client.RestClient;
import org.springframework.web.client.RestClient.ResponseSpec;

import java.util.List;
import java.util.Map;

import static org.junit.jupiter.api.Assertions.*;

/**
 * 端到端集成测试 - 启动完整 Spring Boot，HTTP 实际请求
 *
 * 用 Spring 6 RestClient 替代 RestTemplate（Boot 4.x 标配）
 */
@SpringBootTest(webEnvironment = SpringBootTest.WebEnvironment.RANDOM_PORT)
@DisplayName("ReportEngine Demo E2E")
class DemoE2ETest {

    @LocalServerPort
    int port;

    @Autowired
    OrderRepository orderRepository;

    private RestClient client() {
        return RestClient.builder().baseUrl("http://localhost:" + port).build();
    }

    private String url(String path) { return "http://localhost:" + port + path; }

    @Test
    @DisplayName("1. 列表 + 数据已 seed")
    void list_orders_seeded() {
        assertEquals(3, orderRepository.count());

        @SuppressWarnings("unchecked")
        List<Map<String, Object>> body = client().get().uri("/api/orders")
            .retrieve()
            .body(List.class);
        assertNotNull(body);
        assertEquals(3, body.size());
        assertEquals("Acme Corp", body.get(0).get("customer"));
    }

    @Test
    @DisplayName("2. 单订单渲染预览")
    void preview_single_order() {
        Map body = client().get().uri("/api/orders/SO-001/preview")
            .retrieve().body(Map.class);
        assertNotNull(body);
        assertNotNull(body.get("pages"));
    }

    @Test
    @DisplayName("3. 单订单 PDF 导出")
    void export_pdf() {
        byte[] body = client().get().uri("/api/orders/SO-001/export/pdf")
            .retrieve().body(byte[].class);
        assertNotNull(body);
        assertTrue(body.length > 100, "PDF 应该非空，实际: " + body.length);
        assertEquals('%', (char) body[0]);
        assertEquals('P', (char) body[1]);
        assertEquals('D', (char) body[2]);
        assertEquals('F', (char) body[3]);
    }

    @Test
    @DisplayName("4. 单订单 Excel 导出")
    void export_excel() {
        byte[] body = client().get().uri("/api/orders/SO-001/export/excel")
            .retrieve().body(byte[].class);
        assertNotNull(body);
        assertTrue(body.length > 100, "Excel 应该非空，实际: " + body.length);
        assertEquals('P', (char) body[0]);
        assertEquals('K', (char) body[1]);
    }

    @Test
    @DisplayName("5. 销售汇总预览")
    void summary_preview() {
        Map body = client().get().uri("/api/orders/report/summary/preview")
            .retrieve().body(Map.class);
        assertNotNull(body);
        assertNotNull(body.get("pages"));
    }

    @Test
    @DisplayName("6. 销售汇总 PDF 导出")
    void summary_pdf() {
        byte[] body = client().get().uri("/api/orders/report/summary/export/pdf")
            .retrieve().body(byte[].class);
        assertNotNull(body);
        assertTrue(body.length > 100);
        assertEquals('%', (char) body[0]);
    }

    @Test
    @DisplayName("7. 不存在的订单 → 404")
    void not_found() {
        int status = client().get().uri("/api/orders/SO-999")
            .retrieve()
            .onStatus(s -> true, (req, res) -> {}) // 阻止抛异常
            .toBodilessEntity()
            .getStatusCode().value();
        assertEquals(404, status);
    }

    @Test
    @DisplayName("8. starter 自动暴露 /api/reports/health")
    void starter_health() {
        Map body = client().get().uri("/api/reports/health")
            .retrieve().body(Map.class);
        assertNotNull(body);
        assertEquals("UP", body.get("status"));
    }

    @Test
    @DisplayName("9. starter 自动暴露 /api/reports/render/preview")
    void starter_render() {
        Map<String, Object> req = Map.of(
            "templateJson", "{\"bands\":[]}",
            "data", Map.of()
        );
        Map body = client().post().uri("/api/reports/render/preview")
            .contentType(MediaType.APPLICATION_JSON)
            .body(req)
            .retrieve().body(Map.class);
        assertNotNull(body);
        assertNotNull(body.get("pages"));
    }
}
