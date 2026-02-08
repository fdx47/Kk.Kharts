<template>
  <DefaultLayout
    :hideSidebar="true"
    showBack
    @back="goLanding"
    @logout="logout"
  >
    <template #title> Export CSV </template>

    <div class="container-fluid py-3 export-page">
      <div class="row justify-content-center">
        <div class="col-12 col-lg-10 col-xl-8">
          <div class="card shadow-sm">
            <div class="card-body">
              <h5 class="card-title mb-3">Paramètres d'export</h5>
              <form @submit.prevent="handleExport">
                <div class="mb-3">
                  <label class="form-label">Kapteurs</label>
                  <select
                    v-model="selectedDeviceIds"
                    class="form-select"
                    multiple
                    size="6"
                  >
                    <optgroup
                      v-for="group in deviceGroups"
                      :key="group.label"
                      :label="group.label"
                    >
                      <option
                        v-for="device in group.options"
                        :key="device.devEui"
                        :value="device.devEui"
                      >
                        {{ device.label }}
                      </option>
                    </optgroup>
                  </select>
                  <div class="form-text">
                    Maintenez Ctrl (Windows) ou Cmd (Mac) pour sélectionner plusieurs Kapteurs.
                  </div>
                </div>

                <div class="mb-3" v-if="variableGroups.length">
                  <label class="form-label d-block">Variables</label>
                  <div
                    v-for="group in variableGroups"
                    :key="group.devEui || group.deviceLabel"
                    class="mb-3"
                  >
                    <div class="fw-semibold small text-uppercase text-muted mb-2">
                      {{ group.deviceLabel }}
                    </div>
                    <div class="row g-2">
                      <div
                        v-for="option in group.options"
                        :key="option.key"
                        class="col-12 col-sm-6"
                      >
                        <div class="form-check">
                          <input
                            class="form-check-input"
                            type="checkbox"
                            :id="`var-${option.key}`"
                            :value="option.key"
                            v-model="selectedVariables"
                          />
                          <label class="form-check-label" :for="`var-${option.key}`">
                            {{ option.label }}
                          </label>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>

                <div v-else class="alert alert-info small">
                  Sélectionnez au moins un Kapteur pour afficher ses variables disponibles.
                </div>

                <div class="row g-3">
                  <div class="col-md-6">
                    <label class="form-label">Début</label>
                    <input
                      class="form-control"
                      type="date"
                      v-model="startDate"
                    />
                  </div>
                  <div class="col-md-6">
                    <label class="form-label">Fin</label>
                    <input
                      class="form-control"
                      type="date"
                      v-model="endDate"
                    />
                  </div>
                </div>
                <div class="form-text mt-1">
                  Les heures sont fixées à 00:00. Sélectionnez le jour suivant pour couvrir une journée complète.
                </div>

                <div class="row g-3 mt-0">
                  <div class="col-md-6">
                    <label class="form-label">Pas de temps</label>
                    <select class="form-select" v-model.number="stepMinutes">
                      <option v-for="opt in stepOptions" :key="opt.value" :value="opt.value">
                        {{ opt.label }}
                      </option>
                    </select>
                    <div class="form-text">
                      Filtre les mesures trop rapprochées pour alléger le fichier exporté.
                    </div>
                  </div>
                </div>

                <div v-if="errorMessage" class="alert alert-danger mt-3 mb-0">
                  {{ errorMessage }}
                </div>

                <div class="d-flex justify-content-end gap-2 mt-4">
                  <button
                    type="button"
                    class="btn btn-outline-secondary"
                    @click="resetForm"
                    :disabled="isLoading"
                  >
                    Réinitialiser
                  </button>
                  <button
                    type="submit"
                    class="btn btn-success"
                    :disabled="!canExport || isLoading"
                  >
                    <span
                      v-if="isLoading"
                      class="spinner-border spinner-border-sm me-2"
                      role="status"
                      aria-hidden="true"
                    ></span>
                    Exporter en CSV
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  </DefaultLayout>
</template>

