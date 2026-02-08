<template>
  <div class="min-h-screen flex flex-col items-center justify-center p-4 bg-gradient-to-br from-kk-primary/10 via-tg-bg to-kk-primary/5 relative overflow-hidden">
    
    <!-- Background decorations -->
    <div class="absolute top-0 left-0 w-full h-full overflow-hidden z-0 pointer-events-none">
      <div class="absolute -top-[10%] -left-[10%] w-[50%] h-[50%] bg-kk-primary/20 rounded-full blur-[100px]"></div>
      <div class="absolute top-[20%] -right-[10%] w-[40%] h-[40%] bg-blue-500/20 rounded-full blur-[100px]"></div>
      <div class="absolute -bottom-[10%] left-[20%] w-[60%] h-[60%] bg-kk-primary/10 rounded-full blur-[100px]"></div>
    </div>

    <!-- Login card -->
    <div class="w-full max-w-md relative z-10">
      <!-- Logo and Header -->
      <div class="text-center mb-8">
        <div class="inline-flex items-center justify-center w-20 h-20 rounded-2xl bg-white shadow-xl shadow-kk-primary/20 mb-6 p-4">
          <img src="/logo.svg" alt="KropKontrol" class="w-full h-full object-contain" />
        </div>
        <h1 class="text-3xl font-bold bg-clip-text text-transparent bg-gradient-to-r from-kk-primary to-blue-600 mb-2">
          KropKontrol
        </h1>
        <p class="text-tg-hint text-lg">Votre solution de monitoring IoT</p>
      </div>

      <div class="bg-white/80 backdrop-blur-xl rounded-3xl shadow-2xl p-8 border border-white/50">
        <h2 class="text-2xl font-bold mb-6 text-gray-800">Se connecter</h2>

        <!-- Email input -->
        <div class="mb-5">
          <label class="block text-sm font-semibold text-gray-700 mb-2 ml-1">Email</label>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
              <svg class="h-5 w-5 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 12a4 4 0 10-8 0 4 4 0 008 0zm0 0v1.5a2.5 2.5 0 005 0V12a9 9 0 10-9 9m4.5-1.206a8.959 8.959 0 01-4.5 1.207" />
              </svg>
            </div>
            <input
              v-model="email"
              type="email"
              placeholder="exemple@email.com"
              class="w-full pl-11 pr-4 py-3 rounded-xl bg-gray-50 border border-gray-200 focus:bg-white focus:border-kk-primary focus:ring-4 focus:ring-kk-primary/10 transition-all outline-none text-gray-800 placeholder-gray-400"
              @keyup.enter="handleLogin"
            />
          </div>
        </div>

        <!-- Password input -->
        <div class="mb-6">
          <div class="flex justify-between items-center mb-2 ml-1">
            <label class="block text-sm font-semibold text-gray-700">Mot de passe</label>
          </div>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
              <svg class="h-5 w-5 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
              </svg>
            </div>
            <input
              v-model="password"
              type="password"
              placeholder="••••••••"
              class="w-full pl-11 pr-4 py-3 rounded-xl bg-gray-50 border border-gray-200 focus:bg-white focus:border-kk-primary focus:ring-4 focus:ring-kk-primary/10 transition-all outline-none text-gray-800 placeholder-gray-400"
              @keyup.enter="handleLogin"
            />
          </div>
        </div>

        <!-- Error message -->
        <transition name="fade">
          <div v-if="error" class="mb-6 p-4 bg-red-50 border border-red-100 rounded-xl flex items-start gap-3">
            <svg class="w-5 h-5 text-red-500 shrink-0 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <span class="text-sm text-red-600 font-medium">{{ error }}</span>
          </div>
        </transition>

        <!-- Login button -->
        <button
          class="w-full bg-gradient-to-r from-kk-primary to-green-600 hover:from-green-600 hover:to-kk-primary text-white py-3.5 rounded-xl font-bold text-lg shadow-lg shadow-kk-primary/30 transform transition-all active:scale-[0.98] disabled:opacity-70 disabled:cursor-not-allowed"
          :disabled="loading || !email || !password"
          @click="handleLogin"
        >
          <span v-if="loading" class="flex items-center justify-center gap-2">
            <div class="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin"></div>
            Connexion...
          </span>
          <span v-else>Se connecter</span>
        </button>
      </div>

      <!-- Info message -->
      <div class="mt-8 text-center">
        <a href="https://kropkontrol.com" target="_blank" class="text-sm text-kk-primary font-medium hover:underline opacity-80 decoration-2 underline-offset-4">
          Besoin d'aide ? Contactez le support
        </a>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAppStore } from '../stores/app'
import api from '../services/api'
import { useToast } from '../services/toastService'

const router = useRouter()
const store = useAppStore()
const toast = useToast()

const email = ref('')
const password = ref('')
const loading = ref(false)
const error = ref('')

async function handleLogin() {
  if (!email.value || !password.value) return

  loading.value = true
  error.value = ''

  try {
    const response = await api.post('/auth/login', {
      Email: email.value,
      Password: password.value
    })

    if (response.data.isSuccess && response.data.token) {
      // Update LocalStorage
      localStorage.setItem('kk_token', response.data.token)
      localStorage.setItem('kk_refresh_token', response.data.refreshToken)
      localStorage.setItem('kk_refresh_expiry', response.data.refreshTokenExpiryTime)
      
      // Update Store State directly to prevent race conditions
      store.token = response.data.token
      store.user = { email: email.value }
      store.isLinked = true
      
      toast.success('Connexion réussie!')
      router.push('/')
    } else {
      error.value = response.data.message || 'Erreur de connexion'
    }
  } catch (e) {
    console.error('Login error:', e)
    error.value = e.response?.data?.message || 'Erreur lors de la connexion.'
    toast.error('Erreur de connexion')
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
input:focus {
  box-shadow: 0 0 0 2px rgba(59, 130, 246, 0.1);
}
</style>
