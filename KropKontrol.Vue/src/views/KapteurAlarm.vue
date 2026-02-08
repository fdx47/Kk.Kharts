<template>
  <DefaultLayout
    :devices="devices"
    :selectedDevice="selectedDevice"
    @select-device="selectDevice"
    showBack
    @back="goLanding"
    @logout="logout"
  >
    <template #title>Configuration des Kapteurs</template>

    <div class="container py-4" v-if="selectedDevice">
      <div
        v-if="deviceConfigError"
        class="alert alert-warning d-flex align-items-start gap-2"
        role="alert"
        style="max-width: 540px"
      >
        <i class="bi bi-exclamation-triangle-fill"></i>
        <span>{{ deviceConfigError }}</span>
      </div>
      <div class="card mb-4" v-if="deviceInfo">
        <div class="card-body">
          <h5 class="card-title mb-3">Konfiguration actuelle du Kapteur</h5>
          <p class="mb-1">{{ deviceInfo.description }}</p>
          <p class="mb-3">{{ deviceInfo.installationLocation }}</p>
          <div v-if="Object.keys(currentThresholds).length">
            <h6 class="mb-2">Seuils actuels</h6>
            <ul class="list-unstyled">
              <li v-for="(thr, key) in currentThresholds" :key="key">
                {{ variableMap[key] || key }} : {{ thr.low ?? "-" }} /
                {{ thr.high ?? "-" }} /
                {{ thr.hysteresis ?? 0 }}
              </li>
            </ul>
          </div>
        </div>
      </div>
      <div class="mb-4" v-if="deviceConfig">
        <h5 class="mb-2">Configuration du Kapteur</h5>
        <div class="mb-2" style="max-width: 400px">
          <label class="form-label">Name</label>
          <input
            v-model="deviceConfig.name"
            type="text"
            class="form-control"
            readonly
          />
        </div>
        <div class="mb-2" style="max-width: 400px">
          <label class="form-label">Description</label>
          <input
            v-model="deviceConfig.description"
            type="text"
            class="form-control"
          />
        </div>
        <div class="form-check form-switch mb-2">
          <input
            class="form-check-input"
            type="checkbox"
            id="active-switch"
            v-model="deviceConfig.activeInKropKontrol"
          />
          <label class="form-check-label" for="active-switch"
            >Sonde active</label
          >
        </div>
        <div class="mb-3" style="max-width: 400px">
          <label class="form-label">Localisation du Kapteur</label>
          <input
            v-model="deviceConfig.installationLocation"
            type="text"
            class="form-control"
          />
        </div>
        <button class="btn btn-primary" @click="saveDeviceConfig">
          Enregistrer
        </button>
      </div>

      <div class="mb-4" v-if="Object.keys(variableMap).length">
        <h5 class="mb-2">Définir des seuils d'alertes</h5>
        <div class="row g-2 align-items-end" style="max-width: 500px">
          <div class="col-md-5">
            <label class="form-label">Variable à surveiller</label>
            <select v-model="selectedVariable" class="form-select">
              <option disabled value="">Sélectionner…</option>
              <option v-for="(lbl, key) in variableMap" :key="key" :value="key">
                {{ lbl }}
              </option>
            </select>
          </div>
          <div class="col-md-2">
            <label class="form-label">Seuil bas</label>
            <input
              v-model.number="lowValue"
              type="number"
              class="form-control"
            />
          </div>
          <div class="col-md-2">
            <label class="form-label">Seuil haut</label>
            <input
              v-model.number="highValue"
              type="number"
              class="form-control"
            />
          </div>
          <div class="col-md-2">
            <label class="form-label">
              Hystérésis
              <i
                class="bi bi-question-circle ms-1"
                title="Seuil haut : la valeur de la variable doit repasser en dessous de seuil haut − l'hystérésis pour lever l’alerte.
Seuil bas : la variable doit dépasser  le seuil bas + l'hystérésis pour lever l’alerte."
              ></i>
            </label>
            <input
              v-model.number="hysteresisValue"
              type="number"
              class="form-control"
            />
          </div>
          <div class="col-md-1 d-grid">
            <button
              class="btn btn-success"
              :disabled="!selectedVariable"
              @click="saveThreshold"
            >
              Valider les paramètres de seuils
            </button>
          </div>
        </div>
      </div>

      <div class="mt-4" v-if="Object.keys(currentThresholds).length">
        <h5 class="mb-3">Seuils définis</h5>
        <ul class="list-group" style="max-width: 500px">
          <li
            v-for="(thr, key) in currentThresholds"
            :key="key"
            class="list-group-item d-flex justify-content-between align-items-center"
          >
            <span
              >{{ variableMap[key] || key }} : {{ thr.low ?? "-" }} /
              {{ thr.high ?? "-" }} (Hyst.: {{ thr.hysteresis ?? "-" }})</span
            >
            <button
              class="btn btn-sm btn-outline-danger"
              @click="deleteThreshold(key)"
            >
              Supprimer
            </button>
          </li>
        </ul>
      </div>
    </div>

    <div v-else class="text-center text-muted py-5">
      Sélectionnez un kapteur dans la liste.
    </div>
  </DefaultLayout>
