<template>
  <DefaultLayout
    :devices="devices"
    :selectedDevice="selectedDevice"
    :devicesWithCharts="devicesWithCharts"
    @select-device="selectDevice"
    @add-chart="addChart"
    @logout="logout"
    :showBack="true"
    @back="goSnapshot"
    sidebar-handle-top="0%"
  >
    <template #title> KropKharts </template>
    <template #subtitle> powered by KropKontrol </template>

    <!-- Mobile: mostrar nome do dispositivo (DPG) no header -->
    <template #mobile-chart-info>
      <span v-if="selectedDevice" class="mobile-device-name">
        {{ selectedDevice.description || selectedDevice.deviceName || 'DPG' }}
      </span>
      <span v-else class="mobile-app-title">KropKharts</span>
    </template>

    <!-- Mobile: botões de ação removidos para maximizar espaço do gráfico -->

    <div class="dashboard-container">
      <main class="d-flex flex-column chart-area">
        <template v-if="!selectedDevice">
          <div
            class="text-center text-muted mt-5 flex-grow-1 d-flex align-items-center justify-content-center"
          >
            Sélectionner un Kapteur dans la liste.
          </div>
        </template>

        <template v-else>
          <div
            v-if="!dataReady"
            class="text-center text-muted mt-5 flex-grow-1 d-flex align-items-center justify-content-center"
          >
            Chargement des données...
          </div>

          <template v-else>
            <div
              v-for="(c, idx) in filteredCharts"
              :key="c.devEui + '-' + c.suffix"
              class="chart-wrapper d-flex flex-column mb-1 flex-grow-1"
            >
              <LazyChartCard
                class="flex-grow-1 h-100"
                :devEui="c.devEui"
                :title="c.title"
                :labelMap="c.labelMap"
                :variables="c.variables"
                :intervalDays="c.intervalDays"
                :startDate="c.startDate"
                :endDate="c.endDate"
                :startDateTime="c.startDateTime"
                :endDateTime="c.endDateTime"
                :sun-annotation-offset="c.sunAnnotationOffset"
                :sunrise-annotation-offset="
                  c.sunriseAnnotationOffset ?? c.sunAnnotationOffset ?? 120
                "
                :sunset-annotation-offset="
                  c.sunsetAnnotationOffset ?? c.sunAnnotationOffset ?? 120
                "
                :base-time="dashboardNow"
                :device="devices.find((d) => d.devEui === c.devEui)"
                :suffix="c.suffix"
                :show-drainage="showDrainage"
                @delete="() => removeChart(c.suffix)"
                @duplicate="() => duplicateChart(c.suffix)"
                @updated="
                  (payload) => {
                    if (!isProd)
                      console.debug(
                        'Dashboard chart updated',
                        c.suffix,
                        payload,
                      );
                    updateChartConfig(c.suffix, payload);
                  }
                "
              />
            </div>
          </template>
        </template>
      </main>

      <!-- Rapport arrosage uniquement pour les Kapteurs type 47 -->
      <section
        v-if="selectedDevice && Number(selectedDevice.model) === 47"
        class="watering-report"
      >
        <WateringReportPanel :devEui="selectedDevice.devEui" />
      </section>

      <!-- Rapport climat pour sondes type 2 ou 7 -->
      <section
        v-if="selectedDevice && [2, 7].includes(Number(selectedDevice.model))"
        class="climate-report"
      >
        <ClimateReportPanel :devEui="selectedDevice.devEui" />
      </section>
    </div>
  </DefaultLayout>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch, inject } from "vue";
import debounce from "lodash/debounce";
import { useRoute } from "vue-router";
import useLocalStorage from "../composables/useLocalStorage.js";
import LazyChartCard from "../components/LazyChartCard.vue";
import { getLabelMap } from "../services/apiService.js";
import { useDevices } from "@/composables/useDevices.js";
import { useNavigation } from "../composables/useNavigation.js";
import { isRootUser } from "../services/roleUtils.js";
import DefaultLayout from "@/views/layout/DefaultLayout.vue";
import { useAuth } from "../composables/useAuth.js";
import { getUserIdFromToken } from "../services/authService.js";
import {
  LS_COORDS,
  LS_DASHBOARD_STATE_PREFIX,
  LS_TIMEZONE,
} from "../config/storageKeys.js";
import { useCharts } from "../composables/useCharts.js";
import { generateUniqueId } from "../utils/generateUniqueId.js";
import { getGroupLabelMap, getVirtualDefaultVariables } from "../utils/getGroupLabelMap.js";
import useDeviceData from "../composables/useDeviceData.js";
import { parseVarKey } from "../composables/useChartCalculations.js";
import { ensureCacheForLastNDays } from "../services/dataCacheService.js";
import WateringReportPanel from "../components/WateringReportPanel.vue";
import ClimateReportPanel from "../components/ClimateReportPanel.vue";
import {
  fetchDashboardStateFromApi,
  pushDashboardStateToApi,
  buildDashboardStatePayload,
  getLastDashboardSyncSignature,
  setLastDashboardSyncSignature,
  isDashboardHydrationInProgress,
  setDashboardHydrationState,
  getLastHydratedDashboardState,
  setLastHydratedDashboardState,
} from "@/services/dashboardStateService.js";

