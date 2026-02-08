// src/services/summaryTableHtmlBuilder.js

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

function formatTimeFromISO(isoTimestampString) {
  if (!isoTimestampString) return "N/A";
  const dateObj = parseIsoFlexible(isoTimestampString);
  if (!dateObj) return "N/A";
  try {
    return dateObj.toLocaleTimeString("fr-FR", {
      hour: "2-digit",
      minute: "2-digit",
      timeZone: "Europe/Paris",
    });
  } catch {
    return "N/A";
  }
}

const asCountCell = (value) =>
  `<span class="cnt-badge" title="Nombre d'arrosages">${value}</span>`;

const asTimeCell = (value) =>
  value && value !== "N/A"
    ? `<span class="time-pill" title="Heure locale">${value}</span>`
    : `<span class="empty">-</span>`;

const asConsChip = (title, raw) => {
  const val = Number.parseFloat(raw);
  if (Number.isNaN(val)) return `<span class="empty">-</span>`;
  const cls = val > 0 ? "pos" : val < 0 ? "neg" : "";
  return `<span class="cons-chip ${cls}" title="${title}">${val.toFixed(2)}</span>`;
};

const asPercentChip = (title, raw) => {
  const val = Number.parseFloat(raw);
  if (Number.isNaN(val)) return `<span class="empty">-</span>`;
  const clipped = Math.min(100, Math.max(0, val));
  const cls = clipped >= 40 ? "warn" : clipped >= 20 ? "mid" : "ok";
  return `<span class="percent-chip ${cls}" title="${title}">${clipped.toFixed(1)}%</span>`;
};

const asEcChip = (value, previous, label) => {
  const current = Number.parseFloat(value);
  if (Number.isNaN(current)) return `<span class="empty">-</span>`;
  const prev = Number.parseFloat(previous);
  let cls = "neutral";
  if (!Number.isNaN(prev)) {
    if (current < prev) cls = "good";
    else if (current > prev) cls = "bad";
  }
  return `<span class="ec-chip ${cls}" title="${label}">${current.toFixed(2)}</span>`;
};

