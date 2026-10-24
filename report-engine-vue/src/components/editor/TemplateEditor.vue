<template>
  <div class="template-editor" ref="rootRef">
    <div class="ruler-corner">{{ Math.round(zoom * 100) }}%</div>
    <div class="ruler ruler-h" :style="rulerHStyle"></div>
    <div class="ruler ruler-v" :style="rulerVStyle"></div>

    <div class="canvas-scroll" @scroll="onScroll">
      <div class="canvas-wrap" :style="{ width: pageWidthPx + 'px' }">
        <!-- 画布（A4 比例） -->
        <div
          class="canvas"
          :style="{
            width: pageWidthPx + 'px',
            minHeight: pageHeightPx + 'px',
            background: props.showGrid !== false
              ? '#fff repeating-linear-gradient(0deg, rgba(0,0,0,0.05) 0, rgba(0,0,0,0.05) 1px, transparent 1px, transparent 20px) repeating-linear-gradient(90deg, rgba(0,0,0,0.05) 0, rgba(0,0,0,0.05) 1px, transparent 1px, transparent 20px)'
              : '#fff'
          }"
          @click="onCanvasClick"
          @mousedown="onCanvasMouseDown"
        >
          <!-- 吸附辅助线 -->
          <div
            v-for="(g, gi) in guides"
            :key="'g' + gi"
            class="snap-guide"
            :class="g.type"
            :style="g.type === 'v' ? { left: g.pos + 'px' } : { top: g.pos + 'px' }"
          ></div>

          <!-- Band 区域 -->
          <div
            v-if="marquee.visible"
            class="marquee"
            :style="{ left: marquee.left + 'px', top: marquee.top + 'px', width: marquee.width + 'px', height: marquee.height + 'px' }"
          ></div>
          <div
            v-for="(band, bi) in template.bands || []"
            :key="bi"
            class="band"
            :class="{
              active: selectedBandIndex === bi,
              editing: selectedBandIndex === bi
            }"
            :style="getBandStyle(Number(bi))"
            @click.stop="selectBand(Number(bi))"
          >
            <div class="band-label">
              {{ bandLabel(band.type) }} · h={{ band.height }}mm · {{ (band.elements || []).length }} 元素
            </div>
            <!-- 元素 -->
            <div
              v-for="(el, ei) in band.elements || []"
              :key="ei"
              class="element"
              :class="{ selected: isSelected(Number(bi), Number(ei)), locked: el.locked }"
              @contextmenu.prevent="onContextMenu($event, Number(bi), Number(ei))"
              :style="getElementBox(Number(bi), Number(ei))"
              @mousedown.stop="onElementMouseDown($event, el, Number(bi), Number(ei))"
              @click.stop="selectElement(Number(bi), Number(ei))"
            >
              <div v-if="el.type === 'text'" class="el-text" :style="getTextStyle(Number(bi), Number(ei))">
                <template v-if="el.systemVariable">⊕ {{ el.systemVariable }}</template>
                <template v-else-if="el.summaryFunction">Σ {{ el.summaryFunction }}({{ el.summaryField }})</template>
                <template v-else-if="el.dataField">⛁ {{ el.dataField }}</template>
                <template v-else>{{ el.text || '(空文本)' }}</template>
              </div>
              <div v-else-if="el.type === 'line'" class="el-line" :style="getLineStyle(Number(bi), Number(ei))"></div>
              <div v-else-if="el.type === 'shape'" class="el-shape" :style="getShapeStyle(Number(bi), Number(ei))"></div>
              <img v-else-if="el.type === 'image' && el.source" :src="el.source" class="el-render" style="object-fit:contain" />
              <div v-else-if="el.type === 'image'" class="el-ph el-image">🖼 图像(未设置)</div>
              <svg v-else-if="el.type === 'barcode' && !isQr(el)" class="el-render" data-kind="barcode" :data-cfg="barcodeCfg(el)"></svg>
              <canvas v-else-if="el.type === 'barcode' && isQr(el)" class="el-render" data-kind="qrcode" :data-cfg="qrCfg(el)"></canvas>
              <table v-else-if="el.type === 'table'" class="el-render real-table"><tr v-for="r in (Number(el.rowCount)||3)" :key="r"><td v-for="c in (Number(el.colCount)||3)" :key="c"></td></tr></table>
              <div v-else-if="el.type === 'crosstab'" class="el-ph el-crosstab">⊞ 交叉表</div>
              <div v-else-if="el.type === 'chart'" class="el-render" data-kind="chart" :data-cfg="chartCfg(el)"></div>
              <div v-else-if="el.type === 'subreport'" class="el-ph el-subreport">⧉ 子报表 {{ el.templateRef || '(未指定)' }}</div>
              <div v-else class="el-other">{{ el.type }}</div>

              <!-- 锁定标记 -->
              <span v-if="el.locked" class="lock-badge">🔒</span>

              <!-- 组合标记：显示组号 -->
              <span v-if="el.groupId" class="group-badge">G</span>

              <!-- 选中时显示 8 个调节手柄 -->
              <template v-if="isSelected(Number(bi), Number(ei))">
                <div class="handle nw" @mousedown.stop="startResize($event, el, Number(bi), Number(ei), 'nw')"></div>
                <div class="handle n" @mousedown.stop="startResize($event, el, Number(bi), Number(ei), 'n')"></div>
                <div class="handle ne" @mousedown.stop="startResize($event, el, Number(bi), Number(ei), 'ne')"></div>
                <div class="handle e" @mousedown.stop="startResize($event, el, Number(bi), Number(ei), 'e')"></div>
                <div class="handle se" @mousedown.stop="startResize($event, el, Number(bi), Number(ei), 'se')"></div>
                <div class="handle s" @mousedown.stop="startResize($event, el, Number(bi), Number(ei), 's')"></div>
                <div class="handle sw" @mousedown.stop="startResize($event, el, Number(bi), Number(ei), 'sw')"></div>
                <div class="handle w" @mousedown.stop="startResize($event, el, Number(bi), Number(ei), 'w')"></div>
              </template>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 右键菜单 -->
    <div
      v-if="ctxMenu.visible"
      class="ctx-menu"
      :style="{ left: ctxMenu.x + 'px', top: ctxMenu.y + 'px' }"
      @mousedown.stop
      @contextmenu.prevent
    >
      <button class="ctx-item" @click="ctxCopy">复制</button>
      <button class="ctx-item" @click="ctxDelete">删除</button>
      <div class="ctx-sep"></div>
      <button class="ctx-item" @click="ctxToggleLock">{{ ctxMenu.locked ? '解锁' : '锁定' }}</button>
      <button class="ctx-item" @click="ctxGroup">组合</button>
      <button class="ctx-item" @click="ctxUngroup">取消组合</button>
      <div class="ctx-sep"></div>
      <button class="ctx-item" @click="ctxMove('front')">置于顶层</button>
      <button class="ctx-item" @click="ctxMove('up')">上移一层</button>
      <button class="ctx-item" @click="ctxMove('down')">下移一层</button>
      <button class="ctx-item" @click="ctxMove('back')">置于底层</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount, nextTick, watch } from 'vue'
