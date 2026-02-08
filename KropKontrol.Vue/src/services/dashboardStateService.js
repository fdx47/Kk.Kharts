import { API_BASE } from "@/config/constants.js";
import {
  LS_COORDS,
  LS_DASHBOARD_STATE_PREFIX,
  LS_TIMEZONE,
} from "@/config/storageKeys.js";
import { fetchWithAuth, getUserIdFromToken } from "./authService.js";

const DASHBOARD_STATE_ENDPOINT = `${API_BASE}/dashboards/state`;
const DASHBOARD_DEBUG_DEFAULT =
  (typeof import.meta !== "undefined" &&
    import.meta.env &&
    Boolean(import.meta.env.DEV)) ||
  false;
let lastDashboardSyncSignature = null;
let hydrationInProgress = false;
let lastHydratedDashboardState = null;

function isDashboardDebugEnabled() {
  if (typeof window !== "undefined") {
    if (window.__KK_DEBUG_DASHBOARD_STATE__ === false) return false;
    if (window.__KK_DEBUG_DASHBOARD_STATE__ === true) return true;
  }
  return DASHBOARD_DEBUG_DEFAULT;
}

function logDashboardDebug(message, ...args) {
  if (!isDashboardDebugEnabled()) return;
  // eslint-disable-next-line no-console
  console.info(`[dashboardState] ${message}`, ...args);
}

function safeClone(value, fallback = []) {
  try {
    return JSON.parse(JSON.stringify(value ?? fallback));
  } catch {
    if (Array.isArray(value)) return value;
    return fallback;
  }
}

function readJsonFromLocalStorage(key) {
  if (typeof window === "undefined") return null;
  try {
    const raw = localStorage.getItem(key);
    if (!raw) return null;
    return JSON.parse(raw);
  } catch {
    return null;
  }
}

function readStringFromLocalStorage(key) {
  if (typeof window === "undefined") return null;
  try {
    const value = localStorage.getItem(key);
    return value ?? null;
  } catch {
    return null;
  }
}

function readStoredDashboardState() {
  if (typeof window === "undefined") return [];
  const storageKey =
    LS_DASHBOARD_STATE_PREFIX + (getUserIdFromToken() ?? "guest");
  const saved = readJsonFromLocalStorage(storageKey);
  return Array.isArray(saved) ? saved : [];
}

function buildDashboardStatePayloadInternal({ charts } = {}) {
  const dashboardState =
    charts !== undefined
      ? safeClone(charts, [])
      : safeClone(readStoredDashboardState(), []);
  const coords = readJsonFromLocalStorage(LS_COORDS);
  const timezone = readStringFromLocalStorage(LS_TIMEZONE);
  return {
    dashboardState,
    coords,
    timezone: timezone ?? null,
  };
}

function cloneHydratedState(state) {
  if (!state || typeof state !== "object") return null;
  logDashboardDebug("Cloning hydrated state snapshot", state);
  return {
    dashboardState: safeClone(
      state.dashboardState ?? state.charts ?? [],
      [],
    ),
    coords:
      state.coords && typeof state.coords === "object"
        ? { ...state.coords }
        : state.coords ?? null,
    timezone: state.timezone ?? state.tz ?? null,
  };
}

function buildJsonRequestOptions(method, body) {
  const headers = {
    Accept: "application/json",
  };
  if (body !== undefined) {
    headers["Content-Type"] = "application/json";
  }
  return {
    method,
    headers,
    body: body === undefined ? undefined : JSON.stringify(body),
  };
}

