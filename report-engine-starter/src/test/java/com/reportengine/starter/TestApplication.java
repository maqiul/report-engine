package com.reportengine.starter;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;

/**
 * 测试用最小 Spring Boot 启动类
 *
 * 模拟"别人的项目"启动入口。
 * 真正的 starter 用户在自己项目里会有自己的 @SpringBootApplication。
 */
@SpringBootApplication
public class TestApplication {
    public static void main(String[] args) {
        SpringApplication.run(TestApplication.class, args);
    }
}