<!--
  LoginPage.vue

  This component renders the login page of the application.
  It provides input fields for user credentials (such as username and password)
  and handles user authentication logic. Upon successful login, it redirects
  the user to the appropriate dashboard or home page. The component may also
  display error messages for failed login attempts and offer links to password
  recovery or registration pages.
-->
<template>
  <div
    class="login-page-bg d-flex justify-content-center align-items-center vh-100"
  >
    <!-- Image de fond doublée -->
    <img src="/assets/KKv.svg" alt="background" class="bg-img" />

    <div class="container py-5 login-container" style="max-width: 400px">
      <div class="card login-card">
        <h2 class="mb-4 text-center text-krop">
          KropKontrol<br />
          <small class="d-block fs-6 text-krop"
            >Smart Charts for Smart Devices</small
          >
        </h2>
        <form @submit.prevent="login">
          <div class="mb-3">
            <label class="form-label">Email</label>
            <input v-model="email" type="email" class="form-control" required />
          </div>
          <div class="mb-3">
            <label class="form-label">Mot de passe</label>
            <input
              v-model="password"
              type="password"
              class="form-control"
              required
            />
          </div>
          <button type="submit" class="btn login-btn" :disabled="loading">
            <span v-if="loading">Connexion...</span>
            <span v-else>Se connecter</span>
          </button>
          <p class="text-danger mt-3" v-if="error">{{ error }}</p>
        </form>
      </div>
    </div>
    <!-- Texte en bas de page -->
    <div class="login-footer text-center mt-4">
      <small class="login-footer-text text-krop">
        KropKontrol by
        <a
          href="https://3ctec.fr/"
          target="_blank"
          rel="noopener"
          class="text-decoration-none text-krop"
        >
          3ctec
        </a>
        and
        <a
          href="http://stratberries.com"
          target="_blank"
          rel="noopener"
          class="text-decoration-none text-krop"
        >
          StratBerries
        </a>
        - © 2025
        <br />
        <span class="login-footer-mail">
          info@kropkontrol.com
        </span>
      </small>
    </div>
  </div>
</template>

<script setup>
import { ref, inject } from "vue";
import { useRouter } from "vue-router";
import {
  clearUserCache,
  filterChartsByAccess,
  getUserIdFromToken,
  storeAuthData,
} from "@/services/authService.js";
import { useDevices } from "@/composables/useDevices.js";
import { jwtDecode } from "jwt-decode";
import {
  LS_VIRTUAL_GROUPS_PREFIX,
  LS_DASHBOARD_STATE_PREFIX,
  LS_COORDS,
  LS_TIMEZONE,
} from "@/config/storageKeys.js";
import { useGeolocation } from "../composables/useGeolocation.js";
import {
  buildDashboardStatePayload,
  fetchDashboardStateFromApi,
  setDashboardHydrationState,
  setLastDashboardSyncSignature,
  setLastHydratedDashboardState,
} from "@/services/dashboardStateService.js";

const router = useRouter();
const { reset: resetVirtualDevices } = inject("virtualDevices");
const email = ref("");
const password = ref("");
const error = ref("");
const loading = ref(false);
const { requestLocation } = useGeolocation();
const { loadDevices } = useDevices();

function isDashboardDebugEnabled() {
  if (typeof window !== "undefined") {
    if (window.__KK_DEBUG_DASHBOARD_STATE__ === false) return false;
    if (window.__KK_DEBUG_DASHBOARD_STATE__ === true) return true;
  }
  return Boolean(import.meta.env?.DEV);
}

function logDashboardDebug(message, ...args) {
  if (!isDashboardDebugEnabled()) return;
  // eslint-disable-next-line no-console
  console.info(`[LoginPage] ${message}`, ...args);
}

