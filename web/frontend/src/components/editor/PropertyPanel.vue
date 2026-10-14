<template>
  <div class="property-panel">
    <div class="section-title">属性 · {{ typeLabel }}</div>
    <div v-if="!element" class="empty">未选中元素</div>
    <div v-else class="form">
      <fieldset>
        <legend>位置和大小 (mm)</legend>
        <div class="grid-2">
          <label>X <input type="number" v-model.number="element.x" step="0.5" @change="emit('update')" /></label>
          <label>Y <input type="number" v-model.number="element.y" step="0.5" @change="emit('update')" /></label>
          <label>宽 <input type="number" v-model.number="element.width" step="0.5" @change="emit('update')" /></label>
          <label>高 <input type="number" v-model.number="element.height" step="0.5" @change="emit('update')" /></label>
        </div>
      </fieldset>

      <!-- ===== text 家族：静态 / 字段 / 统计 / 系统变量 ===== -->
      <template v-if="element.type === 'text'">
        <fieldset>
          <legend>框类型</legend>
          <select :value="boxType" @change="setBoxType(($event.target as HTMLSelectElement).value)">
            <option value="static">静态文本</option>
            <option value="field">字段绑定</option>
            <option value="summary">统计汇总</option>
            <option value="sysvar">系统变量</option>
          </select>
        </fieldset>

        <fieldset v-if="boxType === 'static'">
          <legend>文本内容</legend>
          <textarea v-model="element.text" rows="2" @change="emit('update')"></textarea>
        </fieldset>
        <fieldset v-else-if="boxType === 'field'">
          <legend>字段绑定</legend>
          <label>数据字段 <input type="text" v-model="element.dataField" placeholder="如 currentRow.customer" @change="emit('update')" /></label>
          <label>格式
            <select v-model="element.format" @change="emit('update')">
              <option v-for="f in FORMATS" :key="f" :value="f">{{ f || '(原样)' }}</option>
            </select>
          </label>
        </fieldset>
        <fieldset v-else-if="boxType === 'summary'">
          <legend>统计</legend>
          <label>函数
            <select v-model="element.summaryFunction" @change="emit('update')">
              <option v-for="s in SUMMARY_FUNCTIONS" :key="s">{{ s }}</option>
            </select>
          </label>
          <label>字段 <input type="text" v-model="element.summaryField" placeholder="如 amount" @change="emit('update')" /></label>
          <label>格式
            <select v-model="element.format" @change="emit('update')">
              <option v-for="f in FORMATS" :key="f" :value="f">{{ f || '(原样)' }}</option>
            </select>
          </label>
        </fieldset>
        <fieldset v-else-if="boxType === 'sysvar'">
          <legend>系统变量</legend>
          <label>变量
            <select v-model="element.systemVariable" @change="emit('update')">
              <option v-for="v in SYSTEM_VARIABLES" :key="v">{{ v }}</option>
            </select>
          </label>
        </fieldset>

        <fieldset>
          <legend>字体</legend>
          <div class="grid-2">
            <label>字体 <input type="text" v-model="font.family" @change="emit('update')" /></label>
            <label>字号 <input type="number" v-model.number="font.size" min="6" max="72" @change="emit('update')" /></label>
          </div>
          <div class="checks">
            <label><input type="checkbox" v-model="font.bold" @change="emit('update')" /> 粗</label>
            <label><input type="checkbox" v-model="font.italic" @change="emit('update')" /> 斜</label>
            <label><input type="checkbox" v-model="font.underline" @change="emit('update')" /> 下划线</label>
          </div>
          <label>颜色 <input type="color" :value="font.color || '#000000'" @input="font.color = ($event.target as HTMLInputElement).value; emit('update')" /></label>
          <label>对齐
            <select v-model="element.alignment" @change="emit('update')">
              <option value="left">左</option><option value="center">中</option><option value="right">右</option>
            </select>
          </label>
        </fieldset>
      </template>

      <!-- ===== line ===== -->
      <fieldset v-else-if="element.type === 'line'">
        <legend>线段</legend>
        <label>方向
          <select v-model="element.direction" @change="emit('update')">
            <option v-for="d in LINE_DIRECTIONS" :key="d">{{ d }}</option>
          </select>
        </label>
        <label>线宽 <input type="number" v-model.number="element.lineWidth" step="0.1" @change="emit('update')" /></label>
        <label>颜色 <input type="color" :value="element.lineColor || '#000000'" @input="element.lineColor = ($event.target as HTMLInputElement).value; emit('update')" /></label>
      </fieldset>

      <!-- ===== shape ===== -->
      <fieldset v-else-if="element.type === 'shape'">
        <legend>图形</legend>
        <label>形状
          <select v-model="element.shape" @change="emit('update')">
            <option v-for="s in SHAPE_TYPES" :key="s">{{ s }}</option>
          </select>
        </label>
        <label>填充 <input type="color" :value="element.fillColor || '#FFFFFF'" @input="element.fillColor = ($event.target as HTMLInputElement).value; emit('update')" /></label>
        <label>圆角 <input type="number" v-model.number="element.borderRadius" @change="emit('update')" /></label>
      </fieldset>

      <!-- ===== image ===== -->
      <fieldset v-else-if="element.type === 'image'">
        <legend>图像</legend>
        <label>源 (URL / base64 / 路径) <textarea v-model="element.source" rows="2" @change="emit('update')"></textarea></label>
        <label>缩放
          <select v-model="element.sizing" @change="emit('update')">
            <option v-for="s in IMAGE_SIZING" :key="s">{{ s }}</option>
          </select>
        </label>
      </fieldset>

      <!-- ===== barcode ===== -->
      <fieldset v-else-if="element.type === 'barcode'">
        <legend>条码 / 二维码</legend>
        <label>内容 <input type="text" v-model="element.value" @change="emit('update')" /></label>
        <label>类型
          <select v-model="element.format" @change="emit('update')">
            <option v-for="f in BARCODE_FORMATS" :key="f">{{ f }}</option>
          </select>
        </label>
        <label><input type="checkbox" v-model="element.showText" @change="emit('update')" /> 显示文字</label>
      </fieldset>

      <!-- ===== table ===== -->
      <fieldset v-else-if="element.type === 'table'">
        <legend>表格</legend>
        <div class="grid-2">
          <label>行数 <input type="number" v-model.number="element.rowCount" min="1" @change="emit('update')" /></label>
          <label>列数 <input type="number" v-model.number="element.colCount" min="1" @change="emit('update')" /></label>
        </div>
        <label>边框宽 <input type="number" v-model.number="element.borderWidth" step="0.1" @change="emit('update')" /></label>
      </fieldset>

      <!-- ===== crosstab ===== -->
      <fieldset v-else-if="element.type === 'crosstab'">
        <legend>交叉表</legend>
        <label>数据源 <input type="text" v-model="element.dataSource" @change="emit('update')" /></label>
        <label>行字段(逗号) <input type="text" :value="(element.rowFields || []).join(',')" @change="element.rowFields = splitList(($event.target as HTMLInputElement).value); emit('update')" /></label>
        <label>列字段 <input type="text" v-model="element.columnField" @change="emit('update')" /></label>
        <label>值字段 <input type="text" v-model="element.valueField" @change="emit('update')" /></label>
        <label>汇总
          <select v-model="element.aggregate" @change="emit('update')">
            <option v-for="s in SUMMARY_FUNCTIONS" :key="s">{{ s }}</option>
          </select>
        </label>
      </fieldset>

      <!-- ===== chart ===== -->
      <fieldset v-else-if="element.type === 'chart'">
        <legend>图表</legend>
        <label>类型
          <select v-model="element.chartType" @change="emit('update')">
            <option v-for="c in CHART_TYPES" :key="c">{{ c }}</option>
          </select>
        </label>
        <label>数据源 <input type="text" v-model="element.dataSource" @change="emit('update')" /></label>
        <label>分类字段 <input type="text" v-model="element.categoryField" @change="emit('update')" /></label>
        <label>标题 <input type="text" v-model="element.title" @change="emit('update')" /></label>
        <div v-for="(s, i) in element.series" :key="i" class="series-row">
          <input type="text" v-model="s.name" placeholder="系列名" @change="emit('update')" />
          <input type="text" v-model="s.valueField" placeholder="值字段" @change="emit('update')" />
        </div>
      </fieldset>

      <!-- ===== subreport ===== -->
      <fieldset v-else-if="element.type === 'subreport'">
        <legend>子报表</legend>
        <label>模板引用 <input type="text" v-model="element.templateRef" placeholder="如 order_detail.rptx" @change="emit('update')" /></label>
        <label>数据绑定源 <input type="text" v-model="subSource" @change="emit('update')" /></label>
        <label>高度模式
          <select v-model="element.heightMode" @change="emit('update')">
            <option value="auto">auto</option><option value="fixed">fixed</option>
          </select>
        </label>
        <label><input type="checkbox" v-model="element.repeatPerRow" @change="emit('update')" /> 每行重复</label>
      </fieldset>

      <div class="actions">
        <button class="btn danger" @click="emit('delete')">删除元素</button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import {
  textBoxType, SUMMARY_FUNCTIONS, SYSTEM_VARIABLES, BARCODE_FORMATS,
  CHART_TYPES, IMAGE_SIZING, SHAPE_TYPES, LINE_DIRECTIONS, FORMATS
} from './elementTypes'

