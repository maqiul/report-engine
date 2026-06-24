<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { ordersApi, type Order } from '../api/client'
import { ReportViewer } from '@reportengine/vue'
import { Templates } from '../templates'

const orders = ref<Order[]>([])
const loading = ref(true)

const templateJson = ref<string>(Templates.SALES_SUMMARY)
const data = computed(() => ({ orders: orders.value }))

onMounted(async () => {
  try {
    orders.value = await ordersApi.list()
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="report-dashboard">
    <div class="page-header">
      <div>
        <RouterLink to="/" class="back">← 返回列表</RouterLink>
        <h1>销售报表</h1>
        <p class="subtitle">通过 &lt;ReportViewer&gt; 直接渲染后端数据</p>
      </div>
      <a :href="ordersApi.getSummaryPdfUrl()" class="btn" target="_blank" rel="noopener">下载汇总 PDF</a>
    </div>

    <div v-if="loading" class="state">加载中…</div>
    <div v-else class="report-wrapper">
      <div class="report-frame">
        <ReportViewer
          :templateJson="templateJson"
          :data="data"
          :scale="0.9"
        />
      </div>
    </div>
  </div>
</template>

<style scoped>
.report-dashboard { max-width: 900px; margin: 0 auto; }
.page-header {
  display: flex; align-items: flex-start; justify-content: space-between;
  margin-bottom: 16px;
}
.back { color: #0066cc; text-decoration: none; font-size: 13px; }
.back:hover { text-decoration: underline; }
h1 { font-size: 22px; font-weight: 600; margin-top: 6px; }
.subtitle { color: #666; font-size: 13px; margin-top: 4px; }
.btn {
  padding: 6px 14px;
  background: #0066cc;
  color: #fff;
  text-decoration: none;
  border-radius: 3px;
  font-size: 13px;
}
.btn:hover { background: #0052a3; }
.state { padding: 40px; text-align: center; color: #666; }
.report-wrapper { background: #fff; padding: 24px; border: 1px solid #e0e0e0; }
.report-frame { background: #fff; min-height: 400px; }
</style>
