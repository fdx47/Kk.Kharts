/**
 * @file useComputedVars.js
 * @description
 * Fournit des variables environnementales calculées en fonction du modèle de capteur.
 * - Model 2 (ex : EM300-DI) : volume d’eau (delta), humidité absolue, VPD g/m³, VPD kPa, point de rosée.
 * - Model 7 : idem sans volume.
 * - Model 61 : rayonnement PAR en W/m².
 */

import { MODELS } from "@/services/dataCacheService.js";

const DRAINAGE_MODEL_IDS = new Set(
  [MODELS.UC502_WET150, MODELS.UC502_MULTI_WET150].filter(
    (value) => typeof value === "number" && !Number.isNaN(value),
  ),
);

// Fuseau local pour le reset quotidien
const LOCAL_TZ = "Europe/Paris";
// Formatter de date local réutilisable pour obtenir une clé de jour AAAA-MM-JJ
// Utilise Intl (plus léger que Luxon) et accepte un timestamp number directement
const DAY_FMT = new Intl.DateTimeFormat("en-CA", {
  timeZone: LOCAL_TZ,
  year: "numeric",
  month: "2-digit",
  day: "2-digit",
});

// Conserve une valeur entre les appels (comme une mémoire interne)
const withPrevious = (fn) => {
  let previous = null;
  return (current) => {
    const result = fn(current, previous);
    previous = current;
    return result;
  };
};

/**
 * Cumul une valeur par jour et réinitialise le total à chaque changement de date (minuit local)
 */
const withDailyCumulative = (fn) => {
  let previous = null;
  let total = 0;
  let day = null; // clé de jour locale AAAA-MM-JJ
  return (current) => {
    let currentDay = null;
    // Utilise _ts si disponible (pré-calculé, number), sinon retombe sur timestamp
    // et évite Luxon pour réduire la charge CPU.
    const rawTs =
      current?._ts ??
      (current?.timestamp instanceof Date
        ? current.timestamp.getTime()
        : current?.timestamp != null
          ? Date.parse(current.timestamp)
          : null);
    if (typeof current?._dayKey === "string" && current._dayKey) {
      currentDay = current._dayKey;
    } else if (typeof rawTs === "number" && !Number.isNaN(rawTs)) {
      // DAY_FMT accepte un number: pas besoin de créer un objet Date
      currentDay = DAY_FMT.format(rawTs); // ex: 2025-03-07
    }

    if (day !== currentDay) {
      day = currentDay;
      previous = null;
      total = 0;
    }

    const delta = fn(current, previous);
    previous = current;
    if (delta != null) total += delta;
    return total;
  };
};

const attachDailyMetadata = (fn, key, factory) => {
  if (typeof fn === "function") {
    fn.__dailyCumulKey = key;
    fn.__dailyFactory = factory;
  }
  return fn;
};

const createDailyCumulativeDef = (label, key, factory) => ({
  label,
  compute: attachDailyMetadata(factory(), key, factory),
  rebuild: factory,
});


