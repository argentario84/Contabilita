<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const form = ref({
  email: '',
  password: '',
  confirmPassword: '',
  firstName: '',
  lastName: '',
  initialBudget: 0,
  monthlyIncome: 0
})

const error = ref('')
const loading = ref(false)

async function handleRegister() {
  error.value = ''

  if (form.value.password !== form.value.confirmPassword) {
    error.value = 'Le password non coincidono'
    return
  }

  loading.value = true

  try {
    await authStore.register({
      email: form.value.email,
      password: form.value.password,
      firstName: form.value.firstName,
      lastName: form.value.lastName,
      initialBudget: form.value.initialBudget,
      monthlyIncome: form.value.monthlyIncome
    })
    router.push('/')
  } catch (e: any) {
    error.value = e.response?.data?.errors?.join(', ') || 'Errore durante la registrazione'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="min-vh-100 d-flex align-items-center justify-content-center bg-light py-4">
    <div class="card shadow" style="width: 450px">
      <div class="card-body p-4">
        <div class="text-center mb-4">
          <i class="bi bi-wallet2 text-primary" style="font-size: 3rem"></i>
          <h2 class="mt-2">Registrati</h2>
          <p class="text-muted">Crea il tuo account</p>
        </div>

        <div v-if="error" class="alert alert-danger">
          {{ error }}
        </div>

        <form @submit.prevent="handleRegister">
          <div class="row">
            <div class="col-md-6 mb-3">
              <label class="form-label">Nome</label>
              <input
                v-model="form.firstName"
                type="text"
                class="form-control"
                required
              />
            </div>
            <div class="col-md-6 mb-3">
              <label class="form-label">Cognome</label>
              <input
                v-model="form.lastName"
                type="text"
                class="form-control"
                required
              />
            </div>
          </div>

          <div class="mb-3">
            <label class="form-label">Email</label>
            <input
              v-model="form.email"
              type="email"
              class="form-control"
              required
            />
          </div>

          <div class="row">
            <div class="col-md-6 mb-3">
              <label class="form-label">Password</label>
              <input
                v-model="form.password"
                type="password"
                class="form-control"
                required
                minlength="6"
              />
            </div>
            <div class="col-md-6 mb-3">
              <label class="form-label">Conferma Password</label>
              <input
                v-model="form.confirmPassword"
                type="password"
                class="form-control"
                required
              />
            </div>
          </div>

          <hr />

          <div class="row">
            <div class="col-md-6 mb-3">
              <label class="form-label">Budget Iniziale</label>
              <div class="input-group">
                <span class="input-group-text">€</span>
                <input
                  v-model.number="form.initialBudget"
                  type="number"
                  class="form-control"
                  step="0.01"
                />
              </div>
            </div>
            <div class="col-md-6 mb-3">
              <label class="form-label">Entrata Mensile</label>
              <div class="input-group">
                <span class="input-group-text">€</span>
                <input
                  v-model.number="form.monthlyIncome"
                  type="number"
                  class="form-control"
                  step="0.01"
                />
              </div>
            </div>
          </div>

          <button
            type="submit"
            class="btn btn-primary w-100"
            :disabled="loading"
          >
            <span v-if="loading" class="spinner-border spinner-border-sm me-2"></span>
            Registrati
          </button>
        </form>

        <hr />

        <p class="text-center mb-0">
          Hai già un account?
          <RouterLink to="/login">Accedi</RouterLink>
        </p>
      </div>
    </div>
  </div>
</template>
