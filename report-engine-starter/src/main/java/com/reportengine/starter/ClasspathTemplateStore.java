package com.reportengine.starter;

import com.reportengine.lib.store.TemplateStore;
import org.springframework.core.io.Resource;
import org.springframework.core.io.support.PathMatchingResourcePatternResolver;
import org.springframework.core.io.support.ResourcePatternResolver;

import java.io.InputStream;
import java.nio.charset.StandardCharsets;
import java.util.HashMap;
import java.util.Map;
import java.util.Optional;

/**
 * 默认 TemplateStore：从 classpath:/templates/ 加载 .rptx 文件
 *
 * 启动时扫描一次缓存到内存，热更新可由别人项目自己实现 TemplateStore 替换。
 *
 * 文件命名约定：{id}.rptx → loadById("{id}") 命中
 *
 * @since 0.3.0
 */
public class ClasspathTemplateStore implements TemplateStore {

    private final Map<String, String> cache = new HashMap<>();

    public ClasspathTemplateStore() {
        try {
            ResourcePatternResolver resolver = new PathMatchingResourcePatternResolver();
            Resource[] resources = resolver.getResources("classpath*:templates/**/*.rptx");
            for (Resource r : resources) {
                String filename = r.getFilename();
                if (filename == null || !filename.endsWith(".rptx")) continue;
                String id = filename.substring(0, filename.length() - 5);
                try (InputStream in = r.getInputStream()) {
                    cache.put(id, new String(in.readAllBytes(), StandardCharsets.UTF_8));
                }
            }
        } catch (Exception ignored) {
            // 没有 templates 目录也不报错，只是空 cache
        }
    }

    @Override
    public Optional<String> loadById(String templateId) {
        return Optional.ofNullable(cache.get(templateId));
    }

    /** 测试用：查看缓存大小 */
    public int size() { return cache.size(); }

    /** 测试用：手动放一个模板 */
    public void put(String id, String json) { cache.put(id, json); }
}