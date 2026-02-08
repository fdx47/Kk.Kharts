<template>
  <div class="p-4">
    <!-- Back button -->
    <button class="flex items-center gap-2 text-tg-link mb-4" @click="goBack">
      <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
      </svg>
      Retour
    </button>

    <div v-if="device">
      <!-- Device header -->
      <div class="flex items-center gap-3 mb-6">
        <div :class="device.isOnline ? 'status-online' : 'status-offline'" class="w-4 h-4"></div>
        <div>
          <h1 class="text-2xl font-bold">{{ device.description || device.name }}</h1>
          <p class="text-tg-hint">{{ device.deviceType }} • {{ device.devEui }}</p>
        </div>
      </div>

      <!-- Modular Metric Cards with Individual Charts -->
      <div class="space-y-4 mb-6">
        <div v-for="m in activeMetrics" :key="m.key" class="tg-card !p-0 overflow-hidden">
          <div class="p-3 flex justify-between items-end bg-tg-secondary-bg/30">
            <div>
              <div class="text-tg-hint text-[10px] uppercase font-bold tracking-wider mb-0.5">{{ m.label }}</div>
              <div class="flex items-baseline gap-1">
                <span class="text-2xl font-bold">{{ getVariableValue(m.key) }}</span>
                <span class="text-tg-hint text-sm">{{ getVariableUnit(m.key) }}</span>
              </div>
            </div>
            <div class="p-1.5 rounded-lg" :style="{ color: m.color, backgroundColor: m.color + '15' }">
              <svg v-if="m.key === 'temperature'" class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
              </svg>
              <svg v-else-if="m.key === 'humidity'" class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.824.185 2 2 0 00-.713 1.405v.293c0 .768.391 1.482 1.054 1.889l.34.204a6 6 0 005.772 0l.34-.204a2 2 0 012.108 0l.34.204a6 6 0 005.772 0l.34-.204a2 2 0 012.108 0l.34.204a6 6 0 005.772 0l.34-.204a2 2 0 011.054-1.889v-.293a2 2 0 00-.713-1.405z" />
              </svg>
              <svg v-else class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 7h8m0 0v8m0-8l-8 8-4-4-6 6" />
              </svg>
            </div>
          </div>
          <div class="h-28 p-2">
            <canvas :ref="el => setCanvasRef(el, m.key)"></canvas>
          </div>
        </div>
      </div>

      <!-- Chart period selector -->
      <div class="flex gap-2 mb-4">
        <button 
          v-for="p in periods" 
          :key="p.value"
          class="flex-1 py-2 rounded-lg text-sm font-medium transition-colors"
          :class="period === p.value ? 'bg-tg-button text-tg-button-text' : 'bg-tg-secondary-bg text-tg-text'"
          @click="setPeriod(p.value)">
          {{ p.label }}
        </button>
      </div>

      <!-- Combined Chart (All-in-one) -->
      <div class="tg-card !p-0 overflow-hidden mb-6 relative group" :class="{ 'chart-fullscreen': isFullscreen }">
        <div class="absolute top-2 right-2 z-10">
          <button @click.stop="toggleFullscreen" class="p-2 rounded-full bg-tg-secondary-bg/50 backdrop-blur-sm hover:bg-tg-secondary-bg transition-colors">
            <svg v-if="!isFullscreen" class="w-5 h-5 opacity-80" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 8V4m0 0h4M4 4l5 5m11-1V4m0 0h-4m4 0l-5 5M4 16v4m0 0h4m-4 0l5-5m11 5l-5-5m5 5v-4m0 4h-4" />
            </svg>
            <svg v-else class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
        <div :class="isFullscreen ? 'flex-1 p-4' : 'h-52 p-4'" @click="!isFullscreen && toggleFullscreen()">
          <canvas ref="combinedChartCanvas"></canvas>
        </div>
      </div>

      <!-- Landscape Prompt -->
      <div v-if="showLandscapePrompt" class="landscape-prompt" @click="showLandscapePrompt = false">
        <div class="bg-black/60 backdrop-blur-md p-8 rounded-3xl flex flex-col items-center">
          <svg class="w-20 h-20 mb-6 rotate-icon text-tg-button" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 19l9 2-9-18-9 18 9-2zm0 0v-8" />
          </svg>
          <p class="text-2xl font-bold mb-2">Plein écran</p>
          <p class="text-tg-hint">Tournez votre appareil horizontalement</p>
          <p class="text-xs mt-6 opacity-30">(Touchez pour ignorer)</p>
        </div>
      </div>

      <!-- Device info -->
      <div class="tg-card mb-4">
        <h3 class="font-semibold mb-3">Informations</h3>
        <div class="space-y-2 text-sm">
          <div class="flex justify-between">
            <span class="text-tg-hint">Dernière lecture</span>
            <span>{{ formatRelativeTime(device.lastSeenAt) }}</span>
          </div>
          <div class="flex justify-between">
            <span class="text-tg-hint">Batterie</span>
            <span :class="getBatteryTextColor(device.battery)">{{ device.battery ?? 0 }}%</span>
          </div>
           <div class="flex justify-between">
            <span class="text-tg-hint">SNR</span>
            <span>{{ (device.snr ?? '--') + ' dB' }}</span>
            <span class="text-tg-hint">RSSI</span>
            <span>{{ (device.rssi ?? '--') + ' dBm' }}</span>
            <span class="text-tg-hint">SF</span>
            <span>{{ (device.sf ?? '--') }}</span>
          </div>
        </div>
      </div>

      <!-- Actions -->
      <div class="space-y-3">
        <button class="tg-button" @click="shareDevice">
          Partager les données
        </button>
        <button class="tg-button-secondary" @click="configureAlerts">
          Configurer les alertes
        </button>
      </div>
    </div>

    <!-- Loading state -->
    <div v-else class="text-center py-12">
      <div class="w-10 h-10 border-4 border-tg-button border-t-transparent rounded-full animate-spin mx-auto mb-4"></div>
      <p class="text-tg-hint">Chargement...</p>
    </div>

    <!-- Alarms Configuration Modal -->
    <Transition name="fade">
      <div v-if="showAlarmsModal" class="fixed inset-0 z-[100] flex flex-col bg-tg-bg overflow-hidden">
        <!-- Modal Header -->
        <div class="flex items-center justify-between p-4 border-b border-tg-secondary-bg">
          <button class="text-tg-link" @click="showAlarmsModal = false">Annuler</button>
          <h2 class="text-lg font-bold">Alertes</h2>
          <button class="text-tg-link font-bold" @click="saveAlerts">Enregistrer</button>
        </div>

        <!-- Modal Content -->
        <div class="flex-1 overflow-y-auto p-4 space-y-6 pb-20">
          <div v-if="loadingThresholds" class="flex flex-col items-center justify-center h-40">
            <div class="w-8 h-8 border-3 border-tg-button border-t-transparent rounded-full animate-spin mb-2"></div>
            <p class="text-tg-hint text-sm">Chargement...</p>
          </div>
          <div v-else v-for="(t, idx) in editingThresholds" :key="t.sensorType" class="tg-card">
            <div class="flex items-center justify-between mb-4">
              <div class="flex items-center gap-2">
                <div class="w-2 h-8 rounded-full" :style="{ backgroundColor: getMetricColor(t.sensorType) }"></div>
                <span class="font-bold text-lg">{{ getMetricLabel(t.sensorType) }}</span>
              </div>
              <label class="relative inline-flex items-center cursor-pointer">
                <input type="checkbox" v-model="t.isEnabled" class="sr-only peer">
                <div class="w-11 h-6 bg-tg-secondary-bg peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full rtl:peer-checked:after:-translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:start-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-tg-button"></div>
              </label>
            </div>

            <div v-if="t.isEnabled" class="space-y-4">
              <!-- Global Thresholds -->
              <div v-if="!t.useTimePeriods" class="grid grid-cols-2 gap-4">
                <div>
                  <label class="text-[10px] text-tg-hint uppercase block mb-1">Min ({{ getVariableUnit(t.sensorType) }})</label>
                  <input type="number" v-model="t.minValue" class="tg-input w-full text-center" placeholder="--">
                </div>
                <div>
                  <label class="text-[10px] text-tg-hint uppercase block mb-1">Max ({{ getVariableUnit(t.sensorType) }})</label>
                  <input type="number" v-model="t.maxValue" class="tg-input w-full text-center" placeholder="--">
                </div>
              </div>

              <!-- Time Periods Toggle -->
              <div class="flex items-center justify-between pt-2 border-t border-tg-secondary-bg">
                <span class="text-sm font-medium">Utiliser des périodes horaires</span>
                <label class="relative inline-flex items-center cursor-pointer scale-90">
                  <input type="checkbox" v-model="t.useTimePeriods" class="sr-only peer">
                  <div class="w-11 h-6 bg-tg-secondary-bg peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full rtl:peer-checked:after:-translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:start-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-tg-button"></div>
                </label>
              </div>

              <!-- Periods List -->
              <div v-if="t.useTimePeriods" class="space-y-3 pt-2">
                <div v-for="(p, pIdx) in t.periods" :key="pIdx" class="bg-tg-secondary-bg/50 p-3 rounded-xl border border-tg-secondary-bg relative">
                  <button @click="removePeriod(t, pIdx)" class="absolute -top-2 -right-2 w-6 h-6 bg-tg-danger text-white rounded-full flex items-center justify-center shadow-lg">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path d="M6 18L18 6M6 6l12 12" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>
                  </button>
                  <input v-model="p.name" class="bg-transparent font-bold text-sm w-full mb-3 focus:outline-none border-b border-tg-secondary-bg pb-1" placeholder="Nom de la période">
                  
                  <div class="grid grid-cols-2 gap-3 mb-3">
                    <div>
                      <label class="text-[9px] text-tg-hint uppercase">Début</label>
                      <input type="time" v-model="p.startTime" class="tg-input w-full text-xs py-1 px-2 text-center">
                    </div>
                    <div>
                      <label class="text-[9px] text-tg-hint uppercase">Fin</label>
                      <input type="time" v-model="p.endTime" class="tg-input w-full text-xs py-1 px-2 text-center">
                    </div>
                  </div>

                  <div class="grid grid-cols-2 gap-3">
                    <div>
                      <label class="text-[9px] text-tg-hint uppercase">Min</label>
                      <input type="number" v-model="p.minValue" class="tg-input w-full text-xs py-1 px-2 text-center">
                    </div>
                    <div>
                      <label class="text-[9px] text-tg-hint uppercase">Max</label>
                      <input type="number" v-model="p.maxValue" class="tg-input w-full text-xs py-1 px-2 text-center">
                    </div>
                  </div>
                </div>

                <button @click="addPeriod(t)" class="w-full py-2 border-2 border-dashed border-tg-secondary-bg rounded-xl text-tg-hint text-sm flex items-center justify-center gap-2 hover:border-tg-button hover:text-tg-button transition-colors">
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path d="M12 4v16m8-8H4" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>
                  Ajouter une période
                </button>
              </div>
            </div>
            <div v-else class="text-tg-hint text-sm italic">Désactivé</div>
          </div>
        </div>
      </div>
    </Transition>
  </div>
