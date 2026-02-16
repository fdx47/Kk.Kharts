<template>
  <header class="navigation-header">
    <div class="container-fluid">
      <div class="header-layout">
        <div class="header-top">
          <div class="title-block">
            <div class="page-icon">
              <i :class="getPageIcon()"></i>
            </div>
            <div class="title-text">
              <h1 class="h3 mb-1">{{ getPageTitle() }}</h1>
              <p class="text-muted mb-1">{{ getPageSubtitle() }}</p>             
            </div>
          </div>
        </div>

        <div class="header-toolbar">
          <div class="nav-menu">
            <button 
              class="nav-chip"
              :class="{ active: $route.name === 'Dashboard' }"
              @click="$router.push('/')"
              title="Dashboard"
            >
              <i class="bi bi-speedometer2"></i>
              Dashboard
            </button>
            <button 
              class="nav-chip"
              :class="{ active: $route.name === 'Logs' }"
              @click="$router.push('/logs')"
              title="Logs"
            >
              <i class="bi bi-file-text"></i>
              Logs
            </button>
            <button 
              class="nav-chip"
              :class="{ active: $route.name === 'VpnProfiles' }"
              @click="$router.push('/vpn-profiles')"
              title="VPN Profiles"
            >
              <i class="bi bi-shield-lock"></i>
              VPN
            </button>
            <button
              class="nav-chip"
              :class="{ active: $route.name === 'MiseEnService' }"
              @click="$router.push('/mise-en-service')"
              title="Mise en service"
            >
              <i class="bi bi-rocket-takeoff"></i>
              Mise en service
            </button>
          </div>

          <div class="toolbar-controls">
            <slot name="controls"></slot>
            <button class="btn btn-outline-danger btn-sm logout-button" @click="handleLogout" title="Déconnexion">
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
        case 'MiseEnService':
          return 'Atelier de Mise en Service'
        default:
          return 'KropKontrol Staff'
      }
    }

    const getPageSubtitle = () => {
      switch (router.currentRoute.value.name) {
        case 'Dashboard':
          return 'KropKontrol • Monitoring en temps réel'
        case 'Logs':
          return 'KropKontrol • Visualisation et analyse des logs'
        case 'VpnProfiles':
          return 'KropKontrol • Gestion des profils VPN'
        case 'MiseEnService':
          return 'KropKontrol • Mise en service des Capteurs'
        default:
          return 'KropKontrol • Portail Staff'
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
        case 'MiseEnService':
          return 'bi bi-rocket-takeoff'
        default:
          return 'bi bi-grid'
      }
    }

    const handleLogout = () => {
      authService.logout()
      toast.info('Déconnexion réussie')
      router.push('/login')
    }

    const lastUpdatedLabel = new Intl.DateTimeFormat('fr-FR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    }).format(new Date())

    return {
      getPageTitle,
      getPageSubtitle,
      getPageIcon,
      handleLogout,
      lastUpdatedLabel
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

.navigation-header .container-fluid {
  max-width: 1280px;
  margin: 0 auto;
  padding: 0 1.5rem;
}

.header-layout {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.header-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1.5rem;
  margin-bottom: 1.5rem;
}

.title-block {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.page-icon {
  width: 48px;
  height: 48px;
  border-radius: 16px;
  background: linear-gradient(135deg, #8ec5fc, #e0c3fc);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.35rem;
  color: #3b3c54;
}

.header-meta {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  text-align: right;
  gap: 0.15rem;
  font-size: 0.85rem;
  color: #94a3b8;
}

.badge-pill {
  align-self: flex-end;
  font-size: 0.75rem;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: #475569;
  background: #edf2ff;
  padding: 0.2rem 0.75rem;
  border-radius: 999px;
  border: 1px solid #c7d2fe;
}


.header-toolbar {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
}

.nav-menu {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  row-gap: 0.6rem;
}

.nav-chip {
  border: none;
  border-radius: 999px;
  padding: 0.5rem 1.2rem;
  background: rgba(99, 102, 241, 0.08);
  color: #4c4f6b;
  font-weight: 600;
  display: inline-flex;
  align-items: center;
  gap: 0.45rem;
  transition: all 0.25s ease;
}

.nav-chip i {
  font-size: 1rem;
}

.nav-chip:hover {
  transform: translateY(-1px);
  box-shadow: 0 10px 25px rgba(79, 70, 229, 0.15);
}

.nav-chip.active {
  background: linear-gradient(135deg, #4338ca, #6366f1);
  color: #fff;
  box-shadow: 0 10px 30px rgba(79, 70, 229, 0.35);
}

.toolbar-controls {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  flex-wrap: wrap;
}

.toolbar-controls :deep(.btn) {
  border-radius: 999px;
  font-weight: 600;
  padding: 0.4rem 1.35rem;
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  box-shadow: 0 10px 20px rgba(15, 23, 42, 0.12);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.toolbar-controls :deep(.btn:hover) {
  transform: translateY(-2px);
  box-shadow: 0 15px 30px rgba(15, 23, 42, 0.18);
}

.toolbar-controls :deep(.btn-primary) {
  border: none;
  background: linear-gradient(135deg, #2563eb, #7c3aed);
}

.toolbar-controls :deep(.btn-primary:disabled) {
  opacity: 0.75;
  box-shadow: none;
  transform: none;
}

.toolbar-controls :deep(.btn-outline-danger) {
  border: 2px solid rgba(248, 113, 113, 0.8);
  color: #b91c1c;
  background-color: rgba(248, 113, 113, 0.08);
}

.toolbar-controls :deep(.btn-outline-danger:hover) {
  background: linear-gradient(135deg, #f87171, #ef4444);
  color: #fff;
  border-color: transparent;
}

.logout-button {
  border-width: 2px;
  border-radius: 999px;
  padding-inline: 1.25rem;
  font-weight: 600;
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  transition: background 0.2s, color 0.2s, border-color 0.2s;
}

.logout-button:hover {
  background: #dc3545;
  color: #fff;
}

/* Mobile responsiveness */
@media (max-width: 768px) {
  .header-top {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.75rem;
  }

  .header-meta {
    align-items: flex-start;
    text-align: left;
  }

  .header-toolbar {
    flex-direction: column;
    align-items: flex-start;
    width: 100%;
  }

  .nav-menu {
    width: 100%;
    justify-content: flex-start;
  }

  .nav-chip {
    flex: 1 1 auto;
    min-width: 110px;
    justify-content: center;
  }

  .toolbar-controls {
    width: 100%;
    flex-direction: column;
    align-items: stretch;
    gap: 0.5rem;
  }

  .logout-button {
    width: 100%;
    justify-content: center;
  }
}

</style>
