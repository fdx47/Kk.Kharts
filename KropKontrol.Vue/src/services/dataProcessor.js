// src/services/dataProcessor.js

const MS_PER_MINUTE = 60 * 1000;

export const DEFAULT_ALGO6_CONFIG = Object.freeze({
  emaSpan: 1,
  slopeThreshold: 0.00015,
  minDurationMinutes: 2,
  groupMaxGapMinutes: 5,
  pointsToCheckAfter: 4,
  drainingSlopeThreshold: -0.0015,
  eventExpansionPoints: 2,
  drainReferenceLookbackDays: 3,
});

const DEFAULT_TIMESTAMP_FIELD = "timestamp";

function sanitizeNumericValue(raw) {
  if (raw == null) return null;
  if (typeof raw === "number") {
    return Number.isFinite(raw) ? raw : null;
  }
  if (typeof raw === "string") {
    const cleaned = raw.replace(",", ".").trim();
    if (!cleaned) return null;
    const parsed = Number.parseFloat(cleaned);
    return Number.isFinite(parsed) ? parsed : null;
  }
  return null;
}

function preprocessTimeSeries(rawData, fieldName) {
  if (!Array.isArray(rawData) || rawData.length === 0) {
    return {
      series: [],
      meta: {
        originalCount: Array.isArray(rawData) ? rawData.length : 0,
        dedupedCount: 0,
        duplicateCount: 0,
      },
    };
  }

  const aggregate = new Map();
  let duplicateCount = 0;

  for (const entry of rawData) {
    if (!entry) continue;
    const timestampValue = entry[DEFAULT_TIMESTAMP_FIELD];
    if (!timestampValue) continue;
    const resolvedTime = Date.parse(timestampValue);
    if (!Number.isFinite(resolvedTime)) continue;

    const value = sanitizeNumericValue(entry[fieldName]);
    if (value == null) continue;

    let bucket = aggregate.get(resolvedTime);
    if (!bucket) {
      bucket = {
        timeMs: resolvedTime,
        sourceTimestamp: timestampValue,
        sum: 0,
        count: 0,
      };
      aggregate.set(resolvedTime, bucket);
    } else {
      duplicateCount += 1;
    }
    bucket.sum += value;
    bucket.count += 1;
  }

  const series = Array.from(aggregate.values())
    .map((item) => ({
      timeMs: item.timeMs,
      sourceTimestamp: item.sourceTimestamp,
      isoTimestamp: new Date(item.timeMs).toISOString(),
      value: item.count > 0 ? item.sum / item.count : null,
    }))
    .filter((item) => Number.isFinite(item.value))
    .sort((a, b) => a.timeMs - b.timeMs);

  return {
    series,
    meta: {
      originalCount: rawData.length,
      dedupedCount: series.length,
      duplicateCount,
      fieldName,
    },
  };
}

function applyEma(values, span) {
  if (!Array.isArray(values) || values.length === 0 || span <= 1) {
    return Array.isArray(values) ? [...values] : [];
  }

  const alpha = 2 / (span + 1);
  const smoothed = new Array(values.length);
  let emaPrev = values[0];
  smoothed[0] = emaPrev;

  for (let i = 1; i < values.length; i += 1) {
    const current = values[i];
    emaPrev = current * alpha + emaPrev * (1 - alpha);
    smoothed[i] = emaPrev;
  }

  return smoothed;
}

function computeGradient(values, timesMs) {
  const length = values.length;
  if (length === 0) return [];
  if (length === 1) return [0];

  const slopes = new Array(length).fill(0);

  for (let i = 0; i < length; i += 1) {
    if (i === 0) {
      const dt = (timesMs[1] - timesMs[0]) / 1000;
      slopes[i] = dt !== 0 ? (values[1] - values[0]) / dt : 0;
    } else if (i === length - 1) {
      const dt = (timesMs[length - 1] - timesMs[length - 2]) / 1000;
      slopes[i] = dt !== 0 ? (values[length - 1] - values[length - 2]) / dt : 0;
    } else {
      const dt = (timesMs[i + 1] - timesMs[i - 1]) / 1000;
      slopes[i] = dt !== 0 ? (values[i + 1] - values[i - 1]) / dt : 0;
    }

    if (!Number.isFinite(slopes[i])) {
      slopes[i] = 0;
    }
  }

  return slopes;
}

