package com.reportengine.template;

import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.tags.Tag;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;

/**
 * 模板 CRUD API
 */
@RestController
@RequestMapping("/api/templates")
@Tag(name = "模板管理", description = "报表模板的增删改查接口")
public class TemplateController {

    private final TemplateService service;

    public TemplateController(TemplateService service) {
        this.service = service;
    }

    @GetMapping
    @Operation(summary = "列出所有模板")
    public List<Template> list(@RequestParam(required = false) String category) {
        if (category != null && !category.isBlank()) {
            return service.listByCategory(category);
        }
        return service.listAll();
    }

    @GetMapping("/{id}")
    @Operation(summary = "按 ID 获取模板")
    public ResponseEntity<Template> getById(@PathVariable Long id) {
        return service.getById(id)
            .map(ResponseEntity::ok)
            .orElse(ResponseEntity.notFound().build());
    }

    @GetMapping("/by-name/{name}")
    @Operation(summary = "按名称获取模板")
    public ResponseEntity<Template> getByName(@PathVariable String name) {
        return service.getByName(name)
            .map(ResponseEntity::ok)
            .orElse(ResponseEntity.notFound().build());
    }

    @PostMapping
    @Operation(summary = "新建模板")
    public ResponseEntity<?> create(@RequestBody Template template) {
        try {
            Template saved = service.create(template);
            return ResponseEntity.status(201).body(saved);
        } catch (IllegalArgumentException e) {
            return ResponseEntity.badRequest().body(error(e.getMessage()));
        }
    }

    @PutMapping("/{id}")
    @Operation(summary = "更新模板（自动 +1 版本号）")
    public ResponseEntity<?> update(@PathVariable Long id, @RequestBody Template template) {
        try {
            Template saved = service.update(id, template);
            return ResponseEntity.ok(saved);
        } catch (IllegalArgumentException e) {
            if (e.getMessage().contains("不存在")) {
                return ResponseEntity.notFound().build();
            }
            return ResponseEntity.badRequest().body(error(e.getMessage()));
        }
    }

    @DeleteMapping("/{id}")
    @Operation(summary = "删除模板")
    public ResponseEntity<?> delete(@PathVariable Long id) {
        try {
            service.delete(id);
            return ResponseEntity.noContent().build();
        } catch (IllegalArgumentException e) {
            return ResponseEntity.notFound().build();
        }
    }

    private static java.util.Map<String, String> error(String msg) {
        return java.util.Map.of("error", msg);
    }
}