package com.example.demo;

import com.reportengine.lib.ReportEngine;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.boot.test.web.client.TestRestTemplate;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;

import java.util.Map;

import static org.assertj.core.api.Assertions.assertThat;

/**
 * 模拟"别人的项目"使用 starter 的集成测试
 *
 * 关键证明：第三方项目只要加 starter 依赖，
 * 就能 @Autowired ReportEngine + 自动开 REST 端点。
 */
@SpringBootTest(webEnvironment = SpringBootTest.WebEnvironment.RANDOM_PORT)
@DisplayName("第三方项目集成测试")
class StarterIntegrationFromExternalProjectTest {

    @Autowired
    ReportEngine engine;

    @Autowired
    TestRestTemplate rest;

    @Test
    @DisplayName("1. ReportEngine Bean 由 starter 提供（不是我们写的）")
    void engineBeanProvidedByStarter() {
        assertThat(engine).isNotNull();
        // 来自 com.reportengine.lib.ReportEngine（starter 提供）
        assertThat(engine.getClass().getName()).isEqualTo("com.reportengine.lib.ReportEngine");
    }

    @Test
    @DisplayName("2. starter 自动开 /api/reports/health")
    void starterAutoRestEndpoints() {
        ResponseEntity<Map> resp = rest.getForEntity("/api/reports/health", Map.class);
        assertThat(resp.getStatusCode()).isEqualTo(HttpStatus.OK);
        assertThat(resp.getBody()).containsEntry("status", "UP");
    }

    @Test
    @DisplayName("3. starter 自动开 /api/reports/render/preview")
    void starterAutoRenderPreview() {
        String templateJson = "{\"version\":\"1.0\",\"bands\":[]}";
        Map<String, Object> body = Map.of("templateJson", templateJson);

        ResponseEntity<Map> resp = rest.postForEntity("/api/reports/render/preview", body, Map.class);
        assertThat(resp.getStatusCode()).isEqualTo(HttpStatus.OK);
    }

    @Test
    @DisplayName("4. starter 自动开 /api/reports/export/pdf")
    void starterAutoExportPdf() {
        String templateJson = "{\"version\":\"1.0\",\"bands\":[]}";
        Map<String, Object> body = Map.of("templateJson", templateJson);

        ResponseEntity<byte[]> resp = rest.postForEntity("/api/reports/export/pdf", body, byte[].class);
        assertThat(resp.getStatusCode()).isEqualTo(HttpStatus.OK);
        assertThat(resp.getBody()).isNotEmpty();
        assertThat(new String(resp.getBody(), 0, 4)).isEqualTo("%PDF");
    }

    @Test
    @DisplayName("5. 业务 Controller @Autowired ReportEngine 直接调用")
    void businessControllerUsesEngine() {
        ResponseEntity<String> resp = rest.getForEntity("/business/ping", String.class);
        assertThat(resp.getStatusCode()).isEqualTo(HttpStatus.OK);
        // 证明 engine Bean 真被业务代码用上
        assertThat(resp.getBody()).contains("com.reportengine.lib.ReportEngine");
    }
}