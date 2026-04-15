<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useDebtCreditsStore } from '@/stores/debtCredits'
import { DebtCreditType, type CreateDebtCredit } from '@/types'

const store = useDebtCreditsStore()

const showModal = ref(false)
const editingId = ref<number | null>(null)
const showSettled = ref(false)

const form = ref<CreateDebtCredit>({
  type: DebtCreditType.Debt,
  personName: '',
  amount: 0,
  description: '',
  dueDate: null,
  notes: null
})

onMounted(() => store.fetchAll())

const activeItems = computed(() => store.items.filter((i) => !i.isSettled))
const settledItems = computed(() => store.items.filter((i) => i.isSettled))

const totalDebts = computed(() =>
  activeItems.value.filter((i) => i.type === DebtCreditType.Debt).reduce((s, i) => s + i.amount, 0)
)
const totalCredits = computed(() =>
  activeItems.value.filter((i) => i.type === DebtCreditType.Credit).reduce((s, i) => s + i.amount, 0)
)
const overdueItems = computed(() => activeItems.value.filter((i) => i.isOverdue))

function openModal(id?: number) {
  if (id) {
    const item = store.items.find((i) => i.id === id)!
    editingId.value = id
    form.value = {
      type: item.type,
      personName: item.personName,
      amount: item.amount,
      description: item.description,
      dueDate: item.dueDate ? item.dueDate.substring(0, 10) : null,
      notes: item.notes ?? null
    }
  } else {
    editingId.value = null
    form.value = { type: DebtCreditType.Debt, personName: '', amount: 0, description: '', dueDate: null, notes: null }
  }
  showModal.value = true
}

async function save() {
  if (editingId.value) {
    await store.update(editingId.value, form.value)
  } else {
    await store.create(form.value)
  }
  showModal.value = false
}

async function settle(id: number) {
  await store.settle(id)
}

async function reopen(id: number) {
  await store.reopen(id)
}

async function remove(id: number) {
  if (confirm('Eliminare questo elemento?')) await store.remove(id)
}

function formatCurrency(v: number) {
  return new Intl.NumberFormat('it-IT', { style: 'currency', currency: 'EUR' }).format(v)
}

function formatDate(d: string | null) {
  if (!d) return '–'
  return new Date(d).toLocaleDateString('it-IT')
}

function dueBadge(item: { isOverdue: boolean; daysUntilDue: number | null }) {
  if (item.isOverdue) return { cls: 'bg-danger', label: 'Scaduto' }
  if (item.daysUntilDue === null) return null
  if (item.daysUntilDue === 0) return { cls: 'bg-warning text-dark', label: 'Oggi' }
  if (item.daysUntilDue <= 7) return { cls: 'bg-warning text-dark', label: `${item.daysUntilDue}gg` }
  return { cls: 'bg-secondary', label: `${item.daysUntilDue}gg` }
}
</script>

