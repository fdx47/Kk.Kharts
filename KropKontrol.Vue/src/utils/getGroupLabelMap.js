import { labelMapsByModelNumber } from "@/services/apiService.js";
import { useComputedVars } from "../composables/useComputedVars.js";

const SEP = "|";

const SUBSTRATE_FIELD_MAP = {
  Minerale: { vwc: "mineralVWC", ec: "mineralECp" },
  Organique: { vwc: "organicVWC", ec: "organicECp" },
  "Melange de Tourbe": { vwc: "peatMixVWC", ec: "peatMixECp" },
  "Fibre de Coco": { vwc: "coirVWC", ec: "coirECp" },
  "Laine Minerale": { vwc: "minWoolVWC", ec: "minWoolECp" },
  Perlite: { vwc: "perliteVWC", ec: "perliteECp" },
};

function getDeviceByDevEui(devices = [], devEui) {
  return Array.isArray(devices)
    ? devices.find((d) => d.devEui === devEui)
    : undefined;
}

function resolveDeviceModel(group, devices, devEui) {
  if (group?.deviceModels && group.deviceModels[devEui] != null) {
    const parsed = Number(group.deviceModels[devEui]);
    return Number.isNaN(parsed) ? undefined : parsed;
  }
  const device = getDeviceByDevEui(devices, devEui);
  if (!device) return undefined;
  const value = Number(device.model ?? device.deviceId);
  return Number.isNaN(value) ? undefined : value;
}

function buildFallbackMap(group, devices) {
  const map = {};
  (group?.devEuis || []).forEach((devEui) => {
    const model = resolveDeviceModel(group, devices, devEui);
    if (model == null) return;
    const device = getDeviceByDevEui(devices, devEui);
    const baseMap = labelMapsByModelNumber[model] || {};
    const prefix =
      device?.description || device?.deviceName || device?.name || devEui;
    Object.keys(baseMap).forEach((key) => {
      map[`${key}${SEP}${devEui}`] = `${prefix} - ${baseMap[key]}`;
    });
    const compVars = useComputedVars(model) || {};
    Object.keys(compVars).forEach((key) => {
      map[`${key}${SEP}${devEui}`] = `${prefix} - ${compVars[key].label}`;
    });
    if (Number(model) === 7) {
      const rainKey = `${"volumeDelta_mm"}${SEP}${devEui}`;
      if (!map[rainKey]) {
        map[rainKey] = "Pluviométrie instantanée (mm)";
      }
    }
  });
  return map;
}

function buildSubstrateConfig(group, devices) {
  const parcel = group?.metadata?.parcel;
  if (!parcel?.substrateType) return null;
  const mapping = SUBSTRATE_FIELD_MAP[parcel.substrateType];
  if (!mapping) return null;

  const labelMap = {};
  const defaults = [];

  (group?.devEuis || []).forEach((devEui) => {
    const model = resolveDeviceModel(group, devices, devEui);
    if (model == null) return;
    const baseMap = labelMapsByModelNumber[model] || {};
    if (mapping.vwc && baseMap[mapping.vwc]) {
      const key = `${mapping.vwc}${SEP}${devEui}`;
      labelMap[key] = baseMap[mapping.vwc];
      defaults.push(key);
    }
    if (mapping.ec && baseMap[mapping.ec]) {
      const key = `${mapping.ec}${SEP}${devEui}`;
      labelMap[key] = baseMap[mapping.ec];
      defaults.push(key);
    }
  });

  if (!Object.keys(labelMap).length) return null;

  const uniqueDefaults = Array.from(new Set(defaults));
  return { labelMap, defaultVariables: uniqueDefaults };
}

export function getGroupLabelMap(group, devices = []) {
  if (!group) return {};
  const fallback = buildFallbackMap(group, devices);
  const substrateConfig = buildSubstrateConfig(group, devices);
  if (substrateConfig) {
    return { ...fallback, ...substrateConfig.labelMap };
  }
  return fallback;
}

export function getVirtualDefaultVariables(group, devices = []) {
  if (!group) return [];
  const fallback = buildFallbackMap(group, devices);
  const substrateConfig = buildSubstrateConfig(group, devices);
  const labelMap = substrateConfig
    ? { ...fallback, ...substrateConfig.labelMap }
    : fallback;

  const defaults = new Set(
    substrateConfig ? substrateConfig.defaultVariables : [],
  );

  const addIfAvailable = (field, devEui, label) => {
    const key = `${field}${SEP}${devEui}`;
    if (label && !labelMap[key]) {
      labelMap[key] = label;
    }
    if (labelMap[key]) {
      defaults.add(key);
    }
  };

  (group?.devEuis || []).forEach((devEui) => {
    const modelNumber = Number(resolveDeviceModel(group, devices, devEui));
    if (Number.isNaN(modelNumber)) return;
    if (modelNumber === 2 || modelNumber === 7) {
      addIfAvailable("temperature", devEui);
      addIfAvailable("humidity", devEui);
    }
    if (modelNumber === 7) {
      addIfAvailable(
        "volumeDelta_mm",
        devEui,
        "Pluviométrie instantanée (mm)",
      );
    }
  });

  return Array.from(defaults);
}

export function getSubstrateFieldMapping(type) {
  return SUBSTRATE_FIELD_MAP[type] || null;
}

export const GROUP_VAR_SEPARATOR = SEP;

export default getGroupLabelMap;
