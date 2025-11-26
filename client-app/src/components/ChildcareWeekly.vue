<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useChildcareStore } from '@/stores/childcare'
import { DayOfWeekEnum, TimeSlotEnum } from '@/types'
import type { Caregiver } from '@/types'

const store = useChildcareStore()
const loading = ref(true)
const showCaregiverModal = ref(false)
const editingCaregiver = ref<number | null>(null)

const caregiverForm = ref({
  name: '',
  relationship: '',
  color: '#0d6efd',
  phone: ''
})

const colors = [
  '#0d6efd', '#6610f2', '#6f42c1', '#d63384', '#dc3545',
  '#fd7e14', '#ffc107', '#198754', '#20c997', '#0dcaf0'
]

const days = [
  { value: DayOfWeekEnum.Monday, label: 'Lun' },
  { value: DayOfWeekEnum.Tuesday, label: 'Mar' },
  { value: DayOfWeekEnum.Wednesday, label: 'Mer' },
  { value: DayOfWeekEnum.Thursday, label: 'Gio' },
  { value: DayOfWeekEnum.Friday, label: 'Ven' },
  { value: DayOfWeekEnum.Saturday, label: 'Sab' },
  { value: DayOfWeekEnum.Sunday, label: 'Dom' }
]

const timeSlots = [
  { value: TimeSlotEnum.Morning, label: 'Mattina' },
  { value: TimeSlotEnum.Afternoon, label: 'Pomeriggio' },
  { value: TimeSlotEnum.Evening, label: 'Sera' }
]

const activeCaregivers = computed(() =>
  store.caregivers.filter((c) => c.isActive)
)

const currentWeekLabel = computed(() => {
  if (!store.currentWeekStart) return ''
  const start = new Date(store.currentWeekStart)
  const end = new Date(start)
  end.setDate(end.getDate() + 6)
  return `${formatDate(start)} - ${formatDate(end)}`
})

const draggedCaregiver = ref<Caregiver | null>(null)

onMounted(async () => {
  await Promise.all([store.fetchCaregivers(), store.fetchWeek()])
  loading.value = false
})

function formatDate(date: Date): string {
  return date.toLocaleDateString('it-IT', { day: 'numeric', month: 'short' })
}

function getWeekStart(date: Date): Date {
  const d = new Date(date)
  const day = d.getDay()
  const diff = d.getDate() - day + (day === 0 ? -6 : 1)
  return new Date(d.setDate(diff))
}

async function prevWeek() {
  const current = new Date(store.currentWeekStart)
  current.setDate(current.getDate() - 7)
  await store.fetchWeek(current.toISOString())
}

async function nextWeek() {
  const current = new Date(store.currentWeekStart)
  current.setDate(current.getDate() + 7)
  await store.fetchWeek(current.toISOString())
}

async function goToCurrentWeek() {
  const weekStart = getWeekStart(new Date())
  await store.fetchWeek(weekStart.toISOString())
}

function getSlotCaregiver(dayOfWeek: DayOfWeekEnum, timeSlot: TimeSlotEnum) {
  return store.getSlot(dayOfWeek, timeSlot)
}

function onDragStart(caregiver: Caregiver) {
  draggedCaregiver.value = caregiver
}

function onDragEnd() {
  draggedCaregiver.value = null
}

async function onDrop(dayOfWeek: DayOfWeekEnum, timeSlot: TimeSlotEnum) {
  if (!draggedCaregiver.value) return

  await store.createSlot({
    dayOfWeek,
    timeSlot,
    weekStartDate: store.currentWeekStart,
    caregiverId: draggedCaregiver.value.id
  })

  draggedCaregiver.value = null
}

function onDragOver(event: DragEvent) {
  event.preventDefault()
}

async function removeSlot(dayOfWeek: DayOfWeekEnum, timeSlot: TimeSlotEnum) {
  const slot = store.getSlot(dayOfWeek, timeSlot)
  if (slot) {
    await store.deleteSlot(slot.id)
  }
}