<template>
  <div class="py-4">
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h1 class="h3 mb-0">
        <i class="bi bi-arrow-left-right me-2"></i>Debiti e Crediti
      </h1>
      <button class="btn btn-primary" @click="openModal()">
        <i class="bi bi-plus-lg me-1"></i>Nuovo
      </button>
    </div>

    <!-- KPI -->
    <div class="row g-3 mb-4">
      <div class="col-6 col-md-3">
        <div class="card border-0 shadow-sm h-100">
          <div class="card-body py-3 d-flex align-items-center gap-3">
            <div class="rounded-3 p-2 bg-danger bg-opacity-10">
              <i class="bi bi-arrow-up-right-circle-fill text-danger fs-5"></i>
            </div>
            <div>
              <p class="text-muted small mb-0">Devo io</p>
              <p class="fw-bold text-danger mb-0">{{ formatCurrency(totalDebts) }}</p>
            </div>
          </div>
        </div>
      </div>
      <div class="col-6 col-md-3">
        <div class="card border-0 shadow-sm h-100">
          <div class="card-body py-3 d-flex align-items-center gap-3">
            <div class="rounded-3 p-2 bg-success bg-opacity-10">
              <i class="bi bi-arrow-down-left-circle-fill text-success fs-5"></i>
            </div>
            <div>
              <p class="text-muted small mb-0">Mi devono</p>
              <p class="fw-bold text-success mb-0">{{ formatCurrency(totalCredits) }}</p>
            </div>
          </div>
        </div>
      </div>
      <div class="col-6 col-md-3">
        <div class="card border-0 shadow-sm h-100">
          <div class="card-body py-3 d-flex align-items-center gap-3">
            <div class="rounded-3 p-2 bg-primary bg-opacity-10">
              <i class="bi bi-wallet2 text-primary fs-5"></i>
            </div>
            <div>
              <p class="text-muted small mb-0">Saldo netto</p>
              <p class="fw-bold mb-0" :class="totalCredits - totalDebts >= 0 ? 'text-primary' : 'text-danger'">
                {{ formatCurrency(totalCredits - totalDebts) }}
              </p>
            </div>
          </div>
        </div>
      </div>
      <div class="col-6 col-md-3">
        <div class="card border-0 shadow-sm h-100">
          <div class="card-body py-3 d-flex align-items-center gap-3">
            <div class="rounded-3 p-2" :class="overdueItems.length > 0 ? 'bg-danger bg-opacity-10' : 'bg-secondary bg-opacity-10'">
              <i class="bi bi-exclamation-triangle-fill fs-5" :class="overdueItems.length > 0 ? 'text-danger' : 'text-secondary'"></i>
            </div>
            <div>
              <p class="text-muted small mb-0">Scaduti</p>
              <p class="fw-bold mb-0" :class="overdueItems.length > 0 ? 'text-danger' : 'text-secondary'">
                {{ overdueItems.length }}
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Alert scaduti -->
    <div v-if="overdueItems.length > 0" class="alert alert-danger d-flex align-items-center gap-2 mb-4">
      <i class="bi bi-exclamation-triangle-fill fs-5"></i>
      <div>
        <strong>{{ overdueItems.length }} elemento/i scaduto/i:</strong>
        {{ overdueItems.map(i => `${i.personName} (${formatCurrency(i.amount)})`).join(', ') }}
      </div>
    </div>

    <!-- Lista attivi -->
    <div v-if="store.loading" class="text-center py-5">
      <div class="spinner-border text-primary"></div>
    </div>
    <template v-else>
      <!-- Debiti -->
      <div class="mb-4">
        <h6 class="fw-semibold text-danger mb-3">
          <i class="bi bi-arrow-up-right-circle me-2"></i>Devo io
          <span class="badge bg-danger ms-1">{{ activeItems.filter(i => i.type === DebtCreditType.Debt).length }}</span>
        </h6>
        <div v-if="!activeItems.filter(i => i.type === DebtCreditType.Debt).length" class="text-muted small ps-2">
          Nessun debito attivo.
        </div>
        <div class="row g-2">
          <div
            v-for="item in activeItems.filter(i => i.type === DebtCreditType.Debt)"
            :key="item.id"
            class="col-12 col-md-6 col-xl-4"
          >
            <div class="card border-0 shadow-sm h-100" :class="item.isOverdue ? 'border-danger border-start border-3' : ''">
              <div class="card-body">
                <div class="d-flex justify-content-between align-items-start mb-2">
                  <div>
                    <p class="fw-semibold mb-0">{{ item.personName }}</p>
                    <p class="text-muted small mb-0">{{ item.description }}</p>
                  </div>
                  <span class="fw-bold text-danger fs-5">{{ formatCurrency(item.amount) }}</span>
                </div>
                <div class="d-flex align-items-center gap-2 mb-3 flex-wrap">
                  <span v-if="item.dueDate" class="text-muted small">
                    <i class="bi bi-calendar3 me-1"></i>{{ formatDate(item.dueDate) }}
                  </span>
                  <span v-if="dueBadge(item)" class="badge" :class="dueBadge(item)!.cls">
                    {{ dueBadge(item)!.label }}
                  </span>
                  <span v-if="item.notes" class="text-muted small" :title="item.notes">
                    <i class="bi bi-chat-left-text"></i>
                  </span>
                </div>
                <div class="d-flex gap-2">
                  <button class="btn btn-sm btn-success flex-fill" @click="settle(item.id)">
                    <i class="bi bi-check-lg me-1"></i>Saldato
                  </button>
                  <button class="btn btn-sm btn-outline-primary" @click="openModal(item.id)">
                    <i class="bi bi-pencil"></i>
                  </button>
                  <button class="btn btn-sm btn-outline-danger" @click="remove(item.id)">
                    <i class="bi bi-trash"></i>
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Crediti -->
      <div class="mb-4">
        <h6 class="fw-semibold text-success mb-3">
          <i class="bi bi-arrow-down-left-circle me-2"></i>Mi devono
          <span class="badge bg-success ms-1">{{ activeItems.filter(i => i.type === DebtCreditType.Credit).length }}</span>
        </h6>
        <div v-if="!activeItems.filter(i => i.type === DebtCreditType.Credit).length" class="text-muted small ps-2">
          Nessun credito attivo.
        </div>
        <div class="row g-2">
          <div
            v-for="item in activeItems.filter(i => i.type === DebtCreditType.Credit)"
            :key="item.id"
            class="col-12 col-md-6 col-xl-4"
          >
            <div class="card border-0 shadow-sm h-100" :class="item.isOverdue ? 'border-danger border-start border-3' : ''">
              <div class="card-body">
                <div class="d-flex justify-content-between align-items-start mb-2">
                  <div>
                    <p class="fw-semibold mb-0">{{ item.personName }}</p>
                    <p class="text-muted small mb-0">{{ item.description }}</p>
                  </div>
                  <span class="fw-bold text-success fs-5">{{ formatCurrency(item.amount) }}</span>
                </div>
                <div class="d-flex align-items-center gap-2 mb-3 flex-wrap">
                  <span v-if="item.dueDate" class="text-muted small">
                    <i class="bi bi-calendar3 me-1"></i>{{ formatDate(item.dueDate) }}
                  </span>
                  <span v-if="dueBadge(item)" class="badge" :class="dueBadge(item)!.cls">
                    {{ dueBadge(item)!.label }}
                  </span>
                  <span v-if="item.notes" class="text-muted small" :title="item.notes ?? ''">
                    <i class="bi bi-chat-left-text"></i>
                  </span>
                </div>
                <div class="d-flex gap-2">
                  <button class="btn btn-sm btn-success flex-fill" @click="settle(item.id)">
                    <i class="bi bi-check-lg me-1"></i>Riscosso
                  </button>
                  <button class="btn btn-sm btn-outline-primary" @click="openModal(item.id)">
                    <i class="bi bi-pencil"></i>
                  </button>
                  <button class="btn btn-sm btn-outline-danger" @click="remove(item.id)">
                    <i class="bi bi-trash"></i>
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Storico saldati -->
      <div v-if="settledItems.length > 0">
        <button class="btn btn-link text-muted p-0 mb-3" @click="showSettled = !showSettled">
          <i class="bi me-1" :class="showSettled ? 'bi-chevron-up' : 'bi-chevron-down'"></i>
          Storico saldati ({{ settledItems.length }})
        </button>
        <div v-if="showSettled" class="card border-0 shadow-sm">
          <div class="table-responsive">
            <table class="table table-sm table-hover align-middle mb-0">
              <thead class="table-light">
                <tr>
                  <th>Tipo</th>
                  <th>Persona</th>
                  <th>Descrizione</th>
                  <th class="text-end">Importo</th>
                  <th>Saldato il</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="item in settledItems" :key="item.id" class="text-muted">
                  <td>
                    <span class="badge" :class="item.type === DebtCreditType.Debt ? 'bg-danger' : 'bg-success'">
                      {{ item.type === DebtCreditType.Debt ? 'Debito' : 'Credito' }}
                    </span>
                  </td>
                  <td>{{ item.personName }}</td>
                  <td class="small">{{ item.description }}</td>
                  <td class="text-end">{{ formatCurrency(item.amount) }}</td>
                  <td class="small">{{ formatDate(item.settledAt) }}</td>
                  <td>
                    <button class="btn btn-sm btn-outline-secondary me-1" @click="reopen(item.id)" title="Riapri">
                      <i class="bi bi-arrow-counterclockwise"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-danger" @click="remove(item.id)">
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

    <!-- Modal -->
    <div v-if="showModal" class="modal show d-block" tabindex="-1" style="background:rgba(0,0,0,0.5)">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">{{ editingId ? 'Modifica' : 'Nuovo' }}</h5>
            <button type="button" class="btn-close" @click="showModal = false"></button>
          </div>
          <form @submit.prevent="save">
            <div class="modal-body">
              <!-- Tipo -->
              <div class="mb-3">
                <label class="form-label">Tipo</label>
                <div class="btn-group w-100">
                  <button
                    type="button" class="btn"
                    :class="form.type === DebtCreditType.Debt ? 'btn-danger' : 'btn-outline-danger'"
                    @click="form.type = DebtCreditType.Debt"
                  >
                    <i class="bi bi-arrow-up-right-circle me-1"></i>Devo io
                  </button>
                  <button
                    type="button" class="btn"
                    :class="form.type === DebtCreditType.Credit ? 'btn-success' : 'btn-outline-success'"
                    @click="form.type = DebtCreditType.Credit"
                  >
                    <i class="bi bi-arrow-down-left-circle me-1"></i>Mi devono
                  </button>
                </div>
              </div>
              <!-- Persona -->
              <div class="mb-3">
                <label class="form-label">Persona</label>
                <input v-model="form.personName" type="text" class="form-control" placeholder="Nome" required />
              </div>
              <!-- Importo -->
              <div class="mb-3">
                <label class="form-label">Importo</label>
                <div class="input-group">
                  <span class="input-group-text">€</span>
                  <input v-model.number="form.amount" type="number" step="0.01" min="0.01" class="form-control" required />
                </div>
              </div>
              <!-- Descrizione -->
              <div class="mb-3">
                <label class="form-label">Descrizione</label>
                <input v-model="form.description" type="text" class="form-control" required />
              </div>
              <!-- Scadenza -->
              <div class="mb-3">
                <label class="form-label">Scadenza (opzionale)</label>
                <input v-model="form.dueDate" type="date" class="form-control" />
              </div>
              <!-- Note -->
              <div class="mb-3">
                <label class="form-label">Note (opzionale)</label>
                <textarea v-model="form.notes" class="form-control" rows="2"></textarea>
              </div>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" @click="showModal = false">Annulla</button>
              <button type="submit" class="btn btn-primary">{{ editingId ? 'Salva' : 'Aggiungi' }}</button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>
