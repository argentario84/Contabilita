import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '@/services/api'
import type {
  Caregiver,
  CreateCaregiver,
  ChildcareSlot,
  CreateChildcareSlot,
  WeeklyChildcare
} from '@/types'

export const useChildcareStore = defineStore('childcare', () => {
  const caregivers = ref<Caregiver[]>([])
  const weeklySlots = ref<ChildcareSlot[]>([])
  const currentWeekStart = ref<string>('')
  const loading = ref(false)

  async function fetchCaregivers(activeOnly = false) {
    loading.value = true
    try {
      const response = await api.get<Caregiver[]>('/caregivers', {
        params: { activeOnly }
      })
      caregivers.value = response.data
      return response.data
    } finally {
      loading.value = false
    }
  }

  async function createCaregiver(data: CreateCaregiver) {
    const response = await api.post<Caregiver>('/caregivers', data)
    caregivers.value.push(response.data)
    return response.data
  }

  async function updateCaregiver(id: number, data: Partial<CreateCaregiver>) {
    const response = await api.put<Caregiver>(`/caregivers/${id}`, data)
    const index = caregivers.value.findIndex((c) => c.id === id)
    if (index !== -1) {
      caregivers.value[index] = response.data
    }
    return response.data
  }

  async function deleteCaregiver(id: number) {
    await api.delete(`/caregivers/${id}`)
    caregivers.value = caregivers.value.filter((c) => c.id !== id)
  }

  async function fetchWeek(weekStartDate?: string) {
    loading.value = true
    try {
      const params = weekStartDate ? { weekStartDate } : {}
      const response = await api.get<WeeklyChildcare>('/childcare/week', { params })
      weeklySlots.value = response.data.slots
      currentWeekStart.value = response.data.weekStartDate
      return response.data
    } finally {
      loading.value = false
    }
  }

  async function createSlot(data: CreateChildcareSlot) {
    const response = await api.post<ChildcareSlot>('/childcare/slot', data)
    // Rimuovi slot esistente per la stessa posizione
    weeklySlots.value = weeklySlots.value.filter(
      (s) => !(s.dayOfWeek === data.dayOfWeek && s.timeSlot === data.timeSlot)
    )
    weeklySlots.value.push(response.data)
    return response.data
  }

  async function deleteSlot(id: number) {
    await api.delete(`/childcare/slot/${id}`)
    weeklySlots.value = weeklySlots.value.filter((s) => s.id !== id)
  }

  async function copyWeek(sourceWeek: string, targetWeek: string) {
    const response = await api.post<WeeklyChildcare>('/childcare/week/copy', null, {
      params: { sourceWeek, targetWeek }
    })
    if (targetWeek === currentWeekStart.value) {
      weeklySlots.value = response.data.slots
    }
    return response.data
  }

  function getSlot(dayOfWeek: number, timeSlot: number): ChildcareSlot | undefined {
    return weeklySlots.value.find((s) => s.dayOfWeek === dayOfWeek && s.timeSlot === timeSlot)
  }

  return {
    caregivers,
    weeklySlots,
    currentWeekStart,
    loading,
    fetchCaregivers,
    createCaregiver,
    updateCaregiver,
    deleteCaregiver,
    fetchWeek,
    createSlot,
    deleteSlot,
    copyWeek,
    getSlot
  }
})
