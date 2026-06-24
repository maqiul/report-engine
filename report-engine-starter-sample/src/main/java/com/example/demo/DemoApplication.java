package com.example.demo;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;

/**
 * 第三方项目的 Spring Boot 启动入口
 *
 * 注意：本项目源码里完全没有 ReportEngine 的实现类，
 * 全部依赖 starter 自动配置 + 自动暴露的 REST 端点。
 */
@SpringBootApplication
public class DemoApplication {
    public static void main(String[] args) {
        SpringApplication.run(DemoApplication.class, args);
    }
}