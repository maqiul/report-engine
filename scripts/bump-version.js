#!/usr/bin/env node
// 一键 bump ReportEngine 三件套版本号
// 用法: node scripts/bump-version.js 0.6.0
// 然后: git add . && git commit -m "chore: bump to 0.6.0" && git tag v0.6.0 && git push origin main --tags

const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

const version = process.argv[2];
if (!version) {
  console.error('Usage: node scripts/bump-version.js <version>');
  console.error('Example: node scripts/bump-version.js 0.6.0');
  process.exit(1);
}

const repoRoot = path.resolve(__dirname, '..');

const targets = [
  {
    name: '.NET: Directory.Build.props',
    file: path.join(repoRoot, 'Directory.Build.props'),
    pattern: /<Version>[^<]+<\/Version>/,
    replacement: `<Version>${version}</Version>`
  },
  {
    name: 'Java: java-lib/build.gradle',
    file: path.join(repoRoot, 'java-lib', 'build.gradle'),
    pattern: /version\s*=\s*'[^']+'/,
    replacement: `version = '${version}'`
  },
  {
    name: 'Java: report-engine-starter/build.gradle',
    file: path.join(repoRoot, 'report-engine-starter', 'build.gradle'),
    pattern: /version\s*=\s*'[^']+'/,
    replacement: `version = '${version}'`
  },
  {
    name: 'npm: report-engine-vue/package.json',
    file: path.join(repoRoot, 'report-engine-vue', 'package.json'),
    // 用 npm version 同步 package.json + package-lock.json（只改根包版本，格式规范）
    npmVersion: true
  },
  {
    name: 'npm: web/frontend/package.json',
    file: path.join(repoRoot, 'web', 'frontend', 'package.json'),
    npmVersion: true
  },
  // ===== 依赖坐标里的版本（保持与被依赖模块一致，否则 CI 解析到旧版本）=====
  {
    name: 'Java: web/backend 依赖 java-lib',
    file: path.join(repoRoot, 'web', 'backend', 'build.gradle'),
    pattern: /(com\.reportengine:java-lib):[0-9][^'"]*/,
    replacement: `$1:${version}`
  },
  {
    name: 'Java: starter 依赖 java-lib',
    file: path.join(repoRoot, 'report-engine-starter', 'build.gradle'),
    pattern: /(com\.reportengine:java-lib):[0-9][^'"]*/,
    replacement: `$1:${version}`
  },
  {
    name: 'Java: demo-backend 依赖 starter',
    file: path.join(repoRoot, 'report-engine-demo-backend', 'build.gradle'),
    pattern: /(com\.reportengine:report-engine-spring-boot-starter):[0-9][^'"]*/,
    replacement: `$1:${version}`
  },
  {
    name: 'Java: starter-sample 依赖 starter',
    file: path.join(repoRoot, 'report-engine-starter-sample', 'build.gradle'),
    pattern: /(com\.reportengine:report-engine-spring-boot-starter):[0-9][^'"]*/,
    replacement: `$1:${version}`
  }
];

console.log(`=== Bumping version to ${version} ===\n`);

let updatedCount = 0;
let skippedCount = 0;

for (const t of targets) {
  if (!fs.existsSync(t.file)) {
    console.log(`[SKIP] ${t.name} (file not found)`);
    skippedCount++;
    continue;
  }

  const content = fs.readFileSync(t.file, 'utf8');
  const match = content.match(t.pattern || /"version":\s*"[^"]+"/);

  // npm 项目：交给 npm version 处理（会同步 package-lock.json 的根包版本）
  if (t.npmVersion) {
    try {
      execSync(`npm version ${version} --no-git-tag-version --allow-same-version`, {
        cwd: path.dirname(t.file),
        stdio: 'pipe'
      });
      console.log(`[OK]   ${t.name}  →  npm version ${version}`);
      updatedCount++;
    } catch (e) {
      console.log(`[SKIP] ${t.name} (npm version failed: ${e.message.split('\n')[0]})`);
      skippedCount++;
    }
    continue;
  }

  if (!match) {
    console.log(`[SKIP] ${t.name} (pattern not found)`);
    skippedCount++;
    continue;
  }

  // all=true 时替换所有匹配（package-lock.json 顶层 + 根包两处）
  const newContent = t.all
    ? content.replace(new RegExp(t.pattern.source, 'g'), t.replacement)
    : content.replace(t.pattern, t.replacement);
  fs.writeFileSync(t.file, newContent, 'utf8');
  console.log(`[OK]   ${t.name}  →  ${match[0]}  =>  ${t.replacement}`);
  updatedCount++;
}

console.log(`\n=== Done: ${updatedCount} updated, ${skippedCount} skipped ===\n`);

console.log('Next steps:');
console.log('  git add .');
console.log(`  git commit -m "chore: bump version to ${version}"`);
console.log(`  git tag v${version}`);
console.log('  git push origin main --tags');
console.log('');
console.log('This will trigger 3 publish jobs:');
console.log('  - ci.yml => NuGet (needs NUGET_API_KEY secret)');
console.log('  - java-ci.yml => Maven Central (needs 4 secrets + 1-3 day review)');
console.log('  - frontend-ci.yml => npm public (needs NPM_TOKEN secret)');
console.log('');
console.log('See docs/RELEASE.md for secrets setup.');