function buildPointSeries(series, smoothed, slopes, config) {
  return series.map((item, index) => ({
    timestamp: item.isoTimestamp,
    sourceTimestamp: item.sourceTimestamp,
    timeMs: item.timeMs,
    value: item.value,
    smoothedValue: smoothed[index],
    slope: slopes[index],
    isWatering: slopes[index] > config.slopeThreshold,
    gainOnEvent: 0,
    drainageLoss: 0,
    cumulativeGains: 0,
    cumulativeDrainage: 0,
    drainagePercentage: 0,
    permDiffCumulative: 0,
    permDiffCumulativePositive: 0,
    cumulativeDeltaPerm: 0,
  }));
}

function createEvent(startIndex, endIndex, points) {
  const safeStart = Math.max(0, startIndex);
  const safeEnd = Math.max(safeStart, endIndex);
  const startPoint = points[safeStart];
  const endPoint = points[safeEnd];
  if (!startPoint || !endPoint) return null;

  const startMs = startPoint.timeMs;
  const endMs = Math.max(endPoint.timeMs, startMs);
  const durationMs = endMs - startMs;

  return {
    startIndex: safeStart,
    endIndex: safeEnd,
    startMs,
    endMs,
    durationMs,
    durationMinutes: durationMs / MS_PER_MINUTE,
  };
}

function detectCandidateEvents(points, config) {
  const events = [];
  let currentStart = null;

  for (let i = 0; i < points.length; i += 1) {
    const point = points[i];
    if (point.isWatering) {
      if (currentStart === null) currentStart = i;
    } else if (currentStart !== null) {
      const event = createEvent(currentStart, i - 1, points);
      if (event && event.durationMinutes >= config.minDurationMinutes) {
        events.push(event);
      }
      currentStart = null;
    }
  }

  if (currentStart !== null) {
    const event = createEvent(currentStart, points.length - 1, points);
    if (event && event.durationMinutes >= config.minDurationMinutes) {
      events.push(event);
    }
  }

  return events;
}

function mergeCloseEvents(events, points, config) {
  if (!events.length) return [];
  const merged = [];
  const gapThresholdMs = config.groupMaxGapMinutes * MS_PER_MINUTE;

  let current = { ...events[0] };

  for (let i = 1; i < events.length; i += 1) {
    const next = events[i];
    const gap = next.startMs - current.endMs;

    if (gap <= gapThresholdMs) {
      current.endIndex = next.endIndex;
      current.endMs = next.endMs;
      current.durationMs = current.endMs - current.startMs;
      current.durationMinutes = current.durationMs / MS_PER_MINUTE;
    } else {
      merged.push(current);
      current = { ...next };
    }
  }

  merged.push(current);

  return merged;
}

