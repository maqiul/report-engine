<template>
  <div class="band-tree">
    <div class="section-title">Band 结构 <span class="muted">({{ bands.length }})</span></div>
    <div v-if="bands.length === 0" class="empty">
      暂无 Band，从工具栏添加
    </div>
    <div v-else class="band-list">
      <div
        v-for="(band, i) in bands"
        :key="i"
        class="band-item"
        :class="{ active: selectedBandIndex === i }"
        @click="selectBand(i)"
      >
        <div class="band-header">
          <span class="band-type" :data-type="band.type">{{ bandTypeLabel(band.type) }}</span>
          <span class="band-meta">h={{ band.height }}mm · {{ (band.elements || []).length }} 元素</span>
        </div>
        <div v-if="selectedBandIndex === i" class="band-actions">
          <button class="mini-btn" @click.stop="$emit('moveUp', i)" :disabled="i === 0" title="上移">↑</button>
          <button class="mini-btn" @click.stop="$emit('moveDown', i)" :disabled="i === bands.length - 1" title="下移">↓</button>
          <button class="mini-btn" @click.stop="$emit('duplicate', i)" title="复制">⎘</button>
          <button class="mini-btn danger" @click.stop="$emit('remove', i)" title="删除">×</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  bands: any[]
  selectedBandIndex: number
}>()

const emit = defineEmits<{
  (e: 'select', index: number): void
  (e: 'moveUp', index: number): void
  (e: 'moveDown', index: number): void
  (e: 'duplicate', index: number): void
  (e: 'remove', index: number): void
}>()

function selectBand(i: number) {
  emit('select', i)
}

function bandTypeLabel(type: string): string {
  const map: Record<string, string> = {
    title: '标题',
    reportHeader: '报表头',
    pageHeader: '页头',
    detail: '明细',
    pageFooter: '页脚',
    summary: '汇总'
  }
  return map[type] || type
}
</script>

<style scoped>
.band-tree {
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
}

.muted {
  color: var(--fg-muted);
  font-weight: 400;
}

.empty {
  font-size: 12px;
  color: var(--fg-muted);
  padding: 12px;
  text-align: center;
  border: 1px dashed var(--border);
  border-radius: 4px;
}

.band-list {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.band-item {
  padding: 6px 8px;
  border: 1px solid var(--border);
  border-radius: 4px;
  background: #fff;
  cursor: pointer;
  transition: all 0.1s;
}

.band-item:hover {
  background: var(--bg-elev);
}

.band-item.active {
  background: #e7f0ff;
  border-color: var(--accent);
}

.band-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.band-type {
  font-size: 12px;
  font-weight: 600;
  color: var(--fg);
}

.band-type[data-type="title"] { color: #2563eb; }
.band-type[data-type="pageHeader"] { color: #059669; }
.band-type[data-type="detail"] { color: #d97706; }
.band-type[data-type="pageFooter"] { color: #7c3aed; }

.band-meta {
  font-size: 10px;
  color: var(--fg-muted);
}

.band-actions {
  display: flex;
  gap: 4px;
  margin-top: 6px;
  padding-top: 6px;
  border-top: 1px solid var(--border);
}

.mini-btn {
  flex: 1;
  padding: 3px 0;
  font-size: 11px;
  background: #fff;
  border: 1px solid var(--border);
  border-radius: 3px;
  cursor: pointer;
  color: var(--fg);
}

.mini-btn:hover:not(:disabled) {
  background: var(--bg-elev);
}

.mini-btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.mini-btn.danger:hover {
  background: #fee;
  border-color: #c33;
  color: #c33;
}
</style>
