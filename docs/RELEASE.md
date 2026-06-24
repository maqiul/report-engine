# 公网 Registry 发布指南

> ReportEngine 三件套发布到 **Maven Central / npm public / NuGet public** 的完整步骤。
> 全程约 30-60 分钟（不含 Maven Central 审核 1-3 天）。

---

## 概览

| 包 | Registry | 难度 | 耗时 |
|----|----------|------|------|
| `ReportEngine.Core/.Export.Pdf/.Export.Excel` | **NuGet** | ⭐ | 10 分钟 |
| `com.reportengine:java-lib` + `:report-engine-spring-boot-starter` | **Maven Central** | ⭐⭐⭐ | 1-3 天（含审核） |
| `@reportengine/vue` | **npm public** | ⭐ | 5 分钟 |

3 个 workflow 已经全部就位（`.github/workflows/`），**只需要配 GitHub Secrets + 推 tag**。

---

## 1. NuGet（最简单，先做）

### 1.1 注册账号
1. 打开 https://www.nuget.org/users/account/LogOn
2. 用 Microsoft 账号注册
3. 邮箱验证

### 1.2 创建 API Key
1. 登录后访问 https://www.nuget.org/account/apikeys
2. **Create** → 填 Name `report-engine-bot` → **Package** 权限选 `Push only new package versions and package list` → Glob Pattern 填 `ReportEngine.*` → **Create**
3. **复制 API Key**（只显示一次！）

### 1.3 配 GitHub Secret
1. 打开 https://github.com/maqiul/report-engine/settings/secrets/actions/new
2. Name: `NUGET_API_KEY`
3. Value: 粘贴刚才的 API Key
4. **Add secret**

### 1.4 推 tag 触发发布
```bash
cd D:\report-engine
git tag v0.2.1-web   # 任意新版本
git push origin v0.2.1-web
```

**触发条件**：`ci.yml` 的 `push-nuget` job 在 `github.event_name == 'push' && github.ref == 'refs/heads/main'` 时跑（**注意：当前配置是 main 分支，不是 tag**）。要 tag 触发，把 `if` 改成：
```yaml
if: github.event_name == 'push' && startsWith(github.ref, 'refs/tags/v')
```

### 1.5 验证
访问 https://www.nuget.org/packages/ReportEngine.Core/ 看是否出现新版本。

---

## 2. npm public（简单）

### 2.1 注册账号
1. 打开 https://www.npmjs.com/signup
2. 邮箱 + 用户名 + 密码
3. **必须开启 2FA**（发布 token 必须 2FA）

### 2.2 创建 Publish Token
1. 登录后访问 https://www.npmjs.com/settings/maqiul/tokens/（替换用户名）
2. **Generate New Token** → **Classic Token** → 选 **Automation**（Automation 不需要 2FA 二次验证，CI/CD 用）
3. 复制 token（格式 `npm_xxxxxxxxxx`）

### 2.3 配 GitHub Secret
1. https://github.com/maqiul/report-engine/settings/secrets/actions/new
2. Name: `NPM_TOKEN`
3. Value: 粘贴 token
4. **Add secret**

### 2.4 推 tag 触发发布
```bash
cd D:\report-engine
git tag v0.3.1-vue    # @reportengine/vue 任意新版本
git push origin v0.3.1-vue
```

**触发条件**：`frontend-ci.yml` 的 `publish-npm` job 在 `startsWith(github.ref, 'refs/tags/v')` 时跑。**注意：当前 job 名是 `publish-npm`，确认触发了**。

### 2.5 验证
访问 https://www.npmjs.com/package/@reportengine/vue 看是否出现新版本。

---

## 3. Maven Central（最难，要审核）

### 3.1 注册 Sonatype OSSRH 账号
1. 打开 https://issues.sonatype.org/secure/Signup!default.jspa
2. 填用户名 + 邮箱 + 名字（**全英文**）
3. 邮箱验证
4. 登录后访问 https://issues.sonatype.org/secure/Dashboard.jspa
5. **Create Issue** → Project: **Community Support - Open Source Project Repository Hosting (OSSRH)** → Issue Type: **New Project**
6. 填：
   - **Summary**: `Create namespace com.reportengine for report-engine`
   - **Group Id**: `com.reportengine`
   - **Project URL**: `https://github.com/maqiul/report-engine`
   - **SCM URL**: `https://github.com/maqiul/report-engine.git`
7. 提交后**等待 1-3 天审核**，会邮件通知通过/补充材料
8. 通过后 `com.reportengine` namespace 就归你了

