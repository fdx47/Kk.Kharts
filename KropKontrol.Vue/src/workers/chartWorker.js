// Web Worker: construit les séries pour l'overview (et potentiellement principal)
// Effectue un pré-calcul en un seul passage par device pour toutes les variables sélectionnées.

import { useComputedVars } from "../composables/useComputedVars.js";
import { calculateCumulativeSeries } from "../composables/useCumulativeCalculations.js";

const SEP = "|";
// Formatter de jour local (Europe/Paris) pour accélérer les cumuls journaliers
const DAY_FMT = new Intl.DateTimeFormat("en-CA", {
  timeZone: "Europe/Paris",
  year: "numeric",
  month: "2-digit",
  day: "2-digit",
});

const dailyCumulCache = new Map();

function datasetSignature(records) {
  if (!records || !records.length) return "empty";
  const len = records.length;
  const first = records[0];
  const last = records[len - 1];
  let hash = 2166136261 >>> 0;
  const step = Math.max(1, Math.floor(len / 64));
  for (let i = 0; i < len; i += step) {
    const rec = records[i];
    const ts = rec?._ts ?? 0;
    const tsLow = ts & 0xffffffff;
    const tsHigh = Math.floor(ts / 4294967296);
    hash ^= tsLow;
    hash = Math.imul(hash, 16777619) >>> 0;
    hash ^= tsHigh & 0xffffffff;
    hash = Math.imul(hash, 16777619) >>> 0;
    if (typeof rec?.water === "number") {
      const w = Math.floor(rec.water * 1000);
      hash ^= w & 0xffffffff;
      hash = Math.imul(hash, 16777619) >>> 0;
    }
  }
  return [
    len,
    first?._ts ?? "",
    last?._ts ?? "",
    first?.devEui ?? "",
    last?.devEui ?? "",
    hash,
  ].join(":");
}

function parseVarKey(key) {
  const idx = key.lastIndexOf(SEP);
  if (idx === -1) return { field: key, devEui: null };
  return { field: key.slice(0, idx), devEui: key.slice(idx + 1) };
}

function ensureTs(rec) {
  if (rec._ts != null) return rec._ts;
  if (rec.timestamp instanceof Date) return rec.timestamp.getTime();
  return Date.parse(rec.timestamp);
}

