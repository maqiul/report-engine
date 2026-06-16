package com.reportengine.web.controller;

import com.reportengine.lib.model.RenderRequest;
import com.reportengine.lib.model.RenderResponse;
import com.reportengine.lib.renderer.ReportRenderer;
import org.springframework.web.bind.annotation.*;

/**
 * 渲染控制器 - 提供报表预览 API
 */
@RestController
@RequestMapping("/api/render")
@CrossOrigin(origins = {"http://localhost:5173", "http://localhost:3000", "http://127.0.0.1:3000"})
public class RenderController {

    private final ReportRenderer renderer = new ReportRenderer();

    /**
     * 预览报表 - 解析模板并返回渲染结果
     */
    @PostMapping("/preview")
    public RenderResponse preview(@RequestBody RenderRequest request) {
        return renderer.render(request);
    }
}
