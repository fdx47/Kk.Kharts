<template>
  <DefaultLayout
    sidebar-title="Choix du Graphique"
    :devices="sidebarCharts"
    :selectedDevice="selectedSidebarChart"
    @select-device="selectSidebarChart"
    showBack
    @back="goLanding"
    @logout="logout"
    @add-chart="toggleFormVisibility"
  >
    <template #title> <span>Graphiques personnalisés</span><br /> </template>
    <main class="d-flex flex-column p-3" style="height: 75vh">
      <form
        v-if="isFormVisible"
        @submit.prevent="addChart"
        class="mb-4 card card-body"
      >
        <div class="row align-items-end g-2 mb-3">
          <div class="col-12 col-md-5">
            <label class="form-label">Kapteur :</label>
            <select v-model="deviceToAdd" class="form-select">
              <option disabled value="">Sélectionner un Kapteur…</option>
              <option
                v-for="device in selectableDevices"
                :value="device.devEui"
                :key="device.devEui"
              >
                {{ device.description || device.deviceName || device.devEui }}
              </option>
            </select>
          </div>
          <div class="col-12 col-md-5" v-if="deviceToAdd">
            <label class="form-label">Variable :</label>
            <select v-model="variableToAdd" class="form-select">
              <option disabled value="">Sélectionner une variable…</option>
              <option
                v-for="(lbl, varKey) in availableVariables"
                :value="varKey"
                :key="varKey"
              >
                {{ lbl }}
              </option>
            </select>
          </div>
          <div class="col-12 col-md-2">
            <button
              type="button"
              class="btn btn-success w-100"
              :disabled="!deviceToAdd || !variableToAdd"
              @click="addSerie"
            >
              Ajouter la courbe
            </button>
          </div>
        </div>
        <div v-if="seriesToAdd.length" class="mb-3">
          <label class="form-label">Courbes sélectionnées :</label>
          <ul class="list-group">
            <li
              class="list-group-item d-flex justify-content-between align-items-center"
              v-for="(serie, idx) in seriesToAdd"
              :key="serie.devEui + '-' + serie.variable"
            >
              <span>{{ serie.label }}</span>
              <button
                type="button"
                class="btn btn-sm btn-outline-danger"
                @click="removeSerie(idx)"
              >
                Retirer
              </button>
            </li>
          </ul>
        </div>
        <button
          class="btn btn-success"
          type="submit"
          :disabled="!seriesToAdd.length"
        >
          Ajouter ce graphique (24h)
        </button>
      </form>

      <div
        v-if="visibleCharts.length === 0"
        class="text-center text-muted mb-3"
      >
        Aucun graphique affiché. Sélectionnez-en un dans la barre latérale ou
        créez-en un nouveau.
      </div>

      <div class="d-flex flex-column gap-4 flex-grow-1">
        <div
          v-for="chart in visibleCharts"
          :key="chart.id"
          class="chart-wrapper flex-grow-1 d-flex flex-column mb-1"
        >
          <LazyChartCard
            class="flex-grow-1 h-100"
            :title="chart.title"
            :series="chartSeriesMap[chart.id]"
            :seriesLabels="chart.series.map((s) => s.label)"
            :startDate="chart.startDate"
            :endDate="chart.endDate"
            :startDateTime="chart.startDateTime"
            :endDateTime="chart.endDateTime"
            :sun-annotation-offset="chart.sunAnnotationOffset"
            :sunrise-annotation-offset="
              chart.sunriseAnnotationOffset ?? chart.sunAnnotationOffset ?? 120
            "
            :sunset-annotation-offset="
              chart.sunsetAnnotationOffset ?? chart.sunAnnotationOffset ?? 120
            "
            :suffix="chart.id"
            :intervalDays="chart.activeInterval"
            :show-drainage="showDrainage"
            :showIntervalControls="true"
            @delete="() => removeChart(chart.id)"
            @duplicate="() => duplicateChart(chart.id)"
            @updated="
              (payload) => {
                if (!isProd)
                  console.debug('Specific chart updated', chart.id, payload);
                updateChartConfig(chart.id, payload);
              }
            "
            @refresh="() => refreshAllCharts(true)"
          />
        </div>
      </div>
    </main>
  </DefaultLayout>
</template>