const { goSnapshot } = useNavigation();
const { logout } = useAuth();
const route = useRoute();
const isProd = import.meta.env.PROD;
const showDrainage = isRootUser();

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
  console.info(`[DashboardPage] ${message}`, ...args);
}

const STORAGE_KEY =
  LS_DASHBOARD_STATE_PREFIX + (getUserIdFromToken() ?? "guest");

const { devices: fetchedDevices, loadDevices } = useDevices();

// Rendez l'injection robuste (par défaut : liste vide)
const injectedVirtual = inject("virtualDevices", { virtualDevices: ref([]) });
const virtualDevices = injectedVirtual?.virtualDevices ?? ref([]);

// Protégez l’opération de spread si une des listes est encore undefined
const devices = computed(() => [
  ...(fetchedDevices.value ?? []),
  ...(virtualDevices.value ?? []),
]);

const selectedDevice = ref(null);

const charts = useLocalStorage(STORAGE_KEY, []);
const {
  addChart: addChartInner,
  removeChart: removeChartInner,
  duplicateChart: duplicateChartInner,
  updateChartConfig: updateChartConfigInner,
} = useCharts(charts, {
  idField: "suffix",
  duplicateId: (c) => generateUniqueId(`${c.devEui}-`, false),
});

const { fetchDeviceData } = useDeviceData();
const MIN_DATA_WINDOW_DAYS = 30;
const dataReady = ref(false);
const dashboardNow = ref(new Date());

// --- Migration automatique pour corriger les charts sans suffix ---
if (charts.value && Array.isArray(charts.value)) {
  let updated = false;
  charts.value.forEach((chart) => {
    if (!chart.suffix) {
      chart.suffix = generateUniqueId(`${chart.devEui}-`);
      updated = true;
    }
  });
  if (updated) {
    charts.value = [...charts.value]; // force save
  }
}

let refreshTimer = null;

async function pushDashboardStateNow() {
  if (isDashboardHydrationInProgress()) return;
  const payload = buildDashboardStatePayload({ charts: charts.value });
  const signature = JSON.stringify(payload);
  if (signature === getLastDashboardSyncSignature()) return;
  try {
    logDashboardDebug("Pushing dashboard state", {
      chartCount: payload.dashboardState?.length,
      coords: payload.coords,
      timezone: payload.timezone,
    });
    await pushDashboardStateToApi(payload);
    setLastDashboardSyncSignature(signature);
    setLastHydratedDashboardState(payload);
    logDashboardDebug("Dashboard state push succeeded");
  } catch (err) {
    console.error("Echec de la sauvegarde de l'etat dashboard:", err);
    logDashboardDebug("Dashboard state push failed", err);
  }
}

const scheduleDashboardStatePush = debounce(pushDashboardStateNow, 1000);

function queueDashboardStateSync({ immediate = false } = {}) {
  logDashboardDebug("Queue state sync", {
    immediate,
    hydrationInProgress: isDashboardHydrationInProgress(),
  });
  if (isDashboardHydrationInProgress()) return;
  if (immediate) {
    try {
      scheduleDashboardStatePush.cancel?.();
    } catch {}
    pushDashboardStateNow().catch((err) => {
      console.error("Echec de la sauvegarde immediate du dashboard:", err);
      logDashboardDebug("Immediate sync failed", err);
    });
  } else {
    scheduleDashboardStatePush();
  }
}

