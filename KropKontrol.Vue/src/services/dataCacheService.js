// dataCacheService.js – v7
// -------------------------------------------------------------
// • Rend la fonction normISO robuste : accepte string encodée,
//   string ISO ou objet Date, renvoie toujours une chaîne ISO UTC
//   valide terminée par « Z ».
// • Aucune autre partie du service n’a besoin de changer ; les
//   appels existants depuis fetchWithCache restent inchangés.
// -------------------------------------------------------------

import { openDB } from "idb";
import {
  getUc502Wet150Data,
  getEm300ThData,
  getEm300DiData,
  getUc502ModbusData,
  getUc502MultiSensorData,
  MULTI_SENSOR_CHILD_DELIM,
  MULTI_SENSOR_MODEL,
  isMultiSensorChildDevEui,
} from "./apiService.js";
import { toArray } from "../utils/toArray.js";

/**
 * DATA CACHE SERVICE · v7
 * ————————————————————————————————————————
 * 1.  Isolates **each** IndexedDB interaction in its own fresh connection so
 *     we never run into “database connection is closing” (InvalidStateError).
 * 2.  Keeps the two‑way fill logic introduced in v5 (past & future slices).
 * 3.  Hardened timestamp handling (normISO) – tolerant to Date objects and
 *     URL‑encoded strings, eliminating RangeError / TypeError cascades.
 * 4.  Still outputs detailed console traces when VERBOSE = true.
 */
const VERBOSE = false;

// Évite d'appeler l'API plusieurs fois pour la même tranche temporelle
const pendingFetches = new Map();
const multiSensorRawCache = new Map();

/**
 * Model numbers as exposed by /Device/GetAllDevices
 */
export const MODELS = {
  UC502_WET150: 47,
  EM300_TH: 7,
  EM300_DI: 2,
  UC502_MODBUS: 61,
  UC502_MULTI_WET150: MULTI_SENSOR_MODEL,
};

// ——————————————————————————————————————————
// IndexedDB helpers
// ——————————————————————————————————————————
const DB_NAME = "sensor-cache";
const DB_VERSION = 5; // bump: add byDeviceModelTs index for windowed reads

function upgrade(db, oldVersion, newVersion, transaction) {
  let store;
  if (!db.objectStoreNames.contains("measures")) {
    store = db.createObjectStore("measures", {
      keyPath: ["devEui", "model", "timestamp"],
    });
  } else {
    // Access existing store via upgrade transaction to add missing indexes
    store = transaction.objectStore("measures");
  }
  const idxNames = Array.from(store.indexNames || []);
  if (!idxNames.includes("byDeviceModel")) {
    store.createIndex("byDeviceModel", ["devEui", "model"]);
  }
  if (!idxNames.includes("byDeviceModelTs")) {
    store.createIndex("byDeviceModelTs", ["devEui", "model", "timestamp"]);
  }
}

/**
 * Always return a **fresh** (and therefore open) connection. This avoids the
 * InvalidStateError that occurs when a previously cached connection has been
 * auto‑closed by a `versionchange` event while we still hold a reference.
 */
function openCache() {
  return openDB(DB_NAME, DB_VERSION, { upgrade });
}

// ——————————————————————————————————————————
// Utility fns
// ——————————————————————————————————————————
/**
 * Normalise n’importe quel type de timestamp vers un ISO 8601 UTC se
 * terminant par « Z ». Accepte :
 *   • objet Date            → toISOString()
 *   • string déjà ISO       → inchangé si finit par Z
 *   • string URL‑encodée    → décodée puis convertie si besoin
 */
function normISO(ts) {
  if (!ts) throw new RangeError("normISO : timestamp vide");

  // 1) objet Date – le cas le plus sûr
  if (ts instanceof Date) {
    return ts.toISOString();
  }

  // 2) chaîne : on la décode au cas où elle provient d'encodeURIComponent()
  if (typeof ts === "string") {
    const decoded = decodeURIComponent(ts);
    if (decoded.endsWith("Z")) return decoded; // déjà conforme

    const hasExplicitOffset = /[+-]\d{2}:\d{2}$/.test(decoded);
    const parseString = hasExplicitOffset ? decoded : decoded + "Z";
    const asDate = new Date(parseString);
    if (Number.isNaN(asDate.valueOf())) {
      throw new RangeError("normISO : chaîne date invalide → " + decoded);
    }
    return asDate.toISOString();
  }

  // 3) tout autre type → erreur explicite
  throw new TypeError("normISO : type inconnu (" + typeof ts + ")");
}

