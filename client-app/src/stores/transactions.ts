import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '@/services/api'
import type { Transaction, CreateTransaction, TransactionSummary, TransactionType, BudgetPlanning } from '@/types'

export const useTransactionsStore = defineStore('transactions', () => {
  const transactions = ref<Transaction[]>([])
  const summary = ref<TransactionSummary | null>(null)
  const budgetPlanning = ref<BudgetPlanning | null>(null)
  const loading = ref(false)

  async function fetchTransactions(params?: {
    startDate?: string
    endDate?: string
    categoryId?: number
    type?: TransactionType
  }) {
    loading.value = true
    try {
      const response = await api.get<Transaction[]>('/transactions', { params })
      transactions.value = response.data
      return response.data
    } finally {
      loading.value = false
    }
  }

  async function fetchSummary(startDate?: string, endDate?: string) {
    const response = await api.get<TransactionSummary>('/transactions/summary', {
      params: { startDate, endDate }
    })
    summary.value = response.data
    return response.data
  }

  async function fetchBudgetPlanning() {
    const response = await api.get<BudgetPlanning>('/transactions/budget-planning')
    budgetPlanning.value = response.data
    return response.data
  }

  async function createTransaction(data: CreateTransaction) {
    const response = await api.post<Transaction>('/transactions', data)
    transactions.value.unshift(response.data)
    return response.data
  }

  async function updateTransaction(id: number, data: Partial<CreateTransaction>) {
    const response = await api.put<Transaction>(`/transactions/${id}`, data)
    const index = transactions.value.findIndex((t) => t.id === id)
    if (index !== -1) {
      transactions.value[index] = response.data
    }
    return response.data
  }

  async function deleteTransaction(id: number) {
    await api.delete(`/transactions/${id}`)
    transactions.value = transactions.value.filter((t) => t.id !== id)
  }

  return {
    transactions,
    summary,
    budgetPlanning,
    loading,
    fetchTransactions,
    fetchSummary,
    fetchBudgetPlanning,
    createTransaction,
    updateTransaction,
    deleteTransaction
  }
})
