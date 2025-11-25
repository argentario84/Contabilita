<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useScheduledExpensesStore } from '@/stores/scheduledExpenses'
import { useCategoriesStore } from '@/stores/categories'
import { useTransactionsStore } from '@/stores/transactions'
import { RecurrenceType, TransactionType, type CreateScheduledExpense } from '@/types'

const scheduledStore = useScheduledExpensesStore()
const categoriesStore = useCategoriesStore()
const transactionsStore = useTransactionsStore()

const loading = ref(true)
const showModal = ref(false)
const showConfirmModal = ref(false)
const editingExpense = ref<number | null>(null)
const confirmingExpense = ref<number | null>(null)

const confirmForm = ref({
  actualAmount: 0,
  notes: ''
})

const form = ref({
  name: '',
  amount: 0,
  description: '',
  recurrence: RecurrenceType.Monthly,
  startDate: new Date().toISOString().split('T')[0] as string,
  endDate: undefined as string | undefined,
  categoryId: 0
})

const expenseCategories = computed(() =>
  categoriesStore.categories.filter((c) => c.type === TransactionType.Expense)
)

onMounted(async () => {
  await Promise.all([
    scheduledStore.fetchScheduledExpenses(),
    scheduledStore.fetchDueExpenses(),
    categoriesStore.fetchCategories()
  ])
  loading.value = false
})

const recurrenceLabels: Record<RecurrenceType, string> = {
  [RecurrenceType.Daily]: 'Giornaliera',
  [RecurrenceType.Weekly]: 'Settimanale',
  [RecurrenceType.Monthly]: 'Mensile',
  [RecurrenceType.Yearly]: 'Annuale'
}

function openModal(expenseId?: number) {
  if (expenseId) {
    const e = scheduledStore.scheduledExpenses.find((e) => e.id === expenseId)
    if (e) {
      editingExpense.value = expenseId
      form.value = {
        name: e.name,
        amount: e.amount,
        description: e.description || '',
        recurrence: e.recurrence,
        startDate: e.startDate.split('T')[0] as string,
        endDate: e.endDate?.split('T')[0] as string | undefined,
        categoryId: e.categoryId
      }
    }
  } else {
    editingExpense.value = null
    form.value = {
      name: '',
      amount: 0,
      description: '',
      recurrence: RecurrenceType.Monthly,
      startDate: new Date().toISOString().split('T')[0] as string,
      endDate: undefined,
      categoryId: 0
    }
  }
  showModal.value = true
}

function openConfirmModal(expenseId: number) {
  const e = scheduledStore.scheduledExpenses.find((e) => e.id === expenseId) ||
            scheduledStore.dueExpenses.find((e) => e.id === expenseId)
  if (e) {
    confirmingExpense.value = expenseId
    confirmForm.value = {
      actualAmount: e.amount,
      notes: ''
    }
    showConfirmModal.value = true
  }
}

async function saveExpense() {
  if (editingExpense.value) {
    await scheduledStore.updateScheduledExpense(editingExpense.value, form.value)
  } else {
    await scheduledStore.createScheduledExpense(form.value)
  }
  showModal.value = false
}

async function confirmExpense() {
  if (confirmingExpense.value) {
    await scheduledStore.confirmExpense(confirmingExpense.value, {
      actualAmount: confirmForm.value.actualAmount,
      notes: confirmForm.value.notes || undefined
    })
    showConfirmModal.value = false
    await transactionsStore.fetchTransactions()
  }
}

async function skipExpense(id: number) {
  await scheduledStore.skipExpense(id)
}

async function toggleActive(id: number, currentState: boolean) {
  await scheduledStore.updateScheduledExpense(id, { isActive: !currentState })
  await scheduledStore.fetchScheduledExpenses()
}

async function deleteExpense(id: number) {
  if (confirm('Sei sicuro di voler eliminare questa spesa programmata?')) {
    await scheduledStore.deleteScheduledExpense(id)
  }
}

function formatCurrency(value: number) {
  return new Intl.NumberFormat('it-IT', { style: 'currency', currency: 'EUR' }).format(value)
}

