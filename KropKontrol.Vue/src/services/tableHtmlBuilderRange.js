// src/services/tableHtmlBuilderRange.js

import { findClosestDataPoint } from "./dataProcessor.js";
import SunCalc from "suncalc";
import { DateTime } from "luxon";
import { LS_COORDS, LS_TIMEZONE } from "../config/storageKeys.js";

const MS_PER_DAY = 86400000;

function parseIsoFlexible(value) {
  if (!value) return null;
  try {
    const decoded = decodeURIComponent(value);
    const hasOffset = /[+-]\d{2}:\d{2}$/.test(decoded);
    const hasZ = decoded.endsWith("Z");
    const normalized = hasOffset || hasZ ? decoded : `${decoded}Z`;
    const date = new Date(normalized);
    return Number.isNaN(date.valueOf()) ? null : date;
  } catch {
    return null;
  }
}

function formatLocalTime(dateObj, zone) {
  if (!(dateObj instanceof Date) || Number.isNaN(dateObj.valueOf())) {
    return "--";
  }
  try {
    return dateObj.toLocaleTimeString("fr-FR", {
      hour: "2-digit",
      minute: "2-digit",
      timeZone: zone,
    });
  } catch {
    return "--";
  }
}

function formatDeltaLabel(value) {
  if (typeof value !== "number" || Number.isNaN(value)) {
    return "N/A";
  }
  const abs = Math.abs(value);
  const digits = abs >= 1 || abs === 0 ? 0 : 2;
  const sign = value > 0 ? "+" : "";
  return `${sign}${value.toFixed(digits)}%VWC`;
}

function formatVwcValue(value) {
  if (typeof value !== "number" || Number.isNaN(value)) {
    return "N/A";
  }
  return `${value.toFixed(0)}%`;
}

function formatGainLoss(value) {
  if (typeof value !== "number" || Number.isNaN(value)) {
    return null;
  }
  return `${value >= 0 ? "+" : ""}${value.toFixed(2)}`;
}

function formatPercent(value) {
  if (typeof value !== "number" || Number.isNaN(value)) {
    return null;
  }
  const clipped = Math.min(100, Math.max(0, value));
  return `${clipped.toFixed(1)}%`;
}

function formatPercentageDelta(gain, loss) {
  const gainVal = typeof gain === "number" && Number.isFinite(gain) ? gain : 0;
  const lossVal = typeof loss === "number" && Number.isFinite(loss) ? loss : 0;
  const net = gainVal - lossVal;
  return `${net >= 0 ? "+" : ""}${net.toFixed(2)}`;
}

function collectVwcStats(startMs, endMs, rawDataArray, fieldName) {
  if (
    !Array.isArray(rawDataArray) ||
    rawDataArray.length === 0 ||
    !Number.isFinite(startMs) ||
    !Number.isFinite(endMs) ||
    endMs <= startMs
  ) {
    return { delta: null, startValue: null, endValue: null };
  }
  const startPoint = findClosestDataPoint(startMs, rawDataArray, fieldName);
  const endPoint = findClosestDataPoint(endMs, rawDataArray, fieldName);
  if (!startPoint || !endPoint) {
    return { delta: null, startValue: null, endValue: null };
  }
  const startValue = Number.parseFloat(startPoint[fieldName]);
  const endValue = Number.parseFloat(endPoint[fieldName]);
  if (!Number.isFinite(startValue) || !Number.isFinite(endValue)) {
    return { delta: null, startValue: null, endValue: null };
  }
  return { delta: endValue - startValue, startValue, endValue };
}

