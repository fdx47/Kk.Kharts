<template>
  <div class="tg-card text-center">
    <div class="text-tg-hint text-sm mb-1 flex items-center justify-center gap-1">
      <span>{{ label }}</span>
      <span v-if="unit" class="text-xs opacity-70">({{ unit }})</span>
    </div>
    <div class="sensor-value" :class="{ 'opacity-50': !hasValue }">
      {{ displayValue }}
    </div>
    <div v-if="unit" class="sensor-unit">{{ unit }}</div>
    <div v-if="showStatus && hasValue" class="text-xs text-tg-hint mt-1">
      {{ statusText }}
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  label: {
    type: String,
    required: true
  },
  value: {
    type: [Number, String],
    default: null
  },
  unit: {
    type: String,
    default: ''
  },
  decimals: {
    type: Number,
    default: 1
  },
  showStatus: {
    type: Boolean,
    default: false
  },
  minValue: {
    type: Number,
    default: null
  },
  maxValue: {
    type: Number,
    default: null
  },
  warningMin: {
    type: Number,
    default: null
  },
  warningMax: {
    type: Number,
    default: null
  }
})

const hasValue = computed(() => props.value !== null && props.value !== undefined && props.value !== '')

const displayValue = computed(() => {
  if (!hasValue.value) return '--'
  
  if (typeof props.value === 'number') {
    return props.value.toFixed(props.decimals)
  }
  return props.value
})

const statusText = computed(() => {
  if (!hasValue.value) return ''
  
  const numValue = Number(props.value)
  
  if (props.warningMin !== null && numValue < props.warningMin) {
    return '⚠️ Baixo'
  }
  if (props.warningMax !== null && numValue > props.warningMax) {
    return '⚠️ Alto'
  }
  
  return '✓ Normal'
})
</script>

<style scoped>
.sensor-value {
  font-size: 1.875rem;
  font-weight: bold;
  color: var(--tg-text-color);
  transition: opacity 0.2s ease;
}

.sensor-unit {
  font-size: 0.875rem;
  color: var(--tg-hint-color);
  margin-top: 0.25rem;
}
</style>
