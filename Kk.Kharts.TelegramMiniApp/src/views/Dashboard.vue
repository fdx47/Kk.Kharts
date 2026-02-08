<template>
  <div class="pb-24 bg-gray-50 min-h-screen">
    <!-- Header with Gradient -->
    <div class="bg-gradient-to-r from-kk-primary to-green-600 text-white pt-8 pb-12 px-6 rounded-b-[2rem] shadow-lg relative overflow-hidden">
        <!-- Background decorative circles -->
        <div class="absolute top-0 right-0 w-32 h-32 bg-white/10 rounded-full -mr-10 -mt-10 blur-2xl"></div>
        <div class="absolute bottom-0 left-0 w-24 h-24 bg-black/10 rounded-full -ml-8 -mb-8 blur-xl"></div>

        <div class="relative z-10 flex justify-between items-center mb-6">
            <div>
              <p class="text-green-100 text-sm font-medium mb-1">Bienvenue,</p>
              <h1 class="text-2xl font-bold">{{ userName }}</h1>
            </div>
            <div class="w-10 h-10 bg-white/20 backdrop-blur-md rounded-full flex items-center justify-center border border-white/30">
               <span class="text-lg font-bold">{{ userInitials }}</span>
            </div>
        </div>

        <!-- Main Stats Card (Floating) -->
        <div class="grid grid-cols-2 gap-4">
            <div class="bg-white/20 backdrop-blur-md rounded-2xl p-4 border border-white/30 text-center">
                <div class="text-3xl font-bold mb-1">{{ store.stats?.onlineDevices || store.onlineDevices.length }}</div>
                <div class="text-xs text-green-100 font-medium tracking-wide uppercase">Capteurs en Ligne</div>
            </div>
             <div class="bg-white/20 backdrop-blur-md rounded-2xl p-4 border border-white/30 text-center">
                <div class="text-3xl font-bold mb-1" :class="activeAlertsCount > 0 ? 'text-red-200' : ''">{{ activeAlertsCount }}</div>
                <div class="text-xs text-green-100 font-medium tracking-wide uppercase">Alertes Actives</div>
            </div>
        </div>
    </div>

    <div class="px-5 -mt-6 relative z-20">
      <!-- Loading State -->
      <div v-if="store.loading && !refreshing" class="bg-white rounded-3xl shadow-xl p-8 text-center animate-pulse">
         <div class="w-12 h-12 bg-gray-200 rounded-full mx-auto mb-4"></div>
         <div class="h-4 bg-gray-200 rounded w-1/2 mx-auto mb-2"></div>
         <div class="h-3 bg-gray-200 rounded w-1/3 mx-auto"></div>
      </div>

      <div v-else>
         <!-- Quick Status Cards -->
        <div class="bg-white rounded-3xl shadow-xl p-6 mb-6">
            <div class="grid grid-cols-2 gap-8">
                <div class="text-center border-r border-gray-100">
                     <div class="text-gray-400 text-xs font-semibold uppercase mb-2">Total Capteurs</div>
                     <div class="text-2xl font-bold text-gray-800">{{ store.devices.length }}</div>
                </div>
                <div class="text-center">
                     <div class="text-gray-400 text-xs font-semibold uppercase mb-2">Hors Ligne</div>
                     <div class="text-2xl font-bold" :class="offlineDevicesCount > 0 ? 'text-orange-500' : 'text-gray-800'">{{ offlineDevicesCount }}</div>
                </div>
            </div>
        </div>

        <!-- Recent Activity Section -->
        <div class="flex items-center justify-between mb-4 px-2">
            <h2 class="text-lg font-bold text-gray-800">Activité Récente</h2>
            <router-link to="/devices" class="text-kk-primary text-sm font-semibold hover:text-green-700">Voir tout</router-link>
        </div>

        <div v-if="recentDevices.length === 0" class="text-center py-10 bg-white rounded-3xl shadow-sm border border-gray-100">
             <div class="text-4xl mb-3">📡</div>
             <p class="text-gray-500 font-medium">Aucun capteur trouvé</p>
             <button @click="refreshData" class="mt-4 text-kk-primary text-sm font-bold">Actualiser</button>
        </div>

        <div v-else class="space-y-4 mb-8">
            <div v-for="device in recentDevices" :key="device.devEui" 
                 class="bg-white p-4 rounded-2xl shadow-sm border border-gray-100 flex items-center gap-4 hover:shadow-md transition-shadow cursor-pointer active:scale-[0.99] transform transition-transform"
                 @click="goToDevice(device.devEui)">
                
                <!-- Status Indicator -->
                <div class="w-12 h-12 rounded-2xl flex items-center justify-center shrink-0" 
                     :class="device.isOnline ? 'bg-green-100 text-green-600' : 'bg-gray-100 text-gray-400'">
                    <svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                         <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.824.185 2 2 0 00-.713 1.405v.293c0 .768.391 1.482 1.054 1.889l.34.204a6 6 0 005.772 0l.34-.204a2 2 0 012.108 0l.34.204a6 6 0 005.772 0l.34-.204a2 2 0 012.108 0l.34.204a6 6 0 005.772 0l.34-.204a2 2 0 011.054-1.889v-.293a2 2 0 00-.713-1.405z" />
                    </svg>
                </div>

                <div class="flex-1 min-w-0">
                    <h3 class="font-bold text-gray-800 truncate">{{ device.description || device.name }}</h3>
                    <p class="text-xs text-gray-500 truncate flex items-center gap-1">
                        <span>{{ device.model }}</span>
                        <span class="w-1 h-1 bg-gray-300 rounded-full"></span>
                        <span>{{ formatRelativeTime(device.lastSeenAt) }}</span>
                    </p>
                </div>

                <div class="text-right shrink-0">
                    <div class="flex flex-col items-end">
                         <span class="font-bold text-gray-800" v-if="device.lastTemperature">{{ device.lastTemperature.toFixed(1) }}°C</span>
                         <span class="text-xs" :class="getBatteryColorText(device.battery)">{{ device.battery || 0 }}%</span>
                    </div>
                </div>
            </div>
        </div>

        <!-- Quick Actions Grid -->
        <h2 class="text-lg font-bold text-gray-800 mb-4 px-2">Actions Rapides</h2>
        <div class="grid grid-cols-2 gap-4 mb-8">
            <button @click="refreshData" class="bg-blue-50 p-4 rounded-2xl flex flex-col items-center justify-center gap-2 hover:bg-blue-100 transition-colors group">
                 <div class="w-10 h-10 bg-blue-100 text-blue-600 rounded-full flex items-center justify-center group-hover:bg-white transition-colors">
                    <svg class="w-5 h-5" :class="{ 'animate-spin': refreshing }" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                    </svg>
                 </div>
                 <span class="text-sm font-semibold text-blue-700">Actualiser</span>
            </button>
             <router-link to="/support" class="bg-purple-50 p-4 rounded-2xl flex flex-col items-center justify-center gap-2 hover:bg-purple-100 transition-colors group">
                 <div class="w-10 h-10 bg-purple-100 text-purple-600 rounded-full flex items-center justify-center group-hover:bg-white transition-colors">
                    <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M18.364 5.636l-3.536 3.536m0 5.656l3.536 3.536M9.172 9.172L5.636 5.636m3.536 9.192l-3.536 3.536M21 12a9 9 0 11-18 0 9 9 0 0118 0zm-5 0a4 4 0 11-8 0 4 4 0 018 0z" />
                    </svg>
                 </div>
                 <span class="text-sm font-semibold text-purple-700">Support</span>
            </router-link>
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
import SkeletonLoader from '../components/SkeletonLoader.vue'

const store = useAppStore()
const router = useRouter()
const refreshing = ref(false)

const userName = computed(() => {
  return store.user?.email || 'Utilisateur'
})

const recentDevices = computed(() => {
  return [...store.devices]
    .sort((a, b) => new Date(b.lastSeenAt) - new Date(a.lastSeenAt))
    .slice(0, 5)
})

const lowBatteryDevices = computed(() => {
  return store.devices.filter(d => d.battery !== null && d.battery < 20)
})

function goToDevice(devEui) {
  router.push(`/device/${devEui}`)
}

async function refreshData() {
  refreshing.value = true
  
  await Promise.all([
    store.fetchDevices(),
    store.fetchAlerts(),
    store.fetchStats()
  ])
  
  refreshing.value = false
}

const formatRelativeTime = (date) => formatTimeAgo(date)
</script>
