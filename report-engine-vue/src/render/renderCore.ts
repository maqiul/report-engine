// 纯前端渲染核心（不依赖 Vue/DOM）：模板 + 数据 → 渲染元素列表。
// 用途：
//   1. PreviewCanvas / TemplateEditor 的渲染数据来源（UI 与渲染逻辑解耦）
//   2. 跨端一致性校验：同一模板在 .NET / Java / 前端 三端渲染结果比对
// 语义对齐后端 ReportEngine.Core：表达式 {{currentRow.x}}、detail 按数据源展开、band 自上而下堆叠。

export interface RenderTextStyle {
  fontFamily: string
  fontSize: string
  fontWeight: string
  fontStyle: string
  textDecoration: string
  color: string
  textAlign: string
  display: string
  width: string
  lineHeight: string
  whiteSpace: string
}

export interface RenderedBox {
  left: string
  top: string
  width: string
  height: string
  /** 绕自身中心旋转角度（度）。0 = 不旋转。 */
  rotation: number
  /** CSS transform (rotation), e.g. rotate(30deg) */
  transform?: string
}

export interface RenderedItem {
  box: RenderedBox
  kind: 'text' | 'line' | 'shape' | 'image' | 'barcode' | 'qrcode' | 'chart' | 'table' | 'other'
  content?: string
  content2?: string
  textStyle?: RenderTextStyle
  lineStyle?: Record<string, string>
  shapeStyle?: Record<string, string>
  imgStyle?: Record<string, string>
  cfg?: any
  /** 长文本自动换行后的文本行 */
  lines?: string[]
  /** 换行行高（mm） */
  lineHeight?: number
}

export interface RenderedBand {
  top: number
  height: number
  els: RenderedItem[]
}

export const MM_TO_PX = 3.7795

/**
 * 长文本自动换行（三端一致规范 v1）
 * 必须与 .NET ReportEngine.Core.Rendering.TextWrapper / Java TextWrapper 完全一致，
 * 否则跨端一致性校验会失败。
 *
 * 规范：
 *   1. fontMm = fontSize(pt) * 25.4 / 72
 *   2. 字符宽度（em）：CJK / 全角标点 = 1.0，其余 = 0.5
 *   3. 行容量（em）= widthMm / fontMm（最小 1）
 *   4. 逐字符累加，超出容量则换行
 *   5. 行高 lineHeightMm = fontMm * 1.2
 *   6. 显式换行先按行拆分，再各自软换行
 *   7. maxLines > 0 时截断到 maxLines 行，末行末尾用 "…" 结尾
 */
export const LINE_HEIGHT_FACTOR = 1.2
export const ELLIPSIS = '…'

export function fontMm(fontSizePt: number): number {
  const mm = (Number(fontSizePt) || 0) * 25.4 / 72
  return mm > 0 ? mm : 1
}

export function lineHeightMm(fontSizePt: number): number {
  return fontMm(fontSizePt) * LINE_HEIGHT_FACTOR
}

/** 是否全角字符（CJK / 全角标点） */
export function isFullWidth(ch: string): boolean {
  const c = ch.charCodeAt(0)
  return c >= 0x1100 && (
    c <= 0x115f ||
    c === 0x2329 || c === 0x232a ||
    (c >= 0x2e80 && c <= 0xa4cf) ||
    (c >= 0xac00 && c <= 0xd7a3) ||
    (c >= 0xf900 && c <= 0xfaff) ||
    (c >= 0xfe10 && c <= 0xfe19) ||
    (c >= 0xfe30 && c <= 0xfe6f) ||
    (c >= 0xff00 && c <= 0xff60) ||
    (c >= 0xffe0 && c <= 0xffe6) ||
    (c >= 0x3000 && c <= 0x303e)
  )
}

/** 自动换行，返回文本行集合 */
export function wrap(text: string, widthMm: number, fontSizePt: number, maxLines = 0): string[] {
  const lines: string[] = []
  if (!text) {
    lines.push('')
    return lines
  }
  const fm = fontMm(fontSizePt)
  let capacity = widthMm / fm
  if (capacity <= 0) capacity = 1

  const normalized = text.replace(/\r\n/g, '\n').replace(/\r/g, '\n')
  for (const rawLine of normalized.split('\n')) {
    let cur = ''
    let curWidth = 0
    for (const ch of Array.from(rawLine)) {
      const w = isFullWidth(ch) ? 1.0 : 0.5
      if (curWidth + w > capacity && cur.length > 0) {
        lines.push(cur)
        cur = ''
        curWidth = 0
      }
      cur += ch
      curWidth += w
    }
    lines.push(cur)
  }

  if (maxLines > 0 && lines.length > maxLines) {
    const cut = lines.slice(0, maxLines)
    const last = cut[cut.length - 1]
    cut[cut.length - 1] = last.length > 0 ? last.slice(0, last.length - 1) + ELLIPSIS : ELLIPSIS
    return cut
  }
  return lines
}

