import { createApp } from 'vue'
import { createRouter, createWebHistory } from 'vue-router'
import App from './App.vue'
import { jwtDecode } from 'jwt-decode'
import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap-icons/font/bootstrap-icons.css'
import 'vue3-toastify/dist/index.css'
import 'bootstrap/dist/js/bootstrap.bundle.min.js'
import Vue3Toastify from 'vue3-toastify'
import 'vue3-toastify/dist/index.css'

// Import des vues
import LoginView from './views/LoginView.vue'
import DashboardView from './views/DashboardView.vue'
import VpnProfilesView from './views/VpnProfilesView.vue'
import LogsView from './views/LogsView.vue'
import MiseEnServiceView from './views/MiseEnServiceView.vue'

// Configuration du routeur
const routes = [
  {
    path: '/login',
    name: 'Login',
    component: LoginView,
    meta: { requiresAuth: false }
  },
  {
    path: '/',
    name: 'Dashboard',
    component: DashboardView,
    meta: { requiresAuth: true }
  },
  {
    path: '/vpn-profiles',
    name: 'VpnProfiles',
    component: VpnProfilesView,
    meta: { requiresAuth: true }
  },
  {
    path: '/logs',
    name: 'Logs',
    component: LogsView,
    meta: { requiresAuth: true }
  },
  {
    path: '/mise-en-service',
    name: 'MiseEnService',
    component: MiseEnServiceView,
    meta: { requiresAuth: true }
  }
]

const router = createRouter({
  history: createWebHistory('/staff/'),
  routes
})

// Guard de navigation pour vérifier l'authentification et le rôle Root
router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('authToken')

  if (to.meta.requiresAuth && !token) {
    next({ path: '/login' })
    return
  } else if (to.path === '/login' && token) {
    next({ path: '/' })
    return
  } else if (to.meta.requiresAuth && token) {
    try {
      const decoded = jwtDecode(token)
      const role = decoded.role || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']

      if (role !== 'Root') {
        localStorage.removeItem('authToken')
        localStorage.removeItem('refreshToken')
        next({ path: '/login' })
        return
      }
    } catch (error) {
      localStorage.removeItem('authToken')
      localStorage.removeItem('refreshToken')
      next({ path: '/login' })
      return
    }
    next()
  } else {
    next()
  }
})

const app = createApp(App)
app.use(router)

// Configurar toast para aparecer no canto inferior direito
app.use(Vue3Toastify, {
  position: 'bottom-right',
  timeout: 5000,
  closeOnClick: true,
  pauseOnHover: true,
  draggable: true,
  showCloseButtonOnHover: false,
  hideProgressBar: false,
  closeButton: true,
  icon: true,
  rtl: false
})

// Debug logging - supprimé pour production
if (import.meta.env.DEV) {
  app.config.errorHandler = (err, vm, info) => {
    console.error('Vue Error:', err, info)
  }
}

app.mount('#app')

// Verificação se o app montou corretamente - silencioso
setTimeout(() => {
  const mountedApp = document.getElementById('app')
  if (mountedApp && mountedApp.innerHTML.trim() === '<div id="app"><!----></div>') {
    // App montado mas sem conteúdo - possivelmente erro de roteamento
  }
}, 1000)