export function buildSummaryTableHtml(
  dayHeaders,
  eventsByDaySlot,
  consoPostArrosage,
  consoDeNuit,
  consoPreArrosage,
  ecAtSunrise = [],
  ecDayAvg = [],
  ecDayDelta = [],
  daysInPeriod,
  enhancedEventsByDay = null,
  drainageByDay = null,
  isRoot = false,
) {
  if (
    !Array.isArray(dayHeaders) ||
    !Array.isArray(eventsByDaySlot) ||
    !Array.isArray(consoPostArrosage) ||
    !Array.isArray(consoDeNuit) ||
    !Array.isArray(consoPreArrosage) ||
    daysInPeriod <= 0
  ) {
    return `
      <div style="
        text-align:center;
        margin-top:20px;
        padding:15px;
        font-family: Arial, sans-serif;
        color: #555;
        background-color:#f0f0f0;
        border-radius:5px;
      ">
        Donnees insuffisantes pour le tableau recapitulatif.
      </div>`;
  }

  const effectiveEvents =
    Array.isArray(enhancedEventsByDay) &&
    enhancedEventsByDay.length === daysInPeriod
      ? enhancedEventsByDay
      : eventsByDaySlot;

  const getDrainageStats = (idx) => {
    if (
      Array.isArray(drainageByDay) &&
      drainageByDay[idx] &&
      typeof drainageByDay[idx] === "object"
    ) {
      return drainageByDay[idx];
    }
    const events = effectiveEvents[idx] || [];
    const gainTotal = events.reduce(
      (sum, ev) => sum + (Number.isFinite(ev?.gain) ? ev.gain : 0),
      0,
    );
    const lossTotal = events.reduce(
      (sum, ev) => sum + (Number.isFinite(ev?.loss) ? ev.loss : 0),
      0,
    );
    const percent =
      gainTotal > 0 ? (lossTotal / gainTotal) * 100 : 0;
    return { gainTotal, lossTotal, percent };
  };

  let html = `
    <style>
      .summary-wrap { margin: 24px 0; border-radius: 16px; border: 1px solid #e2e9dd; background: #ffffff; }
      .summary-headbar { padding: 12px 18px; border-bottom: 1px solid #e2e9dd; font-size: 1.05rem; font-weight: 600; color: #2c4a1d; background: #f2f8ef; }
      .summary-scroller { overflow-x: auto; }
      .summary-table { width: 100%; border-collapse: collapse; font-family: 'Inter', system-ui, Arial, sans-serif; font-size: 0.95rem; }
      .summary-table thead th { background: #f7fbf5; padding: 10px; text-align: center; font-weight: 600; color: #3c5a2b; border-bottom: 1px solid #e2e9dd; }
      .summary-table tbody td { padding: 10px 12px; border-bottom: 1px solid #eef3ea; text-align: center; }
      .summary-table tbody td.metric-label { text-align: left; font-weight: 600; color: #2d3b27; background: #f9fcf7; position: sticky; left: 0; z-index: 1; }
      .summary-table tbody tr:nth-child(even) td { background: #fcfdfb; }
      .summary-table tbody tr:hover td { background: #f3f8ef; }
      .summary-table .cnt-badge { display: inline-block; min-width: 34px; padding: 4px 10px; border-radius: 999px; background: #edf6ea; color: #2d5a1d; font-weight: 600; }
      .summary-table .time-pill { display: inline-block; padding: 4px 8px; border-radius: 6px; background: #eaf3fd; color: #21548d; font-variant-numeric: tabular-nums; }
      .summary-table .cons-chip { display: inline-block; padding: 4px 8px; border-radius: 8px; background: #f5f7f9; color: #3a4753; font-weight: 600; }
      .summary-table .cons-chip.pos { background: #ffecec; color: #c12c2c; }
      .summary-table .cons-chip.neg { background: #e9f7ef; color: #1b7a3a; }
      .summary-table .percent-chip { display: inline-block; padding: 4px 10px; border-radius: 999px; font-weight: 600; }
      .summary-table .percent-chip.ok { background: #e6f7f1; color: #1f7a56; }
      .summary-table .percent-chip.mid { background: #fff4db; color: #c07008; }
      .summary-table .percent-chip.warn { background: #fdeaea; color: #b62020; }
      .summary-table .ec-chip { display: inline-block; padding: 4px 8px; border-radius: 8px; background: #f2f4f6; }
      .summary-table .ec-chip.good { background: #e6f7f1; color: #1f7a56; }
      .summary-table .ec-chip.bad { background: #fdeaea; color: #b62020; }
      .summary-table .ec-chip.neutral { background: #f3f5f7; color: #3a4753; }
      .summary-table .empty { color: #9aa1a8; }
      .summary-table td.hl { background: #fff7d1 !important; box-shadow: inset 0 0 0 2px #ffd666; }
    </style>

    <div class="summary-wrap">
      <div class="summary-headbar">Resume des irrigations detectees</div>
      <div class="summary-scroller">
        <table class="summary-table" aria-label="Resume des irrigations">
          <thead>
            <tr>
              <th>Indicateur</th>`;

  const enterFn = `kkSumHover(this)`;
  const leaveFn = `kkSumLeave(this)`;

  dayHeaders.forEach((headerDate, idx) => {
    html += `<th data-col="${idx}" onmouseenter='${enterFn}' onmouseleave='${leaveFn}'>${headerDate}</th>`;
  });

  html += `
            </tr>
          </thead>
          <tbody>`;

  const firstDrainLabels = effectiveEvents.map((slot) => {
    const index = Array.isArray(slot)
      ? slot.findIndex((ev) => ev?.isDraining)
      : -1;
    if (index === -1) return null;
    return index + 1;
  });

  const rows = [
    { label: "Nombre d'arrosages", key: "count" },
    { label: "Nombre d'arrosages avec drainage", key: "drainingCount", rootOnly: true },
    { label: "% drainage estimé (Cumul)", key: "drainagePercent", rootOnly: true },
    { label: "Premier arrosage drainant", key: "firstDrainEvent", rootOnly: true },
    { label: "Heure du premier arrosage", key: "firstTime" },
    { label: "Heure du dernier arrosage", key: "lastTime" },
    { label: "Consommation d'eau de nuit", key: "night", className: "row-night" },
    { label: "Consommation d'eau pré arrosage", key: "pre", className: "row-pre" },
    { label: "Consommation d'eau post-arrosage", key: "post", className: "row-post" },
    { label: "EC moyenne du jour", key: "ecAvg" },
  ];

  const visibleRows = rows.filter((metric) => !metric.rootOnly || isRoot);

  visibleRows.forEach((metric) => {
    const rowClass = metric.className ? ` class="${metric.className}"` : "";
    const hoverAttr =
      metric.key === "pre"
        ? ` onmousemove="kkSumHoverCell(event)" onmouseleave="kkSumLeaveCell(event)"`
        : "";

    html += `<tr${rowClass}${hoverAttr}><td class="metric-label">${metric.label}</td>`;

    for (let i = 0; i < daysInPeriod; i += 1) {
      const dayEvents = effectiveEvents[i] || [];
      const drainageStats = getDrainageStats(i);
      let raw = "N/A";
      let cellContent = "";

      switch (metric.key) {
        case "count":
          raw = dayEvents.length;
          cellContent = asCountCell(raw);
          break;
        case "drainingCount":
          raw = dayEvents.filter((ev) => ev?.isDraining).length;
          cellContent = asCountCell(raw);
          break;
        case "firstTime":
          raw = dayEvents.length > 0 ? formatTimeFromISO(dayEvents[0]?.time || dayEvents[0]?.startTime) : "N/A";
          cellContent = asTimeCell(raw);
          break;
        case "lastTime":
          raw =
            dayEvents.length > 0
              ? formatTimeFromISO(dayEvents[dayEvents.length - 1]?.time || dayEvents[dayEvents.length - 1]?.startTime)
              : "N/A";
          cellContent = asTimeCell(raw);
          break;
        case "post":
          raw = consoPostArrosage[i];
          cellContent = asConsChip("Consommation post arrosage", raw);
          break;
        case "night":
          raw = consoDeNuit[i];
          cellContent = asConsChip("Consommation de nuit", raw);
          break;
        case "pre":
          raw = consoPreArrosage[i];
          cellContent = asConsChip("Consommation pré arrosage", raw);
          break;
        case "drainagePercent":
          raw = drainageStats?.percent ?? null;
          cellContent = Number.isFinite(raw)
            ? asPercentChip("Pourcentage drainage estimé", raw)
            : `<span class="empty">-</span>`;
          break;
        case "firstDrainEvent":
          raw = firstDrainLabels[i];
          if (Number.isFinite(raw)) {
            const ordinal =
              raw === 1 ? "1er" : `${raw}ième`;
            cellContent = `<span class="time-pill" title="Premier événement drainant">${ordinal} arrosage</span>`;
          } else {
            cellContent = `<span class="empty">Pas de drainage</span>`;
          }
          break;
        case "ecAvg":
          cellContent = asEcChip(ecDayAvg[i], ecDayAvg[i - 1], "EC moyenne du jour");
          break;
        default:
          cellContent = `<span class="empty">-</span>`;
      }

      const shouldHighlight =
        metric.key === "count" ||
        metric.key === "drainingCount" ||
        metric.key === "drainagePercent" ||
        metric.key === "firstDrainEvent" ||
        metric.key === "firstTime" ||
        metric.key === "lastTime" ||
        metric.key === "post" ||
        metric.key === "night";

      const eventAttrs = shouldHighlight
        ? ` data-col="${i}" onmouseenter='${enterFn}' onmouseleave='${leaveFn}'`
        : "";

      html += `<td${eventAttrs}>${cellContent}</td>`;
    }

    html += `</tr>`;
  });

  html += `
          </tbody>
        </table>
      </div>
    </div>

    <div class="summary-info-banner" role="note">
      <p>
        Le détail des arrosages est basé sur une analyse mathématique des courbes d'humidité.
        Si la sonde est déplacée et/ou touchée, ou si les arrosages sont très serrés ou très longs,
        les données peuvent être erronées. L'heure et le nombre réel d'arrosages peuvent différer de la réalité ;
        ces informations sont données à titre indicatif.
      </p>
    </div>
  `;

  return html;
}
