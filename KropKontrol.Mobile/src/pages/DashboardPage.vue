<template>
  <q-page class="dashboard-page">
    <q-pull-to-refresh @refresh="onRefresh" color="primary">
      
      <!-- Cabeçalho premium com gradiente -->
      <div class="dashboard-header">
        <!-- Elementos decorativos de fundo -->
        <div class="decorative-circle-1"></div>
        <div class="decorative-circle-2"></div>
        
        <div class="row justify-between items-center relative-position z-top text-white">
          <div>
            <div class="text-subtitle2 text-white-alpha-70">{{ greeting }}</div>
            <div class="text-h4 text-weight-bold">{{ userName }}</div>
          </div>
          <q-avatar size="56px" class="bg-white-alpha-20 shadow-5 bordered-avatar cursor-pointer" @click="goToSettings">
            <span class="text-weight-bold text-white">{{ userInitials }}</span>
          </q-avatar>
        </div>

        <!-- Resumo das estatísticas -->
        <div class="row q-mt-lg q-col-gutter-md stats-grid">
          <div class="col-3">
            <div class="stat-card glass-card" @click="goToDevicesAll">
              <div class="stat-value">{{ devicesStore.devices.length }}</div>
              <div class="stat-label">Total</div>
            </div>
          </div>
          <div class="col-3">
            <div class="stat-card glass-card" @click="goToDevicesOnline">
              <div class="stat-value">{{ devicesStore.onlineDevices.length }}</div>
              <div class="stat-label">En Ligne</div>
            </div>
          </div>
          <div class="col-3">
            <div class="stat-card glass-card" @click="goToDevicesOffline">
              <div class="stat-value" :class="{'text-orange-400': devicesStore.offlineDevices.length > 0}">{{ devicesStore.offlineDevices.length }}</div>
              <div class="stat-label">Hors Ligne</div>
            </div>
          </div>
          <div class="col-3">
            <div class="stat-card glass-card" @click="goToAlerts">
              <div class="stat-value" :class="{'text-orange-400': alertsStore.alertCount > 0}">{{ alertsStore.alertCount }}</div>
              <div class="stat-label">Alertes</div>
            </div>
          </div>
        </div>
      </div>

      <!-- Quick Actions -->
      <div class="actions-section">
        <q-btn
          unelevated
          class="full-width action-btn bg-blue-1 text-blue-9"
          padding="16px"
          @click="refreshData"
          :loading="devicesStore.loading"
        >
          <div class="row items-center q-col-gutter-sm">
            <div class="col-auto">
              <q-icon name="mdi-refresh" size="24px" />
            </div>
            <div class="col">
              <span class="text-body2 text-weight-medium">Actualiser</span>
            </div>
          </div>
        </q-btn>
      </div>

      <!-- Recent Devices -->
      <div class="devices-section">
        <div class="row justify-between items-center q-mb-md">
          <div class="section-title">Capteurs Récents</div>
          <q-btn flat dense color="primary" label="Voir tout" no-caps class="text-weight-bold" @click="goToDevicesAll" />
        </div>
        
        <template v-if="devicesStore.loading && !devicesStore.devices.length">
          <div v-for="n in 3" :key="n" class="q-mb-md">
            <q-skeleton height="80px" class="radius-16" />
          </div>
        </template>

        <template v-else-if="devicesStore.devices.length === 0">
          <div class="text-center q-py-xl bg-white radius-16 shadow-1">
            <div class="text-h2">📡</div>
            <div class="text-grey-6 q-mt-sm">Aucun capteur trouvé</div>
            <q-btn flat color="primary" label="Actualiser" class="q-mt-sm text-weight-bold" @click="refreshData" />
          </div>
        </template>

        <div v-else class="devices-grid">
          <q-card
            v-for="device in recentDevices"
            :key="device.devEui"
            class="device-card cursor-pointer"
            v-ripple
            @click="goToDevice(device.devEui)"
          >
            <q-card-section class="row items-center no-wrap q-pa-sm">
              <div class="status-indicator q-mr-sm" :class="device.isOnline ? 'bg-green-1 text-green-7' : 'bg-grey-2 text-grey-5'">
                <q-icon name="mdi-wifi" size="18px" />
              </div>

              <div class="col overflow-hidden">
                <div class="text-subtitle2 text-weight-bold text-grey-9 ellipsis">{{ device.description || device.name }}</div>
                <div class="row items-center text-caption text-grey-6">
                  <span class="ellipsis max-width-100">{{ device.model }}</span>
                  <span class="text-grey-4 q-mx-xs">•</span>
                  <span>{{ formatRelativeTime(device.lastSeenAt) }}</span>
                </div>
              </div>

              <div class="column items-end q-ml-sm">
                <div v-if="device.lastTemperature" class="text-subtitle2 text-weight-bolder text-grey-9">
                  {{ device.lastTemperature.toFixed(1) }}°C
                </div>
                <div class="text-caption text-weight-medium" :class="getBatteryColorText(device.battery)">
                  {{ device.battery || 0 }}%
                </div>
              </div>
            </q-card-section>
          </q-card>
        </div>
      </div>

      <!-- Support Link -->
      <div class="support-section">
        <q-btn
          flat
          dense
          color="grey-6"
          icon="mdi-message-text-outline"
          label="Support"
          no-caps
          class="text-caption"
          @click="goToSupport"
        />
      </div>
    </q-pull-to-refresh>
  </q-page>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useDevicesStore } from '@/stores/devices'
