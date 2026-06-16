<template>
  <div class="report-viewer">
    <!-- 工具栏 -->
    <div class="toolbar">
      <button @click="handlePrint" :disabled="!report || loading">
        🖨️ 打印
      </button>
      <button @click="handleExportPdf" :disabled="!report || loading">
        📄 导出 PDF
      </button>
      <button @click="handleExportExcel" :disabled="!report || loading">
        📊 导出 Excel
      </button>
      <span class="page-info" v-if="report">
        第 {{ currentPage + 1 }} / {{ report.totalPages }} 页
      </span>
      <button @click="prevPage" :disabled="currentPage <= 0">◀ 上一页</button>
      <button @click="nextPage" :disabled="currentPage >= (report?.totalPages ?? 1) - 1">下一页 ▶</button>
    </div>

    <!-- 加载状态 -->
    <div v-if="loading" class="loading">
      <span>加载中...</span>
    </div>

    <!-- 错误信息 -->
    <div v-if="error" class="error">
      <span>❌ {{ error }}</span>
    </div>

    <!-- 报表预览区域 -->
    <div v-if="report && !loading && currentPageData" class="preview-container">
      <div
        class="page"
        :style="{
          width: `${currentPageData.width}mm`,
          height: `${currentPageData.height}mm`,
          transform: `scale(${scale})`,
          transformOrigin: 'top left'
        }"
      >
        <div class="page-content">
          <div
            v-for="(el, idx) in currentPageData.elements"
            :key="idx"
            :class="['element', `element-${el.type}`]"
            :style="getElementStyle(el)"
          >
            <!-- 文本元素 -->
            <template v-if="el.type === 'text'">
              <span :style="getTextStyle(el)">{{ el.text }}</span>
            </template>

            <!-- 线条元素 -->
            <template v-else-if="el.type === 'line'">
              <div
                class="line"
                :style="{
                  borderColor: el.borderColor || '#000',
                  borderWidth: `${el.borderWidth || 1}px`
                }"
              ></div>
            </template>

            <!-- 形状元素 -->
            <template v-else-if="el.type === 'shape'">
              <div
                class="shape"
                :style="{ backgroundColor: el.backgroundColor || '#fff' }"
              ></div>
            </template>

            <!-- 条码元素 -->
            <template v-else-if="el.type === 'barcode'">
              <div class="barcode">{{ el.text }}</div>
            </template>

            <!-- 图片元素 -->
            <template v-else-if="el.type === 'image'">
              <img v-if="el.text" :src="el.text" class="image" />
            </template>
          </div>
        </div>
      </div>
    </div>

    <!-- 空状态 -->
    <div v-if="!report && !loading && !error" class="empty">
      <p>📋 暂无报表数据</p>
      <p>请提供模板和数据以预览报表</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import {
  previewReport,
  exportPdf,
  exportExcel,
  downloadBlob,
  type RenderRequest,
  type RenderResponse,
  type RenderedElementInfo
} from '../api/report'

// Props
const props = defineProps<{
  templateJson: string
  data: Record<string, Array<Record<string, any>>>
  scale?: number
}>()

// 状态
const loading = ref(false)
const error = ref<string | null>(null)
const report = ref<RenderResponse | null>(null)
const currentPage = ref(0)
const scale = computed(() => props.scale ?? 1)

// 当前页数据
const currentPageData = computed(() => {
  if (!report.value || report.value.pages.length === 0) return null
  return report.value.pages[currentPage.value]
})

// 监听 props 变化，自动预览
watch(
  () => [props.templateJson, props.data],
  async () => {
    if (props.templateJson) {
      await loadReport()
    }
  },
  { immediate: true, deep: true }
)

// 加载报表
async function loadReport() {
  loading.value = true
  error.value = null
  currentPage.value = 0

  try {
    report.value = await previewReport({
      templateJson: props.templateJson,
      data: props.data
    })

    if (!report.value.success) {
      error.value = report.value.error || '渲染失败'
      report.value = null
    }
  } catch (e: any) {
    error.value = e.message || '请求失败'
    report.value = null
  } finally {
    loading.value = false
  }
}

// 翻页
function prevPage() {
  if (currentPage.value > 0) currentPage.value--
}

function nextPage() {
  if (report.value && currentPage.value < report.value.totalPages - 1) {
    currentPage.value++
  }
}

