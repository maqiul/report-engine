package com.reportengine.starter;

import org.springframework.boot.context.properties.ConfigurationProperties;

/**
 * ReportEngine 配置项
 *
 * application.yml 示例：
 * <pre>
 * report:
 *   engine:
 *     enabled: true
 *     auto-rest-api: true
 *     rest-prefix: /api/reports
 *     font-path: /opt/fonts/STSONG.TTF
 * </pre>
 *
 * @since 0.3.0
 */
@ConfigurationProperties(prefix = "report.engine")
public class ReportEngineProperties {

    /** 是否启用 starter（默认 true） */
    private boolean enabled = true;

    /** 是否自动暴露 REST 端点 /api/reports/** */
    private boolean autoRestApi = true;

    /** REST 端点前缀 */
    private String restPrefix = "/api/reports";

    /** 中文字体文件路径（PDF/Excel 必需）。不填时走跨平台 fallback */
    private String fontPath;

    public boolean isEnabled() { return enabled; }
    public void setEnabled(boolean enabled) { this.enabled = enabled; }

    public boolean isAutoRestApi() { return autoRestApi; }
    public void setAutoRestApi(boolean autoRestApi) { this.autoRestApi = autoRestApi; }

    public String getRestPrefix() { return restPrefix; }
    public void setRestPrefix(String restPrefix) { this.restPrefix = restPrefix; }

    public String getFontPath() { return fontPath; }
    public void setFontPath(String fontPath) { this.fontPath = fontPath; }
}