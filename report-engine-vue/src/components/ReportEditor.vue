<template>
  <div class="report-editor">
    <div class="editor-toolbar">
      <button class="tab" :class="{ active: mode === 'code' }" @click="mode = 'code'">代码</button>
      <button class="tab" :class="{ active: mode === 'visual' }" @click="mode = 'visual'">可视化</button>
      <button class="tab" :class="{ active: mode === 'preview' }" @click="mode = 'preview'">预览</button>
      <span class="spacer"></span>
      <button class="tbtn" :disabled="!currentElement" @click="handleCopy" title="复制 (Ctrl+C)">⧉ 复制</button>
      <button class="tbtn" :disabled="!copied" @click="handlePaste" title="粘贴 (Ctrl+V)">📋 粘贴</button>
      <button class="tbtn" :disabled="!currentElement" @click="handleCut" title="剪切 (Ctrl+X)">✂ 剪切</button>
      <span class="sep"></span>
      <button class="tbtn" :disabled="!currentElement" @click="alignLeft" title="左对齐">⇤</button>
      <button class="tbtn" :disabled="!currentElement" @click="alignHCenter" title="水平居中">⇹</button>
      <button class="tbtn" :disabled="!currentElement" @click="alignRight" title="右对齐">⇥</button>
      <button class="tbtn" :disabled="!currentElement" @click="alignTop" title="顶对齐">⤒</button>
      <button class="tbtn" :disabled="!currentElement" @click="alignVCenter" title="垂直居中">⇕</button>
      <button class="tbtn" :disabled="!currentElement" @click="alignBottom" title="底对齐">⤓</button>
      <span class="sep"></span>
      <button class="tbtn" @click="zoomOut" title="缩小">🔍−</button>
      <span class="zoom-lbl">{{ Math.round(zoom * 100) }}%</span>
      <button class="tbtn" @click="zoomIn" title="放大">🔍+</button>
      <button class="tbtn" @click="zoomFit" title="适合">⤢</button>
      <span class="sep"></span>
      <button class="tbtn" :disabled="histIdx <= 0" @click="undo" title="撤销 (Ctrl+Z)">↶ 撤销</button>
      <button class="tbtn" :disabled="histIdx >= history.length - 1" @click="redo" title="重做 (Ctrl+Y)">↷ 重做</button>
    </div>

    <!-- 代码模式 -->
    <div v-show="mode === 'code'" class="pane code-pane">
      <textarea class="code-editor" v-model="templateJsonText" spellcheck="false" @change="commit"></textarea>
    </div>

    <!-- 可视化模式 -->
    <div v-show="mode === 'visual'" class="pane visual-pane">
      <div class="visual-left">
        <Toolbox @add="handleAddElement" @addBand="handleAddBand" />
        <BandTree
          :bands="templateObj.bands || []"
          :selectedBandIndex="selectedBandIdx"
          @select="(i: number) => (selectedBandIdx = i)"
          @moveUp="handleMoveBand($event, -1)"
          @moveDown="handleMoveBand($event, 1)"
          @duplicate="handleDuplicateBand"
          @remove="handleRemoveBand"
        />
      </div>
      <div class="visual-center">
        <TemplateEditor
          :template="templateObj"
          :selectedBandIndex="selectedBandIdx"
          :selectedElementIndex="selectedElementIdx"
          :zoom="zoom"
          @selectBand="(i: number) => (selectedBandIdx = i)"
          @selectElement="(bi: number, ei: number) => { selectedBandIdx = bi; selectedElementIdx = ei }"
          @update="commit"
        />
      </div>
      <div class="visual-right">
        <PropertyPanel
          v-if="currentElement"
          :element="currentElement"
          :dataSources="templateObj.dataSources || []"
          @delete="handleDeleteCurrent"
          @update="commit"
        />
        <div v-else class="no-sel">选择一个元素以编辑属性</div>
      </div>
    </div>

    <!-- 预览模式 -->
    <div v-show="mode === 'preview'" class="pane preview-pane">
      <ReportViewer :templateJson="templateJsonText" :data="data" :scale="1" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed, onMounted, onBeforeUnmount } from 'vue'
import ReportViewer from './ReportViewer.vue'
import Toolbox from './editor/Toolbox.vue'
import BandTree from './editor/BandTree.vue'
import PropertyPanel from './editor/PropertyPanel.vue'
import TemplateEditor from './editor/TemplateEditor.vue'
import { findTypeDef, type ElementKind } from './editor/elementTypes'

interface TemplateObj {
  version: string
  page?: any
  dataSources?: any[]
  bands: any[]
}

const props = defineProps<{
  modelValue: string
  data?: Record<string, any[]>
}>()

const emit = defineEmits<{ 'update:modelValue': [value: string] }>()

const mode = ref<'code' | 'visual' | 'preview'>('visual')
const templateJsonText = ref(props.modelValue || defaultTemplate())
const data = computed(() => props.data ?? {})
const zoom = ref(1)

