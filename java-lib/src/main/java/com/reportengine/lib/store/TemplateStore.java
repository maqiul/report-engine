package com.reportengine.lib.store;

import java.util.Optional;

/**
 * 模板存储 SPI
 *
 * 别人项目需要自定义存储位置（DB / Redis / OSS / Git）时，实现这个接口，
 * 然后注册到 Spring 容器里，starter 会自动接管。
 *
 * 默认 starter 会提供一个基于 classpath 的内存实现。
 *
 * @since 0.3.0
 */
public interface TemplateStore {

    /**
     * 按 ID 加载模板 JSON。
     *
     * @return Optional.empty() 表示不存在
     */
    Optional<String> loadById(String templateId);

    /**
     * 按路径加载（支持子报表 / include）。
     *
     * 默认实现就是 loadById。
     */
    default Optional<String> loadByPath(String path) {
        return loadById(path);
    }
}