export interface RenderOptions {
  /** mm → px 的缩放系数（默认 0.5，屏幕友好） */
  zoom?: number
  /** 是否输出样式（一致性校验时只需几何+文本，可关掉省内存） */
  withStyle?: boolean
}

const SYSTEM_VARIABLES: Record<string, (t: any) => string> = {
  PageNumber: () => '1',
  TOTAL_PAGES: () => '1',
  PrintDate: () => new Date().toISOString().slice(0, 10),
  PrintTime: () => new Date().toTimeString().slice(0, 5),
  ReportTitle: (t: any) => t?.description || ''
}

/** 表达式求值：{{currentRow.x}} / {{ds.x}} / 系统变量 */
export function evalExpression(text: string, row: any, template?: any): string {
  if (!text) return ''
  return text.replace(/\{\{([^}]+)\}\}/g, (_m: string, rawExpr: string) => {
    const expr = String(rawExpr).trim()
    const sysFn = SYSTEM_VARIABLES[expr]
    if (sysFn) return sysFn(template)
    let field = expr
    if (expr.startsWith('currentRow.')) field = expr.slice(11)
    else if (expr.includes('.')) field = expr.split('.').slice(1).join('.')
    const v = row?.[field]
    return v !== undefined && v !== null ? String(v) : ''
  })
}

/** 统计聚合：Sum / Count / Avg / Max / Min */
export function evalSummary(fn: string, field: string, rows: any[]): string {
  const nums = rows.map(r => Number(r?.[field])).filter((n: number) => !isNaN(n))
  if (!nums.length) return '0'
  switch (fn) {
    case 'Sum': return String(nums.reduce((a: number, b: number) => a + b, 0))
    case 'Count': return String(rows.length)
    case 'Avg': return String(Math.round((nums.reduce((a: number, b: number) => a + b, 0) / nums.length) * 100) / 100)
    case 'Max': return String(Math.max(...nums))
    case 'Min': return String(Math.min(...nums))
    default: return ''
  }
}

function mapChartType(t: string): string {
  return ({ Bar: 'bar', Line: 'line', Pie: 'pie', Area: 'line', Scatter: 'scatter' } as Record<string, string>)[t] || 'bar'
}

/** 图表配置（供 ECharts 渲染） */
export function buildChartConfig(el: any, data: Record<string, any[]>): any {
  const rows = data?.[el.dataSource] || []
  const cats = rows.map((r: any) => String(r?.[el.categoryField] ?? ''))
  const series = (el.series || []).map((sr: any) => ({
    name: sr.name || '',
    type: mapChartType(el.chartType),
    data: rows.map((r: any) => Number(r?.[sr.valueField]) || 0),
    areaStyle: el.chartType === 'Area' ? {} : undefined,
    itemStyle: sr.color ? { color: sr.color } : undefined
  }))
  return { title: el.title || '', cats, series, chartType: el.chartType }
}

