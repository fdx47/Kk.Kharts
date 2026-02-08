<template>
  <q-page class="kk-page">
    <q-pull-to-refresh @refresh="onRefresh">
      <!-- Header -->
      <div class="kk-page__header">
        <h1 class="page-title">{{ $t('alerts.title') }}</h1>
        <p class="page-subtitle">{{ alertsStore.alertCount }} {{ $t('alerts.title').toLowerCase() }} actives</p>
      </div>

      <!-- Mute All Button -->
      <div v-if="alertsStore.activeAlerts.length > 0" class="mute-all-container mb-4">
        <q-btn
          flat
          color="warning"
          icon="mdi-bell-off"
          :label="$t('alerts.muteAll')"
          @click="muteAll"
        />
      </div>

      <!-- Alerts List -->
      <template v-if="alertsStore.loading && alertsStore.alerts.length === 0">
        <div v-for="i in 3" :key="i" class="kk-skeleton kk-skeleton--card mb-3"></div>
      </template>

      <template v-else-if="alertsStore.alerts.length === 0">
        <div class="kk-card text-center py-8">
          <q-icon name="mdi-bell-check" size="64px" class="text-positive mb-4" />
          <h3 class="text-h6 mb-2">Tout va bien !</h3>
          <p class="text-grey-6">{{ $t('alerts.noAlerts') }}</p>
        </div>
      </template>

      <template v-else>
        <q-slide-item
          v-for="alert in alertsStore.alerts"
          :key="alert.id"
          @left="onSwipeLeft(alert)"
          @right="onSwipeRight(alert)"
          left-color="positive"
          right-color="warning"
        >
          <template #left>
            <div class="kk-swipe-action kk-swipe-action--acknowledge">
              <q-icon name="mdi-check" />
              {{ $t('alerts.acknowledge') }}
            </div>
          </template>
          <template #right>
            <div class="kk-swipe-action kk-swipe-action--mute">
              <q-icon name="mdi-bell-off" />
              {{ $t('alerts.mute') }}
            </div>
          </template>

          <div class="kk-alert-item" :class="{ 'alert-inactive': !alert.isActive }">
            <div class="alert-indicator" :class="`alert-indicator--${alert.severity}`"></div>
            <div class="alert-content">
              <div class="alert-header">
                <span class="alert-device">{{ alert.deviceName }}</span>
                <span class="alert-badge" :class="`alert-badge--${alert.severity}`">
                  {{ $t(`alerts.${alert.severity}`) }}
                </span>
              </div>
              <p class="alert-message">{{ alert.message }}</p>
              <div class="alert-footer">
                <span class="alert-time">
                  <q-icon name="mdi-clock-outline" size="12px" />
                  {{ formatRelativeTime(alert.triggeredAt) }}
                </span>
                <span v-if="alert.acknowledgedAt" class="alert-ack">
                  <q-icon name="mdi-check" size="12px" />
                  Acquitté
                </span>
              </div>
            </div>
          </div>
        </q-slide-item>
      </template>
    </q-pull-to-refresh>
  </q-page>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { useAlertsStore, type Alert } from '@/stores/alerts'
import { useSettingsStore } from '@/stores/settings'
import { Notify } from 'quasar'

const alertsStore = useAlertsStore()
const settingsStore = useSettingsStore()

onMounted(async () => {
  await alertsStore.fetchAlerts()
})

async function onRefresh(done: () => void) {
  settingsStore.hapticFeedback('light')
  await alertsStore.fetchAlerts()
  done()
}

async function onSwipeLeft(alert: Alert) {
  settingsStore.hapticFeedback('medium')
  const success = await alertsStore.acknowledgeAlert(alert.id)
  if (success) {
    Notify.create({
      message: 'Alerte acquittée',
      color: 'positive',
      icon: 'mdi-check'
    })
  }
}

async function onSwipeRight(alert: Alert) {
  settingsStore.hapticFeedback('medium')
  const success = await alertsStore.muteAlert(alert.id)
  if (success) {
    Notify.create({
      message: 'Alerte silenciée',
      color: 'warning',
      icon: 'mdi-bell-off'
    })
  }
}

async function muteAll() {
  settingsStore.hapticFeedback('heavy')
  const success = await alertsStore.muteAllAlerts()
  if (success) {
    Notify.create({
      message: 'Toutes les alertes ont été silenciées',
      color: 'warning',
      icon: 'mdi-bell-off'
    })
  }
}

function formatRelativeTime(dateStr: string): string {
  const date = new Date(dateStr)
  const now = new Date()
  const diffMs = now.getTime() - date.getTime()
  const diffMin = Math.floor(diffMs / 60000)
  const diffHour = Math.floor(diffMin / 60)
  const diffDay = Math.floor(diffHour / 24)

  if (diffMin < 60) return `Il y a ${diffMin} min`
  if (diffHour < 24) return `Il y a ${diffHour}h`
  return `Il y a ${diffDay}j`
}
</script>

<style lang="scss" scoped>
.mute-all-container {
  display: flex;
  justify-content: flex-end;
}

.kk-alert-item {
  display: flex;
  gap: 12px;
  background: rgba(255, 255, 255, 0.05);
  border-radius: 16px;
  padding: 16px;
  margin-bottom: 12px;

  &.alert-inactive {
    opacity: 0.5;
  }
}

.alert-indicator {
  width: 4px;
  border-radius: 2px;
  flex-shrink: 0;

  &--critical { background: #ef4444; }
  &--warning { background: #f59e0b; }
  &--info { background: #3b82f6; }
}

.alert-content {
  flex: 1;
  min-width: 0;
}

.alert-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.alert-device {
  font-weight: 700;
  font-size: 14px;
}

.alert-badge {
  font-size: 10px;
  font-weight: 600;
  padding: 4px 8px;
  border-radius: 12px;
  text-transform: uppercase;

  &--critical {
    background: rgba(239, 68, 68, 0.15);
    color: #ef4444;
  }
  &--warning {
    background: rgba(245, 158, 11, 0.15);
    color: #f59e0b;
  }
  &--info {
    background: rgba(59, 130, 246, 0.15);
    color: #3b82f6;
  }
}

.alert-message {
  font-size: 14px;
  opacity: 0.8;
  margin: 0 0 8px 0;
  line-height: 1.4;
}

.alert-footer {
  display: flex;
  gap: 16px;
  font-size: 12px;
  opacity: 0.5;
}

.alert-time, .alert-ack {
  display: flex;
  align-items: center;
  gap: 4px;
}

.mb-2 {
  margin-bottom: 8px;
}

.mb-3 {
  margin-bottom: 12px;
}

.mb-4 {
  margin-bottom: 16px;
}

.py-8 {
  padding-top: 32px;
  padding-bottom: 32px;
}
</style>
