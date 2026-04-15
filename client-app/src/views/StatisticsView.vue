<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import api from '@/services/api'

interface MonthlyTrend {
  year: number
  month: number
  monthLabel: string
  income: number
  expenses: number
  balance: number
}

interface CategorySummary {
  categoryId: number
  categoryName: string
  categoryColor: string | null
  total: number
  percentage: number
}

interface CategoryMonthlyTrend {
  categoryId: number
  categoryName: string
  categoryColor: string | null
  monthlyAmounts: number[]
}

interface DayOfWeekStats {
  dayName: string
  averageExpense: number
  totalExpense: number
  transactionCount: number
}

interface Statistics {
  monthlyTrend: MonthlyTrend[]
  topCategories: CategorySummary[]
  categoryMonthlyTrend: CategoryMonthlyTrend[]
  dayOfWeekStats: DayOfWeekStats[]
  averageMonthlyExpenses: number
  averageMonthlyIncome: number
  totalExpensesYTD: number
  totalIncomeYTD: number
  bestMonth: string | null
  worstMonth: string | null
  monthLabels: string[]
}

const stats = ref<Statistics | null>(null)
const loading = ref(false)
const error = ref<string | null>(null)
const selectedMonths = ref(12)

const formatCurrency = (val: number) =>
  new Intl.NumberFormat('it-IT', { style: 'currency', currency: 'EUR' }).format(val)

async function fetchStatistics() {
  loading.value = true
  error.value = null
  try {
    const { data } = await api.get<Statistics>(`/transactions/statistics?months=${selectedMonths.value}`)
    stats.value = data
  } catch {
    error.value = 'Errore nel caricamento delle statistiche.'
  } finally {
    loading.value = false
  }
}

onMounted(fetchStatistics)
watch(selectedMonths, fetchStatistics)

// ─── Colori palette ────────────────────────────────────────────────
const palette = [
  '#4361ee','#f72585','#4cc9f0','#3a0ca3','#7209b7',
  '#560bad','#480ca8','#3f37c9','#4895ef','#4cc9f0'
]

function catColor(cat: CategorySummary, idx: number) {
  return cat.categoryColor || palette[idx % palette.length]
}

// ─── Chart 1: Donut spese per categoria ───────────────────────────
const donutOptions = computed(() => ({
  chart: { type: 'donut', fontFamily: 'inherit', animations: { enabled: true, speed: 600 } },
  labels: stats.value?.topCategories.map(c => c.categoryName) ?? [],
  colors: stats.value?.topCategories.map((c, i) => catColor(c, i)) ?? [],
  legend: { position: 'bottom', fontSize: '13px' },
  plotOptions: {
    pie: {
      donut: {
        size: '65%',
        labels: {
          show: true,
          total: {
            show: true,
            label: 'Totale',
            formatter: (w: any) => {
              const total = w.globals.seriesTotals.reduce((a: number, b: number) => a + b, 0)
              return formatCurrency(total)
            }
          }
        }
      }
    }
  },
  dataLabels: { enabled: true, formatter: (_val: number, opts: any) => `${opts.w.globals.series[opts.seriesIndex].toFixed(0)}€` },
  tooltip: { y: { formatter: (val: number) => formatCurrency(val) } },
  responsive: [{ breakpoint: 480, options: { chart: { width: 300 } } }]
}))

const donutSeries = computed(() => stats.value?.topCategories.map(c => c.total) ?? [])

// ─── Chart 2: Area entrate vs uscite ─────────────────────────────
const areaOptions = computed(() => ({
  chart: {
    type: 'area', fontFamily: 'inherit', toolbar: { show: true },
    animations: { enabled: true, speed: 800 },
    zoom: { enabled: true }
  },
  colors: ['#2ecc71', '#e74c3c'],
  dataLabels: { enabled: false },
  stroke: { curve: 'smooth', width: 2 },
  fill: { type: 'gradient', gradient: { opacityFrom: 0.4, opacityTo: 0.05 } },
  xaxis: { categories: stats.value?.monthLabels ?? [], labels: { rotate: -45 } },
  yaxis: { labels: { formatter: (v: number) => `${v.toFixed(0)}€` } },
  tooltip: { y: { formatter: (val: number) => formatCurrency(val) } },
  legend: { position: 'top' },
  grid: { borderColor: '#f1f1f1' }
}))

