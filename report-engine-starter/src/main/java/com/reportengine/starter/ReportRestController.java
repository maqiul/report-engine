package com.reportengine.starter;

import com.reportengine.lib.ReportEngine;
import com.reportengine.lib.model.RenderRequest;
import com.reportengine.lib.model.RenderResponse;
import com.reportengine.lib.store.TemplateStore;
import org.springframework.http.HttpHeaders;
import org.springframework.http.MediaType;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.Map;
import java.util.List;
import java.util.Optional;

/**
 * Starter 自动暴露的 REST 端点
 *
 * <ul>
 *   <li>POST {prefix}/render/preview       - 渲染为 JSON（前端预览）</li>
 *   <li>POST {prefix}/export/pdf            - 导出 PDF</li>
 *   <li>POST {prefix}/export/excel          - 导出 Excel</li>
 *   <li>POST {prefix}/render-by-id/{id}     - 用 TemplateStore 加载模板 + 渲染</li>
 * </ul>
 *
 * @since 0.3.0
 */
@RestController
@RequestMapping("${report.engine.rest-prefix:/api/reports}")
public class ReportRestController {

    private final ReportEngine engine;
    private final ReportEngineProperties props;
    private final TemplateStore store;

    public ReportRestController(ReportEngine engine,
                                ReportEngineProperties props,
                                TemplateStore store) {
        this.engine = engine;
        this.props = props;
        this.store = store;
    }

    @PostMapping("/render/preview")
    public RenderResponse renderPreview(@RequestBody RenderRequest request) {
        return engine.render(request);
    }

    @PostMapping(value = "/export/pdf", produces = MediaType.APPLICATION_PDF_VALUE)
    public ResponseEntity<byte[]> exportPdf(@RequestBody RenderRequest request) throws Exception {
        byte[] pdf = engine.exportPdf(request);
        return ResponseEntity.ok()
            .header(HttpHeaders.CONTENT_DISPOSITION, "inline; filename=report.pdf")
            .contentType(MediaType.APPLICATION_PDF)
            .body(pdf);
    }

    @PostMapping(value = "/export/excel",
        produces = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
    public ResponseEntity<byte[]> exportExcel(@RequestBody RenderRequest request) throws Exception {
        byte[] xlsx = engine.exportExcel(request);
        return ResponseEntity.ok()
            .header(HttpHeaders.CONTENT_DISPOSITION, "inline; filename=report.xlsx")
            .contentType(MediaType.parseMediaType(
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"))
            .body(xlsx);
    }

    @PostMapping("/render-by-id/{templateId}")
    public ResponseEntity<?> renderById(@PathVariable String templateId,
                                        @RequestBody(required = false) Map<String, Object> data) {
        Optional<String> templateJson = store.loadById(templateId);
        if (templateJson.isEmpty()) {
            return ResponseEntity.notFound().build();
        }
        RenderRequest req = new RenderRequest();
        req.setTemplateJson(templateJson.get());
        if (data != null) {
            req.setData(toDataMap(data));
        }
        return ResponseEntity.ok(engine.render(req));
    }

    /** 把外部 Map<String,Object> 转成 RenderRequest 需要的 Map<String,List<Map<String,Object>>> */
    @SuppressWarnings("unchecked")
    private static Map<String, List<Map<String, Object>>> toDataMap(Map<String, Object> raw) {
        Map<String, List<Map<String, Object>>> result = new java.util.HashMap<>();
        for (Map.Entry<String, Object> e : raw.entrySet()) {
            Object v = e.getValue();
            if (v instanceof List<?> list) {
                List<Map<String, Object>> rows = new java.util.ArrayList<>();
                for (Object item : list) {
                    if (item instanceof Map<?, ?> m) {
                        rows.add((Map<String, Object>) m);
                    } else {
                        rows.add(Map.of("value", item));
                    }
                }
                result.put(e.getKey(), rows);
            } else {
                result.put(e.getKey(), List.of(Map.of("value", v)));
            }
        }
        return result;
    }

    /** 健康检查端点（方便 Docker k8s 探针） */
    @GetMapping("/health")
    public Map<String, Object> health() {
        return Map.of(
            "status", "UP",
            "autoRestApi", props.isAutoRestApi(),
            "prefix", props.getRestPrefix()
        );
    }
}