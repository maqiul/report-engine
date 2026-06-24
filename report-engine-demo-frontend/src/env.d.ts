/// <reference types="vite/client" />

declare module '*.vue' {
  import type { DefineComponent } from 'vue'
  const component: DefineComponent<{}, {}, any>
  export default component
}

// @reportengine/vue 本地包声明（file: 协议下 typeResolution 不可靠）
declare module '@reportengine/vue' {
  import type { DefineComponent } from 'vue'
  import type { App } from 'vue'

  export interface ReportEngineVueOptions {
    apiBase?: string
    headers?: Record<string, string>
  }

  export interface RenderRequest {
    templateJson: string
    data: Record<string, any[]>
  }

  export interface RenderResponse {
    pages: any[]
    totalPages?: number
  }

  export const ReportViewer: DefineComponent<{
    templateJson: string
    data: any
    scale?: number
  }>

  export const ReportEditor: DefineComponent<{
    templateJson?: string
    data?: any
    scale?: number
  }>

  export const ReportClient: new (apiBase: string, options?: ReportEngineVueOptions) => any

  export function downloadBlob(blob: Blob, filename: string): void

  const ReportEngineVue: { install: (app: App, options?: ReportEngineVueOptions) => void }
  export default ReportEngineVue
}
