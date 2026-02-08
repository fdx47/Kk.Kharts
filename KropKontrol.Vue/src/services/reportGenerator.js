// src/services/reportGenerator.js

import { buildDetailedTableHtml } from "./tableHtmlBuilder.js";
import { buildSummaryTableHtml } from "./summaryTableHtmlBuilder.js";

/**
 * Genere le HTML complet du rapport (tables + conteneurs pour le chart).
 * Le script pour ApexCharts n'est pas inclus : il est gere par le composant Vue.
 *
 * @param {Array<Object>} arr            Donnees brutes
 * @param {{ points?: Array, events?: Array }} analysis Resultat de l'analyse algo6
 * @param {string} fieldName             Nom du champ (ex. 'permittivite')
 * @param {number} daysInPeriod          Nombre de jours dans la periode
 * @returns {string}                     HTML final
 */
export function generateReportHtml(
  arr,
  analysis,
  fieldName = "permittivite",
  daysInPeriod = 7,
) {
  if (import.meta.env.DEV) {
    console.log("[reportGenerator] Generation du rapport (HTML statique)...");
  }

  const events = Array.isArray(analysis?.events) ? analysis.events : [];

  const {
    detailedTableHtml,
    consoPostArrosage,
    consoDeNuit,
    consoPreArrosage,
    ecAtSunrise,
    ecDayAvg,
    ecDayDelta,
    dayHeaders,
    anomaliesByDaySlot,
    eventsByDay,
    drainageByDay,
  } = buildDetailedTableHtml(events, arr, fieldName, daysInPeriod);

  const summaryTableString = buildSummaryTableHtml(
    dayHeaders,
    anomaliesByDaySlot,
    consoPostArrosage,
    consoDeNuit,
    consoPreArrosage,
    ecAtSunrise,
    ecDayAvg,
    ecDayDelta,
    daysInPeriod,
    eventsByDay,
    drainageByDay,
  );

  const chartTitle = "Permittivite, Derivee & Consommations";

  const finalHtml = `<!DOCTYPE html>
<html lang="fr">
<head>
  <meta charset="UTF-8">
  <title>${chartTitle}</title>
  <style>
    body { font-family: Arial, sans-serif; margin: 0; padding: 0; background-color: #f4f7f6; }
    .main-container {
      max-width: 1200px;
      margin: 20px auto;
      padding: 20px;
      background-color: #fff;
      box-shadow: 0 0 15px rgba(0,0,0,0.1);
      border-radius: 8px;
    }
    h1.report-title { text-align: center; color: #333; margin-bottom: 30px; }
    .toggle-controls {
      margin-bottom: 20px;
      padding: 10px;
      background-color: #e9ecef;
      border-radius: 5px;
      text-align: center;
    }
    .toggle-button {
      background-color: #5dade2;
      color: white;
      border: none;
      padding: 10px 15px;
      margin: 5px;
      border-radius: 5px;
      cursor: pointer;
      font-size: 0.95em;
      transition: background-color 0.3s ease;
    }
    .toggle-button:hover { background-color: #2e86c1; }
    .hidden-section { display: none; }
    hr.section-divider {
      margin: 30px 0;
      border: 0;
      border-top: 1px solid #dee2e6;
    }
  </style>
</head>
<body>
  <div class="main-container">
    <h1 class="report-title">Rapport d'Irrigation KropKontrol</h1>

    ${summaryTableString}

    <hr class="section-divider">

    <div class="toggle-controls">
      <button id="toggleDetailedTableBtn" class="toggle-button">
        Afficher les Details
      </button>
      <button id="toggleChartBtn" class="toggle-button">
        Afficher le Graphique
      </button>
    </div>

    <div id="detailedTableContainer" class="hidden-section">
      ${detailedTableHtml}
    </div>

    <div id="chartContainer" class="hidden-section">
      <div id="chart"></div>
    </div>
  </div>
</body>
</html>`;

  return finalHtml;
}
