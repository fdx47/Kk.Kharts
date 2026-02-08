<template>
  <div class="p-4">
    <div class="flex items-center justify-between mb-4">
      <h1 class="text-2xl font-bold">Alertes</h1>
      <button 
        class="text-tg-link text-sm flex items-center gap-1"
        @click="refreshAlerts">
        <svg class="w-4 h-4" :class="{ 'animate-spin': refreshing }" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
        </svg>
        Actualiser
      </button>
    </div>

    <!-- Loading -->
    <div v-if="store.loading" class="text-center py-12">
      <div class="w-8 h-8 border-2 border-tg-button border-t-transparent rounded-full animate-spin mx-auto mb-4"></div>
      <p class="text-tg-hint">Chargement...</p>
    </div>

    <!-- Not linked message -->
    <div v-else-if="!store.isLinked" class="text-center py-12">
      <svg class="w-16 h-16 mx-auto text-kk-warning mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.828 10.172a4 4 0 00-5.656 0l-4 4a4 4 0 105.656 5.656l1.102-1.101m-.758-4.899a4 4 0 005.656 0l4-4a4 4 0 00-5.656-5.656l-1.1 1.1" />
      </svg>
      <h2 class="text-lg font-semibold mb-2">Compte non lié</h2>
      <p class="text-tg-hint mb-4">Liez votre compte pour voir vos alertes</p>
      <p class="text-sm text-tg-hint">Utilisez <code class="bg-tg-secondary-bg px-2 py-1 rounded">/link</code> dans le bot</p>
    </div>

    <template v-else>
      <!-- Active alerts -->
      <div v-if="store.activeAlerts.length > 0" class="space-y-3 mb-6">
        <div class="flex items-center justify-between">
          <h2 class="text-sm font-semibold text-kk-danger flex items-center gap-2">
            <span class="w-2 h-2 bg-kk-danger rounded-full animate-pulse"></span>
            {{ store.activeAlerts.length }} alerte(s) active(s)
          </h2>
          <div class="flex gap-2">
            <button 
              class="text-xs px-3 py-1 rounded-full bg-gray-200 text-gray-700 hover:bg-gray-300"
              @click="muteAllAlerts">
              Silencier tous
            </button>
            <button 
              class="text-xs px-3 py-1 rounded-full bg-kk-primary text-white hover:bg-kk-primary/80"
              @click="activateAllAlerts">
              Activer tous
            </button>
          </div>
        </div>
        <div 
          v-for="alert in store.activeAlerts" 
          :key="alert.id"
          class="tg-card border-l-4"
          :class="getAlertBorderColor(alert.type)">
          <div class="flex items-start justify-between">
            <div class="flex items-start gap-3 flex-1">
              <div class="mt-1">
                <svg v-if="alert.type === 'threshold'" class="w-5 h-5 text-kk-danger" fill="currentColor" viewBox="0 0 20 20">
                  <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clip-rule="evenodd" />
                </svg>
                <svg v-else class="w-5 h-5 text-kk-warning" fill="currentColor" viewBox="0 0 20 20">
                  <path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd" />
                </svg>
              </div>
              <div class="flex-1">
                <div class="font-medium">{{ alert.deviceName }}</div>
                <div class="text-sm text-tg-hint">{{ alert.message }}</div>
                <div v-if="alert.value !== undefined" class="text-xs text-tg-hint mt-1">
                  Valeur: {{ alert.value }} | Seuil: {{ alert.threshold }}
                </div>
                <div class="text-xs text-tg-hint mt-1">{{ formatRelativeTime(alert.triggeredAt) }}</div>
              </div>
            </div>
            <div class="flex gap-2 ml-2">
              <button 
                class="text-tg-link text-sm px-2 py-1 rounded hover:bg-tg-secondary-bg"
                @click="viewDevice(alert.devEui)">
                Voir
              </button>
              <button 
                class="text-xs px-2 py-1 rounded"
                :class="alert.isActive ? 'bg-kk-danger text-white hover:bg-kk-danger/80' : 'bg-gray-200 text-gray-700 hover:bg-gray-300'"
                @click="toggleAlert(alert.id)">
                {{ alert.isActive ? '🔔' : '🔕' }}
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- No alerts -->
      <div v-else class="text-center py-12">
        <svg class="w-16 h-16 mx-auto text-kk-primary mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
        <h2 class="text-lg font-semibold mb-2">Tout va bien!</h2>
        <p class="text-tg-hint">Aucune alerte active pour le moment</p>
      </div>

      <!-- Alarm configuration section -->
      <div class="mt-6 border-t border-tg-secondary-bg pt-6">
        <h2 class="text-lg font-semibold mb-3 flex items-center gap-2">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
          </svg>
          Configuration des seuils
        </h2>
        
        <div v-if="store.devices.length === 0" class="text-center py-6 text-tg-hint">
          Aucun capteur disponible
        </div>
        
        <div v-else class="space-y-2">
          <div 
            v-for="device in store.devices.slice(0, 5)" 
            :key="device.devEui"
            class="tg-card flex items-center justify-between cursor-pointer hover:bg-tg-secondary-bg transition-colors"
            @click="openThresholdConfig(device)">
            <div class="flex items-center gap-3">
              <div 
                class="w-3 h-3 rounded-full"
                :class="device.isOnline ? 'bg-kk-primary' : 'bg-kk-danger'">
              </div>
              <div>
                <div class="font-medium text-sm">{{ device.description || device.name }}</div>
                <div class="text-xs text-tg-hint">{{ device.model }}</div>
              </div>
            </div>
            <svg class="w-5 h-5 text-tg-hint" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
            </svg>
          </div>
          
          <div v-if="store.devices.length > 5" class="text-center pt-2">
            <router-link to="/devices" class="text-tg-link text-sm">
              Voir tous les capteurs ({{ store.devices.length }})
            </router-link>
          </div>
        </div>
      </div>

      <!-- Alert history -->
      <div v-if="recentAlerts.length > 0" class="mt-6 border-t border-tg-secondary-bg pt-6">
        <h2 class="text-lg font-semibold mb-3">Historique récent</h2>
        <div class="space-y-2">
          <div 
            v-for="alert in recentAlerts" 
            :key="alert.id"
            class="tg-card opacity-60">
            <div class="flex items-center justify-between">
              <div>
                <div class="font-medium text-sm">{{ alert.deviceName }}</div>
                <div class="text-xs text-tg-hint">{{ alert.message }}</div>
              </div>
              <div class="text-xs text-tg-hint">{{ formatDate(alert.triggeredAt) }}</div>
            </div>
          </div>
        </div>
      </div>
    </template>

    <!-- Threshold configuration modal -->
    <div 
      v-if="showThresholdModal" 
      class="fixed inset-0 bg-black/50 flex items-end z-50"
      @click.self="closeThresholdModal">
      <div class="bg-tg-bg w-full rounded-t-2xl p-4 max-h-[80vh] overflow-y-auto">
        <div class="flex items-center justify-between mb-4">
          <h3 class="text-lg font-bold">{{ selectedDevice?.description || selectedDevice?.name }}</h3>
          <button @click="closeThresholdModal" class="p-2">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <div v-if="loadingThresholds" class="text-center py-8">
          <div class="w-6 h-6 border-2 border-tg-button border-t-transparent rounded-full animate-spin mx-auto"></div>
        </div>

        <div v-else class="space-y-4">
          <div v-for="threshold in deviceThresholds" :key="threshold.sensorType" class="tg-card">
            <div class="flex items-center justify-between mb-3">
              <span class="font-medium">{{ getSensorLabel(threshold.sensorType) }}</span>
              <label class="relative inline-flex items-center cursor-pointer">
                <input type="checkbox" v-model="threshold.isEnabled" class="sr-only peer">
                <div class="w-11 h-6 bg-gray-200 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-kk-primary"></div>
              </label>
            </div>
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="text-xs text-tg-hint">Min</label>
                <input 
                  type="number" 
                  v-model.number="threshold.minValue"
                  class="w-full bg-tg-secondary-bg rounded-lg px-3 py-2 text-sm"
                  :disabled="!threshold.isEnabled"
                  step="0.1">
              </div>
              <div>
                <label class="text-xs text-tg-hint">Max</label>
                <input 
                  type="number" 
                  v-model.number="threshold.maxValue"
                  class="w-full bg-tg-secondary-bg rounded-lg px-3 py-2 text-sm"
                  :disabled="!threshold.isEnabled"
                  step="0.1">
              </div>
            </div>
          </div>

          <button 
            class="w-full bg-tg-button text-tg-button-text py-3 rounded-xl font-medium"
            :disabled="savingThresholds"
            @click="saveThresholds">
            <span v-if="savingThresholds">Enregistrement...</span>
            <span v-else>Enregistrer les seuils</span>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAppStore } from '../stores/app'
