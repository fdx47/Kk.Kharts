<template>
  <NavigationHeader>
    <template #controls>
      <!-- Pas de contrôles supplémentaires pour cette page -->
    </template>
  </NavigationHeader>
  
  <div class="container-fluid py-4">
    <div class="row">
      <div class="col-12">
        <div class="card border-0 shadow-sm">
          <div class="card-header bg-white border-bottom">
            <h5 class="mb-0">
              <i class="bi bi-shield-lock me-2"></i>
              Authentification à Deux Facteurs (2FA)
            </h5>
          </div>
          <div class="card-body">
            <!-- Statut 2FA -->
            <div class="row mb-4">
              <div class="col-md-6">
                <div class="card border" :class="twoFactorStatus.enabled ? 'border-success' : 'border-secondary'">
                  <div class="card-body text-center">
                    <div class="mb-3">
                      <i class="bi" :class="twoFactorStatus.enabled ? 'bi-shield-check text-success' : 'bi-shield-x text-secondary'" style="font-size: 3rem;"></i>
                    </div>
                    <h6 class="card-title">Statut 2FA</h6>
                    <p class="card-text">
                      <span class="badge" :class="twoFactorStatus.enabled ? 'bg-success' : 'bg-secondary'">
                        {{ twoFactorStatus.enabled ? 'Activé' : 'Désactivé' }}
                      </span>
                      <span v-if="twoFactorStatus.required" class="badge bg-warning ms-2">
                        Obligatoire
                      </span>
                    </p>
                    <small v-if="twoFactorStatus.enabledAt" class="text-muted">
                      Activé le {{ formatDate(twoFactorStatus.enabledAt) }}
                    </small>
                  </div>
                </div>
              </div>
              
              <!-- Actions rapides -->
              <div class="col-md-6">
                <div class="card border">
                  <div class="card-body">
                    <h6 class="card-title mb-3">Actions</h6>
                    <div class="d-grid gap-2">
                      <button 
                        v-if="!twoFactorStatus.enabled && !setupInfo.secret"
                        class="btn btn-primary"
                        @click="setupTwoFactor"
                        :disabled="loading"
                      >
                        <i class="bi bi-plus-circle me-2"></i>
                        Activer le 2FA
                      </button>
                      
                      <button 
                        v-if="twoFactorStatus.enabled"
                        class="btn btn-outline-danger"
                        @click="showDisableModal = true"
                        :disabled="loading"
                      >
                        <i class="bi bi-shield-x me-2"></i>
                        Désactiver le 2FA
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Section Setup 2FA -->
            <div v-if="setupInfo.secret" class="row">
              <div class="col-12">
                <div class="card border-primary">
                  <div class="card-header bg-primary text-white">
                    <h6 class="mb-0">
                      <i class="bi bi-qr-code me-2"></i>
                      Configuration du 2FA
                    </h6>
                  </div>
                  <div class="card-body">
                    <div class="row">
                      <div class="col-md-6">
                        <div class="text-center mb-3">
                          <img :src="qrCodeUrl" alt="QR Code" class="img-fluid" style="max-width: 200px;">
                        </div>
                        <p class="text-center text-muted">
                          Scannez ce QR code avec Google Authenticator ou Microsoft Authenticator
                        </p>
                      </div>
                      <div class="col-md-6">
                        <h6>Code manuel</h6>
                        <div class="alert alert-info">
                          <strong>{{ setupInfo.manualEntryKey }}</strong>
                          <button class="btn btn-sm btn-outline-secondary ms-2" @click="copySecret">
                            <i class="bi bi-clipboard"></i>
                          </button>
                        </div>
                        
                        <h6 class="mt-4">Vérification</h6>
                        <div class="input-group mb-3">
                          <input 
                            type="text" 
                            class="form-control" 
                            v-model="verifyCode"
                            placeholder="Code à 6 chiffres"
                            maxlength="6"
                            @input="verifyCode = verifyCode.replace(/\D/g, '')"
                          >
                          <button 
                            class="btn btn-success"
                            @click="enableTwoFactor"
                            :disabled="loading || verifyCode.length !== 6"
                          >
                            <i class="bi bi-check-circle me-2"></i>
                            Activer
                          </button>
                        </div>
                        <small class="text-muted">
                          Entrez le code à 6 chiffres généré par votre application d'authentification
                        </small>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Section Admin (Root only) -->
            <div v-if="userRole === 'Root'" class="row mt-4">
              <div class="col-12">
                <div class="card border-warning">
                  <div class="card-header bg-warning">
                    <h6 class="mb-0">
                      <i class="bi bi-gear me-2"></i>
                      Administration 2FA
                    </h6>
                  </div>
                  <div class="card-body">
                    <div class="row">
                      <div class="col-md-6">
                        <label class="form-label">Sélectionner un utilisateur</label>
                        <select class="form-select" v-model="selectedUserId">
                          <option value="">Choisir un utilisateur...</option>
                          <option v-for="user in users" :key="user.id" :value="user.id">
                            {{ user.nom }} ({{ user.email }})
                          </option>
                        </select>
                      </div>
                      <div class="col-md-6">
                        <label class="form-label">Action</label>
                        <div class="btn-group w-100" role="group">
                          <button 
                            class="btn"
                            :class="getUserTwoFactorStatus(selectedUserId)?.required ? 'btn-warning' : 'btn-outline-warning'"
                            @click="toggleTwoFactorRequirement(selectedUserId, true)"
                            :disabled="!selectedUserId || loading"
                          >
                            <i class="bi bi-shield-exclamation me-2"></i>
                            Rendre obligatoire
                          </button>
                          <button 
                            class="btn"
                            :class="getUserTwoFactorStatus(selectedUserId)?.required ? 'btn-outline-secondary' : 'btn-secondary'"
                            @click="toggleTwoFactorRequirement(selectedUserId, false)"
                            :disabled="!selectedUserId || loading"
                          >
                            <i class="bi bi-shield me-2"></i>
                            Rendre optionnel
                          </button>
                        </div>
                      </div>
                    </div>
                    
                    <!-- Statut utilisateur sélectionné -->
                    <div v-if="selectedUserId" class="mt-3">
                      <div class="alert alert-info">
                        <strong>Statut 2FA de {{ getUser(selectedUserId)?.nom }}:</strong>
                        <span class="badge ms-2" :class="getUserTwoFactorStatus(selectedUserId)?.enabled ? 'bg-success' : 'bg-secondary'">
                          {{ getUserTwoFactorStatus(selectedUserId)?.enabled ? 'Activé' : 'Désactivé' }}
                        </span>
                        <span v-if="getUserTwoFactorStatus(selectedUserId)?.required" class="badge bg-warning ms-2">
                          Obligatoire
                        </span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal de confirmation désactivation -->
    <div class="modal fade" :class="{ show: showDisableModal }" :style="{ display: showDisableModal ? 'block' : 'none' }">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Désactiver le 2FA</h5>
            <button type="button" class="btn-close" @click="showDisableModal = false"></button>
          </div>
          <div class="modal-body">
            <p>Êtes-vous sûr de vouloir désactiver l'authentification à deux facteurs ?</p>
            <div class="mb-3">
              <label class="form-label">Code de vérification requis</label>
              <input 
                type="text" 
                class="form-control" 
                v-model="disableCode"
                placeholder="Code à 6 chiffres"
                maxlength="6"
                @input="disableCode = disableCode.replace(/\D/g, '')"
              >
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="showDisableModal = false">Annuler</button>
            <button 
              type="button" 
              class="btn btn-danger"
              @click="disableTwoFactor"
              :disabled="loading || disableCode.length !== 6"
            >
              <i class="bi bi-shield-x me-2"></i>
              Désactiver
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted, computed } from 'vue'
import api from '../services/api'
import { toast } from 'vue3-toastify'
import { jwtDecode } from 'jwt-decode'
import NavigationHeader from '../components/NavigationHeader.vue'

