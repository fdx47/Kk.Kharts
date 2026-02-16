<template>
  <div class="mise-en-service-page">
    <NavigationHeader>
      <template #controls>
        <button class="btn btn-primary btn-sm" @click="rechargerDonnees" :disabled="chargementGeneral">
          <i class="bi bi-arrow-clockwise me-1" :class="{ spin: chargementGeneral }"></i>
          Actualiser
        </button>
      </template>
    </NavigationHeader>

    <main class="mise-en-service-main">
      <div class="container-fluid">
        <div class="row g-4">
          <!-- Cartão de Processo -->
          <div class="col-12">
            <div class="card carte-processus">
              <div class="card-body">
                <div class="atelier-entete">
                  <div>
                    <h5 class="mb-2"><i class="bi bi-rocket-takeoff me-2"></i>Activation & Configuration</h5>
                  </div>
                </div>               
              </div>
            </div>
          </div>

          <!-- Dossier Entreprise -->
          <div class="col-xl-4 col-lg-6">
            <div class="card h-100">
              <div class="card-header d-flex justify-content-between align-items-center">
                <h5 class="mb-0"><i class="bi bi-building-add me-2"></i>Organisation</h5>
                <div v-if="modeEditionEntreprise" class="badge bg-warning text-dark">Mode édition</div>
              </div>
              <div class="card-body">
                <div class="mb-3">
                  <div class="d-flex align-items-center justify-content-between mb-2">
                    <label class="form-label mb-0">Mode édition</label>
                    <div class="form-check form-switch">
                      <input class="form-check-input" type="checkbox" v-model="modeEditionEntreprise" @change="onToggleEditionEntreprise" />
                    </div>
                  </div>
                  <div v-if="modeEditionEntreprise && entreprises.length > 0" class="bloc-chercheur">
                    <label class="form-label">Sélectionner une organisation à éditer</label>
                    <input v-model.trim="rechercheEntreprise" type="search" class="form-control mb-2" placeholder="Recherche par nom ou ID" />
                    <select v-model.number="selectionEntrepriseId" class="form-select" @change="chargerEntreprisePourEditionParId(selectionEntrepriseId)">
                      <option :value="0">-- Sélectionner pour éditer --</option>
                      <option v-for="ent in entreprisesFiltrees" :key="ent.id" :value="ent.id">{{ ent.name }}</option>
                    </select>
                  </div>
                </div>
                <p class="texte-intro">Enregistrement de l'entité légale et configuration de l'accès API.</p>
                <form @submit.prevent="soumettreEntreprise">
                  <div class="mb-3">
                    <label class="form-label">Nom de l'entreprise</label>
                    <input v-model.trim="formEntreprise.nom" class="form-control" type="text" required />
                  </div>
                  <div class="mb-3">
                    <label class="form-label">Entreprise parente (optionnel)</label>
                    <select v-model="formEntreprise.parentCompanyId" class="form-select">
                      <option :value="null">Aucune</option>
                      <option v-for="ent in entreprises" :key="ent.id" :value="ent.id">{{ ent.name }}</option>
                    </select>
                  </div>
                  <div class="mb-3">
                    <label class="form-label">Header API (Nom)</label>
                    <input v-model.trim="formEntreprise.headerName" class="form-control" type="text" placeholder="Ex. X-KK-Api" />
                    <small class="text-muted">Nom de l'en-tête transportant la clé API client.</small>
                  </div>
                  <div class="mb-3">
                    <label class="form-label d-flex justify-content-between align-items-center">
                      <span>Header API (Valeur)</span>
                      <button type="button" class="btn btn-link btn-sm p-0" @click="regenererCleApi">
                        <i class="bi bi-shuffle me-1"></i>Générer
                      </button>
                    </label>
                    <div class="input-group">
                      <input v-model.trim="formEntreprise.headerValue" class="form-control" type="text" placeholder="Clé secrète fournie au client" />
                      <button class="btn btn-outline-secondary" type="button" @click="copierCleApi">
                        <i class="bi bi-clipboard"></i>
                      </button>
                    </div>
                    <small class="text-muted">Clé aléatoire (48 caractères) mélangeant lettres, chiffres et symboles.</small>
                  </div>
                  <button class="btn btn-success w-100" type="submit" :disabled="chargementEntreprise">
                    <span v-if="chargementEntreprise" class="spinner-border spinner-border-sm me-2" role="status" />
                    {{ modeEditionEntreprise ? 'Mettre à jour' : 'Activer' }} l’organisation
                  </button>
                  <button v-if="modeEditionEntreprise" class="btn btn-outline-secondary w-100 mt-2" type="button" @click="annulerEditionEntreprise">
                    Annuler
                  </button>
                </form>
              </div>
            </div>
          </div>

          <!-- Référent opérationnel -->
          <div class="col-xl-4 col-lg-6">
            <div class="card h-100">
              <div class="card-header d-flex justify-content-between align-items-center">
                <h5 class="mb-0"><i class="bi bi-person-plus me-2"></i>Compte utilisateur</h5>
                <div v-if="modeEditionUtilisateur" class="badge bg-warning text-dark">Mode édition</div>
              </div>
              <div class="card-body">
                <div class="mb-3">
                  <div class="d-flex align-items-center justify-content-between mb-2">
                    <label class="form-label mb-0">Mode édition</label>
                    <div class="form-check form-switch">
                      <input class="form-check-input" type="checkbox" v-model="modeEditionUtilisateur" @change="onToggleEditionUtilisateur" />
                    </div>
                  </div>
                  <div v-if="modeEditionUtilisateur && utilisateurs.length > 0" class="bloc-chercheur">
                    <label class="form-label">Sélectionner un utilisateur à éditer</label>
                    <input v-model.trim="rechercheUtilisateur" type="search" class="form-control mb-2" placeholder="Recherche par nom ou email" />
                    <select v-model.number="selectionUtilisateurId" class="form-select" @change="chargerUtilisateurPourEditionParId(selectionUtilisateurId)">
                      <option :value="0">-- Sélectionner pour éditer --</option>
                      <option v-for="usr in utilisateursFiltres" :key="usr.id" :value="usr.id">{{ usr.nom }} ({{ usr.email }})</option>
                    </select>
                  </div>
                </div>
                <p class="texte-intro">Création du compte utilisateur, rattaché à l’entreprise.</p>
                <form @submit.prevent="soumettreUtilisateur">
                  <div class="mb-3">
                    <label class="form-label">Nom</label>
                    <input v-model.trim="formUtilisateur.nom" class="form-control" type="text" placeholder="Ex. Responsable exploitation" required />
                  </div>
                  <div class="mb-3">
                    <label class="form-label">Email</label>
                    <input v-model.trim="formUtilisateur.email" class="form-control" type="email" placeholder="prenom.nom@client.com" required />
                  </div>
                  <div class="mb-3">
                    <label class="form-label">Mot de passe</label>
                    <input v-model="formUtilisateur.password" class="form-control" type="password" minlength="8" required />
                    <small class="text-muted">8+ caractères, majuscule, minuscule, chiffre, caractère spécial.</small>
                  </div>
                  <div class="mb-3">
                    <label class="form-label">Rôle</label>
                    <select v-model="formUtilisateur.role" class="form-select" required>
                      <option v-for="role in rolesDisponibles" :key="role" :value="role">{{ role }}</option>
                    </select>
                  </div>
                  <div class="mb-3">
                    <label class="form-label">Entreprise</label>
                    <select v-model.number="formUtilisateur.companyId" class="form-select" required>
                      <option disabled :value="0">Sélectionner une entreprise</option>
                      <option v-for="ent in entreprises" :key="ent.id" :value="ent.id">{{ ent.name }}</option>
                    </select>
                  </div>
                  <button class="btn btn-success w-100" type="submit" :disabled="chargementUtilisateur">
                    <span v-if="chargementUtilisateur" class="spinner-border spinner-border-sm me-2" role="status" />
                    {{ modeEditionUtilisateur ? 'Mettre à jour' : 'Créer' }} le compte admin
                  </button>
                  <button v-if="modeEditionUtilisateur" class="btn btn-outline-secondary w-100 mt-2" type="button" @click="annulerEditionUtilisateur">
                    Annuler
                  </button>
                </form>
              </div>
            </div>
          </div>

          <!-- Capteur -->
          <div class="col-xl-4 col-lg-12">
            <div class="card h-100">
              <div class="card-header d-flex justify-content-between align-items-center">
                <h5 class="mb-0"><i class="bi bi-cpu me-2"></i>Device</h5>
                <div v-if="modeEditionDispositif" class="badge bg-warning text-dark">Mode édition</div>
              </div>
              <div class="card-body">
                <div class="mb-3">
                  <div class="d-flex align-items-center justify-content-between mb-2">
                    <label class="form-label mb-0">Mode édition</label>
                    <div class="form-check form-switch">
                      <input class="form-check-input" type="checkbox" v-model="modeEditionDispositif" @change="onToggleEditionDispositif" />
                    </div>
                  </div>
                  <div v-if="modeEditionDispositif && devices.length > 0" class="bloc-chercheur">
                    <label class="form-label">Sélectionner un device à éditer</label>
                    <input v-model.trim="rechercheDispositif" type="search" class="form-control mb-2" placeholder="Recherche par nom ou DevEUI" />
                    <select v-model.number="selectionDispositifId" class="form-select" @change="chargerDispositifPourEditionParId(selectionDispositifId)">
                      <option :value="0">-- Sélectionner pour éditer --</option>
                      <option v-for="dev in devicesFiltres" :key="dev.id" :value="dev.id">{{ dev.name }} ({{ dev.devEui }})</option>
                    </select>
                  </div>
                </div>
                <p class="texte-intro">Création du device.</p>
                <form @submit.prevent="soumettreDispositif">
                  <div class="row g-2">
                    <div class="col-12">
                      <label class="form-label">DevEUI</label>
                      <input v-model.trim="formDispositif.devEui" class="form-control" type="text" placeholder="24E0123456789012" required />
                    </div>
                    <div class="col-md-6">
                      <label class="form-label">Nom</label>
                      <input v-model.trim="formDispositif.name" class="form-control" type="text" placeholder="Serre Nord" required />
                    </div>
                    <div class="col-md-6">
                      <label class="form-label">Modèle</label>
                      <select v-model.number="formDispositif.deviceModel" class="form-select" required>
                        <option disabled :value="0">Sélectionner</option>
                        <option
                          v-for="modele in modelesDispositifs"
                          :key="resolveModeleId(modele)"
                          :value="resolveModeleId(modele)"
                          :title="`ID: ${resolveModeleId(modele) || '?'} · ${resolveModeleNom(modele)} - ${resolveModeleDescription(modele)}`"
                        >
                          ID: {{ resolveModeleId(modele) || '?' }} · {{ resolveModeleNom(modele) }}
                        </option>
                      </select>
                    </div>
                    <div class="col-12">
                      <label class="form-label">Description</label>
                      <input v-model.trim="formDispositif.description" class="form-control" type="text" placeholder="Zone + type de mesure" required />
                    </div>
                    <div class="col-md-6">
                      <label class="form-label">Entreprise</label>
                      <select v-model.number="formDispositif.companyId" class="form-select" required>
                        <option disabled :value="0">Sélectionner une entreprise</option>
                        <option v-for="ent in entreprises" :key="ent.id" :value="ent.id">{{ ent.name }}</option>
                      </select>
                    </div>
                    <div class="col-md-6">
                      <label class="form-label">Vendeur</label>
                      <input v-model.trim="formDispositif.seller" class="form-control" type="text" placeholder="Revendeur / intégrateur" required />
                    </div>
                    <div class="col-md-6">
                      <label class="form-label">Date d'installation</label>
                      <input v-model="formDispositif.installationDate" class="form-control" type="date" required />
                    </div>
                    <div class="col-md-6">
                      <label class="form-label">Batterie initiale (%)</label>
                      <input v-model.number="formDispositif.battery" class="form-control" type="number" min="0" max="100" required />
                    </div>
                    <div class="col-12">
                      <label class="form-label">Lieu d'installation (optionnel)</label>
                      <input v-model.trim="formDispositif.installationLocation" class="form-control" type="text" placeholder="Serre A - allée 2" />
                    </div>
                    <div class="col-12 form-check mt-2 ms-2">
                      <input id="actif-krop" v-model="formDispositif.activeInKropKontrol" class="form-check-input" type="checkbox" />
                      <label for="actif-krop" class="form-check-label">Actif dans KropKontrol</label>
                    </div>
                  </div>

                  <button class="btn btn-success w-100 mt-3" type="submit" :disabled="chargementDispositif">
                    <span v-if="chargementDispositif" class="spinner-border spinner-border-sm me-2" role="status" />
                    {{ modeEditionDispositif ? 'Mettre à jour' : 'Enregistrer' }} le device
                  </button>
                  <button v-if="modeEditionDispositif" class="btn btn-outline-secondary w-100 mt-2" type="button" @click="annulerEditionDispositif">
                    Annuler
                  </button>
                </form>
              </div>
            </div>
          </div>

          <!-- Historique -->
          <div class="col-12">
            <div class="card">
              <div class="card-header d-flex justify-content-between align-items-center">
                <h5 class="mb-0"><i class="bi bi-clock-history me-2"></i>Dernières opérations</h5>
              </div>
              <div class="card-body">
                <div v-if="operations.length === 0" class="text-muted">Aucune opération pour le moment.</div>
                <ul v-else class="liste-operations mb-0">
                  <li v-for="op in operations" :key="op.id">
                    <span class="badge" :class="op.statut === 'succès' ? 'badge-success' : 'badge-danger'">{{ op.statut === 'succès' ? 'Validé' : 'Erreur' }}</span>
                    <strong>{{ op.type }}</strong>
                    <span>— {{ op.message }}</span>
                    <small class="text-muted">({{ op.horodatage }})</small>
                  </li>
                </ul>
              </div>
            </div>
          </div>

        </div>
      </div>
    </main>
  </div>
