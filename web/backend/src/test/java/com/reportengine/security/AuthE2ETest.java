package com.reportengine.security;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.reportengine.BackendApplication;
import org.junit.jupiter.api.MethodOrderer;
import org.junit.jupiter.api.Order;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.TestMethodOrder;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.http.MediaType;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.MvcResult;
import org.springframework.test.web.servlet.setup.MockMvcBuilders;
import org.springframework.web.context.WebApplicationContext;

import java.util.Map;

import static org.springframework.security.test.web.servlet.setup.SecurityMockMvcConfigurers.springSecurity;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.*;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.*;

@SpringBootTest(classes = BackendApplication.class)
@TestMethodOrder(MethodOrderer.OrderAnnotation.class)
class AuthE2ETest {

    @Autowired
    private WebApplicationContext context;

    private MockMvc mockMvc;
    private final ObjectMapper json = new ObjectMapper();

    private void setupMvc() {
        if (mockMvc == null) {
            mockMvc = MockMvcBuilders.webAppContextSetup(context).apply(springSecurity()).build();
        }
    }

    @Test
    @Order(1)
    void register_then_login_then_access_protected() throws Exception {
        setupMvc();
        String username = "alice" + System.currentTimeMillis();

        // 1. 注册
        String regBody = json.writeValueAsString(Map.of(
            "username", username,
            "password", "secret123",
            "role", "USER"
        ));
        MvcResult regResult = mockMvc.perform(post("/api/auth/register")
                .contentType(MediaType.APPLICATION_JSON)
                .content(regBody))
            .andExpect(status().isOk())
            .andExpect(jsonPath("$.token").exists())
            .andExpect(jsonPath("$.username").value(username))
            .andExpect(jsonPath("$.role").value("USER"))
            .andReturn();
        String token = json.readTree(regResult.getResponse().getContentAsString()).get("token").asText();

        // 2. /me 用 token
        mockMvc.perform(get("/api/auth/me").header("Authorization", "Bearer " + token))
            .andExpect(status().isOk())
            .andExpect(jsonPath("$.username").value(username));

        // 3. 访问受保护接口（template list）需要 token
        mockMvc.perform(get("/api/templates"))
            .andExpect(status().isUnauthorized());

        mockMvc.perform(get("/api/templates").header("Authorization", "Bearer " + token))
            .andExpect(status().isOk());

        // 4. 重名注册 → 400
        mockMvc.perform(post("/api/auth/register")
                .contentType(MediaType.APPLICATION_JSON)
                .content(regBody))
            .andExpect(status().isBadRequest())
            .andExpect(jsonPath("$.error").value(org.hamcrest.Matchers.containsString("已存在")));
    }

    @Test
    @Order(2)
    void login_wrong_password_returns_401() throws Exception {
        setupMvc();
        String username = "bob" + System.currentTimeMillis();
        String regBody = json.writeValueAsString(Map.of(
            "username", username,
            "password", "rightpass",
            "role", "USER"
        ));
        mockMvc.perform(post("/api/auth/register")
                .contentType(MediaType.APPLICATION_JSON)
                .content(regBody))
            .andExpect(status().isOk());

        // 错密码
        mockMvc.perform(post("/api/auth/login")
                .contentType(MediaType.APPLICATION_JSON)
                .content(json.writeValueAsString(Map.of(
                    "username", username,
                    "password", "wrongpass"
                ))))
            .andExpect(status().isUnauthorized())
            .andExpect(jsonPath("$.error").value(org.hamcrest.Matchers.containsString("错误")));

        // 对密码
        mockMvc.perform(post("/api/auth/login")
                .contentType(MediaType.APPLICATION_JSON)
                .content(json.writeValueAsString(Map.of(
                    "username", username,
                    "password", "rightpass"
                ))))
            .andExpect(status().isOk())
            .andExpect(jsonPath("$.token").exists());
    }

    @Test
    @Order(3)
    void register_short_password_rejected() throws Exception {
        setupMvc();
        mockMvc.perform(post("/api/auth/register")
                .contentType(MediaType.APPLICATION_JSON)
                .content(json.writeValueAsString(Map.of(
                    "username", "carol" + System.currentTimeMillis(),
                    "password", "123"
                ))))
            .andExpect(status().isBadRequest())
            .andExpect(jsonPath("$.error").value(org.hamcrest.Matchers.containsString("6 位")));
    }

    @Test
    @Order(4)
    void invalid_token_returns_401_on_protected() throws Exception {
        setupMvc();
        mockMvc.perform(get("/api/templates").header("Authorization", "Bearer invalid.token.here"))
            .andExpect(status().isUnauthorized());
    }

    @Test
    @Order(5)
    void admin_role_can_be_assigned() throws Exception {
        setupMvc();
        String username = "admin" + System.currentTimeMillis();
        MvcResult r = mockMvc.perform(post("/api/auth/register")
                .contentType(MediaType.APPLICATION_JSON)
                .content(json.writeValueAsString(Map.of(
                    "username", username,
                    "password", "adminpass",
                    "role", "ADMIN"
                ))))
            .andExpect(status().isOk())
            .andReturn();
        String token = json.readTree(r.getResponse().getContentAsString()).get("token").asText();
        mockMvc.perform(get("/api/auth/me").header("Authorization", "Bearer " + token))
            .andExpect(status().isOk())
            .andExpect(jsonPath("$.role").value("ADMIN"));
    }
}