function formatDate(date: string) {
  return new Date(date).toLocaleDateString('it-IT')
}
</script>

<template>
  <div class="py-4">
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h1 class="h3 mb-0">
        <i class="bi bi-calendar-check me-2"></i>
        Spese Programmate
      </h1>
      <button class="btn btn-primary" @click="openModal()">
        <i class="bi bi-plus-lg me-1"></i>
        Nuova Spesa Programmata
      </button>
    </div>

    <div v-if="loading" class="text-center py-5">
      <div class="spinner-border text-primary"></div>
    </div>

    <template v-else>
      <!-- Alert Spese in Scadenza -->
      <div v-if="scheduledStore.dueExpenses.length > 0" class="card border-warning mb-4">
        <div class="card-header bg-warning text-dark">
          <h5 class="mb-0">
            <i class="bi bi-exclamation-triangle me-2"></i>
            Spese da confermare ({{ scheduledStore.dueExpenses.length }})
          </h5>
        </div>
        <div class="card-body p-0">
          <div class="list-group list-group-flush">
            <div
              v-for="expense in scheduledStore.dueExpenses"
              :key="expense.id"
              class="list-group-item d-flex justify-content-between align-items-center"
            >
              <div>
                <div class="fw-bold">{{ expense.name }}</div>
                <small class="text-muted">
                  {{ expense.categoryName }} - Scadenza: {{ formatDate(expense.nextDueDate) }}
                </small>
              </div>
              <div class="d-flex align-items-center gap-2">
                <span class="fw-bold">{{ formatCurrency(expense.amount) }}</span>
                <button
                  class="btn btn-sm btn-success"
                  @click="openConfirmModal(expense.id)"
                >
                  <i class="bi bi-check-lg me-1"></i>
                  Conferma
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
      </div>

      <!-- Lista Spese Programmate -->
      <div class="card border-0 shadow-sm">
        <div class="card-header bg-transparent">
          <h5 class="mb-0">Tutte le Spese Programmate</h5>
        </div>
        <div class="card-body p-0">
          <div v-if="scheduledStore.scheduledExpenses.length === 0" class="text-center text-muted py-5">
            Nessuna spesa programmata
          </div>
          <div v-else class="table-responsive">
            <table class="table table-hover mb-0">
              <thead class="table-light">
                <tr>
                  <th>Nome</th>
                  <th>Categoria</th>
                  <th>Ricorrenza</th>
                  <th>Prossima Scadenza</th>
                  <th class="text-end">Importo</th>
                  <th>Stato</th>
                  <th class="text-end">Azioni</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="expense in scheduledStore.scheduledExpenses"
                  :key="expense.id"
                  :class="{ 'table-warning': expense.isDueToday }"
                >
                  <td>
                    <div class="fw-medium">{{ expense.name }}</div>
                    <small v-if="expense.description" class="text-muted">{{ expense.description }}</small>
                  </td>
                  <td>
                    <span
                      class="badge"
                      :style="{ backgroundColor: expense.categoryColor || '#6c757d' }"
                    >
                      {{ expense.categoryName }}
                    </span>
                  </td>
                  <td>{{ recurrenceLabels[expense.recurrence] }}</td>
                  <td>{{ formatDate(expense.nextDueDate) }}</td>
                  <td class="text-end fw-bold">{{ formatCurrency(expense.amount) }}</td>
                  <td>
                    <span
                      class="badge"
                      :class="expense.isActive ? 'bg-success' : 'bg-secondary'"
                    >
                      {{ expense.isActive ? 'Attiva' : 'Disattiva' }}
                    </span>
                  </td>
                  <td class="text-end">
                    <button
                      v-if="expense.isDueToday"
                      class="btn btn-sm btn-success me-1"
                      @click="openConfirmModal(expense.id)"
                    >
                      <i class="bi bi-check"></i>
                    </button>
                    <button
                      class="btn btn-sm btn-outline-secondary me-1"
                      @click="toggleActive(expense.id, expense.isActive)"
                      :title="expense.isActive ? 'Disattiva' : 'Attiva'"
                    >
                      <i :class="expense.isActive ? 'bi-pause' : 'bi-play'"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-primary me-1" @click="openModal(expense.id)">
                      <i class="bi bi-pencil"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-danger" @click="deleteExpense(expense.id)">
                      <i class="bi bi-trash"></i>
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </template>

    <!-- Modal Nuova/Modifica Spesa -->
    <div v-if="showModal" class="modal show d-block" tabindex="-1" style="background: rgba(0,0,0,0.5)">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">
              {{ editingExpense ? 'Modifica Spesa Programmata' : 'Nuova Spesa Programmata' }}
            </h5>
            <button type="button" class="btn-close" @click="showModal = false"></button>
          </div>
          <form @submit.prevent="saveExpense">
            <div class="modal-body">
              <div class="mb-3">
                <label class="form-label">Nome</label>
                <input v-model="form.name" type="text" class="form-control" required />
              </div>

              <div class="mb-3">
                <label class="form-label">Importo</label>
                <div class="input-group">
                  <span class="input-group-text">€</span>
                  <input
                    v-model.number="form.amount"
                    type="number"
                    step="0.01"
                    min="0.01"
                    class="form-control"
                    required
                  />
                </div>
              </div>

              <div class="mb-3">
                <label class="form-label">Categoria</label>
                <select v-model="form.categoryId" class="form-select" required>
                  <option value="0" disabled>Seleziona categoria</option>
                  <option v-for="cat in expenseCategories" :key="cat.id" :value="cat.id">
                    {{ cat.name }}
                  </option>
                </select>
              </div>

              <div class="mb-3">
                <label class="form-label">Ricorrenza</label>
                <select v-model="form.recurrence" class="form-select" required>
                  <option :value="RecurrenceType.Daily">Giornaliera</option>
                  <option :value="RecurrenceType.Weekly">Settimanale</option>
                  <option :value="RecurrenceType.Monthly">Mensile</option>
                  <option :value="RecurrenceType.Yearly">Annuale</option>
                </select>
              </div>

              <div class="row">
                <div class="col-md-6 mb-3">
                  <label class="form-label">Data Inizio</label>
                  <input
                    v-model="form.startDate"
                    type="date"
                    class="form-control"
                    required
                    :disabled="!!editingExpense"
                  />
                </div>
                <div class="col-md-6 mb-3">
                  <label class="form-label">Data Fine (opzionale)</label>
                  <input v-model="form.endDate" type="date" class="form-control" />
                </div>
              </div>

              <div class="mb-3">
                <label class="form-label">Descrizione (opzionale)</label>
                <textarea v-model="form.description" class="form-control" rows="2"></textarea>
              </div>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" @click="showModal = false">
                Annulla
              </button>
              <button type="submit" class="btn btn-primary">
                {{ editingExpense ? 'Salva' : 'Aggiungi' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Modal Conferma Spesa -->
    <div v-if="showConfirmModal" class="modal show d-block" tabindex="-1" style="background: rgba(0,0,0,0.5)">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Conferma Spesa</h5>
            <button type="button" class="btn-close" @click="showConfirmModal = false"></button>
          </div>
          <form @submit.prevent="confirmExpense">
            <div class="modal-body">
              <p class="text-muted">
                Conferma la spesa e registra la transazione. Puoi modificare l'importo se necessario.
              </p>

              <div class="mb-3">
                <label class="form-label">Importo Effettivo</label>
                <div class="input-group">
                  <span class="input-group-text">€</span>
                  <input
                    v-model.number="confirmForm.actualAmount"
                    type="number"
                    step="0.01"
                    min="0.01"
                    class="form-control"
                    required
                  />
                </div>
              </div>

              <div class="mb-3">
                <label class="form-label">Note (opzionale)</label>
                <textarea v-model="confirmForm.notes" class="form-control" rows="2"></textarea>
              </div>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" @click="showConfirmModal = false">
                Annulla
              </button>
              <button type="submit" class="btn btn-success">
                <i class="bi bi-check-lg me-1"></i>
                Conferma e Registra
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>
