package com.reportengine.lib.renderer;

import java.util.ArrayList;
import java.util.List;

/**
 * 长文本自动换行（三端一致规范 v1）
 *
 * <p>规范（.NET / Java / 前端必须完全一致，否则跨端一致性校验会失败）：
 * <ol>
 *   <li>fontMm = fontSize(pt) * 25.4 / 72</li>
 *   <li>字符宽度（em）：CJK / 全角标点 = 1.0，其余 = 0.5</li>
 *   <li>行容量（em）= widthMm / fontMm（最小 1）</li>
 *   <li>逐字符累加，超出容量则换行（行首不额外留空格）</li>
 *   <li>行高 lineHeightMm = fontMm * 1.2</li>
 *   <li>显式 \r\n / \r / \n 先按行拆分，再各自软换行</li>
 *   <li>maxLines > 0 时截断到 maxLines 行，末行末尾用 "…" 结尾</li>
 * </ol>
 */
public final class TextWrapper {

    /** 行距系数（相对字号） */
    public static final double LINE_HEIGHT_FACTOR = 1.2;

    /** 截断标记 */
    public static final String ELLIPSIS = "…";

    private TextWrapper() {
    }

    /** 字号(pt) → 毫米 */
    public static double fontMm(double fontSizePt) {
        double mm = fontSizePt * 25.4 / 72.0;
        return mm > 0 ? mm : 1.0;
    }

    /** 行高（mm） */
    public static double lineHeightMm(double fontSizePt) {
        return fontMm(fontSizePt) * LINE_HEIGHT_FACTOR;
    }

    /** 自动换行，返回文本行集合 */
    public static List<String> wrap(String text, double widthMm, double fontSizePt, int maxLines) {
        List<String> lines = new ArrayList<>();
        if (text == null || text.isEmpty()) {
            lines.add("");
            return lines;
        }

        double fontMm = fontMm(fontSizePt);
        double capacity = widthMm / fontMm;
        if (capacity <= 0) capacity = 1;

        String normalized = text.replace("\r\n", "\n").replace('\r', '\n');
        for (String rawLine : normalized.split("\n", -1)) {
            StringBuilder cur = new StringBuilder();
            double curWidth = 0;
            for (int i = 0; i < rawLine.length(); i++) {
                char ch = rawLine.charAt(i);
                double w = isFullWidth(ch) ? 1.0 : 0.5;
                if (curWidth + w > capacity && cur.length() > 0) {
                    lines.add(cur.toString());
                    cur.setLength(0);
                    curWidth = 0;
                }
                cur.append(ch);
                curWidth += w;
            }
            lines.add(cur.toString());
        }

        if (maxLines > 0 && lines.size() > maxLines) {
            lines = new ArrayList<>(lines.subList(0, maxLines));
            int last = lines.size() - 1;
            String s = lines.get(last);
            lines.set(last, s.length() > 0 ? s.substring(0, s.length() - 1) + ELLIPSIS : ELLIPSIS);
        }

        return lines;
    }

    /**
     * 自动换行结果
     */
    public static class WrapResult {
        public final List<String> lines;
        public final double lineHeight;
        public final double grownHeight;

        WrapResult(List<String> lines, double lineHeight, double grownHeight) {
            this.lines = lines;
            this.lineHeight = lineHeight;
            this.grownHeight = grownHeight;
        }
    }

    /** 自动换行并按 canGrow 撑高 */
    public static WrapResult wrapAndGrow(String text, double widthMm, double heightMm,
                                         double fontSizePt, boolean canGrow, int maxLines) {
        List<String> lines = wrap(text, widthMm, fontSizePt, maxLines);
        double lh = lineHeightMm(fontSizePt);
        double grown = canGrow ? lines.size() * lh : 0;
        return new WrapResult(lines, lh, grown);
    }

    /** 是否全角字符（CJK / 全角标点） */
    public static boolean isFullWidth(char ch) {
        return ch >= 0x1100 && (
            ch <= 0x115F ||
            ch == 0x2329 || ch == 0x232A ||
            (ch >= 0x2E80 && ch <= 0xA4CF) ||
            (ch >= 0xAC00 && ch <= 0xD7A3) ||
            (ch >= 0xF900 && ch <= 0xFAFF) ||
            (ch >= 0xFE10 && ch <= 0xFE19) ||
            (ch >= 0xFE30 && ch <= 0xFE6F) ||
            (ch >= 0xFF00 && ch <= 0xFF60) ||
            (ch >= 0xFFE0 && ch <= 0xFFE6) ||
            (ch >= 0x3000 && ch <= 0x303E));
    }
}