function applyDashboardStatePayload(payload, { updateSignature = true } = {}) {
  if (!payload || typeof payload !== "object") return false;

  logDashboardDebug("Applying dashboard payload", {
    updateSignature,
    payloadKeys: Object.keys(payload),
  });

  const remoteCharts = Array.isArray(payload.dashboardState)
    ? payload.dashboardState
    : Array.isArray(payload?.charts)
      ? payload.charts
      : null;

  const hydrationEventDetail = {};
  let hydrated = false;

  if (remoteCharts) {
    charts.value = remoteCharts;
    hydrated = true;
    logDashboardDebug("Charts applied", { count: remoteCharts.length });
  }

  const remoteCoords =
    payload.coords ?? payload.coordinates ?? payload.location ?? null;
  if (remoteCoords) {
    try {
      localStorage.setItem(LS_COORDS, JSON.stringify(remoteCoords));
    } catch (err) {
      console.warn("Impossible d'ecrire kk_coords depuis l'API", err);
    }
    hydrationEventDetail.coords = remoteCoords;
    hydrated = true;
    logDashboardDebug("Coords applied", remoteCoords);
  }

  const remoteTimezone =
    payload.timezone ?? payload.timeZone ?? payload.tz ?? null;
  if (remoteTimezone) {
    try {
      localStorage.setItem(LS_TIMEZONE, remoteTimezone);
    } catch (err) {
      console.warn("Impossible d'ecrire kk_tz depuis l'API", err);
    }
    hydrationEventDetail.timezone = remoteTimezone;
    hydrated = true;
    logDashboardDebug("Timezone applied", remoteTimezone);
  }

  if (
    Object.keys(hydrationEventDetail).length &&
    typeof window !== "undefined"
  ) {
    try {
      window.dispatchEvent(
        new CustomEvent("kk-dashboard-state-hydrated", {
          detail: hydrationEventDetail,
        }),
      );
    } catch (err) {
      console.warn("Echec de l'emission de l'evenement d'hydratation:", err);
      logDashboardDebug("Hydration event dispatch failed", err);
    }
  }

  if (hydrated) {
    setLastHydratedDashboardState({
      dashboardState: charts.value,
      coords: hydrationEventDetail.coords ?? remoteCoords ?? null,
      timezone: hydrationEventDetail.timezone ?? remoteTimezone ?? null,
    });
    if (updateSignature) {
      setLastDashboardSyncSignature(
        JSON.stringify(buildDashboardStatePayload({ charts: charts.value })),
      );
      logDashboardDebug("Dashboard signature updated after hydration", {
        chartCount: charts.value.length,
      });
    }
  } else {
    logDashboardDebug("Payload did not apply any dashboard state");
  }

  return hydrated;
}

function pushChart(chart) {
  addChartInner(chart);
  queueDashboardStateSync();
  logDashboardDebug("Chart added", chart);
}

function removeChart(id) {
  removeChartInner(id);
  queueDashboardStateSync();
  logDashboardDebug("Chart removed", id);
}

function duplicateChart(id) {
  duplicateChartInner(id);
  queueDashboardStateSync();
  logDashboardDebug("Chart duplicated", id);
}

function updateChartConfig(id, payload) {
  updateChartConfigInner(id, payload);
  queueDashboardStateSync();
  logDashboardDebug("Chart updated", { id, payload });
}

async function hydrateDashboardStateFromApi() {
  setDashboardHydrationState(true);
  try {
    const payload = await fetchDashboardStateFromApi();
    const result = applyDashboardStatePayload(payload);
    logDashboardDebug("Hydration from API finished", {
      applied: result,
      payload,
    });
    return result;
  } catch (err) {
    console.error("Echec du chargement de l'etat dashboard:", err);
    logDashboardDebug("Hydration from API failed", err);
    return false;
  } finally {
    setDashboardHydrationState(false);
  }
}

const filteredCharts = computed(() =>
  charts.value.filter(
    (c) => selectedDevice.value && c.devEui === selectedDevice.value.devEui,
  ),
);

// Set de devEuis que já têm gráficos (para esconder botão + na sidebar)
const devicesWithCharts = computed(() => 
  new Set(charts.value.map(c => c.devEui))
);

// Mobile: informações do gráfico ativo para o header
const hasActiveChart = computed(() => filteredCharts.value.length > 0);

// Funções para ações do gráfico no header mobile
function duplicateActiveChart() {
  if (filteredCharts.value.length > 0) {
    duplicateChart(filteredCharts.value[0].suffix);
  }
}

function deleteActiveChart() {
  if (filteredCharts.value.length > 0) {
    if (confirm('Êtes-vous sûr·e de vouloir supprimer ce graphique ?')) {
      removeChart(filteredCharts.value[0].suffix);
    }
  }
}

