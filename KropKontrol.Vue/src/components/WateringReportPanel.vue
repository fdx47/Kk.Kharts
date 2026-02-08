<template>
  <div class="report-container">
    <div class="text-center my-4" v-if="loading">
      <div class="spinner-border" role="status">
        <span class="visually-hidden">Chargement...</span>
      </div>
    </div>
    <div class="alert alert-danger mt-3" v-if="error">{{ error }}</div>

    <div v-if="reportReady" class="report-section">
      <div class="summary-wrapper">
        <div class="summary-toggle" v-if="isRoot">
          <button class="btn btn-sm toggle-btn chart-toggle-btn" @click="onToggleChart">
            {{ showChart ? 'Masquer le graphique' : 'Afficher le graphique' }}
          </button>
        </div>
        <div v-html="summaryHtml"></div>
      </div>

      <!-- Zone graphique (ApexCharts) -->
      <div v-show="showChart" class="chart-area" ref="chartRef"></div>

      <div class="mb-3 d-flex flex-wrap gap-2 justify-content-end">
        <button class="btn btn-sm toggle-btn" @click="showDetail = !showDetail">
          {{ showDetail ? 'Masquer le détail des arrosages' : 'Afficher le détail des arrosages' }}
        </button>
      </div>

      <div v-show="showDetail" v-html="detailedHtml"></div>
    </div>

    <div v-else class="d-flex align-items-center justify-content-center placeholder-message" style="color: #82be20ff">
      Sélectionnez un Kapteur dans la sidebar pour générer le rapport.
    </div>
  </div>
</template>

<script setup>
import { ref, watch, onMounted, nextTick } from 'vue'
import { refreshTokenIfNeeded } from '../services/authService.js'
import { isRootUser } from '../services/roleUtils.js'
import { getUc502Wet150DataCached } from '../services/dataCacheService.js'
import { analyzeWateringSeries } from '../services/dataProcessor.js'
import { buildSummaryTableHtml } from '../services/summaryTableHtmlBuilder.js'
import { buildDetailedTableHtmlForRange } from '../services/tableHtmlBuilderRange.js'
import { buildChartOptions } from '../services/chartOptionsBuilder.js'
import { getUserIdFromToken } from '../services/authService.js'
import useLocalStorage from '../composables/useLocalStorage.js'
import { LS_DASHBOARD_STATE_PREFIX } from '../config/storageKeys.js'
import { parseVarKey } from '../composables/useChartCalculations.js'

// Handlers globaux pour le surlignage du tableau rapitulatif
if (typeof window !== 'undefined') {
  if (!window.kkSumHover) {
    window.kkSumHover = function (el) {
      try {
        const t = el?.closest?.('table')
        if (!t) return
        // Clear any previous highlights to avoid sticky states
        try {
          t.querySelectorAll('td.hl').forEach((n) => n.classList.remove('hl'))
        } catch {}
        let c = Number(el?.dataset?.col)
        if (!Number.isFinite(c)) {
          const td = el.closest ? el.closest('td,th') : null
          if (td && td.parentElement) {
            const idx = Array.prototype.indexOf.call(td.parentElement.children, td)
            c = Math.max(0, idx - 1)
          } else {
            c = 0
          }
        }
        const q = (row, k) => t.querySelector(`tr.row-${row} td[data-col="${k}"]`)
        const qAlt = (row, k) => t.querySelector(`tr.row-${row} td:nth-child(${k + 2})`)
        const pick = (row, k) => q(row, k) || qAlt(row, k)
        const add = (n) => n && n.classList && n.classList.add('hl')
        add(pick('night', c))
        add(pick('pre', c))
        if (c > 0) add(pick('post', c - 1))
      } catch {}
    }
  }
  if (!window.kkSumLeave) {
    window.kkSumLeave = function (el) {
      try {
        const t = el?.closest?.('table')
        if (!t) return
        // Proactively clear in case mouseleave isn't triggered symmetrically
        try {
          t.querySelectorAll('td.hl').forEach((n) => n.classList.remove('hl'))
        } catch {}
        let c = Number(el?.dataset?.col)
        if (!Number.isFinite(c)) {
          const td = el.closest ? el.closest('td,th') : null
          if (td && td.parentElement) {
            const idx = Array.prototype.indexOf.call(td.parentElement.children, td)
            c = Math.max(0, idx - 1)
          } else {
            c = 0
          }
        }
        const q = (row, k) => t.querySelector(`tr.row-${row} td[data-col="${k}"]`)
        const qAlt = (row, k) => t.querySelector(`tr.row-${row} td:nth-child(${k + 2})`)
        const pick = (row, k) => q(row, k) || qAlt(row, k)
        const rem = (n) => n && n.classList && n.classList.remove('hl')
        rem(pick('night', c))
        rem(pick('pre', c))
        if (c > 0) rem(pick('post', c - 1))
      } catch {}
    }
  }
  if (!window.kkSumHoverCell) {
    window.kkSumHoverCell = function (ev) {
      const td = ev?.target?.closest?.('td')
      if (td) window.kkSumHover(td)
    }
  }
  if (!window.kkSumLeaveCell) {
    window.kkSumLeaveCell = function (ev) {
      const td = ev?.target?.closest?.('td')
      if (td) window.kkSumLeave(td)
    }
  }
}