<script setup>
import {
  ref,
  onMounted,
  onUnmounted,
  watch,
  reactive,
  computed,
  inject,
} from "vue";
import useLocalStorage from "../composables/useLocalStorage.js";
import LazyChartCard from "../components/LazyChartCard.vue";
import { getLabelMap } from "../services/apiService.js";
import { useDevices } from "@/composables/useDevices.js";
import {
  getUc502Wet150DataCached,
  getEm300ThDataCached,
  getUc502ModbusDataCached,
} from "@/services/dataCacheService.js";
import { useDeviceData } from "../composables/useDeviceData.js";
import DefaultLayout from "@/views/layout/DefaultLayout.vue";
import { useAuth } from "../composables/useAuth.js";
import { useCharts } from "../composables/useCharts.js";
import { toArray } from "../utils/toArray.js";
import { useNavigation } from "../composables/useNavigation.js";
import { generateUniqueId } from "../utils/generateUniqueId.js";
import { alignAndInterpolateSeries } from "../utils/interpolateSeries.js";
import { useComputedVars } from "../composables/useComputedVars.js";
import { getGroupLabelMap } from "../utils/getGroupLabelMap.js";
import { getUserIdFromToken } from "@/services/authService.js";
import { isRootUser } from "../services/roleUtils.js";
import { LS_SPECIFIC_CHARTS_PREFIX } from "@/config/storageKeys.js";

// Import des fonctions et composants nécessaires
const { logout } = useAuth();
const { goLanding } = useNavigation();
const { fetchDeviceData, getCachedDeviceData } = useDeviceData();
let refreshTimer = null;
const isProd = import.meta.env.PROD;
const showDrainage = isRootUser();
// Variable réactive pour la visibilité du formulaire
const isFormVisible = ref(false);
// Méthode pour afficher ou masquer le formulaire via le bouton du layout
function toggleFormVisibility() {
  isFormVisible.value = !isFormVisible.value;
}
const CHARTS_KEY =
  LS_SPECIFIC_CHARTS_PREFIX + (getUserIdFromToken() ?? "guest");
const { devices: fetchedDevices, loadDevices } = useDevices();
const { virtualDevices } = inject("virtualDevices");
const devices = computed(() => [
  ...fetchedDevices.value,
  ...virtualDevices.value,
]);
const selectableDevices = computed(() =>
  devices.value.filter((d) => !d.isVirtual),
);
const charts = useLocalStorage(CHARTS_KEY, []);
const {
  addChart: pushChart,
  removeChart: baseRemoveChart,
  duplicateChart: baseDuplicateChart,
  updateChartConfig: baseUpdateChartConfig,
} = useCharts(charts, {
  idField: "id",
  duplicateId: () => generateUniqueId("spec-"),
});

const selectedChartId = ref(null);
const sidebarCharts = computed(() =>
  charts.value.map((c) => ({ devEui: c.id, description: c.title })),
);
const selectedSidebarChart = computed(
  () =>
    sidebarCharts.value.find((s) => s.devEui === selectedChartId.value) || {
      devEui: "placeholder",
    },
);
const visibleCharts = computed(() => charts.value.filter((c) => c.visible));

const deviceToAdd = ref("");
const variableToAdd = ref("");
const seriesToAdd = ref([]);
const availableVariables = computed(() => {
  const dev = devices.value.find((d) => d.devEui === deviceToAdd.value);
  if (!dev) return {};
  const base = dev.isVirtual
    ? getGroupLabelMap(dev.group, fetchedDevices.value)
    : { ...getLabelMap(dev) };
  if (!dev.isVirtual) {
    const computedVars = useComputedVars(dev.model);
    Object.keys(computedVars).forEach((k) => {
      base[k] = computedVars[k].label;
    });
  }
  return base;
});
const startDate = ref("");
const endDate = ref("");
const chartSeriesMap = reactive({});
const showRangePicker = reactive({});
const customRange = reactive({});

// ouvre le sélecteur de dates
function openCustomRangePicker(id) {
  const chart = charts.value.find((c) => c.id === id);
  customRange[id] = { start: chart.startDate, end: chart.endDate };
  showRangePicker[id] = true;
}

// applique la plage personnalisée
function applyCustomRange(id, range) {
  const chart = charts.value.find((c) => c.id === id);
  chart.startDate = range.start;
  chart.endDate = range.end;
  chart.activeInterval = null;
  showRangePicker[id] = false;
  refreshAllCharts();
}

onMounted(async () => {
  if (!Array.isArray(fetchedDevices.value) || fetchedDevices.value.length === 0) {
    await loadDevices().catch(console.error);
  }
  const now = new Date();
  endDate.value = now.toISOString().slice(0, 10);
  startDate.value = new Date(now.getTime() - 6 * 24 * 60 * 60 * 1000)
    .toISOString()
    .slice(0, 10);
  charts.value.forEach((c) => {
    if (c.visible === undefined) c.visible = true;
  });
  const firstVisible = charts.value.find((c) => c.visible);
  charts.value.forEach((c) => {
    c.visible = c === firstVisible;
  });
  selectedChartId.value = firstVisible ? firstVisible.id : null;
  if (!charts.value.length) {
    isFormVisible.value = true;
  }
  await refreshAllCharts(true);
  refreshTimer = setInterval(() => refreshAllCharts(true), 15 * 60 * 1000);
});

