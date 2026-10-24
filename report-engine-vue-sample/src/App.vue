<template>
  <div class="app">
    <header class="app-header">
      <h1>第三方业务系统</h1>
      <p>
        使用 <code>@reportengine/vue</code> 集成报表组件
        <span class="badge">纯前端设计器 · 零后端依赖</span>
      </p>
    </header>

    <main class="app-main">
      <!-- ① 模板编辑器：v-model 双向绑定模板 JSON，纯前端，无后端 -->
      <section class="demo-section">
        <div class="sec-head">
          <h2>① 模板编辑器（ReportEditor）</h2>
          <div class="sec-actions">
            <button class="btn" @click="saveTemplate">💾 下载模板 .rptx</button>
            <button class="btn" @click="exportViaBackend">
              🖨 走后端导出 PDF
            </button>
          </div>
        </div>
        <p class="hint">
          设计器（编辑 + 所见即所得预览）完全在浏览器内运行，不需要任何后端。
          导出的 .rptx 是一份 JSON 模板，可直接交给后端渲染引擎出 PDF/Excel。
        </p>
        <ReportEditor v-model="editingTemplate" :data="demoData" />
        <details class="raw">
          <summary>查看模板 JSON（实时）</summary>
          <pre class="template-out">{{ editingTemplate }}</pre>
        </details>
      </section>

      <!-- ② 后端渲染预览：需要后端渲染服务（可选） -->
      <section class="demo-section">
        <h2>② 后端渲染预览（ReportViewer，可选）</h2>
        <p class="hint">
          <code>&lt;ReportViewer&gt;</code> 调用后端 <code>/api/reports/render/preview</code>，
          需要你的项目里有 report-engine 渲染服务（.NET WebApi 或 Java Starter）。
          没有后端时用上面的设计器预览即可。
        </p>
        <ReportViewer :templateJson="editingTemplate" :data="demoData" :scale="0.8" />
      </section>
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { getClient } from '@reportengine/vue'

// ---------- ① 设计器数据：模板 JSON + 业务数据 ----------
const demoData = ref({
  items: [
    { name: '张三', amount: 100 },
    { name: '李四', amount: 200 },
    { name: '王五', amount: 300 }
  ]
})

const editingTemplate = ref(JSON.stringify({
  version: '1.0',
  page: { width: 210, height: 297, unit: 'mm', margin: { top: 10, bottom: 10, left: 10, right: 10 } },
  dataSources: [{ name: 'items', type: 'json' }],
  bands: [
    {
      type: 'reportHeader', height: 20,
      elements: [
        { type: 'text', text: '示例报表', x: 10, y: 5, width: 190, height: 10,
          font: { family: 'Microsoft YaHei', size: 18, bold: true, color: '#000' },
          alignment: 'center' }
      ]
    },
    {
      type: 'detail', height: 10, dataSource: 'items',
      elements: [
        { type: 'text', text: '{{currentRow.name}}', x: 10, y: 1, width: 100, height: 8 },
        { type: 'text', text: '¥{{currentRow.amount}}', x: 120, y: 1, width: 60, height: 8, alignment: 'right' }
      ]
    }
  ]
}, null, 2))

// ---------- 保存模板（纯前端下载） ----------
function saveTemplate() {
  const blob = new Blob([editingTemplate.value], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = 'my-report.rptx'
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
}

// ---------- ② 可选：走后端导出 PDF ----------
// 需要后端渲染服务；没有时会提示，不影响设计器使用
async function exportViaBackend() {
  try {
    const client = getClient()
    const blob = await client.exportPdf({
      templateJson: editingTemplate.value,
      data: demoData.value
    })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = 'my-report.pdf'
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)
    URL.revokeObjectURL(url)
  } catch (e) {
    alert('后端渲染服务不可用：' + (e as Error).message +
      '\n提示：设计器与前端预览不需要后端，只有导出 PDF/Excel 需要。')
  }
}
</script>

<style>
* { margin: 0; padding: 0; box-sizing: border-box; }
body, html, #app { height: 100%; font-family: 'Microsoft YaHei', system-ui, sans-serif; }
.app { display: flex; flex-direction: column; height: 100%; background: #f5f5f5; }
.app-header { padding: 16px 24px; background: #2c3e50; color: white; display: flex; align-items: center; gap: 16px; }
.app-header h1 { font-size: 18px; }
.app-header p { font-size: 13px; color: #bdc3c7; margin-top: 4px; }
.app-header code { background: rgba(255,255,255,0.1); padding: 2px 6px; border-radius: 3px; }
.badge { display: inline-block; margin-left: 8px; padding: 1px 8px; border-radius: 10px; background: #27ae60; color: #fff; font-size: 11px; }
.app-main { flex: 1; overflow: auto; padding: 24px; }
.demo-section { background: white; border-radius: 6px; padding: 16px; margin-bottom: 24px; box-shadow: 0 1px 4px rgba(0,0,0,0.08); }
.demo-section h2 { font-size: 15px; margin-bottom: 8px; color: #2c3e50; }
.sec-head { display: flex; align-items: center; justify-content: space-between; }
.sec-actions { display: flex; gap: 8px; }
.btn { padding: 6px 12px; font-size: 12px; background: #fff; border: 1px solid #ccc; border-radius: 4px; cursor: pointer; }
.btn:hover:not(:disabled) { border-color: #2c3e50; }
.btn:disabled { opacity: .5; cursor: not-allowed; }
.hint { font-size: 12px; color: #666; line-height: 1.7; margin-bottom: 10px; }
.hint code { background: #f0f0f0; padding: 1px 5px; border-radius: 3px; }
.raw { margin-top: 12px; font-size: 12px; }
.raw summary { cursor: pointer; color: #666; }
.template-out { background: #1e1e1e; color: #d4d4d4; padding: 12px; border-radius: 4px; font-size: 12px; margin-top: 8px; overflow: auto; max-height: 220px; }
</style>
