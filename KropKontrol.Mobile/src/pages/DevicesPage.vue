<template>
  <q-page class="kk-page">
    <q-pull-to-refresh @refresh="onRefresh">
      <!-- Header -->
      <div class="kk-page__header">
        <h1 class="page-title">{{ $t('devices.title') }}</h1>
        <p class="page-subtitle">{{ devicesStore.totalDevices }} {{ $t('devices.title').toLowerCase() }}</p>
      </div>

      <!-- Search -->
      <div class="search-container mb-4">
        <q-input
          v-model="searchQuery"
          :placeholder="$t('devices.searchPlaceholder')"
          dense
          rounded
          standout
          class="kk-input"
        >
          <template #prepend>
            <q-icon name="mdi-magnify" />
          </template>
          <template v-if="searchQuery" #append>
            <q-icon name="mdi-close" class="cursor-pointer" @click="searchQuery = ''" />
          </template>
        </q-input>
      </div>

      <!-- Filter Chips -->
      <div class="filter-chips mb-4">
        <q-chip
          v-for="filter in filters"
          :key="filter.value"
          :selected="activeFilter === filter.value"
          clickable
          :color="getFilterColor(filter.value, activeFilter === filter.value)"
          text-color="white"
          @click="setFilter(filter.value)"
        >
          {{ filter.label }}
          <q-badge 
            v-if="filter.count > 0" 
            :label="filter.count" 
            floating 
            :color="filter.value === 'offline' ? 'negative' : 'grey-7'" 
          />
        </q-chip>
      </div>

      <!-- Devices List -->
      <template v-if="devicesStore.loading && devicesStore.devices.length === 0">
        <div v-for="i in 4" :key="i" class="kk-skeleton kk-skeleton--card mb-3"></div>
      </template>

      <template v-else-if="filteredDevices.length === 0">
        <div class="kk-card text-center py-8">
          <q-icon name="mdi-access-point-off" size="48px" class="text-grey-6 mb-4" />
          <p class="text-grey-6">{{ $t('devices.noDevices') }}</p>
        </div>
      </template>

      <template v-else>
        <SensorCard
          v-for="device in filteredDevices"
          :key="device.devEui"
          :device="device"
          @click="goToDevice(device.devEui)"
        />
      </template>
    </q-pull-to-refresh>
  </q-page>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useDevicesStore } from '@/stores/devices'
import { useSettingsStore } from '@/stores/settings'
import SensorCard from '@/components/SensorCard.vue'

const router = useRouter()
const route = useRoute()
const devicesStore = useDevicesStore()
const settingsStore = useSettingsStore()

const searchQuery = ref('')
const activeFilter = ref<string>((route.query.filter as string) || 'all')

const filters = computed(() => [
  { value: 'all', label: 'Tous', count: 0 },
  { value: 'online', label: 'En ligne', count: devicesStore.onlineDevices.length },
  { value: 'offline', label: 'Hors ligne', count: devicesStore.offlineDevices.length }
])

const filteredDevices = computed(() => {
  let devices = devicesStore.devices

  if (activeFilter.value === 'online') {
    devices = devices.filter(d => d.isOnline)
  } else if (activeFilter.value === 'offline') {
    devices = devices.filter(d => !d.isOnline)
  }

  if (searchQuery.value.trim()) {
    const query = searchQuery.value.toLowerCase()
    devices = devices.filter(d =>
      d.name.toLowerCase().includes(query) ||
      d.devEui.toLowerCase().includes(query) ||
      d.installationLocation?.toLowerCase().includes(query) ||
      d.description?.toLowerCase().includes(query)
    )
  }

  return devices
})

onMounted(async () => {
  await devicesStore.fetchDevices()
})

async function onRefresh(done: () => void) {
  settingsStore.hapticFeedback('light')
  await devicesStore.fetchDevices(true)
  done()
}

function setFilter(filter: string) {
  settingsStore.hapticFeedback('light')
  activeFilter.value = filter
}

function goToDevice(devEui: string) {
  settingsStore.hapticFeedback('light')
  router.push({ name: 'device-detail', params: { devEui } })
}

function getFilterColor(filterValue: string, isActive: boolean): string {
  if (!isActive) return 'grey-8'
  if (filterValue === 'online') return 'positive'
  if (filterValue === 'offline') return 'negative'
  return 'grey-7'
}
</script>

<style lang="scss" scoped>
.search-container {
  margin-bottom: 16px;
}

.filter-chips {
  display: flex;
  gap: 8px;
  overflow-x: auto;
  padding-bottom: 8px;
  margin-bottom: 16px;

  &::-webkit-scrollbar {
    display: none;
  }
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