function expandAndClassifyEvents(events, points, config) {
  if (!Array.isArray(events) || !Array.isArray(points) || points.length === 0) {
    return [];
  }

  const expansion = Math.max(0, Number(config.eventExpansionPoints) || 0);

  return events
    .map((event) => {
      const startIndex = Number.isInteger(event.startIndex) ? event.startIndex : null;
      const endIndex = Number.isInteger(event.endIndex) ? event.endIndex : null;
      if (startIndex == null || endIndex == null) return null;
      const startPoint = points[startIndex];
      const endPoint = points[endIndex];
      if (!startPoint || !endPoint) return null;

      const expandedStartIndex = Math.max(0, startIndex - expansion);
      const expandedEndIndex = Math.min(points.length - 1, endIndex + expansion);
      const expandedStartPoint = points[expandedStartIndex] ?? startPoint;
      const expandedEndPoint = points[expandedEndIndex] ?? endPoint;
      const expandedDurationMinutes =
        (expandedEndPoint.timeMs - expandedStartPoint.timeMs) / MS_PER_MINUTE;

      const slopes = points
        .slice(expandedStartIndex, expandedEndIndex + 1)
        .map((pt) => pt.slope)
        .filter((slope) => Number.isFinite(slope));
      const minSlope =
        slopes.length > 0 ? slopes.reduce((min, slope) => Math.min(min, slope), slopes[0]) : null;
      const maxSlope =
        slopes.length > 0 ? slopes.reduce((max, slope) => Math.max(max, slope), slopes[0]) : null;
      const variationSlope =
        Number.isFinite(minSlope) && Number.isFinite(maxSlope) ? maxSlope - minSlope : null;

      const permValues = [];
      for (let i = startIndex; i <= expandedEndIndex; i += 1) {
        const candidate = points[i];
        if (candidate && Number.isFinite(candidate.value)) {
          permValues.push(candidate.value);
        }
      }
      const permMaxValue = permValues.length ? Math.max(...permValues) : null;

      return {
        ...event,
        isDraining:
          Number.isFinite(minSlope) && minSlope < config.drainingSlopeThreshold,
        minSlopeExpanded: Number.isFinite(minSlope) ? minSlope : null,
        maxSlopeExpanded: Number.isFinite(maxSlope) ? maxSlope : null,
        variationSlope: Number.isFinite(variationSlope) ? variationSlope : null,
        permMaxValue: Number.isFinite(permMaxValue) ? permMaxValue : null,
        expandedStartIndex,
        expandedEndIndex,
        expandedDurationMinutes: Number.isFinite(expandedDurationMinutes)
          ? Math.max(0, expandedDurationMinutes)
          : 0,
      };
    })
    .filter(Boolean);
}

function getDayKeyFromMs(epochMs) {
  if (!Number.isFinite(epochMs)) return null;
  try {
    return new Date(epochMs).toISOString().slice(0, 10);
  } catch {
    return null;
  }
}

function buildDrainValuesByDay(points, events) {
  const map = new Map();
  if (!Array.isArray(points) || !Array.isArray(events)) return map;

  events.forEach((event) => {
    if (!event?.isDraining) return;
    if (!Number.isInteger(event.startIndex) || !Number.isInteger(event.endIndex)) return;
    const safeStart = Math.max(0, event.startIndex);
    const safeEnd = Math.min(points.length - 1, event.endIndex);
    if (safeEnd < safeStart) return;
    const startPoint = points[safeStart];
    if (!startPoint) return;
    const dayKey = getDayKeyFromMs(startPoint.timeMs);
    if (!dayKey) return;

    const values = [];
    for (let i = safeStart; i <= safeEnd; i += 1) {
      const val = points[i]?.value;
      if (Number.isFinite(val)) values.push(val);
    }
    if (!values.length) return;
    if (!map.has(dayKey)) map.set(dayKey, []);
    map.get(dayKey).push(values);
  });

  return map;
}

function computeMedian(values) {
  if (!Array.isArray(values) || !values.length) return null;
  const sorted = values
    .filter((value) => Number.isFinite(value))
    .sort((a, b) => a - b);
  if (!sorted.length) return null;
  const mid = Math.floor(sorted.length / 2);
  if (sorted.length % 2 === 0) {
    return (sorted[mid - 1] + sorted[mid]) / 2;
  }
  return sorted[mid];
}

