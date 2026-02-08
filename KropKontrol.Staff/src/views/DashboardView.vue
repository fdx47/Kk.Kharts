<template>
  <div class="dashboard">
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
        <button class="btn btn-primary btn-sm" @click="refreshData" :disabled="loading">
          <i class="bi bi-arrow-clockwise me-1" :class="{ 'spin': loading }"></i>
          Actualiser
        </button>
      </template>
    </NavigationHeader>

    <!-- Main Content -->
    <main class="dashboard-main">
      <div class="container-fluid">
        <!-- Loading State -->
        <div v-if="loading && !hasData" class="text-center py-5">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Chargement...</span>
          </div>
          <p class="mt-3 text-muted">Chargement des données...</p>
        </div>

        <!-- Dashboard Content -->
        <div v-else>
          <!-- Quick Stats Row -->
          <div class="row mb-4">
            <div class="col-lg-3 col-md-6 mb-3">
              <div class="stat-card stat-devices">
                <div class="stat-icon">
                  <i class="bi bi-cpu"></i>
                </div>
                <div class="stat-content">
                  <h3>{{ stats.totalDevices }}</h3>
                  <span>Capteurs Actifs</span>
                  <div class="stat-detail">
                    <span class="text-success">{{ stats.onlineDevices }} en ligne</span>
                  </div>
                </div>
              </div>
            </div>
            <div class="col-lg-3 col-md-6 mb-3">
              <div class="stat-card stat-offline" @click="showOfflineModal = true">
                <div class="stat-icon">
                  <i class="bi bi-wifi-off"></i>
                </div>
                <div class="stat-content">
                  <h3>{{ stats.offlineDevices }}</h3>
                  <span>Hors Ligne</span>
                  <div class="stat-detail">
                    <span class="text-warning">{{ stats.inactiveDevices }} inactifs</span>
                  </div>
                </div>
              </div>
            </div>
            <div class="col-lg-3 col-md-6 mb-3">
              <div class="stat-card stat-users">
                <div class="stat-icon">
                  <i class="bi bi-people"></i>
                </div>
                <div class="stat-content">
                  <h3>{{ stats.totalUsers }}</h3>
                  <span>Utilisateurs</span>
                  <div class="stat-detail">
                    <span class="text-info">{{ stats.linkedTelegramUsers }} Telegram</span>
                  </div>
                </div>
              </div>
            </div>
            <div class="col-lg-3 col-md-6 mb-3">
              <div class="stat-card stat-alerts">
                <div class="stat-icon">
                  <i class="bi bi-bell"></i>
                </div>
                <div class="stat-content">
                  <h3>{{ stats.activeAlerts }}</h3>
                  <span>Alertes Actives</span>
                  <div class="stat-detail">
                    <span class="text-secondary">{{ stats.totalRules }} règles</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Charts Row -->
          <div class="row mb-4">
            <div class="col-lg-8 mb-3">
              <div class="card chart-card">
                <div class="card-header">
                  <h5 class="mb-0">
                    <i class="bi bi-graph-up me-2"></i>
                    Activité API (24h)
                  </h5>
                </div>
                <div class="card-body">
                  <canvas ref="activityChart" height="250"></canvas>
                </div>
              </div>
            </div>
            <div class="col-lg-4 mb-3">
              <div class="card chart-card">
                <div class="card-header">
                  <h5 class="mb-0">
                    <i class="bi bi-pie-chart me-2"></i>
                    Méthodes HTTP
                  </h5>
                </div>
                <div class="card-body">
                  <canvas ref="methodsChart" height="250"></canvas>
                </div>
              </div>
            </div>
          </div>

          <!-- Devices & Users Row -->
          <div class="row mb-4">
            <!-- Offline Devices -->
            <div class="col-lg-6 mb-3">
              <div class="card data-card">
                <div class="card-header d-flex justify-content-between align-items-center">
                  <h5 class="mb-0">
                    <i class="bi bi-wifi-off text-danger me-2"></i>
                    Capteurs Hors Ligne
                  </h5>
                  <span class="badge bg-danger">{{ offlineDevicesList.length }}</span>
                </div>
                <div class="card-body">
                  <div v-if="offlineDevicesList.length === 0" class="text-center text-muted py-4">
                    <i class="bi bi-check-circle" style="font-size: 2rem;"></i>
                    <p class="mt-2 mb-0">Tous les capteurs sont en ligne</p>
                  </div>
                  <div v-else class="device-list">
                    <div v-for="device in offlineDevicesList.slice(0, 5)" :key="device.devEui" class="device-item offline">
                      <div class="device-icon">
                        <i class="bi bi-cpu"></i>
                      </div>
                      <div class="device-info">
                        <h6>{{ device.name }}</h6>
                        <small class="text-muted">{{ device.company }}</small>
                        <div class="device-meta">
                          <span class="badge bg-secondary">{{ device.location }}</span>
                          <span class="text-danger">
                            <i class="bi bi-clock me-1"></i>
                            {{ formatLastSeen(device.lastSendAt) }}
                          </span>
                        </div>
                      </div>
                    </div>
                    <div v-if="offlineDevicesList.length > 5" class="text-center mt-3">
                      <button class="btn btn-outline-danger btn-sm" @click="showAllOffline = true">
                        Voir tous ({{ offlineDevicesList.length }})
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Active Users -->
            <div class="col-lg-6 mb-3">
              <div class="card data-card">
                <div class="card-header d-flex justify-content-between align-items-center">
                  <h5 class="mb-0">
                    <i class="bi bi-person-check text-success me-2"></i>
                    Utilisateurs Actifs
                  </h5>
                  <span class="badge bg-success">{{ activeUsers.length }}</span>
                </div>
                <div class="card-body">
                  <div v-if="activeUsers.length === 0" class="text-center text-muted py-4">
                    <i class="bi bi-person-x" style="font-size: 2rem;"></i>
                    <p class="mt-2 mb-0">Aucun utilisateur actif</p>
                  </div>
                  <div v-else class="user-list">
                    <div v-for="user in activeUsers.slice(0, 5)" :key="user.name" class="user-item">
                      <div class="user-avatar" :style="{ background: getAvatarColor(user.name) }">
                        {{ getInitials(user.name) }}
                      </div>
                      <div class="user-info">
                        <h6>{{ user.name }}</h6>
                        <div class="user-stats">
                          <span class="badge" :class="getRoleBadgeClass(user.role)">{{ user.role }}</span>
                          <span class="badge bg-primary">{{ user.totalRequests }} req</span>
                          <span class="text-muted">
                            <i class="bi bi-clock me-1"></i>
                            {{ formatSessionTime(user.sessionDuration) }}
                          </span>
                        </div>
                        <div class="user-methods mt-1">
                          <span v-for="(count, method) in user.methods" :key="method" class="method-badge">
                            {{ method }}: {{ count }}
                          </span>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Companies & Battery Row -->
          <div class="row mb-4">
            <!-- Top Companies -->
            <div class="col-lg-6 mb-3">
              <div class="card data-card">
                <div class="card-header">
                  <h5 class="mb-0">
                    <i class="bi bi-building me-2"></i>
                    Top Sociétés (Capteurs)
                  </h5>
                </div>
                <div class="card-body">
                  <div v-for="(company, index) in topCompanies" :key="company.name" class="company-item">
                    <div class="company-rank">{{ index + 1 }}</div>
                    <div class="company-info">
                      <h6>{{ company.name }}</h6>
                      <div class="progress" style="height: 8px;">
                        <div 
                          class="progress-bar" 
                          :style="{ width: getCompanyPercentage(company.count) + '%', background: getCompanyColor(index) }"
                        ></div>
                      </div>
                    </div>
                    <div class="company-count">
                      <span class="badge bg-primary">{{ company.count }}</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Low Battery Devices -->
            <div class="col-lg-6 mb-3">
              <div class="card data-card">
                <div class="card-header d-flex justify-content-between align-items-center">
                  <h5 class="mb-0">
                    <i class="bi bi-battery-half text-warning me-2"></i>
                    Batterie Faible (≤20%)
                  </h5>
                  <span class="badge bg-warning text-dark">{{ lowBatteryDevices.length }}</span>
                </div>
                <div class="card-body">
                  <div v-if="lowBatteryDevices.length === 0" class="text-center text-muted py-4">
                    <i class="bi bi-battery-full" style="font-size: 2rem;"></i>
                    <p class="mt-2 mb-0">Tous les capteurs ont une batterie suffisante</p>
                  </div>
                  <div v-else class="battery-list">
                    <div v-for="device in lowBatteryDevices.slice(0, 5)" :key="device.devEui" class="battery-item">
                      <div class="battery-icon" :class="getBatteryClass(device.battery)">
                        <i :class="getBatteryIcon(device.battery)"></i>
                      </div>
                      <div class="battery-info">
                        <h6>{{ device.name }}</h6>
                        <small class="text-muted">{{ device.company }}</small>
                      </div>
                      <div class="battery-level">
                        <span class="badge" :class="getBatteryBadgeClass(device.battery)">
                          {{ device.battery }}%
                        </span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Activity Feed -->
          <div class="row">
            <div class="col-12">
              <div class="card data-card">
                <div class="card-header d-flex justify-content-between align-items-center">
                  <h5 class="mb-0">
                    <i class="bi bi-activity me-2"></i>
                    Activité Récente
                  </h5>
                  <div class="btn-group btn-group-sm">
                    <button 
                      class="btn" 
                      :class="activityFilter === 'all' ? 'btn-primary' : 'btn-outline-primary'"
                      @click="activityFilter = 'all'"
                    >Tout</button>
                    <button 
                      class="btn" 
                      :class="activityFilter === 'errors' ? 'btn-danger' : 'btn-outline-danger'"
                      @click="activityFilter = 'errors'"
                    >Erreurs</button>
                    <button 
                      class="btn" 
                      :class="activityFilter === 'logins' ? 'btn-success' : 'btn-outline-success'"
                      @click="activityFilter = 'logins'"
                    >Connexions</button>
                  </div>
                </div>
                <div class="card-body">
                  <div class="activity-feed">
                    <div v-for="(activity, index) in filteredActivities.slice(0, 10)" :key="index" class="activity-item">
                      <div class="activity-icon" :class="getActivityIconClass(activity.type)">
                        <i :class="getActivityIcon(activity.type)"></i>
                      </div>
                      <div class="activity-content">
                        <p class="mb-0">{{ activity.message }}</p>
                        <small class="text-muted">
                          <i class="bi bi-clock me-1"></i>
                          {{ formatActivityTime(activity.timestamp) }}
                        </small>
                      </div>
                    </div>
                    <div v-if="filteredActivities.length === 0" class="text-center text-muted py-4">
                      <i class="bi bi-inbox" style="font-size: 2rem;"></i>
                      <p class="mt-2 mb-0">Aucune activité récente</p>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </main>

    <!-- Modal for offline devices -->
    <div v-if="showOfflineModal" class="modal-overlay" @click.self="showOfflineModal = false">
      <div class="modal-panel">
        <div class="modal-header">
          <h5 class="mb-0">Dispositivos Offline</h5>
          <button type="button" class="btn-close" @click="showOfflineModal = false"></button>
        </div>
        <div class="modal-body">
          <table class="table table-striped">
            <thead>
              <tr>
                <th>Nome</th>
                <th>DevEUI</th>
                <th>Última comunicação</th>
                <th>Bateria</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="device in offlineDevicesList" :key="device.devEui">
                <td>{{ device.name }}</td>
                <td>{{ device.devEui }}</td>
                <td>{{ formatLastSeen(device.lastSendAt) }}</td>
                <td>{{ device.battery }}%</td>
              </tr>
            </tbody>
          </table>
        </div>
        <div class="modal-footer">
          <button class="btn btn-secondary" @click="showOfflineModal = false">Fermer</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, computed, onMounted, nextTick, watch } from 'vue'
