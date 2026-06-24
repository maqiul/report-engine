/** ReportEngine 公共类型定义 */

/** 字体信息 */
export interface FontInfo {
  family: string
  size: number
  bold: boolean
  italic: boolean
  underline: boolean
  color: string
}

/** 渲染后的元素 */
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

/** 单页信息 */
export interface PageInfo {
  pageNumber: number
  width: number
  height: number
  elements: RenderedElementInfo[]
}

/** 渲染请求 */
export interface RenderRequest {
  templateJson: string
  data: Record<string, Array<Record<string, any>>>
}

/** 渲染响应 */
export interface RenderResponse {
  success: boolean
  error?: string
  pages: PageInfo[]
  totalPages: number
}

/** ReportEngine Vue 插件配置 */
export interface ReportEngineVueOptions {
  /** 后端 API 地址（starter 自动暴露），默认 '/api/reports' */
  apiBase?: string
  /** 主题 */
  theme?: 'light' | 'dark'
  /** 默认字体 */
  defaultFont?: string
}