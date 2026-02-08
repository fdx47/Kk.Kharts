<template>
  <div class="logs-page">
    <!-- Navigation Header -->
    <NavigationHeader>
      <template #controls>
        <!-- Date Picker -->
        <div class="date-picker-wrapper">
          <div class="input-group">
            <span class="input-group-text">
              <i class="bi bi-calendar3"></i>
            </span>
            <input
              ref="dateInput"
              type="text"
              class="form-control form-control-sm date-input"
              :value="selectedDate ? formatDateForDisplay(selectedDate) : ''"
              placeholder="Choisir une date..."
              readonly
              @click="openDatePicker"
            />
          </div>
        </div>
        
        <!-- Actions -->
        <button class="btn btn-primary btn-sm" @click="openDatePicker">
          <i class="bi bi-calendar3 me-1"></i>
          Choisir Date
        </button>
        <button class="btn btn-success btn-sm" @click="fetchLogs" :disabled="loading">
          <i class="bi bi-search me-1"></i>
          Charger
        </button>
        <button class="btn btn-outline-danger btn-sm" @click="clearLogs">
          <i class="bi bi-trash me-1"></i>
          Vider
        </button>
      </template>
    </NavigationHeader>

    <!-- Main Content -->
    <main class="logs-main">
      <div class="container-fluid">
        <!-- File Info & Stats -->
        <div v-if="logMetadata && logMetadata.fileName" class="row mb-4">
          <div class="col-12">
            <div class="card file-info-card">
              <div class="card-body">
                <div class="row align-items-center">
                  <div class="col-md-8">
                    <div class="file-details">
                      <h5 class="mb-1">
                        <i class="bi bi-file-earmark-text me-2"></i>
                        {{ logMetadata.fileName }}
                      </h5>
                      <div class="file-meta">
                        <span class="badge bg-primary">{{ formatFileSize(logMetadata.fileSizeBytes) }}</span>
                        <span class="badge bg-info">{{ logMetadata.totalEntries }} entrées</span>
                        <span class="badge bg-secondary">{{ formatDate(logMetadata.lastModifiedUtc) }}</span>
                      </div>
                    </div>
                  </div>
                  <div class="col-md-4 text-end">
                    <small class="text-muted">
                      <i class="bi bi-download me-1"></i>
                      Téléchargement automatique
                    </small>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Statistics Cards -->
        <div v-if="logs.length > 0" class="row mb-4">
          <div class="col-md-3 col-sm-6 col-6 mb-3">
            <div class="stat-card stat-get">
              <div class="stat-icon">
                <i class="bi bi-arrow-down-circle"></i>
              </div>
              <div class="stat-content">
                <h3>{{ getMethodCount('GET') }}</h3>
                <span>GET</span>
              </div>
            </div>
          </div>
          <div class="col-md-3 col-sm-6 col-6 mb-3">
            <div class="stat-card stat-post">
              <div class="stat-icon">
                <i class="bi bi-plus-circle"></i>
              </div>
              <div class="stat-content">
                <h3>{{ getMethodCount('POST') }}</h3>
                <span>POST</span>
              </div>
            </div>
          </div>
          <div class="col-md-3 col-sm-6 col-6 mb-3">
            <div class="stat-card stat-login">
              <div class="stat-icon">
                <i class="bi bi-shield-check"></i>
              </div>
              <div class="stat-content">
                <h3>{{ getLoginCount() }}</h3>
                <span>Logins</span>
              </div>
            </div>
          </div>
          <div class="col-md-3 col-sm-6 col-6 mb-3">
            <div class="stat-card stat-users">
              <div class="stat-icon">
                <i class="bi bi-people"></i>
              </div>
              <div class="stat-content">
                <h3>{{ getUniqueUsers() }}</h3>
                <span>Utilisateurs</span>
              </div>
            </div>
          </div>
        </div>

        <!-- User Session Details -->
        <div v-if="uniqueUsersData.length > 0" class="row mb-4">
          <div class="col-12">
            <div class="card user-sessions-card">
              <div class="card-header">
                <h5 class="mb-0">
                  <i class="bi bi-person-check me-2"></i>
                  Sessions Utilisateurs
                </h5>
              </div>
              <div class="card-body">
                <div class="row">
                  <div v-for="(user, index) in uniqueUsersData.slice(0, 6)" :key="user.username" class="col-md-4 col-sm-6 mb-3">
                    <div class="user-session-item">
                      <div class="user-avatar">
                        <i class="bi bi-person-circle"></i>
                      </div>
                      <div class="user-info">
                        <h6 :title="user.fullName">{{ user.username }}</h6>
                        <small class="text-muted">
                          <i class="bi bi-clock me-1"></i>
                          {{ formatSessionDuration(user.sessionDuration) }}
                        </small>
                        <div class="user-stats">
                          <span class="badge" :class="getRoleBadgeClass(user.role)">{{ user.role }}</span>
                          <span class="badge bg-primary">{{ user.activityCount }} activités</span>
                          <span class="badge bg-secondary">{{ getPercentage(user.activityCount) }}%</span>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                <div v-if="uniqueUsersData.length > 6" class="text-center mt-3">
                  <small class="text-muted">+{{ uniqueUsersData.length - 6 }} autres utilisateurs</small>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Loading State -->
        <div v-if="loading" class="text-center py-5">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Chargement...</span>
          </div>
          <p class="mt-3 text-muted">Chargement des logs...</p>
        </div>

        <!-- Empty State -->
        <div v-else-if="logs.length === 0" class="text-center py-5">
          <div class="empty-state">
            <i class="bi bi-file-earmark-x" style="font-size: 4rem; color: #dee2e6;"></i>
            <h4 class="mt-3 text-muted">Aucun log trouvé</h4>
            <p class="text-muted">Sélectionnez une date pour afficher les logs</p>
          </div>
        </div>

        <!-- Logs List -->
        <div v-else class="logs-container">
          <!-- Load More Button -->
          <div v-if="logMetadata && logMetadata.hasNextPage" class="text-center mb-4">
            <button 
              class="btn btn-outline-primary load-more-btn" 
              @click="loadMoreLogs" 
              :disabled="loading"
            >
              <i class="bi bi-arrow-down-circle me-2"></i>
              Charger plus (Page {{ logMetadata.currentPage + 1 }}/{{ logMetadata.totalPages }})
            </button>
          </div>

          <!-- Log Entries -->
          <div class="log-entries">
            <div 
              v-for="(log, index) in paginatedLogs" 
              :key="index"
              class="log-entry"
              :class="getLogClass(log.level)"
            >
              <div class="log-header">
                <div class="log-time">
                  <i class="bi bi-clock me-1"></i>
                  {{ formatTime(log.timestamp) }}
                </div>
                <div class="log-badges">
                  <span class="badge" :class="getLevelBadgeClass(log.level)">
                    {{ log.level }}
                  </span>
                  <span class="badge bg-secondary">
                    {{ log.source }}
                  </span>
                </div>
                <button 
                  class="btn-expand"
                  @click="toggleLogDetails(index)"
                >
                  <i class="bi" :class="expandedLogs[index] ? 'bi-chevron-up' : 'bi-chevron-down'"></i>
                </button>
              </div>
              <div class="log-message">{{ log.message }}</div>
              <div v-if="expandedLogs[index]" class="log-details">
                <div v-if="log.details" class="log-details-content">
                  <pre>{{ log.details }}</pre>
                </div>
                <div v-if="log.stackTrace" class="log-stack-trace">
                  <strong>Stack Trace:</strong>
                  <pre>{{ log.stackTrace }}</pre>
                </div>
              </div>
            </div>
          </div>

          <!-- Pagination -->
          <nav v-if="totalPages > 1" class="mt-4">
            <ul class="pagination justify-content-center">
              <li class="page-item" :class="{ disabled: currentPage === 1 }">
                <a class="page-link" @click="currentPage--">
                  <i class="bi bi-chevron-left"></i>
                </a>
              </li>
              <li
                class="page-item"
                v-for="page in totalPages"
                :key="page"
                :class="{ active: currentPage === page }"
              >
                <a class="page-link" @click="currentPage = page">{{ page }}</a>
              </li>
              <li class="page-item" :class="{ disabled: currentPage === totalPages }">
                <a class="page-link" @click="currentPage++">
                  <i class="bi bi-chevron-right"></i>
                </a>
              </li>
            </ul>
          </nav>
        </div>
      </div>
    </main>
  </div>
