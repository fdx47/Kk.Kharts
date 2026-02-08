import {
  MODELS,
  getUc502Wet150DataCached,
  getEm300ThDataCached,
  getEm300DiDataCached,
  getUc502ModbusDataCached,
  getUc502MultiSensorDataCached,
  getCachedSlice,
} from "@/services/dataCacheService.js";
import { isMultiSensorChildDevEui } from "@/services/apiService.js";
import { useVirtualDevices } from "./useVirtualDevices.js";
import { useDevices } from "./useDevices.js";

// Cache partagé pour éviter les requêtes doublons.
const sharedData = new Map();
const groupDataCache = new Map();

function roundToMinute(d) {
  d.setSeconds(0, 0);
  return d.toISOString();
}

/**
 * Unified data fetcher for device measures.
 * Chooses the appropriate cached API based on model number.
 */
export function useDeviceData() {
  const { groups } = useVirtualDevices();
  const { devices, loadDevices } = useDevices();

  async function getAllDevices() {
    await loadDevices().catch((e) => {
      console.error("useDeviceData: cannot fetch devices", e);
    });
    return devices.value;
  }

  async function fetchDeviceData(
    devEui,
    model,
    startISO,
    endISO,
    onlyDevEuis = null,
  ) {
    const startRounded = roundToMinute(new Date(startISO));
    const endRounded = roundToMinute(new Date(endISO));
    const startMs = Date.parse(startRounded);
    const endMs = Date.parse(endRounded);
    const isMultiSensorChild = isMultiSensorChildDevEui(devEui);

    // Virtual device : agrège les données de chaque appareil réel
    if (devEui && devEui.startsWith("group-")) {
      const groupId = devEui.slice("group-".length);
      const cacheKey = `${groupId}|${startRounded}|${endRounded}`;
      if (groupDataCache.has(cacheKey)) {
        return groupDataCache.get(cacheKey);
      }
      const group = groups.value.find((g) => g.id === groupId);
      if (!group) return [];
      const devList =
        onlyDevEuis && onlyDevEuis.length
          ? group.devEuis.filter((e) => onlyDevEuis.includes(e))
          : group.devEuis;
      const segments = await Promise.all(
        devList.map(async (realDevEui) => {
          let m = group.deviceModels
            ? group.deviceModels[realDevEui]
            : undefined;
          if (m == null) {
            const allDevices = await getAllDevices();
            const dev = allDevices.find((d) => d.devEui === realDevEui);
            m = dev?.model;
          }
          const data = await fetchDeviceData(
            realDevEui,
            m,
            startRounded,
            endRounded,
          );
          return data.map((d) => ({ ...d, devEui: realDevEui }));
        }),
      );
      const result = segments
        .flat()
        .sort(
          (a, b) =>
            new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime(),
        );
      groupDataCache.set(cacheKey, result);
      return result;
    }

    const cached = sharedData.get(devEui);
    if (cached) {
      if (cached.promise) {
        try {
          await cached.promise;
        } catch {
          /* ignore */
        }
      }
      if (cached.start <= startMs && cached.end >= endMs) {
        return cached.data.filter((d) => {
          const ts = Date.parse(d.timestamp);
          return ts >= startMs && ts <= endMs;
        });
      }
    }

    const fetchPromise = (async () => {
      let m = Number(model);
      if (!m) {
        const allDevices = await getAllDevices();
        const dev = allDevices.find((d) => d.devEui === devEui);
        m = Number(dev?.model);
      }
      try {
        let res;
        if (isMultiSensorChild)
          res = await getUc502MultiSensorDataCached(
            devEui,
            startRounded,
            endRounded,
          );
        else if (m === 7)
          res = await getEm300ThDataCached(devEui, startRounded, endRounded);
        else if (m === 2)
          res = await getEm300DiDataCached(devEui, startRounded, endRounded);
        else if (m === 61)
          res = await getUc502ModbusDataCached(
            devEui,
            startRounded,
            endRounded,
          );
        else
          res = await getUc502Wet150DataCached(
            devEui,
            startRounded,
            endRounded,
          );
        const existing = sharedData.get(devEui);
        const merged = existing ? mergeAndDedup(existing.data, res) : res;
        const newStart = existing ? Math.min(existing.start, startMs) : startMs;
        const newEnd = existing ? Math.max(existing.end, endMs) : endMs;
        sharedData.set(devEui, { data: merged, start: newStart, end: newEnd });
        return merged;
      } catch (err) {
        console.error("fetchDeviceData error", err);
        sharedData.delete(devEui);
        return [];
      }
    })();

    sharedData.set(devEui, {
      promise: fetchPromise,
      start: startMs,
      end: endMs,
      data: cached?.data || [],
    });

    return await fetchPromise;
  }

  async function getCachedDeviceData(
    devEui,
    model,
    startISO,
    endISO,
    onlyDevEuis = null,
  ) {
    const startRounded = roundToMinute(new Date(startISO));
    const endRounded = roundToMinute(new Date(endISO));

    if (devEui && devEui.startsWith("group-")) {
      const groupId = devEui.slice("group-".length);
      const group = groups.value.find((g) => g.id === groupId);
      if (!group) return [];
      const devList =
        onlyDevEuis && onlyDevEuis.length
          ? group.devEuis.filter((e) => onlyDevEuis.includes(e))
          : group.devEuis;
      const segments = await Promise.all(
        devList.map(async (realDevEui) => {
          let m = group.deviceModels
            ? group.deviceModels[realDevEui]
            : undefined;
          if (m == null) {
            const allDevices = await getAllDevices();
            const dev = allDevices.find((d) => d.devEui === realDevEui);
            m = dev?.model;
          }
          const data = await getCachedDeviceData(
            realDevEui,
            m,
            startRounded,
            endRounded,
          );
          return data.map((d) => ({ ...d, devEui: realDevEui }));
        }),
      );
      return segments
        .flat()
        .sort(
          (a, b) =>
            new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime(),
        );
    }

    let m = Number(model);
    if (!m) {
      const allDevices = await getAllDevices();
      const dev = allDevices.find((d) => d.devEui === devEui);
      m = Number(dev?.model);
    }
    if (isMultiSensorChildDevEui(devEui)) {
      return await getCachedSlice(
        MODELS.UC502_MULTI_WET150,
        devEui,
        startRounded,
        endRounded,
      );
    }
    return await getCachedSlice(m, devEui, startRounded, endRounded);
  }

  function clearGroupCache() {
    groupDataCache.clear();
  }

  return { fetchDeviceData, getCachedDeviceData, clearGroupCache };
}

export default useDeviceData;

function mergeAndDedup(existing, incoming) {
  const map = new Map();
  existing.forEach((d) => map.set(d.timestamp + "|" + (d.devEui || ""), d));
  incoming.forEach((d) => map.set(d.timestamp + "|" + (d.devEui || ""), d));
  return Array.from(map.values()).sort(
    (a, b) => new Date(a.timestamp) - new Date(b.timestamp),
  );
}
