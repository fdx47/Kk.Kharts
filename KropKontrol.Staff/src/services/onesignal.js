const defaultAppId = '773b3554-2728-4944-aee2-2a8f88faa07a'

const appId = import.meta.env.VITE_ONESIGNAL_APP_ID || defaultAppId

async function loadAndInit() {
  if (!window.OneSignal) throw new Error('SDK OneSignal non chargé')
  if (window.__oneSignalInitDone) return window.OneSignal
  // attendre le init de index.html; si non fait en 5s, on tente ici
  let attempts = 0
  while (!window.__oneSignalInitDone && attempts < 50) {
    await new Promise(r => setTimeout(r, 100))
    attempts++
  }
  if (window.__oneSignalInitDone) return window.OneSignal
  throw new Error("Initialisation OneSignal non terminée (index.html)")
}

export async function ensureReady() {
  return loadAndInit()
}

export async function requestPermissionAndOptIn() {
  const OneSignal = await loadAndInit()
  const permission = await OneSignal.Notifications.requestPermission()
  if (permission !== 'granted') {
    throw new Error('Permission notifications refusée ou bloquée')
  }
  await OneSignal.User.PushSubscription.optIn()
  return OneSignal
}

export async function getPlayerId() {
  const OneSignal = await requestPermissionAndOptIn()
  const pid = OneSignal?.User?.PushSubscription?.id || (await OneSignal.getUserId?.())
  if (!pid) throw new Error('Player ID non disponible')
  return pid
}