function parseStateJsonCandidate(candidate) {
  if (!candidate || typeof candidate !== "object") return null;
  if (
    Array.isArray(candidate.dashboardState) ||
    candidate.coords ||
    candidate.timezone ||
    candidate.stateJson
  ) {
    let rawState = candidate;
    if (candidate.stateJson !== undefined) {
      const value = candidate.stateJson;
      if (typeof value === "string") {
        try {
          const parsed = JSON.parse(value);
          rawState =
            parsed && typeof parsed === "object" ? { ...parsed } : parsed;
          logDashboardDebug("Parsed stateJson string", {
            id: candidate.id,
            createdAt: candidate.createdAt,
          });
        } catch (err) {
          console.error(
            "Impossible de parser stateJson depuis /dashboards/state:",
            err,
          );
          return null;
        }
      } else if (value && typeof value === "object") {
        rawState = { ...value };
        logDashboardDebug("Parsed stateJson object", {
          id: candidate.id,
          createdAt: candidate.createdAt,
        });
      } else {
        return null;
      }
    }
    if (
      rawState &&
      typeof rawState === "object" &&
      rawState.stateJson !== undefined
    ) {
      const nestedValue = rawState.stateJson;
      if (typeof nestedValue === "string") {
        try {
          const nestedParsed = JSON.parse(nestedValue);
          if (nestedParsed && typeof nestedParsed === "object") {
            rawState = {
              ...nestedParsed,
              coords: rawState.coords ?? nestedParsed.coords ?? null,
              timezone:
                rawState.timezone ??
                rawState.tz ??
                nestedParsed.timezone ??
                null,
            };
            logDashboardDebug("Unwrapped nested stateJson string", {
              id: candidate.id,
            });
          }
        } catch (err) {
          console.warn(
            "Impossible de parser l'objet stateJson imbriqué:",
            err,
          );
        }
      } else if (
        nestedValue &&
        typeof nestedValue === "object" &&
        !Array.isArray(nestedValue)
      ) {
        rawState = {
          ...nestedValue,
          coords: rawState.coords ?? nestedValue.coords ?? null,
          timezone:
            rawState.timezone ?? rawState.tz ?? nestedValue.timezone ?? null,
        };
        logDashboardDebug("Unwrapped nested stateJson objet", {
          id: candidate.id,
        });
      }
    }
    if (rawState && typeof rawState === "object") {
      delete rawState.stateJson;
      if (rawState.tz && !rawState.timezone) {
        rawState.timezone = rawState.tz;
      }
      delete rawState.tz;
    }
    if (rawState && typeof rawState === "object") {
      return {
        state: rawState,
        createdAt: Date.parse(candidate.createdAt),
        id: candidate.id ?? null,
      };
    }
  }
  return null;
}

function normaliseDashboardStateResponse(data) {
  if (!data) return null;
  const entries = Array.isArray(data) ? data : [data];
  const parsedEntries = entries
    .map((entry) => parseStateJsonCandidate(entry))
    .filter(Boolean);
  if (!parsedEntries.length) return null;
  parsedEntries.sort((a, b) => {
    const aTs = Number.isFinite(a.createdAt) ? a.createdAt : -Infinity;
    const bTs = Number.isFinite(b.createdAt) ? b.createdAt : -Infinity;
    if (aTs === bTs) {
      const aId = typeof a.id === "number" ? a.id : -Infinity;
      const bId = typeof b.id === "number" ? b.id : -Infinity;
      return bId - aId;
    }
    return bTs - aTs;
  });
  const winner = parsedEntries[0];
  logDashboardDebug("Normalised dashboard state response", {
    entryCount: parsedEntries.length,
    selectedId: winner?.id,
    createdAt: winner?.createdAt,
    keys: winner?.state ? Object.keys(winner.state) : [],
  });
  return winner?.state ?? null;
}

async function parseJsonResponse(response) {
  const text = await response.text();
  if (!response.ok) {
    throw new Error(
      `Erreur ${response.status} lors de l'appel a /dashboards/state: ${text}`,
    );
  }
  if (!text) return null;
  try {
    return JSON.parse(text);
  } catch (err) {
    console.error("Impossible d'analyser la reponse JSON:", err);
    return null;
  }
}

export async function fetchDashboardStateFromApi() {
  const response = await fetchWithAuth(
    DASHBOARD_STATE_ENDPOINT,
    buildJsonRequestOptions("GET"),
  );
  const data = await parseJsonResponse(response);
  logDashboardDebug("Raw dashboard state fetched", data);
  const state = normaliseDashboardStateResponse(data);
  if (state) {
    setLastHydratedDashboardState(state);
    logDashboardDebug("Stored last hydrated dashboard state", state);
  } else {
    logDashboardDebug("No dashboard state returned by API");
  }
  return state;
}

export async function pushDashboardStateToApi(payload) {
  const body =
    payload && typeof payload === "object"
      ? {
          stateJson: JSON.stringify(payload),
          coords: payload.coords ?? null,
      timezone: payload.timezone ?? null,
    }
      : payload;
  logDashboardDebug("Sending dashboard state to API", body);
  const response = await fetchWithAuth(
    DASHBOARD_STATE_ENDPOINT,
    buildJsonRequestOptions("POST", body),
  );
  const result = await parseJsonResponse(response);
  logDashboardDebug("API acknowledged dashboard state save", result);
  return result;
}

export function buildDashboardStatePayload(options = {}) {
  return buildDashboardStatePayloadInternal(options);
}

export function getLastDashboardSyncSignature() {
  return lastDashboardSyncSignature;
}

export function setLastDashboardSyncSignature(signature) {
  lastDashboardSyncSignature = signature;
}

export function setDashboardHydrationState(value) {
  hydrationInProgress = Boolean(value);
  logDashboardDebug("Hydration state updated", { hydrationInProgress });
}

export function isDashboardHydrationInProgress() {
  return hydrationInProgress;
}

export function setLastHydratedDashboardState(state) {
  logDashboardDebug("Updating last hydrated dashboard state", state);
  lastHydratedDashboardState = cloneHydratedState(state);
}

export function getLastHydratedDashboardState() {
  return cloneHydratedState(lastHydratedDashboardState);
}
