// 设计器子入口：让使用方（如 web/frontend）直接复用库里的设计器组件，
// 避免在业务项目里再拷贝一份造成"改一处漏一处"的重复维护问题。
export { default as Toolbox } from './components/editor/Toolbox.vue'
export { default as PropertyPanel } from './components/editor/PropertyPanel.vue'
export { default as TemplateEditor } from './components/editor/TemplateEditor.vue'
export { default as PreviewCanvas } from './components/editor/PreviewCanvas.vue'
export { default as BandTree } from './components/editor/BandTree.vue'

export {
  ELEMENT_TYPES,
  findTypeDef,
  textBoxType,
  SUMMARY_FUNCTIONS,
  SYSTEM_VARIABLES,
  BARCODE_FORMATS,
  CHART_TYPES,
  IMAGE_SIZING,
  SHAPE_TYPES,
  LINE_DIRECTIONS,
  FORMATS
} from './components/editor/elementTypes'

export type { ElementKind, ElementTypeDef } from './components/editor/elementTypes'
