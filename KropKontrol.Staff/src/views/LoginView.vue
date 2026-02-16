<template>
  <div class="login-container">
    <div class="login-card">
      <div class="text-center mb-4">
        <h2 class="fw-bold">KropKontrol</h2>
        <p class="text-muted">Staff Portal</p>
      </div>

      <form @submit.prevent="handleLogin">
        <div class="mb-3">
          <label for="email" class="form-label">Adresse e-mail</label>
          <input
            type="email"
            class="form-control"
            id="email"
            v-model="email"
            required
            placeholder="votre@email.com"
          />
        </div>

        <div class="mb-3">
          <label for="password" class="form-label">Mot de passe</label>
          <input
            type="password"
            class="form-control"
            id="password"
            v-model="password"
            required
            placeholder="••••••••"
          />
        </div>

        <div v-if="errorMessage" class="alert alert-danger" role="alert">
          {{ errorMessage }}
        </div>

        <button type="submit" class="btn btn-primary w-100" :disabled="loading">
          <span v-if="loading" class="spinner-border spinner-border-sm me-2"></span>
          {{ loading ? 'Connexion...' : 'Se connecter' }}
        </button>
      </form>

          </div>
  </div>
</template>

<script>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { authService } from '@/services/api'
import { toast } from 'vue3-toastify'

export default {
  name: 'LoginView',
  setup() {
    const router = useRouter()
    const email = ref('')
    const password = ref('')
    const loading = ref(false)
    const errorMessage = ref('')

    const handleLogin = async () => {
      loading.value = true
      errorMessage.value = ''

      try {
        const data = await authService.login(email.value, password.value)

        if (!data?.token || !data?.refreshToken) {
          throw new Error(data?.message || 'Identifiants invalides')
        }

        toast.success('Connexion réussie')
        router.push('/')
      } catch (error) {
        errorMessage.value = error.message || 'Identifiants incorrects'
      } finally {
        loading.value = false
      }
    }

    return {
      email,
      password,
      loading,
      errorMessage,
      handleLogin
    }
  }
}
</script>

<style scoped>
.login-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.login-card {
  background: white;
  padding: 2.5rem;
  border-radius: 1rem;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
  width: 100%;
  max-width: 400px;
}

.login-card h2 {
  color: #333;
}
</style>
