import { ref, computed } from 'vue'
import { register as apiRegister, login as apiLogin, me as apiMe } from '../api/auth'

// 简化版：直接 reactive + localStorage 持久化，不依赖 pinia
const STORAGE_KEY = 'reportengine.auth'

interface AuthSnapshot {
  token: string
  username: string
  role: string
}

function loadFromStorage(): AuthSnapshot | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    return raw ? JSON.parse(raw) : null
  } catch {
    return null
  }
}

function saveToStorage(snap: AuthSnapshot | null) {
  if (snap) {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(snap))
  } else {
    localStorage.removeItem(STORAGE_KEY)
  }
}

const initial = loadFromStorage()
const tokenRef = ref<string | null>(initial?.token ?? null)
const usernameRef = ref<string>(initial?.username ?? '')
const roleRef = ref<string>(initial?.role ?? '')

export function useAuthStore() {
  const isLoggedIn = computed(() => !!tokenRef.value)
  const isAdmin = computed(() => roleRef.value === 'ADMIN')

  async function login(username: string, password: string) {
    const res = await apiLogin({ username, password })
    applyAuth(res)
    return res
  }

  async function register(username: string, password: string, role: 'USER' | 'ADMIN' = 'USER') {
    const res = await apiRegister({ username, password, role })
    applyAuth(res)
    return res
  }

  async function refresh() {
    if (!tokenRef.value) return null
    try {
      const info = await apiMe()
      usernameRef.value = info.username
      roleRef.value = info.role
      return info
    } catch (e) {
      logout()
      throw e
    }
  }

  function logout() {
    tokenRef.value = null
    usernameRef.value = ''
    roleRef.value = ''
    saveToStorage(null)
  }

  function applyAuth(res: { token: string; username: string; role: string }) {
    tokenRef.value = res.token
    usernameRef.value = res.username
    roleRef.value = res.role
    saveToStorage({ token: res.token, username: res.username, role: res.role })
  }

  return {
    token: tokenRef,
    username: usernameRef,
    role: roleRef,
    isLoggedIn,
    isAdmin,
    login,
    register,
    refresh,
    logout,
  }
}
