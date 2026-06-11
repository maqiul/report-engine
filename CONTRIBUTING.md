# 贡献指南

欢迎参与 ReportEngine 的开发！本文档说明开发环境、代码规范、提 PR 流程、测试要求等。

---

## 🛠️ 开发环境

### 必需

- **.NET SDK 8.0** 或更高（项目已用 .NET 10.0.203 验证，向下兼容 8.0）
- **Git** 2.30+
- **Windows 10/11**（WPF / WinForms 设计器编译目标）；Core/Tests 在 Linux/macOS 上也能 build & test
- 跨平台 IDE：Visual Studio 2022 / JetBrains Rider / VS Code + C# Dev Kit

### 推荐

- **dotnet-format** 全局工具（`dotnet tool install -g dotnet-format`）
- **PowerShell 7+** 或 **Git Bash**（Windows 上更好的 shell 体验）
- **xunit.runner.visualstudio** 已在 `tests/*.csproj` 内置，VS / Rider 测试资源管理器开箱即用

### 验证安装

```bash
dotnet --version   # 应 >= 8.0
git --version      # 应 >= 2.30
```

---

## 📁 项目结构

```
report-engine/
├── ReportEngine.slnx                       # 解决方案（slnx 简洁格式，9 + 1 项目）
├── README.md / CONTRIBUTING.md / LICENSE    # 三件套
├── ReportEngine.Core/                       # 跨平台核心（net462 + netstandard2.0 + net8.0）
│   ├── Barcodes/                            # ZXing 条码生成
│   ├── Data/                                # 表达式引擎
│   ├── Export/                              # IPdfExporter / IExcelExporter 接口
│   ├── Parsing/                             # 模板 (.rptx JSON) 解析
│   ├── Rendering/                           # 渲染引擎 (ReportRenderer + 渲染输出模型)
│   ├── SubReports/                          # 子报表解析
│   └── TemplateSchema.cs                    # 所有领域模型定义
├── ReportEngine.Export.Excel/               # ClosedXML 导出（多目标）
├── ReportEngine.Export.Pdf/                 # PdfSharpCore 导出（多目标）
├── ReportEngine.Viewer.Wpf / WinForms/      # 运行时预览（net8.0-windows）
├── ReportEngine.Designer.Wpf / WinForms/    # 可视化设计器（net8.0-windows）
├── ReportEngine.Desktop/                    # 桌面启动器
└── tests/
    ├── ReportEngine.Core.Tests/             # Core 单元测试（net8.0，40 条）
    └── ReportEngine.Export.Pdf.Tests/       # PDF 导出单元测试（net8.0，10 条）
```

---

## 🔨 本地构建

```bash
# 还原 + 编译整个解决方案
dotnet build ReportEngine.slnx -c Debug

# 编译单个项目
dotnet build ReportEngine.Core/ReportEngine.Core.csproj

# 仅跑测试
dotnet test tests/ReportEngine.Core.Tests/ReportEngine.Core.Tests.csproj
dotnet test tests/ReportEngine.Export.Pdf.Tests/ReportEngine.Export.Pdf.Tests.csproj
```

**目标框架矩阵**：

| 项目 | 目标框架 |
|------|----------|
| `ReportEngine.Core` | `net462` + `netstandard2.0` + `net8.0` |
| `ReportEngine.Export.Excel` | `net462` + `netstandard2.0` + `net8.0` |
| `ReportEngine.Export.Pdf` | `net462` + `netstandard2.0` + `net8.0` |
| `ReportEngine.Viewer.*` / `Designer.*` / `Desktop` | `net462` + `net8.0-windows` |
| `tests/*` | `net8.0` |

**所有改动必须在 4 套目标框架下编译 0 警告 0 错误**。本地全 build 命令：

```bash
dotnet build ReportEngine.slnx -nologo | find /c "error"   # 应返回 0
```

---

## 🧪 测试要求

### 写新测试

- 所有新功能 / 修 bug 必须附单元测试
- 测试项目**只锁 `net8.0`**，不引入多目标（CI 时间成本）
- 框架：**xunit 2.9.0** + **FluentAssertions 6.12.0**
- 命名规范：`<MethodName>_<Condition>_<Expected>` 或描述性句子
- 测试工厂函数（`BuildReport` / `Text`）复用 `ClosedXmlExporterTests` 风格

### 跑测试

```bash
# 全测试
dotnet test ReportEngine.slnx

# 单个测试类
dotnet test --filter "FullyQualifiedName~PdfSharpExporterTests"

# 单个测试方法
dotnet test --filter "Name=Export_With_Only_Text"
```

**当前基准**：50 / 0 / 0（全绿，v0.1.7 状态）。PR 提交前必须保持全绿。

### 覆盖率指引