async function login() {
  loading.value = true;
  error.value = "";

  try {
    if (typeof window !== "undefined") {
      window.__KK_DEBUG_DASHBOARD_STATE__ = true;
      console.info(
        "[dashboardState] Debug logging enabled (set window.__KK_DEBUG_DASHBOARD_STATE__ = false pour le désactiver)",
      );
    }
    logDashboardDebug("Login request started", { email: email.value });
    const res = await fetch(
      "https://kropkontrol.premiumasp.net/api/v1/auth/login",
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ Email: email.value, Password: password.value }),
      },
    );

    const data = await res.json();

    if (
      !res.ok ||
      !data.token ||
      !data.refreshToken ||
      !data.refreshTokenExpiryTime
    ) {
      throw new Error(data.message || "Identifiants invalides");
    }

    const newUserId = jwtDecode(data.token).nameid;
    const previousUserId = getUserIdFromToken();
    if (previousUserId && previousUserId !== newUserId) {
      clearUserCache(previousUserId);
    }
    // Centralized, safe storage (handles invalid/unknown expiry)
    storeAuthData(data.token, data.refreshToken, data.refreshTokenExpiryTime);

    const userStorageKey =
      LS_DASHBOARD_STATE_PREFIX + (newUserId ?? "guest");
    setDashboardHydrationState(true);
    try {
      const remoteState = await fetchDashboardStateFromApi();
      logDashboardDebug("Fetched remote dashboard state", remoteState);
      if (remoteState) {
        const remoteCharts = Array.isArray(remoteState.dashboardState)
          ? remoteState.dashboardState
          : Array.isArray(remoteState.charts)
            ? remoteState.charts
            : null;
        const hasMeaningfulState =
          (Array.isArray(remoteCharts) && remoteCharts.length > 0) ||
          Boolean(remoteState.coords) ||
          Boolean(remoteState.timezone);

        if (hasMeaningfulState) {
          if (remoteCharts) {
            try {
              localStorage.setItem(
                userStorageKey,
                JSON.stringify(remoteCharts),
              );
            } catch (err) {
              console.error("Impossible d'ecrire dashboardState local:", err);
            }
            logDashboardDebug("Stored remote charts in localStorage", {
              chartCount: remoteCharts.length,
            });
          }

          let chartsForSignature = remoteCharts;
          if (!chartsForSignature) {
            try {
              const existingRaw = localStorage.getItem(userStorageKey);
              if (existingRaw) {
                const parsedExisting = JSON.parse(existingRaw);
                if (Array.isArray(parsedExisting)) {
                  chartsForSignature = parsedExisting;
                }
              }
            } catch {}
          }
          logDashboardDebug("Charts used for signature", {
            chartCount: chartsForSignature?.length,
          });

          const hydrationEventDetail = {};
          if (remoteState.coords) {
            try {
              localStorage.setItem(
                LS_COORDS,
                JSON.stringify(remoteState.coords),
              );
            } catch (err) {
              console.warn("Impossible d'ecrire kk_coords au login:", err);
            }
            hydrationEventDetail.coords = remoteState.coords;
            logDashboardDebug("Stored remote coords", remoteState.coords);
          }

          if (remoteState.timezone) {
            try {
              localStorage.setItem(LS_TIMEZONE, remoteState.timezone);
            } catch (err) {
              console.warn("Impossible d'ecrire kk_tz au login:", err);
            }
            hydrationEventDetail.timezone = remoteState.timezone;
            logDashboardDebug("Stored remote timezone", remoteState.timezone);
          }

          if (
            typeof window !== "undefined" &&
            Object.keys(hydrationEventDetail).length
          ) {
            try {
              window.dispatchEvent(
                new CustomEvent("kk-dashboard-state-hydrated", {
                  detail: hydrationEventDetail,
                }),
              );
            } catch (err) {
              console.warn("Echec de l'emission de l'evenement d'hydratation:", err);
            }
            logDashboardDebug("Hydration event dispatched", hydrationEventDetail);
          }

          const signaturePayload = buildDashboardStatePayload({
            charts: chartsForSignature ?? [],
          });
          setLastDashboardSyncSignature(JSON.stringify(signaturePayload));
          logDashboardDebug("Dashboard signature established at login", {
            chartCount: signaturePayload.dashboardState?.length,
          });
          setLastHydratedDashboardState({
            dashboardState: chartsForSignature ?? [],
            coords: hydrationEventDetail.coords ?? remoteState.coords ?? null,
            timezone:
              hydrationEventDetail.timezone ?? remoteState.timezone ?? null,
          });
          logDashboardDebug("Cached hydrated dashboard state after login", {
            chartCount: chartsForSignature?.length,
          });
        }
      } else {
        setLastHydratedDashboardState(null);
        logDashboardDebug("Remote dashboard state empty at login");
      }
    } catch (err) {
      console.error("Echec du chargement du dashboard depuis l'API:", err);
      logDashboardDebug("Fetching dashboard state during login failed", err);
    } finally {
      setDashboardHydrationState(false);
    }

    // Migrate virtual devices stored before login under the "guest" key
    const guestGroups = localStorage.getItem(
      `${LS_VIRTUAL_GROUPS_PREFIX}guest`,
    );
    if (guestGroups) {
      localStorage.setItem(
        `${LS_VIRTUAL_GROUPS_PREFIX}${newUserId}`,
        guestGroups,
      );
    }
    resetVirtualDevices();

    loadDevices()
      .then((list) => filterChartsByAccess(list.map((d) => d.devEui)))
      .catch(() => {});
    requestLocation();
    logDashboardDebug("Login successful, redirecting to Snapshot");
    router.push({ name: "Snapshot" });
  } catch (err) {
    error.value = err.message;
    logDashboardDebug("Login failed", err);
  } finally {
    loading.value = false;
  }
}
// calcul dynamique de l’année courante
const currentYear = ref(new Date().getFullYear());
</script>
<style scoped>
@import url("https://fonts.googleapis.com/css2?family=Poppins:wght@400;600;700&display=swap");

