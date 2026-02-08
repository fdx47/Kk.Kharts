<template>
  <q-page class="kk-page">
    <q-pull-to-refresh @refresh="onRefresh">
      <!-- Header with Back Button -->
      <div class="detail-header">
        <q-btn flat round icon="mdi-arrow-left" @click="goBack" />
        <div class="header-info">
          <h1 class="page-title">{{ device?.name || 'Capteur' }}</h1>
          <p class="page-subtitle">{{ device?.installationLocation || device?.devEui }}</p>
        </div>
        <span class="kk-badge" :class="device?.isOnline ? 'kk-badge--online' : 'kk-badge--offline'">
          <span class="badge-dot"></span>
          {{ device?.isOnline ? 'Online' : 'Offline' }}
        </span>
      </div>

      <!-- Loading State -->
      <template v-if="loading">
        <div v-for="i in 4" :key="i" class="kk-skeleton kk-skeleton--card mb-3"></div>
      </template>

      <template v-else-if="device">
        <!-- Chart Section -->
        <div class="kk-page__section">
          <div class="section-header chart-controls row items-center justify-center q-mb-sm">
            <div class="row items-center q-gutter-sm">
              <q-btn
                dense
                round
                flat
                :icon="showTooltip ? 'mdi-eye' : 'mdi-eye-off'"
                :color="showTooltip ? 'primary' : 'grey-6'"
                @click.stop="showTooltip = !showTooltip"
              />
              <q-btn
                dense
                round
                flat
                icon="mdi-fullscreen"
                color="grey-7"
                @click.stop="toggleFullscreen"
              />
              <q-btn-toggle
                v-model="selectedPeriod"
                flat
                dense
                toggle-color="primary"
                :options="periodOptions"
                @update:model-value="loadChartData"
              />
            </div>
          </div>
          
          <div class="kk-chart-container" @click="toggleFullscreen">
            <MetricChart
              v-if="chartData.length > 0"
              :data="chartData"
              :variables="activeMetricsKeys"
              :fullscreen="isFullscreen"
              :show-tooltip="showTooltip"
            />
            <div v-else class="chart-placeholder">
              <q-icon name="mdi-chart-line" size="48px" class="text-grey-6" />
              <p class="text-grey-6">Aucune donnée disponible</p>
            </div>
          </div>
        </div>

        <!-- Metrics Grid (Interactive Mini-Charts) -->
        <div class="kk-page__section">
          <div class="mini-charts-grid">
            <div v-for="m in activeMetrics" :key="m.key" class="mini-chart-card">
              <div class="mini-chart-header row justify-between items-center">
                <div class="column">
                  <span class="mini-chart-label">{{ m.label }}</span>
                  <div class="mini-chart-current row items-baseline">
                    <span class="value">{{ getVariableValue(m.key) }}</span>
                    <span class="unit">{{ getVariableUnit(m.key) }}</span>
                  </div>
                </div>
                <div class="mini-chart-icon" :style="{ backgroundColor: m.color + '15', color: m.color }">
                  <q-icon :name="m.icon" size="20px" />
                </div>
              </div>
              <div class="mini-chart-body">
                <MetricChart
                  v-if="chartData.length > 0"
                  :data="chartData"
                  :variables="[m.key]"
                  mini
                  :show-axes="true"
                  height="120px"
                />
              </div>
            </div>
          </div>
        </div>

        <!-- Quick Actions -->
        <div class="kk-page__section">
          <h3 class="section-title">Actions</h3>
          <div class="actions-grid">
            <QuickActionButton
              icon="mdi-bell-cog"
              :label="$t('devices.configureAlerts')"
              color="primary"
              @click="openThresholdModal"
            />
            <QuickActionButton
              icon="mdi-refresh"
              :label="$t('common.refresh')"
              color="secondary"
              @click="refreshData"
            />
            <QuickActionButton
              icon="mdi-fullscreen"
              label="Plein écran"
              color="accent"
              @click="toggleFullscreen"
            />
            <QuickActionButton
              icon="mdi-share"
              label="Partager"
              color="grey"
              @click="shareDevice"
            />
          </div>
        </div>

        <!-- Device Info -->
        <div class="kk-page__section">
          <h3 class="section-title">Informations</h3>
          <div class="kk-card info-card">
            <div class="info-row">
              <span class="info-label">DevEUI</span>
              <span class="info-value">{{ device.devEui }}</span>
            </div>
            <div class="info-row">
              <span class="info-label">Modèle</span>
              <span class="info-value">{{ device.model }}</span>
            </div>
            <div class="info-row">
              <span class="info-label">{{ $t('devices.battery') }}</span>
              <span class="info-value">{{ device.battery }}%</span>
            </div>
            <div class="info-row">
              <span class="info-label">{{ $t('devices.lastSeen') }}</span>
              <span class="info-value">{{ formatDateTime(device.lastSeenAt) }}</span>
            </div>
            <div v-if="device.company" class="info-row">
              <span class="info-label">Entreprise</span>
              <span class="info-value">{{ device.company }}</span>
            </div>
          </div>
        </div>
      </template>

      <!-- Not Found -->
      <template v-else>
        <div class="kk-card text-center py-8">
          <q-icon name="mdi-alert-circle" size="48px" class="text-warning mb-4" />
          <p>Capteur non trouvé</p>
        </div>
      </template>
    </q-pull-to-refresh>

    <!-- Threshold Modal -->
    <ThresholdModal
      v-model="showThresholdModal"
      :dev-eui="devEui"
      :thresholds="thresholds"
      @save="saveThresholds"
    />

    <!-- Fullscreen Chart -->
    <q-dialog v-model="isFullscreen" maximized>
      <div class="kk-chart-container--fullscreen bg-white">
        <q-btn
          flat
          round
          icon="mdi-close"
          class="fullscreen-close text-dark"
          @click="isFullscreen = false"
        />
        <MetricChart
          v-if="chartData.length > 0"
          :data="chartData"
          :variables="activeMetricsKeys"
          :fullscreen="true"
          :show-tooltip="showTooltip"
        />
      </div>
    </q-dialog>
  </q-page>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useDevicesStore } from '@/stores/devices'
