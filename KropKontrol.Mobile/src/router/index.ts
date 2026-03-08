import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: () => import('@/layouts/MainLayout.vue'),
    children: [
      {
        path: '',
        name: 'dashboard',
        component: () => import('@/pages/DashboardPage.vue'),
        meta: { requiresAuth: true }
      },
      {
        path: 'devices',
        name: 'devices',
        component: () => import('@/pages/DevicesPage.vue'),
        meta: { requiresAuth: true }
      },
      {
        path: 'devices/:devEui',
        name: 'device-detail',
        component: () => import('@/pages/DeviceDetailPage.vue'),
        meta: { requiresAuth: true }
      },
      {
        path: 'alerts',
        name: 'alerts',
        component: () => import('@/pages/AlertsPage.vue'),
        meta: { requiresAuth: true }
      },
      {
        path: 'support',
        name: 'support',
        component: () => import('@/pages/SupportPage.vue'),
        meta: { requiresAuth: true }
      },
      {
        path: 'settings',
        name: 'settings',
        component: () => import('@/pages/SettingsPage.vue'),
        meta: { requiresAuth: true }
      },
      {
        path: 'drainage',
        name: 'drainage',
        component: () => import('@/pages/DrainPluviometrePage.vue'),
        meta: { requiresAuth: true }
      }
    ]
  },
  {
    path: '/login',
    component: () => import('@/layouts/AuthLayout.vue'),
    children: [
      {
        path: '',
        name: 'login',
        component: () => import('@/pages/LoginPage.vue')
      }
    ]
  },
  {
    path: '/onboarding',
    redirect: '/login'
  },
  {
    path: '/:catchAll(.*)*',
    redirect: '/'
  }
]

const router = createRouter({
  history: createWebHistory('/pwa/'),
  routes,
  scrollBehavior: () => ({ left: 0, top: 0 })
})

router.beforeEach(async (to, from, next) => {
  const authStore = useAuthStore()

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    if (!authStore.token) {
      next({ name: 'login' })
      return
    }

    const valid = await authStore.validateToken()
    if (!valid) {
      next({ name: 'login' })
      return
    }
  }

  if (to.name === 'login' && authStore.isAuthenticated) {
    next({ name: 'dashboard' })
    return
  }

  next()
})

export default router
