// src/services/apiService.js

//const API_BASE = 'https://kropkontrol.premiumasp.net/api/v1';
import { API_BASE } from "@/config/constants.js";
import { fetchWithAuth } from "./authService.js";

/******* Récupère le token stocké *******/

const MULTI_SENSOR_CHILD_DELIM = "::sdi12-";
const MULTI_SENSOR_MODEL = 62;

const multiSensorMetadataCache = new Map();

function resolveBaseDevEui(devEui) {
  if (typeof devEui !== "string") return devEui;
  const sepIndex = devEui.indexOf(MULTI_SENSOR_CHILD_DELIM);
  return sepIndex === -1 ? devEui : devEui.slice(0, sepIndex);
}

function parseMultiSensorDevEui(devEui) {
  if (!isMultiSensorChildDevEui(devEui)) return null;
  const parts = devEui.split(MULTI_SENSOR_CHILD_DELIM);
  if (parts.length !== 2) return null;
  const index = Number(parts[1]);
  return Number.isInteger(index) && index > 0 ? index : null;
}

function isMultiSensorChildDevEui(devEui) {
  return typeof devEui === "string" && devEui.includes(MULTI_SENSOR_CHILD_DELIM);
}

function cloneWithoutSensorArrays(device) {
  const { sdi12_1, sdi12_2, sdi12_3, sdi12_4, ...rest } = device || {};
  return { ...rest };
}

function normaliseMetadata(meta) {
  if (!meta) return null;
  let obj = meta;
  if (typeof obj === "string") {
    const trimmed = obj.trim();
    if (!trimmed) return null;
    try {
      obj = JSON.parse(trimmed);
    } catch {
      obj = { name: decodeURIComponent(trimmed.replace(/\+/g, ' ')) };
    }
  }
  if (typeof obj !== "object" || obj === null) {
    const coerced = String(obj ?? "").trim();
    if (!coerced) return null;
    return { name: decodeURIComponent(coerced.replace(/\+/g, ' ')), installationLocation: "" };
  }
  const rawName = obj.name ?? obj.Name ?? obj.label ?? obj.Label ?? "";
  let name = decodeURIComponent(String(rawName ?? "").trim().replace(/\+/g, ' '));
  const rawLocation = obj.installationLocation ?? obj.InstallationLocation ?? "";
  const location = decodeURIComponent(String(rawLocation ?? "").trim().replace(/\+/g, ' '));
  if (!name && !location) return null;
  if (!name && location) name = location;
  return { ...obj, name, installationLocation: location };
}

function formatMultiSensorDate(date) {
  const pad = (n) => (n < 10 ? `0${n}` : `${n}`);
  const year = date.getFullYear();
  const month = pad(date.getMonth() + 1);
  const day = pad(date.getDate());
  const hours = pad(date.getHours());
  const minutes = pad(date.getMinutes());
  return `${year}-${month}-${day} ${hours}:${minutes}`;
}

function formatMultiSensorDateParam(value) {
  if (value instanceof Date) return formatMultiSensorDate(value);
  if (typeof value === "number") {
    const date = new Date(value);
    if (!Number.isNaN(date.getTime())) return formatMultiSensorDate(date);
  }
  if (typeof value === "string") {
    const trimmed = value.trim();
    if (!trimmed) return trimmed;
    const direct = trimmed.replace('Z', '').replace('T', ' ');
    const parsedDirect = Date.parse(direct.replace(' ', 'T'));
    if (!Number.isNaN(parsedDirect)) return formatMultiSensorDate(new Date(parsedDirect));
    const parsedIso = Date.parse(trimmed);
    if (!Number.isNaN(parsedIso)) return formatMultiSensorDate(new Date(parsedIso));
    return trimmed;
  }
  const fallback = new Date(value);
  if (!Number.isNaN(fallback.getTime())) return formatMultiSensorDate(fallback);
  return value ?? "";
}

async function fetchMultiSensorMetadata(devEui) {
  const baseDevEui = resolveBaseDevEui(devEui);
  if (multiSensorMetadataCache.has(baseDevEui)) {
    return multiSensorMetadataCache.get(baseDevEui);
  }
  const now = new Date();
  const endParam = formatMultiSensorDateParam(now);
  const startParam = formatMultiSensorDateParam(new Date(now.getTime() - 24 * 60 * 60 * 1000));
  try {
    const payload = await getUc502MultiSensorData(baseDevEui, startParam, endParam);
    multiSensorMetadataCache.set(baseDevEui, payload);
    return payload;
  } catch (err) {
    console.error("getDevices: échec du chargement des métadonnées multi-sondes", err);
    multiSensorMetadataCache.set(baseDevEui, null);
    return null;
  }
}

