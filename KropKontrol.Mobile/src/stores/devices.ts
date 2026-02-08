import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { api } from '@/services/api'
import { get, set } from 'idb-keyval'

export interface Device {
  devEui: string
  name: string
  description?: string
  installationLocation?: string
  model: string
  isOnline: boolean
  lastSeenAt: string
  battery: number
  company?: string
  variables: string[]
  lastTemperature?: number
  lastHumidity?: number
  lastVwc?: number
  lastEc?: number
}

export interface DashboardState {
  temperature?: number
  humidity?: number
  vwc?: number
  ec?: number
  timestamp: string
}

const CACHE_KEY = 'kk_devices_cache'
const CACHE_TTL = 5 * 60 * 1000 // 5 minutos

// Retorna variáveis disponíveis com base no modelo do dispositivo
function getVariablesForModel(model: number | undefined): string[] {
  const m = Number(model) || 7
  if (m === 7) return ['temperature', 'humidity'] // Modelo EM300-TH
  if (m === 2) return ['temperature', 'humidity', 'water'] // Modelo EM300-DI
  if (m === 47 || m === 62) return ['temperature', 'vwc', 'ec'] // Modelo UC502-WET150
  if (m === 61) return ['ModbusChannel1'] // Modelo UC502-MODBUS
  return ['temperature', 'humidity']
}

export const useDevicesStore = defineStore('devices', () => {
  const devices = ref<Device[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const lastFetch = ref<number>(0)

  const onlineDevices = computed(() => devices.value.filter(d => d.isOnline))
  const offlineDevices = computed(() => devices.value.filter(d => !d.isOnline))
  const totalDevices = computed(() => devices.value.length)

  async function fetchDevices(forceRefresh = false): Promise<void> {
    if (!forceRefresh && Date.now() - lastFetch.value < CACHE_TTL && devices.value.length > 0) {
      return
    }

    loading.value = true
    error.value = null

    try {
      // Tentar carregar do cache primeiro para evitar chamadas desnecessárias à API
      if (!forceRefresh) {
        const cached = await get<{ data: Device[]; timestamp: number }>(CACHE_KEY)
        if (cached && Date.now() - cached.timestamp < CACHE_TTL) {
          // Filtrar dispositivos virtuais do cache também
          devices.value = (cached.data || []).filter((d: any) => d.devEui !== '0000000000000000')
          lastFetch.value = cached.timestamp
          loading.value = false
          return
        }
      }

      // Usar endpoint da Mini App para obter dados completos de sensores
      const response = await api.get<any[]>('/miniapp/devices')
      const rawDevices = (response.data || [])

      devices.value = rawDevices.map((d: any) => {
        return {
          devEui: d.devEui || '',
          name: d.name || d.description || 'Capteur',
          description: d.description || '',
          installationLocation: d.installationLocation || d.description || '',
          model: String(d.model || 7),
          isOnline: !!d.isOnline,
          lastSeenAt: d.lastSeenAt || '',
          battery: d.battery || 0,
          company: d.company || '',
          variables: d.variables || getVariablesForModel(d.model),
          lastTemperature: d.lastTemperature,
          lastHumidity: d.lastHumidity,
          lastVwc: d.lastVwc,
          lastEc: d.lastEc
        }
      }).filter(d => d.devEui && d.devEui !== '0000000000000000')

      lastFetch.value = Date.now()

      // Guardar os dados em cache para diminuir latência em navegação subsequente
      const cacheData = JSON.parse(JSON.stringify(devices.value))
      await set(CACHE_KEY, { data: cacheData, timestamp: lastFetch.value })
    } catch (e: any) {
      console.error('Erro ao buscar dispositivos:', e)
      error.value = 'Erreur lors du chargement des capteurs'

      // Tentar carregar do cache em caso de erro para manter alguma informação na interface
      const cached = await get<{ data: Device[]; timestamp: number }>(CACHE_KEY)
      if (cached) {
        devices.value = (cached.data || []).filter((d: any) => d.devEui !== '0000000000000000')
      }
    } finally {
      loading.value = false
    }
  }

  async function fetchDeviceData(devEui: string, period = '48h'): Promise<any[]> {
    try {
      const startDate = getStartDate(period)
      const endDate = new Date().toISOString()

      // Usar endpoint unificado da Mini App que suporta todos os modelos
      const endpoint = `/miniapp/devices/${encodeURIComponent(devEui)}/data`

      const response = await api.get<any[]>(endpoint, {
        params: { period }
      })

      const data = Array.isArray(response.data) ? response.data : []
      return data
    } catch (e: any) {
      console.error('Failed to fetch device data:', e.message, e.response?.data)
      return []
    }
  }

  async function fetchDashboardState(devEui: string): Promise<DashboardState | null> {
    // Endpoint de estado do dashboard pode não existir na API padrão, retornar null por enquanto
    return null
  }

  async function fetchDeviceThresholds(devEui: string): Promise<any[]> {
    try {
      const response = await api.get<any[]>(`/miniapp/devices/${encodeURIComponent(devEui)}/thresholds`)
      return Array.isArray(response.data) ? response.data : []
    } catch (e: any) {
      if (e.response?.status !== 404) {
        console.error('Failed to fetch thresholds:', e.message)
      }
      return []
    }
  }

  async function updateDeviceThresholds(devEui: string, thresholds: any[]): Promise<boolean> {
    try {
      // Converter para o formato esperado pelo MiniAppController
      const payload = {
        thresholds: thresholds.map(t => ({
          sensorType: t.variable || t.propertyName,
          minValue: t.minValue ?? t.lowValue,
          maxValue: t.maxValue ?? t.highValue,
          isEnabled: t.enabled !== undefined ? t.enabled : true,
          useTimePeriods: t.useTimePeriods || false,
          periods: t.periods || []
        }))
      }

      await api.put(`/miniapp/devices/${encodeURIComponent(devEui)}/thresholds`, payload)
      return true
    } catch (e) {
      console.error('Failed to update thresholds:', e)
      return false
    }
  }

  function getStartDate(period: string): string {
    const now = new Date()
    switch (period) {
      case '24h': now.setHours(now.getHours() - 24); break
      case '36h': now.setHours(now.getHours() - 36); break
      case '48h': now.setHours(now.getHours() - 48); break
      case '72h': now.setHours(now.getHours() - 72); break
      case '5d': now.setDate(now.getDate() - 5); break
      case '7d': now.setDate(now.getDate() - 7); break
      case '30d': now.setDate(now.getDate() - 30); break
      default: now.setHours(now.getHours() - 36); break
    }
    return now.toISOString()
  }

  function getDeviceByDevEui(devEui: string): Device | undefined {
    return devices.value.find(d => d.devEui === devEui)
  }

  function clearCache(): void {
    devices.value = []
    lastFetch.value = 0
  }

  return {
    devices,
    loading,
    error,
    onlineDevices,
    offlineDevices,
    totalDevices,
    fetchDevices,
    fetchDeviceData,
    fetchDashboardState,
    fetchDeviceThresholds,
    updateDeviceThresholds,
    getDeviceByDevEui,
    clearCache
  }
})