<script setup>
import { ref, computed, inject, onMounted, watch } from "vue";
import DefaultLayout from "@/views/layout/DefaultLayout.vue";
import { useAuth } from "@/composables/useAuth.js";
import { useNavigation } from "@/composables/useNavigation.js";
import { useDevices } from "@/composables/useDevices.js";
import useDeviceData from "@/composables/useDeviceData.js";
import { useComputedVars } from "@/composables/useComputedVars.js";
import { getLabelMap } from "@/services/apiService.js";
import {
  getGroupLabelMap,
  GROUP_VAR_SEPARATOR,
} from "@/utils/getGroupLabelMap.js";
import { parseVarKey } from "@/composables/useChartCalculations.js";
import { exportMeasurementsCsv } from "@/utils/csvExport.js";

const { logout } = useAuth();
const { goLanding } = useNavigation();
const { devices: fetchedDevices, loadDevices } = useDevices();
const injectedVirtual = inject("virtualDevices", { virtualDevices: ref([]) });
const virtualDevices = injectedVirtual?.virtualDevices ?? ref([]);
const { fetchDeviceData } = useDeviceData();

const selectedDeviceIds = ref([]);
const selectedVariables = ref([]);
const errorMessage = ref("");
const isLoading = ref(false);

const stepOptions = [
  { value: 0, label: "Ne pas filtrer" },
  { value: 5, label: "5 minutes" },
  { value: 15, label: "15 minutes" },
  { value: 30, label: "30 minutes" },
  { value: 60, label: "1 heure" },
  { value: 180, label: "3 heures" },
  { value: 1440, label: "1 jour" },
];
const stepMinutes = ref(15);

function toDateInputValue(date) {
  const pad = (n) => String(n).padStart(2, "0");
  const year = date.getFullYear();
  const month = pad(date.getMonth() + 1);
  const day = pad(date.getDate());
  return `${year}-${month}-${day}`;
}

function defaultEndDate() {
  const now = new Date();
  return new Date(now.getFullYear(), now.getMonth(), now.getDate());
}

function defaultStartDate() {
  const end = defaultEndDate();
  return new Date(end.getTime() - 24 * 60 * 60 * 1000);
}

const endDate = ref(toDateInputValue(defaultEndDate()));
const startDate = ref(toDateInputValue(defaultStartDate()));

const allDevices = computed(() => [
  ...(fetchedDevices.value ?? []),
  ...(virtualDevices.value ?? []),
]);

function deviceDisplayLabel(device) {
  return (
    device?.description ||
    device?.deviceName ||
    device?.name ||
    device?.devEui ||
    "Kapteur"
  );
}

const MODEL_LABELS = {
  2: "EM300-DI",
  7: "EM300-TH",
  47: "UC502 Wet150",
  61: "UC502 Modbus",
  62: "UC502 multi-sondes",
};

function deviceTypeLabel(device) {
  if (device?.isVirtual) return "Groupes virtuels";
  const model = Number(device?.model ?? device?.deviceId);
  if (Number.isFinite(model) && MODEL_LABELS[model]) {
    return MODEL_LABELS[model];
  }
  if (typeof device?.deviceName === "string" && device.deviceName.trim()) {
    return device.deviceName.trim();
  }
  return "Autres kapteurs";
}

const deviceGroups = computed(() => {
  const groups = new Map();
  allDevices.value.forEach((device) => {
    const typeLabel = deviceTypeLabel(device);
    if (!groups.has(typeLabel)) groups.set(typeLabel, []);
    groups.get(typeLabel).push({
      devEui: device.devEui,
      label: deviceDisplayLabel(device),
    });
  });
  return Array.from(groups.entries())
    .map(([label, options]) => ({
      label,
      options: options.sort((a, b) =>
        a.label.localeCompare(b.label, "fr", { sensitivity: "base" }),
      ),
    }))
    .sort((a, b) => a.label.localeCompare(b.label, "fr", { sensitivity: "base" }));
});