function computeDailyDrainStats(drainValuesByDay) {
  const stats = new Map();
  drainValuesByDay.forEach((seriesList, dayKey) => {
    if (!Array.isArray(seriesList) || !seriesList.length) return;
    const combined = [];
    seriesList.forEach((series) => {
      if (Array.isArray(series)) {
        series.forEach((value) => {
          if (Number.isFinite(value)) combined.push(value);
        });
      }
    });
    if (!combined.length) return;
    const sum = combined.reduce((acc, value) => acc + value, 0);
    stats.set(dayKey, {
      mean: sum / combined.length,
      median: computeMedian(combined),
    });
  });
  return stats;
}

function bisectLeft(array, target) {
  let low = 0;
  let high = array.length;
  while (low < high) {
    const mid = Math.floor((low + high) / 2);
    if (array[mid] < target) low = mid + 1;
    else high = mid;
  }
  return low;
}

function annotateEventsWithDrainDiff(events, points, dailyStats, config) {
  if (!Array.isArray(events) || !Array.isArray(points) || !events.length) return;
  const dayKeys = Array.from(dailyStats.keys()).sort();
  const dayMeans = dayKeys.map((key) => {
    const stat = dailyStats.get(key);
    return stat && Number.isFinite(stat.mean) ? stat.mean : null;
  });
  const lookback = Math.max(
    1,
    Number(config.drainReferenceLookbackDays) || 3,
  );

  let currentDayKey = null;
  let cumulative = 0;
  let cumulativePositive = 0;

  events.forEach((event) => {
    const startPoint =
      Number.isInteger(event.startIndex) && event.startIndex < points.length
        ? points[event.startIndex]
        : null;
    const dayKey = startPoint ? getDayKeyFromMs(startPoint.timeMs) : null;

    if (dayKey == null) {
      event.permDiffReference = null;
      event.permDiffLast3 = null;
      event.permDiffCumulative = cumulative;
      event.permDiffCumulativePositive = cumulativePositive;
      return;
    }

    if (dayKey !== currentDayKey) {
      currentDayKey = dayKey;
      cumulative = 0;
      cumulativePositive = 0;
    }

    let reference = null;
    if (dayKeys.length) {
      const idx = bisectLeft(dayKeys, dayKey);
      if (idx > 0) {
        const startIdx = Math.max(0, idx - lookback);
        const prevValues = dayMeans.slice(startIdx, idx).filter((val) => Number.isFinite(val));
        if (prevValues.length) {
          reference =
            prevValues.reduce((acc, value) => acc + value, 0) / prevValues.length;
        }
      }
    }

    let permDiff = null;
    if (Number.isFinite(event.permMaxValue) && Number.isFinite(reference)) {
      permDiff = event.permMaxValue - reference;
      cumulative += permDiff;
      if (permDiff > 0) {
        cumulativePositive += permDiff;
      }
    }

    event.permDiffReference = Number.isFinite(reference) ? reference : null;
    event.permDiffLast3 = Number.isFinite(permDiff) ? permDiff : null;
    event.permDiffCumulative = Number.isFinite(cumulative) ? cumulative : 0;
    event.permDiffCumulativePositive = Number.isFinite(cumulativePositive)
      ? cumulativePositive
      : 0;
  });
}

