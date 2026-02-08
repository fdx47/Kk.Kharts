// src/services/chartOptionsBuilder.js

/**
 * Construit les options pour un graphique ApexCharts
 * à partir des données et du résultat d'analyse algo6.
 *
 * @param {Array<Object>} arr        Données brutes [{ timestamp, champ, … }, …]
 * @param {{ points?: Array<Object> }} analysis Résultat de `analyzeWateringSeries`
 * @param {string} fieldName         Champ à tracer (par défaut 'permittivite')
 * @returns {object} Options ApexCharts
 */
export function buildChartOptions(arr, analysis, fieldName = "permittivite") {
  const displayName = "Permittivite";

  const seriesPerm = Array.isArray(arr)
    ? arr
        .map((pt) => {
          if (!pt?.timestamp || typeof pt[fieldName] === "undefined") return null;
          const x = new Date(pt.timestamp).getTime();
          if (!Number.isFinite(x)) return null;
          const y = Number.parseFloat(pt[fieldName]);
          return Number.isFinite(y) ? { x, y } : null;
        })
        .filter(Boolean)
    : [];

  const pointSeries = Array.isArray(analysis?.points) ? analysis.points : [];

  const seriesSlope = pointSeries.length
    ? pointSeries
        .map((pt) => {
          if (!pt?.timestamp || typeof pt.slope !== "number") return null;
          const x = new Date(pt.timestamp).getTime();
          if (!Number.isFinite(x)) return null;
          const y = Number.isFinite(pt.slope) ? pt.slope : null;
          return y == null ? null : { x, y };
        })
        .filter(Boolean)
    : [];

  const drainingEvents = Array.isArray(analysis?.events)
    ? analysis.events.filter(
        (ev) =>
          ev?.isDraining &&
          ev.startTime &&
          ev.endTime &&
          Number.isFinite(new Date(ev.startTime).getTime()) &&
          Number.isFinite(new Date(ev.endTime).getTime()),
      )
    : [];

  const drainingMarkers = drainingEvents
    .map((ev) => {
      const x = new Date(ev.startTime).getTime();
      if (!Number.isFinite(x)) return null;

      let y =
        Number.isFinite(ev.startValue) && ev.startValue != null
          ? ev.startValue
          : null;

      if (y == null && Number.isInteger(ev.startIndex)) {
        const point = pointSeries[ev.startIndex];
        if (Number.isFinite(point?.value)) {
          y = point.value;
        } else if (Number.isFinite(point?.smoothedValue)) {
          y = point.smoothedValue;
        }
      }

      if (y == null && Number.isFinite(ev.endValue)) {
        y = ev.endValue;
      }

      return y == null ? null : { x, y };
    })
    .filter(Boolean);

  const options = {
    chart: {
      height: 550,
      type: "line",
      zoom: { enabled: true, type: "x", autoScaleYaxis: true },
      toolbar: {
        show: true,
        tools: {
          zoom: true,
          zoomin: true,
          zoomout: true,
          pan: true,
          reset: true,
          selection: true,
        },
        autoSelected: "zoom",
      },
    },
    series: [
      { name: displayName, data: seriesPerm },
      { name: "Derivee (slope)", data: seriesSlope, yAxisIndex: 1 },
    ],
    annotations: undefined,
    xaxis: {
      type: "datetime",
      labels: { datetimeUTC: false },
    },
    yaxis: [
      {
        show: false,
        axisBorder: { show: false },
        axisTicks: { show: false },
        labels: { show: false },
      },
      {
        show: false,
        axisBorder: { show: false },
        axisTicks: { show: false },
        labels: { show: false },
      },
    ],
    tooltip: {
      shared: true,
      intersect: false,
      x: { format: "dd MMM HH:mm:ss" },
      y: {
        formatter: (value, { seriesIndex }) =>
          typeof value === "number"
            ? seriesIndex === 1
              ? value.toFixed(4)
              : value.toFixed(2)
            : value,
      },
    },
    stroke: { curve: "straight", width: 2 },
    markers: { size: 0, hover: { sizeOffset: 3 } },
  };

  if (drainingMarkers.length) {
    options.series.push({
      name: "Debut arrosage drainant",
      type: "scatter",
      data: drainingMarkers,
      marker: {
        size: 6,
        shape: "circle",
        fillColor: "#b63226",
        strokeColor: "#ffffff",
        strokeWidth: 1.5,
      },
    });
  }

  if (drainingEvents.length) {
    const bands = drainingEvents
      .map((ev) => {
        const x = new Date(ev.startTime).getTime();
        const x2 = new Date(ev.endTime).getTime();
        if (!Number.isFinite(x) || !Number.isFinite(x2) || x2 <= x) return null;
        return {
          x,
          x2,
          borderColor: "#b63226",
          fillColor: "rgba(182,50,38,0.08)",
          opacity: 0.2,
          label: {
            text: "Drain",
            borderColor: "#b63226",
            style: {
              background: "#b63226",
              color: "#ffffff",
            },
          },
        };
      })
      .filter(Boolean);

    if (bands.length) {
      options.annotations = { xaxis: bands };
    }
  }

  return options;
}
