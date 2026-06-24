<script setup lang="ts">
import { ref } from 'vue'
import { RouterView, RouterLink } from 'vue-router'

const version = '0.5.0-demo'
</script>

<template>
  <div class="app">
    <header class="topbar">
      <div class="brand">📊 ReportEngine Demo</div>
      <nav class="nav">
        <RouterLink to="/" class="nav-item">订单列表</RouterLink>
        <RouterLink to="/reports" class="nav-item">销售报表</RouterLink>
      </nav>
      <div class="version">v{{ version }}</div>
    </header>
    <main class="main">
      <RouterView />
    </main>
  </div>
</template>

<style>
* { box-sizing: border-box; margin: 0; padding: 0; }

body {
  font-family: -apple-system, "Segoe UI", "Microsoft YaHei", sans-serif;
  background: #f5f5f5;
  color: #1a1a1a;
  font-size: 14px;
}

.app { min-height: 100vh; display: flex; flex-direction: column; }

.topbar {
  display: flex;
  align-items: center;
  gap: 24px;
  padding: 0 24px;
  height: 48px;
  background: #1e1e1e;
  color: #d4d4d4;
  border-bottom: 1px solid #333;
}

.brand { font-weight: 600; font-size: 14px; }
.nav { display: flex; gap: 4px; flex: 1; }
.nav-item {
  padding: 6px 12px;
  color: #969696;
  text-decoration: none;
  border-radius: 3px;
  font-size: 13px;
}
.nav-item:hover { background: #2d2d2d; color: #fff; }
.nav-item.router-link-active { background: #37373d; color: #fff; }
.version { color: #6a6a6a; font-size: 12px; }

.main { flex: 1; padding: 24px; }
</style>