function assignPermDiffCumulativeToPoints(events, points) {
  if (!Array.isArray(points) || !points.length) return;
  points.forEach((point) => {
    point.permDiffCumulative = null;
    point.permDiffCumulativePositive = null;
    point.cumulativeDeltaPerm = 0;
  });

  events.forEach((event) => {
    const endIndex = Number.isInteger(event.endIndex) ? event.endIndex : null;
    if (endIndex == null || endIndex < 0 || endIndex >= points.length) return;
    const target = points[endIndex];
    if (!target) return;
    if (Number.isFinite(event.permDiffCumulative)) {
      target.permDiffCumulative = event.permDiffCumulative;
    }
    if (Number.isFinite(event.permDiffCumulativePositive)) {
      target.permDiffCumulativePositive = event.permDiffCumulativePositive;
    }
  });

  let currentDayKey = null;
  let cumulative = 0;
  let cumulativePositive = 0;

  points.forEach((point) => {
    const dayKey = getDayKeyFromMs(point.timeMs);
    if (dayKey !== currentDayKey) {
      currentDayKey = dayKey;
      cumulative = 0;
      cumulativePositive = 0;
    }

    if (Number.isFinite(point.permDiffCumulative)) {
      cumulative = point.permDiffCumulative;
    }
    if (Number.isFinite(point.permDiffCumulativePositive)) {
      cumulativePositive = point.permDiffCumulativePositive;
    }

    const safeCumulative = Number.isFinite(cumulative) ? cumulative : 0;
    const safePositive = Number.isFinite(cumulativePositive)
      ? cumulativePositive
      : 0;

    point.permDiffCumulative = safeCumulative;
    point.permDiffCumulativePositive = safePositive;
    point.cumulativeDeltaPerm = safeCumulative;
    point.drainagePercentage = safeCumulative;
  });
}

function attachEventMetrics(events, points, config) {
  events.forEach((event) => {
    const startPoint = points[event.startIndex];
    const endPoint = points[event.endIndex];
    const startValue = startPoint?.value;
    const endValue = endPoint?.value;

    let gain = 0;
    let delta = null;
    if (Number.isFinite(startValue) && Number.isFinite(endValue)) {
      delta = endValue - startValue;
      if (delta > 0) gain = delta;
    }

    event.startValue = Number.isFinite(startValue) ? startValue : null;
    event.endValue = Number.isFinite(endValue) ? endValue : null;
    event.delta = delta;
    event.gain = gain;

    const checkIndex = event.endIndex + config.pointsToCheckAfter;
    event.checkIndex = Number.isInteger(checkIndex) ? checkIndex : null;

    let loss = 0;
    let afterValue = null;

    if (Number.isInteger(checkIndex) && checkIndex < points.length) {
      const checkPoint = points[checkIndex];
      afterValue = checkPoint?.value ?? null;

      if (gain > 0) {
        checkPoint.gainOnEvent += gain;
      }

      if (event.isDraining && Number.isFinite(endValue) && Number.isFinite(afterValue)) {
        const deltaLoss = endValue - afterValue;
        if (deltaLoss > 0) {
          loss = deltaLoss;
          checkPoint.drainageLoss += deltaLoss;
        }
      }
    }

    event.loss = loss;
    event.afterValue = Number.isFinite(afterValue) ? afterValue : null;
  });
}

