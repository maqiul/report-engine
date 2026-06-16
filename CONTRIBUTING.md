# 贡献指南

欢迎参与 ReportEngine 的开发！本文档说明开发环境、代码规范、提 PR 流程、测试要求等。

---

## 🛠️ 开发环境

### 必需

- **.NET SDK 8.0** 或更高
- **Git** 2.30+
- **Windows 10/11**（WPF / WinForms 设计器编译目标）；Core/Tests 在 Linux/macOS 上也能 build & test
- IDE：Visual Studio 2022 / JetBrains Rider / VS Code + C# Dev Kit

### 验证安装

```bash
dotnet --version   # 应 >= 8.0
git --version      # 应 >= 2.30
```

---

## 📁 项目结构

```
report-engine/
├── ReportEngine.slnx                       # 解决方案
├── Directory.Build.props                   # 统一构建属性（版本号/NuGet元数据）
├── README.md / CONTRIBUTING.md / LICENSE   # 文档三件套
├── .editorconfig                           # 代码风格规范
├── ReportEngine.Core/                      # 跨平台核心（net462 + netstandard2.0 + net8.0）
│   ├── Barcodes/                           # ZXing 条码生成
│   ├── Data/                               # 表达式引擎
│   ├── Export/                             # IPdfExporter / IExcelExporter 接口
│   ├── Parsing/                            # 模板 (.rptx JSON) 解析
│   ├── Rendering/                          # 渲染引擎
│   ├── SubReports/                         # 子报表解析
│   └── TemplateSchema.cs                   # 领域模型定义
├── ReportEngine.Export.Excel/              # ClosedXML 导出（多目标）
├── ReportEngine.Export.Pdf/                # PdfSharpCore 导出（多目标）
├── ReportEngine.Viewer.Wpf / WinForms/     # 运行时预览
├── ReportEngine.Designer.Wpf / WinForms/   # 可视化设计器
├── ReportEngine.Desktop/                   # 桌面启动器
└── tests/
    ├── ReportEngine.Core.Tests/            # Core 单元测试（4575 条）
    ├── ReportEngine.Export.Pdf.Tests/      # PDF 导出测试（67 条，含端到端）
    └── ReportEngine.Export.Excel.Tests/    # Excel 导出测试（51 条，含端到端）
```

---

## 🔨 本地构建

```bash
# 还原 + 编译整个解决方案
dotnet build ReportEngine.slnx -c Debug

# 编译单个项目
dotnet build ReportEngine.Core/ReportEngine.Core.csproj

# 仅跑测试
dotnet test ReportEngine.slnx
```

**目标框架矩阵**：

| 项目 | 目标框架 |
|------|----------|
| `ReportEngine.Core` | `net462` + `netstandard2.0` + `net8.0` |
| `ReportEngine.Export.Excel` | `net462` + `netstandard2.0` + `net8.0` |
| `ReportEngine.Export.Pdf` | `net462` + `netstandard2.0` + `net8.0` |
| `ReportEngine.Viewer.*` / `Designer.*` | `net462` + `net8.0-windows` |
| `ReportEngine.Desktop` | `net8.0` |
| `tests/*` | `net8.0` |

**所有改动必须编译 0 警告 0 错误**。

---

## 🧪 测试要求

### 当前状态

**4693 个测试，全部通过。**

| 项目 | 测试数 | 覆盖范围 |
|------|--------|----------|
| `ReportEngine.Core.Tests` | 4575 | 模板解析、表达式引擎、条码生成、子报表、渲染路径、ClosedXML 导出 |
| `ReportEngine.Export.Pdf.Tests` | 67 | PdfSharpExporter + 17 个端到端集成测试 |
| `ReportEngine.Export.Excel.Tests` | 51 | ClosedXmlExporter + 16 个端到端集成测试 |

### 写新测试

- 所有新功能 / 修 bug 必须附单元测试
- 测试项目**只锁 `net8.0`**，不引入多目标
- 框架：**xunit 2.9.0** + **FluentAssertions 6.12.0**
- 命名规范：描述性句子或 `<MethodName>_<Condition>_<Expected>`
- 端到端测试：验证 模板解析 → 渲染 → 导出 完整流程

### 跑测试

