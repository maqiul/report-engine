package com.reportengine.lib;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.reportengine.lib.model.RenderRequest;
import com.reportengine.lib.model.RenderResponse;
import com.reportengine.lib.model.RenderResponse.PageInfo;
import com.reportengine.lib.model.RenderResponse.RenderedElementInfo;
import com.reportengine.lib.renderer.ReportRenderer;
import org.junit.jupiter.api.Test;

import java.io.File;
import java.util.ArrayList;
import java.util.Comparator;
import java.util.HashMap;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.stream.Collectors;

import static org.junit.jupiter.api.Assertions.*;

/**
 * 跨端一致性测试 — Java 侧 dump。
 *
 * 用与 .NET tests/ReportEngine.Core.Tests/CrossConsistencyDumpTest.cs 完全相同的
 * 模板 JSON + 字符串数据，渲染后导出规范化摘要 JSON，供
 * tests/cross-consistency/check.js 比对。
 *
 * 输出路径由环境变量 CC_DUMP 指定；未设置时只做结构断言（不写文件）。
 */
class CrossConsistencyDumpTest {

    // ⚠️ 这份 JSON 必须与 .NET CrossConsistencyDumpTest.TemplateJson 逐字一致。
    //    表达式用两端通用的 {{currentRow.xxx}}；字段值全用字符串，规避数字格式化差异。
    static final String TEMPLATE = """
    {
      "version": "1.0",
      "page": { "width": 210, "height": 297, "margin": { "top": 15, "bottom": 15, "left": 15, "right": 15 } },
      "dataSources": [ { "name": "orders", "type": "json" } ],
      "bands": [
        { "type": "reportHeader", "height": 12, "elements": [
          { "type": "text", "text": "ORDER SUMMARY", "x": 0, "y": 0, "width": 180, "height": 10, "font": { "size": 18, "bold": true }, "alignment": "center" }
        ] },
        { "type": "detail", "height": 8, "dataSource": "orders", "elements": [
          { "type": "text", "text": "{{currentRow.id}}",       "x": 0,   "y": 0, "width": 30, "height": 6, "font": { "size": 12 }, "alignment": "left" },
          { "type": "text", "text": "{{currentRow.customer}}", "x": 35,  "y": 0, "width": 70, "height": 6, "font": { "size": 12 }, "alignment": "left" },
          { "type": "text", "text": "{{currentRow.status}}",   "x": 110, "y": 0, "width": 70, "height": 6, "font": { "size": 12 }, "alignment": "left" }
        ] }
      ]
    }
    """;

    static List<Map<String, Object>> orders() {
        List<Map<String, Object>> l = new ArrayList<>();
        l.add(row("SO-001", "Acme Corp", "PAID"));
        l.add(row("SO-002", "Globex", "OPEN"));
        l.add(row("SO-003", "Initech", "PAID"));
        return l;
    }

    static Map<String, Object> row(String id, String customer, String status) {
        Map<String, Object> m = new LinkedHashMap<>();
        m.put("id", id);
        m.put("customer", customer);
        m.put("status", status);
        return m;
    }

    @Test
    void Dump_Java_Summary() throws Exception {
        Map<String, List<Map<String, Object>>> data = new HashMap<>();
        data.put("orders", orders());

        RenderResponse resp = new ReportRenderer().render(new RenderRequest(TEMPLATE, data));
        assertTrue(resp.isSuccess(), () -> "render failed: " + resp.getError());
        assertFalse(resp.getPages().isEmpty());

        List<RenderedElementInfo> els = resp.getPages().get(0).getElements();
        List<String> texts = els.stream().map(RenderedElementInfo::getText).collect(Collectors.toList());

        // ---- 结构断言：确保 Java 渲染本身正确 ----
        assertTrue(texts.contains("ORDER SUMMARY"));
        assertTrue(texts.contains("SO-001"));
        assertTrue(texts.contains("SO-002"));
        assertTrue(texts.contains("SO-003"));
        assertTrue(texts.contains("Acme Corp"));
        assertTrue(texts.contains("Initech"));
        assertTrue(texts.contains("PAID"));
        assertTrue(texts.contains("OPEN"));
        assertTrue(texts.stream().noneMatch(t -> t != null && t.contains("currentRow.")),
                "currentRow 占位符必须被替换");
        assertEquals(10, els.size());

        // ---- 规范化摘要（坐标不含 y：.NET 分页 vs Java 流式纵向不可比）----
        List<Map<String, Object>> elements = new ArrayList<>();
        for (RenderedElementInfo e : els) {
            Map<String, Object> m = new LinkedHashMap<>();
            m.put("type", e.getType());
            m.put("text", e.getText() == null ? "" : e.getText());
            m.put("x", round(e.getX()));
            m.put("w", round(e.getWidth()));
            m.put("size", e.getFont() != null ? round(e.getFont().getSize()) : 0.0);
            m.put("align", e.getAlignment() == null ? "" : e.getAlignment());
            elements.add(m);
        }
        elements.sort(Comparator
                .comparing((Map<String, Object> m) -> (String) m.get("text"))
                .thenComparing(m -> (Double) m.get("x")));

        Map<String, Object> root = new LinkedHashMap<>();
        root.put("engine", "java");
        root.put("pageCount", resp.getPages().size());
        root.put("elements", elements);

        String outPath = System.getenv("CC_DUMP");
        if (outPath != null && !outPath.isBlank()) {
            File f = new File(outPath);
            f.getParentFile().mkdirs();
            new ObjectMapper().writerWithDefaultPrettyPrinter().writeValue(f, root);
        }
    }

    static double round(double v) {
        return Math.round(v * 100) / 100.0;
    }
}
