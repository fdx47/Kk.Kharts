<template>
  <div class="page">
    <header class="mb-3">
      <h2>Test Push OneSignal</h2>
      <p class="text-muted">Demande d'autorisation, capture du Player ID et envoi d'un push de test via l'API.</p>
    </header>

    <section class="card">
      <div class="card-body">
        <h5>1) Initialiser et demander la permission</h5>
        <p class="small text-muted">App Id préconfiguré côté front : {{ appId }}</p>
        <div class="d-flex gap-2 flex-wrap">
          <button class="btn btn-primary" :disabled="busyInit" @click="onInit">Initialiser OneSignal</button>
          <button class="btn btn-secondary" :disabled="!sdkReady" @click="onPrompt">Montrer la demande de permission</button>
        </div>
      </div>
    </section>

    <section class="card">
      <div class="card-body">
        <h5>2) Lire le Player ID</h5>
        <div class="d-flex gap-2 flex-wrap align-items-center">
          <button class="btn btn-outline-primary" :disabled="!sdkReady" @click="onGetId">Lire Player ID</button>
          <span class="fw-bold" v-if="playerId">{{ playerId }}</span>
          <span class="text-muted" v-else>Aucun Player ID capturé.</span>
        </div>
      </div>
    </section>

    <section class="card">
      <div class="card-body">
        <h5>3) Envoyer un push de test (backend)</h5>
        <div class="mb-2">
          <label class="form-label">Message (optionnel)</label>
          <input v-model="message" class="form-control" placeholder="Texte de la notification" />
        </div>
        <div class="mb-2">
          <small class="text-muted">Le token JWT est lu depuis localStorage (authToken). Assurez-vous d’être connecté.</small>
        </div>
        <button class="btn btn-success" :disabled="!sdkReady || sending" @click="onSend">Envoyer via /api/v1/notifications-test/onesignal</button>
      </div>
    </section>

    <section class="card">
      <div class="card-body">
        <h5>Logs</h5>
        <pre class="logbox">{{ logs.join('\n') }}</pre>
      </div>
    </section>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { ensureReady, requestPermissionAndOptIn, getPlayerId } from '../services/onesignal'

const appId = '773b3554-2728-4944-aee2-2a8f88faa07a'
const sdkReady = ref(false)
const busyInit = ref(false)
const sending = ref(false)
const playerId = ref('')
const message = ref('')
const logs = ref([])

const apiBase = import.meta.env.VITE_API_BASE_URL || ''

function log(msg) {
  const ts = new Date().toISOString()
  logs.value.push(`[${ts}] ${msg}`)
}

async function onInit() {
  busyInit.value = true
  try {
    await requestPermissionAndOptIn()
    const subscribed = await window.OneSignal.Notifications.isPushSubscribed?.() ?? false
    sdkReady.value = true
    log(`OneSignal initialisé. Subscription active: ${subscribed}`)
  } catch (e) {
    console.error(e)
    log('Erreur init: ' + e)
  } finally {
    busyInit.value = false
  }
}

async function onPrompt() {
  if (!sdkReady.value) return
  try {
    await requestPermissionAndOptIn()
    const subscribed = await window.OneSignal.Notifications.isPushSubscribed?.() ?? false
    log(`Permission accordée. Subscription active: ${subscribed}`)
  } catch (e) {
    console.error(e)
    log('Erreur prompt: ' + e)
  }
}

async function onGetId() {
  if (!sdkReady.value) return
  try {
    const pid = await getPlayerId()
    playerId.value = pid || ''
    log(pid ? 'Player ID capturé.' : 'Player ID non disponible (autorisation/refus ?).')
  } catch (e) {
    console.error(e)
    log('Erreur getUserId: ' + e)
  }
}

async function onSend() {
  if (!sdkReady.value) return
  const token = localStorage.getItem('authToken')
  if (!token) {
    alert('Token JWT non trouvé (authToken). Connectez-vous d\'abord.')
    return
  }
  sending.value = true
  try {
    const resp = await fetch(`${apiBase}/api/v1/notifications-test/onesignal`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`
      },
      body: JSON.stringify({ message: message.value || undefined })
    })
    const data = await resp.json().catch(() => ({}))
    log(`Réponse ${resp.status}: ${JSON.stringify(data)}`)
  } catch (e) {
    console.error(e)
    log('Erreur envoi: ' + e)
  } finally {
    sending.value = false
  }
}

onMounted(() => {
  log('Page test OneSignal prête. Étapes: init → permission → Player ID → envoi.')
})
</script>

<style scoped>
.page { max-width: 900px; margin: 0 auto; padding: 16px; }
.card { background: #111827; border: 1px solid #1f2937; border-radius: 12px; margin-bottom: 16px; box-shadow: 0 10px 20px rgba(0,0,0,0.25); }
.card-body { color: #e5e7eb; }
pre.logbox { background: #0b1220; color: #e5e7eb; padding: 12px; border-radius: 8px; min-height: 120px; border: 1px solid #1f2937; }
button { min-width: 180px; }
</style>
