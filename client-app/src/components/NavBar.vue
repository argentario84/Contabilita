<script setup lang="ts">
import { RouterLink, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useScheduledExpensesStore } from '@/stores/scheduledExpenses'
import { computed, onMounted } from 'vue'

const authStore = useAuthStore()
const scheduledStore = useScheduledExpensesStore()
const router = useRouter()

const dueCount = computed(() => scheduledStore.dueExpenses.length)

onMounted(() => {
  scheduledStore.fetchDueExpenses()
})

function logout() {
  authStore.logout()
  router.push('/login')
}
</script>

<template>
  <nav class="navbar navbar-expand-lg navbar-dark bg-primary sticky-top">
    <div class="container-fluid">
      <RouterLink class="navbar-brand" to="/">
        <i class="bi bi-wallet2 me-2"></i>
        Contabilita
      </RouterLink>

      <button
        class="navbar-toggler"
        type="button"
        data-bs-toggle="collapse"
        data-bs-target="#navbarNav"
      >
        <span class="navbar-toggler-icon"></span>
      </button>

      <div class="collapse navbar-collapse" id="navbarNav">
        <ul class="navbar-nav me-auto">
          <li class="nav-item">
            <RouterLink class="nav-link" to="/">
              <i class="bi bi-speedometer2 me-1"></i>
              Dashboard
            </RouterLink>
          </li>
          <li class="nav-item dropdown">
            <a
              class="nav-link dropdown-toggle"
              href="#"
              role="button"
              data-bs-toggle="dropdown"
            >
              <i class="bi bi-cash-stack me-1"></i>
              Transazioni
            </a>
            <ul class="dropdown-menu">
              <li>
                <RouterLink class="dropdown-item" to="/transactions">
                  <i class="bi bi-list-ul me-2"></i>
                  Lista
                </RouterLink>
              </li>
              <li>
                <RouterLink class="dropdown-item" to="/transactions-calendar">
                  <i class="bi bi-calendar-week me-2"></i>
                  Calendario
                </RouterLink>
              </li>
            </ul>
          </li>
          <li class="nav-item">
            <RouterLink class="nav-link" to="/categories">
              <i class="bi bi-tags me-1"></i>
              Categorie
            </RouterLink>
          </li>
          <li class="nav-item">
            <RouterLink class="nav-link" to="/scheduled">
              <i class="bi bi-calendar-check me-1"></i>
              Spese Programmate
              <span v-if="dueCount > 0" class="badge bg-danger ms-1">{{ dueCount }}</span>
            </RouterLink>
          </li>
          <li class="nav-item">
            <RouterLink class="nav-link" to="/calendar">
              <i class="bi bi-calendar3 me-1"></i>
              Calendario
            </RouterLink>
          </li>
        </ul>

        <ul class="navbar-nav">
          <li class="nav-item dropdown">
            <a
              class="nav-link dropdown-toggle"
              href="#"
              role="button"
              data-bs-toggle="dropdown"
            >
              <i class="bi bi-person-circle me-1"></i>
              {{ authStore.user?.firstName }}
            </a>
            <ul class="dropdown-menu dropdown-menu-end">
              <li>
                <RouterLink class="dropdown-item" to="/settings">
                  <i class="bi bi-gear me-2"></i>
                  Impostazioni
                </RouterLink>
              </li>
              <li><hr class="dropdown-divider" /></li>
              <li>
                <a class="dropdown-item" href="#" @click.prevent="logout">
                  <i class="bi bi-box-arrow-right me-2"></i>
                  Esci
                </a>
              </li>
            </ul>
          </li>
        </ul>
      </div>
    </div>
  </nav>
</template>
