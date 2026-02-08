// Utilities for chart calculations extracted from ChartCard.vue
import { toRaw } from "vue";
import SunCalc from "suncalc";
import { DateTime } from "luxon";
import { alignAndInterpolateSeries } from "../utils/interpolateSeries.js";
import { analyzeWateringSeries } from "../services/dataProcessor.js";
import {
  calculateDailyCumulativeValues,
  calculateCumulativeSeries,
} from "./useCumulativeCalculations.js";
import { downsampleLTTB } from "../utils/lttb.js";

const SEP = "|";
const COLUMN_VARS = ["volumeDelta", "volumeDelta_mm"];
const COLUMN_WIDTH = "8px"; // ou '28px' pour une largeur fixe
// Formatter de jour local (Europe/Paris) pour clé AAAA-MM-JJ
const DAY_FMT = new Intl.DateTimeFormat("en-CA", {
  timeZone: "Europe/Paris",
  year: "numeric",
  month: "2-digit",
  day: "2-digit",
});

export function parseVarKey(key) {
  const idx = key.lastIndexOf(SEP);
  if (idx === -1) return { field: key, devEui: null };
  return { field: key.slice(0, idx), devEui: key.slice(idx + 1) };
}

function pad(n) {
  return n < 10 ? "0" + n : "" + n;
}

export function formatDateTime(d) {
  return (
    `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}` +
    `T${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`
  );
}

const sunAnnotationsCache = new Map();

export function clearSunAnnotationsCache() {
  sunAnnotationsCache.clear();
}

export function computeSunAnnotations(
  start,
  end,
  zoneRef,
  coordsRef,
  sunriseOffsetRef,
  sunsetOffsetRef,
) {
  if (!start || !end) return [];
  // Include zone, coordinates and offset in the cache key so different
  // charts (or updates) with distinct settings don't reuse stale entries.
  const zoneKey = zoneRef && zoneRef.value ? String(zoneRef.value) : "";
  const lat =
    coordsRef && coordsRef.value && coordsRef.value.lat != null
      ? coordsRef.value.lat
      : "0";
  const lon =
    coordsRef && coordsRef.value && coordsRef.value.lon != null
      ? coordsRef.value.lon
      : "0";
  const sunriseMin =
    sunriseOffsetRef && sunriseOffsetRef.value != null
      ? Number(sunriseOffsetRef.value)
      : 0;
  const sunsetMin =
    sunsetOffsetRef && sunsetOffsetRef.value != null
      ? Number(sunsetOffsetRef.value)
      : 0;
  const key = `${start.getTime()}_${end.getTime()}_${zoneKey}_${lat}_${lon}_${sunriseMin}_${sunsetMin}`;
  if (sunAnnotationsCache.has(key)) return sunAnnotationsCache.get(key);
  const showLabels = end - start <= 72 * 60 * 60 * 1000;
  const annotations = [];
  let day = DateTime.fromJSDate(start, { zone: zoneRef.value }).startOf("day");
  const endDay = DateTime.fromJSDate(end, { zone: zoneRef.value }).startOf(
    "day",
  );
  for (; day <= endDay; day = day.plus({ days: 1 })) {
    const safeDate = day.set({ hour: 12 }).toJSDate();
    const t = SunCalc.getTimes(
      safeDate,
      coordsRef.value.lat,
      coordsRef.value.lon,
    );
    const midnight = day.startOf("day").toJSDate();
    const nextMidnight = day.plus({ days: 1 }).startOf("day").toJSDate();
    annotations.push({
      x: midnight.getTime(),
      x2: t.sunrise.getTime(),
      fillColor: "#e9ecef",
      opacity: 0.4,
      zIndex: 0,
    });
    annotations.push({
      x: t.sunset.getTime(),
      x2: nextMidnight.getTime(),
      fillColor: "#e9ecef",
      opacity: 0.4,
      zIndex: 0,
    });
    annotations.push({
      x: t.sunrise.getTime(),
      strokeDashArray: 2,
      borderColor: "#FFA500",
      ...(showLabels
        ? { label: { text: "☀️", offsetX: 0, orientation: "horizontal" } }
        : {}),
    });
    const sunrisePlus = new Date(t.sunrise.getTime() + sunriseMin * 60 * 1000);
    let offsetRef = { value: sunriseMin };
    annotations.push({
      x: sunrisePlus.getTime(),
      strokeDashArray: 2,
      borderColor: "#00BFFF",
      ...(showLabels
        ? {
            label: {
              text: `☀️+ ${offsetRef.value}`,
              offsetX: 0,
              orientation: "horizontal",
            },
          }
        : {}),
    });
    const sunsetMinus = new Date(t.sunset.getTime() - sunsetMin * 60 * 1000);
    offsetRef = { value: sunsetMin };
    annotations.push({
      x: sunsetMinus.getTime(),
      strokeDashArray: 2,
      borderColor: "#00BFFF",
      ...(showLabels
        ? {
            label: {
              text: `🌙- ${offsetRef.value}`,
              offsetX: 0,
              orientation: "horizontal",
            },
          }
        : {}),
    });
    annotations.push({
      x: t.sunset.getTime(),
      strokeDashArray: 2,
      borderColor: "#FFA500",
      ...(showLabels
        ? { label: { text: "🌙", offsetX: 0, orientation: "horizontal" } }
        : {}),
    });
  }
  sunAnnotationsCache.set(key, annotations);
  return annotations;
}

