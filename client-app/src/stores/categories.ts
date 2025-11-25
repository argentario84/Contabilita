import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '@/services/api'
import type { Category, CreateCategory, TransactionType, CategoryBudgetSummary } from '@/types'

export const useCategoriesStore = defineStore('categories', () => {
  const categories = ref<Category[]>([])
  const budgetSummary = ref<CategoryBudgetSummary[]>([])
  const loading = ref(false)

  async function fetchCategories(type?: TransactionType) {
    loading.value = true
    try {
      const params = type ? { type } : {}
      const response = await api.get<Category[]>('/categories', { params })
      categories.value = response.data
      return response.data
    } finally {
      loading.value = false
    }
  }

  async function fetchBudgetSummary() {
    const response = await api.get<CategoryBudgetSummary[]>('/categories/budget-summary')
    budgetSummary.value = response.data
    return response.data
  }

  async function createCategory(data: CreateCategory) {
    const response = await api.post<Category>('/categories', data)
    categories.value.push(response.data)
    return response.data
  }

  async function updateCategory(id: number, data: Partial<CreateCategory>) {
    const response = await api.put<Category>(`/categories/${id}`, data)
    const index = categories.value.findIndex((c) => c.id === id)
    if (index !== -1) {
      categories.value[index] = response.data
    }
    return response.data
  }

  async function deleteCategory(id: number) {
    await api.delete(`/categories/${id}`)
    categories.value = categories.value.filter((c) => c.id !== id)
  }

  return {
    categories,
    budgetSummary,
    loading,
    fetchCategories,
    fetchBudgetSummary,
    createCategory,
    updateCategory,
    deleteCategory
  }
})