</template>

<script setup>
import { ref, computed, onMounted } from "vue";
import DefaultLayout from "@/views/layout/DefaultLayout.vue";
import { useAuth } from "@/composables/useAuth.js";
import { useNavigation } from "@/composables/useNavigation.js";
import {
  getLabelMap,
  getDeviceConfig,
  updateDeviceConfig,
  postThresholdsAlarms,
  getThresholdsAlarms,
} from "@/services/apiService.js";
import { useDevices } from "@/composables/useDevices.js";
import useLocalStorage from "@/composables/useLocalStorage.js";
import { LS_DEVICE_THRESHOLDS } from "@/config/storageKeys.js";

const { logout } = useAuth();
const { goLanding } = useNavigation();

const { devices: fetchedDevices, loadDevices } = useDevices();
const thresholdsStore = useLocalStorage(LS_DEVICE_THRESHOLDS, {});
const deviceConfig = ref(null);
const deviceInfo = ref(null);

onMounted(async () => {
  if (!Array.isArray(fetchedDevices.value) || fetchedDevices.value.length === 0) {
    await loadDevices().catch(() => []);
  }
});

const devices = computed(() => fetchedDevices.value);

const selectedDevice = ref(null);
const selectedVariable = ref("");
const lowValue = ref(null);
const highValue = ref(null);
const hysteresisValue = ref(null);
const deviceConfigError = ref("");

function normaliseString(value) {
  return typeof value === "string" ? value.trim() : "";
}

function buildDeviceConfigFromSource(source = {}, fallbackSource = {}) {
  const fallbackDevEui =
    normaliseString(source.devEui) ||
    normaliseString(fallbackSource.devEui) ||
    "Kapteur";
  const name =
    normaliseString(source.name) ||
    normaliseString(source.deviceName) ||
    normaliseString(fallbackSource.name) ||
    normaliseString(fallbackSource.deviceName) ||
    fallbackDevEui;
  const description =
    normaliseString(source.description) ||
    normaliseString(fallbackSource.description) ||
    normaliseString(source.deviceName) ||
    normaliseString(fallbackSource.deviceName) ||
    `Kapteur ${fallbackDevEui}`;
  const installationLocation =
    normaliseString(source.installationLocation) ||
    normaliseString(source.location) ||
    normaliseString(fallbackSource.installationLocation) ||
    normaliseString(fallbackSource.location) ||
    normaliseString(fallbackSource.companyName) ||
    `Site ${fallbackDevEui}`;

  return {
    name,
    description,
    activeInKropKontrol:
      typeof source.activeInKropKontrol === "boolean"
        ? source.activeInKropKontrol
        : Boolean(fallbackSource.activeInKropKontrol),
    installationLocation,
  };
}

function sanitiseDeviceConfigInput(input = {}, fallbackSource = {}) {
  const merged = {
    ...fallbackSource,
    ...input,
    activeInKropKontrol:
      typeof input.activeInKropKontrol === "boolean"
        ? input.activeInKropKontrol
        : fallbackSource.activeInKropKontrol,
  };
  return buildDeviceConfigFromSource(merged, fallbackSource);
}

async function selectDevice(dev) {
  selectedDevice.value = dev;
  selectedVariable.value = "";
  lowValue.value = null;
  highValue.value = null;
  hysteresisValue.value = null;
  deviceInfo.value = dev ? { ...dev } : null;
  deviceConfig.value = buildDeviceConfigFromSource(dev);
  deviceConfigError.value = "";
  await loadDeviceConfig(dev.devEui);
  await loadDeviceThresholds(dev.devEui);
}