export function useChartCalculations(options) {
  const {
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
    baseTimeRef,
  } = options;

  const MIN_DATA_WINDOW_DAYS = options.MIN_DATA_WINDOW_DAYS || 7;
  const MAX_SERIES_POINTS = options.MAX_SERIES_POINTS || 3500;

  // Cache des valeurs calculées pour la fenêtre courante
  // key: varKey -> Map<ts:number, y:number>
  const valueCache = new Map();

  // Worker helper for main series precompute
  let __seriesWorker = null;
  function getSeriesWorker() {
    if (!__seriesWorker) {
      try {
        __seriesWorker = new Worker(
          new URL("../workers/chartWorker.js", import.meta.url),
          { type: "module" },
        );
      } catch (err) {
        console.warn("getSeriesWorker: worker unavailable", err);
        __seriesWorker = null;
      }
    }
    return __seriesWorker;
  }

  // Append/update helpers for light refresh
  function tryAppendOrUpdateSeries(seriesWithAxis, yaxisOptions, annotations) {
    const chart = chartInstance.value;
    const globals = chart?.w?.globals;
    const existingNames = globals?.seriesNames || [];
    const sameCount = existingNames.length === seriesWithAxis.length;
    if (!sameCount || existingNames.length === 0) return false;
    // Names and types must match to safely append
    for (let i = 0; i < seriesWithAxis.length; i++) {
      const s = seriesWithAxis[i];
      const nameOk = existingNames[i] === (s.name || existingNames[i]);
      if (!nameOk) return false;
      if (!Array.isArray(globals.seriesX?.[i])) return false;
      if (globals.seriesX[i].length === 0) return false;
    }
    const payload = [];
    let hasNew = false;
    for (let i = 0; i < seriesWithAxis.length; i++) {
      const xs = chart.w.globals.seriesX[i];
      const lastX = xs[xs.length - 1];
      const newPts = (seriesWithAxis[i].data || []).filter(
        (p) => p && p.x > lastX,
      );
      if (newPts.length) hasNew = true;
      payload.push({ data: newPts });
    }
    if (!hasNew) return false;
    // Minimal options update first, then append
    chart.updateOptions(
      { yaxis: yaxisOptions, annotations: { xaxis: annotations } },
      false,
      false,
      false,
    );
    try {
      chart.appendData(payload);
    } catch {
      return false;
    }
    return true;
  }

  function getAnnotations(start, end) {
    return computeSunAnnotations(
      start,
      end,
      zone,
      coords,
      sunriseAnnotationOffset,
      sunsetAnnotationOffset,
    );
  }

  function updateCustomOverviewChart(seriesWithAxis) {
    if (!overviewChartInstance.value) return;
    const palette = chartInstance.value?.w?.globals?.colors || [];
    const raw = Array.isArray(seriesWithAxis)
      ? seriesWithAxis.map((s) => ({
          name: s.name,
          data: s.data,
          ...(s.type ? { type: s.type } : {}),
        }))
      : [];
    const strokeWidths = raw.map((s) => (s.type === "column" ? 0 : 1));
    const fillOpacities = raw.map((s) => (s.type === "column" ? 1 : 0));
    const yaxis =
      raw.length > 1
        ? raw.map((_, i) => ({
            show: true,
            opposite: false,
            title: { text: "" },
            labels: { show: false, style: { colors: palette[i] } },
          }))
        : [
            {
              show: true,
              title: { text: "" },
              labels: { show: false, style: { colors: palette[0] } },
            },
          ];
    // Mise à jour légère: options minimales puis séries
    overviewChartInstance.value.updateOptions(
      {
        legend: { show: false },
        fill: { type: "solid", opacity: fillOpacities },
        stroke: { width: strokeWidths },
        yaxis,
        plotOptions: { bar: { columnWidth: COLUMN_WIDTH } },
      },
      false,
      false,
      false,
    );
    overviewChartInstance.value.updateSeries(raw, false);
    if (startDate.value && endDate.value) {
      const selStart = new Date(startDate.value).getTime();
      const selEnd = new Date(endDate.value).getTime();
      overviewChartInstance.value.updateOptions({
        chart: { selection: { xaxis: { min: selStart, max: selEnd } } },
      });
    }
  }

  function updateCustomSeriesChart(seriesData) {
    if (!chartInstance.value) return;
    const palette = chartInstance.value?.w?.globals?.colors || [];
    if (seriesData && seriesData.length > 0) {
      const rawSeries = alignAndInterpolateSeries(toRaw(seriesData));
      const seriesWithAxis = rawSeries.map((s, i) => ({ ...s, yAxisIndex: i }));
      const strokeWidths = seriesWithAxis.map((s) =>
        s.type === "column" ? 0 : 2,
      );
      let startTs = null;
      let endTs = null;
      seriesWithAxis.forEach((s) => {
        if (s.data.length) {
          const first =
            s.data[0].x instanceof Date ? s.data[0].x.getTime() : s.data[0].x;
          const last =
            s.data[s.data.length - 1].x instanceof Date
              ? s.data[s.data.length - 1].x.getTime()
              : s.data[s.data.length - 1].x;
          if (startTs == null || first < startTs) startTs = first;
          if (endTs == null || last > endTs) endTs = last;
        }
      });
      let rangeStart = startDate.value
        ? new Date(startDate.value)
        : startTs != null
          ? new Date(startTs)
          : null;
      let rangeEnd = endDate.value
        ? new Date(endDate.value)
        : endTs != null
          ? new Date(endTs)
          : null;
      if (rangeStart && !startDate.value) rangeStart.setHours(0, 0, 0, 0);
      if (rangeEnd && !endDate.value) rangeEnd.setHours(23, 59, 59, 999);
      const annotations = getAnnotations(rangeStart, rangeEnd);
      // Mise à jour légère: yaxis + annotations, puis séries
      chartInstance.value.updateOptions(
        {
          yaxis: seriesWithAxis.map((s, i) => {
            const scale = customScales.value[s.name] || {};
            // Só usar min/max se forem números válidos e não vazios
            const hasMin = scale.min !== "" && scale.min != null && !isNaN(Number(scale.min));
            const hasMax = scale.max !== "" && scale.max != null && !isNaN(Number(scale.max));
            return {
              title: { text: "", style: { color: palette[i] } },
              opposite: Boolean(i % 2),
              forceNiceScale: !hasMin && !hasMax, // auto-scale mais bonito quando não há limites manuais
              labels: {
                style: { colors: palette[i] },
                formatter: (val) =>
                  val != null && !isNaN(val) ? Number(val).toFixed(1) : "",
              },
              min: hasMin ? Number(scale.min) : undefined,
              max: hasMax ? Number(scale.max) : undefined,
            };
          }),
          annotations: { xaxis: annotations },
          plotOptions: { bar: { columnWidth: COLUMN_WIDTH } },
          stroke: { width: strokeWidths },
          noData: { text: "" },
        },
        false,
        false,
        false,
      );
      chartInstance.value.updateSeries(seriesWithAxis, false);
      updateCustomOverviewChart(seriesWithAxis);
    } else {
      chartInstance.value.updateOptions(
        {
          series: [],
          noData: { text: "En attente de données..." },
        },
        false,
        false,
        false,
      );
      updateCustomOverviewChart([]);
    }
  }

  function getPlantingDateMs() {
    const iso = props.device?.group?.metadata?.parcel?.plantingDate;
    if (!iso) return null;
    const date = new Date(iso);
    if (Number.isNaN(date.getTime())) return null;
    date.setHours(0, 0, 0, 0);
    return date.getTime();
  }

  async function updateChart(useCacheOnly = false) {
    // Emit the update as soon as possible so parent components
    // can persist the configuration even if the chart fetch is
    // interrupted (e.g. when navigating away).
    if (emitUpdated) emitUpdated();

    if (useCustomSeries.value) {
      updateCustomSeriesChart(props.series);
      return;
    }
    if (!props.devEui) return;
    if (!chartInstance.value) return;
    if (!selectedVariables.value.length) {
      currentData.value = [];
      chartInstance.value.updateOptions({
        series: [],
        annotations: { xaxis: [] },
        noData: { text: "Sélectionnez des variables" },
      });
      if (overviewChartInstance.value) {
        overviewChartInstance.value.updateOptions({ series: [] });
      }
      return;
    }

    try {
      let displayStart;
      let displayEnd;
      if (customDatesActive.value && startDateTime.value && endDateTime.value) {
        displayStart = new Date(startDateTime.value);
        displayEnd = new Date(endDateTime.value);
      } else if (customDatesActive.value && startDate.value && endDate.value) {
        displayStart = new Date(startDate.value);
        displayStart.setHours(0, 0, 0, 0);
        displayEnd = new Date(endDate.value);
        displayEnd.setHours(23, 59, 59, 999);
      } else {
        const base = new Date(baseTimeRef.value);
        displayEnd = new Date(base);
        displayStart = new Date(
          base.getTime() - intervalDays.value * 24 * 60 * 60 * 1000,
        );
      }
      const plantingMs = getPlantingDateMs();
      if (plantingMs != null) {
        if (displayEnd.getTime() < plantingMs) {
          displayEnd = new Date(plantingMs);
        }
        if (displayStart.getTime() < plantingMs) {
          displayStart = new Date(plantingMs);
        }
        if (displayStart.getTime() === plantingMs) {
          if (customDatesActive.value) {
            if (startDateTime.value) {
              startDateTime.value = new Date(plantingMs);
            }
            startDate.value = new Date(plantingMs).toISOString().slice(0, 10);
          } else {
            const diffDays = Math.max(
              0,
              (displayEnd.getTime() - plantingMs) / 86400000,
            );
            if (!Number.isNaN(diffDays) && diffDays !== intervalDays.value) {
              intervalDays.value = diffDays || intervalDays.value;
            }
          }
        }
        if (displayEnd.getTime() === plantingMs && customDatesActive.value) {
          endDate.value = new Date(plantingMs).toISOString().slice(0, 10);
          if (endDateTime.value) {
            endDateTime.value = new Date(plantingMs);
          }
        }
      }
      currentPeriodStart.value = new Date(displayStart);
      currentPeriodEnd.value = new Date(displayEnd);

      const requestedWindowDays =
        customDatesActive.value && startDateTime.value && endDateTime.value
          ? (endDateTime.value - startDateTime.value) / 86400000
          : intervalDays.value;
      const windowDays = Math.max(MIN_DATA_WINDOW_DAYS, requestedWindowDays);
      let fetchStart = new Date(
        displayEnd.getTime() - windowDays * 24 * 60 * 60 * 1000,
      );
      if (plantingMs != null && fetchStart.getTime() < plantingMs) {
        fetchStart = new Date(plantingMs);
      }
      const varsDevEuis = selectedVariables.value
        .map((v) => parseVarKey(v).devEui)
        .filter(Boolean);
      const devList = varsDevEuis.length
        ? Array.from(new Set(varsDevEuis))
        : undefined;

      let res = [];
      if (getCachedDeviceData) {
        res = await getCachedDeviceData(
          props.devEui,
          props.device?.model,
          fetchStart.toISOString(),
          displayEnd.toISOString(),
          devList,
        );
      }
      res.sort((a, b) => Date.parse(a.timestamp) - Date.parse(b.timestamp));
      const startMs = fetchStart.getTime();
      const endMs = displayEnd.getTime();
      const coversRange =
        res.length &&
        Date.parse(res[0].timestamp) <= startMs &&
        Date.parse(res[res.length - 1].timestamp) >= endMs;
      if (!coversRange && !useCacheOnly) {
        const fresh = await fetchDeviceData(
          props.devEui,
          props.device?.model,
          fetchStart.toISOString(),
          displayEnd.toISOString(),
          devList,
        );
        res = fresh.filter((d) => {
          const ts = Date.parse(d.timestamp);
          return ts >= startMs && ts <= endMs;
        });
      }
      // Prépare un timestamp numérique réutilisable et alimente currentData
      for (const d of res) {
        if (d) {
          if (d._ts == null) {
            const t =
              d.timestamp instanceof Date
                ? d.timestamp.getTime()
                : Date.parse(d.timestamp);
            d._ts = t;
          }
          // Clé de jour locale (Europe/Paris) pour accélérer les cumuls journaliers
          if (d._dayKey == null) {
            try {
              d._dayKey = DAY_FMT.format(d._ts);
            } catch {
              /* ignore */
            }
          }
        }
      }
      if (
        res.length &&
        res.some((entry) => entry && entry.permittivite != null)
      ) {
        try {
          const analysis = analyzeWateringSeries(res, "permittivite");
          if (Array.isArray(analysis?.points) && analysis.points.length) {
            const mapMs = new Map();
            const mapIso = new Map();
            analysis.points.forEach((pt) => {
              if (!pt) return;
              let rawVal = null;
              if (typeof pt.cumulativeDeltaPerm === "number") {
                rawVal = pt.cumulativeDeltaPerm;
              } else if (typeof pt.permDiffCumulative === "number") {
                rawVal = pt.permDiffCumulative;
              } else if (typeof pt.drainagePercentage === "number") {
                rawVal = pt.drainagePercentage;
              } else {
                const fallback =
                  pt.cumulativeDeltaPerm ??
                  pt.permDiffCumulative ??
                  pt.drainagePercentage;
                rawVal = Number.parseFloat(fallback ?? Number.NaN);
              }
              if (!Number.isFinite(rawVal)) return;
              if (Number.isFinite(pt.timeMs)) {
                mapMs.set(pt.timeMs, rawVal);
              }
              if (pt.timestamp) {
                mapIso.set(pt.timestamp, rawVal);
              }
            });
            res.forEach((d) => {
              if (!d) return;
              const ts =
                d._ts ??
                (d.timestamp instanceof Date
                  ? d.timestamp.getTime()
                  : Date.parse(d.timestamp));
              let val =
                Number.isFinite(ts) && mapMs.size ? mapMs.get(ts) : undefined;
              if (val == null && mapIso.size) {
                let iso = null;
                if (typeof d.timestamp === "string") iso = d.timestamp;
                else if (Number.isFinite(ts)) {
                  try {
                    iso = new Date(ts).toISOString();
                  } catch {
                    iso = null;
                  }
                }
                if (iso && mapIso.has(iso)) {
                  val = mapIso.get(iso);
                }
              }
              if (val == null) val = 0;
              const numericVal = Number.isFinite(val) ? val : 0;
              d.drainagePercent = Number(numericVal.toFixed(2));
              d.drainagePercentage = d.drainagePercent;
            });
          }
        } catch (err) {
          if (import.meta.env.DEV) {
            console.warn("analyzeWateringSeries failed", err);
          }
        }
      }
      currentData.value = res;
      // Réinitialise le cache pour la nouvelle fenêtre
      valueCache.clear();

      const palette = chartInstance.value?.w?.globals?.colors || [];
      const keys = selectedVariables.value.length
        ? selectedVariables.value
        : Object.keys(labelMapLocal);
      const seriesWithAxis = [];

      // Tentative: offload calcul des séries via Web Worker
      try {
        const w = getSeriesWorker();
        if (w) {
          const id = `${Date.now()}_${Math.random()}`;
          const recordsRaw = currentData.value.map((d) => ({
            ...d,
            _ts:
              d._ts ??
              (d.timestamp instanceof Date
                ? d.timestamp.getTime()
                : Date.parse(d.timestamp)),
          }));
          let records;
          try {
            records = structuredClone(recordsRaw);
          } catch {
            records = JSON.parse(JSON.stringify(recordsRaw));
          }
          const dm = props.device?.group?.deviceModels || {};
          const deviceModels = Object.keys(dm || {}).reduce((acc, k) => {
            acc[k] = Number(dm[k]);
            return acc;
          }, {});
          const defaultModel = Number(props.device?.model);
          const safeKeys = Array.from(keys);
          const seriesByKey = await new Promise((resolve, reject) => {
            let to;
            const handler = (ev) => {
              const msg = ev.data || {};
              if (msg.id !== id) return;
              w.removeEventListener("message", handler);
              clearTimeout(to);
              if (msg.ok) resolve(msg.seriesByKey);
              else reject(new Error(msg.error || "Worker error"));
            };
            w.addEventListener("message", handler);
            try {
              w.postMessage({
                id,
                cmd: "buildSeries",
                records,
                keys: safeKeys,
                deviceModels,
                defaultModel,
              });
            } catch (err) {
              try {
                w.removeEventListener("message", handler);
              } catch {}
              reject(err);
              return;
            }
            to = setTimeout(() => {
              try {
                w.removeEventListener("message", handler);
              } catch {}
              reject(new Error("Worker timeout"));
            }, 8000);
          });
          // Construire les séries et alimenter valueCache pour les stats
          keys.forEach((varKey, i) => {
            const { field } = parseVarKey(varKey);
            let data = seriesByKey[varKey] || [];
            if (data.length > MAX_SERIES_POINTS)
              data = downsampleLTTB(data, MAX_SERIES_POINTS);
            const map = new Map();
            data.forEach((p) => map.set(p.x, +p.y));
            valueCache.set(varKey, map);
            const isColumn = COLUMN_VARS.includes(field);
            seriesWithAxis.push({
              name: labelMapLocal[varKey] || varKey,
              data,
              yAxisIndex: i,
              ...(isColumn ? { type: "column" } : {}),
            });
          });

          const yaxisOptions = keys.map((varKey, i) => {
            const scale = customScales.value[varKey] || {};
            const hasMin = scale.min !== "" && scale.min != null && !isNaN(Number(scale.min));
            const hasMax = scale.max !== "" && scale.max != null && !isNaN(Number(scale.max));
            return {
              opposite: Boolean(i % 2),
              forceNiceScale: !hasMin && !hasMax,
              title: { text: "", style: { color: palette[i] } },
              labels: {
                style: { colors: palette[i] },
                formatter: (val) =>
                  val != null && !isNaN(val) ? Number(val).toFixed(1) : "",
              },
              min: hasMin ? Number(scale.min) : undefined,
              max: hasMax ? Number(scale.max) : undefined,
            };
          });
          if (import.meta.env.DEV) {
            console.groupCollapsed(
              `[DEBUG] ChartCard – données injectées (worker) (${props.title || ""})`,
            );
            seriesWithAxis.forEach((s) => {
              console.log("Série :", s.name);
              console.table(s.data.slice(0, 35));
            });
            console.groupEnd();
          }
          const annotations = getAnnotations(displayStart, displayEnd);
          const strokeWidths = seriesWithAxis.map((s) =>
            s.type === "column" ? 0 : 2,
          );
          // Tentative d'append léger si la structure de séries est inchangée
          if (
            !tryAppendOrUpdateSeries(seriesWithAxis, yaxisOptions, annotations)
          ) {
            chartInstance.value.updateOptions(
              {
                yaxis: yaxisOptions,
                annotations: { xaxis: annotations },
                plotOptions: { bar: { columnWidth: COLUMN_WIDTH } },
                stroke: { width: strokeWidths },
              },
              false,
              false,
              false,
            );
            chartInstance.value.updateSeries(seriesWithAxis, false);
          }
          chartInstance.value.zoomX(
            displayStart.getTime(),
            displayEnd.getTime(),
          );
          return;
        }
      } catch (err) {
        console.warn("Worker principal échoué, fallback local:", err);
      }

      // Pré-indexation par devEui pour éviter des filtres répétés
      const byDev = new Map();
      for (const d of currentData.value) {
        const k = d.devEui || "__default__";
        if (!byDev.has(k)) byDev.set(k, []);
        byDev.get(k).push(d);
      }
      // One-pass pre-calcul: regroupe par devEui et calcule toutes les variables sélectionnées en un passage
      const keysByDev = new Map();
      keys.forEach((k) => {
        const { devEui } = parseVarKey(k);
        const bucket = devEui || "__default__";
        if (!keysByDev.has(bucket)) keysByDev.set(bucket, []);
        keysByDev.get(bucket).push(k);
      });

      const resultMap = new Map();
      for (const [bucket, vars] of keysByDev.entries()) {
        const devData =
          bucket === "__default__"
            ? currentData.value
            : byDev.get(bucket) || [];
        if (!devData.length) {
          vars.forEach((vk) => resultMap.set(vk, []));
          continue;
        }
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
          valueCache.set(vk, new Map());
        });

        let cumSeries = null;
        if (needCum61) {
          const dayMap = {};
          devData.forEach((d) => {
            const dateStr = new Date(d._ts).toISOString().slice(0, 10);
            if (!dayMap[dateStr]) dayMap[dateStr] = [];
            dayMap[dateStr].push(d);
          });
          const dates = Object.keys(dayMap).sort();
          cumSeries = [];
          dates.forEach((dateStr) => {
            const daily = dayMap[dateStr].slice().sort((a, b) => a._ts - b._ts);
            const cumul = calculateCumulativeSeries(daily);
            cumul.forEach((p) => {
              cumSeries.push({
                ts: p.ts,
                dli: p.dli,
                joules_cm2: p.joules_cm2,
              });
            });
          });
        }

        for (const d of devData) {
          const ts = d._ts;
          for (const vk of vars) {
            const spec = computes.get(vk);
            if (!spec || spec.type === "cum61") continue;
            let y;
            if (spec.type === "comp") y = spec.fn(d);
            else y = d[spec.field];
            if (y == null || Number.isNaN(y)) continue;
            const arr = resultMap.get(vk);
            arr.push({ x: ts, y: +y });
            valueCache.get(vk)?.set(ts, +y);
          }
        }

        if (needCum61 && cumSeries) {
          for (const vk of vars) {
            const { field } = parseVarKey(vk);
            if (field !== "DLI" && field !== "Rayonnement") continue;
            const arr = [];
            if (field === "DLI")
              cumSeries.forEach((p) => arr.push({ x: p.ts, y: p.dli }));
            else
              cumSeries.forEach((p) => arr.push({ x: p.ts, y: p.joules_cm2 }));
            resultMap.set(vk, arr);
          }
        }
      }

      // Construire les séries dans l'ordre demandé
      keys.forEach((varKey, i) => {
        const { field } = parseVarKey(varKey);
        let data = resultMap.get(varKey) || [];
        if (data.length > MAX_SERIES_POINTS)
          data = downsampleLTTB(data, MAX_SERIES_POINTS);
        const isColumn = COLUMN_VARS.includes(field);
        seriesWithAxis.push({
          name: labelMapLocal[varKey] || varKey,
          data,
          yAxisIndex: i,
          ...(isColumn ? { type: "column" } : {}),
        });
      });
      const yaxisOptions = keys.map((varKey, i) => {
        const scale = customScales.value[varKey] || {};
        const hasMin = scale.min !== "" && scale.min != null && !isNaN(Number(scale.min));
        const hasMax = scale.max !== "" && scale.max != null && !isNaN(Number(scale.max));
        return {
          opposite: Boolean(i % 2),
          forceNiceScale: !hasMin && !hasMax,
          title: { text: "", style: { color: palette[i] } },
          labels: {
            style: { colors: palette[i] },
            formatter: (val) =>
              val != null && !isNaN(val) ? Number(val).toFixed(1) : "",
          },
          min: hasMin ? Number(scale.min) : undefined,
          max: hasMax ? Number(scale.max) : undefined,
        };
      });

      if (import.meta.env.DEV) {
        console.groupCollapsed(
          `[DEBUG] ChartCard – données injectées (${props.title || ""})`,
        );
        seriesWithAxis.forEach((s) => {
          console.log("Série :", s.name);
          console.table(s.data.slice(0, 35));
        });
        console.groupEnd();
      }
      const annotations = getAnnotations(displayStart, displayEnd);
      const strokeWidths = seriesWithAxis.map((s) =>
        s.type === "column" ? 0 : 2,
      );
      if (!tryAppendOrUpdateSeries(seriesWithAxis, yaxisOptions, annotations)) {
        chartInstance.value.updateOptions(
          {
            yaxis: yaxisOptions,
            annotations: { xaxis: annotations },
            plotOptions: { bar: { columnWidth: COLUMN_WIDTH } },
            stroke: { width: strokeWidths },
          },
          false,
          false,
          false,
        );
        chartInstance.value.updateSeries(seriesWithAxis, false);
      }
      chartInstance.value.zoomX(displayStart.getTime(), displayEnd.getTime());
    } catch (err) {
      console.error("Erreur dans updateChart:", err);
      chartInstance.value.updateOptions({ noData: { text: "Erreur API" } });
    }
  }

  function computeStats() {
    if (useCustomSeries.value) {
      const rawSeries = props.series || [];
      if (!rawSeries.length) {
        statsPeriodTitle.value = "";
        statsList.value = [];
        return;
      }
      const start = new Date(startDate.value);
      const end = new Date(endDate.value);
      statsPeriodTitle.value = `Période : ${formatDateTime(start)} → ${formatDateTime(end)}`;
      const daySet = new Set();
      const perSerieDayMap = {};
      rawSeries.forEach((s) => {
        perSerieDayMap[s.name] = {};
        s.data.forEach((p) => {
          const ts = p.x instanceof Date ? p.x.getTime() : p.x;
          const dateStr = new Date(ts).toISOString().slice(0, 10);
          if (!perSerieDayMap[s.name][dateStr])
            perSerieDayMap[s.name][dateStr] = [];
          perSerieDayMap[s.name][dateStr].push(+p.y);
          daySet.add(dateStr);
        });
      });
      const dayKeys = Array.from(daySet).sort();
      statsList.value = rawSeries.map((s) => {
        const stats = {};
        dayKeys.forEach((date) => {
          const vals = (perSerieDayMap[s.name][date] || []).filter(
            (v) => !isNaN(v),
          );
          if (vals.length) {
            stats[date] = {
              min: Math.min(...vals).toFixed(2),
              max: Math.max(...vals).toFixed(2),
              avg: (vals.reduce((a, b) => a + b, 0) / vals.length).toFixed(2),
            };
          }
        });
        return { label: s.name, stats };
      });
      statsList.value.dayKeys = dayKeys;
    } else {
      if (!currentData.value.length) {
        statsPeriodTitle.value = "";
        statsList.value = [];
        return;
      }
      statsPeriodTitle.value = `Période : ${formatDateTime(currentPeriodStart.value)} → ${formatDateTime(currentPeriodEnd.value)}`;
      const keys = (
        selectedVariables.value.length
          ? selectedVariables.value
          : Object.keys(labelMapLocal)
      ).map((k) => inverseLabelMap[k] ?? k);
      const dayMap = {};
      currentData.value.forEach((d) => {
        const ts =
          d._ts ??
          (d.timestamp instanceof Date
            ? d.timestamp.getTime()
            : Date.parse(d.timestamp));
        if (
          ts < currentPeriodStart.value.getTime() ||
          ts > currentPeriodEnd.value.getTime()
        )
          return;
        const dateStr = new Date(ts).toISOString().slice(0, 10);
        if (!dayMap[dateStr]) dayMap[dateStr] = [];
        dayMap[dateStr].push(d);
      });
      const dayKeys = Object.keys(dayMap).sort();
      const cumulativeTotals = {};
      dayKeys.forEach((date) => {
        const sortedDay = dayMap[date]
          .slice()
          .sort(
            (a, b) =>
              (a._ts ?? Date.parse(a.timestamp)) -
              (b._ts ?? Date.parse(b.timestamp)),
          );
        cumulativeTotals[date] = calculateDailyCumulativeValues(sortedDay);
      });
      statsList.value = keys.map((key) => {
        const { field, devEui } = parseVarKey(key);
        const stats = {};
        dayKeys.forEach((date) => {
          const vals = dayMap[date]
            .filter((d) => !devEui || d.devEui === devEui)
            .map((d) => {
              const ts =
                d._ts ??
                (d.timestamp instanceof Date
                  ? d.timestamp.getTime()
                  : Date.parse(d.timestamp));
              const map = valueCache.get(key);
              if (map && map.has(ts)) return +map.get(ts);
              const comp =
                computedVarDefs[key] ||
                computedVarDefs[`${field}${SEP}${d.devEui}`];
              const raw = comp ? comp.compute(d) : d[field];
              return +raw;
            })
            .filter((v) => !isNaN(v));
          if (vals.length) {
            stats[date] = {
              min: Math.min(...vals).toFixed(2),
              max: Math.max(...vals).toFixed(2),
              avg: (vals.reduce((a, b) => a + b, 0) / vals.length).toFixed(2),
            };
          }
        });
        return { label: labelMapLocal[key] || key, stats };
      });
      const dliStats = {};
      const jouleStats = {};
      dayKeys.forEach((date) => {
        const totals = cumulativeTotals[date] || { dli: 0, joules_cm2: 0 };
        const dliVal = totals.dli.toFixed(2);
        const jouleVal = totals.joules_cm2.toFixed(2);
        dliStats[date] = { min: dliVal, max: dliVal, avg: dliVal };
        jouleStats[date] = { min: jouleVal, max: jouleVal, avg: jouleVal };
      });
      statsList.value.push({ label: "DLI (mol/m²/j)", stats: dliStats });
      statsList.value.push({ label: "Rayonnement (J/cm²)", stats: jouleStats });
      statsList.value.dayKeys = dayKeys;
    }
  }

  return {
    computeStats,
    updateChart,
    updateCustomSeriesChart,
    updateCustomOverviewChart,
  };
}

export default useChartCalculations;
