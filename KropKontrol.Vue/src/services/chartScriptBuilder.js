// src/services/chartScriptBuilder.js

/**
 * Construit un script JavaScript (à `eval`) pour rendre un graphique ApexCharts
 * basé sur les données brutes et leurs pentes.
 *
 * @param {Array}  arr       Tableau d’objets { timestamp, [fieldName]: valeur }
 * @param {Array}  slopes    Tableau d’objets { time, slope }
 * @param {string} fieldName Nom du champ à tracer (par défaut "permittivite")
 * @returns {string} Le code JS à `eval` pour initialiser/rendre le chart
 */
export function buildChartScript(arr, slopes, fieldName = "permittivite") {
  const displayName = "Permittivité";

  const safeArr = Array.isArray(arr) ? arr : [];
  const safeSlopes = Array.isArray(slopes) ? slopes : [];

  if (import.meta.env.DEV) {
    console.log(
      `buildChartScript: safeArr length = ${safeArr.length}, safeSlopes length = ${safeSlopes.length}`,
    );
  }

  // Série principale
  const seriesPerm = safeArr
    .map((pt) => {
      if (
        !pt ||
        typeof pt.timestamp !== "string" ||
        typeof pt[fieldName] === "undefined"
      )
        return null;
      const value = parseFloat(pt[fieldName]);
      if (isNaN(value)) {
        console.warn(
          `buildChartScript (seriesPerm): Valeur NaN pour ${fieldName}, timestamp: ${pt.timestamp}`,
        );
        return null;
      }
      return `[new Date('${pt.timestamp}Z').getTime(), ${value.toFixed(2)}]`;
    })
    .filter(Boolean)
    .join(",\n    ");

  // Série des pentes
  const seriesSlope = safeSlopes
    .map((s) => {
      if (
        !s ||
        typeof s.time !== "string" ||
        typeof s.slope !== "number" ||
        isNaN(s.slope)
      ) {
        return null;
      }
      return `[new Date('${s.time}Z').getTime(), ${s.slope.toFixed(4)}]`;
    })
    .filter(Boolean)
    .join(",\n    ");

  if (import.meta.env.DEV) {
    console.log("--- Contenu de seriesPerm (pour le script graphique) ---");
    console.log(seriesPerm);
    console.log("--- Fin de seriesPerm ---");
    console.log("--- Contenu de seriesSlope (pour le script graphique) ---");
    console.log(seriesSlope);
    console.log("--- Fin de seriesSlope ---");
  }

  // Le script JS complet à évaluer
  const chartOptionsScript = `
    if (import.meta.env.DEV) {
      console.log("Attempting to render chart with DYNAMIC seriesPerm and DYNAMIC seriesSlope...");
    }
    try {
      const options = {
        chart: { 
          height: 550,
          type: 'line',
          zoom: { enabled: true, type: 'x', autoScaleYaxis: true },
          toolbar: { show: true, autoSelected: 'zoom' }
        },
        series: [
          { name: '${displayName}', data: [${seriesPerm}] },
          { name: 'Dérivée (slope)', data: [${seriesSlope}], yAxisIndex: 1 }
        ],
        xaxis: { 
          type: 'datetime',
          labels: { datetimeUTC: false }
        },
        yaxis: [
          { title: { text: '${displayName}' } },
          { opposite: true, title: { text: 'Dérivée (slope)' } }
        ],
        tooltip: {
          shared: true,
          intersect: false,
          x: { format: 'dd MMM HH:mm:ss' },
          y: {
            formatter: function(value, { seriesIndex, w }) {
              const seriesName = w.config.series[seriesIndex].name;
              if (typeof value === 'number') {
                return seriesName === 'Dérivée (slope)' ? value.toFixed(4) : value.toFixed(2);
              }
              return value;
            }
          }
        },
        stroke: { curve: 'straight', width: 2 },
        markers: { size: 0, hover: { sizeOffset: 3 } }
      };

      // Supprimer l'instance précédente si existante
      if (window.apexChartInstance) {
        window.apexChartInstance.destroy();
      }

      // Créer et rendre le graphique
      window.apexChartInstance = new ApexCharts(document.querySelector("#chart"), options);
      window.apexChartInstance.render();
      if (import.meta.env.DEV) {
        console.log("Chart rendered and instance stored on window.apexChartInstance.");
      }
    } catch (e) {
      console.error("ERREUR DANS LE SCRIPT APEXCHARTS:", e.message, e.stack);
      const chartDiv = document.querySelector("#chart");
      if (chartDiv) {
        chartDiv.innerHTML = "<p style='color:red; text-align:center;'>Erreur lors du rendu du graphique : " + e.message + "</p>";
      }
    }
  `;

  return chartOptionsScript;
}