.login-page-bg {
  position: relative;
  background-color: #fff;
  overflow: hidden; /* pour cacher tout débordement */
}
.login-card {
  background-color: #ffffff; /* fond blanc */
  border: 4px solid #82be20ff; /* liseré Bootstrap “success” */
  border-radius: 0.5rem; /* arrondis sympas */
  padding: 1rem; /* un peu d’air autour */
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}
.login-card h2 {
  color: #82be20ff; /* même vert que le liseré */
  font-family: "Poppins", sans-serif; /* police Poppins renforçant la marque */
  font-size: 3rem; /* augmente la taille */
  font-weight: 700;
  margin-bottom: 1rem; /* ajuste l’espacement sous le titre */
  text-align: center;
}
.login-card h2 small {
  color: #82be20ff; /* harmonise le sous-titre */
  font-family: "Poppins", sans-serif;
  font-weight: 400;
}
.text-krop {
  color: #82be20ff !important; /* fixe couleur des textes de marque */
}

.bg-img {
  position: absolute;
  top: 50%;
  left: 50%;
  width: 160vw;
  max-width: 2100px;
  min-width: 500px;
  height: auto;
  max-height: 120vh;
  min-height: 180px;
  transform: translate(-50%, -50%);
  opacity: 1;
  pointer-events: none;
  z-index: 0;
  user-select: none;
  transition:
    width 0.22s,
    max-width 0.19s,
    opacity 0.18s;
}

@media (max-width: 900px) {
  .bg-img {
    width: 220vw;
    max-width: 1400px;
    min-width: 220px;
    top: 54%;
    transform: translate(-50%, -54%);
  }
}
@media (max-width: 600px) {
  .bg-img {
    width: 260vw;
    max-width: none;
    min-width: 0;
    top: 59%;
    opacity: 1;
    transform: translate(-50%, -59%);
  }
}

@media (max-width: 600px) {
  .bg-img {
    width: 320vw;
    max-width: none;
    min-width: 0;
    top: 60%;
    opacity: 1;
    transform: translate(-50%, -60%);
  }
}