```bash
# 全测试
dotnet test ReportEngine.slnx

# 单个测试类
dotnet test --filter "FullyQualifiedName~PdfSharpExporterTests"

# 单个测试方法
dotnet test --filter "Name=EndToEnd_SimpleTemplate_PdfGenerated"
```

**PR 提交前必须保持全绿。**

---

## 📐 代码规范

### 命名

- 类名 / 方法名 / 属性名：`PascalCase`
- 局部变量 / 参数：`camelCase`
- 私有字段：`_camelCase`（下划线前缀）
- 常量：`PascalCase`
- 接口：`I<Name>` 前缀（如 `IPdfExporter`）

### 文件组织

- 一个文件一个主类（同 namespace 下的辅助类可放同文件）
- `using` 按 `System.*` → 第三方 → 项目内 顺序
- 文件名与主类名一致

### C# 风格

- 启用 `<Nullable>enable</Nullable>`，所有引用类型显式标注可空性
- **不用 `record`**：项目多目标包含 `net462` / `netstandard2.0`，缺 `IsExternalInit`，用普通 `class`
- 字符串内插优先于 `string.Format`
- async 方法返回 `Task` / `Task<T>`，**不要** `async void`（除事件处理器）
- 测试方法用 `async Task` + `await`，不要用 `.GetAwaiter().GetResult()`

### 提交前自检

- [ ] `dotnet build ReportEngine.slnx` 0 警告 0 错误
- [ ] `dotnet test ReportEngine.slnx` 全绿
- [ ] 新代码包含单元测试
- [ ] 改动范围严格可控，不顺手扩大

---

## 🔀 提 PR 流程

### 1. Fork & Branch

```bash
git clone https://github.com/<you>/report-engine.git
cd report-engine
git remote add upstream https://github.com/maqiul/report-engine.git
git checkout -b feat/<short-desc>
```

### 2. Commit 规范

格式：`<类型>(<范围>): <简短描述>`

| 类型 | 用途 | 示例 |
|------|------|------|
| `feat` | 新功能 | `feat(export): add barcode support in PDF` |
| `fix` | 修 bug | `fix(renderer): handle null DataSource` |
| `refactor` | 重构 | `refactor(designer): extract MainWindow partials` |
| `docs` | 文档 | `docs: update README with new test count` |
| `test` | 测试 | `test(e2e): add subreport integration tests` |
| `chore` | 杂项 | `chore: bump ClosedXML to 0.95.5` |
| `perf` | 性能 | `perf(renderer): cache font resolution` |

### 3. Push & PR

```bash
git push origin feat/<short-desc>
```

PR 描述建议包含：
- **背景 / 动机**
- **改动概览**
- **测试**：新加 / 修改了哪些测试
- **关联 Issue**

### 4. Code Review

- 至少 1 位 maintainer approve
- CI 全绿（Windows + Linux build + test）
- Squash merge 优先

---

## 🏷️ 发布流程

### 版本号管理

版本号统一在 `Directory.Build.props` 中定义，修改一处全局生效：

```xml
<Version>0.2.0-alpha</Version>
```

### 发布步骤

```bash
# 1. 更新 Directory.Build.props 中的版本号
# 2. 提交并推送
git add -A
git commit -m "v0.2.0: 简短描述"
git push origin main

# 3. 打 tag
git tag v0.2.0
git push origin v0.2.0
```

### CI 自动发布

推送 `v*` 标签后，GitHub Actions 自动：
1. Build & Test (Win + Linux)
2. Pack NuGet 包
3. 发布到 NuGet.org（需配置 `NUGET_API_KEY` secret）

### 版本号约定

- `0.x.y` → `0.x.z`：补丁（小改进 / 重构 / 新测试，不破坏 API）
- `0.x.y` → `0.y.0`：次版本（新特性，可能破坏 API）
- `1.0.0`：首个稳定版本

---

## 💬 沟通渠道

- **GitHub Issues**：bug 报告 / 特性请求
- **GitHub Discussions**：设计讨论
- **Email**：maqiuliang4@163.com（私事 / 安全报告）

---

## 📜 许可证

提交 PR 即视为同意按 [MIT License](./LICENSE) 许可你的贡献。

---

<sub>Happy hacking! 🚀</sub>