</template>

<script setup>
import MetricCard from '../components/MetricCard.vue'
import { ref, computed, onMounted, watch, nextTick } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAppStore } from '../stores/app'
import { formatTimeAgo } from '../utils/time'
import { Chart, registerables } from 'chart.js'

Chart.register(...registerables)

const route = useRoute()
const router = useRouter()
const store = useAppStore()

const chartInstances = {}
const canvasRefs = ref({})
const combinedChartCanvas = ref(null)
const combinedChartInstance = ref(null)
const chartData = ref([])
const period = ref('36h')
const isFullscreen = ref(false)
const showLandscapePrompt = ref(false)
const showAlarmsModal = ref(false)
const editingThresholds = ref([])
const loadingThresholds = ref(false)
const formatRelativeTime = (date) => formatTimeAgo(date)

function setCanvasRef(el, key) {
  if (el) canvasRefs.value[key] = el
}

const periods = [
  { value: '36h', label: '36h' },
   { value: '48h', label: '48h' },
   { value: '72h', label: '72h' },
   { value: '7d', label: '7j' },
]

const dashboardState = ref(null)

const device = computed(() => {
  return store.devices.find(d => d.devEui === route.params.devEui)
})

const activeMetrics = computed(() => {
  if (chartData.value.length === 0) return []
  const firstEntry = chartData.value[0]
  const metrics = [
    { key: 'temperature', label: 'Température', color: '#22c55e' },
    { key: 'humidity', label: 'Humidité Air', color: '#3b82f6' },
    { key: 'vwc', label: 'VWC (Humidité Sol)', color: '#8b5cf6' },
    { key: 'ec', label: 'EC (Conductivité)', color: '#f59e0b' }
  ]
  
  return metrics.filter(m => firstEntry[m.key] !== undefined && firstEntry[m.key] !== null)
})

