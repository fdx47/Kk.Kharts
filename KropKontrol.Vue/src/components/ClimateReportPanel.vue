<template>
  <div class="report-container">
    <div class="text-center my-4" v-if="loading">
      <div class="spinner-border" role="status">
        <span class="visually-hidden">Chargement...</span>
      </div>
    </div>
    <div class="alert alert-danger mt-3" v-if="error">{{ error }}</div>

    <div v-if="reportReady" class="report-content">
      <div class="card mb-3">
        <div class="card-header">
          Recapitulatif Klimatique des 7 derniers jours
        </div>
        <div class="card-body p-0">
          <table class="table table-sm table-striped mb-0">
            <thead>
              <tr>
                <th class="text-center">Date</th>
                <th class="text-center">Lever</th>
                <th class="text-center">Coucher</th>
                <th class="text-center">Moy 24h (degC)</th>
                <th class="text-center">Moy Jour (degC)</th>
                <th class="text-center">Moy Nuit (degC)</th>
                <th class="text-center">VPD 3-8 (h)</th>
                <th class="text-center">Rosee &lt; T (h)</th>
                <th v-if="isEm300Di" class="text-center">
                  Pluviometrie cumulee (mm)
                </th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="d in dailyReports" :key="d.date">
                <td class="text-center">{{ d.date }}</td>
                <td class="text-center">{{ d.sunrise }}</td>
                <td class="text-center">{{ d.sunset }}</td>
                <td class="text-center">{{ d.tempAvg24.toFixed(2) }}</td>
                <td class="text-center">{{ d.tempAvgDay.toFixed(2) }}</td>
                <td class="text-end">{{ d.tempAvgNight.toFixed(2) }}</td>
                <td class="text-end">{{ d.hoursVpd.toFixed(2) }}</td>
                <td class="text-end">{{ d.hoursDewBelowTemp.toFixed(2) }}</td>
                <td v-if="isEm300Di" class="text-end">
                  {{ d.rain.toFixed(2) }}
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <div
      v-else-if="!loading"
      class="placeholder-message d-flex align-items-center justify-content-center"
    >
      Selectionnez une sonde dans la sidebar pour afficher le rapport.
    </div>
  </div>
</template>
<script setup>
import { ref, computed, watch, onMounted, onBeforeUnmount, inject } from "vue";
import { useDevices } from "@/composables/useDevices.js";
import { useDeviceData } from "../composables/useDeviceData.js";
import SunCalc from "suncalc";
import { DateTime } from "luxon";
import { useGeolocation } from "../composables/useGeolocation.js";

const props = defineProps({
  devEui: { type: String, required: false },
});

const { devices: fetchedDevices, loadDevices } = useDevices();
const injectedVirtual = inject("virtualDevices", { virtualDevices: ref([]) });
const virtualDevices = injectedVirtual?.virtualDevices ?? ref([]);
const devices = computed(() => [
  ...(fetchedDevices.value || []),
  ...(virtualDevices.value || []),
]);

const selectedDevice = computed(
  () => devices.value.find((d) => d.devEui === props.devEui) || null,
);

const isEm300Di = computed(() => Number(selectedDevice.value?.model) === 2);

const { fetchDeviceData } = useDeviceData();
const { coords, zone, requestLocation } = useGeolocation();

const loading = ref(false);
const error = ref("");
const reportReady = ref(false);
const dailyReports = ref([]);
let workerInstance = null;
let workerSeq = 0;
const workerPending = new Map();

function createWorkerAbortError() {
  const err = new Error("climateReportWorker aborted");
  err.__climateWorkerAbort = true;
  return err;
}

function terminateWorker(error) {
  if (workerInstance) {
    workerInstance.terminate();
    workerInstance = null;
  }
  if (workerPending.size) {
    const reason = error ?? createWorkerAbortError();
    workerPending.forEach(({ reject }) => reject(reason));
  }
  workerPending.clear();
}

function ensureWorker() {
  if (!workerInstance) {
    workerInstance = new Worker(
      new URL("../workers/climateReportWorker.js", import.meta.url),
      { type: "module" },
    );
    workerInstance.onmessage = (event) => {
      const { id, ok, result, error } = event.data || {};
      const pending = workerPending.get(id);
      if (!pending) return;
      workerPending.delete(id);
      if (ok) {
        pending.resolve(result);
      } else {
        pending.reject(new Error(error || "Worker error"));
      }
    };
    workerInstance.onerror = (err) => {
      const errorObj =
        err instanceof Error
          ? err
          : new Error(err?.message || "Worker execution failed");
      terminateWorker(errorObj);
    };
  }
  return workerInstance;
}

