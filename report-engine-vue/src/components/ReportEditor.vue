<template>
  <div class="report-editor">
    <div class="editor-toolbar">
      <button class="tab" :class="{ active: mode === 'code' }" @click="mode = 'code'">代码</button>
      <button class="tab" :class="{ active: mode === 'visual' }" @click="mode = 'visual'">可视化</button>
      <button class="tab" :class="{ active: mode === 'preview' }" @click="mode = 'preview'">预览</button>
    </div>

    <!-- 代码模式 -->
    <div v-show="mode === 'code'" class="pane code-pane">
      <textarea class="code-editor" v-model="templateJsonText" spellcheck="false"></textarea>
    </div>

    <!-- 可视化模式 -->
    <div v-show="mode === 'visual'" class="pane visual-pane">
      <div class="visual-left">
        <Toolbox
          :template="templateObj"
          @add-element="handleAddElement"
          @add-band="handleAddBand"
        />
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
          :zoom="1"
          @selectBand="(i: number) => (selectedBandIdx = i)"
          @selectElement="(bi: number, ei: number) => { selectedBandIdx = bi; selectedElementIdx = ei }"
          @update="handleTemplateEditorUpdate"
        />
      </div>
      <div class="visual-right">
        <PropertyPanel
          v-if="currentElement"
          :element="currentElement"
          @delete="handleDeleteCurrent"
        />
      </div>
    </div>

    <!-- 预览模式 -->
    <div v-show="mode === 'preview'" class="pane preview-pane">
      <ReportViewer
        :templateJson="templateJsonText"
        :data="data"
        :scale="1"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import ReportViewer from './ReportViewer.vue'
import Toolbox from './editor/Toolbox.vue'
import BandTree from './editor/BandTree.vue'
import PropertyPanel from './editor/PropertyPanel.vue'
import TemplateEditor from './editor/TemplateEditor.vue'

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

const emit = defineEmits<{
  'update:modelValue': [value: string]
}>()

const mode = ref<'code' | 'visual' | 'preview'>('visual')
const templateJsonText = ref(props.modelValue || defaultTemplate())
const data = computed(() => props.data ?? {})

const templateObj = computed<TemplateObj>(() => {
  try {
    return JSON.parse(templateJsonText.value)
  } catch {
    return { version: '1.0', bands: [] }
  }
})

function defaultTemplate(): string {
  return JSON.stringify({
    version: '1.0',
    page: { width: 210, height: 297, unit: 'mm', margin: { top: 10, bottom: 10, left: 10, right: 10 } },
    dataSources: [],
    bands: []
  }, null, 2)
}

function emitChange() {
  emit('update:modelValue', templateJsonText.value)
}

watch(() => props.modelValue, (val) => {
  if (val !== templateJsonText.value) templateJsonText.value = val
})

const selectedBandIdx = ref(0)
const selectedElementIdx = ref(-1)

const currentElement = computed(() => {
  return templateObj.value.bands?.[selectedBandIdx.value]?.elements?.[selectedElementIdx.value] ?? null
})

// ====== Toolbox ======
function handleAddElement(element: any) {
  const t = JSON.parse(JSON.stringify(templateObj.value))
  if (!t.bands[selectedBandIdx.value]) {
    t.bands[selectedBandIdx.value] = { type: 'detail', height: 10, elements: [] }
  }
  if (!t.bands[selectedBandIdx.value].elements) {
    t.bands[selectedBandIdx.value].elements = []
  }
  t.bands[selectedBandIdx.value].elements.push(element)
  selectedElementIdx.value = t.bands[selectedBandIdx.value].elements.length - 1
  templateJsonText.value = JSON.stringify(t, null, 2)
  emitChange()
}

function handleAddBand(band: any) {
  const t = JSON.parse(JSON.stringify(templateObj.value))
  t.bands.push(band)
  selectedBandIdx.value = t.bands.length - 1
  templateJsonText.value = JSON.stringify(t, null, 2)
  emitChange()
}

// ====== BandTree ======
function handleMoveBand(idx: number, delta: number) {
  const t = JSON.parse(JSON.stringify(templateObj.value))
  const j = idx + delta
  if (j < 0 || j >= t.bands.length) return
  ;[t.bands[idx], t.bands[j]] = [t.bands[j], t.bands[idx]]
  selectedBandIdx.value = j
  templateJsonText.value = JSON.stringify(t, null, 2)
  emitChange()
}

function handleDuplicateBand(idx: number) {
  const t = JSON.parse(JSON.stringify(templateObj.value))
  t.bands.splice(idx + 1, 0, JSON.parse(JSON.stringify(t.bands[idx])))
  selectedBandIdx.value = idx + 1
  templateJsonText.value = JSON.stringify(t, null, 2)
  emitChange()
}

function handleRemoveBand(idx: number) {
  const t = JSON.parse(JSON.stringify(templateObj.value))
  t.bands.splice(idx, 1)
  if (selectedBandIdx.value >= t.bands.length) selectedBandIdx.value = Math.max(0, t.bands.length - 1)
  templateJsonText.value = JSON.stringify(t, null, 2)
  emitChange()
}

// ====== TemplateEditor (内部已改 template 对象) ======
function handleTemplateEditorUpdate() {
  // TemplateEditor 内部直接修改了 templateObj 的引用
  // 重新序列化为 text 并 emit
  templateJsonText.value = JSON.stringify(templateObj.value, null, 2)
  emitChange()
}

// ====== PropertyPanel delete ======
function handleDeleteCurrent() {
  const t = JSON.parse(JSON.stringify(templateObj.value))
  if (!t.bands[selectedBandIdx.value]?.elements) return
  t.bands[selectedBandIdx.value].elements.splice(selectedElementIdx.value, 1)
  selectedElementIdx.value = -1
  templateJsonText.value = JSON.stringify(t, null, 2)
  emitChange()
}
</script>

<style scoped>
.report-editor { display: flex; flex-direction: column; height: 100%; font-family: 'Microsoft YaHei', 'PingFang SC', system-ui, sans-serif; }
.editor-toolbar { display: flex; gap: 4px; padding: 8px 12px; background: #1e1e1e; border-bottom: 1px solid #2d2d2d; }
.tab { background: transparent; border: none; color: #969696; padding: 6px 14px; cursor: pointer; font-size: 13px; border-radius: 3px; }
.tab:hover { background: #2d2d2d; color: #cccccc; }
.tab.active { background: #0e639c; color: #fff; }
.pane { flex: 1; overflow: hidden; }
.code-pane { padding: 12px; background: #1e1e1e; }
.code-editor { width: 100%; height: 100%; background: #1e1e1e; color: #d4d4d4; border: 1px solid #2d2d2d; font-family: 'Cascadia Code', 'Consolas', monospace; font-size: 13px; padding: 12px; resize: none; outline: none; box-sizing: border-box; }
.visual-pane { display: grid; grid-template-columns: 220px 1fr 280px; background: #252526; }
.visual-left { background: #252526; border-right: 1px solid #2d2d2d; overflow-y: auto; }
.visual-center { background: #1e1e1e; overflow: auto; }
.visual-right { background: #252526; border-left: 1px solid #2d2d2d; overflow-y: auto; }
.preview-pane { background: #fff; }
</style>