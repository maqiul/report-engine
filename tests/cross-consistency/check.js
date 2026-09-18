#!/usr/bin/env node
/*
 * 跨端一致性比对 — 读取 .NET / Java 两侧 dump 出的规范化渲染摘要，逐项比对。
 *
 * 用法（先在两侧生成摘要，见 run.cmd / CI）：
 *   node tests/cross-consistency/check.js
 *
 * 比对维度（强一致）：
 *   - pageCount
 *   - 文本元素多重集（内容完全相同）
 *   - 元素类型计数
 *   - 每个文本元素的 x / w / size / align（数值容差 1e-6）
 *
 * 不比对（已知跨端差异，见下方打印说明）：
 *   - y 坐标：.NET 多页分页 vs Java 单页流式，纵向布局算法不同
 *   - 数字字段格式化：.NET FormatValue vs Java toString()
 */
const fs = require('fs');
const path = require('path');

const ART = path.join(__dirname, 'artifacts');
const dotnet = JSON.parse(fs.readFileSync(path.join(ART, 'dotnet-summary.json'), 'utf8'));
const java = JSON.parse(fs.readFileSync(path.join(ART, 'java-summary.json'), 'utf8'));

const EPS = 1e-6;
const failures = [];

function numEq(a, b) {
  return Math.abs((a || 0) - (b || 0)) <= EPS;
}

// 1) pageCount
if (dotnet.pageCount !== java.pageCount) {
  failures.push(`pageCount 不一致: .NET=${dotnet.pageCount} Java=${java.pageCount}`);
}

// 2) 元素个数
if (dotnet.elements.length !== java.elements.length) {
  failures.push(`元素总数不一致: .NET=${dotnet.elements.length} Java=${java.elements.length}`);
}

// 3) 文本多重集
function texts(s) {
  return s.elements.map(e => e.text).sort();
}
const dt = texts(dotnet);
const jt = texts(java);
if (JSON.stringify(dt) !== JSON.stringify(jt)) {
  const onlyD = dt.filter((t, i) => t !== jt[i]);
  const onlyJ = jt.filter((t, i) => t !== dt[i]);
  failures.push(`文本多重集不一致:\n    仅 .NET: ${JSON.stringify(onlyD)}\n    仅 Java: ${JSON.stringify(onlyJ)}`);
}

// 4) 类型计数
function typeCounts(s) {
  const m = {};
  for (const e of s.elements) m[e.type] = (m[e.type] || 0) + 1;
  return m;
}
const tcD = typeCounts(dotnet);
const tcJ = typeCounts(java);
if (JSON.stringify(tcD) !== JSON.stringify(tcJ)) {
  failures.push(`类型计数不一致: .NET=${JSON.stringify(tcD)} Java=${JSON.stringify(tcJ)}`);
}

// 5) 逐元素属性比对（按 text + x 配对；同 text 多实例靠出现次序配对）
function key(e) {
  return `${e.text}\u0000${e.x}`;
}
function indexBy(s) {
  const m = {};
  for (const e of s.elements) {
    const k = key(e);
    (m[k] = m[k] || []).push(e);
  }
  return m;
}
const idxD = indexBy(dotnet);
const idxJ = indexBy(java);
const allKeys = new Set([...Object.keys(idxD), ...Object.keys(idxJ)]);
for (const k of allKeys) {
  const a = idxD[k] || [];
  const b = idxJ[k] || [];
  if (a.length !== b.length) {
    failures.push(`键 [text=${JSON.stringify(k.split('\u0000')[0])}] 出现次数不一致: .NET=${a.length} Java=${b.length}`);
    continue;
  }
  for (let i = 0; i < a.length; i++) {
    const ea = a[i], eb = b[i];
    if (ea.type !== eb.type) failures.push(`[${ea.text}] type 不一致: .NET=${ea.type} Java=${eb.type}`);
    if (!numEq(ea.w, eb.w)) failures.push(`[${ea.text}] 宽度 w 不一致: .NET=${ea.w} Java=${eb.w}`);
    if (!numEq(ea.size, eb.size)) failures.push(`[${ea.text}] 字号 size 不一致: .NET=${ea.size} Java=${eb.size}`);
    if ((ea.align || '') !== (eb.align || '')) failures.push(`[${ea.text}] 对齐 align 不一致: .NET=${ea.align} Java=${eb.align}`);
  }
}

// ---- 报告 ----
console.log('=== Cross-Platform Consistency Check (.NET vs Java) ===');
console.log(`.NET : ${dotnet.engine}, ${dotnet.elements.length} elements, ${dotnet.pageCount} page(s)`);
console.log(`Java : ${java.engine}, ${java.elements.length} elements, ${java.pageCount} page(s)`);
console.log('');

if (failures.length === 0) {
  console.log('[PASS] 同一份 .rptx 在 .NET 与 Java 渲染结果一致（文本 + 横向布局 + 字号 + 对齐）。');
  console.log('');
  console.log('已知未比对项（设计上可接受）：');
  console.log('  - y 坐标：.NET 多页分页 vs Java 单页流式，纵向算法不同');
  console.log('  - 数字字段格式化：.NET FormatValue vs Java toString()');
  console.log('  - 表达式语法交集：测试用 {{currentRow.x}}（两端通用）；.NET 另支持 {{ds.field}}/{{field}}，Java 不支持');
  process.exit(0);
} else {
  console.log(`[FAIL] 发现 ${failures.length} 处不一致：`);
  for (const f of failures) console.log('  - ' + f);
  process.exit(1);
}
