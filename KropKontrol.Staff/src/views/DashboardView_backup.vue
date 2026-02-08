<template>
  <div class="dashboard">
    <nav class="navbar navbar-dark bg-dark">
      <div class="container-fluid">
        <span class="navbar-brand mb-0 h1">
          <i class="bi bi-grid-3x3-gap me-2"></i>
          KropKontrol - Staff Portal
        </span>
        <div class="d-flex align-items-center">
          <span class="text-white me-3">
            <i class="bi bi-person-circle me-1"></i>
            Root
          </span>
          <button class="btn btn-outline-light btn-sm" @click="handleLogout">
            <i class="bi bi-box-arrow-right me-1"></i>
            Déconnexion
          </button>
        </div>
      </div>
    </nav>

    <div class="container-fluid mt-4">
      <div class="row">
        <div class="col-md-3">
          <div class="card">
            <div class="card-body">
              <h5 class="card-title">
                <i class="bi bi-compass me-2"></i>
                Navigation
              </h5>
              <div class="list-group list-group-flush">
                <router-link
                  to="/"
                  class="list-group-item list-group-item-action"
                  :class="{ active: $route.path === '/' }"
                >
                  <i class="bi bi-speedometer2 me-2"></i>
                  Tableau de bord
                </router-link>
                
                <!-- Gestion Réseau -->
                <div class="list-group-item">
                  <small class="text-muted text-uppercase">Réseau & Sécurité</small>
                </div>
                <router-link
                  to="/vpn-profiles"
                  class="list-group-item list-group-item-action ms-3"
                  :class="{ active: $route.path === '/vpn-profiles' }"
                >
                  <i class="bi bi-shield-lock me-2"></i>
                  Profils VPN
                </router-link>
              </div>
            </div>
          </div>
        </div>

        <div class="col-md-9">
          <!-- Header Dashboard -->
          <div class="d-flex justify-content-between align-items-center mb-4">
            <h2>
              <i class="bi bi-speedometer2 me-2"></i>
              Tableau de Bord
            </h2>
            <div class="text-muted">
              <i class="bi bi-clock me-1"></i>
              Dernière mise à jour: {{ lastUpdate }}
            </div>
          </div>

          <!-- Cartes Statistiques -->
          <div class="row mb-4">
            <!-- Réseau & Sécurité -->
            <div class="col-lg-4 col-md-6 mb-3">
              <div class="card border-primary h-100">
                <div class="card-header bg-primary text-white">
                  <i class="bi bi-shield-lock me-2"></i>
                  Réseau & Sécurité
                </div>
                <div class="card-body">
                  <div class="row text-center">
                    <div class="col-6">
                      <h4 class="text-primary">{{ stats.totalProfiles }}</h4>
                      <small>Profils VPN</small>
                    </div>
                    <div class="col-6">
                      <h4 class="text-success">{{ stats.assignedProfiles }}</h4>
                      <small>Attribués</small>
                    </div>
                  </div>
                  <div class="progress mt-2" style="height: 6px;">
                    <div class="progress-bar bg-success" :style="{width: vpnUsagePercentage + '%'}"></div>
                  </div>
                  <small class="text-muted">Taux d'utilisation: {{ vpnUsagePercentage }}%</small>
                </div>
                <div class="card-footer">
                  <router-link to="/vpn-profiles" class="btn btn-sm btn-outline-primary">
                    <i class="bi bi-arrow-right me-1"></i>
                    Gérer VPN
                  </router-link>
                </div>
              </div>
            </div>

            <!-- Appareils IoT -->
            <div class="col-lg-4 col-md-6 mb-3">
              <div class="card border-success h-100">
                <div class="card-header bg-success text-white">
                  <i class="bi bi-cpu me-2"></i>
                  Devices
                </div>
                <div class="card-body">
                  <div v-if="loading" class="text-center py-3">
                    <div class="spinner-border spinner-border-sm text-success" role="status">
                      <span class="visually-hidden">Chargement...</span>
                    </div>
                  </div>
                  <div v-else>
                    <div class="row text-center">
                      <div class="col-6">
                        <h4 class="text-success">{{ deviceStats.total }}</h4>
                        <small>Total</small>
                      </div>
                      <div class="col-6">
                        <h4 class="text-info">{{ deviceStats.online }}</h4>
                        <small>En ligne</small>
                      </div>
                    </div>
                    <div class="progress mt-2" style="height: 6px;">
                      <div class="progress-bar bg-info" :style="{width: deviceStats.onlinePercentage + '%'}"></div>
                    </div>
                    <small class="text-muted">Taux de connexion: {{ deviceStats.onlinePercentage }}%</small>
                  </div>
                </div>
            </div>

            <!-- Utilisateurs -->
            <div class="col-lg-4 col-md-6 mb-3">
              <div class="card border-info h-100">
                <div class="card-header bg-info text-white">
                  <i class="bi bi-people me-2"></i>
                  Utilisateurs
                </div>
                <div class="card-body">
                  <div v-if="loading" class="text-center py-3">
                    <div class="spinner-border spinner-border-sm text-info" role="status">
                      <span class="visually-hidden">Chargement...</span>
                    </div>
                  </div>
                  <div v-else>
                    <div class="text-center">
                      <h4 class="text-info">{{ userStats.total }}</h4>
                      <small>Total</small>
                    </div>
                  </div>
                </div>
          </div>

          <!-- Activité Récente -->
          <div class="row">
            <div class="col-md-8">
              <div class="card">
                <div class="card-header">
                  <i class="bi bi-activity me-2"></i>
                  Activité Récente
                </div>
                <div class="card-body">
                  <div v-if="loading" class="text-center py-3">
                    <div class="spinner-border spinner-border-sm text-primary" role="status">
                      <span class="visually-hidden">Chargement...</span>
                    </div>
                  </div>
                  <div v-else-if="recentActivities.length === 0" class="text-center py-3">
                    <small class="text-muted">Aucune activité récente</small>
                  </div>
                  <div v-else class="timeline">
                    <div 
                      v-for="activity in recentActivities" 
                      :key="activity.id"
                      class="timeline-item"
                    >
                      <div 
                        class="timeline-marker"
                        :class="{
                          'bg-success': activity.type === 'vpn_profile',
                          'bg-info': activity.type === 'device',
                          'bg-warning': activity.type === 'alarm',
                          'bg-primary': activity.type === 'user'
                        }"
                      ></div>
                      <div class="timeline-content">
                        <small class="text-muted">{{ formatTimeAgo(activity.CreatedAt) }}</small>
                        <div>{{ activity.Description }}: {{ activity.EntityName }}</div>
                        <div v-if="activity.Details" class="text-muted small">{{ activity.Details }}</div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div class="col-md-4">
              <div class="card">
                <div class="card-header">
                  <i class="bi bi-gear me-2"></i>
                  Actions Rapides
                </div>
                <div class="card-body">
                  <div class="d-grid gap-2">
                    <router-link to="/vpn-profiles" class="btn btn-primary">
                      <i class="bi bi-plus-circle me-2"></i>
                      Nouveau Profil VPN
                    </router-link>
                    <button class="btn btn-outline-secondary" disabled>
                      <i class="bi bi-download me-2"></i>
                      Exporter Données
                    </button>
                    <button class="btn btn-outline-secondary" disabled>
                      <i class="bi bi-arrow-repeat me-2"></i>
                      Synchroniser Appareils
                    </button>
                    <button class="btn btn-outline-secondary" disabled>
                      <i class="bi bi-file-earmark-text me-2"></i>
                      Générer Rapport
                    </button>
                  </div>
                </div>
              </div>

              <!-- Système Status -->
              <div class="card mt-3">
                <div class="card-header">
                  <i class="bi bi-server me-2"></i>
                  Status Système
                </div>
                <div class="card-body">
                  <div v-if="loading" class="text-center py-3">
                    <div class="spinner-border spinner-border-sm text-primary" role="status">
                      <span class="visually-hidden">Chargement...</span>
                    </div>
                  </div>
                  <div v-else>
                    <div class="d-flex justify-content-between align-items-center mb-2">
                      <span>API</span>
                      <span class="badge" :class="systemStatus.apiOnline ? 'bg-success' : 'bg-danger'">
                        {{ systemStatus.apiOnline ? 'En ligne' : 'Hors ligne' }}
                      </span>
                    </div>
                    <div class="d-flex justify-content-between align-items-center mb-2">
                      <span>Base de données</span>
                      <span class="badge" :class="systemStatus.databaseConnected ? 'bg-success' : 'bg-danger'">
                        {{ systemStatus.databaseConnected ? 'Connectée' : 'Déconnectée' }}
                      </span>
                    </div>
                    <div class="d-flex justify-content-between align-items-center mb-2">
                      <span>Services</span>
                      <span class="badge" :class="systemStatus.servicesActive ? 'bg-success' : 'bg-warning'">
                        {{ systemStatus.servicesActive ? 'Actifs' : 'Inactifs' }}
                      </span>
                    </div>
                    <div class="d-flex justify-content-between align-items-center mb-2">
                      <span>Espace disque</span>
                      <span class="badge" :class="systemStatus.diskUsagePercentage > 80 ? 'bg-danger' : systemStatus.diskUsagePercentage > 60 ? 'bg-warning' : 'bg-success'">
                        {{ Math.round(systemStatus.diskUsagePercentage) }}%
                      </span>
                    </div>
                    <div class="d-flex justify-content-between align-items-center">
                      <span>Dernière vérification</span>
                      <span class="badge bg-info">{{ formatTimeAgo(systemStatus.lastCheck) }}</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { authService, vpnProfileService, deviceService, userStatsService, activityService, systemStatusService } from '@/services/api'
