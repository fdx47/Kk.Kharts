<template>
  <div class="snapshot-card">
    <div class="snapshot-title">{{ displayedTitle }}</div>
    <div class="battery-gauge" ref="batteryGaugeRef"></div>
    <div class="snapshot-chart" ref="chartRef"></div>
  </div>
</template>

<script setup>
import { onMounted, ref, watch, computed } from "vue";
import { useDeviceData } from "@/composables/useDeviceData.js";
import { downsampleLTTB } from "../utils/lttb.js";
import { useComputedVars } from "@/composables/useComputedVars.js";
import { calculateCumulativeSeries } from "@/composables/useCumulativeCalculations.js";
import { useVirtualDevices } from "@/composables/useVirtualDevices.js";

const props = defineProps({
  devEui: String,
  title: String,
  labelMap: Object,
  variables: { type: Array, default: () => [] },
  device: { type: Object, default: () => ({}) },
  intervalDays: { type: Number, default: undefined },
  startDate: { type: String, default: undefined },
  endDate: { type: String, default: undefined },
  startDateTime: { type: [String, Date], default: undefined },
  endDateTime: { type: [String, Date], default: undefined },
});

// Compute the title dynamically so snapshots reflect device renaming
const displayedTitle = computed(
  () => props.device?.description || props.device?.deviceName || props.title,
);

let ApexChartsModule = null;
async function ensureApexCharts() {
  if (!ApexChartsModule) {
    const { default: ApexCharts } = await import("apexcharts");
    ApexChartsModule = ApexCharts;
  }
  return ApexChartsModule;
}

const chartRef = ref(null);
const batteryGaugeRef = ref(null);
const chartInstance = ref(null);
const batteryGaugeInstance = ref(null);
const batteryValue = ref(null);
const plantingDateMs = computed(() => {
  const iso = props.device?.group?.metadata?.parcel?.plantingDate;
  if (!iso) return null;
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return null;
  date.setHours(0, 0, 0, 0);
  return date.getTime();
});
const gaugeOptions = computed(() => ({
  chart: {
    type: "radialBar",
    height: 50,
    width: 50,
    sparkline: { enabled: true },
  },
  series:
    batteryValue.value != null
      ? [batteryValue.value]
      : [
          (() => {
            // Try to find the last positive battery value from device data history
            if (props.device?.history && Array.isArray(props.device.history)) {
              for (let i = props.device.history.length - 1; i >= 0; i--) {
                const val =
                  props.device.history[i].Battery ??
                  props.device.history[i].battery;
                if (val != null && val > 0) return val;
              }
            }
            return 0;
          })(),
        ],
  plotOptions: {
    radialBar: {
      hollow: { size: "40%" },
      track: {
        background: "#eee",
        strokeWidth: "120%",
      },
      dataLabels: {
        show: true,
        value: {
          offsetY: 7,
          show: true,
          fontSize: "16px",
          fontWeight: 700,
          color: "#333",
          formatter: function (val) {
            return val != null ? `${Math.round(val)}` : "--";
          },
        },
        name: { show: false },
      },
      strokeWidth: 50, // Épaisseur du trait de la gauge
    },
  },
  colors: [
    batteryValue.value != null && batteryValue.value <= 20
      ? "#dc3545"
      : "#28a745",
  ],
  labels: [],
  tooltip: { enabled: false },
}));

const { fetchDeviceData, getCachedDeviceData } = useDeviceData();
const SEP = "|";
const MAX_SERIES_POINTS = 200; // Downsampling threshold
// Supporte les anciennes clés et les clefs actuelles
const COLUMN_VARS = [
  "deltaVolume",
  "deltaVolume_mm",
  "volumeDelta",
  "volumeDelta_mm",
];

function parseVarKey(key) {
  const idx = key.lastIndexOf(SEP);
  if (idx === -1) return { field: key, devEui: null };
  return { field: key.slice(0, idx), devEui: key.slice(idx + 1) };
}

const computedVars = computed(() => {
  if (props.device?.isVirtual && props.device?.group) {
    const defs = {};
    props.device.group.devEuis.forEach((devEui) => {
      const model = props.device.group.deviceModels?.[devEui];
      if (model != null) {
        const cv = useComputedVars(Number(model));
        Object.keys(cv).forEach((k) => {
          defs[`${k}${SEP}${devEui}`] = cv[k];
        });
      }
    });
    return defs;
  }
  return useComputedVars(props.device?.model);
});
const labelMapWithComputed = computed(() => {
  const base = { ...props.labelMap };
  const defs = computedVars.value;
  Object.keys(defs).forEach((k) => {
    if (!base[k]) base[k] = defs[k].label;
  });
  return base;
});

