<template>
  <q-page class="login-page row items-center justify-center">
    <div class="auth-card-container">
      <q-card class="auth-card q-pa-lg shadow-24">
        <q-card-section class="hero-section text-center q-pb-lg">
          <div class="logo-circle q-mx-auto q-mb-md">
            <q-icon name="mdi-leaf" size="3.5rem" class="text-white" />
          </div>
          <h1 class="text-h4 text-weight-bolder text-gradient q-mb-xs">KropKontrol</h1>
          <p class="text-subtitle1 text-grey-7">Monitoring IoT</p>
        </q-card-section>

        <q-card-section>
          <q-form @submit.prevent="login" class="q-gutter-md">
            
            <q-banner v-if="authStore.error" dense rounded class="bg-red-1 text-negative q-mb-md border-red">
              <template v-slot:avatar>
                <q-icon name="mdi-alert-circle" color="negative" />
              </template>
              {{ authStore.error }}
            </q-banner>

            <div class="input-wrapper">
              <span class="input-label text-weight-medium text-grey-8">Email</span>
              <q-input
                v-model="email"
                type="email"
                placeholder="exemple@kropkontrol.com"
                outlined
                dense
                class="premium-input"
                bg-color="grey-1"
                :rules="[val => !!val || 'Email requis']"
                hide-bottom-space
              >
                <template v-slot:prepend>
                  <q-icon name="mdi-email-outline" color="primary" />
                </template>
              </q-input>
            </div>

            <div class="input-wrapper q-mt-md">
              <div class="row justify-between items-center q-mb-xs">
                <span class="input-label text-weight-medium text-grey-8">Mot de passe</span>
              </div>
              <q-input
                v-model="password"
                :type="showPassword ? 'text' : 'password'"
                placeholder="••••••••"
                outlined
                dense
                class="premium-input"
                bg-color="grey-1"
                :rules="[val => !!val || 'Mot de passe requis']"
                hide-bottom-space
              >
                 <template v-slot:prepend>
                  <q-icon name="mdi-lock-outline" color="primary" />
                </template>
                <template v-slot:append>
                  <q-icon
                    :name="showPassword ? 'mdi-eye-off' : 'mdi-eye'"
                    class="cursor-pointer text-grey-6"
                    @click="showPassword = !showPassword"
                  />
                </template>
              </q-input>
            </div>

            <q-btn
              type="submit"
              label="Se Connecter"
              class="full-width q-mt-lg submit-btn"
              :loading="authStore.loading"
              unelevated
              size="lg"
            />
          </q-form>
        </q-card-section>

        <q-card-section class="text-center q-pt-none q-mt-sm">
           <a href="https://kropkontrol.com" target="_blank" class="text-caption text-primary text-weight-medium" style="text-decoration: none;">Besoin d'aide ?</a>
        </q-card-section>
      </q-card>
      
      <div class="text-center q-mt-lg text-white text-caption opacity-80">
        &copy; 2024-2026 KropKontrol.com
      </div>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useSettingsStore } from '@/stores/settings'

const router = useRouter()
const authStore = useAuthStore()
const settingsStore = useSettingsStore()

const email = ref('')
const password = ref('')
const showPassword = ref(false)

async function login() {
  settingsStore.hapticFeedback('medium')
  const success = await authStore.loginWithEmail(email.value, password.value)
  if (success) {
    settingsStore.hapticFeedback('light')
    router.push({ name: 'dashboard' })
  } else {
    settingsStore.hapticFeedback('heavy')
  }
}

function isValidEmail(val: string): boolean {
  const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  return emailPattern.test(val)
}
</script>

<style lang="scss" scoped>
.login-page {
  background: linear-gradient(135deg, #6366f1 0%, #8b5cf6 50%, #a855f7 100%);
  position: relative;
  overflow: hidden;

  &::before {
    content: '';
    position: absolute;
    top: -10%;
    left: -10%;
    width: 60%;
    height: 60%;
    background: radial-gradient(circle, rgba(255,255,255,0.15) 0%, rgba(255,255,255,0) 70%);
    border-radius: 50%;
  }
  
  &::after {
    content: '';
    position: absolute;
    bottom: -10%;
    right: -10%;
    width: 40%;
    height: 40%;
    background: radial-gradient(circle, rgba(255,255,255,0.1) 0%, rgba(255,255,255,0) 70%);
    border-radius: 50%;
  }
}

.auth-card-container {
  width: 100%;
  max-width: 440px;
  position: relative;
  z-index: 10;
}

.auth-card {
  border-radius: 24px;
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
}

.logo-circle {
  width: 90px;
  height: 90px;
  background: linear-gradient(45deg, #6366f1, #8b5cf6);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 10px 25px rgba(99, 102, 241, 0.3);
}

.text-gradient {
  background: linear-gradient(45deg, #6366f1, #8b5cf6);
  -webkit-background-clip: text;
  background-clip: text;
  -webkit-text-fill-color: transparent;
}

.premium-input {
  :deep(.q-field__control) {
    border-radius: 12px;
    height: 56px; 
    
    &:before, &:after {
      border-width: 1px !important;
    }
  }

  :deep(.q-field__control:hover:before) {
    border-color: #6366f1 !important;
  }
}

.submit-btn {
  height: 54px;
  border-radius: 14px;
  font-weight: 700;
  font-size: 16px;
  background: linear-gradient(45deg, #6366f1, #8b5cf6) !important;
  box-shadow: 0 8px 20px rgba(99, 102, 241, 0.25);
  transition: transform 0.2s;

  &:active {
    transform: scale(0.98);
  }
}

.border-red {
  border: 1px solid #fcacaa;
}
</style>
