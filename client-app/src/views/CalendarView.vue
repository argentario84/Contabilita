<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useCalendarEventsStore } from '@/stores/calendarEvents'
import { useScheduledExpensesStore } from '@/stores/scheduledExpenses'
import type { CreateCalendarEvent, CalendarEvent } from '@/types'

const calendarStore = useCalendarEventsStore()
const scheduledStore = useScheduledExpensesStore()

const loading = ref(true)
const showModal = ref(false)
const editingEvent = ref<number | null>(null)

const currentDate = ref(new Date())

const form = ref({
  title: '',
  description: '',
  startDate: new Date().toISOString().split('T')[0] as string,
  endDate: undefined as string | undefined,
  allDay: true,
  color: '#0d6efd',
  isShared: false
})

const colors = [
  '#0d6efd', '#6610f2', '#6f42c1', '#d63384', '#dc3545',
  '#fd7e14', '#ffc107', '#198754', '#20c997', '#0dcaf0'
]

const currentMonth = computed(() => currentDate.value.getMonth())
const currentYear = computed(() => currentDate.value.getFullYear())

const monthName = computed(() =>
  currentDate.value.toLocaleDateString('it-IT', { month: 'long', year: 'numeric' })
)

const daysInMonth = computed(() => {
  const year = currentYear.value
  const month = currentMonth.value
  const firstDay = new Date(year, month, 1)
  const lastDay = new Date(year, month + 1, 0)
  const days: { date: Date; isCurrentMonth: boolean }[] = []

  // Giorni del mese precedente
  const firstDayOfWeek = firstDay.getDay() || 7
  for (let i = firstDayOfWeek - 1; i > 0; i--) {
    const date = new Date(year, month, 1 - i)
    days.push({ date, isCurrentMonth: false })
  }

  // Giorni del mese corrente
  for (let i = 1; i <= lastDay.getDate(); i++) {
    days.push({ date: new Date(year, month, i), isCurrentMonth: true })
  }

  // Giorni del mese successivo
  const remainingDays = 42 - days.length
  for (let i = 1; i <= remainingDays; i++) {
    days.push({ date: new Date(year, month + 1, i), isCurrentMonth: false })
  }

  return days
})

onMounted(async () => {
  await loadEvents()
  await scheduledStore.fetchScheduledExpenses(true)
  loading.value = false
})

async function loadEvents() {
  const start = new Date(currentYear.value, currentMonth.value, 1)
  const end = new Date(currentYear.value, currentMonth.value + 1, 0)
  await calendarStore.fetchEvents(start.toISOString(), end.toISOString())
}

function prevMonth() {
  currentDate.value = new Date(currentYear.value, currentMonth.value - 1, 1)
  loadEvents()
}

function nextMonth() {
  currentDate.value = new Date(currentYear.value, currentMonth.value + 1, 1)
  loadEvents()
}

function goToToday() {
  currentDate.value = new Date()
  loadEvents()
}

function getEventsForDay(date: Date): CalendarEvent[] {
  const dateStr = date.toISOString().split('T')[0]
  return calendarStore.events.filter((e) => {
    const eventDate = e.startDate.split('T')[0]
    return eventDate === dateStr
  })
}

function getScheduledForDay(date: Date) {
  const dateStr = date.toISOString().split('T')[0]
  return scheduledStore.scheduledExpenses.filter((s) => {
    const dueDate = s.nextDueDate.split('T')[0]
    return dueDate === dateStr && s.isActive
  })
}

function isToday(date: Date): boolean {
  const today = new Date()
  return (
    date.getDate() === today.getDate() &&
    date.getMonth() === today.getMonth() &&
    date.getFullYear() === today.getFullYear()
  )
}

