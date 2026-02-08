<template>
  <div ref="chartCardRef" class="chart-card card shadow position-relative">
    <!-- Header visível apenas em desktop (escondido em mobile e landscape pequeno) -->
    <div
      v-if="!hideCardHeader"
      class="card-header d-flex justify-content-between align-items-center py-2 px-3"
    >
      <h6 class="mb-0 chart-title">{{ displayedTitle }}</h6>
      <div class="header-actions">
        <button
          class="btn btn-sm btn-outline-secondary"
          title="Dupliquer"
          @click="emit('duplicate')"
        >
          <span class="btn-icon">📝</span>
        </button>
        <button
          class="btn btn-sm btn-outline-secondary"
          title="Plein écran"
          @click="toggleFullscreen"
        >
          <span class="btn-icon">🖥️</span>
        </button>
        <button
          class="btn btn-sm btn-outline-danger"
          title="Supprimer"
          @click="confirmDelete"
        >
          <span class="btn-icon">🗑️</span>
        </button>
      </div>
    </div>

    <div class="card-body d-flex p-0">
      <!-- Desktop: coluna lateral com controlos -->
      <!-- Mobile: barra horizontal com dias primeiro -->
      <div class="chart-controls-column p-2 border-end d-flex flex-column align-items-center">
        <!-- Interval pills PRIMEIRO em mobile (à esquerda) -->
        <div class="interval-pills" role="group">
          <button
            v-for="opt in intervalOptions"
            :key="opt.days"
            type="button"
            class="interval-pill"
            :class="{ active: intervalDays === opt.days }"
            @click="setIntervalAndUpdate(opt.days)"
            :title="opt.title"
          >
            {{ opt.text }}
          </button>
        </div>

        <!-- Tool buttons depois dos dias -->
        <div class="tool-buttons">
          <button
            v-if="!useCustomSeries"
            class="btn btn-sm btn-light tool-btn"
            @click="togglePanel('variables')"
            title="Variables"
          >
            🔧
          </button>
          <button
            class="btn btn-sm btn-light tool-btn"
            @click="togglePanel('calendar')"
            title="Calendrier"
          >
            📅
          </button>
          <button 
            class="btn btn-sm btn-light tool-btn" 
            @click="togglePanel('stats')"
            title="Statistiques"
          >
            📊
          </button>
          <button
            class="btn btn-sm btn-light tool-btn"
            @click="togglePanel('scale')"
            title="Ajuster les échelles"
          >
            ⚖️
          </button>
          <button
            class="btn btn-sm btn-light tool-btn"
            @click="togglePanel('sun')"
            title="Annotations soleil"
          >
            ☀️
          </button>
        </div>

        <button class="refresh-btn" @click="refreshChart" title="Actualiser">
          <i class="bi bi-arrow-clockwise"></i>
        </button>
      </div>

      <!-- Zone des panneaux de configuration -->
      <div v-if="showPanel" class="chart-panels-area p-2 border-start">
        <div v-if="activePanel === 'variables' && !useCustomSeries">
          <h6 class="small">Variables</h6>
          <div
            v-for="(lbl, key) in labelMapLocal"
            :key="key"
            class="form-check"
          >
            <input
              class="form-check-input"
              type="checkbox"
              :value="key"
              v-model="selectedVariables"
              :id="`cb-${key}`"
              @change="updateChartDebounced"
            />
            <label class="form-check-label" :for="`cb-${key}`">
              {{ lbl }}
            </label>
          </div>
        </div>

        <div v-else-if="activePanel === 'calendar'">
          <h6 class="small fw-bold mb-2">Dates personnalisées</h6>
          <label class="form-label small mb-1">
            Début :
            <input
              type="date"
              v-model="startDate"
              class="form-control form-control-sm"
            />
          </label>
          <label class="form-label small mb-1">
            Fin :
            <input
              type="date"
              v-model="endDate"
              class="form-control form-control-sm"
            />
          </label>
          <button class="btn btn-sm btn-primary mt-2" @click="applyCustomDates">
            Appliquer
          </button>
        </div>

        <div v-else-if="activePanel === 'scale'">
          <h6 class="small fw-bold mb-2">Ajuster les échelles</h6>
          <div v-if="!scaleVariables.length" class="small text-muted">
            Aucune variable ou série sélectionnée.<br />
            Sélectionnez des variables dans le panneau « Variables » ou
            fournissez des séries pour ajuster les échelles.
          </div>
          <div v-else>
            <div
              v-for="varKey in scaleVariables"
              :key="varKey"
              class="mb-2 scale-variable-item"
            >
              <label class="form-label small fw-bold mb-1">
                {{ labelMapLocal[varKey] || varKey }}
              </label>
              <div class="input-group input-group-sm mb-1">
                <span class="input-group-text" style="width: 45px"> Min: </span>
                <input
                  type="number"
                  step="any"
                  class="form-control scale-min-input"
                  :value="customScales[varKey]?.min"
                  @input="onScaleChange(varKey, 'min', $event.target.value)"
                  placeholder="auto"
                />
              </div>
              <div class="input-group input-group-sm">
                <span class="input-group-text" style="width: 45px"> Max: </span>
                <input
                  type="number"
                  step="any"
                  class="form-control scale-max-input"
                  :value="customScales[varKey]?.max"
                  @input="onScaleChange(varKey, 'max', $event.target.value)"
                  placeholder="auto"
                />
              </div>
              <button 
                type="button" 
                class="btn btn-sm btn-outline-secondary mt-1 w-100"
                @click="resetScale(varKey)"
                title="Remettre en auto-scale"
              >
                <i class="bi bi-arrow-counterclockwise"></i> Auto
              </button>
            </div>
          </div>
          <button 
            type="button" 
            class="btn btn-sm btn-outline-primary w-100 mt-2"
            @click="resetAllScales"
            title="Remettre toutes les échelles en auto"
          >
            <i class="bi bi-arrow-counterclockwise"></i> Reset toutes
          </button>
        </div>

        <div v-else-if="activePanel === 'sun'">
          <h6 class="small fw-bold mb-2">Annotations Soleil</h6>
          <label class="form-label small mb-1"> Décalage (minutes) : </label>
          <input
            type="number"
            class="form-control form-control-sm"
            v-model.number="sunriseAnnotationOffset"
          />
          <label class="form-label small mb-1"
            >Décalage coucher (minutes) :</label
          >
          <input
            type="number"
            class="form-control form-control-sm"
            v-model.number="sunsetAnnotationOffset"
          />
        </div>
      </div>

      <div class="flex-grow-1 p-2 chart-container">
        <div class="chart" ref="chartRef"></div>
      </div>
    </div>
    
    <!-- Overview chart FORA do card-body, abaixo das legendas -->
    <div 
      v-show="showOverview" 
      class="overview-chart-wrapper"
    >
      <div class="overview-chart" ref="overviewChartRef"></div>
      <!-- Toggle button para overview em mobile -->
      <button 
        v-if="hideCardHeader"
        class="overview-toggle"
        @click="showOverview = !showOverview"
        :title="showOverview ? 'Masquer navigation' : 'Afficher navigation'"
      >
        <i :class="showOverview ? 'bi bi-chevron-down' : 'bi bi-chevron-up'"></i>
        <span class="overview-toggle-text">{{ showOverview ? 'Masquer' : 'Nav' }}</span>
      </button>
    </div>

    <div
      v-if="activePanel === 'stats'"
      class="d-flex justify-content-center mt-3"
      style="width: 100%"
    >
      <div
        class="card p-3 shadow"
        style="
          background: #fff;
          max-width: 98%;
          min-width: 260px;
          border-radius: 14px;
        "
      >
        <h6 class="small fw-bold mb-3 text-center">
          {{ statsPeriodTitle }}
        </h6>
        <div v-if="!statsList.length" class="small text-muted text-center">
          Aucune donnée pour cette période.
        </div>
        <template v-else>
          <div v-for="stat in statsList" :key="stat.label" class="mb-3">
            <div class="small fw-bold mb-1 text-center">
              {{ stat.label }}
            </div>
            <table
              class="table table-sm table-bordered text-center mb-0"
              style="font-size: 0.97rem; background: #fff"
            >
              <thead>
                <tr>
                  <th style="width: 90px">Stat</th>
                  <th v-for="date in statsList.dayKeys" :key="date">
                    {{ new Date(date).toLocaleDateString("fr-FR") }}
                  </th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td>Min</td>
                  <td v-for="date in statsList.dayKeys" :key="'min-' + date">
                    {{ stat.stats[date]?.min ?? "-" }}
                  </td>
                </tr>
                <tr>
                  <td>Max</td>
                  <td v-for="date in statsList.dayKeys" :key="'max-' + date">
                    {{ stat.stats[date]?.max ?? "-" }}
                  </td>
                </tr>
                <tr>
                  <td>Moy</td>
                  <td v-for="date in statsList.dayKeys" :key="'avg-' + date">
                    {{ stat.stats[date]?.avg ?? "-" }}
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup>
import {
  ref,
  reactive,
  computed,
  watch,
  onMounted,
  onUnmounted,
  toRaw,
  nextTick,
} from "vue";
let ApexCharts;
import { useLocalStorage } from "../composables/useLocalStorage.js";
import { useResponsive } from "../composables/useResponsive.js";
import { useDeviceData } from "../composables/useDeviceData.js";
import { useComputedVars } from "../composables/useComputedVars.js";
import { alignAndInterpolateSeries } from "../utils/interpolateSeries.js";
import { useGeolocation } from "../composables/useGeolocation.js";
import { labelMapsByModelNumber } from "@/services/apiService.js"; // accès aux maps
import { MODELS } from "@/services/dataCacheService.js";
import {
  calculateDailyCumulativeValues,
  calculateCumulativeSeries,
} from "@/composables/useCumulativeCalculations.js";
import { generateUniqueId } from "../utils/generateUniqueId.js";
import { downsampleLTTB } from "../utils/lttb.js";
import debounce from "lodash/debounce";
import { LS_CUSTOM_SCALES_PREFIX } from "../config/storageKeys.js";
import {
  useChartCalculations,
  parseVarKey,
  formatDateTime,
  clearSunAnnotationsCache,
} from "../composables/useChartCalculations.js";

