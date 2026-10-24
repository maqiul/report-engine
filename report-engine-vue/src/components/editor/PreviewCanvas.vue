<template>
  <div class="preview-scroll" ref="rootRef">
    <div class="page" :style="{ width: pageW + 'px', minHeight: pageH + 'px' }">
      <div
        v-for="(band, bi) in layout"
        :key="bi"
        class="band"
        :style="{ top: band.top + 'px', height: band.height + 'px' }"
      >
        <div v-for="(el, ei) in band.els" :key="ei" class="el" :style="el.box">
          <span v-if="el.kind === 'text'" :style="el.textStyle">{{ el.content }}</span>
          <div v-else-if="el.kind === 'line'" :style="el.lineStyle"></div>
          <div v-else-if="el.kind === 'shape'" :style="el.shapeStyle"></div>
          <img v-else-if="el.kind === 'image' && el.content" :src="el.content" style="width:100%;height:100%;object-fit:contain" />
          <div v-else-if="el.kind === 'image'" class="ph">🖼 图像</div>
          <svg v-else-if="el.kind === 'barcode'" class="render-box" data-kind="barcode" :data-cfg="JSON.stringify(el.cfg)"></svg>
          <canvas v-else-if="el.kind === 'qrcode'" class="render-box" data-kind="qrcode" :data-cfg="JSON.stringify(el.cfg)"></canvas>
          <div v-else-if="el.kind === 'chart'" class="render-box" data-kind="chart" :data-cfg="JSON.stringify(el.cfg)"></div>
          <table v-else-if="el.kind === 'table'" class="real-table">
            <tr v-for="r in el.cfg.rowCount" :key="r"><td v-for="c in el.cfg.colCount" :key="c"></td></tr>
          </table>
          <div v-else class="ph">{{ el.kind }}</div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
// 纯前端真渲染：图表 ECharts / 条形码 JsBarcode / 二维码 qrcode / 表格真网格 / 图像真图。
// 命令式渲染在 onMounted + watch(layout) 后遍历 .render-box 完成。
import { computed, onMounted, nextTick, watch, ref } from 'vue'
// ECharts 按需引入：只打包用到的图表与组件，减小 bundle
import * as echarts from 'echarts/core'
import { BarChart, LineChart, PieChart, ScatterChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, TitleComponent, LegendComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import JsBarcode from 'jsbarcode'
import QRCode from 'qrcode'

echarts.use([BarChart, LineChart, PieChart, ScatterChart, GridComponent, TooltipComponent, TitleComponent, LegendComponent, CanvasRenderer])

const props = defineProps<{ template: any; data?: Record<string, any[]>; scale?: number }>()
const rootRef = ref<HTMLElement | null>(null)

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
  const nums = rows.map((r: any) => Number(r[field])).filter((n: number) => !isNaN(n))
  if (!nums.length) return '0'
  switch (fn) {
    case 'Sum': return String(nums.reduce((a: number, b: number) => a + b, 0))
    case 'Count': return String(rows.length)
    case 'Avg': return String(Math.round((nums.reduce((a: number, b: number) => a + b, 0) / nums.length) * 100) / 100)
    case 'Max': return String(Math.max(...nums))
    case 'Min': return String(Math.min(...nums))
    default: return ''
  }
}

function mapChartType(t: string): string {
  return ({ Bar: 'bar', Line: 'line', Pie: 'pie', Area: 'line', Scatter: 'scatter' } as Record<string, string>)[t] || 'bar'
}

function buildChartCfg(el: any) {
  const rows = props.data?.[el.dataSource] || []
  const cats = rows.map((r: any) => String(r[el.categoryField] ?? ''))
  const series = (el.series || []).map((sr: any) => ({
    name: sr.name || '', type: mapChartType(el.chartType),
    data: rows.map((r: any) => Number(r[sr.valueField]) || 0),
    areaStyle: el.chartType === 'Area' ? {} : undefined,
    itemStyle: sr.color ? { color: sr.color } : undefined
  }))
  return { title: el.title || '', cats, series, chartType: el.chartType }
}

