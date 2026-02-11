import axios from 'axios'
import { jwtDecode } from 'jwt-decode'

// Override console methods to suppress all output
if (false && typeof window !== 'undefined') {
  const originalConsole = window.console
  window.console = {
    ...originalConsole,
    log: () => {},
    error: () => {},
    warn: () => {},
    info: () => {},
    debug: () => {},
    trace: () => {},
    table: () => {},
    group: () => {},
    groupEnd: () => {},
    groupCollapsed: () => {},
    clear: () => {},
    count: () => {},
    countReset: () => {},
    assert: () => {},
    dir: () => {},
    dirxml: () => {},
    profile: () => {},
    profileEnd: () => {},
    time: () => {},
    timeEnd: () => {},
    timeLog: () => {},
    timeStamp: () => {},
    context: () => {},
    memory: () => {},
  }
}

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || ''

// Instance Axios configurée
const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json'
  },
  // Supprimer les logs par défaut
  validateStatus: () => true
})

const ensureOk = (response) => {
  if (response && response.status >= 200 && response.status < 300) return response
  const message = response?.data?.message || response?.data?.error || `HTTP ${response?.status || 'error'}`
  throw new Error(message)
}

// Intercepteur pour ajouter le token JWT
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('authToken')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
      // Debug: verificar se token está a ser enviado
      if (config.url?.includes('/api/v1/users') || config.url?.includes('/api/v1/devices')) {
        console.log('Token sent:', token.substring(0, 30) + '...')
      }
    } else {
      console.log('No token in localStorage')
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// Intercepteur pour supprimer les logs de requêtes
apiClient.interceptors.request.use(
  (config) => {
    // Supprimer les logs de requête
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// Intercepteur pour gérer les erreurs d'authentification et supprimer les logs de réponse
apiClient.interceptors.response.use(
  (response) => {
    // Supprimer les logs de réponse
    return response
  },
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('authToken')
      localStorage.removeItem('refreshToken')
      window.location.href = '/staff/login'
    }
    return Promise.reject(error)
  }
)

// Service d'authentification
export const authService = {
  async login(email, password) {
    const normalizedEmail = (email || '').trim()
    const response = await apiClient.post('/api/v1/auth/login', {
      login: normalizedEmail,
      email: normalizedEmail,
      password
    })
    const { token, refreshToken } = response.data
    localStorage.setItem('authToken', token)
    localStorage.setItem('refreshToken', refreshToken)
    return response.data
  },

  logout() {
    localStorage.removeItem('authToken')
    localStorage.removeItem('refreshToken')
  },

  isAuthenticated() {
    const token = localStorage.getItem('authToken')
    if (!token) return false

    try {
      const decoded = jwtDecode(token)
      return decoded.exp * 1000 > Date.now()
    } catch {
      return false
    }
  },

  getUserRole() {
    const token = localStorage.getItem('authToken')
    if (!token) return null

    try {
      const decoded = jwtDecode(token)
      return decoded.role || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
    } catch {
      return null
    }
  },

  getCurrentUser() {
    const token = localStorage.getItem('authToken')
    if (!token) return null

    try {
      const decoded = jwtDecode(token)
      return {
        id: decoded.sub || decoded.nameid,
        email: decoded.email || decoded.upn,
        name: decoded.name || decoded.given_name,
        role: decoded.role || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'],
        exp: decoded.exp
      }
    } catch {
      return null
    }
  }
}

// Service VPN Profiles
export const vpnProfileService = {
  async getAll() {
    const response = await apiClient.get('/api/v1/vpn-profiles')
    return response.data
  },

  async getById(id) {
    const response = await apiClient.get(`/api/v1/vpn-profiles/${id}`)
    return response.data
  },

  async create(data) {
    const response = await apiClient.post('/api/v1/vpn-profiles', data)
    return response.data
  },

  async update(id, data) {
    const response = await apiClient.put(`/api/v1/vpn-profiles/${id}`, data)
    return response.data
  },

  async delete(id) {
    await apiClient.delete(`/api/v1/vpn-profiles/${id}`)
  },

  async assign(id, payload) {
    const response = await apiClient.post(`/api/v1/vpn-profiles/${id}/assign`, payload)
    return response.data
  },

  async unassign(id) {
    const response = await apiClient.post(`/api/v1/vpn-profiles/${id}/unassign`)
    return response.data
  },

  async uploadOvpnFile(id, file) {
    const formData = new FormData()
    formData.append('file', file)
    const response = await apiClient.post(`/api/v1/vpn-profiles/${id}/upload-ovpn`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })
    return response.data
  },

  async downloadOvpnFile(id) {
    const response = await apiClient.get(`/api/v1/vpn-profiles/${id}/download-ovpn`, {
      responseType: 'blob'
    })
    return response.data
  },

  async importFromCsv(file) {
    const formData = new FormData()
    formData.append('file', file)
    const response = await apiClient.post('/api/v1/vpn-profiles/import-csv', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })
    return response.data
  }
}

