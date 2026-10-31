<template>
  <div class="preview-scroll" ref="rootRef">
    <div class="page" :style="{ width: pageW + 'px', minHeight: pageH + 'px' }">
      <div
        v-for="(band, bi) in layout"
        :key="bi"
        class="band"
        :style="{ top: band.top + 'px', height: band.height + 'px' }"
      >
        <div v-for="(el, ei) in band.els" :key="ei" class="el" :style="css(el.box)">
          <div v-if="el.kind === 'text'" class="el-text" :style="css(el.textStyle)">
            <template v-if="el.lines && el.lines.length > 1">
              <div v-for="(ln, i) in el.lines" :key="i" class="el-line-text">{{ ln }}</div>
            </template>
            <template v-else>{{ el.content }}</template>
          </div>
          <div v-else-if="el.kind === 'line'" :style="css(el.lineStyle)"></div>
          <div v-else-if="el.kind === 'shape'" :style="css(el.shapeStyle)"></div>
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
// 纯前端真渲染：渲染数据来自 renderCore（纯函数），本组件只负责把结果画到 DOM。
// 图表 ECharts / 条形码 JsBarcode / 二维码 qrcode 为命令式渲染。
import { computed, onMounted, nextTick, watch, ref } from 'vue'
import * as echarts from 'echarts/core'
import { BarChart, LineChart, PieChart, ScatterChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, TitleComponent, LegendComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import JsBarcode from 'jsbarcode'
import QRCode from 'qrcode'
import { renderTemplate, MM_TO_PX } from '../../render/renderCore'

echarts.use([BarChart, LineChart, PieChart, ScatterChart, GridComponent, TooltipComponent, TitleComponent, LegendComponent, CanvasRenderer])

const props = defineProps<{ template: any; data?: Record<string, any[]>; scale?: number }>()
const rootRef = ref<HTMLElement | null>(null)

const s = () => Number(props.scale || 1)
const pageW = computed(() => Number(props.template?.page?.width || 210) * MM_TO_PX * 0.5 * s())
const pageH = computed(() => Number(props.template?.page?.height || 297) * MM_TO_PX * 0.5 * s())

const layout = computed(() => renderTemplate(props.template, props.data || {}, { zoom: 0.5 * s() }))

// Vue :style 期望 CSSProperties，renderCore 返回 Record<string,string> —— 此处做类型桥接
function css(v: unknown): any {
  return v
}

function renderWidgets() {
  const root = rootRef.value
  if (!root) return
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
  root.querySelectorAll<HTMLCanvasElement>('canvas[data-kind="qrcode"]').forEach(cv => {
    try {
      const c = JSON.parse(cv.getAttribute('data-cfg') || '{}')
      const size = Math.max(20, cv.clientWidth || 60)
      cv.width = size; cv.height = size
      QRCode.toCanvas(cv, String(c.text || ' '), { width: size, margin: 0 })
    } catch { /* ignore */ }
  })
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
.el { position: absolute; overflow: hidden; transform-origin: center center; }
.el-text { width: 100%; height: 100%; box-sizing: border-box; }
.el-line-text { white-space: pre-wrap; word-break: break-all; }
.render-box { width: 100%; height: 100%; display: block; }
.real-table { width: 100%; height: 100%; border-collapse: collapse; table-layout: fixed; }
.real-table td { border: 1px solid #999; }
.ph { width: 100%; height: 100%; display: flex; align-items: center; justify-content: center; font-size: 11px; color: #666; border: 1px dashed #ccc; box-sizing: border-box; }
</style>