const props = defineProps({
  devEui: { type: String, required: false },
  startDate: { type: String, required: false }, // 'YYYY-MM-DD'
  endDate: { type: String, required: false } // 'YYYY-MM-DD'
})

const loading = ref(false)
const error = ref('')
const reportReady = ref(false)
const summaryHtml = ref('')
const detailedHtml = ref('')
const showDetail = ref(false)
const showChart = ref(false)
const chartRef = ref(null)
let chartInstance = null

// Données conservées pour le rendu du graphique
const lastArr = ref([])
const lastAnalysis = ref({ points: [], events: [], meta: {}, config: {} })
const isRoot = isRootUser()
const selectedField = ref('permittivite')

// Lecture de la variable VWC sélectionnée dans les graphiques (Dashboard)
const STORAGE_KEY = LS_DASHBOARD_STATE_PREFIX + (getUserIdFromToken() ?? 'guest')
const chartsState = useLocalStorage(STORAGE_KEY, [])
const VWC_FIELDS = ['mineralVWC', 'organicVWC', 'peatMixVWC', 'coirVWC', 'minWoolVWC', 'perliteVWC']

function getSelectedVwcFieldForDevice(devEui) {
  const charts = Array.isArray(chartsState.value) ? chartsState.value : []
  const devCharts = charts.filter((c) => c?.devEui === devEui)
  if (!devCharts.length) return null
  for (const cfg of devCharts) {
    const vars = Array.isArray(cfg.variables) ? cfg.variables : []
    // Priorité aux clés connues de VWC
    for (const field of VWC_FIELDS) {
      const has = vars.some((v) => {
        const { field: f, devEui: vd } = parseVarKey(String(v))
        return f === field && (!vd || vd === devEui)
      })
      if (has) return field
    }
    // fallback: toute variable finissant par 'VWC' et liée au même device
    const any = vars
      .map((v) => parseVarKey(String(v)))
      .find((p) => p.field?.endsWith('VWC') && (!p.devEui || p.devEui === devEui))
    if (any) return any.field
  }
  return null
}

function toLocalDayISO(dateStr, endOfDay = false) {
  if (!dateStr) return null
  const [y, m, d] = String(dateStr).split('-').map(Number)
  if (!y || !m || !d) return null
  const dt = endOfDay
    ? new Date(y, m - 1, d, 23, 59, 59, 999)
    : new Date(y, m - 1, d, 0, 0, 0, 0)
  return dt.toISOString()
}

async function generateReport() {
  if (!props.devEui) return
  loading.value = true
  error.value = ''
  reportReady.value = false
  showDetail.value = false
  if (showChart.value) showChart.value = false

  try {
    await refreshTokenIfNeeded()
    // Détermine la variable VWC sélectionnée côté graphique pour ce device
    const vwcField = getSelectedVwcFieldForDevice(props.devEui)
    if (!vwcField) {
      summaryHtml.value = `
        <div style="text-align:center; margin-top:16px; padding:12px; border-radius:10px; background:#fff3cd; color:#856404; border:1px solid #ffeeba; font-family: Inter, system-ui, Arial, sans-serif; font-size:0.95rem;">
          Pour afficher les consommations (post, nuit, pré), sélectionnez d'abord une variable VWC dans le graphique.
        </div>`
      detailedHtml.value = ''
      reportReady.value = true
      return
    }
    selectedField.value = vwcField
    let startISO
    let endISO
    if (props.startDate) {
      startISO = toLocalDayISO(props.startDate)
    } else {
      const now = new Date()
      const startLocal = new Date(now.getFullYear(), now.getMonth(), now.getDate() - 6, 0, 0, 0, 0)
      startISO = startLocal.toISOString()
    }
    if (props.endDate) {
      endISO = toLocalDayISO(props.endDate, true)
    } else {
      const now = new Date()
      const endLocal = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 23, 59, 59, 999)
      endISO = endLocal.toISOString()
    }
    const arr = await getUc502Wet150DataCached(props.devEui, startISO, endISO)

    const detectionField = arr.some((row) => row?.permittivite != null)
      ? 'permittivite'
      : selectedField.value
    const analysis = analyzeWateringSeries(arr, detectionField)
    const finalEvents = Array.isArray(analysis?.events) ? analysis.events : []

    lastArr.value = arr
    lastAnalysis.value = analysis

    const daysInPeriod = (() => {
      const s = new Date(startISO)
      const e = new Date(endISO)
      const s0 = new Date(s.getFullYear(), s.getMonth(), s.getDate())
      const e0 = new Date(e.getFullYear(), e.getMonth(), e.getDate())
      return Math.max(1, Math.round((e0 - s0) / 86400000) + 1)
    })()

    if (!vwcField) {
      // Aucun VWC sélectionné: affiche un message explicite
      summaryHtml.value = `
        <div style="text-align:center; margin-top:16px; padding:12px; border-radius:10px;
          background:#fff3cd; color:#856404; border:1px solid #ffeeba;
          font-family: Inter, system-ui, Arial, sans-serif; font-size:0.95rem;">
          Pour afficher les consommations (post, nuit, pré), sélectionnez d'abord une variable VWC dans le graphique.
        </div>`
      detailedHtml.value = ''
      reportReady.value = true
      return
    }

    const {
      dayHeaders,
      anomaliesByDaySlot,
      consoPostArrosage,
      consoDeNuit,
      consoPreArrosage,
      ecAtSunrise,
      ecDayAvg,
      ecDayDelta,
      detailedTableHtml,
      eventsByDay,
      drainageByDay,
    } = buildDetailedTableHtmlForRange(finalEvents, arr, vwcField, startISO, endISO)

    summaryHtml.value = buildSummaryTableHtml(
      dayHeaders,
      anomaliesByDaySlot,
      consoPostArrosage,
      consoDeNuit,
      consoPreArrosage,
      ecAtSunrise,
      ecDayAvg,
      ecDayDelta,
      daysInPeriod,
      eventsByDay,
      drainageByDay,
      isRoot
    )
    detailedHtml.value = detailedTableHtml
    reportReady.value = true
  } catch (e) {
    console.error(e)
    error.value = e?.message || 'Erreur lors de la génération du rapport'
  } finally {
    loading.value = false
  }
}