const props = defineProps<{ element: any; dataSources?: any[] }>()
const emit = defineEmits<{ (e: 'delete'): void; (e: 'update'): void }>()

const boxType = computed(() => textBoxType(props.element))

// 确保 font 对象存在（副作用式补默认，首次访问时）
const font = computed(() => {
  if (!props.element.font) {
    props.element.font = { family: 'SimSun', size: 10, bold: false, italic: false, underline: false, color: '#000000' }
  }
  return props.element.font
})

const subSource = computed({
  get: () => props.element?.dataBinding?.source ?? '',
  set: (v: string) => {
    if (!props.element.dataBinding) props.element.dataBinding = { source: '', paramMap: {} }
    props.element.dataBinding.source = v
  }
})

function setBoxType(t: string) {
  const el = props.element
  delete el.dataField
  delete el.summaryFunction
  delete el.summaryField
  delete el.systemVariable
  if (t === 'field') el.dataField = ''
  else if (t === 'summary') { el.summaryFunction = 'Sum'; el.summaryField = '' }
  else if (t === 'sysvar') el.systemVariable = 'PageNumber'
  else if (!el.text) el.text = '文本'
  emit('update')
}

function splitList(s: string): string[] {
  return s.split(',').map(x => x.trim()).filter(Boolean)
}

