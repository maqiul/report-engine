<template>
  <div class="viewer">
    <div class="toolbar">
      <div class="toolbar-left">
        <button class="btn" @click="handlePrint" :disabled="!report || loading">打印</button>
        <button class="btn" @click="handleExportPdf" :disabled="!report || loading">PDF</button>
        <button class="btn" @click="handleExportExcel" :disabled="!report || loading">Excel</button>
      </div>
      <div class="toolbar-center">
        <button class="btn-icon" @click="prevPage" :disabled="currentPage <= 0" title="上一页">‹</button>
        <span class="page-info" v-if="report">
          {{ currentPage + 1 }} / {{ report.totalPages }}
        </span>
        <button class="btn-icon" @click="nextPage" :disabled="currentPage >= (report?.totalPages ?? 1) - 1" title="下一页">›</button>
      </div>
    </div>

    <div v-if="loading" class="state">
      <span class="state-text">加载中</span>
    </div>

    <div v-if="error" class="state state-error">
      <span class="state-text">{{ error }}</span>
    </div>

    <div v-if="report && !loading && currentPageData" class="preview">
      <div
        class="page"
        :style="{
          width: `${currentPageData.width}mm`,
          height: `${currentPageData.height}mm`,
          transform: `scale(${scale})`,
          transformOrigin: 'top left'
        }"
      >
        <div
          v-for="(el, idx) in currentPageData.elements"
          :key="idx"
          :class="['element', `element-${el.type}`]"
          :style="getElementStyle(el)"
        >
          <template v-if="el.type === 'text'">
            <span :style="getTextStyle(el)">{{ el.text }}</span>
          </template>
          <template v-else-if="el.type === 'line'">
            <div
              class="line"
              :style="{
                borderColor: el.borderColor || '#000',
                borderWidth: `${el.borderWidth || 1}px`
              }"
            ></div>
          </template>
          <template v-else-if="el.type === 'shape'">
            <div
              class="shape"
              :style="{ backgroundColor: el.backgroundColor || '#fff' }"
            ></div>
          </template>
          <template v-else-if="el.type === 'barcode'">
            <div class="barcode">{{ el.text }}</div>
          </template>
          <template v-else-if="el.type === 'image'">
            <img v-if="el.text" :src="el.text" class="image" />
          </template>
        </div>
      </div>
    </div>

    <div v-if="!report && !loading && !error" class="state">
      <span class="state-text">暂无数据 — 请在左侧编辑模板和数据</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, inject } from 'vue'
import { getClient, downloadBlob } from '../index'
import type { ReportClient } from '../index'
import type { RenderResponse } from '../types'

const props = defineProps<{
  templateJson: string
  data: Record<string, any[]>
  scale: number
}>()

// 优先用 inject，没有就用全局 getClient
const injectedClient = inject<ReportClient | null>('reportClient', null)
const client = injectedClient ?? getClient()

const report = ref<RenderResponse | null>(null)
const loading = ref(false)
const error = ref<string | null>(null)
const currentPage = ref(0)
let debounceTimer: number | null = null

// 当前页数据
const currentPageData = computed(() => {
  if (!report.value) return null
  return report.value.pages[currentPage.value]
})

// 监听模板和数据变化，自动重新预览
watch(
  () => [props.templateJson, props.data],
  () => {
    if (debounceTimer) clearTimeout(debounceTimer)
    debounceTimer = window.setTimeout(() => {
      loadReport()
    }, 400)
  },
  { deep: true }
)

// 首次加载
onMounted(() => {
  loadReport()
})

async function loadReport() {
  if (!props.templateJson.trim()) {
    report.value = null
    return
  }
  loading.value = true
  error.value = null
  try {
    report.value = await client.previewReport({
      templateJson: props.templateJson,
      data: props.data
    })
    if (currentPage.value >= (report.value?.totalPages ?? 1)) {
      currentPage.value = 0
    }
  } catch (e: any) {
    error.value = e.message || '加载失败'
    report.value = null
  } finally {
    loading.value = false
  }
}

// 元素样式
function getElementStyle(el: any) {
  return {
    left: `${el.x}mm`,
    top: `${el.y}mm`,
    width: `${el.width}mm`,
    height: `${el.height}mm`
  }
}

// 文本样式
function getTextStyle(el: any) {
  const style: Record<string, string> = {
    width: '100%',
    overflow: 'hidden',
    textOverflow: 'ellipsis',
    whiteSpace: 'nowrap',
    display: 'block'
  }
  if (el.alignment) style.textAlign = el.alignment
  if (el.font) {
    if (el.font.family) style.fontFamily = el.font.family
    if (el.font.size) style.fontSize = `${el.font.size}pt`
    if (el.font.bold) style.fontWeight = 'bold'
    if (el.font.italic) style.fontStyle = 'italic'
    if (el.font.underline) style.textDecoration = 'underline'
    if (el.font.color) style.color = el.font.color
  }
  return style
}

function prevPage() {
  if (currentPage.value > 0) currentPage.value--
}

function nextPage() {
  if (report.value && currentPage.value < report.value.totalPages - 1) {
    currentPage.value++
  }
}

