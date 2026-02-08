// src/services/tableHtmlBuilder.js

import buildDetailedTableHtmlForRange from "./tableHtmlBuilderRange.js";

function computeBoundsFromData(rawDataArray, daysInPeriod) {
  const timestamps = Array.isArray(rawDataArray)
    ? rawDataArray
        .map((pt) => {
          if (!pt?.timestamp) return null;
          const t = new Date(pt.timestamp).getTime();
          return Number.isFinite(t) ? t : null;
        })
        .filter((t) => t != null)
    : [];

  const now = Date.now();
  const endMs =
    timestamps.length > 0 ? Math.max(...timestamps) : now;
  const startMs =
    daysInPeriod && daysInPeriod > 0
      ? endMs - (daysInPeriod - 1) * 86400000
      : timestamps.length > 0
      ? Math.min(...timestamps)
      : endMs - 6 * 86400000;

  return {
    startISO: new Date(startMs).toISOString(),
    endISO: new Date(endMs).toISOString(),
  };
}

export function buildDetailedTableHtml(
  events,
  rawDataArray,
  fieldName = "permittivite",
  daysInPeriod = 7,
) {
  const { startISO, endISO } = computeBoundsFromData(rawDataArray, daysInPeriod);
  return buildDetailedTableHtmlForRange(
    events,
    rawDataArray,
    fieldName,
    startISO,
    endISO,
  );
}

export default buildDetailedTableHtml;
