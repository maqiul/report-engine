# Cross-Platform Consistency Tests (.NET / Java / 前端)

验证**同一份 `.rptx` 模板 + 同一份数据**在 .NET、Java、前端三套渲染实现里结果一致。

## 参与者

| 端 | 实现 | dump 脚本 |
|----|------|-----------|
| .NET | `ReportEngine.Core` | `tests/ReportEngine.Core.Tests/CrossConsistencyDumpTest.cs` |
| Java | `java-lib` | `java-lib/src/test/.../CrossConsistencyDumpTest.java` |
| 前端 | `report-engine-vue` 的 `renderCore` | `tests/cross-consistency/frontend-dump.mjs` |

## 工作原理

```
.NET dump  ──▶ artifacts/dotnet-summary.json   ┐
Java dump  ──▶ artifacts/java-summary.json     ├─▶ check.js 三端两两比对
前端 dump   ──▶ artifacts/frontend-summary.json ┘
```

各端用**逐字相同**的模板 JSON + 数据渲染，各自导出规范化摘要
（`{type,text,x,w,size,align}`，几何 mm、字号 pt），`check.js` 两两比对。

前端渲染逻辑抽在 `report-engine-vue/src/render/renderCore.ts`（**纯函数，不依赖 Vue/DOM**），
所以既能被预览组件用，也能在 node 里直接跑做一致性校验。

## 跑一遍

```bash
tests\cross-consistency\run.cmd
```

或手动三步：

```bash
# 1) .NET
cd tests/ReportEngine.Core.Tests
CC_DUMP=../../tests/cross-consistency/artifacts/dotnet-summary.json \
  dotnet test -c Release --filter CrossConsistencyDumpTest
# 2) Java
cd java-lib
CC_DUMP=../tests/cross-consistency/artifacts/java-summary.json \
  ./gradlew test --tests com.reportengine.lib.CrossConsistencyDumpTest
# 3) 前端
cd report-engine-vue && npm run build
node ../tests/cross-consistency/frontend-dump.mjs
# 4) 比对
node tests/cross-consistency/check.js
```

## 交集语法（关键约束）

模板表达式必须用**两端通用**的 `{{currentRow.xxx}}`：

| 语法 | .NET | Java | 前端 |
|------|:----:|:----:|:----:|
| `{{currentRow.field}}` | ✅ | ✅ | ✅ |
| `{{dataSource.field}}` | ✅ | ❌ | ❌ |
| `{{field}}`（裸） | ✅ | ❌ | ❌ |

跨端模板只能用第一行。fixture 即用 `{{currentRow.*}}`。

## 强一致项（三端必须相同）

- `pageCount`
- 文本元素多重集（内容完全一致）
- 元素类型计数
- 每个文本元素的 `x` / `width` / `font.size` / `alignment`

## 已知跨端差异（不比对，设计上可接受）

1. **y 坐标 / 分页**：.NET 多页分页（header/footer 每页重复），Java 与前端是单页流式（`currentY` 累加、恒 1 页）→ 纵向布局算法不同，不可比。
2. **数字字段格式化**：.NET `FormatValue` vs Java `toString()`（如 `1990` / `1990.0` / `1990.00`）。fixture 字段全用字符串规避。
3. **表达式语法超集**：.NET 支持 3 种引用，Java/前端仅 `currentRow`。
4. **字体渲染**：前端用浏览器字体，后端用 PDF/Excel 字体引擎 —— 字距/行高可能有细微差异，故只比对字号数值。

## 现状

✅ 简单订单模板（reportHeader + detail×3）**三端完全一致**。
