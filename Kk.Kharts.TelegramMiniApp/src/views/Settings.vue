<template>
  <div class="p-4">
    <h1 class="text-2xl font-bold mb-6">Paramètres</h1>

    <!-- User info -->
    <div class="tg-card mb-6">
      <div class="flex items-center gap-4">
        <div class="w-16 h-16 rounded-full bg-tg-button flex items-center justify-center">
          <span class="text-2xl text-tg-button-text font-bold">{{ userInitials }}</span>
        </div>
        <div>
          <div class="font-semibold text-lg">{{ userName }}</div>
          <div class="text-sm text-tg-hint">@{{ username }}</div>
          <div v-if="store.user?.email" class="text-sm text-tg-hint">{{ store.user.email }}</div>
        </div>
      </div>
    </div>

    <!-- Notifications -->
    <div class="mb-6">
      <h2 class="text-lg font-semibold mb-3">Notifications</h2>
      <div class="tg-card space-y-4">
        <div class="flex items-center justify-between">
          <div>
            <div class="font-medium">Alertes critiques</div>
            <div class="text-sm text-tg-hint">Recevoir les alertes urgentes</div>
          </div>
          <label class="relative inline-flex items-center cursor-pointer">
            <input type="checkbox" v-model="settings.criticalAlerts" class="sr-only peer">
            <div class="w-11 h-6 bg-tg-secondary-bg peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-tg-button"></div>
          </label>
        </div>
        
        <div class="flex items-center justify-between">
          <div>
            <div class="font-medium">Alertes batterie</div>
            <div class="text-sm text-tg-hint">Batterie faible des capteurs</div>
          </div>
          <label class="relative inline-flex items-center cursor-pointer">
            <input type="checkbox" v-model="settings.batteryAlerts" class="sr-only peer">
            <div class="w-11 h-6 bg-tg-secondary-bg peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-tg-button"></div>
          </label>
        </div>
        
        <div class="flex items-center justify-between">
          <div>
            <div class="font-medium">Capteurs hors ligne</div>
            <div class="text-sm text-tg-hint">Notification si un capteur se déconnecte</div>
          </div>
          <label class="relative inline-flex items-center cursor-pointer">
            <input type="checkbox" v-model="settings.offlineAlerts" class="sr-only peer">
            <div class="w-11 h-6 bg-tg-secondary-bg peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-tg-button"></div>
          </label>
        </div>
      </div>
    </div>

    <!-- Display -->
    <div class="mb-6">
      <h2 class="text-lg font-semibold mb-3">Affichage</h2>
      <div class="tg-card space-y-4">
        <div class="flex items-center justify-between">
          <div>
            <div class="font-medium">Unité de température</div>
          </div>
          <div class="flex bg-tg-secondary-bg rounded-lg p-1">
            <button 
              class="px-3 py-1 rounded text-sm"
              :class="settings.tempUnit === 'C' ? 'bg-tg-button text-tg-button-text' : ''"
              @click="settings.tempUnit = 'C'">
              °C
            </button>
            <button 
              class="px-3 py-1 rounded text-sm"
              :class="settings.tempUnit === 'F' ? 'bg-tg-button text-tg-button-text' : ''"
              @click="settings.tempUnit = 'F'">
              °F
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- About -->
    <div class="mb-6">
      <h2 class="text-lg font-semibold mb-3">À propos</h2>
      <div class="tg-card space-y-3">
        <div class="flex justify-between text-sm">
          <span class="text-tg-hint">Version</span>
          <span>1.0.0</span>
        </div>
        <div class="flex justify-between text-sm">
          <span class="text-tg-hint">Plateforme</span>
          <span>{{ platform }}</span>
        </div>
      </div>
    </div>

    <!-- Actions -->
    <div class="space-y-3">
      <button class="tg-button-secondary" @click="openWebsite">
        Ouvrir KropKontrol Web
      </button>
      <button class="tg-button-secondary text-kk-danger" @click="logout">
        Délier le compte
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed } from 'vue'
import { useAppStore } from '../stores/app'

const store = useAppStore()

const settings = reactive({
  criticalAlerts: true,
  batteryAlerts: true,
  offlineAlerts: true,
  tempUnit: 'C'
})

const userName = computed(() => {
  return store.user?.email || 'Utilisateur'
})

const username = computed(() => {
  return store.user?.email?.split('@')[0] || ''
})

const userInitials = computed(() => {
  return userName.value.substring(0, 2).toUpperCase()
})

const platform = computed(() => 'Mobile App')

function openWebsite() {
  store.openLink('https://kropkontrol.com')
}

async function logout() {
  const confirmed = await store.showConfirm('Voulez-vous vraiment délier votre compte?')
  if (confirmed) {
    store.showToast('Compte délié')
    localStorage.removeItem('kk_token')
    window.location.reload()
  }
}
</script>
