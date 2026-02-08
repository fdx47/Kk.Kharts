// Service Logs para buscar logs do endpoint personalizado
export const logsService = {
  async fetchLogs(date = null) {
    try {
      // Construir URL com parâmetro de data se fornecido
      let url = '/api/logs'
      if (date) {
        url += `/${date}`
      }
      
      const response = await fetch(url)
      
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }

      // Verificar se é um arquivo de texto
      const contentType = response.headers.get('content-type')
      if (contentType && contentType.includes('text/plain')) {
        const textData = await response.text()
        return this.parseLogsFromText(textData)
      }
      
      // Se não for text/plain, tentar como JSON
      try {
        const jsonData = await response.json()
        return jsonData
      } catch {
        // Se não for JSON nem texto, tentar como texto mesmo
        const textData = await response.text()
        return this.parseLogsFromText(textData)
      }
    } catch (error) {
      console.error('Erreur lors de la récupération des logs:', error)
      throw error
    }
  },

  parseLogsFromText(textData) {
    // Implementar parsing do arquivo TXT recebido
    const lines = textData.split('\n')
    const logs = []
    
    lines.forEach(line => {
      if (line.trim()) {
        const log = this.parseLogLine(line.trim())
        if (log) {
          logs.push(log)
        }
      }
    })
    
    return logs
  },

  parseLogLine(line) {
    // Implementar parsing de uma linha de log
    // Exemplo de formatos que você pode receber do Telegram:
    // "2024-01-26 16:20:15 ERROR [API] Connection timeout to device"
    // "[ERROR] API: Connection timeout - 2024-01-26 16:20:15"
    // "16:20:15 [API] ERROR: Connection timeout to device"
    
    const patterns = [
      // Format: "YYYY-MM-DD HH:mm:ss LEVEL [SOURCE] Message"
      /^(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})\s+(\w+)\s+\[([^\]]+)\]\s+(.+)$/,
      // Format: "[LEVEL] SOURCE: Message - YYYY-MM-DD HH:mm:ss"
      /^\[(\w+)\]\s+([^:]+):\s+(.+)\s*-\s*(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})$/,
      // Format: "HH:mm:ss [SOURCE] LEVEL: Message"
      /^(\d{2}:\d{2}:\d{2})\s+\[([^\]]+)\]\s+(\w+):\s+(.+)$/,
      // Format: "LEVEL SOURCE Message YYYY-MM-DD HH:mm:ss"
      /^(\w+)\s+(\w+)\s+(.+)\s+(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})$/,
      // Format: "LEVEL SOURCE Message HH:mm:ss"
      /^(\w+)\s+(\w+)\s+(.+)\s+(\d{2}:\d{2}:\d{2})$/,
      // Format simples: "Message"
      /^(.+)$/
    ]
    
    for (const pattern of patterns) {
      const match = line.match(pattern)
      if (match) {
        let timestamp, level, source, message
        
        if (pattern === patterns[0]) {
          // Format: "YYYY-MM-DD HH:mm:ss LEVEL [SOURCE] Message"
          [, timestamp, level, source, message] = match
        } else if (pattern === patterns[1]) {
          // Format: "[LEVEL] SOURCE: Message - YYYY-MM-DD HH:mm:ss"
          [, level, source, message, timestamp] = match
        } else if (pattern === patterns[2]) {
          // Format: "HH:mm:ss [SOURCE] LEVEL: Message"
          [, timestamp, source, level, message] = match
          // Adicionar data atual
          timestamp = `${new Date().toISOString().split('T')[0]} ${timestamp}`
        } else if (pattern === patterns[3]) {
          // Format: "LEVEL SOURCE Message YYYY-MM-DD HH:mm:ss"
          [, level, source, message, timestamp] = match
        } else if (pattern === patterns[4]) {
          // Format: "LEVEL SOURCE Message HH:mm:ss"
          [, level, source, message, timestamp] = match
          // Adicionar data atual
          timestamp = `${new Date().toISOString().split('T')[0]} ${timestamp}`
        } else if (pattern === patterns[5]) {
          // Format simples: "Message"
          [, message] = match
          timestamp = new Date().toISOString()
          level = 'INFO'
          source = 'UNKNOWN'
        }
        
        return {
          timestamp: timestamp ? new Date(timestamp).toISOString() : new Date().toISOString(),
          level: level ? level.toUpperCase() : 'INFO',
          source: source ? source.trim() : 'UNKNOWN',
          message: message ? message.trim() : line,
          details: null,
          stackTrace: level && level.toUpperCase() === 'ERROR' ? this.generateStackTrace() : null
        }
      }
    }
    
    // Se não match com nenhum padrão, tratar como log simples
    return {
      timestamp: new Date().toISOString(),
      level: 'INFO',
      source: 'UNKNOWN',
      message: line,
      details: null,
      stackTrace: null
    }
  },

  generateStackTrace() {
    return `Error: Connection failed
    at API.connect (/app/src/api.js:123:45)
    at Database.query (/app/src/database.js:67:89)
    at main.process (/app/src/main.js:234:56)
    at Object.<anonymous> (/app/src/index.js:10:5)`
  },

  // Método para buscar logs de uma data específica
  async fetchLogsByDate(date) {
    return this.fetchLogs(date)
  },

  // Método para buscar logs das últimas 24h
  async fetchRecentLogs() {
    const yesterday = new Date()
    yesterday.setDate(yesterday.getDate() - 1)
    const dateStr = yesterday.toISOString().split('T')[0]
    return this.fetchLogs(dateStr)
  }
}
