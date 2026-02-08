<template>
  <div class="kk-card sensor-card" @click="$emit('click')">
    <div class="sensor-header">
      <div class="sensor-info">
        <h4 class="sensor-name">{{ device.name }}</h4>
        <p class="sensor-location">{{ device.installationLocation || device.description }}</p>
      </div>
      <div class="sensor-status">
        <span class="kk-badge" :class="device.isOnline ? 'kk-badge--online' : 'kk-badge--offline'">
          <span class="badge-dot"></span>
          {{ device.isOnline ? 'Online' : 'Offline' }}
        </span>
      </div>
    </div>

    <div class="sensor-metrics">
      <div v-if="device.lastTemperature != null" class="metric-item">
        <q-icon name="mdi-thermometer" class="text-primary" />
        <span class="metric-value">{{ formatValue(device.lastTemperature, 1) }}°C</span>
      </div>
      
      <div v-if="device.lastHumidity != null" class="metric-item">
        <q-icon name="mdi-water-percent" class="text-secondary" />
        <span class="metric-value">{{ formatValue(device.lastHumidity, 0) }}%</span>
      </div>
      
      <div v-if="device.lastVwc != null" class="metric-item">
        <q-icon name="mdi-water" class="text-accent" />
        <span class="metric-value">{{ formatValue(device.lastVwc, 1) }}%</span>
      </div>
      
      <div v-if="device.lastEc != null" class="metric-item">
        <q-icon name="mdi-flash" class="text-warning" />
        <span class="metric-value">{{ formatValue(device.lastEc, 0) }} µS</span>
      </div>
    </div>

    <div class="sensor-footer">
      <div class="last-seen">
        <q-icon name="mdi-clock-outline" size="14px" />
        <span>{{ formatRelativeTime(device.lastSeenAt) }}</span>
      </div>
      <div v-if="device.battery" class="battery">
        <q-icon :name="batteryIcon" :class="batteryClass" size="16px" />
        <span>{{ device.battery }}%</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { Device } from '@/stores/devices'
import { formatTimeAgo } from '@/utils/time'

const props = defineProps<{
  device: Device
}>()

defineEmits<{
  click: []
}>()

function formatValue(value: number | undefined | null, decimals: number): string {
  if (value == null) return '--'
  return value.toFixed(decimals)
}

function formatRelativeTime(dateStr: string | undefined): string {
  return formatTimeAgo(dateStr)
}

const batteryIcon = computed(() => {
  const level = props.device.battery || 0
  if (level >= 90) return 'mdi-battery'
  if (level >= 70) return 'mdi-battery-80'
  if (level >= 50) return 'mdi-battery-60'
  if (level >= 30) return 'mdi-battery-40'
  if (level >= 10) return 'mdi-battery-20'
  return 'mdi-battery-alert'
})

const batteryClass = computed(() => {
  const level = props.device.battery || 0
  if (level >= 50) return 'text-positive'
  if (level >= 20) return 'text-warning'
  return 'text-negative'
})
</script>

<style lang="scss" scoped>
.sensor-card {
  margin-bottom: 12px;
  cursor: pointer;

  &:active {
    transform: scale(0.98);
  }
}

.sensor-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 12px;
}

.sensor-info {
  flex: 1;
  min-width: 0;
}

.sensor-name {
  font-size: 16px;
  font-weight: 700;
  margin: 0 0 4px 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.sensor-location {
  font-size: 12px;
  opacity: 0.6;
  margin: 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.sensor-metrics {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
  margin-bottom: 12px;
}

.metric-item {
  display: flex;
  align-items: center;
  gap: 6px;

  .metric-value {
    font-size: 14px;
    font-weight: 600;
  }
}

.sensor-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 12px;
  border-top: 1px solid rgba(255, 255, 255, 0.08);
}

.last-seen, .battery {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 12px;
  opacity: 0.6;
}
</style>