const SEP = "|";
const DRAINAGE_KEY = "drainagePercent";
const DRAINAGE_MODEL_IDS = new Set(
  [MODELS.UC502_WET150, MODELS.UC502_MULTI_WET150].filter(
    (value) => typeof value === "number" && !Number.isNaN(value),
  ),
);
const parseModelToId = (value) => {
  if (value == null) return undefined;
  const parsed = Number.parseInt(String(value).split("|")[0], 10);
  return Number.isNaN(parsed) ? undefined : parsed;
};
const isDrainageModel = (value) => {
  const parsed = parseModelToId(value);
  return parsed != null && DRAINAGE_MODEL_IDS.has(parsed);
};
const isDrainageVarKey = (key) =>
  key === DRAINAGE_KEY || String(key).startsWith(`${DRAINAGE_KEY}${SEP}`);
const COLUMN_VARS = ["volumeDelta", "volumeDelta_mm"];
const COLUMN_WIDTH = "8px"; // ou '28px' pour une largeur fixe

// Nombre de jours minimal de données à récupérer par défaut. Le
// téléchargement s'adapte automatiquement à la plage actuellement
// sélectionnée afin de ne pas couper les graphiques lorsque
// l'utilisateur choisit une période plus longue.
const MIN_DATA_WINDOW_DAYS = 30;
const MAX_SERIES_POINTS = 7000; // Seuil de downsampling pour les séries
const MAX_OVERVIEW_POINTS = 750; // Limite de points pour l'aperçu

// Web worker pour pré-calculer les séries côté thread de fond
let overviewWorker = null;
function getOverviewWorker() {
  if (!overviewWorker) {
    try {
      overviewWorker = new Worker(
        new URL("../workers/chartWorker.js", import.meta.url),
        { type: "module" },
      );
    } catch (err) {
      console.warn("Worker non disponible:", err);
      overviewWorker = null;
    }
  }
  return overviewWorker;
}

function pad(n) {
  return n < 10 ? "0" + n : "" + n;
}

const { coords, zone, requestLocation } = useGeolocation();
const { fetchDeviceData, getCachedDeviceData } = useDeviceData();
// Map inverse "libellé -> clé technique"
const inverseLabelMap = Object.fromEntries(
  Object.values(labelMapsByModelNumber).flatMap((map) =>
    Object.entries(map).map(([k, v]) => [v, k]),
  ),
);

const props = defineProps({
  devEui: String,
  suffix: String,
  title: String,
  labelMap: Object,
  variables: { type: Array, default: () => [] },
  startDate: {
    type: String,
    default: () => new Date(Date.now() - 86400000).toISOString().slice(0, 10),
  },
  endDate: {
    type: String,
    default: () => new Date().toISOString().slice(0, 10),
  },
  startDateTime: { type: [String, Date], default: null },
  endDateTime: { type: [String, Date], default: null },
  intervalDays: { type: Number, default: () => 1.5 },
  showIntervalControls: { type: Boolean, default: undefined },
  device: { type: Object, default: () => ({}) },
  series: { type: Array, default: null },
  seriesLabels: { type: Array, default: () => [] },
  showDrainage: { type: Boolean, default: false },
  // Legacy single offset kept for backward compatibility
  sunAnnotationOffset: { type: Number, default: 120 },
  sunriseAnnotationOffset: { type: Number, default: 120 },
  sunsetAnnotationOffset: { type: Number, default: 120 },
  baseTime: { type: [Date, String], default: null },
});
const emit = defineEmits(["delete", "duplicate", "updated", "refresh"]);

const useCustomSeries = computed(() => !props.devEui);
const showIntervalControls = computed(() =>
  props.showIntervalControls !== undefined
    ? props.showIntervalControls
    : !useCustomSeries.value,
);
let customScales = ref({});
if (!useCustomSeries.value) {
  const LS_KEY = `${LS_CUSTOM_SCALES_PREFIX}${props.devEui}_${props.suffix}`;
  customScales = useLocalStorage(LS_KEY, {});
}

