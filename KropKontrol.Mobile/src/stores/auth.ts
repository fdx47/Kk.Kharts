import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { api } from '@/services/api'
import { saveTokens, clearTokens, getAccessToken, getRefreshToken, getRefreshExpiry } from '@/utils/auth-storage'

export interface User {
  id: number
  email: string
  name: string
  company?: string
}

export interface AuthState {
  user: User | null
  token: string | null
}

export const useAuthStore = defineStore('auth', () => {
  // Inicializa o estado com os dados guardados no localStorage
  const storedToken = getAccessToken()
  console.log('🔐 Auth Store - Token from storage:', storedToken ? 'EXISTS' : 'NULL')
  
  const token = ref(storedToken)
  const storedUser = localStorage.getItem('kk_user')
  const user = ref<User | null>(storedUser ? JSON.parse(storedUser) : null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  const isAuthenticated = computed(() => !!token.value)

  // Trata da renovação transparente de tokens usando o refresh token persistido
  async function refreshTokens(): Promise<boolean> {
    const refreshToken = getRefreshToken()
    const expiry = getRefreshExpiry()

    if (!refreshToken) {
      logout()
      return false
    }

    if (expiry && new Date(expiry) <= new Date()) {
      logout()
      return false
    }

    try {
      const response = await api.post<any>('/auth/refresh-token', { refreshToken })
      const data = response.data
      const newAccess = data.token || data.Token
      const newRefresh = data.refreshToken || data.RefreshToken || refreshToken
      const newExpiry = data.refreshTokenExpiryTime || data.RefreshTokenExpiryTime || expiry

      if (!newAccess) {
        logout()
        return false
      }

      token.value = newAccess
      saveTokens(newAccess, newRefresh, newExpiry)

      // Recupera o utilizador do cache local caso o estado tenha sido perdido
      if (!user.value) {
        const cached = localStorage.getItem('kk_user')
        if (cached) {
          user.value = JSON.parse(cached)
        }
      }

      return true
    } catch (e) {
      console.error('Refresh token error:', e)
      logout()
      return false
    }
  }

  async function initAuth() {
    loading.value = true
    error.value = null

    try {
      console.log('🔐 InitAuth - A iniciar o fluxo de autenticação')
      console.log('🔐 InitAuth - Current token:', token.value ? 'EXISTS' : 'NULL')
      if (token.value) {
        const isValid = await validateToken()
        console.log('🔐 InitAuth - Token validation result:', isValid)
      } else {
        console.log('🔐 InitAuth - Sem token, utilizador precisa iniciar sessão')
      }
    } catch (e) {
      console.error('Auth init error:', e)
      error.value = "Erreur d'authentification"
    } finally {
      loading.value = false
    }
  }

  async function loginWithEmail(email: string, password: string): Promise<boolean> {
    loading.value = true
    error.value = null

    try {
      const response = await api.post<any>('/auth/login', { email, password })
      const data = response.data

      if (data.isSuccess || data.IsSuccess) {
        const authToken = data.token || data.Token
        const refreshToken = data.refreshToken || data.RefreshToken
        const refreshExpiry = data.refreshTokenExpiryTime || data.RefreshTokenExpiryTime
        const userData = data.userAccount || data.UserAccount

        console.log('🔐 Login - Token recebido:', authToken ? 'SIM' : 'NÃO')
        console.log('🔐 Login - A guardar token no localStorage')

        token.value = authToken
        const normalizedUser: User = userData ? {
          id: userData.id || userData.Id || 0,
          email: userData.email || userData.Email || email,
          name: userData.name || userData.fullName || `${userData.firstName || userData.FirstName || userData.nom || userData.Nom || ''} ${userData.lastName || userData.LastName || ''}`.trim() || userData.email || userData.Email || email.split('@')[0],
          company: userData.companyName || userData.CompanyName
        } : {
          id: 0,
          email,
          name: email.split('@')[0]
        }
        user.value = normalizedUser
        localStorage.setItem('kk_user', JSON.stringify(normalizedUser))

        saveTokens(authToken, refreshToken, refreshExpiry)
        
        console.log(' Login - Token saved, current value:', getAccessToken())
        return true
      }

      error.value = data.message || data.Message || 'Identifiants incorrects'
      return false
    } catch (e: any) {
      console.error('Login error:', e)
      error.value = e.response?.data?.message || e.response?.data?.Message || 'Erreur de connexion'
      return false
    } finally {
      loading.value = false
    }
  }

  async function validateToken(): Promise<boolean> {
    if (!token.value) {
      console.log('🔐 ValidateToken - No token to validate')
      return false
    }

    try {
      console.log('🔐 ValidateToken - Existe token, a ignorar validação (endpoint 404)')
      // TODO: Corrigir endpoint /miniapp/me - temporariamente saltamos a validação para evitar logout indevido
      // const response = await api.get<any>('/miniapp/me')
      
      // Regressa true temporariamente se existir token
      console.log('🔐 ValidateToken - Validação ignorada (token presente)')
      // Garante que o utilizador é reconstruído a partir do armazenamento, se necessário
      if (!user.value) {
        const cached = localStorage.getItem('kk_user')
        if (cached) {
          user.value = JSON.parse(cached)
        }
      }
      return true
      
      /*
      const response = await api.get<any>('/miniapp/me')
      
      console.log('🔐 ValidateToken - Status da resposta:', response.status)
      console.log('🔐 ValidateToken - Dados devolvidos:', response.data)

      if (response.data) {
        user.value = {
          id: response.data.id || response.data.userId || 0,
          email: response.data.email || '',
          name: response.data.name || response.data.firstName || 'Utilisateur',
          company: response.data.company
        }
        console.log('🔐 ValidateToken - Validação bem sucedida')
        return true
      }

      console.log('🔐 ValidateToken - Sem dados na resposta, a terminar sessão')
      logout()
      return false
      */
    } catch (e: any) {
      console.log('🔐 ValidateToken - Validation failed:', e.message)
      console.log('🔐 ValidateToken - Response status:', e.response?.status)
      console.log('🔐 ValidateToken - Response data:', e.response?.data)
      
      // Trata silenciosamente falhas de validação para não gerar loops de logout
      // Só regista no console se não for um 401/403/404 (erros esperados de autenticação)
      if (e.response?.status !== 401 && e.response?.status !== 403 && e.response?.status !== 404) {
        console.warn('Token validation error:', e.message)
      }
      
      console.log('🔐 ValidateToken - Calling logout due to validation failure')
      logout()
      return false
    }
  }

  function logout() {
    console.log('🚪 Logout - Function called, clearing auth state')
    // Limpa primeiro o estado reativo para forçar a atualização da UI
    user.value = null
    token.value = null
    clearTokens()
    localStorage.removeItem('kk_user')
    console.log('🚪 Logout - Auth state cleared')
  }

  return {
    user,
    token,
    loading,
    error,
    isAuthenticated,
    initAuth,
    loginWithEmail,
    validateToken,
    refreshTokens,
    logout
  }
})
