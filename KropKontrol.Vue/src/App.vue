<template>
  <router-view />

  <!-- Force refresh button: visible only for Root -->
  <div v-if="canForceRefresh" class="kk-dev-tools">
    <button class="kk-dev-btn" @click="forceRefresh" :disabled="forcing">
      {{ forcing ? "Refresh…" : "Forcer refresh" }}
    </button>
  </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount } from "vue";
import { refreshTokenIfNeeded } from "./services/authService.js";
import { isRootUser } from "./services/roleUtils.js";
import { printAuthDebugLog } from "./services/authDebugReport.js";

const canForceRefresh = ref(isRootUser());
function updateCanForce() {
  canForceRefresh.value = isRootUser();
}
onMounted(() => {
  window.addEventListener("kk-auth-changed", updateCanForce);
});
onBeforeUnmount(() => {
  window.removeEventListener("kk-auth-changed", updateCanForce);
});
const forcing = ref(false);

async function forceRefresh() {
  if (!isRootUser()) return; // extra guard
  if (forcing.value) return;
  forcing.value = true;
  try {
    console.log("[Dev] Forcer refresh: start");
    await refreshTokenIfNeeded();
    console.log("[Dev] Forcer refresh: done");
  } catch (e) {
    console.warn("[Dev] Forcer refresh error:", e?.message || e);
  } finally {
    try {
      printAuthDebugLog();
    } catch {}
    forcing.value = false;
  }
}

// Expose helpers only in dev and for allowed role
if (import.meta.env.DEV && isRootUser()) {
  // @ts-ignore
  window.forceKKRefresh = forceRefresh;
  // @ts-ignore
  window.refreshTokenIfNeeded = refreshTokenIfNeeded;
  // @ts-ignore
  window.printAuthDebugLog = printAuthDebugLog;
}
</script>

<style>
body {
  background-color: #f8f9fa;
  font-family: system-ui, sans-serif;
  margin: 0;
}

.kk-dev-tools {
  position: fixed;
  right: 12px;
  bottom: 12px;
  z-index: 9999;
}

.kk-dev-btn {
  background: #0d6efd;
  color: #fff;
  border: none;
  padding: 8px 12px;
  border-radius: 6px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.15);
  cursor: pointer;
}

.kk-dev-btn[disabled] {
  opacity: 0.7;
  cursor: default;
}
</style>