export default {
  name: 'TwoFactorAuthView',
  components: {
    NavigationHeader
  },
  setup() {
    const loading = ref(false)
    const twoFactorStatus = ref({
      enabled: false,
      required: false,
      enabledAt: null
    })
    const setupInfo = ref({
      secret: '',
      qrCodeUri: '',
      manualEntryKey: ''
    })
    const verifyCode = ref('')
    const disableCode = ref('')
    const showDisableModal = ref(false)
    const users = ref([])
    const selectedUserId = ref('')
    const userTwoFactorStatuses = ref({})

    const userRole = computed(() => {
      const token = localStorage.getItem('authToken')
      if (!token) return null
      try {
        const decoded = jwtDecode(token)
        return decoded.role || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
      } catch {
        return null
      }
    })

    const qrCodeUrl = computed(() => {
      if (!setupInfo.value.qrCodeUri) return ''
      // Utiliser un service QR code externe ou générer localement
      return `https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=${encodeURIComponent(setupInfo.value.qrCodeUri)}`
    })

    const formatDate = (dateString) => {
      if (!dateString) return ''
      return new Date(dateString).toLocaleDateString('fr-FR', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      })
    }

    const getUser = (userId) => {
      return users.value.find(u => u.id === userId)
    }

    const getUserTwoFactorStatus = (userId) => {
      return userTwoFactorStatuses.value[userId]
    }

    const loadStatus = async () => {
      try {
        loading.value = true
        const response = await api.get('/api/v1/2fa/status')
        twoFactorStatus.value = response.data
      } catch (error) {
        toast.error('Erreur lors du chargement du statut 2FA')
        console.error('Error loading 2FA status:', error)
      } finally {
        loading.value = false
      }
    }

    const setupTwoFactor = async () => {
      try {
        loading.value = true
        const response = await api.post('/api/v1/2fa/setup')
        setupInfo.value = response.data
        toast.success('Configuration 2FA initialisée. Scannez le QR code.')
      } catch (error) {
        toast.error('Erreur lors de l\'initialisation du 2FA')
        console.error('Error setting up 2FA:', error)
      } finally {
        loading.value = false
      }
    }

    const enableTwoFactor = async () => {
      try {
        loading.value = true
        await api.post('/api/v1/2fa/enable', { code: verifyCode.value })
        setupInfo.value = { secret: '', qrCodeUri: '', manualEntryKey: '' }
        verifyCode.value = ''
        await loadStatus()
        toast.success('Authentification à deux facteurs activée avec succès!')
      } catch (error) {
        toast.error('Code invalide. Veuillez réessayer.')
        console.error('Error enabling 2FA:', error)
      } finally {
        loading.value = false
      }
    }

    const disableTwoFactor = async () => {
      try {
        loading.value = true
        await api.post('/api/v1/2fa/disable', { code: disableCode.value })
        showDisableModal.value = false
        disableCode.value = ''
        await loadStatus()
        toast.success('Authentification à deux facteurs désactivée.')
      } catch (error) {
        toast.error('Code invalide ou erreur lors de la désactivation.')
        console.error('Error disabling 2FA:', error)
      } finally {
        loading.value = false
      }
    }

    const loadUsers = async () => {
      if (userRole.value !== 'Root') return
      try {
        const response = await api.get('/api/v1/users')
        users.value = response.data
      } catch (error) {
        console.error('Error loading users:', error)
      }
    }

    const loadUserTwoFactorStatuses = async () => {
      if (userRole.value !== 'Root') return
      try {
        for (const user of users.value) {
          try {
            const response = await api.get(`/api/v1/2fa/status/${user.id}`)
            userTwoFactorStatuses.value[user.id] = response.data
          } catch (error) {
            console.error(`Error loading 2FA status for user ${user.id}:`, error)
          }
        }
      } catch (error) {
        console.error('Error loading user 2FA statuses:', error)
      }
    }

    const toggleTwoFactorRequirement = async (userId, required) => {
      try {
        loading.value = true
        await api.post('/api/v1/2fa/require', { userId, required })
        await loadUserTwoFactorStatuses()
        toast.success(`Le 2FA est maintenant ${required ? 'obligatoire' : 'optionnel'} pour cet utilisateur.`)
      } catch (error) {
        toast.error('Erreur lors de la modification du statut 2FA.')
        console.error('Error toggling 2FA requirement:', error)
      } finally {
        loading.value = false
      }
    }

    const copySecret = () => {
      navigator.clipboard.writeText(setupInfo.value.secret)
        .then(() => toast.success('Secret copié dans le presse-papiers'))
        .catch(() => toast.error('Erreur lors de la copie du secret'))
    }

    onMounted(async () => {
      await loadStatus()
      if (userRole.value === 'Root') {
        await loadUsers()
        await loadUserTwoFactorStatuses()
      }
    })

    return {
      loading,
      twoFactorStatus,
      setupInfo,
      verifyCode,
      disableCode,
      showDisableModal,
      users,
      selectedUserId,
      userTwoFactorStatuses,
      userRole,
      qrCodeUrl,
      formatDate,
      getUser,
      getUserTwoFactorStatus,
      setupTwoFactor,
      enableTwoFactor,
      disableTwoFactor,
      toggleTwoFactorRequirement,
      copySecret
    }
  }
}
</script>

<style scoped>
.card {
  transition: transform 0.2s ease-in-out;
}

.card:hover {
  transform: translateY(-2px);
}

.modal.show {
  background-color: rgba(0, 0, 0, 0.5);
}

.modal.fade {
  transition: opacity 0.15s linear;
}

.btn-group .btn {
  flex: 1;
}

.input-group .form-control {
  font-family: 'Courier New', monospace;
  letter-spacing: 0.1em;
}

.badge {
  font-size: 0.8em;
}
</style>