function finalizeEvents(events, points) {
  return events.map((event, idx) => {
    const startPoint = points[event.startIndex];
    const endPoint = points[event.endIndex];
    const checkPoint =
      Number.isInteger(event.checkIndex) && event.checkIndex < points.length
        ? points[event.checkIndex]
        : null;
    const expandedStartPoint =
      Number.isInteger(event.expandedStartIndex) &&
      event.expandedStartIndex < points.length
        ? points[event.expandedStartIndex]
        : startPoint;
    const expandedEndPoint =
      Number.isInteger(event.expandedEndIndex) &&
      event.expandedEndIndex < points.length
        ? points[event.expandedEndIndex]
        : endPoint;

    const gain = Number.isFinite(event.gain) ? event.gain : 0;
    const loss = Number.isFinite(event.loss) ? event.loss : 0;
    const ratio = gain > 0 ? loss / gain : 0;
    const drainagePercentage = Math.min(
      100,
      Math.max(0, Number.isFinite(ratio) ? ratio * 100 : 0),
    );

    return {
      id: idx + 1,
      startIndex: event.startIndex,
      endIndex: event.endIndex,
      checkIndex: Number.isInteger(event.checkIndex) ? event.checkIndex : null,
      startTime: startPoint?.timestamp ?? null,
      durationMinutes: Number.isFinite(event.durationMinutes) ? event.durationMinutes : 0,
      durationMs: Number.isFinite(event.durationMs) ? event.durationMs : 0,
      isDraining: Boolean(event.isDraining),
      votes: event.votes ?? 0,
      gain,
      loss,
      drainageRatio: Number.isFinite(ratio) ? Math.max(0, ratio) : 0,
      drainagePercentage,
      startValue: event.startValue,
      endValue: event.endValue,
      afterValue: event.afterValue,
      checkTimestamp: checkPoint?.timestamp ?? null,
      time: startPoint?.timestamp ?? null,
      endTime: endPoint?.timestamp ?? null,
      expandedStartTime: expandedStartPoint?.timestamp ?? startPoint?.timestamp ?? null,
      expandedEndTime: expandedEndPoint?.timestamp ?? endPoint?.timestamp ?? null,
      expandedDurationMinutes: Number.isFinite(event.expandedDurationMinutes)
        ? event.expandedDurationMinutes
        : 0,
      minSlopeExpanded: Number.isFinite(event.minSlopeExpanded)
        ? event.minSlopeExpanded
        : null,
      maxSlopeExpanded: Number.isFinite(event.maxSlopeExpanded)
        ? event.maxSlopeExpanded
        : null,
      variationSlope: Number.isFinite(event.variationSlope)
        ? event.variationSlope
        : null,
      permMaxValue: Number.isFinite(event.permMaxValue) ? event.permMaxValue : null,
      permDiffReference: Number.isFinite(event.permDiffReference)
        ? event.permDiffReference
        : null,
      permDiffLast3: Number.isFinite(event.permDiffLast3) ? event.permDiffLast3 : null,
      permDiffCumulative: Number.isFinite(event.permDiffCumulative)
        ? event.permDiffCumulative
        : null,
      permDiffCumulativePositive: Number.isFinite(event.permDiffCumulativePositive)
        ? event.permDiffCumulativePositive
        : null,
    };
  });
}

export function analyzeWateringSeries(rawData, fieldName = "permittivite", options = {}) {
  const config = { ...DEFAULT_ALGO6_CONFIG, ...(options ?? {}) };
  const { series, meta } = preprocessTimeSeries(rawData, fieldName);

  if (!series.length) {
    return {
      points: [],
      events: [],
      config,
      meta,
    };
  }

  const values = series.map((item) => item.value);
  const smoothed = applyEma(values, config.emaSpan);
  const slopes = computeGradient(smoothed, series.map((item) => item.timeMs));
  const points = buildPointSeries(series, smoothed, slopes, config);

  const rawEvents = detectCandidateEvents(points, config);
  const mergedEvents = mergeCloseEvents(rawEvents, points, config);
  const classifiedEvents = expandAndClassifyEvents(mergedEvents, points, config);
  attachEventMetrics(classifiedEvents, points, config);
  const drainValuesByDay = buildDrainValuesByDay(points, classifiedEvents);
  const dailyDrainStats = computeDailyDrainStats(drainValuesByDay);
  annotateEventsWithDrainDiff(classifiedEvents, points, dailyDrainStats, config);
  assignPermDiffCumulativeToPoints(classifiedEvents, points);
  const events = finalizeEvents(classifiedEvents, points);

  return {
    points,
    events,
    config,
    meta,
  };
}

export function findClosestDataPoint(targetEpoch, dataArray, fieldNameToRead) {
  if (!Array.isArray(dataArray) || dataArray.length === 0) {
    return null;
  }

  let closest = null;
  let minDiff = Infinity;

  for (const pt of dataArray) {
    if (!pt?.timestamp || typeof pt[fieldNameToRead] === "undefined") continue;
    const t = new Date(pt.timestamp).getTime();
    if (!Number.isFinite(t)) continue;
    const diff = Math.abs(t - targetEpoch);
    if (diff < minDiff) {
      minDiff = diff;
      closest = pt;
    }
  }
  return closest;
}