</template>

<script>
import { toast } from 'vue3-toastify'
import NavigationHeader from '@/components/NavigationHeader.vue'
import { companyService, userService, deviceService } from '@/services/api'

const alphabetSets = {
  upper: 'ABCDEFGHIJKLMNOPQRSTUVWXYZ',
  lower: 'abcdefghijklmnopqrstuvwxyz',
  digits: '0123456789',
  symbols: '!@#$%^&*()-_=+[]{}|:,.?'
}

const genererNombreAleatoire = (max) => {
  if (typeof window !== 'undefined' && window.crypto?.getRandomValues) {
    const array = new Uint32Array(1)
    window.crypto.getRandomValues(array)
    return array[0] % max
  }
  return Math.floor(Math.random() * max)
}

const genererCleApiComplexe = (longueur = 48) => {
  const pools = Object.values(alphabetSets)
  const ensureChars = pools.map(pool => pool[genererNombreAleatoire(pool.length)])
  const reste = longueur - ensureChars.length
  const alphabet = pools.join('')
  const chars = [...ensureChars]

  for (let i = 0; i < reste; i += 1) {
    chars.push(alphabet[genererNombreAleatoire(alphabet.length)])
  }

  for (let i = chars.length - 1; i > 0; i -= 1) {
    const j = genererNombreAleatoire(i + 1)
    ;[chars[i], chars[j]] = [chars[j], chars[i]]
  }

  return chars.join('')
}