import { formatTimeAgo } from '../utils/time'

const store = useAppStore()
const router = useRouter()
const formatRelativeTime = (date) => formatTimeAgo(date)

const refreshing = ref(false)
const showThresholdModal = ref(false)
const selectedDevice = ref(null)
const deviceThresholds = ref([])
const loadingThresholds = ref(false)
const savingThresholds = ref(false)

const recentAlerts = computed(() => {
  return store.alerts
    .filter(a => !a.isActive)
    .sort((a, b) => new Date(b.triggeredAt) - new Date(a.triggeredAt))
    .slice(0, 10)
})

async function refreshAlerts() {
  refreshing.value = true
  await store.fetchAlerts()
  refreshing.value = false
}

function getAlertBorderColor(type) {
  return type === 'threshold' ? 'border-kk-danger' : 'border-kk-warning'
}

function viewDevice(devEui) {
  router.push(`/device/${devEui}`)
}

async function muteAllAlerts() {
  await store.muteAllAlerts()
}

async function activateAllAlerts() {
  await store.activateAllAlerts()
}

async function toggleAlert(alertId) {
  await store.toggleAlertStatus(alertId)
}

async function openThresholdConfig(device) {
  store.hapticFeedback('light')
  selectedDevice.value = device
  showThresholdModal.value = true
  loadingThresholds.value = true
  
  try {
    const thresholds = await store.fetchDeviceThresholds(device.devEui)
    
    // If no thresholds, create defaults based on device type
    if (thresholds.length === 0) {
      deviceThresholds.value = getDefaultThresholds(device.model)
    } else {
      deviceThresholds.value = thresholds
    }
  } catch (e) {
    deviceThresholds.value = getDefaultThresholds(device.model)
  } finally {
    loadingThresholds.value = false
  }
}