import { useRouter } from 'vue-router'
import { deviceService, logsService, userService } from '@/services/api'
import { toast } from 'vue3-toastify'
import flatpickr from 'flatpickr'
import 'flatpickr/dist/flatpickr.min.css'
import { French } from 'flatpickr/dist/l10n/fr'
import Chart from 'chart.js/auto'
import NavigationHeader from '@/components/NavigationHeader.vue'

export default {
  name: 'DashboardView',
  components: {
    NavigationHeader
  },
  setup() {
    const router = useRouter()
    const loading = ref(false)
    const selectedDate = ref('')
    const dateInput = ref(null)
    const activityChart = ref(null)
    const methodsChart = ref(null)
    const activityFilter = ref('all')
    const showAllOffline = ref(false)
    const showOfflineModal = ref(false)
    let flatpickrInstance = null
    let activityChartInstance = null
    let methodsChartInstance = null

    // Data
    const stats = ref({
      totalDevices: 0,
      onlineDevices: 0,
      offlineDevices: 0,
      inactiveDevices: 0,
      lowBatteryDevices: 0,
      activeAlerts: 0,
      totalRules: 0,
      totalUsers: 0,
      linkedTelegramUsers: 0,
      dashboards: 0
    })

    const systemAccounts = new Set(['root', 'admin', 'system', 'demo', 'userrw', 'technician', 'tech'])

    const offlineDevicesList = ref([])

    const lowBatteryDevices = ref([])
    const activeUsers = ref([])
    const topCompanies = ref([])
    const activities = ref([])
    const logs = ref([])

    const hasData = computed(() => stats.value.totalDevices > 0 || logs.value.length > 0)

    const filteredActivities = computed(() => {
      if (activityFilter.value === 'all') return activities.value
      if (activityFilter.value === 'errors') return activities.value.filter(a => a.type === 'error')
      if (activityFilter.value === 'logins') return activities.value.filter(a => a.type === 'login')
      return activities.value
    })

    // Methods
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
        defaultDate: selectedDate.value ? new Date(selectedDate.value) : new Date(),
        onChange: (selectedDates) => {
          if (selectedDates.length > 0) {
            const formatted = selectedDates[0].toISOString().split('T')[0]
            if (selectedDate.value !== formatted) {
              selectedDate.value = formatted
            }
          }
        }
      })
      flatpickrInstance.open()
    }

    const isDeviceOnline = (device) => {
      const activeFlag = device?.activeInKropKontrol ?? device?.ActiveInKropKontrol
      const isActive = activeFlag === true

      const lastSendAt = device?.lastSendAt ?? device?.LastSendAt
      const isRecent = lastSendAt && (
        lastSendAt === "À l'instant" ||
        lastSendAt.includes('seconde') ||
        (lastSendAt.includes('minute') && parseInt(lastSendAt.match(/\d+/)?.[0] || 0) <= 15)
      )

      return isActive && isRecent
    }

    const refreshData = async () => {
      loading.value = true
      try {
        const date = selectedDate.value || new Date().toISOString().split('T')[0]

        const [devicesRaw, usersRaw] = await Promise.all([
          deviceService.getAll(),
          userService.getAll()
        ])

        const devices = Array.isArray(devicesRaw) ? devicesRaw : []
        const users = Array.isArray(usersRaw) ? usersRaw : []

        const getActiveFlag = (d) => (d?.activeInKropKontrol ?? d?.ActiveInKropKontrol)
        const getBattery = (d) => (d?.battery ?? d?.Battery)
        const getDevEui = (d) => (d?.devEui ?? d?.DevEui)
        const getName = (d) => (d?.name ?? d?.Name)
        const getCompanyName = (d) => (d?.companyName ?? d?.CompanyName)
        const getInstallationLocation = (d) => (d?.installationLocation ?? d?.InstallationLocation)
        const getLastSendAt = (d) => (d?.lastSendAt ?? d?.LastSendAt)

        const inactive = devices.filter(d => getActiveFlag(d) === false)
        const active = devices.filter(d => getActiveFlag(d) !== false)

        const online = active.filter(isDeviceOnline)
        const offline = active.filter(d => !isDeviceOnline(d))

        stats.value.totalDevices = active.length
        stats.value.onlineDevices = online.length
        stats.value.offlineDevices = offline.length
        stats.value.inactiveDevices = inactive.length
        stats.value.lowBatteryDevices = active.filter(d => typeof getBattery(d) === 'number' && getBattery(d) <= 20).length

        offlineDevicesList.value = offline.map(d => ({
          name: getName(d) || getDevEui(d),
          company: getCompanyName(d) || 'KropKontrol',
          location: getInstallationLocation(d) || '—',
          devEui: getDevEui(d),
          lastSendAt: getLastSendAt(d),
          battery: getBattery(d)
        }))

        lowBatteryDevices.value = active
          .filter(d => typeof getBattery(d) === 'number' && getBattery(d) <= 20)
          .map(d => ({
            name: getName(d) || getDevEui(d),
            company: getCompanyName(d) || 'KropKontrol',
            devEui: getDevEui(d),
            battery: getBattery(d)
          }))

        const companyCounts = new Map()
        active.forEach(d => {
          const company = getCompanyName(d) || 'KropKontrol'
          companyCounts.set(company, (companyCounts.get(company) || 0) + 1)
        })
        topCompanies.value = Array.from(companyCounts.entries())
          .map(([name, count]) => ({ name, count }))
          .sort((a, b) => b.count - a.count)
          .slice(0, 5)

        stats.value.totalUsers = users.length
        stats.value.linkedTelegramUsers = 0
        stats.value.activeAlerts = 0
        stats.value.totalRules = 0

        // Process logs for activity timeline and charts using raw download endpoint
        const rawLogText = await logsService.fetchLogFile(date)
        const parsedLogs = parseLogFile(rawLogText).slice(-2000)

        if (parsedLogs.length) {
          logs.value = parsedLogs
          processActivityLogs(parsedLogs)

          const sessions = new Map()
          const methodRegex = /\b(GET|POST|PUT|DELETE)\b/i

          parsedLogs.forEach(entry => {
            const msg = entry.message || ''
            const ts = new Date(entry.timestamp)
            if (Number.isNaN(ts)) return

            const extractedName = entry.user || extractUserName(msg)
            if (!extractedName) {
              return
            }

            const normalized = extractedName.replace(/\s+/g, ' ').trim()
            if (!normalized) return

            const lowerName = normalized.toLowerCase()
            if (systemAccounts.has(lowerName)) return

            const displayName = formatDisplayName(normalized)
            const detectedRole = classifyUserRole(normalized, msg)

            if (!sessions.has(displayName)) {
              sessions.set(displayName, {
                displayName,
                fullName: normalized,
                role: detectedRole,
                firstSeen: ts,
                lastSeen: ts,
                totalRequests: 0,
                methods: { GET: 0, POST: 0, PUT: 0, DELETE: 0 }
              })
            }

            const s = sessions.get(displayName)
            s.totalRequests += 1
            if (ts < s.firstSeen) s.firstSeen = ts
            if (ts > s.lastSeen) s.lastSeen = ts
            if (detectedRole && detectedRole !== 'unknown') {
              s.role = detectedRole
            }

            const methodFromEntry = (entry.method || '').toUpperCase()
            if (methodFromEntry && s.methods[methodFromEntry] !== undefined) {
              s.methods[methodFromEntry] += 1
            } else {
              const m = msg.match(methodRegex)
              if (m) {
                const method = m[1].toUpperCase()
                if (s.methods[method] !== undefined) s.methods[method] += 1
              }
            }
          })

          activeUsers.value = Array.from(sessions.values())
            .map(s => ({
              name: s.displayName,
              role: s.role || 'user',
              totalRequests: s.totalRequests,
              sessionDuration: s.lastSeen - s.firstSeen,
              methods: Object.fromEntries(Object.entries(s.methods).filter(([, v]) => v > 0))
            }))
            .sort((a, b) => b.totalRequests - a.totalRequests)
            .slice(0, 10)
        } else {
          logs.value = []
          activeUsers.value = []
        }
        
        toast.success('Données actualisées')
      } catch (error) {
        toast.error('Erreur lors de la récupération des données: ' + error.message)
      } finally {
        loading.value = false
      }
    }

    const processActivityLogs = (logEntries) => {
      // Process logs only for activity timeline (last 24h)
      const activityList = []
      const methodCounts = { GET: 0, POST: 0, PUT: 0, DELETE: 0 }
      const hourlyActivity = Array(24).fill(0)
      const now = new Date()
      const windowStart = new Date(now.getTime() - 24 * 60 * 60 * 1000)

      logEntries.forEach(log => {
        const message = log.message || ''
        const timestamp = new Date(log.timestamp)
        if (Number.isNaN(timestamp)) return
        if (timestamp < windowStart || timestamp > now) return

        const hour = timestamp.getHours()
        hourlyActivity[hour]++

        // Extract HTTP methods
        const normalizedMethod = (log.method || '').toUpperCase()
        if (normalizedMethod && methodCounts[normalizedMethod] !== undefined) {
          methodCounts[normalizedMethod]++
        } else {
          const methodMatch = message.match(/Méthode\s+HTTP\s*:\s*(GET|POST|PUT|DELETE)/i)
          if (methodMatch) {
            const method = methodMatch[1].toUpperCase()
            methodCounts[method]++
          }
        }

        // Create activity entries
        if (message.toLowerCase().includes('error') || log.level === 'ERROR') {
          activityList.push({
            type: 'error',
            message: message.substring(0, 100),
            timestamp: timestamp
          })
        } else if (message.toLowerCase().includes('login') || message.toLowerCase().includes('connecté')) {
          activityList.push({
            type: 'login',
            message: message.substring(0, 100),
            timestamp: timestamp
          })
        } else if (activityList.length < 50) {
          activityList.push({
            type: 'info',
            message: message.substring(0, 100),
            timestamp: timestamp
          })
        }
      })

      // Update activities
      activities.value = activityList.sort((a, b) => b.timestamp - a.timestamp)

      // Update charts
      updateCharts(hourlyActivity, methodCounts)
    }

    const updateCharts = (hourlyData, methodData) => {
      // Activity Chart
      if (activityChartInstance) {
        activityChartInstance.destroy()
      }

      if (activityChart.value) {
        const ctx = activityChart.value.getContext('2d')
        activityChartInstance = new Chart(ctx, {
          type: 'line',
          data: {
            labels: Array.from({ length: 24 }, (_, i) => `${i}h`),
            datasets: [{
              label: 'Requêtes',
              data: hourlyData,
              borderColor: '#667eea',
              backgroundColor: 'rgba(102, 126, 234, 0.1)',
              fill: true,
              tension: 0.4
            }]
          },
          options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
              legend: { display: false }
            },
            scales: {
              y: { beginAtZero: true }
            }
          }
        })
      }

      // Methods Chart
      if (methodsChartInstance) {
        methodsChartInstance.destroy()
      }

      if (methodsChart.value) {
        const ctx = methodsChart.value.getContext('2d')
        methodsChartInstance = new Chart(ctx, {
          type: 'doughnut',
          data: {
            labels: Object.keys(methodData),
            datasets: [{
              data: Object.values(methodData),
              backgroundColor: ['#667eea', '#f093fb', '#4facfe', '#fa709a']
            }]
          },
          options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
              legend: { position: 'bottom' }
            }
          }
        })
      }
    }

    // Helper functions
    const formatLastSeen = (lastSendAt) => {
      if (!lastSendAt) return 'Jamais'
      const parsed = Date.parse(lastSendAt)
      if (Number.isNaN(parsed)) return lastSendAt
      const date = new Date(parsed)
      const now = new Date()
      const diffMs = now - date
      const diffHours = Math.floor(diffMs / (1000 * 60 * 60))
      const diffDays = Math.floor(diffHours / 24)
      
      if (diffDays > 0) return `Il y a ${diffDays}j`
      if (diffHours > 0) return `Il y a ${diffHours}h`
      return "Récemment"
    }

    const extractUserName = (message) => {
      if (!message) return null
      const patterns = [
        /(?:User:|user\s+|Utilisateur\s+connecté\s*:|Utilisateur\s*: )\s*([A-Za-zÀ-ÿ][A-Za-zÀ-ÿ\s]+)/i,
        /([A-Za-zÀ-ÿ][A-Za-zÀ-ÿ\s]+)\s+(?:logged\s+in|connexion|login)/i,
        /(?:User:|user\s+)(\w+)/i
      ]
      for (const pattern of patterns) {
        const match = message.match(pattern)
        if (match?.[1]) {
          return match[1]
        }
      }
      return null
    }

    const classifyUserRole = (username, message) => {
      const lowerName = (username || '').toLowerCase()
      const msgLower = (message || '').toLowerCase()
      if (lowerName.includes('root') || msgLower.includes('root') || msgLower.includes('admin') || msgLower.includes('administrator')) {
        return 'root'
      }
      if (lowerName.includes('tech') || msgLower.includes('technician') || msgLower.includes('tech') || msgLower.includes('support')) {
        return 'technician'
      }
      if (lowerName.includes('demo') || msgLower.includes('demo') || msgLower.includes('test')) {
        return 'demo'
      }
      if (lowerName.includes('userrw') || msgLower.includes('userrw') || msgLower.includes('read-write')) {
        return 'UserRW'
      }
      return 'user'
    }

    const formatDisplayName = (name) => {
      return name.length > 24 ? `${name.substring(0, 24)}…` : name
    }

    const parseLogFile = (rawText) => {
      if (!rawText) return []
      const lines = rawText.split(/\r?\n/)
      const entries = []

      const resetCurrent = () => ({
        user: null,
        endpoint: null,
        method: null,
        timestamp: null,
        params: [],
        collectingParams: false,
        extra: []
      })

      let current = resetCurrent()

      const commitEntry = () => {
        if (!current.user && !current.endpoint && !current.timestamp) {
          current = resetCurrent()
          return
        }

        const timestamp = current.timestamp || new Date().toISOString()
        const paramsText = current.params.join(' ').trim()
        const messageParts = [
          current.user ? `Utilisateur: ${current.user}` : null,
          current.endpoint ? `Endpoint: ${current.endpoint}` : null,
          current.method ? `Méthode: ${current.method}` : null,
          paramsText ? `Paramètres: ${paramsText}` : null,
          current.extra.length ? current.extra.join(' ') : null
        ].filter(Boolean)

        entries.push({
          timestamp,
          level: 'INFO',
          source: current.endpoint || 'API',
          message: messageParts.join(' | '),
          user: current.user,
          endpoint: current.endpoint,
          method: current.method,
          params: paramsText || null
        })

        current = resetCurrent()
      }

      lines.forEach(rawLine => {
        const line = rawLine.trim()
        if (!line) return

        if (/^-{5,}$/.test(line)) {
          commitEntry()
          return
        }

        if (line.toLowerCase().startsWith('utilisateur')) {
          current.user = line.split(':')[1]?.trim() || line
          return
        }

        if (line.toLowerCase().startsWith('endpoint')) {
          current.endpoint = line.split(':')[1]?.trim() || line
          return
        }

        if (line.toLowerCase().startsWith('méthode') || line.toLowerCase().startsWith('methode')) {
          current.method = line.split(':')[1]?.trim().toUpperCase() || 'GET'
          return
        }

        if (line.toLowerCase().startsWith('paramètres') || line.toLowerCase().startsWith('parametres')) {
          current.collectingParams = true
          current.params = []
          return
        }

        if (line.toLowerCase().startsWith('@ heure') || line.toLowerCase().startsWith('heure')) {
          current.timestamp = parseLogTimestamp(line)
          current.collectingParams = false
          return
        }

        if (current.collectingParams) {
          current.params.push(line)
          return
        }

        current.extra.push(line)
      })

      commitEntry()

      return entries
    }

    const parseLogTimestamp = (line) => {
      const normalized = line.replace(/^@?\s*Heure\s*:\s*/i, '').trim()
      const match = normalized.match(/(\d{2})\/(\d{2})\/(\d{2}) (\d{2}):(\d{2}):(\d{2})/)
      if (!match) {
        return new Date().toISOString()
      }
      const [, day, month, year, hour, minute, second] = match
      const fullYear = 2000 + parseInt(year, 10)
      const date = new Date(Date.UTC(fullYear, parseInt(month, 10) - 1, parseInt(day, 10), parseInt(hour, 10), parseInt(minute, 10), parseInt(second, 10)))
      return date.toISOString()
    }

    const formatSessionTime = (duration) => {
      if (!duration || duration < 0) return '0s'
      const seconds = Math.floor(duration / 1000)
      const minutes = Math.floor(seconds / 60)
      const hours = Math.floor(minutes / 60)
      if (hours > 0) return `${hours}h ${minutes % 60}m`
      if (minutes > 0) return `${minutes}m`
      return `${seconds}s`
    }

    const formatActivityTime = (timestamp) => {
      if (!timestamp) return ''
      return new Date(timestamp).toLocaleString('fr-FR', {
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit'
      })
    }

    const getInitials = (name) => {
      if (!name) return '?'
      const parts = name.split(' ')
      return parts.map(p => p[0]).join('').substring(0, 2).toUpperCase()
    }

    const getAvatarColor = (name) => {
      const colors = ['#667eea', '#f093fb', '#4facfe', '#fa709a', '#30cfd0', '#a8edea']
      const index = name.charCodeAt(0) % colors.length
      return colors[index]
    }

    const getRoleBadgeClass = (role) => {
      switch (role?.toLowerCase()) {
        case 'root': return 'bg-danger'
        case 'technician': return 'bg-warning'
        case 'demo': return 'bg-info'
        default: return 'bg-secondary'
      }
    }

    const getCompanyPercentage = (count) => {
      const max = topCompanies.value[0]?.count || 1
      return (count / max) * 100
    }

    const getCompanyColor = (index) => {
      const colors = ['#667eea', '#f093fb', '#4facfe', '#fa709a', '#30cfd0']
      return colors[index % colors.length]
    }

    const getBatteryClass = (level) => {
      if (level <= 10) return 'battery-critical'
      if (level <= 20) return 'battery-low'
      return 'battery-ok'
    }

    const getBatteryIcon = (level) => {
      if (level <= 10) return 'bi bi-battery'
      if (level <= 25) return 'bi bi-battery-half'
      return 'bi bi-battery-full'
    }

    const getBatteryBadgeClass = (level) => {
      if (level <= 10) return 'bg-danger'
      if (level <= 20) return 'bg-warning text-dark'
      return 'bg-success'
    }

    const getActivityIconClass = (type) => {
      switch (type) {
        case 'error': return 'activity-error'
        case 'login': return 'activity-login'
        default: return 'activity-info'
      }
    }

    const getActivityIcon = (type) => {
      switch (type) {
        case 'error': return 'bi bi-exclamation-triangle'
        case 'login': return 'bi bi-person-check'
        default: return 'bi bi-info-circle'
      }
    }

    
    // Lifecycle
    onMounted(() => {
      // Set default date to today
      selectedDate.value = new Date().toISOString().split('T')[0]
      
      nextTick(() => {
        if (dateInput.value) {
          flatpickrInstance = flatpickr(dateInput.value, {
            locale: French,
            dateFormat: 'd/m/Y',
            maxDate: 'today',
            defaultDate: new Date(),
            onChange: (selectedDates) => {
              if (selectedDates.length > 0) {
                const formatted = selectedDates[0].toISOString().split('T')[0]
                if (selectedDate.value !== formatted) {
                  selectedDate.value = formatted
                }
              }
            }
          })
        }
        
        // Initial data load from API
        refreshData()
      })
    })

    // Watch for date changes
    watch(selectedDate, (newDate) => {
      if (newDate) {
        refreshData()
      }
    })

    return {
      loading,
      selectedDate,
      dateInput,
      activityChart,
      methodsChart,
      activityFilter,
      showAllOffline,
      showOfflineModal,
      stats,
      offlineDevicesList,
      lowBatteryDevices,
      activeUsers,
      topCompanies,
      activities,
      logs,
      hasData,
      filteredActivities,
      formatDateForDisplay,
      openDatePicker,
      refreshData,
      formatLastSeen,
      formatSessionTime,
      formatActivityTime,
      getInitials,
      getAvatarColor,
      getRoleBadgeClass,
      getCompanyPercentage,
      getCompanyColor,
      getBatteryClass,
      getBatteryIcon,
      getBatteryBadgeClass,
      getActivityIconClass,
      getActivityIcon
    }
  }
}
</script>