watch(selectedChartId, (id) => {
  if (import.meta.env.DEV) {
    console.debug("Selected chart changed", id);
  }
});

onUnmounted(() => {
  if (refreshTimer) {
    clearInterval(refreshTimer);
    refreshTimer = null;
  }
});

function addSerie() {
  if (!deviceToAdd.value || !variableToAdd.value) return;
  if (
    seriesToAdd.value.some(
      (s) =>
        s.devEui === deviceToAdd.value && s.variable === variableToAdd.value,
    )
  )
    return;
  const dev = devices.value.find((d) => d.devEui === deviceToAdd.value);
  if (!dev) return;
  const compVars = useComputedVars(dev.model);
  const varLabel = dev.isVirtual
    ? getGroupLabelMap(dev.group, fetchedDevices.value)[variableToAdd.value] ||
      variableToAdd.value
    : compVars[variableToAdd.value]
      ? compVars[variableToAdd.value].label
      : getLabelMap(dev)[variableToAdd.value] || variableToAdd.value;
  seriesToAdd.value.push({
    devEui: deviceToAdd.value,
    variable: variableToAdd.value,
    label: dev?.isVirtual
      ? varLabel
      : `${dev?.description || dev?.deviceName || deviceToAdd.value} - ${varLabel}`,
    model: dev?.model,
  });
  variableToAdd.value = "";
}

function removeSerie(idx) {
  seriesToAdd.value.splice(idx, 1);
}

async function addChart() {
  if (!seriesToAdd.value.length) return;
  const suffix = generateUniqueId("spec-");
  // CORRECTION : Ajout de activeInterval pour le suivi de l'état du bouton
  const daysDiff = Math.round(
    (new Date(endDate.value) - new Date(startDate.value)) /
      (1000 * 60 * 60 * 24),
  );

  pushChart({
    id: suffix,
    title: seriesToAdd.value
      .map((s) => {
        const dev = devices.value.find((d) => d.devEui === s.devEui);
        const devLabel = dev?.description || dev?.deviceName || s.devEui;
        const compVars = useComputedVars(dev?.model);
        const varLabel = dev?.isVirtual
          ? getGroupLabelMap(dev.group, fetchedDevices.value)[s.variable] ||
            s.variable
          : compVars[s.variable]
            ? compVars[s.variable].label
            : getLabelMap(dev || {})[s.variable] || s.variable;
        if (dev?.isVirtual) {
          return varLabel;
        }
        return `${devLabel} - ${varLabel}`;
      })
      .join(" | "),
    series: [...seriesToAdd.value],
    startDate: startDate.value,
    endDate: endDate.value,
    startDateTime: null,
    endDateTime: null,
    activeInterval: daysDiff + 1, // +1 pour correspondre aux intervalles
    visible: true,
    sunAnnotationOffset: 120,
    sunriseAnnotationOffset: 120,
    sunsetAnnotationOffset: 120,
  });
  charts.value.forEach((c) => {
    if (c.id !== suffix) c.visible = false;
  });
  seriesToAdd.value = [];
  deviceToAdd.value = "";
  variableToAdd.value = "";
  selectedChartId.value = suffix;
  await refreshAllCharts(true);
  if (import.meta.env.DEV) {
    console.debug("Chart added", suffix);
  }
}

function removeChart(id) {
  if (import.meta.env.DEV) {
    console.debug("Chart removed", id);
  }
  baseRemoveChart(id);
  delete chartSeriesMap[id];
  if (selectedChartId.value === id) selectedChartId.value = null;
}

function duplicateChart(id) {
  const before = charts.value.length;
  baseDuplicateChart(id);
  if (charts.value.length > before) {
    const newChart = charts.value[charts.value.length - 1];
    newChart.visible = true;
    charts.value.forEach((c) => {
      if (c.id !== newChart.id) c.visible = false;
    });
    selectedChartId.value = newChart.id;
    refreshAllCharts();
    if (import.meta.env.DEV) {
      console.debug("Chart duplicated", id, "->", newChart.id);
    }
  }
}

function updateChartConfig(id, payload) {
  if ("intervalDays" in payload) payload.activeInterval = payload.intervalDays;
  if (import.meta.env.DEV) {
    console.debug("Chart config update", id, payload);
  }
  baseUpdateChartConfig(id, payload);
}

function selectSidebarChart(item) {
  if (import.meta.env.DEV) {
    console.debug("Select sidebar chart", item.devEui);
  }
  selectedChartId.value = item.devEui;
  charts.value.forEach((c) => {
    c.visible = c.id === item.devEui;
  });
}

