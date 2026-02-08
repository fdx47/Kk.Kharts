<template>
  <div class="p-4">
    <h1 class="text-2xl font-bold mb-4">Mes Capteurs</h1>

    <!-- Filter tabs -->
    <div class="flex border-b border-tg-secondary-bg mb-4">
      <button 
        v-for="tab in tabs" 
        :key="tab.id"
        class="nav-tab"
        :class="{ active: activeTab === tab.id }"
        @click="setTab(tab.id)">
        {{ tab.label }} ({{ tab.count }})
      </button>
    </div>

    <!-- Device list -->
    <div class="space-y-3">
      <div 
        v-for="device in filteredDevices" 
        :key="device.devEui"
        class="tg-card"
        @click="goToDevice(device.devEui)">
        <div class="flex items-center justify-between">
          <div class="flex items-center gap-3">
            <div :class="device.isOnline ? 'status-online' : 'status-offline'"></div>
            <div>
              <div class="font-medium">{{ device.description || device.name }}</div>
              <div class="text-xs text-tg-hint flex items-center gap-1">
                <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                    d="M12 11c1.38 0 2.5-1.12 2.5-2.5S13.38 6 12 6s-2.5 1.12-2.5 2.5S10.62 11 12 11zm0 0c-2.21 0-4 1.79-4 4 0 2.67 4 7 4 7s4-4.33 4-7c0-2.21-1.79-4-4-4z" />
                </svg>
                <span>{{ device.installationLocation || 'Emplacement non défini' }}</span>
              </div>
            </div>
          </div>
          <div class="text-right">
            <div class="font-bold text-lg">{{ formatValue(device) }}</div>
            <div class="text-xs text-tg-hint">{{ formatTimeAgo(device.lastSeenAt) }}</div>
          </div>
        </div>
        
        <!-- Battery indicator -->
        <div class="mt-3 flex items-center gap-2">
          <div class="flex-1 h-2 bg-tg-secondary-bg rounded-full overflow-hidden">
            <div 
              class="h-full rounded-full transition-all"
              :class="getBatteryColor(device.batteryLevel)"
              :style="{ width: `${device.batteryLevel || 0}%` }">
            </div>
          </div>
          <span class="text-xs text-tg-hint">{{ device.batteryLevel || 0 }}%</span>
        </div>
      </div>
    </div>

    <!-- Empty state -->
    <div v-if="filteredDevices.length === 0" class="text-center py-12">
      <svg class="w-16 h-16 mx-auto text-tg-hint mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 3v2m6-2v2M9 19v2m6-2v2M5 9H3m2 6H3m18-6h-2m2 6h-2M7 19h10a2 2 0 002-2V7a2 2 0 00-2-2H7a2 2 0 00-2 2v10a2 2 0 002 2zM9 9h6v6H9V9z" />
      </svg>
      <p class="text-tg-hint">Aucun capteur trouvé</p>
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
const activeTab = ref('all')

const tabs = computed(() => [
  { id: 'all', label: 'Tous', count: store.devices.length },
  { id: 'online', label: 'En ligne', count: store.onlineDevices.length },
  { id: 'offline', label: 'Hors ligne', count: store.offlineDevices.length }
])

const filteredDevices = computed(() => {
  if (activeTab.value === 'online') return store.onlineDevices
  if (activeTab.value === 'offline') return store.offlineDevices
  return store.devices
})

function setTab(tab) {
  activeTab.value = tab
}

function goToDevice(devEui) {
  router.push(`/device/${devEui}`)
}

const formatRelativeTime = (date) => formatTimeAgo(date)

function formatValue(device) {
  if (device.deviceType === 'EM300-TH') {
    return `${device.lastTemperature?.toFixed(1) || '--'}°C`
  }
  if (device.deviceType === 'WET150') {
    return `${device.lastMoisture?.toFixed(0) || '--'}%`
  }
  return '--'
}

function getBatteryColor(level) {
  if (level > 50) return 'bg-kk-primary'
  if (level > 20) return 'bg-kk-warning'
  return 'bg-kk-danger'
}
</script>
