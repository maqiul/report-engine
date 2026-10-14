// 元素类型定义 + 工厂 —— web 设计器与后端 Core 的单一事实源。
// type 字符串必须对齐 ReportEngine.Core 的 ReportElementConverter：
//   text / image / line / shape / subreport / chart / barcode / table / crosstab
// 字段框/统计框/系统变量框都是 type=text，靠 dataField / summaryFunction / systemVariable 区分。
// 属性名用 camelCase（TemplateParser 用 CamelCasePropertyNamesContractResolver）。

export type ElementKind =
  | 'text' | 'field' | 'summary' | 'sysvar'
  | 'line' | 'shape' | 'image' | 'barcode' | 'qrcode'
  | 'table' | 'crosstab' | 'chart' | 'subreport'

export interface ElementTypeDef {
  kind: ElementKind
  label: string
  icon: string
  group: 'text' | 'graphic' | 'data'
  create: () => any
}

const base = { x: 10, y: 0, width: 40, height: 8 }
const font = () => ({ family: 'SimSun', size: 10, bold: false, italic: false, underline: false, color: '#000000' })

export const ELEMENT_TYPES: ElementTypeDef[] = [
  {
    kind: 'text', label: '静态框', icon: 'T', group: 'text',
    create: () => ({ ...base, type: 'text', text: '文本', alignment: 'left', font: font() })
  },
  {
    kind: 'field', label: '字段框', icon: '{}', group: 'text',
    create: () => ({ ...base, type: 'text', dataField: '', format: '', alignment: 'left', font: font() })
  },
  {
    kind: 'summary', label: '统计框', icon: 'Σ', group: 'text',
    create: () => ({ ...base, type: 'text', summaryFunction: 'Sum', summaryField: '', format: 'number:2', alignment: 'right', font: font() })
  },
  {
    kind: 'sysvar', label: '系统变量', icon: '@', group: 'text',
    create: () => ({ ...base, type: 'text', systemVariable: 'PageNumber', alignment: 'center', font: font() })
  },
  {
    kind: 'line', label: '线段', icon: '━', group: 'graphic',
    create: () => ({ ...base, height: 0.3, type: 'line', direction: 'Horizontal', lineWidth: 0.3, lineColor: '#000000' })
  },
  {
    kind: 'shape', label: '图形框', icon: '▭', group: 'graphic',
    create: () => ({ ...base, width: 40, height: 20, type: 'shape', shape: 'Rectangle', borderRadius: 0, fillColor: '#FFFFFF', border: { width: 0.3, color: '#000000', top: true, bottom: true, left: true, right: true } })
  },
  {
    kind: 'image', label: '图像框', icon: '🖼', group: 'graphic',
    create: () => ({ ...base, width: 30, height: 20, type: 'image', source: '', sizing: 'FitProportional' })
  },
  {
    kind: 'barcode', label: '条形码', icon: '▮▮', group: 'graphic',
    create: () => ({ ...base, width: 40, height: 15, type: 'barcode', value: '{{currentRow.code}}', format: 'Code128', foreColor: '#000000', backColor: '#FFFFFF', showText: true })
  },
  {
    kind: 'qrcode', label: '二维码', icon: '⊞', group: 'graphic',
    create: () => ({ ...base, width: 25, height: 25, type: 'barcode', value: '{{currentRow.url}}', format: 'QRCode', foreColor: '#000000', backColor: '#FFFFFF', showText: false })
  },
  {
    kind: 'table', label: '表格', icon: '▦', group: 'data',
    create: () => ({ ...base, width: 120, height: 30, type: 'table', rowCount: 3, colCount: 3, columnWidths: [], rowHeights: [], cells: [], borderWidth: 0.3, borderColor: '#000000' })
  },
  {
    kind: 'crosstab', label: '交叉表', icon: '⊞', group: 'data',
    create: () => ({ ...base, width: 120, height: 40, type: 'crosstab', dataSource: '', rowFields: [], columnField: '', valueField: '', aggregate: 'Sum', borderWidth: 0.3, borderColor: '#000000' })
  },
  {
    kind: 'chart', label: '图表', icon: '📊', group: 'data',
    create: () => ({ ...base, width: 90, height: 60, type: 'chart', chartType: 'Bar', dataSource: '', categoryField: '', series: [{ name: '系列1', valueField: '', color: '#4e79a7' }], title: '图表' })
  },
  {
    kind: 'subreport', label: '子报表', icon: '⧉', group: 'data',
    create: () => ({ ...base, width: 120, height: 40, type: 'subreport', templateRef: '', dataBinding: { source: '', paramMap: {} }, heightMode: 'auto', repeatPerRow: true })
  }
]

export function findTypeDef(kind: ElementKind): ElementTypeDef | undefined {
  return ELEMENT_TYPES.find(t => t.kind === kind)
}

// 判断一个 text 元素属于哪个 BoxType（用于属性面板/画布显示）
export function textBoxType(el: any): 'static' | 'field' | 'summary' | 'sysvar' {
  if (el?.systemVariable) return 'sysvar'
  if (el?.summaryFunction) return 'summary'
  if (el?.dataField) return 'field'
  return 'static'
}

export const SUMMARY_FUNCTIONS = ['Sum', 'Count', 'Avg', 'Max', 'Min']
export const SYSTEM_VARIABLES = ['PageNumber', 'TotalPages', 'PrintDate', 'PrintTime', 'ReportTitle']
export const BARCODE_FORMATS = ['QRCode', 'Code128', 'Code39', 'EAN13', 'EAN8', 'UPC_A', 'DataMatrix', 'PDF417']
export const CHART_TYPES = ['Bar', 'Line', 'Pie', 'Area', 'Scatter']
export const IMAGE_SIZING = ['FitProportional', 'Stretch', 'Clip', 'ActualSize']
export const SHAPE_TYPES = ['Rectangle', 'Ellipse', 'RoundedRect', 'Triangle']
export const LINE_DIRECTIONS = ['Horizontal', 'Vertical', 'Diagonal']
export const FORMATS = ['', 'currency', 'date', 'percent', 'number:2', 'number:0']
