package com.reportengine.template;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;
import java.util.Optional;

/**
 * 模板服务层
 *
 * - 列表 / 按 ID 查 / 按名称查
 * - 新建（名称重复抛 400）
 * - 更新（版本号 +1）
 * - 删除
 * - 模板 JSON 合法性校验（必须能解析成 JSON 对象）
 */
@Service
public class TemplateService {

    private final TemplateRepository repo;
    private final ObjectMapper objectMapper = new ObjectMapper();

    public TemplateService(TemplateRepository repo) {
        this.repo = repo;
    }

    public List<Template> listAll() {
        return repo.findAll();
    }

    public List<Template> listByCategory(String category) {
        return repo.findByCategoryOrderByUpdatedAtDesc(category);
    }

    public Optional<Template> getById(Long id) {
        return repo.findById(id);
    }

    public Optional<Template> getByName(String name) {
        return repo.findByName(name);
    }

    @Transactional
    public Template create(Template input) {
        validateTemplateJson(input.getTemplateJson());
        if (repo.existsByName(input.getName())) {
            throw new IllegalArgumentException("模板名称已存在: " + input.getName());
        }
        input.setVersion(1);
        return repo.save(input);
    }

    @Transactional
    public Template update(Long id, Template input) {
        validateTemplateJson(input.getTemplateJson());
        Template existing = repo.findById(id)
            .orElseThrow(() -> new IllegalArgumentException("模板不存在: id=" + id));

        // 名称变更时检查重复
        if (!existing.getName().equals(input.getName()) && repo.existsByName(input.getName())) {
            throw new IllegalArgumentException("模板名称已存在: " + input.getName());
        }

        existing.setName(input.getName());
        existing.setCategory(input.getCategory());
        existing.setDescription(input.getDescription());
        existing.setTemplateJson(input.getTemplateJson());
        existing.setVersion(existing.getVersion() + 1);
        return repo.save(existing);
    }

    @Transactional
    public void delete(Long id) {
        if (!repo.existsById(id)) {
            throw new IllegalArgumentException("模板不存在: id=" + id);
        }
        repo.deleteById(id);
    }

    private void validateTemplateJson(String json) {
        if (json == null || json.isBlank()) {
            throw new IllegalArgumentException("templateJson 不能为空");
        }
        try {
            JsonNode node = objectMapper.readTree(json);
            if (!node.isObject()) {
                throw new IllegalArgumentException("templateJson 必须是 JSON 对象");
            }
        } catch (Exception e) {
            throw new IllegalArgumentException("templateJson 不是合法 JSON: " + e.getMessage());
        }
    }
}
