<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { ordersApi, type Order } from '../api/client'

const orders = ref<Order[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

onMounted(async () => {
  try {
    orders.value = await ordersApi.list()
  } catch (e: any) {
    error.value = e?.message ?? '加载失败'
  } finally {
    loading.value = false
  }
})

function fmt(n: number) {
  return '¥' + n.toFixed(2)
}
</script>

<template>
  <div class="order-list">
    <div class="page-header">
      <h1>销售订单</h1>
      <RouterLink to="/reports" class="btn-secondary">查看销售报表 →</RouterLink>
    </div>

    <div v-if="loading" class="state">加载中…</div>
    <div v-else-if="error" class="state error">错误: {{ error }}</div>
    <table v-else class="orders-table">
      <thead>
        <tr>
          <th>订单号</th>
          <th>客户</th>
          <th>产品</th>
          <th class="num">数量</th>
          <th class="num">单价</th>
          <th class="num">合计</th>
          <th>日期</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="o in orders" :key="o.id">
          <td>
            <RouterLink :to="`/orders/${o.id}`" class="link">{{ o.id }}</RouterLink>
          </td>
          <td>{{ o.customer }}</td>
          <td>{{ o.product }}</td>
          <td class="num">{{ o.quantity }}</td>
          <td class="num">{{ fmt(o.unitPrice) }}</td>
          <td class="num strong">{{ fmt(o.total) }}</td>
          <td>{{ o.orderDate }}</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style scoped>
.order-list { max-width: 1100px; margin: 0 auto; }
.page-header {
  display: flex; align-items: center; justify-content: space-between;
  margin-bottom: 16px;
}
h1 { font-size: 22px; font-weight: 600; }
.btn-secondary {
  padding: 6px 12px;
  background: #fff;
  border: 1px solid #d4d4d4;
  color: #333;
  text-decoration: none;
  border-radius: 3px;
  font-size: 13px;
}
.btn-secondary:hover { background: #f0f0f0; }
.state { padding: 40px; text-align: center; color: #666; }
.state.error { color: #c00; }
.orders-table {
  width: 100%;
  background: #fff;
  border-collapse: collapse;
  border: 1px solid #e0e0e0;
}
.orders-table th, .orders-table td {
  padding: 10px 12px;
  text-align: left;
  border-bottom: 1px solid #f0f0f0;
}
.orders-table th {
  background: #fafafa;
  font-weight: 600;
  font-size: 12px;
  color: #555;
  text-transform: uppercase;
  letter-spacing: 0.4px;
}
.orders-table tr:hover td { background: #fafafa; }
.num { text-align: right; font-variant-numeric: tabular-nums; }
.strong { font-weight: 600; }
.link { color: #0066cc; text-decoration: none; }
.link:hover { text-decoration: underline; }
</style>
