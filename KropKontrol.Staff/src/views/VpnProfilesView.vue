<template>
  <div class="vpn-page">
    <!-- Navigation Header -->
    <NavigationHeader>
      <template #controls>
        <!-- VPN specific controls can go here -->
        <button class="btn btn-primary btn-sm" @click="refreshVpnProfiles" :disabled="loading">
          <i class="bi bi-arrow-clockwise me-1" :class="{ 'spin': loading }"></i>
          Actualiser
        </button>
      </template>
    </NavigationHeader>

    <!-- Main Content -->
    <main class="vpn-main">
      <div class="container-fluid">
        <div class="row">
          <div class="col-md-3">
            <div class="card">
              <div class="card-body">
                <h5 class="card-title">Navigation</h5>
                <div class="list-group list-group-flush">
                  <router-link to="/" class="list-group-item list-group-item-action">
                    <i class="bi bi-speedometer2 me-2"></i>
                  Tableau de bord
                </router-link>
                <router-link
                  to="/vpn-profiles"
                  class="list-group-item list-group-item-action ms-3"
                  :class="{ active: $route.path === '/vpn-profiles' }"
                >
                  <i class="bi bi-shield-lock me-2"></i>
                  Profils VPN
                </router-link>
                <router-link
                  to="/logs"
                  class="list-group-item list-group-item-action ms-3"
                  :class="{ active: $route.path === '/logs' }"
                >
                  <i class="bi bi-file-text me-2"></i>
                  Logs
                </router-link>
              </div>
            </div>
          </div>
        </div>

        <div class="col-md-9">
          <div class="card">
            <div class="card-header d-flex justify-content-between align-items-center">
              <h5 class="mb-0">
                <i class="bi bi-shield-lock me-2"></i>
                Gestion des Profils VPN
              </h5>
            </div>

            <div class="card-body">
              <!-- Filtres -->
              <div class="row mb-3">
                <div class="col-md-4">
                  <input
                    type="text"
                    class="form-control"
                    placeholder="Rechercher..."
                    v-model="searchQuery"
                  />
                </div>
                <div class="col-md-3">
                  <select class="form-select" v-model="filterType">
                    <option value="">Tous les types</option>
                    <option value="ug65">UG65 Gateway</option>
                    <option value="pc">PC Remote Access</option>
                  </select>
                </div>
                <div class="col-md-3">
                  <select class="form-select" v-model="filterStatus">
                    <option value="">Tous les statuts</option>
                    <option value="assigned">Attribués</option>
                    <option value="available">Disponibles</option>
                  </select>
                </div>
              </div>

              <!-- Tableau -->
              <div class="table-responsive">
                <table class="table table-hover">
                  <thead>
                    <tr>
                      <th>Type</th>
                      <th>Nom Commun</th>
                      <th>IP VPN</th>
                      <th>Utilisateur</th>
                      <th>Localisation</th>
                      <th>Statut</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-if="loading">
                      <td colspan="7" class="text-center">
                        <div class="spinner-border" role="status">
                          <span class="visually-hidden">Chargement...</span>
                        </div>
                      </td>
                    </tr>
                    <tr v-else-if="filteredProfiles.length === 0">
                      <td colspan="7" class="text-center text-muted">Aucun profil trouvé</td>
                    </tr>
                    <tr v-else v-for="profile in paginatedProfiles" :key="profile.id">
                      <td>
                        <span 
                          class="badge" 
                          :class="{
                            'bg-purple': profile.type === 'ug65',
                            'bg-success': profile.type === 'ug67',
                            'bg-info': profile.type === 'ug63',
                            'bg-dark': profile.type === 'ug56',
                            'bg-secondary': profile.type === 'usg50',
                            'bg-danger': profile.type === 'pc'
                          }"
                        >
                          {{ profile.type.toUpperCase() }}
                        </span>
                      </td>
                      <td>{{ profile.commonName }}</td>
                      <td>
                        <code>{{ profile.vpnIp }}</code>
                      </td>
                      <td>
                        <template v-if="profile.assignedCompanyName">
                          <span class="text-primary">
                            <i class="bi bi-building me-1"></i>
                            {{ profile.assignedCompanyName }}
                          </span>
                        </template>
                        <template v-else-if="profile.assignedUserName">
                          <span class="text-success">
                            <i class="bi bi-person-check me-1"></i>
                            {{ profile.assignedUserName }}
                          </span>
                        </template>
                        <span v-else class="text-muted">Non attribué</span>
                      </td>
                      <td>
                        <small>{{ profile.installationLocation || '-' }}</small>
                      </td>
                      <td>
                        <span
                          v-if="profile.isActive"
                          class="badge bg-success"
                        >
                          Actif
                        </span>
                        <span v-else class="badge bg-secondary">Inactif</span>
                      </td>
                      <td>
                        <div class="btn-group">
                          <button
                            class="btn btn-outline-success btn-sm"
                            @click="openAssignModal(profile)"
                            title="Attribuer"
                          >
                            <i class="bi bi-person-plus"></i>
                          </button>
                          <button
                            class="btn btn-outline-primary btn-sm"
                            @click="openEditModal(profile)"
                            title="Modifier"
                          >
                            <i class="bi bi-pencil"></i>
                          </button>
                          <button
                            v-if="profile.ovpnFileName"
                            class="btn btn-outline-info btn-sm"
                            @click="downloadOvpn(profile.id)"
                            title="Télécharger OVPN"
                          >
                            <i class="bi bi-download"></i>
                          </button>
                          <div class="btn-group" v-if="profile.ovpnFileName">
                          <button
                            class="btn btn-outline-secondary btn-sm dropdown-toggle"
                            type="button"
                            data-bs-toggle="dropdown"
                            aria-expanded="false"
                            title="Plus d'actions"
                          >
                            <i class="bi bi-three-dots"></i>
                          </button>
                          <ul class="dropdown-menu">
                            <li>
                              <a class="dropdown-item" href="#" @click="openUploadModal(profile)">
                                <i class="bi bi-upload me-2"></i>Upload OVPN
                              </a>
                            </li>
                            <li><hr class="dropdown-divider"></li>
                            <li>
                              <a class="dropdown-item text-danger" href="#" @click="confirmDelete(profile)">
                                <i class="bi bi-trash me-2"></i>Supprimer
                              </a>
                            </li>
                          </ul>
                        </div>
                        <div class="btn-group" v-else>
                          <button
                            class="btn btn-outline-secondary btn-sm dropdown-toggle"
                            type="button"
                            data-bs-toggle="dropdown"
                            aria-expanded="false"
                            title="Plus d'actions"
                          >
                            <i class="bi bi-three-dots"></i>
                          </button>
                          <ul class="dropdown-menu">
                            <li>
                              <a class="dropdown-item" href="#" @click="openUploadModal(profile)">
                                <i class="bi bi-upload me-2"></i>Upload OVPN
                              </a>
                            </li>
                            <li><hr class="dropdown-divider"></li>
                            <li>
                              <a class="dropdown-item text-danger" href="#" @click="confirmDelete(profile)">
                                <i class="bi bi-trash me-2"></i>Supprimer
                              </a>
                            </li>
                          </ul>
                        </div>
                        </div>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>

              <!-- Pagination -->
              <nav v-if="totalPages > 1">
                <ul class="pagination justify-content-center">
                  <li class="page-item" :class="{ disabled: currentPage === 1 }">
                    <a class="page-link" @click="currentPage--">Précédent</a>
                  </li>
                  <li
                    class="page-item"
                    v-for="page in totalPages"
                    :key="page"
                    :class="{ active: currentPage === page }"
                  >
                    <a class="page-link" @click="currentPage = page">{{ page }}</a>
                  </li>
                  <li class="page-item" :class="{ disabled: currentPage === totalPages }">
                    <a class="page-link" @click="currentPage++">Suivant</a>
                  </li>
                </ul>
              </nav>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal Créer/Modifier -->
    <div
      class="modal fade"
      :class="{ show: showEditModal }"
      :style="{ display: showEditModal ? 'block' : 'none' }"
      tabindex="-1"
    >
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">
              {{ editingProfile ? 'Modifier le Profil' : 'Nouveau Profil' }}
            </h5>
            <button type="button" class="btn-close" @click="closeEditModal"></button>
          </div>
          <div class="modal-body">
            <form @submit.prevent="saveProfile">
              <div class="mb-3">
                <label class="form-label">Type</label>
                <select
                  class="form-select"
                  v-model="formData.type"
                  required
                >
                  <option value="ug65">UG65 Gateway</option>
                  <option value="ug67">UG67 Gateway</option>
                  <option value="ug63">UG63 Gateway</option>
                  <option value="ug56">UG56 Gateway</option>
                  <option value="usg50">USG50 Gateway</option>
                  <option value="pc">PC Remote Access</option>
                </select>
              </div>
              <div class="mb-3">
                <label class="form-label">Nom Commun</label>
                <input
                  type="text"
                  class="form-control"
                  v-model="formData.commonName"
                  required
                />
              </div>
              <div class="mb-3">
                <label class="form-label">IP VPN</label>
                <input
                  type="text"
                  class="form-control"
                  v-model="formData.vpnIp"
                  :disabled="editingProfile"
                  required
                  placeholder="10.8.0.x"
                />
              </div>
              <div class="mb-3">
                <label class="form-label">Notes</label>
                <textarea
                  class="form-control"
                  v-model="formData.notes"
                  rows="3"
                ></textarea>
              </div>
              <div class="mb-3">
                <label class="form-label">Localisation d'installation</label>
                <input type="text" class="form-control" v-model="formData.installationLocation" />
              </div>
              <!-- Campo de atribuição baseado no tipo -->
              <div class="mb-3" v-if="!editingProfile">
                <label class="form-label">Attribution</label>
                <div v-if="formData.type === 'ug65'">
                  <select class="form-select" v-model="formData.companyId">
                    <option value="">Sélectionner une entreprise</option>
                    <option v-for="company in companies" :key="company.id" :value="company.id">
                      {{ company.name }}
                    </option>
                  </select>
                  <small class="text-muted">Les profils UG65 sont attribués aux entreprises (gateways)</small>
                </div>
                <div v-else-if="formData.type === 'pc'">
                  <select class="form-select" v-model="formData.userId">
                    <option value="">Sélectionner un utilisateur</option>
                    <option v-for="user in users.filter(u => u.role === 'Root')" :key="user.id" :value="user.id">
                      {{ user.nom }} ({{ user.email }})
                    </option>
                  </select>
                  <small class="text-muted">Les profils PC sont attribués aux utilisateurs Root uniquement</small>
                </div>
              </div>
              
              <div class="mb-3" v-if="editingProfile">
                <div class="form-check">
                  <input
                    class="form-check-input"
                    type="checkbox"
                    v-model="formData.isActive"
                    id="isActive"
                  />
                  <label class="form-check-label" for="isActive"> Actif </label>
                </div>
              </div>
            </form>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="closeEditModal">
              Annuler
            </button>
            <button type="button" class="btn btn-primary" @click="saveProfile">
              Enregistrer
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal Attribuer -->
    <div
      class="modal fade"
      :class="{ show: showAssignModal }"
      :style="{ display: showAssignModal ? 'block' : 'none' }"
      tabindex="-1"
    >
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Attribuer le Profil VPN</h5>
            <button type="button" class="btn-close" @click="closeAssignModal"></button>
          </div>
          <div class="modal-body">
            <div class="mb-3">
              <label class="form-label">Profil</label>
              <input
                type="text"
                class="form-control"
                :value="selectedProfile?.commonName"
                disabled
              />
            </div>
            <div class="mb-3" v-if="selectedProfile?.type === 'ug65' || selectedProfile?.type === 'ug67' || selectedProfile?.type === 'ug63' || selectedProfile?.type === 'ug56' || selectedProfile?.type === 'usg50'">
              <label class="form-label">Entreprise</label>
              <select class="form-select" v-model="assignData.companyId">
                <option value="">Sélectionner une entreprise...</option>
                <option v-for="company in companies" :key="company.id" :value="company.id">
                  {{ company.name }}
                </option>
              </select>
            </div>
            <div class="mb-3" v-else>
              <label class="form-label">Utilisateur (Root)</label>
              <select class="form-select" v-model="assignData.userId">
                <option value="">Sélectionner un utilisateur...</option>
                <option
                  v-for="user in users.filter(u => u.role === 'Root')"
                  :key="user.id"
                  :value="user.id"
                >
                  {{ user.nom }} ({{ user.email }})
                </option>
              </select>
            </div>
            <div class="mb-3">
              <label class="form-label">Localisation d'installation</label>
              <input type="text" class="form-control" v-model="assignData.installationLocation" />
            </div>
            <div class="alert alert-info" v-if="selectedProfile?.assignedCompanyName || selectedProfile?.assignedUserName">
              <i class="bi bi-info-circle me-2"></i>
              Actuellement attribué à :
              <strong>
                {{ selectedProfile?.assignedCompanyName || selectedProfile?.assignedUserName }}
              </strong>
            </div>
          </div>
          <div class="modal-footer">
            <button
              type="button"
              class="btn btn-warning"
              @click="unassignProfile"
              v-if="selectedProfile?.assignedUserId || selectedProfile?.assignedCompanyId"
            >
              Désattribuer
            </button>
            <button type="button" class="btn btn-secondary" @click="closeAssignModal">
              Annuler
            </button>
            <button type="button" class="btn btn-primary" @click="assignProfile">
              Attribuer
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal Upload OVPN -->
    <div
      class="modal fade"
      :class="{ show: showUploadModal }"
      :style="{ display: showUploadModal ? 'block' : 'none' }"
      tabindex="-1"
    >
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Upload Fichier OVPN</h5>
            <button type="button" class="btn-close" @click="closeUploadModal"></button>
          </div>
          <div class="modal-body">
            <div class="mb-3">
              <label class="form-label">Profil</label>
              <input
                type="text"
                class="form-control"
                :value="selectedProfile?.commonName"
                disabled
              />
            </div>
            <div class="mb-3">
              <label class="form-label">Fichier .ovpn</label>
              <input
                type="file"
                class="form-control"
                accept=".ovpn"
                @change="handleFileSelect"
                ref="fileInput"
              />
            </div>
            <div class="alert alert-success" v-if="selectedProfile?.ovpnFileName">
              <i class="bi bi-check-circle me-2"></i>
              Fichier actuel : <strong>{{ selectedProfile.ovpnFileName }}</strong>
            </div>
          </div>
          <div class="modal-footer">
            <button
              type="button"
              class="btn btn-primary"
              @click="uploadOvpnFile"
              :disabled="!selectedFile"
            >
              Upload
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal Import CSV -->
    <div
      class="modal fade"
      :class="{ show: showImportModal }"
      :style="{ display: showImportModal ? 'block' : 'none' }"
      tabindex="-1"
    >
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Importer depuis CSV</h5>
            <button type="button" class="btn-close" @click="closeImportModal"></button>
          </div>
          <div class="modal-body">
            <div class="mb-3">
              <label class="form-label">Fichier CSV</label>
              <input
                type="file"
                class="form-control"
                accept=".csv"
                @change="handleCsvSelect"
                ref="csvInput"
              />
            </div>
            <div class="alert alert-info">
              <i class="bi bi-info-circle me-2"></i>
              Format attendu : type, cn, vpn_ip, notes, created_at
            </div>
          </div>
          <div class="modal-footer">
            <button
              type="button"
              class="btn btn-success"
              @click="importCsv"
              :disabled="!selectedCsvFile"
            >
              Importer
            </button>
          </div>
        </div>
      </div>
    </div>

    </main>

    <!-- Backdrop -->
    <div
      class="modal-backdrop fade"
      :class="{ show: showEditModal || showAssignModal || showUploadModal || showImportModal }"
      v-if="showEditModal || showAssignModal || showUploadModal || showImportModal"
    ></div>
  </div>
