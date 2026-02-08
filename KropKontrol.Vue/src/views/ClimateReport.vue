<template>
  <DefaultLayout
    :devices="devices"
    :selectedDevice="selectedDevice"
    showBack
    @select-device="onSelectDevice"
    @back="goLanding"
    @logout="logout"
  >
    <template #title>Rapport Klimat</template>

    <ClimateReportPanel :devEui="devEui" />
  </DefaultLayout>
</template>

<script setup>
import { ref, computed, onMounted } from "vue";
import DefaultLayout from "@/views/layout/DefaultLayout.vue";
import { useNavigation } from "../composables/useNavigation.js";
import { useAuth } from "../composables/useAuth.js";
import { useDevices } from "@/composables/useDevices.js";
import { useVirtualDevices } from "../composables/useVirtualDevices.js";
import ClimateReportPanel from "@/components/ClimateReportPanel.vue";

const { goLanding } = useNavigation();
const { logout } = useAuth();

const { devices: fetchedDevices, loadDevices } = useDevices();
const { virtualDevices } = useVirtualDevices();
const devices = computed(() => [
  ...fetchedDevices.value,
  ...virtualDevices.value,
]);

const devEui = ref("");
const selectedDevice = computed(
  () => devices.value.find((d) => d.devEui === devEui.value) || null,
);

function onSelectDevice(dev) {
  devEui.value = dev.devEui;
}

onMounted(async () => {
  if (!Array.isArray(fetchedDevices.value) || fetchedDevices.value.length === 0) {
    await loadDevices().catch(console.error);
  }
});
</script>

<style scoped>
.report-container {
  max-width: 900px;
  margin: 0 auto;
  padding-bottom: 2rem;
}
.placeholder-message {
  min-height: 30vh;
  color: #82be20ff;
  font-size: 1.1rem;
}
.card-header {
  background-color: #82be20ff;
  color: #fff;
}
.spinner-border {
  color: #82be20ff;
}
</style>