async function copyFromPreviousWeek() {
  const current = new Date(store.currentWeekStart)
  const previous = new Date(current)
  previous.setDate(previous.getDate() - 7)
  await store.copyWeek(previous.toISOString(), current.toISOString())
}

function openCaregiverModal(caregiver?: Caregiver) {
  if (caregiver) {
    editingCaregiver.value = caregiver.id
    caregiverForm.value = {
      name: caregiver.name,
      relationship: caregiver.relationship || '',
      color: caregiver.color || '#0d6efd',
      phone: caregiver.phone || ''
    }
  } else {
    editingCaregiver.value = null
    caregiverForm.value = {
      name: '',
      relationship: '',
      color: '#0d6efd',
      phone: ''
    }
  }
  showCaregiverModal.value = true
}

async function saveCaregiver() {
  if (editingCaregiver.value) {
    await store.updateCaregiver(editingCaregiver.value, caregiverForm.value)
  } else {
    await store.createCaregiver(caregiverForm.value)
  }
  showCaregiverModal.value = false
}

async function toggleCaregiverActive(caregiver: Caregiver) {
  await store.updateCaregiver(caregiver.id, { isActive: !caregiver.isActive })
}
</script>

<template>
  <div class="card border-0 shadow-sm">
    <div class="card-header bg-transparent d-flex justify-content-between align-items-center">
      <h5 class="mb-0">
        <i class="bi bi-people me-2"></i>
        Gestione Settimanale Bambina
      </h5>
      <button class="btn btn-sm btn-outline-primary" @click="openCaregiverModal()">
        <i class="bi bi-person-plus me-1"></i>
        Aggiungi Persona
      </button>
    </div>

    <div v-if="loading" class="card-body text-center py-5">
      <div class="spinner-border text-primary"></div>
    </div>

    <template v-else>
      <!-- Navigazione settimana -->
      <div class="card-body border-bottom py-2">
        <div class="d-flex justify-content-between align-items-center">
          <button class="btn btn-outline-primary btn-sm" @click="prevWeek">
            <i class="bi bi-chevron-left"></i>
          </button>
          <div class="d-flex align-items-center gap-2">
            <span class="fw-medium">{{ currentWeekLabel }}</span>
            <button class="btn btn-outline-secondary btn-sm" @click="goToCurrentWeek">
              Oggi
            </button>
            <button
              class="btn btn-outline-info btn-sm"
              @click="copyFromPreviousWeek"
              title="Copia dalla settimana precedente"
            >
              <i class="bi bi-clipboard"></i>
            </button>
          </div>
          <button class="btn btn-outline-primary btn-sm" @click="nextWeek">
            <i class="bi bi-chevron-right"></i>
          </button>
        </div>
      </div>

      <!-- Lista caregiver trascinabili -->
      <div class="card-body border-bottom py-2">
        <div class="d-flex flex-wrap gap-2">
          <div
            v-for="caregiver in activeCaregivers"
            :key="caregiver.id"
            class="badge fs-6 py-2 px-3"
            :style="{ backgroundColor: caregiver.color || '#6c757d', cursor: 'grab' }"
            draggable="true"
            @dragstart="onDragStart(caregiver)"
            @dragend="onDragEnd"
            @dblclick="openCaregiverModal(caregiver)"
          >
            {{ caregiver.name }}
            <small v-if="caregiver.relationship" class="ms-1 opacity-75">
              ({{ caregiver.relationship }})
            </small>
          </div>
          <div v-if="activeCaregivers.length === 0" class="text-muted small">
            Nessuna persona configurata. Aggiungi nonni, zii, etc.
          </div>
        </div>
      </div>

      <!-- Griglia settimanale -->
      <div class="card-body p-0">
        <div class="table-responsive">
          <table class="table table-bordered mb-0">
            <thead class="table-light">
              <tr>
                <th style="width: 80px"></th>
                <th v-for="day in days" :key="day.value" class="text-center" style="width: 13%">
                  {{ day.label }}
                </th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="slot in timeSlots" :key="slot.value">
                <td class="align-middle text-center fw-medium bg-light">
                  {{ slot.label }}
                </td>
                <td
                  v-for="day in days"
                  :key="`${day.value}-${slot.value}`"
                  class="p-1 align-middle text-center"
                  style="height: 60px"
                  @dragover="onDragOver"
                  @drop="onDrop(day.value, slot.value)"
                >
                  <div
                    v-if="getSlotCaregiver(day.value, slot.value)"
                    class="badge w-100 py-2 position-relative"
                    :style="{
                      backgroundColor: getSlotCaregiver(day.value, slot.value)?.caregiverColor || '#6c757d'
                    }"
                  >
                    {{ getSlotCaregiver(day.value, slot.value)?.caregiverName }}
                    <button
                      class="btn btn-link btn-sm text-white position-absolute top-0 end-0 p-0 pe-1"
                      @click.stop="removeSlot(day.value, slot.value)"
                      style="font-size: 0.7rem"
                    >
                      <i class="bi bi-x"></i>
                    </button>
                  </div>
                  <div
                    v-else
                    class="border border-dashed rounded h-100 d-flex align-items-center justify-content-center text-muted"
                    :class="{ 'bg-primary bg-opacity-10': draggedCaregiver }"
                    style="min-height: 40px"
                  >
                    <small v-if="draggedCaregiver">Trascina qui</small>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </template>

    <!-- Modal Caregiver -->
    <div
      v-if="showCaregiverModal"
      class="modal show d-block"
      tabindex="-1"
      style="background: rgba(0, 0, 0, 0.5)"
    >
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">
              {{ editingCaregiver ? 'Modifica Persona' : 'Nuova Persona' }}
            </h5>
            <button type="button" class="btn-close" @click="showCaregiverModal = false"></button>
          </div>
          <form @submit.prevent="saveCaregiver">
            <div class="modal-body">
              <div class="mb-3">
                <label class="form-label">Nome</label>
                <input v-model="caregiverForm.name" type="text" class="form-control" required />
              </div>

              <div class="mb-3">
                <label class="form-label">Parentela</label>
                <select v-model="caregiverForm.relationship" class="form-select">
                  <option value="">Seleziona...</option>
                  <option value="Nonno">Nonno</option>
                  <option value="Nonna">Nonna</option>
                  <option value="Zio">Zio</option>
                  <option value="Zia">Zia</option>
                  <option value="Babysitter">Babysitter</option>
                  <option value="Altro">Altro</option>
                </select>
              </div>

              <div class="mb-3">
                <label class="form-label">Telefono</label>
                <input v-model="caregiverForm.phone" type="tel" class="form-control" />
              </div>

              <div class="mb-3">
                <label class="form-label">Colore</label>
                <div class="d-flex flex-wrap gap-2">
                  <button
                    v-for="color in colors"
                    :key="color"
                    type="button"
                    class="btn p-0 rounded-circle"
                    :style="{
                      backgroundColor: color,
                      width: '30px',
                      height: '30px',
                      border: caregiverForm.color === color ? '3px solid #000' : 'none'
                    }"
                    @click="caregiverForm.color = color"
                  ></button>
                </div>
              </div>
            </div>
            <div class="modal-footer">
              <button
                v-if="editingCaregiver"
                type="button"
                class="btn btn-outline-secondary me-auto"
                @click="
                  toggleCaregiverActive(store.caregivers.find((c) => c.id === editingCaregiver)!)
                "
              >
                {{
                  store.caregivers.find((c) => c.id === editingCaregiver)?.isActive
                    ? 'Disattiva'
                    : 'Attiva'
                }}
              </button>
              <button type="button" class="btn btn-secondary" @click="showCaregiverModal = false">
                Annulla
              </button>
              <button type="submit" class="btn btn-primary">
                {{ editingCaregiver ? 'Salva' : 'Aggiungi' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.border-dashed {
  border-style: dashed !important;
}
</style>
