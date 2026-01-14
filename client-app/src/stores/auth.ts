import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api from '@/services/api'
import type { User, AuthResponse, UpdateUser } from '@/types'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<User | null>(null)
  const token = ref<string | null>(null)

  const isAuthenticated = computed(() => !!token.value)

  function init() {
    const storedToken = localStorage.getItem('token')
    const storedUser = localStorage.getItem('user')

    if (storedToken && storedUser) {
      token.value = storedToken
      user.value = JSON.parse(storedUser)
    }
  }

  async function login(email: string, password: string) {
    const response = await api.post<AuthResponse>('/auth/login', { email, password })
    setAuth(response.data)
    return response.data
  }

  async function register(data: {
    email: string
    password: string
    firstName: string
    lastName: string
    initialBudget: number
    monthlyIncome: number
  }) {
    const response = await api.post<AuthResponse>('/auth/register', data)
    setAuth(response.data)
    return response.data
  }

  async function updateProfile(data: UpdateUser) {
    const response = await api.put<User>('/auth/me', data)
    user.value = response.data
    localStorage.setItem('user', JSON.stringify(response.data))
    return response.data
  }

  function setAuth(authResponse: AuthResponse) {
    token.value = authResponse.token
    user.value = authResponse.user
    localStorage.setItem('token', authResponse.token)
    localStorage.setItem('user', JSON.stringify(authResponse.user))
  }

  function logout() {
    token.value = null
    user.value = null
    localStorage.removeItem('token')
    localStorage.removeItem('user')
  }

  return {
    user,
    token,
    isAuthenticated,
    init,
    login,
    register,
    updateProfile,
    logout
  }
})