| 模块 | 当前 | v0.2 目标 |
|------|------|-----------|
| `TemplateParser` | 4 测试 | 8+ |
| `ExpressionEngine` | 8 测试 | 15+（含嵌套 / 类型推断） |
| `BarcodeGenerator` | 9 测试 | 10+ |
| `SubReportResolver` | 7 测试 | 10+（含集成） |
| `ClosedXmlExporter` | 6 测试 | 12+（含条件格式 / 嵌套表） |
| `ReportRenderer` | 6 测试 | 12+（含空模板 / 异常输入） |
| `PdfSharpExporter` | 10 测试 | 12+（含字体 / 多页压力） |

---

## 📐 代码规范

### 命名

- 类名 / 方法名 / 属性名：`PascalCase`
- 局部变量 / 参数：`camelCase`
- 私有字段：`_camelCase`（下划线前缀）
- 常量：`PascalCase`（不用 `SCREAMING_SNAKE`）
- 接口：`I<Name>` 前缀（如 `IPdfExporter`）

### 文件组织

- 一个文件一个主类（同 namespace 下的辅助类可放同文件）
- `using` 按 `System.*` → 第三方 → 项目内 顺序
- 文件名与主类名一致

### C# 风格

- 启用 `<Nullable>enable</Nullable>`，所有引用类型显式标注可空性（`string?` / `List<int>?`）
- **不用 `record`**：项目多目标包含 `net462` / `netstandard2.0`，缺 `IsExternalInit`。用普通 `class` + 显式 getter
- **不用 `using static` 跨文件**：本类同名静态方法遮蔽 `using static` 引入；只有当 `using static` 引入的成员名不与本类成员冲突时才用
- 字符串内插优先于 `string.Format`
- async 方法返回 `Task` / `Task<T>`，**不要** `async void`（除事件处理器）

### 提交前自检

- [ ] `dotnet build ReportEngine.slnx` 0 警告 0 错误
- [ ] `dotnet test ReportEngine.slnx` 全绿
- [ ] 新代码包含单元测试
- [ ] 改动范围严格可控，不顺手扩大（"等价抽离"原则）
- [ ] 不引入新依赖（除非必要且经过讨论）

---

## 🔀 提 PR 流程

### 1. Fork & Branch

```bash
# fork 后
git clone https://github.com/<you>/report-engine.git
cd report-engine
git remote add upstream https://github.com/maqiu/report-engine.git
git checkout -b feat/<short-desc>     # 例: feat/step4a-addprop-factory
```

### 2. Commit 规范

格式：`<类型>(<范围>): <简短描述>`

| 类型 | 用途 | 示例 |
|------|------|------|
| `feat` | 新功能 | `feat(D1): add PdfSharpExporter unit tests` |
| `fix` | 修 bug | `fix(renderer): handle null DataSource gracefully` |
| `refactor` | 重构（无功能变化） | `refactor(designer): extract AddPropExpr to factory` |
| `docs` | 文档 | `docs: add v0.2 roadmap` |
| `test` | 仅测试 | `test(parser): cover nested SubReport case` |
| `chore` | 杂项 | `chore: bump PdfSharpCore to 1.3.65` |
| `perf` | 性能 | `perf(renderer): cache font resolution` |

**关键约定**：

- 每个 commit 应**自包含且可 build**——避免"WIP commit + fix commit"模式
- 重构类 PR（如 Step4.A 拆 MainWindow）建议拆为多个 commit：refactor commit → doc commit → tag
- 详见 [版本与路线图](./README.md#🗓️-版本与路线图) 与 `git log --oneline` 看历史 commit 风格

### 3. Push & PR

```bash
git push origin feat/<short-desc>
```

PR 标题遵循 Commit 规范格式。PR 描述建议包含：

- **背景 / 动机**：为什么需要这个改动
- **改动概览**：列文件 + 关键点
- **测试**：新加 / 修改了哪些测试
- **截图 / 录屏**（如适用）：WPF 设计器改动
- **关联 Issue**：关闭 / 引用哪些 issue

### 4. Code Review

- 至少 1 位 maintainer approve
- CI 全绿（Windows + Linux build + test）
- Squash merge 优先（保持 main 历史干净）

---

## 🏷️ 发布流程（maintainer）

v0.1.x 系列的发布走 `git tag -a` 流程：

```bash
# 1. 累积若干 commit 后, 在 main 上:
git tag -a v0.1.8 -m "v0.1.8: <简短描述>"
git push origin v0.1.8

# 2. 更新 README 的"当前版本"行 + 路线图状态
# 3. 必要时在 GitHub Releases 写 changelog
```

**版本号约定**：

- 0.1.x → 0.1.y：补丁（小改进 / 重构 / 新测试，**不破坏 API**）
- 0.1.x → 0.2.0：次版本（**破坏性 API 变更**或大特性）
- 1.0 之前不做 minor/major 区分

---

## 💬 沟通渠道

- **GitHub Issues**：bug 报告 / 特性请求 / 任务追踪
- **GitHub Discussions**：设计讨论 / 路线图脑暴
- **Email**：maqiuliang4@163.com（私事 / 安全报告）

---

## 📜 许可证

提交 PR 即视为同意按 [MIT License](./LICENSE) 许可你的贡献。

---

<sub>Happy hacking! 🚀</sub>