export function buildDetailedTableHtmlForRange(
  events,
  rawDataArray,
  fieldName = "permittivite",
  startISO,
  endISO,
) {
  let coords = { lat: 48.8566, lon: 2.3522 };
  let zone = "Europe/Paris";
  try {
    const storedCoords = localStorage.getItem(LS_COORDS);
    if (storedCoords) {
      const parsed = JSON.parse(storedCoords);
      if (parsed && parsed.lat != null && parsed.lon != null) coords = parsed;
    }
    const storedZone = localStorage.getItem(LS_TIMEZONE);
    if (storedZone) zone = storedZone;
  } catch {
    // ignore storage errors
  }

  const startD = parseIsoFlexible(startISO) || new Date();
  const endD = parseIsoFlexible(endISO) || new Date();

  const startLocal = new Date(startD.toLocaleString("en-US", { timeZone: zone }));
  const endLocal = new Date(endD.toLocaleString("en-US", { timeZone: zone }));
  startLocal.setHours(0, 0, 0, 0);
  endLocal.setHours(23, 59, 59, 999);

  const diffDays = Math.floor((endLocal.getTime() - startLocal.getTime()) / MS_PER_DAY);
  let daysInPeriod = Math.max(1, diffDays + 1);

  const startDayZ = DateTime.fromJSDate(startLocal, { zone }).startOf("day");
  let dayHeaders = Array.from({ length: daysInPeriod }, (_, i) =>
    startDayZ.plus({ days: i }).toFormat("dd/LL"),
  );

  const nowDayZ = DateTime.now().setZone(zone).startOf("day");
  const requestedEndDayZ = DateTime.fromJSDate(endLocal, { zone }).startOf("day");
  let cappedEndDayZ = requestedEndDayZ;
  if (cappedEndDayZ.toMillis() > nowDayZ.toMillis()) {
    cappedEndDayZ = nowDayZ;
  }
  let validDays = Math.floor(cappedEndDayZ.diff(startDayZ, "days").days) + 1;
  if (!Number.isFinite(validDays) || validDays <= 0) {
    validDays = 1;
  }
  if (validDays < daysInPeriod) {
    dayHeaders = dayHeaders.slice(0, validDays);
    daysInPeriod = validDays;
  }

  const irrigationDetailsPerDay = Array.from({ length: daysInPeriod }, () => []);
  const eventsByDaySlot = Array.from({ length: daysInPeriod }, () => []);
  let maxIrrigationsPerDay = 0;

  if (Array.isArray(events)) {
    events.forEach((event) => {
      const startDate = parseIsoFlexible(event?.time || event?.startTime);
      if (!startDate) return;
      const startZ = DateTime.fromJSDate(startDate, { zone });
      const eventDayZ = startZ.startOf("day");
      const idx = Math.floor(eventDayZ.diff(startDayZ, "days").days + 1e-6);
      if (idx < 0 || idx >= daysInPeriod) return;

      const endDate = parseIsoFlexible(event?.endTime) || startDate;
      const startMs = startDate.getTime();
      const endMs = Math.max(endDate.getTime(), startMs);

      let startValue = Number.isFinite(event?.startValue) ? event.startValue : null;
      let endValue = Number.isFinite(event?.endValue) ? event.endValue : null;
      let deltaValue =
        startValue != null && endValue != null ? endValue - startValue : null;

      if (startValue == null || endValue == null) {
        const stats = collectVwcStats(startMs, endMs, rawDataArray, fieldName);
        if (startValue == null) startValue = stats.startValue;
        if (endValue == null) endValue = stats.endValue;
        if (deltaValue == null) deltaValue = stats.delta;
      }

      irrigationDetailsPerDay[idx].push({
        startMs,
        startLabel: formatLocalTime(startDate, zone),
        endLabel: formatLocalTime(endDate, zone),
        deltaLabel: formatDeltaLabel(deltaValue),
        startValue,
        endValue,
        drainingLabel: event?.isDraining ? "Oui" : "Non",
        gainValue: Number.isFinite(event?.gain) ? event.gain : null,
        lossValue: Number.isFinite(event?.loss) ? event.loss : null,
        drainagePercent: Number.isFinite(event?.drainagePercentage)
          ? Math.min(100, Math.max(0, event.drainagePercentage))
          : null,
      });

      eventsByDaySlot[idx].push(event);
      if (irrigationDetailsPerDay[idx].length > maxIrrigationsPerDay) {
        maxIrrigationsPerDay = irrigationDetailsPerDay[idx].length;
      }
    });

    irrigationDetailsPerDay.forEach((details) =>
      details.sort((a, b) => (a.startMs ?? 0) - (b.startMs ?? 0)),
    );
    eventsByDaySlot.forEach((slot) =>
      slot.sort((a, b) => {
        const da = parseIsoFlexible(a?.time || a?.startTime);
        const db = parseIsoFlexible(b?.time || b?.startTime);
        return (da?.getTime?.() ?? 0) - (db?.getTime?.() ?? 0);
      }),
    );
  }

  const sunriseTimes = Array(daysInPeriod).fill(null);
  const sunsetTimes = Array(daysInPeriod).fill(null);
  const firstDayZ = startDayZ;
  for (let i = 0; i < daysInPeriod; i += 1) {
    const dayZ = firstDayZ.plus({ days: i });
    const safeDate = dayZ.set({ hour: 12, minute: 0, second: 0, millisecond: 0 }).toJSDate();
    const t = SunCalc.getTimes(safeDate, coords.lat, coords.lon);
    sunriseTimes[i] = t.sunrise ? new Date(t.sunrise.getTime()) : null;
    sunsetTimes[i] = t.sunset ? new Date(t.sunset.getTime()) : null;
  }

  const consoPostArrosage = Array(daysInPeriod).fill("N/A");
  const consoDeNuit = Array(daysInPeriod).fill("N/A");
  const consoPreArrosage = Array(daysInPeriod).fill("N/A");
  const ecAtSunrise = Array(daysInPeriod).fill("N/A");
  const ecDayAvg = Array(daysInPeriod).fill("N/A");
  const ecDayDelta = Array(daysInPeriod).fill("N/A");

  const ecField =
    typeof fieldName === "string" && fieldName.endsWith("VWC")
      ? fieldName.replace(/VWC$/, "ECp")
      : null;

  for (let i = 0; i < daysInPeriod; i += 1) {
    const slot = eventsByDaySlot[i];

    if (slot.length > 0 && sunsetTimes[i]) {
      const last = slot[slot.length - 1];
      const dLast = parseIsoFlexible(last?.endTime || last?.time);
      if (dLast) {
        const tStart = dLast.getTime() + 30 * 60 * 1000;
        const tEnd = sunsetTimes[i].getTime();
        if (tStart < tEnd) {
          const pA = findClosestDataPoint(tStart, rawDataArray, fieldName);
          const pB = findClosestDataPoint(tEnd, rawDataArray, fieldName);
          if (pA && pB) {
            const vA = Number.parseFloat(pA[fieldName]);
            const vB = Number.parseFloat(pB[fieldName]);
            if (!Number.isNaN(vA) && !Number.isNaN(vB)) {
              consoPostArrosage[i] = (vB - vA).toFixed(2);
            }
          }
        }
      }
    }

    if (sunsetTimes[i] && sunriseTimes[i + 1]) {
      const tStart = sunsetTimes[i].getTime();
      const tEnd = sunriseTimes[i + 1].getTime();
      if (tEnd > tStart) {
        const pA = findClosestDataPoint(tStart, rawDataArray, fieldName);
        const pB = findClosestDataPoint(tEnd, rawDataArray, fieldName);
        if (pA && pB) {
          const vA = Number.parseFloat(pA[fieldName]);
          const vB = Number.parseFloat(pB[fieldName]);
          if (!Number.isNaN(vA) && !Number.isNaN(vB) && i + 1 < daysInPeriod) {
            consoDeNuit[i + 1] = (vB - vA).toFixed(2);
          }
        }
      }
    }

    if (slot.length > 0 && sunriseTimes[i]) {
      const first = slot[0];
      const dFirst = parseIsoFlexible(first?.time || first?.startTime);
      if (dFirst) {
        const tEnd = dFirst.getTime() - 5 * 60 * 1000;
        const tStart = sunriseTimes[i].getTime();
        if (tEnd > tStart) {
          const pA = findClosestDataPoint(tStart, rawDataArray, fieldName);
          const pB = findClosestDataPoint(tEnd, rawDataArray, fieldName);
          if (pA && pB) {
            const vA = Number.parseFloat(pA[fieldName]);
            const vB = Number.parseFloat(pB[fieldName]);
            if (!Number.isNaN(vA) && !Number.isNaN(vB)) {
              consoPreArrosage[i] = (vB - vA).toFixed(2);
            }
          }
        }
      }
    }

    if (ecField && sunriseTimes[i]) {
      const p = findClosestDataPoint(sunriseTimes[i].getTime(), rawDataArray, ecField);
      if (p && p[ecField] != null && !Number.isNaN(Number.parseFloat(p[ecField]))) {
        ecAtSunrise[i] = Number.parseFloat(p[ecField]).toFixed(2);
      }
    }

    if (ecField) {
      const dayStart = new Date(startLocal.getTime() + i * MS_PER_DAY);
      const dayEnd = new Date(dayStart.getTime() + MS_PER_DAY - 1);
      const tStart = (sunriseTimes[i] || dayStart).getTime();
      const tEnd = (sunsetTimes[i] || dayEnd).getTime();
      if (tEnd > tStart) {
        const vals = [];
        for (const pt of rawDataArray) {
          if (!pt?.timestamp) continue;
          const t = new Date(pt.timestamp).getTime();
          if (t >= tStart && t <= tEnd) {
            const v = Number.parseFloat(pt[ecField]);
            if (!Number.isNaN(v)) vals.push(v);
          }
        }
        if (vals.length) {
          const sum = vals.reduce((acc, v) => acc + v, 0);
          const avg = sum / vals.length;
          const vmin = Math.min(...vals);
          const vmax = Math.max(...vals);
          ecDayAvg[i] = avg.toFixed(2);
          ecDayDelta[i] = (vmax - vmin).toFixed(2);
        }
      }
    }
  }

  const drainageByDay = eventsByDaySlot.map((slot) => {
    const gainTotal = slot.reduce(
      (sum, ev) => sum + (Number.isFinite(ev?.gain) ? ev.gain : 0),
      0,
    );
    const lossTotal = slot.reduce(
      (sum, ev) => sum + (Number.isFinite(ev?.loss) ? ev.loss : 0),
      0,
    );
    const percent = gainTotal > 0 ? (lossTotal / gainTotal) * 100 : 0;
    return {
      gainTotal,
      lossTotal,
      percent,
    };
  });

  const headerCells = Array.from(
    { length: maxIrrigationsPerDay },
    (_, i) => `<th>Arrosage #${i + 1}</th>`,
  ).join("");

  const bodyRows = [];
  if (maxIrrigationsPerDay > 0) {
    for (let i = 0; i < daysInPeriod; i += 1) {
      const details = irrigationDetailsPerDay[i];
      const cells = [`<td>${dayHeaders[i]}</td>`];
      for (let j = 0; j < maxIrrigationsPerDay; j += 1) {
        const detail = details[j];
        if (detail) {
          const netDelta = formatPercentageDelta(detail.gainValue, detail.lossValue);
          const drainageFlag = detail.drainingLabel ?? "Non";
          const percentLabel = formatPercent(detail.drainagePercent) ?? "0.0%";
          const lines = [
            `Delta VWC: ${netDelta}`,
            `Arrosage drainant: ${drainageFlag}`,
            "",
            `% Drain: ${percentLabel}`,
          ];
          cells.push(`<td>${lines.join("<br />")}</td>`);
        } else {
          cells.push("<td>-</td>");
        }
      }
      bodyRows.push(`<tr>${cells.join("")}</tr>`);
    }
  } else if (Array.isArray(events) && events.length) {
    bodyRows.push(
      `<tr><td colspan="${maxIrrigationsPerDay + 1}">Aucune irrigation detaillee.</td></tr>`,
    );
  } else {
    bodyRows.push(
      `<tr><td colspan="${maxIrrigationsPerDay + 1}">Aucune irrigation detectee.</td></tr>`,
    );
  }

  const detailedTableHtml = [
    '<table aria-label="Detail des irrigations detectees">',
    "  <thead>",
    "    <tr>",
    "      <th>Jour</th>",
    `      ${headerCells}`,
    "    </tr>",
    "  </thead>",
    "  <tbody>",
    ...bodyRows.map((row) => `    ${row}`),
    "  </tbody>",
    "</table>",
  ].join("\n");

  return {
    detailedTableHtml,
    maxIrrigationsPerDay,
    consoPostArrosage,
    consoDeNuit,
    consoPreArrosage,
    ecAtSunrise,
    ecDayAvg,
    ecDayDelta,
    dayHeaders,
    anomaliesByDaySlot: eventsByDaySlot,
    eventsByDay: eventsByDaySlot,
    drainageByDay,
  };
}

export default buildDetailedTableHtmlForRange;
