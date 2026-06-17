package com.reportengine.lib;

import org.junit.jupiter.api.Test;
import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;

/**
 * Sample test - 验证 JUnit 5 框架是否就绪
 */
class SampleTest {

    @Test
    void shouldRunBasicAssertion() {
        assertEquals(2, 1 + 1);
    }

    @Test
    void shouldHandleString() {
        String hello = "ReportEngine";
        assertTrue(hello.startsWith("Report"));
    }
}
