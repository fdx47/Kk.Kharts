<template>
  <div>
    <DefaultLayout
      v-if="!loading"
      sidebar-title="Kompagnies"
      :devices="sidebarCompanies"
      :selectedDevice="selectedCompany"
      @select-device="selectCompany"
      :showAddButton="false"
      :sidebar-default-open="false"
      :hide-sidebar="!hasMultipleCompanies"
      sidebar-handle-text="Kompagnies"
      sidebar-handle-top="50%"
      showBack
      back-icon="bi bi-gear"
      back-title="Menu avancé"
      @logout="logout"
      @back="goLanding"
    >
      <template #title>Snapkharts</template>
      <div
        v-if="snapshots.length === 0"
        class="text-center text-muted mt-5 flex-grow-1 d-flex align-items-center justify-content-center"
      >
        <div>
          <div style="font-size: 3rem">
            Aucun snapshot disponible.<br />
            Créez des graphiques dans la page
            <router-link to="/dashboard">Dashboard</router-link>
            pour qu'ils s'affichent ici lors de votre prochaine connexion.
          </div>
        </div>
      </div>
      <div
        v-else
        v-for="(group, idx) in visibleGroups"
        :key="group.key"
        :class="[
          'mb-4',
          { 'pb-4 mb-5 border-bottom': idx < visibleGroups.length - 1 },
        ]"
      >
        <TileLayout class="py-4">
          <template #before-row>
            <h5 v-if="visibleGroups.length > 1" class="mb-3">
              {{ group.name }} ({{ group.items.length }} Kapteur{{
                group.items.length > 1 ? "s" : ""
              }}
              )
            </h5>
          </template>
          <div
            v-for="snap in group.items"
            :key="snap.devEui"
            class="col-12 col-sm-6 col-md-4"
          >
            <div
              class="card tile snapshot-tile"
              @click="openDashboard(snap.devEui)"
            >
              <div class="card-body p-2">
                <LazySnapshotCard
                  :devEui="snap.devEui"
                  :title="snap.title"
                  :labelMap="snap.labelMap"
                  :variables="snap.variables"
                  :device="devices.find((d) => d.devEui === snap.devEui)"
                  :interval-days="snap.intervalDays"
                  :start-date="snap.startDate"
                  :end-date="snap.endDate"
                  :start-date-time="snap.startDateTime"
                  :end-date-time="snap.endDateTime"
                />
              </div>
            </div>
          </div>
        </TileLayout>
      </div>
    </DefaultLayout>
    <div
      v-else
      class="d-flex justify-content-center align-items-center py-5 text-muted"
    >
      Chargement...
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, ref, inject } from "vue";
import { useRouter } from "vue-router";
import { useNavigation } from "../composables/useNavigation.js";
import TileLayout from "@/components/TileLayout.vue";
import DefaultLayout from "@/views/layout/DefaultLayout.vue";
import LazySnapshotCard from "@/components/LazySnapshotCard.vue";
import useLocalStorage from "@/composables/useLocalStorage.js";
import { getCompanies } from "@/services/apiService.js";
import { useDevices } from "@/composables/useDevices.js";
import { useAuth } from "@/composables/useAuth.js";
import { getUserIdFromToken } from "@/services/authService.js";
import {
  LS_COORDS,
  LS_DASHBOARD_STATE_PREFIX,
  LS_TIMEZONE,
} from "@/config/storageKeys.js";
import {
  buildDashboardStatePayload,
  fetchDashboardStateFromApi,
  getLastHydratedDashboardState,
  setDashboardHydrationState,
  setLastDashboardSyncSignature,
  setLastHydratedDashboardState,
} from "@/services/dashboardStateService.js";

function isDashboardDebugEnabled() {
  if (typeof window !== "undefined") {
    if (window.__KK_DEBUG_DASHBOARD_STATE__ === false) return false;
    if (window.__KK_DEBUG_DASHBOARD_STATE__ === true) return true;
  }
  return Boolean(import.meta.env?.DEV);
}

function logDashboardDebug(message, ...args) {
  if (!isDashboardDebugEnabled()) return;
  // eslint-disable-next-line no-console
  console.info(`[SnapshotPage] ${message}`, ...args);
}

const router = useRouter();
const { logout } = useAuth();
const { goLanding } = useNavigation();

const STORAGE_KEY =
  LS_DASHBOARD_STATE_PREFIX + (getUserIdFromToken() ?? "guest");
const charts = useLocalStorage(STORAGE_KEY, []);
const cachedDashboardState = getLastHydratedDashboardState();
if (
  cachedDashboardState &&
  Array.isArray(cachedDashboardState.dashboardState) &&
  cachedDashboardState.dashboardState.length &&
  charts.value.length === 0
) {
  charts.value = cachedDashboardState.dashboardState;
  logDashboardDebug("Applied cached dashboard state to snapshots", {
    chartCount: charts.value.length,
  });
}
const { devices: fetchedDevices, loadDevices } = useDevices();
// Liste des compagnies récupérées depuis l'API
const companies = ref([]);
// Injection robuste des groupes virtuels (fallback liste vide)
const injectedVirtual = inject("virtualDevices", { virtualDevices: ref([]) });
const virtualDevices = injectedVirtual?.virtualDevices ?? ref([]);
const devices = computed(() => [
  ...(fetchedDevices.value ?? []),
  ...(virtualDevices.value ?? []),
]);
const loading = ref(true);

