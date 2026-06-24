<template>
  <Login v-if="!auth.isLoggedIn" @success="onLoginSuccess" />
  <div v-else class="app">
    <header class="topbar">
      <div class="brand">
        <span class="logo">RE</span>
        <span class="name">ReportEngine</span>
        <span class="version">v0.4.0-auth</span>
      </div>
      <nav class="topnav">
        <span class="status">Java 后端 : 5000</span>
        <div class="mode-tabs">
          <button
            class="mode-tab"
            :class="{ active: mode === 'code' }"
            @click="mode = 'code'"
          >代码</button>
          <button
            class="mode-tab"
            :class="{ active: mode === 'visual' }"
            @click="mode = 'visual'"
          >可视化</button>
          <button
            class="mode-tab"
            :class="{ active: mode === 'preview' }"
            @click="mode = 'preview'"
          >预览</button>
        </div>
        <div v-if="auth.isLoggedIn" class="user-info">
          <span class="username">{{ auth.username }}</span>
          <span class="role-badge" :class="{ admin: auth.isAdmin }">
            {{ auth.role }}
          </span>
          <button class="btn-logout" @click="onLogout">登出</button>
        </div>
      </nav>
    </header>

    <main class="layout">
      <!-- ===== 代码模式：JSON 编辑 ===== -->
      <template v-if="mode === 'code'">
        <aside class="panel">
          <div class="panel-section">
            <div class="section-title">模板</div>
            <textarea
              v-model="templateJson"
              class="code-input"
              rows="22"
              spellcheck="false"
            ></textarea>
          </div>
          <div class="panel-section">
            <div class="section-title">数据源</div>
            <textarea
              v-model="dataJson"
              class="code-input"
              rows="10"
              spellcheck="false"
            ></textarea>
          </div>
          <div class="panel-section">
            <div class="section-title">缩放 <span class="muted">{{ scale * 100 }}%</span></div>
            <input type="range" v-model.number="scale" min="0.5" max="2" step="0.05" class="slider" />
          </div>
          <div class="panel-section">
            <div class="section-title">示例</div>
            <div class="button-group">
              <button class="btn" @click="loadSampleTemplate">简单报表</button>
              <button class="btn" @click="loadSalesTemplate">销售订单</button>
            </div>
          </div>
        </aside>
        <section class="stage">
          <ReportViewer
            ref="viewerRef"
            :template-json="templateJson"
            :data="parsedData"
            :scale="scale"
          />
        </section>
      </template>

      <!-- ===== 可视化模式：拖拽编辑器 ===== -->
      <template v-else-if="mode === 'visual'">
        <aside class="panel">
          <Toolbox @add="addElement" @add-band="addBand" />
          <BandTree
            :bands="parsedTemplate.bands || []"
            :selected-band-index="selectedBandIndex"
            @select="onSelectBand"
            @move-up="moveBandUp"
            @move-down="moveBandDown"
            @duplicate="duplicateBand"
            @remove="removeBand"
          />
          <PropertyPanel
            :element="selectedElement"
            @delete="deleteElement"
          />
        </aside>
        <section class="stage">
          <TemplateEditor
            :template="parsedTemplate"
            :selected-band-index="selectedBandIndex"
            :selected-element-index="selectedElementIndex"
            :zoom="scale"
            @select-band="onSelectBand"
            @select-element="onSelectElement"
            @canvas-click="onCanvasClick"
            @update="syncTemplateJson"
          />
        </section>
      </template>

      <!-- ===== 预览模式：ReportViewer ===== -->
      <template v-else>
        <aside class="panel">
          <div class="panel-section">
            <div class="section-title">数据源</div>
            <textarea
              v-model="dataJson"
              class="code-input"
              rows="18"
              spellcheck="false"
            ></textarea>
          </div>
          <div class="panel-section">
            <div class="section-title">缩放 <span class="muted">{{ scale * 100 }}%</span></div>
            <input type="range" v-model.number="scale" min="0.5" max="2" step="0.05" class="slider" />
          </div>
          <div class="panel-section">
            <div class="section-title">导出</div>
            <div class="button-group">
              <button class="btn" @click="exportAs('pdf')">PDF</button>
              <button class="btn" @click="exportAs('excel')">Excel</button>
            </div>
          </div>
        </aside>
        <section class="stage">
          <ReportViewer
            ref="viewerRef"
            :template-json="templateJson"
            :data="parsedData"
            :scale="scale"
          />
        </section>
      </template>
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import ReportViewer from './components/ReportViewer.vue'
import Login from './components/Login.vue'
import Toolbox from './components/editor/Toolbox.vue'
import BandTree from './components/editor/BandTree.vue'
import PropertyPanel from './components/editor/PropertyPanel.vue'
import TemplateEditor from './components/editor/TemplateEditor.vue'
import { exportPdf, exportExcel } from './api/report'
import { useAuthStore } from './stores/auth'

