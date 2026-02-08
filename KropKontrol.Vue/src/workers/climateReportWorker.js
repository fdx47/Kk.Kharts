const LOCAL_TZ = "Europe/Paris";
const DAY_FMT = new Intl.DateTimeFormat("en-CA", {
  timeZone: LOCAL_TZ,
  year: "numeric",
  month: "2-digit",
  day: "2-digit",
});

const MM_PER_PULSE = 0.2;
const HOURS_PER_MS = 1 / 3_600_000;

function computeVpdGm3(temperature, humidity) {
  const svp = 6.112 * Math.exp((17.67 * temperature) / (temperature + 243.5));
  const vp = (svp * humidity) / 100;
  const vpdHectoPascal = svp - vp;
  return (vpdHectoPascal * 216.7) / (273.15 + temperature);
}

function computeDewPoint(temperature, humidity) {
  const a = 17.27;
  const b = 237.7;
  const ratio = humidity / 100;
  if (!Number.isFinite(ratio) || ratio <= 0) return null;
  const alpha = (a * temperature) / (b + temperature) + Math.log(ratio);
  return (b * alpha) / (a - alpha);
}

function sanitizeMeasures(measures) {
  if (!Array.isArray(measures) || measures.length === 0) return [];
  const sanitized = [];
  let lastTs = -Infinity;
  let isSorted = true;
  for (const entry of measures) {
    if (!entry) continue;
    const ts = Number.isFinite(entry.ts)
      ? Number(entry.ts)
      : entry.timestamp instanceof Date
        ? entry.timestamp.getTime()
        : typeof entry.timestamp === "number"
          ? Number(entry.timestamp)
          : entry.timestamp != null
            ? Date.parse(entry.timestamp)
            : NaN;
    if (!Number.isFinite(ts)) continue;
    const temperature = Number(entry.temperature);
    const humidity = Number(entry.humidity);
    if (!Number.isFinite(temperature) || !Number.isFinite(humidity)) continue;
    let water = null;
    if (entry.water != null) {
      const numericWater = Number(entry.water);
      if (Number.isFinite(numericWater)) {
        water = numericWater;
      }
    }
    const dayKey =
      typeof entry.dayKey === "string" && entry.dayKey
        ? entry.dayKey
        : typeof entry._dayKey === "string" && entry._dayKey
          ? entry._dayKey
          : undefined;
    if (ts < lastTs) isSorted = false;
    lastTs = ts;
    sanitized.push({ ts, temperature, humidity, water, dayKey });
  }
  if (!isSorted) {
    sanitized.sort((a, b) => a.ts - b.ts);
  }
  return sanitized;
}

function createDefaultResults(periodCount) {
  const defaults = [];
  for (let i = 0; i < periodCount; i++) {
    defaults.push({
      sumDay: 0,
      countDay: 0,
      sumNight: 0,
      countNight: 0,
      sumAll: 0,
      countAll: 0,
      vpdHours: 0,
      dewHours: 0,
      rainMax: 0,
    });
  }
  return defaults;
}

function updateRainState(state, ts, water, providedDayKey) {
  const dayKey =
    typeof providedDayKey === "string" && providedDayKey
      ? providedDayKey
      : DAY_FMT.format(ts);
  if (state.dayKey !== dayKey) {
    state.dayKey = dayKey;
    state.prevWater = null;
    state.total = 0;
  }

  if (!Number.isFinite(water)) {
    state.prevWater = null;
    return state.total;
  }

  if (state.prevWater == null) {
    state.prevWater = water;
    return state.total;
  }

  const deltaCount = water - state.prevWater;
  state.prevWater = water;
  if (!Number.isFinite(deltaCount)) return state.total;
  const delta = deltaCount * MM_PER_PULSE;
  if (delta >= 0) state.total += delta;
  return state.total;
}

function accumulateDuration(stats, record, durationMs) {
  if (!record || durationMs <= 0) return;
  const deltaHours = durationMs * HOURS_PER_MS;
  if (
    record.vpd != null &&
    record.vpd >= 3 &&
    record.vpd <= 8 &&
    Number.isFinite(record.vpd)
  ) {
    stats.vpdHours += deltaHours;
  }
  if (
    record.dew != null &&
    Number.isFinite(record.dew) &&
    record.dew < record.temperature
  ) {
    stats.dewHours += deltaHours;
  }
}

function aggregate({ measures, periods, includeRain }) {
  const sanitized = sanitizeMeasures(measures);
  const results = createDefaultResults(periods.length);
  if (!sanitized.length || !periods.length) return results.map(summarize);

  const rainState = { dayKey: null, prevWater: null, total: 0 };
  let periodIndex = 0;
  let currentPeriod = periods[periodIndex];
  let lastRecord = null;

  for (const entry of sanitized) {
    const rainValue = includeRain
      ? updateRainState(rainState, entry.ts, entry.water, entry.dayKey)
      : 0;

    while (currentPeriod && entry.ts >= currentPeriod.endMs) {
      if (lastRecord) {
        const stats = results[periodIndex];
        accumulateDuration(stats, lastRecord, currentPeriod.endMs - lastRecord.ts);
        lastRecord = null;
      }
      periodIndex += 1;
      currentPeriod = periods[periodIndex];
    }

    if (!currentPeriod) break;

    if (entry.ts < currentPeriod.startMs) {
      continue;
    }

    const stats = results[periodIndex];

    if (lastRecord) {
      accumulateDuration(stats, lastRecord, entry.ts - lastRecord.ts);
    }

    const vpd = computeVpdGm3(entry.temperature, entry.humidity);
    const dew = computeDewPoint(entry.temperature, entry.humidity);

    stats.sumAll += entry.temperature;
    stats.countAll += 1;
    if (entry.ts < currentPeriod.sunsetMs) {
      stats.sumDay += entry.temperature;
      stats.countDay += 1;
    } else {
      stats.sumNight += entry.temperature;
      stats.countNight += 1;
    }

    if (includeRain && rainValue > stats.rainMax) {
      stats.rainMax = rainValue;
    }

    lastRecord = {
      ts: entry.ts,
      temperature: entry.temperature,
      vpd,
      dew,
    };
  }

  while (currentPeriod && periodIndex < periods.length) {
    if (lastRecord) {
      const stats = results[periodIndex];
      accumulateDuration(stats, lastRecord, currentPeriod.endMs - lastRecord.ts);
      lastRecord = null;
    }
    periodIndex += 1;
    currentPeriod = periods[periodIndex];
  }

  return results.map(summarize);
}

function summarize(stats) {
  return {
    tempAvgDay: stats.countDay ? stats.sumDay / stats.countDay : 0,
    tempAvgNight: stats.countNight ? stats.sumNight / stats.countNight : 0,
    tempAvg24: stats.countAll ? stats.sumAll / stats.countAll : 0,
    hoursVpd: stats.vpdHours,
    hoursDewBelowTemp: stats.dewHours,
    rain: stats.rainMax,
  };
}

self.onmessage = (event) => {
  const { id, measures, periods, includeRain } = event.data || {};
  try {
    const result = aggregate({ measures, periods: periods || [], includeRain });
    self.postMessage({ id, ok: true, result });
  } catch (err) {
    self.postMessage({ id, ok: false, error: err?.message || String(err) });
  }
};
