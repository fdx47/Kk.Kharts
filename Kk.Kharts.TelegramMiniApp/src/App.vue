<template>
  <div class="min-h-screen bg-tg-bg text-tg-text">
    <!-- Header -->
    <header class="sticky top-0 z-50 bg-tg-bg border-b border-tg-secondary-bg px-4 py-3">
      <div class="flex items-center justify-between">
        <div class="flex items-center gap-2">
          <img src="/logo.svg" alt="KropKontrol" class="w-8 h-8" />
          <span class="font-bold text-lg">KropKontrol</span>
        </div>
        <div class="flex items-center gap-3">
          <!-- Alerts badge -->
          <router-link to="/alerts" class="relative">
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
            </svg>
            <span v-if="store.activeAlerts.length > 0" class="alert-badge">
              {{ store.activeAlerts.length }}
            </span>
          </router-link>
          <!-- Settings -->
          <router-link to="/settings">
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
            </svg>
          </router-link>
        </div>
      </div>
    </header>

    <!-- Loading overlay -->
    <div v-if="store.loading" class="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
      <div class="bg-tg-bg rounded-xl p-6 flex flex-col items-center gap-3">
        <div class="w-10 h-10 border-4 border-tg-button border-t-transparent rounded-full animate-spin"></div>
        <span class="text-tg-hint">Chargement...</span>
      </div>
    </div>

    <!-- Main content -->
    <main v-if="$route.path !== '/login'" class="pb-20">
      <router-view v-slot="{ Component }">
        <transition name="fade" mode="out-in">
          <component :is="Component" />
        </transition>
      </router-view>
    </main>

    <!-- Login page (no navigation) -->
    <main v-else>
      <router-view v-slot="{ Component }">
        <transition name="fade" mode="out-in">
          <component :is="Component" />
        </transition>
      </router-view>
    </main>

    <!-- Bottom navigation -->
    <nav v-if="$route.path !== '/login'" class="fixed bottom-0 inset-x-0 bg-tg-bg border-t border-tg-secondary-bg safe-area-bottom">
      <div class="flex justify-around py-2">
        <router-link to="/" class="nav-item" :class="{ active: $route.path === '/' }">
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
              d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
          </svg>
          <span class="text-xs">Accueil</span>
        </router-link>
        
        <router-link to="/devices" class="nav-item" :class="{ active: $route.path.startsWith('/device') }">
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
              d="M9 3v2m6-2v2M9 19v2m6-2v2M5 9H3m2 6H3m18-6h-2m2 6h-2M7 19h10a2 2 0 002-2V7a2 2 0 00-2-2H7a2 2 0 00-2 2v10a2 2 0 002 2zM9 9h6v6H9V9z" />
          </svg>
          <span class="text-xs">Capteurs</span>
        </router-link>
        
        <router-link to="/map" class="nav-item" :class="{ active: $route.path === '/map' }">
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
              d="M9 20l-5.447-2.724A1 1 0 013 16.382V5.618a1 1 0 011.447-.894L9 7m0 13l6-3m-6 3V7m6 10l4.553 2.276A1 1 0 0021 18.382V7.618a1 1 0 00-.553-.894L15 4m0 13V4m0 0L9 7" />
          </svg>
          <span class="text-xs">Carte</span>
        </router-link>
        
        <router-link to="/support" class="nav-item" :class="{ active: $route.path === '/support' }">
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
              d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
          </svg>
          <span class="text-xs">Support</span>
        </router-link>
      </div>
    </nav>

    <!-- Toast notifications -->
    <ToastContainer />
  </div>
</template>

<script setup>
import { onMounted } from 'vue'
import { useAppStore } from './stores/app'
import ToastContainer from './components/ToastContainer.vue'

const store = useAppStore()

onMounted(() => {
  store.initApp()
})
</script>

<style scoped>
.nav-item {
  @apply flex flex-col items-center gap-1 text-tg-hint transition-colors px-4 py-1;
}

.nav-item.active {
  @apply text-tg-button;
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.15s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

.safe-area-bottom {
  padding-bottom: env(safe-area-inset-bottom);
}
</style>
