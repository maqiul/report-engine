<template>
  <div class="app">
    <header class="app-header">
      <h1>第三方业务系统</h1>
      <p>使用 <code>@reportengine/vue</code> 集成报表组件</p>
    </header>

    <main class="app-main">
      <!-- 直接用全局注册的 <ReportViewer> 组件 -->
      <section class="demo-section">
        <h2>报表预览</h2>
        <ReportViewer
          :templateJson="demoTemplate"
          :data="demoData"
          :scale="0.8"
        />
      </section>

      <!-- 完整编辑器（代码 / 可视化 / 预览） -->
      <section class="demo-section">
        <h2>模板编辑器</h2>
        <ReportEditor
          v-model:template="editingTemplate"
          :data="demoData"
        />
        <pre class="template-out">实时输出：{{ editingTemplate.slice(0, 200) }}{{ editingTemplate.length > 200 ? '...' : '' }}</pre>
      </section>
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'

const demoTemplate = ref(JSON.stringify({
  version: '1.0',
  page: { width: 210, height: 297, unit: 'mm' },
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
        { type: 'text', text: '{{currentRow.name}}', x: 10, y: 1, width: 100, height: 8 }
      ]
    }
  ]
}, null, 2))

const demoData = ref({
  items: [
    { name: '张三' },
    { name: '李四' },
    { name: '王五' }
  ]
})

const editingTemplate = ref(JSON.stringify({
  version: '1.0',
  bands: []
}, null, 2))
</script>

<style>
* { margin: 0; padding: 0; box-sizing: border-box; }
body, html, #app { height: 100%; font-family: 'Microsoft YaHei', system-ui, sans-serif; }
.app { display: flex; flex-direction: column; height: 100%; background: #f5f5f5; }
.app-header { padding: 16px 24px; background: #2c3e50; color: white; }
.app-header h1 { font-size: 18px; }
.app-header p { font-size: 13px; color: #bdc3c7; margin-top: 4px; }
.app-header code { background: rgba(255,255,255,0.1); padding: 2px 6px; border-radius: 3px; }
.app-main { flex: 1; overflow: auto; padding: 24px; }
.demo-section { background: white; border-radius: 6px; padding: 16px; margin-bottom: 24px; box-shadow: 0 1px 4px rgba(0,0,0,0.08); }
.demo-section h2 { font-size: 15px; margin-bottom: 12px; color: #2c3e50; }
.template-out { background: #1e1e1e; color: #d4d4d4; padding: 12px; border-radius: 4px; font-size: 12px; margin-top: 12px; overflow: auto; max-height: 120px; }
</style>