async function loadDeviceConfig(devEui) {
  try {
    deviceConfigError.value = "";
    const info = await getDeviceConfig(devEui);
    deviceInfo.value = info;
    deviceConfig.value = buildDeviceConfigFromSource(
      info,
      selectedDevice.value || info,
    );
  } catch (e) {
    console.error(e);
    deviceInfo.value = selectedDevice.value
      ? { ...selectedDevice.value }
      : null;
    deviceConfig.value = buildDeviceConfigFromSource(
      selectedDevice.value || { devEui },
    );
    const message =
      e instanceof Error
        ? `${e.message}. Les informations affichées proviennent des données disponibles localement.`
        : "Impossible de charger la configuration du kapteur.";
    deviceConfigError.value = message;
  }
}

async function loadDeviceThresholds(devEui) {
  try {
    const apiData = await getThresholdsAlarms(devEui);
    if (Array.isArray(apiData)) {
      const devThr = {};
      apiData.forEach((th) => {
        devThr[th.propertyName] = {
          low: th.lowValue ?? null,
          high: th.highValue ?? null,
          hysteresis: th.hysteresis ?? null,
        };
      });
      thresholdsStore.value = { ...thresholdsStore.value, [devEui]: devThr };
    }
  } catch (e) {
    console.error(e);
  }
}

async function saveDeviceConfig() {
  if (!selectedDevice.value) return;
  try {
    deviceConfigError.value = "";
    const fallbackSource = {
      ...selectedDevice.value,
      ...(deviceInfo.value || {}),
    };
    const payload = sanitiseDeviceConfigInput(
      deviceConfig.value,
      fallbackSource,
    );
    const updated = await updateDeviceConfig(
      selectedDevice.value.devEui,
      payload,
    );
    const hasBody = updated && Object.keys(updated).length > 0;
    const mergedInfo = hasBody ? updated : payload;
    deviceInfo.value = {
      ...(deviceInfo.value || {}),
      ...mergedInfo,
    };
    deviceConfig.value = buildDeviceConfigFromSource(
      deviceInfo.value,
      selectedDevice.value,
    );
    const idx = Array.isArray(fetchedDevices.value)
      ? fetchedDevices.value.findIndex(
          (d) => d.devEui === selectedDevice.value.devEui,
        )
      : -1;
    if (idx >= 0) {
      fetchedDevices.value[idx] = {
        ...fetchedDevices.value[idx],
        ...deviceInfo.value,
      };
    }
    if (selectedDevice.value) {
      selectedDevice.value = {
        ...selectedDevice.value,
        ...deviceInfo.value,
      };
    }
  } catch (e) {
    console.error(e);
    const message =
      e instanceof Error
        ? `${e.message}. Vérifiez que les champs contiennent au moins un caractère.`
        : "Impossible d'enregistrer la configuration.";
    deviceConfigError.value = message;
  }
}

const variableMap = computed(() =>
  selectedDevice.value ? getLabelMap(selectedDevice.value) : {},
);

const currentThresholds = computed(
  () => thresholdsStore.value[selectedDevice.value?.devEui] || {},
);

function saveThreshold() {
  if (!selectedDevice.value || !selectedVariable.value) return;
  const devEui = selectedDevice.value.devEui;
  const devThr = {
    ...currentThresholds.value,
    [selectedVariable.value]: {
      low: lowValue.value,
      high: highValue.value,
      hysteresis: hysteresisValue.value,
    },
  };
  thresholdsStore.value = { ...thresholdsStore.value, [devEui]: devThr };
  sendThresholdsToApi(devEui, devThr);
  selectedVariable.value = "";
  lowValue.value = null;
  highValue.value = null;
  hysteresisValue.value = null;
}

function deleteThreshold(key) {
  if (!selectedDevice.value) return;
  const devEui = selectedDevice.value.devEui;
  const devThr = { ...currentThresholds.value };
  delete devThr[key];
  thresholdsStore.value = { ...thresholdsStore.value, [devEui]: devThr };
  sendThresholdsToApi(devEui, devThr);
}

function sendThresholdsToApi(devEui, thresholds) {
  const payload = {
    [devEui]: Object.fromEntries(
      Object.entries(thresholds).map(([varName, val]) => [
        varName,
        { Low: val.low, High: val.high, Hysteresis: val.hysteresis },
      ]),
    ),
  };
  if (import.meta.env.DEV) {
    console.log(
      "sendThresholdsToApi payload",
      JSON.stringify(payload, null, 2),
    );
  }
  postThresholdsAlarms(payload)
    .then((res) => {
      if (import.meta.env.DEV) {
        console.log("postThresholdsAlarms response", res);
      }
      return res;
    })
    .catch((err) => console.error("postThresholdsAlarms error", err));
}
</script>

<style scoped>
/* minimal styling */
</style>

