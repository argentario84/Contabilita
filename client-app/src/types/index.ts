export enum TransactionType {
  Income = 1,
  Expense = 2
}

export enum RecurrenceType {
  Daily = 1,
  Weekly = 2,
  Monthly = 3,
  Yearly = 4
}

export interface User {
  id: string
  email: string
  firstName: string
  lastName: string
  initialBudget: number
  monthlyIncome: number
}

export interface AuthResponse {
  token: string
  expiration: string
  user: User
}

export interface Category {
  id: number
  name: string
  description?: string
  color?: string
  icon?: string
  type: TransactionType
  monthlyBudget?: number
  requireDescription: boolean
  spentThisMonth: number
  remainingBudget?: number
  budgetPercentageUsed?: number
}

export interface CreateCategory {
  name: string
  description?: string
  color?: string
  icon?: string
  type: TransactionType
  monthlyBudget?: number
  requireDescription?: boolean
}

export interface CategoryBudgetSummary {
  categoryId: number
  categoryName: string
  categoryColor?: string
  monthlyBudget?: number
  spentThisMonth: number
  remainingBudget?: number
  budgetPercentageUsed?: number
  isOverBudget: boolean
}

export interface Transaction {
  id: number
  amount: number
  description: string
  date: string
  type: TransactionType
  notes?: string
  categoryId: number
  categoryName: string
  categoryColor?: string
  scheduledExpenseId?: number
}

export interface CreateTransaction {
  amount: number
  description: string
  date: string
  type: TransactionType
  notes?: string
  categoryId: number
  scheduledExpenseId?: number
}

export interface TransactionSummary {
  totalIncome: number
  totalExpenses: number
  balance: number
  currentBudget: number
  expensesByCategory: CategorySummary[]
}

export interface CategorySummary {
  categoryId: number
  categoryName: string
  categoryColor?: string
  total: number
  percentage: number
}

export interface ScheduledExpense {
  id: number
  name: string
  amount: number
  description?: string
  recurrence: RecurrenceType
  startDate: string
  endDate?: string
  nextDueDate: string
  isActive: boolean
  categoryId: number
  categoryName: string
  categoryColor?: string
  isDueToday: boolean
}

export interface CreateScheduledExpense {
  name: string
  amount: number
  description?: string
  recurrence: RecurrenceType
  startDate: string
  endDate?: string
  categoryId: number
}

export interface CalendarEvent {
  id: number
  title: string
  description?: string
  startDate: string
  endDate?: string
  allDay: boolean
  color?: string
  isShared: boolean
  createdByName?: string
}

export interface CreateCalendarEvent {
  title: string
  description?: string
  startDate: string
  endDate?: string
  allDay: boolean
  color?: string
  isShared?: boolean
}

// Childcare types
export enum DayOfWeekEnum {
  Monday = 1,
  Tuesday = 2,
  Wednesday = 3,
  Thursday = 4,
  Friday = 5,
  Saturday = 6,
  Sunday = 7
}

export enum TimeSlotEnum {
  Morning = 1,
  Afternoon = 2,
  Evening = 3
}

export interface Caregiver {
  id: number
  name: string
  relationship?: string
  color?: string
  phone?: string
  displayOrder: number
  isActive: boolean
}

export interface CreateCaregiver {
  name: string
  relationship?: string
  color?: string
  phone?: string
  displayOrder?: number
  isActive?: boolean
}

export interface ChildcareSlot {
  id: number
  dayOfWeek: DayOfWeekEnum
  timeSlot: TimeSlotEnum
  weekStartDate: string
  caregiverId: number
  caregiverName: string
  caregiverColor?: string
}

export interface CreateChildcareSlot {
  dayOfWeek: DayOfWeekEnum
  timeSlot: TimeSlotEnum
  weekStartDate: string
  caregiverId: number
}

export interface WeeklyChildcare {
  weekStartDate: string
  slots: ChildcareSlot[]
}
