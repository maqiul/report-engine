<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, RouterLink } from 'vue-router'
import { ordersApi, type Order } from '../api/client'
import { ReportViewer } from '@reportengine/vue'
import { Templates } from '../templates'

const route = useRoute()
const order = ref<Order | null>(null)
const loading = ref(true)
const error = ref<string | null>(null)

const orderId = computed(() => String(route.params.id))

// 单订单模板
const templateJson = ref<string>(Templates.SALES_ORDER)
// 给模板注入数据
const data = computed(() => ({
  orders: order.value ? [order.value] : []
}))

onMounted(async () => {
  try {
    order.value = await ordersApi.get(orderId.value)
  } catch (e: any) {
    error.value = e?.response?.status === 404 ? '订单不存在' : (e?.message ?? '加载失败')
  } finally {
    loading.value = false
  }
})

const pdfUrl = computed(() => order.value ? ordersApi.getPdfUrl(order.value.id) : '')
const excelUrl = computed(() => order.value ? ordersApi.getExcelUrl(order.value.id) : '')
</script>

<template>
  <div class="order-detail">
    <div class="page-header">
      <div>
        <RouterLink to="/" class="back">← 返回列表</RouterLink>
        <h1>订单 {{ orderId }}</h1>
      </div>
      <div v-if="order" class="actions">
        <a :href="pdfUrl" class="btn" target="_blank" rel="noopener">下载 PDF</a>
        <a :href="excelUrl" class="btn" target="_blank" rel="noopener">下载 Excel</a>
      </div>
    </div>

    <div v-if="loading" class="state">加载中…</div>
    <div v-else-if="error" class="state error">{{ error }}</div>
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
.order-detail { max-width: 900px; margin: 0 auto; }
.page-header {
  display: flex; align-items: flex-start; justify-content: space-between;
  margin-bottom: 16px;
}
.back { color: #0066cc; text-decoration: none; font-size: 13px; }
.back:hover { text-decoration: underline; }
h1 { font-size: 22px; font-weight: 600; margin-top: 6px; }
.actions { display: flex; gap: 8px; }
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
.state.error { color: #c00; }
.report-wrapper { background: #fff; padding: 24px; border: 1px solid #e0e0e0; }
.report-frame { background: #fff; min-height: 400px; }
</style>
