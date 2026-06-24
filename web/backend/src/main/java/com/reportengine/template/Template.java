package com.reportengine.template;

import jakarta.persistence.*;
import java.time.Instant;

/**
 * 报表模板实体
 *
 * 存储用户保存的 .rptx 模板，含版本号、名称、描述、分类。
 */
@Entity
@Table(name = "report_templates")
public class Template {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    /** 模板名称（唯一） */
    @Column(nullable = false, length = 100, unique = true)
    private String name;

    /** 分类/标签（可选） */
    @Column(length = 50)
    private String category;

    /** 描述 */
    @Column(length = 500)
    private String description;

    /** .rptx 模板 JSON */
    @Lob
    @Column(nullable = false, columnDefinition = "CLOB")
    private String templateJson;

    /** 版本号（自增，每次保存 +1） */
    @Column(nullable = false)
    private int version = 1;

    /** 创建时间 */
    @Column(nullable = false, updatable = false)
    private Instant createdAt;

    /** 更新时间 */
    @Column(nullable = false)
    private Instant updatedAt;

    @PrePersist
    void onCreate() {
        Instant now = Instant.now();
        this.createdAt = now;
        this.updatedAt = now;
    }

    @PreUpdate
    void onUpdate() {
        this.updatedAt = Instant.now();
    }

    // ====== Getters / Setters ======

    public Long getId() { return id; }
    public void setId(Long id) { this.id = id; }

    public String getName() { return name; }
    public void setName(String name) { this.name = name; }

    public String getCategory() { return category; }
    public void setCategory(String category) { this.category = category; }

    public String getDescription() { return description; }
    public void setDescription(String description) { this.description = description; }

    public String getTemplateJson() { return templateJson; }
    public void setTemplateJson(String templateJson) { this.templateJson = templateJson; }

    public int getVersion() { return version; }
    public void setVersion(int version) { this.version = version; }

    public Instant getCreatedAt() { return createdAt; }
    public void setCreatedAt(Instant createdAt) { this.createdAt = createdAt; }

    public Instant getUpdatedAt() { return updatedAt; }
    public void setUpdatedAt(Instant updatedAt) { this.updatedAt = updatedAt; }
}