</template>

<script>
import { ref, computed, onMounted, nextTick, watch } from 'vue'
import { useRouter } from 'vue-router'
import { logsService } from '@/services/api'
import { toast } from 'vue3-toastify'
import flatpickr from 'flatpickr'
import 'flatpickr/dist/flatpickr.min.css'
import { French } from 'flatpickr/dist/l10n/fr'
import NavigationHeader from '@/components/NavigationHeader.vue'

export default {
  name: 'LogsView',
  components: {
    NavigationHeader
  },
  setup() {
    const router = useRouter()
    const logs = ref([])
    const loading = ref(false)
    const searchQuery = ref('')
    const filterLevel = ref('')
    const filterSource = ref('')
    const sortBy = ref('time')
    const currentPage = ref(1)
    const logsPerPage = 50
    const expandedLogs = ref({})
    const selectedDate = ref('')
    const dateInput = ref(null)
    const maxDate = computed(() => new Date().toISOString().split('T')[0])
    let flatpickrInstance = null
    const showStatistics = ref(false)
    const statistics = ref([])
    const uniqueUsersData = ref([])
    const logMetadata = ref({
      date: '',
      fileName: '',
      fileSizeBytes: 0,
      lastModifiedUtc: new Date(),
      totalEntries: 0,
      returnedEntries: 0,
      currentPage: 1,
      pageSize: 500,
      totalPages: 0,
      hasNextPage: false,
      hasPreviousPage: false,
      hasMore: false
    })

    // Computed
    const filteredLogs = computed(() => {
      let result = logs.value

      // Filtrer par recherche
      if (searchQuery.value) {
        const query = searchQuery.value.toLowerCase()
        result = result.filter(log => 
          log.message.toLowerCase().includes(query) ||
          log.source.toLowerCase().includes(query) ||
          (log.details && log.details.toLowerCase().includes(query))
        )
      }

      // Filtrer par niveau
      if (filterLevel.value) {
        result = result.filter(log => log.level === filterLevel.value)
      }

      // Filtrer par source
      if (filterSource.value) {
        result = result.filter(log => log.source === filterSource.value)
      }

      // Trier
      result.sort((a, b) => {
        switch (sortBy.value) {
          case 'level':
            const levelOrder = { 'ERROR': 0, 'WARN': 1, 'INFO': 2, 'DEBUG': 3 }
            return levelOrder[a.level] - levelOrder[b.level]
          case 'source':
            return a.source.localeCompare(b.source)
          case 'time':
          default:
            return new Date(b.timestamp) - new Date(a.timestamp)
        }
      })

      return result
    })

    const totalPages = computed(() => {
      return Math.ceil(filteredLogs.value.length / logsPerPage)
    })

    const paginatedLogs = computed(() => {
      const start = (currentPage.value - 1) * logsPerPage
      const end = start + logsPerPage
      return filteredLogs.value.slice(start, end)
    })

    // Methods
    const downloadDirectLog = async () => {
      try {
        const result = await logsService.downloadLog(selectedDate.value || null)
        toast.success(`Fichier ${result.filename} téléchargé avec succès`)
      } catch (error) {
        toast.error('Erreur lors du téléchargement du log')
      }
    }

    const fetchLogs = async () => {
      loading.value = true
      try {
        // Use the original logs endpoint with higher limit for statistics
        const logsData = await logsService.fetchLogs(selectedDate.value || null, 2000)
        
        // If hasMore, load more pages
        let allEntries = [...logsData.entries]
        let currentLimit = 2000
        
        while (logsData.hasMore && currentLimit < 10000) {
          currentLimit += 2000
          const moreLogs = await logsService.fetchLogs(selectedDate.value || null, currentLimit)
          allEntries = [...moreLogs.entries]
          if (!moreLogs.hasMore) break
        }
        
        logs.value = allEntries
        logMetadata.value = {
          date: logsData.date,
          fileName: logsData.fileName,
          fileSizeBytes: logsData.fileSizeBytes,
          lastModifiedUtc: logsData.lastModifiedUtc,
          totalEntries: logsData.totalEntries,
          currentPage: 1,
          pageSize: allEntries.length,
          totalPages: 1,
          hasNextPage: false,
          hasPreviousPage: false,
          hasMore: false,
          returnedEntries: allEntries.length
        }
        
        if (selectedDate.value) {
          toast.success(`Logs récupérés pour le ${selectedDate.value} (${logsData.totalEntries} entrées)`)
        } else {
          toast.success('Logs récupérés avec succès')
        }
      } catch (error) {
        toast.error('Erreur lors de la récupération des logs')
      } finally {
        loading.value = false
      }
    }

    const loadMoreLogs = async () => {
      // Not needed since we load all logs at once
    }

    const clearLogs = () => {
      if (confirm('Êtes-vous sûr de vouloir vider tous les logs ?')) {
        logs.value = []
        expandedLogs.value = {}
        logMetadata.value = {
          date: '',
          fileName: '',
          fileSizeBytes: 0,
          lastModifiedUtc: new Date(),
          totalEntries: 0,
          returnedEntries: 0,
          currentPage: 1,
          pageSize: 500,
          totalPages: 0,
          hasNextPage: false,
          hasPreviousPage: false,
          hasMore: false
        }
        toast.info('Logs vidés')
      }
    }

    const getMethodCount = (method) => {
      return logs.value.filter(log => 
        log.message.toUpperCase().includes(method.toUpperCase())
      ).length
    }

    const getLoginCount = () => {
      return logs.value.filter(log => 
        log.message.toLowerCase().includes('login') ||
        log.message.toLowerCase().includes('connexion') ||
        log.message.toLowerCase().includes('auth')
      ).length
    }

    const getUniqueUsers = () => {
      const users = new Set()
      const userSessions = new Map()
      
      // System accounts to exclude
      const systemAccounts = new Set(['root', 'admin', 'system', 'demo', 'UserRW', 'technician', 'tech'])
      
      logs.value.forEach(log => {
        // Extract user from different patterns - enhanced for full names
        const userMatch = log.message.match(/(?:User:|user\s+|Utilisateur\s+connecté\s*:)\s*([A-Za-zÀ-ÿ][A-Za-zÀ-ÿ\s]+)/i) ||
                         log.message.match(/([A-Za-zÀ-ÿ][A-Za-zÀ-ÿ\s]+)\s+(?:logged\s+in|connexion|login)/i) ||
                         log.message.match(/(?:User:|user\s+)(\w+)/i)
        
        if (userMatch) {
          let fullName = userMatch[1].trim()
          
          // Skip system accounts
          if (systemAccounts.has(fullName.toLowerCase())) {
            return
          }
          
          // Clean up the name - remove extra spaces and capitalize properly
          fullName = fullName.replace(/\s+/g, ' ')
          const displayName = fullName.length > 20 ? fullName.substring(0, 20) + '...' : fullName
          
          users.add(displayName)
          
          // Track session time and classify role
          if (!userSessions.has(displayName)) {
            userSessions.set(displayName, {
              firstSeen: new Date(log.timestamp),
              lastSeen: new Date(log.timestamp),
              count: 1,
              role: classifyUserRole(displayName, log.message),
              fullName: fullName
            })
          } else {
            const session = userSessions.get(displayName)
            session.lastSeen = new Date(log.timestamp)
            session.count++
            // Update role if more specific info found
            const role = classifyUserRole(displayName, log.message)
            if (role !== 'unknown') {
              session.role = role
            }
          }
        }
      })
      
      // Store session data for display
      uniqueUsersData.value = Array.from(userSessions.entries()).map(([username, session]) => ({
        username,
        fullName: session.fullName,
        sessionDuration: session.lastSeen - session.firstSeen,
        firstSeen: session.firstSeen,
        lastSeen: session.lastSeen,
        activityCount: session.count,
        role: session.role
      })).sort((a, b) => b.activityCount - a.activityCount)
      
      return users.size
    }

    const classifyUserRole = (username, message) => {
      const msgLower = message.toLowerCase()
      
      // Check for root/admin patterns
      if (msgLower.includes('root') || msgLower.includes('admin') || msgLower.includes('administrator')) {
        return 'root'
      }
      
      // Check for technician patterns
      if (msgLower.includes('technician') || msgLower.includes('tech') || msgLower.includes('support')) {
        return 'technician'
      }
      
      // Check for demo patterns
      if (msgLower.includes('demo') || msgLower.includes('test') || username.toLowerCase().includes('demo')) {
        return 'demo'
      }
      
      // Check for UserRW patterns
      if (msgLower.includes('userrw') || msgLower.includes('read-write')) {
        return 'UserRW'
      }
      
      // Default to regular user
      return 'user'
    }

    const formatDateForDisplay = (dateStr) => {
      if (!dateStr) return ''
      const [year, month, day] = dateStr.split('-')
      return `${day}/${month}/${year}`
    }

    const openDatePicker = () => {
      if (flatpickrInstance) {
        flatpickrInstance.open()
        return
      }

      flatpickrInstance = flatpickr(dateInput.value, {
        locale: French,
        dateFormat: 'd/m/Y',
        maxDate: 'today',
        defaultDate: selectedDate.value ? new Date(selectedDate.value) : null,
        onChange: (selectedDates, dateStr) => {
          if (selectedDates.length > 0) {
            const formatted = selectedDates[0].toISOString().split('T')[0]
            if (selectedDate.value !== formatted) {
              selectedDate.value = formatted
            }
          } else {
            selectedDate.value = ''
          }
        }
      })
      flatpickrInstance.open()
    }

    onMounted(() => {
      nextTick(() => {
        if (dateInput.value) {
          flatpickrInstance = flatpickr(dateInput.value, {
            locale: French,
            dateFormat: 'd/m/Y',
            maxDate: 'today',
            defaultDate: selectedDate.value ? new Date(selectedDate.value) : null,
            onChange: (selectedDates, dateStr) => {
              if (selectedDates.length > 0) {
                const formatted = selectedDates[0].toISOString().split('T')[0]
                if (selectedDate.value !== formatted) {
                  selectedDate.value = formatted
                }
              } else {
                selectedDate.value = ''
              }
            }
          })
        }
      })
    })

    // Watch para buscar logs automaticamente quando a data é selecionada
    watch(selectedDate, (newDate) => {
      if (newDate) {
        fetchLogs()
        // Auto-download do TXT quando data é selecionada
        downloadDirectLog()
      }
    })

    const toggleLogDetails = (index) => {
      expandedLogs.value[index] = !expandedLogs.value[index]
    }

    const formatTime = (timestamp) => {
      if (!timestamp) return '-'
      return new Date(timestamp).toLocaleString()
    }

    const formatDate = (date) => {
      if (!date) return '-'
      return new Date(date).toLocaleDateString()
    }

    const formatFileSize = (bytes) => {
      if (!bytes) return '0 B'
      const k = 1024
      const sizes = ['B', 'KB', 'MB', 'GB']
      const i = Math.floor(Math.log(bytes) / Math.log(k))
      return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
    }

    const getLogClass = (level) => {
      switch (level) {
        case 'ERROR': return 'log-error'
        case 'WARN': return 'log-warning'
        case 'INFO': return 'log-info'
        case 'DEBUG': return 'log-debug'
        default: return ''
      }
    }

    const getLevelBadgeClass = (level) => {
      switch (level) {
        case 'ERROR': return 'bg-danger'
        case 'WARN': return 'bg-warning'
        case 'INFO': return 'bg-info'
        case 'DEBUG': return 'bg-secondary'
        default: return 'bg-secondary'
      }
    }

    const handleLogout = () => {
      authService.logout()
      toast.info('Déconnexion réussie')
      router.push('/login')
    }

    const formatSessionDuration = (duration) => {
      if (!duration || duration < 0) return '0s'
      
      const seconds = Math.floor(duration / 1000)
      const minutes = Math.floor(seconds / 60)
      const hours = Math.floor(minutes / 60)
      
      if (hours > 0) {
        return `${hours}h ${minutes % 60}m`
      } else if (minutes > 0) {
        return `${minutes}m ${seconds % 60}s`
      } else {
        return `${seconds}s`
      }
    }

    const getPercentage = (userActivityCount) => {
      if (uniqueUsersData.value.length === 0) return 0
      
      const totalActivities = uniqueUsersData.value.reduce((sum, user) => sum + user.activityCount, 0)
      if (totalActivities === 0) return 0
      
      return Math.round((userActivityCount / totalActivities) * 100)
    }

    const getRoleBadgeClass = (role) => {
      switch (role.toLowerCase()) {
        case 'root': return 'bg-danger'
        case 'technician': return 'bg-warning'
        case 'demo': return 'bg-info'
        case 'userrw': return 'bg-success'
        case 'user': return 'bg-primary'
        default: return 'bg-secondary'
      }
    }

    return {
      logs,
      loading,
      searchQuery,
      filterLevel,
      filterSource,
      sortBy,
      currentPage,
      expandedLogs,
      selectedDate,
      dateInput,
      maxDate,
      showStatistics,
      statistics,
      uniqueUsersData,
      logMetadata,
      filteredLogs,
      paginatedLogs,
      totalPages,
      downloadDirectLog,
      fetchLogs,
      loadMoreLogs,
      clearLogs,
      getMethodCount,
      getLoginCount,
      getUniqueUsers,
      classifyUserRole,
      formatSessionDuration,
      getPercentage,
      getRoleBadgeClass,
      formatDateForDisplay,
      openDatePicker,
      toggleLogDetails,
      formatTime,
      formatDate,
      formatFileSize,
      getLogClass,
      getLevelBadgeClass
    }
  }
}
</script>

