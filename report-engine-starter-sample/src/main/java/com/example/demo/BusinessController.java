package com.example.demo;

import com.reportengine.lib.ReportEngine;
import com.reportengine.lib.model.RenderRequest;
import com.reportengine.lib.model.RenderResponse;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.util.List;
import java.util.Map;

/**
 * 第三方业务代码：只 @Autowired ReportEngine Bean，
 * 没有任何 ReportEngine 实现细节。
 */
@RestController
@RequestMapping("/business")
public class BusinessController {

    @Autowired
    private ReportEngine engine;

    @PostMapping("/hello-report")
    public RenderResponse helloReport(@RequestBody Map<String, Object> body) {
        RenderRequest req = new RenderRequest();
        req.setTemplateJson((String) body.get("templateJson"));
        if (body.get("data") instanceof Map<?, ?> data) {
            // 简易转换：实际项目里写个 util
            @SuppressWarnings("unchecked")
            Map<String, List<Map<String, Object>>> dataMap = (Map<String, List<Map<String, Object>>>) data;
            req.setData(dataMap);
        }
        return engine.render(req);
    }

    @GetMapping("/ping")
    public ResponseEntity<String> ping() {
        return ResponseEntity.ok("pong from " + engine.getClass().getName());
    }
}