async function renderChart() {
  try {
    const { default: ApexCharts } = await import('apexcharts')
    if (chartInstance) {
      chartInstance.destroy()
      chartInstance = null
    }
    const options = buildChartOptions(lastArr.value, lastAnalysis.value, selectedField.value)
    await nextTick()
    if (!chartRef.value) return
    chartInstance = new ApexCharts(chartRef.value, options)
    await chartInstance.render()
  } catch (err) {
    console.error('Erreur rendu graphique:', err)
  }
}

function destroyChart() {
  if (chartInstance) {
    chartInstance.destroy()
    chartInstance = null
  }
}

async function onToggleChart() {
  if (!isRoot) return
  showChart.value = !showChart.value
}

watch(showChart, async val => {
  if (val) {
    await nextTick()
    await renderChart()
  } else {
    destroyChart()
  }
})

watch(() => props.devEui, () => generateReport())
watch(() => props.startDate, () => generateReport())
watch(() => props.endDate, () => generateReport())
onMounted(() => generateReport())
</script>

<style scoped>
.report-container {
  max-width: 1200px;
  margin: 0 auto;
  padding-bottom: 2rem;
}

@media (max-width: 991px) {
  .report-container {
    max-width: 98vw;
    padding-left: 0.2rem;
    padding-right: 0.2rem;
  }
}
@media (max-width: 576px) {
  .report-container {
    padding-left: 0 !important;
    padding-right: 0 !important;
    padding-bottom: 1rem;
  }
  .report-section .card,
  .report-section .card-header,
  .report-section .card-body {
    font-size: 0.96rem !important;
  }
}

.toggle-btn {
  color: #82be20ff;
  border-color: #82be20ff;
  background: #f4f9f7;
  transition: background 0.2s;
}
.toggle-btn:hover,
.toggle-btn:focus {
  background: #eafaf3;
  color: #82be20ff;
  border-color: #82be20ff;
}

.summary-wrapper {
  position: relative;
}
.summary-toggle {
  position: absolute;
  top: 10px;
  right: 12px;
  z-index: 3;
  pointer-events: none; /* laisse les clics traverser sauf sur le bouton */
}
.chart-toggle-btn {
  pointer-events: auto;
}

.report-section .card {
  border: none;
}
.report-section .card-header {
  background-color: #82be20ff;
  color: #fff;
  font-weight: 600;
  font-size: 1.1rem;
  border-bottom: 1px solid #82be20ff;
}
.report-section .card-body {
  background-color: #fff;
  overflow-x: auto;
  word-break: break-word;
  font-size: 1rem;
}
.report-section .overflow-auto {
  max-height: 400px;
  min-height: 110px;
}
.spinner-border {
  color: #26b673;
}

.chart-area {
  width: 100%;
  min-height: 310px;
  height: 50vw;
  max-height: 540px;
  margin: 12px auto 0;
}

.placeholder-message {
  font-size: 1.25rem;
  color: #26b673;
  min-height: 38vh;
  text-align: center;
  padding: 1.5rem 0 0 0;
}
@media (max-width: 576px) {
  .placeholder-message {
    font-size: 1.05rem;
    min-height: 20vh;
    padding: 0.6rem 0 0 0;
  }
  .chart-area {
    height: 220px !important;
    min-height: 140px !important;
    max-height: 340px !important;
  }
}
</style>