function toggleActiveChartFullscreen() {
  // Implementar fullscreen se necessário
  const chartCard = document.querySelector('.chart-card');
  if (chartCard) {
    if (document.fullscreenElement) {
      document.exitFullscreen();
    } else {
      chartCard.requestFullscreen?.();
    }
  }
}

async function refreshData(force = false) {
  dataReady.value = false;
  try {
    dashboardNow.value = new Date();
    const currentDevEui = selectedDevice.value?.devEui;

    await loadDevices(force).catch((err) => {
      if (!isProd) console.error("loadDevices failed", err);
    });

    if (currentDevEui) {
      selectedDevice.value =
        devices.value.find((d) => d.devEui === currentDevEui) || null;
    } else if (!selectedDevice.value && charts.value.length) {
      const devParam = route.query.devEui;
      const target = devParam || charts.value[0].devEui;
      selectedDevice.value =
        devices.value.find((d) => d.devEui === target) || null;
    }

    if (selectedDevice.value) {
      const dev = devices.value.find(
        (d) => d.devEui === selectedDevice.value.devEui,
      );
      if (dev) {
        refreshLabelMapForDevice(dev);

        const deviceCharts = charts.value.filter(
          (c) => c.devEui === dev.devEui,
        );

        if (deviceCharts.length) {
          const range = deviceCharts.reduce(
            ({ start, end }, c) => {
              const endDt = c.endDateTime
                ? new Date(c.endDateTime)
                : dashboardNow.value;
              const startDt = c.startDateTime
                ? new Date(c.startDateTime)
                : new Date(endDt.getTime() - (c.intervalDays || 0) * 864e5);
              const rangeDays = (endDt - startDt) / 86400000;
              const windowDays = Math.max(MIN_DATA_WINDOW_DAYS, rangeDays || 0);
              const windowStart = new Date(
                endDt.getTime() - windowDays * 864e5,
              );
              return {
                start: !start || windowStart < start ? windowStart : start,
                end: !end || endDt > end ? endDt : end,
              };
            },
            { start: null, end: null },
          );

          const needed = new Set();
          deviceCharts.forEach((c) => {
            (c.variables || []).forEach((v) => {
              const { devEui } = parseVarKey(v);
              if (devEui) needed.add(devEui);
            });
          });

          // Backfill cache (max 30 jours) pour le device sélectionné et les devEui nécessaires
          const ensureSet = new Set([dev.devEui, ...needed]);
          await Promise.all(
            [...ensureSet].map(async (de) => {
              let modelNum = Number(dev.model);
              if (dev.isVirtual) {
                const m = dev.group?.deviceModels?.[de];
                if (m != null) modelNum = Number(m);
              }
              if (!modelNum || Number.isNaN(modelNum)) return 0;
              try {
                return await ensureCacheForLastNDays(de, modelNum, 30);
              } catch {
                return 0;
              }
            }),
          );
          await fetchDeviceData(
            dev.devEui,
            dev.model,
            range.start.toISOString(),
            range.end.toISOString(),
            [...needed],
          ).catch((err) => {
            if (!isProd) console.error("fetchDeviceData failed", err);
          });
        }
      }
    }

    // déclenche la persistance si des références internes ont changé
    charts.value = [...charts.value];
  } finally {
    dataReady.value = true;
  }
}

onMounted(async () => {
  logDashboardDebug("DashboardPage mounted");
  let hasRemoteState = false;
  const cachedState = getLastHydratedDashboardState();
  if (cachedState) {
    logDashboardDebug("Applying cached dashboard state", cachedState);
    hasRemoteState =
      applyDashboardStatePayload(cachedState, { updateSignature: false }) ||
      hasRemoteState;
  }

  const fetchedState = await hydrateDashboardStateFromApi();
  hasRemoteState = hasRemoteState || fetchedState;
  logDashboardDebug("Hydration results", {
    fromCache: hasRemoteState && Boolean(cachedState),
    fromApi: fetchedState,
  });

  if (!hasRemoteState) {
    const initialPayload = buildDashboardStatePayload({ charts: charts.value });
    const hasLocalState =
      (Array.isArray(initialPayload.dashboardState) &&
        initialPayload.dashboardState.length > 0) ||
      Boolean(initialPayload.coords) ||
      Boolean(initialPayload.timezone);
    if (hasLocalState) {
      await pushDashboardStateNow();
      logDashboardDebug("No remote state found, pushed local state");
    }
  }
  await refreshData();
  refreshTimer = setInterval(() => refreshData(true), 15 * 60 * 1000);
});