import { toast } from 'vue3-toastify'

export default {
  name: 'DashboardView',
  setup() {
    const router = useRouter()
    const lastUpdate = ref('')
    const loading = ref(false)
    
    // Stats VPN
    const stats = ref({
      totalProfiles: 0,
      assignedProfiles: 0,
      availableProfiles: 0
    })
    
    // Stats Devices
    const deviceStats = ref({
      total: 0,
      online: 0,
      offline: 0,
      onlinePercentage: 0
    })
    
    // Stats Users
    const userStats = ref({
      total: 0,
      active: 0,
      inactive: 0,
      activePercentage: 0
    })
    
    // Recent Activities
    const recentActivities = ref([])
    
    // System Status
    const systemStatus = ref({
      apiOnline: false,
      databaseConnected: false,
      servicesActive: false,
      diskUsagePercentage: 0,
      lastCheck: new Date(),
      apiVersion: '',
      environment: '',
      totalDevices: 0,
      onlineDevices: 0,
      totalUsers: 0,
      activeUsers: 0,
      totalVpnProfiles: 0,
      assignedVpnProfiles: 0
    })

    // Computed para taxa de uso VPN
    const vpnUsagePercentage = computed(() => {
      if (stats.value.totalProfiles === 0) return 0
      return Math.round((stats.value.assignedProfiles / stats.value.totalProfiles) * 100)
    })

    const loadStats = async () => {
      loading.value = true
      try {
        // Carregar stats VPN
        try {
          const profiles = await vpnProfileService.getAll()
                    stats.value.totalProfiles = profiles.length
          stats.value.assignedProfiles = profiles.filter((p) => p.assignedUserId || p.assignedCompanyId).length
          stats.value.availableProfiles = profiles.filter((p) => !p.assignedUserId && !p.assignedCompanyId).length
        } catch (error) {
          console.error('Erreur VPN stats:', error)
        }
        
        // Carregar stats Devices (dados reais)
        try {
          deviceStats.value = await deviceService.getStats()
        } catch (error) {
          console.error('Erreur Device stats:', error)
        }
        
        // Carregar stats Users (dados reais)
        try {
          userStats.value = await userStatsService.getStats()
        } catch (error) {
          console.error('Erreur User stats:', error)
        }
        
        // Carregar atividades recentes (dados reais)
        try {
          recentActivities.value = await activityService.getRecent(6)
        } catch (error) {
          console.error('Erreur activités récentes:', error)
          // Fallback para dados mock se a API falhar
          recentActivities.value = []
        }
        
        // Carregar status do sistema (dados reais)
        try {
          systemStatus.value = await systemStatusService.getStatus()
        } catch (error) {
          console.error('Erreur status système:', error)
          // Fallback para valores padrão se a API falhar
          systemStatus.value = {
            apiOnline: true, // Se estamos aqui, API está online
            databaseConnected: false,
            servicesActive: false,
            diskUsagePercentage: 0,
            lastCheck: new Date(),
            apiVersion: '',
            environment: '',
            totalDevices: 0,
            onlineDevices: 0,
            totalUsers: 0,
            activeUsers: 0,
            totalVpnProfiles: 0,
            assignedVpnProfiles: 0
          }
        }
        
      } catch (error) {
        console.error('Erreur lors du chargement des statistiques:', error)
      } finally {
        loading.value = false
      }
    }

    const updateLastUpdateTime = () => {
      const now = new Date()
      lastUpdate.value = now.toLocaleTimeString('fr-FR', {
        hour: '2-digit',
        minute: '2-digit'
      })
    }

    const handleLogout = () => {
      authService.logout()
      toast.info('Déconnexion réussie')
      router.push('/login')
    }

    const formatTimeAgo = (date) => {
      const now = new Date()
      const diff = now - new Date(date)
      const minutes = Math.floor(diff / 60000)
      const hours = Math.floor(diff / 3600000)
      const days = Math.floor(diff / 86400000)
      
      if (days > 0) return `Il y a ${days} jour${days > 1 ? 's' : ''}`
      if (hours > 0) return `Il y a ${hours} heure${hours > 1 ? 's' : ''}`
      if (minutes > 0) return `Il y a ${minutes} minute${minutes > 1 ? 's' : ''}`
      return "À l'instant"
    }

    onMounted(() => {
      loadStats()
      updateLastUpdateTime()
      
      // Atualizar timestamp a cada minuto
      const interval = setInterval(updateLastUpdateTime, 60000)
      
      // Cleanup
      return () => clearInterval(interval)
    })

    return {
      stats,
      deviceStats,
      userStats,
      recentActivities,
      systemStatus,
      lastUpdate,
      vpnUsagePercentage,
      loading,
      handleLogout,
      formatTimeAgo
    }
  }
}
</script>