function deviceMetadataEntries(device, fallback) {
  return [
    { meta: normaliseMetadata(device?.sdi12_1_metadata ?? fallback?.sdi12_1_metadata), index: 1 },
    { meta: normaliseMetadata(device?.sdi12_2_metadata ?? fallback?.sdi12_2_metadata), index: 2 },
    { meta: normaliseMetadata(device?.sdi12_3_metadata ?? fallback?.sdi12_3_metadata), index: 3 },
    { meta: normaliseMetadata(device?.sdi12_4_metadata ?? fallback?.sdi12_4_metadata), index: 4 },
  ].filter(({ meta }) => meta);
}

async function expandMultiSensorDevices(list) {
  if (!Array.isArray(list)) return [];
  const expanded = [];
  for (const device of list) {
    if (Number(device?.model) !== MULTI_SENSOR_MODEL) {
      expanded.push(device);
      continue;
    }
    const base = cloneWithoutSensorArrays(device);
    const payload = await fetchMultiSensorMetadata(device.devEui);
    const metadataEntries = deviceMetadataEntries(device, payload);
    if (!metadataEntries.length) {
      expanded.push(device);
      continue;
    }
    for (const { meta, index } of metadataEntries) {
      const childName = meta.name || base.name || base.deviceName || `${device.devEui} SDI-${index}`;
      const rawDescription = meta.installationLocation || "";
      const fallbackDescription = base.installationLocation || base.description || "";
      if (import.meta.env?.DEV) {
        console.debug('[MultiSensor] metadata entry', {
          devEui: device.devEui,
          index,
          metaName: meta.name,
          childName,
          rawDescription,
          fallbackDescription,
        });
      }
      const child = {
        ...base,
        devEui: `${device.devEui}${MULTI_SENSOR_CHILD_DELIM}${index}`,
        baseDevEui: device.devEui,
        deviceId: `${device.deviceId ?? device.devEui}-${index}`,
        model: 47,
        originalModel: device.model,
        sensorIndex: index,
        sensorMetadata: meta,
        name: meta.name || childName,
        deviceName: meta.name || childName,
        description: meta.name || childName,
        installationLocation: rawDescription || fallbackDescription || null,
        isMultiSensorChild: true,
      };
      expanded.push(child);
    }
  }
  return expanded;
}

function getHeaders() {
  return {
    "Content-Type": "application/json",
  };
}

/******* Récupère la liste des devices du compte *******/
export async function getDevices() {
  const res = await fetchWithAuth(`${API_BASE}/devices`, {
    headers: getHeaders(), // Recupere le token stocke
  });
  if (!res.ok) {
    throw new Error(`Erreur ${res.status} lors de la recuperation des devices`);
  }
  const list = await res.json(); // tableau d'objets avec { devEui, deviceName, model, description, ... }
  return await expandMultiSensorDevices(list);
}

// Recupere les informations detaillees d'un device
export async function getDeviceByDevEui(devEui) {
  const encoded = encodeURIComponent(resolveBaseDevEui(devEui));
  const res = await fetchWithAuth(
    `${API_BASE}/devices/${encoded}?devEui=${encoded}`,
    { headers: getHeaders() },
  );
  if (!res.ok) {
    throw new Error(`Erreur ${res.status} lors de la récupération du device`);
  }
  return await res.json();
}

// --- Companies ------------------------------------------------------------
/**
 * Récupère la liste des compagnies disponibles.
 * Retourne un tableau d'objets ayant au moins { id, name }.
 */
export async function getCompanies() {
  const res = await fetchWithAuth(`${API_BASE}/companies`, {
    headers: getHeaders(),
  });
  if (!res.ok) {
    throw new Error(
      `Erreur ${res.status} lors de la récupération des compagnies`,
    );
  }
  return await res.json();
}

// --- Device configuration -------------------------------------------------
export async function getDeviceConfig(devEui) {
  return getDeviceByDevEui(devEui);
}

