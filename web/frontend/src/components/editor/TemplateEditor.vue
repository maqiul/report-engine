<template>
  <div class="template-editor" ref="rootRef">
    <div class="ruler">{{ Math.round(zoom * 100) }}%</div>

    <div class="canvas-scroll" @scroll="onScroll">
      <div class="canvas-wrap" :style="{ width: pageWidthPx + 'px' }">
        <!-- 画布（A4 比例） -->
        <div
          class="canvas"
          :style="{
            width: pageWidthPx + 'px',
            minHeight: pageHeightPx + 'px',
            background: 'white'
          }"
          @click="onCanvasClick"
        >
          <!-- Band 区域 -->
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
              :class="{ selected: isSelected(Number(bi), Number(ei)) }"
              :style="getElementBox(Number(bi), Number(ei))"
              @mousedown.stop="onElementMouseDown($event, el, Number(bi), Number(ei))"
              @click.stop="selectElement(Number(bi), Number(ei))"
            >
              <div v-if="el.type === 'text'" class="el-text" :style="getTextStyle(Number(bi), Number(ei))">
                {{ el.text || '(空文本)' }}
              </div>
              <div v-else-if="el.type === 'line'" class="el-line" :style="getLineStyle(Number(bi), Number(ei))"></div>
              <div v-else-if="el.type === 'rect'" class="el-rect" :style="getRectStyle(Number(bi), Number(ei))"></div>
              <div v-else class="el-other">{{ el.type }}</div>

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
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'

const props = defineProps<{
  template: any
  selectedBandIndex: number
  selectedElementIndex: number
  zoom: number
}>()

const emit = defineEmits<{
  (e: 'selectBand', i: number): void
  (e: 'selectElement', bandIndex: number, elementIndex: number): void
  (e: 'canvasClick'): void
  (e: 'update'): void
}>()

const rootRef = ref<HTMLElement | null>(null)

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
  return props.selectedBandIndex === bi && props.selectedElementIndex === ei
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

function getRectStyle(bi: number, ei: number): Record<string, string> {
  const s = elementStyles.value[bi]?.[ei]
  if (!s) return {}
  return { borderColor: s.borderColor, borderWidth: s.borderWidth, borderStyle: 'solid' }
}

function selectBand(i: number) {
  emit('selectBand', i)
}

function selectElement(bi: number, ei: number) {
  emit('selectElement', bi, ei)
}

function onCanvasClick() {
  emit('canvasClick')
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
}

const dragState = ref<DragState | null>(null)

function onElementMouseDown(e: MouseEvent, el: any, bi: number, ei: number) {
  selectElement(bi, ei)
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
    origH: el.height || 5
  }
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
    s.el.x = Math.max(0, Number(s.origX) + dx)
    s.el.y = Math.max(0, Number(s.origY) + dy)
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
  document.removeEventListener('mousemove', onMouseMove)
  document.removeEventListener('mouseup', onMouseUp)
}

function onScroll() {
  // 预留：ruler 联动
}
</script>

<style scoped>
.template-editor {
  position: relative;
  height: 100%;
  display: flex;
  flex-direction: column;
  background: #f3f3f3;
  overflow: hidden;
}

.ruler {
  height: 24px;
  background: #2d2d30;
  color: #d4d4d4;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 11px;
  font-family: 'Cascadia Code', 'Consolas', monospace;
  border-bottom: 1px solid #1e1e1e;
}

.canvas-scroll {
  flex: 1;
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

.el-rect {
  width: 100%;
  height: 100%;
  box-sizing: border-box;
}

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
