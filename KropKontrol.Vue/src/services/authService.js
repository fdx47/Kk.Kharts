// This script provides authentication-related utility functions for managing JWT tokens and refresh tokens in localStorage..
// It includes functions to store, retrieve, clear, and refresh tokens, as well as decode JWTs and extract user information.

import { jwtDecode } from "jwt-decode";
import { logAuthDebug } from "./authDebugReport.js";
import {
  LS_AUTH_TOKEN,
  LS_REFRESH_TOKEN,
  LS_REFRESH_EXPIRY,
  LS_DEVICE_THRESHOLDS,
  LS_VIRTUAL_GROUPS_PREFIX,
  LS_DASHBOARD_STATE_PREFIX,
  LS_SPECIFIC_CHARTS_PREFIX,
  LS_CUSTOM_SCALES_PREFIX,
} from "../config/storageKeys.js";

const API_BASE = "https://kropkontrol.premiumasp.net/api/v1";
// Promise tracking an ongoing refresh operation so calls can share it
let refreshPromise = null;
// Inter-tab coordination (BroadcastChannel + localStorage lock)
const REFRESH_LOCK_KEY = "kk_refresh_lock";
let refreshBC = null;
try {
  refreshBC = new BroadcastChannel("kk-auth-refresh");
} catch {}
let interTabWaitPromise = null;

function signalRefreshStart() {
  try {
    localStorage.setItem(REFRESH_LOCK_KEY, String(Date.now()));
  } catch {}
  try {
    refreshBC?.postMessage({ type: "refresh_started" });
  } catch {}
}
function signalRefreshDone() {
  try {
    localStorage.removeItem(REFRESH_LOCK_KEY);
  } catch {}
  try {
    refreshBC?.postMessage({ type: "refresh_done" });
  } catch {}
}
function waitForInterTabRefresh(ttlMs = 12000) {
  if (!interTabWaitPromise) {
    interTabWaitPromise = new Promise((resolve) => {
      const timeout = setTimeout(() => cleanup(), ttlMs);
      const onAuthChanged = () => cleanup();
      const onMsg = (ev) => {
        const t = ev?.data?.type;
        if (t === "refresh_done") cleanup();
      };
      function cleanup() {
        try {
          clearTimeout(timeout);
        } catch {}
        try {
          window.removeEventListener("kk-auth-changed", onAuthChanged);
        } catch {}
        try {
          refreshBC?.removeEventListener("message", onMsg);
        } catch {}
        resolve();
      }
      try {
        window.addEventListener("kk-auth-changed", onAuthChanged);
      } catch {}
      try {
        refreshBC?.addEventListener("message", onMsg);
      } catch {}
    }).finally(() => {
      interTabWaitPromise = null;
    });
  }
  return interTabWaitPromise;
}

export function getStoredToken() {
  return localStorage.getItem(LS_AUTH_TOKEN);
}

export function getStoredRefreshToken() {
  return localStorage.getItem(LS_REFRESH_TOKEN);
}

export function getRefreshTokenExpiry() {
  const v = localStorage.getItem(LS_REFRESH_EXPIRY);
  if (!v) return undefined;
  if (typeof v === "string" && v.startsWith("0001-01-01")) return undefined; // .NET MinValue sentinel
  const t = Date.parse(v);
  if (Number.isNaN(t)) return undefined;
  return v;
}