watch(selectedDevice, (dev) => {
  if (import.meta.env.DEV)
    console.debug("Dashboard selected device", dev?.devEui);
});

watch(
  () => route.query.devEui,
  (val) => {
    if (val) {
      const dev = devices.value.find((d) => d.devEui === val);
      if (dev) selectDevice(dev);
    }
  },
);

let chartsSnapshot = JSON.stringify(charts.value);
watch(
  charts,
  (newCharts) => {
    queueDashboardStateSync();
    if (import.meta.env.DEV) {
      try {
        const serialized = JSON.stringify(newCharts);
        if (serialized !== chartsSnapshot) {
          console.debug("Dashboard charts changed", JSON.parse(serialized));
          chartsSnapshot = serialized;
        }
      } catch (err) {
        console.debug("Dashboard charts changed", newCharts, err);
      }
    }
    logDashboardDebug("Charts watcher triggered", {
      chartCount: newCharts?.length,
      hydrationInProgress: isDashboardHydrationInProgress(),
    });
  },
  { deep: true },
);

onUnmounted(() => {
  if (refreshTimer) {
    clearInterval(refreshTimer);
    refreshTimer = null;
  }
  try {
    scheduleDashboardStatePush.flush?.();
    scheduleDashboardStatePush.cancel?.();
  } catch {}
});

function uniqueArray(list = []) {
  if (!Array.isArray(list)) return [];
  return Array.from(new Set(list));
}

function arraysEqual(a = [], b = []) {
  if (!Array.isArray(a) || !Array.isArray(b)) return false;
  if (a.length !== b.length) return false;
  return a.every((value, index) => value === b[index]);
}

function refreshLabelMapForDevice(dev) {
  if (!dev) return;
  const devicesList = fetchedDevices.value ?? [];
  const isVirtual = !!dev.isVirtual;
  const map = isVirtual
    ? getGroupLabelMap(dev.group, devicesList)
    : getLabelMap(dev);
  const defaults = isVirtual
    ? uniqueArray(getVirtualDefaultVariables(dev.group, devicesList)).filter(
        (key) => map[key],
      )
    : [];

  charts.value
    .filter((c) => c.devEui === dev.devEui)
    .forEach((c) => {
      const payload = { labelMap: map };
      const currentVars = Array.isArray(c.variables) ? c.variables : [];
      if (isVirtual) {
        const cleaned = uniqueArray(currentVars.filter((key) => map[key]));
        const finalVars = cleaned.length ? cleaned : defaults;
        if (!arraysEqual(finalVars, currentVars)) {
          payload.variables = finalVars;
        }
      } else {
        const deduped = uniqueArray(currentVars);
        if (!arraysEqual(deduped, currentVars)) {
          payload.variables = deduped;
        }
      }
      updateChartConfig(c.suffix, payload);
    });
}

function selectDevice(device) {
  if (!device || selectedDevice.value?.devEui === device.devEui) return;
  if (import.meta.env.DEV) console.debug("Selecting device", device.devEui);
  selectedDevice.value = device;
  refreshLabelMapForDevice(device);
  charts.value = [...charts.value];
}

function addChart(deviceParam = null) {
  const dev = deviceParam || selectedDevice.value;
  if (!dev) return;

  if (!selectedDevice.value || selectedDevice.value.devEui !== dev.devEui) {
    selectDevice(dev);
  }

  let map;
  let defaultVariables = [];
  if (dev.isVirtual) {
    const devicesList = fetchedDevices.value ?? [];
    map = getGroupLabelMap(dev.group, devicesList);
    defaultVariables = uniqueArray(
      getVirtualDefaultVariables(dev.group, devicesList),
    ).filter((key) => map[key]);
  } else {
    map = getLabelMap(dev);
  }

  pushChart({
    devEui: dev.devEui,
    suffix: generateUniqueId(`${dev.devEui}-`, false),
    title: dev.description || dev.deviceName || dev.devEui,
    labelMap: map,
    variables: defaultVariables.length ? [...defaultVariables] : [],
    intervalDays: 1.5,
    startDate: "",
    endDate: "",
    startDateTime: null,
    endDateTime: null,
    sunAnnotationOffset: 120,
    sunriseAnnotationOffset: 120,
    sunsetAnnotationOffset: 120,
  });
}
</script>

<!-- *************************  CSS ************************* -->
<style scoped>
/* AJOUT : Le conteneur principal qui gère la disposition */
.dashboard-container {
  display: flex;
  flex-direction: column;
  /* Fait en sorte que le conteneur prenne au minimum toute la hauteur visible
     moins la hauteur approximative du header (ajustez 60px si besoin) */
  min-height: calc(100vh - 60px);
}

