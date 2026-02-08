// src/services/apiClient.js
import { fetchWithAuth } from "./authService.js";
import { getUc502Wet150DataCached } from "./dataCacheService.js";

async function getJson(baseUrl, pathUrl) {
  const res = await fetchWithAuth(`${baseUrl}${pathUrl}`, {
    headers: {
      Accept: "application/json",
    },
  });
  const text = await res.text();
  if (!res.ok) throw new Error(`HTTP ${res.status}: ${text}`);
  return JSON.parse(text);
}

export async function getDeviceLastDays(baseUrl, devEui, days = 3) {
  // Note: baseUrl is kept for backward compatibility but not used when
  // leveraging the local IndexedDB cache via dataCacheService.
  if (import.meta.env.DEV) {
    console.log(`--- getDeviceLastDays (days=${days}) via cache ---`);
  }
  const now = new Date();
  const endDateForAPI = now.toISOString();
  const todayMidnight = new Date(
    now.getFullYear(),
    now.getMonth(),
    now.getDate(),
  );
  let startDateForAPI;

  if (days === 1) {
    startDateForAPI = todayMidnight.toISOString();
  } else {
    const d = new Date(todayMidnight);
    d.setDate(d.getDate() - (days - 1));
    startDateForAPI = d.toISOString();
  }

  // Fetch through IndexedDB-backed cache; fills gaps if needed.
  // Wrap into the same shape as the REST payload expected by callers.
  try {
    const data = await getUc502Wet150DataCached(
      devEui,
      startDateForAPI,
      endDateForAPI,
    );
    return { data };
  } catch (err) {
    if (import.meta.env.DEV) {
      console.warn("Cache fetch failed, falling back to network:", err);
    }
    const qs = new URLSearchParams({
      startDate: startDateForAPI,
      endDate: endDateForAPI,
    }).toString();
    const pathUrl = `api/v1/uc502/${encodeURIComponent(devEui)}/wet150?${qs}`;
    return getJson(baseUrl, pathUrl);
  }
}
