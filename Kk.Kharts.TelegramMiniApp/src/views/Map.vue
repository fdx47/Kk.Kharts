<template>
  <div class="p-4">
    <h1 class="text-2xl font-bold mb-4">Carte des Capteurs</h1>

    <!-- Map container -->
    <div ref="mapContainer" class="map-container mb-4"></div>

    <!-- Device list below map -->
    <div class="space-y-2">
      <div 
        v-for="device in devicesWithLocation" 
        :key="device.devEui"
        class="tg-card flex items-center justify-between"
        @click="centerOnDevice(device)">
        <div class="flex items-center gap-3">
          <div :class="device.isOnline ? 'status-online' : 'status-offline'"></div>
          <div>
            <div class="font-medium text-sm">{{ device.description || device.name }}</div>
            <div class="text-xs text-tg-hint">{{ device.deviceType }}</div>
          </div>
        </div>
        <svg class="w-5 h-5 text-tg-hint" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
        </svg>
      </div>
    </div>

    <!-- No devices with location -->
    <div v-if="devicesWithLocation.length === 0" class="text-center py-8">
      <svg class="w-12 h-12 mx-auto text-tg-hint mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
      </svg>
      <p class="text-tg-hint">Aucun capteur avec localisation</p>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useAppStore } from '../stores/app'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'

const store = useAppStore()
const mapContainer = ref(null)
let map = null
const markers = []

const devicesWithLocation = computed(() => {
  return store.devices.filter(d => d.latitude && d.longitude)
})

onMounted(() => {
  initMap()
})

onUnmounted(() => {
  if (map) {
    map.remove()
  }
})

function initMap() {
  if (!mapContainer.value) return

  // Default center (Marmande / Agen area)
  const defaultCenter = [44.36, 0.37]
  const defaultZoom = 9

  map = L.map(mapContainer.value).setView(defaultCenter, defaultZoom)

  // Add tile layer (OpenStreetMap)
  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution: '© OpenStreetMap'
  }).addTo(map)

  // Add markers for devices
  addDeviceMarkers()

  // Fit bounds if we have devices
  if (devicesWithLocation.value.length > 0) {
    const bounds = L.latLngBounds(
      devicesWithLocation.value.map(d => [d.latitude, d.longitude])
    )
    map.fitBounds(bounds, { padding: [50, 50] })
  }
}

function addDeviceMarkers() {
  devicesWithLocation.value.forEach(device => {
    const icon = L.divIcon({
      className: 'custom-marker',
      html: `
        <div class="w-8 h-8 rounded-full flex items-center justify-center shadow-lg ${device.isOnline ? 'bg-green-500' : 'bg-red-500'}">
          <svg class="w-4 h-4 text-white" fill="currentColor" viewBox="0 0 20 20">
            <path d="M5.5 16a3.5 3.5 0 01-.369-6.98 4 4 0 117.753-1.977A4.5 4.5 0 1113.5 16h-8z" />
          </svg>
        </div>
      `,
      iconSize: [32, 32],
      iconAnchor: [16, 32]
    })

    const marker = L.marker([device.latitude, device.longitude], { icon })
      .addTo(map)
      .bindPopup(`
        <div class="text-center">
          <strong>${device.description || device.name}</strong><br>
          <span class="text-sm">${device.lastTemperature?.toFixed(1) || '--'}°C</span>
        </div>
      `)

    markers.push(marker)
  })
}

function centerOnDevice(device) {
  if (map && device.latitude && device.longitude) {
    map.setView([device.latitude, device.longitude], 15)
  }
}
</script>

<style>
.custom-marker {
  background: transparent;
  border: none;
}
</style>
