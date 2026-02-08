export const LS_AUTH_DEBUG_LOG = "authDebugLog";

export function logAuthDebug(entry) {
  try {
    const logs = JSON.parse(localStorage.getItem(LS_AUTH_DEBUG_LOG) || "[]");
    logs.push({ time: new Date().toISOString(), ...entry });
    localStorage.setItem(LS_AUTH_DEBUG_LOG, JSON.stringify(logs));
  } catch {
    // localStorage may not be accessible
  }
  if (import.meta.env.DEV) {
    console.log("[AuthDebug]", entry);
  }
}

export function getAuthDebugLog() {
  try {
    return JSON.parse(localStorage.getItem(LS_AUTH_DEBUG_LOG) || "[]");
  } catch {
    return [];
  }
}

export function clearAuthDebugLog() {
  localStorage.removeItem(LS_AUTH_DEBUG_LOG);
}

export function printAuthDebugLog() {
  const logs = getAuthDebugLog();
  if (console.table) {
    console.table(logs);
  } else {
    console.log(logs);
  }
}

export function downloadAuthDebugReport() {
  const logs = getAuthDebugLog();
  const blob = new Blob([JSON.stringify(logs, null, 2)], {
    type: "application/json",
  });
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = "auth-debug-report.json";
  a.click();
  URL.revokeObjectURL(url);
}