function tsNum(ts) {
  return Date.parse(normISO(ts));
}

function log(...args) {
  if (VERBOSE) console.debug("%c[Cache]", "color:teal", ...args);
}

function splitMultiSensorDevEui(devEui) {
  if (!isMultiSensorChildDevEui(devEui)) return null;
  const [base, suffix] = devEui.split(MULTI_SENSOR_CHILD_DELIM);
  const index = Number(suffix);
  if (!Number.isInteger(index) || index <= 0) return null;
  return { baseDevEui: base, sensorIndex: index };
}

// ——————————————————————————————————————————
// Core – fetch with cache & automatic gap filling
// ——————————————————————————————————————————
async function fetchWithCache(model, devEui, startISO, endISO, fetchFn) {
  const key = `${model}|${devEui}|${startISO}|${endISO}`;
  if (pendingFetches.has(key)) return pendingFetches.get(key);

  const task = (async () => {
    log("▶︎ call", { devEui, model, startISO, endISO });

    const db = await openCache();
    const rStore = db.transaction("measures").objectStore("measures");

    // 1️⃣ local slice ----------------------------------------------------------
    const normStart = normISO(startISO);
    const normEnd = normISO(endISO);
    let local = [];
    try {
      const idxNames = Array.from(rStore.indexNames || []);
      if (idxNames.includes("byDeviceModelTs")) {
        const range = IDBKeyRange.bound(
          [devEui, model, normStart],
          [devEui, model, normEnd],
        );
        local = await rStore.index("byDeviceModelTs").getAll(range);
      } else {
        const allLocal = await rStore
          .index("byDeviceModel")
          .getAll([devEui, model]);
        local = allLocal
          .filter((m) => m.timestamp)
          .filter(
            (m) =>
              tsNum(m.timestamp) >= tsNum(normStart) &&
              tsNum(m.timestamp) <= tsNum(normEnd),
          );
      }
    } catch (e) {
      console.warn(
        "[Cache] local window read failed, fallback to full scan",
        e,
      );
      const allLocal = await rStore
        .index("byDeviceModel")
        .getAll([devEui, model]);
      local = allLocal
        .filter((m) => m.timestamp)
        .filter(
          (m) =>
            tsNum(m.timestamp) >= tsNum(normStart) &&
            tsNum(m.timestamp) <= tsNum(normEnd),
        );
    }
    log("local hits", local.length);

    // first & last timestamps presently in cache for requested window
    const firstTsNum =
      local.length === 0
        ? Number.POSITIVE_INFINITY
        : local.reduce(
            (min, m) => Math.min(min, tsNum(m.timestamp)),
            Number.POSITIVE_INFINITY,
          );

    const lastTsNum =
      local.length === 0
        ? tsNum(normStart)
        : local.reduce(
            (max, m) => Math.max(max, tsNum(m.timestamp)),
            tsNum(normStart),
          );

    let remoteOld = [];
    // 2️⃣ fetch **older** missing slice (only if some data already cached) -----
    if (
      firstTsNum !== Number.POSITIVE_INFINITY &&
      firstTsNum > tsNum(normStart)
    ) {
      const missingEndISO = new Date(firstTsNum - 1).toISOString();
      log("remoteOld fetch until", missingEndISO);
      try {
        remoteOld = toArray(await fetchFn(devEui, normStart, missingEndISO))
          .filter((m) => m.timestamp)
          .map((m) => ({
            ...m,
            timestamp: normISO(m.timestamp),
          }));
      } catch (err) {
        console.error("[Cache] remoteOld network error", err);
      }
      log("remoteOld length", remoteOld.length);
    }

    // The newest point we have *after* merging remoteOld (if any)
    const newestLocalNum = Math.max(
      lastTsNum,
      ...remoteOld.map((m) => tsNum(m.timestamp)),
    );
    if (newestLocalNum >= tsNum(normEnd)) {
      log("cache fully satisfied – network skip");
      const merged = [...remoteOld, ...local].sort((a, b) =>
        a.timestamp.localeCompare(b.timestamp),
      );
      return merged;
    }

    // 3️⃣ fetch **newer** missing slice ---------------------------------------
    const fetchStartISO = new Date(newestLocalNum).toISOString();
    log("remoteNew fetch from", fetchStartISO);

    let remoteNew = [];
    try {
      remoteNew = toArray(await fetchFn(devEui, fetchStartISO, normEnd))
        .filter((m) => m.timestamp)
        .map((m) => ({
          ...m,
          timestamp: normISO(m.timestamp),
        }));
    } catch (err) {
      console.error(
        "[Cache] remoteNew network error – returning partial data",
        err,
      );
      const mergedPartial = [...remoteOld, ...local].sort((a, b) =>
        a.timestamp.localeCompare(b.timestamp),
      );
      return mergedPartial;
    }
    log("remoteNew length", remoteNew.length);

    // 4️⃣ persist -------------------------------------------------------------
    if (remoteOld.length || remoteNew.length) {
      const wDb = await openCache(); // fresh connection for RW tx
      const wTx = wDb.transaction("measures", "readwrite");
      const wStore = wTx.objectStore("measures");
      await Promise.all([
        ...remoteOld.map((m) => wStore.put({ ...m, devEui, model })),
        ...remoteNew.map((m) => wStore.put({ ...m, devEui, model })),
      ]);
      await wTx.done;

      const total = (
        await (await openCache())
          .transaction("measures")
          .objectStore("measures")
          .index("byDeviceModel")
          .getAll([devEui, model])
      ).length;
      log(
        "saved to IDB",
        remoteOld.length + remoteNew.length,
        "— total now",
        total,
      );
    }

    // 5️⃣ return merged --------------------------------------------------------
    const merged = [...remoteOld, ...local, ...remoteNew]
      .sort((a, b) => a.timestamp.localeCompare(b.timestamp))
      .filter((m, i, arr) => i === 0 || m.timestamp !== arr[i - 1].timestamp);
    log("return size", merged.length);
    return merged;
  })();

  pendingFetches.set(key, task);
  try {
    return await task;
  } finally {
    pendingFetches.delete(key);
  }
}

