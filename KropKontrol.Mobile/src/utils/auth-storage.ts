export function saveTokens(token: string, refreshToken?: string, refreshExpiry?: string | Date | null): void {
  console.log('💾 SaveTokens - Saving token to localStorage')
  localStorage.setItem('kk_token', token)
  console.log('💾 SaveTokens - Token saved, verifying:', localStorage.getItem('kk_token'))

  if (refreshToken) {
    localStorage.setItem('kk_refresh_token', refreshToken)
  }

  if (refreshExpiry) {
    const value = typeof refreshExpiry === 'string' ? refreshExpiry : refreshExpiry.toISOString()
    localStorage.setItem('kk_refresh_expiry', value)
  }
}

export function clearTokens(): void {
  console.log('🗑️ ClearTokens - Clearing all tokens from localStorage')
  localStorage.removeItem('kk_token')
  localStorage.removeItem('kk_refresh_token')
  localStorage.removeItem('kk_refresh_expiry')
  localStorage.removeItem('kk_auth_mode')
}

export function getAccessToken(): string | null {
  const token = localStorage.getItem('kk_token')
  console.log('📖 GetAccessToken - Token from localStorage:', token ? 'EXISTS' : 'NULL')
  return token
}

export function getRefreshToken(): string | null {
  return localStorage.getItem('kk_refresh_token')
}

export function getRefreshExpiry(): string | null {
  return localStorage.getItem('kk_refresh_expiry')
}