const selectedVariables = ref([...props.variables]);
const supportsDrainagePercent = computed(() => {
  if (!props.showDrainage) return false;
  const baseMap = props.labelMap || {};
  const hasDrainageLabel = Object.keys(baseMap).some((key) =>
    String(key).startsWith(DRAINAGE_KEY),
  );
  if (hasDrainageLabel) return true;

  if (isDrainageModel(props.device?.model ?? props.device?.deviceId)) {
    return true;
  }

  if (props.device?.isVirtual) {
    const models = props.device?.group?.deviceModels;
    if (!models) return false;
    return Object.values(models).some((modelNumber) =>
      isDrainageModel(modelNumber),
    );
  }

  return false;
});
const labelMapLocal = reactive({ ...props.labelMap });
const syncDrainageVisibility = (enabled) => {
  if (!enabled) {
    Object.keys(labelMapLocal)
      .filter((key) => isDrainageVarKey(key))
      .forEach((key) => {
        delete labelMapLocal[key];
      });
    if (selectedVariables.value.length) {
      selectedVariables.value = selectedVariables.value.filter(
        (key) => !isDrainageVarKey(key),
      );
    }
    return;
  }

  Object.keys(props.labelMap || {})
    .filter((key) => isDrainageVarKey(key))
    .forEach((key) => {
      if (!labelMapLocal[key]) {
        labelMapLocal[key] = props.labelMap[key];
      }
    });

  if (!labelMapLocal[DRAINAGE_KEY]) {
    labelMapLocal[DRAINAGE_KEY] =
      props.labelMap?.[DRAINAGE_KEY] ?? "Cumul Δ perm";
  }
};
syncDrainageVisibility(supportsDrainagePercent.value);
watch(supportsDrainagePercent, (enabled) => {
  syncDrainageVisibility(enabled);
});
const startDate = ref(props.startDate);
const endDate = ref(props.endDate);
const startDateTime = ref(
  props.startDateTime ? new Date(props.startDateTime) : null,
);
const endDateTime = ref(props.endDateTime ? new Date(props.endDateTime) : null);
const customDatesActive = ref(false);
const intervalDays = ref(props.intervalDays);
const sunriseAnnotationOffset = ref(
  props.sunriseAnnotationOffset ?? props.sunAnnotationOffset,
);
const sunsetAnnotationOffset = ref(
  props.sunsetAnnotationOffset ?? props.sunAnnotationOffset,
);
const fixedBaseTime = ref(
  props.baseTime ? new Date(props.baseTime) : new Date(),
);

// Responsividade
const { isMobile, isLandscape, isTouchDevice } = useResponsive();

// Em landscape mobile, esconder overview por defeito para dar mais espaço ao gráfico
const isLandscapeMobile = computed(() => 
  isLandscape.value && isTouchDevice.value && window.innerHeight < 500
);

// Em mobile: NUNCA mostrar overview (rect não é usado, interações só por botões)
// Em desktop: mostrar overview
const showOverview = ref(!isMobile.value && !isTouchDevice.value);

// Esconder header em mobile (portrait ou landscape com altura pequena)
const hideCardHeader = computed(() => {
  if (isMobile.value) return true;
  if (isLandscapeMobile.value) return true;
  return false;
});

// Computed para detectar mobile (usado em useChartCalculations)
const isMobileDevice = computed(() => isMobile.value || isTouchDevice.value);

const showPanel = ref(false);
const activePanel = ref(null);
const statsPeriodTitle = ref("");
const statsList = ref([]);
const chartCardRef = ref(null);
const chartRef = ref(null);
const chartInstance = ref(null);
const overviewChartRef = ref(null);
const overviewChartInstance = ref(null);
const overviewData = ref([]);
const mainId = generateUniqueId("main-");
const overviewId = generateUniqueId("overview-");
const currentData = ref([]);
const currentPeriodStart = ref(null);
const currentPeriodEnd = ref(null);
let computedVarDefs = {};
if (props.device?.isVirtual && props.device?.group) {
  props.device.group.devEuis.forEach((devEui) => {
    const model = props.device.group.deviceModels?.[devEui];
    if (model != null) {
      const defs = useComputedVars(Number(model), {
        showDrainage: props.showDrainage,
      });
      Object.keys(defs).forEach((k) => {
        computedVarDefs[`${k}${SEP}${devEui}`] = defs[k];
        if (!labelMapLocal[`${k}${SEP}${devEui}`]) {
          labelMapLocal[`${k}${SEP}${devEui}`] = `${defs[k].label}`;
        }
      });
    }
  });
} else {
  const defs = useComputedVars(props.device?.model, {
    showDrainage: props.showDrainage,
  });
  Object.assign(computedVarDefs, defs);
  Object.keys(defs).forEach((k) => {
    if (!labelMapLocal[k]) labelMapLocal[k] = defs[k].label;
  });
}
if (!props.device?.isVirtual && Number(props.device?.model) === 61) {
  labelMapLocal.DLI = "DLI (mol/m²)";
  labelMapLocal.Rayonnement = "Rayonnement (J/cm²)";
}
const {
  computeStats,
  updateChart,
  updateCustomSeriesChart,
  updateCustomOverviewChart,
} = useChartCalculations({
  props,
  useCustomSeries,
  labelMapLocal,
  inverseLabelMap,
  computedVarDefs,
  selectedVariables,
  startDate,
  endDate,
  startDateTime,
  endDateTime,
  intervalDays,
  customDatesActive,
  customScales,
  chartInstance,
  overviewChartInstance,
  currentData,
  statsPeriodTitle,
  statsList,
  zone,
  coords,
  sunriseAnnotationOffset,
  sunsetAnnotationOffset,
  currentPeriodStart,
  currentPeriodEnd,
  fetchDeviceData,
  getCachedDeviceData,
  emitUpdated,
  MIN_DATA_WINDOW_DAYS,
  MAX_SERIES_POINTS,
  baseTimeRef: fixedBaseTime,
});

const updateChartDebounced = debounce(updateChart, 600);
const updateOverviewChartDebounced = debounce(updateOverviewChart, 600);

// Atualizar baseTime quando a prop muda (refresh automático)
watch(
  () => props.baseTime,
  (newVal) => {
    if (newVal) {
      const newTime = new Date(newVal);
      // Só atualiza se a diferença for significativa (> 3 minuto)
      if (Math.abs(newTime.getTime() - fixedBaseTime.value.getTime()) > 1800000) {
        fixedBaseTime.value = newTime;
        if (chartInstance.value && !customDatesActive.value) {
          updateChartDebounced();
        }
      }
    }
  }
);

// Keep internal state in sync when parent props change
watch(
  [
    () => props.startDate,
    () => props.startDateTime,
    () => props.endDate,
    () => props.endDateTime,
    () => props.intervalDays,
    () => props.sunAnnotationOffset,
    () => props.sunriseAnnotationOffset,
    () => props.sunsetAnnotationOffset,
  ],
  ([sd, sdt, ed, edt, intDays, sunOffsetLegacy, sunriseOff, sunsetOff]) => {
    startDate.value = sd;
    startDateTime.value = sdt != null ? new Date(sdt) : null;
    if (startDateTime.value) {
      startDateTime.value.setHours(0, 0, 0, 0);
      startDate.value = startDateTime.value.toISOString().slice(0, 10);
    }

    endDate.value = ed;
    endDateTime.value = edt != null ? new Date(edt) : null;
    if (endDateTime.value) {
      endDateTime.value.setHours(23, 59, 59, 999);
      endDate.value = endDateTime.value.toISOString().slice(0, 10);
    }

    intervalDays.value = intDays;
    sunriseAnnotationOffset.value =
      sunriseOff != null ? sunriseOff : sunOffsetLegacy;
    sunsetAnnotationOffset.value =
      sunsetOff != null ? sunsetOff : sunOffsetLegacy;

    if (chartInstance.value) {
      if (useCustomSeries.value) updateCustomSeriesChart(props.series);
      else updateChartDebounced();
    }
  },
);
watch(sunriseAnnotationOffset, () => {
  clearSunAnnotationsCache();
  if (chartInstance.value) {
    if (useCustomSeries.value) updateCustomSeriesChart(props.series);
    else updateChart();
  }
});
watch(sunsetAnnotationOffset, () => {
  clearSunAnnotationsCache();
  if (chartInstance.value) {
    if (useCustomSeries.value) updateCustomSeriesChart(props.series);
    else updateChart();
  }
});
watch([zone, () => coords.value.lat, () => coords.value.lon], () => {
  clearSunAnnotationsCache();
});

