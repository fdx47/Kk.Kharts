import { ref, effectScope, watch } from "vue";
import debounce from "lodash/debounce";
import {
  pushDashboardStateToApi,
  buildDashboardStatePayload,
  getLastDashboardSyncSignature,
  setLastDashboardSyncSignature,
  isDashboardHydrationInProgress,
} from "@/services/dashboardStateService.js";
import { LS_COORDS, LS_TIMEZONE } from "../config/storageKeys.js";

const coords = ref({ lat: 48.8566, lon: 2.3522 });
const zone = ref("Europe/Paris");
let storageInitialised = false;

function hydrateFromLocalStorage() {
  if (storageInitialised) return;
  storageInitialised = true;
  if (typeof window === "undefined") return;
  try {
    const storedCoords = localStorage.getItem(LS_COORDS);
    if (storedCoords) {
      const parsed = JSON.parse(storedCoords);
      if (parsed && parsed.lat != null && parsed.lon != null) {
        coords.value = parsed;
      }
    }
    const storedZone = localStorage.getItem(LS_TIMEZONE);
    if (storedZone) zone.value = storedZone;
  } catch {}
}

hydrateFromLocalStorage();

const persistDashboardPreferences = debounce(async () => {
  if (isDashboardHydrationInProgress()) return;
  const payload = buildDashboardStatePayload();
  const signature = JSON.stringify(payload);
  if (signature === getLastDashboardSyncSignature()) return;
  try {
    await pushDashboardStateToApi(payload);
    setLastDashboardSyncSignature(signature);
  } catch (err) {
    console.error(
      "Echec de la sauvegarde des preferences de geolocalisation:",
      err,
    );
  }
}, 1200);

const syncScope = effectScope();
syncScope.run(() => {
  watch(
    [coords, zone],
    () => {
      if (isDashboardHydrationInProgress()) return;
      if (typeof window !== "undefined") {
        try {
          localStorage.setItem(LS_COORDS, JSON.stringify(coords.value));
        } catch {}
        try {
          localStorage.setItem(LS_TIMEZONE, zone.value);
        } catch {}
      }
      persistDashboardPreferences();
    },
    { deep: true },
  );
});

if (typeof window !== "undefined") {
  window.addEventListener("kk-dashboard-state-hydrated", (event) => {
    const detail = event?.detail ?? {};
    if (detail.coords && detail.coords.lat != null && detail.coords.lon != null) {
      coords.value = detail.coords;
    }
    if (detail.timezone) {
      zone.value = detail.timezone;
    }
  });
}

export function useGeolocation() {
  hydrateFromLocalStorage();

  function requestLocation() {
    if (typeof navigator === "undefined" || !navigator.geolocation) return;

    try {
      if (
        localStorage.getItem(LS_COORDS) &&
        localStorage.getItem(LS_TIMEZONE)
      ) {
        return;
      }
    } catch {}

    if (
      !confirm(
        "Autoriser la geolocalisation pour calculer lever et coucher du soleil localement ?",
      )
    ) {
      return;
    }

    navigator.geolocation.getCurrentPosition(
      (pos) => {
        coords.value = {
          lat: pos.coords.latitude,
          lon: pos.coords.longitude,
        };
        zone.value =
          Intl.DateTimeFormat().resolvedOptions().timeZone || zone.value;
        try {
          localStorage.setItem(LS_COORDS, JSON.stringify(coords.value));
          localStorage.setItem(LS_TIMEZONE, zone.value);
        } catch {}
        persistDashboardPreferences();
      },
      (err) => {
        console.warn("Erreur de geolocalisation", err);
      },
    );
  }

  return { coords, zone, requestLocation };
}

export default useGeolocation;