function openModal(date?: Date, eventId?: number) {
  if (eventId) {
    const e = calendarStore.events.find((e) => e.id === eventId)
    if (e) {
      editingEvent.value = eventId
      form.value = {
        title: e.title,
        description: e.description || '',
        startDate: e.startDate.split('T')[0] as string,
        endDate: e.endDate?.split('T')[0] as string | undefined,
        allDay: e.allDay,
        color: e.color || '#0d6efd',
        isShared: e.isShared
      }
    }
  } else {
    editingEvent.value = null
    form.value = {
      title: '',
      description: '',
      startDate: (date ? date.toISOString().split('T')[0] : new Date().toISOString().split('T')[0]) as string,
      endDate: undefined,
      allDay: true,
      color: '#0d6efd',
      isShared: false
    }
  }
  showModal.value = true
}

async function saveEvent() {
  if (editingEvent.value) {
    await calendarStore.updateEvent(editingEvent.value, form.value)
  } else {
    await calendarStore.createEvent(form.value)
  }
  showModal.value = false
  await loadEvents()
}

async function deleteEvent(id: number) {
  if (confirm('Sei sicuro di voler eliminare questo evento?')) {
    await calendarStore.deleteEvent(id)
    await loadEvents()
  }
}

function formatCurrency(value: number) {
  return new Intl.NumberFormat('it-IT', { style: 'currency', currency: 'EUR' }).format(value)
}
</script>