import { useSettingsStore } from '@/stores/settings'
import QuickActionButton from '@/components/QuickActionButton.vue'
import MetricChart from '@/components/MetricChart.vue'
import ThresholdModal from '@/components/ThresholdModal.vue'

const router = useRouter()
const route = useRoute()
const devicesStore = useDevicesStore()
const settingsStore = useSettingsStore()

const devEui = computed(() => route.params.devEui as string)
const device = computed(() => devicesStore.getDeviceByDevEui(devEui.value))

const loading = ref(true)
const chartData = ref<any[]>([])
const thresholds = ref<any[]>([])
const selectedPeriod = ref('72h')

const activeMetrics = computed(() => {
  if (chartData.value.length === 0) return []
  
  // Detecção dinâmica de métricas baseada nos dados recebidos do endpoint unificado
  const firstEntry = chartData.value[chartData.value.length - 1] 
  const allMetrics = [
    { key: 'temperature', label: 'Température', color: '#6366f1', icon: 'mdi-thermometer' },
    { key: 'humidity', label: 'Humidité Air', color: '#0ea5e9', icon: 'mdi-water-percent' },
    { key: 'vwc', label: 'VWC (Sol)', color: '#10b981', icon: 'mdi-water' },
    { key: 'ec', label: 'EC (Sol)', color: '#f59e0b', icon: 'mdi-flash' },
    { key: 'soilTemperature', label: 'Temp. Sol', color: '#8b5cf6', icon: 'mdi-thermometer-lines' },
    { key: 'soilHumidity', label: 'Hum. Sol', color: '#0ea5e9', icon: 'mdi-percent' },
    { key: 'mineralVWC', label: 'VWC Sol', color: '#10b981', icon: 'mdi-water-check' },
    { key: 'mineralECp', label: 'EC Sol', color: '#f59e0b', icon: 'mdi-lightning-bolt' }
  ]
  
  return allMetrics.filter(m => firstEntry[m.key] !== undefined && firstEntry[m.key] !== null)
})

const TOOLTIP_STORAGE_KEY = 'kk_show_tooltip'
const activeMetricsKeys = computed(() => activeMetrics.value.map(m => m.key))
const showTooltip = ref<boolean>((() => {
  const cached = localStorage.getItem(TOOLTIP_STORAGE_KEY)
  return cached === 'false' ? false : true
})())

watch(showTooltip, (val) => {
  localStorage.setItem(TOOLTIP_STORAGE_KEY, String(val))
})

function getVariableValue(key: string) {
  if (chartData.value.length === 0) return '--'
  const val = chartData.value[chartData.value.length - 1][key]
  return val != null ? val.toFixed(1) : '--'
}

function getVariableUnit(key: string) {
  if (key.includes('temperature') || key.includes('Temperature')) return '°C'
  if (key.includes('humidity') || key.includes('Humidity') || key.includes('vwc') || key.includes('VWC')) return '%'
  if (key.includes('ec') || key.includes('EC')) return 'µS/cm'
  return ''
}
const isFullscreen = ref(false)
const showThresholdModal = ref(false)