// Service Users (para a lista de utilizadores)
export const userService = {
  async getAll() {
    const response = await apiClient.get('/api/v1/users')
    return ensureOk(response).data
  }
}

// Service Devices (para dados reais dos dispositivos)
export const deviceService = {
  async getAll() {
    const response = await apiClient.get('/api/v1/devices')
    return ensureOk(response).data
  },

  async getModels() {
    const response = await apiClient.get('/api/v1/devices/models')
    return ensureOk(response).data
  },

  async getStats() {
    try {
      const devices = await this.getAll()
      const total = devices.length
      
      // Dispositivos online: activeInKropKontrol = true e lastSendAt recente (últimas 15 minutos)
      const online = devices.filter(d => {
        const isActive = d.activeInKropKontrol === true
        // Considerar online se lastSendAt for "À l'instant", "Il y a X secondes/minutes" (menos de 15 minutos)
        const isRecent = d.lastSendAt && (
          d.lastSendAt === "À l'instant" ||
          d.lastSendAt.includes("seconde") ||
          (d.lastSendAt.includes("minute") && !d.lastSendAt.includes("minutes")) ||
          (d.lastSendAt.includes("minute") && parseInt(d.lastSendAt.match(/\d+/)?.[0] || 0) <= 15)
        )
        return isActive && isRecent
      }).length
      
      const offline = total - online
      
      return {
        total,
        online,
        offline,
        onlinePercentage: total > 0 ? Math.round((online / total) * 100) : 0
      }
    } catch (error) {
      console.error('Erro ao buscar estatísticas de dispositivos:', error)
      return {
        total: 0,
        online: 0,
        offline: 0,
        onlinePercentage: 0
      }
    }
  }
}

// Service Users Stats
export const userStatsService = {
  async getStats() {
    try {
      const users = await userService.getAll()
      const total = users.length
      
      // Retorna apenas dados reais do backend sem distinção ativo/inativo
      return {
        total: users.length
      }
    } catch (error) {
      console.error('Erro ao buscar estatísticas de usuários:', error)
      return {
        total: 0,
        active: 0,
        inactive: 0,
        activePercentage: 0
      }
    }
  }
}

// Service Companies
export const companyService = {
  async getAll() {
    const response = await apiClient.get('/api/v1/companies')
    return ensureOk(response).data
  }
}

// Service Activities
export const activityService = {
  async getRecent(count = 10) {
    const response = await apiClient.get(`/api/v1/activities?count=${count}`)
    return ensureOk(response).data
  }
}

// Service System Status
export const systemStatusService = {
  async getStatus() {
    const response = await apiClient.get('/api/v1/system/status')
    return ensureOk(response).data
  }
}

