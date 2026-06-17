<template>
  <div class="toolbox">
    <div class="section-title">添加元素</div>
    <div class="tool-grid">
      <button class="tool" @click="$emit('add', 'text')" title="文本">
        <span class="tool-icon">T</span>
        <span class="tool-label">文本</span>
      </button>
      <button class="tool" @click="$emit('add', 'line')" title="直线">
        <span class="tool-icon">━</span>
        <span class="tool-label">直线</span>
      </button>
      <button class="tool" @click="$emit('add', 'rect')" title="矩形">
        <span class="tool-icon">▭</span>
        <span class="tool-label">矩形</span>
      </button>
      <button class="tool" @click="$emit('add', 'barcode')" title="条码">
        <span class="tool-icon">|||</span>
        <span class="tool-label">条码</span>
      </button>
    </div>

    <div class="section-title">添加 Band</div>
    <div class="tool-grid">
      <button class="tool" @click="$emit('addBand', 'title')" title="报表标题">
        <span class="tool-label">标题</span>
      </button>
      <button class="tool" @click="$emit('addBand', 'pageHeader')" title="页头">
        <span class="tool-label">页头</span>
      </button>
      <button class="tool" @click="$emit('addBand', 'detail')" title="明细">
        <span class="tool-label">明细</span>
      </button>
      <button class="tool" @click="$emit('addBand', 'pageFooter')" title="页脚">
        <span class="tool-label">页脚</span>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
defineEmits<{
  (e: 'add', type: 'text' | 'line' | 'rect' | 'barcode'): void
  (e: 'addBand', type: 'title' | 'pageHeader' | 'detail' | 'pageFooter'): void
}>()
</script>

<style scoped>
.toolbox {
  padding: 12px;
  border-bottom: 1px solid var(--border);
}

.section-title {
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--fg-muted);
  margin-bottom: 8px;
  margin-top: 4px;
}

.section-title:first-child {
  margin-top: 0;
}

.tool-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 6px;
  margin-bottom: 12px;
}

.tool {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 2px;
  padding: 8px 4px;
  background: #fff;
  border: 1px solid var(--border-strong);
  border-radius: 4px;
  cursor: pointer;
  font-size: 12px;
  color: var(--fg);
  transition: all 0.1s;
}

.tool:hover {
  background: var(--bg-elev);
  border-color: var(--accent);
}

.tool:active {
  background: #ebebeb;
}

.tool-icon {
  font-size: 16px;
  font-weight: 600;
  line-height: 1;
  color: var(--accent);
}

.tool-label {
  font-size: 11px;
  color: var(--fg-muted);
}
</style>
