<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useTransactionsStore } from '@/stores/transactions'
import { useScheduledExpensesStore } from '@/stores/scheduledExpenses'
import { useCalendarEventsStore } from '@/stores/calendarEvents'
import { TransactionType } from '@/types'
import ChildcareWeekly from '@/components/ChildcareWeekly.vue'

const authStore = useAuthStore()
const transactionsStore = useTransactionsStore()
const scheduledStore = useScheduledExpensesStore()
const calendarStore = useCalendarEventsStore()

const loading = ref(true)

const currentMonth = computed(() => {
  const now = new Date()
  return new Date(now.getFullYear(), now.getMonth(), 1)
})

const endOfMonth = computed(() => {
  const now = new Date()
  return new Date(now.getFullYear(), now.getMonth() + 1, 0)
})

const nextWeek = computed(() => {
  const now = new Date()
  const next = new Date(now)
  next.setDate(next.getDate() + 7)
  return next
})

onMounted(async () => {
  try {
    const now = new Date()
    await Promise.all([
      transactionsStore.fetchSummary(
        currentMonth.value.toISOString(),
        endOfMonth.value.toISOString()
      ),
      transactionsStore.fetchTransactions({
        startDate: currentMonth.value.toISOString(),
        endDate: endOfMonth.value.toISOString()
      }),
      transactionsStore.fetchBudgetPlanning(),
      scheduledStore.fetchDueExpenses(),
      calendarStore.fetchEvents(now.toISOString(), nextWeek.value.toISOString())
    ])
  } finally {
    loading.value = false
  }
})

const upcomingEvents = computed(() => {
  const now = new Date()
  return calendarStore.events
    .filter(e => new Date(e.startDate) >= now)
    .sort((a, b) => new Date(a.startDate).getTime() - new Date(b.startDate).getTime())
    .slice(0, 5)
})

const recentTransactions = computed(() =>
  transactionsStore.transactions.slice(0, 5)
)

function formatCurrency(value: number) {
  return new Intl.NumberFormat('it-IT', {
    style: 'currency',
    currency: 'EUR'
  }).format(value)
}

function formatDate(date: string) {
  return new Date(date).toLocaleDateString('it-IT')
}

function formatDateTime(date: string) {
  const d = new Date(date)
  const today = new Date()
  const tomorrow = new Date(today)
  tomorrow.setDate(tomorrow.getDate() + 1)

  const isToday = d.toDateString() === today.toDateString()
  const isTomorrow = d.toDateString() === tomorrow.toDateString()

  const timeStr = d.toLocaleTimeString('it-IT', { hour: '2-digit', minute: '2-digit' })

  if (isToday) return `Oggi, ${timeStr}`
  if (isTomorrow) return `Domani, ${timeStr}`
  return d.toLocaleDateString('it-IT', { weekday: 'short', day: 'numeric', month: 'short' }) + `, ${timeStr}`
}

async function confirmExpense(id: number) {
  await scheduledStore.confirmExpense(id)
  await transactionsStore.fetchSummary(
    currentMonth.value.toISOString(),
    endOfMonth.value.toISOString()
  )
}

async function skipExpense(id: number) {
  await scheduledStore.skipExpense(id)
}

const budgetProgressClass = computed(() => {
  const bp = transactionsStore.budgetPlanning
  if (!bp) return 'bg-success'
  if (bp.isOverBudget) return 'bg-danger'
  if (bp.isOverThreshold) return 'bg-warning'
  return 'bg-success'
})
</script>

