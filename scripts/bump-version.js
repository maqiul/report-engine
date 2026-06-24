#!/usr/bin/env node
// 一键 bump ReportEngine 三件套版本号
// 用法: node scripts/bump-version.js 0.6.0
// 然后: git add . && git commit -m "chore: bump to 0.6.0" && git tag v0.6.0 && git push origin main --tags

const fs = require('fs');
const path = require('path');

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
    // 先匹配整段（含 closing quote），再字面量替换
    pattern: /"version":\s*"[^"]+"/,
    replacement: `"version": "${version}"`
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
  const match = content.match(t.pattern);
  if (!match) {
    console.log(`[SKIP] ${t.name} (pattern not found)`);
    skippedCount++;
    continue;
  }

  const newContent = content.replace(t.pattern, t.replacement);
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
