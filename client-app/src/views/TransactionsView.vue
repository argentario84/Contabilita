<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useTransactionsStore } from '@/stores/transactions'
import { useCategoriesStore } from '@/stores/categories'
import { TransactionType, type CreateTransaction } from '@/types'

const transactionsStore = useTransactionsStore()
const categoriesStore = useCategoriesStore()

const loading = ref(true)
const showModal = ref(false)
const editingTransaction = ref<number | null>(null)

const filters = ref({
  startDate: '',
  endDate: '',
  type: '' as '' | TransactionType
})

const form = ref({
  amount: 0,
  description: '',
  date: new Date().toISOString().split('T')[0] as string,
  type: TransactionType.Expense,
  notes: '',
  categoryId: 0
})

const filteredCategories = computed(() =>
  categoriesStore.categories.filter((c) => c.type === form.value.type)
)

onMounted(async () => {
  const now = new Date()
  filters.value.startDate = new Date(now.getFullYear(), now.getMonth(), 1).toISOString().split('T')[0] as string
  filters.value.endDate = new Date(now.getFullYear(), now.getMonth() + 1, 0).toISOString().split('T')[0] as string

  await Promise.all([
    loadTransactions(),
    categoriesStore.fetchCategories()
  ])
  loading.value = false
})

async function loadTransactions() {
  const params: any = {}
  if (filters.value.startDate) params.startDate = filters.value.startDate
  if (filters.value.endDate) params.endDate = filters.value.endDate
  if (filters.value.type) params.type = filters.value.type
  await transactionsStore.fetchTransactions(params)
}

watch(filters, loadTransactions, { deep: true })

function openModal(transactionId?: number) {
  if (transactionId) {
    const t = transactionsStore.transactions.find((t) => t.id === transactionId)
    if (t) {
      editingTransaction.value = transactionId
      form.value = {
        amount: t.amount,
        description: t.description,
        date: t.date.split('T')[0] as string,
        type: t.type,
        notes: t.notes || '',
        categoryId: t.categoryId
      }
    }
  } else {
    editingTransaction.value = null
    form.value = {
      amount: 0,
      description: '',
      date: new Date().toISOString().split('T')[0] as string,
      type: TransactionType.Expense,
      notes: '',
      categoryId: 0
    }
  }
  showModal.value = true
}

async function saveTransaction() {
  if (editingTransaction.value) {
    await transactionsStore.updateTransaction(editingTransaction.value, form.value)
  } else {
    await transactionsStore.createTransaction(form.value)
  }
  showModal.value = false
  await loadTransactions()
}