</template>

<script>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { vpnProfileService, userService, companyService } from '@/services/api'
import { toast } from 'vue3-toastify'
import NavigationHeader from '@/components/NavigationHeader.vue'

export default {
  name: 'VpnProfilesView',
  components: {
    NavigationHeader
  },
  setup() {
    const router = useRouter()
    const profiles = ref([])
    const users = ref([])
    const companies = ref([])
    const loading = ref(false)
    const searchQuery = ref('')
    const filterType = ref('')
    const filterStatus = ref('')
    const currentPage = ref(1)
    const itemsPerPage = 10

    // Modals
    const showEditModal = ref(false)
    const showAssignModal = ref(false)
    const showUploadModal = ref(false)
    const showImportModal = ref(false)

    // Form data
    const editingProfile = ref(null)
    const selectedProfile = ref(null)
    const selectedFile = ref(null)
    const selectedCsvFile = ref(null)
    const formData = ref({
      type: 'ug65',
      commonName: '',
      vpnIp: '',
      notes: '',
      installationLocation: '',
      companyId: '',
      userId: '',
      isActive: true
    })
    const assignData = ref({
      userId: '',
      companyId: '',
      installationLocation: ''
    })

    // Computed
    const filteredProfiles = computed(() => {
      let result = profiles.value

      if (searchQuery.value) {
        const query = searchQuery.value.toLowerCase()
        result = result.filter(
          (p) =>
            p.commonName.toLowerCase().includes(query) ||
            p.vpnIp.toLowerCase().includes(query) ||
            p.assignedUserName?.toLowerCase().includes(query) ||
            p.assignedCompanyName?.toLowerCase().includes(query)
        )
      }

      if (filterType.value) {
        result = result.filter((p) => p.type === filterType.value)
      }

      if (filterStatus.value === 'assigned') {
        result = result.filter((p) => p.assignedUserId || p.assignedCompanyId)
      } else if (filterStatus.value === 'available') {
        result = result.filter((p) => !p.assignedUserId && !p.assignedCompanyId)
      }

      return result
    })

    const totalPages = computed(() => Math.ceil(filteredProfiles.value.length / itemsPerPage))

    const paginatedProfiles = computed(() => {
      const start = (currentPage.value - 1) * itemsPerPage
      const end = start + itemsPerPage
      return filteredProfiles.value.slice(start, end)
    })

    // Methods
    const loadProfiles = async () => {
      loading.value = true
      try {
        profiles.value = await vpnProfileService.getAll()
      } catch (error) {
        console.error('Erreur lors du chargement des profils:', error)
        toast.error('Erreur lors du chargement des profils')
      } finally {
        loading.value = false
      }
    }

    const loadUsers = async () => {
      try {
        users.value = await userService.getAll()
      } catch (error) {
        console.error('Erreur lors du chargement des utilisateurs:', error)
      }
    }

    const loadCompanies = async () => {
      try {
        companies.value = await companyService.getAll()
      } catch (error) {
        console.error('Erreur lors du chargement des entreprises:', error)
      }
    }

    const openCreateModal = () => {
      editingProfile.value = null
      formData.value = {
        type: 'ug65',
        commonName: '',
        vpnIp: '',
        notes: '',
        installationLocation: '',
        companyId: '',
        userId: '',
        isActive: true
      }
      showEditModal.value = true
    }

    const openEditModal = (profile) => {
      editingProfile.value = profile
      formData.value = {
        type: profile.type,
        commonName: profile.commonName,
        vpnIp: profile.vpnIp,
        notes: profile.notes || '',
        installationLocation: profile.installationLocation || '',
        isActive: profile.isActive
      }
      showEditModal.value = true
    }

    const closeEditModal = () => {
      showEditModal.value = false
      editingProfile.value = null
    }

    const saveProfile = async () => {
      try {
        if (editingProfile.value) {
          await vpnProfileService.update(editingProfile.value.id, {
            type: formData.value.type,
            commonName: formData.value.commonName,
            notes: formData.value.notes,
            installationLocation: formData.value.installationLocation,
            isActive: formData.value.isActive
          })
          toast.success('Profil mis à jour avec succès')
        } else {
          await vpnProfileService.create(formData.value)
          toast.success('Profil créé avec succès')
        }
        closeEditModal()
        loadProfiles()
      } catch (error) {
        console.error('Erreur lors de la sauvegarde:', error)
        toast.error('Erreur lors de la sauvegarde du profil')
      }
    }

    
    const openAssignModal = (profile) => {
      selectedProfile.value = profile
      assignData.value = {
        userId: profile.assignedUserId || '',
        companyId: profile.assignedCompanyId || '',
        installationLocation: profile.installationLocation || ''
      }
      showAssignModal.value = true
      loadUsers()
      loadCompanies()
    }

    const closeAssignModal = () => {
      showAssignModal.value = false
      selectedProfile.value = null
    }

    const assignProfile = async () => {
      const payload = {
        installationLocation: assignData.value.installationLocation
      }

      if (selectedProfile.value.type === 'ug65' || selectedProfile.value.type === 'ug67' || selectedProfile.value.type === 'ug63' || selectedProfile.value.type === 'ug56' || selectedProfile.value.type === 'usg50') {
        if (!assignData.value.companyId) {
          toast.error('Veuillez sélectionner une entreprise')
          return
        }
        payload.companyId = assignData.value.companyId
      } else {
        if (!assignData.value.userId) {
          toast.error('Veuillez sélectionner un utilisateur Root')
          return
        }
        payload.userId = assignData.value.userId
      }

      try {
        await vpnProfileService.assign(selectedProfile.value.id, payload)
        toast.success('Profil attribué avec succès')
        closeAssignModal()
        loadProfiles()
      } catch (error) {
        console.error('Erreur lors de l\'attribution:', error)
        toast.error('Erreur lors de l\'attribution du profil')
      }
    }

    const unassignProfile = async () => {
      try {
        await vpnProfileService.unassign(selectedProfile.value.id)
        toast.success('Profil désattribué avec succès')
        closeAssignModal()
        loadProfiles()
      } catch (error) {
        console.error('Erreur lors de la désattribution:', error)
        toast.error('Erreur lors de la désattribution du profil')
      }
    }

    const openUploadModal = (profile) => {
      console.log('Opening upload modal for:', profile.commonName)
      selectedProfile.value = profile
      selectedFile.value = null
      showUploadModal.value = true
      console.log('showUploadModal:', showUploadModal.value)
    }

    const closeUploadModal = () => {
      showUploadModal.value = false
      selectedProfile.value = null
      selectedFile.value = null
    }

    const handleFileSelect = (event) => {
      selectedFile.value = event.target.files[0]
    }

    const uploadOvpnFile = async () => {
      if (!selectedFile.value) {
        toast.error('Veuillez sélectionner un fichier')
        return
      }

      try {
        await vpnProfileService.uploadOvpnFile(selectedProfile.value.id, selectedFile.value)
        toast.success('Fichier OVPN uploadé avec succès')
        closeUploadModal()
        loadProfiles()
      } catch (error) {
        console.error('Erreur lors de l\'upload:', error)
        toast.error('Erreur lors de l\'upload du fichier')
      }
    }

    
    const downloadOvpn = async (profileId) => {
      try {
        const blob = await vpnProfileService.downloadOvpnFile(profileId)
        const profile = profiles.value.find((p) => p.id === profileId)
        const url = window.URL.createObjectURL(blob)
        const a = document.createElement('a')
        a.href = url
        a.download = profile.ovpnFileName || `${profile.commonName}.ovpn`
        document.body.appendChild(a)
        a.click()
        window.URL.revokeObjectURL(url)
        document.body.removeChild(a)
        toast.success('Fichier téléchargé')
      } catch (error) {
        console.error('Erreur lors du téléchargement:', error)
        toast.error('Erreur lors du téléchargement du fichier')
      }
    }

    const confirmDelete = async (profile) => {
      if (confirm(`Êtes-vous sûr de vouloir supprimer le profil "${profile.commonName}" ?`)) {
        try {
          await vpnProfileService.delete(profile.id)
          toast.success('Profil supprimé avec succès')
          loadProfiles()
        } catch (error) {
          console.error('Erreur lors de la suppression:', error)
          toast.error('Erreur lors de la suppression du profil')
        }
      }
    }

    const closeImportModal = () => {
      showImportModal.value = false
      selectedCsvFile.value = null
    }

    const handleCsvSelect = (event) => {
      selectedCsvFile.value = event.target.files[0]
    }

    const importCsv = async () => {
      if (!selectedCsvFile.value) {
        toast.error('Veuillez sélectionner un fichier CSV')
        return
      }

      try {
        const result = await vpnProfileService.importFromCsv(selectedCsvFile.value)
        toast.success(`${result.importedCount} profils importés sur ${result.totalLines}`)
        closeImportModal()
        loadProfiles()
      } catch (error) {
        console.error('Erreur lors de l\'import:', error)
        toast.error('Erreur lors de l\'import du fichier CSV')
      }
    }

    const refreshVpnProfiles = async () => {
      await loadProfiles()
      toast.success('Profils VPN actualisés')
    }

    onMounted(() => {
      loadProfiles()
      loadUsers()
      loadCompanies()
    })

    return {
      profiles,
      users,
      companies,
      loading,
      searchQuery,
      filterType,
      filterStatus,
      currentPage,
      filteredProfiles,
      totalPages,
      paginatedProfiles,
      showEditModal,
      showAssignModal,
      showUploadModal,
      showImportModal,
      editingProfile,
      selectedProfile,
      selectedFile,
      selectedCsvFile,
      formData,
      assignData,
      openCreateModal,
      openEditModal,
      closeEditModal,
      saveProfile,
      confirmDelete,
      openAssignModal,
      closeAssignModal,
      assignProfile,
      unassignProfile,
      openUploadModal,
      closeUploadModal,
      handleFileSelect,
      uploadOvpnFile,
      downloadOvpn,
      closeImportModal,
      handleCsvSelect,
      importCsv,
      refreshVpnProfiles
    }
  }
}
</script>

<style scoped>
.vpn-page {
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.vpn-main {
  padding: 2rem 0;
}

.card {
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  border: none;
  margin-bottom: 1rem;
}

.list-group-item.active {
  background-color: #667eea;
  border-color: #667eea;
}

.modal.show {
  display: block;
  background-color: rgba(0, 0, 0, 0.5);
}

.modal-backdrop.show {
  opacity: 0.5;
}

.page-link {
  cursor: pointer;
}

code {
  background-color: #f8f9fa;
  padding: 0.2rem 0.4rem;
  border-radius: 0.25rem;
  font-size: 0.875rem;
}

.bg-purple {
  background-color: #6f42c1 !important;
}
</style>