### 3.2 生成 GPG Key（签名用）
```bash
# 安装 gpg
# Windows: 下载 https://www.gpg4win.org/

gpg --gen-key
# 选 RSA and RSA, 4096 bits
# 真实姓名 + 邮箱 + 注释（可空）
# 密码：记下来（= GPG_SIGNING_PASSWORD）

# 列出私钥
gpg --list-secret-keys
# 找 sec 行的 40 位指纹，如：ABC123DEF456...

# 导出私钥（armored）
gpg --armor --export-secret-keys ABC123DEF456... > gpg-private.key
# cat gpg-private.key → 全选复制（包括 BEGIN/END 行）
```

### 3.3 配 GitHub Secrets（4 个）
访问 https://github.com/maqiul/report-engine/settings/secrets/actions/new 创建 4 个：

| Name | Value |
|------|-------|
| `MAVEN_CENTRAL_USERNAME` | Sonatype 账号用户名 |
| `MAVEN_CENTRAL_TOKEN` | Sonatype 账号密码 |
| `GPG_SIGNING_KEY` | 3.2 导出的 armored 私钥全部内容 |
| `GPG_SIGNING_PASSWORD` | 3.2 设的 GPG 密码 |

### 3.4 推 tag 触发发布
```bash
cd D:\report-engine
git tag v0.3.1-starter    # java-lib 0.3.1 + starter 0.3.1
git push origin v0.3.1-starter
```

**触发条件**：`java-ci.yml` 的 `publish-maven` job 在 `startsWith(github.ref, 'refs/tags/v')` 时跑。

### 3.5 验证
1. 访问 https://central.sonatype.com/artifact/com.reportengine/java-lib 看版本列表
2. **首次发布**要在 https://central.sonatype.com/publishing/deployments 手动点 **Publish**（之后自动）

---

## 4. 一键检查清单

secrets 配齐后，**任何 tag push 都会触发 3 个 publish job**。

```bash
# 配完 secrets 后验证（不需要 push 真 tag）
gh workflow list    # 看 3 个 workflow
gh workflow run java-ci.yml
gh workflow run frontend-ci.yml
gh workflow run ci.yml
```

**手动触发 publish job**：在 GitHub Actions 页面选 workflow → **Run workflow** → 选 tag → **Run**。

---

## 5. 后续维护

### 5.1 版本号策略
- **Patch** (0.2.0 → 0.2.1): bug fix
- **Minor** (0.2.0 → 0.3.0): 新功能
- **Major** (0.2.0 → 1.0.0): 破坏性变更
- 三件套**版本号独立**：
  - `ReportEngine.Core` → `Directory.Build.props` 改 `<Version>`
  - `java-lib` → `java-lib/build.gradle` 改 `version`
  - `report-engine-starter` → `report-engine-starter/build.gradle` 改 `version`
  - `@reportengine/vue` → `report-engine-vue/package.json` 改 `version`

### 5.2 撤回错误发布
- **npm**: `npm unpublish @reportengine/vue@0.3.1 --force`（24 小时内）
- **NuGet**: 登录后 https://www.nuget.org/packages/manage/ → **Unlist**
- **Maven Central**: 只能 **Unpublish**（不是真删，只是隐藏）

### 5.3 密钥轮换
每年至少 1 次：
- **NPM_TOKEN**: https://www.npmjs.com/settings/.../tokens → 删旧生成新
- **NUGET_API_KEY**: https://www.nuget.org/account/apikeys → Regenerate
- **GPG_KEY**: 重新 `gpg --gen-key`
- **Maven**: 不需要轮换，账号密码自己改

---

## 6. 常见问题

### Q1: Maven Central 审核被拒
**A**: 检查 namespace 是否和你邮箱域名一致。`com.reportengine` 不属于你 → 要么改成你拥有的域名（反写），要么用 `io.github.<username>`（GitHub 验证 1 分钟）。

### Q2: `npm publish` 报 401
**A**: `NPM_TOKEN` 没配对。注意要选 **Automation** 类型的 token，**不要 Legacy password**。

### Q3: GPG 签名报 `gpg: signing failed: Inappropriate ioctl for device`
**A**: 在 workflow 里加 `export GPG_TTY=$(tty)` 或 `--pinentry-mode loopback --passphrase $GPG_SIGNING_PASSWORD`。

### Q4: NuGet push 报 403
**A**: API Key 没 `Push` 权限。重新创建一个勾选 `Push` 权限的。

### Q5: workflow 触发但 publish job 跳过
**A**: 看 job 的 `if` 条件。当前配置：
- `ci.yml` (NuGet): `if: github.event_name == 'push' && github.ref == 'refs/heads/main'` → **只 main 分支 push 触发**
- `java-ci.yml` (Maven): `if: startsWith(github.ref, 'refs/tags/v')` → **只 tag push 触发**
- `frontend-ci.yml` (npm): `if: startsWith(github.ref, 'refs/tags/v')` → **只 tag push 触发**

要 tag 触发 NuGet，编辑 `ci.yml` 把 `if` 改 tag 条件。
