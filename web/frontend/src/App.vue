<template>
  <div class="app">
    <header class="topbar">
      <div class="brand">
        <span class="logo">RE</span>
        <span class="name">ReportEngine</span>
        <span class="version">v0.2.0-web</span>
      </div>
      <nav class="topnav">
        <span class="status">Java 后端 : 5000</span>
      </nav>
    </header>

    <main class="layout">
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
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import ReportViewer from './components/ReportViewer.vue'

const viewerRef = ref<InstanceType<typeof ReportViewer> | null>(null)
const scale = ref(0.8)

// 默认模板
const templateJson = ref(`{
  "version": "1.0",
  "page": {
    "width": 210,
    "height": 297,
    "margin": { "top": 15, "bottom": 15, "left": 10, "right": 10 }
  },
  "dataSources": [{ "name": "orders" }],
  "bands": [
    {
      "type": "reportHeader",
      "height": 30,
      "elements": [
        {
          "type": "text",
          "text": "销售订单报表",
          "x": 10, "y": 10, "width": 190, "height": 15,
          "font": { "family": "Microsoft YaHei", "size": 20, "bold": true },
          "alignment": "center"
        }
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
      "height": 8,
      "dataSource": "orders",
      "elements": [
        { "type": "text", "text": "{{currentRow.orderNo}}", "x": 10, "y": 0, "width": 40, "height": 6 },
        { "type": "text", "text": "{{currentRow.customer}}", "x": 55, "y": 0, "width": 50, "height": 6 },
        { "type": "text", "text": "{{currentRow.amount}}", "x": 130, "y": 0, "width": 60, "height": 6, "alignment": "right" }
      ]
    },
    {
      "type": "pageFooter",
      "height": 10,
      "elements": [
        { "type": "text", "text": "第 {{page}} / {{totalPages}} 页", "x": 130, "y": 0, "width": 60, "height": 6, "alignment": "right" }
      ]
    }
  ]
}`)

// 默认数据
const dataJson = ref(JSON.stringify({
  orders: [
    { orderNo: 'SO-001', customer: '张三', amount: 1234.56 },
    { orderNo: 'SO-002', customer: '李四', amount: 789.00 },
    { orderNo: 'SO-003', customer: '王五', amount: 2345.67 },
    { orderNo: 'SO-004', customer: '赵六', amount: 567.89 },
    { orderNo: 'SO-005', customer: '钱七', amount: 3456.78 }
  ]
}, null, 2))

// 解析数据
const parsedData = computed(() => {
  try {
    return JSON.parse(dataJson.value)
  } catch {
    return {}
  }
})

// 加载简单示例
function loadSampleTemplate() {
  templateJson.value = `{
  "version": "1.0",
  "page": { "width": 210, "height": 297 },
  "dataSources": [{ "name": "items" }],
  "bands": [
    {
      "type": "reportHeader",
      "height": 20,
      "elements": [{
        "type": "text",
        "text": "简单报表",
        "x": 80, "y": 5, "width": 50, "height": 10,
        "font": { "size": 16, "bold": true }
      }]
    },
    {
      "type": "detail",
      "height": 8,
      "dataSource": "items",
      "elements": [{
        "type": "text",
        "text": "{{currentRow.name}} - {{currentRow.value}}",
        "x": 20, "y": 0, "width": 170, "height": 6
      }]
    }
  ]
}`
  dataJson.value = JSON.stringify({
    items: [
      { name: '项目A', value: 100 },
      { name: '项目B', value: 200 },
      { name: '项目C', value: 300 }
    ]
  }, null, 2)
}

// 加载销售报表
function loadSalesTemplate() {
  templateJson.value = `{
  "version": "1.0",
  "page": {
    "width": 210,
    "height": 297,
    "margin": { "top": 15, "bottom": 15, "left": 10, "right": 10 }
  },
  "dataSources": [{ "name": "orders" }],
  "bands": [
    {
      "type": "reportHeader",
      "height": 30,
      "elements": [{
        "type": "text",
        "text": "销售订单报表",
        "x": 10, "y": 10, "width": 190, "height": 15,
        "font": { "size": 20, "bold": true },
        "alignment": "center"
      }]
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
      "height": 8,
      "dataSource": "orders",
      "elements": [
        { "type": "text", "text": "{{currentRow.orderNo}}", "x": 10, "y": 0, "width": 40, "height": 6 },
        { "type": "text", "text": "{{currentRow.customer}}", "x": 55, "y": 0, "width": 50, "height": 6 },
        { "type": "text", "text": "{{currentRow.amount}}", "x": 130, "y": 0, "width": 60, "height": 6, "alignment": "right" }
      ]
    },
    {
      "type": "reportFooter",
      "height": 15,
      "elements": [{
        "type": "text",
        "text": "合计: {{sum.orders.amount}}",
        "x": 130, "y": 5, "width": 60, "height": 8,
        "font": { "bold": true },
        "alignment": "right"
      }]
    }
  ]
}`
  dataJson.value = JSON.stringify({
    orders: [
      { orderNo: 'SO-2026-001', customer: '北京科技有限公司', amount: 12500.00 },
      { orderNo: 'SO-2026-002', customer: '上海贸易公司', amount: 8900.50 },
      { orderNo: 'SO-2026-003', customer: '深圳电子科技', amount: 23400.00 },
      { orderNo: 'SO-2026-004', customer: '广州商贸集团', amount: 5670.80 },
      { orderNo: 'SO-2026-005', customer: '杭州网络技术', amount: 15890.00 }
    ]
  }, null, 2)
}
</script>

