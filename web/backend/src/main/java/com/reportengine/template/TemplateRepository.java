package com.reportengine.template;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import java.util.List;
import java.util.Optional;

/**
 * 模板 Repository
 */
@Repository
public interface TemplateRepository extends JpaRepository<Template, Long> {

    /** 按名称查找 */
    Optional<Template> findByName(String name);

    /** 按分类查找 */
    List<Template> findByCategory(String category);

    /** 按分类查找（无分类时不返回 null） */
    List<Template> findByCategoryOrderByUpdatedAtDesc(String category);

    /** 名称是否存在 */
    boolean existsByName(String name);

    /** 按分类分组统计 */
    long countByCategory(String category);
}