import { useAlertsStore } from '@/stores/alerts'
import { useSettingsStore } from '@/stores/settings'
import { formatTimeAgo } from '@/utils/time'

const router = useRouter()
const authStore = useAuthStore()
const devicesStore = useDevicesStore()
const alertsStore = useAlertsStore()
const settingsStore = useSettingsStore()

const refreshing = ref(false)
const isMounted = ref(false)

const userName = computed(() => {
  const name = authStore.user?.name || ''
  if (!name || name === 'Utilisateur') {
    // Fallback para email se name não estiver disponível ou for valor padrão
    const email = authStore.user?.email || ''
    return email ? email.split('@')[0] : 'Utilisateur'
  }
  return name
})

const userInitials = computed(() => {
  const name = authStore.user?.name || authStore.user?.email || 'U'
  return name.substring(0, 2).toUpperCase()
})

const greeting = computed(() => {
  const hour = new Date().getHours()
  if (hour < 6) return 'Bonne nuit'
  if (hour < 12) return 'Bonjour'
  if (hour < 18) return 'Bon après-midi'
  return 'Bonsoir'
})

const recentDevices = computed(() => {
  // Garantir ordenação por data para consistência com Mini App
  return [...devicesStore.devices]
    .filter(d => d.devEui !== '0000000000000000')
    .sort((a, b) => {
      const dateA = a.lastSeenAt ? new Date(a.lastSeenAt).getTime() : 0
      const dateB = b.lastSeenAt ? new Date(b.lastSeenAt).getTime() : 0
      return dateB - dateA
    })
    .slice(0, 5)
})

onMounted(async () => {
  isMounted.value = true
  // Validar token e carregar dados do usuário se necessário
  if (authStore.token && !authStore.user) {
    await authStore.validateToken()
  }
  await loadData()
})

onUnmounted(() => {
  isMounted.value = false
})

async function onRefresh(done: () => void) {
  settingsStore.hapticFeedback('light')
  await loadData(true)
  done()
}

async function loadData(force = false) {
  if (!isMounted.value) return
  
  await Promise.all([
    devicesStore.fetchDevices(force),
    alertsStore.fetchAlerts()
  ])
}

async function refreshData() {
  settingsStore.hapticFeedback('medium')
  await loadData(true)
}

function goToDevices(filter?: string) {
  settingsStore.hapticFeedback('light')
  router.push({ name: 'devices', query: filter ? { filter } : undefined })
}

function goToDevicesOnline() { goToDevices('online') }
function goToDevicesOffline() { goToDevices('offline') }
function goToDevicesAll() { goToDevices() }

function goToDevice(devEui: string) {
  settingsStore.hapticFeedback('light')
  router.push({ name: 'device-detail', params: { devEui } })
}

function goToAlerts() {
  settingsStore.hapticFeedback('light')
  router.push({ name: 'alerts' })
}

function goToSupport() {
  settingsStore.hapticFeedback('light')
  router.push({ name: 'support' })
}

function goToSettings() {
  settingsStore.hapticFeedback('light')
  router.push({ name: 'settings' })
}

