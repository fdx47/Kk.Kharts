import { createApp } from 'vue'
import { createRouter, createWebHistory } from 'vue-router'
import App from './App.vue'
import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap-icons/font/bootstrap-icons.css'
import 'vue3-toastify/dist/index.css'
import 'bootstrap/dist/js/bootstrap.bundle.min.js'
import Vue3Toastify from 'vue3-toastify'
import 'vue3-toastify/dist/index.css'
import { authService } from './services/api'
import { ensureReady as ensureOneSignalReady } from './services/onesignal'

// Import des vues
import LoginView from './views/LoginView.vue'
import DashboardView from './views/DashboardView.vue'
import VpnProfilesView from './views/VpnProfilesView.vue'
import LogsView from './views/LogsView.vue'
import MiseEnServiceView from './views/MiseEnServiceView.vue'
import TwoFactorAuthView from './views/TwoFactorAuthView.vue'
import OneSignalTestView from './views/OneSignalTestView.vue'

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
  },
  {
    path: '/two-factor',
    name: 'TwoFactorAuth',
    component: TwoFactorAuthView,
    meta: { requiresAuth: true }
  },
  {
    path: '/onesignal-test',
    name: 'OneSignalTest',
    component: OneSignalTestView,
    meta: { requiresAuth: true }
  }
]

const router = createRouter({
  history: createWebHistory('/staff/'),
  routes
})

// Guard de navigation (auth stricte + rôle Root)
router.beforeEach((to, from, next) => {
  const isAuth = authService.isAuthenticated()

  if (to.meta.requiresAuth && !isAuth) {
    authService.logout()
    next({ path: '/login' })
    return
  }

  if (to.path === '/login' && isAuth) {
    next({ path: '/' })
    return
  }

  if (to.meta.requiresAuth && isAuth) {
    const role = authService.getUserRole()
    if (role !== 'Root') {
      authService.logout()
      next({ path: '/login' })
      return
    }
  }

  next()
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

// Initialiser OneSignal dès le chargement (ne demande pas la permission ici)
ensureOneSignalReady().catch((err) => {
  // Silencieux en prod; consulter la console pour diagnostics (MIME SW, domaine, permissions)
  console.warn('OneSignal init auto: ' + err)
})

// Verificação se o app montou corretamente - silencioso
setTimeout(() => {
  const mountedApp = document.getElementById('app')
  if (mountedApp && mountedApp.innerHTML.trim() === '<div id="app"><!----></div>') {
    // App montado mas sem conteúdo - possivelmente erro de roteamento
  }
}, 1000)
