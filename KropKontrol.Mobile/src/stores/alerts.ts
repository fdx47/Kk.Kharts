import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { api } from '@/services/api'

export interface Alert {
  id: number
  devEui: string
  deviceName: string
  type: string
  message: string
  isActive: boolean
  triggeredAt: string
  acknowledgedAt?: string
  severity: 'critical' | 'warning' | 'info'
}

export const useAlertsStore = defineStore('alerts', () => {
  const alerts = ref<Alert[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  const activeAlerts = computed(() => alerts.value.filter(a => a.isActive))
  const criticalAlerts = computed(() => alerts.value.filter(a => a.severity === 'critical' && a.isActive))
  const alertCount = computed(() => activeAlerts.value.length)

  async function fetchAlerts(): Promise<void> {
    loading.value = true
    error.value = null

    try {
      // Usa o endpoint da Mini App, para utilizadores não administradores
      const response = await api.get<any[]>('/miniapp/alerts')

      if (Array.isArray(response.data)) {
        // Filtra apenas alertas ativos e normaliza para o formato interno do store
        alerts.value = response.data
          .filter((a: any) => a.isAlarmActive || a.enabled)
          .map((a: any) => ({
            id: a.id || 0,
            devEui: a.devEui || '',
            deviceName: a.deviceName || a.propertyName || 'Capteur',
            type: a.activeThresholdType || 'threshold',
            message: a.propertyName ? `${a.propertyName} hors limites` : 'Alerte active',
            isActive: a.isAlarmActive || false,
            triggeredAt: a.triggeredAt || new Date().toISOString(),
            severity: 'warning' as const
          }))
      } else {
        alerts.value = []
      }
    } catch (e: any) {
      // Silencia 403 (perfil sem permissão) e 401 para evitar spam de erros
      if (e.response?.status !== 403 && e.response?.status !== 401) {
        console.error('Failed to fetch alerts:', e.message)
      }
      alerts.value = []
    } finally {
      loading.value = false
    }
  }

  async function acknowledgeAlert(alertId: number): Promise<boolean> {
    // TODO: API ainda não exposta - manter true para não bloquear UI
    return true
  }

  async function muteAlert(alertId: number): Promise<boolean> {
    // TODO: ligar ao endpoint quando existir
    return true
  }

  async function muteAllAlerts(): Promise<boolean> {
    // TODO: Idem acima: placeholder para futura API
    return true
  }

  function clearAlerts(): void {
    alerts.value = []
  }

  return {
    alerts,
    loading,
    error,
    activeAlerts,
    criticalAlerts,
    alertCount,
    fetchAlerts,
    acknowledgeAlert,
    muteAlert,
    muteAllAlerts,
    clearAlerts
  }
})