/* AJOUT : La zone qui contient le(s) graphique(s) */
.chart-area {
  flex-grow: 1;
  display: flex;
  flex-direction: column;
}

/* Mobile: gráfico ocupa toda a altura disponível */
@media (max-width: 767px) {
  .dashboard-container {
    min-height: calc(100vh - 44px); /* Menos header compacto */
    padding: 0;
  }
  
  .chart-area {
    min-height: calc(100vh - 44px);
  }
  
  .chart-wrapper {
    height: calc(100vh - 44px);
    min-height: calc(100vh - 44px);
    margin: 0 !important;
  }
  
  .chart-wrapper :deep(.chart-card) {
    height: 100%;
    border-radius: 0;
  }
  
  .chart-wrapper :deep(.card-body) {
    height: 100%;
  }
  
  .chart-wrapper :deep(.chart-container) {
    height: calc(100% - 40px); /* Menos barra de controlos */
  }
  
  .chart-wrapper :deep(.chart) {
    height: calc(100% - 30px) !important; /* Menos overview pequeno */
    min-height: 200px;
  }
}

/* Desktop: gráfico ocupa toda a altura com espaço para overview */
@media (min-width: 768px) {
  .chart-wrapper {
    height: calc(100vh - 130px);
    min-height: 500px;
  }
  
  .chart-wrapper :deep(.chart-card) {
    height: 100%;
    display: flex;
    flex-direction: column;
  }
  
  .chart-wrapper :deep(.card-body) {
    flex: 1 1 auto;
    display: flex;
    min-height: 0;
  }
  
  .chart-wrapper :deep(.chart-container) {
    flex: 1 1 auto;
    display: flex;
    flex-direction: column;
    min-height: 0;
  }
  
  .chart-wrapper :deep(.chart) {
    flex: 1 1 auto;
    min-height: 200px;
  }
  
  .chart-wrapper :deep(.overview-chart) {
    height: 100px !important;
    min-height: 100px !important;
    flex-shrink: 0 !important;
  }
}

.chart-wrapper:last-child,
.chart-wrapper:last-of-type {
  margin-bottom: 0 !important;
}

.title-container {
  text-align: center;
}

.watering-report {
  margin-top: 2rem;
  padding-top: 1rem;
  border-top: 1px solid #dee2e6;
  flex-shrink: 0; /* Empêche le tableau de se compresser */
}

.climate-report {
  margin-top: 2rem;
  padding-top: 1rem;
  border-top: 1px solid #dee2e6;
  flex-shrink: 0;
}

.title-container h1 {
  margin-top: 40px;
  font-size: 2rem;
}

.title-container small {
  margin-top: 5px;
  font-size: 0.9rem;
  color: #555;
}

:deep(.sidebar-handle) {
  transform: none;
}
.chart-wrapper:last-child,
.chart-wrapper:last-of-type {
  margin-bottom: 0 !important;
}

.title-container {
  text-align: center;
}

.watering-report {
  margin-top: 2rem;
  padding-top: 1rem;
  border-top: 1px solid #dee2e6;
}

.title-container h1 {
  margin-top: 40px;
  font-size: 2rem;
}

.title-container small {
  margin-top: 5px;
  font-size: 0.9rem;
  color: #555;
}

:deep(.sidebar-handle) {
  transform: none;
}

/* Mobile header chart info */
.mobile-device-name {
  font-size: 0.85rem;
  font-weight: 600;
  color: #fff;
}

.mobile-app-title {
  font-family: "Orbitron", sans-serif;
  font-size: 1rem;
  font-weight: bold;
  color: #fff;
}

/* Chart action buttons in header */
.chart-action-btn {
  padding: 0.2rem 0.4rem;
  min-width: 28px;
  min-height: 28px;
  font-size: 0.8rem;
  background: rgba(255, 255, 255, 0.15);
  border: 1px solid rgba(255, 255, 255, 0.3);
  color: #fff;
  border-radius: 4px;
  cursor: pointer;
  transition: all 0.15s ease;
}

.chart-action-btn:hover {
  background: rgba(255, 255, 255, 0.25);
}

.chart-action-btn.text-danger {
  color: #ff6b6b;
  border-color: rgba(255, 107, 107, 0.5);
}

.chart-action-btn.text-danger:hover {
  background: rgba(255, 107, 107, 0.2);
}
</style>