function hideChart(id) {
  const chart = charts.value.find((c) => c.id === id);
  if (chart) {
    if (import.meta.env.DEV) {
      console.debug("Hide chart", id);
    }
    chart.visible = false;
    if (selectedChartId.value === id) selectedChartId.value = null;
  }
}

const MIN_DATA_WINDOW_DAYS = 7;
const COLUMN_VARS = ["volumeDelta", "volumeDelta_mm"];

async function getChartSeries(chart, force = false) {
  const allSeries = [];
  let displayStart, displayEnd;
  if (chart.startDateTime && chart.endDateTime) {
    displayStart = new Date(chart.startDateTime);
    displayEnd = new Date(chart.endDateTime);
  } else {
    displayStart = new Date(chart.startDate);
    displayStart.setHours(0, 0, 0, 0);
    displayEnd = new Date(chart.endDate);
    displayEnd.setHours(23, 59, 59, 999);
  }

  const requestedWindowDays =
    chart.startDateTime && chart.endDateTime
      ? (displayEnd - displayStart) / 86400000
      : chart.activeInterval || 1;
  const windowDays = Math.max(MIN_DATA_WINDOW_DAYS, requestedWindowDays);
  const fetchStart = new Date(displayEnd.getTime() - windowDays * 86400000);

  console.debug(
    "getChartSeries",
    chart.id,
    fetchStart.toISOString(),
    displayEnd.toISOString(),
  );

  const startDateISO = fetchStart.toISOString();
  const endDateISO = displayEnd.toISOString();

  for (const s of chart.series) {
    if (!s.devEui) continue;

    let apiResponse = [];
    try {
      if (force) {
        apiResponse = await fetchDeviceData(
          s.devEui,
          s.model,
          startDateISO,
          endDateISO,
        );
      } else {
        apiResponse = await getCachedDeviceData(
          s.devEui,
          s.model,
          startDateISO,
          endDateISO,
        );
      }
      if (import.meta.env.DEV) {
        console.debug(
          "Fetched",
          s.devEui,
          s.variable,
          apiResponse.length,
          "points",
        );
      }
      if (apiResponse.length) {
        const firstTs = apiResponse[0].timestamp;
        const lastTs = apiResponse[apiResponse.length - 1].timestamp;
        if (import.meta.env.DEV) {
          console.debug("Range", firstTs, lastTs);
        }
      }
    } catch (e) {
      console.error(`Error fetching data for ${s.devEui}:`, e);
      apiResponse = [];
    }

    const data = toArray(apiResponse); // ← toujours un tableau
    let compute;
    if (s.model != null) {
      const compVars = useComputedVars(s.model);
      compute = compVars[s.variable]?.compute;
    } else {
      const dev = devices.value.find((d) => d.devEui === s.devEui);
      if (dev?.isVirtual && dev.group) {
        for (const m of Object.values(dev.group.deviceModels || {})) {
          const defs = useComputedVars(m);
          if (defs[s.variable]?.compute) {
            compute = defs[s.variable].compute;
            break;
          }
        }
      }
    }

    const seriesData = data.map((d) => {
      const ts =
        d.timestamp instanceof Date
          ? d.timestamp.getTime()
          : new Date(d.timestamp).getTime();
      return {
        x: ts,
        y: compute ? compute(d) : d[s.variable],
      };
    });
    const isColumn = COLUMN_VARS.includes(s.variable);
    allSeries.push({
      name: s.label,
      data: seriesData,
      ...(isColumn ? { type: "column" } : {}),
    });
    if (import.meta.env.DEV) {
      console.debug("Series built for", s.devEui, s.variable);
    }
  }
  const aligned = alignAndInterpolateSeries(allSeries);
  if (import.meta.env.DEV) {
    console.debug(
      "Aligned series for",
      chart.id,
      aligned.map((s) => `${s.name}:${s.data.length}`),
    );
  }
  return aligned;
}

async function refreshAllCharts(force = false) {
  for (const chart of charts.value) {
    if (!chart.visible) continue;
    if (import.meta.env.DEV) {
      // Log the chart currently being refreshed to help with debugging
      console.debug("Refreshing chart", chart.id);
    }
    chartSeriesMap[chart.id] = await getChartSeries(chart, force);
    if (import.meta.env.DEV) {
      console.debug(
        "Series lengths",
        chart.id,
        chartSeriesMap[chart.id].map((s) => `${s.name}:${s.data.length}`),
      );
    }
  }
}

watch(
  charts,
  (newCharts) => {
    if (import.meta.env.DEV) {
      console.debug(
        "Specific charts changed",
        JSON.parse(JSON.stringify(newCharts)),
      );
    }
    refreshAllCharts();
  },
  { deep: true },
);
</script>

<style scoped>
.title-container {
  text-align: center;
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
</style>
