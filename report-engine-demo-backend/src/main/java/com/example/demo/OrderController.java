package com.example.demo;

import com.reportengine.lib.model.RenderRequest;
import com.reportengine.lib.model.RenderResponse;
import com.reportengine.lib.ReportEngine;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * 销售订单业务 API
 *
 * 演示 @Autowired ReportEngine：业务 Controller 直接调导出
 */
@RestController
@RequestMapping("/api/orders")
public class OrderController {

    private final OrderRepository orderRepository;
    private final ReportEngine engine;

    public OrderController(OrderRepository orderRepository, ReportEngine engine) {
        this.orderRepository = orderRepository;
        this.engine = engine;
    }

    @GetMapping
    public List<Order> list() {
        return orderRepository.findAll();
    }

    @GetMapping("/{id}")
    public ResponseEntity<Order> get(@PathVariable String id) {
        return orderRepository.findById(id)
            .map(ResponseEntity::ok)
            .orElse(ResponseEntity.notFound().build());
    }

    /**
     * 渲染订单详情为 JSON 预览（用内置 sales-order 模板）
     */
    @GetMapping("/{id}/preview")
    public RenderResponse preview(@PathVariable String id) throws Exception {
        Order order = orderRepository.findById(id)
            .orElseThrow(() -> new IllegalArgumentException("订单不存在: " + id));
        return engine.render(buildRequest("sales-order", List.of(orderToMap(order))));
    }

    /**
     * 导出订单 PDF
     */
    @GetMapping("/{id}/export/pdf")
    public ResponseEntity<byte[]> exportPdf(@PathVariable String id) throws Exception {
        Order order = orderRepository.findById(id)
            .orElseThrow(() -> new IllegalArgumentException("订单不存在: " + id));
        byte[] pdf = engine.exportPdf(buildRequest("sales-order", List.of(orderToMap(order))));
        return ResponseEntity.ok()
            .header("Content-Disposition", "attachment; filename=order-" + id + ".pdf")
            .contentType(org.springframework.http.MediaType.APPLICATION_PDF)
            .body(pdf);
    }

    /**
     * 导出订单 Excel
     */
    @GetMapping("/{id}/export/excel")
    public ResponseEntity<byte[]> exportExcel(@PathVariable String id) throws Exception {
        Order order = orderRepository.findById(id)
            .orElseThrow(() -> new IllegalArgumentException("订单不存在: " + id));
        byte[] xlsx = engine.exportExcel(buildRequest("sales-order", List.of(orderToMap(order))));
        return ResponseEntity.ok()
            .header("Content-Disposition", "attachment; filename=order-" + id + ".xlsx")
            .contentType(org.springframework.http.MediaType.parseMediaType(
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"))
            .body(xlsx);
    }

    /**
     * 销售汇总报表（多订单 detail）
     */
    @GetMapping("/report/summary/preview")
    public RenderResponse summaryPreview() throws Exception {
        return engine.render(buildRequest("sales-summary",
            orderRepository.findAll().stream().map(this::orderToMap).toList()));
    }

    @GetMapping("/report/summary/export/pdf")
    public ResponseEntity<byte[]> summaryPdf() throws Exception {
        byte[] pdf = engine.exportPdf(buildRequest("sales-summary",
            orderRepository.findAll().stream().map(this::orderToMap).toList()));
        return ResponseEntity.ok()
            .header("Content-Disposition", "attachment; filename=sales-summary.pdf")
            .contentType(org.springframework.http.MediaType.APPLICATION_PDF)
            .body(pdf);
    }

    private RenderRequest buildRequest(String templateId, List<Map<String, Object>> orders) {
        RenderRequest req = new RenderRequest();
        req.setTemplateJson(Templates.load(templateId));
        Map<String, List<Map<String, Object>>> data = new HashMap<>();
        data.put("orders", orders);
        req.setData(data);
        return req;
    }

    private Map<String, Object> orderToMap(Order o) {
        Map<String, Object> m = new HashMap<>();
        m.put("id", o.getId());
        m.put("customer", o.getCustomer());
        m.put("product", o.getProduct());
        m.put("quantity", o.getQuantity());
        m.put("unitPrice", o.getUnitPrice());
        m.put("total", o.getTotal());
        m.put("orderDate", o.getOrderDate().toString());
        return m;
    }
}
