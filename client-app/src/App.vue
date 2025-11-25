<script setup lang="ts">
import { RouterView } from 'vue-router'
import NavBar from './components/NavBar.vue'
import { useAuthStore } from './stores/auth'

const authStore = useAuthStore()
authStore.init()
</script>

<template>
  <NavBar v-if="authStore.isAuthenticated" />
  <main :class="{ 'container-fluid': authStore.isAuthenticated, 'min-vh-100': !authStore.isAuthenticated }">
    <RouterView />
  </main>
</template>

<style>
body {
  background-color: #f8f9fa;
}
</style>
