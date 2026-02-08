<template>
  <div
    class="chart-wrapper"
    :class="{ 'chart-wrapper--fullscreen': fullscreen }"
    :style="props.height ? { height: props.height } : undefined"
  >
    <!-- Aviso de modo paisagem (apenas em tela cheia) -->
    <div v-if="fullscreen && showLandscapePrompt" class="landscape-prompt" @click="dismissLandscapePrompt">
      <div class="prompt-content">
        <q-icon name="mdi-phone-rotate-landscape" size="48px" class="text-primary q-mb-md animate-rotate" />
        <p class="text-h6 text-weight-bold q-mb-xs">Tournez votre appareil</p>
        <p class="text-grey-6 text-caption">Pour une meilleure visualisation</p>
        <q-btn flat dense color="primary" label="Rester en portrait" class="q-mt-md" @click.stop="dismissLandscapePrompt" />
      </div>
    </div>

    <Line
      v-if="chartData"
      :data="chartData"
      :options="chartOptions"
    />
  </div>
</template>

<script setup lang="ts">
import { computed, ref, onMounted, watch } from 'vue'
import { Line } from 'vue-chartjs'
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  Filler
} from 'chart.js'

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  Filler
)

const props = defineProps<{
  data: any[]
  variables: string[]
  fullscreen?: boolean
  mini?: boolean
  height?: string
  showAxes?: boolean
  showTooltip?: boolean
}>()

const showLandscapePrompt = ref(false)

onMounted(() => {
  if (props.fullscreen && window.matchMedia("(orientation: portrait)").matches) {
    showLandscapePrompt.value = true
  }
})

watch(() => props.fullscreen, (val) => {
  if (val && window.matchMedia("(orientation: portrait)").matches) {
    showLandscapePrompt.value = true
  } else {
    showLandscapePrompt.value = false
  }
})

function dismissLandscapePrompt() {
  showLandscapePrompt.value = false
}

const variableColors: Record<string, string> = {
  temperature: '#6366f1',     // Índigo (temperatura)
  humidity: '#0ea5e9',        // Cyan (humidade ar)
  vwc: '#10b981',             // Esmeralda (VWC)
  ec: '#f59e0b',              // Âmbar (EC)
  soilTemperature: '#8b5cf6', // Violeta
  soilHumidity: '#0ea5e9',    // Cyan
  mineralVWC: '#10b981',      // Esmeralda
  mineralECp: '#f59e0b'       // Âmbar
}

const variableLabels: Record<string, string> = {
  temperature: 'Température (°C)',
  humidity: 'Humidité (%)',
  vwc: 'VWC (%)',
  ec: 'EC (µS/cm)',
  soilTemperature: 'Temp. Sol (°C)',
  soilHumidity: 'Hum. Sol (%)',
  mineralVWC: 'VWC Sol (%)',
  mineralECp: 'EC Sol (µS/cm)'
}

