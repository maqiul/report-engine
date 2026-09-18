# Cross-Platform Consistency Tests (.NET vs Java)

验证**同一份 `.rptx` 模板 + 同一份数据**在 .NET 与 Java 两套独立实现里渲染结果一致。

## 背景

report-engine 的 .NET（`ReportEngine.Core`）和 Java（`java-lib`）是两套独立实现的渲染引擎。
本测试量化两端一致性：哪些必须相同（断言），哪些是设计差异（记录）。

## 工作原理

```
.NET CrossConsistencyDumpTest  ──dump──▶  artifacts/dotnet-summary.json ┐
                                                                        ├─▶ check.js 比对
Java CrossConsistencyDumpTest  ──dump──▶  artifacts/java-summary.json   ┘
```

两侧测试用**逐字相同**的模板 JSON + 数据渲染，各自导出规范化摘要（`{type,text,x,w,size,align}`），
`check.js` 读两个摘要逐项比对。

## 跑一遍

```bash
tests\cross-consistency\run.cmd
```

或手动：
```bash
# 1) .NET
cd tests/ReportEngine.Core.Tests
CC_DUMP=../../tests/cross-consistency/artifacts/dotnet-summary.json \
  dotnet test -c Release --filter CrossConsistencyDumpTest
# 2) Java
cd java-lib
CC_DUMP=../tests/cross-consistency/artifacts/java-summary.json \
  ./gradlew test --tests com.reportengine.lib.CrossConsistencyDumpTest
# 3) 比对
node tests/cross-consistency/check.js
```

## 交集语法（关键约束）

模板表达式必须用**两端通用**的 `{{currentRow.xxx}}`：

| 语法 | .NET | Java |
|------|:----:|:----:|
| `{{currentRow.field}}` | ✅ | ✅ |
| `{{dataSource.field}}` | ✅ | ❌ |
| `{{field}}`（裸） | ✅ | ❌ |

跨端模板只能用第一行。fixture 即用 `{{currentRow.*}}`。

## 强一致项（断言相同）

- `pageCount`
- 文本元素多重集（内容完全一致）
- 元素类型计数
- 每个文本元素的 `x` / `width` / `font.size` / `alignment`

## 已知跨端差异（不比对，设计上可接受）

1. **y 坐标 / 分页**：.NET 多页分页（header/footer 每页重复），Java 单页流式（`currentY` 累加、恒 `totalPages=1`）→ 纵向布局算法不同，不可比。
2. **数字格式化**：.NET `FormatValue` vs Java `toString()`（如 `1990` / `1990.0` / `1990.00`）。fixture 字段全用字符串规避；真实数字渲染仍属已知差异。
3. **表达式语法超集**：.NET 支持 3 种引用，Java 仅 `currentRow`。

## 现状

✅ 简单订单模板（reportHeader + detail×3）跨端**完全一致**。