watch(currentData, () => {
  if (!useCustomSeries.value) updateOverviewChartDebounced();
});

watch(
  [
    startDate,
    endDate,
    startDateTime,
    endDateTime,
    intervalDays,
    customDatesActive,
  ],
  () => {
    if (!overviewChartInstance.value) return;
    let start, end;
    if (customDatesActive.value) {
      start = startDateTime.value
        ? new Date(startDateTime.value)
        : new Date(startDate.value);
      end = endDateTime.value
        ? new Date(endDateTime.value)
        : new Date(endDate.value);
    } else {
      end = new Date(fixedBaseTime.value);
      start = new Date(end.getTime() - intervalDays.value * 864e5);
    }
    overviewChartInstance.value.updateOptions({
      chart: {
        selection: { xaxis: { min: start.getTime(), max: end.getTime() } },
      },
    });
  },
);

// ————————————————————————————————————————————————————————————————
// 1) Construire la map inverse : "libellé" -> "clé technique"

const chartSeries = computed(() => {
  if (useCustomSeries.value) {
    return props.series || [];
  }

  // 2) Normaliser la liste : remplace chaque libellé par sa clé technique
  const techKeys = (
    selectedVariables.value.length ? selectedVariables.value : []
  ).map((k) => inverseLabelMap[k] ?? k);

  return techKeys.map((techKey, i) => {
    const { field, devEui } = parseVarKey(techKey);
    const filtered = devEui
      ? currentData.value.filter((d) => d.devEui === devEui)
      : currentData.value;
    const data = filtered
      .map((d) => {
        const comp =
          computedVarDefs[techKey] ||
          computedVarDefs[`${field}${SEP}${d.devEui}`];
        const val = comp ? comp.compute(d) : d[field];
        if (val == null || Number.isNaN(val)) return null;
        const ts =
          d.timestamp instanceof Date
            ? d.timestamp.getTime()
            : new Date(d.timestamp).getTime();
        return { x: ts, y: val };
      })
      .filter(Boolean); // retire les « null »
    return { name: labelMapLocal[techKey] || techKey, data, yAxisIndex: i };
  });
});

const scaleVariables = computed(() => {
  if (useCustomSeries.value) {
    if (props.seriesLabels && props.seriesLabels.length) {
      return props.seriesLabels;
    }
    return (props.series || []).map((s) => s.name);
  }
  return selectedVariables.value.map((k) => inverseLabelMap[k] ?? k);
});

const intervalOptions = [
  { days: 1.5, text: "36h", title: "36 heures" },
  { days: 3, text: "3J", title: "3 jours" },
  { days: 5, text: "5J", title: "5 jours" },
  { days: 7, text: "7J", title: "7 jours" },
  { days: 15, text: "15J", title: "15 jours" },
  { days: 30, text: "30J", title: "30 jours" },
];

// Base title uses the device description if available so it updates when the
// device name changes
const baseTitle = computed(() => {
  if (useCustomSeries.value) return props.title;
  return props.device?.description || props.device?.deviceName || props.title;
});

const displayedTitle = computed(() => {
  if (useCustomSeries.value) return baseTitle.value;
  const base = baseTitle.value;
  if (!selectedVariables.value.length) return base;
  const vars = selectedVariables.value
    .map((key) => labelMapLocal[key] || key)
    .join(" – ");
  return `${base} – ${vars}`;
});

function confirmDelete() {
  if (
    confirm(
      "Êtes-vous sûr·e de vouloir supprimer ce graphique ? Cette action est irréversible.",
    )
  ) {
    emit("delete");
  }
}

function emitUpdated() {
  emit("updated", {
    devEui: props.devEui,
    variables: [...selectedVariables.value],
    intervalDays: intervalDays.value,
    startDate: startDate.value,
    endDate: endDate.value,
    startDateTime: startDateTime.value
      ? startDateTime.value.toISOString()
      : null,
    endDateTime: endDateTime.value ? endDateTime.value.toISOString() : null,
    sunriseAnnotationOffset: sunriseAnnotationOffset.value,
    sunsetAnnotationOffset: sunsetAnnotationOffset.value,
    // Legacy field for backward compatibility
    sunAnnotationOffset: sunriseAnnotationOffset.value,
  });
}

function refreshChart() {
  fixedBaseTime.value = new Date();
  emit("refresh");
  updateChart();
}

watch(
  () => props.series,
  (newSeries) => {
    if (useCustomSeries.value) {
      updateCustomSeriesChart(newSeries);
    }
  },
  { deep: true },
);

function togglePanel(panel) {
  if (useCustomSeries.value && panel === "variables") return;
  activePanel.value = activePanel.value === panel ? null : panel;
  showPanel.value = !!activePanel.value;
}

function toggleFullscreen() {
  const { chartRef, chartCardRef } = chartCardRef.value;
  if (!document.fullscreenElement) chartCardRef.requestFullscreen();
  else document.exitFullscreen();
}

function onScaleChange(varKey, bound, value) {
  if (!customScales.value[varKey]) customScales.value[varKey] = {};
  customScales.value[varKey][bound] = value !== "" ? Number(value) : "";
  if (useCustomSeries.value) {
    updateCustomSeriesChart(props.series);
  } else {
    updateChart();
  }
}

function resetScale(varKey) {
  if (customScales.value[varKey]) {
    delete customScales.value[varKey];
    customScales.value = { ...customScales.value }; // trigger reactivity
  }
  if (useCustomSeries.value) {
    updateCustomSeriesChart(props.series);
  } else {
    updateChart();
  }
}

function resetAllScales() {
  customScales.value = {};
  if (useCustomSeries.value) {
    updateCustomSeriesChart(props.series);
  } else {
    updateChart();
  }
}

watch(activePanel, (p) => {
  if (p === "stats") computeStats();
});

// Recalculate stats when data or selected variables change while the stats
// panel is active
watch([currentData, selectedVariables], () => {
  if (activePanel.value === "stats") {
    computeStats();
  }
});

function setIntervalAndUpdate(days) {
  intervalDays.value = days;
  customDatesActive.value = false;
  startDateTime.value = null;
  endDateTime.value = null;
  const now = new Date(fixedBaseTime.value);
  endDate.value = now.toISOString().slice(0, 10);
  const start = new Date(now.getTime() - days * 24 * 60 * 60 * 1000);
  startDate.value = start.toISOString().slice(0, 10);
  updateChart(true);
}

function applyCustomDates() {
  customDatesActive.value = true;
  startDateTime.value = new Date(startDate.value);
  startDateTime.value.setHours(0, 0, 0, 0);
  endDateTime.value = new Date(endDate.value);
  endDateTime.value.setHours(23, 59, 59, 999);
  updateChart();
}