const exportContext = computed(() => {
  const labelMap = {};
  const computedVarDefs = {};
  const physicalDevices = fetchedDevices.value ?? [];

  selectedDeviceIds.value.forEach((devEui) => {
    const device = allDevices.value.find((d) => d.devEui === devEui);
    if (!device) return;

    if (device.isVirtual && device.group) {
      const baseMap = getGroupLabelMap(device.group, physicalDevices) || {};
      Object.entries(baseMap).forEach(([key, label]) => {
        labelMap[key] = label;
      });
      (device.group.devEuis || []).forEach((realDevEui) => {
        let model = device.group.deviceModels?.[realDevEui];
        if (model == null) {
          const match = physicalDevices.find((d) => d.devEui === realDevEui);
          model = match?.model;
        }
        model = Number(model);
        if (!model) return;
        const defs = useComputedVars(model) || {};
        Object.entries(defs).forEach(([key, def]) => {
          const composedKey = `${key}${GROUP_VAR_SEPARATOR}${realDevEui}`;
          if (!labelMap[composedKey]) {
    labelMap[composedKey] = `${deviceDisplayLabel(device)} – ${def.label}`;
          }
          computedVarDefs[composedKey] = def;
        });
      });
    } else {
      const baseMap = getLabelMap(device) || {};
      const deviceLabel = deviceDisplayLabel(device);
      Object.entries(baseMap).forEach(([key, label]) => {
        const composedKey = `${key}${GROUP_VAR_SEPARATOR}${device.devEui}`;
  labelMap[composedKey] = `${deviceLabel} – ${label}`;
      });
      const defs = useComputedVars(Number(device.model)) || {};
      Object.entries(defs).forEach(([key, def]) => {
        const composedKey = `${key}${GROUP_VAR_SEPARATOR}${device.devEui}`;
        if (!labelMap[composedKey]) {
      labelMap[composedKey] = `${deviceLabel} – ${def.label}`;
        }
        computedVarDefs[composedKey] = def;
      });
    }
  });

  return { labelMap, computedVarDefs };
});

const variableGroups = computed(() => {
  const groups = new Map();
  const mapEntries = Object.entries(exportContext.value.labelMap);
  if (!mapEntries.length) return [];

  mapEntries.forEach(([key, label]) => {
    const { devEui } = parseVarKey(key);
    const bucket = devEui || "global";
    if (!groups.has(bucket)) {
      const device = devEui
        ? allDevices.value.find((d) => d.devEui === devEui)
        : null;
      groups.set(bucket, {
        devEui,
        deviceLabel: device ? deviceDisplayLabel(device) : "Variables",
        options: [],
      });
    }
    const separator = [" \u2013 ", " - "].find((sep) => label.includes(sep));
    const optionLabel = separator
      ? label.split(separator).slice(-1)[0].trim()
      : label;
    groups.get(bucket).options.push({ key, label: optionLabel });
  });

  return Array.from(groups.values())
    .map((group) => ({
      ...group,
      options: group.options.sort((a, b) =>
        a.label.localeCompare(b.label, "fr", { sensitivity: "base" }),
      ),
    }))
    .sort((a, b) =>
      (a.deviceLabel || "").localeCompare(b.deviceLabel || "", "fr", {
        sensitivity: "base",
      }),
    );
});

const canExport = computed(
  () =>
    selectedDeviceIds.value.length > 0 &&
    selectedVariables.value.length > 0 &&
    !!startDate.value &&
    !!endDate.value,
);

watch(
  exportContext,
  (ctx) => {
    selectedVariables.value = selectedVariables.value.filter((key) =>
      Object.prototype.hasOwnProperty.call(ctx.labelMap, key),
    );
  },
  { deep: true },
);

function parseLocalDate(value, endOfDay = false) {
  if (!value) return null;
  const [year, month, day] = value.split("-");
  const y = Number(year);
  const m = Number(month);
  const d = Number(day);
  if (
    Number.isNaN(y) ||
    Number.isNaN(m) ||
    Number.isNaN(d) ||
    month.length !== 2 ||
    day.length !== 2
  ) {
    return null;
  }
  const date = new Date(y, m - 1, d, 0, 0, 0, 0);
  if (endOfDay) {
    date.setDate(date.getDate() + 1);
  }
  return date;
}

