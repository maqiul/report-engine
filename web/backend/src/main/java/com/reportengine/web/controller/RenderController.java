package com.reportengine.web.controller;

import com.reportengine.lib.model.RenderRequest;
import com.reportengine.lib.model.RenderResponse;
import com.reportengine.lib.renderer.ReportRenderer;
import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.tags.Tag;
import org.springframework.web.bind.annotation.*;

/**
 * 渲染控制器 - 提供报表预览 API
 */
@RestController
@RequestMapping("/api/render")
@CrossOrigin(origins = {"http://localhost:5173", "http://localhost:3000", "http://127.0.0.1:3000"})
@Tag(name = "报表渲染", description = "解析模板并返回渲染结果（用于前端预览）")
public class RenderController {

    private final ReportRenderer renderer = new ReportRenderer();

    /**
     * 预览报表 - 解析模板并返回渲染结果
     */
    @PostMapping("/preview")
    @Operation(
        summary = "预览报表",
        description = "解析报表模板，按 detail band 展开数据，返回分页后的元素列表（用于前端预览）"
    )
    public RenderResponse preview(@RequestBody RenderRequest request) {
        return renderer.render(request);
    }
}
