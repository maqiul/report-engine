import axios from 'axios'

const api = axios.create({
  baseURL: 'http://localhost:5000/api',
  timeout: 30000,
})

// 渲染请求
export interface RenderRequest {
  templateJson: string
  data: Record<string, Array<Record<string, any>>>
}

// 渲染响应
export interface RenderResponse {
  success: boolean
  error?: string
  pages: PageInfo[]
  totalPages: number
}

export interface PageInfo {
  pageNumber: number
  width: number
  height: number
  elements: RenderedElementInfo[]
}

export interface RenderedElementInfo {
  type: string
  x: number
  y: number
  width: number
  height: number
  text?: string
  font?: FontInfo
  alignment?: string
  backgroundColor?: string
  borderColor?: string
  borderWidth?: number
}

export interface FontInfo {
  family: string
  size: number
  bold: boolean
  italic: boolean
  underline: boolean
  color: string
}

// 预览报表
export async function previewReport(request: RenderRequest): Promise<RenderResponse> {
  const response = await api.post<RenderResponse>('/render/preview', request)
  return response.data
}

// 导出 PDF
export async function exportPdf(request: RenderRequest): Promise<Blob> {
  const response = await api.post('/export/pdf', request, {
    responseType: 'blob',
  })
  return response.data
}

// 导出 Excel
export async function exportExcel(request: RenderRequest): Promise<Blob> {
  const response = await api.post('/export/excel', request, {
    responseType: 'blob',
  })
  return response.data
}

// 下载文件辅助函数
export function downloadBlob(blob: Blob, filename: string) {
  const url = window.URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = filename
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  window.URL.revokeObjectURL(url)
}
