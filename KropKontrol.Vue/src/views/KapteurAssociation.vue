<template>
  <DefaultLayout
    :devices="virtualDevices"
    :selectedDevice="selectedDevice"
    :hideSidebar="false"
    @logout="logout"
    showBack
    @back="goToUsersPage"
    @select-device="onSelectDevice"
  >
    <template #title>Gestion des groupes de Kapteurs</template>
    <div class="container py-4">
      <div class="row g-4">
        <div class="col-md-4">
          <h2 class="text-center">Créer un groupe</h2>
          <p>Gérez ici l'association des Kapteurs à différents groupes.</p>

          <form class="mb-4" @submit.prevent="createGroup">
            <div class="mb-3">
              <label class="form-label">Nom du groupe</label>
              <input v-model="groupName" class="form-control" required />
            </div>
            <div class="mb-3">
              <label class="form-label">Capteurs disponibles</label>
              <select
                v-model="selectedDevEuis"
                multiple
                class="form-select"
                size="5"
              >
                <option
                  v-for="d in fetchedDevices"
                  :key="d.devEui"
                  :value="d.devEui"
                >
                  {{ d.description || d.deviceName }}
                </option>
              </select>
            </div>
            <button class="btn btn-primary" type="submit">
              Ajouter le groupe
            </button>
          </form>
        </div>

        <div class="col-md-4">
          <h2 class="text-center">Groupes existants</h2>
          <div v-if="groups.length">
            <ul class="list-group">
              <li
                v-for="g in groups"
                :key="g.id"
                class="list-group-item d-flex justify-content-between align-items-center"
              >
                <span>{{ g.name }}</span>
                <button
                  class="btn btn-sm btn-outline-danger"
                  @click="deleteGroup(g.id)"
                >
                  Supprimer
                </button>
              </li>
            </ul>
          </div>
          <p v-else class="text-muted mt-3">Aucun groupe virtuel enregistré.</p>
        </div>

        <div class="col-md-4" v-if="selectedGroup">
          <h2 class="text-center">Détails du groupe</h2>

          <h5 class="mb-3 text-center">{{ selectedGroup.name }}</h5>
          <p class="mb-1">Kapteurs associés :</p>
          <ul class="list-group">
            <li
              v-for="device in selectedGroupDevices"
              :key="device.devEui"
              class="list-group-item"
            >
              {{ device.description || device.deviceName || device.devEui }}
            </li>
          </ul>
        </div>

        <div class="col-md-4" v-else>
          <h2>Détails du groupe</h2>
          <p class="text-muted">
            Sélectionnez un groupe virtuel dans la barre latérale.
          </p>
        </div>
      </div>
    </div>
  </DefaultLayout>
</template>

<script setup>
import { ref, computed, onMounted, inject } from "vue";
import { useRouter } from "vue-router";
import DefaultLayout from "@/views/layout/DefaultLayout.vue";
import { useAuth } from "@/composables/useAuth.js";
import { useDevices } from "@/composables/useDevices.js";
import { refreshTokenIfNeeded } from "@/services/authService.js";

const router = useRouter();
const { logout } = useAuth();
const { groups, addGroup, removeGroup, virtualDevices } =
  inject("virtualDevices");

const { devices: fetchedDevices, loadDevices } = useDevices();

const groupName = ref("");
const selectedDevEuis = ref([]);

const selectedDevice = ref(null);
const selectedGroup = ref(null);
const selectedGroupId = computed(() => selectedGroup.value?.id);
const selectedGroupDevices = computed(() =>
  (selectedGroup.value?.devEuis || [])
    .map((devEui) => fetchedDevices.value.find((d) => d.devEui === devEui))
    .filter(Boolean),
);
function onSelectDevice(dev) {
  // L'objet complet est attendu par DefaultLayout
  selectedDevice.value = dev || null;
  selectedGroup.value = dev?.group || null;
}

function createGroup() {
  if (!groupName.value || !selectedDevEuis.value.length) return;
  const deviceModels = {};
  selectedDevEuis.value.forEach((devEui) => {
    const dev = fetchedDevices.value.find((d) => d.devEui === devEui);
    if (dev) deviceModels[devEui] = dev.model;
  });
  addGroup({
    name: groupName.value,
    devEuis: [...selectedDevEuis.value],
    deviceModels,
  });
  groupName.value = "";
  selectedDevEuis.value = [];
}

onMounted(async () => {
  try {
    await refreshTokenIfNeeded();
    if (!Array.isArray(fetchedDevices.value) || fetchedDevices.value.length === 0) {
      await loadDevices();
    }
  } catch (e) {
    console.error(e);
  }
});

function deleteGroup(id) {
  if (confirm("Supprimer ce groupe ?")) removeGroup(id);
}

function goToUsersPage() {
  router.push({ name: "Users" });
}
</script>

<style scoped>
/* Add styles for Kapteur association page if needed */
</style>