const periodOptions = [
  { label: '36h', value: '36h' },
  { label: '48h', value: '48h' },
  { label: '72h', value: '72h' },
  { label: '5j', value: '5d' },
  { label: '7j', value: '7d' }
]

onMounted(async () => {
  await devicesStore.fetchDevices()
  await loadChartData()
  await loadThresholds()
  loading.value = false
})

async function loadChartData() {
  if (!devEui.value) return
  chartData.value = await devicesStore.fetchDeviceData(devEui.value, selectedPeriod.value)
}

async function loadThresholds() {
  if (!devEui.value) return
  thresholds.value = await devicesStore.fetchDeviceThresholds(devEui.value)
}

async function onRefresh(done: () => void) {
  settingsStore.hapticFeedback('light')
  await Promise.all([
    devicesStore.fetchDevices(true),
    loadChartData(),
    loadThresholds()
  ])
  done()
}

async function refreshData() {
  settingsStore.hapticFeedback('medium')
  loading.value = true
  await Promise.all([
    devicesStore.fetchDevices(true),
    loadChartData()
  ])
  loading.value = false
}

function goBack() {
  settingsStore.hapticFeedback('light')
  router.back()
}

function toggleFullscreen() {
  settingsStore.hapticFeedback('light')
  isFullscreen.value = !isFullscreen.value
}

function openThresholdModal() {
  settingsStore.hapticFeedback('light')
  showThresholdModal.value = true
}

async function saveThresholds(newThresholds: any[]) {
  const success = await devicesStore.updateDeviceThresholds(devEui.value, newThresholds)
  if (success) {
    thresholds.value = newThresholds
    showThresholdModal.value = false
  }
}

function shareDevice() {
  settingsStore.hapticFeedback('light')
  if (navigator.share) {
    navigator.share({
      title: device.value?.name || 'Capteur',
      text: `Données du capteur ${device.value?.name}`,
      url: window.location.href
    })
  }
}

function formatDateTime(dateStr: string | undefined): string {
  if (!dateStr) return '--'
  const date = new Date(dateStr)
  return date.toLocaleString('fr-FR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}
</script>

<style lang="scss" scoped>
.kk-page {
  padding-top: 8px !important;
  padding-bottom: 80px !important;
}

.detail-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 8px;
}

.header-info {
  flex: 1;
  min-width: 0;

  .page-title {
    font-size: 20px;
    margin-bottom: 2px;
  }

  .page-subtitle {
    font-size: 12px;
  }
}

.mini-charts-grid {
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin-bottom: 16px;
}

.mini-chart-card {
  background: white;
  border-radius: 16px;
  padding: 8px;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.05);
  border: 1px solid rgba(0, 0, 0, 0.03);
  display: flex;
  flex-direction: column;
  gap: 6px;
  transition: transform 0.2s;
  overflow: hidden;
  position: relative;

  &:active {
    transform: scale(0.98);
  }
}

.mini-chart-label {
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  color: #94a3b8;
  letter-spacing: 0.5px;
}

.mini-chart-current {
  .value {
    font-size: 24px;
    font-weight: 800;
    color: #1e293b;
    line-height: 1;
  }
  .unit {
    font-size: 12px;
    font-weight: 600;
    color: #64748b;
    margin-left: 2px;
  }
}

.mini-chart-icon {
  width: 36px;
  height: 36px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.mini-chart-body {
  height: 90px;
  margin: -2px -4px 0;
  padding: 0 4px 4px;
  border-radius: 12px;
  overflow: hidden;
  clip-path: none;
}

.mini-chart-body canvas {
  width: 100% !important;
  height: 100% !important;
  border-radius: 12px;
  border-radius: 16px;
}

.metrics-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 12px;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;

  .section-title {
    margin-bottom: 0;
  }
}

.chart-controls {
  justify-content: center;

  > .row {
    justify-content: center;
    flex-wrap: wrap;
  }
}

.chart-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 48px;
  text-align: center;
}

.actions-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 12px;
}

.info-card {
  .info-row {
    display: flex;
    justify-content: space-between;
    padding: 12px 0;
    border-bottom: 1px solid rgba(255, 255, 255, 0.08);

    &:last-child {
      border-bottom: none;
    }
  }

  .info-label {
    opacity: 0.6;
    font-size: 14px;
  }

  .info-value {
    font-weight: 600;
    font-size: 14px;
  }
}

.fullscreen-close {
  position: absolute;
  top: 16px;
  right: 16px;
  z-index: 10;
}

.mb-3 {
  margin-bottom: 12px;
}

.mb-4 {
  margin-bottom: 16px;
}

.py-8 {
  padding-top: 32px;
  padding-bottom: 32px;
}
</style>