const selectedCompanyId = ref("");

const snapshots = computed(() => {
  const map = new Map();
  charts.value.forEach((c) => {
    // Ignorer les snapshots dont le device n'existe plus
    if (!devices.value.some((d) => d.devEui === c.devEui)) return;
    if (!map.has(c.devEui)) {
      map.set(c.devEui, {
        devEui: c.devEui,
        title: c.title,
        labelMap: c.labelMap,
        variables: c.variables,
        // Conserver une fenàªtre utilisateur si dispo (meilleure compatibilité avec les capteurs virtuels)
        intervalDays: 1.5,
        startDate: '',
        endDate: '',
        startDateTime: null,
        endDateTime: null,
      });
    }
  });
  return Array.from(map.values());
});

const snapshotsByCompany = computed(() => {
  const groups = {};
  // Table de correspondance id -> compagnie pour retrouver le nom
  const companiesMap = new Map(
    companies.value.flatMap((c) => [
      [String(c.id ?? c.companyId ?? c.key), c],
      [c.name, c],
    ]),
  );
  snapshots.value.forEach((s) => {
    const dev = devices.value.find((d) => d.devEui === s.devEui);
    const companyId = dev?.companyId ?? dev?.idCompany;
    const key = companyId || dev?.companyName || "none";
    const company =
      companiesMap.get(String(companyId)) || companiesMap.get(dev?.companyName);
    const name = company?.name || dev?.companyName || "Capteurs virtuels";
    if (!groups[key]) {
      groups[key] = {
        key,
        name,
        items: [],
      };
    }
    groups[key].items.push(s);
  });
  const result = Object.values(groups);
  result.forEach((g) =>
    g.items.sort((a, b) => a.title?.localeCompare(b.title ?? "") ?? 0),
  );
  return result.sort((a, b) => a.name.localeCompare(b.name));
});

const sidebarCompanies = computed(() =>
  snapshotsByCompany.value.map((g) => ({ devEui: g.key, description: g.name })),
);

const selectedCompany = computed(
  () =>
    sidebarCompanies.value.find((c) => c.devEui === selectedCompanyId.value) ||
    null,
);

const hasMultipleCompanies = computed(() => sidebarCompanies.value.length > 1);

const visibleGroups = computed(() => {
  if (!selectedCompanyId.value) return snapshotsByCompany.value;
  return snapshotsByCompany.value.filter(
    (g) => g.key === selectedCompanyId.value,
  );
});

onMounted(async () => {
  if (
    !Array.isArray(fetchedDevices.value) ||
    fetchedDevices.value.length === 0
  ) {
    await loadDevices().catch((err) => {
      console.error("Impossible de charger les devices pour SnapshotPage:", err);
      return [];
    });
  }
  const apiCompanies = await getCompanies().catch(console.error);
  companies.value = apiCompanies || [];
  if (import.meta.env.DEV) {
    console.debug("Companies:", companies.value);
    console.debug("Devices:", fetchedDevices.value);
  }
  await hydrateSnapshotsIfNeeded();
});

function openDashboard(devEui) {
  router.push({ name: "Dashboard", query: { devEui } });
}

function selectCompany(company) {
  selectedCompanyId.value = company.devEui;
}

async function hydrateSnapshotsIfNeeded() {
  if (Array.isArray(charts.value) && charts.value.length > 0) {
    logDashboardDebug("Local snapshots already present", {
      count: charts.value.length,
    });
    loading.value = false;
    return;
  }

  setDashboardHydrationState(true);
  try {
    const state = await fetchDashboardStateFromApi();
    if (!state) return;

    const remoteCharts = Array.isArray(state.dashboardState)
      ? state.dashboardState
      : Array.isArray(state.charts)
        ? state.charts
        : null;

    if (remoteCharts && remoteCharts.length) {
      charts.value = remoteCharts;
      logDashboardDebug("Snapshots hydrated from API", {
        count: remoteCharts.length,
      });
      setLastHydratedDashboardState({
        dashboardState: remoteCharts,
        coords: state.coords ?? null,
        timezone: state.timezone ?? state.tz ?? null,
      });
      setLastDashboardSyncSignature(
        JSON.stringify(buildDashboardStatePayload({ charts: remoteCharts })),
      );
    }

    if (state.coords) {
      try {
        localStorage.setItem(LS_COORDS, JSON.stringify(state.coords));
      } catch {}
    }
    const tz = state.timezone ?? state.tz ?? null;
    if (tz) {
      try {
        localStorage.setItem(LS_TIMEZONE, tz);
      } catch {}
    }
  } catch (err) {
    console.error("Impossible de charger les snapshots distants:", err);
    logDashboardDebug("Snapshot hydration failed", err);
  } finally {
    setDashboardHydrationState(false);
    loading.value = false;
  }
}
</script>

<style scoped>
.snapshot-tile {
  cursor: pointer;
}
</style>