// 打印
function handlePrint() {
  if (!report.value) return

  // 构造打印 HTML
  let html = `<!DOCTYPE html>
<html>
<head>
<meta charset="UTF-8">
<title>打印报表</title>
<style>
  @page { size: A4; margin: 0; }
  * { margin: 0; padding: 0; box-sizing: border-box; }
  body { font-family: "Microsoft YaHei", "PingFang SC", sans-serif; background: #fff; }
  .print-page {
    width: ${report.value.pages[0]?.width || 210}mm;
    height: ${report.value.pages[0]?.height || 297}mm;
    position: relative;
    overflow: hidden;
    page-break-after: always;
  }
  .print-page:last-child { page-break-after: auto; }
  .element { position: absolute; box-sizing: border-box; overflow: hidden; }
  .element-text { display: flex; align-items: center; }
  .element-text span { width: 100%; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; display: block; }
  .element-line .line { width: 100%; height: 100%; border-bottom-style: solid; }
  .element-shape .shape { width: 100%; height: 100%; }
  .element-image img { width: 100%; height: 100%; object-fit: contain; }
</style>
</head>
<body>`

  for (const p of report.value.pages) {
    html += `<div class="print-page">`
    for (const el of p.elements) {
      const style = `left:${el.x}mm;top:${el.y}mm;width:${el.width}mm;height:${el.height}mm;`
      const font = el.font
        ? `font-family:${el.font.family || 'Microsoft YaHei'};font-size:${el.font.size || 11}pt;${el.font.bold ? 'font-weight:bold;' : ''}${el.font.italic ? 'font-style:italic;' : ''}${el.font.underline ? 'text-decoration:underline;' : ''}color:${el.font.color || '#000'};`
        : ''
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

  const printWindow = window.open('', '_blank', 'width=800,height=600')
  if (printWindow) {
    printWindow.document.write(html)
    printWindow.document.close()
    setTimeout(() => {
      printWindow.focus()
      printWindow.print()
      setTimeout(() => printWindow.close(), 1000)
    }, 500)
  } else {
    alert('请允许浏览器弹出窗口以使用打印功能')
  }
}

// 导出 PDF
async function handleExportPdf() {
  try {
    const blob = await client.exportPdf({
      templateJson: props.templateJson,
      data: props.data
    })
    downloadBlob(blob, 'report.pdf')
  } catch (e: any) {
    alert('PDF 导出失败：' + (e.message || '未知错误'))
  }
}

// 导出 Excel
async function handleExportExcel() {
  try {
    const blob = await client.exportExcel({
      templateJson: props.templateJson,
      data: props.data
    })
    downloadBlob(blob, 'report.xlsx')
  } catch (e: any) {
    alert('Excel 导出失败：' + (e.message || '未知错误'))
  }
}

defineExpose({ loadReport })
</script>

<style scoped>
.viewer {
  display: flex;
  flex-direction: column;
  height: 100%;
  background: var(--bg);
}

.toolbar {
  height: 44px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 16px;
  background: #fff;
  border-bottom: 1px solid var(--border);
  flex-shrink: 0;
}

.toolbar-left,
.toolbar-center {
  display: flex;
  align-items: center;
  gap: 6px;
}

.toolbar-center {
  position: absolute;
  left: 50%;
  transform: translateX(-50%);
}

.btn {
  height: 28px;
  padding: 0 12px;
  background: #fff;
  color: var(--fg);
  border: 1px solid var(--border-strong);
  border-radius: 4px;
  font-size: 13px;
  cursor: pointer;
  transition: all 0.1s;
}

.btn:hover:not(:disabled) {
  background: var(--bg-elev);
  border-color: var(--fg-muted);
}

.btn:active:not(:disabled) {
  background: #ebebeb;
}

.btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.btn-icon {
  width: 28px;
  height: 28px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: #fff;
  color: var(--fg);
  border: 1px solid var(--border-strong);
  border-radius: 4px;
  font-size: 16px;
  cursor: pointer;
  transition: all 0.1s;
}

.btn-icon:hover:not(:disabled) {
  background: var(--bg-elev);
  border-color: var(--fg-muted);
}

.btn-icon:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.page-info {
  font-family: var(--mono);
  font-size: 12px;
  color: var(--fg-muted);
  min-width: 60px;
  text-align: center;
}

.state {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--fg-muted);
}

.state-error {
  color: var(--danger);
}

.state-text {
  font-size: 13px;
}

.preview {
  flex: 1;
  overflow: auto;
  padding: 32px;
  display: flex;
  justify-content: center;
  align-items: flex-start;
}

.page {
  background: #fff;
  border: 1px solid var(--border);
  position: relative;
  overflow: hidden;
}

.page-content,
.preview {
  position: relative;
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

.line {
  width: 100%;
  height: 100%;
  border-bottom-style: solid;
}

.shape {
  width: 100%;
  height: 100%;
}

.barcode {
  font-family: monospace;
  font-size: 10px;
  text-align: center;
  width: 100%;
}

.image {
  width: 100%;
  height: 100%;
  object-fit: contain;
}

@media print {
  .toolbar {
    display: none !important;
  }
  .preview {
    padding: 0;
    overflow: visible;
  }
  .page {
    border: none;
    page-break-after: always;
    transform: none !important;
  }
}
</style>
