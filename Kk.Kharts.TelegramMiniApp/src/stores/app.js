import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api from '../services/api'
import { useToast } from '../services/toastService'

export const useAppStore = defineStore('app', () => {
  const toast = useToast()
  // State
  const user = ref(null)
  const devices = ref([])
  const alerts = ref([])
  const stats = ref(null)
  const loading = ref(false)
  const error = ref(null)
  const isLinked = ref(false)
  const token = ref(localStorage.getItem('kk_token') || null)
  const isInitialized = ref(false)
  let initPromise = null

  // Computed
  const isAuthenticated = computed(() => !!token.value && !!user.value)
  const activeAlerts = computed(() => alerts.value.filter(a => a.isActive))
  const onlineDevices = computed(() => devices.value.filter(d => d.isOnline))
  const offlineDevices = computed(() => devices.value.filter(d => !d.isOnline))

  // Actions
  async function initApp() {
    if (initPromise) return initPromise

    initPromise = (async () => {
      loading.value = true
      error.value = null

      try {
        // Sync token from storage in case it changed (e.g. login)
        const storedToken = localStorage.getItem('kk_token')
        if (storedToken && storedToken !== token.value) {
          token.value = storedToken
        }

        // Check if we have a valid token
        if (token.value) {
          // We can fetch initial data
          try {
            await Promise.all([
              fetchDevices(),
              // fetchAlerts(), 
              // fetchStats()
            ])
            user.value = { email: 'User' } // Placeholder
            isLinked.value = true
          } catch (err) {
            if (err.response?.status === 401) {
              user.value = null
              token.value = null
              isLinked.value = false
            }
          }
        }
      } catch (e) {
        console.error('Init error:', e)
      } finally {
        loading.value = false
        isInitialized.value = true
      }
    })()

    return initPromise
  }

  async function fetchDevices() {
    try {
      const response = await api.get('/devices')
      devices.value = (response.data || []).filter(d => d.devEui !== '0000000000000000')
    } catch (e) {
      console.error('Failed to fetch devices:', e)
      toast.error('Erreur lors du chargement des capteurs')
      devices.value = []
    }
  }

  async function fetchAlerts() {
    alerts.value = []
  }

  async function fetchStats() {
    stats.value = null
  }

  async function fetchDeviceData(devEui, startDate, endDate) {
    try {
      const end = endDate || new Date()
      const start = startDate || new Date(end.getTime() - 24 * 60 * 60 * 1000)

      const response = await api.get(`/em300/${devEui}/th`, {
        params: {
          startDate: start.toISOString(),
          endDate: end.toISOString()
        }
      })
      return response.data || []
    } catch (e) {
      console.error('Failed to fetch device data:', e)
      return []
    }
  }

  async function fetchDeviceThresholds(devEui) {
    // Placeholder - endpoint needs to be identified or created in Standard API
    return []
  }

  async function updateDeviceThresholds(devEui, thresholds) {
    // Placeholder
    return false
  }

  async function sendSupportMessage(message) {
    // Placeholder - Support endpoint might need update
    return false
  }

  async function muteAllAlerts() {
    return false
  }

  async function activateAllAlerts() {
    return false
  }

  async function toggleAlertStatus(alertId) {
    return false
  }

  // Helpers (formerly Telegram, now standard)
  function hapticFeedback(type = 'light') {
    // No-op for now
  }

  function showToast(message, type = 'success') {
    if (type === 'error') toast.error(message)
    else toast.success(message)
  }

  function showConfirm(message) {
    return window.confirm(message)
  }

  function openLink(url) {
    window.open(url, '_blank')
  }

  function close() {
    // No-op
  }

  async function fetchDashboardState(devEui) {
    return null
  }

  return {
    // State
    user,
    devices,
    alerts,
    stats,
    loading,
    error,
    isLinked,
    token,
    isInitialized,
    // Computed
    isAuthenticated,
    activeAlerts,
    onlineDevices,
    offlineDevices,
    // Actions
    initApp,
    fetchDevices,
    fetchAlerts,
    fetchStats,
    fetchDeviceData,
    fetchDeviceThresholds,
    updateDeviceThresholds,
    fetchDashboardState,
    sendSupportMessage,
    muteAllAlerts,
    activateAllAlerts,
    toggleAlertStatus,
    // Helpers
    hapticFeedback,
    showToast,
    showConfirm,
    openLink,
    close
  }
})