function getDefaultThresholds(model) {
  const defaults = [
    { sensorType: 'temperature', minValue: 5, maxValue: 35, isEnabled: true },
    { sensorType: 'battery', minValue: 20, maxValue: null, isEnabled: true }
  ]
  
  if (model?.includes('WET') || model?.includes('UC502')) {
    defaults.push(
      { sensorType: 'vwc', minValue: 10, maxValue: 60, isEnabled: true },
      { sensorType: 'ec', minValue: null, maxValue: 5, isEnabled: false }
    )
  }
  
  if (model?.includes('EM300') || model?.includes('TH')) {
    defaults.push(
      { sensorType: 'humidity', minValue: 30, maxValue: 80, isEnabled: true }
    )
  }
  
  return defaults
}

function getSensorLabel(sensorType) {
  const labels = {
    temperature: 'Température (°C)',
    humidity: 'Humidité (%)',
    vwc: 'VWC (%)',
    ec: 'EC (mS/cm)',
    battery: 'Batterie (%)'
  }
  return labels[sensorType] || sensorType
}

function closeThresholdModal() {
  showThresholdModal.value = false
  selectedDevice.value = null
  deviceThresholds.value = []
}

async function saveThresholds() {
  if (!selectedDevice.value) return
  
  savingThresholds.value = true
  store.hapticFeedback('medium')
  
  const success = await store.updateDeviceThresholds(
    selectedDevice.value.devEui,
    deviceThresholds.value
  )
  
  savingThresholds.value = false
  
  if (success) {
    closeThresholdModal()
  }
}

function formatDate(date) {
  if (!date) return ''
  return new Date(date).toLocaleDateString('fr-FR', {
    day: '2-digit',
    month: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}
</script>