// ——————————————————————————————————————————
async function fetchUc502MultiSensorChild(devEui, startISO, endISO) {
  const parsed = splitMultiSensorDevEui(devEui);
  if (!parsed) {
    log('multi-sensor fetch called with non child devEui', devEui);
    return [];
  }
  const { baseDevEui, sensorIndex } = parsed;
  const key = `${baseDevEui}|${normISO(startISO)}|${normISO(endISO)}`;
  let promise = multiSensorRawCache.get(key);
  if (!promise) {
    promise = getUc502MultiSensorData(baseDevEui, startISO, endISO);
    multiSensorRawCache.set(key, promise);
  }
  let payload;
  try {
    payload = await promise;
  } finally {
    multiSensorRawCache.delete(key);
  }
  const seriesKey = `sdi12_${sensorIndex}`;
  const series = Array.isArray(payload?.[seriesKey]) ? payload[seriesKey] : [];
  return series
    .map((entry) => ({
      timestamp: entry.timestamp,
      permittivite: entry.permittivite ?? null,
      eCb: entry.eCb ?? null,
      soilTemperature: entry.soilTemperature ?? null,
      mineralVWC: entry.mineralVWC ?? null,
      organicVWC: entry.organicVWC ?? null,
      peatMixVWC: entry.peatMixVWC ?? null,
      coirVWC: entry.coirVWC ?? null,
      minWoolVWC: entry.minWoolVWC ?? null,
      perliteVWC: entry.perliteVWC ?? null,
      mineralECp: entry.mineralECp ?? null,
      organicECp: entry.organicECp ?? null,
      peatMixECp: entry.peatMixECp ?? null,
      coirECp: entry.coirECp ?? null,
      minWoolECp: entry.minWoolECp ?? null,
      perliteECp: entry.perliteECp ?? null,
      battery: entry.battery ?? null,
    }))
    .sort((a, b) => String(a.timestamp).localeCompare(String(b.timestamp)));
}