onMounted(async () => {
  console.log('[DeviceDetail] Component mounted, devEui:', route.params.devEui)
})

// Use a watcher for the device instead of relying solely on onMounted
// This handles cases where the devices list loads after the component mounts
watch(device, async (newDevice, oldDevice) => {
  if (newDevice && (!oldDevice || newDevice.devEui !== oldDevice.devEui)) {
    console.log('[DeviceDetail] Device found or changed, loading data for:', newDevice.devEui)
    try {
      loadingThresholds.value = true
      dashboardState.value = await store.fetchDashboardState(newDevice.devEui)
      await loadChartData()
    } catch (e) {
      console.error('[DeviceDetail] Error during initial data load:', e)
    } finally {
      loadingThresholds.value = false
    }
  }
}, { immediate: true })

watch(period, async () => {
  console.log('[DeviceDetail] Period changed:', period.value)
  try {
    await loadChartData()
  } catch (e) {
    console.error('[DeviceDetail] Error updating chart data for period:', e)
  }
})

async function loadChartData() {
  if (!device.value) {
    console.warn('[DeviceDetail] loadChartData called without device active')
    return
  }
  
  try {
    // Calculate dates from period
    const end = new Date()
    const start = new Date()
    const p = period.value
    
    if (typeof p === 'string') {
      if (p.endsWith('h')) {
        const hours = parseInt(p)
        start.setTime(end.getTime() - hours * 60 * 60 * 1000)
      } else if (p.endsWith('d')) {
        const days = parseInt(p)
        start.setDate(end.getDate() - days)
      } else {
        // Default 24h
        start.setTime(end.getTime() - 24 * 60 * 60 * 1000)
      }
    }

    console.log('[DeviceDetail] Fetching data for:', device.value.devEui, 'range:', start, end)
    chartData.value = await store.fetchDeviceData(device.value.devEui, start, end)
    await nextTick()
    renderCharts()
    renderCombinedChart()
    console.log('[DeviceDetail] Data loaded and charts rendered')
  } catch (e) {
    console.error('[DeviceDetail] Failed to load chart data:', e)
    store.showToast('Erreur lors du chargement des données', 'error')
  }
}