function slugify(value) {
  return String(value || "")
    .normalize("NFD")
    .replace(/\p{Diacritic}/gu, "")
    .replace(/[^a-z0-9]+/gi, "-")
    .replace(/^-+|-+$/g, "")
    .toLowerCase();
}

function buildFileName(start, end) {
  const formatTag = (date) => {
    const pad = (n) => String(n).padStart(2, "0");
    return (
      `${date.getFullYear()}${pad(date.getMonth() + 1)}${pad(date.getDate())}` +
      `_${pad(date.getHours())}${pad(date.getMinutes())}`
    );
  };
  const startTag = formatTag(start);
  const endTag = formatTag(end);
  let devicePart = "multi";
  if (selectedDeviceIds.value.length === 1) {
    const device = allDevices.value.find(
      (d) => d.devEui === selectedDeviceIds.value[0],
    );
    devicePart = slugify(deviceDisplayLabel(device));
  }
  return `export_${devicePart}_${startTag}_${endTag}.csv`;
}

async function handleExport() {
  if (!canExport.value || isLoading.value) return;
  const start = parseLocalDate(startDate.value);
  const end = parseLocalDate(endDate.value, true);
  if (!start || !end) {
    errorMessage.value = "Veuillez renseigner une période valide.";
    return;
  }
  if (start >= end) {
    errorMessage.value = "La date de début doit précéder la date de fin.";
    return;
  }

  errorMessage.value = "";
  isLoading.value = true;

  try {
    await loadDevices();
    const startIso = start.toISOString();
    const endIso = end.toISOString();
    const rows = [];

    for (const devEui of selectedDeviceIds.value) {
      const device = allDevices.value.find((d) => d.devEui === devEui);
      if (!device) continue;
      const model = device.model ?? device.deviceId;
      const data = await fetchDeviceData(devEui, model, startIso, endIso);
      data.forEach((entry) => {
        const tsRaw =
          entry.timestamp ?? entry.Timestamp ?? entry.time ?? entry.date ?? null;
        if (!tsRaw) return;
        const ts = new Date(tsRaw);
        if (Number.isNaN(ts.getTime())) return;
        const normalized = { ...entry };
        if (!normalized.devEui) {
          normalized.devEui = entry.devEui || devEui;
        }
        normalized.timestamp = ts.toISOString();
        rows.push(normalized);
      });
    }

    if (!rows.length) {
      errorMessage.value = "Aucune donnée disponible pour cette sélection.";
      return;
    }

    rows.sort(
      (a, b) => new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime(),
    );

    exportMeasurementsCsv({
      rows,
      stepMinutes: stepMinutes.value,
      variableKeys: selectedVariables.value,
      labelMap: exportContext.value.labelMap,
      computedVarDefs: exportContext.value.computedVarDefs,
      parseVarKey,
      fileName: buildFileName(start, end),
    });
  } catch (err) {
    console.error("CSV export failed", err);
    errorMessage.value = "Une erreur est survenue pendant l'export.";
  } finally {
    isLoading.value = false;
  }
}

function resetForm() {
  selectedDeviceIds.value = [];
  selectedVariables.value = [];
  startDate.value = toDateInputValue(defaultStartDate());
  endDate.value = toDateInputValue(defaultEndDate());
  stepMinutes.value = 15;
  errorMessage.value = "";
}

onMounted(async () => {
  try {
    await loadDevices();
  } catch (err) {
    console.error("Failed to load devices", err);
  }
});
</script>

<style scoped>
.export-page {
  background: #f7f9fb;
  min-height: calc(100vh - 140px);
}
.export-page .card {
  border-radius: 16px;
  border: none;
}
.export-page .form-label {
  font-weight: 600;
}
.export-page .form-text {
  font-size: 0.85rem;
}
.export-page .form-check-label {
  font-size: 0.95rem;
}
</style>
