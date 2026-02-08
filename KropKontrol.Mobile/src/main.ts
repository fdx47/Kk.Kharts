import { createApp } from 'vue'
import { Quasar, Notify, Dialog, Loading, LocalStorage, SessionStorage, BottomSheet, AppFullscreen } from 'quasar'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'
import i18n from './i18n'
import { useAuthStore } from './stores/auth'

import '@quasar/extras/roboto-font/roboto-font.css'
import '@quasar/extras/material-icons/material-icons.css'
import '@quasar/extras/mdi-v7/mdi-v7.css'
import 'quasar/src/css/index.sass'
import './css/app.scss'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)
app.use(i18n)
app.use(Quasar, {
  plugins: {
    Notify,
    Dialog,
    Loading,
    LocalStorage,
    SessionStorage,
    BottomSheet,
    AppFullscreen
  },
  config: {
    dark: false,
    brand: {
      primary: '#22c55e',
      secondary: '#3b82f6',
      accent: '#8b5cf6',
      dark: '#1C1C1E',
      positive: '#22c55e',
      negative: '#ef4444',
      info: '#3b82f6',
      warning: '#f59e0b'
    }
  }
})

// Inicializar a autenticação antes da montagem
const authStore = useAuthStore()
authStore.initAuth().then(() => {
  app.mount('#q-app')
})
