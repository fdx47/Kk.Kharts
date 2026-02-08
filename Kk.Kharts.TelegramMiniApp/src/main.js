import { createApp } from 'vue'
import { createPinia } from 'pinia'
import router from './router'
import App from './App.vue'
import './style.css'

// Initialize Telegram WebApp
const tg = window.Telegram?.WebApp

if (tg) {
  tg.ready()
  tg.expand()
  tg.enableClosingConfirmation()
}

const app = createApp(App)
app.use(createPinia())
app.use(router)

// Make Telegram WebApp available globally
app.config.globalProperties.$tg = tg

app.mount('#app')
