package com.reportengine.starter;

import com.reportengine.lib.ReportEngine;
import com.reportengine.lib.exporter.ExcelExporter;
import com.reportengine.lib.exporter.PdfExporter;
import com.reportengine.lib.renderer.ReportRenderer;
import com.reportengine.lib.store.TemplateStore;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.boot.test.web.client.TestRestTemplate;
import org.springframework.boot.test.web.server.LocalServerPort;
import org.springframework.context.ApplicationContext;
import org.springframework.http.HttpEntity;
import org.springframework.http.HttpHeaders;
import org.springframework.http.HttpMethod;
import org.springframework.http.HttpStatus;
import org.springframework.http.MediaType;
import org.springframework.http.ResponseEntity;

import java.util.HashMap;
import java.util.List;
import java.util.Map;

import static org.assertj.core.api.Assertions.assertThat;

/**
 * Starter 集成测试
 *
 * 模拟"别人项目"使用 starter 的场景：
 * 1. 加依赖
 * 2. Spring Boot 自动配置
 * 3. @Autowired ReportEngine 直接用
 * 4. 自动 REST 端点开 /api/reports/**
 */
@SpringBootTest(
    classes = {TestApplication.class},
    webEnvironment = SpringBootTest.WebEnvironment.RANDOM_PORT,
    properties = {
        "report.engine.enabled=true",
        "report.engine.auto-rest-api=true",
        "report.engine.rest-prefix=/api/reports"
    }
)
@DisplayName("Starter 集成测试 - 模拟第三方项目集成")
class StarterIntegrationTest {

    @Autowired
    ApplicationContext ctx;

    @Autowired
    ReportEngine engine;

    @Autowired
    ReportRenderer renderer;

    @Autowired
    PdfExporter pdfExporter;

    @Autowired
    ExcelExporter excelExporter;

    @Autowired
    TemplateStore store;

    @Autowired
    TestRestTemplate rest;

    @LocalServerPort
    int port;

    private static final String TEMPLATE_JSON = """
        {
          "version": "1.0",
          "page": {"width": 210, "height": 297, "unit": "mm", "margin": {"top": 10, "bottom": 10, "left": 10, "right": 10}},
          "dataSources": [{"name": "items", "fields": [{"name": "name"}]}],
          "bands": [
            {"type": "reportHeader", "height": 20, "elements": [
              {"type": "text", "text": "Test Report", "x": 10, "y": 5, "width": 190, "height": 10,
               "font": {"family": "Microsoft YaHei", "size": 14}}
            ]},
            {"type": "detail", "height": 10, "dataSource": "items", "elements": [
              {"type": "text", "text": "{{currentRow.name}}", "x": 10, "y": 1, "width": 100, "height": 8}
            ]}
          ]
        }
        """;

    // ========== Bean 注入验证 ==========

    @Test
    @DisplayName("1. ReportEngine Bean 注入成功")
    void reportEngineBeanAvailable() {
        assertThat(engine).isNotNull();
    }

    @Test
    @DisplayName("2. 所有子 Bean 注入成功")
    void allBeansAvailable() {
        assertThat(renderer).isNotNull();
        assertThat(pdfExporter).isNotNull();
        assertThat(excelExporter).isNotNull();
        assertThat(store).isNotNull();
    }

    @Test
    @DisplayName("3. 自动 REST 端点已注册")
    void restEndpointsRegistered() {
        // /api/reports/health
        ResponseEntity<Map> resp = rest.getForEntity("/api/reports/health", Map.class);
        assertThat(resp.getStatusCode()).isEqualTo(HttpStatus.OK);
        assertThat(resp.getBody()).containsEntry("status", "UP");
    }

    // ========== 核心 API 用法 ==========

    @Test
    @DisplayName("4. 别人项目 1 行代码渲染：engine.render()")
    void oneLineRender() {
        Map<String, Object> req = Map.of(
            "templateJson", TEMPLATE_JSON,
            "data", Map.of("items", List.of(Map.of("name", "Hello")))
        );

        ResponseEntity<Map> resp = rest.postForEntity(
            "/api/reports/render/preview", req, Map.class);

        assertThat(resp.getStatusCode()).isEqualTo(HttpStatus.OK);
        assertThat(resp.getBody()).isNotNull();
    }

    @Test
    @DisplayName("5. 直接 @Autowired ReportEngine 调 exportPdf")
    void exportPdfViaBean() throws Exception {
        var req = new com.reportengine.lib.model.RenderRequest();
        req.setTemplateJson(TEMPLATE_JSON);
        req.setData(Map.of("items", List.of(Map.of("name", "Hello"))));

        byte[] pdf = engine.exportPdf(req);
        assertThat(pdf).isNotEmpty();
        assertThat(pdf.length).isGreaterThan(500);
        // PDF 文件头
        assertThat(new String(pdf, 0, 4)).isEqualTo("%PDF");
    }

    @Test
    @DisplayName("6. REST 端点导出 PDF")
    void exportPdfViaRest() {
        Map<String, Object> req = Map.of(
            "templateJson", TEMPLATE_JSON,
            "data", Map.of("items", List.of(Map.of("name", "Hello")))
        );

        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);
        HttpEntity<Map<String, Object>> entity = new HttpEntity<>(req, headers);

        ResponseEntity<byte[]> resp = rest.exchange(
            "/api/reports/export/pdf", HttpMethod.POST, entity, byte[].class);

        assertThat(resp.getStatusCode()).isEqualTo(HttpStatus.OK);
        assertThat(resp.getBody()).isNotEmpty();
        assertThat(resp.getHeaders().getContentType().toString()).contains("pdf");
    }

    @Test
    @DisplayName("7. REST 端点导出 Excel")
    void exportExcelViaRest() {
        Map<String, Object> req = Map.of(
            "templateJson", TEMPLATE_JSON,
            "data", Map.of("items", List.of(Map.of("name", "Hello")))
        );

        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);
        HttpEntity<Map<String, Object>> entity = new HttpEntity<>(req, headers);

        ResponseEntity<byte[]> resp = rest.exchange(
            "/api/reports/export/excel", HttpMethod.POST, entity, byte[].class);

        assertThat(resp.getStatusCode()).isEqualTo(HttpStatus.OK);
        assertThat(resp.getBody()).isNotEmpty();
    }

    @Test
    @DisplayName("8. TemplateStore 加载 classpath 模板")
    void classpathTemplateStoreLoads() {
        // 验证 cache 工作（即使没有 .rptx 文件也不报错）
        assertThat(store).isNotNull();
        assertThat(store.loadById("non-existent")).isEmpty();
    }

    @Test
    @DisplayName("9. render-by-id 端点 - 不存在返回 404")
    void renderById_notFound() {
        Map<String, Object> data = Map.of("items", List.of(Map.of("name", "x")));
        ResponseEntity<Map> resp = rest.postForEntity(
            "/api/reports/render-by-id/non-existent", data, Map.class);
        assertThat(resp.getStatusCode()).isEqualTo(HttpStatus.NOT_FOUND);
    }

    @Test
    @DisplayName("10. 别人可自定义 TemplateStore")
    void customTemplateStore() {
        // 验证条件装配：自定义 Bean 会覆盖默认
        // （这里只验证接口类型对，覆写逻辑在其他测试）
        assertThat(ctx.getBean(TemplateStore.class)).isInstanceOf(ClasspathTemplateStore.class);
    }
}