const chartData = computed(() => {
  if (!props.data || props.data.length === 0) return null

  const labels = props.data.map(d => {
    const rawDate = d.time || d.timestamp
    const date = new Date(rawDate)
    if (Number.isNaN(date.getTime())) return ''
    return date.toLocaleString('fr-FR', {
      day: props.mini ? undefined : '2-digit',
      month: props.mini ? undefined : '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    })
  })

  const datasets = props.variables
    .filter(v => props.data.some(d => d[v] != null))
    .map((variable, idx) => ({
      label: variableLabels[variable] || variable,
      data: props.data.map(d => d[variable]),
      borderColor: variableColors[variable] || '#888',
      backgroundColor: 'transparent',
      fill: false,
      tension: 0.2,
      pointRadius: 0,
      pointHoverRadius: props.mini ? 0 : 3,
      borderWidth: props.mini ? 1.5 : 1.5,
      yAxisID: props.mini ? 'y' : `y_${idx}`
    }))

  return { labels, datasets }
})

// Construir configuração dinâmica do eixo Y para múltiplos eixos (um por variável)
const buildYAxes = computed(() => {
  if (props.mini) {
    return {
      y: {
        display: !!props.showAxes,
        grid: {
          display: !!props.showAxes,
          color: 'rgba(0, 0, 0, 0.05)'
        },
        ticks: {
          display: !!props.showAxes,
          color: '#94a3b8',
          font: { size: 9 }
        }
      }
    }
  }

  const axes: Record<string, any> = {}
  const activeVars = props.variables.filter(v => props.data.some(d => d[v] != null))

  activeVars.forEach((variable, idx) => {
    axes[`y_${idx}`] = {
      type: 'linear' as const,
      display: idx < 2 || props.fullscreen, // Mostrar apenas os 2 primeiros eixos, a menos que esteja em tela cheia
      position: idx % 2 === 0 ? 'left' : 'right',
      grid: {
        display: idx === 0,
        color: 'rgba(0, 0, 0, 0.05)'
      },
      ticks: {
        color: variableColors[variable] || '#888',
        font: { size: 9 },
        callback: (value: number) => value.toFixed(1)
      },
      title: {
        display: props.fullscreen,
        text: variableLabels[variable] || variable,
        color: variableColors[variable] || '#888',
        font: { size: 9, weight: 'bold' as const }
      }
    }
  })

  return axes
})

const chartOptions = computed(() => ({
  responsive: true,
  maintainAspectRatio: false,
  aspectRatio: props.fullscreen ? undefined : (props.mini ? 2.5 : 1.5),
  layout: props.mini && props.showAxes ? { padding: { bottom: 28, left: 0, right: 0, top: 0 } } : {},
  interaction: {
    mode: 'index' as const,
    intersect: false
  },
  plugins: {
    legend: {
      display: !props.mini,
      position: 'top' as const,
      labels: {
        color: '#666666',
        usePointStyle: true,
        pointStyle: 'line',
        boxWidth: 20,
        padding: 10,
        font: {
          size: 10
        }
      }
    },
    tooltip: {
      enabled: props.showTooltip ?? !props.mini,
      backgroundColor: 'rgba(255, 255, 255, 0.95)',
      titleColor: '#1e293b',
      bodyColor: '#475569',
      borderColor: 'rgba(0, 0, 0, 0.1)',
      borderWidth: 1,
      padding: 10,
      cornerRadius: 8,
      displayColors: true
    }
  },
  scales: {
    x: {
      type: 'category' as const,
      display: !props.mini || !!props.showAxes,
      grid: {
        display: !!props.showAxes,
        color: 'rgba(0, 0, 0, 0.05)'
      },
      ticks: {
        display: !props.mini || !!props.showAxes,
        color: '#94a3b8',
        maxRotation: 0,
        autoSkip: props.mini && props.showAxes ? false : true,
        maxTicksLimit: props.mini && props.showAxes ? 5 : (props.fullscreen ? 10 : 6),
        padding: props.mini && props.showAxes ? 8 : 0,
        align: 'center' as const,
        font: {
          size: 9
        }
      },
      offset: !!props.showAxes
    },
    ...buildYAxes.value
  }
}))
</script>

<style lang="scss" scoped>
.chart-wrapper {
  width: 100%;
  min-height: 100px;
  height: 100%;
  position: relative;

  &--fullscreen {
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 60px 16px 16px;
  }

  canvas {
    max-width: 100% !important;
    max-height: 100% !important;
  }
}

.landscape-prompt {
  position: absolute;
  inset: 0;
  background: rgba(255, 255, 255, 0.95);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 10;
  backdrop-filter: blur(4px);
}

.prompt-content {
  text-align: center;
  padding: 24px;
}

.animate-rotate {
  animation: rotate-hint 2s ease-in-out infinite;
}

@keyframes rotate-hint {
  0%, 100% { transform: rotate(0deg); }
  25% { transform: rotate(-15deg); }
  75% { transform: rotate(90deg); }
}
</style>