// Accès direct aux groupes virtuels pour résolution des modèles si device n'est pas encore injecté
const { groups } = useVirtualDevices();

onMounted(async () => {
  const ApexCharts = await ensureApexCharts();
  chartInstance.value = new ApexCharts(chartRef.value, {
    chart: {
      type: "line",
      height: "100%",
      animations: { enabled: false },
      toolbar: { show: false },
    },
    legend: { show: false },
    series: [],
    stroke: { width: 2 },
    plotOptions: { bar: { columnWidth: "8px" } },
    xaxis: {
      type: "datetime",
      labels: { datetimeUTC: false, show: false },
    },
    yaxis: [
      {
        labels: { show: false },
      },
    ],
    tooltip: {
      x: { format: "dd/MM HH:mm" },
      y: {
        formatter: (val) => (val != null && !isNaN(val) ? val.toFixed(1) : val),
      },
    },
    grid: { padding: { left: 8, right: 8 } },
    noData: { text: "Chargement..." },
  });
  await chartInstance.value.render();
  batteryGaugeInstance.value = new ApexCharts(
    batteryGaugeRef.value,
    gaugeOptions.value,
  );
  await batteryGaugeInstance.value.render();
  await updateChart();
  if (import.meta.env.DEV) {
    console.debug("SnapshotCard mounted for", props.devEui);
  }
});

watch(
  () => props.device?.model,
  async (model) => {
    if (model && chartInstance.value) await updateChart();
  },
);

watch(batteryValue, () => {
  if (batteryGaugeInstance.value) {
    if (import.meta.env.DEV) {
      console.debug("batteryValue updated", batteryValue.value);
      console.debug("gauge options", gaugeOptions.value);
    }
    // Update the gauge value and colors when the battery level changes
    batteryGaugeInstance.value.updateSeries(
      batteryValue.value != null ? [batteryValue.value] : [],
    );
    batteryGaugeInstance.value.updateOptions({
      colors: gaugeOptions.value.colors,
    });
  }
});

