<template>
  <div class="property-panel">
    <div class="section-title">属性</div>
    <div v-if="!element" class="empty">未选中元素</div>
    <div v-else class="form">
      <fieldset>
        <legend>位置和大小</legend>
        <div class="grid-2">
          <label>X (mm) <input type="number" v-model.number="element.x" step="0.5" /></label>
          <label>Y (mm) <input type="number" v-model.number="element.y" step="0.5" /></label>
          <label>W (mm) <input type="number" v-model.number="element.width" step="0.5" min="1" /></label>
          <label>H (mm) <input type="number" v-model.number="element.height" step="0.5" min="1" /></label>
        </div>
      </fieldset>

      <fieldset v-if="element.type === 'text'">
        <legend>文本</legend>
        <label class="full">
          内容
          <textarea v-model="element.text" rows="3" spellcheck="false" placeholder="支持 {{var}} 变量"></textarea>
        </label>
        <label class="full">
          对齐
          <select v-model="element.alignment">
            <option value="left">左对齐</option>
            <option value="center">居中</option>
            <option value="right">右对齐</option>
          </select>
        </label>
      </fieldset>

      <fieldset v-if="element.type === 'text' || element.type === 'line' || element.type === 'rect'">
        <legend>字体</legend>
        <div class="grid-2">
          <label>字号
            <input type="number" v-model.number="fontSize" min="6" max="72" />
          </label>
          <label>粗体
            <select v-model="fontBold">
              <option :value="true">是</option>
              <option :value="false">否</option>
            </select>
          </label>
        </div>
      </fieldset>

      <fieldset>
        <legend>颜色</legend>
        <div class="grid-2">
          <label>文字
            <input type="color" v-model="fontColor" />
          </label>
          <label>边框
            <input type="color" v-model="borderColor" />
          </label>
        </div>
      </fieldset>

      <fieldset v-if="element.type === 'rect' || element.type === 'line'">
        <legend>线条</legend>
        <label class="full">线宽
          <input type="number" v-model.number="lineWidth" min="0.1" max="5" step="0.1" />
        </label>
      </fieldset>

      <div class="actions">
        <button class="btn danger" @click="$emit('delete')">删除元素</button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{ element: any }>()
defineEmits<{ (e: 'delete'): void }>()

const fontSize = computed({
  get: () => props.element?.font?.size ?? 10,
  set: (v) => {
    if (!props.element.font) props.element.font = {}
    props.element.font.size = v
  }
})

const fontBold = computed({
  get: () => props.element?.font?.bold ?? false,
  set: (v) => {
    if (!props.element.font) props.element.font = {}
    props.element.font.bold = v
  }
})

const fontColor = computed({
  get: () => props.element?.font?.color ?? '#000000',
  set: (v) => {
    if (!props.element.font) props.element.font = {}
    props.element.font.color = v
  }
})

const borderColor = computed({
  get: () => props.element?.borderColor ?? '#000000',
  set: (v) => { props.element.borderColor = v }
})

const lineWidth = computed({
  get: () => props.element?.lineWidth ?? 0.5,
  set: (v) => { props.element.lineWidth = v }
})
</script>

<style scoped>
.property-panel {
  padding: 12px;
}

.section-title {
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--fg-muted);
  margin-bottom: 8px;
}

.empty {
  font-size: 12px;
  color: var(--fg-muted);
  padding: 24px 12px;
  text-align: center;
  border: 1px dashed var(--border);
  border-radius: 4px;
}

fieldset {
  border: 1px solid var(--border);
  border-radius: 4px;
  padding: 8px 10px 10px;
  margin-bottom: 10px;
}

legend {
  font-size: 11px;
  font-weight: 600;
  color: var(--fg-muted);
  padding: 0 4px;
}

label {
  display: block;
  font-size: 11px;
  color: var(--fg-muted);
  margin-bottom: 6px;
}

label.full {
  display: block;
  width: 100%;
}

.grid-2 {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 6px;
}

input[type="number"],
input[type="text"],
textarea,
select {
  display: block;
  width: 100%;
  margin-top: 2px;
  padding: 4px 6px;
  font-size: 12px;
  border: 1px solid var(--border-strong);
  border-radius: 3px;
  background: #fff;
  color: var(--fg);
  font-family: inherit;
}

textarea {
  resize: vertical;
  font-family: 'Cascadia Code', 'Consolas', monospace;
}

input[type="color"] {
  width: 100%;
  height: 24px;
  padding: 1px;
  cursor: pointer;
}

input:focus,
textarea:focus,
select:focus {
  outline: 1px solid var(--accent);
  border-color: var(--accent);
}

.actions {
  margin-top: 12px;
}

.btn {
  display: block;
  width: 100%;
  padding: 6px 12px;
  font-size: 12px;
  background: #fff;
  border: 1px solid var(--border-strong);
  border-radius: 4px;
  cursor: pointer;
  color: var(--fg);
}

.btn.danger {
  background: #fee;
  border-color: #c33;
  color: #c33;
}

.btn.danger:hover {
  background: #fdd;
}
</style>
