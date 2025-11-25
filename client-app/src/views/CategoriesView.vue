<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useCategoriesStore } from '@/stores/categories'
import { TransactionType } from '@/types'

const categoriesStore = useCategoriesStore()

const loading = ref(true)
const showModal = ref(false)
const editingCategory = ref<number | null>(null)

const colors = [
  '#0d6efd', '#6610f2', '#6f42c1', '#d63384', '#dc3545',
  '#fd7e14', '#ffc107', '#198754', '#20c997', '#0dcaf0'
]

const icons = [
  'bi-cart', 'bi-house', 'bi-car-front', 'bi-heart-pulse', 'bi-film',
  'bi-book', 'bi-phone', 'bi-gift', 'bi-briefcase', 'bi-piggy-bank',
  'bi-cup-hot', 'bi-airplane', 'bi-controller', 'bi-music-note', 'bi-basket'
]

const form = ref({
  name: '',
  description: '',
  color: '#0d6efd',
  icon: 'bi-tag',
  type: TransactionType.Expense,
  monthlyBudget: undefined as number | undefined
})

onMounted(async () => {
  await categoriesStore.fetchCategories()
  loading.value = false
})

function openModal(categoryId?: number) {
  if (categoryId) {
    const c = categoriesStore.categories.find((c) => c.id === categoryId)
    if (c) {
      editingCategory.value = categoryId
      form.value = {
        name: c.name,
        description: c.description || '',
        color: c.color || '#0d6efd',
        icon: c.icon || 'bi-tag',
        type: c.type,
        monthlyBudget: c.monthlyBudget
      }
    }
  } else {
    editingCategory.value = null
    form.value = {
      name: '',
      description: '',
      color: '#0d6efd',
      icon: 'bi-tag',
      type: TransactionType.Expense,
      monthlyBudget: undefined
    }
  }
  showModal.value = true
}

async function saveCategory() {
  if (editingCategory.value) {
    await categoriesStore.updateCategory(editingCategory.value, form.value)
  } else {
    await categoriesStore.createCategory(form.value)
  }
  showModal.value = false
  await categoriesStore.fetchCategories()
}

async function deleteCategory(id: number) {
  if (confirm('Sei sicuro di voler eliminare questa categoria?')) {
    await categoriesStore.deleteCategory(id)
  }
}

function formatCurrency(value: number) {
  return new Intl.NumberFormat('it-IT', { style: 'currency', currency: 'EUR' }).format(value)
}

const incomeCategories = () => categoriesStore.categories.filter((c) => c.type === TransactionType.Income)
const expenseCategories = () => categoriesStore.categories.filter((c) => c.type === TransactionType.Expense)
</script>