function formatRelativeTime(dateStr: string) {
  return formatTimeAgo(dateStr)
}

function getBatteryColorText(level: number) {
  if (level > 50) return 'text-green-7'
  if (level > 20) return 'text-orange-8'
  return 'text-red-8'
}
</script>

<style lang="scss" scoped>
.dashboard-page {
  background: #f6f7fb;
  max-width: 430px;
  margin: 0 auto;
  min-height: 100vh;
}

.dashboard-header {
  background: linear-gradient(135deg, #6366f1 0%, #8b5cf6 50%, #a855f7 100%);
  padding: 24px 16px 20px;
  border-bottom-left-radius: 32px;
  border-bottom-right-radius: 32px;
  box-shadow: 0 10px 30px -10px rgba(99, 102, 241, 0.5);
  position: relative;
  overflow: hidden;
}

.decorative-circle-1 {
  position: absolute;
  width: 200px;
  height: 200px;
  background: radial-gradient(circle, rgba(255,255,255,0.15) 0%, rgba(255,255,255,0) 70%);
  border-radius: 50%;
  top: -80px;
  right: -60px;
  filter: blur(20px);
}

.decorative-circle-2 {
  position: absolute;
  width: 150px;
  height: 150px;
  background: radial-gradient(circle, rgba(255,255,255,0.1) 0%, rgba(255,255,255,0) 70%);
  border-radius: 50%;
  bottom: -40px;
  left: -30px;
  filter: blur(15px);
}

.text-white-alpha-70 { color: rgba(255,255,255,0.7); }

.bordered-avatar {
  border: 2px solid rgba(255, 255, 255, 0.3);
}

.stats-grid {
  display: flex;
  align-items: stretch;
}

.stat-card {
  background: rgba(255, 255, 255, 0.15);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 20px;
  padding: 12px;
  text-align: center;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  flex-direction: column;
  justify-content: center;
  height: 100%;
  
  &:hover {
    transform: translateY(-2px);
    background: rgba(255, 255, 255, 0.2);
  }
  
  .stat-value {
    font-size: 24px;
    font-weight: 700;
    color: white;
    margin-bottom: 2px;
  }
  
  .stat-label {
    font-size: 10px;
    font-weight: 600;
    color: rgba(255, 255, 255, 0.8);
    text-transform: uppercase;
    letter-spacing: 0.5px;
  }
}

.glass-card {
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.actions-section {
  padding: 20px 16px;
}

.section-title {
  font-size: 16px;
  font-weight: 700;
  color: #1e293b;
  margin-bottom: 12px;
}

.action-btn {
  border-radius: 16px;
  transition: all 0.3s ease;
  
  &:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  }
}

.devices-section {
  padding: 0 16px 20px;
}

.devices-grid {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.device-card {
  background: #ffffff;
  border-radius: 14px;
  border: 1px solid #e8ecf2;
  box-shadow: 0 6px 16px -10px rgba(15, 23, 42, 0.18);
  transition: transform 0.12s ease, box-shadow 0.12s ease;

  &:hover {
    transform: translateY(-2px);
    box-shadow: 0 10px 20px -12px rgba(15, 23, 42, 0.25);
  }
}

.mini-chart-card {
  background: white;
  border-radius: 16px;
  padding: 8px;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.05);
  border: 1px solid rgba(0, 0, 0, 0.03);
  display: flex;
  flex-direction: column;
  gap: 4px;
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
    font-weight: 700;
    color: #1e293b;
    line-height: 1;
    margin-left: 2px;
  }

  .unit {
    font-size: 14px;
    font-weight: 500;
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
  border-radius: 16px;
}

.device-info {
  padding: 8px 12px;
  display: flex;
  flex-direction: column;
  gap: 4px;
  height: 100%;
  justify-content: space-between;
}

.device-model {
  font-size: 12px;
  color: #64748b;
  font-weight: 500;
}

.device-time {
  font-size: 11px;
  color: #94a3b8;
}

.device-battery {
  font-size: 11px;
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 4px;
}

.text-truncate {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.radius-16 {
  border-radius: 16px;
}

.max-width-100 {
  max-width: 100px;
}

.support-section {
  padding: 16px;
  text-align: center;
  border-top: 1px solid rgba(0, 0, 0, 0.05);
  margin-top: 20px;
}
</style>
