<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useTransactionsStore } from '@/stores/transactions'
import { useScheduledExpensesStore } from '@/stores/scheduledExpenses'
import FullCalendar from '@fullcalendar/vue3'
import dayGridPlugin from '@fullcalendar/daygrid'
import interactionPlugin from '@fullcalendar/interaction'
import itLocale from '@fullcalendar/core/locales/it'
import type { Transaction, ScheduledExpense } from '@/types'
import { TransactionType } from '@/types'

const transactionsStore = useTransactionsStore()
const scheduledStore = useScheduledExpensesStore()

const loading = ref(true)
const selectedTransaction = ref<Transaction | null>(null)
const showModal = ref(false)

// Stato per il range di date corrente del calendario
const currentStart = ref<string>('')
const currentEnd = ref<string>('')

onMounted(async () => {
  await scheduledStore.fetchScheduledExpenses(true)
  loading.value = false
})

// Eventi del calendario basati sulle transazioni
const calendarEvents = computed(() => {
  const events: any[] = []

  // Transazioni
  transactionsStore.transactions.forEach((t) => {
    const isExpense = t.type === TransactionType.Expense
    events.push({
      id: `t-${t.id}`,
      title: `${isExpense ? '-' : '+'}${formatAmount(t.amount)} ${t.description}`,
      start: t.date.split('T')[0],
      backgroundColor: isExpense ? (t.categoryColor || '#dc3545') : '#198754',
      borderColor: isExpense ? (t.categoryColor || '#dc3545') : '#198754',
      extendedProps: {
        type: 'transaction',
        transaction: t
      }
    })
  })

  // Spese programmate (prossima scadenza)
  scheduledStore.scheduledExpenses
    .filter((s) => s.isActive)
    .forEach((s) => {
      events.push({
        id: `s-${s.id}`,
        title: `[Programmata] ${s.name} - ${formatAmount(s.amount)}`,
        start: s.nextDueDate.split('T')[0],
        backgroundColor: '#ffc107',
        borderColor: '#ffc107',
        textColor: '#000',
        extendedProps: {
          type: 'scheduled',
          scheduled: s
        }
      })
    })

  return events
})

const calendarOptions = computed(() => ({
  plugins: [dayGridPlugin, interactionPlugin],
  initialView: 'dayGridMonth',
  locale: itLocale,
  headerToolbar: {
    left: 'prev,next today',
    center: 'title',
    right: 'dayGridMonth,dayGridWeek'
  },
  events: calendarEvents.value,
  eventClick: handleEventClick,
  datesSet: handleDatesSet,
  height: 'auto',
  dayMaxEvents: 3,
  eventDisplay: 'block'
}))

async function handleDatesSet(arg: any) {
  const start = arg.start.toISOString().split('T')[0]
  const end = arg.end.toISOString().split('T')[0]

  // Evita ricaricamenti inutili
  if (start !== currentStart.value || end !== currentEnd.value) {
    currentStart.value = start
    currentEnd.value = end
    await transactionsStore.fetchTransactions({ startDate: start, endDate: end })
  }
}

function handleEventClick(arg: any) {
  const { type, transaction, scheduled } = arg.event.extendedProps

  if (type === 'transaction') {
    selectedTransaction.value = transaction
    showModal.value = true
  } else if (type === 'scheduled') {
    // Per le spese programmate mostriamo info basica
    selectedTransaction.value = {
      id: scheduled.id,
      amount: scheduled.amount,
      description: scheduled.name,
      date: scheduled.nextDueDate,
      type: TransactionType.Expense,
      notes: `Spesa programmata - ${scheduled.description || ''}`,
      categoryId: scheduled.categoryId,
      categoryName: scheduled.categoryName,
      categoryColor: scheduled.categoryColor,
      scheduledExpenseId: scheduled.id
    } as Transaction
    showModal.value = true
  }
}

function formatAmount(value: number): string {
  return new Intl.NumberFormat('it-IT', {
    style: 'currency',
    currency: 'EUR'
  }).format(value)
}