const auth = useAuthStore()
function onLoginSuccess() { /* App.vue 自动从 v-if 切到主界面 */ }
function onLogout() { auth.logout() }

type Mode = 'code' | 'visual' | 'preview'

const mode = ref<Mode>('code')

const viewerRef = ref<InstanceType<typeof ReportViewer> | null>(null)
const scale = ref(0.8)

// ===== 模板 JSON =====
const defaultTemplate = `{
  "version": "1.0",
  "page": {
    "width": 210,
    "height": 297,
    "margin": { "top": 15, "bottom": 15, "left": 10, "right": 10 }
  },
  "dataSources": [{ "name": "orders" }],
  "bands": [
    {
      "type": "title",
      "height": 30,
      "elements": [
        { "type": "text", "text": "报表标题", "x": 10, "y": 10, "width": 190, "height": 15, "font": { "size": 18, "bold": true }, "alignment": "center" }
      ]
    },
    {
      "type": "pageHeader",
      "height": 10,
      "elements": [
        { "type": "text", "text": "列 1", "x": 10, "y": 0, "width": 60, "height": 8, "font": { "bold": true } }
      ]
    },
    {
      "type": "detail",
      "height": 10,
      "dataSource": "orders",
      "elements": [
        { "type": "text", "text": "{{currentRow.name}}", "x": 10, "y": 0, "width": 60, "height": 8 }
      ]
    }
  ]
}`

const templateJson = ref(defaultTemplate)
const dataJson = ref(`{
  "orders": [
    { "name": "张三", "amount": 100 },
    { "name": "李四", "amount": 200 }
  ]
}`)

const parsedData = computed(() => {
  try {
    return JSON.parse(dataJson.value || '{}')
  } catch {
    return {}
  }
})

// 可视化模式共享同一份 JSON
const parsedTemplate = computed({
  get: () => {
    try {
      return JSON.parse(templateJson.value || '{}')
    } catch {
      return { bands: [] }
    }
  },
  set: (v) => {
    templateJson.value = JSON.stringify(v, null, 2)
  }
})

watch(parsedTemplate, (val) => {
  if (!val.bands) val.bands = []
  if (!val.page) {
    val.page = { width: 210, height: 297, margin: { top: 15, bottom: 15, left: 10, right: 10 } }
  }
}, { deep: true })

// ===== 选中状态 =====
const selectedBandIndex = ref<number>(-1)
const selectedElementIndex = ref<number>(-1)

const selectedElement = computed(() => {
  if (selectedBandIndex.value < 0) return null
  const band = parsedTemplate.value.bands?.[selectedBandIndex.value]
  if (!band) return null
  if (selectedElementIndex.value < 0) return null
  return band.elements?.[selectedElementIndex.value] || null
})

function onSelectBand(i: number) {
  selectedBandIndex.value = i
  selectedElementIndex.value = -1
}

function onSelectElement(bi: number, ei: number) {
  selectedBandIndex.value = bi
  selectedElementIndex.value = ei
}

function onCanvasClick() {
  selectedBandIndex.value = -1
  selectedElementIndex.value = -1
}

// ===== 添加操作 =====
function addBand(type: string) {
  if (!parsedTemplate.value.bands) parsedTemplate.value.bands = []
  const newBand: any = {
    type,
    height: type === 'detail' ? 10 : 20,
    elements: []
  }
  if (type === 'detail') newBand.dataSource = 'orders'
  parsedTemplate.value.bands.push(newBand)
  selectedBandIndex.value = parsedTemplate.value.bands.length - 1
  selectedElementIndex.value = -1
  syncTemplateJson()
}

