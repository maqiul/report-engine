package com.reportengine.lib.exporter;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.itextpdf.kernel.colors.ColorConstants;
import com.itextpdf.kernel.events.Event;
import com.itextpdf.kernel.events.IEventHandler;
import com.itextpdf.kernel.events.PdfDocumentEvent;
import com.itextpdf.kernel.font.PdfFont;
import com.itextpdf.kernel.font.PdfFontFactory;
import com.itextpdf.kernel.geom.PageSize;
import com.itextpdf.kernel.geom.Rectangle;
import com.itextpdf.kernel.pdf.PdfDocument;
import com.itextpdf.kernel.pdf.PdfPage;
import com.itextpdf.kernel.pdf.PdfWriter;
import com.itextpdf.kernel.pdf.canvas.PdfCanvas;
import com.itextpdf.layout.Canvas;
import com.itextpdf.layout.Document;
import com.itextpdf.layout.borders.SolidBorder;
import com.itextpdf.layout.element.Cell;
import com.itextpdf.layout.element.Paragraph;
import com.itextpdf.layout.element.Table;
import com.itextpdf.layout.properties.HorizontalAlignment;
import com.itextpdf.layout.properties.TextAlignment;
import com.itextpdf.layout.properties.UnitValue;
import com.reportengine.lib.model.RenderRequest;
import com.reportengine.lib.renderer.ReportRenderer;

import java.io.ByteArrayOutputStream;
import java.util.*;

/**
 * PDF 导出器 - 使用 ReportRenderer 渲染 + iText 排版
 * 
 * 支持：
 * - 中文字体（系统 STSONG.TTF）
 * - 模板元素按坐标定位
 * - pageFooter band 转 PDF 页脚，支持 {{page}} {{totalPages}} 变量
 * - 跨页 detail 重复 pageHeader
 */
public class PdfExporter {

    private final ObjectMapper objectMapper = new ObjectMapper();
    private List<JsonNode> pageFooterElements = new ArrayList<>();
    private int totalPages = 1;

    public byte[] export(RenderRequest request) throws Exception {
        JsonNode template = objectMapper.readTree(request.getTemplateJson());
        
        // 收集 pageFooter band 元素（用于页脚渲染）
        pageFooterElements = new ArrayList<>();
        if (template.has("bands")) {
            for (JsonNode band : template.get("bands")) {
                if ("pageFooter".equals(band.get("type").asText()) && band.has("elements")) {
                    for (JsonNode el : band.get("elements")) {
                        pageFooterElements.add(el);
                    }
                }
            }
        }
        
        // 页面尺寸
        double pageWidth = 210, pageHeight = 297;
        double marginTop = 0, marginBottom = 0, marginLeft = 0, marginRight = 0;
        if (template.has("page")) {
            JsonNode page = template.get("page");
            if (page.has("width")) pageWidth = page.get("width").asDouble();
            if (page.has("height")) pageHeight = page.get("height").asDouble();
            if (page.has("margin")) {
                JsonNode margin = page.get("margin");
                if (margin.has("top")) marginTop = margin.get("top").asDouble();
                if (margin.has("bottom")) marginBottom = margin.get("bottom").asDouble();
                if (margin.has("left")) marginLeft = margin.get("left").asDouble();
                if (margin.has("right")) marginRight = margin.get("right").asDouble();
            }
        }
        
        // 渲染所有元素
        ReportRenderer renderer = new ReportRenderer();
        var response = renderer.render(request);
        var elements = response.getPages().get(0).getElements();
        int totalPages = response.getTotalPages();
        this.totalPages = totalPages;
        
        // 创建 PDF（自定义页面大小）
        ByteArrayOutputStream baos = new ByteArrayOutputStream();
        PdfWriter writer = new PdfWriter(baos);
        PdfDocument pdf = new PdfDocument(writer);
        PageSize pageSize = new PageSize(
            (float) mmToPt(pageWidth),
            (float) mmToPt(pageHeight)
        );
        PdfPage pdfPage = pdf.addNewPage(pageSize);
        
        // 注册页脚事件
        pdf.addEventHandler(PdfDocumentEvent.END_PAGE, new FooterEventHandler(pageWidth, pageHeight, marginBottom, marginLeft, marginRight));
        
        Document document = new Document(pdf, pageSize);
        // 设置边距
        document.setMargins(
            (float) mmToPt(marginTop),
            (float) mmToPt(marginRight),
            (float) mmToPt(marginBottom),
            (float) mmToPt(marginLeft)
        );
        
        // 中文字体
        PdfFont chineseFont = PdfFontFactory.createFont(
            resolveFontPath(),
            PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED
        );
        
        // 渲染所有元素（除 pageFooter 外）
        for (var el : elements) {
            renderElement(document, el, chineseFont);
        }
        
        document.close();
        return baos.toByteArray();
    }
    
