<template>
  <q-layout view="lHh Lpr lFf">
    <q-page-container>
      <router-view />
    </q-page-container> 

    <!-- Bottom Navigation -->
    <q-footer class="kk-bottom-nav">
      <div class="nav-items">
        <router-link 
          v-for="item in navItems" 
          :key="item.name"
          :to="item.to" 
          class="nav-item"
          :class="{ active: $route.name === item.name }"
          @click="haptic"
        >
          <q-icon :name="item.icon" class="nav-icon" />
          <span class="nav-label">{{ $t(item.label) }}</span>
        </router-link>
      </div>
    </q-footer>
  </q-layout>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useSettingsStore } from '@/stores/settings'

const router = useRouter()
const settingsStore = useSettingsStore()

const navItems = [
  { name: 'dashboard', to: '/', icon: 'mdi-view-dashboard', label: 'nav.dashboard' },
  { name: 'devices', to: '/devices', icon: 'mdi-access-point', label: 'nav.devices' },
  { name: 'alerts', to: '/alerts', icon: 'mdi-bell-outline', label: 'nav.alerts' },
  { name: 'settings', to: '/settings', icon: 'mdi-cog-outline', label: 'nav.settings' }
]

function haptic() {
  settingsStore.hapticFeedback('light')
}

function goHome() {
  settingsStore.hapticFeedback('light')
  router.push({ name: 'dashboard' })
}

function goAlerts() {
  settingsStore.hapticFeedback('light')
  router.push({ name: 'alerts' })
}

function goSettings() {
  settingsStore.hapticFeedback('light')
  router.push({ name: 'settings' })
}
</script>

<style lang="scss" scoped>
.kk-top-bar {
  background: var(--tg-bg, #ffffff);
  color: var(--tg-text, #0f172a);
  border-bottom: 1px solid var(--tg-border, #e2e6f0);

  .kk-toolbar {
    padding: 8px 16px;
    display: flex;
    align-items: center;
    justify-content: space-between;
  }

  .brand {
    display: flex;
    align-items: center;
    cursor: pointer;
  }

  .brand-text {
    display: flex;
    flex-direction: column;

    .title {
      font-weight: 700;
      font-size: 16px;
    }

    .subtitle {
      font-size: 11px;
      color: var(--tg-hint, #7a8194);
    }
  }

  .toolbar-actions {
    display: flex;
    gap: 6px;
  }

  .toolbar-btn {
    color: var(--tg-text, #0f172a);
  }
}

.kk-bottom-nav {
  background: var(--tg-bg, #ffffff);
  border-top: 1px solid var(--tg-secondary-bg, #f0f0f0);
  padding-bottom: env(safe-area-inset-bottom, 0px);

  .nav-items {
    display: flex;
    justify-content: space-around;
    padding: 8px 0;
  }

  .nav-item {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 4px;
    padding: 8px 16px;
    color: var(--tg-hint, #999999);
    text-decoration: none;
    transition: color 0.2s ease;

    &.active {
      color: var(--tg-button, #2481cc);
    }

    .nav-icon {
      font-size: 24px;
    }

    .nav-label {
      font-size: 10px;
      font-weight: 600;
    }
  }
}

.body--dark .kk-bottom-nav {
  background: rgba(28, 28, 30, 0.95);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border-top: 1px solid rgba(255, 255, 255, 0.08);
}
</style>
