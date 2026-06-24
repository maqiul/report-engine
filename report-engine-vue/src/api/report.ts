import axios, { type AxiosInstance } from 'axios'
import type { RenderRequest, RenderResponse } from '../types'

/**
 * 报表 HTTP 客户端
 *
 * baseURL 默认从全局配置读（app.use(ReportEngineVue, { apiBase })）。
 */
export class ReportClient {
  private axios: AxiosInstance
  private apiBase: string

  constructor(apiBase: string = '/api/reports') {
    this.apiBase = apiBase
    this.axios = axios.create({
      baseURL: apiBase,
      timeout: 30000,
    })
  }

  /** 渲染为 JSON（前端预览用） */
  async previewReport(request: RenderRequest): Promise<RenderResponse> {
    const response = await this.axios.post<RenderResponse>('/render/preview', request)
    return response.data
  }

  /** 导出 PDF（返回 Blob） */
  async exportPdf(request: RenderRequest): Promise<Blob> {
    const response = await this.axios.post('/export/pdf', request, {
      responseType: 'blob',
    })
    return response.data
  }

  /** 导出 Excel（返回 Blob） */
  async exportExcel(request: RenderRequest): Promise<Blob> {
    const response = await this.axios.post('/export/excel', request, {
      responseType: 'blob',
    })
    return response.data
  }

  /** 健康检查 */
  async health(): Promise<{ status: string }> {
    const response = await this.axios.get<{ status: string }>('/health')
    return response.data
  }

  /** 修改 baseURL（运行时切换后端） */
  setApiBase(apiBase: string): void {
    this.apiBase = apiBase
    this.axios.defaults.baseURL = apiBase
  }

  getApiBase(): string {
    return this.apiBase
  }
}

/** 浏览器端下载 Blob */
export function downloadBlob(blob: Blob, filename: string): void {
  const url = window.URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = filename
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  window.URL.revokeObjectURL(url)
}