const templateObj = computed<TemplateObj>(() => {
  try { return JSON.parse(templateJsonText.value) }
  catch { return { version: '1.0', bands: [] } }
})

function defaultTemplate(): string {
  return JSON.stringify({
    version: '1.0',
    page: { width: 210, height: 297, unit: 'mm', margin: { top: 10, bottom: 10, left: 10, right: 10 } },
    dataSources: [],
    bands: []
  }, null, 2)
}

// ====== undo / redo 历史栈 ======
const history = ref<string[]>([])
const histIdx = ref(-1)

function commit() {
  // 把对 templateObj 的引用式修改序列化回文本（PropertyPanel/TemplateEditor 直接改对象引用）
  templateJsonText.value = JSON.stringify(templateObj.value, null, 2)
  emit('update:modelValue', templateJsonText.value)
  pushHistory()
}

function pushHistory() {
  // 截断 redo 分支后压入当前快照
  history.value = history.value.slice(0, histIdx.value + 1)
  history.value.push(templateJsonText.value)
  if (history.value.length > 100) history.value.shift()
  histIdx.value = history.value.length - 1
}

function undo() {
  if (histIdx.value > 0) {
    histIdx.value--
    templateJsonText.value = history.value[histIdx.value]
    emit('update:modelValue', templateJsonText.value)
  }
}

function redo() {
  if (histIdx.value < history.value.length - 1) {
    histIdx.value++
    templateJsonText.value = history.value[histIdx.value]
    emit('update:modelValue', templateJsonText.value)
  }
}