<style scoped>
.dashboard {
  min-height: 100vh;
  background-color: #f8f9fa;
}

.card {
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  border: none;
  margin-bottom: 1rem;
  transition: transform 0.2s ease-in-out;
}

.card:hover {
  transform: translateY(-2px);
}

.list-group-item.active {
  background-color: #667eea;
  border-color: #667eea;
}

.list-group-item:hover:not(.active) {
  background-color: #f8f9fa;
}

.list-group-item.disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.list-group-item.disabled:hover {
  background-color: transparent;
}

/* Timeline */
.timeline {
  position: relative;
  padding-left: 30px;
}

.timeline::before {
  content: '';
  position: absolute;
  left: 8px;
  top: 0;
  bottom: 0;
  width: 2px;
  background-color: #e9ecef;
}

.timeline-item {
  position: relative;
  padding-bottom: 20px;
}

.timeline-marker {
  position: absolute;
  left: -22px;
  top: 0;
  width: 16px;
  height: 16px;
  border-radius: 50%;
  border: 2px solid #fff;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.timeline-content {
  padding-left: 10px;
}

.timeline-content small {
  display: block;
  margin-bottom: 4px;
}

/* Progress bars */
.progress {
  background-color: #e9ecef;
}

.progress-bar {
  transition: width 0.3s ease;
}