async function deleteTransaction(id: number) {
  if (confirm('Sei sicuro di voler eliminare questa transazione?')) {
    await transactionsStore.deleteTransaction(id)
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
        <i class="bi bi-cash-stack me-2"></i>
        Transazioni
      </h1>
      <button class="btn btn-primary" @click="openModal()">
        <i class="bi bi-plus-lg me-1"></i>
        Nuova Transazione
      </button>
    </div>

    <!-- Filtri -->
    <div class="card border-0 shadow-sm mb-4">
      <div class="card-body">
        <div class="row g-3">
          <div class="col-md-4">
            <label class="form-label">Data Inizio</label>
            <input v-model="filters.startDate" type="date" class="form-control" />
          </div>
          <div class="col-md-4">
            <label class="form-label">Data Fine</label>
            <input v-model="filters.endDate" type="date" class="form-control" />
          </div>
          <div class="col-md-4">
            <label class="form-label">Tipo</label>
            <select v-model="filters.type" class="form-select">
              <option value="">Tutti</option>
              <option :value="TransactionType.Income">Entrate</option>
              <option :value="TransactionType.Expense">Uscite</option>
            </select>
          </div>
        </div>
      </div>
    </div>

    <!-- Tabella Transazioni -->
    <div class="card border-0 shadow-sm">
      <div class="card-body p-0">
        <div v-if="loading" class="text-center py-5">
          <div class="spinner-border text-primary"></div>
        </div>
        <div v-else-if="transactionsStore.transactions.length === 0" class="text-center py-5 text-muted">
          Nessuna transazione trovata
        </div>
        <div v-else class="table-responsive">
          <table class="table table-hover mb-0">
            <thead class="table-light">
              <tr>
                <th>Data</th>
                <th>Descrizione</th>
                <th>Categoria</th>
                <th>Tipo</th>
                <th class="text-end">Importo</th>
                <th class="text-end">Azioni</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="t in transactionsStore.transactions" :key="t.id">
                <td>{{ formatDate(t.date) }}</td>
                <td>
                  {{ t.description }}
                  <small v-if="t.notes" class="d-block text-muted">{{ t.notes }}</small>
                </td>
                <td>
                  <span
                    class="badge"
                    :style="{ backgroundColor: t.categoryColor || '#6c757d' }"
                  >
                    {{ t.categoryName }}
                  </span>
                </td>
                <td>
                  <span
                    class="badge"
                    :class="t.type === TransactionType.Income ? 'bg-success' : 'bg-danger'"
                  >
                    {{ t.type === TransactionType.Income ? 'Entrata' : 'Uscita' }}
                  </span>
                </td>
                <td
                  class="text-end fw-bold"
                  :class="t.type === TransactionType.Income ? 'text-success' : 'text-danger'"
                >
                  {{ t.type === TransactionType.Income ? '+' : '-' }}{{ formatCurrency(t.amount) }}
                </td>
                <td class="text-end">
                  <button class="btn btn-sm btn-outline-primary me-1" @click="openModal(t.id)">
                    <i class="bi bi-pencil"></i>
                  </button>
                  <button class="btn btn-sm btn-outline-danger" @click="deleteTransaction(t.id)">
                    <i class="bi bi-trash"></i>
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Modal -->
    <div v-if="showModal" class="modal show d-block" tabindex="-1" style="background: rgba(0,0,0,0.5)">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">
              {{ editingTransaction ? 'Modifica Transazione' : 'Nuova Transazione' }}
            </h5>
            <button type="button" class="btn-close" @click="showModal = false"></button>
          </div>
          <form @submit.prevent="saveTransaction">
            <div class="modal-body">
              <div class="mb-3">
                <label class="form-label">Tipo</label>
                <div class="btn-group w-100">
                  <button
                    type="button"
                    class="btn"
                    :class="form.type === TransactionType.Expense ? 'btn-danger' : 'btn-outline-danger'"
                    @click="form.type = TransactionType.Expense; form.categoryId = 0"
                  >
                    <i class="bi bi-arrow-down-circle me-1"></i>
                    Uscita
                  </button>
                  <button
                    type="button"
                    class="btn"
                    :class="form.type === TransactionType.Income ? 'btn-success' : 'btn-outline-success'"
                    @click="form.type = TransactionType.Income; form.categoryId = 0"
                  >
                    <i class="bi bi-arrow-up-circle me-1"></i>
                    Entrata
                  </button>
                </div>
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
                <label class="form-label">Descrizione</label>
                <input v-model="form.description" type="text" class="form-control" required />
              </div>

              <div class="mb-3">
                <label class="form-label">Data</label>
                <input v-model="form.date" type="date" class="form-control" required />
              </div>

              <div class="mb-3">
                <label class="form-label">Categoria</label>
                <select v-model="form.categoryId" class="form-select" required>
                  <option value="0" disabled>Seleziona categoria</option>
                  <option v-for="cat in filteredCategories" :key="cat.id" :value="cat.id">
                    {{ cat.name }}
                  </option>
                </select>
              </div>

              <div class="mb-3">
                <label class="form-label">Note (opzionale)</label>
                <textarea v-model="form.notes" class="form-control" rows="2"></textarea>
              </div>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" @click="showModal = false">
                Annulla
              </button>
              <button type="submit" class="btn btn-primary">
                {{ editingTransaction ? 'Salva' : 'Aggiungi' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>