// Service Logs para buscar logs do endpoint personalizado
export const logsService = {
  async fetchLogs(date = null, limit = 500) {
    try {
      const params = new URLSearchParams()
      if (date) params.append('date', date)
      if (limit && limit !== 500) params.append('limit', limit.toString())

      const response = await apiClient.get(`/api/v1/logs?${params.toString()}`)
      return ensureOk(response).data
    } catch (error) {
      throw error
    }
  },

  async fetchLogFile(date = null) {
    try {
      const params = new URLSearchParams()
      if (date) params.append('date', date)

      const response = await apiClient.get(`/api/v1/logs/download?${params.toString()}`, {
        responseType: 'text'
      })

      ensureOk(response)
      return response.data
    } catch (error) {
      throw error
    }
  },

  async downloadLog(date = null) {
    try {
      const params = new URLSearchParams()
      if (date) params.append('date', date)

      const response = await apiClient.get(`/api/v1/logs/download?${params.toString()}`, {
        responseType: 'blob'
      })

      ensureOk(response)

      // Criar URL para download
      const url = window.URL.createObjectURL(new Blob([response.data]))
      const link = document.createElement('a')
      link.href = url

      // Determinar nome do ficheiro
      const disposition = response.headers['content-disposition']
      let filename = 'log.txt'
      if (disposition && disposition.includes('filename=')) {
        filename = disposition.split('filename=')[1].replace(/"/g, '')
      } else if (date) {
        filename = `${date.replace(/-/g, '')}.txt`
      }

      link.setAttribute('download', filename)
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
      window.URL.revokeObjectURL(url)

      return { success: true, filename }
    } catch (error) {
      throw error
    }
  },

  // Método para buscar logs de uma data específica
  async fetchLogsByDate(date, limit) {
    return this.fetchLogs(date, limit)
  },

  // Método para buscar logs das últimas 24h
  async fetchRecentLogs(limit) {
    const yesterday = new Date()
    yesterday.setDate(yesterday.getDate() - 1)
    const dateStr = yesterday.toISOString().split('T')[0]
    return this.fetchLogs(dateStr, limit)
  },

  async fetchStatistics() {
    try {
      const response = await apiClient.get('/api/v1/logs/statistics')
      return response.data
    } catch (error) {
      console.error('Erreur lors de la récupération des statistiques:', error)
      throw error
    }
  },

  async fetchDailyStatistics(date = null) {
    try {
      const url = date ? `/api/v1/logs/statistics/daily?date=${date}` : '/api/v1/logs/statistics/daily'
      const response = await apiClient.get(url)
      return response.data
    } catch (error) {
      throw error
    }
  },

  async streamLogs(date = null, page = 1, pageSize = 500) {
    try {
      const params = new URLSearchParams()
      if (date) params.append('date', date)
      params.append('page', page.toString())
      params.append('pageSize', pageSize.toString())

      const response = await apiClient.get(`/api/v1/logs/stream?${params.toString()}`)
      return response.data
    } catch (error) {
      throw error
    }
  },

  async getDashboardStats(date = null) {
    try {
      const url = date ? `/api/v1/dashboard/stats?date=${date}` : '/api/v1/dashboard/stats'
      const response = await apiClient.get(url)
      return response.data
    } catch (error) {
      throw error
    }
  },

  async getDevicesStats(date = null) {
    try {
      const url = date ? `/api/v1/dashboard/devices?date=${date}` : '/api/v1/dashboard/devices'
      const response = await apiClient.get(url)
      return response.data
    } catch (error) {
      throw error
    }
  },

  async getUsersStats(date = null) {
    try {
      const url = date ? `/api/v1/dashboard/users?date=${date}` : '/api/v1/dashboard/users'
      const response = await apiClient.get(url)
      return response.data
    } catch (error) {
      throw error
    }
  },

  async fetchAnalytics(date = null) {
    try {
      const url = date ? `/api/v1/logs/analytics?date=${date}` : '/api/v1/logs/analytics'
      const response = await apiClient.get(url)
      return ensureOk(response).data
    } catch (error) {
      throw error
    }
  },

  async fetchAvailableDates() {
    try {
      const response = await apiClient.get('/api/v1/logs/available-dates')
      return ensureOk(response).data
    } catch (error) {
      throw error
    }
  }
}

export default apiClient