export function storeAuthData(token, refreshToken, expiry) {
  const currentRT = localStorage.getItem(LS_REFRESH_TOKEN);
  const currentExp = localStorage.getItem(LS_REFRESH_EXPIRY);

  const expValid = (v) => {
    if (!v) return false;
    const t = new Date(v).getTime();
    if (Number.isNaN(t)) return false;
    if (typeof v === "string" && v.startsWith("0001-01-01")) return false; // .NET MinValue sentinel
    return true;
  };

  // Always update access token if provided
  if (token) {
    localStorage.setItem(LS_AUTH_TOKEN, token);
    try {
      window.dispatchEvent(new CustomEvent("kk-auth-changed"));
    } catch {}
  }

  // Update refresh token if explicitly provided (even if expiry unknown)
  if (typeof refreshToken !== "undefined") {
    localStorage.setItem(LS_REFRESH_TOKEN, refreshToken);
  }

  // Handle expiry: accept new value only if valid; otherwise preserve existing valid value.
  let finalExp = currentExp;
  if (expValid(expiry)) {
    finalExp = expiry;
  }

  if (expValid(finalExp)) {
    localStorage.setItem(LS_REFRESH_EXPIRY, finalExp);
  } else {
    // If no valid expiry is known, remove the key so logic treats it as unknown
    localStorage.removeItem(LS_REFRESH_EXPIRY);
    finalExp = null;
  }

  logAuthDebug({ action: "storeAuthData", tokenExpires: finalExp });
}
export function clearAuthData() {
  localStorage.removeItem(LS_AUTH_TOKEN);
  localStorage.removeItem(LS_REFRESH_TOKEN);
  localStorage.removeItem(LS_REFRESH_EXPIRY);
  logAuthDebug({ action: "clearAuthData" });
}

export function clearUserCache(userId = getUserIdFromToken()) {
  [LS_DEVICE_THRESHOLDS].forEach((k) => localStorage.removeItem(k));
  if (userId) {
    localStorage.removeItem(`${LS_VIRTUAL_GROUPS_PREFIX}${userId}`);
    localStorage.removeItem(`${LS_DASHBOARD_STATE_PREFIX}${userId}`);
    localStorage.removeItem(`${LS_SPECIFIC_CHARTS_PREFIX}${userId}`);
  }
  Object.keys(localStorage).forEach((k) => {
    if (k.startsWith(LS_CUSTOM_SCALES_PREFIX)) localStorage.removeItem(k);
  });
  indexedDB.deleteDatabase("sensor-cache");
}

export function filterChartsByAccess(devEuis) {
  const userId = getUserIdFromToken() ?? "guest";
  let allowedGroups = [];
  try {
    const raw = localStorage.getItem(`${LS_VIRTUAL_GROUPS_PREFIX}${userId}`);
    const groups = JSON.parse(raw);
    if (Array.isArray(groups)) {
      allowedGroups = groups.map((g) => `group-${g.id}`);
    }
  } catch {}
  const allowed = new Set([...devEuis, ...allowedGroups]);
  [
    LS_DASHBOARD_STATE_PREFIX.slice(0, -1),
    LS_SPECIFIC_CHARTS_PREFIX.slice(0, -1),
  ].forEach((prefix) => {
    const key = `${prefix}_${userId}`;
    try {
      const raw = localStorage.getItem(key);
      if (!raw) return;
      const charts = JSON.parse(raw);
      if (!Array.isArray(charts)) return;

      const filtered = charts
        .map((chart) => {
          if (
            prefix === "specificChartsConfig" &&
            Array.isArray(chart.series)
          ) {
            const series = chart.series.filter((s) => allowed.has(s.devEui));
            return series.length ? { ...chart, series } : null;
          } else if (allowed.has(chart.devEui)) {
            return chart;
          }
          return null;
        })
        .filter(Boolean);

      localStorage.setItem(key, JSON.stringify(filtered));
    } catch (e) {
      console.warn("Failed to filter charts for", prefix, e);
    }
  });
}

