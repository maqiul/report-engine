import axios from 'axios'
import { useAuthStore } from '../stores/auth'

const api = axios.create({
  baseURL: 'http://localhost:5000/api',
  timeout: 30000,
})

// 自动带 Bearer token
api.interceptors.request.use((config) => {
  const auth = useAuthStore()
  if (auth.token) {
    config.headers.Authorization = `Bearer ${auth.token}`
  }
  return config
})

// 401 自动清登录态
api.interceptors.response.use(
  (res) => res,
  (err) => {
    if (err.response?.status === 401) {
      const auth = useAuthStore()
      auth.logout()
    }
    return Promise.reject(err)
  }
)

export interface RegisterPayload {
  username: string
  password: string
  role?: 'USER' | 'ADMIN'
}

export interface LoginPayload {
  username: string
  password: string
}

export interface AuthResponse {
  token: string
  username: string
  role: string
  tokenType: string
  expiresInMs: number
}

export const register = (data: RegisterPayload) =>
  api.post<AuthResponse>('/auth/register', data).then((r) => r.data)

export const login = (data: LoginPayload) =>
  api.post<AuthResponse>('/auth/login', data).then((r) => r.data)

export const me = () =>
  api.get<{ username: string; role: string; id: number; createdAt: string }>('/auth/me').then((r) => r.data)

export default api