function runWorker(payload) {
  const worker = ensureWorker();
  const id = ++workerSeq;
  return new Promise((resolve, reject) => {
    workerPending.set(id, { resolve, reject });
    try {
      worker.postMessage({ id, ...payload });
    } catch (err) {
      workerPending.delete(id);
      reject(err);
    }
  });
}
function cancelPendingWork(reason) {
  if (!workerPending.size) return;
  const error = reason ?? createWorkerAbortError();
  workerPending.forEach(({ reject }) => reject(error));
  workerPending.clear();
}
async function processData(measures) {
  dailyReports.value = [];
  if (!Array.isArray(measures) || !measures.length) {
    return;
  }

  const entries = [];
  for (const measure of measures) {
    if (!measure || measure.timestamp == null) continue;
    const rawTs =
      Number.isFinite(measure._ts)
        ? Number(measure._ts)
        : measure.timestamp instanceof Date
          ? measure.timestamp.getTime()
          : typeof measure.timestamp === "number"
            ? Number(measure.timestamp)
            : Date.parse(measure.timestamp);
    if (!Number.isFinite(rawTs)) continue;
    const temperature = Number(measure.temperature);
    const humidity = Number(measure.humidity);
    if (!Number.isFinite(temperature) || !Number.isFinite(humidity)) continue;
    const waterValue =
      measure.water == null || Number.isNaN(Number(measure.water))
        ? null
        : Number(measure.water);
    const dayKeyValue =
      typeof measure._dayKey === "string" && measure._dayKey
        ? measure._dayKey
        : undefined;
    entries.push({
      ts: rawTs,
      temperature,
      humidity,
      water: waterValue,
      dayKey: dayKeyValue,
    });
  }

  if (!entries.length) {
    return;
  }

  entries.sort((a, b) => a.ts - b.ts);

  const now = DateTime.now().setZone(zone.value);
  const todaySunTimes = SunCalc.getTimes(
    now.toJSDate(),
    coords.value.lat,
    coords.value.lon,
  );
  const todaySunrise = DateTime.fromJSDate(todaySunTimes.sunrise, {
    zone: zone.value,
  });

  const lastCompletedDay =
    now < todaySunrise
      ? now.minus({ days: 2 }).startOf("day")
      : now.minus({ days: 1 }).startOf("day");

  const sunTimes = [];
  const reportDays = [];
  for (let i = 6; i >= 0; i--) {
    reportDays.push(lastCompletedDay.minus({ days: i }));
  }

  const sunCalcDays = [...reportDays, lastCompletedDay.plus({ days: 1 })];
  for (const day of sunCalcDays) {
    const safeDateForSunCalc = day.set({ hour: 12 }).toJSDate();
    sunTimes.push(
      SunCalc.getTimes(safeDateForSunCalc, coords.value.lat, coords.value.lon),
    );
  }

  const periods = [];
  for (let i = 0; i < 7; i++) {
    periods.push({
      startMs: sunTimes[i].sunrise.getTime(),
      sunsetMs: sunTimes[i].sunset.getTime(),
      endMs: sunTimes[i + 1].sunrise.getTime(),
    });
  }

  let stats;
  cancelPendingWork();
  try {
    stats = await runWorker({
      measures: entries,
      periods,
      includeRain: isEm300Di.value,
    });
  } catch (err) {
    if (!err?.__climateWorkerAbort) {
      console.error("climateReportWorker error", err);
    }
    throw err;
  }

  const reports = [];
  for (let i = 0; i < 7; i++) {
    const periodStart = sunTimes[i].sunrise;
    const sunset = sunTimes[i].sunset;
    const {
      tempAvg24 = 0,
      tempAvgDay = 0,
      tempAvgNight = 0,
      hoursVpd = 0,
      hoursDewBelowTemp = 0,
      rain = 0,
    } = (Array.isArray(stats) && stats[i]) || {};

    reports.push({
      date: DateTime.fromJSDate(periodStart, { zone: zone.value }).toFormat(
        "dd/LL/yyyy",
      ),
      sunrise: DateTime.fromJSDate(periodStart, { zone: zone.value }).toFormat(
        "HH:mm",
      ),
      sunset: DateTime.fromJSDate(sunset, { zone: zone.value }).toFormat(
        "HH:mm",
      ),
      tempAvg24,
      tempAvgDay,
      tempAvgNight,
      hoursVpd,
      hoursDewBelowTemp,
      rain: isEm300Di.value ? Math.max(0, rain) : 0,
    });
  }

  dailyReports.value = reports;
}
async function generateReport() {
  if (!props.devEui) return;
  loading.value = true;
  error.value = "";
  reportReady.value = false;
  try {
    const endDT = DateTime.now().setZone(zone.value);
    const startDT = endDT.minus({ days: 8 });
    const data = await fetchDeviceData(
      props.devEui,
      Number(selectedDevice.value?.model ?? 7),
      startDT.toUTC().toISO(),
      endDT.toUTC().toISO(),
    );
    await processData(data);
    reportReady.value = true;
  } catch (e) {
    if (e?.__climateWorkerAbort) {
      return;
    }
    console.error(e);
    error.value = e.message || "Erreur lors de la recuperation des donnees";
  } finally {
    loading.value = false;
  }
}

watch(() => props.devEui, generateReport);

onMounted(async () => {
  if (!Array.isArray(fetchedDevices.value) || fetchedDevices.value.length === 0) {
    await loadDevices().catch(console.error);
  }
  requestLocation();
  if (props.devEui) {
    await generateReport();
  }
});

onBeforeUnmount(() => {
  terminateWorker();
});

</script>

<style scoped>
.report-container {
  max-width: 900px;
  margin: 0 auto;
  padding-bottom: 2rem;
}
.placeholder-message {
  min-height: 30vh;
  color: #82be20ff;
  font-size: 1.1rem;
}
.card-header {
  background-color: #82be20ff;
  color: #fff;
}
.spinner-border {
  color: #82be20ff;
}
</style>