const areaSeries = computed(() => [
  { name: 'Entrate', data: stats.value?.monthlyTrend.map(m => m.income) ?? [] },
  { name: 'Uscite', data: stats.value?.monthlyTrend.map(m => m.expenses) ?? [] }
])

// ─── Chart 3: Saldo mensile (bar positivo/negativo) ───────────────
const balanceBarOptions = computed(() => ({
  chart: { type: 'bar', fontFamily: 'inherit', toolbar: { show: false } },
  colors: stats.value?.monthlyTrend.map(m => m.balance >= 0 ? '#2ecc71' : '#e74c3c') ?? [],
  plotOptions: {
    bar: {
      borderRadius: 5,
      distributed: true,
      columnWidth: '60%',
      colors: {
        ranges: [
          { from: -999999, to: -0.01, color: '#e74c3c' },
          { from: 0, to: 999999, color: '#2ecc71' }
        ]
      }
    }
  },
  dataLabels: { enabled: false },
  xaxis: { categories: stats.value?.monthLabels ?? [], labels: { rotate: -45 } },
  yaxis: { labels: { formatter: (v: number) => `${v.toFixed(0)}€` } },
  tooltip: { y: { formatter: (val: number) => formatCurrency(val) } },
  legend: { show: false },
  grid: { borderColor: '#f1f1f1' }
}))

const balanceBarSeries = computed(() => [
  { name: 'Saldo', data: stats.value?.monthlyTrend.map(m => m.balance) ?? [] }
])

// ─── Chart 4: Stacked bar categorie per mese ──────────────────────
const stackedOptions = computed(() => ({
  chart: {
    type: 'bar', fontFamily: 'inherit', stacked: true,
    toolbar: { show: true },
    animations: { enabled: true, speed: 600 }
  },
  colors: stats.value?.categoryMonthlyTrend.map((c, i) => catColor({ categoryColor: c.categoryColor, categoryName: c.categoryName, categoryId: c.categoryId, total: 0, percentage: 0 }, i)) ?? [],
  plotOptions: { bar: { borderRadius: 3, columnWidth: '70%' } },
  dataLabels: { enabled: false },
  xaxis: { categories: stats.value?.monthLabels ?? [], labels: { rotate: -45 } },
  yaxis: { labels: { formatter: (v: number) => `${v.toFixed(0)}€` } },
  tooltip: {
    y: { formatter: (val: number) => formatCurrency(val) },
    shared: true, intersect: false
  },
  legend: { position: 'top' },
  grid: { borderColor: '#f1f1f1' },
  fill: { opacity: 1 }
}))

const stackedSeries = computed(() =>
  stats.value?.categoryMonthlyTrend.map(cat => ({
    name: cat.categoryName,
    data: cat.monthlyAmounts
  })) ?? []
)

// ─── Chart 5: Radar giorno della settimana ────────────────────────
const radarOptions = computed(() => ({
  chart: { type: 'radar', fontFamily: 'inherit', toolbar: { show: false } },
  colors: ['#4361ee'],
  fill: { opacity: 0.2 },
  stroke: { width: 2 },
  markers: { size: 4 },
  xaxis: { categories: stats.value?.dayOfWeekStats.map(d => d.dayName) ?? [] },
  yaxis: { show: false },
  tooltip: { y: { formatter: (val: number) => formatCurrency(val) } }
}))

const radarSeries = computed(() => [
  { name: 'Media spesa', data: stats.value?.dayOfWeekStats.map(d => d.averageExpense) ?? [] }
])

// ─── Chart 6: Heatmap-like bar per giorno settimana (totale) ──────
const dowBarOptions = computed(() => ({
  chart: { type: 'bar', fontFamily: 'inherit', toolbar: { show: false } },
  colors: ['#7209b7'],
  plotOptions: {
    bar: {
      borderRadius: 6, horizontal: true, distributed: false,
      dataLabels: { position: 'top' }
    }
  },
  dataLabels: {
    enabled: true,
    formatter: (val: number) => formatCurrency(val),
    offsetX: 8,
    style: { fontSize: '11px', colors: ['#333'] }
  },
  xaxis: {
    categories: stats.value?.dayOfWeekStats.map(d => d.dayName) ?? [],
    labels: { formatter: (v: number) => `${v.toFixed(0)}€` }
  },
  tooltip: { y: { formatter: (val: number) => formatCurrency(val) } },
  grid: { borderColor: '#f1f1f1' }
}))

