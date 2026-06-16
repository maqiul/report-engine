package com.reportengine.lib.model;

import java.util.List;
import java.util.Map;

/**
 * 渲染请求模型
 */
public class RenderRequest {
    private String templateJson;
    private Map<String, List<Map<String, Object>>> data;

    public RenderRequest() {}

    public RenderRequest(String templateJson, Map<String, List<Map<String, Object>>> data) {
        this.templateJson = templateJson;
        this.data = data;
    }

    public String getTemplateJson() { return templateJson; }
    public void setTemplateJson(String templateJson) { this.templateJson = templateJson; }

    public Map<String, List<Map<String, Object>>> getData() { return data; }
    public void setData(Map<String, List<Map<String, Object>>> data) { this.data = data; }
}