// Cached wrappers
// ——————————————————————————————————————————
export const getUc502Wet150DataCached = (d, s, e) =>
  fetchWithCache(MODELS.UC502_WET150, d, s, e, getUc502Wet150Data);
export const getEm300ThDataCached = (d, s, e) =>
  fetchWithCache(MODELS.EM300_TH, d, s, e, getEm300ThData);
export const getEm300DiDataCached = (d, s, e) =>
  fetchWithCache(MODELS.EM300_DI, d, s, e, getEm300DiData);
export const getUc502ModbusDataCached = (d, s, e) =>
  fetchWithCache(MODELS.UC502_MODBUS, d, s, e, getUc502ModbusData);

export const getUc502MultiSensorDataCached = (d, s, e) =>
  fetchWithCache(
    MODELS.UC502_MULTI_WET150,
    d,
    s,
    e,
    fetchUc502MultiSensorChild,
  );

/**
 * Assure la présence en cache des données sur la fenêtre des N derniers jours (max 30)
 * pour un device et un modèle donnés. Utilise les wrappers *Cached existants,
 * qui comblent automatiquement les trous via fetchWithCache.
 *
 * @param {string} devEui  Identifiant du device
 * @param {number} model   Numéro de modèle (voir MODELS)
 * @param {number} days    Nombre de jours à couvrir (défaut 30, borné à 30)
 * @returns {Promise<number>} Nombre de points ramenés (approx, selon wrapper)
 */
export async function ensureCacheForLastNDays(devEui, model, days = 30) {
  try {
    if (isMultiSensorChildDevEui(devEui)) {
      model = MODELS.UC502_MULTI_WET150;
    }
    const n = Math.max(1, Math.min(30, Number(days) || 30));
    const end = new Date();
    const endISO = end.toISOString();
    const start = new Date(end.getTime() - n * 86400000);
    const startISO = start.toISOString();

    // Sélectionne le fetcher cache selon modèle
    let fetcher;
    switch (Number(model)) {
      case MODELS.UC502_WET150:
        fetcher = getUc502Wet150DataCached;
        break;
      case MODELS.EM300_TH:
        fetcher = getEm300ThDataCached;
        break;
      case MODELS.EM300_DI:
        fetcher = getEm300DiDataCached;
        break;
      case MODELS.UC502_MODBUS:
        fetcher = getUc502ModbusDataCached;
        break;
      case MODELS.UC502_MULTI_WET150:
        fetcher = getUc502MultiSensorDataCached;
        break;
      default:
        console.warn('[Cache] ensureCacheForLastNDays: modèle non supporté', model);
        return 0;
    }

    const data = await fetcher(devEui, startISO, endISO);
    return Array.isArray(data) ? data.length : (Array.isArray(data?.data) ? data.data.length : 0);
  } catch (err) {
    console.error('[Cache] ensureCacheForLastNDays error', err);
    return 0;
  }
}

/**
 * Retourne uniquement les mesures déjà en cache pour la tranche demandée.
 * Aucun appel réseau n’est effectué.
 */
export async function getCachedSlice(model, devEui, startISO, endISO) {
  const db = await openCache();
  const store = db.transaction("measures").objectStore("measures");
  const normStart = normISO(startISO);
  const normEnd = normISO(endISO);
  try {
    const idxNames = Array.from(store.indexNames || []);
    if (idxNames.includes("byDeviceModelTs")) {
      const range = IDBKeyRange.bound(
        [devEui, model, normStart],
        [devEui, model, normEnd],
      );
      const res = await store.index("byDeviceModelTs").getAll(range);
      return res.sort((a, b) => a.timestamp.localeCompare(b.timestamp));
    }
  } catch (e) {
    console.warn(
      "[Cache] getCachedSlice window read failed, fallback to full scan",
      e,
    );
  }
  const all = await store.index("byDeviceModel").getAll([devEui, model]);
  return all
    .filter(
      (m) =>
        tsNum(m.timestamp) >= tsNum(normStart) &&
        tsNum(m.timestamp) <= tsNum(normEnd),
    )
    .sort((a, b) => a.timestamp.localeCompare(b.timestamp));
}

export { fetchWithCache };

// Supprime toutes les mesures de l'IndexedDB
export async function clearCache() {
  const db = await openCache();
  await db.clear("measures");
}