export async function updateDeviceConfig(devEui, config) {
  const res = await fetchWithAuth(
    `${API_BASE}/devices/${encodeURIComponent(resolveBaseDevEui(devEui))}/config`,
    {
      method: "PUT",
      headers: getHeaders(),
      body: JSON.stringify(config),
    },
  );
  if (!res.ok) {
    throw new Error(
      `Erreur ${res.status} lors de l'enregistrement de la configuration`,
    );
  }
  if (res.status === 204) {
    return {};
  }
  if (res.headers.get("content-type")?.includes("application/json")) {
    return await res.json();
  }
  return {};
}

// Récupère les seuils d'alarme d'un dispositif
export async function getThresholdsAlarms(devEui) {
  const res = await fetchWithAuth(
    `${API_BASE}/devices/${encodeURIComponent(resolveBaseDevEui(devEui))}/thresholds-alarms`,
    { headers: getHeaders() },
  );
  if (!res.ok) {
    throw new Error(`Erreur ${res.status} lors de la récupération des seuils`);
  }
  return await res.json();
}

// Envoie les seuils d'alarme pour un ou plusieurs dispositifs
export async function postThresholdsAlarms(data) {
  // Endpoint RESTful: /devices/thresholds-alarms
  const url = `${API_BASE}/devices/thresholds-alarms`;
  if (import.meta.env.DEV) {
    console.log("POST", url, JSON.stringify(data));
  }
  const res = await fetchWithAuth(url, {
    method: "POST",
    headers: getHeaders(),
    body: JSON.stringify(data),
  });
  if (!res.ok) {
    let text = "";
    try {
      text = await res.text();
    } catch {}
    console.error("postThresholdsAlarms failed", res.status, text);
    throw new Error(
      `Erreur ${res.status} lors de l'enregistrement des seuils: ${text}`,
    );
  }
  if (res.headers.get("content-type")?.includes("application/json")) {
    return await res.json();
  }
  return {};
}

// --- Mapping des labels par type de capteur ---
const labelMapUC502Wet150 = {
  permittivite: "Permittivité",
  soilTemperature: "Température du Sol (°C)",
  mineralVWC: "VWC - Minérale",
  organicVWC: "VWC - Organique",
  peatMixVWC: "VWC - Mélange de Tourbe",
  coirVWC: "VWC - Fibre de Coco",
  minWoolVWC: "VWC - Laine Minérale",
  perliteVWC: "VWC - Perlite",
  mineralECp: "Ec Minérale (ECp)",
  organicECp: "Ec Organique (ECp)",
  peatMixECp: "Ec Mélange de Tourbe (ECp)",
  coirECp: "Ec Fibre de Coco (ECp)",
  minWoolECp: "Ec Laine Minérale (ECp)",
  perliteECp: "Ec Perlite (ECp)",
  battery: "Batterie (V)",
  drainagePercent: "% Drain (algo6)",
};

const labelMapEm300TH = {
  temperature: "Température (°C)",
  humidity: "Humidité (%)",
  battery: "Batterie (V)",
};

const labelMapEm300DI = {
  temperature: "Température (°C)",
  humidity: "Humidité (%)",
  water: "Compteur",
  battery: "Batterie (V)",
};

const labelMapUC502Modbus = {
  ModbusChannel1: "Modbus Channel 1",
  ModbusChannel2: "Modbus Channel 2",
  ModbusChannel3: "Modbus Channel 3",
  ModbusChannel4: "Modbus Channel 4",
  ModbusChannel5: "Modbus Channel 5",
  ModbusChannel6: "Modbus Channel 6",
  Battery: "Batterie (V)",
};
const labelMapUC502_2Wet150 = {
  permittivite1: "Permittivité_1",
  eCb1: "Ec Bulk_1",
  soilTemperature1: "Température du Sol (°C)_1",
  permittivite2: "Permittivité_2",
  eCb2: "Ec Bulk_2",
  soilTemperature2: "Température du Sol (°C)_2",
  battery: "Batterie (V)",
};

// Correspondance entre le numéro de modèle du dispositif et son mapping de labels spécifique.
const labelMapsByModelNumber = {
  2: labelMapEm300DI,
  7: labelMapEm300TH,
  47: labelMapUC502Wet150,
  61: labelMapUC502Modbus,
  62: labelMapUC502Wet150,
};

export function getLabelMap(device) {
  return (
    labelMapsByModelNumber[Number(device.model ?? device.deviceId)] ||
    labelMapUC502Wet150
  );
}