const typeLabel = computed(() => {
  const m: Record<string, string> = {
    text: '文本框', line: '线段', shape: '图形', image: '图像', barcode: '条码',
    table: '表格', crosstab: '交叉表', chart: '图表', subreport: '子报表'
  }
  return m[props.element?.type] || props.element?.type || ''
})
</script>

<style scoped>
.property-panel { padding: 12px; }
.section-title { font-size: 11px; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px; color: var(--fg-muted); margin-bottom: 8px; }
.empty { font-size: 12px; color: var(--fg-muted); padding: 24px 12px; text-align: center; border: 1px dashed var(--border); border-radius: 4px; }
fieldset { border: 1px solid var(--border); border-radius: 4px; padding: 8px 10px 10px; margin-bottom: 10px; }
legend { font-size: 11px; font-weight: 600; color: var(--fg-muted); padding: 0 4px; }
label { display: block; font-size: 11px; color: var(--fg-muted); margin-bottom: 6px; }
.grid-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 6px; }
.checks { display: flex; gap: 10px; margin-bottom: 6px; }
.checks label { display: flex; align-items: center; gap: 3px; margin: 0; }
.series-row { display: grid; grid-template-columns: 1fr 1fr; gap: 6px; margin-bottom: 4px; }
input[type="number"], input[type="text"], textarea, select {
  display: block; width: 100%; margin-top: 2px; padding: 4px 6px; font-size: 12px;
  border: 1px solid var(--border-strong); border-radius: 3px; background: #fff; color: var(--fg); font-family: inherit;
}
textarea { resize: vertical; font-family: 'Cascadia Code', 'Consolas', monospace; }
input[type="color"] { width: 100%; height: 24px; padding: 1px; cursor: pointer; }
input:focus, textarea:focus, select:focus { outline: 1px solid var(--accent); border-color: var(--accent); }
.actions { margin-top: 12px; }
.btn { display: block; width: 100%; padding: 6px 12px; font-size: 12px; background: #fff; border: 1px solid var(--border-strong); border-radius: 4px; cursor: pointer; color: var(--fg); }
.btn.danger { background: #fee; border-color: #c33; color: #c33; }
.btn.danger:hover { background: #fdd; }
</style>
