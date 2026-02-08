// Service Telegram para buscar logs
export const telegramService = {
  async fetchLogs() {
    try {
      // Importar authService para pegar usuário logado
      const { authService } = await import('./api.js')
      const currentUser = authService.getCurrentUser()
      
      if (!currentUser) {
        console.error('Aucun utilisateur connecté')
        return []
      }

      // Criar log do usuário atual conectado
      const logs = [{
        timestamp: new Date().toISOString(),
        level: 'INFO',
        source: 'STAFF_SESSION',
        message: `Utilisateur connecté: ${currentUser.name || currentUser.email}`,
        details: `Email: ${currentUser.email} | Role: ${currentUser.role} | ID: ${currentUser.id}`,
        stackTrace: null
      }]

      return logs
    } catch (error) {
      console.error('Erreur lors de la récupération des données utilisateur:', error)
      return []
    }
  }
}