async function updateOverviewChart() {
  if (!props.devEui || useCustomSeries.value) return;
  if (!overviewChartInstance.value) return;
  if (!selectedVariables.value.length) {
    overviewChartInstance.value.updateOptions({ series: [] });
    return;
  }
  const end = currentPeriodEnd.value
    ? new Date(currentPeriodEnd.value)
    : new Date(fixedBaseTime.value);
  const rangeDays = Math.max(MIN_DATA_WINDOW_DAYS, intervalDays.value);
  const start = new Date(end.getTime() - rangeDays * 24 * 60 * 60 * 1000);
  const res = currentData.value.filter((d) => {
    const ts =
      d._ts ??
      (d.timestamp instanceof Date
        ? d.timestamp.getTime()
        : Date.parse(d.timestamp));
    return ts >= start.getTime() && ts <= end.getTime();
  });
  overviewData.value = res;
  const keys = selectedVariables.value.length
    ? selectedVariables.value
    : Object.keys(labelMapLocal);

  // Tentative via Web Worker (fallback ci-dessous si indisponible)
  try {
    const w = getOverviewWorker();
    if (w) {
      const id = `${Date.now()}_${Math.random()}`;
      const records = res.map((d) => ({
        ...d,
        _ts:
          d._ts ??
          (d.timestamp instanceof Date
            ? d.timestamp.getTime()
            : Date.parse(d.timestamp)),
      }));
      const deviceModels = props.device?.group?.deviceModels || {};
      const defaultModel = props.device?.model;
      const safeKeys = Array.from(keys);
      const seriesByKey = await new Promise((resolve, reject) => {
        const handler = (ev) => {
          const msg = ev.data || {};
          if (msg.id !== id) return;
          w.removeEventListener("message", handler);
          if (msg.ok) {
            clearTimeout(to);
            resolve(msg.seriesByKey);
          } else {
            clearTimeout(to);
            reject(new Error(msg.error || "Worker error"));
          }
        };
        let to;
        w.addEventListener("message", handler);
        const safeRecords = (() => {
          const rr = res.map((d) => ({
            ...d,
            _ts:
              d._ts ??
              (d.timestamp instanceof Date
                ? d.timestamp.getTime()
                : Date.parse(d.timestamp)),
          }));
          try {
            return structuredClone(rr);
          } catch {
            return JSON.parse(JSON.stringify(rr));
          }
        })();
        const dm = props.device?.group?.deviceModels || {};
        const safeDeviceModels = Object.keys(dm).reduce((acc, k) => {
          acc[k] = Number(dm[k]);
          return acc;
        }, {});
        w.postMessage({
          id,
          cmd: "buildSeries",
          records: safeRecords,
          keys: safeKeys,
          deviceModels: safeDeviceModels,
          defaultModel: Number(defaultModel),
          showDrainage: !!props.showDrainage,
        });
        // Timeout de sécurité
        to = setTimeout(() => {
          try {
            w.removeEventListener("message", handler);
          } catch {}
          reject(new Error("Worker timeout"));
        }, 8000);
      });
      const series = [];
      keys.forEach((varKey, i) => {
        const { field } = parseVarKey(varKey);
        let data = seriesByKey[varKey] || [];
        if (data.length > MAX_OVERVIEW_POINTS)
          data = downsampleLTTB(data, MAX_OVERVIEW_POINTS);
        const isColumn = COLUMN_VARS.includes(field);
        series.push({
          name: labelMapLocal[varKey] || varKey,
          data,
          yAxisIndex: i,
          ...(isColumn ? { type: "column" } : {}),
        });
      });

      // Multi Y axes if several curves
      const yaxis =
        series.length > 1
          ? series.map(() => ({
              show: true,
              opposite: false,
              title: { text: "" },
              labels: { show: false },
            }))
          : [{ show: true, title: { text: "" }, labels: { show: false } }];

      const strokeWidths = series.map((s) => (s.type === "column" ? 0 : 1));
      const fillOpacities = series.map((s) => (s.type === "column" ? 1 : 0));
      // Mise à jour légère: options minimales puis séries
      overviewChartInstance.value.updateOptions({
        legend: { show: false },
        fill: { type: "solid", opacity: fillOpacities },
        stroke: { width: strokeWidths },
        yaxis,
        plotOptions: { bar: { columnWidth: COLUMN_WIDTH } },
      });
      overviewChartInstance.value.updateSeries(series, false);

      let selStart;
      let selEnd;
      if (customDatesActive.value) {
        selEnd = endDateTime.value
          ? endDateTime.value.getTime()
          : new Date(endDate.value).getTime();
        selStart = startDateTime.value
          ? startDateTime.value.getTime()
          : new Date(startDate.value).getTime();
      } else {
        selEnd = end.getTime();
        selStart = end.getTime() - intervalDays.value * 864e5;
      }
      overviewChartInstance.value.updateOptions({
        chart: { selection: { xaxis: { min: selStart, max: selEnd } } },
      });
      return; // succès worker => on sort
    }
  } catch (err) {
    console.warn("Worker overview échoué, fallback local:", err);
  }

  // Pré-indexation par devEui pour éviter des filtres répétés (fallback local)
  const byDev = new Map();
  for (const d of res) {
    const k = d.devEui || "__default__";
    if (!byDev.has(k)) byDev.set(k, []);
    byDev.get(k).push(d);
  }
  // Regroupement des variables par devEui ciblé
  const keysByDev = new Map();
  keys.forEach((k) => {
    const { devEui } = parseVarKey(k);
    const bucket = devEui || "__default__";
    if (!keysByDev.has(bucket)) keysByDev.set(bucket, []);
    keysByDev.get(bucket).push(k);
  });

  const resultMap = new Map(); // varKey -> array of {x,y}
  for (const [bucket, vars] of keysByDev.entries()) {
    const devData = bucket === "__default__" ? res : byDev.get(bucket) || [];
    if (!devData.length) {
      vars.forEach((vk) => resultMap.set(vk, []));
      continue;
    }
    // Préparer les compute fns par varKey
    const computes = new Map();
    let needCum61 = false;
    vars.forEach((vk) => {
      const { field, devEui } = parseVarKey(vk);
      const devModel = devEui
        ? props.device.group?.deviceModels?.[devEui]
        : props.device?.model;
      if (
        Number(devModel) === 61 &&
        (field === "DLI" || field === "Rayonnement")
      ) {
        computes.set(vk, { type: "cum61", field });
        needCum61 = true;
      } else {
        const comp =
          computedVarDefs[vk] ||
          computedVarDefs[
            `${field}${SEP}${bucket === "__default__" ? "" : bucket}`
          ];
        if (comp?.compute)
          computes.set(vk, { type: "comp", field, fn: comp.compute });
        else computes.set(vk, { type: "raw", field });
      }
      resultMap.set(vk, []);
    });

    // Cumul journalier pour model 61 (DLI / Rayonnement)
    let cumSeries = null;
    if (needCum61) {
      const dayMap = {};
      devData.forEach((d) => {
        const tsn =
          d._ts ??
          (d.timestamp instanceof Date
            ? d.timestamp.getTime()
            : Date.parse(d.timestamp));
        const dateStr = new Date(tsn).toISOString().slice(0, 10);
        if (!dayMap[dateStr]) dayMap[dateStr] = [];
        dayMap[dateStr].push(d);
      });
      const dates = Object.keys(dayMap).sort();
      cumSeries = [];
      dates.forEach((dateStr) => {
        const daily = dayMap[dateStr]
          .slice()
          .sort(
            (a, b) =>
              (a._ts ?? Date.parse(a.timestamp)) -
              (b._ts ?? Date.parse(b.timestamp)),
          );
        const cumul = calculateCumulativeSeries(daily);
        cumul.forEach((p) => {
          cumSeries.push({ ts: p.ts, dli: p.dli, joules_cm2: p.joules_cm2 });
        });
      });
    }

    // Un passage: calcule toutes les variables non-cumulées
    for (const d of devData) {
      const ts =
        d._ts ??
        (d.timestamp instanceof Date
          ? d.timestamp.getTime()
          : Date.parse(d.timestamp));
      for (const vk of vars) {
        const spec = computes.get(vk);
        if (!spec || spec.type === "cum61") continue;
        let y;
        if (spec.type === "comp") y = spec.fn(d);
        else y = d[spec.field];
        if (y == null || Number.isNaN(y)) continue;
        const arr = resultMap.get(vk);
        arr.push({ x: ts, y: +y });
      }
    }

    // Injecter les séries cumulées si nécessaire
    if (needCum61 && cumSeries) {
      for (const vk of vars) {
        const { field } = parseVarKey(vk);
        if (field !== "DLI" && field !== "Rayonnement") continue;
        const arr = [];
        if (field === "DLI")
          cumSeries.forEach((p) => arr.push({ x: p.ts, y: p.dli }));
        else cumSeries.forEach((p) => arr.push({ x: p.ts, y: p.joules_cm2 }));
        resultMap.set(vk, arr);
      }
    }
  }

  // Construire les séries finales dans l'ordre demandé
  const series = [];
  keys.forEach((varKey, i) => {
    const { field } = parseVarKey(varKey);
    let data = resultMap.get(varKey) || [];
    if (data.length > MAX_OVERVIEW_POINTS)
      data = downsampleLTTB(data, MAX_OVERVIEW_POINTS);
    const isColumn = COLUMN_VARS.includes(field);
    series.push({
      name: labelMapLocal[varKey] || varKey,
      data:
        data.length > MAX_OVERVIEW_POINTS
          ? downsampleLTTB(data, MAX_OVERVIEW_POINTS)
          : data,
      yAxisIndex: i,
      ...(isColumn ? { type: "column" } : {}),
    });
  });

  // Multi Y axes if several curves
  const yaxis =
    series.length > 1
      ? series.map(() => ({
          show: true,
          opposite: false,
          title: { text: "" },
          labels: { show: false },
        }))
      : [{ show: true, title: { text: "" }, labels: { show: false } }];

  const strokeWidths = series.map((s) => (s.type === "column" ? 0 : 1));
  const fillOpacities = series.map((s) => (s.type === "column" ? 1 : 0));
  // Mise à jour légère: options minimales puis séries
  overviewChartInstance.value.updateOptions({
    legend: { show: false },
    fill: { type: "solid", opacity: fillOpacities },
    stroke: { width: strokeWidths },
    yaxis,
    plotOptions: { bar: { columnWidth: COLUMN_WIDTH } },
  });
  overviewChartInstance.value.updateSeries(series, false);
  let selStart;
  let selEnd;
  if (customDatesActive.value) {
    selEnd = endDateTime.value
      ? endDateTime.value.getTime()
      : new Date(endDate.value).getTime();
    selStart = startDateTime.value
      ? startDateTime.value.getTime()
      : new Date(startDate.value).getTime();
  } else {
    selEnd = end.getTime();
    selStart = end.getTime() - intervalDays.value * 864e5;
  }
  overviewChartInstance.value.updateOptions({
    chart: { selection: { xaxis: { min: selStart, max: selEnd } } },
  });
}