async function updateChart() {
  if (!chartInstance.value) return;
  // Détermine la fenêtre à utiliser: priorité aux dates explicites, sinon intervalle, sinon 36h
  let end = new Date();
  let start;
  if (
    props.startDateTime ||
    props.endDateTime ||
    props.startDate ||
    props.endDate
  ) {
    if (props.startDateTime) start = new Date(props.startDateTime);
    else if (props.startDate) {
      start = new Date(props.startDate);
      start.setHours(0, 0, 0, 0);
    }
    if (props.endDateTime) end = new Date(props.endDateTime);
    else if (props.endDate) {
      end = new Date(props.endDate);
      end.setHours(23, 59, 59, 999);
    }
    if (!start) start = new Date(end.getTime() - 36 * 60 * 60 * 1000);
  } else if (
    typeof props.intervalDays === "number" &&
    !Number.isNaN(props.intervalDays)
  ) {
    start = new Date(end.getTime() - props.intervalDays * 24 * 60 * 60 * 1000);
  } else {
    start = new Date(end.getTime() - 36 * 60 * 60 * 1000);
  }
  const minMs = plantingDateMs.value;
  if (minMs) {
    if (end.getTime() < minMs) {
      end = new Date(minMs);
    }
    if (start.getTime() < minMs) {
      start = new Date(minMs);
    }
  }
  if (start.getTime() > end.getTime()) {
    end = new Date(start.getTime());
  }
  const startISO = start.toISOString();
  const endISO = end.toISOString();
  let data = await getCachedDeviceData(
    props.devEui,
    props.device?.model,
    startISO,
    endISO,
  );
  const coversRange =
    data.length &&
    Date.parse(data[0].timestamp) <= start.getTime() &&
    Date.parse(data[data.length - 1].timestamp) >= end.getTime();
  if (!coversRange) {
    const fresh = await fetchDeviceData(
      props.devEui,
      props.device?.model,
      startISO,
      endISO,
    );
    data = fresh.filter((d) => {
      const ts = Date.parse(d.timestamp);
      return ts >= start.getTime() && ts <= end.getTime();
    });
  }
  if (import.meta.env.DEV) {
    console.debug("fetched", data.length, "measures for", props.devEui);
  }
  // Determine the most recent non-null battery value
  let lastBattery = null;
  for (let i = data.length - 1; i >= 0; i--) {
    const val = data[i].Battery ?? data[i].battery;
    if (val != null) {
      lastBattery = val;
      break;
    }
  }
  batteryValue.value = lastBattery;
  if (import.meta.env.DEV) {
    console.debug("latest battery value", batteryValue.value);
  }

  const vars = props.variables.length
    ? props.variables
    : Object.keys(labelMapWithComputed.value);
  const series = vars.map((key) => {
    const { field, devEui } = parseVarKey(key);
    const name = labelMapWithComputed.value[key] || key;
    const comp = computedVars.value[key];
    const filtered = devEui ? data.filter((d) => d.devEui === devEui) : data;
    const isColumn = COLUMN_VARS.includes(field);

    // Modèle 61 (PAR) – variables cumulées DLI / Rayonnement
    let devModel = props.device?.isVirtual
      ? Number(props.device?.group?.deviceModels?.[devEui])
      : Number(props.device?.model);
    if (
      (!devModel || Number.isNaN(devModel)) &&
      props.devEui?.startsWith("group-") &&
      devEui
    ) {
      const gid = props.devEui.slice("group-".length);
      const gr = groups?.value?.find?.((g) => g.id === gid);
      const m2 = gr?.deviceModels?.[devEui];
      if (m2 != null) devModel = Number(m2);
    }

    let points = [];
    if (devModel === 61 && (field === "DLI" || field === "Rayonnement")) {
      const dayMap = {};
      filtered.forEach((d) => {
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
      const acc = [];
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
          const ts = p.ts;
          if (field === "DLI") acc.push({ x: ts, y: p.dli });
          else acc.push({ x: ts, y: p.joules_cm2 });
        });
      });
      points = acc;
    } else {
      points = filtered
        .map((d) => {
          const val = comp ? comp.compute(d) : d[field];
          if (val == null || isNaN(val)) return null;
          const ts =
            d.timestamp instanceof Date
              ? d.timestamp.getTime()
              : new Date(d.timestamp).getTime();
          return { x: ts, y: val };
        })
        .filter(Boolean);
    }
    if (points.length > MAX_SERIES_POINTS) {
      points = downsampleLTTB(points, MAX_SERIES_POINTS);
    }
    return { name, data: points, ...(isColumn ? { type: "column" } : {}) };
  });

  const seriesWithAxis = series.map((s, i) => ({ ...s, yAxisIndex: i }));
  const yaxisOptions = series.map((s, i) => ({
    opposite: Boolean(i % 2),
    labels: { show: false },
  }));
  const strokeWidths = seriesWithAxis.map((s) =>
    s.type === "column" ? 0.5 : 2,
  );
  const fillOpacities = seriesWithAxis.map((s) =>
    s.type === "column" ? 1 : 1,
  );

  chartInstance.value.updateOptions({
    series: seriesWithAxis,
    yaxis: yaxisOptions,
    xaxis: { labels: { show: false } },
    legend: { show: false },
    stroke: { width: strokeWidths, colors: undefined, opacity: 1 },
    fill: { type: "solid", opacity: fillOpacities },
    plotOptions: { bar: { columnWidth: "80%" } },
    colors: [
      "#008FFB",
      "#00E396",
      "#FEB019",
      "#FF4560",
      "#775DD0",
      "#00D9E9",
      "#FFB900",
    ],
  });
}

defineExpose({ updateChart });
</script>

<style scoped>
.snapshot-card {
  position: relative;
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  min-height: 0;
}

.snapshot-title {
  font-weight: bold;
  text-align: center;
  margin-bottom: 0.25rem;
  font-size: clamp(0.8rem, 2.5vw, 1rem);
  padding: 0 2.5rem 0 0.5rem;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  line-height: 1.3;
}

.snapshot-chart {
  flex-grow: 1;
  min-height: 80px;
}

.battery-gauge {
  position: absolute;
  top: 0.25rem;
  right: 0.25rem;
  width: 50px;
  height: 50px;
}

/* ============================================
   Mobile-First Responsive Improvements
   ============================================ */

@media (max-width: 767.98px) {
  .snapshot-title {
    font-size: 0.85rem;
    padding-right: 3rem;
  }
  
  .battery-gauge {
    width: 45px;
    height: 45px;
  }
  
  .snapshot-chart {
    min-height: 100px;
  }
}

@media (max-width: 575.98px) {
  .snapshot-title {
    font-size: 0.8rem;
    margin-bottom: 0.15rem;
  }
  
  .battery-gauge {
    width: 40px;
    height: 40px;
    top: 0.15rem;
    right: 0.15rem;
  }
}

/* Landscape mobile */
@media (max-height: 500px) and (orientation: landscape) {
  .snapshot-chart {
    min-height: 60px;
  }
  
  .battery-gauge {
    width: 35px;
    height: 35px;
  }
}
</style>
