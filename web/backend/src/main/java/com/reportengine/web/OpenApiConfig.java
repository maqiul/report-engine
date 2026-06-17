package com.reportengine.web;

import io.swagger.v3.oas.models.OpenAPI;
import io.swagger.v3.oas.models.info.Contact;
import io.swagger.v3.oas.models.info.Info;
import io.swagger.v3.oas.models.info.License;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

/**
 * OpenAPI / Swagger 配置
 *
 * 访问地址:
 * - JSON: http://localhost:5000/v3/api-docs
 * - UI:   http://localhost:5000/swagger-ui.html
 */
@Configuration
public class OpenApiConfig {

    @Bean
    public OpenAPI reportEngineOpenApi() {
        return new OpenAPI()
            .info(new Info()
                .title("ReportEngine API")
                .description("报表引擎后端 API - 提供报表预览、PDF/Excel 导出能力")
                .version("0.2.0")
                .contact(new Contact()
                    .name("老马")
                    .email("maqiuliang4@163.com")
                    .url("https://github.com/maqiul/report-engine"))
                .license(new License()
                    .name("MIT")
                    .url("https://github.com/maqiul/report-engine/blob/main/LICENSE")));
    }
}