onMounted(async () => {
  ({ default: ApexCharts } = await import("apexcharts"));
  requestLocation();
  if (!startDateTime.value && !endDateTime.value) {
    endDateTime.value = new Date(fixedBaseTime.value);
    startDateTime.value = new Date(
      endDateTime.value.getTime() - intervalDays.value * 24 * 60 * 60 * 1000,
    );
    startDate.value = startDateTime.value.toISOString().slice(0, 10);
    endDate.value = endDateTime.value.toISOString().slice(0, 10);
  }
  // On ne configure que la structure du graphique, sans données initiales.
  // Em mobile: desativar TODAS as interações no gráfico (só botões)
  const isMobileNow = isMobile.value || isTouchDevice.value;
  
  const chartOptions = {
    chart: {
      id: mainId,
      type: "line",
      height: "100%",
      // Em mobile: desativar zoom completamente
      // Em desktop: zoom por seleção (arrastar), nunca por scroll
      zoom: { 
        enabled: !isMobileNow,
        type: 'x',
        autoScaleYaxis: true,
        allowMouseWheelZoom: false
      },
      selection: {
        enabled: false
      },
      // Toolbar: mostrar apenas em desktop
      toolbar: { 
        show: !isMobileNow,
        tools: {
          download: false,
          selection: false,
          zoom: true,
          zoomin: true,
          zoomout: true,
          pan: false,
          reset: true
        },
        autoSelected: 'zoom'
      },
      background: "transparent",
      timezone: zone.value,
      animations: { enabled: false, dynamicAnimation: { enabled: false } },
      redrawOnParentResize: false
    },
    plotOptions: { bar: { columnWidth: COLUMN_WIDTH } },
    series: [], // Important : commencer avec un tableau de séries vide
    dataLabels: { enabled: false },
    markers: { size: 0 },
    stroke: { width: 2 },
    xaxis: {
      type: "datetime",
      labels: {
        datetimeUTC: false,
        formatter: (val) => {
          const d = new Date(Number(val));
          if (isNaN(d)) return "";
          const span =
            (currentPeriodEnd.value || fixedBaseTime.value) -
            (currentPeriodStart.value || fixedBaseTime.value);
          return span <= 86400000
            ? `${pad(d.getHours())}:${pad(d.getMinutes())}`
            : `${pad(d.getDate())}/${pad(d.getMonth() + 1)} ${pad(d.getHours())}:${pad(d.getMinutes())}`;
        },
      },
    },
    yaxis: [
      {
        show: true,
        labels: {
          formatter: (val) =>
            val != null && !isNaN(val) ? Number(val).toFixed(1) : "",
        },
      },
    ],
    tooltip: {
      shared: true,
      intersect: false,
      custom: ({ seriesIndex, dataPointIndex, w }) => {
        const containerStyle = `
      background: rgba(255,255,255,0.4);
      border: 1px solid #777;
      border-radius: 5px;
      padding: 0;
    `;
        const valueStyleBase = `
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 4px 8px;
      font-family: Arial, sans-serif;
      font-weight: 600;
      text-shadow:
        -1px -1px 0 #FFF,
         1px -1px 0 #FFF,
        -1px  1px 0 #FFF,
         1px  1px 0 #FFF;
    `;

        const refX = w.globals.seriesX?.[seriesIndex]?.[dataPointIndex];
        if (refX == null) return "";

        const TOL_MS = 90 * 1000; // tolérance 90s

        const nearestIndex = (xs, target) => {
          if (!xs || xs.length === 0) return -1;
          let lo = 0,
            hi = xs.length - 1;
          while (lo <= hi) {
            const mid = (lo + hi) >> 1;
            if (xs[mid] === target) return mid;
            if (xs[mid] < target) lo = mid + 1;
            else hi = mid - 1;
          }
          const cand = [];
          if (hi >= 0) cand.push(hi);
          if (lo < xs.length) cand.push(lo);
          let best = -1,
            bestDiff = Infinity;
          for (const i of cand) {
            const diff = Math.abs(xs[i] - target);
            if (diff < bestDiff) {
              best = i;
              bestDiff = diff;
            }
          }
          return bestDiff <= TOL_MS ? best : -1;
        };

        let bodyHtml = "";

        for (let i = 0; i < w.globals.series.length; i++) {
          const xs = w.globals.seriesX?.[i];
          const ys = w.globals.series?.[i];
          if (!xs || !ys) continue;

          const idx = nearestIndex(xs, refX);
          if (idx < 0) continue;

          const value = ys[idx];
          if (value == null || Number.isNaN(value)) continue;

          const color = w.globals.colors?.[i] || "#333";
          const formattedValue = Number(value).toFixed(1); // Numero depois da virgula para os graficos

          bodyHtml += `<div style="${valueStyleBase} color:${color};">${formattedValue}</div>`;
        }

        if (!bodyHtml) return "";
        return `<div style="${containerStyle}">${bodyHtml}</div>`;
      },
    },

    noData: { text: "Chargement..." },
    locales: [{ name: "fr", options: {} }],
    defaultLocale: "fr",
  };

  const chart = new ApexCharts(chartRef.value, chartOptions);
  chart.render();
  // Não bloqueamos o wheel para permitir scroll da página sobre o gráfico
  // O zoom já está desativado nas opções do chart (zoom: { enabled: false })
  chartInstance.value = chart; // On stocke l'instance pour les mises à jour

  const overviewOptions = {
    chart: {
      id: overviewId,
      height: 80,
      type: "area",
      // Brush para controlar o gráfico principal
      brush: { 
        enabled: true, 
        target: mainId,
        autoScaleYaxis: false
      },
      selection: {
        enabled: true,
        type: 'x',
        xaxis: {
          min: fixedBaseTime.value.getTime() - intervalDays.value * 864e5,
          max: fixedBaseTime.value.getTime(),
        },
        fill: {
          color: '#82be20',
          opacity: 0.2
        },
        stroke: {
          width: 2,
          color: '#82be20',
          opacity: 0.8,
          dashArray: 0
        }
      },
      toolbar: { show: false },
      animations: { enabled: false, dynamicAnimation: { enabled: false } },
      redrawOnParentResize: false,
      zoom: { enabled: false, allowMouseWheelZoom: false },
    },
    dataLabels: { enabled: false },
    markers: { size: 0 },
    stroke: { width: 1 },
    fill: { opacity: 0.3 },
    xaxis: { type: "datetime", labels: { show: false } },
    yaxis: { labels: { show: false } },
    grid: { show: false },
    series: [],
  };

  const ov = new ApexCharts(overviewChartRef.value, overviewOptions);
  ov.render();
  overviewChartInstance.value = ov;

  // Quando o utilizador seleciona uma área no overview, atualizar datas
  ov.addEventListener("selection", (_, { xaxis }) => {
    if (!xaxis) return;
    startDateTime.value = new Date(xaxis.min);
    endDateTime.value = new Date(xaxis.max);
    startDate.value = startDateTime.value.toISOString().slice(0, 10);
    endDate.value = endDateTime.value.toISOString().slice(0, 10);
    customDatesActive.value = true;
    updateChartDebounced();
  });

  // Si c'est un graphique personnalisé, on utilise sa propre logique
  if (useCustomSeries.value) {
    updateCustomSeriesChart(props.series);
  } else {
    if (selectedVariables.value.length) {
      await updateChart();
    } else {
      chartInstance.value.updateOptions({
        series: [],
        noData: { text: "Sélectionnez des variables" },
      });
      overviewChartInstance.value.updateOptions({ series: [] });
    }
  }
});

