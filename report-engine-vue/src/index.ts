import type { App } from 'vue'
import type { ReportEngineVueOptions } from './types'
import { ReportClient } from './api/report'
import ReportViewer from './components/ReportViewer.vue'
import ReportEditor from './components/ReportEditor.vue'

/**
 * 全局配置（app.use 时设置）
 */
let globalConfig: Required<ReportEngineVueOptions> = {
  apiBase: '/api/reports',
  theme: 'light',
  defaultFont: 'Microsoft YaHei',
}

let globalClient: ReportClient | null = null

/** 内部 API：获取/创建全局 client */
export function getClient(): ReportClient {
  if (!globalClient) {
    globalClient = new ReportClient(globalConfig.apiBase)
  }
  return globalClient
}

/** 重置 client（测试用） */
export function resetClient(): void {
  globalClient = null
}

/** Vue Plugin install */
export function install(app: App, options: ReportEngineVueOptions = {}): void {
  globalConfig = {
    apiBase: options.apiBase ?? '/api/reports',
    theme: options.theme ?? 'light',
    defaultFont: options.defaultFont ?? 'Microsoft YaHei',
  }
  globalClient = new ReportClient(globalConfig.apiBase)

  // 全局注入：组件可以直接 inject('reportClient') / inject('reportConfig')
  app.provide('reportClient', globalClient)
  app.provide('reportConfig', globalConfig)

  // 全局注册组件
  app.component('ReportViewer', ReportViewer)
  app.component('ReportEditor', ReportEditor)
}

/** 默认导出（app.use 友好） */
export default { install }

/** 命名导出 */
export { ReportViewer, ReportEditor }
export { ReportClient, downloadBlob } from './api/report'
export type {
  RenderRequest,
  RenderResponse,
  PageInfo,
  RenderedElementInfo,
  FontInfo,
  ReportEngineVueOptions,
} from './types'