/******* Récupère les données de type EM300Th *******/
export async function getEm300ThData(devEui, startDate, endDate) {
  const url = new URL(`${API_BASE}/em300/${encodeURIComponent(devEui)}/th`);
  url.searchParams.set("startDate", startDate);
  url.searchParams.set("endDate", endDate);
  const res = await fetchWithAuth(url.toString(), { headers: getHeaders() });

  if (!res.ok) {
    throw new Error(
      `Erreur ${res.status} lors de la récupération des données EM300Th`,
    );
  }

  return await res.json();
}

/******* Récupère les données de type EM300DI *******/
export async function getEm300DiData(devEui, startDate, endDate) {
  const url = new URL(`${API_BASE}/em300/${encodeURIComponent(devEui)}/di`);
  url.searchParams.set("startDate", startDate);
  url.searchParams.set("endDate", endDate);
  const res = await fetchWithAuth(url.toString(), { headers: getHeaders() });

  if (!res.ok) {
    throw new Error(
      `Erreur ${res.status} lors de la récupération des données EM300Di`,
    );
  }

  return await res.json();
}

/******* Récupère les données de type UC502Wet150 *******/
export async function getUc502Wet150Data(devEui, startDate, endDate) {
  const sensorIndex = parseMultiSensorDevEui(devEui);
  if (sensorIndex != null) {
    const payload = await getUc502MultiSensorData(devEui, startDate, endDate);
    const seriesKey = `sdi12_${sensorIndex}`;
    const fallbackKey = `sdi12_${sensorIndex}_`;
    const data = Array.isArray(payload?.[seriesKey])
      ? payload[seriesKey]
      : Array.isArray(payload?.[fallbackKey])
        ? payload[fallbackKey]
        : [];
    return data;
  }

  const url = new URL(`${API_BASE}/uc502/${encodeURIComponent(devEui)}/wet150`);
  url.searchParams.set("startDate", startDate);
  url.searchParams.set("endDate", endDate);
  const res = await fetchWithAuth(url.toString(), { headers: getHeaders() });

  if (!res.ok) {
    throw new Error(
      `Erreur ${res.status} lors de la recuperation des donnees UC502Wet150`,
    );
  }

  return await res.json();
}

export async function getUc502MultiSensorData(devEui, startDate, endDate) {
  const url = new URL(`${API_BASE}/uc502/wet150/multisensor`);
  url.searchParams.set("devEui", resolveBaseDevEui(devEui));
  url.searchParams.set("startDate", formatMultiSensorDateParam(startDate));
  url.searchParams.set("endDate", formatMultiSensorDateParam(endDate));
  const res = await fetchWithAuth(url.toString(), { headers: getHeaders() });

  if (!res.ok) {
    throw new Error(
      `Erreur ${res.status} lors de la recuperation des donnees UC502 multi-sondes`,
    );
  }

  return await res.json();
}


/******* Récupère les données de type UC502Modbus *******/
export async function getUc502ModbusData(devEui, startDate, endDate) {
  const url = new URL(`${API_BASE}/uc502/${encodeURIComponent(devEui)}/modbus`);
  url.searchParams.set("startDate", startDate);
  url.searchParams.set("endDate", endDate);
  const res = await fetchWithAuth(url.toString(), { headers: getHeaders() });

  if (!res.ok) {
    throw new Error(
      `Erreur ${res.status} lors de la récupération des données UC502Modbus`,
    );
  }
  const json = await res.json();
  const rawMeasures = Array.isArray(json) ? json : (json.data ?? []);
  return rawMeasures.map((m) => ({
    timestamp: m.timestamp,
    ModbusChannel1: m.modbus_chn_1 ?? null,
    ModbusChannel2: m.modbus_chn_2 ?? null,
    ModbusChannel3: m.modbus_chn_3 ?? null,
    ModbusChannel4: m.modbus_chn_4 ?? null,
    ModbusChannel5: m.modbus_chn_5 ?? null,
    ModbusChannel6: m.modbus_chn_6 ?? null,
    Battery: m.battery ?? null,
  }));
}

export {
  labelMapUC502Wet150,
  labelMapEm300TH,
  labelMapEm300DI,
  labelMapUC502Modbus,
  labelMapsByModelNumber,
  MULTI_SENSOR_CHILD_DELIM,
  MULTI_SENSOR_MODEL,
  isMultiSensorChildDevEui,
  resolveBaseDevEui,
};