function renderCharts() {
  activeMetrics.value.forEach(m => {
    renderMetricChart(m)
  })
}

function renderCombinedChart() {
  if (!combinedChartCanvas.value || chartData.value.length === 0) return

  if (combinedChartInstance.value) {
    combinedChartInstance.value.destroy()
  }

  const ctx = combinedChartCanvas.value.getContext('2d')
  // Telegram WebApp logic removed
  
  const datasets = activeMetrics.value.map(m => ({
    label: m.label,
    data: chartData.value.map(d => d[m.key]),
    borderColor: m.color,
    backgroundColor: m.color + '10',
    fill: false,
    tension: 0.1,
    borderWidth: 1.2,
    pointRadius: 0,
    yAxisID: `y_${m.key}`
  }))

  combinedChartInstance.value = new Chart(ctx, {
    type: 'line',
    data: {
      labels: chartData.value.map(d => formatChartLabel(d.time || d.timestamp)),
      datasets: datasets
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          display: true,
          position: 'top',
          labels: {
            usePointStyle: true,
            boxWidth: 6,
            font: { size: 10 },
            color: tg?.themeParams?.text_color || '#000000'
          }
        },
        tooltip: {
          mode: 'index',
          intersect: false
        }
      },
      scales: {
        x: {
          grid: { display: false },
          ticks: {
            color: tg?.themeParams?.hint_color || '#999999',
            font: { size: 9 },
            maxTicksLimit: 6
          }
        },
        ...Object.fromEntries(activeMetrics.value.map((m, idx) => [
          `y_${m.key}`,
          {
            type: 'linear',
            display: isFullscreen.value || idx === 0 || idx === 1, // Only show first two axes in compact mode
            position: idx % 2 === 0 ? 'left' : 'right',
            grid: {
              display: idx === 0, // Only show grid for the first axis
              color: tg?.themeParams?.hint_color + '10'
            },
            ticks: {
              color: m.color,
              font: { size: 8 },
              callback: (value) => value.toFixed(1)
            },
            title: {
              display: isFullscreen.value,
              text: m.label,
              color: m.color,
              font: { size: 10, weight: 'bold' }
            }
          }
        ]))
      }
    }
  })
}