    private void renderElement(Document document, com.reportengine.lib.model.RenderResponse.RenderedElementInfo el, PdfFont font) {
        if ("text".equals(el.getType())) {
            Paragraph p = new Paragraph(el.getText() != null ? el.getText() : "")
                .setFont(font)
                .setFontSize(11);
            
            if (el.getFont() != null) {
                if (el.getFont().getSize() > 0) p.setFontSize((float) el.getFont().getSize());
                if (el.getFont().isBold()) p.setBold();
                if (el.getFont().isItalic()) p.setItalic();
            }
            
            if (el.getAlignment() != null) {
                p.setTextAlignment(parseAlignment(el.getAlignment()));
            }
            
            // 绝对定位（PDF 坐标系 y 从底部算）
            Rectangle ps = document.getPdfDocument().getDefaultPageSize();
            float x = (float) mmToPt(el.getX());
            float yTop = (float) mmToPt(el.getY());
            float w = (float) mmToPt(el.getWidth());
            float h = (float) mmToPt(el.getHeight());
            
            p.setFixedPosition(x, ps.getHeight() - yTop - h, w);
            document.add(p);
        }
    }
    
    private TextAlignment parseAlignment(String align) {
        return switch (align) {
            case "center" -> TextAlignment.CENTER;
            case "right" -> TextAlignment.RIGHT;
            default -> TextAlignment.LEFT;
        };
    }
    
    private double mmToPt(double mm) {
        return mm * 2.83465;
    }
    
    /**
     * 页脚事件处理器 - 在每页底部画 pageFooter 元素
     */
    private class FooterEventHandler implements IEventHandler {
        private final double pageWidth;
        private final double pageHeight;
        private final double marginBottom;
        private final double marginLeft;
        private final double marginRight;
        
        public FooterEventHandler(double pageWidth, double pageHeight, double marginBottom, double marginLeft, double marginRight) {
            this.pageWidth = pageWidth;
            this.pageHeight = pageHeight;
            this.marginBottom = marginBottom;
            this.marginLeft = marginLeft;
            this.marginRight = marginRight;
        }
        
        @Override
        public void handleEvent(Event event) {
            PdfDocumentEvent docEvent = (PdfDocumentEvent) event;
            PdfPage page = docEvent.getPage();
            int pageNum = docEvent.getDocument().getPageNumber(page);
            
            try {
                PdfFont font = PdfFontFactory.createFont(
                    resolveFontPath(),
                    PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED
                );
                
                Rectangle pageRect = page.getPageSize();
                PdfCanvas canvas = new PdfCanvas(page);
                Canvas pdfCanvas = new Canvas(canvas, pageRect);
                
                for (JsonNode el : pageFooterElements) {
                    String text = el.has("text") ? el.get("text").asText() : "";
                    String align = el.has("alignment") ? el.get("alignment").asText() : "left";
                    
                    // 替换页码变量
                    text = text.replace("{{page}}", String.valueOf(pageNum));
                    text = text.replace("{{totalPages}}", String.valueOf(totalPages));
                    
                    // 元素位置
                    double x = el.has("x") ? el.get("x").asDouble() : 0;
                    double y = el.has("y") ? el.get("y").asDouble() : 0;
                    double w = el.has("width") ? el.get("width").asDouble() : pageWidth;
                    double h = el.has("height") ? el.get("height").asDouble() : 6;
                    
                    Paragraph p = new Paragraph(text)
                        .setFont(font)
                        .setFontSize(10);
                    
                    if ("center".equals(align)) {
                        p.setTextAlignment(TextAlignment.CENTER);
                    } else if ("right".equals(align)) {
                        p.setTextAlignment(TextAlignment.RIGHT);
                    }
                    
                    float px = (float) mmToPt(marginLeft + x);
                    float py = (float) mmToPt(marginBottom + y);
                    float pw = (float) mmToPt(w);
                    float ph = (float) mmToPt(h);
                    
                    p.setFixedPosition(px, py, pw);
                    pdfCanvas.add(p);
                }
                
                pdfCanvas.close();
            } catch (Exception e) {
                // 忽略字体错误
            }
        }
    }

    /**
     * 解析中文字体路径
     * 优先级：
     *   1. 环境变量 REPORT_ENGINE_FONT
     *   2. 当前工作目录下的 fonts/STSONG.TTF
     *   3. classpath:/fonts/STSONG.TTF
     *   4. Windows 系统字体目录
     *   5. Linux 常见字体目录（/usr/share/fonts/...）
     */
    static String resolveFontPath() {
        String env = System.getenv("REPORT_ENGINE_FONT");
        if (env != null && !env.isEmpty() && new java.io.File(env).exists()) {
            return env;
        }
        String cwd = System.getProperty("user.dir");
        java.io.File local = new java.io.File(cwd, "fonts/STSONG.TTF");
        if (local.exists()) return local.getAbsolutePath();

        // classpath 查找
        try {
            java.net.URL url = PdfExporter.class.getResource("/fonts/STSONG.TTF");
            if (url != null && "file".equals(url.getProtocol())) {
                return new java.io.File(url.toURI()).getAbsolutePath();
            }
        } catch (Exception ignore) { }

        // Windows 系统目录
        String os = System.getProperty("os.name").toLowerCase();
        if (os.contains("win")) {
            String win = "C:/Windows/Fonts/STSONG.TTF";
            if (new java.io.File(win).exists()) return win;
        }

        // Linux 常见目录
        String[] linuxPaths = {
            "/usr/share/fonts/truetype/wqy/wqy-microhei.ttc",
            "/usr/share/fonts/wqy-microhei/wqy-microhei.ttc",
            "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
            "/app/fonts/STSONG.TTF"
        };
        for (String p : linuxPaths) {
            if (new java.io.File(p).exists()) return p;
        }

        // 都没有就回退到 Windows 路径（让 iText 报明确错）
        return "C:/Windows/Fonts/STSONG.TTF";
    }
}
