import axios from 'axios'

const client = axios.create({
  baseURL: '/api',
  timeout: 15000,
  responseType: 'json'
})

export interface Order {
  id: string
  customer: string
  product: string
  quantity: number
  unitPrice: number
  total: number
  orderDate: string
}

export const ordersApi = {
  list: async (): Promise<Order[]> => (await client.get<Order[]>('/orders')).data,
  get: async (id: string): Promise<Order> => (await client.get<Order>(`/orders/${id}`)).data,
  getPdfUrl: (id: string) => `/api/orders/${id}/export/pdf`,
  getExcelUrl: (id: string) => `/api/orders/${id}/export/excel`,
  getSummaryPdfUrl: () => `/api/orders/report/summary/export/pdf`
}

export default client