/* centrer le formulaire par-dessus */
.d-flex {
  display: flex !important;
}
.justify-content-center {
  justify-content: center !important;
}
.align-items-center {
  align-items: center !important;
}
.vh-100 {
  height: 100vh !important;
}
.login-container {
  position: relative;
  z-index: 1;
}
.login-btn {
  display: block; /* passe en bloc pour que les margins fonctionnent */
  width: auto; /* largeur dépend du contenu */
  background-color: #82be20ff;
  border-color: #82be20ff;
  color: #ffffff;
  padding: 0.5rem 2rem; /* ajustable */
  margin: 1rem auto 0; /* 1rem au-dessus, auto à gauche/droite, 0 en-dessous */
}
.login-btn:hover,
.login-btn:focus {
  background-color: #6baa1aff; /* légère variation au hover */
  border-color: #6baa1aff;
  color: #ffffff;
}
.login-btn:disabled {
  background-color: #82be20ff;
  border-color: #82be20ff;
  opacity: 0.7;
}
.login-card form .form-control {
  width: 100%; /* pour mobile, prend tout l’espace disponible */
  max-width: 200px; /* largeur fixe souhaitée */
  margin: 0.5rem auto; /* 0.5 rem en haut/bas, centré horizontalement */
  box-sizing: border-box;
}
/* Nouveau : texte en bas */
.login-footer {
  position: absolute;
  bottom: 1rem; /* 1rem au-dessus du bas */
  left: 50%;
  transform: translateX(-50%);
  width: auto;
  z-index: 1;
  font-size: 1.5rem; /* passez de 0.9rem à 1.1rem ou plus */
  color: #333;
}
.login-footer {
  width: 100%;
  padding: 1rem 0.5rem;
  box-sizing: border-box;
  /* Pour éviter que ça touche les bords sur mobile */
}

.login-footer-text {
  display: block;
  font-family: "Poppins", sans-serif;
  font-size: 1.2rem;
  font-weight: 600;
  word-break: break-word; /* Pour couper les mots longs/urls si besoin */
  line-height: 1.4;
}

@media (max-width: 600px) {
  .login-footer-text {
    font-size: 1rem; /* Plus petit sur mobile */
    padding: 0 0.2rem;
  }
  .login-footer-mail {
    font-size: 0.9rem;
  }
}
.login-footer-mail {
  display: inline-block;
  font-weight: 400;
  font-size: 1rem;
}

/* ============================================
   Mobile-First Responsive Improvements
   ============================================ */

/* Touch-friendly form controls */
.login-card form .form-control {
  min-height: 44px;
  font-size: 16px; /* Previne zoom no iOS */
  padding: 0.75rem 1rem;
  border-radius: 8px;
}

.login-btn {
  min-height: 48px;
  font-size: 1rem;
  border-radius: 8px;
  padding: 0.75rem 2rem;
  -webkit-tap-highlight-color: transparent;
}

.login-btn:active {
  transform: scale(0.98);
}

/* Responsive title */
@media (max-width: 767.98px) {
  .login-card h2 {
    font-size: 2.2rem;
  }
  
  .login-card h2 small {
    font-size: 0.9rem;
  }
  
  .login-card {
    margin: 0 1rem;
    padding: 1.5rem 1rem;
  }
  
  .login-card form .form-control {
    max-width: 100%;
    width: 100%;
  }
}

@media (max-width: 575.98px) {
  .login-card h2 {
    font-size: 1.8rem;
  }
  
  .login-card h2 small {
    font-size: 0.8rem;
  }
  
  .login-card {
    border-width: 3px;
  }
}

/* Landscape mobile */
@media (max-height: 500px) and (orientation: landscape) {
  .login-card {
    padding: 0.75rem;
  }
  
  .login-card h2 {
    font-size: 1.5rem;
    margin-bottom: 0.5rem;
  }
  
  .login-card form .form-control {
    min-height: 38px;
    padding: 0.5rem 0.75rem;
    margin: 0.25rem auto;
  }
  
  .login-btn {
    min-height: 40px;
    padding: 0.5rem 1.5rem;
    margin-top: 0.5rem;
  }
  
  .login-footer {
    bottom: 0.5rem;
    padding: 0.5rem;
  }
  
  .login-footer-text {
    font-size: 0.8rem;
  }
}

/* Safe area for notch devices */
.login-page-bg {
  padding-top: env(safe-area-inset-top, 0);
  padding-bottom: env(safe-area-inset-bottom, 0);
}
</style>
