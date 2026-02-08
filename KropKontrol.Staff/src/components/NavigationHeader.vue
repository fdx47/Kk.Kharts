<template>
  <header class="navigation-header">
    <div class="container-fluid">
      <div class="row align-items-center">
        <div class="col-md-6">
          <h1 class="h3 mb-0">
            <i :class="getPageIcon()" class="text-primary me-2"></i>
            {{ getPageTitle() }}
          </h1>
          <p class="text-muted mb-0">{{ getPageSubtitle() }}</p>
        </div>
        <div class="col-md-6 text-end">
          <div class="d-inline-flex gap-2 align-items-center">
            <!-- Navigation Menu -->
            <div class="nav-menu">
              <div class="btn-group" role="group">
                <button 
                  class="btn btn-sm" 
                  :class="$route.name === 'Dashboard' ? 'btn-primary' : 'btn-outline-primary'"
                  @click="$router.push('/')"
                  title="Dashboard"
                >
                  <i class="bi bi-speedometer2 me-1"></i>
                  Dashboard
                </button>
                <button 
                  class="btn btn-sm" 
                  :class="$route.name === 'Logs' ? 'btn-primary' : 'btn-outline-primary'"
                  @click="$router.push('/logs')"
                  title="Logs"
                >
                  <i class="bi bi-file-text me-1"></i>
                  Logs
                </button>
                <button 
                  class="btn btn-sm" 
                  :class="$route.name === 'VpnProfiles' ? 'btn-primary' : 'btn-outline-primary'"
                  @click="$router.push('/vpn-profiles')"
                  title="VPN Profiles"
                >
                  <i class="bi bi-shield-lock me-1"></i>
                  VPN
                </button>
              </div>
            </div>
            
            <!-- Page-specific controls (slot) -->
            <slot name="controls"></slot>
            
            <!-- Actions -->
            <button class="btn btn-outline-danger btn-sm" @click="handleLogout" title="Déconnexion">
              <i class="bi bi-box-arrow-right me-1"></i>
              Déconnexion
            </button>
          </div>
        </div>
      </div>
    </div>
  </header>
</template>

<script>
import { useRouter } from 'vue-router'
import { authService } from '@/services/api'
import { toast } from 'vue3-toastify'

export default {
  name: 'NavigationHeader',
  setup() {
    const router = useRouter()

    const getPageTitle = () => {
      switch (router.currentRoute.value.name) {
        case 'Dashboard':
          return 'Tableau de Bord Système'
        case 'Logs':
          return 'Journaux Système'
        case 'VpnProfiles':
          return 'Profils VPN'
        default:
          return 'KropKontrol Staff'
      }
    }

    const getPageSubtitle = () => {
      switch (router.currentRoute.value.name) {
        case 'Dashboard':
          return 'Monitoring en temps réel • KropKontrol'
        case 'Logs':
          return 'Visualisation et analyse des logs • KropKontrol'
        case 'VpnProfiles':
          return 'Gestion des profils VPN • KropKontrol'
        default:
          return 'Portail Staff • KropKontrol'
      }
    }

    const getPageIcon = () => {
      switch (router.currentRoute.value.name) {
        case 'Dashboard':
          return 'bi bi-speedometer2'
        case 'Logs':
          return 'bi bi-file-text'
        case 'VpnProfiles':
          return 'bi bi-shield-lock'
        default:
          return 'bi bi-grid'
      }
    }

    const handleLogout = () => {
      authService.logout()
      toast.info('Déconnexion réussie')
      router.push('/login')
    }

    return {
      getPageTitle,
      getPageSubtitle,
      getPageIcon,
      handleLogout
    }
  }
}
</script>

<style scoped>
.navigation-header {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  border-bottom: 1px solid rgba(255, 255, 255, 0.2);
  padding: 1.5rem 0;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
}

.nav-menu {
  display: inline-block;
  margin-right: 1rem;
}

.nav-menu .btn-group .btn {
  min-width: 100px;
  transition: all 0.3s ease;
}

.nav-menu .btn-group .btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
}

/* Mobile responsiveness */
@media (max-width: 768px) {
  .navigation-header .col-md-6 {
    text-align: center !important;
  }
  
  .nav-menu {
    margin-right: 0;
    margin-bottom: 1rem;
  }
  
  .nav-menu .btn-group .btn {
    min-width: 80px;
    font-size: 0.875rem;
  }
  
  .nav-menu .btn-group .btn i {
    display: block;
    margin-bottom: 0.25rem;
  }
}
</style>