export async function refreshTokenIfNeeded() {
  if (refreshPromise) {
    await refreshPromise;
    return;
  }

  const token = getStoredToken();
  if (!token) return;

  try {
    const payload = jwtDecode(token);
    const expiration = payload.exp * 1000;
    const now = Date.now();

    if (expiration - now < 5 * 60 * 1000) {
      // If another tab is already refreshing, wait for it
      const lockTs = parseInt(
        localStorage.getItem(REFRESH_LOCK_KEY) || "0",
        10,
      );
      if (lockTs && Date.now() - lockTs < 12000) {
        await waitForInterTabRefresh();
        return;
      }

      let rt = getStoredRefreshToken();
      const refreshExpiryStr = getRefreshTokenExpiry();
      const refreshExpiry = refreshExpiryStr
        ? new Date(refreshExpiryStr)
        : null;
      if (!rt || (refreshExpiry && Date.now() > refreshExpiry.getTime()))
        throw new Error("Refresh token expire");

      logAuthDebug({ action: "refresh_start" });

      refreshPromise = (async () => {
        signalRefreshStart();
        try {
          const attempt = async (refreshToken) => {
            const res = await fetch(`${API_BASE}/auth/refresh-token`, {
              method: "POST",
              headers: { "Content-Type": "application/json" },
              body: JSON.stringify({ RefreshToken: refreshToken }),
            });
            let data = null;
            try {
              data = await res.json();
            } catch {}
            return { res, data };
          };

          // First attempt with current refresh token
          let { res, data } = await attempt(rt);
          if (!res.ok || !data?.token) {
            // Retry once if another tab updated tokens in the meantime
            const rt2 = getStoredRefreshToken();
            if (rt2 && rt2 !== rt) {
              ({ res, data } = await attempt(rt2));
            }
          }

          if (!res.ok || !data?.token) {
            const status = res?.status;
            const msg = data?.message || "Refresh failed";
            logAuthDebug({
              action: "refresh_failure",
              error: `${status || "ERR"} ${msg}`,
            });
            // Only clear on explicit 400/401 invalid responses; not on network errors
            if (status === 400 || status === 401) {
              clearAuthData();
            }
            return;
          }

          storeAuthData(
            data.token,
            data.refreshToken,
            data.refreshTokenExpiryTime,
          );
          logAuthDebug({ action: "refresh_success", status: res.status });
          if (import.meta.env.DEV) {
            console.log("Token refreshed");
          }
        } catch (e) {
          console.warn("Refresh token error", e?.message || e);
          logAuthDebug({
            action: "refresh_failure",
            error: String(e?.message || e),
          });
          // Do not clear on network-level errors
        } finally {
          signalRefreshDone();
          refreshPromise = null;
        }
      })();

      await refreshPromise;
    }
  } catch (e) {
    console.warn("Refresh token process error", e?.message || e);
    logAuthDebug({ action: "refresh_failure", error: String(e?.message || e) });
    // Do not clear here; let fetchWithAuth handle explicit 401s
  }
}

export async function fetchWithAuth(url, options = {}) {
  await refreshTokenIfNeeded();
  const token = getStoredToken();
  const headers = { ...options.headers, Authorization: `Bearer ${token}` };
  let response = await fetch(url, { ...options, headers });

  if (response.status === 401) {
    await refreshTokenIfNeeded();
    const retryToken = getStoredToken();
    const retryHeaders = {
      ...options.headers,
      Authorization: `Bearer ${retryToken}`,
    };
    response = await fetch(url, { ...options, headers: retryHeaders });

    if (response.status === 401) {
      clearAuthData();
      throw new Error("Unauthorized");
    }
  }

  return response;
}
// Check if the current auth token is expired
export function isTokenExpired() {
  const token = getStoredToken();
  if (!token) return true;

  try {
    const payload = jwtDecode(token);
    const expiration = payload.exp * 1000;
    return Date.now() > expiration;
  } catch (e) {
    console.warn("Erreur lors de la vérification du token :", e.message);
    return true;
  }
}

// Decode Token
export function decodeToken() {
  const token = getStoredToken();
  if (!token) return null;

  try {
    const payload = jwtDecode(token);
    return payload;
  } catch (e) {
    console.warn("Erreur lors du décodage du token :", e.message);
    return null;
  }
}

// User ID
export function getUserIdFromToken() {
  const payload = decodeToken();
  return payload ? payload.nameid : null;
}

// Role
export function getUserRoleFromToken() {
  const token = localStorage.getItem(LS_AUTH_TOKEN);
  if (!token) return null;

  try {
    const decoded = jwtDecode(token);
    return decoded.role;
  } catch (error) {
    console.error("Erreur lors du décodage du token :", error);
    return null;
  }
}
