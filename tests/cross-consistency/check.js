#!/usr/bin/env node
/*
 * 跨端一致性比对 —— .NET / Java / 前端 三端渲染结果对比。
 *
 * 参与者（存在即参与比对）：
 *   artifacts/dotnet-summary.json     .NET ReportEngine.Core
 *   artifacts/java-summary.json       java-lib
 *   artifacts/frontend-summary.json   前端 renderCore（report-engine-vue）
 *
 * 强一致维度：pageCount / 文本多重集 / 类型计数 / 每文本元素的 x·w·size·align
 * 不比对：y 坐标（.NET 多页分页 vs Java/前端 单页流式）、数字格式化、表达式语法超集
 *
 * 用法：node tests/cross-consistency/check.js
 */
const fs = require('fs')
const path = require('path')

const ART = path.join(__dirname, 'artifacts')
const EPS = 1e-6
const SOURCES = [
  { file: 'dotnet-summary.json', label: '.NET' },
  { file: 'java-summary.json', label: 'Java' },
  { file: 'frontend-summary.json', label: '前端' }
]

const loaded = SOURCES
  .map(s => ({ ...s, data: fs.existsSync(path.join(ART, s.file)) ? JSON.parse(fs.readFileSync(path.join(ART, s.file), 'utf8')) : null }))
  .filter(s => s.data)

if (loaded.length < 2) {
  console.error('[FAIL] 可用摘要不足 2 份，无法比对。请先运行 dump（见 README.md）。')
  process.exit(1)
}

function numEq(a, b) {
  return Math.abs((a || 0) - (b || 0)) <= EPS
}

function compare(a, b) {
  const failures = []

  if (a.data.pageCount !== b.data.pageCount) {
    failures.push(`pageCount 不一致: ${a.label}=${a.data.pageCount} ${b.label}=${b.data.pageCount}`)
  }
  if (a.data.elements.length !== b.data.elements.length) {
    failures.push(`元素总数不一致: ${a.label}=${a.data.elements.length} ${b.label}=${b.data.elements.length}`)
  }

  // 文本多重集
  const ta = a.data.elements.map(e => e.text).sort()
  const tb = b.data.elements.map(e => e.text).sort()
  if (JSON.stringify(ta) !== JSON.stringify(tb)) {
    const onlyA = ta.filter((t, i) => t !== tb[i])
    const onlyB = tb.filter((t, i) => t !== ta[i])
    failures.push(`文本多重集不一致:\n    仅 ${a.label}: ${JSON.stringify(onlyA)}\n    仅 ${b.label}: ${JSON.stringify(onlyB)}`)
  }

  // 类型计数
  const count = s => {
    const m = {}
    for (const e of s.elements) m[e.type] = (m[e.type] || 0) + 1
    return m
  }
  const ca = count(a.data), cb = count(b.data)
  if (JSON.stringify(ca) !== JSON.stringify(cb)) {
    failures.push(`类型计数不一致: ${a.label}=${JSON.stringify(ca)} ${b.label}=${JSON.stringify(cb)}`)
  }

  // 逐元素属性（按 text + x 配对）
  const indexBy = s => {
    const m = {}
    for (const e of s.elements) {
      const k = `${e.text}\u0000${e.x}`
      ;(m[k] = m[k] || []).push(e)
    }
    return m
  }
  const ia = indexBy(a.data), ib = indexBy(b.data)
  const keys = new Set([...Object.keys(ia), ...Object.keys(ib)])
  for (const k of keys) {
    const xa = ia[k] || [], xb = ib[k] || []
    if (xa.length !== xb.length) {
      failures.push(`键 [text=${JSON.stringify(k.split('\u0000')[0])}] 出现次数不一致: ${a.label}=${xa.length} ${b.label}=${xb.length}`)
      continue
    }
    for (let i = 0; i < xa.length; i++) {
      const ea = xa[i], eb = xb[i]
      if (ea.type !== eb.type) failures.push(`[${ea.text}] type 不一致: ${a.label}=${ea.type} ${b.label}=${eb.type}`)
      if (!numEq(ea.w, eb.w)) failures.push(`[${ea.text}] 宽度 w 不一致: ${a.label}=${ea.w} ${b.label}=${eb.w}`)
      if (!numEq(ea.size, eb.size)) failures.push(`[${ea.text}] 字号 size 不一致: ${a.label}=${ea.size} ${b.label}=${eb.size}`)
      if ((ea.align || '') !== (eb.align || '')) failures.push(`[${ea.text}] 对齐 align 不一致: ${a.label}=${ea.align} ${b.label}=${eb.align}`)
    }
  }

  return failures
}

console.log('=== Cross-Platform Consistency Check (.NET / Java / 前端) ===')
for (const s of loaded) {
  console.log(`${s.label.padEnd(6)}: ${s.data.elements.length} elements, ${s.data.pageCount} page(s)`)
}
console.log('')

let total = 0
for (let i = 0; i < loaded.length; i++) {
  for (let j = i + 1; j < loaded.length; j++) {
    const failures = compare(loaded[i], loaded[j])
    const pair = `${loaded[i].label} vs ${loaded[j].label}`
    if (failures.length === 0) {
      console.log(`[PASS] ${pair} — 文本 + 横向布局 + 字号 + 对齐一致`)
    } else {
      total += failures.length
      console.log(`[FAIL] ${pair} — ${failures.length} 处不一致：`)
      for (const f of failures) console.log('   - ' + f)
    }
  }
}

console.log('')
if (total === 0) {
  console.log('已知未比对项（设计上可接受）：')
  console.log('  - y 坐标：.NET 多页分页 vs Java/前端 单页流式，纵向算法不同')
  console.log('  - 数字字段格式化：.NET FormatValue vs Java toString()')
  console.log('  - 表达式语法交集：测试用 {{currentRow.x}}（两端通用）；.NET 另支持 {{ds.field}}/{{field}}')
  process.exit(0)
} else {
  console.log(`共 ${total} 处不一致。`)
  process.exit(1)
}