<template>
  <div class="py-4">
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h1 class="h3 mb-0">
        <i class="bi bi-calendar3 me-2"></i>
        Calendario
      </h1>
      <button class="btn btn-primary" @click="openModal()">
        <i class="bi bi-plus-lg me-1"></i>
        Nuovo Evento
      </button>
    </div>

    <div v-if="loading" class="text-center py-5">
      <div class="spinner-border text-primary"></div>
    </div>

    <template v-else>
      <div class="card border-0 shadow-sm">
        <div class="card-header bg-transparent d-flex justify-content-between align-items-center">
          <button class="btn btn-outline-primary btn-sm" @click="prevMonth">
            <i class="bi bi-chevron-left"></i>
          </button>
          <div class="d-flex align-items-center gap-2">
            <h5 class="mb-0 text-capitalize">{{ monthName }}</h5>
            <button class="btn btn-outline-secondary btn-sm" @click="goToToday">Oggi</button>
          </div>
          <button class="btn btn-outline-primary btn-sm" @click="nextMonth">
            <i class="bi bi-chevron-right"></i>
          </button>
        </div>
        <div class="card-body p-0">
          <!-- Header giorni settimana -->
          <div class="d-flex border-bottom">
            <div
              v-for="day in ['Lun', 'Mar', 'Mer', 'Gio', 'Ven', 'Sab', 'Dom']"
              :key="day"
              class="flex-fill text-center py-2 fw-bold text-muted"
              style="width: 14.28%"
            >
              {{ day }}
            </div>
          </div>

          <!-- Griglia giorni -->
          <div class="d-flex flex-wrap">
            <div
              v-for="(day, index) in daysInMonth"
              :key="index"
              class="border-end border-bottom position-relative"
              style="width: 14.28%; min-height: 100px; cursor: pointer"
              :class="{
                'bg-light': !day.isCurrentMonth,
                'bg-primary bg-opacity-10': isToday(day.date)
              }"
              @dblclick="openModal(day.date)"
            >
              <div class="p-2">
                <span
                  class="d-inline-block text-center rounded-circle"
                  :class="{
                    'bg-primary text-white': isToday(day.date),
                    'text-muted': !day.isCurrentMonth
                  }"
                  style="width: 28px; height: 28px; line-height: 28px"
                >
                  {{ day.date.getDate() }}
                </span>

                <!-- Eventi del giorno -->
                <div class="mt-1">
                  <div
                    v-for="event in getEventsForDay(day.date)"
                    :key="'e-' + event.id"
                    class="small rounded px-1 mb-1 text-white text-truncate"
                    :style="{ backgroundColor: event.color || '#0d6efd' }"
                    style="font-size: 0.7rem; cursor: pointer"
                    @click.stop="openModal(undefined, event.id)"
                    :title="event.isShared && event.createdByName ? `Condiviso da ${event.createdByName}` : event.title"
                  >
                    <i v-if="event.isShared" class="bi bi-people-fill me-1"></i>
                    {{ event.title }}
                  </div>

                  <!-- Spese programmate -->
                  <div
                    v-for="scheduled in getScheduledForDay(day.date)"
                    :key="'s-' + scheduled.id"
                    class="small rounded px-1 mb-1 bg-warning text-dark text-truncate"
                    style="font-size: 0.7rem"
                    :title="formatCurrency(scheduled.amount)"
                  >
                    <i class="bi bi-cash-coin"></i>
                    {{ scheduled.name }}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Legenda -->
      <div class="mt-3 d-flex gap-4 flex-wrap">
        <div class="d-flex align-items-center">
          <span class="badge bg-primary me-2">&nbsp;</span>
          <small class="text-muted">Eventi personali</small>
        </div>
        <div class="d-flex align-items-center">
          <span class="badge bg-primary me-2"><i class="bi bi-people-fill"></i></span>
          <small class="text-muted">Eventi condivisi</small>
        </div>
        <div class="d-flex align-items-center">
          <span class="badge bg-warning me-2">&nbsp;</span>
          <small class="text-muted">Spese Programmate</small>
        </div>
      </div>
    </template>

    <!-- Modal -->
    <div v-if="showModal" class="modal show d-block" tabindex="-1" style="background: rgba(0,0,0,0.5)">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">
              {{ editingEvent ? 'Modifica Evento' : 'Nuovo Evento' }}
            </h5>
            <button type="button" class="btn-close" @click="showModal = false"></button>
          </div>
          <form @submit.prevent="saveEvent">
            <div class="modal-body">
              <div class="mb-3">
                <label class="form-label">Titolo</label>
                <input v-model="form.title" type="text" class="form-control" required />
              </div>

              <div class="mb-3">
                <label class="form-label">Data</label>
                <input v-model="form.startDate" type="date" class="form-control" required />
              </div>

              <div class="mb-3 form-check">
                <input
                  v-model="form.allDay"
                  type="checkbox"
                  class="form-check-input"
                  id="allDay"
                />
                <label class="form-check-label" for="allDay">Tutto il giorno</label>
              </div>

              <div class="mb-3 form-check">
                <input
                  v-model="form.isShared"
                  type="checkbox"
                  class="form-check-input"
                  id="isShared"
                />
                <label class="form-check-label" for="isShared">
                  <i class="bi bi-people me-1"></i>
                  Evento condiviso
                </label>
                <div class="form-text">
                  Se attivo, questo evento sarà visibile a tutti gli utenti
                </div>
              </div>

              <div class="mb-3">
                <label class="form-label">Descrizione (opzionale)</label>
                <textarea v-model="form.description" class="form-control" rows="2"></textarea>
              </div>

              <div class="mb-3">
                <label class="form-label">Colore</label>
                <div class="d-flex flex-wrap gap-2">
                  <button
                    v-for="color in colors"
                    :key="color"
                    type="button"
                    class="btn p-0 rounded-circle"
                    :style="{
                      backgroundColor: color,
                      width: '30px',
                      height: '30px',
                      border: form.color === color ? '3px solid #000' : 'none'
                    }"
                    @click="form.color = color"
                  ></button>
                </div>
              </div>
            </div>
            <div class="modal-footer">
              <button
                v-if="editingEvent"
                type="button"
                class="btn btn-danger me-auto"
                @click="deleteEvent(editingEvent!)"
              >
                <i class="bi bi-trash me-1"></i>
                Elimina
              </button>
              <button type="button" class="btn btn-secondary" @click="showModal = false">
                Annulla
              </button>
              <button type="submit" class="btn btn-primary">
                {{ editingEvent ? 'Salva' : 'Aggiungi' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>