const dowBarSeries = computed(() => [
  { name: 'Totale spese', data: stats.value?.dayOfWeekStats.map(d => d.totalExpense) ?? [] }
])

// KPI helpers
const balanceYTD = computed(() => (stats.value?.totalIncomeYTD ?? 0) - (stats.value?.totalExpensesYTD ?? 0))
const savingsRate = computed(() => {
  const inc = stats.value?.totalIncomeYTD ?? 0
  const exp = stats.value?.totalExpensesYTD ?? 0
  return inc > 0 ? Math.round(((inc - exp) / inc) * 100) : 0
})
</script>

<template>
  <div class="container-fluid py-4 px-3 px-md-4">
    <!-- Header -->
    <div class="d-flex flex-wrap align-items-center justify-content-between gap-3 mb-4">
      <div>
        <h2 class="fw-bold mb-1">
          <i class="bi bi-bar-chart-line me-2 text-primary"></i>Statistiche Avanzate
        </h2>
        <p class="text-muted mb-0">Analisi dettagliata delle tue finanze</p>
      </div>
      <div class="d-flex align-items-center gap-2">
        <label class="text-muted small fw-semibold">Periodo:</label>
        <select v-model="selectedMonths" class="form-select form-select-sm" style="width:auto">
          <option :value="3">Ultimi 3 mesi</option>
          <option :value="6">Ultimi 6 mesi</option>
          <option :value="12">Ultimi 12 mesi</option>
          <option :value="24">Ultimi 24 mesi</option>
        </select>
      </div>
    </div>

    <!-- Loading / Error -->
    <div v-if="loading" class="text-center py-5">
      <div class="spinner-border text-primary" role="status"></div>
      <p class="mt-2 text-muted">Caricamento statistiche...</p>
    </div>
    <div v-else-if="error" class="alert alert-danger">{{ error }}</div>

    <template v-else-if="stats">
      <!-- ── KPI Cards ── -->
      <div class="row g-3 mb-4">
        <div class="col-6 col-lg-3">
          <div class="card border-0 shadow-sm h-100">
            <div class="card-body">
              <div class="d-flex align-items-center gap-3">
                <div class="rounded-3 p-3 bg-success bg-opacity-10">
                  <i class="bi bi-arrow-up-circle-fill text-success fs-4"></i>
                </div>
                <div>
                  <p class="text-muted small mb-0">Entrate (YTD)</p>
                  <p class="fw-bold fs-5 mb-0 text-success">{{ formatCurrency(stats.totalIncomeYTD) }}</p>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div class="col-6 col-lg-3">
          <div class="card border-0 shadow-sm h-100">
            <div class="card-body">
              <div class="d-flex align-items-center gap-3">
                <div class="rounded-3 p-3 bg-danger bg-opacity-10">
                  <i class="bi bi-arrow-down-circle-fill text-danger fs-4"></i>
                </div>
                <div>
                  <p class="text-muted small mb-0">Uscite (YTD)</p>
                  <p class="fw-bold fs-5 mb-0 text-danger">{{ formatCurrency(stats.totalExpensesYTD) }}</p>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div class="col-6 col-lg-3">
          <div class="card border-0 shadow-sm h-100">
            <div class="card-body">
              <div class="d-flex align-items-center gap-3">
                <div class="rounded-3 p-3" :class="balanceYTD >= 0 ? 'bg-primary bg-opacity-10' : 'bg-warning bg-opacity-10'">
                  <i class="bi bi-wallet2 fs-4" :class="balanceYTD >= 0 ? 'text-primary' : 'text-warning'"></i>
                </div>
                <div>
                  <p class="text-muted small mb-0">Saldo (YTD)</p>
                  <p class="fw-bold fs-5 mb-0" :class="balanceYTD >= 0 ? 'text-primary' : 'text-warning'">
                    {{ formatCurrency(balanceYTD) }}
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div class="col-6 col-lg-3">
          <div class="card border-0 shadow-sm h-100">
            <div class="card-body">
              <div class="d-flex align-items-center gap-3">
                <div class="rounded-3 p-3 bg-info bg-opacity-10">
                  <i class="bi bi-piggy-bank-fill text-info fs-4"></i>
                </div>
                <div>
                  <p class="text-muted small mb-0">Tasso Risparmio</p>
                  <p class="fw-bold fs-5 mb-0 text-info">{{ savingsRate }}%</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- ── Insight pills ── -->
      <div class="d-flex flex-wrap gap-2 mb-4">
        <span class="badge rounded-pill bg-success bg-opacity-15 text-success border border-success border-opacity-25 px-3 py-2">
          <i class="bi bi-trophy me-1"></i>Mese migliore: <strong>{{ stats.bestMonth ?? '–' }}</strong>
        </span>
        <span class="badge rounded-pill bg-danger bg-opacity-15 text-danger border border-danger border-opacity-25 px-3 py-2">
          <i class="bi bi-exclamation-circle me-1"></i>Mese peggiore: <strong>{{ stats.worstMonth ?? '–' }}</strong>
        </span>
        <span class="badge rounded-pill bg-primary bg-opacity-15 text-primary border border-primary border-opacity-25 px-3 py-2">
          <i class="bi bi-calculator me-1"></i>Media mensile uscite: <strong>{{ formatCurrency(stats.averageMonthlyExpenses) }}</strong>
        </span>
        <span class="badge rounded-pill bg-secondary bg-opacity-15 text-secondary border border-secondary border-opacity-25 px-3 py-2">
          <i class="bi bi-graph-up me-1"></i>Media mensile entrate: <strong>{{ formatCurrency(stats.averageMonthlyIncome) }}</strong>
        </span>
      </div>

      <!-- ── Row 1: Donut + Area trend ── -->
      <div class="row g-3 mb-3">
        <div class="col-12 col-xl-5">
          <div class="card border-0 shadow-sm h-100">
            <div class="card-body">
              <h6 class="fw-semibold text-muted mb-3">
                <i class="bi bi-pie-chart-fill me-2 text-primary"></i>Spese per Categoria
              </h6>
              <apexchart
                v-if="donutSeries.length"
                type="donut"
                :options="donutOptions"
                :series="donutSeries"
                height="320"
              />
              <div v-else class="text-center text-muted py-5">Nessun dato disponibile</div>
            </div>
          </div>
        </div>

        <div class="col-12 col-xl-7">
          <div class="card border-0 shadow-sm h-100">
            <div class="card-body">
              <h6 class="fw-semibold text-muted mb-3">
                <i class="bi bi-graph-up-arrow me-2 text-success"></i>Entrate vs Uscite nel Tempo
              </h6>
              <apexchart
                v-if="areaSeries[0]?.data.length"
                type="area"
                :options="areaOptions"
                :series="areaSeries"
                height="300"
              />
              <div v-else class="text-center text-muted py-5">Nessun dato disponibile</div>
            </div>
          </div>
        </div>
      </div>

      <!-- ── Row 2: Saldo mensile ── -->
      <div class="row g-3 mb-3">
        <div class="col-12">
          <div class="card border-0 shadow-sm">
            <div class="card-body">
              <h6 class="fw-semibold text-muted mb-3">
                <i class="bi bi-bar-chart-fill me-2 text-info"></i>Saldo Mensile
              </h6>
              <apexchart
                v-if="balanceBarSeries[0]?.data.length"
                type="bar"
                :options="balanceBarOptions"
                :series="balanceBarSeries"
                height="260"
              />
              <div v-else class="text-center text-muted py-5">Nessun dato disponibile</div>
            </div>
          </div>
        </div>
      </div>

      <!-- ── Row 3: Stacked categorie per mese ── -->
      <div class="row g-3 mb-3">
        <div class="col-12 col-xl-8">
          <div class="card border-0 shadow-sm h-100">
            <div class="card-body">
              <h6 class="fw-semibold text-muted mb-3">
                <i class="bi bi-layers-fill me-2 text-warning"></i>Composizione Spese per Categoria (Top 5)
              </h6>
              <apexchart
                v-if="stackedSeries.length"
                type="bar"
                :options="stackedOptions"
                :series="stackedSeries"
                height="300"
              />
              <div v-else class="text-center text-muted py-5">Nessun dato disponibile</div>
            </div>
          </div>
        </div>

        <div class="col-12 col-xl-4">
          <div class="card border-0 shadow-sm h-100">
            <div class="card-body">
              <h6 class="fw-semibold text-muted mb-3">
                <i class="bi bi-hexagon-fill me-2 text-primary"></i>Spesa Media per Giorno
              </h6>
              <apexchart
                v-if="radarSeries[0]?.data.length"
                type="radar"
                :options="radarOptions"
                :series="radarSeries"
                height="280"
              />
              <div v-else class="text-center text-muted py-5">Nessun dato disponibile</div>
            </div>
          </div>
        </div>
      </div>

      <!-- ── Row 4: DoW totale + Top categorie tabella ── -->
      <div class="row g-3 mb-3">
        <div class="col-12 col-xl-5">
          <div class="card border-0 shadow-sm h-100">
            <div class="card-body">
              <h6 class="fw-semibold text-muted mb-3">
                <i class="bi bi-calendar-week me-2 text-purple"></i>Totale Spese per Giorno della Settimana
              </h6>
              <apexchart
                v-if="dowBarSeries[0]?.data.length"
                type="bar"
                :options="dowBarOptions"
                :series="dowBarSeries"
                height="260"
              />
              <div v-else class="text-center text-muted py-5">Nessun dato disponibile</div>
            </div>
          </div>
        </div>

        <div class="col-12 col-xl-7">
          <div class="card border-0 shadow-sm h-100">
            <div class="card-body">
              <h6 class="fw-semibold text-muted mb-3">
                <i class="bi bi-list-ol me-2 text-danger"></i>Top Categorie per Importo
              </h6>
              <div class="table-responsive">
                <table class="table table-hover align-middle mb-0">
                  <thead class="table-light">
                    <tr>
                      <th>#</th>
                      <th>Categoria</th>
                      <th class="text-end">Totale</th>
                      <th class="text-end">%</th>
                      <th>Peso</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="(cat, idx) in stats.topCategories" :key="cat.categoryId">
                      <td class="text-muted small">{{ idx + 1 }}</td>
                      <td>
                        <span
                          class="badge rounded-pill me-2"
                          :style="{ backgroundColor: catColor(cat, idx) }"
                        >&nbsp;</span>
                        {{ cat.categoryName }}
                      </td>
                      <td class="text-end fw-semibold">{{ formatCurrency(cat.total) }}</td>
                      <td class="text-end text-muted small">{{ cat.percentage.toFixed(1) }}%</td>
                      <td style="min-width:100px">
                        <div class="progress" style="height:6px">
                          <div
                            class="progress-bar"
                            :style="{ width: cat.percentage + '%', backgroundColor: catColor(cat, idx) }"
                          ></div>
                        </div>
                      </td>
                    </tr>
                    <tr v-if="!stats.topCategories.length">
                      <td colspan="5" class="text-center text-muted py-3">Nessun dato</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- ── Row 5: Dettaglio giorni settimana tabella ── -->
      <div class="row g-3">
        <div class="col-12">
          <div class="card border-0 shadow-sm">
            <div class="card-body">
              <h6 class="fw-semibold text-muted mb-3">
                <i class="bi bi-table me-2 text-secondary"></i>Dettaglio Spese per Giorno della Settimana
              </h6>
              <div class="row g-2">
                <div
                  v-for="day in stats.dayOfWeekStats"
                  :key="day.dayName"
                  class="col-6 col-sm-4 col-lg-auto flex-lg-fill"
                >
                  <div class="card border-0 bg-light text-center py-3 px-2 h-100">
                    <p class="fw-bold text-primary mb-1">{{ day.dayName }}</p>
                    <p class="small text-muted mb-1">Totale</p>
                    <p class="fw-semibold mb-1">{{ formatCurrency(day.totalExpense) }}</p>
                    <p class="small text-muted mb-1">Media</p>
                    <p class="text-danger small mb-1">{{ formatCurrency(day.averageExpense) }}</p>
                    <p class="small text-muted mb-0">
                      <i class="bi bi-receipt me-1"></i>{{ day.transactionCount }} op.
                    </p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<style scoped>
.card {
  border-radius: 12px;
}
.badge.rounded-pill {
  font-size: 0.8rem;
}
</style>
