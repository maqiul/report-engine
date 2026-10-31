// 前端渲染摘要 dump —— 与 .NET / Java 两侧用同一份模板 + 数据渲染，
// 输出同格式摘要，供 tests/cross-consistency/check.js 三端比对。
//
// 用法：
//   node tests/cross-consistency/frontend-dump.mjs
//   （需先构建库：cd report-engine-vue && npm run build）
import { writeFileSync, mkdirSync } from 'node:fs'
import { dirname, join } from 'node:path'
import { fileURLToPath, pathToFileURL } from 'node:url'

const __dirname = dirname(fileURLToPath(import.meta.url))
const libPath = join(__dirname, '..', '..', 'report-engine-vue', 'dist', 'report-engine-vue.es.js')
// Windows 上 ESM 动态 import 必须用 file:// URL
const { toSummary } = await import(pathToFileURL(libPath).href)

// ⚠️ 模板与数据必须与 .NET CrossConsistencyDumpTest / Java CrossConsistencyDumpTest 完全一致
const TEMPLATE = {
  version: '1.0',
  page: { width: 210, height: 297, margin: { top: 15, bottom: 15, left: 15, right: 15 } },
  dataSources: [{ name: 'orders', type: 'json' }],
  bands: [
    {
      type: 'reportHeader', height: 12,
      elements: [
        { type: 'text', text: 'ORDER SUMMARY', x: 0, y: 0, width: 180, height: 10, font: { size: 18, bold: true }, alignment: 'center' }
      ]
    },
    {
      type: 'detail', height: 8, dataSource: 'orders',
      elements: [
        { type: 'text', text: '{{currentRow.id}}', x: 0, y: 0, width: 30, height: 6, font: { size: 12 }, alignment: 'left' },
        { type: 'text', text: '{{currentRow.customer}}', x: 35, y: 0, width: 70, height: 6, font: { size: 12 }, alignment: 'left' },
        { type: 'text', text: '{{currentRow.status}}', x: 110, y: 0, width: 70, height: 6, font: { size: 12 }, alignment: 'left', rotation: 15 }
      ]
    }
  ]
}

const DATA = {
  orders: [
    { id: 'SO-001', customer: 'Acme Corp', status: 'PAID' },
    { id: 'SO-002', customer: 'Globex', status: 'OPEN' },
    { id: 'SO-003', customer: 'Initech', status: 'PAID' }
  ]
}

// zoom=0.5 → u = MM_TO_PX * 0.5
const summary = toSummary(TEMPLATE, DATA, 'frontend', 0.5)

// 换算回原始单位以便与 .NET/Java 的 mm/pt 坐标直接比对：
//   几何：px → mm（除以 MM_TO_PX * zoom）
//   字号：px → pt（除以 zoom，因为 fontSize 已经是缩放后的 px）
const ZOOM = 0.5
const PX_PER_MM = 3.7795 * ZOOM
for (const e of summary.elements) {
  e.x = Math.round((e.x / PX_PER_MM) * 100) / 100
  e.w = Math.round((e.w / PX_PER_MM) * 100) / 100
  e.size = Math.round((e.size / ZOOM) * 100) / 100
}

const out = join(__dirname, 'artifacts', 'frontend-summary.json')
mkdirSync(dirname(out), { recursive: true })
writeFileSync(out, JSON.stringify(summary, null, 2))
console.log('[frontend] summary written:', out)
console.log('[frontend] elements:', summary.elements.length)
