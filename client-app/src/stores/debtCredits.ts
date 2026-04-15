import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '@/services/api'
import type { DebtCredit, CreateDebtCredit } from '@/types'

export const useDebtCreditsStore = defineStore('debtCredits', () => {
  const items = ref<DebtCredit[]>([])
  const loading = ref(false)

  async function fetchAll() {
    loading.value = true
    try {
      const { data } = await api.get<DebtCredit[]>('/debtcredits')
      items.value = data
    } finally {
      loading.value = false
    }
  }

  async function create(data: CreateDebtCredit) {
    const { data: item } = await api.post<DebtCredit>('/debtcredits', data)
    items.value.unshift(item)
    return item
  }

  async function update(id: number, data: Partial<CreateDebtCredit>) {
    const { data: item } = await api.put<DebtCredit>(`/debtcredits/${id}`, data)
    const idx = items.value.findIndex((i) => i.id === id)
    if (idx !== -1) items.value[idx] = item
    return item
  }

  async function settle(id: number) {
    const { data: item } = await api.post<DebtCredit>(`/debtcredits/${id}/settle`)
    const idx = items.value.findIndex((i) => i.id === id)
    if (idx !== -1) items.value[idx] = item
  }

  async function reopen(id: number) {
    const { data: item } = await api.post<DebtCredit>(`/debtcredits/${id}/reopen`)
    const idx = items.value.findIndex((i) => i.id === id)
    if (idx !== -1) items.value[idx] = item
  }

  async function remove(id: number) {
    await api.delete(`/debtcredits/${id}`)
    items.value = items.value.filter((i) => i.id !== id)
  }

  return { items, loading, fetchAll, create, update, settle, reopen, remove }
})