function addElement(type: 'text' | 'line' | 'rect' | 'barcode') {
  if (selectedBandIndex.value < 0) {
    addBand('title')
  }
  const band = parsedTemplate.value.bands[selectedBandIndex.value]
  if (!band.elements) band.elements = []
  const defaults: any = {
    text: { type: 'text', text: '新文本', x: 20, y: 2, width: 50, height: 8, font: { size: 10 } },
    line: { type: 'line', x: 20, y: 5, width: 100, height: 0, lineWidth: 0.5, borderColor: '#000000' },
    rect: { type: 'rect', x: 20, y: 2, width: 60, height: 8, lineWidth: 0.5, borderColor: '#000000' },
    barcode: { type: 'barcode', x: 20, y: 2, width: 60, height: 12, dataField: '' }
  }
  const el = JSON.parse(JSON.stringify(defaults[type]))
  band.elements.push(el)
  selectedElementIndex.value = band.elements.length - 1
  syncTemplateJson()
}

function deleteElement() {
  if (selectedBandIndex.value < 0 || selectedElementIndex.value < 0) return
  const band = parsedTemplate.value.bands[selectedBandIndex.value]
  band.elements.splice(selectedElementIndex.value, 1)
  selectedElementIndex.value = -1
  syncTemplateJson()
}

// ===== Band 操作 =====
function moveBandUp(i: number) {
  if (i <= 0) return
  const arr = parsedTemplate.value.bands
  ;[arr[i - 1], arr[i]] = [arr[i], arr[i - 1]]
  selectedBandIndex.value = i - 1
  syncTemplateJson()
}

function moveBandDown(i: number) {
  const arr = parsedTemplate.value.bands
  if (i >= arr.length - 1) return
  ;[arr[i + 1], arr[i]] = [arr[i], arr[i + 1]]
  selectedBandIndex.value = i + 1
  syncTemplateJson()
}

function duplicateBand(i: number) {
  const arr = parsedTemplate.value.bands
  const copy = JSON.parse(JSON.stringify(arr[i]))
  copy.elements = (copy.elements || []).map((e: any) => ({ ...e, y: (e.y || 0) + 5 }))
  arr.splice(i + 1, 0, copy)
  selectedBandIndex.value = i + 1
  syncTemplateJson()
}

function removeBand(i: number) {
  parsedTemplate.value.bands.splice(i, 1)
  if (selectedBandIndex.value === i) selectedBandIndex.value = -1
  else if (selectedBandIndex.value > i) selectedBandIndex.value--
  syncTemplateJson()
}

function syncTemplateJson() {
  // 触发 parsedTemplate.setter
  templateJson.value = JSON.stringify(parsedTemplate.value, null, 2)
}

// ===== 示例加载 =====
function loadSampleTemplate() {
  templateJson.value = defaultTemplate
  dataJson.value = `{
  "orders": [
    { "name": "示例 1", "amount": 100 },
    { "name": "示例 2", "amount": 200 }
  ]
}`
}

function loadSalesTemplate() {
  templateJson.value = `{
  "version": "1.0",
  "page": { "width": 210, "height": 297, "margin": { "top": 15, "bottom": 15, "left": 10, "right": 10 } },
  "dataSources": [{ "name": "orders" }],
  "bands": [
    {
      "type": "title",
      "height": 30,
      "elements": [
        { "type": "text", "text": "销售订单报表", "x": 10, "y": 10, "width": 190, "height": 15, "font": { "size": 20, "bold": true }, "alignment": "center" }
      ]
    },
    {
      "type": "pageHeader",
      "height": 10,
      "elements": [
        { "type": "text", "text": "订单号", "x": 10, "y": 0, "width": 40, "height": 8, "font": { "bold": true } },
        { "type": "text", "text": "客户", "x": 55, "y": 0, "width": 50, "height": 8, "font": { "bold": true } },
        { "type": "text", "text": "金额", "x": 130, "y": 0, "width": 60, "height": 8, "font": { "bold": true }, "alignment": "right" }
      ]
    },
    {
      "type": "detail",
      "height": 10,
      "dataSource": "orders",
      "elements": [
        { "type": "text", "text": "{{currentRow.id}}", "x": 10, "y": 0, "width": 40, "height": 8 },
        { "type": "text", "text": "{{currentRow.cust}}", "x": 55, "y": 0, "width": 50, "height": 8 },
        { "type": "text", "text": "{{currentRow.amt}}", "x": 130, "y": 0, "width": 60, "height": 8, "alignment": "right" }
      ]
    }
  ]
}`
  dataJson.value = `{
  "orders": [
    { "id": "001", "cust": "张三", "amt": 100 },
    { "id": "002", "cust": "李四", "amt": 200 }
  ]
}`
}