export default {
  name: 'MiseEnServiceView',
  components: { NavigationHeader },
  data() {
    return {
      chargementGeneral: false,
      chargementEntreprise: false,
      chargementUtilisateur: false,
      chargementDispositif: false,
      entreprises: [],
      utilisateurs: [],
      devices: [],
      modelesDispositifs: [],
      rolesDisponibles: ['UserRW', 'Technician'],
      modeleFavoriId: 47,
      operations: [],
      rechercheEntreprise: '',
      rechercheUtilisateur: '',
      rechercheDispositif: '',
      selectionEntrepriseId: 0,
      selectionUtilisateurId: 0,
      selectionDispositifId: 0,
      modeEditionEntreprise: false,
      modeEditionUtilisateur: false,
      modeEditionDispositif: false,
      entrepriseEnEdition: null,
      utilisateurEnEdition: null,
      dispositifEnEdition: null,
      formEntreprise: {
        nom: '',
        parentCompanyId: null,
        headerName: 'Nom commercial',
        headerValue: genererCleApiComplexe()
      },
      formUtilisateur: {
        id: null,
        nom: '',
        email: '',
        password: '',
        role: 'UserRW',
        companyId: 0
      },
      formDispositif: {
        id: null,
        devEui: '',
        name: '',
        description: '',
        installationLocation: '',
        battery: 100,
        installationDate: new Date().toISOString().slice(0, 10),
        deviceModel: 47,
        companyId: 0,
        seller: '3CTEC',
        activeInKropKontrol: true
      }
    }
  },
  watch: {
    'formDispositif.deviceModel': 'mettreAJourNomDeviceAuto',
    'formDispositif.devEui': 'mettreAJourNomDeviceAuto'
  },
  computed: {
    entreprisesFiltrees() {
      const terme = (this.rechercheEntreprise || '').trim().toLowerCase()
      if (!terme) return this.entreprises
      return this.entreprises.filter((ent) => {
        const nom = (ent?.name || '').toLowerCase()
        const id = ent?.id?.toString() || ''
        return nom.includes(terme) || id.includes(terme)
      })
    },
    utilisateursFiltres() {
      const terme = (this.rechercheUtilisateur || '').trim().toLowerCase()
      if (!terme) return this.utilisateurs
      return this.utilisateurs.filter((utilisateur) => {
        const nom = (utilisateur?.nom || '').toLowerCase()
        const email = (utilisateur?.email || '').toLowerCase()
        return nom.includes(terme) || email.includes(terme)
      })
    },
    devicesFiltres() {
      const terme = (this.rechercheDispositif || '').trim().toLowerCase()
      if (!terme) return this.devices
      return this.devices.filter((device) => {
        const nom = (device?.name || '').toLowerCase()
        const devEui = (device?.devEui || '').toLowerCase()
        return nom.includes(terme) || devEui.includes(terme)
      })
    }
  },
  async mounted() {
    await this.rechargerDonnees()
  },
  methods: {
    ajouterOperation(type, statut, message) {
      this.operations.unshift({
        id: `${Date.now()}-${Math.random()}`,
        type,
        statut,
        message,
        horodatage: new Date().toLocaleString('fr-FR')
      })
      this.operations = this.operations.slice(0, 12)
    },

    async rechargerDonnees() {
      this.chargementGeneral = true
      try {
        const [entreprises, utilisateurs, devices, modeles] = await Promise.all([
          companyService.getAll(),
          userService.getAll(),
          deviceService.getAll(),
          deviceService.getModels()
        ])
        this.entreprises = Array.isArray(entreprises) ? entreprises : []
        this.utilisateurs = Array.isArray(utilisateurs) ? utilisateurs : []
        this.devices = Array.isArray(devices) ? devices : []
        this.modelesDispositifs = Array.isArray(modeles) ? modeles : []
        this.appliquerModeleFavori()
      } catch (error) {
        toast.error(`Impossible de charger les référentiels: ${error.message || 'erreur inconnue'}`)
      } finally {
        this.chargementGeneral = false
      }
    },

    appliquerModeleFavori() {
      if (!this.modelesDispositifs.length) {
        this.formDispositif.deviceModel = 0
        return
      }

      const cible = Number(this.modeleFavoriId)
      const modeleFavori = this.modelesDispositifs.find((modele) => this.resolveModeleId(modele) === cible)

      if (modeleFavori) {
        this.formDispositif.deviceModel = this.resolveModeleId(modeleFavori)
        return
      }

      const premierId = this.resolveModeleId(this.modelesDispositifs[0])
      this.formDispositif.deviceModel = premierId || 0
    },

    resolveModeleId(modele) {
      if (!modele) return 0
      const candidats = [modele.id, modele.modelId, modele.ModelId, modele.Model]
      const valeur = candidats.find((val) => val !== undefined && val !== null && !Number.isNaN(Number(val)))
      return valeur ? Number(valeur) : 0
    },

    resolveModeleNom(modele) {
      if (!modele) return 'Modèle inconnu'
      return modele.model || modele.Model || `Modèle #${this.resolveModeleId(modele)}`
    },

    resolveModeleDescription(modele) {
      if (!modele) return 'Sans description'
      return modele.description || modele.Description || 'Sans description'
    },

    extraireSuffixeDevEui(devEui) {
      if (!devEui) return ''
      const sanitized = devEui.replace(/[^0-9a-fA-F]/g, '').toUpperCase()
      return sanitized.slice(-6)
    },

    mettreAJourNomDeviceAuto() {
      const modele = this.modelesDispositifs.find(m => this.resolveModeleId(m) === Number(this.formDispositif.deviceModel))
      const suffixe = this.extraireSuffixeDevEui(this.formDispositif.devEui)

      if (!modele) return
      if (!suffixe) {
        this.formDispositif.name = this.resolveModeleNom(modele)
        return
      }

      this.formDispositif.name = `${this.resolveModeleNom(modele)}_${suffixe}`
    },

    async soumettreEntreprise() {
      const nomHeader = (this.formEntreprise.headerName || '').trim()
      if (!nomHeader || nomHeader.toLowerCase() === 'nom commercial'.toLowerCase()) {
        toast.warning('Veuillez définir un nom d\'en-tête API personnalisé avant de créer l\'organisation.')
        return
      }

      this.chargementEntreprise = true
      try {
        const payload = {
          name: this.formEntreprise.nom,
          parentCompanyId: this.formEntreprise.parentCompanyId,
          headerNameApiKey: this.formEntreprise.headerName,
          headerValueApiKey: this.formEntreprise.headerValue
        }
        let resultat
        let message

        if (this.modeEditionEntreprise && this.entrepriseEnEdition) {
          resultat = await companyService.mettreAJour(this.entrepriseEnEdition.id, payload)
          message = `Organisation '${payload.name}' mise à jour (#${this.entrepriseEnEdition.id}).`
        } else {
          resultat = await companyService.creer(payload)
          const idEntreprise = resultat?.id
          message = `Organisation '${payload.name}' activée${idEntreprise ? ` (#${idEntreprise})` : ''}.`
          if (idEntreprise) {
            this.formUtilisateur.companyId = idEntreprise
            this.formDispositif.companyId = idEntreprise
          }
        }

        this.formEntreprise.nom = ''
        this.formEntreprise.parentCompanyId = null
        this.formEntreprise.headerName = 'Nom commercial'
        this.formEntreprise.headerValue = genererCleApiComplexe()
        this.selectionEntrepriseId = 0
        this.rechercheEntreprise = ''
        this.modeEditionEntreprise = false
        this.entrepriseEnEdition = null

        await this.rechargerDonnees()
        toast.success(message)
        this.ajouterOperation('Entreprise', 'succès', message)
      } catch (err) {
        const message = err.message || 'Erreur lors de la création de l\'organisation.'
        toast.error(message)
        this.ajouterOperation('Entreprise', 'erreur', message)
      } finally {
        this.chargementEntreprise = false
      }
    },

    async chargerEntreprisePourEdition(entreprise) {
      try {
        this.modeEditionEntreprise = true
        this.entrepriseEnEdition = entreprise
        this.formEntreprise.nom = entreprise.name
        this.formEntreprise.parentCompanyId = entreprise.parentCompanyId
        this.formEntreprise.headerName = entreprise.headerNameApiKey || 'Nom commercial'
        this.formEntreprise.headerValue = entreprise.headerValueApiKey || genererCleApiComplexe()
      } catch (error) {
        toast.error('Impossible de charger les données de l\'organisation.')
      }
    },

    chargerEntreprisePourEditionParId(id) {
      if (!id) return
      const entreprise = this.entreprises.find((ent) => ent.id === id)
      if (entreprise) {
        this.chargerEntreprisePourEdition(entreprise)
      } else {
        toast.warning('Entreprise introuvable dans la liste.')
        this.selectionEntrepriseId = 0
      }
    },

    async annulerEditionEntreprise() {
      this.modeEditionEntreprise = false
      this.entrepriseEnEdition = null
      this.formEntreprise.nom = ''
      this.formEntreprise.parentCompanyId = null
      this.formEntreprise.headerName = 'Nom commercial'
      this.formEntreprise.headerValue = genererCleApiComplexe()
      this.selectionEntrepriseId = 0
      this.rechercheEntreprise = ''
    },

    async soumettreUtilisateur() {
      if (!this.formUtilisateur.companyId) {
        toast.warning("Sélectionnez d'abord une organisation.")
        return
      }

      this.chargementUtilisateur = true
      try {
        const payload = {
          nom: this.formUtilisateur.nom,
          email: this.formUtilisateur.email,
          password: this.formUtilisateur.password,
          role: this.formUtilisateur.role,
          companyId: this.formUtilisateur.companyId
        }
        let resultat
        let message

        if (this.modeEditionUtilisateur && this.utilisateurEnEdition) {
          resultat = await userService.mettreAJour(this.utilisateurEnEdition.id, payload)
          message = `Utilisateur '${payload.nom}' mis à jour.`
        } else {
          resultat = await userService.creer(payload)
          message = `Utilisateur '${payload.nom}' créé.`
        }

        this.formUtilisateur.nom = ''
        this.formUtilisateur.email = ''
        this.formUtilisateur.password = ''
        this.selectionUtilisateurId = 0
        this.rechercheUtilisateur = ''
        this.modeEditionUtilisateur = false
        this.utilisateurEnEdition = null

        toast.success(message)
        this.ajouterOperation('Utilisateur', 'succès', message)
      } catch (err) {
        const message = err.message || 'Erreur lors de la création de l\'utilisateur.'
        toast.error(message)
        this.ajouterOperation('Utilisateur', 'erreur', message)
      } finally {
        this.chargementUtilisateur = false
      }
    },

    async chargerUtilisateurPourEdition(utilisateur) {
      try {
        this.modeEditionUtilisateur = true
        this.utilisateurEnEdition = utilisateur
        this.formUtilisateur.id = utilisateur.id
        this.formUtilisateur.nom = utilisateur.nom
        this.formUtilisateur.email = utilisateur.email
        this.formUtilisateur.role = utilisateur.role
        this.formUtilisateur.companyId = utilisateur.companyId
        this.formUtilisateur.password = ''
      } catch (error) {
        toast.error('Impossible de charger les données de l\'utilisateur.')
      }
    },

    chargerUtilisateurPourEditionParId(id) {
      if (!id) return
      const utilisateur = this.utilisateurs.find((usr) => usr.id === id)
      if (utilisateur) {
        this.chargerUtilisateurPourEdition(utilisateur)
      } else {
        toast.warning('Utilisateur introuvable dans la liste.')
        this.selectionUtilisateurId = 0
      }
    },

    async annulerEditionUtilisateur() {
      this.modeEditionUtilisateur = false
      this.utilisateurEnEdition = null
      this.formUtilisateur.id = null
      this.formUtilisateur.nom = ''
      this.formUtilisateur.email = ''
      this.formUtilisateur.password = ''
      this.formUtilisateur.role = 'UserRW'
      this.formUtilisateur.companyId = 0
      this.selectionUtilisateurId = 0
      this.rechercheUtilisateur = ''
    },

    async soumettreDispositif() {
      if (!this.formDispositif.companyId) {
        toast.warning("Sélectionnez d'abord une organisation.")
        return
      }

      if (!this.formDispositif.deviceModel) {
        toast.warning("Sélectionnez d'abord un modèle de device.")
        return
      }

      this.chargementDispositif = true
      try {
        const payload = {
          devEui: this.formDispositif.devEui,
          name: this.formDispositif.name,
          description: this.formDispositif.description,
          installationLocation: this.formDispositif.installationLocation || null,
          battery: Number(this.formDispositif.battery),
          installationDate: new Date(this.formDispositif.installationDate).toISOString(),
          deviceModel: Number(this.formDispositif.deviceModel),
          companyId: Number(this.formDispositif.companyId),
          seller: this.formDispositif.seller,
          activeInKropKontrol: this.formDispositif.activeInKropKontrol
        }
        let resultat
        let message

        if (this.modeEditionDispositif && this.dispositifEnEdition) {
          resultat = await deviceService.mettreAJour(this.dispositifEnEdition.id, payload)
          message = `Device '${payload.name}' (${payload.devEui}) mis à jour.`
        } else {
          resultat = await deviceService.creer(payload)
          message = `Device '${payload.name}' (${payload.devEui}) enregistré.`
        }

        toast.success(message)
        this.ajouterOperation('Device', 'succès', message)

        this.formDispositif.devEui = ''
        this.formDispositif.name = ''
        this.formDispositif.description = ''
        this.formDispositif.installationLocation = ''
        this.formDispositif.battery = 100
        this.selectionDispositifId = 0
        this.rechercheDispositif = ''
        this.modeEditionDispositif = false
        this.dispositifEnEdition = null
      } catch (err) {
        const message = err.message || 'Erreur lors de l\'enregistrement du device.'
        toast.error(message)
        this.ajouterOperation('Device', 'erreur', message)
      } finally {
        this.chargementDispositif = false
      }
    },

    async chargerDispositifPourEdition(dispositif) {
      try {
        this.modeEditionDispositif = true
        this.dispositifEnEdition = dispositif
        this.formDispositif.id = dispositif.id
        this.formDispositif.devEui = dispositif.devEui
        this.formDispositif.name = dispositif.name
        this.formDispositif.description = dispositif.description
        this.formDispositif.installationLocation = dispositif.installationLocation || ''
        this.formDispositif.battery = dispositif.battery
        this.formDispositif.installationDate = dispositif.installationDate ? new Date(dispositif.installationDate).toISOString().slice(0, 10) : new Date().toISOString().slice(0, 10)
        this.formDispositif.deviceModel = dispositif.deviceModel
        this.formDispositif.companyId = dispositif.companyId
        this.formDispositif.seller = dispositif.seller
        this.formDispositif.activeInKropKontrol = dispositif.activeInKropKontrol
      } catch (error) {
        toast.error('Impossible de charger les données du device.')
      }
    },

    chargerDispositifPourEditionParId(id) {
      if (!id) return
      const dispositif = this.devices.find((dev) => dev.id === id)
      if (dispositif) {
        this.chargerDispositifPourEdition(dispositif)
      } else {
        toast.warning('Device introuvable dans la liste.')
        this.selectionDispositifId = 0
      }
    },

    async annulerEditionDispositif() {
      this.modeEditionDispositif = false
      this.dispositifEnEdition = null
      this.formDispositif.id = null
      this.formDispositif.devEui = ''
      this.formDispositif.name = ''
      this.formDispositif.description = ''
      this.formDispositif.installationLocation = ''
      this.formDispositif.battery = 100
      this.formDispositif.installationDate = new Date().toISOString().slice(0, 10)
      this.formDispositif.deviceModel = 47
      this.formDispositif.companyId = 0
      this.formDispositif.seller = '3CTEC'
      this.formDispositif.activeInKropKontrol = true
      this.selectionDispositifId = 0
      this.rechercheDispositif = ''
    },

    regenererCleApi() {
      this.formEntreprise.headerValue = genererCleApiComplexe()
      toast.info('Nouvelle clé API générée')
    },

    async copierCleApi() {
      try {
        await navigator.clipboard.writeText(this.formEntreprise.headerValue)
        toast.info('Clé API copiée dans le presse-papiers.')
      } catch (error) {
        toast.error("Impossible de copier la clé API")
      }
    },

    onToggleEditionEntreprise() {
      if (!this.modeEditionEntreprise) {
        this.annulerEditionEntreprise()
      } else {
        // Vider les champs du formulaire en mode édition mais garder les placeholders
        this.formEntreprise.nom = ''
        this.formEntreprise.parentCompanyId = null
        this.formEntreprise.headerName = ''
        this.formEntreprise.headerValue = ''
        // Réinitialiser la recherche
        this.rechercheEntreprise = ''
        this.selectionEntrepriseId = 0
      }
    },

    onToggleEditionUtilisateur() {
      if (!this.modeEditionUtilisateur) {
        this.annulerEditionUtilisateur()
      } else {
        // Vider les champs du formulaire en mode édition mais garder les placeholders
        this.formUtilisateur.id = null
        this.formUtilisateur.nom = ''
        this.formUtilisateur.email = ''
        this.formUtilisateur.password = ''
        this.formUtilisateur.role = 'UserRW'
        this.formUtilisateur.companyId = 0
        // Réinitialiser la recherche
        this.rechercheUtilisateur = ''
        this.selectionUtilisateurId = 0
      }
    },

    onToggleEditionDispositif() {
      if (!this.modeEditionDispositif) {
        this.annulerEditionDispositif()
      } else {
        // Vider les champs du formulaire en mode édition mais garder les placeholders
        this.formDispositif.id = null
        this.formDispositif.devEui = ''
        this.formDispositif.name = ''
        this.formDispositif.description = ''
        this.formDispositif.installationLocation = ''
        this.formDispositif.battery = 100
        this.formDispositif.installationDate = new Date().toISOString().slice(0, 10)
        this.formDispositif.deviceModel = 0
        this.formDispositif.companyId = 0
        this.formDispositif.seller = ''
        this.formDispositif.activeInKropKontrol = true
        // Réinitialiser la recherche
        this.rechercheDispositif = ''
        this.selectionDispositifId = 0
      }
    }
  }
}
</script>

<style scoped>
.mise-en-service-page {
  background: radial-gradient(circle at top, #eef4ff 0%, #fbfbfe 35%, #f5f6fb 100%);
  min-height: 100vh;
  padding-bottom: 3rem;
}

.mise-en-service-main {
  padding-top: 1.5rem;
}

.mise-en-service-main .container-fluid {
  max-width: 1440px;
}

.carte-processus {
  border-radius: 22px;
  border: none;
  background: linear-gradient(120deg, rgba(79, 70, 229, 0.08), rgba(14, 165, 233, 0.08));
  box-shadow: 0 25px 45px rgba(15, 23, 42, 0.12);
  overflow: hidden;
}

.atelier-entete {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 1rem;
}

.atelier-entete h5 {
  font-size: 1.4rem;
  font-weight: 700;
  color: #0f172a;
}

.atelier-entete p {
  margin: 0;
  color: #475569;
  font-size: 0.95rem;
}

.atelier-actions {
  display: flex;
  gap: 0.75rem;
  flex-wrap: wrap;
}

.atelier-pill {
  border-radius: 999px;
  padding: 0.45rem 1.2rem;
  font-size: 0.85rem;
  font-weight: 600;
  background: #fff;
  border: 1px solid rgba(99, 102, 241, 0.25);
  color: #4338ca;
}

.card {
  border: none;
  border-radius: 20px;
  box-shadow: 0 20px 45px rgba(15, 23, 42, 0.08);
  background: #fff;
}

.card-header {
  border-bottom: none;
  background: none;
  padding-bottom: 0.25rem;
}

.card-header h5 {
  font-size: 1.1rem;
  font-weight: 700;
  color: #0f172a;
}

.card-body {
  padding-top: 0.75rem;
}

.texte-intro {
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  padding: 0.8rem 1rem;
  margin-bottom: 1.2rem;
  color: #475569;
  font-size: 0.9rem;
}

.form-label {
  font-weight: 600;
  color: #0f172a;
  font-size: 0.92rem;
}

.form-control,
.form-select {
  border-radius: 12px;
  border: 1px solid #e2e8f0;
  padding: 0.65rem 0.85rem;
  font-size: 0.95rem;
  background-color: #fbfdff;
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
}

.form-control:focus,
.form-select:focus {
  border-color: #6366f1;
  box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.15);
}

.form-control::placeholder,
.form-select::placeholder {
  color: #cbd5e1;
  font-style: italic;
  opacity: 0.7;
}

.form-control:not(:placeholder-shown),
.form-select:not(:placeholder-shown) {
  font-weight: 600;
  color: #0f172a;
}

.bloc-chercheur {
  background: #f1f5f9;
  border-radius: 16px;
  padding: 0.75rem 1rem;
  border: 1px dashed rgba(99, 102, 241, 0.35);
}

.bloc-chercheur select {
  background-color: #fff;
}

.btn-success {
  background: linear-gradient(135deg, #6366f1, #8b5cf6);
  border: none;
  border-radius: 14px;
  font-weight: 600;
  padding: 0.75rem;
  box-shadow: 0 18px 35px rgba(79, 70, 229, 0.35);
}

.btn-outline-secondary {
  border-radius: 12px;
}

.badge.bg-warning {
  background: rgba(250, 204, 21, 0.2) !important;
  color: #b45309 !important;
  padding: 0.4rem 0.9rem;
  border-radius: 999px;
  font-weight: 600;
}

.titre-section {
  font-weight: 700;
  font-size: 1.15rem;
  color: #0f172a;
}

.liste-operations li {
  background: #ffffff;
  border: 1px solid #e5e7eb;
  padding: 0.6rem 0.85rem;
  border-radius: 10px;
  margin-bottom: 0.5rem;
  display: flex;
  align-items: center;
  gap: 0.6rem;
  font-size: 0.9rem;
}

.badge-success {
  background: rgba(16, 185, 129, 0.18);
  color: #047857;
  border-radius: 999px;
  padding: 0.15rem 0.65rem;
}

.badge-danger {
  background: rgba(239, 68, 68, 0.2);
  color: #b91c1c;
  border-radius: 999px;
  padding: 0.15rem 0.65rem;
}

.spinner-border.spin {
  animation: spin 0.9s linear infinite;
}

@keyframes spin {
  100% {
    transform: rotate(360deg);
  }
}

@media (max-width: 1200px) {
  .card {
    border-radius: 16px;
  }
}

@media (max-width: 768px) {
  .card-body {
    padding: 1rem;
  }

  .bloc-chercheur {
    padding: 0.65rem;
  }
}

@media (max-width: 576px) {
  .card {
    box-shadow: 0 12px 25px rgba(15, 23, 42, 0.08);
  }

  .mise-en-service-page {
    padding-bottom: 2rem;
  }

  .atelier-entete {
    flex-direction: column;
    align-items: flex-start;
  }

  .btn-success {
    font-size: 0.95rem;
  }
}
</style>
