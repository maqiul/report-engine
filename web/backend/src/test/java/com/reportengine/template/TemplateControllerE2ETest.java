package com.reportengine.template;

import com.fasterxml.jackson.databind.ObjectMapper;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.http.MediaType;
import org.springframework.security.test.context.support.WithMockUser;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.setup.MockMvcBuilders;
import org.springframework.web.context.WebApplicationContext;

import static org.springframework.security.test.web.servlet.setup.SecurityMockMvcConfigurers.springSecurity;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.*;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.*;

/**
 * TemplateController 端到端测试
 *
 * - 使用真实 H2 数据库（@SpringBootTest 默认）
 * - 每个测试前清空表
 * - @WithMockUser 绕过 JWT，直接注入 ROLE_USER
 */
@SpringBootTest
@WithMockUser(username = "tester", roles = "USER")
@DisplayName("模板 CRUD E2E")
class TemplateControllerE2ETest {

    @Autowired
    private WebApplicationContext context;

    @Autowired
    private TemplateRepository repo;

    private MockMvc mockMvc;
    private final ObjectMapper json = new ObjectMapper();

    @BeforeEach
    void setup() {
        mockMvc = MockMvcBuilders.webAppContextSetup(context).apply(springSecurity()).build();
        repo.deleteAll();
    }

    private static final String VALID_JSON = "{\"version\":\"1.0\",\"bands\":[]}";

    @Test
    @DisplayName("1. 初始列表为空")
    void list_empty() throws Exception {
        mockMvc.perform(get("/api/templates"))
            .andExpect(status().isOk())
            .andExpect(content().contentTypeCompatibleWith(MediaType.APPLICATION_JSON))
            .andExpect(content().json("[]"));
    }

    @Test
    @DisplayName("2. 创建模板 - 成功")
    void create_success() throws Exception {
        String body = json.writeValueAsString(java.util.Map.of(
            "name", "sales_order",
            "category", "销售",
            "description", "销售订单报表",
            "templateJson", VALID_JSON
        ));

        mockMvc.perform(post("/api/templates")
                .contentType(MediaType.APPLICATION_JSON)
                .content(body))
            .andExpect(status().isCreated())
            .andExpect(jsonPath("$.id").exists())
            .andExpect(jsonPath("$.name").value("sales_order"))
            .andExpect(jsonPath("$.category").value("销售"))
            .andExpect(jsonPath("$.version").value(1));
    }

    @Test
    @DisplayName("3. 创建模板 - 名称重复返回 400")
    void create_duplicate_name() throws Exception {
        // 先建一个
        Template t = new Template();
        t.setName("dup");
        t.setTemplateJson(VALID_JSON);
        repo.save(t);

        // 再尝试同名
        String body = json.writeValueAsString(java.util.Map.of(
            "name", "dup",
            "templateJson", VALID_JSON
        ));

        mockMvc.perform(post("/api/templates")
                .contentType(MediaType.APPLICATION_JSON)
                .content(body))
            .andExpect(status().isBadRequest())
            .andExpect(jsonPath("$.error").value(org.hamcrest.Matchers.containsString("名称已存在")));
    }

    @Test
    @DisplayName("4. 创建模板 - 非法 JSON 返回 400")
    void create_invalid_json() throws Exception {
        String body = json.writeValueAsString(java.util.Map.of(
            "name", "bad",
            "templateJson", "not-json"
        ));

        mockMvc.perform(post("/api/templates")
                .contentType(MediaType.APPLICATION_JSON)
                .content(body))
            .andExpect(status().isBadRequest())
            .andExpect(jsonPath("$.error").value(org.hamcrest.Matchers.containsString("不是合法 JSON")));
    }

    @Test
    @DisplayName("5. 按 ID 查找")
    void getById() throws Exception {
        Template t = new Template();
        t.setName("findme");
        t.setCategory("测试");
        t.setTemplateJson(VALID_JSON);
        Template saved = repo.save(t);

        mockMvc.perform(get("/api/templates/" + saved.getId()))
            .andExpect(status().isOk())
            .andExpect(jsonPath("$.name").value("findme"))
            .andExpect(jsonPath("$.category").value("测试"));
    }

    @Test
    @DisplayName("6. 按 ID 查找 - 不存在返回 404")
    void getById_notFound() throws Exception {
        mockMvc.perform(get("/api/templates/99999"))
            .andExpect(status().isNotFound());
    }

    @Test
    @DisplayName("7. 按名称查找")
    void getByName() throws Exception {
        Template t = new Template();
        t.setName("byname");
        t.setTemplateJson(VALID_JSON);
        repo.save(t);

        mockMvc.perform(get("/api/templates/by-name/byname"))
            .andExpect(status().isOk())
            .andExpect(jsonPath("$.name").value("byname"));
    }

    @Test
    @DisplayName("8. 更新模板 - 版本号自增")
    void update_versionIncrement() throws Exception {
        Template t = new Template();
        t.setName("updateme");
        t.setTemplateJson(VALID_JSON);
        Template saved = repo.save(t);
        long id = saved.getId();

        String body = json.writeValueAsString(java.util.Map.of(
            "name", "updateme",
            "description", "new description",
            "templateJson", "{\"version\":\"1.0\",\"bands\":[1,2,3]}"
        ));

        mockMvc.perform(put("/api/templates/" + id)
                .contentType(MediaType.APPLICATION_JSON)
                .content(body))
            .andExpect(status().isOk())
            .andExpect(jsonPath("$.version").value(2))
            .andExpect(jsonPath("$.description").value("new description"));
    }

    @Test
    @DisplayName("9. 更新不存在的模板返回 404")
    void update_notFound() throws Exception {
        String body = json.writeValueAsString(java.util.Map.of(
            "name", "ghost",
            "templateJson", VALID_JSON
        ));

        mockMvc.perform(put("/api/templates/99999")
                .contentType(MediaType.APPLICATION_JSON)
                .content(body))
            .andExpect(status().isNotFound());
    }

    @Test
    @DisplayName("10. 按分类筛选")
    void list_byCategory() throws Exception {
        Template a = new Template();
        a.setName("a");
        a.setCategory("销售");
        a.setTemplateJson(VALID_JSON);
        repo.save(a);

        Template b = new Template();
        b.setName("b");
        b.setCategory("财务");
        b.setTemplateJson(VALID_JSON);
        repo.save(b);

        mockMvc.perform(get("/api/templates").param("category", "销售"))
            .andExpect(status().isOk())
            .andExpect(jsonPath("$.length()").value(1))
            .andExpect(jsonPath("$[0].name").value("a"));
    }

    @Test
    @DisplayName("11. 删除模板")
    void delete_success() throws Exception {
        Template t = new Template();
        t.setName("deleteMe");
        t.setTemplateJson(VALID_JSON);
        Template saved = repo.save(t);

        mockMvc.perform(delete("/api/templates/" + saved.getId()))
            .andExpect(status().isNoContent());

        // 再查应该 404
        mockMvc.perform(get("/api/templates/" + saved.getId()))
            .andExpect(status().isNotFound());
    }
}