<style>
:root {
  --bg: #fafafa;
  --bg-panel: #ffffff;
  --bg-elev: #f5f5f5;
  --bg-code: #1e1e1e;
  --fg: #1a1a1a;
  --fg-muted: #6b7280;
  --fg-subtle: #9ca3af;
  --border: #e5e7eb;
  --border-strong: #d1d5db;
  --accent: #2563eb;
  --accent-hover: #1d4ed8;
  --danger: #dc2626;
  --mono: ui-monospace, SFMono-Regular, "SF Mono", Menlo, Consolas, monospace;
  --sans: -apple-system, BlinkMacSystemFont, "Segoe UI", "PingFang SC", "Microsoft YaHei", sans-serif;
}

* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

html, body {
  height: 100%;
  font-family: var(--sans);
  color: var(--fg);
  background: var(--bg);
  font-size: 14px;
  line-height: 1.5;
  -webkit-font-smoothing: antialiased;
}

.app {
  display: flex;
  flex-direction: column;
  height: 100vh;
}

/* 顶部栏 */
.topbar {
  height: 48px;
  background: #1a1a1a;
  color: #e5e5e5;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 16px;
  border-bottom: 1px solid #000;
  flex-shrink: 0;
}

.brand {
  display: flex;
  align-items: center;
  gap: 10px;
}

.logo {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  background: var(--accent);
  color: #fff;
  font-weight: 700;
  font-size: 12px;
  letter-spacing: -0.5px;
  border-radius: 4px;
}

.name {
  font-weight: 600;
  font-size: 15px;
  color: #fff;
}

.version {
  font-family: var(--mono);
  font-size: 11px;
  color: #9ca3af;
  padding: 2px 6px;
  background: #2a2a2a;
  border-radius: 3px;
}

.topnav {
  display: flex;
  align-items: center;
  gap: 16px;
}

.status {
  font-family: var(--mono);
  font-size: 12px;
  color: #9ca3af;
}

.status::before {
  content: '';
  display: inline-block;
  width: 6px;
  height: 6px;
  background: #22c55e;
  border-radius: 50%;
  margin-right: 6px;
  vertical-align: middle;
}

/* 主布局 */
.layout {
  display: flex;
  flex: 1;
  overflow: hidden;
}

/* 左侧面板 */
.panel {
  width: 360px;
  background: var(--bg-panel);
  border-right: 1px solid var(--border);
  overflow-y: auto;
  flex-shrink: 0;
}

.panel-section {
  padding: 16px;
  border-bottom: 1px solid var(--border);
}

.section-title {
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--fg-muted);
  margin-bottom: 10px;
}

.muted {
  color: var(--fg-subtle);
  font-weight: 400;
  text-transform: none;
  letter-spacing: 0;
}

.code-input {
  width: 100%;
  padding: 8px 10px;
  background: #fafafa;
  border: 1px solid var(--border-strong);
  border-radius: 4px;
  font-family: var(--mono);
  font-size: 12px;
  color: var(--fg);
  resize: vertical;
  line-height: 1.5;
}

.code-input:focus {
  outline: none;
  border-color: var(--accent);
  background: #fff;
}

.slider {
  width: 100%;
  height: 4px;
  appearance: none;
  background: var(--border-strong);
  border-radius: 2px;
  outline: none;
}

.slider::-webkit-slider-thumb {
  appearance: none;
  width: 14px;
  height: 14px;
  background: var(--accent);
  border-radius: 50%;
  cursor: pointer;
}

.button-group {
  display: flex;
  gap: 8px;
}

.btn {
  flex: 1;
  padding: 6px 12px;
  background: #fff;
  color: var(--fg);
  border: 1px solid var(--border-strong);
  border-radius: 4px;
  font-size: 13px;
  cursor: pointer;
  transition: all 0.1s;
}

.btn:hover {
  background: var(--bg-elev);
  border-color: var(--fg-muted);
}

.btn:active {
  background: #ebebeb;
}

/* 右侧舞台 */
.stage {
  flex: 1;
  overflow: hidden;
  background: var(--bg);
}

/* 打印样式 */
@media print {
  body { background: #fff; }
  .topbar, .panel, .toolbar, .page-info, .toolbar button { display: none !important; }
  .layout { display: block; }
  .stage { overflow: visible; }
  .preview-container { transform: none !important; }
  .page { transform: none !important; box-shadow: none; margin: 0; page-break-after: always; }
}
</style>
