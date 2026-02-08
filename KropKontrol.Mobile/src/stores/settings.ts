import { defineStore } from 'pinia'
import { ref } from 'vue'
import { Dark } from 'quasar'

export type Theme = 'dark' | 'light' | 'auto'
export type Language = 'fr' | 'en' | 'pt'
export type TemperatureUnit = 'c' | 'f'

export const useSettingsStore = defineStore('settings', () => {
  const theme = ref<Theme>((localStorage.getItem('kk_theme') as Theme) || 'light')
  const language = ref<Language>((localStorage.getItem('kk_language') as Language) || 'fr')
  const pushEnabled = ref<boolean>(localStorage.getItem('kk_push_enabled') === 'true')
  const hapticEnabled = ref<boolean>(localStorage.getItem('kk_haptic_enabled') !== 'false')
  const alertCritical = ref<boolean>(localStorage.getItem('kk_alert_critical') !== 'false')
  const alertBattery = ref<boolean>(localStorage.getItem('kk_alert_battery') !== 'false')
  const alertOffline = ref<boolean>(localStorage.getItem('kk_alert_offline') !== 'false')
  const temperatureUnit = ref<TemperatureUnit>((localStorage.getItem('kk_temp_unit') as TemperatureUnit) || 'c')

  function setTheme(newTheme: Theme): void {
    theme.value = newTheme
    localStorage.setItem('kk_theme', newTheme)
    applyTheme(newTheme)
  }

  function applyTheme(t: Theme): void {
    if (t === 'auto') {
      Dark.set('auto')
    } else {
      Dark.set(t === 'dark')
    }
  }

  function setLanguage(lang: Language): void {
    language.value = lang
    localStorage.setItem('kk_language', lang)
  }

  function setPushEnabled(enabled: boolean): void {
    pushEnabled.value = enabled
    localStorage.setItem('kk_push_enabled', String(enabled))
  }

  function setHapticEnabled(enabled: boolean): void {
    hapticEnabled.value = enabled
    localStorage.setItem('kk_haptic_enabled', String(enabled))
  }

  function setAlertCritical(enabled: boolean): void {
    alertCritical.value = enabled
    localStorage.setItem('kk_alert_critical', String(enabled))
  }

  function setAlertBattery(enabled: boolean): void {
    alertBattery.value = enabled
    localStorage.setItem('kk_alert_battery', String(enabled))
  }

  function setAlertOffline(enabled: boolean): void {
    alertOffline.value = enabled
    localStorage.setItem('kk_alert_offline', String(enabled))
  }

  function setTemperatureUnit(unit: TemperatureUnit): void {
    temperatureUnit.value = unit
    localStorage.setItem('kk_temp_unit', unit)
  }

  // Centraliza o feedback háptico para que possamos ativar/desativar facilmente conforme a preferência do utilizador
  function hapticFeedback(type: 'light' | 'medium' | 'heavy' = 'light'): void {
    if (!hapticEnabled.value) return

    try {
      const tg = (window as any).Telegram?.WebApp
      tg?.HapticFeedback?.impactOccurred(type)
    } catch {
      // Ignorar qualquer erro causado por indisponibilidade da API do Telegram
    }
  }

  applyTheme(theme.value)

  return {
    theme,
    language,
    pushEnabled,
    hapticEnabled,
    alertCritical,
    alertBattery,
    alertOffline,
    temperatureUnit,
    setTheme,
    applyTheme,
    setLanguage,
    setPushEnabled,
    setHapticEnabled,
    setAlertCritical,
    setAlertBattery,
    setAlertOffline,
    setTemperatureUnit,
    hapticFeedback
  }
})
