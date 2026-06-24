<template>
  <div class="login-shell">
    <div class="login-card">
      <div class="login-header">
        <h1>ReportEngine</h1>
        <p>JSON 模板 + 渲染 + 导出一体化</p>
      </div>

      <div class="tab-bar">
        <button
          :class="['tab', { active: mode === 'login' }]"
          @click="mode = 'login'"
        >登录</button>
        <button
          :class="['tab', { active: mode === 'register' }]"
          @click="mode = 'register'"
        >注册</button>
      </div>

      <form @submit.prevent="onSubmit" class="login-form">
        <label>
          <span>用户名</span>
          <input
            v-model="username"
            type="text"
            autocomplete="username"
            required
            :disabled="loading"
          />
        </label>
        <label>
          <span>密码{{ mode === 'register' ? '（≥6 位）' : '' }}</span>
          <input
            v-model="password"
            type="password"
            :autocomplete="mode === 'login' ? 'current-password' : 'new-password'"
            required
            :disabled="loading"
          />
        </label>
        <label v-if="mode === 'register'">
          <span>角色</span>
          <select v-model="role" :disabled="loading">
            <option value="USER">USER（普通用户）</option>
            <option value="ADMIN">ADMIN（管理员）</option>
          </select>
        </label>

        <p v-if="error" class="error">{{ error }}</p>

        <button type="submit" class="primary" :disabled="loading">
          {{ loading ? '处理中…' : (mode === 'login' ? '登录' : '注册并登录') }}
        </button>
      </form>

      <div class="login-footer">
        <small>
          {{ mode === 'login'
            ? '没有账号？点上方"注册"标签。'
            : '注册成功后自动登录，token 存浏览器。' }}
        </small>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '../stores/auth'

const auth = useAuthStore()
const mode = ref<'login' | 'register'>('login')
const username = ref('')
const password = ref('')
const role = ref<'USER' | 'ADMIN'>('USER')
const loading = ref(false)
const error = ref('')

const emit = defineEmits<{ (e: 'success'): void }>()

async function onSubmit() {
  error.value = ''
  loading.value = true
  try {
    if (mode.value === 'login') {
      await auth.login(username.value, password.value)
    } else {
      if (password.value.length < 6) {
        error.value = '密码至少 6 位'
        return
      }
      await auth.register(username.value, password.value, role.value)
    }
    emit('success')
  } catch (e: any) {
    error.value = e?.response?.data?.error || e?.message || '操作失败'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.login-shell {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #1e1e1e;
  color: #d4d4d4;
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", "PingFang SC", "Microsoft YaHei", sans-serif;
}
.login-card {
  width: 380px;
  padding: 32px 28px 24px;
  background: #252526;
  border: 1px solid #333;
  border-radius: 6px;
}
.login-header h1 {
  font-size: 20px;
  font-weight: 600;
  margin: 0 0 4px;
  color: #d4d4d4;
}
.login-header p {
  font-size: 12px;
  color: #888;
  margin: 0 0 20px;
}
.tab-bar {
  display: flex;
  border-bottom: 1px solid #333;
  margin-bottom: 18px;
}
.tab {
  flex: 1;
  padding: 8px 0;
  background: transparent;
  border: 0;
  border-bottom: 2px solid transparent;
  color: #888;
  font-size: 13px;
  cursor: pointer;
}
.tab.active {
  color: #d4d4d4;
  border-bottom-color: #4ec9b0;
}
.login-form label {
  display: block;
  margin-bottom: 12px;
}
.login-form label > span {
  display: block;
  font-size: 12px;
  color: #888;
  margin-bottom: 4px;
}
.login-form input,
.login-form select {
  width: 100%;
  padding: 6px 8px;
  background: #1e1e1e;
  border: 1px solid #3c3c3c;
  border-radius: 3px;
  color: #d4d4d4;
  font-size: 13px;
  font-family: inherit;
  box-sizing: border-box;
}
.login-form input:focus,
.login-form select:focus {
  outline: none;
  border-color: #4ec9b0;
}
.login-form input:disabled {
  opacity: 0.5;
}
.error {
  color: #f48771;
  font-size: 12px;
  margin: 4px 0 12px;
}
.primary {
  width: 100%;
  padding: 8px 12px;
  background: #0e639c;
  border: 0;
  border-radius: 3px;
  color: #fff;
  font-size: 13px;
  cursor: pointer;
  font-family: inherit;
}
.primary:hover:not(:disabled) {
  background: #1177bb;
}
.primary:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
.login-footer {
  margin-top: 18px;
  color: #666;
  font-size: 11px;
  text-align: center;
}
</style>