self.addEventListener("message", async (e) => {
  const {
    id,
    cmd,
    records,
    keys,
    deviceModels = {},
    defaultModel = null,
    showDrainage = false,
  } = e.data || {};
  if (cmd !== "buildSeries") return;
  try {
    // Nettoyage minimal: s'assure que chaque record a _ts, _dayKey et devEui
    const recs = Array.isArray(records)
      ? records.map((r) => {
          const t = ensureTs(r);
          let dk = null;
          try {
            dk = DAY_FMT.format(t);
          } catch {}
          return { ...r, _ts: t, _dayKey: dk };
        })
      : [];
    // Index par devEui
    const byDev = new Map();
    for (const d of recs) {
      const k = d.devEui || "__default__";
      if (!byDev.has(k)) byDev.set(k, []);
      byDev.get(k).push(d);
    }

    // Regroupe variables par devEui
    const keysByDev = new Map();
    keys.forEach((k) => {
      const { devEui } = parseVarKey(k);
      const bucket = devEui || "__default__";
      if (!keysByDev.has(bucket)) keysByDev.set(bucket, []);
      keysByDev.get(bucket).push(k);
    });

    const result = {}; // varKey -> [{x,y}]

    for (const [bucket, vars] of keysByDev.entries()) {
      const devData = bucket === "__default__" ? recs : byDev.get(bucket) || [];
      if (!devData.length) {
        vars.forEach((vk) => {
          result[vk] = [];
        });
        continue;
      }
      devData.sort((a, b) => a._ts - b._ts);
      // Détermine le modèle pour ce bucket
      const model =
        bucket === "__default__"
          ? Number(defaultModel)
          : Number(deviceModels[bucket]);
      const defs = useComputedVars(model, { showDrainage });

      // Prépare compute specs
      const computes = new Map();
      const cumulativeFieldInfo = new Map();
      let needCum61 = false;
      vars.forEach((vk) => {
        const { field } = parseVarKey(vk);
        if (model === 61 && (field === "DLI" || field === "Rayonnement")) {
          computes.set(vk, { type: "cum61", field });
          needCum61 = true;
        } else {
          const def = defs[field];
          if (def?.compute) {
            const entry = { type: "comp", field, fn: def.compute, def };
            if (def.compute?.__dailyCumulKey) {
              entry.dailyKey = def.compute.__dailyCumulKey;
              cumulativeFieldInfo.set(field, {
                key: entry.dailyKey,
                def,
              });
            }
            computes.set(vk, entry);
          } else {
            computes.set(vk, { type: "raw", field });
          }
        }
        result[vk] = [];
      });

      if (cumulativeFieldInfo.size) {
        const signature = datasetSignature(devData);
        const dailyMaps = new Map();
        for (const [field, info] of cumulativeFieldInfo.entries()) {
          const cacheKey = `${bucket}|${info.key || field}`;
          let entry = dailyCumulCache.get(cacheKey);
          if (!entry || entry.signature !== signature) {
            const factory =
              typeof info.def?.rebuild === "function"
                ? info.def.rebuild
                : typeof info.def?.compute?.__dailyFactory === "function"
                  ? info.def.compute.__dailyFactory
                  : null;
            const calculator =
              typeof factory === "function" ? factory() : info.def?.compute;
            const map = new Map();
            if (calculator) {
              for (const point of devData) {
                const value = calculator(point);
                if (value != null && !Number.isNaN(value)) {
                  map.set(point._ts, +value);
                }
              }
            }
            entry = { signature, map };
            dailyCumulCache.set(cacheKey, entry);
          }
          dailyMaps.set(field, entry.map);
        }
        for (const vk of vars) {
          const info = computes.get(vk);
          if (info && info.dailyKey) {
            computes.set(vk, {
              type: "dailyCache",
              field: info.field,
              map: dailyMaps.get(info.field) || new Map(),
            });
          }
        }
      }

      // Cumul 61 si demandé
      let cumSeries = null;
      if (needCum61) {
        const dayMap = {};
        devData.forEach((d) => {
          const dateStr = new Date(d._ts).toISOString().slice(0, 10);
          if (!dayMap[dateStr]) dayMap[dateStr] = [];
          dayMap[dateStr].push(d);
        });
        const dates = Object.keys(dayMap).sort();
        cumSeries = [];
        dates.forEach((dateStr) => {
          const daily = dayMap[dateStr].slice().sort((a, b) => a._ts - b._ts);
          const cumul = calculateCumulativeSeries(daily);
          cumul.forEach((p) => {
            cumSeries.push({ ts: p.ts, dli: p.dli, joules_cm2: p.joules_cm2 });
          });
        });
      }

      // Un passage pour les autres variables
      for (const d of devData) {
        const ts = d._ts;
        for (const vk of vars) {
          const spec = computes.get(vk);
          if (!spec || spec.type === "cum61") continue;
          if (spec.type === "dailyCache") {
            const cached = spec.map?.get(ts);
            if (cached == null || Number.isNaN(cached)) continue;
            result[vk].push({ x: ts, y: +cached });
            continue;
          }
          let y;
          if (spec.type === "comp") y = spec.fn(d);
          else y = d[spec.field];
          if (y == null || Number.isNaN(y)) continue;
          result[vk].push({ x: ts, y: +y });
        }
      }

      if (needCum61 && cumSeries) {
        for (const vk of vars) {
          const { field } = parseVarKey(vk);
          if (field !== "DLI" && field !== "Rayonnement") continue;
          const arr = [];
          if (field === "DLI")
            cumSeries.forEach((p) => arr.push({ x: p.ts, y: p.dli }));
          else cumSeries.forEach((p) => arr.push({ x: p.ts, y: p.joules_cm2 }));
          result[vk] = arr;
        }
      }
    }

    self.postMessage({ id, ok: true, seriesByKey: result });
  } catch (err) {
    self.postMessage({ id, ok: false, error: String(err?.message || err) });
  }
});