<style scoped>
.dashboard {
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.dashboard-main {
  padding: 2rem 0;
}

.date-picker-wrapper {
  display: inline-block;
}

.date-input {
  width: 180px;
  cursor: pointer;
}

.spin {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

/* Stat Cards */
.stat-card {
  border: none;
  border-radius: 16px;
  padding: 1.5rem;
  color: white;
  display: flex;
  align-items: center;
  gap: 1rem;
  transition: transform 0.3s, box-shadow 0.3s;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
}

.stat-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15);
}

.stat-devices { background: linear-gradient(135deg, #667eea, #764ba2); }
.stat-offline { background: linear-gradient(135deg, #f093fb, #f5576c); }
.stat-users { background: linear-gradient(135deg, #4facfe, #00f2fe); }
.stat-alerts { background: linear-gradient(135deg, #fa709a, #fee140); }

.stat-icon {
  width: 60px;
  height: 60px;
  background: rgba(255, 255, 255, 0.2);
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
}

.stat-content h3 {
  font-size: 2rem;
  font-weight: bold;
  margin: 0;
}

.stat-content span {
  opacity: 0.9;
  font-size: 0.875rem;
}

.stat-detail {
  margin-top: 0.25rem;
  font-size: 0.75rem;
}

/* Chart Cards */
.chart-card {
  border: none;
  border-radius: 16px;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
  height: 100%;
}

.chart-card .card-header {
  background: transparent;
  border-bottom: 1px solid #eee;
  padding: 1rem 1.5rem;
}

.chart-card .card-body {
  padding: 1.5rem;
}

/* Data Cards */
.data-card {
  border: none;
  border-radius: 16px;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
}

.data-card .card-header {
  background: transparent;
  border-bottom: 1px solid #eee;
  padding: 1rem 1.5rem;
}

.data-card .card-body {
  padding: 1.5rem;
  max-height: 400px;
  overflow-y: auto;
}

/* Device List */
.device-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem;
  background: #f8f9fa;
  border-radius: 12px;
  margin-bottom: 0.75rem;
  transition: transform 0.2s;
}

.device-item:hover {
  transform: translateX(5px);
}

.device-item.offline {
  border-left: 4px solid #dc3545;
}

.device-icon {
  width: 40px;
  height: 40px;
  background: linear-gradient(135deg, #667eea, #764ba2);
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
}

.device-info h6 {
  margin: 0 0 0.25rem 0;
  font-weight: 600;
}

.device-meta {
  display: flex;
  gap: 0.5rem;
  align-items: center;
  font-size: 0.75rem;
}

/* User List */
.user-item {
  display: flex;
  align-items: flex-start;
  gap: 1rem;
  padding: 1rem;
  background: #f8f9fa;
  border-radius: 12px;
  margin-bottom: 0.75rem;
}

.user-avatar {
  width: 45px;
  height: 45px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-weight: bold;
  font-size: 0.875rem;
}

.user-info h6 {
  margin: 0 0 0.5rem 0;
  font-weight: 600;
}

.user-stats {
  display: flex;
  gap: 0.5rem;
  align-items: center;
  flex-wrap: wrap;
}

.user-methods {
  display: flex;
  gap: 0.25rem;
  flex-wrap: wrap;
}

.method-badge {
  font-size: 0.65rem;
  padding: 0.15rem 0.4rem;
  background: #e9ecef;
  border-radius: 4px;
  color: #495057;
}

/* Company List */
.company-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 0.75rem 0;
  border-bottom: 1px solid #eee;
}

.company-item:last-child {
  border-bottom: none;
}

.company-rank {
  width: 30px;
  height: 30px;
  background: linear-gradient(135deg, #667eea, #764ba2);
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-weight: bold;
  font-size: 0.875rem;
}

.company-info {
  flex: 1;
}

.company-info h6 {
  margin: 0 0 0.25rem 0;
  font-size: 0.875rem;
}

/* Battery List */
.battery-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 0.75rem;
  background: #f8f9fa;
  border-radius: 10px;
  margin-bottom: 0.5rem;
}

.battery-icon {
  width: 35px;
  height: 35px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.25rem;
}

.battery-critical {
  background: #fee2e2;
  color: #dc3545;
}

.battery-low {
  background: #fef3c7;
  color: #f59e0b;
}

.battery-ok {
  background: #d1fae5;
  color: #10b981;
}

.battery-info {
  flex: 1;
}

.battery-info h6 {
  margin: 0;
  font-size: 0.875rem;
}

/* Activity Feed */
.activity-feed {
  max-height: 400px;
  overflow-y: auto;
}

.activity-item {
  display: flex;
  gap: 1rem;
  padding: 0.75rem 0;
  border-bottom: 1px solid #eee;
}

.activity-item:last-child {
  border-bottom: none;
}

.activity-icon {
  width: 35px;
  height: 35px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.activity-error {
  background: #fee2e2;
  color: #dc3545;
}

.activity-login {
  background: #d1fae5;
  color: #10b981;
}

.activity-info {
  background: #e0f2fe;
  color: #0284c7;
}

.activity-content {
  flex: 1;
}

.activity-content p {
  font-size: 0.875rem;
  word-break: break-word;
}

/* Scrollbar */
.data-card .card-body::-webkit-scrollbar,
.activity-feed::-webkit-scrollbar {
  width: 6px;
}

.data-card .card-body::-webkit-scrollbar-track,
.activity-feed::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 3px;
}

.data-card .card-body::-webkit-scrollbar-thumb,
.activity-feed::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 3px;
}

.data-card .card-body::-webkit-scrollbar-thumb:hover,
.activity-feed::-webkit-scrollbar-thumb:hover {
  background: #a1a1a1;
}
</style>