function renderMetricChart(m) {
  const canvas = canvasRefs.value[m.key]
  if (!canvas || chartData.value.length === 0) return

  if (chartInstances[m.key]) {
    chartInstances[m.key].destroy()
  }

  const ctx = canvas.getContext('2d')
  const tg = window.Telegram?.WebApp
  
  const gradient = ctx.createLinearGradient(0, 0, 0, 160)
  gradient.addColorStop(0, m.color + '40')
  gradient.addColorStop(1, m.color + '00')

  chartInstances[m.key] = new Chart(ctx, {
    type: 'line',
    data: {
      labels: chartData.value.map(d => formatChartLabel(d.time || d.timestamp)),
      datasets: [{
        label: m.label,
        data: chartData.value.map(d => d[m.key]),
        borderColor: m.color,
        backgroundColor: gradient,
        fill: true,
        tension: 0.1,
        borderWidth: 1,
        pointRadius: 0, // No points for cleaner look
        pointHoverRadius: 4,
        hitRadius: 10
      }]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { display: false },
        tooltip: {
          mode: 'index',
          intersect: false,
          backgroundColor: tg?.themeParams?.secondary_bg_color || '#ffffff',
          titleColor: tg?.themeParams?.text_color || '#000000',
          bodyColor: tg?.themeParams?.text_color || '#000000',
          borderColor: m.color + '40',
          borderWidth: 1,
          padding: 8,
          displayColors: false,
          callbacks: {
            label: (context) => `${context.parsed.y} ${getVariableUnit(m.key)}`
          }
        }
      },
      scales: {
        x: {
          display: true,
          grid: { display: false },
          ticks: {
            color: tg?.themeParams?.hint_color || '#999999',
            maxRotation: 0,
            autoSkip: true,
            maxTicksLimit: 5,
            font: { size: 9 }
          }
        },
        y: {
          display: true,
          grid: {
            color: tg?.themeParams?.hint_color + '10' || '#99999910'
          },
          ticks: {
            color: tg?.themeParams?.hint_color || '#999999',
            font: { size: 9 },
            maxTicksLimit: 4
          }
        }
      },
      interaction: {
        intersect: false,
        mode: 'index'
      }
    }
  })
}

function setPeriod(p) {
  store.hapticFeedback('light')
  period.value = p
}

function goBack() {
  store.hapticFeedback('light')
  router.back()
}

function formatChartLabel(timestamp) {
  if (!timestamp) return ''
  const date = new Date(timestamp)
  if (Number.isNaN(date.getTime())) return ''
  
  if (period.value === '6h' || period.value === '24h') {
    return date.toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' })
  }
  return date.toLocaleDateString('fr-FR', { day: '2-digit', month: '2-digit' })
}

function getBatteryTextColor(level) {
  if (level > 50) return 'text-kk-primary'
  if (level > 20) return 'text-kk-warning'
  return 'text-kk-danger'
}

function shareDevice() {
  store.hapticFeedback('medium')
  const tg = window.Telegram?.WebApp
  const text = `📊 ${device.value.description || device.value.name}\n🌡️ ${device.value.lastTemperature?.toFixed(1)}°C\n💧 ${device.value.lastHumidity?.toFixed(0)}%`
  tg?.switchInlineQuery(text, ['users', 'groups', 'channels'])
}

async function configureAlerts() {
  store.hapticFeedback('light')
  showAlarmsModal.value = true
  loadingThresholds.value = true
  try {
    const data = await store.fetchDeviceThresholds(device.value.devEui)
    // Garantir que todos os sensores ativos têm uma entrada
    const thresholds = activeMetrics.value.map(m => {
      const existing = data.find(t => t.sensorType === m.key)
      return existing || {
        sensorType: m.key,
        minValue: null,
        maxValue: null,
        isEnabled: false,
        useTimePeriods: false,
        periods: []
      }
    })
    editingThresholds.value = JSON.parse(JSON.stringify(thresholds)) // Deep clone
  } catch (e) {
    store.showToast('Erreur lors do chargement des alertes', 'error')
  } finally {
    loadingThresholds.value = false
  }
}

async function saveAlerts() {
  store.hapticFeedback('medium')
  const success = await store.updateDeviceThresholds(device.value.devEui, editingThresholds.value)
  if (success) {
    showAlarmsModal.value = false
  }
}

