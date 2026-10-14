<template>
  <div class="toolbox">
    <div class="section-title">插入元素</div>
    <div class="tool-grid">
      <button
        v-for="t in ELEMENT_TYPES"
        :key="t.kind"
        class="tool"
        @click="$emit('add', t.kind)"
        :title="t.label"
      >
        <span class="tool-icon">{{ t.icon }}</span>
        <span class="tool-label">{{ t.label }}</span>
      </button>
    </div>

    <div class="section-title">插入区域</div>
    <div class="tool-grid">
      <button
        v-for="b in BAND_TYPES"
        :key="b.type"
        class="tool band"
        @click="$emit('addBand', b.type)"
      >
        <span class="tool-label">{{ b.label }}</span>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ELEMENT_TYPES, type ElementKind } from './elementTypes'

defineEmits<{
  (e: 'add', kind: ElementKind): void
  (e: 'addBand', type: string): void
}>()

// band type 对齐 Core BandType（camelCase）：reportHeader/header/detail/groupHeader/groupFooter/footer/reportFooter
const BAND_TYPES = [
  { type: 'reportHeader', label: '报表头' },
  { type: 'header', label: '页眉' },
  { type: 'detail', label: '明细' },
  { type: 'groupHeader', label: '分组头' },
  { type: 'groupFooter', label: '分组尾' },
  { type: 'footer', label: '页脚' },
  { type: 'reportFooter', label: '报表尾' }
]
</script>

<style scoped>
.toolbox { padding: 12px; border-bottom: 1px solid var(--border); }
.section-title {
  font-size: 11px; font-weight: 600; text-transform: uppercase;
  letter-spacing: 0.5px; color: var(--fg-muted); margin-bottom: 8px; margin-top: 4px;
}
.section-title:first-child { margin-top: 0; }
.tool-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 6px; margin-bottom: 12px; }
.tool {
  display: flex; flex-direction: column; align-items: center; justify-content: center;
  gap: 2px; padding: 8px 4px; background: #fff; border: 1px solid var(--border-strong);
  border-radius: 4px; cursor: pointer; font-size: 12px; color: var(--fg); transition: all 0.1s;
}
.tool:hover { background: var(--bg-elev); border-color: var(--accent); }
.tool:active { background: #ebebeb; }
.tool.band { flex-direction: row; padding: 6px 4px; }
.tool-icon { font-size: 15px; font-weight: 600; line-height: 1; color: var(--accent); }
.tool-label { font-size: 11px; color: var(--fg-muted); }
</style>