// ECharts 按需引入
import * as echarts from 'echarts/core'
import { BarChart, LineChart, PieChart, ScatterChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, TitleComponent, LegendComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import JsBarcode from 'jsbarcode'
import QRCode from 'qrcode'

echarts.use([BarChart, LineChart, PieChart, ScatterChart, GridComponent, TooltipComponent, TitleComponent, LegendComponent, CanvasRenderer])

const props = defineProps<{
  template: any
  selectedBandIndex: number
  selectedElementIndex: number
  zoom: number
  data?: Record<string, any[]>
  showGrid?: boolean
  formatPainterActive?: boolean
}>()

const emit = defineEmits<{
  (e: 'selectBand', i: number): void
  (e: 'selectElement', bandIndex: number, elementIndex: number): void
  (e: 'selectionChange', keys: Array<[number, number]>): void
  (e: 'formatPaint', bandIndex: number, elementIndex: number): void
  (e: 'canvasClick'): void
  (e: 'update'): void
}>()

const rootRef = ref<HTMLElement | null>(null)

// ===== 多选集合（key = "bi:ei"）；主选中仍由 props.selectedElementIndex 驱动 =====
const selectedKeys = ref<string[]>([])
const lastAnchor = ref<[number, number] | null>(null)

function keyOf(bi: number, ei: number): string { return bi + ':' + ei }
function parseKey(k: string): [number, number] {
  const p = k.split(':')
  return [Number(p[0]), Number(p[1])]
}

// 1mm = 3.7795px (96 DPI)，用 mm * zoom * 0.5 让 A4 适合屏幕
const MM_TO_PX = 3.7795

const pageWidthPx = computed(() => Number(props.template?.page?.width || 210) * Number(MM_TO_PX) * Number(props.zoom) * 0.5)
const pageHeightPx = computed(() => Number(props.template?.page?.height || 297) * Number(MM_TO_PX) * Number(props.zoom) * 0.5)

// 预计算所有 band 的样式（避免模板里函数调用类型推断问题）
const bandStyles = computed(() => {
  const result: Array<{ top: number; height: number; background: string }> = []
  if (!props.template?.bands) return result
  let offset = 0
  for (const band of props.template.bands) {
    const h = Number(band?.height || 0) * Number(MM_TO_PX) * Number(props.zoom) * 0.5
    result.push({
      top: offset,
      height: h,
      background: bandBgColor(band.type)
    })
    offset += h
  }
  return result
})

// 预计算所有元素的样式
interface ElementStyle {
  left: number
  top: number
  width: number
  height: number
  fontFamily: string
  fontSize: string
  fontWeight: string
  fontStyle: string
  textDecoration: string
  textAlign: string
  color: string
  background: string
  lineHeight: string
  borderColor: string
  borderWidth: string
}

const elementStyles = computed<ElementStyle[][]>(() => {
  const u = Number(MM_TO_PX) * Number(props.zoom) * 0.5
  const result: ElementStyle[][] = []
  if (!props.template?.bands) return result
  for (const band of props.template.bands) {
    const list: ElementStyle[] = []
    for (const el of (band.elements || [])) {
      const f = el.font || {}
      list.push({
        left: Number(el.x || 0) * u,
        top: Number(el.y || 0) * u,
        width: Number(el.width || 10) * u,
        height: Number(el.height || 5) * u,
        fontFamily: f.family || 'Microsoft YaHei, sans-serif',
        fontSize: String(Number(f.size || 10) * Number(props.zoom)) + 'px',
        fontWeight: f.bold ? 'bold' : 'normal',
        fontStyle: f.italic ? 'italic' : 'normal',
        textDecoration: f.underline ? 'underline' : 'none',
        textAlign: el.alignment || 'left',
        color: f.color || '#000',
        background: el.borderColor || '#000',
        lineHeight: String(Number(el.lineWidth || 0.5) * Number(props.zoom)) + 'px',
        borderColor: el.borderColor || '#000',
        borderWidth: String(Number(el.lineWidth || 0.5) * Number(props.zoom)) + 'px'
      })
    }
    result.push(list)
  }
  return result
})

function bandOffsetPx(i: number): number {
  if (!props.template?.bands) return 0
  let offset = 0
  for (let j = 0; j < i; j++) {
    offset += Number(props.template.bands[j]?.height || 0) * Number(MM_TO_PX) * Number(props.zoom) * 0.5
  }
  return Number(offset)
}

function bandHeightPx(band: any): number {
  return Number(band?.height || 0) * Number(MM_TO_PX) * Number(props.zoom) * 0.5
}

function bandBgColor(type: string): string {
  const map: Record<string, string> = {
    title: 'rgba(37, 99, 235, 0.04)',
    reportHeader: 'rgba(5, 150, 105, 0.04)',
    pageHeader: 'rgba(5, 150, 105, 0.08)',
    detail: 'rgba(217, 119, 6, 0.03)',
    pageFooter: 'rgba(124, 58, 237, 0.05)',
    summary: 'rgba(0, 0, 0, 0.03)'
  }
  return map[type] || 'rgba(0, 0, 0, 0.02)'
}

function bandLabel(type: string): string {
  const map: Record<string, string> = {
    title: '标题',
    reportHeader: '报表头',
    pageHeader: '页头',
    detail: '明细',
    pageFooter: '页脚',
    summary: '汇总'
  }
  return map[type] || type
}

function isSelected(bi: number, ei: number) {
  return selectedKeys.value.includes(keyOf(bi, ei))
}

// 类型安全 helper：把 number 转 string，给模板用
function px(n: number): string { return n + 'px' }
function n(v: any, d: number): number { const x = Number(v); return isNaN(x) ? d : x }

function getBandStyle(bi: number): Record<string, string> {
  const s = bandStyles.value[bi] || { top: 0, height: 0, background: 'transparent' }
  return { top: String(s.top) + 'px', height: String(s.height) + 'px', background: s.background }
}

function getElementBox(bi: number, ei: number): Record<string, string> {
  const s = elementStyles.value[bi]?.[ei]
  if (!s) return {}
  return { left: String(s.left) + 'px', top: String(s.top) + 'px', width: String(s.width) + 'px', height: String(s.height) + 'px' }
}

function getTextStyle(bi: number, ei: number): Record<string, string> {
  const s = elementStyles.value[bi]?.[ei]
  if (!s) return {}
  return {
    fontFamily: s.fontFamily,
    fontSize: s.fontSize,
    fontWeight: s.fontWeight,
    fontStyle: s.fontStyle,
    textDecoration: s.textDecoration,
    textAlign: s.textAlign,
    color: s.color
  }
}

function getLineStyle(bi: number, ei: number): Record<string, string> {
  const s = elementStyles.value[bi]?.[ei]
  if (!s) return {}
  return { background: s.background, height: s.lineHeight, top: '50%', transform: 'translateY(-50%)' }
}

function getShapeStyle(bi: number, ei: number): Record<string, string> {
  const s = elementStyles.value[bi]?.[ei]
  const el = props.template?.bands?.[bi]?.elements?.[ei]
  if (!s) return {}
  return {
    borderColor: s.borderColor,
    borderWidth: s.borderWidth,
    borderStyle: 'solid',
    background: el?.fillColor || '#FFFFFF',
    borderRadius: String(Number(el?.borderRadius || 0) * 3.7795) + 'px'
  }
}

function selectBand(i: number) {
  emit('selectBand', i)
}

function notifySelection() {
  emit('selectionChange', selectedKeys.value.map(parseKey))
}

function applySelection(bi: number, ei: number, e?: MouseEvent) {
  const k = keyOf(bi, ei)
  if (e && (e.ctrlKey || e.metaKey)) {
    // Ctrl/Cmd+点击：切换选中
    const idx = selectedKeys.value.indexOf(k)
    if (idx >= 0) selectedKeys.value.splice(idx, 1)
    else selectedKeys.value.push(k)
    if (selectedKeys.value.includes(k)) emit('selectElement', bi, ei)
    notifySelection()
    return
  }
  if (e && e.shiftKey && lastAnchor.value) {
    // Shift+点击：同 band 内范围选
    const [abi, aei] = lastAnchor.value
    if (abi === bi) {
      const els = props.template?.bands?.[bi]?.elements || []
      const lo = Math.min(aei, ei)
      const hi = Math.max(aei, ei)
      const next: string[] = []
      for (let i = lo; i <= hi; i++) next.push(keyOf(bi, i))
      selectedKeys.value = next
      emit('selectElement', bi, ei)
      notifySelection()
      return
    }
  }
  // 普通单击：单选
  selectedKeys.value = [k]
  lastAnchor.value = [bi, ei]
  emit('selectElement', bi, ei)
  notifySelection()
}

function selectElement(bi: number, ei: number) {
  applySelection(bi, ei)
}

function onCanvasClick() {
  selectedKeys.value = []
  lastAnchor.value = null
  notifySelection()
  emit('canvasClick')
}

// ===== 框选（marquee）=====
const marquee = ref({ visible: false, left: 0, top: 0, width: 0, height: 0 })
let marqueeStart: { x: number; y: number } | null = null

function onCanvasMouseDown(e: MouseEvent) {
  // 只在空白处（无 ctrl/shift）开始框选；元素上的 mousedown 由元素自己 stop
  if (e.button !== 0) return
  const target = e.target as HTMLElement
  if (target.closest('.element')) return
  const canvasEl = target.closest('.canvas') as HTMLElement | null
  if (!canvasEl) return
  const rect = canvasEl.getBoundingClientRect()
  marqueeStart = { x: e.clientX - rect.left, y: e.clientY - rect.top }
  marquee.value = { visible: true, left: marqueeStart.x, top: marqueeStart.y, width: 0, height: 0 }
  document.addEventListener('mousemove', onMarqueeMove)
  document.addEventListener('mouseup', onMarqueeUp)
}

function onMarqueeMove(e: MouseEvent) {
  if (!marqueeStart) return
  const canvasEl = rootRef.value?.querySelector('.canvas') as HTMLElement | null
  if (!canvasEl) return
  const rect = canvasEl.getBoundingClientRect()
  const x = e.clientX - rect.left
  const y = e.clientY - rect.top
  marquee.value = {
    visible: true,
    left: Math.min(marqueeStart.x, x),
    top: Math.min(marqueeStart.y, y),
    width: Math.abs(x - marqueeStart.x),
    height: Math.abs(y - marqueeStart.y)
  }
}

function onMarqueeUp() {
  document.removeEventListener('mousemove', onMarqueeMove)
  document.removeEventListener('mouseup', onMarqueeUp)
  if (!marqueeStart) return
  const m = marquee.value
  marquee.value = { visible: false, left: 0, top: 0, width: 0, height: 0 }
  // 拖动距离过小视为点击空白
  if (m.width < 3 && m.height < 3) { marqueeStart = null; return }
  const u = Number(MM_TO_PX) * Number(props.zoom) * 0.5
  const picked: string[] = []
  const bands = props.template?.bands || []
  for (let bi = 0; bi < bands.length; bi++) {
    const bandTop = bandOffsetPx(bi)
    const els = bands[bi]?.elements || []
    for (let ei = 0; ei < els.length; ei++) {
      const el = els[ei]
      const ex = Number(el.x || 0) * u
      const ey = bandTop + Number(el.y || 0) * u
      const ew = Number(el.width || 0) * u
      const eh = Number(el.height || 0) * u
      if (ex + ew >= m.left && ex <= m.left + m.width && ey + eh >= m.top && ey <= m.top + m.height) {
        picked.push(keyOf(bi, ei))
      }
    }
  }
  if (picked.length) {
    selectedKeys.value = picked
    const lastKey = picked[picked.length - 1]
    if (lastKey) {
      const [pb, pe] = parseKey(lastKey)
      emit('selectElement', pb, pe)
    }
    notifySelection()
  }
  marqueeStart = null
}

// ============== 拖拽 ==============

interface DragState {
  type: 'move' | 'resize'
  el: any
  bi: number
  ei: number
  handle?: string
  startX: number
  startY: number
  origX: number
  origY: number
  origW: number
  origH: number
  moving?: Array<{ b: number; i: number; el: any; x: number; y: number }>
}

const dragState = ref<DragState | null>(null)

function onElementMouseDown(e: MouseEvent, el: any, bi: number, ei: number) {
  // 格式刷模式：点击目标元素 → 上抛 formatPaint（不进入选择/拖拽）
  if (props.formatPainterActive) {
    emit('formatPaint', bi, ei)
    return
  }
  applySelection(bi, ei, e)
  // 锁定元素：可选中但不可拖动
  if (el.locked) return
  // 记录所有选中元素的原始位置（多选整体拖动）
  const moving = selectedKeys.value.map(k => {
    const [b, i] = parseKey(k)
    const e2 = props.template?.bands?.[b]?.elements?.[i]
    return { b, i, el: e2, x: Number(e2?.x || 0), y: Number(e2?.y || 0) }
  }).filter(m => m.el)
  dragState.value = {
    type: 'move',
    el,
    bi,
    ei,
    startX: e.clientX,
    startY: e.clientY,
    origX: el.x || 0,
    origY: el.y || 0,
    origW: el.width || 10,
    origH: el.height || 5,
    moving
  } as any
  document.addEventListener('mousemove', onMouseMove)
  document.addEventListener('mouseup', onMouseUp)
}

function startResize(e: MouseEvent, el: any, bi: number, ei: number, handle: string) {
  selectElement(bi, ei)
  dragState.value = {
    type: 'resize',
    el,
    bi,
    ei,
    handle,
    startX: e.clientX,
    startY: e.clientY,
    origX: el.x || 0,
    origY: el.y || 0,
    origW: el.width || 10,
    origH: el.height || 5
  }
  document.addEventListener('mousemove', onMouseMove)
  document.addEventListener('mouseup', onMouseUp)
}

function onMouseMove(e: MouseEvent) {
  const s = dragState.value
  if (!s) return

  const unit: number = Number(MM_TO_PX) * Number(props.zoom) * 0.5
  const dx: number = (e.clientX - s.startX) / unit
  const dy: number = (e.clientY - s.startY) / unit

  if (s.type === 'move') {
    if (s.moving && s.moving.length > 1) {
      // 多选整体移动（主元素吸附，整组跟随）
      const main = s.moving.find(m => m.b === s.bi && m.i === s.ei) || s.moving[0]
      if (main) {
        const snapped = computeSnap(main.b, main.i, Number(main.x) + dx, Number(main.y) + dy)
        const adjX = snapped.x - (Number(main.x) + dx)
        const adjY = snapped.y - (Number(main.y) + dy)
        for (const m of s.moving) {
          m.el.x = Math.max(0, m.x + dx + adjX)
          m.el.y = Math.max(0, m.y + dy + adjY)
        }
      }
    } else {
      const snapped = computeSnap(s.bi, s.ei, Number(s.origX) + dx, Number(s.origY) + dy)
      s.el.x = Math.max(0, snapped.x)
      s.el.y = Math.max(0, snapped.y)
    }
  } else {
    const h = s.handle!
    if (h.includes('e')) s.el.width = Math.max(1, Number(s.origW) + dx)
    if (h.includes('s')) s.el.height = Math.max(1, Number(s.origH) + dy)
    if (h.includes('w')) {
      const newW: number = Math.max(1, Number(s.origW) - dx)
      s.el.x = Number(s.origX) + (Number(s.origW) - newW)
      s.el.width = newW
    }
    if (h.includes('n')) {
      const newH: number = Math.max(1, Number(s.origH) - dy)
      s.el.y = Number(s.origY) + (Number(s.origH) - newH)
      s.el.height = newH
    }
  }
  emit('update')
}

function onMouseUp() {
  dragState.value = null
  guides.value = []
  document.removeEventListener('mousemove', onMouseMove)
  document.removeEventListener('mouseup', onMouseUp)
}

function onScroll() {
  // 标尺随画布滚动同步偏移
  const sc = rootRef.value?.querySelector('.canvas-scroll') as HTMLElement | null
  if (sc) {
    scrollLeft.value = sc.scrollLeft
    scrollTop.value = sc.scrollTop
  }
}

// ===== 标尺刻度（10mm 一格）+ 滚动偏移 =====
const scrollLeft = ref(0)
const scrollTop = ref(0)
const unitPx = computed(() => Number(MM_TO_PX) * Number(props.zoom) * 0.5)
const tickPx = computed(() => 10 * unitPx.value)

const rulerHStyle = computed(() => ({
  backgroundImage: `repeating-linear-gradient(90deg, rgba(255,255,255,0.35) 0, rgba(255,255,255,0.35) 1px, transparent 1px, transparent ${tickPx.value}px)`,
  backgroundPosition: `${-scrollLeft.value}px 0`
}))

const rulerVStyle = computed(() => ({
  backgroundImage: `repeating-linear-gradient(0deg, rgba(255,255,255,0.35) 0, rgba(255,255,255,0.35) 1px, transparent 1px, transparent ${tickPx.value}px)`,
  backgroundPosition: `0 ${-scrollTop.value}px`
}))

// ===== 编辑画布真渲染（图表/条形码/二维码/表格），与预览一致 =====
function isQr(el: any): boolean {
  const f = el.format
  return f === 'QRCode' || f === 'DataMatrix' || f === 'PDF417'
}
function sampleValue(el: any): string {
  const v = el.value
  if (!v || String(v).includes('{{')) return '1234567890'
  return String(v)
}
function barcodeCfg(el: any): string {
  return JSON.stringify({ text: sampleValue(el), format: el.format || 'CODE128', lineColor: el.foreColor || '#000', background: el.backColor || '#fff', displayValue: el.showText !== false })
}
function qrCfg(el: any): string {
  const v = el.value
  return JSON.stringify({ text: (!v || String(v).includes('{{')) ? 'https://report-engine' : String(v) })
}
function chartCfg(el: any): string {
  const rows = (props.data && el.dataSource && props.data[el.dataSource]) || [{}, {}, {}]
  const cats = rows.map((r: any, i: number) => String(r[el.categoryField] ?? ('项' + (i + 1))))
  const typeMap: Record<string, string> = { Bar: 'bar', Line: 'line', Pie: 'pie', Area: 'line', Scatter: 'scatter' }
  const series = (el.series && el.series.length ? el.series : [{ valueField: '' }]).map((sr: any) => ({
    name: sr.name || '', type: typeMap[el.chartType] || 'bar',
    data: rows.map((r: any) => Number(r[sr.valueField]) || Math.round(Math.random() * 100))
  }))
  return JSON.stringify({ title: el.title || '', cats, series, chartType: el.chartType })
}

function renderWidgets() {
  const root = rootRef.value
  if (!root) return
  root.querySelectorAll<SVGElement>('svg[data-kind="barcode"]').forEach(svg => {
    try { const c = JSON.parse(svg.getAttribute('data-cfg') || '{}'); JsBarcode(svg, String(c.text || '0'), { format: c.format || 'CODE128', displayValue: c.displayValue !== false, lineColor: c.lineColor || '#000', background: c.background || '#fff', width: 1.5, height: Math.max(10, svg.clientHeight || 40), margin: 0 }) } catch { /* ignore */ }
  })
  root.querySelectorAll<HTMLCanvasElement>('canvas[data-kind="qrcode"]').forEach(cv => {
    try { const c = JSON.parse(cv.getAttribute('data-cfg') || '{}'); const sz = Math.max(20, cv.clientWidth || 60); cv.width = sz; cv.height = sz; QRCode.toCanvas(cv, String(c.text || ' '), { width: sz, margin: 0 }) } catch { /* ignore */ }
  })
  root.querySelectorAll<HTMLElement>('div[data-kind="chart"]').forEach(div => {
    try {
      const c = JSON.parse(div.getAttribute('data-cfg') || '{}')
      const chart = echarts.getInstanceByDom(div) || echarts.init(div)
      const isPie = c.chartType === 'Pie'
      const series = isPie ? [{ type: 'pie', radius: '60%', data: (c.cats || []).map((cat: string, i: number) => ({ name: cat, value: c.series?.[0]?.data?.[i] ?? 0 })) }] : (c.series || [])
      chart.setOption({ title: c.title ? { text: c.title, left: 'center', textStyle: { fontSize: 12 } } : undefined, tooltip: {}, xAxis: isPie ? undefined : { type: 'category', data: c.cats || [] }, yAxis: isPie ? undefined : { type: 'value' }, series }, true)
    } catch { /* ignore */ }
  })
}

onMounted(() => {
  nextTick(renderWidgets)
  window.addEventListener('keydown', onKey)
  document.addEventListener('mousedown', onDocMouseDown)
})
onBeforeUnmount(() => {
  window.removeEventListener('keydown', onKey)
  document.removeEventListener('mousedown', onDocMouseDown)
})

function onDocMouseDown(e: MouseEvent) {
  if (!ctxMenu.value.visible) return
  const t = e.target as HTMLElement
  if (t.closest && t.closest('.ctx-menu')) return
  closeCtxMenu()
}
watch(() => [props.template, props.zoom, props.selectedElementIndex], () => nextTick(renderWidgets))

// ===== 吸附（拖动时靠近其他元素/页边距自动对齐 + 黄色辅助线）=====
const guides = ref<Array<{ type: 'v' | 'h'; pos: number }>>([])
const SNAP_MM = 1.5

function snapAxis(value: number, size: number, cands: number[]): { v: number; guide: number | null; dist: number } {
  let best = { v: value, guide: null as number | null, dist: Infinity }
  for (const c of cands) {
    for (const anchor of [0, size / 2, size]) {
      const d = Math.abs((value + anchor) - c)
      if (d < best.dist) best = { v: value + (c - anchor), guide: c, dist: d }
    }
  }
  return best
}

function computeSnap(bi: number, ei: number, nx: number, ny: number) {
  const u = unitPx.value
  const threshPx = SNAP_MM * u
  const band = props.template?.bands?.[bi]
  const els = band?.elements || []
  const pageW = Number(props.template?.page?.width || 210)
  const margin = props.template?.page?.margin || {}
  const self = els[ei] || {}
  const w = Number(self.width || 0)
  const h = Number(self.height || 0)

  const xCands: number[] = [0, pageW]
  if (margin.left !== undefined) xCands.push(Number(margin.left))
  if (margin.right !== undefined) xCands.push(pageW - Number(margin.right))
  const yCands: number[] = [0]
  if (margin.top !== undefined) yCands.push(Number(margin.top))
  for (let i = 0; i < els.length; i++) {
    if (i === ei) continue
    const e2 = els[i]
    const ex = Number(e2.x || 0), ew = Number(e2.width || 0)
    const ey = Number(e2.y || 0), eh = Number(e2.height || 0)
    xCands.push(ex, ex + ew, ex + ew / 2)
    yCands.push(ey, ey + eh, ey + eh / 2)
  }

  const rx = snapAxis(nx, w, xCands)
  const ry = snapAxis(ny, h, yCands)
  const out: Array<{ type: 'v' | 'h'; pos: number }> = []
  let fx = nx, fy = ny
  if (rx.guide !== null && rx.dist <= threshPx) {
    fx = rx.v
    out.push({ type: 'v', pos: rx.guide * u })
  }
  if (ry.guide !== null && ry.dist <= threshPx) {
    fy = ry.v
    out.push({ type: 'h', pos: bandOffsetPx(bi) + ry.guide * u })
  }
  guides.value = out
  return { x: fx, y: fy }
}
// ===== 右键菜单 =====
const ctxMenu = ref({ visible: false, x: 0, y: 0, bi: -1, ei: -1, locked: false })

function closeCtxMenu() { ctxMenu.value = { ...ctxMenu.value, visible: false } }

function onContextMenu(e: MouseEvent, bi: number, ei: number) {
  const el = props.template?.bands?.[bi]?.elements?.[ei]
  ctxMenu.value = { visible: true, x: e.clientX, y: e.clientY, bi, ei, locked: !!el?.locked }
  if (!isSelected(bi, ei)) applySelection(bi, ei)
}

function ctxCopy() {
  const el = props.template?.bands?.[ctxMenu.value.bi]?.elements?.[ctxMenu.value.ei]
  if (!el) { closeCtxMenu(); return }
  const els = props.template?.bands?.[ctxMenu.value.bi]?.elements
  if (!els) { closeCtxMenu(); return }
  const copy = JSON.parse(JSON.stringify(el))
  copy.x = Number(el.x || 0) + 5
  copy.y = Number(el.y || 0) + 5
  els.push(copy)
  emit('update')
  closeCtxMenu()
}

function ctxDelete() {
  const els = props.template?.bands?.[ctxMenu.value.bi]?.elements
  if (els) els.splice(ctxMenu.value.ei, 1)
  emit('update')
  closeCtxMenu()
}

function ctxToggleLock() {
  const el = props.template?.bands?.[ctxMenu.value.bi]?.elements?.[ctxMenu.value.ei]
  if (el) el.locked = !el.locked
  emit('update')
  closeCtxMenu()
}

function ctxGroup() {
  const els = props.template?.bands?.[ctxMenu.value.bi]?.elements
  if (!els) { closeCtxMenu(); return }
  const gid = 'g' + Date.now().toString(36)
  for (const e2 of els) e2.groupId = gid
  emit('update')
  closeCtxMenu()
}

function ctxUngroup() {
  const els = props.template?.bands?.[ctxMenu.value.bi]?.elements
  if (els) for (const e2 of els) delete e2.groupId
  emit('update')
  closeCtxMenu()
}

function ctxMove(mode: 'front' | 'up' | 'down' | 'back') {
  const els = props.template?.bands?.[ctxMenu.value.bi]?.elements
  const i = ctxMenu.value.ei
  if (!els || i < 0 || i >= els.length) { closeCtxMenu(); return }
  const [el] = els.splice(i, 1)
  if (mode === 'front') els.push(el)
  else if (mode === 'back') els.unshift(el)
  else if (mode === 'up' && i < els.length) els.splice(i + 1, 0, el)
  else if (mode === 'down' && i > 0) els.splice(i - 1, 0, el)
  else els.splice(i, 0, el)
  emit('update')
  closeCtxMenu()
}

// ===== 键盘：方向键微调 / Delete 删除 / Ctrl+D 复制 =====
function currentSelEl(): any {
  if (props.selectedBandIndex < 0 || props.selectedElementIndex < 0) return null
  return props.template?.bands?.[props.selectedBandIndex]?.elements?.[props.selectedElementIndex] ?? null
}
function onKey(e: KeyboardEvent) {
  const tag = (e.target as HTMLElement)?.tagName
  if (tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT') return
  const el = currentSelEl()
  if (!el) return
  const step = e.shiftKey ? 5 : 1
  const band = props.template?.bands?.[props.selectedBandIndex]
  if (e.key === 'ArrowLeft') { el.x = Math.max(0, Number(el.x || 0) - step); emit('update'); e.preventDefault() }
  else if (e.key === 'ArrowRight') { el.x = Number(el.x || 0) + step; emit('update'); e.preventDefault() }
  else if (e.key === 'ArrowUp') { el.y = Math.max(0, Number(el.y || 0) - step); emit('update'); e.preventDefault() }
  else if (e.key === 'ArrowDown') { el.y = Number(el.y || 0) + step; emit('update'); e.preventDefault() }
  else if (e.key === 'Delete' || e.key === 'Backspace') {
    if (band?.elements) { band.elements.splice(props.selectedElementIndex, 1); emit('update'); e.preventDefault() }
  } else if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'd') {
    if (band?.elements) {
      const copy = JSON.parse(JSON.stringify(el)); copy.x = Number(el.x || 0) + 5; copy.y = Number(el.y || 0) + 5
      band.elements.push(copy); emit('update'); e.preventDefault()
    }
  }
}
</script>

<style scoped>
.template-editor {
  position: relative;
  height: 100%;
  display: grid;
  grid-template-columns: 24px 1fr;
  grid-template-rows: 24px 1fr;
  background: #f3f3f3;
  overflow: hidden;
}

.ruler-corner {
  grid-area: 1 / 1;
  background: #2d2d30;
  color: #aaa;
  font-size: 9px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-family: 'Cascadia Code', 'Consolas', monospace;
  border-right: 1px solid #1e1e1e;
  border-bottom: 1px solid #1e1e1e;
}

.ruler {
  background-color: #2d2d30;
  overflow: hidden;
  position: relative;
}
.ruler-h { grid-area: 1 / 2; border-bottom: 1px solid #1e1e1e; }
.ruler-v { grid-area: 2 / 1; border-right: 1px solid #1e1e1e; }

.canvas-scroll {
  grid-area: 2 / 2;
  overflow: auto;
  padding: 24px;
}

.canvas-wrap {
  margin: 0 auto;
}

.canvas {
  position: relative;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.15);
  background: white;
}

.band {
  position: absolute;
  left: 0;
  right: 0;
  border-bottom: 1px dashed #ddd;
  cursor: pointer;
}

.band:hover {
  outline: 1px solid #4a9eff;
  outline-offset: -1px;
}

.band.active {
  outline: 1px solid #2563eb;
  outline-offset: -1px;
}

.band-label {
  position: absolute;
  top: -14px;
  left: 0;
  font-size: 10px;
  color: #888;
  background: #f3f3f3;
  padding: 0 4px;
  font-family: 'Cascadia Code', 'Consolas', monospace;
  user-select: none;
}

.band.active .band-label {
  color: #2563eb;
  font-weight: 600;
}

.element {
  position: absolute;
  border: 1px solid transparent;
  cursor: move;
  user-select: none;
  overflow: hidden;
}

.element:hover {
  border-color: #4a9eff;
}

.element.selected {
  border: 1px solid #2563eb;
  background: rgba(37, 99, 235, 0.04);
}

.element.locked {
  cursor: not-allowed;
  border-style: dashed;
}

.lock-badge {
  position: absolute;
  top: -8px;
  right: -8px;
  font-size: 10px;
  pointer-events: none;
}

.group-badge {
  position: absolute;
  top: -8px;
  left: -6px;
  font-size: 9px;
  color: #fff;
  background: #7c3aed;
  border-radius: 3px;
  padding: 0 3px;
  pointer-events: none;
}

.el-text {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  white-space: pre-wrap;
  word-break: break-all;
  padding: 0 2px;
  box-sizing: border-box;
  line-height: 1.2;
}

.el-line {
  width: 100%;
  position: absolute;
  left: 0;
  right: 0;
}

.el-shape {
  width: 100%;
  height: 100%;
  box-sizing: border-box;
}

.el-ph {
  width: 100%;
  height: 100%;
  box-sizing: border-box;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 11px;
  color: #666;
  border: 1px dashed #bbb;
  background: #fbfbfb;
  overflow: hidden;
  white-space: nowrap;
}
.el-image { background: #f0f4ff; color: #4a6fb5; }
.el-barcode { background: #fff; letter-spacing: 2px; }
.el-table { background: #f7fff7; }
.el-chart { background: #fffaf0; color: #b58a4a; }
.el-subreport { background: #faf0ff; color: #8a4ab5; }
.el-render { width: 100%; height: 100%; display: block; pointer-events: none; }
.real-table { width: 100%; height: 100%; border-collapse: collapse; table-layout: fixed; }
.real-table td { border: 1px solid #999; }

.el-other {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  color: #888;
  background: #fafafa;
}

.ctx-menu {
  position: fixed;
  z-index: 999;
  background: #fff;
  border: 1px solid #c8c8c8;
  border-radius: 4px;
  box-shadow: 0 4px 14px rgba(0, 0, 0, 0.18);
  padding: 4px 0;
  min-width: 130px;
  user-select: none;
}

.ctx-item {
  display: block;
  width: 100%;
  text-align: left;
  padding: 5px 14px;
  font-size: 12px;
  background: transparent;
  border: none;
  cursor: pointer;
  color: #333;
  font-family: inherit;
}

.ctx-item:hover { background: #0e639c; color: #fff; }
.ctx-sep { height: 1px; background: #e0e0e0; margin: 3px 0; }

.snap-guide {
  position: absolute;
  background: #f59e0b;
  pointer-events: none;
  z-index: 20;
}
.snap-guide.v { top: 0; bottom: 0; width: 1px; }
.snap-guide.h { left: 0; right: 0; height: 1px; }

.marquee {
  position: absolute;
  border: 1px dashed #2563eb;
  background: rgba(37, 99, 235, 0.1);
  pointer-events: none;
  z-index: 10;
}

.handle {
  position: absolute;
  width: 8px;
  height: 8px;
  background: #2563eb;
  border: 1px solid #fff;
  box-sizing: border-box;
}

.handle.nw { top: -4px; left: -4px; cursor: nwse-resize; }
.handle.n  { top: -4px; left: 50%; margin-left: -4px; cursor: ns-resize; }
.handle.ne { top: -4px; right: -4px; cursor: nesw-resize; }
.handle.e  { top: 50%; right: -4px; margin-top: -4px; cursor: ew-resize; }
.handle.se { bottom: -4px; right: -4px; cursor: nwse-resize; }
.handle.s  { bottom: -4px; left: 50%; margin-left: -4px; cursor: ns-resize; }
.handle.sw { bottom: -4px; left: -4px; cursor: nesw-resize; }
.handle.w  { top: 50%; left: -4px; margin-top: -4px; cursor: ew-resize; }
</style>