function formatDate(dateStr: string): string {
  return new Date(dateStr).toLocaleDateString('it-IT', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}

function getRecurrenceLabel(scheduled: ScheduledExpense | undefined): string {
  if (!scheduled) return ''
  const labels: Record<number, string> = {
    1: 'Giornaliera',
    2: 'Settimanale',
    3: 'Mensile',
    4: 'Annuale'
  }
  return labels[scheduled.recurrence] || ''
}
</script>

<template>
  <div class="py-4">
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h1 class="h3 mb-0">
        <i class="bi bi-calendar-week me-2"></i>
        Calendario Transazioni
      </h1>
      <RouterLink to="/transactions" class="btn btn-outline-primary">
        <i class="bi bi-list-ul me-1"></i>
        Vista Lista
      </RouterLink>
    </div>

    <div v-if="loading" class="text-center py-5">
      <div class="spinner-border text-primary"></div>
    </div>

    <template v-else>
      <div class="card border-0 shadow-sm">
        <div class="card-body">
          <FullCalendar :options="calendarOptions" />
        </div>
      </div>

      <!-- Legenda -->
      <div class="mt-3 d-flex gap-4 flex-wrap">
        <div class="d-flex align-items-center">
          <span class="badge bg-success me-2">&nbsp;</span>
          <small class="text-muted">Entrate</small>
        </div>
        <div class="d-flex align-items-center">
          <span class="badge bg-danger me-2">&nbsp;</span>
          <small class="text-muted">Uscite</small>
        </div>
        <div class="d-flex align-items-center">
          <span class="badge bg-warning me-2">&nbsp;</span>
          <small class="text-muted">Spese Programmate</small>
        </div>
      </div>
    </template>

    <!-- Modal dettaglio transazione -->
    <div v-if="showModal" class="modal show d-block" tabindex="-1" style="background: rgba(0,0,0,0.5)">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">
              <i class="bi bi-receipt me-2"></i>
              Dettaglio Transazione
            </h5>
            <button type="button" class="btn-close" @click="showModal = false"></button>
          </div>
          <div class="modal-body" v-if="selectedTransaction">
            <div class="mb-3">
              <label class="form-label text-muted small">Importo</label>
              <div class="fs-3 fw-bold" :class="selectedTransaction.type === TransactionType.Expense ? 'text-danger' : 'text-success'">
                {{ selectedTransaction.type === TransactionType.Expense ? '-' : '+' }}{{ formatAmount(selectedTransaction.amount) }}
              </div>
            </div>

            <div class="mb-3">
              <label class="form-label text-muted small">Descrizione</label>
              <div class="fw-medium">{{ selectedTransaction.description }}</div>
            </div>

            <div class="mb-3">
              <label class="form-label text-muted small">Data</label>
              <div>{{ formatDate(selectedTransaction.date) }}</div>
            </div>

            <div class="mb-3">
              <label class="form-label text-muted small">Categoria</label>
              <div>
                <span
                  class="badge"
                  :style="{ backgroundColor: selectedTransaction.categoryColor || '#6c757d' }"
                >
                  {{ selectedTransaction.categoryName }}
                </span>
              </div>
            </div>

            <div v-if="selectedTransaction.notes" class="mb-3">
              <label class="form-label text-muted small">Note</label>
              <div class="text-muted">{{ selectedTransaction.notes }}</div>
            </div>

            <div v-if="selectedTransaction.scheduledExpenseId" class="alert alert-warning mb-0">
              <i class="bi bi-calendar-check me-2"></i>
              Questa transazione e legata ad una spesa programmata
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="showModal = false">
              Chiudi
            </button>
            <RouterLink
              :to="`/transactions?id=${selectedTransaction?.id}`"
              class="btn btn-primary"
              v-if="selectedTransaction && !selectedTransaction.scheduledExpenseId"
            >
              <i class="bi bi-pencil me-1"></i>
              Modifica
            </RouterLink>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style>
.fc {
  font-family: inherit;
}

.fc .fc-toolbar-title {
  font-size: 1.25rem;
  text-transform: capitalize;
}

.fc .fc-button-primary {
  background-color: #0d6efd;
  border-color: #0d6efd;
}

.fc .fc-button-primary:not(:disabled).fc-button-active,
.fc .fc-button-primary:not(:disabled):active {
  background-color: #0b5ed7;
  border-color: #0a58ca;
}

.fc .fc-daygrid-day.fc-day-today {
  background-color: rgba(13, 110, 253, 0.1);
}

.fc .fc-event {
  font-size: 0.75rem;
  padding: 2px 4px;
  border-radius: 4px;
  cursor: pointer;
}

.fc .fc-daygrid-more-link {
  color: #0d6efd;
}
</style>