// 打印 - 打开新窗口只打印报表内容
function handlePrint() {
  if (!report.value || !currentPageData.value) return
  
  // 获取当前页的页面尺寸
  const page = currentPageData.value
  const widthMm = page.width
  const heightMm = page.height
  
  // 构建打印 HTML
  let html = `<!DOCTYPE html>
<html>
<head>
  <meta charset="utf-8">
  <title>报表打印</title>
  <style>
    @page {
      size: ${widthMm}mm ${heightMm}mm;
      margin: 0;
    }
    body {
      margin: 0;
      padding: 0;
      font-family: 'Microsoft YaHei', 'SimSun', sans-serif;
    }
    .print-page {
      width: ${widthMm}mm;
      height: ${heightMm}mm;
      position: relative;
      overflow: hidden;
      page-break-after: always;
    }
    .element {
      position: absolute;
      box-sizing: border-box;
      overflow: hidden;
    }
    .element-text {
      display: flex;
      align-items: center;
    }
    .element-text span {
      width: 100%;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }
    .element-line .line {
      width: 100%;
      height: 100%;
      border-bottom-style: solid;
    }
    .element-shape .shape {
      width: 100%;
      height: 100%;
    }
    .element-image img {
      width: 100%;
      height: 100%;
      object-fit: contain;
    }
  </style>
</head>
<body>`

  // 添加所有页面的内容
  for (const p of report.value.pages) {
    html += `<div class="print-page">`
    for (const el of p.elements) {
      const style = `left:${el.x}mm;top:${el.y}mm;width:${el.width}mm;height:${el.height}mm;`
      const font = el.font ? `font-family:${el.font.family || 'Microsoft YaHei'};font-size:${el.font.size}pt;${el.font.bold ? 'font-weight:bold;' : ''}${el.font.italic ? 'font-style:italic;' : ''}${el.font.underline ? 'text-decoration:underline;' : ''}color:${el.font.color || '#000'};` : ''
      const align = el.alignment ? `text-align:${el.alignment};` : ''
      
      if (el.type === 'text') {
        html += `<div class="element element-text" style="${style}"><span style="${font}${align}">${el.text || ''}</span></div>`
      } else if (el.type === 'line') {
        html += `<div class="element element-line" style="${style}"><div class="line" style="border-color:${el.borderColor || '#000'};border-width:${el.borderWidth || 1}px;"></div></div>`
      } else if (el.type === 'shape') {
        html += `<div class="element element-shape" style="${style}background-color:${el.backgroundColor || '#fff'};"></div>`
      } else if (el.type === 'image' && el.text) {
        html += `<div class="element element-image" style="${style}"><img src="${el.text}" /></div>`
      } else if (el.type === 'barcode') {
        html += `<div class="element" style="${style}font-family:monospace;font-size:10px;text-align:center;">${el.text || ''}</div>`
      }
    }
    html += `</div>`
  }

  html += `</body></html>`

  // 打开新窗口并打印
  const printWindow = window.open('', '_blank', 'width=800,height=600')
  if (printWindow) {
    printWindow.document.write(html)
    printWindow.document.close()
    
    // 延迟打印，确保页面完全加载
    setTimeout(() => {
      printWindow.focus()
      printWindow.print()
      // 打印完成后延迟关闭，给用户时间操作
      setTimeout(() => {
        printWindow.close()
      }, 1000)
    }, 500)
  } else {
    alert('请允许浏览器弹出窗口以使用打印功能')
  }
}

// 导出 PDF
async function handleExportPdf() {
  try {
    const blob = await exportPdf({
      templateJson: props.templateJson,
      data: props.data
    })
    downloadBlob(blob, 'report.pdf')
  } catch (e: any) {
    alert('导出失败: ' + e.message)
  }
}

// 导出 Excel
async function handleExportExcel() {
  try {
    const blob = await exportExcel({
      templateJson: props.templateJson,
      data: props.data
    })
    downloadBlob(blob, 'report.xlsx')
  } catch (e: any) {
    alert('导出失败: ' + e.message)
  }
}

// 元素样式
function getElementStyle(el: RenderedElementInfo) {
  return {
    left: `${el.x}mm`,
    top: `${el.y}mm`,
    width: `${el.width}mm`,
    height: `${el.height}mm`,
    backgroundColor: el.backgroundColor || 'transparent'
  }
}

// 文本样式
function getTextStyle(el: RenderedElementInfo) {
  const style: any = {
    textAlign: (el.alignment || 'left') as any
  }
  
  if (el.font) {
    style.fontFamily = el.font.family
    style.fontSize = `${el.font.size}pt`
    style.fontWeight = el.font.bold ? 'bold' : 'normal'
    style.fontStyle = el.font.italic ? 'italic' : 'normal'
    style.textDecoration = el.font.underline ? 'underline' : 'none'
    style.color = el.font.color || '#000'
  }
  
  return style
}

// 暴露方法
defineExpose({
  loadReport,
  print: handlePrint,
  exportPdf: handleExportPdf,
  exportExcel: handleExportExcel
})
</script>

<style scoped>
.report-viewer {
  display: flex;
  flex-direction: column;
  height: 100%;
  background: #f5f5f5;
}

.toolbar {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 16px;
  background: #fff;
  border-bottom: 1px solid #e0e0e0;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.toolbar button {
  padding: 8px 16px;
  border: 1px solid #d0d0d0;
  border-radius: 4px;
  background: #fff;
  cursor: pointer;
  font-size: 14px;
  transition: all 0.2s;
}

.toolbar button:hover:not(:disabled) {
  background: #f0f0f0;
  border-color: #999;
}

.toolbar button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.page-info {
  margin: 0 8px;
  font-size: 14px;
  color: #666;
}

.loading,
.error,
.empty {
  display: flex;
  align-items: center;
  justify-content: center;
  flex: 1;
  font-size: 16px;
  color: #666;
}

.error {
  color: #d32f2f;
}

.preview-container {
  flex: 1;
  overflow: auto;
  padding: 24px;
  display: flex;
  justify-content: center;
}

.page {
  background: #fff;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
  position: relative;
  overflow: hidden;
}

.page-content {
  position: relative;
  width: 100%;
  height: 100%;
}

.element {
  position: absolute;
  box-sizing: border-box;
  overflow: hidden;
}

.element-text {
  display: flex;
  align-items: center;
  box-sizing: border-box;
}

.element-text span {
  width: 100%;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  display: block;
}

.element-line .line {
  width: 100%;
  height: 100%;
  border-bottom-style: solid;
}

.element-shape .shape {
  width: 100%;
  height: 100%;
}

.element-barcode {
  display: flex;
  align-items: center;
  justify-content: center;
  font-family: monospace;
  font-size: 10px;
}

.element-image .image {
  width: 100%;
  height: 100%;
  object-fit: contain;
}

/* 打印样式 */
@media print {
  .toolbar,
  .page-info,
  .toolbar button {
    display: none !important;
  }

  .preview-container {
    padding: 0;
    overflow: visible;
    display: block;
  }

  .page {
    box-shadow: none;
    page-break-after: always;
    transform: none !important;
    margin: 0;
  }
}
</style>