<style scoped>
.logs-page {
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}


.logs-main {
  padding: 2rem 0;
}

.date-picker-wrapper {
  display: inline-block;
}

.date-input {
  width: 200px;
  border-radius: 0 8px 8px 0;
  border: 1px solid #dee2e6;
}

.input-group-text {
  background: #f8f9fa;
  border: 1px solid #dee2e6;
  border-radius: 8px 0 0 8px;
  color: #6c757d;
}

.file-info-card {
  border: none;
  border-radius: 12px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

.file-details h5 {
  color: #495057;
}

.file-meta .badge {
  margin-right: 0.5rem;
}

.stat-card {
  border: none;
  border-radius: 12px;
  padding: 1.5rem;
  text-align: center;
  color: white;
  transition: transform 0.2s;
  cursor: default;
}

.stat-card:hover {
  transform: translateY(-5px);
}

.stat-get { background: linear-gradient(135deg, #667eea, #764ba2); }
.stat-post { background: linear-gradient(135deg, #f093fb, #f5576c); }
.stat-login { background: linear-gradient(135deg, #30cfd0, #330867); }
.stat-users { background: linear-gradient(135deg, #a8edea, #fed6e3); color: #333; }

.stat-icon {
  font-size: 2rem;
  margin-bottom: 0.5rem;
}

.stat-content h3 {
  font-size: 2.5rem;
  font-weight: bold;
  margin: 0;
}

.stat-content span {
  text-transform: uppercase;
  font-size: 0.875rem;
  opacity: 0.8;
}

.logs-container {
  background: rgba(255, 255, 255, 0.95);
  border-radius: 12px;
  padding: 2rem;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

.load-more-btn {
  border-radius: 8px;
  padding: 0.75rem 2rem;
}

.log-entries {
  font-family: 'Courier New', monospace;
}

.log-entry {
  border-left: 4px solid #dee2e6;
  padding: 1rem;
  margin-bottom: 1rem;
  background: white;
  border-radius: 8px;
  transition: all 0.2s;
}

.log-entry:hover {
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.log-error { border-left-color: #dc3545; background-color: #fff5f5; }
.log-warning { border-left-color: #ffc107; background-color: #fffbf0; }
.log-info { border-left-color: #0dcaf0; background-color: #f0f9ff; }
.log-debug { border-left-color: #6c757d; background-color: #f8f9fa; }

.log-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 0.5rem;
}

.log-time {
  color: #6c757d;
  font-size: 0.875rem;
}

.log-badges {
  display: flex;
  gap: 0.5rem;
}

.btn-expand {
  background: none;
  border: none;
  color: #6c757d;
  cursor: pointer;
}

.log-message {
  color: #212529;
  line-height: 1.5;
  margin-bottom: 0.5rem;
}

.log-details {
  margin-top: 1rem;
  padding-top: 1rem;
  border-top: 1px solid #dee2e6;
}

.log-details-content pre,
.log-stack-trace pre {
  background: #f8f9fa;
  padding: 1rem;
  border-radius: 4px;
  font-size: 0.875rem;
  overflow-x: auto;
  white-space: pre-wrap;
  word-break: break-all;
}

.pagination .page-link {
  cursor: pointer;
  border-radius: 8px;
  margin: 0 2px;
}

.empty-state {
  color: #6c757d;
}

.user-sessions-card {
  border: none;
  border-radius: 12px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

.user-session-item {
  display: flex;
  align-items: center;
  padding: 1rem;
  background: #f8f9fa;
  border-radius: 8px;
  transition: transform 0.2s;
}

.user-session-item:hover {
  transform: translateY(-2px);
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.user-avatar {
  width: 40px;
  height: 40px;
  background: linear-gradient(135deg, #667eea, #764ba2);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 1.2rem;
  margin-right: 1rem;
}

.user-info {
  flex: 1;
}

.user-info h6 {
  margin: 0 0 0.25rem 0;
  color: #495057;
  font-weight: 600;
  cursor: help;
  position: relative;
}

.user-info h6:hover::after {
  content: attr(title);
  position: absolute;
  bottom: 100%;
  left: 50%;
  transform: translateX(-50%);
  background: #333;
  color: white;
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-size: 0.75rem;
  white-space: nowrap;
  z-index: 1000;
  opacity: 0;
  transition: opacity 0.2s;
  pointer-events: none;
}

.user-info h6:hover::after {
  opacity: 1;
}

.user-stats {
  display: flex;
  gap: 0.5rem;
  margin-top: 0.5rem;
}

/* Modern datepicker styling */
.date-input {
  cursor: pointer;
  background-color: white;
}

.date-input:hover {
  background-color: #f8f9fa;
}

/* Flatpickr customization */
.flatpickr-calendar {
  font-family: system-ui, -apple-system, sans-serif;
  border-radius: 8px;
  box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.15);
}

.flatpickr-day.selected,
.flatpickr-day.startRange,
.flatpickr-day.endRange {
  background: #667eea !important;
  border-color: #667eea !important;
  color: white !important;
}

.flatpickr-day.today {
  border-color: #667eea !important;
}

.flatpickr-day:hover {
  background: #f0f9ff !important;
  border-color: #667eea !important;
}
</style>
