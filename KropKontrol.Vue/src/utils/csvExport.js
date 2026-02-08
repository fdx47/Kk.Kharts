// src/utils/csvExport.js
import { toRaw } from "vue";
import { alignAndInterpolateSeries } from "./interpolateSeries.js";

const DEFAULT_SEPARATOR = ";";
const KEY_SEPARATOR = "|";

function formatTimestamp(ts) {
  const d = ts instanceof Date ? ts : new Date(ts);
  if (Number.isNaN(d.getTime())) return "";
  const pad = (n) => String(n).padStart(2, "0");
  return (
    `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}` +
    ` ${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`
  );
}

function downloadCsv(content, fileName) {
  const blob = new Blob([content], { type: "text/csv;charset=utf-8;" });
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.setAttribute("href", url);
  link.setAttribute("download", fileName);
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
}

export function exportCustomSeriesCsv({
  series,
  fileName = "donnees_graphique_personnalise.csv",
  separator = DEFAULT_SEPARATOR,
} = {}) {
  if (!Array.isArray(series) || series.length === 0) return;

  const rawSeries = alignAndInterpolateSeries(toRaw(series));
  const allTimestamps = new Set();
  rawSeries.forEach((serie) =>
    serie.data.forEach((point) =>
      allTimestamps.add(
        point.x instanceof Date ? point.x.getTime() : Number(point.x),
      ),
    ),
  );
  const sortedTimestamps = Array.from(allTimestamps).sort((a, b) => a - b);
  const headers = ["Date/Heure", ...rawSeries.map((serie) => serie.name)];

  const rows = sortedTimestamps.map((ts) => {
    const formattedDate = formatTimestamp(ts);
    const values = rawSeries.map((serie) => {
      const point = serie.data.find((p) => {
        const px = p.x instanceof Date ? p.x.getTime() : Number(p.x);
        return px === ts;
      });
      return point != null ? point.y : "";
    });
    return [formattedDate, ...values];
  });

  const csvContent = [headers, ...rows]
    .map((row) => row.join(separator))
    .join("\n");
  downloadCsv(csvContent, fileName);
}

export function exportMeasurementsCsv({
  rows,
  variableKeys,
  labelMap,
  computedVarDefs = {},
  parseVarKey,
  fileName = "donnees_chart.csv",
  separator = DEFAULT_SEPARATOR,
  stepMinutes = 0,
} = {}) {
  if (typeof parseVarKey !== "function") {
    console.warn("exportMeasurementsCsv requires a parseVarKey function.");
    return;
  }

  const effectiveKeys =
    Array.isArray(variableKeys) && variableKeys.length
      ? variableKeys
      : Object.keys(labelMap || {});

  if (!effectiveKeys.length) {
    console.warn("exportMeasurementsCsv: no columns selected.");
    return;
  }

  if (!Array.isArray(rows) || rows.length === 0) {
    console.warn("exportMeasurementsCsv: no rows provided.");
    return;
  }

  const stepMs = Number(stepMinutes) > 0 ? Number(stepMinutes) * 60 * 1000 : 0;
  let lastKeptTs = null;

  const sanitizedRows = [];
  rows.forEach((row) => {
    const ts = Date.parse(row.timestamp);
    if (Number.isNaN(ts)) return;

    if (stepMs > 0 && lastKeptTs != null && ts - lastKeptTs < stepMs) {
      return;
    }
    lastKeptTs = ts;
    sanitizedRows.push({ row, ts });
  });

  if (!sanitizedRows.length) {
    console.warn("exportMeasurementsCsv: no rows left after filtering.");
    return;
  }

  sanitizedRows.sort((a, b) => a.ts - b.ts);

  const headers = [
    "Date/Heure",
    ...effectiveKeys.map((key) => (labelMap?.[key] ? labelMap[key] : key)),
  ];

  const rowsContent = sanitizedRows.map(({ row, ts }) => {
    const formattedDate = formatTimestamp(ts);
    const values = effectiveKeys.map((key) => {
      const { field, devEui } = parseVarKey(key);
      if (devEui && row.devEui && row.devEui !== devEui) return "";

      const specificKey =
        row.devEui && field
          ? `${field}${KEY_SEPARATOR}${row.devEui}`
          : undefined;
      const definition =
        computedVarDefs?.[key] ||
        (specificKey ? computedVarDefs?.[specificKey] : undefined);

      const value =
        typeof definition?.compute === "function"
          ? definition.compute(row)
          : row[field];

      return value == null ? "" : value;
    });
    return [formattedDate, ...values];
  });

  const csvContent = [headers, ...rowsContent]
    .map((row) => row.join(separator))
    .join("\n");
  downloadCsv(csvContent, fileName);
}

export default {
  exportCustomSeriesCsv,
  exportMeasurementsCsv,
};