function buildItem(el: any, row: any, ds: string, bandTop: number, u: number, data: Record<string, any[]>, withStyle: boolean): RenderedItem {
  const box: RenderedBox = {
    left: (Number(el.x || 0) * u) + 'px',
    top: (bandTop + Number(el.y || 0) * u) + 'px',
    width: (Number(el.width || 10) * u) + 'px',
    height: (Number(el.height || 5) * u) + 'px',
    rotation: Number(el.rotation || 0)
  }
  const rot = Number(el.rotation || 0)
  if (rot !== 0) {
    box.transform = 'rotate(' + rot + 'deg)'
  }
  const f = el.font || {}

  if (el.type === 'text') {
    let content: string
    if (el.systemVariable) content = evalExpression('{{' + el.systemVariable + '}}', row)
    else if (el.summaryFunction) content = evalSummary(el.summaryFunction, el.summaryField, data?.[ds] || [])
    else if (el.dataField) content = evalExpression('{{currentRow.' + el.dataField + '}}', row)
    else content = evalExpression(el.text || '', row)

    // 长文本自动换行（canGrow 撑高 / maxLines 截断）
    const fontSize = Number(f.size || 10)
    const lines = wrap(content, Number(el.width || 10), fontSize, Number(el.maxLines || 0))
    const lh = lineHeightMm(fontSize)
    if (el.canGrow) {
      box.height = (lines.length * lh) * u + 'px'
    }

    const item: RenderedItem = { box, kind: 'text', content, lines, lineHeight: lh }
    if (withStyle) {
      item.textStyle = {
        fontFamily: f.family || 'Microsoft YaHei, sans-serif',
        fontSize: (Number(f.size || 10) * (u / MM_TO_PX)) + 'px',
        fontWeight: f.bold ? 'bold' : 'normal',
        fontStyle: f.italic ? 'italic' : 'normal',
        textDecoration: f.underline ? 'underline' : 'none',
        color: f.color || '#000',
        textAlign: el.alignment || 'left',
        display: 'block', width: '100%', lineHeight: '1.2', whiteSpace: 'pre-wrap'
      }
    }
    return item
  }

  if (el.type === 'line') {
    return {
      box, kind: 'line',
      lineStyle: {
        borderTop: (Number(el.lineWidth || 0.3) * u) + 'px solid ' + (el.lineColor || '#000'),
        position: 'absolute', top: '50%', left: '0', right: '0'
      }
    }
  }

  if (el.type === 'shape') {
    return {
      box, kind: 'shape',
      shapeStyle: {
        background: el.fillColor || '#fff', border: '1px solid #000',
        borderRadius: (Number(el.borderRadius || 0) * u) + 'px',
        width: '100%', height: '100%', boxSizing: 'border-box'
      }
    }
  }

  if (el.type === 'image') return { box, kind: 'image', content: el.source || '' }

  if (el.type === 'barcode') {
    const isQR = el.format === 'QRCode' || el.format === 'DataMatrix' || el.format === 'PDF417'
    const content = evalExpression(el.value || '', row)
    return isQR
      ? { box, kind: 'qrcode', cfg: { text: content || ' ' } }
      : {
        box, kind: 'barcode',
        content,
        cfg: {
          text: content || '0', format: el.format || 'CODE128',
          lineColor: el.foreColor || '#000000', background: el.backColor || '#ffffff',
          displayValue: el.showText !== false
        }
      }
  }

  if (el.type === 'chart') return { box, kind: 'chart', cfg: buildChartConfig(el, data) }
  if (el.type === 'table') return { box, kind: 'table', cfg: { rowCount: Number(el.rowCount) || 3, colCount: Number(el.colCount) || 3 } }
  return { box, kind: (el.type || 'other') as any }
}

/**
 * 渲染模板 → 按 band 堆叠的渲染元素列表。
 * @param template 模板对象
 * @param data     数据源 { name: rows[] }
 * @param opts     zoom(缩放) / withStyle(是否输出样式)
 */
export function renderTemplate(template: any, data: Record<string, any[]> = {}, opts: RenderOptions = {}): RenderedBand[] {
  const zoom = opts.zoom ?? 0.5
  const withStyle = opts.withStyle !== false
  const u = MM_TO_PX * zoom
  const bands = template?.bands || []
  const out: RenderedBand[] = []
  let offset = 0
  for (const band of bands) {
    const bh = Number(band.height || 0) * u
    const rowsData = data?.[band.dataSource] || []
    const rows: any[] = band.type === 'detail' ? (rowsData.length ? rowsData : [{}]) : [null]
    for (const row of rows) {
      const els = (band.elements || []).map((el: any) => buildItem(el, row, band.dataSource, offset, u, data, withStyle))
      out.push({ top: offset, height: bh, els })
      offset += bh
    }
  }
  return out
}

/** 一致性校验用摘要（与 .NET/Java dump 同格式） */
export function toSummary(template: any, data: Record<string, any[]>, engine: string, zoom = 0.5): any {
  const bands = renderTemplate(template, data, { zoom, withStyle: true })
  const elements: any[] = []
  for (const band of bands) {
    for (const el of band.els) {
      if (el.kind !== 'text') continue
      elements.push({
        type: 'text',
        text: el.content || '',
        x: Math.round(parseFloat(el.box.left) * 100) / 100,
        w: Math.round(parseFloat(el.box.width) * 100) / 100,
        size: el.textStyle ? Math.round(parseFloat(el.textStyle.fontSize) * 100) / 100 : 0,
        align: el.textStyle?.textAlign || '',
        rotation: el.box.rotation || 0
      })
    }
  }
  elements.sort((a: any, b: any) => a.text.localeCompare(b.text) || a.x - b.x)
  // 前端渲染是单页流式（不分页），与 .NET/Java 的"页数"语义对齐：恒为 1
  return { engine, pageCount: 1, elements }
}
