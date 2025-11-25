import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '@/services/api'
import type { CalendarEvent, CreateCalendarEvent } from '@/types'

export const useCalendarEventsStore = defineStore('calendarEvents', () => {
  const events = ref<CalendarEvent[]>([])
  const loading = ref(false)

  async function fetchEvents(startDate?: string, endDate?: string) {
    loading.value = true
    try {
      const params = startDate && endDate ? { startDate, endDate } : {}
      const response = await api.get<CalendarEvent[]>('/calendarevents', { params })
      events.value = response.data
      return response.data
    } finally {
      loading.value = false
    }
  }

  async function createEvent(data: CreateCalendarEvent) {
    const response = await api.post<CalendarEvent>('/calendarevents', data)
    events.value.push(response.data)
    return response.data
  }

  async function updateEvent(id: number, data: Partial<CreateCalendarEvent>) {
    const response = await api.put<CalendarEvent>(`/calendarevents/${id}`, data)
    const index = events.value.findIndex((e) => e.id === id)
    if (index !== -1) {
      events.value[index] = response.data
    }
    return response.data
  }

  async function deleteEvent(id: number) {
    await api.delete(`/calendarevents/${id}`)
    events.value = events.value.filter((e) => e.id !== id)
  }

  return {
    events,
    loading,
    fetchEvents,
    createEvent,
    updateEvent,
    deleteEvent
  }
})