// ===== 导出 =====
async function exportAs(type: 'pdf' | 'excel') {
  const req = {
    templateJson: templateJson.value,
    data: parsedData.value
  }
  try {
    const blob = type === 'pdf' ? await exportPdf(req) : await exportExcel(req)
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `report.${type === 'pdf' ? 'pdf' : 'xlsx'}`
    a.click()
    URL.revokeObjectURL(url)
  } catch (e: any) {
    alert('导出失败：' + e.message)
  }
}

onMounted(() => {
  // 初始模式由 mode 控制
})
</script>

<style>
:root {
  --bg: #f3f3f3;
  --bg-elev: #e7e7e7;
  --border: #e0e0e0;
  --border-strong: #c8c8c8;
  --fg: #1f1f1f;
  --fg-muted: #6b6b6b;
  --accent: #2563eb;
  --accent-soft: #e7f0ff;
}

* { box-sizing: border-box; }
html, body, #app { margin: 0; padding: 0; height: 100%; font-family: 'Segoe UI', -apple-system, BlinkMacSystemFont, sans-serif; font-size: 13px; color: var(--fg); background: var(--bg); }

.app { display: flex; flex-direction: column; height: 100%; }

.topbar {
  height: 44px;
  background: #2d2d30;
  color: #d4d4d4;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 16px;
  border-bottom: 1px solid #1e1e1e;
  flex-shrink: 0;
}

.brand { display: flex; align-items: center; gap: 10px; }
.logo {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 26px; height: 26px;
  background: #0e639c;
  color: #fff;
  font-size: 11px;
  font-weight: 700;
  border-radius: 4px;
  font-family: 'Cascadia Code', 'Consolas', monospace;
}
.name { font-size: 14px; font-weight: 600; color: #fff; }
.version { font-size: 11px; color: #888; font-family: 'Cascadia Code', 'Consolas', monospace; }

.topnav { display: flex; align-items: center; gap: 16px; }
.status { font-size: 12px; color: #999; font-family: 'Cascadia Code', 'Consolas', monospace; }

.user-info { display: flex; align-items: center; gap: 8px; padding-left: 12px; border-left: 1px solid #333; }
.user-info .username { font-size: 12px; color: #d4d4d4; }
.user-info .role-badge {
  font-size: 10px;
  padding: 1px 6px;
  border-radius: 2px;
  background: #2d2d30;
  color: #888;
  font-family: 'Cascadia Code', 'Consolas', monospace;
}
.user-info .role-badge.admin { background: #5a1d1d; color: #f48771; }
.btn-logout {
  font-size: 11px;
  padding: 3px 10px;
  background: transparent;
  border: 1px solid #3c3c3c;
  color: #888;
  border-radius: 3px;
  cursor: pointer;
  font-family: inherit;
}
.btn-logout:hover { background: #2d2d30; color: #d4d4d4; border-color: #555; }

.mode-tabs {
  display: flex;
  background: #1e1e1e;
  border-radius: 4px;
  padding: 2px;
  gap: 2px;
}

.mode-tab {
  padding: 4px 12px;
  font-size: 12px;
  background: transparent;
  color: #aaa;
  border: none;
  border-radius: 3px;
  cursor: pointer;
  font-family: inherit;
}

.mode-tab:hover { color: #fff; }

.mode-tab.active {
  background: #0e639c;
  color: #fff;
}

.layout {
  flex: 1;
  display: flex;
  min-height: 0;
}

.panel {
  width: 280px;
  background: #fafafa;
  border-right: 1px solid var(--border);
  overflow-y: auto;
  flex-shrink: 0;
}

.panel-section { padding: 12px; border-bottom: 1px solid var(--border); }

.section-title {
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--fg-muted);
  margin-bottom: 8px;
}

.muted { color: var(--fg-muted); font-weight: 400; }

.code-input {
  width: 100%;
  font-family: 'Cascadia Code', 'Consolas', monospace;
  font-size: 11.5px;
  border: 1px solid var(--border-strong);
  border-radius: 4px;
  padding: 6px 8px;
  background: #fff;
  color: var(--fg);
  resize: vertical;
}

.slider { width: 100%; }

.button-group { display: flex; gap: 6px; flex-wrap: wrap; }
.btn {
  padding: 5px 12px;
  font-size: 12px;
  background: #fff;
  border: 1px solid var(--border-strong);
  border-radius: 4px;
  cursor: pointer;
  color: var(--fg);
  font-family: inherit;
}
.btn:hover { background: var(--bg-elev); }

.stage {
  flex: 1;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  min-width: 0;
}
</style>