<template>
  <div class="py-4">
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h1 class="h3 mb-0">
        <i class="bi bi-speedometer2 me-2"></i>
        Dashboard
      </h1>
      <span class="text-muted">
        Benvenuto, {{ authStore.user?.firstName }}!
      </span>
    </div>

    <div v-if="loading" class="text-center py-5">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Caricamento...</span>
      </div>
    </div>

    <template v-else>
      <!-- Alert Spese in Scadenza -->
      <div v-if="scheduledStore.dueExpenses.length > 0" class="alert alert-warning mb-4">
        <h5 class="alert-heading">
          <i class="bi bi-exclamation-triangle me-2"></i>
          Spese da confermare oggi
        </h5>
        <div class="list-group list-group-flush">
          <div
            v-for="expense in scheduledStore.dueExpenses"
            :key="expense.id"
            class="list-group-item d-flex justify-content-between align-items-center bg-transparent border-0 px-0"
          >
            <div>
              <strong>{{ expense.name }}</strong>
              <span class="text-muted ms-2">{{ formatCurrency(expense.amount) }}</span>
            </div>
            <div>
              <button
                class="btn btn-sm btn-success me-2"
                @click="confirmExpense(expense.id)"
              >
                <i class="bi bi-check"></i> Conferma
              </button>
              <button
                class="btn btn-sm btn-outline-secondary"
                @click="skipExpense(expense.id)"
              >
                Salta
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Cards Riepilogo -->
      <div class="row g-4 mb-4">
        <div class="col-md-6 col-lg-4">
          <div class="card h-100 border-0 shadow-sm">
            <div class="card-body">
              <div class="d-flex justify-content-between">
                <div>
                  <p class="text-muted mb-1">Entrate (Mese)</p>
                  <h3 class="mb-0 text-success">{{ formatCurrency(transactionsStore.summary?.totalIncome || 0) }}</h3>
                </div>
                <div class="bg-success bg-opacity-10 rounded-circle p-3">
                  <i class="bi bi-arrow-up-circle text-success fs-4"></i>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="col-md-6 col-lg-4">
          <div class="card h-100 border-0 shadow-sm">
            <div class="card-body">
              <div class="d-flex justify-content-between">
                <div>
                  <p class="text-muted mb-1">Uscite (Mese)</p>
                  <h3 class="mb-0 text-danger">{{ formatCurrency(transactionsStore.summary?.totalExpenses || 0) }}</h3>
                </div>
                <div class="bg-danger bg-opacity-10 rounded-circle p-3">
                  <i class="bi bi-arrow-down-circle text-danger fs-4"></i>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="col-md-6 col-lg-4">
          <div class="card h-100 border-0 shadow-sm">
            <div class="card-body">
              <div class="d-flex justify-content-between">
                <div>
                  <p class="text-muted mb-1">Bilancio (Mese)</p>
                  <h3
                    class="mb-0"
                    :class="(transactionsStore.summary?.balance || 0) >= 0 ? 'text-success' : 'text-danger'"
                  >
                    {{ formatCurrency(transactionsStore.summary?.balance || 0) }}
                  </h3>
                </div>
                <div class="bg-info bg-opacity-10 rounded-circle p-3">
                  <i class="bi bi-graph-up text-info fs-4"></i>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Budget Planning -->
      <div v-if="transactionsStore.budgetPlanning && authStore.user?.monthlyIncome" class="card border-0 shadow-sm mb-4">
        <div class="card-header bg-transparent border-0 d-flex justify-content-between align-items-center">
          <h5 class="mb-0">
            <i class="bi bi-wallet2 me-2"></i>
            Pianificazione Budget
          </h5>
          <RouterLink to="/settings" class="btn btn-sm btn-outline-secondary">
            <i class="bi bi-gear"></i> Configura
          </RouterLink>
        </div>
        <div class="card-body">
          <!-- Alert se sopra soglia -->
          <div v-if="transactionsStore.budgetPlanning.isOverBudget" class="alert alert-danger mb-3">
            <i class="bi bi-exclamation-triangle-fill me-2"></i>
            <strong>Attenzione!</strong> Hai superato il budget disponibile per spese variabili.
          </div>
          <div v-else-if="transactionsStore.budgetPlanning.isOverThreshold" class="alert alert-warning mb-3">
            <i class="bi bi-exclamation-triangle me-2"></i>
            Hai usato il <strong>{{ transactionsStore.budgetPlanning.budgetPercentageUsed }}%</strong> del budget disponibile.
            Rallenta le spese!
          </div>

          <!-- Flusso Budget -->
          <div class="row text-center mb-4">
            <div class="col">
              <div class="text-muted small">Stipendio</div>
              <div class="fs-5 fw-bold text-success">{{ formatCurrency(transactionsStore.budgetPlanning.monthlyIncome) }}</div>
            </div>
            <div class="col-auto d-flex align-items-center">
              <i class="bi bi-arrow-right text-muted"></i>
            </div>
            <div class="col">
              <div class="text-muted small">Spese Fisse</div>
              <div class="fs-5 fw-bold text-danger">-{{ formatCurrency(transactionsStore.budgetPlanning.totalFixedExpenses) }}</div>
            </div>
            <div class="col-auto d-flex align-items-center">
              <i class="bi bi-arrow-right text-muted"></i>
            </div>
            <div class="col">
              <div class="text-muted small">Risparmio</div>
              <div class="fs-5 fw-bold text-info">-{{ formatCurrency(transactionsStore.budgetPlanning.savingsGoal) }}</div>
            </div>
            <div class="col-auto d-flex align-items-center">
              <i class="bi bi-arrow-right text-muted"></i>
            </div>
            <div class="col">
              <div class="text-muted small">Disponibile</div>
              <div class="fs-5 fw-bold" :class="transactionsStore.budgetPlanning.availableBudget >= 0 ? 'text-primary' : 'text-danger'">
                {{ formatCurrency(transactionsStore.budgetPlanning.availableBudget) }}
              </div>
            </div>
          </div>

          <!-- Progress Bar -->
          <div class="mb-3">
            <div class="d-flex justify-content-between mb-1">
              <span>Speso questo mese</span>
              <span>
                {{ formatCurrency(transactionsStore.budgetPlanning.spentThisMonth) }} / {{ formatCurrency(transactionsStore.budgetPlanning.availableBudget) }}
              </span>
            </div>
            <div class="progress" style="height: 20px">
              <div
                class="progress-bar"
                :class="budgetProgressClass"
                :style="{ width: Math.min(transactionsStore.budgetPlanning.budgetPercentageUsed, 100) + '%' }"
              >
                {{ transactionsStore.budgetPlanning.budgetPercentageUsed }}%
              </div>
            </div>
            <div class="d-flex justify-content-between mt-1">
              <small class="text-muted">0%</small>
              <small class="text-muted">Soglia {{ transactionsStore.budgetPlanning.alertThreshold }}%</small>
              <small class="text-muted">100%</small>
            </div>
          </div>

          <!-- Rimanente -->
          <div class="text-center">
            <span class="text-muted">Budget rimanente: </span>
            <span class="fs-4 fw-bold" :class="transactionsStore.budgetPlanning.remainingBudget >= 0 ? 'text-success' : 'text-danger'">
              {{ formatCurrency(transactionsStore.budgetPlanning.remainingBudget) }}
            </span>
          </div>
        </div>
      </div>

      <!-- Messaggio se non configurato -->
      <div v-else-if="!authStore.user?.monthlyIncome" class="alert alert-info mb-4">
        <i class="bi bi-info-circle me-2"></i>
        Configura il tuo stipendio mensile nelle <RouterLink to="/settings">Impostazioni</RouterLink> per visualizzare la pianificazione del budget.
      </div>

      <div class="row g-4">
        <!-- Spese per Categoria -->
        <div class="col-lg-6">
          <div class="card border-0 shadow-sm h-100">
            <div class="card-header bg-transparent border-0">
              <h5 class="mb-0">
                <i class="bi bi-pie-chart me-2"></i>
                Spese per Categoria
              </h5>
            </div>
            <div class="card-body">
              <div v-if="transactionsStore.summary?.expensesByCategory.length === 0" class="text-center text-muted py-4">
                Nessuna spesa questo mese
              </div>
              <div v-else>
                <div
                  v-for="cat in transactionsStore.summary?.expensesByCategory"
                  :key="cat.categoryId"
                  class="mb-3"
                >
                  <div class="d-flex justify-content-between mb-1">
                    <span>{{ cat.categoryName }}</span>
                    <span>{{ formatCurrency(cat.total) }} ({{ cat.percentage }}%)</span>
                  </div>
                  <div class="progress" style="height: 8px">
                    <div
                      class="progress-bar"
                      :style="{
                        width: cat.percentage + '%',
                        backgroundColor: cat.categoryColor || '#0d6efd'
                      }"
                    ></div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Ultime Transazioni -->
        <div class="col-lg-6">
          <div class="card border-0 shadow-sm h-100">
            <div class="card-header bg-transparent border-0 d-flex justify-content-between align-items-center">
              <h5 class="mb-0">
                <i class="bi bi-clock-history me-2"></i>
                Ultime Transazioni
              </h5>
              <RouterLink to="/transactions" class="btn btn-sm btn-outline-primary">
                Vedi tutte
              </RouterLink>
            </div>
            <div class="card-body p-0">
              <div v-if="recentTransactions.length === 0" class="text-center text-muted py-4">
                Nessuna transazione questo mese
              </div>
              <div v-else class="list-group list-group-flush">
                <div
                  v-for="transaction in recentTransactions"
                  :key="transaction.id"
                  class="list-group-item d-flex justify-content-between align-items-center"
                >
                  <div>
                    <div class="fw-medium">{{ transaction.description }}</div>
                    <small class="text-muted">
                      {{ transaction.categoryName }} - {{ formatDate(transaction.date) }}
                    </small>
                  </div>
                  <span
                    :class="transaction.type === TransactionType.Income ? 'text-success' : 'text-danger'"
                    class="fw-bold"
                  >
                    {{ transaction.type === TransactionType.Income ? '+' : '-' }}
                    {{ formatCurrency(transaction.amount) }}
                  </span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Prossimi Eventi -->
      <div class="row g-4 mt-2">
        <div class="col-12">
          <div class="card border-0 shadow-sm">
            <div class="card-header bg-transparent border-0 d-flex justify-content-between align-items-center">
              <h5 class="mb-0">
                <i class="bi bi-calendar-event me-2"></i>
                Prossimi Eventi
              </h5>
              <RouterLink to="/calendar" class="btn btn-sm btn-outline-primary">
                Vedi calendario
              </RouterLink>
            </div>
            <div class="card-body p-0">
              <div v-if="upcomingEvents.length === 0" class="text-center text-muted py-4">
                Nessun evento nei prossimi 7 giorni
              </div>
              <div v-else class="list-group list-group-flush">
                <div
                  v-for="event in upcomingEvents"
                  :key="event.id"
                  class="list-group-item d-flex align-items-center"
                >
                  <div
                    class="rounded-circle me-3"
                    :style="{
                      width: '12px',
                      height: '12px',
                      backgroundColor: event.color || '#0d6efd'
                    }"
                  ></div>
                  <div class="flex-grow-1">
                    <div class="fw-medium">
                      <i v-if="event.isShared" class="bi bi-people-fill text-primary me-1" title="Evento condiviso"></i>
                      {{ event.title }}
                    </div>
                    <small class="text-muted">
                      {{ event.allDay ? formatDate(event.startDate) : formatDateTime(event.startDate) }}
                      <span v-if="event.isShared && event.createdByName"> · {{ event.createdByName }}</span>
                    </small>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Gestione Settimanale Bambina -->
      <div class="row g-4 mt-2">
        <div class="col-12">
          <ChildcareWeekly />
        </div>
      </div>
    </template>
  </div>
</template>