<template>
  <div class="py-4">
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h1 class="h3 mb-0">
        <i class="bi bi-tags me-2"></i>
        Categorie
      </h1>
      <button class="btn btn-primary" @click="openModal()">
        <i class="bi bi-plus-lg me-1"></i>
        Nuova Categoria
      </button>
    </div>

    <div v-if="loading" class="text-center py-5">
      <div class="spinner-border text-primary"></div>
    </div>

    <template v-else>
      <div class="row g-4">
        <!-- Categorie Uscite -->
        <div class="col-lg-6">
          <div class="card border-0 shadow-sm h-100">
            <div class="card-header bg-danger text-white">
              <h5 class="mb-0">
                <i class="bi bi-arrow-down-circle me-2"></i>
                Categorie Uscite
              </h5>
            </div>
            <div class="card-body p-0">
              <div v-if="expenseCategories().length === 0" class="text-center text-muted py-4">
                Nessuna categoria di uscita
              </div>
              <div v-else class="list-group list-group-flush">
                <div
                  v-for="cat in expenseCategories()"
                  :key="cat.id"
                  class="list-group-item"
                >
                  <div class="d-flex align-items-center">
                    <div
                      class="rounded-circle d-flex align-items-center justify-content-center me-3"
                      :style="{ backgroundColor: cat.color || '#6c757d', width: '40px', height: '40px' }"
                    >
                      <i :class="cat.icon || 'bi-tag'" class="text-white"></i>
                    </div>
                    <div class="flex-grow-1">
                      <div class="fw-medium">{{ cat.name }}</div>
                      <small v-if="cat.monthlyBudget" class="text-muted">
                        Budget: {{ formatCurrency(cat.monthlyBudget) }}/mese
                      </small>
                    </div>
                    <div>
                      <button class="btn btn-sm btn-outline-primary me-1" @click="openModal(cat.id)">
                        <i class="bi bi-pencil"></i>
                      </button>
                      <button class="btn btn-sm btn-outline-danger" @click="deleteCategory(cat.id)">
                        <i class="bi bi-trash"></i>
                      </button>
                    </div>
                  </div>
                  <!-- Progress bar budget -->
                  <div v-if="cat.monthlyBudget" class="mt-2">
                    <div class="d-flex justify-content-between small mb-1">
                      <span>Speso: {{ formatCurrency(cat.spentThisMonth) }}</span>
                      <span :class="(cat.remainingBudget || 0) < 0 ? 'text-danger' : 'text-success'">
                        {{ (cat.remainingBudget || 0) >= 0 ? 'Rimangono' : 'Sforato di' }}:
                        {{ formatCurrency(Math.abs(cat.remainingBudget || 0)) }}
                      </span>
                    </div>
                    <div class="progress" style="height: 8px">
                      <div
                        class="progress-bar"
                        :class="{
                          'bg-success': (cat.budgetPercentageUsed || 0) < 75,
                          'bg-warning': (cat.budgetPercentageUsed || 0) >= 75 && (cat.budgetPercentageUsed || 0) < 100,
                          'bg-danger': (cat.budgetPercentageUsed || 0) >= 100
                        }"
                        :style="{ width: Math.min(cat.budgetPercentageUsed || 0, 100) + '%' }"
                      ></div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Categorie Entrate -->
        <div class="col-lg-6">
          <div class="card border-0 shadow-sm h-100">
            <div class="card-header bg-success text-white">
              <h5 class="mb-0">
                <i class="bi bi-arrow-up-circle me-2"></i>
                Categorie Entrate
              </h5>
            </div>
            <div class="card-body p-0">
              <div v-if="incomeCategories().length === 0" class="text-center text-muted py-4">
                Nessuna categoria di entrata
              </div>
              <div v-else class="list-group list-group-flush">
                <div
                  v-for="cat in incomeCategories()"
                  :key="cat.id"
                  class="list-group-item d-flex align-items-center"
                >
                  <div
                    class="rounded-circle d-flex align-items-center justify-content-center me-3"
                    :style="{ backgroundColor: cat.color || '#6c757d', width: '40px', height: '40px' }"
                  >
                    <i :class="cat.icon || 'bi-tag'" class="text-white"></i>
                  </div>
                  <div class="flex-grow-1">
                    <div class="fw-medium">{{ cat.name }}</div>
                    <small v-if="cat.description" class="text-muted">{{ cat.description }}</small>
                  </div>
                  <div>
                    <button class="btn btn-sm btn-outline-primary me-1" @click="openModal(cat.id)">
                      <i class="bi bi-pencil"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-danger" @click="deleteCategory(cat.id)">
                      <i class="bi bi-trash"></i>
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </template>

    <!-- Modal -->
    <div v-if="showModal" class="modal show d-block" tabindex="-1" style="background: rgba(0,0,0,0.5)">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">
              {{ editingCategory ? 'Modifica Categoria' : 'Nuova Categoria' }}
            </h5>
            <button type="button" class="btn-close" @click="showModal = false"></button>
          </div>
          <form @submit.prevent="saveCategory">
            <div class="modal-body">
              <div class="mb-3">
                <label class="form-label">Tipo</label>
                <div class="btn-group w-100">
                  <button
                    type="button"
                    class="btn"
                    :class="form.type === TransactionType.Expense ? 'btn-danger' : 'btn-outline-danger'"
                    @click="form.type = TransactionType.Expense"
                  >
                    Uscita
                  </button>
                  <button
                    type="button"
                    class="btn"
                    :class="form.type === TransactionType.Income ? 'btn-success' : 'btn-outline-success'"
                    @click="form.type = TransactionType.Income"
                  >
                    Entrata
                  </button>
                </div>
              </div>

              <div class="mb-3">
                <label class="form-label">Nome</label>
                <input v-model="form.name" type="text" class="form-control" required />
              </div>

              <div class="mb-3">
                <label class="form-label">Descrizione (opzionale)</label>
                <textarea v-model="form.description" class="form-control" rows="2"></textarea>
              </div>

              <!-- Budget mensile solo per categorie Uscita -->
              <div v-if="form.type === TransactionType.Expense" class="mb-3">
                <label class="form-label">Budget Mensile (opzionale)</label>
                <div class="input-group">
                  <span class="input-group-text">€</span>
                  <input
                    v-model.number="form.monthlyBudget"
                    type="number"
                    step="0.01"
                    min="0"
                    class="form-control"
                    placeholder="Es. 100"
                  />
                </div>
                <small class="text-muted">Imposta un limite di spesa mensile per questa categoria</small>
              </div>

              <div class="mb-3">
                <label class="form-label">Colore</label>
                <div class="d-flex flex-wrap gap-2">
                  <button
                    v-for="color in colors"
                    :key="color"
                    type="button"
                    class="btn p-0 rounded-circle"
                    :class="{ 'ring ring-primary': form.color === color }"
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

              <div class="mb-3">
                <label class="form-label">Icona</label>
                <div class="d-flex flex-wrap gap-2">
                  <button
                    v-for="icon in icons"
                    :key="icon"
                    type="button"
                    class="btn btn-outline-secondary"
                    :class="{ 'btn-primary text-white': form.icon === icon }"
                    style="width: 40px; height: 40px"
                    @click="form.icon = icon"
                  >
                    <i :class="icon"></i>
                  </button>
                </div>
              </div>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" @click="showModal = false">
                Annulla
              </button>
              <button type="submit" class="btn btn-primary">
                {{ editingCategory ? 'Salva' : 'Aggiungi' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>
