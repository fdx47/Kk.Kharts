import axios from 'axios'

const API_URL = import.meta.env.VITE_API_URL || 'https://kropkontrol.premiumasp.net/api/v1'

const api = axios.create({
  baseURL: API_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json'
  }
})

// Add JWT token if available
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('kk_token')
  if (token) {
    config.headers['Authorization'] = `Bearer ${token}`
  }
  return config
})

// Handle errors and Refresh Token
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config

    // Handle 401 - Try to refresh token
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true

      const refreshToken = localStorage.getItem('kk_refresh_token')

      if (refreshToken) {
        try {
          // Call refresh endpoint using a clean axios instance to avoid interceptor loops
          const response = await axios.post(`${API_URL}/auth/refresh-token`, {
            RefreshToken: refreshToken
          })

          if (response.data.isSuccess && response.data.token) {
            // Update storage
            localStorage.setItem('kk_token', response.data.token)
            localStorage.setItem('kk_refresh_token', response.data.refreshToken)
            localStorage.setItem('kk_refresh_expiry', response.data.refreshTokenExpiryTime)

            // Update header and retry original request
            originalRequest.headers['Authorization'] = `Bearer ${response.data.token}`
            return api(originalRequest)
          }
        } catch (refreshError) {
          console.error('[API] Refresh token failed:', refreshError)
        }
      }

      // If we get here, refresh failed or no token exists
      localStorage.removeItem('kk_token')
      localStorage.removeItem('kk_refresh_token')
      localStorage.removeItem('kk_refresh_expiry')

      // Only redirect if not already on login
      if (window.location.pathname !== '/login') {
        window.location.href = '/login'
      }
      return Promise.reject(error)
    }

    const errorDetail = {
      message: error.message,
      status: error.response?.status,
      data: error.response?.data,
      url: error.config?.url,
      method: error.config?.method
    }
    console.error('[API Error Detail]:', JSON.stringify(errorDetail, null, 2))
    return Promise.reject(error)
  }
)

export default api
