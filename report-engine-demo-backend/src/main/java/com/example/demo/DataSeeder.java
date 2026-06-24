package com.example.demo;

import org.springframework.beans.factory.annotation.Value;
import org.springframework.boot.CommandLineRunner;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

import java.math.BigDecimal;
import java.time.LocalDate;
import java.util.List;

/**
 * 启动时种入演示数据：3 个销售订单
 */
@Configuration
public class DataSeeder {

    @Bean
    CommandLineRunner seedOrders(OrderRepository repo) {
        return args -> {
            if (repo.count() > 0) return;
            repo.saveAll(List.of(
                new Order("SO-001", "Acme Corp", "Widget Pro", 10, new BigDecimal("199.00"), LocalDate.of(2026, 1, 15)),
                new Order("SO-002", "Globex Inc", "Gadget Plus", 5, new BigDecimal("299.50"), LocalDate.of(2026, 2, 3)),
                new Order("SO-003", "Initech Ltd", "Sprocket X", 25, new BigDecimal("49.99"), LocalDate.of(2026, 3, 22))
            ));
        };
    }
}
