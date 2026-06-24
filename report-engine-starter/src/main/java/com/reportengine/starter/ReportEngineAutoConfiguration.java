package com.reportengine.starter;

import com.reportengine.lib.ReportEngine;
import com.reportengine.lib.exporter.ExcelExporter;
import com.reportengine.lib.exporter.PdfExporter;
import com.reportengine.lib.renderer.ReportRenderer;
import com.reportengine.lib.store.TemplateStore;
import org.springframework.beans.factory.ObjectProvider;
import org.springframework.boot.autoconfigure.AutoConfiguration;
import org.springframework.boot.autoconfigure.condition.ConditionalOnClass;
import org.springframework.boot.autoconfigure.condition.ConditionalOnMissingBean;
import org.springframework.boot.autoconfigure.condition.ConditionalOnProperty;
import org.springframework.boot.context.properties.EnableConfigurationProperties;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Primary;
import org.springframework.web.servlet.config.annotation.WebMvcConfigurer;

/**
 * ReportEngine 自动配置
 *
 * 别人的 Spring Boot 项目只需要在 pom 里加：
 * <pre>
 * &lt;dependency&gt;
 *     &lt;groupId&gt;com.reportengine&lt;/groupId&gt;
 *     &lt;artifactId&gt;report-engine-spring-boot-starter&lt;/artifactId&gt;
 *     &lt;version&gt;0.3.0&lt;/version&gt;
 * &lt;/dependency&gt;
 * </pre>
 *
 * 然后 @Autowired ReportEngine 就能用了。
 *
 * @since 0.3.0
 */
@AutoConfiguration
@ConditionalOnClass(ReportEngine.class)
@EnableConfigurationProperties(ReportEngineProperties.class)
@ConditionalOnProperty(prefix = "report.engine", name = "enabled", havingValue = "true", matchIfMissing = true)
public class ReportEngineAutoConfiguration implements WebMvcConfigurer {

    /**
     * 核心 Bean：ReportRenderer
     */
    @Bean
    @ConditionalOnMissingBean
    public ReportRenderer reportRenderer() {
        return new ReportRenderer();
    }

    /**
     * 核心 Bean：PdfExporter
     */
    @Bean
    @ConditionalOnMissingBean
    public PdfExporter pdfExporter() {
        return new PdfExporter();
    }

    /**
     * 核心 Bean：ExcelExporter
     */
    @Bean
    @ConditionalOnMissingBean
    public ExcelExporter excelExporter() {
        return new ExcelExporter();
    }

    /**
     * 门面 Bean：ReportEngine（最常用的入口）
     */
    @Bean
    @ConditionalOnMissingBean
    public ReportEngine reportEngine(ReportRenderer renderer,
                                     PdfExporter pdfExporter,
                                     ExcelExporter excelExporter) {
        return new ReportEngine(renderer, pdfExporter, excelExporter);
    }

    /**
     * 默认 TemplateStore：classpath 加载。别人可自定义覆盖。
     *
     * 同时注册为 ClasspathTemplateStore 具名 Bean（@Primary），
     * 让依赖 TemplateStore 接口的地方优先注入这个。
     */
    @Bean
    @Primary
    @ConditionalOnMissingBean
    public ClasspathTemplateStore classpathTemplateStore() {
        return new ClasspathTemplateStore();
    }

    /**
     * 自动 REST 端点（开关 report.engine.auto-rest-api）
     */
    @Bean
    @ConditionalOnProperty(prefix = "report.engine", name = "auto-rest-api", havingValue = "true", matchIfMissing = true)
    public ReportRestController reportRestController(ReportEngine engine,
                                                     ReportEngineProperties props,
                                                     TemplateStore store) {
        return new ReportRestController(engine, props, store);
    }
}