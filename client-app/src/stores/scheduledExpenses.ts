import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '@/services/api'
import type { ScheduledExpense, CreateScheduledExpense, Transaction } from '@/types'

export const useScheduledExpensesStore = defineStore('scheduledExpenses', () => {
  const scheduledExpenses = ref<ScheduledExpense[]>([])
  const dueExpenses = ref<ScheduledExpense[]>([])
  const loading = ref(false)

  async function fetchScheduledExpenses(activeOnly?: boolean) {
    loading.value = true
    try {
      const params = activeOnly !== undefined ? { activeOnly } : {}
      const response = await api.get<ScheduledExpense[]>('/scheduledexpenses', { params })
      scheduledExpenses.value = response.data
      return response.data
    } finally {
      loading.value = false
    }
  }

  async function fetchDueExpenses() {
    const response = await api.get<ScheduledExpense[]>('/scheduledexpenses/due')
    dueExpenses.value = response.data
    return response.data
  }

  async function createScheduledExpense(data: CreateScheduledExpense) {
    const response = await api.post<ScheduledExpense>('/scheduledexpenses', data)
    scheduledExpenses.value.push(response.data)
    return response.data
  }

  async function confirmExpense(id: number, data?: { actualAmount?: number; notes?: string }) {
    const response = await api.post<Transaction>(`/scheduledexpenses/${id}/confirm`, data || {})
    await fetchDueExpenses()
    return response.data
  }

  async function skipExpense(id: number) {
    const response = await api.post<ScheduledExpense>(`/scheduledexpenses/${id}/skip`)
    await fetchDueExpenses()
    return response.data
  }

  async function updateScheduledExpense(id: number, data: Partial<CreateScheduledExpense & { isActive?: boolean }>) {
    const response = await api.put<ScheduledExpense>(`/scheduledexpenses/${id}`, data)
    const index = scheduledExpenses.value.findIndex((s) => s.id === id)
    if (index !== -1) {
      scheduledExpenses.value[index] = response.data
    }
    return response.data
  }

  async function deleteScheduledExpense(id: number) {
    await api.delete(`/scheduledexpenses/${id}`)
    scheduledExpenses.value = scheduledExpenses.value.filter((s) => s.id !== id)
  }

  return {
    scheduledExpenses,
    dueExpenses,
    loading,
    fetchScheduledExpenses,
    fetchDueExpenses,
    createScheduledExpense,
    confirmExpense,
    skipExpense,
    updateScheduledExpense,
    deleteScheduledExpense
  }
})
