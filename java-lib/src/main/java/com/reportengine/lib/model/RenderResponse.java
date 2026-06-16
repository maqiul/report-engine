package com.reportengine.lib.model;

import java.util.List;

/**
 * 渲染响应模型
 */
public class RenderResponse {
    private boolean success;
    private String error;
    private List<PageInfo> pages;
    private int totalPages;

    public boolean isSuccess() { return success; }
    public void setSuccess(boolean success) { this.success = success; }

    public String getError() { return error; }
    public void setError(String error) { this.error = error; }

    public List<PageInfo> getPages() { return pages; }
    public void setPages(List<PageInfo> pages) { this.pages = pages; }

    public int getTotalPages() { return totalPages; }
    public void setTotalPages(int totalPages) { this.totalPages = totalPages; }

    public static class PageInfo {
        private int pageNumber;
        private double width;
        private double height;
        private List<RenderedElementInfo> elements;

        public int getPageNumber() { return pageNumber; }
        public void setPageNumber(int pageNumber) { this.pageNumber = pageNumber; }

        public double getWidth() { return width; }
        public void setWidth(double width) { this.width = width; }

        public double getHeight() { return height; }
        public void setHeight(double height) { this.height = height; }

        public List<RenderedElementInfo> getElements() { return elements; }
        public void setElements(List<RenderedElementInfo> elements) { this.elements = elements; }
    }

    public static class RenderedElementInfo {
        private String type;
        private double x;
        private double y;
        private double width;
        private double height;
        private String text;
        private FontInfo font;
        private String alignment;
        private String backgroundColor;
        private String borderColor;
        private Double borderWidth;

        public String getType() { return type; }
        public void setType(String type) { this.type = type; }

        public double getX() { return x; }
        public void setX(double x) { this.x = x; }

        public double getY() { return y; }
        public void setY(double y) { this.y = y; }

        public double getWidth() { return width; }
        public void setWidth(double width) { this.width = width; }

        public double getHeight() { return height; }
        public void setHeight(double height) { this.height = height; }

        public String getText() { return text; }
        public void setText(String text) { this.text = text; }

        public FontInfo getFont() { return font; }
        public void setFont(FontInfo font) { this.font = font; }

        public String getAlignment() { return alignment; }
        public void setAlignment(String alignment) { this.alignment = alignment; }

        public String getBackgroundColor() { return backgroundColor; }
        public void setBackgroundColor(String backgroundColor) { this.backgroundColor = backgroundColor; }

        public String getBorderColor() { return borderColor; }
        public void setBorderColor(String borderColor) { this.borderColor = borderColor; }

        public Double getBorderWidth() { return borderWidth; }
        public void setBorderWidth(Double borderWidth) { this.borderWidth = borderWidth; }
    }

    public static class FontInfo {
        private String family;
        private double size;
        private boolean bold;
        private boolean italic;
        private boolean underline;
        private String color;

        public String getFamily() { return family; }
        public void setFamily(String family) { this.family = family; }

        public double getSize() { return size; }
        public void setSize(double size) { this.size = size; }

        public boolean isBold() { return bold; }
        public void setBold(boolean bold) { this.bold = bold; }

        public boolean isItalic() { return italic; }
        public void setItalic(boolean italic) { this.italic = italic; }

        public boolean isUnderline() { return underline; }
        public void setUnderline(boolean underline) { this.underline = underline; }

        public String getColor() { return color; }
        public void setColor(String color) { this.color = color; }
    }
}