export const useComputedVars = (model, options = {}) => {
  const { showDrainage = false } = options ?? {};
  const modelString = model == null ? "" : String(model);
  const numericModel = Number.parseInt(modelString.split("|")[0], 10);
  const supportsDrainagePercent =
    Number.isNaN(numericModel) ? false : DRAINAGE_MODEL_IDS.has(numericModel);
  const includeDrainage = showDrainage && supportsDrainagePercent;

  const base = includeDrainage
    ? {
        drainagePercent: {
          label: "% Drain (algo6)",
          compute: (d) => {
            if (!d) return null;
            const raw =
              typeof d.drainagePercent !== "undefined"
                ? d.drainagePercent
                : d.drainagePercentage;
            const num =
              typeof raw === "number"
                ? raw
                : Number.parseFloat(raw ?? Number.NaN);
            return Number.isFinite(num) ? num : null;
          },
        },
      }
    : {};

  const absHumidity = (t, rh) => {
    const svp = 6.112 * Math.exp((17.67 * t) / (t + 243.5));
    const vp = (svp * rh) / 100;
    return (vp * 216.7) / (273.15 + t);
  };

  const vpdGm3 = (t, rh) => {
    const svp = 6.112 * Math.exp((17.67 * t) / (t + 243.5));
    const vp = (svp * rh) / 100;
    const vpd_hPa = svp - vp;
    return (vpd_hPa * 216.7) / (273.15 + t);
  };

  const vpdKpa = (t, rh) => {
    const es = 0.6108 * Math.exp((17.27 * t) / (t + 237.3));
    const ea = (es * rh) / 100;
    return es - ea;
  };

  const dewPoint = (t, rh) => {
    const a = 17.27;
    const b = 237.7;
    const alpha = (a * t) / (b + t) + Math.log(rh / 100);
    return (b * alpha) / (a - alpha);
  };

  const computedByModel = {
    2: {
      volumeDelta: {
        label: "Apport en eau instantanné (L)",
        compute: withPrevious((current, previous) => {
          const valuePerPulse = 0.00428;
          if (
            current?.water == null ||
            previous?.water == null ||
            valuePerPulse == null
          )
            return null;

          const delta = (current.water - previous.water) * valuePerPulse;
          return delta >= 0 ? delta : 0;
        }),
      },
      volumeDelta_mm: {
        label: "Pluviometrie instantannée (mm)",
        compute: withPrevious((current, previous) => {
          const valuePerPulse = 0.2;
          if (
            current?.water == null ||
            previous?.water == null ||
            valuePerPulse == null
          )
            return null;

          const delta = (current.water - previous.water) * valuePerPulse;
          return delta >= 0 ? delta : 0;
        }),
      },
      volumeDaily: createDailyCumulativeDef(
        "Volume cumulé (L)",
        "volumeDaily",
        () =>
          withDailyCumulative((current, previous) => {
            const valuePerPulse = 0.00428;
            if (
              current?.water == null ||
              previous?.water == null ||
              valuePerPulse == null
            )
              return null;

            const delta = (current.water - previous.water) * valuePerPulse;
            return delta >= 0 ? delta : 0;
          }),
      ),
      volumeDailymm: createDailyCumulativeDef(
        "Précipitations cumulées (mm)",
        "volumeDailymm",
        () =>
          withDailyCumulative((current, previous) => {
            const valuePerPulse = 0.2;
            if (
              current?.water == null ||
              previous?.water == null ||
              valuePerPulse == null
            )
              return null;

            const delta = (current.water - previous.water) * valuePerPulse;
            return delta >= 0 ? delta : 0;
          }),
      ),
      absoluteHumidity: {
        label: "Humidité absolue (g/m³)",
        compute: (d) =>
          d.temperature != null && d.humidity != null
            ? absHumidity(d.temperature, d.humidity)
            : null,
      },
      vpdGm3: {
        label: "VPD (g/m³)",
        compute: (d) =>
          d.temperature != null && d.humidity != null
            ? vpdGm3(d.temperature, d.humidity)
            : null,
      },
      vpdKpa: {
        label: "VPD (kPa)",
        compute: (d) =>
          d.temperature != null && d.humidity != null
            ? vpdKpa(d.temperature, d.humidity)
            : null,
      },
      dewPoint: {
        label: "Point de rosée (°C)",
        compute: (d) =>
          d.temperature != null && d.humidity != null
            ? dewPoint(d.temperature, d.humidity)
            : null,
      },
    },
    7: {
      absoluteHumidity: {
        label: "Humidité absolue (g/m³)",
        compute: (d) =>
          d.temperature != null && d.humidity != null
            ? absHumidity(d.temperature, d.humidity)
            : null,
      },
      vpdGm3: {
        label: "VPD (g/m³)",
        compute: (d) =>
          d.temperature != null && d.humidity != null
            ? vpdGm3(d.temperature, d.humidity)
            : null,
      },
      vpdKpa: {
        label: "VPD (kPa)",
        compute: (d) =>
          d.temperature != null && d.humidity != null
            ? vpdKpa(d.temperature, d.humidity)
            : null,
      },
      dewPoint: {
        label: "Point de rosée (°C)",
        compute: (d) =>
          d.temperature != null && d.humidity != null
            ? dewPoint(d.temperature, d.humidity)
            : null,
      },
    },
    61: {
      parWm2: {
        label: "Rayonnement (W/m²)",
        compute: (d) =>
          d.ModbusChannel1 != null ? d.ModbusChannel1 * 0.219 : null,
      },
    },
  };

  return {
    ...base,
    ...(Number.isNaN(numericModel) ? {} : computedByModel[numericModel] || {}),
  };
};

export default useComputedVars;
