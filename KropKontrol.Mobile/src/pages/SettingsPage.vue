<template>
  <q-page class="settings-page kk-page">
    <div class="settings-container">
      <div class="page-head">
        <h1>Paramètres</h1>
        <span>Personnalise ton expérience KropKontrol</span>
      </div>

      <div class="settings-card user-card">
        <q-avatar size="64px" color="primary" text-color="white">
          {{ userInitials }}
        </q-avatar>
        <div class="user-info">
          <h4>{{ authStore.user?.name || 'Utilisateur' }}</h4>
          <p>{{ authStore.user?.email || '@inconnu' }}</p>
          <span class="auth-mode">
            <q-icon name="mdi-email" size="14px" />
            Connexion email sécurisée
          </span>
        </div>
      </div>

      <div class="section-group">
        <h3>Notifications</h3>
        <div class="settings-card section-card">
          <div class="setting-row">
            <div>
              <strong>Alertes critiques</strong>
              <p>Recevoir les alertes urgentes</p>
            </div>
            <q-toggle
              v-model="alertCritical"
              color="primary"
              @update:model-value="toggleCritical"
            />
          </div>
          <div class="setting-row">
            <div>
              <strong>Alertes batterie</strong>
              <p>Batterie faible des capteurs</p>
            </div>
            <q-toggle
              v-model="alertBattery"
              color="primary"
              @update:model-value="toggleBattery"
            />
          </div>
          <div class="setting-row">
            <div>
              <strong>Capteurs hors ligne</strong>
              <p>Notification si un capteur se déconnecte</p>
            </div>
            <q-toggle
              v-model="alertOffline"
              color="primary"
              @update:model-value="toggleOffline"
            />
          </div>
        </div>
      </div>

      <div class="section-group">
        <h3>Affichage</h3>
        <div class="settings-card section-card">
          <div class="setting-row">
            <div>
              <strong>Unité de température</strong>
              <p>Sélectionne l'unité d'affichage</p>
            </div>
            <q-btn-toggle
              v-model="temperatureUnit"
              glossy
              spread
              toggle-color="primary"
              color="white"
              text-color="primary"
              unelevated
              size="sm"
              class="unit-toggle"
              :options="temperatureOptions"
              @update:model-value="setTemperature"
            />
          </div>
        </div>
      </div>

      <div class="section-group">
        <h3>À propos</h3>
        <div class="settings-card section-card">
          <div class="setting-row static">
            <span>Version</span>
            <strong>{{ appVersion }}</strong>
          </div>
          <div class="setting-row static">
            <span>Plateforme</span>
            <strong>{{ platform }}</strong>
          </div>
        </div>
      </div>

      <q-btn
        unelevated
        color="primary"
        label="Ouvrir KropKontrol Web"
        class="web-btn"
        icon="mdi-open-in-new"
        @click="openWeb"
      />

      <q-btn
        flat
        color="negative"
        icon="mdi-logout"
        :label="$t('auth.logout')"
        class="full-width logout-btn"
        @click="logout"
      />
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useSettingsStore, type TemperatureUnit } from '@/stores/settings'

const router = useRouter()
const authStore = useAuthStore()
const settingsStore = useSettingsStore()

const appVersion = import.meta.env.VITE_APP_VERSION || '1.0.0'
const platform = computed(() => {
  const nav = navigator as Navigator & { userAgentData?: { platform?: string } }
  return nav.userAgentData?.platform || nav.platform || 'unknown'
})

const temperatureOptions = [
  { label: '°C', value: 'c' },
  { label: '°F', value: 'f' }
]

const userInitials = computed(() => {
  const name = authStore.user?.name || 'Utilisateur'
  return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2)
})

const alertCritical = computed({
  get: () => settingsStore.alertCritical,
  set: (val: boolean) => settingsStore.setAlertCritical(val)
})

const alertBattery = computed({
  get: () => settingsStore.alertBattery,
  set: (val: boolean) => settingsStore.setAlertBattery(val)
})

const alertOffline = computed({
  get: () => settingsStore.alertOffline,
  set: (val: boolean) => settingsStore.setAlertOffline(val)
})

const temperatureUnit = computed({
  get: () => settingsStore.temperatureUnit,
  set: (val: TemperatureUnit) => settingsStore.setTemperatureUnit(val)
})

function toggleCritical(val: boolean) {
  settingsStore.hapticFeedback('light')
  settingsStore.setAlertCritical(val)
}

function toggleBattery(val: boolean) {
  settingsStore.hapticFeedback('light')
  settingsStore.setAlertBattery(val)
}

function toggleOffline(val: boolean) {
  settingsStore.hapticFeedback('light')
  settingsStore.setAlertOffline(val)
}

function setTemperature(unit: TemperatureUnit) {
  settingsStore.hapticFeedback('light')
  settingsStore.setTemperatureUnit(unit)
}

function openWeb() {
  settingsStore.hapticFeedback('light')
  window.open('https://kropkontrol.com', '_blank')
}

function logout() {
  settingsStore.hapticFeedback('heavy')
  authStore.logout()
  router.push({ name: 'login' })
}
</script>

<style lang="scss" scoped>
.settings-page {
  background: #f4f6fb;
}

.settings-container {
  max-width: 520px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.page-head {
  text-align: left;

  h1 {
    margin: 0;
    font-size: 28px;
    font-weight: 800;
    color: #0f172a;
  }

  span {
    font-size: 14px;
    color: #94a3b8;
  }
}

.settings-card {
  background: #ffffff;
  border-radius: 20px;
  padding: 20px;
  box-shadow: 0 16px 40px rgba(15, 23, 42, 0.08);
}

.user-card {
  display: flex;
  align-items: center;
  gap: 16px;
}

.user-info h4 {
  margin: 0;
  font-size: 18px;
  font-weight: 700;
  color: #0f172a;
}

.user-info p {
  margin: 4px 0 8px;
  color: #64748b;
  font-size: 14px;
}

.auth-mode {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 12px;
  color: #2563eb;
  background: rgba(37, 99, 235, 0.08);
  padding: 4px 10px;
  border-radius: 999px;
}

.section-group h3 {
  margin: 0 0 8px;
  font-size: 18px;
  color: #0f172a;
}

.section-card {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.setting-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;

  strong {
    display: block;
    font-size: 15px;
    color: #0f172a;
  }

  p {
    margin: 4px 0 0;
    font-size: 13px;
    color: #94a3b8;
  }
}

.setting-row.static {
  span {
    color: #64748b;
    font-size: 14px;
  }

  strong {
    font-size: 14px;
  }
}

.unit-toggle {
  width: 120px;
}

.web-btn {
  width: 100%;
  border-radius: 14px;
  font-weight: 600;
}

.logout-btn {
  margin-bottom: 24px;
}
</style>
