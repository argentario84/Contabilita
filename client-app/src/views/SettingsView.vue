<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()

const form = ref({
  firstName: '',
  lastName: '',
  initialBudget: 0,
  monthlyIncome: 0,
  // Budget Planning
  useSavingsPercentage: false,
  savingsGoalAmount: 0,
  savingsGoalPercentage: 0,
  extraFixedExpenses: 0,
  budgetAlertThreshold: 80
})

const saving = ref(false)
const success = ref(false)
const error = ref('')

onMounted(() => {
  if (authStore.user) {
    form.value = {
      firstName: authStore.user.firstName,
      lastName: authStore.user.lastName,
      initialBudget: authStore.user.initialBudget,
      monthlyIncome: authStore.user.monthlyIncome,
      useSavingsPercentage: authStore.user.useSavingsPercentage || false,
      savingsGoalAmount: authStore.user.savingsGoalAmount || 0,
      savingsGoalPercentage: authStore.user.savingsGoalPercentage || 0,
      extraFixedExpenses: authStore.user.extraFixedExpenses || 0,
      budgetAlertThreshold: authStore.user.budgetAlertThreshold || 80
    }
  }
})

async function saveSettings() {
  saving.value = true
  success.value = false
  error.value = ''

  try {
    await authStore.updateProfile(form.value)
    success.value = true
    setTimeout(() => (success.value = false), 3000)
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Errore durante il salvataggio'
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <div class="py-4">
    <h1 class="h3 mb-4">
      <i class="bi bi-gear me-2"></i>
      Impostazioni
    </h1>

    <div class="row">
      <div class="col-lg-8">
        <div class="card border-0 shadow-sm">
          <div class="card-header bg-transparent">
            <h5 class="mb-0">Profilo Utente</h5>
          </div>
          <div class="card-body">
            <div v-if="success" class="alert alert-success">
              Impostazioni salvate con successo!
            </div>
            <div v-if="error" class="alert alert-danger">
              {{ error }}
            </div>

            <form @submit.prevent="saveSettings">
              <div class="row mb-3">
                <div class="col-md-6">
                  <label class="form-label">Nome</label>
                  <input v-model="form.firstName" type="text" class="form-control" required />
                </div>
                <div class="col-md-6">
                  <label class="form-label">Cognome</label>
                  <input v-model="form.lastName" type="text" class="form-control" required />
                </div>
              </div>

              <div class="mb-3">
                <label class="form-label">Email</label>
                <input
                  type="email"
                  class="form-control"
                  :value="authStore.user?.email"
                  disabled
                />
                <small class="text-muted">L'email non può essere modificata</small>
              </div>

              <hr />

              <h6 class="mb-3">Impostazioni Budget</h6>

              <div class="row mb-3">
                <div class="col-md-6">
                  <label class="form-label">Budget Iniziale</label>
                  <div class="input-group">
                    <span class="input-group-text">€</span>
                    <input
                      v-model.number="form.initialBudget"
                      type="number"
                      step="0.01"
                      class="form-control"
                    />
                  </div>
                  <small class="text-muted">
                    Il budget iniziale da cui partire per i calcoli
                  </small>
                </div>
                <div class="col-md-6">
                  <label class="form-label">Entrata Mensile (Stipendio)</label>
                  <div class="input-group">
                    <span class="input-group-text">€</span>
                    <input
                      v-model.number="form.monthlyIncome"
                      type="number"
                      step="0.01"
                      class="form-control"
                    />
                  </div>
                  <small class="text-muted">
                    L'entrata mensile prevista (es. stipendio)
                  </small>
                </div>
              </div>

              <hr />

              <h6 class="mb-3">
                <i class="bi bi-wallet2 me-2"></i>
                Pianificazione Budget
              </h6>

              <!-- Obiettivo Risparmio -->
              <div class="mb-3">
                <label class="form-label">Obiettivo Risparmio Mensile</label>
                <div class="form-check form-switch mb-2">
                  <input
                    v-model="form.useSavingsPercentage"
                    class="form-check-input"
                    type="checkbox"
                    id="useSavingsPercentage"
                  />
                  <label class="form-check-label" for="useSavingsPercentage">
                    Usa percentuale dello stipendio
                  </label>
                </div>
                <div class="row">
                  <div class="col-md-6">
                    <div v-if="!form.useSavingsPercentage" class="input-group">
                      <span class="input-group-text">€</span>
                      <input
                        v-model.number="form.savingsGoalAmount"
                        type="number"
                        step="0.01"
                        min="0"
                        class="form-control"
                        placeholder="Importo fisso"
                      />
                    </div>
                    <div v-else class="input-group">
                      <input
                        v-model.number="form.savingsGoalPercentage"
                        type="number"
                        step="1"
                        min="0"
                        max="100"
                        class="form-control"
                        placeholder="Percentuale"
                      />
                      <span class="input-group-text">%</span>
                    </div>
                    <small class="text-muted">
                      {{ form.useSavingsPercentage ? 'Percentuale dello stipendio da risparmiare' : 'Importo fisso mensile da mettere da parte' }}
                    </small>
                  </div>
                </div>
              </div>

              <!-- Spese Fisse Extra -->
              <div class="mb-3">
                <label class="form-label">Spese Fisse Extra</label>
                <div class="input-group" style="max-width: 300px;">
                  <span class="input-group-text">€</span>
                  <input
                    v-model.number="form.extraFixedExpenses"
                    type="number"
                    step="0.01"
                    min="0"
                    class="form-control"
                  />
                </div>
                <small class="text-muted">
                  Importo aggiuntivo alle spese programmate (es. spese non ricorrenti previste)
                </small>
              </div>

              <!-- Soglia Alert -->
              <div class="mb-4">
                <label class="form-label">Soglia Alert Budget</label>
                <div class="d-flex align-items-center">
                  <input
                    v-model.number="form.budgetAlertThreshold"
                    type="range"
                    class="form-range me-3"
                    min="50"
                    max="100"
                    step="5"
                    style="max-width: 300px;"
                  />
                  <span class="badge bg-warning fs-6">{{ form.budgetAlertThreshold }}%</span>
                </div>
                <small class="text-muted">
                  Riceverai un avviso quando avrai speso oltre questa percentuale del budget disponibile
                </small>
              </div>

              <div class="text-end">
                <button type="submit" class="btn btn-primary" :disabled="saving">
                  <span v-if="saving" class="spinner-border spinner-border-sm me-2"></span>
                  Salva Impostazioni
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>

      <div class="col-lg-4">
        <div class="card border-0 shadow-sm">
          <div class="card-header bg-transparent">
            <h5 class="mb-0">Info Account</h5>
          </div>
          <div class="card-body">
            <ul class="list-unstyled mb-0">
              <li class="mb-3">
                <small class="text-muted d-block">ID Utente</small>
                <code>{{ authStore.user?.id }}</code>
              </li>
              <li class="mb-3">
                <small class="text-muted d-block">Email</small>
                <span>{{ authStore.user?.email }}</span>
              </li>
              <li>
                <small class="text-muted d-block">Nome Completo</small>
                <span>{{ authStore.user?.firstName }} {{ authStore.user?.lastName }}</span>
              </li>
            </ul>
          </div>
        </div>

        <div class="card border-0 shadow-sm mt-4">
          <div class="card-header bg-transparent">
            <h5 class="mb-0">Informazioni App</h5>
          </div>
          <div class="card-body">
            <ul class="list-unstyled mb-0">
              <li class="mb-2">
                <small class="text-muted d-block">Versione</small>
                <span>1.0.0</span>
              </li>
              <li>
                <small class="text-muted d-block">Tecnologie</small>
                <span>Vue.js 3, ASP.NET Core 8, MySQL</span>
              </li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