onUnmounted(() => {
  if (overviewChartInstance.value) {
    overviewChartInstance.value.destroy();
    overviewChartInstance.value = null;
  }
  if (chartInstance.value) {
    chartInstance.value.destroy();
    chartInstance.value = null;
  }
});
</script>

<style scoped>
/* ============================================
   Base Chart Card Styles
   ============================================ */
.chart-card {
  display: flex;
  flex-direction: column;
  height: 100%;
  min-width: 0;
  overflow: visible;
}

/* ============================================
   Header Styles
   ============================================ */
.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.5rem 0.75rem;
  gap: 0.5rem;
  flex-wrap: nowrap;
  min-height: 44px;
}

.chart-title {
  flex: 1 1 auto;
  min-width: 0;
  font-size: 0.95rem;
  font-weight: 600;
  line-height: 1.3;
  margin: 0;
  /* Multi-line truncation */
  display: -webkit-box;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.header-actions {
  display: flex;
  gap: 0.25rem;
  flex-shrink: 0;
}

.header-actions .btn {
  padding: 0.25rem 0.4rem;
  min-width: 32px;
  min-height: 32px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.btn-icon {
  font-size: 0.85rem;
  line-height: 1;
}

/* ============================================
   Card Body & Controls Layout
   ============================================ */
.card-body.d-flex {
  flex: 1 1 auto;
  min-width: 0;
  min-height: 0;
  overflow: visible;
  flex-direction: row;
  gap: 0;
}

.chart-controls-column {
  width: 52px;
  min-width: 48px;
  background: #f8f9fa;
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 0.5rem 0.25rem;
  gap: 0.5rem;
}

/* Tool buttons container */
.tool-buttons {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  align-items: center;
}

.tool-btn {
  width: 36px;
  height: 36px;
  padding: 0;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 1rem;
  border-radius: 8px;
  -webkit-tap-highlight-color: transparent;
}

/* ============================================
   Interval Pills - Compact Design
   ============================================ */
.interval-pills {
  display: flex;
  flex-direction: column;
  gap: 2px;
  width: 100%;
  margin: 0.25rem 0;
}

.interval-pill {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0.2rem 0.4rem;
  font-size: 0.7rem;
  font-weight: 600;
  color: #555;
  background: #fff;
  border: 1px solid #ddd;
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.15s ease;
  min-height: 24px;
  -webkit-tap-highlight-color: transparent;
}

.interval-pill:hover {
  background: #e8f5e9;
  border-color: #82be20;
}

.interval-pill.active {
  background: linear-gradient(135deg, #82be20 0%, #6aa818 100%);
  color: #fff;
  border-color: #6aa818;
  box-shadow: 0 2px 4px rgba(130, 190, 32, 0.3);
}

.interval-pill:active {
  transform: scale(0.95);
}

/* Refresh button */
.refresh-btn {
  width: 36px;
  height: 36px;
  padding: 0;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 1.1rem;
  border-radius: 8px;
  margin-top: auto;
  background: transparent;
  border: 1px solid #0d6efd;
  color: #0d6efd;
  cursor: pointer;
  transition: all 0.15s ease;
}

.refresh-btn:hover {
  background: #0d6efd;
  color: #fff;
}

.refresh-btn:active {
  transform: scale(0.92);
}

/* ============================================
   Panels Area
   ============================================ */
.chart-panels-area {
  width: 220px;
  min-width: 140px;
  background: #f8f9fa;
  -webkit-overflow-scrolling: touch;
  overscroll-behavior: contain;
}

.chart-panels-area h6 {
  font-size: 0.9rem;
  margin-bottom: 0.5rem;
}

/* ============================================
   Chart Area
   ============================================ */
.chart-container {
  position: relative;
  display: flex;
  flex-direction: column;
  flex: 1 1 auto;
  min-height: 0;
  overflow: visible;
}

.chart {
  width: 100%;
  flex: 1 1 auto;
  min-width: 0;
  min-height: 150px;
  margin: 0 auto;
}

/* Esconder o rect de seleção do brush no gráfico principal */
.chart :deep(.apexcharts-selection-rect) {
  display: none !important;
}

/* Overview wrapper - fora do card-body, abaixo das legendas */
.overview-chart-wrapper {
  position: relative;
  width: 100%;
  padding: 8px 12px;
  background: #f8f9fa;
  border-top: 1px solid #e9ecef;
  flex-shrink: 0;
}

.overview-chart {
  width: 100%;
  height: 80px;
}

/* Overview toggle button */
.overview-toggle {
  position: absolute;
  bottom: 4px;
  right: 8px;
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 4px 8px;
  font-size: 0.7rem;
  background: rgba(255, 255, 255, 0.9);
  border: 1px solid #ddd;
  border-radius: 12px;
  color: #666;
  cursor: pointer;
  z-index: 5;
  transition: all 0.15s ease;
}

.overview-toggle:hover {
  background: #fff;
  border-color: #82be20;
  color: #82be20;
}

.overview-toggle i {
  font-size: 0.65rem;
}

.overview-toggle-text {
  font-weight: 500;
}

/* ============================================
   Form Controls
   ============================================ */
.form-control,
.form-check-input {
  font-size: 0.9rem;
  padding: 0.25rem 0.5rem;
}

.form-check-label {
  font-size: 0.9rem;
}

/* ============================================
   Tables
   ============================================ */
.card .table {
  margin-bottom: 0;
  background: #fff;
}

/* ============================================
   ApexCharts Tooltip Styles
   ============================================ */
:deep(.apexcharts-tooltip) {
  background: transparent !important;
  border: transparent !important;
  border-radius: 5px !important;
  box-shadow: none !important;
  padding: 0 !important;
  overflow: hidden;
}

:deep(.apexcharts-tooltip .apexcharts-tooltip-title),
:deep(.apexcharts-tooltip .apexcharts-tooltip-series-group) {
  background: transparent !important;
  color: transparent !important;
  text-shadow: -1px -1px 0 #fff, 1px -1px 0 #fff, -1px 1px 0 #fff, 1px 1px 0 #fff;
}

:deep(.apexcharts-tooltip .apexcharts-tooltip-title) {
  padding: 5px 8px !important;
  border-bottom: 1px solid #ddd !important;
  text-align: center !important;
}

:deep(.apexcharts-tooltip .apexcharts-tooltip-series-group) {
  padding: 3px 8px !important;
  text-align: center !important;
}

/* ============================================
   MOBILE RESPONSIVE - Portrait Mode
   ============================================ */
@media (max-width: 800px) {
  /* Remover card styling em mobile - gráfico ocupa todo o espaço */
  .chart-card {
    border: none;
    border-radius: 0;
    box-shadow: none;
    background: #fff;
  }
  
  .card-body.d-flex {
    flex-direction: column;
  }
  
  .chart-controls-column {
    flex-direction: row !important;
    width: 100%;
    min-width: 0;
    padding: 0.3rem 0.5rem;
    gap: 0.3rem;
    border-right: none;
    border-bottom: 1px solid #e9ecef;
    overflow-x: auto;
    justify-content: flex-start;
    align-items: center;
    background: #fafafa;
  }
  
  /* Em mobile: interval pills PRIMEIRO (order: 1), tool buttons DEPOIS (order: 2) */
  .interval-pills {
    order: 1;
    flex-direction: row;
    flex-wrap: nowrap;
    gap: 3px;
    margin: 0;
    padding: 0;
  }
  
  .tool-buttons {
    order: 3;
    flex-direction: row;
    gap: 0.2rem;
  }
  
  .refresh-btn {
    order: 2;
    margin-top: 0;
    margin-left: 0.35rem;
  }
  
  .interval-pill {
    padding: 0.3rem 0.5rem;
    font-size: 0.7rem;
    min-height: 26px;
    white-space: nowrap;
    border-radius: 13px;
  }
  
  .chart-panels-area {
    flex-direction: row !important;
    width: 100%;
    border-left: none;
    border-bottom: 1px solid #dee2e6;
    padding: 0.5rem;
    overflow-x: auto;
  }
  
  .chart {
    min-height: 280px;
  }
}

/* ============================================
   MOBILE RESPONSIVE - Small Screens
   ============================================ */
@media (max-width: 576px) {
  .card-header {
    padding: 0.4rem 0.5rem;
    min-height: 40px;
  }
  
  .chart-title {
    font-size: 0.85rem;
    -webkit-line-clamp: 2;
    line-clamp: 2;
  }
  
  .header-actions .btn {
    min-width: 28px;
    min-height: 28px;
    padding: 0.2rem;
  }
  
  .btn-icon {
    font-size: 0.8rem;
  }
  
  .tool-btn {
    width: 32px;
    height: 32px;
    font-size: 0.9rem;
  }
  
  .interval-pill {
    padding: 0.3rem 0.5rem;
    font-size: 0.7rem;
    min-height: 26px;
  }
  
  .refresh-btn {
    width: 32px;
    height: 32px;
    font-size: 0.9rem;
  }
  
  .chart {
    min-height: 220px;
  }
  
  .overview-chart {
    height: 60px;
  }
}

/* ============================================
   MOBILE RESPONSIVE - Extra Small
   ============================================ */
@media (max-width: 400px) {
  .chart-title {
    font-size: 0.8rem;
  }
  
  .interval-pill {
    padding: 0.25rem 0.4rem;
    font-size: 0.65rem;
  }
  
  .chart-panels-area {
    max-height: 180px;
    overflow-y: auto;
  }
}

/* ============================================
   LANDSCAPE MOBILE - Small Height
   Barra de controlos vertical na esquerda
   ============================================ */
@media (max-height: 500px) and (orientation: landscape) {
  .chart-card {
    --overview-height: 20px;
  }
  
  .card-body.d-flex {
    flex-direction: row;
  }
  
  .chart-controls-column {
    flex-direction: column !important;
    width: auto;
    min-width: 48px;
    max-width: 54px;
    padding: 0.25rem;
    gap: 0.25rem;
    border-right: 1px solid #dee2e6;
    border-bottom: none;
    overflow-y: auto;
    overflow-x: hidden;
    justify-content: flex-start;
    align-items: center;
  }
  
  .interval-pills {
    order: unset;
    flex-direction: column;
    gap: 0.3rem;
    margin: 0;
    padding: 0;
    width: 100%;
  }
  
  .interval-pill {
    width: 100%;
    padding: 0.25rem 0.3rem;
    font-size: 0.65rem;
    border-radius: 10px;
  }
  
  .tool-buttons {
    order: unset;
    flex-direction: column;
    gap: 0.25rem;
    width: 100%;
  }
  
  .tool-btn {
    width: 100%;
    min-height: 28px;
    font-size: 0.85rem;
  }
  
  .refresh-btn {
    order: unset;
    margin-top: auto;
    margin-left: 0;
    width: 100%;
  }
  
  .chart {
    flex: 1 1 auto;
    min-height: 200px !important;
  }
  
  .overview-chart {
    height: 20px;
  }
  
  .overview-toggle {
    bottom: 4px;
    right: 8px;
    padding: 2px 6px;
    font-size: 0.65rem;
  }
}

/* ============================================
   Touch Feedback
   ============================================ */
.tool-btn:active,
.interval-pill:active,
.refresh-btn:active,
.header-actions .btn:active {
  transform: scale(0.92);
  opacity: 0.85;
}

/* Scrollbar styling */
.chart-controls-column::-webkit-scrollbar,
.chart-panels-area::-webkit-scrollbar {
  height: 4px;
  background: #eafaf3;
}

.chart-controls-column::-webkit-scrollbar-thumb,
.chart-panels-area::-webkit-scrollbar-thumb {
  background: #82be20;
  border-radius: 2px;
}

.chart-controls-column,
.chart-panels-area {
  scrollbar-width: thin;
  scrollbar-color: #82be20 #eafaf3;
}
</style>