function onKey(e: KeyboardEvent) {
  if (!(e.ctrlKey || e.metaKey)) return
  const tag = (e.target as HTMLElement)?.tagName
  if (tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT') return
  const k = e.key.toLowerCase()
  if (k === 'z' && !e.shiftKey) { e.preventDefault(); undo() }
  else if (k === 'y' || (k === 'z' && e.shiftKey)) { e.preventDefault(); redo() }
  else if (k === 'c') { e.preventDefault(); handleCopy() }
  else if (k === 'v') { e.preventDefault(); handlePaste() }
  else if (k === 'x') { e.preventDefault(); handleCut() }
}

onMounted(() => { pushHistory(); window.addEventListener('keydown', onKey) })
onBeforeUnmount(() => window.removeEventListener('keydown', onKey))

watch(() => props.modelValue, (val) => {
  if (val !== templateJsonText.value) templateJsonText.value = val
})

const selectedBandIdx = ref(0)
const selectedElementIdx = ref(-1)

const currentElement = computed(() =>
  templateObj.value.bands?.[selectedBandIdx.value]?.elements?.[selectedElementIdx.value] ?? null)

// ====== Toolbox：插入元素（工厂造默认属性，type 对齐后端） ======
function handleAddElement(kind: ElementKind) {
  const def = findTypeDef(kind)
  if (!def) return
  const t = JSON.parse(JSON.stringify(templateObj.value))
  if (!t.bands[selectedBandIdx.value]) {
    t.bands[selectedBandIdx.value] = { type: 'detail', height: 10, elements: [] }
  }
  if (!t.bands[selectedBandIdx.value].elements) t.bands[selectedBandIdx.value].elements = []
  t.bands[selectedBandIdx.value].elements.push(def.create())
  selectedElementIdx.value = t.bands[selectedBandIdx.value].elements.length - 1
  templateJsonText.value = JSON.stringify(t, null, 2)
  commit()
}

function handleAddBand(type: string) {
  const t = JSON.parse(JSON.stringify(templateObj.value))
  t.bands.push({ type, height: type === 'detail' ? 10 : 15, elements: [] })
  selectedBandIdx.value = t.bands.length - 1
  templateJsonText.value = JSON.stringify(t, null, 2)
  commit()
}

// ====== BandTree ======
function handleMoveBand(idx: number, delta: number) {
  const t = JSON.parse(JSON.stringify(templateObj.value))
  const j = idx + delta
  if (j < 0 || j >= t.bands.length) return
  ;[t.bands[idx], t.bands[j]] = [t.bands[j], t.bands[idx]]
  selectedBandIdx.value = j
  templateJsonText.value = JSON.stringify(t, null, 2)
  commit()
}

function handleDuplicateBand(idx: number) {
  const t = JSON.parse(JSON.stringify(templateObj.value))
  t.bands.splice(idx + 1, 0, JSON.parse(JSON.stringify(t.bands[idx])))
  selectedBandIdx.value = idx + 1
  templateJsonText.value = JSON.stringify(t, null, 2)
  commit()
}

function handleRemoveBand(idx: number) {
  const t = JSON.parse(JSON.stringify(templateObj.value))
  t.bands.splice(idx, 1)
  if (selectedBandIdx.value >= t.bands.length) selectedBandIdx.value = Math.max(0, t.bands.length - 1)
  selectedElementIdx.value = -1
  templateJsonText.value = JSON.stringify(t, null, 2)
  commit()
}

function handleDeleteCurrent() {
  const t = JSON.parse(JSON.stringify(templateObj.value))
  if (!t.bands[selectedBandIdx.value]?.elements) return
  t.bands[selectedBandIdx.value].elements.splice(selectedElementIdx.value, 1)
  selectedElementIdx.value = -1
  templateJsonText.value = JSON.stringify(t, null, 2)
  commit()
}

// ====== 复制 / 粘贴 / 剪切 ======
const copied = ref<any>(null)
function handleCopy() {
  if (!currentElement.value) return
  copied.value = JSON.parse(JSON.stringify(currentElement.value))
}
function handlePaste() {
  if (!copied.value) return
  const t = JSON.parse(JSON.stringify(templateObj.value))
  if (!t.bands[selectedBandIdx.value]) t.bands[selectedBandIdx.value] = { type: 'detail', height: 10, elements: [] }
  if (!t.bands[selectedBandIdx.value].elements) t.bands[selectedBandIdx.value].elements = []
  const el = JSON.parse(JSON.stringify(copied.value))
  el.x = Number(el.x || 0) + 5
  el.y = Number(el.y || 0) + 3
  t.bands[selectedBandIdx.value].elements.push(el)
  selectedElementIdx.value = t.bands[selectedBandIdx.value].elements.length - 1
  templateJsonText.value = JSON.stringify(t, null, 2)
  commit()
}
function handleCut() { handleCopy(); handleDeleteCurrent() }

// ====== 画布缩放 ======
function zoomIn() { zoom.value = Math.min(3, Math.round((zoom.value + 0.25) * 100) / 100) }
function zoomOut() { zoom.value = Math.max(0.5, Math.round((zoom.value - 0.25) * 100) / 100) }
function zoomFit() { zoom.value = 1 }

// ====== 对齐（选中元素对齐到页面宽 / 带区高） ======
function mutateCurrent(fn: (el: any, pageW: number, bandH: number) => void) {
  if (!currentElement.value) return
  const t = JSON.parse(JSON.stringify(templateObj.value))
  const el = t.bands[selectedBandIdx.value]?.elements?.[selectedElementIdx.value]
  if (!el) return
  const pageW = Number(t.page?.width || 210)
  const bandH = Number(t.bands[selectedBandIdx.value]?.height || 0)
  fn(el, pageW, bandH)
  templateJsonText.value = JSON.stringify(t, null, 2)
  commit()
}
const alignLeft = () => mutateCurrent((el) => { el.x = 0 })
const alignHCenter = () => mutateCurrent((el, w) => { el.x = (w - Number(el.width || 0)) / 2 })
const alignRight = () => mutateCurrent((el, w) => { el.x = w - Number(el.width || 0) })
const alignTop = () => mutateCurrent((el) => { el.y = 0 })
const alignVCenter = () => mutateCurrent((el, _w, h) => { el.y = (h - Number(el.height || 0)) / 2 })
const alignBottom = () => mutateCurrent((el, _w, h) => { el.y = h - Number(el.height || 0) })
</script>

<style scoped>
.report-editor { display: flex; flex-direction: column; height: 100%; font-family: 'Microsoft YaHei', 'PingFang SC', system-ui, sans-serif; }
.editor-toolbar { display: flex; gap: 4px; padding: 8px 12px; background: #1e1e1e; border-bottom: 1px solid #2d2d2d; align-items: center; }
.tab { background: transparent; border: none; color: #969696; padding: 6px 14px; cursor: pointer; font-size: 13px; border-radius: 3px; }
.tab:hover { background: #2d2d2d; color: #cccccc; }
.tab.active { background: #0e639c; color: #fff; }
.spacer { flex: 1; }
.sep { width: 1px; height: 18px; background: #3c3c3c; margin: 0 4px; }
.zoom-lbl { color: #cccccc; font-size: 12px; min-width: 42px; text-align: center; }
.tbtn { background: #2d2d2d; border: 1px solid #3c3c3c; color: #cccccc; padding: 4px 10px; font-size: 12px; border-radius: 3px; cursor: pointer; }
.tbtn:hover:not(:disabled) { background: #3c3c3c; }
.tbtn:disabled { opacity: 0.4; cursor: not-allowed; }
.pane { flex: 1; overflow: hidden; }
.code-pane { padding: 12px; background: #1e1e1e; }
.code-editor { width: 100%; height: 100%; background: #1e1e1e; color: #d4d4d4; border: 1px solid #2d2d2d; font-family: 'Cascadia Code', 'Consolas', monospace; font-size: 13px; padding: 12px; resize: none; outline: none; box-sizing: border-box; }
.visual-pane { display: grid; grid-template-columns: 220px 1fr 300px; background: #252526; }
.visual-left { background: #252526; border-right: 1px solid #2d2d2d; overflow-y: auto; }
.visual-center { background: #1e1e1e; overflow: auto; }
.visual-right { background: #252526; border-left: 1px solid #2d2d2d; overflow-y: auto; }
.no-sel { color: #6a6a6a; font-size: 12px; padding: 24px 12px; text-align: center; }
.preview-pane { background: #fff; }
</style>