function buildEl(el: any, row: any, ds: string, bandTop: number): any {
  const sc = s()
  const box = {
    left: (Number(el.x || 0) * MM * sc) + 'px',
    top: (bandTop + Number(el.y || 0) * MM * sc) + 'px',
    width: (Number(el.width || 10) * MM * sc) + 'px',
    height: (Number(el.height || 5) * MM * sc) + 'px'
  }
  const f = el.font || {}
  if (el.type === 'text') {
    let content: string
    if (el.systemVariable) content = evalText('{{' + el.systemVariable + '}}', row)
    else if (el.summaryFunction) content = evalSummary(el.summaryFunction, el.summaryField, ds)
    else if (el.dataField) content = evalText('{{currentRow.' + el.dataField + '}}', row)
    else content = evalText(el.text || '', row)
    return {
      box, kind: 'text', content,
      textStyle: {
        fontFamily: f.family || 'Microsoft YaHei, sans-serif', fontSize: (Number(f.size || 10) * sc) + 'px',
        fontWeight: f.bold ? 'bold' : 'normal', fontStyle: f.italic ? 'italic' : 'normal',
        textDecoration: f.underline ? 'underline' : 'none', color: f.color || '#000',
        textAlign: el.alignment || 'left', display: 'block', width: '100%', lineHeight: '1.2', whiteSpace: 'pre-wrap'
      }
    }
  }
  if (el.type === 'line') return { box, kind: 'line', lineStyle: { borderTop: (Number(el.lineWidth || 0.3) * sc) + 'px solid ' + (el.lineColor || '#000'), position: 'absolute', top: '50%', left: 0, right: 0 } }
  if (el.type === 'shape') return { box, kind: 'shape', shapeStyle: { background: el.fillColor || '#fff', border: '1px solid #000', borderRadius: (Number(el.borderRadius || 0) * sc) + 'px', width: '100%', height: '100%', boxSizing: 'border-box' } }
  if (el.type === 'image') return { box, kind: 'image', content: el.source || '' }
  if (el.type === 'barcode') {
    const isQR = el.format === 'QRCode' || el.format === 'DataMatrix' || el.format === 'PDF417'
    const content = evalText(el.value || '', row)
    return isQR
      ? { box, kind: 'qrcode', cfg: { text: content || ' ' } }
      : { box, kind: 'barcode', cfg: { text: content || '0', format: el.format || 'CODE128', lineColor: el.foreColor || '#000000', background: el.backColor || '#ffffff', displayValue: el.showText !== false } }
  }
  if (el.type === 'chart') return { box, kind: 'chart', cfg: buildChartCfg(el) }
  if (el.type === 'table') return { box, kind: 'table', cfg: { rowCount: Number(el.rowCount) || 3, colCount: Number(el.colCount) || 3 } }
  return { box, kind: el.type || 'other' }
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

function renderWidgets() {
  const root = rootRef.value
  if (!root) return
  // 条形码
  root.querySelectorAll<SVGElement>('svg[data-kind="barcode"]').forEach(svg => {
    try {
      const c = JSON.parse(svg.getAttribute('data-cfg') || '{}')
      JsBarcode(svg, String(c.text || '0'), {
        format: c.format || 'CODE128', displayValue: c.displayValue !== false,
        lineColor: c.lineColor || '#000', background: c.background || '#fff',
        width: 1.5, height: Math.max(10, svg.clientHeight || 40), margin: 0
      })
    } catch { /* ignore */ }
  })
  // 二维码
  root.querySelectorAll<HTMLCanvasElement>('canvas[data-kind="qrcode"]').forEach(cv => {
    try {
      const c = JSON.parse(cv.getAttribute('data-cfg') || '{}')
      const size = Math.max(20, cv.clientWidth || 60)
      cv.width = size; cv.height = size
      QRCode.toCanvas(cv, String(c.text || ' '), { width: size, margin: 0 })
    } catch { /* ignore */ }
  })
  // 图表
  root.querySelectorAll<HTMLElement>('div[data-kind="chart"]').forEach(div => {
    try {
      const c = JSON.parse(div.getAttribute('data-cfg') || '{}')
      const chart = echarts.getInstanceByDom(div) || echarts.init(div)
      const isPie = c.chartType === 'Pie'
      const series = isPie
        ? [{ type: 'pie', radius: '60%', data: (c.cats || []).map((cat: string, i: number) => ({ name: cat, value: c.series?.[0]?.data?.[i] ?? 0 })) }]
        : (c.series || [])
      chart.setOption({
        title: c.title ? { text: c.title, left: 'center', textStyle: { fontSize: 12 } } : undefined,
        tooltip: { trigger: isPie ? 'item' : 'axis' },
        xAxis: isPie ? undefined : { type: 'category', data: c.cats || [] },
        yAxis: isPie ? undefined : { type: 'value' },
        series
      }, true)
    } catch { /* ignore */ }
  })
}

onMounted(() => nextTick(renderWidgets))
watch(layout, () => nextTick(renderWidgets))
</script>

<style scoped>
.preview-scroll { height: 100%; overflow: auto; background: #888; padding: 20px; display: flex; justify-content: center; }
.page { background: #fff; position: relative; box-shadow: 0 2px 12px rgba(0, 0, 0, 0.3); flex-shrink: 0; }
.band { position: absolute; left: 0; right: 0; }
.el { position: absolute; overflow: hidden; }
.render-box { width: 100%; height: 100%; display: block; }
.real-table { width: 100%; height: 100%; border-collapse: collapse; table-layout: fixed; }
.real-table td { border: 1px solid #999; }
.ph { width: 100%; height: 100%; display: flex; align-items: center; justify-content: center; font-size: 11px; color: #666; border: 1px dashed #ccc; box-sizing: border-box; }
</style>
