<template>
  <div id="app">
    <header class="app-header">
      <h1>📊 ReportEngine Web</h1>
      <p>在线报表预览、打印、导出</p>
    </header>

    <main class="app-main">
      <div class="sidebar">
        <h3>📋 模板配置</h3>
        <div class="form-group">
          <label>模板 JSON：</label>
          <textarea
            v-model="templateJson"
            rows="20"
            placeholder="输入报表模板 JSON..."
          ></textarea>
        </div>
        <div class="form-group">
          <label>数据源 (JSON)：</label>
          <textarea
            v-model="dataJson"
            rows="10"
            placeholder='{"ds": [{"name": "test"}]}'
          ></textarea>
        </div>
        <div class="form-group">
          <label>缩放比例：{{ scale * 100 }}%</label>
          <input type="range" v-model.number="scale" min="0.5" max="2" step="0.1" />
        </div>
        <div class="form-group">
          <h4>示例模板：</h4>
          <button @click="loadSampleTemplate">加载简单示例</button>
          <button @click="loadSalesTemplate">加载销售报表</button>
        </div>
      </div>

      <div class="viewer">
        <ReportViewer
          ref="viewerRef"
          :template-json="templateJson"
          :data="parsedData"
          :scale="scale"
        />
      </div>
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
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

body {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
  background: #f0f2f5;
}

#app {
  display: flex;
  flex-direction: column;
  height: 100vh;
}

.app-header {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 16px 24px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
}

.app-header h1 {
  font-size: 24px;
  margin-bottom: 4px;
}

.app-header p {
  font-size: 14px;
  opacity: 0.9;
}

.app-main {
  display: flex;
  flex: 1;
  overflow: hidden;
}

.sidebar {
  width: 380px;
  background: #fff;
  border-right: 1px solid #e0e0e0;
  padding: 16px;
  overflow-y: auto;
}

.sidebar h3 {
  font-size: 16px;
  margin-bottom: 16px;
  color: #333;
}

.form-group {
  margin-bottom: 16px;
}

.form-group label {
  display: block;
  font-size: 13px;
  color: #666;
  margin-bottom: 6px;
}

.form-group textarea {
  width: 100%;
  padding: 8px 12px;
  border: 1px solid #d0d0d0;
  border-radius: 4px;
  font-family: 'Consolas', 'Monaco', monospace;
  font-size: 12px;
  resize: vertical;
}

.form-group textarea:focus {
  outline: none;
  border-color: #667eea;
}

.form-group input[type="range"] {
  width: 100%;
}

.form-group h4 {
  font-size: 13px;
  color: #666;
  margin-bottom: 8px;
}

.form-group button {
  padding: 6px 12px;
  margin-right: 8px;
  margin-bottom: 8px;
  border: 1px solid #d0d0d0;
  border-radius: 4px;
  background: #fff;
  cursor: pointer;
  font-size: 12px;
  transition: all 0.2s;
}

.form-group button:hover {
  background: #f0f0f0;
  border-color: #999;
}

.viewer {
  flex: 1;
  overflow: hidden;
}

/* 打印样式：只打印报表预览区域 */
@media print {
  body {
    margin: 0;
    padding: 0;
  }
  
  .app-header,
  .sidebar,
  .toolbar,
  .page-info,
  .toolbar button {
    display: none !important;
  }
  
  .app-main {
    display: block;
    padding: 0;
    margin: 0;
  }
  
  .viewer {
    overflow: visible;
    width: 100%;
  }
  
  .report-viewer {
    background: white;
  }
  
  .preview-container {
    transform: none !important;
  }
  
  .page {
    transform: none !important;
    box-shadow: none;
    margin: 0;
    page-break-after: always;
  }
}
</style>