/* Badges */
.badge {
  font-size: 0.75em;
}

/* Buttons */
.btn {
  transition: all 0.2s ease-in-out;
}

.btn:hover {
  transform: translateY(-1px);
}

/* Cards de estatísticas */
.card.border-primary:hover {
  border-color: #4dabf7 !important;
}

.card.border-success:hover {
  border-color: #51cf66 !important;
}

.card.border-info:hover {
  border-color: #74c0fc !important;
}

/* Header */
.navbar-brand {
  font-weight: 600;
}

/* Responsive */
@media (max-width: 768px) {
  .col-md-3 {
    margin-bottom: 20px;
  }
  
  .timeline {
    padding-left: 20px;
  }
  
  .timeline::before {
    left: 6px;
  }
  
  .timeline-marker {
    left: -18px;
    width: 12px;
    height: 12px;
  }
}

/* Animações */
@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.card {
  animation: fadeIn 0.3s ease-out;
}

/* Status indicators */
.badge.bg-success {
  background-color: #51cf66 !important;
}

.badge.bg-warning {
  background-color: #ffd43b !important;
  color: #000 !important;
}

.badge.bg-info {
  background-color: #74c0fc !important;
}

/* Text utilities */
.text-muted {
  color: #6c757d !important;
}

/* Card headers */
.card-header {
  background-color: rgba(0, 0, 0, 0.03);
  border-bottom: 1px solid rgba(0, 0, 0, 0.125);
  font-weight: 600;
}
</style>
