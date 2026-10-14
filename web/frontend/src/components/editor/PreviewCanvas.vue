<template>
  <div class="preview-scroll">
    <div class="page" :style="{ width: pageW + 'px', minHeight: pageH + 'px' }">
      <div
        v-for="(band, bi) in layout"
        :key="bi"
        class="band"
        :style="{ top: band.top + 'px', height: band.height + 'px' }"
      >
        <div
          v-for="(el, ei) in band.els"
          :key="ei"
          class="el"
          :style="{ left: el.left + 'px', top: el.top + 'px', width: el.width + 'px', height: el.height + 'px' }"
        >
          <span v-if="el.kind === 'text'" :style="el.textStyle">{{ el.content }}</span>
          <div v-else-if="el.kind === 'line'" :style="el.lineStyle"></div>
          <div v-else-if="el.kind === 'shape'" :style="el.shapeStyle"></div>
          <img v-else-if="el.kind === 'image' && el.content" :src="el.content" :style="el.imgStyle" />
          <div v-else-if="el.kind === 'image'" class="ph">🖼 图像</div>
          <div v-else-if="el.kind === 'barcode'" class="ph">{{ el.content2 }} {{ el.content }}</div>
          <div v-else-if="el.kind === 'table'" class="ph">▦ 表格</div>
          <div v-else class="ph">{{ el.kind }}</div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
// 纯前端预览：不依赖后端。做表达式求值 + 数据绑定 + detail 行展开 + band 堆叠 + 元素定位。
// 与后端 PDF 渲染不追求 100% 一致，用于编辑态所见即所得查看布局与数据。
import { computed } from 'vue'

const props = defineProps<{
  template: any
  data?: Record<string, any[]>
  scale?: number
}>()

const MM = 3.7795
const s = () => Number(props.scale || 1)
const pageW = computed(() => Number(props.template?.page?.width || 210) * MM * s())
const pageH = computed(() => Number(props.template?.page?.height || 297) * MM * s())

function evalText(text: string, row: any): string {
  if (!text) return ''
  return text.replace(/\{\{([^}]+)\}\}/g, (_m, expr) => {
    expr = String(expr).trim()
    const sys: Record<string, string> = {
      PageNumber: '1', TOTAL_PAGES: '1', PrintDate: new Date().toISOString().slice(0, 10),
      PrintTime: new Date().toTimeString().slice(0, 5), ReportTitle: props.template?.description || ''
    }
    const sv = sys[expr]
    if (sv !== undefined) return sv
    let field = expr
    if (expr.startsWith('currentRow.')) field = expr.slice(11)
    else if (expr.includes('.')) field = expr.split('.').slice(1).join('.')
    const v = row?.[field]
    return v !== undefined && v !== null ? String(v) : ''
  })
}

function evalSummary(fn: string, field: string, ds: string): string {
  const rows = props.data?.[ds] || []
  const nums = rows.map(r => Number(r[field])).filter(n => !isNaN(n))
  if (!nums.length) return '0'
  switch (fn) {
    case 'Sum': return String(nums.reduce((a, b) => a + b, 0))
    case 'Count': return String(rows.length)
    case 'Avg': return String(Math.round((nums.reduce((a, b) => a + b, 0) / nums.length) * 100) / 100)
    case 'Max': return String(Math.max(...nums))
    case 'Min': return String(Math.min(...nums))
    default: return ''
  }
}

function buildEl(el: any, row: any, ds: string, bandTop: number): any {
  const sc = s()
  const box = {
    left: Number(el.x || 0) * MM * sc,
    top: bandTop + Number(el.y || 0) * MM * sc,
    width: Number(el.width || 10) * MM * sc,
    height: Number(el.height || 5) * MM * sc
  }
  const f = el.font || {}
  if (el.type === 'text') {
    let content: string
    if (el.systemVariable) content = evalText('{{' + el.systemVariable + '}}', row)
    else if (el.summaryFunction) content = evalSummary(el.summaryFunction, el.summaryField, ds)
    else if (el.dataField) content = evalText('{{currentRow.' + el.dataField + '}}', row)
    else content = evalText(el.text || '', row)
    return {
      ...box, kind: 'text', content,
      textStyle: {
        fontFamily: f.family || 'Microsoft YaHei, sans-serif',
        fontSize: (Number(f.size || 10) * sc) + 'px',
        fontWeight: f.bold ? 'bold' : 'normal',
        fontStyle: f.italic ? 'italic' : 'normal',
        textDecoration: f.underline ? 'underline' : 'none',
        color: f.color || '#000', textAlign: el.alignment || 'left',
        display: 'block', width: '100%', lineHeight: '1.2', whiteSpace: 'pre-wrap'
      }
    }
  }
  if (el.type === 'line') return { ...box, kind: 'line', lineStyle: { borderTop: (Number(el.lineWidth || 0.3) * sc) + 'px solid ' + (el.lineColor || '#000'), position: 'absolute', top: '50%', left: 0, right: 0 } }
  if (el.type === 'shape') return { ...box, kind: 'shape', shapeStyle: { background: el.fillColor || '#fff', border: '1px solid #000', borderRadius: (Number(el.borderRadius || 0) * sc) + 'px', width: '100%', height: '100%', boxSizing: 'border-box' } }
  if (el.type === 'image') return { ...box, kind: 'image', content: el.source || '', imgStyle: { width: '100%', height: '100%', objectFit: 'contain' } }
  if (el.type === 'barcode') return { ...box, kind: 'barcode', content: evalText(el.value || '', row), content2: el.format === 'QRCode' ? '▦' : '▮▮' }
  if (el.type === 'table') return { ...box, kind: 'table' }
  return { ...box, kind: el.type || 'other' }
}

const layout = computed(() => {
  const bands = props.template?.bands || []
  const data = props.data || {}
  const out: any[] = []
  let offset = 0
  for (const band of bands) {
    const bh = Number(band.height || 0) * MM * s()
    const rowsData = data[band.dataSource] || []
    const rows: any[] = band.type === 'detail' ? (rowsData.length ? rowsData : [{}]) : [null]
    for (const row of rows) {
      const els = (band.elements || []).map((el: any) => buildEl(el, row, band.dataSource, offset))
      out.push({ top: offset, height: bh, els })
      offset += bh
    }
  }
  return out
})
</script>

<style scoped>
.preview-scroll { height: 100%; overflow: auto; background: #888; padding: 20px; display: flex; justify-content: center; }
.page { background: #fff; position: relative; box-shadow: 0 2px 12px rgba(0, 0, 0, 0.3); flex-shrink: 0; }
.band { position: absolute; left: 0; right: 0; }
.el { position: absolute; overflow: hidden; }
.ph { width: 100%; height: 100%; display: flex; align-items: center; justify-content: center; font-size: 11px; color: #666; border: 1px dashed #ccc; box-sizing: border-box; }
</style>