function addPeriod(threshold) {
  store.hapticFeedback('light')
  threshold.periods.push({
    name: `Période ${threshold.periods.length + 1}`,
    startTime: '00:00',
    endTime: '23:59',
    minValue: threshold.minValue,
    maxValue: threshold.maxValue,
    isEnabled: true
  })
}

function removePeriod(threshold, index) {
  store.hapticFeedback('light')
  threshold.periods.splice(index, 1)
}

// Fullscreen & Orientation logic
function checkOrientation() {
  if (isFullscreen.value && window.matchMedia("(orientation: portrait)").matches) {
    showLandscapePrompt.value = true
  } else {
    showLandscapePrompt.value = false
  }
}

function toggleFullscreen() {
  isFullscreen.value = !isFullscreen.value
  
  if (isFullscreen.value) {
    window.addEventListener('resize', checkOrientation)
    checkOrientation()
    document.body.style.overflow = 'hidden'
  } else {
    window.removeEventListener('resize', checkOrientation)
    showLandscapePrompt.value = false
    document.body.style.overflow = ''
  }
  
  setTimeout(() => {
    if (combinedChartInstance.value) {
      combinedChartInstance.value.resize()
    }
  }, 100)
}

function getMetricLabel(key) {
  const metric = activeMetrics.value.find(m => m.key === key)
  return metric ? metric.label : key
}

function getMetricColor(key) {
  const metric = activeMetrics.value.find(m => m.key === key)
  return metric ? metric.color : '#999999'
}

watch(isFullscreen, (val) => {
  if (!val) {
     document.body.style.overflow = ''
     window.removeEventListener('resize', checkOrientation)
  }
})



// Helper to get variable value
function getVariableValue(variable) {
  if (!variable || !device.value) return '--'
  
  const key = variable.toLowerCase()
  const d = device.value
  const s = dashboardState.value || {}

  let val = null

  if (key.includes('temp')) {
    val = s.temperature ?? d.lastTemperature
  } else if (key.includes('hum') && !key.includes('sol')) { // Humidity air
    val = s.humidity ?? d.lastHumidity
  } else if (key.includes('vwc') || key.includes('sol')) {
    val = s.vwc ?? d.lastVwc
  } else if (key.includes('ec') || key.includes('cond')) {
    val = s.ec ?? d.lastEc
  } else if (key.includes('bat')) {
    val = d.battery
  }

  if (val === null || val === undefined) return '--'
  
  // Format based on type
  if (key.includes('bat') || key.includes('hum')) return val.toFixed(0)
  return val.toFixed(1)
}

function getVariableUnit(variable) {
  if (!variable) return ''
  const key = variable.toLowerCase()
  
  if (key.includes('temp')) return '°C'
  if (key.includes('hum') && !key.includes('sol')) return '%'
  if (key.includes('vwc') || key.includes('sol')) return '%'
  if (key.includes('ec') || key.includes('cond')) return 'µS/cm' // or mS/cm depending on sensor
  if (key.includes('bat')) return '%'
  
  return ''
}
</script>

<style scoped>
.chart-fullscreen {
  position: fixed !important;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  margin: 0 !important;
  border-radius: 0 !important;
  z-index: 100 !important;
  background: var(--tg-theme-bg-color, #ffffff);
  display: flex;
  flex-direction: column;
}

.landscape-prompt {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.4);
  backdrop-filter: blur(4px);
  -webkit-backdrop-filter: blur(4px);
  color: white;
  z-index: 110;
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
}

.rotate-icon {
  animation: rotate 2s infinite ease-in-out;
}

@keyframes rotate {
  0% { transform: rotate(0deg); }
  35% { transform: rotate(90deg); }
  65% { transform: rotate(90deg); }
  100% { transform: rotate(0deg); }
}

/* Modal Transitions */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

/* Transition for the slide up effect if we want to add it later */
.slide-up-enter-active,
.slide-up-leave-active {
  transition: transform 0.3s ease-out;
}

.slide-up-enter-from,
.slide-up-leave-to {
  transform: translateY(100%);
}

.tg-input {
  background-color: var(--tg-theme-secondary-bg-color, #f4f4f5);
  color: var(--tg-theme-text-color, #000000);
  border: 1px solid rgba(0,0,0,0.1);
  padding: 8px 12px;
  border-radius: 10px;
  font-size: 14px;
}

.dark .tg-input {
  border-color: rgba(255,255,255,0.1);
}

input[type="time"].tg-input {
  appearance: none;
  -webkit-appearance: none;
}
</style>
